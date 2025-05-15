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
 */

using QuickJSON;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Docked)]
    public class JournalDocked : JournalEntry, IStatsJournalEntry, ILocDocked
    {
        public JournalDocked(System.DateTime utc) : base(utc, JournalTypeEnum.Docked)
        {
        }

        public JournalDocked(JObject evt ) : base(evt, JournalTypeEnum.Docked)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            StationState = StationDefinitions.StarportStateToEnum( evt["StationState"].Str("None") );    // missed, added, nov 22, only on bad starports.  Default None
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            MarketID = evt["MarketID"].LongNull();
            CockpitBreach = evt["CockpitBreach"].Bool();

            JToken jk = (JToken)evt["StationFaction"];
            if (jk != null && jk.IsObject)     // new 3.03
            {
                Faction = jk["Name"].Str();                // system faction pick up
                FactionState = FactionDefinitions.FactionStateToEnum(jk["FactionState"].Str("Unknown")).Value;
            }
            else
            {
                // old pre 3.3.3 had this
                Faction = evt.MultiStr(new string[] { "StationFaction", "Faction" });
                FactionState = FactionDefinitions.FactionStateToEnum(evt["FactionState"].Str("Unknown")).Value;           // PRE 2.3 .. not present in newer files, fixed up in next bit of code (but see 3.3.2 as its been incorrectly reintroduced)
            }
            
            StationFactionStateTranslated = FactionDefinitions.ToLocalisedLanguage(FactionState); // null if not present

            Allegiance = AllegianceDefinitions.ToEnum( evt.MultiStr(new string[] { "StationAllegiance", "Allegiance" }, null) );    // may not be present, pass null to accept it

            Economy = EconomyDefinitions.ToEnum(evt.MultiStr(evt.MultiStr(new string[] { "StationEconomy", "Economy" }, null)));    // may not be present
            Economy_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "StationEconomy_Localised", "Economy_Localised" }), EconomyDefinitions.ToEnglish(Economy));

            EconomyList = EconomyDefinitions.ReadEconomiesClassFromJson(evt["StationEconomies"]);        // not checking custom attributes, so name in class

            Government = GovernmentDefinitions.ToEnum(evt.MultiStr(new string[] { "StationGovernment", "Government" }, null));
            Government_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "StationGovernment_Localised", "Government_Localised" }), GovernmentDefinitions.ToEnglish(Government));

            Wanted = evt["Wanted"].Bool();

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);

            ActiveFine = evt["ActiveFine"].BoolNull();

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();

            LandingPads = evt["LandingPads"]?.ToObjectQ<LandingPadList>();      // only from odyssey release 5
        }

        public bool Docked { get; set; } = true;        // always docked but used for commonality with Location
        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public StationDefinitions.StarportState StationState { get; set; }            // fdname, only present in stations not normal - UnderAttack, Damaged, UnderRepairs. None otherwise
        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public long? MarketID { get; set; }
        public bool CockpitBreach { get; set; }
        public string Faction { get; set; }
        public string StationFaction { get => Faction; }            // alias
        public FactionDefinitions.State FactionState { get; set; }       // FDName
        public FactionDefinitions.State StationFactionState { get => FactionState; }    // alias for commonality with Location
        public string StationFactionStateTranslated { get; set; }       // alias for commonality with Location
        public AllegianceDefinitions.Allegiance Allegiance { get; set; }          // FDName
        public AllegianceDefinitions.Allegiance StationAllegiance { get => Allegiance; } // alias for commonality with Location
        public EconomyDefinitions.Economy Economy { get; set; }
        public string Economy_Localised { get; set; }
        public EconomyDefinitions.Economies[] EconomyList { get; set; }        // may be null
        public EconomyDefinitions.Economies[] StationEconomyList { get => EconomyList; } // alias for commonality with Location
        public GovernmentDefinitions.Government Government { get; set; }
        public GovernmentDefinitions.Government StationGovernment { get => Government; } // alias for commonality with Location
        public string Government_Localised { get; set; }
        public string StationGovernment_Localised { get => Government_Localised; } // alias for commonality with Location
        public StationDefinitions.StationServices[] StationServices { get; set; }   // fdname
        public bool Wanted { get; set; }
        public bool? ActiveFine { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }
        public LandingPadList LandingPads { get; set; } // 4.0 update 5, may be null

        public bool IsTrainingEvent { get; private set; }

        // these are EconomyDefinitions.Economies
        public bool HasAnyEconomyTypes(string[] fdnames)
        {
            return fdnames != null && (fdnames.Equals(Economy.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                            (EconomyList != null && Array.FindIndex(EconomyList, 0, x => fdnames.Equals(x.Name.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0) >= 0));
        }

        // these are StationDefinitions.StationServices
        public bool HasAnyServicesTypes(string[] fdnames)
        {
            return fdnames != null && StationServices != null && Array.FindIndex(StationServices, 0, x => fdnames.Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) >= 0) >= 0;
        }

        public class LandingPadList
        {
            public int Small;
            public int Medium;
            public int Large;
        };

        public override string SummaryName(ISystem sys) { return string.Format("At {0}".T(EDCTx.JournalDocked_At), StationName_Localised); }

        public override string GetInfo()
        {
            var sb = new System.Text.StringBuilder(256);
            sb.Build("", "Docked".T(EDCTx.JournalTypeEnum_Docked), "Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType), "< in system ".T(EDCTx.JournalEntry_insystem), StarSystem,
                "State: ".TxID(EDCTx.JournalLocOrJump_State), StationDefinitions.ToLocalisedLanguage(StationState),
                ";(Wanted)".T(EDCTx.JournalEntry_Wanted), Wanted,
                ";Active Fine".T(EDCTx.JournalEntry_ActiveFine), ActiveFine,
                "Faction: ".T(EDCTx.JournalEntry_Faction), Faction,
                "< in state ".T(EDCTx.JournalEntry_instate), FactionDefinitions.ToLocalisedLanguage(FactionState));

            return sb.ToString();
        }

        public override string GetDetailed()
        {
            var sb = new System.Text.StringBuilder(256);

            sb.Build("Allegiance: ".T(EDCTx.JournalEntry_Allegiance), AllegianceDefinitions.ToLocalisedLanguage(Allegiance), 
                    "Economy: ".T(EDCTx.JournalEntry_Economy), EconomyDefinitions.ToLocalisedLanguage(Economy),
                    "Government: ".T(EDCTx.JournalEntry_Government),  GovernmentDefinitions.ToLocalisedLanguage(Government));

            if (StationServices != null)
            {
                sb.AppendCR();
                StationDefinitions.Build(sb, true, StationServices);
            }

            if (EconomyList != null)
            {
                sb.AppendCR();
                EconomyDefinitions.Build(sb, true, EconomyList);
            }

            return sb.ToString();
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (Faction.HasChars())
                stats.Docking(system,this);
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingCancelled)]
    public class JournalDockingCancelled : JournalEntry
    {
        public JournalDockingCancelled(JObject evt) : base(evt, JournalTypeEnum.DockingCancelled)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }

        public override string GetInfo()
        {
            return StationName_Localised;
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingDenied)]
    public class JournalDockingDenied : JournalEntry
    {
        public JournalDockingDenied(JObject evt) : base(evt, JournalTypeEnum.DockingDenied)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDReason = evt["Reason"].Str();
            Reason = JournalFieldNaming.DockingDeniedReason(FDReason);
            FDStationType = StationDefinitions.StarportTypeToEnum( evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string Reason { get; set; }      // friendly reason make cleaner
        public string FDReason { get; set; }    // frontier ID
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", StationName_Localised, "", Reason);
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingGranted)]
    public class JournalDockingGranted : JournalEntry
    {
        public JournalDockingGranted(JObject evt) : base(evt, JournalTypeEnum.DockingGranted)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            LandingPad = evt["LandingPad"].Int();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());    // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public int LandingPad { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", StationName_Localised, "< on pad ".T(EDCTx.JournalEntry_onpad), LandingPad, "Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType));
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingRequested)]
    public class JournalDockingRequested : JournalEntry
    {
        public JournalDockingRequested(JObject evt) : base(evt, JournalTypeEnum.DockingRequested)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());        // may not be there in earlier ones
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
            LandingPads = evt["LandingPads"]?.ToObjectQ<JournalDocked.LandingPadList>();      // only from odyssey release 5
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }
        public JournalDocked.LandingPadList LandingPads { get; set; } // 4.0 update 5

        public override string GetInfo()
        {
            return StationName_Localised;
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingTimeout)]
    public class JournalDockingTimeout : JournalEntry
    {
        public JournalDockingTimeout(JObject evt) : base(evt, JournalTypeEnum.DockingTimeout)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull()); // may not be present
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }

        public override string GetInfo()
        {
            return StationName_Localised;
        }
    }


    [JournalEntryType(JournalTypeEnum.Undocked)]
    public class JournalUndocked : JournalEntry
    {
        public JournalUndocked(JObject evt) : base(evt, JournalTypeEnum.Undocked)
        {
            StationName = evt["StationName"].Str();
            StationName_Localised = JournalFieldNaming.CheckLocalisation(evt["StationName_Localised"].Str(), evt["StationName"].Str());
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
        }

        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // english, only on later events, else Unknown
        public StationDefinitions.StarportTypes FDStationType { get; set; }  // only on later events, else Unknown
        public long? MarketID { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", StationName_Localised, "Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType));
        }
    }


}
