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
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("HL {historylist.Count}")]
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

        #region Entry processing

        // Called on a New Entry, by EDDiscoveryController:NewEntry, to add an journal entry in.  May return null or empty list, or multiple entries.

        public List<HistoryEntry> AddJournalEntryToHistory(JournalEntry je, Action<string> logerror)   
        {
            HistoryEntry he = HistoryEntry.FromJournalEntry(je, hlastprocessed);     // we may check edsm for this entry

            he.UpdateMaterialsCommodities(MaterialCommoditiesMicroResources.Process(je, hlastprocessed?.journalEntry, he.Status.TravelState == HistoryEntryStatus.TravelStateType.SRV));

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

        public static HistoryList LoadHistory(  Action<string> reportProgress, Func<bool> cancelRequested,
                                                int CurrentCommander, 
                                                int fullhistoryloaddaylimit, string essentialitems
                                             )
        {
            HistoryList hist = new HistoryList();

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL", true).Item1 + " History Load");

            reportProgress("Reading Database");

            List<JournalEntry.TableData> tabledata;

            if (fullhistoryloaddaylimit > 0)
            {
                var list = (essentialitems == nameof(JournalEssentialEvents.JumpScanEssentialEvents)) ? JournalEssentialEvents.JumpScanEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.JumpEssentialEvents)) ? JournalEssentialEvents.JumpEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.NoEssentialEvents)) ? JournalEssentialEvents.NoEssentialEvents :
                           (essentialitems == nameof(JournalEssentialEvents.FullStatsEssentialEvents)) ? JournalEssentialEvents.FullStatsEssentialEvents :
                            JournalEssentialEvents.EssentialEvents;

                tabledata = JournalEntry.GetTableData(CurrentCommander,
                    ids: list,
                    allidsafterutc: DateTime.UtcNow.Subtract(new TimeSpan(fullhistoryloaddaylimit, 0, 0, 0)),
                    cancelRequested:cancelRequested
                    );
            }
            else
            {
                tabledata = JournalEntry.GetTableData(CurrentCommander, cancelRequested:cancelRequested);
            }
 
            JournalEntry[] journalentries = new JournalEntry[0];       // default empty array so rest of code works

            if (tabledata != null)          // if not cancelled table read
            {
                Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Journal Creation");

                reportProgress($"Creating {tabledata.Count} Journal Entries");
                var jes = JournalEntry.CreateJournalEntries(tabledata, cancelRequested);
                if (jes != null)        // if not cancelled, use it
                    journalentries = jes;
            }

            tabledata = null;

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Table Clean");

            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();       // to try and lose the tabledata

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Journals read from DB");

            hist.hlastprocessed = null;

            int eno = 0;

            foreach (JournalEntry je in journalentries)
            {
                if (eno++ % 10000 == 0)
                {
                    if (cancelRequested?.Invoke() ?? false)     // if cancelling, stop processing
                        break;

                    reportProgress($"Creating History {eno-1}/{journalentries.Length}");
                }

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

                he.UpdateMaterialsCommodities(hist.MaterialCommoditiesMicroResources.Process(je, hist.hlastprocessed?.journalEntry, he.Status.TravelState == HistoryEntryStatus.TravelStateType.SRV));

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

               // System.Diagnostics.Debug.WriteLine("++ {0} {1}", he.EventTimeUTC.ToString(), he.EntryType);
                var reorderlist = hist.ReorderRemove(he);

                foreach (var heh in reorderlist.EmptyIfNull())
                {
                    // System.Diagnostics.Debug.WriteLine("   ++ {0} {1}", heh.EventTimeUTC.ToString(), heh.EntryType);
                    heh.Index = hist.historylist.Count; // store its index for quick ordering, after all removal etc
                    hist.historylist.Add(heh);        // then add to history
                    hist.AddToVisitsScan(null);  // add to scan database but don't complain
                }
            }

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " History List Created");

            foreach (var s in hist.StarScan.ToProcess) System.Diagnostics.Debug.WriteLine($"StarScan could not find {s.Item2?.Name ?? "FSSSignalDiscovered"} {s.Item2?.SystemAddress} at {s.Item1.EventTimeUTC}");

            //for (int i = hist.Count - 10; i < hist.Count; i++)  System.Diagnostics.Debug.WriteLine("Hist {0} {1} {2}", hist[i].EventTimeUTC, hist[i].Indexno , hist[i].EventSummary);

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

            if (he.EntryType == JournalTypeEnum.Scan)       // may need to do a history match, so intercept
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
            else if (he.EntryType == JournalTypeEnum.SAAScanComplete)       // early entries did not have systemaddress, so need to match
            {
                StarScan.AddSAAScanToBestSystem((JournalSAAScanComplete)he.journalEntry, he.System, pos , historylist);
            }
            else if (he.journalEntry is IStarScan)      // otherwise execute add
            {
                (he.journalEntry as IStarScan).AddStarScan(StarScan,he.System);
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
  * need to check
        Odyssey 5: Trade MR: TradeMicroResource Shiplocker 
        Odyssey 5: Sell MR: SellMicroResource Shiplocker 
*/

        public List<HistoryEntry> ReorderRemove(HistoryEntry he)
        {
            if (EliteConfigInstance.InstanceOptions.DisableMerge)
                return new List<HistoryEntry> { he };

            if ( historylist.Count > 0 )
            {
                // we generally try and remove these as spam if they did not do anything
                if (he.EntryType == JournalTypeEnum.Cargo || he.EntryType == JournalTypeEnum.Materials)       
                {
                    var lasthe = historylist.Last();
                    if ( lasthe.MaterialCommodity != he.MaterialCommodity)  // they changed the mc list, keep
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " Update,keep");
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " No update, remove");
                        return null;
                    }
                }
                // these we try and stop repeats
                else if (he.EntryType == JournalTypeEnum.Outfitting || he.EntryType == JournalTypeEnum.Shipyard )
                {
                    HistoryEntry lasthe = FindBeforeLastDockLoadGameShutdown(1000,he.EntryType);     // don't look back forever
                    if (lasthe != null)
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " Duplicate with " + lasthe.EventTimeUTC.ToString() + " remove");
                        return null;
                    }
                }
                // these we try and stop repeats
                else if (he.EntryType == JournalTypeEnum.EDDCommodityPrices || he.EntryType == JournalTypeEnum.Market)
                {
                    HistoryEntry lasthe = FindBeforeLastDockLoadGameShutdown(1000,JournalTypeEnum.Market, JournalTypeEnum.EDDCommodityPrices);     // don't look back forever
                    if (lasthe != null)
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " Duplicate with " + lasthe.EntryType.ToString() + " " + lasthe.EventTimeUTC.ToString() + " remove");
                        return null;
                    }
                }
            }

            // this stuff only works on journals after Odyssey 5 (1/7/21)
            if (he.EventTimeUTC >= new DateTime(2021, 7, 1))        
            {
                JournalTypeEnum queuetype = reorderqueue.Count > 0 ? reorderqueue[0].EntryType : JournalTypeEnum.Unknown;

                //if ( queuetype != JournalTypeEnum.Unknown) System.Diagnostics.Debug.WriteLine("{0}     Queue {1} Event {2} Count {3}", he.EventTimeUTC.ToString(), queuetype, he.EntryType, reorderqueue.Count);

                // Disembark ship to planet: disembark shiplocker suitloadout backpack  shiplocker
                // Board ship:  embark loadout shiplocker
                // Disembark ship to station: disembark suit loadout backpack  ship locker

                if (he.EntryType == JournalTypeEnum.Embark || he.EntryType == JournalTypeEnum.Disembark)
                {
                    //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
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
                       // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                    }
                }
                else if (he.EntryType == JournalTypeEnum.Loadout)
                {
                    if (queuetype == JournalTypeEnum.Embark)  // if part of this queue
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {
                       // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                    }
                }

                // Part of disembark, or start of SuitLoadout Backpack Ship locker
                else if (he.EntryType == JournalTypeEnum.SuitLoadout)      
                {
                    if (queuetype == JournalTypeEnum.Disembark )
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                        var prevreorder = reorderqueue;
                        reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                        return prevreorder;                                 // if anything queued up, play it out..
                    }
                }
                else if (he.EntryType == JournalTypeEnum.ShipLocker)
                {
                    if ( queuetype == JournalTypeEnum.Disembark )
                    {
                        // Disembark has a possible shiplocker  before suitloadout.  If a suitloadout is there, its the end of the disembark sequence

                        if ( reorderqueue.FindIndex(x=>x.EntryType == JournalTypeEnum.SuitLoadout) >= 0 )       // if we had a suit loadout, its the end
                        {
                            //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} **** End of queue {queuetype}");
                            he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);      // move first entry to here
                            reorderqueue.Clear();
                            return new List<HistoryEntry> { he };
                        }
                        else
                        {
                           // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()}     ignore ship locker {queuetype}");
                            reorderqueue.Add(he);           // in the first part, queue
                            return null;
                        }
                    }

                    // marks the end of these sequences
                    else if ( queuetype == JournalTypeEnum.Embark || queuetype == JournalTypeEnum.SuitLoadout || 
                                queuetype == JournalTypeEnum.Resupply || queuetype == JournalTypeEnum.BackpackChange ||
                                queuetype == JournalTypeEnum.BuyMicroResources || queuetype == JournalTypeEnum.SellMicroResources || queuetype == JournalTypeEnum.TradeMicroResources
                                )  // if part of this queue
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} **** End of queue {queuetype}");
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);      // move first entry to here
                        reorderqueue.Clear();
                        return new List<HistoryEntry> { he };
                    }
                    else
                    {
                       // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                        return null;
                    }
                }

                // Using a consumable: UseConsumable BackpackChange/ Removed.
                // Collect item: CollectItem BackpackChanged/ Added.
                // Drop item: DropItem BackpackChanged/ Removed.
                // Resuppy: Resupply ShipLocker
                // Buy MR: BuyMicroResource ShipLocker
                // TradeMicroResource  Shiplocker 
                // SellMicroResources ShipLocker

                else if (he.EntryType == JournalTypeEnum.CollectItems || he.EntryType == JournalTypeEnum.DropItems || he.EntryType == JournalTypeEnum.UseConsumable ||
                            he.EntryType == JournalTypeEnum.Resupply ||
                            he.EntryType == JournalTypeEnum.BuyMicroResources || he.EntryType == JournalTypeEnum.SellMicroResources || he.EntryType == JournalTypeEnum.TradeMicroResources)
                {
                    //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
                }

                else if (he.EntryType == JournalTypeEnum.BackpackChange)
                {
                    // May be following a UseConsumable, a Collect or Drop

                    var je = he.journalEntry as JournalBackpackChange;

                    if (queuetype == JournalTypeEnum.CollectItems || queuetype == JournalTypeEnum.DropItems || queuetype == JournalTypeEnum.UseConsumable)
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} **** End of queue {queuetype}");
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);
                        reorderqueue.Clear();
                    }

                    // If we are in a backpackchange queue, sum it up. We are in a Backpackchange/Shiplocker transfer to ship sequence

                    else if ( queuetype == JournalTypeEnum.BackpackChange)
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Sum up {he.EntryType}");
                        (reorderqueue[0].journalEntry as JournalBackpackChange).Add(je);      // sum up the BPCs
                        reorderqueue.Add(he);
                        return null;
                    }

                    // if it looks like a throw grenade, we can't distinguish this from a backpack.. shiplocker sequence. Its isolated. 
                    // so must let thru. Thanks frontier for not using useconsumable for throwing grenades.  Otherwise its the start of a BPC queue

                    else if ( je.ThrowGrenade )
                    {
                       // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} throw grenade");
                    }
                    else
                    {           // otherwise, queue it
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                        var prevreorder = reorderqueue;
                        reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                        return prevreorder;                                 // if anything queued up, play it out..
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
                UserDatabase.Instance.DBWrite(cn =>
                {
                    using (DbTransaction txn = cn.BeginTransaction())        // take a transaction over this
                    {
                        foreach (Tuple<HistoryEntry, ISystem> hesys in updatesystems)
                        {
                            logger?.Invoke($"Update position of {hesys.Item1.System.Name} at {hesys.Item1.EntryNumber} in journal");
                            hesys.Item1.journalEntry.UpdateStarPosition(hesys.Item2, cn, txn);
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
