/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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

//#define LISTSCANS

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace EliteDangerousCore
{
    public partial class HistoryList //: IEnumerable<HistoryEntry>
    {
        public MissionListAccumulator MissionListAccumulator { get; private set; } = new MissionListAccumulator(); // and mission list..
        public MaterialCommoditiesMicroResourceList MaterialCommoditiesMicroResources { get; private set; } = new MaterialCommoditiesMicroResourceList();
        public SuitWeaponList WeaponList { get; private set; } = new SuitWeaponList();
        public SuitList SuitList { get; private set; } = new SuitList();
        public SuitLoadoutList SuitLoadoutList { get; private set; } = new SuitLoadoutList();

        private List<HistoryEntry> historylist = new List<HistoryEntry>();  // oldest first here
        private Stats statisticsaccumulator = new Stats();

        // last one processed into this class, after merge, before MC removal, null at creation, updated by loadhistory and addjournalentry
        private HistoryEntry lastprocessedbeforeremoval = null;                         

        public HistoryList() { }

        public HistoryList(List<HistoryEntry> hl) { historylist = hl; }         // SPECIAL USE ONLY - DOES NOT COMPUTE ALL THE OTHER STUFF

        public void Copy(HistoryList other)       // Must copy all relevant items.. 
        {
            historylist = new List<HistoryEntry>(other.EntryOrder());       // clone list
            CashLedger = other.CashLedger;
            StarScan = other.StarScan;
            ShipInformationList = other.ShipInformationList;
            CommanderId = other.CommanderId;
            MissionListAccumulator = other.MissionListAccumulator;
            MaterialCommoditiesMicroResources = other.MaterialCommoditiesMicroResources;
            WeaponList = other.WeaponList;
            SuitList = other.SuitList;
            SuitLoadoutList = other.SuitLoadoutList;
            statisticsaccumulator = other.statisticsaccumulator;
            Shipyards = other.Shipyards;
            Outfitting = other.Outfitting;
            Visited = other.Visited;
            LastSystem = other.LastSystem;
        }

        public Dictionary<string,Stats.FactionInfo> GetStatsAtGeneration(uint g)
        {
            return statisticsaccumulator.GetAtGeneration(g);
        }


        #region Entry processing

        // Called on a New Entry, by EDDiscoveryController:NewEntry, to add an journal entry in.  May return null if don't want it in history

        public HistoryEntry AddJournalEntryToHistory(JournalEntry je, Action<string> logerror)   
        {
            HistoryEntry hlaststored = GetLast;

            HistoryEntry he = HistoryEntry.FromJournalEntry(je, hlaststored);     // we may check edsm for this entry

            he.UpdateMaterialsCommodities(MaterialCommoditiesMicroResources.Process(je,lastprocessedbeforeremoval?.journalEntry));

            // IN THIS order, so suits can be added, then weapons, then loadouts
            he.UpdateSuits(SuitList.Process(je, he.WhereAmI, he.System));
            he.UpdateWeapons(WeaponList.Process(je, he.WhereAmI, he.System));
            he.UpdateLoadouts(SuitLoadoutList.Process(je, WeaponList, he.WhereAmI, he.System));

            // check here to see if we want to remove the entry.. can move this lower later, but at first point where we have the data

            bool remove = CheckForRemovalAfterMCSuits(he, hlaststored, lastprocessedbeforeremoval);

            lastprocessedbeforeremoval = he;                                              // update this, we may remove now..

            if (remove)                                     
                return null;

            he.UpdateStats(je, statisticsaccumulator, he.StationFaction);
            he.UpdateSystemNote();

            CashLedger.Process(je);
            he.Credits = CashLedger.CashTotal;

            Shipyards.Process(je);
            Outfitting.Process(je);

            Tuple<ShipInformation, ModulesInStore> ret = ShipInformationList.Process(je, he.WhereAmI, he.System);
            he.UpdateShipInformation(ret.Item1);
            he.UpdateShipStoredModules(ret.Item2);
            
            he.UpdateMissionList(MissionListAccumulator.Process(je, he.System, he.WhereAmI));

            historylist.Add(he);        // then add to history

            AddToVisitsScan(this, this.historylist.Count-1, logerror);  // add to scan database and complain if can't add. Do this after history add, so it has a list.

            return he;
        }

        // History load system, read DB for entries and make a history up

        public static HistoryList LoadHistory(  Action<string> reportProgress, 
                                                int CurrentCommander, 
                                                int fullhistoryloaddaylimit, string essentialitems
                                             )
        {
            HistoryList hist = new HistoryList();

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL", true).Item1 + " History Load");

            reportProgress("Reading Database");

            List<JournalEntry> jlist;       // returned in date ascending, oldest first order.

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
            {
                jlist = JournalEntry.GetAll(CurrentCommander);
            }

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Journals read from DB");

            reportProgress( "Creating History");

            HistoryEntry hlaststored = null;

            foreach (JournalEntry je in jlist)
            {
                if (MergeOrDiscardEntries(hlaststored?.journalEntry, je))        // if we merge, don't store into HE
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

                HistoryEntry he = HistoryEntry.FromJournalEntry(je, hlaststored);     // create entry

                he.UpdateMaterialsCommodities(hist.MaterialCommoditiesMicroResources.Process(je,hist.lastprocessedbeforeremoval?.journalEntry));

                // IN THIS order, so suits can be added, then weapons, then loadouts
                he.UpdateSuits(hist.SuitList.Process(je, he.WhereAmI, he.System));
                he.UpdateWeapons(hist.WeaponList.Process(je, he.WhereAmI, he.System));          // update the entries in suit entry list
                he.UpdateLoadouts(hist.SuitLoadoutList.Process(je, hist.WeaponList, he.WhereAmI, he.System));

                // check here to see if we want to remove the entry.. can move this lower later, but at first point where we have the data
                bool remove = CheckForRemovalAfterMCSuits(he, hlaststored, hist.lastprocessedbeforeremoval);

                hist.lastprocessedbeforeremoval = he;       // and we store the last processed

                if ( remove)
                    continue;

                hist.historylist.Add(he);                                       // now add it to the history

                hlaststored = he;
            }

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " History List Created");
            reportProgress( "Analysing History");

            for( int i = 0; i < hist.historylist.Count; i++ )
            {
                HistoryEntry he = hist.historylist[i];
                JournalEntry je = he.journalEntry;

                he.UpdateStats(je, hist.statisticsaccumulator, he.StationFaction);
                he.UpdateSystemNote();

                hist.CashLedger.Process(je);            // update the ledger     
                he.Credits = hist.CashLedger.CashTotal;

                hist.Shipyards.Process(je);
                hist.Outfitting.Process(je);

                Tuple<ShipInformation, ModulesInStore> ret = hist.ShipInformationList.Process(je, he.WhereAmI, he.System);  // the ships
                he.UpdateShipInformation(ret.Item1);
                he.UpdateShipStoredModules(ret.Item2);

                he.UpdateMissionList(hist.MissionListAccumulator.Process(je, he.System, he.WhereAmI));

                AddToVisitsScan(hist, i, null);          // add to scan but don't complain if can't add
            }

            foreach (var s in hist.StarScan.ToProcess)
                System.Diagnostics.Debug.WriteLine("StarScan could not find " + s.Item2.SystemAddress + " at " + s.Item1.EventTimeUTC);

        //for (int i = hist.Count - 10; i < hist.Count; i++)  System.Diagnostics.Debug.WriteLine("Hist {0} {1} {2}", hist[i].EventTimeUTC, hist[i].Indexno , hist[i].EventSummary);

        // now database has been updated due to initial fill, now fill in stuff which needs the user database

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Anaylsis End");

            hist.CommanderId = CurrentCommander;

#if LISTSCANS
            {
                using (var fileout = new System.IO.StreamWriter(@"c:\code\scans.csv"))
                {
                    fileout.WriteLine($"System,0,fullname,ownname,customname,bodyname,bodydesignation, bodyid,parentlist");
                    foreach (var sn in hist.StarScan.ScansSortedByName())
                    {
                        foreach (var body in sn.Bodies)
                        {
                            string pl = body.ScanData?.ParentList();

                            fileout.WriteLine($"{sn.System.Name},0, {body.FullName},{body.OwnName},{body.CustomName},{body.ScanData?.BodyName},{body.ScanData?.BodyDesignation},{body.BodyID},{pl}");
                        }
                    }
                }
            }
#endif

            return hist;
        }

        public static void AddToVisitsScan(HistoryList hist, int pos, Action<string> logerror)
        {
            HistoryEntry he = hist[pos];

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

                if (!hist.StarScan.AddScanToBestSystem(js, pos - 1, hist.historylist, out HistoryEntry jlhe, out JournalLocOrJump jl))
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
                hist.StarScan.AddSAAScanToBestSystem((JournalSAAScanComplete)he.journalEntry, pos , hist.historylist);
            }
            else if (he.EntryType == JournalTypeEnum.SAASignalsFound)
            {
                hist.StarScan.AddSAASignalsFoundToBestSystem((JournalSAASignalsFound)he.journalEntry, pos , hist.historylist);
            }
            else if (he.EntryType == JournalTypeEnum.FSSDiscoveryScan)
            {
                hist.StarScan.SetFSSDiscoveryScan((JournalFSSDiscoveryScan)he.journalEntry, he.System);
            }
            else if (he.EntryType == JournalTypeEnum.FSSSignalDiscovered)
            {
                hist.StarScan.AddFSSSignalsDiscoveredToSystem((JournalFSSSignalDiscovered)he.journalEntry, he.System);
            }
            else if (he.journalEntry is IBodyNameAndID)
            {
                hist.StarScan.AddBodyToBestSystem((IBodyNameAndID)he.journalEntry, pos, hist.historylist);
            }
        }

        public static int MergeTypeDelay(JournalEntry je)   // 0 = no delay, delay for attempts to merge items..  used by new entry mech only, not by historylist
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

        // this allows entries to be merged or discarded before any processing
        // true if to discard
        public static bool MergeOrDiscardEntries(JournalEntry prev, JournalEntry je)
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

        // this allows for discarding after materialcommodities and suit processing

        public static bool CheckForRemovalAfterMCSuits(HistoryEntry he, HistoryEntry hlaststored, HistoryEntry hlastprocessed)
        {
            // we generally try and remove cargo as spam, but we need to keep its updated MC

            if (hlaststored == null || hlastprocessed == null)
                return false;

            bool removeodysseyevents = true;       // set to try normally, false for debug

            if (he.EntryType == JournalTypeEnum.Cargo)
            {
                var cargo = he.journalEntry as JournalCargo;

                if (cargo.EDDFromFile == true ||       // if from file, its a newer entry, after nov 20, so we remove it
                                                       // else if older than when this flag was introduced, we remove if its not following the two types below
                     (cargo.EventTimeUTC < new DateTime(2020, 11, 20) && hlaststored.EntryType != JournalTypeEnum.Statistics && hlaststored.EntryType != JournalTypeEnum.Friends)
                    )
                {
                    //  System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Remove cargo and assign to previous entry FromFile: " + cargo.EDDFromFile);
                    hlaststored.UpdateMaterialsCommodities(he.MaterialCommodity);        // assign its updated commodity list to previous entry
                    return true;
                }
                else
                {
                    //  System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Keep cargo entry FromFile: " + cargo.EDDFromFile);
                }
            }
            else if (he.EntryType == JournalTypeEnum.ShipLocker)      // we updated the MCML list, just remove. Next entry will deal with it
            {
                System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Shiplocker removed");
                return removeodysseyevents;
            }
            else if (he.EntryType == JournalTypeEnum.SuitLoadout)
            {
                // sequence disembark/embark, suitloadout, push back and remove.  we use laststored since we keep embark/disembark
                if (hlaststored.EntryType == JournalTypeEnum.Disembark || hlaststored.EntryType == JournalTypeEnum.Embark)       
                {
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Suitloadout following disembark, copy back");
                    hlaststored.UpdateSuits(he.Suits);
                    hlaststored.UpdateWeapons(he.Weapons);
                    hlaststored.UpdateLoadouts(he.Loadouts);
                    return removeodysseyevents;
                }
                else
                {
                    // keep for now
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Suitloadout keep");
                }
            }
            else if (he.EntryType == JournalTypeEnum.Backpack)
            {
                // sequence disembark, suitloadout, backpack, push back. 
                // Note suitloadout should have been removed by above so it should now be disembark, backpack.  
                // use last stored as disembark should be there as the last stored entry
                if (hlaststored.EntryType == JournalTypeEnum.Disembark )       
                {
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " backpack following disembark or suitloadout, copy back mats");
                    hlaststored.UpdateMaterialsCommodities(he.MaterialCommodity);
                }

                System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " backpack removed");
                return removeodysseyevents;
            }
            else if (he.EntryType == JournalTypeEnum.BackpackChange)
            {
                var bp = he.journalEntry as JournalBackpackChange;

                // if its a grenade, use alchemy to turn it back into a use consumable
                if (bp.ThrowGrenade)       
                {
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Throw grenade, use Alchemy");
                    he.ReplaceJournalEntry(new JournalUseConsumable(bp.EventTimeUTC, bp.Removed[0].Name, bp.Removed[0].Name_Localised, bp.Removed[0].Category, bp.TLUId, bp.CommanderId, bp.Id));
                }

                // sequence droptitems (etc), backpack change, but we need to make sure we are attached to the last processed, not the last stored, since we remove
                // BPC and if you have sequences like USEC(frag) SL BPL (get from ship) and with the SL being removed it would be confused

                else if (hlastprocessed.EntryType == JournalTypeEnum.DropItems || hlastprocessed.EntryType == JournalTypeEnum.CollectItems || hlastprocessed.EntryType == JournalTypeEnum.UseConsumable)
                {
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " BackPackChange following " + hlaststored.EntryType + " update prev " + he.MaterialCommodity + ", remove");
                    hlaststored.UpdateMaterialsCommodities(he.MaterialCommodity);         // backpack change has the change, 
                    return removeodysseyevents;
                }
                else
                {           // KEEP - transfer from ship does shiplocker/backpackchange
                    System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Backpackchange keep");
                }
            }

            return false;
        }

        #endregion

        #region EDSM

        public void FillInPositionsFSDJumps(Action<string> logger)       // call if you want to ensure we have the best posibile position data on FSD Jumps.  Only occurs on pre 2.1 netlogs
        {
            List<Tuple<HistoryEntry, ISystem>> updatesystems = new List<Tuple<HistoryEntry, ISystem>>();

            if (!SystemsDatabase.Instance.RebuildRunning)       // only run this when the system db is stable.. this prevents the UI from slowing to a standstill
            {
                foreach (HistoryEntry he in historylist)
                {
                    // try and load ones without position.. if its got pos we are happy.  If its 0,0,0 and its not sol, it may just be a stay entry

                    if (he.IsFSDCarrierJump)
                    {
                        //logger?.Invoke($"Checking system {he.System.Name}");

                        if (!he.System.HasCoordinate || (Math.Abs(he.System.X) < 1 && Math.Abs(he.System.Y) < 1 && Math.Abs(he.System.Z) < 0 && he.System.Name != "Sol"))
                        {
                            ISystem found = SystemCache.FindSystem(he.System, true);        // find, thru edsm if required
                                    
                            if (found != null)
                            {
                                logger?.Invoke($"System {he.System.Name} found system in EDSM");
                                updatesystems.Add(new Tuple<HistoryEntry, ISystem>(he, found));
                            }
                            else
                                logger?.Invoke($"System {he.System.Name} failed to find system in EDSM");
                        }
                    }
                }
            }

            if (updatesystems.Count > 0)
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbTransaction txn = cn.Connection.BeginTransaction())        // take a transaction over this
                    {
                        foreach (Tuple<HistoryEntry, ISystem> hesys in updatesystems)
                        {
                            logger?.Invoke($"Update position of {hesys.Item1.System.Name} at {hesys.Item1.EntryNumber} in journal");
                            hesys.Item1.journalEntry.UpdateStarPosition(hesys.Item2, cn.Connection, txn);
                            hesys.Item1.UpdateSystem(hesys.Item2);
                        }

                        txn.Commit();
                    }
                });
            }
        }

#endregion

    }
}
