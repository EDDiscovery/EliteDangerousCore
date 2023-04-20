/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{ 
    public enum SystemSource                // Who made the information?
    {
        Synthesised,
        FromDB,
        FromJournal,
        FromEDSM,
    }

    public interface ISystemBase : IEquatable<ISystemBase>
    {
        string Name { get; set; }
        double X { get; set; }
        int Xi { get; set; }
        double Y { get; set; }
        int Yi { get; set; }
        double Z { get; set; }
        int Zi { get; set; }
        bool HasCoordinate { get; }
        int GridID { get; set; }
        long? SystemAddress { get; set; }
        long? EDSMID { get; set; }      // if sourced from EDSM DB or web

        double Distance(ISystemBase other);
        double Distance(double x, double y, double z);
        double DistanceSq(double x, double y, double z);
        bool Distance(ISystemBase other, double min, double max);
        bool Cuboid(ISystemBase other, double min, double max);
    }

    public interface ISystem : ISystemBase
    {
        SystemSource Source { get; set; }        // Who made this entry, where did the info come from?
        EDStar MainStarType { get; set; }        // some DB hold main star type..  will be EDStar.Unknown if not known

        string ToString();
    }

    // useful to pass for isystem comparision of name only
    public class ISystemNameCompareCaseInsensitiveInvariantCulture : IEqualityComparer<ISystem>
    {
        public bool Equals(ISystem x, ISystem y)
        {
            return x.Name.Equals(y.Name, StringComparison.InvariantCulture);
        }

        public int GetHashCode(ISystem obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
