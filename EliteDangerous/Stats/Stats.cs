/*
 * Copyright © 2021 EDDiscovery development team
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
 
using BaseUtils;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class Stats
    {
        public class FactionInfo
        {
            public FactionInfo(string f) { Faction = f; }
            public FactionInfo(FactionInfo other)
            {
                Faction = other.Faction;
                BoughtCommodity = other.BoughtCommodity; SoldCommodity = other.SoldCommodity; ProfitCommodity = other.ProfitCommodity;
                BoughtMaterial = other.BoughtMaterial; SoldMaterial = other.SoldMaterial;
                BountyKill = other.BountyKill;
                BountyRewards = other.BountyRewards;
                BountyRewardsValue = other.BountyRewardsValue;
                CapShipAwardAsAwaringFaction = other.CapShipAwardAsAwaringFaction;
                CapShipAwardAsAwaringFactionValue = other.CapShipAwardAsAwaringFactionValue;
                CapShipAwardAsVictimFaction = other.CapShipAwardAsVictimFaction;
                CrimesAgainst = other.CrimesAgainst;
                Interdicted = other.Interdicted;
                Interdiction = other.Interdiction;
                KillBondAwardAsAwaringFaction = other.KillBondAwardAsAwaringFaction;
                KillBondAwardAsAwaringFactionValue = other.KillBondAwardAsAwaringFactionValue;
                KillBondAwardAsVictimFaction = other.KillBondAwardAsVictimFaction;
            }

            public string Faction { get; set; }
            public int BoughtCommodity { get; set; }
            public int SoldCommodity { get; set; }
            public long ProfitCommodity { get; set; }
            public int BoughtMaterial { get; set; }
            public int SoldMaterial { get; set; }
            public int BountyKill { get; set; }
            public int BountyRewards { get; set; }
            public long BountyRewardsValue { get; set; }
            public int CapShipAwardAsAwaringFaction { get; set; }
            public long CapShipAwardAsAwaringFactionValue { get; set; }
            public int CapShipAwardAsVictimFaction { get; set; }
            public int CrimesAgainst { get; set; }
            public int Interdicted { get; set; } // how many times been intercepted
            public int Interdiction { get; set; }
            public int KillBondAwardAsAwaringFaction { get; set; }
            public long KillBondAwardAsAwaringFactionValue { get; set; }
            public int KillBondAwardAsVictimFaction { get; set; }
        }

        private GenerationalDictionary<string, FactionInfo> history;

        public Stats()
        {
            history = new GenerationalDictionary<string, FactionInfo>();
        }

        public Stats(Stats other)
        {
            history = other.history;
        }

        public Dictionary<string, FactionInfo> GetAtGeneration(uint g)
        {
            return history.Get(g);
        }

        public Dictionary<string, FactionInfo> GetLastEntries()
        {
            return history.GetLast();
        }

        private FactionInfo Clone(string faction, bool incrgen = true)               // clone both FactionInformation structure and a faction
        {
            if (faction.HasChars() && faction != "$faction_none;")
            {
                FactionInfo newfi = history.GetLast(faction);        // get the last one, or null
                newfi = newfi != null ? new FactionInfo(newfi) : new FactionInfo(faction);  // make a new copy, or an empty copy
                if ( incrgen )
                    history.NextGeneration();
                history[faction] = newfi;                    // add this new one to the history list
                return newfi;
            }
            else
                return null;
        }

        public void UpdateCommodity(string name, int amount, long profit, string faction)
        {
            var newfi = Clone(faction);
            if (newfi != null)
            {
//                if (faction == "89 Leonis Republic Party") { } // debug
                if (amount < 0)
                    newfi.SoldCommodity += -amount;
                else
                    newfi.BoughtCommodity += amount;

                newfi.ProfitCommodity += profit;
            }
        }

        public void UpdateMaterial(string name, int amount, string faction)
        {
            var newfi = Clone(faction);
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
            var newfi = Clone(victimfaction);
            if (newfi != null)
            {
                newfi.BountyKill++;
            }
        }

        public void BountyRewards(string victimfaction, long reward)
        {
            var newfi = Clone(victimfaction);
            if (newfi != null)
            {
                newfi.BountyRewards++;
                newfi.BountyRewardsValue += reward;
            }
        }

        public void CapShipAward(string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = Clone(victimfaction);
            if (vnewfi != null)
            {
                var anewfi = Clone(awardingfaction,false);      // not a new generation, part of this generation
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
            var newfi = Clone(againstfaction);
            if (newfi != null)
            {
                newfi.CrimesAgainst++;
            }
        }

        public void Interdicted(string byfaction)
        {
            var newfi = Clone(byfaction);
            if (newfi != null)
            {
                newfi.Interdicted++;
            }
        }
        public void Interdiction(string onfaction)
        {
            var newfi = Clone(onfaction);
            if (newfi != null)
            {
                newfi.Interdiction++;
            }
        }

        public void KillBond(string awardingfaction, string victimfaction, long reward)
        {
            var vnewfi = Clone(victimfaction);
            if (vnewfi != null)
            {
                var anewfi = Clone(awardingfaction,false);
                if (anewfi != null)
                {
                    anewfi.KillBondAwardAsAwaringFaction++;
                    anewfi.KillBondAwardAsAwaringFactionValue += reward;
                    vnewfi.KillBondAwardAsVictimFaction++;
                }
            }
        }

        public uint Process(JournalEntry je, string stationfaction)
        {
            if (je is IStatsJournalEntry)
            {
                ((IStatsJournalEntry)je).UpdateStats(this, stationfaction);
            }

            return history.Generation;
        }
    }
}
