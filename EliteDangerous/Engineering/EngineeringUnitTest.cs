/*t
 * Copyright © 2024-2024 EDDiscovery development team
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

// IF def it out - it adds over 300k to the DLL

//#define INCLUDE_ENGINEERING

using QuickJSON;
using BaseUtils;
using System;

namespace EliteDangerousCore
{
    // done this way, instead of a unit test harness, because you'd have to include baseutils etc into the SLN (like ExtendedControls) and it just makes it harder.

    public static class EngineeringUnitTest
    {
#if INCLUDE_ENGINEERING
        public static void UnitTest()
        {
            MaterialCommodityMicroResourceType.Initialise();     // lets statically fill the table way before anyone wants to access it
            ItemData.Initialise();

            {
                // slugshot error 19/7/24 due to EDSY not listing bstrof or bstsize

                string t = @"{""timestamp"":""2024-07-18T20:54:27Z"",""event"":""Loadout"",""Ship"":""python"",""ShipID"":153,""ShipName"":""Kajblood"",""ShipIdent"":""KJBLD"",""HullValue"":45842780,""ModulesValue"":163348357,""HullHealth"":1.0,""UnladenMass"":671.450012,""CargoCapacity"":286,""MaxJumpRange"":24.341875,""FuelCapacity"":{""Main"":32.0,""Reserve"":0.83},""Rebuy"":10459558,""Modules"":[{""Slot"":""LargeHardpoint1"",""Item"":""hpt_slugshot_fixed_large_range"",""On"":true,""Priority"":2,""AmmoInClip"":4,""AmmoInHopper"":180,""Health"":1.0,""Value"":1365812,""Engineering"":{""Engineer"":""Zacariah Nemo"",""EngineerID"":300050,""BlueprintID"":128673437,""BlueprintName"":""Weapon_DoubleShot"",""Level"":3,""Quality"":1.0,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":297.0,""OriginalValue"":216.0,""LessIsGood"":0},{""Label"":""MaximumRange"",""Value"":2820.0,""OriginalValue"":3000.0,""LessIsGood"":0},{""Label"":""RateOfFire"",""Value"":6.25,""OriginalValue"":4.545455,""LessIsGood"":0},{""Label"":""BurstRateOfFire"",""Value"":10.0,""OriginalValue"":-1.0,""LessIsGood"":0},{""Label"":""BurstSize"",""Value"":2.0,""OriginalValue"":1.0,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":4.0,""OriginalValue"":3.0,""LessIsGood"":0}]}},{""Slot"":""LargeHardpoint2"",""Item"":""hpt_slugshot_fixed_large_range"",""On"":true,""Priority"":2,""AmmoInClip"":4,""AmmoInHopper"":180,""Health"":1.0,""Value"":1365812,""Engineering"":{""Engineer"":""Zacariah Nemo"",""EngineerID"":300050,""BlueprintID"":128673437,""BlueprintName"":""Weapon_DoubleShot"",""Level"":3,""Quality"":1.0,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":297.0,""OriginalValue"":216.0,""LessIsGood"":0},{""Label"":""MaximumRange"",""Value"":2820.0,""OriginalValue"":3000.0,""LessIsGood"":0},{""Label"":""RateOfFire"",""Value"":6.25,""OriginalValue"":4.545455,""LessIsGood"":0},{""Label"":""BurstRateOfFire"",""Value"":10.0,""OriginalValue"":-1.0,""LessIsGood"":0},{""Label"":""BurstSize"",""Value"":2.0,""OriginalValue"":1.0,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":4.0,""OriginalValue"":3.0,""LessIsGood"":0}]}},{""Slot"":""LargeHardpoint3"",""Item"":""hpt_slugshot_fixed_large_range"",""On"":true,""Priority"":2,""AmmoInClip"":4,""AmmoInHopper"":180,""Health"":1.0,""Value"":1365812,""Engineering"":{""Engineer"":""Zacariah Nemo"",""EngineerID"":300050,""BlueprintID"":128673437,""BlueprintName"":""Weapon_DoubleShot"",""Level"":3,""Quality"":1.0,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":297.0,""OriginalValue"":216.0,""LessIsGood"":0},{""Label"":""MaximumRange"",""Value"":2820.0,""OriginalValue"":3000.0,""LessIsGood"":0},{""Label"":""RateOfFire"",""Value"":6.25,""OriginalValue"":4.545455,""LessIsGood"":0},{""Label"":""BurstRateOfFire"",""Value"":10.0,""OriginalValue"":-1.0,""LessIsGood"":0},{""Label"":""BurstSize"",""Value"":2.0,""OriginalValue"":1.0,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":4.0,""OriginalValue"":3.0,""LessIsGood"":0}]}},{""Slot"":""MediumHardpoint1"",""Item"":""hpt_drunkmissilerack_fixed_medium"",""On"":true,""Priority"":2,""AmmoInClip"":19,""AmmoInHopper"":182,""Health"":1.0,""Value"":749385,""Engineering"":{""Engineer"":""Liz Ryder"",""EngineerID"":300080,""BlueprintID"":128673476,""BlueprintName"":""Weapon_HighCapacity"",""Level"":2,""Quality"":1.0,""Modifiers"":[{""Label"":""Mass"",""Value"":5.2,""OriginalValue"":4.0,""LessIsGood"":1},{""Label"":""PowerDraw"",""Value"":1.296,""OriginalValue"":1.2,""LessIsGood"":1},{""Label"":""DamagePerSecond"",""Value"":62.500004,""OriginalValue"":60.0,""LessIsGood"":0},{""Label"":""RateOfFire"",""Value"":2.083333,""OriginalValue"":2.0,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":19.0,""OriginalValue"":12.0,""LessIsGood"":0},{""Label"":""AmmoMaximum"",""Value"":182.0,""OriginalValue"":120.0,""LessIsGood"":0}]}},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_drunkmissilerack_fixed_medium"",""On"":true,""Priority"":2,""AmmoInClip"":23,""AmmoInHopper"":220,""Health"":1.0,""Value"":749385,""Engineering"":{""Engineer"":""Liz Ryder"",""EngineerID"":300080,""BlueprintID"":128673478,""BlueprintName"":""Weapon_HighCapacity"",""Level"":4,""Quality"":0.9706,""Modifiers"":[{""Label"":""Mass"",""Value"":6.0,""OriginalValue"":4.0,""LessIsGood"":1},{""Label"":""PowerDraw"",""Value"":1.392,""OriginalValue"":1.2,""LessIsGood"":1},{""Label"":""DamagePerSecond"",""Value"":65.217392,""OriginalValue"":60.0,""LessIsGood"":0},{""Label"":""RateOfFire"",""Value"":2.173913,""OriginalValue"":2.0,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":23.0,""OriginalValue"":12.0,""LessIsGood"":0},{""Label"":""AmmoMaximum"",""Value"":220.0,""OriginalValue"":120.0,""LessIsGood"":0}]}},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_chafflauncher_tiny"",""On"":true,""Priority"":1,""AmmoInClip"":1,""AmmoInHopper"":10,""Health"":1.0,""Value"":7045},{""Slot"":""TinyHardpoint2"",""Item"":""hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":3,""AmmoInClip"":12,""AmmoInHopper"":10000,""Health"":1.0,""Value"":15371},{""Slot"":""TinyHardpoint3"",""Item"":""hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":3,""AmmoInClip"":1,""AmmoInHopper"":2,""Health"":1.0,""Value"":2901},{""Slot"":""TinyHardpoint4"",""Item"":""hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":3,""AmmoInClip"":1,""AmmoInHopper"":2,""Health"":1.0,""Value"":2901},{""Slot"":""PaintJob"",""Item"":""paintjob_python_egypt_01"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Decal1"",""Item"":""decal_triple_elite"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Decal2"",""Item"":""decal_triple_elite"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Decal3"",""Item"":""decal_triple_elite"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipName0"",""Item"":""nameplate_empire02_white"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipName1"",""Item"":""nameplate_empire02_white"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipID0"",""Item"":""nameplate_shipid_grey"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipID1"",""Item"":""nameplate_shipid_grey"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Armour"",""Item"":""python_armour_reactive"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":130940128,""Engineering"":{""Engineer"":""Petra Olmanova"",""EngineerID"":300130,""BlueprintID"":128673643,""BlueprintName"":""Armour_HeavyDuty"",""Level"":4,""Quality"":0.91,""Modifiers"":[{""Label"":""Mass"",""Value"":66.25,""OriginalValue"":53.0,""LessIsGood"":1},{""Label"":""DefenceModifierHealthMultiplier"",""Value"":343.835022,""OriginalValue"":250.0,""LessIsGood"":0},{""Label"":""KineticResistance"",""Value"":27.932501,""OriginalValue"":25.0,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":-34.526001,""OriginalValue"":-39.999996,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":23.127996,""OriginalValue"":19.999998,""LessIsGood"":0}]}},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class2"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":1194423},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class5"",""On"":true,""Priority"":0,""Health"":1.0,""Value"":13408787,""Engineering"":{""Engineer"":""Professor Palin"",""EngineerID"":300220,""BlueprintID"":128673656,""BlueprintName"":""Engine_Dirty"",""Level"":2,""Quality"":0.9129,""Modifiers"":[{""Label"":""Integrity"",""Value"":116.559998,""OriginalValue"":124.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":8.013599,""OriginalValue"":7.56,""LessIsGood"":1},{""Label"":""EngineOptimalMass"",""Value"":1368.0,""OriginalValue"":1440.0,""LessIsGood"":0},{""Label"":""EngineOptPerformance"",""Value"":118.389999,""OriginalValue"":100.0,""LessIsGood"":0},{""Label"":""EngineHeatRate"",""Value"":1.69,""OriginalValue"":1.3,""LessIsGood"":1}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_overcharge_size5_class2"",""On"":true,""Priority"":0,""Health"":1.0,""Value"":1990542,""Engineering"":{""Engineer"":""Elvira Martuuk"",""EngineerID"":300160,""BlueprintID"":128673692,""BlueprintName"":""FSD_LongRange"",""Level"":3,""Quality"":0.869,""Modifiers"":[{""Label"":""Mass"",""Value"":9.6,""OriginalValue"":8.0,""LessIsGood"":1},{""Label"":""Integrity"",""Value"":100.099998,""OriginalValue"":110.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":0.545,""OriginalValue"":0.5,""LessIsGood"":1},{""Label"":""FSDOptimalMass"",""Value"":1403.744995,""OriginalValue"":1050.0,""LessIsGood"":0}]}},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class2"",""On"":true,""Priority"":3,""Health"":1.0,""Value"":23516},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class5"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":8065334,""Engineering"":{""Engineer"":""The Dweller"",""EngineerID"":300180,""BlueprintID"":128673739,""BlueprintName"":""PowerDistributor_HighFrequency"",""Level"":5,""Quality"":1.0,""Modifiers"":[{""Label"":""WeaponsCapacity"",""Value"":57.950001,""OriginalValue"":61.0,""LessIsGood"":0},{""Label"":""WeaponsRecharge"",""Value"":8.845,""OriginalValue"":6.1,""LessIsGood"":0},{""Label"":""EnginesCapacity"",""Value"":38.950001,""OriginalValue"":41.0,""LessIsGood"":0},{""Label"":""EnginesRecharge"",""Value"":5.8,""OriginalValue"":4.0,""LessIsGood"":0},{""Label"":""SystemsCapacity"",""Value"":38.950001,""OriginalValue"":41.0,""LessIsGood"":0},{""Label"":""SystemsRecharge"",""Value"":5.8,""OriginalValue"":4.0,""LessIsGood"":0}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":2,""Health"":1.0,""Value"":73740},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":81013},{""Slot"":""Slot01_Size6"",""Item"":""int_cargorack_size6_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":300498},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size6_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":300498},{""Slot"":""Slot03_Size6"",""Item"":""int_cargorack_size6_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":300498},{""Slot"":""Slot04_Size5"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":92460},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":92462},{""Slot"":""Slot06_Size4"",""Item"":""int_corrosionproofcargorack_size4_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":91970},{""Slot"":""Slot07_Size3"",""Item"":""int_shieldgenerator_size3_class5_strong"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":742822,""Engineering"":{""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673838,""BlueprintName"":""ShieldGenerator_Reinforced"",""Level"":4,""Quality"":0.9733,""Modifiers"":[{""Label"":""ShieldGenStrength"",""Value"":197.865005,""OriginalValue"":150.0,""LessIsGood"":0},{""Label"":""BrokenRegenRate"",""Value"":1.17,""OriginalValue"":1.3,""LessIsGood"":0},{""Label"":""EnergyPerRegen"",""Value"":0.66,""OriginalValue"":0.6,""LessIsGood"":1},{""Label"":""KineticResistance"",""Value"":48.051994,""OriginalValue"":39.999996,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":-3.89601,""OriginalValue"":-20.000004,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":56.709999,""OriginalValue"":50.0,""LessIsGood"":0}]}},{""Slot"":""Slot08_Size3"",""Item"":""int_cargorack_size3_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":10299},{""Slot"":""Slot09_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":2694},{""Slot"":""Slot10_Size1"",""Item"":""int_corrosionproofcargorack_size1_class2"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":12249},{""Slot"":""PlanetaryApproachSuite"",""Item"":""int_planetapproachsuite_advanced"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Bobble01"",""Item"":""bobble_plant_rosequartz"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Bobble08"",""Item"":""bobble_plant_anemone"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Bobble09"",""Item"":""bobble_plant_succulent"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Bobble10"",""Item"":""bobble_plant_aloe"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitSpoiler"",""Item"":""python_shipkit1_spoiler3"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitWings"",""Item"":""python_shipkit1_wings3"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitTail"",""Item"":""python_shipkit1_tail2"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitBumper"",""Item"":""python_shipkit1_bumper1"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""WeaponColour"",""Item"":""weaponcustomisation_green"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""EngineColour"",""Item"":""enginecustomisation_green"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""VesselVoice"",""Item"":""voicepack_carina"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipCockpit"",""Item"":""python_cockpit"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":4,""Health"":1.0}]}";
                var mod = GetModule(t, ShipSlots.Slot.LargeHardpoint1, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(8));
                Check(mod.Integrity.Value.ApproxEqualsPercent(64));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.02));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(297));
                Check(mod.Damage.Value.ApproxEqualsPercent(3.96));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.57));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(1.13));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(45));
                Check(mod.Range.Value.ApproxEqualsPercent(2820));
                Check(mod.Falloff.Value.ApproxEqualsPercent(2800));
                Check(mod.Speed.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(6.25));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.22));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(10));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(2));
                Check(mod.Clip == 4);
                Check(mod.Ammo == 180);
                Check(mod.Rounds == 12);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(3.6));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(1.7));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(100));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(0));

            }

            {
                // gauss cannon no eng
                string t = @"{""event"":""Loadout"",""Ship"":""python"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":55316050,""ModulesValue"":1357640,""UnladenMass"":622,""CargoCapacity"":0,""MaxJumpRange"":9.186911,""FuelCapacity"":{""Main"":32,""Reserve"":0.83},""Rebuy"":2833684,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""LargeHardpoint1"",""Item"":""hpt_guardian_gausscannon_fixed_small"",""On"":true,""Priority"":0,""Value"":167250},{""Slot"":""Armour"",""Item"":""python_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.LargeHardpoint1, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.91));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(19.7));
                Check(mod.Damage.Value.ApproxEqualsPercent(40));
                Check(mod.Time.Value.ApproxEqualsPercent(1.2));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(3.8));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(15));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(140));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1500));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.4926));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.83));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(1));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(20));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(20));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(40));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.AXPorportionDamage.Value.ApproxEqualsPercent(50));
            }

            {
                // gauss cannon rapid fire
                string t = @"{""event"":""Loadout"",""Ship"":""python"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":55316050,""ModulesValue"":1357640,""UnladenMass"":622,""CargoCapacity"":0,""MaxJumpRange"":9.186911,""FuelCapacity"":{""Main"":32,""Reserve"":0.83},""Rebuy"":2833684,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""LargeHardpoint1"",""Item"":""hpt_guardian_gausscannon_fixed_small"",""On"":true,""Priority"":0,""Value"":167250,""Engineering"":{""BlueprintName"":""Weapon_RapidFire"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":22.825565,""OriginalValue"":19.704433},{""Label"":""Damage"",""Value"":38,""OriginalValue"":40},{""Label"":""DistributorDraw"",""Value"":2.47,""OriginalValue"":3.8},{""Label"":""RateOfFire"",""Value"":0.600673,""OriginalValue"":0.492611}]}},{""Slot"":""Armour"",""Item"":""python_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.LargeHardpoint1, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.91));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(22.83));
                Check(mod.Damage.Value.ApproxEqualsPercent(38));
                Check(mod.Time.Value.ApproxEqualsPercent(1.2));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2.47));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(15));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(140));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1500));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6007));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.4658));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(1));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(19));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(20));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(40));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.AXPorportionDamage.Value.ApproxEqualsPercent(50));
            }


            {
                // this one has GuardianModuleResistance - check its ignored nicely
                string t = @"{""timestamp"":""2024-06-23T20:05:48Z"",""event"":""Loadout"",""Ship"":""typex"",""ShipID"":278,""ShipName"":"""",""ShipIdent"":""SK-09T"",""HullHealth"":1.0,""UnladenMass"":724.900024,""CargoCapacity"":16,""MaxJumpRange"":16.874365,""FuelCapacity"":{""Main"":16.0,""Reserve"":0.77},""Rebuy"":0,""Modules"":[{""Slot"":""LargeHardpoint1"",""Item"":""hpt_atmulticannon_gimbal_large"",""On"":true,""Priority"":2,""AmmoInClip"":100,""AmmoInHopper"":2100,""Health"":1.0},{""Slot"":""LargeHardpoint2"",""Item"":""hpt_atmulticannon_gimbal_large"",""On"":true,""Priority"":2,""AmmoInClip"":100,""AmmoInHopper"":2100,""Health"":1.0},{""Slot"":""MediumHardpoint1"",""Item"":""hpt_atventdisruptorpylon_fixed_medium"",""On"":true,""Priority"":2,""AmmoInClip"":1,""AmmoInHopper"":64,""Health"":1.0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_guardian_gausscannon_fixed_small"",""On"":true,""Priority"":2,""AmmoInClip"":1,""AmmoInHopper"":80,""Health"":1.0,""Engineering"":{""Engineer"":""Ram Tah"",""EngineerID"":300110,""BlueprintID"":129030458,""BlueprintName"":""GuardianWeapon_Sturdy"",""Level"":1,""Quality"":0.0,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":21.204821,""OriginalValue"":26.506025,""LessIsGood"":0},{""Label"":""Damage"",""Value"":17.6,""OriginalValue"":22.0,""LessIsGood"":0},{""Label"":""GuardianModuleResistance"",""ValueStr"":""$INT_PANEL_module_active;"",""ValueStr_Localised"":""Active""}]}},{""Slot"":""SmallHardpoint3"",""Item"":""hpt_guardian_gausscannon_fixed_small"",""On"":true,""Priority"":2,""AmmoInClip"":1,""AmmoInHopper"":80,""Health"":1.0,""Engineering"":{""Engineer"":""Ram Tah"",""EngineerID"":300110,""BlueprintID"":129030458,""BlueprintName"":""GuardianWeapon_Sturdy"",""Level"":1,""Quality"":0.0,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":21.204821,""OriginalValue"":26.506025,""LessIsGood"":0},{""Label"":""Damage"",""Value"":17.6,""OriginalValue"":22.0,""LessIsGood"":0},{""Label"":""GuardianModuleResistance"",""ValueStr"":""$INT_PANEL_module_active;"",""ValueStr_Localised"":""Active""}]}},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":2,""AmmoInClip"":1,""AmmoInHopper"":2,""Health"":1.0},{""Slot"":""TinyHardpoint2"",""Item"":""hpt_causticsinklauncher_turret_tiny"",""On"":true,""Priority"":2,""AmmoInClip"":1,""AmmoInHopper"":5,""Health"":1.0},{""Slot"":""TinyHardpoint3"",""Item"":""hpt_xenoscannermk2_basic_tiny"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""TinyHardpoint4"",""Item"":""hpt_antiunknownshutdown_tiny_v2"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""PaintJob"",""Item"":""paintjob_typex_iridescentblack_04"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Armour"",""Item"":""typex_armour_grade3"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size6_class5"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class5"",""On"":true,""Priority"":2,""Health"":1.0,""Engineering"":{""Engineer"":""Professor Palin"",""EngineerID"":300220,""BlueprintID"":128673659,""BlueprintName"":""Engine_Dirty"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_engine_overloaded"",""ExperimentalEffect_Localised"":""Drag Drives"",""Modifiers"":[{""Label"":""Integrity"",""Value"":105.400002,""OriginalValue"":124.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":8.4672,""OriginalValue"":7.56,""LessIsGood"":1},{""Label"":""EngineOptimalMass"",""Value"":1260.0,""OriginalValue"":1440.0,""LessIsGood"":0},{""Label"":""EngineOptPerformance"",""Value"":145.599991,""OriginalValue"":100.0,""LessIsGood"":0},{""Label"":""EngineHeatRate"",""Value"":2.288,""OriginalValue"":1.3,""LessIsGood"":1}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class5"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class2"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size6_class5"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""Radar"",""Item"":""int_sensors_size4_class2"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size4_class3"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Slot01_Size6"",""Item"":""int_shieldgenerator_size6_class3_fast"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""Slot02_Size5"",""Item"":""int_hullreinforcement_size5_class2"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Slot03_Size4"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Slot04_Size2"",""Item"":""int_repairer_size2_class5"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""Slot05_Size2"",""Item"":""int_dronecontrol_repair_size1_class5"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""Slot06_Size1"",""Item"":""int_dronecontrol_unkvesselresearch"",""On"":true,""Priority"":2,""Health"":1.0},{""Slot"":""Military01"",""Item"":""int_modulereinforcement_size4_class2"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Military02"",""Item"":""int_hullreinforcement_size4_class2"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""Military03"",""Item"":""int_hullreinforcement_size4_class2"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""PlanetaryApproachSuite"",""Item"":""int_planetapproachsuite_advanced"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitSpoiler"",""Item"":""typex_shipkit1_spoiler4"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitWings"",""Item"":""typex_shipkit1_wings1"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipKitBumper"",""Item"":""typex_shipkit1_bumper3"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""VesselVoice"",""Item"":""voicepack_verity"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""ShipCockpit"",""Item"":""typex_cockpit"",""On"":true,""Priority"":1,""Health"":1.0},{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":2,""Health"":1.0}]}";
                GetModule(t, ShipSlots.Slot.SmallHardpoint2, true);
            }

            {
                // larger gauss cannon rapid fire
                string t = @"{""event"":""Loadout"",""Ship"":""python"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":55316050,""ModulesValue"":1734190,""UnladenMass"":624,""CargoCapacity"":0,""MaxJumpRange"":9.15762,""FuelCapacity"":{""Main"":32,""Reserve"":0.83},""Rebuy"":2852512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""LargeHardpoint2"",""Item"":""hpt_guardian_gausscannon_fixed_medium"",""On"":true,""Priority"":0,""Value"":543800,""Engineering"":{""BlueprintName"":""Weapon_RapidFire"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":39.944738,""OriginalValue"":34.482759},{""Label"":""Damage"",""Value"":66.5,""OriginalValue"":70},{""Label"":""DistributorDraw"",""Value"":4.68,""OriginalValue"":7.2},{""Label"":""RateOfFire"",""Value"":0.600673,""OriginalValue"":0.492611}]}},{""Slot"":""Armour"",""Item"":""python_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.LargeHardpoint2, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(42));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(2.61));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(39.94));
                Check(mod.Damage.Value.ApproxEqualsPercent(66.5));
                Check(mod.Time.Value.ApproxEqualsPercent(1.2));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(4.68));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(25));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(140));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1500));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6007));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.4658));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(1));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(33.25));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(20));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(40));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.AXPorportionDamage.Value.ApproxEqualsPercent(50));


            }

            {
                // robbies FDL
                string t = @"{ ""timestamp"":""2024 - 06 - 28T09: 20:18.255Z"",""event"":""Loadout"",""Ship"":""FerDeLance"",""ShipID"":15,""ShipName"":""Intrepid"",""ShipIdent"":""RXP-2"",""HullValue"":51232230,""ModulesValue"":75512490,""HullHealth"":1.0,""UnladenMass"":512.199951,""CargoCapacity"":16,""FuelCapacity"":{ ""Main"":8.0,""Reserve"":0.67},""Rebuy"":4752930,""Modules"":[{""Slot"":""Armour"",""Item"":""ferdelance_Armour_grade3"",""On"":true,""Priority"":1,""Value"":39448786,""Engineering"":{""Engineer"":""Selene Jean"",""EngineerID"":300210,""BlueprintID"":128673644,""BlueprintName"":""Armour_HeavyDuty"",""Level"":5,""Quality"":0.74,""Modifiers"":[{""Label"":""Mass"",""Value"":49.399998,""OriginalValue"":38.0,""LessIsGood"":1},{""Label"":""DefenceModifierHealthMultiplier"",""Value"":358.5,""OriginalValue"":250.0,""LessIsGood"":0},{""Label"":""KineticResistance"",""Value"":-14.312005,""OriginalValue"":-20.000004,""LessIsGood"":0},{ ""Label"":""ThermicResistance"",""Value"":4.74,""OriginalValue"":0.0,""LessIsGood"":0},{ ""Label"":""ExplosiveResistance"",""Value"":-33.363998,""OriginalValue"":-39.999996,""LessIsGood"":0}]}},{ ""Slot"":""CargoHatch"",""Item"":""modularcargobaydoorfdl"",""On"":true,""Priority"":4},{ ""Slot"":""Decal1"",""Item"":""Decal_Combat_Dangerous"",""On"":true,""Priority"":1},{ ""Slot"":""Decal3"",""Item"":""Decal_Combat_Dangerous"",""On"":true,""Priority"":1},{ ""Slot"":""Decal2"",""Item"":""decal_explorer_elite"",""On"":true,""Priority"":1},{ ""Slot"":""FrameShiftDrive"",""Item"":""Int_hyperdrive_size4_class5"",""On"":true,""Priority"":0,""Value"":1610080,""Engineering"":{ ""Engineer"":""Professor Palin"",""EngineerID"":300220,""BlueprintID"":128673692,""BlueprintName"":""FSD_LongRange"",""Level"":3,""Quality"":0.888,""Modifiers"":[{ ""Label"":""Mass"",""Value"":12.0,""OriginalValue"":10.0,""LessIsGood"":1},{ ""Label"":""Integrity"",""Value"":91.0,""OriginalValue"":100.0,""LessIsGood"":0},{ ""Label"":""PowerDraw"",""Value"":0.4905,""OriginalValue"":0.45,""LessIsGood"":1},{ ""Label"":""FSDOptimalMass"",""Value"":702.869995,""OriginalValue"":525.0,""LessIsGood"":0}]} },{ ""Slot"":""FuelTank"",""Item"":""Int_fueltank_size3_class3"",""On"":true,""Priority"":1,""Value"":7063},{ ""Slot"":""HugeHardpoint1"",""Item"":""Hpt_multicannon_gimbal_huge"",""On"":true,""Priority"":0,""Value"":6377600,""Engineering"":{ ""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""BlueprintID"":128673504,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":0.362,""Modifiers"":[{ ""Label"":""DamagePerSecond"",""Value"":38.12291,""OriginalValue"":23.299664,""LessIsGood"":0},{ ""Label"":""Damage"",""Value"":5.661252,""OriginalValue"":3.46,""LessIsGood"":0},{ ""Label"":""DistributorDraw"",""Value"":0.4995,""OriginalValue"":0.37,""LessIsGood"":1},{ ""Label"":""ThermalLoad"",""Value"":0.5865,""OriginalValue"":0.51,""LessIsGood"":1},{ ""Label"":""AmmoClipSize"",""Value"":77.0,""OriginalValue"":90.0,""LessIsGood"":0}]} },{ ""Slot"":""LifeSupport"",""Item"":""Int_lifesupport_size4_class2"",""On"":true,""Priority"":0,""Value"":28373},{ ""Slot"":""MediumHardpoint1"",""Item"":""Hpt_beamlaser_gimbal_medium"",""On"":true,""Priority"":4,""Value"":500600,""Engineering"":{ ""Engineer"":""Broo Tarquin"",""EngineerID"":300030,""BlueprintID"":128739086,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":0.291,""Modifiers"":[{ ""Label"":""DamagePerSecond"",""Value"":20.396334,""OriginalValue"":12.52,""LessIsGood"":0},{ ""Label"":""DistributorDraw"",""Value"":4.644,""OriginalValue"":3.44,""LessIsGood"":1},{ ""Label"":""ThermalLoad"",""Value"":6.118,""OriginalValue"":5.32,""LessIsGood"":1}]} },{ ""Slot"":""MediumHardpoint2"",""Item"":""Hpt_beamlaser_gimbal_medium"",""On"":true,""Priority"":0,""Value"":500600,""Engineering"":{ ""Engineer"":""Broo Tarquin"",""EngineerID"":300030,""BlueprintID"":128739086,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":0.284,""Modifiers"":[{ ""Label"":""DamagePerSecond"",""Value"":20.387569,""OriginalValue"":12.52,""LessIsGood"":0},{ ""Label"":""DistributorDraw"",""Value"":4.644,""OriginalValue"":3.44,""LessIsGood"":1},{ ""Label"":""ThermalLoad"",""Value"":6.118,""OriginalValue"":5.32,""LessIsGood"":1}]} },{ ""Slot"":""Slot01_Size5"",""Item"":""Int_shieldgenerator_size5_class5"",""On"":true,""Priority"":0,""Value"":4338361,""Engineering"":{ ""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673838,""BlueprintName"":""ShieldGenerator_Reinforced"",""Level"":4,""Quality"":0.9617,""Modifiers"":[{ ""Label"":""ShieldGenStrength"",""Value"":158.124008,""OriginalValue"":120.000008,""LessIsGood"":0},{ ""Label"":""BrokenRegenRate"",""Value"":3.375,""OriginalValue"":3.75,""LessIsGood"":0},{ ""Label"":""EnergyPerRegen"",""Value"":0.66,""OriginalValue"":0.6,""LessIsGood"":1},{ ""Label"":""KineticResistance"",""Value"":48.051994,""OriginalValue"":39.999996,""LessIsGood"":0},{ ""Label"":""ThermicResistance"",""Value"":-3.89601,""OriginalValue"":-20.000004,""LessIsGood"":0},{ ""Label"":""ExplosiveResistance"",""Value"":56.709999,""OriginalValue"":50.0,""LessIsGood"":0}]} },{ ""Slot"":""Slot02_Size4"",""Item"":""Int_shieldcellbank_size4_class4"",""On"":true,""Priority"":3,""Value"":177331},{ ""Slot"":""PaintJob"",""Item"":""PaintJob_FerDeLance_BlackFriday_01"",""On"":true,""Priority"":1},{ ""Slot"":""PlanetaryApproachSuite"",""Item"":""Int_planetapproachsuite_advanced"",""On"":true,""Priority"":1},{ ""Slot"":""PowerDistributor"",""Item"":""Int_powerdistributor_size6_class5"",""On"":true,""Priority"":0,""Value"":3475688,""Engineering"":{ ""Engineer"":""The Dweller"",""EngineerID"":300180,""BlueprintID"":128673739,""BlueprintName"":""PowerDistributor_HighFrequency"",""Level"":5,""Quality"":0.2822,""Modifiers"":[{ ""Label"":""WeaponsCapacity"",""Value"":47.5,""OriginalValue"":50.0,""LessIsGood"":0},{ ""Label"":""WeaponsRecharge"",""Value"":7.20408,""OriginalValue"":5.2,""LessIsGood"":0},{ ""Label"":""EnginesCapacity"",""Value"":33.25,""OriginalValue"":35.0,""LessIsGood"":0},{ ""Label"":""EnginesRecharge"",""Value"":4.43328,""OriginalValue"":3.2,""LessIsGood"":0},{ ""Label"":""SystemsCapacity"",""Value"":33.25,""OriginalValue"":35.0,""LessIsGood"":0},{ ""Label"":""SystemsRecharge"",""Value"":4.43328,""OriginalValue"":3.2,""LessIsGood"":0}]} },{ ""Slot"":""PowerPlant"",""Item"":""Int_powerplant_size6_class5"",""On"":true,""Priority"":1,""Value"":13752602},{ ""Slot"":""Radar"",""Item"":""Int_sensors_size4_class2"",""On"":true,""Priority"":0,""Value"":28373,""Engineering"":{ ""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128740136,""BlueprintName"":""Sensor_LongRange"",""Level"":5,""Quality"":0.3273,""Modifiers"":[{ ""Label"":""Mass"",""Value"":8.0,""OriginalValue"":4.0,""LessIsGood"":1},{ ""Label"":""SensorTargetScanAngle"",""Value"":21.0,""OriginalValue"":30.0,""LessIsGood"":0},{ ""Label"":""Range"",""Value"":8311.463867,""OriginalValue"":5040.0,""LessIsGood"":0}]} },{ ""Slot"":""ShipCockpit"",""Item"":""ferdelance_cockpit"",""On"":true,""Priority"":1},{ ""Slot"":""MainEngines"",""Item"":""Int_engine_size5_class5"",""On"":true,""Priority"":0,""Value"":4338361,""Engineering"":{ ""Engineer"":""Professor Palin"",""EngineerID"":300220,""BlueprintID"":128673659,""BlueprintName"":""Engine_Dirty"",""Level"":5,""Quality"":0.9757,""Modifiers"":[{ ""Label"":""Integrity"",""Value"":90.100006,""OriginalValue"":106.0,""LessIsGood"":0},{ ""Label"":""PowerDraw"",""Value"":6.8544,""OriginalValue"":6.12,""LessIsGood"":1},{ ""Label"":""EngineOptimalMass"",""Value"":735.0,""OriginalValue"":840.0,""LessIsGood"":0},{ ""Label"":""EngineOptPerformance"",""Value"":139.829987,""OriginalValue"":100.0,""LessIsGood"":0},{ ""Label"":""EngineHeatRate"",""Value"":2.08,""OriginalValue"":1.3,""LessIsGood"":1}]} },{ ""Slot"":""TinyHardpoint1"",""Item"":""Hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":0,""Value"":18546},{ ""Slot"":""TinyHardpoint2"",""Item"":""Hpt_shieldbooster_size0_class4"",""On"":true,""Priority"":0,""Value"":118950,""Engineering"":{ ""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673797,""BlueprintName"":""ShieldBooster_Thermic"",""Level"":3,""Quality"":0.974,""Modifiers"":[{ ""Label"":""KineticResistance"",""Value"":-2.499998,""OriginalValue"":0.0,""LessIsGood"":0},{ ""Label"":""ThermicResistance"",""Value"":16.869999,""OriginalValue"":0.0,""LessIsGood"":0},{ ""Label"":""ExplosiveResistance"",""Value"":-2.499998,""OriginalValue"":0.0,""LessIsGood"":0}]} },{ ""Slot"":""TinyHardpoint3"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":0,""Value"":281000,""Engineering"":{ ""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673782,""BlueprintName"":""ShieldBooster_HeavyDuty"",""Level"":3,""Quality"":0.8829,""Modifiers"":[{ ""Label"":""Mass"",""Value"":10.5,""OriginalValue"":3.5,""LessIsGood"":1},{ ""Label"":""Integrity"",""Value"":52.1712,""OriginalValue"":48.0,""LessIsGood"":0},{ ""Label"":""PowerDraw"",""Value"":1.38,""OriginalValue"":1.2,""LessIsGood"":1},{ ""Label"":""DefenceModifierShieldMultiplier"",""Value"":47.816002,""OriginalValue"":20.000004,""LessIsGood"":0}]} },{ ""Slot"":""TinyHardpoint4"",""Item"":""Hpt_shieldbooster_size0_class4"",""On"":true,""Priority"":0,""Value"":122000,""Engineering"":{ ""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673782,""BlueprintName"":""ShieldBooster_HeavyDuty"",""Level"":3,""Quality"":0.91,""Modifiers"":[{ ""Label"":""Mass"",""Value"":9.0,""OriginalValue"":3.0,""LessIsGood"":1},{ ""Label"":""Integrity"",""Value"":48.928501,""OriginalValue"":45.0,""LessIsGood"":0},{ ""Label"":""PowerDraw"",""Value"":1.15,""OriginalValue"":1.0,""LessIsGood"":1},{ ""Label"":""DefenceModifierShieldMultiplier"",""Value"":43.236794,""OriginalValue"":15.999996,""LessIsGood"":0}]} },{ ""Slot"":""TinyHardpoint5"",""Item"":""Hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":0,""Value"":18546},{ ""Slot"":""TinyHardpoint6"",""Item"":""Hpt_chafflauncher_tiny"",""On"":true,""Priority"":0,""Value"":8500},{ ""Slot"":""ShipName1"",""Item"":""Nameplate_Explorer01_White"",""On"":true,""Priority"":1},{ ""Slot"":""ShipName0"",""Item"":""Nameplate_Explorer01_White"",""On"":true,""Priority"":1},{ ""Slot"":""ShipID1"",""Item"":""nameplate_shipid_doubleline_white"",""On"":true,""Priority"":1},{ ""Slot"":""ShipID0"",""Item"":""nameplate_shipid_doubleline_white"",""On"":true,""Priority"":1},{ ""Slot"":""WeaponColour"",""Item"":""weaponcustomisation_red"",""On"":true,""Priority"":1},{ ""Slot"":""VesselVoice"",""Item"":""VoicePack_Verity"",""On"":true,""Priority"":1},{ ""Slot"":""Slot04_Size2"",""Item"":""Int_buggybay_size2_class1"",""On"":true,""Priority"":0,""Value"":17550},{ ""Slot"":""Slot06_Size1"",""Item"":""Int_dronecontrol_collection_size1_class5"",""On"":true,""Priority"":0,""Value"":9360},{ ""Slot"":""Slot03_Size4"",""Item"":""Int_cargorack_size4_class1"",""On"":true,""Priority"":1,""Value"":33470},{ ""Slot"":""Slot05_Size1"",""Item"":""Int_dronecontrol_collection_size1_class5"",""On"":true,""Priority"":0,""Value"":9360},{ ""Slot"":""MediumHardpoint3"",""Item"":""Hpt_multicannon_gimbal_medium"",""On"":true,""Priority"":0,""Value"":57000,""Engineering"":{ ""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""BlueprintID"":128673504,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":0.226,""Modifiers"":[{ ""Label"":""DamagePerSecond"",""Value"":20.469725,""OriginalValue"":12.615385,""LessIsGood"":0},{ ""Label"":""Damage"",""Value"":2.661064,""OriginalValue"":1.64,""LessIsGood"":0},{ ""Label"":""DistributorDraw"",""Value"":0.189,""OriginalValue"":0.14,""LessIsGood"":1},{ ""Label"":""ThermalLoad"",""Value"":0.23,""OriginalValue"":0.2,""LessIsGood"":1},{ ""Label"":""AmmoClipSize"",""Value"":77.0,""OriginalValue"":90.0,""LessIsGood"":0}]} },{ ""Slot"":""MediumHardpoint4"",""Item"":""Hpt_dumbfiremissilerack_fixed_medium"",""On"":true,""Priority"":0,""Value"":234390}]}";

                var mod = GetModule(t, ShipSlots.Slot.HugeHardpoint1, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(16));
                Check(mod.Integrity.Value.ApproxEqualsPercent(80));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.22));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(38.12));
                Check(mod.Damage.Value.ApproxEqualsPercent(5.661));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.4995));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.5865));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(68));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(2000));
                Check(mod.Speed.Value.ApproxEqualsPercent(1600));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(3.367));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.297));
                Check(mod.Clip == 77);
                Check(mod.Ammo == 2100);
                Check(mod.Rounds == 2);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(5.072));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(100));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(0));

                mod = GetModule(t, ShipSlots.Slot.MediumHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(51));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(20.396));
                Check(mod.Damage.Value.ApproxEqualsPercent(20.396));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(4.644));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(6.118));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(35));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(600));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(16.291));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(100));

                mod = GetModule(t, ShipSlots.Slot.MediumHardpoint3);
                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(51));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.64));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(20.47));
                Check(mod.Damage.Value.ApproxEqualsPercent(2.661));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.189));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.23));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(37));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(2000));
                Check(mod.Speed.Value.ApproxEqualsPercent(1600));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(7.692));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.13));
                Check(mod.Clip.Value == 77);
                Check(mod.Ammo.Value == 2100);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(2.4339));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(100));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(0));

                mod = GetModule(t, ShipSlots.Slot.TinyHardpoint2);
                Check(mod.Mass.Value.ApproxEqualsPercent(3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(45));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(16));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-2.5));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(16.87));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-2.5));

                mod = GetModule(t, ShipSlots.Slot.TinyHardpoint3);
                Check(mod.Mass.Value.ApproxEqualsPercent(10.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(52.17));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.38));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(47.82));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(0));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(0));

                mod = GetModule(t, ShipSlots.Slot.TinyHardpoint4);
                Check(mod.Mass.Value.ApproxEqualsPercent(9));
                Check(mod.Integrity.Value.ApproxEqualsPercent(48.93));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.15));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(43.24));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(0));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(0));

                mod = GetModule(t, ShipSlots.Slot.Slot01_Size5);

                Check(mod.Mass.Value.ApproxEqualsPercent(20));
                Check(mod.Integrity.Value.ApproxEqualsPercent(115));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(3.64));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(203));
                Check(mod.OptMass.Value.ApproxEqualsPercent(405));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(1013));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(92.24));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(158.12));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(224));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(3.375));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.66));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(48.05));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-3.89601));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(56.71));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));

                Ship si = Ship.CreateFromLoadout(t);
                DebuggerHelpers.BreakAssert(si != null, "Bad ship");

                var stats = si.GetShipStats(4, 4, 4, 0, 8, 0);

                Check(Math.Round(stats.CurrentSpeed.Value) == 361);
                Check(Math.Round(stats.LadenSpeed) == 389);
                Check(Math.Round(stats.UnladenSpeed) == 391);
                Check(Math.Round(stats.MaxSpeed) == 393);
                Check(Math.Round(stats.CurrentBoost) == 527);
                Check(Math.Round(stats.LadenBoost) == 523);
                Check(Math.Round(stats.UnladenBoost) == 527);
                Check(Math.Round(stats.MaxBoost) == 529);
                Check(stats.CurrentBoostFrequency.ApproxEqualsPercent(9.1867));
                Check(stats.MaxBoostFrequency.ApproxEqualsPercent(4.2857));

                Check(stats.ShieldsSystemPercentage.ApproxEqualsPercent(33.3));
                Check(stats.ShieldsKineticPercentage.ApproxEqualsPercent(46.8));
                Check(stats.ShieldsThermalPercentage.ApproxEqualsPercent(13.6312));
                Check(stats.ShieldsExplosivePercentage.ApproxEqualsPercent(55.6277));
                Check(stats.ShieldsSystemValue.ApproxEqualsPercent(1920.7));
                Check(stats.ShieldsKineticValue.ApproxEqualsPercent(3607.2));
                Check(stats.ShieldsThermalValue.ApproxEqualsPercent(2223.9));
                Check(stats.ShieldsExplosiveValue.ApproxEqualsPercent(4328.7));
                Check(stats.ShieldBuildTime.Value.ApproxEqualsPercent(3*60+26));
                Check(stats.ShieldRegenTime.ApproxEqualsPercent(10 * 60 + 41));

                Check(stats.ArmourRaw.Value.ApproxEqualsPercent(1031.625));
                Check(stats.ArmourKineticPercentage.ApproxEqualsPercent(-14.312));
                Check(stats.ArmourThermalPercentage.ApproxEqualsPercent(4.74));
                Check(stats.ArmourExplosivePercentage.ApproxEqualsPercent(-33.363));
                Check(stats.ArmourCausticPercentage.ApproxEqualsPercent(0));
                Check(stats.ArmourKineticValue.ApproxEqualsPercent(902.464));
                Check(stats.ArmourThermalValue.ApproxEqualsPercent(1082.95));
                Check(stats.ArmourExplosiveValue.ApproxEqualsPercent(773.54));
                Check(stats.ArmourCausticValue.ApproxEqualsPercent(1031.625));

                Check(stats.FSDCurrentRange.Value.ApproxEqualsPercent(14.903));
                Check(stats.FSDCurrentMaxRange.ApproxEqualsPercent(42.533));
                Check(stats.FSDLadenRange.ApproxEqualsPercent(14.4587));
                Check(stats.FSDUnladenRange.ApproxEqualsPercent(14.9034));
                Check(stats.FSDMaxRange.ApproxEqualsPercent(15.048));
                Check(stats.FSDMaxFuelPerJump.ApproxEqualsPercent(3));

                Check(stats.WeaponRaw.Value.ApproxEqualsPercent(108.396));
                Check(stats.WeaponAbsolutePercentage.ApproxEqualsPercent(0));
                Check(stats.WeaponKineticPercentage.ApproxEqualsPercent(41.874));
                Check(stats.WeaponThermalPercentage.ApproxEqualsPercent(37.624));
                Check(stats.WeaponExplosivePercentage.ApproxEqualsPercent(20.5008));
                Check(stats.WeaponAXPercentage.ApproxEqualsPercent(0));
                Check(stats.WeaponDuration.ApproxEqualsPercent(5.172));
                Check(stats.WeaponDurationMax.ApproxEqualsPercent(8.895));
                Check(stats.WeaponAmmoDuration.ApproxEqualsPercent(3*60+35));
                Check(stats.WeaponCurSus.ApproxEqualsPercent(28.559));
                Check(stats.WeaponMaxSus.ApproxEqualsPercent(61.219));

            }

            {
                // armour, refinforcement (noting that reinforcement is setting a zero value for armour)
                string t = @"{""timestamp"":""2024-07-11T14:36:56.902Z"",""event"":""Loadout"",""Ship"":""Cutter"",""ShipID"":8,""ShipName"":""Phönix"",""ShipIdent"":""MIS-08"",""HullValue"":175924977,""ModulesValue"":342785372,""HullHealth"":1.0,""UnladenMass"":1799.76001,""CargoCapacity"":704,""FuelCapacity"":{""Main"":64.0,""Reserve"":1.16},""Rebuy"":25935519,""Modules"":[{""Slot"":""Armour"",""Item"":""cutter_Armour_grade1"",""On"":true,""Priority"":1,""Engineering"":{""Engineer"":""Selene Jean"",""EngineerID"":300210,""BlueprintID"":128673634,""BlueprintName"":""Armour_Advanced"",""Level"":5,""Quality"":0.986,""Modifiers"":[{""Label"":""DefenceModifierHealthMultiplier"",""Value"":70.999992,""OriginalValue"":79.999992,""LessIsGood"":0},{""Label"":""KineticResistance"",""Value"":-2.00001,""OriginalValue"":-20.000004,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":14.999998,""OriginalValue"":0.0,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":-19.000006,""OriginalValue"":-39.999996,""LessIsGood"":0}]}},{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":3},{""Slot"":""Decal1"",""Item"":""decal_trade_elite"",""On"":true,""Priority"":1},{""Slot"":""Decal3"",""Item"":""decal_trade_elite"",""On"":true,""Priority"":1},{""Slot"":""Decal2"",""Item"":""decal_trade_elite"",""On"":true,""Priority"":1},{""Slot"":""FrameShiftDrive"",""Item"":""Int_hyperdrive_size7_class5"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""BlueprintID"":128673694,""BlueprintName"":""FSD_LongRange"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_fsd_fuelcapacity"",""ExperimentalEffect_Localised"":""Deep Charge"",""Modifiers"":[{""Label"":""Mass"",""Value"":104.0,""OriginalValue"":80.0,""LessIsGood"":1},{""Label"":""Integrity"",""Value"":139.400009,""OriginalValue"":164.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":1.08675,""OriginalValue"":0.9,""LessIsGood"":1},{""Label"":""FSDOptimalMass"",""Value"":4185.0,""OriginalValue"":2700.0,""LessIsGood"":0},{""Label"":""MaxFuelPerJump"",""Value"":14.080001,""OriginalValue"":12.8,""LessIsGood"":0}]}},{""Slot"":""FuelTank"",""Item"":""Int_fueltank_size6_class3"",""On"":true,""Priority"":1},{""Slot"":""HugeHardpoint1"",""Item"":""Hpt_multicannon_gimbal_huge"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""BlueprintID"":128673504,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_incendiary_rounds"",""ExperimentalEffect_Localised"":""Incendiary Rounds"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":37.62896,""OriginalValue"":23.299664,""LessIsGood"":0},{""Label"":""Damage"",""Value"":5.882,""OriginalValue"":3.46,""LessIsGood"":0},{""Label"":""DistributorDraw"",""Value"":0.4995,""OriginalValue"":0.37,""LessIsGood"":1},{""Label"":""ThermalLoad"",""Value"":1.7595,""OriginalValue"":0.51,""LessIsGood"":1},{""Label"":""RateOfFire"",""Value"":3.198653,""OriginalValue"":3.367003,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":77.0,""OriginalValue"":90.0,""LessIsGood"":0},{""Label"":""DamageType"",""ValueStr"":""$Thermic;""}]}},{""Slot"":""LargeHardpoint1"",""Item"":""Hpt_multicannon_gimbal_large"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""BlueprintID"":128673504,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_corrosive_shell"",""ExperimentalEffect_Localised"":""Corrosive Shell"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":32.186665,""OriginalValue"":18.933332,""LessIsGood"":0},{""Label"":""Damage"",""Value"":4.828,""OriginalValue"":2.84,""LessIsGood"":0},{""Label"":""DistributorDraw"",""Value"":0.3375,""OriginalValue"":0.25,""LessIsGood"":1},{""Label"":""ThermalLoad"",""Value"":0.391,""OriginalValue"":0.34,""LessIsGood"":1},{""Label"":""AmmoClipSize"",""Value"":77.0,""OriginalValue"":90.0,""LessIsGood"":0},{""Label"":""AmmoMaximum"",""Value"":1680.0,""OriginalValue"":2100.0,""LessIsGood"":0}]}},{""Slot"":""LargeHardpoint2"",""Item"":""Hpt_multicannon_gimbal_large"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""BlueprintID"":128673504,""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_incendiary_rounds"",""ExperimentalEffect_Localised"":""Incendiary Rounds"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":30.577332,""OriginalValue"":18.933332,""LessIsGood"":0},{""Label"":""Damage"",""Value"":4.828,""OriginalValue"":2.84,""LessIsGood"":0},{""Label"":""DistributorDraw"",""Value"":0.3375,""OriginalValue"":0.25,""LessIsGood"":1},{""Label"":""ThermalLoad"",""Value"":1.173,""OriginalValue"":0.34,""LessIsGood"":1},{""Label"":""RateOfFire"",""Value"":6.333333,""OriginalValue"":6.666667,""LessIsGood"":0},{""Label"":""AmmoClipSize"",""Value"":77.0,""OriginalValue"":90.0,""LessIsGood"":0},{""Label"":""DamageType"",""ValueStr"":""$Thermic;""}]}},{""Slot"":""LifeSupport"",""Item"":""Int_lifesupport_size7_class2"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint1"",""Item"":""Hpt_pulselaserburst_turret_medium"",""On"":true,""Priority"":1},{""Slot"":""MediumHardpoint2"",""Item"":""Hpt_pulselaser_turret_medium"",""On"":true,""Priority"":1,""Engineering"":{""Engineer"":""The Dweller"",""EngineerID"":300180,""BlueprintID"":128673577,""BlueprintName"":""Weapon_LongRange"",""Level"":3,""Quality"":1.0,""ExperimentalEffect"":""special_phasing_sequence"",""ExperimentalEffect_Localised"":""Phasing Sequence"",""Modifiers"":[{""Label"":""Mass"",""Value"":4.8,""OriginalValue"":4.0,""LessIsGood"":1},{""Label"":""PowerDraw"",""Value"":0.6322,""OriginalValue"":0.58,""LessIsGood"":1},{""Label"":""DamagePerSecond"",""Value"":5.590909,""OriginalValue"":6.212121,""LessIsGood"":0},{""Label"":""Damage"",""Value"":1.845,""OriginalValue"":2.05,""LessIsGood"":0},{""Label"":""MaximumRange"",""Value"":4800.0,""OriginalValue"":3000.0,""LessIsGood"":0},{""Label"":""DamageFalloffRange"",""Value"":4800.0,""OriginalValue"":500.0,""LessIsGood"":0}]}},{""Slot"":""MediumHardpoint3"",""Item"":""Hpt_beamlaser_turret_medium"",""On"":true,""Priority"":1},{""Slot"":""MediumHardpoint4"",""Item"":""Hpt_beamlaser_turret_medium"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Broo Tarquin"",""EngineerID"":300030,""BlueprintID"":128739091,""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_thermal_vent"",""ExperimentalEffect_Localised"":""Thermal Vent"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.4836,""OriginalValue"":0.93,""LessIsGood"":1},{""Label"":""DamagePerSecond"",""Value"":10.9492,""OriginalValue"":8.83,""LessIsGood"":0},{""Label"":""DistributorDraw"",""Value"":1.188,""OriginalValue"":2.16,""LessIsGood"":1},{""Label"":""ThermalLoad"",""Value"":1.412,""OriginalValue"":3.53,""LessIsGood"":1}]}},{""Slot"":""Military01"",""Item"":""Int_shieldcellbank_size5_class5"",""On"":true,""Priority"":2},{""Slot"":""Military02"",""Item"":""Int_hullreinforcement_size5_class2"",""On"":true,""Priority"":1,""Engineering"":{""Engineer"":""Selene Jean"",""EngineerID"":300210,""BlueprintID"":128673709,""BlueprintName"":""HullReinforcement_Advanced"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_hullreinforcement_chunky"",""ExperimentalEffect_Localised"":""Deep Plating"",""Modifiers"":[{""Label"":""Mass"",""Value"":12.16,""OriginalValue"":16.0,""LessIsGood"":1},{""Label"":""DefenceModifierHealthMultiplier"",""Value"":24.0,""OriginalValue"":0.0,""LessIsGood"":0},{""Label"":""DefenceModifierHealthAddition"",""Value"":343.200012,""OriginalValue"":390.0,""LessIsGood"":0},{""Label"":""KineticResistance"",""Value"":0.550002,""OriginalValue"":2.499998,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":0.550002,""OriginalValue"":2.499998,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":0.550002,""OriginalValue"":2.499998,""LessIsGood"":0}]}},{""Slot"":""Slot01_Size8"",""Item"":""Int_cargorack_size8_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot10_Size1"",""Item"":""Int_supercruiseassist"",""On"":true,""Priority"":4},{""Slot"":""Slot02_Size8"",""Item"":""Int_cargorack_size8_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot03_Size6"",""Item"":""Int_cargorack_size6_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot04_Size6"",""Item"":""Int_cargorack_size6_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot05_Size6"",""Item"":""Int_shieldgenerator_size6_class5_strong"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Lei Cheung"",""EngineerID"":300120,""BlueprintID"":128673839,""BlueprintName"":""ShieldGenerator_Reinforced"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_shield_health"",""ExperimentalEffect_Localised"":""Hi-Cap"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":7.161,""OriginalValue"":6.51,""LessIsGood"":1},{""Label"":""ShieldGenStrength"",""Value"":219.419983,""OriginalValue"":150.0,""LessIsGood"":0},{""Label"":""BrokenRegenRate"",""Value"":2.88,""OriginalValue"":3.2,""LessIsGood"":0},{""Label"":""EnergyPerRegen"",""Value"":0.84,""OriginalValue"":0.6,""LessIsGood"":1},{""Label"":""KineticResistance"",""Value"":49.900002,""OriginalValue"":39.999996,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":-0.199997,""OriginalValue"":-20.000004,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":58.25,""OriginalValue"":50.0,""LessIsGood"":0}]}},{""Slot"":""Slot06_Size5"",""Item"":""Int_cargorack_size5_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot07_Size5"",""Item"":""Int_cargorack_size5_class1"",""On"":true,""Priority"":1},{""Slot"":""Slot08_Size4"",""Item"":""Int_dronecontrol_collection_size3_class5"",""On"":true,""Priority"":0},{""Slot"":""Slot09_Size3"",""Item"":""Int_dockingcomputer_advanced"",""On"":true,""Priority"":4},{""Slot"":""PaintJob"",""Item"":""paintjob_cutter_militaire_forest_green"",""On"":true,""Priority"":1},{""Slot"":""PlanetaryApproachSuite"",""Item"":""Int_planetapproachsuite"",""On"":true,""Priority"":1},{""Slot"":""PowerDistributor"",""Item"":""Int_powerdistributor_size7_class5"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""The Dweller"",""EngineerID"":300180,""BlueprintID"":128673739,""BlueprintName"":""PowerDistributor_HighFrequency"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_powerdistributor_fast"",""ExperimentalEffect_Localised"":""Super Conduits"",""Modifiers"":[{""Label"":""WeaponsCapacity"",""Value"":55.632,""OriginalValue"":61.0,""LessIsGood"":0},{""Label"":""WeaponsRecharge"",""Value"":9.1988,""OriginalValue"":6.1,""LessIsGood"":0},{""Label"":""EnginesCapacity"",""Value"":37.391998,""OriginalValue"":41.0,""LessIsGood"":0},{""Label"":""EnginesRecharge"",""Value"":6.032,""OriginalValue"":4.0,""LessIsGood"":0},{""Label"":""SystemsCapacity"",""Value"":37.391998,""OriginalValue"":41.0,""LessIsGood"":0},{""Label"":""SystemsRecharge"",""Value"":6.032,""OriginalValue"":4.0,""LessIsGood"":0}]}},{""Slot"":""PowerPlant"",""Item"":""Int_powerplant_size8_class5"",""On"":true,""Priority"":1,""Engineering"":{""Engineer"":""Hera Tani"",""EngineerID"":300090,""BlueprintID"":128673764,""BlueprintName"":""PowerPlant_Armoured"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_powerplant_cooled"",""ExperimentalEffect_Localised"":""Thermal Spread"",""Modifiers"":[{""Label"":""Mass"",""Value"":96.0,""OriginalValue"":80.0,""LessIsGood"":1},{""Label"":""Integrity"",""Value"":363.0,""OriginalValue"":165.0,""LessIsGood"":0},{""Label"":""PowerCapacity"",""Value"":40.32,""OriginalValue"":36.0,""LessIsGood"":0},{""Label"":""HeatEfficiency"",""Value"":0.3168,""OriginalValue"":0.4,""LessIsGood"":1}]}},{""Slot"":""Radar"",""Item"":""Int_sensors_size7_class2"",""On"":true,""Priority"":0},{""Slot"":""ShipCockpit"",""Item"":""cutter_cockpit"",""On"":true,""Priority"":1},{""Slot"":""MainEngines"",""Item"":""Int_engine_size8_class5"",""On"":true,""Priority"":0,""Engineering"":{""Engineer"":""Professor Palin"",""EngineerID"":300220,""BlueprintID"":128673659,""BlueprintName"":""Engine_Dirty"",""Level"":5,""Quality"":1.0,""ExperimentalEffect"":""special_engine_overloaded"",""ExperimentalEffect_Localised"":""Drag Drives"",""Modifiers"":[{""Label"":""Integrity"",""Value"":140.25,""OriginalValue"":165.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":12.096001,""OriginalValue"":10.8,""LessIsGood"":1},{""Label"":""EngineOptimalMass"",""Value"":2940.0,""OriginalValue"":3360.0,""LessIsGood"":0},{""Label"":""EngineOptPerformance"",""Value"":145.600006,""OriginalValue"":100.0,""LessIsGood"":0},{""Label"":""EngineHeatRate"",""Value"":2.288,""OriginalValue"":1.3,""LessIsGood"":1}]}},{""Slot"":""TinyHardpoint1"",""Item"":""Hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint2"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":2,""Engineering"":{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""BlueprintID"":128673780,""BlueprintName"":""ShieldBooster_HeavyDuty"",""Level"":1,""Quality"":1.0,""Modifiers"":[{""Label"":""Mass"",""Value"":7.0,""OriginalValue"":3.5,""LessIsGood"":1},{""Label"":""Integrity"",""Value"":49.439999,""OriginalValue"":48.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":1.26,""OriginalValue"":1.2,""LessIsGood"":1},{""Label"":""DefenceModifierShieldMultiplier"",""Value"":32.000004,""OriginalValue"":20.000004,""LessIsGood"":0}]}},{""Slot"":""TinyHardpoint3"",""Item"":""Hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":1},{""Slot"":""TinyHardpoint4"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":2,""Engineering"":{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""BlueprintID"":128673780,""BlueprintName"":""ShieldBooster_HeavyDuty"",""Level"":1,""Quality"":1.0,""Modifiers"":[{""Label"":""Mass"",""Value"":7.0,""OriginalValue"":3.5,""LessIsGood"":1},{""Label"":""Integrity"",""Value"":49.439999,""OriginalValue"":48.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":1.26,""OriginalValue"":1.2,""LessIsGood"":1},{""Label"":""DefenceModifierShieldMultiplier"",""Value"":32.000004,""OriginalValue"":20.000004,""LessIsGood"":0}]}},{""Slot"":""TinyHardpoint5"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":2},{""Slot"":""TinyHardpoint6"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":2},{""Slot"":""TinyHardpoint7"",""Item"":""Hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":1},{""Slot"":""TinyHardpoint8"",""Item"":""Hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":2,""Engineering"":{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""BlueprintID"":128673790,""BlueprintName"":""ShieldBooster_Resistive"",""Level"":1,""Quality"":1.0,""Modifiers"":[{""Label"":""Integrity"",""Value"":46.079998,""OriginalValue"":48.0,""LessIsGood"":0},{""Label"":""PowerDraw"",""Value"":1.26,""OriginalValue"":1.2,""LessIsGood"":1},{""Label"":""KineticResistance"",""Value"":5.000001,""OriginalValue"":0.0,""LessIsGood"":0},{""Label"":""ThermicResistance"",""Value"":5.000001,""OriginalValue"":0.0,""LessIsGood"":0},{""Label"":""ExplosiveResistance"",""Value"":5.000001,""OriginalValue"":0.0,""LessIsGood"":0}]}},{""Slot"":""VesselVoice"",""Item"":""voicepack_verity"",""On"":true,""Priority"":1}]}";
                var mod = GetModule(t, ShipSlots.Slot.Armour);

                Check(mod.Mass.Value.ApproxEqualsPercent(0));
                Check(mod.HullStrengthBonus.Value.ApproxEqualsPercent(71));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-2));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(15));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(-19));


                mod = GetModule(t, ShipSlots.Slot.Military02);
                Check(mod.Mass.Value.ApproxEqualsPercent(12.16));
                Check(mod.HullStrengthBonus.Value.ApproxEqualsPercent(24));
                Check(mod.HullReinforcement.Value.ApproxEqualsPercent(343.2));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(0.55));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(0.55));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(0.55));

                Ship si = Ship.CreateFromLoadout(t);
                DebuggerHelpers.BreakAssert(si != null, "Bad ship");
                var stats = si.GetShipStats(4, 4, 4, 0, 8, 0);
                Check(stats.ArmourRaw.Value.ApproxEqualsPercent(1123.2));
            }

            {
                // edsy mixed with ealhstans loadout description direct from game
                string t = @"{ ""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":""ST-13K"",""HullValue"":38743029,""ModulesValue"":115884722,""UnladenMass"":559,""CargoCapacity"":0,""MaxJumpRange"":21.837943,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":7731387,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint1"",""Item"":""Hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":42559,""Engineering"":{""Engineer"":""The Dweller"",""EngineerID"":300180,""BlueprintID"":128673362,""BlueprintName"":""Weapon_Focused"",""Level"":3,""Quality"":0.3,""ExperimentalEffect"":""special_distortion_field"",""ExperimentalEffect_Localised"":""Inertial Impact"",""Modifiers"":[{""Label"":""ThermalLoad"",""Value"":0.6901,""OriginalValue"":0.67,""LessIsGood"":1},{""Label"":""ArmourPenetration"",""Value"":58.100002,""OriginalValue"":35.0,""LessIsGood"":0},{""Label"":""MaximumRange"",""Value"":4714.5,""OriginalValue"":3000.0,""LessIsGood"":0},{""Label"":""Jitter"",""Value"":3.0,""OriginalValue"":0.0,""LessIsGood"":1},{""Label"":""DamageType"",""ValueStr"":""$Kinetic;""},{""Label"":""DamageFalloffRange"",""Value"":785.750061,""OriginalValue"":500.0,""LessIsGood"":0}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_reactive"",""On"":true,""Priority"":1,""Value"":94756030},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class2"",""On"":true,""Priority"":1,""Value"":1264679},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class5"",""On"":true,""Priority"":0,""Value"":14197538},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class5"",""On"":true,""Priority"":0,""Value"":4478716},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class2"",""On"":true,""Priority"":0,""Value"":24895},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class2"",""On"":true,""Priority"":0,""Value"":546542},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class3"",""On"":true,""Priority"":0,""Value"":487987},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":1,""Value"":85776}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint1, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.04));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(15.445));
                Check(mod.Damage.Value.ApproxEqualsPercent(3.675));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.49));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.6901));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(58.1));
                Check(mod.Range.Value.ApproxEqualsPercent(4715));
                Check(mod.Falloff.Value.ApproxEqualsPercent(785.8));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(3.15));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(3));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));


            }

            {

            //TEST Module Hpt_pulselaserburst_gimbal_medium in MediumHardpoint2 Blueprint: Efficient Weapon
            //Level: 5
            //Quality: 1
            //Power Draw: 0.541, Original: 1.04, Mult: -48.0 % (Worse)
            //Damage Per Second: 12.767, Original: 10.296, Mult: 24.0 % (Better)
            //Damage: 3.038, Original: 2.45, Mult: 24.0 % (Better)
            //Distributor Draw: 0.27, Original: 0.49, Mult: -45.0 % (Worse)
            //Thermal Load: 0.268, Original: 0.67, Mult: -60.0 % (Worse)

            //Engineer Burst Laser Gimbal Medium PowerDraw PowerDraw 1.04-> 0.5408 ratio 0.52
            //Engineer Burst Laser Gimbal Medium DamagePerSecond DPS 10.296-> 12.767457 ratio 1.24000003496389
            //   Engineer Burst Laser Gimbal Medium DamagePerSecond Damage NOT changing due to primary modifier being present
            //   Engineer Burst Laser Gimbal Medium DamagePerSecond BreachDamage NOT changing due to condition - Damage
            //Engineer Burst Laser Gimbal Medium Damage Damage 2.45-> 3.038 ratio 1.24
            //   Engineer Burst Laser Gimbal Medium Damage BreachDamage 2.1-> 2.604 ratio 1.24
            //   Engineer Burst Laser Gimbal Medium Damage BurstInterval NOT changing due to condition +hpt_railgun *
            //Engineer Burst Laser Gimbal Medium DistributorDraw DistributorDraw 0.49-> 0.2695 ratio 0.55
            //Engineer Burst Laser Gimbal Medium ThermalLoad ThermalLoad 0.67-> 0.268 ratio 0.4

                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":44152080,""ModulesValue"":1708430,""UnladenMass"":636,""CargoCapacity"":82,""MaxJumpRange"":8.985727,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":2293025,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":48500,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.5408,""OriginalValue"":1.04},{""Label"":""DamagePerSecond"",""Value"":12.767457,""OriginalValue"":10.296336},{""Label"":""Damage"",""Value"":3.038,""OriginalValue"":2.45},{""Label"":""DistributorDraw"",""Value"":0.2695,""OriginalValue"":0.49},{""Label"":""ThermalLoad"",""Value"":0.268,""OriginalValue"":0.67}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size5"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot04_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot08_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot09_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.5408));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(12.767));
                Check(mod.Damage.Value.ApproxEqualsPercent(3.038));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.2695));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.268));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(35));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(500));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(2.604));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(100));
            }


            {
                //TEST Module Hpt_pulselaserburst_turret_small in SmallHardpoint1 Blueprint: Focused Weapon
                //Level: 5
                //Quality: 1
                //Experimental Effect: Inertial Impact
                //   Damage: 50
                //   Jitter: 3
                //   KineticProportionDamage: 50
                //   ThermalProportionDamage: 50

                //Damage Per Second: 6.261, Original: 4.174, Mult: 50.0 % (Better)
                //Damage: 1.305, Original: 0.87, Mult: 50.0 % (Better)
                //Thermal Load: 0.2, Original: 0.19, Mult: 5.0 % (Better)
                //Armour Penetration: 44, Original: 20, Mult: 120.0 % (Better)
                //Maximum Range: 6000, Original: 3000, Mult: 100.0 % (Better)
                //Falloff Range: 1000, Original: 500, Mult: 100.0 % (Better)
                //Jitter: 3, Original: 0, Mult: ∞% (Better)

                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":61300,""UnladenMass"":38.4,""CargoCapacity"":0,""MaxJumpRange"":9.089833,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3318,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaserburst_turret_small"",""On"":true,""Priority"":0,""Value"":52800,""Engineering"":{""BlueprintName"":""Weapon_Focused"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_distortion_field"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":6.261364,""OriginalValue"":4.174242},{""Label"":""Damage"",""Value"":1.305,""OriginalValue"":0.87},{""Label"":""ThermalLoad"",""Value"":0.1995,""OriginalValue"":0.19},{""Label"":""ArmourPenetration"",""Value"":44,""OriginalValue"":20},{""Label"":""MaximumRange"",""Value"":6000,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":1000,""OriginalValue"":500},{""Label"":""Jitter"",""Value"":3,""OriginalValue"":0}]}},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000}]}";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.6));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(6.261));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.305));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.139));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.1995));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(44));
                Check(mod.Range.Value.ApproxEqualsPercent(6000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.798));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.52));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(19));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(0.6));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(60));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(3));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
            }

            {
//TEST Module Hpt_pulselaserburst_gimbal_medium in MediumHardpoint2 Blueprint: Efficient Weapon
//Level: 3
//Quality: 1
//Power Draw: 0.79, Original: 1.04, Mult: -24.0 % (Worse)
//Damage Per Second: 11.944, Original: 10.296, Mult: 16.0 % (Better)
//Damage: 2.842, Original: 2.45, Mult: 16.0 % (Better)
//Distributor Draw: 0.368, Original: 0.49, Mult: -25.0 % (Worse)
//Thermal Load: 0.352, Original: 0.67, Mult: -47.5 % (Worse)

//Engineer Burst Laser Gimbal Medium PowerDraw PowerDraw 1.04-> 0.7904 ratio 0.76
//Engineer Burst Laser Gimbal Medium DamagePerSecond DPS 10.296-> 11.94375 ratio 1.16000002330926
//   Engineer Burst Laser Gimbal Medium DamagePerSecond Damage NOT changing due to primary modifier being present
//   Engineer Burst Laser Gimbal Medium DamagePerSecond BreachDamage NOT changing due to condition - Damage
//Engineer Burst Laser Gimbal Medium Damage Damage 2.45-> 2.842 ratio 1.16
//   Engineer Burst Laser Gimbal Medium Damage BreachDamage 2.1-> 2.436 ratio 1.16
//   Engineer Burst Laser Gimbal Medium Damage BurstInterval NOT changing due to condition +hpt_railgun *
//Engineer Burst Laser Gimbal Medium DistributorDraw DistributorDraw 0.49-> 0.3675 ratio 0.75
//Engineer Burst Laser Gimbal Medium ThermalLoad ThermalLoad 0.67-> 0.35175 ratio 0.525

                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":44152080,""ModulesValue"":1238890,""UnladenMass"":594,""CargoCapacity"":0,""MaxJumpRange"":9.617571,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":2269548,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":48500,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":3,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.7904,""OriginalValue"":1.04},{""Label"":""DamagePerSecond"",""Value"":11.94375,""OriginalValue"":10.296336},{""Label"":""Damage"",""Value"":2.842,""OriginalValue"":2.45},{""Label"":""DistributorDraw"",""Value"":0.3675,""OriginalValue"":0.49},{""Label"":""ThermalLoad"",""Value"":0.35175,""OriginalValue"":0.67}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.7904));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(11.944));
                Check(mod.Damage.Value.ApproxEqualsPercent(2.842));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.3675));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.3518));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(35));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(500));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(2.436));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(100));
            }

            {
//TEST Module Hpt_pulselaserburst_gimbal_medium in MediumHardpoint2 Blueprint: Focused Weapon
//Level: 3
//Quality: 1
//Thermal Load: 0.69, Original: 0.67, Mult: 3.0 % (Better)
//Armour Penetration: 63, Original: 35, Mult: 80.0 % (Better)
//Maximum Range: 5040, Original: 3000, Mult: 68.0 % (Better)
//Falloff Range: 840, Original: 500, Mult: 68.0 % (Better)

//* **Engineer module Hpt_pulselaserburst_gimbal_medium
//Engineer Burst Laser Gimbal Medium ThermalLoad ThermalLoad 0.67-> 0.6901 ratio 1.03
//Engineer Burst Laser Gimbal Medium ArmourPenetration ArmourPiercing 35-> 63 ratio 1.8
//Engineer Burst Laser Gimbal Medium MaximumRange Range 3000-> 5040 ratio 1.68
//Engineer Burst Laser Gimbal Medium FalloffRange Falloff 500-> 840 ratio 1.68


                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":44152080,""ModulesValue"":1238890,""UnladenMass"":594,""CargoCapacity"":0,""MaxJumpRange"":9.617571,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":2269548,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":48500,""Engineering"":{""BlueprintName"":""Weapon_Focused"",""Level"":3,""Quality"":1,""Modifiers"":[{""Label"":""ThermalLoad"",""Value"":0.6901,""OriginalValue"":0.67},{""Label"":""ArmourPenetration"",""Value"":63,""OriginalValue"":35},{""Label"":""MaximumRange"",""Value"":5040,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":840,""OriginalValue"":500}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.04));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(10.296));
                Check(mod.Damage.Value.ApproxEqualsPercent(2.45));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.49));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.6901));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(63));
                Check(mod.Range.Value.ApproxEqualsPercent(5040));
                Check(mod.Falloff.Value.ApproxEqualsPercent(840));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(2.1));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(100));
            }

            {
// From edsy
//TEST Module Hpt_pulselaserburst_gimbal_medium in MediumHardpoint2 Blueprint: Focused Weapon
//Level: 3
//Quality: 1
//Experimental Effect: Inertial Impact
//   Damage: 50
//   Jitter: 3
//   KineticProportionDamage: 50
//   ThermalProportionDamage: 50
                //Damage Per Second: 15.445, Original: 10.296, Mult: 50.0 % (Better)
                //Damage: 3.675, Original: 2.45, Mult: 50.0 % (Better)
//Thermal Load: 0.69, Original: 0.67, Mult: 3.0 % (Better)
//Armour Penetration: 63, Original: 35, Mult: 80.0 % (Better)
//Maximum Range: 5040, Original: 3000, Mult: 68.0 % (Better)
//Falloff Range: 840, Original: 500, Mult: 68.0 % (Better)
//Jitter: 3, Original: 0, Mult: ∞% (Better)

                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":44152080,""ModulesValue"":1238890,""UnladenMass"":594,""CargoCapacity"":0,""MaxJumpRange"":9.617571,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":2269548,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":48500,""Engineering"":{""BlueprintName"":""Weapon_Focused"",""Level"":3,""Quality"":1,""ExperimentalEffect"":""special_distortion_field"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":15.444504,""OriginalValue"":10.296336},{""Label"":""Damage"",""Value"":3.675,""OriginalValue"":2.45},{""Label"":""ThermalLoad"",""Value"":0.6901,""OriginalValue"":0.67},{""Label"":""ArmourPenetration"",""Value"":63,""OriginalValue"":35},{""Label"":""MaximumRange"",""Value"":5040,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":840,""OriginalValue"":500},{""Label"":""Jitter"",""Value"":3,""OriginalValue"":0}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.04));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(15.445));
                Check(mod.Damage.Value.ApproxEqualsPercent(3.675));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.49));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.6901));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(63));
                Check(mod.Range.Value.ApproxEqualsPercent(5040));
                Check(mod.Falloff.Value.ApproxEqualsPercent(840));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(3.15));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(3));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
            }


            {
                // from ealhstan ship import, to edsy, to export

// TEST Module Hpt_pulselaserburst_gimbal_medium in MediumHardpoint2 Blueprint: Focused Weapon
//Level: 3
//Quality: 0
//Experimental Effect: Inertial Impact
//   Damage: 50
//   Jitter: 3
//   KineticProportionDamage: 50
//   ThermalProportionDamage: 50
//Damage Per Second: 15.445, Original: 10.296, Mult: 50.0 % (Better)
//Damage: 3.675, Original: 2.45, Mult: 50.0 % (Better)
//Thermal Load: 0.69, Original: 0.67, Mult: 3.0 % (Better)
//Armour Penetration: 58.324, Original: 35, Mult: 66.6 % (Better)
//Maximum Range: 4690.2, Original: 3000, Mult: 56.3 % (Better)
//Falloff Range: 781.7, Original: 500, Mult: 56.3 % (Better)
//Jitter: 3, Original: 0, Mult: ∞% (Better)

                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":""ST-13K"",""HullValue"":38743029,""ModulesValue"":115884722,""UnladenMass"":559,""CargoCapacity"":0,""MaxJumpRange"":21.837943,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":7731387,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":42559,""Engineering"":{""BlueprintName"":""Weapon_Focused"",""Level"":3,""Quality"":0.2713,""ExperimentalEffect"":""special_distortion_field"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":15.444504,""OriginalValue"":10.296336},{""Label"":""Damage"",""Value"":3.675,""OriginalValue"":2.45},{""Label"":""ThermalLoad"",""Value"":0.6901,""OriginalValue"":0.67},{""Label"":""ArmourPenetration"",""Value"":58.323997,""OriginalValue"":35},{""Label"":""MaximumRange"",""Value"":4690.200195,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":781.700012,""OriginalValue"":500},{""Label"":""Jitter"",""Value"":3,""OriginalValue"":0}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_reactive"",""On"":true,""Priority"":1,""Value"":94756030},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class2"",""On"":true,""Priority"":1,""Value"":1264679},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class5"",""On"":true,""Priority"":0,""Value"":14197538},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class5"",""On"":true,""Priority"":0,""Value"":4478716},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class2"",""On"":true,""Priority"":0,""Value"":24895},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class2"",""On"":true,""Priority"":0,""Value"":546542},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class3"",""On"":true,""Priority"":0,""Value"":487987},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":1,""Value"":85776}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);
                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.04));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(15.445));
                Check(mod.Damage.Value.ApproxEqualsPercent(3.675));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.49));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.6901));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(58.32));
                Check(mod.Range.Value.ApproxEqualsPercent(4690));
                Check(mod.Falloff.Value.ApproxEqualsPercent(781.7));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(3.15));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(3));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(50));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(50));
            }

            {
                // burst laser focused  Level 3
                string t = @"{""event"":""Loadout"",""Ship"":""krait_mkii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":44152080,""ModulesValue"":1708430,""UnladenMass"":636,""CargoCapacity"":82,""MaxJumpRange"":8.985727,""FuelCapacity"":{""Main"":32,""Reserve"":0.63},""Rebuy"":2293025,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""MediumHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""MediumHardpoint2"",""Item"":""hpt_pulselaserburst_gimbal_medium"",""On"":true,""Priority"":0,""Value"":48500,""Engineering"":{""BlueprintName"":""Weapon_Focused"",""Level"":3,""Quality"":1,""Modifiers"":[{""Label"":""ThermalLoad"",""Value"":0.6901,""OriginalValue"":0.67},{""Label"":""ArmourPenetration"",""Value"":63,""OriginalValue"":35},{""Label"":""MaximumRange"",""Value"":5040,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":840,""OriginalValue"":500}]}},{""Slot"":""Armour"",""Item"":""krait_mkii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size7_class1"",""On"":true,""Priority"":0,""Value"":480410},{""Slot"":""MainEngines"",""Item"":""int_engine_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size5_class1"",""On"":true,""Priority"":0,""Value"":63010},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size4_class1"",""On"":true,""Priority"":0,""Value"":11350},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size7_class1"",""On"":true,""Priority"":0,""Value"":249140},{""Slot"":""Radar"",""Item"":""int_sensors_size6_class1"",""On"":true,""Priority"":0,""Value"":88980},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size5"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot04_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot08_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot09_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.MediumHardpoint2, true);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.04));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(10.296));
                Check(mod.Damage.Value.ApproxEqualsPercent(2.45));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.49));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.6901));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(63));
                Check(mod.Range.Value.ApproxEqualsPercent(5040));
                Check(mod.Falloff.Value.ApproxEqualsPercent(840));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4.203));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.56));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(13));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(3));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(2.1));
                Check(mod.BreachMin.Value.ApproxEqualsPercent(40));
                Check(mod.BreachMax.Value.ApproxEqualsPercent(80));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(0));
                Check(mod.ThermalProportionDamage.Value.ApproxEqualsPercent(100));
            }



            {
                // thrusters clean tuning thermal spread
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Eht00FBR00,,9p300A3w00AJYG03L_W0AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":411380,""UnladenMass"":41.825,""CargoCapacity"":4,""MaxJumpRange"":8.356005,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":20822,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class5"",""On"":true,""Priority"":0,""Value"":160140},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class5"",""On"":true,""Priority"":0,""Value"":160220,""Engineering"":{""BlueprintName"":""Engine_Tuned"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_engine_cooled"",""Modifiers"":[{""Label"":""Mass"",""Value"":2.625,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":47.04,""OriginalValue"":56},{""Label"":""PowerDraw"",""Value"":3.48,""OriginalValue"":3},{""Label"":""EngineOptimalMass"",""Value"":64.8,""OriginalValue"":72},{""Label"":""EngineOptPerformance"",""Value"":128,""OriginalValue"":100},{""Label"":""EngineHeatRate"",""Value"":0.468,""OriginalValue"":1.3}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.MainEngines);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.625));
                Check(mod.Integrity.Value.ApproxEqualsPercent(47.04));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(3.48));
                Check(mod.MinMass.Value.ApproxEqualsPercent(32.4));
                Check(mod.OptMass.Value.ApproxEqualsPercent(64.8));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(97.2));
                Check(mod.EngineMinMultiplier.Value.ApproxEqualsPercent(122.8));
                Check(mod.EngineOptMultiplier.Value.ApproxEqualsPercent(128));
                Check(mod.EngineMaxMultiplier.Value.ApproxEqualsPercent(148.48));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.468));
            }
            {
                // thrusters strengthing drive distrubutors
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBR00FBR00,,9p300A4Y00AKAG07J_W0AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":26930,""UnladenMass"":43.525,""CargoCapacity"":4,""MaxJumpRange"":8.034074,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1600,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""Engine_Reinforced"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_engine_haulage"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.125,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":96.6,""OriginalValue"":46},{""Label"":""EngineOptimalMass"",""Value"":52.8,""OriginalValue"":48},{""Label"":""EngineHeatRate"",""Value"":0.65,""OriginalValue"":1.3}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.MainEngines);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.125));
                Check(mod.Integrity.Value.ApproxEqualsPercent(96.6));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(2));
                Check(mod.MinMass.Value.ApproxEqualsPercent(26.4));
                Check(mod.OptMass.Value.ApproxEqualsPercent(52.8));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(79.2));
                Check(mod.EngineMinMultiplier.Value.ApproxEqualsPercent(83));
                Check(mod.EngineOptMultiplier.Value.ApproxEqualsPercent(100));
                Check(mod.EngineMaxMultiplier.Value.ApproxEqualsPercent(103));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.65));
            }

            {
                // enhanced performance thrusters dirty tuning drive distributors
                string t = @"{""event"":""Loadout"",""Ship"":""cobramkiii"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":186260,""ModulesValue"":5178500,""UnladenMass"":220,""CargoCapacity"":0,""MaxJumpRange"":12.113121,""FuelCapacity"":{""Main"":16,""Reserve"":0.49},""Rebuy"":268238,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""Armour"",""Item"":""cobramkiii_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size4_class1"",""On"":true,""Priority"":0,""Value"":17790},{""Slot"":""MainEngines"",""Item"":""int_engine_size3_class5_fast"",""On"":true,""Priority"":0,""Value"":5103950,""Engineering"":{""BlueprintName"":""Engine_Dirty"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_engine_haulage"",""Modifiers"":[{""Label"":""Integrity"",""Value"":46.75,""OriginalValue"":55},{""Label"":""PowerDraw"",""Value"":5.6,""OriginalValue"":5},{""Label"":""EngineOptimalMass"",""Value"":86.625,""OriginalValue"":90},{""Label"":""EngineOptPerformance"",""Value"":161,""OriginalValue"":115},{""Label"":""EngineHeatRate"",""Value"":2.08,""OriginalValue"":1.3}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size4_class1"",""On"":true,""Priority"":0,""Value"":19880},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size3_class1"",""On"":true,""Priority"":0,""Value"":4050},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size3_class1"",""On"":true,""Priority"":0,""Value"":4050},{""Slot"":""Radar"",""Item"":""int_sensors_size3_class1"",""On"":true,""Priority"":0,""Value"":4050},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size4_class3"",""On"":true,""Priority"":0,""Value"":24730}]}";
                var mod = GetModule(t, ShipSlots.Slot.MainEngines);

                Check(mod.Mass.Value.ApproxEqualsPercent(5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(46.75));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(5.6));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.MinMass.Value.ApproxEqualsPercent(67.38));
                Check(mod.OptMass.Value.ApproxEqualsPercent(86.63));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(192.5));
                Check(mod.EngineMinMultiplier.Value.ApproxEqualsPercent(126));
                Check(mod.EngineOptMultiplier.Value.ApproxEqualsPercent(161));
                Check(mod.EngineMaxMultiplier.Value.ApproxEqualsPercent(191.7));
                Check(mod.MinimumSpeedModifier.Value.ApproxEqualsPercent(126));
                Check(mod.OptimalSpeedModifier.Value.ApproxEqualsPercent(175));
                Check(mod.MaximumSpeedModifier.Value.ApproxEqualsPercent(224));
                Check(mod.MinimumAccelerationModifier.Value.ApproxEqualsPercent(126));
                Check(mod.OptimalAccelerationModifier.Value.ApproxEqualsPercent(154));
                Check(mod.MaximumAccelerationModifier.Value.ApproxEqualsPercent(168));
                Check(mod.MinimumRotationModifier.Value.ApproxEqualsPercent(126));
                Check(mod.OptimalRotationModifier.Value.ApproxEqualsPercent(154));
                Check(mod.MaximumRotationModifier.Value.ApproxEqualsPercent(182));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(2.08));
            }


            {
                // power plant armoured monstered
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Eht00FBR00,,9p300A3wG03I_W0AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":253140,""UnladenMass"":42.116,""CargoCapacity"":4,""MaxJumpRange"":8.29908,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":12910,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class5"",""On"":true,""Priority"":0,""Value"":160140,""Engineering"":{""BlueprintName"":""PowerPlant_Armoured"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerplant_highcharge"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.716,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":123.2,""OriginalValue"":56},{""Label"":""PowerCapacity"",""Value"":11.2896,""OriginalValue"":9.6},{""Label"":""HeatEfficiency"",""Value"":0.352,""OriginalValue"":0.4}]}},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.PowerPlant);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.7165));
                Check(mod.Integrity.Value.ApproxEqualsPercent(123.2));
                Check(mod.PowerGen.Value.ApproxEqualsPercent(11.29));
                Check(mod.HeatEfficiency.Value.ApproxEqualsPercent(0.352));
            }

            {
                // power plant overcharge thermal spread
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Eht00FBR00,,9p300A3wG07K_W0AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":253140,""UnladenMass"":41.7,""CargoCapacity"":4,""MaxJumpRange"":8.380697,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":12910,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class5"",""On"":true,""Priority"":0,""Value"":160140,""Engineering"":{""BlueprintName"":""PowerPlant_Boosted"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerplant_cooled"",""Modifiers"":[{""Label"":""Integrity"",""Value"":42,""OriginalValue"":56},{""Label"":""PowerCapacity"",""Value"":13.44,""OriginalValue"":9.6},{""Label"":""HeatEfficiency"",""Value"":0.45,""OriginalValue"":0.4}]}},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.PowerPlant);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(42));
                Check(mod.PowerGen.Value.ApproxEqualsPercent(13.44));
                Check(mod.HeatEfficiency.Value.ApproxEqualsPercent(0.45));
            }



            {
                // chaff
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":8668390,""UnladenMass"":1067.25,""CargoCapacity"":50,""MaxJumpRange"":9.632341,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7555810,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_chafflauncher_tiny"",""On"":true,""Priority"":0,""Value"":8500,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":80,""OriginalValue"":20}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size7"",""Item"":""int_refinery_size4_class5"",""On"":true,""Priority"":0,""Value"":4500850},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot13_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot14_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(80));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(4));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(4));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 10);
                Check(mod.ReloadTime == 10);
                Check(mod.Time == 20);
            }
            {
                // chaff ammo cap
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":8668390,""UnladenMass"":1066.6,""CargoCapacity"":50,""MaxJumpRange"":9.638182,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7555810,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_chafflauncher_tiny"",""On"":true,""Priority"":0,""Value"":8500,""Engineering"":{""BlueprintName"":""Misc_ChaffCapacity"",""Level"":1,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":2.6,""OriginalValue"":1.3},{""Label"":""AmmoMaximum"",""Value"":15,""OriginalValue"":10},{""Label"":""ReloadTime"",""Value"":11,""OriginalValue"":10}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size7"",""Item"":""int_refinery_size4_class5"",""On"":true,""Priority"":0,""Value"":4500850},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot13_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot14_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.6));
                Check(mod.Integrity.Value.ApproxEqualsPercent(20));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(4));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(4));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 15);
                Check(mod.ReloadTime == 11);
                Check(mod.Time == 20);
            }

            {
                // ecm reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":8672390,""UnladenMass"":1067.25,""CargoCapacity"":50,""MaxJumpRange"":9.632341,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7556010,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_electroniccountermeasure_tiny"",""On"":true,""Priority"":0,""Value"":12500,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":80,""OriginalValue"":20}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size7"",""Item"":""int_refinery_size4_class5"",""On"":true,""Priority"":0,""Value"":4500850},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot13_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot14_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(80));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Time == 3);
                Check(mod.ActivePower.Value.ApproxEqualsPercent(4));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(4));
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(10));
            }

            {
                //  heat sink reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":8663390,""UnladenMass"":1067.25,""CargoCapacity"":50,""MaxJumpRange"":9.632341,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7555560,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":0,""Value"":3500,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":180,""OriginalValue"":45}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size7"",""Item"":""int_refinery_size4_class5"",""On"":true,""Priority"":0,""Value"":4500850},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot13_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot14_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(180));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.2));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 2);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(10));
                Check(mod.Time.Value.ApproxEqualsPercent(10));
                Check(mod.ThermalDrain.Value.ApproxEqualsPercent(100));
            }
            {
                //  heat sink ammo cap
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":8663390,""UnladenMass"":1066.6,""CargoCapacity"":50,""MaxJumpRange"":9.638182,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7555560,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_fixed_small"",""On"":true,""Priority"":0,""Value"":2200},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_heatsinklauncher_turret_tiny"",""On"":true,""Priority"":0,""Value"":3500,""Engineering"":{""BlueprintName"":""Misc_HeatSinkCapacity"",""Level"":1,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":2.6,""OriginalValue"":1.3},{""Label"":""AmmoMaximum"",""Value"":3,""OriginalValue"":2},{""Label"":""ReloadTime"",""Value"":15,""OriginalValue"":10}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750},{""Slot"":""Slot01_Size7"",""Item"":""int_refinery_size4_class5"",""On"":true,""Priority"":0,""Value"":4500850},{""Slot"":""Slot02_Size6"",""Item"":""int_cargorack_size5_class1"",""On"":true,""Priority"":0,""Value"":111570},{""Slot"":""Slot03_Size6"",""Item"":""int_shieldgenerator_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""Slot05_Size5"",""Item"":""int_cargorack_size4_class1"",""On"":true,""Priority"":0,""Value"":34330},{""Slot"":""Slot13_Size2"",""Item"":""int_cargorack_size1_class1"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot14_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":9120}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.6));
                Check(mod.Integrity.Value.ApproxEqualsPercent(45));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.2));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(5));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 3);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(15));
                Check(mod.Time.Value.ApproxEqualsPercent(10));
                Check(mod.ThermalDrain.Value.ApproxEqualsPercent(100));
            }
            {
                // kill warrant reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":4895970,""UnladenMass"":1023.25,""CargoCapacity"":0,""MaxJumpRange"":10.044399,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7367189,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_crimescanner_size0_class5"",""On"":true,""Priority"":0,""Value"":1097100,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":192,""OriginalValue"":48}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(192));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(3.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(2));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Angle.Value.ApproxEqualsPercent(15));
                Check(mod.Time.Value.ApproxEqualsPercent(10));
            }
            {
                // manifest scanner shielded
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":4895970,""UnladenMass"":1021.3,""CargoCapacity"":0,""MaxJumpRange"":10.063478,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7367189,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_cargoscanner_size0_class5"",""On"":true,""Priority"":0,""Value"":1097100,""Engineering"":{""BlueprintName"":""Misc_Shielded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Integrity"",""Value"":192,""OriginalValue"":48},{""Label"":""PowerDraw"",""Value"":6.4,""OriginalValue"":3.2}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(192));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(6.4));
                Check(mod.BootTime.Value.ApproxEqualsPercent(3));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Angle.Value.ApproxEqualsPercent(15));
                Check(mod.Time.Value.ApproxEqualsPercent(10));
            }
            {
                // point defence
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":3817420,""UnladenMass"":1021,""CargoCapacity"":0,""MaxJumpRange"":10.06642,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7313262,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_plasmapointdefence_turret_tiny"",""On"":true,""Priority"":0,""Value"":18550,""Engineering"":{""BlueprintName"":""Misc_PointDefenseCapacity"",""Level"":1,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":1,""OriginalValue"":0.5},{""Label"":""AmmoMaximum"",""Value"":15000,""OriginalValue"":10000},{""Label"":""ReloadTime"",""Value"":0.44,""OriginalValue"":0.4}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(1));
                Check(mod.Integrity.Value.ApproxEqualsPercent(30));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.DPS.Value.ApproxEqualsPercent(2));
                Check(mod.Damage.Value.ApproxEqualsPercent(0.2));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.07));
                Check(mod.Range.Value.ApproxEqualsPercent(2500));
                Check(mod.Speed.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(10));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.2));
                Check(mod.BurstRateOfFire.Value.ApproxEqualsPercent(15));
                Check(mod.BurstSize.Value.ApproxEqualsPercent(4));
                Check(mod.Clip.Value == 12);
                Check(mod.Ammo.Value == 15000);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(0.44));
                Check(mod.Jitter.Value.ApproxEqualsPercent(0.75));
                Check(mod.KineticProportionDamage.Value.ApproxEqualsPercent(100));
            }
            {
                // frame shift wake scanner
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":4895970,""UnladenMass"":1022.6,""CargoCapacity"":0,""MaxJumpRange"":10.050751,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7367189,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_cloudscanner_size0_class5"",""On"":true,""Priority"":0,""Value"":1097100,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":2.6,""OriginalValue"":1.3},{""Label"":""MaxAngle"",""Value"":45,""OriginalValue"":15},{""Label"":""ScannerTimeToScan"",""Value"":15,""OriginalValue"":10}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.6));
                Check(mod.Integrity.Value.ApproxEqualsPercent(48));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(3.2));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Angle.Value.ApproxEqualsPercent(45));
                Check(mod.Time.Value.ApproxEqualsPercent(15));
            }
            {
                // shield defence
                string t = @"{""event"":""Loadout"",""Ship"":""anaconda"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":142447820,""ModulesValue"":4079870,""UnladenMass"":1034,""CargoCapacity"":0,""MaxJumpRange"":9.940505,""FuelCapacity"":{""Main"":32,""Reserve"":1.07},""Rebuy"":7326384,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""TinyHardpoint1"",""Item"":""hpt_shieldbooster_size0_class5"",""On"":true,""Priority"":0,""Value"":281000,""Engineering"":{""BlueprintName"":""ShieldBooster_HeavyDuty"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shieldbooster_chunky"",""Modifiers"":[{""Label"":""Mass"",""Value"":14,""OriginalValue"":3.5},{""Label"":""Integrity"",""Value"":55.2,""OriginalValue"":48},{""Label"":""PowerDraw"",""Value"":1.5,""OriginalValue"":1.2},{""Label"":""DefenceModifierShieldMultiplier"",""Value"":73.88,""OriginalValue"":20},{""Label"":""KineticResistance"",""Value"":-2,""OriginalValue"":0},{""Label"":""ThermicResistance"",""Value"":-2,""OriginalValue"":0},{""Label"":""ExplosiveResistance"",""Value"":-2,""OriginalValue"":0}]}},{""Slot"":""Armour"",""Item"":""anaconda_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size8_class1"",""On"":true,""Priority"":0,""Value"":1441230},{""Slot"":""MainEngines"",""Item"":""int_engine_size7_class1"",""On"":true,""Priority"":0,""Value"":633200},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size6_class1"",""On"":true,""Priority"":0,""Value"":199750},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size5_class1"",""On"":true,""Priority"":0,""Value"":31780},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""Radar"",""Item"":""int_sensors_size8_class1"",""On"":true,""Priority"":0,""Value"":697580},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size5_class3"",""On"":true,""Priority"":0,""Value"":97750}]}";
                var mod = GetModule(t, ShipSlots.Slot.TinyHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(14));
                Check(mod.Integrity.Value.ApproxEqualsPercent(55.2));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.5));
                Check(mod.BootTime.Value.ApproxEqualsPercent(0));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(73.88));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-2));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-2));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-2));
            }

            {
                // SCB rapid charge flow control
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":81500,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4328,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldcellbank_size2_class5"",""On"":true,""Priority"":0,""Value"":56550,""Engineering"":{""BlueprintName"":""ShieldCellBank_Rapid"",""Level"":4,""Quality"":1,""ExperimentalEffect"":""special_shieldcell_efficient"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":1.062,""OriginalValue"":1.18},{""Label"":""BootTime"",""Value"":31.25,""OriginalValue"":25},{""Label"":""ShieldBankSpinUp"",""Value"":3,""OriginalValue"":5},{""Label"":""ShieldBankDuration"",""Value"":1.14,""OriginalValue"":1.5},{""Label"":""ShieldBankReinforcement"",""Value"":38.4,""OriginalValue"":32}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(61));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.062));
                Check(mod.BootTime.Value.ApproxEqualsPercent(31.25));
                Check(mod.SCBSpinUp.Value.ApproxEqualsPercent(3));
                Check(mod.SCBDuration.Value.ApproxEqualsPercent(1.14));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(38.4));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(240));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 3);
            }
            {
                // SCB specialised boss cells
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":81500,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4328,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldcellbank_size2_class5"",""On"":true,""Priority"":0,""Value"":56550,""Engineering"":{""BlueprintName"":""ShieldCellBank_Specialised"",""Level"":4,""Quality"":1,""ExperimentalEffect"":""special_shieldcell_oversized"",""Modifiers"":[{""Label"":""Integrity"",""Value"":48.8,""OriginalValue"":61},{""Label"":""PowerDraw"",""Value"":1.475,""OriginalValue"":1.18},{""Label"":""BootTime"",""Value"":17,""OriginalValue"":25},{""Label"":""ShieldBankSpinUp"",""Value"":6,""OriginalValue"":5},{""Label"":""ShieldBankReinforcement"",""Value"":36.96,""OriginalValue"":32},{""Label"":""ShieldBankHeat"",""Value"":182.4,""OriginalValue"":240}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(48.8));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.475));
                Check(mod.BootTime.Value.ApproxEqualsPercent(17));
                Check(mod.SCBSpinUp.Value.ApproxEqualsPercent(6));
                Check(mod.SCBDuration.Value.ApproxEqualsPercent(1.5));
                Check(mod.ShieldReinforcement.Value.ApproxEqualsPercent(36.96));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(182.4));
                Check(mod.Clip == 1);
                Check(mod.Ammo == 3);
            }

            {
                // detailed surface scanner expanded
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":274950,""UnladenMass"":40.4,""CargoCapacity"":4,""MaxJumpRange"":8.646427,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":14001,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_detailedsurfacescanner_tiny"",""On"":true,""Priority"":0,""Value"":250000,""Engineering"":{""BlueprintName"":""Sensor_Expanded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""DSS_PatchRadius"",""Value"":30,""OriginalValue"":20}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Integrity.Value.ApproxEqualsPercent(20));
                Check(mod.Clip == 3);
                Check(mod.ProbeRadius.Value.ApproxEqualsPercent(30));
            }

            {
                // frame shift drive interdictos expanded arc capture
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":2746550,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":137581,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_fsdinterdictor_size2_class5"",""On"":true,""Priority"":0,""Value"":2721600,""Engineering"":{""BlueprintName"":""FSDinterdictor_Expanded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.585,""OriginalValue"":0.39},{""Label"":""FSDInterdictorRange"",""Value"":7,""OriginalValue"":10},{""Label"":""FSDInterdictorFacingLimit"",""Value"":110,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(61));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.585));
                Check(mod.BootTime.Value.ApproxEqualsPercent(15));
                Check(mod.TargetMaxTime == 7);
                Check(mod.Angle.Value == 110);
            }

            {
                // frame shift drive interdictos expanded arc capture
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":2746550,""UnladenMass"":43.65,""CargoCapacity"":4,""MaxJumpRange"":8.011378,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":137581,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_fsdinterdictor_size2_class5"",""On"":true,""Priority"":0,""Value"":2721600,""Engineering"":{""BlueprintName"":""FSDinterdictor_LongRange"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":2.5},{""Label"":""PowerDraw"",""Value"":0.585,""OriginalValue"":0.39},{""Label"":""FSDInterdictorRange"",""Value"":16,""OriginalValue"":10},{""Label"":""FSDInterdictorFacingLimit"",""Value"":35,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(61));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.585));
                Check(mod.BootTime.Value.ApproxEqualsPercent(15));
                Check(mod.TargetMaxTime == 16);
                Check(mod.Angle.Value == 35);
            }

            {
                // fuel scoop shielded
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":309790,""UnladenMass"":40.4,""CargoCapacity"":4,""MaxJumpRange"":8.646427,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":15743,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_fuelscoop_size2_class5"",""On"":true,""Priority"":0,""Value"":284840,""Engineering"":{""BlueprintName"":""Misc_Shielded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Integrity"",""Value"":244,""OriginalValue"":61},{""Label"":""PowerDraw"",""Value"":0.78,""OriginalValue"":0.39}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Integrity.Value.ApproxEqualsPercent(244));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.78));
                Check(mod.BootTime.Value.ApproxEqualsPercent(4));
                Check(mod.RefillRate.Value.ApproxEquals(0.075));
            }

            {
                // fuel transfer limpet lightweight
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34550,""UnladenMass"":40.595,""CargoCapacity"":4,""MaxJumpRange"":8.605498,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1981,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_fueltransfer_size1_class5"",""On"":true,""Priority"":0,""Value"":9600,""Engineering"":{""BlueprintName"":""Misc_LightWeight"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":0.195,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":28,""OriginalValue"":56}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.195));
                Check(mod.Integrity.Value.ApproxEqualsPercent(28));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.28));
                Check(mod.BootTime.Value.ApproxEqualsPercent(10));
                Check(mod.Limpets == 1);
                Check(mod.Range == 1400);
                Check(mod.Time.Value == 60);
                Check(mod.Speed.Value == 200);
                Check(mod.FuelTransfer == 1);
            }

            {
                // fuel transfer limpet reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34550,""UnladenMass"":43.65,""CargoCapacity"":4,""MaxJumpRange"":8.011378,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1981,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_fueltransfer_size1_class5"",""On"":true,""Priority"":0,""Value"":9600,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":224,""OriginalValue"":56}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(224));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.28));
                Check(mod.BootTime.Value.ApproxEqualsPercent(10));
                Check(mod.Limpets == 1);
                Check(mod.Range == 1400);
                Check(mod.Time.Value == 60);
                Check(mod.Speed.Value == 200);
                Check(mod.FuelTransfer == 1);
            }
            {
                // prospector limpet reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34550,""UnladenMass"":43.65,""CargoCapacity"":4,""MaxJumpRange"":8.011378,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1981,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_prospector_size1_class5"",""On"":true,""Priority"":0,""Value"":9600,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":224,""OriginalValue"":56}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(224));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.28));
                Check(mod.BootTime.Value.ApproxEqualsPercent(4));
                Check(mod.Limpets == 1);
                Check(mod.Range == 7000);
                Check(mod.Speed.Value == 200);
            }
            {
                // refinery shielded
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":1045550,""UnladenMass"":40.4,""CargoCapacity"":4,""MaxJumpRange"":8.646427,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":52531,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_refinery_size2_class5"",""On"":true,""Priority"":0,""Value"":1020600,""Engineering"":{""BlueprintName"":""Misc_Shielded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Integrity"",""Value"":244,""OriginalValue"":61},{""Label"":""PowerDraw"",""Value"":0.78,""OriginalValue"":0.39}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Integrity.Value.ApproxEqualsPercent(244));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.78));
                Check(mod.BootTime.Value.ApproxEqualsPercent(10));
                Check(mod.Capacity == 6);
            }

            {
                // hatch breaker shielded
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34550,""UnladenMass"":41.7,""CargoCapacity"":4,""MaxJumpRange"":8.380697,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1981,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_resourcesiphon_size1_class5"",""On"":true,""Priority"":0,""Value"":9600,""Engineering"":{""BlueprintName"":""Misc_Shielded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Integrity"",""Value"":192,""OriginalValue"":48},{""Label"":""PowerDraw"",""Value"":0.56,""OriginalValue"":0.28}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(192));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.56));
                Check(mod.BootTime.Value.ApproxEqualsPercent(3));
                Check(mod.Limpets == 1);
                Check(mod.Range == 3600);
                Check(mod.TargetRange == 3500);
                Check(mod.Time.Value == 120);
                Check(mod.Speed.Value == 500);
                Check(mod.HackTime.Value == 10);
                Check(mod.MinCargo == 5);
                Check(mod.MaxCargo == 10);
            }

            {
                // shields enhanced low power multi weave
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":41.845,""CargoCapacity"":4,""MaxJumpRange"":18.234246,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Optimised"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_resistive"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.25,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":30.75,""OriginalValue"":41},{""Label"":""PowerDraw"",""Value"":0.594,""OriginalValue"":0.9},{""Label"":""ShieldGenOptimalMass"",""Value"":51.7,""OriginalValue"":55},{""Label"":""ShieldGenStrength"",""Value"":92,""OriginalValue"":80},{""Label"":""EnergyPerRegen"",""Value"":0.75,""OriginalValue"":0.6},{""Label"":""KineticResistance"",""Value"":41.8,""OriginalValue"":40},{""Label"":""ThermicResistance"",""Value"":-16.4,""OriginalValue"":-20},{""Label"":""ExplosiveResistance"",""Value"":51.5,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(30.75));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.594));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(26.32));
                Check(mod.OptMass.Value.ApproxEqualsPercent(51.7));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(34.5));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(92));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(149.5));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.6));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.75));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(41.8));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-16.4));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(51.5));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }

            {
                // shields enhanced low power force block
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":41.845,""CargoCapacity"":4,""MaxJumpRange"":18.234246,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Optimised"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_kinetic"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.25,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":30.75,""OriginalValue"":41},{""Label"":""PowerDraw"",""Value"":0.54,""OriginalValue"":0.9},{""Label"":""ShieldGenOptimalMass"",""Value"":51.7,""OriginalValue"":55},{""Label"":""ShieldGenStrength"",""Value"":89.24,""OriginalValue"":80},{""Label"":""KineticResistance"",""Value"":44.8,""OriginalValue"":40}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(30.75));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.54));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(26.32));
                Check(mod.OptMass.Value.ApproxEqualsPercent(51.7));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(33.46));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(89.24));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(145.02));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.6));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.6));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(44.8));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-20));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(50));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }

            {
                // shields  enhanced low power thermo block
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":41.845,""CargoCapacity"":4,""MaxJumpRange"":18.234246,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Optimised"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_thermic"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.25,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":30.75,""OriginalValue"":41},{""Label"":""PowerDraw"",""Value"":0.54,""OriginalValue"":0.9},{""Label"":""ShieldGenOptimalMass"",""Value"":51.7,""OriginalValue"":55},{""Label"":""ShieldGenStrength"",""Value"":89.24,""OriginalValue"":80},{""Label"":""ThermicResistance"",""Value"":-10.4,""OriginalValue"":-20}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(30.75));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.54));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(26.32));
                Check(mod.OptMass.Value.ApproxEqualsPercent(51.7));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(33.46));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(89.24));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(145.02));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.6));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.6));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(40));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-10.4));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(50));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }


            {
                // shields kinetic multi weave
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.095,""CargoCapacity"":4,""MaxJumpRange"":17.716169,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Kinetic"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_resistive"",""Modifiers"":[{""Label"":""Integrity"",""Value"":57.4,""OriginalValue"":41},{""Label"":""PowerDraw"",""Value"":0.99,""OriginalValue"":0.9},{""Label"":""EnergyPerRegen"",""Value"":0.75,""OriginalValue"":0.6},{""Label"":""KineticResistance"",""Value"":70.9,""OriginalValue"":40},{""Label"":""ThermicResistance"",""Value"":-33.86,""OriginalValue"":-20},{""Label"":""ExplosiveResistance"",""Value"":51.5,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(57.4));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.99));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(28));
                Check(mod.OptMass.Value.ApproxEqualsPercent(55));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(30));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(80));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(130));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.6));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.75));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(70.9));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-33.86));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(51.5));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }
            {
                // shields reinforced lo draw
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.095,""CargoCapacity"":4,""MaxJumpRange"":17.716169,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Reinforced"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_efficient"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.72,""OriginalValue"":0.9},{""Label"":""ShieldGenStrength"",""Value"":108.192,""OriginalValue"":80},{""Label"":""BrokenRegenRate"",""Value"":1.44,""OriginalValue"":1.6},{""Label"":""EnergyPerRegen"",""Value"":0.5376,""OriginalValue"":0.6},{""Label"":""KineticResistance"",""Value"":49.399,""OriginalValue"":40},{""Label"":""ThermicResistance"",""Value"":-1.202,""OriginalValue"":-20},{""Label"":""ExplosiveResistance"",""Value"":57.8325,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(41));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.72));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(28));
                Check(mod.OptMass.Value.ApproxEqualsPercent(55));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(40.57));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(108.19));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(175.81));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.44));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.5376));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(49.4));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-1.202));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(57.83));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }
            {
                // shields thermal fast charge
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.095,""CargoCapacity"":4,""MaxJumpRange"":17.716169,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""ShieldGenerator_Thermic"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_shield_regenerative"",""Modifiers"":[{""Label"":""Integrity"",""Value"":57.4,""OriginalValue"":41},{""Label"":""RegenRate"",""Value"":1.15,""OriginalValue"":1},{""Label"":""BrokenRegenRate"",""Value"":1.84,""OriginalValue"":1.6},{""Label"":""KineticResistance"",""Value"":26.92,""OriginalValue"":40},{""Label"":""ThermicResistance"",""Value"":39.1,""OriginalValue"":-20},{""Label"":""ExplosiveResistance"",""Value"":49.25,""OriginalValue"":50}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(57.4));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.9));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.MinMass.Value.ApproxEqualsPercent(28));
                Check(mod.OptMass.Value.ApproxEqualsPercent(55));
                Check(mod.MaxMass.Value.ApproxEqualsPercent(138));
                Check(mod.MinStrength.Value.ApproxEqualsPercent(30));
                Check(mod.OptStrength.Value.ApproxEqualsPercent(80));
                Check(mod.MaxStrength.Value.ApproxEqualsPercent(130));
                Check(mod.RegenRate.Value.ApproxEqualsPercent(1.15));
                Check(mod.BrokenRegenRate.Value.ApproxEqualsPercent(1.84));
                Check(mod.MWPerUnit.Value.ApproxEqualsPercent(0.6));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(26.92));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(39.1));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(49.25));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(95));
            }

            {
                // afm shielded
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":1641190,""UnladenMass"":40.595,""CargoCapacity"":4,""MaxJumpRange"":18.783537,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":82313,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_repairer_size2_class5"",""On"":true,""Priority"":0,""Value"":1458000,""Engineering"":{""BlueprintName"":""Misc_Shielded"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Integrity"",""Value"":236,""OriginalValue"":59},{""Label"":""PowerDraw"",""Value"":3.16,""OriginalValue"":1.58}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Integrity.Value.ApproxEqualsPercent(236));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(3.16));
                Check(mod.BootTime.Value.ApproxEqualsPercent(9));
                Check(mod.Ammo.Value == 2500);
                Check(mod.RateOfRepairConsumption.Value.ApproxEqualsPercent(10));
                Check(mod.RepairCostPerMat.Value.ApproxEqualsPercent(0.028));
            }
            {
                // collection climpet lightweight
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":187990,""UnladenMass"":40.7,""CargoCapacity"":4,""MaxJumpRange"":18.736127,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9653,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_collection_size1_class4"",""On"":true,""Priority"":0,""Value"":4800,""Engineering"":{""BlueprintName"":""Misc_LightWeight"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":0.3,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":24,""OriginalValue"":48}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(24));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.28));
                Check(mod.BootTime.Value.ApproxEqualsPercent(6));
                Check(mod.Limpets == 1);
                Check(mod.Range == 1400);
                Check(mod.Time.Value == 420);
                Check(mod.Speed.Value == 200);
                Check(mod.MultiTargetSpeed == 60);
            }
            {
                // collection climpet reinforced
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":187990,""UnladenMass"":45.4,""CargoCapacity"":4,""MaxJumpRange"":16.834187,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9653,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_dronecontrol_collection_size1_class4"",""On"":true,""Priority"":0,""Value"":4800,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":5,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":192,""OriginalValue"":48}]}},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Slot01_Size2);

                Check(mod.Mass.Value.ApproxEqualsPercent(5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(192));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.28));
                Check(mod.BootTime.Value.ApproxEqualsPercent(6));
                Check(mod.Limpets == 1);
                Check(mod.Range == 1400);
                Check(mod.Time.Value == 420);
                Check(mod.Speed.Value == 200);
                Check(mod.MultiTargetSpeed == 60);
            }


            {
                // rail gun nothing
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,KYi00FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":71930,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3850,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_railgun_fixed_small"",""On"":true,""Priority"":0,""Value"":51600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.15));
                Check(mod.DPS.Value.ApproxEqualsPercent(14.319));
                Check(mod.Damage.Value.ApproxEqualsPercent(23.34));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2.69));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(12));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(100));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6135));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.63));
                Check(mod.Clip.Value == 1);
                Check(mod.Ammo.Value == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(22.2));
            }

            {
                // railgun high cap plasma slug
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,KYiG03M_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":71930,""UnladenMass"":44.1,""CargoCapacity"":4,""MaxJumpRange"":7.930727,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3850,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_railgun_fixed_small"",""On"":true,""Priority"":0,""Value"":51600,""Engineering"":{""BlueprintName"":""Weapon_HighCapacity"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_plasma_slug_cooled"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.2,""OriginalValue"":2},{""Label"":""PowerDraw"",""Value"":1.38,""OriginalValue"":1.15},{""Label"":""DamagePerSecond"",""Value"":13.405233,""OriginalValue"":14.319018},{""Label"":""Damage"",""Value"":21.006,""OriginalValue"":23.34},{""Label"":""ThermalLoad"",""Value"":7.2,""OriginalValue"":12},{""Label"":""RateOfFire"",""Value"":0.638162,""OriginalValue"":0.613497},{""Label"":""AmmoClipSize"",""Value"":2,""OriginalValue"":1},{""Label"":""AmmoMaximum"",""Value"":0,""OriginalValue"":80}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.38));
                Check(mod.DPS.Value.ApproxEqualsPercent(13.405));
                Check(mod.Damage.Value.ApproxEqualsPercent(21.01));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2.69));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(7.2));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(100));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6382));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.567));
                Check(mod.Clip.Value == 2);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(19.98));
            }


            {
                // rail gun long range feedback cascade
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,KYiG07I_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":71930,""UnladenMass"":43.5,""CargoCapacity"":4,""MaxJumpRange"":8.038628,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3850,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_railgun_fixed_small"",""On"":true,""Priority"":0,""Value"":51600,""Engineering"":{""BlueprintName"":""Weapon_LongRange"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_feedback_cascade_cooled"",""Modifiers"":[{""Label"":""Mass"",""Value"":2.6,""OriginalValue"":2},{""Label"":""PowerDraw"",""Value"":1.3225,""OriginalValue"":1.15},{""Label"":""DamagePerSecond"",""Value"":11.455215,""OriginalValue"":14.319018},{""Label"":""Damage"",""Value"":18.672,""OriginalValue"":23.34},{""Label"":""ThermalLoad"",""Value"":7.2,""OriginalValue"":12},{""Label"":""MaximumRange"",""Value"":6000,""OriginalValue"":3000},{""Label"":""FalloffRange"",""Value"":6000,""OriginalValue"":1000}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.6));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(1.3225));
                Check(mod.DPS.Value.ApproxEqualsPercent(11.455));
                Check(mod.Damage.Value.ApproxEqualsPercent(18.67));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2.69));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(7.2));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(100));
                Check(mod.Range.Value.ApproxEqualsPercent(6000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(6000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6135));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.63));
                Check(mod.Clip.Value == 1);
                Check(mod.Ammo.Value == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(17.76));
            }


            {
                // rail gun light weight feedback cascade
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,KYiG05I_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":71930,""UnladenMass"":41.1,""CargoCapacity"":4,""MaxJumpRange"":8.501283,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3850,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_railgun_fixed_small"",""On"":true,""Priority"":0,""Value"":51600,""Engineering"":{""BlueprintName"":""Weapon_LightWeight"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_feedback_cascade_cooled"",""Modifiers"":[{""Label"":""Mass"",""Value"":0.2,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":16,""OriginalValue"":40},{""Label"":""PowerDraw"",""Value"":0.69,""OriginalValue"":1.15},{""Label"":""DamagePerSecond"",""Value"":11.455215,""OriginalValue"":14.319018},{""Label"":""Damage"",""Value"":18.672,""OriginalValue"":23.34},{""Label"":""DistributorDraw"",""Value"":1.7485,""OriginalValue"":2.69},{""Label"":""ThermalLoad"",""Value"":7.2,""OriginalValue"":12}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(16));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.69));
                Check(mod.DPS.Value.ApproxEqualsPercent(11.455));
                Check(mod.Damage.Value.ApproxEqualsPercent(18.67));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(1.749));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(7.2));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(100));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.6135));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.63));
                Check(mod.Clip.Value == 1);
                Check(mod.Ammo.Value == 80);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(1));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(17.76));
            }




            {
                //  missile high cap penetrator munitions
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,K38G03O_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":52510,""UnladenMass"":44.1,""CargoCapacity"":4,""MaxJumpRange"":7.930727,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":2879,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_dumbfiremissilerack_fixed_small"",""On"":true,""Priority"":0,""Value"":32180,""Engineering"":{""BlueprintName"":""Weapon_HighCapacity"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_penetrator_munitions"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.2,""OriginalValue"":2},{""Label"":""PowerDraw"",""Value"":0.48,""OriginalValue"":0.4},{""Label"":""DamagePerSecond"",""Value"":27.777778,""OriginalValue"":25},{""Label"":""RateOfFire"",""Value"":0.555556,""OriginalValue"":0.5},{""Label"":""AmmoClipSize"",""Value"":16,""OriginalValue"":8},{""Label"":""AmmoMaximum"",""Value"":32,""OriginalValue"":16}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.48));
                Check(mod.DPS.Value.ApproxEqualsPercent(27.78));
                Check(mod.Damage.Value.ApproxEqualsPercent(50));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.24));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(3.6));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(60));
                Check(mod.Speed.Value.ApproxEqualsPercent(750));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.5556));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1.7777));
                Check(mod.Clip.Value == 16);
                Check(mod.Ammo.Value == 32);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(20));
            }
            {
                // missile sturdy emissive munitions
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,K38G09I_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":52510,""UnladenMass"":44.9,""CargoCapacity"":4,""MaxJumpRange"":7.791286,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":2879,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_dumbfiremissilerack_fixed_small"",""On"":true,""Priority"":0,""Value"":32180,""Engineering"":{""BlueprintName"":""Weapon_Sturdy"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_emissive_munitions"",""Modifiers"":[{""Label"":""Mass"",""Value"":4,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":160,""OriginalValue"":40},{""Label"":""ThermalLoad"",""Value"":5.04,""OriginalValue"":3.6},{""Label"":""ArmourPenetration"",""Value"":96,""OriginalValue"":60}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);


                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.Integrity.Value.ApproxEqualsPercent(160));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.4));
                Check(mod.DPS.Value.ApproxEqualsPercent(25));
                Check(mod.Damage.Value.ApproxEqualsPercent(50));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.24));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(5.04));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(96));
                Check(mod.Speed.Value.ApproxEqualsPercent(750));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.5));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(2));
                Check(mod.Clip.Value == 8);
                Check(mod.Ammo.Value == 16);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(20));
            }
            {
                string pulselaserefficientscramble = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBRG03O_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":26930,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1600,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_scramble_spectrum"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.2028,""OriginalValue"":0.39},{""Label"":""DamagePerSecond"",""Value"":6.96384,""OriginalValue"":6.24},{""Label"":""Damage"",""Value"":1.9344,""OriginalValue"":1.56},{""Label"":""DistributorDraw"",""Value"":0.1705,""OriginalValue"":0.31},{""Label"":""ThermalLoad"",""Value"":0.124,""OriginalValue"":0.31},{""Label"":""RateOfFire"",""Value"":3.6,""OriginalValue"":4}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";

                var mod = GetModule(pulselaserefficientscramble, ShipSlots.Slot.SmallHardpoint1);
                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2028));
                Check(mod.DPS.Value.ApproxEqualsPercent(6.964));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.9344));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.1705));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.124));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(20));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(3.6));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.2778));
            }

            {
                string pulseefficientemmisive = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBRG03J_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":26930,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1600,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_emissive_munitions"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.2028,""OriginalValue"":0.39},{""Label"":""DamagePerSecond"",""Value"":7.7376,""OriginalValue"":6.24},{""Label"":""Damage"",""Value"":1.9344,""OriginalValue"":1.56},{""Label"":""DistributorDraw"",""Value"":0.1705,""OriginalValue"":0.31},{""Label"":""ThermalLoad"",""Value"":0.248,""OriginalValue"":0.31}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(pulseefficientemmisive, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2028));
                Check(mod.DPS.Value.ApproxEqualsPercent(7.738));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.9344));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.1705));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.248));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(20));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.25));
            }
            {
                string burstefficientemmisive = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBRG03J_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":26930,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1600,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_emissive_munitions"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.2028,""OriginalValue"":0.39},{""Label"":""DamagePerSecond"",""Value"":7.7376,""OriginalValue"":6.24},{""Label"":""Damage"",""Value"":1.9344,""OriginalValue"":1.56},{""Label"":""DistributorDraw"",""Value"":0.1705,""OriginalValue"":0.31},{""Label"":""ThermalLoad"",""Value"":0.248,""OriginalValue"":0.31}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(burstefficientemmisive, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.2028));
                Check(mod.DPS.Value.ApproxEqualsPercent(7.738));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.9344));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.1705));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.248));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(20));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(4));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.25));
            }
            {
                // cannon efficient smart
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,H87G03P_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":62530,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3380,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_cannon_gimbal_small"",""On"":true,""Priority"":0,""Value"":42200,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_smart_rounds"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.1976,""OriginalValue"":0.38},{""Label"":""DamagePerSecond"",""Value"":10.281667,""OriginalValue"":8.291667},{""Label"":""Damage"",""Value"":19.7408,""OriginalValue"":15.92},{""Label"":""DistributorDraw"",""Value"":0.264,""OriginalValue"":0.48},{""Label"":""ThermalLoad"",""Value"":0.5,""OriginalValue"":1.25}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.1976));
                Check(mod.DPS.Value.ApproxEqualsPercent(10.282));
                Check(mod.Damage.Value.ApproxEqualsPercent(19.741));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.264));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.5));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(35));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(3000));
                Check(mod.Speed.Value.ApproxEqualsPercent(1000));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.5208));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1.92));
                Check(mod.Clip.Value == 5);
                Check(mod.Ammo.Value == 100);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(4));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(18.724));
            }
            {
                // cannon efficient force shell
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,H87G03L_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":62530,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":3380,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_cannon_gimbal_small"",""On"":true,""Priority"":0,""Value"":42200,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_force_shell"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.1976,""OriginalValue"":0.38},{""Label"":""DamagePerSecond"",""Value"":10.281667,""OriginalValue"":8.291667},{""Label"":""Damage"",""Value"":19.7408,""OriginalValue"":15.92},{""Label"":""DistributorDraw"",""Value"":0.264,""OriginalValue"":0.48},{""Label"":""ThermalLoad"",""Value"":0.5,""OriginalValue"":1.25},{""Label"":""ShotSpeed"",""Value"":833.333333,""OriginalValue"":1000}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.1976));
                Check(mod.DPS.Value.ApproxEqualsPercent(10.282));
                Check(mod.Damage.Value.ApproxEqualsPercent(19.741));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.264));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.5));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(35));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(3000));
                Check(mod.Speed.Value.ApproxEqualsPercent(833.3));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.5208));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1.92));
                Check(mod.Clip.Value == 5);
                Check(mod.Ammo.Value == 100);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(4));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(18.724));
            }

            {
                // fragment efficient special ince
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,HNlG05M_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":75050,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4006,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_slugshot_gimbal_small"",""On"":true,""Priority"":0,""Value"":54720,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_incendiary_rounds"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.3068,""OriginalValue"":0.59},{""Label"":""DamagePerSecond"",""Value"":83.984471,""OriginalValue"":71.294118},{""Label"":""Damage"",""Value"":1.2524,""OriginalValue"":1.01},{""Label"":""DistributorDraw"",""Value"":0.143,""OriginalValue"":0.26},{""Label"":""ThermalLoad"",""Value"":0.528,""OriginalValue"":0.44},{""Label"":""RateOfFire"",""Value"":5.588235,""OriginalValue"":5.882353}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.3068));
                Check(mod.DPS.Value.ApproxEqualsPercent(83.98));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.2524));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.143));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.528));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(20));
                Check(mod.Range.Value.ApproxEqualsPercent(2000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1800));
                Check(mod.Speed.Value.ApproxEqualsPercent(667));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(5.588));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.1789));
                Check(mod.Clip.Value == 3);
                Check(mod.Ammo.Value == 180);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(1.116));
            }

            {
                // fragment overchanged incendiary
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,HNlG0BM_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":75050,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4006,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_slugshot_gimbal_small"",""On"":true,""Priority"":0,""Value"":54720,""Engineering"":{""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_incendiary_rounds"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":115.14,""OriginalValue"":71.294118},{""Label"":""Damage"",""Value"":1.717,""OriginalValue"":1.01},{""Label"":""DistributorDraw"",""Value"":0.351,""OriginalValue"":0.26},{""Label"":""ThermalLoad"",""Value"":1.518,""OriginalValue"":0.44},{""Label"":""RateOfFire"",""Value"":5.588235,""OriginalValue"":5.882353}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.59));
                Check(mod.DPS.Value.ApproxEqualsPercent(115.14));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.717));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.351));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(1.518));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(20));
                Check(mod.Range.Value.ApproxEqualsPercent(2000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(1800));
                Check(mod.Speed.Value.ApproxEqualsPercent(667));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(5.588));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.1789));
                Check(mod.Clip.Value == 3);
                Check(mod.Ammo.Value == 180);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(1.53));
            }

            {
                // beam efficient thermal shock
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,EhtG03O_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":94980,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":5002,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_thermalshock"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.312,""OriginalValue"":0.6},{""Label"":""DamagePerSecond"",""Value"":8.57088,""OriginalValue"":7.68},{""Label"":""DistributorDraw"",""Value"":1.1605,""OriginalValue"":2.11},{""Label"":""ThermalLoad"",""Value"":1.46,""OriginalValue"":3.65}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.312));
                Check(mod.DPS.Value.ApproxEqualsPercent(8.571));
                Check(mod.Damage.Value.ApproxEqualsPercent(8.571));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(1.161));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(1.46));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(18));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(600));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(6.808));
            }

            {
                // beam efficient concordant
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,EhtG03H_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":94980,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":5002,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_concordant_sequence"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.312,""OriginalValue"":0.6},{""Label"":""DamagePerSecond"",""Value"":9.5232,""OriginalValue"":7.68},{""Label"":""DistributorDraw"",""Value"":1.1605,""OriginalValue"":2.11},{""Label"":""ThermalLoad"",""Value"":2.19,""OriginalValue"":3.65}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.312));
                Check(mod.DPS.Value.ApproxEqualsPercent(9.523));
                Check(mod.Damage.Value.ApproxEqualsPercent(9.523));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(1.161));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(2.19));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(18));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(600));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(7.564));
            }
            {
                // beam overchanged thermal conduit
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,EhtG09N_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":94980,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":5002,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650,""Engineering"":{""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_thermal_conduit"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":13.056,""OriginalValue"":7.68},{""Label"":""DistributorDraw"",""Value"":2.8485,""OriginalValue"":2.11},{""Label"":""ThermalLoad"",""Value"":4.1975,""OriginalValue"":3.65}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.6));
                Check(mod.DPS.Value.ApproxEqualsPercent(13.056));
                Check(mod.Damage.Value.ApproxEqualsPercent(13.056));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(2.849));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(4.198));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(18));
                Check(mod.Range.Value.ApproxEqualsPercent(3000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(600));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(10.37));
            }
            {
                // multicannon efficient incendiary rounds
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,HdhG03M_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34580,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1982,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_multicannon_gimbal_small"",""On"":true,""Priority"":0,""Value"":14250,""Engineering"":{""BlueprintName"":""Weapon_Efficient"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_incendiary_rounds"",""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.1924,""OriginalValue"":0.37},{""Label"":""DamagePerSecond"",""Value"":8.049667,""OriginalValue"":6.833333},{""Label"":""Damage"",""Value"":1.0168,""OriginalValue"":0.82},{""Label"":""DistributorDraw"",""Value"":0.0385,""OriginalValue"":0.07},{""Label"":""ThermalLoad"",""Value"":0.12,""OriginalValue"":0.1},{""Label"":""RateOfFire"",""Value"":7.916667,""OriginalValue"":8.333333}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.1924));
                Check(mod.DPS.Value.ApproxEqualsPercent(8.05));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.0168));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.0385));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.12));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(22));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(2000));
                Check(mod.Speed.Value.ApproxEqualsPercent(1600));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(7.917));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.12632));
                Check(mod.Clip.Value == 90);
                Check(mod.Ammo.Value == 2100);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(0.868));
            }

            {
                // multicannon overchanged corrosive
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,HdhG0BI_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":34580,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1982,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_multicannon_gimbal_small"",""On"":true,""Priority"":0,""Value"":14250,""Engineering"":{""BlueprintName"":""Weapon_Overcharged"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_corrosive_shell"",""Modifiers"":[{""Label"":""DamagePerSecond"",""Value"":11.616667,""OriginalValue"":6.833333},{""Label"":""Damage"",""Value"":1.394,""OriginalValue"":0.82},{""Label"":""DistributorDraw"",""Value"":0.0945,""OriginalValue"":0.07},{""Label"":""ThermalLoad"",""Value"":0.115,""OriginalValue"":0.1},{""Label"":""AmmoClipSize"",""Value"":77,""OriginalValue"":90},{""Label"":""AmmoMaximum"",""Value"":1680,""OriginalValue"":2100}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(40));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.37));
                Check(mod.DPS.Value.ApproxEqualsPercent(11.617));
                Check(mod.Damage.Value.ApproxEqualsPercent(1.394));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.0945));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(0.115));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(22));
                Check(mod.Range.Value.ApproxEqualsPercent(4000));
                Check(mod.Falloff.Value.ApproxEqualsPercent(2000));
                Check(mod.Speed.Value.ApproxEqualsPercent(1600));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(8.333));
                Check(mod.Clip.Value == 77);
                Check(mod.Ammo.Value == 1680);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(1.19));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(0.12));
            }




            {
                // missile, light weight, overload
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,K3BG05M_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":92930,""UnladenMass"":41.1,""CargoCapacity"":4,""MaxJumpRange"":8.501283,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4900,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_basicmissilerack_fixed_small"",""On"":true,""Priority"":0,""Value"":72600,""Engineering"":{""BlueprintName"":""Weapon_LightWeight"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_overload_munitions"",""Modifiers"":[{""Label"":""Mass"",""Value"":0.2,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":16,""OriginalValue"":40},{""Label"":""PowerDraw"",""Value"":0.36,""OriginalValue"":0.6},{""Label"":""DistributorDraw"",""Value"":0.156,""OriginalValue"":0.24}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(16));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.36));
                Check(mod.DPS.Value.ApproxEqualsPercent(13.333));
                Check(mod.Damage.Value.ApproxEqualsPercent(40));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.156));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(3.6));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(60));
                Check(mod.Speed.Value.ApproxEqualsPercent(625));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.3333));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(3));
                Check(mod.Clip.Value == 6);
                Check(mod.Ammo.Value == 6);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(12));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(16));
            }

            {
                // missile, light weight, stripped down, 50%
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,K3BG05PVG0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":92930,""UnladenMass"":41.215,""CargoCapacity"":4,""MaxJumpRange"":8.477903,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":4900,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_basicmissilerack_fixed_small"",""On"":true,""Priority"":0,""Value"":72600,""Engineering"":{""BlueprintName"":""Weapon_LightWeight"",""Level"":5,""Quality"":0.5,""ExperimentalEffect"":""special_weapon_lightweight"",""Modifiers"":[{""Label"":""Mass"",""Value"":0.315,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":16,""OriginalValue"":40},{""Label"":""PowerDraw"",""Value"":0.39,""OriginalValue"":0.6},{""Label"":""DistributorDraw"",""Value"":0.162,""OriginalValue"":0.24}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.315));
                Check(mod.Integrity.Value.ApproxEqualsPercent(16));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.39));
                Check(mod.DPS.Value.ApproxEqualsPercent(13.333));
                Check(mod.Damage.Value.ApproxEqualsPercent(40));
                Check(mod.DistributorDraw.Value.ApproxEqualsPercent(0.162));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(3.6));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(60));
                Check(mod.Speed.Value.ApproxEqualsPercent(625));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(0.3333));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(3));
                Check(mod.Clip.Value == 6);
                Check(mod.Ammo.Value == 6);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(12));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(16));
            }

            {
                // torpedo light weight penetrator
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Kp9G03L_W0FBR00,,9p300A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":31530,""UnladenMass"":41.1,""CargoCapacity"":4,""MaxJumpRange"":8.501283,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":1830,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_advancedtorppylon_fixed_small"",""On"":true,""Priority"":0,""Value"":11200,""Engineering"":{""BlueprintName"":""Weapon_LightWeight"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_deep_cut_payload"",""Modifiers"":[{""Label"":""Mass"",""Value"":0.2,""OriginalValue"":2},{""Label"":""Integrity"",""Value"":16,""OriginalValue"":40},{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.4}]}},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.SmallHardpoint1);

                Check(mod.Mass.Value.ApproxEqualsPercent(0.2));
                Check(mod.Integrity.Value.ApproxEqualsPercent(16));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.24));
                Check(mod.DPS.Value.ApproxEqualsPercent(120));
                Check(mod.Damage.Value.ApproxEqualsPercent(120));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(45));
                Check(mod.ArmourPiercing.Value.ApproxEqualsPercent(10000));
                Check(mod.Speed.Value.ApproxEqualsPercent(250));
                Check(mod.RateOfFire.Value.ApproxEqualsPercent(1));
                Check(mod.BurstInterval.Value.ApproxEqualsPercent(1));
                Check(mod.Clip.Value == 1);
                Check(mod.ReloadTime.Value.ApproxEqualsPercent(5));
                Check(mod.BreachDamage.Value.ApproxEqualsPercent(60));
            }
            {
                // lightweight alloy heavy duty deep plating
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Eht00FBR00,,9p3G05I_W0A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":94980,""UnladenMass"":42.9,""CargoCapacity"":4,""MaxJumpRange"":8.149506,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":5002,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0,""Engineering"":{""BlueprintName"":""Armour_HeavyDuty"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_armour_chunky"",""Modifiers"":[{""Label"":""DefenceModifierHealthMultiplier"",""Value"":156.608,""OriginalValue"":80},{""Label"":""KineticResistance"",""Value"":-17.42,""OriginalValue"":-20},{""Label"":""ThermicResistance"",""Value"":2.15,""OriginalValue"":0},{""Label"":""ExplosiveResistance"",""Value"":-36.99,""OriginalValue"":-40}]}},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.Armour);

                Check(mod.Mass.Value.ApproxEqualsPercent(0));
                Check(mod.HullStrengthBonus.Value.ApproxEqualsPercent(156.61));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(-17.42));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(2.15));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(-36.99));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(90));
            }

            {
                // reactive alloy kinetic resistance deep plating
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,Eht00FBR00,,9opG07I_W0A4Y00AKA00AZo00Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":234400,""UnladenMass"":46.9,""CargoCapacity"":4,""MaxJumpRange"":7.463231,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":11973,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_beamlaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":74650},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_reactive"",""On"":true,""Priority"":0,""Value"":139420,""Engineering"":{""BlueprintName"":""Armour_Kinetic"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_armour_chunky"",""Modifiers"":[{""Label"":""DefenceModifierHealthMultiplier"",""Value"":278,""OriginalValue"":250},{""Label"":""KineticResistance"",""Value"":53.65,""OriginalValue"":25},{""Label"":""ThermicResistance"",""Value"":-61.504,""OriginalValue"":-40},{""Label"":""ExplosiveResistance"",""Value"":7.712,""OriginalValue"":20}]}},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.Armour);

                Check(mod.Mass.Value.ApproxEqualsPercent(4));
                Check(mod.HullStrengthBonus.Value.ApproxEqualsPercent(278));
                Check(mod.KineticResistance.Value.ApproxEqualsPercent(53.65));
                Check(mod.ThermalResistance.Value.ApproxEqualsPercent(-61.5));
                Check(mod.ExplosiveResistance.Value.ApproxEqualsPercent(7.71));
                Check(mod.AXResistance.Value.ApproxEqualsPercent(90));
            }

            {
                // frame shift drive
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBR00FBR00,,9p300A4Y00AKAG07J_W0AZAG05J_W0Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":44.275,""CargoCapacity"":4,""MaxJumpRange"":27.812499,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""Engine_Reinforced"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_engine_haulage"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.125,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":96.6,""OriginalValue"":46},{""Label"":""EngineOptimalMass"",""Value"":52.8,""OriginalValue"":48},{""Label"":""EngineHeatRate"",""Value"":0.65,""OriginalValue"":1.3}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220,""Engineering"":{""BlueprintName"":""FSD_LongRange"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_fsd_heavy"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":50.048,""OriginalValue"":64},{""Label"":""PowerDraw"",""Value"":0.345,""OriginalValue"":0.3},{""Label"":""FSDOptimalMass"",""Value"":145.08,""OriginalValue"":90}]}},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.FrameShiftDrive);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(50.05));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.345));
                Check(mod.BootTime.Value.ApproxEqualsPercent(10));
                Check(mod.OptMass.Value.ApproxEqualsPercent(145.08));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(10));
                Check(mod.MaxFuelPerJump.Value.ApproxEqualsPercent(0.9));
            }
            {
                // frame shift drive
                string t = @"[{""header"":{""appName"":""EDSY"",""appVersion"":308189904,""appURL"":""https://edsy.org/#/L=H100000H4C0S00,FBR00FBR00,,9p300A4Y00AKAG07J_W0AZAG03L_W0Ans00B1U00BH600BWQ00,,7Og0003w00mpU0nG0-0nF0-""},""data"":{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.525,""CargoCapacity"":4,""MaxJumpRange"":20.176394,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980,""Engineering"":{""BlueprintName"":""Engine_Reinforced"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_engine_haulage"",""Modifiers"":[{""Label"":""Mass"",""Value"":3.125,""OriginalValue"":2.5},{""Label"":""Integrity"",""Value"":96.6,""OriginalValue"":46},{""Label"":""EngineOptimalMass"",""Value"":52.8,""OriginalValue"":48},{""Label"":""EngineHeatRate"",""Value"":0.65,""OriginalValue"":1.3}]}},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220,""Engineering"":{""BlueprintName"":""FSD_FastBoot"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_fsd_cooled"",""Modifiers"":[{""Label"":""Integrity"",""Value"":54.4,""OriginalValue"":64},{""Label"":""BootTime"",""Value"":2,""OriginalValue"":10},{""Label"":""FSDOptimalMass"",""Value"":103.5,""OriginalValue"":90},{""Label"":""FSDHeatRate"",""Value"":10.8,""OriginalValue"":10}]}},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}}]";
                var mod = GetModule(t, ShipSlots.Slot.FrameShiftDrive);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.5));
                Check(mod.Integrity.Value.ApproxEqualsPercent(54.4));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.3));
                Check(mod.BootTime.Value.ApproxEqualsPercent(2));
                Check(mod.OptMass.Value.ApproxEqualsPercent(103.5));
                Check(mod.ThermalLoad.Value.ApproxEqualsPercent(10.8));
                Check(mod.MaxFuelPerJump.Value.ApproxEqualsPercent(0.9));
            }
            {
                // life support
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":44.85,""CargoCapacity"":4,""MaxJumpRange"":17.036565,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Misc_Reinforced"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":3.25,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":128,""OriginalValue"":32}]}},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.LifeSupport);

                Check(mod.Mass.Value.ApproxEqualsPercent(3.25));
                Check(mod.Integrity.Value.ApproxEqualsPercent(128));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.32));
                Check(mod.BootTime.Value.ApproxEqualsPercent(1));
                Check(mod.Time.Value.ApproxEqualsPercent(300));
            }
            {
                // power dist engine focused stripped down
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":42.77,""CargoCapacity"":4,""MaxJumpRange"":17.848016,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_PriorityEngines"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_lightweight"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.17,""OriginalValue"":1.3},{""Label"":""WeaponsCapacity"",""Value"":8.5,""OriginalValue"":10},{""Label"":""WeaponsRecharge"",""Value"":1.14,""OriginalValue"":1.2},{""Label"":""EnginesCapacity"",""Value"":12.8,""OriginalValue"":8},{""Label"":""EnginesRecharge"",""Value"":0.576,""OriginalValue"":0.4},{""Label"":""SystemsCapacity"",""Value"":6.8,""OriginalValue"":8},{""Label"":""SystemsRecharge"",""Value"":0.34,""OriginalValue"":0.4}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.PowerDistributor);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.17));
                Check(mod.Integrity.Value.ApproxEqualsPercent(36));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.32));
                Check(mod.BootTime.Value.ApproxEqualsPercent(5));
                Check(mod.WeaponsCapacity.Value.ApproxEqualsPercent(8.5));
                Check(mod.WeaponsRechargeRate.Value.ApproxEqualsPercent(1.14));
                Check(mod.EngineCapacity.Value.ApproxEqualsPercent(12.8));
                Check(mod.EngineRechargeRate.Value.ApproxEqualsPercent(0.576));
                Check(mod.SystemsCapacity.Value.ApproxEqualsPercent(6.8));
                Check(mod.SystemsRechargeRate.Value.ApproxEqualsPercent(0.34));
            }

            {
                // power dist shielded double braced
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.095,""CargoCapacity"":4,""MaxJumpRange"":17.716169,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.PowerDistributor);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.495));
                Check(mod.Integrity.Value.ApproxEqualsPercent(124.2));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.224));
                Check(mod.BootTime.Value.ApproxEqualsPercent(5));
                Check(mod.WeaponsCapacity.Value.ApproxEqualsPercent(10));
                Check(mod.WeaponsRechargeRate.Value.ApproxEqualsPercent(1.2));
                Check(mod.EngineCapacity.Value.ApproxEqualsPercent(8));
                Check(mod.EngineRechargeRate.Value.ApproxEqualsPercent(0.4));
                Check(mod.SystemsCapacity.Value.ApproxEqualsPercent(8));
                Check(mod.SystemsRechargeRate.Value.ApproxEqualsPercent(0.4));
            }

            {
                // long range
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":44.395,""CargoCapacity"":4,""MaxJumpRange"":17.207702,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_LongRange"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""Mass"",""Value"":2.6,""OriginalValue"":1.3},{""Label"":""SensorTargetScanAngle"",""Value"":21,""OriginalValue"":30},{""Label"":""Range"",""Value"":7000,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Radar);

                Check(mod.Mass.Value.ApproxEqualsPercent(2.6));
                Check(mod.Integrity.Value.ApproxEqualsPercent(36));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.16));
                Check(mod.BootTime.Value.ApproxEqualsPercent(5));
                Check(mod.Range.Value.ApproxEqualsPercent(14000));
                Check(mod.Angle.Value.ApproxEqualsPercent(21));
                Check(mod.TypicalEmission.Value.ApproxEqualsPercent(7000));
            }

            {
                // wide angle
                string t = @"{""event"":""Loadout"",""Ship"":""sidewinder"",""ShipName"":"""",""ShipIdent"":"""",""HullValue"":5070,""ModulesValue"":185170,""UnladenMass"":43.095,""CargoCapacity"":4,""MaxJumpRange"":17.716169,""FuelCapacity"":{""Main"":2,""Reserve"":0.3},""Rebuy"":9512,""Modules"":[{""Slot"":""CargoHatch"",""Item"":""modularcargobaydoor"",""On"":true,""Priority"":0},{""Slot"":""SmallHardpoint1"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""SmallHardpoint2"",""Item"":""hpt_pulselaser_gimbal_small"",""On"":true,""Priority"":0,""Value"":6600},{""Slot"":""Armour"",""Item"":""sidewinder_armour_grade1"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""PowerPlant"",""Item"":""int_powerplant_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""MainEngines"",""Item"":""int_engine_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""FrameShiftDrive"",""Item"":""int_hyperdrive_size2_class5"",""On"":true,""Priority"":0,""Value"":160220},{""Slot"":""LifeSupport"",""Item"":""int_lifesupport_size1_class1"",""On"":true,""Priority"":0,""Value"":520},{""Slot"":""PowerDistributor"",""Item"":""int_powerdistributor_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""PowerDistributor_Shielded"",""Level"":5,""Quality"":1,""ExperimentalEffect"":""special_powerdistributor_toughened"",""Modifiers"":[{""Label"":""Mass"",""Value"":1.495,""OriginalValue"":1.3},{""Label"":""Integrity"",""Value"":124.2,""OriginalValue"":36},{""Label"":""PowerDraw"",""Value"":0.224,""OriginalValue"":0.32}]}},{""Slot"":""Radar"",""Item"":""int_sensors_size1_class1"",""On"":true,""Priority"":0,""Value"":520,""Engineering"":{""BlueprintName"":""Sensor_WideAngle"",""Level"":5,""Quality"":1,""Modifiers"":[{""Label"":""PowerDraw"",""Value"":0.24,""OriginalValue"":0.16},{""Label"":""SensorTargetScanAngle"",""Value"":90,""OriginalValue"":30},{""Label"":""Range"",""Value"":3200,""OriginalValue"":4000}]}},{""Slot"":""FuelTank"",""Item"":""int_fueltank_size1_class3"",""On"":true,""Priority"":0,""Value"":1000},{""Slot"":""Slot01_Size2"",""Item"":""int_shieldgenerator_size2_class1"",""On"":true,""Priority"":0,""Value"":1980},{""Slot"":""Slot02_Size2"",""Item"":""int_cargorack_size2_class1"",""On"":true,""Priority"":0,""Value"":3250},{""Slot"":""Slot05_Size1"",""Item"":""int_supercruiseassist"",""On"":true,""Priority"":0,""Value"":0},{""Slot"":""Slot06_Size1"",""Item"":""int_dockingcomputer_advanced"",""On"":true,""Priority"":0,""Value"":0}]}";
                var mod = GetModule(t, ShipSlots.Slot.Radar);

                Check(mod.Mass.Value.ApproxEqualsPercent(1.3));
                Check(mod.Integrity.Value.ApproxEqualsPercent(36));
                Check(mod.PowerDraw.Value.ApproxEqualsPercent(0.24));
                Check(mod.BootTime.Value.ApproxEqualsPercent(5));
                Check(mod.Range.Value.ApproxEqualsPercent(6400));
                Check(mod.Angle.Value.ApproxEqualsPercent(90));
                Check(mod.TypicalEmission.Value.ApproxEqualsPercent(3200));
            }


        }


        static Ship lastship;
        static ShipSlots.Slot lastslot;

        [System.Diagnostics.DebuggerHidden]
        public static ItemData.ShipModule GetModule(string loadout, ShipSlots.Slot slot, bool debugit = false)
        {
            Ship si = Ship.CreateFromLoadout(loadout);
            DebuggerHelpers.BreakAssert(si != null, "Bad ship");
            lastship = si;
            lastslot = slot;

            System.Diagnostics.Debug.WriteLine($"\r\nTEST Module {si.Modules[slot].ItemFD} in {lastslot} {si.Modules[slot]?.Engineering?.ToString()}");
            //System.Diagnostics.Debug.WriteLine($"\r\nTEST Module {si.Modules[slot].ItemFD} in {lastslot} ");

            var module = si.GetShipModulePropertiesEngineered(slot, debugit);
            DebuggerHelpers.BreakAssert(module != null, "Bad module");
            return module;
        }

        [System.Diagnostics.DebuggerHidden]
        public static void Check(bool good)
        {
            DebuggerHelpers.BreakAssert(good, () => { var module = lastship.Modules[lastslot]; return $"{lastslot} error : {module.Engineering.ToString()}"; });
        }

#endif
    }


}
