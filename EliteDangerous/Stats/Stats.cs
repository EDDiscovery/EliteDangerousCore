using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                BoughtCommodity = other.BoughtCommodity; SoldCommodity = other.SoldCommodity; BoughtMaterial = other.BoughtMaterial; SoldMaterial = other.SoldMaterial;
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

        public Dictionary<string, FactionInfo> FactionInformation { get; private set; }

        public Stats()
        {
            FactionInformation = new Dictionary<string, FactionInfo>();
        }

        public Stats(Stats other)
        {
            FactionInformation = other.FactionInformation;      // not changed yet, still pointing to the same dictionary
        }

        private FactionInfo Clone(string faction)               // clone both FactionInformation structure and a faction
        {
            if (faction.HasChars() && faction != "$faction_none;")
            {
                FactionInformation = new Dictionary<string, FactionInfo>(FactionInformation);       // create a copy so we can modify
                var newfi = FactionInformation.ContainsKey(faction) ? new FactionInfo(FactionInformation[faction]) : new FactionInfo(faction);
                FactionInformation[faction] = newfi;
                return newfi;
            }
            else
                return null;
        }

        private FactionInfo CloneFaction(string faction)        // clone only a Faction
        {
            if (faction.HasChars() && faction != "$faction_none;")
            {
                var newfi = FactionInformation.ContainsKey(faction) ? new FactionInfo(FactionInformation[faction]) : new FactionInfo(faction);
                FactionInformation[faction] = newfi;
                return newfi;
            }
            else
                return null;
        }

        public void UpdateCommodity(string name, int amount, string faction)
        {
            var newfi = Clone(faction);
            if (newfi != null)
            {
                if (amount < 0)
                    newfi.SoldCommodity += -amount;
                else
                    newfi.BoughtCommodity += amount;
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
            UpdateCommodity(namematcom, amount, name);
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
                var anewfi = CloneFaction(awardingfaction);
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
                var anewfi = CloneFaction(awardingfaction);
                if (anewfi != null)
                {
                    anewfi.KillBondAwardAsAwaringFaction++;
                    anewfi.KillBondAwardAsAwaringFactionValue += reward;
                    vnewfi.KillBondAwardAsVictimFaction++;
                }
            }
        }

        public static Stats Process(JournalEntry je, Stats prev, string stationfaction)
        {
            if (je is IStats)
            {
                Stats news = prev != null ? new Stats(prev) : new Stats();
                ((IStats)je).UpdateStats(news, stationfaction);
                return news;
            }
            else
                return prev ?? new Stats();
        }
    }
}
