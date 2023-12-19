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
 * Data is intellectual property and copyright of Frontier Developments plc ('Frontier', 'Frontier Developments') and are subject to their terms and conditions.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        static public bool TryGetShipModule(string fdid, out ShipModule m, bool synthesiseit )
        {
            m = null;
            string lowername = fdid.ToLowerInvariant();

            // try the static values first, this is thread safe
            bool state = shipmodules.TryGetValue(lowername, out m) || othershipmodules.TryGetValue(lowername, out m) ||
                        srvmodules.TryGetValue(lowername, out m) || fightermodules.TryGetValue(lowername, out m) || vanitymodules.TryGetValue(lowername, out m);

            if ( state == false)    // not found, try find the synth modules. Since we can be called in journal creation thread, we need some safety.
            {
                lock(synthesisedmodules)
                {
                    state = synthesisedmodules.TryGetValue(lowername, out m);
                }
            }

            if ( !state && synthesiseit )   // if not found, and we want to synthesise it
            {
                lock (synthesisedmodules)  // lock for safety
                {
                    string candidatename = fdid;
                    candidatename = candidatename.Replace("weaponcustomisation", "WeaponCustomisation").Replace("testbuggy", "SRV").
                                            Replace("enginecustomisation", "EngineCustomisation");

                    candidatename = candidatename.SplitCapsWordFull();

                    var newmodule = new ShipModule(-1, 0, candidatename, IsVanity(lowername) ? ShipModule.ModuleTypes.VanityType : ShipModule.ModuleTypes.UnknownType);

                    System.Diagnostics.Debug.WriteLine("******* Unknown Module { \"" + lowername + "\", new ShipModule(-1,0, \"" + newmodule.ModName + "\", " + (IsVanity(lowername) ? "VanityType" : "UnknownType") + " ) },");

                    synthesisedmodules[lowername] = m = newmodule;                   // lets cache them for completeness..
                }
            }

            return state;
        }

        // List of ship modules. Synthesised are not included
        static public List<ShipModule> GetShipModulesList(bool includenonbuyable = false, bool includesrv = false, bool includefighter = false, bool includevanity = false,  bool addunknowntype = false)       
        {
            List<ShipModule> mlist = new List<ShipModule>(shipmodules.Values);
            if (includenonbuyable)
                mlist.AddRange(othershipmodules.Values);
            if (includesrv)
                mlist.AddRange(srvmodules.Values);
            if (includefighter)
                mlist.AddRange(fightermodules.Values);
            if (includevanity)
                mlist.AddRange(vanitymodules.Values);
            if (addunknowntype)
                mlist.Add(new ShipModule(-1, 0, "Unknown Type", ShipModule.ModuleTypes.UnknownType));
            return mlist;
        }

        // Dictionary of ship modules, merged. Using this can show up repeats between dictionaries. Synthesised are not included
        static public Dictionary<string,ShipModule> GetShipModuleDictionary(bool includenonbuyable = false, bool includesrv = false, bool includefighter = false, bool includevanity = false)
        {
            Dictionary<string, ShipModule> res = new Dictionary<string, ShipModule>(shipmodules);
            if (includenonbuyable)
            {
                foreach (var kvp in othershipmodules)
                    res.Add(kvp.Key, kvp.Value);
            }
            if (includesrv)
            {
                foreach (var kvp in srvmodules)
                    res.Add(kvp.Key, kvp.Value);
            }
            if (includefighter)
            {
                foreach (var kvp in fightermodules)
                    res.Add(kvp.Key, kvp.Value);
            }
            if (includevanity)
            {
                foreach (var kvp in vanitymodules)
                    res.Add(kvp.Key, kvp.Value);
            }

            return res;
        }

        static public bool IsVanity(string ifd)
        {
            ifd = ifd.ToLowerInvariant();
            string[] vlist = new[] { "bobble", "decal", "enginecustomisation", "nameplate", "paintjob",
                                    "shipkit", "weaponcustomisation", "voicepack" , "lights", "spoiler" , "wings", "bumper"};
            return Array.Find(vlist, x => ifd.Contains(x)) != null;
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

                // EDD, not buyable
                DiscoveryScanner,PrisonCells,DataLinkScanner,SRVScanner,FighterWeapon,
                VanityType,UnknownType,CockpitType,CargoBayDoorType,WearAndTearType,Codex,
            };

            public string ModName { get; set; }
            public int ModuleID { get; set; }
            public double Mass { get; set; }
            public ModuleTypes ModType { get; set; }
            public string ModTypeString {  get { return ModType.ToString().Replace("AX","AX ").Replace("_", "-").SplitCapsWordFull(); } }     // string should be in spansh/EDCD csv compatible format
            public double Power { get; set; }
            public string Info { get; set; }
            
            public bool IsBuyable { get { return !(ModType == ModuleTypes.VanityType || ModType == ModuleTypes.UnknownType || ModType == ModuleTypes.CockpitType || ModType == ModuleTypes.CargoBayDoorType || ModType == ModuleTypes.WearAndTearType || ModType == ModuleTypes.Codex); } }

            public ShipModule(int id, double mass, string descr, ModuleTypes modtype) { ModuleID = id; Mass = mass; ModName = descr; ModType = modtype; }
            public ShipModule(int id, double mass, double power, string descr, ModuleTypes modtype) { ModuleID = id; Mass = mass; Power = power; ModName = descr; ModType = modtype; }
            public ShipModule(int id, double mass, double power, string info, string descr, ModuleTypes modtype) { ModuleID = id; Mass = mass; Power = power; Info = info; ModName = descr; ModType = modtype; }

            public string InfoMassPower(bool mass)
            {
                string i = (Info ?? "").AppendPrePad(Power > 0 ? ("Power:" + Power.ToString("0.#MW")) : "", ", ");
                if (mass)
                    return i.AppendPrePad(Mass > 0 ? ("Mass:" + Mass.ToString("0.#t")) : "", ", ");
                else
                    return i;
            }

            public override string ToString()
            {
                string i = Info == null ? "null" : $"\"{Info}\"";
                return $"{ModuleID},{Mass:0.##},{Power:0.##},{i},\"{ModName}\",{ModTypeString}";
                //return $"{ModuleID}, {Mass:0.##}, {Power:0.##}, {i}, \"{ModName}\", {mt}";
            }

        };

        #endregion

        // History
        // Originally from coriolis
        // Nov 1/12/23 synched with EDDI data, with outfitting.csv

        #region Ship Modules

        public static Dictionary<string, ShipModule> shipmodules = new Dictionary<string, ShipModule>
        {
            // Armour, in ID order

            { "sidewinder_armour_grade1", new ShipModule(128049250,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Sidewinder Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },   // EDDI
            { "sidewinder_armour_grade2", new ShipModule(128049251,2,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Sidewinder Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "sidewinder_armour_grade3", new ShipModule(128049252,4,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Sidewinder Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "sidewinder_armour_mirrored", new ShipModule(128049253,4,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Sidewinder Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "sidewinder_armour_reactive", new ShipModule(128049254,4,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Sidewinder Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "eagle_armour_grade1", new ShipModule(128049256,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Eagle Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "eagle_armour_grade2", new ShipModule(128049257,4,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Eagle Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "eagle_armour_grade3", new ShipModule(128049258,8,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Eagle Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "eagle_armour_mirrored", new ShipModule(128049259,8,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Eagle Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "eagle_armour_reactive", new ShipModule(128049260,8,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Eagle Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "hauler_armour_grade1", new ShipModule(128049262,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Hauler Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },   // EDDI
            { "hauler_armour_grade2", new ShipModule(128049263,1,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Hauler Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "hauler_armour_grade3", new ShipModule(128049264,2,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Hauler Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "hauler_armour_mirrored", new ShipModule(128049265,2,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Hauler Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "hauler_armour_reactive", new ShipModule(128049266,2,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Hauler Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "adder_armour_grade1", new ShipModule(128049268,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Adder Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "adder_armour_grade2", new ShipModule(128049269,3,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Adder Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "adder_armour_grade3", new ShipModule(128049270,5,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Adder Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "adder_armour_mirrored", new ShipModule(128049271,5,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Adder Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "adder_armour_reactive", new ShipModule(128049272,5,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Adder Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "viper_armour_grade1", new ShipModule(128049274,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "viper_armour_grade2", new ShipModule(128049275,5,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "viper_armour_grade3", new ShipModule(128049276,9,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "viper_armour_mirrored", new ShipModule(128049277,9,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Viper Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "viper_armour_reactive", new ShipModule(128049278,9,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Viper Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "cobramkiii_armour_grade1", new ShipModule(128049280,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk III Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "cobramkiii_armour_grade2", new ShipModule(128049281,14,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk III Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "cobramkiii_armour_grade3", new ShipModule(128049282,27,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk III Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "cobramkiii_armour_mirrored", new ShipModule(128049283,27,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Cobra Mk III Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "cobramkiii_armour_reactive", new ShipModule(128049284,27,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Cobra Mk III Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "type6_armour_grade1", new ShipModule(128049286,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-6 Transporter Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },    // EDDI
            { "type6_armour_grade2", new ShipModule(128049287,12,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-6 Transporter Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "type6_armour_grade3", new ShipModule(128049288,23,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-6 Transporter Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "type6_armour_mirrored", new ShipModule(128049289,23,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Type-6 Transporter Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "type6_armour_reactive", new ShipModule(128049290,23,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Type-6 Transporter Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "dolphin_armour_grade1", new ShipModule(128049292,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Dolphin Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "dolphin_armour_grade2", new ShipModule(128049293,32,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Dolphin Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "dolphin_armour_grade3", new ShipModule(128049294,63,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Dolphin Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "dolphin_armour_mirrored", new ShipModule(128049295,63,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Dolphin Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "dolphin_armour_reactive", new ShipModule(128049296,63,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Dolphin Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "type7_armour_grade1", new ShipModule(128049298,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-7 Transporter Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "type7_armour_grade2", new ShipModule(128049299,32,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-7 Transporter Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "type7_armour_grade3", new ShipModule(128049300,63,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-7 Transporter Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "type7_armour_mirrored", new ShipModule(128049301,63,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Type-7 Transporter Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "type7_armour_reactive", new ShipModule(128049302,63,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Type-7 Transporter Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "asp_armour_grade1", new ShipModule(128049304,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Explorer Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "asp_armour_grade2", new ShipModule(128049305,21,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Explorer Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "asp_armour_grade3", new ShipModule(128049306,42,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Explorer Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "asp_armour_mirrored", new ShipModule(128049307,42,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Asp Explorer Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "asp_armour_reactive", new ShipModule(128049308,42,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Asp Explorer Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "vulture_armour_grade1", new ShipModule(128049310,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Vulture Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },     // EDDI
            { "vulture_armour_grade2", new ShipModule(128049311,17,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Vulture Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "vulture_armour_grade3", new ShipModule(128049312,35,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Vulture Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "vulture_armour_mirrored", new ShipModule(128049313,35,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Vulture Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "vulture_armour_reactive", new ShipModule(128049314,35,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Vulture Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "empire_trader_armour_grade1", new ShipModule(128049316,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Clipper Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },  // EDDI
            { "empire_trader_armour_grade2", new ShipModule(128049317,30,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Clipper Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "empire_trader_armour_grade3", new ShipModule(128049318,60,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Clipper Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "empire_trader_armour_mirrored", new ShipModule(128049319,60,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Imperial Clipper Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "empire_trader_armour_reactive", new ShipModule(128049320,60,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Imperial Clipper Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "federation_dropship_armour_grade1", new ShipModule(128049322,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Dropship Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },    // EDDI
            { "federation_dropship_armour_grade2", new ShipModule(128049323,44,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Dropship Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "federation_dropship_armour_grade3", new ShipModule(128049324,87,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Dropship Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "federation_dropship_armour_mirrored", new ShipModule(128049325,87,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Federal Dropship Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "federation_dropship_armour_reactive", new ShipModule(128049326,87,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Federal Dropship Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "orca_armour_grade1", new ShipModule(128049328,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Orca Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "orca_armour_grade2", new ShipModule(128049329,21,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Orca Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "orca_armour_grade3", new ShipModule(128049330,87,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Orca Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "orca_armour_mirrored", new ShipModule(128049331,87,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Orca Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "orca_armour_reactive", new ShipModule(128049332,87,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Orca Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "type9_armour_grade1", new ShipModule(128049334,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-9 Heavy Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "type9_armour_grade2", new ShipModule(128049335,75,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-9 Heavy Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "type9_armour_grade3", new ShipModule(128049336,150,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-9 Heavy Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "type9_armour_mirrored", new ShipModule(128049337,150,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Type-9 Heavy Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "type9_armour_reactive", new ShipModule(128049338,150,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Type-9 Heavy Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "python_armour_grade1", new ShipModule(128049340,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Python Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },   // EDDI
            { "python_armour_grade2", new ShipModule(128049341,26,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Python Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "python_armour_grade3", new ShipModule(128049342,53,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Python Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "python_armour_mirrored", new ShipModule(128049343,53,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Python Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "python_armour_reactive", new ShipModule(128049344,53,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Python Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "belugaliner_armour_grade1", new ShipModule(128049346,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Beluga Liner Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },    // EDDI
            { "belugaliner_armour_grade2", new ShipModule(128049347,83,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Beluga Liner Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "belugaliner_armour_grade3", new ShipModule(128049348,165,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Beluga Liner Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "belugaliner_armour_mirrored", new ShipModule(128049349,165,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Beluga Liner Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "belugaliner_armour_reactive", new ShipModule(128049350,165,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Beluga Liner Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "ferdelance_armour_grade1", new ShipModule(128049352,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Fer-de-Lance Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "ferdelance_armour_grade2", new ShipModule(128049353,19,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Fer-de-Lance Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "ferdelance_armour_grade3", new ShipModule(128049354,38,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Fer-de-Lance Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "ferdelance_armour_mirrored", new ShipModule(128049355,38,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Fer-de-Lance Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "ferdelance_armour_reactive", new ShipModule(128049356,38,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Fer-de-Lance Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "anaconda_armour_grade1", new ShipModule(128049364,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Anaconda Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },   // EDDI
            { "anaconda_armour_grade2", new ShipModule(128049365,30,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Anaconda Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "anaconda_armour_grade3", new ShipModule(128049366,60,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Anaconda Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "anaconda_armour_mirrored", new ShipModule(128049367,60,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Anaconda Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "anaconda_armour_reactive", new ShipModule(128049368,60,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Anaconda Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "federation_corvette_armour_grade1", new ShipModule(128049370,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Corvette Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },    // EDDI
            { "federation_corvette_armour_grade2", new ShipModule(128049371,30,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Corvette Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "federation_corvette_armour_grade3", new ShipModule(128049372,60,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Corvette Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "federation_corvette_armour_mirrored", new ShipModule(128049373,60,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Federal Corvette Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "federation_corvette_armour_reactive", new ShipModule(128049374,60,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Federal Corvette Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "cutter_armour_grade1", new ShipModule(128049376,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Cutter Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "cutter_armour_grade2", new ShipModule(128049377,30,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Cutter Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "cutter_armour_grade3", new ShipModule(128049378,60,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Cutter Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "cutter_armour_mirrored", new ShipModule(128049379,60,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Imperial Cutter Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "cutter_armour_reactive", new ShipModule(128049380,60,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Imperial Cutter Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "diamondbackxl_armour_grade1", new ShipModule(128671832,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Explorer Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },  // EDDI
            { "diamondbackxl_armour_grade2", new ShipModule(128671833,23,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Explorer Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "diamondbackxl_armour_grade3", new ShipModule(128671834,47,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Explorer Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "diamondbackxl_armour_mirrored", new ShipModule(128671835,26,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Diamondback Explorer Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "diamondbackxl_armour_reactive", new ShipModule(128671836,47,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Diamondback Explorer Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },


            { "empire_eagle_armour_grade1", new ShipModule(128672140,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Eagle Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "empire_eagle_armour_grade2", new ShipModule(128672141,4,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Eagle Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "empire_eagle_armour_grade3", new ShipModule(128672142,8,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Eagle Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "empire_eagle_armour_mirrored", new ShipModule(128672143,8,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Imperial Eagle Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "empire_eagle_armour_reactive", new ShipModule(128672144,8,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Imperial Eagle Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "federation_dropship_mkii_armour_grade1", new ShipModule(128672147,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Assault Ship Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "federation_dropship_mkii_armour_grade2", new ShipModule(128672148,44,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Assault Ship Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "federation_dropship_mkii_armour_grade3", new ShipModule(128672149,87,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Assault Ship Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "federation_dropship_mkii_armour_mirrored", new ShipModule(128672150,87,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Federal Assault Ship Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "federation_dropship_mkii_armour_reactive", new ShipModule(128672151,87,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Federal Assault Ship Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "federation_gunship_armour_grade1", new ShipModule(128672154,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Gunship Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "federation_gunship_armour_grade2", new ShipModule(128672155,44,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Gunship Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "federation_gunship_armour_grade3", new ShipModule(128672156,87,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Federal Gunship Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "federation_gunship_armour_mirrored", new ShipModule(128672157,87,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Federal Gunship Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "federation_gunship_armour_reactive", new ShipModule(128672158,87,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Federal Gunship Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "viper_mkiv_armour_grade1", new ShipModule(128672257,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Mk IV Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },      // EDDI
            { "viper_mkiv_armour_grade2", new ShipModule(128672258,5,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Mk IV Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "viper_mkiv_armour_grade3", new ShipModule(128672259,9,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Viper Mk IV Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "viper_mkiv_armour_mirrored", new ShipModule(128672260,9,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Viper Mk IV Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "viper_mkiv_armour_reactive", new ShipModule(128672261,9,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Viper Mk IV Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "cobramkiv_armour_grade1", new ShipModule(128672264,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk IV Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "cobramkiv_armour_grade2", new ShipModule(128672265,14,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk IV Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "cobramkiv_armour_grade3", new ShipModule(128672266,27,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Cobra Mk IV Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "cobramkiv_armour_mirrored", new ShipModule(128672267,27,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Cobra Mk IV Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "cobramkiv_armour_reactive", new ShipModule(128672268,27,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Cobra Mk IV Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "independant_trader_armour_grade1", new ShipModule(128672271,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Keelback Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "independant_trader_armour_grade2", new ShipModule(128672272,12,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Keelback Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "independant_trader_armour_grade3", new ShipModule(128672273,23,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Keelback Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "independant_trader_armour_mirrored", new ShipModule(128672274,23,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Keelback Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "independant_trader_armour_reactive", new ShipModule(128672275,23,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Keelback Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "asp_scout_armour_grade1", new ShipModule(128672278,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Scout Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "asp_scout_armour_grade2", new ShipModule(128672279,21,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Scout Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "asp_scout_armour_grade3", new ShipModule(128672280,42,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Asp Scout Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "asp_scout_armour_mirrored", new ShipModule(128672281,42,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Asp Scout Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "asp_scout_armour_reactive", new ShipModule(128672282,42,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Asp Scout Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },


            { "krait_mkii_armour_grade1", new ShipModule(128816569,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Mk II Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "krait_mkii_armour_grade2", new ShipModule(128816570,36,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Mk II Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "krait_mkii_armour_grade3", new ShipModule(128816571,67,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Mk II Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "krait_mkii_armour_mirrored", new ShipModule(128816572,67,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Krait Mk II Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "krait_mkii_armour_reactive", new ShipModule(128816573,67,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Krait Mk II Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "typex_armour_grade1", new ShipModule(128816576,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Chieftain Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },// EDDI
            { "typex_armour_grade2", new ShipModule(128816577,40,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Chieftain Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "typex_armour_grade3", new ShipModule(128816578,78,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Chieftain Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "typex_armour_mirrored", new ShipModule(128816579,78,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Alliance Chieftain Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "typex_armour_reactive", new ShipModule(128816580,78,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Alliance Chieftain Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "typex_2_armour_grade1", new ShipModule(128816583,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Crusader Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },// EDDI
            { "typex_2_armour_grade2", new ShipModule(128816584,40,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Crusader Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "typex_2_armour_grade3", new ShipModule(128816585,78,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Crusader Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "typex_2_armour_mirrored", new ShipModule(128816586,78,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Alliance Crusader Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "typex_2_armour_reactive", new ShipModule(128816587,78,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Alliance Crusader Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "typex_3_armour_grade1", new ShipModule(128816590,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Challenger Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },// EDDI
            { "typex_3_armour_grade2", new ShipModule(128816591,40,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Challenger Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "typex_3_armour_grade3", new ShipModule(128816592,78,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Alliance Challenger Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "typex_3_armour_mirrored", new ShipModule(128816593,78,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Alliance Challenger Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "typex_3_armour_reactive", new ShipModule(128816594,78,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Alliance Challenger Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "diamondback_armour_grade1", new ShipModule(128671218,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Scout Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) },       // EDDI
            { "diamondback_armour_grade2", new ShipModule(128671219,13,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Scout Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "diamondback_armour_grade3", new ShipModule(128671220,26,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Diamondback Scout Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "diamondback_armour_mirrored", new ShipModule(128671221,26,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Diamondback Scout Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "diamondback_armour_reactive", new ShipModule(128671222,26,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Diamondback Scout Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "empire_courier_armour_grade1", new ShipModule(128671224,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Courier Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "empire_courier_armour_grade2", new ShipModule(128671225,4,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Courier Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "empire_courier_armour_grade3", new ShipModule(128671226,8,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Imperial Courier Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "empire_courier_armour_mirrored", new ShipModule(128671227,8,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Imperial Courier Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "empire_courier_armour_reactive", new ShipModule(128671228,8,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Imperial Courier Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "type9_military_armour_grade1", new ShipModule(128785621,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-10 Defender Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "type9_military_armour_grade2", new ShipModule(128785622,75,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-10 Defender Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "type9_military_armour_grade3", new ShipModule(128785623,150,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Type-10 Defender Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "type9_military_armour_mirrored", new ShipModule(128785624,150,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Type-10 Defender Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "type9_military_armour_reactive", new ShipModule(128785625,150,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Type-10 Defender Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "krait_light_armour_grade1", new ShipModule(128839283,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Phantom Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, //EDDI
            { "krait_light_armour_grade2", new ShipModule(128839284,26,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Phantom Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "krait_light_armour_grade3", new ShipModule(128839285,53,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Krait Phantom Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "krait_light_armour_mirrored", new ShipModule(128839286,53,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Krait Phantom Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "krait_light_armour_reactive", new ShipModule(128839287,53,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Krait Phantom Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            { "mamba_armour_grade1", new ShipModule(128915981,0,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Mamba Lightweight Armour",ShipModule.ModuleTypes.LightweightAlloy) }, // EDDI
            { "mamba_armour_grade2", new ShipModule(128915982,19,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Mamba Reinforced Armour",ShipModule.ModuleTypes.ReinforcedAlloy) },
            { "mamba_armour_grade3", new ShipModule(128915983,38,0,"Explosive:-40%, Kinetic:-20%, Thermal:0%","Mamba Military Armour",ShipModule.ModuleTypes.MilitaryGradeComposite) },
            { "mamba_armour_mirrored", new ShipModule(128915984,38,0,"Explosive:-50%, Kinetic:-75%, Thermal:50%","Mamba Mirrored Surface Composite Armour",ShipModule.ModuleTypes.MirroredSurfaceComposite) },
            { "mamba_armour_reactive", new ShipModule(128915985,38,0,"Explosive:20%, Kinetic:25%, Thermal:-40%","Mamba Reactive Surface Composite Armour",ShipModule.ModuleTypes.ReactiveSurfaceComposite) },

            // Auto field maint

            { "int_repairer_size1_class1", new ShipModule(128667598,0,0.54,"Ammo:1000, Repair:12","Auto Field Maintenance Class 1 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },    //EDDI
            { "int_repairer_size1_class2", new ShipModule(128667606,0,0.72,"Ammo:900, Repair:14.4","Auto Field Maintenance Class 1 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size1_class3", new ShipModule(128667614,0,0.9,"Ammo:1000, Repair:20","Auto Field Maintenance Class 1 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size1_class4", new ShipModule(128667622,0,1.04,"Ammo:1200, Repair:27.6","Auto Field Maintenance Class 1 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size1_class5", new ShipModule(128667630,0,1.26,"Ammo:1100, Repair:30.8","Auto Field Maintenance Class 1 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size2_class1", new ShipModule(128667599,0,0.68,"Ammo:2300, Repair:27.6","Auto Field Maintenance Class 2 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size2_class2", new ShipModule(128667607,0,0.9,"Ammo:2100, Repair:33.6","Auto Field Maintenance Class 2 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size2_class3", new ShipModule(128667615,0,1.13,"Ammo:2300, Repair:46","Auto Field Maintenance Class 2 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size2_class4", new ShipModule(128667623,0,1.29,"Ammo:2800, Repair:64.4","Auto Field Maintenance Class 2 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size2_class5", new ShipModule(128667631,0,1.58,"Ammo:2500, Repair:70","Auto Field Maintenance Class 2 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size3_class1", new ShipModule(128667600,0,0.81,"Ammo:3600, Repair:43.2","Auto Field Maintenance Class 3 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size3_class2", new ShipModule(128667608,0,1.08,"Ammo:3200, Repair:51.2","Auto Field Maintenance Class 3 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size3_class3", new ShipModule(128667616,0,1.35,"Ammo:3600, Repair:72","Auto Field Maintenance Class 3 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size3_class4", new ShipModule(128667624,0,1.55,"Ammo:4300, Repair:98.9","Auto Field Maintenance Class 3 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size3_class5", new ShipModule(128667632,0,1.89,"Ammo:4000, Repair:112","Auto Field Maintenance Class 3 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size4_class1", new ShipModule(128667601,0,0.99,"Ammo:4900, Repair:58.8","Auto Field Maintenance Class 4 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size4_class2", new ShipModule(128667609,0,1.32,"Ammo:4400, Repair:70.4","Auto Field Maintenance Class 4 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size4_class3", new ShipModule(128667617,0,1.65,"Ammo:4900, Repair:98","Auto Field Maintenance Class 4 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size4_class4", new ShipModule(128667625,0,1.9,"Ammo:5900, Repair:135.7","Auto Field Maintenance Class 4 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size4_class5", new ShipModule(128667633,0,2.31,"Ammo:5400, Repair:151.2","Auto Field Maintenance Class 4 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size5_class1", new ShipModule(128667602,0,1.17,"Ammo:6100, Repair:73.2","Auto Field Maintenance Class 5 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size5_class2", new ShipModule(128667610,0,1.56,"Ammo:5500, Repair:88","Auto Field Maintenance Class 5 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size5_class3", new ShipModule(128667618,0,1.95,"Ammo:6100, Repair:122","Auto Field Maintenance Class 5 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size5_class4", new ShipModule(128667626,0,2.24,"Ammo:7300, Repair:167.9","Auto Field Maintenance Class 5 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size5_class5", new ShipModule(128667634,0,2.73,"Ammo:6700, Repair:187.6","Auto Field Maintenance Class 5 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size6_class1", new ShipModule(128667603,0,1.4,"Ammo:7400, Repair:88.8","Auto Field Maintenance Class 6 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size6_class2", new ShipModule(128667611,0,1.86,"Ammo:6700, Repair:107.2","Auto Field Maintenance Class 6 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size6_class3", new ShipModule(128667619,0,2.33,"Ammo:7400, Repair:148","Auto Field Maintenance Class 6 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size6_class4", new ShipModule(128667627,0,2.67,"Ammo:8900, Repair:204.7","Auto Field Maintenance Class 6 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size6_class5", new ShipModule(128667635,0,3.26,"Ammo:8100, Repair:226.8","Auto Field Maintenance Class 6 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size7_class1", new ShipModule(128667604,0,1.58,"Ammo:8700, Repair:104.4","Auto Field Maintenance Class 7 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size7_class2", new ShipModule(128667612,0,2.1,"Ammo:7800, Repair:124.8","Auto Field Maintenance Class 7 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size7_class3", new ShipModule(128667620,0,2.63,"Ammo:8700, Repair:174","Auto Field Maintenance Class 7 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size7_class4", new ShipModule(128667628,0,3.02,"Ammo:10400, Repair:239.2","Auto Field Maintenance Class 7 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size7_class5", new ShipModule(128667636,0,3.68,"Ammo:9600, Repair:268.8","Auto Field Maintenance Class 7 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size8_class1", new ShipModule(128667605,0,1.8,"Ammo:10000, Repair:120","Auto Field Maintenance Class 8 Rating E",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size8_class2", new ShipModule(128667613,0,2.4,"Ammo:9000, Repair:144","Auto Field Maintenance Class 8 Rating D",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size8_class3", new ShipModule(128667621,0,3,"Ammo:10000, Repair:200","Auto Field Maintenance Class 8 Rating C",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size8_class4", new ShipModule(128667629,0,3.45,"Ammo:12000, Repair:276","Auto Field Maintenance Class 8 Rating B",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },
            { "int_repairer_size8_class5", new ShipModule(128667637,0,4.2,"Ammo:11000, Repair:308","Auto Field Maintenance Class 8 Rating A",ShipModule.ModuleTypes.AutoField_MaintenanceUnit) },

            // Beam lasers

            { "hpt_beamlaser_fixed_small", new ShipModule(128049428,2,0.62,"Damage:9.8, Range:3000m, ThermL:3.5","Beam Laser Fixed Small",ShipModule.ModuleTypes.BeamLaser) },      // EDDI
            { "hpt_beamlaser_fixed_medium", new ShipModule(128049429,4,1.01,"Damage:16, Range:3000m, ThermL:5.1","Beam Laser Fixed Medium",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_fixed_large", new ShipModule(128049430,8,1.62,"Damage:25.8, Range:3000m, ThermL:7.2","Beam Laser Fixed Large",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_fixed_huge", new ShipModule(128049431,16,2.61,"Damage:41.4, Range:3000m, ThermL:9.9","Beam Laser Fixed Huge",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_small", new ShipModule(128049432,2,0.6,"Damage:7.7, Range:3000m, ThermL:3.6","Beam Laser Gimbal Small",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_medium", new ShipModule(128049433,4,1,"Damage:12.5, Range:3000m, ThermL:5.3","Beam Laser Gimbal Medium",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_large", new ShipModule(128049434,8,1.6,"Damage:20.3, Range:3000m, ThermL:7.6","Beam Laser Gimbal Large",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_turret_small", new ShipModule(128049435,2,0.57,"Damage:5.4, Range:3000m, ThermL:2.4","Beam Laser Turret Small",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_turret_medium", new ShipModule(128049436,4,0.93,"Damage:8.8, Range:3000m, ThermL:3.5","Beam Laser Turret Medium",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_turret_large", new ShipModule(128049437,8,1.51,"Damage:14.3, Range:3000m, ThermL:5.1","Beam Laser Turret Large",ShipModule.ModuleTypes.BeamLaser) },

            { "hpt_beamlaser_fixed_small_heat", new ShipModule(128671346,2,0.62,"Damage:4.9, Range:3000m, ThermL:2.7","Beam Laser Fixed Small Heat",ShipModule.ModuleTypes.RetributorBeamLaser) },        // EDDI
            { "hpt_beamlaser_gimbal_huge", new ShipModule(128681994,16,2.57,"Damage:32.7, Range:3000m, ThermL:10.6","Beam Laser Gimbal Huge",ShipModule.ModuleTypes.BeamLaser) },

            // burst laser

            { "hpt_pulselaserburst_fixed_small", new ShipModule(128049400,2,0.65,"Damage:1.7, Range:3000m, ThermL:0.4","Burst Laser Fixed Small",ShipModule.ModuleTypes.BurstLaser) },      // EDDI
            { "hpt_pulselaserburst_fixed_medium", new ShipModule(128049401,4,1.05,"Damage:3.5, Range:3000m, ThermL:0.8","Burst Laser Fixed Medium",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_fixed_large", new ShipModule(128049402,8,1.66,"Damage:7.7, Range:3000m, ThermL:1.7","Burst Laser Fixed Large",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_fixed_huge", new ShipModule(128049403,16,2.58,"Damage:20.6, Range:3000m, ThermL:4.5","Burst Laser Fixed Huge",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_gimbal_small", new ShipModule(128049404,2,0.64,"Damage:1.2, Range:3000m, ThermL:0.3","Burst Laser Gimbal Small",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_gimbal_medium", new ShipModule(128049405,4,1.04,"Damage:2.5, Range:3000m, ThermL:0.7","Burst Laser Gimbal Medium",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_gimbal_large", new ShipModule(128049406,8,1.65,"Damage:5.2, Range:3000m, ThermL:1.4","Burst Laser Gimbal Large",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_turret_small", new ShipModule(128049407,2,0.6,"Damage:0.9, Range:3000m, ThermL:0.2","Burst Laser Turret Small",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_turret_medium", new ShipModule(128049408,4,0.98,"Damage:1.7, Range:3000m, ThermL:0.4","Burst Laser Turret Medium",ShipModule.ModuleTypes.BurstLaser) },
            { "hpt_pulselaserburst_turret_large", new ShipModule(128049409,8,1.57,"Damage:3.5, Range:3000m, ThermL:0.8","Burst Laser Turret Large",ShipModule.ModuleTypes.BurstLaser) },


            { "hpt_pulselaserburst_gimbal_huge", new ShipModule(128727920,16,2.59,"Damage:12.1, Range:3000m, ThermL:3.3","Burst Laser Gimbal Huge",ShipModule.ModuleTypes.BurstLaser) },                // EDDI

            { "hpt_pulselaserburst_fixed_small_scatter", new ShipModule(128671449,2,0.8,"Damage:3.6, Range:1000m, ThermL:0.3","Burst Laser Fixed Small Scatter",ShipModule.ModuleTypes.CytoscramblerBurstLaser) },   // EDDI

            // Cannons

            { "hpt_cannon_fixed_small", new ShipModule(128049438,2,0.34,"Ammo:120/6, Damage:22.5, Range:3000m, Speed:1200m/s, Reload:3s, ThermL:1.4","Cannon Fixed Small",ShipModule.ModuleTypes.Cannon) },     // EDDI
            { "hpt_cannon_fixed_medium", new ShipModule(128049439,4,0.49,"Ammo:120/6, Damage:36.5, Range:3500m, Speed:1051m/s, Reload:3s, ThermL:2.1","Cannon Fixed Medium",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_fixed_large", new ShipModule(128049440,8,0.67,"Ammo:120/6, Damage:54.9, Range:4000m, Speed:959m/s, Reload:3s, ThermL:3.2","Cannon Fixed Large",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_fixed_huge", new ShipModule(128049441,16,0.92,"Ammo:120/6, Damage:82.1, Range:4500m, Speed:900m/s, Reload:3s, ThermL:4.8","Cannon Fixed Huge",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_gimbal_small", new ShipModule(128049442,2,0.38,"Ammo:100/5, Damage:16, Range:3000m, Speed:1000m/s, Reload:4s, ThermL:1.3","Cannon Gimbal Small",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_gimbal_medium", new ShipModule(128049443,4,0.54,"Ammo:100/5, Damage:24.5, Range:3500m, Speed:875m/s, Reload:4s, ThermL:1.9","Cannon Gimbal Medium",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_gimbal_huge", new ShipModule(128049444,16,1.03,"Ammo:100/5, Damage:56.6, Range:4500m, Speed:750m/s, Reload:4s, ThermL:4.4","Cannon Gimbal Huge",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_turret_small", new ShipModule(128049445,2,0.32,"Ammo:100/5, Damage:12.8, Range:3000m, Speed:1000m/s, Reload:4s, ThermL:0.7","Cannon Turret Small",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_turret_medium", new ShipModule(128049446,4,0.45,"Ammo:100/5, Damage:19.8, Range:3500m, Speed:875m/s, Reload:4s, ThermL:1","Cannon Turret Medium",ShipModule.ModuleTypes.Cannon) },
            { "hpt_cannon_turret_large", new ShipModule(128049447,8,0.64,"Ammo:100/5, Damage:30.4, Range:4000m, Speed:800m/s, Reload:4s, ThermL:1.6","Cannon Turret Large",ShipModule.ModuleTypes.Cannon) },

            { "hpt_cannon_gimbal_large", new ShipModule(128671120,8,0.75,"Ammo:100/5, Damage:37.4, Range:4000m, Speed:800m/s, Reload:4s, ThermL:2.9","Cannon Gimbal Large",ShipModule.ModuleTypes.Cannon) },    // EDDI

            // Frag cannon

            { "hpt_slugshot_fixed_small", new ShipModule(128049448,2,0.45,"Ammo:180/3, Damage:1.4, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.4","Fragment Cannon Fixed Small",ShipModule.ModuleTypes.FragmentCannon) },    // EDDI
            { "hpt_slugshot_fixed_medium", new ShipModule(128049449,4,0.74,"Ammo:180/3, Damage:3, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.7","Fragment Cannon Fixed Medium",ShipModule.ModuleTypes.FragmentCannon) },
            { "hpt_slugshot_fixed_large", new ShipModule(128049450,8,1.02,"Ammo:180/3, Damage:4.6, Range:2000m, Speed:667m/s, Reload:5s, ThermL:1.1","Fragment Cannon Fixed Large",ShipModule.ModuleTypes.FragmentCannon) },
            { "hpt_slugshot_gimbal_small", new ShipModule(128049451,2,0.59,"Ammo:180/3, Damage:1, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.4","Fragment Cannon Gimbal Small",ShipModule.ModuleTypes.FragmentCannon) },
            { "hpt_slugshot_gimbal_medium", new ShipModule(128049452,4,1.03,"Ammo:180/3, Damage:2.3, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.8","Fragment Cannon Gimbal Medium",ShipModule.ModuleTypes.FragmentCannon) },
            { "hpt_slugshot_turret_small", new ShipModule(128049453,2,0.42,"Ammo:180/3, Damage:0.7, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.2","Fragment Cannon Turret Small",ShipModule.ModuleTypes.FragmentCannon) },
            { "hpt_slugshot_turret_medium", new ShipModule(128049454,4,0.79,"Ammo:180/3, Damage:1.7, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.4","Fragment Cannon Turret Medium",ShipModule.ModuleTypes.FragmentCannon) },

            { "hpt_slugshot_gimbal_large", new ShipModule(128671321,8,1.55,"Ammo:180/3, Damage:3.8, Range:2000m, Speed:667m/s, Reload:5s, ThermL:1.4","Fragment Cannon Gimbal Large",ShipModule.ModuleTypes.FragmentCannon) },  // EDDI
            { "hpt_slugshot_turret_large", new ShipModule(128671322,8,1.29,"Ammo:180/3, Damage:3, Range:2000m, Speed:667m/s, Reload:5s, ThermL:0.7","Fragment Cannon Turret Large",ShipModule.ModuleTypes.FragmentCannon) },

            { "hpt_slugshot_fixed_large_range", new ShipModule(128671343,8,1.02,"Ammo:180/3, Damage:4, Speed:1000m/s, Reload:5s, ThermL:1.1","Fragment Cannon Fixed Large Range",ShipModule.ModuleTypes.PacifierFrag_Cannon) }, // EDDI

            // Cargo racks

            { "int_cargorack_size1_class1", new ShipModule(128064338,0,0,"Size:2t","Cargo Rack Class 1 Rating E",ShipModule.ModuleTypes.CargoRack) },   // EDDI
            { "int_cargorack_size2_class1", new ShipModule(128064339,0,0,"Size:4t","Cargo Rack Class 2 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size3_class1", new ShipModule(128064340,0,0,"Size:8t","Cargo Rack Class 3 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size4_class1", new ShipModule(128064341,0,0,"Size:16t","Cargo Rack Class 4 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size5_class1", new ShipModule(128064342,0,0,"Size:32t","Cargo Rack Class 5 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size6_class1", new ShipModule(128064343,0,0,"Size:64t","Cargo Rack Class 6 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size7_class1", new ShipModule(128064344,0,0,"Size:128t","Cargo Rack Class 7 Rating E",ShipModule.ModuleTypes.CargoRack) },
            { "int_cargorack_size8_class1", new ShipModule(128064345,0,0,"Size:256t","Cargo Rack Class 8 Rating E",ShipModule.ModuleTypes.CargoRack) },

            { "int_cargorack_size2_class1_free", new ShipModule(128666643,0,0,"Size:4t","Cargo Rack Class 2 Rating E",ShipModule.ModuleTypes.CargoRack) },  // EDDI

            { "int_corrosionproofcargorack_size1_class1", new ShipModule(128681641,0,0,"Size:1t","Corrosion Proof Cargo Rack Class 1 Rating E",ShipModule.ModuleTypes.CorrosionResistantCargoRack) },   // EDDI
            { "int_corrosionproofcargorack_size1_class2", new ShipModule(128681992,0,0,"Size:2t","Corrosion Proof Cargo Rack Class 1 Rating F",ShipModule.ModuleTypes.CorrosionResistantCargoRack) },

            { "int_corrosionproofcargorack_size4_class1", new ShipModule(128833944,0,0,"Size:16t","Corrosion Proof Cargo Rack Class 4 Rating E",ShipModule.ModuleTypes.CorrosionResistantCargoRack) },  // EDDI
            { "int_corrosionproofcargorack_size5_class1", new ShipModule(128833944,0,0,"Size:32t","Corrosion Proof Cargo Rack Class 5 Rating E",ShipModule.ModuleTypes.CorrosionResistantCargoRack) },  // EDDI

            // Cargo scanner

            { "hpt_cargoscanner_size0_class1", new ShipModule(128662520,1.3,0.2,"Range:2000m","Cargo Scanner Rating E",ShipModule.ModuleTypes.CargoScanner) },  // EDDI
            { "hpt_cargoscanner_size0_class2", new ShipModule(128662521,1.3,0.4,"Range:2500m","Cargo Scanner Rating D",ShipModule.ModuleTypes.CargoScanner) },
            { "hpt_cargoscanner_size0_class3", new ShipModule(128662522,1.3,0.8,"Range:3000m","Cargo Scanner Rating C",ShipModule.ModuleTypes.CargoScanner) },
            { "hpt_cargoscanner_size0_class4", new ShipModule(128662523,1.3,1.6,"Range:3500m","Cargo Scanner Rating B",ShipModule.ModuleTypes.CargoScanner) },
            { "hpt_cargoscanner_size0_class5", new ShipModule(128662524,1.3,3.2,"Range:4000m","Cargo Scanner Rating A",ShipModule.ModuleTypes.CargoScanner) },

            // Chaff, ECM

            { "hpt_chafflauncher_tiny", new ShipModule(128049513,1.3,0.2,"Ammo:10/1, Reload:10s, ThermL:4","Chaff Launcher",ShipModule.ModuleTypes.ChaffLauncher) },    // EDDI
            { "hpt_electroniccountermeasure_tiny", new ShipModule(128049516,1.3,0.2,"Range:3000m, Reload:10s, ThermL:4","Electronic Countermeasure", ShipModule.ModuleTypes.ElectronicCountermeasure) },
            { "hpt_heatsinklauncher_turret_tiny", new ShipModule(128049519,1.3,0.2,"Ammo:2/1, Reload:10s","Heat Sink Launcher Turret",ShipModule.ModuleTypes.HeatSinkLauncher) },
            { "hpt_causticsinklauncher_turret_tiny", new ShipModule(129019262,1.3,0.2,"Ammo:2/1, Reload:10s","Caustic Heat Sink Launcher Turret",ShipModule.ModuleTypes.CausticSinkLauncher) },
            { "hpt_plasmapointdefence_turret_tiny", new ShipModule(128049522,0.5,0.2,"Ammo:10000/12, Damage:0.2, Range:2500m, Speed:1000m/s, Reload:0.4s, ThermL:0.1","Plasma Point Defence Turret",ShipModule.ModuleTypes.PointDefence) },

            // kill warrant

            { "hpt_crimescanner_size0_class1", new ShipModule(128662530,1.3,0.2,"Range:2000m","Crime Scanner Rating E",ShipModule.ModuleTypes.KillWarrantScanner) }, // EDDI
            { "hpt_crimescanner_size0_class2", new ShipModule(128662531,1.3,0.4,"Range:2500m","Crime Scanner Rating D",ShipModule.ModuleTypes.KillWarrantScanner) },
            { "hpt_crimescanner_size0_class3", new ShipModule(128662532,1.3,0.8,"Range:3000m","Crime Scanner Rating C",ShipModule.ModuleTypes.KillWarrantScanner) },
            { "hpt_crimescanner_size0_class4", new ShipModule(128662533,1.3,1.6,"Range:3500m","Crime Scanner Rating B",ShipModule.ModuleTypes.KillWarrantScanner) },
            { "hpt_crimescanner_size0_class5", new ShipModule(128662534,1.3,3.2,"Range:4000m","Crime Scanner Rating A",ShipModule.ModuleTypes.KillWarrantScanner) },

            // surface scanner

            { "int_detailedsurfacescanner_tiny", new ShipModule(128666634,0,0,null,"Detailed Surface Scanner",ShipModule.ModuleTypes.DetailedSurfaceScanner) }, // EDDI

            // docking computer

            { "int_dockingcomputer_standard", new ShipModule(128049549,0,0.39,null,"Docking Computer Standard",ShipModule.ModuleTypes.StandardDockingComputer) },       // EDDI
            { "int_dockingcomputer_advanced", new ShipModule(128935155,0,0.45,null,"Docking Computer Advanced",ShipModule.ModuleTypes.AdvancedDockingComputer) },

            // figther bays

            { "int_fighterbay_size5_class1", new ShipModule(128727930,20,0.25,"Rebuilds:6t","Fighter Hangar Class 5 Rating E",ShipModule.ModuleTypes.FighterHangar) },  // EDDI
            { "int_fighterbay_size6_class1", new ShipModule(128727931,40,0.35,"Rebuilds:8t","Fighter Hangar Class 6 Rating E",ShipModule.ModuleTypes.FighterHangar) },
            { "int_fighterbay_size7_class1", new ShipModule(128727932,60,0.35,"Rebuilds:15t","Fighter Hangar Class 7 Rating E",ShipModule.ModuleTypes.FighterHangar) },

            // flak

            { "hpt_flakmortar_fixed_medium", new ShipModule(128785626,4,1.2,"Ammo:32/1, Damage:34, Speed:550m/s, Reload:2s, ThermL:3.6","Flak Mortar Fixed Medium",ShipModule.ModuleTypes.RemoteReleaseFlakLauncher) },     // EDDI
            { "hpt_flakmortar_turret_medium", new ShipModule(128793058,4,1.2,"Ammo:32/1, Damage:34, Speed:550m/s, Reload:2s, ThermL:3.6","Flak Mortar Turret Medium",ShipModule.ModuleTypes.RemoteReleaseFlakLauncher) },

            // flechette

            { "hpt_flechettelauncher_fixed_medium", new ShipModule(128833996,4,1.2,"Ammo:72/1, Damage:13, Speed:550m/s, Reload:2s, ThermL:3.6","Flechette Launcher Fixed Medium",ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher) },  // EDDI
            { "hpt_flechettelauncher_turret_medium", new ShipModule(128833997,4,1.2,"Ammo:72/1, Damage:13, Speed:550m/s, Reload:2s, ThermL:3.6","Flechette Launcher Turret Medium",ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher) },

            // fsd interdictor

            { "int_fsdinterdictor_size1_class1", new ShipModule(128666704,1.3,0.14,null,"FSD Interdictor Class 1 Rating E",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },    // EDDI
            { "int_fsdinterdictor_size2_class1", new ShipModule(128666705,2.5,0.17,null,"FSD Interdictor Class 2 Rating E",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size3_class1", new ShipModule(128666706,5,0.2,null,"FSD Interdictor Class 3 Rating E",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size4_class1", new ShipModule(128666707,10,0.25,null,"FSD Interdictor Class 4 Rating E",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size1_class2", new ShipModule(128666708,0.5,0.18,null,"FSD Interdictor Class 1 Rating D",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size2_class2", new ShipModule(128666709,1,0.22,null,"FSD Interdictor Class 2 Rating D",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size3_class2", new ShipModule(128666710,2,0.27,null,"FSD Interdictor Class 3 Rating D",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size4_class2", new ShipModule(128666711,4,0.33,null,"FSD Interdictor Class 4 Rating D",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size1_class3", new ShipModule(128666712,1.3,0.23,null,"FSD Interdictor Class 1 Rating C",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size2_class3", new ShipModule(128666713,2.5,0.28,null,"FSD Interdictor Class 2 Rating C",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size3_class3", new ShipModule(128666714,5,0.34,null,"FSD Interdictor Class 3 Rating C",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size4_class3", new ShipModule(128666715,10,0.41,null,"FSD Interdictor Class 4 Rating C",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size1_class4", new ShipModule(128666716,2,0.28,null,"FSD Interdictor Class 1 Rating B",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size2_class4", new ShipModule(128666717,4,0.34,null,"FSD Interdictor Class 2 Rating B",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size3_class4", new ShipModule(128666718,8,0.41,null,"FSD Interdictor Class 3 Rating B",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size4_class4", new ShipModule(128666719,16,0.49,null,"FSD Interdictor Class 4 Rating B",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size1_class5", new ShipModule(128666720,1.3,0.32,null,"FSD Interdictor Class 1 Rating A",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size2_class5", new ShipModule(128666721,2.5,0.39,null,"FSD Interdictor Class 2 Rating A",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size3_class5", new ShipModule(128666722,5,0.48,null,"FSD Interdictor Class 3 Rating A",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },
            { "int_fsdinterdictor_size4_class5", new ShipModule(128666723,10,0.57,null,"FSD Interdictor Class 4 Rating A",ShipModule.ModuleTypes.FrameShiftDriveInterdictor) },

            // Fuel scoop

            { "int_fuelscoop_size1_class1", new ShipModule(128666644,0,0.14,"Rate:18","Fuel Scoop Class 1 Rating E",ShipModule.ModuleTypes.FuelScoop) },        // EDDI
            { "int_fuelscoop_size2_class1", new ShipModule(128666645,0,0.17,"Rate:32","Fuel Scoop Class 2 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size3_class1", new ShipModule(128666646,0,0.2,"Rate:75","Fuel Scoop Class 3 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size4_class1", new ShipModule(128666647,0,0.25,"Rate:147","Fuel Scoop Class 4 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size5_class1", new ShipModule(128666648,0,0.3,"Rate:247","Fuel Scoop Class 5 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size6_class1", new ShipModule(128666649,0,0.35,"Rate:376","Fuel Scoop Class 6 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size7_class1", new ShipModule(128666650,0,0.41,"Rate:534","Fuel Scoop Class 7 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size8_class1", new ShipModule(128666651,0,0.48,"Rate:720","Fuel Scoop Class 8 Rating E",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size1_class2", new ShipModule(128666652,0,0.18,"Rate:24","Fuel Scoop Class 1 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size2_class2", new ShipModule(128666653,0,0.22,"Rate:43","Fuel Scoop Class 2 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size3_class2", new ShipModule(128666654,0,0.27,"Rate:100","Fuel Scoop Class 3 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size4_class2", new ShipModule(128666655,0,0.33,"Rate:196","Fuel Scoop Class 4 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size5_class2", new ShipModule(128666656,0,0.4,"Rate:330","Fuel Scoop Class 5 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size6_class2", new ShipModule(128666657,0,0.47,"Rate:502","Fuel Scoop Class 6 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size7_class2", new ShipModule(128666658,0,0.55,"Rate:712","Fuel Scoop Class 7 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size8_class2", new ShipModule(128666659,0,0.64,"Rate:960","Fuel Scoop Class 8 Rating D",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size1_class3", new ShipModule(128666660,0,0.23,"Rate:30","Fuel Scoop Class 1 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size2_class3", new ShipModule(128666661,0,0.28,"Rate:54","Fuel Scoop Class 2 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size3_class3", new ShipModule(128666662,0,0.34,"Rate:126","Fuel Scoop Class 3 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size4_class3", new ShipModule(128666663,0,0.41,"Rate:245","Fuel Scoop Class 4 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size5_class3", new ShipModule(128666664,0,0.5,"Rate:412","Fuel Scoop Class 5 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size6_class3", new ShipModule(128666665,0,0.59,"Rate:627","Fuel Scoop Class 6 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size7_class3", new ShipModule(128666666,0,0.69,"Rate:890","Fuel Scoop Class 7 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size8_class3", new ShipModule(128666667,0,0.8,"Rate:1200","Fuel Scoop Class 8 Rating C",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size1_class4", new ShipModule(128666668,0,0.28,"Rate:36","Fuel Scoop Class 1 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size2_class4", new ShipModule(128666669,0,0.34,"Rate:65","Fuel Scoop Class 2 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size3_class4", new ShipModule(128666670,0,0.41,"Rate:151","Fuel Scoop Class 3 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size4_class4", new ShipModule(128666671,0,0.49,"Rate:294","Fuel Scoop Class 4 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size5_class4", new ShipModule(128666672,0,0.6,"Rate:494","Fuel Scoop Class 5 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size6_class4", new ShipModule(128666673,0,0.71,"Rate:752","Fuel Scoop Class 6 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size6_class5", new ShipModule(128666681,0,0.83,"Rate:878","Fuel Scoop Class 6 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size7_class4", new ShipModule(128666674,0,0.83,"Rate:1068","Fuel Scoop Class 7 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size8_class4", new ShipModule(128666675,0,0.96,"Rate:1440","Fuel Scoop Class 8 Rating B",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size1_class5", new ShipModule(128666676,0,0.32,"Rate:42","Fuel Scoop Class 1 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size2_class5", new ShipModule(128666677,0,0.39,"Rate:75","Fuel Scoop Class 2 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size3_class5", new ShipModule(128666678,0,0.48,"Rate:176","Fuel Scoop Class 3 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size4_class5", new ShipModule(128666679,0,0.57,"Rate:342","Fuel Scoop Class 4 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size5_class5", new ShipModule(128666680,0,0.7,"Rate:577","Fuel Scoop Class 5 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size7_class5", new ShipModule(128666682,0,0.97,"Rate:1245","Fuel Scoop Class 7 Rating A",ShipModule.ModuleTypes.FuelScoop) },
            { "int_fuelscoop_size8_class5", new ShipModule(128666683,0,1.12,"Rate:1680","Fuel Scoop Class 8 Rating A",ShipModule.ModuleTypes.FuelScoop) },

            // fuel tank

            { "int_fueltank_size1_class3", new ShipModule(128064346,0,0,"Size:2t","Fuel Tank Class 1 Rating C",ShipModule.ModuleTypes.FuelTank) },      // EDDI
            { "int_fueltank_size2_class3", new ShipModule(128064347,0,0,"Size:4t","Fuel Tank Class 2 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size3_class3", new ShipModule(128064348,0,0,"Size:8t","Fuel Tank Class 3 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size4_class3", new ShipModule(128064349,0,0,"Size:16t","Fuel Tank Class 4 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size5_class3", new ShipModule(128064350,0,0,"Size:32t","Fuel Tank Class 5 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size6_class3", new ShipModule(128064351,0,0,"Size:64t","Fuel Tank Class 6 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size7_class3", new ShipModule(128064352,0,0,"Size:128t","Fuel Tank Class 7 Rating C",ShipModule.ModuleTypes.FuelTank) },
            { "int_fueltank_size8_class3", new ShipModule(128064353,0,0,"Size:256t","Fuel Tank Class 8 Rating C",ShipModule.ModuleTypes.FuelTank) },

            { "int_fueltank_size1_class3_free", new ShipModule(128667018,0,0,"Size:2t","Fuel Tank Class 1 Rating C",ShipModule.ModuleTypes.FuelTank) }, // EDDI

            // Gardian

            { "hpt_guardian_plasmalauncher_turret_small", new ShipModule(128891606,2,1.6,"Ammo:200/15, Damage:1.1, Range:3000m, Speed:1200m/s, Reload:3s, ThermL:5","Guardian Plasma Launcher Turret Small",ShipModule.ModuleTypes.GuardianPlasmaCharger) },    // EDDI
            { "hpt_guardian_plasmalauncher_fixed_small", new ShipModule(128891607,2,1.4,"Ammo:200/15, Damage:1.7, Range:3000m, Speed:1200m/s, Reload:3s, ThermL:4.2","Guardian Plasma Launcher Fixed Small",ShipModule.ModuleTypes.GuardianPlasmaCharger) },
            { "hpt_guardian_shardcannon_turret_small", new ShipModule(128891608,2,0.72,"Ammo:180/5, Damage:1.1, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:0.6","Guardian Shard Cannon Turret Small",ShipModule.ModuleTypes.GuardianShardCannon) },
            { "hpt_guardian_shardcannon_fixed_small", new ShipModule(128891609,2,0.87,"Ammo:180/5, Damage:2, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:0.7","Guardian Shard Cannon Fixed Small",ShipModule.ModuleTypes.GuardianShardCannon) },
            { "hpt_guardian_gausscannon_fixed_small", new ShipModule(128891610,2,1.91,"Ammo:80/1, Damage:22, Range:3000m, Reload:1s, ThermL:15","Guardian Gauss Cannon Fixed Small",ShipModule.ModuleTypes.GuardianGaussCannon) },

            { "hpt_guardian_gausscannon_fixed_medium", new ShipModule(128833687,4,2.61,"Ammo:80/1, Damage:38.5, Range:3000m, Reload:1s, ThermL:25","Guardian Gauss Cannon Fixed Medium",ShipModule.ModuleTypes.GuardianGaussCannon) }, // EDDI

            { "hpt_guardian_plasmalauncher_fixed_medium", new ShipModule(128833998,4,2.13,"Ammo:200/15, Damage:5, Range:3500m, Speed:1200m/s, Reload:3s, ThermL:5.2","Guardian Plasma Launcher Fixed Medium",ShipModule.ModuleTypes.GuardianPlasmaCharger) },   // EDDI
            { "hpt_guardian_plasmalauncher_turret_medium", new ShipModule(128833999,4,2.01,"Ammo:200/15, Damage:4, Range:3500m, Speed:1200m/s, Reload:3s, ThermL:5.8","Guardian Plasma Launcher Turret Medium",ShipModule.ModuleTypes.GuardianPlasmaCharger) },
            { "hpt_guardian_shardcannon_fixed_medium", new ShipModule(128834000,4,1.21,"Ammo:180/5, Damage:3.7, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:1.2","Guardian Shard Cannon Fixed Medium",ShipModule.ModuleTypes.GuardianShardCannon) },
            { "hpt_guardian_shardcannon_turret_medium", new ShipModule(128834001,4,1.16,"Ammo:180/5, Damage:2.4, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:1.1","Guardian Shard Cannon Turret Medium",ShipModule.ModuleTypes.GuardianShardCannon) },

            { "hpt_guardian_plasmalauncher_fixed_large", new ShipModule(128834783,8,3.1,"Ammo:200/15, Damage:3.4, Range:3000m, Speed:1200m/s, Reload:3s, ThermL:6.2","Guardian Plasma Launcher Fixed Large",ShipModule.ModuleTypes.GuardianPlasmaCharger) },    // EDDI
            { "hpt_guardian_plasmalauncher_turret_large", new ShipModule(128834784,8,2.53,"Ammo:200/15, Damage:3.3, Range:3000m, Speed:1200m/s, Reload:3s, ThermL:6.4","Guardian Plasma Launcher Turret Large",ShipModule.ModuleTypes.GuardianPlasmaCharger) },

            { "hpt_guardian_shardcannon_fixed_large", new ShipModule(128834778,8,1.68,"Ammo:180/5, Damage:5.2, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:2.2","Guardian Shard Cannon Fixed Large",ShipModule.ModuleTypes.GuardianShardCannon) }, // EDDI
            { "hpt_guardian_shardcannon_turret_large", new ShipModule(128834779,8,1.39,"Ammo:180/5, Damage:3.4, Range:1700m, Speed:1133m/s, Reload:5s, ThermL:2","Guardian Shard Cannon Turret Large",ShipModule.ModuleTypes.GuardianShardCannon) },

            { "int_guardianhullreinforcement_size1_class2", new ShipModule(128833946,1,0.56,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.GuardianHullReinforcement) },  // EDDI
            { "int_guardianhullreinforcement_size1_class1", new ShipModule(128833945,2,0.45,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size2_class1", new ShipModule(128833947,4,0.68,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size2_class2", new ShipModule(128833948,2,0.79,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size3_class1", new ShipModule(128833949,8,0.9,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size3_class2", new ShipModule(128833950,4,1.01,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size4_class1", new ShipModule(128833951,16,1.13,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size4_class2", new ShipModule(128833952,8,1.24,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size5_class1", new ShipModule(128833953,32,1.35,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.GuardianHullReinforcement) },
            { "int_guardianhullreinforcement_size5_class2", new ShipModule(128833954,16,1.46,"Explosive:0%, Kinetic:0%, Thermal:2%","Guardian Hull Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.GuardianHullReinforcement) },

            { "int_guardianmodulereinforcement_size1_class1", new ShipModule(128833955,2,0.27,"Protection:0.3","Guardian Module Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.GuardianModuleReinforcement) },  // EDDI
            { "int_guardianmodulereinforcement_size1_class2", new ShipModule(128833956,1,0.34,"Protection:0.6","Guardian Module Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size2_class1", new ShipModule(128833957,4,0.41,"Protection:0.3","Guardian Module Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size2_class2", new ShipModule(128833958,2,0.47,"Protection:0.6","Guardian Module Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size3_class1", new ShipModule(128833959,8,0.54,"Protection:0.3","Guardian Module Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size3_class2", new ShipModule(128833960,4,0.61,"Protection:0.6","Guardian Module Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size4_class1", new ShipModule(128833961,16,0.68,"Protection:0.3","Guardian Module Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size4_class2", new ShipModule(128833962,8,0.74,"Protection:0.6","Guardian Module Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size5_class1", new ShipModule(128833963,32,0.81,"Protection:0.3","Guardian Module Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.GuardianModuleReinforcement) },
            { "int_guardianmodulereinforcement_size5_class2", new ShipModule(128833964,16,0.88,"Protection:0.6","Guardian Module Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.GuardianModuleReinforcement) },

            { "int_guardianshieldreinforcement_size1_class1", new ShipModule(128833965,2,0.35,null,"Guardian Shield Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.GuardianShieldReinforcement) },// EDDI
            { "int_guardianshieldreinforcement_size1_class2", new ShipModule(128833966,1,0.46,null,"Guardian Shield Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size2_class1", new ShipModule(128833967,4,0.56,null,"Guardian Shield Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size2_class2", new ShipModule(128833968,2,0.67,null,"Guardian Shield Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size3_class1", new ShipModule(128833969,8,0.74,null,"Guardian Shield Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size3_class2", new ShipModule(128833970,4,0.84,null,"Guardian Shield Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size4_class1", new ShipModule(128833971,16,0.95,null,"Guardian Shield Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size4_class2", new ShipModule(128833972,8,1.05,null,"Guardian Shield Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size5_class1", new ShipModule(128833973,32,1.16,null,"Guardian Shield Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.GuardianShieldReinforcement) },
            { "int_guardianshieldreinforcement_size5_class2", new ShipModule(128833974,16,1.26,null,"Guardian Shield Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.GuardianShieldReinforcement) },

            { "int_guardianfsdbooster_size1", new ShipModule(128833975,1.3,0.75,null,"Guardian FSD Booster Class 1",ShipModule.ModuleTypes.GuardianFSDBooster) },    // EDDI
            { "int_guardianfsdbooster_size2", new ShipModule(128833976,1.3,0.98,null,"Guardian FSD Booster Class 2",ShipModule.ModuleTypes.GuardianFSDBooster) },
            { "int_guardianfsdbooster_size3", new ShipModule(128833977,1.3,1.27,null,"Guardian FSD Booster Class 3",ShipModule.ModuleTypes.GuardianFSDBooster) },
            { "int_guardianfsdbooster_size4", new ShipModule(128833978,1.3,1.65,null,"Guardian FSD Booster Class 4",ShipModule.ModuleTypes.GuardianFSDBooster) },
            { "int_guardianfsdbooster_size5", new ShipModule(128833979,1.3,2.14,null,"Guardian FSD Booster Class 5",ShipModule.ModuleTypes.GuardianFSDBooster) },

            { "int_guardianpowerdistributor_size1", new ShipModule(128833980,1.4,0.62,"Sys:0.8MW, Eng:0.8MW, Wep:2.5MW","Guardian Power Distributor Class 1",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },  // EDDI
            { "int_guardianpowerdistributor_size2", new ShipModule(128833981,2.6,0.73,"Sys:0.8MW, Eng:0.8MW, Wep:2.5MW","Guardian Power Distributor Class 2",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size3", new ShipModule(128833982,5.25,0.78,"Sys:1.7MW, Eng:1.7MW, Wep:3.1MW","Guardian Power Distributor Class 3",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size4", new ShipModule(128833983,10.5,0.87,"Sys:1.7MW, Eng:2.5MW, Wep:4.9MW","Guardian Power Distributor Class 4",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size5", new ShipModule(128833984,21,0.96,"Sys:3.3MW, Eng:3.3MW, Wep:6MW","Guardian Power Distributor Class 5",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size6", new ShipModule(128833985,42,1.07,"Sys:4.2MW, Eng:4.2MW, Wep:7.3MW","Guardian Power Distributor Class 6",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size7", new ShipModule(128833986,84,1.16,"Sys:5.2MW, Eng:5.2MW, Wep:8.5MW","Guardian Power Distributor Class 7",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },
            { "int_guardianpowerdistributor_size8", new ShipModule(128833987,168,1.25,"Sys:6.2MW, Eng:6.2MW, Wep:10.1MW","Guardian Power Distributor Class 8",ShipModule.ModuleTypes.GuardianHybridPowerDistributor) },

            { "int_guardianpowerplant_size2", new ShipModule(128833988,1.5,0,"Power:12.7MW","Guardian Powerplant Class 2",ShipModule.ModuleTypes.GuardianHybridPowerPlant) }, // EDDI
            { "int_guardianpowerplant_size3", new ShipModule(128833989,2.9,0,"Power:15.8MW","Guardian Powerplant Class 3",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },
            { "int_guardianpowerplant_size4", new ShipModule(128833990,5.9,0,"Power:20.6MW","Guardian Powerplant Class 4",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },
            { "int_guardianpowerplant_size5", new ShipModule(128833991,11.7,0,"Power:26.9MW","Guardian Powerplant Class 5",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },
            { "int_guardianpowerplant_size6", new ShipModule(128833992,23.4,0,"Power:33.3MW","Guardian Powerplant Class 6",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },
            { "int_guardianpowerplant_size7", new ShipModule(128833993,46.8,0,"Power:39.6MW","Guardian Powerplant Class 7",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },
            { "int_guardianpowerplant_size8", new ShipModule(128833994,93.6,0,"Power:47.5MW","Guardian Powerplant Class 8",ShipModule.ModuleTypes.GuardianHybridPowerPlant) },

            // hull reinforcements 

            { "int_hullreinforcement_size1_class1", new ShipModule(128668537,2,0,"Explosive:0.5%, Kinetic:0.5%, Thermal:0.5%","Hull Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.HullReinforcementPackage) }, // EDDI
            { "int_hullreinforcement_size1_class2", new ShipModule(128668538,1,0,"Explosive:0.5%, Kinetic:0.5%, Thermal:0.5%","Hull Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size2_class1", new ShipModule(128668539,4,0,"Explosive:1%, Kinetic:1%, Thermal:1%","Hull Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size2_class2", new ShipModule(128668540,2,0,"Explosive:1%, Kinetic:1%, Thermal:1%","Hull Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size3_class1", new ShipModule(128668541,8,0,"Explosive:1.5%, Kinetic:1.5%, Thermal:1.5%","Hull Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size3_class2", new ShipModule(128668542,4,0,"Explosive:1.5%, Kinetic:1.5%, Thermal:1.5%","Hull Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size4_class1", new ShipModule(128668543,16,0,"Explosive:2%, Kinetic:2%, Thermal:2%","Hull Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size4_class2", new ShipModule(128668544,8,0,"Explosive:2%, Kinetic:2%, Thermal:2%","Hull Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size5_class1", new ShipModule(128668545,32,0,"Explosive:2.5%, Kinetic:2.5%, Thermal:2.5%","Hull Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.HullReinforcementPackage) },
            { "int_hullreinforcement_size5_class2", new ShipModule(128668546,16,0,"Explosive:2.5%, Kinetic:2.5%, Thermal:2.5%","Hull Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.HullReinforcementPackage) },

            // Frame ship drive

            { "int_hyperdrive_size2_class1", new ShipModule(128064103,2.5,0.16,"OptMass:48t","Hyperdrive Class 2 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },   // EDDI
            { "int_hyperdrive_size2_class2", new ShipModule(128064104,1,0.18,"OptMass:54t","Hyperdrive Class 2 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size2_class3", new ShipModule(128064105,2.5,0.2,"OptMass:60t","Hyperdrive Class 2 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size2_class4", new ShipModule(128064106,4,0.25,"OptMass:75t","Hyperdrive Class 2 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size2_class5", new ShipModule(128064107,2.5,0.3,"OptMass:90t","Hyperdrive Class 2 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size3_class1", new ShipModule(128064108,5,0.24,"OptMass:80t","Hyperdrive Class 3 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size3_class2", new ShipModule(128064109,2,0.27,"OptMass:90t","Hyperdrive Class 3 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size3_class3", new ShipModule(128064110,5,0.3,"OptMass:100t","Hyperdrive Class 3 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size3_class4", new ShipModule(128064111,8,0.38,"OptMass:125t","Hyperdrive Class 3 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size3_class5", new ShipModule(128064112,5,0.45,"OptMass:150t","Hyperdrive Class 3 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size4_class1", new ShipModule(128064113,10,0.24,"OptMass:280t","Hyperdrive Class 4 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size4_class2", new ShipModule(128064114,4,0.27,"OptMass:315t","Hyperdrive Class 4 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size4_class3", new ShipModule(128064115,10,0.3,"OptMass:350t","Hyperdrive Class 4 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size4_class4", new ShipModule(128064116,16,0.38,"OptMass:438t","Hyperdrive Class 4 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size4_class5", new ShipModule(128064117,10,0.45,"OptMass:525t","Hyperdrive Class 4 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size5_class1", new ShipModule(128064118,20,0.32,"OptMass:560t","Hyperdrive Class 5 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size5_class2", new ShipModule(128064119,8,0.36,"OptMass:630t","Hyperdrive Class 5 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size5_class3", new ShipModule(128064120,20,0.4,"OptMass:700t","Hyperdrive Class 5 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size5_class4", new ShipModule(128064121,32,0.5,"OptMass:875t","Hyperdrive Class 5 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size5_class5", new ShipModule(128064122,20,0.6,"OptMass:1050t","Hyperdrive Class 5 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size6_class1", new ShipModule(128064123,40,0.4,"OptMass:960t","Hyperdrive Class 6 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size6_class2", new ShipModule(128064124,16,0.45,"OptMass:1080t","Hyperdrive Class 6 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size6_class3", new ShipModule(128064125,40,0.5,"OptMass:1200t","Hyperdrive Class 6 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size6_class4", new ShipModule(128064126,64,0.63,"OptMass:1500t","Hyperdrive Class 6 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size6_class5", new ShipModule(128064127,40,0.75,"OptMass:1800t","Hyperdrive Class 6 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size7_class1", new ShipModule(128064128,80,0.48,"OptMass:1440t","Hyperdrive Class 7 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size7_class2", new ShipModule(128064129,32,0.54,"OptMass:1620t","Hyperdrive Class 7 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size7_class3", new ShipModule(128064130,80,0.6,"OptMass:1800t","Hyperdrive Class 7 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size7_class4", new ShipModule(128064131,128,0.75,"OptMass:2250t","Hyperdrive Class 7 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size7_class5", new ShipModule(128064132,80,0.9,"OptMass:2700t","Hyperdrive Class 7 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size8_class1", new ShipModule(128064133,160,0.56,"OptMass:0t","Hyperdrive Class 8 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size8_class2", new ShipModule(128064134,64,0.63,"OptMass:0t","Hyperdrive Class 8 Rating D",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size8_class3", new ShipModule(128064135,160,0.7,"OptMass:0t","Hyperdrive Class 8 Rating C",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size8_class4", new ShipModule(128064136,256,0.88,"OptMass:0t","Hyperdrive Class 8 Rating B",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size8_class5", new ShipModule(128064137,160,1.05,"OptMass:0t","Hyperdrive Class 8 Rating A",ShipModule.ModuleTypes.FrameShiftDrive) },
            { "int_hyperdrive_size2_class1_free", new ShipModule(128666637,2.5,0.16,"OptMass:48t","Hyperdrive Class 2 Rating E",ShipModule.ModuleTypes.FrameShiftDrive) },

            // wake scanner

            { "hpt_cloudscanner_size0_class1", new ShipModule(128662525,1.3,0.2,"Range:2000m","Cloud Scanner Rating E",ShipModule.ModuleTypes.FrameShiftWakeScanner) },  // EDDI
            { "hpt_cloudscanner_size0_class2", new ShipModule(128662526,1.3,0.4,"Range:2500m","Cloud Scanner Rating D",ShipModule.ModuleTypes.FrameShiftWakeScanner) },
            { "hpt_cloudscanner_size0_class3", new ShipModule(128662527,1.3,0.8,"Range:3000m","Cloud Scanner Rating C",ShipModule.ModuleTypes.FrameShiftWakeScanner) },
            { "hpt_cloudscanner_size0_class4", new ShipModule(128662528,1.3,1.6,"Range:3500m","Cloud Scanner Rating B",ShipModule.ModuleTypes.FrameShiftWakeScanner) },
            { "hpt_cloudscanner_size0_class5", new ShipModule(128662529,1.3,3.2,"Range:4000m","Cloud Scanner Rating A",ShipModule.ModuleTypes.FrameShiftWakeScanner) },

            // life support

            { "int_lifesupport_size1_class1", new ShipModule(128064138,1.3,0.32,"Time:300s","Life Support Class 1 Rating E",ShipModule.ModuleTypes.LifeSupport) },  // EDDI
            { "int_lifesupport_size1_class2", new ShipModule(128064139,0.5,0.36,"Time:450s","Life Support Class 1 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size1_class3", new ShipModule(128064140,1.3,0.4,"Time:600s","Life Support Class 1 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size1_class4", new ShipModule(128064141,2,0.44,"Time:900s","Life Support Class 1 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size1_class5", new ShipModule(128064142,1.3,0.48,"Time:1500s","Life Support Class 1 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size2_class1", new ShipModule(128064143,2.5,0.37,"Time:300s","Life Support Class 2 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size2_class2", new ShipModule(128064144,1,0.41,"Time:450s","Life Support Class 2 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size2_class3", new ShipModule(128064145,2.5,0.46,"Time:600s","Life Support Class 2 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size2_class4", new ShipModule(128064146,4,0.51,"Time:900s","Life Support Class 2 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size2_class5", new ShipModule(128064147,2.5,0.55,"Time:1500s","Life Support Class 2 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size3_class1", new ShipModule(128064148,5,0.42,"Time:300s","Life Support Class 3 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size3_class2", new ShipModule(128064149,2,0.48,"Time:450s","Life Support Class 3 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size3_class3", new ShipModule(128064150,5,0.53,"Time:600s","Life Support Class 3 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size3_class4", new ShipModule(128064151,8,0.58,"Time:900s","Life Support Class 3 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size3_class5", new ShipModule(128064152,5,0.64,"Time:1500s","Life Support Class 3 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size4_class1", new ShipModule(128064153,10,0.5,"Time:300s","Life Support Class 4 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size4_class2", new ShipModule(128064154,4,0.56,"Time:450s","Life Support Class 4 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size4_class3", new ShipModule(128064155,10,0.62,"Time:600s","Life Support Class 4 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size4_class4", new ShipModule(128064156,16,0.68,"Time:900s","Life Support Class 4 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size4_class5", new ShipModule(128064157,10,0.74,"Time:1500s","Life Support Class 4 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size5_class1", new ShipModule(128064158,20,0.57,"Time:300s","Life Support Class 5 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size5_class2", new ShipModule(128064159,8,0.64,"Time:450s","Life Support Class 5 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size5_class3", new ShipModule(128064160,20,0.71,"Time:600s","Life Support Class 5 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size5_class4", new ShipModule(128064161,32,0.78,"Time:900s","Life Support Class 5 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size5_class5", new ShipModule(128064162,20,0.85,"Time:1500s","Life Support Class 5 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size6_class1", new ShipModule(128064163,40,0.64,"Time:300s","Life Support Class 6 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size6_class2", new ShipModule(128064164,16,0.72,"Time:450s","Life Support Class 6 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size6_class3", new ShipModule(128064165,40,0.8,"Time:600s","Life Support Class 6 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size6_class4", new ShipModule(128064166,64,0.88,"Time:900s","Life Support Class 6 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size6_class5", new ShipModule(128064167,40,0.96,"Time:1500s","Life Support Class 6 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size7_class1", new ShipModule(128064168,80,0.72,"Time:300s","Life Support Class 7 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size7_class2", new ShipModule(128064169,32,0.81,"Time:450s","Life Support Class 7 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size7_class3", new ShipModule(128064170,80,0.9,"Time:600s","Life Support Class 7 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size7_class4", new ShipModule(128064171,128,0.99,"Time:900s","Life Support Class 7 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size7_class5", new ShipModule(128064172,80,1.08,"Time:1500s","Life Support Class 7 Rating A",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size8_class1", new ShipModule(128064173,160,0.8,"Time:300s","Life Support Class 8 Rating E",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size8_class2", new ShipModule(128064174,64,0.9,"Time:450s","Life Support Class 8 Rating D",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size8_class3", new ShipModule(128064175,160,1,"Time:600s","Life Support Class 8 Rating C",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size8_class4", new ShipModule(128064176,256,1.1,"Time:900s","Life Support Class 8 Rating B",ShipModule.ModuleTypes.LifeSupport) },
            { "int_lifesupport_size8_class5", new ShipModule(128064177,160,1.2,"Time:1500s","Life Support Class 8 Rating A",ShipModule.ModuleTypes.LifeSupport) },

            { "int_lifesupport_size1_class1_free", new ShipModule(128666638,1.3,0.32,"Time:300s","Life Support Class 1 Rating E",ShipModule.ModuleTypes.LifeSupport) }, //EDDI

            // Limpet control

            { "int_dronecontrol_collection_size1_class1", new ShipModule(128671229,0.5,0.14,"Time:300s, Range:0.8km","Collection Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.CollectorLimpetController) },    // EDDI
            { "int_dronecontrol_collection_size1_class2", new ShipModule(128671230,0.5,0.18,"Time:600s, Range:0.6km","Collection Drone Controller Class 1 Rating D",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size1_class3", new ShipModule(128671231,1.3,0.23,"Time:510s, Range:1km","Collection Drone Controller Class 1 Rating C",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size1_class4", new ShipModule(128671232,2,0.28,"Time:420s, Range:1.4km","Collection Drone Controller Class 1 Rating B",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size1_class5", new ShipModule(128671233,2,0.32,"Time:720s, Range:1.2km","Collection Drone Controller Class 1 Rating A",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size3_class1", new ShipModule(128671234,2,0.2,"Time:300s, Range:0.9km","Collection Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size3_class2", new ShipModule(128671235,2,0.27,"Time:600s, Range:0.7km","Collection Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size3_class3", new ShipModule(128671236,5,0.34,"Time:510s, Range:1.1km","Collection Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size3_class4", new ShipModule(128671237,8,0.41,"Time:420s, Range:1.5km","Collection Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size3_class5", new ShipModule(128671238,8,0.48,"Time:720s, Range:1.3km","Collection Drone Controller Class 3 Rating A",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size5_class1", new ShipModule(128671239,8,0.3,"Time:300s, Range:1km","Collection Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size5_class2", new ShipModule(128671240,8,0.4,"Time:600s, Range:0.8km","Collection Drone Controller Class 5 Rating D",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size5_class3", new ShipModule(128671241,20,0.5,"Time:510s, Range:1.3km","Collection Drone Controller Class 5 Rating C",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size5_class4", new ShipModule(128671242,32,0.6,"Time:420s, Range:1.8km","Collection Drone Controller Class 5 Rating B",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size5_class5", new ShipModule(128671243,32,0.7,"Time:720s, Range:1.6km","Collection Drone Controller Class 5 Rating A",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size7_class1", new ShipModule(128671244,32,0.41,"Time:300s, Range:1.4km","Collection Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size7_class2", new ShipModule(128671245,32,0.55,"Time:600s, Range:1km","Collection Drone Controller Class 7 Rating D",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size7_class3", new ShipModule(128671246,80,0.69,"Time:510s, Range:1.7km","Collection Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size7_class4", new ShipModule(128671247,128,0.83,"Time:420s, Range:2.4km","Collection Drone Controller Class 7 Rating B",ShipModule.ModuleTypes.CollectorLimpetController) },
            { "int_dronecontrol_collection_size7_class5", new ShipModule(128671248,128,0.97,"Time:720s, Range:2km","Collection Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.CollectorLimpetController) },

            { "int_dronecontrol_fueltransfer_size1_class1", new ShipModule(128671249,1.3,0.18,"Range:0.6km","Fuel Transfer Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.FuelTransferLimpetController) },   // EDDI
            { "int_dronecontrol_fueltransfer_size1_class2", new ShipModule(128671250,0.5,0.14,"Range:0.8km","Fuel Transfer Drone Controller Class 1 Rating D",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size1_class3", new ShipModule(128671251,1.3,0.23,"Range:1km","Fuel Transfer Drone Controller Class 1 Rating C",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size1_class4", new ShipModule(128671252,2,0.32,"Range:1.2km","Fuel Transfer Drone Controller Class 1 Rating B",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size1_class5", new ShipModule(128671253,1.3,0.28,"Range:1.4km","Fuel Transfer Drone Controller Class 1 Rating A",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size3_class1", new ShipModule(128671254,5,0.27,"Range:0.7km","Fuel Transfer Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size3_class2", new ShipModule(128671255,2,0.2,"Range:0.9km","Fuel Transfer Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size3_class3", new ShipModule(128671256,5,0.34,"Range:1.1km","Fuel Transfer Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size3_class4", new ShipModule(128671257,8,0.48,"Range:1.3km","Fuel Transfer Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size3_class5", new ShipModule(128671258,5,0.41,"Range:1.5km","Fuel Transfer Drone Controller Class 3 Rating A",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size5_class1", new ShipModule(128671259,20,0.4,"Range:0.8km","Fuel Transfer Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size5_class2", new ShipModule(128671260,8,0.3,"Range:1km","Fuel Transfer Drone Controller Class 5 Rating D",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size5_class3", new ShipModule(128671261,20,0.5,"Range:1.3km","Fuel Transfer Drone Controller Class 5 Rating C",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size5_class4", new ShipModule(128671262,32,0.97,"Range:1.6km","Fuel Transfer Drone Controller Class 5 Rating B",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size5_class5", new ShipModule(128671263,20,0.6,"Range:1.8km","Fuel Transfer Drone Controller Class 5 Rating A",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size7_class1", new ShipModule(128671264,80,0.55,"Range:1km","Fuel Transfer Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size7_class2", new ShipModule(128671265,32,0.41,"Range:1.4km","Fuel Transfer Drone Controller Class 7 Rating D",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size7_class3", new ShipModule(128671266,80,0.69,"Range:1.7km","Fuel Transfer Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size7_class4", new ShipModule(128671267,128,0.97,"Range:2km","Fuel Transfer Drone Controller Class 7 Rating B",ShipModule.ModuleTypes.FuelTransferLimpetController) },
            { "int_dronecontrol_fueltransfer_size7_class5", new ShipModule(128671268,80,0.83,"Range:2.4km","Fuel Transfer Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.FuelTransferLimpetController) },

            { "int_dronecontrol_resourcesiphon_size1_class1", new ShipModule(128066532,1.3,0.12,"Time:42s, Range:1.5km","Hatch Breaker Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.HatchBreakerLimpetController) },   // EDDI
            { "int_dronecontrol_resourcesiphon_size1_class2", new ShipModule(128066533,0.5,0.16,"Time:36s, Range:2km","Hatch Breaker Drone Controller Class 1 Rating D",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size1_class3", new ShipModule(128066534,1.3,0.2,"Time:30s, Range:2.5km","Hatch Breaker Drone Controller Class 1 Rating C",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size1_class4", new ShipModule(128066535,2,0.24,"Time:24s, Range:3km","Hatch Breaker Drone Controller Class 1 Rating B",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size1_class5", new ShipModule(128066536,1.3,0.28,"Time:18s, Range:3.5km","Hatch Breaker Drone Controller Class 1 Rating A",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size3_class1", new ShipModule(128066537,5,0.18,"Time:36s, Range:1.6km","Hatch Breaker Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size3_class2", new ShipModule(128066538,2,0.24,"Time:31s, Range:2.2km","Hatch Breaker Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size3_class3", new ShipModule(128066539,5,0.3,"Time:26s, Range:2.7km","Hatch Breaker Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size3_class4", new ShipModule(128066540,8,0.36,"Time:21s, Range:3.2km","Hatch Breaker Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size3_class5", new ShipModule(128066541,5,0.42,"Time:16s, Range:3.8km","Hatch Breaker Drone Controller Class 3 Rating A",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size5_class1", new ShipModule(128066542,20,0.3,"Time:31s, Range:2km","Hatch Breaker Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size5_class2", new ShipModule(128066543,8,0.4,"Time:26s, Range:2.6km","Hatch Breaker Drone Controller Class 5 Rating D",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size5_class3", new ShipModule(128066544,20,0.5,"Time:22s, Range:3.3km","Hatch Breaker Drone Controller Class 5 Rating C",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size5_class4", new ShipModule(128066545,32,0.6,"Time:18s, Range:4km","Hatch Breaker Drone Controller Class 5 Rating B",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size5_class5", new ShipModule(128066546,20,0.7,"Time:13s, Range:4.6km","Hatch Breaker Drone Controller Class 5 Rating A",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size7_class1", new ShipModule(128066547,80,0.42,"Time:25s, Range:2.6km","Hatch Breaker Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size7_class2", new ShipModule(128066548,32,0.56,"Time:22s, Range:3.4km","Hatch Breaker Drone Controller Class 7 Rating D",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size7_class3", new ShipModule(128066549,80,0.7,"Time:18s, Range:4.3km","Hatch Breaker Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size7_class4", new ShipModule(128066550,128,0.84,"Time:14s, Range:5.2km","Hatch Breaker Drone Controller Class 7 Rating B",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon_size7_class5", new ShipModule(128066551,80,0.98,"Time:11s, Range:6km","Hatch Breaker Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.HatchBreakerLimpetController) },
            { "int_dronecontrol_resourcesiphon", new ShipModule(128066402,0,0,"","Hatch Breaker Limpet Controller",ShipModule.ModuleTypes.HatchBreakerLimpetController) },

            { "int_dronecontrol_prospector_size1_class1", new ShipModule(128671269,1.3,0.18,"Range:3km","Prospector Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.ProspectorLimpetController) },    // EDDI
            { "int_dronecontrol_prospector_size1_class2", new ShipModule(128671270,0.5,0.14,"Range:4km","Prospector Drone Controller Class 1 Rating D",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size1_class3", new ShipModule(128671271,1.3,0.23,"Range:5km","Prospector Drone Controller Class 1 Rating C",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size1_class4", new ShipModule(128671272,2,0.32,"Range:6km","Prospector Drone Controller Class 1 Rating B",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size1_class5", new ShipModule(128671273,1.3,0.28,"Range:7km","Prospector Drone Controller Class 1 Rating A",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size3_class1", new ShipModule(128671274,5,0.27,"Range:3.3km","Prospector Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size3_class2", new ShipModule(128671275,2,0.2,"Range:4.4km","Prospector Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size3_class3", new ShipModule(128671276,5,0.34,"Range:5.5km","Prospector Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size3_class4", new ShipModule(128671277,8,0.48,"Range:6.6km","Prospector Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size3_class5", new ShipModule(128671278,5,0.41,"Range:7.7km","Prospector Drone Controller Class 3 Rating A",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size5_class1", new ShipModule(128671279,20,0.4,"Range:3.9km","Prospector Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size5_class2", new ShipModule(128671280,8,0.3,"Range:5.2km","Prospector Drone Controller Class 5 Rating D",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size5_class3", new ShipModule(128671281,20,0.5,"Range:6.5km","Prospector Drone Controller Class 5 Rating C",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size5_class4", new ShipModule(128671282,32,0.97,"Range:7.8km","Prospector Drone Controller Class 5 Rating B",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size5_class5", new ShipModule(128671283,20,0.6,"Range:9.1km","Prospector Drone Controller Class 5 Rating A",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size7_class1", new ShipModule(128671284,80,0.55,"Range:5.1km","Prospector Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size7_class2", new ShipModule(128671285,32,0.41,"Range:6.8km","Prospector Drone Controller Class 7 Rating D",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size7_class3", new ShipModule(128671286,80,0.69,"Range:8.5km","Prospector Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size7_class4", new ShipModule(128671287,128,0.97,"Range:10.2km","Prospector Drone Controller Class 7 Rating B",ShipModule.ModuleTypes.ProspectorLimpetController) },
            { "int_dronecontrol_prospector_size7_class5", new ShipModule(128671288,80,0.83,"Range:11.9km","Prospector Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.ProspectorLimpetController) },

            { "int_dronecontrol_repair_size1_class1", new ShipModule(128777327,1.3,0.18,"Range:0.6km","Repair Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.RepairLimpetController) },  // EDDI
            { "int_dronecontrol_repair_size1_class2", new ShipModule(128777328,0.5,0.14,"Range:0.8km","Repair Drone Controller Class 1 Rating D",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size1_class3", new ShipModule(128777329,1.3,0.23,"Range:1km","Repair Drone Controller Class 1 Rating C",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size1_class4", new ShipModule(128777330,2,0.32,"Range:1.2km","Repair Drone Controller Class 1 Rating B",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size1_class5", new ShipModule(128777331,1.3,0.28,"Range:1.4km","Repair Drone Controller Class 1 Rating A",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size3_class1", new ShipModule(128777332,5,0.27,"Range:0.7km","Repair Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size3_class2", new ShipModule(128777333,2,0.2,"Range:0.9km","Repair Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size3_class3", new ShipModule(128777334,5,0.34,"Range:1.1km","Repair Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size3_class4", new ShipModule(128777335,8,0.48,"Range:1.3km","Repair Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size3_class5", new ShipModule(128777336,5,0.41,"Range:1.5km","Repair Drone Controller Class 3 Rating A",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size5_class1", new ShipModule(128777337,20,0.4,"Range:0.8km","Repair Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size5_class2", new ShipModule(128777338,8,0.3,"Range:1km","Repair Drone Controller Class 5 Rating D",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size5_class3", new ShipModule(128777339,20,0.5,"Range:1.3km","Repair Drone Controller Class 5 Rating C",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size5_class4", new ShipModule(128777340,32,0.97,"Range:1.6km","Repair Drone Controller Class 5 Rating B",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size5_class5", new ShipModule(128777341,20,0.6,"Range:1.8km","Repair Drone Controller Class 5 Rating A",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size7_class1", new ShipModule(128777342,80,0.55,"Range:1km","Repair Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size7_class2", new ShipModule(128777343,32,0.41,"Range:1.4km","Repair Drone Controller Class 7 Rating D",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size7_class3", new ShipModule(128777344,80,0.69,"Range:1.7km","Repair Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size7_class4", new ShipModule(128777345,128,0.97,"Range:2km","Repair Drone Controller Class 7 Rating B",ShipModule.ModuleTypes.RepairLimpetController) },
            { "int_dronecontrol_repair_size7_class5", new ShipModule(128777346,80,0.83,"Range:2.4km","Repair Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.RepairLimpetController) },

            { "int_dronecontrol_unkvesselresearch", new ShipModule(128793116,1.3,0.4,"Time:300s, Range:2km","Drone Controller Vessel Research",ShipModule.ModuleTypes.ResearchLimpetController) },  // EDDI

            // More limpets

            { "int_dronecontrol_decontamination_size1_class1", new ShipModule(128793941,1.3,0.18,"Range:0.6km","Decontamination Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.DecontaminationLimpetController) },   // EDDI
            { "int_dronecontrol_decontamination_size3_class1", new ShipModule(128793942,2,0.2,"Range:0.9km","Decontamination Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.DecontaminationLimpetController) },
            { "int_dronecontrol_decontamination_size5_class1", new ShipModule(128793943,20,0.5,"Range:1.3km","Decontamination Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.DecontaminationLimpetController) },
            { "int_dronecontrol_decontamination_size7_class1", new ShipModule(128793944,128,0.97,"Range:2km","Decontamination Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.DecontaminationLimpetController) },

            { "int_dronecontrol_recon_size1_class1", new ShipModule(128837858,1.3,0.18,"Range:1.2km","Recon Drone Controller Class 1 Rating E",ShipModule.ModuleTypes.ReconLimpetController) }, // EDDI

            { "int_dronecontrol_recon_size3_class1", new ShipModule(128841592,2,0.2,"Range:1.4km","Recon Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.ReconLimpetController) },        // EDDI
            { "int_dronecontrol_recon_size5_class1", new ShipModule(128841593,20,0.5,"Range:1.7km","Recon Drone Controller Class 5 Rating E",ShipModule.ModuleTypes.ReconLimpetController) },
            { "int_dronecontrol_recon_size7_class1", new ShipModule(128841594,128,0.97,"Range:2km","Recon Drone Controller Class 7 Rating E",ShipModule.ModuleTypes.ReconLimpetController) },

            { "int_multidronecontrol_mining_size3_class1", new ShipModule(129001921,12,0.5,"Range:3.3km","Multi Purpose Mining Drone Controller Class 3 Rating E",ShipModule.ModuleTypes.MiningMultiLimpetController) },    // EDDI
            { "int_multidronecontrol_mining_size3_class3", new ShipModule(129001922,10,0.35,"Range:5km","Multi Purpose Mining Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.MiningMultiLimpetController) },
            { "int_multidronecontrol_operations_size3_class3", new ShipModule(129001923,10,0.35,"Range:5km","Multi Purpose Operations Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.OperationsMultiLimpetController) },
            { "int_multidronecontrol_operations_size3_class4", new ShipModule(129001924,15,0.3,"Range:3.1km","Multi Purpose Operations Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.OperationsMultiLimpetController) },
            { "int_multidronecontrol_rescue_size3_class2", new ShipModule(129001925,8,0.4,"Range:2.1km","Multi Purpose Operations Drone Controller Class 3 Rating D",ShipModule.ModuleTypes.RescueMultiLimpetController) },
            { "int_multidronecontrol_rescue_size3_class3", new ShipModule(129001926,10,0.35,"Range:2.6km","Multi Purpose Operations Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.RescueMultiLimpetController) },
            { "int_multidronecontrol_xeno_size3_class3", new ShipModule(129001927,10,0.35,"Range:5km","Multi Purpose Xeno Drone Controller Class 3 Rating C",ShipModule.ModuleTypes.XenoMultiLimpetController) },
            { "int_multidronecontrol_xeno_size3_class4", new ShipModule(129001928,15,0.3,"Range:5km","Multi Purpose Xeno Drone Controller Class 3 Rating B",ShipModule.ModuleTypes.XenoMultiLimpetController) },
            { "int_multidronecontrol_universal_size7_class3", new ShipModule(129001929,126,0.8,"Range:6.5km","Multi Purpose Universal Drone Controller Class 7 Rating C",ShipModule.ModuleTypes.UniversalMultiLimpetController) },
            { "int_multidronecontrol_universal_size7_class5", new ShipModule(129001930,140,1.1,"Range:9.1km","Multi Purpose Universal Drone Controller Class 7 Rating A",ShipModule.ModuleTypes.UniversalMultiLimpetController) },

            // Meta hull reinforcements

            { "int_metaalloyhullreinforcement_size1_class1", new ShipModule(128793117,2,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) }, // EDDI
            { "int_metaalloyhullreinforcement_size1_class2", new ShipModule(128793118,2,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size2_class1", new ShipModule(128793119,4,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size2_class2", new ShipModule(128793120,2,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size3_class1", new ShipModule(128793121,8,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size3_class2", new ShipModule(128793122,4,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size4_class1", new ShipModule(128793123,16,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size4_class2", new ShipModule(128793124,8,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size5_class1", new ShipModule(128793125,32,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },
            { "int_metaalloyhullreinforcement_size5_class2", new ShipModule(128793126,16,0,"Explosive:0%, Kinetic:0%, Thermal:0%","Meta Alloy Hull Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.MetaAlloyHullReinforcement) },

            // Mine launches charges

            { "hpt_minelauncher_fixed_small", new ShipModule(128049500,2,0.4,"Ammo:36/1, Damage:44, Reload:2s, ThermL:5","Mine Launcher Fixed Small",ShipModule.ModuleTypes.MineLauncher) },        // EDDI
            { "hpt_minelauncher_fixed_medium", new ShipModule(128049501,4,0.4,"Ammo:72/3, Damage:44, Reload:6.6s, ThermL:7.5","Mine Launcher Fixed Medium",ShipModule.ModuleTypes.MineLauncher) },
            { "hpt_minelauncher_fixed_small_impulse", new ShipModule(128671448,2,0.4,"Ammo:36/1, Damage:32, Reload:2s, ThermL:5","Mine Launcher Fixed Small Impulse",ShipModule.ModuleTypes.ShockMineLauncher) },

            { "hpt_mining_abrblstr_fixed_small", new ShipModule(128915458,2,0.34,"Damage:4, Range:1000m, Speed:667m/s, Reload:2s, ThermL:1.8","Mining Abrasion Blaster Fixed Small",ShipModule.ModuleTypes.AbrasionBlaster) },  // EDDI
            { "hpt_mining_abrblstr_turret_small", new ShipModule(128915459,2,0.47,"Damage:4, Range:1000m, Speed:667m/s, Reload:2s, ThermL:1.8","Mining Abrasion Blaster Turret Small",ShipModule.ModuleTypes.AbrasionBlaster) },

            { "hpt_mining_seismchrgwarhd_fixed_medium", new ShipModule(128915460,4,1.2,"Ammo:72/1, Damage:15, Range:3000m, Speed:350m/s, ThermL:3.6","Mining Seismic Charge Warhead Fixed Medium",ShipModule.ModuleTypes.SeismicChargeLauncher) }, // EDDI
            { "hpt_mining_seismchrgwarhd_turret_medium", new ShipModule(128915461,4,1.2,"Ammo:72/1, Damage:15, Range:3000m, Speed:350m/s, ThermL:3.6","Mining Seismic Charge Warhead Turret Medium",ShipModule.ModuleTypes.SeismicChargeLauncher) },

            { "hpt_mining_subsurfdispmisle_fixed_small", new ShipModule(128915454,2,0.42,"Ammo:32/1, Damage:5, Range:3000m, Speed:550m/s, Reload:2s, ThermL:2.2","Mining Sub Surface Displacement Missile Fixed Small",ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile) }, // EDDI
            { "hpt_mining_subsurfdispmisle_turret_small", new ShipModule(128915455,2,0.53,"Ammo:32/1, Damage:5, Range:3000m, Speed:550m/s, Reload:2s, ThermL:2.2","Mining Subsurface Displacement Missile Turret Small",ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile) },
            { "hpt_mining_subsurfdispmisle_fixed_medium", new ShipModule(128915456,4,1.01,"Ammo:96/1, Damage:5, Range:3000m, Speed:550m/s, Reload:2s, ThermL:2.9","Mining Sub Surface Displacement Missile Fixed Medium",ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile) },
            { "hpt_mining_subsurfdispmisle_turret_medium", new ShipModule(128915457,4,0.93,"Ammo:96/1, Damage:5, Range:3000m, Speed:550m/s, Reload:2s, ThermL:2.9","Mining Subsurface Displacement Missile Turret Medium",ShipModule.ModuleTypes.Sub_SurfaceDisplacementMissile) },

            // Mining lasers

            { "hpt_mininglaser_fixed_small", new ShipModule(128049525,2,0.5,"Damage:2, Range:500m, ThermL:2","Mining Laser Fixed Small",ShipModule.ModuleTypes.MiningLaser) },      // EDDI
            { "hpt_mininglaser_fixed_medium", new ShipModule(128049526,2,0.75,"Damage:4, Range:500m, ThermL:4","Mining Laser Fixed Medium",ShipModule.ModuleTypes.MiningLaser) },
            { "hpt_mininglaser_turret_small", new ShipModule(128740819,2,0.5,"Damage:2, Range:500m, ThermL:2","Mining Laser Turret Small",ShipModule.ModuleTypes.MiningLaser) },
            { "hpt_mininglaser_turret_medium", new ShipModule(128740820,2,0.75,"Damage:4, Range:500m, ThermL:4","Mining Laser Turret Medium",ShipModule.ModuleTypes.MiningLaser) },
            { "hpt_mininglaser_fixed_small_advanced", new ShipModule(128671340,2,0.7,"Damage:8, Range:2000m, ThermL:6","Mining Laser Fixed Small Advanced",ShipModule.ModuleTypes.MiningLance) },
            
            // Missiles

            { "hpt_atdumbfiremissile_fixed_medium", new ShipModule(128788699,4,1.2,"Ammo:64/8, Damage:64, Speed:750m/s, Reload:5s, ThermL:2.4","AX Dumbfire Missile Fixed Medium",ShipModule.ModuleTypes.AXMissileRack) },  // EDDI
            { "hpt_atdumbfiremissile_fixed_large", new ShipModule(128788700,8,1.62,"Ammo:128/12, Damage:64, Speed:750m/s, Reload:5s, ThermL:3.6","AX Dumbfire Missile Fixed Large",ShipModule.ModuleTypes.AXMissileRack) },
            { "hpt_atdumbfiremissile_turret_medium", new ShipModule(128788704,4,1.2,"Ammo:64/8, Damage:50, Speed:750m/s, Reload:5s, ThermL:1.5","AX Dumbfire Missile Turret Medium",ShipModule.ModuleTypes.AXMissileRack) },
            { "hpt_atdumbfiremissile_turret_large", new ShipModule(128788705,8,1.75,"Ammo:128/12, Damage:64, Speed:750m/s, Reload:5s, ThermL:1.9","AX Dumbfire Missile Turret Large",ShipModule.ModuleTypes.AXMissileRack) },

            { "hpt_atdumbfiremissile_fixed_medium_v2", new ShipModule(129022081,4,1.3,"Damage: 16.0/S","Enhanced AX Missile Rack 2D",ShipModule.ModuleTypes.EnhancedAXMissileRack) }, // EDDI
            { "hpt_atdumbfiremissile_fixed_large_v2", new ShipModule(129022079,8,1.72,"Damage: 16.0/S","Enhanced AX Missile Rack 3B",ShipModule.ModuleTypes.EnhancedAXMissileRack) },
            { "hpt_atdumbfiremissile_turret_medium_v2", new ShipModule(129022083,4,1.3,"Damage: 12.2/S","Enhanced AX Missile Rack 2E",ShipModule.ModuleTypes.EnhancedAXMissileRack) },
            { "hpt_atdumbfiremissile_turret_large_v2", new ShipModule(129022082,8,1.85,"Damage: 12.2/S","Enhanced AX Missile Rack 3D",ShipModule.ModuleTypes.EnhancedAXMissileRack) },

            { "hpt_basicmissilerack_fixed_small", new ShipModule(128049492,2,0.6,"Ammo:6/6, Damage:40, Speed:625m/s, Reload:12s, ThermL:3.6","Seeker Missile Rack Fixed Small",ShipModule.ModuleTypes.SeekerMissileRack) },     // EDDI
            { "hpt_basicmissilerack_fixed_medium", new ShipModule(128049493,4,1.2,"Ammo:18/6, Damage:40, Speed:625m/s, Reload:12s, ThermL:3.6","Seeker Missile Rack Fixed Medium",ShipModule.ModuleTypes.SeekerMissileRack) },
            { "hpt_basicmissilerack_fixed_large", new ShipModule(128049494,8,1.62,"Ammo:36/6, Damage:40, Speed:625m/s, Reload:12s, ThermL:3.6","Seeker Missile Rack Fixed Large",ShipModule.ModuleTypes.SeekerMissileRack) },

            { "hpt_dumbfiremissilerack_fixed_small", new ShipModule(128666724,2,0.4,"Ammo:16/8, Damage:50, Speed:750m/s, Reload:5s, ThermL:3.6","Dumbfire Missile Rack Fixed Small",ShipModule.ModuleTypes.MissileRack) },      // EDDI
            { "hpt_dumbfiremissilerack_fixed_medium", new ShipModule(128666725,4,1.2,"Ammo:48/12, Damage:50, Speed:750m/s, Reload:5s, ThermL:3.6","Dumbfire Missile Rack Fixed Medium",ShipModule.ModuleTypes.MissileRack) },
            { "hpt_dumbfiremissilerack_fixed_large", new ShipModule(128891602,8,1.62,"Ammo:96/12, Damage:50, Speed:750m/s, Reload:5s, ThermL:3.6","Dumbfire Missile Rack Fixed Large",ShipModule.ModuleTypes.MissileRack) },

            { "hpt_dumbfiremissilerack_fixed_medium_lasso", new ShipModule(128732552,4,1.2,"Ammo:48/12, Damage:40, Speed:750m/s, Reload:5s, ThermL:3.6","Dumbfire Missile Rack Fixed Medium Lasso",ShipModule.ModuleTypes.RocketPropelledFSDDisruptor) },   // EDDI
            { "hpt_drunkmissilerack_fixed_medium", new ShipModule(128671344,4,1.2,"Ammo:120/12, Damage:7.5, Speed:600m/s, Reload:5s, ThermL:3.6","Pack Hound Missile Rack Fixed Medium",ShipModule.ModuleTypes.Pack_HoundMissileRack) },

            { "hpt_advancedtorppylon_fixed_small", new ShipModule(128049509,2,0.4,"Ammo:1/1, Damage:120, Speed:250m/s, Reload:5s, ThermL:45","Advanced Torp Pylon Fixed Small",ShipModule.ModuleTypes.TorpedoPylon) },   // EDDI
            { "hpt_advancedtorppylon_fixed_medium", new ShipModule(128049510,4,0.4,"Ammo:2/1, Damage:120, Speed:250m/s, Reload:5s, ThermL:50","Advanced Torp Pylon Fixed Medium",ShipModule.ModuleTypes.TorpedoPylon) },
            { "hpt_advancedtorppylon_fixed_large", new ShipModule(128049511,8,0.6,"Ammo:4/4, Damage:120, Speed:250m/s, Reload:5s, ThermL:55","Advanced Torp Pylon Fixed Large",ShipModule.ModuleTypes.TorpedoPylon) },

            { "hpt_dumbfiremissilerack_fixed_small_advanced", new ShipModule(128935982,1,0.4,null,"Dumbfire Missile Rack Fixed Small Advanced",ShipModule.ModuleTypes.AdvancedMissileRack) },       // EDDI
            { "hpt_dumbfiremissilerack_fixed_medium_advanced", new ShipModule(128935983,1,1.2,null,"Dumbfire Missile Rack Fixed Medium Advanced",ShipModule.ModuleTypes.AdvancedMissileRack) },

            { "hpt_human_extraction_fixed_medium", new ShipModule(129028577,1,1.2,null,"Human Extraction Missile",ShipModule.ModuleTypes.MissileRack) },        // EDDI TBD CAT

            { "hpt_causticmissile_fixed_medium", new ShipModule(128833995,4,1.2,"Ammo:64/8, Damage:5, Speed:750m/s, Reload:5s, ThermL:1.5","Caustic Missile Fixed Medium",ShipModule.ModuleTypes.EnzymeMissileRack) },


            // Module Reinforcements

            { "int_modulereinforcement_size1_class1", new ShipModule(128737270,2,0,"Protection:0.3","Module Reinforcement Class 1 Rating E",ShipModule.ModuleTypes.ModuleReinforcementPackage) },   // EDDI
            { "int_modulereinforcement_size1_class2", new ShipModule(128737271,1,0,"Protection:0.6","Module Reinforcement Class 1 Rating D",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size2_class1", new ShipModule(128737272,4,0,"Protection:0.3","Module Reinforcement Class 2 Rating E",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size2_class2", new ShipModule(128737273,2,0,"Protection:0.6","Module Reinforcement Class 2 Rating D",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size3_class1", new ShipModule(128737274,8,0,"Protection:0.3","Module Reinforcement Class 3 Rating E",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size3_class2", new ShipModule(128737275,4,0,"Protection:0.6","Module Reinforcement Class 3 Rating D",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size4_class1", new ShipModule(128737276,16,0,"Protection:0.3","Module Reinforcement Class 4 Rating E",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size4_class2", new ShipModule(128737277,8,0,"Protection:0.6","Module Reinforcement Class 4 Rating D",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size5_class1", new ShipModule(128737278,32,0,"Protection:0.3","Module Reinforcement Class 5 Rating E",ShipModule.ModuleTypes.ModuleReinforcementPackage) },
            { "int_modulereinforcement_size5_class2", new ShipModule(128737279,16,0,"Protection:0.6","Module Reinforcement Class 5 Rating D",ShipModule.ModuleTypes.ModuleReinforcementPackage) },

            // Multicannons

            { "hpt_atmulticannon_fixed_medium", new ShipModule(128788701,4,0.46,"Ammo:2100/100, Damage:3.3, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.2","AX Multi Cannon Fixed Medium",ShipModule.ModuleTypes.AXMulti_Cannon) }, // EDDI
            { "hpt_atmulticannon_fixed_large", new ShipModule(128788702,8,0.64,"Ammo:2100/100, Damage:6.1, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.3","AX Multi Cannon Fixed Large",ShipModule.ModuleTypes.AXMulti_Cannon) },

            { "hpt_atmulticannon_turret_medium", new ShipModule(128793059,4,0.5,"Ammo:2100/90, Damage:1.7, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.1","AX Multi Cannon Turret Medium",ShipModule.ModuleTypes.AXMulti_Cannon) }, // EDDI
            { "hpt_atmulticannon_turret_large", new ShipModule(128793060,8,0.64,"Ammo:2100/90, Damage:3.3, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.1","AX Multi Cannon Turret Large",ShipModule.ModuleTypes.AXMulti_Cannon) },

            { "hpt_atmulticannon_fixed_medium_v2", new ShipModule(129022080,4,0.48,"Damage: 10/S","Enhanced AX Multi Cannon 2D",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) }, // EDDI
            { "hpt_atmulticannon_fixed_large_v2", new ShipModule(129022084,8,0.69,"Damage: 15.6/S","Enhanced AX Multi Cannon 3B",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) },

            { "hpt_atmulticannon_turret_medium_v2", new ShipModule(129022086,4,0.52,"","Enhanced AX Multi Cannon 2E",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) }, // EDDI
            { "hpt_atmulticannon_turret_large_v2", new ShipModule(129022085,8,0.69,"","Enhanced AX Multi Cannon 3D",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) },

            { "hpt_atmulticannon_gimbal_medium", new ShipModule(129022089,4,0.46,"Damage: 9/6/S","Enhanced AX Multi Cannon 2E",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) }, // EDDI
            { "hpt_atmulticannon_gimbal_large", new ShipModule(129022088,8,0.64,"Damage: 15.2/S","Enhanced AX Multi Cannon 3C",ShipModule.ModuleTypes.EnhancedAXMulti_Cannon) },

            { "hpt_multicannon_fixed_small", new ShipModule(128049455,2,0.28,"Ammo:2100/100, Damage:1.1, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.1","Multi Cannon Fixed Small",ShipModule.ModuleTypes.Multi_Cannon) },  // EDDI
            { "hpt_multicannon_fixed_medium", new ShipModule(128049456,4,0.46,"Ammo:2100/100, Damage:2.2, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.2","Multi Cannon Fixed Medium",ShipModule.ModuleTypes.Multi_Cannon) },
            { "hpt_multicannon_fixed_large", new ShipModule(128049457,4,0.46,"Ammo:2100/100, Damage:2.2, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.2","Multi Cannon Fixed Medium",ShipModule.ModuleTypes.Multi_Cannon) },     //TBD STATS
            { "hpt_multicannon_fixed_huge", new ShipModule(128049458,16,0.73,"Ammo:2100/100, Damage:4.6, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.4","Multi Cannon Fixed Huge",ShipModule.ModuleTypes.Multi_Cannon) },

            { "hpt_multicannon_gimbal_small", new ShipModule(128049459,2,0.37,"Ammo:2100/90, Damage:0.8, Range:4000m, Speed:1600m/s, Reload:5s, ThermL:0.1","Multi Cannon Gimbal Small",ShipModule.ModuleTypes.Multi_Cannon) }, // EDDI
            { "hpt_multicannon_gimbal_medium", new ShipModule(128049460,4,0.64,"Ammo:2100/90, Damage:1.6, Range:4000m, Speed:1600m/s, Reload:5s, ThermL:0.2","Multi Cannon Gimbal Medium",ShipModule.ModuleTypes.Multi_Cannon) },
            { "hpt_multicannon_gimbal_large", new ShipModule(128049461,8,0.97,"Ammo:2100/90, Damage:2.8, Range:4000m, Speed:1600m/s, Reload:5s, ThermL:0.3","Multi Cannon Gimbal Large",ShipModule.ModuleTypes.Multi_Cannon) },

            { "hpt_multicannon_turret_small", new ShipModule(128049462,2,0.26,"Ammo:2100/90, Damage:0.6, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0","Multi Cannon Turret Small",ShipModule.ModuleTypes.Multi_Cannon) }, // EDDI
            { "hpt_multicannon_turret_medium", new ShipModule(128049463,4,0.5,"Ammo:2100/90, Damage:1.2, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.1","Multi Cannon Turret Medium",ShipModule.ModuleTypes.Multi_Cannon) },
            { "hpt_multicannon_turret_large", new ShipModule(128049464,8,0.86,"Ammo:2100/90, Damage:2.2, Range:4000m, Speed:1600m/s, Reload:4s, ThermL:0.2","Multi Cannon Turret Large",ShipModule.ModuleTypes.Multi_Cannon) },

            { "hpt_multicannon_gimbal_huge", new ShipModule(128681996,16,1.22,"Ammo:2100/90, Damage:3.5, Range:4000m, Speed:1600m/s, Reload:5s, ThermL:0.5","Multi Cannon Gimbal Huge",ShipModule.ModuleTypes.Multi_Cannon) },  // EDDI

            { "hpt_multicannon_fixed_small_strong", new ShipModule(128671345,2,0.28,"Ammo:1000/60, Damage:2.9, Range:4500m, Speed:1800m/s, Reload:4s, ThermL:0.2","Multi Cannon Fixed Small Strong",ShipModule.ModuleTypes.EnforcerCannon) },   // EDDI

            { "hpt_multicannon_fixed_medium_advanced", new ShipModule(128935980,1,0.5,null,"Multi Cannon Fixed Medium Advanced",ShipModule.ModuleTypes.AdvancedMulti_Cannon) }, // EDDI
            { "hpt_multicannon_fixed_small_advanced", new ShipModule(128935981,1,0.3,null,"Multi Cannon Fixed Small Advanced",ShipModule.ModuleTypes.AdvancedMulti_Cannon) },

            // Passenger cabins

            { "int_passengercabin_size4_class1", new ShipModule(128727922,10,0,"Passengers:8","Economy Passenger Cabin Class 4 Rating E",ShipModule.ModuleTypes.EconomyClassPassengerCabin ) },         // EDDI
            { "int_passengercabin_size4_class2", new ShipModule(128727923,10,0,"Passengers:6","Business Class Passenger Cabin Class 4 Rating D",ShipModule.ModuleTypes.BusinessClassPassengerCabin) },
            { "int_passengercabin_size4_class3", new ShipModule(128727924,10,0,"Passengers:3","First Class Passenger Cabin Class 4 Rating C",ShipModule.ModuleTypes.FirstClassPassengerCabin) },
            { "int_passengercabin_size5_class4", new ShipModule(128727925,20,0,"Passengers:4","Luxury Passenger Cabin Class 5 Rating B",ShipModule.ModuleTypes.LuxuryClassPassengerCabin) },
            { "int_passengercabin_size6_class1", new ShipModule(128727926,40,0,"Passengers:32","Economy Passenger Cabin Class 6 Rating E",ShipModule.ModuleTypes.EconomyClassPassengerCabin) },
            { "int_passengercabin_size6_class2", new ShipModule(128727927,40,0,"Passengers:16","Business Class Passenger Cabin Class 6 Rating D",ShipModule.ModuleTypes.BusinessClassPassengerCabin ) },
            { "int_passengercabin_size6_class3", new ShipModule(128727928,40,0,"Passengers:12","First Class Passenger Cabin Class 6 Rating C",ShipModule.ModuleTypes.FirstClassPassengerCabin) },
            { "int_passengercabin_size6_class4", new ShipModule(128727929,40,0,"Passengers:8","Luxury Passenger Cabin Class 6 Rating B",ShipModule.ModuleTypes.LuxuryClassPassengerCabin) },

            { "int_passengercabin_size2_class1", new ShipModule(128734690,2.5,0,"Passengers:2","Economy Passenger Cabin Class 2 Rating E",ShipModule.ModuleTypes.EconomyClassPassengerCabin ) },        // EDDI
            { "int_passengercabin_size3_class1", new ShipModule(128734691,5,0,"Passengers:4","Economy Passenger Cabin Class 3 Rating E",ShipModule.ModuleTypes.EconomyClassPassengerCabin ) },
            { "int_passengercabin_size3_class2", new ShipModule(128734692,5,0,"Passengers:3","Business Class Passenger Cabin Class 3 Rating D",ShipModule.ModuleTypes.BusinessClassPassengerCabin) },
            { "int_passengercabin_size5_class1", new ShipModule(128734693,20,0,"Passengers:16","Economy Passenger Cabin Class 5 Rating E",ShipModule.ModuleTypes.EconomyClassPassengerCabin) },
            { "int_passengercabin_size5_class2", new ShipModule(128734694,20,0,"Passengers:10","Business Class Passenger Cabin Class 5 Rating D",ShipModule.ModuleTypes.BusinessClassPassengerCabin ) },
            { "int_passengercabin_size5_class3", new ShipModule(128734695,20,0,"Passengers:6","First Class Passenger Cabin Class 5 Rating C",ShipModule.ModuleTypes.FirstClassPassengerCabin) },

            // Planetary approach

            { "int_planetapproachsuite_advanced", new ShipModule(128975719,0,0,null,"Advanced Planet Approach Suite",ShipModule.ModuleTypes.AdvancedPlanetaryApproachSuite) },  // EDDI
            { "int_planetapproachsuite", new ShipModule(128672317,0,0,null,"Planet Approach Suite",ShipModule.ModuleTypes.PlanetaryApproachSuite) },

            // planetary hangar

            { "int_buggybay_size2_class1", new ShipModule(128672288,12,0.25,null,"Planetary Vehicle Hangar Class 2 Rating H",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },   // EDDI
            { "int_buggybay_size2_class2", new ShipModule(128672289,6,0.75,null,"Planetary Vehicle Hangar Class 2 Rating G",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },
            { "int_buggybay_size4_class1", new ShipModule(128672290,20,0.4,null,"Planetary Vehicle Hangar Class 4 Rating H",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },
            { "int_buggybay_size4_class2", new ShipModule(128672291,10,1.2,null,"Planetary Vehicle Hangar Class 4 Rating G",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },
            { "int_buggybay_size6_class1", new ShipModule(128672292,34,0.6,null,"Planetary Vehicle Hangar Class 6 Rating H",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },
            { "int_buggybay_size6_class2", new ShipModule(128672293,17,1.8,null,"Planetary Vehicle Hangar Class 6 Rating G",ShipModule.ModuleTypes.PlanetaryVehicleHangar) },

            // Plasmas

            { "hpt_plasmaaccelerator_fixed_medium", new ShipModule(128049465,4,1.43,"Ammo:100/5, Damage:54.3, Range:3500m, Speed:875m/s, Reload:6s, ThermL:15.6","Plasma Accelerator Fixed Medium",ShipModule.ModuleTypes.PlasmaAccelerator) }, // EDDI
            { "hpt_plasmaaccelerator_fixed_large", new ShipModule(128049466,8,1.97,"Ammo:100/5, Damage:83.4, Range:3500m, Speed:875m/s, Reload:6s, ThermL:21.8","Plasma Accelerator Fixed Large",ShipModule.ModuleTypes.PlasmaAccelerator) },
            { "hpt_plasmaaccelerator_fixed_huge", new ShipModule(128049467,16,2.63,"Ammo:100/5, Damage:125.2, Range:3500m, Speed:875m/s, Reload:6s, ThermL:29.5","Plasma Accelerator Fixed Huge",ShipModule.ModuleTypes.PlasmaAccelerator) },
            { "hpt_plasmaaccelerator_fixed_large_advanced", new ShipModule(128671339,8,1.97,"Ammo:300/20, Damage:34.5, Range:3500m, Speed:875m/s, Reload:6s, ThermL:11","Plasma Accelerator Fixed Large Advanced",ShipModule.ModuleTypes.AdvancedPlasmaAccelerator) },

            { "hpt_plasmashockcannon_fixed_large", new ShipModule(128834780,8,0.89,"Ammo:240/16, Damage:18.1, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:2.7","Plasma Shock Cannon Fixed Large",ShipModule.ModuleTypes.ShockCannon) },   // EDDI
            { "hpt_plasmashockcannon_gimbal_large", new ShipModule(128834781,8,0.89,"Ammo:240/16, Damage:14.9, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:3.1","Plasma Shock Cannon Gimbal Large",ShipModule.ModuleTypes.ShockCannon) },
            { "hpt_plasmashockcannon_turret_large", new ShipModule(128834782,8,0.64,"Ammo:240/16, Damage:12.3, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:2.2","Plasma Shock Cannon Turret Large",ShipModule.ModuleTypes.ShockCannon) },

            { "hpt_plasmashockcannon_fixed_medium", new ShipModule(128834002,4,0.57,"Ammo:240/16, Damage:13, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:1.8","Plasma Shock Cannon Fixed Medium",ShipModule.ModuleTypes.ShockCannon) },   // EDDI 
            { "hpt_plasmashockcannon_gimbal_medium", new ShipModule(128834003,4,0.61,"Ammo:240/16, Damage:10.2, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:2.1","Plasma Shock Cannon Gimbal Medium",ShipModule.ModuleTypes.ShockCannon) },
            { "hpt_plasmashockcannon_turret_medium", new ShipModule(128834004,4,0.5,"Ammo:240/16, Damage:9, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:1.2","Plasma Shock Cannon Turret Medium",ShipModule.ModuleTypes.ShockCannon) },

            { "hpt_plasmashockcannon_turret_small", new ShipModule(128891603,2,0.54,"Ammo:240/16, Damage:4.5, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:0.7","Plasma Shock Cannon Turret Small",ShipModule.ModuleTypes.ShockCannon) },  // EDDI 
            { "hpt_plasmashockcannon_gimbal_small", new ShipModule(128891604,2,0.47,"Ammo:240/16, Damage:6.9, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:1.5","Plasma Shock Cannon Gimbal Small",ShipModule.ModuleTypes.ShockCannon) },
            { "hpt_plasmashockcannon_fixed_small", new ShipModule(128891605,2,0.41,"Ammo:240/16, Damage:8.6, Range:3000m, Speed:1200m/s, Reload:6s, ThermL:1.1","Plasma Shock Cannon Fixed Small",ShipModule.ModuleTypes.ShockCannon) },

            // power distributor

            { "int_powerdistributor_size1_class1", new ShipModule(128064178,1.3,0.32,"Sys:0.4MW, Eng:0.4MW, Wep:1.2MW","Power Distributor Class 1 Rating E",ShipModule.ModuleTypes.PowerDistributor) },     // EDDI
            { "int_powerdistributor_size1_class2", new ShipModule(128064179,0.5,0.36,"Sys:0.5MW, Eng:0.5MW, Wep:1.4MW","Power Distributor Class 1 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size1_class3", new ShipModule(128064180,1.3,0.4,"Sys:0.5MW, Eng:0.5MW, Wep:1.5MW","Power Distributor Class 1 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size1_class4", new ShipModule(128064181,2,0.44,"Sys:0.6MW, Eng:0.6MW, Wep:1.7MW","Power Distributor Class 1 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size1_class5", new ShipModule(128064182,1.3,0.48,"Sys:0.6MW, Eng:0.6MW, Wep:1.8MW","Power Distributor Class 1 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size2_class1", new ShipModule(128064183,2.5,0.36,"Sys:0.6MW, Eng:0.6MW, Wep:1.4MW","Power Distributor Class 2 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size2_class2", new ShipModule(128064184,1,0.41,"Sys:0.6MW, Eng:0.6MW, Wep:1.6MW","Power Distributor Class 2 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size2_class3", new ShipModule(128064185,2.5,0.45,"Sys:0.7MW, Eng:0.7MW, Wep:1.8MW","Power Distributor Class 2 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size2_class4", new ShipModule(128064186,4,0.5,"Sys:0.8MW, Eng:0.8MW, Wep:2MW","Power Distributor Class 2 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size2_class5", new ShipModule(128064187,2.5,0.54,"Sys:0.8MW, Eng:0.8MW, Wep:2.2MW","Power Distributor Class 2 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size3_class1", new ShipModule(128064188,5,0.4,"Sys:0.9MW, Eng:0.9MW, Wep:1.8MW","Power Distributor Class 3 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size3_class2", new ShipModule(128064189,2,0.45,"Sys:1MW, Eng:1MW, Wep:2.1MW","Power Distributor Class 3 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size3_class3", new ShipModule(128064190,5,0.5,"Sys:1.1MW, Eng:1.1MW, Wep:2.3MW","Power Distributor Class 3 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size3_class4", new ShipModule(128064191,8,0.55,"Sys:1.2MW, Eng:1.2MW, Wep:2.5MW","Power Distributor Class 3 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size3_class5", new ShipModule(128064192,5,0.6,"Sys:1.3MW, Eng:1.3MW, Wep:2.8MW","Power Distributor Class 3 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size4_class1", new ShipModule(128064193,10,0.45,"Sys:1.3MW, Eng:1.3MW, Wep:2.3MW","Power Distributor Class 4 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size4_class2", new ShipModule(128064194,4,0.5,"Sys:1.4MW, Eng:1.4MW, Wep:2.6MW","Power Distributor Class 4 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size4_class3", new ShipModule(128064195,10,0.56,"Sys:1.6MW, Eng:1.6MW, Wep:2.9MW","Power Distributor Class 4 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size4_class4", new ShipModule(128064196,16,0.62,"Sys:1.8MW, Eng:1.8MW, Wep:3.2MW","Power Distributor Class 4 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size4_class5", new ShipModule(128064197,10,0.67,"Sys:1.9MW, Eng:1.9MW, Wep:3.5MW","Power Distributor Class 4 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size5_class1", new ShipModule(128064198,20,0.5,"Sys:1.7MW, Eng:1.7MW, Wep:2.9MW","Power Distributor Class 5 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size5_class2", new ShipModule(128064199,8,0.56,"Sys:1.9MW, Eng:1.9MW, Wep:3.2MW","Power Distributor Class 5 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size5_class3", new ShipModule(128064200,20,0.62,"Sys:2.1MW, Eng:2.1MW, Wep:3.6MW","Power Distributor Class 5 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size5_class4", new ShipModule(128064201,32,0.68,"Sys:2.3MW, Eng:2.3MW, Wep:4MW","Power Distributor Class 5 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size5_class5", new ShipModule(128064202,20,0.74,"Sys:2.5MW, Eng:2.5MW, Wep:4.3MW","Power Distributor Class 5 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size6_class1", new ShipModule(128064203,40,0.54,"Sys:2.2MW, Eng:2.2MW, Wep:3.4MW","Power Distributor Class 6 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size6_class2", new ShipModule(128064204,16,0.61,"Sys:2.4MW, Eng:2.4MW, Wep:3.9MW","Power Distributor Class 6 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size6_class3", new ShipModule(128064205,40,0.68,"Sys:2.7MW, Eng:2.7MW, Wep:4.3MW","Power Distributor Class 6 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size6_class4", new ShipModule(128064206,64,0.75,"Sys:3MW, Eng:3MW, Wep:4.7MW","Power Distributor Class 6 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size6_class5", new ShipModule(128064207,40,0.82,"Sys:3.2MW, Eng:3.2MW, Wep:5.2MW","Power Distributor Class 6 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size7_class1", new ShipModule(128064208,80,0.59,"Sys:2.6MW, Eng:2.6MW, Wep:4.1MW","Power Distributor Class 7 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size7_class2", new ShipModule(128064209,32,0.67,"Sys:3MW, Eng:3MW, Wep:4.6MW","Power Distributor Class 7 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size7_class3", new ShipModule(128064210,80,0.74,"Sys:3.3MW, Eng:3.3MW, Wep:5.1MW","Power Distributor Class 7 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size7_class4", new ShipModule(128064211,128,0.81,"Sys:3.6MW, Eng:3.6MW, Wep:5.6MW","Power Distributor Class 7 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size7_class5", new ShipModule(128064212,80,0.89,"Sys:4MW, Eng:4MW, Wep:6.1MW","Power Distributor Class 7 Rating A",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size8_class1", new ShipModule(128064213,160,0.64,"Sys:3.2MW, Eng:3.2MW, Wep:4.8MW","Power Distributor Class 8 Rating E",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size8_class2", new ShipModule(128064214,64,0.72,"Sys:3.6MW, Eng:3.6MW, Wep:5.4MW","Power Distributor Class 8 Rating D",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size8_class3", new ShipModule(128064215,160,0.8,"Sys:4MW, Eng:4MW, Wep:6MW","Power Distributor Class 8 Rating C",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size8_class4", new ShipModule(128064216,256,0.88,"Sys:4.4MW, Eng:4.4MW, Wep:6.6MW","Power Distributor Class 8 Rating B",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerdistributor_size8_class5", new ShipModule(128064217,160,0.96,"Sys:4.8MW, Eng:4.8MW, Wep:7.2MW","Power Distributor Class 8 Rating A",ShipModule.ModuleTypes.PowerDistributor) },

            { "int_powerdistributor_size1_class1_free", new ShipModule(128666639,1.3,0.32,"Sys:0.4MW, Eng:0.4MW, Wep:1.2MW","Power Distributor Class 1 Rating E",ShipModule.ModuleTypes.PowerDistributor) },

            // Power plant

            { "int_powerplant_size2_class1", new ShipModule(128064033,2.5,0,"Power:6.4MW","Powerplant Class 2 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size2_class2", new ShipModule(128064034,1,0,"Power:7.2MW","Powerplant Class 2 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size2_class3", new ShipModule(128064035,1.3,0,"Power:8MW","Powerplant Class 2 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size2_class4", new ShipModule(128064036,2,0,"Power:8.8MW","Powerplant Class 2 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size2_class5", new ShipModule(128064037,1.3,0,"Power:9.6MW","Powerplant Class 2 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size3_class1", new ShipModule(128064038,5,0,"Power:8MW","Powerplant Class 3 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size3_class2", new ShipModule(128064039,2,0,"Power:9MW","Powerplant Class 3 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size3_class3", new ShipModule(128064040,2.5,0,"Power:10MW","Powerplant Class 3 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size3_class4", new ShipModule(128064041,4,0,"Power:11MW","Powerplant Class 3 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size3_class5", new ShipModule(128064042,2.5,0,"Power:12MW","Powerplant Class 3 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size4_class1", new ShipModule(128064043,10,0,"Power:10.4MW","Powerplant Class 4 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size4_class2", new ShipModule(128064044,4,0,"Power:11.7MW","Powerplant Class 4 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size4_class3", new ShipModule(128064045,5,0,"Power:13MW","Powerplant Class 4 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size4_class4", new ShipModule(128064046,8,0,"Power:14.3MW","Powerplant Class 4 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size4_class5", new ShipModule(128064047,5,0,"Power:15.6MW","Powerplant Class 4 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size5_class1", new ShipModule(128064048,20,0,"Power:13.6MW","Powerplant Class 5 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size5_class2", new ShipModule(128064049,8,0,"Power:15.3MW","Powerplant Class 5 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size5_class3", new ShipModule(128064050,10,0,"Power:17MW","Powerplant Class 5 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size5_class4", new ShipModule(128064051,16,0,"Power:18.7MW","Powerplant Class 5 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size5_class5", new ShipModule(128064052,10,0,"Power:20.4MW","Powerplant Class 5 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size6_class1", new ShipModule(128064053,40,0,"Power:16.8MW","Powerplant Class 6 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size6_class2", new ShipModule(128064054,16,0,"Power:18.9MW","Powerplant Class 6 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size6_class3", new ShipModule(128064055,20,0,"Power:21MW","Powerplant Class 6 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size6_class4", new ShipModule(128064056,32,0,"Powter:23.1MW","Powerplant Class 6 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size6_class5", new ShipModule(128064057,20,0,"Power:25.2MW","Powerplant Class 6 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size7_class1", new ShipModule(128064058,80,0,"Power:20MW","Powerplant Class 7 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size7_class2", new ShipModule(128064059,32,0,"Power:22.5MW","Powerplant Class 7 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size7_class3", new ShipModule(128064060,40,0,"Power:25MW","Powerplant Class 7 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size7_class4", new ShipModule(128064061,64,0,"Power:27.5MW","Powerplant Class 7 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size7_class5", new ShipModule(128064062,40,0,"Power:30MW","Powerplant Class 7 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size8_class1", new ShipModule(128064063,160,0,"Power:24MW","Powerplant Class 8 Rating E",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size8_class2", new ShipModule(128064064,64,0,"Power:27MW","Powerplant Class 8 Rating D",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size8_class3", new ShipModule(128064065,80,0,"Power:30MW","Powerplant Class 8 Rating C",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size8_class4", new ShipModule(128064066,128,0,"Power:33MW","Powerplant Class 8 Rating B",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size8_class5", new ShipModule(128064067,80,0,"Power:36MW","Powerplant Class 8 Rating A",ShipModule.ModuleTypes.PowerPlant) },
            { "int_powerplant_size2_class1_free", new ShipModule(128666635,2.5,0,"Power:6.4MW","Powerplant Class 2 Rating E",ShipModule.ModuleTypes.PowerPlant) },

            // Pulse laser

            { "hpt_pulselaser_fixed_small", new ShipModule(128049381,2,0.39,"Damage:2.1, Range:3000m, ThermL:0.3","Pulse Laser Fixed Small",ShipModule.ModuleTypes.PulseLaser) },   // EDDI
            { "hpt_pulselaser_fixed_medium", new ShipModule(128049382,4,0.6,"Damage:3.5, Range:3000m, ThermL:0.6","Pulse Laser Fixed Medium",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_fixed_large", new ShipModule(128049383,8,0.9,"Damage:6, Range:3000m, ThermL:1","Pulse Laser Fixed Large",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_fixed_huge", new ShipModule(128049384,16,1.33,"Damage:10.2, Range:3000m, ThermL:1.6","Pulse Laser Fixed Huge",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_small", new ShipModule(128049385,2,0.39,"Damage:1.6, Range:3000m, ThermL:0.3","Pulse Laser Gimbal Small",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_medium", new ShipModule(128049386,4,0.6,"Damage:2.7, Range:3000m, ThermL:0.5","Pulse Laser Gimbal Medium",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_large", new ShipModule(128049387,8,0.92,"Damage:4.6, Range:3000m, ThermL:0.9","Pulse Laser Gimbal Large",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_turret_small", new ShipModule(128049388,2,0.38,"Damage:1.2, Range:3000m, ThermL:0.2","Pulse Laser Turret Small",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_turret_medium", new ShipModule(128049389,4,0.58,"Damage:2.1, Range:3000m, ThermL:0.3","Pulse Laser Turret Medium",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_turret_large", new ShipModule(128049390,8,0.89,"Damage:3.5, Range:3000m, ThermL:0.6","Pulse Laser Turret Large",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_huge", new ShipModule(128681995,16,1.37,"Damage:7.8, Range:3000m, ThermL:1.6","Pulse Laser Gimbal Huge",ShipModule.ModuleTypes.PulseLaser) },

            { "hpt_pulselaser_fixed_smallfree", new ShipModule(128049673,1,0.4,null,"Pulse Laser Fixed Small Free",ShipModule.ModuleTypes.PulseLaser) },                        // EDDI
            { "hpt_pulselaser_fixed_medium_disruptor", new ShipModule(128671342,4,0.7,"Damage:2.8, ThermL:1","Pulse Laser Fixed Medium Disruptor",ShipModule.ModuleTypes.PulseDisruptorLaser) },

            // Pulse wave Scanner

            { "hpt_mrascanner_size0_class1", new ShipModule(128915718,1.3,0.2,null,"Pulse Wave scanner Size 0 Rating E",ShipModule.ModuleTypes.PulseWaveAnalyser) },    // EDDI
            { "hpt_mrascanner_size0_class2", new ShipModule(128915719,1.3,0.4,null,"Pulse Wave sscanner Size 0 Rating D",ShipModule.ModuleTypes.PulseWaveAnalyser) },
            { "hpt_mrascanner_size0_class3", new ShipModule(128915720,1.3,0.8,null,"Pulse Wave scanner Size 0 Rating C",ShipModule.ModuleTypes.PulseWaveAnalyser) },
            { "hpt_mrascanner_size0_class4", new ShipModule(128915721,1.3,1.6,null,"Pulse Wave scanner Size 0 Rating B",ShipModule.ModuleTypes.PulseWaveAnalyser) },
            { "hpt_mrascanner_size0_class5", new ShipModule(128915722,1.3,3.2,null,"Pulse Wave scanner Rating A",ShipModule.ModuleTypes.PulseWaveAnalyser) },

            // Rail guns

            { "hpt_railgun_fixed_small", new ShipModule(128049488,2,1.15,"Ammo:80/1, Damage:23.3, Range:3000m, Reload:1s, ThermL:12","Railgun Fixed Small",ShipModule.ModuleTypes.RailGun) },   // EDDI
            { "hpt_railgun_fixed_medium", new ShipModule(128049489,4,1.63,"Ammo:80/1, Damage:41.5, Range:3000m, Reload:1s, ThermL:20","Railgun Fixed Medium",ShipModule.ModuleTypes.RailGun) },
            { "hpt_railgun_fixed_medium_burst", new ShipModule(128671341,4,1.63,"Ammo:240/3, Damage:15, Range:3000m, Reload:1s, ThermL:11","Railgun Fixed Medium Burst",ShipModule.ModuleTypes.ImperialHammerRailGun) },

            // Refineries

            { "int_refinery_size1_class1", new ShipModule(128666684,0,0.14,null,"Refinery Class 1 Rating E",ShipModule.ModuleTypes.Refinery) }, // EDDI
            { "int_refinery_size2_class1", new ShipModule(128666685,0,0.17,null,"Refinery Class 2 Rating E",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size3_class1", new ShipModule(128666686,0,0.2,null,"Refinery Class 3 Rating E",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size4_class1", new ShipModule(128666687,0,0.25,null,"Refinery Class 4 Rating E",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size1_class2", new ShipModule(128666688,0,0.18,null,"Refinery Class 1 Rating D",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size2_class2", new ShipModule(128666689,0,0.22,null,"Refinery Class 2 Rating D",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size3_class2", new ShipModule(128666690,0,0.27,null,"Refinery Class 3 Rating D",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size4_class2", new ShipModule(128666691,0,0.33,null,"Refinery Class 4 Rating D",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size1_class3", new ShipModule(128666692,0,0.23,null,"Refinery Class 1 Rating C",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size2_class3", new ShipModule(128666693,0,0.28,null,"Refinery Class 2 Rating C",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size3_class3", new ShipModule(128666694,0,0.34,null,"Refinery Class 3 Rating C",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size4_class3", new ShipModule(128666695,0,0.41,null,"Refinery Class 4 Rating C",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size1_class4", new ShipModule(128666696,0,0.28,null,"Refinery Class 1 Rating B",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size2_class4", new ShipModule(128666697,0,0.34,null,"Refinery Class 2 Rating B",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size3_class4", new ShipModule(128666698,0,0.41,null,"Refinery Class 3 Rating B",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size4_class4", new ShipModule(128666699,0,0.49,null,"Refinery Class 4 Rating B",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size1_class5", new ShipModule(128666700,0,0.32,null,"Refinery Class 1 Rating A",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size2_class5", new ShipModule(128666701,0,0.39,null,"Refinery Class 2 Rating A",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size3_class5", new ShipModule(128666702,0,0.48,null,"Refinery Class 3 Rating A",ShipModule.ModuleTypes.Refinery) },
            { "int_refinery_size4_class5", new ShipModule(128666703,0,0.57,null,"Refinery Class 4 Rating A",ShipModule.ModuleTypes.Refinery) },

            // Sensors

            { "int_sensors_size1_class1", new ShipModule(128064218,1.3,0.16,"Range:4km","Sensors Class 1 Rating E",ShipModule.ModuleTypes.Sensors) },   // EDDI
            { "int_sensors_size1_class2", new ShipModule(128064219,0.5,0.18,"Range:4.5km","Sensors Class 1 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size1_class3", new ShipModule(128064220,1.3,0.2,"Range:5km","Sensors Class 1 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size1_class4", new ShipModule(128064221,2,0.33,"Range:5.5km","Sensors Class 1 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size1_class5", new ShipModule(128064222,1.3,0.6,"Range:6km","Sensors Class 1 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size2_class1", new ShipModule(128064223,2.5,0.18,"Range:4.2km","Sensors Class 2 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size2_class2", new ShipModule(128064224,1,0.21,"Range:4.7km","Sensors Class 2 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size2_class3", new ShipModule(128064225,2.5,0.23,"Range:5.2km","Sensors Class 2 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size2_class4", new ShipModule(128064226,4,0.38,"Range:5.7km","Sensors Class 2 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size2_class5", new ShipModule(128064227,2.5,0.69,"Range:6.2km","Sensors Class 2 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size3_class1", new ShipModule(128064228,5,0.22,"Range:4.3km","Sensors Class 3 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size3_class2", new ShipModule(128064229,2,0.25,"Range:4.9km","Sensors Class 3 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size3_class3", new ShipModule(128064230,5,0.28,"Range:5.4km","Sensors Class 3 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size3_class4", new ShipModule(128064231,8,0.46,"Range:5.9km","Sensors Class 3 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size3_class5", new ShipModule(128064232,5,0.84,"Range:6.5km","Sensors Class 3 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size4_class1", new ShipModule(128064233,10,0.27,"Range:4.5km","Sensors Class 4 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size4_class2", new ShipModule(128064234,4,0.31,"Range:5km","Sensors Class 4 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size4_class3", new ShipModule(128064235,10,0.34,"Range:5.6km","Sensors Class 4 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size4_class4", new ShipModule(128064236,16,0.56,"Range:6.2km","Sensors Class 4 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size4_class5", new ShipModule(128064237,10,1.02,"Range:6.7km","Sensors Class 4 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size5_class1", new ShipModule(128064238,20,0.33,"Range:4.6km","Sensors Class 5 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size5_class2", new ShipModule(128064239,8,0.37,"Range:5.2km","Sensors Class 5 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size5_class3", new ShipModule(128064240,20,0.41,"Range:5.8km","Sensors Class 5 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size5_class4", new ShipModule(128064241,32,0.68,"Range:6.4km","Sensors Class 5 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size5_class5", new ShipModule(128064242,20,1.23,"Range:7km","Sensors Class 5 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size6_class1", new ShipModule(128064243,40,0.4,"Range:4.8km","Sensors Class 6 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size6_class2", new ShipModule(128064244,16,0.45,"Range:5.4km","Sensors Class 6 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size6_class3", new ShipModule(128064245,40,0.5,"Range:6km","Sensors Class 6 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size6_class4", new ShipModule(128064246,64,0.83,"Range:6.6km","Sensors Class 6 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size6_class5", new ShipModule(128064247,40,1.5,"Range:7.2km","Sensors Class 6 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size7_class1", new ShipModule(128064248,80,0.47,"Range:5km","Sensors Class 7 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size7_class2", new ShipModule(128064249,32,0.53,"Range:5.6km","Sensors Class 7 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size7_class3", new ShipModule(128064250,80,0.59,"Range:6.2km","Sensors Class 7 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size7_class4", new ShipModule(128064251,128,0.97,"Range:6.8km","Sensors Class 7 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size7_class5", new ShipModule(128064252,80,1.77,"Range:7.4km","Sensors Class 7 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size8_class1", new ShipModule(128064253,160,0.55,"Range:5.1km","Sensors Class 8 Rating E",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size8_class2", new ShipModule(128064254,64,0.62,"Range:5.8km","Sensors Class 8 Rating D",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size8_class3", new ShipModule(128064255,160,0.69,"Range:6.4km","Sensors Class 8 Rating C",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size8_class4", new ShipModule(128064256,256,1.14,"Range:7km","Sensors Class 8 Rating B",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size8_class5", new ShipModule(128064257,160,2.07,"Range:7.7km","Sensors Class 8 Rating A",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_size1_class1_free", new ShipModule(128666640,1.3,0.16,"Range:4km","Sensors Class 1 Rating E",ShipModule.ModuleTypes.Sensors) },

            // Shield Boosters

            { "hpt_shieldbooster_size0_class1", new ShipModule(128668532,0.5,0.2,"Boost:4.0%, Explosive:0%, Kinetic:0%, Thermal:0%","Shield Booster Rating E",ShipModule.ModuleTypes.ShieldBooster) },  // EDDI
            { "hpt_shieldbooster_size0_class2", new ShipModule(128668533,1,0.5,"Boost:8.0%, Explosive:0%, Kinetic:0%, Thermal:0%","Shield Booster Rating D",ShipModule.ModuleTypes.ShieldBooster) },
            { "hpt_shieldbooster_size0_class3", new ShipModule(128668534,2,0.7,"Boost:12.0%, Explosive:0%, Kinetic:0%, Thermal:0%","Shield Booster Rating C",ShipModule.ModuleTypes.ShieldBooster) },
            { "hpt_shieldbooster_size0_class4", new ShipModule(128668535,3,1,"Boost:16.0%, Explosive:0%, Kinetic:0%, Thermal:0%","Shield Booster Rating B",ShipModule.ModuleTypes.ShieldBooster) },
            { "hpt_shieldbooster_size0_class5", new ShipModule(128668536,3.5,1.2,"Boost:20.0%, Explosive:0%, Kinetic:0%, Thermal:0%","Shield Booster Rating A",ShipModule.ModuleTypes.ShieldBooster) },

            // cell banks

            { "int_shieldcellbank_size1_class1", new ShipModule(128064298,1.3,0.41,"Ammo:3/1, ThermL:170","Shield Cell Bank Class 1 Rating E",ShipModule.ModuleTypes.ShieldCellBank) }, // EDDI
            { "int_shieldcellbank_size1_class2", new ShipModule(128064299,0.5,0.55,"Ammo:0/1, ThermL:170","Shield Cell Bank Class 1 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size1_class3", new ShipModule(128064300,1.3,0.69,"Ammo:2/1, ThermL:170","Shield Cell Bank Class 1 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size1_class4", new ShipModule(128064301,2,0.83,"Ammo:3/1, ThermL:170","Shield Cell Bank Class 1 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size1_class5", new ShipModule(128064302,1.3,0.97,"Ammo:2/1, ThermL:170","Shield Cell Bank Class 1 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size2_class1", new ShipModule(128064303,2.5,0.5,"Ammo:4/1, ThermL:240","Shield Cell Bank Class 2 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size2_class2", new ShipModule(128064304,1,0.67,"Ammo:2/1, ThermL:240","Shield Cell Bank Class 2 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size2_class3", new ShipModule(128064305,2.5,0.84,"Ammo:3/1, ThermL:240","Shield Cell Bank Class 2 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size2_class4", new ShipModule(128064306,4,1.01,"Ammo:4/1, ThermL:240","Shield Cell Bank Class 2 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size2_class5", new ShipModule(128064307,2.5,1.18,"Ammo:3/1, ThermL:240","Shield Cell Bank Class 2 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size3_class1", new ShipModule(128064308,5,0.61,"Ammo:4/1, ThermL:340","Shield Cell Bank Class 3 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size3_class2", new ShipModule(128064309,2,0.82,"Ammo:2/1, ThermL:340","Shield Cell Bank Class 3 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size3_class3", new ShipModule(128064310,5,1.02,"Ammo:3/1, ThermL:340","Shield Cell Bank Class 3 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size3_class4", new ShipModule(128064311,8,1.22,"Ammo:4/1, ThermL:340","Shield Cell Bank Class 3 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size3_class5", new ShipModule(128064312,5,1.43,"Ammo:3/1, ThermL:340","Shield Cell Bank Class 3 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size4_class1", new ShipModule(128064313,10,0.74,"Ammo:4/1, ThermL:410","Shield Cell Bank Class 4 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size4_class2", new ShipModule(128064314,4,0.98,"Ammo:2/1, ThermL:410","Shield Cell Bank Class 4 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size4_class3", new ShipModule(128064315,10,1.23,"Ammo:3/1, ThermL:410","Shield Cell Bank Class 4 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size4_class4", new ShipModule(128064316,16,1.48,"Ammo:4/1, ThermL:410","Shield Cell Bank Class 4 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size4_class5", new ShipModule(128064317,10,1.72,"Ammo:3/1, ThermL:410","Shield Cell Bank Class 4 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size5_class1", new ShipModule(128064318,20,0.9,"Ammo:4/1, ThermL:540","Shield Cell Bank Class 5 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size5_class2", new ShipModule(128064319,8,1.2,"Ammo:2/1, ThermL:540","Shield Cell Bank Class 5 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size5_class3", new ShipModule(128064320,20,1.5,"Ammo:3/1, ThermL:540","Shield Cell Bank Class 5 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size5_class4", new ShipModule(128064321,32,1.8,"Ammo:4/1, ThermL:540","Shield Cell Bank Class 5 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size5_class5", new ShipModule(128064322,20,2.1,"Ammo:3/1, ThermL:540","Shield Cell Bank Class 5 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size6_class1", new ShipModule(128064323,40,1.06,"Ammo:5/1, ThermL:640","Shield Cell Bank Class 6 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size6_class2", new ShipModule(128064324,16,1.42,"Ammo:3/1, ThermL:640","Shield Cell Bank Class 6 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size6_class3", new ShipModule(128064325,40,1.77,"Ammo:4/1, ThermL:640","Shield Cell Bank Class 6 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size6_class4", new ShipModule(128064326,64,2.12,"Ammo:5/1, ThermL:640","Shield Cell Bank Class 6 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size6_class5", new ShipModule(128064327,40,2.48,"Ammo:4/1, ThermL:640","Shield Cell Bank Class 6 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size7_class1", new ShipModule(128064328,80,1.24,"Ammo:5/1, ThermL:720","Shield Cell Bank Class 7 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size7_class2", new ShipModule(128064329,32,1.66,"Ammo:3/1, ThermL:720","Shield Cell Bank Class 7 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size7_class3", new ShipModule(128064330,80,2.07,"Ammo:4/1, ThermL:720","Shield Cell Bank Class 7 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size7_class4", new ShipModule(128064331,128,2.48,"Ammo:5/1, ThermL:720","Shield Cell Bank Class 7 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size7_class5", new ShipModule(128064332,80,2.9,"Ammo:4/1, ThermL:720","Shield Cell Bank Class 7 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size8_class1", new ShipModule(128064333,160,1.44,"Ammo:5/1, ThermL:800","Shield Cell Bank Class 8 Rating E",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size8_class2", new ShipModule(128064334,64,1.92,"Ammo:3/1, ThermL:800","Shield Cell Bank Class 8 Rating D",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size8_class3", new ShipModule(128064335,160,2.4,"Ammo:4/1, ThermL:800","Shield Cell Bank Class 8 Rating C",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size8_class4", new ShipModule(128064336,256,2.88,"Ammo:5/1, ThermL:800","Shield Cell Bank Class 8 Rating B",ShipModule.ModuleTypes.ShieldCellBank) },
            { "int_shieldcellbank_size8_class5", new ShipModule(128064337,160,3.36,"Ammo:4/1, ThermL:800","Shield Cell Bank Class 8 Rating A",ShipModule.ModuleTypes.ShieldCellBank) },

            // Shield Generators

            { "int_shieldgenerator_size1_class1", new ShipModule(128064258,1.3,0.72,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) }, // EDDI
            { "int_shieldgenerator_size1_class2", new ShipModule(128064259,0.5,0.96,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size1_class3", new ShipModule(128064260,1.3,1.2,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size1_class5", new ShipModule(128064262,1.3,1.68,"OptMass:25t, MaxMass:63t, MinMass:13t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 1 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size2_class1", new ShipModule(128064263,2.5,0.9,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size2_class2", new ShipModule(128064264,1,1.2,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size2_class3", new ShipModule(128064265,2.5,1.5,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size2_class4", new ShipModule(128064266,4,1.8,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size2_class5", new ShipModule(128064267,2.5,2.1,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size3_class1", new ShipModule(128064268,5,1.08,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size3_class2", new ShipModule(128064269,2,1.44,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size3_class3", new ShipModule(128064270,5,1.8,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size3_class4", new ShipModule(128064271,8,2.16,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size3_class5", new ShipModule(128064272,5,2.52,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size4_class1", new ShipModule(128064273,10,1.32,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size4_class2", new ShipModule(128064274,4,1.76,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size4_class3", new ShipModule(128064275,10,2.2,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size4_class4", new ShipModule(128064276,16,2.64,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size4_class5", new ShipModule(128064277,10,3.08,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size5_class1", new ShipModule(128064278,20,1.56,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size5_class2", new ShipModule(128064279,8,2.08,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size5_class3", new ShipModule(128064280,20,2.6,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size5_class4", new ShipModule(128064281,32,3.12,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size5_class5", new ShipModule(128064282,20,3.64,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size6_class1", new ShipModule(128064283,40,1.86,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size6_class2", new ShipModule(128064284,16,2.48,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size6_class3", new ShipModule(128064285,40,3.1,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size6_class4", new ShipModule(128064286,64,3.72,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size6_class5", new ShipModule(128064287,40,4.34,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size7_class1", new ShipModule(128064288,80,2.1,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size7_class2", new ShipModule(128064289,32,2.8,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size7_class3", new ShipModule(128064290,80,3.5,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size7_class4", new ShipModule(128064291,128,4.2,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size7_class5", new ShipModule(128064292,80,4.9,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size8_class1", new ShipModule(128064293,160,2.4,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size8_class2", new ShipModule(128064294,64,3.2,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating D",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size8_class3", new ShipModule(128064295,160,4,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating C",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size8_class4", new ShipModule(128064296,256,4.8,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating B",ShipModule.ModuleTypes.ShieldGenerator) },
            { "int_shieldgenerator_size8_class5", new ShipModule(128064297,160,5.6,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating A",ShipModule.ModuleTypes.ShieldGenerator) },

            { "int_shieldgenerator_size2_class1_free", new ShipModule(128666641,2.5,0.9,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating E",ShipModule.ModuleTypes.ShieldGenerator) }, // EDDI

            { "int_shieldgenerator_size1_class5_strong", new ShipModule(128671323,2.6,2.52,"OptMass:25t, MaxMass:63t, MinMass:13t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 1 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) }, // EDDI
            { "int_shieldgenerator_size2_class5_strong", new ShipModule(128671324,5,3.15,"OptMass:55t, MaxMass:138t, MinMass:23t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size3_class5_strong", new ShipModule(128671325,10,3.78,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size4_class5_strong", new ShipModule(128671326,20,4.62,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size5_class5_strong", new ShipModule(128671327,40,5.46,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size6_class5_strong", new ShipModule(128671328,80,6.51,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size7_class5_strong", new ShipModule(128671329,160,7.35,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },
            { "int_shieldgenerator_size8_class5_strong", new ShipModule(128671330,320,8.4,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating A Strong",ShipModule.ModuleTypes.PrismaticShieldGenerator) },

            { "int_shieldgenerator_size1_class3_fast", new ShipModule(128671331,1.3,1.2,"OptMass:25t, MaxMass:63t, MinMass:13t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 1 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) }, // EDDI
            { "int_shieldgenerator_size2_class3_fast", new ShipModule(128671332,2.5,1.5,"OptMass:55t, MaxMass:138t, MinMass:28t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 2 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size3_class3_fast", new ShipModule(128671333,5,1.8,"OptMass:165t, MaxMass:413t, MinMass:83t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 3 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size4_class3_fast", new ShipModule(128671334,10,2.2,"OptMass:285t, MaxMass:713t, MinMass:143t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 4 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size5_class3_fast", new ShipModule(128671335,20,2.6,"OptMass:405t, MaxMass:1013t, MinMass:203t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 5 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size6_class3_fast", new ShipModule(128671336,40,3.1,"OptMass:540t, MaxMass:1350t, MinMass:270t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 6 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size7_class3_fast", new ShipModule(128671337,80,3.5,"OptMass:1060t, MaxMass:2650t, MinMass:530t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 7 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },
            { "int_shieldgenerator_size8_class3_fast", new ShipModule(128671338,160,4,"OptMass:1800t, MaxMass:4500t, MinMass:900t, Explosive:50%, Kinetic:40%, Thermal:-20%","Shield Generator Class 8 Rating C Fast",ShipModule.ModuleTypes.Bi_WeaveShieldGenerator) },

            // shield shutdown neutraliser

            { "hpt_antiunknownshutdown_tiny", new ShipModule(128771884,1.3,0.2,"Range:3000m","Shutdown Field Neutraliser",ShipModule.ModuleTypes.ShutdownFieldNeutraliser) },   // EDDI
            { "hpt_antiunknownshutdown_tiny_v2", new ShipModule(129022663,1.3,0.2,"Range:3000m","Shutdown Field Neutraliser V2",ShipModule.ModuleTypes.ShutdownFieldNeutraliser) },   // EDDI

            // weapon stabliser
            { "int_expmodulestabiliser_size3_class3", new ShipModule(129019260,8,1.5,"","Exp Module Weapon Stabiliser 3F",ShipModule.ModuleTypes.ExperimentalWeaponStabiliser) }, //EDDI
            { "int_expmodulestabiliser_size5_class3", new ShipModule(129019261,20,3,"","Exp Module Weapon Stabiliser 5F",ShipModule.ModuleTypes.ExperimentalWeaponStabiliser) },

            // supercruise
            { "int_supercruiseassist", new ShipModule(128932273,0,0.3,null,"Supercruise Assist",ShipModule.ModuleTypes.SupercruiseAssist) }, // EDDI

            // stellar scanners

            { "int_stellarbodydiscoveryscanner_standard_free", new ShipModule(128666642,2,0,"Range:500ls","Stellar Body Discovery Scanner Standard",ShipModule.ModuleTypes.DiscoveryScanner) },         //EDDI, not spansh classs
            { "int_stellarbodydiscoveryscanner_standard", new ShipModule(128662535,2,0,"Range:500ls","Stellar Body Discovery Scanner Standard",ShipModule.ModuleTypes.DiscoveryScanner) },
            { "int_stellarbodydiscoveryscanner_intermediate", new ShipModule(128663560,2,0,"Range:1000ls","Stellar Body Discovery Scanner Intermediate",ShipModule.ModuleTypes.DiscoveryScanner) },
            { "int_stellarbodydiscoveryscanner_advanced", new ShipModule(128663561,2,0,null,"Stellar Body Discovery Scanner Advanced",ShipModule.ModuleTypes.DiscoveryScanner) },

            // thrusters

            { "int_engine_size2_class1", new ShipModule(128064068,2.5,2,"OptMass:48t, MaxMass:72t, MinMass:24t","Thrusters Class 2 Rating E",ShipModule.ModuleTypes.Thrusters) },       // EDDI
            { "int_engine_size2_class2", new ShipModule(128064069,1,2.25,"OptMass:54t, MaxMass:81t, MinMass:27t","Thrusters Class 2 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size2_class3", new ShipModule(128064070,2.5,2.5,"OptMass:60t, MaxMass:90t, MinMass:30t","Thrusters Class 2 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size2_class4", new ShipModule(128064071,4,2.75,"OptMass:66t, MaxMass:99t, MinMass:33t","Thrusters Class 2 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size2_class5", new ShipModule(128064072,2.5,3,"OptMass:72t, MaxMass:108t, MinMass:36t","Thrusters Class 2 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size3_class1", new ShipModule(128064073,5,2.48,"OptMass:80t, MaxMass:120t, MinMass:40t","Thrusters Class 3 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size3_class2", new ShipModule(128064074,2,2.79,"OptMass:90t, MaxMass:135t, MinMass:45t","Thrusters Class 3 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size3_class3", new ShipModule(128064075,5,3.1,"OptMass:100t, MaxMass:150t, MinMass:50t","Thrusters Class 3 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size3_class4", new ShipModule(128064076,8,3.41,"OptMass:110t, MaxMass:165t, MinMass:55t","Thrusters Class 3 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size3_class5", new ShipModule(128064077,5,3.72,"OptMass:120t, MaxMass:180t, MinMass:60t","Thrusters Class 3 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size4_class1", new ShipModule(128064078,10,3.28,"OptMass:280t, MaxMass:420t, MinMass:140t","Thrusters Class 4 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size4_class2", new ShipModule(128064079,4,3.69,"OptMass:315t, MaxMass:472t, MinMass:158t","Thrusters Class 4 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size4_class3", new ShipModule(128064080,10,4.1,"OptMass:350t, MaxMass:525t, MinMass:175t","Thrusters Class 4 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size4_class4", new ShipModule(128064081,16,4.51,"OptMass:385t, MaxMass:578t, MinMass:192t","Thrusters Class 4 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size4_class5", new ShipModule(128064082,10,4.92,"OptMass:420t, MaxMass:630t, MinMass:210t","Thrusters Class 4 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size5_class1", new ShipModule(128064083,20,4.08,"OptMass:560t, MaxMass:840t, MinMass:280t","Thrusters Class 5 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size5_class2", new ShipModule(128064084,8,4.59,"OptMass:630t, MaxMass:945t, MinMass:315t","Thrusters Class 5 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size5_class3", new ShipModule(128064085,20,5.1,"OptMass:700t, MaxMass:1050t, MinMass:350t","Thrusters Class 5 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size5_class4", new ShipModule(128064086,32,5.61,"OptMass:770t, MaxMass:1155t, MinMass:385t","Thrusters Class 5 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size5_class5", new ShipModule(128064087,20,6.12,"OptMass:840t, MaxMass:1260t, MinMass:420t","Thrusters Class 5 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size6_class1", new ShipModule(128064088,40,5.04,"OptMass:960t, MaxMass:1440t, MinMass:480t","Thrusters Class 6 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size6_class2", new ShipModule(128064089,16,5.67,"OptMass:1080t, MaxMass:1620t, MinMass:540t","Thrusters Class 6 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size6_class3", new ShipModule(128064090,40,6.3,"OptMass:1200t, MaxMass:1800t, MinMass:600t","Thrusters Class 6 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size6_class4", new ShipModule(128064091,64,6.93,"OptMass:1320t, MaxMass:1980t, MinMass:660t","Thrusters Class 6 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size6_class5", new ShipModule(128064092,40,7.56,"OptMass:1440t, MaxMass:2160t, MinMass:720t","Thrusters Class 6 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size7_class1", new ShipModule(128064093,80,6.08,"OptMass:1440t, MaxMass:2160t, MinMass:720t","Thrusters Class 7 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size7_class2", new ShipModule(128064094,32,6.84,"OptMass:1620t, MaxMass:2430t, MinMass:810t","Thrusters Class 7 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size7_class3", new ShipModule(128064095,80,7.6,"OptMass:1800t, MaxMass:2700t, MinMass:900t","Thrusters Class 7 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size7_class4", new ShipModule(128064096,128,8.36,"OptMass:1980t, MaxMass:2970t, MinMass:990t","Thrusters Class 7 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size7_class5", new ShipModule(128064097,80,9.12,"OptMass:2160t, MaxMass:3240t, MinMass:1080t","Thrusters Class 7 Rating A",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size8_class1", new ShipModule(128064098,160,7.2,"OptMass:2240t, MaxMass:3360t, MinMass:1120t","Thrusters Class 8 Rating E",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size8_class2", new ShipModule(128064099,64,8.1,"OptMass:2520t, MaxMass:3780t, MinMass:1260t","Thrusters Class 8 Rating D",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size8_class3", new ShipModule(128064100,160,9,"OptMass:2800t, MaxMass:4200t, MinMass:1400t","Thrusters Class 8 Rating C",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size8_class4", new ShipModule(128064101,256,9.9,"OptMass:3080t, MaxMass:4620t, MinMass:1540t","Thrusters Class 8 Rating B",ShipModule.ModuleTypes.Thrusters) },
            { "int_engine_size8_class5", new ShipModule(128064102,160,10.8,"OptMass:3360t, MaxMass:5040t, MinMass:1680t","Thrusters Class 8 Rating A",ShipModule.ModuleTypes.Thrusters) },

            { "int_engine_size2_class1_free", new ShipModule(128666636,2.5,2,"OptMass:48t, MaxMass:72t, MinMass:24t","Thrusters Class 2 Rating E",ShipModule.ModuleTypes.Thrusters) }, // EDDI

            { "int_engine_size3_class5_fast", new ShipModule(128682013,5,5,"OptMass:90t, MaxMass:200t, MinMass:70t","Thrusters Class 3 Rating A Fast",ShipModule.ModuleTypes.EnhancedPerformanceThrusters) },   // EDDI
            { "int_engine_size2_class5_fast", new ShipModule(128682014,2.5,4,"OptMass:60t, MaxMass:120t, MinMass:50t","Thrusters Class 2 Rating A Fast",ShipModule.ModuleTypes.EnhancedPerformanceThrusters) },

            // XENO Scanners

            { "hpt_xenoscanner_basic_tiny", new ShipModule(128793115,1.3,0.2,"Range:500m","Xeno Scanner",ShipModule.ModuleTypes.XenoScanner) },         // EDDI
            { "hpt_xenoscannermk2_basic_tiny", new ShipModule(128808878,1.3,0.8,"Range:2000m","Xeno Scanner MK 2",ShipModule.ModuleTypes.EnhancedXenoScanner) },
            { "hpt_xenoscanner_advanced_tiny", new ShipModule(129022952,1.3,0.8,"Range:2000m","Advanced Xeno Scanner",ShipModule.ModuleTypes.EnhancedXenoScanner) },

        };

        // non buyable

        public static Dictionary<string, ShipModule> othershipmodules = new Dictionary<string, ShipModule>
        {
            { "adder_cockpit", new ShipModule(999999913,0,0,null,"Adder Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "typex_3_cockpit", new ShipModule(999999945,0,0,null,"Alliance Challenger Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "typex_cockpit", new ShipModule(999999943,0,0,null,"Alliance Chieftain Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "anaconda_cockpit", new ShipModule(999999926,0,0,null,"Anaconda Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "asp_cockpit", new ShipModule(999999918,0,0,null,"Asp Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "asp_scout_cockpit", new ShipModule(999999934,0,0,null,"Asp Scout Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "belugaliner_cockpit", new ShipModule(999999938,0,0,null,"Beluga Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "cobramkiii_cockpit", new ShipModule(999999915,0,0,null,"Cobra Mk III Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "cobramkiv_cockpit", new ShipModule(999999937,0,0,null,"Cobra Mk IV Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "cutter_cockpit", new ShipModule(999999932,0,0,null,"Cutter Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "diamondbackxl_cockpit", new ShipModule(999999928,0,0,null,"Diamondback Explorer Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "diamondback_cockpit", new ShipModule(999999927,0,0,null,"Diamondback Scout Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "dolphin_cockpit", new ShipModule(999999939,0,0,null,"Dolphin Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "eagle_cockpit", new ShipModule(999999911,0,0,null,"Eagle Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "empire_courier_cockpit", new ShipModule(999999909,0,0,null,"Empire Courier Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "empire_eagle_cockpit", new ShipModule(999999929,0,0,null,"Empire Eagle Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "empire_fighter_cockpit", new ShipModule(899990000,0,0,null,"Empire Fighter Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "empire_trader_cockpit", new ShipModule(999999920,0,0,null,"Empire Trader Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "federation_corvette_cockpit", new ShipModule(999999933,0,0,null,"Federal Corvette Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "federation_dropship_mkii_cockpit", new ShipModule(999999930,0,0,null,"Federal Dropship Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "federation_dropship_cockpit", new ShipModule(999999921,0,0,null,"Federal Gunship Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "federation_gunship_cockpit", new ShipModule(999999931,0,0,null,"Federal Gunship Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "federation_fighter_cockpit", new ShipModule(899990001,0,0,null,"Federation Fighter Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "ferdelance_cockpit", new ShipModule(999999925,0,0,null,"Fer De Lance Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "hauler_cockpit", new ShipModule(999999912,0,0,null,"Hauler Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "independant_trader_cockpit", new ShipModule(999999936,0,0,null,"Independant Trader Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "independent_fighter_cockpit", new ShipModule(899990002,0,0,null,"Independent Fighter Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "krait_light_cockpit", new ShipModule(999999948,0,0,null,"Krait Light Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "krait_mkii_cockpit", new ShipModule(999999946,0,0,null,"Krait MkII Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "mamba_cockpit", new ShipModule(999999949,0,0,null,"Mamba Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "orca_cockpit", new ShipModule(999999922,0,0,null,"Orca Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "python_cockpit", new ShipModule(999999924,0,0,null,"Python Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "sidewinder_cockpit", new ShipModule(999999910,0,0,null,"Sidewinder Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "type6_cockpit", new ShipModule(999999916,0,0,null,"Type 6 Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "type7_cockpit", new ShipModule(999999917,0,0,null,"Type 7 Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "type9_cockpit", new ShipModule(999999923,0,0,null,"Type 9 Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "type9_military_cockpit", new ShipModule(999999942,0,0,null,"Type 9 Military Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "typex_2_cockpit", new ShipModule(999999950,0,0,null,"Typex 2 Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "viper_cockpit", new ShipModule(999999914,0,0,null,"Viper Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "viper_mkiv_cockpit", new ShipModule(999999935,0,0,null,"Viper Mk IV Cockpit", ShipModule.ModuleTypes.CockpitType) },
            { "vulture_cockpit", new ShipModule(999999919,0,0,null,"Vulture Cockpit", ShipModule.ModuleTypes.CockpitType) },

            { "int_codexscanner", new ShipModule(999999947,0,0,null,"Codex Scanner",ShipModule.ModuleTypes.Codex) },
            { "hpt_shipdatalinkscanner", new ShipModule(999999940,0,0,null,"Hpt Shipdatalinkscanner",ShipModule.ModuleTypes.DataLinkScanner) },

            { "int_passengercabin_size2_class0", new ShipModule(-1,2.5,0,"Prisoners:2","Prison Cell",ShipModule.ModuleTypes.PrisonCells) },
            { "int_passengercabin_size3_class0", new ShipModule(-1,5,0,"Prisoners:4","Prison Cell",ShipModule.ModuleTypes.PrisonCells) },
            { "int_passengercabin_size4_class0", new ShipModule(-1,10,0,"Prisoners:8","Prison Cell",ShipModule.ModuleTypes.PrisonCells) },
            { "int_passengercabin_size5_class0", new ShipModule(-1,20,0,"Prisoners:16","Prison Cell",ShipModule.ModuleTypes.PrisonCells) },
            { "int_passengercabin_size6_class0", new ShipModule(-1,40,0,"Prisoners:32","Prison Cell",ShipModule.ModuleTypes.PrisonCells) },

            { "hpt_cannon_turret_huge", new ShipModule(-1,1,0.9,null,"Cannon Turret Huge",ShipModule.ModuleTypes.Cannon) }, // seen in logs

            { "modularcargobaydoorfdl", new ShipModule(999999907,0,0,null,"FDL Cargo Bay Door", ShipModule.ModuleTypes.CargoBayDoorType) },
            { "modularcargobaydoor", new ShipModule(999999908,0,0,null,"Modular Cargo Bay Door", ShipModule.ModuleTypes.CargoBayDoorType) },

            { "hpt_cargoscanner_basic_tiny", new ShipModule(-1,0,0,null,"Cargo Scanner Basic Tiny",ShipModule.ModuleTypes.CargoScanner) },  // seen in logs

           // { "int_corrosionproofcargorack_size2_class1", new ShipModule(-1,0,0,null,"Corrosion Resistant Cargo Rack",ShipModule.ModuleTypes.CargoRack) },
           // { "hpt_plasmaburstcannon_fixed_medium", new ShipModule(-1,1,1.4,null,"Plasma Burst Cannon Fixed Medium","Plasma Accelerator") },      // no evidence
           // { "hpt_pulselaserstealth_fixed_small", new ShipModule(-1,1,0.2,null,"Pulse Laser Stealth Fixed Small",ShipModule.ModuleTypes.PulseLaser) },
            ///{ "int_shieldgenerator_size1_class4", new ShipModule(-1,2,1.44,null,"Shield Generator Class 1 Rating E",ShipModule.ModuleTypes.ShieldGenerator) },
        };

        #endregion

        #region Fighters

        public static Dictionary<string, ShipModule> fightermodules = new Dictionary<string, ShipModule>
        {
            { "hpt_guardiangauss_fixed_gdn_fighter", new ShipModule(899990050,1,1,null,"Guardian Gauss Fixed GDN Fighter",ShipModule.ModuleTypes.FighterWeapon) },
            { "hpt_guardianplasma_fixed_gdn_fighter", new ShipModule(899990050,1,1,null,"Guardian Plasma Fixed GDN Fighter",ShipModule.ModuleTypes.FighterWeapon) },
            { "hpt_guardianshard_fixed_gdn_fighter", new ShipModule(899990050,1,1,null,"Guardian Shard Fixed GDN Fighter",ShipModule.ModuleTypes.FighterWeapon) },

            { "empire_fighter_armour_standard", new ShipModule(899990059,0,0,null,"Empire Fighter Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },
            { "federation_fighter_armour_standard", new ShipModule(899990060,0,0,null,"Federation Fighter Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },
            { "independent_fighter_armour_standard", new ShipModule(899990070,0,0,null,"Independent Fighter Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },
            { "gdn_hybrid_fighter_v1_armour_standard", new ShipModule(899990060,0,0,null,"GDN Hybrid Fighter V 1 Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },
            { "gdn_hybrid_fighter_v2_armour_standard", new ShipModule(899990060,0,0,null,"GDN Hybrid Fighter V 2 Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },
            { "gdn_hybrid_fighter_v3_armour_standard", new ShipModule(899990060,0,0,null,"GDN Hybrid Fighter V 3 Armour Standard",ShipModule.ModuleTypes.LightweightAlloy) },

            { "hpt_beamlaser_fixed_empire_fighter", new ShipModule(899990018,0,1,null,"Beam Laser Fixed Empire Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_fixed_fed_fighter", new ShipModule(899990019,0,1,null,"Beam Laser Fixed Federation Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_fixed_indie_fighter", new ShipModule(899990020,0,1,null,"Beam Laser Fixed Indie Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_empire_fighter", new ShipModule(899990023,0,1,null,"Beam Laser Gimbal Empire Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_fed_fighter", new ShipModule(899990024,0,1,null,"Beam Laser Gimbal Federation Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_beamlaser_gimbal_indie_fighter", new ShipModule(899990025,0,1,null,"Beam Laser Gimbal Indie Fighter",ShipModule.ModuleTypes.BeamLaser) },
            { "hpt_plasmarepeater_fixed_empire_fighter", new ShipModule(899990026,0,1,null,"Plasma Repeater Fixed Empire Fighter",ShipModule.ModuleTypes.PlasmaAccelerator) },
            { "hpt_plasmarepeater_fixed_fed_fighter", new ShipModule(899990027,0,1,null,"Plasma Repeater Fixed Fed Fighter",ShipModule.ModuleTypes.PlasmaAccelerator) },
            { "hpt_plasmarepeater_fixed_indie_fighter", new ShipModule(899990028,0,1,null,"Plasma Repeater Fixed Indie Fighter",ShipModule.ModuleTypes.PlasmaAccelerator) },
            { "hpt_pulselaser_fixed_empire_fighter", new ShipModule(899990029,0,1,null,"Pulse Laser Fixed Empire Fighter",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_fixed_fed_fighter", new ShipModule(899990030,0,1,null,"Pulse Laser Fixed Federation Fighter",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_fixed_indie_fighter", new ShipModule(899990031,0,1,null,"Pulse Laser Fixed Indie Fighter",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_empire_fighter", new ShipModule(899990032,0,1,null,"Pulse Laser Gimbal Empire Fighter",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_fed_fighter", new ShipModule(899990033,0,1,null,"Pulse Laser Gimbal Federation Fighter",ShipModule.ModuleTypes.PulseLaser) },
            { "hpt_pulselaser_gimbal_indie_fighter", new ShipModule(899990034,0,1,null,"Pulse Laser Gimbal Indie Fighter",ShipModule.ModuleTypes.PulseLaser) },

            { "int_engine_fighter_class1", new ShipModule(-1,1,1,null,"Fighter Engine Class 1",ShipModule.ModuleTypes.Thrusters) },

            { "gdn_hybrid_fighter_v1_cockpit", new ShipModule(899990101,0,0,null,"GDN Hybrid Fighter V 1 Cockpit",ShipModule.ModuleTypes.CockpitType) },
            { "gdn_hybrid_fighter_v2_cockpit", new ShipModule(899990102,0,0,null,"GDN Hybrid Fighter V 2 Cockpit",ShipModule.ModuleTypes.CockpitType) },
            { "gdn_hybrid_fighter_v3_cockpit", new ShipModule(899990103,0,0,null,"GDN Hybrid Fighter V 3 Cockpit",ShipModule.ModuleTypes.CockpitType) },

            { "hpt_atmulticannon_fixed_indie_fighter", new ShipModule(899990040,0,1,null,"AX Multicannon Fixed Indie Fighter",ShipModule.ModuleTypes.AXMulti_Cannon) },
            { "hpt_multicannon_fixed_empire_fighter", new ShipModule(899990050,0,1,null,"Multicannon Fixed Empire Fighter",ShipModule.ModuleTypes.Multi_Cannon) },
            { "hpt_multicannon_fixed_fed_fighter", new ShipModule(899990051,0,1,null,"Multicannon Fixed Fed Fighter",ShipModule.ModuleTypes.Multi_Cannon) },
            { "hpt_multicannon_fixed_indie_fighter", new ShipModule(899990052,0,1,null,"Multicannon Fixed Indie Fighter",ShipModule.ModuleTypes.Multi_Cannon) },

            { "int_powerdistributor_fighter_class1", new ShipModule(-1,0,0,null,"Int Powerdistributor Fighter Class 1",ShipModule.ModuleTypes.PowerDistributor) },

            { "int_powerplant_fighter_class1", new ShipModule(-1,0,0,null,"Int Powerplant Fighter Class 1",ShipModule.ModuleTypes.PowerPlant) },

            { "int_sensors_fighter_class1", new ShipModule(-1,0,0,null,"Int Sensors Fighter Class 1",ShipModule.ModuleTypes.Sensors) },
            { "int_shieldgenerator_fighter_class1", new ShipModule(899990080,0,0,null,"Shield Generator Fighter Class 1",ShipModule.ModuleTypes.ShieldGenerator) },
            { "ext_emitter_guardian", new ShipModule(899990190,0,0,null,"Ext Emitter Guardian",ShipModule.ModuleTypes.Sensors) },
            { "ext_emitter_standard", new ShipModule(899990090,0,0,null,"Ext Emitter Standard",ShipModule.ModuleTypes.Sensors) },

        };

        #endregion

        #region SRV

        public static Dictionary<string, ShipModule> srvmodules = new Dictionary<string, ShipModule>
        {
            { "buggycargobaydoor", new ShipModule(-1,0,0,null,"SRV Cargo Bay Door",ShipModule.ModuleTypes.CargoBayDoorType) },
            { "int_fueltank_size0_class3", new ShipModule(-1,0,0,null,"SRV Scarab Fuel Tank",ShipModule.ModuleTypes.FuelTank) },
            { "vehicle_scorpion_missilerack_lockon", new ShipModule(-1,0,0,null,"SRV Scorpion Missile Rack",ShipModule.ModuleTypes.MissileRack) },
            { "int_powerdistributor_size0_class1", new ShipModule(-1,0,0,null,"SRV Scarab Power Distributor",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerplant_size0_class1", new ShipModule(-1,0,0,null,"SRV Scarab Powerplant",ShipModule.ModuleTypes.PowerPlant) },
            { "vehicle_plasmaminigun_turretgun", new ShipModule(-1,0,0,null,"SRV Scorpion Plasma Turret Gun",ShipModule.ModuleTypes.PulseLaser) },

            { "testbuggy_cockpit", new ShipModule(-1,0,0,null,"SRV Scarab Cockpit",ShipModule.ModuleTypes.CockpitType) },
            { "scarab_armour_grade1", new ShipModule(-1,0,0,null,"SRV Scarab Armour",ShipModule.ModuleTypes.LightweightAlloy) },
            { "int_fueltank_size0_class2", new ShipModule(-1,0,0,null,"SRV Scopion Fuel tank Size 0 Class 2",ShipModule.ModuleTypes.FuelTank) },
            { "combat_multicrew_srv_01_cockpit", new ShipModule(-1,0,0,null,"SRV Scorpion Cockpit",ShipModule.ModuleTypes.CockpitType) },
            { "int_powerdistributor_size0_class1_cms", new ShipModule(-1,0,0,null,"SRV Scorpion Power Distributor Size 0 Class 1 Cms",ShipModule.ModuleTypes.PowerDistributor) },
            { "int_powerplant_size0_class1_cms", new ShipModule(-1,0,0,null,"SRV Scorpion Powerplant Size 0 Class 1 Cms",ShipModule.ModuleTypes.PowerPlant) },
            { "vehicle_turretgun", new ShipModule(-1,0,0,null,"SRV Scarab Turret",ShipModule.ModuleTypes.PulseLaser) },

            { "hpt_datalinkscanner", new ShipModule(-1,0,0,null,"SRV Data Link Scanner",ShipModule.ModuleTypes.Sensors) },
            { "int_sinewavescanner_size1_class1", new ShipModule(-1,0,0,null,"SRV Scarab Scanner",ShipModule.ModuleTypes.Sensors) },
            { "int_sensors_surface_size1_class1", new ShipModule(-1,0,0,null,"SRV Sensors",ShipModule.ModuleTypes.Sensors) },

            { "int_lifesupport_size0_class1", new ShipModule(-1,0,0,null,"SRV Life Support",ShipModule.ModuleTypes.LifeSupport) },
            { "int_shieldgenerator_size0_class3", new ShipModule(-1,0,0,null,"SRV Shields",ShipModule.ModuleTypes.ShieldGenerator) },
        };

        #endregion

        #region Vanity Modules

        public static Dictionary<string, ShipModule> vanitymodules = new Dictionary<string, ShipModule>   // DO NOT USE DIRECTLY - public is for checking only
        {
            { "null", new ShipModule(-1,0,0,null,"Error in frontier journal - Null module", ShipModule.ModuleTypes.UnknownType) },

            { "typex_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Alliance Chieftain Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "typex_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Alliance Chieftain Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "typex_shipkit1_wings1", new ShipModule(-1,0,0,null,"Alliance Chieftain Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_tail1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_tail2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_tail3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_tail4", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_wings1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_wings2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_wings3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit1_wings4", new ShipModule(-1,0,0,null,"Anaconda Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_bumper1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_bumper2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_bumper3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_spoiler1", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_spoiler2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_spoiler3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_tail2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_tail3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_wings2", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "anaconda_shipkit2raider_wings3", new ShipModule(-1,0,0,null,"Anaconda Shipkit 2 Raider Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "asp_industrial1_bumper1", new ShipModule(-1,0,0,null,"Asp Industrial 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_industrial1_spoiler1", new ShipModule(-1,0,0,null,"Asp Industrial 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_industrial1_wings1", new ShipModule(-1,0,0,null,"Asp Industrial 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_wings1", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_wings2", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_wings3", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit1_wings4", new ShipModule(-1,0,0,null,"Asp Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit2raider_bumper2", new ShipModule(-1,0,0,null,"Asp Shipkit 2 Raider Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit2raider_bumper3", new ShipModule(-1,0,0,null,"Asp Shipkit 2 Raider Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit2raider_tail2", new ShipModule(-1,0,0,null,"Asp Shipkit 2 Raider Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_shipkit2raider_wings2", new ShipModule(-1,0,0,null,"Asp Shipkit 2 Raider Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "asp_science1_spoiler1", new ShipModule(-1,0, "Asp Science 1 Spoiler 1", ShipModule.ModuleTypes.VanityType ) },
            { "asp_science1_wings1", new ShipModule(-1,0, "Asp Science 1 Wings 1", ShipModule.ModuleTypes.VanityType ) },
            { "asp_science1_bumper1", new ShipModule(-1,0, "Asp Science 1 Bumper 1", ShipModule.ModuleTypes.VanityType ) },
            { "bobble_ap2_textexclam", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text !", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_texte", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text e", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textl", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text l", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textn", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text n", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_texto", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text o", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textr", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text r", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_texts", new ShipModule(-1,0,0,null,"Bobble Ap 2 Text s", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textasterisk", new ShipModule(-1,0,0,null,"Bobble Ap 2 Textasterisk", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textg", new ShipModule(-1,0,0,null,"Bobble Ap 2 Textg", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textj", new ShipModule(-1,0,0,null,"Bobble Ap 2 Textj", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ap2_textu", new ShipModule(-1,0, "Bobble Ap 2 Textu", ShipModule.ModuleTypes.VanityType ) },
            { "bobble_ap2_texty", new ShipModule(-1,0, "Bobble Ap 2 Texty", ShipModule.ModuleTypes.VanityType ) },
            { "bobble_christmastree", new ShipModule(-1,0,0,null,"Bobble Christmas Tree", ShipModule.ModuleTypes.VanityType) },
            { "bobble_davidbraben", new ShipModule(-1,0,0,null,"Bobble David Braben", ShipModule.ModuleTypes.VanityType) },
            { "bobble_dotd_blueskull", new ShipModule(-1,0,0,null,"Bobble Dotd Blueskull", ShipModule.ModuleTypes.VanityType) },
            { "bobble_nav_beacon", new ShipModule(-1,0,0,null,"Bobble Nav Beacon", ShipModule.ModuleTypes.VanityType) },
            { "bobble_oldskool_anaconda", new ShipModule(-1,0,0,null,"Bobble Oldskool Anaconda", ShipModule.ModuleTypes.VanityType) },
            { "bobble_oldskool_aspmkii", new ShipModule(-1,0,0,null,"Bobble Oldskool Asp Mk II", ShipModule.ModuleTypes.VanityType) },
            { "bobble_oldskool_cobramkiii", new ShipModule(-1,0,0,null,"Bobble Oldskool Cobram Mk III", ShipModule.ModuleTypes.VanityType) },
            { "bobble_oldskool_python", new ShipModule(-1,0,0,null,"Bobble Oldskool Python", ShipModule.ModuleTypes.VanityType) },
            { "bobble_oldskool_thargoid", new ShipModule(-1,0,0,null,"Bobble Oldskool Thargoid", ShipModule.ModuleTypes.VanityType) },
            { "bobble_pilot_dave_expo_flight_suit", new ShipModule(-1,0,0,null,"Bobble Pilot Dave Expo Flight Suit", ShipModule.ModuleTypes.VanityType) },
            { "bobble_pilotfemale", new ShipModule(-1,0,0,null,"Bobble Pilot Female", ShipModule.ModuleTypes.VanityType) },
            { "bobble_pilotmale", new ShipModule(-1,0,0,null,"Bobble Pilot Male", ShipModule.ModuleTypes.VanityType) },
            { "bobble_pilotmale_expo_flight_suit", new ShipModule(-1,0,0,null,"Bobble Pilot Male Expo Flight Suit", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_earth", new ShipModule(-1,0,0,null,"Bobble Planet Earth", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_jupiter", new ShipModule(-1,0,0,null,"Bobble Planet Jupiter", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_mars", new ShipModule(-1,0,0,null,"Bobble Planet Mars", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_mercury", new ShipModule(-1,0,0,null,"Bobble Planet Mercury", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_neptune", new ShipModule(-1,0,0,null,"Bobble Planet Neptune", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_saturn", new ShipModule(-1,0,0,null,"Bobble Planet Saturn", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_uranus", new ShipModule(-1,0,0,null,"Bobble Planet Uranus", ShipModule.ModuleTypes.VanityType) },
            { "bobble_planet_venus", new ShipModule(-1,0,0,null,"Bobble Planet Venus", ShipModule.ModuleTypes.VanityType) },
            { "bobble_plant_aloe", new ShipModule(-1,0,0,null,"Bobble Plant Aloe", ShipModule.ModuleTypes.VanityType) },
            { "bobble_plant_braintree", new ShipModule(-1,0,0,null,"Bobble Plant Braintree", ShipModule.ModuleTypes.VanityType) },
            { "bobble_plant_rosequartz", new ShipModule(-1,0,0,null,"Bobble Plant Rosequartz", ShipModule.ModuleTypes.VanityType) },
            { "bobble_pumpkin", new ShipModule(-1,0,0,null,"Bobble Pumpkin", ShipModule.ModuleTypes.VanityType) },
            { "bobble_santa", new ShipModule(-1,0,0,null,"Bobble Santa", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ship_anaconda", new ShipModule(-1,0,0,null,"Bobble Ship Anaconda", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ship_cobramkiii", new ShipModule(-1,0,0,null,"Bobble Ship Cobra Mk III", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ship_cobramkiii_ffe", new ShipModule(-1,0,0,null,"Bobble Ship Cobra Mk III FFE", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ship_thargoid", new ShipModule(-1,0,0,null,"Bobble Ship Thargoid", ShipModule.ModuleTypes.VanityType) },
            { "bobble_ship_viper", new ShipModule(-1,0,0,null,"Bobble Ship Viper", ShipModule.ModuleTypes.VanityType) },
            { "bobble_snowflake", new ShipModule(-1,0,0,null,"Bobble Snowflake", ShipModule.ModuleTypes.VanityType) },
            { "bobble_snowman", new ShipModule(-1,0,0,null,"Bobble Snowman", ShipModule.ModuleTypes.VanityType) },
            { "bobble_station_coriolis", new ShipModule(-1,0,0,null,"Bobble Station Coriolis", ShipModule.ModuleTypes.VanityType) },
            { "bobble_station_coriolis_wire", new ShipModule(-1,0,0,null,"Bobble Station Coriolis Wire", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textexclam", new ShipModule(-1,0,0,null,"Bobble Text !", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textpercent", new ShipModule(-1,0,0,null,"Bobble Text %", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textquest", new ShipModule(-1,0,0,null,"Bobble Text ?", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text0", new ShipModule(-1,0,0,null,"Bobble Text 0", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text1", new ShipModule(-1,0,0,null,"Bobble Text 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text2", new ShipModule(-1,0,0,null,"Bobble Text 2", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text3", new ShipModule(-1,0,0,null,"Bobble Text 3", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text4", new ShipModule(-1,0,0,null,"Bobble Text 4", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text5", new ShipModule(-1,0,0,null,"Bobble Text 5", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text6", new ShipModule(-1,0,0,null,"Bobble Text 6", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text7", new ShipModule(-1,0,0,null,"Bobble Text 7", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text8", new ShipModule(-1,0,0,null,"Bobble Text 8", ShipModule.ModuleTypes.VanityType) },
            { "bobble_text9", new ShipModule(-1,0,0,null,"Bobble Text 9", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texta", new ShipModule(-1,0,0,null,"Bobble Text A", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textb", new ShipModule(-1,0,0,null,"Bobble Text B", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textbracket01", new ShipModule(-1,0,0,null,"Bobble Text Bracket 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textbracket02", new ShipModule(-1,0,0,null,"Bobble Text Bracket 2", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textcaret", new ShipModule(-1,0,0,null,"Bobble Text Caret", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textd", new ShipModule(-1,0,0,null,"Bobble Text d", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textdollar", new ShipModule(-1,0,0,null,"Bobble Text Dollar", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texte", new ShipModule(-1,0,0,null,"Bobble Text e", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texte04", new ShipModule(-1,0,0,null,"Bobble Text E 4", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textexclam01", new ShipModule(-1,0,0,null,"Bobble Text Exclam 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textf", new ShipModule(-1,0,0,null,"Bobble Text f", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textg", new ShipModule(-1,0,0,null,"Bobble Text G", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texth", new ShipModule(-1,0,0,null,"Bobble Text H", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texthash", new ShipModule(-1,0,0,null,"Bobble Text Hash", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texti", new ShipModule(-1,0,0,null,"Bobble Text I", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texti02", new ShipModule(-1,0,0,null,"Bobble Text I 2", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textm", new ShipModule(-1,0,0,null,"Bobble Text m", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textn", new ShipModule(-1,0,0,null,"Bobble Text n", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texto02", new ShipModule(-1,0,0,null,"Bobble Text O 2", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texto03", new ShipModule(-1,0,0,null,"Bobble Text O 3", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textp", new ShipModule(-1,0,0,null,"Bobble Text P", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textplus", new ShipModule(-1,0,0,null,"Bobble Text Plus", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textr", new ShipModule(-1,0,0,null,"Bobble Text r", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textt", new ShipModule(-1,0,0,null,"Bobble Text t", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textu", new ShipModule(-1,0,0,null,"Bobble Text U", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textu01", new ShipModule(-1,0,0,null,"Bobble Text U 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textv", new ShipModule(-1,0,0,null,"Bobble Text V", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textx", new ShipModule(-1,0,0,null,"Bobble Text X", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texty", new ShipModule(-1,0,0,null,"Bobble Text Y", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textz", new ShipModule(-1,0,0,null,"Bobble Text Z", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textasterisk", new ShipModule(-1,0,0,null,"Bobble Textasterisk", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texte01", new ShipModule(-1,0,0,null,"Bobble Texte 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texti01", new ShipModule(-1,0,0,null,"Bobble Texti 1", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textk", new ShipModule(-1,0,0,null,"Bobble Textk", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textl", new ShipModule(-1,0,0,null,"Bobble Textl", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textminus", new ShipModule(-1,0,0,null,"Bobble Textminus", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texto", new ShipModule(-1,0,0,null,"Bobble Texto", ShipModule.ModuleTypes.VanityType) },
            { "bobble_texts", new ShipModule(-1,0,0,null,"Bobble Texts", ShipModule.ModuleTypes.VanityType) },
            { "bobble_textunderscore", new ShipModule(-1,0,0,null,"Bobble Textunderscore", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_anti_thargoid_s", new ShipModule(-1,0,0,null,"Bobble Trophy Anti Thargoid S", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_combat", new ShipModule(-1,0,0,null,"Bobble Trophy Combat", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_combat_s", new ShipModule(-1,0,0,null,"Bobble Trophy Combat S", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_community", new ShipModule(-1,0,0,null,"Bobble Trophy Community", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_exploration", new ShipModule(-1,0,0,null,"Bobble Trophy Exploration", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_exploration_b", new ShipModule(-1,0,0,null,"Bobble Trophy Exploration B", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_exploration_s", new ShipModule(-1,0,0,null,"Bobble Trophy Exploration S", ShipModule.ModuleTypes.VanityType) },
            { "bobble_trophy_powerplay_b", new ShipModule(-1,0,0,null,"Bobble Trophy Powerplay B", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_wings3", new ShipModule(-1,0,0,null,"Co)bra MK III Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Cobra MK III Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Cobra MK III Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_tail1", new ShipModule(-1,0,0,null,"Cobra MK III Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_tail3", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_wings1", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_wings2", new ShipModule(-1,0,0,null,"Cobra MK III Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkitraider1_bumper2", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit Raider 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkitraider1_spoiler3", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit Raider 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkitraider1_tail2", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit Raider 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkitraider1_wings1", new ShipModule(-1,0,0,null,"Cobra Mk III Shipkit Raider 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Cobramkiii Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "cobramkiii_shipkit1_tail4", new ShipModule(-1,0,0,null,"Cobramkiii Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_wings2", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "cutter_shipkit1_wings3", new ShipModule(-1,0,0,null,"Cutter Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_elite02", new ShipModule(-1, 0, "Decal Explorer Elite 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_elite03", new ShipModule(-1,0, "Decal Explorer Elite 3", ShipModule.ModuleTypes.VanityType ) },
            { "decal_skull9", new ShipModule(-1,0, "Decal Skull 9", ShipModule.ModuleTypes.VanityType ) },
            { "decal_skull8", new ShipModule(-1,0, "Decal Skull 8", ShipModule.ModuleTypes.VanityType ) },
            { "decal_alien_hunter2", new ShipModule(-1,0,0,null,"Decal Alien Hunter 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_alien_hunter6", new ShipModule(-1,0,0,null,"Decal Alien Hunter 6", ShipModule.ModuleTypes.VanityType) },
            { "decal_alien_sympathiser_b", new ShipModule(-1,0,0,null,"Decal Alien Sympathiser B", ShipModule.ModuleTypes.VanityType) },
            { "decal_anti_thargoid", new ShipModule(-1,0,0,null,"Decal Anti Thargoid", ShipModule.ModuleTypes.VanityType) },
            { "decal_bat2", new ShipModule(-1,0,0,null,"Decal Bat 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_beta_tester", new ShipModule(-1,0,0,null,"Decal Beta Tester", ShipModule.ModuleTypes.VanityType) },
            { "decal_bounty_hunter", new ShipModule(-1,0,0,null,"Decal Bounty Hunter", ShipModule.ModuleTypes.VanityType) },
            { "decal_bridgingthegap", new ShipModule(-1,0,0,null,"Decal Bridgingthegap", ShipModule.ModuleTypes.VanityType) },
            { "decal_cannon", new ShipModule(-1,0,0,null,"Decal Cannon", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_competent", new ShipModule(-1,0,0,null,"Decal Combat Competent", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_dangerous", new ShipModule(-1,0,0,null,"Decal Combat Dangerous", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_deadly", new ShipModule(-1,0,0,null,"Decal Combat Deadly", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_elite", new ShipModule(-1,0,0,null,"Decal Combat Elite", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_expert", new ShipModule(-1,0,0,null,"Decal Combat Expert", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_master", new ShipModule(-1,0,0,null,"Decal Combat Master", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_mostly_harmless", new ShipModule(-1,0,0,null,"Decal Combat Mostly Harmless", ShipModule.ModuleTypes.VanityType) },
            { "decal_combat_novice", new ShipModule(-1,0,0,null,"Decal Combat Novice", ShipModule.ModuleTypes.VanityType) },
            { "decal_community", new ShipModule(-1,0,0,null,"Decal Community", ShipModule.ModuleTypes.VanityType) },
            { "decal_distantworlds", new ShipModule(-1,0,0,null,"Decal Distant Worlds", ShipModule.ModuleTypes.VanityType) },
            { "decal_distantworlds2", new ShipModule(-1,0,0,null,"Decal Distantworlds 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_egx", new ShipModule(-1,0,0,null,"Decal Egx", ShipModule.ModuleTypes.VanityType) },
            { "decal_espionage", new ShipModule(-1,0,0,null,"Decal Espionage", ShipModule.ModuleTypes.VanityType) },
            { "decal_exploration_emisswhite", new ShipModule(-1,0,0,null,"Decal Exploration Emisswhite", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_elite", new ShipModule(-1,0,0,null,"Decal Explorer Elite", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_elite05", new ShipModule(-1,0,0,null,"Decal Explorer Elite 5", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_mostly_aimless", new ShipModule(-1,0,0,null,"Decal Explorer Mostly Aimless", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_pathfinder", new ShipModule(-1,0,0,null,"Decal Explorer Pathfinder", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_ranger", new ShipModule(-1,0,0,null,"Decal Explorer Ranger", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_scout", new ShipModule(-1,0,0,null,"Decal Explorer Scout", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_starblazer", new ShipModule(-1,0,0,null,"Decal Explorer Starblazer", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_surveyor", new ShipModule(-1,0,0,null,"Decal Explorer Surveyor", ShipModule.ModuleTypes.VanityType) },
            { "decal_explorer_trailblazer", new ShipModule(-1,0,0,null,"Decal Explorer Trailblazer", ShipModule.ModuleTypes.VanityType) },
            { "decal_expo", new ShipModule(-1,0,0,null,"Decal Expo", ShipModule.ModuleTypes.VanityType) },
            { "decal_founders_reversed", new ShipModule(-1,0,0,null,"Decal Founders Reversed", ShipModule.ModuleTypes.VanityType) },
            { "decal_fuelrats", new ShipModule(-1,0,0,null,"Decal Fuel Rats", ShipModule.ModuleTypes.VanityType) },
            { "decal_galnet", new ShipModule(-1,0,0,null,"Decal Galnet", ShipModule.ModuleTypes.VanityType) },
            { "decal_lavecon", new ShipModule(-1,0,0,null,"Decal Lave Con", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_constructshipemp_gold", new ShipModule(-1,0,0,null,"Decal Met Constructshipemp Gold", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_espionage_gold", new ShipModule(-1,0,0,null,"Decal Met Espionage Gold", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_espionage_silver", new ShipModule(-1,0,0,null,"Decal Met Espionage Silver", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_exploration_gold", new ShipModule(-1,0,0,null,"Decal Met Exploration Gold", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_mining_bronze", new ShipModule(-1,0,0,null,"Decal Met Mining Bronze", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_mining_gold", new ShipModule(-1,0,0,null,"Decal Met Mining Gold", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_mining_silver", new ShipModule(-1,0,0,null,"Decal Met Mining Silver", ShipModule.ModuleTypes.VanityType) },
            { "decal_met_salvage_gold", new ShipModule(-1,0,0,null,"Decal Met Salvage Gold", ShipModule.ModuleTypes.VanityType) },
            { "decal_mining", new ShipModule(-1,0,0,null,"Decal Mining", ShipModule.ModuleTypes.VanityType) },
            { "decal_networktesters", new ShipModule(-1,0,0,null,"Decal Network Testers", ShipModule.ModuleTypes.VanityType) },
            { "decal_onionhead1", new ShipModule(-1,0,0,null,"Decal Onionhead 1", ShipModule.ModuleTypes.VanityType) },
            { "decal_onionhead2", new ShipModule(-1,0,0,null,"Decal Onionhead 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_onionhead3", new ShipModule(-1,0,0,null,"Decal Onionhead 3", ShipModule.ModuleTypes.VanityType) },
            { "decal_passenger_e", new ShipModule(-1,0,0,null,"Decal Passenger E", ShipModule.ModuleTypes.VanityType) },
            { "decal_passenger_g", new ShipModule(-1,0,0,null,"Decal Passenger G", ShipModule.ModuleTypes.VanityType) },
            { "decal_passenger_l", new ShipModule(-1,0,0,null,"Decal Passenger L", ShipModule.ModuleTypes.VanityType) },
            { "decal_paxprime", new ShipModule(-1,0,0,null,"Decal Pax Prime", ShipModule.ModuleTypes.VanityType) },
            { "decal_pilot_fed1", new ShipModule(-1,0,0,null,"Decal Pilot Fed 1", ShipModule.ModuleTypes.VanityType) },
            { "decal_planet2", new ShipModule(-1,0,0,null,"Decal Planet 2", ShipModule.ModuleTypes.VanityType) },
            { "decal_playergroup_wolves_of_jonai", new ShipModule(-1,0,0,null,"Decal Player Group Wolves Of Jonai", ShipModule.ModuleTypes.VanityType) },
            { "decal_playergroup_ugc", new ShipModule(-1,0,0,null,"Decal Playergroup Ugc", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_hudson", new ShipModule(-1,0,0,null,"Decal Power Play Hudson", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_mahon", new ShipModule(-1,0,0,null,"Decal Power Play Mahon", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_utopia", new ShipModule(-1,0,0,null,"Decal Power Play Utopia", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_aislingduval", new ShipModule(-1,0,0,null,"Decal Powerplay Aislingduval", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_halsey", new ShipModule(-1,0,0,null,"Decal Powerplay Halsey", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_kumocrew", new ShipModule(-1,0,0,null,"Decal Powerplay Kumocrew", ShipModule.ModuleTypes.VanityType) },
            { "decal_powerplay_sirius", new ShipModule(-1,0,0,null,"Decal Powerplay Sirius", ShipModule.ModuleTypes.VanityType) },
            { "decal_pumpkin", new ShipModule(-1,0,0,null,"Decal Pumpkin", ShipModule.ModuleTypes.VanityType) },
            { "decal_shark1", new ShipModule(-1,0,0,null,"Decal Shark 1", ShipModule.ModuleTypes.VanityType) },
            { "decal_skull3", new ShipModule(-1,0,0,null,"Decal Skull 3", ShipModule.ModuleTypes.VanityType) },
            { "decal_skull5", new ShipModule(-1,0,0,null,"Decal Skull 5", ShipModule.ModuleTypes.VanityType) },
            { "decal_specialeffect", new ShipModule(-1,0,0,null,"Decal Special Effect", ShipModule.ModuleTypes.VanityType) },
            { "decal_spider", new ShipModule(-1,0,0,null,"Decal Spider", ShipModule.ModuleTypes.VanityType) },
            { "decal_thegolconda", new ShipModule(-1,0,0,null,"Decal Thegolconda", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_broker", new ShipModule(-1,0,0,null,"Decal Trade Broker", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_dealer", new ShipModule(-1,0,0,null,"Decal Trade Dealer", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_elite", new ShipModule(-1,0,0,null,"Decal Trade Elite", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_elite05", new ShipModule(-1,0,0,null,"Decal Trade Elite 5", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_entrepeneur", new ShipModule(-1,0,0,null,"Decal Trade Entrepeneur", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_merchant", new ShipModule(-1,0,0,null,"Decal Trade Merchant", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_mostly_penniless", new ShipModule(-1,0,0,null,"Decal Trade Mostly Penniless", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_peddler", new ShipModule(-1,0,0,null,"Decal Trade Peddler", ShipModule.ModuleTypes.VanityType) },
            { "decal_trade_tycoon", new ShipModule(-1,0,0,null,"Decal Trade Tycoon", ShipModule.ModuleTypes.VanityType) },
            { "decal_triple_elite", new ShipModule(-1,0,0,null,"Decal Triple Elite", ShipModule.ModuleTypes.VanityType) },
            { "diamondbackxl_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Diamond Back XL Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "diamondbackxl_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Diamond Back XL Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "diamondbackxl_shipkit1_wings2", new ShipModule(-1,0,0,null,"Diamond Back XL Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_tail4", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_wings2", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "dolphin_shipkit1_wings3", new ShipModule(-1,0,0,null,"Dolphin Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "eagle_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Eagle Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "eagle_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Eagle Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "eagle_shipkit1_wings1", new ShipModule(-1,0,0,null,"Eagle Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_wings1", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_wings2", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "empire_courier_shipkit1_wings3", new ShipModule(-1,0,0,null,"Empire Courier Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_tail1", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_tail2", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_tail3", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_tail4", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "empire_trader_shipkit1_wings1", new ShipModule(-1,0,0,null,"Empire Trader Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_blue", new ShipModule(-1,0,0,null,"Engine Customisation Blue", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_cyan", new ShipModule(-1,0,0,null,"Engine Customisation Cyan", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_green", new ShipModule(-1,0,0,null,"Engine Customisation Green", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_orange", new ShipModule(-1,0,0,null,"Engine Customisation Orange", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_pink", new ShipModule(-1,0,0,null,"Engine Customisation Pink", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_purple", new ShipModule(-1,0,0,null,"Engine Customisation Purple", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_red", new ShipModule(-1,0,0,null,"Engine Customisation Red", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_white", new ShipModule(-1,0,0,null,"Engine Customisation White", ShipModule.ModuleTypes.VanityType) },
            { "enginecustomisation_yellow", new ShipModule(-1,0,0,null,"Engine Customisation Yellow", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_tail1", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_tail2", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_tail3", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_tail4", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_wings3", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "federation_corvette_shipkit1_wings4", new ShipModule(-1,0,0,null,"Federation Corvette Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "federation_gunship_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Federation Gunship Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Fer De Lance Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_tail1", new ShipModule(-1,0,0,null,"Fer De Lance Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_wings2", new ShipModule(-1,0,0,null,"Fer De Lance Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Ferdelance Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Ferdelance Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Ferdelance Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_tail3", new ShipModule(-1,0,0,null,"Ferdelance Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "ferdelance_shipkit1_wings1", new ShipModule(-1,0,0,null,"Ferdelance Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_tail3", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_tail4", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_wings1", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_wings2", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_wings3", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_light_shipkit1_wings4", new ShipModule(-1,0,0,null,"Krait Light Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_tail1", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_tail2", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_tail3", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_wings2", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_wings3", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkit1_wings4", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkitraider1_spoiler3", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit raider 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "krait_mkii_shipkitraider1_wings2", new ShipModule(-1,0,0,null,"Krait Mkii Shipkit raider 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_combat01_white", new ShipModule(-1,0,0,null,"Nameplate Combat 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_combat02_white", new ShipModule(-1,0,0,null,"Nameplate Combat 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_combat03_black", new ShipModule(-1,0,0,null,"Nameplate Combat 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_combat03_white", new ShipModule(-1,0,0,null,"Nameplate Combat 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_empire01_white", new ShipModule(-1,0,0,null,"Nameplate Empire 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_empire02_black", new ShipModule(-1,0,0,null,"Nameplate Empire 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_empire03_black", new ShipModule(-1,0,0,null,"Nameplate Empire 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_empire03_white", new ShipModule(-1,0,0,null,"Nameplate Empire 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition01_black", new ShipModule(-1,0,0,null,"Nameplate Expedition 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition01_white", new ShipModule(-1,0,0,null,"Nameplate Expedition 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition02_black", new ShipModule(-1,0,0,null,"Nameplate Expedition 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition02_white", new ShipModule(-1,0,0,null,"Nameplate Expedition 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition03_black", new ShipModule(-1,0,0,null,"Nameplate Expedition 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_expedition03_white", new ShipModule(-1,0,0,null,"Nameplate Expedition 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer01_black", new ShipModule(-1,0,0,null,"Nameplate Explorer 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer01_grey", new ShipModule(-1,0,0,null,"Nameplate Explorer 1 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer01_white", new ShipModule(-1,0,0,null,"Nameplate Explorer 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer02_black", new ShipModule(-1,0,0,null,"Nameplate Explorer 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer02_grey", new ShipModule(-1,0,0,null,"Nameplate Explorer 2 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer02_white", new ShipModule(-1,0,0,null,"Nameplate Explorer 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer03_black", new ShipModule(-1,0,0,null,"Nameplate Explorer 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_explorer03_white", new ShipModule(-1,0,0,null,"Nameplate Explorer 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_hunter01_white", new ShipModule(-1,0,0,null,"Nameplate Hunter 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_passenger01_black", new ShipModule(-1,0,0,null,"Nameplate Passenger 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_passenger01_white", new ShipModule(-1,0,0,null,"Nameplate Passenger 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_passenger02_black", new ShipModule(-1,0,0,null,"Nameplate Passenger 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_passenger03_white", new ShipModule(-1,0,0,null,"Nameplate Passenger 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_pirate03_white", new ShipModule(-1,0,0,null,"Nameplate Pirate 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical01_black", new ShipModule(-1,0,0,null,"Nameplate Practical 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical01_grey", new ShipModule(-1,0,0,null,"Nameplate Practical 1 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical01_white", new ShipModule(-1,0,0,null,"Nameplate Practical 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical02_black", new ShipModule(-1,0,0,null,"Nameplate Practical 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical02_grey", new ShipModule(-1,0,0,null,"Nameplate Practical 2 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical02_white", new ShipModule(-1,0,0,null,"Nameplate Practical 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical03_black", new ShipModule(-1,0,0,null,"Nameplate Practical 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_practical03_white", new ShipModule(-1,0,0,null,"Nameplate Practical 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_raider03_black", new ShipModule(-1,0,0,null,"Nameplate Raider 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_doubleline_black", new ShipModule(-1,0,0,null,"Nameplate Ship ID Double Line Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_doubleline_grey", new ShipModule(-1,0,0,null,"Nameplate Ship ID Double Line Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_doubleline_white", new ShipModule(-1,0,0,null,"Nameplate Ship ID Double Line White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_grey", new ShipModule(-1,0,0,null,"Nameplate Ship ID Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_singleline_black", new ShipModule(-1,0,0,null,"Nameplate Ship ID Single Line Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_white", new ShipModule(-1,0,0,null,"Nameplate Ship ID White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_white", new ShipModule(-1,0,0,null,"Nameplate Ship Name White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_black", new ShipModule(-1,0,0,null,"Nameplate Shipid Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_singleline_grey", new ShipModule(-1,0,0,null,"Nameplate Shipid Singleline Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipid_singleline_white", new ShipModule(-1,0,0,null,"Nameplate Shipid Singleline White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_black", new ShipModule(-1,0,0,null,"Nameplate Shipname Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_distressed_black", new ShipModule(-1,0,0,null,"Nameplate Shipname Distressed Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_distressed_grey", new ShipModule(-1,0,0,null,"Nameplate Shipname Distressed Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_distressed_white", new ShipModule(-1,0,0,null,"Nameplate Shipname Distressed White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_worn_black", new ShipModule(-1,0,0,null,"Nameplate Shipname Worn Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_shipname_worn_white", new ShipModule(-1,0,0,null,"Nameplate Shipname Worn White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_skulls01_white", new ShipModule(-1,0,0,null,"Nameplate Skulls 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_skulls03_black", new ShipModule(-1,0,0,null,"Nameplate Skulls 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_skulls03_white", new ShipModule(-1,0,0,null,"Nameplate Skulls 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_sympathiser03_white", new ShipModule(-1,0,0,null,"Nameplate Sympathiser 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader01_black", new ShipModule(-1,0,0,null,"Nameplate Trader 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader01_white", new ShipModule(-1,0,0,null,"Nameplate Trader 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader02_black", new ShipModule(-1,0,0,null,"Nameplate Trader 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader02_grey", new ShipModule(-1,0,0,null,"Nameplate Trader 2 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader02_white", new ShipModule(-1,0,0,null,"Nameplate Trader 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader03_black", new ShipModule(-1,0,0,null,"Nameplate Trader 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_trader03_white", new ShipModule(-1,0,0,null,"Nameplate Trader 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_victory02_white", new ShipModule(-1,0,0,null,"Nameplate Victory 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_victory03_white", new ShipModule(-1,0,0,null,"Nameplate Victory 3 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings01_black", new ShipModule(-1,0,0,null,"Nameplate Wings 1 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings01_white", new ShipModule(-1,0,0,null,"Nameplate Wings 1 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings02_black", new ShipModule(-1,0,0,null,"Nameplate Wings 2 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings02_white", new ShipModule(-1,0,0,null,"Nameplate Wings 2 White", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings03_black", new ShipModule(-1,0,0,null,"Nameplate Wings 3 Black", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings03_grey", new ShipModule(-1,0,0,null,"Nameplate Wings 3 Grey", ShipModule.ModuleTypes.VanityType) },
            { "nameplate_wings03_white", new ShipModule(-1,0,0,null,"Nameplate Wings 3 White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_adder_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Adder Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Anaconda Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_corrosive_04", new ShipModule(-1,0,0,null,"Paint Job Anaconda Corrosive 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_eliteexpo_eliteexpo", new ShipModule(-1,0,0,null,"Paint Job Anaconda Elite Expo Elite Expo", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_faction1_04", new ShipModule(-1,0,0,null,"Paint Job Anaconda Faction 1 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_gold_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Anaconda Gold Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_horus2_03", new ShipModule(-1,0,0,null,"Paint Job Anaconda Horus 2 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_iridescenthighcolour_02", new ShipModule(-1,0,0,null,"Paint Job Anaconda Iridescent High Colour 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Anaconda Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_luminous_stripe_03", new ShipModule(-1,0,0,null,"Paint Job Anaconda Luminous Stripe 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_luminous_stripe_04", new ShipModule(-1,0,0,null,"Paint Job Anaconda Luminous Stripe 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_luminous_stripe_06", new ShipModule(-1,0,0,null,"Paint Job Anaconda Luminous Stripe 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Anaconda Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_metallic_gold", new ShipModule(-1,0,0,null,"Paint Job Anaconda Metallic Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_militaire_earth_red", new ShipModule(-1,0,0,null,"Paint Job Anaconda Militaire Earth Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_militaire_earth_yellow", new ShipModule(-1,0,0,null,"Paint Job Anaconda Militaire Earth Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_pulse2_purple", new ShipModule(-1,0,0,null,"Paint Job Anaconda Pulse 2 Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_strife_strife", new ShipModule(-1,0,0,null,"Paint Job Anaconda Strife Strife", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Anaconda Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_green", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_red", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Anaconda Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Anaconda Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Asp Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_gamescom_gamescom", new ShipModule(-1,0,0,null,"Paint Job Asp Games Com GamesCom", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_gold_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Asp Gold Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_iridescenthighcolour_01", new ShipModule(-1,0,0,null,"Paint Job Asp Iridescent High Colour 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_largelogometallic_05", new ShipModule(-1,0,0,null,"Paint Job Asp Largelogometallic 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_metallic_gold", new ShipModule(-1,0,0,null,"Paint Job Asp Metallic Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Asp Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_salvage_06", new ShipModule(-1,0,0,null,"Paint Job Asp Salvage 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_scout_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Asp Scout Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_squadron_green", new ShipModule(-1,0,0,null,"Paint Job Asp Squadron Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_squadron_red", new ShipModule(-1,0,0,null,"Paint Job Asp Squadron Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_stripe1_03", new ShipModule(-1,0,0,null,"Paint Job Asp Stripe 1 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Asp Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_tactical_white", new ShipModule(-1,0,0,null,"Paint Job Asp Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_trespasser_01", new ShipModule(-1,0,0,null,"Paint Job Asp Trespasser 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Asp Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_vibrant_red", new ShipModule(-1,0,0,null,"Paint Job Asp Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Asp Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_belugaliner_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Beluga Liner Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_25thanniversary_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III 25 Thanniversary 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_flag_canada_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Flag Canada 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_flag_uk_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Flag UK 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_militaire_earth_red", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Militaire Earth Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_militaire_forest_green", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_militaire_sand", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Militaire Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_onionhead1_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Onionhead 1 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_stripe2_02", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Stripe 2 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk III Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiv_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk IV Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiv_gradient2_06", new ShipModule(-1,0,0,null,"Paint Job Cobra Mk IV Gradient 2 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_corrosive_05", new ShipModule(-1,0,0,null,"Paint Job Cobra MKIII Corrosive 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_default_52", new ShipModule(-1,0,0,null,"Paint Job Cobra Mkiii Default 52", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Cutter Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_fullmetal_cobalt", new ShipModule(-1,0,0,null,"Paint Job Cutter Full Metal Cobalt", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_fullmetal_paladium", new ShipModule(-1,0,0,null,"Paint Job Cutter Fullmetal Paladium", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_iridescenthighcolour_02", new ShipModule(-1,0,0,null,"Paint Job Cutter Iridescent High Colour 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Cutter Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_luminous_stripe_ver2_02", new ShipModule(-1,0,0,null,"Paint Job Cutter Luminous Stripe Ver 2 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_luminous_stripe_ver2_04", new ShipModule(-1,0,0,null,"Paint Job Cutter Luminous Stripe Ver 2 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Cutter Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Cutter Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_metallic_chrome", new ShipModule(-1,0,0,null,"Paint Job Cutter Metallic Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_militaire_forest_green", new ShipModule(-1,0,0,null,"Paint Job Cutter Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_smartfancy_2_06", new ShipModule(-1,0,0,null,"Paint Job Cutter Smartfancy 2 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_smartfancy_04", new ShipModule(-1,0,0,null,"Paint Job Cutter Smartfancy 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Cutter Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Cutter Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Cutter Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Cutter Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Diamond Back XL Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Diamondbackxl Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Dolphin Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_iridescentblack_01", new ShipModule(-1,0,0,null,"Paint Job Dolphin Iridescentblack 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Dolphin Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Dolphin Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_eagle_crimson", new ShipModule(-1,0,0,null,"Paint Job Eagle Crimson", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_eagle_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Eagle Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_aerial_display_blue", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Aerial Display Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_aerial_display_red", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Aerial Display Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_smartfancy_04", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Smartfancy 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Empire Courier Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_eagle_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Empire Eagle Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_eagle_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Empire Eagle Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Empire Trader Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_trader_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Empire Trader Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_smartfancy_2_06", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Smartfancy 2 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_smartfancy_04", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Smartfancy 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Empiretrader Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_mkii_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Fed Dropship Mk II Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_mkii_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Fed Dropship Mk II Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_mkii_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Fed Dropship Mk II Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Fed Dropship Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_mkii_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Feddropship Mkii Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Feddropship Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_colourgeo2_blue", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Colour Geo 2 Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_colourgeo_blue", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Colour Geo Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_iridescenthighcolour_02", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Iridescent High Colour 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_iridescentblack_02", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Iridescentblack 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_metallic_chrome", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Metallic Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_predator_red", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Predator Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Federation Corvette Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_gunship_metallic_chrome", new ShipModule(-1,0,0,null,"Paint Job Federation Gunship Metallic Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_gunship_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Federation Gunship Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_gunship_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Federation Gunship Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Fer De Lance Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Fer De Lance Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Fer De Lance Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Fer De Lance Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_gradient2_crimson", new ShipModule(-1,0,0,null,"Paint Job Ferdelance Gradient 2 Crimson", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_vibrant_red", new ShipModule(-1,0,0,null,"Paint Job Ferdelance Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_hauler_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Hauler Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_hauler_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Hauler Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Ind Fighter Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Ind Fighter Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_vibrant_green", new ShipModule(-1,0,0,null,"Paint Job Ind Fighter Vibrant Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Ind Fighter Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_independant_trader_tactical_white", new ShipModule(-1,0,0,null,"Paint Job Independant Trader Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_independant_trader_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Independant Trader Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Indfighter Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Krait Light Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_gradient2_blue", new ShipModule(-1,0,0,null,"Paint Job Krait Light Gradient 2 Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_horus1_03", new ShipModule(-1,0,0,null,"Paint Job Krait Light Horus 1 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_horus2_03", new ShipModule(-1,0,0,null,"Paint Job Krait Light Horus 2 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_iridescentblack_02", new ShipModule(-1,0,0,null,"Paint Job Krait Light Iridescentblack 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Krait Light Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_salvage_01", new ShipModule(-1,0,0,null,"Paint Job Krait Light Salvage 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Krait Light Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_salvage_04", new ShipModule(-1,0,0,null,"Paint Job Krait Light Salvage 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_salvage_06", new ShipModule(-1,0,0,null,"Paint Job Krait Light Salvage 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_spring_05", new ShipModule(-1,0,0,null,"Paint Job Krait Light Spring 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_tactical_white", new ShipModule(-1,0,0,null,"Paint Job Krait Light Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_iridescenthighcolour_05", new ShipModule(-1,0,0,null,"Paint Job Krait Mk II Iridescent High Colour 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_specialeffectchristmas_01", new ShipModule(-1,0,0,null,"Paint Job Krait Mk II Special Effect Christmas 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_festive_silver", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Festive Silver", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_horus2_01", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Horus 2 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_militaire_forest_green", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_tactical_red", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Tactical Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_trims_blackmagenta", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Trims Blackmagenta", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_turbulence_02", new ShipModule(-1,0,0,null,"Paint Job Krait Mkii Turbulence 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_mamba_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Mamba Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_orca_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Orca Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_orca_corporate2_corporate2e", new ShipModule(-1,0,0,null,"Paint Job Orca Corporate 2 Corporate 2 E", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_orca_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Orca Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Python Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_corrosive_05", new ShipModule(-1,0,0,null,"Paint Job Python Corrosive 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_eliteexpo_eliteexpo", new ShipModule(-1,0,0,null,"Paint Job Python Elite Expo Elite Expo", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_gold_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Python Gold Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_gradient2_02", new ShipModule(-1,0,0,null,"Paint Job Python Gradient 2 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_gradient2_06", new ShipModule(-1,0,0,null,"Paint Job Python Gradient 2 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_horus1_01", new ShipModule(-1,0,0,null,"Paint Job Python Horus 1 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_iridescentblack_06", new ShipModule(-1,0,0,null,"Paint Job Python Iridescentblack 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_luminous_stripe_03", new ShipModule(-1,0,0,null,"Paint Job Python Luminous Stripe 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Python Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_metallic2_gold", new ShipModule(-1,0,0,null,"Paint Job Python Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_dark_green", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Dark Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_desert_sand", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Desert Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_earth_red", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Earth Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_earth_yellow", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Earth Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_forest_green", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militaire_sand", new ShipModule(-1,0,0,null,"Paint Job Python Militaire Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_militarystripe_blue", new ShipModule(-1,0,0,null,"Paint Job Python Military Stripe Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Python Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_squadron_black", new ShipModule(-1,0,0,null,"Paint Job Python Squadron Black", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_green", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_red", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Python Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_wireframe_01", new ShipModule(-1,0,0,null,"Paint Job Python Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_doublestripe_08", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Doublestripe 8", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_festive_silver", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Festive Silver", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_hotrod_01", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Hotrod 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_metallic_chrome", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Metallic Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_pax_east_pax_east", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Pax East", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_pilotreward_01", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Pilotreward 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Sidewinder Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_chase_04", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Chase 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_chase_05", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Chase 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_militaire_desert_sand", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Militaire Desert Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_tactical_grey", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_tactical_red", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Tactical Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_tactical_white", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Testbuggy Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Type 6 Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Type 6 Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_militaire_sand", new ShipModule(-1,0,0,null,"Paint Job Type 6 Militaire Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_tactical_blue", new ShipModule(-1,0,0,null,"Paint Job Type 6 Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Type 6 Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Type 6 Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type7_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Type 7 Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type7_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Type 7 Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type7_tactical_white", new ShipModule(-1,0,0,null,"Paint Job Type 7 Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_mechanist_04", new ShipModule(-1,0,0,null,"Paint Job Type 9 Mechanist 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_fullmetal_cobalt", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Full Metal Cobalt", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_metallic2_chrome", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Metallic 2 Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_militaire_forest_green", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_tactical_red", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Tactical Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Type 9 Military Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_salvage_03", new ShipModule(-1,0,0,null,"Paint Job Type 9 Salvage 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_salvage_06", new ShipModule(-1,0,0,null,"Paint Job Type 9 Salvage 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_spring_04", new ShipModule(-1,0,0,null,"Paint Job Type 9 Spring 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Type 9 Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_2_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Typex 2 Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_3_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Typex 3 Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_festive_silver", new ShipModule(-1,0,0,null,"Paint Job Typex Festive Silver", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Typex Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Viper Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_default_03", new ShipModule(-1,0,0,null,"Paint Job Viper Default 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Viper Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_merc", new ShipModule(-1,0,0,null,"Paint Job Viper Merc", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Viper Mk IV Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Viper Mkiv Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_stripe1_02", new ShipModule(-1,0,0,null,"Paint Job Viper Stripe 1 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_blue", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_green", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_orange", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_purple", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Purple", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_red", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_vibrant_yellow", new ShipModule(-1,0,0,null,"Paint Job Viper Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_blackfriday_01", new ShipModule(-1,0,0,null,"Paint Job Vulture Black Friday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_lrpo_azure", new ShipModule(-1,0,0,null,"Paint Job Vulture Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_metallic_chrome", new ShipModule(-1,0,0,null,"Paint Job Vulture Metallic Chrome", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_militaire_desert_sand", new ShipModule(-1,0,0,null,"Paint Job Vulture Militaire Desert Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_synth_orange", new ShipModule(-1,0,0,null,"Paint Job Vulture Synth Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_corrosive_05", new ShipModule(-1,0,0,null,"Paintjob Anaconda Corrosive 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_lavecon_lavecon", new ShipModule(-1,0,0,null,"Paintjob Anaconda Lavecon Lavecon", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_metallic2_gold", new ShipModule(-1,0,0,null,"Paintjob Anaconda Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_squadron_black", new ShipModule(-1,0,0,null,"Paintjob Anaconda Squadron Black", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_squadron_blue", new ShipModule(-1,0,0,null,"Paintjob Anaconda Squadron Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_squadron_green", new ShipModule(-1,0,0,null,"Paintjob Anaconda Squadron Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_squadron_red", new ShipModule(-1,0,0,null,"Paintjob Anaconda Squadron Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_tactical_grey", new ShipModule(-1,0,0,null,"Paintjob Anaconda Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_tactical_red", new ShipModule(-1,0,0,null,"Paintjob Anaconda Tactical Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Anaconda Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_halloween01_05", new ShipModule(-1,0,0,null,"Paintjob Asp Halloween 1 5", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_lavecon_lavecon", new ShipModule(-1,0,0,null,"Paintjob Asp Lavecon Lavecon", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_lrpo_azure", new ShipModule(-1,0,0,null,"Paintjob Asp Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_metallic2_gold", new ShipModule(-1,0,0,null,"Paintjob Asp Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_operator_green", new ShipModule(-1,0,0,null,"Paintjob Asp Operator Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_operator_red", new ShipModule(-1,0,0,null,"Paintjob Asp Operator Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_squadron_black", new ShipModule(-1,0,0,null,"Paintjob Asp Squadron Black", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_squadron_blue", new ShipModule(-1,0,0,null,"Paintjob Asp Squadron Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_stripe1_04", new ShipModule(-1,0,0,null,"Paintjob Asp Stripe 1 4", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Asp Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_asp_vibrant_orange", new ShipModule(-1,0,0,null,"Paintjob Asp Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_belugaliner_corporatefleet_fleeta", new ShipModule(-1,0,0,null,"Paintjob Belugaliner Corporatefleet Fleeta", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_horizons_desert", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Horizons Desert", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_horizons_lunar", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Horizons Lunar", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_horizons_polar", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Horizons Polar", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_stripe1_03", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Stripe 1 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_tactical_grey", new ShipModule(-1,0,0,null,"Paintjob Cobra Mk III Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_vibrant_orange", new ShipModule(-1,0,0,null,"Paintjob Cobra Mk III Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_yogscast_01", new ShipModule(-1,0,0,null,"Paintjob Cobra MK III Yogscast 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cobramkiii_stripe2_03", new ShipModule(-1,0,0,null,"Paintjob Cobramkiii Stripe 2 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Cutter Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondback_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob Diamondback Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondback_tactical_brown", new ShipModule(-1,0,0,null,"Paintjob Diamondback Tactical Brown", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondback_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Diamondback Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_blackfriday_01", new ShipModule(-1,0,0,null,"Paintjob Diamondbackxl Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob Diamondbackxl Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Diamondbackxl Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Diamondbackxl Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_corporatefleet_fleeta", new ShipModule(-1,0,0,null,"Paintjob Dolphin Corporatefleet Fleeta", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_dolphin_vibrant_yellow", new ShipModule(-1,0,0,null,"Paintjob Dolphin Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_eagle_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob Eagle Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_eagle_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Eagle Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_blackfriday_01", new ShipModule(-1,0,0,null,"Paintjob Empire Courier Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_courier_metallic2_gold", new ShipModule(-1,0,0,null,"Paintjob Empire Courier Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_fighter_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Empire Fighter Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empire_fighter_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Empire Fighter Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_empiretrader_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Empiretrader Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_feddropship_tactical_grey", new ShipModule(-1,0,0,null,"Paintjob Feddropship Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_colourgeo_red", new ShipModule(-1,0,0,null,"Paintjob Federation Corvette Colourgeo Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_predator_blue", new ShipModule(-1,0,0,null,"Paintjob Federation Corvette Predator Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Federation Corvette Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_corvette_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Federation Corvette Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_fighter_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Federation Fighter Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_fighter_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Federation Fighter Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_gunship_tactical_brown", new ShipModule(-1,0,0,null,"Paintjob Federation Gunship Tactical Brown", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_federation_gunship_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Federation Gunship Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Ferdelance Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Ferdelance Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_ferdelance_vibrant_yellow", new ShipModule(-1,0,0,null,"Paintjob Ferdelance Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_hauler_doublestripe_01", new ShipModule(-1,0,0,null,"Paintjob Hauler Doublestripe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_hauler_doublestripe_02", new ShipModule(-1,0,0,null,"Paintjob Hauler Doublestripe 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_independant_trader_blackfriday_01", new ShipModule(-1,0,0,null,"Paintjob Independant Trader Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_indfighter_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Indfighter Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_egypt_02", new ShipModule(-1,0,0,null,"Paintjob Krait Mkii Egypt 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_mkii_vibrant_red", new ShipModule(-1,0,0,null,"Paintjob Krait Mkii Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_orca_militaire_desert_sand", new ShipModule(-1,0,0,null,"Paintjob Orca Militaire Desert Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_orca_vibrant_yellow", new ShipModule(-1,0,0,null,"Paintjob Orca Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_corrosive_01", new ShipModule(-1,0,0,null,"Paintjob Python Corrosive 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_corrosive_06", new ShipModule(-1,0,0,null,"Paintjob Python Corrosive 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_horus1_02", new ShipModule(-1,0,0,null,"Paintjob Python Horus 1 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_horus2_03", new ShipModule(-1,0,0,null,"Paintjob Python Horus 2 3", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_lrpo_azure", new ShipModule(-1,0,0,null,"Paintjob Python Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_luminous_stripe_02", new ShipModule(-1,0,0,null,"Paintjob Python Luminous Stripe 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Python Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_doublestripe_07", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Doublestripe 7", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_gold_wireframe_01", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Gold Wireframe 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_militaire_forest_green", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Militaire Forest Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_specialeffect_01", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Specialeffect 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_thirds_06", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Thirds 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_sidewinder_vibrant_red", new ShipModule(-1,0,0,null,"Paintjob Sidewinder Vibrant Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_chase_06", new ShipModule(-1,0,0,null,"Paintjob SRV Chase 6", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_destination_blue", new ShipModule(-1,0,0,null,"Paintjob SRV Destination Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_luminous_blue", new ShipModule(-1,0,0,null,"Paintjob SRV Luminous Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_luminous_red", new ShipModule(-1,0,0,null,"Paintjob SRV Luminous Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_metallic2_gold", new ShipModule(-1,0,0,null,"Paintjob SRV Metallic 2 Gold", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_militaire_earth_red", new ShipModule(-1,0,0,null,"Paintjob SRV Militaire Earth Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_militaire_earth_yellow", new ShipModule(-1,0,0,null,"Paintjob SRV Militaire Earth Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob SRV Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob SRV Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_vibrant_orange", new ShipModule(-1,0,0,null,"Paintjob SRV Vibrant Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_testbuggy_vibrant_yellow", new ShipModule(-1,0,0,null,"Paintjob SRV Vibrant Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type6_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Type 6 Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type7_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Type 7 Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_blackfriday_01", new ShipModule(-1,0,0,null,"Paintjob Type 9 Blackfriday 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_lrpo_azure", new ShipModule(-1,0,0,null,"Paintjob Type 9 Lrpo Azure", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_military_iridescentblack_02", new ShipModule(-1,0,0,null,"Paintjob Type 9 Military Iridescent black 2", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_type9_vibrant_blue", new ShipModule(-1,0,0,null,"Paintjob Type 9 Vibrant Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_military_tactical_grey", new ShipModule(-1,0,0,null,"Paintjob Typex Military Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_military_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Typex Military Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_typex_operator_red", new ShipModule(-1,0,0,null,"Paintjob Typex Operator Red", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_flag_norway_01", new ShipModule(-1,0,0,null,"Paintjob Viper Flag Norway 1", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_militaire_sand", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Militaire Sand", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_squadron_black", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Squadron Black", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_squadron_orange", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Squadron Orange", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_tactical_brown", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Tactical Brown", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_tactical_green", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Tactical Green", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_tactical_grey", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_viper_mkiv_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Viper MK IV Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_tactical_blue", new ShipModule(-1,0,0,null,"Paintjob Vulture Tactical Blue", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_vulture_tactical_white", new ShipModule(-1,0,0,null,"Paintjob Vulture Tactical White", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_diamondbackxl_tactical_grey", new ShipModule(-1, 0, "Paintjob Diamondbackxl Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_python_tactical_grey", new ShipModule(-1, 0, "Paintjob Python Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_krait_light_tactical_grey", new ShipModule(-1, 0, "Paintjob Krait Light Tactical Grey", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_cutter_militaire_earth_yellow", new ShipModule(-1, 0, "Paintjob Cutter Militaire Earth Yellow", ShipModule.ModuleTypes.VanityType) },
            { "paintjob_anaconda_fullmetal_cobalt", new ShipModule(-1, 0, "Paintjob Anaconda Fullmetal Cobalt", ShipModule.ModuleTypes.VanityType) },

            { "nameplate_expedition02_grey", new ShipModule(-1,0, "Nameplate Expedition 2 Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_adder_lrpo_azure", new ShipModule(-1,0, "Paintjob Adder Lrpo Azure", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_adder_vibrant_orange", new ShipModule(-1,0, "Paintjob Adder Vibrant Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_horus1_02", new ShipModule(-1,0, "Paintjob Anaconda Horus 1 2", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_horus1_03", new ShipModule(-1,0, "Paintjob Anaconda Horus 1 3", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_horus2_01", new ShipModule(-1,0, "Paintjob Anaconda Horus 2 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_icarus_grey", new ShipModule(-1,0, "Paintjob Anaconda Icarus Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_iridescentblack_02", new ShipModule(-1,0, "Paintjob Anaconda Iridescentblack 2", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_lowlighteffect_01_01", new ShipModule(-1,0, "Paintjob Anaconda Lowlighteffect 1 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_militaire_forest_green", new ShipModule(-1,0, "Paint Job Anaconda Militaire Forest Green", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_militaire_sand", new ShipModule(-1,0, "Paintjob Anaconda Militaire Sand", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_prestige_blue", new ShipModule(-1,0, "Paintjob Anaconda Prestige Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_prestige_green", new ShipModule(-1,0, "Paintjob Anaconda Prestige Green", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_prestige_purple", new ShipModule(-1,0, "Paintjob Anaconda Prestige Purple", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_prestige_red", new ShipModule(-1,0, "Paintjob Anaconda Prestige Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_pulse2_green", new ShipModule(-1,0, "Paintjob Anaconda Pulse 2 Green", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_anaconda_war_orange", new ShipModule(-1,0, "Paintjob Anaconda War Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_asp_icarus_grey", new ShipModule(-1,0, "Paintjob Asp Icarus Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_asp_iridescentblack_04", new ShipModule(-1,0, "Paintjob Asp Iridescentblack 4", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_belugaliner_blackfriday_01", new ShipModule(-1,0, "Paintjob Belugaliner Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_belugaliner_ember_blue", new ShipModule(-1,0, "Paintjob Belugaliner Ember Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cobramkiv_lrpo_azure", new ShipModule(-1,0, "Paintjob Cobramkiv Lrpo Azure", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_gradient2_red", new ShipModule(-1,0, "Paintjob Cutter Gradient 2 Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_iridescentblack_05", new ShipModule(-1,0, "Paintjob Cutter Iridescentblack 5", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_synth_orange", new ShipModule(-1,0, "Paintjob Cutter Synth Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_tactical_blue", new ShipModule(-1,0, "Paintjob Cutter Tactical Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_vibrant_red", new ShipModule(-1,0, "Paintjob Cutter Vibrant Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_cutter_war_blue", new ShipModule(-1,0, "Paintjob Cutter War Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_diamondback_blackfriday_01", new ShipModule(-1,0, "Paint Job Diamond Back Black Friday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_diamondback_lrpo_azure", new ShipModule(-1,0, "Paintjob Diamondback Lrpo Azure", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_diamondbackxl_vibrant_orange", new ShipModule(-1,0, "Paintjob Diamondbackxl Vibrant Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_dolphin_vibrant_blue", new ShipModule(-1,0, "Paintjob Dolphin Vibrant Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_eagle_aerial_display_red", new ShipModule(-1,0, "Paintjob Eagle Aerial Display Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_eagle_stripe1_01", new ShipModule(-1,0, "Paintjob Eagle Stripe 1 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_empire_courier_iridescenthighcolour_02", new ShipModule(-1,0, "Paintjob Empire Courier Iridescenthighcolour 2", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_empire_courier_tactical_white", new ShipModule(-1,0, "Paintjob Empire Courier Tactical White", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_empiretrader_slipstream_orange", new ShipModule(-1,0, "Paintjob Empiretrader Slipstream Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_feddropship_militaire_earth_red", new ShipModule(-1,0, "Paintjob Feddropship Militaire Earth Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_corvette_colourgeo_grey", new ShipModule(-1,0, "Paintjob Federation Corvette Colourgeo Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_corvette_razormetal_silver", new ShipModule(-1,0, "Paintjob Federation Corvette Razormetal Silver", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_corvette_tactical_grey", new ShipModule(-1,0, "Paintjob Federation Corvette Tactical Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_corvette_tactical_red", new ShipModule(-1,0, "Paintjob Federation Corvette Tactical Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_corvette_vibrant_red", new ShipModule(-1,0, "Paintjob Federation Corvette Vibrant Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_gunship_blackfriday_01", new ShipModule(-1,0, "Paintjob Federation Gunship Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_federation_gunship_militarystripe_red", new ShipModule(-1,0, "Paintjob Federation Gunship Militarystripe Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_ferdelance_razormetal_copper", new ShipModule(-1,0, "Paintjob Ferdelance Razormetal Copper", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_ferdelance_slipstream_orange", new ShipModule(-1,0, "Paintjob Ferdelance Slipstream Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_hauler_tactical_red", new ShipModule(-1,0, "Paintjob Hauler Tactical Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_hauler_vibrant_blue", new ShipModule(-1,0, "Paintjob Hauler Vibrant Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_light_lowlighteffect_01_06", new ShipModule(-1,0, "Paintjob Krait Light Lowlighteffect 1 6", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_light_turbulence_06", new ShipModule(-1,0, "Paintjob Krait Light Turbulence 6", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_blackfriday_01", new ShipModule(-1,0, "Paintjob Krait Mkii Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_egypt_01", new ShipModule(-1,0, "Paintjob Krait Mkii Egypt 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_horus1_02", new ShipModule(-1,0, "Paintjob Krait Mkii Horus 1 2", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_horus1_03", new ShipModule(-1,0, "Paintjob Krait Mkii Horus 1 3", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_tactical_blue", new ShipModule(-1,0, "Paint Job Krait Mk II Tactical Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_trims_greyorange", new ShipModule(-1,0, "Paintjob Krait Mkii Trims Greyorange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_krait_mkii_vibrant_orange", new ShipModule(-1,0, "Paintjob Krait Mkii Vibrant Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_mamba_tactical_white", new ShipModule(-1,0, "Paintjob Mamba Tactical White", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_orca_corporate1_corporate1", new ShipModule(-1,0, "Paintjob Orca Corporate 1 Corporate 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_orca_geometric_blue", new ShipModule(-1,0, "Paintjob Orca Geometric Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_egypt_01", new ShipModule(-1,0, "Paintjob Python Egypt 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_horus2_01", new ShipModule(-1,0, "Paintjob Python Horus 2 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_lowlighteffect_01_03", new ShipModule(-1,0, "Paintjob Python Lowlighteffect 1 3", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_salvage_06", new ShipModule(-1,0, "Paintjob Python Salvage 6", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_squadron_blue", new ShipModule(-1,0, "Paintjob Python Squadron Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_squadron_gold", new ShipModule(-1,0, "Paintjob Python Squadron Gold", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_squadron_red", new ShipModule(-1,0, "Paintjob Python Squadron Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_python_tactical_red", new ShipModule(-1,0, "Paintjob Python Tactical Red", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type6_foss_orangewhite", new ShipModule(-1,0, "Paintjob Type 6 Foss Orangewhite", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type6_foss_whitered", new ShipModule(-1,0, "Paintjob Type 6 Foss Whitered", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type6_iridescentblack_03", new ShipModule(-1,0, "Paintjob Type 6 Iridescentblack 3", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type7_lrpo_azure", new ShipModule(-1,0, "Paintjob Type 7 Lrpo Azure", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type7_militaire_earth_yellow", new ShipModule(-1,0, "Paintjob Type 7 Militaire Earth Yellow", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type7_turbulence_06", new ShipModule(-1,0, "Paintjob Type 7 Turbulence 6", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type9_military_blackfriday_01", new ShipModule(-1,0, "Paintjob Type 9 Military Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type9_military_vibrant_orange", new ShipModule(-1,0, "Paintjob Type 9 Military Vibrant Orange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type9_tactical_grey", new ShipModule(-1,0, "Paintjob Type 9 Tactical Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_type9_turbulence_03", new ShipModule(-1,0, "Paintjob Type 9 Turbulence 3", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_2_blackfriday_01", new ShipModule(-1,0, "Paintjob Typex 2 Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_3_blackfriday_01", new ShipModule(-1,0, "Paintjob Typex 3 Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_3_military_militaire_forest_green", new ShipModule(-1,0, "Paintjob Typex 3 Military Militaire Forest Green", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_3_military_tactical_grey", new ShipModule(-1,0, "Paintjob Typex 3 Military Tactical Grey", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_3_military_vibrant_yellow", new ShipModule(-1,0, "Paintjob Typex 3 Military Vibrant Yellow", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_3_trims_greyorange", new ShipModule(-1,0, "Paintjob Typex 3 Trims Greyorange", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_typex_blackfriday_01", new ShipModule(-1,0, "Paintjob Typex Blackfriday 1", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_viper_mkiv_slipstream_blue", new ShipModule(-1,0, "Paintjob Viper Mkiv Slipstream Blue", ShipModule.ModuleTypes.VanityType ) },
            { "paintjob_viper_predator_blue", new ShipModule(-1,0, "Paintjob Viper Predator Blue", ShipModule.ModuleTypes.VanityType ) },


            { "python_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Python Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Python Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Python Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Python Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Python Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Python Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Python Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Python Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_tail1", new ShipModule(-1,0,0,null,"Python Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_tail2", new ShipModule(-1,0,0,null,"Python Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_tail3", new ShipModule(-1,0,0,null,"Python Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_tail4", new ShipModule(-1,0,0,null,"Python Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_wings1", new ShipModule(-1,0,0,null,"Python Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_wings2", new ShipModule(-1,0,0,null,"Python Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_wings3", new ShipModule(-1,0,0,null,"Python Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit1_wings4", new ShipModule(-1,0,0,null,"Python Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_bumper1", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_bumper3", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_spoiler1", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_spoiler2", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_tail1", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_tail3", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_wings2", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "python_shipkit2raider_wings3", new ShipModule(-1,0,0,null,"Python Shipkit 2 Raider Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_bumper2", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Bumper 2", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_spoiler1", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Spoiler 1", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_tail1", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_tail3", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Tail 3", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_tail4", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_wings2", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_wings3", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "sidewinder_shipkit1_wings4", new ShipModule(-1,0,0,null,"Sidewinder Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "string_lights_coloured", new ShipModule(999999941,0,0,null,"String Lights Coloured", ShipModule.ModuleTypes.VanityType) },
            { "string_lights_thargoidprobe", new ShipModule(-1,0,0,null,"String Lights Thargoid probe", ShipModule.ModuleTypes.VanityType) },
            { "string_lights_warm_white", new ShipModule(999999944,0,0,null,"String Lights Warm White", ShipModule.ModuleTypes.VanityType) },
            { "type6_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Type 6 Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "type6_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Type 6 Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "type6_shipkit1_wings1", new ShipModule(-1,0,0,null,"Type 6 Shipkit 1 Wings 1", ShipModule.ModuleTypes.VanityType) },
            { "type9_military_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Type 9 Military Ship Kit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "type9_military_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Type 9 Military Ship Kit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "type9_military_shipkit1_wings3", new ShipModule(-1,0,0,null,"Type 9 Military Ship Kit 1 Wings 3", ShipModule.ModuleTypes.VanityType) },
            { "type9_military_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Type 9 Military Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "type9_military_shipkit1_spoiler2", new ShipModule(-1,0,0,null,"Type 9 Military Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType) },
            { "typex_3_shipkit1_bumper3", new ShipModule(-1,0,0,null,"Typex 3 Shipkit 1 Bumper 3", ShipModule.ModuleTypes.VanityType) },
            { "typex_3_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Typex 3 Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "typex_3_shipkit1_wings4", new ShipModule(-1,0,0,null,"Typex 3 Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "viper_shipkit1_bumper4", new ShipModule(-1,0,0,null,"Viper Shipkit 1 Bumper 4", ShipModule.ModuleTypes.VanityType) },
            { "viper_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Viper Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "viper_shipkit1_tail4", new ShipModule(-1,0,0,null,"Viper Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType) },
            { "viper_shipkit1_wings4", new ShipModule(-1,0,0,null,"Viper Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_verity", new ShipModule(999999901,0,0,null,"Voice Pack Verity", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_alix", new ShipModule(-1,0,0,null,"Voicepack Alix", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_amelie", new ShipModule(-1,0,0,null,"Voicepack Amelie", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_archer", new ShipModule(-1,0,0,null,"Voicepack Archer", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_carina", new ShipModule(-1,0,0,null,"Voicepack Carina", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_celeste", new ShipModule(999999904,0,0,null,"Voicepack Celeste", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_eden", new ShipModule(-1,0,0,null,"Voicepack Eden", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_gerhard", new ShipModule(-1,0,0,null,"Voicepack Gerhard", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_jefferson", new ShipModule(-1,0,0,null,"Voicepack Jefferson", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_leo", new ShipModule(-1,0,0,null,"Voicepack Leo", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_luciana", new ShipModule(-1,0,0,null,"Voicepack Luciana", ShipModule.ModuleTypes.VanityType) },
            { "voicepack_victor", new ShipModule(999999902,0,0,null,"Voicepack Victor", ShipModule.ModuleTypes.VanityType) },
            { "vulture_shipkit1_bumper1", new ShipModule(-1,0,0,null,"Vulture Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType) },
            { "vulture_shipkit1_spoiler3", new ShipModule(-1,0,0,null,"Vulture Shipkit 1 Spoiler 3", ShipModule.ModuleTypes.VanityType) },
            { "vulture_shipkit1_spoiler4", new ShipModule(-1,0,0,null,"Vulture Shipkit 1 Spoiler 4", ShipModule.ModuleTypes.VanityType) },
            { "vulture_shipkit1_tail1", new ShipModule(-1,0,0,null,"Vulture Shipkit 1 Tail 1", ShipModule.ModuleTypes.VanityType) },
            { "vulture_shipkit1_wings2", new ShipModule(-1,0,0,null,"Vulture Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_blue", new ShipModule(-1,0,0,null,"Weapon Customisation Blue", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_cyan", new ShipModule(-1,0,0,null,"Weapon Customisation Cyan", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_green", new ShipModule(-1,0,0,null,"Weapon Customisation Green", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_pink", new ShipModule(-1,0,0,null,"Weapon Customisation Pink", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_purple", new ShipModule(-1,0,0,null,"Weapon Customisation Purple", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_red", new ShipModule(-1,0,0,null,"Weapon Customisation Red", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_white", new ShipModule(-1,0,0,null,"Weapon Customisation White", ShipModule.ModuleTypes.VanityType) },
            { "weaponcustomisation_yellow", new ShipModule(-1,0,0,null,"Weapon Customisation Yellow", ShipModule.ModuleTypes.VanityType) },

            { "krait_mkii_shipkit1_tail4", new ShipModule(-1,0, "Krait Mkii Shipkit 1 Tail 4", ShipModule.ModuleTypes.VanityType ) },
            { "cutter_shipkit1_bumper1", new ShipModule(-1,0, "Cutter Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType ) },
            { "type6_shipkit1_spoiler2", new ShipModule(-1,0, "Type 6 Shipkit 1 Spoiler 2", ShipModule.ModuleTypes.VanityType ) },
            { "type6_shipkit1_wings4", new ShipModule(-1,0, "Type 6 Shipkit 1 Wings 4", ShipModule.ModuleTypes.VanityType ) },
            { "type6_shipkit1_wings3", new ShipModule(-1,0, "Type 6 Shipkit 1 Wings 3", ShipModule.ModuleTypes.VanityType ) },
            { "empire_courier_shipkit1_bumper1", new ShipModule(-1,0, "Empire Courier Shipkit 1 Bumper 1", ShipModule.ModuleTypes.VanityType ) },
            { "federation_corvette_shipkit1_wings2", new ShipModule(-1,0, "Federation Corvette Shipkit 1 Wings 2", ShipModule.ModuleTypes.VanityType ) },
            { "krait_light_shipkit1_tail2", new ShipModule(-1,0, "Krait Light Shipkit 1 Tail 2", ShipModule.ModuleTypes.VanityType ) },

            { "paint", new ShipModule(-1,0,0,null,"Paint", ShipModule.ModuleTypes.WearAndTearType) },
            { "all", new ShipModule(-1,0,0,null,"Repair All", ShipModule.ModuleTypes.WearAndTearType) },
            { "hull", new ShipModule(-1,0,0,null,"Repair All", ShipModule.ModuleTypes.WearAndTearType) },
            { "wear", new ShipModule(-1,0,0,null,"Wear", ShipModule.ModuleTypes.WearAndTearType) },
        };


        #endregion

        #region Synth Modules

        static private Dictionary<string, ShipModule> synthesisedmodules = new Dictionary<string, ShipModule>();        // ones made by edd

        #endregion

    }
}
