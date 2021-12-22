/*
 * Copyright © 2016-2018 EDDiscovery development team
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using BaseUtils.JSON;
using System;
using System.Collections.Generic;
using System.Linq;

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
            StationName = evt["StationName"].StrNull();
            StationType = evt["StationType"].StrNull();
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
        public string StationType { get; set; }
        public long? MarketID { get; set; }

        protected override JournalTypeEnum IconEventType { get { return SRV ? JournalTypeEnum.EmbarkSRV : JournalTypeEnum.Embark; } }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            if (Taxi)
                info = "Taxi".T(EDTx.JournalEntry_Taxi);
            else if (SRV)
                info = "SRV".T(EDTx.JournalCargo_CargoSRV);
            else if (Multicrew)
                info = "Multicrew".T(EDTx.JournalStatistics_Multicrew);
            else
                info = "Ship".T(EDTx.JournalCargo_CargoShip);

            string body = Body == StationName ? null : Body.ReplaceIfStartsWith(StarSystem);

            info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("", StationName, "", body, "", StarSystem), ". ");
            detailed = "";
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
            StationName = evt["StationName"].StrNull();
            StationType = evt["StationType"].StrNull();
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
        public string StationType { get; set; }
        public long? MarketID { get; set; }

        protected override JournalTypeEnum IconEventType { get { return SRV ? JournalTypeEnum.DisembarkSRV : JournalTypeEnum.Disembark; } }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            if (Taxi)
                info = "Taxi".T(EDTx.JournalEntry_Taxi);
            else if (SRV)
                info = "SRV".T(EDTx.JournalCargo_CargoSRV);
            else if (Multicrew)
                info = "Multicrew".T(EDTx.JournalStatistics_Multicrew);
            else
                info = "Ship".T(EDTx.JournalCargo_CargoShip);

            string body = Body == StationName ? null : Body.ReplaceIfStartsWith(StarSystem);
            info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("", StationName, "", body, "", StarSystem), ". ");
            detailed = "";
        }
    }
}


