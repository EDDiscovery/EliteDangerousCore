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
    public class SuitInformation
    {
        public ulong ID { get; private set; }                // its Frontier SuitID   
        public string FDName { get; private set; }          // suit type
        public string Name_Localised { get; private set; }         // localised
        public long Price { get; private set; }             // may be 0, not known
        //public List<SuitLoadout> Loadouts { get; private set; }

        public SuitInformation(ulong id, string fdname, string locname, long price)
        {
            ID = id;FDName = fdname;Name_Localised = locname;Price = price;
        }
    }

    public class SuitInformationList
    {
        public Dictionary<ulong, SuitInformation> SuitList { get; private set; } = new Dictionary<ulong, SuitInformation>();

        public SuitInformationList()
        {
            SuitList = new Dictionary<ulong, SuitInformation>();
        }

        public SuitInformationList(SuitInformationList other)
        {
            SuitList = new Dictionary<ulong, SuitInformation>(other.SuitList);
        }

        public void Buy(ulong id, string fdname, string namelocalised, long price)
        {
            SuitList[id] = new SuitInformation(id, fdname, namelocalised, price);
        }

        public void Sell(ulong id)
        {
            SuitList.Remove(id);
        }
    }


}

