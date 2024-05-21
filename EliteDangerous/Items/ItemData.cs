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
        public interface IModuleInfo
        {
        };

        public static void Initialise()
        {
            CreateModules();

            //foreach (var m in GetShipModules())
            //{
            //    var text = m.Value.PropertiesAsText();
            //    System.Diagnostics.Debug.WriteLine(m.Key + " " + text);
            //}

            TranslateModules();

            // AddExtraShipInfo();

            Dictionary<ShipPropID, IModuleInfo>[] ships = GetSpaceships();
            foreach (var x in ships)
            {
                var fdid = ((ShipInfoString)x[ShipPropID.FDID]).Value;
                var manu = ((ShipInfoString)x[ShipPropID.Manu]).Value;
                System.Diagnostics.Debug.WriteLine($" [{fdid.AlwaysQuoteString()}] = {manu.AlwaysQuoteString()},");

            }


            Dictionary<string, string> Manu = new Dictionary<string, string>
            {
                ["Adder"] = "Zorgon Peterson",
                ["TypeX_3"] = "Lakon",
                ["TypeX"] = "Lakon",
                ["TypeX_2"] = "Lakon",
                ["Anaconda"] = "Faulcon DeLacy",
                ["Asp"] = "Lakon",
                ["Asp_Scout"] = "Lakon",
                ["BelugaLiner"] = "Saud Kruger",
                ["CobraMkIII"] = "Faulcon DeLacy",
                ["CobraMkIV"] = "Faulcon DeLacy",
                ["DiamondBackXL"] = "Lakon",
                ["DiamondBack"] = "Lakon",
                ["Dolphin"] = "Saud Kruger",
                ["Eagle"] = "Core Dynamics",
                ["Federation_Dropship_MkII"] = "Core Dynamics",
                ["Federation_Corvette"] = "Core Dynamics",
                ["Federation_Dropship"] = "Core Dynamics",
                ["Federation_Gunship"] = "Core Dynamics",
                ["FerDeLance"] = "Zorgon Peterson",
                ["Hauler"] = "Zorgon Peterson",
                ["Empire_Trader"] = "Gutamaya",
                ["Empire_Courier"] = "Gutamaya",
                ["Cutter"] = "Gutamaya",
                ["Empire_Eagle"] = "Gutamaya",
                ["Independant_Trader"] = "Lakon",
                ["Krait_MkII"] = "Faulcon DeLacy",
                ["Krait_Light"] = "Faulcon DeLacy",
                ["Mamba"] = "Zorgon Peterson",
                ["Orca"] = "Saud Kruger",
                ["Python"] = "Faulcon DeLacy",
                ["Python_NX"] = "Faulcon DeLacy",
                ["SideWinder"] = "Faulcon DeLacy",
                ["Type9_Military"] = "Lakon",
                ["Type6"] = "Lakon",
                ["Type7"] = "Lakon",
                ["Type9"] = "Lakon",
                ["Viper"] = "Faulcon DeLacy",
                ["Viper_MkIV"] = "Faulcon DeLacy",
                ["Vulture"] = "Core Dynamics",
            };

            foreach( var kvp in Manu)
            {
       //         spaceships[kvp.Key.ToLowerInvariant()].Add(ShipPropID.Manu, new ShipInfoString(kvp.Value));
            }
    }

    }
}
