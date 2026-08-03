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
    [System.Diagnostics.DebuggerDisplay("SA{ID}")]
    public class SystemAddress : IEquatable<SystemAddress>, IComparable<SystemAddress>, IEquatable
    {
        private ulong ID;

        public SystemAddress(JToken tk)
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

        public SystemAddress(long mid)          // backwards compatible with DB reader
        {
            ID = (ulong)mid;
        }
        public SystemAddress(ulong mid)
        {
            ID = mid;
        }
        public SystemAddress(ulong? mid)
        {
            ID = mid.HasValue ? mid.Value : 0;
        }
        public SystemAddress()
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

        public bool Equals(SystemAddress other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is SystemAddress other ? other.ID == this.ID : false;
        }

        public int CompareTo(SystemAddress other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(SystemAddress left, SystemAddress right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(SystemAddress left, SystemAddress right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool IsValid => ID != 0;
        public ulong Value => ID;
    }


    /// <summary>
    /// Body and system address combined.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("BSA{ID}")]
    public class BodySystemAddress 
    {
        private ulong ID;

        public BodySystemAddress(SystemAddress addr, int bodyid)
        {
            ID = addr.Value | ((ulong)bodyid) << 55;
        }
        public bool IsValid => ID != 0;
        public bool IsNotValid => ID == 0;
        public ulong Value => ID;
        public override string ToString()       // null if not defined
        {
            if (ID == 0)
                return null;
            else
                return ID.ToStringInvariant();
        }
    }
}
