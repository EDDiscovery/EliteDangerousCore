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
    public class JournalDocked : JournalEntry
    {
        public JournalDocked(System.DateTime utc) : base(utc, JournalTypeEnum.Docked,false)
        {
        }

        public JournalDocked(JObject evt ) : base(evt, JournalTypeEnum.Docked)
        {
            StationName = evt["StationName"].Str();
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
                FactionState = FactionDefinitions.ToEnum(jk["FactionState"].Str("Unknown")).Value;
            }
            else
            {
                // old pre 3.3.3 had this
                Faction = evt.MultiStr(new string[] { "StationFaction", "Faction" });
                FactionState = FactionDefinitions.ToEnum(evt["FactionState"].Str("Unknown")).Value;           // PRE 2.3 .. not present in newer files, fixed up in next bit of code (but see 3.3.2 as its been incorrectly reintroduced)
            }

            Allegiance = AllegianceDefinitions.ToEnum( evt.MultiStr(new string[] { "StationAllegiance", "Allegiance" }, null) );    // may not be present, pass null to accept it

            Economy = EconomyDefinitions.ToEnum(evt.MultiStr(evt.MultiStr(new string[] { "StationEconomy", "Economy" }, null)));    // may not be present
            Economy_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "StationEconomy_Localised", "Economy_Localised" }), EconomyDefinitions.ToEnglish(Economy));

            EconomyList = evt["StationEconomies"]?.ToObjectQ<Economies[]>();        // not checking custom attributes, so name in class

            Government = GovernmentDefinitions.ToEnum(evt.MultiStr(new string[] { "StationGovernment", "Government" }, null));
            Government_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "StationGovernment_Localised", "Government_Localised" }), GovernmentDefinitions.ToEnglish(Government));

            Wanted = evt["Wanted"].Bool();

            StationServices = evt["StationServices"]?.ToObjectQ<StationDefinitions.StationServices[]>();

            ActiveFine = evt["ActiveFine"].BoolNull();

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();

            LandingPads = evt["LandingPads"]?.ToObjectQ<LandingPadList>();      // only from odyssey release 5
        }

        public string StationName { get; set; }
        public string StationType { get; set; }             // Friendly name, used in action packs, may be null
        public StationDefinitions.StarportTypes FDStationType { get; set; }           // may be null
        public StationDefinitions.StarportState StationState { get; set; }            // fdname, only present in stations not normal - UnderAttack, Damaged, UnderRepairs. None otherwise
        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public long? MarketID { get; set; }
        public bool CockpitBreach { get; set; }
        public string Faction { get; set; }
        public FactionDefinitions.State FactionState { get; set; }       //may be null, FDName
        public AllegianceDefinitions.Allegiance Allegiance { get; set; }          // FDName
        public EconomyDefinitions.Economy Economy { get; set; }
        public string Economy_Localised { get; set; }
        public Economies[] EconomyList { get; set; }        // may be null
        public GovernmentDefinitions.Government Government { get; set; }
        public string Government_Localised { get; set; }
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

        public class Economies
        {
            [JsonName("name", "Name")]                  //name is for spansh, Name is for journal
            public EconomyDefinitions.Economy Name;     // fdname
            public string Name_Localised;
            [JsonName("Proportion", "share")]           //share is for spansh, proportion is for journal
            public double Proportion;                   // 0-1
        }


        public class LandingPadList
        {
            public int Small;
            public int Medium;
            public int Large;
        };

        public override string SummaryName(ISystem sys) { return string.Format("At {0}".T(EDCTx.JournalDocked_At), StationName); }

        public override void FillInformation(out string info, out string detailed)
        {
            FillInformation(out info, out detailed, true);
        }

        public void FillInformation(out string info, out string detailed, bool printdocked)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop); // because of translation

            info = "";
            
            if ( printdocked )
                info += "Docked".T(EDCTx.JournalTypeEnum_Docked) + ", ";

            info += BaseUtils.FieldBuilder.Build("Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType), "< in system ".T(EDCTx.JournalEntry_insystem), StarSystem, 
                "State: ".TxID(EDCTx.JournalLocOrJump_State), StationDefinitions.ToLocalisedLanguage(StationState),
                ";(Wanted)".T(EDCTx.JournalEntry_Wanted), Wanted, 
                ";Active Fine".T(EDCTx.JournalEntry_ActiveFine),ActiveFine,
                "Faction: ".T(EDCTx.JournalEntry_Faction), Faction,  
                "< in state ".T(EDCTx.JournalEntry_instate), FactionDefinitions.ToLocalisedLanguage(FactionState));

            detailed = BaseUtils.FieldBuilder.Build("Allegiance: ".T(EDCTx.JournalEntry_Allegiance), AllegianceDefinitions.ToLocalisedLanguage(Allegiance), 
                    "Economy: ".T(EDCTx.JournalEntry_Economy), EconomyDefinitions.ToLocalisedLanguage(Economy),
                    "Government: ".T(EDCTx.JournalEntry_Government),  GovernmentDefinitions.ToLocalisedLanguage(Government));

            if (StationServices != null)
            {
                string l = "";
                foreach (var s in StationServices)
                    l = l.AppendPrePad(StationDefinitions.ToLocalisedLanguage(s), ", ");
                detailed += System.Environment.NewLine + "Station services: ".T(EDCTx.JournalEntry_Stationservices) + l;
            }

            if (EconomyList != null)
            {
                string l = "";
                foreach (Economies e in EconomyList)
                    l = l.AppendPrePad(EconomyDefinitions.ToLocalisedLanguage(e.Name) + " " + (e.Proportion * 100).ToString("0.#") + "%", ", ");
                detailed += System.Environment.NewLine + "Economies: ".T(EDCTx.JournalEntry_Economies) + l;
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.DockingCancelled)]
    public class JournalDockingCancelled : JournalEntry
    {
        public JournalDockingCancelled(JObject evt) : base(evt, JournalTypeEnum.DockingCancelled)
        {
            StationName = evt["StationName"].Str();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationType { get; set; } // friendly
        public StationDefinitions.StarportTypes FDStationType { get; set; } 
        public long? MarketID { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = StationName;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingDenied)]
    public class JournalDockingDenied : JournalEntry
    {
        public JournalDockingDenied(JObject evt) : base(evt, JournalTypeEnum.DockingDenied)
        {
            StationName = evt["StationName"].Str();
            FDReason = evt["Reason"].Str();
            Reason = JournalFieldNaming.DockingDeniedReason(FDReason);
            FDStationType = StationDefinitions.StarportTypeToEnum( evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; } 
        public string Reason { get; set; }      // friendly reason make cleaner
        public string FDReason { get; set; }    // frontier ID
        public string StationType { get; set; } // friendly, in english
        public StationDefinitions.StarportTypes FDStationType { get; set; }  
        public long? MarketID { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", StationName, "", Reason);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingGranted)]
    public class JournalDockingGranted : JournalEntry
    {
        public JournalDockingGranted(JObject evt) : base(evt, JournalTypeEnum.DockingGranted)
        {
            StationName = evt["StationName"].Str();
            LandingPad = evt["LandingPad"].Int();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());    // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public int LandingPad { get; set; }
        public string StationType { get; set; } // friendly
        public StationDefinitions.StarportTypes FDStationType { get; set; }   
        public long? MarketID { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", StationName, "< on pad ".T(EDCTx.JournalEntry_onpad), LandingPad, "Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType));
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingRequested)]
    public class JournalDockingRequested : JournalEntry
    {
        public JournalDockingRequested(JObject evt) : base(evt, JournalTypeEnum.DockingRequested)
        {
            StationName = evt["StationName"].Str();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());        // may not be there in earlier ones
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
            LandingPads = evt["LandingPads"]?.ToObjectQ<JournalDocked.LandingPadList>();      // only from odyssey release 5
        }

        public string StationName { get; set; }
        public string StationType { get; set; } // friendly
        public StationDefinitions.StarportTypes FDStationType { get; set; }  
        public long? MarketID { get; set; }
        public JournalDocked.LandingPadList LandingPads { get; set; } // 4.0 update 5

        public override void FillInformation(out string info, out string detailed)
        {
            info = StationName;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.DockingTimeout)]
    public class JournalDockingTimeout : JournalEntry
    {
        public JournalDockingTimeout(JObject evt) : base(evt, JournalTypeEnum.DockingTimeout)
        {
            StationName = evt["StationName"].Str();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull()); // may not be present
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public string StationName { get; set; }
        public string StationType { get; set; } // friendly
        public StationDefinitions.StarportTypes FDStationType { get; set; }  
        public long? MarketID { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = StationName;
            detailed = "";
        }
    }


    [JournalEntryType(JournalTypeEnum.Undocked)]
    public class JournalUndocked : JournalEntry
    {
        public JournalUndocked(JObject evt) : base(evt, JournalTypeEnum.Undocked)
        {
            StationName = evt["StationName"].Str();
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());  // may not be there
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
        }

        public string StationName { get; set; }
        public string StationType { get; set; } // friendly, may be null
        public StationDefinitions.StarportTypes FDStationType { get; set; }   // may be null
        public long? MarketID { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", StationName, "Type: ".T(EDCTx.JournalEntry_Type), StationDefinitions.ToLocalisedLanguage(FDStationType));
            detailed = "";
        }
    }


}
