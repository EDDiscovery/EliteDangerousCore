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
using QuickJSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.DockSRV)]
    public class JournalDockSRV : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }
        public string SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null

        public JournalDockSRV(JObject evt ) : base(evt, JournalTypeEnum.DockSRV)
        {
            ID = evt["ID"].IntNull();
            SRVType = evt["SRVType"].StrNull();
            SRVType_Localised = evt["SRVType_Localised"].StrNull();
        }

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.DockSRV();
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)  
        {
            info = BaseUtils.FieldBuilder.Build("", SRVType_Localised);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.LaunchSRV)]
    public class JournalLaunchSRV : JournalEntry, IShipInformation
    {
        public JournalLaunchSRV(JObject evt) : base(evt, JournalTypeEnum.LaunchSRV)
        {
            Loadout = evt["Loadout"].Str();
            PlayerControlled = evt["PlayerControlled"].Bool(true);
            ID = evt["ID"].IntNull();
            SRVType = evt["SRVType"].StrNull();
            SRVType_Localised = evt["SRVType_Localised"].StrNull();
        }
        public string Loadout { get; set; }
        public bool PlayerControlled { get; set; }
        public int? ID { get; set; }

        public string SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.LaunchSRV();
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SRVType_Localised, "Loadout: ".T(EDCTx.JournalEntry_Loadout), Loadout) + 
                        BaseUtils.FieldBuilder.Build(", " + "NPC Controlled;".T(EDCTx.JournalEntry_NPCControlled), PlayerControlled);
            detailed = "";
        }
    }


    [JournalEntryType(JournalTypeEnum.SRVDestroyed)]
    public class JournalSRVDestroyed : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }

        public string SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null

        public JournalSRVDestroyed(JObject evt) : base(evt, JournalTypeEnum.SRVDestroyed)
        {
            ID = evt["ID"].IntNull();
            SRVType = evt["SRVType"].StrNull();
            SRVType_Localised = evt["SRVType_Localised"].StrNull();
        }

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.DestroyedSRV();
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SRVType_Localised);
            detailed = "";
        }

    }


}
