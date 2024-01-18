/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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

//#define LISTSCANS

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("HL {historylist.Count}")]
    public partial class HistoryList
    {
        // Databases accumulated on history
        public MissionListAccumulator MissionListAccumulator { get; private set; } = new MissionListAccumulator(); // and mission list..
        public MaterialCommoditiesMicroResourceList MaterialCommoditiesMicroResources { get; private set; } = new MaterialCommoditiesMicroResourceList();
        public SuitWeaponList WeaponList { get; private set; } = new SuitWeaponList();
        public SuitList SuitList { get; private set; } = new SuitList();
        public SuitLoadoutList SuitLoadoutList { get; private set; } = new SuitLoadoutList();
        public EngineeringList Engineering { get; private set; } = new EngineeringList();
        public CarrierStats Carrier { get; private set; } = new CarrierStats();
        public Ledger CashLedger { get; private set; } = new Ledger();       // and the ledger..
        public ShipInformationList ShipInformationList { get; private set; } = new ShipInformationList();     // ship info
        public ShipYardList Shipyards { get; private set; } = new ShipYardList(); // yards in space (not meters)
        public OutfittingList Outfitting { get; private set; } = new OutfittingList();        // outfitting on stations
        public StarScan StarScan { get; private set; } = new StarScan();      // the results of scanning
        public Dictionary<string, HistoryEntry> Visited { get; private set; } = new Dictionary<string, HistoryEntry>(StringComparer.InvariantCultureIgnoreCase);  // not in any particular order.
        public Dictionary<string, EDStar> StarClass { get; private set; } = new Dictionary<string, EDStar>();     // not in any particular order.
        public Dictionary<string, Stats.FactionInfo> GetStatsAtGeneration(uint g) { return statisticsaccumulator.GetAtGeneration(g); }

        // History variables
        public int CommanderId { get; private set; } = -999;                 // set by history load at end, indicating commander loaded


        // privates
        private List<HistoryEntry> historylist = new List<HistoryEntry>();  // oldest first here
        private Stats statisticsaccumulator = new Stats();

        private HistoryEntry hlastprocessed = null;

        private List<HistoryEntry> reorderqueue = new List<HistoryEntry>();
        private int reorderqueueage = 0;

        public HistoryList() { }

        #region Entry processing

        // Called on a New Entry, by EDDiscoveryController:NewEntry, to make an HE, updating the databases

        public HistoryEntry MakeHistoryEntry(JournalEntry je)
        {
            HistoryEntry he = HistoryEntry.FromJournalEntry(je, hlastprocessed, StarClass);     // we may check edsm for this entry

            he.UnfilteredIndex = (hlastprocessed?.UnfilteredIndex?? -1) +1;
            he.UpdateMaterialsCommodities(MaterialCommoditiesMicroResources.Process(je, hlastprocessed?.journalEntry, he.Status.TravelState == HistoryEntryStatus.TravelStateType.SRV));

            // IN THIS order, so suits can be added, then weapons, then loadouts
            he.UpdateSuits(SuitList.Process(je, he.WhereAmI, he.System));
            he.UpdateWeapons(WeaponList.Process(je, he.WhereAmI, he.System));
            he.UpdateLoadouts(SuitLoadoutList.Process(je, WeaponList, he.WhereAmI, he.System));

            he.UpdateStats(je, statisticsaccumulator, he.Status.StationFaction);

            CashLedger.Process(je);
            he.Credits = CashLedger.CashTotal;
            he.Loan = CashLedger.Loan;
            he.Assets = CashLedger.Assets;

            Shipyards.Process(je);
            Outfitting.Process(je);

            Carrier.Process(je,he.Status.OnFootFleetCarrier);

            Tuple<ShipInformation, ModulesInStore> ret = ShipInformationList.Process(je, he.WhereAmI, he.System);
            he.UpdateShipInformation(ret.Item1);
            he.UpdateShipStoredModules(ret.Item2);

            he.UpdateMissionList(MissionListAccumulator.Process(je, he.System, he.WhereAmI));

            he.UpdateEngineering(Engineering.Process(he));

            he.UpdateTravelStatus(hlastprocessed);

            Identifiers.Process(je);

            hlastprocessed = he;

            return he;
        }

        // Called with the HE above to perform reorder removal and add to HL, returns list of HLs added

        public List<HistoryEntry> AddHistoryEntryToListWithReorder(HistoryEntry he, Action<string> logerror)   
        {
            var reorderlist = ReorderRemove(he);

            foreach(var heh in reorderlist.EmptyIfNull())
            {
                heh.Index = historylist.Count;  // store its index
                historylist.Add(heh);        // then add to history
                AddToVisitsScan(logerror);  // add to scan database and complain if can't add. Do this after history add, so it has a list.
            }

            return reorderlist;
        }

        // History load, read DB for entries and add entries to passed in history
        // commanderid is table searched, cmdname is for reporting only
        // if fullhistoryloaddaylimit >0 (days before today), then load ids[] before time, afterwards all items are loaded
        // if maxdateload set, only load up to that date
        
        public static void LoadHistory( HistoryList hist, 
                                        Action<int,string> reportProgress, Func<bool> cancelRequested,
                                        int commanderid, string cmdname, 
                                        int fullhistoryloaddaylimit, JournalTypeEnum[] loadedbeforelimitids, 
                                        DateTime? maxdateload
                                        )
        {

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL", true).Item1 + $" History Load of {commanderid} {cmdname} {fullhistoryloaddaylimit} {maxdateload??DateTime.MinValue}");

            reportProgress(-1,$"Reading Cmdr. {cmdname} database records");

            List<JournalEntry.TableData> tabledata;

            if (fullhistoryloaddaylimit > 0)            // if we are limiting 
            {
                tabledata = JournalEntry.GetTableData(commanderid, enddateutc:maxdateload,
                    ids: loadedbeforelimitids, allidsafterutc: DateTime.UtcNow.Subtract(new TimeSpan(fullhistoryloaddaylimit, 0, 0, 0)),
                    cancelRequested:cancelRequested
                    );
            }
            else
            {
                tabledata = JournalEntry.GetTableData(commanderid, cancelRequested:cancelRequested, enddateutc:maxdateload);
            }
 
            JournalEntry[] journalentries = new JournalEntry[0];       // default empty array so rest of code works

            // Create the journal entries from the table data, MTing if needed

            if (tabledata != null)          // if not cancelled table read
            {
                Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + $" Journal Creation of {tabledata.Count}");

                reportProgress(-1,$"Creating Cmdr. {cmdname} {tabledata.Count.ToString("N0")} journal entries");

                var jes = JournalEntry.CreateJournalEntries(tabledata, cancelRequested);
                if (jes != null)        // if not cancelled, use it
                    journalentries = jes;
            }

            tabledata = null;

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + $" Table Clean {journalentries.Length} records");

            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();       // to try and lose the tabledata

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + " Journals read from DB");

            int eno = 0;

            foreach (JournalEntry je in journalentries)
            {
                if (eno++ % 5000 == 0 || eno == journalentries.Length-1)
                {
                    if (cancelRequested?.Invoke() ?? false)     // if cancelling, stop processing
                        break;

                    reportProgress(100*eno/journalentries.Length, $"Creating Cmdr. {cmdname} history {(eno-1).ToString("N0")}/{journalentries.Length.ToString("N0")}");
                }

                if (MergeJournalEntries(hist.hlastprocessed?.journalEntry, je))        // if we merge, don't store into HE
                {
                    continue;
                }

                // Clean up "UnKnown" systems from EDSM log
                if (je is JournalFSDJump && ((JournalFSDJump)je).StarSystem == "UnKnown")
                {
                    JournalEntry.Delete(je.Id);
                    continue;
                }

                HistoryEntry hecur = hist.MakeHistoryEntry(je);

                // System.Diagnostics.Debug.WriteLine("++ {0} {1}", he.EventTimeUTC.ToString(), he.EntryType);
                var reorderlist = hist.ReorderRemove(hecur);

                foreach (var heh in reorderlist.EmptyIfNull())
                {
                    heh.journalEntry.SetSystemNote();                // since we are displaying it, we can check here to see if a system note needs assigning

                    //if ( heh.EventTimeUTC > new DateTime(2021,8,1)) System.Diagnostics.Debug.WriteLine("   ++ {0} {1}", heh.EventTimeUTC.ToString(), heh.EntryType);

                    heh.Index = hist.historylist.Count; // store its index for quick ordering, after all removal etc

                    hist.historylist.Add(heh);        // then add to history
                    hist.AddToVisitsScan(null);  // add to scan database but don't complain
                    //System.Diagnostics.Debug.WriteLine($"Add {heh.EventTimeUTC} {heh.EntryType} {hist.StarScan.ScanDataByName.Count} {hist.Visited.Count}");
                }
            }

            hist.Carrier.CheckCarrierJump(DateTime.UtcNow);         // lets see if a jump has completed.

            Trace.WriteLine(BaseUtils.AppTicks.TickCountLapDelta("HLL").Item1 + $" History List Created {hist.Count}");

            foreach (var s in hist.StarScan.ToProcess)
            {
                s.Item1.FillInformation(out string info, out string detailed);
                System.Diagnostics.Debug.WriteLine($"StarScan could not assign {s.Item1.EventTimeUTC} {s.Item1.GetType().Name} {info} {s.Item2?.Name ?? "???"} {s.Item2?.SystemAddress}");
            }

            // foreach (var kvp in hist.IdentifierList.Items) System.Diagnostics.Debug.WriteLine($"IDList {kvp.Key} -> {kvp.Value}"); // debug

            //for (int i = hist.Count - 10; i < hist.Count; i++)  System.Diagnostics.Debug.WriteLine("Hist {0} {1} {2}", hist[i].EventTimeUTC, hist[i].Indexno , hist[i].EventSummary);

            hist.CommanderId = commanderid;        // last thing, and this indicates history is loaded.

            // now, with a history, we set some globals used by EDSM/EDDN for sending or creating new entries, in the same style as the last good TLU entry

            for( int entry = hist.Count-1; entry>=0; entry--)
            {
                if (hist[entry].journalEntry.IsJournalSourced )      // if we have a journal record
                {
                    JournalEntry.DefaultHorizonsFlag = hist[entry].journalEntry.IsHorizons;
                    JournalEntry.DefaultOdysseyFlag = hist[entry].journalEntry.IsOdyssey;
                    JournalEntry.DefaultBetaFlag = hist[entry].journalEntry.IsBeta;
                    break;
                }
            }

            //foreach( var kvp in hist.StarScan.ScanDataByName) if (kvp.Value.System.SystemAddress == null) System.Diagnostics.Debug.WriteLine($"{kvp.Value.System.Name} no SA");

            //foreach (var x in Identifiers.Items) System.Diagnostics.Debug.WriteLine($"Identifiers {x.Key} = {x.Value}");


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
        }

        private void AddToVisitsScan(Action<string> logerror)
        {
            HistoryEntry he = GetLast;

            if ((LastSystem == null || he.System.Name != LastSystem ) && he.System.Name != "Unknown" )   // if system is not last, we moved somehow (FSD, location, carrier jump), add
            {
                if (Visited.TryGetValue(he.System.Name, out var value))
                {
                    he.UpdateVisits(value.Visits + 1);               // visits is 1 more than previous entry
                    Visited[he.System.Name] = he;          // reset to point to latest he
                }
                else
                {
                    he.UpdateVisits(1);               // visits is 1 more than previous entry
                    Visited[he.System.Name] = he;          // point to he
                }

                LastSystem = he.System.Name;
            }

            int pos = historylist.Count - 1;                // current entry index

            if (he.EntryType == JournalTypeEnum.StartJump)
            {
                var stj = he.journalEntry as JournalStartJump;
                if (stj.EDStarClass != EDStar.Unknown)
                {
                    StarClass[stj.StarSystem] = stj.EDStarClass;
                   // System.Diagnostics.Debug.WriteLine($"Star Jump gives star {stj.StarSystem} as class {stj.EDStarClass}");
                }
            }

            if (he.EntryType == JournalTypeEnum.Scan)       // may need to do a history match, so intercept
            {
                JournalScan js = he.journalEntry as JournalScan;

                if (!StarScan.AddScanToBestSystem(js, pos - 1, historylist, out HistoryEntry jlhe, out JournalLocOrJump jl))
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
                StarScan.AddSAAScanToBestSystem((JournalSAAScanComplete)he.journalEntry, he.System, pos, historylist);
            }
            else if (he.journalEntry is IStarScan)      // a star scan type takes precendence
            {
                (he.journalEntry as IStarScan).AddStarScan(StarScan, he.System);
            }
            else if (he.journalEntry is IBodyNameAndID je)  // all entries under this type
            {
                if (he.EntryType == JournalTypeEnum.ApproachSettlement)    // we need to process this uniquely, as it has information for starscan as well as body info
                {
                    JournalApproachSettlement jas = he.journalEntry as JournalApproachSettlement;

                    // only interested in some in same system with system addr and bodyids, and we can then set the system name (missing from the event) so the AddBodyworks

                    if (jas.Body.HasChars() && jas.BodyID.HasValue && he.System.SystemAddress.HasValue && he.System.SystemAddress == jas.SystemAddress )
                    {
                        jas.StarSystem = he.System.Name;           // fill in the missing system name 
                        StarScan.AddBodyToBestSystem(je, he.System, pos, historylist); // ensure its in the DB - note BodyType = Settlement
                        StarScan.AddApproachSettlement(jas, he.System);
                    }
                    else
                    {
                        if (je.BodyID.HasValue) System.Diagnostics.Debug.WriteLine($"Starscan approach rejected {he.EventTimeUTC} {he.System.Name} {jas.BodyID} {jas.Body} {jas.Name} {jas.Name_Localised} {jas.Latitude} {jas.Longitude}");
                    }
                }
                else if (he.EntryType == JournalTypeEnum.Touchdown)    // we need to process this uniquely, as it has information for starscan as well as body info
                {
                    JournalTouchdown jt = he.journalEntry as JournalTouchdown;

                    // only interested in some in same system with system addr and bodyids, and we can then set the system name (missing from the event) so the AddBodyworks

                    if (jt.Body.HasChars() && jt.BodyID.HasValue && he.System.SystemAddress.HasValue && he.System.SystemAddress == jt.SystemAddress && 
                                    jt.HasLatLong )  // some touchdowns don't come with lat/long, ignore them, they are not important
                    {
                        jt.StarSystem = he.System.Name;           // fill in the possibly missing system name 
                        StarScan.AddBodyToBestSystem(jt, he.System, pos, historylist); // ensure its in the DB - note BodyType = Settlement
                        StarScan.AddTouchdown(jt, he.System);
                    }
                    else
                    {
                        if (jt.BodyID.HasValue) System.Diagnostics.Debug.WriteLine($"Starscan touchdown rejected {he.EventTimeUTC} {he.System.Name} {jt.BodyID} {jt.Body} {jt.Latitude} {jt.Longitude}");
                    }
                }
                else if (je.Body.HasChars())    // we can add here with bodyid = null, so just check body
                {
                    // only these have a Body of a planet/star/barycentre, the other types (station etc see the frontier doc which is right for a change) are not useful

                    if (je.BodyType.EqualsIIC("Planet") || je.BodyType.EqualsIIC("Star") || je.BodyType.EqualsIIC("Barycentre"))        
                    {
                        StarScan.AddBodyToBestSystem(je, he.System, pos, historylist);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Starscan bodyid rejected {he.EventTimeUTC} {he.System.Name} {je.BodyID} {je.Body} {je.BodyType}");
                }
            }
        }

        // 0 = no delay, delay for attempts to merge items..  used by new entry mech only, not by historylist
        public static int MergeTypeDelayForJournalEntries(JournalEntry je)   
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

        // this allows journal entries to be merged into one before being made into a history entry
        // true if to discard.  
        public static bool MergeJournalEntries(JournalEntry prev, JournalEntry je)
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
                        if (jdprev.Signals[0].SystemAddress == jd.Signals[0].SystemAddress)     // only if same system address
                        {
                            jdprev.Add(jd);
                            return true;
                        }
                    }
                    else if (je.EventTypeID == JournalTypeEnum.ShipTargeted)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalShipTargeted;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalShipTargeted;
                        jdprev.Add(jd);
                        return true;
                    }
                    else if (je.EventTypeID == JournalTypeEnum.FSSAllBodiesFound)
                    {
                        var jdprev = prev as EliteDangerousCore.JournalEvents.JournalFSSAllBodiesFound;
                        var jd = je as EliteDangerousCore.JournalEvents.JournalFSSAllBodiesFound;
                        if ( jdprev.SystemAddress == jd.SystemAddress && jdprev.Count == jd.Count)          // same system, repeat, remove.  seen instances of this
                        {
                          //  System.Diagnostics.Debug.WriteLine("Duplicate FSSAllBodiesFound **********");
                            return true;
                        }
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
   
                }
            }

            return false;
        }

        private List<HistoryEntry> ReorderRemove(HistoryEntry he)
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
                // these we try and stop repeats by not allowing more than one after docking
                else if (he.EntryType == JournalTypeEnum.Outfitting || he.EntryType == JournalTypeEnum.Shipyard || 
                                he.EntryType == JournalTypeEnum.StoredShips || he.EntryType == JournalTypeEnum.StoredModules)
                {
                    HistoryEntry lasthe = FindBeforeLastDockLoadGameShutdown(1000,he.EntryType);     // don't look back forever
                    if (lasthe != null)
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " Duplicate with " + lasthe.EventTimeUTC.ToString() + " remove");
                        return null;
                    }
                }
                // these we try and stop repeats by not allowing more than one after docking
                else if (he.EntryType == JournalTypeEnum.EDDCommodityPrices || he.EntryType == JournalTypeEnum.Market)
                {
                    HistoryEntry lasthe = FindBeforeLastDockLoadGameShutdown(1000, JournalTypeEnum.Market, JournalTypeEnum.EDDCommodityPrices);     // don't look back forever
                    if (lasthe != null)
                    {
                        //System.Diagnostics.Debug.WriteLine(he.EventTimeUTC.ToString() + " " + he.EntryType.ToString() + " Duplicate with " + lasthe.EntryType.ToString() + " " + lasthe.EventTimeUTC.ToString() + " remove");
                        return null;
                    }
                }
                else if ( he.EntryType == JournalTypeEnum.NavRoute)
                {
                    HistoryEntry lasthe = FindBeforeLastDockLoadGameShutdown(1000, he.EntryType);     // don't look back forever, back to last dock
                    if ( lasthe != null )
                    {
                        JournalNavRoute cur= he.journalEntry as JournalNavRoute;
                        JournalNavRoute last = lasthe.journalEntry as JournalNavRoute;
                        if (cur.Route != null && last.Route != null && cur.Equals(last))    // see if routes are the same, if so, no point putting it in again
                        {
                            System.Diagnostics.Debug.WriteLine("Remove repeat nav route");
                            return null;
                        }
                    }
                }
            }

            // this stuff only works on journals after Odyssey 5 (1/7/21)
            if (he.EventTimeUTC >= EliteReleaseDates.Odyssey5)
            {
                JournalTypeEnum queuetype = reorderqueue.Count > 0 ? reorderqueue[0].EntryType : JournalTypeEnum.Unknown;
                reorderqueueage++;      // only used for some re-order queue types

                //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Event {he.EntryType} Queuetype {queuetype} # {reorderqueue.Count}");

                // used for these types of sequences:
                //
                // Disembark ship to planet:    disembark shiplocker suitloadout backpack  shiplocker
                // Board ship:                  embark loadout shiplocker
                // Disembark ship to station:   disembark suit loadout backpack  ship locker
                // Disembark srv to foot:       disembark shiplocker suitloadout backpack 
                // Suitloader:                  SuitLoadout Backpack Ship locker
                // Using a consumable:          UseConsumable BackpackChange/ Removed.
                // Collect item:                CollectItem BackpackChanged/ Added.
                // Drop item:                   DropItem BackpackChanged/ Removed.
                // Resuppy:                     Resupply ShipLocker
                // Buy MR:                      BuyMicroResource ShipLocker
                // Trade MR:                    TradeMicroResource  Shiplocker 
                // Sell MR:                     SellMicroResources ShipLocker
                // Backpack change:             Backpackchange [backpackchange] shiplocker 

                // embark/disembark start sequence
                if (he.EntryType == JournalTypeEnum.Embark || he.EntryType == JournalTypeEnum.Disembark)
                {
                    //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
                }

                // can be part of queues disembark or suitloader. If isolated, just report
                else if (he.EntryType == JournalTypeEnum.Backpack)
                {
                    if (queuetype == JournalTypeEnum.Disembark || queuetype == JournalTypeEnum.SuitLoadout)
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {       // isolated just report
                            // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                    }
                }

                // can be part of queues embark, if isolated, just report
                else if (he.EntryType == JournalTypeEnum.Loadout)
                {
                    if (queuetype == JournalTypeEnum.Embark)  // if part of this queue
                    {
                        reorderqueue.Add(he);
                        return null;
                    }
                    else
                    {       // isolated, just report
                            // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                    }
                }

                // suitloadout start sequence, or part of disembark
                else if (he.EntryType == JournalTypeEnum.SuitLoadout)
                {
                    if (queuetype == JournalTypeEnum.Disembark)
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

                // shiplocker ends lots of sequences, and can be in the middle or end of disembark

                else if (he.EntryType == JournalTypeEnum.ShipLocker)
                {
                    // can be the end of disembark if previous suitloader is there..

                    if (queuetype == JournalTypeEnum.Disembark)
                    {
                        // Disembark has a possible shiplocker  before suitloadout.  If a suitloadout is there, its the end of the disembark sequence

                        if (reorderqueue.FindIndex(x => x.EntryType == JournalTypeEnum.SuitLoadout) >= 0)
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

                    // shiplocker marks the end of these sequences

                    else if (queuetype == JournalTypeEnum.Embark || queuetype == JournalTypeEnum.SuitLoadout ||
                                queuetype == JournalTypeEnum.Resupply || queuetype == JournalTypeEnum.BackpackChange ||
                                queuetype == JournalTypeEnum.BuyMicroResources || queuetype == JournalTypeEnum.SellMicroResources || queuetype == JournalTypeEnum.TradeMicroResources
                                )
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} **** End of queue {queuetype}");
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);      // move first entry to here
                        reorderqueue.Clear();
                        return new List<HistoryEntry> { he };
                    }
                    else
                    {
                        // isolated, just remove
                        // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Isolated {he.EntryType}");
                        return null;
                    }
                }

                // these start a sequence
                else if (he.EntryType == JournalTypeEnum.CollectItems || he.EntryType == JournalTypeEnum.DropItems || he.EntryType == JournalTypeEnum.UseConsumable ||
                            he.EntryType == JournalTypeEnum.Resupply ||
                            he.EntryType == JournalTypeEnum.BuyMicroResources || he.EntryType == JournalTypeEnum.SellMicroResources
                            || he.EntryType == JournalTypeEnum.TradeMicroResources)
                {
                    // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    return prevreorder;                                 // if anything queued up, play it out..
                }

                // this ends some sequences
                else if (he.EntryType == JournalTypeEnum.BackpackChange)
                {
                    // if following a collect, drop, use then it ends the sequence

                    var je = he.journalEntry as JournalBackpackChange;

                    if (queuetype == JournalTypeEnum.CollectItems || queuetype == JournalTypeEnum.DropItems || queuetype == JournalTypeEnum.UseConsumable)
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} **** End of queue {queuetype}");
                        he.ReplaceJournalEntry(reorderqueue[0].journalEntry, he.EventTimeUTC);
                        reorderqueue.Clear();       // then return he
                    }

                    // If we are in a backpackchange queue, sum it up. We are in a Backpackchange/Shiplocker transfer to ship sequence

                    else if (queuetype == JournalTypeEnum.BackpackChange)
                    {
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Sum up {he.EntryType}");
                        (reorderqueue[0].journalEntry as JournalBackpackChange).Add(je);      // sum up the BPCs
                        reorderqueue.Add(he);
                        return null;
                    }

                    // if it looks like a throw grenade, we can't distinguish this from a backpack.. shiplocker sequence. Its isolated. 
                    // so must let thru. Thanks frontier for not using useconsumable for throwing grenades. 

                    else if (je.ThrowGrenade)
                    {
                        // System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} throw grenade");
                    }

                    // Otherwise its the start of a backpackchange queue
                    else
                    {           // otherwise, queue it
                        //System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC.ToString()} Start of queue {he.EntryType}");
                        var prevreorder = reorderqueue;
                        reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                        return prevreorder;                                 // if anything queued up, play it out..
                    }
                }

                /// Update 14 of Odyssey

                // destination drops are stored onto the reorder queue and applied to supercruiseexit

                if (he.EntryType == JournalTypeEnum.SupercruiseDestinationDrop)
                {
                    var prevreorder = reorderqueue;
                    reorderqueue = new List<HistoryEntry> { he };       // reset the queue, and run with it
                    reorderqueueage = 0;                                // use this to stop it pending for ever
                    return prevreorder;                                 // if anything queued up, play it out..
                }

                else if (queuetype == JournalTypeEnum.SupercruiseDestinationDrop)
                {
                    // with a supercruise exit, and a destination drop reorder, store the drop in the event
                    if (he.EntryType == JournalTypeEnum.SupercruiseExit)             
                    {
                        (he.journalEntry as JournalSupercruiseExit).DestinationDrop = reorderqueue[0].journalEntry as JournalSupercruiseDestinationDrop;
                        reorderqueue.Clear();
                    }
                    else if (reorderqueueage >= 10)                     // we age 1 per event, so allowing for music/receive text crap, if its too old, abandon
                    {
                        reorderqueue.Clear();
                    }
                }
            }

            // we can issue this He, not part of a sequence, with the re-order active, presuming the next entries will complete the reorder

            return new List<HistoryEntry> { he };       // pass it through
        }

        #endregion

    }
}
