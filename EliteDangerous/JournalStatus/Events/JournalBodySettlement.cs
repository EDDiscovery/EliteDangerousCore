/*
 * Copyright 2016-2026 EDDiscovery development team
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

using EliteDangerousCore.StarScan2;
using QuickJSON;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.ApproachBody)]
    public class JournalApproachBody : JournalEntry, IBodyFeature, IStarScan
    {
        public JournalApproachBody(JObject evt) : base(evt, JournalTypeEnum.ApproachBody)
        {
            StarSystem = evt["StarSystem"].StrNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].Int();
        }

        public int BodyID { get; set; }                // always been there

        public string Body { get; set; }                // always been there

        // IBodyNameAndID

        public string StarSystem { get; set; }          // very early ones missed this, augmented by AddStarScan
        public long? SystemAddress { get; set; }        // always been there
        public BodyDefinitions.BodyType BodyType => BodyDefinitions.BodyType.Planet;
        public string BodyName => Body;
        int? IBodyFeature.BodyID => BodyID;
        public double? Latitude { get => null; set { } }
        public double? Longitude { get => null; set { } }
        public bool HasLatLong => false;
        public string Name => null;
        public string Name_Localised => null;
        public long? MarketID => null;
        public StationDefinitions.StarportTypes FDStationType => StationDefinitions.StarportTypes.Unknown;
        public string StationFaction => null;

        public override string SummaryName(ISystem sys)
        {
            string sn = base.SummaryName(sys);
            sn += " " + Body.ReplaceIfStartsWith(StarSystem);
            return sn;
        }

        public override string GetInfo()
        {
            return "In ".Tx()+ StarSystem;
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            if (StarSystem == null)
                StarSystem = system.Name;
            if (SystemAddress == null)
                SystemAddress = system.SystemAddress;
            s.AddLocation(this, system);
        }
    }

    [JournalEntryType(JournalTypeEnum.LeaveBody)]
    public class JournalLeaveBody : JournalEntry, IBodyFeature, IStarScan
    {
        public JournalLeaveBody(JObject evt) : base(evt, JournalTypeEnum.LeaveBody)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].Int();
        }

        public string Body { get; set; }                // always been there
        public int BodyID { get; set; }        // always been there

        // IBodyNameAndID
        public string StarSystem { get; set; }      // always been there
        public long? SystemAddress { get; set; }// always been there
        public BodyDefinitions.BodyType BodyType => BodyDefinitions.BodyType.Planet;
        public string BodyName => Body;
        int? IBodyFeature.BodyID => BodyID;
        public double? Latitude { get => null; set { } }
        public double? Longitude { get => null; set { } }
        public bool HasLatLong => false;
        public string Name => null;
        public string Name_Localised => null;
        public long? MarketID => null;
        public string StationFaction => null;
        public StationDefinitions.StarportTypes FDStationType => StationDefinitions.StarportTypes.Unknown;


        public override string SummaryName(ISystem sys)
        {
            string sn = base.SummaryName(sys);
            sn += " " + Body.ReplaceIfStartsWith(StarSystem);
            return sn;
        }

        public override string GetInfo()
        {
            return "In ".Tx()+ StarSystem;
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddLocation(this, system);
        }
    }

    [JournalEntryType(JournalTypeEnum.ApproachSettlement)]
    public class JournalApproachSettlement : JournalEntry, IBodyFeature, IStarScan
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

            EconomyList = EconomyDefinitions.ReadEconomiesClassFromJson(evt["StationEconomies"]);

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);

            Faction = evt["StationFaction"].I("Name").StrNull();        // may not be present, so null if not
            FactionState = FactionDefinitions.FactionStateToEnum(evt["StationFaction"].I("FactionState").StrNull());    // null if not present
            FriendlyFactionState = FactionDefinitions.ToEnglish(FactionState); // null if not present

            StationAllegiance = AllegianceDefinitions.ToEnum( evt["StationAllegiance"].StrNull());

            //if (FactionState != null)  System.Diagnostics.Debug.WriteLine($"Faction {Faction} {FactionState} {FriendlyFactionState}");
        }

        public string Name { get; set; }            // always there
        public string Name_Localised { get; set; }  // there for $Ancient; etc
        public string StationType => "OnFootSettlement";// Fixed to this, no information on specific type of planet station, aligned with Docked
        public StationDefinitions.StarportTypes FDStationType => StationDefinitions.StarportTypes.OnFootSettlement;     // Fixed to this, no information on specific type of planet station
        public long? MarketID { get; set; }         // appears 2018
        public double? Latitude { get; set; }       // 3.3
        public double? Longitude { get; set; }
        public bool HasLatLong { get { return Latitude.HasValue && Longitude.HasValue; } }
        public string StarSystem { get; set; }      // augmented by AddStarScan below
        public long? SystemAddress { get; set; }    // from event, may be null, 2019, augmented by AddStarScan below
        public int? BodyID { get; set; }            // from event, may be null as 2016 ones did not have it in. 2019 it came in
        public string BodyName { get; set; }        // from event, may be null
        public BodyDefinitions.BodyType BodyType => BodyDefinitions.BodyType.Planet;
        public GovernmentDefinitions.Government StationGovernment { get; set; }// may be null
        public string StationGovernment_Localised { get; set; }// may be null
        public EconomyDefinitions.Economy StationEconomy { get; set; }// may be null
        public string StationEconomy_Localised { get; set; }// may be null
        public EconomyDefinitions.Economies[] EconomyList { get; set; }        // may be null
        public StationDefinitions.StationServices[] StationServices { get; set; }       // may be null, fdnames
        public string Faction { get; set; }       //may be null
        public FactionDefinitions.State? FactionState { get; set; }       //may be null, FDName
        public string FriendlyFactionState { get; set; }       //may be null, in english
        public AllegianceDefinitions.Allegiance StationAllegiance { get; set; } //fdname, may be null

        // IBodyFeature 
        public string StationFaction => Faction;       

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name_Localised, "< (;)", BodyName, "Latitude: ;°;F4".Tx(), Latitude, "Longitude: ;°;F4".Tx(), Longitude);
        }

        public override string GetDetailed()
        {
            if (StationGovernment != GovernmentDefinitions.Government.Unknown)      // update 17
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

                sb.Build("Economy".Tx()+": ", EconomyDefinitions.ToLocalisedLanguage(StationEconomy),
                    "Government".Tx()+": ", GovernmentDefinitions.ToLocalisedLanguage(StationGovernment),
                    "Faction".Tx()+": ", Faction,
                    "< in state ".Tx(), FactionDefinitions.ToLocalisedLanguage(FactionState),
                    "Allegiance".Tx()+": ", AllegianceDefinitions.ToLocalisedLanguage(StationAllegiance));

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
            else
                return null;
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            StarSystem = system.Name;
            if (SystemAddress == null)
                SystemAddress = system.SystemAddress;
            s.AddApproachSettlement(this, system);
        }
    }


}
