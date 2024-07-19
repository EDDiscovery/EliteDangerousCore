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
    [JournalEntryType(JournalTypeEnum.ApproachBody)]
    public class JournalApproachBody : JournalEntry, IBodyNameAndID
    {
        public JournalApproachBody(JObject evt) : base(evt, JournalTypeEnum.ApproachBody)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get { return "Planet"; } }
        public string BodyDesignation { get; set; }

        public override string SummaryName(ISystem sys)
        {
            string sn = base.SummaryName(sys);
            sn += " " + Body.ReplaceIfStartsWith(StarSystem);
            return sn;
        }

        public override void FillInformation(out string info, out string detailed) 
        {
            info = "In ".T(EDCTx.JournalApproachBody_In) + StarSystem;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.LeaveBody)]
    public class JournalLeaveBody : JournalEntry, IBodyNameAndID
    {
        public JournalLeaveBody(JObject evt) : base(evt, JournalTypeEnum.LeaveBody)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyDesignation { get; set; }
        public string BodyType { get { return "Planet"; } }

        public override string SummaryName(ISystem sys)
        {
            string sn = base.SummaryName(sys);
            sn += " " + Body.ReplaceIfStartsWith(StarSystem);
            return sn;
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = "In ".T(EDCTx.JournalLeaveBody_In) + StarSystem;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.ApproachSettlement)]
    public class JournalApproachSettlement : JournalEntry, IBodyFeature
    {
        public JournalApproachSettlement(JObject evt) : base(evt, JournalTypeEnum.ApproachSettlement)
        {
            Name = evt["Name"].Str();
            Name_Localised = JournalFieldNaming.CheckLocalisation(evt["Name_Localised"].Str(), Name);
            MarketID = evt["MarketID"].LongNull();
            Latitude = evt["Latitude"].DoubleNull();
            Longitude = evt["Longitude"].DoubleNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            BodyID = evt["BodyID"].IntNull();
            BodyName = evt["BodyName"].StrNull();

            //patch 17 content below

            StationGovernment = GovernmentDefinitions.ToEnum(evt["StationGovernment"].StrNull());       // may not be present
            StationGovernment_Localised = JournalFieldNaming.CheckLocalisation(evt["StationGovernment_Localised"].Str(), GovernmentDefinitions.ToEnglish(StationGovernment));

            StationEconomy = EconomyDefinitions.ToEnum(evt["StationEconomy"].StrNull());
            StationEconomy_Localised = JournalFieldNaming.CheckLocalisation(evt["StationEconomy_Localised"].StrNull(),  EconomyDefinitions.ToEnglish(StationEconomy));

            EconomyList = evt["StationEconomies"]?.ToObjectQ<JournalDocked.Economies[]>();

            StationServices = evt["StationServices"]?.ToObjectQ<StationDefinitions.StationServices[]>();

            Faction = evt["StationFaction"].I("Name").StrNull();        // may not be present, so null if not
            FactionState = FactionDefinitions.ToEnum(evt["StationFaction"].I("FactionState").StrNull());    // null if not present
            FriendlyFactionState = FactionDefinitions.ToEnglish(FactionState); // null if not present

            StationAllegiance = AllegianceDefinitions.ToEnum( evt["StationAllegiance"].StrNull());

            //if (FactionState != null)  System.Diagnostics.Debug.WriteLine($"Faction {Faction} {FactionState} {FriendlyFactionState}");
        }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long? MarketID { get; set; }
        public double? Latitude { get; set; }    // 3.3
        public double? Longitude { get; set; }
        public bool HasLatLong { get { return Latitude.HasValue && Longitude.HasValue; } }  
        public long? SystemAddress { get; set; } // may be null
        public int? BodyID { get; set; } // may be null
        public string BodyName { get; set; }        // from event, may be null
        public string BodyType { get { return "Settlement"; } }
        public GovernmentDefinitions.Government StationGovernment { get; set; }// may be null
        public string StationGovernment_Localised { get; set; }// may be null
        public EconomyDefinitions.Economy StationEconomy { get; set; }// may be null
        public string StationEconomy_Localised { get; set; }// may be null
        public JournalDocked.Economies[] EconomyList { get; set; }        // may be null
        public StationDefinitions.StationServices[] StationServices { get; set; }       // may be null, fdnames
        public string Faction { get; set; }       //may be null
        public FactionDefinitions.State? FactionState { get; set; }       //may be null, FDName
        public string FriendlyFactionState { get; set; }       //may be null, in english
        public AllegianceDefinitions.Allegiance StationAllegiance { get; set; } //fdname, may be null


        // IBodyFeature only
        public string Body { get { return BodyName; } }     // this is an alias
        public string BodyDesignation { get; set; }
        public string StarSystem { get; set; }      // filled in by StarScan::AddApproachSettlement

        public override void FillInformation(out string info, out string detailed)
        {
            info = Name_Localised + " (" + BodyName + ")";

            if (Latitude != null && Longitude != null)
                info += " " + JournalFieldNaming.RLat(Latitude) + " " + JournalFieldNaming.RLong(Longitude);

            detailed = "";

            if (StationGovernment != GovernmentDefinitions.Government.Unknown)      // update 17
            {
                detailed = BaseUtils.FieldBuilder.Build("Economy: ".T(EDCTx.JournalEntry_Economy), EconomyDefinitions.ToLocalisedLanguage(StationEconomy), 
                    "Government: ".T(EDCTx.JournalEntry_Government), GovernmentDefinitions.ToLocalisedLanguage(StationGovernment),
                    "Faction: ".T(EDCTx.JournalEntry_Faction), Faction, 
                    "< in state ".T(EDCTx.JournalEntry_instate), FactionDefinitions.ToLocalisedLanguage(FactionState),
                    "Allegiance: ".T(EDCTx.JournalEntry_Allegiance), AllegianceDefinitions.ToLocalisedLanguage(StationAllegiance));

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
                    foreach (JournalDocked.Economies e in EconomyList)
                        l = l.AppendPrePad(EconomyDefinitions.ToLocalisedLanguage(e.Name) + " " + (e.Proportion * 100).ToString("0.#") + "%", ", ");
                    detailed += System.Environment.NewLine + "Economies: ".T(EDCTx.JournalEntry_Economies) + l;
                }
            }

        }

    }


}
