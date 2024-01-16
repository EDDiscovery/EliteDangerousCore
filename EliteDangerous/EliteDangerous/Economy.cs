/*
 * Copyright © 2023-2023 EDDiscovery development team
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
    public class EconomyDefinitions
    {
        // from EDCD 
        // localisation can be provided via the Identifiers caching of $economy

        public enum Economy
        {
            Unknown,
            Agri,
            Colony,
            Extraction,
            High_Tech,
            Industrial,
            Military,
            None,
            Refinery,
            Service,
            Terraforming,
            Tourism,
            Prison,
            Damaged,
            Rescue,
            Repair,
            Carrier,
            Engineer,
        }

        // maps the $economy_id; to an enum
        public static Economy ToEnum(string fdname)
        {
            fdname = fdname.ToLowerInvariant().Replace("$economy_", "").Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out Economy value))
                return value;
            else
                return Economy.Unknown;
        }

        public static Dictionary<string, string> Types = new Dictionary<string, string>()
        {
            ["$economy_agri;"] = "Agriculture",
            ["$economy_colony;"] = "Colony",
            ["$economy_extraction;"] = "Extraction",
            ["$economy_hightech;"] = "High Tech",
            ["$economy_industrial;"] = "Industrial",
            ["$economy_military;"] = "Military",
            ["$economy_none;"] = "None",
            ["$economy_refinery;"] = "Refinery",
            ["$economy_service;"] = "Service",
            ["$economy_terraforming;"] = "Terraforming",
            ["$economy_tourism;"] = "Tourism",
            ["$economy_prison;"] = "Prison",
            ["$economy_damaged;"] = "Damaged",
            ["$economy_rescue;"] = "Rescue",
            ["$economy_repair;"] = "Repair",
            ["$economy_carrier;"] = "Private Enterprise",
            ["$economy_engineer;"] = "Engineering",

            ["$economy_unknown;"] = "Unknown",      // addition to allow Unknown to be mapped
        };

        public static string ReverseLookup(string englishname)
        {
            foreach(var kvp in Types)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }
            System.Diagnostics.Debug.WriteLine($"*** Reverse lookup economy failed {englishname}");
            return null;
        }
    }
}


