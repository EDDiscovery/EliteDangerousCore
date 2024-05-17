/*
 * Copyright 2016-2024 EDDiscovery development team
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
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        static public bool TryGetShipModule(string fdid, out ShipModule m, bool synthesiseit)
        {
            m = null;
            string lowername = fdid.ToLowerInvariant();

            // try the static values first, this is thread safe
            bool state = shipmodules.TryGetValue(lowername, out m) || othershipmodules.TryGetValue(lowername, out m) ||
                        srvmodules.TryGetValue(lowername, out m) || fightermodules.TryGetValue(lowername, out m) || vanitymodules.TryGetValue(lowername, out m);

            if (state == false)    // not found, try find the synth modules. Since we can be called in journal creation thread, we need some safety.
            {
                lock (synthesisedmodules)
                {
                    state = synthesisedmodules.TryGetValue(lowername, out m);
                }
            }


            if (!state && synthesiseit)   // if not found, and we want to synthesise it
            {
                lock (synthesisedmodules)  // lock for safety
                {
                    string candidatename = fdid;
                    candidatename = candidatename.Replace("weaponcustomisation", "WeaponCustomisation").Replace("testbuggy", "SRV").
                                            Replace("enginecustomisation", "EngineCustomisation");

                    candidatename = candidatename.SplitCapsWordFull();

                    var newmodule = new ShipModule(-1, IsVanity(lowername) ? ShipModule.ModuleTypes.VanityType : ShipModule.ModuleTypes.UnknownType, candidatename);
                    System.Diagnostics.Trace.WriteLine($"*** Unknown Module {{ \"{lowername}\", new ShipModule(-1,{(IsVanity(lowername) ? "ShipModule.ModuleTypes.VanityType" : "ShipModule.ModuleTypes.UnknownType")},\"{candidatename}\" }}, - this will not affect operation but it would be nice to report it to us so we can add it to known module lists");

                    synthesisedmodules[lowername] = m = newmodule;                   // lets cache them for completeness..
                }
            }

            return state;
        }

        // List of ship modules. Synthesised are not included
        // default is buyable modules only
        // you can include other types
        // compressarmour removes all armour entries except the ones for the sidewinder
        static public Dictionary<string, ShipModule> GetShipModules(bool includebuyable = true, bool includenonbuyable = false, bool includesrv = false,
                                                                    bool includefighter = false, bool includevanity = false, bool addunknowntype = false,
                                                                    bool compressarmourtosidewinderonly = false)
        {
            Dictionary<string, ShipModule> ml = new Dictionary<string, ShipModule>();

            if (includebuyable)
            {
                foreach (var x in shipmodules) ml[x.Key] = x.Value;
            }

            if (compressarmourtosidewinderonly)        // remove all but _grade1 armours in list
            {
                var list = shipmodules.Keys;
                foreach (var name in list)
                {
                    if (name.Contains("_armour_") && !name.Contains("sidewinder")) // only keep sidewinder - all other ones are removed
                        ml.Remove(name);
                }
            }

            if (includenonbuyable)
            {
                foreach (var x in othershipmodules) ml[x.Key] = x.Value;
            }
            if (includesrv)
            {
                foreach (var x in srvmodules) ml[x.Key] = x.Value;
            }
            if (includefighter)
            {
                foreach (var x in fightermodules) ml[x.Key] = x.Value;

            }
            if (includevanity)
            {
                foreach (var x in vanitymodules) ml[x.Key] = x.Value;

            }
            if (addunknowntype)
            {
                ml["Unknown"] = new ShipModule(-1, ShipModule.ModuleTypes.UnknownType, "Unknown Type");
            }
            return ml;
        }

        // a dictionary of module english module type vs translated module type for a set of modules
        public static Dictionary<string, string> GetModuleTypeNamesTranslations(Dictionary<string, ShipModule> modules)
        {
            var ret = new Dictionary<string, string>();
            foreach (var x in modules)
            {
                if (!ret.ContainsKey(x.Value.EnglishModTypeString))
                    ret[x.Value.EnglishModTypeString] = x.Value.TranslatedModTypeString;
            }
            return ret;
        }

        // given a module name list containing siderwinder_armour_gradeX only,
        // expand out to include all other ships armours of the same grade
        // used in spansh station to reduce list of shiptype armours shown, as if one is there for a ship, they all are there for all ships
        public static string[] ExpandArmours(string[] list)
        {
            List<string> ret = new List<string>();
            foreach (var x in list)
            {
                if (x.StartsWith("sidewinder_armour"))
                {
                    string grade = x.Substring(x.IndexOf("_"));     // its grade (_armour_grade1, _grade2 etc)

                    foreach (var kvp in shipmodules)
                    {
                        if (kvp.Key.EndsWith(grade))
                            ret.Add(kvp.Key);
                    }
                }
                else
                    ret.Add(x);
            }

            return ret.ToArray();
        }

        static public bool IsVanity(string ifd)
        {
            ifd = ifd.ToLowerInvariant();
            string[] vlist = new[] { "bobble", "decal", "enginecustomisation", "nameplate", "paintjob",
                                    "shipkit", "weaponcustomisation", "voicepack" , "lights", "spoiler" , "wings", "bumper"};
            return Array.Find(vlist, x => ifd.Contains(x)) != null;
        }

        static string TXIT(string text)
        {
            return BaseUtils.Translator.Instance.Translate(text, "ModulePartNames." + text.Replace(" ", "_"));
        }

        // called at start up to set up translation of module names
        static private void TranslateModules()
        {
            foreach (var kvp in shipmodules)
            {
                ShipModule sm = kvp.Value;

                // this logic breaks down the 

                if (kvp.Key.Contains("_armour_", StringComparison.InvariantCulture))
                {
                    string[] armourdelim = new string[] { "Lightweight", "Reinforced", "Military", "Mirrored", "Reactive" };
                    int index = sm.EnglishModName.IndexOf(armourdelim, out int anum, StringComparison.InvariantCulture);
                    string translated = TXIT(sm.EnglishModName.Substring(index));
                    sm.TranslatedModName = sm.EnglishModName.Substring(0, index) + translated;
                }
                else
                {
                    int cindex = sm.EnglishModName.IndexOf(" Class ", StringComparison.InvariantCulture);
                    int rindex = sm.EnglishModName.IndexOf(" Rating ", StringComparison.InvariantCulture);

                    if (cindex != -1 && rindex != -1)
                    {
                        string translated = TXIT(sm.EnglishModName.Substring(0, cindex));
                        string cls = TXIT(sm.EnglishModName.Substring(cindex + 1, 5));
                        string rat = TXIT(sm.EnglishModName.Substring(rindex + 1, 6));
                        sm.TranslatedModName = translated + " " + cls + " " + sm.EnglishModName.Substring(cindex + 7, 1) + " " + rat + " " + sm.EnglishModName.Substring(rindex + 8, 1);
                    }
                    else if (cindex != -1)
                    {
                        string translated = TXIT(sm.EnglishModName.Substring(0, cindex));
                        string cls = TXIT(sm.EnglishModName.Substring(cindex + 1, 5));
                        sm.TranslatedModName = translated + " " + cls + " " + sm.EnglishModName.Substring(cindex + 7, 1);
                    }
                    else if (rindex != -1)
                    {
                        string translated = TXIT(sm.EnglishModName.Substring(0, rindex));
                        string rat = TXIT(sm.EnglishModName.Substring(rindex + 1, 6));
                        sm.TranslatedModName = translated + " " + rat + " " + sm.EnglishModName.Substring(rindex + 8, 1);
                    }
                    else
                    {
                        string[] sizes = new string[] { " Small", " Medium", " Large", " Huge", " Tiny", " Standard", " Intermediate", " Advanced" };
                        int sindex = sm.EnglishModName.IndexOf(sizes, out int snum, StringComparison.InvariantCulture);

                        if (sindex >= 0)
                        {
                            string[] types = new string[] { " Gimbal ", " Fixed ", " Turret " };
                            int gindex = sm.EnglishModName.IndexOf(types, out int gnum, StringComparison.InvariantCulture);

                            if (gindex >= 0)
                            {
                                string translated = TXIT(sm.EnglishModName.Substring(0, gindex));
                                string typen = TXIT(sm.EnglishModName.Substring(gindex + 1, types[gnum].Length - 2));
                                string sizen = TXIT(sm.EnglishModName.Substring(sindex + 1, sizes[snum].Length - 1));
                                sm.TranslatedModName = translated + " " + typen + " " + sizen;
                            }
                            else
                            {
                                string translated = TXIT(sm.EnglishModName.Substring(0, sindex));
                                string sizen = TXIT(sm.EnglishModName.Substring(sindex + 1, sizes[snum].Length - 1));
                                sm.TranslatedModName = translated + " " + sizen;
                            }
                        }
                        else
                        {
                            sm.TranslatedModName = TXIT(sm.EnglishModName);
                            //System.Diagnostics.Debug.WriteLine($"?? {kvp.Key} = {sm.ModName}");
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine($"Module {sm.ModName} : {sm.ModType} => {sm.TranslatedModName} : {sm.TranslatedModTypeString}");
            }
        }

        #region ShipModule

        public class ShipModule : IModuleInfo
        {
            public enum ModuleTypes
            {
                // Aligned with spansh, spansh is aligned with outfitting.csv on EDCD.
                // all buyable

                AXMissileRack,
                AXMulti_Cannon,
                AbrasionBlaster,
                AdvancedDockingComputer,
                AdvancedMissileRack,
                AdvancedMulti_Cannon,
                AdvancedPlanetaryApproachSuite,
                AdvancedPlasmaAccelerator,
                AutoField_MaintenanceUnit,
                BeamLaser,
                Bi_WeaveShieldGenerator,
                BurstLaser,
                BusinessClassPassengerCabin,
                Cannon,
                CargoRack,
                CargoScanner,
                CausticSinkLauncher,
                ChaffLauncher,
                CollectorLimpetController,
                CorrosionResistantCargoRack,
                CytoscramblerBurstLaser,
                DecontaminationLimpetController,
                DetailedSurfaceScanner,
                EconomyClassPassengerCabin,
                ElectronicCountermeasure,
                EnforcerCannon,
                EnhancedAXMissileRack,
                EnhancedAXMulti_Cannon,
                EnhancedPerformanceThrusters,
                EnhancedXenoScanner,
                EnzymeMissileRack,
                ExperimentalWeaponStabiliser,
                FighterHangar,
                FirstClassPassengerCabin,
                FragmentCannon,
                FrameShiftDrive,
                FrameShiftDriveInterdictor,
                FrameShiftWakeScanner,
                FuelScoop,
                FuelTank,
                FuelTransferLimpetController,
                GuardianFSDBooster,
                GuardianGaussCannon,
                GuardianHullReinforcement,
                GuardianHybridPowerDistributor,
                GuardianHybridPowerPlant,
                GuardianModuleReinforcement,
                GuardianPlasmaCharger,
                GuardianShardCannon,
                GuardianShieldReinforcement,
                HatchBreakerLimpetController,
                HeatSinkLauncher,
                HullReinforcementPackage,
                ImperialHammerRailGun,
                KillWarrantScanner,
                LifeSupport,
                LightweightAlloy,
                ////LimpetControl,
                LuxuryClassPassengerCabin,
                MetaAlloyHullReinforcement,
                MilitaryGradeComposite,
                MineLauncher,
                MiningLance,
                MiningLaser,
                MiningMultiLimpetController,
                MirroredSurfaceComposite,
                MissileRack,
                ModuleReinforcementPackage,
                Multi_Cannon,
                OperationsMultiLimpetController,
                PacifierFrag_Cannon,
                Pack_HoundMissileRack,
                PlanetaryApproachSuite,
                PlanetaryVehicleHangar,
                PlasmaAccelerator,
                PointDefence,
                PowerDistributor,
                PowerPlant,
                PrismaticShieldGenerator,
                ProspectorLimpetController,
                PulseDisruptorLaser,
                PulseLaser,
                PulseWaveAnalyser,
                RailGun,
                ReactiveSurfaceComposite,
                ReconLimpetController,
                Refinery,
                ReinforcedAlloy,
                RemoteReleaseFlakLauncher,
                RemoteReleaseFlechetteLauncher,
                RepairLimpetController,
                RescueMultiLimpetController,
                ResearchLimpetController,
                RetributorBeamLaser,
                RocketPropelledFSDDisruptor,
                SeekerMissileRack,
                SeismicChargeLauncher,
                Sensors,
                ShieldBooster,
                ShieldCellBank,
                ShieldGenerator,
                ShockCannon,
                ShockMineLauncher,
                ShutdownFieldNeutraliser,
                StandardDockingComputer,
                Sub_SurfaceDisplacementMissile,
                SupercruiseAssist,
                Thrusters,
                TorpedoPylon,
                UniversalMultiLimpetController,
                XenoMultiLimpetController,
                XenoScanner,

                // Not buyable, DiscoveryScanner marks the first non buyable
                DiscoveryScanner, PrisonCells, DataLinkScanner, SRVScanner, FighterWeapon,
                VanityType, UnknownType, CockpitType, CargoBayDoorType, WearAndTearType, Codex,
            };

            public string EnglishModName { get; set; }     // english name
            public string TranslatedModName { get; set; }     // foreign name
            public int ModuleID { get; set; }
            public ModuleTypes ModType { get; set; }
            public bool IsBuyable { get { return !(ModType < ModuleTypes.DiscoveryScanner); } }

            // string should be in spansh/EDCD csv compatible format, in english, as it it fed into Spansh
            public string EnglishModTypeString { get { return ModType.ToString().Replace("AX", "AX ").Replace("_", "-").SplitCapsWordFull(); } }
            public string TranslatedModTypeString { get { return BaseUtils.Translator.Instance.Translate(EnglishModTypeString, "ModuleTypeNames." + EnglishModTypeString.Replace(" ", "_")); } }     // string should be in spansh/EDCD csv compatible format, in english

            // printed in this order

            public int? Class { get; set; }     // handled specifically
            public string Rating { get; set; }
            [PropertyNameAttribute("t")] public double? Mass { get; set; }        // mass of module t
            [PropertyNameAttribute("MW")] public double? Power { get; set; }       // power used by module MW
            [PropertyNameAttribute("")] public string Mount { get; set; }
            [PropertyNameAttribute("")] public int? Ammo { get; set; }
            [PropertyNameAttribute("")] public int? Clip { get; set; }
            [PropertyNameAttribute("")] public string MissileType { get; set; }
            [PropertyNameAttribute("")] public string CabinClass { get; set; }
            [PropertyNameAttribute("")] public double? Integrity { get; set; }  // units
            [PropertyNameAttribute("")] public double? Damage { get; set; }
            [PropertyNameAttribute("/s")] public double? DPS { get; set; } // /s
            [PropertyNameAttribute("s")] public double? ReloadTime { get; set; } // s
            [PropertyNameAttribute("%")] public double? Explosive { get; set; } //% bonus on base values
            [PropertyNameAttribute("%")] public double? Kinetic { get; set; } //% bonus on base values
            [PropertyNameAttribute("%")] public double? Thermal { get; set; } //% bonus on base values
            [PropertyNameAttribute("")] public double? ThermL { get; set; }
            [PropertyNameAttribute("%")] public double? HullStrengthBonus { get; set; }  // % bonus over the ship information armour value
            [PropertyNameAttribute("%")] public double? CausticReinforcement { get; set; } //%
            [PropertyNameAttribute("")] public double? HullReinforcement { get; set; } // units
            [PropertyNameAttribute("")] public double? ShieldReinforcement { get; set; } // units
            [PropertyNameAttribute("%")] public double? AXResistance { get; set; } //% bonus on base values
            [PropertyNameAttribute("/s")] public double? RegenRate { get; set; } // units/s
            [PropertyNameAttribute("/s")] public double? BrokenRegenRate { get; set; } // units/s
            [PropertyNameAttribute("")] public double? MinStrength { get; set; } // shields
            [PropertyNameAttribute("")] public double? OptStrength { get; set; } // shields
            [PropertyNameAttribute("")] public double? MaxStrength { get; set; } // shields
            [PropertyNameAttribute("")] public double? AdditionalStrength { get; set; } // shields additional strength
            [PropertyNameAttribute("m")] public double? TypicalEmission { get; set; } //m sensors
            [PropertyNameAttribute("%")] public double? HeatEfficiency { get; set; } //% power plants
            [PropertyNameAttribute("")] public int? MaxCargo { get; set; } // hatch breaker limpet
            [PropertyNameAttribute("")] public int? MinCargo { get; set; } // hatch breaker limpet
            [PropertyNameAttribute("s")] public int? HackTime { get; set; }// hatch breaker limpet
            [PropertyNameAttribute("")] public int? Limpets { get; set; }// collector controllers
            [PropertyNameAttribute("deg")] public double? Angle { get; set; } // angle
            [PropertyNameAttribute("m")] public int? Range { get; set; } // m
            [PropertyNameAttribute("m")] public int? Falloff { get; set; } // m weapon fall off distance
            [PropertyNameAttribute("m")] public int? TargetRange { get; set; } // m w
            [PropertyNameAttribute("s")] public double? BurstInterval { get; set; } // s weapon
            [PropertyNameAttribute("/s")] public double? RateOfFire { get; set; } // s weapon
            [PropertyNameAttribute("/shot")] public double? Rounds { get; set; } 
            [PropertyNameAttribute("deg")] public double? Jitter { get; set; } 
            [PropertyNameAttribute("m/s")] public double? Speed { get; set; } // m/s
            [PropertyNameAttribute("")] public double? Protection { get; set; } // multiplier
            [PropertyNameAttribute("MW/s")] public double? SysMW { get; set; } // power distributor rate MW/s
            [PropertyNameAttribute("MW/s")] public double? EngMW { get; set; } // power distributor rate MW/s
            [PropertyNameAttribute("MW/s")] public double? WepMW { get; set; } // power distributor rate MW/s
            [PropertyNameAttribute("MW")] public double? SysCap { get; set; } // max MW power distributor
            [PropertyNameAttribute("MW")] public double? EngCap { get; set; } // max MW power distributor
            [PropertyNameAttribute("MW")] public double? WepCap { get; set; } // max MW power distributor
            [PropertyNameAttribute("%")] public double? PowerBonus { get; set; } // guardian power bonus
            [PropertyNameAttribute("MW")] public double? PowerGen { get; set; } // MW power plant
            [PropertyNameAttribute("MW/u")] public double? MWPerUnit { get; set; } // MW per shield unit
            [PropertyNameAttribute("MW/s")] public double? MWPerSec { get; set; } // MW per sec
            [PropertyNameAttribute("t")] public int? OptMass { get; set; } // t
            [PropertyNameAttribute("t")] public int? MaxMass { get; set; } // t
            [PropertyNameAttribute("t")] public int? MinMass { get; set; } // t
            [PropertyNameAttribute("")] public double? EngineOptMultiplier { get; set; }
            [PropertyNameAttribute("")] public double? EngineMinMultiplier { get; set; }
            [PropertyNameAttribute("")] public double? EngineMaxMultiplier { get; set; }
            [PropertyNameAttribute("s")] public double? SCBSpinUp { get; set; }
            [PropertyNameAttribute("s")] public double? SCBDuration { get; set; }

            [PropertyNameAttribute("")] public int? Prisoners { get; set; }
            [PropertyNameAttribute("")] public int? Passengers { get; set; }
            [PropertyNameAttribute("")] public int? Bins { get; set; }
            [PropertyNameAttribute("t")] public int? Size { get; set; } // tons
            [PropertyNameAttribute("/s")] public double? RefillRate { get; set; } // t/s
            [PropertyNameAttribute("s")] public double? Time { get; set; } // s
            [PropertyNameAttribute("")] public int? Rebuilds { get; set; } // number

            [PropertyNameAttribute("")] public double? PowerConstant { get; set; } // Number
            [PropertyNameAttribute("")] public double? LinearConstant { get; set; } // Number
            [PropertyNameAttribute("t")] public double? MaxFuelPerJump { get; set; } // t

            [PropertyNameAttribute("%")] public double? SCOSpeedIncrease { get; set; }  // %
            [PropertyNameAttribute("")] public double? SCOAccelerationRate { get; set; }  // factor
            [PropertyNameAttribute("")] public double? SCOHeatGenerationRate { get; set; }  // factor
            [PropertyNameAttribute("")] public double? SCOControlInterference { get; set; }  // factor

            [PropertyNameAttribute("ly")] public double? AdditionalRange { get; set; } // ly

            [PropertyNameAttribute("")] public double? SCBHeat { get; set; } // waste heat

            [PropertyNameAttribute("s")] public double? BootTime { get; set; } // seconds
            [PropertyNameAttribute("u/s")] public double? RateOfRepairConsumption { get; set; }
            [PropertyNameAttribute("i/m")] public double? RepairCostPerMat { get; set; }

            [PropertyNameAttribute("shots/s")] public double? BurstRateOfFire { get; set; }
            [PropertyNameAttribute("shots")] public double? BurstSize { get; set; }
            [PropertyNameAttribute("")] public double? BreachDamage { get; set; }
            [PropertyNameAttribute("%/FullI")] public double? BreachMin { get; set; }
            [PropertyNameAttribute("%/ZeroI")] public double? BreachMax { get; set; }
            [PropertyNameAttribute("%")] public double? ProportionThermal { get; set; }
            [PropertyNameAttribute("%")] public double? ProportionKinetic { get; set; }
            [PropertyNameAttribute("")] public double? Pierce { get; set; }
            [PropertyNameAttribute("MW/Shot_or_s")] public double? DistributorDraw { get; set; }
            [PropertyNameAttribute("%")] public double? ExplosiveProportion { get; set; }
            [PropertyNameAttribute("%")] public double? KineticProportion { get; set; }
            [PropertyNameAttribute("%")] public double? ThermalProportion { get; set; }
            [PropertyNameAttribute("%")] public double? AbsolutePortionDamage { get; set; }
            [PropertyNameAttribute("%")] public double? CausticPortionDamage { get; set; }
            [PropertyNameAttribute("%")] public double? AXPortionDamage { get; set; }

            [PropertyNameAttribute("cr")] public int? Cost { get; set; }

            [PropertyNameAttribute("")] public int? AmmoCost { get; set; }
            [PropertyNameAttribute("")] public int? WasteHeat { get; set; }
            [PropertyNameAttribute("MW/use")] public int? ActivePower { get; set; }
            [PropertyNameAttribute("s")] public int? TargetMaxTime { get; set; }

            [PropertyNameAttribute("")] public int? DamageMultiplierFulLCharge { get; set; }
            [PropertyNameAttribute("t")] public int? FuelTransfer { get; set; }
            [PropertyNameAttribute("m/s")] public int? MultiTargetSpeed { get; set; }
            [PropertyNameAttribute("")] public int? MaxRepairMaterialCapacity { get; set; }
            [PropertyNameAttribute("")] public double? MineBonus { get; set; }

            [PropertyNameAttribute("")] public double? MinimumSpeedModifier { get; set; }
            [PropertyNameAttribute("")] public double? OptimalSpeedModifier { get; set; }
            [PropertyNameAttribute("")] public double? MaximumSpeedModifier { get; set; }
            [PropertyNameAttribute("")] public double? MinimumAccelerationModifier { get; set; }
            [PropertyNameAttribute("")] public double? OptimalAccelerationModifier { get; set; }
            [PropertyNameAttribute("")] public double? MaximumAccelerationModifier { get; set; }
            [PropertyNameAttribute("")] public double? MinimumRotationModifier { get; set; }
            [PropertyNameAttribute("")] public double? OptimumRotationModifier { get; set; }
            [PropertyNameAttribute("")] public double? MaximumRotationModifier { get; set; }

            public ShipModule()
            {
            }

            public ShipModule(ShipModule other)
            {
                // cheap way of doing a copy constructor without listing all those effing attributes

                foreach (System.Reflection.PropertyInfo pi in typeof(ShipModule).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if ( pi.CanWrite)
                        pi.SetValue(this, pi.GetValue(other));
                }
            }

            public string PropertiesAsText()
            {
                StringBuilder output = new StringBuilder();
                if (Class != null && Rating != null)
                    output.Append($"{Class}{Rating}");
                else if ( Rating != null || Class != null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                foreach (System.Reflection.PropertyInfo pi in typeof(ShipModule).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var ci = pi.GetCustomAttributes(typeof(PropertyNameAttribute), true);
                    if ( ci.Length > 0 )    // must have a PNA
                    {
                        string postfix = ((dynamic)ci[0]).Text; // note dynamic
                        dynamic value = pi.GetValue(this);
                        if ( value != null )        // if not null, print value
                        {
                            if (output.Length > 0)
                                output.Append(", ");
                            output.Append($"{pi.Name}={value:0.###}{postfix}");
                        }
                    }
                }

                return output.ToString();
            }

            public EliteDangerousCalculations.FSDSpec GetFSDSpec()
            {
                System.Diagnostics.Debug.Assert(LinearConstant != null && PowerConstant != null && MaxFuelPerJump != null && OptMass != null);
                var fsd = new EliteDangerousCalculations.FSDSpec(PowerConstant.Value, LinearConstant.Value, OptMass.Value, MaxFuelPerJump.Value);
                return fsd;
            }
            public ShipModule(int id, ModuleTypes modtype, string descr)
            {
                ModuleID = id; TranslatedModName = EnglishModName = descr; ModType = modtype;
            }

        }


    #endregion

        static private Dictionary<string, ShipModule> shipmodules;
        static private Dictionary<string, ShipModule> synthesisedmodules = new Dictionary<string, ShipModule>();
        static private Dictionary<string, ShipModule> vanitymodules;
        static private Dictionary<string, ShipModule> srvmodules;
        static private Dictionary<string, ShipModule> fightermodules;
        static private Dictionary<string, ShipModule> othershipmodules;

        static void CreateModules()
        {
            System.Diagnostics.Debug.WriteLine("Creating mods");

            shipmodules = new Dictionary<string, ShipModule>
            {
                // Armour, in ID order
                // Raw Value of armour = ship.armour * (1+HullStrengthBonus/100)
                // Kinetic resistance = Raw * (1+Kinteic/100)
                // Explosive resistance = Raw * (1+Explosive/100)
                // Thermal resistance = Raw * (1+Thermal/100)
                // AX = Raw * (1+AXResistance/100) TBD

                { "sidewinder_armour_grade1", new ShipModule(128049250,ShipModule.ModuleTypes.LightweightAlloy,"Sidewinder Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "sidewinder_armour_grade2", new ShipModule(128049251,ShipModule.ModuleTypes.ReinforcedAlloy,"Sidewinder Reinforced Alloy") { Mass=2, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "sidewinder_armour_grade3", new ShipModule(128049252,ShipModule.ModuleTypes.MilitaryGradeComposite,"Sidewinder Military Grade Composite") { Mass=4, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "sidewinder_armour_mirrored", new ShipModule(128049253,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Sidewinder Mirrored Surface Composite") { Mass=4, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "sidewinder_armour_reactive", new ShipModule(128049254,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Sidewinder Reactive Surface Composite") { Mass=4, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "eagle_armour_grade1", new ShipModule(128049256,ShipModule.ModuleTypes.LightweightAlloy,"Eagle Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "eagle_armour_grade2", new ShipModule(128049257,ShipModule.ModuleTypes.ReinforcedAlloy,"Eagle Reinforced Alloy") { Mass=4, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "eagle_armour_grade3", new ShipModule(128049258,ShipModule.ModuleTypes.MilitaryGradeComposite,"Eagle Military Grade Composite") { Mass=8, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "eagle_armour_mirrored", new ShipModule(128049259,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Eagle Mirrored Surface Composite") { Mass=8, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "eagle_armour_reactive", new ShipModule(128049260,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Eagle Reactive Surface Composite") { Mass=8, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "hauler_armour_grade1", new ShipModule(128049262,ShipModule.ModuleTypes.LightweightAlloy,"Hauler Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "hauler_armour_grade2", new ShipModule(128049263,ShipModule.ModuleTypes.ReinforcedAlloy,"Hauler Reinforced Alloy") { Mass=1, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "hauler_armour_grade3", new ShipModule(128049264,ShipModule.ModuleTypes.MilitaryGradeComposite,"Hauler Military Grade Composite") { Mass=2, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "hauler_armour_mirrored", new ShipModule(128049265,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Hauler Mirrored Surface Composite") { Mass=2, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "hauler_armour_reactive", new ShipModule(128049266,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Hauler Reactive Surface Composite") { Mass=2, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "adder_armour_grade1", new ShipModule(128049268,ShipModule.ModuleTypes.LightweightAlloy,"Adder Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "adder_armour_grade2", new ShipModule(128049269,ShipModule.ModuleTypes.ReinforcedAlloy,"Adder Reinforced Alloy") { Mass=3, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "adder_armour_grade3", new ShipModule(128049270,ShipModule.ModuleTypes.MilitaryGradeComposite,"Adder Military Grade Composite") { Mass=5, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "adder_armour_mirrored", new ShipModule(128049271,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Adder Mirrored Surface Composite") { Mass=5, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "adder_armour_reactive", new ShipModule(128049272,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Adder Reactive Surface Composite") { Mass=5, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "viper_armour_grade1", new ShipModule(128049274,ShipModule.ModuleTypes.LightweightAlloy,"Viper Mk III Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "viper_armour_grade2", new ShipModule(128049275,ShipModule.ModuleTypes.ReinforcedAlloy,"Viper Mk III Reinforced Alloy") { Mass=5, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "viper_armour_grade3", new ShipModule(128049276,ShipModule.ModuleTypes.MilitaryGradeComposite,"Viper Mk III Military Grade Composite") { Mass=9, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "viper_armour_mirrored", new ShipModule(128049277,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Viper Mk III Mirrored Surface Composite") { Mass=9, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "viper_armour_reactive", new ShipModule(128049278,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Viper Mk III Reactive Surface Composite") { Mass=9, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "cobramkiii_armour_grade1", new ShipModule(128049280,ShipModule.ModuleTypes.LightweightAlloy,"Cobra Mk III Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "cobramkiii_armour_grade2", new ShipModule(128049281,ShipModule.ModuleTypes.ReinforcedAlloy,"Cobra Mk III Reinforced Alloy") { Mass=14, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "cobramkiii_armour_grade3", new ShipModule(128049282,ShipModule.ModuleTypes.MilitaryGradeComposite,"Cobra Mk III Military Grade Composite") { Mass=27, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "cobramkiii_armour_mirrored", new ShipModule(128049283,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Cobra Mk III Mirrored Surface Composite") { Mass=27, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "cobramkiii_armour_reactive", new ShipModule(128049284,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Cobra Mk III Reactive Surface Composite") { Mass=27, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "type6_armour_grade1", new ShipModule(128049286,ShipModule.ModuleTypes.LightweightAlloy,"Type-6 Transporter Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "type6_armour_grade2", new ShipModule(128049287,ShipModule.ModuleTypes.ReinforcedAlloy,"Type-6 Transporter Reinforced Alloy") { Mass=12, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "type6_armour_grade3", new ShipModule(128049288,ShipModule.ModuleTypes.MilitaryGradeComposite,"Type-6 Transporter Military Grade Composite") { Mass=23, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "type6_armour_mirrored", new ShipModule(128049289,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Type-6 Transporter Mirrored Surface Composite") { Mass=23, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "type6_armour_reactive", new ShipModule(128049290,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Type-6 Transporter Reactive Surface Composite") { Mass=23, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "dolphin_armour_grade1", new ShipModule(128049292,ShipModule.ModuleTypes.LightweightAlloy,"Dolphin Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "dolphin_armour_grade2", new ShipModule(128049293,ShipModule.ModuleTypes.ReinforcedAlloy,"Dolphin Reinforced Alloy") { Mass=32, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "dolphin_armour_grade3", new ShipModule(128049294,ShipModule.ModuleTypes.MilitaryGradeComposite,"Dolphin Military Grade Composite") { Mass=63, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "dolphin_armour_mirrored", new ShipModule(128049295,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Dolphin Mirrored Surface Composite") { Mass=63, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "dolphin_armour_reactive", new ShipModule(128049296,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Dolphin Reactive Surface Composite") { Mass=63, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "type7_armour_grade1", new ShipModule(128049298,ShipModule.ModuleTypes.LightweightAlloy,"Type-7 Transporter Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "type7_armour_grade2", new ShipModule(128049299,ShipModule.ModuleTypes.ReinforcedAlloy,"Type-7 Transporter Reinforced Alloy") { Mass=32, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "type7_armour_grade3", new ShipModule(128049300,ShipModule.ModuleTypes.MilitaryGradeComposite,"Type-7 Transporter Military Grade Composite") { Mass=63, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "type7_armour_mirrored", new ShipModule(128049301,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Type-7 Transporter Mirrored Surface Composite") { Mass=63, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "type7_armour_reactive", new ShipModule(128049302,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Type-7 Transporter Reactive Surface Composite") { Mass=63, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "asp_armour_grade1", new ShipModule(128049304,ShipModule.ModuleTypes.LightweightAlloy,"Asp Explorer Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "asp_armour_grade2", new ShipModule(128049305,ShipModule.ModuleTypes.ReinforcedAlloy,"Asp Explorer Reinforced Alloy") { Mass=21, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "asp_armour_grade3", new ShipModule(128049306,ShipModule.ModuleTypes.MilitaryGradeComposite,"Asp Explorer Military Grade Composite") { Mass=42, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "asp_armour_mirrored", new ShipModule(128049307,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Asp Explorer Mirrored Surface Composite") { Mass=42, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "asp_armour_reactive", new ShipModule(128049308,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Asp Explorer Reactive Surface Composite") { Mass=42, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "vulture_armour_grade1", new ShipModule(128049310,ShipModule.ModuleTypes.LightweightAlloy,"Vulture Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "vulture_armour_grade2", new ShipModule(128049311,ShipModule.ModuleTypes.ReinforcedAlloy,"Vulture Reinforced Alloy") { Mass=17, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "vulture_armour_grade3", new ShipModule(128049312,ShipModule.ModuleTypes.MilitaryGradeComposite,"Vulture Military Grade Composite") { Mass=35, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "vulture_armour_mirrored", new ShipModule(128049313,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Vulture Mirrored Surface Composite") { Mass=35, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "vulture_armour_reactive", new ShipModule(128049314,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Vulture Reactive Surface Composite") { Mass=35, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "empire_trader_armour_grade1", new ShipModule(128049316,ShipModule.ModuleTypes.LightweightAlloy,"Imperial Clipper Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "empire_trader_armour_grade2", new ShipModule(128049317,ShipModule.ModuleTypes.ReinforcedAlloy,"Imperial Clipper Reinforced Alloy") { Mass=30, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "empire_trader_armour_grade3", new ShipModule(128049318,ShipModule.ModuleTypes.MilitaryGradeComposite,"Imperial Clipper Military Grade Composite") { Mass=60, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_trader_armour_mirrored", new ShipModule(128049319,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Imperial Clipper Mirrored Surface Composite") { Mass=60, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_trader_armour_reactive", new ShipModule(128049320,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Imperial Clipper Reactive Surface Composite") { Mass=60, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "federation_dropship_armour_grade1", new ShipModule(128049322,ShipModule.ModuleTypes.LightweightAlloy,"Federal Dropship Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "federation_dropship_armour_grade2", new ShipModule(128049323,ShipModule.ModuleTypes.ReinforcedAlloy,"Federal Dropship Reinforced Alloy") { Mass=44, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "federation_dropship_armour_grade3", new ShipModule(128049324,ShipModule.ModuleTypes.MilitaryGradeComposite,"Federal Dropship Military Grade Composite") { Mass=87, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_dropship_armour_mirrored", new ShipModule(128049325,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Federal Dropship Mirrored Surface Composite") { Mass=87, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_dropship_armour_reactive", new ShipModule(128049326,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Federal Dropship Reactive Surface Composite") { Mass=87, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "orca_armour_grade1", new ShipModule(128049328,ShipModule.ModuleTypes.LightweightAlloy,"Orca Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "orca_armour_grade2", new ShipModule(128049329,ShipModule.ModuleTypes.ReinforcedAlloy,"Orca Reinforced Alloy") { Mass=21, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "orca_armour_grade3", new ShipModule(128049330,ShipModule.ModuleTypes.MilitaryGradeComposite,"Orca Military Grade Composite") { Mass=87, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "orca_armour_mirrored", new ShipModule(128049331,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Orca Mirrored Surface Composite") { Mass=87, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "orca_armour_reactive", new ShipModule(128049332,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Orca Reactive Surface Composite") { Mass=87, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "type9_armour_grade1", new ShipModule(128049334,ShipModule.ModuleTypes.LightweightAlloy,"Type-9 Heavy Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "type9_armour_grade2", new ShipModule(128049335,ShipModule.ModuleTypes.ReinforcedAlloy,"Type-9 Heavy Reinforced Alloy") { Mass=75, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "type9_armour_grade3", new ShipModule(128049336,ShipModule.ModuleTypes.MilitaryGradeComposite,"Type-9 Heavy Military Grade Composite") { Mass=150, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "type9_armour_mirrored", new ShipModule(128049337,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Type-9 Heavy Mirrored Surface Composite") { Mass=150, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "type9_armour_reactive", new ShipModule(128049338,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Type-9 Heavy Reactive Surface Composite") { Mass=150, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "python_armour_grade1", new ShipModule(128049340,ShipModule.ModuleTypes.LightweightAlloy,"Python Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "python_armour_grade2", new ShipModule(128049341,ShipModule.ModuleTypes.ReinforcedAlloy,"Python Reinforced Alloy") { Mass=26, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "python_armour_grade3", new ShipModule(128049342,ShipModule.ModuleTypes.MilitaryGradeComposite,"Python Military Grade Composite") { Mass=53, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "python_armour_mirrored", new ShipModule(128049343,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Python Mirrored Surface Composite") { Mass=53, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "python_armour_reactive", new ShipModule(128049344,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Python Reactive Surface Composite") { Mass=53, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "belugaliner_armour_grade1", new ShipModule(128049346,ShipModule.ModuleTypes.LightweightAlloy,"Beluga Liner Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "belugaliner_armour_grade2", new ShipModule(128049347,ShipModule.ModuleTypes.ReinforcedAlloy,"Beluga Liner Reinforced Alloy") { Mass=83, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "belugaliner_armour_grade3", new ShipModule(128049348,ShipModule.ModuleTypes.MilitaryGradeComposite,"Beluga Liner Military Grade Composite") { Mass=165, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "belugaliner_armour_mirrored", new ShipModule(128049349,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Beluga Liner Mirrored Surface Composite") { Mass=165, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "belugaliner_armour_reactive", new ShipModule(128049350,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Beluga Liner Reactive Surface Composite") { Mass=165, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "ferdelance_armour_grade1", new ShipModule(128049352,ShipModule.ModuleTypes.LightweightAlloy,"Fer-de-Lance Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "ferdelance_armour_grade2", new ShipModule(128049353,ShipModule.ModuleTypes.ReinforcedAlloy,"Fer-de-Lance Reinforced Alloy") { Mass=19, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "ferdelance_armour_grade3", new ShipModule(128049354,ShipModule.ModuleTypes.MilitaryGradeComposite,"Fer-de-Lance Military Grade Composite") { Mass=38, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "ferdelance_armour_mirrored", new ShipModule(128049355,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Fer-de-Lance Mirrored Surface Composite") { Mass=38, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "ferdelance_armour_reactive", new ShipModule(128049356,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Fer-de-Lance Reactive Surface Composite") { Mass=38, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "anaconda_armour_grade1", new ShipModule(128049364,ShipModule.ModuleTypes.LightweightAlloy,"Anaconda Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "anaconda_armour_grade2", new ShipModule(128049365,ShipModule.ModuleTypes.ReinforcedAlloy,"Anaconda Reinforced Alloy") { Mass=30, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "anaconda_armour_grade3", new ShipModule(128049366,ShipModule.ModuleTypes.MilitaryGradeComposite,"Anaconda Military Grade Composite") { Mass=60, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "anaconda_armour_mirrored", new ShipModule(128049367,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Anaconda Mirrored Surface Composite") { Mass=60, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "anaconda_armour_reactive", new ShipModule(128049368,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Anaconda Reactive Surface Composite") { Mass=60, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "federation_corvette_armour_grade1", new ShipModule(128049370,ShipModule.ModuleTypes.LightweightAlloy,"Federal Corvette Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "federation_corvette_armour_grade2", new ShipModule(128049371,ShipModule.ModuleTypes.ReinforcedAlloy,"Federal Corvette Reinforced Alloy") { Mass=30, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "federation_corvette_armour_grade3", new ShipModule(128049372,ShipModule.ModuleTypes.MilitaryGradeComposite,"Federal Corvette Military Grade Composite") { Mass=60, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_corvette_armour_mirrored", new ShipModule(128049373,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Federal Corvette Mirrored Surface Composite") { Mass=60, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_corvette_armour_reactive", new ShipModule(128049374,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Federal Corvette Reactive Surface Composite") { Mass=60, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "cutter_armour_grade1", new ShipModule(128049376,ShipModule.ModuleTypes.LightweightAlloy,"Imperial Cutter Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "cutter_armour_grade2", new ShipModule(128049377,ShipModule.ModuleTypes.ReinforcedAlloy,"Imperial Cutter Reinforced Alloy") { Mass=30, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "cutter_armour_grade3", new ShipModule(128049378,ShipModule.ModuleTypes.MilitaryGradeComposite,"Imperial Cutter Military Grade Composite") { Mass=60, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "cutter_armour_mirrored", new ShipModule(128049379,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Imperial Cutter Mirrored Surface Composite") { Mass=60, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "cutter_armour_reactive", new ShipModule(128049380,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Imperial Cutter Reactive Surface Composite") { Mass=60, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "diamondbackxl_armour_grade1", new ShipModule(128671832,ShipModule.ModuleTypes.LightweightAlloy,"Diamondback Explorer Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "diamondbackxl_armour_grade2", new ShipModule(128671833,ShipModule.ModuleTypes.ReinforcedAlloy,"Diamondback Explorer Reinforced Alloy") { Mass=23, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "diamondbackxl_armour_grade3", new ShipModule(128671834,ShipModule.ModuleTypes.MilitaryGradeComposite,"Diamondback Explorer Military Grade Composite") { Mass=47, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "diamondbackxl_armour_mirrored", new ShipModule(128671835,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Diamondback Explorer Mirrored Surface Composite") { Mass=47, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "diamondbackxl_armour_reactive", new ShipModule(128671836,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Diamondback Explorer Reactive Surface Composite") { Mass=47, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },


                { "empire_eagle_armour_grade1", new ShipModule(128672140,ShipModule.ModuleTypes.LightweightAlloy,"Imperial Eagle Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "empire_eagle_armour_grade2", new ShipModule(128672141,ShipModule.ModuleTypes.ReinforcedAlloy,"Imperial Eagle Reinforced Alloy") { Mass=4, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "empire_eagle_armour_grade3", new ShipModule(128672142,ShipModule.ModuleTypes.MilitaryGradeComposite,"Imperial Eagle Military Grade Composite") { Mass=8, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_eagle_armour_mirrored", new ShipModule(128672143,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Imperial Eagle Mirrored Surface Composite") { Mass=8, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_eagle_armour_reactive", new ShipModule(128672144,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Imperial Eagle Reactive Surface Composite") { Mass=8, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "federation_dropship_mkii_armour_grade1", new ShipModule(128672147,ShipModule.ModuleTypes.LightweightAlloy,"Federal Assault Ship Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "federation_dropship_mkii_armour_grade2", new ShipModule(128672148,ShipModule.ModuleTypes.ReinforcedAlloy,"Federal Assault Ship Reinforced Alloy") { Mass=44, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "federation_dropship_mkii_armour_grade3", new ShipModule(128672149,ShipModule.ModuleTypes.MilitaryGradeComposite,"Federal Assault Ship Military Grade Composite") { Mass=87, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_dropship_mkii_armour_mirrored", new ShipModule(128672150,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Federal Assault Ship Mirrored Surface Composite") { Mass=87, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_dropship_mkii_armour_reactive", new ShipModule(128672151,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Federal Assault Ship Reactive Surface Composite") { Mass=87, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "federation_gunship_armour_grade1", new ShipModule(128672154,ShipModule.ModuleTypes.LightweightAlloy,"Federal Gunship Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "federation_gunship_armour_grade2", new ShipModule(128672155,ShipModule.ModuleTypes.ReinforcedAlloy,"Federal Gunship Reinforced Alloy") { Mass=44, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "federation_gunship_armour_grade3", new ShipModule(128672156,ShipModule.ModuleTypes.MilitaryGradeComposite,"Federal Gunship Military Grade Composite") { Mass=87, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_gunship_armour_mirrored", new ShipModule(128672157,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Federal Gunship Mirrored Surface Composite") { Mass=87, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "federation_gunship_armour_reactive", new ShipModule(128672158,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Federal Gunship Reactive Surface Composite") { Mass=87, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "viper_mkiv_armour_grade1", new ShipModule(128672257,ShipModule.ModuleTypes.LightweightAlloy,"Viper Mk IV Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "viper_mkiv_armour_grade2", new ShipModule(128672258,ShipModule.ModuleTypes.ReinforcedAlloy,"Viper Mk IV Reinforced Alloy") { Mass=5, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "viper_mkiv_armour_grade3", new ShipModule(128672259,ShipModule.ModuleTypes.MilitaryGradeComposite,"Viper Mk IV Military Grade Composite") { Mass=9, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "viper_mkiv_armour_mirrored", new ShipModule(128672260,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Viper Mk IV Mirrored Surface Composite") { Mass=9, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "viper_mkiv_armour_reactive", new ShipModule(128672261,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Viper Mk IV Reactive Surface Composite") { Mass=9, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "cobramkiv_armour_grade1", new ShipModule(128672264,ShipModule.ModuleTypes.LightweightAlloy,"Cobra Mk IV Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "cobramkiv_armour_grade2", new ShipModule(128672265,ShipModule.ModuleTypes.ReinforcedAlloy,"Cobra Mk IV Reinforced Alloy") { Mass=14, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "cobramkiv_armour_grade3", new ShipModule(128672266,ShipModule.ModuleTypes.MilitaryGradeComposite,"Cobra Mk IV Military Grade Composite") { Mass=27, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "cobramkiv_armour_mirrored", new ShipModule(128672267,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Cobra Mk IV Mirrored Surface Composite") { Mass=27, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "cobramkiv_armour_reactive", new ShipModule(128672268,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Cobra Mk IV Reactive Surface Composite") { Mass=27, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "independant_trader_armour_grade1", new ShipModule(128672271,ShipModule.ModuleTypes.LightweightAlloy,"Keelback Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "independant_trader_armour_grade2", new ShipModule(128672272,ShipModule.ModuleTypes.ReinforcedAlloy,"Keelback Reinforced Alloy") { Mass=12, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "independant_trader_armour_grade3", new ShipModule(128672273,ShipModule.ModuleTypes.MilitaryGradeComposite,"Keelback Military Grade Composite") { Mass=23, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "independant_trader_armour_mirrored", new ShipModule(128672274,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Keelback Mirrored Surface Composite") { Mass=23, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "independant_trader_armour_reactive", new ShipModule(128672275,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Keelback Reactive Surface Composite") { Mass=23, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "asp_scout_armour_grade1", new ShipModule(128672278,ShipModule.ModuleTypes.LightweightAlloy,"Asp Scout Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "asp_scout_armour_grade2", new ShipModule(128672279,ShipModule.ModuleTypes.ReinforcedAlloy,"Asp Scout Reinforced Alloy") { Mass=21, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "asp_scout_armour_grade3", new ShipModule(128672280,ShipModule.ModuleTypes.MilitaryGradeComposite,"Asp Scout Military Grade Composite") { Mass=42, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "asp_scout_armour_mirrored", new ShipModule(128672281,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Asp Scout Mirrored Surface Composite") { Mass=42, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "asp_scout_armour_reactive", new ShipModule(128672282,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Asp Scout Reactive Surface Composite") { Mass=42, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },


                { "krait_mkii_armour_grade1", new ShipModule(128816569,ShipModule.ModuleTypes.LightweightAlloy,"Krait Mk II Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "krait_mkii_armour_grade2", new ShipModule(128816570,ShipModule.ModuleTypes.ReinforcedAlloy,"Krait Mk II Reinforced Alloy") { Mass=36, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "krait_mkii_armour_grade3", new ShipModule(128816571,ShipModule.ModuleTypes.MilitaryGradeComposite,"Krait Mk II Military Grade Composite") { Mass=67, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "krait_mkii_armour_mirrored", new ShipModule(128816572,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Krait Mk II Mirrored Surface Composite") { Mass=67, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "krait_mkii_armour_reactive", new ShipModule(128816573,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Krait Mk II Reactive Surface Composite") { Mass=67, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "typex_armour_grade1", new ShipModule(128816576,ShipModule.ModuleTypes.LightweightAlloy,"Alliance Chieftain Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "typex_armour_grade2", new ShipModule(128816577,ShipModule.ModuleTypes.ReinforcedAlloy,"Alliance Chieftain Reinforced Alloy") { Mass=40, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "typex_armour_grade3", new ShipModule(128816578,ShipModule.ModuleTypes.MilitaryGradeComposite,"Alliance Chieftain Military Grade Composite") { Mass=78, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_armour_mirrored", new ShipModule(128816579,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Alliance Chieftain Mirrored Surface Composite") { Mass=78, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_armour_reactive", new ShipModule(128816580,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Alliance Chieftain Reactive Surface Composite") { Mass=78, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "typex_2_armour_grade1", new ShipModule(128816583,ShipModule.ModuleTypes.LightweightAlloy,"Alliance Crusader Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "typex_2_armour_grade2", new ShipModule(128816584,ShipModule.ModuleTypes.ReinforcedAlloy,"Alliance Crusader Reinforced Alloy") { Mass=40, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "typex_2_armour_grade3", new ShipModule(128816585,ShipModule.ModuleTypes.MilitaryGradeComposite,"Alliance Crusader Military Grade Composite") { Mass=78, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_2_armour_mirrored", new ShipModule(128816586,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Alliance Crusader Mirrored Surface Composite") { Mass=78, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_2_armour_reactive", new ShipModule(128816587,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Alliance Crusader Reactive Surface Composite") { Mass=78, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "typex_3_armour_grade1", new ShipModule(128816590,ShipModule.ModuleTypes.LightweightAlloy,"Alliance Challenger Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "typex_3_armour_grade2", new ShipModule(128816591,ShipModule.ModuleTypes.ReinforcedAlloy,"Alliance Challenger Reinforced Alloy") { Mass=40, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "typex_3_armour_grade3", new ShipModule(128816592,ShipModule.ModuleTypes.MilitaryGradeComposite,"Alliance Challenger Military Grade Composite") { Mass=78, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_3_armour_mirrored", new ShipModule(128816593,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Alliance Challenger Mirrored Surface Composite") { Mass=78, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "typex_3_armour_reactive", new ShipModule(128816594,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Alliance Challenger Reactive Surface Composite") { Mass=78, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "diamondback_armour_grade1", new ShipModule(128671218,ShipModule.ModuleTypes.LightweightAlloy,"Diamondback Scout Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "diamondback_armour_grade2", new ShipModule(128671219,ShipModule.ModuleTypes.ReinforcedAlloy,"Diamondback Scout Reinforced Alloy") { Mass=13, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "diamondback_armour_grade3", new ShipModule(128671220,ShipModule.ModuleTypes.MilitaryGradeComposite,"Diamondback Scout Military Grade Composite") { Mass=26, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "diamondback_armour_mirrored", new ShipModule(128671221,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Diamondback Scout Mirrored Surface Composite") { Mass=26, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "diamondback_armour_reactive", new ShipModule(128671222,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Diamondback Scout Reactive Surface Composite") { Mass=26, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "empire_courier_armour_grade1", new ShipModule(128671224,ShipModule.ModuleTypes.LightweightAlloy,"Imperial Courier Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "empire_courier_armour_grade2", new ShipModule(128671225,ShipModule.ModuleTypes.ReinforcedAlloy,"Imperial Courier Reinforced Alloy") { Mass=4, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "empire_courier_armour_grade3", new ShipModule(128671226,ShipModule.ModuleTypes.MilitaryGradeComposite,"Imperial Courier Military Grade Composite") { Mass=8, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_courier_armour_mirrored", new ShipModule(128671227,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Imperial Courier Mirrored Surface Composite") { Mass=8, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "empire_courier_armour_reactive", new ShipModule(128671228,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Imperial Courier Reactive Surface Composite") { Mass=8, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "type9_military_armour_grade1", new ShipModule(128785621,ShipModule.ModuleTypes.LightweightAlloy,"Type-10 Defender Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "type9_military_armour_grade2", new ShipModule(128785622,ShipModule.ModuleTypes.ReinforcedAlloy,"Type-10 Defender Reinforced Alloy") { Mass=75, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "type9_military_armour_grade3", new ShipModule(128785623,ShipModule.ModuleTypes.MilitaryGradeComposite,"Type-10 Defender Military Grade Composite") { Mass=150, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "type9_military_armour_mirrored", new ShipModule(128785624,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Type-10 Defender Mirrored Surface Composite") { Mass=150, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "type9_military_armour_reactive", new ShipModule(128785625,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Type-10 Defender Reactive Surface Composite") { Mass=150, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "krait_light_armour_grade1", new ShipModule(128839283,ShipModule.ModuleTypes.LightweightAlloy,"Krait Phantom Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "krait_light_armour_grade2", new ShipModule(128839284,ShipModule.ModuleTypes.ReinforcedAlloy,"Krait Phantom Reinforced Alloy") { Mass=26, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "krait_light_armour_grade3", new ShipModule(128839285,ShipModule.ModuleTypes.MilitaryGradeComposite,"Krait Phantom Military Grade Composite") { Mass=53, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "krait_light_armour_mirrored", new ShipModule(128839286,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Krait Phantom Mirrored Surface Composite") { Mass=53, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "krait_light_armour_reactive", new ShipModule(128839287,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Krait Phantom Reactive Surface Composite") { Mass=53, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "mamba_armour_grade1", new ShipModule(128915981,ShipModule.ModuleTypes.LightweightAlloy,"Mamba Lightweight Alloy") { Mass=0, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=80 } },
                { "mamba_armour_grade2", new ShipModule(128915982,ShipModule.ModuleTypes.ReinforcedAlloy,"Mamba Reinforced Alloy") { Mass=19, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=152 } },
                { "mamba_armour_grade3", new ShipModule(128915983,ShipModule.ModuleTypes.MilitaryGradeComposite,"Mamba Military Grade Composite") { Mass=38, Explosive=-40, Kinetic=-20, Thermal=0, AXResistance=90, HullStrengthBonus=250 } },
                { "mamba_armour_mirrored", new ShipModule(128915984,ShipModule.ModuleTypes.MirroredSurfaceComposite,"Mamba Mirrored Surface Composite") { Mass=38, Explosive=-50, Kinetic=-75, Thermal=50, AXResistance=90, HullStrengthBonus=250 } },
                { "mamba_armour_reactive", new ShipModule(128915985,ShipModule.ModuleTypes.ReactiveSurfaceComposite,"Mamba Reactive Surface Composite") { Mass=38, Explosive=20, Kinetic=25, Thermal=-40, AXResistance=90, HullStrengthBonus=250 } },

                { "python_nx_armour_grade1", new ShipModule(-1, ShipModule.ModuleTypes.LightweightAlloy,"Python Mk II Lightweight Alloy") { Mass= 0, Explosive= -40, Kinetic= -20, Thermal= 0, AXResistance= 90, HullStrengthBonus= 80 } },
                { "python_nx_armour_grade2", new ShipModule(-1, ShipModule.ModuleTypes.ReinforcedAlloy, "Python Mk II Reinforced Alloy") { Mass=26, Explosive= -40, Kinetic= -20, Thermal= 0, AXResistance= 90, HullStrengthBonus= 152 } },
                { "python_nx_armour_grade3", new ShipModule(-1, ShipModule.ModuleTypes.MilitaryGradeComposite, "Python Mk II Military Grade Composite") { Mass=53, Explosive= -40, Kinetic= -20, Thermal= 0, AXResistance= 90, HullStrengthBonus= 250 } },
                { "python_nx_armour_mirrored", new ShipModule(-1, ShipModule.ModuleTypes.MirroredSurfaceComposite, "Python Mk II Mirrored Surface Composite") { Mass=53, Explosive= -50, Kinetic= -75, Thermal= 50, AXResistance= 90, HullStrengthBonus= 250 } },
                { "python_nx_armour_reactive", new ShipModule(-1, ShipModule.ModuleTypes.ReactiveSurfaceComposite, "Python Mk II Reactive Surface Composite") { Mass=53, Explosive= 20, Kinetic= 25, Thermal= -40, AXResistance= 90, HullStrengthBonus= 250 } },

                // Auto field maint

                { "int_repairer_size1_class1", new ShipModule(128667598,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 1 Rating E"){ Cost = 10000, Class = 1, Rating = "E", Integrity = 32, Power = 0.54, BootTime = 9, Ammo = 1000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size1_class2", new ShipModule(128667606,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 1 Rating D"){ Cost = 30000, Class = 1, Rating = "D", Integrity = 24, Power = 0.72, BootTime = 9, Ammo = 900, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size1_class3", new ShipModule(128667614,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 1 Rating C"){ Cost = 90000, Class = 1, Rating = "C", Integrity = 40, Power = 0.9, BootTime = 9, Ammo = 1000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size1_class4", new ShipModule(128667622,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 1 Rating B"){ Cost = 270000, Class = 1, Rating = "B", Integrity = 56, Power = 1.04, BootTime = 9, Ammo = 1200, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size1_class5", new ShipModule(128667630,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 1 Rating A"){ Cost = 810000, Class = 1, Rating = "A", Integrity = 46, Power = 1.26, BootTime = 9, Ammo = 1100, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size2_class1", new ShipModule(128667599,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 2 Rating E"){ Cost = 18000, Class = 2, Rating = "E", Integrity = 41, Power = 0.68, BootTime = 9, Ammo = 2300, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size2_class2", new ShipModule(128667607,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 2 Rating D"){ Cost = 54000, Class = 2, Rating = "D", Integrity = 31, Power = 0.9, BootTime = 9, Ammo = 2100, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size2_class3", new ShipModule(128667615,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 2 Rating C"){ Cost = 162000, Class = 2, Rating = "C", Integrity = 51, Power = 1.13, BootTime = 9, Ammo = 2300, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size2_class4", new ShipModule(128667623,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 2 Rating B"){ Cost = 486000, Class = 2, Rating = "B", Integrity = 71, Power = 1.29, BootTime = 9, Ammo = 2800, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size2_class5", new ShipModule(128667631,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 2 Rating A"){ Cost = 1458000, Class = 2, Rating = "A", Integrity = 59, Power = 1.58, BootTime = 9, Ammo = 2500, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size3_class1", new ShipModule(128667600,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 3 Rating E"){ Cost = 32400, Class = 3, Rating = "E", Integrity = 51, Power = 0.81, BootTime = 9, Ammo = 3600, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size3_class2", new ShipModule(128667608,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 3 Rating D"){ Cost = 97200, Class = 3, Rating = "D", Integrity = 38, Power = 1.08, BootTime = 9, Ammo = 3200, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size3_class3", new ShipModule(128667616,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 3 Rating C"){ Cost = 291600, Class = 3, Rating = "C", Integrity = 64, Power = 1.35, BootTime = 9, Ammo = 3600, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size3_class4", new ShipModule(128667624,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 3 Rating B"){ Cost = 874800, Class = 3, Rating = "B", Integrity = 90, Power = 1.55, BootTime = 9, Ammo = 4300, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size3_class5", new ShipModule(128667632,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 3 Rating A"){ Cost = 2624400, Class = 3, Rating = "A", Integrity = 74, Power = 1.89, BootTime = 9, Ammo = 4000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size4_class1", new ShipModule(128667601,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 4 Rating E"){ Cost = 58320, Class = 4, Rating = "E", Integrity = 64, Power = 0.99, BootTime = 9, Ammo = 4900, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size4_class2", new ShipModule(128667609,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 4 Rating D"){ Cost = 174960, Class = 4, Rating = "D", Integrity = 48, Power = 1.32, BootTime = 9, Ammo = 4400, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size4_class3", new ShipModule(128667617,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 4 Rating C"){ Cost = 524880, Class = 4, Rating = "C", Integrity = 80, Power = 1.65, BootTime = 9, Ammo = 4900, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size4_class4", new ShipModule(128667625,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 4 Rating B"){ Cost = 1574640, Class = 4, Rating = "B", Integrity = 112, Power = 1.9, BootTime = 9, Ammo = 5900, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size4_class5", new ShipModule(128667633,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 4 Rating A"){ Cost = 4723920, Class = 4, Rating = "A", Integrity = 92, Power = 2.31, BootTime = 9, Ammo = 5400, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size5_class1", new ShipModule(128667602,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 5 Rating E"){ Cost = 104980, Class = 5, Rating = "E", Integrity = 77, Power = 1.17, BootTime = 9, Ammo = 6100, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size5_class2", new ShipModule(128667610,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 5 Rating D"){ Cost = 314930, Class = 5, Rating = "D", Integrity = 58, Power = 1.56, BootTime = 9, Ammo = 5500, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size5_class3", new ShipModule(128667618,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 5 Rating C"){ Cost = 944780, Class = 5, Rating = "C", Integrity = 96, Power = 1.95, BootTime = 9, Ammo = 6100, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size5_class4", new ShipModule(128667626,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 5 Rating B"){ Cost = 2834350, Class = 5, Rating = "B", Integrity = 134, Power = 2.24, BootTime = 9, Ammo = 7300, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size5_class5", new ShipModule(128667634,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 5 Rating A"){ Cost = 8503060, Class = 5, Rating = "A", Integrity = 110, Power = 2.73, BootTime = 9, Ammo = 6700, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size6_class1", new ShipModule(128667603,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 6 Rating E"){ Cost = 188960, Class = 6, Rating = "E", Integrity = 90, Power = 1.4, BootTime = 9, Ammo = 7400, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size6_class2", new ShipModule(128667611,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 6 Rating D"){ Cost = 566870, Class = 6, Rating = "D", Integrity = 68, Power = 1.86, BootTime = 9, Ammo = 6700, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size6_class3", new ShipModule(128667619,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 6 Rating C"){ Cost = 1700610, Class = 6, Rating = "C", Integrity = 113, Power = 2.33, BootTime = 9, Ammo = 7400, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size6_class4", new ShipModule(128667627,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 6 Rating B"){ Cost = 5101830, Class = 6, Rating = "B", Integrity = 158, Power = 2.67, BootTime = 9, Ammo = 8900, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size6_class5", new ShipModule(128667635,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 6 Rating A"){ Cost = 15305500, Class = 6, Rating = "A", Integrity = 130, Power = 3.26, BootTime = 9, Ammo = 8100, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size7_class1", new ShipModule(128667604,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 7 Rating E"){ Cost = 340120, Class = 7, Rating = "E", Integrity = 105, Power = 1.58, BootTime = 9, Ammo = 8700, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size7_class2", new ShipModule(128667612,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 7 Rating D"){ Cost = 1020370, Class = 7, Rating = "D", Integrity = 79, Power = 2.1, BootTime = 9, Ammo = 7800, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size7_class3", new ShipModule(128667620,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 7 Rating C"){ Cost = 3061100, Class = 7, Rating = "C", Integrity = 131, Power = 2.63, BootTime = 9, Ammo = 8700, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size7_class4", new ShipModule(128667628,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 7 Rating B"){ Cost = 9183300, Class = 7, Rating = "B", Integrity = 183, Power = 3.02, BootTime = 9, Ammo = 10400, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size7_class5", new ShipModule(128667636,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 7 Rating A"){ Cost = 27549900, Class = 7, Rating = "A", Integrity = 151, Power = 3.68, BootTime = 9, Ammo = 9600, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },
                { "int_repairer_size8_class1", new ShipModule(128667605,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 8 Rating E"){ Cost = 612220, Class = 8, Rating = "E", Integrity = 120, Power = 1.8, BootTime = 9, Ammo = 10000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.012, AmmoCost = 1 } },
                { "int_repairer_size8_class2", new ShipModule(128667613,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 8 Rating D"){ Cost = 1836660, Class = 8, Rating = "D", Integrity = 90, Power = 2.4, BootTime = 9, Ammo = 9000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.016, AmmoCost = 1 } },
                { "int_repairer_size8_class3", new ShipModule(128667621,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 8 Rating C"){ Cost = 5509980, Class = 8, Rating = "C", Integrity = 150, Power = 3, BootTime = 9, Ammo = 10000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.02, AmmoCost = 1 } },
                { "int_repairer_size8_class4", new ShipModule(128667629,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 8 Rating B"){ Cost = 16529940, Class = 8, Rating = "B", Integrity = 210, Power = 3.45, BootTime = 9, Ammo = 12000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.023, AmmoCost = 1 } },
                { "int_repairer_size8_class5", new ShipModule(128667637,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,"Auto Field Maintenance Unit Class 8 Rating A"){ Cost = 49589820, Class = 8, Rating = "A", Integrity = 173, Power = 4.2, BootTime = 9, Ammo = 11000, RateOfRepairConsumption = 10, RepairCostPerMat = 0.028, AmmoCost = 1 } },

                // Beam lasers

                { "hpt_beamlaser_fixed_small", new ShipModule(128049428,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Small"){ Cost = 37430, Mount = "F", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.62, BootTime = 0, DPS = 9.82, Damage = 9.82, DistributorDraw = 1.94, ThermL = 3.53, Pierce = 18, Range = 3000, BurstInterval = 0, BreachDamage = 7.9, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_fixed_medium", new ShipModule(128049429,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Medium"){ Cost = 299520, Mount = "F", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 1.01, BootTime = 0, DPS = 15.96, Damage = 15.96, DistributorDraw = 3.16, ThermL = 5.11, Pierce = 35, Range = 3000, BurstInterval = 0, BreachDamage = 12.8, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_fixed_large", new ShipModule(128049430,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Large"){ Cost = 1177600, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.62, BootTime = 0, DPS = 25.78, Damage = 25.78, DistributorDraw = 5.1, ThermL = 7.22, Pierce = 50, Range = 3000, BurstInterval = 0, BreachDamage = 20.6, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_fixed_huge", new ShipModule(128049431,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Huge"){ Cost = 2396160, Mount = "F", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 2.61, BootTime = 0, DPS = 41.38, Damage = 41.38, DistributorDraw = 8.19, ThermL = 9.93, Pierce = 60, Range = 3000, BurstInterval = 0, BreachDamage = 33.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_gimbal_small", new ShipModule(128049432,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Small"){ Cost = 74650, Mount = "G", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.6, BootTime = 0, DPS = 7.68, Damage = 7.68, DistributorDraw = 2.11, ThermL = 3.65, Pierce = 18, Range = 3000, BurstInterval = 0, BreachDamage = 6.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_gimbal_medium", new ShipModule(128049433,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Medium"){ Cost = 500600, Mount = "G", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 1, BootTime = 0, DPS = 12.52, Damage = 12.52, DistributorDraw = 3.44, ThermL = 5.32, Pierce = 35, Range = 3000, BurstInterval = 0, BreachDamage = 10, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_gimbal_large", new ShipModule(128049434,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Large"){ Cost = 2396160, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.6, BootTime = 0, DPS = 20.3, Damage = 20.3, DistributorDraw = 5.58, ThermL = 7.61, Pierce = 50, Range = 3000, BurstInterval = 0, BreachDamage = 16.2, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_turret_small", new ShipModule(128049435,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Turret Small"){ Cost = 500000, Mount = "T", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.57, BootTime = 0, DPS = 5.4, Damage = 5.4, DistributorDraw = 1.32, ThermL = 2.4, Pierce = 18, Range = 3000, BurstInterval = 0, BreachDamage = 4.3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_turret_medium", new ShipModule(128049436,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Turret Medium"){ Cost = 2099900, Mount = "T", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.93, BootTime = 0, DPS = 8.83, Damage = 8.83, DistributorDraw = 2.16, ThermL = 3.53, Pierce = 35, Range = 3000, BurstInterval = 0, BreachDamage = 7.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_turret_large", new ShipModule(128049437,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Turret Large"){ Cost = 19399600, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 1.51, BootTime = 0, DPS = 14.36, Damage = 14.36, DistributorDraw = 3.51, ThermL = 5.11, Pierce = 50, Range = 3000, BurstInterval = 0, BreachDamage = 11.5, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },

                { "hpt_beamlaser_fixed_small_heat", new ShipModule(128671346,ShipModule.ModuleTypes.RetributorBeamLaser,"Retributor Beam Laser Fixed Small"){ Cost = 56150, Mount = "F", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.62, BootTime = 0, DPS = 4.91, Damage = 4.91, DistributorDraw = 2.52, ThermL = 2.7, Pierce = 18, Range = 3000, BurstInterval = 0, BreachDamage = 3.9, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },
                { "hpt_beamlaser_gimbal_huge", new ShipModule(128681994,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Huge"){ Cost = 8746160, Mount = "G", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 2.57, BootTime = 0, DPS = 32.68, Damage = 32.68, DistributorDraw = 8.99, ThermL = 10.62, Pierce = 60, Range = 3000, BurstInterval = 0, BreachDamage = 26.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 600 } },

                // burst laser

                { "hpt_pulselaserburst_fixed_small", new ShipModule(128049400,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Fixed Small"){ Cost = 4400, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.65, BootTime = 0, DPS = 8.147, Damage = 1.72, DistributorDraw = 0.25, ThermL = 0.38, Pierce = 20, Range = 3000, RateOfFire = 4.737, BurstInterval = 0.5, BurstRateOfFire = 15, BurstSize = 3, BreachDamage = 1.5, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_fixed_medium", new ShipModule(128049401,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Fixed Medium"){ Cost = 23000, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 40, Power = 1.05, BootTime = 0, DPS = 13.045, Damage = 3.53, DistributorDraw = 0.5, ThermL = 0.78, Pierce = 35, Range = 3000, RateOfFire = 3.695, BurstInterval = 0.63, BurstRateOfFire = 11, BurstSize = 3, BreachDamage = 3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_fixed_large", new ShipModule(128049402,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Fixed Large"){ Cost = 140400, Mount = "F", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 1.66, BootTime = 0, DPS = 20.785, Damage = 7.73, DistributorDraw = 1.11, ThermL = 1.7, Pierce = 52, Range = 3000, RateOfFire = 2.689, BurstInterval = 0.83, BurstRateOfFire = 7, BurstSize = 3, BreachDamage = 3.9, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_fixed_huge", new ShipModule(128049403,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Fixed Huge"){ Cost = 281600, Mount = "F", Class = 4, Rating = "E", Mass = 16, Integrity = 80, Power = 2.58, BootTime = 0, DPS = 32.259, Damage = 20.61, DistributorDraw = 2.98, ThermL = 4.53, Pierce = 65, Range = 3000, RateOfFire = 1.565, BurstInterval = 1.25, BurstRateOfFire = 3, BurstSize = 3, BreachDamage = 17.5, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_gimbal_small", new ShipModule(128049404,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Gimbal Small"){ Cost = 8600, Mount = "G", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.64, BootTime = 0, DPS = 6.448, Damage = 1.22, DistributorDraw = 0.24, ThermL = 0.34, Pierce = 20, Range = 3000, RateOfFire = 5.285, BurstInterval = 0.45, BurstRateOfFire = 17, BurstSize = 3, BreachDamage = 1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_gimbal_medium", new ShipModule(128049405,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Gimbal Medium"){ Cost = 48500, Mount = "G", Class = 2, Rating = "F", Mass = 4, Integrity = 40, Power = 1.04, BootTime = 0, DPS = 10.296, Damage = 2.45, DistributorDraw = 0.49, ThermL = 0.67, Pierce = 35, Range = 3000, RateOfFire = 4.203, BurstInterval = 0.56, BurstRateOfFire = 13, BurstSize = 3, BreachDamage = 2.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_gimbal_large", new ShipModule(128049406,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Gimbal Large"){ Cost = 281600, Mount = "G", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 1.65, BootTime = 0, DPS = 16.605, Damage = 5.16, DistributorDraw = 1.03, ThermL = 1.42, Pierce = 52, Range = 3000, RateOfFire = 3.218, BurstInterval = 0.71, BurstRateOfFire = 9, BurstSize = 3, BreachDamage = 4.4, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_turret_small", new ShipModule(128049407,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Turret Small"){ Cost = 52800, Mount = "T", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.6, BootTime = 0, DPS = 4.174, Damage = 0.87, DistributorDraw = 0.139, ThermL = 0.19, Pierce = 20, Range = 3000, RateOfFire = 4.798, BurstInterval = 0.52, BurstRateOfFire = 19, BurstSize = 3, BreachDamage = 0.4, BreachMin = 60, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_turret_medium", new ShipModule(128049408,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Turret Medium"){ Cost = 162800, Mount = "T", Class = 2, Rating = "F", Mass = 4, Integrity = 40, Power = 0.98, BootTime = 0, DPS = 6.76, Damage = 1.72, DistributorDraw = 0.275, ThermL = 0.38, Pierce = 35, Range = 3000, RateOfFire = 3.93, BurstInterval = 0.63, BurstRateOfFire = 15, BurstSize = 3, BreachDamage = 0.9, BreachMin = 60, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },
                { "hpt_pulselaserburst_turret_large", new ShipModule(128049409,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Turret Large"){ Cost = 800400, Mount = "T", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 1.57, BootTime = 0, DPS = 11.01, Damage = 3.53, DistributorDraw = 0.56, ThermL = 0.78, Pierce = 52, Range = 3000, RateOfFire = 3.119, BurstInterval = 0.78, BurstRateOfFire = 11, BurstSize = 3, BreachDamage = 1.8, BreachMin = 60, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },


                { "hpt_pulselaserburst_gimbal_huge", new ShipModule(128727920,ShipModule.ModuleTypes.BurstLaser,"Burst Laser Gimbal Huge"){ Cost = 1245600, Mount = "G", Class = 4, Rating = "E", Mass = 16, Integrity = 64, Power = 2.59, BootTime = 0, DPS = 25.907, Damage = 12.09, DistributorDraw = 2.41, ThermL = 3.33, Pierce = 65, Range = 3000, RateOfFire = 2.143, BurstInterval = 1, BurstRateOfFire = 5, BurstSize = 3, BreachDamage = 10.3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, KineticProportion = 0, Falloff = 500 } },

                { "hpt_pulselaserburst_fixed_small_scatter", new ShipModule(128671449,ShipModule.ModuleTypes.CytoscramblerBurstLaser,"Cytoscrambler Burst Laser Fixed Small"){ Cost = 8800, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.8, BootTime = 0, DPS = 27.429, Damage = 3.6, DistributorDraw = 0.31, ThermL = 0.28, Pierce = 1, Range = 1000, RateOfFire = 7.619, BurstInterval = 0.7, BurstRateOfFire = 20, BurstSize = 8, BreachDamage = 3.1, BreachMin = 0, BreachMax = 0, Jitter = 1.7, ThermalProportion = 100, KineticProportion = 0, Falloff = 600 } },

                // Cannons

                { "hpt_cannon_fixed_small", new ShipModule(128049438,ShipModule.ModuleTypes.Cannon,"Cannon Fixed Small"){ Cost = 21100, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.34, BootTime = 0, DPS = 11.25, Damage = 22.5, DistributorDraw = 0.46, ThermL = 1.38, Pierce = 35, Range = 3000, Speed = 1200, RateOfFire = 0.5, BurstInterval = 2, Clip = 6, Ammo = 120, ReloadTime = 3, BreachDamage = 21.4, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3000, AmmoCost = 20 } },
                { "hpt_cannon_fixed_medium", new ShipModule(128049439,ShipModule.ModuleTypes.Cannon,"Cannon Fixed Medium"){ Cost = 168430, Mount = "F", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.49, BootTime = 0, DPS = 16.993, Damage = 36.875, DistributorDraw = 0.7, ThermL = 2.11, Pierce = 50, Range = 3500, Speed = 1051.051, RateOfFire = 0.461, BurstInterval = 2.17, Clip = 6, Ammo = 120, ReloadTime = 3, BreachDamage = 35, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3500, AmmoCost = 20 } },
                { "hpt_cannon_fixed_large", new ShipModule(128049440,ShipModule.ModuleTypes.Cannon,"Cannon Fixed Large"){ Cost = 675200, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.67, BootTime = 0, DPS = 23.372, Damage = 55.625, DistributorDraw = 1.07, ThermL = 3.2, Pierce = 70, Range = 4000, Speed = 959.233, RateOfFire = 0.42, BurstInterval = 2.38, Clip = 6, Ammo = 120, ReloadTime = 3, BreachDamage = 52.8, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 4000, AmmoCost = 20 } },
                { "hpt_cannon_fixed_huge", new ShipModule(128049441,ShipModule.ModuleTypes.Cannon,"Cannon Fixed Huge"){ Cost = 2700800, Mount = "F", Class = 4, Rating = "B", Mass = 16, Integrity = 80, Power = 0.92, BootTime = 0, DPS = 31.606, Damage = 83.125, DistributorDraw = 1.61, ThermL = 4.83, Pierce = 90, Range = 4500, Speed = 900, RateOfFire = 0.38, BurstInterval = 2.63, Clip = 6, Ammo = 120, ReloadTime = 3, BreachDamage = 79, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 4500, AmmoCost = 20 } },
                { "hpt_cannon_gimbal_small", new ShipModule(128049442,ShipModule.ModuleTypes.Cannon,"Cannon Gimbal Small"){ Cost = 42200, Mount = "G", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.38, BootTime = 0, DPS = 8.292, Damage = 15.92, DistributorDraw = 0.48, ThermL = 1.25, Pierce = 35, Range = 3000, Speed = 1000, RateOfFire = 0.521, BurstInterval = 1.92, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 15.1, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3000, AmmoCost = 20 } },
                { "hpt_cannon_gimbal_medium", new ShipModule(128049443,ShipModule.ModuleTypes.Cannon,"Cannon Gimbal Medium"){ Cost = 337600, Mount = "G", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.54, BootTime = 0, DPS = 12.274, Damage = 25.53, DistributorDraw = 0.75, ThermL = 1.92, Pierce = 50, Range = 3500, Speed = 875, RateOfFire = 0.481, BurstInterval = 2.08, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 24.3, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3500, AmmoCost = 20 } },
                { "hpt_cannon_gimbal_huge", new ShipModule(128049444,ShipModule.ModuleTypes.Cannon,"Cannon Gimbal Huge"){ Cost = 5401600, Mount = "G", Class = 4, Rating = "B", Mass = 16, Integrity = 80, Power = 1.03, BootTime = 0, DPS = 22.636, Damage = 56.59, DistributorDraw = 1.72, ThermL = 4.43, Pierce = 90, Range = 4500, Speed = 750, RateOfFire = 0.4, BurstInterval = 2.5, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 53.8, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 4500, AmmoCost = 20 } },
                { "hpt_cannon_turret_small", new ShipModule(128049445,ShipModule.ModuleTypes.Cannon,"Cannon Turret Small"){ Cost = 506400, Mount = "T", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.32, BootTime = 0, DPS = 5.528, Damage = 12.77, DistributorDraw = 0.22, ThermL = 0.67, Pierce = 35, Range = 3000, Speed = 1000, RateOfFire = 0.433, BurstInterval = 2.31, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 12.1, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3000, AmmoCost = 20 } },
                { "hpt_cannon_turret_medium", new ShipModule(128049446,ShipModule.ModuleTypes.Cannon,"Cannon Turret Medium"){ Cost = 4051200, Mount = "T", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.45, BootTime = 0, DPS = 7.916, Damage = 19.79, DistributorDraw = 0.34, ThermL = 1.03, Pierce = 50, Range = 3500, Speed = 875, RateOfFire = 0.4, BurstInterval = 2.5, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 18.8, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 3500, AmmoCost = 20 } },
                { "hpt_cannon_turret_large", new ShipModule(128049447,ShipModule.ModuleTypes.Cannon,"Cannon Turret Large"){ Cost = 16204800, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 11.154, Damage = 30.34, DistributorDraw = 0.53, ThermL = 1.58, Pierce = 70, Range = 4000, Speed = 800, RateOfFire = 0.368, BurstInterval = 2.72, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 28.8, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 4000, AmmoCost = 20 } },

                { "hpt_cannon_gimbal_large", new ShipModule(128671120,ShipModule.ModuleTypes.Cannon,"Cannon Gimbal Large"){ Cost = 1350400, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.75, BootTime = 0, DPS = 16.485, Damage = 37.421, DistributorDraw = 1.14, ThermL = 2.93, Pierce = 70, Range = 4000, Speed = 800, RateOfFire = 0.441, BurstInterval = 2.27, Clip = 5, Ammo = 100, ReloadTime = 4, BreachDamage = 35.5, BreachMin = 60, BreachMax = 90, KineticProportion = 100, ExplosiveProportion = 0, Falloff = 4000, AmmoCost = 20 } },

                // Frag cannon

                { "hpt_slugshot_fixed_small", new ShipModule(128049448,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Fixed Small"){ Cost = 36000, Mount = "F", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.45, BootTime = 0, DPS = 95.333, Damage = 1.43, DistributorDraw = 0.21, ThermL = 0.41, Pierce = 20, Range = 2000, Speed = 667, RateOfFire = 5.556, BurstInterval = 0.18, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 1.3, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_fixed_medium", new ShipModule(128049449,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Fixed Medium"){ Cost = 291840, Mount = "F", Class = 2, Rating = "A", Mass = 4, Integrity = 51, Power = 0.74, BootTime = 0, DPS = 179.1, Damage = 2.985, DistributorDraw = 0.37, ThermL = 0.74, Pierce = 30, Range = 2000, Speed = 667, RateOfFire = 5, BurstInterval = 0.2, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 2.7, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_fixed_large", new ShipModule(128049450,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Fixed Large"){ Cost = 1167360, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.02, BootTime = 0, DPS = 249.273, Damage = 4.57, DistributorDraw = 0.57, ThermL = 1.13, Pierce = 45, Range = 2000, Speed = 667, RateOfFire = 4.545, BurstInterval = 0.22, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 4.1, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_gimbal_small", new ShipModule(128049451,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Gimbal Small"){ Cost = 54720, Mount = "G", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.59, BootTime = 0, DPS = 71.294, Damage = 1.01, DistributorDraw = 0.26, ThermL = 0.44, Pierce = 20, Range = 2000, Speed = 667, RateOfFire = 5.882, BurstInterval = 0.17, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 0.9, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_gimbal_medium", new ShipModule(128049452,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Gimbal Medium"){ Cost = 437760, Mount = "G", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 1.03, BootTime = 0, DPS = 143.621, Damage = 2.274, DistributorDraw = 0.49, ThermL = 0.84, Pierce = 30, Range = 2000, Speed = 667, RateOfFire = 5.263, BurstInterval = 0.19, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 2, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_turret_small", new ShipModule(128049453,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Turret Small"){ Cost = 182400, Mount = "T", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.42, BootTime = 0, DPS = 39.429, Damage = 0.69, DistributorDraw = 0.1, ThermL = 0.2, Pierce = 20, Range = 2000, Speed = 667, RateOfFire = 4.762, BurstInterval = 0.21, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 0.6, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_turret_medium", new ShipModule(128049454,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Turret Medium"){ Cost = 1459200, Mount = "T", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.79, BootTime = 0, DPS = 87.13, Damage = 1.67, DistributorDraw = 0.21, ThermL = 0.41, Pierce = 30, Range = 2000, Speed = 667, RateOfFire = 4.348, BurstInterval = 0.23, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 1.5, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },

                { "hpt_slugshot_gimbal_large", new ShipModule(128671321,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Gimbal Large"){ Cost = 1751040, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.55, BootTime = 0, DPS = 215.429, Damage = 3.77, DistributorDraw = 0.81, ThermL = 1.4, Pierce = 45, Range = 2000, Speed = 667, RateOfFire = 4.762, BurstInterval = 0.21, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 3.4, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },
                { "hpt_slugshot_turret_large", new ShipModule(128671322,ShipModule.ModuleTypes.FragmentCannon,"Fragment Cannon Turret Large"){ Cost = 5836800, Mount = "T", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.29, BootTime = 0, DPS = 143.28, Damage = 2.985, DistributorDraw = 0.37, ThermL = 0.74, Pierce = 45, Range = 2000, Speed = 667, RateOfFire = 4, BurstInterval = 0.25, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 2.7, BreachMin = 40, BreachMax = 80, Jitter = 5, KineticProportion = 100, ThermalProportion = 0, Falloff = 1800, AmmoCost = 17 } },

                { "hpt_slugshot_fixed_large_range", new ShipModule(128671343,ShipModule.ModuleTypes.PacifierFrag_Cannon,"Pacifier Fragment Cannon Fixed Large"){ Cost = 1751040, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.02, BootTime = 0, DPS = 216, Damage = 3.96, DistributorDraw = 0.57, ThermL = 1.13, Pierce = 45, Range = 3000, Speed = 1000, RateOfFire = 4.545, BurstInterval = 0.22, Clip = 3, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 3.6, BreachMin = 40, BreachMax = 80, Jitter = 1.7, KineticProportion = 100, ThermalProportion = 0, Falloff = 2800, AmmoCost = 17 } },

                // Cargo racks

                { "int_cargorack_size1_class1", new ShipModule(128064338,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 1 Rating E"){ Cost = 1000, Class = 1, Rating = "E", Size = 2 } },
                { "int_cargorack_size2_class1", new ShipModule(128064339,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 2 Rating E"){ Cost = 3250, Class = 2, Rating = "E", Size = 4 } },
                { "int_cargorack_size3_class1", new ShipModule(128064340,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 3 Rating E"){ Cost = 10560, Class = 3, Rating = "E", Size = 8 } },
                { "int_cargorack_size4_class1", new ShipModule(128064341,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 4 Rating E"){ Cost = 34330, Class = 4, Rating = "E", Size = 16 } },
                { "int_cargorack_size5_class1", new ShipModule(128064342,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 5 Rating E"){ Cost = 111570, Class = 5, Rating = "E", Size = 32 } },
                { "int_cargorack_size6_class1", new ShipModule(128064343,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 6 Rating E"){ Cost = 362590, Class = 6, Rating = "E", Size = 64 } },
                { "int_cargorack_size7_class1", new ShipModule(128064344,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 7 Rating E"){ Cost = 1178420, Class = 7, Rating = "E", Size = 128 } },
                { "int_cargorack_size8_class1", new ShipModule(128064345,ShipModule.ModuleTypes.CargoRack,"Cargo Rack Class 8 Rating E"){ Cost = 3829870, Class = 8, Rating = "E", Size = 256 } },

                { "int_cargorack_size2_class1_free", new ShipModule(128666643, ShipModule.ModuleTypes.CargoRack, "Cargo Rack Class 2 Rating E") { Size = 4 } },

                { "int_corrosionproofcargorack_size1_class1", new ShipModule(128681641,ShipModule.ModuleTypes.CorrosionResistantCargoRack,"Anti Corrosion Cargo Rack Class 1 Rating E"){ Cost = 6250, Class = 1, Rating = "E", Size = 1 } },
                { "int_corrosionproofcargorack_size1_class2", new ShipModule(128681992,ShipModule.ModuleTypes.CorrosionResistantCargoRack,"Anti Corrosion Cargo Rack Class 1 Rating F"){ Cost = 12560, Class = 1, Rating = "F", Size = 2 } },

                { "int_corrosionproofcargorack_size4_class1", new ShipModule(128833944,ShipModule.ModuleTypes.CorrosionResistantCargoRack,"Anti Corrosion Cargo Rack Class 4 Rating E"){ Cost = 94330, Class = 4, Rating = "E", Size = 16 } },
                { "int_corrosionproofcargorack_size5_class1", new ShipModule(128957069,ShipModule.ModuleTypes.CorrosionResistantCargoRack,"Anti Corrosion Cargo Rack Class 5 Rating E"){ Cost = -999, Class = 5, Rating = "E", Size = 32 } },
                { "int_corrosionproofcargorack_size6_class1", new ShipModule(999999906, ShipModule.ModuleTypes.CorrosionResistantCargoRack,"Anti Corrosion Cargo Rack Class 6 Rating E") { Size = 64 } },

                // Manifest Scanner

                { "hpt_cargoscanner_size0_class1", new ShipModule(128662520,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Rating E"){ Cost = 13540, Class = 0, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.2, BootTime = 3, Range = 2000, Angle = 15, Time = 10 } },
                { "hpt_cargoscanner_size0_class2", new ShipModule(128662521,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Rating D"){ Cost = 40630, Class = 0, Rating = "D", Mass = 1.3, Integrity = 24, Power = 0.4, BootTime = 3, Range = 2500, Angle = 15, Time = 10 } },
                { "hpt_cargoscanner_size0_class3", new ShipModule(128662522,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Rating C"){ Cost = 121900, Class = 0, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.8, BootTime = 3, Range = 3000, Angle = 15, Time = 10 } },
                { "hpt_cargoscanner_size0_class4", new ShipModule(128662523,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Rating B"){ Cost = 365700, Class = 0, Rating = "B", Mass = 1.3, Integrity = 56, Power = 1.6, BootTime = 3, Range = 3500, Angle = 15, Time = 10 } },
                { "hpt_cargoscanner_size0_class5", new ShipModule(128662524,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Rating A"){ Cost = 1097100, Class = 0, Rating = "A", Mass = 1.3, Integrity = 48, Power = 3.2, BootTime = 3, Range = 4000, Angle = 15, Time = 10 } },

                // Chaff, ECM

                { "hpt_chafflauncher_tiny", new ShipModule(128049513,ShipModule.ModuleTypes.ChaffLauncher,"Chaff Launcher Tiny"){ Cost = 8500, Class = 0, Rating = "I", Mass = 1.3, Integrity = 20, Power = 0.2, BootTime = 0, DistributorDraw = 4, ThermL = 4, RateOfFire = 1, BurstInterval = 1, Clip = 1, Ammo = 10, ReloadTime = 10, Time = 20, AmmoCost = 100 } },
                { "hpt_electroniccountermeasure_tiny", new ShipModule(128049516,ShipModule.ModuleTypes.ElectronicCountermeasure,"Electronic Countermeasure Tiny"){ Cost = 12500, Class = 0, Rating = "F", Mass = 1.3, Integrity = 20, Power = 0.2, BootTime = 0, Range = 3000, Time = 3, ActivePower = 4, WasteHeat = 4, ReloadTime = 10 } },
                { "hpt_heatsinklauncher_turret_tiny", new ShipModule(128049519,ShipModule.ModuleTypes.HeatSinkLauncher,"Heat Sink Launcher Turret Tiny"){ Cost = 3500, Class = 0, Rating = "I", Mass = 1.3, Integrity = 45, Power = 0.2, BootTime = 0, DistributorDraw = 2, RateOfFire = 0.2, BurstInterval = 5, Clip = 1, Ammo = 2, ReloadTime = 10, Time = 10, WasteHeat = 100, AmmoCost = 25 } },
                { "hpt_causticsinklauncher_turret_tiny", new ShipModule(129019262,ShipModule.ModuleTypes.CausticSinkLauncher,"Caustic Sink Launcher Turret Tiny"){ Cost = 50000, Class = 0, Rating = "I", Mass = 1.7, Integrity = 45, Power = 0.6, BootTime = 0, DistributorDraw = 2, RateOfFire = 0.2, BurstInterval = 5, Clip = 1, Ammo = 5, ReloadTime = 10, AmmoCost = 10 } },
                { "hpt_plasmapointdefence_turret_tiny", new ShipModule(128049522,ShipModule.ModuleTypes.PointDefence,"Point Defence Turret Tiny"){ Cost = 18550, Mount = "T", Class = 0, Rating = "I", Mass = 0.5, Integrity = 30, Power = 0.2, BootTime = 0, DPS = 2, Damage = 0.2, ThermL = 0.07, Range = 2500, Speed = 1000, RateOfFire = 10, BurstInterval = 0.2, BurstRateOfFire = 15, BurstSize = 4, Clip = 12, Ammo = 10000, ReloadTime = 0.4, Jitter = 0.75, KineticProportion = 100, AmmoCost = 1 } },

                // kill warrant

                { "hpt_crimescanner_size0_class1", new ShipModule(128662530,ShipModule.ModuleTypes.KillWarrantScanner,"Kill Warrant Scanner Rating E"){ Cost = 13540, Class = 0, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.2, BootTime = 2, Range = 2000, Angle = 15, Time = 10 } },
                { "hpt_crimescanner_size0_class2", new ShipModule(128662531,ShipModule.ModuleTypes.KillWarrantScanner,"Kill Warrant Scanner Rating D"){ Cost = 40630, Class = 0, Rating = "D", Mass = 1.3, Integrity = 24, Power = 0.4, BootTime = 2, Range = 2500, Angle = 15, Time = 10 } },
                { "hpt_crimescanner_size0_class3", new ShipModule(128662532,ShipModule.ModuleTypes.KillWarrantScanner,"Kill Warrant Scanner Rating C"){ Cost = 121900, Class = 0, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.8, BootTime = 2, Range = 3000, Angle = 15, Time = 10 } },
                { "hpt_crimescanner_size0_class4", new ShipModule(128662533,ShipModule.ModuleTypes.KillWarrantScanner,"Kill Warrant Scanner Rating B"){ Cost = 365700, Class = 0, Rating = "B", Mass = 1.3, Integrity = 56, Power = 1.6, BootTime = 2, Range = 3500, Angle = 15, Time = 10 } },
                { "hpt_crimescanner_size0_class5", new ShipModule(128662534,ShipModule.ModuleTypes.KillWarrantScanner,"Kill Warrant Scanner Rating A"){ Cost = 1097100, Class = 0, Rating = "A", Mass = 1.3, Integrity = 48, Power = 3.2, BootTime = 2, Range = 4000, Angle = 15, Time = 10 } },

                // surface scanner

//                { "int_detailedsurfacescanner_tiny", new ShipModule(128666634,ShipModule.ModuleTypes.DetailedSurfaceScanner,"Detailed Surface Scanner"){ Cost = 250000, Class = 1, Rating = "I", Integrity = 20, Clip = 3, Proberad = 20 } },

                // docking computer

                { "int_dockingcomputer_standard", new ShipModule(128049549,ShipModule.ModuleTypes.StandardDockingComputer,"Standard Docking Computer"){ Cost = 4500, Class = 1, Rating = "E", Integrity = 10, Power = 0.39, BootTime = 3 } },
                { "int_dockingcomputer_advanced", new ShipModule(128935155,ShipModule.ModuleTypes.AdvancedDockingComputer,"Advanced Docking Computer"){ Cost = 13510, Class = 1, Rating = "E", Integrity = 10, Power = 0.45, BootTime = 3 } },

                // figther bays

                { "int_fighterbay_size5_class1", new ShipModule(128727930,ShipModule.ModuleTypes.FighterHangar,"Fighter Hangar Class 5 Rating E"){ Cost = 575660, Class = 5, Rating = "D", Mass = 20, Integrity = 60, Power = 0.25, BootTime = 5, Size = 1, Rebuilds = 6, AmmoCost = 1030 } },
                { "int_fighterbay_size6_class1", new ShipModule(128727931,ShipModule.ModuleTypes.FighterHangar,"Fighter Hangar Class 6 Rating E"){ Cost = 1869350, Class = 6, Rating = "D", Mass = 40, Integrity = 80, Power = 0.35, BootTime = 5, Size = 2, Rebuilds = 8, AmmoCost = 1030 } },
                { "int_fighterbay_size7_class1", new ShipModule(128727932,ShipModule.ModuleTypes.FighterHangar,"Fighter Hangar Class 7 Rating E"){ Cost = 2369330, Class = 7, Rating = "D", Mass = 60, Integrity = 120, Power = 0.35, BootTime = 5, Size = 2, Rebuilds = 15, AmmoCost = 1030 } },

                // flak

                { "hpt_flakmortar_fixed_medium", new ShipModule(128785626,ShipModule.ModuleTypes.RemoteReleaseFlakLauncher,"Remote Release Flak Launcher Fixed Medium"){ Cost = 261800, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 17, Damage = 34, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 32, ReloadTime = 2, BreachDamage = 1.7, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, AmmoCost = 125 } },
                { "hpt_flakmortar_turret_medium", new ShipModule(128793058,ShipModule.ModuleTypes.RemoteReleaseFlakLauncher,"Remote Release Flak Launcher Turret Medium"){ Cost = 1259200, Mount = "T", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 17, Damage = 34, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 32, ReloadTime = 2, BreachDamage = 1.7, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, AmmoCost = 125 } },

                // flechette

                { "hpt_flechettelauncher_fixed_medium", new ShipModule(128833996,ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,"Remote Release Flechette Launcher Fixed Medium"){ Cost = 353760, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 6.5, Damage = 13, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 80, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 72, ReloadTime = 2, BreachDamage = 6.5, BreachMin = 100, BreachMax = 100, KineticProportion = 76.923, ExplosiveProportion = 23.077, AmmoCost = 56 } },
                { "hpt_flechettelauncher_turret_medium", new ShipModule(128833997,ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,"Remote Release Flechette Launcher Turret Medium"){ Cost = 1279200, Mount = "T", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 6.5, Damage = 13, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 70, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 72, ReloadTime = 2, BreachDamage = 6.5, BreachMin = 100, BreachMax = 100, KineticProportion = 76.923, ExplosiveProportion = 23.077, AmmoCost = 56 } },

                // Frame Shift Drive Interdictor

                { "int_fsdinterdictor_size1_class1", new ShipModule(128666704,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 1 Rating E"){ Cost = 12000, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.14, BootTime = 15, TargetMaxTime = 3, Angle = 50 } },
                { "int_fsdinterdictor_size2_class1", new ShipModule(128666705,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 2 Rating E"){ Cost = 33600, Class = 2, Rating = "E", Mass = 2.5, Integrity = 41, Power = 0.17, BootTime = 15, TargetMaxTime = 6, Angle = 50 } },
                { "int_fsdinterdictor_size3_class1", new ShipModule(128666706,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 3 Rating E"){ Cost = 94080, Class = 3, Rating = "E", Mass = 5, Integrity = 51, Power = 0.2, BootTime = 15, TargetMaxTime = 9, Angle = 50 } },
                { "int_fsdinterdictor_size4_class1", new ShipModule(128666707,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 4 Rating E"){ Cost = 263420, Class = 4, Rating = "E", Mass = 10, Integrity = 64, Power = 0.25, BootTime = 15, TargetMaxTime = 12, Angle = 50 } },
                { "int_fsdinterdictor_size1_class2", new ShipModule(128666708,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 1 Rating D"){ Cost = 36000, Class = 1, Rating = "D", Mass = 0.5, Integrity = 24, Power = 0.18, BootTime = 15, TargetMaxTime = 4, Angle = 50 } },
                { "int_fsdinterdictor_size2_class2", new ShipModule(128666709,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 2 Rating D"){ Cost = 100800, Class = 2, Rating = "D", Mass = 1, Integrity = 31, Power = 0.22, BootTime = 15, TargetMaxTime = 7, Angle = 50 } },
                { "int_fsdinterdictor_size3_class2", new ShipModule(128666710,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 3 Rating D"){ Cost = 282240, Class = 3, Rating = "D", Mass = 2, Integrity = 38, Power = 0.27, BootTime = 15, TargetMaxTime = 10, Angle = 50 } },
                { "int_fsdinterdictor_size4_class2", new ShipModule(128666711,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 4 Rating D"){ Cost = 790270, Class = 4, Rating = "D", Mass = 4, Integrity = 48, Power = 0.33, BootTime = 15, TargetMaxTime = 13, Angle = 50 } },
                { "int_fsdinterdictor_size1_class3", new ShipModule(128666712,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 1 Rating C"){ Cost = 108000, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.23, BootTime = 15, TargetMaxTime = 5, Angle = 50 } },
                { "int_fsdinterdictor_size2_class3", new ShipModule(128666713,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 2 Rating C"){ Cost = 302400, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.28, BootTime = 15, TargetMaxTime = 8, Angle = 50 } },
                { "int_fsdinterdictor_size3_class3", new ShipModule(128666714,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 3 Rating C"){ Cost = 846720, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.34, BootTime = 15, TargetMaxTime = 11, Angle = 50 } },
                { "int_fsdinterdictor_size4_class3", new ShipModule(128666715,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 4 Rating C"){ Cost = 2370820, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 0.41, BootTime = 15, TargetMaxTime = 14, Angle = 50 } },
                { "int_fsdinterdictor_size1_class4", new ShipModule(128666716,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 1 Rating B"){ Cost = 324000, Class = 1, Rating = "B", Mass = 2, Integrity = 56, Power = 0.28, BootTime = 15, TargetMaxTime = 6, Angle = 50 } },
                { "int_fsdinterdictor_size2_class4", new ShipModule(128666717,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 2 Rating B"){ Cost = 907200, Class = 2, Rating = "B", Mass = 4, Integrity = 71, Power = 0.34, BootTime = 15, TargetMaxTime = 9, Angle = 50 } },
                { "int_fsdinterdictor_size3_class4", new ShipModule(128666718,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 3 Rating B"){ Cost = 2540160, Class = 3, Rating = "B", Mass = 8, Integrity = 90, Power = 0.41, BootTime = 15, TargetMaxTime = 12, Angle = 50 } },
                { "int_fsdinterdictor_size4_class4", new ShipModule(128666719,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 4 Rating B"){ Cost = 7112450, Class = 4, Rating = "B", Mass = 16, Integrity = 112, Power = 0.49, BootTime = 15, TargetMaxTime = 15, Angle = 50 } },
                { "int_fsdinterdictor_size1_class5", new ShipModule(128666720,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 1 Rating A"){ Cost = 972000, Class = 1, Rating = "A", Mass = 1.3, Integrity = 48, Power = 0.32, BootTime = 15, TargetMaxTime = 7, Angle = 50 } },
                { "int_fsdinterdictor_size2_class5", new ShipModule(128666721,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 2 Rating A"){ Cost = 2721600, Class = 2, Rating = "A", Mass = 2.5, Integrity = 61, Power = 0.39, BootTime = 15, TargetMaxTime = 10, Angle = 50 } },
                { "int_fsdinterdictor_size3_class5", new ShipModule(128666722,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 3 Rating A"){ Cost = 7620480, Class = 3, Rating = "A", Mass = 5, Integrity = 77, Power = 0.48, BootTime = 15, TargetMaxTime = 13, Angle = 50 } },
                { "int_fsdinterdictor_size4_class5", new ShipModule(128666723,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,"Frame Shift Drive Interdictor Class 4 Rating A"){ Cost = 21337340, Class = 4, Rating = "A", Mass = 10, Integrity = 96, Power = 0.57, BootTime = 15, TargetMaxTime = 16, Angle = 50 } },

                // Fuel scoop

                { "int_fuelscoop_size1_class1", new ShipModule(128666644,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 1 Rating E"){ Cost = 310, Class = 1, Rating = "E", Integrity = 32, Power = 0.14, BootTime = 4, RefillRate = 0.018 } },
                { "int_fuelscoop_size2_class1", new ShipModule(128666645,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 2 Rating E"){ Cost = 1070, Class = 2, Rating = "E", Integrity = 41, Power = 0.17, BootTime = 4, RefillRate = 0.032 } },
                { "int_fuelscoop_size3_class1", new ShipModule(128666646,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 3 Rating E"){ Cost = 3390, Class = 3, Rating = "E", Integrity = 51, Power = 0.2, BootTime = 4, RefillRate = 0.075 } },
                { "int_fuelscoop_size4_class1", new ShipModule(128666647,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 4 Rating E"){ Cost = 10730, Class = 4, Rating = "E", Integrity = 64, Power = 0.25, BootTime = 4, RefillRate = 0.147 } },
                { "int_fuelscoop_size5_class1", new ShipModule(128666648,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 5 Rating E"){ Cost = 34030, Class = 5, Rating = "E", Integrity = 77, Power = 0.3, BootTime = 4, RefillRate = 0.247 } },
                { "int_fuelscoop_size6_class1", new ShipModule(128666649,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 6 Rating E"){ Cost = 107860, Class = 6, Rating = "E", Integrity = 90, Power = 0.35, BootTime = 4, RefillRate = 0.376 } },
                { "int_fuelscoop_size7_class1", new ShipModule(128666650,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 7 Rating E"){ Cost = 341930, Class = 7, Rating = "E", Integrity = 105, Power = 0.41, BootTime = 4, RefillRate = 0.534 } },
                { "int_fuelscoop_size8_class1", new ShipModule(128666651,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 8 Rating E"){ Cost = 1083910, Class = 8, Rating = "E", Integrity = 120, Power = 0.48, BootTime = 4, RefillRate = 0.72 } },
                { "int_fuelscoop_size1_class2", new ShipModule(128666652,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 1 Rating D"){ Cost = 1290, Class = 1, Rating = "D", Integrity = 24, Power = 0.18, BootTime = 4, RefillRate = 0.024 } },
                { "int_fuelscoop_size2_class2", new ShipModule(128666653,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 2 Rating D"){ Cost = 4450, Class = 2, Rating = "D", Integrity = 31, Power = 0.22, BootTime = 4, RefillRate = 0.043 } },
                { "int_fuelscoop_size3_class2", new ShipModule(128666654,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 3 Rating D"){ Cost = 14110, Class = 3, Rating = "D", Integrity = 38, Power = 0.27, BootTime = 4, RefillRate = 0.1 } },
                { "int_fuelscoop_size4_class2", new ShipModule(128666655,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 4 Rating D"){ Cost = 44720, Class = 4, Rating = "D", Integrity = 48, Power = 0.33, BootTime = 4, RefillRate = 0.196 } },
                { "int_fuelscoop_size5_class2", new ShipModule(128666656,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 5 Rating D"){ Cost = 141780, Class = 5, Rating = "D", Integrity = 58, Power = 0.4, BootTime = 4, RefillRate = 0.33 } },
                { "int_fuelscoop_size6_class2", new ShipModule(128666657,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 6 Rating D"){ Cost = 449430, Class = 6, Rating = "D", Integrity = 68, Power = 0.47, BootTime = 4, RefillRate = 0.502 } },
                { "int_fuelscoop_size7_class2", new ShipModule(128666658,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 7 Rating D"){ Cost = 1424700, Class = 7, Rating = "D", Integrity = 79, Power = 0.55, BootTime = 4, RefillRate = 0.712 } },
                { "int_fuelscoop_size8_class2", new ShipModule(128666659,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 8 Rating D"){ Cost = 4516290, Class = 8, Rating = "D", Integrity = 90, Power = 0.64, BootTime = 4, RefillRate = 0.96 } },
                { "int_fuelscoop_size1_class3", new ShipModule(128666660,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 1 Rating C"){ Cost = 5140, Class = 1, Rating = "C", Integrity = 40, Power = 0.23, BootTime = 4, RefillRate = 0.03 } },
                { "int_fuelscoop_size2_class3", new ShipModule(128666661,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 2 Rating C"){ Cost = 17800, Class = 2, Rating = "C", Integrity = 51, Power = 0.28, BootTime = 4, RefillRate = 0.054 } },
                { "int_fuelscoop_size3_class3", new ShipModule(128666662,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 3 Rating C"){ Cost = 56440, Class = 3, Rating = "C", Integrity = 64, Power = 0.34, BootTime = 4, RefillRate = 0.126 } },
                { "int_fuelscoop_size4_class3", new ShipModule(128666663,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 4 Rating C"){ Cost = 178900, Class = 4, Rating = "C", Integrity = 80, Power = 0.41, BootTime = 4, RefillRate = 0.245 } },
                { "int_fuelscoop_size5_class3", new ShipModule(128666664,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 5 Rating C"){ Cost = 567110, Class = 5, Rating = "C", Integrity = 96, Power = 0.5, BootTime = 4, RefillRate = 0.412 } },
                { "int_fuelscoop_size6_class3", new ShipModule(128666665,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 6 Rating C"){ Cost = 1797730, Class = 6, Rating = "C", Integrity = 113, Power = 0.59, BootTime = 4, RefillRate = 0.627 } },
                { "int_fuelscoop_size7_class3", new ShipModule(128666666,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 7 Rating C"){ Cost = 5698790, Class = 7, Rating = "C", Integrity = 131, Power = 0.69, BootTime = 4, RefillRate = 0.89 } },
                { "int_fuelscoop_size8_class3", new ShipModule(128666667,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 8 Rating C"){ Cost = 18065170, Class = 8, Rating = "C", Integrity = 150, Power = 0.8, BootTime = 4, RefillRate = 1.2 } },
                { "int_fuelscoop_size1_class4", new ShipModule(128666668,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 1 Rating B"){ Cost = 20570, Class = 1, Rating = "B", Integrity = 56, Power = 0.28, BootTime = 4, RefillRate = 0.036 } },
                { "int_fuelscoop_size2_class4", new ShipModule(128666669,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 2 Rating B"){ Cost = 71210, Class = 2, Rating = "B", Integrity = 70, Power = 0.34, BootTime = 4, RefillRate = 0.065 } },
                { "int_fuelscoop_size3_class4", new ShipModule(128666670,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 3 Rating B"){ Cost = 225740, Class = 3, Rating = "B", Integrity = 90, Power = 0.41, BootTime = 4, RefillRate = 0.151 } },
                { "int_fuelscoop_size4_class4", new ShipModule(128666671,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 4 Rating B"){ Cost = 715590, Class = 4, Rating = "B", Integrity = 112, Power = 0.49, BootTime = 4, RefillRate = 0.294 } },
                { "int_fuelscoop_size5_class4", new ShipModule(128666672,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 5 Rating B"){ Cost = 2268420, Class = 5, Rating = "B", Integrity = 134, Power = 0.6, BootTime = 4, RefillRate = 0.494 } },
                { "int_fuelscoop_size6_class4", new ShipModule(128666673,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 6 Rating B"){ Cost = 7190900, Class = 6, Rating = "B", Integrity = 158, Power = 0.71, BootTime = 4, RefillRate = 0.752 } },
                { "int_fuelscoop_size6_class5", new ShipModule(128666681,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 6 Rating A"){ Cost = 28763610, Class = 6, Rating = "A", Integrity = 136, Power = 0.83, BootTime = 4, RefillRate = 0.878 } },
                { "int_fuelscoop_size7_class4", new ShipModule(128666674,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 7 Rating B"){ Cost = 22795160, Class = 7, Rating = "B", Integrity = 183, Power = 0.83, BootTime = 4, RefillRate = 1.068 } },
                { "int_fuelscoop_size8_class4", new ShipModule(128666675,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 8 Rating B"){ Cost = 72260660, Class = 8, Rating = "B", Integrity = 210, Power = 0.96, BootTime = 4, RefillRate = 1.44 } },
                { "int_fuelscoop_size1_class5", new ShipModule(128666676,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 1 Rating A"){ Cost = 82270, Class = 1, Rating = "A", Integrity = 48, Power = 0.32, BootTime = 4, RefillRate = 0.042 } },
                { "int_fuelscoop_size2_class5", new ShipModule(128666677,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 2 Rating A"){ Cost = 284840, Class = 2, Rating = "A", Integrity = 61, Power = 0.39, BootTime = 4, RefillRate = 0.075 } },
                { "int_fuelscoop_size3_class5", new ShipModule(128666678,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 3 Rating A"){ Cost = 902950, Class = 3, Rating = "A", Integrity = 77, Power = 0.48, BootTime = 4, RefillRate = 0.176 } },
                { "int_fuelscoop_size4_class5", new ShipModule(128666679,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 4 Rating A"){ Cost = 2862360, Class = 4, Rating = "A", Integrity = 96, Power = 0.57, BootTime = 4, RefillRate = 0.342 } },
                { "int_fuelscoop_size5_class5", new ShipModule(128666680,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 5 Rating A"){ Cost = 9073690, Class = 5, Rating = "A", Integrity = 115, Power = 0.7, BootTime = 4, RefillRate = 0.577 } },
                { "int_fuelscoop_size7_class5", new ShipModule(128666682,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 7 Rating A"){ Cost = 91180640, Class = 7, Rating = "A", Integrity = 157, Power = 0.97, BootTime = 4, RefillRate = 1.245 } },
                { "int_fuelscoop_size8_class5", new ShipModule(128666683,ShipModule.ModuleTypes.FuelScoop,"Fuel Scoop Class 8 Rating A"){ Cost = 289042640, Class = 8, Rating = "A", Integrity = 180, Power = 1.12, BootTime = 4, RefillRate = 1.68 } },

                // fuel tank

                { "int_fueltank_size1_class3_free", new ShipModule(128667018, ShipModule.ModuleTypes.FuelTank, "Fuel Tank Class 1 Rating C") { Size= 2 } },
                { "int_fueltank_size1_class3", new ShipModule(128064346,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 1 Rating C"){ Cost = 1000, Class = 1, Rating = "C", Size = 2 } },
                { "int_fueltank_size2_class3", new ShipModule(128064347,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 2 Rating C"){ Cost = 3750, Class = 2, Rating = "C", Size = 4 } },
                { "int_fueltank_size3_class3", new ShipModule(128064348,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 3 Rating C"){ Cost = 7060, Class = 3, Rating = "C", Size = 8 } },
                { "int_fueltank_size4_class3", new ShipModule(128064349,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 4 Rating C"){ Cost = 24730, Class = 4, Rating = "C", Size = 16 } },
                { "int_fueltank_size5_class3", new ShipModule(128064350,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 5 Rating C"){ Cost = 97750, Class = 5, Rating = "C", Size = 32 } },
                { "int_fueltank_size6_class3", new ShipModule(128064351,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 6 Rating C"){ Cost = 341580, Class = 6, Rating = "C", Size = 64 } },
                { "int_fueltank_size7_class3", new ShipModule(128064352,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 7 Rating C"){ Cost = 1780910, Class = 7, Rating = "C", Size = 128 } },
                { "int_fueltank_size8_class3", new ShipModule(128064353,ShipModule.ModuleTypes.FuelTank,"Fuel Tank Class 8 Rating C"){ Cost = 5428430, Class = 8, Rating = "C", Size = 256 } },

                // Gardian

                { "hpt_guardian_plasmalauncher_turret_small", new ShipModule(128891606,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Turret Small"){ Cost = 484050, Mount = "T", Class = 1, Rating = "F", Mass = 2, Integrity = 34, Power = 1.6, BootTime = 0, DPS = 10, Damage = 2, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 0.8, ThermL = 5.01, Pierce = 65, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 0.5, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },
                { "hpt_guardian_plasmalauncher_fixed_small", new ShipModule(128891607,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Fixed Small"){ Cost = 176500, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 34, Power = 1.4, BootTime = 0, DPS = 15, Damage = 3, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 0.68, ThermL = 4.21, Pierce = 65, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 0.75, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },
                { "hpt_guardian_shardcannon_turret_small", new ShipModule(128891608,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Turret Small"){ Cost = 502000, Mount = "T", Class = 1, Rating = "F", Mass = 2, Integrity = 34, Power = 0.72, BootTime = 0, DPS = 40.4, Damage = 2.02, DistributorDraw = 0.36, ThermL = 0.58, Pierce = 30, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 1.6, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },
                { "hpt_guardian_shardcannon_fixed_small", new ShipModule(128891609,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Fixed Small"){ Cost = 151650, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 34, Power = 0.87, BootTime = 0, DPS = 72.8, Damage = 3.64, DistributorDraw = 0.42, ThermL = 0.69, Pierce = 30, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 2.9, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },
                { "hpt_guardian_gausscannon_fixed_small", new ShipModule(128891610,ShipModule.ModuleTypes.GuardianGaussCannon,"Guardian Gauss Cannon Fixed Small"){ Cost = 167250, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 1.91, BootTime = 0, DPS = 48.193, Damage = 40, DistributorDraw = 3.8, ThermL = 15, Pierce = 140, Range = 3000, Time = 1.2, RateOfFire = 1.205, BurstInterval = 0.83, Clip = 1, Ammo = 80, ReloadTime = 1, BreachDamage = 20, BreachMin = 20, BreachMax = 40, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1500, AmmoCost = 75 } },

                { "hpt_guardian_gausscannon_fixed_medium", new ShipModule(128833687,ShipModule.ModuleTypes.GuardianGaussCannon,"Guardian Gauss Cannon Fixed Medium"){ Cost = 543800, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 42, Power = 2.61, BootTime = 0, DPS = 84.337, Damage = 70, DistributorDraw = 7.2, ThermL = 25, Pierce = 140, Range = 3000, Time = 1.2, RateOfFire = 1.205, BurstInterval = 0.83, Clip = 1, Ammo = 80, ReloadTime = 1, BreachDamage = 35, BreachMin = 20, BreachMax = 40, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1500, AmmoCost = 75 } },

                { "hpt_guardian_plasmalauncher_fixed_medium", new ShipModule(128833998,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Fixed Medium"){ Cost = 567760, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 42, Power = 2.13, BootTime = 0, DPS = 25, Damage = 5, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 1.25, ThermL = 5.21, Pierce = 80, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 1.25, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },
                { "hpt_guardian_plasmalauncher_turret_medium", new ShipModule(128833999,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Turret Medium"){ Cost = 1659200, Mount = "T", Class = 2, Rating = "E", Mass = 4, Integrity = 42, Power = 2.01, BootTime = 0, DPS = 20, Damage = 4, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 1.4, ThermL = 5.8, Pierce = 80, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 1, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },
                { "hpt_guardian_shardcannon_fixed_medium", new ShipModule(128834000,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Fixed Medium"){ Cost = 507760, Mount = "F", Class = 2, Rating = "A", Mass = 4, Integrity = 42, Power = 1.21, BootTime = 0, DPS = 135.4, Damage = 6.77, DistributorDraw = 0.65, ThermL = 1.2, Pierce = 45, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 5.4, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },
                { "hpt_guardian_shardcannon_turret_medium", new ShipModule(128834001,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Turret Medium"){ Cost = 1767000, Mount = "T", Class = 2, Rating = "A", Mass = 4, Integrity = 42, Power = 1.16, BootTime = 0, DPS = 86.8, Damage = 4.34, DistributorDraw = 0.57, ThermL = 1.09, Pierce = 45, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 3.5, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },

                { "hpt_guardian_plasmalauncher_fixed_large", new ShipModule(128834783,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Fixed Large"){ Cost = 1423300, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 51, Power = 3.1, BootTime = 0, DPS = 35, Damage = 7, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 2.42, ThermL = 6.15, Pierce = 95, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 1.75, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },
                { "hpt_guardian_plasmalauncher_turret_large", new ShipModule(128834784,ShipModule.ModuleTypes.GuardianPlasmaCharger,"Guardian Plasma Charger Turret Large"){ Cost = 5495200, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 51, Power = 2.53, BootTime = 0, DPS = 30, Damage = 6, Time = 1.8, DamageMultiplierFulLCharge = 17, DistributorDraw = 2.6, ThermL = 6.4, Pierce = 95, Range = 3000, Speed = 1200, RateOfFire = 5, BurstInterval = 0.2, Clip = 15, Ammo = 200, ReloadTime = 3, BreachDamage = 1.5, BreachMin = 50, BreachMax = 80, AbsolutePortionDamage = 50, AXPortionDamage = 50, Falloff = 1000, AmmoCost = 100 } },

                { "hpt_guardian_shardcannon_fixed_large", new ShipModule(128834778,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Fixed Large"){ Cost = 1461350, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 51, Power = 1.68, BootTime = 0, DPS = 190, Damage = 9.5, DistributorDraw = 1.4, ThermL = 2.2, Pierce = 60, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 7.6, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },
                { "hpt_guardian_shardcannon_turret_large", new ShipModule(128834779,ShipModule.ModuleTypes.GuardianShardCannon,"Guardian Shard Cannon Turret Large"){ Cost = 5865030, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 51, Power = 1.39, BootTime = 0, DPS = 124, Damage = 6.2, DistributorDraw = 1.2, ThermL = 1.98, Pierce = 60, Range = 1700, Speed = 1133, RateOfFire = 1.667, BurstInterval = 0.6, Clip = 5, Ammo = 180, Rounds = 12, ReloadTime = 5, BreachDamage = 5, BreachMin = 60, BreachMax = 80, Jitter = 5, ThermalProportion = 50, AXPortionDamage = 50, Falloff = 1700, AmmoCost = 9 } },

                { "int_guardianhullreinforcement_size1_class2", new ShipModule(128833946,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 1 Rating D"){ Cost = 30000, Class = 1, Rating = "D", Mass = 1, Power = 0.56, HullReinforcement = 138, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size1_class1", new ShipModule(128833945,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 1 Rating E"){ Cost = 10000, Class = 1, Rating = "E", Mass = 2, Power = 0.45, HullReinforcement = 100, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size2_class1", new ShipModule(128833947,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 2 Rating E"){ Cost = 24000, Class = 2, Rating = "E", Mass = 4, Power = 0.68, HullReinforcement = 188, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size2_class2", new ShipModule(128833948,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 2 Rating D"){ Cost = 72000, Class = 2, Rating = "D", Mass = 2, Power = 0.79, HullReinforcement = 238, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size3_class1", new ShipModule(128833949,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 3 Rating E"){ Cost = 57600, Class = 3, Rating = "E", Mass = 8, Power = 0.9, HullReinforcement = 288, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size3_class2", new ShipModule(128833950,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 3 Rating D"){ Cost = 172800, Class = 3, Rating = "D", Mass = 4, Power = 1.01, HullReinforcement = 325, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size4_class1", new ShipModule(128833951,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 4 Rating E"){ Cost = 138240, Class = 4, Rating = "E", Mass = 16, Power = 1.13, HullReinforcement = 375, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size4_class2", new ShipModule(128833952,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 4 Rating D"){ Cost = 414720, Class = 4, Rating = "D", Mass = 8, Power = 1.24, HullReinforcement = 413, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size5_class1", new ShipModule(128833953,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 5 Rating E"){ Cost = 331780, Class = 5, Rating = "E", Mass = 32, Power = 1.35, HullReinforcement = 450, Thermal = 2, CausticReinforcement = 5 } },
                { "int_guardianhullreinforcement_size5_class2", new ShipModule(128833954,ShipModule.ModuleTypes.GuardianHullReinforcement,"Guardian Hull Reinforcement Package Class 5 Rating D"){ Cost = 995330, Class = 5, Rating = "D", Mass = 16, Power = 1.46, HullReinforcement = 488, Thermal = 2, CausticReinforcement = 5 } },

                { "int_guardianmodulereinforcement_size1_class1", new ShipModule(128833955,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 1 Rating E"){ Cost = 10000, Class = 1, Rating = "E", Mass = 2, Integrity = 85, Power = 0.27, Protection = 30 } },
                { "int_guardianmodulereinforcement_size1_class2", new ShipModule(128833956,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 1 Rating D"){ Cost = 30000, Class = 1, Rating = "D", Mass = 1, Integrity = 77, Power = 0.34, Protection = 60 } },
                { "int_guardianmodulereinforcement_size2_class1", new ShipModule(128833957,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 2 Rating E"){ Cost = 24000, Class = 2, Rating = "E", Mass = 4, Integrity = 127, Power = 0.41, Protection = 30 } },
                { "int_guardianmodulereinforcement_size2_class2", new ShipModule(128833958,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 2 Rating D"){ Cost = 72000, Class = 2, Rating = "D", Mass = 2, Integrity = 116, Power = 0.47, Protection = 60 } },
                { "int_guardianmodulereinforcement_size3_class1", new ShipModule(128833959,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 3 Rating E"){ Cost = 57600, Class = 3, Rating = "E", Mass = 8, Integrity = 187, Power = 0.54, Protection = 30 } },
                { "int_guardianmodulereinforcement_size3_class2", new ShipModule(128833960,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 3 Rating D"){ Cost = 172800, Class = 3, Rating = "D", Mass = 4, Integrity = 171, Power = 0.61, Protection = 60 } },
                { "int_guardianmodulereinforcement_size4_class1", new ShipModule(128833961,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 4 Rating E"){ Cost = 138240, Class = 4, Rating = "E", Mass = 16, Integrity = 286, Power = 0.68, Protection = 30 } },
                { "int_guardianmodulereinforcement_size4_class2", new ShipModule(128833962,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 4 Rating D"){ Cost = 414720, Class = 4, Rating = "D", Mass = 8, Integrity = 259, Power = 0.74, Protection = 60 } },
                { "int_guardianmodulereinforcement_size5_class1", new ShipModule(128833963,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 5 Rating E"){ Cost = 331780, Class = 5, Rating = "E", Mass = 32, Integrity = 424, Power = 0.81, Protection = 30 } },
                { "int_guardianmodulereinforcement_size5_class2", new ShipModule(128833964,ShipModule.ModuleTypes.GuardianModuleReinforcement,"Guardian Module Reinforcement Package Class 5 Rating D"){ Cost = 995330, Class = 5, Rating = "D", Mass = 16, Integrity = 385, Power = 0.88, Protection = 60 } },

                { "int_guardianshieldreinforcement_size1_class1", new ShipModule(128833965,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 1 Rating E"){ Cost = 10000, Class = 1, Rating = "E", Mass = 2, Integrity = 36, Power = 0.35, AdditionalStrength = 44 } },
                { "int_guardianshieldreinforcement_size1_class2", new ShipModule(128833966,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 1 Rating D"){ Cost = 30000, Class = 1, Rating = "D", Mass = 1, Integrity = 36, Power = 0.46, AdditionalStrength = 61 } },
                { "int_guardianshieldreinforcement_size2_class1", new ShipModule(128833967,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 2 Rating E"){ Cost = 24000, Class = 2, Rating = "E", Mass = 4, Integrity = 36, Power = 0.56, AdditionalStrength = 83 } },
                { "int_guardianshieldreinforcement_size2_class2", new ShipModule(128833968,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 2 Rating D"){ Cost = 72000, Class = 2, Rating = "D", Mass = 2, Integrity = 36, Power = 0.67, AdditionalStrength = 105 } },
                { "int_guardianshieldreinforcement_size3_class1", new ShipModule(128833969,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 3 Rating E"){ Cost = 57600, Class = 3, Rating = "E", Mass = 8, Integrity = 36, Power = 0.74, AdditionalStrength = 127 } },
                { "int_guardianshieldreinforcement_size3_class2", new ShipModule(128833970,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 3 Rating D"){ Cost = 172800, Class = 3, Rating = "D", Mass = 4, Integrity = 36, Power = 0.84, AdditionalStrength = 143 } },
                { "int_guardianshieldreinforcement_size4_class1", new ShipModule(128833971,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 4 Rating E"){ Cost = 138240, Class = 4, Rating = "E", Mass = 16, Integrity = 36, Power = 0.95, AdditionalStrength = 165 } },
                { "int_guardianshieldreinforcement_size4_class2", new ShipModule(128833972,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 4 Rating D"){ Cost = 414720, Class = 4, Rating = "D", Mass = 8, Integrity = 36, Power = 1.05, AdditionalStrength = 182 } },
                { "int_guardianshieldreinforcement_size5_class1", new ShipModule(128833973,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 5 Rating E"){ Cost = 331780, Class = 5, Rating = "E", Mass = 32, Integrity = 36, Power = 1.16, AdditionalStrength = 198 } },
                { "int_guardianshieldreinforcement_size5_class2", new ShipModule(128833974,ShipModule.ModuleTypes.GuardianShieldReinforcement,"Guardian Shield Reinforcement Package Class 5 Rating D"){ Cost = 995330, Class = 5, Rating = "D", Mass = 16, Integrity = 36, Power = 1.26, AdditionalStrength = 215 } },

                { "int_guardianfsdbooster_size1", new ShipModule(128833975,ShipModule.ModuleTypes.GuardianFSDBooster,"Guardian Frame Shift Drive Booster Class 1"){ Cost = 405020, Class = 1, Rating = "H", Mass = 1.3, Integrity = 32, Power = 0.75, BootTime = 15, AdditionalRange = 4 } },
                { "int_guardianfsdbooster_size2", new ShipModule(128833976,ShipModule.ModuleTypes.GuardianFSDBooster,"Guardian Frame Shift Drive Booster Class 2"){ Cost = 810520, Class = 2, Rating = "H", Mass = 1.3, Integrity = 32, Power = 0.98, BootTime = 15, AdditionalRange = 6 } },
                { "int_guardianfsdbooster_size3", new ShipModule(128833977,ShipModule.ModuleTypes.GuardianFSDBooster,"Guardian Frame Shift Drive Booster Class 3"){ Cost = 1620430, Class = 3, Rating = "H", Mass = 1.3, Integrity = 32, Power = 1.27, BootTime = 15, AdditionalRange = 7.75 } },
                { "int_guardianfsdbooster_size4", new ShipModule(128833978,ShipModule.ModuleTypes.GuardianFSDBooster,"Guardian Frame Shift Drive Booster Class 4"){ Cost = 3245010, Class = 4, Rating = "H", Mass = 1.3, Integrity = 32, Power = 1.65, BootTime = 15, AdditionalRange = 9.25 } },
                { "int_guardianfsdbooster_size5", new ShipModule(128833979,ShipModule.ModuleTypes.GuardianFSDBooster,"Guardian Frame Shift Drive Booster Class 5"){ Cost = 6483100, Class = 5, Rating = "H", Mass = 1.3, Integrity = 32, Power = 2.14, BootTime = 15, AdditionalRange = 10.5 } },

                { "int_guardianpowerdistributor_size1", new ShipModule(128833980,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 1"){ Cost = 40960, Class = 1, Rating = "A", Mass = 1.4, Integrity = 35, Power = 0.62, BootTime = 5, PowerBonus = 4, WepCap = 10, WepMW = 2.5, EngCap = 9, EngMW = 0.8, SysCap = 10, SysMW = 0.8 } },
                { "int_guardianpowerdistributor_size2", new ShipModule(128833981,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 2"){ Cost = 111600, Class = 2, Rating = "A", Mass = 2.6, Integrity = 45, Power = 0.73, BootTime = 5, PowerBonus = 4, WepCap = 13, WepMW = 3.1, EngCap = 11, EngMW = 1, SysCap = 11, SysMW = 1 } },
                { "int_guardianpowerdistributor_size3", new ShipModule(128833982,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 3"){ Cost = 311370, Class = 3, Rating = "A", Mass = 5.25, Integrity = 56, Power = 0.78, BootTime = 5, PowerBonus = 4, WepCap = 17, WepMW = 3.9, EngCap = 14, EngMW = 1.7, SysCap = 14, SysMW = 1.7 } },
                { "int_guardianpowerdistributor_size4", new ShipModule(128833983,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 4"){ Cost = 868710, Class = 4, Rating = "A", Mass = 10.5, Integrity = 70, Power = 0.87, BootTime = 5, PowerBonus = 4, WepCap = 22, WepMW = 4.9, EngCap = 17, EngMW = 2.5, SysCap = 17, SysMW = 2.5 } },
                { "int_guardianpowerdistributor_size5", new ShipModule(128833984,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 5"){ Cost = 2423690, Class = 5, Rating = "A", Mass = 21, Integrity = 99, Power = 0.96, BootTime = 5, PowerBonus = 4, WepCap = 29, WepMW = 6, EngCap = 22, EngMW = 3.3, SysCap = 22, SysMW = 3.3 } },
                { "int_guardianpowerdistributor_size6", new ShipModule(128833985,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 6"){ Cost = 6762090, Class = 6, Rating = "A", Mass = 42, Integrity = 99, Power = 1.07, BootTime = 5, PowerBonus = 4, WepCap = 35, WepMW = 7.3, EngCap = 26, EngMW = 4.2, SysCap = 26, SysMW = 4.2 } },
                { "int_guardianpowerdistributor_size7", new ShipModule(128833986,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 7"){ Cost = 18866230, Class = 7, Rating = "A", Mass = 84, Integrity = 115, Power = 1.16, BootTime = 5, PowerBonus = 4, WepCap = 43, WepMW = 8.5, EngCap = 31, EngMW = 5.2, SysCap = 31, SysMW = 5.2 } },
                { "int_guardianpowerdistributor_size8", new ShipModule(128833987,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,"Guardian Hybrid Power Distributor Class 8"){ Cost = 52636790, Class = 8, Rating = "A", Mass = 168, Integrity = 132, Power = 1.25, BootTime = 5, PowerBonus = 4, WepCap = 50, WepMW = 10.1, EngCap = 36, EngMW = 6.2, SysCap = 36, SysMW = 6.2 } },

                { "int_guardianpowerplant_size2", new ShipModule(128833988,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 2"){ Cost = 192170, Class = 2, Rating = "A", Mass = 1.5, Integrity = 56, PowerGen = 12.7, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size3", new ShipModule(128833989,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 3"){ Cost = 576490, Class = 3, Rating = "A", Mass = 2.9, Integrity = 70, PowerGen = 15.8, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size4", new ShipModule(128833990,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 4"){ Cost = 1729480, Class = 4, Rating = "A", Mass = 5.9, Integrity = 88, PowerGen = 20.6, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size5", new ShipModule(128833991,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 5"){ Cost = 5188440, Class = 5, Rating = "A", Mass = 11.7, Integrity = 106, PowerGen = 26.9, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size6", new ShipModule(128833992,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 6"){ Cost = 15565320, Class = 6, Rating = "A", Mass = 23.4, Integrity = 124, PowerGen = 33.3, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size7", new ShipModule(128833993,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 7"){ Cost = 46695950, Class = 7, Rating = "A", Mass = 46.8, Integrity = 144, PowerGen = 39.6, HeatEfficiency = 0.5 } },
                { "int_guardianpowerplant_size8", new ShipModule(128833994,ShipModule.ModuleTypes.GuardianHybridPowerPlant,"Guardian Hybrid Power Plant Class 8"){ Cost = 140087850, Class = 8, Rating = "A", Mass = 93.6, Integrity = 165, PowerGen = 47.5, HeatEfficiency = 0.5 } },


                // Hull Reinforcement Packages 

                { "int_hullreinforcement_size1_class1", new ShipModule(128668537,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 1 Rating E"){ Cost = 5000, Class = 1, Rating = "E", Mass = 2, HullReinforcement = 80, Kinetic = 0.5, Thermal = 0.5, Explosive = 0.5 } },
                { "int_hullreinforcement_size1_class2", new ShipModule(128668538,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 1 Rating D"){ Cost = 15000, Class = 1, Rating = "D", Mass = 1, HullReinforcement = 110, Kinetic = 0.5, Thermal = 0.5, Explosive = 0.5 } },
                { "int_hullreinforcement_size2_class1", new ShipModule(128668539,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 2 Rating E"){ Cost = 12000, Class = 2, Rating = "E", Mass = 4, HullReinforcement = 150, Kinetic = 1, Thermal = 1, Explosive = 1 } },
                { "int_hullreinforcement_size2_class2", new ShipModule(128668540,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 2 Rating D"){ Cost = 36000, Class = 2, Rating = "D", Mass = 2, HullReinforcement = 190, Kinetic = 1, Thermal = 1, Explosive = 1 } },
                { "int_hullreinforcement_size3_class1", new ShipModule(128668541,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 3 Rating E"){ Cost = 28000, Class = 3, Rating = "E", Mass = 8, HullReinforcement = 230, Kinetic = 1.5, Thermal = 1.5, Explosive = 1.5 } },
                { "int_hullreinforcement_size3_class2", new ShipModule(128668542,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 3 Rating D"){ Cost = 84000, Class = 3, Rating = "D", Mass = 4, HullReinforcement = 260, Kinetic = 1.5, Thermal = 1.5, Explosive = 1.5 } },
                { "int_hullreinforcement_size4_class1", new ShipModule(128668543,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 4 Rating E"){ Cost = 65000, Class = 4, Rating = "E", Mass = 16, HullReinforcement = 300, Kinetic = 2, Thermal = 2, Explosive = 2 } },
                { "int_hullreinforcement_size4_class2", new ShipModule(128668544,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 4 Rating D"){ Cost = 195000, Class = 4, Rating = "D", Mass = 8, HullReinforcement = 330, Kinetic = 2, Thermal = 2, Explosive = 2 } },
                { "int_hullreinforcement_size5_class1", new ShipModule(128668545,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 5 Rating E"){ Cost = 150000, Class = 5, Rating = "E", Mass = 32, HullReinforcement = 360, Kinetic = 2.5, Thermal = 2.5, Explosive = 2.5 } },
                { "int_hullreinforcement_size5_class2", new ShipModule(128668546,ShipModule.ModuleTypes.HullReinforcementPackage,"Hull Reinforcement Package Class 5 Rating D"){ Cost = 450000, Class = 5, Rating = "D", Mass = 16, HullReinforcement = 390, Kinetic = 2.5, Thermal = 2.5, Explosive = 2.5 } },

                // Frame ship drive

                { "int_hyperdrive_size2_class1_free", new ShipModule(128666637, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 2 Rating E") { Power=0.16, Mass=2.5, Integrity=46, OptMass=48, PowerConstant=2, LinearConstant=11, MaxFuelPerJump=0.6, ThermL=10, BootTime=10 } },
                { "int_hyperdrive_size2_class1", new ShipModule(128064103,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, Power = 0.16, BootTime = 10, OptMass = 48, ThermL = 10, MaxFuelPerJump = 0.6, LinearConstant = 0.011, PowerConstant = 2 } },
                { "int_hyperdrive_size2_class2", new ShipModule(128064104,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 2 Rating D"){ Cost = 5930, Class = 2, Rating = "D", Mass = 1, Integrity = 41, Power = 0.18, BootTime = 10, OptMass = 54, ThermL = 10, MaxFuelPerJump = 0.6, LinearConstant = 0.01, PowerConstant = 2 } },
                { "int_hyperdrive_size2_class3", new ShipModule(128064105,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 2 Rating C"){ Cost = 17800, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.2, BootTime = 10, OptMass = 60, ThermL = 10, MaxFuelPerJump = 0.6, LinearConstant = 0.008, PowerConstant = 2 } },
                { "int_hyperdrive_size2_class4", new ShipModule(128064106,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 2 Rating B"){ Cost = 53410, Class = 2, Rating = "B", Mass = 4, Integrity = 77, Power = 0.25, BootTime = 10, OptMass = 75, ThermL = 10, MaxFuelPerJump = 0.8, LinearConstant = 0.01, PowerConstant = 2 } },
                { "int_hyperdrive_size2_class5", new ShipModule(128064107,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 2 Rating A"){ Cost = 160220, Class = 2, Rating = "A", Mass = 2.5, Integrity = 64, Power = 0.3, BootTime = 10, OptMass = 90, ThermL = 10, MaxFuelPerJump = 0.9, LinearConstant = 0.012, PowerConstant = 2 } },
                { "int_hyperdrive_size3_class1", new ShipModule(128064108,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 3 Rating E"){ Cost = 6270, Class = 3, Rating = "E", Mass = 5, Integrity = 58, Power = 0.24, BootTime = 10, OptMass = 80, ThermL = 14, MaxFuelPerJump = 1.2, LinearConstant = 0.011, PowerConstant = 2.15 } },
                { "int_hyperdrive_size3_class2", new ShipModule(128064109,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 3 Rating D"){ Cost = 18810, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.27, BootTime = 10, OptMass = 90, ThermL = 14, MaxFuelPerJump = 1.2, LinearConstant = 0.01, PowerConstant = 2.15 } },
                { "int_hyperdrive_size3_class3", new ShipModule(128064110,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 3 Rating C"){ Cost = 56440, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.3, BootTime = 10, OptMass = 100, ThermL = 14, MaxFuelPerJump = 1.2, LinearConstant = 0.008, PowerConstant = 2.15 } },
                { "int_hyperdrive_size3_class4", new ShipModule(128064111,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 3 Rating B"){ Cost = 169300, Class = 3, Rating = "B", Mass = 8, Integrity = 96, Power = 0.38, BootTime = 10, OptMass = 125, ThermL = 14, MaxFuelPerJump = 1.5, LinearConstant = 0.01, PowerConstant = 2.15 } },
                { "int_hyperdrive_size3_class5", new ShipModule(128064112,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 3 Rating A"){ Cost = 507910, Class = 3, Rating = "A", Mass = 5, Integrity = 80, Power = 0.45, BootTime = 10, OptMass = 150, ThermL = 14, MaxFuelPerJump = 1.8, LinearConstant = 0.012, PowerConstant = 2.15 } },
                { "int_hyperdrive_size4_class1", new ShipModule(128064113,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 4 Rating E"){ Cost = 19880, Class = 4, Rating = "E", Mass = 10, Integrity = 72, Power = 0.24, BootTime = 10, OptMass = 280, ThermL = 18, MaxFuelPerJump = 2, LinearConstant = 0.011, PowerConstant = 2.3 } },
                { "int_hyperdrive_size4_class2", new ShipModule(128064114,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 4 Rating D"){ Cost = 59630, Class = 4, Rating = "D", Mass = 4, Integrity = 64, Power = 0.27, BootTime = 10, OptMass = 315, ThermL = 18, MaxFuelPerJump = 2, LinearConstant = 0.01, PowerConstant = 2.3 } },
                { "int_hyperdrive_size4_class3", new ShipModule(128064115,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 4 Rating C"){ Cost = 178900, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 0.3, BootTime = 10, OptMass = 350, ThermL = 18, MaxFuelPerJump = 2, LinearConstant = 0.008, PowerConstant = 2.3 } },
                { "int_hyperdrive_size4_class4", new ShipModule(128064116,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 4 Rating B"){ Cost = 536690, Class = 4, Rating = "B", Mass = 16, Integrity = 120, Power = 0.38, BootTime = 10, OptMass = 438, ThermL = 18, MaxFuelPerJump = 2.5, LinearConstant = 0.01, PowerConstant = 2.3 } },
                { "int_hyperdrive_size4_class5", new ShipModule(128064117,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 4 Rating A"){ Cost = 1610080, Class = 4, Rating = "A", Mass = 10, Integrity = 100, Power = 0.45, BootTime = 10, OptMass = 525, ThermL = 18, MaxFuelPerJump = 3, LinearConstant = 0.012, PowerConstant = 2.3 } },
                { "int_hyperdrive_size5_class1", new ShipModule(128064118,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 5 Rating E"){ Cost = 63010, Class = 5, Rating = "E", Mass = 20, Integrity = 86, Power = 0.32, BootTime = 10, OptMass = 560, ThermL = 27, MaxFuelPerJump = 3.3, LinearConstant = 0.011, PowerConstant = 2.45 } },
                { "int_hyperdrive_size5_class2", new ShipModule(128064119,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 5 Rating D"){ Cost = 189040, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.36, BootTime = 10, OptMass = 630, ThermL = 27, MaxFuelPerJump = 3.3, LinearConstant = 0.01, PowerConstant = 2.45 } },
                { "int_hyperdrive_size5_class3", new ShipModule(128064120,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 5 Rating C"){ Cost = 567110, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.4, BootTime = 10, OptMass = 700, ThermL = 27, MaxFuelPerJump = 3.3, LinearConstant = 0.008, PowerConstant = 2.45 } },
                { "int_hyperdrive_size5_class4", new ShipModule(128064121,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 5 Rating B"){ Cost = 1701320, Class = 5, Rating = "B", Mass = 32, Integrity = 144, Power = 0.5, BootTime = 10, OptMass = 875, ThermL = 27, MaxFuelPerJump = 4.1, LinearConstant = 0.01, PowerConstant = 2.45 } },
                { "int_hyperdrive_size5_class5", new ShipModule(128064122,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 5 Rating A"){ Cost = 5103950, Class = 5, Rating = "A", Mass = 20, Integrity = 120, Power = 0.6, BootTime = 10, OptMass = 1050, ThermL = 27, MaxFuelPerJump = 5, LinearConstant = 0.012, PowerConstant = 2.45 } },
                { "int_hyperdrive_size6_class1", new ShipModule(128064123,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 6 Rating E"){ Cost = 199750, Class = 6, Rating = "E", Mass = 40, Integrity = 102, Power = 0.4, BootTime = 10, OptMass = 960, ThermL = 37, MaxFuelPerJump = 5.3, LinearConstant = 0.011, PowerConstant = 2.6 } },
                { "int_hyperdrive_size6_class2", new ShipModule(128064124,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 6 Rating D"){ Cost = 599240, Class = 6, Rating = "D", Mass = 16, Integrity = 90, Power = 0.45, BootTime = 10, OptMass = 1080, ThermL = 37, MaxFuelPerJump = 5.3, LinearConstant = 0.01, PowerConstant = 2.6 } },
                { "int_hyperdrive_size6_class3", new ShipModule(128064125,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 6 Rating C"){ Cost = 1797730, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 0.5, BootTime = 10, OptMass = 1200, ThermL = 37, MaxFuelPerJump = 5.3, LinearConstant = 0.008, PowerConstant = 2.6 } },
                { "int_hyperdrive_size6_class4", new ShipModule(128064126,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 6 Rating B"){ Cost = 5393180, Class = 6, Rating = "B", Mass = 64, Integrity = 170, Power = 0.63, BootTime = 10, OptMass = 1500, ThermL = 37, MaxFuelPerJump = 6.6, LinearConstant = 0.01, PowerConstant = 2.6 } },
                { "int_hyperdrive_size6_class5", new ShipModule(128064127,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 6 Rating A"){ Cost = 16179530, Class = 6, Rating = "A", Mass = 40, Integrity = 141, Power = 0.75, BootTime = 10, OptMass = 1800, ThermL = 37, MaxFuelPerJump = 8, LinearConstant = 0.012, PowerConstant = 2.6 } },
                { "int_hyperdrive_size7_class1", new ShipModule(128064128,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 7 Rating E"){ Cost = 633200, Class = 7, Rating = "E", Mass = 80, Integrity = 118, Power = 0.48, BootTime = 10, OptMass = 1440, ThermL = 43, MaxFuelPerJump = 8.5, LinearConstant = 0.011, PowerConstant = 2.75 } },
                { "int_hyperdrive_size7_class2", new ShipModule(128064129,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 7 Rating D"){ Cost = 1899600, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.54, BootTime = 10, OptMass = 1620, ThermL = 43, MaxFuelPerJump = 8.5, LinearConstant = 0.01, PowerConstant = 2.75 } },
                { "int_hyperdrive_size7_class3", new ShipModule(128064130,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 7 Rating C"){ Cost = 5698790, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.6, BootTime = 10, OptMass = 1800, ThermL = 43, MaxFuelPerJump = 8.5, LinearConstant = 0.008, PowerConstant = 2.75 } },
                { "int_hyperdrive_size7_class4", new ShipModule(128064131,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 7 Rating B"){ Cost = 17096370, Class = 7, Rating = "B", Mass = 128, Integrity = 197, Power = 0.75, BootTime = 10, OptMass = 2250, ThermL = 43, MaxFuelPerJump = 10.6, LinearConstant = 0.01, PowerConstant = 2.75 } },
                { "int_hyperdrive_size7_class5", new ShipModule(128064132,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive Class 7 Rating A"){ Cost = 51289110, Class = 7, Rating = "A", Mass = 80, Integrity = 164, Power = 0.9, BootTime = 10, OptMass = 2700, ThermL = 43, MaxFuelPerJump = 12.8, LinearConstant = 0.012, PowerConstant = 2.75 } },

                { "int_hyperdrive_size8_class1", new ShipModule(128064133, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 8 Rating E")  { Power=0.56, Mass=160 } },
                { "int_hyperdrive_size8_class2", new ShipModule(128064134, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 8 Rating D") { Power=0.63, Mass=64 } },
                { "int_hyperdrive_size8_class3", new ShipModule(128064135, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 8 Rating C") { Power=0.7, Mass=160 } },
                { "int_hyperdrive_size8_class4", new ShipModule(128064136, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 8 Rating B") { Power=0.88, Mass=256 } },
                { "int_hyperdrive_size8_class5", new ShipModule(128064137, ShipModule.ModuleTypes.FrameShiftDrive, "Frame Shift Drive Class 8 Rating A") { Power=1.05, Mass=160 } },

                { "int_hyperdrive_overcharge_size2_class1", new ShipModule(129030577,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 2 Rating E"){ Cost = 21360, Class = 2, Rating = "E", Mass = 2.5, Integrity = 51, Power = 0.2, BootTime = 10, OptMass = 60, ThermL = 10, MaxFuelPerJump = 0.6, LinearConstant = 0.008, PowerConstant = 2, SCOSpeedIncrease = 25, SCOAccelerationRate = 0.08, SCOHeatGenerationRate = 42.07, SCOControlInterference = 0.25 } },
                { "int_hyperdrive_overcharge_size2_class2", new ShipModule(129030578,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 2 Rating D"){ Cost = 64090, Class = 2, Rating = "D", Mass = 2.5, Integrity = 57, Power = 0.25, BootTime = 10, OptMass = 90, ThermL = 10, MaxFuelPerJump = 0.9, LinearConstant = 0.012, PowerConstant = 2, SCOSpeedIncrease = 142, SCOAccelerationRate = 0.09, SCOHeatGenerationRate = 38, SCOControlInterference = 0.24 } },
                { "int_hyperdrive_overcharge_size2_class3", new ShipModule(129030487,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 2 Rating C"){ Cost = 21360, Class = 2, Rating = "C", Mass = 2.5, Integrity = 57, Power = 0.25, BootTime = 10, OptMass = 90, ThermL = 10, MaxFuelPerJump = 0.9, LinearConstant = 0.012, PowerConstant = 2, SCOSpeedIncrease = 142, SCOAccelerationRate = 0.09, SCOHeatGenerationRate = 27.14, SCOControlInterference = 0.24 } },
                { "int_hyperdrive_overcharge_size2_class4", new ShipModule(129030579,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 2 Rating B"){ Cost = 64090, Class = 2, Rating = "B", Mass = 2.5, Integrity = 57, Power = 0.25, BootTime = 10, OptMass = 90, ThermL = 10, MaxFuelPerJump = 0.9, LinearConstant = 0.012, PowerConstant = 2, SCOSpeedIncrease = 142, SCOAccelerationRate = 0.09, SCOHeatGenerationRate = 38, SCOControlInterference = 0.24 } },
                { "int_hyperdrive_overcharge_size2_class5", new ShipModule(129030580,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 2 Rating A"){ Cost = 192270, Class = 2, Rating = "A", Mass = 2.5, Integrity = 64, Power = 0.3, BootTime = 10, OptMass = 100, ThermL = 10, MaxFuelPerJump = 1, LinearConstant = 0.013, PowerConstant = 2, SCOSpeedIncrease = 160, SCOAccelerationRate = 0.09, SCOHeatGenerationRate = 36.1, SCOControlInterference = 0.23 } },

                { "int_hyperdrive_overcharge_size3_class1", new ShipModule(129030581,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 3 Rating E"){ Cost = 67720, Class = 3, Rating = "E", Mass = 5, Integrity = 64, Power = 0.3, BootTime = 10, OptMass = 100, ThermL = 14, MaxFuelPerJump = 1.2, LinearConstant = 0.008, PowerConstant = 2.15, SCOSpeedIncrease = 20, SCOAccelerationRate = 0.06, SCOHeatGenerationRate = 58.38, SCOControlInterference = 0.3 } },
                { "int_hyperdrive_overcharge_size3_class2", new ShipModule(129030582,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 3 Rating D"){ Cost = 203170, Class = 3, Rating = "D", Mass = 2, Integrity = 70, Power = 0.38, BootTime = 10, OptMass = 150, ThermL = 14, MaxFuelPerJump = 1.8, LinearConstant = 0.012, PowerConstant = 2.15, SCOSpeedIncrease = 120, SCOAccelerationRate = 0.07, SCOHeatGenerationRate = 53, SCOControlInterference = 0.29 } },
                { "int_hyperdrive_overcharge_size3_class4", new ShipModule(129030583,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 3 Rating B"){ Cost = 203170, Class = 3, Rating = "B", Mass = 5, Integrity = 70, Power = 0.38, BootTime = 10, OptMass = 150, ThermL = 14, MaxFuelPerJump = 1.8, LinearConstant = 0.012, PowerConstant = 2.15, SCOSpeedIncrease = 120, SCOAccelerationRate = 0.07, SCOHeatGenerationRate = 53, SCOControlInterference = 0.29 } },
                { "int_hyperdrive_overcharge_size3_class3", new ShipModule(129030486,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 3 Rating C"){ Cost = 67720, Class = 3, Rating = "C", Mass = 5, Integrity = 70, Power = 0.38, BootTime = 10, OptMass = 150, ThermL = 14, MaxFuelPerJump = 1.8, LinearConstant = 0.012, PowerConstant = 2.15, SCOSpeedIncrease = 120, SCOAccelerationRate = 0.07, SCOHeatGenerationRate = 37.86, SCOControlInterference = 0.29 } },
                { "int_hyperdrive_overcharge_size3_class5", new ShipModule(129030584,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 3 Rating A"){ Cost = 609500, Class = 3, Rating = "A", Mass = 5, Integrity = 80, Power = 0.45, BootTime = 10, OptMass = 167, ThermL = 14, MaxFuelPerJump = 1.9, LinearConstant = 0.013, PowerConstant = 2.15, SCOSpeedIncrease = 138, SCOAccelerationRate = 0.07, SCOHeatGenerationRate = 50.35, SCOControlInterference = 0.28 } },

                { "int_hyperdrive_overcharge_size4_class1", new ShipModule(129030585,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 4 Rating E"){ Cost = 214680, Class = 4, Rating = "E", Mass = 10, Integrity = 80, Power = 0.3, BootTime = 10, OptMass = 350, ThermL = 18, MaxFuelPerJump = 2, LinearConstant = 0.008, PowerConstant = 2.3, SCOSpeedIncrease = 15, SCOAccelerationRate = 0.05, SCOHeatGenerationRate = 66.43, SCOControlInterference = 0.37 } },
                { "int_hyperdrive_overcharge_size4_class2", new ShipModule(129030586,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 4 Rating D"){ Cost = 644030, Class = 4, Rating = "D", Mass = 4, Integrity = 90, Power = 0.38, BootTime = 10, OptMass = 525, ThermL = 18, MaxFuelPerJump = 3, LinearConstant = 0.012, PowerConstant = 2.3, SCOSpeedIncrease = 100, SCOAccelerationRate = 0.06, SCOHeatGenerationRate = 60, SCOControlInterference = 0.35 } },
                { "int_hyperdrive_overcharge_size4_class3", new ShipModule(129030485,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 4 Rating C"){ Cost = 214680, Class = 4, Rating = "C", Mass = 10, Integrity = 90, Power = 0.38, BootTime = 10, OptMass = 525, ThermL = 18, MaxFuelPerJump = 3, LinearConstant = 0.012, PowerConstant = 2.3, SCOSpeedIncrease = 100, SCOAccelerationRate = 0.06, SCOHeatGenerationRate = 42.86, SCOControlInterference = 0.35 } },
                { "int_hyperdrive_overcharge_size4_class4", new ShipModule(129030587,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 4 Rating B"){ Cost = 644030, Class = 4, Rating = "B", Mass = 10, Integrity = 90, Power = 0.38, BootTime = 10, OptMass = 525, ThermL = 18, MaxFuelPerJump = 3, LinearConstant = 0.012, PowerConstant = 2.3, SCOSpeedIncrease = 100, SCOAccelerationRate = 0.06, SCOHeatGenerationRate = 60, SCOControlInterference = 0.35 } },
                { "int_hyperdrive_overcharge_size4_class5", new ShipModule(129030588,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 4 Rating A"){ Cost = 1932100, Class = 4, Rating = "A", Mass = 10, Integrity = 100, Power = 0.45, BootTime = 10, OptMass = 585, ThermL = 18, MaxFuelPerJump = 3.2, LinearConstant = 0.013, PowerConstant = 2.3, SCOSpeedIncrease = 107, SCOAccelerationRate = 0.06, SCOHeatGenerationRate = 57, SCOControlInterference = 0.34 } },

                { "int_hyperdrive_overcharge_size5_class1", new ShipModule(129030589,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 5 Rating E"){ Cost = 623820, Class = 5, Rating = "E", Mass = 20, Integrity = 95, Power = 0.45, BootTime = 10, OptMass = 700, ThermL = 27, MaxFuelPerJump = 3.3, LinearConstant = 0.008, PowerConstant = 2.45, SCOSpeedIncrease = 0, SCOAccelerationRate = 0.04, SCOHeatGenerationRate = 108.5, SCOControlInterference = 0.42 } },
                { "int_hyperdrive_overcharge_size5_class2", new ShipModule(129030590,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 5 Rating D"){ Cost = 2041580, Class = 5, Rating = "D", Mass = 8, Integrity = 110, Power = 0.5, BootTime = 10, OptMass = 1050, ThermL = 27, MaxFuelPerJump = 5, LinearConstant = 0.012, PowerConstant = 2.45, SCOSpeedIncrease = 80, SCOAccelerationRate = 0.055, SCOHeatGenerationRate = 98, SCOControlInterference = 0.4 } },
                { "int_hyperdrive_overcharge_size5_class3", new ShipModule(129030474,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 5 Rating C"){ Cost = 623820, Class = 5, Rating = "C", Mass = 20, Integrity = 110, Power = 0.5, BootTime = 10, OptMass = 1050, ThermL = 27, MaxFuelPerJump = 5, LinearConstant = 0.012, PowerConstant = 2.45, SCOSpeedIncrease = 80, SCOAccelerationRate = 0.055, SCOHeatGenerationRate = 70, SCOControlInterference = 0.4 } },
                { "int_hyperdrive_overcharge_size5_class4", new ShipModule(129030591,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 5 Rating B"){ Cost = 2041580, Class = 5, Rating = "B", Mass = 20, Integrity = 110, Power = 0.5, BootTime = 10, OptMass = 1050, ThermL = 27, MaxFuelPerJump = 5, LinearConstant = 0.012, PowerConstant = 2.45, SCOSpeedIncrease = 80, SCOAccelerationRate = 0.055, SCOHeatGenerationRate = 98, SCOControlInterference = 0.4 } },
                { "int_hyperdrive_overcharge_size5_class5", new ShipModule(129030592,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 5 Rating A"){ Cost = 6124740, Class = 5, Rating = "A", Mass = 20, Integrity = 120, Power = 0.6, BootTime = 10, OptMass = 1175, ThermL = 27, MaxFuelPerJump = 5.2, LinearConstant = 0.013, PowerConstant = 2.45, SCOSpeedIncrease = 95, SCOAccelerationRate = 0.055, SCOHeatGenerationRate = 93.1, SCOControlInterference = 0.39 } },

                { "int_hyperdrive_overcharge_size6_class1", new ShipModule(129030593,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 6 Rating E"){ Cost = 2157270, Class = 6, Rating = "E", Mass = 40, Integrity = 113, Power = 0.5, BootTime = 10, OptMass = 1200, ThermL = 37, MaxFuelPerJump = 5.3, LinearConstant = 0.008, PowerConstant = 2.6, SCOSpeedIncrease = 0, SCOAccelerationRate = 0.045, SCOHeatGenerationRate = 139.5, SCOControlInterference = 0.67 } },
                { "int_hyperdrive_overcharge_size6_class2", new ShipModule(129030594,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 6 Rating D"){ Cost = 6471810, Class = 6, Rating = "D", Mass = 16, Integrity = 130, Power = 0.63, BootTime = 10, OptMass = 1800, ThermL = 37, MaxFuelPerJump = 8, LinearConstant = 0.012, PowerConstant = 2.6, SCOSpeedIncrease = 62, SCOAccelerationRate = 0.05, SCOHeatGenerationRate = 126, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size6_class3", new ShipModule(129030484,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 6 Rating C"){ Cost = 2157270, Class = 6, Rating = "C", Mass = 40, Integrity = 130, Power = 0.63, BootTime = 10, OptMass = 1800, ThermL = 37, MaxFuelPerJump = 8, LinearConstant = 0.012, PowerConstant = 2.6, SCOSpeedIncrease = 62, SCOAccelerationRate = 0.05, SCOHeatGenerationRate = 90, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size6_class4", new ShipModule(129030595,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 6 Rating B"){ Cost = 6471810, Class = 6, Rating = "B", Mass = 40, Integrity = 130, Power = 0.63, BootTime = 10, OptMass = 1800, ThermL = 37, MaxFuelPerJump = 8, LinearConstant = 0.012, PowerConstant = 2.6, SCOSpeedIncrease = 62, SCOAccelerationRate = 0.05, SCOHeatGenerationRate = 126, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size6_class5", new ShipModule(129030596,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 6 Rating A"){ Cost = 19415440, Class = 6, Rating = "A", Mass = 40, Integrity = 141, Power = 0.75, BootTime = 10, OptMass = 2000, ThermL = 37, MaxFuelPerJump = 8.3, LinearConstant = 0.013, PowerConstant = 2.6, SCOSpeedIncrease = 76, SCOAccelerationRate = 0.05, SCOHeatGenerationRate = 119.7, SCOControlInterference = 0.62 } },

                { "int_hyperdrive_overcharge_size7_class1", new ShipModule(129030597,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 7 Rating E"){ Cost = 6838550, Class = 7, Rating = "E", Mass = 80, Integrity = 131, Power = 0.6, BootTime = 10, OptMass = 1800, ThermL = 43, MaxFuelPerJump = 8.5, LinearConstant = 0.008, PowerConstant = 2.75, SCOSpeedIncrease = 0, SCOAccelerationRate = 0.03, SCOHeatGenerationRate = 143.93, SCOControlInterference = 0.67 } },
                { "int_hyperdrive_overcharge_size7_class2", new ShipModule(129030598,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 7 Rating D"){ Cost = 20515650, Class = 7, Rating = "D", Mass = 32, Integrity = 150, Power = 0.75, BootTime = 10, OptMass = 2700, ThermL = 43, MaxFuelPerJump = 12.8, LinearConstant = 0.012, PowerConstant = 2.75, SCOSpeedIncrease = 46, SCOAccelerationRate = 0.04, SCOHeatGenerationRate = 130, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size7_class3", new ShipModule(129030483,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 7 Rating C"){ Cost = 6838550, Class = 7, Rating = "C", Mass = 80, Integrity = 150, Power = 0.75, BootTime = 10, OptMass = 2700, ThermL = 43, MaxFuelPerJump = 12.8, LinearConstant = 0.012, PowerConstant = 2.75, SCOSpeedIncrease = 46, SCOAccelerationRate = 0.04, SCOHeatGenerationRate = 92.86, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size7_class4", new ShipModule(129030599,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 7 Rating B"){ Cost = 20515650, Class = 7, Rating = "B", Mass = 80, Integrity = 150, Power = 0.75, BootTime = 10, OptMass = 2700, ThermL = 43, MaxFuelPerJump = 12.8, LinearConstant = 0.012, PowerConstant = 2.75, SCOSpeedIncrease = 46, SCOAccelerationRate = 0.04, SCOHeatGenerationRate = 130, SCOControlInterference = 0.64 } },
                { "int_hyperdrive_overcharge_size7_class5", new ShipModule(129030600,ShipModule.ModuleTypes.FrameShiftDrive,"Frame Shift Drive (SCO) Class 7 Rating A"){ Cost = 61546940, Class = 7, Rating = "A", Mass = 80, Integrity = 164, Power = 0.9, BootTime = 10, OptMass = 3000, ThermL = 43, MaxFuelPerJump = 13.1, LinearConstant = 0.013, PowerConstant = 2.75, SCOSpeedIncrease = 58, SCOAccelerationRate = 0.04, SCOHeatGenerationRate = 123.5, SCOControlInterference = 0.62 } },


                // wake scanner

                { "hpt_cloudscanner_size0_class1", new ShipModule(128662525,ShipModule.ModuleTypes.FrameShiftWakeScanner,"Frame Shift Wake Scanner Rating E"){ Cost = 13540, Class = 0, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.2, BootTime = 1, Range = 2000, Angle = 15, Time = 10 } },
                { "hpt_cloudscanner_size0_class2", new ShipModule(128662526,ShipModule.ModuleTypes.FrameShiftWakeScanner,"Frame Shift Wake Scanner Rating D"){ Cost = 40630, Class = 0, Rating = "D", Mass = 1.3, Integrity = 24, Power = 0.4, BootTime = 1, Range = 2500, Angle = 15, Time = 10 } },
                { "hpt_cloudscanner_size0_class3", new ShipModule(128662527,ShipModule.ModuleTypes.FrameShiftWakeScanner,"Frame Shift Wake Scanner Rating C"){ Cost = 121900, Class = 0, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.8, BootTime = 1, Range = 3000, Angle = 15, Time = 10 } },
                { "hpt_cloudscanner_size0_class4", new ShipModule(128662528,ShipModule.ModuleTypes.FrameShiftWakeScanner,"Frame Shift Wake Scanner Rating B"){ Cost = 365700, Class = 0, Rating = "B", Mass = 1.3, Integrity = 56, Power = 1.6, BootTime = 1, Range = 3500, Angle = 15, Time = 10 } },
                { "hpt_cloudscanner_size0_class5", new ShipModule(128662529,ShipModule.ModuleTypes.FrameShiftWakeScanner,"Frame Shift Wake Scanner Rating A"){ Cost = 1097100, Class = 0, Rating = "A", Mass = 1.3, Integrity = 48, Power = 3.2, BootTime = 1, Range = 4000, Angle = 15, Time = 10 } },

                // life support

                { "int_lifesupport_size1_class1_free", new ShipModule(128666638, ShipModule.ModuleTypes.LifeSupport, "Life Support Class 1 Rating E" ) { Power=0.32, Mass=1.3, Integrity=32, Time=300, BootTime=1 } },
                { "int_lifesupport_size1_class1", new ShipModule(128064138,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.32, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size1_class2", new ShipModule(128064139,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 1 Rating D"){ Cost = 1290, Class = 1, Rating = "D", Mass = 0.5, Integrity = 36, Power = 0.36, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size1_class3", new ShipModule(128064140,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 1 Rating C"){ Cost = 3230, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.4, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size1_class4", new ShipModule(128064141,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 1 Rating B"){ Cost = 8080, Class = 1, Rating = "B", Mass = 2, Integrity = 44, Power = 0.44, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size1_class5", new ShipModule(128064142,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 1 Rating A"){ Cost = 20200, Class = 1, Rating = "A", Mass = 1.3, Integrity = 48, Power = 0.48, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size2_class1", new ShipModule(128064143,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 2 Rating E"){ Cost = 1450, Class = 2, Rating = "E", Mass = 2.5, Integrity = 41, Power = 0.37, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size2_class2", new ShipModule(128064144,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 2 Rating D"){ Cost = 3620, Class = 2, Rating = "D", Mass = 1, Integrity = 46, Power = 0.41, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size2_class3", new ShipModule(128064145,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 2 Rating C"){ Cost = 9050, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.46, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size2_class4", new ShipModule(128064146,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 2 Rating B"){ Cost = 22620, Class = 2, Rating = "B", Mass = 4, Integrity = 56, Power = 0.51, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size2_class5", new ShipModule(128064147,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 2 Rating A"){ Cost = 56550, Class = 2, Rating = "A", Mass = 2.5, Integrity = 61, Power = 0.55, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size3_class1", new ShipModule(128064148,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 3 Rating E"){ Cost = 4050, Class = 3, Rating = "E", Mass = 5, Integrity = 51, Power = 0.42, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size3_class2", new ShipModule(128064149,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 3 Rating D"){ Cost = 10130, Class = 3, Rating = "D", Mass = 2, Integrity = 58, Power = 0.48, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size3_class3", new ShipModule(128064150,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 3 Rating C"){ Cost = 25330, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.53, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size3_class4", new ShipModule(128064151,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 3 Rating B"){ Cost = 63330, Class = 3, Rating = "B", Mass = 8, Integrity = 70, Power = 0.58, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size3_class5", new ShipModule(128064152,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 3 Rating A"){ Cost = 158330, Class = 3, Rating = "A", Mass = 5, Integrity = 77, Power = 0.64, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size4_class1", new ShipModule(128064153,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 4 Rating E"){ Cost = 11350, Class = 4, Rating = "E", Mass = 10, Integrity = 64, Power = 0.5, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size4_class2", new ShipModule(128064154,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 4 Rating D"){ Cost = 28370, Class = 4, Rating = "D", Mass = 4, Integrity = 72, Power = 0.56, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size4_class3", new ShipModule(128064155,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 4 Rating C"){ Cost = 70930, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 0.62, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size4_class4", new ShipModule(128064156,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 4 Rating B"){ Cost = 177330, Class = 4, Rating = "B", Mass = 16, Integrity = 88, Power = 0.68, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size4_class5", new ShipModule(128064157,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 4 Rating A"){ Cost = 443330, Class = 4, Rating = "A", Mass = 10, Integrity = 96, Power = 0.74, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size5_class1", new ShipModule(128064158,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 5 Rating E"){ Cost = 31780, Class = 5, Rating = "E", Mass = 20, Integrity = 77, Power = 0.57, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size5_class2", new ShipModule(128064159,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 5 Rating D"){ Cost = 79440, Class = 5, Rating = "D", Mass = 8, Integrity = 86, Power = 0.64, BootTime = 1, Time = 450 } },

                { "int_lifesupport_size5_class3", new ShipModule(128064160,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 5 Rating C"){ Cost = 198610, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.71, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size5_class4", new ShipModule(128064161,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 5 Rating B"){ Cost = 496530, Class = 5, Rating = "B", Mass = 32, Integrity = 106, Power = 0.78, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size5_class5", new ShipModule(128064162,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 5 Rating A"){ Cost = 1241320, Class = 5, Rating = "A", Mass = 20, Integrity = 115, Power = 0.85, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size6_class1", new ShipModule(128064163,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 6 Rating E"){ Cost = 88980, Class = 6, Rating = "E", Mass = 40, Integrity = 90, Power = 0.64, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size6_class2", new ShipModule(128064164,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 6 Rating D"){ Cost = 222440, Class = 6, Rating = "D", Mass = 16, Integrity = 102, Power = 0.72, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size6_class3", new ShipModule(128064165,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 6 Rating C"){ Cost = 556110, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 0.8, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size6_class4", new ShipModule(128064166,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 6 Rating B"){ Cost = 1390280, Class = 6, Rating = "B", Mass = 64, Integrity = 124, Power = 0.88, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size6_class5", new ShipModule(128064167,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 6 Rating A"){ Cost = 3475690, Class = 6, Rating = "A", Mass = 40, Integrity = 136, Power = 0.96, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size7_class1", new ShipModule(128064168,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 7 Rating E"){ Cost = 249140, Class = 7, Rating = "E", Mass = 80, Integrity = 105, Power = 0.72, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size7_class2", new ShipModule(128064169,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 7 Rating D"){ Cost = 622840, Class = 7, Rating = "D", Mass = 32, Integrity = 118, Power = 0.81, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size7_class3", new ShipModule(128064170,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 7 Rating C"){ Cost = 1557110, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.9, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size7_class4", new ShipModule(128064171,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 7 Rating B"){ Cost = 3892770, Class = 7, Rating = "B", Mass = 128, Integrity = 144, Power = 0.99, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size7_class5", new ShipModule(128064172,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 7 Rating A"){ Cost = 9731930, Class = 7, Rating = "A", Mass = 80, Integrity = 157, Power = 1.08, BootTime = 1, Time = 1500 } },
                { "int_lifesupport_size8_class1", new ShipModule(128064173,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 8 Rating E"){ Cost = 697590, Class = 8, Rating = "E", Mass = 160, Integrity = 120, Power = 0.8, BootTime = 1, Time = 300 } },
                { "int_lifesupport_size8_class2", new ShipModule(128064174,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 8 Rating D"){ Cost = 1743970, Class = 8, Rating = "D", Mass = 64, Integrity = 135, Power = 0.9, BootTime = 1, Time = 450 } },
                { "int_lifesupport_size8_class3", new ShipModule(128064175,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 8 Rating C"){ Cost = 4359900, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 1, BootTime = 1, Time = 600 } },
                { "int_lifesupport_size8_class4", new ShipModule(128064176,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 8 Rating B"){ Cost = 10899770, Class = 8, Rating = "B", Mass = 256, Integrity = 165, Power = 1.1, BootTime = 1, Time = 900 } },
                { "int_lifesupport_size8_class5", new ShipModule(128064177,ShipModule.ModuleTypes.LifeSupport,"Life Support Class 8 Rating A"){ Cost = 27249400, Class = 8, Rating = "A", Mass = 160, Integrity = 180, Power = 1.2, BootTime = 1, Time = 1500 } },

                // Limpet control

                { "int_dronecontrol_collection_size1_class1", new ShipModule(128671229,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 1 Rating E"){ Cost = 600, Class = 1, Rating = "E", Mass = 0.5, Integrity = 24, Power = 0.14, BootTime = 6, Limpets = 1, Range = 800, Time = 300, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size1_class2", new ShipModule(128671230,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 1 Rating D"){ Cost = 1200, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.18, BootTime = 6, Limpets = 1, Range = 600, Time = 600, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size1_class3", new ShipModule(128671231,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 1 Rating C"){ Cost = 2400, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.23, BootTime = 6, Limpets = 1, Range = 1000, Time = 510, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size1_class4", new ShipModule(128671232,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 1 Rating B"){ Cost = 4800, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.28, BootTime = 6, Limpets = 1, Range = 1400, Time = 420, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size1_class5", new ShipModule(128671233,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 1 Rating A"){ Cost = 9600, Class = 1, Rating = "A", Mass = 2, Integrity = 56, Power = 0.32, BootTime = 6, Limpets = 1, Range = 1200, Time = 720, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size3_class1", new ShipModule(128671234,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 3 Rating E"){ Cost = 5400, Class = 3, Rating = "E", Mass = 2, Integrity = 38, Power = 0.2, BootTime = 6, Limpets = 2, Range = 880, Time = 300, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size3_class2", new ShipModule(128671235,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 3 Rating D"){ Cost = 10800, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.27, BootTime = 6, Limpets = 2, Range = 660, Time = 600, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size3_class3", new ShipModule(128671236,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 3 Rating C"){ Cost = 21600, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.34, BootTime = 6, Limpets = 2, Range = 1100, Time = 510, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size3_class4", new ShipModule(128671237,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 3 Rating B"){ Cost = 43200, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.41, BootTime = 6, Limpets = 2, Range = 1540, Time = 420, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size3_class5", new ShipModule(128671238,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 3 Rating A"){ Cost = 86400, Class = 3, Rating = "A", Mass = 8, Integrity = 90, Power = 0.48, BootTime = 6, Limpets = 2, Range = 1320, Time = 720, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size5_class1", new ShipModule(128671239,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 5 Rating E"){ Cost = 48600, Class = 5, Rating = "E", Mass = 8, Integrity = 58, Power = 0.3, BootTime = 6, Limpets = 3, Range = 1040, Time = 300, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size5_class2", new ShipModule(128671240,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 5 Rating D"){ Cost = 97200, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.4, BootTime = 6, Limpets = 3, Range = 780, Time = 600, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size5_class3", new ShipModule(128671241,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 5 Rating C"){ Cost = 194400, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 6, Limpets = 3, Range = 1300, Time = 510, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size5_class4", new ShipModule(128671242,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 5 Rating B"){ Cost = 388800, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.6, BootTime = 6, Limpets = 3, Range = 1820, Time = 420, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size5_class5", new ShipModule(128671243,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 5 Rating A"){ Cost = 777600, Class = 5, Rating = "A", Mass = 32, Integrity = 134, Power = 0.7, BootTime = 6, Limpets = 3, Range = 1560, Time = 720, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size7_class1", new ShipModule(128671244,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 7 Rating E"){ Cost = 437400, Class = 7, Rating = "E", Mass = 32, Integrity = 79, Power = 0.41, BootTime = 6, Limpets = 4, Range = 1360, Time = 300, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size7_class2", new ShipModule(128671245,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 7 Rating D"){ Cost = 874800, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.55, BootTime = 6, Limpets = 4, Range = 1020, Time = 600, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size7_class3", new ShipModule(128671246,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 7 Rating C"){ Cost = 1749600, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.69, BootTime = 6, Limpets = 4, Range = 1700, Time = 510, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size7_class4", new ShipModule(128671247,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 7 Rating B"){ Cost = 3499200, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.83, BootTime = 6, Limpets = 4, Range = 2380, Time = 420, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_dronecontrol_collection_size7_class5", new ShipModule(128671248,ShipModule.ModuleTypes.CollectorLimpetController,"Collector Limpet Controller Class 7 Rating A"){ Cost = 6998400, Class = 7, Rating = "A", Mass = 128, Integrity = 183, Power = 0.97, BootTime = 6, Limpets = 4, Range = 2040, Time = 720, Speed = 200, MultiTargetSpeed = 60 } },

                { "int_dronecontrol_fueltransfer_size1_class1", new ShipModule(128671249,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 1 Rating E"){ Cost = 600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 24, Power = 0.18, BootTime = 10, Limpets = 1, Range = 600, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size1_class2", new ShipModule(128671250,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 1 Rating D"){ Cost = 1200, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.14, BootTime = 10, Limpets = 1, Range = 800, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size1_class3", new ShipModule(128671251,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 1 Rating C"){ Cost = 2400, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.23, BootTime = 10, Limpets = 1, Range = 1000, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size1_class4", new ShipModule(128671252,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 1 Rating B"){ Cost = 4800, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.32, BootTime = 10, Limpets = 1, Range = 1200, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size1_class5", new ShipModule(128671253,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 1 Rating A"){ Cost = 9600, Class = 1, Rating = "A", Mass = 1.3, Integrity = 56, Power = 0.28, BootTime = 10, Limpets = 1, Range = 1400, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size3_class1", new ShipModule(128671254,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 3 Rating E"){ Cost = 5400, Class = 3, Rating = "E", Mass = 5, Integrity = 38, Power = 0.27, BootTime = 10, Limpets = 2, Range = 660, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size3_class2", new ShipModule(128671255,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 3 Rating D"){ Cost = 10800, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.2, BootTime = 10, Limpets = 2, Range = 880, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size3_class3", new ShipModule(128671256,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 3 Rating C"){ Cost = 21600, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.34, BootTime = 10, Limpets = 2, Range = 1100, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size3_class4", new ShipModule(128671257,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 3 Rating B"){ Cost = 43200, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.48, BootTime = 10, Limpets = 2, Range = 1320, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size3_class5", new ShipModule(128671258,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 3 Rating A"){ Cost = 86400, Class = 3, Rating = "A", Mass = 5, Integrity = 90, Power = 0.41, BootTime = 10, Limpets = 2, Range = 1540, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size5_class1", new ShipModule(128671259,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 5 Rating E"){ Cost = 48600, Class = 5, Rating = "E", Mass = 20, Integrity = 58, Power = 0.4, BootTime = 10, Limpets = 4, Range = 780, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size5_class2", new ShipModule(128671260,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 5 Rating D"){ Cost = 97200, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.3, BootTime = 10, Limpets = 4, Range = 1040, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size5_class3", new ShipModule(128671261,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 5 Rating C"){ Cost = 194400, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 10, Limpets = 4, Range = 1300, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size5_class4", new ShipModule(128671262,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 5 Rating B"){ Cost = 388800, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.72, BootTime = 10, Limpets = 4, Range = 1560, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size5_class5", new ShipModule(128671263,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 5 Rating A"){ Cost = 777600, Class = 5, Rating = "A", Mass = 20, Integrity = 134, Power = 0.6, BootTime = 10, Limpets = 4, Range = 1820, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size7_class1", new ShipModule(128671264,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 7 Rating E"){ Cost = 437400, Class = 7, Rating = "E", Mass = 80, Integrity = 79, Power = 0.55, BootTime = 10, Limpets = 8, Range = 1020, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size7_class2", new ShipModule(128671265,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 7 Rating D"){ Cost = 874800, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.41, BootTime = 10, Limpets = 8, Range = 1360, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size7_class3", new ShipModule(128671266,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 7 Rating C"){ Cost = 1749600, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.69, BootTime = 10, Limpets = 8, Range = 1700, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size7_class4", new ShipModule(128671267,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 7 Rating B"){ Cost = 3499200, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 10, Limpets = 8, Range = 2040, Time = 60, Speed = 200, FuelTransfer = 1 } },
                { "int_dronecontrol_fueltransfer_size7_class5", new ShipModule(128671268,ShipModule.ModuleTypes.FuelTransferLimpetController,"Fuel Transfer Limpet Controller Class 7 Rating A"){ Cost = 6998400, Class = 7, Rating = "A", Mass = 80, Integrity = 183, Power = 0.83, BootTime = 10, Limpets = 8, Range = 2380, Time = 60, Speed = 200, FuelTransfer = 1 } },

                { "int_dronecontrol_resourcesiphon", new ShipModule(128066402,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Limpet Control"){ Cost = 18040, Class = 1, Rating = "I", Integrity = 20, Power = 0.4, BootTime = 0, Limpets = 2, Range = 1000, Time = 60, Speed = 200, HackTime = 5, MinCargo = 1, MaxCargo = 2 } },

                { "int_dronecontrol_resourcesiphon_size1_class1", new ShipModule(128066532,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 1 Rating E"){ Cost = 600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.12, BootTime = 3, Limpets = 2, TargetRange = 1500, Range = 1600, Time = 120, Speed = 500, HackTime = 22, MinCargo = 1, MaxCargo = 6 } },
                { "int_dronecontrol_resourcesiphon_size1_class2", new ShipModule(128066533,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 1 Rating D"){ Cost = 1200, Class = 1, Rating = "D", Mass = 0.5, Integrity = 24, Power = 0.16, BootTime = 3, Limpets = 1, TargetRange = 2000, Range = 2100, Time = 120, Speed = 500, HackTime = 19, MinCargo = 2, MaxCargo = 7 } },
                { "int_dronecontrol_resourcesiphon_size1_class3", new ShipModule(128066534,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 1 Rating C"){ Cost = 2400, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.2, BootTime = 3, Limpets = 1, TargetRange = 2500, Range = 2600, Time = 120, Speed = 500, HackTime = 16, MinCargo = 3, MaxCargo = 8 } },
                { "int_dronecontrol_resourcesiphon_size1_class4", new ShipModule(128066535,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 1 Rating B"){ Cost = 4800, Class = 1, Rating = "B", Mass = 2, Integrity = 56, Power = 0.24, BootTime = 3, Limpets = 2, TargetRange = 3000, Range = 3100, Time = 120, Speed = 500, HackTime = 13, MinCargo = 4, MaxCargo = 9 } },
                { "int_dronecontrol_resourcesiphon_size1_class5", new ShipModule(128066536,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 1 Rating A"){ Cost = 9600, Class = 1, Rating = "A", Mass = 1.3, Integrity = 48, Power = 0.28, BootTime = 3, Limpets = 1, TargetRange = 3500, Range = 3600, Time = 120, Speed = 500, HackTime = 10, MinCargo = 5, MaxCargo = 10 } },
                { "int_dronecontrol_resourcesiphon_size3_class1", new ShipModule(128066537,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 3 Rating E"){ Cost = 5400, Class = 3, Rating = "E", Mass = 5, Integrity = 51, Power = 0.18, BootTime = 3, Limpets = 4, TargetRange = 1620, Range = 1720, Time = 120, Speed = 500, HackTime = 17, MinCargo = 1, MaxCargo = 6 } },
                { "int_dronecontrol_resourcesiphon_size3_class2", new ShipModule(128066538,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 3 Rating D"){ Cost = 10800, Class = 3, Rating = "D", Mass = 2, Integrity = 38, Power = 0.24, BootTime = 3, Limpets = 3, TargetRange = 2160, Range = 2260, Time = 120, Speed = 500, HackTime = 14, MinCargo = 2, MaxCargo = 7 } },
                { "int_dronecontrol_resourcesiphon_size3_class3", new ShipModule(128066539,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 3 Rating C"){ Cost = 21600, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.3, BootTime = 3, Limpets = 3, TargetRange = 2700, Range = 2800, Time = 120, Speed = 500, HackTime = 12, MinCargo = 3, MaxCargo = 8 } },
                { "int_dronecontrol_resourcesiphon_size3_class4", new ShipModule(128066540,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 3 Rating B"){ Cost = 43200, Class = 3, Rating = "B", Mass = 8, Integrity = 90, Power = 0.36, BootTime = 3, Limpets = 4, TargetRange = 3240, Range = 3340, Time = 120, Speed = 500, HackTime = 10, MinCargo = 4, MaxCargo = 9 } },
                { "int_dronecontrol_resourcesiphon_size3_class5", new ShipModule(128066541,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 3 Rating A"){ Cost = 86400, Class = 3, Rating = "A", Mass = 5, Integrity = 77, Power = 0.42, BootTime = 3, Limpets = 3, TargetRange = 3780, Range = 3870, Time = 120, Speed = 500, HackTime = 7, MinCargo = 5, MaxCargo = 10 } },
                { "int_dronecontrol_resourcesiphon_size5_class1", new ShipModule(128066542,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 5 Rating E"){ Cost = 48600, Class = 5, Rating = "E", Mass = 20, Integrity = 77, Power = 0.3, BootTime = 3, Limpets = 9, TargetRange = 1980, Range = 2080, Time = 120, Speed = 500, HackTime = 11, MinCargo = 1, MaxCargo = 6 } },
                { "int_dronecontrol_resourcesiphon_size5_class2", new ShipModule(128066543,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 5 Rating D"){ Cost = 97200, Class = 5, Rating = "D", Mass = 8, Integrity = 58, Power = 0.4, BootTime = 3, Limpets = 6, TargetRange = 2640, Range = 2740, Time = 120, Speed = 500, HackTime = 10, MinCargo = 2, MaxCargo = 7 } },
                { "int_dronecontrol_resourcesiphon_size5_class3", new ShipModule(128066544,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 5 Rating C"){ Cost = 194400, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 3, Limpets = 7, TargetRange = 3300, Range = 3400, Time = 120, Speed = 500, HackTime = 8, MinCargo = 3, MaxCargo = 8 } },
                { "int_dronecontrol_resourcesiphon_size5_class4", new ShipModule(128066545,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 5 Rating B"){ Cost = 388800, Class = 5, Rating = "B", Mass = 32, Integrity = 134, Power = 0.6, BootTime = 3, Limpets = 9, TargetRange = 3960, Range = 4060, Time = 120, Speed = 500, HackTime = 6, MinCargo = 4, MaxCargo = 9 } },
                { "int_dronecontrol_resourcesiphon_size5_class5", new ShipModule(128066546,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 5 Rating A"){ Cost = 777600, Class = 5, Rating = "A", Mass = 20, Integrity = 115, Power = 0.7, BootTime = 3, Limpets = 6, TargetRange = 4620, Range = 4720, Time = 120, Speed = 500, HackTime = 5, MinCargo = 5, MaxCargo = 10 } },
                { "int_dronecontrol_resourcesiphon_size7_class1", new ShipModule(128066547,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 7 Rating E"){ Cost = 437400, Class = 7, Rating = "E", Mass = 80, Integrity = 105, Power = 0.42, BootTime = 3, Limpets = 18, TargetRange = 2580, Range = 2680, Time = 120, Speed = 500, HackTime = 6, MinCargo = 1, MaxCargo = 6 } },
                { "int_dronecontrol_resourcesiphon_size7_class2", new ShipModule(128066548,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 7 Rating D"){ Cost = 874800, Class = 7, Rating = "D", Mass = 32, Integrity = 79, Power = 0.56, BootTime = 3, Limpets = 12, TargetRange = 3440, Range = 3540, Time = 120, Speed = 500, HackTime = 5, MinCargo = 2, MaxCargo = 7 } },
                { "int_dronecontrol_resourcesiphon_size7_class3", new ShipModule(128066549,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 7 Rating C"){ Cost = 1749600, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.7, BootTime = 3, Limpets = 15, TargetRange = 4300, Range = 4400, Time = 120, Speed = 500, HackTime = 4, MinCargo = 3, MaxCargo = 8 } },
                { "int_dronecontrol_resourcesiphon_size7_class4", new ShipModule(128066550,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 7 Rating B"){ Cost = 3499200, Class = 7, Rating = "B", Mass = 128, Integrity = 183, Power = 0.84, BootTime = 3, Limpets = 18, TargetRange = 5160, Range = 5260, Time = 120, Speed = 500, HackTime = 3, MinCargo = 4, MaxCargo = 9 } },
                { "int_dronecontrol_resourcesiphon_size7_class5", new ShipModule(128066551,ShipModule.ModuleTypes.HatchBreakerLimpetController,"Hatch Breaker Limpet Controller Class 7 Rating A"){ Cost = 6998400, Class = 7, Rating = "A", Mass = 80, Integrity = 157, Power = 0.98, BootTime = 3, Limpets = 12, TargetRange = 6020, Range = 6120, Time = 120, Speed = 500, HackTime = 2, MinCargo = 5, MaxCargo = 10 } },

                { "int_dronecontrol_prospector_size1_class1", new ShipModule(128671269,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 1 Rating E"){ Cost = 600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 24, Power = 0.18, BootTime = 4, Limpets = 1, Range = 3000, Speed = 200, MineBonus = 1 } },
                { "int_dronecontrol_prospector_size1_class2", new ShipModule(128671270,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 1 Rating D"){ Cost = 1200, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.14, BootTime = 4, Limpets = 1, Range = 4000, Speed = 200, MineBonus = 2 } },
                { "int_dronecontrol_prospector_size1_class3", new ShipModule(128671271,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 1 Rating C"){ Cost = 2400, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.23, BootTime = 4, Limpets = 1, Range = 5000, Speed = 200, MineBonus = 2.5 } },
                { "int_dronecontrol_prospector_size1_class4", new ShipModule(128671272,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 1 Rating B"){ Cost = 4800, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.32, BootTime = 4, Limpets = 1, Range = 6000, Speed = 200, MineBonus = 3 } },
                { "int_dronecontrol_prospector_size1_class5", new ShipModule(128671273,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 1 Rating A"){ Cost = 9600, Class = 1, Rating = "A", Mass = 1.3, Integrity = 56, Power = 0.28, BootTime = 4, Limpets = 1, Range = 7000, Speed = 200, MineBonus = 3.5 } },
                { "int_dronecontrol_prospector_size3_class1", new ShipModule(128671274,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 3 Rating E"){ Cost = 5400, Class = 3, Rating = "E", Mass = 5, Integrity = 38, Power = 0.27, BootTime = 4, Limpets = 2, Range = 3300, Speed = 200, MineBonus = 1 } },
                { "int_dronecontrol_prospector_size3_class2", new ShipModule(128671275,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 3 Rating D"){ Cost = 10800, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.2, BootTime = 4, Limpets = 2, Range = 4400, Speed = 200, MineBonus = 2 } },
                { "int_dronecontrol_prospector_size3_class3", new ShipModule(128671276,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 3 Rating C"){ Cost = 21600, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.34, BootTime = 4, Limpets = 2, Range = 5500, Speed = 200, MineBonus = 2.5 } },
                { "int_dronecontrol_prospector_size3_class4", new ShipModule(128671277,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 3 Rating B"){ Cost = 43200, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.48, BootTime = 4, Limpets = 2, Range = 6600, Speed = 200, MineBonus = 3 } },
                { "int_dronecontrol_prospector_size3_class5", new ShipModule(128671278,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 3 Rating A"){ Cost = 86400, Class = 3, Rating = "A", Mass = 5, Integrity = 90, Power = 0.41, BootTime = 4, Limpets = 2, Range = 7700, Speed = 200, MineBonus = 3.5 } },
                { "int_dronecontrol_prospector_size5_class1", new ShipModule(128671279,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 5 Rating E"){ Cost = 48600, Class = 5, Rating = "E", Mass = 20, Integrity = 58, Power = 0.4, BootTime = 4, Limpets = 4, Range = 3900, Speed = 200, MineBonus = 1 } },
                { "int_dronecontrol_prospector_size5_class2", new ShipModule(128671280,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 5 Rating D"){ Cost = 97200, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.3, BootTime = 4, Limpets = 4, Range = 5200, Speed = 200, MineBonus = 2 } },
                { "int_dronecontrol_prospector_size5_class3", new ShipModule(128671281,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 5 Rating C"){ Cost = 194400, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 4, Limpets = 4, Range = 6500, Speed = 200, MineBonus = 2.5 } },
                { "int_dronecontrol_prospector_size5_class4", new ShipModule(128671282,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 5 Rating B"){ Cost = 388800, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.72, BootTime = 4, Limpets = 4, Range = 7800, Speed = 200, MineBonus = 3 } },
                { "int_dronecontrol_prospector_size5_class5", new ShipModule(128671283,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 5 Rating A"){ Cost = 777600, Class = 5, Rating = "A", Mass = 20, Integrity = 134, Power = 0.6, BootTime = 4, Limpets = 4, Range = 9100, Speed = 200, MineBonus = 3.5 } },
                { "int_dronecontrol_prospector_size7_class1", new ShipModule(128671284,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 7 Rating E"){ Cost = 437400, Class = 7, Rating = "E", Mass = 80, Integrity = 79, Power = 0.55, BootTime = 4, Limpets = 8, Range = 5100, Speed = 200, MineBonus = 1 } },
                { "int_dronecontrol_prospector_size7_class2", new ShipModule(128671285,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 7 Rating D"){ Cost = 874800, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.41, BootTime = 4, Limpets = 8, Range = 6800, Speed = 200, MineBonus = 2 } },
                { "int_dronecontrol_prospector_size7_class3", new ShipModule(128671286,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 7 Rating C"){ Cost = 1749600, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.69, BootTime = 4, Limpets = 8, Range = 8500, Speed = 200, MineBonus = 2.5 } },
                { "int_dronecontrol_prospector_size7_class4", new ShipModule(128671287,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 7 Rating B"){ Cost = 3499200, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 4, Limpets = 8, Range = 10200, Speed = 200, MineBonus = 3 } },
                { "int_dronecontrol_prospector_size7_class5", new ShipModule(128671288,ShipModule.ModuleTypes.ProspectorLimpetController,"Prospector Limpet Controller Class 7 Rating A"){ Cost = 6998400, Class = 7, Rating = "A", Mass = 80, Integrity = 183, Power = 0.83, BootTime = 4, Limpets = 8, Range = 11900, Speed = 200, MineBonus = 3.5 } },

                { "int_dronecontrol_repair_size1_class1", new ShipModule(128777327,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 1 Rating E"){ Cost = 600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 24, Power = 0.18, BootTime = 10, Limpets = 1, Range = 600, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 60 } },
                { "int_dronecontrol_repair_size1_class2", new ShipModule(128777328,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 1 Rating D"){ Cost = 1200, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.14, BootTime = 10, Limpets = 1, Range = 800, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 60 } },
                { "int_dronecontrol_repair_size1_class3", new ShipModule(128777329,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 1 Rating C"){ Cost = 2400, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.23, BootTime = 10, Limpets = 1, Range = 1000, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 60 } },
                { "int_dronecontrol_repair_size1_class4", new ShipModule(128777330,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 1 Rating B"){ Cost = 4800, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.32, BootTime = 10, Limpets = 1, Range = 1200, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 60 } },
                { "int_dronecontrol_repair_size1_class5", new ShipModule(128777331,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 1 Rating A"){ Cost = 9600, Class = 1, Rating = "A", Mass = 1.3, Integrity = 56, Power = 0.28, BootTime = 10, Limpets = 1, Range = 1400, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 60 } },
                { "int_dronecontrol_repair_size3_class1", new ShipModule(128777332,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 3 Rating E"){ Cost = 5400, Class = 3, Rating = "E", Mass = 5, Integrity = 38, Power = 0.27, BootTime = 10, Limpets = 2, Range = 660, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },
                { "int_dronecontrol_repair_size3_class2", new ShipModule(128777333,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 3 Rating D"){ Cost = 10800, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.2, BootTime = 10, Limpets = 2, Range = 880, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },
                { "int_dronecontrol_repair_size3_class3", new ShipModule(128777334,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 3 Rating C"){ Cost = 21600, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.34, BootTime = 10, Limpets = 2, Range = 1100, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },
                { "int_dronecontrol_repair_size3_class4", new ShipModule(128777335,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 3 Rating B"){ Cost = 43200, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.48, BootTime = 10, Limpets = 2, Range = 1320, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },
                { "int_dronecontrol_repair_size3_class5", new ShipModule(128777336,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 3 Rating A"){ Cost = 86400, Class = 3, Rating = "A", Mass = 5, Integrity = 90, Power = 0.41, BootTime = 10, Limpets = 2, Range = 1540, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },
                { "int_dronecontrol_repair_size5_class1", new ShipModule(128777337,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 5 Rating E"){ Cost = 48600, Class = 5, Rating = "E", Mass = 20, Integrity = 58, Power = 0.4, BootTime = 10, Limpets = 3, Range = 780, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 310 } },
                { "int_dronecontrol_repair_size5_class2", new ShipModule(128777338,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 5 Rating D"){ Cost = 97200, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.3, BootTime = 10, Limpets = 3, Range = 1040, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 310 } },
                { "int_dronecontrol_repair_size5_class3", new ShipModule(128777339,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 5 Rating C"){ Cost = 194400, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 10, Limpets = 3, Range = 1300, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 310 } },
                { "int_dronecontrol_repair_size5_class4", new ShipModule(128777340,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 5 Rating B"){ Cost = 388800, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.72, BootTime = 10, Limpets = 3, Range = 1560, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 310 } },
                { "int_dronecontrol_repair_size5_class5", new ShipModule(128777341,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 5 Rating A"){ Cost = 777600, Class = 5, Rating = "A", Mass = 20, Integrity = 134, Power = 0.6, BootTime = 10, Limpets = 3, Range = 1820, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 310 } },
                { "int_dronecontrol_repair_size7_class1", new ShipModule(128777342,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 7 Rating E"){ Cost = 437400, Class = 7, Rating = "E", Mass = 80, Integrity = 79, Power = 0.55, BootTime = 10, Limpets = 4, Range = 1020, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 450 } },
                { "int_dronecontrol_repair_size7_class2", new ShipModule(128777343,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 7 Rating D"){ Cost = 874800, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.41, BootTime = 10, Limpets = 4, Range = 1360, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 450 } },
                { "int_dronecontrol_repair_size7_class3", new ShipModule(128777344,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 7 Rating C"){ Cost = 1749600, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.69, BootTime = 10, Limpets = 4, Range = 1700, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 450 } },
                { "int_dronecontrol_repair_size7_class4", new ShipModule(128777345,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 7 Rating B"){ Cost = 3499200, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 10, Limpets = 4, Range = 2040, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 450 } },
                { "int_dronecontrol_repair_size7_class5", new ShipModule(128777346,ShipModule.ModuleTypes.RepairLimpetController,"Repair Limpet Controller Class 7 Rating A"){ Cost = 6998400, Class = 7, Rating = "A", Mass = 80, Integrity = 183, Power = 0.83, BootTime = 10, Limpets = 4, Range = 2380, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 450 } },

                { "int_dronecontrol_unkvesselresearch", new ShipModule(128793116,ShipModule.ModuleTypes.ResearchLimpetController,"Research Limpet Controller"){ Cost = 1749600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 20, Power = 0.4, BootTime = 0, Limpets = 1, Range = 5000, Time = 300, Speed = 200 } },

                // More limpets

                { "int_dronecontrol_decontamination_size1_class1", new ShipModule(128793941,ShipModule.ModuleTypes.DecontaminationLimpetController,"Decontamination Limpet Controller Class 1 Rating E"){ Cost = 3600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 24, Power = 0.18, BootTime = 10, Limpets = 1, Range = 600, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 30 } },
                { "int_dronecontrol_decontamination_size3_class1", new ShipModule(128793942,ShipModule.ModuleTypes.DecontaminationLimpetController,"Decontamination Limpet Controller Class 3 Rating E"){ Cost = 16200, Class = 3, Rating = "E", Mass = 2, Integrity = 51, Power = 0.2, BootTime = 10, Limpets = 2, Range = 880, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 70 } },
                { "int_dronecontrol_decontamination_size5_class1", new ShipModule(128793943,ShipModule.ModuleTypes.DecontaminationLimpetController,"Decontamination Limpet Controller Class 5 Rating E"){ Cost = 145800, Class = 5, Rating = "E", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 10, Limpets = 3, Range = 1300, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 120 } },
                { "int_dronecontrol_decontamination_size7_class1", new ShipModule(128793944,ShipModule.ModuleTypes.DecontaminationLimpetController,"Decontamination Limpet Controller Class 7 Rating E"){ Cost = 1312200, Class = 7, Rating = "E", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 10, Limpets = 4, Range = 2040, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 180 } },

                { "int_dronecontrol_recon_size1_class1", new ShipModule(128837858,ShipModule.ModuleTypes.ReconLimpetController,"Recon Limpet Controller Class 1 Rating E"){ Cost = 2600, Class = 1, Rating = "E", Mass = 1.3, Integrity = 24, Power = 0.18, BootTime = 10, Limpets = 1, Range = 1200, Speed = 100, HackTime = 22 } },

                { "int_dronecontrol_recon_size3_class1", new ShipModule(128841592,ShipModule.ModuleTypes.ReconLimpetController,"Recon Limpet Controller Class 3 Rating E"){ Cost = 8200, Class = 3, Rating = "E", Mass = 2, Integrity = 51, Power = 0.2, BootTime = 10, Limpets = 1, Range = 1400, Speed = 100, HackTime = 17 } },
                { "int_dronecontrol_recon_size5_class1", new ShipModule(128841593,ShipModule.ModuleTypes.ReconLimpetController,"Recon Limpet Controller Class 5 Rating E"){ Cost = 75800, Class = 5, Rating = "E", Mass = 20, Integrity = 96, Power = 0.5, BootTime = 9.85, Limpets = 1, Range = 1700, Speed = 100, HackTime = 13 } },
                { "int_dronecontrol_recon_size7_class1", new ShipModule(128841594,ShipModule.ModuleTypes.ReconLimpetController,"Recon Limpet Controller Class 7 Rating E"){ Cost = 612200, Class = 7, Rating = "E", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 10, Limpets = 1, Range = 2000, Speed = 100, HackTime = 10 } },

                { "int_multidronecontrol_mining_size3_class1", new ShipModule(129001921,ShipModule.ModuleTypes.MiningMultiLimpetController,"Mining Multi Limpet Controller Class 3 Rating E"){ Cost = 15000, Class = 3, Rating = "E", Mass = 12, Integrity = 45, Power = 0.5, BootTime = 6, Limpets = 4, Range = 3300, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_multidronecontrol_mining_size3_class3", new ShipModule(129001922,ShipModule.ModuleTypes.MiningMultiLimpetController,"Mining Multi Limpet Controller Class 3 Rating C"){ Cost = 50000, Class = 3, Rating = "C", Mass = 10, Integrity = 68, Power = 0.35, BootTime = 6, Limpets = 4, Range = 5000, Speed = 200, MultiTargetSpeed = 60 } },
                { "int_multidronecontrol_operations_size3_class3", new ShipModule(129001923,ShipModule.ModuleTypes.OperationsMultiLimpetController,"Operations Limpet Controller Class 3 Rating C"){ Cost = 50000, Class = 3, Rating = "C", Mass = 10, Integrity = 68, Power = 0.35, BootTime = 6, Limpets = 4, Range = 2600, Time = 510, Speed = 500, MultiTargetSpeed = 60, HackTime = 16, MinCargo = 3, MaxCargo = 8 } },
                { "int_multidronecontrol_operations_size3_class4", new ShipModule(129001924,ShipModule.ModuleTypes.OperationsMultiLimpetController,"Operations Limpet Controller Class 3 Rating B"){ Cost = 80000, Class = 3, Rating = "B", Mass = 15, Integrity = 80, Power = 0.3, BootTime = 6, Limpets = 4, Range = 3100, Time = 420, Speed = 500, MultiTargetSpeed = 60, HackTime = 22, MinCargo = 4, MaxCargo = 9 } },
                { "int_multidronecontrol_rescue_size3_class2", new ShipModule(129001925,ShipModule.ModuleTypes.RescueMultiLimpetController,"Rescue Limpet Controller Class 3 Rating D"){ Cost = 30000, Class = 3, Rating = "D", Mass = 8, Integrity = 58, Power = 0.4, BootTime = 6, Limpets = 4, Range = 2100, Time = 300, Speed = 500, FuelTransfer = 1, MaxRepairMaterialCapacity = 60, HackTime = 19, MinCargo = 2, MaxCargo = 7 } },
                { "int_multidronecontrol_rescue_size3_class3", new ShipModule(129001926,ShipModule.ModuleTypes.RescueMultiLimpetController,"Rescue Limpet Controller Class 3 Rating C"){ Cost = 50000, Class = 3, Rating = "C", Mass = 10, Integrity = 68, Power = 0.35, BootTime = 6, Limpets = 4, Range = 2600, Time = 300, Speed = 500, FuelTransfer = 1, MaxRepairMaterialCapacity = 60, HackTime = 16, MinCargo = 3, MaxCargo = 8 } },
                { "int_multidronecontrol_xeno_size3_class3", new ShipModule(129001927,ShipModule.ModuleTypes.XenoMultiLimpetController,"Xeno Limpet Controller Class 3 Rating C"){ Cost = 50000, Class = 3, Rating = "C", Mass = 10, Integrity = 68, Power = 0.35, BootTime = 6, Limpets = 4, Range = 5000, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 70 } },
                { "int_multidronecontrol_xeno_size3_class4", new ShipModule(129001928,ShipModule.ModuleTypes.XenoMultiLimpetController,"Xeno Limpet Controller Class 3 Rating B"){ Cost = 80000, Class = 3, Rating = "B", Mass = 15, Integrity = 80, Power = 0.3, BootTime = 6, Limpets = 4, Range = 5000, Time = 300, Speed = 200, MaxRepairMaterialCapacity = 70 } },
                { "int_multidronecontrol_universal_size7_class3", new ShipModule(129001929,ShipModule.ModuleTypes.UniversalMultiLimpetController,"Universal Multi Limpet Controller Class 7 Rating C"){ Cost = 4000000, Class = 7, Rating = "C", Mass = 125, Integrity = 150, Power = 0.8, BootTime = 6, Limpets = 8, Range = 6500, Speed = 500, MultiTargetSpeed = 60, FuelTransfer = 1, MaxRepairMaterialCapacity = 310, HackTime = 8, MinCargo = 3, MaxCargo = 8 } },
                { "int_multidronecontrol_universal_size7_class5", new ShipModule(129001930,ShipModule.ModuleTypes.UniversalMultiLimpetController,"Universal Multi Limpet Controller Class 7 Rating A"){ Cost = 8000000, Class = 7, Rating = "A", Mass = 140, Integrity = 200, Power = 1.1, BootTime = 6, Limpets = 8, Range = 9100, Speed = 500, MultiTargetSpeed = 60, FuelTransfer = 1, MaxRepairMaterialCapacity = 310, HackTime = 5, MinCargo = 5, MaxCargo = 10 } },


                // Meta Hull Reinforcement Packages

                //{ "int_metaalloyhullreinforcement_size1_class1", new ShipModule(128793117,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 1 Rating E"){ Cost = 7500, Class = 1, Rating = "E", Mass = 2, HullReinforcement = 72, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size1_class2", new ShipModule(128793118,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 1 Rating D"){ Cost = 22500, Class = 1, Rating = "D", Mass = 1, HullReinforcement = 99, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size2_class1", new ShipModule(128793119,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 2 Rating E"){ Cost = 18000, Class = 2, Rating = "E", Mass = 4, HullReinforcement = 135, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size2_class2", new ShipModule(128793120,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 2 Rating D"){ Cost = 54000, Class = 2, Rating = "D", Mass = 2, HullReinforcement = 171, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size3_class1", new ShipModule(128793121,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 3 Rating E"){ Cost = 42000, Class = 3, Rating = "E", Mass = 8, HullReinforcement = 207, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size3_class2", new ShipModule(128793122,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 3 Rating D"){ Cost = 126000, Class = 3, Rating = "D", Mass = 4, HullReinforcement = 234, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size4_class1", new ShipModule(128793123,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 4 Rating E"){ Cost = 97500, Class = 4, Rating = "E", Mass = 16, HullReinforcement = 270, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size4_class2", new ShipModule(128793124,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 4 Rating D"){ Cost = 292500, Class = 4, Rating = "D", Mass = 8, HullReinforcement = 297, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size5_class1", new ShipModule(128793125,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 5 Rating E"){ Cost = 225000, Class = 5, Rating = "E", Mass = 32, HullReinforcement = 324, CausticReinforcement = 3 } },
                //{ "int_metaalloyhullreinforcement_size5_class2", new ShipModule(128793126,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,"Meta Alloy Hull Reinforcement Package Class 5 Rating D"){ Cost = 675000, Class = 5, Rating = "D", Mass = 16, HullReinforcement = 351, CausticReinforcement = 3 } },

                // Mine launches charges

                { "hpt_minelauncher_fixed_small", new ShipModule(128049500,ShipModule.ModuleTypes.MineLauncher,"Mine Launcher Fixed Small"){ Cost = 24260, Mount = "F", Class = 1, Rating = "I", Mass = 2, Integrity = 40, Power = 0.4, BootTime = 0, DPS = 44, Damage = 44, ThermL = 5, Pierce = 60, RateOfFire = 1, BurstInterval = 1, Clip = 1, Ammo = 36, ReloadTime = 2, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 59.091, ThermalProportion = 40.909, AmmoCost = 209 } },
                { "hpt_minelauncher_fixed_medium", new ShipModule(128049501,ShipModule.ModuleTypes.MineLauncher,"Mine Launcher Fixed Medium"){ Cost = 294080, Mount = "F", Class = 2, Rating = "I", Mass = 4, Integrity = 51, Power = 0.4, BootTime = 0, DPS = 44, Damage = 44, ThermL = 7.5, Pierce = 60, RateOfFire = 1, BurstInterval = 1, Clip = 3, Ammo = 72, ReloadTime = 6.6, BreachDamage = 13.2, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 59.091, ThermalProportion = 40.909, AmmoCost = 209 } },
                { "hpt_minelauncher_fixed_small_impulse", new ShipModule(128671448,ShipModule.ModuleTypes.ShockMineLauncher,"Shock Mine Launcher Fixed Small"){ Cost = 36390, Mount = "F", Class = 1, Rating = "I", Mass = 2, Integrity = 40, Power = 0.4, BootTime = 0, DPS = 32, Damage = 32, ThermL = 5, Pierce = 60, RateOfFire = 1, BurstInterval = 1, Clip = 1, Ammo = 36, ReloadTime = 2, BreachDamage = 9.6, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 62.5, ThermalProportion = 37.5, AmmoCost = 209 } },

                { "hpt_mining_abrblstr_fixed_small", new ShipModule(128915458,ShipModule.ModuleTypes.AbrasionBlaster,"Abrasion Blaster Fixed Small"){ Cost = 9700, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.34, BootTime = 0, DPS = 20, Damage = 4, DistributorDraw = 2, ThermL = 2, Pierce = 18, Range = 1000, Speed = 667, RateOfFire = 5, BurstInterval = 0.2, BreachDamage = 0.6, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 1000 } },
                { "hpt_mining_abrblstr_turret_small", new ShipModule(128915459,ShipModule.ModuleTypes.AbrasionBlaster,"Abrasion Blaster Turret Small"){ Cost = 27480, Mount = "T", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.47, BootTime = 0, DPS = 20, Damage = 4, DistributorDraw = 2, ThermL = 1.8, Pierce = 18, Range = 1000, Speed = 667, RateOfFire = 5, BurstInterval = 0.2, BreachDamage = 0.6, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 1000 } },

                { "hpt_mining_seismchrgwarhd_fixed_medium", new ShipModule(128915460,ShipModule.ModuleTypes.SeismicChargeLauncher,"Seismic Charge Launcher Fixed Medium"){ Cost = 153110, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 15, Damage = 15, Time = 2, DamageMultiplierFulLCharge = 1, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 35, Range = 1000, Speed = 350, RateOfFire = 1, BurstInterval = 1, Clip = 1, Ammo = 72, ReloadTime = 1, BreachDamage = 3, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100 } },
                { "hpt_mining_seismchrgwarhd_turret_medium", new ShipModule(128915461,ShipModule.ModuleTypes.SeismicChargeLauncher,"Seismic Charge Launcher Turret Medium"){ Cost = 445570, Mount = "T", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 15, Damage = 15, Time = 2, DamageMultiplierFulLCharge = 1, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 35, Range = 1000, Speed = 350, RateOfFire = 1, BurstInterval = 1, Clip = 1, Ammo = 72, ReloadTime = 1, BreachDamage = 3, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100 } },

                { "hpt_mining_subsurfdispmisle_fixed_small", new ShipModule(128915454,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,"Sub surface Displacement Missile Fixed Small"){ Cost = 12600, Mount = "F", Class = 1, Rating = "B", Mass = 2, Integrity = 40, Power = 0.42, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.18, ThermL = 2.25, Pierce = 25, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 32, ReloadTime = 2, BreachDamage = 0.5, BreachMin = 10, BreachMax = 20, ExplosiveProportion = 100 } },
                { "hpt_mining_subsurfdispmisle_turret_small", new ShipModule(128915455,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,"Sub surface Displacement Missile Turret Small"){ Cost = 38750, Mount = "T", Class = 1, Rating = "B", Mass = 2, Integrity = 40, Power = 0.53, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.16, ThermL = 2.25, Pierce = 25, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 32, ReloadTime = 2, BreachDamage = 0.5, BreachMin = 10, BreachMax = 20, ExplosiveProportion = 100 } },
                { "hpt_mining_subsurfdispmisle_fixed_medium", new ShipModule(128915456,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,"Sub surface Displacement Missile Fixed Medium"){ Cost = 122170, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.01, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.21, ThermL = 2.9, Pierce = 25, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 96, ReloadTime = 2, BreachDamage = 0.5, BreachMin = 10, BreachMax = 20, ExplosiveProportion = 100 } },
                { "hpt_mining_subsurfdispmisle_turret_medium", new ShipModule(128915457,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,"Sub surface Displacement Missile Turret Medium"){ Cost = 381750, Mount = "T", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 0.93, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.18, ThermL = 2.9, Pierce = 25, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 96, ReloadTime = 2, BreachDamage = 0.5, BreachMin = 10, BreachMax = 20, ExplosiveProportion = 100 } },

                // Mining lasers

                { "hpt_mininglaser_fixed_small", new ShipModule(128049525,ShipModule.ModuleTypes.MiningLaser,"Mining Laser Fixed Small"){ Cost = 6800, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.5, BootTime = 0, DPS = 2, Damage = 2, DistributorDraw = 1.5, ThermL = 2, Pierce = 18, Range = 500, BurstInterval = 0, BreachDamage = 0.3, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 300 } },
                { "hpt_mininglaser_fixed_medium", new ShipModule(128049526,ShipModule.ModuleTypes.MiningLaser,"Mining Laser Fixed Medium"){ Cost = 22580, Mount = "F", Class = 2, Rating = "D", Mass = 2, Integrity = 51, Power = 0.75, BootTime = 0, DPS = 4, Damage = 4, DistributorDraw = 3, ThermL = 4, Pierce = 18, Range = 500, BurstInterval = 0, BreachDamage = 0.6, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 300 } },
                { "hpt_mininglaser_turret_small", new ShipModule(128740819,ShipModule.ModuleTypes.MiningLaser,"Mining Laser Turret Small"){ Cost = 9400, Mount = "T", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.5, BootTime = 0, DPS = 2, Damage = 2, DistributorDraw = 1.5, ThermL = 2, Pierce = 18, Range = 500, BurstInterval = 0, BreachDamage = 0.3, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 300 } },
                { "hpt_mininglaser_turret_medium", new ShipModule(128740820,ShipModule.ModuleTypes.MiningLaser,"Mining Laser Turret Medium"){ Cost = 32580, Mount = "T", Class = 2, Rating = "D", Mass = 2, Integrity = 51, Power = 0.75, BootTime = 0, DPS = 4, Damage = 4, DistributorDraw = 3, ThermL = 4, Pierce = 18, Range = 500, BurstInterval = 0, BreachDamage = 0.6, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 300 } },
                { "hpt_mininglaser_fixed_small_advanced", new ShipModule(128671340,ShipModule.ModuleTypes.MiningLance,"Mining Lance Beam Laser Fixed Small"){ Cost = 33860, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.7, BootTime = 0, DPS = 8, Damage = 8, DistributorDraw = 1.75, ThermL = 6, Pierce = 18, Range = 2000, BurstInterval = 0, BreachDamage = 1.2, BreachMin = 10, BreachMax = 20, ThermalProportion = 100, Falloff = 500 } },

                // Missiles

                { "hpt_atdumbfiremissile_fixed_medium", new ShipModule(128788699,ShipModule.ModuleTypes.AXMissileRack,"AX Missile Rack Fixed Medium"){ Cost = 540900, Mount = "F", MissileType = "D", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 35, Damage = 70, DistributorDraw = 0.14, ThermL = 2.4, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 61.429, ExplosiveProportion = 38.571, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_fixed_large", new ShipModule(128788700,ShipModule.ModuleTypes.AXMissileRack,"AX Missile Rack Fixed Large"){ Cost = 1352250, Mount = "F", MissileType = "D", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 1.62, BootTime = 0, DPS = 35, Damage = 70, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 128, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 61.429, ExplosiveProportion = 38.571, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_turret_medium", new ShipModule(128788704,ShipModule.ModuleTypes.AXMissileRack,"AX Missile Rack Turret Medium"){ Cost = 2022700, Mount = "T", MissileType = "D", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 28.5, Damage = 57, DistributorDraw = 0.08, ThermL = 1.5, Pierce = 60, Range = 5000, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 64.912, ExplosiveProportion = 35.088, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_turret_large", new ShipModule(128788705,ShipModule.ModuleTypes.AXMissileRack,"AX Missile Rack Turret Large"){ Cost = 4056750, Mount = "T", MissileType = "D", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 1.75, BootTime = 0, DPS = 28.5, Damage = 57, DistributorDraw = 0.14, ThermL = 1.9, Pierce = 60, Range = 5000, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 128, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 64.912, ExplosiveProportion = 35.088, AmmoCost = 235 } },

                { "hpt_atdumbfiremissile_fixed_medium_v2", new ShipModule(129022081,ShipModule.ModuleTypes.EnhancedAXMissileRack,"Enhanced AX Missile Rack Medium"){ Cost = 681530, Mount = "F", MissileType = "D", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 1.3, BootTime = 0, DPS = 38.5, Damage = 77, DistributorDraw = 0.14, ThermL = 2.4, Pierce = 60, Speed = 1250, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 64.935, ExplosiveProportion = 35.065, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_fixed_large_v2", new ShipModule(129022079,ShipModule.ModuleTypes.EnhancedAXMissileRack,"Enhanced AX Missile Rack Large"){ Cost = 1703830, Mount = "F", MissileType = "D", Class = 3, Rating = "B", Mass = 8, Integrity = 64, Power = 1.72, BootTime = 0, DPS = 38.5, Damage = 77, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 1250, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 128, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 64.935, ExplosiveProportion = 35.065, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_turret_medium_v2", new ShipModule(129022083,ShipModule.ModuleTypes.EnhancedAXMissileRack,"Enhanced AX Missile Rack Medium"){ Cost = 2666290, Mount = "T", MissileType = "D", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 1.3, BootTime = 0, DPS = 32, Damage = 64, DistributorDraw = 0.08, ThermL = 1.5, Pierce = 60, Range = 5000, Speed = 1250, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 68.75, ExplosiveProportion = 31.25, AmmoCost = 235 } },
                { "hpt_atdumbfiremissile_turret_large_v2", new ShipModule(129022082,ShipModule.ModuleTypes.EnhancedAXMissileRack,"Enhanced AX Missile Rack Large"){ Cost = 5347530, Mount = "T", MissileType = "D", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 1.85, BootTime = 0, DPS = 32, Damage = 64, DistributorDraw = 0.14, ThermL = 1.9, Pierce = 60, Range = 5000, Speed = 1250, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 128, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, AXPortionDamage = 68.75, ExplosiveProportion = 31.25, AmmoCost = 235 } },

                { "hpt_atventdisruptorpylon_fixed_medium", new ShipModule(129030049,ShipModule.ModuleTypes.TorpedoPylon,"Guardian Nanite Torpedo Pylon Medium"){ Cost = 843170, Mount = "F", MissileType = "S", Class = 2, Rating = "I", Mass = 3, Integrity = 50, Power = 0.4, BootTime = 0, DPS = 0, Damage = 0, DistributorDraw = 0, ThermL = 35, Speed = 1000, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 64, ReloadTime = 3, BreachDamage = 0, Jitter = 0, AmmoCost = 15000 } },
                { "hpt_atventdisruptorpylon_fixed_large", new ShipModule(129030050,ShipModule.ModuleTypes.TorpedoPylon,"Guardian Nanite Torpedo Pylon Large"){ Cost = 1627420, Mount = "F", MissileType = "S", Class = 3, Rating = "I", Mass = 5, Integrity = 80, Power = 0.7, BootTime = 0, DPS = 0, Damage = 0, DistributorDraw = 0, ThermL = 35, Speed = 1000, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 128, ReloadTime = 3, BreachDamage = 0, Jitter = 0, AmmoCost = 15000 } },

                { "hpt_basicmissilerack_fixed_small", new ShipModule(128049492,ShipModule.ModuleTypes.SeekerMissileRack,"Seeker Missile Rack Fixed Small"){ Cost = 72600, Mount = "F", MissileType = "S", Class = 1, Rating = "B", Mass = 2, Integrity = 40, Power = 0.6, BootTime = 0, DPS = 13.333, Damage = 40, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 625, RateOfFire = 0.333, BurstInterval = 3, Clip = 6, Ammo = 6, ReloadTime = 12, BreachDamage = 16, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },
                { "hpt_basicmissilerack_fixed_medium", new ShipModule(128049493,ShipModule.ModuleTypes.SeekerMissileRack,"Seeker Missile Rack Fixed Medium"){ Cost = 512400, Mount = "F", MissileType = "S", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 13.333, Damage = 40, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 625, RateOfFire = 0.333, BurstInterval = 3, Clip = 6, Ammo = 18, ReloadTime = 12, BreachDamage = 16, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },
                { "hpt_basicmissilerack_fixed_large", new ShipModule(128049494,ShipModule.ModuleTypes.SeekerMissileRack,"Seeker Missile Rack Fixed Large"){ Cost = 1471030, Mount = "F", MissileType = "S", Class = 3, Rating = "A", Mass = 8, Integrity = 64, Power = 1.62, BootTime = 0, DPS = 13.333, Damage = 40, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 625, RateOfFire = 0.333, BurstInterval = 3, Clip = 6, Ammo = 36, ReloadTime = 12, BreachDamage = 16, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },

                { "hpt_dumbfiremissilerack_fixed_small", new ShipModule(128666724,ShipModule.ModuleTypes.MissileRack,"Missile Rack Fixed Small"){ Cost = 32180, Mount = "F", MissileType = "D", Class = 1, Rating = "B", Mass = 2, Integrity = 40, Power = 0.4, BootTime = 0, DPS = 25, Damage = 50, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 16, ReloadTime = 5, BreachDamage = 20, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },
                { "hpt_dumbfiremissilerack_fixed_medium", new ShipModule(128666725,ShipModule.ModuleTypes.MissileRack,"Missile Rack Fixed Medium"){ Cost = 240400, Mount = "F", MissileType = "D", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 25, Damage = 50, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 48, ReloadTime = 5, BreachDamage = 20, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },

                { "hpt_dumbfiremissilerack_fixed_large", new ShipModule(128891602,ShipModule.ModuleTypes.MissileRack,"Missile Rack Fixed Large"){ Cost = 1021500, Mount = "F", MissileType = "D", Class = 3, Rating = "A", Mass = 8, Integrity = 64, Power = 1.62, BootTime = 0, DPS = 25, Damage = 50, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 96, ReloadTime = 5, BreachDamage = 20, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },

                { "hpt_dumbfiremissilerack_fixed_medium_lasso", new ShipModule(128732552,ShipModule.ModuleTypes.RocketPropelledFSDDisruptor,"Rocket Propelled FSD Disrupter Fixed Medium"){ Cost = 1951040, Mount = "F", MissileType = "D", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 13.333, Damage = 40, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.333, BurstInterval = 3, Clip = 12, Ammo = 48, ReloadTime = 5, BreachDamage = 16, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },
                { "hpt_drunkmissilerack_fixed_medium", new ShipModule(128671344,ShipModule.ModuleTypes.Pack_HoundMissileRack,"Pack Hound Missile Rack Fixed Medium"){ Cost = 768600, Mount = "F", MissileType = "S", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 60, Damage = 7.5, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 600, RateOfFire = 2, BurstInterval = 0.5, Clip = 12, Ammo = 120, Rounds = 4, ReloadTime = 5, BreachDamage = 3, BreachMin = 0, BreachMax = 0, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },

                { "hpt_advancedtorppylon_fixed_small", new ShipModule(128049509,ShipModule.ModuleTypes.TorpedoPylon,"Advanced Torp Pylon Fixed Small"){ Cost = 11200, Mount = "F", MissileType = "S", Class = 1, Rating = "I", Mass = 2, Integrity = 40, Power = 0.4, BootTime = 0, DPS = 120, Damage = 120, ThermL = 45, Pierce = 10000, Speed = 250, RateOfFire = 1, BurstInterval = 1, Clip = 1, ReloadTime = 5, BreachDamage = 60, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, AmmoCost = 15000 } },
                { "hpt_advancedtorppylon_fixed_medium", new ShipModule(128049510,ShipModule.ModuleTypes.TorpedoPylon,"Advanced Torp Pylon Fixed Medium"){ Cost = 44800, Mount = "F", MissileType = "S", Class = 2, Rating = "I", Mass = 4, Integrity = 51, Power = 0.4, BootTime = 0, DPS = 120, Damage = 120, ThermL = 50, Pierce = 10000, Speed = 250, RateOfFire = 1, BurstInterval = 1, Clip = 2, ReloadTime = 5, BreachDamage = 60, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, AmmoCost = 15000 } },
                { "hpt_advancedtorppylon_fixed_large", new ShipModule(128049511,ShipModule.ModuleTypes.TorpedoPylon,"Advanced Torp Pylon Fixed Large"){ Cost = 157960, Mount = "F", MissileType = "S", Class = 3, Rating = "I", Mass = 8, Integrity = 64, Power = 0.6, BootTime = 0, DPS = 120, Damage = 120, ThermL = 55, Pierce = 10000, Speed = 250, RateOfFire = 1, BurstInterval = 1, Clip = 4, ReloadTime = 5, BreachDamage = 60, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, AmmoCost = 15000 } },

                { "hpt_dumbfiremissilerack_fixed_small_advanced", new ShipModule(128935982,ShipModule.ModuleTypes.AdvancedMissileRack,"Advanced Missile Rack Fixed Small"){ Cost = 32180, Mount = "F", MissileType = "D", Class = 1, Rating = "B", Mass = 2, Integrity = 40, Power = 0.4, BootTime = 0, DPS = 25, Damage = 50, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 20, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },
                { "hpt_dumbfiremissilerack_fixed_medium_advanced", new ShipModule(128935983,ShipModule.ModuleTypes.AdvancedMissileRack,"Advanced Missile Rack Fixed Medium"){ Cost = 240400, Mount = "F", MissileType = "D", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 25, Damage = 50, DistributorDraw = 0.24, ThermL = 3.6, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 12, Ammo = 64, ReloadTime = 5, BreachDamage = 20, BreachMin = 100, BreachMax = 100, ExplosiveProportion = 100, ThermalProportion = 0, AmmoCost = 500 } },

                { "hpt_human_extraction_fixed_medium", new ShipModule(129028577,ShipModule.ModuleTypes.MissileRack,"Human Extraction Missile Medium"){ Cost = 843170, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 50, Power = 1, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.21, ThermL = 2.9, Pierce = 25, Speed = 550, RateOfFire = 0.5, BurstInterval = 2, Clip = 1, Ammo = 96, ReloadTime = 2, BreachDamage = 0.5, BreachMin = 10, BreachMax = 20, ExplosiveProportion = 100 } },

                { "hpt_causticmissile_fixed_medium", new ShipModule(128833995,ShipModule.ModuleTypes.EnzymeMissileRack,"Enzyme Missile Rack Medium"){ Cost = 480500, Mount = "F", MissileType = "D", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.2, BootTime = 0, DPS = 2.5, Damage = 5, DistributorDraw = 0.08, ThermL = 1.5, Pierce = 60, Speed = 750, RateOfFire = 0.5, BurstInterval = 2, Clip = 8, Ammo = 64, ReloadTime = 5, BreachDamage = 0, BreachMin = 80, BreachMax = 100, ExplosiveProportion = 80, CausticPortionDamage = 20, AmmoCost = 235 } },

                // Module Reinforcements

                //{ "int_modulereinforcement_size1_class1", new ShipModule(128737270,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 1 Rating E"){ Cost = 5000, Class = 1, Rating = "E", Mass = 2, Integrity = 77, Protection = 30 } },
                //{ "int_modulereinforcement_size1_class2", new ShipModule(128737271,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 1 Rating D"){ Cost = 15000, Class = 1, Rating = "D", Mass = 1, Integrity = 70, Protection = 60 } },
                //{ "int_modulereinforcement_size2_class1", new ShipModule(128737272,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 2 Rating E"){ Cost = 12000, Class = 2, Rating = "E", Mass = 4, Integrity = 115, Protection = 30 } },
                //{ "int_modulereinforcement_size2_class2", new ShipModule(128737273,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 2 Rating D"){ Cost = 36000, Class = 2, Rating = "D", Mass = 2, Integrity = 105, Protection = 60 } },
                //{ "int_modulereinforcement_size3_class1", new ShipModule(128737274,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 3 Rating E"){ Cost = 28000, Class = 3, Rating = "E", Mass = 8, Integrity = 170, Protection = 30 } },
                //{ "int_modulereinforcement_size3_class2", new ShipModule(128737275,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 3 Rating D"){ Cost = 84000, Class = 3, Rating = "D", Mass = 4, Integrity = 155, Protection = 60 } },
                //{ "int_modulereinforcement_size4_class1", new ShipModule(128737276,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 4 Rating E"){ Cost = 65000, Class = 4, Rating = "E", Mass = 16, Integrity = 260, Protection = 30 } },
                //{ "int_modulereinforcement_size4_class2", new ShipModule(128737277,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 4 Rating D"){ Cost = 195000, Class = 4, Rating = "D", Mass = 8, Integrity = 235, Protection = 60 } },
                //{ "int_modulereinforcement_size5_class1", new ShipModule(128737278,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 5 Rating E"){ Cost = 150000, Class = 5, Rating = "E", Mass = 32, Integrity = 385, Protection = 30 } },
                //{ "int_modulereinforcement_size5_class2", new ShipModule(128737279,ShipModule.ModuleTypes.ModuleReinforcementPackage,"Module Reinforcement Package Class 5 Rating D"){ Cost = 450000, Class = 5, Rating = "D", Mass = 16, Integrity = 350, Protection = 60 } },

                // Multicannons (medium is 2E, turret 2F, large is 3E, 3F) Values from EDSY 14/5/24

                { "hpt_atmulticannon_fixed_medium", new ShipModule(128788701,ShipModule.ModuleTypes.AXMulti_Cannon,"AX Multi Cannon Fixed Medium"){ Cost = 379000, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.46, BootTime = 0, DPS = 23.643, Damage = 3.31, DistributorDraw = 0.11, ThermL = 0.18, Pierce = 17, Range = 4000, Speed = 1600, RateOfFire = 7.143, BurstInterval = 0.14, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 2.8, BreachMin = 50, BreachMax = 80, AXPortionDamage = 66.163, KineticProportion = 33.837, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_atmulticannon_fixed_large", new ShipModule(128788702,ShipModule.ModuleTypes.AXMulti_Cannon,"AX Multi Cannon Fixed Large"){ Cost = 1181500, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 35.971, Damage = 6.115, DistributorDraw = 0.18, ThermL = 0.28, Pierce = 33, Range = 4000, Speed = 1600, RateOfFire = 5.882, BurstInterval = 0.17, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 5.2, BreachMin = 50, BreachMax = 80, AXPortionDamage = 64.186, KineticProportion = 35.814, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_atmulticannon_turret_medium", new ShipModule(128793059,ShipModule.ModuleTypes.AXMulti_Cannon,"AX Multi Cannon Turret Medium"){ Cost = 1826500, Mount = "T", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 0.5, BootTime = 0, DPS = 10.812, Damage = 1.73, DistributorDraw = 0.06, ThermL = 0.09, Pierce = 17, Range = 4000, Speed = 1600, RateOfFire = 6.25, BurstInterval = 0.16, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 0.4, BreachMin = 50, BreachMax = 50, AXPortionDamage = 67.63, KineticProportion = 32.37, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_atmulticannon_turret_large", new ShipModule(128793060,ShipModule.ModuleTypes.AXMulti_Cannon,"AX Multi Cannon Turret Large"){ Cost = 3821600, Mount = "T", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 20.688, Damage = 3.31, DistributorDraw = 0.06, ThermL = 0.09, Pierce = 33, Range = 4000, Speed = 1600, RateOfFire = 6.25, BurstInterval = 0.16, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 0.8, BreachMin = 50, BreachMax = 50, AXPortionDamage = 66.163, KineticProportion = 33.837, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_atmulticannon_fixed_medium_v2", new ShipModule(129022080,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Fixed Medium"){ Cost = 455080, Mount = "F", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.48, BootTime = 0, DPS = 27.9, Damage = 3.9, DistributorDraw = 0.11, ThermL = 0.18, Pierce = 17, Range = 4000, Speed = 4000, RateOfFire = 7.1, BurstInterval = 0.14, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 3.3, BreachMin = 50, BreachMax = 80, AXPortionDamage = 71.795, KineticProportion = 28.205, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_atmulticannon_fixed_large_v2", new ShipModule(129022084,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Fixed Large"){ Cost = 1360320, Mount = "F", Class = 3, Rating = "B", Mass = 8, Integrity = 64, Power = 0.69, BootTime = 0, DPS = 42.9, Damage = 7.3, DistributorDraw = 0.18, ThermL = 0.28, Pierce = 33, Range = 4000, Speed = 4000, RateOfFire = 5.9, BurstInterval = 0.17, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 6.2, BreachMin = 50, BreachMax = 80, AXPortionDamage = 69.863, KineticProportion = 30.137, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_atmulticannon_turret_medium_v2", new ShipModule(129022086,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Turret Medium"){ Cost = 2193300, Mount = "T", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.52, BootTime = 0, DPS = 12.5, Damage = 2, DistributorDraw = 0.06, ThermL = 0.1, Pierce = 17, Range = 4000, Speed = 4000, RateOfFire = 6.2, BurstInterval = 0.16, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 0.5, BreachMin = 50, BreachMax = 50, AXPortionDamage = 70, KineticProportion = 30, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_atmulticannon_turret_large_v2", new ShipModule(129022085,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Turret Large"){ Cost = 4588710, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 0.69, BootTime = 0, DPS = 24.4, Damage = 3.9, DistributorDraw = 0.06, ThermL = 0.1, Pierce = 33, Range = 4000, Speed = 4000, RateOfFire = 6.2, BurstInterval = 0.16, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 1, BreachMin = 50, BreachMax = 50, AXPortionDamage = 71.795, KineticProportion = 28.205, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_atmulticannon_gimbal_medium", new ShipModule(129022089,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Gimbal Medium"){ Cost = 1197640, Mount = "G", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.46, BootTime = 0, DPS = 26.4, Damage = 3.7, DistributorDraw = 0.11, ThermL = 0.18, Pierce = 17, Range = 4000, Speed = 4000, RateOfFire = 7.1, BurstInterval = 0.14, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 3.1, BreachMin = 50, BreachMax = 80, AXPortionDamage = 66.667, KineticProportion = 33.333, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_atmulticannon_gimbal_large", new ShipModule(129022088,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,"Enhanced AX Multi Cannon Gimbal Large"){ Cost = 2390460, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 41.8, Damage = 6.3, DistributorDraw = 0.18, ThermL = 0.28, Pierce = 33, Range = 4000, Speed = 4000, RateOfFire = 5.9, BurstInterval = 0.17, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 5.2, BreachMin = 50, BreachMax = 80, AXPortionDamage = 65.079, KineticProportion = 34.921, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_multicannon_fixed_small", new ShipModule(128049455,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Fixed Small"){ Cost = 9500, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.28, BootTime = 0, DPS = 8.615, Damage = 1.12, DistributorDraw = 0.06, ThermL = 0.09, Pierce = 22, Range = 4000, Speed = 1600, RateOfFire = 7.692, BurstInterval = 0.13, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_fixed_medium", new ShipModule(128049456,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Fixed Medium"){ Cost = 38000, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.46, BootTime = 0, DPS = 15.643, Damage = 2.19, DistributorDraw = 0.11, ThermL = 0.18, Pierce = 37, Range = 4000, Speed = 1600, RateOfFire = 7.143, BurstInterval = 0.14, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 2, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_fixed_large", new ShipModule(128049457,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Fixed Medium"){ Cost = 140400, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 23.088, Damage = 3.925, DistributorDraw = 0.18, ThermL = 0.28, Pierce = 54, Range = 4000, Speed = 1600, RateOfFire = 5.882, BurstInterval = 0.17, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 3.5, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_fixed_huge", new ShipModule(128049458,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Fixed Huge"){ Cost = 1177600, Mount = "F", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 0.73, BootTime = 0, DPS = 28.03, Damage = 4.625, DistributorDraw = 0.24, ThermL = 0.39, Pierce = 68, Range = 4000, Speed = 1600, RateOfFire = 3.03, BurstInterval = 0.33, Clip = 100, Ammo = 2100, Rounds = 2, ReloadTime = 4, BreachDamage = 4.2, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_gimbal_small", new ShipModule(128049459,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Gimbal Small"){ Cost = 14250, Mount = "G", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.37, BootTime = 0, DPS = 6.833, Damage = 0.82, DistributorDraw = 0.07, ThermL = 0.1, Pierce = 22, Range = 4000, Speed = 1600, RateOfFire = 8.333, BurstInterval = 0.12, Clip = 90, Ammo = 2100, ReloadTime = 5, BreachDamage = 0.7, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_gimbal_medium", new ShipModule(128049460,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Gimbal Medium"){ Cost = 57000, Mount = "G", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 0.64, BootTime = 0, DPS = 12.615, Damage = 1.64, DistributorDraw = 0.14, ThermL = 0.2, Pierce = 37, Range = 4000, Speed = 1600, RateOfFire = 7.692, BurstInterval = 0.13, Clip = 90, Ammo = 2100, ReloadTime = 5, BreachDamage = 1.5, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_gimbal_large", new ShipModule(128049461,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Gimbal Large"){ Cost = 578440, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.97, BootTime = 0, DPS = 18.933, Damage = 2.84, DistributorDraw = 0.25, ThermL = 0.34, Pierce = 54, Range = 4000, Speed = 1600, RateOfFire = 6.667, BurstInterval = 0.15, Clip = 90, Ammo = 2100, ReloadTime = 5, BreachDamage = 2.6, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_turret_small", new ShipModule(128049462,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Turret Small"){ Cost = 81600, Mount = "T", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.26, BootTime = 0, DPS = 4, Damage = 0.56, DistributorDraw = 0.03, ThermL = 0.04, Pierce = 22, Range = 4000, Speed = 1600, RateOfFire = 7.143, BurstInterval = 0.14, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 0.5, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_turret_medium", new ShipModule(128049463,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Turret Medium"){ Cost = 1292800, Mount = "T", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 0.5, BootTime = 0, DPS = 7.313, Damage = 1.17, DistributorDraw = 0.06, ThermL = 0.09, Pierce = 37, Range = 4000, Speed = 1600, RateOfFire = 6.25, BurstInterval = 0.16, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 1.1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_turret_large", new ShipModule(128049464,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Turret Large"){ Cost = 3794600, Mount = "T", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 0.86, BootTime = 0, DPS = 11.737, Damage = 2.23, DistributorDraw = 0.16, ThermL = 0.19, Pierce = 54, Range = 4000, Speed = 1600, RateOfFire = 5.263, BurstInterval = 0.19, Clip = 90, Ammo = 2100, ReloadTime = 4, BreachDamage = 2, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_multicannon_gimbal_huge", new ShipModule(128681996,ShipModule.ModuleTypes.Multi_Cannon,"Multi Cannon Gimbal Huge"){ Cost = 6377600, Mount = "G", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 1.22, BootTime = 0, DPS = 23.3, Damage = 3.46, DistributorDraw = 0.37, ThermL = 0.51, Pierce = 68, Range = 4000, Speed = 1600, RateOfFire = 3.367, BurstInterval = 0.297, Clip = 90, Ammo = 2100, Rounds = 2, ReloadTime = 5, BreachDamage = 3.1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },

                { "hpt_multicannon_fixed_small_strong", new ShipModule(128671345,ShipModule.ModuleTypes.EnforcerCannon,"Enforcer Cannon Fixed Small"){ Cost = 14250, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.28, BootTime = 0, DPS = 12.391, Damage = 2.85, DistributorDraw = 0.12, ThermL = 0.18, Pierce = 30, Range = 4500, Speed = 1800, RateOfFire = 4.348, BurstInterval = 0.23, Clip = 60, Ammo = 1000, ReloadTime = 4, BreachDamage = 2.6, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = -999, AmmoCost = 1 } },

                { "hpt_multicannon_fixed_medium_advanced", new ShipModule(128935980,ShipModule.ModuleTypes.AdvancedMulti_Cannon,"Advanced Multi Cannon Fixed Medium"){ Cost = 38000, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.46, BootTime = 0, DPS = 15.643, Damage = 2.19, DistributorDraw = 0.11, ThermL = 0.18, Pierce = 37, Range = 4000, Speed = 1600, RateOfFire = 7.143, BurstInterval = 0.14, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 2, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },
                { "hpt_multicannon_fixed_small_advanced", new ShipModule(128935981,ShipModule.ModuleTypes.AdvancedMulti_Cannon,"Advanced Multi Cannon Fixed Small"){ Cost = 9500, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.28, BootTime = 0, DPS = 8.615, Damage = 1.12, DistributorDraw = 0.06, ThermL = 0.09, Pierce = 22, Range = 4000, Speed = 1600, RateOfFire = 7.692, BurstInterval = 0.13, Clip = 100, Ammo = 2100, ReloadTime = 4, BreachDamage = 1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, ThermalProportion = 0, Falloff = 2000, AmmoCost = 1 } },

                // Passenger cabins

                { "int_passengercabin_size4_class1", new ShipModule(128727922,ShipModule.ModuleTypes.EconomyClassPassengerCabin,"Economy Class Passenger Cabin Class 4 Rating E"){ Cost = 18960, Class = 4, Rating = "E", Mass = 10, Passengers = 8, CabinClass = "E" } },
                { "int_passengercabin_size4_class2", new ShipModule(128727923,ShipModule.ModuleTypes.BusinessClassPassengerCabin,"Business Class Passenger Cabin Class 4 Rating D"){ Cost = 56870, Class = 4, Rating = "D", Mass = 10, Passengers = 6, CabinClass = "B" } },
                { "int_passengercabin_size4_class3", new ShipModule(128727924,ShipModule.ModuleTypes.FirstClassPassengerCabin,"First Class Passenger Cabin Class 4 Rating C"){ Cost = 170600, Class = 4, Rating = "C", Mass = 10, Passengers = 3, CabinClass = "F" } },
                { "int_passengercabin_size5_class4", new ShipModule(128727925,ShipModule.ModuleTypes.LuxuryClassPassengerCabin,"Luxury Class Passenger Cabin Class 5 Rating B"){ Cost = 1658100, Class = 5, Rating = "B", Mass = 20, Passengers = 4, CabinClass = "L" } },
                { "int_passengercabin_size6_class1", new ShipModule(128727926,ShipModule.ModuleTypes.EconomyClassPassengerCabin,"Economy Class Passenger Cabin Class 6 Rating E"){ Cost = 61420, Class = 6, Rating = "E", Mass = 40, Passengers = 32, CabinClass = "E" } },
                { "int_passengercabin_size6_class2", new ShipModule(128727927,ShipModule.ModuleTypes.BusinessClassPassengerCabin,"Business Class Passenger Cabin Class 6 Rating D"){ Cost = 184240, Class = 6, Rating = "D", Mass = 40, Passengers = 16, CabinClass = "B" } },
                { "int_passengercabin_size6_class3", new ShipModule(128727928,ShipModule.ModuleTypes.FirstClassPassengerCabin,"First Class Passenger Cabin Class 6 Rating C"){ Cost = 552700, Class = 6, Rating = "C", Mass = 40, Passengers = 12, CabinClass = "F" } },
                { "int_passengercabin_size6_class4", new ShipModule(128727929,ShipModule.ModuleTypes.LuxuryClassPassengerCabin,"Luxury Class Passenger Cabin Class 6 Rating B"){ Cost = 4974300, Class = 6, Rating = "B", Mass = 40, Passengers = 8, CabinClass = "L" } },

                { "int_passengercabin_size2_class1", new ShipModule(128734690,ShipModule.ModuleTypes.EconomyClassPassengerCabin,"Economy Class Passenger Cabin Class 2 Rating E"){ Cost = 4320, Class = 2, Rating = "E", Mass = 2.5, Passengers = 2, CabinClass = "E" } },
                { "int_passengercabin_size3_class1", new ShipModule(128734691,ShipModule.ModuleTypes.EconomyClassPassengerCabin,"Economy Class Passenger Cabin Class 3 Rating E"){ Cost = 8670, Class = 3, Rating = "E", Mass = 5, Passengers = 4, CabinClass = "E" } },
                { "int_passengercabin_size3_class2", new ShipModule(128734692,ShipModule.ModuleTypes.BusinessClassPassengerCabin,"Business Class Passenger Cabin Class 3 Rating D"){ Cost = 26720, Class = 3, Rating = "D", Mass = 5, Passengers = 3, CabinClass = "B" } },
                { "int_passengercabin_size5_class1", new ShipModule(128734693,ShipModule.ModuleTypes.EconomyClassPassengerCabin,"Economy Class Passenger Cabin Class 5 Rating E"){ Cost = 34960, Class = 5, Rating = "E", Mass = 20, Passengers = 16, CabinClass = "E" } },
                { "int_passengercabin_size5_class2", new ShipModule(128734694,ShipModule.ModuleTypes.BusinessClassPassengerCabin,"Business Class Passenger Cabin Class 5 Rating D"){ Cost = 92370, Class = 5, Rating = "D", Mass = 20, Passengers = 10, CabinClass = "B" } },
                { "int_passengercabin_size5_class3", new ShipModule(128734695,ShipModule.ModuleTypes.FirstClassPassengerCabin,"First Class Passenger Cabin Class 5 Rating C"){ Cost = 340540, Class = 5, Rating = "C", Mass = 20, Passengers = 6, CabinClass = "F" } },

                // Planetary approach

                { "int_planetapproachsuite_advanced", new ShipModule(128975719, ShipModule.ModuleTypes.AdvancedPlanetaryApproachSuite, "Advanced Planet Approach Suite") },
                { "int_planetapproachsuite", new ShipModule(128672317, ShipModule.ModuleTypes.PlanetaryApproachSuite, "Planet Approach Suite") },

                // planetary hangar

                { "int_buggybay_size2_class1", new ShipModule(128672288,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 2 Rating H"){ Cost = 18000, Class = 2, Rating = "H", Mass = 12, Integrity = 30, Power = 0.25, BootTime = 5, Size = 1, Rebuilds = 1, AmmoCost = 1030 } },
                { "int_buggybay_size2_class2", new ShipModule(128672289,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 2 Rating G"){ Cost = 21600, Class = 2, Rating = "G", Mass = 6, Integrity = 30, Power = 0.75, BootTime = 5, Size = 1, Rebuilds = 1, AmmoCost = 1030 } },
                { "int_buggybay_size4_class1", new ShipModule(128672290,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 4 Rating H"){ Cost = 72000, Class = 4, Rating = "H", Mass = 20, Integrity = 30, Power = 0.4, BootTime = 5, Size = 2, Rebuilds = 1, AmmoCost = 1030 } },
                { "int_buggybay_size4_class2", new ShipModule(128672291,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 4 Rating G"){ Cost = 86400, Class = 4, Rating = "G", Mass = 10, Integrity = 30, Power = 1.2, BootTime = 5, Size = 2, Rebuilds = 1, AmmoCost = 1030 } },
                { "int_buggybay_size6_class1", new ShipModule(128672292,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 6 Rating H"){ Cost = 576000, Class = 6, Rating = "H", Mass = 34, Integrity = 30, Power = 0.6, BootTime = 5, Size = 4, Rebuilds = 1, AmmoCost = 1030 } },
                { "int_buggybay_size6_class2", new ShipModule(128672293,ShipModule.ModuleTypes.PlanetaryVehicleHangar,"Planetary Vehicle Hangar Class 6 Rating G"){ Cost = 691200, Class = 6, Rating = "G", Mass = 17, Integrity = 30, Power = 1.8, BootTime = 5, Size = 4, Rebuilds = 1, AmmoCost = 1030 } },

                // Plasmas

                { "hpt_plasmaaccelerator_fixed_medium", new ShipModule(128049465,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Accelerator Fixed Medium"){ Cost = 834200, Mount = "F", Class = 2, Rating = "C", Mass = 4, Integrity = 51, Power = 1.43, BootTime = 0, DPS = 17.921, Damage = 54.3, DistributorDraw = 8.65, ThermL = 15.58, Pierce = 100, Range = 3500, Speed = 875, RateOfFire = 0.33, BurstInterval = 3.03, Clip = 5, Ammo = 100, ReloadTime = 6, BreachDamage = 46.2, BreachMin = 40, BreachMax = 80, AbsolutePortionDamage = 59.853, KineticProportion = 20.074, ThermalProportion = 20.074, Falloff = 2000, AmmoCost = 200 } },
                { "hpt_plasmaaccelerator_fixed_large", new ShipModule(128049466,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Accelerator Fixed Large"){ Cost = 3051200, Mount = "F", Class = 3, Rating = "B", Mass = 8, Integrity = 64, Power = 1.97, BootTime = 0, DPS = 24.174, Damage = 83.4, DistributorDraw = 13.6, ThermL = 21.75, Pierce = 100, Range = 3500, Speed = 875, RateOfFire = 0.29, BurstInterval = 3.45, Clip = 5, Ammo = 100, ReloadTime = 6, BreachDamage = 70.9, BreachMin = 40, BreachMax = 80, AbsolutePortionDamage = 59.952, KineticProportion = 20.024, ThermalProportion = 20.024, Falloff = 2000, AmmoCost = 200 } },
                { "hpt_plasmaaccelerator_fixed_huge", new ShipModule(128049467,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Accelerator Fixed Huge"){ Cost = 13793600, Mount = "F", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 2.63, BootTime = 0, DPS = 31.313, Damage = 125.25, DistributorDraw = 21.04, ThermL = 29.46, Pierce = 100, Range = 3500, Speed = 875, RateOfFire = 0.25, BurstInterval = 4, Clip = 5, Ammo = 100, ReloadTime = 6, BreachDamage = 106.5, BreachMin = 40, BreachMax = 80, AbsolutePortionDamage = 60.08, KineticProportion = 19.96, ThermalProportion = 19.96, Falloff = 2000, AmmoCost = 200 } },

                { "hpt_plasmaaccelerator_fixed_large_advanced", new ShipModule(128671339,ShipModule.ModuleTypes.AdvancedPlasmaAccelerator,"Advanced Plasma Accelerator Fixed Large"){ Cost = 4576800, Mount = "F", Class = 3, Rating = "B", Mass = 8, Integrity = 64, Power = 1.97, BootTime = 0, DPS = 28.667, Damage = 34.4, DistributorDraw = 5.5, ThermL = 11, Pierce = 100, Range = 3500, Speed = 875, RateOfFire = 0.833, BurstInterval = 1.2, Clip = 20, Ammo = 300, ReloadTime = 6, BreachDamage = 30.9, BreachMin = 40, BreachMax = 80, AbsolutePortionDamage = 59.884, KineticProportion = 20.058, ThermalProportion = 20.058, Falloff = 2000, AmmoCost = 200 } },

                { "hpt_plasmashockcannon_fixed_large", new ShipModule(128834780,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Fixed Large"){ Cost = 1015750, Mount = "F", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.89, BootTime = 0, DPS = 181.4, Damage = 18.14, DistributorDraw = 0.92, ThermL = 2.66, Pierce = 60, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 12.7, BreachMin = 40, BreachMax = 60, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_gimbal_large", new ShipModule(128834781,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Gimbal Large"){ Cost = 2249050, Mount = "G", Class = 3, Rating = "C", Mass = 8, Integrity = 64, Power = 0.89, BootTime = 0, DPS = 148.7, Damage = 14.87, DistributorDraw = 1.07, ThermL = 3.12, Pierce = 60, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 10.4, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_turret_large", new ShipModule(128834782,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Turret Large"){ Cost = 6050200, Mount = "T", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 0.64, BootTime = 0, DPS = 122.6, Damage = 12.26, DistributorDraw = 0.79, ThermL = 2.2, Pierce = 60, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 8.6, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },

                { "hpt_plasmashockcannon_fixed_medium", new ShipModule(128834002,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Fixed Medium"){ Cost = 367500, Mount = "F", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.57, BootTime = 0, DPS = 129.6, Damage = 12.96, DistributorDraw = 0.47, ThermL = 1.8, Pierce = 40, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 9.1, BreachMin = 40, BreachMax = 60, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_gimbal_medium", new ShipModule(128834003,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Gimbal Medium"){ Cost = 565200, Mount = "G", Class = 2, Rating = "D", Mass = 4, Integrity = 51, Power = 0.61, BootTime = 0, DPS = 102.1, Damage = 10.21, DistributorDraw = 0.58, ThermL = 2.1, Pierce = 40, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 7.1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_turret_medium", new ShipModule(128834004,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Turret Medium"){ Cost = 1359200, Mount = "T", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.5, BootTime = 0, DPS = 89.6, Damage = 8.96, DistributorDraw = 0.39, ThermL = 1.24, Pierce = 40, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 6.3, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },

                { "hpt_plasmashockcannon_turret_small", new ShipModule(128891603,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Turret Small"){ Cost = 364000, Mount = "T", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.54, BootTime = 0, DPS = 44.7, Damage = 4.47, DistributorDraw = 0.21, ThermL = 0.69, Pierce = 25, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 3.1, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_gimbal_small", new ShipModule(128891604,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Gimbal Small"){ Cost = 137500, Mount = "G", Class = 1, Rating = "E", Mass = 2, Integrity = 40, Power = 0.47, BootTime = 0, DPS = 69.1, Damage = 6.91, DistributorDraw = 0.39, ThermL = 1.45, Pierce = 25, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 4.8, BreachMin = 40, BreachMax = 80, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },
                { "hpt_plasmashockcannon_fixed_small", new ShipModule(128891605,ShipModule.ModuleTypes.ShockCannon,"Shock Cannon Fixed Small"){ Cost = 65940, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 0.41, BootTime = 0, DPS = 86.4, Damage = 8.64, DistributorDraw = 0.27, ThermL = 1.14, Pierce = 25, Range = 3000, Speed = 1200, RateOfFire = 10, BurstInterval = 0.1, Clip = 16, Ammo = 240, ReloadTime = 6, BreachDamage = 6, BreachMin = 40, BreachMax = 60, KineticProportion = 100, Falloff = 2500, AmmoCost = 9 } },

                // power distributor

                { "int_powerdistributor_size1_class1_free", new ShipModule(128666639,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 36, Power = 0.32, BootTime = 5, WepCap = 10, WepMW = 1.2, EngCap = 8, EngMW = 0.4, SysCap = 8, SysMW = 0.4 } },
                { "int_powerdistributor_size1_class1", new ShipModule(128064178,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 36, Power = 0.32, BootTime = 5, WepCap = 10, WepMW = 1.2, EngCap = 8, EngMW = 0.4, SysCap = 8, SysMW = 0.4 } },
                { "int_powerdistributor_size1_class2", new ShipModule(128064179,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating D"){ Cost = 1290, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.36, BootTime = 5, WepCap = 11, WepMW = 1.4, EngCap = 9, EngMW = 0.5, SysCap = 9, SysMW = 0.5 } },
                { "int_powerdistributor_size1_class3", new ShipModule(128064180,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating C"){ Cost = 3230, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.4, BootTime = 5, WepCap = 12, WepMW = 1.5, EngCap = 10, EngMW = 0.5, SysCap = 10, SysMW = 0.5 } },
                { "int_powerdistributor_size1_class4", new ShipModule(128064181,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating B"){ Cost = 8080, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.44, BootTime = 5, WepCap = 13, WepMW = 1.7, EngCap = 11, EngMW = 0.6, SysCap = 11, SysMW = 0.6 } },
                { "int_powerdistributor_size1_class5", new ShipModule(128064182,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 1 Rating A"){ Cost = 20200, Class = 1, Rating = "A", Mass = 1.3, Integrity = 44, Power = 0.48, BootTime = 5, WepCap = 14, WepMW = 1.8, EngCap = 12, EngMW = 0.6, SysCap = 12, SysMW = 0.6 } },
                { "int_powerdistributor_size2_class1", new ShipModule(128064183,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 2 Rating E"){ Cost = 1450, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, Power = 0.36, BootTime = 5, WepCap = 12, WepMW = 1.4, EngCap = 10, EngMW = 0.6, SysCap = 10, SysMW = 0.6 } },
                { "int_powerdistributor_size2_class2", new ShipModule(128064184,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 2 Rating D"){ Cost = 3620, Class = 2, Rating = "D", Mass = 1, Integrity = 41, Power = 0.41, BootTime = 5, WepCap = 14, WepMW = 1.6, EngCap = 11, EngMW = 0.6, SysCap = 11, SysMW = 0.6 } },
                { "int_powerdistributor_size2_class3", new ShipModule(128064185,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 2 Rating C"){ Cost = 9050, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.45, BootTime = 5, WepCap = 15, WepMW = 1.8, EngCap = 12, EngMW = 0.7, SysCap = 12, SysMW = 0.7 } },
                { "int_powerdistributor_size2_class4", new ShipModule(128064186,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 2 Rating B"){ Cost = 22620, Class = 2, Rating = "B", Mass = 4, Integrity = 61, Power = 0.5, BootTime = 5, WepCap = 17, WepMW = 2, EngCap = 13, EngMW = 0.8, SysCap = 13, SysMW = 0.8 } },
                { "int_powerdistributor_size2_class5", new ShipModule(128064187,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 2 Rating A"){ Cost = 56550, Class = 2, Rating = "A", Mass = 2.5, Integrity = 56, Power = 0.54, BootTime = 5, WepCap = 18, WepMW = 2.2, EngCap = 14, EngMW = 0.8, SysCap = 14, SysMW = 0.8 } },
                { "int_powerdistributor_size3_class1", new ShipModule(128064188,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 3 Rating E"){ Cost = 4050, Class = 3, Rating = "E", Mass = 5, Integrity = 58, Power = 0.4, BootTime = 5, WepCap = 16, WepMW = 1.8, EngCap = 12, EngMW = 0.9, SysCap = 12, SysMW = 0.9 } },
                { "int_powerdistributor_size3_class2", new ShipModule(128064189,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 3 Rating D"){ Cost = 10130, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.45, BootTime = 5, WepCap = 18, WepMW = 2.1, EngCap = 14, EngMW = 1, SysCap = 14, SysMW = 1 } },
                { "int_powerdistributor_size3_class3", new ShipModule(128064190,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 3 Rating C"){ Cost = 25330, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.5, BootTime = 5, WepCap = 20, WepMW = 2.3, EngCap = 15, EngMW = 1.1, SysCap = 15, SysMW = 1.1 } },
                { "int_powerdistributor_size3_class4", new ShipModule(128064191,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 3 Rating B"){ Cost = 63330, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.55, BootTime = 5, WepCap = 22, WepMW = 2.5, EngCap = 17, EngMW = 1.2, SysCap = 17, SysMW = 1.2 } },
                { "int_powerdistributor_size3_class5", new ShipModule(128064192,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 3 Rating A"){ Cost = 158330, Class = 3, Rating = "A", Mass = 5, Integrity = 70, Power = 0.6, BootTime = 5, WepCap = 24, WepMW = 2.8, EngCap = 18, EngMW = 1.3, SysCap = 18, SysMW = 1.3 } },
                { "int_powerdistributor_size4_class1", new ShipModule(128064193,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 4 Rating E"){ Cost = 11350, Class = 4, Rating = "E", Mass = 10, Integrity = 72, Power = 0.45, BootTime = 5, WepCap = 22, WepMW = 2.3, EngCap = 15, EngMW = 1.3, SysCap = 15, SysMW = 1.3 } },
                { "int_powerdistributor_size4_class2", new ShipModule(128064194,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 4 Rating D"){ Cost = 28370, Class = 4, Rating = "D", Mass = 4, Integrity = 64, Power = 0.5, BootTime = 5, WepCap = 24, WepMW = 2.6, EngCap = 17, EngMW = 1.4, SysCap = 17, SysMW = 1.4 } },
                { "int_powerdistributor_size4_class3", new ShipModule(128064195,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 4 Rating C"){ Cost = 70930, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 0.56, BootTime = 5, WepCap = 27, WepMW = 2.9, EngCap = 19, EngMW = 1.6, SysCap = 19, SysMW = 1.6 } },
                { "int_powerdistributor_size4_class4", new ShipModule(128064196,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 4 Rating B"){ Cost = 177330, Class = 4, Rating = "B", Mass = 16, Integrity = 96, Power = 0.62, BootTime = 5, WepCap = 30, WepMW = 3.2, EngCap = 21, EngMW = 1.8, SysCap = 21, SysMW = 1.8 } },
                { "int_powerdistributor_size4_class5", new ShipModule(128064197,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 4 Rating A"){ Cost = 443330, Class = 4, Rating = "A", Mass = 10, Integrity = 88, Power = 0.67, BootTime = 5, WepCap = 32, WepMW = 3.5, EngCap = 23, EngMW = 1.9, SysCap = 23, SysMW = 1.9 } },
                { "int_powerdistributor_size5_class1", new ShipModule(128064198,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 5 Rating E"){ Cost = 31780, Class = 5, Rating = "E", Mass = 20, Integrity = 86, Power = 0.5, BootTime = 5, WepCap = 27, WepMW = 2.9, EngCap = 19, EngMW = 1.7, SysCap = 19, SysMW = 1.7 } },
                { "int_powerdistributor_size5_class2", new ShipModule(128064199,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 5 Rating D"){ Cost = 79440, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.56, BootTime = 5, WepCap = 31, WepMW = 3.2, EngCap = 22, EngMW = 1.9, SysCap = 22, SysMW = 1.9 } },
                { "int_powerdistributor_size5_class3", new ShipModule(128064200,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 5 Rating C"){ Cost = 198610, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.62, BootTime = 5, WepCap = 34, WepMW = 3.6, EngCap = 24, EngMW = 2.1, SysCap = 24, SysMW = 2.1 } },
                { "int_powerdistributor_size5_class4", new ShipModule(128064201,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 5 Rating B"){ Cost = 496530, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.68, BootTime = 5, WepCap = 37, WepMW = 4, EngCap = 26, EngMW = 2.3, SysCap = 26, SysMW = 2.3 } },
                { "int_powerdistributor_size5_class5", new ShipModule(128064202,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 5 Rating A"){ Cost = 1241320, Class = 5, Rating = "A", Mass = 20, Integrity = 106, Power = 0.74, BootTime = 5, WepCap = 41, WepMW = 4.3, EngCap = 29, EngMW = 2.5, SysCap = 29, SysMW = 2.5 } },
                { "int_powerdistributor_size6_class1", new ShipModule(128064203,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 6 Rating E"){ Cost = 88980, Class = 6, Rating = "E", Mass = 40, Integrity = 102, Power = 0.54, BootTime = 5, WepCap = 34, WepMW = 3.4, EngCap = 23, EngMW = 2.2, SysCap = 23, SysMW = 2.2 } },
                { "int_powerdistributor_size6_class2", new ShipModule(128064204,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 6 Rating D"){ Cost = 222440, Class = 6, Rating = "D", Mass = 16, Integrity = 90, Power = 0.61, BootTime = 5, WepCap = 38, WepMW = 3.9, EngCap = 26, EngMW = 2.4, SysCap = 26, SysMW = 2.4 } },
                { "int_powerdistributor_size6_class3", new ShipModule(128064205,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 6 Rating C"){ Cost = 556110, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 0.68, BootTime = 5, WepCap = 42, WepMW = 4.3, EngCap = 29, EngMW = 2.7, SysCap = 29, SysMW = 2.7 } },
                { "int_powerdistributor_size6_class4", new ShipModule(128064206,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 6 Rating B"){ Cost = 1390280, Class = 6, Rating = "B", Mass = 64, Integrity = 136, Power = 0.75, BootTime = 5, WepCap = 46, WepMW = 4.7, EngCap = 32, EngMW = 3, SysCap = 32, SysMW = 3 } },
                { "int_powerdistributor_size6_class5", new ShipModule(128064207,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 6 Rating A"){ Cost = 3475690, Class = 6, Rating = "A", Mass = 40, Integrity = 124, Power = 0.82, BootTime = 5, WepCap = 50, WepMW = 5.2, EngCap = 35, EngMW = 3.2, SysCap = 35, SysMW = 3.2 } },
                { "int_powerdistributor_size7_class1", new ShipModule(128064208,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 7 Rating E"){ Cost = 249140, Class = 7, Rating = "E", Mass = 80, Integrity = 118, Power = 0.59, BootTime = 5, WepCap = 41, WepMW = 4.1, EngCap = 27, EngMW = 2.6, SysCap = 27, SysMW = 2.6 } },
                { "int_powerdistributor_size7_class2", new ShipModule(128064209,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 7 Rating D"){ Cost = 622840, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.67, BootTime = 5, WepCap = 46, WepMW = 4.6, EngCap = 31, EngMW = 3, SysCap = 31, SysMW = 3 } },
                { "int_powerdistributor_size7_class3", new ShipModule(128064210,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 7 Rating C"){ Cost = 1557110, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.74, BootTime = 5, WepCap = 51, WepMW = 5.1, EngCap = 34, EngMW = 3.3, SysCap = 34, SysMW = 3.3 } },
                { "int_powerdistributor_size7_class4", new ShipModule(128064211,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 7 Rating B"){ Cost = 3892770, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.81, BootTime = 5, WepCap = 56, WepMW = 5.6, EngCap = 37, EngMW = 3.6, SysCap = 37, SysMW = 3.6 } },
                { "int_powerdistributor_size7_class5", new ShipModule(128064212,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 7 Rating A"){ Cost = 9731930, Class = 7, Rating = "A", Mass = 80, Integrity = 144, Power = 0.89, BootTime = 5, WepCap = 61, WepMW = 6.1, EngCap = 41, EngMW = 4, SysCap = 41, SysMW = 4 } },
                { "int_powerdistributor_size8_class1", new ShipModule(128064213,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 8 Rating E"){ Cost = 697580, Class = 8, Rating = "E", Mass = 160, Integrity = 135, Power = 0.64, BootTime = 5, WepCap = 48, WepMW = 4.8, EngCap = 32, EngMW = 3.2, SysCap = 32, SysMW = 3.2 } },
                { "int_powerdistributor_size8_class2", new ShipModule(128064214,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 8 Rating D"){ Cost = 1743960, Class = 8, Rating = "D", Mass = 64, Integrity = 120, Power = 0.72, BootTime = 5, WepCap = 54, WepMW = 5.4, EngCap = 36, EngMW = 3.6, SysCap = 36, SysMW = 3.6 } },
                { "int_powerdistributor_size8_class3", new ShipModule(128064215,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 8 Rating C"){ Cost = 4359900, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 0.8, BootTime = 5, WepCap = 60, WepMW = 6, EngCap = 40, EngMW = 4, SysCap = 40, SysMW = 4 } },
                { "int_powerdistributor_size8_class4", new ShipModule(128064216,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 8 Rating B"){ Cost = 10899760, Class = 8, Rating = "B", Mass = 256, Integrity = 180, Power = 0.88, BootTime = 5, WepCap = 66, WepMW = 6.6, EngCap = 44, EngMW = 4.4, SysCap = 44, SysMW = 4.4 } },
                { "int_powerdistributor_size8_class5", new ShipModule(128064217,ShipModule.ModuleTypes.PowerDistributor,"Power Distributor Class 8 Rating A"){ Cost = 27249390, Class = 8, Rating = "A", Mass = 160, Integrity = 165, Power = 0.96, BootTime = 5, WepCap = 72, WepMW = 7.2, EngCap = 48, EngMW = 4.8, SysCap = 48, SysMW = 4.8 } },

                
                // Power plant

                { "int_powerplant_size2_class1_free", new ShipModule(128666635,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, PowerGen = 6.4, HeatEfficiency = 1 } },
                { "int_powerplant_size2_class1", new ShipModule(128064033,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, PowerGen = 6.4, HeatEfficiency = 1 } },
                { "int_powerplant_size2_class2", new ShipModule(128064034,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating D"){ Cost = 5930, Class = 2, Rating = "D", Mass = 1, Integrity = 41, PowerGen = 7.2, HeatEfficiency = 0.75 } },
                { "int_powerplant_size2_class3", new ShipModule(128064035,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating C"){ Cost = 17790, Class = 2, Rating = "C", Mass = 1.3, Integrity = 51, PowerGen = 8, HeatEfficiency = 0.5 } },
                { "int_powerplant_size2_class4", new ShipModule(128064036,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating B"){ Cost = 53380, Class = 2, Rating = "B", Mass = 2, Integrity = 61, PowerGen = 8.8, HeatEfficiency = 0.45 } },
                { "int_powerplant_size2_class5", new ShipModule(128064037,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 2 Rating A"){ Cost = 160140, Class = 2, Rating = "A", Mass = 1.3, Integrity = 56, PowerGen = 9.6, HeatEfficiency = 0.4 } },
                { "int_powerplant_size3_class1", new ShipModule(128064038,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 3 Rating E"){ Cost = 5930, Class = 3, Rating = "E", Mass = 5, Integrity = 58, PowerGen = 8, HeatEfficiency = 1 } },
                { "int_powerplant_size3_class2", new ShipModule(128064039,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 3 Rating D"){ Cost = 17790, Class = 3, Rating = "D", Mass = 2, Integrity = 51, PowerGen = 9, HeatEfficiency = 0.75 } },
                { "int_powerplant_size3_class3", new ShipModule(128064040,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 3 Rating C"){ Cost = 53380, Class = 3, Rating = "C", Mass = 2.5, Integrity = 64, PowerGen = 10, HeatEfficiency = 0.5 } },
                { "int_powerplant_size3_class4", new ShipModule(128064041,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 3 Rating B"){ Cost = 160140, Class = 3, Rating = "B", Mass = 4, Integrity = 77, PowerGen = 11, HeatEfficiency = 0.45 } },
                { "int_powerplant_size3_class5", new ShipModule(128064042,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 3 Rating A"){ Cost = 480410, Class = 3, Rating = "A", Mass = 2.5, Integrity = 70, PowerGen = 12, HeatEfficiency = 0.4 } },
                { "int_powerplant_size4_class1", new ShipModule(128064043,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 4 Rating E"){ Cost = 17790, Class = 4, Rating = "E", Mass = 10, Integrity = 72, PowerGen = 10.4, HeatEfficiency = 1 } },
                { "int_powerplant_size4_class2", new ShipModule(128064044,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 4 Rating D"){ Cost = 53380, Class = 4, Rating = "D", Mass = 4, Integrity = 64, PowerGen = 11.7, HeatEfficiency = 0.75 } },
                { "int_powerplant_size4_class3", new ShipModule(128064045,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 4 Rating C"){ Cost = 160140, Class = 4, Rating = "C", Mass = 5, Integrity = 80, PowerGen = 13, HeatEfficiency = 0.5 } },
                { "int_powerplant_size4_class4", new ShipModule(128064046,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 4 Rating B"){ Cost = 480410, Class = 4, Rating = "B", Mass = 8, Integrity = 96, PowerGen = 14.3, HeatEfficiency = 0.45 } },
                { "int_powerplant_size4_class5", new ShipModule(128064047,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 4 Rating A"){ Cost = 1441230, Class = 4, Rating = "A", Mass = 5, Integrity = 88, PowerGen = 15.6, HeatEfficiency = 0.4 } },
                { "int_powerplant_size5_class1", new ShipModule(128064048,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 5 Rating E"){ Cost = 53380, Class = 5, Rating = "E", Mass = 20, Integrity = 86, PowerGen = 13.6, HeatEfficiency = 1 } },
                { "int_powerplant_size5_class2", new ShipModule(128064049,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 5 Rating D"){ Cost = 160140, Class = 5, Rating = "D", Mass = 8, Integrity = 77, PowerGen = 15.3, HeatEfficiency = 0.75 } },
                { "int_powerplant_size5_class3", new ShipModule(128064050,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 5 Rating C"){ Cost = 480410, Class = 5, Rating = "C", Mass = 10, Integrity = 96, PowerGen = 17, HeatEfficiency = 0.5 } },
                { "int_powerplant_size5_class4", new ShipModule(128064051,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 5 Rating B"){ Cost = 1441230, Class = 5, Rating = "B", Mass = 16, Integrity = 115, PowerGen = 18.7, HeatEfficiency = 0.45 } },
                { "int_powerplant_size5_class5", new ShipModule(128064052,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 5 Rating A"){ Cost = 4323700, Class = 5, Rating = "A", Mass = 10, Integrity = 106, PowerGen = 20.4, HeatEfficiency = 0.4 } },
                { "int_powerplant_size6_class1", new ShipModule(128064053,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 6 Rating E"){ Cost = 160140, Class = 6, Rating = "E", Mass = 40, Integrity = 102, PowerGen = 16.8, HeatEfficiency = 1 } },
                { "int_powerplant_size6_class2", new ShipModule(128064054,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 6 Rating D"){ Cost = 480410, Class = 6, Rating = "D", Mass = 16, Integrity = 90, PowerGen = 18.9, HeatEfficiency = 0.75 } },
                { "int_powerplant_size6_class3", new ShipModule(128064055,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 6 Rating C"){ Cost = 1441230, Class = 6, Rating = "C", Mass = 20, Integrity = 113, PowerGen = 21, HeatEfficiency = 0.5 } },
                { "int_powerplant_size6_class4", new ShipModule(128064056,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 6 Rating B"){ Cost = 4323700, Class = 6, Rating = "B", Mass = 32, Integrity = 136, PowerGen = 23.1, HeatEfficiency = 0.45 } },
                { "int_powerplant_size6_class5", new ShipModule(128064057,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 6 Rating A"){ Cost = 12971100, Class = 6, Rating = "A", Mass = 20, Integrity = 124, PowerGen = 25.2, HeatEfficiency = 0.4 } },
                { "int_powerplant_size7_class1", new ShipModule(128064058,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 7 Rating E"){ Cost = 480410, Class = 7, Rating = "E", Mass = 80, Integrity = 118, PowerGen = 20, HeatEfficiency = 1 } },
                { "int_powerplant_size7_class2", new ShipModule(128064059,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 7 Rating D"){ Cost = 1441230, Class = 7, Rating = "D", Mass = 32, Integrity = 105, PowerGen = 22.5, HeatEfficiency = 0.75 } },
                { "int_powerplant_size7_class3", new ShipModule(128064060,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 7 Rating C"){ Cost = 4323700, Class = 7, Rating = "C", Mass = 40, Integrity = 131, PowerGen = 25, HeatEfficiency = 0.5 } },
                { "int_powerplant_size7_class4", new ShipModule(128064061,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 7 Rating B"){ Cost = 12971100, Class = 7, Rating = "B", Mass = 64, Integrity = 157, PowerGen = 27.5, HeatEfficiency = 0.45 } },
                { "int_powerplant_size7_class5", new ShipModule(128064062,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 7 Rating A"){ Cost = 38913290, Class = 7, Rating = "A", Mass = 40, Integrity = 144, PowerGen = 30, HeatEfficiency = 0.4 } },
                { "int_powerplant_size8_class1", new ShipModule(128064063,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 8 Rating E"){ Cost = 1441230, Class = 8, Rating = "E", Mass = 160, Integrity = 135, PowerGen = 24, HeatEfficiency = 1 } },
                { "int_powerplant_size8_class2", new ShipModule(128064064,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 8 Rating D"){ Cost = 4323700, Class = 8, Rating = "D", Mass = 64, Integrity = 120, PowerGen = 27, HeatEfficiency = 0.75 } },
                { "int_powerplant_size8_class3", new ShipModule(128064065,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 8 Rating C"){ Cost = 12971100, Class = 8, Rating = "C", Mass = 80, Integrity = 150, PowerGen = 30, HeatEfficiency = 0.5 } },
                { "int_powerplant_size8_class4", new ShipModule(128064066,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 8 Rating B"){ Cost = 38913290, Class = 8, Rating = "B", Mass = 128, Integrity = 180, PowerGen = 33, HeatEfficiency = 0.45 } },
                { "int_powerplant_size8_class5", new ShipModule(128064067,ShipModule.ModuleTypes.PowerPlant,"Power Plant Class 8 Rating A"){ Cost = 116739870, Class = 8, Rating = "A", Mass = 80, Integrity = 165, PowerGen = 36, HeatEfficiency = 0.4 } },

                // Pulse laser

                { "hpt_pulselaser_fixed_smallfree", new ShipModule(128049673,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Small Free"){ Cost = 2200, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.39, BootTime = 0, DPS = 7.885, Damage = 2.05, DistributorDraw = 0.3, ThermL = 0.33, Pierce = 20, Range = 3000, RateOfFire = 3.846, BurstInterval = 0.26, BreachDamage = 1.7, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_fixed_small", new ShipModule(128049381,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Small"){ Cost = 2200, Mount = "F", Class = 1, Rating = "F", Mass = 2, Integrity = 40, Power = 0.39, BootTime = 0, DPS = 7.885, Damage = 2.05, DistributorDraw = 0.3, ThermL = 0.33, Pierce = 20, Range = 3000, RateOfFire = 3.846, BurstInterval = 0.26, BreachDamage = 1.7, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_fixed_medium", new ShipModule(128049382,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Medium"){ Cost = 17600, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.6, BootTime = 0, DPS = 12.069, Damage = 3.5, DistributorDraw = 0.5, ThermL = 0.56, Pierce = 35, Range = 3000, RateOfFire = 3.448, BurstInterval = 0.29, BreachDamage = 3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_fixed_large", new ShipModule(128049383,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Large"){ Cost = 70400, Mount = "F", Class = 3, Rating = "D", Mass = 8, Integrity = 64, Power = 0.9, BootTime = 0, DPS = 18.121, Damage = 5.98, DistributorDraw = 0.86, ThermL = 0.96, Pierce = 52, Range = 3000, RateOfFire = 3.03, BurstInterval = 0.33, BreachDamage = 5.1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_fixed_huge", new ShipModule(128049384,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Huge"){ Cost = 177600, Mount = "F", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 1.33, BootTime = 0, DPS = 26.947, Damage = 10.24, DistributorDraw = 1.48, ThermL = 1.64, Pierce = 65, Range = 3000, RateOfFire = 2.632, BurstInterval = 0.38, BreachDamage = 8.7, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_gimbal_small", new ShipModule(128049385,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Small"){ Cost = 6600, Mount = "G", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.39, BootTime = 0, DPS = 6.24, Damage = 1.56, DistributorDraw = 0.31, ThermL = 0.31, Pierce = 20, Range = 3000, RateOfFire = 4, BurstInterval = 0.25, BreachDamage = 1.3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_gimbal_medium", new ShipModule(128049386,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Medium"){ Cost = 35400, Mount = "G", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 0.6, BootTime = 0, DPS = 9.571, Damage = 2.68, DistributorDraw = 0.54, ThermL = 0.54, Pierce = 35, Range = 3000, RateOfFire = 3.571, BurstInterval = 0.28, BreachDamage = 2.3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_gimbal_large", new ShipModule(128049387,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Large"){ Cost = 140600, Mount = "G", Class = 3, Rating = "E", Mass = 8, Integrity = 64, Power = 0.92, BootTime = 0, DPS = 14.774, Damage = 4.58, DistributorDraw = 0.92, ThermL = 0.92, Pierce = 52, Range = 3000, RateOfFire = 3.226, BurstInterval = 0.31, BreachDamage = 3.9, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_turret_small", new ShipModule(128049388,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Turret Small"){ Cost = 26000, Mount = "T", Class = 1, Rating = "G", Mass = 2, Integrity = 40, Power = 0.38, BootTime = 0, DPS = 3.967, Damage = 1.19, DistributorDraw = 0.19, ThermL = 0.19, Pierce = 20, Range = 3000, RateOfFire = 3.333, BurstInterval = 0.3, BreachDamage = 1, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_turret_medium", new ShipModule(128049389,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Turret Medium"){ Cost = 132800, Mount = "T", Class = 2, Rating = "F", Mass = 4, Integrity = 51, Power = 0.58, BootTime = 0, DPS = 6.212, Damage = 2.05, DistributorDraw = 0.33, ThermL = 0.33, Pierce = 35, Range = 3000, RateOfFire = 3.03, BurstInterval = 0.33, BreachDamage = 1.7, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },
                { "hpt_pulselaser_turret_large", new ShipModule(128049390,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Turret Large"){ Cost = 400400, Mount = "T", Class = 3, Rating = "F", Mass = 8, Integrity = 64, Power = 0.89, BootTime = 0, DPS = 9.459, Damage = 3.5, DistributorDraw = 0.56, ThermL = 0.56, Pierce = 52, Range = 3000, RateOfFire = 2.703, BurstInterval = 0.37, BreachDamage = 3, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },


                { "hpt_pulselaser_gimbal_huge", new ShipModule(128681995,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Huge"){ Cost = 877600, Mount = "G", Class = 4, Rating = "A", Mass = 16, Integrity = 80, Power = 1.37, BootTime = 0, DPS = 21.722, Damage = 7.82, DistributorDraw = 1.56, ThermL = 1.56, Pierce = 65, Range = 3000, RateOfFire = 2.778, BurstInterval = 0.36, BreachDamage = 6.6, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },

                { "hpt_pulselaser_fixed_medium_disruptor", new ShipModule(128671342,ShipModule.ModuleTypes.PulseDisruptorLaser,"Pulse Disruptor Laser Fixed Medium"){ Cost = 26400, Mount = "F", Class = 2, Rating = "E", Mass = 4, Integrity = 51, Power = 0.7, BootTime = 0, DPS = 4.667, Damage = 2.8, DistributorDraw = 0.9, ThermL = 1, Pierce = 35, Range = 3000, RateOfFire = 1.667, BurstInterval = 0.6, BreachDamage = 2.4, BreachMin = 40, BreachMax = 80, ThermalProportion = 100, Falloff = 500 } },

                // Pulse Wave Analyser

                { "hpt_mrascanner_size0_class1", new ShipModule(128915718,ShipModule.ModuleTypes.PulseWaveAnalyser,"Pulse Wave Analyser Rating E"){ Cost = 13550, Class = 0, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.2, BootTime = 3, Range = 12000, Angle = 15, Time = 3 } },
                { "hpt_mrascanner_size0_class2", new ShipModule(128915719,ShipModule.ModuleTypes.PulseWaveAnalyser,"Pulse Wave Analyser Rating D"){ Cost = 40630, Class = 0, Rating = "D", Mass = 1.3, Integrity = 24, Power = 0.4, BootTime = 3, Range = 15000, Angle = 15, Time = 3 } },
                { "hpt_mrascanner_size0_class3", new ShipModule(128915720,ShipModule.ModuleTypes.PulseWaveAnalyser,"Pulse Wave Analyser Rating C"){ Cost = 121900, Class = 0, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.8, BootTime = 3, Range = 18000, Angle = 15, Time = 3 } },
                { "hpt_mrascanner_size0_class4", new ShipModule(128915721,ShipModule.ModuleTypes.PulseWaveAnalyser,"Pulse Wave Analyser Rating B"){ Cost = 365700, Class = 0, Rating = "B", Mass = 1.3, Integrity = 56, Power = 1.6, BootTime = 3, Range = 21000, Angle = 15, Time = 3 } },
                { "hpt_mrascanner_size0_class5", new ShipModule(128915722,ShipModule.ModuleTypes.PulseWaveAnalyser,"Pulse Wave Analyser Rating A"){ Cost = 1097100, Class = 0, Rating = "A", Mass = 1.3, Integrity = 48, Power = 3.2, BootTime = 3, Range = 24000, Angle = 15, Time = 3 } },

                // Rail guns

                { "hpt_railgun_fixed_small", new ShipModule(128049488,ShipModule.ModuleTypes.RailGun,"Rail Gun Fixed Small"){ Cost = 51600, Mount = "F", Class = 1, Rating = "D", Mass = 2, Integrity = 40, Power = 1.15, BootTime = 0, DPS = 37.048, Damage = 23.34, DistributorDraw = 2.69, ThermL = 12, Pierce = 100, Range = 3000, Time = 1, RateOfFire = 1.587, BurstInterval = 0.63, Clip = 1, Ammo = 80, ReloadTime = 1, BreachDamage = 22.2, BreachMin = 40, BreachMax = 80, ThermalProportion = 66.667, KineticProportion = 33.333, Falloff = 1000, AmmoCost = 75 } },
                { "hpt_railgun_fixed_medium", new ShipModule(128049489,ShipModule.ModuleTypes.RailGun,"Rail Gun Fixed Medium"){ Cost = 412800, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.63, BootTime = 0, DPS = 50.036, Damage = 41.53, DistributorDraw = 5.11, ThermL = 20, Pierce = 100, Range = 3000, Time = 1.2, RateOfFire = 1.205, BurstInterval = 0.83, Clip = 1, Ammo = 80, ReloadTime = 1, BreachDamage = 39.5, BreachMin = 40, BreachMax = 80, ThermalProportion = 66.675, KineticProportion = 33.325, Falloff = 1000, AmmoCost = 75 } },
                { "hpt_railgun_fixed_medium_burst", new ShipModule(128671341,ShipModule.ModuleTypes.ImperialHammerRailGun,"Imperial Hammer Rail Gun Fixed Medium"){ Cost = 619200, Mount = "F", Class = 2, Rating = "B", Mass = 4, Integrity = 51, Power = 1.63, BootTime = 0, DPS = 61.364, Damage = 15, DistributorDraw = 2, ThermL = 11, Pierce = 100, Range = 3000, Time = 1.2, RateOfFire = 4.091, BurstInterval = 0.4, BurstRateOfFire = 6, BurstSize = 3, Clip = 3, Ammo = 240, ReloadTime = 1.2, BreachDamage = 14.3, BreachMin = 40, BreachMax = 80, ThermalProportion = 66.667, KineticProportion = 33.333, Falloff = 1000, AmmoCost = 75 } },

                // Refineries

                { "int_refinery_size1_class1", new ShipModule(128666684,ShipModule.ModuleTypes.Refinery,"Refinery Class 1 Rating E"){ Cost = 6000, Class = 1, Rating = "E", Integrity = 32, Power = 0.14, BootTime = 10, Bins = 1 } },
                { "int_refinery_size2_class1", new ShipModule(128666685,ShipModule.ModuleTypes.Refinery,"Refinery Class 2 Rating E"){ Cost = 12600, Class = 2, Rating = "E", Integrity = 41, Power = 0.17, BootTime = 10, Bins = 2 } },
                { "int_refinery_size3_class1", new ShipModule(128666686,ShipModule.ModuleTypes.Refinery,"Refinery Class 3 Rating E"){ Cost = 26460, Class = 3, Rating = "E", Integrity = 51, Power = 0.2, BootTime = 10, Bins = 3 } },
                { "int_refinery_size4_class1", new ShipModule(128666687,ShipModule.ModuleTypes.Refinery,"Refinery Class 4 Rating E"){ Cost = 55570, Class = 4, Rating = "E", Integrity = 64, Power = 0.25, BootTime = 10, Bins = 4 } },
                { "int_refinery_size1_class2", new ShipModule(128666688,ShipModule.ModuleTypes.Refinery,"Refinery Class 1 Rating D"){ Cost = 18000, Class = 1, Rating = "D", Integrity = 24, Power = 0.18, BootTime = 10, Bins = 1 } },
                { "int_refinery_size2_class2", new ShipModule(128666689,ShipModule.ModuleTypes.Refinery,"Refinery Class 2 Rating D"){ Cost = 37800, Class = 2, Rating = "D", Integrity = 31, Power = 0.22, BootTime = 10, Bins = 3 } },
                { "int_refinery_size3_class2", new ShipModule(128666690,ShipModule.ModuleTypes.Refinery,"Refinery Class 3 Rating D"){ Cost = 79380, Class = 3, Rating = "D", Integrity = 38, Power = 0.27, BootTime = 10, Bins = 4 } },
                { "int_refinery_size4_class2", new ShipModule(128666691,ShipModule.ModuleTypes.Refinery,"Refinery Class 4 Rating D"){ Cost = 166700, Class = 4, Rating = "D", Integrity = 48, Power = 0.33, BootTime = 10, Bins = 5 } },
                { "int_refinery_size1_class3", new ShipModule(128666692,ShipModule.ModuleTypes.Refinery,"Refinery Class 1 Rating C"){ Cost = 54000, Class = 1, Rating = "C", Integrity = 40, Power = 0.23, BootTime = 10, Bins = 2 } },
                { "int_refinery_size2_class3", new ShipModule(128666693,ShipModule.ModuleTypes.Refinery,"Refinery Class 2 Rating C"){ Cost = 113400, Class = 2, Rating = "C", Integrity = 51, Power = 0.28, BootTime = 10, Bins = 4 } },
                { "int_refinery_size3_class3", new ShipModule(128666694,ShipModule.ModuleTypes.Refinery,"Refinery Class 3 Rating C"){ Cost = 238140, Class = 3, Rating = "C", Integrity = 64, Power = 0.34, BootTime = 10, Bins = 6 } },
                { "int_refinery_size4_class3", new ShipModule(128666695,ShipModule.ModuleTypes.Refinery,"Refinery Class 4 Rating C"){ Cost = 500090, Class = 4, Rating = "C", Integrity = 80, Power = 0.41, BootTime = 10, Bins = 7 } },
                { "int_refinery_size1_class4", new ShipModule(128666696,ShipModule.ModuleTypes.Refinery,"Refinery Class 1 Rating B"){ Cost = 162000, Class = 1, Rating = "B", Integrity = 56, Power = 0.28, BootTime = 10, Bins = 3 } },
                { "int_refinery_size2_class4", new ShipModule(128666697,ShipModule.ModuleTypes.Refinery,"Refinery Class 2 Rating B"){ Cost = 340200, Class = 2, Rating = "B", Integrity = 71, Power = 0.34, BootTime = 10, Bins = 5 } },
                { "int_refinery_size3_class4", new ShipModule(128666698,ShipModule.ModuleTypes.Refinery,"Refinery Class 3 Rating B"){ Cost = 714420, Class = 3, Rating = "B", Integrity = 90, Power = 0.41, BootTime = 10, Bins = 7 } },
                { "int_refinery_size4_class4", new ShipModule(128666699,ShipModule.ModuleTypes.Refinery,"Refinery Class 4 Rating B"){ Cost = 1500280, Class = 4, Rating = "B", Integrity = 112, Power = 0.49, BootTime = 10, Bins = 9 } },
                { "int_refinery_size1_class5", new ShipModule(128666700,ShipModule.ModuleTypes.Refinery,"Refinery Class 1 Rating A"){ Cost = 486000, Class = 1, Rating = "A", Integrity = 48, Power = 0.32, BootTime = 10, Bins = 4 } },
                { "int_refinery_size2_class5", new ShipModule(128666701,ShipModule.ModuleTypes.Refinery,"Refinery Class 2 Rating A"){ Cost = 1020600, Class = 2, Rating = "A", Integrity = 61, Power = 0.39, BootTime = 10, Bins = 6 } },
                { "int_refinery_size3_class5", new ShipModule(128666702,ShipModule.ModuleTypes.Refinery,"Refinery Class 3 Rating A"){ Cost = 2143260, Class = 3, Rating = "A", Integrity = 77, Power = 0.48, BootTime = 10, Bins = 8 } },
                { "int_refinery_size4_class5", new ShipModule(128666703,ShipModule.ModuleTypes.Refinery,"Refinery Class 4 Rating A"){ Cost = 4500850, Class = 4, Rating = "A", Integrity = 96, Power = 0.57, BootTime = 10, Bins = 10 } },

                // Sensors

                { "int_sensors_size1_class1_free", new ShipModule(128666640,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 36, Power = 0.16, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4000 } },
                { "int_sensors_size1_class1", new ShipModule(128064218,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 36, Power = 0.16, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4000 } },
                { "int_sensors_size1_class2", new ShipModule(128064219,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating D"){ Cost = 1290, Class = 1, Rating = "D", Mass = 0.5, Integrity = 32, Power = 0.18, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4500 } },
                { "int_sensors_size1_class3", new ShipModule(128064220,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating C"){ Cost = 3230, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.2, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5000 } },
                { "int_sensors_size1_class4", new ShipModule(128064221,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating B"){ Cost = 8080, Class = 1, Rating = "B", Mass = 2, Integrity = 48, Power = 0.33, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5500 } },
                { "int_sensors_size1_class5", new ShipModule(128064222,ShipModule.ModuleTypes.Sensors,"Sensors Class 1 Rating A"){ Cost = 20200, Class = 1, Rating = "A", Mass = 1.3, Integrity = 44, Power = 0.6, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6000 } },
                { "int_sensors_size2_class1", new ShipModule(128064223,ShipModule.ModuleTypes.Sensors,"Sensors Class 2 Rating E"){ Cost = 1450, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, Power = 0.18, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4160 } },
                { "int_sensors_size2_class2", new ShipModule(128064224,ShipModule.ModuleTypes.Sensors,"Sensors Class 2 Rating D"){ Cost = 3620, Class = 2, Rating = "D", Mass = 1, Integrity = 41, Power = 0.21, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4680 } },
                { "int_sensors_size2_class3", new ShipModule(128064225,ShipModule.ModuleTypes.Sensors,"Sensors Class 2 Rating C"){ Cost = 9050, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.23, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5200 } },
                { "int_sensors_size2_class4", new ShipModule(128064226,ShipModule.ModuleTypes.Sensors,"Sensors Class 2 Rating B"){ Cost = 22620, Class = 2, Rating = "B", Mass = 4, Integrity = 61, Power = 0.38, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5720 } },
                { "int_sensors_size2_class5", new ShipModule(128064227,ShipModule.ModuleTypes.Sensors,"Sensors Class 2 Rating A"){ Cost = 56550, Class = 2, Rating = "A", Mass = 2.5, Integrity = 56, Power = 0.69, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6240 } },
                { "int_sensors_size3_class1", new ShipModule(128064228,ShipModule.ModuleTypes.Sensors,"Sensors Class 3 Rating E"){ Cost = 4050, Class = 3, Rating = "E", Mass = 5, Integrity = 58, Power = 0.22, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4320 } },
                { "int_sensors_size3_class2", new ShipModule(128064229,ShipModule.ModuleTypes.Sensors,"Sensors Class 3 Rating D"){ Cost = 10130, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 0.25, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4860 } },
                { "int_sensors_size3_class3", new ShipModule(128064230,ShipModule.ModuleTypes.Sensors,"Sensors Class 3 Rating C"){ Cost = 25330, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 0.28, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5400 } },
                { "int_sensors_size3_class4", new ShipModule(128064231,ShipModule.ModuleTypes.Sensors,"Sensors Class 3 Rating B"){ Cost = 63330, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 0.46, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5940 } },
                { "int_sensors_size3_class5", new ShipModule(128064232,ShipModule.ModuleTypes.Sensors,"Sensors Class 3 Rating A"){ Cost = 158330, Class = 3, Rating = "A", Mass = 5, Integrity = 70, Power = 0.84, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6480 } },
                { "int_sensors_size4_class1", new ShipModule(128064233,ShipModule.ModuleTypes.Sensors,"Sensors Class 4 Rating E"){ Cost = 11350, Class = 4, Rating = "E", Mass = 10, Integrity = 72, Power = 0.27, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4480 } },
                { "int_sensors_size4_class2", new ShipModule(128064234,ShipModule.ModuleTypes.Sensors,"Sensors Class 4 Rating D"){ Cost = 28370, Class = 4, Rating = "D", Mass = 4, Integrity = 64, Power = 0.31, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5040 } },
                { "int_sensors_size4_class3", new ShipModule(128064235,ShipModule.ModuleTypes.Sensors,"Sensors Class 4 Rating C"){ Cost = 70930, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 0.34, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5600 } },
                { "int_sensors_size4_class4", new ShipModule(128064236,ShipModule.ModuleTypes.Sensors,"Sensors Class 4 Rating B"){ Cost = 177330, Class = 4, Rating = "B", Mass = 16, Integrity = 96, Power = 0.56, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6160 } },
                { "int_sensors_size4_class5", new ShipModule(128064237,ShipModule.ModuleTypes.Sensors,"Sensors Class 4 Rating A"){ Cost = 443330, Class = 4, Rating = "A", Mass = 10, Integrity = 88, Power = 1.02, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6720 } },
                { "int_sensors_size5_class1", new ShipModule(128064238,ShipModule.ModuleTypes.Sensors,"Sensors Class 5 Rating E"){ Cost = 31780, Class = 5, Rating = "E", Mass = 20, Integrity = 86, Power = 0.33, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4640 } },
                { "int_sensors_size5_class2", new ShipModule(128064239,ShipModule.ModuleTypes.Sensors,"Sensors Class 5 Rating D"){ Cost = 79440, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 0.37, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5220 } },
                { "int_sensors_size5_class3", new ShipModule(128064240,ShipModule.ModuleTypes.Sensors,"Sensors Class 5 Rating C"){ Cost = 198610, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 0.41, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5800 } },
                { "int_sensors_size5_class4", new ShipModule(128064241,ShipModule.ModuleTypes.Sensors,"Sensors Class 5 Rating B"){ Cost = 496530, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 0.68, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6380 } },
                { "int_sensors_size5_class5", new ShipModule(128064242,ShipModule.ModuleTypes.Sensors,"Sensors Class 5 Rating A"){ Cost = 1241320, Class = 5, Rating = "A", Mass = 20, Integrity = 106, Power = 1.23, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6960 } },
                { "int_sensors_size6_class1", new ShipModule(128064243,ShipModule.ModuleTypes.Sensors,"Sensors Class 6 Rating E"){ Cost = 88980, Class = 6, Rating = "E", Mass = 40, Integrity = 102, Power = 0.4, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4800 } },
                { "int_sensors_size6_class2", new ShipModule(128064244,ShipModule.ModuleTypes.Sensors,"Sensors Class 6 Rating D"){ Cost = 222440, Class = 6, Rating = "D", Mass = 16, Integrity = 90, Power = 0.45, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5400 } },
                { "int_sensors_size6_class3", new ShipModule(128064245,ShipModule.ModuleTypes.Sensors,"Sensors Class 6 Rating C"){ Cost = 556110, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 0.5, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6000 } },
                { "int_sensors_size6_class4", new ShipModule(128064246,ShipModule.ModuleTypes.Sensors,"Sensors Class 6 Rating B"){ Cost = 1390280, Class = 6, Rating = "B", Mass = 64, Integrity = 136, Power = 0.83, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6600 } },
                { "int_sensors_size6_class5", new ShipModule(128064247,ShipModule.ModuleTypes.Sensors,"Sensors Class 6 Rating A"){ Cost = 3475690, Class = 6, Rating = "A", Mass = 40, Integrity = 124, Power = 1.5, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 7200 } },
                { "int_sensors_size7_class1", new ShipModule(128064248,ShipModule.ModuleTypes.Sensors,"Sensors Class 7 Rating E"){ Cost = 249140, Class = 7, Rating = "E", Mass = 80, Integrity = 118, Power = 0.47, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 4960 } },
                { "int_sensors_size7_class2", new ShipModule(128064249,ShipModule.ModuleTypes.Sensors,"Sensors Class 7 Rating D"){ Cost = 622840, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 0.53, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5580 } },
                { "int_sensors_size7_class3", new ShipModule(128064250,ShipModule.ModuleTypes.Sensors,"Sensors Class 7 Rating C"){ Cost = 1557110, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 0.59, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6200 } },
                { "int_sensors_size7_class4", new ShipModule(128064251,ShipModule.ModuleTypes.Sensors,"Sensors Class 7 Rating B"){ Cost = 3892770, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 0.97, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6820 } },
                { "int_sensors_size7_class5", new ShipModule(128064252,ShipModule.ModuleTypes.Sensors,"Sensors Class 7 Rating A"){ Cost = 9731930, Class = 7, Rating = "A", Mass = 80, Integrity = 144, Power = 1.77, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 7440 } },
                { "int_sensors_size8_class1", new ShipModule(128064253,ShipModule.ModuleTypes.Sensors,"Sensors Class 8 Rating E"){ Cost = 697580, Class = 8, Rating = "E", Mass = 160, Integrity = 135, Power = 0.55, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5120 } },
                { "int_sensors_size8_class2", new ShipModule(128064254,ShipModule.ModuleTypes.Sensors,"Sensors Class 8 Rating D"){ Cost = 1743960, Class = 8, Rating = "D", Mass = 64, Integrity = 120, Power = 0.62, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 5760 } },
                { "int_sensors_size8_class3", new ShipModule(128064255,ShipModule.ModuleTypes.Sensors,"Sensors Class 8 Rating C"){ Cost = 4359900, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 0.69, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 6400 } },
                { "int_sensors_size8_class4", new ShipModule(128064256,ShipModule.ModuleTypes.Sensors,"Sensors Class 8 Rating B"){ Cost = 10899760, Class = 8, Rating = "B", Mass = 256, Integrity = 180, Power = 1.14, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 7040 } },
                { "int_sensors_size8_class5", new ShipModule(128064257,ShipModule.ModuleTypes.Sensors,"Sensors Class 8 Rating A"){ Cost = 27249390, Class = 8, Rating = "A", Mass = 160, Integrity = 165, Power = 2.07, BootTime = 5, Range = 8, Angle = 30, TypicalEmission = 7680 } },


                // Shield Boosters

                { "hpt_shieldbooster_size0_class1", new ShipModule(128668532,ShipModule.ModuleTypes.ShieldBooster,"Shield Booster Rating E"){ Cost = 10000, Class = 0, Rating = "E", Mass = 0.5, Integrity = 25, Power = 0.2, BootTime = 0, ShieldReinforcement = 4, Kinetic = 0, Thermal = 0, Explosive = 0 } },
                { "hpt_shieldbooster_size0_class2", new ShipModule(128668533,ShipModule.ModuleTypes.ShieldBooster,"Shield Booster Rating D"){ Cost = 23000, Class = 0, Rating = "D", Mass = 1, Integrity = 35, Power = 0.5, BootTime = 0, ShieldReinforcement = 8, Kinetic = 0, Thermal = 0, Explosive = 0 } },
                { "hpt_shieldbooster_size0_class3", new ShipModule(128668534,ShipModule.ModuleTypes.ShieldBooster,"Shield Booster Rating C"){ Cost = 53000, Class = 0, Rating = "C", Mass = 2, Integrity = 40, Power = 0.7, BootTime = 0, ShieldReinforcement = 12, Kinetic = 0, Thermal = 0, Explosive = 0 } },
                { "hpt_shieldbooster_size0_class4", new ShipModule(128668535,ShipModule.ModuleTypes.ShieldBooster,"Shield Booster Rating B"){ Cost = 122000, Class = 0, Rating = "B", Mass = 3, Integrity = 45, Power = 1, BootTime = 0, ShieldReinforcement = 16, Kinetic = 0, Thermal = 0, Explosive = 0 } },
                { "hpt_shieldbooster_size0_class5", new ShipModule(128668536,ShipModule.ModuleTypes.ShieldBooster,"Shield Booster Rating A"){ Cost = 281000, Class = 0, Rating = "A", Mass = 3.5, Integrity = 48, Power = 1.2, BootTime = 0, ShieldReinforcement = 20, Kinetic = 0, Thermal = 0, Explosive = 0 } },

                // cell banks

                { "int_shieldcellbank_size1_class1", new ShipModule(128064298,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 1 Rating E"){ Cost = 520, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.41, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1, ShieldReinforcement = 12, SCBHeat = 170, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size1_class2", new ShipModule(128064299,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 1 Rating D"){ Cost = 1290, Class = 1, Rating = "D", Mass = 0.5, Integrity = 24, Power = 0.55, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1, ShieldReinforcement = 16, SCBHeat = 170, Clip = 1, Ammo = 1, AmmoCost = 300 } },
                { "int_shieldcellbank_size1_class3", new ShipModule(128064300,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 1 Rating C"){ Cost = 3230, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 0.69, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1, ShieldReinforcement = 20, SCBHeat = 170, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size1_class4", new ShipModule(128064301,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 1 Rating B"){ Cost = 8080, Class = 1, Rating = "B", Mass = 2, Integrity = 56, Power = 0.83, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1, ShieldReinforcement = 24, SCBHeat = 170, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size1_class5", new ShipModule(128064302,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 1 Rating A"){ Cost = 20200, Class = 1, Rating = "A", Mass = 1.3, Integrity = 48, Power = 0.97, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1, ShieldReinforcement = 28, SCBHeat = 170, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size2_class1", new ShipModule(128064303,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 2 Rating E"){ Cost = 1450, Class = 2, Rating = "E", Mass = 2.5, Integrity = 41, Power = 0.5, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1.5, ShieldReinforcement = 14, SCBHeat = 240, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size2_class2", new ShipModule(128064304,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 2 Rating D"){ Cost = 3620, Class = 2, Rating = "D", Mass = 1, Integrity = 31, Power = 0.67, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1.5, ShieldReinforcement = 18, SCBHeat = 240, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size2_class3", new ShipModule(128064305,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 2 Rating C"){ Cost = 9050, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 0.84, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1.5, ShieldReinforcement = 23, SCBHeat = 240, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size2_class4", new ShipModule(128064306,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 2 Rating B"){ Cost = 22620, Class = 2, Rating = "B", Mass = 4, Integrity = 71, Power = 1.01, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1.5, ShieldReinforcement = 28, SCBHeat = 240, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size2_class5", new ShipModule(128064307,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 2 Rating A"){ Cost = 56550, Class = 2, Rating = "A", Mass = 2.5, Integrity = 61, Power = 1.18, BootTime = 25, SCBSpinUp = 5, SCBDuration = 1.5, ShieldReinforcement = 32, SCBHeat = 240, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size3_class1", new ShipModule(128064308,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 3 Rating E"){ Cost = 4050, Class = 3, Rating = "E", Mass = 5, Integrity = 51, Power = 0.61, BootTime = 25, SCBSpinUp = 5, SCBDuration = 2.3, ShieldReinforcement = 17, SCBHeat = 340, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size3_class2", new ShipModule(128064309,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 3 Rating D"){ Cost = 10130, Class = 3, Rating = "D", Mass = 2, Integrity = 38, Power = 0.82, BootTime = 25, SCBSpinUp = 5, SCBDuration = 2.3, ShieldReinforcement = 23, SCBHeat = 340, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size3_class3", new ShipModule(128064310,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 3 Rating C"){ Cost = 25330, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 1.02, BootTime = 25, SCBSpinUp = 5, SCBDuration = 2.3, ShieldReinforcement = 29, SCBHeat = 340, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size3_class4", new ShipModule(128064311,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 3 Rating B"){ Cost = 63330, Class = 3, Rating = "B", Mass = 8, Integrity = 90, Power = 1.22, BootTime = 25, SCBSpinUp = 5, SCBDuration = 2.3, ShieldReinforcement = 35, SCBHeat = 340, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size3_class5", new ShipModule(128064312,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 3 Rating A"){ Cost = 158330, Class = 3, Rating = "A", Mass = 5, Integrity = 77, Power = 1.43, BootTime = 25, SCBSpinUp = 5, SCBDuration = 2.3, ShieldReinforcement = 41, SCBHeat = 340, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size4_class1", new ShipModule(128064313,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 4 Rating E"){ Cost = 11350, Class = 4, Rating = "E", Mass = 10, Integrity = 64, Power = 0.74, BootTime = 25, SCBSpinUp = 5, SCBDuration = 3.4, ShieldReinforcement = 20, SCBHeat = 410, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size4_class2", new ShipModule(128064314,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 4 Rating D"){ Cost = 28370, Class = 4, Rating = "D", Mass = 4, Integrity = 48, Power = 0.98, BootTime = 25, SCBSpinUp = 5, SCBDuration = 3.4, ShieldReinforcement = 26, SCBHeat = 410, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size4_class3", new ShipModule(128064315,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 4 Rating C"){ Cost = 70930, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 1.23, BootTime = 25, SCBSpinUp = 5, SCBDuration = 3.4, ShieldReinforcement = 33, SCBHeat = 410, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size4_class4", new ShipModule(128064316,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 4 Rating B"){ Cost = 177330, Class = 4, Rating = "B", Mass = 16, Integrity = 112, Power = 1.48, BootTime = 25, SCBSpinUp = 5, SCBDuration = 3.4, ShieldReinforcement = 39, SCBHeat = 410, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size4_class5", new ShipModule(128064317,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 4 Rating A"){ Cost = 443330, Class = 4, Rating = "A", Mass = 10, Integrity = 96, Power = 1.72, BootTime = 25, SCBSpinUp = 5, SCBDuration = 3.4, ShieldReinforcement = 46, SCBHeat = 410, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size5_class1", new ShipModule(128064318,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 5 Rating E"){ Cost = 31780, Class = 5, Rating = "E", Mass = 20, Integrity = 77, Power = 0.9, BootTime = 25, SCBSpinUp = 5, SCBDuration = 5.1, ShieldReinforcement = 21, SCBHeat = 540, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size5_class2", new ShipModule(128064319,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 5 Rating D"){ Cost = 79440, Class = 5, Rating = "D", Mass = 8, Integrity = 58, Power = 1.2, BootTime = 25, SCBSpinUp = 5, SCBDuration = 5.1, ShieldReinforcement = 28, SCBHeat = 540, Clip = 1, Ammo = 2, AmmoCost = 300 } },
                { "int_shieldcellbank_size5_class3", new ShipModule(128064320,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 5 Rating C"){ Cost = 198610, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 1.5, BootTime = 25, SCBSpinUp = 5, SCBDuration = 5.1, ShieldReinforcement = 35, SCBHeat = 540, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size5_class4", new ShipModule(128064321,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 5 Rating B"){ Cost = 496530, Class = 5, Rating = "B", Mass = 32, Integrity = 134, Power = 1.8, BootTime = 25, SCBSpinUp = 5, SCBDuration = 5.1, ShieldReinforcement = 41, SCBHeat = 540, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size5_class5", new ShipModule(128064322,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 5 Rating A"){ Cost = 1241320, Class = 5, Rating = "A", Mass = 20, Integrity = 115, Power = 2.1, BootTime = 25, SCBSpinUp = 5, SCBDuration = 5.1, ShieldReinforcement = 48, SCBHeat = 540, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size6_class1", new ShipModule(128064323,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 6 Rating E"){ Cost = 88980, Class = 6, Rating = "E", Mass = 40, Integrity = 90, Power = 1.06, BootTime = 25, SCBSpinUp = 5, SCBDuration = 7.6, ShieldReinforcement = 20, SCBHeat = 640, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size6_class2", new ShipModule(128064324,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 6 Rating D"){ Cost = 222440, Class = 6, Rating = "D", Mass = 16, Integrity = 68, Power = 1.42, BootTime = 25, SCBSpinUp = 5, SCBDuration = 7.6, ShieldReinforcement = 26, SCBHeat = 640, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size6_class3", new ShipModule(128064325,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 6 Rating C"){ Cost = 556110, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 1.77, BootTime = 25, SCBSpinUp = 5, SCBDuration = 7.6, ShieldReinforcement = 33, SCBHeat = 640, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size6_class4", new ShipModule(128064326,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 6 Rating B"){ Cost = 1390280, Class = 6, Rating = "B", Mass = 64, Integrity = 158, Power = 2.12, BootTime = 25, SCBSpinUp = 5, SCBDuration = 7.6, ShieldReinforcement = 39, SCBHeat = 640, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size6_class5", new ShipModule(128064327,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 6 Rating A"){ Cost = 3475690, Class = 6, Rating = "A", Mass = 40, Integrity = 136, Power = 2.48, BootTime = 25, SCBSpinUp = 5, SCBDuration = 7.6, ShieldReinforcement = 46, SCBHeat = 640, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size7_class1", new ShipModule(128064328,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 7 Rating E"){ Cost = 249140, Class = 7, Rating = "E", Mass = 80, Integrity = 105, Power = 1.24, BootTime = 25, SCBSpinUp = 5, SCBDuration = 11.4, ShieldReinforcement = 24, SCBHeat = 720, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size7_class2", new ShipModule(128064329,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 7 Rating D"){ Cost = 622840, Class = 7, Rating = "D", Mass = 32, Integrity = 79, Power = 1.66, BootTime = 25, SCBSpinUp = 5, SCBDuration = 11.4, ShieldReinforcement = 32, SCBHeat = 720, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size7_class3", new ShipModule(128064330,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 7 Rating C"){ Cost = 1557110, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 2.07, BootTime = 25, SCBSpinUp = 5, SCBDuration = 11.4, ShieldReinforcement = 41, SCBHeat = 720, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size7_class4", new ShipModule(128064331,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 7 Rating B"){ Cost = 3892770, Class = 7, Rating = "B", Mass = 128, Integrity = 183, Power = 2.48, BootTime = 25, SCBSpinUp = 5, SCBDuration = 11.4, ShieldReinforcement = 49, SCBHeat = 720, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size7_class5", new ShipModule(128064332,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 7 Rating A"){ Cost = 9731930, Class = 7, Rating = "A", Mass = 80, Integrity = 157, Power = 2.9, BootTime = 25, SCBSpinUp = 5, SCBDuration = 11.4, ShieldReinforcement = 57, SCBHeat = 720, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size8_class1", new ShipModule(128064333,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 8 Rating E"){ Cost = 697580, Class = 8, Rating = "E", Mass = 160, Integrity = 120, Power = 1.44, BootTime = 25, SCBSpinUp = 5, SCBDuration = 17.1, ShieldReinforcement = 28, SCBHeat = 800, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size8_class2", new ShipModule(128064334,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 8 Rating D"){ Cost = 1743960, Class = 8, Rating = "D", Mass = 64, Integrity = 90, Power = 1.92, BootTime = 25, SCBSpinUp = 5, SCBDuration = 17.1, ShieldReinforcement = 37, SCBHeat = 800, Clip = 1, Ammo = 3, AmmoCost = 300 } },
                { "int_shieldcellbank_size8_class3", new ShipModule(128064335,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 8 Rating C"){ Cost = 4359900, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 2.4, BootTime = 25, SCBSpinUp = 5, SCBDuration = 17.1, ShieldReinforcement = 47, SCBHeat = 800, Clip = 1, Ammo = 4, AmmoCost = 300 } },
                { "int_shieldcellbank_size8_class4", new ShipModule(128064336,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 8 Rating B"){ Cost = 10899760, Class = 8, Rating = "B", Mass = 256, Integrity = 210, Power = 2.88, BootTime = 25, SCBSpinUp = 5, SCBDuration = 17.1, ShieldReinforcement = 56, SCBHeat = 800, Clip = 1, Ammo = 5, AmmoCost = 300 } },
                { "int_shieldcellbank_size8_class5", new ShipModule(128064337,ShipModule.ModuleTypes.ShieldCellBank,"Shield Cell Bank Class 8 Rating A"){ Cost = 27249390, Class = 8, Rating = "A", Mass = 160, Integrity = 180, Power = 3.36, BootTime = 25, SCBSpinUp = 5, SCBDuration = 17.1, ShieldReinforcement = 65, SCBHeat = 800, Clip = 1, Ammo = 4, AmmoCost = 300 } },

                // Shield Generators
                // ship.sheilds

                { "int_shieldgenerator_size2_class1_free", new ShipModule(128666641,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating E"){ Cost = 300, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.72, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size1_class1", new ShipModule(128064258,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 1 Rating E"){ Cost = 300, Class = 1, Rating = "E", Mass = 1.3, Integrity = 32, Power = 0.72, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size1_class2", new ShipModule(128064259,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 1 Rating E"){ Cost = 1240, Class = 1, Rating = "D", Mass = 0.5, Integrity = 24, Power = 0.96, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size1_class3", new ShipModule(128064260,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 1 Rating E"){ Cost = 5140, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 1.2, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size1_class5", new ShipModule(128064262,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 1 Rating A"){ Cost = 88075, Class = 1, Rating = "A", Mass = 1.3, Integrity = 48, Power = 1.68, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class1", new ShipModule(128064263,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 41, Power = 0.9, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class2", new ShipModule(128064264,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating D"){ Cost = 5930, Class = 2, Rating = "D", Mass = 1, Integrity = 31, Power = 1.2, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class3", new ShipModule(128064265,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating C"){ Cost = 17800, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 1.5, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class4", new ShipModule(128064266,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating B"){ Cost = 53410, Class = 2, Rating = "B", Mass = 4, Integrity = 71, Power = 1.8, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class5", new ShipModule(128064267,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 2 Rating A"){ Cost = 160220, Class = 2, Rating = "A", Mass = 2.5, Integrity = 61, Power = 2.1, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1, BrokenRegenRate = 1.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class1", new ShipModule(128064268,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 3 Rating E"){ Cost = 6270, Class = 3, Rating = "E", Mass = 5, Integrity = 51, Power = 1.08, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 1.87, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class2", new ShipModule(128064269,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 3 Rating D"){ Cost = 18810, Class = 3, Rating = "D", Mass = 2, Integrity = 38, Power = 1.44, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1, BrokenRegenRate = 1.87, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class3", new ShipModule(128064270,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 3 Rating C"){ Cost = 56440, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 1.8, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1, BrokenRegenRate = 1.87, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class4", new ShipModule(128064271,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 3 Rating B"){ Cost = 169300, Class = 3, Rating = "B", Mass = 8, Integrity = 90, Power = 2.16, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1, BrokenRegenRate = 1.87, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class5", new ShipModule(128064272,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 3 Rating A"){ Cost = 507910, Class = 3, Rating = "A", Mass = 5, Integrity = 77, Power = 2.52, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1, BrokenRegenRate = 1.87, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class1", new ShipModule(128064273,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 4 Rating E"){ Cost = 19880, Class = 4, Rating = "E", Mass = 10, Integrity = 64, Power = 1.32, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 2.53, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class2", new ShipModule(128064274,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 4 Rating D"){ Cost = 59630, Class = 4, Rating = "D", Mass = 4, Integrity = 48, Power = 1.76, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1, BrokenRegenRate = 2.53, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class3", new ShipModule(128064275,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 4 Rating C"){ Cost = 178900, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 2.2, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1, BrokenRegenRate = 2.53, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class4", new ShipModule(128064276,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 4 Rating B"){ Cost = 536690, Class = 4, Rating = "B", Mass = 16, Integrity = 112, Power = 2.64, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1, BrokenRegenRate = 2.53, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class5", new ShipModule(128064277,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 4 Rating A"){ Cost = 1610080, Class = 4, Rating = "A", Mass = 10, Integrity = 96, Power = 3.08, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1, BrokenRegenRate = 2.53, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },

                //30550 : { mtype:'isg', cost:    63010, namekey:30110, name:'Shield Generator', class:5, rating:'E', mass: 20.00, integ: 77, pwrdraw:1.56, boottime:1, genminmass:203.0, genoptmass: 405.0, genmaxmass:1013.0, genminmul:30, genoptmul: 80, genmaxmul:130, genrate:1.0, bgenrate:3.75, /*thmload:1.2,*/ genpwr:0.6, kinres:40.0, thmres:-20.0, expres:50.0, axeres:95.0, limit:'isg', fdid:, fdname:'Int_ShieldGenerator_Size5_Class1', eddbid:1131 },
                { "int_shieldgenerator_size5_class1", new ShipModule(128064278,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 5 Rating E"){ Cost = 63010, Class = 5, Rating = "E", Mass = 20, Integrity = 77, Power = 1.56, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1, BrokenRegenRate = 3.75, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class2", new ShipModule(128064279,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 5 Rating D"){ Cost = 189040, Class = 5, Rating = "D", Mass = 8, Integrity = 58, Power = 2.08, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1, BrokenRegenRate = 3.75, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class3", new ShipModule(128064280,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 5 Rating C"){ Cost = 567110, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 2.6, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1, BrokenRegenRate = 3.75, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class4", new ShipModule(128064281,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 5 Rating B"){ Cost = 1701320, Class = 5, Rating = "B", Mass = 32, Integrity = 134, Power = 3.12, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1, BrokenRegenRate = 3.75, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class5", new ShipModule(128064282,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 5 Rating A"){ Cost = 5103950, Class = 5, Rating = "A", Mass = 20, Integrity = 115, Power = 3.64, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1, BrokenRegenRate = 3.75, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class1", new ShipModule(128064283,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 6 Rating E"){ Cost = 199750, Class = 6, Rating = "E", Mass = 40, Integrity = 90, Power = 1.86, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1.3, BrokenRegenRate = 5.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class2", new ShipModule(128064284,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 6 Rating D"){ Cost = 599240, Class = 6, Rating = "D", Mass = 16, Integrity = 68, Power = 2.48, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.3, BrokenRegenRate = 5.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class3", new ShipModule(128064285,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 6 Rating C"){ Cost = 1797730, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 3.1, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1.3, BrokenRegenRate = 5.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class4", new ShipModule(128064286,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 6 Rating B"){ Cost = 5393180, Class = 6, Rating = "B", Mass = 64, Integrity = 158, Power = 3.72, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1.3, BrokenRegenRate = 5.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class5", new ShipModule(128064287,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 6 Rating A"){ Cost = 16179530, Class = 6, Rating = "A", Mass = 40, Integrity = 136, Power = 4.34, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1.3, BrokenRegenRate = 5.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class1", new ShipModule(128064288,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 7 Rating E"){ Cost = 633200, Class = 7, Rating = "E", Mass = 80, Integrity = 105, Power = 2.1, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 1.8, BrokenRegenRate = 7.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class2", new ShipModule(128064289,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 7 Rating D"){ Cost = 1899600, Class = 7, Rating = "D", Mass = 32, Integrity = 79, Power = 2.8, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.8, BrokenRegenRate = 7.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class3", new ShipModule(128064290,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 7 Rating C"){ Cost = 5698790, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 3.5, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 1.8, BrokenRegenRate = 7.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class4", new ShipModule(128064291,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 7 Rating B"){ Cost = 17096370, Class = 7, Rating = "B", Mass = 128, Integrity = 183, Power = 4.2, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 1.8, BrokenRegenRate = 7.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class5", new ShipModule(128064292,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 7 Rating A"){ Cost = 51289110, Class = 7, Rating = "A", Mass = 80, Integrity = 157, Power = 4.9, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 1.8, BrokenRegenRate = 7.33, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class1", new ShipModule(128064293,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 8 Rating E"){ Cost = 2007240, Class = 8, Rating = "E", Mass = 160, Integrity = 120, Power = 2.4, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 30, OptStrength = 80, MaxStrength = 130, RegenRate = 2.4, BrokenRegenRate = 9.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class2", new ShipModule(128064294,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 8 Rating D"){ Cost = 6021720, Class = 8, Rating = "D", Mass = 64, Integrity = 90, Power = 3.2, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 2.4, BrokenRegenRate = 9.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class3", new ShipModule(128064295,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 8 Rating C"){ Cost = 18065170, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 4, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 50, OptStrength = 100, MaxStrength = 150, RegenRate = 2.4, BrokenRegenRate = 9.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class4", new ShipModule(128064296,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 8 Rating B"){ Cost = 54195500, Class = 8, Rating = "B", Mass = 256, Integrity = 210, Power = 4.8, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 60, OptStrength = 110, MaxStrength = 160, RegenRate = 2.4, BrokenRegenRate = 9.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class5", new ShipModule(128064297,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Class 8 Rating A"){ Cost = 162586490, Class = 8, Rating = "A", Mass = 160, Integrity = 180, Power = 5.6, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 70, OptStrength = 120, MaxStrength = 170, RegenRate = 2.4, BrokenRegenRate = 9.6, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },


                { "int_shieldgenerator_size1_class5_strong", new ShipModule(128671323,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 1 Rating A"){ Cost = 132200, Class = 1, Rating = "A", Mass = 2.6, Integrity = 48, Power = 2.52, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 1.2, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class5_strong", new ShipModule(128671324,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 2 Rating A"){ Cost = 240340, Class = 2, Rating = "A", Mass = 5, Integrity = 61, Power = 3.15, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 1.2, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class5_strong", new ShipModule(128671325,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 3 Rating A"){ Cost = 761870, Class = 3, Rating = "A", Mass = 10, Integrity = 77, Power = 3.78, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 1.3, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class5_strong", new ShipModule(128671326,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 4 Rating A"){ Cost = 2415120, Class = 4, Rating = "A", Mass = 20, Integrity = 96, Power = 4.62, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 1.66, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class5_strong", new ShipModule(128671327,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 5 Rating A"){ Cost = 7655930, Class = 5, Rating = "A", Mass = 40, Integrity = 115, Power = 5.46, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 2.34, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class5_strong", new ShipModule(128671328,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 6 Rating A"){ Cost = 24269300, Class = 6, Rating = "A", Mass = 80, Integrity = 136, Power = 6.51, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1, BrokenRegenRate = 3.2, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class5_strong", new ShipModule(128671329,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 7 Rating A"){ Cost = 76933670, Class = 7, Rating = "A", Mass = 160, Integrity = 157, Power = 7.35, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1.1, BrokenRegenRate = 4.25, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class5_strong", new ShipModule(128671330,ShipModule.ModuleTypes.PrismaticShieldGenerator,"Prismatic Shield Generator Class 8 Rating A"){ Cost = 243879730, Class = 8, Rating = "A", Mass = 320, Integrity = 180, Power = 8.4, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 100, OptStrength = 150, MaxStrength = 200, RegenRate = 1.4, BrokenRegenRate = 5.4, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },

                { "int_shieldgenerator_size1_class3_fast", new ShipModule(128671331,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 1 Rating C"){ Cost = 7710, Class = 1, Rating = "C", Mass = 1.3, Integrity = 40, Power = 1.2, BootTime = 1, MinMass = 13, OptMass = 25, MaxMass = 63, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.8, BrokenRegenRate = 2.4, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size2_class3_fast", new ShipModule(128671332,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 2 Rating C"){ Cost = 26710, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 1.5, BootTime = 1, MinMass = 28, OptMass = 55, MaxMass = 138, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.8, BrokenRegenRate = 2.4, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size3_class3_fast", new ShipModule(128671333,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 3 Rating C"){ Cost = 84650, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 1.8, BootTime = 1, MinMass = 83, OptMass = 165, MaxMass = 413, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.8, BrokenRegenRate = 2.8, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size4_class3_fast", new ShipModule(128671334,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 4 Rating C"){ Cost = 268350, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 2.2, BootTime = 1, MinMass = 143, OptMass = 285, MaxMass = 713, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 1.8, BrokenRegenRate = 3.8, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size5_class3_fast", new ShipModule(128671335,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 5 Rating C"){ Cost = 850660, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 2.6, BootTime = 1, MinMass = 203, OptMass = 405, MaxMass = 1013, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 2.2, BrokenRegenRate = 5.63, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size6_class3_fast", new ShipModule(128671336,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 6 Rating C"){ Cost = 2696590, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 3.1, BootTime = 1, MinMass = 270, OptMass = 540, MaxMass = 1350, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 3.2, BrokenRegenRate = 8, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size7_class3_fast", new ShipModule(128671337,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 7 Rating C"){ Cost = 8548190, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 3.5, BootTime = 1, MinMass = 530, OptMass = 1060, MaxMass = 2650, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 4.4, BrokenRegenRate = 11, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },
                { "int_shieldgenerator_size8_class3_fast", new ShipModule(128671338,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,"Bi Weave Shield Generator Class 8 Rating C"){ Cost = 27097750, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 4, BootTime = 1, MinMass = 900, OptMass = 1800, MaxMass = 4500, MinStrength = 40, OptStrength = 90, MaxStrength = 140, RegenRate = 5.8, BrokenRegenRate = 14.4, MWPerUnit = 0.6, Kinetic = 40, Thermal = -20, Explosive = 50, AXResistance = 95 } },

                // shield shutdown neutraliser

                { "hpt_antiunknownshutdown_tiny", new ShipModule(128771884,ShipModule.ModuleTypes.ShutdownFieldNeutraliser,"Shutdown Field Neutraliser"){ Cost = 63000, Class = 0, Rating = "F", Mass = 1.3, Integrity = 35, Power = 0.2, BootTime = 0, Range = 3000, Time = 1, MWPerSec = 0.25, ReloadTime = 10 } },
                { "hpt_antiunknownshutdown_tiny_v2", new ShipModule(129022663,ShipModule.ModuleTypes.ShutdownFieldNeutraliser,"Thargoid Pulse Neutraliser"){ Cost = 0, Class = 0, Rating = "E", Mass = 3, Integrity = 70, Power = 0.4, BootTime = 0, Range = 0, Time = 2, MWPerSec = 0.33, ReloadTime = 10 } },

                // weapon stabliser
                { "int_expmodulestabiliser_size3_class3", new ShipModule(129019260,ShipModule.ModuleTypes.ExperimentalWeaponStabiliser,"Experimental Weapon Stabiliser Class 3 Rating F"){ Cost = 2000000, Class = 3, Rating = "F", Mass = 8, Power = 0 } },
                { "int_expmodulestabiliser_size5_class3", new ShipModule(129019261,ShipModule.ModuleTypes.ExperimentalWeaponStabiliser,"Experimental Weapon Stabiliser Class 5 Rating F"){ Cost = 4000000, Class = 5, Rating = "F", Mass = 20, Power = 0 } },

                // supercruise
                { "int_supercruiseassist", new ShipModule(128932273,ShipModule.ModuleTypes.SupercruiseAssist,"Supercruise Assist"){ Cost = 9120, Class = 1, Rating = "E", Integrity = 10, Power = 0.3, BootTime = 3 } },

                // stellar scanners

                { "int_stellarbodydiscoveryscanner_standard_free", new ShipModule(128666642, ShipModule.ModuleTypes.DiscoveryScanner, "Stellar Body Discovery Scanner Standard") {Mass=2, Range= 500 } },
                { "int_stellarbodydiscoveryscanner_standard", new ShipModule(128662535, ShipModule.ModuleTypes.DiscoveryScanner, "Stellar Body Discovery Scanner Standard") {Mass=2, Range= 500 } },
                { "int_stellarbodydiscoveryscanner_intermediate", new ShipModule(128663560, ShipModule.ModuleTypes.DiscoveryScanner, "Stellar Body Discovery Scanner Intermediate") {Mass=2, Range= 100 } },
                { "int_stellarbodydiscoveryscanner_advanced", new ShipModule(128663561, ShipModule.ModuleTypes.DiscoveryScanner,  "Stellar Body Discovery Scanner Advanced") {Mass=2 } },

                // thrusters

                { "int_engine_size2_class1_free", new ShipModule(128666636,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, Power = 2, BootTime = 0, MinMass = 24, OptMass = 48, MaxMass = 72, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size2_class1", new ShipModule(128064068,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating E"){ Cost = 1980, Class = 2, Rating = "E", Mass = 2.5, Integrity = 46, Power = 2, BootTime = 0, MinMass = 24, OptMass = 48, MaxMass = 72, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size2_class2", new ShipModule(128064069,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating D"){ Cost = 5930, Class = 2, Rating = "D", Mass = 1, Integrity = 41, Power = 2.25, BootTime = 0, MinMass = 27, OptMass = 54, MaxMass = 81, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size2_class3", new ShipModule(128064070,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating C"){ Cost = 17800, Class = 2, Rating = "C", Mass = 2.5, Integrity = 51, Power = 2.5, BootTime = 0, MinMass = 30, OptMass = 60, MaxMass = 90, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size2_class4", new ShipModule(128064071,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating B"){ Cost = 53410, Class = 2, Rating = "B", Mass = 4, Integrity = 61, Power = 2.75, BootTime = 0, MinMass = 33, OptMass = 66, MaxMass = 99, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size2_class5", new ShipModule(128064072,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 2 Rating A"){ Cost = 160220, Class = 2, Rating = "A", Mass = 2.5, Integrity = 56, Power = 3, BootTime = 0, MinMass = 36, OptMass = 72, MaxMass = 108, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size3_class1", new ShipModule(128064073,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 3 Rating E"){ Cost = 6270, Class = 3, Rating = "E", Mass = 5, Integrity = 58, Power = 2.48, BootTime = 0, MinMass = 40, OptMass = 80, MaxMass = 120, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size3_class2", new ShipModule(128064074,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 3 Rating D"){ Cost = 18810, Class = 3, Rating = "D", Mass = 2, Integrity = 51, Power = 2.79, BootTime = 0, MinMass = 45, OptMass = 90, MaxMass = 135, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size3_class3", new ShipModule(128064075,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 3 Rating C"){ Cost = 56440, Class = 3, Rating = "C", Mass = 5, Integrity = 64, Power = 3.1, BootTime = 0, MinMass = 50, OptMass = 100, MaxMass = 150, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size3_class4", new ShipModule(128064076,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 3 Rating B"){ Cost = 169300, Class = 3, Rating = "B", Mass = 8, Integrity = 77, Power = 3.41, BootTime = 0, MinMass = 55, OptMass = 110, MaxMass = 165, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size3_class5", new ShipModule(128064077,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 3 Rating A"){ Cost = 507910, Class = 3, Rating = "A", Mass = 5, Integrity = 70, Power = 3.72, BootTime = 0, MinMass = 60, OptMass = 120, MaxMass = 180, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size4_class1", new ShipModule(128064078,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 4 Rating E"){ Cost = 19880, Class = 4, Rating = "E", Mass = 10, Integrity = 72, Power = 3.28, BootTime = 0, MinMass = 140, OptMass = 280, MaxMass = 420, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size4_class2", new ShipModule(128064079,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 4 Rating D"){ Cost = 59630, Class = 4, Rating = "D", Mass = 4, Integrity = 64, Power = 3.69, BootTime = 0, MinMass = 158, OptMass = 315, MaxMass = 473, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size4_class3", new ShipModule(128064080,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 4 Rating C"){ Cost = 178900, Class = 4, Rating = "C", Mass = 10, Integrity = 80, Power = 4.1, BootTime = 0, MinMass = 175, OptMass = 350, MaxMass = 525, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size4_class4", new ShipModule(128064081,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 4 Rating B"){ Cost = 536690, Class = 4, Rating = "B", Mass = 16, Integrity = 96, Power = 4.51, BootTime = 0, MinMass = 193, OptMass = 385, MaxMass = 578, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size4_class5", new ShipModule(128064082,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 4 Rating A"){ Cost = 1610080, Class = 4, Rating = "A", Mass = 10, Integrity = 88, Power = 4.92, BootTime = 0, MinMass = 210, OptMass = 420, MaxMass = 630, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size5_class1", new ShipModule(128064083,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 5 Rating E"){ Cost = 63010, Class = 5, Rating = "E", Mass = 20, Integrity = 86, Power = 4.08, BootTime = 0, MinMass = 280, OptMass = 560, MaxMass = 840, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size5_class2", new ShipModule(128064084,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 5 Rating D"){ Cost = 189040, Class = 5, Rating = "D", Mass = 8, Integrity = 77, Power = 4.59, BootTime = 0, MinMass = 315, OptMass = 630, MaxMass = 945, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size5_class3", new ShipModule(128064085,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 5 Rating C"){ Cost = 567110, Class = 5, Rating = "C", Mass = 20, Integrity = 96, Power = 5.1, BootTime = 0, MinMass = 350, OptMass = 700, MaxMass = 1050, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size5_class4", new ShipModule(128064086,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 5 Rating B"){ Cost = 1701320, Class = 5, Rating = "B", Mass = 32, Integrity = 115, Power = 5.61, BootTime = 0, MinMass = 385, OptMass = 770, MaxMass = 1155, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size5_class5", new ShipModule(128064087,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 5 Rating A"){ Cost = 5103950, Class = 5, Rating = "A", Mass = 20, Integrity = 106, Power = 6.12, BootTime = 0, MinMass = 420, OptMass = 840, MaxMass = 1260, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size6_class1", new ShipModule(128064088,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 6 Rating E"){ Cost = 199750, Class = 6, Rating = "E", Mass = 40, Integrity = 102, Power = 5.04, BootTime = 0, MinMass = 480, OptMass = 960, MaxMass = 1440, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size6_class2", new ShipModule(128064089,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 6 Rating D"){ Cost = 599240, Class = 6, Rating = "D", Mass = 16, Integrity = 90, Power = 5.67, BootTime = 0, MinMass = 540, OptMass = 1080, MaxMass = 1620, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size6_class3", new ShipModule(128064090,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 6 Rating C"){ Cost = 1797730, Class = 6, Rating = "C", Mass = 40, Integrity = 113, Power = 6.3, BootTime = 0, MinMass = 600, OptMass = 1200, MaxMass = 1800, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size6_class4", new ShipModule(128064091,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 6 Rating B"){ Cost = 5393180, Class = 6, Rating = "B", Mass = 64, Integrity = 136, Power = 6.93, BootTime = 0, MinMass = 660, OptMass = 1320, MaxMass = 1980, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size6_class5", new ShipModule(128064092,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 6 Rating A"){ Cost = 16179530, Class = 6, Rating = "A", Mass = 40, Integrity = 124, Power = 7.56, BootTime = 0, MinMass = 720, OptMass = 1440, MaxMass = 2160, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size7_class1", new ShipModule(128064093,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 7 Rating E"){ Cost = 633200, Class = 7, Rating = "E", Mass = 80, Integrity = 118, Power = 6.08, BootTime = 0, MinMass = 720, OptMass = 1440, MaxMass = 2160, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size7_class2", new ShipModule(128064094,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 7 Rating D"){ Cost = 1899600, Class = 7, Rating = "D", Mass = 32, Integrity = 105, Power = 6.84, BootTime = 0, MinMass = 810, OptMass = 1620, MaxMass = 2430, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size7_class3", new ShipModule(128064095,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 7 Rating C"){ Cost = 5698790, Class = 7, Rating = "C", Mass = 80, Integrity = 131, Power = 7.6, BootTime = 0, MinMass = 900, OptMass = 1800, MaxMass = 2700, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size7_class4", new ShipModule(128064096,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 7 Rating B"){ Cost = 17096370, Class = 7, Rating = "B", Mass = 128, Integrity = 157, Power = 8.36, BootTime = 0, MinMass = 990, OptMass = 1980, MaxMass = 2970, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size7_class5", new ShipModule(128064097,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 7 Rating A"){ Cost = 51289110, Class = 7, Rating = "A", Mass = 80, Integrity = 144, Power = 9.12, BootTime = 0, MinMass = 1080, OptMass = 2160, MaxMass = 3240, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },
                { "int_engine_size8_class1", new ShipModule(128064098,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 8 Rating E"){ Cost = 2007240, Class = 8, Rating = "E", Mass = 160, Integrity = 135, Power = 7.2, BootTime = 0, MinMass = 1120, OptMass = 2240, MaxMass = 3360, EngineMinMultiplier = 83, EngineOptMultiplier = 100, EngineMaxMultiplier = 103, ThermL = 1.3 } },
                { "int_engine_size8_class2", new ShipModule(128064099,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 8 Rating D"){ Cost = 6021720, Class = 8, Rating = "D", Mass = 64, Integrity = 120, Power = 8.1, BootTime = 0, MinMass = 1260, OptMass = 2520, MaxMass = 3780, EngineMinMultiplier = 86, EngineOptMultiplier = 100, EngineMaxMultiplier = 106, ThermL = 1.3 } },
                { "int_engine_size8_class3", new ShipModule(128064100,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 8 Rating C"){ Cost = 18065170, Class = 8, Rating = "C", Mass = 160, Integrity = 150, Power = 9, BootTime = 0, MinMass = 1400, OptMass = 2800, MaxMass = 4200, EngineMinMultiplier = 90, EngineOptMultiplier = 100, EngineMaxMultiplier = 110, ThermL = 1.3 } },
                { "int_engine_size8_class4", new ShipModule(128064101,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 8 Rating B"){ Cost = 54195500, Class = 8, Rating = "B", Mass = 256, Integrity = 180, Power = 9.9, BootTime = 0, MinMass = 1540, OptMass = 3080, MaxMass = 4620, EngineMinMultiplier = 93, EngineOptMultiplier = 100, EngineMaxMultiplier = 113, ThermL = 1.3 } },
                { "int_engine_size8_class5", new ShipModule(128064102,ShipModule.ModuleTypes.Thrusters,"Thrusters Class 8 Rating A"){ Cost = 162586490, Class = 8, Rating = "A", Mass = 160, Integrity = 165, Power = 10.8, BootTime = 0, MinMass = 1680, OptMass = 3360, MaxMass = 5040, EngineMinMultiplier = 96, EngineOptMultiplier = 100, EngineMaxMultiplier = 116, ThermL = 1.3 } },


                { "int_engine_size3_class5_fast", new ShipModule(128682013,ShipModule.ModuleTypes.EnhancedPerformanceThrusters,"Enhanced Performance Thrusters Class 3 Rating A"){ Cost = 5103950, Class = 3, Rating = "A", Mass = 5, Integrity = 55, Power = 5, BootTime = 0, MinMass = 70, OptMass = 90, MaxMass = 200, EngineMinMultiplier = 90, EngineOptMultiplier = 115, EngineMaxMultiplier = 137, ThermL = 1.3, MinimumSpeedModifier = 90, OptimalSpeedModifier = 125, MaximumSpeedModifier = 160, MinimumAccelerationModifier = 90, OptimalAccelerationModifier = 110, MaximumAccelerationModifier = 120, MinimumRotationModifier = 90, OptimumRotationModifier = 110, MaximumRotationModifier = 130 } },
                { "int_engine_size2_class5_fast", new ShipModule(128682014,ShipModule.ModuleTypes.EnhancedPerformanceThrusters,"Enhanced Performance Thrusters Class 2 Rating A"){ Cost = 1610080, Class = 2, Rating = "A", Mass = 2.5, Integrity = 40, Power = 4, BootTime = 0, MinMass = 50, OptMass = 60, MaxMass = 120, EngineMinMultiplier = 90, EngineOptMultiplier = 115, EngineMaxMultiplier = 137, ThermL = 2, MinimumSpeedModifier = 90, OptimalSpeedModifier = 125, MaximumSpeedModifier = 160, MinimumAccelerationModifier = 90, OptimalAccelerationModifier = 110, MaximumAccelerationModifier = 120, MinimumRotationModifier = 90, OptimumRotationModifier = 110, MaximumRotationModifier = 130 } },

                // XENO Scanners

                { "hpt_xenoscanner_basic_tiny", new ShipModule(128793115,ShipModule.ModuleTypes.XenoScanner,"Xeno Scanner"){ Cost = 365700, Class = 0, Rating = "E", Mass = 1.3, Integrity = 56, Power = 0.2, BootTime = 2, Range = 500, Angle = 23, Time = 10 } },
                { "hpt_xenoscannermk2_basic_tiny", new ShipModule(128808878,ShipModule.ModuleTypes.EnhancedXenoScanner,"Enhanced Xeno Scanner"){ Cost = 745950, Class = 0, Rating = "C", Mass = 1.3, Integrity = 56, Power = 0.8, BootTime = 2, Range = 2000, Angle = 23, Time = 10 } },
                { "hpt_xenoscanner_advanced_tiny", new ShipModule(129022952,ShipModule.ModuleTypes.EnhancedXenoScanner,"Pulse Wave Xeno Scanner"){ Cost = 850000, Class = 0, Rating = "C", Mass = 3, Integrity = 100, Power = 1, BootTime = 2, Range = 1000, Angle = 23, Time = 10 } },
            };

            othershipmodules = new Dictionary<string, ShipModule>
            {
                { "adder_cockpit", new ShipModule(999999913,ShipModule.ModuleTypes.CockpitType,"Adder Cockpit" ) },
                { "typex_3_cockpit", new ShipModule(999999945,ShipModule.ModuleTypes.CockpitType,"Alliance Challenger Cockpit" ) },
                { "typex_cockpit", new ShipModule(999999943,ShipModule.ModuleTypes.CockpitType,"Alliance Chieftain Cockpit" ) },
                { "anaconda_cockpit", new ShipModule(999999926,ShipModule.ModuleTypes.CockpitType,"Anaconda Cockpit" ) },
                { "asp_cockpit", new ShipModule(999999918,ShipModule.ModuleTypes.CockpitType,"Asp Cockpit" ) },
                { "asp_scout_cockpit", new ShipModule(999999934,ShipModule.ModuleTypes.CockpitType,"Asp Scout Cockpit" ) },
                { "belugaliner_cockpit", new ShipModule(999999938,ShipModule.ModuleTypes.CockpitType,"Beluga Cockpit" ) },
                { "cobramkiii_cockpit", new ShipModule(999999915,ShipModule.ModuleTypes.CockpitType,"Cobra Mk III Cockpit" ) },
                { "cobramkiv_cockpit", new ShipModule(999999937,ShipModule.ModuleTypes.CockpitType,"Cobra Mk IV Cockpit" ) },
                { "cutter_cockpit", new ShipModule(999999932,ShipModule.ModuleTypes.CockpitType,"Cutter Cockpit" ) },
                { "diamondbackxl_cockpit", new ShipModule(999999928,ShipModule.ModuleTypes.CockpitType,"Diamondback Explorer Cockpit" ) },
                { "diamondback_cockpit", new ShipModule(999999927,ShipModule.ModuleTypes.CockpitType,"Diamondback Scout Cockpit" ) },
                { "dolphin_cockpit", new ShipModule(999999939,ShipModule.ModuleTypes.CockpitType,"Dolphin Cockpit" ) },
                { "eagle_cockpit", new ShipModule(999999911,ShipModule.ModuleTypes.CockpitType,"Eagle Cockpit" ) },
                { "empire_courier_cockpit", new ShipModule(999999909,ShipModule.ModuleTypes.CockpitType,"Empire Courier Cockpit" ) },
                { "empire_eagle_cockpit", new ShipModule(999999929,ShipModule.ModuleTypes.CockpitType,"Empire Eagle Cockpit" ) },
                { "empire_fighter_cockpit", new ShipModule(899990000,ShipModule.ModuleTypes.CockpitType,"Empire Fighter Cockpit" ) },
                { "empire_trader_cockpit", new ShipModule(999999920,ShipModule.ModuleTypes.CockpitType,"Empire Trader Cockpit" ) },
                { "federation_corvette_cockpit", new ShipModule(999999933,ShipModule.ModuleTypes.CockpitType,"Federal Corvette Cockpit" ) },
                { "federation_dropship_mkii_cockpit", new ShipModule(999999930,ShipModule.ModuleTypes.CockpitType,"Federal Dropship Cockpit" ) },
                { "federation_dropship_cockpit", new ShipModule(999999921,ShipModule.ModuleTypes.CockpitType,"Federal Gunship Cockpit" ) },
                { "federation_gunship_cockpit", new ShipModule(999999931,ShipModule.ModuleTypes.CockpitType,"Federal Gunship Cockpit" ) },
                { "federation_fighter_cockpit", new ShipModule(899990001,ShipModule.ModuleTypes.CockpitType,"Federation Fighter Cockpit" ) },
                { "ferdelance_cockpit", new ShipModule(999999925,ShipModule.ModuleTypes.CockpitType,"Fer De Lance Cockpit" ) },
                { "hauler_cockpit", new ShipModule(999999912,ShipModule.ModuleTypes.CockpitType,"Hauler Cockpit" ) },
                { "independant_trader_cockpit", new ShipModule(999999936,ShipModule.ModuleTypes.CockpitType,"Independant Trader Cockpit" ) },
                { "independent_fighter_cockpit", new ShipModule(899990002,ShipModule.ModuleTypes.CockpitType,"Independent Fighter Cockpit" ) },
                { "krait_light_cockpit", new ShipModule(999999948,ShipModule.ModuleTypes.CockpitType,"Krait Light Cockpit" ) },
                { "krait_mkii_cockpit", new ShipModule(999999946,ShipModule.ModuleTypes.CockpitType,"Krait MkII Cockpit" ) },
                { "mamba_cockpit", new ShipModule(999999949,ShipModule.ModuleTypes.CockpitType,"Mamba Cockpit" ) },
                { "orca_cockpit", new ShipModule(999999922,ShipModule.ModuleTypes.CockpitType,"Orca Cockpit" ) },
                { "python_cockpit", new ShipModule(999999924,ShipModule.ModuleTypes.CockpitType,"Python Cockpit" ) },
                { "python_nx_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,"Python Nx Cockpit" ) },
                { "sidewinder_cockpit", new ShipModule(999999910,ShipModule.ModuleTypes.CockpitType,"Sidewinder Cockpit" ) },
                { "type6_cockpit", new ShipModule(999999916,ShipModule.ModuleTypes.CockpitType,"Type 6 Cockpit" ) },
                { "type7_cockpit", new ShipModule(999999917,ShipModule.ModuleTypes.CockpitType,"Type 7 Cockpit" ) },
                { "type9_cockpit", new ShipModule(999999923,ShipModule.ModuleTypes.CockpitType,"Type 9 Cockpit" ) },
                { "type9_military_cockpit", new ShipModule(999999942,ShipModule.ModuleTypes.CockpitType,"Type 9 Military Cockpit" ) },
                { "typex_2_cockpit", new ShipModule(999999950,ShipModule.ModuleTypes.CockpitType,"Typex 2 Cockpit" ) },
                { "viper_cockpit", new ShipModule(999999914,ShipModule.ModuleTypes.CockpitType,"Viper Cockpit" ) },
                { "viper_mkiv_cockpit", new ShipModule(999999935,ShipModule.ModuleTypes.CockpitType,"Viper Mk IV Cockpit" ) },
                { "vulture_cockpit", new ShipModule(999999919,ShipModule.ModuleTypes.CockpitType,"Vulture Cockpit" ) },

                { "int_codexscanner", new ShipModule(999999947,ShipModule.ModuleTypes.Codex,"Codex Scanner" ) },
                { "hpt_shipdatalinkscanner", new ShipModule(999999940,ShipModule.ModuleTypes.DataLinkScanner,"Hpt Shipdatalinkscanner" ) },

                { "int_passengercabin_size2_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,"Prison Cell") { Mass=2.5, Prisoners = 2 } },
                { "int_passengercabin_size3_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,"Prison Cell")  { Mass=5, Prisoners = 4 } },
                { "int_passengercabin_size4_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,"Prison Cell") { Mass=1, Prisoners = 8 } },
                { "int_passengercabin_size5_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,"Prison Cell") { Mass=1, Prisoners = 16 } },
                { "int_passengercabin_size6_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,"Prison Cell") { Mass=1, Prisoners = 32 } },

                { "hpt_cannon_turret_huge", new ShipModule(-1,ShipModule.ModuleTypes.Cannon,"Cannon Turret Huge" ) },       // withdrawn we think

                { "modularcargobaydoorfdl", new ShipModule(999999907,ShipModule.ModuleTypes.CargoBayDoorType,"FDL Cargo Bay Door" ) },
                { "modularcargobaydoor", new ShipModule(999999908,ShipModule.ModuleTypes.CargoBayDoorType,"Modular Cargo Bay Door" ) },

                { "hpt_cargoscanner_basic_tiny", new ShipModule(-1,ShipModule.ModuleTypes.CargoScanner,"Manifest Scanner Basic" ) },

               // { "int_corrosionproofcargorack_size2_class1", new ShipModule(-1,null,"Anti Corrosion Cargo Rack",ShipModule.ModuleTypes.CargoRack) },
               // { "hpt_plasmaburstcannon_fixed_medium", new ShipModule(-1,1,1.4,null,"Plasma Burst Cannon Fixed Medium","Plasma Accelerator") },      // no evidence
               // { "hpt_pulselaserstealth_fixed_small", new ShipModule(-1,1,0.2,null,"Pulse Laser Stealth Fixed Small",ShipModule.ModuleTypes.PulseLaser) },
                ///{ "int_shieldgenerator_size1_class4", new ShipModule(-1,2,1.44,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },

            };

            fightermodules = new Dictionary<string, ShipModule>
            {
                { "hpt_guardiangauss_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,"Guardian Gauss Fixed GDN Fighter")  },
                { "hpt_guardianplasma_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,"Guardian Plasma Fixed GDN Fighter") },
                { "hpt_guardianshard_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,"Guardian Shard Fixed GDN Fighter") },

                { "empire_fighter_armour_standard", new ShipModule(899990059,ShipModule.ModuleTypes.LightweightAlloy,"Empire Fighter Armour Standard") },
                { "federation_fighter_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,"Federation Fighter Armour Standard") },
                { "independent_fighter_armour_standard", new ShipModule(899990070,ShipModule.ModuleTypes.LightweightAlloy,"Independent Fighter Armour Standard") },
                { "gdn_hybrid_fighter_v1_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,"GDN Hybrid Fighter V 1 Armour Standard") },
                { "gdn_hybrid_fighter_v2_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,"GDN Hybrid Fighter V 2 Armour Standard") },
                { "gdn_hybrid_fighter_v3_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,"GDN Hybrid Fighter V 3 Armour Standard") },

                { "hpt_beamlaser_fixed_empire_fighter", new ShipModule(899990018,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Empire Fighter") },
                { "hpt_beamlaser_fixed_fed_fighter", new ShipModule(899990019,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Federation Fighter") },
                { "hpt_beamlaser_fixed_indie_fighter", new ShipModule(899990020,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Fixed Indie Fighter") },
                { "hpt_beamlaser_gimbal_empire_fighter", new ShipModule(899990023,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Empire Fighter") },
                { "hpt_beamlaser_gimbal_fed_fighter", new ShipModule(899990024,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Federation Fighter") },
                { "hpt_beamlaser_gimbal_indie_fighter", new ShipModule(899990025,ShipModule.ModuleTypes.BeamLaser,"Beam Laser Gimbal Indie Fighter") },
                { "hpt_plasmarepeater_fixed_empire_fighter", new ShipModule(899990026,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Repeater Fixed Empire Fighter") },
                { "hpt_plasmarepeater_fixed_fed_fighter", new ShipModule(899990027,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Repeater Fixed Fed Fighter") },
                { "hpt_plasmarepeater_fixed_indie_fighter", new ShipModule(899990028,ShipModule.ModuleTypes.PlasmaAccelerator,"Plasma Repeater Fixed Indie Fighter") },
                { "hpt_pulselaser_fixed_empire_fighter", new ShipModule(899990029,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Empire Fighter") },
                { "hpt_pulselaser_fixed_fed_fighter", new ShipModule(899990030,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Federation Fighter") },
                { "hpt_pulselaser_fixed_indie_fighter", new ShipModule(899990031,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Fixed Indie Fighter") },
                { "hpt_pulselaser_gimbal_empire_fighter", new ShipModule(899990032,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Empire Fighter") },
                { "hpt_pulselaser_gimbal_fed_fighter", new ShipModule(899990033,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Federation Fighter") },
                { "hpt_pulselaser_gimbal_indie_fighter", new ShipModule(899990034,ShipModule.ModuleTypes.PulseLaser,"Pulse Laser Gimbal Indie Fighter") },

                { "int_engine_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.Thrusters,"Fighter Engine Class 1") },

                { "gdn_hybrid_fighter_v1_cockpit", new ShipModule(899990101,ShipModule.ModuleTypes.CockpitType,"GDN Hybrid Fighter V 1 Cockpit") },
                { "gdn_hybrid_fighter_v2_cockpit", new ShipModule(899990102,ShipModule.ModuleTypes.CockpitType,"GDN Hybrid Fighter V 2 Cockpit") },
                { "gdn_hybrid_fighter_v3_cockpit", new ShipModule(899990103,ShipModule.ModuleTypes.CockpitType,"GDN Hybrid Fighter V 3 Cockpit") },

                { "hpt_atmulticannon_fixed_indie_fighter", new ShipModule(899990040,ShipModule.ModuleTypes.AXMulti_Cannon,"AX Multicannon Fixed Indie Fighter") },
                { "hpt_multicannon_fixed_empire_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.Multi_Cannon,"Multicannon Fixed Empire Fighter") },
                { "hpt_multicannon_fixed_fed_fighter", new ShipModule(899990051,ShipModule.ModuleTypes.Multi_Cannon,"Multicannon Fixed Fed Fighter") },
                { "hpt_multicannon_fixed_indie_fighter", new ShipModule(899990052,ShipModule.ModuleTypes.Multi_Cannon,"Multicannon Fixed Indie Fighter") },

                { "int_powerdistributor_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,"Int Powerdistributor Fighter Class 1") },

                { "int_powerplant_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,"Int Powerplant Fighter Class 1") },

                { "int_sensors_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,"Int Sensors Fighter Class 1") },
                { "int_shieldgenerator_fighter_class1", new ShipModule(899990080,ShipModule.ModuleTypes.ShieldGenerator,"Shield Generator Fighter Class 1") },
                { "ext_emitter_guardian", new ShipModule(899990190,ShipModule.ModuleTypes.Sensors,"Ext Emitter Guardian") },
                { "ext_emitter_standard", new ShipModule(899990090,ShipModule.ModuleTypes.Sensors,"Ext Emitter Standard") },
            };

            srvmodules = new Dictionary<string, ShipModule>
            {
                { "buggycargobaydoor", new ShipModule(-1,ShipModule.ModuleTypes.CargoBayDoorType,"SRV Cargo Bay Door") },
                { "int_fueltank_size0_class3", new ShipModule(-1,ShipModule.ModuleTypes.FuelTank,"SRV Scarab Fuel Tank") },
                { "vehicle_scorpion_missilerack_lockon", new ShipModule(-1,ShipModule.ModuleTypes.MissileRack,"SRV Scorpion Missile Rack") },
                { "int_powerdistributor_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,"SRV Scarab Power Distributor") },
                { "int_powerplant_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,"SRV Scarab Powerplant") },
                { "vehicle_plasmaminigun_turretgun", new ShipModule(-1,ShipModule.ModuleTypes.PulseLaser,"SRV Scorpion Plasma Turret Gun") },

                { "testbuggy_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,"SRV Scarab Cockpit") },
                { "scarab_armour_grade1", new ShipModule(-1,ShipModule.ModuleTypes.LightweightAlloy,"SRV Scarab Armour") },
                { "int_fueltank_size0_class2", new ShipModule(-1,ShipModule.ModuleTypes.FuelTank,"SRV Scopion Fuel tank Size 0 Class 2") },
                { "combat_multicrew_srv_01_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,"SRV Scorpion Cockpit") },
                { "int_powerdistributor_size0_class1_cms", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,"SRV Scorpion Power Distributor Size 0 Class 1 Cms") },
                { "int_powerplant_size0_class1_cms", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,"SRV Scorpion Powerplant Size 0 Class 1 Cms") },
                { "vehicle_turretgun", new ShipModule(-1,ShipModule.ModuleTypes.PulseLaser,"SRV Scarab Turret") },

                { "hpt_datalinkscanner", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,"SRV Data Link Scanner") },
                { "int_sinewavescanner_size1_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,"SRV Scarab Scanner") },
                { "int_sensors_surface_size1_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,"SRV Sensors") },

                { "int_lifesupport_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.LifeSupport,"SRV Life Support") },
                { "int_shieldgenerator_size0_class3", new ShipModule(-1,ShipModule.ModuleTypes.ShieldGenerator,"SRV Shields") },
            };

            vanitymodules = new Dictionary<string, ShipModule>   // DO NOT USE DIRECTLY - public is for checking only
            {
                { "null", new ShipModule(-1,ShipModule.ModuleTypes.UnknownType,"Error in frontier journal - Null module") },

                { "typex_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Alliance Chieftain Shipkit 1 Bumper 3") },
                { "typex_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Alliance Chieftain Shipkit 1 Spoiler 3") },
                { "typex_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Alliance Chieftain Shipkit 1 Wings 1") },
                { "anaconda_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Bumper 1") },
                { "anaconda_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Bumper 2") },
                { "anaconda_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Bumper 3") },
                { "anaconda_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Bumper 4") },
                { "anaconda_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Spoiler 1") },
                { "anaconda_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Spoiler 2") },
                { "anaconda_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Spoiler 3") },
                { "anaconda_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Spoiler 4") },
                { "anaconda_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Tail 1") },
                { "anaconda_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Tail 2") },
                { "anaconda_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Tail 3") },
                { "anaconda_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Tail 4") },
                { "anaconda_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Wings 1") },
                { "anaconda_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Wings 2") },
                { "anaconda_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Wings 3") },
                { "anaconda_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 1 Wings 4") },
                { "anaconda_shipkit2raider_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Bumper 1") },
                { "anaconda_shipkit2raider_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Bumper 2") },
                { "anaconda_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Bumper 3") },
                { "anaconda_shipkit2raider_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Spoiler 1") },
                { "anaconda_shipkit2raider_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Spoiler 2") },
                { "anaconda_shipkit2raider_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Spoiler 3") },
                { "anaconda_shipkit2raider_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Tail 2") },
                { "anaconda_shipkit2raider_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Tail 3") },
                { "anaconda_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Wings 2") },
                { "anaconda_shipkit2raider_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Anaconda Shipkit 2 Raider Wings 3") },
                { "asp_industrial1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Industrial 1 Bumper 1") },
                { "asp_industrial1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Industrial 1 Spoiler 1") },
                { "asp_industrial1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Industrial 1 Wings 1") },
                { "asp_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Bumper 1") },
                { "asp_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Bumper 2") },
                { "asp_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Bumper 3") },
                { "asp_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Bumper 4") },
                { "asp_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Spoiler 1") },
                { "asp_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Spoiler 2") },
                { "asp_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Spoiler 3") },
                { "asp_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Spoiler 4") },
                { "asp_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Wings 1") },
                { "asp_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Wings 2") },
                { "asp_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Wings 3") },
                { "asp_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 1 Wings 4") },
                { "asp_shipkit2raider_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 2 Raider Bumper 2") },
                { "asp_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 2 Raider Bumper 3") },
                { "asp_shipkit2raider_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 2 Raider Tail 2") },
                { "asp_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Shipkit 2 Raider Wings 2") },
                { "asp_science1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Science 1 Spoiler 1") },
                { "asp_science1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Science 1 Wings 1") },
                { "asp_science1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Asp Science 1 Bumper 1") },
                { "bobble_ap2_textexclam", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text !") },
                { "bobble_ap2_texte", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text e") },
                { "bobble_ap2_textl", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text l") },
                { "bobble_ap2_textn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text n") },
                { "bobble_ap2_texto", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text o") },
                { "bobble_ap2_textr", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text r") },
                { "bobble_ap2_texts", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Text s") },
                { "bobble_ap2_textasterisk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Textasterisk") },
                { "bobble_ap2_textg", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Textg") },
                { "bobble_ap2_textj", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Textj") },
                { "bobble_ap2_textu", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Textu") },
                { "bobble_ap2_texty", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ap 2 Texty") },
                { "bobble_christmastree", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Christmas Tree") },
                { "bobble_davidbraben", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble David Braben") },
                { "bobble_dotd_blueskull", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Dotd Blueskull") },
                { "bobble_nav_beacon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Nav Beacon") },
                { "bobble_oldskool_anaconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Oldskool Anaconda") },
                { "bobble_oldskool_aspmkii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Oldskool Asp Mk II") },
                { "bobble_oldskool_cobramkiii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Oldskool Cobram Mk III") },
                { "bobble_oldskool_python", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Oldskool Python") },
                { "bobble_oldskool_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Oldskool Thargoid") },
                { "bobble_pilot_dave_expo_flight_suit", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Pilot Dave Expo Flight Suit") },
                { "bobble_pilotfemale", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Pilot Female") },
                { "bobble_pilotmale", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Pilot Male") },
                { "bobble_pilotmale_expo_flight_suit", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Pilot Male Expo Flight Suit") },
                { "bobble_planet_earth", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Earth") },
                { "bobble_planet_jupiter", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Jupiter") },
                { "bobble_planet_mars", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Mars") },
                { "bobble_planet_mercury", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Mercury") },
                { "bobble_planet_neptune", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Neptune") },
                { "bobble_planet_saturn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Saturn") },
                { "bobble_planet_uranus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Uranus") },
                { "bobble_planet_venus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Planet Venus") },
                { "bobble_plant_aloe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Plant Aloe") },
                { "bobble_plant_braintree", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Plant Braintree") },
                { "bobble_plant_rosequartz", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Plant Rosequartz") },
                { "bobble_pumpkin", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Pumpkin") },
                { "bobble_santa", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Santa") },
                { "bobble_ship_anaconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ship Anaconda") },
                { "bobble_ship_cobramkiii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ship Cobra Mk III") },
                { "bobble_ship_cobramkiii_ffe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ship Cobra Mk III FFE") },
                { "bobble_ship_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ship Thargoid") },
                { "bobble_ship_viper", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Ship Viper") },
                { "bobble_snowflake", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Snowflake") },
                { "bobble_snowman", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Snowman") },
                { "bobble_station_coriolis", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Station Coriolis") },
                { "bobble_station_coriolis_wire", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Station Coriolis Wire") },
                { "bobble_textexclam", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text !") },
                { "bobble_textpercent", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text %") },
                { "bobble_textquest", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text ?") },
                { "bobble_text0", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 0") },
                { "bobble_text1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 1") },
                { "bobble_text2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 2") },
                { "bobble_text3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 3") },
                { "bobble_text4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 4") },
                { "bobble_text5", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 5") },
                { "bobble_text6", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 6") },
                { "bobble_text7", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 7") },
                { "bobble_text8", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 8") },
                { "bobble_text9", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text 9") },
                { "bobble_texta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text A") },
                { "bobble_textb", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text B") },
                { "bobble_textbracket01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Bracket 1") },
                { "bobble_textbracket02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Bracket 2") },
                { "bobble_textcaret", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Caret") },
                { "bobble_textd", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text d") },
                { "bobble_textdollar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Dollar") },
                { "bobble_texte", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text e") },
                { "bobble_texte04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text E 4") },
                { "bobble_textexclam01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Exclam 1") },
                { "bobble_textf", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text f") },
                { "bobble_textg", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text G") },
                { "bobble_texth", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text H") },
                { "bobble_texthash", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Hash") },
                { "bobble_texti", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text I") },
                { "bobble_texti02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text I 2") },
                { "bobble_textm", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text m") },
                { "bobble_textn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text n") },
                { "bobble_texto02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text O 2") },
                { "bobble_texto03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text O 3") },
                { "bobble_textp", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text P") },
                { "bobble_textplus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Plus") },
                { "bobble_textr", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text r") },
                { "bobble_textt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text t") },
                { "bobble_textu", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text U") },
                { "bobble_textu01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text U 1") },
                { "bobble_textv", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text V") },
                { "bobble_textx", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text X") },
                { "bobble_texty", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Y") },
                { "bobble_textz", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Text Z") },
                { "bobble_textasterisk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Textasterisk") },
                { "bobble_texte01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Texte 1") },
                { "bobble_texti01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Texti 1") },
                { "bobble_textk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Textk") },
                { "bobble_textl", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Textl") },
                { "bobble_textminus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Textminus") },
                { "bobble_texto", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Texto") },
                { "bobble_texts", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Texts") },
                { "bobble_textunderscore", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Textunderscore") },
                { "bobble_trophy_anti_thargoid_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Anti Thargoid S") },
                { "bobble_trophy_combat", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Combat") },
                { "bobble_trophy_combat_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Combat S") },
                { "bobble_trophy_community", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Community") },
                { "bobble_trophy_exploration", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Exploration") },
                { "bobble_trophy_exploration_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Exploration B") },
                { "bobble_trophy_exploration_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Exploration S") },
                { "bobble_trophy_powerplay_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Bobble Trophy Powerplay B") },
                { "cobramkiii_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Co)bra MK III Shipkit 1 Wings 3") },
                { "cobramkiii_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra MK III Shipkit 1 Bumper 1") },
                { "cobramkiii_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra MK III Shipkit 1 Spoiler 2") },
                { "cobramkiii_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit 1 Spoiler 4") },
                { "cobramkiii_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra MK III Shipkit 1 Tail 1") },
                { "cobramkiii_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit 1 Tail 3") },
                { "cobramkiii_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit 1 Wings 1") },
                { "cobramkiii_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra MK III Shipkit 1 Wings 2") },
                { "cobramkiii_shipkitraider1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit Raider 1 Bumper 2") },
                { "cobramkiii_shipkitraider1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit Raider 1 Spoiler 3") },
                { "cobramkiii_shipkitraider1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit Raider 1 Tail 2") },
                { "cobramkiii_shipkitraider1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobra Mk III Shipkit Raider 1 Wings 1") },
                { "cobramkiii_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobramkiii Shipkit 1 Bumper 4") },
                { "cobramkiii_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cobramkiii Shipkit 1 Tail 4") },
                { "cutter_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Bumper 2") },
                { "cutter_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Bumper 3") },
                { "cutter_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Bumper 4") },
                { "cutter_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Spoiler 2") },
                { "cutter_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Spoiler 3") },
                { "cutter_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Spoiler 4") },
                { "cutter_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Wings 2") },
                { "cutter_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Wings 3") },
                { "decal_explorer_elite02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Elite 2") },
                { "decal_explorer_elite03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Elite 3") },
                { "decal_skull9", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Skull 9") },
                { "decal_skull8", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Skull 8") },
                { "decal_alien_hunter2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Alien Hunter 2") },
                { "decal_alien_hunter6", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Alien Hunter 6") },
                { "decal_alien_sympathiser_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Alien Sympathiser B") },
                { "decal_anti_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Anti Thargoid") },
                { "decal_bat2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Bat 2") },
                { "decal_beta_tester", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Beta Tester") },
                { "decal_bounty_hunter", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Bounty Hunter") },
                { "decal_bridgingthegap", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Bridgingthegap") },
                { "decal_cannon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Cannon") },
                { "decal_combat_competent", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Competent") },
                { "decal_combat_dangerous", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Dangerous") },
                { "decal_combat_deadly", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Deadly") },
                { "decal_combat_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Elite") },
                { "decal_combat_expert", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Expert") },
                { "decal_combat_master", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Master") },
                { "decal_combat_mostly_harmless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Mostly Harmless") },
                { "decal_combat_novice", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Combat Novice") },
                { "decal_community", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Community") },
                { "decal_distantworlds", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Distant Worlds") },
                { "decal_distantworlds2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Distantworlds 2") },
                { "decal_egx", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Egx") },
                { "decal_espionage", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Espionage") },
                { "decal_exploration_emisswhite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Exploration Emisswhite") },
                { "decal_explorer_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Elite") },
                { "decal_explorer_elite05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Elite 5") },
                { "decal_explorer_mostly_aimless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Mostly Aimless") },
                { "decal_explorer_pathfinder", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Pathfinder") },
                { "decal_explorer_ranger", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Ranger") },
                { "decal_explorer_scout", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Scout") },
                { "decal_explorer_starblazer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Starblazer") },
                { "decal_explorer_surveyor", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Surveyor") },
                { "decal_explorer_trailblazer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Explorer Trailblazer") },
                { "decal_expo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Expo") },
                { "decal_founders_reversed", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Founders Reversed") },
                { "decal_fuelrats", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Fuel Rats") },
                { "decal_galnet", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Galnet") },
                { "decal_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Lave Con") },
                { "decal_met_constructshipemp_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Constructshipemp Gold") },
                { "decal_met_espionage_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Espionage Gold") },
                { "decal_met_espionage_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Espionage Silver") },
                { "decal_met_exploration_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Exploration Gold") },
                { "decal_met_mining_bronze", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Mining Bronze") },
                { "decal_met_mining_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Mining Gold") },
                { "decal_met_mining_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Mining Silver") },
                { "decal_met_salvage_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Met Salvage Gold") },
                { "decal_mining", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Mining") },
                { "decal_networktesters", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Network Testers") },
                { "decal_onionhead1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Onionhead 1") },
                { "decal_onionhead2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Onionhead 2") },
                { "decal_onionhead3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Onionhead 3") },
                { "decal_passenger_e", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Passenger E") },
                { "decal_passenger_g", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Passenger G") },
                { "decal_passenger_l", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Passenger L") },
                { "decal_paxprime", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Pax Prime") },
                { "decal_pilot_fed1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Pilot Fed 1") },
                { "decal_planet2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Planet 2") },
                { "decal_playergroup_wolves_of_jonai", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Player Group Wolves Of Jonai") },
                { "decal_playergroup_ugc", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Playergroup Ugc") },
                { "decal_powerplay_hudson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Power Play Hudson") },
                { "decal_powerplay_mahon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Power Play Mahon") },
                { "decal_powerplay_utopia", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Power Play Utopia") },
                { "decal_powerplay_aislingduval", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Powerplay Aislingduval") },
                { "decal_powerplay_halsey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Powerplay Halsey") },
                { "decal_powerplay_kumocrew", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Powerplay Kumocrew") },
                { "decal_powerplay_sirius", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Powerplay Sirius") },
                { "decal_pumpkin", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Pumpkin") },
                { "decal_shark1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Shark 1") },
                { "decal_skull3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Skull 3") },
                { "decal_skull5", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Skull 5") },
                { "decal_specialeffect", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Special Effect") },
                { "decal_spider", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Spider") },
                { "decal_thegolconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Thegolconda") },
                { "decal_trade_broker", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Broker") },
                { "decal_trade_dealer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Dealer") },
                { "decal_trade_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Elite") },
                { "decal_trade_elite05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Elite 5") },
                { "decal_trade_entrepeneur", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Entrepeneur") },
                { "decal_trade_merchant", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Merchant") },
                { "decal_trade_mostly_penniless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Mostly Penniless") },
                { "decal_trade_peddler", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Peddler") },
                { "decal_trade_tycoon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Trade Tycoon") },
                { "decal_triple_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Decal Triple Elite") },
                { "diamondbackxl_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Diamond Back XL Shipkit 1 Bumper 1") },
                { "diamondbackxl_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Diamond Back XL Shipkit 1 Spoiler 2") },
                { "diamondbackxl_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Diamond Back XL Shipkit 1 Wings 2") },
                { "dolphin_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Bumper 2") },
                { "dolphin_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Bumper 3") },
                { "dolphin_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Spoiler 2") },
                { "dolphin_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Tail 4") },
                { "dolphin_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Wings 2") },
                { "dolphin_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Dolphin Shipkit 1 Wings 3") },
                { "eagle_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Eagle Shipkit 1 Bumper 2") },
                { "eagle_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Eagle Shipkit 1 Spoiler 1") },
                { "eagle_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Eagle Shipkit 1 Wings 1") },
                { "empire_courier_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Bumper 2") },
                { "empire_courier_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Bumper 3") },
                { "empire_courier_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Spoiler 2") },
                { "empire_courier_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Spoiler 3") },
                { "empire_courier_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Wings 1") },
                { "empire_courier_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Wings 2") },
                { "empire_courier_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Wings 3") },
                { "empire_trader_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Bumper 3") },
                { "empire_trader_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Spoiler 1") },
                { "empire_trader_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Spoiler 3") },
                { "empire_trader_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Spoiler 4") },
                { "empire_trader_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Tail 1") },
                { "empire_trader_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Tail 2") },
                { "empire_trader_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Tail 3") },
                { "empire_trader_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Tail 4") },
                { "empire_trader_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Trader Shipkit 1 Wings 1") },
                { "enginecustomisation_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Blue") },
                { "enginecustomisation_cyan", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Cyan") },
                { "enginecustomisation_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Green") },
                { "enginecustomisation_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Orange") },
                { "enginecustomisation_pink", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Pink") },
                { "enginecustomisation_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Purple") },
                { "enginecustomisation_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Red") },
                { "enginecustomisation_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation White") },
                { "enginecustomisation_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Engine Customisation Yellow") },
                { "federation_corvette_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Bumper 2") },
                { "federation_corvette_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Bumper 3") },
                { "federation_corvette_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Bumper 4") },
                { "federation_corvette_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Spoiler 1") },
                { "federation_corvette_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Spoiler 2") },
                { "federation_corvette_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Spoiler 3") },
                { "federation_corvette_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Spoiler 4") },
                { "federation_corvette_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Tail 1") },
                { "federation_corvette_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Tail 2") },
                { "federation_corvette_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Tail 3") },
                { "federation_corvette_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Tail 4") },
                { "federation_corvette_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Wings 3") },
                { "federation_corvette_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Wings 4") },
                { "federation_gunship_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Gunship Shipkit 1 Bumper 1") },
                { "ferdelance_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Fer De Lance Shipkit 1 Bumper 4") },
                { "ferdelance_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Fer De Lance Shipkit 1 Tail 1") },
                { "ferdelance_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Fer De Lance Shipkit 1 Wings 2") },
                { "ferdelance_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Ferdelance Shipkit 1 Bumper 1") },
                { "ferdelance_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Ferdelance Shipkit 1 Bumper 3") },
                { "ferdelance_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Ferdelance Shipkit 1 Spoiler 3") },
                { "ferdelance_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Ferdelance Shipkit 1 Tail 3") },
                { "ferdelance_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Ferdelance Shipkit 1 Wings 1") },
                { "krait_light_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Bumper 1") },
                { "krait_light_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Bumper 2") },
                { "krait_light_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Bumper 4") },
                { "krait_light_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Spoiler 1") },
                { "krait_light_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Spoiler 2") },
                { "krait_light_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Spoiler 3") },
                { "krait_light_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Spoiler 4") },
                { "krait_light_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Tail 3") },
                { "krait_light_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Tail 4") },
                { "krait_light_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Wings 1") },
                { "krait_light_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Wings 2") },
                { "krait_light_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Wings 3") },
                { "krait_light_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Wings 4") },
                { "krait_mkii_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Bumper 1") },
                { "krait_mkii_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Bumper 2") },
                { "krait_mkii_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Bumper 3") },
                { "krait_mkii_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Spoiler 1") },
                { "krait_mkii_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Spoiler 2") },
                { "krait_mkii_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Spoiler 4") },
                { "krait_mkii_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Tail 1") },
                { "krait_mkii_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Tail 2") },
                { "krait_mkii_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Tail 3") },
                { "krait_mkii_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Wings 2") },
                { "krait_mkii_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Wings 3") },
                { "krait_mkii_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Wings 4") },
                { "krait_mkii_shipkitraider1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit raider 1 Spoiler 3") },
                { "krait_mkii_shipkitraider1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit raider 1 Wings 2") },
                { "nameplate_combat01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Combat 1 White") },
                { "nameplate_combat02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Combat 2 White") },
                { "nameplate_combat03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Combat 3 Black") },
                { "nameplate_combat03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Combat 3 White") },
                { "nameplate_empire01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Empire 1 White") },
                { "nameplate_empire02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Empire 2 Black") },
                { "nameplate_empire03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Empire 3 Black") },
                { "nameplate_empire03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Empire 3 White") },
                { "nameplate_expedition01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 1 Black") },
                { "nameplate_expedition01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 1 White") },
                { "nameplate_expedition02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 2 Black") },
                { "nameplate_expedition02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 2 White") },
                { "nameplate_expedition03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 3 Black") },
                { "nameplate_expedition03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 3 White") },
                { "nameplate_explorer01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 1 Black") },
                { "nameplate_explorer01_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 1 Grey") },
                { "nameplate_explorer01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 1 White") },
                { "nameplate_explorer02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 2 Black") },
                { "nameplate_explorer02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 2 Grey") },
                { "nameplate_explorer02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 2 White") },
                { "nameplate_explorer03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 3 Black") },
                { "nameplate_explorer03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Explorer 3 White") },
                { "nameplate_hunter01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Hunter 1 White") },
                { "nameplate_passenger01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Passenger 1 Black") },
                { "nameplate_passenger01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Passenger 1 White") },
                { "nameplate_passenger02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Passenger 2 Black") },
                { "nameplate_passenger03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Passenger 3 White") },
                { "nameplate_pirate03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Pirate 3 White") },
                { "nameplate_practical01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 1 Black") },
                { "nameplate_practical01_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 1 Grey") },
                { "nameplate_practical01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 1 White") },
                { "nameplate_practical02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 2 Black") },
                { "nameplate_practical02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 2 Grey") },
                { "nameplate_practical02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 2 White") },
                { "nameplate_practical03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 3 Black") },
                { "nameplate_practical03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Practical 3 White") },
                { "nameplate_raider03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Raider 3 Black") },
                { "nameplate_shipid_doubleline_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID Double Line Black") },
                { "nameplate_shipid_doubleline_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID Double Line Grey") },
                { "nameplate_shipid_doubleline_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID Double Line White") },
                { "nameplate_shipid_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID Grey") },
                { "nameplate_shipid_singleline_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID Single Line Black") },
                { "nameplate_shipid_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship ID White") },
                { "nameplate_shipname_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Ship Name White") },
                { "nameplate_shipid_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipid Black") },
                { "nameplate_shipid_singleline_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipid Singleline Grey") },
                { "nameplate_shipid_singleline_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipid Singleline White") },
                { "nameplate_shipname_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Black") },
                { "nameplate_shipname_distressed_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Distressed Black") },
                { "nameplate_shipname_distressed_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Distressed Grey") },
                { "nameplate_shipname_distressed_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Distressed White") },
                { "nameplate_shipname_worn_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Worn Black") },
                { "nameplate_shipname_worn_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Shipname Worn White") },
                { "nameplate_skulls01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Skulls 1 White") },
                { "nameplate_skulls03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Skulls 3 Black") },
                { "nameplate_skulls03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Skulls 3 White") },
                { "nameplate_sympathiser03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Sympathiser 3 White") },
                { "nameplate_trader01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 1 Black") },
                { "nameplate_trader01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 1 White") },
                { "nameplate_trader02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 2 Black") },
                { "nameplate_trader02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 2 Grey") },
                { "nameplate_trader02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 2 White") },
                { "nameplate_trader03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 3 Black") },
                { "nameplate_trader03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Trader 3 White") },
                { "nameplate_victory02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Victory 2 White") },
                { "nameplate_victory03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Victory 3 White") },
                { "nameplate_wings01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 1 Black") },
                { "nameplate_wings01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 1 White") },
                { "nameplate_wings02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 2 Black") },
                { "nameplate_wings02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 2 White") },
                { "nameplate_wings03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 3 Black") },
                { "nameplate_wings03_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 3 Grey") },
                { "nameplate_wings03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Wings 3 White") },
                { "paintjob_adder_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Adder Black Friday 1") },
                { "paintjob_anaconda_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Blackfriday 1") },
                { "paintjob_anaconda_corrosive_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Corrosive 4") },
                { "paintjob_anaconda_eliteexpo_eliteexpo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Elite Expo Elite Expo") },
                { "paintjob_anaconda_faction1_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Faction 1 4") },
                { "paintjob_anaconda_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Gold Wireframe 1") },
                { "paintjob_anaconda_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Horus 2 3") },
                { "paintjob_anaconda_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Iridescent High Colour 2") },
                { "paintjob_anaconda_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Lrpo Azure") },
                { "paintjob_anaconda_luminous_stripe_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Luminous Stripe 3") },
                { "paintjob_anaconda_luminous_stripe_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Luminous Stripe 4") },
                { "paintjob_anaconda_luminous_stripe_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Luminous Stripe 6") },
                { "paintjob_anaconda_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Metallic 2 Chrome") },
                { "paintjob_anaconda_metallic_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Metallic Gold") },
                { "paintjob_anaconda_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Militaire Earth Red") },
                { "paintjob_anaconda_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Militaire Earth Yellow") },
                { "paintjob_anaconda_pulse2_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Pulse 2 Purple") },
                { "paintjob_anaconda_strife_strife", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Strife Strife") },
                { "paintjob_anaconda_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Tactical Blue") },
                { "paintjob_anaconda_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Blue") },
                { "paintjob_anaconda_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Green") },
                { "paintjob_anaconda_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Orange") },
                { "paintjob_anaconda_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Purple") },
                { "paintjob_anaconda_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Red") },
                { "paintjob_anaconda_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Vibrant Yellow") },
                { "paintjob_anaconda_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Wireframe 1") },
                { "paintjob_asp_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Blackfriday 1") },
                { "paintjob_asp_gamescom_gamescom", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Games Com GamesCom") },
                { "paintjob_asp_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Gold Wireframe 1") },
                { "paintjob_asp_iridescenthighcolour_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Iridescent High Colour 1") },
                { "paintjob_asp_largelogometallic_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Largelogometallic 5") },
                { "paintjob_asp_metallic_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Metallic Gold") },
                { "paintjob_asp_blackfriday2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Blackfriday 2 1") },
                { "paintjob_asp_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Salvage 3") },
                { "paintjob_asp_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Salvage 6") },
                { "paintjob_asp_scout_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Scout Black Friday 1") },
                { "paintjob_asp_squadron_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Squadron Green") },
                { "paintjob_asp_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Squadron Red") },
                { "paintjob_asp_stripe1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Stripe 1 3") },
                { "paintjob_asp_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Tactical Grey") },
                { "paintjob_asp_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Tactical White") },
                { "paintjob_asp_trespasser_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Trespasser 1") },
                { "paintjob_asp_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Vibrant Purple") },
                { "paintjob_asp_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Vibrant Red") },
                { "paintjob_asp_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Asp Wireframe 1") },
                { "paintjob_belugaliner_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Beluga Liner Metallic 2 Gold") },
                { "paintjob_cobramkiii_25thanniversary_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III 25 Thanniversary 1") },
                { "paintjob_cobramkiii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Black Friday 1") },
                { "paintjob_cobramkiii_flag_canada_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Flag Canada 1") },
                { "paintjob_cobramkiii_flag_uk_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Flag UK 1") },
                { "paintjob_cobramkiii_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Militaire Earth Red") },
                { "paintjob_cobramkiii_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Militaire Forest Green") },
                { "paintjob_cobramkiii_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Militaire Sand") },
                { "paintjob_cobramkiii_onionhead1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Onionhead 1 1") },
                { "paintjob_cobramkiii_stripe2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Stripe 2 2") },
                { "paintjob_cobramkiii_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Vibrant Yellow") },
                { "paintjob_cobramkiii_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk III Wireframe 1") },
                { "paintjob_cobramkiv_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk IV Black Friday 1") },
                { "paintjob_cobramkiv_gradient2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mk IV Gradient 2 6") },
                { "paintjob_cobramkiii_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra MKIII Corrosive 5") },
                { "paintjob_cobramkiii_default_52", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cobra Mkiii Default 52") },
                { "paintjob_cutter_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Black Friday 1") },
                { "paintjob_cutter_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Full Metal Cobalt") },
                { "paintjob_cutter_fullmetal_paladium", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Fullmetal Paladium") },
                { "paintjob_cutter_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Iridescent High Colour 2") },
                { "paintjob_cutter_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Lrpo Azure") },
                { "paintjob_cutter_luminous_stripe_ver2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Luminous Stripe Ver 2 2") },
                { "paintjob_cutter_luminous_stripe_ver2_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Luminous Stripe Ver 2 4") },
                { "paintjob_cutter_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Metallic 2 Chrome") },
                { "paintjob_cutter_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Metallic 2 Gold") },
                { "paintjob_cutter_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Metallic Chrome") },
                { "paintjob_cutter_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Militaire Forest Green") },
                { "paintjob_cutter_smartfancy_2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Smartfancy 2 6") },
                { "paintjob_cutter_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Smartfancy 4") },
                { "paintjob_cutter_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Tactical Grey") },
                { "paintjob_cutter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Vibrant Blue") },
                { "paintjob_cutter_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Vibrant Purple") },
                { "paintjob_cutter_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Cutter Vibrant Yellow") },
                { "paintjob_diamondbackxl_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Diamond Back XL Metallic 2 Chrome") },
                { "paintjob_diamondbackxl_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Diamondbackxl Lrpo Azure") },
                { "paintjob_dolphin_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Dolphin Blackfriday 1") },
                { "paintjob_dolphin_iridescentblack_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Dolphin Iridescentblack 1") },
                { "paintjob_dolphin_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Dolphin Lrpo Azure") },
                { "paintjob_dolphin_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Dolphin Metallic 2 Gold") },
                { "paintjob_eagle_crimson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Eagle Crimson") },
                { "paintjob_eagle_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Eagle Tactical Grey") },
                { "paintjob_empire_courier_aerial_display_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Aerial Display Blue") },
                { "paintjob_empire_courier_aerial_display_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Aerial Display Red") },
                { "paintjob_empire_courier_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Lrpo Azure") },
                { "paintjob_empire_courier_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Smartfancy 4") },
                { "paintjob_empire_courier_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Tactical Grey") },
                { "paintjob_empire_courier_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Courier Vibrant Yellow") },
                { "paintjob_empire_eagle_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Eagle Black Friday 1") },
                { "paintjob_empire_eagle_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Eagle Lrpo Azure") },
                { "paintjob_empiretrader_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Trader Black Friday 1") },
                { "paintjob_empire_trader_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empire Trader Lrpo Azure") },
                { "paintjob_empiretrader_smartfancy_2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Smartfancy 2 6") },
                { "paintjob_empiretrader_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Smartfancy 4") },
                { "paintjob_empiretrader_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Tactical Blue") },
                { "paintjob_empiretrader_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Tactical Grey") },
                { "paintjob_empiretrader_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Vibrant Blue") },
                { "paintjob_empiretrader_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Empiretrader Vibrant Purple") },
                { "paintjob_feddropship_mkii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fed Dropship Mk II Black Friday 1") },
                { "paintjob_feddropship_mkii_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fed Dropship Mk II Tactical Blue") },
                { "paintjob_feddropship_mkii_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fed Dropship Mk II Vibrant Purple") },
                { "paintjob_feddropship_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fed Dropship Tactical Blue") },
                { "paintjob_feddropship_mkii_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Feddropship Mkii Vibrant Yellow") },
                { "paintjob_feddropship_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Feddropship Vibrant Orange") },
                { "paintjob_federation_corvette_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Blackfriday 1") },
                { "paintjob_federation_corvette_colourgeo2_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Colour Geo 2 Blue") },
                { "paintjob_federation_corvette_colourgeo_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Colour Geo Blue") },
                { "paintjob_federation_corvette_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Iridescent High Colour 2") },
                { "paintjob_federation_corvette_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Iridescentblack 2") },
                { "paintjob_federation_corvette_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Lrpo Azure") },
                { "paintjob_federation_corvette_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Metallic 2 Chrome") },
                { "paintjob_federation_corvette_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Metallic 2 Gold") },
                { "paintjob_federation_corvette_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Metallic Chrome") },
                { "paintjob_federation_corvette_predator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Predator Red") },
                { "paintjob_federation_corvette_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Corvette Vibrant Purple") },
                { "paintjob_federation_gunship_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Gunship Metallic Chrome") },
                { "paintjob_federation_gunship_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Gunship Tactical Blue") },
                { "paintjob_federation_gunship_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Federation Gunship Tactical Grey") },
                { "paintjob_ferdelance_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fer De Lance Black Friday 1") },
                { "paintjob_ferdelance_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fer De Lance Metallic 2 Chrome") },
                { "paintjob_ferdelance_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fer De Lance Metallic 2 Gold") },
                { "paintjob_ferdelance_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Fer De Lance Wireframe 1") },
                { "paintjob_ferdelance_gradient2_crimson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ferdelance Gradient 2 Crimson") },
                { "paintjob_ferdelance_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ferdelance Vibrant Red") },
                { "paintjob_hauler_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Hauler Blackfriday 1") },
                { "paintjob_hauler_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Hauler Lrpo Azure") },
                { "paintjob_indfighter_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ind Fighter Black Friday 1") },
                { "paintjob_indfighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ind Fighter Vibrant Blue") },
                { "paintjob_indfighter_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ind Fighter Vibrant Green") },
                { "paintjob_indfighter_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Ind Fighter Vibrant Yellow") },
                { "paintjob_independant_trader_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Independant Trader Tactical White") },
                { "paintjob_independant_trader_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Independant Trader Vibrant Purple") },
                { "paintjob_indfighter_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Indfighter Vibrant Purple") },
                { "paintjob_krait_light_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Blackfriday 1") },
                { "paintjob_krait_light_gradient2_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Gradient 2 Blue") },
                { "paintjob_krait_light_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Horus 1 3") },
                { "paintjob_krait_light_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Horus 2 3") },
                { "paintjob_krait_light_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Iridescentblack 2") },
                { "paintjob_krait_light_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Lrpo Azure") },
                { "paintjob_krait_light_salvage_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Salvage 1") },
                { "paintjob_krait_light_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Salvage 3") },
                { "paintjob_krait_light_salvage_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Salvage 4") },
                { "paintjob_krait_light_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Salvage 6") },
                { "paintjob_krait_light_spring_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Spring 5") },
                { "paintjob_krait_light_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Light Tactical White") },
                { "paintjob_krait_mkii_iridescenthighcolour_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mk II Iridescent High Colour 5") },
                { "paintjob_krait_mkii_specialeffectchristmas_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mk II Special Effect Christmas 1") },
                { "paintjob_krait_mkii_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Festive Silver") },
                { "paintjob_krait_mkii_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Horus 2 1") },
                { "paintjob_krait_mkii_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Lrpo Azure") },
                { "paintjob_krait_mkii_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Militaire Forest Green") },
                { "paintjob_krait_mkii_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Salvage 3") },
                { "paintjob_krait_mkii_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Tactical Red") },
                { "paintjob_krait_mkii_trims_blackmagenta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Trims Blackmagenta") },
                { "paintjob_krait_mkii_turbulence_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mkii Turbulence 2") },
                { "paintjob_mamba_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Mamba Black Friday 1") },
                { "paintjob_orca_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Orca Black Friday 1") },
                { "paintjob_orca_corporate2_corporate2e", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Orca Corporate 2 Corporate 2 E") },
                { "paintjob_orca_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Orca Lrpo Azure") },
                { "paintjob_python_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Black Friday 1") },
                { "paintjob_python_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Corrosive 5") },
                { "paintjob_python_eliteexpo_eliteexpo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Elite Expo Elite Expo") },
                { "paintjob_python_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Gold Wireframe 1") },
                { "paintjob_python_gradient2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Gradient 2 2") },
                { "paintjob_python_gradient2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Gradient 2 6") },
                { "paintjob_python_horus1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Horus 1 1") },
                { "paintjob_python_iridescentblack_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Iridescentblack 6") },
                { "paintjob_python_luminous_stripe_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Luminous Stripe 3") },
                { "paintjob_python_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Metallic 2 Chrome") },
                { "paintjob_python_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Metallic 2 Gold") },
                { "paintjob_python_militaire_dark_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Dark Green") },
                { "paintjob_python_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Desert Sand") },
                { "paintjob_python_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Earth Red") },
                { "paintjob_python_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Earth Yellow") },
                { "paintjob_python_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Forest Green") },
                { "paintjob_python_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Militaire Sand") },
                { "paintjob_python_militarystripe_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Military Stripe Blue") },
                { "paintjob_python_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Salvage 3") },
                { "paintjob_python_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Squadron Black") },
                { "paintjob_python_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Blue") },
                { "paintjob_python_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Green") },
                { "paintjob_python_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Orange") },
                { "paintjob_python_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Purple") },
                { "paintjob_python_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Red") },
                { "paintjob_python_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Vibrant Yellow") },
                { "paintjob_python_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Python Wireframe 1") },
                { "paintjob_python_nx_venom_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Nx Venom 1") },
                { "paintjob_sidewinder_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Blackfriday 1") },
                { "paintjob_sidewinder_doublestripe_08", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Doublestripe 8") },
                { "paintjob_sidewinder_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Festive Silver") },
                { "paintjob_sidewinder_hotrod_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Hotrod 1") },
                { "paintjob_sidewinder_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Metallic Chrome") },
                { "paintjob_sidewinder_pax_east_pax_east", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Pax East") },
                { "paintjob_sidewinder_pilotreward_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Pilotreward 1") },
                { "paintjob_sidewinder_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Vibrant Blue") },
                { "paintjob_sidewinder_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Sidewinder Vibrant Orange") },
                { "paintjob_testbuggy_chase_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Chase 4") },
                { "paintjob_testbuggy_chase_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Chase 5") },
                { "paintjob_testbuggy_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Militaire Desert Sand") },
                { "paintjob_testbuggy_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Tactical Grey") },
                { "paintjob_testbuggy_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Tactical Red") },
                { "paintjob_testbuggy_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Tactical White") },
                { "paintjob_testbuggy_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Testbuggy Vibrant Purple") },
                { "paintjob_type6_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Blackfriday 1") },
                { "paintjob_type6_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Lrpo Azure") },
                { "paintjob_type6_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Militaire Sand") },
                { "paintjob_type6_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Tactical Blue") },
                { "paintjob_type6_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Vibrant Blue") },
                { "paintjob_type6_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 6 Vibrant Yellow") },
                { "paintjob_type7_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 7 Black Friday 1") },
                { "paintjob_type7_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 7 Salvage 3") },
                { "paintjob_type7_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 7 Tactical White") },
                { "paintjob_type9_mechanist_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Mechanist 4") },
                { "paintjob_type9_military_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Full Metal Cobalt") },
                { "paintjob_type9_military_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Lrpo Azure") },
                { "paintjob_type9_military_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Metallic 2 Chrome") },
                { "paintjob_type9_military_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Militaire Forest Green") },
                { "paintjob_type9_military_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Tactical Red") },
                { "paintjob_type9_military_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Military Vibrant Blue") },
                { "paintjob_type9_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Salvage 3") },
                { "paintjob_type9_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Salvage 6") },
                { "paintjob_type9_spring_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Spring 4") },
                { "paintjob_type9_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Type 9 Vibrant Orange") },
                { "paintjob_typex_2_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Typex 2 Lrpo Azure") },
                { "paintjob_typex_3_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Typex 3 Lrpo Azure") },
                { "paintjob_typex_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Typex Festive Silver") },
                { "paintjob_typex_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Typex Lrpo Azure") },
                { "paintjob_viper_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Blackfriday 1") },
                { "paintjob_viper_default_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Default 3") },
                { "paintjob_viper_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Lrpo Azure") },
                { "paintjob_viper_merc", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Merc") },
                { "paintjob_viper_mkiv_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Mk IV Black Friday 1") },
                { "paintjob_viper_mkiv_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Mkiv Lrpo Azure") },
                { "paintjob_viper_stripe1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Stripe 1 2") },
                { "paintjob_viper_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Blue") },
                { "paintjob_viper_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Green") },
                { "paintjob_viper_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Orange") },
                { "paintjob_viper_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Purple") },
                { "paintjob_viper_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Red") },
                { "paintjob_viper_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Viper Vibrant Yellow") },
                { "paintjob_vulture_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Vulture Black Friday 1") },
                { "paintjob_vulture_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Vulture Lrpo Azure") },
                { "paintjob_vulture_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Vulture Metallic Chrome") },
                { "paintjob_vulture_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Vulture Militaire Desert Sand") },
                { "paintjob_vulture_synth_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Vulture Synth Orange") },
                { "paintjob_anaconda_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Corrosive 5") },
                { "paintjob_anaconda_lavecon_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Lavecon Lavecon") },
                { "paintjob_anaconda_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Metallic 2 Gold") },
                { "paintjob_anaconda_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Squadron Black") },
                { "paintjob_anaconda_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Squadron Blue") },
                { "paintjob_anaconda_squadron_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Squadron Green") },
                { "paintjob_anaconda_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Squadron Red") },
                { "paintjob_anaconda_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Tactical Grey") },
                { "paintjob_anaconda_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Tactical Red") },
                { "paintjob_anaconda_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Tactical White") },
                { "paintjob_asp_halloween01_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Halloween 1 5") },
                { "paintjob_asp_lavecon_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Lavecon Lavecon") },
                { "paintjob_asp_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Lrpo Azure") },
                { "paintjob_asp_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Metallic 2 Gold") },
                { "paintjob_asp_operator_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Operator Green") },
                { "paintjob_asp_operator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Operator Red") },
                { "paintjob_asp_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Squadron Black") },
                { "paintjob_asp_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Squadron Blue") },
                { "paintjob_asp_stripe1_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Stripe 1 4") },
                { "paintjob_asp_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Vibrant Blue") },
                { "paintjob_asp_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Vibrant Orange") },
                { "paintjob_belugaliner_corporatefleet_fleeta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Belugaliner Corporatefleet Fleeta") },
                { "paintjob_cobramkiii_horizons_desert", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Horizons Desert") },
                { "paintjob_cobramkiii_horizons_lunar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Horizons Lunar") },
                { "paintjob_cobramkiii_horizons_polar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Horizons Polar") },
                { "paintjob_cobramkiii_stripe1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Stripe 1 3") },
                { "paintjob_cobramkiii_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra Mk III Tactical Grey") },
                { "paintjob_cobramkiii_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Tactical White") },
                { "paintjob_cobramkiii_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra Mk III Vibrant Orange") },
                { "paintjob_cobramkiii_yogscast_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobra MK III Yogscast 1") },
                { "paintjob_cobramkiii_stripe2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobramkiii Stripe 2 3") },
                { "paintjob_cutter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Tactical White") },
                { "paintjob_diamondback_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondback Tactical Blue") },
                { "paintjob_diamondback_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondback Tactical Brown") },
                { "paintjob_diamondback_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondback Tactical White") },
                { "paintjob_diamondbackxl_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Blackfriday 1") },
                { "paintjob_diamondbackxl_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Tactical Blue") },
                { "paintjob_diamondbackxl_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Tactical White") },
                { "paintjob_diamondbackxl_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Vibrant Blue") },
                { "paintjob_dolphin_corporatefleet_fleeta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Dolphin Corporatefleet Fleeta") },
                { "paintjob_dolphin_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Dolphin Vibrant Yellow") },
                { "paintjob_eagle_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Eagle Tactical Blue") },
                { "paintjob_eagle_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Eagle Tactical White") },
                { "paintjob_empire_courier_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Courier Blackfriday 1") },
                { "paintjob_empire_courier_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Courier Metallic 2 Gold") },
                { "paintjob_empire_fighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Fighter Tactical White") },
                { "paintjob_empire_fighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Fighter Vibrant Blue") },
                { "paintjob_empiretrader_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empiretrader Tactical White") },
                { "paintjob_feddropship_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Feddropship Tactical Grey") },
                { "paintjob_federation_corvette_colourgeo_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Colourgeo Red") },
                { "paintjob_federation_corvette_predator_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Predator Blue") },
                { "paintjob_federation_corvette_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Tactical White") },
                { "paintjob_federation_corvette_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Vibrant Blue") },
                { "paintjob_federation_fighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Fighter Tactical White") },
                { "paintjob_federation_fighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Fighter Vibrant Blue") },
                { "paintjob_federation_gunship_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Gunship Tactical Brown") },
                { "paintjob_federation_gunship_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Gunship Tactical White") },
                { "paintjob_ferdelance_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Ferdelance Tactical White") },
                { "paintjob_ferdelance_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Ferdelance Vibrant Blue") },
                { "paintjob_ferdelance_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Ferdelance Vibrant Yellow") },
                { "paintjob_hauler_doublestripe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Hauler Doublestripe 1") },
                { "paintjob_hauler_doublestripe_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Hauler Doublestripe 2") },
                { "paintjob_independant_trader_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Independant Trader Blackfriday 1") },
                { "paintjob_indfighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Indfighter Tactical White") },
                { "paintjob_krait_mkii_egypt_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Egypt 2") },
                { "paintjob_krait_mkii_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Vibrant Red") },
                { "paintjob_orca_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Orca Militaire Desert Sand") },
                { "paintjob_orca_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Orca Vibrant Yellow") },
                { "paintjob_python_corrosive_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Corrosive 1") },
                { "paintjob_python_corrosive_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Corrosive 6") },
                { "paintjob_python_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Horus 1 2") },
                { "paintjob_python_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Horus 2 3") },
                { "paintjob_python_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Lrpo Azure") },
                { "paintjob_python_luminous_stripe_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Luminous Stripe 2") },
                { "paintjob_python_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Tactical White") },
                { "paintjob_sidewinder_doublestripe_07", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Doublestripe 7") },
                { "paintjob_sidewinder_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Gold Wireframe 1") },
                { "paintjob_sidewinder_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Militaire Forest Green") },
                { "paintjob_sidewinder_specialeffect_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Specialeffect 1") },
                { "paintjob_sidewinder_thirds_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Thirds 6") },
                { "paintjob_sidewinder_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Sidewinder Vibrant Red") },
                { "paintjob_testbuggy_chase_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Chase 6") },
                { "paintjob_testbuggy_destination_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Destination Blue") },
                { "paintjob_testbuggy_luminous_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Luminous Blue") },
                { "paintjob_testbuggy_luminous_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Luminous Red") },
                { "paintjob_testbuggy_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Metallic 2 Gold") },
                { "paintjob_testbuggy_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Militaire Earth Red") },
                { "paintjob_testbuggy_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Militaire Earth Yellow") },
                { "paintjob_testbuggy_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Tactical Blue") },
                { "paintjob_testbuggy_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Vibrant Blue") },
                { "paintjob_testbuggy_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Vibrant Orange") },
                { "paintjob_testbuggy_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob SRV Vibrant Yellow") },
                { "paintjob_type6_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 6 Tactical White") },
                { "paintjob_type7_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 7 Vibrant Blue") },
                { "paintjob_type9_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Blackfriday 1") },
                { "paintjob_type9_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Lrpo Azure") },
                { "paintjob_type9_military_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Military Iridescent black 2") },
                { "paintjob_type9_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Vibrant Blue") },
                { "paintjob_typex_military_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex Military Tactical Grey") },
                { "paintjob_typex_military_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex Military Tactical White") },
                { "paintjob_typex_operator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex Operator Red") },
                { "paintjob_viper_flag_norway_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper Flag Norway 1") },
                { "paintjob_viper_mkiv_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Militaire Sand") },
                { "paintjob_viper_mkiv_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Squadron Black") },
                { "paintjob_viper_mkiv_squadron_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Squadron Orange") },
                { "paintjob_viper_mkiv_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Tactical Blue") },
                { "paintjob_viper_mkiv_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Tactical Brown") },
                { "paintjob_viper_mkiv_tactical_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Tactical Green") },
                { "paintjob_viper_mkiv_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Tactical Grey") },
                { "paintjob_viper_mkiv_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper MK IV Tactical White") },
                { "paintjob_vulture_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Vulture Tactical Blue") },
                { "paintjob_vulture_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Vulture Tactical White") },
                { "paintjob_diamondbackxl_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Tactical Grey") },
                { "paintjob_python_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Tactical Grey") },
                { "paintjob_krait_light_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Light Tactical Grey") },
                { "paintjob_cutter_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Militaire Earth Yellow") },
                { "paintjob_anaconda_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Fullmetal Cobalt") },

                { "nameplate_expedition02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Nameplate Expedition 2 Grey") },
                { "paintjob_adder_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Adder Lrpo Azure") },
                { "paintjob_adder_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Adder Vibrant Orange") },
                { "paintjob_anaconda_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Horus 1 2") },
                { "paintjob_anaconda_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Horus 1 3") },
                { "paintjob_anaconda_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Horus 2 1") },
                { "paintjob_anaconda_icarus_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Icarus Grey") },
                { "paintjob_anaconda_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Iridescentblack 2") },
                { "paintjob_anaconda_lowlighteffect_01_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Lowlighteffect 1 1") },
                { "paintjob_anaconda_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Anaconda Militaire Forest Green") },
                { "paintjob_anaconda_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Militaire Sand") },
                { "paintjob_anaconda_prestige_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Prestige Blue") },
                { "paintjob_anaconda_prestige_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Prestige Green") },
                { "paintjob_anaconda_prestige_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Prestige Purple") },
                { "paintjob_anaconda_prestige_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Prestige Red") },
                { "paintjob_anaconda_pulse2_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda Pulse 2 Green") },
                { "paintjob_anaconda_war_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Anaconda War Orange") },
                { "paintjob_asp_icarus_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Icarus Grey") },
                { "paintjob_asp_iridescentblack_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Asp Iridescentblack 4") },
                { "paintjob_belugaliner_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Belugaliner Blackfriday 1") },
                { "paintjob_belugaliner_ember_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Belugaliner Ember Blue") },
                { "paintjob_cobramkiv_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cobramkiv Lrpo Azure") },
                { "paintjob_cutter_gradient2_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Gradient 2 Red") },
                { "paintjob_cutter_iridescentblack_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Iridescentblack 5") },
                { "paintjob_cutter_synth_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Synth Orange") },
                { "paintjob_cutter_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Tactical Blue") },
                { "paintjob_cutter_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter Vibrant Red") },
                { "paintjob_cutter_war_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Cutter War Blue") },
                { "paintjob_diamondback_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Diamond Back Black Friday 1") },
                { "paintjob_diamondback_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondback Lrpo Azure") },
                { "paintjob_diamondbackxl_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Diamondbackxl Vibrant Orange") },
                { "paintjob_dolphin_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Dolphin Vibrant Blue") },
                { "paintjob_eagle_aerial_display_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Eagle Aerial Display Red") },
                { "paintjob_eagle_stripe1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Eagle Stripe 1 1") },
                { "paintjob_empire_courier_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Courier Iridescenthighcolour 2") },
                { "paintjob_empire_courier_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empire Courier Tactical White") },
                { "paintjob_empiretrader_slipstream_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Empiretrader Slipstream Orange") },
                { "paintjob_feddropship_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Feddropship Militaire Earth Red") },
                { "paintjob_federation_corvette_colourgeo_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Colourgeo Grey") },
                { "paintjob_federation_corvette_razormetal_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Razormetal Silver") },
                { "paintjob_federation_corvette_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Tactical Grey") },
                { "paintjob_federation_corvette_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Tactical Red") },
                { "paintjob_federation_corvette_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Corvette Vibrant Red") },
                { "paintjob_federation_gunship_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Gunship Blackfriday 1") },
                { "paintjob_federation_gunship_militarystripe_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Federation Gunship Militarystripe Red") },
                { "paintjob_ferdelance_razormetal_copper", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Ferdelance Razormetal Copper") },
                { "paintjob_ferdelance_slipstream_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Ferdelance Slipstream Orange") },
                { "paintjob_hauler_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Hauler Tactical Red") },
                { "paintjob_hauler_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Hauler Vibrant Blue") },
                { "paintjob_krait_light_lowlighteffect_01_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Light Lowlighteffect 1 6") },
                { "paintjob_krait_light_turbulence_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Light Turbulence 6") },
                { "paintjob_krait_mkii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Blackfriday 1") },
                { "paintjob_krait_mkii_egypt_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Egypt 1") },
                { "paintjob_krait_mkii_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Horus 1 2") },
                { "paintjob_krait_mkii_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Horus 1 3") },
                { "paintjob_krait_mkii_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paint Job Krait Mk II Tactical Blue") },
                { "paintjob_krait_mkii_trims_greyorange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Trims Greyorange") },
                { "paintjob_krait_mkii_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Krait Mkii Vibrant Orange") },
                { "paintjob_mamba_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Mamba Tactical White") },
                { "paintjob_orca_corporate1_corporate1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Orca Corporate 1 Corporate 1") },
                { "paintjob_orca_geometric_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Orca Geometric Blue") },
                { "paintjob_python_egypt_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Egypt 1") },
                { "paintjob_python_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Horus 2 1") },
                { "paintjob_python_lowlighteffect_01_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Lowlighteffect 1 3") },
                { "paintjob_python_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Salvage 6") },
                { "paintjob_python_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Squadron Blue") },
                { "paintjob_python_squadron_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Squadron Gold") },
                { "paintjob_python_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Squadron Red") },
                { "paintjob_python_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Python Tactical Red") },
                { "paintjob_type6_foss_orangewhite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 6 Foss Orangewhite") },
                { "paintjob_type6_foss_whitered", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 6 Foss Whitered") },
                { "paintjob_type6_iridescentblack_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 6 Iridescentblack 3") },
                { "paintjob_type7_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 7 Lrpo Azure") },
                { "paintjob_type7_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 7 Militaire Earth Yellow") },
                { "paintjob_type7_turbulence_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 7 Turbulence 6") },
                { "paintjob_type9_military_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Military Blackfriday 1") },
                { "paintjob_type9_military_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Military Vibrant Orange") },
                { "paintjob_type9_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Tactical Grey") },
                { "paintjob_type9_turbulence_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Type 9 Turbulence 3") },
                { "paintjob_typex_2_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 2 Blackfriday 1") },
                { "paintjob_typex_3_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 3 Blackfriday 1") },
                { "paintjob_typex_3_military_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 3 Military Militaire Forest Green") },
                { "paintjob_typex_3_military_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 3 Military Tactical Grey") },
                { "paintjob_typex_3_military_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 3 Military Vibrant Yellow") },
                { "paintjob_typex_3_trims_greyorange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex 3 Trims Greyorange") },
                { "paintjob_typex_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Typex Blackfriday 1") },
                { "paintjob_viper_mkiv_slipstream_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper Mkiv Slipstream Blue") },
                { "paintjob_viper_predator_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Paintjob Viper Predator Blue") },
                { "python_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Bumper 1") },
                { "python_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Bumper 2") },
                { "python_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Bumper 3") },
                { "python_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Bumper 4") },
                { "python_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Spoiler 1") },
                { "python_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Spoiler 2") },
                { "python_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Spoiler 3") },
                { "python_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Spoiler 4") },
                { "python_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Tail 1") },
                { "python_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Tail 2") },
                { "python_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Tail 3") },
                { "python_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Tail 4") },
                { "python_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Wings 1") },
                { "python_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Wings 2") },
                { "python_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Wings 3") },
                { "python_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 1 Wings 4") },
                { "python_shipkit2raider_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Bumper 1") },
                { "python_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Bumper 3") },
                { "python_shipkit2raider_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Spoiler 1") },
                { "python_shipkit2raider_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Spoiler 2") },
                { "python_shipkit2raider_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Tail 1") },
                { "python_shipkit2raider_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Tail 3") },
                { "python_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Wings 2") },
                { "python_shipkit2raider_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Shipkit 2 Raider Wings 3") },
                { "python_nx_strike_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Nx Strike Spoiler 1") },
                { "python_nx_strike_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Nx Strike Wings 1") },
                { "python_nx_strike_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Python Nx Strike Bumper 1") },
                { "sidewinder_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Bumper 1") },
                { "sidewinder_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Bumper 2") },
                { "sidewinder_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Bumper 4") },
                { "sidewinder_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Spoiler 1") },
                { "sidewinder_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Spoiler 3") },
                { "sidewinder_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Tail 1") },
                { "sidewinder_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Tail 3") },
                { "sidewinder_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Tail 4") },
                { "sidewinder_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Wings 2") },
                { "sidewinder_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Wings 3") },
                { "sidewinder_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Sidewinder Shipkit 1 Wings 4") },
                { "string_lights_coloured", new ShipModule(999999941,ShipModule.ModuleTypes.VanityType,"String Lights Coloured") },
                { "string_lights_thargoidprobe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"String Lights Thargoid probe") },
                { "string_lights_warm_white", new ShipModule(999999944,ShipModule.ModuleTypes.VanityType,"String Lights Warm White") },
                { "string_lights_skull", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"String Lights Skull") },
                { "type6_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Bumper 1") },
                { "type6_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Spoiler 3") },
                { "type6_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Wings 1") },
                { "type9_military_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 9 Military Ship Kit 1 Bumper 4") },
                { "type9_military_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 9 Military Ship Kit 1 Spoiler 3") },
                { "type9_military_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 9 Military Ship Kit 1 Wings 3") },
                { "type9_military_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 9 Military Shipkit 1 Bumper 3") },
                { "type9_military_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 9 Military Shipkit 1 Spoiler 2") },
                { "typex_3_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Typex 3 Shipkit 1 Bumper 3") },
                { "typex_3_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Typex 3 Shipkit 1 Spoiler 3") },
                { "typex_3_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Typex 3 Shipkit 1 Wings 4") },
                { "viper_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Viper Shipkit 1 Bumper 4") },
                { "viper_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Viper Shipkit 1 Spoiler 4") },
                { "viper_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Viper Shipkit 1 Tail 4") },
                { "viper_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Viper Shipkit 1 Wings 4") },
                { "voicepack_verity", new ShipModule(999999901,ShipModule.ModuleTypes.VanityType,"Voice Pack Verity") },
                { "voicepack_alix", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Alix") },
                { "voicepack_amelie", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Amelie") },
                { "voicepack_archer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Archer") },
                { "voicepack_carina", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Carina") },
                { "voicepack_celeste", new ShipModule(999999904,ShipModule.ModuleTypes.VanityType,"Voicepack Celeste") },
                { "voicepack_eden", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Eden") },
                { "voicepack_gerhard", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Gerhard") },
                { "voicepack_jefferson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Jefferson") },
                { "voicepack_leo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Leo") },
                { "voicepack_luciana", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Voicepack Luciana") },
                { "voicepack_victor", new ShipModule(999999902,ShipModule.ModuleTypes.VanityType,"Voicepack Victor") },
                { "vulture_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Vulture Shipkit 1 Bumper 1") },
                { "vulture_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Vulture Shipkit 1 Spoiler 3") },
                { "vulture_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Vulture Shipkit 1 Spoiler 4") },
                { "vulture_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Vulture Shipkit 1 Tail 1") },
                { "vulture_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Vulture Shipkit 1 Wings 2") },
                { "weaponcustomisation_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Blue") },
                { "weaponcustomisation_cyan", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Cyan") },
                { "weaponcustomisation_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Green") },
                { "weaponcustomisation_pink", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Pink") },
                { "weaponcustomisation_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Purple") },
                { "weaponcustomisation_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Red") },
                { "weaponcustomisation_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation White") },
                { "weaponcustomisation_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Weapon Customisation Yellow") },

                { "krait_mkii_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Mkii Shipkit 1 Tail 4") },
                { "cutter_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Cutter Shipkit 1 Bumper 1") },
                { "type6_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Spoiler 2") },
                { "type6_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Wings 4") },
                { "type6_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Type 6 Shipkit 1 Wings 3") },
                { "empire_courier_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Empire Courier Shipkit 1 Bumper 1") },
                { "federation_corvette_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Federation Corvette Shipkit 1 Wings 2") },
                { "krait_light_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,"Krait Light Shipkit 1 Tail 2") },

                { "paint", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,"Paint") },
                { "all", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,"Repair All") },
                { "hull", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,"Repair All") },
                { "wear", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,"Wear") },
            };
        }
    }
}
