/*
 * Copyright 2026-2026 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in coSmpliance with the License. You may obtain a copy of the License at
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
using System.Collections.Generic;

namespace EliteDangerousCore
{

    public class MissionFDName : FDName
    {
        public MissionFDName(): base()
        {
        }

        public MissionFDName(string fdname) : base(fdname) 
        {
        }

        public MissionFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work

        public static MissionFDName Normalise(string fdname, out string engname, JournalEntry ev, bool allownull = false)
        {
            if (fdname.HasChars())
            {
                engname = fdname.Replace("_name", "").SplitCapsWordFull();
                return new MissionFDName(fdname);
            }
            else
            {
                engname = "Missing Mission Name";
                if (ev?.EventTimeUTC > EliteReleaseDates.ComplainTime)
                    BaseUtils.Debugger.TraceBreak($"*** Missing Mission Name {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");
                return new MissionFDName(engname);
            }
        }


    }
}
