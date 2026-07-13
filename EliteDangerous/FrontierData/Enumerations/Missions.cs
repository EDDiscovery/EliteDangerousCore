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
    public class MissionDefinitions
    {
        public enum TargetType
        {
            Unknown = 0,
            NotGiven,       // in order below, must be here
            AIHumanoid,
            Basilisk,
            BossSkimmer,
            Citizen,
            CitizenHumanoid,
            Criminal,
            Cyclops,
            Deserter,
            DeserterASS,
            GuardHumanoid,
            Hydra,
            Industrial,
            Infected,
            Informant,
            Interceptor,
            Medusa,
            PassengerLiner,
            Pirate,
            PirateLord,
            Politician,
            PrisonConvict,
            Prisoner,
            ReligiousLeader,
            Scientist,
            Scout,
            Skimmer,
            Smuggler,
            TerroristLeader,
            Trader,
            VenerableGeneral,
        }

        // maps the allegiance fdname to an enum.  Spaces can be in the name ("Pilots Federation") to cope with Spansh
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static TargetType ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return TargetType.NotGiven;

            string fdm = fdname.Replace(" ", "").Replace("$MissionUtil_FactionTag_", "");

            if (Enum.TryParse(fdm, true, out TargetType value))
            {
                return value;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Mission TargetType unknown `{fdname}`");
                return TargetType.Unknown;
            }
        }
        public static string ToEnglish(TargetType al)
        {
            return al.ToString().SplitCapsWordFull();
        }
        public static string ToFD(TargetType al)
        {
            return "$MissionUtil_FactionTag_" + al.ToString();
        }

        public static string ToLocalisedLanguage(TargetType al)
        {
            return ToEnglish(al).Tx();
        }

        public static void OutputLocalised()
        {
            foreach (TargetType x in Enum.GetValues(typeof(TargetType)))
                ToLocalisedLanguage(x);
        }
    }

}


