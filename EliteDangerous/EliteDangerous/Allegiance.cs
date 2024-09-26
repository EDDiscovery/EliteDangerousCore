/*
 * Copyright © 2023-2024 EDDiscovery development team
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
    public class AllegianceDefinitions
    {
        public enum Allegiance
        {
            Unknown = 0,
            Federation,
            Empire,
            Independent,
            Alliance,
            Guardian,
            Thargoid,
            PilotsFederation,
            Undefined,      // seen in logs
            PlayerPilots,   // seen in logs
        }

        // maps the allegiance fdname to an enum.  Spaces can be in the name ("Pilots Federation") to cope with Spansh
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Allegiance ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return Allegiance.Unknown;

            if (parselist.TryGetValue(fdname.ToLower(), out Allegiance value))
            {
                return value;
            }
            else if (parselist.TryGetValue(fdname.ToLower().Replace(" ", ""), out value))
            {
                return value;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Allegiance unknown {fdname}");
                return Allegiance.Unknown;
            }
        }
        public static string ToEnglish(Allegiance al)
        {
            return al.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(Allegiance al)
        {
            string id = "Allegiances." + al.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(al), id);
        }

        static Dictionary<string, Allegiance> parselist;
        static AllegianceDefinitions()
        {
            parselist = new Dictionary<string, Allegiance>();
            foreach (var v in Enum.GetValues(typeof(Allegiance)))
                parselist[v.ToString().ToLowerInvariant()] = (Allegiance)v;
        }

    }
}


