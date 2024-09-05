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
        // Data for a faction accumulated by the computer - mostly from Stats.FactionStatistics
        // adding in mission events 

        public class FactionResults
        {
            public string Name { get; }

            // mission info

            public int TotalMissions { get; private set; }
            public int MissionsInProgress { get; private set; }
            public int Influence { get; private set; }
            public int Reputation { get; private set; }
            public long MissionCredits { get; private set; }
            public Dictionary<string, MissionReward> MissionRewards { get; }
            public Dictionary<long, SystemInfluence> MissionSystemsWithInfluence { get; }       // by system address, systems with influence set
            
            // other stats from FactionStatistics
            public Stats.FactionStatistics FactionStats { get; private set; }

            public FactionResults(string name)
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
        }

       // Compute Stats from the mission list (total), statsinfo (pass in latest)
       // will be filtered by start/end date
   
       public static Dictionary<string, FactionResults> Compute(List<MissionState> ml,   // total mission list
                                                            Stats statsinfo, // if passed in null, then this will compute it. If passed in, uses this to populate FactionStats above
                                                            HistoryList hist,   // total history
                                                            DateTime startdateutc, DateTime enddateutc) // bounding times, must be set up
        {
            var FactionList = new Dictionary<string, FactionResults>();

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
                        var faction = ms.Mission.Faction;
                        FactionResults factionStats;
                        if (!FactionList.TryGetValue(faction, out factionStats))   // is faction present? if not create
                        {
                            factionStats = new FactionResults(faction);
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
                                    factionStats.AddMaterialCommodityReward(mr.Name_Localised, mr.Count);
                                }
                            }
                            if (ms.Completed.CommodityReward != null)
                            {
                                foreach (var cr in ms.Completed.CommodityReward)
                                {
                                    factionStats.AddMaterialCommodityReward(cr.Name_Localised, cr.Count);
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

            // if we were not passed stats, then we need to calculate it over the period

            if ( statsinfo == null )    // don't have it, so we need to recalc
            { 
                statsinfo = new Stats();      // reprocess this list completely

                foreach (var he in HistoryList.FilterByDateRange(hist.EntryOrder(), startdateutc, enddateutc))
                {
                    statsinfo.Process(he.journalEntry, he.Status.StationFaction);
                }
            }

            // if we have some stats on factions accumulated via the history, add to the faction list

            if (statsinfo != null)
            {
                foreach (var fkvp in statsinfo.GetFactionData())    // for all factions in statsinfo
                {
                    if (!FactionList.ContainsKey(fkvp.Value.Faction)) // is faction present? if not create
                    {
                        var factionStats = new FactionResults(fkvp.Value.Faction);       // make an empty FactionStats of our class and add it to the faction list
                        FactionList.Add(fkvp.Value.Faction, factionStats);
                    }

                    FactionList[fkvp.Value.Faction].AddFactionStats(fkvp.Value);        // set the FactionStats to the statsinfo
                }
            }

            return FactionList;
        }

        // Collect data from sources for presentation in a system summary

        //public class SystemInfo
        //{
        //    public string Name { get; set; }
        //    public long? Address { get; set; }

        //    public GovernmentDefinitions.Government Government { get; set; }
        //    public AllegianceDefinitions.Allegiance Allegiance { get; set; }
        //    public int? Influence { get; set; }
        //    public int? Missions { get; set; }
        //    public int? CommoditiesSold { get; private set; }
        //    public int? CommoditiesBought { get; private set; }
        //    public int? MaterialsSold { get; private set; }
        //    public int? MaterialsBought { get; private set; }
        //    public int? Bounties { get; private set; }
        //    public long? BountyRewardsValue { get; private set; }
        //    public int? KillBonds { get; private set; }
        //    public long? BondsRewardsValue { get; private set; }
        //    public long? CartographicValue { get; private set; }

        //    public void AddCommoditiesSold(int a) { CommoditiesSold = (CommoditiesSold ?? 0) + a; }
        //    public void AddCommoditiesBought(int a) { CommoditiesBought = (CommoditiesBought ?? 0) + a; }
        //    public void AddMaterialsSold(int a) { MaterialsSold = (MaterialsSold ?? 0) + a; }
        //    public void AddMaterialsBought(int a) { MaterialsBought = (MaterialsBought ?? 0) + a; }
        //    public void AddBounties(int a) { Bounties = (Bounties ?? 0) + a; }
        //    public void AddBountyRewardsValue(long a) { BountyRewardsValue = (BountyRewardsValue ?? 0) + a; }
        //    public void AddKillBonds(int a) { KillBonds = (KillBonds ?? 0) + a; }
        //    public void AddBondsRewardsValue(long a) { BondsRewardsValue = (BondsRewardsValue ?? 0) + a; }
        //    public void AddCartographicValue(long a) { CartographicValue = (CartographicValue ?? 0) + a; }
        //}

        //public static List<SystemInfo> ComputeSystemView(FactionResults fs, HistoryList hist,
        //                                         DateTime startdateutc, DateTime enddateutc) // bounding times, must be set up
        //{
        //    var systems = new List<SystemInfo>();

        //    var helistindaterange = HistoryList.FilterByDateRange(hist.EntryOrder(), startdateutc, enddateutc);     // get all within range

        //    // first look at mission influences

        //    foreach (var si in fs.MissionSystemsWithInfluence.Values)
        //    {
        //        var he = helistindaterange.Find(x => x.System.SystemAddress == si.SystemAddress);       // find an HEs with the same system address, and if present, add it to the list
        //        if (he != null)
        //            systems.Add(new SystemInfo { Name = he.System.Name, Address = si.SystemAddress, Missions = si.Missions, Influence = si.Influence });
        //    }

        //    // then lets look at FactionStats faction info per system
        //    foreach (var kvp in fs.FactionStats.FactionInfoPerSystem)         // protect, but should always be set.
        //    {
        //        SystemInfo si = systems.Find(x =>           // do we have this previous entry?
        //            (kvp.Value.System.SystemAddress != null && x.Address == kvp.Value.System.SystemAddress) ||
        //            (kvp.Value.System.Name != null && x.Name == kvp.Value.System.Name));

        //        if (si == null)
        //        {
        //            si = new SystemInfo { Name = kvp.Value.System.Name, Address = kvp.Value.System.SystemAddress };
        //            systems.Add(si);
        //        }

        //        si.Government = kvp.Value.Government;
        //        si.Allegiance = kvp.Value.Allegiance;
        //    }

        //    // now look at all history entries within the date range which are of these types and have the right faction

        //    var list = HistoryList.FilterByDateRange(hist.EntryOrder(), startdateutc, enddateutc,x =>
        //              (x.journalEntry is IStatsJournalEntryMatCommod && x.Status.StationFaction == fs.Name) ||  // he's with changes in stats due to MatCommod trading
        //              (x.journalEntry is IStatsJournalEntryBountyOrBond && (x.journalEntry as IStatsJournalEntryBountyOrBond).HasFaction(fs.Name)) ||  // he's with Bountry/bond
        //              ((x.journalEntry.EventTypeID == JournalTypeEnum.SellExplorationData || x.journalEntry.EventTypeID == JournalTypeEnum.MultiSellExplorationData) && x.Status.StationFaction == fs.Name)// he's for exploration
        //              );

        //    foreach (var he in list)
        //    {
        //        SystemInfo si = systems.Find(x =>           // do we have this previous entry?
        //            (he.System.SystemAddress != null && x.Address == he.System.SystemAddress) ||
        //            (he.System.Name != null && x.Name == he.System.Name));

        //        if (si == null)     // no, add it to the system list
        //        {
        //            si = new SystemInfo { Name = he.System.Name, Address = he.System.SystemAddress };
        //            systems.Add(si);
        //        }

        //        if (he.journalEntry is IStatsJournalEntryMatCommod)         // is this a material or commodity trade?
        //        {
        //            var items = (he.journalEntry as IStatsJournalEntryMatCommod).ItemsList;
        //            foreach (var i in items)
        //            {
        //                if (he.journalEntry.EventTypeID == JournalTypeEnum.MaterialTrade)       // material trade is only counter for mats
        //                {
        //                    if (i.Count > 0)
        //                        si.AddMaterialsBought(i.Count);
        //                    else if (i.Count < 0)
        //                        si.AddMaterialsSold(-i.Count);
        //                }
        //                else
        //                {                                               // all others are commds
        //                    if (i.Count > 0)
        //                        si.AddCommoditiesBought(i.Count);
        //                    else
        //                        si.AddCommoditiesSold(-i.Count);        // value is negative, invert
        //                }
        //            }
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Faction {fs.Name} Journal entry {he.journalEntry.EventTypeStr} {he.System.Name}");

        //            if (he.journalEntry.EventTypeID == JournalTypeEnum.Bounty)
        //            {
        //                si.AddBounties(1);
        //                si.AddBountyRewardsValue((he.journalEntry as IStatsJournalEntryBountyOrBond).FactionReward(fs.Name));
        //            }
        //            else if (he.journalEntry.EventTypeID == JournalTypeEnum.FactionKillBond)
        //            {
        //                si.AddKillBonds(1);
        //                si.AddBondsRewardsValue((he.journalEntry as IStatsJournalEntryBountyOrBond).FactionReward(fs.Name));
        //            }
        //            else if (he.journalEntry.EventTypeID == JournalTypeEnum.SellExplorationData)
        //            {
        //                si.AddCartographicValue((he.journalEntry as EliteDangerousCore.JournalEvents.JournalSellExplorationData).TotalEarnings);
        //            }
        //            else if (he.journalEntry.EventTypeID == JournalTypeEnum.MultiSellExplorationData)
        //            {
        //                si.AddCartographicValue((he.journalEntry as EliteDangerousCore.JournalEvents.JournalMultiSellExplorationData).TotalEarnings);
        //            }
        //        }
        //    }


        //    return systems;
        //}

    }
}
