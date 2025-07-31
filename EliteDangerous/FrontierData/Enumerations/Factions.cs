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
using System.Collections.Generic;
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

        public static State? FactionStateToEnum(string englishname)
        {
            if (englishname == null)
            {
                //System.Diagnostics.Trace.WriteLine($"**** No faction state");
                return null;
            }
            else if (parseliststate.TryGetValue(englishname.ToLowerInvariant().Trim(),out State value)) // case insensitive
            {
                return value;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"**** Unknown faction state `{englishname}`");
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
            return ToEnglish(stat).Tx();
        }

        public static void IDSTx()
        {
            foreach (var name in Enum.GetNames(typeof(State)))
                System.Diagnostics.Trace.WriteLine($".{name}: \"{name.SplitCapsWordFull()}\" @");
        }

        static Dictionary<string, State> parseliststate;
        static FactionDefinitions()
        {
            parseliststate = new Dictionary<string, State>();
            foreach (var v in Enum.GetValues(typeof(State)))
                parseliststate[v.ToString().ToLowerInvariant()] = (State)v;
        }

        // from journal location or fsd jump or carrier jump, faction info array
        [System.Diagnostics.DebuggerDisplay("{Name} {FactionState} {Government} {Allegiance}")]
        public class FactionInformation
        {
            public string Name { get; set; }                                    // All properties except noted from Json of event
            public State FactionState { get; set; }
            public GovernmentDefinitions.Government Government { get; set; }
            public double Influence { get; set; }                               // influence of faction in system, fractional
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
                    return "Unknown".Tx();
                else if (value < -90)
                    return "Hostile".Tx();
                else if (value <= -35)
                    return "Unfriendly".Tx();
                else if (value <= 4)
                    return "Neutral".Tx();
                else if (value <= 35)
                    return "Cordial".Tx();
                else if (value <= 90)
                    return "Friendly".Tx();
                else
                    return "Allied".Tx();
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

            // long form report into string builder, with a LF at the end
            public void ToString(System.Text.StringBuilder sb, bool datetime, bool frontpart, bool otherinfo, bool pendingstates, bool recoveringstates, bool activestates)
            {
                if (datetime)
                    sb.Append(EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(UTC).ToString("yyyy/MM/dd") + Environment.NewLine);

                if (frontpart)
                {
                    sb.Build("", Name,  "State: ".Tx(), FactionDefinitions.ToLocalisedLanguage(FactionState),
                                        "Reputation: ;%;N1".Tx(), MyReputation,
                                        "Government: ".Tx(), GovernmentDefinitions.ToLocalisedLanguage(Government),
                                        "Allegiance: ".Tx(), AllegianceDefinitions.ToLocalisedLanguage(Allegiance),
                                        "Inf: ;%".Tx(), (Influence * 100.0).ToString("0.0")
                                                                   );
                }

                if (otherinfo)
                {
                    sb.BuildCont("Happiness: ".Tx(), Happiness_Localised,
                                                                   ";Squadron System".Tx(), SquadronFaction,
                                                                   ";Happiest System".Tx(), HappiestSystem,
                                                                   ";Home System".Tx(), HomeSystem
                                                                   );
                }

                if (pendingstates && PendingStates != null)
                {
                    sb.AppendCS();
                    sb.Append("Pending State: ".Tx());

                    foreach (FactionDefinitions.FactionInformation.PowerStatesInfo state in PendingStates)
                        sb.Build(" ", state.State, "<(;)", state.Trend);

                }

                if (recoveringstates && RecoveringStates != null)
                {
                    sb.AppendCS();
                    sb.Append("Recovering State: ".Tx());

                    foreach (FactionDefinitions.FactionInformation.PowerStatesInfo state in RecoveringStates)
                        sb.Build(" ", state.State, "<(;)", state.Trend);
                }

                if (activestates && ActiveStates != null)
                {
                    sb.AppendCS();
                    sb.Append("Active State: ".Tx());

                    foreach (FactionDefinitions.FactionInformation.ActiveStatesInfo state in ActiveStates)
                        sb.Build(" ", state.State);
                }
                sb.Append(Environment.NewLine);
            }

            // handle reading from JSON this field
            public static FactionInformation[] ReadJSON(JArray evt, DateTime utc, ISystem sys)
            {
                var Factions = evt.ToObject<FactionInformation[]>(false, process: (t, x) =>
                {
                    if (t.Name.Contains("Happiness"))
                    {
                        return x.IsEmpty() ? HappinessState.Unknown : (HappinessState)Enum.Parse(typeof(HappinessState), x.Mid(18).Replace(";", ""), true);
                    }
                    else if (t.Name.Contains("Government"))
                        return GovernmentDefinitions.ToEnum(x);
                    else if (t.Name.Contains("Allegiance"))
                        return AllegianceDefinitions.ToEnum(x);
                    else if (t.Name.Contains("State"))
                        return FactionDefinitions.FactionStateToEnum(x);
                    else
                        System.Diagnostics.Debug.Assert(false);
                    return null;
                });

                if (Factions != null)   // if read okay
                {
                    //Factions = Factions.OrderByDescending(x => x.Influence)?.ToArray();  // POST 2.3

                    foreach (var x in Factions)     // normalise localised and store in class UTC time for use in other places
                    {
                        x.Happiness_Localised = JournalFieldNaming.CheckLocalisation(x.Happiness_Localised, x.Happiness.ToString());
                        x.UTC = utc;
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
                public string Stake_Localised { get; set; }
                public int WonDays { get; set; }
                public string StakeLoc { get { return Stake_Localised.HasChars() ? Stake_Localised : Stake; } }
            }


            public ConflictFactionInfo Faction1 { get; set; }
            public ConflictFactionInfo Faction2 { get; set; }
            public DateTime UTC { get; set; }           // EDD addition, UTC of record

            public static ConflictInfo[] ReadJSON(JArray evt, DateTime utc)
            {
                var Conflicts = evt.ToObject<ConflictInfo[]>(false, process: (t, x) =>
                    {
                        if ( t == typeof(WarTypeState))
                            return x.Length > 0 ? (WarTypeState)Enum.Parse(typeof(WarTypeState), x, true) : ConflictInfo.WarTypeState.Unknown;
                        else
                            return x.Length > 0 ? (StatusState)Enum.Parse(typeof(StatusState), x, true) : ConflictInfo.StatusState.NoStatus;
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
                    System.Diagnostics.Trace.WriteLine($"*** Bad decode conflict status `{evt["Conflicts"]?.ToString()}`");
                }

                return Conflicts;
            }

            // long form report into string builder
            public void ToString(System.Text.StringBuilder sb)
            {
                sb.AppendLine($"Conflict: {WarType.ToString().SplitCapsWordFull()}, {Status.ToString().SplitCapsWordFull()} : {Faction1.Name} vs {Faction2.Name}");
                sb.AppendLine($"  Won days: {Faction1.WonDays} vs {Faction2.WonDays}");
                if ( Faction1.StakeLoc.HasChars() )
                {
                    sb.Append("  Stake: ");
                    sb.Append(Faction1.StakeLoc);

                    if (Faction2.StakeLoc.HasChars())
                    {
                        sb.Append(" vs ");
                        sb.Append(Faction2.StakeLoc);
                    }
                }
                else if (Faction2.StakeLoc.HasChars())
                {
                    sb.Append("  Stake: ");
                    sb.Append(Faction2.StakeLoc);
                }
            }
        }
    }
}


