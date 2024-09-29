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
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Scanned)]
    public class JournalScanned : JournalEntry
    {
        public JournalScanned(JObject evt) : base(evt, JournalTypeEnum.Scanned)
        {
            FDScanType = evt["ScanType"].Str();
            ScanType = JournalFieldNaming.ScanType(FDScanType);
        }

        public string ScanType { get; set; }        // Friendly, not FDEV
        public string FDScanType { get; set; }        // fdname

        public override string GetInfo()
        {
            return ScanType;
        }
    }

    [JournalEntryType(JournalTypeEnum.ShipTargeted)]
    public class JournalShipTargeted : JournalEntry
    {
        public JournalShipTargeted(JObject evt) : base(evt, JournalTypeEnum.ShipTargeted)
        {
            TargetLocked = evt["TargetLocked"].Bool();

            ShipFD = evt["Ship"].StrNull();
            if (ItemData.IsShip(ShipFD))
            {
                ShipFD = JournalFieldNaming.NormaliseFDShipName(ShipFD);
                Ship = JournalFieldNaming.GetBetterShipSuitActorName(ShipFD);
            }
            Ship_Localised = JournalFieldNaming.CheckLocalisation(evt["Ship_Localised"].Str(), Ship);

            ScanStage = evt["ScanStage"].IntNull();
            PilotName = evt["PilotName"].StrNull();
            PilotName_Localised = JournalFieldNaming.CheckLocalisation(evt["PilotName_Localised"].Str(), PilotName);

            PilotRank = evt["PilotRank"].StrNull();
            if (PilotRank != null)
                PilotCombatRank = RankDefinitions.CombatRankToEnum(PilotRank);

            ShieldHealth = evt["ShieldHealth"].DoubleNull();
            HullHealth = evt["HullHealth"].DoubleNull();
            Faction = evt["Faction"].StrNull();
            LegalStatus = evt["LegalStatus"].StrNull();
            Bounty = evt["Bounty"].IntNull();
            SubSystem = evt["SubSystem"].StrNull();
            SubSystemHealth = evt["SubSystemHealth"].DoubleNull();
            Power = evt["Power"].StrNull();
        }

        public bool TargetLocked { get; set; }          // if false, no info below
        public int? ScanStage { get; set; }             // targetlocked= true, 0/1/2/3

        public string Ship { get; set; }                // 0 null
        public string ShipFD { get; set; }              // 0 null
        public string Ship_Localised { get; set; }      // 0 will be empty
        public string PilotName { get; set; }           // 1 null
        public string PilotName_Localised { get; set; } // 1 will be empty 
        public string PilotRank { get; set; }           // 1 null, Friendly name
        public RankDefinitions.CombatRank PilotCombatRank { get; set; } // 1 May be unknown at any level
        public double? ShieldHealth { get; set; }       // 2 null
        public double? HullHealth { get; set; }         // 2 null
        public string Faction { get; set; }             // 3 null
        public string LegalStatus { get; set; }         // 3 null
        public int? Bounty { get; set; }                // 3 null 
        public string SubSystem { get; set; }           // 3 null
        public double? SubSystemHealth { get; set; }    // 3 null
        public string Power { get; set; }               // 3 null

        public List<JournalShipTargeted> MergedEntries { get; set; }    // if verbose.. doing it this way does not break action packs as the variables are maintained
                                                                        // This is second, third merge etc.  First one is in above variables

        public void Add(JournalShipTargeted next)
        {
            if (MergedEntries == null)
                MergedEntries = new List<JournalShipTargeted>();
            MergedEntries.Add(next);
        }

        public override string GetInfo()
        {
            if (MergedEntries == null)
                return ToString();
            else
            {
                return (MergedEntries.Count() + 1).ToString() + " Target Events".T(EDCTx.JournalShipTargeted_MC);
            }
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (MergedEntries != null)
            {
                for (int i = MergedEntries.Count - 1; i >= 0; i--)
                    sb.AppendPrePad(MergedEntries[i].ToString(), System.Environment.NewLine);
            }
            sb.AppendPrePad(ToString(), System.Environment.NewLine);   // ours is the last one
            return sb.ToString();
        }

        public override string ToString()
        {
            string info;
            if (TargetLocked)
            {
                if (ScanStage == null)
                {
                    info = "Missing Scan Stage - report to EDD team";
                }
                else if (ScanStage.Value == 0)
                {
                    return BaseUtils.FieldBuilder.Build("", Ship_Localised);
                }
                else if (ScanStage.Value == 1)
                {
                    return BaseUtils.FieldBuilder.Build("", PilotName_Localised, "Rank: ".T(EDCTx.JournalEntry_Rank), PilotRank, "< in ".T(EDCTx.JournalShipTargeted_in), Ship_Localised);
                }
                else if (ScanStage.Value == 2)
                {
                    return BaseUtils.FieldBuilder.Build(
                        "", PilotName_Localised, "Rank: ".T(EDCTx.JournalEntry_Rank), PilotRank, "< in ".T(EDCTx.JournalShipTargeted_in), Ship_Localised,
                        "Shield ;;N1".T(EDCTx.JournalEntry_Shield), ShieldHealth, "Hull ;;N1".T(EDCTx.JournalShipTargeted_Hull), HullHealth);


                }
                else if (ScanStage.Value == 3)
                {
                    return BaseUtils.FieldBuilder.Build(
                                    "", PilotName_Localised, "< (;)", LegalStatus, "Rank: ".T(EDCTx.JournalEntry_Rank), PilotRank, "< in ".T(EDCTx.JournalShipTargeted_in), Ship_Localised,
                                    "Shield ;;N1".T(EDCTx.JournalEntry_Shield), ShieldHealth, "Hull ;;N1".T(EDCTx.JournalShipTargeted_Hull), HullHealth,
                                    "Bounty: ; cr;N0".T(EDCTx.JournalEntry_Bounty), Bounty,
                                    "", SubSystem, "< at ;;N1".T(EDCTx.JournalShipTargeted_at), SubSystemHealth,
                                    "Power: ".T(EDCTx.JournalEntry_Power), Power
                                    );
                }
                else
                    info = "Unknown Scan Stage type - report to EDD team";
            }
            else
                info = "Lost Target".T(EDCTx.JournalEntry_LostTarget);

            return info;
        }

    }


    [JournalEntryType(JournalTypeEnum.UnderAttack)]
    public class JournalUnderAttack : JournalEntry
    {
        public JournalUnderAttack(JObject evt) : base(evt, JournalTypeEnum.UnderAttack)
        {
            Target = evt["Target"].Str();
        }

        public string Target { get; set; }                  // always first one if merged list.
        public List<string> MergedEntries { get; set; }     // if verbose.. doing it this way does not break action packs as the variables are maintained
                                                            // This is second, third merge etc.  First one is in above variables

        public override string GetInfo()
        {
            if (MergedEntries != null)
                return (MergedEntries.Count + 1).ToString("N0") + " " + "times".T(EDCTx.JournalUnderAttack_ACOUNT);
            else
                return BaseUtils.FieldBuilder.Build("", Target);
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (MergedEntries != null)
            {
                for (int i = MergedEntries.Count - 1; i >= 0; i--)
                    sb.AppendPrePad(MergedEntries[i].ToString(), System.Environment.NewLine);
            }
            sb.AppendPrePad(Target, System.Environment.NewLine);   // ours is the last one
            return sb.ToString();
        }

        public void Add(string target)
        {
            if (MergedEntries == null)
                MergedEntries = new List<string>();
            MergedEntries.Add(target);    // first is second oldest, etc
        }

    }

    [JournalEntryType(JournalTypeEnum.ShieldState)]
    public class JournalShieldState : JournalEntry
    {
        public JournalShieldState(JObject evt) : base(evt, JournalTypeEnum.ShieldState)
        {
            ShieldsUp = evt["ShieldsUp"].Bool();
        }

        public bool ShieldsUp { get; set; }

        protected override JournalTypeEnum IconEventType { get { return ShieldsUp ? JournalTypeEnum.ShieldState_ShieldsUp : JournalTypeEnum.ShieldState_ShieldsDown; } }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Shields Down;Shields Up".T(EDCTx.JournalEntry_ShieldsDown), ShieldsUp);
        }
    }


}
