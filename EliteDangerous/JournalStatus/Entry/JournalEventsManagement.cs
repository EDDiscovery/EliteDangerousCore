/*
 * Copyright 2016-2024 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace EliteDangerousCore
{
    public static class JournalEventsManagement
    {
        // events list groups for filtering out events when reading

        static public JournalTypeEnum[] EssentialEvents = new JournalTypeEnum[]      
            {
                // due to materials/commodities
                JournalTypeEnum.Cargo, JournalTypeEnum.CargoDepot,JournalTypeEnum.CollectCargo,
                JournalTypeEnum.EjectCargo,
                JournalTypeEnum.EngineerContribution,
                JournalTypeEnum.EngineerCraft, JournalTypeEnum.MarketBuy, JournalTypeEnum.MarketSell,
                JournalTypeEnum.MaterialCollected, JournalTypeEnum.MaterialDiscarded, JournalTypeEnum.Materials, JournalTypeEnum.MaterialTrade,
                JournalTypeEnum.Synthesis, JournalTypeEnum.TechnologyBroker,

                // Missions
                JournalTypeEnum.MissionAccepted, JournalTypeEnum.MissionCompleted, JournalTypeEnum.MissionAbandoned, JournalTypeEnum.MissionFailed, JournalTypeEnum.MissionRedirected,

                // Combat
                JournalTypeEnum.Bounty, JournalTypeEnum.CommitCrime, JournalTypeEnum.FactionKillBond,  JournalTypeEnum.PVPKill,
                JournalTypeEnum.Died, JournalTypeEnum.Resurrect, JournalTypeEnum.SelfDestruct, 

                // Journey
                JournalTypeEnum.FSDJump, JournalTypeEnum.CarrierJump, JournalTypeEnum.Location, JournalTypeEnum.Docked,

                // Ship state
                JournalTypeEnum.Loadout, JournalTypeEnum.MassModuleStore, JournalTypeEnum.ModuleBuy, JournalTypeEnum.ModuleSell,
                JournalTypeEnum.ModuleRetrieve,
                JournalTypeEnum.ModuleSellRemote, JournalTypeEnum.ModuleStore, JournalTypeEnum.ModuleSwap, JournalTypeEnum.SellShipOnRebuy,
                JournalTypeEnum.SetUserShipName, JournalTypeEnum.ShipyardBuy, JournalTypeEnum.ShipyardNew, JournalTypeEnum.ShipyardSell,
                JournalTypeEnum.ShipyardSwap , JournalTypeEnum.ShipyardTransfer, JournalTypeEnum.StoredModules, JournalTypeEnum.StoredShips,

                // scan
                JournalTypeEnum.Scan, JournalTypeEnum.SAASignalsFound, JournalTypeEnum.FSSBodySignals, JournalTypeEnum.ScanOrganic,  JournalTypeEnum.FSSSignalDiscovered, JournalTypeEnum.ScanBaryCentre, 
                JournalTypeEnum.CodexEntry,
                JournalTypeEnum.SellExplorationData,JournalTypeEnum.SAAScanComplete, 

                // misc
                JournalTypeEnum.ClearSavedGame,
            };

        static public JournalTypeEnum[] FullStatsEssentialEvents
        {
            get
            {
                var statsAdditional = new JournalTypeEnum[]
                {
                    // Travel
                    JournalTypeEnum.JetConeBoost, JournalTypeEnum.SAAScanComplete
                };
                return EssentialEvents.Concat(statsAdditional).ToArray();
            }
        }

        static public JournalTypeEnum[] JumpScanEssentialEvents = new JournalTypeEnum[]    
            {
                JournalTypeEnum.FSDJump,JournalTypeEnum.CarrierJump, JournalTypeEnum.Location,JournalTypeEnum.SAAScanComplete,

                JournalTypeEnum.Scan, JournalTypeEnum.SAASignalsFound, JournalTypeEnum.FSSBodySignals, JournalTypeEnum.ScanOrganic, JournalTypeEnum.FSSSignalDiscovered, JournalTypeEnum.ScanBaryCentre,
                JournalTypeEnum.CodexEntry
            };
        static public JournalTypeEnum[] JumpEssentialEvents = new JournalTypeEnum[]    
            {
                JournalTypeEnum.FSDJump,JournalTypeEnum.CarrierJump,JournalTypeEnum.Location,
            };

        static public JournalTypeEnum[] NoEssentialEvents = new JournalTypeEnum[]
            {
            };

        // Determine if we want an journal entry to enter the DB and/or generate a UI event
        // transient things are reported via UI method
        // synchronise with this all functions below
        public static void FilterJournalEntriesToDBUI(JournalEntry newentry, List<JournalEntry> jent, List<UIEvent> uievents)
        {
            if (newentry.EventTypeID == JournalTypeEnum.Music)
            {
                var jm = newentry as JournalEvents.JournalMusic;
                uievents.Add(new UIEvents.UIMusic(jm.MusicTrack, jm.MusicTrackID, jm.EventTimeUTC, false));
                jent.Add(newentry);     // we also want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.UnderAttack)
            {
                var ja = newentry as JournalEvents.JournalUnderAttack;
                uievents.Add(new UIEvents.UIUnderAttack(ja.Target, ja.EventTimeUTC, false));
                jent.Add(newentry);     // we also want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.SendText)
            {
                var jt = newentry as JournalEvents.JournalSendText;
                if (jt.Command) // EDD Commands are always UI and not to history
                {
                    uievents.Add(new UIEvents.UICommand(jt.Message, jt.To, jt.EventTimeUTC, false));
                    return;
                }
                else
                    jent.Add(newentry);     // we want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.ShipTargeted)
            {
                var jst = newentry as JournalEvents.JournalShipTargeted;
                if (jst.TargetLocked == false)      // target locked lost are UI events and not shown in history
                {
                    uievents.Add(new UIEvents.UIShipTargeted(jst, jst.EventTimeUTC, false));
                }
                jent.Add(newentry);     // we also want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.ReceiveText)
            {
                var jt = newentry as JournalEvents.JournalReceiveText;
                if (jt.Channel == "Info")
                {
                    uievents.Add(new UIEvents.UIReceiveText(jt, jt.EventTimeUTC, false));
                }
                else
                    jent.Add(newentry);     // we also want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.FSDTarget)
            {
                var jt = newentry as JournalEvents.JournalFSDTarget;
                uievents.Add(new UIEvents.UIFSDTarget(jt, jt.EventTimeUTC, false));
                jent.Add(newentry);     // we also want it in journal records
            }
            else if (newentry.EventTypeID == JournalTypeEnum.NavRouteClear)
            {
                var jnc = newentry as JournalEvents.JournalNavRouteClear;
                uievents.Add(new UIEvents.UINavRouteClear(jnc, jnc.EventTimeUTC, false));
                jent.Add(newentry);     // we also want it in journal records
            }
            else
                jent.Add(newentry); // all others add

        }

        // synchronise with DiscardDynamicJournalRecordsFromHistory these are events we never want to read from the DB again when creating history
        // They used to never be stored in the DB FilterJournalEntriesToDBUI, but now they are stored in there

        static public JournalTypeEnum[] NeverReadFromDBEvents = new JournalTypeEnum[]
        {
            JournalTypeEnum.Music, JournalTypeEnum.UnderAttack, JournalTypeEnum.FSDTarget, JournalTypeEnum.NavRouteClear, 
        };

        // These are discarded from history during reading database
        // During history read, NeverReadFromDBEvents above rejects the majority of events, but these need more processing to determine if to reject
        // this allows journal entries to be discarded when creating history during history read
        // true if to discard the current
        public static bool DiscardHistoryReadJournalRecordsFromHistory(JournalEntry je)
        {
            if (!EliteConfigInstance.InstanceOptions.DisableJournalRemoval)
            {
                if ((je.EventTypeID == JournalTypeEnum.ShipTargeted && ((JournalShipTargeted)je).TargetLocked == false) )      // Shiptargeted knock out lost target ones
                {
                    return true;
                }
            }

            return false;
        }

        // this allows journal entries to be discarded when creating history dynamically during play
        // true if to discard the current
        public static bool DiscardDynamicJournalRecordsFromHistory(JournalEntry je)
        {
            // these are just discarded from history - they used to be removed by EDJournalReaderfrom the db, but now since python support stored
            // dynamically, they are passed thru and discarded here

            if (!EliteConfigInstance.InstanceOptions.DisableJournalRemoval)
            {
                if (je.EventTypeID == JournalTypeEnum.Music ||
                    je.EventTypeID == JournalTypeEnum.UnderAttack ||
                    (je.EventTypeID == JournalTypeEnum.ShipTargeted && ((JournalShipTargeted)je).TargetLocked == false) ||      // Shiptargeted knock out lost target ones
                    je.EventTypeID == JournalTypeEnum.FSDTarget ||
                    je.EventTypeID == JournalTypeEnum.NavRouteClear)
                {
                    return true;
                }
            }

            return false;
        }

        // this allows journal entries to be discarded from history entry both during history read and dynamic play if its present in the list before
        public static bool DiscardJournalRecordDueToRepeat(JournalEntry je, List<HistoryEntry> list)
        {
            if (je.EventTypeID == JournalTypeEnum.ColonisationConstructionDepot)
            {
                JournalColonisationConstructionDepot cur = je as JournalColonisationConstructionDepot;

                // deep lookback 100 entries to see if another one is present before last dock etc.
                HistoryEntry helastconstruction = HistoryList.FindBeforeLastDockLoadGameShutdown(list, 100, JournalTypeEnum.ColonisationConstructionDepot);

                if (helastconstruction != null)
                {
                    JournalColonisationConstructionDepot prev = helastconstruction.journalEntry as JournalColonisationConstructionDepot;
                    bool same = prev.Equals(cur);
                   // System.Diagnostics.Debug.WriteLine($"Journal Colonisation Depot Discard={same} {cur.EventTimeUTC} {cur.ConstructionProgress} with {helastconstruction.EventTimeUTC}");
                    return same;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"Journal Colonisation Depot No matching entry {cur.EventTimeUTC} {cur.ConstructionProgress}");
                }

            }
            return false;
        }

        // this allows journal entries to be merged when creating history dynamically or during history read
        // data is merged into the prev entry
        // true if to discard the current
        public static bool MergeJournalRecordsFromHistory(JournalEntry prev, JournalEntry je)
        {
            // these are merged into prev
            if (!EliteConfigInstance.InstanceOptions.DisableJournalMerge && prev != null)
            {
                bool prevsame = je.EventTypeID == prev.EventTypeID;

                if (prevsame)
                {
                    if (je.EventTypeID == JournalTypeEnum.FuelScoop)  // merge scoops
                    {
                        JournalFuelScoop jfs = je as JournalFuelScoop;
                        JournalFuelScoop jfsprev = prev as JournalFuelScoop;
                        jfsprev.Scooped += jfs.Scooped;
                        jfsprev.Total = jfs.Total;
                        //System.Diagnostics.Debug.WriteLine("Merge FS " + jfsprev.EventTimeUTC);
                        return true;
                    }
                    else if ( je.EventTypeID == JournalTypeEnum.BuyMicroResources)
                    {
                        JournalBuyMicroResources jm = je as JournalBuyMicroResources;
                        JournalBuyMicroResources jmprev = prev as JournalBuyMicroResources;
                        jmprev.Merge(jm);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.Friends) // merge friends
                    {
                        JournalFriends jfprev = prev as JournalFriends;
                        JournalFriends jf = je as JournalFriends;
                        jfprev.AddFriend(jf);
                        //System.Diagnostics.Debug.WriteLine("Merge Friends " + jfprev.EventTimeUTC + " " + jfprev.NameList.Count);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
                    {
                        var jdprev = prev as JournalFSSSignalDiscovered;
                        var jd = je as JournalFSSSignalDiscovered;
                        if (jdprev.Signals[0].SystemAddress == jd.Signals[0].SystemAddress)     // only if same system address
                        {
                            jdprev.Add(jd);
                            return true;
                        }
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ShipTargeted)
                    {
                        var jdprev = prev as JournalShipTargeted;
                        var jd = je as JournalShipTargeted;
                        jdprev.Add(jd);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.FSSAllBodiesFound)
                    {
                        var jdprev = prev as JournalFSSAllBodiesFound;
                        var jd = je as JournalFSSAllBodiesFound;
                        if (jdprev.SystemAddress == jd.SystemAddress && jdprev.Count == jd.Count)          // same system, repeat, remove.  seen instances of this
                        {
                            //  System.Diagnostics.Debug.WriteLine("Duplicate FSSAllBodiesFound **********");
                            return true;
                        }
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ReceiveText)
                    {
                        var jdprev = prev as JournalReceiveText;
                        var jd = je as JournalReceiveText;

                        // merge if same channel 
                        if (jd.Channel == jdprev.Channel)
                        {
                            jdprev.Add(jd);
                            return true;
                        }
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ReservoirReplenished)
                    {
                        var jdprev = prev as JournalReservoirReplenished;
                        var jd = je as JournalReservoirReplenished;

                        if (jd.FuelReservoir == jdprev.FuelReservoir)      // if we have the same reservior
                        {
                            // we are throwing away the current one (jd) and keeping jdprev.
                            // If first time, we move into fuelstart the opening fuel main
                            if (jdprev.FuelStart == null)
                                jdprev.FuelStart = jdprev.FuelMain;
                            // move the updated fuel to jdprev.
                            jdprev.FuelMain = jd.FuelMain;
                            // and incremement the event count
                            jdprev.FuelEvents = jdprev.FuelEvents == null ? 2 : jdprev.FuelEvents.Value + 1;
                            return true;
                        }
                    }

                    else if (je.EventTypeID == JournalTypeEnum.FactionKillBond)
                    {
                        var jdprev = prev as JournalFactionKillBond;
                        var jd = je as JournalFactionKillBond;

                        if (jd.AwardingFaction == jdprev.AwardingFaction && jd.VictimFaction == jdprev.VictimFaction)
                        {
                            // we are throwing away the current one (jd) and keeping jdprev.
                            if (jdprev.NumberRewards == null)
                                jdprev.NumberRewards = 1;

                            jdprev.NumberRewards++;     // increment count
                            jdprev.Reward += jd.Reward;
                            return true;
                        }
                    }

                    else if (je.EventTypeID == JournalTypeEnum.ShieldState)
                    {
                        var jdprev = prev as JournalShieldState;
                        var jd = je as JournalShieldState;

                        jdprev.ShieldsUp = jd.ShieldsUp;            // move current state back to prev
                        jdprev.Events = (jdprev.Events ?? 1) + 1;   // increment counter
                        return true;
                    }

                    else if (je.EventTypeID == JournalTypeEnum.HullDamage)
                    {
                        var jdprev = prev as JournalHullDamage;
                        var jd = je as JournalHullDamage;

                        //System.Diagnostics.Debug.Write($"{jdprev.HealthPercentMax} {jdprev.Health} with {jd.Health} -> ");
                        jdprev.HealthPercentMax = System.Math.Max(jdprev.HealthPercentMax ?? (jdprev.Health * 100), jd.Health * 100);     // pick the maximum value
                        jdprev.Health = jd.Health;  // set to the current value
                        //System.Diagnostics.Debug.WriteLine($"{jdprev.HealthPercentMax} {jdprev.Health}");

                        return true;
                    }
                }
            }

            return false;
        }

        // dynamic delay on certain events to see if we can merge them.  Used only during dynamic update
        // 0 = no delay, delay for attempts to merge items..  
        public static int MergeTypeDelayForJournalEntries(JournalEntry je)
        {
            if (je.EventTypeID == JournalTypeEnum.Friends)
                return 2000;
            else if (je.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
                return 250;                                         // each one of these pushes it another 250ms into the future
            else if (je.EventTypeID == JournalTypeEnum.FuelScoop)
                return 10000;
            else if (je.EventTypeID == JournalTypeEnum.ShipTargeted)
                return 250;                                         // short, so normally does not merge unless your clicking around like mad
            else if (je.EventTypeID == JournalTypeEnum.ColonisationConstructionDepot)
                return 16000;                                         // short, so normally does not merge unless your clicking around like mad
            else
                return 0;
        }

    }
}

