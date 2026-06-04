/*
 * Copyright 2023-2026 EDDiscovery development team
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
using System.Collections.Generic;
using static EliteDangerousCore.EconomyDefinitions;
using static EliteDangerousCore.GovernmentDefinitions;
using static EliteDangerousCore.StationDefinitions;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public static EDStar? SpanshStarNameToEDStar(string name)
        {
            if (spanshtoedstar.TryGetValue(name, out EDStar value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** SPANSH failed to decode star `{name}`");
                return null;
            }
        }

        // from https://spansh.co.uk/api/bodies/field_values/subtype
        private static Dictionary<string, EDStar> spanshtoedstar = new Dictionary<string, EDStar>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "O (Blue-White) Star", EDStar.O },
            { "B (Blue-White) Star", EDStar.B },
            { "A (Blue-White) Star", EDStar.A },
            { "F (White) Star", EDStar.F },
            { "G (White-Yellow) Star", EDStar.G },
            { "K (Yellow-Orange) Star", EDStar.K },
            { "M (Red dwarf) Star", EDStar.M },

            { "L (Brown dwarf) Star", EDStar.L },
            { "T (Brown dwarf) Star", EDStar.T },
            { "Y (Brown dwarf) Star", EDStar.Y },
            { "Herbig Ae Be Star", EDStar.AeBe },
            { "Herbig Ae/Be Star", EDStar.AeBe },
            { "T Tauri Star", EDStar.TTS },

            { "Wolf-Rayet Star", EDStar.W },
            { "Wolf-Rayet N Star", EDStar.WN },
            { "Wolf-Rayet NC Star", EDStar.WNC },
            { "Wolf-Rayet C Star", EDStar.WC },
            { "Wolf-Rayet O Star", EDStar.WO },

            // missing CS
            { "C Star", EDStar.C },
            { "CN Star", EDStar.CN },
            { "CJ Star", EDStar.CJ },
            // missing CHd

            { "MS-type Star", EDStar.MS },
            { "S-type Star", EDStar.S },

            { "White Dwarf (D) Star", EDStar.D },
            { "White Dwarf (DA) Star", EDStar.DA },
            { "White Dwarf (DAB) Star", EDStar.DAB },
            // missing DAO
            { "White Dwarf (DAZ) Star", EDStar.DAZ },
            { "White Dwarf (DAV) Star", EDStar.DAV },
            { "White Dwarf (DB) Star", EDStar.DB },
            { "White Dwarf (DBZ) Star", EDStar.DBZ },
            { "White Dwarf (DBV) Star", EDStar.DBV },
            // missing DO,DOV
            { "White Dwarf (DQ) Star", EDStar.DQ },
            { "White Dwarf (DC) Star", EDStar.DC },
            { "White Dwarf (DCV) Star", EDStar.DCV },
            // missing DX
            { "Neutron Star", EDStar.N },
            { "Black Hole", EDStar.H },
            // missing X but not confirmed with actual journal data


            { "A (Blue-White super giant) Star", EDStar.A_BlueWhiteSuperGiant },
            { "F (White super giant) Star", EDStar.F_WhiteSuperGiant },
            { "M (Red super giant) Star", EDStar.M_RedSuperGiant },
            { "M (Red giant) Star", EDStar.M_RedGiant},
            { "K (Yellow-Orange giant) Star", EDStar.K_OrangeGiant },
            // missing rogueplanet, nebula, stellarremanant
            { "Supermassive Black Hole", EDStar.SuperMassiveBlackHole },
            { "B (Blue-White super giant) Star", EDStar.B_BlueWhiteSuperGiant },
            { "G (White-Yellow super giant) Star", EDStar.G_WhiteSuperGiant },
        };

        private static EDPlanet? SpanshPlanetNameToEDPlanet(string name)
        {
            if (spanshtoedplanet.TryGetValue(name, out EDPlanet value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** SPANSH failed to decode planet `{name}`");
                return null;
            }
        }

        private static Dictionary<string, EDPlanet> spanshtoedplanet = new Dictionary<string, EDPlanet>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Ammonia world", EDPlanet.Ammonia_world },
            { "Class I gas giant", EDPlanet.Sudarsky_class_I_gas_giant },
            { "Class II gas giant", EDPlanet.Sudarsky_class_II_gas_giant },
            { "Class III gas giant", EDPlanet.Sudarsky_class_III_gas_giant },
            { "Class IV gas giant", EDPlanet.Sudarsky_class_IV_gas_giant },
            { "Class V gas giant", EDPlanet.Sudarsky_class_V_gas_giant },
            { "Earth-like world", EDPlanet.Earthlike_body },
            { "Gas giant with ammonia-based life", EDPlanet.Gas_giant_with_ammonia_based_life },
            { "Gas giant with water-based life", EDPlanet.Gas_giant_with_water_based_life },
            { "Helium gas giant", EDPlanet.Helium_gas_giant },
            { "Helium-rich gas giant", EDPlanet.Helium_rich_gas_giant },
            { "High metal content world", EDPlanet.High_metal_content_body },
            { "Metal-rich body", EDPlanet.Metal_rich_body },
            { "Icy body", EDPlanet.Icy_body },
            { "Rocky Ice world", EDPlanet.Rocky_ice_body },
            { "Rocky body", EDPlanet.Rocky_body },
            { "Water giant", EDPlanet.Water_giant },
            { "Water world", EDPlanet.Water_world },
        };

        private static Government GovernmentNameToEnum(string englishname)
        {
            foreach (var kvp in spanshgovernmentnames)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Trace.WriteLine($"*** Spansh Reverse lookup government failed {englishname}");
            return Government.Unknown;
        }

        // converts spansh name to EDD name
        private static Dictionary<Economy, string> spansheconomynames = new Dictionary<Economy, string>()
        {
            // 22/4/24 list
            [Economy.Agri] = "Agriculture",
            [Economy.Colony] = "Colony",
            [Economy.Damaged] = "Damaged",
            [Economy.Engineer] = "Engineering",
            [Economy.Extraction] = "Extraction",
            [Economy.High_Tech] = "High Tech",
            [Economy.Industrial] = "Industrial",
            [Economy.Military] = "Military",
            [Economy.Prison] = "Prison",
            [Economy.Carrier] = "Private Enterprise",
            [Economy.Refinery] = "Refinery",
            [Economy.Repair] = "Repair",
            [Economy.Rescue] = "Rescue",
            [Economy.Service] = "Service",
            [Economy.Terraforming] = "Terraforming",
            [Economy.Tourism] = "Tourism",

            [Economy.None] = "None",
            [Economy.Undefined] = "Undefined",
            [Economy.Unknown] = "Unknown",      // addition to allow Unknown to be mapped
        };

        public static Economy EconomyNameToEnum(string englishname)
        {
            foreach (var kvp in spansheconomynames)
            {
                if (englishname.EqualsIIC(kvp.Value))
                    return kvp.Key;
            }
            System.Diagnostics.Trace.WriteLine($"*** Spansh Economy Reverse lookup failed {englishname}");
            return Economy.Unknown;
        }

        // on right, spansh name
        private static Dictionary<StarportTypes, string> spanshstartporttypenames = new Dictionary<StarportTypes, string>()
        {
            [StarportTypes.AsteroidBase] = "Asteroid Base",
            [StarportTypes.Coriolis] = "Coriolis Starport",
            [StarportTypes.FleetCarrier] = "Drake-Class Carrier",
            [StarportTypes.Megaship] = "Mega Ship",
            [StarportTypes.Ocellus] = "Ocellus Starport",
            [StarportTypes.Orbis] = "Orbis Starport",
            [StarportTypes.Outpost] = "Outpost",
            [StarportTypes.SurfaceStation] = "Planetary Outpost",
            [StarportTypes.CraterPort] = "Planetary Port",
            [StarportTypes.OnFootSettlement] = "Settlement",
            [StarportTypes.SpaceConstructionDepot] = "Space Construction Depot",
            [StarportTypes.PlanetaryConstructionDepot] = "Planetary Construction Depot",
            [StarportTypes.DockablePlanetStation] = "Dockable Planet Station",
            [StarportTypes.Dodec] = "Dodec Starport",
        };

        public static StarportTypes StarportTypeNameToEnum(string englishname)
        {
            foreach (var kvp in spanshstartporttypenames)
            {
                if (englishname.Equals(kvp.Value, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Trace.WriteLine($"*** Reverse lookup name types failed {englishname}");
            return StarportTypes.Unknown;
        }

        private static Dictionary<StationServices, string> spanshservicenames = new Dictionary<StationServices, string>()   // 22/4/24
        {
            [StationServices.ApexInterstellar] = "Apex Interstellar",
            [StationServices.Autodock] = "Autodock",
            [StationServices.Bartender] = "Bartender",
            [StationServices.BlackMarket] = "Black Market",
            [StationServices.Contacts] = "Contacts",
            [StationServices.CrewLounge] = "Crew Lounge",
            [StationServices.Dock] = "Dock",
            [StationServices.FleetCarrierAdministration] = "Fleet Carrier Administration",
            [StationServices.FleetCarrierManagement] = "Fleet Carrier Management",
            [StationServices.FleetCarrierFuel] = "Fleet Carrier Fuel",
            [StationServices.FleetCarrierVendor] = "Fleet Carrier Vendor",
            [StationServices.FlightController] = "Flight Controller",
            [StationServices.FrontlineSolutions] = "Frontline Solutions",
            [StationServices.InterstellarFactorsContact] = "Interstellar Factors Contact",
            [StationServices.Livery] = "Livery",
            [StationServices.Market] = "Market",
            [StationServices.MaterialTrader] = "Material Trader",
            [StationServices.Missions] = "Missions",
            [StationServices.MissionsGenerated] = "Missions Generated",
            [StationServices.OnDockMission] = "On Dock Mission",
            [StationServices.Outfitting] = "Outfitting",
            [StationServices.PioneerSupplies] = "Pioneer Supplies",
            [StationServices.Powerplay] = "Powerplay",
            [StationServices.RedemptionOffice] = "Redemption Office",
            [StationServices.Refuel] = "Refuel",
            [StationServices.Repair] = "Repair",
            [StationServices.Restock] = "Restock",
            [StationServices.SearchAndRescue] = "Search And Rescue",
            [StationServices.Shipyard] = "Shipyard",
            [StationServices.Shop] = "Shop",
            [StationServices.SocialSpace] = "Social Space",
            [StationServices.StationMenu] = "Station Menu",
            [StationServices.StationOperations] = "Station Operations",
            [StationServices.TechnologyBroker] = "Technology Broker",
            [StationServices.Tuning] = "Tuning",
            [StationServices.UniversalCartographics] = "Universal Cartographics",
            [StationServices.VistaGenomics] = "Vista Genomics",
            [StationServices.Workshop] = "Workshop",
            [StationServices.ConstructionServices] = "Construction Services",
            [StationServices.SystemColonisation] = "System Colonisation",
            [StationServices.RefineryContact] = "Refinery Contact",
            [StationServices.SquadronBank] = "Squadron Bank",
        };

        private static StationServices StationServiceNameToEnum(string fdname)
        {
            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StationServices value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown Spansh services type {fdname}");
                return StationServices.Unknown;
            }
        }

        // 
        private static Dictionary<Government, string> spanshgovernmentnames = new Dictionary<Government, string>()
        {
            [Government.Anarchy] = "Anarchy",
            [Government.Communism] = "Communism",
            [Government.Confederacy] = "Confederacy",
            [Government.Cooperative] = "Cooperative",
            [Government.Corporate] = "Corporate",
            [Government.Democracy] = "Democracy",
            [Government.Dictatorship] = "Dictatorship",
            [Government.Engineer] = "Engineer",
            [Government.Feudal] = "Feudal",
            [Government.None] = "None",
            [Government.Patronage] = "Patronage",
            [Government.Prison] = "Prison",
            [Government.PrisonColony] = "Prison Colony",    // not listed in spansh
            [Government.Carrier] = "Private Ownership",
            [Government.Theocracy] = "Theocracy",
            [Government.Megaconstruction] = "Megaconstruction",

            //            [Government.Imperial] = "Imperial",       // not in spansh

            [Government.Unknown] = "Unknown",      // addition to allow Unknown to be mapped
        };




    }
}

