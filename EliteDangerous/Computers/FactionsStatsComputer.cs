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
    // this accumulates mission events and information from Stats.FactionStatsitic on a per faction basis

    public class FactionStats
    {
        public class MissionReward
        {
            public string Name { get; }
            public int Count { get; private set; }

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

        public class SystemInfluence
        {
            public long SystemAddress { get; }
            public int Influence { get; private set; }
            public int Missions { get { return MissionsList.Count; } }
            private List<ulong> MissionsList;

            public SystemInfluence(long systemAddress, int influence, ulong missionId)
            {
                this.SystemAddress = systemAddress;
                this.Influence = influence;
                this.MissionsList = new List<ulong>();
                this.MissionsList.Add(missionId);
            }

            public void AddInfluence(int influence, ulong missionId)
            {
                this.Influence += influence;
                if (!this.MissionsList.Contains(missionId))
                {
                    this.MissionsList.Add(missionId);
                }
            }
        }

        public class FactionStatistics
        {
            public string Name { get; }
            public int TotalMissions { get; private set; }
            public int MissionsInProgress { get; private set; }
            public int Influence { get; private set; }
            public int Reputation { get; private set; }
            public long MissionCredits { get; private set; }
            public Dictionary<string, MissionReward> MissionRewards { get; }
            public Dictionary<long, SystemInfluence> MissionSystemsWithInfluence { get; }

            // these are the faction statsS
            public Stats.FactionStatistics FactionStats { get; private set; }

            public FactionStatistics(string name)
            {
                Name = name;
                TotalMissions = 0;
                Influence = 0;
                Reputation = 0;
                MissionCredits = 0;
                MissionRewards = new Dictionary<string, MissionReward>();
                this.MissionSystemsWithInfluence = new Dictionary<long, SystemInfluence>();
                FactionStats = defstats;
            }

            private static Stats.FactionStatistics defstats = new Stats.FactionStatistics("-");

            public void AddMissions(int amount)
            {
                TotalMissions += amount;
            }

            public void AddMissionsInProgress(int amount)
            {
                MissionsInProgress += amount;
            }

            public void AddInfluence(int amount)
            {
                Influence += amount;
            }

            public void AddReputation(int amount)
            {
                Reputation += amount;
            }

            public void AddMissionCredits(long amount)
            {
                MissionCredits += amount;
            }

            public void AddFactionStats(Stats.FactionStatistics mc)
            {
                FactionStats = mc;
            }

            public void AddReward(string name, int amount)
            {
                MissionReward reward;
                if (!MissionRewards.TryGetValue(name, out reward))
                {
                    reward = new MissionReward(name);
                    MissionRewards.Add(name, reward);
                }
                reward.Add(amount);
            }

            public void AddSystemInfluence(long systemAddress, int amount, ulong missionId)
            {
                SystemInfluence si;
                if (!MissionSystemsWithInfluence.TryGetValue(systemAddress, out si))
                {
                    si = new SystemInfluence(systemAddress, amount, missionId);
                    MissionSystemsWithInfluence.Add(systemAddress, si);
                    //System.Diagnostics.Debug.WriteLine($"Faction sys influence made for {systemAddress}");
                }
                else
                {
                    si.AddInfluence(amount, missionId);
                }
            }
        }

        public static Dictionary<string, FactionStatistics> Compute(List<MissionState> ml, Dictionary<string, Stats.FactionStatistics> factioninfo,
                                                            HistoryList hist, DateTime startdateutc, DateTime enddateutc)
        {
            var FactionList = new Dictionary<string, FactionStatistics>();

            if ( ml != null )
            {
                foreach (MissionState ms in ml)
                {
                    bool withinstarttime = DateTime.Compare(ms.Mission.EventTimeUTC, startdateutc) >= 0 && DateTime.Compare(ms.Mission.EventTimeUTC, enddateutc) <= 0;
                    bool withinexpirytime = ms.Mission.ExpiryValid && DateTime.Compare(ms.Mission.Expiry, startdateutc) >= 0 && DateTime.Compare(ms.Mission.Expiry, enddateutc) <= 0;
                    bool withincompletetime = ms.Completed != null && DateTime.Compare(ms.Completed.EventTimeUTC, startdateutc) >= 0 && DateTime.Compare(ms.Completed.EventTimeUTC, enddateutc) <= 0;

                    if (withinstarttime || withincompletetime)
                    {
                        var faction = ms.Mission.Faction;
                        FactionStatistics factionStats;
                        if (!FactionList.TryGetValue(faction, out factionStats))   // is faction present? if not create
                        {
                            factionStats = new FactionStatistics(faction);
                            FactionList.Add(faction, factionStats);
                        }

                        if (ms.Completed != null)           // effects/rewards are dependent on completion
                        {
                            factionStats.AddMissions(1);        // 1 more mission, 

                            if (ms.Completed.FactionEffects != null)
                            {
                                foreach (var fe in ms.Completed.FactionEffects)
                                {
                                    if (fe.Faction == faction)
                                    {
                                        if (fe.ReputationTrend == "UpGood" && fe.Reputation?.Length > 0)
                                        {
                                            factionStats.AddReputation(fe.Reputation.Length);
                                        }

                                        foreach (var si in fe.Influence)
                                        {
                                            if (si.Trend == "UpGood" && si.Influence?.Length > 0)
                                            {
                                                factionStats.AddInfluence(si.Influence.Length);
                                                factionStats.AddSystemInfluence(si.SystemAddress, si.Influence.Length, ms.Completed.MissionId);
                                            }
                                        }
                                    }
                                }
                            }
                            long credits = ms.Completed.Reward != null ? (long)ms.Completed.Reward : 0;
                            if (credits > 0)
                            {
                                factionStats.AddMissionCredits(credits);
                            }
                            if (ms.Completed.MaterialsReward != null)
                            {
                                foreach (var mr in ms.Completed.MaterialsReward)
                                {
                                    factionStats.AddReward(mr.Name_Localised, mr.Count);
                                }
                            }
                            if (ms.Completed.CommodityReward != null)
                            {
                                foreach (var cr in ms.Completed.CommodityReward)
                                {
                                    factionStats.AddReward(cr.Name_Localised, cr.Count);
                                }
                            }
                        }
                        else if (withinexpirytime && ms.State == MissionState.StateTypes.InProgress)
                        {
                            factionStats.AddMissionsInProgress(1);
                        }
                    }

                }
            }  // end ml

            // if we were not passed factioninfo stats, then we need to calculate it over the period

            if ( factioninfo == null )    // don't have it, so we need to recalc
            { 
                var stats = new Stats();      // reprocess this list completely

                foreach (var he in HistoryList.FilterByDateRange(hist.EntryOrder(), startdateutc, enddateutc))
                {
                    stats.Process(he.journalEntry, he.Status.StationFaction);
                }

                factioninfo = stats.GetLastEntries(); // pick the last generation in there.
            }

            // if we have some stats on factions accumulated via the history, add to the faction list

            if (factioninfo != null)
            {
                foreach (var fkvp in factioninfo)
                {
                    if (!FactionList.TryGetValue(fkvp.Value.Faction, out FactionStatistics factionStats))  // is faction present? if not create
                    {
                        factionStats = new FactionStatistics(fkvp.Value.Faction);
                        FactionList.Add(fkvp.Value.Faction, factionStats);
                    }

                    FactionList[fkvp.Value.Faction].AddFactionStats(fkvp.Value);
                }
            }

            return FactionList;
        }

    }
}
