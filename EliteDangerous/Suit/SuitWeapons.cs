/*
 * Copyright © 2021 - 2021 EDDiscovery development team
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using BaseUtils;
using System;

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

        public SuitWeapon(DateTime time, ulong id, string fdname, string namelocalised, long price, int cls, bool sold)
        {
            EventTime = time; ID = id;FDName = fdname; Name_Localised = namelocalised; Price = price; Sold = sold; Class = cls;
            FriendlyName = ItemData.GetWeapon(fdname, Name_Localised)?.Name ?? Name_Localised;
        }
    }

    public class SuitWeaponList
    {
        public GenerationalDictionary<ulong, SuitWeapon> Weapons { get; private set; } = new GenerationalDictionary<ulong, SuitWeapon>();

        public SuitWeaponList()
        {
        }

        public void Buy(DateTime time, ulong id, string fdname, string namelocalised, long price, int cls)
        {
            Weapons[id] = new SuitWeapon(time, id, fdname, namelocalised, price, cls, false);
        }

        public void Sell(DateTime time, ulong id)
        {
            if (Weapons.ContainsKey(id))
            {
                var last = Weapons.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    Weapons[id] = new SuitWeapon(time, id, last.FDName, last.Name_Localised, last.Price, last.Class, true);               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Weapons sold a weapon already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Weapons sold a weapon not seen " + id);
        }

        public void Upgrade(DateTime time, ulong id, int cls)
        {
            if (Weapons.ContainsKey(id))
            {
                var last = Weapons.GetLast(id);
                if (last.Sold == false)       // if not sold
                {                   // new entry with the new class
                    Weapons[id] = new SuitWeapon(time, id, last.FDName, last.Name_Localised, last.Price, cls, false);
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
                Weapons.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                IWeaponInformation e = je as IWeaponInformation;
                e.WeaponInformation(this,whereami,system);

                if (Weapons.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Weapon Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Weapons.Generation);
                    Weapons.AbandonGeneration();
                }
                else
                {
               //     System.Diagnostics.Debug.WriteLine("{0} {1} Weapon List Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Weapons.Generation, Weapons.UpdatesAtThisGeneration);
                }
            }

            return Weapons.Generation;        // return the generation we are on.
        }

    }
}

//