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
using System.Linq;

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

            Undefined,      // Jugom logs
        }

        // maps the $economy_id; to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Economy ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return Economy.Unknown;

            fdname = fdname.ToLowerInvariant().Replace("$economy_", "").Replace(" ", "").Replace(";", "");

            if (Enum.TryParse(fdname, true, out Economy value))
                return value;
            else if (fdname == "hightech")
                return Economy.High_Tech;
            else if (fdname == "unknown_value")     // this has been found in a few 2017/2018 journal files
                return Economy.Unknown;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Economy is unknown {fdname}");
                return Economy.Unknown;
            }
        }

        public static string ToDecorated(Economy ec)
        {
            return "$economy_" + ec.ToString() + ";";
        }

        public static string ToEnglish(Economy ec)
        {
            return ec.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(Economy ec)
        {
            string id = "EconomyTypes." + ec.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(ec), id);
        }

        public static Economy[] ValidStates()
        {
            var list = (Economy[])Enum.GetValues(typeof(Economy));
            return list.Where(x => x != Economy.Unknown && x!=Economy.None && x!=Economy.Undefined).ToArray();
        }
    }
}


