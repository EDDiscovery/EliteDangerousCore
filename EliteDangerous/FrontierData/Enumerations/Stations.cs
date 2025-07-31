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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class StationDefinitions
    {
        #region Services

        // Names are sort of syned with Spansh, and are more meaningful than fdnames.
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
            Workshop,                   // engineer
            FleetCarrierAdministration, // modulepacks
            FleetCarrierFuel,           // carrierfuel
            FleetCarrierManagement,     // carriermanagment
            FleetCarrierVendor,         // carriervendor
                FlightController,
                FrontlineSolutions,
            InterstellarFactorsContact, // facilitator
                Initiatives,
                Livery,
            Market,                     // commodities
                MaterialTrader,
                Missions,
                MissionsGenerated,
                OnDockMission,
                Outfitting,
                PioneerSupplies,
                Powerplay,
            RedemptionOffice,           // voucherredemption
                Refuel,
                Repair,
            Restock,                    // rearm
                SearchAndRescue,        // also searchrescue
                Shipyard,
                Shop,
                SocialSpace,
                StationMenu,
                StationOperations,
            TechnologyBroker,           // techbroker
                Tuning,
            UniversalCartographics,     // exploration
                VistaGenomics,

            ConstructionServices,   // trailblazers feb 25 in game names, see mapping below to fdnames
            RefineryContact,
            SystemColonisation,
        }

        public static StationServices StationServicesToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return StationServices.Unknown;

            if (stationservicesparselist.TryGetValue(fdname.ToLowerInvariant().Trim(), out StationServices value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Station Services is unknown `{fdname}`");
                return StationServices.Unknown;
            }
        }


        // translate between some journal names and ours
        static Dictionary<string, StationServices> translations = new Dictionary<string, StationServices>
        {
            ["engineer"] = StationServices.Workshop,
            ["modulepacks"] = StationServices.FleetCarrierAdministration,
            ["carrierfuel"] = StationServices.FleetCarrierFuel,
            ["carriermanagement"] = StationServices.FleetCarrierManagement,
            ["carriervendor"] = StationServices.FleetCarrierVendor,
            ["facilitator"] = StationServices.InterstellarFactorsContact,
            ["commodities"] = StationServices.Market,
            ["voucherredemption"] = StationServices.RedemptionOffice,
            ["rearm"] = StationServices.Restock,
            ["searchrescue"] = StationServices.SearchAndRescue,
            ["techbroker"] = StationServices.TechnologyBroker,
            ["exploration"] = StationServices.UniversalCartographics,
            ["colonisationcontribution"] = StationServices.ConstructionServices,
            ["refinery"] = StationServices.RefineryContact,
            ["registeringcolonisation"] = StationServices.SystemColonisation,

        };


        // convert to array from Jarray as formatted using Frontier names, may be null if fails
        public static StationServices[] ReadServicesFromJson(JToken services)
        {
            if (services != null)
            {
                var ret = services.Array()?.ToObject<StationDefinitions.StationServices[]>(false, 
                        process: (t, x) =>      // we need to process the station services enum ourself
                        {
                            if (translations.TryGetValue(x.ToLowerInvariant(), out StationServices tx))     // if its a special name
                                return tx;
                            else
                                return StationServicesToEnum(x);
                        });

                return ret;
            }
            else
                return null;
        }

        public static string ToEnglish(StationServices ec)
        {
            return ec.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StationServices sc)
        {
            return ToEnglish(sc).Tx();
        }

        public static StationServices[] ValidServices()
        {
            var list = (StationServices[])Enum.GetValues(typeof(StationServices));
            return list.Where(x => x != StationServices.Unknown).ToArray();
        }

        public static void Build(System.Text.StringBuilder sb, bool title, StationServices[] list)
        {
            if ( title )
                sb.Append("Station services: ".Tx());

            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0)
                {
                    if (i % 10 == 0)
                        sb.AppendCR();
                    else
                        sb.AppendCS();
                }

                sb.Append(ToLocalisedLanguage(list[i]));
            }
        }

        #endregion

        #region Starports
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
            SpaceConstructionDepot,
            PlanetaryConstructionDepot,
            DockablePlanetStation,
            GameplayPOI,
        }

        // maps the StationType field to an enum.
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static StarportTypes StarportTypeToEnum(string fdname)
        {
            if (!fdname.HasChars())     // again seen empty output from fdev of type
                return StarportTypes.Unknown;

            string fdm= fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");

            if (starporttypesparselist.TryGetValue(fdm,out StarportTypes value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown starport type `{fdname}`");
                return StarportTypes.Unknown;
            }
        }

        public static string ToEnglish(StarportTypes starporttype)
        {
            if (starporttype == StarportTypes.SpaceConstructionDepot)
                return "Orbital Construction Site";
            else if (starporttype == StarportTypes.PlanetaryConstructionDepot)
                return "Planetary Construction Site";
            else if (starporttype == StarportTypes.GameplayPOI)
                return "Construction POI";
            else
                return starporttype.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StarportTypes sc)
        {
            string id = "StarportTypes." + sc.ToString();
            return ToEnglish(sc).Tx();
        }

        public static StarportTypes[] ValidTypes(bool removeduplicates)
        {
            var list = (StarportTypes[])Enum.GetValues(typeof(StarportTypes));
            return list.Where(x => x != StarportTypes.Unknown && (!removeduplicates || x != StarportTypes.Bernal)).ToArray();
        }

        #endregion

        #region Startport state

        public enum StarportState
        {
            Unknown,
            None,
            UnderRepairs,
            Damaged,
            Abandoned,
            UnderAttack,
            Construction,
        }

        // maps the StationType field to an enum
        public static StarportState StarportStateToEnum(string fdname)
        {
            string fdm = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");

            if (Enum.TryParse(fdm, true, out StarportState value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown starport state `{fdname}`");
                return StarportState.Unknown;
            }
        }

        public static string ToEnglish(StarportState sts)
        {
            return sts.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StarportState sc)
        {
            return ToEnglish(sc).Tx();
        }

        public static StarportState[] ValidStates()
        {
            var list = (StarportState[])Enum.GetValues(typeof(StarportState));
            return list.Where(x => x != StarportState.Unknown).ToArray();
        }

        #endregion

        #region Market Classification

// log analysis May 2025
//128 = AsteroidBase,Coriolis,CraterOutpost,CraterPort,MegaShip,Ocellus,Orbis,Outpost,SurfaceStation
//129 = Coriolis,MegaShip,Ocellus,SurfaceStation
//3221-3231 = Coriolis,Ocellus,Orbis,Outpost
//3240 = Orbis
//3400 = MegaShip
//3500-3546 = CraterOutpost,CraterPort,SurfaceStation
//3600 = MegaShip
//3700 = FleetCarrier,SurfaceStation
//3701-3711 = FleetCarrier
//3789 = CraterOutpost,CraterPort
//3790 = CraterOutpost,CraterPort
//3791 = CraterOutpost,CraterPort
//3802-3928 = OnFootSettlement
//3930 = SurfaceStation
//3951 = SpaceConstructionDepot,SurfaceStation
//3952 = SpaceConstructionDepot,SurfaceStation
//3953 = SurfaceStation
//3954 = SpaceConstructionDepot,SurfaceStation
//3955 = SurfaceStation
//3956 = SurfaceStation
//3957 = SpaceConstructionDepot,SurfaceStation
//3958 = SurfaceStation
//3959 = SurfaceStation
//3960 = SpaceConstructionDepot
//3962 = SpaceConstructionDepot
//4207 = SurfaceStation
//4210 = Outpost,SurfaceStation
//4211 = Coriolis,OnFootSettlement,Outpost,PlanetaryConstructionDepot,SurfaceStation
//4212 = OnFootSettlement,Outpost
//4214 = OnFootSettlement,Outpost,PlanetaryConstructionDepot
//4215 = Outpost,PlanetaryConstructionDepot,SurfaceStation
//4216 = Coriolis
//4217 = CraterOutpost,Outpost,PlanetaryConstructionDepot
//4221 = Orbis,SurfaceStation
//4222 = SurfaceStation
//4223 = PlanetaryConstructionDepot
//4233 = Orbis
//4238 = CraterOutpost,Ocellus,SurfaceStation
//4239 = OnFootSettlement,PlanetaryConstructionDepot
//4242 = PlanetaryConstructionDepot

        public enum Classification { 
            Unknown,
            NormalPort,  
            MegaShip,       
            RescueShip,     
            FleetCarrier,
            TypesBelowColonisation,
            SpaceConstructionDepot,
            ColonisationShip,
            ColonisationPort,
        }

        public static Classification Classify(long marketid, StarportTypes type)
        {
            long topcode = marketid / 1000000; // codes 128, 3220 etc.
            if (topcode == 3400)
                return Classification.RescueShip;
            if (type == StarportTypes.Megaship || topcode == 3600)
                return Classification.MegaShip;
            if (type == StarportTypes.SpaceConstructionDepot)
                return Classification.SpaceConstructionDepot;
            if (topcode >= 3700 && topcode <= 3770)                 // these are all fleet carriers, some are called surface stations by error
                return Classification.FleetCarrier;
            if (topcode >= 3950 && topcode <= 3990)
                return Classification.ColonisationShip;
            if (topcode >= 4200 && topcode <= 4290)                  // only 4200-4230 seen, allow space
                return Classification.ColonisationPort;
            return Classification.NormalPort;
        }

        #endregion

        static Dictionary<string, StationServices> stationservicesparselist;
        static Dictionary<string, StarportTypes> starporttypesparselist;
        static StationDefinitions()
        {
            stationservicesparselist = new Dictionary<string, StationServices>();
            foreach (var v in Enum.GetValues(typeof(StationServices)))
                stationservicesparselist[v.ToString().ToLowerInvariant()] = (StationServices)v;
            starporttypesparselist = new Dictionary<string, StarportTypes>();
            foreach (var v in Enum.GetValues(typeof(StarportTypes)))
                starporttypesparselist[v.ToString().ToLowerInvariant()] = (StarportTypes)v;
        }


    }
}


