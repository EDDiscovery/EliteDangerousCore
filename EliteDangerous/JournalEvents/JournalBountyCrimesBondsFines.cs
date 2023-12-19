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
                    TargetLocalised = ((ItemData.ShipInfoString)sp[ItemData.ShipPropID.Name]).Value;
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
        public bool StatsEliteAboveShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank >= CombatRank.Elite; }
        public bool StatsRankShip(CombatRank r) { return ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank == r; }
        public bool StatsDangerousShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank == CombatRank.Dangerous; }
        public bool StatsHarmlessShip { get => ShipTargettedForStatsOnly != null && ShipTargettedForStatsOnly.PilotCombatRank <= CombatRank.Mostly_Harmless && ShipTargettedForStatsOnly.PilotCombatRank>= CombatRank.Harmless; }

        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
//            System.Diagnostics.Debug.WriteLine("Bounty Victim " + VictimFactionLocalised);
            stats.BountyKill(VictimFactionLocalised);
            if (Rewards != null)
            {
                foreach (var r in Rewards)
                {
  //                  System.Diagnostics.Debug.WriteLine("..Bounty Reward {0} {1}" , r.Faction, r.Reward);
                    stats.BountyRewards(r.Faction_Localised, r.Reward);
                }
            }
        }

        public override void FillInformation(out string info, out string detailed) 
        {
            
            info = BaseUtils.FieldBuilder.Build("; cr;N0", TotalReward, "Target: ".T(EDCTx.JournalEntry_Target), TargetLocalised, "Pilot: ".T(EDCTx.JournalEntry_Pilot), PilotName_Localised, "Victim faction: ".T(EDCTx.JournalEntry_Victimfaction), VictimFactionLocalised);

            detailed = "";
            if ( Rewards!=null)
            {
                foreach (BountyReward r in Rewards)
                {
                    if (detailed.Length > 0)
                        detailed += ", ";

                    detailed += BaseUtils.FieldBuilder.Build("Faction: ".T(EDCTx.JournalEntry_Faction), r.Faction, "; cr;N0", r.Reward);
                }
            }
        }

        public string Type { get { return "Bounty".T(EDCTx.JournalEntry_BountyOnly); } }
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

        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
            stats.CapShipAward(AwardingFaction_Localised, VictimFaction_Localised, Reward);
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("; cr;N0", Reward, "< from ".T(EDCTx.JournalEntry_from), AwardingFaction_Localised,
                "<, due to ".T(EDCTx.JournalEntry_dueto), VictimFaction_Localised);
            detailed = "";
        }

        public string Type { get { return "Capital Ship Bond".T(EDCTx.JournalTypeEnum_CapShipBond); } }
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
            CrimeType = evt["CrimeType"].Str().SplitCapsWordFull();
            Faction = evt["Faction"].Str();
            Victim = evt["Victim"].Str();
            VictimLocalised = JournalFieldNaming.CheckLocalisation(evt["Victim_Localised"].Str(), Victim);
            Fine = evt["Fine"].LongNull();
            Bounty = evt["Bounty"].LongNull();
        }
        public string CrimeType { get; set; }
        public string Faction { get; set; }
        public string Victim { get; set; }
        public string VictimLocalised { get; set; }
        public long? Fine { get; set; }
        public long? Bounty { get; set; }
        public long Cost { get { return (Fine.HasValue ? Fine.Value : 0) + (Bounty.HasValue ? Bounty.Value : 0); } }

        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
            stats.CommitCrime(Faction);
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", CrimeType, "< on faction ".T(EDCTx.JournalEntry_onfaction), Faction, "Against ".T(EDCTx.JournalEntry_Against), VictimLocalised, "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Fine, "Bounty: ; cr;N0".T(EDCTx.JournalEntry_Bounty), Bounty);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.CrimeVictim)]
    public class JournalCrimeVictim : JournalEntry      // someone is commiting a crime against you
    {
        public JournalCrimeVictim(JObject evt) : base(evt, JournalTypeEnum.CrimeVictim)
        {
            CrimeType = evt["CrimeType"].Str().SplitCapsWordFull();
            Offender = evt["Offender"].Str();
            OffenderLocalised = JournalFieldNaming.CheckLocalisation(evt["Offender_Localised"].Str(), Offender);
            Bounty = evt["Bounty"].Long();
        }
        public string CrimeType { get; set; }
        public string Offender { get; set; }
        public string OffenderLocalised { get; set; }
        public long Bounty { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", CrimeType, "Offender ".T(EDCTx.JournalEntry_Offender), OffenderLocalised, "Bounty: ; cr;N0".T(EDCTx.JournalEntry_Bounty), Bounty);
            detailed = "";
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

        public string Type { get { return "Faction Kill Bond".T(EDCTx.JournalTypeEnum_FactionKillBond); } }
        public string Target { get { return ""; } }
        public string TargetFaction { get { return VictimFaction; } }


        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
            stats.KillBond(AwardingFaction_Localised, VictimFaction_Localised, Reward);
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Reward: ; cr;N0".T(EDCTx.JournalEntry_Reward), Reward, "< from ".T(EDCTx.JournalEntry_from), AwardingFaction_Localised,
                "<, due to ".T(EDCTx.JournalEntry_dueto), VictimFaction_Localised);
            detailed = "";
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
    public class JournalPayBounties : JournalEntry, ILedgerJournalEntry
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

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Amount, "< to ".T(EDCTx.JournalEntry_to), Faction_Localised);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".T(EDCTx.JournalEntry_Brokertook), BrokerPercentage);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.PayFines)]
    public class JournalPayFines : JournalEntry, ILedgerJournalEntry
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

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Amount, "< to ".T(EDCTx.JournalEntry_to), Faction_Localised);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".T(EDCTx.JournalEntry_Brokertook), BrokerPercentage);
            detailed = "";
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

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Amount);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".T(EDCTx.JournalEntry_Brokertook), BrokerPercentage);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.RedeemVoucher)]
    public class JournalRedeemVoucher : JournalEntry, ILedgerJournalEntry
    {
        public JournalRedeemVoucher(JObject evt) : base(evt, JournalTypeEnum.RedeemVoucher)
        {
            Type = evt["Type"].Str().SplitCapsWordFull();
            Amount = evt["Amount"].Long();
            Faction = evt["Faction"].Str();
            BrokerPercentage = evt["BrokerPercentage"].Double();
        }

        public string Type { get; set; }
        public long Amount { get; set; }
        public string Faction { get; set; }
        public double BrokerPercentage { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Type + " Broker " + BrokerPercentage.ToString("0.0") + "%", Amount);
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Type: ".T(EDCTx.JournalEntry_Type), Type, "Amount: ; cr;N0".T(EDCTx.JournalEntry_Amount), Amount, "Faction: ".T(EDCTx.JournalEntry_Faction), Faction);
            if (BrokerPercentage > 0)
                info += string.Format(", Broker took {0:N0}%".T(EDCTx.JournalEntry_Brokertook), BrokerPercentage);
            detailed = "";
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

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Ship: ".T(EDCTx.JournalEntry_Ship), ShipType_Localised, "< in system ".T(EDCTx.JournalLocOrJump_insystem), System);
            detailed = "";
        }
    }

}
