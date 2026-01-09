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
using EliteDangerousCore.DB;
using EliteDangerousCore.StarScan2;
using QuickJSON;
using System;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.SupercruiseEntry)]
    public class JournalSupercruiseEntry : JournalEntry, IShipInformation
    {
        public JournalSupercruiseEntry(JObject evt ) : base(evt, JournalTypeEnum.SupercruiseEntry)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
            Wanted = evt["Wanted"].BoolNull();

        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }

        public bool? Wanted { get; set; }   //seen in patch 17, but might be older
        public override string GetInfo()
        {
            string info = StarSystem;
            if (Wanted == true)
                info += ", You are wanted.".Tx();
            return info;
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.SupercruiseEntry(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.SupercruiseExit)]
    public class JournalSupercruiseExit : JournalEntry, IBodyFeature, IStarScan
    {
        public JournalSupercruiseExit(JObject evt) : base(evt, JournalTypeEnum.SupercruiseExit)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = BodyDefinitions.GetBodyType(evt["BodyType"].Str());
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
        }

        public JournalSupercruiseExit(DateTime utc, string starSystem, long? systemAddress, string body, BodyDefinitions.BodyType bodyType, int? bodyID,
                            string name, string nameloc, StationDefinitions.StarportTypes fd,
                            bool? taxi, bool? multicrew
            ) : base(utc,JournalTypeEnum.SupercruiseExit,0)
        {
            StarSystem = starSystem;
            Body = body;
            SystemAddress = systemAddress;
            BodyID = bodyID;
            BodyType = bodyType;
            Taxi = taxi;
            Multicrew = multicrew;
            Name = name;
            Name_Localised = nameloc;
            FDStationType = fd;
        }

        public string StarSystem { get; set; }          // always there
        public long? SystemAddress { get; set; }        // 2018 on, augmented below with AddStarScan
        public string Body { get; set; }                // always there
        public int? BodyID { get; set; }                // 2018 on
        public BodyDefinitions.BodyType BodyType { get; set; }      // late 2016
        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }
        public JournalSupercruiseDestinationDrop DestinationDrop { get; set; }       // update 15 associated destination drop. 


        // IBodyNameAndID
        public string BodyName => Body;
        int? IBodyFeature.BodyID => BodyID;
        public double? Latitude { get => null; set { } }
        public double? Longitude { get => null; set { } }
        public bool HasLatLong => false;
        public string Name { get; set; } = null;
        public string Name_Localised { get; set; } = null;
        public long? MarketID => null;
        public StationDefinitions.StarportTypes FDStationType { get; set; } = StationDefinitions.StarportTypes.Unknown;
        public string StationFaction => null;

        public void AddStarScan(StarScan s, ISystem system)
        {
            if (SystemAddress == null)
                SystemAddress = system.SystemAddress;
            s.AddLocation(this, system);
        }

        public override string GetInfo()
        {
            string info = "";
            if ( DestinationDrop != null )                                          // this gets set during history merge
            {
                info += DestinationDrop.GetInfo() + ", ";
            }
            else
                info = "At ".Tx();

            info += BaseUtils.FieldBuilder.Build("",Body, "< in ".Tx(), StarSystem, "Type".Tx()+": ", BodyType);
            return info;
        }


    }

    [JournalEntryType(JournalTypeEnum.SupercruiseDestinationDrop)]
    public class JournalSupercruiseDestinationDrop : JournalEntry
    {
        public JournalSupercruiseDestinationDrop(JObject evt) : base(evt, JournalTypeEnum.SupercruiseDestinationDrop)
        {
            Location = evt["Type"].Str();
            Location_Localised = evt["Type_Localised"].StrNull();
            Threat = evt["Threat"].Int();
            MarketID = evt["MarketID"].Long();
        }

        public string Location { get; set; }
        public string Location_Localised { get; set; }      // may be null if not present
        public int Threat { get; set; }
        public long MarketID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("At ".Tx(), Location_Localised.Alt(Location), "Threat Level".Tx()+": ", Threat);
        }
    }

}
