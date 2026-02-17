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

using QuickJSON;
using System;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [System.Diagnostics.DebuggerDisplay("Location {EventTimeUTC} JID {Id} `{BodyName}` @ `{StarSystem}` d{Docked} `{StationName}`")]
    [JournalEntryType(JournalTypeEnum.Location)]
    public class JournalLocation : JournalLocOrJump, IStarScan, ILocDocked
    {
        public JournalLocation(JObject evt) : base(evt, JournalTypeEnum.Location)      // all have evidence 16/3/2017
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
            InSRV = evt["InSRV"].BoolNull();
            OnFoot = evt["OnFoot"].BoolNull();

            if (Docked)
            {
                var snl = JournalFieldNaming.GetStationNames(evt);
                Name = StationName = snl.Item1;                             // store it in both StationName for backwards compat, and Name for IBodyFeature
                Name_Localised = StationName_Localised = snl.Item2;
                string st = evt["StationType"].StrNull();
                if (st != null && st.Length == 0)        // seem empty ones
                    st = null;
                FDStationType = StationDefinitions.StarportTypeToEnum(st);    // may not be there
                StationType = StationDefinitions.ToEnglish(FDStationType);
            }
            else if ( BodyType == BodyDefinitions.BodyType.Station)         
            {
                // this means we are at the station location not docked but outside either in space or on foot
                // HES will now correct this if it knows the type back to the correct type

                // { "timestamp":"2017-07-03T21:16:10Z", "event":"Location", "Docked":false, "StarSystem":"Nemehi", "Body":"Haipeng Orbital", "BodyType":"Station",
                // { "timestamp":"2022-05-13T12:25:14Z", "event":"Location", "DistFromStarLS":591.029403,
                // "Docked":false, "OnFoot":true, "StarSystem":"Arque", "SystemAddress":113573366131, "StarPos":[66.50000,38.06250,61.12500],
                // ...  "Body":"Baird Gateway", "BodyID":43, 

                Name = StationName = Name_Localised = StationName_Localised = Body;
                FDStationType = StationDefinitions.StarportTypes.Station;
                StationType = StationDefinitions.ToEnglish(FDStationType);

                //  System.Diagnostics.Debug.WriteLine($"{EventTimeUTC.ToStringZuluInvariant()} Location Body Type Station {Name} onfoot {OnFoot}");
            }

            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            Latitude = evt["Latitude"].DoubleNull();
            Longitude = evt["Longitude"].DoubleNull();

            MarketID = evt["MarketID"].LongNull();

            // station data only if docked..

            JObject jk = evt["StationFaction"].Object();  // 3.3.3 post

            if ( jk != null )
            {
                StationFaction = jk["Name"].Str();                // system faction pick up
                StationFactionState = FactionDefinitions.FactionStateToEnum(jk["FactionState"].Str("Unknown")).Value;

                if (StationFactionState == FactionDefinitions.State.Unknown && Factions != null) // no data on this in event, but we have factions..
                {
                    int i = Array.FindIndex(Factions, x => x.Name == Faction);
                    if (i != -1)
                        StationFactionState = Factions[i].FactionState;        // set to State of controlling faction
                }

                StationFactionStateTranslated = FactionDefinitions.ToLocalisedLanguage(StationFactionState); // null if not present
            }
            else
            {
            }

            StationGovernment = GovernmentDefinitions.ToEnum(evt["StationGovernment"].StrNull());       // may be missing
            StationGovernment_Localised = JournalFieldNaming.CheckLocalisation(evt["StationGovernment_Localised"].Str(), GovernmentDefinitions.ToEnglish(Government));

            StationAllegiance = AllegianceDefinitions.ToEnum(evt["StationAllegiance"].StrNull());    // may be missing, due to training

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);          // need to read thru these functions to do some name mangling
            StationEconomyList = EconomyDefinitions.ReadEconomiesClassFromJson(evt["StationEconomies"]);

        }

        public JournalLocation(DateTime utc, string sysname, long? sysaddr,
                                string bodyname, BodyDefinitions.BodyType bt, int? bodyid,
                                string stationname = null, string stationnameloc = null, StationDefinitions.StarportTypes fdstationtype = StationDefinitions.StarportTypes.Unknown,
                                string name = null, string name_loc = null,
                                double? lat = null, double? lon = null) : base(utc,JournalTypeEnum.Location)
        {
            StarSystem = sysname; SystemAddress = sysaddr;
            Body = bodyname;  BodyType = bt; BodyID = bodyid;
            StationName = stationname; StationName_Localised = stationnameloc; FDStationType = fdstationtype; StationType = StationDefinitions.ToEnglish(fdstationtype);
            Name = name; Name_Localised = name_loc;
            Latitude = lat;
            Longitude = lon;
        }
        public string StationName { get; set; } // will be null if not docked, 
        public string StationName_Localised { get; set; } // will be null if not docked
        public string StationType { get; set; } // will be null if not docked, english name
        public double? DistFromStarLS { get; set; }
        public StationDefinitions.Classification MarketClass() { return MarketID != null ? StationDefinitions.Classify(MarketID.Value, FDStationType) : StationDefinitions.Classification.Unknown; }

        // 3.3.2 will be empty/null for previous logs.
        public FactionDefinitions.State StationFactionState { get; set; } // fdname
        public string StationFactionStateTranslated { get; set; }
        public GovernmentDefinitions.Government StationGovernment { get; set; }
        public string StationGovernment_Localised { get; set; }
        public AllegianceDefinitions.Allegiance StationAllegiance { get; set; }   // fdname
        public StationDefinitions.StationServices[] StationServices { get; set; }   // may be null
        public EconomyDefinitions.Economies[] StationEconomyList { get; set; }        // may be null

        public bool LocatedOutsideOrbitalStation => OnFoot != true && Docked == false && StationName.HasChars();

        //4.0 alpha 4
        public bool? Taxi { get; set; }
        public bool? Multicrew { get; set; }
        public bool? InSRV { get; set; }
        public bool? OnFoot { get; set; }

        public override string SummaryName(ISystem sys)     // Location
        {
            if (Docked)
                return string.Format("At {0}".Tx(), StationName_Localised);
            else
            {
                string bodyname = Body.HasChars() ? Body.ReplaceIfStartsWith(StarSystem) : StarSystem;
                if ( OnFoot == true )
                    return string.Format("On Foot at {0}".Tx(), bodyname);
                else if (Latitude.HasValue && Longitude.HasValue)
                    return string.Format("Landed on {0}".Tx(), bodyname);
                else
                    return string.Format("At {0}".Tx(), bodyname);
            }

        }

        public override string GetInfo()        // Location
        {
            if (Docked)
            {
                return BaseUtils.FieldBuilder.Build("Type ".Tx(), StationDefinitions.ToLocalisedLanguage(FDStationType),
                            "< in system ".Tx(), StarSystem);
            }
            else if (Latitude.HasValue && Longitude.HasValue)
            {
                return BaseUtils.FieldBuilder.Build("", Body, "< @ " + "Latitude: ;°;F4".Tx(), Latitude, "Longitude: ;°;F4".Tx(), Longitude);
            }
            else
            {
                return "Near".Tx()+": "+ " " + BodyType + " " + Body;     // remove JournalLocOrJump_Inspacenear
            }
        }

        public override string GetDetailed() // Location
        {
            StringBuilder sb = new StringBuilder();

            if (Docked)
            {
                sb.Build("<;(Wanted) ".Tx(), Wanted,
                        "Faction".Tx()+": ", StationFaction,
                        "State".Tx()+": ", StationFactionStateTranslated,
                        "Allegiance".Tx()+": ", AllegianceDefinitions.ToLocalisedLanguage(StationAllegiance),
                        "Economy".Tx()+": ", EconomyDefinitions.ToLocalisedLanguage(Economy),
                        "Government".Tx()+": ", GovernmentDefinitions.ToLocalisedLanguage(Government),
                        "Security".Tx()+": ", SecurityDefinitions.ToLocalisedLanguage(Security));
            }

            if (HasPowerPlayInfo)
            {
                sb.AppendCR();
                FillPowerInfo(sb);
            }

            if (HasFactionConflictThargoidInfo)
            {
                sb.AppendCR();
                FillFactionConflictThargoidPowerPlayConflictInfo(sb);
            }

            return sb.Length>0 ? sb.ToString() : null;
        }

        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            var sys = new SystemClass(StarSystem, SystemAddress, StarPos.X, StarPos.Y, StarPos.Z);
            s.GetOrAddSystem( sys);     // we use our data to fill in 
            s.AddLocation(this, sys);
        }

    }
}
