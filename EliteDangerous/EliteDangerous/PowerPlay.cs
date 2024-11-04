/*
 * Copyright © 2024-2024 EDDiscovery development team
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
    public class PowerPlayDefinitions
    {
        public enum State
        {
            Unknown = 0,
            InPrepareRadius,
            Prepared,
            Exploited,
            Contested,
            Controlled,
            Turmoil,    // not seen in logs, but in manual
            HomeSystem,
            Fortified,  // new Ascendency 
            Stronghold,
        }


        // maps the security to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static State ToEnum(string fdname)
        {
            if (fdname == null)
                return State.Unknown;

            if (Enum.TryParse(fdname, true, out State value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Power Play state is unknown {fdname}");
                return State.Unknown;
            }
        }

        public static string ToEnglish(State sec)
        {
            return sec.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(State sc)
        {
            string id = "PowerPlayStates." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

    }
}


