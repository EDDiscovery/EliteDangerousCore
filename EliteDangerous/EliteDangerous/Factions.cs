/*
 * Copyright © 2023-2023 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using QuickJSON;
using System;
using System.Linq;

namespace EliteDangerousCore
{
    public class FactionDefinitions
    {
        public enum State
        {
            Unknown,
            UnknownSpansh,
            None,
            Boom,
            Bust,
            CivilUnrest,
            CivilWar,
            Election,
            Expansion,
            Famine,
            Investment,
            Lockdown,
            Outbreak,
            Retreat,
            War,
            CivilLiberty,
            PirateAttack,
            Blight,
            Drought,
            InfrastructureFailure,
            NaturalDisaster,
            PublicHoliday,
            Terrorism,
            ColdWar,
            Colonisation,
            HistoricEvent,
            Revolution,
            TechnologicalLeap,
            TradeWar,
            Exploited,
        }

        public static State? ToEnum(string englishname)
        {
            if (englishname == null)
            {
                //System.Diagnostics.Trace.WriteLine($"**** No faction state");
                return null;
            }
            else if (Enum.TryParse(englishname, true, out State value)) // case insensitive
            {
                return value;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"**** Unknown faction state {englishname}");
                return State.Unknown;
            }
        }

        public static string ToEnglish(State? stat)
        {
            return stat != null ? stat.ToString().SplitCapsWordFull() : null;
        }

        public static string ToLocalisedLanguage(State? stat)
        {
            if (stat == null)
                return null;
            string id = "FactionStates." + stat.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(stat), id);
        }

        public static void IDSTx()
        {
            foreach (var name in Enum.GetNames(typeof(State)))
                System.Diagnostics.Trace.WriteLine($".{name}: \"{name.SplitCapsWordFull()}\" @");
        }

        // from journal location or fsd jump or carrier jump, faction info array

        [System.Diagnostics.DebuggerDisplay("{Name} {FactionState} {Government} {Allegiance}")]
        public class FactionInformation
        {
            public string Name { get; set; }                                    // All properties except noted from Json of event
            public FactionDefinitions.State FactionState { get; set; }
            public GovernmentDefinitions.Government Government { get; set; }
            public double Influence { get; set; }                               // influence of faction in system
            public AllegianceDefinitions.Allegiance Allegiance { get; set; }
            public enum HappinessState { Unknown, Band1, Band2, Band3, Band4, Band5 };
            public HappinessState Happiness { get; set; }   // 3.3 May be missing thus Unknown
            public string Happiness_Localised { get; set; } //3.3, may be null

            //TBC
            //-100.. -90: hostile
            //-90.. -35: unfriendly
            //-35..+ 4: neutral
            //+4..+35: cordial
            //+35..+90: friendly
            //+90..+100: allied
            public double? MyReputation { get; set; } //3.3, may be null

            public static string Reputation(double? value)
            {
                if (value == null)
                    return "Unknown".TxID(EDCTx.Unknown);
                else if (value < -90)
                    return "Hostile".TxID(EDCTx.Hostile);
                else if (value <= -35)
                    return "Unfriendly".TxID(EDCTx.Unfriendly);
                else if (value <= 4)
                    return "Neutral".TxID(EDCTx.Neutral);
                else if (value <= 35)
                    return "Cordial".TxID(EDCTx.Cordial);
                else if (value <= 90)
                    return "Friendly".TxID(EDCTx.Friendly);
                else
                    return "Allied".TxID(EDCTx.Allied);
            }

            public class PowerStatesInfo
            {
                public enum States
                {
                    Unknown,
                    Blight,
                    Boom,
                    Bust,
                    CivilLiberty,
                    CivilUnrest,
                    CivilWar,
                    Drought,
                    Election,
                    Expansion,
                    Famine,
                    InfrastructureFailure,
                    Investment,
                    Lockdown,
                    NaturalDisaster,
                    Outbreak,
                    PirateAttack,
                    PublicHoliday,
                    Retreat,
                    Terrorism,
                    War,
                }
                public States State { get; set; }
                public int Trend { get; set; }
            }

            public class ActiveStatesInfo       // may be null, Howard info via discord .. just state
            {
                public PowerStatesInfo.States State { get; set; }
            }

            public PowerStatesInfo[] PendingStates { get; set; }    // may be null
            public PowerStatesInfo[] RecoveringStates { get; set; } // may be null
            public ActiveStatesInfo[] ActiveStates { get; set; }    //3.3, may be null

            public bool? SquadronFaction { get; set; }              //3.3, may be null
            public bool? HappiestSystem { get; set; }               //3.3, may be null
            public bool? HomeSystem { get; set; }                   //3.3, may be null

            public DateTime UTC { get; set; }                       // EDD addition, UTC of record
            public ISystem System {get;set;}

            // short form report
            public override string ToString()
            {
                return EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(UTC).ToString("yyyy/MM/dd HH:mm") + Environment.NewLine +
                        ToLocalisedLanguage(FactionState) + Environment.NewLine +
                        AllegianceDefinitions.ToLocalisedLanguage(Allegiance) + Environment.NewLine +
                        GovernmentDefinitions.ToLocalisedLanguage(Government) + Environment.NewLine +
                        "Rep:" + (MyReputation?.ToString("N2") ?? "-");
            }

            // long form report into string builder
            public void ToString(System.Text.StringBuilder sb)
            {
                sb.Append(BaseUtils.FieldBuilder.Build("", Name, "State: ".T(EDCTx.JournalLocOrJump_State), FactionDefinitions.ToLocalisedLanguage(FactionState),
                                                                   "Government: ".T(EDCTx.JournalLocOrJump_Government), GovernmentDefinitions.ToLocalisedLanguage(Government),
                                                                   "Inf: ;%".T(EDCTx.JournalLocOrJump_Inf), (Influence * 100.0).ToString("0.0"),
                                                                   "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), AllegianceDefinitions.ToLocalisedLanguage(Allegiance),
                                                                   "Happiness: ".T(EDCTx.JournalLocOrJump_Happiness), Happiness_Localised,
                                                                   "Reputation: ;%;N1".T(EDCTx.JournalLocOrJump_Reputation), MyReputation,
                                                                   ";Squadron System".T(EDCTx.JournalLocOrJump_SquadronSystem), SquadronFaction,
                                                                   ";Happiest System".T(EDCTx.JournalLocOrJump_HappiestSystem), HappiestSystem,
                                                                   ";Home System".T(EDCTx.JournalLocOrJump_HomeSystem), HomeSystem
                                                                   ));

                if (PendingStates != null)
                {
                    sb.Append(BaseUtils.FieldBuilder.Build(",", "Pending State: ".T(EDCTx.JournalLocOrJump_PendingState)));

                    foreach (FactionDefinitions.FactionInformation.PowerStatesInfo state in PendingStates)
                        sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend));

                }

                if (RecoveringStates != null)
                {
                    sb.Append(BaseUtils.FieldBuilder.Build(",", "Recovering State: ".T(EDCTx.JournalLocOrJump_RecoveringState)));

                    foreach (FactionDefinitions.FactionInformation.PowerStatesInfo state in RecoveringStates)
                        sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend));
                }

                if (ActiveStates != null)
                {
                    sb.Append(BaseUtils.FieldBuilder.Build(",", "Active State: ".T(EDCTx.JournalLocOrJump_ActiveState)));

                    foreach (FactionDefinitions.FactionInformation.ActiveStatesInfo state in ActiveStates)
                        sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State));
                }
                sb.Append(Environment.NewLine);

            }

            // handle reading from JSON this field
            public static FactionInformation[] ReadJSON(JArray evt, DateTime utc, ISystem sys)
            {
                var Factions = evt.ToObject<FactionInformation[]>(false, false, (type, s) =>
                {
                    if (type.Name.Contains("Happiness"))
                    {
                        return s.IsEmpty() ? "Unknown" : s.Mid(18).Replace(";", "");
                    }
                    else
                        return s;
                });

                if (Factions != null)   // if read okay
                {
                    //Factions = Factions.OrderByDescending(x => x.Influence)?.ToArray();  // POST 2.3

                    foreach (var x in Factions)     // normalise localised and store in class UTC time for use in other places
                    {
                        x.Happiness_Localised = JournalFieldNaming.CheckLocalisation(x.Happiness_Localised, x.Happiness.ToString());
                        x.UTC = utc;
                        x.System = sys;
                    }
                }
                else
                {
                    global::System.Diagnostics.Trace.WriteLine($"Bad Factions read {evt}");
                }

                return Factions;
            }
        };

        // from journal location or fsd jump or carrier jump, conflict info
        [System.Diagnostics.DebuggerDisplay("{Faction1.Name} vs {Faction2.Name} {WarType} {Status}")]
        public class ConflictInfo   // 3.4
        {
            public enum WarTypeState
            {
                Unknown,
                War,        // lower case in frontier, but the json decoder allows case insenitivity for enums
                CivilWar,
                Election,
            }
            public WarTypeState WarType { get; set; }

            public enum StatusState
            {
                NoStatus,        // found in numerous logs as "" - map to NoStatus
                Active,
                Pending,
            }
            public StatusState Status { get; set; }

            public class ConflictFactionInfo   // 3.4
            {
                public string Name { get; set; }        // faction name
                public string Stake { get; set; }
                public int WonDays { get; set; }
            }


            public ConflictFactionInfo Faction1 { get; set; }
            public ConflictFactionInfo Faction2 { get; set; }
            public DateTime UTC { get; set; }           // EDD addition, UTC of record

            public static ConflictInfo[] ReadJSON(JArray evt, DateTime utc)
            {
                var Conflicts = evt.ToObject<ConflictInfo[]>(false, false, (t, x) =>
                    {
                        return x.Length > 0 ? x : t.Name == "StatusState" ? nameof(ConflictInfo.StatusState.NoStatus) : nameof(ConflictInfo.WarTypeState.Unknown);
                    }
                );

                if (Conflicts != null)
                {
                    foreach (var x in Conflicts)        // set up UTC
                    {
                        x.UTC = utc;
                    }
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"*** Bad decode conflict status {evt["Conflicts"]?.ToString()}");
                }

                return Conflicts;
            }

            // long form report into string builder
            public void ToString(System.Text.StringBuilder sb)
            {
                sb.AppendLine($"Conflict: {WarType.ToString().SplitCapsWordFull()}, {Status.ToString().SplitCapsWordFull()} : {Faction1.Name} vs {Faction2.Name}");
                sb.AppendLine($"  Won days: {Faction1.WonDays} vs {Faction2.WonDays}");
                string s = BaseUtils.FieldBuilder.Build($"  Stake: ", Faction1.Stake, "< vs ", Faction2.Stake);
                if (s != null)
                    sb.AppendLine(s);
            }
        }
    }
}


