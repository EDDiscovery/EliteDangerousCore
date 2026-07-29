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

    public class EngineeringRecipeFDName : FDName
    {
        public EngineeringRecipeFDName() : base()
        {
        }

        public EngineeringRecipeFDName(string fdname) : base(fdname)
        {
        }

        public EngineeringRecipeFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static EngineeringRecipeFDName Normalise(string fdname, out string engname, JournalEntry ev, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    engname = null;
                    return null;
                }
                else
                {
                    engname = "Unknown Recipe";

                    if (ev?.EventTimeUTC > EliteReleaseDates.Odyssey1)
                        BaseUtils.Debugger.TraceBreak($"*** Missing engineering recipe name {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                    return new EngineeringRecipeFDName("Unknown Recipe");
                }
            }
            else
            {
                var fd = new EngineeringRecipeFDName(fdname);

                var rp = Recipes.FindEngineering(fd);

                if (rp != null)
                {
                    engname = rp.Name;
                    //   System.Diagnostics.Debug.WriteLine($"known blueprint name {ev?.EventTimeUTC} {ev?.EventTypeStr} : {fdname}");
                }
                else
                {
                    if (ev?.EventTimeUTC > EliteReleaseDates.Odyssey1)
                        BaseUtils.Debugger.TraceBreak($"*** Unknown Engineering Recipe name `{fdname}` {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                    engname = fdname.SplitCapsWordFull();
                }
                return fd;
            }
        }
    }

    public class SynthesisRecipeFDName : FDName
    {
        public SynthesisRecipeFDName() : base()
        {
        }

        public SynthesisRecipeFDName(string fdname) : base(fdname)
        {
        }

        public SynthesisRecipeFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static SynthesisRecipeFDName Normalise(string fdname, out string engname, out Recipes.SynthesisRecipe.SynthesisLevel level, JournalEntry ev, bool allownull = false)
        {
            level = Recipes.SynthesisRecipe.SynthesisLevel.Basic;

            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    engname = null;
                    return null;
                }
                else
                {
                    engname = "Unknown Recipe";

                    if (ev?.EventTimeUTC > EliteReleaseDates.Odyssey1)
                        BaseUtils.Debugger.TraceBreak($"*** Missing synthesis recipe name {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                    return new SynthesisRecipeFDName("Unknown Recipe");
                }
            }
            else
            {
                var fd = new SynthesisRecipeFDName(fdname);

                var rp = Recipes.FindSynthesis(fd);

                if (rp != null)
                {
                    engname = rp.Name;
                    level = rp.Level;
                    //   System.Diagnostics.Debug.WriteLine($"known blueprint name {ev?.EventTimeUTC} {ev?.EventTypeStr} : {fdname}");
                }
                else
                {
                    if (ev?.EventTimeUTC > EliteReleaseDates.Odyssey1)
                        BaseUtils.Debugger.TraceBreak($"*** Unknown Synthesis Recipe name `{fdname}` {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                    engname = fdname.SplitCapsWordFull();
                }
                return fd;
            }
        }
    }


}
