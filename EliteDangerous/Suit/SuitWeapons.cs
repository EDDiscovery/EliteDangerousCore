/*
 * Copyright © 2016 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EliteDangerousCore.JournalEvents;
using BaseUtils.JSON;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}:{FDName}")]
    public class SuitWeapon
    {
        public ulong ID { get; private set; }              // its Frontier ID LoadoutID
        public string FDName { get; private set; }          // Loadout user name
        public string Name_Localised { get; private set; }
        public long Price { get; private set; }

        public SuitWeapon(ulong id, string fdname, string namelocalised, long price)
        {
            ID = id;FDName = fdname; Name_Localised = namelocalised; Price = price;
        }
    }

    public class SuitWeaponList
    {
        public Dictionary<ulong, SuitWeapon> WeaponList { get; private set; } = new Dictionary<ulong, SuitWeapon>();

        public SuitWeaponList()
        {
            WeaponList = new Dictionary<ulong, SuitWeapon>();
        }

        public SuitWeaponList(SuitWeaponList other)
        {
            WeaponList = new Dictionary<ulong, SuitWeapon>(other.WeaponList);
        }

        public void Buy(ulong id, string fdname, string namelocalised, long price)
        {
            WeaponList[id] = new SuitWeapon(id, fdname, namelocalised, price);
        }

        public void Sell(ulong id)
        {
            WeaponList.Remove(id);
        }
    }

}

//