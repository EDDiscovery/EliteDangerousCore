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
            string id = "StationServices." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

        public static StationServices[] ValidServices()
        {
            var list = (StationServices[])Enum.GetValues(typeof(StationServices));
            return list.Where(x => x != StationServices.Unknown).ToArray();
        }

        public static void Build(System.Text.StringBuilder sb, bool title, StationServices[] list)
        {
            if ( title )
                sb.Append("Station services: ".T(EDCTx.JournalEntry_Stationservices));

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
            return starporttype.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(StarportTypes sc)
        {
            string id = "StarportTypes." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
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
            string id = "StarportStates." + sc.ToString();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(sc), id);
        }

        public static StarportState[] ValidStates()
        {
            var list = (StarportState[])Enum.GetValues(typeof(StarportState));
            return list.Where(x => x != StarportState.Unknown).ToArray();
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


