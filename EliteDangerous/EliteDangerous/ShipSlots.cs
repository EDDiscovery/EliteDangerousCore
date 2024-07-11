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
            Armour,
            Bobble01,
            Bobble02,
            Bobble03,
            Bobble04,
            Bobble05,
            Bobble06,
            Bobble07,
            Bobble08,
            Bobble09,
            Bobble10,
            CargoHatch,
            CodexScanner,
            DataLinkScanner,
            DiscoveryScanner,
            Decal1,
            Decal2,
            Decal3,
            EngineColour,
            Federation_Fighter_Shield,
            FrameShiftDrive,
            FuelTank,
            GDN_Hybrid_Fighter_V1_Shield,
            GDN_Hybrid_Fighter_V2_Shield,
            GDN_Hybrid_Fighter_V3_Shield,
            HugeHardpoint1,
            HugeHardpoint2,
            Independent_Fighter_Shield,
            LargeHardpoint1,
            LargeHardpoint2,
            LargeHardpoint3,
            LargeHardpoint4,
            LifeSupport,
            MainEngines,
            MediumHardpoint1,
            MediumHardpoint2,
            MediumHardpoint3,
            MediumHardpoint4,
            MediumHardpoint5,
            Military01,
            Military02,
            Military03,
            PaintJob,
            PlanetaryApproachSuite,
            PowerDistributor,
            PowerPlant,
            Radar,
            ShieldGenerator,        // fighter
            ShipCockpit,
            ShipID0,
            ShipID1,
            ShipKitBumper,
            ShipKitSpoiler,
            ShipKitTail,
            ShipKitWings,
            ShipName0,
            ShipName1,
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
            Slot13_Size2,
            Slot14_Size1,
            SmallHardpoint1,
            SmallHardpoint2,
            SmallHardpoint3,
            SmallHardpoint4,
            StringLights,
            TinyHardpoint1,
            TinyHardpoint2,
            TinyHardpoint3,
            TinyHardpoint4,
            TinyHardpoint5,
            TinyHardpoint6,
            TinyHardpoint7,
            TinyHardpoint8,
            VesselVoice,
            WeaponColour,
            Turret,
            SineWaveScanner,
            BuggyCargoHatch,
        }

        private static Dictionary<Slot, string> english = new Dictionary<Slot, string>
        {
            [Slot.Unknown] = "Unknown",
            [Slot.Armour] = "Armour",
            [Slot.Bobble01] = "Bobble Position 1",
            [Slot.Bobble02] = "Bobble Position 2",
            [Slot.Bobble03] = "Bobble Position 3",
            [Slot.Bobble04] = "Bobble Position 4",
            [Slot.Bobble05] = "Bobble Position 5",
            [Slot.Bobble06] = "Bobble Position 6",
            [Slot.Bobble07] = "Bobble Position 7",
            [Slot.Bobble08] = "Bobble Position 8",
            [Slot.Bobble09] = "Bobble Position 9",
            [Slot.Bobble10] = "Bobble Position 10",
            [Slot.CargoHatch] = "Cargo Hatch",
            [Slot.CodexScanner] = "Codex Scanner",
            [Slot.DataLinkScanner] = "Data Link Scanner",
            [Slot.DiscoveryScanner] = "Discovery Scanner",
            [Slot.Decal1] = "Decal Front",
            [Slot.Decal2] = "Decal Right",
            [Slot.Decal3] = "Decal Left",
            [Slot.EngineColour] = "Engine Colour",
            [Slot.Federation_Fighter_Shield] = "Federation Fighter Shield",
            [Slot.FrameShiftDrive] = "Frame Shift Drive",
            [Slot.FuelTank] = "Fuel Tank",
            [Slot.GDN_Hybrid_Fighter_V1_Shield] = "GDN Hybrid Fighter V 1 Shield",
            [Slot.GDN_Hybrid_Fighter_V2_Shield] = "GDN Hybrid Fighter V 2 Shield",
            [Slot.GDN_Hybrid_Fighter_V3_Shield] = "GDN Hybrid Fighter V 3 Shield",
            [Slot.HugeHardpoint1] = "Huge Hardpoint 1",
            [Slot.HugeHardpoint2] = "Huge Hardpoint 2",
            [Slot.Independent_Fighter_Shield] = "Independent Fighter Shield",
            [Slot.LargeHardpoint1] = "Large Hardpoint 1",
            [Slot.LargeHardpoint2] = "Large Hardpoint 2",
            [Slot.LargeHardpoint3] = "Large Hardpoint 3",
            [Slot.LargeHardpoint4] = "Large Hardpoint 4",
            [Slot.LifeSupport] = "Life Support",
            [Slot.MainEngines] = "Thrusters",
            [Slot.MediumHardpoint1] = "Medium Hardpoint 1",
            [Slot.MediumHardpoint2] = "Medium Hardpoint 2",
            [Slot.MediumHardpoint3] = "Medium Hardpoint 3",
            [Slot.MediumHardpoint4] = "Medium Hardpoint 4",
            [Slot.MediumHardpoint5] = "Medium Hardpoint 5",
            [Slot.Military01] = "Military Slot 1",
            [Slot.Military02] = "Military Slot 2",
            [Slot.Military03] = "Military Slot 3",
            [Slot.PaintJob] = "Paint Job",
            [Slot.PlanetaryApproachSuite] = "Planetary Approach Suite",
            [Slot.PowerDistributor] = "Power Distributor",
            [Slot.PowerPlant] = "Power Plant",
            [Slot.Radar] = "Sensors",
            [Slot.ShieldGenerator] = "Shield Generator",
            [Slot.ShipCockpit] = "Ship Cockpit",
            [Slot.ShipID0] = "Ship ID Right",
            [Slot.ShipID1] = "Ship ID Left",
            [Slot.ShipKitBumper] = "Ship Kit Bumper",
            [Slot.ShipKitSpoiler] = "Ship Kit Spoiler",
            [Slot.ShipKitTail] = "Ship Kit Tail",
            [Slot.ShipKitWings] = "Ship Kit Wings",
            [Slot.ShipName0] = "Nameplate Right",
            [Slot.ShipName1] = "Nameplate Left",
            [Slot.Slot00_Size8] = "Optional Slot 0 Class 8",

            [Slot.Slot01_Size2] = "Optional Slot 1 Class 2",
            [Slot.Slot01_Size3] = "Optional Slot 1 Class 3",
            [Slot.Slot01_Size4] = "Optional Slot 1 Class 4",
            [Slot.Slot01_Size5] = "Optional Slot 1 Class 5",
            [Slot.Slot01_Size6] = "Optional Slot 1 Class 6",
            [Slot.Slot01_Size7] = "Optional Slot 1 Class 7",
            [Slot.Slot01_Size8] = "Optional Slot 1 Class 8",

            [Slot.Slot02_Size2] = "Optional Slot 2 Class 2",
            [Slot.Slot02_Size3] = "Optional Slot 2 Class 3",
            [Slot.Slot02_Size4] = "Optional Slot 2 Class 4",
            [Slot.Slot02_Size5] = "Optional Slot 2 Class 5",
            [Slot.Slot02_Size6] = "Optional Slot 2 Class 6",
            [Slot.Slot02_Size7] = "Optional Slot 2 Class 7",
            [Slot.Slot02_Size8] = "Optional Slot 2 Class 8",
            [Slot.Slot03_Size1] = "Optional Slot 3 Class 1",

            [Slot.Slot03_Size2] = "Optional Slot 3 Class 2",
            [Slot.Slot03_Size3] = "Optional Slot 3 Class 3",
            [Slot.Slot03_Size4] = "Optional Slot 3 Class 4",
            [Slot.Slot03_Size5] = "Optional Slot 3 Class 5",
            [Slot.Slot03_Size6] = "Optional Slot 3 Class 6",
            [Slot.Slot03_Size7] = "Optional Slot 3 Class 7",

            [Slot.Slot04_Size1] = "Optional Slot 4 Class 1",
            [Slot.Slot04_Size2] = "Optional Slot 4 Class 2",
            [Slot.Slot04_Size3] = "Optional Slot 4 Class 3",
            [Slot.Slot04_Size4] = "Optional Slot 4 Class 4",
            [Slot.Slot04_Size5] = "Optional Slot 4 Class 5",
            [Slot.Slot04_Size6] = "Optional Slot 4 Class 6",

            [Slot.Slot05_Size1] = "Optional Slot 5 Class 1",
            [Slot.Slot05_Size2] = "Optional Slot 5 Class 2",
            [Slot.Slot05_Size3] = "Optional Slot 5 Class 3",
            [Slot.Slot05_Size4] = "Optional Slot 5 Class 4",
            [Slot.Slot05_Size5] = "Optional Slot 5 Class 5",
            [Slot.Slot05_Size6] = "Optional Slot 5 Class 6",

            [Slot.Slot06_Size1] = "Optional Slot 6 Class 1",
            [Slot.Slot06_Size2] = "Optional Slot 6 Class 2",
            [Slot.Slot06_Size3] = "Optional Slot 6 Class 3",
            [Slot.Slot06_Size4] = "Optional Slot 6 Class 4",
            [Slot.Slot06_Size5] = "Optional Slot 6 Class 5",

            [Slot.Slot07_Size1] = "Optional Slot 7 Class 1",
            [Slot.Slot07_Size2] = "Optional Slot 7 Class 2",
            [Slot.Slot07_Size3] = "Optional Slot 7 Class 3",
            [Slot.Slot07_Size4] = "Optional Slot 7 Class 4",
            [Slot.Slot07_Size5] = "Optional Slot 7 Class 5",

            [Slot.Slot08_Size1] = "Optional Slot 8 Class 1",
            [Slot.Slot08_Size2] = "Optional Slot 8 Class 2",
            [Slot.Slot08_Size3] = "Optional Slot 8 Class 3",
            [Slot.Slot08_Size4] = "Optional Slot 8 Class 4",

            [Slot.Slot09_Size1] = "Optional Slot 9 Class 1",
            [Slot.Slot09_Size2] = "Optional Slot 9 Class 2",
            [Slot.Slot09_Size3] = "Optional Slot 9 Class 3",
            [Slot.Slot09_Size4] = "Optional Slot 9 Class 4",

            [Slot.Slot10_Size1] = "Optional Slot 10 Class 1",
            [Slot.Slot10_Size2] = "Optional Slot 10 Class 2",
            [Slot.Slot10_Size3] = "Optional Slot 10 Class 3",
            [Slot.Slot10_Size4] = "Optional Slot 10 Class 4",

            [Slot.Slot11_Size1] = "Optional Slot 11 Class 1",
            [Slot.Slot11_Size2] = "Optional Slot 11 Class 2",
            [Slot.Slot11_Size3] = "Optional Slot 11 Class 3",

            [Slot.Slot12_Size1] = "Optional Slot 12 Class 1",
            [Slot.Slot13_Size2] = "Optional Slot 13 Class 2",
            [Slot.Slot14_Size1] = "Optional Slot 14 Class 1",

            [Slot.SmallHardpoint1] = "Small Hardpoint 1",
            [Slot.SmallHardpoint2] = "Small Hardpoint 2",
            [Slot.SmallHardpoint3] = "Small Hardpoint 3",
            [Slot.SmallHardpoint4] = "Small Hardpoint 4",

            [Slot.StringLights] = "String Lights",

            [Slot.TinyHardpoint1] = "Utility Mount 1",
            [Slot.TinyHardpoint2] = "Utility Mount 2",
            [Slot.TinyHardpoint3] = "Utility Mount 3",
            [Slot.TinyHardpoint4] = "Utility Mount 4",
            [Slot.TinyHardpoint5] = "Utility Mount 5",
            [Slot.TinyHardpoint6] = "Utility Mount 6",
            [Slot.TinyHardpoint7] = "Utility Mount 7",
            [Slot.TinyHardpoint8] = "Utility Mount 8",

            [Slot.VesselVoice] = "Vessel Voice",
            [Slot.WeaponColour] = "Weapon Colour",
            [Slot.Turret] = "Turret",
            [Slot.SineWaveScanner] = "Sine Wave Scanner",
            [Slot.BuggyCargoHatch] = "Cargo Hatch",
        };

        // maps the slot fdname to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Slot ToEnum(string fdname)
        {
            if (fdname == null)
                return Slot.Unknown;

            if (Enum.TryParse(fdname, true, out Slot value))
            {
                return value;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Slots unknown {fdname}");
                return Slot.Unknown;
            }
        }
        public static string ToEnglish(Slot al)
        {
            return english[al];
        }

        public static string ToLocalisedLanguage(Slot al)
        {
            string id = "ShipSlots." + al.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(al), id);
        }
    }
}


