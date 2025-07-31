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

namespace EliteDangerousCore
{
    public class SecurityDefinitions
    {
        public enum Security
        {
            Unknown = 0,
            Low,
            Medium,
            High,
            Anarchy,
            Lawless,
        }


        // maps the security to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Security ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return Security.Unknown;

            fdname = fdname.ToLowerInvariant().Replace("$system_security_", "").Replace("$galaxy_map_info_state_", "").Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out Security value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Security is unknown `{fdname}`");
                return Security.Unknown;
            }
        }

        public static string ToEnglish(Security sec)
        {
            return sec.ToString();
        }

        public static string ToLocalisedLanguage(Security sc)
        {
            return ToEnglish(sc).Tx();
        }

    }
}


