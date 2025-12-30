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

using BaseUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore
{
    public sealed class OrderedPropertyNameAttribute : Attribute
    {
        public int Order { get; set; }
        public string Text { get; set; }
        public OrderedPropertyNameAttribute(int n, string text) { Order = n; Text = text; }
    }

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
                    string candidatename = GenerateCandidateModuleName(fdid);

                    var newmodule = new ShipModule(-1, IsVanity(lowername) ? ShipModule.ModuleTypes.VanityType : ShipModule.ModuleTypes.UnknownType, candidatename);
                    string futilemessage = " - THIS IS NOT AN ERROR. This is item unknown to EDD and will be added automatically to your EDD module list. Please though report it to us so we can add it to known module lists";
                    System.Diagnostics.Trace.WriteLine($"*** Unknown Module {{ \"{lowername}\", new ShipModule(-1,{(IsVanity(lowername) ? "ShipModule.ModuleTypes.VanityType" : "ShipModule.ModuleTypes.UnknownType")},\"{candidatename}\") }}," + futilemessage);

                    synthesisedmodules[lowername] = m = newmodule;                   // lets cache them for completeness..
                }
            }

            return state;
        }

        internal static string GenerateCandidateModuleName(string fdid)
        {
            string candidatename = fdid.Replace("weaponcustomisation", "WeaponCustomisation").Replace("testbuggy", "SRV").
                                    Replace("enginecustomisation", "EngineCustomisation");

            candidatename = candidatename.SplitCapsWordFull();
            candidatename = candidatename.Replace("Paintjob", "Paint Job");
            candidatename = candidatename.Replace("Mkii", "Mk II");
            candidatename = candidatename.Replace("Mkiv", "Mk IV");
            candidatename = candidatename.Replace("mkiii", " Mk III");
            candidatename = candidatename.Replace("mkiv", " Mk IV");
            candidatename = candidatename.Replace("Python Nx", "Python Mk II");
            candidatename = candidatename.Replace("Typex 2", "Alliance Crusader");
            candidatename = candidatename.Replace("Typex 3", "Alliance Challenger");
            candidatename = candidatename.Replace("Empire Trader", "Imperial Clipper");
            candidatename = candidatename.Replace("Krait Light", "Krait Phantom");
            candidatename = candidatename.Replace("Ferdelance", "Fer De Lance");
            candidatename = candidatename.Replace("Type 9 Military ", "Type-10 Defender ");
            candidatename = candidatename.Replace("Type 9 Militarystripe", "Type-9 Military Stripe");
            candidatename = candidatename.Replace("Belugaliner", "Beluga Liner");
            candidatename = candidatename.Replace("Ppaisling", "PP Aisling");
            candidatename = candidatename.Replace("MK I Ii", "MK III");
            candidatename = candidatename.Replace("Highcolour", "High Colour");
            candidatename = candidatename.Replace("Blackfriday", "Black Friday");
            candidatename = candidatename.Replace("Shipkita", "Ship Kit A");
            candidatename = candidatename.Replace("Shipkitb", "Ship Kit B");
            candidatename = candidatename.Replace("Shipkitc", "Ship Kit C");
            candidatename = candidatename.Replace("Iridescentblack", "Iridescent Black");
            candidatename = candidatename.Replace("Militarystripe", "Military Stripe");
            candidatename = candidatename.Replace("Colourgeo", "Colour Geo");
            candidatename = candidatename.Replace(" Textamper", " Text Ampersand");
            candidatename = candidatename.Replace("Lowlighteffect", "Low Light Effect");
            candidatename = candidatename.Replace("Metallicdesign", "Metallic Design");
            candidatename = candidatename.Replace("Thargoidreward", "Thargoid Reward");
            candidatename = candidatename.Replace("Largelogometallic", "Large Logo Metallic");
            candidatename = candidatename.Replace("Blackmagenta", "Black Magenta");
            candidatename = candidatename.Replace("Blueorange", "Blue Orange");
            candidatename = candidatename.Replace("Greygreen", "Grey Green");
            candidatename = candidatename.Replace("Greyorange", "Grey Orange");
            candidatename = candidatename.Replace("Yellowblack", "Yellow Black");
            candidatename = candidatename.Replace("Iridescenthighcolour", "Iridescent High Colour");
            candidatename = candidatename.Replace("Panthermkii", "Panther Clipper Mk II");
            candidatename = candidatename.Replace("Pparissa", "PP Arissa");
            candidatename = candidatename.Replace("Ppyuri", "PP Yuri");
            candidatename = candidatename.Replace("Pp Aislingduval", "PP Aisling Duval");
            candidatename = candidatename.Replace("Pp Arissalavignyduval", "Pp Arissa Lavigny Duval");
            candidatename = candidatename.Replace("Pp Edmundmahon", "Pp Edmund Mahon");
            candidatename = candidatename.Replace("Pp Feliciawinters", "Pp Felicia Winters");
            candidatename = candidatename.Replace("Pp Jeromearcher", "Pp Jerome Archer");
            candidatename = candidatename.Replace("Pp Liyongrui", "Pp Li Yongrui");
            candidatename = candidatename.Replace("Pp Pranavantal", "Pp Prana Vantal");
            candidatename = candidatename.Replace("Pp Yurigrom", "Pp Yuri Grom");
            candidatename = candidatename.Replace("Explorer Nx", "Caspian Explorer");
            candidatename = candidatename.Replace(" lakonminer", " Type-11 Prospector", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("ownersclub", " Owners Club", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("brewercorporation", "Brewer Corporation", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 6", "Type-6", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 7", "Type-7", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 8", "Type-8", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 9", "Type-9", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 10", "Type-10", StringComparison.InvariantCultureIgnoreCase);
            candidatename = candidatename.Replace("Type 11", "Type-11", StringComparison.InvariantCultureIgnoreCase);
            return candidatename;
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

        private static string TXIT(string text)
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

        [System.Diagnostics.DebuggerDisplay("{EnglishModName} {ModType} {ModuleID} {Class} {Rating}")]
        public class ShipModule
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
                Colonisation,
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

                // Not buyable, DiscoveryScanner marks the first non buyable - see code below
                DiscoveryScanner, PrisonCells, DataLinkScanner, SRVScanner, FighterWeapon,
                VanityType, UnknownType, CockpitType, CargoBayDoorType, WearAndTearType, Codex,

                // marks it as a special effect modifier list not a module
                SpecialEffect,
            };

            public string EnglishModName { get; set; }     // english name
            public string TranslatedModName { get; set; }     // foreign name
            public int ModuleID { get; set; }
            public ModuleTypes ModType { get; set; }
            public bool IsBuyable { get { return !(ModType < ModuleTypes.DiscoveryScanner); } }

            public bool IsShieldGenerator { get { return ModType == ModuleTypes.PrismaticShieldGenerator || ModType == ModuleTypes.Bi_WeaveShieldGenerator || ModType == ModuleTypes.ShieldGenerator; } }
            public bool IsPowerDistributor { get { return ModType == ModuleTypes.PowerDistributor; } }
            public bool IsHardpoint { get { return Damage.HasValue && ModuleID != 128049522; } }        // Damage, but not point defense

            // string should be in spansh/EDCD csv compatible format, in english, as it it fed into Spansh
            public string EnglishModTypeString { get { return ModType.ToString().Replace("AX", "AX ").Replace("_", "-").SplitCapsWordFull(); } }

            public string TranslatedModTypeString
            {
                get
                {
                    string kn = EnglishModTypeString.Replace(" ", "_");     // use ModulePartNames if its there, else use ModuleTypeNames
                    return BaseUtils.Translator.Instance.Translate(EnglishModTypeString, BaseUtils.Translator.Instance.IsDefined("ModulePartNames." + kn) ? "ModulePartNames." + kn : "ModuleTypeNames." + kn);
                }
            }

            // printed in this order

            public int? Class { get; set; }     // handled specifically
            public string Rating { get; set; }

            // EDSY ordered

            [OrderedPropertyNameAttribute(0, "")] public string Mount { get; set; }                               // 'mount'
            [OrderedPropertyNameAttribute(1, "")] public string MissileType { get; set; }                         // 'missile'


            [OrderedPropertyNameAttribute(10, "t")] public double? Mass { get; set; }                              // 'mass' of module t
            [OrderedPropertyNameAttribute(11, "")] public double? Integrity { get; set; }                          // 'integ'
            [OrderedPropertyNameAttribute(12, "MW")] public double? PowerDraw { get; set; }                        // 'pwrdraw'
            [OrderedPropertyNameAttribute(13, "s")] public double? BootTime { get; set; }                          // 'boottime'


            [OrderedPropertyNameAttribute(20, "s")] public double? SCBSpinUp { get; set; }                        // SCBs
            [OrderedPropertyNameAttribute(21, "s")] public double? SCBDuration { get; set; }

            [OrderedPropertyNameAttribute(25, "%")] public double? PowerBonus { get; set; }                       // guardian power bonus

            [OrderedPropertyNameAttribute(30, "MW")] public double? WeaponsCapacity { get; set; }                  // 'wepcap' max MW power distributor
            [OrderedPropertyNameAttribute(31, "MW/s")] public double? WeaponsRechargeRate { get; set; }            // 'wepchg' power distributor rate MW/s
            [OrderedPropertyNameAttribute(32, "MW")] public double? EngineCapacity { get; set; }                   // 'engcap' max MW power distributor
            [OrderedPropertyNameAttribute(33, "MW/s")] public double? EngineRechargeRate { get; set; }             // 'engchg' power distributor rate MW/s
            [OrderedPropertyNameAttribute(34, "MW")] public double? SystemsCapacity { get; set; }                  // 'syscap' max MW power distributor
            [OrderedPropertyNameAttribute(35, "MW/s")] public double? SystemsRechargeRate { get; set; }            // 'syschg' power distributor rate MW/s

            [OrderedPropertyNameAttribute(40, "/s")] public double? DPS { get; set; }                          // 'dps'
            [OrderedPropertyNameAttribute(41, "")] public double? Damage { get; set; }                         // 'damage'
            [OrderedPropertyNameAttribute(42, "")] public int? DamageMultiplierFullCharge { get; set; }        // 'dmgmul' Guardian weapons

            [OrderedPropertyNameAttribute(50, "t")] public double? MinMass { get; set; }                       // 'fsdminmass' 'genminmass' 'engminmass'
            [OrderedPropertyNameAttribute(51, "t")] public double? OptMass { get; set; }                       // 'fsdoptmass' 'genoptmass' 'engoptmass'
            [OrderedPropertyNameAttribute(52, "t")] public double? MaxMass { get; set; }                       // 'fsdmaxmass' 'genmaxmass' 'engmaxmass'
            [OrderedPropertyNameAttribute(53, "")] public double? EngineMinMultiplier { get; set; }            // 'engminmul'
            [OrderedPropertyNameAttribute(54, "")] public double? EngineOptMultiplier { get; set; }            // 'engoptmul'
            [OrderedPropertyNameAttribute(55, "")] public double? EngineMaxMultiplier { get; set; }            // 'engmaxmul'

            [OrderedPropertyNameAttribute(60, "")] public double? MinStrength { get; set; }                    // 'genminmul'
            [OrderedPropertyNameAttribute(61, "")] public double? OptStrength { get; set; }                    // 'genoptmul' shields
            [OrderedPropertyNameAttribute(62, "")] public double? MaxStrength { get; set; }                    // 'genmaxmul' shields
            [OrderedPropertyNameAttribute(63, "/s")] public double? RegenRate { get; set; }                    // 'genrate' units/s
            [OrderedPropertyNameAttribute(64, "/s")] public double? BrokenRegenRate { get; set; }              // 'bgenrate' units/s
            [OrderedPropertyNameAttribute(65, "MW/Shot_or_s")] public double? DistributorDraw { get; set; }    // 'distdraw'


            [OrderedPropertyNameAttribute(80, "%")] public double? MinimumSpeedModifier { get; set; }           // enhanced thrusters
            [OrderedPropertyNameAttribute(81, "%")] public double? OptimalSpeedModifier { get; set; }
            [OrderedPropertyNameAttribute(82, "%")] public double? MaximumSpeedModifier { get; set; }
            [OrderedPropertyNameAttribute(83, "%")] public double? MinimumAccelerationModifier { get; set; }
            [OrderedPropertyNameAttribute(84, "%")] public double? OptimalAccelerationModifier { get; set; }
            [OrderedPropertyNameAttribute(85, "%")] public double? MaximumAccelerationModifier { get; set; }
            [OrderedPropertyNameAttribute(86, "%")] public double? MinimumRotationModifier { get; set; }
            [OrderedPropertyNameAttribute(87, "%")] public double? OptimalRotationModifier { get; set; }
            [OrderedPropertyNameAttribute(88, "%")] public double? MaximumRotationModifier { get; set; }

            [OrderedPropertyNameAttribute(90, "/s")] public double? ThermalLoad { get; set; }                    // 'engheat' 'fsdheat' 'thmload'

            [OrderedPropertyNameAttribute(100, "t")] public double? MaxFuelPerJump { get; set; }                // 'maxfuel'
            [OrderedPropertyNameAttribute(101, "")] public double? PowerConstant { get; set; }                  // 'fuelpower' Number
            [OrderedPropertyNameAttribute(102, "")] public double? LinearConstant { get; set; }                 // 'fuelmul' Number

            [OrderedPropertyNameAttribute(120, "%")] public double? SCOSpeedIncrease { get; set; }              // 'scospd' %
            [OrderedPropertyNameAttribute(121, "")] public double? SCOAccelerationRate { get; set; }            // 'scoacc' factor
            [OrderedPropertyNameAttribute(122, "")] public double? SCOHeatGenerationRate { get; set; }          // 'scoheat' factor
            [OrderedPropertyNameAttribute(123, "")] public double? SCOControlInterference { get; set; }         // 'scoconint' factor
            [OrderedPropertyNameAttribute(124, "t/s")] public double? SCOFuelDuringOvercharge { get; set; }     // 'scofuel' use during overdrive

            [OrderedPropertyNameAttribute(200, "%")] public double? HullStrengthBonus { get; set; }             // 'hullbst' % bonus over the ship information armour value
            [OrderedPropertyNameAttribute(201, "%")] public double? HullReinforcement { get; set; }             // 'hullrnf' units

            [OrderedPropertyNameAttribute(209, "%")] public double? ShieldReinforcement { get; set; }          // 'shieldrnf'

            [OrderedPropertyNameAttribute(210, "%")] public double? KineticResistance { get; set; }             // 'kinres' % bonus on base values
            [OrderedPropertyNameAttribute(211, "%")] public double? ThermalResistance { get; set; }             // 'thmres' % bonus on base values
            [OrderedPropertyNameAttribute(212, "%")] public double? ExplosiveResistance { get; set; }           // 'expres' % bonus on base values
            [OrderedPropertyNameAttribute(213, "%")] public double? AXResistance { get; set; }                  // 'axeres' % bonus on base values armour - not engineered
            [OrderedPropertyNameAttribute(214, "%")] public double? CausticResistance { get; set; }             // 'caures' % bonus on base values not armour
            [OrderedPropertyNameAttribute(215, "MW/u")] public double? MWPerUnit { get; set; }                  // 'genpwr' MW per shield unit

            [OrderedPropertyNameAttribute(220, "")] public double? ArmourPiercing { get; set; }                 // 'pierce'

            [OrderedPropertyNameAttribute(230, "m")] public double? Range { get; set; }                         // 'maximumrng' 'lpactrng' 'ecmrng' 'barrierrng' 'scanrng' 'maxrng'
            [OrderedPropertyNameAttribute(231, "deg")] public double? Angle { get; set; }                       // 'maxangle' 'scanangle' 'facinglim'
            [OrderedPropertyNameAttribute(232, "m")] public double? TypicalEmission { get; set; }               // 'typemis'

            [OrderedPropertyNameAttribute(240, "m")] public double? Falloff { get; set; }                       // 'dmgfall' m weapon fall off distance
            [OrderedPropertyNameAttribute(241, "m/s")] public double? Speed { get; set; }                       // 'shotspd' 'maxspd' m/s
            [OrderedPropertyNameAttribute(242, "/s")] public double? RateOfFire { get; set; }                   // 'rof' s weapon
            [OrderedPropertyNameAttribute(243, "shots/s")] public double? BurstRateOfFire { get; set; }         // 'bstrof'
            [OrderedPropertyNameAttribute(244, "s")] public double? BurstInterval { get; set; }                 // 'bstint' s weapon
            [OrderedPropertyNameAttribute(250, "shots")] public double? BurstSize { get; set; }                     // 'bstsize'
            [OrderedPropertyNameAttribute(251, "")] public int? Clip { get; set; }                              // 'ammoclip'
            [OrderedPropertyNameAttribute(252, "")] public int? Ammo { get; set; }                              // 'ammomax'
            [OrderedPropertyNameAttribute(255, "/shot")] public double? Rounds { get; set; }                    // 'rounds'
            [OrderedPropertyNameAttribute(256, "s")] public double? ReloadTime { get; set; }                    // 'ecmcool' 'rldtime' 'barriercool'

            [OrderedPropertyNameAttribute(260, "")] public double? BreachDamage { get; set; }                   // 'brcdmg'
            [OrderedPropertyNameAttribute(261, "%/FullI")] public double? BreachMin { get; set; }               // 'minbrc'
            [OrderedPropertyNameAttribute(262, "%/ZeroI")] public double? BreachMax { get; set; }               // 'maxbrc'
            [OrderedPropertyNameAttribute(263, "%")] public double? BreachModuleDamageAfterBreach { get; set; } // 'brcpct'

            [OrderedPropertyNameAttribute(280, "deg")] public double? Jitter { get; set; }                      // 'jitter'


            [OrderedPropertyNameAttribute(400, "%")] public double? AbsoluteProportionDamage { get; set; }      // 'abswgt'
            [OrderedPropertyNameAttribute(401, "%")] public double? KineticProportionDamage { get; set; }       // 'kinwgt'
            [OrderedPropertyNameAttribute(402, "%")] public double? ThermalProportionDamage { get; set; }       // 'thmwgt'
            [OrderedPropertyNameAttribute(403, "%")] public double? ExplosiveProportionDamage { get; set; }     // 'expwgt'
            [OrderedPropertyNameAttribute(404, "%")] public double? CausticPorportionDamage { get; set; }       // 'cauwgt'
            [OrderedPropertyNameAttribute(405, "%")] public double? AXPorportionDamage { get; set; }            // 'axewgt'


            [OrderedPropertyNameAttribute(420, "MW")] public double? PowerGen { get; set; }             // MW power plant
            [OrderedPropertyNameAttribute(421, "%")] public double? HeatEfficiency { get; set; } //% power plants

            [OrderedPropertyNameAttribute(440, "MW/s")] public double? MWPerSec { get; set; }                   // MW per sec shutdown field neutr


            [OrderedPropertyNameAttribute(450, "s")] public double? Time { get; set; }                          // 'jamdir' 'ecmdur' 'hsdur' 'duration' 'emgcylife' 'limpettime' 'scantime' 'barrierdur'


            [OrderedPropertyNameAttribute(500, "m")] public double? TargetRange { get; set; } // m w
            [OrderedPropertyNameAttribute(501, "")] public int? Limpets { get; set; }// collector controllers
            [OrderedPropertyNameAttribute(502, "s")] public double? HackTime { get; set; }// hatch breaker limpet
            [OrderedPropertyNameAttribute(503, "")] public int? MinCargo { get; set; } // hatch breaker limpet
            [OrderedPropertyNameAttribute(504, "")] public int? MaxCargo { get; set; } // hatch breaker limpet

            [OrderedPropertyNameAttribute(505, "/S")] public double? ThermalDrain { get; set; }                    // 'thmdrain'

            [OrderedPropertyNameAttribute(550, "")] public string CabinClass { get; set; }              // 'cabincls'

            [OrderedPropertyNameAttribute(551, "")] public int? Passengers { get; set; }

            [OrderedPropertyNameAttribute(560, "")] public double? AdditionalReinforcement { get; set; }        // shields additional strength, units guardian shield reinforcement

            [OrderedPropertyNameAttribute(600, "u/s")] public double? RateOfRepairConsumption { get; set; }
            [OrderedPropertyNameAttribute(601, "i/m")] public double? RepairCostPerMat { get; set; }

            [OrderedPropertyNameAttribute(620, "t")] public int? FuelTransfer { get; set; }

            [OrderedPropertyNameAttribute(630, "/s")] public double? RefillRate { get; set; } // t/s

            [OrderedPropertyNameAttribute(640, "")] public int? MaxRepairMaterialCapacity { get; set; }     // drone repair

            [OrderedPropertyNameAttribute(650, "m/s")] public double? MultiTargetSpeed { get; set; }        // drones

            [OrderedPropertyNameAttribute(660, "MW/use")] public double? ActivePower { get; set; }           //ECM

            // Any order

            [OrderedPropertyNameAttribute(999, "")] public double? Protection { get; set; }                         // 'dmgprot' multiplier
            [OrderedPropertyNameAttribute(999, "")] public int? Prisoners { get; set; }
            [OrderedPropertyNameAttribute(999, "")] public int? Capacity { get; set; }                          // 'bins' 'vslots'
            [OrderedPropertyNameAttribute(999, "t")] public int? Size { get; set; }                             // 'cargocap' 'fuelcap'
            [OrderedPropertyNameAttribute(999, "")] public int? Rebuilds { get; set; }                          // 'vcount'
            [OrderedPropertyNameAttribute(999, "ly")] public double? AdditionalRange { get; set; } // ly
            [OrderedPropertyNameAttribute(999, "s")] public double? TargetMaxTime { get; set; }
            [OrderedPropertyNameAttribute(999, "")] public double? MineBonus { get; set; }
            [OrderedPropertyNameAttribute(999, "")] public double? ProbeRadius { get; set; }
            [OrderedPropertyNameAttribute(999, "")] public string GuardianModuleResistance { get; set; }        // Active, blank

            // at end

            [OrderedPropertyNameAttribute(1000, "cr")] public int? Cost { get; set; }
            [OrderedPropertyNameAttribute(1001, "cr")] public int? AmmoCost { get; set; }

            [OrderedPropertyNameAttribute(2000, "")] public int FSDNeutronMultiplier => ModuleID == 129038968 ? 6 : 4;  // CASPIAN dodge until we decide, hopefully EDSY, encodes it in a variable

            const double WEAPON_CHARGE = 0.0;

            // look up one of the values above (use nameof()), or look up a special computed value
            // if not defined by module, return default value
            public double getEffectiveAttrValue(string attr, double defaultvalue = 1)
            {
                switch (attr)
                {
                    case "fpc":
                    case "sfpc":
                        {
                            var ammoclip = getEffectiveAttrValue(nameof(Clip), 0);
                            if (ammoclip != 0)
                            {
                                //if (modified && this.expid === 'wpnx_aulo')
                                //  ammoclip += ammoclip - 1;
                                return ammoclip;
                            }
                            else
                                return getEffectiveAttrValue(nameof(BurstSize), 1);
                        }

                    case "spc":
                    case "sspc":
                        {
                            var dmgmul = getEffectiveAttrValue(nameof(DamageMultiplierFullCharge), 0);      // if not there, 0
                            var duration = getEffectiveAttrValue(nameof(Time), 0) * (dmgmul != 0 ? WEAPON_CHARGE : 1.0);
                            if (ModuleID == 128671341 && attr == "spc") // TODO: bug? Imperial Hammer Rail Gun can keep firing through reloads without re-charging
                                duration = 0;
                            var bstsize = getEffectiveAttrValue(nameof(BurstSize), 1);      // if not there, from eddb.js, defaults are 1
                            var bstrof = getEffectiveAttrValue(nameof(BurstRateOfFire), 1);
                            var bstint = getEffectiveAttrValue(nameof(BurstInterval), 1);
                            var spc = (duration + (bstsize - 1) / bstrof + bstint);
                            var ammoclip = getEffectiveAttrValue(nameof(Clip), 0);
                            var rldtime = (attr == "sspc") ? getEffectiveAttrValue(nameof(ReloadTime), 0) : 0;
                            if (ammoclip != 0)
                            {
                                // Auto Loader adds 1 round per 2 shots for an almost-but-not-quite +100% effective clip size
                                //if (modified && this.expid === 'wpnx_aulo')
                                //    ammoclip += ammoclip - 1;
                                spc *= Math.Ceiling(ammoclip / bstsize);
                            }
                            var spcres = spc + Math.Max(0, rldtime - duration - bstint);
                            return spcres;
                        }

                    case "eps":     // energy per second
                    case "seps":
                        {
                            var distdraw = getEffectiveAttrValue(nameof(DistributorDraw), 0);
                            var rof = getEffectiveAttrValue(attr == "seps" ? "srof" : "rof");        // may be infinite
                            return distdraw * (double.IsInfinity(rof) ? 1 : rof);
                        }

                    case "dps":     // damage per second adjusted to firing rate
                    case "sdps":
                        {
                            var damage = getEffectiveAttrValue(nameof(Damage), 0);
                            var dmgmul = (1 + WEAPON_CHARGE * (getEffectiveAttrValue(nameof(DamageMultiplierFullCharge), 1) - 1));
                            var rounds = getEffectiveAttrValue(nameof(Rounds), 1);
                            var rof = getEffectiveAttrValue(attr == "sdps" ? "srof" : "rof");        // may be infinite
                            return (damage * dmgmul * rounds * (double.IsInfinity(rof) ? 1 : rof));
                        }

                    case "rof":     // may be Infinite
                        return getEffectiveAttrValue("fpc") / getEffectiveAttrValue("spc");

                    case "srof":    // may be Infinite - sustained
                        return getEffectiveAttrValue("sfpc") / getEffectiveAttrValue("sspc");

                    default:
                        var found = GetType().GetProperty(attr);
                        if (found != null)
                        {
                            var value = found.GetValue(this);
                            return value != null ? Convert.ToDouble(value) : defaultvalue;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false, $"Tried to find module attribute {attr} but does not exist");
                            return 0;
                        }
                }
            }

            public ShipModule()
            {
            }

            public ShipModule(ShipModule other)
            {
                // cheap way of doing a copy constructor without listing all those effing attributes

                foreach (System.Reflection.PropertyInfo pi in typeof(ShipModule).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (pi.CanWrite)
                        pi.SetValue(this, pi.GetValue(other));
                }
            }

            public string ToString(string separ = ", ")
            {
                StringBuilder output = new StringBuilder();
                if (Class != null && Rating != null)
                {
                    output.Append($"{Class}{Rating}");
                }
                else if (Rating != null || Class != null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                foreach (var kvp in GetPropertiesInOrder())
                {
                    string postfix = kvp.Value.Text;
                    dynamic value = kvp.Key.GetValue(this);
                    if (value != null)        // if not null, print value
                    {
                        if (output.Length > 0)
                            output.Append(separ);
                        string title = kvp.Key.Name.SplitCapsWord() + ':';
                        //if (namepadding > 0 && title.Length < namepadding)  title += new string(' ', namepadding - title.Length); // does not work due to prop font
                        output.Append(title);
                        if (value is string)
                            output.Append($"{value}{postfix}");
                        else
                            output.Append($"{value:0.####}{postfix}");
                    }
                }

                return output.ToString();
            }

            public static Dictionary<System.Reflection.PropertyInfo, OrderedPropertyNameAttribute> GetPropertiesInOrder()
            {
                lock (locker)   // do this once, and needs locking across threads
                {
                    if (PropertiesInOrder == null)
                    {
                        var props = typeof(ShipModule).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).
                                            Where(x => x.GetCustomAttributes(typeof(OrderedPropertyNameAttribute), true).Length > 0).
                                            ToArray();
                        var inorder = props.OrderBy(x => ((OrderedPropertyNameAttribute)(x.GetCustomAttributes(typeof(OrderedPropertyNameAttribute), true)[0])).Order).ToArray();

                        PropertiesInOrder = inorder.
                                            ToDictionary(x => x, y => (OrderedPropertyNameAttribute)(y.GetCustomAttributes(typeof(OrderedPropertyNameAttribute), true)[0]));
                    }
                }

                return PropertiesInOrder;
            }

            public ShipModule(int id, ModuleTypes modtype, string descr)
            {
                ModuleID = id; TranslatedModName = EnglishModName = descr; ModType = modtype;
            }

            private static Dictionary<System.Reflection.PropertyInfo, OrderedPropertyNameAttribute> PropertiesInOrder = null;
            private static Object locker = new object();
        }
        #endregion
    }
}
