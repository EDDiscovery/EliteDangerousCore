/*
 * Copyright © 2025-2025 EDDiscovery development team
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

using System;

namespace EliteDangerousCore
{
    public class CarrierDefinitions
    {
        public enum CarrierType { FleetCarrier, SquadronCarrier,  UnknownType };
    
        // maps the allegiance fdname to an enum.  Spaces can be in the name ("Pilots Federation") to cope with Spansh
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static CarrierType ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return CarrierType.FleetCarrier;

            if (Enum.TryParse<CarrierType>(fdname, true, out CarrierType type))
                return type;
            else
                return CarrierType.FleetCarrier;
        }
        public static string ToEnglish(CarrierType al)
        {
            return al.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(CarrierType al)
        {
            return ToEnglish(al).Tx();
        }

        public enum ServiceOperationType { Activate, Deactivate, Pause, Resume, Replace, Unknown }
        public enum ModulePackOperationType { BuyPack, SellPack, RestockPack, Unknown }
        public enum ShipPackOperationType { BuyPack, SellPack, RestockPack, Unknown }

        // as per frontier CrewRole Entry
        public enum ServiceType
        {
            BridgeCrew, CommodityTrading, TritiumDepot,        // not acrew services, but core items. UserControlCarrier iterates along this list and we use this to guide it

            // searching logs for CarrierStats and CarrierCrewServices gave these july 26
            Refuel,
            Repair,
            Rearm,
            VoucherRedemption,
            Shipyard,
            Outfitting,
            BlackMarket,
            Exploration,
            Bartender,      
            VistaGenomics,
            PioneerSupplies,

            // added july 26
            Captain,
            CarrierFuel,
            Commodities,

            Unknown,

            // originally before july 26 Refuel, Repair, Rearm, VoucherRedemption, Shipyard, Outfitting, BlackMarket, Exploration, VistaGenomics, PioneerSupplies,
        };
        public static ServiceType ToEnumServiceType(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return ServiceType.Unknown;

            if (Enum.TryParse<ServiceType>(fdname, true, out ServiceType type))
                return type;
            else
            {
                BaseUtils.Debugger.TraceBreak("*** Carrier Service type not recognised {fdname}");
                return ServiceType.Unknown;
            }
        }

        static public string GetTranslatedServiceName(ServiceType t) { return translatedname[(int)t]; }
        static public bool IsOptionalService(ServiceType t) { return t >= ServiceType.Refuel && t != ServiceType.Unknown; }
        static public bool IsValidService(ServiceType t) { return t != ServiceType.Unknown; }

        private static string[] translatedname = new string[] {
            "Bridge Crew".Tx(),
            "Commodity Trading".Tx(),
            "Tritium Depot".Tx(),
            "Refuel Station".Tx(),
            "Repair Crews".Tx(),
            "Armoury".Tx(),
            "Redemption Office".Tx(),
            "Shipyard".Tx(),
            "Outfitting".Tx(),
            "Secure Warehouse".Tx(),
            "Universal Cartographics".Tx(),
            "Concourse Bar".Tx(),
            "Vista Genomics".Tx(),
            "Pioneer Supplies".Tx(),

            "Captain",      // not shown, not translating
            "CarrierFuel",
            "Commodities",
            "Unknown",
        };
        static public int GetServiceCount() { var entries = Enum.GetValues(typeof(ServiceType)); return entries.Length - 1; }      // ignore Unknown

        [System.Diagnostics.DebuggerDisplay("{Service} {CargoSize}t {InstallCost}cr up {UpkeepCost}")]
        public class ServicesData       // https://elite-dangerous.fandom.com/wiki/Drake-Class_Carrier
        {
            public ServicesData(CarrierDefinitions.ServiceType t, long cost, long upkeep, long suspendedcost, int cargosize)
            { Service = t; InstallCost = cost; UpkeepCost = upkeep; SuspendedUpkeepCost = suspendedcost; CargoSize = cargosize; }
            public CarrierDefinitions.ServiceType Service { get; set; }
            public long InstallCost { get; set; }
            public long UpkeepCost { get; set; }
            public long SuspendedUpkeepCost { get; set; }
            public long CargoSize { get; set; }
        }

        private static ServicesData[] ServiceInformation = new ServicesData[]        // verified with game oct 22
        {
            new ServicesData(ServiceType.Refuel,40000000,1500000,750000,500),
            new ServicesData(ServiceType.Repair,50000000,1500000,750000,180),
            new ServicesData(ServiceType.Rearm,95000000,1500000,750000,250),
            new ServicesData(ServiceType.VoucherRedemption,150000000,1850000,850000,100),
            new ServicesData(ServiceType.Shipyard,250000000,6500000,1800000,3000),
            new ServicesData(ServiceType.Outfitting,250000000,5000000,1500000,1750),
            new ServicesData(ServiceType.BlackMarket,165000000,2000000,1250000,250),
            new ServicesData(ServiceType.Exploration,150000000,1850000,700000,120),
            new ServicesData(ServiceType.Bartender,200000000,1750000,1250000,150),
            new ServicesData(ServiceType.VistaGenomics,150000000,1500000,700000,120),
            new ServicesData(ServiceType.PioneerSupplies,250000000,5000000,1500000,200),
        };
        public static ServicesData GetDataOnServiceType(ServiceType t) { return Array.Find(CarrierDefinitions.ServiceInformation, x => x.Service == t); }

    }
}

