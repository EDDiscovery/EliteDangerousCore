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

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class HistoryList 
    {
        #region General gets
        public bool HistoryLoaded { get { return CommanderId > -999; } }      // history is loaded
        public bool IsRealCommanderId { get { return CommanderId >= 0; } }      // history is loaded with a non hidden commander log etc
        public string LastSystem { get; private set; }                          // last system seen in

        public int Count { get { return historylist.Count; } }

        public int Visits(string name) { return Visited.TryGetValue(name, out var res) ? res.Visits : 0; }     // name is case insensitive
        public int VisitedSystemsCount { get { return Visited.Count; } }

        // oldest first
        public List<HistoryEntry> EntryOrder() { return historylist; }
        public HistoryEntry this[int i] { get {return historylist[i]; } }
        public HistoryEntry LastOrDefault { get { return historylist.LastOrDefault(); } }

        // combat, spanel,stats, history filter
        public List<HistoryEntry> LatestFirst() { return historylist.OrderByDescending(s => s.EntryNumber).ToList(); }

        #endregion

        #region Output filters

        // history filter. list should be in entry order (oldest first)
        static public List<HistoryEntry> LatestFirst(List<HistoryEntry> list) 
        {
            return Enumerable.Reverse(list).ToList();
        }
        // history filter. list should be in entry order (oldest first)
        static public List<HistoryEntry> LatestFirst(List<HistoryEntry> list, HashSet<JournalTypeEnum> entries) 
        {
            return list.Where(x => entries.Contains(x.EntryType)).Reverse().ToList();
        }
        // history filter, surveyor, search scans. list should be in entry order (oldest first)
        static public List<HistoryEntry> FilterByEventEntryOrder(List<HistoryEntry> list, HashSet<JournalTypeEnum> entries, ISystem sys = null, HashSet<JournalTypeEnum> afterlastevent = null) 
        {
            if ( afterlastevent != null)        // find last index of journal event and subset the list.
            {
                int index = list.FindLastIndex(x => afterlastevent.Contains(x.EntryType));
                if (index >= 0)
                    list = list.GetRange(index, list.Count - index);
            }

            if (sys == null)
                return list.Where(x => entries.Contains(x.EntryType)).ToList();
            else
                return list.Where(x => x.System.Name == sys.Name && entries.Contains(x.EntryType)).ToList();
        }

        // history filter
        static public List<HistoryEntry> LatestFirstLimitNumber(List<HistoryEntry> list, int max) // list should be in entry order (oldest first)
        {
            return Enumerable.Reverse(list).Take(max).ToList();
        }

        // history filter
        static public List<HistoryEntry> LimitByDate(List<HistoryEntry> list, TimeSpan days, HashSet<JournalTypeEnum> entries = null, bool reverse = true)     // list should be in entry order (oldest first)
        {
            var oldestData = DateTime.UtcNow.Subtract(days);
            if (entries != null)
            {
                if (reverse)
                    return list.Where(x => entries.Contains(x.EntryType) && x.EventTimeUTC >= oldestData).Reverse().ToList();
                else
                    return list.Where(x => entries.Contains(x.EntryType) && x.EventTimeUTC >= oldestData).ToList();
            }
            else
            {
                if (reverse)
                    return list.Where(x => x.EventTimeUTC >= oldestData).Reverse().ToList();
                else
                    return list.Where(x => x.EventTimeUTC >= oldestData).ToList();
            }
        }

        // history filter List should be in entry order.
        static public List<HistoryEntry> StartStopFlags(List<HistoryEntry> list, HashSet<JournalTypeEnum> entriestoaccept = null, bool reverse = true)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();
            bool started = false;
            foreach (HistoryEntry he in list)   // in entry order, last first
            {
                if (he.StartMarker)
                {
                    started = true;
                    if (entriestoaccept == null || entriestoaccept.Contains(he.EntryType))
                        entries.Add(he);
                }
                else if (started)
                {
                    if (entriestoaccept == null || entriestoaccept.Contains(he.EntryType))
                        entries.Add(he);

                    if (he.StopMarker && !he.StartMarker)
                        started = false;
                }
            }

            if ( reverse)
                return Enumerable.Reverse(entries).ToList();
            else
                return entries;
        }

        // history filter, combat panel. List should be in entry order.
        static public List<HistoryEntry> ToLastDock(List<HistoryEntry> list, HashSet<JournalTypeEnum> entriestoaccept = null, bool reverse = true)
        {
            int lastdock = list.FindLastIndex(x => !x.Status.MultiPlayer && x.EntryType == JournalTypeEnum.Docked);
            if (lastdock >= 0)
            {
                List<HistoryEntry> tolastdock = new List<HistoryEntry>();

                if (reverse)
                {
                    if (entriestoaccept == null)
                    {
                        for (int i = list.Count - 1; i >= lastdock; i--)     // go backwards so in lastest first order
                            tolastdock.Add(list[i]);
                    }
                    else
                    {
                        for (int i = list.Count - 1; i >= lastdock; i--)     // go backwards so in lastest first order
                        {
                            if (entriestoaccept.Contains(list[i].EntryType))
                                tolastdock.Add(list[i]);
                        }
                    }
                }
                else
                {
                    if (entriestoaccept == null)
                    {
                        for (int i = lastdock; i < list.Count; i++)     // go backwards so in lastest first order
                            tolastdock.Add(list[i]);
                    }
                    else
                    {
                        for (int i = lastdock; i < list.Count; i++)     // go backwards so in lastest first order
                        {
                            if (entriestoaccept.Contains(list[i].EntryType))
                                tolastdock.Add(list[i]);
                        }
                    }

                }

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
        // from and including pos, go back in time, and return all which match where
        static public List<HistoryEntry> FilterBefore(List<HistoryEntry> list, HistoryEntry pos, Predicate<HistoryEntry> where)         
        {
            int indexno = pos.EntryNumber-1;        // position in HElist..
            List<HistoryEntry> helist = new List<HistoryEntry>();
            while (indexno >= 0 && indexno < list.Count)        // check within range.
            {
                if (where(list[indexno]))
                {
                    //System.Diagnostics.Debug.WriteLine(list[indexno].EntryType + " " + list[indexno].StationFaction);
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
            ISystem other = SystemCache.FindSystem(system);    // does not use EDSM for this, just DB and history
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

        // find the condition, from this HE inclusive, backwards, until stop date is passed.
        public HistoryEntry GetLastHistoryEntry(Predicate<HistoryEntry> where, HistoryEntry frominclusive, DateTime stopbeforeinc)
        {
            for (int index = frominclusive.EntryNumber - 1; index >= 0 && index < historylist.Count && historylist[index].EventTimeUTC >= stopbeforeinc; index--)
            {
                if (where(historylist[index]))
                    return historylist[index];
            }

            return null;
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
        // return trip start/stop he's within the date range.
        // may return start but no end, or end without a start, or both null
        public void FindStartStopMarkersWithinDateTimeRange(DateTime starttimeutc, DateTime endtimeutc, out HistoryEntry tripstarthe, out HistoryEntry tripendhe)
        {
            tripstarthe = tripendhe = null;

            HistoryEntry firstdatebeforeend = GetLastHistoryEntry(x => x.EventTimeUTC <= endtimeutc);        // where is the entry at the end time..

            if (firstdatebeforeend != null)     // found an entry within the range.
            {
                tripendhe = GetLastHistoryEntry(x => x.StopMarker, firstdatebeforeend, starttimeutc);       // from endtime, find the first stop marker

                if (tripendhe != null)            // found one
                {
                    tripstarthe = GetLastHistoryEntry(x => x.StartMarker, tripendhe, starttimeutc);        // find the start marker if in the range.  May be null
                }
                else
                {           // no end marker, is there a start?
                    tripstarthe = GetLastHistoryEntry(x => x.StartMarker, firstdatebeforeend, starttimeutc);        // find the start marker if in the range.  May be null
                }
            }
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
        public List<HistoryEntry> FilterByTravelTime(DateTime? starttimeutc, DateTime? endtimeutc, bool musthavecoord)        // filter, in its own order. return FSD,carrier and location events after death
        {
            List<HistoryEntry> ents = new List<HistoryEntry>();
            string lastsystem = null;
            foreach (HistoryEntry he in historylist)        // in add order, oldest first
            {
                if ((he.EntryType == JournalTypeEnum.Location || he.EntryType == JournalTypeEnum.CarrierJump || he.EntryType == JournalTypeEnum.FSDJump) &&
                    (he.System.HasCoordinate || !musthavecoord))
                { 
                    if ((starttimeutc == null || he.EventTimeUTC >= starttimeutc) && (endtimeutc == null || he.EventTimeUTC <= endtimeutc))
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

        // findsystemusercontrol
        // find the last jump entry to system name
        public HistoryEntry FindLastFSDCarrierJumpBySystemName(string name)
        {
            return historylist.FindLast(x => x.IsFSDCarrierJump && x.System.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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

        #region Common info extractors

        public void ReturnSystemInfo(HistoryEntry he, out string allegiance, out string economy, out string gov,
                                out string faction, out string factionstate, out string security)
        {
            JournalFSDJump lastfsd = GetLastHistoryEntry(x => x.journalEntry is EliteDangerousCore.JournalEvents.JournalFSDJump, he)?.journalEntry as EliteDangerousCore.JournalEvents.JournalFSDJump;

            allegiance = lastfsd != null && lastfsd.Allegiance.Length > 0 ? lastfsd.Allegiance.SplitCapsWordFull() : "?";
            economy = lastfsd != null && lastfsd.Economy_Localised.Length > 0 ? lastfsd.Economy_Localised : "?";
            gov = lastfsd != null && lastfsd.Government_Localised.Length > 0 ? lastfsd.Government_Localised : "?";
            faction = lastfsd != null && lastfsd.Faction.Length > 0 ? lastfsd.Faction.SplitCapsWordFull() : "-";
            factionstate = lastfsd != null && lastfsd.FactionState.Length > 0 ? lastfsd.FactionState.SplitCapsWordFull() : "?";
            security = lastfsd != null && lastfsd.Security_Localised.Length > 0 ? lastfsd.Security_Localised : "-";
        }

        #endregion

        #region Gameversion helpers

        // find last entry in history with a good gameversion, or ""
        public Tuple<string,string> GetLastGameversionBuild()
        {
            for (int i = historylist.Count - 1; i >= 0; i--)
            {
                if (historylist[i].journalEntry.GameVersion.HasChars())
                {
                    return new Tuple<string, string>(historylist[i].journalEntry.GameVersion, historylist[i].journalEntry.Build);
                }
            }

            return new Tuple<string, string>("", "");
        }


        #endregion
    }
}
