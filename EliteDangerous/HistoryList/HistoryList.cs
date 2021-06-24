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

        private HistoryEntry hlastprocessed = null;

        List<HistoryEntry> reorderqueue = new List<HistoryEntry>();

        public HistoryList() { }

        public HistoryList(List<HistoryEntry> hl) { historylist = hl; }         // SPECIAL USE ONLY - DOES NOT COMPUTE ALL THE OTHER STUFF

        #region Entry processing

        // Called on a New Entry, by EDDiscoveryController:NewEntry, to add an journal entry in.  May return null if don't want it in history

        public List<HistoryEntry> AddJournalEntryToHistory(JournalEntry je, Action<string> logerror)   
        {
            HistoryEntry he = HistoryEntry.FromJournalEntry(je, hlastprocessed);     // we may check edsm for this entry

            he.UpdateMaterialsCommodities(MaterialCommoditiesMicroResources.Process(je, hlastprocessed?.journalEntry));

            // IN THIS order, so suits can be added, then weapons, then loadouts
            he.UpdateSuits(SuitList.Process(je, he.WhereAmI, he.System));
            he.UpdateWeapons(WeaponList.Process(je, he.WhereAmI, he.System));
            he.UpdateLoadouts(SuitLoadoutList.Process(je, WeaponList, he.WhereAmI, he.System));

            // check here to see if we want to remove the entry.. can move this lower later, but at first point where we have the data

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

            hlastprocessed = he;

            var reorderlist = ReorderRemove(he);

            foreach(var heh in reorderlist.EmptyIfNull())
            {
                heh.Index = historylist.Count;  // store its index
                historylist.Add(heh);        // then add to history
                AddToVisitsScan(logerror);  // add to scan database and complain if can't add. Do this after history add, so it has a list.
            }

            return reorderlist;
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

            hist.hlastprocessed = null;

            foreach (JournalEntry je in jlist)
            {
                if (MergeOrDiscardEntries(hist.hlastprocessed?.journalEntry, je))        // if we merge, don't store into HE
                {
                    continue;
                }

                // Clean up "UnKnown" systems from EDSM log
                if (je is JournalFSDJump && ((JournalFSDJump)je).StarSystem == "UnKnown")
                {
                    JournalEntry.Delete(je.Id);
                    continue;
                }

                HistoryEntry he = HistoryEntry.FromJournalEntry(je, hist.hlastprocessed);     // create entry

                he.UpdateMaterialsCommodities(hist.MaterialCommoditiesMicroResources.Process(je, hist.hlastprocessed?.journalEntry));

                // IN THIS order, so suits can be added, then weapons, then loadouts
                he.UpdateSuits(hist.SuitList.Process(je, he.WhereAmI, he.System));
                he.UpdateWeapons(hist.WeaponList.Process(je, he.WhereAmI, he.System));          // update the entries in suit entry list
                he.UpdateLoadouts(hist.SuitLoadoutList.Process(je, hist.WeaponList, he.WhereAmI, he.System));

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

                hist.hlastprocessed = he;

                var reorderlist = hist.ReorderRemove(he);

                foreach (var heh in reorderlist.EmptyIfNull())
                {
                    System.Diagnostics.Debug.WriteLine("   ++ {0} {1}", heh.EventTimeUTC.ToString(), heh.EntryType);
                    heh.Index = hist.historylist.Count; // store its index for quick ordering, after all removal etc
                    hist.historylist.Add(heh);        // then add to history
                    hist.AddToVisitsScan(null);  // add to scan database but don't complain
                }
            }

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " History List Created");

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

        public void AddToVisitsScan(Action<string> logerror)
        {
            HistoryEntry he = GetLast;

            if ((LastSystem == null || he.System.Name != LastSystem ) && he.System.Name != "Unknown" )   // if system is not last, we moved somehow (FSD, location, carrier jump), add
            {
                if (Visited.TryGetValue(he.System.Name, out var value))
                {
                    he.Visits = value.Visits + 1;               // visits is 1 more than previous entry
                    Visited[he.System.Name] = he;          // reset to point to latest he
                }
                else
                {
                    he.Visits = 1;                              // first visit
                    Visited[he.System.Name] = he;          // point to he
                }

                LastSystem = he.System.Name;
            }

            int pos = historylist.Count - 1;                // current entry index

            if (he.EntryType == JournalTypeEnum.Scan)
            {
                JournalScan js = he.journalEntry as JournalScan;

                if (!StarScan.AddScanToBestSystem(js, pos-1, historylist, out HistoryEntry jlhe, out JournalLocOrJump jl))
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
                StarScan.AddSAAScanToBestSystem((JournalSAAScanComplete)he.journalEntry, pos , historylist);
            }
            else if (he.EntryType == JournalTypeEnum.SAASignalsFound)
            {
                StarScan.AddSAASignalsFoundToBestSystem((JournalSAASignalsFound)he.journalEntry, pos , historylist);
            }
            else if (he.EntryType == JournalTypeEnum.FSSDiscoveryScan)
            {
                StarScan.SetFSSDiscoveryScan((JournalFSSDiscoveryScan)he.journalEntry, he.System);
            }
            else if (he.EntryType == JournalTypeEnum.FSSSignalDiscovered)
            {
                StarScan.AddFSSSignalsDiscoveredToSystem((JournalFSSSignalDiscovered)he.journalEntry, he.System);
            }
            else if (he.journalEntry is IBodyNameAndID)
            {
                StarScan.AddBodyToBestSystem((IBodyNameAndID)he.journalEntry, pos, historylist);
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

/*
Disembark ship to planet: shiplocker  disembark suit loadout  backpack  shiplocker
Disembark ship to station:  disembark  suit loadout  backpack  ship locker
Board ship:  shiplocker embark loadout shiplocker
Exit suit loadout menu, after loadout change, plus other places: SuitLoadout Backpack Ship locker
Throwing a grenade causes:  BackpackChange/Removed event.
Using a consumable: UseConsumable BackpackChange/Removed. 
Collect item: CollectItem BackpackChanged/Added.
Drop item: DropItem BackpackChanged/Removed.
Odyssey 3: Buy MR: BuyMicroResource ShipLocker
Odyssey 3: Trade MR: Shiplocker TradeMicroResource
Odyssey 3: Sell MR: ShipLocker SellMicroResources
*/

        public List<HistoryEntry> ReorderRemove(HistoryEntry he)
        {
            if (he.EventTimeUTC >= new DateTime(2021, 6, 17))        // this stuff only works on journals after odyssey 3 update
            {
                JournalTypeEnum queuetype = reorderqueue.Count > 0 ? reorderqueue[0].EntryType : JournalTypeEnum.Unknown;
                //                bool embarkdisembark = queuetype == JournalTypeEnum.Disembark || queuetype == JournalTypeEnum.Embark;

                System.Diagnostics.Debug.WriteLine("-> {0} {1} Reorder queue {2} {3}", he.EventTimeUTC.ToString(), he.EntryType, queuetype, reorderqueue.Count);

                if (he.EntryType == JournalTypeEnum.Embark || he.EntryType == JournalTypeEnum.Disembark)
                {
                    if (queuetype == JournalTypeEnum.ShipLocker)        // disembark to planet : SL-DIS-SuitLoadout-Backpack-Shiplocker
                    {
                        System.Diagnostics.Debug.WriteLine("-> Embark/disembark after shiplocker, discard shiplocker");
                        reorderqueue.Clear();
                    }

                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
                }
                else if (he.EntryType == JournalTypeEnum.SuitLoadout)      // either part of a Embark/Disembark - Suit loadout - Backpack - Ship locker sequence, or SL-BP-Ship locker
                {
                    if (queuetype == JournalTypeEnum.Disembark || queuetype == JournalTypeEnum.Embark)
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {
                        var prevreorder = reorderqueue;
                        reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                        return prevreorder;                                 // if anything queued up, play it out..
                    }
                }
                else if (he.EntryType == JournalTypeEnum.Backpack)
                {
                    if (queuetype == JournalTypeEnum.Disembark || queuetype == JournalTypeEnum.SuitLoadout)  // if part of this queue
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Isolated back pack, remove");
                        return null;
                    }
                }
                else if ( he.EntryType == JournalTypeEnum.Loadout)
                {
                    if (queuetype == JournalTypeEnum.Embark)  // if part of this queue
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Isolated loadout, let through");
                    }
                }
                else if (he.EntryType == JournalTypeEnum.ShipLocker)
                {
                    if (queuetype == JournalTypeEnum.Disembark || queuetype == JournalTypeEnum.Embark || queuetype == JournalTypeEnum.SuitLoadout )  // if part of this queue
                    {
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);      // move first entry to here
                        reorderqueue.Clear();
                        return new List<HistoryEntry> { he };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Isolated ship locker, remove");
                        return null;
                    }
                }
                else if (he.EntryType == JournalTypeEnum.CollectItems || he.EntryType == JournalTypeEnum.DropItems || he.EntryType == JournalTypeEnum.UseConsumable)
                {
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
                }
                else if (he.EntryType == JournalTypeEnum.BackpackChange)
                {
                    var bp = he.journalEntry as JournalBackpackChange;

                    // if its a grenade, use alchemy to turn it back into a use consumable
                    if (bp.ThrowGrenade)
                    {
                        System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Throw grenade, use Alchemy");
                        he.ReplaceJournalEntry(new JournalUseConsumable(bp.EventTimeUTC, bp.Removed[0], bp.TLUId, bp.CommanderId, bp.Id), he.EventTimeUTC);
                    }
                    else if (queuetype == JournalTypeEnum.CollectItems || queuetype == JournalTypeEnum.DropItems || queuetype == JournalTypeEnum.UseConsumable)
                    {
                        System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + queuetype + " use Alchemy ");
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);
                        reorderqueue.Clear();
                    }
                    else
                    {           // KEEP - transfer from ship does shiplocker/backpackchange
                        System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " Backpackchange keep");
                    }
                }
            }

            return new List<HistoryEntry> { he };       // pass it through
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
