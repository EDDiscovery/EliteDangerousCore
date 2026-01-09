/*
 * Copyright 2017-2019 EDDiscovery development team
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
 */

using System;
using System.Collections.Generic;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore
{
    public interface IMaterialJournalEntry
    {
        void UpdateMaterials(MaterialCommoditiesMicroResourceList mc);
    }

    public interface ICommodityJournalEntry
    {
        void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool insrv);
    }

    public interface IMicroResourceJournalEntry
    {
        void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry prev);
    }

    public interface ILedgerJournalEntry
    {
        void Ledger(Ledger mcl);
    }
    public interface IShipInformation
    {
        void ShipInformation(ShipList shp, string whereami, ISystem system);
    }

    public interface IShipNaming
    {
        string ShipFD { get;}
        string ShipType { get;  }       // english friendly name
        ulong ShipId { get;  }
    }

    public interface IStarScan
    {
        void AddStarScan(StarScan2.StarScan s, ISystem system);
    }

    // events containing feature information on a body
    public interface IBodyFeature  
    {
        DateTime EventTimeUTC { get; }
        JournalTypeEnum EventTypeID { get; }
        string EventTypeStr { get; }
        string SummaryName(ISystem sys);
        string BodyName { get; }
        int? BodyID { get; }
        BodyDefinitions.BodyType BodyType { get; }
        string StarSystem { get; }
        long? SystemAddress { get; }
        double? Latitude { get; set; }
        double? Longitude { get; set; }
        bool HasLatLong { get; }
        string Name { get; }                // name of installation/feature on body, docked name of station
        string Name_Localised { get; }      // name of installation/feature on body, docked name of station
        long? MarketID { get; }             // for docked on station
        StationDefinitions.StarportTypes FDStationType { get; } // valid on docked
        string StationFaction { get; }             // for FSDJump and Docked.
    }

    // allows commonality of information between Location (when docked) and Docked events
    public interface ILocDocked
    {
        bool Docked { get; }
        string StarSystem { get; }
        long? SystemAddress { get; }
        string StationName { get; }
        string StationName_Localised { get; }
        StationDefinitions.StarportTypes FDStationType { get; }  // only on later events, else Unknown
        string StationType { get; } // english, only on later events, else Unknown
        long? MarketID { get; }
        StationDefinitions.Classification MarketClass();
        string StationFaction { get; }
        FactionDefinitions.State StationFactionState { get; }       //may be null, FDName
        string StationFactionStateTranslated { get; }
        GovernmentDefinitions.Government StationGovernment { get; }
        string StationGovernment_Localised { get; }
        AllegianceDefinitions.Allegiance StationAllegiance { get; }   // fdname
        StationDefinitions.StationServices[] StationServices { get; }   // may be null
        EconomyDefinitions.Economies[] StationEconomyList { get; }        // may be null
    }

    public interface ITaxiDropship
    {
        string DestinationSystem { get;  }
        string DestinationLocation { get; }
        string DestinationLocation_Localised { get; }       // no evidence
    }

    public interface IMissions
    {
        void UpdateMissions(MissionListAccumulator mlist, ISystem sys, string body);
    }

    public interface IAdditionalFiles
    {
        void ReadAdditionalFiles(string directory); 
    }
    public interface IIdentifiers
    {
        void UpdateIdentifiers();
    }

    public interface IJournalJumpColor
    {
        int MapColor { get; set; }
        System.Drawing.Color MapColorARGB { get; }
    }

    public interface IStatsJournalEntry
    {
        void UpdateStats(Stats stats, ISystem system, string stationfaction);
    }

    public class IStatsItemsInfo
    {
        public string FDName;
        public int Count;        // neg sold
        public long Profit;         // any profit
    }

    public interface IStatsJournalEntryMatCommod : IStatsJournalEntry
    {
        List<IStatsItemsInfo> ItemsList { get; }     
    }

    public interface IStatsJournalEntryBountyOrBond : IStatsJournalEntry
    {
        string Type { get; }
        string Target { get; }
        string TargetFaction { get; }
        bool HasFaction(string faction);
        long FactionReward(string faction);
    }

    public interface ISuitInformation
    {
        void SuitInformation(SuitList shp, string whereami, ISystem system);
    }

    public interface ISuitLoadoutInformation
    {
        void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system);
    }

    public interface IWeaponInformation
    {
        void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system);
    }

    public interface ICarrierStats
    {
        CarrierDefinitions.CarrierType CarrierType { get;  }
        void UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrier);
    }
}
