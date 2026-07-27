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
    // purposely not doing auto conversion to/from string so the use of FDName can be found easier

    [System.Diagnostics.DebuggerDisplay("FD {fdname}")]
    public abstract class FDName : IEquatable<FDName>, IComparable<FDName>, IEquatable
    {
        public FDName()
        {
            this.fdname = this.fdname_lower = "Unknown";
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public FDName(string fdname)
        {
            this.fdname = fdname.HasChars() ? fdname : "Unknown";
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public FDName(QuickJSON.JToken token)      // new July 26 QuickJson constructor
        {
            string txt = token.Str("Unknown");
            this.fdname = txt;
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public bool IsValid => !this.fdname.Equals("Unknown");

        public string Str()
        {
            return fdname;
        }
        public string WithQuotes()
        {
            return fdname.AlwaysQuoteString();
        }
        public string ToLower()
        {
            return fdname_lower;
        }
        public string SplitCapsWordFull()
        {
            return fdname.SplitCapsWordFull();
        }

        public QuickJSON.JToken ToJToken()      // new July26 converter for JTOKEN
        {
            return new JToken(fdname);
        }

        public override string ToString()
        {
            BaseUtils.Debugger.TraceBreak($"*** !!! YOUR USING FNAME Using ToString() {Environment.StackTrace}");
            return fdname;
        }

        #region Compare

        public static bool operator ==(FDName left, FDName right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(FDName left, FDName right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is FDName other ? other.fdname_lower.EqualsIIC(this.fdname_lower) : false;
        }

        public bool Equals(FDName right)        // other may be null
        {
            return !(right is null) && right.fdname_lower.EqualsIIC(this.fdname_lower);
        }

        public bool Equals(string right)        // other may be null
        {
            return right is null ? false : this.fdname_lower.EqualsIIC(right);
        }
        public int CompareTo(FDName other)
        {
            return fdname_lower.CompareTo(other.fdname_lower); 
        }

        public int GetHashCode(FDName obj)
        {
            return obj.hashcode;
        }
        public override int GetHashCode()
        {
            return hashcode;
        }
        public bool Contains(string partname)
        {
            return fdname_lower.ContainsIIC(partname);
        }
        public bool EndsWith(string partname)
        {
            return fdname_lower.EndsWithIIC(partname);
        }
        public bool StartsWith(string partname)
        {
            return fdname_lower.StartsWithIIC(partname);
        }

        #endregion

        #region vars
        private string fdname;
        private string fdname_lower;
        private int hashcode;
        #endregion
    }
}
