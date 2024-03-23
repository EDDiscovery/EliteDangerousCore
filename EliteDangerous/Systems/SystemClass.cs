/*
 * Copyright © 2015 - 2023 EDDiscovery development team
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

using System;
using System.Diagnostics;

namespace EliteDangerousCore
{
    // For when you need a minimal version and don't want to mess up the database. 
    // Useful for creation of test doubles
    public class SystemClassBase : ISystemBase
    {
        public const float XYZScalar = 128.0F;     // scaling between DB stored values and floats

        static public float IntToFloat(int pos) { return (float)pos / XYZScalar; }
        static public double IntToDouble(int pos) { return (double)pos / XYZScalar; }
        static public int DoubleToInt(double pos) { return (int)(pos * XYZScalar); }
        static public double IntToDoubleSq(int pos) { double p = (float)pos / XYZScalar; return p * p; }

        public string Name { get; set; }

        public int Xi { get; set; }
        public int Yi { get; set; }
        public int Zi { get; set; }

        public double X { get { return Xi >= int.MinValue ? (double)Xi / XYZScalar : double.NaN; } set { Xi = double.IsNaN(value) ? int.MinValue : (int)(value * XYZScalar); } }
        public double Y { get { return Xi >= int.MinValue ? (double)Yi / XYZScalar : double.NaN; } set { Yi = (int)(value * XYZScalar); } }
        public double Z { get { return Xi >= int.MinValue ? (double)Zi / XYZScalar : double.NaN; } set { Zi = (int)(value * XYZScalar); } }

        public bool HasCoordinate { get { return Xi != int.MinValue; } }

        public int GridID { get; set; }
        public long? SystemAddress { get; set; }
        public long? EDSMID { get; set; }

        public object Tag { get; set; }

        public SystemClassBase()
        {
            Xi = int.MinValue;
        }

        public SystemClassBase(string name, double vx, double vy, double vz)
        {
            Name = name;
            X = vx; Y = vy; Z = vz;
            GridID = EliteDangerousCore.DB.GridId.Id128(Xi, Zi);
        }

        public SystemClassBase(string name, int xi, int yi, int zi, int gridid = -1)
        {
            Name = name;
            Xi = xi; Yi = yi; Zi = zi;
            GridID = gridid == -1 ? EliteDangerousCore.DB.GridId.Id128(Xi,Zi) : gridid;
        }

        public SystemClassBase(ISystemBase sys)
        {
            this.Name = sys.Name;
            this.Xi = sys.Xi;
            this.Yi = sys.Yi;
            this.Zi = sys.Zi;
            this.GridID = sys.GridID;
            this.SystemAddress = sys.SystemAddress;
            this.EDSMID = sys.EDSMID;
        }

        public bool Equals(ISystemBase other)
        {
            return other != null &&
                   other.Name.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   (!this.HasCoordinate || !other.HasCoordinate ||
                    (Math.Abs(this.X - other.X) < 0.125 &&
                     Math.Abs(this.Y - other.Y) < 0.125 &&
                     Math.Abs(this.Z - other.Z) < 0.125));
        }

        public double Distance(ISystemBase s2)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
                return Math.Sqrt((X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z));
            else
                return -1;
        }

        public bool Distance(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double distsq = (X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z);
                return distsq >= min * min && distsq <= max * max;
            }
            else
                return false;
        }

        public double Distance(double ox, double oy, double oz)
        {
            if (HasCoordinate)
                return Math.Sqrt((X - ox) * (X - ox) + (Y - oy) * (Y - oy) + (Z - oz) * (Z - oz));
            else
                return -1;
        }

        public double DistanceSq(double x, double y, double z)
        {
            if (HasCoordinate)
                return (X - x) * (X - x) + (Y - y) * (Y - y) + (Z - z) * (Z - z);
            else
                return -1;
        }

        public bool Cuboid(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double xd = Math.Abs(X - s2.X);
                double yd = Math.Abs(Y - s2.Y);
                double zd = Math.Abs(Z - s2.Z);
                return xd >= min && xd <= max && yd >= min && yd <= max && zd >= min && zd <= max;
            }
            else
                return false;
        }
    }

    [DebuggerDisplay("System {Name} ({X,nq},{Y,nq},{Z,nq}) {SystemAddress} {MainStarType} {Source}")]
    public class SystemClass : SystemClassBase, ISystem
    {
        public SystemClass() : base()
        {
        }

        public SystemClass(ISystem sys) : base(sys)
        {
            this.Source = sys.Source;
            this.MainStarType = sys.MainStarType;
        }

        public SystemClass(string name) : base()
        {
            Name = name;
        }

        // with no co-ords
        public SystemClass(string name, long? systemaddress, SystemSource src = SystemSource.Synthesised) : base()
        {
            Name = name;
            SystemAddress = systemaddress;
            Source = src;
        }

        // with co-ords
        public SystemClass(string name, long? systemaddress, double vx, double vy, double vz, SystemSource src = SystemSource.Synthesised, EDStar starclass = EDStar.Unknown) : base(name, vx, vy, vz)
        {
            SystemAddress = systemaddress;
            Source = src;
            MainStarType = starclass;
        }

        // used by EDSMClass
        public SystemClass(string name, long edsmid, long? systemaddress, SystemSource src) : base()
        {
            Name = name;
            EDSMID = edsmid;
            SystemAddress = systemaddress;
            Source = src;
        }

        // used by EDSMClass
        public SystemClass(string name, long edsmid, long? systemaddress, double vx, double vy, double vz, SystemSource src) : base(name, vx, vy, vz)
        {
            EDSMID = edsmid;
            SystemAddress = systemaddress;
            Source = src;
        }

        // used by StoreDB
        public SystemClass(string name, int xi, int yi, int zi, long? sysaddress, long? edsmid, int gridid, EDStar startype, SystemSource src) : base(name, xi, yi, zi, gridid)
        {
            SystemAddress = sysaddress;
            EDSMID = edsmid;
            MainStarType = startype;
            Source = src;
        }

        // added oct 23 since edsm has faulty data
        public bool Triage()
        {
            if (Xi == 0 && Name == "Sol")
                return true;

            if (Math.Abs(Xi) < 3 * XYZScalar && Math.Abs(Yi) < 3 * XYZScalar && Math.Abs(Zi) < 3 * XYZScalar)
                return false;

            return true;
        }

        static public bool Triage(string name, double x, double y, double z)
        {
            if (x == 0 && name == "Sol")
                return true;

            if (Math.Abs(x) < 3 && Math.Abs(y) < 3 && Math.Abs(z) < 3)
                return false;

            return true;

        }

        public SystemSource Source { get; set; }        // default source is Sythesised
        public EDStar MainStarType { get; set; }

        public override string ToString()
        {
            if (SystemAddress != null && EDSMID != null)
                return string.Format($"{Name} @ {X:N3},{Y:N3},{Z:N3}: {EDSMID}, {SystemAddress}");
            else if (SystemAddress != null)
                return string.Format($"{Name} @ {X:N3},{Y:N3},{Z:N3}: {SystemAddress}");
            else
                return string.Format($"{Name} @ {X:N3},{Y:N3},{Z:N3}");
        }

    }
}
