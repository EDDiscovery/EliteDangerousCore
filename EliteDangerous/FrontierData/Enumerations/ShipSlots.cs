/*
 * Copyright © 2024-2024 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class ShipSlots
    {
        public enum Slot
        {
            Unknown = 0,

            // In Order normally presented by Frontier

            HugeHardpoint1,     // MUST be first in list
            HugeHardpoint2,
            LargeHardpoint1,
            LargeHardpoint2,
            LargeHardpoint3,
            LargeHardpoint4,
            LargeMiningHardpoint1, // type 11 prospector sep 25

            MediumHardpoint1,
            MediumHardpoint2,
            MediumHardpoint3,
            MediumHardpoint4,
            MediumHardpoint5,
            MediumHardpoint6,
            MediumMiningHardpoint1, // type 11 prospector sep 25
            MediumMiningHardpoint2, // type 11 prospector sep 25

            SmallMiningHardpoint1, // type 11 prospector sep 25
            SmallHardpoint1,
            SmallHardpoint2,
            SmallHardpoint3,
            SmallHardpoint4,
            SmallHardpoint5,        //type8
            SmallHardpoint6,        //MUST BE LAST IN LIST type8

            Armour,                 // START OF CORE INTERNAL MUST BE FIRST.
            PowerPlant,             
            MainEngines,            
            FrameShiftDrive,
            LifeSupport,
            PowerDistributor,
            Radar,
            FuelTank,
            CargoHatch,             
            PlanetaryApproachSuite, // no priority
            LimpetController01,     // type 11 prospector sep 25 
            ColonisationSuite,      // trailblazers feb 25 (moduleinfo only)
            CodexScanner,           // Present in ModulesInfo only
            DataLinkScanner,        // Present in ModulesInfo only
            DiscoveryScanner,       // END OF CORE INTERNAL Present in ModulesInfo only  

            // utility mounts
            TinyHardpoint1, // MUST BE FIRST
            TinyHardpoint2,
            TinyHardpoint3,
            TinyHardpoint4,
            TinyHardpoint5,
            TinyHardpoint6,
            TinyHardpoint7, 
            TinyHardpoint8, // MUST BE LAST

            // military

            Military01,
            Military02,
            Military03,

            // Passengers

            Passenger01,
            Passenger02,
            Passenger03,

            // optional internal

            Cargo01,     // MUST BE FIRST   // Panther clipper, july 25
            Cargo02,        // Panther clipper, july 25
            Slot00_Size8,       
            Slot01_Size2,
            Slot01_Size3,
            Slot01_Size4,
            Slot01_Size5,
            Slot01_Size6,
            Slot01_Size7,
            Slot01_Size8,
            Slot02_Size2,
            Slot02_Size3,
            Slot02_Size4,
            Slot02_Size5,
            Slot02_Size6,
            Slot02_Size7,
            Slot02_Size8,
            Slot03_Size1,
            Slot03_Size2,
            Slot03_Size3,
            Slot03_Size4,
            Slot03_Size5,
            Slot03_Size6,
            Slot03_Size7,
            Slot04_Size1,
            Slot04_Size2,
            Slot04_Size3,
            Slot04_Size4,
            Slot04_Size5,
            Slot04_Size6,
            Slot05_Size1,
            Slot05_Size2,
            Slot05_Size3,
            Slot05_Size4,
            Slot05_Size5,
            Slot05_Size6,
            Slot06_Size1,
            Slot06_Size2,
            Slot06_Size3,
            Slot06_Size4,
            Slot06_Size5,
            Slot07_Size1,
            Slot07_Size2,
            Slot07_Size3,
            Slot07_Size4,
            Slot07_Size5,
            Slot08_Size1,
            Slot08_Size2,
            Slot08_Size3,
            Slot08_Size4,
            Slot09_Size1,
            Slot09_Size2,
            Slot09_Size3,
            Slot09_Size4,
            Slot10_Size1,
            Slot10_Size2,
            Slot10_Size3,
            Slot10_Size4,
            Slot11_Size1,
            Slot11_Size2,
            Slot11_Size3,
            Slot12_Size1,
            Slot13_Size1,
            Slot13_Size2,
            Slot14_Size1,
            Slot14_Size2,       
            FighterBay01,       // type 11 prospector sep 25 // MUST BE LAST (IF MORE ADJUST IsOptionalInternal)

            // Vanity
            Bobble01,    // MUST BE FIRST
            Bobble02,
            Bobble03,
            Bobble04,
            Bobble05,
            Bobble06,
            Bobble07,
            Bobble08,
            Bobble09,
            Bobble10,
            Decal1,
            Decal2,
            Decal3,
            EngineColour,
            Hologram, // type 11 prospector sep 25
            PaintJob,
            ShipCockpit,
            ShipID0,
            ShipID1,
            ShipKitBumper,
            ShipKitSpoiler,
            ShipKitTail,
            ShipKitWings,
            ShipName0,
            ShipName1,
            StringLights,
            VesselVoice,
            WeaponColour,       // MUST BE LAST
         
            // srv
            Turret, // MUST BE FIRST
            Turret2,        // reported by users
            SineWaveScanner,
            MiningRigDeployment,    // rhino sep 26
            Refinery, // rhino sep 26
            DepositScanner, // rhino sep 26
            BuggyCargoHatch, // MUST BE LAST

            // fighters 
            ShieldGenerator,     // MUST BE FIRST
            Federation_Fighter_Shield,
            GDN_Hybrid_Fighter_V1_Shield,
            GDN_Hybrid_Fighter_V2_Shield,
            GDN_Hybrid_Fighter_V3_Shield,
            BiologicalScanner,
            Independent_Fighter_Shield, // MUST BE LAST

        }

        public static bool IsHardpoint(ShipSlots.Slot slot) => slot >= Slot.HugeHardpoint1 && slot <= Slot.SmallHardpoint6;
        public static bool IsCoreInternal(ShipSlots.Slot slot) => slot >= Slot.Armour && slot <= Slot.DiscoveryScanner;
        public static bool IsUtility(ShipSlots.Slot slot) => slot >= Slot.TinyHardpoint1 && slot <= Slot.TinyHardpoint8;
        public static bool IsMilitary(ShipSlots.Slot slot) => slot >= Slot.Military01 && slot <= Slot.Military03;
        public static bool IsPassenger(ShipSlots.Slot slot) => slot >= Slot.Passenger01 && slot <= Slot.Passenger03;
        public static bool IsOptionalInternal(ShipSlots.Slot slot) => slot >= Slot.Cargo01 && slot <= Slot.FighterBay01;
        public static bool IsVanity(ShipSlots.Slot slot) => slot >= Slot.Bobble01 && slot <= Slot.WeaponColour;
        public static bool IsSRV(ShipSlots.Slot slot) => slot >= Slot.Turret && slot <= Slot.BuggyCargoHatch;
        public static bool IsFighter(ShipSlots.Slot slot) => slot >= Slot.ShieldGenerator && slot <= Slot.Independent_Fighter_Shield;

        // only modules with power draw have priority, so this is further qualified
        public static bool CanHavePriority(ShipSlots.Slot slot) => IsHardpoint(slot) || IsCoreInternal(slot) || 
                                                                  IsOptionalInternal(slot) || IsUtility(slot) || IsMilitary(slot);

        [System.Diagnostics.DebuggerDisplay("{Slot} {Size}")]
        public class SlotAndSize
        {
            public ShipSlots.Slot Slot { get; set; }
            public bool IsHardpoint => IsHardpoint(Slot);
            public bool IsUtility => IsUtility(Slot);
            public bool IsMilitary => IsMilitary(Slot);
            public bool IsPassenger => IsPassenger(Slot);
            public bool CanHavePriority => CanHavePriority(Slot);
            public int Size { get; set; }      // hardpoints 0 (tiny or N/A) 1 (small), 2 (med), 3 (large), 4 (huge) else size/class of slot
            public SlotAndSize(ShipSlots.Slot slot, int size) { Slot = slot; Size = size; }

            public override string ToString() => $"{Slot}: {Size}";
        }


        private static Dictionary<Slot, string> english = new Dictionary<Slot, string>
        {
            [Slot.Unknown] = "Unknown".PTx(),
            [Slot.Armour] = "Armour".PTx(),
            [Slot.Bobble01] = "Bobble Position 1".PTx(),
            [Slot.Bobble02] = "Bobble Position 2".PTx(),
            [Slot.Bobble03] = "Bobble Position 3".PTx(),
            [Slot.Bobble04] = "Bobble Position 4".PTx(),
            [Slot.Bobble05] = "Bobble Position 5".PTx(),
            [Slot.Bobble06] = "Bobble Position 6".PTx(),
            [Slot.Bobble07] = "Bobble Position 7".PTx(),
            [Slot.Bobble08] = "Bobble Position 8".PTx(),
            [Slot.Bobble09] = "Bobble Position 9".PTx(),
            [Slot.Bobble10] = "Bobble Position 10".PTx(),
            [Slot.CargoHatch] = "Cargo Hatch".PTx(),
            [Slot.CodexScanner] = "Codex Scanner".PTx(),
            [Slot.DataLinkScanner] = "Data Link Scanner".PTx(),
            [Slot.DiscoveryScanner] = "Discovery Scanner".PTx(),
            [Slot.Decal1] = "Decal Front".PTx(),
            [Slot.Decal2] = "Decal Right".PTx(),
            [Slot.Decal3] = "Decal Left".PTx(),
            [Slot.EngineColour] = "Engine Colour".PTx(),
            [Slot.Federation_Fighter_Shield] = "Federation Fighter Shield".PTx(),
            [Slot.FrameShiftDrive] = "Frame Shift Drive".PTx(),
            [Slot.FuelTank] = "Fuel Tank".PTx(),
            [Slot.GDN_Hybrid_Fighter_V1_Shield] = "GDN Hybrid Fighter V 1 Shield".PTx(),
            [Slot.GDN_Hybrid_Fighter_V2_Shield] = "GDN Hybrid Fighter V 2 Shield".PTx(),
            [Slot.GDN_Hybrid_Fighter_V3_Shield] = "GDN Hybrid Fighter V 3 Shield".PTx(),
            [Slot.HugeHardpoint1] = "Huge Hardpoint 1".PTx(),
            [Slot.HugeHardpoint2] = "Huge Hardpoint 2".PTx(),
            [Slot.Independent_Fighter_Shield] = "Independent Fighter Shield".PTx(),
            [Slot.LargeHardpoint1] = "Large Hardpoint 1".PTx(),
            [Slot.LargeHardpoint2] = "Large Hardpoint 2".PTx(),
            [Slot.LargeHardpoint3] = "Large Hardpoint 3".PTx(),
            [Slot.LargeHardpoint4] = "Large Hardpoint 4".PTx(),
            [Slot.LifeSupport] = "Life Support".PTx(),
            [Slot.MainEngines] = "Thrusters".PTx(),
            [Slot.MediumHardpoint1] = "Medium Hardpoint 1".PTx(),
            [Slot.MediumHardpoint2] = "Medium Hardpoint 2".PTx(),
            [Slot.MediumHardpoint3] = "Medium Hardpoint 3".PTx(),
            [Slot.MediumHardpoint4] = "Medium Hardpoint 4".PTx(),
            [Slot.MediumHardpoint5] = "Medium Hardpoint 5".PTx(),
            [Slot.MediumHardpoint6] = "Medium Hardpoint 6".PTx(),
            [Slot.Military01] = "Military Slot 1".PTx(),
            [Slot.Military02] = "Military Slot 2".PTx(),
            [Slot.Military03] = "Military Slot 3".PTx(),
            [Slot.PaintJob] = "Paint Job".PTx(),
            [Slot.PlanetaryApproachSuite] = "Planetary Approach Suite".PTx(),
            [Slot.PowerDistributor] = "Power Distributor".PTx(),
            [Slot.PowerPlant] = "Power Plant".PTx(),
            [Slot.Radar] = "Sensor".PTx(),
            [Slot.ShieldGenerator] = "Shield Generator".PTx(),
            [Slot.ShipCockpit] = "Ship Cockpit".PTx(),
            [Slot.ShipID0] = "Ship ID Right".PTx(),
            [Slot.ShipID1] = "Ship ID Left".PTx(),
            [Slot.ShipKitBumper] = "Ship Kit Bumper".PTx(),
            [Slot.ShipKitSpoiler] = "Ship Kit Spoiler".PTx(),
            [Slot.ShipKitTail] = "Ship Kit Tail".PTx(),
            [Slot.ShipKitWings] = "Ship Kit Wings".PTx(),
            [Slot.ShipName0] = "Nameplate Right".PTx(),
            [Slot.ShipName1] = "Nameplate Left".PTx(),
            [Slot.Slot00_Size8] = "Optional Slot 0 Class 8".PTx(),

            [Slot.Slot01_Size2] = "Optional Slot 1 Class 2".PTx(),
            [Slot.Slot01_Size3] = "Optional Slot 1 Class 3".PTx(),
            [Slot.Slot01_Size4] = "Optional Slot 1 Class 4".PTx(),
            [Slot.Slot01_Size5] = "Optional Slot 1 Class 5".PTx(),
            [Slot.Slot01_Size6] = "Optional Slot 1 Class 6".PTx(),
            [Slot.Slot01_Size7] = "Optional Slot 1 Class 7".PTx(),
            [Slot.Slot01_Size8] = "Optional Slot 1 Class 8".PTx(),

            [Slot.Slot02_Size2] = "Optional Slot 2 Class 2".PTx(),
            [Slot.Slot02_Size3] = "Optional Slot 2 Class 3".PTx(),
            [Slot.Slot02_Size4] = "Optional Slot 2 Class 4".PTx(),
            [Slot.Slot02_Size5] = "Optional Slot 2 Class 5".PTx(),
            [Slot.Slot02_Size6] = "Optional Slot 2 Class 6".PTx(),
            [Slot.Slot02_Size7] = "Optional Slot 2 Class 7".PTx(),
            [Slot.Slot02_Size8] = "Optional Slot 2 Class 8".PTx(),
            [Slot.Slot03_Size1] = "Optional Slot 3 Class 1".PTx(),

            [Slot.Slot03_Size2] = "Optional Slot 3 Class 2".PTx(),
            [Slot.Slot03_Size3] = "Optional Slot 3 Class 3".PTx(),
            [Slot.Slot03_Size4] = "Optional Slot 3 Class 4".PTx(),
            [Slot.Slot03_Size5] = "Optional Slot 3 Class 5".PTx(),
            [Slot.Slot03_Size6] = "Optional Slot 3 Class 6".PTx(),
            [Slot.Slot03_Size7] = "Optional Slot 3 Class 7".PTx(),

            [Slot.Slot04_Size1] = "Optional Slot 4 Class 1".PTx(),
            [Slot.Slot04_Size2] = "Optional Slot 4 Class 2".PTx(),
            [Slot.Slot04_Size3] = "Optional Slot 4 Class 3".PTx(),
            [Slot.Slot04_Size4] = "Optional Slot 4 Class 4".PTx(),
            [Slot.Slot04_Size5] = "Optional Slot 4 Class 5".PTx(),
            [Slot.Slot04_Size6] = "Optional Slot 4 Class 6".PTx(),

            [Slot.Slot05_Size1] = "Optional Slot 5 Class 1".PTx(),
            [Slot.Slot05_Size2] = "Optional Slot 5 Class 2".PTx(),
            [Slot.Slot05_Size3] = "Optional Slot 5 Class 3".PTx(),
            [Slot.Slot05_Size4] = "Optional Slot 5 Class 4".PTx(),
            [Slot.Slot05_Size5] = "Optional Slot 5 Class 5".PTx(),
            [Slot.Slot05_Size6] = "Optional Slot 5 Class 6".PTx(),

            [Slot.Slot06_Size1] = "Optional Slot 6 Class 1".PTx(),
            [Slot.Slot06_Size2] = "Optional Slot 6 Class 2".PTx(),
            [Slot.Slot06_Size3] = "Optional Slot 6 Class 3".PTx(),
            [Slot.Slot06_Size4] = "Optional Slot 6 Class 4".PTx(),
            [Slot.Slot06_Size5] = "Optional Slot 6 Class 5".PTx(),

            [Slot.Slot07_Size1] = "Optional Slot 7 Class 1".PTx(),
            [Slot.Slot07_Size2] = "Optional Slot 7 Class 2".PTx(),
            [Slot.Slot07_Size3] = "Optional Slot 7 Class 3".PTx(),
            [Slot.Slot07_Size4] = "Optional Slot 7 Class 4".PTx(),
            [Slot.Slot07_Size5] = "Optional Slot 7 Class 5".PTx(),

            [Slot.Slot08_Size1] = "Optional Slot 8 Class 1".PTx(),
            [Slot.Slot08_Size2] = "Optional Slot 8 Class 2".PTx(),
            [Slot.Slot08_Size3] = "Optional Slot 8 Class 3".PTx(),
            [Slot.Slot08_Size4] = "Optional Slot 8 Class 4".PTx(),

            [Slot.Slot09_Size1] = "Optional Slot 9 Class 1".PTx(),
            [Slot.Slot09_Size2] = "Optional Slot 9 Class 2".PTx(),
            [Slot.Slot09_Size3] = "Optional Slot 9 Class 3".PTx(),
            [Slot.Slot09_Size4] = "Optional Slot 9 Class 4".PTx(),

            [Slot.Slot10_Size1] = "Optional Slot 10 Class 1".PTx(),
            [Slot.Slot10_Size2] = "Optional Slot 10 Class 2".PTx(),
            [Slot.Slot10_Size3] = "Optional Slot 10 Class 3".PTx(),
            [Slot.Slot10_Size4] = "Optional Slot 10 Class 4".PTx(),

            [Slot.Slot11_Size1] = "Optional Slot 11 Class 1".PTx(),
            [Slot.Slot11_Size2] = "Optional Slot 11 Class 2".PTx(),
            [Slot.Slot11_Size3] = "Optional Slot 11 Class 3".PTx(),

            [Slot.Slot12_Size1] = "Optional Slot 12 Class 1".PTx(),

            [Slot.Slot13_Size1] = "Optional Slot 13 Class 1".PTx(),
            [Slot.Slot13_Size2] = "Optional Slot 13 Class 2".PTx(),

            [Slot.Slot14_Size1] = "Optional Slot 14 Class 1".PTx(),
            [Slot.Slot14_Size2] = "Optional Slot 14 Class 2".PTx(),

            [Slot.Passenger01] = "Passenger Slot 1".PTx(),
            [Slot.Passenger02] = "Passenger Slot 2".PTx(),
            [Slot.Passenger03] = "Passenger Slot 3".PTx(),

            [Slot.SmallHardpoint1] = "Small Hardpoint 1".PTx(),
            [Slot.SmallHardpoint2] = "Small Hardpoint 2".PTx(),
            [Slot.SmallHardpoint3] = "Small Hardpoint 3".PTx(),
            [Slot.SmallHardpoint4] = "Small Hardpoint 4".PTx(),
            [Slot.SmallHardpoint5] = "Small Hardpoint 5".PTx(),
            [Slot.SmallHardpoint6] = "Small Hardpoint 6".PTx(),

            [Slot.StringLights] = "String Lights".PTx(),

            [Slot.TinyHardpoint1] = "Utility Mount 1".PTx(),
            [Slot.TinyHardpoint2] = "Utility Mount 2".PTx(),
            [Slot.TinyHardpoint3] = "Utility Mount 3".PTx(),
            [Slot.TinyHardpoint4] = "Utility Mount 4".PTx(),
            [Slot.TinyHardpoint5] = "Utility Mount 5".PTx(),
            [Slot.TinyHardpoint6] = "Utility Mount 6".PTx(),
            [Slot.TinyHardpoint7] = "Utility Mount 7".PTx(),
            [Slot.TinyHardpoint8] = "Utility Mount 8".PTx(),

            [Slot.VesselVoice] = "Vessel Voice".PTx(),
            [Slot.WeaponColour] = "Weapon Colour".PTx(),
            [Slot.Turret] = "Turret".PTx(),
            [Slot.Turret2] = "Turret Type 2".PTx(),
            [Slot.SineWaveScanner] = "Sine Wave Scanner".PTx(),
            [Slot.BuggyCargoHatch] = "Cargo Hatch".PTx(),
            [Slot.ColonisationSuite] = "Colonisation Suite".PTx(),
            [Slot.Cargo01] = "Large Cargo 1".PTx(),
            [Slot.Cargo02] = "Large Cargo 2".PTx(),

            [Slot.MediumMiningHardpoint1] = "Medium Mining Hardpoint 1".PTx(),
            [Slot.MediumMiningHardpoint2] = "Medium Mining Hardpoint 2".PTx(),
            [Slot.SmallMiningHardpoint1] = "Small Mining Hardpoint 1".PTx(),
            [Slot.LargeMiningHardpoint1]= "Large Mining Hardpoint 1".PTx(),
            [Slot.LimpetController01] = "Limpet Controller 1".PTx(),
            [Slot.FighterBay01] = "Fighter Bay 1".PTx(),
            [Slot.Hologram] = "Hologram".PTx(),
            [Slot.BiologicalScanner] = "Biological Scanner".PTx(),
            [Slot.MiningRigDeployment] = "Mining Rig".PTx(),
            [Slot.Refinery] = "Refinery".PTx(),
            [Slot.DepositScanner] = "Deposit Scanner".PTx(),
        };

        // maps the slot fdname to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Slot ToEnum(string fdname)
        {
            if (!fdname.HasChars())
                return Slot.Unknown;

            if (parselist.TryGetValue(fdname.ToLowerInvariant().Trim(), out Slot value))
            {
                return value;
            }
            else
            {
                BaseUtils.Debugger.TraceBreak($"*** Ship slot unknown `{fdname}`");
                return Slot.Unknown;
            }
        }

        public static string ToEnglish(Slot al)
        {
            return english[al];
        }

        public static string ToLocalisedLanguage(Slot al)
        {
            return ToEnglish(al).Tx();
        }

        static Dictionary<string, Slot> parselist;
        static ShipSlots()
        {
            parselist = new Dictionary<string, Slot>();
            foreach (var v in Enum.GetValues(typeof(Slot)))
                parselist[v.ToString().ToLowerInvariant()] = (Slot)v;
        }



    }
}


