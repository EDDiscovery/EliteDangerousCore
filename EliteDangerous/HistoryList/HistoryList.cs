/*
 * Copyright © 2016 - 2020 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class HistoryList //: IEnumerable<HistoryEntry>
    {
        private List<HistoryEntry> historylist = new List<HistoryEntry>();  // oldest first here
        private MissionListAccumulator missionlistaccumulator = new MissionListAccumulator(); // and mission list..

        public HistoryList() { }

        public HistoryList(List<HistoryEntry> hl) { historylist = hl; }         // SPECIAL USE ONLY - DOES NOT COMPUTE ALL THE OTHER STUFF

        public void Copy(HistoryList other)       // Must copy all relevant items.. been caught out by this 23/6/2017
        {
            historylist.Clear();

            foreach (var ent in other.EntryOrder)
            {
                historylist.Add(ent);
                Debug.Assert(ent.MaterialCommodity != null);
            }

            CashLedger = other.CashLedger;
            StarScan = other.StarScan;
            ShipInformationList = other.ShipInformationList;
            CommanderId = other.CommanderId;
            missionlistaccumulator = other.missionlistaccumulator;
            Shipyards = other.Shipyards;
            Outfitting = other.Outfitting;
            Visited = other.Visited;
            LastSystem = other.LastSystem;
        }

        #region EDSM

        public void FillInPositionsFSDJumps()       // call if you want to ensure we have the best posibile position data on FSD Jumps.  Only occurs on pre 2.1 with lazy load of just name/edsmid
        {
            List<Tuple<HistoryEntry, ISystem>> updatesystems = new List<Tuple<HistoryEntry, ISystem>>();

            if (!SystemsDatabase.Instance.RebuildRunning)       // only run this when the system db is stable.. this prevents the UI from slowing to a standstill
            {
                SystemsDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    foreach (HistoryEntry he in historylist)
                    {
                        if (he.IsFSDCarrierJump && !he.System.HasCoordinate)// try and load ones without position.. if its got pos we are happy
                        {           // done in two IFs for debugging, in case your wondering why!
                            if (he.System.source != SystemSource.FromEDSM && he.System.EDSMID == 0)   // and its not from EDSM and we have not already tried
                            {
                                ISystem found = SystemCache.FindSystem(he.System, cn);
                                if (found != null)
                                    updatesystems.Add(new Tuple<HistoryEntry, ISystem>(he, found));
                            }
                        }
                    }
                });
            }

            if (updatesystems.Count > 0)
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbTransaction txn = cn.Connection.BeginTransaction())        // take a transaction over this
                    {
                        foreach (Tuple<HistoryEntry, ISystem> he in updatesystems)
                        {
                            FillInSystemFromDBInt(he.Item1, he.Item2, cn.Connection, txn);  // fill, we already have an EDSM system to use
                        }

                        txn.Commit();
                    }
                });
            }
        }

        public void FillEDSM(HistoryEntry syspos)       // call to fill in ESDM data for entry, and also fills in all others pointing to the system object
        {
            if (syspos.System.source == SystemSource.FromEDSM || syspos.System.EDSMID == -1)  // if set already, or we tried and failed..
            {
                //System.Diagnostics.Debug.WriteLine("Checked System {0} already id {1} ", syspos.System.name , syspos.System.id_edsm);
                return;
            }

            ISystem edsmsys = SystemCache.FindSystem(syspos.System);        // see if we have it..

            if (edsmsys != null)                                            // if we found it externally, fill in info
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbTransaction txn = cn.Connection.BeginTransaction())
                    {
                        FillInSystemFromDBInt(syspos, edsmsys, cn.Connection, txn); // and fill in using this connection/tx
                        txn.Commit();
                    }
                });
            }
            else
                FillInSystemFromDBInt(syspos, null, null, null);        // else fill in using null system, which means just mark it checked
        }

        private void FillInSystemFromDBInt(HistoryEntry syspos, ISystem edsmsys, SQLiteConnectionUser uconn, DbTransaction utn)       // call to fill in ESDM data for entry, and also fills in all others pointing to the system object
        {
            List<HistoryEntry> alsomatching = new List<HistoryEntry>();

            foreach (HistoryEntry he in historylist)       // list of systems in historylist using the same system object
            {
                if (Object.ReferenceEquals(he.System, syspos.System))
                    alsomatching.Add(he);
            }

            if (edsmsys != null)
            {
                ISystem oldsys = syspos.System;

                bool updateedsmid = oldsys.EDSMID != edsmsys.EDSMID;
                bool updatesyspos = edsmsys.HasCoordinate &&
                                    (edsmsys.Xi != oldsys.Xi || edsmsys.Yi != oldsys.Yi || edsmsys.Zi != oldsys.Zi) &&
                                    oldsys.source != SystemSource.FromJournal; // NEVER EVER EVER OVERRIDE JOURNAL COORDINATES
                bool updatename = oldsys.source != SystemSource.FromJournal ||
                                  !oldsys.Name.Equals(edsmsys.Name, StringComparison.InvariantCultureIgnoreCase);

                ISystem newsys = new SystemClass
                {
                    EDSMID = updateedsmid ? edsmsys.EDSMID : oldsys.EDSMID,
                    Name = updatename ? edsmsys.Name : oldsys.Name,
                    X = updatesyspos ? edsmsys.X : oldsys.X,
                    Y = updatesyspos ? edsmsys.Y : oldsys.Y,
                    Z = updatesyspos ? edsmsys.Z : oldsys.Z,
                    GridID = edsmsys.GridID,
                    SystemAddress = oldsys.SystemAddress ?? edsmsys.SystemAddress,

                    EDDBID = edsmsys.EDDBID,
                    Population = oldsys.Government == EDGovernment.Unknown ? edsmsys.Population : oldsys.Population,
                    Faction = oldsys.Faction ?? edsmsys.Faction,
                    Government = oldsys.Government == EDGovernment.Unknown ? edsmsys.Government : oldsys.Government,
                    Allegiance = oldsys.Allegiance == EDAllegiance.Unknown ? edsmsys.Allegiance : oldsys.Allegiance,
                    State = oldsys.State == EDState.Unknown ? edsmsys.State : oldsys.State,
                    Security = oldsys.Security == EDSecurity.Unknown ? edsmsys.Security : oldsys.Security,
                    PrimaryEconomy = oldsys.PrimaryEconomy == EDEconomy.Unknown ? edsmsys.PrimaryEconomy : oldsys.PrimaryEconomy,
                    Power = !oldsys.Power.HasChars() ? edsmsys.Power : oldsys.Power,
                    PowerState = !oldsys.PowerState.HasChars() ? edsmsys.PowerState : oldsys.PowerState,
                    NeedsPermit = edsmsys.NeedsPermit,
                    EDDBUpdatedAt = edsmsys.EDDBUpdatedAt,
                    source = SystemSource.FromEDSM
                };

                foreach (HistoryEntry he in alsomatching)       // list of systems in historylist using the same system object
                {
                    bool updatepos = he.IsLocOrJump && updatesyspos;

                    if (updatepos || updateedsmid)
                        JournalEntry.UpdateEDSMIDPosJump(he.Journalid, edsmsys, updatepos, -1, uconn, utn);  // update pos and edsmid, jdist not updated

                    he.System = newsys;
                }
            }
            else
            {
                foreach (HistoryEntry he in alsomatching)       // list of systems in historylist using the same system object
                    he.System.EDSMID = -1;                     // can't do it
            }
        }

        #endregion

        #region Entry processing

        // Called on a New Entry, by EDDiscoveryController:NewEntry, to add an journal entry in.  May return null if don't want it in history

        public HistoryEntry AddJournalEntryToHistory(JournalEntry je, Action<string> logerror)   
        {
            HistoryEntry hprev = GetLast;

            HistoryEntry he = HistoryEntry.FromJournalEntry(je, hprev, true, out bool journalupdate);     // we may check edsm for this entry

            he.UpdateMaterialsCommodities(je, hprev?.MaterialCommodity);           // let some processes which need the user db to work
            Debug.Assert(he.MaterialCommodity != null);

            if (CheckForRemoval(he, hprev))                                     // check here to see if we want to remove the entry.. can move this lower later, but at first point where we have the data
                return null;

            he.UpdateStats(je, hprev?.Stats, he.StationFaction);
            he.UpdateSystemNote();

            CashLedger.Process(je);
            he.Credits = CashLedger.CashTotal;

            Shipyards.Process(je);
            Outfitting.Process(je);

            Tuple<ShipInformation, ModulesInStore> ret = ShipInformationList.Process(je, he.WhereAmI, he.System);
            he.UpdateShipInformation(ret.Item1);
            he.UpdateShipStoredModules(ret.Item2);
            
            he.UpdateMissionList(missionlistaccumulator.Process(je, he.System, he.WhereAmI));

            if (journalupdate)
            {
                JournalFSDJump jfsd = je as JournalFSDJump;

                if (jfsd != null)
                {
                    UserDatabase.Instance.ExecuteWithDatabase(cn =>
                    {
                        JournalEntry.UpdateEDSMIDPosJump(jfsd.Id, he.System, !jfsd.HasCoordinate && he.System.HasCoordinate, jfsd.JumpDist, cn.Connection);
                    });
                }
            }

            historylist.Add(he);        // then add to history

            AddToVisitsScan(this, he, logerror);  // add to scan database and complain if can't add. Do this after history add, so it has a list.
            
            return he;
        }

        public static HistoryList LoadHistory(EDJournalUIScanner journalmonitor, Func<bool> cancelRequested, Action<int, string> reportProgress,
                                    string NetLogPath = null,
                                    bool ForceNetLogReload = false,
                                    bool ForceJournalReload = false,
                                    int CurrentCommander = Int32.MinValue,
                                    int fullhistoryloaddaylimit = 0,
                                    string essentialitems = ""
                                    )
        {
            HistoryList hist = new HistoryList();

            if (CurrentCommander >= 0)
            {
                journalmonitor.SetupWatchers();   // Parse files stop monitor..
                int forcereloadoflastn = ForceJournalReload ? int.MaxValue / 2 : 0;     // if forcing a reload, we indicate that by setting the reload count to a very high value, but not enough to cause int wrap
                journalmonitor.ParseJournalFilesOnWatchers((p, s) => reportProgress(p, s), forcereloadoflastn );

                if (NetLogPath != null)
                {
                    string errstr = null;
                    NetLogClass.ParseFiles(NetLogPath, out errstr, EDCommander.Current.MapColour, () => cancelRequested(), (p, s) => reportProgress(p, s), ForceNetLogReload, currentcmdrid: CurrentCommander);
                }
            }

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLap() + " Files read ");

            reportProgress(-1, "Reading Database");

            List<JournalEntry> jlist;       // returned in date ascending, oldest first order.

            System.Diagnostics.Debug.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL", true) + "History Load");

            if (fullhistoryloaddaylimit > 0)
            {
                var list = (essentialitems == nameof(JournalEssentialEvents.JumpScanEssentialEvents)) ? JournalEssentialEvents.JumpScanEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.JumpEssentialEvents)) ? JournalEssentialEvents.JumpEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.NoEssentialEvents)) ? JournalEssentialEvents.NoEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.FullStatsEssentialEvents)) ? JournalEssentialEvents.FullStatsEssentialEvents :
                            JournalEssentialEvents.EssentialEvents;

                jlist = JournalEntry.GetAll(CurrentCommander,
                    ids: list,
                    allidsafterutc: DateTime.UtcNow.Subtract(new TimeSpan(fullhistoryloaddaylimit, 0, 0, 0))
                    );
            }
            else
                jlist = JournalEntry.GetAll(CurrentCommander);

            System.Diagnostics.Debug.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL") + "History Load END");

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLap() + " Database read " + jlist.Count);

            List<Tuple<JournalEntry, HistoryEntry>> jlistUpdated = new List<Tuple<JournalEntry, HistoryEntry>>();

            HistoryEntry hprev = null;
            JournalEntry jprev = null;

            reportProgress(-1, "Creating History");

            bool checkforunknownsystemsindb = true;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (JournalEntry je in jlist)
            {
                if (MergeEntries(jprev, je))        // if we merge, don't store into HE
                {
                    continue;
                }

                // Clean up "UnKnown" systems from EDSM log
                if (je is JournalFSDJump && ((JournalFSDJump)je).StarSystem == "UnKnown")
                {
                    JournalEntry.Delete(je.Id);
                    continue;
                }

                if (je is EliteDangerousCore.JournalEvents.JournalMusic)      // remove music.. not shown.. now UI event. remove it for backwards compatibility
                {
                    //System.Diagnostics.Debug.WriteLine("**** Filter out " + je.EventTypeStr + " on " + je.EventTimeLocal.ToString());
                    continue;
                }

                long timetoload = sw.ElapsedMilliseconds;
                HistoryEntry he = HistoryEntry.FromJournalEntry(je, hprev, checkforunknownsystemsindb, out bool journalupdate);

                if (sw.ElapsedMilliseconds - timetoload > 100)
                {
                    System.Diagnostics.Debug.WriteLine("DB is slow - probably 3dmap is being initialised, give up checking for old systems");
                    checkforunknownsystemsindb = false;
                }

                // **** REMEMBER NEW Journal entry needs this too *****************

                he.UpdateMaterialsCommodities(je, hprev?.MaterialCommodity);        // update material commodities
                Debug.Assert(he.MaterialCommodity != null);

                if (CheckForRemoval(he, hprev))                                     // check here to see if we want to remove the entry.. can move this lower later, but at first point where we have the data
                    continue;

                he.UpdateStats(je, hprev?.Stats, he.StationFaction);
                he.UpdateSystemNote();

                hist.CashLedger.Process(je);            // update the ledger     
                he.Credits = hist.CashLedger.CashTotal;

                hist.Shipyards.Process(je);
                hist.Outfitting.Process(je);

                Tuple<ShipInformation, ModulesInStore> ret = hist.ShipInformationList.Process(je, he.WhereAmI, he.System);  // the ships
                he.UpdateShipInformation(ret.Item1);
                he.UpdateShipStoredModules(ret.Item2);

                he.UpdateMissionList(hist.missionlistaccumulator.Process(je, he.System, he.WhereAmI));

                hist.historylist.Add(he);           // now add it to the history

                AddToVisitsScan(hist, he, null);          // add to scan but don't complain if can't add.  Do this AFTER add, as it uses the history list

                hprev = he;
                jprev = je;

                if (journalupdate)
                {
                    jlistUpdated.Add(new Tuple<JournalEntry, HistoryEntry>(je, he));
                    Debug.WriteLine("Queued update requested {0} {1}", he.System.EDSMID, he.System.Name);
                }
            }

            reportProgress(-1, "Updating user statistics");

            // see if there are any DB entries to update

            if (jlistUpdated.Count > 0)
            {
                reportProgress(-1, "Updating journal entries");

                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbTransaction txn = cn.Connection.BeginTransaction())
                    {
                        foreach (Tuple<JournalEntry, HistoryEntry> jehe in jlistUpdated)
                        {
                            JournalEntry je = jehe.Item1;
                            HistoryEntry he = jehe.Item2;

                            double dist = (je is JournalFSDJump) ? (je as JournalFSDJump).JumpDist : 0;
                            bool updatecoord = (je is JournalLocOrJump) ? (!(je as JournalLocOrJump).HasCoordinate && he.System.HasCoordinate) : false;

                            Debug.WriteLine("Push update {0} {1} to JE {2} HE {3}", he.System.EDSMID, he.System.Name, je.Id, he.Indexno);
                            JournalEntry.UpdateEDSMIDPosJump(je.Id, he.System, updatecoord, dist, cn.Connection, txn);
                        }

                        txn.Commit();
                    }
                });
            }

            // now database has been updated due to initial fill, now fill in stuff which needs the user database

            hist.CommanderId = CurrentCommander;

            EDCommander.Current.FID = hist.GetCommanderFID();               // ensure FID is set.. the other place it gets changed is a read of LoadGame.

            reportProgress(-1, "Done");

            return hist;
        }

        public static bool CheckForRemoval(HistoryEntry he, HistoryEntry hprev)
        {
            if (he.EntryType == JournalTypeEnum.Cargo && hprev != null)       // we generally try and remove cargo as spam, but we need to keep its updated MC
            {
                var cargo = he.journalEntry as JournalCargo;

                if (cargo.EDDFromFile == true ||       // if from file, its a newer entry, after nov 20, so we remove it
                                                       // else if older than when this flag was introduced, we remove if its not following the two types below
                     (cargo.EventTimeUTC < new DateTime(2020, 11, 20) && hprev.EntryType != JournalTypeEnum.Statistics && hprev.EntryType != JournalTypeEnum.Friends)
                    )
                {
                  //  System.Diagnostics.Debug.WriteLine(he.EventTimeUTC + " Remove cargo and assign to previous entry FromFile: " + cargo.EDDFromFile);
                    hprev.UpdateMaterialCommodity(he.MaterialCommodity);        // assign its updated commodity list to previous entry
                    return true;
                }
                else
                {
                  //  System.Diagnostics.Debug.WriteLine(he.EventTimeUTC + " Keep cargo entry FromFile: " + cargo.EDDFromFile);
                }
            }

            return false;
        }


        public static void AddToVisitsScan(HistoryList hist, HistoryEntry he, Action<string> logerror )
        {
            if ((hist.LastSystem == null || he.System.Name != hist.LastSystem ) && he.System.Name != "Unknown" )   // if system is not last, we moved somehow (FSD, location, carrier jump), add
            {
                if (hist.Visited.TryGetValue(he.System.Name, out var value))
                {
                    he.Visits = value.Visits + 1;               // visits is 1 more than previous entry
                    hist.Visited[he.System.Name] = he;          // reset to point to latest he
                }
                else
                {
                    he.Visits = 1;                              // first visit
                    hist.Visited[he.System.Name] = he;          // point to he
                }
                hist.LastSystem = he.System.Name;
            }

            if (he.EntryType == JournalTypeEnum.Scan)
            {
                JournalScan js = he.journalEntry as JournalScan;

                if (!hist.StarScan.AddScanToBestSystem(js, hist.historylist.Count - 1, hist.historylist, out HistoryEntry jlhe, out JournalLocOrJump jl))
                {
                    if (logerror != null)
                    {
                        // Ignore scans where the system name has been changed
                        // Also ignore belt clusters
                        var bodyname = js.BodyDesignation ?? js.BodyName;

                        if (bodyname == null)
                        {
                            logerror("Body name not set in scan entry");
                        }
                        else if (jl == null || (jl.StarSystem.Equals(jlhe.System.Name, StringComparison.InvariantCultureIgnoreCase) && !bodyname.ToLowerInvariant().Contains(" belt cluster ")))
                        {
                            logerror("Cannot add scan to system - alert the EDDiscovery developers using either discord or Github (see help)" + Environment.NewLine +
                                                "Scan object " + js.BodyName + " in " + he.System.Name);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("******** Cannot add scan to system " + (he.journalEntry as JournalScan).BodyName + " in " + he.System.Name);
                    }
                }
            }
            else if (he.EntryType == JournalTypeEnum.SAAScanComplete)
            {
                hist.StarScan.AddSAAScanToBestSystem((JournalSAAScanComplete)he.journalEntry, hist.historylist.Count - 1, hist.historylist);
            }
            else if (he.EntryType == JournalTypeEnum.SAASignalsFound)
            {
                hist.StarScan.AddSAASignalsFoundToBestSystem((JournalSAASignalsFound)he.journalEntry, hist.historylist.Count - 1, hist.historylist);
            }
            else if (he.EntryType == JournalTypeEnum.FSSDiscoveryScan && he.System != null)
            {
                hist.StarScan.SetFSSDiscoveryScan((JournalFSSDiscoveryScan)he.journalEntry, he.System);
            }
            else if (he.journalEntry is IBodyNameAndID)
            {
                hist.StarScan.AddBodyToBestSystem((IBodyNameAndID)he.journalEntry, hist.historylist.Count - 1, hist.historylist);
            }
        }

        public static int MergeTypeDelay(JournalEntry je)   // 0 = no delay, delay for attempts to merge items..
        {
            if (je.EventTypeID == JournalTypeEnum.Friends)
                return 2000;
            else if (je.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
                return 2000;
            else if (je.EventTypeID == JournalTypeEnum.FuelScoop)
                return 10000;
            else if (je.EventTypeID == JournalTypeEnum.ShipTargeted)
                return 250; // short, so normally does not merge unless your clicking around like mad
            else
                return 0;
        }

        // true if merged back to previous..
        public static bool MergeEntries(JournalEntry prev, JournalEntry je)
        {
            if (prev != null && !EliteConfigInstance.InstanceOptions.DisableMerge)
            {
                bool prevsame = je.EventTypeID == prev.EventTypeID;

                if (prevsame)
                {
                    if (je.EventTypeID == JournalTypeEnum.FuelScoop)  // merge scoops
                    {
                        EliteDangerousCore.JournalEvents.JournalFuelScoop jfs = je as EliteDangerousCore.JournalEvents.JournalFuelScoop;
                        EliteDangerousCore.JournalEvents.JournalFuelScoop jfsprev = prev as EliteDangerousCore.JournalEvents.JournalFuelScoop;
                        jfsprev.Scooped += jfs.Scooped;
                        jfsprev.Total = jfs.Total;
                        //System.Diagnostics.Debug.WriteLine("Merge FS " + jfsprev.EventTimeUTC);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.Friends) // merge friends
                    {
                        EliteDangerousCore.JournalEvents.JournalFriends jfprev = prev as EliteDangerousCore.JournalEvents.JournalFriends;
                        EliteDangerousCore.JournalEvents.JournalFriends jf = je as EliteDangerousCore.JournalEvents.JournalFriends;
                        jfprev.AddFriend(jf);
                        //System.Diagnostics.Debug.WriteLine("Merge Friends " + jfprev.EventTimeUTC + " " + jfprev.NameList.Count);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalFSSSignalDiscovered;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalFSSSignalDiscovered;
                        jdprev.Add(jd);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ShipTargeted)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalShipTargeted;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalShipTargeted;
                        jdprev.Add(jd);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.UnderAttack)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalUnderAttack;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalUnderAttack;
                        jdprev.Add(jd.Target);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ReceiveText)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalReceiveText;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalReceiveText;

                        // merge if same channel 
                        if (jd.Channel == jdprev.Channel)
                        {
                            jdprev.Add(jd);
                            return true;
                        }
                    }
                    else if (je.EventTypeID == JournalTypeEnum.FSSAllBodiesFound)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalFSSAllBodiesFound;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalFSSAllBodiesFound;

                        // throw away if same..
                        if (jdprev.SystemName == jd.SystemName && jdprev.Count == jd.Count) // if same, we just waste the repeater, ED sometimes spews out multiples
                        {
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        #endregion
    }
}
