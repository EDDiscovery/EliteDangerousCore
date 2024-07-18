/*
 * Copyright 2016-2024 EDDiscovery development team
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
using BaseUtils;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        public static void Initialise()
        {
            CreateModules();

            TranslateModules();

            AddExtraShipInfo();

            // this ensures all is checked - use it against example.ex and check translator-ids.log
            //foreach (var m in GetShipModules(true, true, true, true, true)) { string s = $"{m.Key} = {m.Value.TranslatedModName} {m.Value.TranslatedModTypeString}";  }

            // for translator example.ex
            
            //foreach (ShipSlots.Slot x in Enum.GetValues(typeof(ShipSlots.Slot))) System.Diagnostics.Debug.WriteLine($".{x}: {ShipSlots.ToEnglish(x).AlwaysQuoteString()} @");

            //foreach ( StationDefinitions.StationServices x in Enum.GetValues(typeof(StationDefinitions.StationServices))) System.Diagnostics.Debug.WriteLine($".{x}: {StationDefinitions.ToEnglish(x).AlwaysQuoteString()} @");
        }
    }
}
