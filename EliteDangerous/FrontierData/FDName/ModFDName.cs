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

    public class ModFDName//: FDName
    {
        public ModFDName() //: base()
        {
        }

        public ModFDName(string fdname) //: base(fdname) 
        {
        }

        public ModFDName(QuickJSON.JToken token) //: base(token)
        {
        }

        public string Str()
        {
            return "";
        }
        public string StrNull()
        {
            return "";
        }

        public string SplitCapsWordFull()
        {
            return "";
        }
        public bool Contains(string partname)
        {
            return false;
        }

        public int CompareTo(ModFDName other)
        {
            return 0;
        }

        public ModFDName Clone()
        {
            return null;
        }

        public bool IsValid => true;


    }

    public class ModFDNameEqualityComparer : IEqualityComparer<ModFDName>
    {
        public bool Equals(ModFDName left, ModFDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(ModFDName obj)
        {
            return obj.GetHashCode();
        }
    }
}
