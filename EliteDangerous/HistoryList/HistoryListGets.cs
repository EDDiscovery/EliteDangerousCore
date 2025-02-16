﻿/*
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
        public int CommanderId { get; private set; } = -999;                 // set by history load at end, indicating commander loaded
        public bool HistoryLoaded { get { return CommanderId > -999; } }      // history is loaded
        public bool IsRealCommanderId { get { return CommanderId >= 0; } }      // history is loaded with a non hidden commander log etc
        public string CommanderName() { return EDCommander.GetCommander(CommanderId)?.Name ?? null; }
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

        // history filter - limit by date, from startdate backwards by days
        static public List<HistoryEntry> LimitByDate(List<HistoryEntry> list, DateTime startdateutc, TimeSpan days, HashSet<JournalTypeEnum> entries = null, bool reverse = true)     // list should be in entry order (oldest first)
        {
            System.Diagnostics.Debug.Assert(startdateutc.Kind == DateTimeKind.Utc);

            var oldestData = startdateutc.Subtract(days);

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

        // history filter - limit by date range
        // reverse means reverse whatever order the list is in
        // entries means only accept these entries
        static public List<HistoryEntry> LimitByDate(List<HistoryEntry> list, DateTime startdateutc, DateTime enddaysutc, HashSet<JournalTypeEnum> entries = null, bool reverse = true)     // list should be in entry order (oldest first)
        {
            System.Diagnostics.Debug.Assert(startdateutc.Kind == DateTimeKind.Utc);

            if (entries != null)
            {
                if (reverse)
                    return list.Where(x => entries.Contains(x.EntryType) && x.EventTimeUTC >= startdateutc && x.EventTimeUTC <= enddaysutc).Reverse().ToList();
                else
                    return list.Where(x => entries.Contains(x.EntryType) && x.EventTimeUTC >= startdateutc && x.EventTimeUTC <= enddaysutc).ToList();
            }
            else
            {
                if (reverse)
                    return list.Where(x => x.EventTimeUTC >= startdateutc && x.EventTimeUTC <= enddaysutc).Reverse().ToList();
                else
                    return list.Where(x => x.EventTimeUTC >= startdateutc && x.EventTimeUTC <= enddaysutc).ToList();
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
            int lastdock = list.FindLastIndex(x => !x.Status.IsInMultiPlayer && x.EntryType == JournalTypeEnum.Docked);
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
        static public List<HistoryEntry> FilterByDateRange(List<HistoryEntry> list, DateTime startutc, DateTime endutc, Predicate<HistoryEntry> pred) // UTC! in time ascending order
        {
            return list.Where(s => pred(s) && s.EventTimeUTC >= startutc && s.EventTimeUTC <= endutc).ToList();
        }

        // combat panel, List should be in entry order.
        static public List<HistoryEntry> FilterByDateRangeLatestFirst(List<HistoryEntry> list, DateTime startutc, DateTime endutc) // UTC! in time ascending order
        {
            return list.Where(s => s.EventTimeUTC >= startutc && s.EventTimeUTC <= endutc).OrderByDescending(s => s.EventTimeUTC).ToList();
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

        // market data, backwards in time, ordered by system, then by whereis, then by time.  
        // not been easy!
        //(!filteroutempty || (x.journalEntry as JournalCommodityPricesBase).Commodities.Count > 0)
        static public List<List<IGrouping<string, HistoryEntry>>> FilterByCommodityPricesBackwardsSystemWhereAmI(List<HistoryEntry> list, bool filteroutempty = false)
        {
            var c1 = list.Where( x => (x.EntryType == JournalTypeEnum.Market || x.EntryType == JournalTypeEnum.EDDCommodityPrices) &&
                                (!filteroutempty || (x.journalEntry as JournalCommodityPricesBase).Commodities.Count > 0) 
                                )
                                .OrderByDescending(x => x.EventTimeUTC)
                                .ToList();

            List<IGrouping<string, HistoryEntry>> systems = c1.GroupBy(x => x.System.Name).ToList();        // group into Systems ordered by time

            List<List<IGrouping<string, HistoryEntry>>> systemwhere = new List<List<IGrouping<string, HistoryEntry>>>();

            foreach (IGrouping<string, HistoryEntry> sys in systems)    // now go thru systems and create a grouping based on whereami (prob use a linq but hard to work out)
            {
                List<IGrouping<string, HistoryEntry>> values = sys.GroupBy(x => x.WhereAmI).ToList();
                systemwhere.Add(values);
            }

            //foreach (List<IGrouping<string, HistoryEntry>> s in systemwhere)
            //{
            //    foreach (var w in s)
            //    {
            //        foreach (var h in w)
            //            System.Diagnostics.Debug.WriteLine($"Entry {h.System.Name} | {h.WhereAmI} | {h.EventTimeUTC}");
            //    }
            //    System.Diagnostics.Debug.WriteLine($"-");
            //}

            return systemwhere;
        }


        // trilat
        public List<HistoryEntry> FilterByFSDOnly() { return (from s in historylist where s.EntryType == JournalTypeEnum.FSDJump select s).ToList(); }

        #endregion

        #region Status

        public HistoryEntryStatus.TravelStateType CurrentTravelState() { HistoryEntry he = GetLast; return (he != null) ? he.Status.TravelState : HistoryEntryStatus.TravelStateType.Unknown; }     //safe methods
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
            return historylist.FindLast(x => x.Journalid == jid);
        }

        public HistoryEntry GetByEntryNo(int entryno)
        {
            return (entryno >= 1 && entryno <= historylist.Count) ? historylist[entryno - 1] : null;
        }

        public int GetIndexOfJID(long jid)
        {
            return historylist.FindIndex(x => x.Journalid == jid);
        }

        public HistoryEntry GetLast => historylist.LastOrDefault();

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

        // trilat
        public HistoryEntry GetLastLocation() { return historylist.FindLast(x => x.IsFSDLocationCarrierJump); }

        // multiple
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

        // sysinfo
        public HistoryEntry GetLastWithPosition() { return historylist.FindLast(x => x.System.HasCoordinate); }

        // everywhere
        public int GetVisitsCount(string name)
        {
            if (Visited.TryGetValue(name, out var he))
                return he.Visits;
            else
                return 0;
        }

        // historylist
        public string GetCommanderFID()     // may be null
        {
            var cmdr = historylist.FindLast(x => x.EntryType == JournalTypeEnum.Commander);
            return (cmdr?.journalEntry as JournalCommander)?.FID;
        }

        // 3dmap
        public List<HistoryEntry> FilterByTravelTimeAndMulticrew(DateTime? starttimeutc, DateTime? endtimeutc, bool musthavecoord)        // filter, in its own order. return FSD,carrier and location events after death
        {
            List<HistoryEntry> ents = new List<HistoryEntry>();
            string lastsystem = null;
            foreach (HistoryEntry he in historylist)        // in add order, oldest first
            {
                if ((he.EntryType == JournalTypeEnum.Location || he.EntryType == JournalTypeEnum.CarrierJump || he.EntryType == JournalTypeEnum.FSDJump) &&
                    (he.System.HasCoordinate || !musthavecoord) && !he.Status.IsInMultiPlayer)
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

                if (pos.IsFSDLocationCarrierJump && pos.System.HasCoordinate && !listnames.Contains(pos.System.Name))
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

        #endregion

        #region Common info extractors

        public void ReturnSystemInfo(HistoryEntry he, out string allegiance, out string economy, out string gov,
                                out string faction, out string factionstate, out string security)
        {
            ReturnSystemInfo(he, out AllegianceDefinitions.Allegiance pallegiance, out EconomyDefinitions.Economy peconomy, out GovernmentDefinitions.Government pgov,
                                out faction, out FactionDefinitions.State pfactionstate, out SecurityDefinitions.Security psecurity);

            allegiance = pallegiance!=AllegianceDefinitions.Allegiance.Unknown ? AllegianceDefinitions.ToLocalisedLanguage(pallegiance) : "?";
            economy = peconomy != EconomyDefinitions.Economy.Unknown ? EconomyDefinitions.ToLocalisedLanguage(peconomy) : "?";
            gov = pgov != GovernmentDefinitions.Government.Unknown ? GovernmentDefinitions.ToLocalisedLanguage(pgov) : "?";
            factionstate = pfactionstate != FactionDefinitions.State.Unknown ? FactionDefinitions.ToLocalisedLanguage(pfactionstate) : "?";
            security = psecurity != SecurityDefinitions.Security.Unknown ? SecurityDefinitions.ToLocalisedLanguage(psecurity) : "-";
        }

        public void ReturnSystemInfo(HistoryEntry he, out AllegianceDefinitions.Allegiance allegiance, out EconomyDefinitions.Economy economy, out GovernmentDefinitions.Government gov,
                                out string faction, out FactionDefinitions.State factionstate, out SecurityDefinitions.Security security)
        {
            JournalFSDJump lastfsd = GetLastHistoryEntry(x => x.journalEntry is EliteDangerousCore.JournalEvents.JournalFSDJump, he)?.journalEntry as EliteDangerousCore.JournalEvents.JournalFSDJump;

            allegiance = lastfsd != null ? lastfsd.Allegiance : AllegianceDefinitions.Allegiance.Unknown;
            economy = lastfsd != null ? lastfsd.Economy : EconomyDefinitions.Economy.Unknown;
            gov = lastfsd != null ? lastfsd.Government : GovernmentDefinitions.Government.Unknown;
            faction = lastfsd != null && lastfsd.Faction.Length > 0 ? lastfsd.Faction.SplitCapsWordFull() : "-";
            factionstate = lastfsd != null ? lastfsd.FactionState: FactionDefinitions.State.Unknown;
            security = lastfsd != null ? lastfsd.Security : SecurityDefinitions.Security.Unknown;
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
