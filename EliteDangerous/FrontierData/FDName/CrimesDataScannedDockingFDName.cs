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

    public class CrimesFDName : FDName
    {
        public CrimesFDName(): base()
        {
        }

        public CrimesFDName(string fdname) : base(fdname) 
        {
        }

        public CrimesFDName(QuickJSON.JToken token) : base(token)
        {
        }
    }

    //public class CrimesFDNameEqualityComparer : IEqualityComparer<CrimesFDName>
    //{
    //    public bool Equals(CrimesFDName left, CrimesFDName right)
    //    {
    //        return left.Equals(right);
    //    }

    //    public int GetHashCode(CrimesFDName obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}


    public class DockingFDName : FDName
    {
        public DockingFDName() : base()
        {
        }

        public DockingFDName(string fdname) : base(fdname)
        {
        }

        public DockingFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static DockingFDName Normalise(string fdname, out string engname)
        {
            engname = fdname.SplitCapsWordFull();
            return new DockingFDName(fdname);
        }
    }

    public class DataScannedFDName : FDName
    {
        public DataScannedFDName() : base()
        {
        }

        public DataScannedFDName(string fdname) : base(fdname)
        {
        }

        public DataScannedFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static DataScannedFDName Normalise(string fdname, out string engname)
        {
            if (fdname.Length >= 8 && fdname.StartsWithIIC("$Datascan_") && fdname.EndsWith(";", System.StringComparison.InvariantCultureIgnoreCase))
                fdname = fdname.Substring(10, fdname.Length - 1 - 10);        // remove decoration

            engname = fdname.SplitCapsWordFull();
            return new DataScannedFDName(fdname);
        }
    }


}
