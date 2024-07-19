/*
 * Copyright © 2016-2022 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using QuickJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EliteDangerousCore.EDSM
{
    public static class EDSMJournalSync
    {
        public static void StopSync()
        {
            Exit = true;
            exitevent.Set();
            historyevent.Set();     // also trigger in case we are in thread hold
        }

        public static List<HistoryEntry> GetListToSend(List<HistoryEntry> helist) // get the ones to send - the discard list may be slightly 
        {
            lock (alwaysDiscard)        // use this as a perm proxy to lock discardEvents
            {
                var lastset = helist.FindLast(x => x.EdsmSync == true);
                int nexttosend = (lastset != null) ? (lastset.EntryNumber - 1) + 1 : 0;

                List<HistoryEntry> list = new List<HistoryEntry>();

                bool hasbeta = false;

                for ( int i = nexttosend; i < helist.Count; i++ )
                {
                    HistoryEntry he = helist[i];
                    string eventtype = he.EntryType.ToString();

                    if (he.Commander == null)
                    {
                        continue;
                    }
                    else if (he.Commander.NameIsBeta || he.journalEntry.IsBeta )
                    {
                        he.journalEntry.SetEdsmSync();       // crappy slow but unusual, but lets mark them as sent..
                        hasbeta = true;
                    }
                    else if (!(he.Status.IsInMultiPlayer || discardEvents.Contains(eventtype) || alwaysDiscard.Contains(eventtype)))
                    {
                        list.Add(he);
                    }
                    else
                    {

                    }
                }

                return hasbeta ? new List<HistoryEntry>() : list;
            }
        }

        public static bool OkayToSend(HistoryEntry he)
        {
            lock (alwaysDiscard)        // use this as a perm proxy to lock discardEvents
            {
                string eventtype = he.EntryType.ToString();
                return !(he.Commander.NameIsBeta || he.journalEntry.IsBeta || he.Status.IsInMultiPlayer || discardEvents.Contains(eventtype) || alwaysDiscard.Contains(eventtype));
            }
        }

        // called by onNewEntry
        // Called by Perform, by Sync, by above.

        public static bool SendEDSMEvents(Action<string> log, List<HistoryEntry> helist, string gameversionv, string buildv)
        {
            System.Diagnostics.Debug.WriteLine($"EDSM Send Events {helist.Count()} {String.Join(",", helist.Select(x => x.EntryType))}");

            // pass in he's into queue giving the logger, version and build
            foreach ( var he in helist )
                queue.Enqueue(new HistoryQueueEntry { HistoryEntry = he, Logger = log, gameversion = gameversionv, build = buildv});      

            historyevent.Set();

            // Start the sync thread if it's not already running
            if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
            {
                Exit = false;
                exitevent.Reset();
                ThreadEDSMSync = new System.Threading.Thread(new System.Threading.ThreadStart(SyncThread));
                ThreadEDSMSync.Name = "EDSM Journal Sync";
                ThreadEDSMSync.IsBackground = true;
                ThreadEDSMSync.Start();
            }

            return true;
        }

        private static void SyncThread()
        {
            try
            {
                running = 1;

                UpdateDiscardList();                // make sure the list is up to date

                while (queue.Count != 0)      // while stuff to send
                {
                    HistoryQueueEntry hqe = null;

                    if (queue.TryDequeue(out hqe))        // next history event...
                    {
                        historyevent.Reset();

                        HistoryEntry first = hqe.HistoryEntry;

                        bool firstjournalsourced = first.journalEntry.IsJournalSourced;

                        Action<string> logger = hqe.Logger;
                        string gameversion = hqe.gameversion;       // use first entry gameversion/build, which for a batch, will all be the same
                        string build = hqe.build;

                        List<HistoryEntry> hl = new List<HistoryEntry>() { first };

                        if (holdEvents.Contains(first.EntryType) || (first.EntryType == JournalTypeEnum.Location && first.Status.IsDocked))
                        {
                            System.Diagnostics.Debug.WriteLine("EDSM Holding for another event");

                            if (queue.IsEmpty)
                            {
                                historyevent.WaitOne(20000); // Wait up to 20 seconds for another entry to come through
                            }
                        }

                        // Leave event in queue if commander changes, or journal sourced is different
                        // we don't send multiple commanders, or different sourced journal entries, or different builds together

                        while (hl.Count < maxEventsPerMessage && queue.TryPeek(out hqe) && hqe.HistoryEntry.journalEntry.IsJournalSourced == firstjournalsourced &&
                               hqe.gameversion == gameversion && hqe.build == build) 
                        {
                            HistoryEntry he = hqe.HistoryEntry;

                            if (he == null || he.Commander != first.Commander)
                            {
                                break;
                            }

                            queue.TryDequeue(out hqe);
                            historyevent.Reset();

                            // now we have an updated discard list, 

                            if (hqe.HistoryEntry != null && discardEvents.Contains(hqe.HistoryEntry.EntryType.ToString()))
                            {
                                System.Diagnostics.Debug.WriteLine("EDSM Discarding in sync " + hqe.HistoryEntry.EventSummary);
                                continue;
                            }

                            hl.Add(he);

                            if ((holdEvents.Contains(he.EntryType) || (he.EntryType == JournalTypeEnum.Location && he.Status.IsDocked)) && queue.IsEmpty)
                            {
                                historyevent.WaitOne(20000); // Wait up to 20 seconds for another entry to come through
                            }

                            if (Exit)
                            {
                                return;
                            }
                        }

                        int sendretries = 5;
                        int waittime = 30000;
                        string firstdiscovery = "";

                        while (sendretries > 0 && !SendToEDSM(hl, first.Commander, gameversion, build, out string errmsg, out firstdiscovery))
                        {
                            logger?.Invoke($"Error sending EDSM events {errmsg}");
                            System.Diagnostics.Trace.WriteLine($"Error sending EDSM events {errmsg}");
                            exitevent.WaitOne(waittime); // Wait and retry
                            if (Exit)
                            {
                                return;
                            }
                            sendretries--;
                            waittime *= 2; // Double back-off time, up to 8 minutes between tries or 15.5 minutes total
                        }

                        if (sendretries == 0)
                        {
                            logger?.Invoke("Unable to send events - giving up");
                            System.Diagnostics.Trace.WriteLine("Unable to send events - giving up");
                        }
                        else
                        {
                            SentEvents?.Invoke(hl.Count,firstdiscovery);       // finished sending everything, tell..
                            if ( hl.Count>=5)
                                logger?.Invoke($"Sent {hl.Count} Events to EDSM");
                        }
                    }

                    // Wait at least N between messages
                    exitevent.WaitOne(100);

                    if (Exit)
                        return;

                    if (queue.IsEmpty)
                        historyevent.WaitOne(120000);       // wait for another event keeping the thread open.. Note stop also sets this

                    if (Exit)
                        return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.Message);
            }
            finally
            {
                running = 0;
            }
        }

        // Send to EDSM the records with the game/version build

        static private bool SendToEDSM(List<HistoryEntry> hl, EDCommander cmdr, string gameversion, string build, out string errmsg , out string firstdiscovers )
        {
            EDSMClass edsm = new EDSMClass(cmdr);       // Ensure we use the commanders EDSM credentials.
            errmsg = null;
            firstdiscovers = "";

            List<JObject> entries = new List<JObject>();

            foreach (HistoryEntry he in hl)
            {
                JournalEntry je = he.journalEntry;

                if (je == null)
                {
                    je = JournalEntry.Get(he.Journalid);
                }

                JObject json = je.GetJsonCloned();

                if (json == null)
                {
                    continue;
                }

                json.RemoveWildcard("EDD*");        // remove any EDD specials
                if (je.EventTypeID == JournalTypeEnum.FSDJump && json["FuelUsed"].IsNull())
                    json["_convertedNetlog"] = true;
                if (json["StarPosFromEDSM"].Bool(false)) // Remove star pos from EDSM
                    json.Remove("StarPos");
                json.Remove("StarPosFromEDSM");
                json["_systemName"] = he.System.Name;
                json["_systemCoordinates"] = new JArray(he.System.X, he.System.Y, he.System.Z);
                if (he.System.SystemAddress != null)
                    json["_systemAddress"] = he.System.SystemAddress;
                if (he.Status.IsDocked)
                {
                    json["_stationName"] = he.WhereAmI;
                    if (he.Status.MarketID != null)
                        json["_stationMarketId"] = he.Status.MarketID;
                }
                json["_shipId"] = he.Status.ShipID;
                entries.Add(json);
            }

            // game version on all should be the same as hl[0].

            if (!hl[0].journalEntry.IsJournalSourced)        // override for capi stuff
            { 
                gameversion = "CAPI-journal";
                build = "";
            }

            List<JObject> results = edsm.SendJournalEvents(entries, gameversion, build, out errmsg);
            System.Diagnostics.Debug.WriteLine($"EDSM Result of sending {entries.Count} events : gv {gameversion} bu {build} result {errmsg}");
            //List<JObject> results = new List<JObject>();    for( int i = 0; i < hl.Count; i++ ) results.Add(new JObject { ["msgnum"] = 100, ["systemId"] = 200 }); //debug

            if (results == null)
            {
                return false;
            }
            else
            {
                firstdiscovers = UserDatabase.Instance.DBWrite<string>(cn =>
                {
                    string firsts = "";
                    using (var txn = cn.BeginTransaction())
                    {
                        for (int i = 0; i < hl.Count && i < results.Count; i++)
                        {
                            HistoryEntry he = hl[i];
                            JObject result = results[i];
                            int msgnr = result["msgnum"].Int();
                            int systemId = result["systemId"].Int();

                            if ((msgnr >= 100 && msgnr < 200) || msgnr == 500)
                            {
                                if (he.EntryType == JournalTypeEnum.FSDJump)       // only on FSD, confirmed with Anthor.  25/4/2018
                                {
                                    bool systemCreated = result["systemCreated"].Bool();

                                    if (systemCreated)
                                    {
                                        System.Diagnostics.Debug.WriteLine("** EDSM indicates first entry for " + he.System.Name);
                                        (he.journalEntry as JournalFSDJump).UpdateFirstDiscover(true, cn, txn);
                                        firsts = firsts.AppendPrePad(he.System.Name, ";");
                                    }
                                }

//                                System.Diagnostics.Debug.WriteLine("Setting sync on " + (he.Indexno-1));
                                he.journalEntry.SetEdsmSync(cn, txn);

                                if (msgnr == 500)
                                {
                                    System.Diagnostics.Trace.WriteLine($"EDSM Server error {he.Journalid} '{he.EventSummary}': {msgnr} {result["msg"].Str()}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Trace.WriteLine($"EDSM Reports {he.Journalid} '{he.EventSummary}'': {msgnr} {result["msg"].Str()}");
                            }

                        }

                        txn.Commit();
                        return firsts;
                    }
                });

                return true;
            }
        }

        // the discard list removes the ones to send
        // since the discard list may be the default one above, on first check, it may include some which are on the current discard list. 
        // this is not a problem as it will be removed in the sync process

        public static void UpdateDiscardList()
        {
            if (lastDiscardFetch < DateTime.UtcNow.AddMinutes(-120))     // check if we need a new discard list
            {
                try
                {
                    EDSMClass edsm = new EDSMClass();
                    var discardlist = edsm.GetJournalEventsToDiscard();

                    if (discardlist != null)        // if got one
                    {
                        var newdiscardEvents = new HashSet<string>(discardlist);
                        System.Diagnostics.Debug.WriteLine("EDSM Discard list updated " + string.Join(",", newdiscardEvents));

                        lock (alwaysDiscard)        // use this as a perm proxy to lock discardEvents
                        {
                            discardEvents = newdiscardEvents;
                        }
                    }

                    lastDiscardFetch = DateTime.UtcNow; // try again later
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Unable to retrieve events to be discarded: {ex.ToString()}");
                }
            }
        }

        private static HashSet<string> discardEvents = new HashSet<string>
        {   // From https://github.com/EDSM-NET/Journal-Events/blob/master/Discard.php on 2/23/2022

        //'StartUp', // Give first system response to EDMC
        "ShutDown",
        "EDDItemSet",
        "EDDCommodityPrices",
        "ModuleArrived",
        "ShipArrived",
        "Coriolis",
        "EDShipyard",

        // Extra files (Taking them from EDDN)
        "Market",
        "Shipyard",
        "Outfitting",
        "ModuleInfo",
        "Status",

        // Squadron
        "SquadronCreated",
        "SquadronStartup",
        "DisbandedSquadron",

        "InvitedToSquadron",
        "AppliedToSquadron",
        "JoinedSquadron",
        "LeftSquadron",

        "SharedBookmarkToSquadron",

        // Fleet Carrier
        "CarrierStats",
        "CarrierTradeOrder",
        "CarrierFinance",
        "CarrierBankTransfer",
        "CarrierCrewServices",

        "CarrierJumpRequest",
        "CarrierJumpCancelled",
        "CarrierDepositFuel",
        "CarrierDockingPermission",
        "CarrierModulePack",

        "CarrierBuy",
        "CarrierNameChange",
        "CarrierDecommission",

        // Odyssey
        "BookDropship",
        "CancelDropship",
        "DropshipDeploy",

        // Odyssey - Backed by BackpackChange
        "CollectItems",
        "DropItems",

        "Disembark", 
        "Embark",

        // Load events
        "Fileheader",
        "Commander",
        "NewCommander",
        "ClearSavedGame",
        "Music",
        "Continued",
        "Passengers",

        // Docking events
        "DockingCancelled",
        "DockingDenied",
        "DockingGranted",
        "DockingRequested",
        "DockingTimeout",

        // Fly events
        "StartJump",
        "Touchdown",
        "Liftoff",
        "NavBeaconScan",
        "SupercruiseEntry",
        "SupercruiseExit",
        "NavRoute",

        // We might reconsider this, and see if we can do something about crime report?
        "PVPKill",
        "CrimeVictim",
        "UnderAttack",
        "ShipTargeted",
        "Scanned",
        "DataScanned",
        "DatalinkScan",

        // Engineer
        "EngineerApply",
        "EngineerLegacyConvert",

        // Reward (See RedeemVoucher for credits)
        "FactionKillBond",
        "Bounty",
        "CapShipBond",
        "DatalinkVoucher",

        // Ship events
        "SystemsShutdown",
        "EscapeInterdiction",
        "HeatDamage",
        "HeatWarning",
        "HullDamage",
        "ShieldState",
        "FuelScoop",
        "LaunchDrone",
        "AfmuRepairs",
        "CockpitBreached",
        "ReservoirReplenished",
        "CargoTransfer", 

        "ApproachBody",
        "LeaveBody",
        "DiscoveryScan",
        "MaterialDiscovered",
        "Screenshot",

        // NPC Crew
        "CrewAssign",
        "CrewFire",
        "NpcCrewRank",

        // Shipyard / Outfitting
        "ShipyardNew",
        "StoredModules",
        "MassModuleStore",
        "ModuleStore",
        "ModuleSwap",
        
        // Suit
        "SuitLoadout",
        "SwitchSuitLoadout",
        "CreateSuitLoadout",
        "LoadoutEquipModule",

        // Powerplay
        "PowerplayVote",
        "PowerplayVoucher",

        "ChangeCrewRole",
        "CrewLaunchFighter",
        "CrewMemberJoins",
        "CrewMemberQuits",
        "CrewMemberRoleChange",
        "KickCrewMember",
        "EndCrewSession", // ??

        "LaunchFighter",
        "DockFighter",
        "FighterDestroyed",
        "FighterRebuilt",
        "VehicleSwitch",
        "LaunchSRV",
        "DockSRV",
        "SRVDestroyed",

        "JetConeBoost",
        "JetConeDamage",

        "RebootRepair",
        "RepairDrone",

        // Wings
        "WingAdd",
        "WingInvite",
        "WingJoin",
        "WingLeave",

        // Chat
        "ReceiveText",
        "SendText",

        // End of game
        "Shutdown",

        // Temp Discard...
        "FSSSignalDiscovered",
        "AsteroidCracked",
        "ProspectedAsteroid",
        "ScanBaryCentre",
        "FSSBodySignals",
        "SAASignalsFound",
        "ScanOrganic",
        };

        // 15/3/22 removed all of these - kept list for the future and as it acts as a lock
        // this enables Friends and CommunityGoals to be sent to EDSM.  The other ones ReceiveText, SendText, FuelScoop are in the normal discard list
        // Underattack, FSDTarget are UI events which EDSM does not see.

        private static HashSet<string> alwaysDiscard = new HashSet<string>
        { 
        };

        private static HashSet<JournalTypeEnum> holdEvents = new HashSet<JournalTypeEnum>
        {
            JournalTypeEnum.Cargo,
            JournalTypeEnum.Loadout,
            JournalTypeEnum.Materials,
            JournalTypeEnum.LoadGame,
            JournalTypeEnum.Rank,
            JournalTypeEnum.Progress,
            JournalTypeEnum.ShipyardBuy,
            JournalTypeEnum.ShipyardNew,
            JournalTypeEnum.ShipyardSwap
        };

        private class HistoryQueueEntry
        {
            public Action<string> Logger;
            public HistoryEntry HistoryEntry;
            public string gameversion;
            public string build;
        }

        static public Action<int, string> SentEvents;       // called in thread when sync thread has finished and is terminating. first discovery list

        private static Thread ThreadEDSMSync;
        private static int running = 0;
        private static bool Exit = false;
        private static ConcurrentQueue<HistoryQueueEntry> queue = new ConcurrentQueue<HistoryQueueEntry>();
        private static AutoResetEvent historyevent = new AutoResetEvent(false);
        private static ManualResetEvent exitevent = new ManualResetEvent(false);
        private static DateTime lastDiscardFetch;
        private static int maxEventsPerMessage = 200;
    }
}
