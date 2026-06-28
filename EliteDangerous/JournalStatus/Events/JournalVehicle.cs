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
    [JournalEntryType(JournalTypeEnum.RestockVehicle)]
    public class JournalRestockVehicle : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        // Fighter or Nomad

        public JournalRestockVehicle(JObject evt ) : base(evt, JournalTypeEnum.RestockVehicle)
        {
            TypeFD = evt["Type"].Str();
            Type = JournalFieldNaming.GetBetterShipName(TypeFD);
            Type_Localised = JournalFieldNaming.CheckLocalisation(evt["Type_Localised"].Str(), Type);
            Loadout = evt["Loadout"].Str();
            Cost = evt["Cost"].Long();
            Count = evt["Count"].Int();
            ID = evt["ID"].ULongNull();
        }

        public string TypeFD { get; set; }
        public string Type { get; set; }                    // better name
        public string Type_Localised { get; set; }          // new June 26, evidence of it from 2021!
        public string Loadout { get; set; }
        public long Cost { get; set; }
        public int Count { get; set; }

        public ulong? ID { get; set; }                        // since 2023 ish.

        // beta for nomad june 26 uses this
        public bool IsLander => TypeFD.ContainsIIC("Lander");

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Type_Localised + " " + Count.ToString(), -Cost);
        }
        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if (ID.HasValue)
            {
                shp.RestockVehicle(ID.Value, TypeFD, Type_Localised, Loadout);
            }
        }
        protected override JournalTypeEnum IconEventType { get { return ItemData.IsSRV(Type) ? JournalTypeEnum.RestockVehicle_SRV : JournalTypeEnum.RestockVehicle_Fighter; } }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("",Type_Localised , "Cost: ; cr;N0".Tx(), Cost , "Count".Tx()+": ", Count , "Loadout".Tx()+": ", Loadout);
        }
    }


    [JournalEntryType(JournalTypeEnum.VehicleSwitch)]
    public class JournalVehicleSwitch : JournalEntry, IShipInformation
    {
        public JournalVehicleSwitch(JObject evt) : base(evt, JournalTypeEnum.VehicleSwitch)
        {
            To = evt["To"].Str();
            if (To.Length == 0)             // Frontier BUG, sometimes To is missing
                To = "Mothership";
        }
        public string To { get; set; }

        protected override JournalTypeEnum IconEventType { get { return To.Contains("Mothership") ? JournalTypeEnum.VehicleSwitch_Mothership : JournalTypeEnum.VehicleSwitch_Fighter; } }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.VehicleSwitch(To);
        }

        public override string GetInfo()
        {
            return To;
        }
    }

}
