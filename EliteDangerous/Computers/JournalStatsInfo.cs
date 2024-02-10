/*
 * Copyright © 2022-2023 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public class JournalStatsInfo
    {
        // analyse the journalstats helpers

        public class DataReturn        // used to pass data from thread back to user control
        {
            public string[][] griddata;
            public int[][] chart1data;
            public string[] chart2labels;
            public int[][] chart2data;
            public string[] chart1labels;
        }

        public static System.Threading.Tasks.Task<DataReturn> ComputeCombat(JournalStats currentstat, Tuple<DateTime[], DateTime[]> tupletimes)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                DataReturn crs = new DataReturn();

                int intervals = tupletimes.Item1.Length;

                int results = 40;                               // does not need to be accurate
                crs.griddata = new string[results][];         // outer [] is results
                for (var i = 0; i < results; i++)
                    crs.griddata[i] = new string[intervals];

                crs.chart1labels = new string[]
                {
                    "Bounties on Thargoids".T(EDCTx.UserControlStats_BountiesThargoid),
                    "Bounties on On Foot NPC".T(EDCTx.UserControlStats_BountiesOnFootNPC),
                    "Bounties on Skimmers".T(EDCTx.UserControlStats_BountiesSkimmers),
                    "Ships Unknown Rank".T(EDCTx.UserControlStats_ShipsUnknown),
                    "Ships Elite Rank".T(EDCTx.UserControlStats_ShipsElite),
                    "Ships Deadly Rank".T(EDCTx.UserControlStats_ShipsDeadly),
                    "Ships Dangerous Rank".T(EDCTx.UserControlStats_ShipsDangerous),
                    "Ships Master Rank".T(EDCTx.UserControlStats_ShipsMaster),
                    "Ships Expert Rank".T(EDCTx.UserControlStats_ShipsExpert),
                    "Ships Competent Rank".T(EDCTx.UserControlStats_ShipsCompetent),
                    "Ships Novice Rank".T(EDCTx.UserControlStats_ShipsNovice),
                    "Ships Harmless Rank".T(EDCTx.UserControlStats_ShipsHarmless),
                };

                crs.chart1data = new int[intervals][];        // outer [] is intervals      CHART 1 is PVP
                for (var i = 0; i < intervals; i++)
                    crs.chart1data[i] = new int[crs.chart1labels.Length];

                crs.chart2labels = new string[]
                {
                     "PVP Elite Rank".T(EDCTx.UserControlStats_PVPElite),
                     "PVP Deadly Rank".T(EDCTx.UserControlStats_PVPDeadly),
                     "PVP Dangerous Rank".T(EDCTx.UserControlStats_PVPDangerous),
                     "PVP Master Rank".T(EDCTx.UserControlStats_PVPMaster),
                     "PVP Expert Rank".T(EDCTx.UserControlStats_PVPExpert),
                     "PVP Competent Rank".T(EDCTx.UserControlStats_PVPCompetent),
                     "PVP Novice Rank".T(EDCTx.UserControlStats_PVPNovice),
                     "PVP Harmless Rank".T(EDCTx.UserControlStats_PVPHarmless),
                };

                crs.chart2data = new int[intervals][];        // outer [] is intervals
                for (var i = 0; i < intervals; i++)
                    crs.chart2data[i] = new int[crs.chart2labels.Length];

                for (var ii = 0; ii < intervals; ii++)
                {
                    var startutc = tupletimes.Item1[ii];
                    var endutc = tupletimes.Item2[ii];

                    var pvpStats = currentstat.PVPKills.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var bountyStats = currentstat.Bounties.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var crimesStats = currentstat.Crimes.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var sfactionkillbonds = currentstat.FactionKillBonds.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var interdictions = currentstat.Interdiction.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var interdicted = currentstat.Interdicted.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();

                    //foreach(var x in bountyStats) System.Diagnostics.Debug.WriteLine($"{ii} = {x.EventTimeUTC} {x.Target} {x.IsShip} {x.IsThargoid} {x.IsOnFootNPC} {x.VictimFaction}");

                    int row = 0;

                    crs.griddata[row++][ii] = bountyStats.Count.ToString("N0");
                    crs.griddata[row++][ii] = bountyStats.Select(x => x.TotalReward).Sum().ToString("N0");
                    crs.griddata[row++][ii] = bountyStats.Where(x => x.IsShip).Count().ToString("N0");

                    int p = 0;
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.IsThargoid).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.IsOnFootNPC).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.IsSkimmer).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsUnknownShip).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsEliteAboveShip).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Deadly)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Dangerous)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Master)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Expert)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Competent)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsRankShip(RankDefinitions.CombatRank.Novice)).Count();
                    crs.chart1data[ii][p++] = bountyStats.Where(x => x.StatsHarmlessShip).Count();

                    for (int pp = 0; pp < p; pp++)
                    {
                        //if ( ii==0)
                        //crs.chart1data[ii][pp] = (pp + 1);
                        //crs.chart1data[ii][pp] = 0;
                        crs.griddata[row++][ii] = crs.chart1data[ii][pp].ToString("N0");
                    }

                    crs.griddata[row++][ii] = crimesStats.Count.ToString("N0");
                    crs.griddata[row++][ii] = crimesStats.Select(x => x.Cost).Sum().ToString("N0");

                    crs.griddata[row++][ii] = sfactionkillbonds.Count.ToString("N0");
                    crs.griddata[row++][ii] = sfactionkillbonds.Select(x => x.Reward).Sum().ToString("N0");

                    crs.griddata[row++][ii] = interdictions.Where(x => x.Success && x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdictions.Where(x => !x.Success && x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdictions.Where(x => x.Success && !x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdictions.Where(x => !x.Success && !x.IsPlayer).Count().ToString("N0");

                    crs.griddata[row++][ii] = interdicted.Where(x => x.Submitted && x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdicted.Where(x => !x.Submitted && x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdicted.Where(x => x.Submitted && !x.IsPlayer).Count().ToString("N0");
                    crs.griddata[row++][ii] = interdicted.Where(x => !x.Submitted && !x.IsPlayer).Count().ToString("N0");

                    crs.griddata[row++][ii] = pvpStats.Count.ToString("N0");

                    p = 0;
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank >= RankDefinitions.CombatRank.Elite).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Deadly).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Dangerous).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Master).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Expert).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Competent).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank == RankDefinitions.CombatRank.Novice).Count();
                    crs.chart2data[ii][p++] = pvpStats.Where(x => x.CombatRank <= RankDefinitions.CombatRank.Mostly_Harmless).Count();

                    for (int pp = 0; pp < p; pp++)
                    {
                        //if ( ii == 2)
                        //crs.chart2data[ii][pp] = (pp + 1);
                        //crs.chart2data[ii][pp] = 0;
                        crs.griddata[row++][ii] = crs.chart2data[ii][pp].ToString("N0");
                    }
                }

                return crs;
            });
        }


        public static System.Threading.Tasks.Task<DataReturn> ComputeScans(JournalStats currentstat, Tuple<DateTime[], DateTime[]> tupletimes, bool starmode)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                DataReturn crs = new DataReturn();

                int results = starmode ? Enum.GetValues(typeof(EDStar)).Length : Enum.GetValues(typeof(EDPlanet)).Length;

                crs.chart1labels = new string[results];     // fill up chart labels
                if (starmode)
                {
                    int i = 0;
                    foreach (EDStar startype in Enum.GetValues(typeof(EDStar)))
                        crs.chart1labels[i++] = Stars.StarName(startype);
                }
                else
                {
                    int i = 0;
                    foreach (EDPlanet planettype in Enum.GetValues(typeof(EDPlanet)))
                        crs.chart1labels[i++] = planettype == EDPlanet.Unknown_Body_Type ? "Belt Cluster".T(EDCTx.UserControlStats_Beltcluster) : Planets.PlanetName(planettype);
                }

                int intervals = tupletimes.Item1.Length;

                crs.chart1data = new int[intervals][];        // outer [] is intervals      CHART 1 is PVP
                for (var i = 0; i < intervals; i++)
                    crs.chart1data[i] = new int[crs.chart1labels.Length];

                var scanlists = new List<JournalScan>[intervals];
                for (int ii = 0; ii < intervals; ii++)
                    scanlists[ii] = currentstat.Scans.Values.Where(x => x.EventTimeUTC >= tupletimes.Item1[ii] && x.EventTimeUTC < tupletimes.Item2[ii]).ToList();

                results++;      // 1 more for totals at end

                crs.griddata = new string[results][];
                for (var i = 0; i < results; i++)
                    crs.griddata[i] = new string[intervals];

                long[] totals = new long[intervals];

                if (starmode)
                {
                    int row = 0;
                    foreach (EDStar startype in Enum.GetValues(typeof(EDStar)))
                    {
                        for (int ii = 0; ii < intervals; ii++)
                        {
                            int num = 0;
                            for (int jj = 0; jj < scanlists[ii].Count; jj++)
                            {
                                if (scanlists[ii][jj].StarTypeID == startype && scanlists[ii][jj].IsStar)
                                    num++;
                            }

                            crs.chart1data[ii][row] = num;
                            crs.griddata[row][ii] = num.ToString("N0");
                            totals[ii] += num;
                        }

                        row++;
                    }
                }
                else
                {
                    int row = 0;
                    foreach (EDPlanet planettype in Enum.GetValues(typeof(EDPlanet)))
                    {
                        for (int ii = 0; ii < intervals; ii++)
                        {
                            int num = 0;
                            for (int jj = 0; jj < scanlists[ii].Count; jj++)
                            {
                                // System.Diagnostics.Debug.WriteLine($"Planet for {planettype} {scanlists[ii][jj].PlanetTypeID} {scanlists[ii][jj].EventTimeUTC}");
                                if (scanlists[ii][jj].PlanetTypeID == planettype && !scanlists[ii][jj].IsStar)
                                    num++;
                            }

                            crs.chart1data[ii][row] = planettype == EDPlanet.Unknown_Body_Type ? 0 : num;       // we knock out of the chart belt clusters
                            crs.griddata[row][ii] = num.ToString("N0");
                            totals[ii] += num;
                        }
                        row++;
                    }
                }

                for (int i = 0; i < intervals; i++)
                    crs.griddata[results - 1][i] = totals[i].ToString("N0");

                return crs;
            });
        }

        public static System.Threading.Tasks.Task<string[][]> ComputeTravel(JournalStats currentstat, Tuple<DateTime[], DateTime[]> tupletimes)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                int results = 10;

                string[][] res = new string[results][];
                for (var i = 0; i < results; i++)
                    res[i] = new string[tupletimes.Item1.Length];

                for (var ii = 0; ii < tupletimes.Item1.Length; ii++)
                {
                    var startutc = tupletimes.Item1[ii];
                    var endutc = tupletimes.Item2[ii];

                    var fsdStats = currentstat.FSDJumps.Where(x => x.utc >= startutc && x.utc < endutc).ToList();
                    var jetconeboosts = currentstat.JetConeBoost.Where(x => x.utc >= startutc && x.utc < endutc).ToList();
                    var scanStats = currentstat.Scans.Values.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var saascancomplete = currentstat.SAAScanComplete.Values.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();
                    var organicscans = currentstat.OrganicScans.Where(x => x.EventTimeUTC >= startutc && x.EventTimeUTC < endutc).ToList();

                    int row = 0;
                    res[row++][ii] = fsdStats.Count.ToString("N0");
                    res[row++][ii] = fsdStats.Sum(j => j.jumpdist).ToString("N2");
                    res[row++][ii] = fsdStats.Where(j => j.boostvalue == 1).Count().ToString("N0");
                    res[row++][ii] = fsdStats.Where(j => j.boostvalue == 2).Count().ToString("N0");

                    res[row++][ii] = fsdStats.Where(j => j.boostvalue == 3).Count().ToString("N0");
                    res[row++][ii] = jetconeboosts.Count().ToString("N0");
                    res[row++][ii] = scanStats.Count.ToString("N0");       // scan count
                    res[row++][ii] = saascancomplete.Count().ToString("N0");   // mapped

                    res[row++][ii] = scanStats.Sum(x => (long)x.EstimatedValue).ToString("N0");
                    res[row++][ii] = organicscans.Sum(x => (long)(x.EstimatedValue ?? 0)).ToString("N0");
                    System.Diagnostics.Debug.Assert(row == results);
                }

                return res;
            });
        }

        public enum TimeModeType
        {
            Summary = 0,
            Day = 1,
            Week = 2,
            Month = 3,
            Year = 4,
            NotSet = 999,
        }

        public static Tuple<DateTime[], DateTime[]> SetUpDaysMonthsYear(DateTime endtimenowutc, TimeModeType timemode)
        {
            int intervals = timemode == TimeModeType.Year ? Math.Min(12, endtimenowutc.Year - 2013) : 12;

            DateTime[] starttimeutc = new DateTime[intervals];
            DateTime[] endtimeutc = new DateTime[intervals];

            for (int ii = 0; ii < intervals; ii++)
            {
                if (ii == 0)
                {
                    if (timemode == TimeModeType.Month)
                        endtimeutc[0] = endtimenowutc.EndOfMonth().AddSeconds(1);
                    else if (timemode == TimeModeType.Week)
                        endtimeutc[0] = endtimenowutc.EndOfWeek().AddSeconds(1);
                    else if (timemode == TimeModeType.Year)
                        endtimeutc[0] = endtimenowutc.EndOfYear().AddSeconds(1);
                    else
                        endtimeutc[0] = endtimenowutc.AddSeconds(1);
                }
                else
                    endtimeutc[ii] = starttimeutc[ii - 1];

                starttimeutc[ii] = timemode == TimeModeType.Day ? endtimeutc[ii].AddDays(-1) :
                                timemode == TimeModeType.Week ? endtimeutc[ii].AddDays(-7) :
                                timemode == TimeModeType.Year ? endtimeutc[ii].AddYears(-1) :
                                endtimeutc[ii].AddMonths(-1);

            }

            return new Tuple<DateTime[], DateTime[]>(starttimeutc, endtimeutc);
        }

        public static Tuple<DateTime[], DateTime[]> SetupSummary(DateTime starttimeutc, DateTime endtimeutc, DateTime lastdockedutc)
        {
            DateTime[] starttimesutc = new DateTime[5];
            DateTime[] endtimesutc = new DateTime[5];
            starttimesutc[0] = endtimeutc.AddDays(-1).AddSeconds(1);
            starttimesutc[1] = endtimeutc.AddDays(-7).AddSeconds(1);
            starttimesutc[2] = endtimeutc.AddMonths(-1).AddSeconds(1);
            starttimesutc[3] = lastdockedutc;
            starttimesutc[4] = starttimeutc;
            endtimesutc[0] = endtimesutc[1] = endtimesutc[2] = endtimesutc[3] = endtimesutc[4] = endtimeutc;

            return new Tuple<DateTime[], DateTime[]>(starttimesutc, endtimesutc);
        }
    }
}

