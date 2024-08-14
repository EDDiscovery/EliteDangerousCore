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
        }

        // convert to array from Jarray, may be null if fails
        public static StationServices[] ReadJson(JToken services)
        {
            if (services != null)
            {
                Dictionary<string, string> translations = new Dictionary<string, string>
                {
                    ["engineer"] = "Workshop",
                    ["modulepacks"] = "FleetCarrierAdministration",
                    ["carrierfuel"] = "FleetCarrierFuel",
                    ["carriermanagement"] = "FleetCarrierManagement",
                    ["carriervendor"] = "FleetCarrierVendor",
                    ["facilitator"] = "InterstellarFactorsContact",
                    ["commodities"] = "Market",
                    ["voucherredemption"] = "RedemptionOffice",
                    ["rearm"] = "Restock",
                    ["searchrescue"] = "SearchAndRescue",
                    ["techbroker"] = "TechnologyBroker",
                    ["exploration"] = "UniversalCartographics",
                };
                var ret = services.Array()?.ToObject<StationDefinitions.StationServices[]>(false, false,
                        preprocess: (t, x) =>
                        {
                            if (translations.TryGetValue(x.ToLowerInvariant(), out string tx))
                                return tx;
                            else
                                return x;
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
            if (fdname == null || fdname == "")     // again seen empty output from fdev of type
                return StarportTypes.Unknown;

            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StarportTypes value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown starport type {fdname}");
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
            fdname = fdname.ToLowerInvariant().Replace(" ", "").Replace(";", "");
            if (Enum.TryParse(fdname, true, out StarportState value))
                return value;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown starport state {fdname}");
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
    }
}


