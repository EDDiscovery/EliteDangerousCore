/*
 * Copyright © 2023-2023 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License")"] = "you may not use this
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
    public class GovernmentDefinitions
    {
        public enum Government
        {
            Unknown,
            Anarchy,
            Communism,
            Confederacy,
            Cooperative,
            Corporate,
            Democracy,
            Dictatorship,
            Feudal,
            Imperial,
            None,
            Patronage,
            PrisonColony,
            Theocracy,
            Engineer,
            Carrier,
        }

        // maps the $government_id; to an enum
        public static Government ToEnum(string fdname)
        {
            fdname = fdname.ToLowerInvariant().Replace("$government_", "").Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out Government value))
                return value;
            else
                return Government.Unknown;
        }

        // from EDCD 
        // localisation can be provided via the Identifiers caching of $government

        public static Dictionary<string, string> Types = new Dictionary<string, string>()
        {
            ["$government_anarchy;"] = "Anarchy",
            ["$government_communism;"] = "Communism",
            ["$government_confederacy;"] = "Confederacy",
            ["$government_cooperative;"] = "Cooperative",
            ["$government_corporate;"] = "Corporate",
            ["$government_democracy;"] = "Democracy",
            ["$government_dictatorship;"] = "Dictatorship",
            ["$government_feudal;"] = "Feudal",
            ["$government_imperial;"] = "Imperial",
            ["$government_none;"] = "None",
            ["$government_patronage;"] = "Patronage",
            ["$government_prisoncolony;"] = "Prison Colony",
            ["$government_theocracy;"] = "Theocracy",
            ["$government_engineer;"] = "Engineer",
            ["$government_carrier;"] = "Private Ownership",

            ["$government_unknown;"] = "Unknown",      // addition to allow Unknown to be mapped
        };

        public static string ReverseLookup(string englishname)
        {
            foreach(var kvp in Types)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Debug.WriteLine($"*** Reverse lookup government failed {englishname}");
            return null;
        }
    }
}


