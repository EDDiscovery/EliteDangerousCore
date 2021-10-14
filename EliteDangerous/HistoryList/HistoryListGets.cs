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
using System.Linq;

namespace EliteDangerousCore
{
    public partial class HistoryList //: IEnumerable<HistoryEntry>
    {
        #region List access

        public Ledger CashLedger { get; private set; } = new Ledger();       // and the ledger..
        public ShipInformationList ShipInformationList { get; private set; } = new ShipInformationList();     // ship info
        public ShipYardList Shipyards = new ShipYardList(); // yards in space (not meters)
        public OutfittingList Outfitting = new OutfittingList();        // outfitting on stations
        public StarScan StarScan { get; private set; } = new StarScan();      // the results of scanning
        public int CommanderId { get; private set; }
        public Dictionary<string, HistoryEntry> Visited { get; private set; } = new Dictionary<string, HistoryEntry>();  // not in any particular order.
        public string LastSystem { get; private set; }                          // last system seen in

        public int Count { get { return historylist.Count; } }

        public int Visits(string name) { return Visited.TryGetValue(name, out var res) ? res.Visits : 0; }
        public int VisitedSystemsCount { get { return Visited.Count; } }

        // oldest first
        public List<HistoryEntry> EntryOrder() { return historylist; }
        public HistoryEntry this[int i] { get {return historylist[i]; } }
        public HistoryEntry LastOrDefault { get { return historylist.LastOrDefault(); } }

        // combat, spanel,stats, history filter
        public List<HistoryEntry> LatestFirst() { return historylist.OrderByDescending(s => s.EntryNumber).ToList(); }

        public Dictionary<string, Stats.FactionInfo> GetStatsAtGeneration(uint g)
        {
            return statisticsaccumulator.GetAtGeneration(g);
        }

        #endregion

        #region Output filters

        // history filter
        static public List<HistoryEntry> LatestFirst(List<HistoryEntry> list) // list should be in entry order (oldest first)
        {
            return Enumerable.Reverse(list).ToList();
        }

        // history filter
        static public List<HistoryEntry> LatestFirstLimitNumber(List<HistoryEntry> list, int max) // list should be in entry order (oldest first)
        {
            return Enumerable.Reverse(list).Take(max).ToList();
        }

        // history filter
        static public List<HistoryEntry> LatestFirstLimitByDate(List<HistoryEntry> list, TimeSpan days)     // list should be in entry order (oldest first)
        {
            var oldestData = DateTime.UtcNow.Subtract(days);
            return Enumerable.Reverse(list.Where(x => x.EventTimeUTC >= oldestData)).ToList();
        }

        // history filter List should be in entry order.
        static public List<HistoryEntry> LatestFirstStartStopFlags(List<HistoryEntry> list)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();
            bool started = false;
            foreach (HistoryEntry he in list)   // in entry order, last first
            {
                if (he.StartMarker)
                {
                    started = true;
                    entries.Add(he);
                }
                else if (started)
                {
                    entries.Add(he);
                    if (he.StopMarker && !he.StartMarker)
                        started = false;
                }
            }
            return Enumerable.Reverse(entries).ToList();
        }

        // history filter, combat panel.  List should be in entry order.
        static public List<HistoryEntry> LatestFirstToLastDock(List<HistoryEntry> list)
        {
            int lastdock = list.FindLastIndex(x => !x.MultiPlayer && x.EntryType == JournalTypeEnum.Docked);
            if (lastdock >= 0)
            {
                List<HistoryEntry> tolastdock = new List<HistoryEntry>();
                for (int i = list.Count - 1; i >= lastdock; i--)     // go backwards so in lastest first order
                    tolastdock.Add(list[i]);
                return tolastdock;
            }
            else
                return new List<HistoryEntry>();
        }

        // mining overlay, List should be in entry order.
        static public List<HistoryEntry> FilterBetween(List<HistoryEntry> list, HistoryEntry pos, Predicate<HistoryEntry> boundcondition, Predicate<HistoryEntry> where, out HistoryEntry hebelow, out HistoryEntry heabove)
        {
            heabove = GetFromPos(list, pos, boundcondition, 1, returnlast: true);     // find entry later matching bound conditions
            hebelow = GetFromPos(list, pos, boundcondition, -1, usecurrent: true, returnlast: true);     // find entry before, including ourselves, matching bound conditions

            if (heabove != null && hebelow != null)
            {
                int hebelowix = hebelow.EntryNumber;
                int heaboveix = heabove.EntryNumber;
                return list.Where(x => x.EntryNumber >= hebelowix && x.EntryNumber <= heaboveix && where(x) == true).ToList();
            }
            else
                return null;
        }

