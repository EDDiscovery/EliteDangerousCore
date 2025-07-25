/*
 * Copyright 2022-2024 EDDiscovery development team
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
        static public Weapon GetWeapon(string fdname, string locname = null)         // suit weapons
        {
            fdname = fdname.ToLowerInvariant();
            if (weapons.TryGetValue(fdname, out Weapon var))
                return var;
            else
            {
                System.Diagnostics.Debug.WriteLine("Unknown Weapon: {{ \"{0}\", new Weapon(\"{1}\",0.0) }},", fdname, locname ?? fdname.SplitCapsWordFull());
                return null;
            }
        }

        public class WeaponStats
        {
            public double Damage;
            public double RatePerSec;
            public int ClipSize;
            public int HopperSize;
            public int Range;
            public double HeadShotMultiplier;
            public double DPS { get { return Damage * RatePerSec; } }
            public WeaponStats(double damage, double rate, int clip, int hoppersize, int range, double hsm)
            { Damage = damage; RatePerSec = rate; ClipSize = clip; HopperSize = hoppersize; Range = range; HeadShotMultiplier = hsm; }
            public WeaponStats(WeaponStats other)
            { Damage = other.Damage; RatePerSec = other.RatePerSec; ClipSize = other.ClipSize; HopperSize = other.HopperSize; Range = other.Range; HeadShotMultiplier = other.HeadShotMultiplier; }

            // https://elite-dangerous.fandom.com/wiki/Category:Engineer_Upgrades_for_Pilot_Equipment
            // damage - no engineering
            // rate - no engineering
            // clip Weapon_ClipSize https://elite-dangerous.fandom.com/wiki/Magazine_Size 1.5x
            // hoppersize - applied at suit level
            // range Weapon_Range  https://elite-dangerous.fandom.com/wiki/Greater_Range 1.5x
            // headshot  Weapon_HeadshotDamage https://elite-dangerous.fandom.com/wiki/Headshot_Damage 1.5x
            // Applied at suit level Suit_IncreasedAmmoReserves https://elite-dangerous.fandom.com/wiki/Extra_Ammo_Capacity 1.5x

            public WeaponStats ApplyEngineering(string[] mods)
            {
                if (mods.Length > 0)
                {
                    WeaponStats newws = new WeaponStats(this);
                    foreach (var m in mods)
                    {
                        //System.Diagnostics.Debug.WriteLine($"Weapon mod {m}");
                        if (m.Equals("Weapon_ClipSize", StringComparison.InvariantCultureIgnoreCase))
                            newws.ClipSize = newws.ClipSize * 3 / 2;
                        else if (m.Equals("Weapon_Range", StringComparison.InvariantCultureIgnoreCase))
                            newws.Range = newws.Range * 3 / 2;
                        else if (m.Equals("Weapon_HeadshotDamage", StringComparison.InvariantCultureIgnoreCase))
                            newws.HeadShotMultiplier = newws.HeadShotMultiplier * 1.5;
                    }
                    return newws;
                }
                else
                    return this;
            }
        }
        public class Weapon 
        {
            public string Name;
            public bool Primary;
            public enum WeaponClass { Launcher, Carbine, LongRangeRifle, Rifle, ShotGun, Pistol }
            public enum WeaponDamageType { Thermal, Plasma, Kinetic, Explosive }
            public enum WeaponFireMode { Automatic, SemiAutomatic, Burst }
            public WeaponClass Class;
            public WeaponDamageType DamageType;
            public WeaponFireMode FireMode;
            public WeaponStats[] Stats;     // 5 classes,0 to 4

            public WeaponStats GetStats(int cls) // 1 to 5
            {
                if (cls >= 1 && cls <= 5)
                    return Stats[cls - 1];
                else
                    return null;
            }

            public Weapon(string name, bool primary, WeaponDamageType ty, WeaponClass ds, WeaponFireMode fr, WeaponStats[] values)
            {
                Name = name;
                Primary = primary;
                DamageType = ty;
                Class = ds;
                FireMode = fr;
                Stats = values;
            }
        }

        public static Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>   // DO NOT USE DIRECTLY - public is for checking only
        {
             { "wpn_m_assaultrifle_kinetic_fauto", new Weapon("Karma AR-50", true, Weapon.WeaponDamageType.Kinetic, Weapon.WeaponClass.LongRangeRifle, Weapon.WeaponFireMode.Automatic,new WeaponStats[] {
                    new WeaponStats(0.9,10,40,240,50,2.0),      // game wiki https://elite-dangerous.fandom.com/wiki/Karma_AR-50
                    new WeaponStats(1.2,10,40,240,50,2.0),      // game x1.33 
                    new WeaponStats(1.6,10,40,240,50,2.0),      // wiki x1.33
                    new WeaponStats(2.0,10,40,240,50,2.0),      // wiki x1.25
                    new WeaponStats(2.5,10,40,240,50,2.0),  }) },   // wiki x1.25

             { "wpn_m_submachinegun_kinetic_fauto", new Weapon("Karma C-44", true, Weapon.WeaponDamageType.Kinetic, Weapon.WeaponClass.Carbine, Weapon.WeaponFireMode.Automatic, new WeaponStats[] {
                 new WeaponStats(0.65,13.3,60,360,20,2.0),      // wiki https://elite-dangerous.fandom.com/wiki/Karma_C-44
                 new WeaponStats(0.85,13.3,60,360,20,2.0),      // wiki
                 new WeaponStats(1.1,13.3,60,360,20,2.0),       // game
                 new WeaponStats(1.5,13.3,60,360,20,2.0),       // wiki          
                 new WeaponStats(1.875,13.3,60,360,20,2.0),  }) },    // TBD Guess at same muliplier of 1.25

             { "wpn_s_pistol_kinetic_sauto", new Weapon("Karma P-15", false, Weapon.WeaponDamageType.Kinetic, Weapon.WeaponClass.Pistol, Weapon.WeaponFireMode.SemiAutomatic, new WeaponStats[] {
                 new WeaponStats(1.4,10,24,240,25,2.0),         // game https://elite-dangerous.fandom.com/wiki/Karma_P-15
                 new WeaponStats(1.8,10,24,240,25,2.0),         // game
                 new WeaponStats(2.4,10,24,240,25,2.0),         // game
                 new WeaponStats(2.7,10,24,240,25,2.0),         // guess
                 new WeaponStats(3,10,24,240,25,2.0),  }) },    // guess at x1.25

            { "wpn_m_launcher_rocket_sauto", new Weapon("Karma L-6", true, Weapon.WeaponDamageType.Explosive, Weapon.WeaponClass.Launcher, Weapon.WeaponFireMode.Automatic, new WeaponStats[] {
                new WeaponStats(40,1,2,8,300,1.0),              // game - wiki wrong on 1  https://elite-dangerous.fandom.com/wiki/Karma_L-6 
                new WeaponStats(52.4,1,2,8,300,1.0),            // game
                new WeaponStats(69.2,1,2,8,300,1.0),            // game
                new WeaponStats(90,1,2,8,300,1.0),              // wiki
                new WeaponStats(119.2,1,2,8,300,1.0), }) },     // wiki


             { "wpn_m_assaultrifle_laser_fauto", new Weapon("TK Aphelion", true, Weapon.WeaponDamageType.Thermal, Weapon.WeaponClass.Rifle, Weapon.WeaponFireMode.Automatic, new WeaponStats[] {
                 new WeaponStats(1.6,5.7,25,150,70,1.0),    // game
                 new WeaponStats(2,5.7,25,150,70,1.0),      // wiki https://elite-dangerous.fandom.com/wiki/TK_Aphelion
                 new WeaponStats(2.7,5.7,25,150,70,1.0),    // game
                 new WeaponStats(3.6,5.7,25,150,70,1.0),    // wiki
                 new WeaponStats(4.4,5.7,25,150,70,1.0), }) },    // wiki

             { "wpn_m_submachinegun_laser_fauto", new Weapon("TK Eclipse", true, Weapon.WeaponDamageType.Thermal, Weapon.WeaponClass.Carbine, Weapon.WeaponFireMode.Automatic, new WeaponStats[] {
                 new WeaponStats(0.9,10,40,280,25,1.0),     // game wiki has it at 0.85 https://elite-dangerous.fandom.com/wiki/TK_Eclipse
                 new WeaponStats(1.1,10,40,280,25,1.0),     // game
                 new WeaponStats(1.5,10,40,280,25,1.0),     // wiki
                 new WeaponStats(1.9,10,40,280,25,1.0),     // wiki
                 new WeaponStats(2.375,10,40,280,25,1.0),   }) }, // guess at x1.24

             { "wpn_s_pistol_laser_sauto", new Weapon("TK Zenith", false, Weapon.WeaponDamageType.Thermal, Weapon.WeaponClass.Pistol, Weapon.WeaponFireMode.Burst, new WeaponStats[] {
                 new WeaponStats(1.7,5.7,18,180,35,1.0),    // game, frontier data - note wiki is wrong https://elite-dangerous.fandom.com/wiki/TK_Zenith
                 new WeaponStats(2.2,5.7,18,180,35,1.0),    // game
                 new WeaponStats(2.9,5.7,18,180,35,1.0),    // game
                 new WeaponStats(3.6,5.7,18,180,35,1.0),    // guess x1.25
                 new WeaponStats(4.5,5.7,18,180,35,1.0),   }) }, // guess x1.25


            { "wpn_m_sniper_plasma_charged", new Weapon("Manticore Executioner", true, Weapon.WeaponDamageType.Plasma, Weapon.WeaponClass.LongRangeRifle, Weapon.WeaponFireMode.SemiAutomatic,new WeaponStats[] {
                new WeaponStats(15,0.8,3,30,100,2.0),       // game
                new WeaponStats(19.6,0.8,3,30,100,2.0),     // wiki https://elite-dangerous.fandom.com/wiki/Manticore_Executioner
                new WeaponStats(26,0.8,3,30,100,2.0),       // wiki
                new WeaponStats(34,0.8,3,30,100,2.0),       // wiki
                new WeaponStats(44.7,0.8,3,30,100,2.0), }) },   // wiki

            { "wpn_m_assaultrifle_plasma_fauto", new Weapon("Manticore Oppressor", true, Weapon.WeaponDamageType.Plasma, Weapon.WeaponClass.Rifle, Weapon.WeaponFireMode.Automatic, new WeaponStats[] {
                new WeaponStats(0.8,6.7,50,300,35,1.5),     // game
                new WeaponStats(1.0,6.7,50,300,35,1.5),     // game
                new WeaponStats(1.4,6.7,50,300,35,1.5),     // wiki https://elite-dangerous.fandom.com/wiki/Manticore_Oppressor
                new WeaponStats(1.8,6.7,50,300,35,1.5),     // wiki
                new WeaponStats(2.4,6.7,50,300,35,1.5),  }) },  // wiki

            { "wpn_m_shotgun_plasma_doublebarrel", new Weapon("Manticore Intimidator", true,  Weapon.WeaponDamageType.Plasma, Weapon.WeaponClass.ShotGun, Weapon.WeaponFireMode.SemiAutomatic,new WeaponStats[] {
                new WeaponStats(1.8,1.25,2,24,7,1.5),       // game https://elite-dangerous.fandom.com/wiki/Manticore_Intimidator does not match either frontier or in-game numbers
                new WeaponStats(2.3,1.25,2,24,7,1.5),       // game
                new WeaponStats(3.2,1.25,2,24,7,1.5),       // guess 
                new WeaponStats(4.14,1.25,2,24,7,1.5),      // guess 
                new WeaponStats(5.52,1.25,2,24,7,1.5), }) }, // guess

            { "wpn_s_pistol_plasma_charged", new Weapon("Manticore Tormentor", false, Weapon.WeaponDamageType.Plasma, Weapon.WeaponClass.Pistol, Weapon.WeaponFireMode.SemiAutomatic, new WeaponStats[] {
                new WeaponStats(7.5,1.7,6,72,15,2.0),       // game 
                new WeaponStats(9.8,1.7,6,72,15,2.0),       // wiki https://elite-dangerous.fandom.com/wiki/Manticore_Tormentor
                new WeaponStats(13,1.7,6,72,15,2.0),        // wiki
                new WeaponStats(17,1.7,6,72,15,2.0),        // wiki
                new WeaponStats(22.4,1.7,6,72,15,2.0),      // wiki
            }) },
        };

    }

}
