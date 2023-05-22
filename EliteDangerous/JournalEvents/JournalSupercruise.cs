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
using QuickJSON;
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

        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = StarSystem;
            detailed = "";
        }

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.SupercruiseEntry(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.SupercruiseExit)]
    public class JournalSupercruiseExit : JournalEntry, IBodyNameAndID
    {
        public JournalSupercruiseExit(JObject evt) : base(evt, JournalTypeEnum.SupercruiseExit)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = JournalFieldNaming.NormaliseBodyType(evt["BodyType"].Str());
            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get; set; }
        public string BodyDesignation { get; set; }

        public bool? Taxi { get; set; }             //4.0 alpha 4
        public bool? Multicrew { get; set; }
        public JournalSupercruiseDestinationDrop DestinationDrop { get; set; }       // update 15 associated destination drop. 

        public override void FillInformation(out string info, out string detailed)
        {
            if ( DestinationDrop != null )                                          // this gets set during history merge
            {
                DestinationDrop.FillInformation(out info, out string d);
                info += ", ";
            }
            else
                info = "At ".T(EDCTx.JournalSupercruiseExit_At);

            info += BaseUtils.FieldBuilder.Build("",Body, "< in ".T(EDCTx.JournalSupercruiseExit_in), StarSystem, "Type: ".T(EDCTx.JournalEntry_Type), BodyType);
            detailed = "";
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

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("At ".T(EDCTx.JournalSupercruiseExit_At), Location_Localised.Alt(Location), "Threat Level: ".T(EDCTx.FSSSignal_ThreatLevel), Threat);
            detailed = "";
        }
    }

}
