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
    public class SuitWeaponsLoadout
    {
        public SuitWeaponList SuitWeapons { get; set; } = new SuitWeaponList();
        public SuitInformationList Suits { get; set; } = new SuitInformationList();

        public void Process(JournalEntry je, string whereami, ISystem system)
        {
            if (je is ISuitInformation)
            {
                ISuitInformation ji = je as ISuitInformation;

                ji.SuitInformation(this, whereami, system);
            }
        }

        public void BuyWeapon(ulong id, string fdname, string namelocalised, long price)
        {
            SuitWeapons = new SuitWeaponList(SuitWeapons);      // clone
            SuitWeapons.Buy(id, fdname, namelocalised, price);
        }

        public void SellWeapon(ulong id)
        {
            SuitWeapons = new SuitWeaponList(SuitWeapons);      // clone
            SuitWeapons.Sell(id);
        }

        public void BuySuit(ulong id, string fdname, string namelocalised, long price)
        {
            Suits = new SuitInformationList(Suits);      // clone
            Suits.Buy(id, fdname, namelocalised, price);
        }

        public void SellSuit(ulong id)
        {
            Suits = new SuitInformationList(Suits);      // clone
            Suits.Sell(id);
        }
    }
}



