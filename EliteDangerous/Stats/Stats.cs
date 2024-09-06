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
            public double? LastReputation { get; set; }
            public Dictionary<string, PerSystem> PerSystemData { get; set; } = new Dictionary<string, PerSystem>();
            public class PerSystem
            {
                public ISystem System { get; set; }
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
                public int KillBondAwardAsAwardingFaction { get; set; }
                public long KillBondAwardAsAwardingFactionValue { get; set; }
                public int KillBondAwardAsVictimFaction { get; set; }
                public long CartographicDataSold { get; set; }
                public long OrganicDataSold { get; set; }
                public long DataLinkAwardAsPayeeFaction { get; set; }
                public long DataLinkAwardAsPayeeFactionValue { get; set; }
                public long DataLinkAwardAsVictimFaction { get; set; }
                public FactionDefinitions.FactionInformation FactionInfo { get; set; }
                public FactionDefinitions.State DockedState { get; set; }
            }
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

        public void UpdateCommodity(ISystem system, string name, int amount, long profit, string faction)
        {
            var newfi = GetOrMake(faction,system);
            if (newfi != null)
            {
                if (amount < 0)
                    newfi.SoldCommodity += -amount;
                else
                    newfi.BoughtCommodity += amount;

                newfi.ProfitCommodity += profit;
            }
        }

        public void UpdateMaterial(ISystem system, string name, int amount, string faction)
        {
            var newfi = GetOrMake(faction,system);
            if (newfi != null)
            {
                if (amount < 0)
                    newfi.SoldMaterial += -amount;
                else
                    newfi.BoughtMaterial += amount;
            }
        }

        public void UpdateEngineerMaterial(ISystem system, string name, string namematcom, int amount)
        {
            UpdateMaterial(system,namematcom, amount, name);
        }

        public void UpdateEngineerCommodity(ISystem system, string name, string namematcom, int amount)
        {
            UpdateCommodity(system,namematcom, amount, 0, name);
        }

        public void BountyKill(ISystem system, string victimfaction)
        {
            var newfi = GetOrMake(victimfaction, system);
            if (newfi != null)
            {
                newfi.BountyKill++;
            }
        }

        public void BountyRewards(ISystem system, string victimfaction, long reward)
        {
            var newfi = GetOrMake(victimfaction, system);
            if (newfi != null)
            {
                newfi.BountyRewards++;
                newfi.BountyRewardsValue += reward;
            }
        }

        public void CapShipAward(ISystem system, string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction,system);
            if (vnewfi != null)
            {
                var anewfi = GetOrMake(awardingfaction,system);      // not a new generation, part of this generation
                if (anewfi != null)
                {
                    anewfi.CapShipAwardAsAwaringFaction++;
                    anewfi.CapShipAwardAsAwaringFactionValue += reward;
                    vnewfi.CapShipAwardAsVictimFaction++;
                }
            }
        }

        public void CommitCrime(ISystem system, string againstfaction)
        {
            var newfi = GetOrMake(againstfaction,system);
            if (newfi != null)
            {
                newfi.CrimesAgainst++;
            }
        }

        public void Interdicted(ISystem system, string byfaction)
        {
            var newfi = GetOrMake(byfaction,system);
            if (newfi != null)
            {
                newfi.Interdicted++;
            }
        }
        public void Interdiction(ISystem system, string onfaction)
        {
            var newfi = GetOrMake(onfaction,system);
            if (newfi != null)
            {
                newfi.Interdiction++;
            }
        }

        public void KillBond(ISystem system, string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction,system);
            if (vnewfi != null)
            {
                var anewfi = GetOrMake(awardingfaction,system);
                if (anewfi != null)
                {
                    anewfi.KillBondAwardAsAwardingFaction++;
                    anewfi.KillBondAwardAsAwardingFactionValue += reward;
                    vnewfi.KillBondAwardAsVictimFaction++;
                }
            }
        }

        public void DataLinkVoucher(ISystem system, string victimfaction, string payeefaction, long reward)
        {
            var vnewfi = GetOrMake(victimfaction,system);      // if victimfaction = empty string, its a null, and we don't store. Some events have empty victim factions
            if (vnewfi != null)
                vnewfi.DataLinkAwardAsVictimFaction++;
            else
            { }

            var anewfi = GetOrMake(payeefaction,system);       // don't incr generation if we made a victim faction

            if (anewfi != null)
            {
                anewfi.DataLinkAwardAsPayeeFaction++;
                anewfi.DataLinkAwardAsPayeeFactionValue += reward;
            }
        }

        public void CartographicSold(ISystem system, string faction, long value)
        {
            var vnewfi = GetOrMake(faction, system);
            if (vnewfi != null)
            {
                vnewfi.CartographicDataSold += value;
            }
        }
        public void OrganicDataSold(ISystem system, string faction, long value)
        {
            var vnewfi = GetOrMake(faction, system);
            if (vnewfi != null)
            {
                vnewfi.OrganicDataSold += value;
            }
        }
        public void PayBounties(ISystem system, string faction, long value)
        {
            var vnewfi = GetOrMake(faction,system);
            if (vnewfi != null)
            {
                vnewfi.PayBountyValue += value;
            }

        }

        public void PayFines(ISystem system, string faction, long value)
        {
            var vnewfi = GetOrMake(faction,system);
            if (vnewfi != null)
            {
                vnewfi.FineValue += value;
            }
        }

        public void RedeemVoucher(ISystem system, string faction, long value)
        {
            var vnewfi = GetOrMake(faction,system);
            if (vnewfi != null)
            {
                vnewfi.RedeemVoucherValue += value;
            }
        }
        
        public void UpdateFactions(ISystem system, JournalEvents.JournalLocOrJump locorjump)
        {
            foreach( var f in locorjump.Factions)
            {
                var vnewfi = GetOrMake(f.Name,system);
                if (vnewfi != null)
                {
                    vnewfi.FactionInfo = f;
                    factions[f.Name].LastReputation = f.MyReputation;
                }
            }
        }

        public void Docking(ISystem system, JournalEvents.JournalDocked docked)
        {
            var vnewfi = GetOrMake(docked.Faction,system);
            if ( vnewfi != null )
            {
                vnewfi.DockedState = docked.FactionState;
            }
        }

        public void Process(JournalEntry je, ISystem system, string stationfaction)
        {
            if (je is IStatsJournalEntry)
            {
                ((IStatsJournalEntry)je).UpdateStats(this, system, stationfaction);
            }
        }


        // get existing, or make new FactionStatistics. null if its unhappy with name
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

        // get existing, or make new PerSystemData. null if its unhappy with name
        private FactionStatistics.PerSystem GetOrMake(string faction, ISystem system)
        {
            var newfi = GetOrMake(faction);
            if ( newfi != null)
            {
                if (!newfi.PerSystemData.TryGetValue(system.Name.ToLowerInvariant(), out FactionStatistics.PerSystem value))
                {
                    value = newfi.PerSystemData[system.Name.ToLowerInvariant()] = new FactionStatistics.PerSystem();
                    value.System = system;
                }
                return value;
            }
            return null;
        }


    }
}
