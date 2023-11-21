/*
 * Copyright © 2023-2023 EDDiscovery development team
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

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public System.Threading.Tasks.Task<List<StationInfo>> GetStationsByDumpAsync(ISystem sys)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return GetStationsByDump(sys);
            });
        }


        public List<StationInfo> GetStationsByDump(ISystem sys)
        {
            sys = EnsureSystemAddress(sys);

            if (sys == null)
                return null;

            BaseUtils.ResponseData response = RequestGet("dump/" + sys.SystemAddress.ToStringInvariant(), handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            JObject jsystem = json?["system"].Object();

            if (jsystem == null)
                return null;

            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshstations.txt", json?.ToString(true));

            List<StationInfo> stationinfo = new List<StationInfo>();

            if (!sys.HasCoordinate)
            {
                sys.X = jsystem["coords"].I("x").Double();
                sys.Y = jsystem["coords"].I("y").Double();
                sys.Z = jsystem["coords"].I("z").Double();
            }

            JArray bodyarray = jsystem["bodies"].Array();

            if (bodyarray != null)
            {
                foreach (var body in bodyarray)
                {
                    string bodyname = body["name"].StrNull();
                    string bodytype = body["type"].StrNull();
                    string bodysubtype = body["subType"].StrNull();

                    foreach (var evt in body["stations"].EmptyIfNull())
                    {
                        var si = ReadStationInfo(evt.Object(), sys, bodyname, bodytype, bodysubtype);
                        if (si != null)
                        {
                            stationinfo.Add(si);
                        }
                    }
                }

                JArray stationarray = jsystem["stations"].Array();

                if (stationarray != null)
                {
                    foreach (var evt in stationarray)
                    {
                        var si = ReadStationInfo(evt.Object(), sys, "","","");
                        if (si != null)
                        {
                            stationinfo.Add(si);
                        }

                    }
                }

            }

            return stationinfo;
        }

        // read evt for station info at bodyname.  may return null if it does not like it
        public StationInfo ReadStationInfo(JObject evt, ISystem sys, string bodyname, string bodytype, string bodysubtype)
        {
            DateTime updatedat = evt["updateTime"].DateTimeUTC();
            StationInfo station = new StationInfo(updatedat);

            station.System = sys;

            station.StationName = evt["name"].StrNull();
            station.StationType = evt["type"].StrNull();

            station.Latitude = evt["latitude"].DoubleNull();
            station.Longitude = evt["longitude"].DoubleNull();

            if (station.StationType == null)
                station.StationType = station.Latitude.HasValue ? "Settlement" : "Other";

            station.IsFleetCarrier = station.StationType.Contains("Carrier");

            station.Allegiance = evt["allegiance"].StrNull();
            station.Faction = evt["controllingFaction"].StrNull();

            // faction state

            station.DistanceToArrival = evt["distanceToArrival"].Double();

            station.Economy_Localised = evt["primaryEconomy"].StrNull();
            if ( station.Economy_Localised!=null)
                station.Economy = EconomyDefinitions.ReverseLookup(station.Economy_Localised) ?? station.Economy_Localised;

            JObject eco = evt["economies"].Object();
            if (eco != null)
            {
                station.EconomyList = new JournalDocked.Economies[eco.Count];
                int i = 0;
                foreach (var e in eco)
                {
                    var ec = EconomyDefinitions.ReverseLookup(e.Key) ?? e.Key;
                    station.EconomyList[i++] = new JournalDocked.Economies { Name = ec, Name_Localised = e.Key, Proportion = e.Value.Double(0) / 100.0 };
                }
            }

            station.Government_Localised = evt["government"].StrNull();
            if (station.Government_Localised != null)
                station.Government = GovernmentDefinitions.ReverseLookup(station.Government_Localised) ?? station.Government_Localised;

            station.MarketID = evt["id"].LongNull();

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
                station.HasMarket = market.Count>0;
 
                station.MarketUpdateUTC = evt["market"].I("updateTime").DateTimeUTC();
            }

            JArray outfitting = evt["outfitting"].I("modules").Array();
            if (outfitting != null)
            {
                station.OutFitting = new List<Outfitting.OutfittingItem>();
                foreach (JObject of in outfitting)
                {
                    var oi = new Outfitting.OutfittingItem { id = of["moduleId"].Long(), Name = of["symbol"].Str(), FDName = of["symbol"].Str().ToLower() };
                    oi.Normalise();
                    station.OutFitting.Add(oi);
                }
                station.OutFitting.Sort(delegate (Outfitting.OutfittingItem left, Outfitting.OutfittingItem right) { int v = left.ModType.CompareTo(right.ModType); return v != 0 ? v : left.Name.CompareTo(right.Name); });

                station.OutfittingUpdateUTC = evt["outfitting"].I("updateTime").DateTimeUTC();
            }

            JArray shipyard = evt["shipyard"].I("ships").Array();
            if (shipyard != null)
            {
                station.Shipyard = new List<ShipYard.ShipyardItem>();
                foreach (JObject ship in shipyard)
                {
                    var si = new ShipYard.ShipyardItem { id = ship["shipId"].Long(), ShipType = ship["symbol"].Str().ToLower(), FriendlyShipType = ship["name"].Str()  };
                    si.Normalise();
                    station.Shipyard.Add(si);
                }

                station.Shipyard.Sort(delegate (ShipYard.ShipyardItem left, ShipYard.ShipyardItem right) { int v = left.FriendlyShipType.CompareTo(right.FriendlyShipType); return v; });
                station.ShipyardUpdateUTC = evt["shipyard"].I("updateTime").DateTimeUTC();
            }

            // commodities/prohibited

            station.StationServices = evt["services"]?.ToObject<string[]>();

            station.DistanceRefSystem = 0;

            station.BodyName = bodyname;
            station.BodyType = bodytype;
            station.BodySubType = bodysubtype;

            station.IsPlanetary = station.BodyType == "Planet";

            //  station.StationState = evt["power_state"].StrNull();    // not in spansh

            station.StarSystem = station.System.Name;
            station.SystemAddress = station.System.SystemAddress;

            //debug            
            //station.FillInformation(out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"Station info {info}\r\n{detailed}\r\n\r\n");

            return station;
        }

        // Search method has been abandoned for now, but kept for future.  Needs TLC if turned back on, for UTC times etc.

#if false
        public System.Threading.Tasks.Task<List<StationInfo>> GetStationsBySearchAsync(string name)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return GetStationsBySearch(name);
            });
        }

        public List<StationInfo> GetStationsBySearch(string name)
        {
            JObject query = new JObject()
            {
                ["sort"] = new JArray()
                {
                    new JObject()
                    {
                        ["distance"] = new JObject()
                        {
                            ["direction"] = "asc"
                        }
                    }
                },
                ["reference_system"] = name,
                ["size"] = MaxReturnSize, // this appears the max, oct 23
            };

            //if ( haslargepad.HasValue )
            //{
            //    query["filters"] = new JObject
            //    {
            //        ["has_large_pad"] = new JObject()
            //        {
            //            ["value"] = haslargepad.Value,
            //        }
            //    };
            //}

            return IssueStationQuery(query);
        }

        private List<StationInfo> IssueStationQuery(JObject query)
        {
            System.Diagnostics.Debug.WriteLine($"Spansh post data for station {query.ToString()}");

            var response = RequestPost(query.ToString(), "stations/search", handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            //System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");
            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshstations.txt", json?.ToString(true));

            if (json != null && json["results"] != null)
            {
                // structure tested oct 23

                try
                {
                    List<StationInfo> stationinfo = new List<StationInfo>();

                    foreach (JToken list in json["results"].Array())    // array of objects
                    {
                        JObject evt = list.Object();
                        if (evt != null)
                        {
                            DateTime updatedat = evt["updated_at"].DateTimeUTC();
                            StationInfo station = new StationInfo(updatedat);

                            station.Faction = evt["controlling_minor_faction"].StrNull();
                         
                            station.DistanceRefSystem =  evt["distance"].Double();      // system info at base of object
                            station.System = new SystemClass(evt["system_name"].Str(), evt["system_id64"].LongNull(), evt["system_x"].Double(), evt["system_y"].Double(), evt["system_z"].Double(), SystemSource.FromSpansh);

                            station.BodyName = evt["body_name"].StrNull();
                            station.BodyType = evt["body_type"].StrNull();
                            station.BodySubType = evt["body_subtype"].StrNull();
                            station.DistanceToArrival = evt["distance_to_arrival"].Double();

                            station.IsPlanetary = evt["is_planetary"].Bool();
                            station.Latitude = evt["latitude"].DoubleNull();
                            station.Longitude  = evt["longitude"].DoubleNull();

                            station.StationName = evt["name"].StrNull();
                            station.StationType = evt["type"].StrNull();
                            station.StationState = evt["power_state"].StrNull();

                            station.StarSystem = station.System.Name;
                            station.SystemAddress = station.System.SystemAddress;
                            station.MarketID = evt["market_id"].LongNull();
                            station.HasMarket = evt["has_market"].Bool();
                            station.Allegiance = evt["allegiance"].StrNull();
                            station.Economy = station.Economy_Localised = evt["primary_economy"].StrNull();
                            station.EconomyList = evt["economies"]?.ToObject<JournalDocked.Economies[]>(checkcustomattr:true);
                            foreach (var x in station.EconomyList.EmptyIfNull())
                            {
                                x.Name_Localised = x.Name;
                                x.Proportion /= 100;
                            }
                            station.Government = station.Government_Localised = evt["government"].StrNull();

                            var ss = evt["services"].Array();
                            if ( ss != null)
                            {
                                station.StationServices = new string[ss.Count];
                                int i = 0;
                                foreach (JObject sso in ss)
                                    station.StationServices[i++] = sso["name"].Str();
                            }

                            if (evt.Contains("large_pads") || evt.Contains("medium_pads") || evt.Contains("small_pads"))     // at least one
                                station.LandingPads = new JournalDocked.LandingPadList() { Large = evt["large_pads"].Int(), Medium = evt["medium_pads"].Int(), Small = evt["small_pads"].Int() };

                            // prohibited commodities

                            JArray market = evt["market"].Array();
                            if ( market != null )
                            {
                                station.Market = new List<CCommodities>();
                                foreach( JObject commd in market)
                                {
                                    CCommodities cc = new CCommodities();
                                    if ( cc.FromJsonSpansh(commd, false))
                                    {
                                        station.Market.Add(cc);
                                    }
                                }
                            }

                            // tbd outfitting
                            // tbd ships

                            //station.FillInformation(out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"Station info {info}\r\n{detailed}\r\n\r\n");
                            stationinfo.Add(station);
                        }
                    }

                    return stationinfo;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Spansh Station Query failed due to " + ex);
                }
            }

            return null;
        }
        #endif
    }
}