        // factions, List should be in entry order.
        static public List<HistoryEntry> FilterBefore(List<HistoryEntry> list, HistoryEntry pos, Predicate<HistoryEntry> where)         // from and including pos, go back in time, and return all which match where
        {
            int indexno = pos.EntryNumber-1;        // position in HElist..
            List<HistoryEntry> helist = new List<HistoryEntry>();
            while (indexno >= 0 && indexno < list.Count)        // check within range.
            {
                if (where(list[indexno]))
                {
                    System.Diagnostics.Debug.WriteLine(list[indexno].EntryType + " " + list[indexno].StationFaction);
                    helist.Add(list[indexno]);
                }
                indexno--;
            }

            return helist;
        }

        // factions. List should be in entry order.
        static public List<HistoryEntry> FilterByDateRange(List<HistoryEntry> list, DateTime startutc, DateTime endutc) // UTC! in time ascending order
        {
            return list.Where(s => s.EventTimeUTC >= startutc && s.EventTimeUTC <= endutc).ToList();
        }

        // combat panel, List should be in entry order.
        static public List<HistoryEntry> FilterByDateRangeLatestFirst(List<HistoryEntry> list, DateTime startutc, DateTime endutc) // UTC! in time ascending order
        {
            return list.Where(s => s.EventTimeUTC >= startutc && s.EventTimeUTC <= endutc).OrderByDescending(s => s.EventTimeUTC).ToList();
        }

        // discoveryform menu item
        static public List<HistoryEntry> FilterByScanNotEDDNSynced(List<HistoryEntry> list)
        {
            return (from s in list where s.EDDNSync == false && s.EntryType == JournalTypeEnum.Scan orderby s.EventTimeUTC ascending select s).ToList();
        }

        // 2dmap
        static public List<HistoryEntry> FilterByFSDCarrierJumpAndPosition(List<HistoryEntry> list)
        {
            return (from s in list where s.IsFSDCarrierJump && s.System.HasCoordinate select s).ToList();
        }

        // market data
        static public List<HistoryEntry> FilterByCommodityPricesBackwards(List<HistoryEntry> list)      // full commodity price information only
        {
            return (from s in list
                    where (s.journalEntry is JournalCommodityPricesBase && (s.journalEntry as JournalCommodityPricesBase).Commodities.Count > 0)
                    orderby s.EventTimeUTC descending
                    select s).ToList();
        }

        // trilat/trippanel
        public List<HistoryEntry> FilterByFSDOnly() { return (from s in historylist where s.EntryType == JournalTypeEnum.FSDJump select s).ToList(); }

        // search scans
        public List<HistoryEntry> FilterByScan() { return (from s in historylist where s.journalEntry.EventTypeID == JournalTypeEnum.Scan select s).ToList(); }

        // used by travel, spanel, journal to filter out by journal type
        public static List<HistoryEntry> FilterByJournalEvent(List<HistoryEntry> he, string eventstring, out int count)
        {
            count = 0;
            if (eventstring.Equals("All"))
                return he;
            else
            {
                string[] events = eventstring.Split(';');
                List<HistoryEntry> ret = (from systems in he where systems.IsJournalEventInEventFilter(events) select systems).ToList();
                count = he.Count - ret.Count;
                return ret;
            }
        }

        #endregion

        #region Status

        public HistoryEntryStatus.TravelStateType CurrentTravelState() { HistoryEntry he = GetLast; return (he != null) ? he.TravelState : HistoryEntryStatus.TravelStateType.Unknown; }     //safe methods
        public ISystem CurrentSystem() { HistoryEntry he = GetLast; return (he != null) ? he.System : null; }  // current system

        public double DistanceCurrentTo(string system)          // from current, if we have one, to system, if its found.
        {
            ISystem cursys = CurrentSystem();
            ISystem other = FindSystem(system, null, false);    // does not use EDSM for this, just DB and history
            return cursys != null ? cursys.Distance(other) : -1;  // current can be null, shipsystem can be null, cursys can not have co-ord, -1 if failed.
        }

        #endregion

        #region Gets

