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
          //  Imperial,  not seen
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
            if (!fdname.HasChars()) // null or empty
                return Government.Unknown;

            fdname = fdname.ToLowerInvariant().Replace("$government_", "").Replace(" ", "").Replace(";", "");

            if (Enum.TryParse(fdname, true, out Government value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"**** Unknown government {fdname}");
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

    }
}


