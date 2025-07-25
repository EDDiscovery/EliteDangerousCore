﻿/*
 * Copyright 2021 - 2024 EDDiscovery development team
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

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}:{FDName}:{Name}")]
    public class SuitWeapon
    {
        public DateTime EventTime { get; private set; }
        public ulong ID { get; private set; }              // its Frontier ID LoadoutID
        public string FDName { get; private set; }          // Loadout user name
        public string Name_Localised { get; private set; }
        public string FriendlyName { get; private set; }
        public long Price { get; private set; }
        public bool Sold { get; private set; }
        public int Class { get; private set; }
        public string[] WeaponMods { get; private set; }

        public SuitWeapon(DateTime time, ulong id, string fdname, string namelocalised, long price, int cls, string[] weaponmods, bool sold)
        {
            EventTime = time; ID = id;FDName = fdname; Name_Localised = namelocalised; Price = price; Sold = sold; Class = cls; WeaponMods = weaponmods;
            FriendlyName = ItemData.GetWeapon(fdname, Name_Localised)?.Name ?? Name_Localised;
        }
    }

    public class SuitWeaponList
    {
        public Dictionary<ulong, SuitWeapon> Weapons(uint gen) { return weapons.Get(gen, x => x.Sold == false && x.FDName.HasChars()); }    // all valid unsold weapons with valid names. fdname=null special entry

        public SuitWeaponList()
        {
        }

        public void Buy(DateTime time, ulong id, string fdname, string namelocalised, long price, int cls, string[] weaponmods)
        {
            weapons[id] = new SuitWeapon(time, id, fdname, namelocalised, price, cls, weaponmods, false);
        }

        public bool VerifyPresence(DateTime time, ulong id, string fdname, string namelocalised, long price, int cls, string[] weaponmods)
        {
            var w = weapons.GetLast(id);

            if (w == null)
            {
                System.Diagnostics.Debug.WriteLine("Missing weapon {0} {1} {2}", id, fdname, namelocalised);
                weapons[id] = new SuitWeapon(time, id, fdname, namelocalised, price, cls, weaponmods, false);
                return false;
            }
            else 
            {
                // if differs in cls, or weapons mods is null but new one isnt, or both are set but different
                if ( w.Class != cls || (w.WeaponMods == null && weaponmods != null ) || (w.WeaponMods != null && weaponmods != null && !w.WeaponMods.SequenceEqual(weaponmods)))
                {
                    //System.Diagnostics.Debug.WriteLine("Update weapon info {0} {1} {2}", id, fdname, namelocalised);
                    weapons[id] = new SuitWeapon(time, id, fdname, namelocalised, w.Price, cls, weaponmods, false);
                    return false;
                }
            }

            return true;
        }

        public void Sell(DateTime time, ulong id)
        {
            if (weapons.ContainsKey(id))
            {
                var last = weapons.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    weapons[id] = new SuitWeapon(time, id, last.FDName, last.Name_Localised, last.Price, last.Class, last.WeaponMods, true);               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Weapons sold a weapon already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Weapons sold a weapon not seen " + id);
        }

        public void Upgrade(DateTime time, ulong id, int cls, string[] weaponmods)
        {
            if (weapons.ContainsKey(id))
            {
                var last = weapons.GetLast(id);
                if (last.Sold == false)       // if not sold
                {                   // new entry with the new class
                    weapons[id] = new SuitWeapon(time, id, last.FDName, last.Name_Localised, last.Price, cls, weaponmods, false);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Weapons upgrade but already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Weapons upgrade a weapon not seen " + id);
        }

        public uint Process(JournalEntry je, string whereami, ISystem system)
        {
            if (je is IWeaponInformation )
            {
                weapons.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                IWeaponInformation e = je as IWeaponInformation;
                e.WeaponInformation(this,whereami,system);

                if (weapons.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Weapon Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Weapons.Generation);
                    weapons.AbandonGeneration();
                }
                else
                {
               //     System.Diagnostics.Debug.WriteLine("{0} {1} Weapon List Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Weapons.Generation, Weapons.UpdatesAtThisGeneration);
                }
            }

            return weapons.Generation;        // return the generation we are on.
        }

        public GenerationalDictionary<ulong, SuitWeapon> weapons { get; private set; } = new GenerationalDictionary<ulong, SuitWeapon>();

    }
}

//