        public HistoryEntry GetByJID(long jid)
        {
            return historylist.Find(x => x.Journalid == jid);
        }

        public HistoryEntry GetByEntryNo(int entryno)
        {
            return (entryno >= 1 && entryno <= historylist.Count) ? historylist[entryno - 1] : null;
        }

        public int GetIndex(long jid)
        {
            return historylist.FindIndex(x => x.Journalid == jid);
        }

        public HistoryEntry GetLast
        {
            get
            {
                if (historylist.Count > 0)
                    return historylist[historylist.Count - 1];
                else
                    return null;
            }
        }

        // from athe, find where in a direction..
        // dir = 1 forward to newer, -1 backwards.  usecurrent means check athe entry.  returnlast means if we did not find it, return first or last
        // null if not found, or no athe, or no list
        static public HistoryEntry GetFromPos(List<HistoryEntry> list, HistoryEntry athe, Predicate<HistoryEntry> where, int dir = 1, 
                                            bool usecurrent = false, bool returnlast = false)      
        {
            if (list.Count == 0 || athe == null)
                return null;

            for (int i = (athe.EntryNumber - 1) + (usecurrent ? 0 : dir); dir > 0 ? (i < list.Count) : (i >= 0 && i < list.Count); i += dir )        // indexno is +1 due to historic reasons, start from one on
            {
                if (where(list[i]))
                    return list[i];
            }

            if (returnlast)
                return dir > 0 ? list.Last() : list.First();
            else
                return null;
        }

        //tracker,notepanel
        public HistoryEntry GetLastFSDCarrierJump() { return historylist.FindLast(x => x.IsFSDCarrierJump); }

        // trilat
        public HistoryEntry GetLastFSDOnly() { return historylist.FindLast(x => x.EntryType == JournalTypeEnum.FSDJump); }

        // trippanel
        public HistoryEntry GetLastHistoryEntry(Predicate<HistoryEntry> where)
        {
            return historylist.FindLast(where);
        }

        // spanel, outfitting, shipyards, marketdata, sysinfo
        public HistoryEntry GetLastHistoryEntry(Predicate<HistoryEntry> where, HistoryEntry frominclusive)
        {
            if (frominclusive is null)
                return GetLastHistoryEntry(where);
            else 
            {
                for (int index = frominclusive.EntryNumber - 1; index >= 0 && index < historylist.Count; index--)
                {
                    if (where(historylist[index]))
                        return historylist[index];
                }

                return null;
            }
        }

        // sysinfo, discoveryform
        public HistoryEntry GetLastWithPosition() { return historylist.FindLast(x => x.System.HasCoordinate); }

        // everywhere
        public int GetVisitsCount(string name)
        {
            if (Visited.TryGetValue(name, out var he))
                return he.Visits;
            else
                return 0;
        }

        //stats
        public bool IsTravellingUTC(out DateTime startTimeutc)
        {
            bool inTrip = false;
            startTimeutc = DateTime.UtcNow;
            HistoryEntry lastStartMark = GetLastHistoryEntry(x => x.StartMarker);
            if (lastStartMark != null)
            {
                HistoryEntry lastStopMark = GetLastHistoryEntry(x => x.StopMarker);
                inTrip = lastStopMark == null || lastStopMark.EventTimeUTC < lastStartMark.EventTimeUTC;
                if (inTrip)
                    startTimeutc = lastStartMark.EventTimeUTC;
            }
            return inTrip;
        }

       // historylist
        public string GetCommanderFID()     // may be null
        {
            var cmdr = historylist.FindLast(x => x.EntryType == JournalTypeEnum.Commander);
            return (cmdr?.journalEntry as JournalCommander)?.FID;
        }

 
        // map3d
        public static HistoryEntry FindLastKnownPosition(List<HistoryEntry> syslist)        // can return FSD, Carrier or Location
        {
            return syslist.FindLast(x => x.System.HasCoordinate && x.IsLocOrJump);
        }

        // map3d
        public static HistoryEntry FindByPos(List<HistoryEntry> syslist, float x, float y, float z, double limit)     // go thru setting the lastknowsystem
        {
            return syslist.FindLast(s => s.System.HasCoordinate &&
                                            Math.Abs(s.System.X - x) < limit &&
                                            Math.Abs(s.System.Y - y) < limit &&
                                            Math.Abs(s.System.Z - z) < limit);
        }

