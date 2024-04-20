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
    public class FactionDefinitions
    {
        public enum State
        {
            Unknown,
            UnknownSpansh,
            None,
            Boom,
            Bust,
            CivilUnrest,
            CivilWar,
            Election,
            Expansion,
            Famine,
            Investment,
            Lockdown,
            Outbreak,
            Retreat,
            War,
            CivilLiberty,
            PirateAttack,
            Blight,
            Drought,
            InfrastructureFailure,
            NaturalDisaster,
            PublicHoliday,
            Terrorism,
            ColdWar,
            Colonisation,
            HistoricEvent,
            Revolution,
            TechnologicalLeap,
            TradeWar,
            Exploited,
        }

        public static State? ToEnum(string englishname)
        {
            if (englishname == null)
            {
                //System.Diagnostics.Debug.WriteLine($"**** No faction state");
                return null;
            }
            else if (Enum.TryParse(englishname, true, out State value)) // case insensitive
            {
                return value;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"**** Unknown faction state {englishname}");
                return State.Unknown;
            }
        }

        public static string ToEnglish(State? stat)
        {
            return stat != null ? stat.ToString().SplitCapsWordFull() : null;
        }

        public static string ToLocalisedLanguage(State? stat)
        {
            if (stat == null)
                return null;
            string id = "FactionStates." + stat.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(stat), id);
        }

        public static void IDSTx()
        {
            foreach (var name in Enum.GetNames(typeof(State)))
                System.Diagnostics.Debug.WriteLine($".{name}: \"{name.SplitCapsWordFull()}\" @");
        }
    }
}


