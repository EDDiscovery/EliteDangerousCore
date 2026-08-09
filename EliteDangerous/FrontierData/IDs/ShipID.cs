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
    public class ShipID : IEquatable<ShipID>, IComparable<ShipID>, IEquatable
    {
        private ulong ID;

        public ShipID(JToken tk)
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

        public ShipID(ulong mid)
        {
            ID = mid;
        }
        public ShipID()
        {
            ID = 0;
        }

        public override string ToString()      
        {
            return ID.ToStringInvariant();
        }

        public bool Equals(ShipID other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is ShipID other ? other.ID == this.ID : false;
        }

        public int CompareTo(ShipID other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(ShipID left, ShipID right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(ShipID left, ShipID right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool IsValid => ID != 0;
        public ulong Value => ID;
    }


}
