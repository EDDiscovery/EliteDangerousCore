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

using QuickJSON;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("FD {fdname}")]
    public class FDName : IComparable, IEqualityComparer<FDName>, IEquatable<FDName>
    {
        private string fdname;
        private string fdname_lower;
        private int hashcode;

        public FDName(string fdname)
        {
            this.fdname = fdname.HasChars()? fdname : "Unknown";
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public FDName Clone()
        {
            return new FDName(this.fdname);
        }

        public bool Valid => !this.fdname.Equals("Unknown");

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


        [System.Diagnostics.DebuggerHidden()]       
        public static explicit operator FDName(string fdname)
        {
            System.Diagnostics.Debug.Assert(false);
            System.Diagnostics.Debug.WriteLine("Implicit Convert");
            return new FDName(fdname);
        }


        public static explicit operator string(FDName fd)
        {
            return fd.fdname;
        }


        public void Normalise()
        {
            var norm = Normalise(fdname);
            fdname = norm.fdname;
            fdname_lower = norm.fdname_lower;
            hashcode = norm.hashcode;
        }
        public void NormaliseShip()
        {
            var norm = NormaliseShip(fdname);
            fdname = norm.fdname;
            fdname_lower = norm.fdname_lower;
            hashcode = norm.hashcode;
        }

        // instances in log on mining and mission entries of commodities in this form, back into fd form
        // also normalises HPT stuff
        public static FDName Normalise(string fdname)
        {
            if (fdname.Length >= 8 && fdname.StartsWith("$") && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                return new FDName(fdname.Substring(1, fdname.Length - 7)); // 1 for '$' plus 6 for '_name;'

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

            return new FDName(s);
        }

        public static FDName NormaliseShip(string fdname)
        {
            if (fdname.IsEmpty())
                return new FDName("No Ship Name Given");

            FDName i = ItemData.GetShipFDID(new FDName(fdname));

            if (i != null)
                return i;
            else
            {
                System.Diagnostics.Trace.WriteLine("*** Unknown FD ship ID:" + fdname);
                return new FDName(fdname);
            }
        }

        public static FDName Empty => new FDName("");

        public bool Contains(string partname)
        {
            return fdname_lower.ContainsIIC(partname.ToLowerInvariant());
        }

        public bool EndsWith(string partname)
        {
            return fdname_lower.EndsWithIIC(partname.ToLowerInvariant());
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

        public bool IsWeaponArmour()
        {
            return fdname_lower.StartsWith("hpt_", StringComparison.InvariantCultureIgnoreCase) ||
                                            fdname_lower.StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase) ||
                                            fdname_lower.Contains("_armour_", StringComparison.InvariantCultureIgnoreCase);
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

    public static class FDNameHelpers
    {
        public static FDName FDName(this JToken tk)
        {
            return new FDName(tk != null ? tk.Str() : null);
        }
        public static FDName FDNameNull(this JToken tk)
        {
            return tk != null ? new FDName(tk.Str()) : null;
        }
        public static FDName FDNameNormalise(this JToken tk)
        {
            return EliteDangerousCore.FDName.Normalise(tk != null ? tk.Str() : "Unknown");
        }
        public static FDName FDNameNormaliseNull(this JToken tk)
        {
            return tk != null ? EliteDangerousCore.FDName.Normalise(tk.Str()) : null;
        }
        public static FDName FDNameNormaliseShip(this JToken tk)
        {
            return EliteDangerousCore.FDName.NormaliseShip(tk != null ? tk.Str() : "Unknown");
        }
        public static FDName FDNameNormaliseShipNull(this JToken tk)
        {
            return tk != null ? EliteDangerousCore.FDName.NormaliseShip(tk.Str()) : null;
        }
    }

}


