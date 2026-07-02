/*
 * Copyright 2026-2026 EDDiscovery development team
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
using System.Collections;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("FD {fdname}")]
    public class FDName : IComparable, IEqualityComparer<FDName>
    {
        private string fdname;
        private string fdname_lower;
        private int hashcode;

        public FDName(string fdname)
        {
            this.fdname = fdname ?? "Unknown";
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public static implicit operator FDName(string fdname)
        {
            System.Diagnostics.Debug.WriteLine("Implicit Convert");
            return new FDName(fdname);
        }

        public static implicit operator string(FDName fd)
        {
            return fd.fdname;
        }

        public string Str()
        {
            return fdname;
        }

        public string ToLower()
        {
            return fdname_lower;
        }

        public string SplitCapsWordFull()
        {
            return fdname.SplitCapsWordFull();
        }

        public bool Contains(string partname)
        {
            return fdname_lower.ContainsIIC(partname.ToLowerInvariant());
        }

        public bool EndsWith(string partname)
        {
            return fdname_lower.EndsWithIIC(partname.ToLowerInvariant());
        }
        public string WithQuotes()
        {
            return fdname.AlwaysQuoteString();
        }

        public int CompareTo(object obj)
        {
            return obj is FDName sfd ? sfd.fdname_lower.CompareTo(this.fdname_lower) : 0;
        }

        public bool Equals(FDName other)
        {
            return other.fdname_lower.EqualsIIC(this.fdname_lower);
        }

        public bool Equals(FDName x, FDName y)
        {
            return x.Equals(y);
        }
        public int GetHashCode(FDName obj)
        {
            return obj.hashcode;
        }
        public override int GetHashCode()
        {
            return hashcode;
        }
        public int GetClass()
        {
            int ci = fdname_lower.IndexOf("class");
            int classn = ci > 0 ? fdname_lower.Substring(ci + 5, 1).InvariantParseInt(0) : 0;
            return classn;
        }

    }

    public class FDNameEqualityComparer : IEqualityComparer<FDName>
    {
        public bool Equals(FDName x, FDName y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(FDName obj)
        {
            return obj.GetHashCode();
        }
    }

}


