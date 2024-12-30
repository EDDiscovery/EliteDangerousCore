/*
 * Copyright © 2022-2024 EDDiscovery development team
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

using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{

    public class FactionStatsComputer
    {
        // per faction
        // Data for a faction accumulated by the computer - mostly from Stats.FactionStatistics
        // adding in mission events 

        public class FactionResults
        {
            public string Name { get; }     // faction name

            // mission info

            public class PerSystem          // per system additional info - missions for now
            {
                public ISystem System { get; set; }
                public int Missions { get; set; }
                public int MissionsInProgress { get; set; }
                public int Influence { get; set; }
                public int Reputation { get; set; }
                public long MissionCredits { get; set; }

                public class MissionReward
                {
                    public string Name { get; }
                    public int Count { get; set; }

                    public MissionReward(string name)
                    {
                        Name = name;
                        Count = 0;
                    }

                    public void Add(int amount)
                    {
                        Count += amount;
                    }
                }

                public Dictionary<string, MissionReward> MissionRewards { get; } = new Dictionary<string, MissionReward>();

                public void AddMaterialCommodityReward(string rewardlocalisedname, int amount)
                {
                    MissionReward reward;
                    if (!MissionRewards.TryGetValue(rewardlocalisedname, out reward))
                    {
                        reward = new MissionReward(rewardlocalisedname);
                        MissionRewards.Add(rewardlocalisedname, reward);
                    }
                    reward.Add(amount);
                }
            }

            // the additional per system data
            public Dictionary<string, PerSystem> PerSystemData { get; private set; } = new Dictionary<string, PerSystem>();

            // other stats from FactionStatistics
            public Stats.FactionStatistics FactionStats { get; set; }

            public FactionResults(string name)
            {
                Name = name;
            }
        }

        // Compute Stats from the mission list (total) filtered by start/end date,
        // statsinfo (pass in latest if not limiting by date, or null to recompute over date range)
        // whole history

        public static SortedDictionary<string, FactionResults> Compute(List<MissionState> ml,   // total mission list
                                                                Stats statsinfo, // if passed in null, then this will compute it. If passed in, uses this to populate FactionStats above
                                                                HistoryList hist,   // total history
                                                                DateTime startdateutc, DateTime enddateutc) // bounding times, must be set up
        {
            var FactionList = new SortedDictionary<string, FactionResults>();

            if ( ml != null )
            {
                // first we make this.FactionStatistics and fill in with applicable missions between start/end dates

                foreach (MissionState ms in ml)
                {
                    bool withinstarttime = DateTime.Compare(ms.Mission.EventTimeUTC, startdateutc) >= 0 && DateTime.Compare(ms.Mission.EventTimeUTC, enddateutc) <= 0;
                    bool withinexpirytime = ms.Mission.ExpiryValid && DateTime.Compare(ms.Mission.Expiry, startdateutc) >= 0 && DateTime.Compare(ms.Mission.Expiry, enddateutc) <= 0;
                    bool withincompletetime = ms.Completed != null && DateTime.Compare(ms.Completed.EventTimeUTC, startdateutc) >= 0 && DateTime.Compare(ms.Completed.EventTimeUTC, enddateutc) <= 0;

                    if (withinstarttime || withincompletetime)
                    {
                        if (!FactionList.TryGetValue(ms.Mission.Faction, out FactionResults factionStats))   // is faction present? if not create
                            factionStats = FactionList[ms.Mission.Faction] = new FactionResults(ms.Mission.Faction);

                        if (!factionStats.PerSystemData.TryGetValue(ms.OriginatingSystem.Name.ToLowerInvariant(), out FactionResults.PerSystem entry))
                            entry = factionStats.PerSystemData[ms.OriginatingSystem.Name.ToLowerInvariant()] = new FactionResults.PerSystem() { System = ms.OriginatingSystem };

                        if (ms.Completed != null)           // effects/rewards are dependent on completion
                        {
                            entry.Missions++;

                            if (ms.Completed.FactionEffects != null)
                            {
                                foreach (var fe in ms.Completed.FactionEffects)
                                {
                                    if (fe.Faction == ms.Mission.Faction)
                                    {
                                        if (fe.ReputationTrend == "UpGood" && fe.Reputation?.Length > 0)
                                        {
                                            entry.Reputation += fe.Reputation.Length;
                                        }

                                        foreach (var si in fe.Influence)
                                        {
                                            if (si.Trend == "UpGood" && si.Influence?.Length > 0)
                                            {
                                                entry.Influence += si.Influence.Length;
                                            }
                                        }
                                    }
                                }
                            }

                            long credits = ms.Completed.Reward != null ? (long)ms.Completed.Reward : 0;
                            if (credits > 0)
                            {
                                entry.MissionCredits += credits;
                            }
                            if (ms.Completed.MaterialsReward != null)
                            {
                                foreach (var mr in ms.Completed.MaterialsReward)
                                {
                                    entry.AddMaterialCommodityReward(mr.Name_Localised, mr.Count);
                                }
                            }
                            if (ms.Completed.CommodityReward != null)
                            {
                                foreach (var cr in ms.Completed.CommodityReward)
                                {
                                    entry.AddMaterialCommodityReward(cr.Name_Localised, cr.Count);
                                }
                            }
                        }
                        else if (withinexpirytime && ms.State == MissionState.StateTypes.InProgress)
                        {
                            entry.MissionsInProgress++;
                        }
                    }

                }
            }  // end ml

            // if we were not passed stats, then we need to calculate it over the period

            if ( statsinfo == null )    // don't have it, so we need to recalc
            { 
                statsinfo = new Stats();      // reprocess this list completely

                foreach (var he in HistoryList.FilterByDateRange(hist.EntryOrder(), startdateutc, enddateutc))
                {
                    statsinfo.Process(he.journalEntry, he.System, he.Status.StationFaction);
                }
            }

            // if we have some stats on factions accumulated via the history, add to the faction list

            if (statsinfo != null)
            {
                foreach (var fkvp in statsinfo.FactionData)    // for all factions in statsinfo
                {
                    if (!FactionList.TryGetValue(fkvp.Value.Faction, out FactionResults factionStats))   // is faction present? if not create
                        factionStats = FactionList[fkvp.Value.Faction] = new FactionResults(fkvp.Value.Faction);

                    factionStats.FactionStats = fkvp.Value;        // set the FactionStats to the statsinfo
                }
            }

            return FactionList;
        }
    }
}
