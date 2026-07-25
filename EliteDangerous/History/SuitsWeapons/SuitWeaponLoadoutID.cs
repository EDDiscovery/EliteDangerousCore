/*
 * Copyright 2026 - 2026 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using QuickJSON;
using System;

namespace EliteDangerousCore
{

    [System.Diagnostics.DebuggerDisplay("SID{ID}")]
    public class SuitID : IEquatable<SuitID>, IComparable<SuitID>, IEquatable
    {
        private ulong ID;

        public SuitID(JToken tk)
        {
            if (tk == null)
                ID = 0;
            else
                ID = tk.ULong();
        }

        public QuickJSON.JToken ToJToken()      // new July26 converter for JTOKEN
        {
            return new JToken(ID);
        }

        public SuitID(ulong mid)
        {
            ID = mid;
        }
        public SuitID()
        {
            ID = 0;
        }

        public override string ToString()       // null if not defined
        {
            if (ID == 0)
                return null;
            else
                return ID.ToStringInvariant();
        }

        public bool Equals(SuitID other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is SuitID other ? other.ID == this.ID : false;
        }

        public int CompareTo(SuitID other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(SuitID left, SuitID right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(SuitID left, SuitID right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool IsValid => ID != 0;
        public bool IsNotValid => ID == 0;
        public ulong Value => ID;
    }

    [System.Diagnostics.DebuggerDisplay("WID{ID}")]
    public class WeaponID : IEquatable<WeaponID>, IComparable<WeaponID>, IEquatable
    {
        private ulong ID;

        public WeaponID(JToken tk)
        {
            if (tk == null)
                ID = 0;
            else
                ID = tk.ULong();
        }

        public QuickJSON.JToken ToJToken()      // new July26 converter for JTOKEN
        {
            return new JToken(ID);
        }

        public WeaponID(ulong mid)
        {
            ID = mid;
        }
        public WeaponID()
        {
            ID = 0;
        }

        public override string ToString()       // null if not defined
        {
            if (ID == 0)
                return null;
            else
                return ID.ToStringInvariant();
        }

        public bool Equals(WeaponID other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is WeaponID other ? other.ID == this.ID : false;
        }

        public int CompareTo(WeaponID other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(WeaponID left, WeaponID right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(WeaponID left, WeaponID right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool IsValid => ID != 0;
        public bool IsNotValid => ID == 0;
        public ulong Value => ID;
    }

    [System.Diagnostics.DebuggerDisplay("LID{ID}")]
    public class LoadoutID : IEquatable<LoadoutID>, IComparable<LoadoutID>, IEquatable
    {
        private ulong ID;

        public LoadoutID(JToken tk)
        {
            if (tk == null)
                ID = 0;
            else
                ID = tk.ULong();
        }

        public QuickJSON.JToken ToJToken()      // new July26 converter for JTOKEN
        {
            return new JToken(ID);
        }

        public LoadoutID(ulong mid)
        {
            ID = mid;
        }
        public LoadoutID()
        {
            ID = 0;
        }

        public override string ToString()       // null if not defined
        {
            if (ID == 0)
                return null;
            else
                return ID.ToStringInvariant();
        }

        public bool Equals(LoadoutID other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is LoadoutID other ? other.ID == this.ID : false;
        }

        public int CompareTo(LoadoutID other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(LoadoutID left, LoadoutID right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(LoadoutID left, LoadoutID right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool IsValid => ID != 0;
        public bool IsNotValid => ID == 0;
        public ulong Value => ID;
    }


}
