/*
 * Copyright © 2016-2024 EDDiscovery development team
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

using QuickJSON;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EliteDangerousCore.EDDN
{
    public class EDDNClass : BaseUtils.HttpCom
    {
        static public string SoftwareName { get; set; } = "EDDiscovery";            // override if sending from another program

        private string commandername { get; set; }
        private string fromSoftwareVersion;

        private readonly string EDDNServer = "https://eddn.edcd.io:4430/upload/";
        private readonly string EDDNServerBeta = "https://beta.eddn.edcd.io:4431/upload/";

        private readonly string JournalSchema = "https://eddn.edcd.io/schemas/journal/1";
        private readonly string ShipyardSchema = "https://eddn.edcd.io/schemas/shipyard/2";
        private readonly string OutfittingSchema = "https://eddn.edcd.io/schemas/outfitting/2";
        private readonly string CommoditySchema = "https://eddn.edcd.io/schemas/commodity/3";
        private readonly string FSSDiscoveryScanSchema = "https://eddn.edcd.io/schemas/fssdiscoveryscan/1";
        private readonly string CodexSchema = "https://eddn.edcd.io/schemas/codexentry/1";
        private readonly string NavBeaconSchema = "https://eddn.edcd.io/schemas/navbeaconscan/1";
        private readonly string NavRouteSchema = "https://eddn.edcd.io/schemas/navroute/1";
        private readonly string ScanBarycentreSchema = "https://eddn.edcd.io/schemas/scanbarycentre/1";
        private readonly string ApproachSettlementSchema = "https://eddn.edcd.io/schemas/approachsettlement/1";
        private readonly string FSSAllBodiesFoundSchema = "https://eddn.edcd.io/schemas/fssallbodiesfound/1";
        private readonly string FSSSignalDiscoveredSchema = "https://eddn.edcd.io/schemas/fsssignaldiscovered/1";
        private readonly string FCMaterialsSchema = "https://eddn.edcd.io/schemas/fcmaterials_journal/1";
        private readonly string DockingDenied = "https://eddn.edcd.io/schemas/dockingdenied/1";
        private readonly string DockingGranted = "https://eddn.edcd.io/schemas/dockinggranted/1";
        private readonly string FSSBodySignals = "https://eddn.edcd.io/schemas/fssbodysignals/1";

        public EDDNClass(string commandernamep)
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];
            commandername = commandernamep;
        }

        private JObject Header(string gameversion, string build)
        {
            JObject header = new JObject();
            header["uploaderID"] = commandername;
            header["softwareName"] = SoftwareName;
            header["softwareVersion"] = fromSoftwareVersion;
            header["gameversion"] = gameversion ?? "";
            header["gamebuild"] = build ?? ""; 
            return header;
        }

        static public bool IsEDDNMessage( JournalTypeEnum EntryType)
        {
            return (        // in order in EDDSync.cs
                 EntryType == JournalTypeEnum.FSDJump ||
                 EntryType == JournalTypeEnum.Location ||
                 EntryType == JournalTypeEnum.CarrierJump ||
                 EntryType == JournalTypeEnum.Docked ||
                 EntryType == JournalTypeEnum.Scan ||
                 EntryType == JournalTypeEnum.SAASignalsFound ||
                 EntryType == JournalTypeEnum.Outfitting ||
                 EntryType == JournalTypeEnum.Shipyard ||
                 EntryType == JournalTypeEnum.Market ||
                 EntryType == JournalTypeEnum.EDDCommodityPrices ||
                 EntryType == JournalTypeEnum.FSSDiscoveryScan ||
                 EntryType == JournalTypeEnum.CodexEntry ||
                 EntryType == JournalTypeEnum.NavBeaconScan ||
                 EntryType == JournalTypeEnum.NavRoute ||
                 EntryType == JournalTypeEnum.ScanBaryCentre ||
                 EntryType == JournalTypeEnum.ApproachSettlement ||
                 EntryType == JournalTypeEnum.FSSAllBodiesFound ||
                 EntryType == JournalTypeEnum.FSSSignalDiscovered ||
                 EntryType == JournalTypeEnum.FCMaterials ||
                 EntryType == JournalTypeEnum.DockingDenied ||
                 EntryType == JournalTypeEnum.DockingGranted ||
                 EntryType == JournalTypeEnum.FSSBodySignals
               );
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
            ["ControllingPower"] = true,
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
            },
            ["ThargoidWar"] = new JObject       // update 15
            {
                ["CurrentState"] = true,
                ["NextStateSuccess"] = true,
                ["NextStateFailure"] = true,
                ["SuccessStateReached"] = true,
                ["WarProgress"] = true,
                ["RemainingPorts"] = true,
                ["EstimatedRemainingTime"] = true,
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
            ["OnFoot"] = true,
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
            // synced in journal scan order 22/5/22
            // Common
            ["ScanType"] = true,
            ["BodyName"] = true,
            ["BodyID"] = true,
            // not starsystem
            // not systemaddress
            ["DistanceFromArrivalLS"] = true,
            ["WasDiscovered"] = true,
            ["WasMapped"] = true,
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
 
            ["RotationPeriod"] = true,
            ["SurfaceTemperature"] = true,
            ["Radius"] = true,
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

            ["StarType"] = true,

            ["StellarMass"] = true,         // star only
            ["AbsoluteMagnitude"] = true,
            ["Luminosity"] = true,
            ["Subclass"] = true,
            ["Age_MY"] = true,

            ["PlanetClass"] = true,

            ["SemiMajorAxis"] = true,

            ["Eccentricity"] = true,            // has semi major axis
            ["OrbitalInclination"] = true,
            ["Periapsis"] = true,
            ["MeanAnomaly"] = true,
            ["AscendingNode"] = true,
            ["OrbitalPeriod"] = true,
            ["AxialTilt"] = true,
            ["TidalLock"] = true,

            ["TerraformState"] = true,          // planet
            ["AtmosphereComposition"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Percent"] = true
                }
            },
            ["Atmosphere"] = true,
            ["AtmosphereType"] = true,
            ["Composition"] = new JObject
            {
                ["Rock"] = true,
                ["Metal"] = true,
                ["Ice"] = true
            },
            ["Volcanism"] = true,
            ["SurfaceGravity"] = true,
            ["SurfacePressure"] = true,
            ["Landable"] = true,
            ["MassEM"] = true,
            ["Materials"] = new JArray
            {
                new JObject
                {
                    ["Name"] = true,
                    ["Percent"] = true
                }
            },
            ["ReserveLevel"] = true
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
            },
            ["Genus"] = new JArray
            {
                new JObject
                {
                    ["Genus"] = true,
                }
            }
        };

        // Assumption: there are no cycling references
        private void RemoveCommonKeys(JToken jToken)
        {
            if (jToken is JObject jObject)
            {
                foreach (var key in jObject.PropertyNames())
                {
                    if (key.EndsWith("_Localised") || key.StartsWith("EDD"))
                    {
                        jObject.Remove(key);
                    }
                    else
                    {
                        RemoveCommonKeys(jObject[key]);
                    }
                }
            }
            else if (jToken is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    RemoveCommonKeys(item);
                }
            }
        }

        private void RemoveFactionReputation(JObject obj)
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
        }

        private void RemoveStationEconomyKeys(JObject jo)
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
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNMessage(JournalFSDJump journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();
            
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["FuelUsed"].IsNull() || message["SystemAddress"] == null)  // Old ED 2.1 messages has no Fuel used fields
                return null;

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            RemoveCommonKeys(message);
            RemoveFactionReputation(message);
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

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNMessage(JournalLocation journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            RemoveCommonKeys(message);
            RemoveFactionReputation(message);
            RemoveStationEconomyKeys(message);
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

        // Create EDDN message for journal from JSON Cloned
        public JObject CreateEDDNMessage(JournalCarrierJump journal)
        {
            if (!journal.HasCoordinate || journal.StarPosFromEDSM || journal.SystemAddress == null)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            if (message["StarPosFromEDSM"] != null)  // Reject systems recently updated with EDSM coords
                return null;

            RemoveCommonKeys(message);
            RemoveFactionReputation(message);
            RemoveStationEconomyKeys(message);
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

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNMessage(JournalDocked journal, ISystem system)
        {
            if (!String.Equals(system.Name, journal.StarSystem, StringComparison.InvariantCultureIgnoreCase))
                return null;

            if (system.SystemAddress == null || !system.HasCoordinate)  // don't have a valid system..
                return null;

            if (journal.SystemAddress == null || system.SystemAddress != journal.SystemAddress )    // can't agree where we are from the journal 
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
            {
                return null;
            }

            RemoveCommonKeys(message);
            RemoveStationEconomyKeys(message);
            message.Remove("CockpitBreach");
            message.Remove("Wanted");
            message.Remove("ActiveFine");

            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });

            message = message.Filter(AllowedFieldsDocked);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from Journal fields
        public JObject CreateEDDNOutfittingMessage(JournalOutfitting journal)
        {
            if (journal.YardInfo.Items == null || journal.YardInfo.Items.Length == 0)   // not allowed to send empty lists jan 21
                return null;

            JObject msg = new JObject();

            // header matches Athan wishes for capi sourced data

            bool capi = !journal.IsJournalSourced;
            bool legacy = EDCommander.IsLegacyCommander(journal.CommanderId);
            msg["header"] = Header(capi ? (legacy ? "CAPI-Legacy-shipyard" : "CAPI-Live-shipyard") : journal.GameVersion, capi ? "" : journal.Build);

            msg["$schemaRef"] = OutfittingSchema;

            JObject message = new JObject
            {
                ["timestamp"] = journal.EventTimeUTC.ToStringZuluInvariant(),
                ["systemName"] = journal.YardInfo.StarSystem,
                ["stationName"] = journal.YardInfo.StationName,
                ["marketId"] = journal.MarketID,
                ["modules"] = new JArray(journal.YardInfo.Items
                                .Where(m => m.FDName.StartsWith("Hpt_", StringComparison.InvariantCultureIgnoreCase) || m.FDName.StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase)
                                        || m.FDName.Contains("_armour_", StringComparison.InvariantCultureIgnoreCase))      // Use FDName here note
                                .Select(m => JournalFieldNaming.NormaliseFDItemName(m.FDName)))
            };

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from Journal fields
        public JObject CreateEDDNShipyardMessage(JournalShipyard journal)
        {
            if (journal.Yard.Ships == null || journal.Yard.Ships.Length == 0) // not allowed to send empty lists jan 21
                return null;

            JObject msg = new JObject();

            // header matches Athan wishes for capi sourced data

            bool capi = !journal.IsJournalSourced;
            bool legacy = EDCommander.IsLegacyCommander(journal.CommanderId);
            msg["header"] = Header(capi ? (legacy ? "CAPI-Legacy-shipyard" : "CAPI-Live-shipyard") : journal.GameVersion, capi ? "" : journal.Build);

            msg["$schemaRef"] = ShipyardSchema;

            JObject message = new JObject
            {
                ["timestamp"] = journal.EventTimeUTC.ToStringZuluInvariant(),
                ["systemName"] = journal.Yard.StarSystem,
                ["stationName"] = journal.Yard.StationName,
                ["marketId"] = journal.MarketID,
                ["ships"] = new JArray(journal.Yard.Ships.Select(m => m.ShipType))      // ship type if FDName
            };

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNMessage(JournalScan journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            // Reject scan if system doesn't match scan system
            if (journal.SystemAddress != null && journal.StarSystem != null && (journal.SystemAddress != system.SystemAddress || journal.StarSystem != system.Name))
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

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

            RemoveCommonKeys(message);

            message = message.Filter( AllowedFieldsScan);

            // For now test if its a different name ( a few exception for like sol system with named planets)  To catch a rare out of sync bug in historylist.

            if (!bodydesig.StartsWith(system.Name, StringComparison.InvariantCultureIgnoreCase))  
            {
                System.Diagnostics.Debug.WriteLine($"Reject scan send to EDDN due to DB ${bodydesig} not starting with {system.Name}");
                // previously, it was either rejected or sent thru the test EDDN point, now just reject it
                return null;

                //if (journal.BodyDesignation != null || System.Text.RegularExpressions.Regex.IsMatch(journal.BodyName, " [A-Z][A-Z]-[A-Z] [a-h][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                //{
                    //return null;
                //}
                //message["IsUnknownBody"] = true;
                //msg["$schemaRef"] = GetEDDNJournalSchemaRef(true);
            }

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNMessage(JournalSAASignalsFound journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            // Reject scan if system doesn't match scan system
            if (journal.SystemAddress != system.SystemAddress)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = JournalSchema;

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

            RemoveCommonKeys(message);

            message = message.Filter( AllowedFieldsSAASignalsFound);

            message["odyssey"] = journal.IsOdyssey;     // new may 21
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNFSSDiscoveryScan(JournalFSSDiscoveryScan journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = FSSDiscoveryScanSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            // verified against EDDN logs 25/1/22
            message.Remove("Progress");
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["SystemAddress"] = system.SystemAddress;
            message["SystemName"] = system.Name;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNNavBeaconScan( JournalNavBeaconScan journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = NavBeaconSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["SystemAddress"] = system.SystemAddress;
            message["StarSystem"] = system.Name;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNCodexEntry(JournalCodexEntry journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = CodexSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            RemoveCommonKeys(message);          // remove _localised
            message.Remove("IsNewEntry");               
            message.Remove("NewTraitsDiscovered");  // .md ordes
            if (!message["NearestDestination"].Str().HasChars())
                message.Remove("NearestDestination");

            // verified against EDDN logs 25/1/22
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["SystemAddress"] = system.SystemAddress;
            message["System"] = system.Name;

            JObject orgmsg = journal.GetJson();
            if (orgmsg["EDDBodyID"].Int(-1) != -1)
                message["BodyID"] = orgmsg["EDDBodyID"];
            if (orgmsg["EDDBodyName"].StrNull() != null)
                message["BodyName"] = orgmsg["EDDBodyName"];
            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNNavRoute(JournalNavRoute journal)
        {
            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = NavRouteSchema;

            JObject message = journal.GetJsonCloned();       // only has Route inside it.

            if (message == null)                   // must have something
                return null;

            RemoveCommonKeys(message);          // remove _localised

            var ja = message["Route"].Array();      // must have a valid route inside it
            if (ja == null || ja.Count == 0)
                return null;

            // already has StarSystem/SystemAddress/StarPos
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNScanBaryCentre(JournalScanBaryCentre journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = ScanBarycentreSchema;

            JObject message = journal.GetJsonCloned();      // has all fields for  

            if (message == null)                        // must have something, all the rest of the fields are valid to send
                return null;

            RemoveCommonKeys(message);          // remove _localised

            // already has StarSystem/SystemAddress
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNFSSAllBodiesFound(JournalFSSAllBodiesFound journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = FSSAllBodiesFoundSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            RemoveCommonKeys(message);          // remove _localised

            // Already has SystemName/SystemAddress
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from JSON Cloned
        public JObject CreateEDDNApproachSettlement(JournalApproachSettlement journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || system.SystemAddress != journal.SystemAddress)
                return null;

            // sometimes these are missing, so ignore these. 
            // system may not be set if in EDDLite if we carrier jumped in prev log, then we got approach before Location on second log
            if (journal.Latitude == null || journal.Longitude == null )
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = ApproachSettlementSchema;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            RemoveCommonKeys(message);          // remove _localised

            // already has SystemAddress
            message["StarSystem"] = system.Name;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            message["odyssey"] = journal.IsOdyssey;
            message["horizons"] = journal.IsHorizons;

            msg["message"] = message;
            return msg;
        }

        // Create EDDN message from discrete parameters
        // pass thru gameversion/build as its used for both journal market and capi EDDCommodityPrices

        public JObject CreateEDDNCommodityMessage(string gameversion, string build, List<CCommodities> commodities, bool odyssey, bool horizons, string systemName, 
                                string stationName, StationDefinitions.StarportTypes stationType, string carrieraccess, long? marketID, DateTime time)
        {
            if (commodities == null) // now allowed to send empty lists for FC purposes (jan 21)
                return null;

            JObject msg = new JObject();

            msg["header"] = Header(gameversion,build);
            msg["$schemaRef"] = CommoditySchema;

            JObject message = new JObject();

            message["systemName"] = systemName;
            message["stationName"] = stationName;
            if ( stationType != StationDefinitions.StarportTypes.Unknown)                    // it may be unknown
                message["stationType"] = stationType.ToString();
            if (carrieraccess.HasChars())                   // it may be null
                message["carrierDockingAccess"] = carrieraccess;
            message["marketId"] = marketID;
            message["timestamp"] = time.ToStringZuluInvariant();

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

        // Create EDDN message from journal
        public JObject CreateEDDNFSSSignalDiscovered(JournalFSSSignalDiscovered journal, ISystem system)
        {
            if (system.SystemAddress == null || !system.HasCoordinate || journal.Signals == null || journal.Signals.Count == 0 || system.SystemAddress != journal.Signals[0].SystemAddress)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion,journal.Build);
            msg["$schemaRef"] = FSSSignalDiscoveredSchema;

            JObject message = new JObject();

            message["event"] = "FSSSignalDiscovered";
            message["horizons"] = journal.IsHorizons;
            message["odyssey"] = journal.IsOdyssey;
            message["timestamp"] = journal.Signals[0].RecordedUTC.ToStringZuluInvariant();
            message["SystemAddress"] = system.SystemAddress ?? 0;
            message["StarSystem"] = system.Name;
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });

            JArray ja = new JArray();
            foreach (var sig in journal.Signals)
            {
                if (sig.SystemAddress != system.SystemAddress)          // double check we 
                {
                    System.Diagnostics.Trace.WriteLine($"EDDN FssSignalDiscovered disagree with {system.Name} {system.SystemAddress} vs signal {sig.SystemAddress}");
                    return null;
                }

                // not mission targets, or we are disagreeing with system
                if (sig.USSType == null || !sig.USSType.Contains("$USS_Type_MissionTarget") )
                {
                    JObject sj = new JObject();

                    sj["timestamp"] = sig.RecordedUTC.ToStringZuluInvariant();
                    sj["SignalName"] = sig.SignalName;
                    if (sig.SignalType.HasChars())
                        sj["SignalType"] = sig.SignalType;
                    if (sig.IsStation.HasValue)
                        sj["IsStation"] = sig.IsStation.Value;
                    if (sig.USSType.HasChars())
                        sj["USSType"] = sig.USSType;
                    if (sig.SpawningState.HasChars())
                        sj["SpawningState"] = sig.SpawningState;
                    if (sig.SpawningFaction.HasChars())
                        sj["SpawningFaction"] = sig.SpawningFaction;
                    if (sig.ThreatLevel != null)
                        sj["ThreatLevel"] = sig.ThreatLevel.Value;
                    if (sig.SpawningPower != null)
                        sj["SpawningPower"] = sig.SpawningPower;
                    if (sig.OpposingPower != null)
                        sj["OpposingPower"] = sig.OpposingPower;

                    ja.Add(sj);
                }
            }

            if (ja.Count == 0)  // nothing, all knocked out
                return null;

            message["signals"] = ja;

            msg["message"] = message;

            return msg;
        }

        // Create EDDN message from journal
        public JObject CreateEDDNFCMaterials(JournalFCMaterials journal)
        {
            if (journal.Items == null)
                return null;

            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion, journal.Build);
            msg["$schemaRef"] = FCMaterialsSchema;

            JObject message = new JObject();

            message["timestamp"] = journal.EventTimeUTC.ToStringZuluInvariant();
            message["event"] = "FCMaterials";
            message["horizons"] = journal.IsHorizons;
            message["odyssey"] = journal.IsOdyssey;
            message["MarketID"] = journal.MarketID;
            message["CarrierName"] = journal.CarrierName;
            message["CarrierID"] = journal.CarrierID;

            JArray ja = new JArray();
            foreach (var commodity in journal.Items)
            {
                JObject sj = new JObject();
                sj["id"] = commodity.id;
                sj["Name"] = commodity.fdname_unnormalised;
                sj["Price"] = commodity.buyPrice;
                sj["Stock"] = commodity.stock;
                sj["Demand"] = commodity.demand;
                ja.Add(sj);
            }

            message["Items"] = ja;

            msg["message"] = message;

            return msg;
        }


        // Create EDDN message from journal
        public JObject CreateEDDNDockingDenied(JournalDockingDenied journal)
        {
            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion, journal.Build);
            msg["$schemaRef"] = DockingDenied;

            JObject message = new JObject();

            message["timestamp"] = journal.EventTimeUTC.ToStringZuluInvariant();
            message["event"] = "DockingDenied";
            message["horizons"] = journal.IsHorizons;
            message["odyssey"] = journal.IsOdyssey;
            message["MarketID"] = journal.MarketID;
            message["StationName"] = journal.StationName;
            message["StationType"] = journal.FDStationType.ToString();
            message["Reason"] = journal.FDReason;

            msg["message"] = message;

            return msg;
        }

        // Create EDDN message from journal
        public JObject CreateEDDNDockingGranted(JournalDockingGranted journal)
        {
            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion, journal.Build);
            msg["$schemaRef"] = DockingGranted;

            JObject message = new JObject();

            message["timestamp"] = journal.EventTimeUTC.ToStringZuluInvariant();
            message["event"] = "DockingGranted";
            message["horizons"] = journal.IsHorizons;
            message["odyssey"] = journal.IsOdyssey;
            message["MarketID"] = journal.MarketID;
            message["StationName"] = journal.StationName;
            message["StationType"] = journal.FDStationType.ToString();
            message["LandingPad"] = journal.LandingPad;

            msg["message"] = message;

            return msg;
        }

        public JObject CreateEDDNFSSBodySignals(JournalFSSBodySignals journal, ISystem system)
        {
            JObject msg = new JObject();
            msg["header"] = Header(journal.GameVersion, journal.Build);
            msg["$schemaRef"] = FSSBodySignals;

            JObject message = journal.GetJsonCloned();

            if (message == null)
                return null;

            if (message["SystemAddress"].Long() != system.SystemAddress)        // double check not being 'frontiered'
                return null;

            message["horizons"] = journal.IsHorizons;
            message["odyssey"] = journal.IsOdyssey;
            message["StarSystem"] = system.Name;
            // it has SystemAddress
            message["StarPos"] = new JArray(new float[] { (float)system.X, (float)system.Y, (float)system.Z });
            // it has Signals, BodyName, BodyID

            msg["message"] = message;

            return msg;
        }

        public bool PostMessage(JObject msg, bool betaserver, bool testschema)
        {
            try
            {
                ServerAddress = betaserver ? EDDNServerBeta : EDDNServer;

                if (testschema)
                    msg["$schemaRef"] = msg["$schemaRef"].Str() + "/test";

                System.Diagnostics.Debug.WriteLine($"EDDN Send to {ServerAddress} {msg.ToString()}");

                BaseUtils.HttpCom.Response resp = RequestPost(msg.ToString(), "");

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
