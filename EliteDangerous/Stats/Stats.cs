/*
 * Copyright 2021-2024 EDDiscovery development team
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
 
using BaseUtils;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    // generic class to hold Stats accumulated during journal reading
    // used for FactionStats, but can be used for other stats
    public class Stats
    {
        public class FactionStatistics
        {
            public FactionStatistics(string f) { Faction = f; }
            public string Faction { get; set; }     // faction name
            public int BoughtCommodity { get; set; }
            public int SoldCommodity { get; set; }
            public long ProfitCommodity { get; set; }
            public int BoughtMaterial { get; set; }
            public int SoldMaterial { get; set; }
            public int BountyKill { get; set; }
            public int BountyRewards { get; set; }
            public long BountyRewardsValue { get; set; }
            public long RedeemVoucherValue { get; set; }
            public long FineValue { get; set; }
            public long PayBountyValue { get; set; }
            public int CapShipAwardAsAwaringFaction { get; set; }
            public long CapShipAwardAsAwaringFactionValue { get; set; }
            public int CapShipAwardAsVictimFaction { get; set; }
            public int CrimesAgainst { get; set; }
            public int Interdicted { get; set; } // how many times been intercepted
            public int Interdiction { get; set; }
            public int KillBondAwardAsAwaringFaction { get; set; }
            public long KillBondAwardAsAwaringFactionValue { get; set; }
            public int KillBondAwardAsVictimFaction { get; set; }
            public long CartographicDataSold { get; set; }
            public long DataLinkAwardAsPayeeFaction { get; set; }
            public long DataLinkAwardAsPayeeFactionValue { get; set; }
            public long DataLinkAwardAsVictimFaction { get; set; }
            public double? LastReputation { get; set; }
            public Dictionary<string, FactionDefinitions.FactionInformation> FactionInfoPerSystem { get; set; } = new Dictionary<string, FactionDefinitions.FactionInformation>();  // per system, faction info from fsdjump/loc
            public class FactionDocked
            {
                public DateTime UTC { get; set; }
                public string System { get; set; }
                public FactionDefinitions.State State { get; set; }
            }

            public Dictionary<string, FactionDocked> FactionInfoPerStation { get; set; } = new Dictionary<string, FactionDocked>();  // per station, faction info from docked
        }

        private Dictionary<string, FactionStatistics> factions = new Dictionary<string, FactionStatistics>();

        public Stats()
        {
        }

        // faction data, never null, may be empty!
        public Dictionary<string,FactionStatistics> GetFactionData() => factions;
        // may return null
        public FactionStatistics GetFaction(string faction)
        {
            return factions.ContainsKey(faction) ?  factions[faction] : null;
        }

        public void UpdateCommodity(string name, int amount, long profit, string faction)
        {
            var newfi = GetOrMake(faction);
            if (newfi != null)
            {
                if (amount < 0)
                    newfi.SoldCommodity += -amount;
                else
                    newfi.BoughtCommodity += amount;

                newfi.ProfitCommodity += profit;
            }
        }

        public void UpdateMaterial(string name, int amount, string faction)
        {
            var newfi = GetOrMake(faction);
            if (newfi != null)
            {
                if (amount < 0)
                    newfi.SoldMaterial += -amount;
                else
                    newfi.BoughtMaterial += amount;
            }
        }

        public void UpdateEngineerMaterial(string name, string namematcom, int amount)
        {
            UpdateMaterial(namematcom, amount, name);
        }

        public void UpdateEngineerCommodity(string name, string namematcom, int amount)
        {
            UpdateCommodity(namematcom, amount, 0, name);
        }

        public void BountyKill(string victimfaction)
        {
            var newfi = GetOrMake(victimfaction);
            if (newfi != null)
            {
                newfi.BountyKill++;
            }
        }

        public void BountyRewards(string victimfaction, long reward)
        {
            var newfi = GetOrMake(victimfaction);
            if (newfi != null)
            {
                newfi.BountyRewards++;
                newfi.BountyRewardsValue += reward;
            }
        }

        public void CapShipAward(string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction);
            if (vnewfi != null)
            {
                var anewfi = GetOrMake(awardingfaction);      // not a new generation, part of this generation
                if (anewfi != null)
                {
                    anewfi.CapShipAwardAsAwaringFaction++;
                    anewfi.CapShipAwardAsAwaringFactionValue += reward;
                    vnewfi.CapShipAwardAsVictimFaction++;
                }
            }
        }

        public void CommitCrime(string againstfaction)
        {
            var newfi = GetOrMake(againstfaction);
            if (newfi != null)
            {
                newfi.CrimesAgainst++;
            }
        }

        public void Interdicted(string byfaction)
        {
            var newfi = GetOrMake(byfaction);
            if (newfi != null)
            {
                newfi.Interdicted++;
            }
        }
        public void Interdiction(string onfaction)
        {
            var newfi = GetOrMake(onfaction);
            if (newfi != null)
            {
                newfi.Interdiction++;
            }
        }

        public void KillBond(string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction);
            if (vnewfi != null)
            {
                var anewfi = GetOrMake(awardingfaction);
                if (anewfi != null)
                {
                    anewfi.KillBondAwardAsAwaringFaction++;
                    anewfi.KillBondAwardAsAwaringFactionValue += reward;
                    vnewfi.KillBondAwardAsVictimFaction++;
                }
            }
        }

        public void DataLinkVoucher(string victimfaction, string payeefaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction);      // if victimfaction = empty string, its a null, and we don't store. Some events have empty victim factions
            if (vnewfi != null)
                vnewfi.DataLinkAwardAsVictimFaction++;
            else
            { }

            var anewfi = GetOrMake(payeefaction);       // don't incr generation if we made a victim faction

            if (anewfi != null)
            {
                anewfi.DataLinkAwardAsPayeeFaction++;
                anewfi.DataLinkAwardAsPayeeFactionValue += reward;
            }
        }

        public void CartographicSold(string faction, long value)
        {
            var vnewfi = GetOrMake(faction);
            if (vnewfi != null)
            {
                vnewfi.CartographicDataSold += value;
            }
        }
        public void PayBounties(string faction, long value)
        {
            var vnewfi = GetOrMake(faction);
            if (vnewfi != null)
            {
                vnewfi.PayBountyValue += value;
            }

        }

        public void PayFines(string faction, long value)
        {
            var vnewfi = GetOrMake(faction);
            if (vnewfi != null)
            {
                vnewfi.FineValue += value;
            }
        }

        public void RedeemVoucher(string faction, long value)
        {
            var vnewfi = GetOrMake(faction);
            if (vnewfi != null)
            {
                vnewfi.RedeemVoucherValue += value;
            }
        }
        
        public void UpdateFactions(JournalEvents.JournalLocOrJump locorjump)
        {
            foreach( var f in locorjump.Factions)
            {
                var vnewfi = GetOrMake(f.Name);
                if ( vnewfi != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"Update faction info {f.Name} at {locorjump.StarSystem} : {f.FactionState} {f.Government} {f.Influence} {f.Allegiance} {f.MyReputation}");
                    vnewfi.FactionInfoPerSystem[locorjump.StarSystem.ToLowerInvariant()] = f;
                    if (f.MyReputation.HasValue)        // only update if set..
                        vnewfi.LastReputation = f.MyReputation;
                }
            }
        }

        public void Docking(JournalEvents.JournalDocked docked)
        {
            var vnewfi = GetOrMake(docked.Faction);
            if ( vnewfi != null )
            {
                //System.Diagnostics.Debug.WriteLine($"Stats Docked at {docked.EventTimeUTC} {docked.Faction} {docked.FactionState}");
                vnewfi.FactionInfoPerStation[docked.StationName.ToLowerInvariant()] = new FactionStatistics.FactionDocked { System = docked.StarSystem, UTC = docked.EventTimeUTC, State = docked.FactionState };
            }
        }

        public void Process(JournalEntry je, string stationfaction)
        {
            if (je is IStatsJournalEntry)
            {
                ((IStatsJournalEntry)je).UpdateStats(this, stationfaction);
            }
        }


        // get existing, or make new. null if its unhappy with name
        private FactionStatistics GetOrMake(string faction)
        {
            if (faction.HasChars() && faction != "$faction_none;")
            {
                if (!factions.TryGetValue(faction, out FactionStatistics fs))
                    fs = factions[faction] = new FactionStatistics(faction);
                return fs;
            }
            else
            {
                return null;
            }
        }


    }
}
