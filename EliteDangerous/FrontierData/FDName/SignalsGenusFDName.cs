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

    public class SignalFDName : FDName
    {
        public SignalFDName() : base()
        {
        }

        public SignalFDName(string fdname) : base(fdname)
        {
        }

        public SignalFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static SignalFDName NormaliseSAAFSSSignals(string fdname, JournalEntry ev)
        {
            if (fdname.HasChars())
                return new SignalFDName(fdname.Replace("$SAA_SignalType_", "").Replace(";", "").SplitCapsWordFull());
            else
            {
                if (ev?.EventTimeUTC > EliteReleaseDates.ComplainTime)
                    BaseUtils.Debugger.TraceBreak($"*** Missing Signals {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                return new SignalFDName("Error in Signal no data");
            }
        }
        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work
    }
    public class GenusFDName : FDName
    {
        public GenusFDName() : base()
        {
        }

        public GenusFDName(string fdname) : base(fdname)
        {
        }

        public GenusFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static GenusFDName Normalise(string fdname, JournalEntry ev)
        {
            if (fdname.HasChars())
                return new GenusFDName(fdname.Replace("$Codex_Ent_", "").Replace("_Name;", "").Replace(";", "").Replace("$Codex_", "").SplitCapsWordFull());
            else
            {
                if (ev?.EventTimeUTC > EliteReleaseDates.ComplainTime)
                    BaseUtils.Debugger.TraceBreak($"*** Missing Genus {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                return new GenusFDName("Error in Genus no data");
            }
        }

        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work
    }

}
