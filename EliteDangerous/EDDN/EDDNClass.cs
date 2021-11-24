/*
 * Copyright © 2016 EDDiscovery development team
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

using BaseUtils.JSON;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace EliteDangerousCore.EDDN
{
    public class EDDNClass : BaseUtils.HttpCom
    {
        public string commanderName;
        public bool isBeta;

        static public string SoftwareName { get; set; } = "EDDiscovery";

        private readonly string fromSoftwareVersion;
        private readonly string EDDNServer = "https://eddn.edcd.io:4430/upload/";

        public EDDNClass()
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];
            commanderName = EDCommander.Current.Name;

            httpserveraddress = EDDNServer;
        }

        private JObject Header()
        {
            JObject header = new JObject();

            header["uploaderID"] = commanderName;
            header["softwareName"] = SoftwareName;
            header["softwareVersion"] = fromSoftwareVersion;

            return header;
        }

        static public bool IsEDDNMessage( JournalTypeEnum EntryType)
        {
            return (EntryType == JournalTypeEnum.Scan ||
                 EntryType == JournalTypeEnum.Docked ||
                 EntryType == JournalTypeEnum.FSDJump || EntryType == JournalTypeEnum.CarrierJump || EntryType == JournalTypeEnum.Location ||
                 EntryType == JournalTypeEnum.Market ||
                 EntryType == JournalTypeEnum.Shipyard ||
                 EntryType == JournalTypeEnum.SAASignalsFound ||
                 EntryType == JournalTypeEnum.EDDCommodityPrices ||
                 EntryType == JournalTypeEnum.Outfitting);
        }

        static public bool IsDelayableEDDNMessage(JournalTypeEnum EntryType, DateTime EventTimeUTC)
        {
            DateTime ed22 = new DateTime(2016, 10, 25, 12, 0, 0);
            if ((EntryType == JournalTypeEnum.Scan ||
                 EntryType == JournalTypeEnum.SAASignalsFound) && EventTimeUTC > ed22) return true;
            else return false;
        }

        private string GetEDDNJournalSchemaRef(bool test = false)
        {
            if (isBeta || commanderName.StartsWith("[BETA]") || test)
                return "https://eddn.edcd.io/schemas/journal/1/test";
            else
                return "https://eddn.edcd.io/schemas/journal/1";
        }

        private string GetEDDNCommoditySchemaRef()
        {
            if (isBeta || commanderName.StartsWith("[BETA]"))
                return "https://eddn.edcd.io/schemas/commodity/3/test";
            else
                return "https://eddn.edcd.io/schemas/commodity/3";
                //return "https://eddn.edcd.io/schemas/commodity/3/test"; // For testing now.
        }

        private string GetEDDNOutfittingSchemaRef()
        {
            if (isBeta || commanderName.StartsWith("[BETA]"))
                return "https://eddn.edcd.io/schemas/outfitting/2/test";
            else
                return "https://eddn.edcd.io/schemas/outfitting/2";
        }

        private string GetEDDNShipyardSchemaRef()
        {
            if (isBeta || commanderName.StartsWith("[BETA]"))
                return "https://eddn.edcd.io/schemas/shipyard/2/test";
            else
                return "https://eddn.edcd.io/schemas/shipyard/2";
        }

        private static readonly JObject AllowedFieldsCommon = new JObject
        {
            ["timestamp"] = true,
            ["event"] = true,
            ["StarSystem"] = true,
            ["SystemAddress"] = true,
            ["StarPos"] = "[]",
        };

        private static readonly JObject AllowedFieldsLocJump = new JObject(AllowedFieldsCommon)
        {
            ["SystemAllegiance"] = true,
            ["SystemEconomy"] = true,
            ["SystemSecondEconomy"] = true,
            ["SystemFaction"] = new JObject
            {
                ["Name"] = true,
                ["FactionState"] = true,
            },
            ["SystemGovernment"] = true,
            ["SystemSecurity"] = true,
            ["Population"] = true,
            ["PowerplayState"] = true,
            ["Powers"] = "[]",
            ["Factions"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Allegiance"] = true,
                    ["Government"] = true,
                    ["FactionState"] = true,
                    ["Happiness"] = true,
                    ["Influence"] = true,
                    ["ActiveStates"] = new JArray
                    {
                        new JObject
                        {
                            ["State"] = true
                        }
                    },
                    ["PendingStates"] = new JArray
                    {
                        new JObject
                        {
                            ["State"] = true,
                            ["Trend"] = true
                        }
                    },
                    ["RecoveringStates"] = new JArray
                    {
                        new JObject
                        {
                            ["State"] = true,
                            ["Trend"] = true
                        }
                    },
                }
            },
            ["Conflicts"] = new JArray
            {
                new JObject
                {
                    ["WarType"] = true,
                    ["Status"] = true,
                    ["Faction1"] = new JObject
                    {
                        ["Name"] = true,
                        ["Stake"] = true,
                        ["WonDays"] = true
                    },
                    ["Faction2"] = new JObject
                    {
                        ["Name"] = true,
                        ["Stake"] = true,
                        ["WonDays"] = true
                    },
                }
            }
        };

        private static readonly JObject AllowedFieldsFSDJump = new JObject(AllowedFieldsLocJump)
        {
            ["Body"] = true,
            ["BodyID"] = true,
            ["BodyType"] = true,
        };

        private static readonly JObject AllowedFieldsLocation = new JObject(AllowedFieldsLocJump)
        {
            ["Body"] = true,
            ["BodyID"] = true,
            ["BodyType"] = true,
            ["Docked"] = true,
            ["MarketID"] = true,
            ["StationName"] = true,
            ["StationType"] = true,
            ["DistFromStarLS"] = true,
            ["StationFaction"] = new JObject
            {
                ["Name"] = true,
                ["FactionState"] = true,
            },
            ["StationAllegiance"] = true,
            ["StationGovernment"] = true,
            ["StationEconomy"] = true,
            ["StationServices"] = "[]",
            ["StationState"] = true,
            ["StationEconomies"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Proportion"] = true
                }
            },
        };

        private static readonly JObject AllowedFieldsDocked = new JObject(AllowedFieldsCommon)
        {
            ["MarketID"] = true,
            ["StationName"] = true,
            ["StationType"] = true,
            ["DistFromStarLS"] = true,
            ["StationFaction"] = new JObject
            {
                ["Name"] = true,
                ["FactionState"] = true,
            },
            ["StationAllegiance"] = true,
            ["StationGovernment"] = true,
            ["StationEconomy"] = true,
            ["StationServices"] = "[]",
            ["StationState"] = true,
            ["LandingPads"] = new JObject
            {
                ["Small"] = true,
                ["Medium"] = true,
                ["Large"] = true,
            },
            ["StationEconomies"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Proportion"] = true
                }
            },
            ["Taxi"] = true,
        };

        private static readonly JObject AllowedFieldsScan = new JObject(AllowedFieldsCommon)
        {
            // Common
            ["BodyName"] = true,
            ["BodyID"] = true,
            ["ScanType"] = true,
            ["DistanceFromArrivalLS"] = true,
            ["Parents"] = new JArray
            {
                new JObject
                {
                    ["Null"] = true,
                    ["Star"] = true,
                    ["Planet"] = true,
                    ["Ring"] = true
                }
            },
            ["WasDiscovered"] = true,
            ["WasMapped"] = true,
            // Star / Planet
            ["RotationPeriod"] = true,
            ["OrbitalPeriod"] = true,
            ["SemiMajorAxis"] = true,
            ["Eccentricity"] = true,
            ["OrbitalInclination"] = true,
            ["Periapsis"] = true,
            ["AxialTilt"] = true,
            ["Radius"] = true,
            ["TidalLock"] = true,
            // Star
            ["StarType"] = true,
            ["Age_MY"] = true,
            ["StellarMass"] = true,
            ["AbsoluteMagnitude"] = true,
            ["Luminosity"] = true,
            ["Subclass"] = true,
            // Planet
            ["PlanetClass"] = true,
            ["TerraformState"] = true,
            ["Atmosphere"] = true,
            ["AtmosphereType"] = true,
            ["Volcanism"] = true,
            ["MassEM"] = true,
            ["SurfaceGravity"] = true,
            ["SurfaceTemperature"] = true,
            ["SurfacePressure"] = true,
            ["Landable"] = true,
            // Rings
            ["ReserveLevel"] = true,
            ["Rings"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["RingClass"] = true,
                    ["MassMT"] = true,
                    ["InnerRad"] = true,
                    ["OuterRad"] = true
                }
            },
            // Materials
            ["Materials"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Percent"] = true
                }
            },
            ["Composition"] = new JObject
            {
                ["Rock"] = true,
                ["Metal"] = true,
                ["Ice"] = true
            },
            ["AtmosphereComposition"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Percent"] = true
                }
            }
        };

        private static readonly JObject AllowedFieldsSAASignalsFound = new JObject(AllowedFieldsCommon)
        {
            ["BodyID"] = true,
            ["BodyName"] = true,
            ["Signals"] = new JArray
            {
                new JObject
                {
                    ["Count"] = true,
                    ["Type"] = true
                }
            }
        };


        private JObject RemoveCommonKeys(JObject obj)
        {
            foreach (var key in obj.PropertyNames())
            {
                if (key.EndsWith("_Localised") || key.StartsWith("EDD"))
                {
                    obj.Remove(key);
                }
            }

            return obj;
        }

        private JObject RemoveFactionReputation(JObject obj)
        {
            JArray factions = obj["Factions"] as JArray;

            if (factions != null)
            {
                foreach (JObject faction in factions)
                {
                    faction.Remove("MyReputation");
                    faction.Remove("SquadronFaction");
                    faction.Remove("HomeSystem");
                    faction.Remove("HappiestSystem");
                    RemoveCommonKeys(faction);
                }
            }

            return obj;
        }

        private JObject RemoveStationEconomyKeys(JObject jo)
        {
            JArray economies = jo["StationEconomies"] as JArray;

            if (economies != null)
            {
                foreach (JObject economy in economies)
                {
                    RemoveCommonKeys(economy);
                }

                jo["StationEconomies"] = economies;
            }

            return jo;
        }

        public JObject CreateEDDNMessage(JournalFSDJump journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();
            
            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["FuelUsed"].IsNull() || message["SystemAddress"] == null)  // Old ED 2.1 messages has no Fuel used fields
                return null;

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            message = RemoveCommonKeys(message);
            message = RemoveFactionReputation(message);
            message.Remove("BoostUsed");
            message.Remove("MyReputation"); 
            message.Remove("JumpDist");
            message.Remove("FuelUsed");
            message.Remove("FuelLevel");
            message.Remove("StarPosFromEDSM");
            message.Remove("ActiveFine");

            message = message.Filter(AllowedFieldsFSDJump);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNMessage(JournalLocation journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            message = RemoveCommonKeys(message);
            message = RemoveFactionReputation(message);
            message = RemoveStationEconomyKeys(message);
            message.Remove("StarPosFromEDSM");
            message.Remove("Latitude");
            message.Remove("Longitude");
            message.Remove("MyReputation");
            message.Remove("ActiveFine");

            message =message.Filter(AllowedFieldsLocation);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNMessage(JournalCarrierJump journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            message = RemoveCommonKeys(message);
            message = RemoveFactionReputation(message);
            message = RemoveStationEconomyKeys(message);
            message.Remove("StarPosFromEDSM");
            message.Remove("Latitude");
            message.Remove("Longitude");
            message.Remove("MyReputation");
            message.Remove("ActiveFine");

            message = message.Filter( AllowedFieldsLocation);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNMessage(JournalDocked journal, ISystem system)
        {
            if (!String.Equals(system.Name, journal.StarSystem, StringComparison.InvariantCultureIgnoreCase))
                return null;

            if (system.SystemAddress == null || journal.SystemAddress == null || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            message = RemoveCommonKeys(message);
            message = RemoveStationEconomyKeys(message);
            message.Remove("CockpitBreach");
            message.Remove("Wanted");
            message.Remove("ActiveFine");

            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });

            message = message.Filter( AllowedFieldsDocked);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNOutfittingMessage(JournalOutfitting journal)
        {
            if (journal.YardInfo.Items == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNOutfittingSchemaRef();

            JObject message = new JObject
            {
                ["timestamp"] = journal.EventTimeUTC.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
                ["systemName"] = journal.YardInfo.StarSystem,
                ["stationName"] = journal.YardInfo.StationName,
                ["stationName"] = journal.YardInfo.StationName,
                ["marketId"] = journal.MarketID,
                ["modules"] = new JArray(journal.YardInfo.Items.Select(m => JournalFieldNaming.NormaliseFDItemName(m.FDName)))
            };

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNShipyardMessage(JournalShipyard journal)
        {
            if (journal.Yard.Ships == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNShipyardSchemaRef();

            JObject message = new JObject
            {
                ["timestamp"] = journal.EventTimeUTC.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
                ["systemName"] = journal.Yard.StarSystem,
                ["stationName"] = journal.Yard.StationName,
                ["marketId"] = journal.MarketID,
                ["ships"] = new JArray(journal.Yard.Ships.Select(m => m.ShipType))
            };

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNMessage(JournalScan journal, ISystem system)
        {
            if (system.SystemAddress == null)
                return null;

            // Reject scan if system doesn't match scan system
            if (journal.SystemAddress != null && journal.StarSystem != null && (journal.SystemAddress != system.SystemAddress || journal.StarSystem != system.Name))
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            message["StarSystem"] = system.Name;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });

            message["SystemAddress"] = system.SystemAddress;

            if (message["Materials"] != null && message["Materials"] is JArray)
            {
                foreach (JObject mmat in message["Materials"])
                {
                    mmat.Remove("Name_Localised");
                }
            }

            string bodydesig = journal.BodyDesignation ?? journal.BodyName;

            message = RemoveCommonKeys(message);

            message = message.Filter( AllowedFieldsScan);

            if (!bodydesig.StartsWith(system.Name, StringComparison.InvariantCultureIgnoreCase))  // For now test if its a different name ( a few exception for like sol system with named planets)  To catch a rare out of sync bug in historylist.
            {
                if (journal.BodyDesignation != null || System.Text.RegularExpressions.Regex.IsMatch(journal.BodyName, " [A-Z][A-Z]-[A-Z] [a-h][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return null;
                }

                message["IsUnknownBody"] = true;
                msg["$schemaRef"] = GetEDDNJournalSchemaRef(true);
            }

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        public JObject CreateEDDNMessage(JournalSAASignalsFound journal, ISystem system)
        {
            if (system.SystemAddress == null)
                return null;

            // Reject scan if system doesn't match scan system
            if (journal.SystemAddress != system.SystemAddress)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNJournalSchemaRef();

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            message["StarSystem"] = system.Name;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["SystemAddress"] = system.SystemAddress;

            if (message["Signals"] != null && message["Signals"] is JArray)
            {
                foreach (JObject sig in message["Signals"])
                {
                    sig.Remove("Type_Localised");
                }
            }

            message = RemoveCommonKeys(message);

            message = message.Filter( AllowedFieldsSAASignalsFound);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }


        public JObject CreateEDDNCommodityMessage(List<CCommodities> commodities, bool odyssey, bool horizons, string systemName, string stationName, long? marketID, DateTime time)
        {
            if (commodities == null || commodities.Count == 0)
                return null;
                
            JObject msg = new JObject();

            msg["header"] = Header();
            msg["$schemaRef"] = GetEDDNCommoditySchemaRef();

            JObject message = new JObject();

            message["systemName"] = systemName;
            message["stationName"] = stationName;
            message["marketId"] = marketID;
            message["timestamp"] = time.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

            JArray JAcommodities = new JArray();

            foreach (var commodity in commodities)
            {
                if (commodity.category.IndexOf("NonMarketable", StringComparison.InvariantCultureIgnoreCase)>=0)
                {
                    continue;
                }

                JObject jo = new JObject();

                jo["name"] = commodity.fdname;
                jo["meanPrice"] = commodity.meanPrice;
                jo["buyPrice"] = commodity.buyPrice;
                jo["stock"] = commodity.stock;
                jo["stockBracket"] = commodity.stockBracket;
                jo["sellPrice"] = commodity.sellPrice;
                jo["demand"] = commodity.demand;
                jo["demandBracket"] = commodity.demandBracket;

                if (commodity.statusFlags!=null && commodity.statusFlags.Count > 0)
                {
                    jo["statusFlags"] = new JArray(commodity.statusFlags);
                }

                JAcommodities.Add(jo);
            }

            message["commodities"] = JAcommodities;

            message["odyssey"] = odyssey;
            message["horizons"] = horizons;


            msg["message"] = message;
            return msg;
        }


        public bool PostMessage(JObject msg)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("EDDN Send");

                BaseUtils.ResponseData resp = RequestPost(msg.ToString(), "");

                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    System.Diagnostics.Debug.WriteLine("EDDN Status OK");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("EDDN Error code " + resp.StatusCode);
                    return false;
                }
            }
            catch (System.Net.WebException ex)
            {
                System.Net.WebResponse response = ex.Response;
                System.Net.HttpWebResponse httpResponse = response as System.Net.HttpWebResponse;
                string responsetext = null;

                if (response != null)
                {
                    using (var responsestream = response.GetResponseStream())
                    {
                        using (var reader = new System.IO.StreamReader(responsestream))
                        {
                            responsetext = reader.ReadToEnd();
                        }
                    }
                }

                System.Diagnostics.Trace.WriteLine($"EDDN message post failed - status: {httpResponse?.StatusCode.ToString() ?? ex.Status.ToString()}\nResponse: {responsetext}\nEDDN Message: {msg.ToString()}");
                return false;
            }
        }
    }
}
