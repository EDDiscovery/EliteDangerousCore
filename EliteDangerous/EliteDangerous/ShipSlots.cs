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
            ShieldGenerator,
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
        }

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
                System.Diagnostics.Debug.WriteLine($"*** Slots unknown {fdname}");
                return Slot.Unknown;
            }
        }
        public static string ToEnglish(Slot al)
        {
            return al.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(Slot al)
        {
            string id = "ShipSlots." + al.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(al), id);
        }
    }
}


