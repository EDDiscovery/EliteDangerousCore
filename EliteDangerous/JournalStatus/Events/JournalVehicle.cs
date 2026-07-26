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

    // Nomad or Fighter
    [JournalEntryType(JournalTypeEnum.LaunchFighter)]
    public class JournalLaunchFighter : JournalEntry, IShipInformation
    {
        public JournalLaunchFighter(JObject evt) : base(evt, JournalTypeEnum.LaunchFighter)
        {
            Loadout = evt["Loadout"].Str();
            PlayerControlled = evt["PlayerControlled"].Bool();
            ID = evt["ID"].IntNull();
        }
        public string Loadout { get; set; }
        public bool PlayerControlled { get; set; }
        public int? ID { get; set; }
        public bool IsLander => (Loadout.ContainsIIC("base") || Loadout.ContainsIIC("advanced") || Loadout.ContainsIIC("galactic")) && PlayerControlled == true;

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if (IsLander)
                shp.LaunchLander();
            else
                shp.LaunchFighter(PlayerControlled);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Loadout".Tx() + ": ", Loadout, "NPC Controlled;".Tx(), PlayerControlled);
        }

        public override string SummaryName(ISystem sys)
        {
            return IsLander ? "Launch Lander" : base.EventFilterName;
        }
    }

    // Fighter
    [JournalEntryType(JournalTypeEnum.DockFighter)]
    public class JournalDockFighter : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }

        public JournalDockFighter(JObject evt) : base(evt, JournalTypeEnum.DockFighter)
        {
            ID = evt["ID"].IntNull();
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.DockFighter();
        }
    }

    // Fighter
    [JournalEntryType(JournalTypeEnum.FighterDestroyed)]
    public class JournalFighterDestroyed : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }

        public JournalFighterDestroyed(JObject evt) : base(evt, JournalTypeEnum.FighterDestroyed)
        {
            ID = evt["ID"].IntNull();
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.FighterDestroyed();
        }

    }

    // Fighter?
    [JournalEntryType(JournalTypeEnum.FighterRebuilt)]
    public class JournalFighterRebuilt : JournalEntry
    {
        public JournalFighterRebuilt(JObject evt) : base(evt, JournalTypeEnum.FighterRebuilt)
        {
            Loadout = evt["Loadout"].Str();
            ID = evt["ID"].IntNull();
        }

        public string Loadout { get; set; }
        public int? ID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Loadout);
        }
    }


    // SRV or Nomad
    [JournalEntryType(JournalTypeEnum.RestockVehicle)]
    public class JournalRestockVehicle : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalRestockVehicle(JObject evt) : base(evt, JournalTypeEnum.RestockVehicle)
        {
            TypeFD = VehicleFDName.Normalise(evt["Type"].Str(), out string shipname, this);
            Type = shipname;
            Type_Localised = JournalFieldNaming.CheckLocalisation(evt["Type_Localised"].Str(), Type);
            Loadout = evt["Loadout"].Str();
            Cost = evt["Cost"].Long();
            Count = evt["Count"].Int();
            ID = new ShipID(evt["ID"]);
        }

        public VehicleFDName TypeFD { get; set; }
        public string Type { get; set; }                    // better name
        public string Type_Localised { get; set; }          // new June 26, evidence of it from 2021!
        public string Loadout { get; set; }
        public long Cost { get; set; }
        public int Count { get; set; }
        public ShipID ID { get; set; }                        // since 2023 ish.
        public bool IsLander => TypeFD.VehicleType == VehicleFDName.VehicleTypeEnum.Lander;

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Type_Localised + " " + Count.ToString(), -Cost);
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if (ID.IsValid)
            {
                shp.RestockVehicle(ID, TypeFD, Type_Localised, Loadout);
            }
        }

        protected override JournalTypeEnum IconEventType { get { return TypeFD.VehicleType == VehicleFDName.VehicleTypeEnum.SRV ? JournalTypeEnum.RestockVehicle_SRV : JournalTypeEnum.RestockVehicle_Fighter; } }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Type_Localised, "Cost: ; cr;N0".Tx(), Cost, "Count".Tx() + ": ", Count, "Loadout".Tx() + ": ", Loadout);
        }
    }

    // Only Fighter
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

    // Only SRV
    [JournalEntryType(JournalTypeEnum.LaunchSRV)]
    public class JournalLaunchSRV : JournalEntry, IShipInformation
    {
        public JournalLaunchSRV(JObject evt) : base(evt, JournalTypeEnum.LaunchSRV)
        {
            Loadout = evt["Loadout"].Str();
            PlayerControlled = evt["PlayerControlled"].Bool(true);
            ID = evt["ID"].IntNull();
            SRVType = evt["SRVType"].FDName();      // should not need ship norm
            SRVType_Localised = evt["SRVType_Localised"].StrNull();
        }
        public string Loadout { get; set; }
        public bool PlayerControlled { get; set; }
        public int? ID { get; set; }

        public FDName SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.LaunchSRV();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SRVType_Localised, "Loadout".Tx() + ": ", Loadout) +
                        BaseUtils.FieldBuilder.Build(", " + "NPC Controlled;".Tx(), PlayerControlled);
        }
    }

    // SRV or Nomad
    [JournalEntryType(JournalTypeEnum.DockSRV)]
    public class JournalDockSRV : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }
        public VehicleFDName SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null
        public bool IsLander => SRVType.VehicleType == VehicleFDName.VehicleTypeEnum.Lander;

        public JournalDockSRV(JObject evt) : base(evt, JournalTypeEnum.DockSRV)
        {
            ID = evt["ID"].IntNull();
            SRVType = VehicleFDName.Normalise(evt["SRVType"].Str(), out string engname, this);
            SRVType_Localised = JournalFieldNaming.CheckLocalisation(evt["SRVType_Localised"].StrNull(), engname);
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if (IsLander)
                shp.DockLander();
            else
                shp.DockSRV();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SRVType_Localised);
        }
        public override string SummaryName(ISystem sys)
        {
            return IsLander ? "Dock Lander" : base.EventFilterName;
        }
    }

    // SRV or Nomad

    [JournalEntryType(JournalTypeEnum.SRVDestroyed)]
    public class JournalSRVDestroyed : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }

        public VehicleFDName SRVType;          // new odyssey 9, dec 21, may be null
        public string SRVType_Localised; // new odyssey 9, dec 21, may be null
        public bool IsLander => SRVType.VehicleType == VehicleFDName.VehicleTypeEnum.Lander;

        public JournalSRVDestroyed(JObject evt) : base(evt, JournalTypeEnum.SRVDestroyed)
        {
            ID = evt["ID"].IntNull();
            SRVType = VehicleFDName.Normalise(evt["SRVType"].Str(), out string engname, this);
            SRVType_Localised = JournalFieldNaming.CheckLocalisation(evt["SRVType_Localised"].StrNull(), engname);
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if (IsLander)
                shp.DestroyedLander();
            else
                shp.DestroyedSRV();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SRVType_Localised);
        }

        public override string SummaryName(ISystem sys)
        {
            if (IsLander)
                return "Destroyed Lander";
            else
                return base.EventFilterName;
        }

    }



}