        // map3d
        public List<HistoryEntry> FilterByTravelTime(DateTime? starttimeutc, DateTime? endtimeutc)        // filter, in its own order. return FSD,carrier and location events after death
        {
            List<HistoryEntry> ents = new List<HistoryEntry>();
            string lastsystem = null;
            foreach (HistoryEntry he in historylist)        // in add order, oldest first
            {
                if ((starttimeutc == null || he.EventTimeUTC >= starttimeutc) && (endtimeutc == null || he.EventTimeUTC <= endtimeutc))
                {
                    if (he.EntryType == JournalTypeEnum.Location || he.EntryType == JournalTypeEnum.CarrierJump || he.EntryType == JournalTypeEnum.FSDJump)
                    {
                        if (lastsystem != he.System.Name)
                        {
                            ents.Add(he);
                            lastsystem = he.System.Name;
                          //  System.Diagnostics.Debug.WriteLine($"TH {he.EventTimeUTC} {he.System.Name}");
                        }
                        else
                        {
                          //  System.Diagnostics.Debug.WriteLine($"Reject {he.EventTimeUTC} {he.System.Name}");
                        }
                    }
                }
            }

            return ents;
        }



        // findsystemusercontrol
        public static IEnumerable<ISystem> FindSystemsWithinLy(List<HistoryEntry> he, ISystem centre, double minrad, double maxrad, bool spherical)
        {
            IEnumerable<ISystem> list;

            if (spherical)
                list = (from x in he where x.System.HasCoordinate && x.System.Distance(centre, minrad, maxrad) select x.System);
            else
                list = (from x in he where x.System.HasCoordinate && x.System.Cuboid(centre, minrad, maxrad) select x.System);

            return list.GroupBy(x => x.Name).Select(group => group.First());
        }

        public HistoryEntry FindBeforeLastDockLoadGameShutdown( int depthback, params JournalTypeEnum[] e)
        {
            for( int i = historylist.Count-1; i>=0 && depthback-->0; i--)
            {
                if (Array.IndexOf(e,historylist[i].EntryType) >= 0)     // if found..
                    return historylist[i];

                if (historylist[i].EntryType == JournalTypeEnum.Docked || historylist[i].EntryType == JournalTypeEnum.LoadGame || historylist[i].EntryType == JournalTypeEnum.Shutdown)
                    break;
            }

            return null;
        }


        #endregion

        #region General Info

        // Add in any systems we have to the distlist 

        public void CalculateSqDistances(BaseUtils.SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                         int maxitems, double mindistance, double maxdistance, bool spherical, int maxjumpsback = int.MaxValue)
        {
            HashSet<string> listnames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (ISystem sys in distlist.Values.ToList())
                listnames.Add(sys.Name);

            mindistance *= mindistance;

            for( int i = historylist.Count-1; i >=0 && maxjumpsback>0; i-- )
            {
                HistoryEntry pos = historylist[i];

                if (pos.IsLocOrJump && pos.System.HasCoordinate && !listnames.Contains(pos.System.Name))
                {
                    double dx = (pos.System.X - x);
                    double dy = (pos.System.Y - y);
                    double dz = (pos.System.Z - z);
                    double distsq = dx * dx + dy * dy + dz * dz;

                    listnames.Add(pos.System.Name); //stops repeats..

                    if (distsq >= mindistance &&
                            (spherical && distsq <= maxdistance * maxdistance ||
                            !spherical && Math.Abs(dx) <= maxdistance && Math.Abs(dy) <= maxdistance && Math.Abs(dz) <= maxdistance))
                    {
                        if (distlist.Count < maxitems)          // if less than max, add..
                        {
                            distlist.Add(distsq, pos.System);
                        }
                        else if (distsq < distlist.Keys[distlist.Count - 1])   // if last entry (which must be the biggest) is greater than dist..
                        {
                            distlist.Add(distsq, pos.System);           // add in
                            distlist.RemoveAt(maxitems);        // remove last..
                        }
                    }

                    maxjumpsback--;
                }
            }
        }

