/*
 * Copyright © 2016-2024 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EliteDangerousCore.JournalEvents;
using QuickJSON;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}:{ShipType}:{ShipFD}:{Modules.Count}")]
    public class Ship
    {
        #region Information interface

        public ulong ID { get; private set; }                 // its Frontier ID.     ID's are moved to high range when sold
        public enum ShipState { Owned, Sold, Destroyed, Imported};
        public ShipState State { get; set; } = ShipState.Owned; // if owned, sold, destroyed. Default owned
        public string ShipType { get; private set; }        // ship type name, nice, fer-de-lance, etc. can be null
        public string ShipFD { get; private set; }          // ship type name, fdname
        public string ShipUserName { get; private set; }    // ship name, may be empty or null
        public string ShipUserIdent { get; private set; }   // ship ident, may be empty or null
        public long HullValue { get; private set; }         // may be 0, not known
        public long ModulesValue { get; private set; }      // may be 0, not known
        public double HullHealthAtLoadout { get; private set; } // may be 0, in range 0-100.
        public double UnladenMass { get; private set; }     // may be 0, not known, from loadout
        public double FuelLevel { get; private set; }       // fuel level may be 0 not known, from UI event fuel or from other events which give fuel level (scoop etc)
        public double FuelCapacity { get; private set; }    // fuel capacity may be 0 not known. Calculated as previous loadouts did not include this. 3.4 does
        public double ReserveFuelCapacity { get; private set; }  // 3.4 from loadout.. you can also get this from Item ship data
        public double ReserveFuelLevel { get; private set; }  // from UI event Fuel. 
        public long Rebuy { get; private set; }             // may be 0, not known

        // for this ship, the ship properites. May be null
        public ItemData.ShipProperties GetShipProperties()  { return ItemData.GetShipProperties(ShipFD); }


        // Modules

        public Dictionary<ShipSlots.Slot, ShipModule> Modules { get; private set; }     // slot to ship module installed

        // null if nothing in slot
        public ShipModule GetModuleInSlot(ShipSlots.Slot slot) { return Modules.ContainsKey(slot) ? Modules[slot] : null; }      

        // for this ship module the unengineered properties, may be null
        public ItemData.ShipModule GetShipModulePropertiesUnengineered(ShipSlots.Slot slot)
        {
            if (Modules.TryGetValue(slot, out ShipModule module))
                return module.GetModuleUnengineered();
            else
                return null;
        }

        // for this ship module the engineered properties, may be null.  If engineering fails, you still get the properties.
        public ItemData.ShipModule GetShipModulePropertiesEngineered(ShipSlots.Slot slot, bool debugit = false)
        {
            if (Modules.TryGetValue(slot, out ShipModule module))
            {
                return module.GetModuleEngineered(out string _, debugit);
            }
            else
            {
                return null;
            }

        }
        // for this ship module the engineered properties, may be null.  If engineering fails, you still get the properties.
        public ItemData.ShipModule GetShipModulePropertiesEngineered(ShipSlots.Slot slot, out string report, bool debugit = false)
        {
            report = "";
            if (Modules.TryGetValue(slot, out ShipModule module))
            {
                return module.GetModuleEngineered(out report, debugit);
            }
            else
            {
                return null;
            }

        }

        // slots with modules matching this test, return list of slots where they are in
        public ShipSlots.Slot[] FindShipModules(Predicate<ShipModule> test)
        {
            return Modules.Where(kvp => test(kvp.Value)).Select(x=>x.Key).ToArray();
        }

        public string StoredAtSystem { get; private set; }  // null if not stored, else where stored
        public string StoredAtStation { get; private set; } // null if not stored or unknown
        public DateTime TransferArrivalTimeUTC { get; private set; }     // if current UTC < this, its in transit
        public bool Hot { get; private set; }               // if known to be hot.

        public enum SubVehicleType
        {
            None, SRV, Fighter
        }

        public SubVehicleType SubVehicle { get; private set; } = SubVehicleType.None;    // if in a sub vehicle or mothership


        public bool InTransit { get { return TransferArrivalTimeUTC.CompareTo(DateTime.UtcNow)>0; } }


        public string ShipFullInfo(bool cargo = true, bool fuel = true, bool manu = false)
        {
            StringBuilder sb = new StringBuilder(64);
            if (ShipUserIdent != null)
                sb.Append(ShipUserIdent);
            sb.AppendPrePad(ShipUserName);
            if (manu && GetShipProperties()?.Manufacturer != null)
                sb.AppendPrePad(GetShipProperties()?.Manufacturer??"Unknown ship", ", ");
            sb.AppendPrePad(ShipType);
            sb.AppendPrePad("(" + ID.ToString() + ")");

            if (SubVehicle == SubVehicleType.SRV)
                sb.AppendPrePad(" in SRV");
            else if (SubVehicle == SubVehicleType.Fighter)
                sb.AppendPrePad(" in Fighter");
            else
            {
                if (State != ShipState.Owned)
                    sb.Append(" (" + State.ToString() + ")");

                if (InTransit)
                    sb.Append(" (Tx to " + StoredAtSystem + ")");
                else if (StoredAtSystem != null)
                    sb.Append(" (@" + StoredAtSystem + ")");

                if (fuel)
                {
                    double cap = FuelCapacity;
                    if (cap > 0)
                        sb.Append(" Fuel Cap " + cap.ToString("0.#"));
                }

                if (cargo)
                {
                    double cap = CalculateCargoCapacity();
                    if (cap > 0)
                        sb.Append(" Cargo Cap " + cap);
                }
            }

            return sb.ToString();
        }

        public string Name          // Name of ship, either user named or ship type
        {
            get                  // unique ID
            {
                if (ShipUserName != null && ShipUserName.Length > 0)
                    return ShipUserName;
                else
                    return ShipType;
            }
        }

        public string ShipShortName
        {
            get                  // unique ID
            {
                StringBuilder sb = new StringBuilder(64);
                if (ShipUserName != null && ShipUserName.Length > 0)
                {
                    sb.AppendPrePad(ShipUserName);
                }
                else
                {
                    sb.AppendPrePad(ShipType);
                    sb.AppendPrePad("(" + ID.ToString() + ")");
                }
                return sb.ToString();
            }
        }

        public string ShipNameIdentType
        {
            get                  // unique ID
            {
                string res = string.IsNullOrEmpty(ShipUserName) ? "" : ShipUserName;
                res = res.AppendPrePad(string.IsNullOrEmpty(ShipUserIdent) ? "" : ShipUserIdent, ",");
                bool empty = string.IsNullOrEmpty(res);
                res = res.AppendPrePad(ShipType, ",");
                if (empty)
                    res += " (" + ID.ToString() + ")";

                if (State != ShipState.Owned)
                    res += " (" + State.ToString() + ")";

                if (InTransit)
                    res += " (Tx to " + StoredAtSystem + ")";
                else if (StoredAtSystem != null)
                    res += " (@" + StoredAtSystem + ")";

                return res;
            }
        }

        public int CalculateCargoCapacity()
        {
            int cap = 0;
            foreach (ShipModule sm in Modules.Values)
            {
                var me = sm.GetModuleEngineered(out string _);
                //System.Diagnostics.Debug.WriteLine($"Module {me.ModType}");
                // paranoia check on engineering - new computation based on new pather clipper July 25, added corrosion proof cargo racks
                if (me?.ModType == ItemData.ShipModule.ModuleTypes.CargoRack ||
                    me?.ModType == ItemData.ShipModule.ModuleTypes.CorrosionResistantCargoRack)
                {
                    //System.Diagnostics.Debug.WriteLine($"Cargo Module {me.Size}");
                    cap += me.Size ?? 0;
                }
            }
            
            return cap;
        }

        //// may be null due to not having the info
        public EliteDangerousCalculations.FSDSpec GetFSDSpec()
        {
            var module = GetShipModulePropertiesEngineered(ShipSlots.Slot.FrameShiftDrive);
            if (module?.PowerConstant != null)
            {
                var spec = new EliteDangerousCalculations.FSDSpec(module.PowerConstant.Value, module.LinearConstant.Value, module.OptMass.Value, module.MaxFuelPerJump.Value);
                var gmodules = FindShipModules(x => x.GetModuleUnengineered()?.ModType == ItemData.ShipModule.ModuleTypes.GuardianFSDBooster);
                if (gmodules.Length == 1)
                {
                    spec.FSDGuardianBoosterRange = GetShipModulePropertiesEngineered(gmodules[0]).AdditionalRange.Value;
                }

                return spec;
            }
            else
                return null;
        }

        // current jump range or null if can't calc
        // if no parameters, uses maximum cargo and maximum fuel
        public double? GetJumpRange(int? cargo = null, double? fuel = null)
        {
            var fsd = GetFSDSpec();
            if (fsd != null)
            {
                if (cargo == null)
                    cargo = CalculateCargoCapacity();
                if (fuel == null)
                    fuel = FuelCapacity;

                return fsd.JumpRange(cargo.Value, ModuleMass() + HullMass(), fuel.Value, 1.0);
            }
            else
                return null;
        }

        public double ModuleMass()
        {
            //foreach( var x in Modules)  System.Diagnostics.Debug.WriteLine($"Module {x.Value.Item} mass {x.Value.Mass}");
            return (from var in Modules select var.Value.Mass()).Sum();
        }

        public double HullMass()
        {
            var ship = GetShipProperties();
            return ship?.HullMass ?? 0;
        }

        public double HullModuleMass()      // based on modules and hull, not on FDev unladen mass in loadout
        {
            return ModuleMass() + HullMass();
        }

        public double FuelWarningPercent
        {
            get { return fuelwarningpercent; }
            set { fuelwarningpercent = value; EliteDangerousCore.DB.UserDatabase.Instance.PutSetting("ShipInformation:" + ShipFD + ID.ToStringInvariant() + "Warninglevel", value); }
        }
        public void UpdateFuelWarningPercent()
        {
            if ( fuelwarningpercent == -999 )
                fuelwarningpercent = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ShipInformation:" + ShipFD + ID.ToStringInvariant() + "Warninglevel", 0);
        }

        public bool HasWeapons()
        {
            var wmodules = FindShipModules(x => x.GetModuleUnengineered()?.IsHardpoint == true);
            return wmodules.Length > 0;
        }
        public bool HasMiningEquipment()
        {
            var wmodules = FindShipModules(x => x.GetModuleUnengineered()?.IsMiningEquipment == true);
            return wmodules.Length > 0;
        }

        private double fuelwarningpercent = -999;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.AppendFormat("Ship {0}", ShipFullInfo());
            sb.Append(Environment.NewLine);
            foreach (ShipModule sm in Modules.Values)
            {
                sb.AppendFormat(sm.ToString());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #region Stats
        public class Stats
        {
            public double? ArmourRaw { get; set; }      // will be null if can't compute
            public double ArmourKineticPercentage { get; set; }
            public double ArmourThermalPercentage { get; set; }
            public double ArmourExplosivePercentage { get; set; }
            public double ArmourCausticPercentage { get; set; }
            public double ArmourKineticValue { get; set; }
            public double ArmourThermalValue { get; set; }
            public double ArmourExplosiveValue { get; set; }
            public double ArmourCausticValue { get; set; }
            public double? ShieldsRaw { get; set; }      // will be null if can't compute
            public double ShieldsSystemPercentage { get; set; }
            public double ShieldsKineticPercentage { get; set; }
            public double ShieldsThermalPercentage { get; set; }
            public double ShieldsExplosivePercentage { get; set; }
            public double ShieldsSystemValue { get; set; }
            public double ShieldsKineticValue { get; set; }
            public double ShieldsThermalValue { get; set; }
            public double ShieldsExplosiveValue { get; set; }

            public double? ShieldBuildTime { get; set; }        // if null, can't find modules
            public double ShieldRegenTime { get; set; }
            
            public double? CurrentSpeed { get; set; }                  // if null ,can't find modules
            public double CurrentBoost { get; set; }
            public double LadenSpeed { get; set; }
            public double LadenBoost { get; set; }
            public double UnladenSpeed { get; set; }
            public double UnladenBoost { get; set; }
            public double MaxSpeed { get; set; }
            public double MaxBoost { get; set; }
            public double CurrentBoostFrequency { get; set; }
            public double MaxBoostFrequency { get; set; }

            public double? FSDCurrentRange { get; set; }        // if null, no fsd
            public double FSDCurrentMaxRange { get; set; }       
            public double FSDLadenRange { get; set; }      
            public double FSDUnladenRange { get; set; }
            public double FSDMaxRange { get; set; }
            public double FSDMaxFuelPerJump { get; set; }

            public bool ValidWeaponData { get { return WeaponRaw.HasValue && !double.IsNaN(WeaponAbsolutePercentage); } }
            public double? WeaponRaw { get; set; }              // if null, no pd
            public double WeaponAbsolutePercentage { get; set; }  
            public double WeaponThermalPercentage { get; set; }
            public double WeaponKineticPercentage { get; set; }
            public double WeaponExplosivePercentage { get; set; }
            public double WeaponAXPercentage { get; set; }
            public double WeaponDuration { get; set; }
            public double WeaponDurationMax { get; set; }
            public double WeaponAmmoDuration { get; set; }
            public double WeaponCurSus { get; set; }
            public double WeaponMaxSus { get; set; }
        }

        // derived (nicked) from EDSY thank you

        public Stats GetShipStats(int powerdist_sys, int powerdist_eng, int powerdist_wep, int currentcargo, double currentfuellevel, double currentreservelevel)
        {
            var res = new Stats();
            var ship = GetShipProperties();

            if (ship != null)
            {
                double hullrnf = 0;
                double hullbst = 0;
                double kinmod_ihrp = 1;
                double thmmod_ihrp = 1;
                double expmod_ihrp = 1;
                double caumod_ihrp = 1;
                double kinmin_ihrp = 1;
                double thmmin_ihrp = 1;
                double expmin_ihrp = 1;
                double caumin_ihrp = 1;
                double kinmod_usb = 1;
                double thmmod_usb = 1;
                double expmod_usb = 1;
                double caumod_usb = 1;
                double shieldbst = 0;
                double shieldrnf = 0;

                double dps = 0;
				double dps_abs= 0;
                double dps_thm = 0;
				double dps_kin= 0;
                double dps_exp = 0;
				double dps_axe= 0;
                double dps_cau = 0;
				double dps_nodistdraw= 0;
                double dps_distdraw = 0;
                double thmload_hardpoint_wepfull = 0;
                double thmload_hardpoint_wepempty = 0;
                double ammotime_wepcap = double.PositiveInfinity;
                double ammotime_nocap = double.PositiveInfinity;
                

                ItemData.ShipModule powerdistributorengineered = GetShipModulePropertiesEngineered(ShipSlots.Slot.PowerDistributor); // may be null
                double wepcap = powerdistributorengineered?.WeaponsCapacity ?? 10;      // set a minimum so does not except if PD not there

                List<Tuple<double, double, double>> weapons = new List<Tuple<double, double, double>>();

                foreach (var modkvp in Modules)
                {
                    var me = modkvp.Value.GetModuleEngineered(out string _);        // may be null if no loadout present, due to crap journal

                    if (me != null)
                    {
                        if ( me.HullReinforcement.HasValue)    // hull reinforcements , Guardian Hull, meta allow hulls
                        {
                            hullrnf += me.HullReinforcement.Value;
                            if (me.KineticResistance.HasValue) // hull reinforcements (ihrp = hull, guardian hull, imahrp = meta alloy)
                            {
                                double kinmod = (1 - me.KineticResistance.Value / 100.0);
                                kinmod_ihrp *= kinmod;
                                kinmin_ihrp = Math.Min(kinmin_ihrp, kinmod);
                            }
                            if (me.ThermalResistance.HasValue) // hull reinforcements, guardian hull
                            {
                                double thmmod = (1 - me.ThermalResistance.Value / 100.0);
                                thmmod_ihrp *= thmmod;
                                thmmin_ihrp = Math.Min(thmmin_ihrp, thmmod);
                            }
                            if (me.ExplosiveResistance.HasValue)  // hull reinforcements
                            {
                                double expmod = (1 - me.ExplosiveResistance.Value / 100.0);
                                expmod_ihrp *= expmod;
                                expmin_ihrp = Math.Min(expmin_ihrp, expmod);
                            }
                            if (me.CausticResistance.HasValue)  // meta allow hull , guardian hull
                            {
                                double caumod = (1 - me.CausticResistance.Value / 100.0);
                                caumod_ihrp *= caumod;
                                caumin_ihrp = Math.Min(caumin_ihrp, caumod);
                            }
                        }

                        if (me.HullStrengthBonus.HasValue)      // armour
                            hullbst += me.HullStrengthBonus.Value;

                        if (me.ModType == ItemData.ShipModule.ModuleTypes.ShieldBooster) //  shieldbst
                        {
                            shieldbst += me.ShieldReinforcement.Value;
                            kinmod_usb *= (1 - (me.KineticResistance.Value / 100));
                            thmmod_usb *= (1 - (me.ThermalResistance.Value / 100));
                            expmod_usb *= (1 - (me.ExplosiveResistance.Value / 100));
                            caumod_usb *= (1 - ((me.CausticResistance ?? 0) / 100));      // not present currently
                        }

                        if (me.ModType == ItemData.ShipModule.ModuleTypes.GuardianShieldReinforcement) // shieldrnf
                            shieldrnf += me.AdditionalReinforcement.Value;

                        if (me.IsHardpoint)
                        {
                            var thmload = me.getEffectiveAttrValue(nameof(ItemData.ShipModule.ThermalLoad), 1);      // should always be there
                            var distdraw = me.getEffectiveAttrValue(nameof(ItemData.ShipModule.DistributorDraw), 1);// should always be there
                            var ammoclip = me.getEffectiveAttrValue(nameof(ItemData.ShipModule.Clip), 0);       // if weapon does not have limits, is 0
                            var ammomax = me.getEffectiveAttrValue(nameof(ItemData.ShipModule.Ammo), 0);
                            var eps = me.getEffectiveAttrValue("eps");
                            var seps = me.getEffectiveAttrValue("seps");
                            var spc = me.getEffectiveAttrValue("spc", 1);
                            if (spc == 0)
                                spc = 1;
                            var sspc = me.getEffectiveAttrValue("sspc", 1);
                            if (sspc == 0)
                                sspc = 1;
                            var sfpc = me.getEffectiveAttrValue("sfpc");
                            var sdps = me.getEffectiveAttrValue("sdps");

                            System.Diagnostics.Debug.WriteLine($"{me.EnglishModName}: {thmload} {distdraw} {ammoclip} {ammomax} eps {eps} seps {seps} spc {spc} sspc {sspc} sfpc {sfpc} sdps {sdps}");

                            thmload *= sfpc / sspc;
                            thmload_hardpoint_wepfull += getEffectiveWeaponThermalLoad(thmload, distdraw, wepcap, 1.0);
                            thmload_hardpoint_wepempty += getEffectiveWeaponThermalLoad(thmload, distdraw, wepcap, 0.0);

                            dps += sdps;
                            var thm = me.getEffectiveAttrValue(nameof(ItemData.ShipModule.ThermalProportionDamage), 0);
                            dps_abs += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.AbsoluteProportionDamage), 0) / 100.0;
                            dps_thm += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.ThermalProportionDamage), 0) / 100.0;
                            dps_kin += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.KineticProportionDamage), 0) / 100.0;
                            dps_exp += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.ExplosiveProportionDamage), 0) / 100.0;
                            dps_axe += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.AXPorportionDamage), 0) / 100.0;
                            dps_cau += sdps * me.getEffectiveAttrValue(nameof(ItemData.ShipModule.CausticPorportionDamage), 0) / 100.0;

                            //System.Diagnostics.Debug.WriteLine($"{me.EnglishModName}: thmload {thmload} {thmload_hardpoint_wepfull} {thmload_hardpoint_wepempty}");
                            //System.Diagnostics.Debug.WriteLine($"{me.EnglishModName}: dps {dps} {dps_abs} {dps_thm}");

                            weapons.Add(new Tuple<double, double, double>(spc, eps, seps));

                            var ammotime = ammoclip != 0 ? (sspc * ((ammoclip + ammomax) / sfpc)) : double.PositiveInfinity;
                            if (distdraw != 0)
                            {
                                dps_distdraw += sdps;
                                ammotime_wepcap = Math.Min(ammotime_wepcap, ammotime);
                            }
                            else
                            {
                                dps_nodistdraw += sdps;
                                ammotime_nocap = Math.Min(ammotime_nocap, ammotime);
                            }
                        }

                        //System.Diagnostics.Debug.WriteLine($"{me.EnglishModName}: nodistdraw {dps_nodistdraw} {dps_distdraw} {ammotime_wepcap} {ammotime_nocap}");
                    }
                }

                var armourmoduleengineered = GetShipModulePropertiesEngineered(ShipSlots.Slot.Armour);
                if (armourmoduleengineered != null)
                {
                    var armour = ship.Armour;
                    var kinres = armourmoduleengineered.KineticResistance ?? 0;
                    var thmres = armourmoduleengineered.ThermalResistance ?? 0;
                    var expres = armourmoduleengineered.ExplosiveResistance ?? 0;
                    var caures = armourmoduleengineered.CausticResistance ?? 0;     // should always be not defined.
                    res.ArmourRaw = armour * (1 + hullbst / 100.0) + hullrnf;
                    res.ArmourKineticPercentage = getEffectiveDamageResistance(kinres, (1 - kinmod_ihrp) * 100, 0, (1 - kinmin_ihrp) * 100);
                    res.ArmourThermalPercentage = getEffectiveDamageResistance(thmres, (1 - thmmod_ihrp) * 100, 0, (1 - thmmin_ihrp) * 100);
                    res.ArmourExplosivePercentage = getEffectiveDamageResistance(expres, (1 - expmod_ihrp) * 100, 0, (1 - expmin_ihrp) * 100);
                    res.ArmourCausticPercentage = getEffectiveDamageResistance(caures, (1 - caumod_ihrp) * 100, 0, (1 - caumin_ihrp) * 100);

                    res.ArmourKineticValue = res.ArmourRaw.Value / (1 - res.ArmourKineticPercentage / 100);
                    res.ArmourThermalValue = res.ArmourRaw.Value / (1 - res.ArmourThermalPercentage / 100);
                    res.ArmourExplosiveValue = res.ArmourRaw.Value / (1 - res.ArmourExplosivePercentage / 100);
                    res.ArmourCausticValue = res.ArmourRaw.Value / (1 - res.ArmourCausticPercentage / 100);

                }


                var shieldlist = FindShipModules(x => x.GetModuleUnengineered()?.IsShieldGenerator ?? false); // allow get module unengineered to fail, slots with shields 
                ItemData.ShipModule shieldmoduleengineered = shieldlist.Length == 1 ? GetShipModulePropertiesEngineered(shieldlist[0]) : null;

                if (shieldmoduleengineered != null)
                {
                    var mass_hull = ship.HullMass;
                    var maxmass = shieldmoduleengineered.MaxMass;
                    if (maxmass >= mass_hull)
                    {
                        var shields = ship.Shields;
                        var minmass = shieldmoduleengineered.MinMass;
                        var optmass = shieldmoduleengineered.OptMass;
                        var minmul = shieldmoduleengineered.MinStrength;
                        var optmul = shieldmoduleengineered.OptStrength;
                        var maxmul = shieldmoduleengineered.MaxStrength;
                        var kinres = shieldmoduleengineered.KineticResistance ?? 0;
                        var thmres = shieldmoduleengineered.ThermalResistance ?? 0;
                        var expres = shieldmoduleengineered.ExplosiveResistance ?? 0;
                        var caures = shieldmoduleengineered.CausticResistance ?? 0;  // should always be not defined.
                        double rawShdStr = shields * getEffectiveShieldBoostMultiplier(shieldbst) *
                                                    getMassCurveMultiplier(mass_hull, minmass.Value, optmass.Value, maxmass.Value, minmul.Value, optmul.Value, maxmul.Value) / 100
                                                    + shieldrnf;
                        res.ShieldsRaw = rawShdStr;

                        var kinShdRes = res.ShieldsKineticPercentage = getEffectiveDamageResistance(0, (1 - kinmod_usb) * 100, kinres, 0);
                        var thmShdRes = res.ShieldsThermalPercentage = getEffectiveDamageResistance(0, (1 - thmmod_usb) * 100, thmres, 0);
                        var expShdRes = res.ShieldsExplosivePercentage = getEffectiveDamageResistance(0, (1 - expmod_usb) * 100, expres, 0);

                        var absShdRes = res.ShieldsSystemPercentage = getPipDamageResistance(powerdist_sys);
                        res.ShieldsSystemValue = rawShdStr / (1 - absShdRes / 100);
                        res.ShieldsKineticValue = rawShdStr / (1 - absShdRes / 100) / (1 - kinShdRes / 100);
                        res.ShieldsThermalValue = rawShdStr / (1 - absShdRes / 100) / (1 - thmShdRes / 100);
                        res.ShieldsExplosiveValue = rawShdStr / (1 - absShdRes / 100) / (1 - expShdRes / 100);

                        //System.Diagnostics.Debug.WriteLine($"ABs {absShdRes * 100}% raw {res.ShieldsRaw} sys {res.ShieldsSystemValue} kin {res.ShieldsKineticValue} thm {res.ShieldsThermalValue} exp {res.ShieldsExplosiveValue}");


                        if (powerdistributorengineered != null)
                        {
                            var syscap = powerdistributorengineered.SystemsCapacity.Value;
                            var syschg = powerdistributorengineered.SystemsRechargeRate.Value;

                            var powerdistSysMul = Math.Pow((double)powerdist_sys / MAX_POWER_DIST, 1.1);
                            var bgenrate = shieldmoduleengineered.BrokenRegenRate.Value;
                            var genrate = shieldmoduleengineered.RegenRate.Value;
                            var distdraw_mj = shieldmoduleengineered.MWPerUnit.Value;

                            var bgenFastTime = Math.Min((rawShdStr / 2 / bgenrate), (syscap / Math.Max(0, bgenrate * distdraw_mj - syschg * powerdistSysMul)));
                            var bgenSlowTime = (rawShdStr / 2 - bgenrate * bgenFastTime) / Math.Min(bgenrate, (syschg * powerdistSysMul) / distdraw_mj);

                            var genFastTime = 0;
                            var genSlowTime = (rawShdStr / 2) / Math.Min(genrate, (syschg * powerdistSysMul) / distdraw_mj);

                            res.ShieldBuildTime = 16 + bgenFastTime + bgenSlowTime;
                            res.ShieldRegenTime = genFastTime + genSlowTime;
                        }
                    }
                    else
                        res.ShieldsRaw = 0;     // mass too big
                }

                var hullmodulemass = HullModuleMass();

                ItemData.ShipModule thrustersengineered = GetShipModulePropertiesEngineered(ShipSlots.Slot.MainEngines);

                if ( powerdistributorengineered != null && thrustersengineered != null)
                {
                    var minthrust = ship.MinThrust / 100;
                    var boostcost = ship.BoostCost;
                    var topspd = ship.Speed;
                    var bstspd = ship.Boost;

                    var engcap = powerdistributorengineered.EngineCapacity.Value;
                    var engchg = powerdistributorengineered.EngineRechargeRate.Value;

                    var minmass = thrustersengineered.MinMass.Value;
                    var optmass = thrustersengineered.OptMass.Value;
                    var maxmass = thrustersengineered.MaxMass.Value;
                    var minmulspd = thrustersengineered.EngineMinMultiplier.Value;
                    var optmulspd = thrustersengineered.EngineOptMultiplier.Value;
                    var maxmulspd = thrustersengineered.EngineMaxMultiplier.Value;
                    var fuelcap = FuelCapacity;

                    var powerdistEngMul = (double)powerdist_eng / MAX_POWER_DIST;

                    var cargocap = CalculateCargoCapacity();

                    var curNavSpdMul = getMassCurveMultiplier(hullmodulemass + currentfuellevel + currentreservelevel + currentcargo, minmass, optmass, maxmass, minmulspd, optmulspd, maxmulspd) / 100;
                    var ldnNavSpdMul = getMassCurveMultiplier(hullmodulemass + fuelcap + cargocap, minmass, optmass, maxmass, minmulspd, optmulspd, maxmulspd) / 100;
                    var unlNavSpdMul = getMassCurveMultiplier(hullmodulemass + fuelcap, minmass, optmass, maxmass, minmulspd, optmulspd, maxmulspd) / 100;
                    var maxNavSpdMul = getMassCurveMultiplier(hullmodulemass, minmass, optmass, maxmass, minmulspd, optmulspd, maxmulspd) / 100;

                    res.CurrentSpeed = curNavSpdMul * topspd * (powerdistEngMul + minthrust * (1 - powerdistEngMul));
                    res.LadenSpeed = ldnNavSpdMul * topspd;
                    res.UnladenSpeed = unlNavSpdMul * topspd;
                    res.MaxSpeed = maxNavSpdMul * topspd;

                    res.CurrentBoost = curNavSpdMul * bstspd;
                    res.LadenBoost = ldnNavSpdMul * bstspd;
                    res.UnladenBoost = unlNavSpdMul * bstspd;
                    res.MaxBoost = maxNavSpdMul * bstspd;

                    res.CurrentBoostFrequency = (boostcost / (engchg * Math.Pow((double)powerdist_eng / MAX_POWER_DIST, 1.1)));
                    res.MaxBoostFrequency = (boostcost / engchg);
                }

                var fsdspec = GetFSDSpec();
                if ( fsdspec != null )
                {
                    res.FSDCurrentRange = fsdspec.JumpRange(currentcargo, hullmodulemass, currentfuellevel, 1.0);
                    res.FSDCurrentMaxRange = fsdspec.CalculateMaxJumpDistance(currentcargo, hullmodulemass, currentfuellevel, out double _);
                    res.FSDLadenRange = fsdspec.JumpRange(CalculateCargoCapacity(), hullmodulemass, FuelCapacity, 1.0);
                    res.FSDUnladenRange = fsdspec.JumpRange(0, hullmodulemass, FuelCapacity, 1.0);
                    res.FSDMaxRange = fsdspec.JumpRange(0, hullmodulemass, Math.Min(FuelCapacity, fsdspec.MaxFuelPerJump), 1.0);
                    res.FSDMaxFuelPerJump = fsdspec.MaxFuelPerJump;
                }

                if (powerdistributorengineered != null)
                {
                    double wepchg = powerdistributorengineered.WeaponsRechargeRate.Value;
                    double powerdistWepMul = Math.Pow((double)powerdist_wep / MAX_POWER_DIST, 1.1);

                    // sort by spc (first tuple entry) spc,eps,seps
                    weapons.Sort(delegate (Tuple<double, double, double> left, Tuple<double, double, double> right) { return left.Item1.CompareTo(right.Item1); });

                    double eps = 0;
                    double seps = 0;
                    foreach (var w in weapons)      // spc,eps,seps
                    {
                        eps += w.Item2;
                        seps += w.Item3;
                    }

                    var eps_cur = eps;
                    var eps_max = eps;
                    double wepcap_burst_cur = (wepcap / Math.Max(0, eps_cur - wepchg * powerdistWepMul));
                    double wepcap_burst_max = (wepcap / Math.Max(0, eps_max - wepchg));
                    foreach (var w in weapons)      // spc,eps,seps
                    {
                        if (wepcap_burst_cur >= w.Item1)        // spc
                        {
                            eps_cur = eps_cur - w.Item2 + w.Item3; // + eps-seps
                            wepcap_burst_cur = (wepcap / Math.Max(0, eps_cur - wepchg * powerdistWepMul));
                        }
                        if (wepcap_burst_max >= w.Item1) // spc
                        {
                            eps_max = eps_max - w.Item2 + w.Item3;
                            wepcap_burst_max = (wepcap / Math.Max(0, eps_max - wepchg));
                        }
                    }

                    double wepchg_sustain_cur = Math.Min(Math.Max(wepchg * powerdistWepMul / seps, 0), 1);
                    double wepchg_sustain_max = Math.Min(Math.Max(wepchg / seps, 0), 1);

                    //System.Diagnostics.Debug.WriteLine($"WEPAccum {wepcap_burst_cur} {wepcap_burst_max} {wepchg_sustain_cur} {wepchg_sustain_max}");

                    // compute derived stats
                    var curWpnSus = ((dps_nodistdraw + (dps_distdraw !=0 ? (dps_distdraw * wepchg_sustain_cur) : 0)) / dps);
                    var maxWpnSus = ((dps_nodistdraw + (dps_distdraw !=0 ? (dps_distdraw * wepchg_sustain_max) : 0)) / dps);
                    var ammWpnDur = Math.Min(ammotime_nocap, ((ammotime_wepcap <= wepcap_burst_max) ? ammotime_wepcap : (wepcap_burst_max + (ammotime_wepcap - wepcap_burst_max) / maxWpnSus)));

                    res.WeaponRaw = dps;
                    res.WeaponAbsolutePercentage = dps_abs / dps * 100.0;
                    res.WeaponKineticPercentage = dps_kin / dps * 100.0;
                    res.WeaponThermalPercentage = dps_thm / dps * 100.0;
                    res.WeaponExplosivePercentage = dps_exp / dps * 100.0;
                    res.WeaponAXPercentage = dps_axe / dps * 100.0;
                    res.WeaponDuration = wepcap_burst_cur;
                    res.WeaponDurationMax = wepcap_burst_max;
                    res.WeaponAmmoDuration = ammWpnDur;
                    res.WeaponCurSus = curWpnSus * 100.0;
                    res.WeaponMaxSus = maxWpnSus * 100.0;
                }

                return res;
            }
            else
                return null;
        }

        #endregion

        #region Creating and changing

        public Ship(ulong id)
        {
            ID = id;
            Modules = new Dictionary<ShipSlots.Slot, ShipModule>();
        }

        public Ship ShallowClone()          // shallow clone.. does not clone the ship modules, just the dictionary
        {
            Ship sm = new Ship(this.ID);
            sm.State = this.State;
            sm.ShipType = this.ShipType;
            sm.ShipFD = this.ShipFD;
            sm.ShipUserName = this.ShipUserName;
            sm.ShipUserIdent = this.ShipUserIdent;
            sm.FuelLevel = this.FuelLevel;
            sm.FuelCapacity = this.FuelCapacity;
            sm.SubVehicle = this.SubVehicle;
            sm.HullValue = this.HullValue;
            sm.HullHealthAtLoadout = this.HullHealthAtLoadout;
            sm.ModulesValue = this.ModulesValue;
            sm.UnladenMass = this.UnladenMass;
            sm.Rebuy = this.Rebuy;
            sm.ReserveFuelCapacity = this.ReserveFuelCapacity;
            sm.ReserveFuelLevel = this.ReserveFuelLevel;
            sm.StoredAtStation = this.StoredAtStation;
            sm.StoredAtSystem = this.StoredAtSystem;
            sm.TransferArrivalTimeUTC = this.TransferArrivalTimeUTC;
            sm.Hot = this.Hot;
            sm.Modules = new Dictionary<ShipSlots.Slot, ShipModule>(this.Modules);
            return sm;
        }

        public bool Contains(ShipSlots.Slot slot)
        {
            return Modules.ContainsKey(slot);
        }

        public bool Same(ShipModule sm)
        {
            if (Modules.ContainsKey(sm.SlotFD))
            {
                return Modules[sm.SlotFD].Same(sm);
            }
            else
                return false;
        }

        public void SetModule(ShipModule sm)                // changed the module array, so you should have cloned that first..
        {
            if (Modules.ContainsKey(sm.SlotFD))
            {
                ShipModule oldsm = Modules[sm.SlotFD];

                if (sm.Item.Equals(oldsm.Item) && sm.LocalisedItem == null && oldsm.LocalisedItem != null)  // if item the same, old one has a localised name..
                    sm.LocalisedItem = oldsm.LocalisedItem; // keep it

            }

            Modules[sm.SlotFD] = sm;

            if (sm.Item.Contains("Fuel Tank") && sm.Item.IndexOf("Class ") != -1)
            {
                FuelCapacity = CalculateFuelCapacity();
                if (FuelLevel > FuelCapacity)
                    FuelLevel = FuelCapacity;
            }
        }

        public Ship SetShipDetails(string ship, string shipfd, string name = null, string ident = null, 
                                    double fuellevel = 0, double fueltotal = 0,
                                    long hullvalue = 0, long modulesvalue = 0, long rebuy = 0,
                                    double unladenmass = 0, double reservefuelcapacity = 0 , double hullhealth = 0, bool? hot = null)
        {
            System.Diagnostics.Debug.Assert(shipfd != null && ship != null);

            bool s1 = ShipFD != shipfd;
            bool s2 = ship != ShipType;
            bool s3 = name != null && name != ShipUserName;
            bool s4 = ident != null && ident != ShipUserIdent;
            bool s5 = fuellevel != 0 && fuellevel != FuelLevel;
            bool s6 = fueltotal != 0 && fueltotal != FuelCapacity;
            bool s7 = hullvalue != 0 && hullvalue != HullValue;
            bool s8 = modulesvalue != 0 && modulesvalue != ModulesValue;
            bool s9 = rebuy != 0 && rebuy != Rebuy;
            bool s10 = unladenmass != 0 && unladenmass != UnladenMass;
            bool s11 = reservefuelcapacity != 0 && reservefuelcapacity != ReserveFuelCapacity;
            bool s12 = hullhealth != 0 && HullHealthAtLoadout != hullhealth;
            bool s13 = hot != null && hot.Value != Hot;

            if (s1 || s2 || s3 || s4 || s5 || s6 || s7 || s8 || s9 || s10 || s11 || s12 || s13 )
            {
                //System.Diagnostics.Debug.WriteLine($".. update SetShipDetails");

                Ship sm = this.ShallowClone();

                sm.ShipType = ship;
                sm.ShipFD = shipfd;
                if (name.HasNonSpaceChars())        // seen " " as a name!
                    sm.ShipUserName = name;
                if (ident.HasNonSpaceChars())
                    sm.ShipUserIdent = ident;
                if (fuellevel != 0)
                    sm.FuelLevel = fuellevel;
                if (fueltotal == 0 && fuellevel > sm.FuelCapacity)
                    sm.FuelCapacity = fuellevel;
                if (fueltotal != 0)
                    sm.FuelCapacity = fueltotal;
                if (hullvalue != 0)
                    sm.HullValue = hullvalue;
                if (modulesvalue != 0)
                    sm.ModulesValue = modulesvalue;
                if (rebuy != 0)
                    sm.Rebuy = rebuy;
                if (unladenmass != 0)
                    sm.UnladenMass = unladenmass;
                if (reservefuelcapacity != 0)
                    sm.ReserveFuelCapacity = reservefuelcapacity;
                if (hullhealth != 0)
                    sm.HullHealthAtLoadout = hullhealth;

                if (hot != null)
                    sm.Hot = hot.Value;

                //System.Diagnostics.Debug.WriteLine(ship + " " + sm.FuelCapacity + " " + sm.FuelLevel + " " + sm.ReserveFuelCapacity);

                return sm;
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($".. don't update SetShipDetails");
                return this;
            }
        }

        public Ship SetSubVehicle(SubVehicleType vh)
        {
            if (vh != this.SubVehicle)
            {
                Ship sm = this.ShallowClone();
                sm.SubVehicle = vh;
                return sm;
            }
            else
                return this;
        }

        public Ship SetFuelLevel(double fuellevel)
        {
            if (fuellevel != 0 && fuellevel != FuelLevel)
            {
                Ship sm = this.ShallowClone();

                if (fuellevel != 0)
                    sm.FuelLevel = fuellevel;
                if (fuellevel > sm.FuelCapacity)
                    sm.FuelCapacity = fuellevel;

                return sm;
            }

            return this;
        }

        // from  UI event Fuel
        public Ship SetFuelLevel(double fuellevel, double reservelevel)       // fuellevel >=0 to set
        {
            if (fuellevel >= 0 && ( Math.Abs(FuelLevel - fuellevel) > 0.01 || Math.Abs(ReserveFuelLevel - reservelevel) > 0.01))
            {
                //System.Diagnostics.Debug.WriteLine("Update ship fuel to " + fuellevel + " " + reserve);

                Ship sm = this.ShallowClone();

                if (fuellevel != 0)
                    sm.FuelLevel = fuellevel;
                if (fuellevel > sm.FuelCapacity)
                    sm.FuelCapacity = fuellevel;
                sm.ReserveFuelLevel = reservelevel;

                return sm;
            }

            return this;
        }

        public Ship AddModule(string slot, ShipSlots.Slot slotfd, string item, string itemfd, string itemlocalised)
        {
            if (!Modules.ContainsKey(slotfd) || Modules[slotfd].Item.Equals(item) == false)       // if does not have it, or item is not the same..
            {
                Ship sm = this.ShallowClone();
                sm.Modules[slotfd] = new ShipModule(slot, slotfd, item, itemfd, itemlocalised);
                //System.Diagnostics.Debug.WriteLine("Slot add " + slot);

                if (item.Contains("Fuel Tank") && item.IndexOf("Class ") != -1)
                {
                    sm.FuelCapacity = sm.CalculateFuelCapacity();
                    if (sm.FuelLevel > sm.FuelCapacity)
                        sm.FuelLevel = sm.FuelCapacity;
                }

                return sm;
            }
            return this;
        }

        public Ship RemoveModule(ShipSlots.Slot slot, string item)
        {
            if (Modules.ContainsKey(slot))       // if has it..
            {
                Ship sm = this.ShallowClone();
                sm.Modules.Remove(slot);
                //System.Diagnostics.Debug.WriteLine("Slot remove " + slot);

                if (item.Contains("Fuel Tank") && item.IndexOf("Class ") != -1)
                {
                    sm.FuelCapacity = sm.CalculateFuelCapacity();
                    if (sm.FuelLevel > sm.FuelCapacity)
                        sm.FuelLevel = sm.FuelCapacity;
                }

                return sm;
            }
            return this;
        }

        public Ship RemoveModules(JournalMassModuleStore.ModuleItem[] items)
        {
            Ship sm = null;
            foreach (var it in items)
            {
                if (Modules.ContainsKey(it.SlotFD))       // if has it..
                {
                    if (sm == null)
                        sm = this.ShallowClone();

                    //System.Diagnostics.Debug.WriteLine("Slot mass remove " + it.Slot + " Exists " + sm.Modules.ContainsKey(it.Slot));
                    sm.Modules.Remove(it.SlotFD);

                    if (it.Name.Contains("Fuel Tank") && it.Name.IndexOf("Class ") != -1)
                    {
                        sm.FuelCapacity = sm.CalculateFuelCapacity();
                        if (sm.FuelLevel > sm.FuelCapacity)
                            sm.FuelLevel = sm.FuelCapacity;
                    }
                }
            }

            return sm ?? this;
        }

        public Ship SwapModule(string fromslot, ShipSlots.Slot fromslotfd, string fromitem, string fromitemfd, string fromiteml,
                                          string toslot, ShipSlots.Slot toslotfd, string toitem, string toitemfd, string toiteml)
        {
            Ship sm = this.ShallowClone();
            if (Modules.ContainsKey(fromslotfd))
            {
                if (Modules.ContainsKey(toslotfd))
                {
                    sm.Modules[fromslotfd] = new ShipModule(fromslot, fromslotfd, toitem, toitemfd, toiteml);
                }
                else
                    sm.Modules.Remove(fromslotfd);

                sm.Modules[toslotfd] = new ShipModule(toslot, toslotfd, fromitem, fromitemfd, fromiteml);

                if (fromitem != toitem && ((fromitem.Contains("Fuel Tank") && fromitem.IndexOf("Class ") != -1) ||
                                           (fromitem.Contains("Fuel Tank") && fromitem.IndexOf("Class ") != -1)))
                {
                    sm.FuelCapacity = sm.CalculateFuelCapacity();
                    if (sm.FuelLevel > sm.FuelCapacity)
                        sm.FuelLevel = sm.FuelCapacity;
                }
            }
            return sm;
        }

        public Ship Craft(ShipSlots.Slot slotfd, string item, EngineeringData eng)
        {
            if (Modules.ContainsKey(slotfd) && Modules[slotfd].Item.Equals(item))       // craft, module must be there, otherwise just ignore
            {
                Ship sm = this.ShallowClone();
                sm.Modules[slotfd] = new ShipModule(sm.Modules[slotfd]);        // clone
                sm.Modules[slotfd].SetEngineering(eng);                       // and update engineering
                return sm;
            }

            return this;
        }

        public Ship SellShip()
        {
            Ship sm = this.ShallowClone();
            sm.State = ShipState.Sold;
            sm.SubVehicle = SubVehicleType.None;
            sm.ClearStorage();
            return sm;
        }

        public Ship Destroyed()
        {
            Ship sm = this.ShallowClone();
            sm.State = ShipState.Destroyed;
            sm.SubVehicle = SubVehicleType.None;
            sm.ClearStorage();
            return sm;
        }

        public Ship Store(string station, string system)
        {
            Ship sm = this.ShallowClone();
            //if (sm.StoredAtSystem != null) { if (sm.StoredAtSystem.Equals(system)) System.Diagnostics.Debug.WriteLine("..Previous known stored at" + sm.StoredAtSystem + ":" + sm.StoredAtStation); else System.Diagnostics.Debug.WriteLine("************************ DISGREEE..Previous known stored at" + sm.StoredAtSystem + ":" + sm.StoredAtStation); }
            sm.SubVehicle = SubVehicleType.None;
            sm.StoredAtSystem = system;
            sm.StoredAtStation = station ?? sm.StoredAtStation;     // we may get one with just the system, so use the previous station if we have one
            //System.Diagnostics.Debug.WriteLine(".." + ShipFD + " Stored at " + sm.StoredAtSystem + ":" + sm.StoredAtStation);
            return sm;                                              // don't change transfer time as it may be in progress..
        }

        public Ship SwapTo()
        {
            Ship sm = this.ShallowClone();
            sm.ClearStorage();    // just in case
            return sm;
        }

        public Ship Transfer(string tosystem , string tostation, DateTime arrivaltimeutc)
        {
            Ship sm = this.ShallowClone();
            sm.StoredAtStation = tostation;
            sm.StoredAtSystem = tosystem;
            sm.TransferArrivalTimeUTC = arrivaltimeutc;
            return sm;
        }

        private void ClearStorage()
        {
            StoredAtStation = StoredAtSystem = null;
            TransferArrivalTimeUTC = DateTime.MinValue;
        }

        #endregion

        #region Export

        public bool CheckMinimumModulesForCoriolisEDSY()
        {
            return GetModuleInSlot(ShipSlots.Slot.PowerPlant) != null && GetModuleInSlot(ShipSlots.Slot.MainEngines) != null &&
                    GetModuleInSlot(ShipSlots.Slot.FrameShiftDrive) != null && GetModuleInSlot(ShipSlots.Slot.LifeSupport) != null &&
                    GetModuleInSlot(ShipSlots.Slot.PowerDistributor) != null && GetModuleInSlot(ShipSlots.Slot.Radar) != null &&
                    GetModuleInSlot(ShipSlots.Slot.FuelTank) != null && GetModuleInSlot(ShipSlots.Slot.Armour) != null;
        }

        public string ToJSONCoriolis(out string errstring)
        {
            return JSONCoriolis(out errstring).ToString();
        }
        
        public JObject JSONCoriolis(out string errstring)
        {
            errstring = "";

            JObject jo = new JObject();

            jo["event"] = "Loadout";
            jo["Ship"] = ShipFD;

            JArray mlist = new JArray();
            foreach (ShipModule sm in Modules.Values)
            {
                JObject module = new JObject();

                if (ItemData.TryGetShipModule(sm.ItemFD, out ItemData.ShipModule si, false) && si.ModuleID != 0)   // don't synth it
                {
                    module["Item"] = sm.ItemFD;
                    module["Slot"] = sm.SlotFD.ToString();
                    module["On"] = sm.Enabled.HasValue ? sm.Enabled : true;
                    module["Priority"] = sm.Priority.HasValue ? sm.Priority : 0;

                    if (sm.Engineering != null)
                        module["Engineering"] = ToJsonCoriolisEngineering(sm);

                    mlist.Add(module);
                }
                else
                {
                    errstring += sm.Item + ":" + sm.ItemFD + Environment.NewLine;
                }
            }

            jo["Modules"] = mlist;

            return jo;
        }

        private JObject ToJsonCoriolisEngineering(ShipModule module)
        {
            JObject engineering = new JObject();

            engineering["BlueprintID"] = module.Engineering.BlueprintID;
            engineering["BlueprintName"] = module.Engineering.BlueprintName;
            engineering["Level"] = module.Engineering.Level;
            engineering["Quality"] = module.Engineering.Quality;

            if (module.Engineering.Modifiers != null) // may not have any
            {
                JArray modifiers = new JArray();
                foreach (EngineeringModifiers modifier in module.Engineering.Modifiers)
                {
                    JObject jmodifier = new JObject();
                    jmodifier["Label"] = modifier.Label;
                    jmodifier["Value"] = modifier.Value;
                    jmodifier["OriginalValue"] = modifier.OriginalValue;
                    jmodifier["LessIsGood"] = modifier.LessIsGood;
                    modifiers.Add(jmodifier);
                }

                engineering["Modifiers"] = modifiers;
            }

            if (module.Engineering.ExperimentalEffect.HasChars() )
                engineering["ExperimentalEffect"] = module.Engineering.ExperimentalEffect;

            return engineering;
        }

        public string ToJSONLoadout()
        {
            return JSONLoadout().ToString();
        }

        public JObject JSONLoadout()
        {
            JObject jo = new JObject();

            jo["timestamp"] = DateTime.UtcNow.ToStringZuluInvariant();
            jo["event"] = "Loadout";
            jo["Ship"] = ShipFD;
            jo["ShipID"] = ID;
            if (!string.IsNullOrEmpty(ShipUserName))
                jo["ShipName"] = ShipUserName;
            if (!string.IsNullOrEmpty(ShipUserIdent))
                jo["ShipIdent"] = ShipUserIdent;
            if (HullValue > 0)
                jo["HullValue"] = HullValue;
            if (ModulesValue > 0)
                jo["ModulesValue"] = ModulesValue;
            if (HullHealthAtLoadout > 0)
                jo["HullHealth"] = HullHealthAtLoadout / 100.0;
            if (UnladenMass > 0)
                jo["UnladenMass"] = UnladenMass;
            jo["CargoCapacity"] = CalculateCargoCapacity();
            if (FuelCapacity > 0 && ReserveFuelCapacity > 0)
            {
                JObject fc = new JObject();
                fc["Main"] = FuelCapacity;
                fc["Reserve"] = ReserveFuelCapacity;
                jo["FuelCapacity"] = fc;
            }
            if (Rebuy > 0)
                jo["Rebuy"] = Rebuy;

            JArray mlist = new JArray();

            foreach (ShipModule sm in Modules.Values)
            {
                JObject module = new JObject();

                module["Slot"] = sm.SlotFD.ToString();
                module["Item"] = sm.ItemFD;
                module["On"] = sm.Enabled.HasValue ? sm.Enabled : true;
                module["Priority"] = sm.Priority.HasValue ? sm.Priority : 0;

                if (sm.Value.HasValue)
                    module["Value"] = sm.Value;

                if ( sm.Engineering != null )
                    module["Engineering"] = sm.Engineering.ToJSONLoadout();

                mlist.Add(module);
            }

            jo["Modules"] = mlist;

            return jo;
        }

        #endregion

        #region Create from loadout

        public static Ship CreateFromLoadout(string loadout)
        {
            JToken jo = JToken.Parse(loadout);

            EliteDangerousCore.JournalEvents.JournalLoadout jloadout = null;
            if (jo != null)
            {
                if (jo.IsArray && jo.Count == 1 && jo[0].IsObject && jo[0].Object().Contains("header") && jo[0].Object().Contains("data"))
                {
                    jo = jo[0]["data"];
                    jloadout = new EliteDangerousCore.JournalEvents.JournalLoadout(jo.Object());
                }
                else if (jo.IsObject && jo["event"].Str() == "Loadout")
                {
                    jloadout = new EliteDangerousCore.JournalEvents.JournalLoadout(jo.Object());
                }
            }

            if (jloadout != null)
            {
                ShipList sl = new ShipList();
                jloadout.ShipInformation(sl, "Nowhere", new SystemClass("Sol"));
                if (sl.Ships.Count > 0)
                {
                    Ship importedship = sl.Ships.First().Value;
                    importedship.State = Ship.ShipState.Imported;
                    importedship.FuelLevel = importedship.FuelCapacity; // presume half tank
                    return importedship;
                }
            }

            return null;
        }

        #endregion

        #region Helpers

        // called when modules changes
        private int CalculateFuelCapacity()
        {
            int cap = 0;
            foreach (ShipModule sm in Modules.Values)
            {
                int classpos;
                if (sm.Item.Contains("Fuel Tank") && (classpos = sm.Item.IndexOf("Class ")) != -1)
                {
                    char digit = sm.Item[classpos + 6];
                    cap += (1 << (digit - '0'));        // 1<<1 = 2.. 1<<2 = 4, etc.
                }
            }

            return cap;
        }


        private double getEffectiveDamageResistance(double baseres, double extrares, double exemptres, double bestres)
        {
            // https://forums.frontier.co.uk/threads/kinetic-resistance-calculation.266235/post-4230114
            // https://forums.frontier.co.uk/threads/shield-booster-mod-calculator.286097/post-4998592

            var lo = Math.Max(Math.Max(30, baseres), bestres);
            var hi = 65; // half credit past 30% means 100% -> 30 + (100 - 30) / 2 = 65%
            var expected = (1 - ((1 - baseres / 100) * (1 - extrares / 100))) * 100;
            var penalized = lo + (expected - lo) / (100 - lo) * (hi - lo); // remap range [lo..100] to [lo..hi]
            var actual = ((penalized >= 30) ? penalized : expected);
            return (1 - ((1 - exemptres / 100) * (1 - actual / 100))) * 100;
        }

        private double getEffectiveShieldBoostMultiplier(double shieldbst)
        {
            // https://forums.frontier.co.uk/threads/very-experimental-shield-change.314820/post-4895068
            var i = (1 + (shieldbst / 100));
            return i;
        }

        private double getMassCurveMultiplier(double mass, double minMass, double optMass, double maxMass, double minMul, double optMul, double maxMul)
        {
            // https://forums.frontier.co.uk/threads/the-one-formula-to-rule-them-all-the-mechanics-of-shield-and-thruster-mass-curves.300225/

            return (minMul + Math.Pow(Math.Min(1.0, (maxMass - mass) / (maxMass - minMass)), Math.Log((optMul - minMul) / (maxMul - minMul)) / Math.Log((maxMass - optMass) / (maxMass - minMass))) * (maxMul - minMul));
        }

        private double getPipDamageResistance(double sys)
        {
            // https://forums.frontier.co.uk/threads/2-3-the-commanders-changelog.341916/
            return 60 * Math.Pow(sys / MAX_POWER_DIST, 0.85);

        }

        private double getEffectiveWeaponThermalLoad( double thmload, double distdraw, double wepcap, double weplvl) 
        {
		    // https://forums.frontier.co.uk/threads/research-detailed-heat-mechanics.286628/post-6408594
    		return (thmload* (1 + 4 * Math.Min(Math.Max(1 - (wepcap* weplvl - distdraw) / wepcap, 0), 1)));
    	}


    const int MAX_POWER_DIST = 8;
        const double BOOST_MARGIN = 0.005;
        #endregion
    }
}

