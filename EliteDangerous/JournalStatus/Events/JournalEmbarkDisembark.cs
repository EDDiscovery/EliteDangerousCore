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
using static System.Collections.Specialized.BitVector32;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Embark)]
    public class JournalEmbark : JournalEntry
    {
        public JournalEmbark(JObject evt) : base(evt, JournalTypeEnum.Embark)
        {
            SRV = evt["SRV"].Bool();
            Taxi = evt["Taxi"].Bool();
            Multicrew = evt["Multicrew"].Bool();
            ShipID = evt["ID"].ULongNull();
            StarSystem = evt["StarSystem"].StrNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].LongNull();
            OnPlanet = evt["OnPlanet"].BoolNull();
            OnStation = evt["OnStation"].BoolNull();
            var snl = JournalFieldNaming.GetStationNames(evt);
            StationName = snl.Item1;
            StationName_Localised = snl.Item2;
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());        // may not be embarking from a station, accept 
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public bool SRV { get; set; }       // 4.0 alpha 1
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public bool Ship { get { return !SRV && !Taxi && !Multicrew; } }

        public ulong? ShipID { get; set; }

        public string StarSystem { get; set; }      //4.0 alpha 4 on
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public long? BodyID { get; set; }
        public bool? OnStation { get; set; }
        public bool? OnPlanet { get; set; }
        public string StationName { get; set; }
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // friendly, may be null
        public StationDefinitions.StarportTypes FDStationType { get; set; }   // may be null
        public long? MarketID { get; set; }

        protected override JournalTypeEnum IconEventType { get { return SRV ? JournalTypeEnum.EmbarkSRV : JournalTypeEnum.Embark; } }

        public override string GetInfo()
        {
            string info = "";
            if (Taxi)
                info = "Taxi".Tx();
            else if (SRV)
                info = "SRV".Tx();
            else if (Multicrew)
                info = "Multicrew".Tx();
            else
                info = "Ship".Tx();

            string body = Body == StationName ? null : Body.ReplaceIfStartsWith(StarSystem);

            info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("", StationName_Localised, "", body, "", StarSystem), ". ");
            if (FDStationType != StationDefinitions.StarportTypes.Unknown)
                info += ", " + "Type: ".Tx()+ StationDefinitions.ToLocalisedLanguage(FDStationType);

            return info;
        }
    }

    [JournalEntryType(JournalTypeEnum.Disembark)]
    public class JournalDisembark : JournalEntry
    {
        public JournalDisembark(JObject evt) : base(evt, JournalTypeEnum.Disembark)
        {
            SRV = evt["SRV"].Bool();
            Taxi = evt["Taxi"].Bool();
            Multicrew = evt["Multicrew"].Bool();
            ShipID = evt["ID"].ULongNull();
            StarSystem = evt["StarSystem"].StrNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].LongNull();
            OnPlanet = evt["OnPlanet"].BoolNull();
            OnStation = evt["OnStation"].BoolNull();
            var snl = JournalFieldNaming.GetStationNames(evt);
            StationName = snl.Item1;
            StationName_Localised = snl.Item2;
            HasStationTypeName = StationName.HasChars() && evt["StationType"] != null;
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());        // null means its not present, will be set to Unknown
            StationType = StationDefinitions.ToEnglish(FDStationType);
            MarketID = evt["MarketID"].LongNull();
        }

        public bool SRV { get; set; }       // 4.0 alpha 1
        public bool Taxi { get; set; }
        public bool Multicrew { get; set; }
        public bool Ship { get { return !SRV && !Taxi && !Multicrew; } }

        public ulong? ShipID { get; set; }

        public string StarSystem { get; set; }      //4.0 alpha 4 on
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public long? BodyID { get; set; }
        public bool? OnStation { get; set; }            // note bugs means this is unreliable
        public bool? OnPlanet { get; set; }
        public bool HasStationTypeName { get; set; }        // note bugs means that stationname/type does not seem to appear always even if you jump out to a station
        public string StationName { get; set; } // may be null, but that does not mean we may not be in a station
        public string StationName_Localised { get; set; } 
        public string StationType { get; set; } // english friendly, may be Unknown if not there
        public StationDefinitions.StarportTypes FDStationType { get; set; }   // will be Unknown for no station given
        public long? MarketID { get; set; }

        protected override JournalTypeEnum IconEventType { get { return SRV ? JournalTypeEnum.DisembarkSRV : JournalTypeEnum.Disembark; } }

        public override string GetInfo()
        {
            string info = "";
            if (Taxi)
                info = "Taxi".Tx();
            else if (SRV)
                info = "SRV".Tx();
            else if (Multicrew)
                info = "Multicrew".Tx();
            else
                info = "Ship".Tx();

            string body = Body == StationName ? null : Body.ReplaceIfStartsWith(StarSystem);
            info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("", StationName_Localised, "", body, "", StarSystem), ". ");
            if (FDStationType != StationDefinitions.StarportTypes.Unknown)
                info += ", " + "Type: ".Tx()+ StationDefinitions.ToLocalisedLanguage(FDStationType);
            return info;
        }
    }
}


