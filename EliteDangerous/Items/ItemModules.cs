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

                    var newmodule = new ShipModule(-1, IsVanity(lowername) ? ShipModule.ModuleTypes.VanityType : ShipModule.ModuleTypes.UnknownType, 0, 0, candidatename);
                    System.Diagnostics.Trace.WriteLine($"*** Unknown Module {{ \"{lowername}\", new ShipModule(-1,{(IsVanity(lowername) ? "ShipModule.ModuleTypes.VanityType" : "ShipModule.ModuleTypes.UnknownType")},0,0,\"{candidatename}\" }}, - this will not affect operation but it would be nice to report it to us so we can add it to known module lists");

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
                ml["Unknown"] = new ShipModule(-1, ShipModule.ModuleTypes.UnknownType, 0, 0, "Unknown Type");
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

            public double? Mass { get; set; }        // mass of module
            public double? Power { get; set; }       // power used by module
            public int? Ammo { get; set; }
            public int? Clip { get; set; }
            public double? Damage { get; set; }
            public double? ReloadTime { get; set; } // s
            public double? Explosive { get; set; } //% bonus on base values
            public double? Kinetic { get; set; } //% bonus on base values
            public double? Thermal { get; set; } //% bonus on base values
            public double? ThermL { get; set; }
            public double? HullStrengthBonus { get; set; }  // % bonus over the ship information armour value
            public double? CausticReinforcement { get; set; } //%
            public double? HullReinforcement { get; set; } // units
            public double? ShieldReinforcement { get; set; } // units
            public double? AXResistance { get; set; } //% bonus on base values
            public double? RegenRate { get; set; } // units/s
            public double? BrokenRegenRate { get; set; } // units/s
            public double? MinStrength { get; set; } // shields
            public double? OptStrength { get; set; } // shields
            public double? MaxStrength { get; set; } // shields
            public double? TypicalEmission { get; set; } //m sensors
            public double? HeatEfficiency { get; set; } //% power plants
            public int? MaxCargo { get; set; } // hatch breaker limpet
            public int? MinCargo { get; set; } // hatch breaker limpet
            public int? HackTime { get; set; }// hatch breaker limpet
            public int? Limpets { get; set; }// collector controllers
            public double? FacingLimit { get; set; } // angle
            public int? Range { get; set; } // m
            public int? FallOff { get; set; } // m weapon fall off distance
            public double? BurstInterval { get; set; } // s weapon
            public double? RateOfFire { get; set; } // s weapon
            public int? Speed { get; set; } // m/s
            public double? Protection { get; set; } // multiplier
            public double? SysMW { get; set; } // power distributor rate MW/s
            public double? EngMW { get; set; } // power distributor rate MW/s
            public double? WepMW { get; set; } // power distributor rate MW/s
            public double? SysCap { get; set; } // max MW power distributor
            public double? EngCap { get; set; } // max MW power distributor
            public double? WepCap { get; set; } // max MW power distributor
            public double? PowerGen { get; set; } // MW power plant
            public int? OptMass { get; set; } // t
            public int? MaxMass { get; set; } // t
            public int? MinMass { get; set; } // t
            public double? EngineOptMultiplier { get; set; }
            public double? EngineMinMultiplier { get; set; }
            public double? EngineMaxMultiplier { get; set; }

            public int? Prisoners { get; set; }
            public int? Passengers { get; set; }
            public int? Bins { get; set; }
            public int? Size { get; set; } // tons
            public double? RefillRate { get; set; } // t/s
            public double? Time { get; set; } // s
            public int? Rebuilds { get; set; } // number

            public double? PowerConstant { get; set; } // Number
            public double? LinearConstant { get; set; } // Number
            public double? MaxFuelPerJump { get; set; } // t

            public double? SCOSpeedIncrease { get; set; }  // %
            public double? SCOAccelerationRate { get; set; }  // factor
            public double? SCOHeatGenerationRate { get; set; }  // factor
            public double? SCOControlInterference { get; set; }  // factor

            public double? AdditionalRange { get; set; } // ly

            public ShipModule()
            {
            }

            public ShipModule(ShipModule other)
            {
                EnglishModName = other.EnglishModName;
                TranslatedModName = other.TranslatedModName;
                ModuleID = other.ModuleID;
                ModType = other.ModType;
                Mass = other.Mass;
                Power = other.Power;
                Ammo = other.Ammo;
                Clip = other.Clip;
                Damage = other.Damage;
                ReloadTime = other.ReloadTime;
                ThermL = other.ThermL;
                Explosive = other.Explosive;
                Kinetic = other.Kinetic;
                Thermal = other.Thermal;
                CausticReinforcement = other.CausticReinforcement;
                HullReinforcement = other.HullReinforcement;
                ShieldReinforcement = other.ShieldReinforcement;
                AXResistance = other.AXResistance;
                RegenRate = other.RegenRate;
                BrokenRegenRate = other.BrokenRegenRate;
                MinStrength = other.MinStrength;
                OptStrength = other.OptStrength;
                MaxStrength = other.MaxStrength;
                TypicalEmission = other.TypicalEmission;
                HeatEfficiency = other.HeatEfficiency;
                MaxCargo = other.MaxCargo;
                MinCargo = other.MinCargo;
                HackTime = other.HackTime;
                Limpets = other.Limpets;
                FacingLimit = other.FacingLimit;
                Range = other.Range;
                FallOff = other.FallOff;
                BurstInterval = other.BurstInterval;
                RateOfFire = other.RateOfFire;
                Speed = other.Speed;
                Protection = other.Protection;
                SysMW = other.SysMW;
                EngMW = other.EngMW;
                WepMW = other.WepMW;
                SysCap = other.SysCap;
                EngCap= other.EngCap;
                WepCap = other.WepCap;
                PowerGen = other.PowerGen;
                EngineOptMultiplier = other.EngineOptMultiplier;
                EngineMinMultiplier = other.EngineMinMultiplier;
                EngineMaxMultiplier = other.EngineMaxMultiplier;
                OptMass = other.OptMass;
                MaxMass = other.MaxMass;
                MinMass = other.MinMass;
                Prisoners = other.Prisoners;
                Passengers = other.Passengers;
                Bins = other.Bins;
                Size = other.Size;
                RefillRate = other.RefillRate;
                Time = other.Time;
                Rebuilds = other.Rebuilds;
                PowerConstant = other.PowerConstant;
                LinearConstant = other.LinearConstant;
                MaxFuelPerJump = other.MaxFuelPerJump;
                SCOSpeedIncrease = other.SCOSpeedIncrease;
                SCOAccelerationRate = other.SCOAccelerationRate;
                SCOHeatGenerationRate = other.SCOHeatGenerationRate;
                SCOControlInterference = other.SCOControlInterference;
                AdditionalRange = other.AdditionalRange;
            }

            public string PropertiesAsText
            {
                get
                {
                    return BaseUtils.FieldBuilder.Build("Mass:;t;0.##", Mass, "Power:;MW;0.##", Power,
                        "Ammo:", Ammo, "Clip:", Clip, "Damage:", Damage, "Reload Time:;s", ReloadTime,
                        "Explosive:;%;0.#", Explosive, "Kinetic:;%;0.#", Kinetic, "Thermal:;%;0.#", Thermal,
                        "Caustic Reinforcement:;%;0.#", CausticReinforcement, "Hull Reinforcement Package:; units;0.#", HullReinforcement, 
                        "Shield Reinforcement:; units;0.#", ShieldReinforcement, "AXResistance:;%;0.#", AXResistance, "Hull Strength Bonus:;%;0.#",HullStrengthBonus,
                        "Generation Rate:; units/s;0.#", RegenRate, "Broken Generation Rate:; units/s;0.#", BrokenRegenRate,
                        "Min Strength:;%;0.#", MinStrength, "Optimum Strength:;%;0.#", OptStrength, "Maximum Strength:;%;0.#", MaxStrength,
                        "Typical Emissions:;m;0.#", TypicalEmission,
                        "Heat Efficiency:;/MW;0.#", HeatEfficiency,
                        "Max Cargo:;t;0.#", MaxCargo, "Min Cargo:;t;0.#", MinCargo, "Hack Time:;s;0.#", HackTime, "Limpets:", Limpets,
                        "Target Angle:;deg;0.##", FacingLimit, "Range:;m", Range,
                        "Burst Interval:;s;0.#", BurstInterval, "Rate of File:;shots/s;0.#", RateOfFire, "Speed:;m/s;0.#", Speed, "Fall Off Range:;m", FallOff,
                        "Protection:; units;", Protection,
                        "System Rate:;MW/s;0.#", SysMW, "Engine Rate:;MW/s;0.#", EngMW, "Weapons Rate:;MW/s;0.#", WepMW,
                        "System Cap:;MW;0.#", SysCap, "Engine Cap:;MW;0.#", EngCap, "Weapons Cap:;MW;0.#", WepCap, 
                        "Power Generation:;MW;0.#", PowerGen,
                        "Optimal Mass:;t", OptMass, "Max Mass:;t", MaxMass, "Min Mass:;t", MinMass,
                        "Engine Optimal Multipler:", EngineOptMultiplier, "Engine Max Multipler:", EngineMaxMultiplier, "Engine Min Multipler:", EngineMinMultiplier,
                        "Prisoners:", Prisoners, "Passengers:", Passengers, "Bins:", Bins, "Size:", Size,
                        "Refill Rate:;t/s;0.###", RefillRate,
                        "Time:;s;0.#", Time,
                        "Rebuilds:", Rebuilds,
                        "Max fuel per jump:;t;0.##", MaxFuelPerJump, "Power Constant:;;0.#", PowerConstant, "Linear Constant:;;0.#", PowerConstant,
                        "Speed Increase:;%", SCOSpeedIncrease, "Acceleration Rate:;;0.##", SCOAccelerationRate, "Heat Generation Rate:;;0.##", SCOHeatGenerationRate, "Control Interference:;;0.##", SCOControlInterference,
                        "Thermal Limit:; units/s", ThermL,
                        "Additional Range:;ly;0.##", AdditionalRange
                    );
                }
            }

            public EliteDangerousCalculations.FSDSpec GetFSDSpec()
            {
                System.Diagnostics.Debug.Assert(LinearConstant != null && PowerConstant != null && MaxFuelPerJump != null && OptMass != null);
                var fsd = new EliteDangerousCalculations.FSDSpec(PowerConstant.Value, LinearConstant.Value, OptMass.Value, MaxFuelPerJump.Value);
                return fsd;
            }

            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr)
            {
                ModuleID = id; TranslatedModName = EnglishModName = descr; ModType = modtype;
                if ( mass>0)
                    Mass = mass; 
                if ( power>0)
                    Power = power;
            }
            
            // armour
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                double Explosive, double Kinetic, double Thermal, double AXResistance, double HullStrengthBonus
                ) : this(id, modtype, mass, power, descr)
            {
                this.Explosive = Explosive; this.Kinetic = Kinetic; this.Thermal = Thermal; this.AXResistance = AXResistance; this.HullStrengthBonus = HullStrengthBonus;
            }

            // armour boosters/shwield boosters
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                double Explosive, double Kinetic, double Thermal, double? ShieldReinforcement = null, double? HullReinforcement = null
                ) : this(id, modtype, mass, power, descr)
            {
                this.Explosive = Explosive; this.Kinetic = Kinetic; this.Thermal = Thermal;
                this.ShieldReinforcement = ShieldReinforcement; this.HullReinforcement = HullReinforcement;
            }

            // meta alloy
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                double CausticReinforcement, double HullReinforcement
                ) : this(id, modtype, mass, power, descr)
            {
                this.CausticReinforcement = CausticReinforcement; this.HullReinforcement = HullReinforcement;
            }

            // guardian hull reinforcement
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                double Thermal, double CausticReinforcement, double HullReinforcement
                ) : this(id, modtype, mass, power, descr)
            {
                this.Thermal = Thermal;
                this.CausticReinforcement = CausticReinforcement; this.HullReinforcement = HullReinforcement;
            }

            // guardian shield reinforcement
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                double ShieldReinforcement
                ) : this(id, modtype, mass, power, descr)
            {
                this.ShieldReinforcement = ShieldReinforcement;
            }

            // auto field maint
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                int Ammo
                ) : this(id, modtype, mass, power, descr)
            {
                this.Ammo = Ammo;
            }

            // Misc
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, 
                int? Prisoners = null, int? Size = null, int? Range = null, int? Rebuilds = null, double? RefillRate = null, 
                double? PowerGen = null, double? Protection = null, double? FacingLimit = null, double? Reload = null, double? TypicalEmission = null,  double? HeatEfficiency = null,
                int? Bins = null, int? Time = null, double? AdditionalRange = null,
                int? Passengers = null) : this(id, modtype, mass, power, descr)
            {
                this.Prisoners = Prisoners; this.Size = Size; this.Range = Range; this.Rebuilds = Rebuilds; this.RefillRate = RefillRate;
                this.PowerGen = PowerGen; this.Protection = Protection; this.FacingLimit = FacingLimit; this.ReloadTime = Reload; this.TypicalEmission = TypicalEmission; this.HeatEfficiency = HeatEfficiency;
                this.Bins = Bins; this.Time = Time; this.AdditionalRange = AdditionalRange;
                this.Passengers = Passengers;
            }


            // resource siphons
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                    int Limpets, int Speed,int Range, int? HackTime = null, int? MinCargo = null, int? MaxCargo = null, 
                    int? Time = null, int? TargetRange = null) : this(id, modtype, mass, power, descr)
            {
                this.Limpets = Limpets; this.Speed = Speed; this.Range = Range; this.HackTime = HackTime;
                this.MinCargo = MinCargo; this.MaxCargo = MaxCargo; this.Time = Time; this.Range = TargetRange;
            }

            // Weapons
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, 
                                        int Clip, double Reload, int? Ammo = null, int? FallOff = null, double? ThermL = null, double? RateOfFire = null, 
                                        double? BurstInterval = null, int? Speed = null, double? Damage = null, int? Range = null) : this(id, modtype, mass, power, descr)
            {
                this.Clip = Clip;
                this.ReloadTime = Reload;
                this.Ammo = Ammo; 
                this.FallOff = FallOff;
                this.ThermL = ThermL;
                this.RateOfFire = RateOfFire;
                this.BurstInterval = BurstInterval;
                this.Speed = Speed;
                this.Damage = Damage;
                this.Range = Range;
            }

            // Weapons
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr,
                               double Damage, int Range, int FallOff,
                               double BurstInterval, double ThermL, double? RateOfFire = null, int? Speed = null, double? Reload = null) : this(id, modtype, mass, power, descr)
            {
                this.Damage = Damage; this.Range = Range; this.FallOff = FallOff;
                this.BurstInterval = BurstInterval;
                this.ThermL = ThermL;
                this.RateOfFire = RateOfFire;
                this.Speed = Speed; this.ReloadTime = Reload;
            }

            // power distributor
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, double SysMW, double EngMW, double WepMW,
                 double SysCap, double EngCap, double WepCap
                ) : this(id, modtype, mass, power, descr)
            {
                this.SysMW = SysMW; this.EngMW = EngMW; this.WepMW = WepMW;
                this.SysCap = SysCap; this.EngCap = EngCap; this.WepCap = WepCap;
            }

            // engines
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, int OptMass, int MaxMass, int MinMass, double ThermL, 
                            double EngineOptMultiplier, double EngineMinMultiplier, double EngineMaxMultiplier) : this(id, modtype, mass, power, descr)
            {
                this.MaxMass = MaxMass; this.MinMass = MinMass; this.OptMass = OptMass;
                this.EngineOptMultiplier = EngineOptMultiplier; this.EngineMinMultiplier = EngineMinMultiplier; this.EngineMaxMultiplier = EngineMaxMultiplier;
            }

            // auto field
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, int Ammo, double RepairL) : this(id, modtype, mass, power, descr)
            {
                this.Ammo = Ammo; this.Clip = Clip; this.ThermL = ThermL;
            }
            // shield cell bank
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, int Ammo, int Clip, double ThermL) : this(id, modtype, mass, power, descr)
            {
                this.Ammo = Ammo; this.Clip = Clip; this.ThermL = ThermL;
            }

            // shields
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, 
                                int OptMass, int MaxMass, int MinMass, double Explosive, double Kinetic, double Thermal, 
                                double AXResistance, double RegenRate, double BrokenRegenRate, double MinStrength, double OptStrength, double MaxStrength
                                                                    ) : this(id, modtype, mass, power, descr)
            {
                this.OptMass = OptMass; this.MaxMass = MaxMass; this.MinMass = MinMass; this.Explosive = Explosive; this.Kinetic = Kinetic; this.Thermal = Thermal;
                this.AXResistance = AXResistance; this.RegenRate = RegenRate; this.BrokenRegenRate = BrokenRegenRate; this.MinStrength = MinStrength; this.OptStrength = OptStrength; this.MaxStrength = MaxStrength;
            }

            // hyperdrive modules
            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, 
                                int OptMass, double PowerConstant, double LinearConstant, double MaxFuelPerJump, double ThermL) : this(id, modtype, mass, power, descr)
            {
                this.OptMass = OptMass; this.PowerConstant = PowerConstant; this.LinearConstant = LinearConstant; this.MaxFuelPerJump = MaxFuelPerJump; this.ThermL = ThermL;
            }

            public ShipModule(int id, ModuleTypes modtype, double mass, double power, string descr, 
                                int OptMass, double PowerConstant, double LinearConstant, double MaxFuelPerJump, double ThermL,
                                double SCOSpeedIncrease, double SCOAccelerationRate, double SCOHeatGenerationRate, double SCOControlInterference ) : this(id, modtype, mass, power, descr)
            {
                this.OptMass = OptMass; this.PowerConstant = PowerConstant; this.LinearConstant = LinearConstant; this.MaxFuelPerJump = MaxFuelPerJump; this.ThermL = ThermL;
                this.SCOSpeedIncrease = SCOSpeedIncrease; this.SCOAccelerationRate = SCOAccelerationRate; this.SCOHeatGenerationRate = SCOHeatGenerationRate; this.SCOControlInterference = SCOControlInterference;
            }


        }
        #endregion

        // History
        // Originally from coriolis, but now not.  Synced with Frontier data
        // Nov 1/12/23 synched with EDDI data, with outfitting.csv

        #region Ship Modules

        public static Dictionary<string, ShipModule> shipmodules = new Dictionary<string, ShipModule>
        {
            // Armour, in ID order
            // Raw Value of armour = ship.armour * (1+HullStrengthBonus/100)
            // Kinetic resistance = Raw * (1+Kinteic/100)
            // Explosive resistance = Raw * (1+Explosive/100)
            // Thermal resistance = Raw * (1+Thermal/100)
            // AX = Raw * (1+AXResistance/100) TBD

            { "sidewinder_armour_grade1", new ShipModule(128049250,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Sidewinder Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "sidewinder_armour_grade2", new ShipModule(128049251,ShipModule.ModuleTypes.ReinforcedAlloy,2,0,"Sidewinder Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "sidewinder_armour_grade3", new ShipModule(128049252,ShipModule.ModuleTypes.MilitaryGradeComposite,4,0,"Sidewinder Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "sidewinder_armour_mirrored", new ShipModule(128049253,ShipModule.ModuleTypes.MirroredSurfaceComposite,4,0,"Sidewinder Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "sidewinder_armour_reactive", new ShipModule(128049254,ShipModule.ModuleTypes.ReactiveSurfaceComposite,4,0,"Sidewinder Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "eagle_armour_grade1", new ShipModule(128049256,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Eagle Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "eagle_armour_grade2", new ShipModule(128049257,ShipModule.ModuleTypes.ReinforcedAlloy,4,0,"Eagle Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "eagle_armour_grade3", new ShipModule(128049258,ShipModule.ModuleTypes.MilitaryGradeComposite,8,0,"Eagle Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "eagle_armour_mirrored", new ShipModule(128049259,ShipModule.ModuleTypes.MirroredSurfaceComposite,8,0,"Eagle Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "eagle_armour_reactive", new ShipModule(128049260,ShipModule.ModuleTypes.ReactiveSurfaceComposite,8,0,"Eagle Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "hauler_armour_grade1", new ShipModule(128049262,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Hauler Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "hauler_armour_grade2", new ShipModule(128049263,ShipModule.ModuleTypes.ReinforcedAlloy,1,0,"Hauler Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "hauler_armour_grade3", new ShipModule(128049264,ShipModule.ModuleTypes.MilitaryGradeComposite,2,0,"Hauler Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "hauler_armour_mirrored", new ShipModule(128049265,ShipModule.ModuleTypes.MirroredSurfaceComposite,2,0,"Hauler Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "hauler_armour_reactive", new ShipModule(128049266,ShipModule.ModuleTypes.ReactiveSurfaceComposite,2,0,"Hauler Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "adder_armour_grade1", new ShipModule(128049268,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Adder Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "adder_armour_grade2", new ShipModule(128049269,ShipModule.ModuleTypes.ReinforcedAlloy,3,0,"Adder Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "adder_armour_grade3", new ShipModule(128049270,ShipModule.ModuleTypes.MilitaryGradeComposite,5,0,"Adder Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "adder_armour_mirrored", new ShipModule(128049271,ShipModule.ModuleTypes.MirroredSurfaceComposite,5,0,"Adder Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "adder_armour_reactive", new ShipModule(128049272,ShipModule.ModuleTypes.ReactiveSurfaceComposite,5,0,"Adder Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "viper_armour_grade1", new ShipModule(128049274,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Viper Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "viper_armour_grade2", new ShipModule(128049275,ShipModule.ModuleTypes.ReinforcedAlloy,5,0,"Viper Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "viper_armour_grade3", new ShipModule(128049276,ShipModule.ModuleTypes.MilitaryGradeComposite,9,0,"Viper Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "viper_armour_mirrored", new ShipModule(128049277,ShipModule.ModuleTypes.MirroredSurfaceComposite,9,0,"Viper Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "viper_armour_reactive", new ShipModule(128049278,ShipModule.ModuleTypes.ReactiveSurfaceComposite,9,0,"Viper Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "cobramkiii_armour_grade1", new ShipModule(128049280,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Cobra Mk III Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "cobramkiii_armour_grade2", new ShipModule(128049281,ShipModule.ModuleTypes.ReinforcedAlloy,14,0,"Cobra Mk III Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "cobramkiii_armour_grade3", new ShipModule(128049282,ShipModule.ModuleTypes.MilitaryGradeComposite,27,0,"Cobra Mk III Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "cobramkiii_armour_mirrored", new ShipModule(128049283,ShipModule.ModuleTypes.MirroredSurfaceComposite,27,0,"Cobra Mk III Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "cobramkiii_armour_reactive", new ShipModule(128049284,ShipModule.ModuleTypes.ReactiveSurfaceComposite,27,0,"Cobra Mk III Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "type6_armour_grade1", new ShipModule(128049286,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Type-6 Transporter Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "type6_armour_grade2", new ShipModule(128049287,ShipModule.ModuleTypes.ReinforcedAlloy,12,0,"Type-6 Transporter Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "type6_armour_grade3", new ShipModule(128049288,ShipModule.ModuleTypes.MilitaryGradeComposite,23,0,"Type-6 Transporter Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "type6_armour_mirrored", new ShipModule(128049289,ShipModule.ModuleTypes.MirroredSurfaceComposite,23,0,"Type-6 Transporter Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "type6_armour_reactive", new ShipModule(128049290,ShipModule.ModuleTypes.ReactiveSurfaceComposite,23,0,"Type-6 Transporter Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "dolphin_armour_grade1", new ShipModule(128049292,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Dolphin Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "dolphin_armour_grade2", new ShipModule(128049293,ShipModule.ModuleTypes.ReinforcedAlloy,32,0,"Dolphin Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "dolphin_armour_grade3", new ShipModule(128049294,ShipModule.ModuleTypes.MilitaryGradeComposite,63,0,"Dolphin Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "dolphin_armour_mirrored", new ShipModule(128049295,ShipModule.ModuleTypes.MirroredSurfaceComposite,63,0,"Dolphin Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "dolphin_armour_reactive", new ShipModule(128049296,ShipModule.ModuleTypes.ReactiveSurfaceComposite,63,0,"Dolphin Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "type7_armour_grade1", new ShipModule(128049298,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Type-7 Transporter Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "type7_armour_grade2", new ShipModule(128049299,ShipModule.ModuleTypes.ReinforcedAlloy,32,0,"Type-7 Transporter Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "type7_armour_grade3", new ShipModule(128049300,ShipModule.ModuleTypes.MilitaryGradeComposite,63,0,"Type-7 Transporter Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "type7_armour_mirrored", new ShipModule(128049301,ShipModule.ModuleTypes.MirroredSurfaceComposite,63,0,"Type-7 Transporter Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "type7_armour_reactive", new ShipModule(128049302,ShipModule.ModuleTypes.ReactiveSurfaceComposite,63,0,"Type-7 Transporter Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "asp_armour_grade1", new ShipModule(128049304,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Asp Explorer Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "asp_armour_grade2", new ShipModule(128049305,ShipModule.ModuleTypes.ReinforcedAlloy,21,0,"Asp Explorer Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "asp_armour_grade3", new ShipModule(128049306,ShipModule.ModuleTypes.MilitaryGradeComposite,42,0,"Asp Explorer Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "asp_armour_mirrored", new ShipModule(128049307,ShipModule.ModuleTypes.MirroredSurfaceComposite,42,0,"Asp Explorer Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "asp_armour_reactive", new ShipModule(128049308,ShipModule.ModuleTypes.ReactiveSurfaceComposite,42,0,"Asp Explorer Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "vulture_armour_grade1", new ShipModule(128049310,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Vulture Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "vulture_armour_grade2", new ShipModule(128049311,ShipModule.ModuleTypes.ReinforcedAlloy,17,0,"Vulture Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "vulture_armour_grade3", new ShipModule(128049312,ShipModule.ModuleTypes.MilitaryGradeComposite,35,0,"Vulture Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "vulture_armour_mirrored", new ShipModule(128049313,ShipModule.ModuleTypes.MirroredSurfaceComposite,35,0,"Vulture Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "vulture_armour_reactive", new ShipModule(128049314,ShipModule.ModuleTypes.ReactiveSurfaceComposite,35,0,"Vulture Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "empire_trader_armour_grade1", new ShipModule(128049316,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Imperial Clipper Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "empire_trader_armour_grade2", new ShipModule(128049317,ShipModule.ModuleTypes.ReinforcedAlloy,30,0,"Imperial Clipper Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "empire_trader_armour_grade3", new ShipModule(128049318,ShipModule.ModuleTypes.MilitaryGradeComposite,60,0,"Imperial Clipper Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_trader_armour_mirrored", new ShipModule(128049319,ShipModule.ModuleTypes.MirroredSurfaceComposite,60,0,"Imperial Clipper Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_trader_armour_reactive", new ShipModule(128049320,ShipModule.ModuleTypes.ReactiveSurfaceComposite,60,0,"Imperial Clipper Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "federation_dropship_armour_grade1", new ShipModule(128049322,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Federal Dropship Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "federation_dropship_armour_grade2", new ShipModule(128049323,ShipModule.ModuleTypes.ReinforcedAlloy,44,0,"Federal Dropship Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "federation_dropship_armour_grade3", new ShipModule(128049324,ShipModule.ModuleTypes.MilitaryGradeComposite,87,0,"Federal Dropship Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_dropship_armour_mirrored", new ShipModule(128049325,ShipModule.ModuleTypes.MirroredSurfaceComposite,87,0,"Federal Dropship Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_dropship_armour_reactive", new ShipModule(128049326,ShipModule.ModuleTypes.ReactiveSurfaceComposite,87,0,"Federal Dropship Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "orca_armour_grade1", new ShipModule(128049328,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Orca Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "orca_armour_grade2", new ShipModule(128049329,ShipModule.ModuleTypes.ReinforcedAlloy,21,0,"Orca Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "orca_armour_grade3", new ShipModule(128049330,ShipModule.ModuleTypes.MilitaryGradeComposite,87,0,"Orca Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "orca_armour_mirrored", new ShipModule(128049331,ShipModule.ModuleTypes.MirroredSurfaceComposite,87,0,"Orca Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "orca_armour_reactive", new ShipModule(128049332,ShipModule.ModuleTypes.ReactiveSurfaceComposite,87,0,"Orca Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "type9_armour_grade1", new ShipModule(128049334,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Type-9 Heavy Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "type9_armour_grade2", new ShipModule(128049335,ShipModule.ModuleTypes.ReinforcedAlloy,75,0,"Type-9 Heavy Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "type9_armour_grade3", new ShipModule(128049336,ShipModule.ModuleTypes.MilitaryGradeComposite,150,0,"Type-9 Heavy Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "type9_armour_mirrored", new ShipModule(128049337,ShipModule.ModuleTypes.MirroredSurfaceComposite,150,0,"Type-9 Heavy Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "type9_armour_reactive", new ShipModule(128049338,ShipModule.ModuleTypes.ReactiveSurfaceComposite,150,0,"Type-9 Heavy Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "python_armour_grade1", new ShipModule(128049340,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Python Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "python_armour_grade2", new ShipModule(128049341,ShipModule.ModuleTypes.ReinforcedAlloy,26,0,"Python Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "python_armour_grade3", new ShipModule(128049342,ShipModule.ModuleTypes.MilitaryGradeComposite,53,0,"Python Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "python_armour_mirrored", new ShipModule(128049343,ShipModule.ModuleTypes.MirroredSurfaceComposite,53,0,"Python Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "python_armour_reactive", new ShipModule(128049344,ShipModule.ModuleTypes.ReactiveSurfaceComposite,53,0,"Python Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "belugaliner_armour_grade1", new ShipModule(128049346,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Beluga Liner Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "belugaliner_armour_grade2", new ShipModule(128049347,ShipModule.ModuleTypes.ReinforcedAlloy,83,0,"Beluga Liner Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "belugaliner_armour_grade3", new ShipModule(128049348,ShipModule.ModuleTypes.MilitaryGradeComposite,165,0,"Beluga Liner Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "belugaliner_armour_mirrored", new ShipModule(128049349,ShipModule.ModuleTypes.MirroredSurfaceComposite,165,0,"Beluga Liner Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "belugaliner_armour_reactive", new ShipModule(128049350,ShipModule.ModuleTypes.ReactiveSurfaceComposite,165,0,"Beluga Liner Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "ferdelance_armour_grade1", new ShipModule(128049352,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Fer-de-Lance Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "ferdelance_armour_grade2", new ShipModule(128049353,ShipModule.ModuleTypes.ReinforcedAlloy,19,0,"Fer-de-Lance Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "ferdelance_armour_grade3", new ShipModule(128049354,ShipModule.ModuleTypes.MilitaryGradeComposite,38,0,"Fer-de-Lance Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "ferdelance_armour_mirrored", new ShipModule(128049355,ShipModule.ModuleTypes.MirroredSurfaceComposite,38,0,"Fer-de-Lance Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "ferdelance_armour_reactive", new ShipModule(128049356,ShipModule.ModuleTypes.ReactiveSurfaceComposite,38,0,"Fer-de-Lance Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "anaconda_armour_grade1", new ShipModule(128049364,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Anaconda Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "anaconda_armour_grade2", new ShipModule(128049365,ShipModule.ModuleTypes.ReinforcedAlloy,30,0,"Anaconda Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "anaconda_armour_grade3", new ShipModule(128049366,ShipModule.ModuleTypes.MilitaryGradeComposite,60,0,"Anaconda Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "anaconda_armour_mirrored", new ShipModule(128049367,ShipModule.ModuleTypes.MirroredSurfaceComposite,60,0,"Anaconda Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "anaconda_armour_reactive", new ShipModule(128049368,ShipModule.ModuleTypes.ReactiveSurfaceComposite,60,0,"Anaconda Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "federation_corvette_armour_grade1", new ShipModule(128049370,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Federal Corvette Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "federation_corvette_armour_grade2", new ShipModule(128049371,ShipModule.ModuleTypes.ReinforcedAlloy,30,0,"Federal Corvette Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "federation_corvette_armour_grade3", new ShipModule(128049372,ShipModule.ModuleTypes.MilitaryGradeComposite,60,0,"Federal Corvette Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_corvette_armour_mirrored", new ShipModule(128049373,ShipModule.ModuleTypes.MirroredSurfaceComposite,60,0,"Federal Corvette Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_corvette_armour_reactive", new ShipModule(128049374,ShipModule.ModuleTypes.ReactiveSurfaceComposite,60,0,"Federal Corvette Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "cutter_armour_grade1", new ShipModule(128049376,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Imperial Cutter Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "cutter_armour_grade2", new ShipModule(128049377,ShipModule.ModuleTypes.ReinforcedAlloy,30,0,"Imperial Cutter Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "cutter_armour_grade3", new ShipModule(128049378,ShipModule.ModuleTypes.MilitaryGradeComposite,60,0,"Imperial Cutter Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "cutter_armour_mirrored", new ShipModule(128049379,ShipModule.ModuleTypes.MirroredSurfaceComposite,60,0,"Imperial Cutter Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "cutter_armour_reactive", new ShipModule(128049380,ShipModule.ModuleTypes.ReactiveSurfaceComposite,60,0,"Imperial Cutter Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "diamondbackxl_armour_grade1", new ShipModule(128671832,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Diamondback Explorer Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "diamondbackxl_armour_grade2", new ShipModule(128671833,ShipModule.ModuleTypes.ReinforcedAlloy,23,0,"Diamondback Explorer Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "diamondbackxl_armour_grade3", new ShipModule(128671834,ShipModule.ModuleTypes.MilitaryGradeComposite,47,0,"Diamondback Explorer Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "diamondbackxl_armour_mirrored", new ShipModule(128671835,ShipModule.ModuleTypes.MirroredSurfaceComposite,47,0,"Diamondback Explorer Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "diamondbackxl_armour_reactive", new ShipModule(128671836,ShipModule.ModuleTypes.ReactiveSurfaceComposite,47,0,"Diamondback Explorer Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },


            { "empire_eagle_armour_grade1", new ShipModule(128672140,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Imperial Eagle Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "empire_eagle_armour_grade2", new ShipModule(128672141,ShipModule.ModuleTypes.ReinforcedAlloy,4,0,"Imperial Eagle Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "empire_eagle_armour_grade3", new ShipModule(128672142,ShipModule.ModuleTypes.MilitaryGradeComposite,8,0,"Imperial Eagle Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_eagle_armour_mirrored", new ShipModule(128672143,ShipModule.ModuleTypes.MirroredSurfaceComposite,8,0,"Imperial Eagle Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_eagle_armour_reactive", new ShipModule(128672144,ShipModule.ModuleTypes.ReactiveSurfaceComposite,8,0,"Imperial Eagle Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "federation_dropship_mkii_armour_grade1", new ShipModule(128672147,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Federal Assault Ship Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "federation_dropship_mkii_armour_grade2", new ShipModule(128672148,ShipModule.ModuleTypes.ReinforcedAlloy,44,0,"Federal Assault Ship Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "federation_dropship_mkii_armour_grade3", new ShipModule(128672149,ShipModule.ModuleTypes.MilitaryGradeComposite,87,0,"Federal Assault Ship Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_dropship_mkii_armour_mirrored", new ShipModule(128672150,ShipModule.ModuleTypes.MirroredSurfaceComposite,87,0,"Federal Assault Ship Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_dropship_mkii_armour_reactive", new ShipModule(128672151,ShipModule.ModuleTypes.ReactiveSurfaceComposite,87,0,"Federal Assault Ship Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "federation_gunship_armour_grade1", new ShipModule(128672154,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Federal Gunship Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "federation_gunship_armour_grade2", new ShipModule(128672155,ShipModule.ModuleTypes.ReinforcedAlloy,44,0,"Federal Gunship Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "federation_gunship_armour_grade3", new ShipModule(128672156,ShipModule.ModuleTypes.MilitaryGradeComposite,87,0,"Federal Gunship Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_gunship_armour_mirrored", new ShipModule(128672157,ShipModule.ModuleTypes.MirroredSurfaceComposite,87,0,"Federal Gunship Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "federation_gunship_armour_reactive", new ShipModule(128672158,ShipModule.ModuleTypes.ReactiveSurfaceComposite,87,0,"Federal Gunship Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "viper_mkiv_armour_grade1", new ShipModule(128672257,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Viper Mk IV Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "viper_mkiv_armour_grade2", new ShipModule(128672258,ShipModule.ModuleTypes.ReinforcedAlloy,5,0,"Viper Mk IV Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "viper_mkiv_armour_grade3", new ShipModule(128672259,ShipModule.ModuleTypes.MilitaryGradeComposite,9,0,"Viper Mk IV Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "viper_mkiv_armour_mirrored", new ShipModule(128672260,ShipModule.ModuleTypes.MirroredSurfaceComposite,9,0,"Viper Mk IV Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "viper_mkiv_armour_reactive", new ShipModule(128672261,ShipModule.ModuleTypes.ReactiveSurfaceComposite,9,0,"Viper Mk IV Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "cobramkiv_armour_grade1", new ShipModule(128672264,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Cobra Mk IV Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "cobramkiv_armour_grade2", new ShipModule(128672265,ShipModule.ModuleTypes.ReinforcedAlloy,14,0,"Cobra Mk IV Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "cobramkiv_armour_grade3", new ShipModule(128672266,ShipModule.ModuleTypes.MilitaryGradeComposite,27,0,"Cobra Mk IV Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "cobramkiv_armour_mirrored", new ShipModule(128672267,ShipModule.ModuleTypes.MirroredSurfaceComposite,27,0,"Cobra Mk IV Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "cobramkiv_armour_reactive", new ShipModule(128672268,ShipModule.ModuleTypes.ReactiveSurfaceComposite,27,0,"Cobra Mk IV Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "independant_trader_armour_grade1", new ShipModule(128672271,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Keelback Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "independant_trader_armour_grade2", new ShipModule(128672272,ShipModule.ModuleTypes.ReinforcedAlloy,12,0,"Keelback Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "independant_trader_armour_grade3", new ShipModule(128672273,ShipModule.ModuleTypes.MilitaryGradeComposite,23,0,"Keelback Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "independant_trader_armour_mirrored", new ShipModule(128672274,ShipModule.ModuleTypes.MirroredSurfaceComposite,23,0,"Keelback Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "independant_trader_armour_reactive", new ShipModule(128672275,ShipModule.ModuleTypes.ReactiveSurfaceComposite,23,0,"Keelback Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "asp_scout_armour_grade1", new ShipModule(128672278,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Asp Scout Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "asp_scout_armour_grade2", new ShipModule(128672279,ShipModule.ModuleTypes.ReinforcedAlloy,21,0,"Asp Scout Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "asp_scout_armour_grade3", new ShipModule(128672280,ShipModule.ModuleTypes.MilitaryGradeComposite,42,0,"Asp Scout Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "asp_scout_armour_mirrored", new ShipModule(128672281,ShipModule.ModuleTypes.MirroredSurfaceComposite,42,0,"Asp Scout Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "asp_scout_armour_reactive", new ShipModule(128672282,ShipModule.ModuleTypes.ReactiveSurfaceComposite,42,0,"Asp Scout Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },


            { "krait_mkii_armour_grade1", new ShipModule(128816569,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Krait Mk II Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "krait_mkii_armour_grade2", new ShipModule(128816570,ShipModule.ModuleTypes.ReinforcedAlloy,36,0,"Krait Mk II Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "krait_mkii_armour_grade3", new ShipModule(128816571,ShipModule.ModuleTypes.MilitaryGradeComposite,67,0,"Krait Mk II Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "krait_mkii_armour_mirrored", new ShipModule(128816572,ShipModule.ModuleTypes.MirroredSurfaceComposite,67,0,"Krait Mk II Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "krait_mkii_armour_reactive", new ShipModule(128816573,ShipModule.ModuleTypes.ReactiveSurfaceComposite,67,0,"Krait Mk II Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "typex_armour_grade1", new ShipModule(128816576,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Alliance Chieftain Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "typex_armour_grade2", new ShipModule(128816577,ShipModule.ModuleTypes.ReinforcedAlloy,40,0,"Alliance Chieftain Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "typex_armour_grade3", new ShipModule(128816578,ShipModule.ModuleTypes.MilitaryGradeComposite,78,0,"Alliance Chieftain Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_armour_mirrored", new ShipModule(128816579,ShipModule.ModuleTypes.MirroredSurfaceComposite,78,0,"Alliance Chieftain Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_armour_reactive", new ShipModule(128816580,ShipModule.ModuleTypes.ReactiveSurfaceComposite,78,0,"Alliance Chieftain Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "typex_2_armour_grade1", new ShipModule(128816583,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Alliance Crusader Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "typex_2_armour_grade2", new ShipModule(128816584,ShipModule.ModuleTypes.ReinforcedAlloy,40,0,"Alliance Crusader Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "typex_2_armour_grade3", new ShipModule(128816585,ShipModule.ModuleTypes.MilitaryGradeComposite,78,0,"Alliance Crusader Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_2_armour_mirrored", new ShipModule(128816586,ShipModule.ModuleTypes.MirroredSurfaceComposite,78,0,"Alliance Crusader Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_2_armour_reactive", new ShipModule(128816587,ShipModule.ModuleTypes.ReactiveSurfaceComposite,78,0,"Alliance Crusader Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "typex_3_armour_grade1", new ShipModule(128816590,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Alliance Challenger Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "typex_3_armour_grade2", new ShipModule(128816591,ShipModule.ModuleTypes.ReinforcedAlloy,40,0,"Alliance Challenger Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "typex_3_armour_grade3", new ShipModule(128816592,ShipModule.ModuleTypes.MilitaryGradeComposite,78,0,"Alliance Challenger Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_3_armour_mirrored", new ShipModule(128816593,ShipModule.ModuleTypes.MirroredSurfaceComposite,78,0,"Alliance Challenger Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "typex_3_armour_reactive", new ShipModule(128816594,ShipModule.ModuleTypes.ReactiveSurfaceComposite,78,0,"Alliance Challenger Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "diamondback_armour_grade1", new ShipModule(128671218,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Diamondback Scout Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "diamondback_armour_grade2", new ShipModule(128671219,ShipModule.ModuleTypes.ReinforcedAlloy,13,0,"Diamondback Scout Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "diamondback_armour_grade3", new ShipModule(128671220,ShipModule.ModuleTypes.MilitaryGradeComposite,26,0,"Diamondback Scout Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "diamondback_armour_mirrored", new ShipModule(128671221,ShipModule.ModuleTypes.MirroredSurfaceComposite,26,0,"Diamondback Scout Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "diamondback_armour_reactive", new ShipModule(128671222,ShipModule.ModuleTypes.ReactiveSurfaceComposite,26,0,"Diamondback Scout Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "empire_courier_armour_grade1", new ShipModule(128671224,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Imperial Courier Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "empire_courier_armour_grade2", new ShipModule(128671225,ShipModule.ModuleTypes.ReinforcedAlloy,4,0,"Imperial Courier Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "empire_courier_armour_grade3", new ShipModule(128671226,ShipModule.ModuleTypes.MilitaryGradeComposite,8,0,"Imperial Courier Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_courier_armour_mirrored", new ShipModule(128671227,ShipModule.ModuleTypes.MirroredSurfaceComposite,8,0,"Imperial Courier Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "empire_courier_armour_reactive", new ShipModule(128671228,ShipModule.ModuleTypes.ReactiveSurfaceComposite,8,0,"Imperial Courier Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "type9_military_armour_grade1", new ShipModule(128785621,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Type-10 Defender Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "type9_military_armour_grade2", new ShipModule(128785622,ShipModule.ModuleTypes.ReinforcedAlloy,75,0,"Type-10 Defender Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "type9_military_armour_grade3", new ShipModule(128785623,ShipModule.ModuleTypes.MilitaryGradeComposite,150,0,"Type-10 Defender Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "type9_military_armour_mirrored", new ShipModule(128785624,ShipModule.ModuleTypes.MirroredSurfaceComposite,150,0,"Type-10 Defender Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "type9_military_armour_reactive", new ShipModule(128785625,ShipModule.ModuleTypes.ReactiveSurfaceComposite,150,0,"Type-10 Defender Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "krait_light_armour_grade1", new ShipModule(128839283,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Krait Phantom Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "krait_light_armour_grade2", new ShipModule(128839284,ShipModule.ModuleTypes.ReinforcedAlloy,26,0,"Krait Phantom Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "krait_light_armour_grade3", new ShipModule(128839285,ShipModule.ModuleTypes.MilitaryGradeComposite,53,0,"Krait Phantom Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "krait_light_armour_mirrored", new ShipModule(128839286,ShipModule.ModuleTypes.MirroredSurfaceComposite,53,0,"Krait Phantom Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "krait_light_armour_reactive", new ShipModule(128839287,ShipModule.ModuleTypes.ReactiveSurfaceComposite,53,0,"Krait Phantom Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "mamba_armour_grade1", new ShipModule(128915981,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Mamba Lightweight Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:80 ) },
            { "mamba_armour_grade2", new ShipModule(128915982,ShipModule.ModuleTypes.ReinforcedAlloy,19,0,"Mamba Reinforced Alloy", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:152 ) },
            { "mamba_armour_grade3", new ShipModule(128915983,ShipModule.ModuleTypes.MilitaryGradeComposite,38,0,"Mamba Military Grade Composite", Explosive:-40, Kinetic:-20, Thermal:0, AXResistance:90, HullStrengthBonus:250 ) },
            { "mamba_armour_mirrored", new ShipModule(128915984,ShipModule.ModuleTypes.MirroredSurfaceComposite,38,0,"Mamba Mirrored Surface Composite", Explosive:-50, Kinetic:-75, Thermal:50, AXResistance:90, HullStrengthBonus:250 ) },
            { "mamba_armour_reactive", new ShipModule(128915985,ShipModule.ModuleTypes.ReactiveSurfaceComposite,38,0,"Mamba Reactive Surface Composite", Explosive:20, Kinetic:25, Thermal:-40, AXResistance:90, HullStrengthBonus:250 ) },

            { "python_nx_armour_grade1", new ShipModule(-1,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Python Mk II Lightweight Alloy",Explosive:-40, Kinetic:-20, Thermal:0 ) },
            { "python_nx_armour_grade2", new ShipModule(-1,ShipModule.ModuleTypes.ReinforcedAlloy,19,0,"Python Mk II Reinforced Alloy",Explosive:-40, Kinetic:-20, Thermal:0 ) },
            { "python_nx_armour_grade3", new ShipModule(-1,ShipModule.ModuleTypes.MilitaryGradeComposite,38,0,"Python Mk II Military Grade Composite",Explosive:-40, Kinetic:-20, Thermal:0 ) },
            { "python_nx_armour_mirrored", new ShipModule(-1,ShipModule.ModuleTypes.MirroredSurfaceComposite,38,0,"Python Mk II Mirrored Surface Composite",Explosive:-50, Kinetic:-75, Thermal:50 ) },
            { "python_nx_armour_reactive", new ShipModule(-1,ShipModule.ModuleTypes.ReactiveSurfaceComposite,38,0,"Python Mk II Reactive Surface Composite",Explosive:20, Kinetic:25, Thermal:-40 ) },

            // Auto field maint

            { "int_repairer_size1_class1", new ShipModule(128667598,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.54,"Auto Field Maintenance Unit Class 1 Rating E", Ammo:1000 ) },
            { "int_repairer_size1_class2", new ShipModule(128667606,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.72,"Auto Field Maintenance Unit Class 1 Rating D", Ammo:900 ) },
            { "int_repairer_size1_class3", new ShipModule(128667614,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.9,"Auto Field Maintenance Unit Class 1 Rating C", Ammo:1000 ) },
            { "int_repairer_size1_class4", new ShipModule(128667622,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.04,"Auto Field Maintenance Unit Class 1 Rating B", Ammo:1200 ) },
            { "int_repairer_size1_class5", new ShipModule(128667630,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.26,"Auto Field Maintenance Unit Class 1 Rating A", Ammo:1100 ) },
            { "int_repairer_size2_class1", new ShipModule(128667599,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.68,"Auto Field Maintenance Unit Class 2 Rating E", Ammo:2300 ) },
            { "int_repairer_size2_class2", new ShipModule(128667607,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.9,"Auto Field Maintenance Unit Class 2 Rating D", Ammo:2100 ) },
            { "int_repairer_size2_class3", new ShipModule(128667615,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.13,"Auto Field Maintenance Unit Class 2 Rating C", Ammo:2300 ) },
            { "int_repairer_size2_class4", new ShipModule(128667623,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.29,"Auto Field Maintenance Unit Class 2 Rating B", Ammo:2800 ) },
            { "int_repairer_size2_class5", new ShipModule(128667631,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.58,"Auto Field Maintenance Unit Class 2 Rating A", Ammo:2500 ) },
            { "int_repairer_size3_class1", new ShipModule(128667600,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.81,"Auto Field Maintenance Unit Class 3 Rating E", Ammo:3600 ) },
            { "int_repairer_size3_class2", new ShipModule(128667608,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.08,"Auto Field Maintenance Unit Class 3 Rating D", Ammo:3200 ) },
            { "int_repairer_size3_class3", new ShipModule(128667616,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.35,"Auto Field Maintenance Unit Class 3 Rating C", Ammo:3600 ) },
            { "int_repairer_size3_class4", new ShipModule(128667624,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.55,"Auto Field Maintenance Unit Class 3 Rating B", Ammo:4300 ) },
            { "int_repairer_size3_class5", new ShipModule(128667632,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.89,"Auto Field Maintenance Unit Class 3 Rating A", Ammo:4000 ) },
            { "int_repairer_size4_class1", new ShipModule(128667601,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,0.99,"Auto Field Maintenance Unit Class 4 Rating E", Ammo:4900 ) },
            { "int_repairer_size4_class2", new ShipModule(128667609,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.32,"Auto Field Maintenance Unit Class 4 Rating D", Ammo:4400 ) },
            { "int_repairer_size4_class3", new ShipModule(128667617,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.65,"Auto Field Maintenance Unit Class 4 Rating C", Ammo:4900 ) },
            { "int_repairer_size4_class4", new ShipModule(128667625,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.9,"Auto Field Maintenance Unit Class 4 Rating B", Ammo:5900 ) },
            { "int_repairer_size4_class5", new ShipModule(128667633,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.31,"Auto Field Maintenance Unit Class 4 Rating A", Ammo:5400 ) },
            { "int_repairer_size5_class1", new ShipModule(128667602,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.17,"Auto Field Maintenance Unit Class 5 Rating E", Ammo:6100 ) },
            { "int_repairer_size5_class2", new ShipModule(128667610,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.56,"Auto Field Maintenance Unit Class 5 Rating D", Ammo:5500 ) },
            { "int_repairer_size5_class3", new ShipModule(128667618,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.95,"Auto Field Maintenance Unit Class 5 Rating C", Ammo:6100 ) },
            { "int_repairer_size5_class4", new ShipModule(128667626,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.24,"Auto Field Maintenance Unit Class 5 Rating B", Ammo:7300 ) },
            { "int_repairer_size5_class5", new ShipModule(128667634,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.73,"Auto Field Maintenance Unit Class 5 Rating A", Ammo:6700 ) },
            { "int_repairer_size6_class1", new ShipModule(128667603,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.4,"Auto Field Maintenance Unit Class 6 Rating E", Ammo:7400 ) },
            { "int_repairer_size6_class2", new ShipModule(128667611,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.86,"Auto Field Maintenance Unit Class 6 Rating D", Ammo:6700 ) },
            { "int_repairer_size6_class3", new ShipModule(128667619,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.33,"Auto Field Maintenance Unit Class 6 Rating C", Ammo:7400 ) },
            { "int_repairer_size6_class4", new ShipModule(128667627,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.67,"Auto Field Maintenance Unit Class 6 Rating B", Ammo:8900 ) },
            { "int_repairer_size6_class5", new ShipModule(128667635,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,3.26,"Auto Field Maintenance Unit Class 6 Rating A", Ammo:8100 ) },
            { "int_repairer_size7_class1", new ShipModule(128667604,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.58,"Auto Field Maintenance Unit Class 7 Rating E", Ammo:8700 ) },
            { "int_repairer_size7_class2", new ShipModule(128667612,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.1,"Auto Field Maintenance Unit Class 7 Rating D", Ammo:7800 ) },
            { "int_repairer_size7_class3", new ShipModule(128667620,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.63,"Auto Field Maintenance Unit Class 7 Rating C", Ammo:8700 ) },
            { "int_repairer_size7_class4", new ShipModule(128667628,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,3.02,"Auto Field Maintenance Unit Class 7 Rating B", Ammo:10400 ) },
            { "int_repairer_size7_class5", new ShipModule(128667636,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,3.68,"Auto Field Maintenance Unit Class 7 Rating A", Ammo:9600 ) },
            { "int_repairer_size8_class1", new ShipModule(128667605,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,1.8,"Auto Field Maintenance Unit Class 8 Rating E", Ammo:10000 ) },
            { "int_repairer_size8_class2", new ShipModule(128667613,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,2.4,"Auto Field Maintenance Unit Class 8 Rating D", Ammo:9000 ) },
            { "int_repairer_size8_class3", new ShipModule(128667621,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,3,"Auto Field Maintenance Unit Class 8 Rating C", Ammo:10000 ) },
            { "int_repairer_size8_class4", new ShipModule(128667629,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,3.45,"Auto Field Maintenance Unit Class 8 Rating B", Ammo:12000 ) },
            { "int_repairer_size8_class5", new ShipModule(128667637,ShipModule.ModuleTypes.AutoField_MaintenanceUnit,0,4.2,"Auto Field Maintenance Unit Class 8 Rating A", Ammo:11000 ) },

            // Beam lasers

            { "hpt_beamlaser_fixed_small", new ShipModule(128049428,ShipModule.ModuleTypes.BeamLaser,2,0.62,"Beam Laser Fixed Small", Damage:9.82, Range:3000, FallOff:600, BurstInterval:0, ThermL:3.53 ) },
            { "hpt_beamlaser_fixed_medium", new ShipModule(128049429,ShipModule.ModuleTypes.BeamLaser,4,1.01,"Beam Laser Fixed Medium", Damage:15.96, Range:3000, FallOff:600, BurstInterval:0, ThermL:5.11 ) },
            { "hpt_beamlaser_fixed_large", new ShipModule(128049430,ShipModule.ModuleTypes.BeamLaser,8,1.62,"Beam Laser Fixed Large", Damage:25.78, Range:3000, FallOff:600, BurstInterval:0, ThermL:7.22 ) },
            { "hpt_beamlaser_fixed_huge", new ShipModule(128049431,ShipModule.ModuleTypes.BeamLaser,16,2.61,"Beam Laser Fixed Huge", Damage:41.38, Range:3000, FallOff:600, BurstInterval:0, ThermL:9.93 ) },
            { "hpt_beamlaser_gimbal_small", new ShipModule(128049432,ShipModule.ModuleTypes.BeamLaser,2,0.6,"Beam Laser Gimbal Small", Damage:7.68, Range:3000, FallOff:600, BurstInterval:0, ThermL:3.65 ) },
            { "hpt_beamlaser_gimbal_medium", new ShipModule(128049433,ShipModule.ModuleTypes.BeamLaser,4,1,"Beam Laser Gimbal Medium", Damage:12.52, Range:3000, FallOff:600, BurstInterval:0, ThermL:5.32 ) },
            { "hpt_beamlaser_gimbal_large", new ShipModule(128049434,ShipModule.ModuleTypes.BeamLaser,8,1.6,"Beam Laser Gimbal Large", Damage:20.3, Range:3000, FallOff:600, BurstInterval:0, ThermL:7.61 ) },
            { "hpt_beamlaser_turret_small", new ShipModule(128049435,ShipModule.ModuleTypes.BeamLaser,2,0.57,"Beam Laser Turret Small", Damage:5.4, Range:3000, FallOff:600, BurstInterval:0, ThermL:2.4 ) },
            { "hpt_beamlaser_turret_medium", new ShipModule(128049436,ShipModule.ModuleTypes.BeamLaser,4,0.93,"Beam Laser Turret Medium", Damage:8.83, Range:3000, FallOff:600, BurstInterval:0, ThermL:3.53 ) },
            { "hpt_beamlaser_turret_large", new ShipModule(128049437,ShipModule.ModuleTypes.BeamLaser,8,1.51,"Beam Laser Turret Large", Damage:14.36, Range:3000, FallOff:600, BurstInterval:0, ThermL:5.11 ) },

            { "hpt_beamlaser_fixed_small_heat", new ShipModule(128671346,ShipModule.ModuleTypes.RetributorBeamLaser,2,0.62,"Retributor Beam Laser Fixed Small", Damage:4.91, Range:3000, FallOff:600, BurstInterval:0, ThermL:2.7 ) },
            { "hpt_beamlaser_gimbal_huge", new ShipModule(128681994,ShipModule.ModuleTypes.BeamLaser,16,2.57,"Beam Laser Gimbal Huge", Damage:32.68, Range:3000, FallOff:600, BurstInterval:0, ThermL:10.62 ) },

            // burst laser

            { "hpt_pulselaserburst_fixed_small", new ShipModule(128049400,ShipModule.ModuleTypes.BurstLaser,2,0.65,"Burst Laser Fixed Small", Damage:1.72, Range:3000, FallOff:500, RateOfFire:4.74, BurstInterval:0.5, ThermL:0.38 ) },
            { "hpt_pulselaserburst_fixed_medium", new ShipModule(128049401,ShipModule.ModuleTypes.BurstLaser,4,1.05,"Burst Laser Fixed Medium", Damage:3.53, Range:3000, FallOff:500, RateOfFire:3.7, BurstInterval:0.63, ThermL:0.78 ) },
            { "hpt_pulselaserburst_fixed_large", new ShipModule(128049402,ShipModule.ModuleTypes.BurstLaser,8,1.66,"Burst Laser Fixed Large", Damage:7.73, Range:3000, FallOff:500, RateOfFire:2.69, BurstInterval:0.83, ThermL:1.7 ) },
            { "hpt_pulselaserburst_fixed_huge", new ShipModule(128049403,ShipModule.ModuleTypes.BurstLaser,16,2.58,"Burst Laser Fixed Huge", Damage:20.61, Range:3000, FallOff:500, RateOfFire:1.57, BurstInterval:1.25, ThermL:4.53 ) },
            { "hpt_pulselaserburst_gimbal_small", new ShipModule(128049404,ShipModule.ModuleTypes.BurstLaser,2,0.64,"Burst Laser Gimbal Small", Damage:1.22, Range:3000, FallOff:500, RateOfFire:5.29, BurstInterval:0.45, ThermL:0.34 ) },
            { "hpt_pulselaserburst_gimbal_medium", new ShipModule(128049405,ShipModule.ModuleTypes.BurstLaser,4,1.04,"Burst Laser Gimbal Medium", Damage:2.45, Range:3000, FallOff:500, RateOfFire:4.2, BurstInterval:0.56, ThermL:0.67 ) },
            { "hpt_pulselaserburst_gimbal_large", new ShipModule(128049406,ShipModule.ModuleTypes.BurstLaser,8,1.65,"Burst Laser Gimbal Large", Damage:5.16, Range:3000, FallOff:500, RateOfFire:3.22, BurstInterval:0.71, ThermL:1.42 ) },
            { "hpt_pulselaserburst_turret_small", new ShipModule(128049407,ShipModule.ModuleTypes.BurstLaser,2,0.6,"Burst Laser Turret Small", Damage:0.87, Range:3000, FallOff:500, RateOfFire:4.8, BurstInterval:0.52, ThermL:0.19 ) },
            { "hpt_pulselaserburst_turret_medium", new ShipModule(128049408,ShipModule.ModuleTypes.BurstLaser,4,0.98,"Burst Laser Turret Medium", Damage:1.72, Range:3000, FallOff:500, RateOfFire:3.93, BurstInterval:0.63, ThermL:0.38 ) },
            { "hpt_pulselaserburst_turret_large", new ShipModule(128049409,ShipModule.ModuleTypes.BurstLaser,8,1.57,"Burst Laser Turret Large", Damage:3.53, Range:3000, FallOff:500, RateOfFire:3.12, BurstInterval:0.78, ThermL:0.78 ) },


            { "hpt_pulselaserburst_gimbal_huge", new ShipModule(128727920,ShipModule.ModuleTypes.BurstLaser,16,2.59,"Burst Laser Gimbal Huge", Damage:12.09, Range:3000, FallOff:500, RateOfFire:2.14, BurstInterval:1, ThermL:3.33 ) },

            { "hpt_pulselaserburst_fixed_small_scatter", new ShipModule(128671449,ShipModule.ModuleTypes.CytoscramblerBurstLaser,2,0.8,"Cytoscrambler Burst Laser Fixed Small", Damage:3.6, Range:1000, FallOff:600, RateOfFire:7.62, BurstInterval:0.7, ThermL:0.28 ) },

            // Cannons

            { "hpt_cannon_fixed_small", new ShipModule(128049438,ShipModule.ModuleTypes.Cannon,2,0.34,"Cannon Fixed Small", Ammo:120, Clip:6, Speed:1200, Damage:22.5, Range:3000, FallOff:3000, RateOfFire:0.5, BurstInterval:2, Reload:3, ThermL:1.38 ) },
            { "hpt_cannon_fixed_medium", new ShipModule(128049439,ShipModule.ModuleTypes.Cannon,4,0.49,"Cannon Fixed Medium", Ammo:120, Clip:6, Speed:1051, Damage:36.875, Range:3500, FallOff:3500, RateOfFire:0.46, BurstInterval:2.17, Reload:3, ThermL:2.11 ) },
            { "hpt_cannon_fixed_large", new ShipModule(128049440,ShipModule.ModuleTypes.Cannon,8,0.67,"Cannon Fixed Large", Ammo:120, Clip:6, Speed:959, Damage:55.625, Range:4000, FallOff:4000, RateOfFire:0.42, BurstInterval:2.38, Reload:3, ThermL:3.2 ) },
            { "hpt_cannon_fixed_huge", new ShipModule(128049441,ShipModule.ModuleTypes.Cannon,16,0.92,"Cannon Fixed Huge", Ammo:120, Clip:6, Speed:900, Damage:83.125, Range:4500, FallOff:4500, RateOfFire:0.38, BurstInterval:2.63, Reload:3, ThermL:4.83 ) },
            { "hpt_cannon_gimbal_small", new ShipModule(128049442,ShipModule.ModuleTypes.Cannon,2,0.38,"Cannon Gimbal Small", Ammo:100, Clip:5, Speed:1000, Damage:15.92, Range:3000, FallOff:3000, RateOfFire:0.52, BurstInterval:1.92, Reload:4, ThermL:1.25 ) },
            { "hpt_cannon_gimbal_medium", new ShipModule(128049443,ShipModule.ModuleTypes.Cannon,4,0.54,"Cannon Gimbal Medium", Ammo:100, Clip:5, Speed:875, Damage:25.53, Range:3500, FallOff:3500, RateOfFire:0.48, BurstInterval:2.08, Reload:4, ThermL:1.92 ) },
            { "hpt_cannon_gimbal_huge", new ShipModule(128049444,ShipModule.ModuleTypes.Cannon,16,1.03,"Cannon Gimbal Huge", Ammo:100, Clip:5, Speed:750, Damage:56.59, Range:4500, FallOff:4500, RateOfFire:0.4, BurstInterval:2.5, Reload:4, ThermL:4.43 ) },
            { "hpt_cannon_turret_small", new ShipModule(128049445,ShipModule.ModuleTypes.Cannon,2,0.32,"Cannon Turret Small", Ammo:100, Clip:5, Speed:1000, Damage:12.77, Range:3000, FallOff:3000, RateOfFire:0.43, BurstInterval:2.31, Reload:4, ThermL:0.67 ) },
            { "hpt_cannon_turret_medium", new ShipModule(128049446,ShipModule.ModuleTypes.Cannon,4,0.45,"Cannon Turret Medium", Ammo:100, Clip:5, Speed:875, Damage:19.79, Range:3500, FallOff:3500, RateOfFire:0.4, BurstInterval:2.5, Reload:4, ThermL:1.03 ) },
            { "hpt_cannon_turret_large", new ShipModule(128049447,ShipModule.ModuleTypes.Cannon,8,0.64,"Cannon Turret Large", Ammo:100, Clip:5, Speed:800, Damage:30.34, Range:4000, FallOff:4000, RateOfFire:0.37, BurstInterval:2.72, Reload:4, ThermL:1.58 ) },

            { "hpt_cannon_gimbal_large", new ShipModule(128671120,ShipModule.ModuleTypes.Cannon,8,0.75,"Cannon Gimbal Large", Ammo:100, Clip:5, Speed:800, Damage:37.421, Range:4000, FallOff:4000, RateOfFire:0.44, BurstInterval:2.27, Reload:4, ThermL:2.93 ) },

            // Frag cannon

            { "hpt_slugshot_fixed_small", new ShipModule(128049448,ShipModule.ModuleTypes.FragmentCannon,2,0.45,"Fragment Cannon Fixed Small", Ammo:180, Clip:3, Speed:667, Damage:1.43, Range:2000, FallOff:1800, RateOfFire:5.56, BurstInterval:0.18, Reload:5, ThermL:0.41 ) },
            { "hpt_slugshot_fixed_medium", new ShipModule(128049449,ShipModule.ModuleTypes.FragmentCannon,4,0.74,"Fragment Cannon Fixed Medium", Ammo:180, Clip:3, Speed:667, Damage:2.985, Range:2000, FallOff:1800, RateOfFire:5, BurstInterval:0.2, Reload:5, ThermL:0.74 ) },
            { "hpt_slugshot_fixed_large", new ShipModule(128049450,ShipModule.ModuleTypes.FragmentCannon,8,1.02,"Fragment Cannon Fixed Large", Ammo:180, Clip:3, Speed:667, Damage:4.57, Range:2000, FallOff:1800, RateOfFire:4.55, BurstInterval:0.22, Reload:5, ThermL:1.13 ) },
            { "hpt_slugshot_gimbal_small", new ShipModule(128049451,ShipModule.ModuleTypes.FragmentCannon,2,0.59,"Fragment Cannon Gimbal Small", Ammo:180, Clip:3, Speed:667, Damage:1.01, Range:2000, FallOff:1800, RateOfFire:5.88, BurstInterval:0.17, Reload:5, ThermL:0.44 ) },
            { "hpt_slugshot_gimbal_medium", new ShipModule(128049452,ShipModule.ModuleTypes.FragmentCannon,4,1.03,"Fragment Cannon Gimbal Medium", Ammo:180, Clip:3, Speed:667, Damage:2.274, Range:2000, FallOff:1800, RateOfFire:5.26, BurstInterval:0.19, Reload:5, ThermL:0.84 ) },
            { "hpt_slugshot_turret_small", new ShipModule(128049453,ShipModule.ModuleTypes.FragmentCannon,2,0.42,"Fragment Cannon Turret Small", Ammo:180, Clip:3, Speed:667, Damage:0.69, Range:2000, FallOff:1800, RateOfFire:4.76, BurstInterval:0.21, Reload:5, ThermL:0.2 ) },
            { "hpt_slugshot_turret_medium", new ShipModule(128049454,ShipModule.ModuleTypes.FragmentCannon,4,0.79,"Fragment Cannon Turret Medium", Ammo:180, Clip:3, Speed:667, Damage:1.67, Range:2000, FallOff:1800, RateOfFire:4.35, BurstInterval:0.23, Reload:5, ThermL:0.41 ) },

            { "hpt_slugshot_gimbal_large", new ShipModule(128671321,ShipModule.ModuleTypes.FragmentCannon,8,1.55,"Fragment Cannon Gimbal Large", Ammo:180, Clip:3, Speed:667, Damage:3.77, Range:2000, FallOff:1800, RateOfFire:4.76, BurstInterval:0.21, Reload:5, ThermL:1.4 ) },
            { "hpt_slugshot_turret_large", new ShipModule(128671322,ShipModule.ModuleTypes.FragmentCannon,8,1.29,"Fragment Cannon Turret Large", Ammo:180, Clip:3, Speed:667, Damage:2.985, Range:2000, FallOff:1800, RateOfFire:4, BurstInterval:0.25, Reload:5, ThermL:0.74 ) },

            { "hpt_slugshot_fixed_large_range", new ShipModule(128671343,ShipModule.ModuleTypes.PacifierFrag_Cannon,8,1.02,"Pacifier Fragment Cannon Fixed Large", Ammo:180, Clip:3, Speed:1000, Damage:3.96, Range:3000, FallOff:2800, RateOfFire:4.55, BurstInterval:0.22, Reload:5, ThermL:1.13 ) },

            // Cargo racks

            { "int_cargorack_size1_class1", new ShipModule(128064338,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 1 Rating E", Size:2 ) },
            { "int_cargorack_size2_class1", new ShipModule(128064339,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 2 Rating E", Size:4 ) },
            { "int_cargorack_size3_class1", new ShipModule(128064340,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 3 Rating E", Size:8 ) },
            { "int_cargorack_size4_class1", new ShipModule(128064341,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 4 Rating E", Size:16 ) },
            { "int_cargorack_size5_class1", new ShipModule(128064342,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 5 Rating E", Size:32 ) },
            { "int_cargorack_size6_class1", new ShipModule(128064343,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 6 Rating E", Size:64 ) },
            { "int_cargorack_size7_class1", new ShipModule(128064344,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 7 Rating E", Size:128 ) },
            { "int_cargorack_size8_class1", new ShipModule(128064345,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 8 Rating E", Size:256 ) },

            { "int_cargorack_size2_class1_free", new ShipModule(128666643,ShipModule.ModuleTypes.CargoRack,0,0,"Cargo Rack Class 2 Rating E",Size:4 ) },

            { "int_corrosionproofcargorack_size1_class1", new ShipModule(128681641,ShipModule.ModuleTypes.CorrosionResistantCargoRack,0,0,"Anti Corrosion Cargo Rack Class 1 Rating E", Size:1 ) },
            { "int_corrosionproofcargorack_size1_class2", new ShipModule(128681992,ShipModule.ModuleTypes.CorrosionResistantCargoRack,0,0,"Anti Corrosion Cargo Rack Class 1 Rating F", Size:2 ) },

            { "int_corrosionproofcargorack_size4_class1", new ShipModule(128833944,ShipModule.ModuleTypes.CorrosionResistantCargoRack,0,0,"Anti Corrosion Cargo Rack Class 4 Rating E", Size:16 ) },
            { "int_corrosionproofcargorack_size5_class1", new ShipModule(128957069,ShipModule.ModuleTypes.CorrosionResistantCargoRack,0,0,"Anti Corrosion Cargo Rack Class 5 Rating E", Size:32 ) },
            { "int_corrosionproofcargorack_size6_class1", new ShipModule(999999906,ShipModule.ModuleTypes.CorrosionResistantCargoRack,0,0,"Anti Corrosion Cargo Rack Class 6 Rating E",Size:64 ) },

            // Manifest Scanner

            { "hpt_cargoscanner_size0_class1", new ShipModule(128662520,ShipModule.ModuleTypes.CargoScanner,1.3,0.2,"Manifest Scanner Rating E", FacingLimit:15, Range:2000, Time:10 ) },
            { "hpt_cargoscanner_size0_class2", new ShipModule(128662521,ShipModule.ModuleTypes.CargoScanner,1.3,0.4,"Manifest Scanner Rating D", FacingLimit:15, Range:2500, Time:10 ) },
            { "hpt_cargoscanner_size0_class3", new ShipModule(128662522,ShipModule.ModuleTypes.CargoScanner,1.3,0.8,"Manifest Scanner Rating C", FacingLimit:15, Range:3000, Time:10 ) },
            { "hpt_cargoscanner_size0_class4", new ShipModule(128662523,ShipModule.ModuleTypes.CargoScanner,1.3,1.6,"Manifest Scanner Rating B", FacingLimit:15, Range:3500, Time:10 ) },
            { "hpt_cargoscanner_size0_class5", new ShipModule(128662524,ShipModule.ModuleTypes.CargoScanner,1.3,3.2,"Manifest Scanner Rating A", FacingLimit:15, Range:4000, Time:10 ) },

            // Chaff, ECM

            { "hpt_chafflauncher_tiny", new ShipModule(128049513,ShipModule.ModuleTypes.ChaffLauncher,1.3,0.2,"Chaff Launcher Tiny", Ammo:10, Clip:1, RateOfFire:1, BurstInterval:1, Reload:10, ThermL:4 ) },
            { "hpt_electroniccountermeasure_tiny", new ShipModule(128049516,ShipModule.ModuleTypes.ElectronicCountermeasure,1.3,0.2,"Electronic Countermeasure Tiny", Range:3000, Time:3, Reload:10 ) },
            { "hpt_heatsinklauncher_turret_tiny", new ShipModule(128049519,ShipModule.ModuleTypes.HeatSinkLauncher,1.3,0.2,"Heat Sink Launcher Turret Tiny", Ammo:2, Clip:1, RateOfFire:0.2, BurstInterval:5, Reload:10 ) },
            { "hpt_causticsinklauncher_turret_tiny", new ShipModule(129019262,ShipModule.ModuleTypes.CausticSinkLauncher,1.7,0.6,"Caustic Sink Launcher Turret Tiny", Ammo:5, Clip:1, RateOfFire:0.2, BurstInterval:5, Reload:10 ) },
            { "hpt_plasmapointdefence_turret_tiny", new ShipModule(128049522,ShipModule.ModuleTypes.PointDefence,0.5,0.2,"Point Defence Turret Tiny", Ammo:10000, Clip:12, Speed:1000, Damage:0.2, Range:2500, RateOfFire:10, BurstInterval:0.2, Reload:0.4, ThermL:0.07 ) },

            // kill warrant

            { "hpt_crimescanner_size0_class1", new ShipModule(128662530,ShipModule.ModuleTypes.KillWarrantScanner,1.3,0.2,"Kill Warrant Scanner Rating E", FacingLimit:15, Range:2000, Time:10 ) },
            { "hpt_crimescanner_size0_class2", new ShipModule(128662531,ShipModule.ModuleTypes.KillWarrantScanner,1.3,0.4,"Kill Warrant Scanner Rating D", FacingLimit:15, Range:2500, Time:10 ) },
            { "hpt_crimescanner_size0_class3", new ShipModule(128662532,ShipModule.ModuleTypes.KillWarrantScanner,1.3,0.8,"Kill Warrant Scanner Rating C", FacingLimit:15, Range:3000, Time:10 ) },
            { "hpt_crimescanner_size0_class4", new ShipModule(128662533,ShipModule.ModuleTypes.KillWarrantScanner,1.3,1.6,"Kill Warrant Scanner Rating B", FacingLimit:15, Range:3500, Time:10 ) },
            { "hpt_crimescanner_size0_class5", new ShipModule(128662534,ShipModule.ModuleTypes.KillWarrantScanner,1.3,3.2,"Kill Warrant Scanner Rating A", FacingLimit:15, Range:4000, Time:10 ) },

            // surface scanner

            { "int_detailedsurfacescanner_tiny", new ShipModule(128666634,ShipModule.ModuleTypes.DetailedSurfaceScanner,0,0,"Detailed Surface Scanner" ) },

            // docking computer

            { "int_dockingcomputer_standard", new ShipModule(128049549,ShipModule.ModuleTypes.StandardDockingComputer,0,0.39,"Docking Computer Standard" ) },
            { "int_dockingcomputer_advanced", new ShipModule(128935155,ShipModule.ModuleTypes.AdvancedDockingComputer,0,0.45,"Docking Computer Advanced" ) },

            // figther bays

            { "int_fighterbay_size5_class1", new ShipModule(128727930,ShipModule.ModuleTypes.FighterHangar,20,0.25,"Fighter Hangar Class 5 Rating E", Size:1, Rebuilds:6 ) },
            { "int_fighterbay_size6_class1", new ShipModule(128727931,ShipModule.ModuleTypes.FighterHangar,40,0.35,"Fighter Hangar Class 6 Rating E", Size:2, Rebuilds:8 ) },
            { "int_fighterbay_size7_class1", new ShipModule(128727932,ShipModule.ModuleTypes.FighterHangar,60,0.35,"Fighter Hangar Class 7 Rating E", Size:2, Rebuilds:15 ) },

            // flak

            { "hpt_flakmortar_fixed_medium", new ShipModule(128785626,ShipModule.ModuleTypes.RemoteReleaseFlakLauncher,4,1.2,"Remote Release Flak Launcher Fixed Medium", Ammo:32, Clip:1, Speed:550, Damage:34, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:3.6 ) },
            { "hpt_flakmortar_turret_medium", new ShipModule(128793058,ShipModule.ModuleTypes.RemoteReleaseFlakLauncher,4,1.2,"Remote Release Flak Launcher Turret Medium", Ammo:32, Clip:1, Speed:550, Damage:34, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:3.6 ) },

            // flechette

            { "hpt_flechettelauncher_fixed_medium", new ShipModule(128833996,ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,4,1.2,"Remote Release Flechette Launcher Fixed Medium", Ammo:72, Clip:1, Speed:550, Damage:13, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:3.6 ) },
            { "hpt_flechettelauncher_turret_medium", new ShipModule(128833997,ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,4,1.2,"Remote Release Flechette Launcher Turret Medium", Ammo:72, Clip:1, Speed:550, Damage:13, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:3.6 ) },

            // Frame Shift Drive Interdictor

            { "int_fsdinterdictor_size1_class1", new ShipModule(128666704,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,1.3,0.14,"Frame Shift Drive Interdictor Class 1 Rating E", FacingLimit:50, Time:3 ) },
            { "int_fsdinterdictor_size2_class1", new ShipModule(128666705,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,2.5,0.17,"Frame Shift Drive Interdictor Class 2 Rating E", FacingLimit:50, Time:6 ) },
            { "int_fsdinterdictor_size3_class1", new ShipModule(128666706,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,5,0.2,"Frame Shift Drive Interdictor Class 3 Rating E", FacingLimit:50, Time:9 ) },
            { "int_fsdinterdictor_size4_class1", new ShipModule(128666707,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,10,0.25,"Frame Shift Drive Interdictor Class 4 Rating E", FacingLimit:50, Time:12 ) },
            { "int_fsdinterdictor_size1_class2", new ShipModule(128666708,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,0.5,0.18,"Frame Shift Drive Interdictor Class 1 Rating D", FacingLimit:50, Time:4 ) },
            { "int_fsdinterdictor_size2_class2", new ShipModule(128666709,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,1,0.22,"Frame Shift Drive Interdictor Class 2 Rating D", FacingLimit:50, Time:7 ) },
            { "int_fsdinterdictor_size3_class2", new ShipModule(128666710,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,2,0.27,"Frame Shift Drive Interdictor Class 3 Rating D", FacingLimit:50, Time:10 ) },
            { "int_fsdinterdictor_size4_class2", new ShipModule(128666711,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,4,0.33,"Frame Shift Drive Interdictor Class 4 Rating D", FacingLimit:50, Time:13 ) },
            { "int_fsdinterdictor_size1_class3", new ShipModule(128666712,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,1.3,0.23,"Frame Shift Drive Interdictor Class 1 Rating C", FacingLimit:50, Time:5 ) },
            { "int_fsdinterdictor_size2_class3", new ShipModule(128666713,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,2.5,0.28,"Frame Shift Drive Interdictor Class 2 Rating C", FacingLimit:50, Time:8 ) },
            { "int_fsdinterdictor_size3_class3", new ShipModule(128666714,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,5,0.34,"Frame Shift Drive Interdictor Class 3 Rating C", FacingLimit:50, Time:11 ) },
            { "int_fsdinterdictor_size4_class3", new ShipModule(128666715,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,10,0.41,"Frame Shift Drive Interdictor Class 4 Rating C", FacingLimit:50, Time:14 ) },
            { "int_fsdinterdictor_size1_class4", new ShipModule(128666716,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,2,0.28,"Frame Shift Drive Interdictor Class 1 Rating B", FacingLimit:50, Time:6 ) },
            { "int_fsdinterdictor_size2_class4", new ShipModule(128666717,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,4,0.34,"Frame Shift Drive Interdictor Class 2 Rating B", FacingLimit:50, Time:9 ) },
            { "int_fsdinterdictor_size3_class4", new ShipModule(128666718,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,8,0.41,"Frame Shift Drive Interdictor Class 3 Rating B", FacingLimit:50, Time:12 ) },
            { "int_fsdinterdictor_size4_class4", new ShipModule(128666719,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,16,0.49,"Frame Shift Drive Interdictor Class 4 Rating B", FacingLimit:50, Time:15 ) },
            { "int_fsdinterdictor_size1_class5", new ShipModule(128666720,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,1.3,0.32,"Frame Shift Drive Interdictor Class 1 Rating A", FacingLimit:50, Time:7 ) },
            { "int_fsdinterdictor_size2_class5", new ShipModule(128666721,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,2.5,0.39,"Frame Shift Drive Interdictor Class 2 Rating A", FacingLimit:50, Time:10 ) },
            { "int_fsdinterdictor_size3_class5", new ShipModule(128666722,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,5,0.48,"Frame Shift Drive Interdictor Class 3 Rating A", FacingLimit:50, Time:13 ) },
            { "int_fsdinterdictor_size4_class5", new ShipModule(128666723,ShipModule.ModuleTypes.FrameShiftDriveInterdictor,10,0.57,"Frame Shift Drive Interdictor Class 4 Rating A", FacingLimit:50, Time:16 ) },

            // Fuel scoop

            { "int_fuelscoop_size1_class1", new ShipModule(128666644,ShipModule.ModuleTypes.FuelScoop,0,0.14,"Fuel Scoop Class 1 Rating E", RefillRate:0.018 ) },
            { "int_fuelscoop_size2_class1", new ShipModule(128666645,ShipModule.ModuleTypes.FuelScoop,0,0.17,"Fuel Scoop Class 2 Rating E", RefillRate:0.032 ) },
            { "int_fuelscoop_size3_class1", new ShipModule(128666646,ShipModule.ModuleTypes.FuelScoop,0,0.2,"Fuel Scoop Class 3 Rating E", RefillRate:0.075 ) },
            { "int_fuelscoop_size4_class1", new ShipModule(128666647,ShipModule.ModuleTypes.FuelScoop,0,0.25,"Fuel Scoop Class 4 Rating E", RefillRate:0.147 ) },
            { "int_fuelscoop_size5_class1", new ShipModule(128666648,ShipModule.ModuleTypes.FuelScoop,0,0.3,"Fuel Scoop Class 5 Rating E", RefillRate:0.247 ) },
            { "int_fuelscoop_size6_class1", new ShipModule(128666649,ShipModule.ModuleTypes.FuelScoop,0,0.35,"Fuel Scoop Class 6 Rating E", RefillRate:0.376 ) },
            { "int_fuelscoop_size7_class1", new ShipModule(128666650,ShipModule.ModuleTypes.FuelScoop,0,0.41,"Fuel Scoop Class 7 Rating E", RefillRate:0.534 ) },
            { "int_fuelscoop_size8_class1", new ShipModule(128666651,ShipModule.ModuleTypes.FuelScoop,0,0.48,"Fuel Scoop Class 8 Rating E", RefillRate:0.72 ) },
            { "int_fuelscoop_size1_class2", new ShipModule(128666652,ShipModule.ModuleTypes.FuelScoop,0,0.18,"Fuel Scoop Class 1 Rating D", RefillRate:0.024 ) },
            { "int_fuelscoop_size2_class2", new ShipModule(128666653,ShipModule.ModuleTypes.FuelScoop,0,0.22,"Fuel Scoop Class 2 Rating D", RefillRate:0.043 ) },
            { "int_fuelscoop_size3_class2", new ShipModule(128666654,ShipModule.ModuleTypes.FuelScoop,0,0.27,"Fuel Scoop Class 3 Rating D", RefillRate:0.1 ) },
            { "int_fuelscoop_size4_class2", new ShipModule(128666655,ShipModule.ModuleTypes.FuelScoop,0,0.33,"Fuel Scoop Class 4 Rating D", RefillRate:0.196 ) },
            { "int_fuelscoop_size5_class2", new ShipModule(128666656,ShipModule.ModuleTypes.FuelScoop,0,0.4,"Fuel Scoop Class 5 Rating D", RefillRate:0.33 ) },
            { "int_fuelscoop_size6_class2", new ShipModule(128666657,ShipModule.ModuleTypes.FuelScoop,0,0.47,"Fuel Scoop Class 6 Rating D", RefillRate:0.502 ) },
            { "int_fuelscoop_size7_class2", new ShipModule(128666658,ShipModule.ModuleTypes.FuelScoop,0,0.55,"Fuel Scoop Class 7 Rating D", RefillRate:0.712 ) },
            { "int_fuelscoop_size8_class2", new ShipModule(128666659,ShipModule.ModuleTypes.FuelScoop,0,0.64,"Fuel Scoop Class 8 Rating D", RefillRate:0.96 ) },
            { "int_fuelscoop_size1_class3", new ShipModule(128666660,ShipModule.ModuleTypes.FuelScoop,0,0.23,"Fuel Scoop Class 1 Rating C", RefillRate:0.03 ) },
            { "int_fuelscoop_size2_class3", new ShipModule(128666661,ShipModule.ModuleTypes.FuelScoop,0,0.28,"Fuel Scoop Class 2 Rating C", RefillRate:0.054 ) },
            { "int_fuelscoop_size3_class3", new ShipModule(128666662,ShipModule.ModuleTypes.FuelScoop,0,0.34,"Fuel Scoop Class 3 Rating C", RefillRate:0.126 ) },
            { "int_fuelscoop_size4_class3", new ShipModule(128666663,ShipModule.ModuleTypes.FuelScoop,0,0.41,"Fuel Scoop Class 4 Rating C", RefillRate:0.245 ) },
            { "int_fuelscoop_size5_class3", new ShipModule(128666664,ShipModule.ModuleTypes.FuelScoop,0,0.5,"Fuel Scoop Class 5 Rating C", RefillRate:0.412 ) },
            { "int_fuelscoop_size6_class3", new ShipModule(128666665,ShipModule.ModuleTypes.FuelScoop,0,0.59,"Fuel Scoop Class 6 Rating C", RefillRate:0.627 ) },
            { "int_fuelscoop_size7_class3", new ShipModule(128666666,ShipModule.ModuleTypes.FuelScoop,0,0.69,"Fuel Scoop Class 7 Rating C", RefillRate:0.89 ) },
            { "int_fuelscoop_size8_class3", new ShipModule(128666667,ShipModule.ModuleTypes.FuelScoop,0,0.8,"Fuel Scoop Class 8 Rating C", RefillRate:1.2 ) },
            { "int_fuelscoop_size1_class4", new ShipModule(128666668,ShipModule.ModuleTypes.FuelScoop,0,0.28,"Fuel Scoop Class 1 Rating B", RefillRate:0.036 ) },
            { "int_fuelscoop_size2_class4", new ShipModule(128666669,ShipModule.ModuleTypes.FuelScoop,0,0.34,"Fuel Scoop Class 2 Rating B", RefillRate:0.065 ) },
            { "int_fuelscoop_size3_class4", new ShipModule(128666670,ShipModule.ModuleTypes.FuelScoop,0,0.41,"Fuel Scoop Class 3 Rating B", RefillRate:0.151 ) },
            { "int_fuelscoop_size4_class4", new ShipModule(128666671,ShipModule.ModuleTypes.FuelScoop,0,0.49,"Fuel Scoop Class 4 Rating B", RefillRate:0.294 ) },
            { "int_fuelscoop_size5_class4", new ShipModule(128666672,ShipModule.ModuleTypes.FuelScoop,0,0.6,"Fuel Scoop Class 5 Rating B", RefillRate:0.494 ) },
            { "int_fuelscoop_size6_class4", new ShipModule(128666673,ShipModule.ModuleTypes.FuelScoop,0,0.71,"Fuel Scoop Class 6 Rating B", RefillRate:0.752 ) },
            { "int_fuelscoop_size6_class5", new ShipModule(128666681,ShipModule.ModuleTypes.FuelScoop,0,0.83,"Fuel Scoop Class 6 Rating A", RefillRate:0.878 ) },
            { "int_fuelscoop_size7_class4", new ShipModule(128666674,ShipModule.ModuleTypes.FuelScoop,0,0.83,"Fuel Scoop Class 7 Rating B", RefillRate:1.068 ) },
            { "int_fuelscoop_size8_class4", new ShipModule(128666675,ShipModule.ModuleTypes.FuelScoop,0,0.96,"Fuel Scoop Class 8 Rating B", RefillRate:1.44 ) },
            { "int_fuelscoop_size1_class5", new ShipModule(128666676,ShipModule.ModuleTypes.FuelScoop,0,0.32,"Fuel Scoop Class 1 Rating A", RefillRate:0.042 ) },
            { "int_fuelscoop_size2_class5", new ShipModule(128666677,ShipModule.ModuleTypes.FuelScoop,0,0.39,"Fuel Scoop Class 2 Rating A", RefillRate:0.075 ) },
            { "int_fuelscoop_size3_class5", new ShipModule(128666678,ShipModule.ModuleTypes.FuelScoop,0,0.48,"Fuel Scoop Class 3 Rating A", RefillRate:0.176 ) },
            { "int_fuelscoop_size4_class5", new ShipModule(128666679,ShipModule.ModuleTypes.FuelScoop,0,0.57,"Fuel Scoop Class 4 Rating A", RefillRate:0.342 ) },
            { "int_fuelscoop_size5_class5", new ShipModule(128666680,ShipModule.ModuleTypes.FuelScoop,0,0.7,"Fuel Scoop Class 5 Rating A", RefillRate:0.577 ) },
            { "int_fuelscoop_size7_class5", new ShipModule(128666682,ShipModule.ModuleTypes.FuelScoop,0,0.97,"Fuel Scoop Class 7 Rating A", RefillRate:1.245 ) },
            { "int_fuelscoop_size8_class5", new ShipModule(128666683,ShipModule.ModuleTypes.FuelScoop,0,1.12,"Fuel Scoop Class 8 Rating A", RefillRate:1.68 ) },

            // fuel tank

            { "int_fueltank_size1_class3", new ShipModule(128064346,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 1 Rating C", Size:2 ) },
            { "int_fueltank_size2_class3", new ShipModule(128064347,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 2 Rating C", Size:4 ) },
            { "int_fueltank_size3_class3", new ShipModule(128064348,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 3 Rating C", Size:8 ) },
            { "int_fueltank_size4_class3", new ShipModule(128064349,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 4 Rating C", Size:16 ) },
            { "int_fueltank_size5_class3", new ShipModule(128064350,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 5 Rating C", Size:32 ) },
            { "int_fueltank_size6_class3", new ShipModule(128064351,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 6 Rating C", Size:64 ) },
            { "int_fueltank_size7_class3", new ShipModule(128064352,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 7 Rating C", Size:128 ) },
            { "int_fueltank_size8_class3", new ShipModule(128064353,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 8 Rating C", Size:256 ) },

            { "int_fueltank_size1_class3_free", new ShipModule(128667018,ShipModule.ModuleTypes.FuelTank,0,0,"Fuel Tank Class 1 Rating C",Size:2 ) },

            // Gardian

            { "hpt_guardian_plasmalauncher_turret_small", new ShipModule(128891606,ShipModule.ModuleTypes.GuardianPlasmaCharger,2,1.6,"Guardian Plasma Charger Turret Small", Ammo:200, Clip:15, Speed:1200, Damage:2, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:5.01 ) },
            { "hpt_guardian_plasmalauncher_fixed_small", new ShipModule(128891607,ShipModule.ModuleTypes.GuardianPlasmaCharger,2,1.4,"Guardian Plasma Charger Fixed Small", Ammo:200, Clip:15, Speed:1200, Damage:3, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:4.21 ) },
            { "hpt_guardian_shardcannon_turret_small", new ShipModule(128891608,ShipModule.ModuleTypes.GuardianShardCannon,2,0.72,"Guardian Shard Cannon Turret Small", Ammo:180, Clip:5, Speed:1133, Damage:2.02, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:0.58 ) },
            { "hpt_guardian_shardcannon_fixed_small", new ShipModule(128891609,ShipModule.ModuleTypes.GuardianShardCannon,2,0.87,"Guardian Shard Cannon Fixed Small", Ammo:180, Clip:5, Speed:1133, Damage:3.64, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:0.69 ) },
            { "hpt_guardian_gausscannon_fixed_small", new ShipModule(128891610,ShipModule.ModuleTypes.GuardianGaussCannon,2,1.91,"Guardian Gauss Cannon Fixed Small", Ammo:80, Clip:1, Damage:40, Range:3000, FallOff:1500, RateOfFire:1.21, BurstInterval:0.83, Reload:1, ThermL:15 ) },

            { "hpt_guardian_gausscannon_fixed_medium", new ShipModule(128833687,ShipModule.ModuleTypes.GuardianGaussCannon,4,2.61,"Guardian Gauss Cannon Fixed Medium", Ammo:80, Clip:1, Damage:70, Range:3000, FallOff:1500, RateOfFire:1.21, BurstInterval:0.83, Reload:1, ThermL:25 ) },

            { "hpt_guardian_plasmalauncher_fixed_medium", new ShipModule(128833998,ShipModule.ModuleTypes.GuardianPlasmaCharger,4,2.13,"Guardian Plasma Charger Fixed Medium", Ammo:200, Clip:15, Speed:1200, Damage:5, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:5.21 ) },
            { "hpt_guardian_plasmalauncher_turret_medium", new ShipModule(128833999,ShipModule.ModuleTypes.GuardianPlasmaCharger,4,2.01,"Guardian Plasma Charger Turret Medium", Ammo:200, Clip:15, Speed:1200, Damage:4, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:5.8 ) },
            { "hpt_guardian_shardcannon_fixed_medium", new ShipModule(128834000,ShipModule.ModuleTypes.GuardianShardCannon,4,1.21,"Guardian Shard Cannon Fixed Medium", Ammo:180, Clip:5, Speed:1133, Damage:6.77, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:1.2 ) },
            { "hpt_guardian_shardcannon_turret_medium", new ShipModule(128834001,ShipModule.ModuleTypes.GuardianShardCannon,4,1.16,"Guardian Shard Cannon Turret Medium", Ammo:180, Clip:5, Speed:1133, Damage:4.34, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:1.09 ) },

            { "hpt_guardian_plasmalauncher_fixed_large", new ShipModule(128834783,ShipModule.ModuleTypes.GuardianPlasmaCharger,8,3.1,"Guardian Plasma Charger Fixed Large", Ammo:200, Clip:15, Speed:1200, Damage:7, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:6.15 ) },
            { "hpt_guardian_plasmalauncher_turret_large", new ShipModule(128834784,ShipModule.ModuleTypes.GuardianPlasmaCharger,8,2.53,"Guardian Plasma Charger Turret Large", Ammo:200, Clip:15, Speed:1200, Damage:6, Range:3000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, Reload:3, ThermL:6.4 ) },

            { "hpt_guardian_shardcannon_fixed_large", new ShipModule(128834778,ShipModule.ModuleTypes.GuardianShardCannon,8,1.68,"Guardian Shard Cannon Fixed Large", Ammo:180, Clip:5, Speed:1133, Damage:9.5, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:2.2 ) },
            { "hpt_guardian_shardcannon_turret_large", new ShipModule(128834779,ShipModule.ModuleTypes.GuardianShardCannon,8,1.39,"Guardian Shard Cannon Turret Large", Ammo:180, Clip:5, Speed:1133, Damage:6.2, Range:1700, FallOff:1700, RateOfFire:1.67, BurstInterval:0.6, Reload:5, ThermL:1.98 ) },

            { "int_guardianhullreinforcement_size1_class2", new ShipModule(128833946,ShipModule.ModuleTypes.GuardianHullReinforcement,1,0.56,"Guardian Hull Reinforcement Package Class 1 Rating D", Thermal:2, CausticReinforcement:5, HullReinforcement:138 ) },
            { "int_guardianhullreinforcement_size1_class1", new ShipModule(128833945,ShipModule.ModuleTypes.GuardianHullReinforcement,2,0.45,"Guardian Hull Reinforcement Package Class 1 Rating E", Thermal:2, CausticReinforcement:5, HullReinforcement:100 ) },
            { "int_guardianhullreinforcement_size2_class1", new ShipModule(128833947,ShipModule.ModuleTypes.GuardianHullReinforcement,4,0.68,"Guardian Hull Reinforcement Package Class 2 Rating E", Thermal:2, CausticReinforcement:5, HullReinforcement:188 ) },
            { "int_guardianhullreinforcement_size2_class2", new ShipModule(128833948,ShipModule.ModuleTypes.GuardianHullReinforcement,2,0.79,"Guardian Hull Reinforcement Package Class 2 Rating D", Thermal:2, CausticReinforcement:5, HullReinforcement:238 ) },
            { "int_guardianhullreinforcement_size3_class1", new ShipModule(128833949,ShipModule.ModuleTypes.GuardianHullReinforcement,8,0.9,"Guardian Hull Reinforcement Package Class 3 Rating E", Thermal:2, CausticReinforcement:5, HullReinforcement:288 ) },
            { "int_guardianhullreinforcement_size3_class2", new ShipModule(128833950,ShipModule.ModuleTypes.GuardianHullReinforcement,4,1.01,"Guardian Hull Reinforcement Package Class 3 Rating D", Thermal:2, CausticReinforcement:5, HullReinforcement:325 ) },
            { "int_guardianhullreinforcement_size4_class1", new ShipModule(128833951,ShipModule.ModuleTypes.GuardianHullReinforcement,16,1.13,"Guardian Hull Reinforcement Package Class 4 Rating E", Thermal:2, CausticReinforcement:5, HullReinforcement:375 ) },
            { "int_guardianhullreinforcement_size4_class2", new ShipModule(128833952,ShipModule.ModuleTypes.GuardianHullReinforcement,8,1.24,"Guardian Hull Reinforcement Package Class 4 Rating D", Thermal:2, CausticReinforcement:5, HullReinforcement:413 ) },
            { "int_guardianhullreinforcement_size5_class1", new ShipModule(128833953,ShipModule.ModuleTypes.GuardianHullReinforcement,32,1.35,"Guardian Hull Reinforcement Package Class 5 Rating E", Thermal:2, CausticReinforcement:5, HullReinforcement:450 ) },
            { "int_guardianhullreinforcement_size5_class2", new ShipModule(128833954,ShipModule.ModuleTypes.GuardianHullReinforcement,16,1.46,"Guardian Hull Reinforcement Package Class 5 Rating D", Thermal:2, CausticReinforcement:5, HullReinforcement:488 ) },

            { "int_guardianmodulereinforcement_size1_class1", new ShipModule(128833955,ShipModule.ModuleTypes.GuardianModuleReinforcement,2,0.27,"Guardian Module Reinforcement Package Class 1 Rating E", Protection:30 ) },
            { "int_guardianmodulereinforcement_size1_class2", new ShipModule(128833956,ShipModule.ModuleTypes.GuardianModuleReinforcement,1,0.34,"Guardian Module Reinforcement Package Class 1 Rating D", Protection:60 ) },
            { "int_guardianmodulereinforcement_size2_class1", new ShipModule(128833957,ShipModule.ModuleTypes.GuardianModuleReinforcement,4,0.41,"Guardian Module Reinforcement Package Class 2 Rating E", Protection:30 ) },
            { "int_guardianmodulereinforcement_size2_class2", new ShipModule(128833958,ShipModule.ModuleTypes.GuardianModuleReinforcement,2,0.47,"Guardian Module Reinforcement Package Class 2 Rating D", Protection:60 ) },
            { "int_guardianmodulereinforcement_size3_class1", new ShipModule(128833959,ShipModule.ModuleTypes.GuardianModuleReinforcement,8,0.54,"Guardian Module Reinforcement Package Class 3 Rating E", Protection:30 ) },
            { "int_guardianmodulereinforcement_size3_class2", new ShipModule(128833960,ShipModule.ModuleTypes.GuardianModuleReinforcement,4,0.61,"Guardian Module Reinforcement Package Class 3 Rating D", Protection:60 ) },
            { "int_guardianmodulereinforcement_size4_class1", new ShipModule(128833961,ShipModule.ModuleTypes.GuardianModuleReinforcement,16,0.68,"Guardian Module Reinforcement Package Class 4 Rating E", Protection:30 ) },
            { "int_guardianmodulereinforcement_size4_class2", new ShipModule(128833962,ShipModule.ModuleTypes.GuardianModuleReinforcement,8,0.74,"Guardian Module Reinforcement Package Class 4 Rating D", Protection:60 ) },
            { "int_guardianmodulereinforcement_size5_class1", new ShipModule(128833963,ShipModule.ModuleTypes.GuardianModuleReinforcement,32,0.81,"Guardian Module Reinforcement Package Class 5 Rating E", Protection:30 ) },
            { "int_guardianmodulereinforcement_size5_class2", new ShipModule(128833964,ShipModule.ModuleTypes.GuardianModuleReinforcement,16,0.88,"Guardian Module Reinforcement Package Class 5 Rating D", Protection:60 ) },

            { "int_guardianshieldreinforcement_size1_class1", new ShipModule(128833965,ShipModule.ModuleTypes.GuardianShieldReinforcement,2,0.35,"Guardian Shield Reinforcement Package Class 1 Rating E", ShieldReinforcement:44 ) },
            { "int_guardianshieldreinforcement_size1_class2", new ShipModule(128833966,ShipModule.ModuleTypes.GuardianShieldReinforcement,1,0.46,"Guardian Shield Reinforcement Package Class 1 Rating D", ShieldReinforcement:61 ) },
            { "int_guardianshieldreinforcement_size2_class1", new ShipModule(128833967,ShipModule.ModuleTypes.GuardianShieldReinforcement,4,0.56,"Guardian Shield Reinforcement Package Class 2 Rating E", ShieldReinforcement:83 ) },
            { "int_guardianshieldreinforcement_size2_class2", new ShipModule(128833968,ShipModule.ModuleTypes.GuardianShieldReinforcement,2,0.67,"Guardian Shield Reinforcement Package Class 2 Rating D", ShieldReinforcement:105 ) },
            { "int_guardianshieldreinforcement_size3_class1", new ShipModule(128833969,ShipModule.ModuleTypes.GuardianShieldReinforcement,8,0.74,"Guardian Shield Reinforcement Package Class 3 Rating E", ShieldReinforcement:127 ) },
            { "int_guardianshieldreinforcement_size3_class2", new ShipModule(128833970,ShipModule.ModuleTypes.GuardianShieldReinforcement,4,0.84,"Guardian Shield Reinforcement Package Class 3 Rating D", ShieldReinforcement:143 ) },
            { "int_guardianshieldreinforcement_size4_class1", new ShipModule(128833971,ShipModule.ModuleTypes.GuardianShieldReinforcement,16,0.95,"Guardian Shield Reinforcement Package Class 4 Rating E", ShieldReinforcement:165 ) },
            { "int_guardianshieldreinforcement_size4_class2", new ShipModule(128833972,ShipModule.ModuleTypes.GuardianShieldReinforcement,8,1.05,"Guardian Shield Reinforcement Package Class 4 Rating D", ShieldReinforcement:182 ) },
            { "int_guardianshieldreinforcement_size5_class1", new ShipModule(128833973,ShipModule.ModuleTypes.GuardianShieldReinforcement,32,1.16,"Guardian Shield Reinforcement Package Class 5 Rating E", ShieldReinforcement:198 ) },
            { "int_guardianshieldreinforcement_size5_class2", new ShipModule(128833974,ShipModule.ModuleTypes.GuardianShieldReinforcement,16,1.26,"Guardian Shield Reinforcement Package Class 5 Rating D", ShieldReinforcement:215 ) },

            { "int_guardianfsdbooster_size1", new ShipModule(128833975,ShipModule.ModuleTypes.GuardianFSDBooster,1.3,0.75,"Guardian Frame Shift Drive Booster Class 1", AdditionalRange:4 ) },
            { "int_guardianfsdbooster_size2", new ShipModule(128833976,ShipModule.ModuleTypes.GuardianFSDBooster,1.3,0.98,"Guardian Frame Shift Drive Booster Class 2", AdditionalRange:6 ) },
            { "int_guardianfsdbooster_size3", new ShipModule(128833977,ShipModule.ModuleTypes.GuardianFSDBooster,1.3,1.27,"Guardian Frame Shift Drive Booster Class 3", AdditionalRange:7.75 ) },
            { "int_guardianfsdbooster_size4", new ShipModule(128833978,ShipModule.ModuleTypes.GuardianFSDBooster,1.3,1.65,"Guardian Frame Shift Drive Booster Class 4", AdditionalRange:9.25 ) },
            { "int_guardianfsdbooster_size5", new ShipModule(128833979,ShipModule.ModuleTypes.GuardianFSDBooster,1.3,2.14,"Guardian Frame Shift Drive Booster Class 5", AdditionalRange:10.5 ) },

            { "int_guardianpowerdistributor_size1", new ShipModule(128833980,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,1.4,0.62,"Guardian Hybrid Power Distributor Class 1", SysMW:0.8, EngMW:0.8, WepMW:2.5, SysCap:10, EngCap:9, WepCap:10 ) },
            { "int_guardianpowerdistributor_size2", new ShipModule(128833981,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,2.6,0.73,"Guardian Hybrid Power Distributor Class 2", SysMW:1, EngMW:1, WepMW:3.1, SysCap:11, EngCap:11, WepCap:13 ) },
            { "int_guardianpowerdistributor_size3", new ShipModule(128833982,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,5.25,0.78,"Guardian Hybrid Power Distributor Class 3", SysMW:1.7, EngMW:1.7, WepMW:3.9, SysCap:14, EngCap:14, WepCap:17 ) },
            { "int_guardianpowerdistributor_size4", new ShipModule(128833983,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,10.5,0.87,"Guardian Hybrid Power Distributor Class 4", SysMW:2.5, EngMW:2.5, WepMW:4.9, SysCap:17, EngCap:17, WepCap:22 ) },
            { "int_guardianpowerdistributor_size5", new ShipModule(128833984,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,21,0.96,"Guardian Hybrid Power Distributor Class 5", SysMW:3.3, EngMW:3.3, WepMW:6, SysCap:22, EngCap:22, WepCap:29 ) },
            { "int_guardianpowerdistributor_size6", new ShipModule(128833985,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,42,1.07,"Guardian Hybrid Power Distributor Class 6", SysMW:4.2, EngMW:4.2, WepMW:7.3, SysCap:26, EngCap:26, WepCap:35 ) },
            { "int_guardianpowerdistributor_size7", new ShipModule(128833986,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,84,1.16,"Guardian Hybrid Power Distributor Class 7", SysMW:5.2, EngMW:5.2, WepMW:8.5, SysCap:31, EngCap:31, WepCap:43 ) },
            { "int_guardianpowerdistributor_size8", new ShipModule(128833987,ShipModule.ModuleTypes.GuardianHybridPowerDistributor,168,1.25,"Guardian Hybrid Power Distributor Class 8", SysMW:6.2, EngMW:6.2, WepMW:10.1, SysCap:36, EngCap:36, WepCap:50 ) },

            { "int_guardianpowerplant_size2", new ShipModule(128833988,ShipModule.ModuleTypes.GuardianHybridPowerPlant,1.5,0,"Guardian Hybrid Power Plant Class 2", PowerGen:12.7, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size3", new ShipModule(128833989,ShipModule.ModuleTypes.GuardianHybridPowerPlant,2.9,0,"Guardian Hybrid Power Plant Class 3", PowerGen:15.8, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size4", new ShipModule(128833990,ShipModule.ModuleTypes.GuardianHybridPowerPlant,5.9,0,"Guardian Hybrid Power Plant Class 4", PowerGen:20.6, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size5", new ShipModule(128833991,ShipModule.ModuleTypes.GuardianHybridPowerPlant,11.7,0,"Guardian Hybrid Power Plant Class 5", PowerGen:26.9, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size6", new ShipModule(128833992,ShipModule.ModuleTypes.GuardianHybridPowerPlant,23.4,0,"Guardian Hybrid Power Plant Class 6", PowerGen:33.3, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size7", new ShipModule(128833993,ShipModule.ModuleTypes.GuardianHybridPowerPlant,46.8,0,"Guardian Hybrid Power Plant Class 7", PowerGen:39.6, HeatEfficiency:0.5 ) },
            { "int_guardianpowerplant_size8", new ShipModule(128833994,ShipModule.ModuleTypes.GuardianHybridPowerPlant,93.6,0,"Guardian Hybrid Power Plant Class 8", PowerGen:47.5, HeatEfficiency:0.5 ) },

            // Hull Reinforcement Packages 

            { "int_hullreinforcement_size1_class1", new ShipModule(128668537,ShipModule.ModuleTypes.HullReinforcementPackage,2,0,"Hull Reinforcement Package Class 1 Rating E", Explosive:0.5, Kinetic:0.5, Thermal:0.5, HullReinforcement:80 ) },
            { "int_hullreinforcement_size1_class2", new ShipModule(128668538,ShipModule.ModuleTypes.HullReinforcementPackage,1,0,"Hull Reinforcement Package Class 1 Rating D", Explosive:0.5, Kinetic:0.5, Thermal:0.5, HullReinforcement:110 ) },
            { "int_hullreinforcement_size2_class1", new ShipModule(128668539,ShipModule.ModuleTypes.HullReinforcementPackage,4,0,"Hull Reinforcement Package Class 2 Rating E", Explosive:1, Kinetic:1, Thermal:1, HullReinforcement:150 ) },
            { "int_hullreinforcement_size2_class2", new ShipModule(128668540,ShipModule.ModuleTypes.HullReinforcementPackage,2,0,"Hull Reinforcement Package Class 2 Rating D", Explosive:1, Kinetic:1, Thermal:1, HullReinforcement:190 ) },
            { "int_hullreinforcement_size3_class1", new ShipModule(128668541,ShipModule.ModuleTypes.HullReinforcementPackage,8,0,"Hull Reinforcement Package Class 3 Rating E", Explosive:1.5, Kinetic:1.5, Thermal:1.5, HullReinforcement:230 ) },
            { "int_hullreinforcement_size3_class2", new ShipModule(128668542,ShipModule.ModuleTypes.HullReinforcementPackage,4,0,"Hull Reinforcement Package Class 3 Rating D", Explosive:1.5, Kinetic:1.5, Thermal:1.5, HullReinforcement:260 ) },
            { "int_hullreinforcement_size4_class1", new ShipModule(128668543,ShipModule.ModuleTypes.HullReinforcementPackage,16,0,"Hull Reinforcement Package Class 4 Rating E", Explosive:2, Kinetic:2, Thermal:2, HullReinforcement:300 ) },
            { "int_hullreinforcement_size4_class2", new ShipModule(128668544,ShipModule.ModuleTypes.HullReinforcementPackage,8,0,"Hull Reinforcement Package Class 4 Rating D", Explosive:2, Kinetic:2, Thermal:2, HullReinforcement:330 ) },
            { "int_hullreinforcement_size5_class1", new ShipModule(128668545,ShipModule.ModuleTypes.HullReinforcementPackage,32,0,"Hull Reinforcement Package Class 5 Rating E", Explosive:2.5, Kinetic:2.5, Thermal:2.5, HullReinforcement:360 ) },
            { "int_hullreinforcement_size5_class2", new ShipModule(128668546,ShipModule.ModuleTypes.HullReinforcementPackage,16,0,"Hull Reinforcement Package Class 5 Rating D", Explosive:2.5, Kinetic:2.5, Thermal:2.5, HullReinforcement:390 ) },

            // Frame ship drive

            { "int_hyperdrive_size2_class1", new ShipModule(128064103,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.16,"Frame Shift Drive Class 2 Rating E", OptMass:48, PowerConstant:2, LinearConstant:11, MaxFuelPerJump:0.6, ThermL:10 ) },
            { "int_hyperdrive_size2_class2", new ShipModule(128064104,ShipModule.ModuleTypes.FrameShiftDrive,1,0.18,"Frame Shift Drive Class 2 Rating D", OptMass:54, PowerConstant:2, LinearConstant:10, MaxFuelPerJump:0.6, ThermL:10 ) },
            { "int_hyperdrive_size2_class3", new ShipModule(128064105,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.2,"Frame Shift Drive Class 2 Rating C", OptMass:60, PowerConstant:2, LinearConstant:8, MaxFuelPerJump:0.6, ThermL:10 ) },
            { "int_hyperdrive_size2_class4", new ShipModule(128064106,ShipModule.ModuleTypes.FrameShiftDrive,4,0.25,"Frame Shift Drive Class 2 Rating B", OptMass:75, PowerConstant:2, LinearConstant:10, MaxFuelPerJump:0.8, ThermL:10 ) },
            { "int_hyperdrive_size2_class5", new ShipModule(128064107,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.3,"Frame Shift Drive Class 2 Rating A", OptMass:90, PowerConstant:2, LinearConstant:12, MaxFuelPerJump:0.9, ThermL:10 ) },
            { "int_hyperdrive_size3_class1", new ShipModule(128064108,ShipModule.ModuleTypes.FrameShiftDrive,5,0.24,"Frame Shift Drive Class 3 Rating E", OptMass:80, PowerConstant:2.15, LinearConstant:11, MaxFuelPerJump:1.2, ThermL:14 ) },
            { "int_hyperdrive_size3_class2", new ShipModule(128064109,ShipModule.ModuleTypes.FrameShiftDrive,2,0.27,"Frame Shift Drive Class 3 Rating D", OptMass:90, PowerConstant:2.15, LinearConstant:10, MaxFuelPerJump:1.2, ThermL:14 ) },
            { "int_hyperdrive_size3_class3", new ShipModule(128064110,ShipModule.ModuleTypes.FrameShiftDrive,5,0.3,"Frame Shift Drive Class 3 Rating C", OptMass:100, PowerConstant:2.15, LinearConstant:8, MaxFuelPerJump:1.2, ThermL:14 ) },
            { "int_hyperdrive_size3_class4", new ShipModule(128064111,ShipModule.ModuleTypes.FrameShiftDrive,8,0.38,"Frame Shift Drive Class 3 Rating B", OptMass:125, PowerConstant:2.15, LinearConstant:10, MaxFuelPerJump:1.5, ThermL:14 ) },
            { "int_hyperdrive_size3_class5", new ShipModule(128064112,ShipModule.ModuleTypes.FrameShiftDrive,5,0.45,"Frame Shift Drive Class 3 Rating A", OptMass:150, PowerConstant:2.15, LinearConstant:12, MaxFuelPerJump:1.8, ThermL:14 ) },
            { "int_hyperdrive_size4_class1", new ShipModule(128064113,ShipModule.ModuleTypes.FrameShiftDrive,10,0.24,"Frame Shift Drive Class 4 Rating E", OptMass:280, PowerConstant:2.3, LinearConstant:11, MaxFuelPerJump:2, ThermL:18 ) },
            { "int_hyperdrive_size4_class2", new ShipModule(128064114,ShipModule.ModuleTypes.FrameShiftDrive,4,0.27,"Frame Shift Drive Class 4 Rating D", OptMass:315, PowerConstant:2.3, LinearConstant:10, MaxFuelPerJump:2, ThermL:18 ) },
            { "int_hyperdrive_size4_class3", new ShipModule(128064115,ShipModule.ModuleTypes.FrameShiftDrive,10,0.3,"Frame Shift Drive Class 4 Rating C", OptMass:350, PowerConstant:2.3, LinearConstant:8, MaxFuelPerJump:2, ThermL:18 ) },
            { "int_hyperdrive_size4_class4", new ShipModule(128064116,ShipModule.ModuleTypes.FrameShiftDrive,16,0.38,"Frame Shift Drive Class 4 Rating B", OptMass:438, PowerConstant:2.3, LinearConstant:10, MaxFuelPerJump:2.5, ThermL:18 ) },
            { "int_hyperdrive_size4_class5", new ShipModule(128064117,ShipModule.ModuleTypes.FrameShiftDrive,10,0.45,"Frame Shift Drive Class 4 Rating A", OptMass:525, PowerConstant:2.3, LinearConstant:12, MaxFuelPerJump:3, ThermL:18 ) },
            { "int_hyperdrive_size5_class1", new ShipModule(128064118,ShipModule.ModuleTypes.FrameShiftDrive,20,0.32,"Frame Shift Drive Class 5 Rating E", OptMass:560, PowerConstant:2.45, LinearConstant:11, MaxFuelPerJump:3.3, ThermL:27 ) },
            { "int_hyperdrive_size5_class2", new ShipModule(128064119,ShipModule.ModuleTypes.FrameShiftDrive,8,0.36,"Frame Shift Drive Class 5 Rating D", OptMass:630, PowerConstant:2.45, LinearConstant:10, MaxFuelPerJump:3.3, ThermL:27 ) },
            { "int_hyperdrive_size5_class3", new ShipModule(128064120,ShipModule.ModuleTypes.FrameShiftDrive,20,0.4,"Frame Shift Drive Class 5 Rating C", OptMass:700, PowerConstant:2.45, LinearConstant:8, MaxFuelPerJump:3.3, ThermL:27 ) },
            { "int_hyperdrive_size5_class4", new ShipModule(128064121,ShipModule.ModuleTypes.FrameShiftDrive,32,0.5,"Frame Shift Drive Class 5 Rating B", OptMass:875, PowerConstant:2.45, LinearConstant:10, MaxFuelPerJump:4.1, ThermL:27 ) },
            { "int_hyperdrive_size5_class5", new ShipModule(128064122,ShipModule.ModuleTypes.FrameShiftDrive,20,0.6,"Frame Shift Drive Class 5 Rating A", OptMass:1050, PowerConstant:2.45, LinearConstant:12, MaxFuelPerJump:5, ThermL:27 ) },
            { "int_hyperdrive_size6_class1", new ShipModule(128064123,ShipModule.ModuleTypes.FrameShiftDrive,40,0.4,"Frame Shift Drive Class 6 Rating E", OptMass:960, PowerConstant:2.6, LinearConstant:11, MaxFuelPerJump:5.3, ThermL:37 ) },
            { "int_hyperdrive_size6_class2", new ShipModule(128064124,ShipModule.ModuleTypes.FrameShiftDrive,16,0.45,"Frame Shift Drive Class 6 Rating D", OptMass:1080, PowerConstant:2.6, LinearConstant:10, MaxFuelPerJump:5.3, ThermL:37 ) },
            { "int_hyperdrive_size6_class3", new ShipModule(128064125,ShipModule.ModuleTypes.FrameShiftDrive,40,0.5,"Frame Shift Drive Class 6 Rating C", OptMass:1200, PowerConstant:2.6, LinearConstant:8, MaxFuelPerJump:5.3, ThermL:37 ) },
            { "int_hyperdrive_size6_class4", new ShipModule(128064126,ShipModule.ModuleTypes.FrameShiftDrive,64,0.63,"Frame Shift Drive Class 6 Rating B", OptMass:1500, PowerConstant:2.6, LinearConstant:10, MaxFuelPerJump:6.6, ThermL:37 ) },
            { "int_hyperdrive_size6_class5", new ShipModule(128064127,ShipModule.ModuleTypes.FrameShiftDrive,40,0.75,"Frame Shift Drive Class 6 Rating A", OptMass:1800, PowerConstant:2.6, LinearConstant:12, MaxFuelPerJump:8, ThermL:37 ) },
            { "int_hyperdrive_size7_class1", new ShipModule(128064128,ShipModule.ModuleTypes.FrameShiftDrive,80,0.48,"Frame Shift Drive Class 7 Rating E", OptMass:1440, PowerConstant:2.75, LinearConstant:11, MaxFuelPerJump:8.5, ThermL:43 ) },
            { "int_hyperdrive_size7_class2", new ShipModule(128064129,ShipModule.ModuleTypes.FrameShiftDrive,32,0.54,"Frame Shift Drive Class 7 Rating D", OptMass:1620, PowerConstant:2.75, LinearConstant:10, MaxFuelPerJump:8.5, ThermL:43 ) },
            { "int_hyperdrive_size7_class3", new ShipModule(128064130,ShipModule.ModuleTypes.FrameShiftDrive,80,0.6,"Frame Shift Drive Class 7 Rating C", OptMass:1800, PowerConstant:2.75, LinearConstant:8, MaxFuelPerJump:8.5, ThermL:43 ) },
            { "int_hyperdrive_size7_class4", new ShipModule(128064131,ShipModule.ModuleTypes.FrameShiftDrive,128,0.75,"Frame Shift Drive Class 7 Rating B", OptMass:2250, PowerConstant:2.75, LinearConstant:10, MaxFuelPerJump:10.6, ThermL:43 ) },
            { "int_hyperdrive_size7_class5", new ShipModule(128064132,ShipModule.ModuleTypes.FrameShiftDrive,80,0.9,"Frame Shift Drive Class 7 Rating A", OptMass:2700, PowerConstant:2.75, LinearConstant:12, MaxFuelPerJump:12.8, ThermL:43 ) },

            { "int_hyperdrive_size8_class1", new ShipModule(128064133,ShipModule.ModuleTypes.FrameShiftDrive,160,0.56,"Frame Shift Drive Class 8 Rating E") },
            { "int_hyperdrive_size8_class2", new ShipModule(128064134,ShipModule.ModuleTypes.FrameShiftDrive,64,0.63,"Frame Shift Drive Class 8 Rating D") },
            { "int_hyperdrive_size8_class3", new ShipModule(128064135,ShipModule.ModuleTypes.FrameShiftDrive,160,0.7,"Frame Shift Drive Class 8 Rating C" ) },
            { "int_hyperdrive_size8_class4", new ShipModule(128064136,ShipModule.ModuleTypes.FrameShiftDrive,256,0.88,"Frame Shift Drive Class 8 Rating B" ) },
            { "int_hyperdrive_size8_class5", new ShipModule(128064137,ShipModule.ModuleTypes.FrameShiftDrive,160,1.05,"Frame Shift Drive Class 8 Rating A" ) },
            
            { "int_hyperdrive_size2_class1_free", new ShipModule(128666637,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.16,"Frame Shift Drive Class 2 Rating E",OptMass:48, PowerConstant:2, LinearConstant:11, MaxFuelPerJump:0.6, ThermL:10 ) },


            { "int_hyperdrive_overcharge_size2_class1", new ShipModule(129030577,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.2,"Frame Shift Drive (SCO) Class 2 Rating E", OptMass:60, PowerConstant:2, LinearConstant:8, MaxFuelPerJump:0.6, ThermL:10, SCOSpeedIncrease:25, SCOAccelerationRate:0.08, SCOHeatGenerationRate:42.07, SCOControlInterference:0.25 ) },
            { "int_hyperdrive_overcharge_size2_class2", new ShipModule(129030578,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.25,"Frame Shift Drive (SCO) Class 2 Rating D", OptMass:90, PowerConstant:2, LinearConstant:12, MaxFuelPerJump:0.9, ThermL:10, SCOSpeedIncrease:142, SCOAccelerationRate:0.09, SCOHeatGenerationRate:38, SCOControlInterference:0.24 ) },
            { "int_hyperdrive_overcharge_size2_class3", new ShipModule(129030487,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.25,"Frame Shift Drive (SCO) Class 2 Rating C", OptMass:90, PowerConstant:2, LinearConstant:12, MaxFuelPerJump:0.9, ThermL:10, SCOSpeedIncrease:142, SCOAccelerationRate:0.09, SCOHeatGenerationRate:27.14, SCOControlInterference:0.24 ) },
            { "int_hyperdrive_overcharge_size2_class4", new ShipModule(129030579,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.25,"Frame Shift Drive (SCO) Class 2 Rating B", OptMass:90, PowerConstant:2, LinearConstant:12, MaxFuelPerJump:0.9, ThermL:10, SCOSpeedIncrease:142, SCOAccelerationRate:0.09, SCOHeatGenerationRate:38, SCOControlInterference:0.24 ) },
            { "int_hyperdrive_overcharge_size2_class5", new ShipModule(129030580,ShipModule.ModuleTypes.FrameShiftDrive,2.5,0.3,"Frame Shift Drive (SCO) Class 2 Rating A", OptMass:100, PowerConstant:2, LinearConstant:13, MaxFuelPerJump:1, ThermL:10, SCOSpeedIncrease:160, SCOAccelerationRate:0.09, SCOHeatGenerationRate:36.1, SCOControlInterference:0.23 ) },

            { "int_hyperdrive_overcharge_size3_class1", new ShipModule(129030581,ShipModule.ModuleTypes.FrameShiftDrive,5,0.3,"Frame Shift Drive (SCO) Class 3 Rating E", OptMass:100, PowerConstant:2.15, LinearConstant:8, MaxFuelPerJump:1.2, ThermL:14, SCOSpeedIncrease:20, SCOAccelerationRate:0.06, SCOHeatGenerationRate:58.38, SCOControlInterference:0.3 ) },
            { "int_hyperdrive_overcharge_size3_class2", new ShipModule(129030582,ShipModule.ModuleTypes.FrameShiftDrive,2,0.38,"Frame Shift Drive (SCO) Class 3 Rating D", OptMass:150, PowerConstant:2.15, LinearConstant:12, MaxFuelPerJump:1.8, ThermL:14, SCOSpeedIncrease:120, SCOAccelerationRate:0.07, SCOHeatGenerationRate:53, SCOControlInterference:0.29 ) },
            { "int_hyperdrive_overcharge_size3_class4", new ShipModule(129030583,ShipModule.ModuleTypes.FrameShiftDrive,5,0.38,"Frame Shift Drive (SCO) Class 3 Rating B", OptMass:150, PowerConstant:2.15, LinearConstant:12, MaxFuelPerJump:1.8, ThermL:14, SCOSpeedIncrease:120, SCOAccelerationRate:0.07, SCOHeatGenerationRate:53, SCOControlInterference:0.29 ) },
            { "int_hyperdrive_overcharge_size3_class3", new ShipModule(129030486,ShipModule.ModuleTypes.FrameShiftDrive,5,0.38,"Frame Shift Drive (SCO) Class 3 Rating C", OptMass:150, PowerConstant:2.15, LinearConstant:12, MaxFuelPerJump:1.8, ThermL:14, SCOSpeedIncrease:120, SCOAccelerationRate:0.07, SCOHeatGenerationRate:37.86, SCOControlInterference:0.29 ) },
            { "int_hyperdrive_overcharge_size3_class5", new ShipModule(129030584,ShipModule.ModuleTypes.FrameShiftDrive,5,0.45,"Frame Shift Drive (SCO) Class 3 Rating A", OptMass:167, PowerConstant:2.15, LinearConstant:13, MaxFuelPerJump:1.9, ThermL:14, SCOSpeedIncrease:138, SCOAccelerationRate:0.07, SCOHeatGenerationRate:50.35, SCOControlInterference:0.28 ) },

            { "int_hyperdrive_overcharge_size4_class1", new ShipModule(129030585,ShipModule.ModuleTypes.FrameShiftDrive,10,0.3,"Frame Shift Drive (SCO) Class 4 Rating E", OptMass:350, PowerConstant:2.3, LinearConstant:8, MaxFuelPerJump:2, ThermL:18, SCOSpeedIncrease:15, SCOAccelerationRate:0.05, SCOHeatGenerationRate:66.43, SCOControlInterference:0.37 ) },
            { "int_hyperdrive_overcharge_size4_class2", new ShipModule(129030586,ShipModule.ModuleTypes.FrameShiftDrive,4,0.38,"Frame Shift Drive (SCO) Class 4 Rating D", OptMass:525, PowerConstant:2.3, LinearConstant:12, MaxFuelPerJump:3, ThermL:18, SCOSpeedIncrease:100, SCOAccelerationRate:0.06, SCOHeatGenerationRate:60, SCOControlInterference:0.35 ) },
            { "int_hyperdrive_overcharge_size4_class3", new ShipModule(129030485,ShipModule.ModuleTypes.FrameShiftDrive,10,0.38,"Frame Shift Drive (SCO) Class 4 Rating C", OptMass:525, PowerConstant:2.3, LinearConstant:12, MaxFuelPerJump:3, ThermL:18, SCOSpeedIncrease:100, SCOAccelerationRate:0.06, SCOHeatGenerationRate:42.86, SCOControlInterference:0.35 ) },
            { "int_hyperdrive_overcharge_size4_class4", new ShipModule(129030587,ShipModule.ModuleTypes.FrameShiftDrive,10,0.38,"Frame Shift Drive (SCO) Class 4 Rating B", OptMass:525, PowerConstant:2.3, LinearConstant:12, MaxFuelPerJump:3, ThermL:18, SCOSpeedIncrease:100, SCOAccelerationRate:0.06, SCOHeatGenerationRate:60, SCOControlInterference:0.35 ) },
            { "int_hyperdrive_overcharge_size4_class5", new ShipModule(129030588,ShipModule.ModuleTypes.FrameShiftDrive,10,0.45,"Frame Shift Drive (SCO) Class 4 Rating A", OptMass:585, PowerConstant:2.3, LinearConstant:13, MaxFuelPerJump:3.2, ThermL:18, SCOSpeedIncrease:107, SCOAccelerationRate:0.06, SCOHeatGenerationRate:57, SCOControlInterference:0.34 ) },

            { "int_hyperdrive_overcharge_size5_class1", new ShipModule(129030589,ShipModule.ModuleTypes.FrameShiftDrive,20,0.45,"Frame Shift Drive (SCO) Class 5 Rating E", OptMass:700, PowerConstant:2.45, LinearConstant:8, MaxFuelPerJump:3.3, ThermL:27, SCOSpeedIncrease:0, SCOAccelerationRate:0.04, SCOHeatGenerationRate:108.5, SCOControlInterference:0.42 ) },
            { "int_hyperdrive_overcharge_size5_class2", new ShipModule(129030590,ShipModule.ModuleTypes.FrameShiftDrive,8,0.5,"Frame Shift Drive (SCO) Class 5 Rating D", OptMass:1050, PowerConstant:2.45, LinearConstant:12, MaxFuelPerJump:5, ThermL:27, SCOSpeedIncrease:80, SCOAccelerationRate:0.055, SCOHeatGenerationRate:98, SCOControlInterference:0.4 ) },
            { "int_hyperdrive_overcharge_size5_class3", new ShipModule(129030474,ShipModule.ModuleTypes.FrameShiftDrive,20,0.5,"Frame Shift Drive (SCO) Class 5 Rating C", OptMass:1050, PowerConstant:2.45, LinearConstant:12, MaxFuelPerJump:5, ThermL:27, SCOSpeedIncrease:80, SCOAccelerationRate:0.055, SCOHeatGenerationRate:70, SCOControlInterference:0.4 ) },
            { "int_hyperdrive_overcharge_size5_class4", new ShipModule(129030591,ShipModule.ModuleTypes.FrameShiftDrive,20,0.5,"Frame Shift Drive (SCO) Class 5 Rating B", OptMass:1050, PowerConstant:2.45, LinearConstant:12, MaxFuelPerJump:5, ThermL:27, SCOSpeedIncrease:80, SCOAccelerationRate:0.055, SCOHeatGenerationRate:98, SCOControlInterference:0.4 ) },
            { "int_hyperdrive_overcharge_size5_class5", new ShipModule(129030592,ShipModule.ModuleTypes.FrameShiftDrive,20,0.6,"Frame Shift Drive (SCO) Class 5 Rating A", OptMass:1175, PowerConstant:2.45, LinearConstant:13, MaxFuelPerJump:5.2, ThermL:27, SCOSpeedIncrease:95, SCOAccelerationRate:0.055, SCOHeatGenerationRate:93.1, SCOControlInterference:0.39 ) },

            { "int_hyperdrive_overcharge_size6_class1", new ShipModule(129030593,ShipModule.ModuleTypes.FrameShiftDrive,40,0.5,"Frame Shift Drive (SCO) Class 6 Rating E", OptMass:1200, PowerConstant:2.6, LinearConstant:8, MaxFuelPerJump:5.3, ThermL:37, SCOSpeedIncrease:0, SCOAccelerationRate:0.045, SCOHeatGenerationRate:139.5, SCOControlInterference:0.67 ) },
            { "int_hyperdrive_overcharge_size6_class2", new ShipModule(129030594,ShipModule.ModuleTypes.FrameShiftDrive,16,0.63,"Frame Shift Drive (SCO) Class 6 Rating D", OptMass:1800, PowerConstant:2.6, LinearConstant:12, MaxFuelPerJump:8, ThermL:37, SCOSpeedIncrease:62, SCOAccelerationRate:0.05, SCOHeatGenerationRate:126, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size6_class3", new ShipModule(129030484,ShipModule.ModuleTypes.FrameShiftDrive,40,0.63,"Frame Shift Drive (SCO) Class 6 Rating C", OptMass:1800, PowerConstant:2.6, LinearConstant:12, MaxFuelPerJump:8, ThermL:37, SCOSpeedIncrease:62, SCOAccelerationRate:0.05, SCOHeatGenerationRate:90, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size6_class4", new ShipModule(129030595,ShipModule.ModuleTypes.FrameShiftDrive,40,0.63,"Frame Shift Drive (SCO) Class 6 Rating B", OptMass:1800, PowerConstant:2.6, LinearConstant:12, MaxFuelPerJump:8, ThermL:37, SCOSpeedIncrease:62, SCOAccelerationRate:0.05, SCOHeatGenerationRate:126, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size6_class5", new ShipModule(129030596,ShipModule.ModuleTypes.FrameShiftDrive,40,0.75,"Frame Shift Drive (SCO) Class 6 Rating A", OptMass:2000, PowerConstant:2.6, LinearConstant:13, MaxFuelPerJump:8.3, ThermL:37, SCOSpeedIncrease:76, SCOAccelerationRate:0.05, SCOHeatGenerationRate:119.7, SCOControlInterference:0.62 ) },

            { "int_hyperdrive_overcharge_size7_class1", new ShipModule(129030597,ShipModule.ModuleTypes.FrameShiftDrive,80,0.6,"Frame Shift Drive (SCO) Class 7 Rating E", OptMass:1800, PowerConstant:2.75, LinearConstant:8, MaxFuelPerJump:8.5, ThermL:43, SCOSpeedIncrease:0, SCOAccelerationRate:0.03, SCOHeatGenerationRate:143.93, SCOControlInterference:0.67 ) },
            { "int_hyperdrive_overcharge_size7_class2", new ShipModule(129030598,ShipModule.ModuleTypes.FrameShiftDrive,32,0.75,"Frame Shift Drive (SCO) Class 7 Rating D", OptMass:2700, PowerConstant:2.75, LinearConstant:12, MaxFuelPerJump:12.8, ThermL:43, SCOSpeedIncrease:46, SCOAccelerationRate:0.04, SCOHeatGenerationRate:130, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size7_class3", new ShipModule(129030483,ShipModule.ModuleTypes.FrameShiftDrive,80,0.75,"Frame Shift Drive (SCO) Class 7 Rating C", OptMass:2700, PowerConstant:2.75, LinearConstant:12, MaxFuelPerJump:12.8, ThermL:43, SCOSpeedIncrease:46, SCOAccelerationRate:0.04, SCOHeatGenerationRate:92.86, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size7_class4", new ShipModule(129030599,ShipModule.ModuleTypes.FrameShiftDrive,80,0.75,"Frame Shift Drive (SCO) Class 7 Rating B", OptMass:2700, PowerConstant:2.75, LinearConstant:12, MaxFuelPerJump:12.8, ThermL:43, SCOSpeedIncrease:46, SCOAccelerationRate:0.04, SCOHeatGenerationRate:130, SCOControlInterference:0.64 ) },
            { "int_hyperdrive_overcharge_size7_class5", new ShipModule(129030600,ShipModule.ModuleTypes.FrameShiftDrive,80,0.9,"Frame Shift Drive (SCO) Class 7 Rating A", OptMass:3000, PowerConstant:2.75, LinearConstant:13, MaxFuelPerJump:13.1, ThermL:43, SCOSpeedIncrease:58, SCOAccelerationRate:0.04, SCOHeatGenerationRate:123.5, SCOControlInterference:0.62 ) },


            // wake scanner

            { "hpt_cloudscanner_size0_class1", new ShipModule(128662525,ShipModule.ModuleTypes.FrameShiftWakeScanner,1.3,0.2,"Frame Shift Wake Scanner Rating E", FacingLimit:15, Range:2000, Time:10 ) },
            { "hpt_cloudscanner_size0_class2", new ShipModule(128662526,ShipModule.ModuleTypes.FrameShiftWakeScanner,1.3,0.4,"Frame Shift Wake Scanner Rating D", FacingLimit:15, Range:2500, Time:10 ) },
            { "hpt_cloudscanner_size0_class3", new ShipModule(128662527,ShipModule.ModuleTypes.FrameShiftWakeScanner,1.3,0.8,"Frame Shift Wake Scanner Rating C", FacingLimit:15, Range:3000, Time:10 ) },
            { "hpt_cloudscanner_size0_class4", new ShipModule(128662528,ShipModule.ModuleTypes.FrameShiftWakeScanner,1.3,1.6,"Frame Shift Wake Scanner Rating B", FacingLimit:15, Range:3500, Time:10 ) },
            { "hpt_cloudscanner_size0_class5", new ShipModule(128662529,ShipModule.ModuleTypes.FrameShiftWakeScanner,1.3,3.2,"Frame Shift Wake Scanner Rating A", FacingLimit:15, Range:4000, Time:10 ) },

            // life support

            { "int_lifesupport_size1_class1", new ShipModule(128064138,ShipModule.ModuleTypes.LifeSupport,1.3,0.32,"Life Support Class 1 Rating E", Time:300 ) },
            { "int_lifesupport_size1_class2", new ShipModule(128064139,ShipModule.ModuleTypes.LifeSupport,0.5,0.36,"Life Support Class 1 Rating D", Time:450 ) },
            { "int_lifesupport_size1_class3", new ShipModule(128064140,ShipModule.ModuleTypes.LifeSupport,1.3,0.4,"Life Support Class 1 Rating C", Time:600 ) },
            { "int_lifesupport_size1_class4", new ShipModule(128064141,ShipModule.ModuleTypes.LifeSupport,2,0.44,"Life Support Class 1 Rating B", Time:900 ) },
            { "int_lifesupport_size1_class5", new ShipModule(128064142,ShipModule.ModuleTypes.LifeSupport,1.3,0.48,"Life Support Class 1 Rating A", Time:1500 ) },
            { "int_lifesupport_size2_class1", new ShipModule(128064143,ShipModule.ModuleTypes.LifeSupport,2.5,0.37,"Life Support Class 2 Rating E", Time:300 ) },
            { "int_lifesupport_size2_class2", new ShipModule(128064144,ShipModule.ModuleTypes.LifeSupport,1,0.41,"Life Support Class 2 Rating D", Time:450 ) },
            { "int_lifesupport_size2_class3", new ShipModule(128064145,ShipModule.ModuleTypes.LifeSupport,2.5,0.46,"Life Support Class 2 Rating C", Time:600 ) },
            { "int_lifesupport_size2_class4", new ShipModule(128064146,ShipModule.ModuleTypes.LifeSupport,4,0.51,"Life Support Class 2 Rating B", Time:900 ) },
            { "int_lifesupport_size2_class5", new ShipModule(128064147,ShipModule.ModuleTypes.LifeSupport,2.5,0.55,"Life Support Class 2 Rating A", Time:1500 ) },
            { "int_lifesupport_size3_class1", new ShipModule(128064148,ShipModule.ModuleTypes.LifeSupport,5,0.42,"Life Support Class 3 Rating E", Time:300 ) },
            { "int_lifesupport_size3_class2", new ShipModule(128064149,ShipModule.ModuleTypes.LifeSupport,2,0.48,"Life Support Class 3 Rating D", Time:450 ) },
            { "int_lifesupport_size3_class3", new ShipModule(128064150,ShipModule.ModuleTypes.LifeSupport,5,0.53,"Life Support Class 3 Rating C", Time:600 ) },
            { "int_lifesupport_size3_class4", new ShipModule(128064151,ShipModule.ModuleTypes.LifeSupport,8,0.58,"Life Support Class 3 Rating B", Time:900 ) },
            { "int_lifesupport_size3_class5", new ShipModule(128064152,ShipModule.ModuleTypes.LifeSupport,5,0.64,"Life Support Class 3 Rating A", Time:1500 ) },
            { "int_lifesupport_size4_class1", new ShipModule(128064153,ShipModule.ModuleTypes.LifeSupport,10,0.5,"Life Support Class 4 Rating E", Time:300 ) },
            { "int_lifesupport_size4_class2", new ShipModule(128064154,ShipModule.ModuleTypes.LifeSupport,4,0.56,"Life Support Class 4 Rating D", Time:450 ) },
            { "int_lifesupport_size4_class3", new ShipModule(128064155,ShipModule.ModuleTypes.LifeSupport,10,0.62,"Life Support Class 4 Rating C", Time:600 ) },
            { "int_lifesupport_size4_class4", new ShipModule(128064156,ShipModule.ModuleTypes.LifeSupport,16,0.68,"Life Support Class 4 Rating B", Time:900 ) },
            { "int_lifesupport_size4_class5", new ShipModule(128064157,ShipModule.ModuleTypes.LifeSupport,10,0.74,"Life Support Class 4 Rating A", Time:1500 ) },
            { "int_lifesupport_size5_class1", new ShipModule(128064158,ShipModule.ModuleTypes.LifeSupport,20,0.57,"Life Support Class 5 Rating E", Time:300 ) },
            { "int_lifesupport_size5_class2", new ShipModule(128064159,ShipModule.ModuleTypes.LifeSupport,8,0.64,"Life Support Class 5 Rating D", Time:450 ) },
            { "int_lifesupport_size5_class3", new ShipModule(128064160,ShipModule.ModuleTypes.LifeSupport,20,0.71,"Life Support Class 5 Rating C", Time:600 ) },
            { "int_lifesupport_size5_class4", new ShipModule(128064161,ShipModule.ModuleTypes.LifeSupport,32,0.78,"Life Support Class 5 Rating B", Time:900 ) },
            { "int_lifesupport_size5_class5", new ShipModule(128064162,ShipModule.ModuleTypes.LifeSupport,20,0.85,"Life Support Class 5 Rating A", Time:1500 ) },
            { "int_lifesupport_size6_class1", new ShipModule(128064163,ShipModule.ModuleTypes.LifeSupport,40,0.64,"Life Support Class 6 Rating E", Time:300 ) },
            { "int_lifesupport_size6_class2", new ShipModule(128064164,ShipModule.ModuleTypes.LifeSupport,16,0.72,"Life Support Class 6 Rating D", Time:450 ) },
            { "int_lifesupport_size6_class3", new ShipModule(128064165,ShipModule.ModuleTypes.LifeSupport,40,0.8,"Life Support Class 6 Rating C", Time:600 ) },
            { "int_lifesupport_size6_class4", new ShipModule(128064166,ShipModule.ModuleTypes.LifeSupport,64,0.88,"Life Support Class 6 Rating B", Time:900 ) },
            { "int_lifesupport_size6_class5", new ShipModule(128064167,ShipModule.ModuleTypes.LifeSupport,40,0.96,"Life Support Class 6 Rating A", Time:1500 ) },
            { "int_lifesupport_size7_class1", new ShipModule(128064168,ShipModule.ModuleTypes.LifeSupport,80,0.72,"Life Support Class 7 Rating E", Time:300 ) },
            { "int_lifesupport_size7_class2", new ShipModule(128064169,ShipModule.ModuleTypes.LifeSupport,32,0.81,"Life Support Class 7 Rating D", Time:450 ) },
            { "int_lifesupport_size7_class3", new ShipModule(128064170,ShipModule.ModuleTypes.LifeSupport,80,0.9,"Life Support Class 7 Rating C", Time:600 ) },
            { "int_lifesupport_size7_class4", new ShipModule(128064171,ShipModule.ModuleTypes.LifeSupport,128,0.99,"Life Support Class 7 Rating B", Time:900 ) },
            { "int_lifesupport_size7_class5", new ShipModule(128064172,ShipModule.ModuleTypes.LifeSupport,80,1.08,"Life Support Class 7 Rating A", Time:1500 ) },
            { "int_lifesupport_size8_class1", new ShipModule(128064173,ShipModule.ModuleTypes.LifeSupport,160,0.8,"Life Support Class 8 Rating E", Time:300 ) },
            { "int_lifesupport_size8_class2", new ShipModule(128064174,ShipModule.ModuleTypes.LifeSupport,64,0.9,"Life Support Class 8 Rating D", Time:450 ) },
            { "int_lifesupport_size8_class3", new ShipModule(128064175,ShipModule.ModuleTypes.LifeSupport,160,1,"Life Support Class 8 Rating C", Time:600 ) },
            { "int_lifesupport_size8_class4", new ShipModule(128064176,ShipModule.ModuleTypes.LifeSupport,256,1.1,"Life Support Class 8 Rating B", Time:900 ) },
            { "int_lifesupport_size8_class5", new ShipModule(128064177,ShipModule.ModuleTypes.LifeSupport,160,1.2,"Life Support Class 8 Rating A", Time:1500 ) },

            { "int_lifesupport_size1_class1_free", new ShipModule(128666638,ShipModule.ModuleTypes.LifeSupport,1.3,0.32,"Life Support Class 1 Rating E",Time:300 ) },

            // Limpet control

            { "int_dronecontrol_collection_size1_class1", new ShipModule(128671229,ShipModule.ModuleTypes.CollectorLimpetController,0.5,0.14,"Collector Limpet Controller Class 1 Rating E", Limpets:1, Speed:200, Range:800, Time:300 ) },
            { "int_dronecontrol_collection_size1_class2", new ShipModule(128671230,ShipModule.ModuleTypes.CollectorLimpetController,0.5,0.18,"Collector Limpet Controller Class 1 Rating D", Limpets:1, Speed:200, Range:600, Time:600 ) },
            { "int_dronecontrol_collection_size1_class3", new ShipModule(128671231,ShipModule.ModuleTypes.CollectorLimpetController,1.3,0.23,"Collector Limpet Controller Class 1 Rating C", Limpets:1, Speed:200, Range:1000, Time:510 ) },
            { "int_dronecontrol_collection_size1_class4", new ShipModule(128671232,ShipModule.ModuleTypes.CollectorLimpetController,2,0.28,"Collector Limpet Controller Class 1 Rating B", Limpets:1, Speed:200, Range:1400, Time:420 ) },
            { "int_dronecontrol_collection_size1_class5", new ShipModule(128671233,ShipModule.ModuleTypes.CollectorLimpetController,2,0.32,"Collector Limpet Controller Class 1 Rating A", Limpets:1, Speed:200, Range:1200, Time:720 ) },
            { "int_dronecontrol_collection_size3_class1", new ShipModule(128671234,ShipModule.ModuleTypes.CollectorLimpetController,2,0.2,"Collector Limpet Controller Class 3 Rating E", Limpets:2, Speed:200, Range:880, Time:300 ) },
            { "int_dronecontrol_collection_size3_class2", new ShipModule(128671235,ShipModule.ModuleTypes.CollectorLimpetController,2,0.27,"Collector Limpet Controller Class 3 Rating D", Limpets:2, Speed:200, Range:660, Time:600 ) },
            { "int_dronecontrol_collection_size3_class3", new ShipModule(128671236,ShipModule.ModuleTypes.CollectorLimpetController,5,0.34,"Collector Limpet Controller Class 3 Rating C", Limpets:2, Speed:200, Range:1100, Time:510 ) },
            { "int_dronecontrol_collection_size3_class4", new ShipModule(128671237,ShipModule.ModuleTypes.CollectorLimpetController,8,0.41,"Collector Limpet Controller Class 3 Rating B", Limpets:2, Speed:200, Range:1540, Time:420 ) },
            { "int_dronecontrol_collection_size3_class5", new ShipModule(128671238,ShipModule.ModuleTypes.CollectorLimpetController,8,0.48,"Collector Limpet Controller Class 3 Rating A", Limpets:2, Speed:200, Range:1320, Time:720 ) },
            { "int_dronecontrol_collection_size5_class1", new ShipModule(128671239,ShipModule.ModuleTypes.CollectorLimpetController,8,0.3,"Collector Limpet Controller Class 5 Rating E", Limpets:3, Speed:200, Range:1040, Time:300 ) },
            { "int_dronecontrol_collection_size5_class2", new ShipModule(128671240,ShipModule.ModuleTypes.CollectorLimpetController,8,0.4,"Collector Limpet Controller Class 5 Rating D", Limpets:3, Speed:200, Range:780, Time:600 ) },
            { "int_dronecontrol_collection_size5_class3", new ShipModule(128671241,ShipModule.ModuleTypes.CollectorLimpetController,20,0.5,"Collector Limpet Controller Class 5 Rating C", Limpets:3, Speed:200, Range:1300, Time:510 ) },
            { "int_dronecontrol_collection_size5_class4", new ShipModule(128671242,ShipModule.ModuleTypes.CollectorLimpetController,32,0.6,"Collector Limpet Controller Class 5 Rating B", Limpets:3, Speed:200, Range:1820, Time:420 ) },
            { "int_dronecontrol_collection_size5_class5", new ShipModule(128671243,ShipModule.ModuleTypes.CollectorLimpetController,32,0.7,"Collector Limpet Controller Class 5 Rating A", Limpets:3, Speed:200, Range:1560, Time:720 ) },
            { "int_dronecontrol_collection_size7_class1", new ShipModule(128671244,ShipModule.ModuleTypes.CollectorLimpetController,32,0.41,"Collector Limpet Controller Class 7 Rating E", Limpets:4, Speed:200, Range:1360, Time:300 ) },
            { "int_dronecontrol_collection_size7_class2", new ShipModule(128671245,ShipModule.ModuleTypes.CollectorLimpetController,32,0.55,"Collector Limpet Controller Class 7 Rating D", Limpets:4, Speed:200, Range:1020, Time:600 ) },
            { "int_dronecontrol_collection_size7_class3", new ShipModule(128671246,ShipModule.ModuleTypes.CollectorLimpetController,80,0.69,"Collector Limpet Controller Class 7 Rating C", Limpets:4, Speed:200, Range:1700, Time:510 ) },
            { "int_dronecontrol_collection_size7_class4", new ShipModule(128671247,ShipModule.ModuleTypes.CollectorLimpetController,128,0.83,"Collector Limpet Controller Class 7 Rating B", Limpets:4, Speed:200, Range:2380, Time:420 ) },
            { "int_dronecontrol_collection_size7_class5", new ShipModule(128671248,ShipModule.ModuleTypes.CollectorLimpetController,128,0.97,"Collector Limpet Controller Class 7 Rating A", Limpets:4, Speed:200, Range:2040, Time:720 ) },

            { "int_dronecontrol_fueltransfer_size1_class1", new ShipModule(128671249,ShipModule.ModuleTypes.FuelTransferLimpetController,1.3,0.18,"Fuel Transfer Limpet Controller Class 1 Rating E", Limpets:1, Speed:200, Range:600, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size1_class2", new ShipModule(128671250,ShipModule.ModuleTypes.FuelTransferLimpetController,0.5,0.14,"Fuel Transfer Limpet Controller Class 1 Rating D", Limpets:1, Speed:200, Range:800, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size1_class3", new ShipModule(128671251,ShipModule.ModuleTypes.FuelTransferLimpetController,1.3,0.23,"Fuel Transfer Limpet Controller Class 1 Rating C", Limpets:1, Speed:200, Range:1000, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size1_class4", new ShipModule(128671252,ShipModule.ModuleTypes.FuelTransferLimpetController,2,0.32,"Fuel Transfer Limpet Controller Class 1 Rating B", Limpets:1, Speed:200, Range:1200, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size1_class5", new ShipModule(128671253,ShipModule.ModuleTypes.FuelTransferLimpetController,1.3,0.28,"Fuel Transfer Limpet Controller Class 1 Rating A", Limpets:1, Speed:200, Range:1400, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size3_class1", new ShipModule(128671254,ShipModule.ModuleTypes.FuelTransferLimpetController,5,0.27,"Fuel Transfer Limpet Controller Class 3 Rating E", Limpets:2, Speed:200, Range:660, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size3_class2", new ShipModule(128671255,ShipModule.ModuleTypes.FuelTransferLimpetController,2,0.2,"Fuel Transfer Limpet Controller Class 3 Rating D", Limpets:2, Speed:200, Range:880, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size3_class3", new ShipModule(128671256,ShipModule.ModuleTypes.FuelTransferLimpetController,5,0.34,"Fuel Transfer Limpet Controller Class 3 Rating C", Limpets:2, Speed:200, Range:1100, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size3_class4", new ShipModule(128671257,ShipModule.ModuleTypes.FuelTransferLimpetController,8,0.48,"Fuel Transfer Limpet Controller Class 3 Rating B", Limpets:2, Speed:200, Range:1320, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size3_class5", new ShipModule(128671258,ShipModule.ModuleTypes.FuelTransferLimpetController,5,0.41,"Fuel Transfer Limpet Controller Class 3 Rating A", Limpets:2, Speed:200, Range:1540, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size5_class1", new ShipModule(128671259,ShipModule.ModuleTypes.FuelTransferLimpetController,20,0.4,"Fuel Transfer Limpet Controller Class 5 Rating E", Limpets:4, Speed:200, Range:780, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size5_class2", new ShipModule(128671260,ShipModule.ModuleTypes.FuelTransferLimpetController,8,0.3,"Fuel Transfer Limpet Controller Class 5 Rating D", Limpets:4, Speed:200, Range:1040, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size5_class3", new ShipModule(128671261,ShipModule.ModuleTypes.FuelTransferLimpetController,20,0.5,"Fuel Transfer Limpet Controller Class 5 Rating C", Limpets:4, Speed:200, Range:1300, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size5_class4", new ShipModule(128671262,ShipModule.ModuleTypes.FuelTransferLimpetController,32,0.72,"Fuel Transfer Limpet Controller Class 5 Rating B", Limpets:4, Speed:200, Range:1560, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size5_class5", new ShipModule(128671263,ShipModule.ModuleTypes.FuelTransferLimpetController,20,0.6,"Fuel Transfer Limpet Controller Class 5 Rating A", Limpets:4, Speed:200, Range:1820, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size7_class1", new ShipModule(128671264,ShipModule.ModuleTypes.FuelTransferLimpetController,80,0.55,"Fuel Transfer Limpet Controller Class 7 Rating E", Limpets:8, Speed:200, Range:1020, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size7_class2", new ShipModule(128671265,ShipModule.ModuleTypes.FuelTransferLimpetController,32,0.41,"Fuel Transfer Limpet Controller Class 7 Rating D", Limpets:8, Speed:200, Range:1360, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size7_class3", new ShipModule(128671266,ShipModule.ModuleTypes.FuelTransferLimpetController,80,0.69,"Fuel Transfer Limpet Controller Class 7 Rating C", Limpets:8, Speed:200, Range:1700, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size7_class4", new ShipModule(128671267,ShipModule.ModuleTypes.FuelTransferLimpetController,128,0.97,"Fuel Transfer Limpet Controller Class 7 Rating B", Limpets:8, Speed:200, Range:2040, Time:60 ) },
            { "int_dronecontrol_fueltransfer_size7_class5", new ShipModule(128671268,ShipModule.ModuleTypes.FuelTransferLimpetController,80,0.83,"Fuel Transfer Limpet Controller Class 7 Rating A", Limpets:8, Speed:200, Range:2380, Time:60 ) },

            { "int_dronecontrol_resourcesiphon", new ShipModule(128066402,ShipModule.ModuleTypes.HatchBreakerLimpetController,0,0.4,"Limpet Control", Limpets:2, Speed:200, Range:1000, HackTime:5, MinCargo:1, MaxCargo:2, Time:60 ) },

            { "int_dronecontrol_resourcesiphon_size1_class1", new ShipModule(128066532,ShipModule.ModuleTypes.HatchBreakerLimpetController,1.3,0.12,"Hatch Breaker Limpet Controller Class 1 Rating E", Limpets:2, Speed:500, Range:1600, TargetRange:1500, HackTime:22, MinCargo:1, MaxCargo:6, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size1_class2", new ShipModule(128066533,ShipModule.ModuleTypes.HatchBreakerLimpetController,0.5,0.16,"Hatch Breaker Limpet Controller Class 1 Rating D", Limpets:1, Speed:500, Range:2100, TargetRange:2000, HackTime:19, MinCargo:2, MaxCargo:7, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size1_class3", new ShipModule(128066534,ShipModule.ModuleTypes.HatchBreakerLimpetController,1.3,0.2,"Hatch Breaker Limpet Controller Class 1 Rating C", Limpets:1, Speed:500, Range:2600, TargetRange:2500, HackTime:16, MinCargo:3, MaxCargo:8, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size1_class4", new ShipModule(128066535,ShipModule.ModuleTypes.HatchBreakerLimpetController,2,0.24,"Hatch Breaker Limpet Controller Class 1 Rating B", Limpets:2, Speed:500, Range:3100, TargetRange:3000, HackTime:13, MinCargo:4, MaxCargo:9, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size1_class5", new ShipModule(128066536,ShipModule.ModuleTypes.HatchBreakerLimpetController,1.3,0.28,"Hatch Breaker Limpet Controller Class 1 Rating A", Limpets:1, Speed:500, Range:3600, TargetRange:3500, HackTime:10, MinCargo:5, MaxCargo:10, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size3_class1", new ShipModule(128066537,ShipModule.ModuleTypes.HatchBreakerLimpetController,5,0.18,"Hatch Breaker Limpet Controller Class 3 Rating E", Limpets:4, Speed:500, Range:1720, TargetRange:1620, HackTime:17, MinCargo:1, MaxCargo:6, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size3_class2", new ShipModule(128066538,ShipModule.ModuleTypes.HatchBreakerLimpetController,2,0.24,"Hatch Breaker Limpet Controller Class 3 Rating D", Limpets:3, Speed:500, Range:2260, TargetRange:2160, HackTime:14, MinCargo:2, MaxCargo:7, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size3_class3", new ShipModule(128066539,ShipModule.ModuleTypes.HatchBreakerLimpetController,5,0.3,"Hatch Breaker Limpet Controller Class 3 Rating C", Limpets:3, Speed:500, Range:2800, TargetRange:2700, HackTime:12, MinCargo:3, MaxCargo:8, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size3_class4", new ShipModule(128066540,ShipModule.ModuleTypes.HatchBreakerLimpetController,8,0.36,"Hatch Breaker Limpet Controller Class 3 Rating B", Limpets:4, Speed:500, Range:3340, TargetRange:3240, HackTime:10, MinCargo:4, MaxCargo:9, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size3_class5", new ShipModule(128066541,ShipModule.ModuleTypes.HatchBreakerLimpetController,5,0.42,"Hatch Breaker Limpet Controller Class 3 Rating A", Limpets:3, Speed:500, Range:3870, TargetRange:3780, HackTime:7, MinCargo:5, MaxCargo:10, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size5_class1", new ShipModule(128066542,ShipModule.ModuleTypes.HatchBreakerLimpetController,20,0.3,"Hatch Breaker Limpet Controller Class 5 Rating E", Limpets:9, Speed:500, Range:2080, TargetRange:1980, HackTime:11, MinCargo:1, MaxCargo:6, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size5_class2", new ShipModule(128066543,ShipModule.ModuleTypes.HatchBreakerLimpetController,8,0.4,"Hatch Breaker Limpet Controller Class 5 Rating D", Limpets:6, Speed:500, Range:2740, TargetRange:2640, HackTime:10, MinCargo:2, MaxCargo:7, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size5_class3", new ShipModule(128066544,ShipModule.ModuleTypes.HatchBreakerLimpetController,20,0.5,"Hatch Breaker Limpet Controller Class 5 Rating C", Limpets:7, Speed:500, Range:3400, TargetRange:3300, HackTime:8, MinCargo:3, MaxCargo:8, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size5_class4", new ShipModule(128066545,ShipModule.ModuleTypes.HatchBreakerLimpetController,32,0.6,"Hatch Breaker Limpet Controller Class 5 Rating B", Limpets:9, Speed:500, Range:4060, TargetRange:3960, HackTime:6, MinCargo:4, MaxCargo:9, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size5_class5", new ShipModule(128066546,ShipModule.ModuleTypes.HatchBreakerLimpetController,20,0.7,"Hatch Breaker Limpet Controller Class 5 Rating A", Limpets:6, Speed:500, Range:4720, TargetRange:4620, HackTime:5, MinCargo:5, MaxCargo:10, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size7_class1", new ShipModule(128066547,ShipModule.ModuleTypes.HatchBreakerLimpetController,80,0.42,"Hatch Breaker Limpet Controller Class 7 Rating E", Limpets:18, Speed:500, Range:2680, TargetRange:2580, HackTime:6, MinCargo:1, MaxCargo:6, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size7_class2", new ShipModule(128066548,ShipModule.ModuleTypes.HatchBreakerLimpetController,32,0.56,"Hatch Breaker Limpet Controller Class 7 Rating D", Limpets:12, Speed:500, Range:3540, TargetRange:3440, HackTime:5, MinCargo:2, MaxCargo:7, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size7_class3", new ShipModule(128066549,ShipModule.ModuleTypes.HatchBreakerLimpetController,80,0.7,"Hatch Breaker Limpet Controller Class 7 Rating C", Limpets:15, Speed:500, Range:4400, TargetRange:4300, HackTime:4, MinCargo:3, MaxCargo:8, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size7_class4", new ShipModule(128066550,ShipModule.ModuleTypes.HatchBreakerLimpetController,128,0.84,"Hatch Breaker Limpet Controller Class 7 Rating B", Limpets:18, Speed:500, Range:5260, TargetRange:5160, HackTime:3, MinCargo:4, MaxCargo:9, Time:120 ) },
            { "int_dronecontrol_resourcesiphon_size7_class5", new ShipModule(128066551,ShipModule.ModuleTypes.HatchBreakerLimpetController,80,0.98,"Hatch Breaker Limpet Controller Class 7 Rating A", Limpets:12, Speed:500, Range:6120, TargetRange:6020, HackTime:2, MinCargo:5, MaxCargo:10, Time:120 ) },

            { "int_dronecontrol_prospector_size1_class1", new ShipModule(128671269,ShipModule.ModuleTypes.ProspectorLimpetController,1.3,0.18,"Prospector Limpet Controller Class 1 Rating E", Limpets:1, Speed:200, Range:3000 ) },
            { "int_dronecontrol_prospector_size1_class2", new ShipModule(128671270,ShipModule.ModuleTypes.ProspectorLimpetController,0.5,0.14,"Prospector Limpet Controller Class 1 Rating D", Limpets:1, Speed:200, Range:4000 ) },
            { "int_dronecontrol_prospector_size1_class3", new ShipModule(128671271,ShipModule.ModuleTypes.ProspectorLimpetController,1.3,0.23,"Prospector Limpet Controller Class 1 Rating C", Limpets:1, Speed:200, Range:5000 ) },
            { "int_dronecontrol_prospector_size1_class4", new ShipModule(128671272,ShipModule.ModuleTypes.ProspectorLimpetController,2,0.32,"Prospector Limpet Controller Class 1 Rating B", Limpets:1, Speed:200, Range:6000 ) },
            { "int_dronecontrol_prospector_size1_class5", new ShipModule(128671273,ShipModule.ModuleTypes.ProspectorLimpetController,1.3,0.28,"Prospector Limpet Controller Class 1 Rating A", Limpets:1, Speed:200, Range:7000 ) },
            { "int_dronecontrol_prospector_size3_class1", new ShipModule(128671274,ShipModule.ModuleTypes.ProspectorLimpetController,5,0.27,"Prospector Limpet Controller Class 3 Rating E", Limpets:2, Speed:200, Range:3300 ) },
            { "int_dronecontrol_prospector_size3_class2", new ShipModule(128671275,ShipModule.ModuleTypes.ProspectorLimpetController,2,0.2,"Prospector Limpet Controller Class 3 Rating D", Limpets:2, Speed:200, Range:4400 ) },
            { "int_dronecontrol_prospector_size3_class3", new ShipModule(128671276,ShipModule.ModuleTypes.ProspectorLimpetController,5,0.34,"Prospector Limpet Controller Class 3 Rating C", Limpets:2, Speed:200, Range:5500 ) },
            { "int_dronecontrol_prospector_size3_class4", new ShipModule(128671277,ShipModule.ModuleTypes.ProspectorLimpetController,8,0.48,"Prospector Limpet Controller Class 3 Rating B", Limpets:2, Speed:200, Range:6600 ) },
            { "int_dronecontrol_prospector_size3_class5", new ShipModule(128671278,ShipModule.ModuleTypes.ProspectorLimpetController,5,0.41,"Prospector Limpet Controller Class 3 Rating A", Limpets:2, Speed:200, Range:7700 ) },
            { "int_dronecontrol_prospector_size5_class1", new ShipModule(128671279,ShipModule.ModuleTypes.ProspectorLimpetController,20,0.4,"Prospector Limpet Controller Class 5 Rating E", Limpets:4, Speed:200, Range:3900 ) },
            { "int_dronecontrol_prospector_size5_class2", new ShipModule(128671280,ShipModule.ModuleTypes.ProspectorLimpetController,8,0.3,"Prospector Limpet Controller Class 5 Rating D", Limpets:4, Speed:200, Range:5200 ) },
            { "int_dronecontrol_prospector_size5_class3", new ShipModule(128671281,ShipModule.ModuleTypes.ProspectorLimpetController,20,0.5,"Prospector Limpet Controller Class 5 Rating C", Limpets:4, Speed:200, Range:6500 ) },
            { "int_dronecontrol_prospector_size5_class4", new ShipModule(128671282,ShipModule.ModuleTypes.ProspectorLimpetController,32,0.72,"Prospector Limpet Controller Class 5 Rating B", Limpets:4, Speed:200, Range:7800 ) },
            { "int_dronecontrol_prospector_size5_class5", new ShipModule(128671283,ShipModule.ModuleTypes.ProspectorLimpetController,20,0.6,"Prospector Limpet Controller Class 5 Rating A", Limpets:4, Speed:200, Range:9100 ) },
            { "int_dronecontrol_prospector_size7_class1", new ShipModule(128671284,ShipModule.ModuleTypes.ProspectorLimpetController,80,0.55,"Prospector Limpet Controller Class 7 Rating E", Limpets:8, Speed:200, Range:5100 ) },
            { "int_dronecontrol_prospector_size7_class2", new ShipModule(128671285,ShipModule.ModuleTypes.ProspectorLimpetController,32,0.41,"Prospector Limpet Controller Class 7 Rating D", Limpets:8, Speed:200, Range:6800 ) },
            { "int_dronecontrol_prospector_size7_class3", new ShipModule(128671286,ShipModule.ModuleTypes.ProspectorLimpetController,80,0.69,"Prospector Limpet Controller Class 7 Rating C", Limpets:8, Speed:200, Range:8500 ) },
            { "int_dronecontrol_prospector_size7_class4", new ShipModule(128671287,ShipModule.ModuleTypes.ProspectorLimpetController,128,0.97,"Prospector Limpet Controller Class 7 Rating B", Limpets:8, Speed:200, Range:10200 ) },
            { "int_dronecontrol_prospector_size7_class5", new ShipModule(128671288,ShipModule.ModuleTypes.ProspectorLimpetController,80,0.83,"Prospector Limpet Controller Class 7 Rating A", Limpets:8, Speed:200, Range:11900 ) },

            { "int_dronecontrol_repair_size1_class1", new ShipModule(128777327,ShipModule.ModuleTypes.RepairLimpetController,1.3,0.18,"Repair Limpet Controller Class 1 Rating E", Limpets:1, Speed:200, Range:600, Time:300 ) },
            { "int_dronecontrol_repair_size1_class2", new ShipModule(128777328,ShipModule.ModuleTypes.RepairLimpetController,0.5,0.14,"Repair Limpet Controller Class 1 Rating D", Limpets:1, Speed:200, Range:800, Time:300 ) },
            { "int_dronecontrol_repair_size1_class3", new ShipModule(128777329,ShipModule.ModuleTypes.RepairLimpetController,1.3,0.23,"Repair Limpet Controller Class 1 Rating C", Limpets:1, Speed:200, Range:1000, Time:300 ) },
            { "int_dronecontrol_repair_size1_class4", new ShipModule(128777330,ShipModule.ModuleTypes.RepairLimpetController,2,0.32,"Repair Limpet Controller Class 1 Rating B", Limpets:1, Speed:200, Range:1200, Time:300 ) },
            { "int_dronecontrol_repair_size1_class5", new ShipModule(128777331,ShipModule.ModuleTypes.RepairLimpetController,1.3,0.28,"Repair Limpet Controller Class 1 Rating A", Limpets:1, Speed:200, Range:1400, Time:300 ) },
            { "int_dronecontrol_repair_size3_class1", new ShipModule(128777332,ShipModule.ModuleTypes.RepairLimpetController,5,0.27,"Repair Limpet Controller Class 3 Rating E", Limpets:2, Speed:200, Range:660, Time:300 ) },
            { "int_dronecontrol_repair_size3_class2", new ShipModule(128777333,ShipModule.ModuleTypes.RepairLimpetController,2,0.2,"Repair Limpet Controller Class 3 Rating D", Limpets:2, Speed:200, Range:880, Time:300 ) },
            { "int_dronecontrol_repair_size3_class3", new ShipModule(128777334,ShipModule.ModuleTypes.RepairLimpetController,5,0.34,"Repair Limpet Controller Class 3 Rating C", Limpets:2, Speed:200, Range:1100, Time:300 ) },
            { "int_dronecontrol_repair_size3_class4", new ShipModule(128777335,ShipModule.ModuleTypes.RepairLimpetController,8,0.48,"Repair Limpet Controller Class 3 Rating B", Limpets:2, Speed:200, Range:1320, Time:300 ) },
            { "int_dronecontrol_repair_size3_class5", new ShipModule(128777336,ShipModule.ModuleTypes.RepairLimpetController,5,0.41,"Repair Limpet Controller Class 3 Rating A", Limpets:2, Speed:200, Range:1540, Time:300 ) },
            { "int_dronecontrol_repair_size5_class1", new ShipModule(128777337,ShipModule.ModuleTypes.RepairLimpetController,20,0.4,"Repair Limpet Controller Class 5 Rating E", Limpets:3, Speed:200, Range:780, Time:300 ) },
            { "int_dronecontrol_repair_size5_class2", new ShipModule(128777338,ShipModule.ModuleTypes.RepairLimpetController,8,0.3,"Repair Limpet Controller Class 5 Rating D", Limpets:3, Speed:200, Range:1040, Time:300 ) },
            { "int_dronecontrol_repair_size5_class3", new ShipModule(128777339,ShipModule.ModuleTypes.RepairLimpetController,20,0.5,"Repair Limpet Controller Class 5 Rating C", Limpets:3, Speed:200, Range:1300, Time:300 ) },
            { "int_dronecontrol_repair_size5_class4", new ShipModule(128777340,ShipModule.ModuleTypes.RepairLimpetController,32,0.72,"Repair Limpet Controller Class 5 Rating B", Limpets:3, Speed:200, Range:1560, Time:300 ) },
            { "int_dronecontrol_repair_size5_class5", new ShipModule(128777341,ShipModule.ModuleTypes.RepairLimpetController,20,0.6,"Repair Limpet Controller Class 5 Rating A", Limpets:3, Speed:200, Range:1820, Time:300 ) },
            { "int_dronecontrol_repair_size7_class1", new ShipModule(128777342,ShipModule.ModuleTypes.RepairLimpetController,80,0.55,"Repair Limpet Controller Class 7 Rating E", Limpets:4, Speed:200, Range:1020, Time:300 ) },
            { "int_dronecontrol_repair_size7_class2", new ShipModule(128777343,ShipModule.ModuleTypes.RepairLimpetController,32,0.41,"Repair Limpet Controller Class 7 Rating D", Limpets:4, Speed:200, Range:1360, Time:300 ) },
            { "int_dronecontrol_repair_size7_class3", new ShipModule(128777344,ShipModule.ModuleTypes.RepairLimpetController,80,0.69,"Repair Limpet Controller Class 7 Rating C", Limpets:4, Speed:200, Range:1700, Time:300 ) },
            { "int_dronecontrol_repair_size7_class4", new ShipModule(128777345,ShipModule.ModuleTypes.RepairLimpetController,128,0.97,"Repair Limpet Controller Class 7 Rating B", Limpets:4, Speed:200, Range:2040, Time:300 ) },
            { "int_dronecontrol_repair_size7_class5", new ShipModule(128777346,ShipModule.ModuleTypes.RepairLimpetController,80,0.83,"Repair Limpet Controller Class 7 Rating A", Limpets:4, Speed:200, Range:2380, Time:300 ) },

            { "int_dronecontrol_unkvesselresearch", new ShipModule(128793116,ShipModule.ModuleTypes.ResearchLimpetController,1.3,0.4,"Research Limpet Controller", Limpets:1, Speed:200, Range:5000, Time:300 ) },

            // More limpets

            { "int_dronecontrol_decontamination_size1_class1", new ShipModule(128793941,ShipModule.ModuleTypes.DecontaminationLimpetController,1.3,0.18,"Decontamination Limpet Controller Class 1 Rating E", Limpets:1, Speed:200, Range:600, Time:300 ) },
            { "int_dronecontrol_decontamination_size3_class1", new ShipModule(128793942,ShipModule.ModuleTypes.DecontaminationLimpetController,2,0.2,"Decontamination Limpet Controller Class 3 Rating E", Limpets:2, Speed:200, Range:880, Time:300 ) },
            { "int_dronecontrol_decontamination_size5_class1", new ShipModule(128793943,ShipModule.ModuleTypes.DecontaminationLimpetController,20,0.5,"Decontamination Limpet Controller Class 5 Rating E", Limpets:3, Speed:200, Range:1300, Time:300 ) },
            { "int_dronecontrol_decontamination_size7_class1", new ShipModule(128793944,ShipModule.ModuleTypes.DecontaminationLimpetController,128,0.97,"Decontamination Limpet Controller Class 7 Rating E", Limpets:4, Speed:200, Range:2040, Time:300 ) },

            { "int_dronecontrol_recon_size1_class1", new ShipModule(128837858,ShipModule.ModuleTypes.ReconLimpetController,1.3,0.18,"Recon Limpet Controller Class 1 Rating E", Limpets:1, Speed:100, Range:1200, HackTime:22 ) },

            { "int_dronecontrol_recon_size3_class1", new ShipModule(128841592,ShipModule.ModuleTypes.ReconLimpetController,2,0.2,"Recon Limpet Controller Class 3 Rating E", Limpets:1, Speed:100, Range:1400, HackTime:17 ) },
            { "int_dronecontrol_recon_size5_class1", new ShipModule(128841593,ShipModule.ModuleTypes.ReconLimpetController,20,0.5,"Recon Limpet Controller Class 5 Rating E", Limpets:1, Speed:100, Range:1700, HackTime:13 ) },
            { "int_dronecontrol_recon_size7_class1", new ShipModule(128841594,ShipModule.ModuleTypes.ReconLimpetController,128,0.97,"Recon Limpet Controller Class 7 Rating E", Limpets:1, Speed:100, Range:2000, HackTime:10 ) },

            { "int_multidronecontrol_mining_size3_class1", new ShipModule(129001921,ShipModule.ModuleTypes.MiningMultiLimpetController,12,0.5,"Mining Multi Limpet Controller Class 3 Rating E", Limpets:4, Speed:200, Range:3300 ) },
            { "int_multidronecontrol_mining_size3_class3", new ShipModule(129001922,ShipModule.ModuleTypes.MiningMultiLimpetController,10,0.35,"Mining Multi Limpet Controller Class 3 Rating C", Limpets:4, Speed:200, Range:5000 ) },
            { "int_multidronecontrol_operations_size3_class3", new ShipModule(129001923,ShipModule.ModuleTypes.OperationsMultiLimpetController,10,0.35,"Operations Limpet Controller Class 3 Rating C", Limpets:4, Speed:500, Range:2600, HackTime:16, MinCargo:3, MaxCargo:8, Time:510 ) },
            { "int_multidronecontrol_operations_size3_class4", new ShipModule(129001924,ShipModule.ModuleTypes.OperationsMultiLimpetController,15,0.3,"Operations Limpet Controller Class 3 Rating B", Limpets:4, Speed:500, Range:3100, HackTime:22, MinCargo:4, MaxCargo:9, Time:420 ) },
            { "int_multidronecontrol_rescue_size3_class2", new ShipModule(129001925,ShipModule.ModuleTypes.RescueMultiLimpetController,8,0.4,"Rescue Limpet Controller Class 3 Rating D", Limpets:4, Speed:500, Range:2100, HackTime:19, MinCargo:2, MaxCargo:7, Time:300 ) },
            { "int_multidronecontrol_rescue_size3_class3", new ShipModule(129001926,ShipModule.ModuleTypes.RescueMultiLimpetController,10,0.35,"Rescue Limpet Controller Class 3 Rating C", Limpets:4, Speed:500, Range:2600, HackTime:16, MinCargo:3, MaxCargo:8, Time:300 ) },
            { "int_multidronecontrol_xeno_size3_class3", new ShipModule(129001927,ShipModule.ModuleTypes.XenoMultiLimpetController,10,0.35,"Xeno Limpet Controller Class 3 Rating C", Limpets:4, Speed:200, Range:5000, Time:300 ) },
            { "int_multidronecontrol_xeno_size3_class4", new ShipModule(129001928,ShipModule.ModuleTypes.XenoMultiLimpetController,15,0.3,"Xeno Limpet Controller Class 3 Rating B", Limpets:4, Speed:200, Range:5000, Time:300 ) },
            { "int_multidronecontrol_universal_size7_class3", new ShipModule(129001929,ShipModule.ModuleTypes.UniversalMultiLimpetController,125,0.8,"Universal Multi Limpet Controller Class 7 Rating C", Limpets:8, Speed:500, Range:6500, HackTime:8, MinCargo:3, MaxCargo:8 ) },
            { "int_multidronecontrol_universal_size7_class5", new ShipModule(129001930,ShipModule.ModuleTypes.UniversalMultiLimpetController,140,1.1,"Universal Multi Limpet Controller Class 7 Rating A", Limpets:8, Speed:500, Range:9100, HackTime:5, MinCargo:5, MaxCargo:10 ) },

            // Meta Hull Reinforcement Packages

            { "int_metaalloyhullreinforcement_size1_class1", new ShipModule(128793117,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,2,0,"Meta Alloy Hull Reinforcement Package Class 1 Rating E", CausticReinforcement:3, HullReinforcement:72 ) },
            { "int_metaalloyhullreinforcement_size1_class2", new ShipModule(128793118,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,1,0,"Meta Alloy Hull Reinforcement Package Class 1 Rating D", CausticReinforcement:3, HullReinforcement:99 ) },
            { "int_metaalloyhullreinforcement_size2_class1", new ShipModule(128793119,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,4,0,"Meta Alloy Hull Reinforcement Package Class 2 Rating E", CausticReinforcement:3, HullReinforcement:135 ) },
            { "int_metaalloyhullreinforcement_size2_class2", new ShipModule(128793120,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,2,0,"Meta Alloy Hull Reinforcement Package Class 2 Rating D", CausticReinforcement:3, HullReinforcement:171 ) },
            { "int_metaalloyhullreinforcement_size3_class1", new ShipModule(128793121,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,8,0,"Meta Alloy Hull Reinforcement Package Class 3 Rating E", CausticReinforcement:3, HullReinforcement:207 ) },
            { "int_metaalloyhullreinforcement_size3_class2", new ShipModule(128793122,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,4,0,"Meta Alloy Hull Reinforcement Package Class 3 Rating D", CausticReinforcement:3, HullReinforcement:234 ) },
            { "int_metaalloyhullreinforcement_size4_class1", new ShipModule(128793123,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,16,0,"Meta Alloy Hull Reinforcement Package Class 4 Rating E", CausticReinforcement:3, HullReinforcement:270 ) },
            { "int_metaalloyhullreinforcement_size4_class2", new ShipModule(128793124,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,8,0,"Meta Alloy Hull Reinforcement Package Class 4 Rating D", CausticReinforcement:3, HullReinforcement:297 ) },
            { "int_metaalloyhullreinforcement_size5_class1", new ShipModule(128793125,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,32,0,"Meta Alloy Hull Reinforcement Package Class 5 Rating E", CausticReinforcement:3, HullReinforcement:324 ) },
            { "int_metaalloyhullreinforcement_size5_class2", new ShipModule(128793126,ShipModule.ModuleTypes.MetaAlloyHullReinforcement,16,0,"Meta Alloy Hull Reinforcement Package Class 5 Rating D", CausticReinforcement:3, HullReinforcement:351 ) },

            // Mine launches charges

            { "hpt_minelauncher_fixed_small", new ShipModule(128049500,ShipModule.ModuleTypes.MineLauncher,2,0.4,"Mine Launcher Fixed Small", Ammo:36, Clip:1, Damage:44, RateOfFire:1, BurstInterval:1, Reload:2, ThermL:5 ) },
            { "hpt_minelauncher_fixed_medium", new ShipModule(128049501,ShipModule.ModuleTypes.MineLauncher,4,0.4,"Mine Launcher Fixed Medium", Ammo:72, Clip:3, Damage:44, RateOfFire:1, BurstInterval:1, Reload:6.6, ThermL:7.5 ) },
            { "hpt_minelauncher_fixed_small_impulse", new ShipModule(128671448,ShipModule.ModuleTypes.ShockMineLauncher,2,0.4,"Shock Mine Launcher Fixed Small", Ammo:36, Clip:1, Damage:32, RateOfFire:1, BurstInterval:1, Reload:2, ThermL:5 ) },

            { "hpt_mining_abrblstr_fixed_small", new ShipModule(128915458,ShipModule.ModuleTypes.AbrasionBlaster,2,0.34,"Abrasion Blaster Fixed Small", Speed:667, Damage:4, Range:1000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, ThermL:2 ) },
            { "hpt_mining_abrblstr_turret_small", new ShipModule(128915459,ShipModule.ModuleTypes.AbrasionBlaster,2,0.47,"Abrasion Blaster Turret Small", Speed:667, Damage:4, Range:1000, FallOff:1000, RateOfFire:5, BurstInterval:0.2, ThermL:1.8 ) },

            { "hpt_mining_seismchrgwarhd_fixed_medium", new ShipModule(128915460,ShipModule.ModuleTypes.SeismicChargeLauncher,4,1.2,"Seismic Charge Launcher Fixed Medium", Ammo:72, Clip:1, Speed:350, Damage:15, Range:1000, RateOfFire:1, BurstInterval:1, Reload:1, ThermL:3.6 ) },
            { "hpt_mining_seismchrgwarhd_turret_medium", new ShipModule(128915461,ShipModule.ModuleTypes.SeismicChargeLauncher,4,1.2,"Seismic Charge Launcher Turret Medium", Ammo:72, Clip:1, Speed:350, Damage:15, Range:1000, RateOfFire:1, BurstInterval:1, Reload:1, ThermL:3.6 ) },

            { "hpt_mining_subsurfdispmisle_fixed_small", new ShipModule(128915454,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,2,0.42,"Sub surface Displacement Missile Fixed Small", Ammo:32, Clip:1, Speed:550, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:2.25 ) },
            { "hpt_mining_subsurfdispmisle_turret_small", new ShipModule(128915455,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,2,0.53,"Sub surface Displacement Missile Turret Small", Ammo:32, Clip:1, Speed:550, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:2.25 ) },
            { "hpt_mining_subsurfdispmisle_fixed_medium", new ShipModule(128915456,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,4,1.01,"Sub surface Displacement Missile Fixed Medium", Ammo:96, Clip:1, Speed:550, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:2.9 ) },
            { "hpt_mining_subsurfdispmisle_turret_medium", new ShipModule(128915457,ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile,4,0.93,"Sub surface Displacement Missile Turret Medium", Ammo:96, Clip:1, Speed:550, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:2.9 ) },

            // Mining lasers

            { "hpt_mininglaser_fixed_small", new ShipModule(128049525,ShipModule.ModuleTypes.MiningLaser,2,0.5,"Mining Laser Fixed Small", Damage:2, Range:500, FallOff:300, BurstInterval:0, ThermL:2 ) },
            { "hpt_mininglaser_fixed_medium", new ShipModule(128049526,ShipModule.ModuleTypes.MiningLaser,2,0.75,"Mining Laser Fixed Medium", Damage:4, Range:500, FallOff:300, BurstInterval:0, ThermL:4 ) },
            { "hpt_mininglaser_turret_small", new ShipModule(128740819,ShipModule.ModuleTypes.MiningLaser,2,0.5,"Mining Laser Turret Small", Damage:2, Range:500, FallOff:300, BurstInterval:0, ThermL:2 ) },
            { "hpt_mininglaser_turret_medium", new ShipModule(128740820,ShipModule.ModuleTypes.MiningLaser,2,0.75,"Mining Laser Turret Medium", Damage:4, Range:500, FallOff:300, BurstInterval:0, ThermL:4 ) },
            { "hpt_mininglaser_fixed_small_advanced", new ShipModule(128671340,ShipModule.ModuleTypes.MiningLance,2,0.7,"Mining Lance Beam Laser Fixed Small", Damage:8, Range:2000, FallOff:500, BurstInterval:0, ThermL:6 ) },
            
            // Missiles

            { "hpt_atdumbfiremissile_fixed_medium", new ShipModule(128788699,ShipModule.ModuleTypes.AXMissileRack,4,1.2,"AX Missile Rack Fixed Medium", Ammo:64, Clip:8, Speed:750, Damage:70, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:2.4 ) },
            { "hpt_atdumbfiremissile_fixed_large", new ShipModule(128788700,ShipModule.ModuleTypes.AXMissileRack,8,1.62,"AX Missile Rack Fixed Large", Ammo:128, Clip:12, Speed:750, Damage:70, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },
            { "hpt_atdumbfiremissile_turret_medium", new ShipModule(128788704,ShipModule.ModuleTypes.AXMissileRack,4,1.2,"AX Missile Rack Turret Medium", Ammo:64, Clip:8, Speed:750, Damage:57, Range:5000, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:1.5 ) },
            { "hpt_atdumbfiremissile_turret_large", new ShipModule(128788705,ShipModule.ModuleTypes.AXMissileRack,8,1.75,"AX Missile Rack Turret Large", Ammo:128, Clip:12, Speed:750, Damage:57, Range:5000, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:1.9 ) },

            { "hpt_atdumbfiremissile_fixed_medium_v2", new ShipModule(129022081,ShipModule.ModuleTypes.EnhancedAXMissileRack,4,1.3,"Enhanced AX Missile Rack Medium", Ammo:64, Clip:8, Speed:1250, Damage:77, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:2.4 ) },
            { "hpt_atdumbfiremissile_fixed_large_v2", new ShipModule(129022079,ShipModule.ModuleTypes.EnhancedAXMissileRack,8,1.72,"Enhanced AX Missile Rack Large", Ammo:128, Clip:12, Speed:1250, Damage:77, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },
            { "hpt_atdumbfiremissile_turret_medium_v2", new ShipModule(129022083,ShipModule.ModuleTypes.EnhancedAXMissileRack,4,1.3,"Enhanced AX Missile Rack Medium", Ammo:64, Clip:8, Speed:1250, Damage:64, Range:5000, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:1.5 ) },
            { "hpt_atdumbfiremissile_turret_large_v2", new ShipModule(129022082,ShipModule.ModuleTypes.EnhancedAXMissileRack,8,1.85,"Enhanced AX Missile Rack Large", Ammo:128, Clip:12, Speed:1250, Damage:64, Range:5000, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:1.9 ) },

            { "hpt_atventdisruptorpylon_fixed_medium", new ShipModule(129030049,ShipModule.ModuleTypes.TorpedoPylon,3,0.4,"Guardian Nanite Torpedo Pylon Medium", Ammo:64, Clip:1, Speed:1000, Damage:0, RateOfFire:0.5, BurstInterval:2, Reload:3, ThermL:35 ) },
            { "hpt_atventdisruptorpylon_fixed_large", new ShipModule(129030050,ShipModule.ModuleTypes.TorpedoPylon,5,0.7,"Guardian Nanite Torpedo Pylon Large", Ammo:128, Clip:1, Speed:1000, Damage:0, RateOfFire:0.5, BurstInterval:2, Reload:3, ThermL:35 ) },

            { "hpt_basicmissilerack_fixed_small", new ShipModule(128049492,ShipModule.ModuleTypes.SeekerMissileRack,2,0.6,"Seeker Missile Rack Fixed Small", Ammo:6, Clip:6, Speed:625, Damage:40, RateOfFire:0.33, BurstInterval:3, Reload:12, ThermL:3.6 ) },
            { "hpt_basicmissilerack_fixed_medium", new ShipModule(128049493,ShipModule.ModuleTypes.SeekerMissileRack,4,1.2,"Seeker Missile Rack Fixed Medium", Ammo:18, Clip:6, Speed:625, Damage:40, RateOfFire:0.33, BurstInterval:3, Reload:12, ThermL:3.6 ) },
            { "hpt_basicmissilerack_fixed_large", new ShipModule(128049494,ShipModule.ModuleTypes.SeekerMissileRack,8,1.62,"Seeker Missile Rack Fixed Large", Ammo:36, Clip:6, Speed:625, Damage:40, RateOfFire:0.33, BurstInterval:3, Reload:12, ThermL:3.6 ) },

            { "hpt_dumbfiremissilerack_fixed_small", new ShipModule(128666724,ShipModule.ModuleTypes.MissileRack,2,0.4,"Missile Rack Fixed Small", Ammo:16, Clip:8, Speed:750, Damage:50, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },
            { "hpt_dumbfiremissilerack_fixed_medium", new ShipModule(128666725,ShipModule.ModuleTypes.MissileRack,4,1.2,"Missile Rack Fixed Medium", Ammo:48, Clip:12, Speed:750, Damage:50, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },

            { "hpt_dumbfiremissilerack_fixed_large", new ShipModule(128891602,ShipModule.ModuleTypes.MissileRack,8,1.62,"Missile Rack Fixed Large", Ammo:96, Clip:12, Speed:750, Damage:50, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },

            { "hpt_dumbfiremissilerack_fixed_medium_lasso", new ShipModule(128732552,ShipModule.ModuleTypes.RocketPropelledFSDDisruptor,4,1.2,"Rocket Propelled FSD Disrupter Fixed Medium", Ammo:48, Clip:12, Speed:750, Damage:40, RateOfFire:0.33, BurstInterval:3, Reload:5, ThermL:3.6 ) },
            { "hpt_drunkmissilerack_fixed_medium", new ShipModule(128671344,ShipModule.ModuleTypes.Pack_HoundMissileRack,4,1.2,"Pack Hound Missile Rack Fixed Medium", Ammo:120, Clip:12, Speed:600, Damage:7.5, RateOfFire:2, BurstInterval:0.5, Reload:5, ThermL:3.6 ) },

            { "hpt_advancedtorppylon_fixed_small", new ShipModule(128049509,ShipModule.ModuleTypes.TorpedoPylon,2,0.4,"Advanced Torp Pylon Fixed Small", Clip:1, Speed:250, Damage:120, RateOfFire:1, BurstInterval:1, Reload:5, ThermL:45 ) },
            { "hpt_advancedtorppylon_fixed_medium", new ShipModule(128049510,ShipModule.ModuleTypes.TorpedoPylon,4,0.4,"Advanced Torp Pylon Fixed Medium", Clip:2, Speed:250, Damage:120, RateOfFire:1, BurstInterval:1, Reload:5, ThermL:50 ) },
            { "hpt_advancedtorppylon_fixed_large", new ShipModule(128049511,ShipModule.ModuleTypes.TorpedoPylon,8,0.6,"Advanced Torp Pylon Fixed Large", Clip:4, Speed:250, Damage:120, RateOfFire:1, BurstInterval:1, Reload:5, ThermL:55 ) },

            { "hpt_dumbfiremissilerack_fixed_small_advanced", new ShipModule(128935982,ShipModule.ModuleTypes.AdvancedMissileRack,2,0.4,"Advanced Missile Rack Fixed Small", Ammo:64, Clip:8, Speed:750, Damage:50, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },
            { "hpt_dumbfiremissilerack_fixed_medium_advanced", new ShipModule(128935983,ShipModule.ModuleTypes.AdvancedMissileRack,4,1.2,"Advanced Missile Rack Fixed Medium", Ammo:64, Clip:12, Speed:750, Damage:50, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:3.6 ) },

            { "hpt_human_extraction_fixed_medium", new ShipModule(129028577,ShipModule.ModuleTypes.MissileRack,4,1,"Human Extraction Missile Medium", Ammo:96, Clip:1, Speed:550, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:2, ThermL:2.9 ) },

            { "hpt_causticmissile_fixed_medium", new ShipModule(128833995,ShipModule.ModuleTypes.EnzymeMissileRack,4,1.2,"Enzyme Missile Rack Medium", Ammo:64, Clip:8, Speed:750, Damage:5, RateOfFire:0.5, BurstInterval:2, Reload:5, ThermL:1.5 ) },

            // Module Reinforcements

            { "int_modulereinforcement_size1_class1", new ShipModule(128737270,ShipModule.ModuleTypes.ModuleReinforcementPackage,2,0,"Module Reinforcement Package Class 1 Rating E", Protection:30 ) },
            { "int_modulereinforcement_size1_class2", new ShipModule(128737271,ShipModule.ModuleTypes.ModuleReinforcementPackage,1,0,"Module Reinforcement Package Class 1 Rating D", Protection:60 ) },
            { "int_modulereinforcement_size2_class1", new ShipModule(128737272,ShipModule.ModuleTypes.ModuleReinforcementPackage,4,0,"Module Reinforcement Package Class 2 Rating E", Protection:30 ) },
            { "int_modulereinforcement_size2_class2", new ShipModule(128737273,ShipModule.ModuleTypes.ModuleReinforcementPackage,2,0,"Module Reinforcement Package Class 2 Rating D", Protection:60 ) },
            { "int_modulereinforcement_size3_class1", new ShipModule(128737274,ShipModule.ModuleTypes.ModuleReinforcementPackage,8,0,"Module Reinforcement Package Class 3 Rating E", Protection:30 ) },
            { "int_modulereinforcement_size3_class2", new ShipModule(128737275,ShipModule.ModuleTypes.ModuleReinforcementPackage,4,0,"Module Reinforcement Package Class 3 Rating D", Protection:60 ) },
            { "int_modulereinforcement_size4_class1", new ShipModule(128737276,ShipModule.ModuleTypes.ModuleReinforcementPackage,16,0,"Module Reinforcement Package Class 4 Rating E", Protection:30 ) },
            { "int_modulereinforcement_size4_class2", new ShipModule(128737277,ShipModule.ModuleTypes.ModuleReinforcementPackage,8,0,"Module Reinforcement Package Class 4 Rating D", Protection:60 ) },
            { "int_modulereinforcement_size5_class1", new ShipModule(128737278,ShipModule.ModuleTypes.ModuleReinforcementPackage,32,0,"Module Reinforcement Package Class 5 Rating E", Protection:30 ) },
            { "int_modulereinforcement_size5_class2", new ShipModule(128737279,ShipModule.ModuleTypes.ModuleReinforcementPackage,16,0,"Module Reinforcement Package Class 5 Rating D", Protection:60 ) },

            // Multicannons (medium is 2E, turret 2F, large is 3E, 3F) Values from EDSY 14/5/24

            { "hpt_atmulticannon_fixed_medium", new ShipModule(128788701,ShipModule.ModuleTypes.AXMulti_Cannon,4,0.46,"AX Multi Cannon Fixed Medium", Ammo:2100, Clip:100, Speed:1600, Damage:3.31, Range:4000, FallOff:2000, RateOfFire:7.14, BurstInterval:0.14, Reload:4, ThermL:0.18 ) },
            { "hpt_atmulticannon_fixed_large", new ShipModule(128788702,ShipModule.ModuleTypes.AXMulti_Cannon,8,0.64,"AX Multi Cannon Fixed Large", Ammo:2100, Clip:100, Speed:1600, Damage:6.115, Range:4000, FallOff:2000, RateOfFire:5.88, BurstInterval:0.17, Reload:4, ThermL:0.28 ) },

            { "hpt_atmulticannon_turret_medium", new ShipModule(128793059,ShipModule.ModuleTypes.AXMulti_Cannon,4,0.5,"AX Multi Cannon Turret Medium", Ammo:2100, Clip:90, Speed:1600, Damage:1.73, Range:4000, FallOff:2000, RateOfFire:6.25, BurstInterval:0.16, Reload:4, ThermL:0.09 ) },
            { "hpt_atmulticannon_turret_large", new ShipModule(128793060,ShipModule.ModuleTypes.AXMulti_Cannon,8,0.64,"AX Multi Cannon Turret Large", Ammo:2100, Clip:90, Speed:1600, Damage:3.31, Range:4000, FallOff:2000, RateOfFire:6.25, BurstInterval:0.16, Reload:4, ThermL:0.09 ) },

            { "hpt_atmulticannon_fixed_medium_v2", new ShipModule(129022080,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,4,0.48,"Enhanced AX Multi Cannon Fixed Medium", Ammo:2100, Clip:100, Speed:4000, Damage:3.9, Range:4000, FallOff:2000, RateOfFire:7.1, BurstInterval:0.14, Reload:4, ThermL:0.18 ) },
            { "hpt_atmulticannon_fixed_large_v2", new ShipModule(129022084,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,8,0.69,"Enhanced AX Multi Cannon Fixed Large", Ammo:2100, Clip:100, Speed:4000, Damage:7.3, Range:4000, FallOff:2000, RateOfFire:5.9, BurstInterval:0.17, Reload:4, ThermL:0.28 ) },

            { "hpt_atmulticannon_turret_medium_v2", new ShipModule(129022086,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,4,0.52,"Enhanced AX Multi Cannon Turret Medium", Ammo:2100, Clip:90, Speed:4000, Damage:2, Range:4000, FallOff:2000, RateOfFire:6.2, BurstInterval:0.16, Reload:4, ThermL:0.1 ) },
            { "hpt_atmulticannon_turret_large_v2", new ShipModule(129022085,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,8,0.69,"Enhanced AX Multi Cannon Turret Large", Ammo:2100, Clip:90, Speed:4000, Damage:3.9, Range:4000, FallOff:2000, RateOfFire:6.2, BurstInterval:0.16, Reload:4, ThermL:0.1 ) },

            { "hpt_atmulticannon_gimbal_medium", new ShipModule(129022089,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,4,0.46,"Enhanced AX Multi Cannon Gimbal Medium", Ammo:2100, Clip:100, Speed:4000, Damage:3.7, Range:4000, FallOff:2000, RateOfFire:7.1, BurstInterval:0.14, Reload:4, ThermL:0.18 ) },
            { "hpt_atmulticannon_gimbal_large", new ShipModule(129022088,ShipModule.ModuleTypes.EnhancedAXMulti_Cannon,8,0.64,"Enhanced AX Multi Cannon Gimbal Large", Ammo:2100, Clip:100, Speed:4000, Damage:6.3, Range:4000, FallOff:2000, RateOfFire:5.9, BurstInterval:0.17, Reload:4, ThermL:0.28 ) },

            { "hpt_multicannon_fixed_small", new ShipModule(128049455,ShipModule.ModuleTypes.Multi_Cannon,2,0.28,"Multi Cannon Fixed Small", Ammo:2100, Clip:100, Speed:1600, Damage:1.12, Range:4000, FallOff:2000, RateOfFire:7.69, BurstInterval:0.13, Reload:4, ThermL:0.09 ) },
            { "hpt_multicannon_fixed_medium", new ShipModule(128049456,ShipModule.ModuleTypes.Multi_Cannon,4,0.46,"Multi Cannon Fixed Medium", Ammo:2100, Clip:100, Speed:1600, Damage:2.19, Range:4000, FallOff:2000, RateOfFire:7.14, BurstInterval:0.14, Reload:4, ThermL:0.18 ) },
            { "hpt_multicannon_fixed_large", new ShipModule(128049457,ShipModule.ModuleTypes.Multi_Cannon,8,0.64,"Multi Cannon Fixed Medium", Ammo:2100, Clip:100, Speed:1600, Damage:3.925, Range:4000, FallOff:2000, RateOfFire:5.88, BurstInterval:0.17, Reload:4, ThermL:0.28 ) },
            { "hpt_multicannon_fixed_huge", new ShipModule(128049458,ShipModule.ModuleTypes.Multi_Cannon,16,0.73,"Multi Cannon Fixed Huge", Ammo:2100, Clip:100, Speed:1600, Damage:4.625, Range:4000, FallOff:2000, RateOfFire:3.03, BurstInterval:0.33, Reload:4, ThermL:0.39 ) },
            { "hpt_multicannon_gimbal_small", new ShipModule(128049459,ShipModule.ModuleTypes.Multi_Cannon,2,0.37,"Multi Cannon Gimbal Small", Ammo:2100, Clip:90, Speed:1600, Damage:0.82, Range:4000, FallOff:2000, RateOfFire:8.33, BurstInterval:0.12, Reload:5, ThermL:0.1 ) },
            { "hpt_multicannon_gimbal_medium", new ShipModule(128049460,ShipModule.ModuleTypes.Multi_Cannon,4,0.64,"Multi Cannon Gimbal Medium", Ammo:2100, Clip:90, Speed:1600, Damage:1.64, Range:4000, FallOff:2000, RateOfFire:7.69, BurstInterval:0.13, Reload:5, ThermL:0.2 ) },
            { "hpt_multicannon_gimbal_large", new ShipModule(128049461,ShipModule.ModuleTypes.Multi_Cannon,8,0.97,"Multi Cannon Gimbal Large", Ammo:2100, Clip:90, Speed:1600, Damage:2.84, Range:4000, FallOff:2000, RateOfFire:6.67, BurstInterval:0.15, Reload:5, ThermL:0.34 ) },
            { "hpt_multicannon_turret_small", new ShipModule(128049462,ShipModule.ModuleTypes.Multi_Cannon,2,0.26,"Multi Cannon Turret Small", Ammo:2100, Clip:90, Speed:1600, Damage:0.56, Range:4000, FallOff:2000, RateOfFire:7.14, BurstInterval:0.14, Reload:4, ThermL:0.04 ) },
            { "hpt_multicannon_turret_medium", new ShipModule(128049463,ShipModule.ModuleTypes.Multi_Cannon,4,0.5,"Multi Cannon Turret Medium", Ammo:2100, Clip:90, Speed:1600, Damage:1.17, Range:4000, FallOff:2000, RateOfFire:6.25, BurstInterval:0.16, Reload:4, ThermL:0.09 ) },
            { "hpt_multicannon_turret_large", new ShipModule(128049464,ShipModule.ModuleTypes.Multi_Cannon,8,0.86,"Multi Cannon Turret Large", Ammo:2100, Clip:90, Speed:1600, Damage:2.23, Range:4000, FallOff:2000, RateOfFire:5.26, BurstInterval:0.19, Reload:4, ThermL:0.19 ) },

            { "hpt_multicannon_gimbal_huge", new ShipModule(128681996,ShipModule.ModuleTypes.Multi_Cannon,16,1.22,"Multi Cannon Gimbal Huge", Ammo:2100, Clip:90, Speed:1600, Damage:3.46, Range:4000, FallOff:2000, RateOfFire:3.37, BurstInterval:0.3, Reload:5, ThermL:0.51 ) },

            { "hpt_multicannon_fixed_small_strong", new ShipModule(128671345,ShipModule.ModuleTypes.EnforcerCannon,2,0.28,"Enforcer Cannon Fixed Small", Ammo:1000, Clip:60, Speed:1800, Damage:2.85, Range:4500, FallOff:-999, RateOfFire:4.35, BurstInterval:0.23, Reload:4, ThermL:0.18 ) },

            { "hpt_multicannon_fixed_medium_advanced", new ShipModule(128935980,ShipModule.ModuleTypes.AdvancedMulti_Cannon,4,0.46,"Advanced Multi Cannon Fixed Medium", Ammo:2100, Clip:100, Speed:1600, Damage:2.19, Range:4000, FallOff:2000, RateOfFire:7.14, BurstInterval:0.14, Reload:4, ThermL:0.18 ) },
            { "hpt_multicannon_fixed_small_advanced", new ShipModule(128935981,ShipModule.ModuleTypes.AdvancedMulti_Cannon,2,0.28,"Advanced Multi Cannon Fixed Small", Ammo:2100, Clip:100, Speed:1600, Damage:1.12, Range:4000, FallOff:2000, RateOfFire:7.69, BurstInterval:0.13, Reload:4, ThermL:0.09 ) },

            // Passenger cabins

            { "int_passengercabin_size4_class1", new ShipModule(128727922,ShipModule.ModuleTypes.EconomyClassPassengerCabin,10,0,"Economy Class Passenger Cabin Class 4 Rating E", Passengers:8 ) },
            { "int_passengercabin_size4_class2", new ShipModule(128727923,ShipModule.ModuleTypes.BusinessClassPassengerCabin,10,0,"Business Class Passenger Cabin Class 4 Rating D", Passengers:6 ) },
            { "int_passengercabin_size4_class3", new ShipModule(128727924,ShipModule.ModuleTypes.FirstClassPassengerCabin,10,0,"First Class Passenger Cabin Class 4 Rating C", Passengers:3 ) },
            { "int_passengercabin_size5_class4", new ShipModule(128727925,ShipModule.ModuleTypes.LuxuryClassPassengerCabin,20,0,"Luxury Class Passenger Cabin Class 5 Rating B", Passengers:4 ) },
            { "int_passengercabin_size6_class1", new ShipModule(128727926,ShipModule.ModuleTypes.EconomyClassPassengerCabin,40,0,"Economy Class Passenger Cabin Class 6 Rating E", Passengers:32 ) },
            { "int_passengercabin_size6_class2", new ShipModule(128727927,ShipModule.ModuleTypes.BusinessClassPassengerCabin,40,0,"Business Class Passenger Cabin Class 6 Rating D", Passengers:16 ) },
            { "int_passengercabin_size6_class3", new ShipModule(128727928,ShipModule.ModuleTypes.FirstClassPassengerCabin,40,0,"First Class Passenger Cabin Class 6 Rating C", Passengers:12 ) },
            { "int_passengercabin_size6_class4", new ShipModule(128727929,ShipModule.ModuleTypes.LuxuryClassPassengerCabin,40,0,"Luxury Class Passenger Cabin Class 6 Rating B", Passengers:8 ) },

            { "int_passengercabin_size2_class1", new ShipModule(128734690,ShipModule.ModuleTypes.EconomyClassPassengerCabin,2.5,0,"Economy Class Passenger Cabin Class 2 Rating E", Passengers:2 ) },
            { "int_passengercabin_size3_class1", new ShipModule(128734691,ShipModule.ModuleTypes.EconomyClassPassengerCabin,5,0,"Economy Class Passenger Cabin Class 3 Rating E", Passengers:4 ) },
            { "int_passengercabin_size3_class2", new ShipModule(128734692,ShipModule.ModuleTypes.BusinessClassPassengerCabin,5,0,"Business Class Passenger Cabin Class 3 Rating D", Passengers:3 ) },
            { "int_passengercabin_size5_class1", new ShipModule(128734693,ShipModule.ModuleTypes.EconomyClassPassengerCabin,20,0,"Economy Class Passenger Cabin Class 5 Rating E", Passengers:16 ) },
            { "int_passengercabin_size5_class2", new ShipModule(128734694,ShipModule.ModuleTypes.BusinessClassPassengerCabin,20,0,"Business Class Passenger Cabin Class 5 Rating D", Passengers:10 ) },
            { "int_passengercabin_size5_class3", new ShipModule(128734695,ShipModule.ModuleTypes.FirstClassPassengerCabin,20,0,"First Class Passenger Cabin Class 5 Rating C", Passengers:6 ) },

            // Planetary approach

            { "int_planetapproachsuite_advanced", new ShipModule(128975719,ShipModule.ModuleTypes.AdvancedPlanetaryApproachSuite,0,0,"Advanced Planet Approach Suite" ) },
            { "int_planetapproachsuite", new ShipModule(128672317,ShipModule.ModuleTypes.PlanetaryApproachSuite,0,0,"Planet Approach Suite" ) },

            // planetary hangar

            { "int_buggybay_size2_class1", new ShipModule(128672288,ShipModule.ModuleTypes.PlanetaryVehicleHangar,12,0.25,"Planetary Vehicle Hangar Class 2 Rating H", Size:1, Rebuilds:1 ) },
            { "int_buggybay_size2_class2", new ShipModule(128672289,ShipModule.ModuleTypes.PlanetaryVehicleHangar,6,0.75,"Planetary Vehicle Hangar Class 2 Rating G", Size:1, Rebuilds:1 ) },
            { "int_buggybay_size4_class1", new ShipModule(128672290,ShipModule.ModuleTypes.PlanetaryVehicleHangar,20,0.4,"Planetary Vehicle Hangar Class 4 Rating H", Size:2, Rebuilds:1 ) },
            { "int_buggybay_size4_class2", new ShipModule(128672291,ShipModule.ModuleTypes.PlanetaryVehicleHangar,10,1.2,"Planetary Vehicle Hangar Class 4 Rating G", Size:2, Rebuilds:1 ) },
            { "int_buggybay_size6_class1", new ShipModule(128672292,ShipModule.ModuleTypes.PlanetaryVehicleHangar,34,0.6,"Planetary Vehicle Hangar Class 6 Rating H", Size:4, Rebuilds:1 ) },
            { "int_buggybay_size6_class2", new ShipModule(128672293,ShipModule.ModuleTypes.PlanetaryVehicleHangar,17,1.8,"Planetary Vehicle Hangar Class 6 Rating G", Size:4, Rebuilds:1 ) },

            // Plasmas

            { "hpt_plasmaaccelerator_fixed_medium", new ShipModule(128049465,ShipModule.ModuleTypes.PlasmaAccelerator,4,1.43,"Plasma Accelerator Fixed Medium", Ammo:100, Clip:5, Speed:875, Damage:54.3, Range:3500, FallOff:2000, RateOfFire:0.33, BurstInterval:3.03, Reload:6, ThermL:15.58 ) },
            { "hpt_plasmaaccelerator_fixed_large", new ShipModule(128049466,ShipModule.ModuleTypes.PlasmaAccelerator,8,1.97,"Plasma Accelerator Fixed Large", Ammo:100, Clip:5, Speed:875, Damage:83.4, Range:3500, FallOff:2000, RateOfFire:0.29, BurstInterval:3.45, Reload:6, ThermL:21.75 ) },
            { "hpt_plasmaaccelerator_fixed_huge", new ShipModule(128049467,ShipModule.ModuleTypes.PlasmaAccelerator,16,2.63,"Plasma Accelerator Fixed Huge", Ammo:100, Clip:5, Speed:875, Damage:125.25, Range:3500, FallOff:2000, RateOfFire:0.25, BurstInterval:4, Reload:6, ThermL:29.46 ) },

            { "hpt_plasmaaccelerator_fixed_large_advanced", new ShipModule(128671339,ShipModule.ModuleTypes.AdvancedPlasmaAccelerator,8,1.97,"Advanced Plasma Accelerator Fixed Large", Ammo:300, Clip:20, Speed:875, Damage:34.4, Range:3500, FallOff:2000, RateOfFire:0.83, BurstInterval:1.2, Reload:6, ThermL:11 ) },

            { "hpt_plasmashockcannon_fixed_large", new ShipModule(128834780,ShipModule.ModuleTypes.ShockCannon,8,0.89,"Shock Cannon Fixed Large", Ammo:240, Clip:16, Speed:1200, Damage:18.14, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:2.66 ) },
            { "hpt_plasmashockcannon_gimbal_large", new ShipModule(128834781,ShipModule.ModuleTypes.ShockCannon,8,0.89,"Shock Cannon Gimbal Large", Ammo:240, Clip:16, Speed:1200, Damage:14.87, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:3.12 ) },
            { "hpt_plasmashockcannon_turret_large", new ShipModule(128834782,ShipModule.ModuleTypes.ShockCannon,8,0.64,"Shock Cannon Turret Large", Ammo:240, Clip:16, Speed:1200, Damage:12.26, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:2.2 ) },

            { "hpt_plasmashockcannon_fixed_medium", new ShipModule(128834002,ShipModule.ModuleTypes.ShockCannon,4,0.57,"Shock Cannon Fixed Medium", Ammo:240, Clip:16, Speed:1200, Damage:12.96, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:1.8 ) },
            { "hpt_plasmashockcannon_gimbal_medium", new ShipModule(128834003,ShipModule.ModuleTypes.ShockCannon,4,0.61,"Shock Cannon Gimbal Medium", Ammo:240, Clip:16, Speed:1200, Damage:10.21, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:2.1 ) },
            { "hpt_plasmashockcannon_turret_medium", new ShipModule(128834004,ShipModule.ModuleTypes.ShockCannon,4,0.5,"Shock Cannon Turret Medium", Ammo:240, Clip:16, Speed:1200, Damage:8.96, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:1.24 ) },

            { "hpt_plasmashockcannon_turret_small", new ShipModule(128891603,ShipModule.ModuleTypes.ShockCannon,2,0.54,"Shock Cannon Turret Small", Ammo:240, Clip:16, Speed:1200, Damage:4.47, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:0.69 ) },
            { "hpt_plasmashockcannon_gimbal_small", new ShipModule(128891604,ShipModule.ModuleTypes.ShockCannon,2,0.47,"Shock Cannon Gimbal Small", Ammo:240, Clip:16, Speed:1200, Damage:6.91, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:1.45 ) },
            { "hpt_plasmashockcannon_fixed_small", new ShipModule(128891605,ShipModule.ModuleTypes.ShockCannon,2,0.41,"Shock Cannon Fixed Small", Ammo:240, Clip:16, Speed:1200, Damage:8.64, Range:3000, FallOff:2500, RateOfFire:10, BurstInterval:0.1, Reload:6, ThermL:1.14 ) },

            // power distributor

            { "int_powerdistributor_size1_class1", new ShipModule(128064178,ShipModule.ModuleTypes.PowerDistributor,1.3,0.32,"Power Distributor Class 1 Rating E", SysMW:0.4, EngMW:0.4, WepMW:1.2, SysCap:8, EngCap:8, WepCap:10 ) },
            { "int_powerdistributor_size1_class2", new ShipModule(128064179,ShipModule.ModuleTypes.PowerDistributor,0.5,0.36,"Power Distributor Class 1 Rating D", SysMW:0.5, EngMW:0.5, WepMW:1.4, SysCap:9, EngCap:9, WepCap:11 ) },
            { "int_powerdistributor_size1_class3", new ShipModule(128064180,ShipModule.ModuleTypes.PowerDistributor,1.3,0.4,"Power Distributor Class 1 Rating C", SysMW:0.5, EngMW:0.5, WepMW:1.5, SysCap:10, EngCap:10, WepCap:12 ) },
            { "int_powerdistributor_size1_class4", new ShipModule(128064181,ShipModule.ModuleTypes.PowerDistributor,2,0.44,"Power Distributor Class 1 Rating B", SysMW:0.6, EngMW:0.6, WepMW:1.7, SysCap:11, EngCap:11, WepCap:13 ) },
            { "int_powerdistributor_size1_class5", new ShipModule(128064182,ShipModule.ModuleTypes.PowerDistributor,1.3,0.48,"Power Distributor Class 1 Rating A", SysMW:0.6, EngMW:0.6, WepMW:1.8, SysCap:12, EngCap:12, WepCap:14 ) },
            { "int_powerdistributor_size2_class1", new ShipModule(128064183,ShipModule.ModuleTypes.PowerDistributor,2.5,0.36,"Power Distributor Class 2 Rating E", SysMW:0.6, EngMW:0.6, WepMW:1.4, SysCap:10, EngCap:10, WepCap:12 ) },
            { "int_powerdistributor_size2_class2", new ShipModule(128064184,ShipModule.ModuleTypes.PowerDistributor,1,0.41,"Power Distributor Class 2 Rating D", SysMW:0.6, EngMW:0.6, WepMW:1.6, SysCap:11, EngCap:11, WepCap:14 ) },
            { "int_powerdistributor_size2_class3", new ShipModule(128064185,ShipModule.ModuleTypes.PowerDistributor,2.5,0.45,"Power Distributor Class 2 Rating C", SysMW:0.7, EngMW:0.7, WepMW:1.8, SysCap:12, EngCap:12, WepCap:15 ) },
            { "int_powerdistributor_size2_class4", new ShipModule(128064186,ShipModule.ModuleTypes.PowerDistributor,4,0.5,"Power Distributor Class 2 Rating B", SysMW:0.8, EngMW:0.8, WepMW:2, SysCap:13, EngCap:13, WepCap:17 ) },
            { "int_powerdistributor_size2_class5", new ShipModule(128064187,ShipModule.ModuleTypes.PowerDistributor,2.5,0.54,"Power Distributor Class 2 Rating A", SysMW:0.8, EngMW:0.8, WepMW:2.2, SysCap:14, EngCap:14, WepCap:18 ) },
            { "int_powerdistributor_size3_class1", new ShipModule(128064188,ShipModule.ModuleTypes.PowerDistributor,5,0.4,"Power Distributor Class 3 Rating E", SysMW:0.9, EngMW:0.9, WepMW:1.8, SysCap:12, EngCap:12, WepCap:16 ) },
            { "int_powerdistributor_size3_class2", new ShipModule(128064189,ShipModule.ModuleTypes.PowerDistributor,2,0.45,"Power Distributor Class 3 Rating D", SysMW:1, EngMW:1, WepMW:2.1, SysCap:14, EngCap:14, WepCap:18 ) },
            { "int_powerdistributor_size3_class3", new ShipModule(128064190,ShipModule.ModuleTypes.PowerDistributor,5,0.5,"Power Distributor Class 3 Rating C", SysMW:1.1, EngMW:1.1, WepMW:2.3, SysCap:15, EngCap:15, WepCap:20 ) },
            { "int_powerdistributor_size3_class4", new ShipModule(128064191,ShipModule.ModuleTypes.PowerDistributor,8,0.55,"Power Distributor Class 3 Rating B", SysMW:1.2, EngMW:1.2, WepMW:2.5, SysCap:17, EngCap:17, WepCap:22 ) },
            { "int_powerdistributor_size3_class5", new ShipModule(128064192,ShipModule.ModuleTypes.PowerDistributor,5,0.6,"Power Distributor Class 3 Rating A", SysMW:1.3, EngMW:1.3, WepMW:2.8, SysCap:18, EngCap:18, WepCap:24 ) },
            { "int_powerdistributor_size4_class1", new ShipModule(128064193,ShipModule.ModuleTypes.PowerDistributor,10,0.45,"Power Distributor Class 4 Rating E", SysMW:1.3, EngMW:1.3, WepMW:2.3, SysCap:15, EngCap:15, WepCap:22 ) },
            { "int_powerdistributor_size4_class2", new ShipModule(128064194,ShipModule.ModuleTypes.PowerDistributor,4,0.5,"Power Distributor Class 4 Rating D", SysMW:1.4, EngMW:1.4, WepMW:2.6, SysCap:17, EngCap:17, WepCap:24 ) },
            { "int_powerdistributor_size4_class3", new ShipModule(128064195,ShipModule.ModuleTypes.PowerDistributor,10,0.56,"Power Distributor Class 4 Rating C", SysMW:1.6, EngMW:1.6, WepMW:2.9, SysCap:19, EngCap:19, WepCap:27 ) },
            { "int_powerdistributor_size4_class4", new ShipModule(128064196,ShipModule.ModuleTypes.PowerDistributor,16,0.62,"Power Distributor Class 4 Rating B", SysMW:1.8, EngMW:1.8, WepMW:3.2, SysCap:21, EngCap:21, WepCap:30 ) },
            { "int_powerdistributor_size4_class5", new ShipModule(128064197,ShipModule.ModuleTypes.PowerDistributor,10,0.67,"Power Distributor Class 4 Rating A", SysMW:1.9, EngMW:1.9, WepMW:3.5, SysCap:23, EngCap:23, WepCap:32 ) },
            { "int_powerdistributor_size5_class1", new ShipModule(128064198,ShipModule.ModuleTypes.PowerDistributor,20,0.5,"Power Distributor Class 5 Rating E", SysMW:1.7, EngMW:1.7, WepMW:2.9, SysCap:19, EngCap:19, WepCap:27 ) },
            { "int_powerdistributor_size5_class2", new ShipModule(128064199,ShipModule.ModuleTypes.PowerDistributor,8,0.56,"Power Distributor Class 5 Rating D", SysMW:1.9, EngMW:1.9, WepMW:3.2, SysCap:22, EngCap:22, WepCap:31 ) },
            { "int_powerdistributor_size5_class3", new ShipModule(128064200,ShipModule.ModuleTypes.PowerDistributor,20,0.62,"Power Distributor Class 5 Rating C", SysMW:2.1, EngMW:2.1, WepMW:3.6, SysCap:24, EngCap:24, WepCap:34 ) },
            { "int_powerdistributor_size5_class4", new ShipModule(128064201,ShipModule.ModuleTypes.PowerDistributor,32,0.68,"Power Distributor Class 5 Rating B", SysMW:2.3, EngMW:2.3, WepMW:4, SysCap:26, EngCap:26, WepCap:37 ) },
            { "int_powerdistributor_size5_class5", new ShipModule(128064202,ShipModule.ModuleTypes.PowerDistributor,20,0.74,"Power Distributor Class 5 Rating A", SysMW:2.5, EngMW:2.5, WepMW:4.3, SysCap:29, EngCap:29, WepCap:41 ) },
            { "int_powerdistributor_size6_class1", new ShipModule(128064203,ShipModule.ModuleTypes.PowerDistributor,40,0.54,"Power Distributor Class 6 Rating E", SysMW:2.2, EngMW:2.2, WepMW:3.4, SysCap:23, EngCap:23, WepCap:34 ) },
            { "int_powerdistributor_size6_class2", new ShipModule(128064204,ShipModule.ModuleTypes.PowerDistributor,16,0.61,"Power Distributor Class 6 Rating D", SysMW:2.4, EngMW:2.4, WepMW:3.9, SysCap:26, EngCap:26, WepCap:38 ) },
            { "int_powerdistributor_size6_class3", new ShipModule(128064205,ShipModule.ModuleTypes.PowerDistributor,40,0.68,"Power Distributor Class 6 Rating C", SysMW:2.7, EngMW:2.7, WepMW:4.3, SysCap:29, EngCap:29, WepCap:42 ) },
            { "int_powerdistributor_size6_class4", new ShipModule(128064206,ShipModule.ModuleTypes.PowerDistributor,64,0.75,"Power Distributor Class 6 Rating B", SysMW:3, EngMW:3, WepMW:4.7, SysCap:32, EngCap:32, WepCap:46 ) },
            { "int_powerdistributor_size6_class5", new ShipModule(128064207,ShipModule.ModuleTypes.PowerDistributor,40,0.82,"Power Distributor Class 6 Rating A", SysMW:3.2, EngMW:3.2, WepMW:5.2, SysCap:35, EngCap:35, WepCap:50 ) },
            { "int_powerdistributor_size7_class1", new ShipModule(128064208,ShipModule.ModuleTypes.PowerDistributor,80,0.59,"Power Distributor Class 7 Rating E", SysMW:2.6, EngMW:2.6, WepMW:4.1, SysCap:27, EngCap:27, WepCap:41 ) },
            { "int_powerdistributor_size7_class2", new ShipModule(128064209,ShipModule.ModuleTypes.PowerDistributor,32,0.67,"Power Distributor Class 7 Rating D", SysMW:3, EngMW:3, WepMW:4.6, SysCap:31, EngCap:31, WepCap:46 ) },
            { "int_powerdistributor_size7_class3", new ShipModule(128064210,ShipModule.ModuleTypes.PowerDistributor,80,0.74,"Power Distributor Class 7 Rating C", SysMW:3.3, EngMW:3.3, WepMW:5.1, SysCap:34, EngCap:34, WepCap:51 ) },
            { "int_powerdistributor_size7_class4", new ShipModule(128064211,ShipModule.ModuleTypes.PowerDistributor,128,0.81,"Power Distributor Class 7 Rating B", SysMW:3.6, EngMW:3.6, WepMW:5.6, SysCap:37, EngCap:37, WepCap:56 ) },
            { "int_powerdistributor_size7_class5", new ShipModule(128064212,ShipModule.ModuleTypes.PowerDistributor,80,0.89,"Power Distributor Class 7 Rating A", SysMW:4, EngMW:4, WepMW:6.1, SysCap:41, EngCap:41, WepCap:61 ) },
            { "int_powerdistributor_size8_class1", new ShipModule(128064213,ShipModule.ModuleTypes.PowerDistributor,160,0.64,"Power Distributor Class 8 Rating E", SysMW:3.2, EngMW:3.2, WepMW:4.8, SysCap:32, EngCap:32, WepCap:48 ) },
            { "int_powerdistributor_size8_class2", new ShipModule(128064214,ShipModule.ModuleTypes.PowerDistributor,64,0.72,"Power Distributor Class 8 Rating D", SysMW:3.6, EngMW:3.6, WepMW:5.4, SysCap:36, EngCap:36, WepCap:54 ) },
            { "int_powerdistributor_size8_class3", new ShipModule(128064215,ShipModule.ModuleTypes.PowerDistributor,160,0.8,"Power Distributor Class 8 Rating C", SysMW:4, EngMW:4, WepMW:6, SysCap:40, EngCap:40, WepCap:60 ) },
            { "int_powerdistributor_size8_class4", new ShipModule(128064216,ShipModule.ModuleTypes.PowerDistributor,256,0.88,"Power Distributor Class 8 Rating B", SysMW:4.4, EngMW:4.4, WepMW:6.6, SysCap:44, EngCap:44, WepCap:66 ) },
            { "int_powerdistributor_size8_class5", new ShipModule(128064217,ShipModule.ModuleTypes.PowerDistributor,160,0.96,"Power Distributor Class 8 Rating A", SysMW:4.8, EngMW:4.8, WepMW:7.2, SysCap:48, EngCap:48, WepCap:72 ) },

            { "int_powerdistributor_size1_class1_free", new ShipModule(128666639,ShipModule.ModuleTypes.PowerDistributor,1.3,0.32,"Power Distributor Class 1 Rating E", SysMW:0.4, EngMW:0.4, WepMW:1.2, SysCap:8, EngCap:8, WepCap:10 ) },

            // Power plant

            { "int_powerplant_size2_class1", new ShipModule(128064033,ShipModule.ModuleTypes.PowerPlant,2.5,0,"Power Plant Class 2 Rating E", PowerGen:6.4, HeatEfficiency:1 ) },
            { "int_powerplant_size2_class2", new ShipModule(128064034,ShipModule.ModuleTypes.PowerPlant,1,0,"Power Plant Class 2 Rating D", PowerGen:7.2, HeatEfficiency:0.75 ) },
            { "int_powerplant_size2_class3", new ShipModule(128064035,ShipModule.ModuleTypes.PowerPlant,1.3,0,"Power Plant Class 2 Rating C", PowerGen:8, HeatEfficiency:0.5 ) },
            { "int_powerplant_size2_class4", new ShipModule(128064036,ShipModule.ModuleTypes.PowerPlant,2,0,"Power Plant Class 2 Rating B", PowerGen:8.8, HeatEfficiency:0.45 ) },
            { "int_powerplant_size2_class5", new ShipModule(128064037,ShipModule.ModuleTypes.PowerPlant,1.3,0,"Power Plant Class 2 Rating A", PowerGen:9.6, HeatEfficiency:0.4 ) },
            { "int_powerplant_size3_class1", new ShipModule(128064038,ShipModule.ModuleTypes.PowerPlant,5,0,"Power Plant Class 3 Rating E", PowerGen:8, HeatEfficiency:1 ) },
            { "int_powerplant_size3_class2", new ShipModule(128064039,ShipModule.ModuleTypes.PowerPlant,2,0,"Power Plant Class 3 Rating D", PowerGen:9, HeatEfficiency:0.75 ) },
            { "int_powerplant_size3_class3", new ShipModule(128064040,ShipModule.ModuleTypes.PowerPlant,2.5,0,"Power Plant Class 3 Rating C", PowerGen:10, HeatEfficiency:0.5 ) },
            { "int_powerplant_size3_class4", new ShipModule(128064041,ShipModule.ModuleTypes.PowerPlant,4,0,"Power Plant Class 3 Rating B", PowerGen:11, HeatEfficiency:0.45 ) },
            { "int_powerplant_size3_class5", new ShipModule(128064042,ShipModule.ModuleTypes.PowerPlant,2.5,0,"Power Plant Class 3 Rating A", PowerGen:12, HeatEfficiency:0.4 ) },
            { "int_powerplant_size4_class1", new ShipModule(128064043,ShipModule.ModuleTypes.PowerPlant,10,0,"Power Plant Class 4 Rating E", PowerGen:10.4, HeatEfficiency:1 ) },
            { "int_powerplant_size4_class2", new ShipModule(128064044,ShipModule.ModuleTypes.PowerPlant,4,0,"Power Plant Class 4 Rating D", PowerGen:11.7, HeatEfficiency:0.75 ) },
            { "int_powerplant_size4_class3", new ShipModule(128064045,ShipModule.ModuleTypes.PowerPlant,5,0,"Power Plant Class 4 Rating C", PowerGen:13, HeatEfficiency:0.5 ) },
            { "int_powerplant_size4_class4", new ShipModule(128064046,ShipModule.ModuleTypes.PowerPlant,8,0,"Power Plant Class 4 Rating B", PowerGen:14.3, HeatEfficiency:0.45 ) },
            { "int_powerplant_size4_class5", new ShipModule(128064047,ShipModule.ModuleTypes.PowerPlant,5,0,"Power Plant Class 4 Rating A", PowerGen:15.6, HeatEfficiency:0.4 ) },
            { "int_powerplant_size5_class1", new ShipModule(128064048,ShipModule.ModuleTypes.PowerPlant,20,0,"Power Plant Class 5 Rating E", PowerGen:13.6, HeatEfficiency:1 ) },
            { "int_powerplant_size5_class2", new ShipModule(128064049,ShipModule.ModuleTypes.PowerPlant,8,0,"Power Plant Class 5 Rating D", PowerGen:15.3, HeatEfficiency:0.75 ) },
            { "int_powerplant_size5_class3", new ShipModule(128064050,ShipModule.ModuleTypes.PowerPlant,10,0,"Power Plant Class 5 Rating C", PowerGen:17, HeatEfficiency:0.5 ) },
            { "int_powerplant_size5_class4", new ShipModule(128064051,ShipModule.ModuleTypes.PowerPlant,16,0,"Power Plant Class 5 Rating B", PowerGen:18.7, HeatEfficiency:0.45 ) },
            { "int_powerplant_size5_class5", new ShipModule(128064052,ShipModule.ModuleTypes.PowerPlant,10,0,"Power Plant Class 5 Rating A", PowerGen:20.4, HeatEfficiency:0.4 ) },
            { "int_powerplant_size6_class1", new ShipModule(128064053,ShipModule.ModuleTypes.PowerPlant,40,0,"Power Plant Class 6 Rating E", PowerGen:16.8, HeatEfficiency:1 ) },
            { "int_powerplant_size6_class2", new ShipModule(128064054,ShipModule.ModuleTypes.PowerPlant,16,0,"Power Plant Class 6 Rating D", PowerGen:18.9, HeatEfficiency:0.75 ) },
            { "int_powerplant_size6_class3", new ShipModule(128064055,ShipModule.ModuleTypes.PowerPlant,20,0,"Power Plant Class 6 Rating C", PowerGen:21, HeatEfficiency:0.5 ) },
            { "int_powerplant_size6_class4", new ShipModule(128064056,ShipModule.ModuleTypes.PowerPlant,32,0,"Power Plant Class 6 Rating B", PowerGen:23.1, HeatEfficiency:0.45 ) },
            { "int_powerplant_size6_class5", new ShipModule(128064057,ShipModule.ModuleTypes.PowerPlant,20,0,"Power Plant Class 6 Rating A", PowerGen:25.2, HeatEfficiency:0.4 ) },
            { "int_powerplant_size7_class1", new ShipModule(128064058,ShipModule.ModuleTypes.PowerPlant,80,0,"Power Plant Class 7 Rating E", PowerGen:20, HeatEfficiency:1 ) },
            { "int_powerplant_size7_class2", new ShipModule(128064059,ShipModule.ModuleTypes.PowerPlant,32,0,"Power Plant Class 7 Rating D", PowerGen:22.5, HeatEfficiency:0.75 ) },
            { "int_powerplant_size7_class3", new ShipModule(128064060,ShipModule.ModuleTypes.PowerPlant,40,0,"Power Plant Class 7 Rating C", PowerGen:25, HeatEfficiency:0.5 ) },
            { "int_powerplant_size7_class4", new ShipModule(128064061,ShipModule.ModuleTypes.PowerPlant,64,0,"Power Plant Class 7 Rating B", PowerGen:27.5, HeatEfficiency:0.45 ) },
            { "int_powerplant_size7_class5", new ShipModule(128064062,ShipModule.ModuleTypes.PowerPlant,40,0,"Power Plant Class 7 Rating A", PowerGen:30, HeatEfficiency:0.4 ) },
            { "int_powerplant_size8_class1", new ShipModule(128064063,ShipModule.ModuleTypes.PowerPlant,160,0,"Power Plant Class 8 Rating E", PowerGen:24, HeatEfficiency:1 ) },
            { "int_powerplant_size8_class2", new ShipModule(128064064,ShipModule.ModuleTypes.PowerPlant,64,0,"Power Plant Class 8 Rating D", PowerGen:27, HeatEfficiency:0.75 ) },
            { "int_powerplant_size8_class3", new ShipModule(128064065,ShipModule.ModuleTypes.PowerPlant,80,0,"Power Plant Class 8 Rating C", PowerGen:30, HeatEfficiency:0.5 ) },
            { "int_powerplant_size8_class4", new ShipModule(128064066,ShipModule.ModuleTypes.PowerPlant,128,0,"Power Plant Class 8 Rating B", PowerGen:33, HeatEfficiency:0.45 ) },
            { "int_powerplant_size8_class5", new ShipModule(128064067,ShipModule.ModuleTypes.PowerPlant,80,0,"Power Plant Class 8 Rating A", PowerGen:36, HeatEfficiency:0.4 ) },
            { "int_powerplant_size2_class1_free", new ShipModule(128666635,ShipModule.ModuleTypes.PowerPlant,2.5,0,"Power Plant Class 2 Rating E",PowerGen:6.4 ) },

            // Pulse laser

            { "hpt_pulselaser_fixed_small", new ShipModule(128049381,ShipModule.ModuleTypes.PulseLaser,2,0.39,"Pulse Laser Fixed Small", Damage:2.05, Range:3000, FallOff:500, RateOfFire:3.85, BurstInterval:0.26, ThermL:0.33 ) },
            { "hpt_pulselaser_fixed_medium", new ShipModule(128049382,ShipModule.ModuleTypes.PulseLaser,4,0.6,"Pulse Laser Fixed Medium", Damage:3.5, Range:3000, FallOff:500, RateOfFire:3.45, BurstInterval:0.29, ThermL:0.56 ) },
            { "hpt_pulselaser_fixed_large", new ShipModule(128049383,ShipModule.ModuleTypes.PulseLaser,8,0.9,"Pulse Laser Fixed Large", Damage:5.98, Range:3000, FallOff:500, RateOfFire:3.03, BurstInterval:0.33, ThermL:0.96 ) },
            { "hpt_pulselaser_fixed_huge", new ShipModule(128049384,ShipModule.ModuleTypes.PulseLaser,16,1.33,"Pulse Laser Fixed Huge", Damage:10.24, Range:3000, FallOff:500, RateOfFire:2.63, BurstInterval:0.38, ThermL:1.64 ) },
            { "hpt_pulselaser_gimbal_small", new ShipModule(128049385,ShipModule.ModuleTypes.PulseLaser,2,0.39,"Pulse Laser Gimbal Small", Damage:1.56, Range:3000, FallOff:500, RateOfFire:4, BurstInterval:0.25, ThermL:0.31 ) },
            { "hpt_pulselaser_gimbal_medium", new ShipModule(128049386,ShipModule.ModuleTypes.PulseLaser,4,0.6,"Pulse Laser Gimbal Medium", Damage:2.68, Range:3000, FallOff:500, RateOfFire:3.57, BurstInterval:0.28, ThermL:0.54 ) },
            { "hpt_pulselaser_gimbal_large", new ShipModule(128049387,ShipModule.ModuleTypes.PulseLaser,8,0.92,"Pulse Laser Gimbal Large", Damage:4.58, Range:3000, FallOff:500, RateOfFire:3.23, BurstInterval:0.31, ThermL:0.92 ) },
            { "hpt_pulselaser_turret_small", new ShipModule(128049388,ShipModule.ModuleTypes.PulseLaser,2,0.38,"Pulse Laser Turret Small", Damage:1.19, Range:3000, FallOff:500, RateOfFire:3.33, BurstInterval:0.3, ThermL:0.19 ) },
            { "hpt_pulselaser_turret_medium", new ShipModule(128049389,ShipModule.ModuleTypes.PulseLaser,4,0.58,"Pulse Laser Turret Medium", Damage:2.05, Range:3000, FallOff:500, RateOfFire:3.03, BurstInterval:0.33, ThermL:0.33 ) },
            { "hpt_pulselaser_turret_large", new ShipModule(128049390,ShipModule.ModuleTypes.PulseLaser,8,0.89,"Pulse Laser Turret Large", Damage:3.5, Range:3000, FallOff:500, RateOfFire:2.7, BurstInterval:0.37, ThermL:0.56 ) },
                                                                                                                                           

            { "hpt_pulselaser_gimbal_huge", new ShipModule(128681995,ShipModule.ModuleTypes.PulseLaser,16,1.37,"Pulse Laser Gimbal Huge", Damage:7.82, Range:3000, FallOff:500, RateOfFire:2.78, BurstInterval:0.36, ThermL:1.56 ) },

            { "hpt_pulselaser_fixed_smallfree", new ShipModule(128049673,ShipModule.ModuleTypes.PulseLaser,1,0.4,"Pulse Laser Fixed Small Free", Damage:2.05, Range:3000, FallOff:500, RateOfFire:3.85, BurstInterval:0.26, ThermL:0.33 ) },
            { "hpt_pulselaser_fixed_medium_disruptor", new ShipModule(128671342,ShipModule.ModuleTypes.PulseDisruptorLaser,4,0.7,"Pulse Disruptor Laser Fixed Medium", Damage:2.8, Range:3000, FallOff:500, RateOfFire:1.67, BurstInterval:0.6, ThermL:1 ) },

            // Pulse Wave Analyser

            { "hpt_mrascanner_size0_class1", new ShipModule(128915718,ShipModule.ModuleTypes.PulseWaveAnalyser,1.3,0.2,"Pulse Wave Analyser Rating E", FacingLimit:15, Range:12000, Time:3 ) },
            { "hpt_mrascanner_size0_class2", new ShipModule(128915719,ShipModule.ModuleTypes.PulseWaveAnalyser,1.3,0.4,"Pulse Wave Analyser Rating D", FacingLimit:15, Range:15000, Time:3 ) },
            { "hpt_mrascanner_size0_class3", new ShipModule(128915720,ShipModule.ModuleTypes.PulseWaveAnalyser,1.3,0.8,"Pulse Wave Analyser Rating C", FacingLimit:15, Range:18000, Time:3 ) },
            { "hpt_mrascanner_size0_class4", new ShipModule(128915721,ShipModule.ModuleTypes.PulseWaveAnalyser,1.3,1.6,"Pulse Wave Analyser Rating B", FacingLimit:15, Range:21000, Time:3 ) },
            { "hpt_mrascanner_size0_class5", new ShipModule(128915722,ShipModule.ModuleTypes.PulseWaveAnalyser,1.3,3.2,"Pulse Wave Analyser Rating A", FacingLimit:15, Range:24000, Time:3 ) },

            // Rail guns

            { "hpt_railgun_fixed_small", new ShipModule(128049488,ShipModule.ModuleTypes.RailGun,2,1.15,"Rail Gun Fixed Small", Ammo:80, Clip:1, Damage:23.34, Range:3000, FallOff:1000, RateOfFire:1.59, BurstInterval:0.63, Reload:1, ThermL:12 ) },
            { "hpt_railgun_fixed_medium", new ShipModule(128049489,ShipModule.ModuleTypes.RailGun,4,1.63,"Rail Gun Fixed Medium", Ammo:80, Clip:1, Damage:41.53, Range:3000, FallOff:1000, RateOfFire:1.21, BurstInterval:0.83, Reload:1, ThermL:20 ) },
            { "hpt_railgun_fixed_medium_burst", new ShipModule(128671341,ShipModule.ModuleTypes.ImperialHammerRailGun,4,1.63,"Imperial Hammer Rail Gun Fixed Medium", Ammo:240, Clip:3, Damage:15, Range:3000, FallOff:1000, RateOfFire:4.09, BurstInterval:0.4, Reload:1.2, ThermL:11 ) },

            // Refineries

            { "int_refinery_size1_class1", new ShipModule(128666684,ShipModule.ModuleTypes.Refinery,0,0.14,"Refinery Class 1 Rating E", Bins:1 ) },
            { "int_refinery_size2_class1", new ShipModule(128666685,ShipModule.ModuleTypes.Refinery,0,0.17,"Refinery Class 2 Rating E", Bins:2 ) },
            { "int_refinery_size3_class1", new ShipModule(128666686,ShipModule.ModuleTypes.Refinery,0,0.2,"Refinery Class 3 Rating E", Bins:3 ) },
            { "int_refinery_size4_class1", new ShipModule(128666687,ShipModule.ModuleTypes.Refinery,0,0.25,"Refinery Class 4 Rating E", Bins:4 ) },
            { "int_refinery_size1_class2", new ShipModule(128666688,ShipModule.ModuleTypes.Refinery,0,0.18,"Refinery Class 1 Rating D", Bins:1 ) },
            { "int_refinery_size2_class2", new ShipModule(128666689,ShipModule.ModuleTypes.Refinery,0,0.22,"Refinery Class 2 Rating D", Bins:3 ) },
            { "int_refinery_size3_class2", new ShipModule(128666690,ShipModule.ModuleTypes.Refinery,0,0.27,"Refinery Class 3 Rating D", Bins:4 ) },
            { "int_refinery_size4_class2", new ShipModule(128666691,ShipModule.ModuleTypes.Refinery,0,0.33,"Refinery Class 4 Rating D", Bins:5 ) },
            { "int_refinery_size1_class3", new ShipModule(128666692,ShipModule.ModuleTypes.Refinery,0,0.23,"Refinery Class 1 Rating C", Bins:2 ) },
            { "int_refinery_size2_class3", new ShipModule(128666693,ShipModule.ModuleTypes.Refinery,0,0.28,"Refinery Class 2 Rating C", Bins:4 ) },
            { "int_refinery_size3_class3", new ShipModule(128666694,ShipModule.ModuleTypes.Refinery,0,0.34,"Refinery Class 3 Rating C", Bins:6 ) },
            { "int_refinery_size4_class3", new ShipModule(128666695,ShipModule.ModuleTypes.Refinery,0,0.41,"Refinery Class 4 Rating C", Bins:7 ) },
            { "int_refinery_size1_class4", new ShipModule(128666696,ShipModule.ModuleTypes.Refinery,0,0.28,"Refinery Class 1 Rating B", Bins:3 ) },
            { "int_refinery_size2_class4", new ShipModule(128666697,ShipModule.ModuleTypes.Refinery,0,0.34,"Refinery Class 2 Rating B", Bins:5 ) },
            { "int_refinery_size3_class4", new ShipModule(128666698,ShipModule.ModuleTypes.Refinery,0,0.41,"Refinery Class 3 Rating B", Bins:7 ) },
            { "int_refinery_size4_class4", new ShipModule(128666699,ShipModule.ModuleTypes.Refinery,0,0.49,"Refinery Class 4 Rating B", Bins:9 ) },
            { "int_refinery_size1_class5", new ShipModule(128666700,ShipModule.ModuleTypes.Refinery,0,0.32,"Refinery Class 1 Rating A", Bins:4 ) },
            { "int_refinery_size2_class5", new ShipModule(128666701,ShipModule.ModuleTypes.Refinery,0,0.39,"Refinery Class 2 Rating A", Bins:6 ) },
            { "int_refinery_size3_class5", new ShipModule(128666702,ShipModule.ModuleTypes.Refinery,0,0.48,"Refinery Class 3 Rating A", Bins:8 ) },
            { "int_refinery_size4_class5", new ShipModule(128666703,ShipModule.ModuleTypes.Refinery,0,0.57,"Refinery Class 4 Rating A", Bins:10 ) },

            // Sensors

            { "int_sensors_size1_class1", new ShipModule(128064218,ShipModule.ModuleTypes.Sensors,1.3,0.16,"Sensors Class 1 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4000 ) },
            { "int_sensors_size1_class2", new ShipModule(128064219,ShipModule.ModuleTypes.Sensors,0.5,0.18,"Sensors Class 1 Rating D", FacingLimit:30, Range:8000, TypicalEmission:4500 ) },
            { "int_sensors_size1_class3", new ShipModule(128064220,ShipModule.ModuleTypes.Sensors,1.3,0.2,"Sensors Class 1 Rating C", FacingLimit:30, Range:8000, TypicalEmission:5000 ) },
            { "int_sensors_size1_class4", new ShipModule(128064221,ShipModule.ModuleTypes.Sensors,2,0.33,"Sensors Class 1 Rating B", FacingLimit:30, Range:8000, TypicalEmission:5500 ) },
            { "int_sensors_size1_class5", new ShipModule(128064222,ShipModule.ModuleTypes.Sensors,1.3,0.6,"Sensors Class 1 Rating A", FacingLimit:30, Range:8000, TypicalEmission:6000 ) },
            { "int_sensors_size2_class1", new ShipModule(128064223,ShipModule.ModuleTypes.Sensors,2.5,0.18,"Sensors Class 2 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4160 ) },
            { "int_sensors_size2_class2", new ShipModule(128064224,ShipModule.ModuleTypes.Sensors,1,0.21,"Sensors Class 2 Rating D", FacingLimit:30, Range:8000, TypicalEmission:4680 ) },
            { "int_sensors_size2_class3", new ShipModule(128064225,ShipModule.ModuleTypes.Sensors,2.5,0.23,"Sensors Class 2 Rating C", FacingLimit:30, Range:8000, TypicalEmission:5200 ) },
            { "int_sensors_size2_class4", new ShipModule(128064226,ShipModule.ModuleTypes.Sensors,4,0.38,"Sensors Class 2 Rating B", FacingLimit:30, Range:8000, TypicalEmission:5720 ) },
            { "int_sensors_size2_class5", new ShipModule(128064227,ShipModule.ModuleTypes.Sensors,2.5,0.69,"Sensors Class 2 Rating A", FacingLimit:30, Range:8000, TypicalEmission:6240 ) },
            { "int_sensors_size3_class1", new ShipModule(128064228,ShipModule.ModuleTypes.Sensors,5,0.22,"Sensors Class 3 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4320 ) },
            { "int_sensors_size3_class2", new ShipModule(128064229,ShipModule.ModuleTypes.Sensors,2,0.25,"Sensors Class 3 Rating D", FacingLimit:30, Range:8000, TypicalEmission:4860 ) },
            { "int_sensors_size3_class3", new ShipModule(128064230,ShipModule.ModuleTypes.Sensors,5,0.28,"Sensors Class 3 Rating C", FacingLimit:30, Range:8000, TypicalEmission:5400 ) },
            { "int_sensors_size3_class4", new ShipModule(128064231,ShipModule.ModuleTypes.Sensors,8,0.46,"Sensors Class 3 Rating B", FacingLimit:30, Range:8000, TypicalEmission:5940 ) },
            { "int_sensors_size3_class5", new ShipModule(128064232,ShipModule.ModuleTypes.Sensors,5,0.84,"Sensors Class 3 Rating A", FacingLimit:30, Range:8000, TypicalEmission:6480 ) },
            { "int_sensors_size4_class1", new ShipModule(128064233,ShipModule.ModuleTypes.Sensors,10,0.27,"Sensors Class 4 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4480 ) },
            { "int_sensors_size4_class2", new ShipModule(128064234,ShipModule.ModuleTypes.Sensors,4,0.31,"Sensors Class 4 Rating D", FacingLimit:30, Range:8000, TypicalEmission:5040 ) },
            { "int_sensors_size4_class3", new ShipModule(128064235,ShipModule.ModuleTypes.Sensors,10,0.34,"Sensors Class 4 Rating C", FacingLimit:30, Range:8000, TypicalEmission:5600 ) },
            { "int_sensors_size4_class4", new ShipModule(128064236,ShipModule.ModuleTypes.Sensors,16,0.56,"Sensors Class 4 Rating B", FacingLimit:30, Range:8000, TypicalEmission:6160 ) },
            { "int_sensors_size4_class5", new ShipModule(128064237,ShipModule.ModuleTypes.Sensors,10,1.02,"Sensors Class 4 Rating A", FacingLimit:30, Range:8000, TypicalEmission:6720 ) },
            { "int_sensors_size5_class1", new ShipModule(128064238,ShipModule.ModuleTypes.Sensors,20,0.33,"Sensors Class 5 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4640 ) },
            { "int_sensors_size5_class2", new ShipModule(128064239,ShipModule.ModuleTypes.Sensors,8,0.37,"Sensors Class 5 Rating D", FacingLimit:30, Range:8000, TypicalEmission:5220 ) },
            { "int_sensors_size5_class3", new ShipModule(128064240,ShipModule.ModuleTypes.Sensors,20,0.41,"Sensors Class 5 Rating C", FacingLimit:30, Range:8000, TypicalEmission:5800 ) },
            { "int_sensors_size5_class4", new ShipModule(128064241,ShipModule.ModuleTypes.Sensors,32,0.68,"Sensors Class 5 Rating B", FacingLimit:30, Range:8000, TypicalEmission:6380 ) },
            { "int_sensors_size5_class5", new ShipModule(128064242,ShipModule.ModuleTypes.Sensors,20,1.23,"Sensors Class 5 Rating A", FacingLimit:30, Range:8000, TypicalEmission:6960 ) },
            { "int_sensors_size6_class1", new ShipModule(128064243,ShipModule.ModuleTypes.Sensors,40,0.4,"Sensors Class 6 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4800 ) },
            { "int_sensors_size6_class2", new ShipModule(128064244,ShipModule.ModuleTypes.Sensors,16,0.45,"Sensors Class 6 Rating D", FacingLimit:30, Range:8000, TypicalEmission:5400 ) },
            { "int_sensors_size6_class3", new ShipModule(128064245,ShipModule.ModuleTypes.Sensors,40,0.5,"Sensors Class 6 Rating C", FacingLimit:30, Range:8000, TypicalEmission:6000 ) },
            { "int_sensors_size6_class4", new ShipModule(128064246,ShipModule.ModuleTypes.Sensors,64,0.83,"Sensors Class 6 Rating B", FacingLimit:30, Range:8000, TypicalEmission:6600 ) },
            { "int_sensors_size6_class5", new ShipModule(128064247,ShipModule.ModuleTypes.Sensors,40,1.5,"Sensors Class 6 Rating A", FacingLimit:30, Range:8000, TypicalEmission:7200 ) },
            { "int_sensors_size7_class1", new ShipModule(128064248,ShipModule.ModuleTypes.Sensors,80,0.47,"Sensors Class 7 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4960 ) },
            { "int_sensors_size7_class2", new ShipModule(128064249,ShipModule.ModuleTypes.Sensors,32,0.53,"Sensors Class 7 Rating D", FacingLimit:30, Range:8000, TypicalEmission:5580 ) },
            { "int_sensors_size7_class3", new ShipModule(128064250,ShipModule.ModuleTypes.Sensors,80,0.59,"Sensors Class 7 Rating C", FacingLimit:30, Range:8000, TypicalEmission:6200 ) },
            { "int_sensors_size7_class4", new ShipModule(128064251,ShipModule.ModuleTypes.Sensors,128,0.97,"Sensors Class 7 Rating B", FacingLimit:30, Range:8000, TypicalEmission:6820 ) },
            { "int_sensors_size7_class5", new ShipModule(128064252,ShipModule.ModuleTypes.Sensors,80,1.77,"Sensors Class 7 Rating A", FacingLimit:30, Range:8000, TypicalEmission:7440 ) },
            { "int_sensors_size8_class1", new ShipModule(128064253,ShipModule.ModuleTypes.Sensors,160,0.55,"Sensors Class 8 Rating E", FacingLimit:30, Range:8000, TypicalEmission:5120 ) },
            { "int_sensors_size8_class2", new ShipModule(128064254,ShipModule.ModuleTypes.Sensors,64,0.62,"Sensors Class 8 Rating D", FacingLimit:30, Range:8000, TypicalEmission:5760 ) },
            { "int_sensors_size8_class3", new ShipModule(128064255,ShipModule.ModuleTypes.Sensors,160,0.69,"Sensors Class 8 Rating C", FacingLimit:30, Range:8000, TypicalEmission:6400 ) },
            { "int_sensors_size8_class4", new ShipModule(128064256,ShipModule.ModuleTypes.Sensors,256,1.14,"Sensors Class 8 Rating B", FacingLimit:30, Range:8000, TypicalEmission:7040 ) },
            { "int_sensors_size8_class5", new ShipModule(128064257,ShipModule.ModuleTypes.Sensors,160,2.07,"Sensors Class 8 Rating A", FacingLimit:30, Range:8000, TypicalEmission:7680 ) },

            { "int_sensors_size1_class1_free", new ShipModule(128666640,ShipModule.ModuleTypes.Sensors,1.3,0.16,"Sensors Class 1 Rating E", FacingLimit:30, Range:8000, TypicalEmission:4000 ) },

            // Shield Boosters

            { "hpt_shieldbooster_size0_class1", new ShipModule(128668532,ShipModule.ModuleTypes.ShieldBooster,0.5,0.2,"Shield Booster Rating E", Explosive:0, Kinetic:0, Thermal:0, ShieldReinforcement:4 ) },
            { "hpt_shieldbooster_size0_class2", new ShipModule(128668533,ShipModule.ModuleTypes.ShieldBooster,1,0.5,"Shield Booster Rating D", Explosive:0, Kinetic:0, Thermal:0, ShieldReinforcement:8 ) },
            { "hpt_shieldbooster_size0_class3", new ShipModule(128668534,ShipModule.ModuleTypes.ShieldBooster,2,0.7,"Shield Booster Rating C", Explosive:0, Kinetic:0, Thermal:0, ShieldReinforcement:12 ) },
            { "hpt_shieldbooster_size0_class4", new ShipModule(128668535,ShipModule.ModuleTypes.ShieldBooster,3,1,"Shield Booster Rating B", Explosive:0, Kinetic:0, Thermal:0, ShieldReinforcement:16 ) },
            { "hpt_shieldbooster_size0_class5", new ShipModule(128668536,ShipModule.ModuleTypes.ShieldBooster,3.5,1.2,"Shield Booster Rating A", Explosive:0, Kinetic:0, Thermal:0, ShieldReinforcement:20 ) },

            // cell banks

            { "int_shieldcellbank_size1_class1", new ShipModule(128064298,ShipModule.ModuleTypes.ShieldCellBank,1.3,0.41,"Shield Cell Bank Class 1 Rating E", Ammo:3, Clip:1, ThermL:170 ) },
            { "int_shieldcellbank_size1_class2", new ShipModule(128064299,ShipModule.ModuleTypes.ShieldCellBank,0.5,0.55,"Shield Cell Bank Class 1 Rating D", Ammo:1, Clip:1, ThermL:170 ) },
            { "int_shieldcellbank_size1_class3", new ShipModule(128064300,ShipModule.ModuleTypes.ShieldCellBank,1.3,0.69,"Shield Cell Bank Class 1 Rating C", Ammo:2, Clip:1, ThermL:170 ) },
            { "int_shieldcellbank_size1_class4", new ShipModule(128064301,ShipModule.ModuleTypes.ShieldCellBank,2,0.83,"Shield Cell Bank Class 1 Rating B", Ammo:3, Clip:1, ThermL:170 ) },
            { "int_shieldcellbank_size1_class5", new ShipModule(128064302,ShipModule.ModuleTypes.ShieldCellBank,1.3,0.97,"Shield Cell Bank Class 1 Rating A", Ammo:2, Clip:1, ThermL:170 ) },
            { "int_shieldcellbank_size2_class1", new ShipModule(128064303,ShipModule.ModuleTypes.ShieldCellBank,2.5,0.5,"Shield Cell Bank Class 2 Rating E", Ammo:4, Clip:1, ThermL:240 ) },
            { "int_shieldcellbank_size2_class2", new ShipModule(128064304,ShipModule.ModuleTypes.ShieldCellBank,1,0.67,"Shield Cell Bank Class 2 Rating D", Ammo:2, Clip:1, ThermL:240 ) },
            { "int_shieldcellbank_size2_class3", new ShipModule(128064305,ShipModule.ModuleTypes.ShieldCellBank,2.5,0.84,"Shield Cell Bank Class 2 Rating C", Ammo:3, Clip:1, ThermL:240 ) },
            { "int_shieldcellbank_size2_class4", new ShipModule(128064306,ShipModule.ModuleTypes.ShieldCellBank,4,1.01,"Shield Cell Bank Class 2 Rating B", Ammo:4, Clip:1, ThermL:240 ) },
            { "int_shieldcellbank_size2_class5", new ShipModule(128064307,ShipModule.ModuleTypes.ShieldCellBank,2.5,1.18,"Shield Cell Bank Class 2 Rating A", Ammo:3, Clip:1, ThermL:240 ) },
            { "int_shieldcellbank_size3_class1", new ShipModule(128064308,ShipModule.ModuleTypes.ShieldCellBank,5,0.61,"Shield Cell Bank Class 3 Rating E", Ammo:4, Clip:1, ThermL:340 ) },
            { "int_shieldcellbank_size3_class2", new ShipModule(128064309,ShipModule.ModuleTypes.ShieldCellBank,2,0.82,"Shield Cell Bank Class 3 Rating D", Ammo:2, Clip:1, ThermL:340 ) },
            { "int_shieldcellbank_size3_class3", new ShipModule(128064310,ShipModule.ModuleTypes.ShieldCellBank,5,1.02,"Shield Cell Bank Class 3 Rating C", Ammo:3, Clip:1, ThermL:340 ) },
            { "int_shieldcellbank_size3_class4", new ShipModule(128064311,ShipModule.ModuleTypes.ShieldCellBank,8,1.22,"Shield Cell Bank Class 3 Rating B", Ammo:4, Clip:1, ThermL:340 ) },
            { "int_shieldcellbank_size3_class5", new ShipModule(128064312,ShipModule.ModuleTypes.ShieldCellBank,5,1.43,"Shield Cell Bank Class 3 Rating A", Ammo:3, Clip:1, ThermL:340 ) },
            { "int_shieldcellbank_size4_class1", new ShipModule(128064313,ShipModule.ModuleTypes.ShieldCellBank,10,0.74,"Shield Cell Bank Class 4 Rating E", Ammo:4, Clip:1, ThermL:410 ) },
            { "int_shieldcellbank_size4_class2", new ShipModule(128064314,ShipModule.ModuleTypes.ShieldCellBank,4,0.98,"Shield Cell Bank Class 4 Rating D", Ammo:2, Clip:1, ThermL:410 ) },
            { "int_shieldcellbank_size4_class3", new ShipModule(128064315,ShipModule.ModuleTypes.ShieldCellBank,10,1.23,"Shield Cell Bank Class 4 Rating C", Ammo:3, Clip:1, ThermL:410 ) },
            { "int_shieldcellbank_size4_class4", new ShipModule(128064316,ShipModule.ModuleTypes.ShieldCellBank,16,1.48,"Shield Cell Bank Class 4 Rating B", Ammo:4, Clip:1, ThermL:410 ) },
            { "int_shieldcellbank_size4_class5", new ShipModule(128064317,ShipModule.ModuleTypes.ShieldCellBank,10,1.72,"Shield Cell Bank Class 4 Rating A", Ammo:3, Clip:1, ThermL:410 ) },
            { "int_shieldcellbank_size5_class1", new ShipModule(128064318,ShipModule.ModuleTypes.ShieldCellBank,20,0.9,"Shield Cell Bank Class 5 Rating E", Ammo:4, Clip:1, ThermL:540 ) },
            { "int_shieldcellbank_size5_class2", new ShipModule(128064319,ShipModule.ModuleTypes.ShieldCellBank,8,1.2,"Shield Cell Bank Class 5 Rating D", Ammo:2, Clip:1, ThermL:540 ) },
            { "int_shieldcellbank_size5_class3", new ShipModule(128064320,ShipModule.ModuleTypes.ShieldCellBank,20,1.5,"Shield Cell Bank Class 5 Rating C", Ammo:3, Clip:1, ThermL:540 ) },
            { "int_shieldcellbank_size5_class4", new ShipModule(128064321,ShipModule.ModuleTypes.ShieldCellBank,32,1.8,"Shield Cell Bank Class 5 Rating B", Ammo:4, Clip:1, ThermL:540 ) },
            { "int_shieldcellbank_size5_class5", new ShipModule(128064322,ShipModule.ModuleTypes.ShieldCellBank,20,2.1,"Shield Cell Bank Class 5 Rating A", Ammo:3, Clip:1, ThermL:540 ) },
            { "int_shieldcellbank_size6_class1", new ShipModule(128064323,ShipModule.ModuleTypes.ShieldCellBank,40,1.06,"Shield Cell Bank Class 6 Rating E", Ammo:5, Clip:1, ThermL:640 ) },
            { "int_shieldcellbank_size6_class2", new ShipModule(128064324,ShipModule.ModuleTypes.ShieldCellBank,16,1.42,"Shield Cell Bank Class 6 Rating D", Ammo:3, Clip:1, ThermL:640 ) },
            { "int_shieldcellbank_size6_class3", new ShipModule(128064325,ShipModule.ModuleTypes.ShieldCellBank,40,1.77,"Shield Cell Bank Class 6 Rating C", Ammo:4, Clip:1, ThermL:640 ) },
            { "int_shieldcellbank_size6_class4", new ShipModule(128064326,ShipModule.ModuleTypes.ShieldCellBank,64,2.12,"Shield Cell Bank Class 6 Rating B", Ammo:5, Clip:1, ThermL:640 ) },
            { "int_shieldcellbank_size6_class5", new ShipModule(128064327,ShipModule.ModuleTypes.ShieldCellBank,40,2.48,"Shield Cell Bank Class 6 Rating A", Ammo:4, Clip:1, ThermL:640 ) },
            { "int_shieldcellbank_size7_class1", new ShipModule(128064328,ShipModule.ModuleTypes.ShieldCellBank,80,1.24,"Shield Cell Bank Class 7 Rating E", Ammo:5, Clip:1, ThermL:720 ) },
            { "int_shieldcellbank_size7_class2", new ShipModule(128064329,ShipModule.ModuleTypes.ShieldCellBank,32,1.66,"Shield Cell Bank Class 7 Rating D", Ammo:3, Clip:1, ThermL:720 ) },
            { "int_shieldcellbank_size7_class3", new ShipModule(128064330,ShipModule.ModuleTypes.ShieldCellBank,80,2.07,"Shield Cell Bank Class 7 Rating C", Ammo:4, Clip:1, ThermL:720 ) },
            { "int_shieldcellbank_size7_class4", new ShipModule(128064331,ShipModule.ModuleTypes.ShieldCellBank,128,2.48,"Shield Cell Bank Class 7 Rating B", Ammo:5, Clip:1, ThermL:720 ) },
            { "int_shieldcellbank_size7_class5", new ShipModule(128064332,ShipModule.ModuleTypes.ShieldCellBank,80,2.9,"Shield Cell Bank Class 7 Rating A", Ammo:4, Clip:1, ThermL:720 ) },
            { "int_shieldcellbank_size8_class1", new ShipModule(128064333,ShipModule.ModuleTypes.ShieldCellBank,160,1.44,"Shield Cell Bank Class 8 Rating E", Ammo:5, Clip:1, ThermL:800 ) },
            { "int_shieldcellbank_size8_class2", new ShipModule(128064334,ShipModule.ModuleTypes.ShieldCellBank,64,1.92,"Shield Cell Bank Class 8 Rating D", Ammo:3, Clip:1, ThermL:800 ) },
            { "int_shieldcellbank_size8_class3", new ShipModule(128064335,ShipModule.ModuleTypes.ShieldCellBank,160,2.4,"Shield Cell Bank Class 8 Rating C", Ammo:4, Clip:1, ThermL:800 ) },
            { "int_shieldcellbank_size8_class4", new ShipModule(128064336,ShipModule.ModuleTypes.ShieldCellBank,256,2.88,"Shield Cell Bank Class 8 Rating B", Ammo:5, Clip:1, ThermL:800 ) },
            { "int_shieldcellbank_size8_class5", new ShipModule(128064337,ShipModule.ModuleTypes.ShieldCellBank,160,3.36,"Shield Cell Bank Class 8 Rating A", Ammo:4, Clip:1, ThermL:800 ) },

            // Shield Generators
            // ship.sheilds

            { "int_shieldgenerator_size1_class1", new ShipModule(128064258,ShipModule.ModuleTypes.ShieldGenerator,1.3,0.72,"Shield Generator Class 1 Rating E", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size1_class2", new ShipModule(128064259,ShipModule.ModuleTypes.ShieldGenerator,0.5,0.96,"Shield Generator Class 1 Rating E", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size1_class3", new ShipModule(128064260,ShipModule.ModuleTypes.ShieldGenerator,1.3,1.2,"Shield Generator Class 1 Rating E", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size1_class5", new ShipModule(128064262,ShipModule.ModuleTypes.ShieldGenerator,1.3,1.68,"Shield Generator Class 1 Rating A", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size2_class1", new ShipModule(128064263,ShipModule.ModuleTypes.ShieldGenerator,2.5,0.9,"Shield Generator Class 2 Rating E", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size2_class2", new ShipModule(128064264,ShipModule.ModuleTypes.ShieldGenerator,1,1.2,"Shield Generator Class 2 Rating D", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size2_class3", new ShipModule(128064265,ShipModule.ModuleTypes.ShieldGenerator,2.5,1.5,"Shield Generator Class 2 Rating C", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size2_class4", new ShipModule(128064266,ShipModule.ModuleTypes.ShieldGenerator,4,1.8,"Shield Generator Class 2 Rating B", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size2_class5", new ShipModule(128064267,ShipModule.ModuleTypes.ShieldGenerator,2.5,2.1,"Shield Generator Class 2 Rating A", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size3_class1", new ShipModule(128064268,ShipModule.ModuleTypes.ShieldGenerator,5,1.08,"Shield Generator Class 3 Rating E", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.87, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size3_class2", new ShipModule(128064269,ShipModule.ModuleTypes.ShieldGenerator,2,1.44,"Shield Generator Class 3 Rating D", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.87, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size3_class3", new ShipModule(128064270,ShipModule.ModuleTypes.ShieldGenerator,5,1.8,"Shield Generator Class 3 Rating C", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.87, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size3_class4", new ShipModule(128064271,ShipModule.ModuleTypes.ShieldGenerator,8,2.16,"Shield Generator Class 3 Rating B", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.87, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size3_class5", new ShipModule(128064272,ShipModule.ModuleTypes.ShieldGenerator,5,2.52,"Shield Generator Class 3 Rating A", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.87, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size4_class1", new ShipModule(128064273,ShipModule.ModuleTypes.ShieldGenerator,10,1.32,"Shield Generator Class 4 Rating E", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.53, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size4_class2", new ShipModule(128064274,ShipModule.ModuleTypes.ShieldGenerator,4,1.76,"Shield Generator Class 4 Rating D", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.53, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size4_class3", new ShipModule(128064275,ShipModule.ModuleTypes.ShieldGenerator,10,2.2,"Shield Generator Class 4 Rating C", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.53, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size4_class4", new ShipModule(128064276,ShipModule.ModuleTypes.ShieldGenerator,16,2.64,"Shield Generator Class 4 Rating B", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.53, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size4_class5", new ShipModule(128064277,ShipModule.ModuleTypes.ShieldGenerator,10,3.08,"Shield Generator Class 4 Rating A", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.53, MinStrength:70, OptStrength:120, MaxStrength:170 ) },

        //30550 : { mtype:'isg', cost:    63010, namekey:30110, name:'Shield Generator', class:5, rating:'E', mass: 20.00, integ: 77, pwrdraw:1.56, boottime:1, genminmass:203.0, genoptmass: 405.0, genmaxmass:1013.0, genminmul:30, genoptmul: 80, genmaxmul:130, genrate:1.0, bgenrate:3.75, /*thmload:1.2,*/ genpwr:0.6, kinres:40.0, thmres:-20.0, expres:50.0, axeres:95.0, limit:'isg', fdid:, fdname:'Int_ShieldGenerator_Size5_Class1', eddbid:1131 },
            { "int_shieldgenerator_size5_class1", new ShipModule(128064278,ShipModule.ModuleTypes.ShieldGenerator,20,1.56,"Shield Generator Class 5 Rating E", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.75, MinStrength:30, OptStrength:80, MaxStrength:130 ) },

            { "int_shieldgenerator_size5_class2", new ShipModule(128064279,ShipModule.ModuleTypes.ShieldGenerator,8,2.08,"Shield Generator Class 5 Rating D", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.75, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size5_class3", new ShipModule(128064280,ShipModule.ModuleTypes.ShieldGenerator,20,2.6,"Shield Generator Class 5 Rating C", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.75, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size5_class4", new ShipModule(128064281,ShipModule.ModuleTypes.ShieldGenerator,32,3.12,"Shield Generator Class 5 Rating B", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.75, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size5_class5", new ShipModule(128064282,ShipModule.ModuleTypes.ShieldGenerator,20,3.64,"Shield Generator Class 5 Rating A", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.75, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size6_class1", new ShipModule(128064283,ShipModule.ModuleTypes.ShieldGenerator,40,1.86,"Shield Generator Class 6 Rating E", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.3, BrokenRegenRate:5.33, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size6_class2", new ShipModule(128064284,ShipModule.ModuleTypes.ShieldGenerator,16,2.48,"Shield Generator Class 6 Rating D", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.3, BrokenRegenRate:5.33, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size6_class3", new ShipModule(128064285,ShipModule.ModuleTypes.ShieldGenerator,40,3.1,"Shield Generator Class 6 Rating C", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.3, BrokenRegenRate:5.33, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size6_class4", new ShipModule(128064286,ShipModule.ModuleTypes.ShieldGenerator,64,3.72,"Shield Generator Class 6 Rating B", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.3, BrokenRegenRate:5.33, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size6_class5", new ShipModule(128064287,ShipModule.ModuleTypes.ShieldGenerator,40,4.34,"Shield Generator Class 6 Rating A", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.3, BrokenRegenRate:5.33, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size7_class1", new ShipModule(128064288,ShipModule.ModuleTypes.ShieldGenerator,80,2.1,"Shield Generator Class 7 Rating E", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:7.33, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size7_class2", new ShipModule(128064289,ShipModule.ModuleTypes.ShieldGenerator,32,2.8,"Shield Generator Class 7 Rating D", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:7.33, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size7_class3", new ShipModule(128064290,ShipModule.ModuleTypes.ShieldGenerator,80,3.5,"Shield Generator Class 7 Rating C", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:7.33, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size7_class4", new ShipModule(128064291,ShipModule.ModuleTypes.ShieldGenerator,128,4.2,"Shield Generator Class 7 Rating B", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:7.33, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size7_class5", new ShipModule(128064292,ShipModule.ModuleTypes.ShieldGenerator,80,4.9,"Shield Generator Class 7 Rating A", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:7.33, MinStrength:70, OptStrength:120, MaxStrength:170 ) },
            { "int_shieldgenerator_size8_class1", new ShipModule(128064293,ShipModule.ModuleTypes.ShieldGenerator,160,2.4,"Shield Generator Class 8 Rating E", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.4, BrokenRegenRate:9.6, MinStrength:30, OptStrength:80, MaxStrength:130 ) },
            { "int_shieldgenerator_size8_class2", new ShipModule(128064294,ShipModule.ModuleTypes.ShieldGenerator,64,3.2,"Shield Generator Class 8 Rating D", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.4, BrokenRegenRate:9.6, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size8_class3", new ShipModule(128064295,ShipModule.ModuleTypes.ShieldGenerator,160,4,"Shield Generator Class 8 Rating C", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.4, BrokenRegenRate:9.6, MinStrength:50, OptStrength:100, MaxStrength:150 ) },
            { "int_shieldgenerator_size8_class4", new ShipModule(128064296,ShipModule.ModuleTypes.ShieldGenerator,256,4.8,"Shield Generator Class 8 Rating B", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.4, BrokenRegenRate:9.6, MinStrength:60, OptStrength:110, MaxStrength:160 ) },
            { "int_shieldgenerator_size8_class5", new ShipModule(128064297,ShipModule.ModuleTypes.ShieldGenerator,160,5.6,"Shield Generator Class 8 Rating A", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.4, BrokenRegenRate:9.6, MinStrength:70, OptStrength:120, MaxStrength:170 ) },

            { "int_shieldgenerator_size2_class1_free", new ShipModule(128666641,ShipModule.ModuleTypes.ShieldGenerator,2.5,0.9,"Shield Generator Class 2 Rating E",     OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.6, MinStrength:30, OptStrength:80, MaxStrength:130 ) },

            { "int_shieldgenerator_size1_class5_strong", new ShipModule(128671323,ShipModule.ModuleTypes.PrismaticShieldGenerator,2.6,2.52,"Prismatic Shield Generator Class 1 Rating A", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.2, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size2_class5_strong", new ShipModule(128671324,ShipModule.ModuleTypes.PrismaticShieldGenerator,5,3.15,"Prismatic Shield Generator Class 2 Rating A", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.2, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size3_class5_strong", new ShipModule(128671325,ShipModule.ModuleTypes.PrismaticShieldGenerator,10,3.78,"Prismatic Shield Generator Class 3 Rating A", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.3, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size4_class5_strong", new ShipModule(128671326,ShipModule.ModuleTypes.PrismaticShieldGenerator,20,4.62,"Prismatic Shield Generator Class 4 Rating A", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:1.66, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size5_class5_strong", new ShipModule(128671327,ShipModule.ModuleTypes.PrismaticShieldGenerator,40,5.46,"Prismatic Shield Generator Class 5 Rating A", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:2.34, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size6_class5_strong", new ShipModule(128671328,ShipModule.ModuleTypes.PrismaticShieldGenerator,80,6.51,"Prismatic Shield Generator Class 6 Rating A", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1, BrokenRegenRate:3.2, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size7_class5_strong", new ShipModule(128671329,ShipModule.ModuleTypes.PrismaticShieldGenerator,160,7.35,"Prismatic Shield Generator Class 7 Rating A", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.1, BrokenRegenRate:4.25, MinStrength:100, OptStrength:150, MaxStrength:200 ) },
            { "int_shieldgenerator_size8_class5_strong", new ShipModule(128671330,ShipModule.ModuleTypes.PrismaticShieldGenerator,320,8.4,"Prismatic Shield Generator Class 8 Rating A", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.4, BrokenRegenRate:5.4, MinStrength:100, OptStrength:150, MaxStrength:200 ) },

            { "int_shieldgenerator_size1_class3_fast", new ShipModule(128671331,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,1.3,1.2,"Bi Weave Shield Generator Class 1 Rating C", OptMass:25, MaxMass:63, MinMass:13, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:2.4, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size2_class3_fast", new ShipModule(128671332,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,2.5,1.5,"Bi Weave Shield Generator Class 2 Rating C", OptMass:55, MaxMass:138, MinMass:28, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:2.4, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size3_class3_fast", new ShipModule(128671333,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,5,1.8,"Bi Weave Shield Generator Class 3 Rating C", OptMass:165, MaxMass:413, MinMass:83, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:2.8, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size4_class3_fast", new ShipModule(128671334,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,10,2.2,"Bi Weave Shield Generator Class 4 Rating C", OptMass:285, MaxMass:713, MinMass:143, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:1.8, BrokenRegenRate:3.8, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size5_class3_fast", new ShipModule(128671335,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,20,2.6,"Bi Weave Shield Generator Class 5 Rating C", OptMass:405, MaxMass:1013, MinMass:203, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:2.2, BrokenRegenRate:5.63, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size6_class3_fast", new ShipModule(128671336,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,40,3.1,"Bi Weave Shield Generator Class 6 Rating C", OptMass:540, MaxMass:1350, MinMass:270, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:3.2, BrokenRegenRate:8, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size7_class3_fast", new ShipModule(128671337,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,80,3.5,"Bi Weave Shield Generator Class 7 Rating C", OptMass:1060, MaxMass:2650, MinMass:530, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:4.4, BrokenRegenRate:11, MinStrength:40, OptStrength:90, MaxStrength:140 ) },
            { "int_shieldgenerator_size8_class3_fast", new ShipModule(128671338,ShipModule.ModuleTypes.Bi_WeaveShieldGenerator,160,4,"Bi Weave Shield Generator Class 8 Rating C", OptMass:1800, MaxMass:4500, MinMass:900, Explosive:50, Kinetic:40, Thermal:-20, AXResistance:95, RegenRate:5.8, BrokenRegenRate:14.4, MinStrength:40, OptStrength:90, MaxStrength:140 ) },

            // shield shutdown neutraliser

            { "hpt_antiunknownshutdown_tiny", new ShipModule(128771884,ShipModule.ModuleTypes.ShutdownFieldNeutraliser,1.3,0.2,"Shutdown Field Neutraliser", Range:3000, Time:1, Reload:10 ) },
            { "hpt_antiunknownshutdown_tiny_v2", new ShipModule(129022663,ShipModule.ModuleTypes.ShutdownFieldNeutraliser,3,0.4,"Thargoid Pulse Neutraliser", Range:0, Time:2, Reload:10 ) },

            // weapon stabliser
            { "int_expmodulestabiliser_size3_class3", new ShipModule(129019260,ShipModule.ModuleTypes.ExperimentalWeaponStabiliser,8,1.5,"Exp Module Weapon Stabiliser Class 3 Rating F" ) },
            { "int_expmodulestabiliser_size5_class3", new ShipModule(129019261,ShipModule.ModuleTypes.ExperimentalWeaponStabiliser,20,3,"Exp Module Weapon Stabiliser Class 5 Rating F" ) },

            // supercruise
            { "int_supercruiseassist", new ShipModule(128932273,ShipModule.ModuleTypes.SupercruiseAssist,0,0.3,"Supercruise Assist" ) },

            // stellar scanners

            { "int_stellarbodydiscoveryscanner_standard_free", new ShipModule(128666642,ShipModule.ModuleTypes.DiscoveryScanner,2,0,"Stellar Body Discovery Scanner Standard",Range:500 ) },
            { "int_stellarbodydiscoveryscanner_standard", new ShipModule(128662535,ShipModule.ModuleTypes.DiscoveryScanner,2,0,"Stellar Body Discovery Scanner Standard",Range:500 ) },
            { "int_stellarbodydiscoveryscanner_intermediate", new ShipModule(128663560,ShipModule.ModuleTypes.DiscoveryScanner,2,0,"Stellar Body Discovery Scanner Intermediate",Range:1000 ) },
            { "int_stellarbodydiscoveryscanner_advanced", new ShipModule(128663561,ShipModule.ModuleTypes.DiscoveryScanner,2,0,"Stellar Body Discovery Scanner Advanced" ) },

            // thrusters

            { "int_engine_size2_class1", new ShipModule(128064068,ShipModule.ModuleTypes.Thrusters,2.5,2,"Thrusters Class 2 Rating E", OptMass:48, MaxMass:72, MinMass:24, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size2_class2", new ShipModule(128064069,ShipModule.ModuleTypes.Thrusters,1,2.25,"Thrusters Class 2 Rating D", OptMass:54, MaxMass:81, MinMass:27, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size2_class3", new ShipModule(128064070,ShipModule.ModuleTypes.Thrusters,2.5,2.5,"Thrusters Class 2 Rating C", OptMass:60, MaxMass:90, MinMass:30, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size2_class4", new ShipModule(128064071,ShipModule.ModuleTypes.Thrusters,4,2.75,"Thrusters Class 2 Rating B", OptMass:66, MaxMass:99, MinMass:33, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size2_class5", new ShipModule(128064072,ShipModule.ModuleTypes.Thrusters,2.5,3,"Thrusters Class 2 Rating A", OptMass:72, MaxMass:108, MinMass:36, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size3_class1", new ShipModule(128064073,ShipModule.ModuleTypes.Thrusters,5,2.48,"Thrusters Class 3 Rating E", OptMass:80, MaxMass:120, MinMass:40, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size3_class2", new ShipModule(128064074,ShipModule.ModuleTypes.Thrusters,2,2.79,"Thrusters Class 3 Rating D", OptMass:90, MaxMass:135, MinMass:45, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size3_class3", new ShipModule(128064075,ShipModule.ModuleTypes.Thrusters,5,3.1,"Thrusters Class 3 Rating C", OptMass:100, MaxMass:150, MinMass:50, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size3_class4", new ShipModule(128064076,ShipModule.ModuleTypes.Thrusters,8,3.41,"Thrusters Class 3 Rating B", OptMass:110, MaxMass:165, MinMass:55, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size3_class5", new ShipModule(128064077,ShipModule.ModuleTypes.Thrusters,5,3.72,"Thrusters Class 3 Rating A", OptMass:120, MaxMass:180, MinMass:60, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size4_class1", new ShipModule(128064078,ShipModule.ModuleTypes.Thrusters,10,3.28,"Thrusters Class 4 Rating E", OptMass:280, MaxMass:420, MinMass:140, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size4_class2", new ShipModule(128064079,ShipModule.ModuleTypes.Thrusters,4,3.69,"Thrusters Class 4 Rating D", OptMass:315, MaxMass:473, MinMass:158, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size4_class3", new ShipModule(128064080,ShipModule.ModuleTypes.Thrusters,10,4.1,"Thrusters Class 4 Rating C", OptMass:350, MaxMass:525, MinMass:175, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size4_class4", new ShipModule(128064081,ShipModule.ModuleTypes.Thrusters,16,4.51,"Thrusters Class 4 Rating B", OptMass:385, MaxMass:578, MinMass:193, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size4_class5", new ShipModule(128064082,ShipModule.ModuleTypes.Thrusters,10,4.92,"Thrusters Class 4 Rating A", OptMass:420, MaxMass:630, MinMass:210, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size5_class1", new ShipModule(128064083,ShipModule.ModuleTypes.Thrusters,20,4.08,"Thrusters Class 5 Rating E", OptMass:560, MaxMass:840, MinMass:280, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size5_class2", new ShipModule(128064084,ShipModule.ModuleTypes.Thrusters,8,4.59,"Thrusters Class 5 Rating D", OptMass:630, MaxMass:945, MinMass:315, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size5_class3", new ShipModule(128064085,ShipModule.ModuleTypes.Thrusters,20,5.1,"Thrusters Class 5 Rating C", OptMass:700, MaxMass:1050, MinMass:350, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size5_class4", new ShipModule(128064086,ShipModule.ModuleTypes.Thrusters,32,5.61,"Thrusters Class 5 Rating B", OptMass:770, MaxMass:1155, MinMass:385, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size5_class5", new ShipModule(128064087,ShipModule.ModuleTypes.Thrusters,20,6.12,"Thrusters Class 5 Rating A", OptMass:840, MaxMass:1260, MinMass:420, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size6_class1", new ShipModule(128064088,ShipModule.ModuleTypes.Thrusters,40,5.04,"Thrusters Class 6 Rating E", OptMass:960, MaxMass:1440, MinMass:480, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size6_class2", new ShipModule(128064089,ShipModule.ModuleTypes.Thrusters,16,5.67,"Thrusters Class 6 Rating D", OptMass:1080, MaxMass:1620, MinMass:540, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size6_class3", new ShipModule(128064090,ShipModule.ModuleTypes.Thrusters,40,6.3,"Thrusters Class 6 Rating C", OptMass:1200, MaxMass:1800, MinMass:600, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size6_class4", new ShipModule(128064091,ShipModule.ModuleTypes.Thrusters,64,6.93,"Thrusters Class 6 Rating B", OptMass:1320, MaxMass:1980, MinMass:660, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size6_class5", new ShipModule(128064092,ShipModule.ModuleTypes.Thrusters,40,7.56,"Thrusters Class 6 Rating A", OptMass:1440, MaxMass:2160, MinMass:720, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size7_class1", new ShipModule(128064093,ShipModule.ModuleTypes.Thrusters,80,6.08,"Thrusters Class 7 Rating E", OptMass:1440, MaxMass:2160, MinMass:720, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size7_class2", new ShipModule(128064094,ShipModule.ModuleTypes.Thrusters,32,6.84,"Thrusters Class 7 Rating D", OptMass:1620, MaxMass:2430, MinMass:810, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size7_class3", new ShipModule(128064095,ShipModule.ModuleTypes.Thrusters,80,7.6,"Thrusters Class 7 Rating C", OptMass:1800, MaxMass:2700, MinMass:900, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size7_class4", new ShipModule(128064096,ShipModule.ModuleTypes.Thrusters,128,8.36,"Thrusters Class 7 Rating B", OptMass:1980, MaxMass:2970, MinMass:990, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size7_class5", new ShipModule(128064097,ShipModule.ModuleTypes.Thrusters,80,9.12,"Thrusters Class 7 Rating A", OptMass:2160, MaxMass:3240, MinMass:1080, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },
            { "int_engine_size8_class1", new ShipModule(128064098,ShipModule.ModuleTypes.Thrusters,160,7.2,"Thrusters Class 8 Rating E", OptMass:2240, MaxMass:3360, MinMass:1120, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },
            { "int_engine_size8_class2", new ShipModule(128064099,ShipModule.ModuleTypes.Thrusters,64,8.1,"Thrusters Class 8 Rating D", OptMass:2520, MaxMass:3780, MinMass:1260, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:86, EngineMaxMultiplier:106 ) },
            { "int_engine_size8_class3", new ShipModule(128064100,ShipModule.ModuleTypes.Thrusters,160,9,"Thrusters Class 8 Rating C", OptMass:2800, MaxMass:4200, MinMass:1400, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:90, EngineMaxMultiplier:110 ) },
            { "int_engine_size8_class4", new ShipModule(128064101,ShipModule.ModuleTypes.Thrusters,256,9.9,"Thrusters Class 8 Rating B", OptMass:3080, MaxMass:4620, MinMass:1540, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:93, EngineMaxMultiplier:113 ) },
            { "int_engine_size8_class5", new ShipModule(128064102,ShipModule.ModuleTypes.Thrusters,160,10.8,"Thrusters Class 8 Rating A", OptMass:3360, MaxMass:5040, MinMass:1680, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:96, EngineMaxMultiplier:116 ) },

            { "int_engine_size2_class1_free", new ShipModule(128666636,ShipModule.ModuleTypes.Thrusters,2.5,2,"Thrusters Class 2 Rating E", OptMass:48, MaxMass:72, MinMass:24, ThermL:1.3, EngineOptMultiplier:100, EngineMinMultiplier:83, EngineMaxMultiplier:103 ) },

            { "int_engine_size3_class5_fast", new ShipModule(128682013,ShipModule.ModuleTypes.EnhancedPerformanceThrusters,5,5,"Enhanced Performance Thrusters Class 3 Rating A", OptMass:90, MaxMass:200, MinMass:70, ThermL:1.3, EngineOptMultiplier:115, EngineMinMultiplier:90, EngineMaxMultiplier:137 ) },
            { "int_engine_size2_class5_fast", new ShipModule(128682014,ShipModule.ModuleTypes.EnhancedPerformanceThrusters,2.5,4,"Enhanced Performance Thrusters Class 2 Rating A", OptMass:60, MaxMass:120, MinMass:50, ThermL:2, EngineOptMultiplier:115, EngineMinMultiplier:90, EngineMaxMultiplier:137 ) },

            // XENO Scanners

            { "hpt_xenoscanner_basic_tiny", new ShipModule(128793115,ShipModule.ModuleTypes.XenoScanner,1.3,0.2,"Xeno Scanner", FacingLimit:23, Range:500, Time:10 ) },
            { "hpt_xenoscannermk2_basic_tiny", new ShipModule(128808878,ShipModule.ModuleTypes.EnhancedXenoScanner,1.3,0.8,"Enhanced Xeno Scanner", FacingLimit:23, Range:2000, Time:10 ) },
            { "hpt_xenoscanner_advanced_tiny", new ShipModule(129022952,ShipModule.ModuleTypes.EnhancedXenoScanner,3,1,"Pulse Wave Xeno Scanner", FacingLimit:23, Range:1000, Time:10 ) },




        };

// non buyable

public static Dictionary<string, ShipModule> othershipmodules = new Dictionary<string, ShipModule>
        {
        { "adder_cockpit", new ShipModule(999999913,ShipModule.ModuleTypes.CockpitType,0,0,"Adder Cockpit" ) },
            { "typex_3_cockpit", new ShipModule(999999945,ShipModule.ModuleTypes.CockpitType,0,0,"Alliance Challenger Cockpit" ) },
            { "typex_cockpit", new ShipModule(999999943,ShipModule.ModuleTypes.CockpitType,0,0,"Alliance Chieftain Cockpit" ) },
            { "anaconda_cockpit", new ShipModule(999999926,ShipModule.ModuleTypes.CockpitType,0,0,"Anaconda Cockpit" ) },
            { "asp_cockpit", new ShipModule(999999918,ShipModule.ModuleTypes.CockpitType,0,0,"Asp Cockpit" ) },
            { "asp_scout_cockpit", new ShipModule(999999934,ShipModule.ModuleTypes.CockpitType,0,0,"Asp Scout Cockpit" ) },
            { "belugaliner_cockpit", new ShipModule(999999938,ShipModule.ModuleTypes.CockpitType,0,0,"Beluga Cockpit" ) },
            { "cobramkiii_cockpit", new ShipModule(999999915,ShipModule.ModuleTypes.CockpitType,0,0,"Cobra Mk III Cockpit" ) },
            { "cobramkiv_cockpit", new ShipModule(999999937,ShipModule.ModuleTypes.CockpitType,0,0,"Cobra Mk IV Cockpit" ) },
            { "cutter_cockpit", new ShipModule(999999932,ShipModule.ModuleTypes.CockpitType,0,0,"Cutter Cockpit" ) },
            { "diamondbackxl_cockpit", new ShipModule(999999928,ShipModule.ModuleTypes.CockpitType,0,0,"Diamondback Explorer Cockpit" ) },
            { "diamondback_cockpit", new ShipModule(999999927,ShipModule.ModuleTypes.CockpitType,0,0,"Diamondback Scout Cockpit" ) },
            { "dolphin_cockpit", new ShipModule(999999939,ShipModule.ModuleTypes.CockpitType,0,0,"Dolphin Cockpit" ) },
            { "eagle_cockpit", new ShipModule(999999911,ShipModule.ModuleTypes.CockpitType,0,0,"Eagle Cockpit" ) },
            { "empire_courier_cockpit", new ShipModule(999999909,ShipModule.ModuleTypes.CockpitType,0,0,"Empire Courier Cockpit" ) },
            { "empire_eagle_cockpit", new ShipModule(999999929,ShipModule.ModuleTypes.CockpitType,0,0,"Empire Eagle Cockpit" ) },
            { "empire_fighter_cockpit", new ShipModule(899990000,ShipModule.ModuleTypes.CockpitType,0,0,"Empire Fighter Cockpit" ) },
            { "empire_trader_cockpit", new ShipModule(999999920,ShipModule.ModuleTypes.CockpitType,0,0,"Empire Trader Cockpit" ) },
            { "federation_corvette_cockpit", new ShipModule(999999933,ShipModule.ModuleTypes.CockpitType,0,0,"Federal Corvette Cockpit" ) },
            { "federation_dropship_mkii_cockpit", new ShipModule(999999930,ShipModule.ModuleTypes.CockpitType,0,0,"Federal Dropship Cockpit" ) },
            { "federation_dropship_cockpit", new ShipModule(999999921,ShipModule.ModuleTypes.CockpitType,0,0,"Federal Gunship Cockpit" ) },
            { "federation_gunship_cockpit", new ShipModule(999999931,ShipModule.ModuleTypes.CockpitType,0,0,"Federal Gunship Cockpit" ) },
            { "federation_fighter_cockpit", new ShipModule(899990001,ShipModule.ModuleTypes.CockpitType,0,0,"Federation Fighter Cockpit" ) },
            { "ferdelance_cockpit", new ShipModule(999999925,ShipModule.ModuleTypes.CockpitType,0,0,"Fer De Lance Cockpit" ) },
            { "hauler_cockpit", new ShipModule(999999912,ShipModule.ModuleTypes.CockpitType,0,0,"Hauler Cockpit" ) },
            { "independant_trader_cockpit", new ShipModule(999999936,ShipModule.ModuleTypes.CockpitType,0,0,"Independant Trader Cockpit" ) },
            { "independent_fighter_cockpit", new ShipModule(899990002,ShipModule.ModuleTypes.CockpitType,0,0,"Independent Fighter Cockpit" ) },
            { "krait_light_cockpit", new ShipModule(999999948,ShipModule.ModuleTypes.CockpitType,0,0,"Krait Light Cockpit" ) },
            { "krait_mkii_cockpit", new ShipModule(999999946,ShipModule.ModuleTypes.CockpitType,0,0,"Krait MkII Cockpit" ) },
            { "mamba_cockpit", new ShipModule(999999949,ShipModule.ModuleTypes.CockpitType,0,0,"Mamba Cockpit" ) },
            { "orca_cockpit", new ShipModule(999999922,ShipModule.ModuleTypes.CockpitType,0,0,"Orca Cockpit" ) },
            { "python_cockpit", new ShipModule(999999924,ShipModule.ModuleTypes.CockpitType,0,0,"Python Cockpit" ) },
            { "python_nx_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,0,0,"Python Nx Cockpit" ) },
            { "sidewinder_cockpit", new ShipModule(999999910,ShipModule.ModuleTypes.CockpitType,0,0,"Sidewinder Cockpit" ) },
            { "type6_cockpit", new ShipModule(999999916,ShipModule.ModuleTypes.CockpitType,0,0,"Type 6 Cockpit" ) },
            { "type7_cockpit", new ShipModule(999999917,ShipModule.ModuleTypes.CockpitType,0,0,"Type 7 Cockpit" ) },
            { "type9_cockpit", new ShipModule(999999923,ShipModule.ModuleTypes.CockpitType,0,0,"Type 9 Cockpit" ) },
            { "type9_military_cockpit", new ShipModule(999999942,ShipModule.ModuleTypes.CockpitType,0,0,"Type 9 Military Cockpit" ) },
            { "typex_2_cockpit", new ShipModule(999999950,ShipModule.ModuleTypes.CockpitType,0,0,"Typex 2 Cockpit" ) },
            { "viper_cockpit", new ShipModule(999999914,ShipModule.ModuleTypes.CockpitType,0,0,"Viper Cockpit" ) },
            { "viper_mkiv_cockpit", new ShipModule(999999935,ShipModule.ModuleTypes.CockpitType,0,0,"Viper Mk IV Cockpit" ) },
            { "vulture_cockpit", new ShipModule(999999919,ShipModule.ModuleTypes.CockpitType,0,0,"Vulture Cockpit" ) },

            { "int_codexscanner", new ShipModule(999999947,ShipModule.ModuleTypes.Codex,0,0,"Codex Scanner" ) },
            { "hpt_shipdatalinkscanner", new ShipModule(999999940,ShipModule.ModuleTypes.DataLinkScanner,0,0,"Hpt Shipdatalinkscanner" ) },

            { "int_passengercabin_size2_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,2.5,0,"Prison Cell",Prisoners:2 ) },
            { "int_passengercabin_size3_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,5,0,"Prison Cell",Prisoners:4 ) },
            { "int_passengercabin_size4_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,10,0,"Prison Cell",Prisoners:8 ) },
            { "int_passengercabin_size5_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,20,0,"Prison Cell",Prisoners:16 ) },
            { "int_passengercabin_size6_class0", new ShipModule(-1,ShipModule.ModuleTypes.PrisonCells,40,0,"Prison Cell",Prisoners:32 ) },

            { "hpt_cannon_turret_huge", new ShipModule(-1,ShipModule.ModuleTypes.Cannon,1,0.9,"Cannon Turret Huge" ) },

            { "modularcargobaydoorfdl", new ShipModule(999999907,ShipModule.ModuleTypes.CargoBayDoorType,0,0,"FDL Cargo Bay Door" ) },
            { "modularcargobaydoor", new ShipModule(999999908,ShipModule.ModuleTypes.CargoBayDoorType,0,0,"Modular Cargo Bay Door" ) },

            { "hpt_cargoscanner_basic_tiny", new ShipModule(-1,ShipModule.ModuleTypes.CargoScanner,0,0,"Manifest Scanner Basic" ) },

           // { "int_corrosionproofcargorack_size2_class1", new ShipModule(-1,0,0,null,"Anti Corrosion Cargo Rack",ShipModule.ModuleTypes.CargoRack) },
           // { "hpt_plasmaburstcannon_fixed_medium", new ShipModule(-1,1,1.4,null,"Plasma Burst Cannon Fixed Medium","Plasma Accelerator") },      // no evidence
           // { "hpt_pulselaserstealth_fixed_small", new ShipModule(-1,1,0.2,null,"Pulse Laser Stealth Fixed Small",ShipModule.ModuleTypes.PulseLaser) },
            ///{ "int_shieldgenerator_size1_class4", new ShipModule(-1,2,1.44,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },

        };

#endregion

#region Fighters

        public static Dictionary<string, ShipModule> fightermodules = new Dictionary<string, ShipModule>
        {
            { "hpt_guardiangauss_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,1,1,"Guardian Gauss Fixed GDN Fighter") },
            { "hpt_guardianplasma_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,1,1,"Guardian Plasma Fixed GDN Fighter") },
            { "hpt_guardianshard_fixed_gdn_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.FighterWeapon,1,1,"Guardian Shard Fixed GDN Fighter") },

            { "empire_fighter_armour_standard", new ShipModule(899990059,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Empire Fighter Armour Standard") },
            { "federation_fighter_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Federation Fighter Armour Standard") },
            { "independent_fighter_armour_standard", new ShipModule(899990070,ShipModule.ModuleTypes.LightweightAlloy,0,0,"Independent Fighter Armour Standard") },
            { "gdn_hybrid_fighter_v1_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,0,0,"GDN Hybrid Fighter V 1 Armour Standard") },
            { "gdn_hybrid_fighter_v2_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,0,0,"GDN Hybrid Fighter V 2 Armour Standard") },
            { "gdn_hybrid_fighter_v3_armour_standard", new ShipModule(899990060,ShipModule.ModuleTypes.LightweightAlloy,0,0,"GDN Hybrid Fighter V 3 Armour Standard") },

            { "hpt_beamlaser_fixed_empire_fighter", new ShipModule(899990018,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Fixed Empire Fighter") },
            { "hpt_beamlaser_fixed_fed_fighter", new ShipModule(899990019,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Fixed Federation Fighter") },
            { "hpt_beamlaser_fixed_indie_fighter", new ShipModule(899990020,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Fixed Indie Fighter") },
            { "hpt_beamlaser_gimbal_empire_fighter", new ShipModule(899990023,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Gimbal Empire Fighter") },
            { "hpt_beamlaser_gimbal_fed_fighter", new ShipModule(899990024,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Gimbal Federation Fighter") },
            { "hpt_beamlaser_gimbal_indie_fighter", new ShipModule(899990025,ShipModule.ModuleTypes.BeamLaser,0,1,"Beam Laser Gimbal Indie Fighter") },
            { "hpt_plasmarepeater_fixed_empire_fighter", new ShipModule(899990026,ShipModule.ModuleTypes.PlasmaAccelerator,0,1,"Plasma Repeater Fixed Empire Fighter") },
            { "hpt_plasmarepeater_fixed_fed_fighter", new ShipModule(899990027,ShipModule.ModuleTypes.PlasmaAccelerator,0,1,"Plasma Repeater Fixed Fed Fighter") },
            { "hpt_plasmarepeater_fixed_indie_fighter", new ShipModule(899990028,ShipModule.ModuleTypes.PlasmaAccelerator,0,1,"Plasma Repeater Fixed Indie Fighter") },
            { "hpt_pulselaser_fixed_empire_fighter", new ShipModule(899990029,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Fixed Empire Fighter") },
            { "hpt_pulselaser_fixed_fed_fighter", new ShipModule(899990030,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Fixed Federation Fighter") },
            { "hpt_pulselaser_fixed_indie_fighter", new ShipModule(899990031,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Fixed Indie Fighter") },
            { "hpt_pulselaser_gimbal_empire_fighter", new ShipModule(899990032,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Gimbal Empire Fighter") },
            { "hpt_pulselaser_gimbal_fed_fighter", new ShipModule(899990033,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Gimbal Federation Fighter") },
            { "hpt_pulselaser_gimbal_indie_fighter", new ShipModule(899990034,ShipModule.ModuleTypes.PulseLaser,0,1,"Pulse Laser Gimbal Indie Fighter") },

            { "int_engine_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.Thrusters,1,1,"Fighter Engine Class 1") },

            { "gdn_hybrid_fighter_v1_cockpit", new ShipModule(899990101,ShipModule.ModuleTypes.CockpitType,0,0,"GDN Hybrid Fighter V 1 Cockpit") },
            { "gdn_hybrid_fighter_v2_cockpit", new ShipModule(899990102,ShipModule.ModuleTypes.CockpitType,0,0,"GDN Hybrid Fighter V 2 Cockpit") },
            { "gdn_hybrid_fighter_v3_cockpit", new ShipModule(899990103,ShipModule.ModuleTypes.CockpitType,0,0,"GDN Hybrid Fighter V 3 Cockpit") },

            { "hpt_atmulticannon_fixed_indie_fighter", new ShipModule(899990040,ShipModule.ModuleTypes.AXMulti_Cannon,0,1,"AX Multicannon Fixed Indie Fighter") },
            { "hpt_multicannon_fixed_empire_fighter", new ShipModule(899990050,ShipModule.ModuleTypes.Multi_Cannon,0,1,"Multicannon Fixed Empire Fighter") },
            { "hpt_multicannon_fixed_fed_fighter", new ShipModule(899990051,ShipModule.ModuleTypes.Multi_Cannon,0,1,"Multicannon Fixed Fed Fighter") },
            { "hpt_multicannon_fixed_indie_fighter", new ShipModule(899990052,ShipModule.ModuleTypes.Multi_Cannon,0,1,"Multicannon Fixed Indie Fighter") },

            { "int_powerdistributor_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,0,0,"Int Powerdistributor Fighter Class 1") },

            { "int_powerplant_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,0,0,"Int Powerplant Fighter Class 1") },

            { "int_sensors_fighter_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,0,0,"Int Sensors Fighter Class 1") },
            { "int_shieldgenerator_fighter_class1", new ShipModule(899990080,ShipModule.ModuleTypes.ShieldGenerator,0,0,"Shield Generator Fighter Class 1") },
            { "ext_emitter_guardian", new ShipModule(899990190,ShipModule.ModuleTypes.Sensors,0,0,"Ext Emitter Guardian") },
            { "ext_emitter_standard", new ShipModule(899990090,ShipModule.ModuleTypes.Sensors,0,0,"Ext Emitter Standard") },

        };

#endregion

#region SRV

        public static Dictionary<string, ShipModule> srvmodules = new Dictionary<string, ShipModule>
        {
            { "buggycargobaydoor", new ShipModule(-1,ShipModule.ModuleTypes.CargoBayDoorType,0,0,"SRV Cargo Bay Door") },
            { "int_fueltank_size0_class3", new ShipModule(-1,ShipModule.ModuleTypes.FuelTank,0,0,"SRV Scarab Fuel Tank") },
            { "vehicle_scorpion_missilerack_lockon", new ShipModule(-1,ShipModule.ModuleTypes.MissileRack,0,0,"SRV Scorpion Missile Rack") },
            { "int_powerdistributor_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,0,0,"SRV Scarab Power Distributor") },
            { "int_powerplant_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,0,0,"SRV Scarab Powerplant") },
            { "vehicle_plasmaminigun_turretgun", new ShipModule(-1,ShipModule.ModuleTypes.PulseLaser,0,0,"SRV Scorpion Plasma Turret Gun") },

            { "testbuggy_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,0,0,"SRV Scarab Cockpit") },
            { "scarab_armour_grade1", new ShipModule(-1,ShipModule.ModuleTypes.LightweightAlloy,0,0,"SRV Scarab Armour") },
            { "int_fueltank_size0_class2", new ShipModule(-1,ShipModule.ModuleTypes.FuelTank,0,0,"SRV Scopion Fuel tank Size 0 Class 2") },
            { "combat_multicrew_srv_01_cockpit", new ShipModule(-1,ShipModule.ModuleTypes.CockpitType,0,0,"SRV Scorpion Cockpit") },
            { "int_powerdistributor_size0_class1_cms", new ShipModule(-1,ShipModule.ModuleTypes.PowerDistributor,0,0,"SRV Scorpion Power Distributor Size 0 Class 1 Cms") },
            { "int_powerplant_size0_class1_cms", new ShipModule(-1,ShipModule.ModuleTypes.PowerPlant,0,0,"SRV Scorpion Powerplant Size 0 Class 1 Cms") },
            { "vehicle_turretgun", new ShipModule(-1,ShipModule.ModuleTypes.PulseLaser,0,0,"SRV Scarab Turret") },

            { "hpt_datalinkscanner", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,0,0,"SRV Data Link Scanner") },
            { "int_sinewavescanner_size1_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,0,0,"SRV Scarab Scanner") },
            { "int_sensors_surface_size1_class1", new ShipModule(-1,ShipModule.ModuleTypes.Sensors,0,0,"SRV Sensors") },

            { "int_lifesupport_size0_class1", new ShipModule(-1,ShipModule.ModuleTypes.LifeSupport,0,0,"SRV Life Support") },
            { "int_shieldgenerator_size0_class3", new ShipModule(-1,ShipModule.ModuleTypes.ShieldGenerator,0,0,"SRV Shields") },
        };

#endregion

#region Vanity Modules

        public static Dictionary<string, ShipModule> vanitymodules = new Dictionary<string, ShipModule>   // DO NOT USE DIRECTLY - public is for checking only
        {
            { "null", new ShipModule(-1,ShipModule.ModuleTypes.UnknownType,0,0,"Error in frontier journal - Null module") },

            { "typex_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Alliance Chieftain Shipkit 1 Bumper 3") },
            { "typex_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Alliance Chieftain Shipkit 1 Spoiler 3") },
            { "typex_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Alliance Chieftain Shipkit 1 Wings 1") },
            { "anaconda_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Bumper 1") },
            { "anaconda_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Bumper 2") },
            { "anaconda_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Bumper 3") },
            { "anaconda_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Bumper 4") },
            { "anaconda_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Spoiler 1") },
            { "anaconda_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Spoiler 2") },
            { "anaconda_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Spoiler 3") },
            { "anaconda_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Spoiler 4") },
            { "anaconda_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Tail 1") },
            { "anaconda_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Tail 2") },
            { "anaconda_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Tail 3") },
            { "anaconda_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Tail 4") },
            { "anaconda_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Wings 1") },
            { "anaconda_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Wings 2") },
            { "anaconda_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Wings 3") },
            { "anaconda_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 1 Wings 4") },
            { "anaconda_shipkit2raider_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Bumper 1") },
            { "anaconda_shipkit2raider_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Bumper 2") },
            { "anaconda_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Bumper 3") },
            { "anaconda_shipkit2raider_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Spoiler 1") },
            { "anaconda_shipkit2raider_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Spoiler 2") },
            { "anaconda_shipkit2raider_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Spoiler 3") },
            { "anaconda_shipkit2raider_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Tail 2") },
            { "anaconda_shipkit2raider_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Tail 3") },
            { "anaconda_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Wings 2") },
            { "anaconda_shipkit2raider_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Anaconda Shipkit 2 Raider Wings 3") },
            { "asp_industrial1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Industrial 1 Bumper 1") },
            { "asp_industrial1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Industrial 1 Spoiler 1") },
            { "asp_industrial1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Industrial 1 Wings 1") },
            { "asp_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Bumper 1") },
            { "asp_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Bumper 2") },
            { "asp_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Bumper 3") },
            { "asp_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Bumper 4") },
            { "asp_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Spoiler 1") },
            { "asp_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Spoiler 2") },
            { "asp_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Spoiler 3") },
            { "asp_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Spoiler 4") },
            { "asp_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Wings 1") },
            { "asp_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Wings 2") },
            { "asp_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Wings 3") },
            { "asp_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 1 Wings 4") },
            { "asp_shipkit2raider_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 2 Raider Bumper 2") },
            { "asp_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 2 Raider Bumper 3") },
            { "asp_shipkit2raider_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 2 Raider Tail 2") },
            { "asp_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Shipkit 2 Raider Wings 2") },
            { "asp_science1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Science 1 Spoiler 1") },
            { "asp_science1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Science 1 Wings 1") },
            { "asp_science1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Asp Science 1 Bumper 1") },
            { "bobble_ap2_textexclam", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text !") },
            { "bobble_ap2_texte", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text e") },
            { "bobble_ap2_textl", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text l") },
            { "bobble_ap2_textn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text n") },
            { "bobble_ap2_texto", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text o") },
            { "bobble_ap2_textr", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text r") },
            { "bobble_ap2_texts", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Text s") },
            { "bobble_ap2_textasterisk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Textasterisk") },
            { "bobble_ap2_textg", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Textg") },
            { "bobble_ap2_textj", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Textj") },
            { "bobble_ap2_textu", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Textu") },
            { "bobble_ap2_texty", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ap 2 Texty") },
            { "bobble_christmastree", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Christmas Tree") },
            { "bobble_davidbraben", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble David Braben") },
            { "bobble_dotd_blueskull", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Dotd Blueskull") },
            { "bobble_nav_beacon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Nav Beacon") },
            { "bobble_oldskool_anaconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Oldskool Anaconda") },
            { "bobble_oldskool_aspmkii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Oldskool Asp Mk II") },
            { "bobble_oldskool_cobramkiii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Oldskool Cobram Mk III") },
            { "bobble_oldskool_python", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Oldskool Python") },
            { "bobble_oldskool_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Oldskool Thargoid") },
            { "bobble_pilot_dave_expo_flight_suit", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Pilot Dave Expo Flight Suit") },
            { "bobble_pilotfemale", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Pilot Female") },
            { "bobble_pilotmale", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Pilot Male") },
            { "bobble_pilotmale_expo_flight_suit", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Pilot Male Expo Flight Suit") },
            { "bobble_planet_earth", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Earth") },
            { "bobble_planet_jupiter", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Jupiter") },
            { "bobble_planet_mars", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Mars") },
            { "bobble_planet_mercury", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Mercury") },
            { "bobble_planet_neptune", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Neptune") },
            { "bobble_planet_saturn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Saturn") },
            { "bobble_planet_uranus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Uranus") },
            { "bobble_planet_venus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Planet Venus") },
            { "bobble_plant_aloe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Plant Aloe") },
            { "bobble_plant_braintree", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Plant Braintree") },
            { "bobble_plant_rosequartz", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Plant Rosequartz") },
            { "bobble_pumpkin", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Pumpkin") },
            { "bobble_santa", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Santa") },
            { "bobble_ship_anaconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ship Anaconda") },
            { "bobble_ship_cobramkiii", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ship Cobra Mk III") },
            { "bobble_ship_cobramkiii_ffe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ship Cobra Mk III FFE") },
            { "bobble_ship_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ship Thargoid") },
            { "bobble_ship_viper", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Ship Viper") },
            { "bobble_snowflake", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Snowflake") },
            { "bobble_snowman", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Snowman") },
            { "bobble_station_coriolis", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Station Coriolis") },
            { "bobble_station_coriolis_wire", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Station Coriolis Wire") },
            { "bobble_textexclam", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text !") },
            { "bobble_textpercent", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text %") },
            { "bobble_textquest", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text ?") },
            { "bobble_text0", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 0") },
            { "bobble_text1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 1") },
            { "bobble_text2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 2") },
            { "bobble_text3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 3") },
            { "bobble_text4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 4") },
            { "bobble_text5", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 5") },
            { "bobble_text6", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 6") },
            { "bobble_text7", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 7") },
            { "bobble_text8", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 8") },
            { "bobble_text9", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text 9") },
            { "bobble_texta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text A") },
            { "bobble_textb", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text B") },
            { "bobble_textbracket01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Bracket 1") },
            { "bobble_textbracket02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Bracket 2") },
            { "bobble_textcaret", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Caret") },
            { "bobble_textd", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text d") },
            { "bobble_textdollar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Dollar") },
            { "bobble_texte", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text e") },
            { "bobble_texte04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text E 4") },
            { "bobble_textexclam01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Exclam 1") },
            { "bobble_textf", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text f") },
            { "bobble_textg", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text G") },
            { "bobble_texth", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text H") },
            { "bobble_texthash", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Hash") },
            { "bobble_texti", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text I") },
            { "bobble_texti02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text I 2") },
            { "bobble_textm", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text m") },
            { "bobble_textn", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text n") },
            { "bobble_texto02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text O 2") },
            { "bobble_texto03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text O 3") },
            { "bobble_textp", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text P") },
            { "bobble_textplus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Plus") },
            { "bobble_textr", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text r") },
            { "bobble_textt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text t") },
            { "bobble_textu", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text U") },
            { "bobble_textu01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text U 1") },
            { "bobble_textv", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text V") },
            { "bobble_textx", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text X") },
            { "bobble_texty", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Y") },
            { "bobble_textz", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Text Z") },
            { "bobble_textasterisk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Textasterisk") },
            { "bobble_texte01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Texte 1") },
            { "bobble_texti01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Texti 1") },
            { "bobble_textk", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Textk") },
            { "bobble_textl", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Textl") },
            { "bobble_textminus", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Textminus") },
            { "bobble_texto", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Texto") },
            { "bobble_texts", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Texts") },
            { "bobble_textunderscore", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Textunderscore") },
            { "bobble_trophy_anti_thargoid_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Anti Thargoid S") },
            { "bobble_trophy_combat", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Combat") },
            { "bobble_trophy_combat_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Combat S") },
            { "bobble_trophy_community", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Community") },
            { "bobble_trophy_exploration", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Exploration") },
            { "bobble_trophy_exploration_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Exploration B") },
            { "bobble_trophy_exploration_s", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Exploration S") },
            { "bobble_trophy_powerplay_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Bobble Trophy Powerplay B") },
            { "cobramkiii_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Co)bra MK III Shipkit 1 Wings 3") },
            { "cobramkiii_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra MK III Shipkit 1 Bumper 1") },
            { "cobramkiii_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra MK III Shipkit 1 Spoiler 2") },
            { "cobramkiii_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit 1 Spoiler 4") },
            { "cobramkiii_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra MK III Shipkit 1 Tail 1") },
            { "cobramkiii_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit 1 Tail 3") },
            { "cobramkiii_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit 1 Wings 1") },
            { "cobramkiii_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra MK III Shipkit 1 Wings 2") },
            { "cobramkiii_shipkitraider1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit Raider 1 Bumper 2") },
            { "cobramkiii_shipkitraider1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit Raider 1 Spoiler 3") },
            { "cobramkiii_shipkitraider1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit Raider 1 Tail 2") },
            { "cobramkiii_shipkitraider1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobra Mk III Shipkit Raider 1 Wings 1") },
            { "cobramkiii_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobramkiii Shipkit 1 Bumper 4") },
            { "cobramkiii_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cobramkiii Shipkit 1 Tail 4") },
            { "cutter_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Bumper 2") },
            { "cutter_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Bumper 3") },
            { "cutter_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Bumper 4") },
            { "cutter_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Spoiler 2") },
            { "cutter_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Spoiler 3") },
            { "cutter_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Spoiler 4") },
            { "cutter_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Wings 2") },
            { "cutter_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Wings 3") },
            { "decal_explorer_elite02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Elite 2") },
            { "decal_explorer_elite03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Elite 3") },
            { "decal_skull9", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Skull 9") },
            { "decal_skull8", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Skull 8") },
            { "decal_alien_hunter2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Alien Hunter 2") },
            { "decal_alien_hunter6", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Alien Hunter 6") },
            { "decal_alien_sympathiser_b", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Alien Sympathiser B") },
            { "decal_anti_thargoid", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Anti Thargoid") },
            { "decal_bat2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Bat 2") },
            { "decal_beta_tester", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Beta Tester") },
            { "decal_bounty_hunter", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Bounty Hunter") },
            { "decal_bridgingthegap", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Bridgingthegap") },
            { "decal_cannon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Cannon") },
            { "decal_combat_competent", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Competent") },
            { "decal_combat_dangerous", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Dangerous") },
            { "decal_combat_deadly", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Deadly") },
            { "decal_combat_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Elite") },
            { "decal_combat_expert", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Expert") },
            { "decal_combat_master", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Master") },
            { "decal_combat_mostly_harmless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Mostly Harmless") },
            { "decal_combat_novice", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Combat Novice") },
            { "decal_community", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Community") },
            { "decal_distantworlds", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Distant Worlds") },
            { "decal_distantworlds2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Distantworlds 2") },
            { "decal_egx", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Egx") },
            { "decal_espionage", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Espionage") },
            { "decal_exploration_emisswhite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Exploration Emisswhite") },
            { "decal_explorer_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Elite") },
            { "decal_explorer_elite05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Elite 5") },
            { "decal_explorer_mostly_aimless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Mostly Aimless") },
            { "decal_explorer_pathfinder", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Pathfinder") },
            { "decal_explorer_ranger", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Ranger") },
            { "decal_explorer_scout", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Scout") },
            { "decal_explorer_starblazer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Starblazer") },
            { "decal_explorer_surveyor", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Surveyor") },
            { "decal_explorer_trailblazer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Explorer Trailblazer") },
            { "decal_expo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Expo") },
            { "decal_founders_reversed", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Founders Reversed") },
            { "decal_fuelrats", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Fuel Rats") },
            { "decal_galnet", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Galnet") },
            { "decal_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Lave Con") },
            { "decal_met_constructshipemp_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Constructshipemp Gold") },
            { "decal_met_espionage_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Espionage Gold") },
            { "decal_met_espionage_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Espionage Silver") },
            { "decal_met_exploration_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Exploration Gold") },
            { "decal_met_mining_bronze", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Mining Bronze") },
            { "decal_met_mining_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Mining Gold") },
            { "decal_met_mining_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Mining Silver") },
            { "decal_met_salvage_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Met Salvage Gold") },
            { "decal_mining", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Mining") },
            { "decal_networktesters", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Network Testers") },
            { "decal_onionhead1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Onionhead 1") },
            { "decal_onionhead2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Onionhead 2") },
            { "decal_onionhead3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Onionhead 3") },
            { "decal_passenger_e", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Passenger E") },
            { "decal_passenger_g", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Passenger G") },
            { "decal_passenger_l", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Passenger L") },
            { "decal_paxprime", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Pax Prime") },
            { "decal_pilot_fed1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Pilot Fed 1") },
            { "decal_planet2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Planet 2") },
            { "decal_playergroup_wolves_of_jonai", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Player Group Wolves Of Jonai") },
            { "decal_playergroup_ugc", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Playergroup Ugc") },
            { "decal_powerplay_hudson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Power Play Hudson") },
            { "decal_powerplay_mahon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Power Play Mahon") },
            { "decal_powerplay_utopia", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Power Play Utopia") },
            { "decal_powerplay_aislingduval", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Powerplay Aislingduval") },
            { "decal_powerplay_halsey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Powerplay Halsey") },
            { "decal_powerplay_kumocrew", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Powerplay Kumocrew") },
            { "decal_powerplay_sirius", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Powerplay Sirius") },
            { "decal_pumpkin", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Pumpkin") },
            { "decal_shark1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Shark 1") },
            { "decal_skull3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Skull 3") },
            { "decal_skull5", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Skull 5") },
            { "decal_specialeffect", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Special Effect") },
            { "decal_spider", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Spider") },
            { "decal_thegolconda", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Thegolconda") },
            { "decal_trade_broker", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Broker") },
            { "decal_trade_dealer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Dealer") },
            { "decal_trade_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Elite") },
            { "decal_trade_elite05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Elite 5") },
            { "decal_trade_entrepeneur", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Entrepeneur") },
            { "decal_trade_merchant", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Merchant") },
            { "decal_trade_mostly_penniless", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Mostly Penniless") },
            { "decal_trade_peddler", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Peddler") },
            { "decal_trade_tycoon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Trade Tycoon") },
            { "decal_triple_elite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Decal Triple Elite") },
            { "diamondbackxl_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Diamond Back XL Shipkit 1 Bumper 1") },
            { "diamondbackxl_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Diamond Back XL Shipkit 1 Spoiler 2") },
            { "diamondbackxl_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Diamond Back XL Shipkit 1 Wings 2") },
            { "dolphin_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Bumper 2") },
            { "dolphin_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Bumper 3") },
            { "dolphin_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Spoiler 2") },
            { "dolphin_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Tail 4") },
            { "dolphin_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Wings 2") },
            { "dolphin_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Dolphin Shipkit 1 Wings 3") },
            { "eagle_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Eagle Shipkit 1 Bumper 2") },
            { "eagle_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Eagle Shipkit 1 Spoiler 1") },
            { "eagle_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Eagle Shipkit 1 Wings 1") },
            { "empire_courier_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Bumper 2") },
            { "empire_courier_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Bumper 3") },
            { "empire_courier_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Spoiler 2") },
            { "empire_courier_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Spoiler 3") },
            { "empire_courier_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Wings 1") },
            { "empire_courier_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Wings 2") },
            { "empire_courier_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Wings 3") },
            { "empire_trader_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Bumper 3") },
            { "empire_trader_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Spoiler 1") },
            { "empire_trader_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Spoiler 3") },
            { "empire_trader_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Spoiler 4") },
            { "empire_trader_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Tail 1") },
            { "empire_trader_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Tail 2") },
            { "empire_trader_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Tail 3") },
            { "empire_trader_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Tail 4") },
            { "empire_trader_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Trader Shipkit 1 Wings 1") },
            { "enginecustomisation_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Blue") },
            { "enginecustomisation_cyan", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Cyan") },
            { "enginecustomisation_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Green") },
            { "enginecustomisation_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Orange") },
            { "enginecustomisation_pink", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Pink") },
            { "enginecustomisation_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Purple") },
            { "enginecustomisation_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Red") },
            { "enginecustomisation_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation White") },
            { "enginecustomisation_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Engine Customisation Yellow") },
            { "federation_corvette_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Bumper 2") },
            { "federation_corvette_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Bumper 3") },
            { "federation_corvette_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Bumper 4") },
            { "federation_corvette_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Spoiler 1") },
            { "federation_corvette_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Spoiler 2") },
            { "federation_corvette_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Spoiler 3") },
            { "federation_corvette_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Spoiler 4") },
            { "federation_corvette_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Tail 1") },
            { "federation_corvette_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Tail 2") },
            { "federation_corvette_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Tail 3") },
            { "federation_corvette_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Tail 4") },
            { "federation_corvette_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Wings 3") },
            { "federation_corvette_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Wings 4") },
            { "federation_gunship_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Gunship Shipkit 1 Bumper 1") },
            { "ferdelance_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Fer De Lance Shipkit 1 Bumper 4") },
            { "ferdelance_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Fer De Lance Shipkit 1 Tail 1") },
            { "ferdelance_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Fer De Lance Shipkit 1 Wings 2") },
            { "ferdelance_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Ferdelance Shipkit 1 Bumper 1") },
            { "ferdelance_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Ferdelance Shipkit 1 Bumper 3") },
            { "ferdelance_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Ferdelance Shipkit 1 Spoiler 3") },
            { "ferdelance_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Ferdelance Shipkit 1 Tail 3") },
            { "ferdelance_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Ferdelance Shipkit 1 Wings 1") },
            { "krait_light_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Bumper 1") },
            { "krait_light_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Bumper 2") },
            { "krait_light_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Bumper 4") },
            { "krait_light_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Spoiler 1") },
            { "krait_light_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Spoiler 2") },
            { "krait_light_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Spoiler 3") },
            { "krait_light_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Spoiler 4") },
            { "krait_light_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Tail 3") },
            { "krait_light_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Tail 4") },
            { "krait_light_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Wings 1") },
            { "krait_light_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Wings 2") },
            { "krait_light_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Wings 3") },
            { "krait_light_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Wings 4") },
            { "krait_mkii_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Bumper 1") },
            { "krait_mkii_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Bumper 2") },
            { "krait_mkii_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Bumper 3") },
            { "krait_mkii_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Spoiler 1") },
            { "krait_mkii_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Spoiler 2") },
            { "krait_mkii_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Spoiler 4") },
            { "krait_mkii_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Tail 1") },
            { "krait_mkii_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Tail 2") },
            { "krait_mkii_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Tail 3") },
            { "krait_mkii_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Wings 2") },
            { "krait_mkii_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Wings 3") },
            { "krait_mkii_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Wings 4") },
            { "krait_mkii_shipkitraider1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit raider 1 Spoiler 3") },
            { "krait_mkii_shipkitraider1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit raider 1 Wings 2") },
            { "nameplate_combat01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Combat 1 White") },
            { "nameplate_combat02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Combat 2 White") },
            { "nameplate_combat03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Combat 3 Black") },
            { "nameplate_combat03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Combat 3 White") },
            { "nameplate_empire01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Empire 1 White") },
            { "nameplate_empire02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Empire 2 Black") },
            { "nameplate_empire03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Empire 3 Black") },
            { "nameplate_empire03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Empire 3 White") },
            { "nameplate_expedition01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 1 Black") },
            { "nameplate_expedition01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 1 White") },
            { "nameplate_expedition02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 2 Black") },
            { "nameplate_expedition02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 2 White") },
            { "nameplate_expedition03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 3 Black") },
            { "nameplate_expedition03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 3 White") },
            { "nameplate_explorer01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 1 Black") },
            { "nameplate_explorer01_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 1 Grey") },
            { "nameplate_explorer01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 1 White") },
            { "nameplate_explorer02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 2 Black") },
            { "nameplate_explorer02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 2 Grey") },
            { "nameplate_explorer02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 2 White") },
            { "nameplate_explorer03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 3 Black") },
            { "nameplate_explorer03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Explorer 3 White") },
            { "nameplate_hunter01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Hunter 1 White") },
            { "nameplate_passenger01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Passenger 1 Black") },
            { "nameplate_passenger01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Passenger 1 White") },
            { "nameplate_passenger02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Passenger 2 Black") },
            { "nameplate_passenger03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Passenger 3 White") },
            { "nameplate_pirate03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Pirate 3 White") },
            { "nameplate_practical01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 1 Black") },
            { "nameplate_practical01_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 1 Grey") },
            { "nameplate_practical01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 1 White") },
            { "nameplate_practical02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 2 Black") },
            { "nameplate_practical02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 2 Grey") },
            { "nameplate_practical02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 2 White") },
            { "nameplate_practical03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 3 Black") },
            { "nameplate_practical03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Practical 3 White") },
            { "nameplate_raider03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Raider 3 Black") },
            { "nameplate_shipid_doubleline_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID Double Line Black") },
            { "nameplate_shipid_doubleline_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID Double Line Grey") },
            { "nameplate_shipid_doubleline_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID Double Line White") },
            { "nameplate_shipid_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID Grey") },
            { "nameplate_shipid_singleline_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID Single Line Black") },
            { "nameplate_shipid_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship ID White") },
            { "nameplate_shipname_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Ship Name White") },
            { "nameplate_shipid_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipid Black") },
            { "nameplate_shipid_singleline_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipid Singleline Grey") },
            { "nameplate_shipid_singleline_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipid Singleline White") },
            { "nameplate_shipname_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Black") },
            { "nameplate_shipname_distressed_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Distressed Black") },
            { "nameplate_shipname_distressed_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Distressed Grey") },
            { "nameplate_shipname_distressed_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Distressed White") },
            { "nameplate_shipname_worn_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Worn Black") },
            { "nameplate_shipname_worn_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Shipname Worn White") },
            { "nameplate_skulls01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Skulls 1 White") },
            { "nameplate_skulls03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Skulls 3 Black") },
            { "nameplate_skulls03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Skulls 3 White") },
            { "nameplate_sympathiser03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Sympathiser 3 White") },
            { "nameplate_trader01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 1 Black") },
            { "nameplate_trader01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 1 White") },
            { "nameplate_trader02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 2 Black") },
            { "nameplate_trader02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 2 Grey") },
            { "nameplate_trader02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 2 White") },
            { "nameplate_trader03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 3 Black") },
            { "nameplate_trader03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Trader 3 White") },
            { "nameplate_victory02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Victory 2 White") },
            { "nameplate_victory03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Victory 3 White") },
            { "nameplate_wings01_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 1 Black") },
            { "nameplate_wings01_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 1 White") },
            { "nameplate_wings02_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 2 Black") },
            { "nameplate_wings02_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 2 White") },
            { "nameplate_wings03_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 3 Black") },
            { "nameplate_wings03_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 3 Grey") },
            { "nameplate_wings03_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Wings 3 White") },
            { "paintjob_adder_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Adder Black Friday 1") },
            { "paintjob_anaconda_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Blackfriday 1") },
            { "paintjob_anaconda_corrosive_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Corrosive 4") },
            { "paintjob_anaconda_eliteexpo_eliteexpo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Elite Expo Elite Expo") },
            { "paintjob_anaconda_faction1_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Faction 1 4") },
            { "paintjob_anaconda_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Gold Wireframe 1") },
            { "paintjob_anaconda_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Horus 2 3") },
            { "paintjob_anaconda_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Iridescent High Colour 2") },
            { "paintjob_anaconda_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Lrpo Azure") },
            { "paintjob_anaconda_luminous_stripe_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Luminous Stripe 3") },
            { "paintjob_anaconda_luminous_stripe_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Luminous Stripe 4") },
            { "paintjob_anaconda_luminous_stripe_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Luminous Stripe 6") },
            { "paintjob_anaconda_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Metallic 2 Chrome") },
            { "paintjob_anaconda_metallic_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Metallic Gold") },
            { "paintjob_anaconda_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Militaire Earth Red") },
            { "paintjob_anaconda_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Militaire Earth Yellow") },
            { "paintjob_anaconda_pulse2_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Pulse 2 Purple") },
            { "paintjob_anaconda_strife_strife", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Strife Strife") },
            { "paintjob_anaconda_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Tactical Blue") },
            { "paintjob_anaconda_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Blue") },
            { "paintjob_anaconda_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Green") },
            { "paintjob_anaconda_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Orange") },
            { "paintjob_anaconda_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Purple") },
            { "paintjob_anaconda_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Red") },
            { "paintjob_anaconda_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Vibrant Yellow") },
            { "paintjob_anaconda_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Wireframe 1") },
            { "paintjob_asp_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Blackfriday 1") },
            { "paintjob_asp_gamescom_gamescom", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Games Com GamesCom") },
            { "paintjob_asp_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Gold Wireframe 1") },
            { "paintjob_asp_iridescenthighcolour_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Iridescent High Colour 1") },
            { "paintjob_asp_largelogometallic_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Largelogometallic 5") },
            { "paintjob_asp_metallic_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Metallic Gold") },
            { "paintjob_asp_blackfriday2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Blackfriday 2 1") },
            { "paintjob_asp_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Salvage 3") },
            { "paintjob_asp_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Salvage 6") },
            { "paintjob_asp_scout_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Scout Black Friday 1") },
            { "paintjob_asp_squadron_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Squadron Green") },
            { "paintjob_asp_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Squadron Red") },
            { "paintjob_asp_stripe1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Stripe 1 3") },
            { "paintjob_asp_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Tactical Grey") },
            { "paintjob_asp_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Tactical White") },
            { "paintjob_asp_trespasser_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Trespasser 1") },
            { "paintjob_asp_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Vibrant Purple") },
            { "paintjob_asp_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Vibrant Red") },
            { "paintjob_asp_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Asp Wireframe 1") },
            { "paintjob_belugaliner_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Beluga Liner Metallic 2 Gold") },
            { "paintjob_cobramkiii_25thanniversary_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III 25 Thanniversary 1") },
            { "paintjob_cobramkiii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Black Friday 1") },
            { "paintjob_cobramkiii_flag_canada_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Flag Canada 1") },
            { "paintjob_cobramkiii_flag_uk_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Flag UK 1") },
            { "paintjob_cobramkiii_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Militaire Earth Red") },
            { "paintjob_cobramkiii_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Militaire Forest Green") },
            { "paintjob_cobramkiii_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Militaire Sand") },
            { "paintjob_cobramkiii_onionhead1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Onionhead 1 1") },
            { "paintjob_cobramkiii_stripe2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Stripe 2 2") },
            { "paintjob_cobramkiii_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Vibrant Yellow") },
            { "paintjob_cobramkiii_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk III Wireframe 1") },
            { "paintjob_cobramkiv_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk IV Black Friday 1") },
            { "paintjob_cobramkiv_gradient2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mk IV Gradient 2 6") },
            { "paintjob_cobramkiii_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra MKIII Corrosive 5") },
            { "paintjob_cobramkiii_default_52", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cobra Mkiii Default 52") },
            { "paintjob_cutter_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Black Friday 1") },
            { "paintjob_cutter_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Full Metal Cobalt") },
            { "paintjob_cutter_fullmetal_paladium", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Fullmetal Paladium") },
            { "paintjob_cutter_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Iridescent High Colour 2") },
            { "paintjob_cutter_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Lrpo Azure") },
            { "paintjob_cutter_luminous_stripe_ver2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Luminous Stripe Ver 2 2") },
            { "paintjob_cutter_luminous_stripe_ver2_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Luminous Stripe Ver 2 4") },
            { "paintjob_cutter_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Metallic 2 Chrome") },
            { "paintjob_cutter_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Metallic 2 Gold") },
            { "paintjob_cutter_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Metallic Chrome") },
            { "paintjob_cutter_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Militaire Forest Green") },
            { "paintjob_cutter_smartfancy_2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Smartfancy 2 6") },
            { "paintjob_cutter_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Smartfancy 4") },
            { "paintjob_cutter_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Tactical Grey") },
            { "paintjob_cutter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Vibrant Blue") },
            { "paintjob_cutter_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Vibrant Purple") },
            { "paintjob_cutter_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Cutter Vibrant Yellow") },
            { "paintjob_diamondbackxl_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Diamond Back XL Metallic 2 Chrome") },
            { "paintjob_diamondbackxl_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Diamondbackxl Lrpo Azure") },
            { "paintjob_dolphin_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Dolphin Blackfriday 1") },
            { "paintjob_dolphin_iridescentblack_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Dolphin Iridescentblack 1") },
            { "paintjob_dolphin_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Dolphin Lrpo Azure") },
            { "paintjob_dolphin_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Dolphin Metallic 2 Gold") },
            { "paintjob_eagle_crimson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Eagle Crimson") },
            { "paintjob_eagle_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Eagle Tactical Grey") },
            { "paintjob_empire_courier_aerial_display_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Aerial Display Blue") },
            { "paintjob_empire_courier_aerial_display_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Aerial Display Red") },
            { "paintjob_empire_courier_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Lrpo Azure") },
            { "paintjob_empire_courier_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Smartfancy 4") },
            { "paintjob_empire_courier_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Tactical Grey") },
            { "paintjob_empire_courier_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Courier Vibrant Yellow") },
            { "paintjob_empire_eagle_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Eagle Black Friday 1") },
            { "paintjob_empire_eagle_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Eagle Lrpo Azure") },
            { "paintjob_empiretrader_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Trader Black Friday 1") },
            { "paintjob_empire_trader_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empire Trader Lrpo Azure") },
            { "paintjob_empiretrader_smartfancy_2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Smartfancy 2 6") },
            { "paintjob_empiretrader_smartfancy_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Smartfancy 4") },
            { "paintjob_empiretrader_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Tactical Blue") },
            { "paintjob_empiretrader_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Tactical Grey") },
            { "paintjob_empiretrader_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Vibrant Blue") },
            { "paintjob_empiretrader_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Empiretrader Vibrant Purple") },
            { "paintjob_feddropship_mkii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fed Dropship Mk II Black Friday 1") },
            { "paintjob_feddropship_mkii_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fed Dropship Mk II Tactical Blue") },
            { "paintjob_feddropship_mkii_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fed Dropship Mk II Vibrant Purple") },
            { "paintjob_feddropship_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fed Dropship Tactical Blue") },
            { "paintjob_feddropship_mkii_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Feddropship Mkii Vibrant Yellow") },
            { "paintjob_feddropship_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Feddropship Vibrant Orange") },
            { "paintjob_federation_corvette_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Blackfriday 1") },
            { "paintjob_federation_corvette_colourgeo2_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Colour Geo 2 Blue") },
            { "paintjob_federation_corvette_colourgeo_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Colour Geo Blue") },
            { "paintjob_federation_corvette_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Iridescent High Colour 2") },
            { "paintjob_federation_corvette_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Iridescentblack 2") },
            { "paintjob_federation_corvette_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Lrpo Azure") },
            { "paintjob_federation_corvette_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Metallic 2 Chrome") },
            { "paintjob_federation_corvette_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Metallic 2 Gold") },
            { "paintjob_federation_corvette_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Metallic Chrome") },
            { "paintjob_federation_corvette_predator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Predator Red") },
            { "paintjob_federation_corvette_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Corvette Vibrant Purple") },
            { "paintjob_federation_gunship_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Gunship Metallic Chrome") },
            { "paintjob_federation_gunship_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Gunship Tactical Blue") },
            { "paintjob_federation_gunship_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Federation Gunship Tactical Grey") },
            { "paintjob_ferdelance_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fer De Lance Black Friday 1") },
            { "paintjob_ferdelance_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fer De Lance Metallic 2 Chrome") },
            { "paintjob_ferdelance_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fer De Lance Metallic 2 Gold") },
            { "paintjob_ferdelance_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Fer De Lance Wireframe 1") },
            { "paintjob_ferdelance_gradient2_crimson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ferdelance Gradient 2 Crimson") },
            { "paintjob_ferdelance_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ferdelance Vibrant Red") },
            { "paintjob_hauler_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Hauler Blackfriday 1") },
            { "paintjob_hauler_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Hauler Lrpo Azure") },
            { "paintjob_indfighter_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ind Fighter Black Friday 1") },
            { "paintjob_indfighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ind Fighter Vibrant Blue") },
            { "paintjob_indfighter_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ind Fighter Vibrant Green") },
            { "paintjob_indfighter_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Ind Fighter Vibrant Yellow") },
            { "paintjob_independant_trader_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Independant Trader Tactical White") },
            { "paintjob_independant_trader_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Independant Trader Vibrant Purple") },
            { "paintjob_indfighter_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Indfighter Vibrant Purple") },
            { "paintjob_krait_light_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Blackfriday 1") },
            { "paintjob_krait_light_gradient2_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Gradient 2 Blue") },
            { "paintjob_krait_light_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Horus 1 3") },
            { "paintjob_krait_light_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Horus 2 3") },
            { "paintjob_krait_light_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Iridescentblack 2") },
            { "paintjob_krait_light_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Lrpo Azure") },
            { "paintjob_krait_light_salvage_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Salvage 1") },
            { "paintjob_krait_light_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Salvage 3") },
            { "paintjob_krait_light_salvage_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Salvage 4") },
            { "paintjob_krait_light_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Salvage 6") },
            { "paintjob_krait_light_spring_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Spring 5") },
            { "paintjob_krait_light_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Light Tactical White") },
            { "paintjob_krait_mkii_iridescenthighcolour_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mk II Iridescent High Colour 5") },
            { "paintjob_krait_mkii_specialeffectchristmas_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mk II Special Effect Christmas 1") },
            { "paintjob_krait_mkii_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Festive Silver") },
            { "paintjob_krait_mkii_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Horus 2 1") },
            { "paintjob_krait_mkii_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Lrpo Azure") },
            { "paintjob_krait_mkii_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Militaire Forest Green") },
            { "paintjob_krait_mkii_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Salvage 3") },
            { "paintjob_krait_mkii_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Tactical Red") },
            { "paintjob_krait_mkii_trims_blackmagenta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Trims Blackmagenta") },
            { "paintjob_krait_mkii_turbulence_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mkii Turbulence 2") },
            { "paintjob_mamba_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Mamba Black Friday 1") },
            { "paintjob_orca_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Orca Black Friday 1") },
            { "paintjob_orca_corporate2_corporate2e", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Orca Corporate 2 Corporate 2 E") },
            { "paintjob_orca_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Orca Lrpo Azure") },
            { "paintjob_python_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Black Friday 1") },
            { "paintjob_python_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Corrosive 5") },
            { "paintjob_python_eliteexpo_eliteexpo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Elite Expo Elite Expo") },
            { "paintjob_python_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Gold Wireframe 1") },
            { "paintjob_python_gradient2_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Gradient 2 2") },
            { "paintjob_python_gradient2_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Gradient 2 6") },
            { "paintjob_python_horus1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Horus 1 1") },
            { "paintjob_python_iridescentblack_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Iridescentblack 6") },
            { "paintjob_python_luminous_stripe_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Luminous Stripe 3") },
            { "paintjob_python_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Metallic 2 Chrome") },
            { "paintjob_python_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Metallic 2 Gold") },
            { "paintjob_python_militaire_dark_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Dark Green") },
            { "paintjob_python_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Desert Sand") },
            { "paintjob_python_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Earth Red") },
            { "paintjob_python_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Earth Yellow") },
            { "paintjob_python_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Forest Green") },
            { "paintjob_python_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Militaire Sand") },
            { "paintjob_python_militarystripe_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Military Stripe Blue") },
            { "paintjob_python_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Salvage 3") },
            { "paintjob_python_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Squadron Black") },
            { "paintjob_python_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Blue") },
            { "paintjob_python_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Green") },
            { "paintjob_python_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Orange") },
            { "paintjob_python_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Purple") },
            { "paintjob_python_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Red") },
            { "paintjob_python_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Vibrant Yellow") },
            { "paintjob_python_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Python Wireframe 1") },
            { "paintjob_python_nx_venom_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Nx Venom 1") },
            { "paintjob_sidewinder_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Blackfriday 1") },
            { "paintjob_sidewinder_doublestripe_08", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Doublestripe 8") },
            { "paintjob_sidewinder_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Festive Silver") },
            { "paintjob_sidewinder_hotrod_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Hotrod 1") },
            { "paintjob_sidewinder_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Metallic Chrome") },
            { "paintjob_sidewinder_pax_east_pax_east", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Pax East") },
            { "paintjob_sidewinder_pilotreward_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Pilotreward 1") },
            { "paintjob_sidewinder_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Vibrant Blue") },
            { "paintjob_sidewinder_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Sidewinder Vibrant Orange") },
            { "paintjob_testbuggy_chase_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Chase 4") },
            { "paintjob_testbuggy_chase_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Chase 5") },
            { "paintjob_testbuggy_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Militaire Desert Sand") },
            { "paintjob_testbuggy_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Tactical Grey") },
            { "paintjob_testbuggy_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Tactical Red") },
            { "paintjob_testbuggy_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Tactical White") },
            { "paintjob_testbuggy_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Testbuggy Vibrant Purple") },
            { "paintjob_type6_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Blackfriday 1") },
            { "paintjob_type6_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Lrpo Azure") },
            { "paintjob_type6_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Militaire Sand") },
            { "paintjob_type6_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Tactical Blue") },
            { "paintjob_type6_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Vibrant Blue") },
            { "paintjob_type6_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 6 Vibrant Yellow") },
            { "paintjob_type7_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 7 Black Friday 1") },
            { "paintjob_type7_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 7 Salvage 3") },
            { "paintjob_type7_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 7 Tactical White") },
            { "paintjob_type9_mechanist_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Mechanist 4") },
            { "paintjob_type9_military_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Full Metal Cobalt") },
            { "paintjob_type9_military_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Lrpo Azure") },
            { "paintjob_type9_military_metallic2_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Metallic 2 Chrome") },
            { "paintjob_type9_military_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Militaire Forest Green") },
            { "paintjob_type9_military_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Tactical Red") },
            { "paintjob_type9_military_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Military Vibrant Blue") },
            { "paintjob_type9_salvage_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Salvage 3") },
            { "paintjob_type9_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Salvage 6") },
            { "paintjob_type9_spring_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Spring 4") },
            { "paintjob_type9_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Type 9 Vibrant Orange") },
            { "paintjob_typex_2_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Typex 2 Lrpo Azure") },
            { "paintjob_typex_3_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Typex 3 Lrpo Azure") },
            { "paintjob_typex_festive_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Typex Festive Silver") },
            { "paintjob_typex_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Typex Lrpo Azure") },
            { "paintjob_viper_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Blackfriday 1") },
            { "paintjob_viper_default_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Default 3") },
            { "paintjob_viper_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Lrpo Azure") },
            { "paintjob_viper_merc", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Merc") },
            { "paintjob_viper_mkiv_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Mk IV Black Friday 1") },
            { "paintjob_viper_mkiv_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Mkiv Lrpo Azure") },
            { "paintjob_viper_stripe1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Stripe 1 2") },
            { "paintjob_viper_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Blue") },
            { "paintjob_viper_vibrant_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Green") },
            { "paintjob_viper_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Orange") },
            { "paintjob_viper_vibrant_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Purple") },
            { "paintjob_viper_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Red") },
            { "paintjob_viper_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Viper Vibrant Yellow") },
            { "paintjob_vulture_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Vulture Black Friday 1") },
            { "paintjob_vulture_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Vulture Lrpo Azure") },
            { "paintjob_vulture_metallic_chrome", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Vulture Metallic Chrome") },
            { "paintjob_vulture_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Vulture Militaire Desert Sand") },
            { "paintjob_vulture_synth_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Vulture Synth Orange") },
            { "paintjob_anaconda_corrosive_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Corrosive 5") },
            { "paintjob_anaconda_lavecon_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Lavecon Lavecon") },
            { "paintjob_anaconda_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Metallic 2 Gold") },
            { "paintjob_anaconda_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Squadron Black") },
            { "paintjob_anaconda_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Squadron Blue") },
            { "paintjob_anaconda_squadron_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Squadron Green") },
            { "paintjob_anaconda_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Squadron Red") },
            { "paintjob_anaconda_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Tactical Grey") },
            { "paintjob_anaconda_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Tactical Red") },
            { "paintjob_anaconda_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Tactical White") },
            { "paintjob_asp_halloween01_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Halloween 1 5") },
            { "paintjob_asp_lavecon_lavecon", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Lavecon Lavecon") },
            { "paintjob_asp_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Lrpo Azure") },
            { "paintjob_asp_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Metallic 2 Gold") },
            { "paintjob_asp_operator_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Operator Green") },
            { "paintjob_asp_operator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Operator Red") },
            { "paintjob_asp_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Squadron Black") },
            { "paintjob_asp_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Squadron Blue") },
            { "paintjob_asp_stripe1_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Stripe 1 4") },
            { "paintjob_asp_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Vibrant Blue") },
            { "paintjob_asp_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Vibrant Orange") },
            { "paintjob_belugaliner_corporatefleet_fleeta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Belugaliner Corporatefleet Fleeta") },
            { "paintjob_cobramkiii_horizons_desert", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Horizons Desert") },
            { "paintjob_cobramkiii_horizons_lunar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Horizons Lunar") },
            { "paintjob_cobramkiii_horizons_polar", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Horizons Polar") },
            { "paintjob_cobramkiii_stripe1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Stripe 1 3") },
            { "paintjob_cobramkiii_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra Mk III Tactical Grey") },
            { "paintjob_cobramkiii_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Tactical White") },
            { "paintjob_cobramkiii_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra Mk III Vibrant Orange") },
            { "paintjob_cobramkiii_yogscast_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobra MK III Yogscast 1") },
            { "paintjob_cobramkiii_stripe2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobramkiii Stripe 2 3") },
            { "paintjob_cutter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Tactical White") },
            { "paintjob_diamondback_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondback Tactical Blue") },
            { "paintjob_diamondback_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondback Tactical Brown") },
            { "paintjob_diamondback_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondback Tactical White") },
            { "paintjob_diamondbackxl_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Blackfriday 1") },
            { "paintjob_diamondbackxl_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Tactical Blue") },
            { "paintjob_diamondbackxl_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Tactical White") },
            { "paintjob_diamondbackxl_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Vibrant Blue") },
            { "paintjob_dolphin_corporatefleet_fleeta", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Dolphin Corporatefleet Fleeta") },
            { "paintjob_dolphin_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Dolphin Vibrant Yellow") },
            { "paintjob_eagle_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Eagle Tactical Blue") },
            { "paintjob_eagle_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Eagle Tactical White") },
            { "paintjob_empire_courier_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Courier Blackfriday 1") },
            { "paintjob_empire_courier_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Courier Metallic 2 Gold") },
            { "paintjob_empire_fighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Fighter Tactical White") },
            { "paintjob_empire_fighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Fighter Vibrant Blue") },
            { "paintjob_empiretrader_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empiretrader Tactical White") },
            { "paintjob_feddropship_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Feddropship Tactical Grey") },
            { "paintjob_federation_corvette_colourgeo_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Colourgeo Red") },
            { "paintjob_federation_corvette_predator_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Predator Blue") },
            { "paintjob_federation_corvette_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Tactical White") },
            { "paintjob_federation_corvette_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Vibrant Blue") },
            { "paintjob_federation_fighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Fighter Tactical White") },
            { "paintjob_federation_fighter_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Fighter Vibrant Blue") },
            { "paintjob_federation_gunship_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Gunship Tactical Brown") },
            { "paintjob_federation_gunship_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Gunship Tactical White") },
            { "paintjob_ferdelance_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Ferdelance Tactical White") },
            { "paintjob_ferdelance_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Ferdelance Vibrant Blue") },
            { "paintjob_ferdelance_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Ferdelance Vibrant Yellow") },
            { "paintjob_hauler_doublestripe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Hauler Doublestripe 1") },
            { "paintjob_hauler_doublestripe_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Hauler Doublestripe 2") },
            { "paintjob_independant_trader_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Independant Trader Blackfriday 1") },
            { "paintjob_indfighter_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Indfighter Tactical White") },
            { "paintjob_krait_mkii_egypt_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Egypt 2") },
            { "paintjob_krait_mkii_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Vibrant Red") },
            { "paintjob_orca_militaire_desert_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Orca Militaire Desert Sand") },
            { "paintjob_orca_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Orca Vibrant Yellow") },
            { "paintjob_python_corrosive_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Corrosive 1") },
            { "paintjob_python_corrosive_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Corrosive 6") },
            { "paintjob_python_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Horus 1 2") },
            { "paintjob_python_horus2_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Horus 2 3") },
            { "paintjob_python_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Lrpo Azure") },
            { "paintjob_python_luminous_stripe_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Luminous Stripe 2") },
            { "paintjob_python_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Tactical White") },
            { "paintjob_sidewinder_doublestripe_07", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Doublestripe 7") },
            { "paintjob_sidewinder_gold_wireframe_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Gold Wireframe 1") },
            { "paintjob_sidewinder_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Militaire Forest Green") },
            { "paintjob_sidewinder_specialeffect_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Specialeffect 1") },
            { "paintjob_sidewinder_thirds_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Thirds 6") },
            { "paintjob_sidewinder_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Sidewinder Vibrant Red") },
            { "paintjob_testbuggy_chase_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Chase 6") },
            { "paintjob_testbuggy_destination_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Destination Blue") },
            { "paintjob_testbuggy_luminous_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Luminous Blue") },
            { "paintjob_testbuggy_luminous_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Luminous Red") },
            { "paintjob_testbuggy_metallic2_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Metallic 2 Gold") },
            { "paintjob_testbuggy_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Militaire Earth Red") },
            { "paintjob_testbuggy_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Militaire Earth Yellow") },
            { "paintjob_testbuggy_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Tactical Blue") },
            { "paintjob_testbuggy_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Vibrant Blue") },
            { "paintjob_testbuggy_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Vibrant Orange") },
            { "paintjob_testbuggy_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob SRV Vibrant Yellow") },
            { "paintjob_type6_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 6 Tactical White") },
            { "paintjob_type7_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 7 Vibrant Blue") },
            { "paintjob_type9_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Blackfriday 1") },
            { "paintjob_type9_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Lrpo Azure") },
            { "paintjob_type9_military_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Military Iridescent black 2") },
            { "paintjob_type9_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Vibrant Blue") },
            { "paintjob_typex_military_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex Military Tactical Grey") },
            { "paintjob_typex_military_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex Military Tactical White") },
            { "paintjob_typex_operator_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex Operator Red") },
            { "paintjob_viper_flag_norway_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper Flag Norway 1") },
            { "paintjob_viper_mkiv_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Militaire Sand") },
            { "paintjob_viper_mkiv_squadron_black", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Squadron Black") },
            { "paintjob_viper_mkiv_squadron_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Squadron Orange") },
            { "paintjob_viper_mkiv_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Tactical Blue") },
            { "paintjob_viper_mkiv_tactical_brown", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Tactical Brown") },
            { "paintjob_viper_mkiv_tactical_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Tactical Green") },
            { "paintjob_viper_mkiv_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Tactical Grey") },
            { "paintjob_viper_mkiv_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper MK IV Tactical White") },
            { "paintjob_vulture_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Vulture Tactical Blue") },
            { "paintjob_vulture_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Vulture Tactical White") },
            { "paintjob_diamondbackxl_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Tactical Grey") },
            { "paintjob_python_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Tactical Grey") },
            { "paintjob_krait_light_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Light Tactical Grey") },
            { "paintjob_cutter_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Militaire Earth Yellow") },
            { "paintjob_anaconda_fullmetal_cobalt", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Fullmetal Cobalt") },

            { "nameplate_expedition02_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Nameplate Expedition 2 Grey") },
            { "paintjob_adder_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Adder Lrpo Azure") },
            { "paintjob_adder_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Adder Vibrant Orange") },
            { "paintjob_anaconda_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Horus 1 2") },
            { "paintjob_anaconda_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Horus 1 3") },
            { "paintjob_anaconda_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Horus 2 1") },
            { "paintjob_anaconda_icarus_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Icarus Grey") },
            { "paintjob_anaconda_iridescentblack_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Iridescentblack 2") },
            { "paintjob_anaconda_lowlighteffect_01_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Lowlighteffect 1 1") },
            { "paintjob_anaconda_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Anaconda Militaire Forest Green") },
            { "paintjob_anaconda_militaire_sand", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Militaire Sand") },
            { "paintjob_anaconda_prestige_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Prestige Blue") },
            { "paintjob_anaconda_prestige_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Prestige Green") },
            { "paintjob_anaconda_prestige_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Prestige Purple") },
            { "paintjob_anaconda_prestige_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Prestige Red") },
            { "paintjob_anaconda_pulse2_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda Pulse 2 Green") },
            { "paintjob_anaconda_war_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Anaconda War Orange") },
            { "paintjob_asp_icarus_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Icarus Grey") },
            { "paintjob_asp_iridescentblack_04", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Asp Iridescentblack 4") },
            { "paintjob_belugaliner_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Belugaliner Blackfriday 1") },
            { "paintjob_belugaliner_ember_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Belugaliner Ember Blue") },
            { "paintjob_cobramkiv_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cobramkiv Lrpo Azure") },
            { "paintjob_cutter_gradient2_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Gradient 2 Red") },
            { "paintjob_cutter_iridescentblack_05", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Iridescentblack 5") },
            { "paintjob_cutter_synth_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Synth Orange") },
            { "paintjob_cutter_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Tactical Blue") },
            { "paintjob_cutter_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter Vibrant Red") },
            { "paintjob_cutter_war_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Cutter War Blue") },
            { "paintjob_diamondback_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Diamond Back Black Friday 1") },
            { "paintjob_diamondback_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondback Lrpo Azure") },
            { "paintjob_diamondbackxl_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Diamondbackxl Vibrant Orange") },
            { "paintjob_dolphin_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Dolphin Vibrant Blue") },
            { "paintjob_eagle_aerial_display_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Eagle Aerial Display Red") },
            { "paintjob_eagle_stripe1_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Eagle Stripe 1 1") },
            { "paintjob_empire_courier_iridescenthighcolour_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Courier Iridescenthighcolour 2") },
            { "paintjob_empire_courier_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empire Courier Tactical White") },
            { "paintjob_empiretrader_slipstream_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Empiretrader Slipstream Orange") },
            { "paintjob_feddropship_militaire_earth_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Feddropship Militaire Earth Red") },
            { "paintjob_federation_corvette_colourgeo_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Colourgeo Grey") },
            { "paintjob_federation_corvette_razormetal_silver", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Razormetal Silver") },
            { "paintjob_federation_corvette_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Tactical Grey") },
            { "paintjob_federation_corvette_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Tactical Red") },
            { "paintjob_federation_corvette_vibrant_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Corvette Vibrant Red") },
            { "paintjob_federation_gunship_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Gunship Blackfriday 1") },
            { "paintjob_federation_gunship_militarystripe_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Federation Gunship Militarystripe Red") },
            { "paintjob_ferdelance_razormetal_copper", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Ferdelance Razormetal Copper") },
            { "paintjob_ferdelance_slipstream_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Ferdelance Slipstream Orange") },
            { "paintjob_hauler_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Hauler Tactical Red") },
            { "paintjob_hauler_vibrant_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Hauler Vibrant Blue") },
            { "paintjob_krait_light_lowlighteffect_01_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Light Lowlighteffect 1 6") },
            { "paintjob_krait_light_turbulence_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Light Turbulence 6") },
            { "paintjob_krait_mkii_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Blackfriday 1") },
            { "paintjob_krait_mkii_egypt_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Egypt 1") },
            { "paintjob_krait_mkii_horus1_02", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Horus 1 2") },
            { "paintjob_krait_mkii_horus1_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Horus 1 3") },
            { "paintjob_krait_mkii_tactical_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paint Job Krait Mk II Tactical Blue") },
            { "paintjob_krait_mkii_trims_greyorange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Trims Greyorange") },
            { "paintjob_krait_mkii_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Krait Mkii Vibrant Orange") },
            { "paintjob_mamba_tactical_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Mamba Tactical White") },
            { "paintjob_orca_corporate1_corporate1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Orca Corporate 1 Corporate 1") },
            { "paintjob_orca_geometric_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Orca Geometric Blue") },
            { "paintjob_python_egypt_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Egypt 1") },
            { "paintjob_python_horus2_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Horus 2 1") },
            { "paintjob_python_lowlighteffect_01_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Lowlighteffect 1 3") },
            { "paintjob_python_salvage_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Salvage 6") },
            { "paintjob_python_squadron_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Squadron Blue") },
            { "paintjob_python_squadron_gold", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Squadron Gold") },
            { "paintjob_python_squadron_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Squadron Red") },
            { "paintjob_python_tactical_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Python Tactical Red") },
            { "paintjob_type6_foss_orangewhite", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 6 Foss Orangewhite") },
            { "paintjob_type6_foss_whitered", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 6 Foss Whitered") },
            { "paintjob_type6_iridescentblack_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 6 Iridescentblack 3") },
            { "paintjob_type7_lrpo_azure", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 7 Lrpo Azure") },
            { "paintjob_type7_militaire_earth_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 7 Militaire Earth Yellow") },
            { "paintjob_type7_turbulence_06", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 7 Turbulence 6") },
            { "paintjob_type9_military_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Military Blackfriday 1") },
            { "paintjob_type9_military_vibrant_orange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Military Vibrant Orange") },
            { "paintjob_type9_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Tactical Grey") },
            { "paintjob_type9_turbulence_03", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Type 9 Turbulence 3") },
            { "paintjob_typex_2_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 2 Blackfriday 1") },
            { "paintjob_typex_3_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 3 Blackfriday 1") },
            { "paintjob_typex_3_military_militaire_forest_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 3 Military Militaire Forest Green") },
            { "paintjob_typex_3_military_tactical_grey", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 3 Military Tactical Grey") },
            { "paintjob_typex_3_military_vibrant_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 3 Military Vibrant Yellow") },
            { "paintjob_typex_3_trims_greyorange", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex 3 Trims Greyorange") },
            { "paintjob_typex_blackfriday_01", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Typex Blackfriday 1") },
            { "paintjob_viper_mkiv_slipstream_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper Mkiv Slipstream Blue") },
            { "paintjob_viper_predator_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Paintjob Viper Predator Blue") },
            { "python_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Bumper 1") },
            { "python_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Bumper 2") },
            { "python_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Bumper 3") },
            { "python_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Bumper 4") },
            { "python_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Spoiler 1") },
            { "python_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Spoiler 2") },
            { "python_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Spoiler 3") },
            { "python_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Spoiler 4") },
            { "python_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Tail 1") },
            { "python_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Tail 2") },
            { "python_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Tail 3") },
            { "python_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Tail 4") },
            { "python_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Wings 1") },
            { "python_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Wings 2") },
            { "python_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Wings 3") },
            { "python_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 1 Wings 4") },
            { "python_shipkit2raider_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Bumper 1") },
            { "python_shipkit2raider_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Bumper 3") },
            { "python_shipkit2raider_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Spoiler 1") },
            { "python_shipkit2raider_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Spoiler 2") },
            { "python_shipkit2raider_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Tail 1") },
            { "python_shipkit2raider_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Tail 3") },
            { "python_shipkit2raider_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Wings 2") },
            { "python_shipkit2raider_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Shipkit 2 Raider Wings 3") },
            { "python_nx_strike_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Nx Strike Spoiler 1") },
            { "python_nx_strike_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Nx Strike Wings 1") },
            { "python_nx_strike_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Python Nx Strike Bumper 1") },
            { "sidewinder_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Bumper 1") },
            { "sidewinder_shipkit1_bumper2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Bumper 2") },
            { "sidewinder_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Bumper 4") },
            { "sidewinder_shipkit1_spoiler1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Spoiler 1") },
            { "sidewinder_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Spoiler 3") },
            { "sidewinder_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Tail 1") },
            { "sidewinder_shipkit1_tail3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Tail 3") },
            { "sidewinder_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Tail 4") },
            { "sidewinder_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Wings 2") },
            { "sidewinder_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Wings 3") },
            { "sidewinder_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Sidewinder Shipkit 1 Wings 4") },
            { "string_lights_coloured", new ShipModule(999999941,ShipModule.ModuleTypes.VanityType,0,0,"String Lights Coloured") },
            { "string_lights_thargoidprobe", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"String Lights Thargoid probe") },
            { "string_lights_warm_white", new ShipModule(999999944,ShipModule.ModuleTypes.VanityType,0,0,"String Lights Warm White") },
            { "string_lights_skull", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"String Lights Skull") },
            { "type6_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Bumper 1") },
            { "type6_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Spoiler 3") },
            { "type6_shipkit1_wings1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Wings 1") },
            { "type9_military_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 9 Military Ship Kit 1 Bumper 4") },
            { "type9_military_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 9 Military Ship Kit 1 Spoiler 3") },
            { "type9_military_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 9 Military Ship Kit 1 Wings 3") },
            { "type9_military_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 9 Military Shipkit 1 Bumper 3") },
            { "type9_military_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 9 Military Shipkit 1 Spoiler 2") },
            { "typex_3_shipkit1_bumper3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Typex 3 Shipkit 1 Bumper 3") },
            { "typex_3_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Typex 3 Shipkit 1 Spoiler 3") },
            { "typex_3_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Typex 3 Shipkit 1 Wings 4") },
            { "viper_shipkit1_bumper4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Viper Shipkit 1 Bumper 4") },
            { "viper_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Viper Shipkit 1 Spoiler 4") },
            { "viper_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Viper Shipkit 1 Tail 4") },
            { "viper_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Viper Shipkit 1 Wings 4") },
            { "voicepack_verity", new ShipModule(999999901,ShipModule.ModuleTypes.VanityType,0,0,"Voice Pack Verity") },
            { "voicepack_alix", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Alix") },
            { "voicepack_amelie", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Amelie") },
            { "voicepack_archer", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Archer") },
            { "voicepack_carina", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Carina") },
            { "voicepack_celeste", new ShipModule(999999904,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Celeste") },
            { "voicepack_eden", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Eden") },
            { "voicepack_gerhard", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Gerhard") },
            { "voicepack_jefferson", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Jefferson") },
            { "voicepack_leo", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Leo") },
            { "voicepack_luciana", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Luciana") },
            { "voicepack_victor", new ShipModule(999999902,ShipModule.ModuleTypes.VanityType,0,0,"Voicepack Victor") },
            { "vulture_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Vulture Shipkit 1 Bumper 1") },
            { "vulture_shipkit1_spoiler3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Vulture Shipkit 1 Spoiler 3") },
            { "vulture_shipkit1_spoiler4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Vulture Shipkit 1 Spoiler 4") },
            { "vulture_shipkit1_tail1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Vulture Shipkit 1 Tail 1") },
            { "vulture_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Vulture Shipkit 1 Wings 2") },
            { "weaponcustomisation_blue", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Blue") },
            { "weaponcustomisation_cyan", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Cyan") },
            { "weaponcustomisation_green", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Green") },
            { "weaponcustomisation_pink", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Pink") },
            { "weaponcustomisation_purple", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Purple") },
            { "weaponcustomisation_red", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Red") },
            { "weaponcustomisation_white", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation White") },
            { "weaponcustomisation_yellow", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Weapon Customisation Yellow") },

            { "krait_mkii_shipkit1_tail4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Mkii Shipkit 1 Tail 4") },
            { "cutter_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Cutter Shipkit 1 Bumper 1") },
            { "type6_shipkit1_spoiler2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Spoiler 2") },
            { "type6_shipkit1_wings4", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Wings 4") },
            { "type6_shipkit1_wings3", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Type 6 Shipkit 1 Wings 3") },
            { "empire_courier_shipkit1_bumper1", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Empire Courier Shipkit 1 Bumper 1") },
            { "federation_corvette_shipkit1_wings2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Federation Corvette Shipkit 1 Wings 2") },
            { "krait_light_shipkit1_tail2", new ShipModule(-1,ShipModule.ModuleTypes.VanityType,0,0,"Krait Light Shipkit 1 Tail 2") },

            { "paint", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,0,0,"Paint") },
            { "all", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,0,0,"Repair All") },
            { "hull", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,0,0,"Repair All") },
            { "wear", new ShipModule(-1,ShipModule.ModuleTypes.WearAndTearType,0,0,"Wear") },
        };
        #endregion

    #region Synth Modules

static private Dictionary<string, ShipModule> synthesisedmodules = new Dictionary<string, ShipModule>();        // ones made by edd

        #endregion

    }
}
