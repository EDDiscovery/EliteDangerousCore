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

    public class HandItemFDName : FDName
    {
        public HandItemFDName(): base()
        {
        }

        public HandItemFDName(string fdname) : base(fdname) 
        {
        }

        public HandItemFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static string Normalise(string fdname)
        {
            if (fdname.Length >= 8 && fdname.StartsWith("$") && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                return fdname.Substring(1, fdname.Length - 7); // 1 for '$' plus 6 for '_name;'

            string s = fdname;
            if (s.StartsWith("$int_"))
                s = s.Replace("$int_", "Int_");
            if (s.StartsWith("int_"))
                s = s.Replace("int_", "Int_");
            if (s.StartsWith("$hpt_"))
                s = s.Replace("$hpt_", "Hpt_");
            if (s.StartsWith("hpt_"))
                s = s.Replace("hpt_", "Hpt_");
            if (s.Contains("_armour_"))
                s = s.Replace("_armour_", "_Armour_");      // normalise to Armour upper cas.. its a bit over the place with case..
            if (s.EndsWith("_name;", StringComparison.InvariantCultureIgnoreCase))
            {
                //System.Diagnostics.Debug.WriteLine("Correct " + s);
                s = s.Substring(0, s.Length - 6);
            }
            if (s.StartsWith("$"))                          // seen instances of $python_armour..
                s = s.Substring(1);

            return s;
        }
    }

    public class HandItemFDNameEqualityComparer : IEqualityComparer<HandItemFDName>
    {
        public bool Equals(HandItemFDName left, HandItemFDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(HandItemFDName obj)
        {
            return obj.GetHashCode();
        }
    }
}
