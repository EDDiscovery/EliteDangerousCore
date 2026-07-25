using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("MID{ID}")]
    public class MarketID : IEquatable<MarketID>, IComparable<MarketID>, IEquatable
    {
        private ulong ID;

        public MarketID(JToken tk)
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

        public MarketID(ulong mid)
        {
            ID = mid;
        }

        public static implicit operator MarketID(ulong v)
        {
            return new MarketID(v);
        }

        public override string ToString()
        {
            if (ID == 0)
                return null;
            else
                return ID.ToStringInvariant();
        }

        public bool Equals(MarketID other)
        {
            return other != null ? this.ID == other.ID : false;
        }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is MarketID other ? other.ID == this.ID: false;
        }

        public int CompareTo(MarketID other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(MarketID left, MarketID right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(MarketID left, MarketID right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public bool HasValue => ID != 0;
        public ulong Value => ID;
    }
}
