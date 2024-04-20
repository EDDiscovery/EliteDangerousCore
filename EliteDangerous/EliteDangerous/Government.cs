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
            Prison,
            PrisonColony,
            Theocracy,
            Engineer,
            Carrier,
        }

        // maps the $government_id; to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Government ToEnum(string fdname)
        {
            if (fdname == null)
                return Government.Unknown;

            fdname = fdname.ToLowerInvariant().Replace("$government_", "").Replace(" ", "").Replace(";", "");

            if (Enum.TryParse(fdname, true, out Government value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"**** Unknown government {fdname}");
                return Government.Unknown;
            }
        }

        public static string ToEnglish(Government g)
        {
            return g.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(Government gv)
        {
            string id = "GovernmentTypes." + gv.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(gv), id);
        }

        // from EDCD 
        // localisation can be provided via the Identifiers caching of $government

        private static Dictionary<Government, string> Types = new Dictionary<Government, string>()
        {
            [Government.Anarchy] = "Anarchy",
            [Government.Communism] = "Communism",
            [Government.Confederacy] = "Confederacy",
            [Government.Cooperative] = "Cooperative",
            [Government.Corporate] = "Corporate",
            [Government.Democracy] = "Democracy",
            [Government.Dictatorship] = "Dictatorship",
            [Government.Feudal] = "Feudal",
            [Government.Imperial] = "Imperial",
            [Government.None] = "None",
            [Government.Patronage] = "Patronage",
            [Government.PrisonColony] = "Prison Colony",
            [Government.Prison] = "Prison",
            [Government.Theocracy] = "Theocracy",
            [Government.Engineer] = "Engineer",
            [Government.Carrier] = "Private Ownership",

            [Government.Unknown] = "Unknown",      // addition to allow Unknown to be mapped
        };

        public static Government SpashToEnum(string englishname)
        {
            foreach(var kvp in Types)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Debug.WriteLine($"*** Spansh Reverse lookup government failed {englishname}");
            return Government.Unknown;
        }
    }
}


