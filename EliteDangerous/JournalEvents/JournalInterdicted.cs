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
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Interdicted)]
    public class JournalInterdicted : JournalEntry, IStatsJournalEntry
    {
        public JournalInterdicted(JObject evt ) : base(evt, JournalTypeEnum.Interdicted)        // some nasty pilot has got you
        {
            Submitted = evt["Submitted"].Bool();
            Interdictor = evt["Interdictor"].Str();
            Interdictor_Localised = JournalFieldNaming.CheckLocalisation(evt["Interdictor_Localised"].Str(),Interdictor);
            IsPlayer = evt["IsPlayer"].Bool();
            IsThargoid = evt["IsThargoid"].BoolNull();
            CombatRank = evt["CombatRank"].Enum<RankDefinitions.CombatRank>(RankDefinitions.CombatRank.Unknown);
            Faction = evt["Faction"].Str();
            Power = evt["Power"].Str();
        }
        public bool Submitted { get; set; }
        public string Interdictor { get; set; }
        public string Interdictor_Localised { get; set; }
        public bool IsPlayer { get; set; }
        public bool? IsThargoid { get; set; }        // update 15+
        public RankDefinitions.CombatRank CombatRank { get; set; } = RankDefinitions.CombatRank.Unknown;
        public string Faction { get; set; }
        public string Power { get; set; }

        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
            stats.Interdicted(Faction);
        }

        public override void FillInformation(out string info, out string detailed) 
        {
            // tbd thargoid
            if ( Submitted )
                info = BaseUtils.FieldBuilder.Build(";Submitted".T(EDCTx.JournalEntry_Submitted), Submitted, "< to ".T(EDCTx.JournalEntry_to), Interdictor_Localised, 
                    "< (NPC);(Player)".T(EDCTx.JournalEntry_NPC), IsPlayer, "Rank: ", RankDefinitions.FriendlyName(CombatRank), 
                    "Faction: ".T(EDCTx.JournalEntry_Faction), Faction, "Power: ".T(EDCTx.JournalEntry_Power), Power);
            else
                info = BaseUtils.FieldBuilder.Build("By: ".T(EDCTx.JournalEntry_By), Interdictor_Localised, 
                    "< (NPC);(Player)".T(EDCTx.JournalEntry_NPC), IsPlayer, 
                    "Rank: ", RankDefinitions.FriendlyName(CombatRank), 
                    "Faction: ".T(EDCTx.JournalEntry_Faction), Faction, "Power: ".T(EDCTx.JournalEntry_Power), Power);

            detailed = "";
        }
    }


    [JournalEntryType(JournalTypeEnum.Interdiction)]
    public class JournalInterdiction : JournalEntry, IStatsJournalEntry
    {
        public JournalInterdiction(JObject evt) : base(evt, JournalTypeEnum.Interdiction)       // you've been naughty and interdicted someone
        {
            Success = evt["Success"].Bool();
            Interdicted = evt["Interdicted"].Str();
            Interdicted_Localised = JournalFieldNaming.CheckLocalisation(evt["Interdicted_Localised"].Str(), Interdicted);
            IsPlayer = evt["IsPlayer"].Bool();
            CombatRank = evt["CombatRank"].Enum<RankDefinitions.CombatRank>(RankDefinitions.CombatRank.Unknown);
            Faction = evt["Faction"].Str();
            Power = evt["Power"].Str();
        }
        public bool Success { get; set; }
        public string Interdicted { get; set; }
        public string Interdicted_Localised { get; set; }
        public bool IsPlayer { get; set; }
        public RankDefinitions.CombatRank CombatRank { get; set; } = RankDefinitions.CombatRank.Unknown;
        public string Faction { get; set; }
        public string Power { get; set; }

        public void UpdateStats(Stats stats, string unusedstationfaction)
        {
            stats.Interdiction(Faction);
        }

        public override void FillInformation(out string info, out string detailed)
        {

            info = BaseUtils.FieldBuilder.Build("Failed to interdict;Interdicted".T(EDCTx.JournalEntry_Failedtointerdict), Success, "< ", Interdicted_Localised, 
                        "< (NPC);(Player)".T(EDCTx.JournalEntry_NPC), IsPlayer, 
                        "Rank: ", RankDefinitions.FriendlyName(CombatRank), 
                        "Faction: ".T(EDCTx.JournalEntry_Faction), Faction, "Power: ".T(EDCTx.JournalEntry_Power), Power);
            detailed = "";
        }
    }


    [JournalEntryType(JournalTypeEnum.EscapeInterdiction)]
    public class JournalEscapeInterdiction : JournalEntry
    {
        public JournalEscapeInterdiction(JObject evt) : base(evt, JournalTypeEnum.EscapeInterdiction)
        {
            Interdictor = evt["Interdictor"].Str();
            Interdictor_Localised = JournalFieldNaming.CheckLocalisation(evt["Interdictor_Localised"].Str(), Interdictor);
            IsPlayer = evt["IsPlayer"].Bool();
            IsThargoid = evt["IsThargoid"].BoolNull();
        }

        public string Interdictor { get; set; }
        public string Interdictor_Localised { get; set; }
        public bool IsPlayer { get; set; }
        public bool? IsThargoid { get; set; }        // update 15+

        public override void FillInformation(out string info, out string detailed)
        {
            // tbd thargoid
            info = BaseUtils.FieldBuilder.Build("By: ".T(EDCTx.JournalEntry_By), Interdictor_Localised, "< (NPC);(Player)".T(EDCTx.JournalEntry_NPC), IsPlayer);
            detailed = "";
        }
    }

}
