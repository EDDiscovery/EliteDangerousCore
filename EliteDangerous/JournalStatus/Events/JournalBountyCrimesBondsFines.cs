/*
 * Copyright © 2016-2023 EDDiscovery development team
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
 *
 */

using QuickJSON;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Bounty)]
    public class JournalBounty : JournalEntry,  IStatsJournalEntry, IStatsJournalEntryBountyOrBond
    {
        public class BountyReward
        {
            public string Faction;
            public string Faction_Localised;
            public long Reward;
        }

        public JournalBounty(JObject evt) : base(evt, JournalTypeEnum.Bounty)
        {
            TotalReward = evt["TotalReward"].Long();     // others of them..

            VictimFaction = evt["VictimFaction"].Str();
            VictimFactionLocalised = JournalFieldNaming.CheckLocalisation(evt["VictimFaction_Localised"].Str(), VictimFaction);

            SharedWithOthers = evt["SharedWithOthers"].Bool(false);
            Rewards = evt["Rewards"]?.ToObjectQ<BountyReward[]>();

            TargetLocalised = Target = evt["Target"].StrNull();       // set for skimmer target missions and for on foot bounties

            if (Target != null)         
            {
                TargetLocalised = evt["Target_Localised"].Str("$");     // if not there, trigger the getter suit naming

                var sp = ItemData.GetShipProperties(Target);        // if its a ship, replace with ship name
                if (sp != null)
                {
                    TargetLocalised = sp.Name;
                }
                else if (TargetLocalised.StartsWith("$"))
                {
                    TargetLocalised = JournalFieldNaming.GetBetterShipSuitActorName(Target);    // else use suit etc naming
                }

               // System.Diagnostics.Debug.WriteLine($"Bounty {Target} -> {TargetLocalised}");
            }
            else
            {

            }

            if ( Rewards == null )                  // for skimmers, its Faction/Reward.  Bug in manual reported to FD 23/5/2018
            {
                string faction = evt["Faction"].StrNull();
                long? reward = evt["Reward"].IntNull();

                if (faction != null && reward != null)      // create an array from it
                {
                    string factionloc = JournalFieldNaming.CheckLocalisation(evt["Faction_Localised"].Str(), faction);      // not mentioned in frontiers documents, but seen with $alliance, $fed etc
                    Rewards = new BountyReward[1];
                    Rewards[0] = new BountyReward() { Faction = faction, Faction_Localised = factionloc,  Reward = reward.Value };
                    TotalReward = reward.Value;
                }
            }
            else
            {
                foreach( var r in Rewards)
                {
                    r.Faction_Localised = JournalFieldNaming.CheckLocalisation(r.Faction_Localised, r.Faction);
                }
            }

            //new in patch 17
            PilotName = evt["PilotName"].StrNull();
            PilotName_Localised = JournalFieldNaming.CheckLocalisation(evt["PilotName_Localised"].StrNull(), PilotName);
        }

        public long TotalReward { get; set; }
        public string VictimFaction { get; set; }
        public string VictimFactionLocalised { get; set; }
        public string Target { get; set; }
        public string TargetLocalised { get; set; }
        public bool SharedWithOthers { get; set; }
        public BountyReward[] Rewards { get; set; }
        public string PilotName { get; set; }   //may be null
        public string PilotName_Localised { get; set; } //may be null

        // very old logs did not have target or victim faction, and therefore must be a ship
        public bool IsThargoid { get { return VictimFaction != null && VictimFaction.Contains("Thargoid", System.StringComparison.InvariantCultureIgnoreCase); } }       // seen both "Thargoid" and later "$faction_Thargoid;"
        public bool IsSkimmer { get { return Target != null && Target.Contains("Skimmer", System.StringComparison.InvariantCultureIgnoreCase); } }  
        public bool IsOnFootNPC { get { return Target != null && Target.Contains("suitai_", System.StringComparison.InvariantCultureIgnoreCase); } }       // Its a on foot NPC

        public bool IsShip { get { return !IsThargoid && !IsOnFootNPC && !IsSkimmer; } }

        // following only for Stats

        public JournalShipTargeted ShipTargettedForStatsOnly { get; set; }         // used in stats computation only.  Not in main code.
        public bool StatsUnknownShip { get => ShipTargettedForStatsOnly == null && IsShip; }
        public bool StatsEliteAboveShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank >= RankDefinitions.CombatRank.Elite; }
        public bool StatsRankShip(RankDefinitions.CombatRank r) { return ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank == r; }
        public bool StatsDangerousShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank == RankDefinitions.CombatRank.Dangerous; }
        public bool StatsHarmlessShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank <= RankDefinitions.CombatRank.Mostly_Harmless && 
                        ShipTargettedForStatsOnly.PilotCombatRank>= RankDefinitions.CombatRank.Harmless; }

        public void UpdateStats(Stats stats, ISystem system, string unusedstationfaction)
        {
//            System.Diagnostics.Debug.WriteLine("Bounty Victim " + VictimFactionLocalised);
            stats.BountyKill(system, VictimFactionLocalised);
            if (Rewards != null)
            {
                foreach (var r in Rewards)
                {
  //                  System.Diagnostics.Debug.WriteLine("..Bounty Reward {0} {1}" , r.Faction, r.Reward);
                    stats.BountyRewards(system, r.Faction_Localised, r.Reward);
                }
            }
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("; cr;N0", TotalReward, "Target".Tx()+": ", TargetLocalised, "Pilot".Tx()+": ", PilotName_Localised, "Victim faction".Tx()+": ", VictimFactionLocalised);
        }

        public override string GetDetailed()
        {
            if (Rewards != null)
            {
                var sb = new System.Text.StringBuilder(256);
                foreach (BountyReward r in Rewards)
                {
                    sb.BuildCont("Faction".Tx()+": ", r.Faction, "; cr;N0", r.Reward);
                }
                return sb.ToString();
            }
            else
                return null;
        }

        public string Type { get { return "Bounty".Tx(); } }
        public string TargetFaction { get { return VictimFaction; } }

        public bool HasFaction(string faction)
        {
            if (Rewards != null)
            {
                foreach (var br in Rewards)
                {
                    if (br.Faction == faction)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public long FactionReward(string faction)
        {
            if (Rewards != null)
            {
                foreach (var br in Rewards)
                {
                    if (br.Faction == faction)
                    {
                        return br.Reward;
                    }
                }
            }
            return 0;
        }
    }

    [JournalEntryType(JournalTypeEnum.CapShipBond)]
    public class JournalCapShipBond : JournalEntry, IStatsJournalEntry, IStatsJournalEntryBountyOrBond
    {
        public JournalCapShipBond(JObject evt) : base(evt, JournalTypeEnum.CapShipBond)
        {
            AwardingFaction = evt["AwardingFaction"].Str();
            AwardingFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["AwardingFaction_Localised"].Str(), AwardingFaction);      // no evidence of this, but keeping
            VictimFaction = evt["VictimFaction"].Str();
            VictimFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["VictimFaction_Localised"].Str(), VictimFaction);            // no evidence of this, but keeping
            Reward = evt["Reward"].Long();
        }

        public string AwardingFaction { get; set; }
        public string AwardingFaction_Localised { get; set; }       // may be empty
        public string VictimFaction { get; set; }
        public string VictimFaction_Localised { get; set; }         // may be empty

        public long Reward { get; set; }

        public void UpdateStats(Stats stats, ISystem system , string unusedstationfaction)
        {
            stats.CapShipAward(system, AwardingFaction_Localised, VictimFaction_Localised, Reward);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("; cr;N0", Reward, "< from ".Tx(), AwardingFaction_Localised,
                "<, due to ".Tx(), VictimFaction_Localised);
        }

        public string Type { get { return "Capital Ship Bond".Tx(); } }
        public string Target { get { return ""; } }
        public string TargetFaction { get { return VictimFaction; } }

        public bool HasFaction(string faction)
        {
            return faction == AwardingFaction;
        }

        public long FactionReward(string faction)
        {
            return faction == AwardingFaction ? Reward : 0;
        }
    }

    [JournalEntryType(JournalTypeEnum.CommitCrime)]
    public class JournalCommitCrime : JournalEntry, IStatsJournalEntry
    {
        public JournalCommitCrime(JObject evt) : base(evt, JournalTypeEnum.CommitCrime)
        {
            FDCrimeType = evt["CrimeType"].Str();
            CrimeType = JournalFieldNaming.CrimeType(FDCrimeType);
            //System.Diagnostics.Debug.WriteLine($"{FDCrimeType} -> {CrimeType} -> {CrimeTypeTranslated}");

            Faction = evt["Faction"].Str();
            Victim = evt["Victim"].Str();
            VictimLocalised = JournalFieldNaming.CheckLocalisation(evt["Victim_Localised"].Str(), Victim);
            Fine = evt["Fine"].LongNull();
            Bounty = evt["Bounty"].LongNull();
        }
        public string CrimeType { get; set; }       // friendly name
        public string FDCrimeType { get; set; }     // FDName
        public string Faction { get; set; }
        public string Victim { get; set; }
        public string VictimLocalised { get; set; }
        public long? Fine { get; set; }
        public long? Bounty { get; set; }
        public long Cost { get { return (Fine.HasValue ? Fine.Value : 0) + (Bounty.HasValue ? Bounty.Value : 0); } }

        public void UpdateStats(Stats stats, ISystem system, string unusedstationfaction)
        {
            stats.CommitCrime(system,Faction);
        }


        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Crimes.ToLocalisedLanguage(FDCrimeType), "< on faction ".Tx(), Faction, 
                        "Against ".Tx(), VictimLocalised, "Cost: ; cr;N0".Tx(), Fine, "Bounty: ; cr;N0".Tx(), Bounty);
        }
    }

    [JournalEntryType(JournalTypeEnum.CrimeVictim)]
    public class JournalCrimeVictim : JournalEntry      // someone is commiting a crime against you
    {
        public JournalCrimeVictim(JObject evt) : base(evt, JournalTypeEnum.CrimeVictim)
        {
            FDCrimeType = evt["CrimeType"].Str();
            CrimeType = JournalFieldNaming.CrimeType(FDCrimeType);
            Offender = evt["Offender"].Str();
            OffenderLocalised = JournalFieldNaming.CheckLocalisation(evt["Offender_Localised"].Str(), Offender);
            Bounty = evt["Bounty"].Long();
        }
        public string CrimeType { get; set; }       // friendly name
        public string FDCrimeType { get; set; }
        public string Offender { get; set; }
        public string OffenderLocalised { get; set; }
        public long Bounty { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", CrimeType, "Offender ".Tx(), OffenderLocalised, "Bounty: ; cr;N0".Tx(), Bounty);
        }
    }

    [JournalEntryType(JournalTypeEnum.FactionKillBond)]
    public class JournalFactionKillBond : JournalEntry, IStatsJournalEntry, IStatsJournalEntryBountyOrBond
    {
        public JournalFactionKillBond(JObject evt) : base(evt, JournalTypeEnum.FactionKillBond)
        {
            AwardingFaction = evt["AwardingFaction"].Str();
            AwardingFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["AwardingFaction_Localised"].Str(), AwardingFaction);
            VictimFaction = evt["VictimFaction"].Str();
            VictimFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["VictimFaction_Localised"].Str(), VictimFaction);
            Reward = evt["Reward"].Long();
        }

        public string AwardingFaction { get; set; }
        public string AwardingFaction_Localised { get; set; }       // may be empty
        public string VictimFaction { get; set; }
        public string VictimFaction_Localised { get; set; }         // may be empty
        public long Reward { get; set; }
        public int? NumberRewards { get; set; }                      // EDD addition, number of rewards

        public string Type { get { return "Faction Kill Bond".Tx(); } }
        public string Target { get { return ""; } }
        public string TargetFaction { get { return VictimFaction; } }


        public void UpdateStats(Stats stats, ISystem system, string unusedstationfaction)
        {
            stats.KillBond(system, AwardingFaction_Localised, VictimFaction_Localised, Reward);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("x", NumberRewards,
                                        "Reward: ; cr;N0".Tx(), Reward, 
                                        "< from ".Tx(), AwardingFaction_Localised,
                                        "<, due to ".Tx(), VictimFaction_Localised);
        }

        public bool HasFaction(string faction)
        {
            return faction == AwardingFaction;
        }

        public long FactionReward(string faction)
        {
            return faction == AwardingFaction ? Reward : 0;
        }
    }


    [JournalEntryType(JournalTypeEnum.PayBounties)]
    public class JournalPayBounties : JournalEntry, ILedgerJournalEntry, IStatsJournalEntry
    {
        public JournalPayBounties(JObject evt) : base(evt, JournalTypeEnum.PayBounties)
        {
            Amount = evt["Amount"].Long();
            BrokerPercentage = evt["BrokerPercentage"].Double();
            AllFines = evt["AllFines"].Bool();
            Faction = evt["Faction"].Str();
            Faction_Localised = JournalFieldNaming.CheckLocalisation(evt["Faction_Localised"].Str(), Faction);
            ShipId = evt["ShipID"].ULong();
        }

        public long Amount { get; set; }
        public double BrokerPercentage { get; set; }
        public bool AllFines { get; set; }
        public string Faction { get; set; }      // may be blank
        public string Faction_Localised { get; set; }    // may be blank
        public ulong ShipId { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, (Faction_Localised.Length > 0 ? "Faction " + Faction_Localised : "") + " Broker " + BrokerPercentage.ToString("0.0") + "%", -Amount);
        }

        public override string GetInfo()
        {
            string info = BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".Tx(), Amount, "< to ".Tx(), Faction_Localised);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".Tx(), BrokerPercentage);
            return info;
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            stats.PayBounties(system,Faction, Amount);
        }
    }

    [JournalEntryType(JournalTypeEnum.PayFines)]
    public class JournalPayFines : JournalEntry, ILedgerJournalEntry, IStatsJournalEntry
    { 
        public JournalPayFines(JObject evt) : base(evt, JournalTypeEnum.PayFines)
        {
            Amount = evt["Amount"].Long();
            BrokerPercentage = evt["BrokerPercentage"].Double();
            AllFines = evt["AllFines"].Bool();
            Faction = evt["Faction"].Str();
            Faction_Localised = JournalFieldNaming.CheckLocalisation(evt["Faction_Localised"].Str(), Faction);
            ShipId = evt["ShipID"].ULong();
        }

        public long Amount { get; set; }
        public double BrokerPercentage { get; set; }
        public bool AllFines { get; set; }
        public string Faction { get; set; } // may be blank
        public string Faction_Localised { get; set; }       // may be blank
        public ulong ShipId { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, (Faction_Localised.Length > 0 ? "Faction " + Faction_Localised : "") + " Broker " + BrokerPercentage.ToString("0.0") + "%", -Amount);
        }

        public override string GetInfo()
        {
            string info =BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".Tx(), Amount, "< to ".Tx(), Faction_Localised);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".Tx(), BrokerPercentage);
            return info;
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if ( Faction.HasChars())
                stats.PayFines(system, Faction, Amount);
        }
    }


    [JournalEntryType(JournalTypeEnum.PayLegacyFines)]
    public class JournalPayLegacyFines : JournalEntry, ILedgerJournalEntry
    {
        public JournalPayLegacyFines(JObject evt) : base(evt, JournalTypeEnum.PayLegacyFines)
        {
            Amount = evt["Amount"].Long();
            BrokerPercentage = evt["BrokerPercentage"].Double();
        }
        public long Amount { get; set; }
        public double BrokerPercentage { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "Broker " + BrokerPercentage.ToString("0.0") + "%", -Amount);
        }

        public override string GetInfo()
        {
            string info = BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".Tx(), Amount);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".Tx(), BrokerPercentage);
            return info;
        }
    }

    [JournalEntryType(JournalTypeEnum.RedeemVoucher)]
    public class JournalRedeemVoucher : JournalEntry, ILedgerJournalEntry, IStatsJournalEntry
    {
        [System.Diagnostics.DebuggerDisplay("{Faction} {Amount}")]
        public class FactionInfo
        {
            public string Faction { get; set; }
            public long Amount { get; set; }
        };

        public JournalRedeemVoucher(JObject evt) : base(evt, JournalTypeEnum.RedeemVoucher)
        {
            FDType = evt["Type"].Str();
            Type = JournalFieldNaming.RedeemVoucherType(FDType);
            Amount = evt["Amount"].Long();

            if (evt.Contains("Factions"))
            {
                Factions = evt["Factions"]?.ToObjectQ<FactionInfo[]>();
                if (Factions != null)
                {
                    Faction = "";
                    foreach (var x in Factions)
                    {
                        if ( x.Faction.HasChars())
                            Faction = Faction.AppendPrePad(x.Faction, ", ");
                    }
                }
            }
            else
            {
                Faction = evt["Faction"].Str();
                if (Faction.Length == 0)
                    Faction = null;
                else
                    Factions = new FactionInfo[] { new FactionInfo() { Faction = this.Faction, Amount = this.Amount } };
            }

            BrokerPercentage = evt["BrokerPercentage"].Double();
        }

        public string Type { get; set; }
        public string FDType { get; set; }
        public long Amount { get; set; }
        public string Faction { get; set; }     // if multiple, comma separ list.  If empty, its null
        public FactionInfo[] Factions { get; set; }     // null if no factions, else at least one faction here
        public double BrokerPercentage { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Type + " Broker " + BrokerPercentage.ToString("0.0") + "%", Amount);
        }

        public override string GetInfo()
        {
            string info = BaseUtils.FieldBuilder.Build("Type".Tx()+": ", Type, "Amount: ; cr;N0".Tx(), Amount, "Faction".Tx()+": ", Faction);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".Tx(), BrokerPercentage);
            return info;
        }

        public override string GetDetailed()
        {
            if (Factions?.Length > 1)
            {
                var sb = new System.Text.StringBuilder(256);

                foreach (var f in Factions)
                    sb.AppendPrePad($"{f.Faction} = {f.Amount} cr", Environment.NewLine);
                return sb.ToString();
            }
            else
                return null;
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (Faction != null)
            {
                if (Factions?.Length > 1)
                {
                    foreach (var f in Factions)
                        stats.RedeemVoucher(system, f.Faction, f.Amount);

                }
                else
                    stats.RedeemVoucher(system, Faction, Amount);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.ClearImpound)]
    public class JournalClearImpound : JournalEntry
    {
        public JournalClearImpound(JObject evt) : base(evt, JournalTypeEnum.ClearImpound)
        {
            ShipType = JournalFieldNaming.NormaliseFDShipName(evt["ShipType"].Str());
            ShipType_Localised = JournalFieldNaming.CheckLocalisation(evt["ShipType_Localised"].Str(), ShipType);
            ShipId = evt["ShipID"].ULong();
            MarketID = evt["MarketID"].Long();
            ShipMarketID = evt["ShipMarketID"].Long();
            System = evt["System"].StrNull();
        }

        public string ShipType { get; set; }
        public string ShipType_Localised { get; set; }
        public ulong ShipId { get; set; }
        public long ShipMarketID { get; set; }
        public long MarketID { get; set; }
        public string System { get; set; }  //patch 17, so may be null

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Ship".Tx()+": ", ShipType_Localised, "< in system ".Tx(), System);
        }
    }

    [JournalEntryType(JournalTypeEnum.HoloscreenHacked)]
    public class JournalHoloscreenHacked : JournalEntry
    {
        public JournalHoloscreenHacked(JObject evt) : base(evt, JournalTypeEnum.HoloscreenHacked)
        {
            PowerBefore = evt["PowerBefore"].Str();
            PowerAfter = evt["PowerAfter"].Str();
        }

        public string PowerBefore { get; set; }
        public string PowerAfter { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Power".Tx()+": ", PowerBefore, "< -> ", PowerAfter);
        }
    }

}