        public HistoryEntry FindByName(string name, bool fsdjump = false)
        {
            if (fsdjump)
                return historylist.FindLast(x => x.System.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            else
                return historylist.FindLast(x => x.IsFSDCarrierJump && x.System.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        // Checks Cache, Database, History, Galactic map if required, and EDSM directly if required

        public ISystem FindSystem(string name, EDSM.GalacticMapping glist , bool checkedsm)        // in system or name
        {
            ISystem ds1 = SystemCache.FindSystem(name, checkedsm);     // go thru the cache and edsm if required

            if (ds1 == null)
            {
                HistoryEntry vs = FindByName(name);         // else try and find in our list

                if (vs != null)
                    ds1 = vs.System;
                else if (glist != null)                     // if we have a galmap
                {
                    EDSM.GalacticMapObject gmo = glist.Find(name, true);

                    if (gmo != null && gmo.points.Count > 0)                // valid item, and has position
                    {
                        ds1 = SystemCache.FindSystem(gmo.galMapSearch);     // only thru the db/cache, as we checked above for edsm direct, may be null

                        return gmo.GetSystem(ds1);                          // and return a ISystem.  If ds1=null, we use the points pos, if ds1 is found, we use the cache position 
                    }
                }
            }

            return ds1;
        }

        // 3dmap only
        public static HistoryEntry FindNextSystem(List<HistoryEntry> syslist, string sysname, int dir)
        {
            int index = syslist.FindIndex(x => x.System.Name.Equals(sysname));

            if (index != -1)
            {
                if (dir == -1)
                {
                    if (index < 1)                                  //0, we go to the end and work from back..
                        index = syslist.Count;

                    int indexn = syslist.FindLastIndex(index - 1, x => x.System.HasCoordinate);

                    if (indexn == -1)                             // from where we were, did not find one, try from back..
                        indexn = syslist.FindLastIndex(x => x.System.HasCoordinate);

                    return (indexn != -1) ? syslist[indexn] : null;
                }
                else
                {
                    index++;

                    if (index == syslist.Count)             // if at end, go to beginning
                        index = 0;

                    int indexn = syslist.FindIndex(index, x => x.System.HasCoordinate);

                    if (indexn == -1)                             // if not found, go to beginning
                        indexn = syslist.FindIndex(x => x.System.HasCoordinate);

                    return (indexn != -1) ? syslist[indexn] : null;
                }
            }
            else
            {
                index = syslist.FindLastIndex(x => x.System.HasCoordinate);
                return (index != -1) ? syslist[index] : null;
            }
        }

        #endregion

        #region Markers

        public void SetStartStop(HistoryEntry hs)
        {
            bool started = false;

            foreach (HistoryEntry he in historylist)
            {
                if (hs == he)
                {
                    if (he.StartMarker || he.StopMarker)
                        hs.journalEntry.ClearStartEndFlag();
                    else if (started == false)
                        hs.journalEntry.SetStartFlag();
                    else
                        hs.journalEntry.SetEndFlag();

                    break;
                }
                else if (he.StartMarker)
                    started = true;
                else if (he.StopMarker)
                    started = false;
            }
        }

        #endregion


        #region Common info extractors

        public void ReturnSystemInfo(HistoryEntry he, out string allegiance, out string economy, out string gov,
                                out string faction, out string factionstate, out string security)
        {
            EliteDangerousCore.JournalEvents.JournalFSDJump lastfsd =
                GetLastHistoryEntry(x => x.journalEntry is EliteDangerousCore.JournalEvents.JournalFSDJump, he)?.journalEntry as EliteDangerousCore.JournalEvents.JournalFSDJump;
            // same code in spanel.. not sure where to put it
            allegiance = lastfsd != null && lastfsd.Allegiance.Length > 0 ? lastfsd.Allegiance : he.System.Allegiance.ToNullUnknownString();
            if (allegiance.IsEmpty())
                allegiance = "-";
            economy = lastfsd != null && lastfsd.Economy_Localised.Length > 0 ? lastfsd.Economy_Localised : he.System.PrimaryEconomy.ToNullUnknownString();
            if (economy.IsEmpty())
                economy = "-";
            gov = lastfsd != null && lastfsd.Government_Localised.Length > 0 ? lastfsd.Government_Localised : he.System.Government.ToNullUnknownString();
            faction = lastfsd != null && lastfsd.FactionState.Length > 0 ? lastfsd.Faction : "-";
            factionstate = lastfsd != null && lastfsd.FactionState.Length > 0 ? lastfsd.FactionState : he.System.State.ToNullUnknownString();
            factionstate = factionstate.SplitCapsWord();
            if (factionstate.IsEmpty())
                factionstate = "-";

            security = lastfsd != null && lastfsd.Security_Localised.Length > 0 ? lastfsd.Security_Localised : "-";
        }

        #endregion
    }
}
