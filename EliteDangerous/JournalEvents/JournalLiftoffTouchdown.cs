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
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Liftoff)]
    public class JournalLiftoff : JournalEntry
    {
        public JournalLiftoff(JObject evt ) : base(evt, JournalTypeEnum.Liftoff)
        {
            Latitude = evt["Latitude"].Double();
            Longitude = evt["Longitude"].Double();
            PlayerControlled = evt["PlayerControlled"].BoolNull();
            NearestDestination = evt["NearestDestination"].StrNull();
            NearestDestination_Localised = JournalFieldNaming.CheckLocalisation(evt["NearestDestination_Localised"].StrNull(), NearestDestination);
            StarSystem = evt["StarSystem"].StrNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].LongNull();
            OnPlanet = evt["OnPlanet"].BoolNull();
            OnStation = evt["OnStation"].BoolNull();
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool? PlayerControlled { get; set; }
        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }

        public string StarSystem { get; set; }      //4.0 alpha 4 on
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public long? BodyID { get; set; }
        public bool? OnStation { get; set; }
        public bool? OnPlanet { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed) 
        {
            // TBD enhance
            info = JournalFieldNaming.RLat(Latitude) + " " + JournalFieldNaming.RLong(Longitude) + BaseUtils.FieldBuilder.Build(", NPC Controlled;".T(EDTx.JournalEntry_NPCControlled), PlayerControlled, ", " + "Nearest:".T(EDTx.JournalEntry_Nearest), NearestDestination_Localised);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.Touchdown)]
    public class JournalTouchdown : JournalEntry
    {
        public JournalTouchdown(JObject evt) : base(evt, JournalTypeEnum.Touchdown)
        {
            Latitude = evt["Latitude"].Double();
            Longitude = evt["Longitude"].Double();
            PlayerControlled = evt["PlayerControlled"].BoolNull();
            NearestDestination = evt["NearestDestination"].StrNull();
            NearestDestination_Localised = JournalFieldNaming.CheckLocalisation(evt["NearestDestination_Localised"].StrNull(), NearestDestination);
            StarSystem = evt["StarSystem"].StrNull();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].LongNull();
            OnPlanet = evt["OnPlanet"].BoolNull();
            OnStation = evt["OnStation"].BoolNull();
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool? PlayerControlled { get; set; }
        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }

        public string StarSystem { get; set; }      //4.0 alpha 4 on
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public long? BodyID { get; set; }
        public bool? OnStation { get; set; }
        public bool? OnPlanet { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            //TBD enhance
            info = JournalFieldNaming.RLat(Latitude) + " " + JournalFieldNaming.RLong(Longitude) + BaseUtils.FieldBuilder.Build(", NPC Controlled;".T(EDTx.JournalEntry_NPCControlled), PlayerControlled, ", " + "Nearest:".T(EDTx.JournalEntry_Nearest), NearestDestination_Localised);
            detailed = "";
        }
    }

}
