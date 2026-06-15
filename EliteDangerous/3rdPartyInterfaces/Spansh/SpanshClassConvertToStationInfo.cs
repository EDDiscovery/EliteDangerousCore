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

using EliteDangerousCore.JournalEvents;
using QuickJSON;
using System;
using System.Collections.Generic;
using static EliteDangerousCore.StationDefinitions;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        // read spansh evt for station info, which is JournalDocked plus, at bodyname.
        // May return null if it does not like it
        private static StationInfo ConvertToStationInfo(JToken evt, ISystem sys, string bodyname = null, int? bodyid = null, string bodytype = null, string bodysubtype = null)
        {
            try
            {
                DateTime updatedat = evt["updateTime"].DateTimeUTC();
                StationInfo station = new StationInfo(updatedat, SystemSource.FromSpansh);

                station.System = sys;

                station.StationName = evt["name"].StrNull();
                station.StationType = evt["type"].StrNull();

                station.Latitude = evt["latitude"].DoubleNull();
                station.Longitude = evt["longitude"].DoubleNull();

                if (station.StationType == null)
                    station.StationType = station.Latitude.HasValue ? "Settlement" : "Unknown"; // must be getting nulls

                station.FDStationType = StarportTypeNameToEnum(station.StationType);        // set type

                station.IsFleetCarrier = station.StationType.Contains("Carrier");

                station.Allegiance = AllegianceDefinitions.ToEnum(evt["allegiance"].StrNull());   //22/4/24

                station.Faction = evt["controllingFaction"].StrNull();

                station.StationState = StarportState.None; // don't know of a SPANSH return for this

                station.PowerplayState = PowerPlayDefinitions.State.Unknown;    // does not report this in dump

                // faction state

                station.DistanceToArrival = evt["distanceToArrival"].Double();

                station.Economy_Localised = evt["primaryEconomy"].StrNull();        // 22/4/24
                if (station.Economy_Localised != null)
                    station.Economy = EconomyNameToEnum(station.Economy_Localised);

                JObject eco = evt["economies"].Object();
                if (eco != null)
                {
                    station.EconomyList = new EconomyDefinitions.Economies[eco.Count];
                    int i = 0;
                    foreach (var e in eco)
                    {
                        var ec = EconomyNameToEnum(e.Key);
                        station.EconomyList[i++] = new EconomyDefinitions.Economies { Name = ec, Name_Localised = e.Key, Proportion = e.Value.Double(0) / 100.0 };
                    }
                }

                station.Government_Localised = evt["government"].StrNull();     // tested 22/4/24
                if (station.Government_Localised != null)
                    station.Government = GovernmentNameToEnum(station.Government_Localised);

                station.MarketID = evt["id"].LongNull();

                var ss = evt["services"].Array();
                if (ss != null)
                {
                    station.StationServices = new StationDefinitions.StationServices[ss.Count];
                    int i = 0;
                    foreach (JToken name in ss)
                        station.StationServices[i++] = StationServiceNameToEnum(name.Str("Failed"));
                }

                if (evt.Contains("landingPads"))
                {
                    station.LandingPads = new JournalDocked.LandingPadList()
                    {
                        Large = evt["landingPads"].I("large").Int(),
                        Medium = evt["landingPads"].I("medium").Int(),
                        Small = evt["landingPads"].I("small").Int()
                    };
                }

                JArray market = evt["market"].I("commodities").Array();
                if (market != null)
                {
                    station.Market = new List<CCommodities>();
                    foreach (JObject commd in market)
                    {
                        CCommodities cc = new CCommodities();
                        if (cc.FromJsonSpansh(commd, true))
                        {
                            station.Market.Add(cc);
                        }
                    }

                    station.Market.Sort(delegate (CCommodities left, CCommodities right) { int v = left.category.CompareTo(right.category); return v != 0 ? v : left.locName.CompareTo(right.locName); });

                    station.MarketUpdateUTC = evt["market"].I("updateTime").DateTimeUTC();
                }

                // if we have a market field or we have station services of market mark it as good (same as spansh)
                station.HasMarket = station.Market != null || (station.StationServices != null && Array.IndexOf(station.StationServices, "Market") >= 0);

                JArray outfitting = evt["outfitting"].I("modules").Array();
                if (outfitting != null)
                {
                    station.Outfitting = new List<Outfitting.OutfittingItem>();
                    foreach (JObject of in outfitting)
                    {
                        var oi = new Outfitting.OutfittingItem { id = of["moduleId"].Long(), Name = of["symbol"].Str(), BuyPrice = of["price"].Long() };
                        oi.Normalise();
                        station.Outfitting.Add(oi);
                    }

                    // sorted by English module type and english name
                    station.Outfitting.Sort(delegate (Outfitting.OutfittingItem left, Outfitting.OutfittingItem right) { int v = left.EnglishModTypeString.CompareTo(right.EnglishModTypeString); return v != 0 ? v : left.Name.CompareTo(right.Name); });

                    station.OutfittingUpdateUTC = evt["outfitting"].I("updateTime").DateTimeUTC();
                }

                station.HasOutfitting = station.Outfitting != null || (station.StationServices != null && Array.IndexOf(station.StationServices, "Outfitting") >= 0);

                JArray shipyard = evt["shipyard"].I("ships").Array();
                if (shipyard != null)
                {
                    station.Shipyard = new List<ShipYard.ShipyardItem>();
                    foreach (JObject ship in shipyard)
                    {
                        var si = new ShipYard.ShipyardItem { id = ship["shipId"].Long(), ShipType = ship["symbol"].Str().ToLowerInvariant(), FriendlyShipType = ship["name"].Str() };
                        si.Normalise();
                        station.Shipyard.Add(si);
                    }

                    station.Shipyard.Sort(delegate (ShipYard.ShipyardItem left, ShipYard.ShipyardItem right) { int v = left.FriendlyShipType.CompareTo(right.FriendlyShipType); return v; });
                    station.ShipyardUpdateUTC = evt["shipyard"].I("updateTime").DateTimeUTC();
                }

                station.HasShipyard = station.Shipyard != null || (station.StationServices != null && Array.IndexOf(station.StationServices, "Shipyard") >= 0);

                // commodities/prohibited

                station.DistanceRefSystem = 0;

                station.BodyName = bodyname;
                station.BodyID = bodyid;
                station.BodyType = BodyDefinitions.GetBodyType(bodytype);
                station.BodySubType = bodysubtype;

                station.IsPlanetary = station.BodyType == BodyDefinitions.BodyType.Planet;

                station.StarSystem = station.System.Name;
                station.SystemAddress = station.System.SystemAddress;

                //debug            
                //station.FillInformation(out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"Station info {info}\r\n{detailed}\r\n\r\n");

                return station;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Spansh read stations exception {ex}");
            }

            return null;
        }
    }
}

