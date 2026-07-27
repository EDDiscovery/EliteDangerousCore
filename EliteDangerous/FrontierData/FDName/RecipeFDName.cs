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

    public class RecipeFDName : FDName
    {
        public RecipeFDName(): base()
        {
        }

        public RecipeFDName(string fdname) : base(fdname) 
        {
        }

        public RecipeFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static RecipeFDName Normalise(string fdname, out string engname, JournalEntry ev, bool allownull = false)
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
                        BaseUtils.Debugger.TraceBreak($"*** Missing blueprint {ev?.EventTimeUTC} {ev?.EventTypeStr}");

                    return new RecipeFDName("Unknown Recipe");
                }
            }
            else
            {
                var fd = new RecipeFDName(fdname);

                var rp = Recipes.FindRecipe(fd);

                if (rp != null)
                {
                    engname = rp.Name;
                    //   System.Diagnostics.Debug.WriteLine($"known blueprint name {ev?.EventTimeUTC} {ev?.EventTypeStr} : {fdname}");
                }
                else
                {
                    if (ev?.EventTimeUTC > EliteReleaseDates.Odyssey1)
                        BaseUtils.Debugger.TraceBreak($"*** Unknown Recipe name `{fdname}` {ev?.EventTimeUTC} {ev?.EventTypeStr}");

                    engname = fdname.SplitCapsWordFull();
                }
                return fd;
            }
        }
    }

    //public class RecipeFDNameEqualityComparer : IEqualityComparer<RecipeFDName>
    //{
    //    public bool Equals(RecipeFDName left, RecipeFDName right)
    //    {
    //        return left.Equals(right);
    //    }

    //    public int GetHashCode(RecipeFDName obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}
}
