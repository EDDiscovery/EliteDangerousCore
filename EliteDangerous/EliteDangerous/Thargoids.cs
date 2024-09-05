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

using QuickJSON;
using System;

namespace EliteDangerousCore
{
    public class ThargoidDefinitions
    {
        public class ThargoidWar
        {
            // CurrentState and NextState are the same state values that are listed in factions, "Lockdown", "Bust", etc.. 
            // new states related to thargoid systems are "Thargoid_Probing", "Thargoid_Harvest", "Thargoid_Controlled", "Thargoid_Stronghold", "Thargoid_Recovery"

            // Not going to Enum this due to states sometimes having "" as their values - not enough journal records to go on
            public string CurrentState { get; set; }
            public string NextStateSuccess { get; set; }
            public string NextStateFailure { get; set; }
            public bool SuccessStateReached { get; set; }
            public double WarProgress { get; set; }        // 0 to 1
            public int RemainingPorts { get; set; }
            public string EstimatedRemainingTime { get; set; }    // "0 days" !
            public DateTime UTC { get; set; }                       // EDD addition, UTC of record

            public static ThargoidWar ReadJSON(JObject evt, DateTime utc)
            {
                var tw = evt.ToObject<ThargoidWar>(false, false);

                if (tw == null)   // if read okay (following FactionDefinitions pattern)
                {
                    System.Diagnostics.Trace.WriteLine($"Bad Thargoid {evt["Factions"]}");
                }

                return tw;
            }

            // long form report into string builder
            public void ToString(System.Text.StringBuilder sb)
            {
                sb.AppendLine(BaseUtils.FieldBuilder.Build("Thargoid: ", CurrentState.SplitCapsWordFull(),
                                           "< +++ ", NextStateSuccess.SplitCapsWordFull(),
                                           "< --- ", NextStateFailure.SplitCapsWordFull(),
                                           ";Reached", SuccessStateReached,
                                           "Progress ", WarProgress,
                                           "Remaining Ports ", RemainingPorts,
                                           "Time remaining ", EstimatedRemainingTime));

            }
        }
    }
}


