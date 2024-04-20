/*
 * Copyright © 2023-2023 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License")"] = "you may not use this
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
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class StationDefinitions
    {
        public enum StationServices
        {
            Unknown,
            ApexInterstellar,
            Autodock,
            Bartender,
            BlackMarket,
            Contacts,
            CrewLounge,
            Dock,
            Workshop,      // synonmyms
            FleetCarrierAdministration,
            FleetCarrierFuel,
            FleetCarrierManagement,
            FleetCarrierVendor,
            FlightController,
            FrontlineSolutions,
            InterstellarFactorsContact,
            Initiatives,
            Livery,
            Market,
            MaterialTrader,
            Missions,
            MissionsGenerated,
            OnDockMission,
            Outfitting,
            PioneerSupplies,
            Powerplay,
            RedemptionOffice,
            Refuel,
            Repair,
            Restock,
            SearchAndRescue,
            Shipyard,
            Shop,
            SocialSpace,
            StationMenu,
            StationOperations,
            TechnologyBroker,
            Tuning,
            UniversalCartographics,
            VistaGenomics,
        }

        // maps the services names $government_id; to an enum
        public static StationServices ServicesToEnum(string fdname)
        {
            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StationServices value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"*** Unknown services type {fdname}");
                return StationServices.Unknown;
            }
        }

        public static string ToEnglish(StationServices ec)
        {
            return ec.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StationServices sc)
        {
            string id = "StationServices." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

        // names left are in stationservices, names right are spansh names

        private static Dictionary<string, string> ServiceTypesSpansh = new Dictionary<string, string>()
        {
            ["apexinterstellar"] = "Apex Interstellar",
            ["autodock"] = "Autodock",
            ["bartender"] = "Bartender",
            ["blackmarket"] = "Black Market",
            ["contacts"] = "Contacts",
            ["crewlounge"] = "Crew Lounge",
            ["dock"] = "Dock",
            ["workshop"] = "Workshop",      // synonmyms
            ["engineer"] = "Workshop",
            ["modulepacks"] = "Fleet Carrier Administration",
            ["carrierfuel"] = "Fleet Carrier Fuel",
            ["carriermanagement"] = "Fleet Carrier Management",
            ["carriervendor"] = "Fleet Carrier Vendor",
            ["flightcontroller"] = "Flight Controller",
            ["frontlinesolutions"] = "Frontline Solutions",
            ["facilitator"] = "Interstellar Factors Contact",
            ["initiatives"] = "Initiatives",
            ["livery"] = "Livery",
            ["commodities"] = "Market",
            ["materialtrader"] = "Material Trader",
            ["missions"] = "Missions",
            ["missionsgenerated"] = "Missions Generated",
            ["ondockmission"] = "On Dock Mission",
            ["outfitting"] = "Outfitting",
            ["pioneersupplies"] = "Pioneer Supplies",
            ["powerplay"] = "Powerplay",
            ["voucherredemption"] = "Redemption Office",
            ["refuel"] = "Refuel",
            ["repair"] = "Repair",
            ["rearm"] = "Restock",
            ["searchandrescue"] = "Search And Rescue",
            ["searchrescue"] = "Search And Rescue",
            ["shipyard"] = "Shipyard",
            ["shop"] = "Shop",
            ["socialspace"] = "Social Space",
            ["stationmenu"] = "Station Menu",
            ["stationoperations"] = "Station Operations",
            ["techbroker"] = "Technology Broker",
            ["tuning"] = "Tuning",
            ["exploration"] = "Universal Cartographics",
            ["vistagenomics"] = "Vista Genomics",

        };


        //  and text name
        static public List<KeyValuePair<string, string>> SpanshNamesAndText()
        {
            return ServiceTypesSpansh.Distinct().ToList();
        }

        public enum StarportTypes
        {
            Unknown,
            AsteroidBase,
            Coriolis,
            FleetCarrier,
            Megaship,
            Ocellus,
            Bernal,
            Orbis,
            Outpost,
            OnFootSettlement,
            SurfaceStation,
            CraterOutpost,
            CraterPort,
        }

        // maps the StationType field to an enum.
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static StarportTypes StarportTypeToEnum(string fdname)
        {
            if (fdname == null)
                return StarportTypes.Unknown;

            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StarportTypes value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"*** Unknown starport type {fdname}");
                return StarportTypes.Unknown;
            }
        }

        public static string ToEnglish(StarportTypes starporttype)
        {
            return starporttype.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StarportTypes sc)
        {
            string id = "StarportTypes." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

        // on right, spansh name
        private static Dictionary<string, string> StarportNameTypesToSpansh = new Dictionary<string, string>()
        {
            // in journal as of Nov 23

            ["asteroidbase"] = "Asteroid Base",
            ["coriolis"] = "Coriolis Starport", 

            ["fleetcarrier"] = "Drake-Class Carrier",   
            ["megaship"] = "Mega Ship",     

            ["ocellus"] = "Ocellus Starport",   
            ["bernal"] = "Ocellus Starport",    
         
            ["orbis"] = "Orbis Starport",       

            ["outpost"] = "Outpost",            
            ["onfootsettlement"] = "Settlement",    

            ["surfacestation"] = "Planetary Outpost",   
            ["crateroutpost"] = "Planetary Outpost",

            ["craterport"] = "Planetary Port",       

            // these are from Spansh but not seen in my journals

            ["coriolis starport"] = "Coriolis Starport",
            ["orbis starport"] = "Orbis Starport",
            ["ocellus starport"] = "Ocellus Starport",

            ["planetary outpost"] = "Planetary Outpost",
            ["planetary port"] = "Planetary Port",
                
            ["settlement"] = "Settlement",      // only seen in redeemvoucher
            ["carrier"] = "Drake-Class Carrier",

            ["civilian outpost"] = "Outpost",
            ["commercial outpost"] = "Outpost",
            ["industrial outpost"] = "Outpost",
            ["military outpost"] = "Outpost",
            ["mining outpost"] = "Outpost",    
            ["scientific outpost"] = "Outpost",
            ["outpostscientific"] = "Outpost",
            ["megashipcivilian"] = "Mega Ship",
        };

        static public IEnumerable<string> StarPortTypesNamesSpansh()
        {
            return StarportNameTypesToSpansh.Values.Distinct();
        }

        public static string ReverseLookupStarportNameTypes(string englishname)
        {
            foreach (var kvp in StarportNameTypesToSpansh)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Debug.WriteLine($"*** Reverse lookup name types failed {englishname}");
            return "Unknown";
        }


        public enum StarportState
        {
            Unknown,
            UnknownSpansh,
            None,
            UnderRepairs,
            Damaged,
            Abandoned,
            UnderAttack,
        }

        // maps the StationType field to an enum
        public static StarportState StarportStateToEnum(string fdname)
        {
            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StarportState value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"*** Unknown starport state {fdname}");
                return StarportState.Unknown;
            }
        }

        public static string ToEnglish(StarportState sts)
        {
            return sts.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StarportState sc)
        {
            string id = "StarportStates." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

    }
}


