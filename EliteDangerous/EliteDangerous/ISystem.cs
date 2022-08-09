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
    public enum EDGovernment        // synced with FDev IDs Aug 22
    {
        Unknown = 0,
        Anarchy = 1,
        Communism = 2,
        Confederacy = 3,
        Corporate = 4,
        Cooperative = 5,
        Democracy = 6,
        Dictatorship,
        Feudal,
        Imperial,
        None,
        Patronage,
        Prison,
        Prison_Colony,
        Theocracy,
        Engineer,
        Carrier,
    }

    public enum EDAllegiance        // synced with FDEV and logs aug 22
    {
        Unknown = 0,
        Alliance = 1,
        Empire = 3,
        Federation = 4,
        Independent = 5,
        Anarchy = 2,
        None = 6,
        PilotsFederation = 7,
        Pirate = 8,
        Guardian = 9,
    }

    public enum EDState // synced with FDev Ids aug 22
    {
        Unknown = 0,
        None = 1,
        Boom,
        Bust,
        CivilUnrest,
        CivilWar,
        Election,
        Expansion,
        Famine,
        Investment,
        Lockdown,
        Outbreak,
        Retreat,
        War,
        CivilLiberty,
        PirateAttack,
        Blight,
        Drought,
        InfrastructureFailure, 
        NaturalDisaster, 
        PublicHoliday, 
        Terrorism,
        ColdWar, 
        Colonisation, 
        HistoricEvent, 
        Revolution,
        TechnologicalLeap, 
        TradeWar, 
    }

    public enum EDSecurity  // in FDevID august 22
    {
        Unknown = 0,
        Low,
        Medium,
        High,
        Anarchy,
        Lawless,
    }

    public enum EDEconomy   // in FDevID order august 22
    {
        Unknown = 0,
        Agriculture = 1,
        Colony = 11,
        Extraction = 2,
        High_Tech = 3,
        Industrial = 4,
        Military = 5,
        None = 10,
        Refinery = 6,
        Service = 7,
        Terraforming = 8,
        Tourism = 9,
        Prison = 12,
        Damaged = 13,
        Rescue = 14,
        Repair = 15,
        Carrier = 16,
        Engineer = 17,
    }

    public enum SystemSource                // Who made the information?
    {
        Synthesised,
        FromEDSM,
        FromJournal,
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

        Tuple<string, long?> NameSystemAddress { get; }

        double Distance(ISystemBase other);
        double Distance(double x, double y, double z);
        double DistanceSq(double x, double y, double z);
        bool Distance(ISystemBase other, double min, double max);
        bool Cuboid(ISystemBase other, double min, double max);
    }

    public interface ISystemSystemInfo
    {
        string Faction { get; set; }
        long Population { get; set; }
        EDGovernment Government { get; set; }
        EDAllegiance Allegiance { get; set; }
        EDState State { get; set; }
        EDSecurity Security { get; set; }
        EDEconomy PrimaryEconomy { get; set; }
        string Power { get; set; }
        string PowerState { get; set; }
        int NeedsPermit { get; set; }
        bool HasSystemStateInfo { get; }
    }

    public interface ISystem : ISystemBase, ISystemSystemInfo
    {
        long EDSMID { get; set; }
        SystemSource Source { get; set; }        // Who made this entry, where did the info come from?

        string ToString();
        string ToStringVerbose();
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
