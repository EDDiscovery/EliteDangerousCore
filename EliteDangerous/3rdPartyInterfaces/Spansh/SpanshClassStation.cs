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
using System.Linq;
using static EliteDangerousCore.StationDefinitions;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        #region Dump get

        // always return a list
        public System.Threading.Tasks.Task<List<StationInfo>> GetStationsByDumpAsync(ISystem sys)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return GetStationsByDump(sys);
            });
        }

        // always return a list
        public List<StationInfo> GetStationsByDump(ISystem sys)
        {
            sys = EnsureSystemAddressAndName(sys);

            if (sys == null)
                return null;

            BaseUtils.HttpCom.Response response = RequestGet("dump/" + sys.SystemAddress.ToStringInvariant());

            if (response.Error)
                return null;

            var data = response.Body;
            var spanshdump = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            return GetStationsFromDump(spanshdump);
        }


        #endregion

        #region Search

        public System.Threading.Tasks.Task<List<StationInfo>> SearchServicesAsync(string systemname, string[] fdservicenames, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return SearchServices(systemname, fdservicenames, maxradius, largepad, inclcarriers, maxresults);
            });
        }

        public List<StationInfo> SearchServices(string systemname, string[] fdservicenames, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            JArray jsrv = new JArray();
            foreach (var x in fdservicenames)
                jsrv.Add(spanshservicenames[(StationDefinitions.StationServices)Enum.Parse(typeof(StationDefinitions.StationServices), x)]);    // convert to spansh

            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["services"] = new JObject()
                    {
                        ["value"] = jsrv
                    },
                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
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
                ["reference_system"] = systemname,
                ["size"] = maxresults,
            };

            if (largepad.HasValue)
                jo["filters"]["has_large_pad"] = new JObject() { ["value"] = largepad.Value };
            if (!inclcarriers)
                jo["filters"]["type"] = new JObject() { ["value"] = new JArray(spanshstartporttypenames.Where(x => x.Key != StarportTypes.FleetCarrier).Select(x => x.Value)) }; // all but drake

            return IssueStationSearchQuery(jo);
        }

        public class SearchCommoditity
        {
            public string EnglishName { get; set; }
            public Tuple<int, int> buyprice { get; set; } = null; // set to compare buy price
            public Tuple<int, int> sellprice { get; set; } = null; // set to compare sell price
            public Tuple<int, int> supply { get; set; } = null; // set to compare supply
            public Tuple<int, int> demand { get; set; } = null; // set to compare sdemand
        }

        public System.Threading.Tasks.Task<List<StationInfo>> SearchCommoditiesAsync(string systemname, SearchCommoditity[] commodities, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return SearchCommodities(systemname, commodities, maxradius, largepad, inclcarriers, maxresults);
            });
        }

        public List<StationInfo> SearchCommodities(string systemname, SearchCommoditity[] commodities, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["market"] = new JArray(),

                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
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
                ["reference_system"] = systemname,
                ["size"] = maxresults,
            };

            JArray market = jo["filters"]["market"].Array();
            foreach( var c in commodities)
            {
                JObject o = new JObject { ["name"] = c.EnglishName };

                if (c.buyprice != null)
                    o["buy_price"] = new JObject { ["value"] = new JArray(c.buyprice.Item1.ToStringInvariant(), c.buyprice.Item2.ToStringInvariant()), ["comparison"] = "<=>" };
                if (c.sellprice != null)
                    o["sell_price"] = new JObject { ["value"] = new JArray(c.sellprice.Item1.ToStringInvariant(), c.sellprice.Item2.ToStringInvariant()), ["comparison"] = "<=>" };
                if (c.supply != null)
                    o["supply"] = new JObject { ["value"] = new JArray(c.supply.Item1.ToStringInvariant(), c.supply.Item2.ToStringInvariant()), ["comparison"] = "<=>" };
                if (c.demand != null)
                    o["demand"] = new JObject { ["value"] = new JArray(c.demand.Item1.ToStringInvariant(), c.demand.Item2.ToStringInvariant()), ["comparison"] = "<=>" };

                market.Add(o);
            }

            if (largepad.HasValue)
                jo["filters"]["has_large_pad"] = new JObject() { ["value"] = largepad.Value };
            if (!inclcarriers)
                jo["filters"]["type"] = new JObject() { ["value"] = new JArray(spanshstartporttypenames.Where(x => x.Key != StarportTypes.FleetCarrier).Select(x => x.Value)) }; // all but drake

            return IssueStationSearchQuery(jo);
        }

        public System.Threading.Tasks.Task<List<StationInfo>> SearchEconomyAsync(string systemname, string[] fdeconomynames, double maxradius, bool? largepad, int maxresults = 20)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return SearchEconomy(systemname, fdeconomynames, maxradius, largepad, maxresults);
            });
        }

        public List<StationInfo> SearchEconomy(string systemname, string[] fdeconomynames, double maxradius, bool? largepad, int maxresults = 20)
        {
            JArray jeconames = new JArray();
            foreach (var x in fdeconomynames)
                jeconames.Add(spansheconomynames[(EconomyDefinitions.Economy)Enum.Parse(typeof(EconomyDefinitions.Economy), x)]);       // convert to spansh

            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["primary_economy"] = new JObject()
                    {
                        ["value"] = jeconames,
                    },

                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
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
                ["reference_system"] = systemname,
                ["size"] = maxresults,
            };

            if (largepad.HasValue)
                jo["filters"]["has_large_pad"] = new JObject() { ["value"] = largepad.Value };

            return IssueStationSearchQuery(jo);
        }

        // classlist = [0] = All, [1] = class 0 .. [7] = class 6 [8] = class 7 [9] = class 8
        // ratingslist = [0] = All [1] .. A [7] = G
        // module names are per ModTypeString in ShipModule
        public System.Threading.Tasks.Task<List<StationInfo>> SearchOutfittingAsync(string systemname, string[] spanshmoduletypenames, bool[] classlist, bool[] ratinglist, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return SearchOutfitting(systemname, spanshmoduletypenames, classlist, ratinglist, maxradius, largepad, inclcarriers, maxresults);
            });
        }


        public List<StationInfo> SearchOutfitting(string systemname, string[] spanshmoduletypenames, bool[] classlist, bool[] ratinglist, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            System.Diagnostics.Debug.Assert(classlist.Length == 10);
            System.Diagnostics.Debug.Assert(ratinglist.Length == 8);
            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["modules"] = new JObject()
                    {
                        ["name"] = new JArray(spanshmoduletypenames),
                    },

                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
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
                ["reference_system"] = systemname,
                ["size"] = maxresults,
            };

            if (classlist[0] == false)  // if all, don't include it, otherwise for some modules spansh won't return anything (Mil composities for instance)
            {
                JArray classarray = new JArray();
                for (int i = 1; i < classlist.Length; i++)
                {
                    if (classlist[i])
                        classarray.Add((i - 1).ToStringInvariant());
                }
                jo["filters"]["modules"]["class"] = classarray;
            }

            if (ratinglist[0] == false)
            {
                JArray ratingarray = new JArray();
                for (int i = 1; i < ratinglist.Length; i++)
                {
                    if (ratinglist[i])
                        ratingarray.Add(new string((char)('A' + i - 1), 1));
                }

                jo["filters"]["modules"]["rating"] = ratingarray;
            }

            if (largepad.HasValue)
                jo["filters"]["has_large_pad"] = new JObject() { ["value"] = largepad.Value };
            if (!inclcarriers)
                jo["filters"]["type"] = new JObject() { ["value"] = new JArray(spanshstartporttypenames.Where(x => x.Key != StarportTypes.FleetCarrier).Select(x => x.Value)) }; // all but drake

            return IssueStationSearchQuery(jo);
        }

        // shipnames are EDCD ship name from ShipPropID
        public System.Threading.Tasks.Task<List<StationInfo>> SearchShipsAsync(string systemname, string[] englishshipnames, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return SearchShips(systemname, englishshipnames, maxradius, largepad, inclcarriers, maxresults);
            });
        }

        public List<StationInfo> SearchShips(string systemname, string[] englishshipnames, double maxradius, bool? largepad, bool inclcarriers, int maxresults = 20)
        {
            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["ships"] = new JObject()
                    {
                        ["value"] = new JArray(englishshipnames),
                    },
                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
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
                ["reference_system"] = systemname,
                ["size"] = maxresults,
            };

            if (largepad.HasValue)
                jo["filters"]["has_large_pad"] = new JObject() { ["value"] = largepad.Value };
            if (!inclcarriers)
                jo["filters"]["type"] = new JObject() { ["value"] = new JArray(spanshstartporttypenames.Where(x => x.Key != StarportTypes.FleetCarrier).Select(x => x.Value)) }; // all but drake

            return IssueStationSearchQuery(jo);
        }

        private List<StationInfo> IssueStationSearchQuery(JObject query)
        {
            System.Diagnostics.Debug.WriteLine($"Spansh post data for station search {query.ToString(true)}");

            var response = RequestPost(query.ToString(), "stations/search");

            if (response.Error)
                return null;

            var data = response.Body;

            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

          //  System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");
            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshstationsearch.txt", json?.ToString(true));

            if (json != null && json["results"] != null)
            {
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

                            station.DataSource = SystemSource.FromSpansh;

                            station.Faction = evt["controlling_minor_faction"].StrNull();

                            station.DistanceRefSystem = evt["distance"].Double();      // system info at base of object
                            station.System = new SystemClass(evt["system_name"].Str(), evt["system_id64"].LongNull(), evt["system_x"].Double(), evt["system_y"].Double(), evt["system_z"].Double(), SystemSource.FromSpansh);

                            station.BodyName = evt["body_name"].StrNull();
                            station.BodyType = BodyDefinitions.GetBodyType(evt["body_type"].Str());
                            station.BodySubType = evt["body_subtype"].StrNull();
                            station.DistanceToArrival = evt["distance_to_arrival"].Double();

                            station.IsPlanetary = evt["is_planetary"].Bool();

                            station.StationName = evt["name"].StrNull();
                            station.StationType = evt["type"].StrNull();

                            station.Latitude = evt["latitude"].DoubleNull();
                            station.Longitude = evt["longitude"].DoubleNull();

                            if (station.StationType == null)
                                station.StationType = station.Latitude.HasValue ? "Settlement" : "Unknown"; // must be getting nulls

                            station.FDStationType = StarportTypeNameToEnum(station.StationType);        // set type

                            station.StationState = StarportState.None; // don't know of a SPANSH return for this

                            station.PowerplayState = PowerPlayDefinitions.ToEnum(evt["power_state"].StrNull());     // this is in here, but not in dump!

                            station.StarSystem = station.System.Name;
                            station.SystemAddress = station.System.SystemAddress;
                            station.Allegiance = AllegianceDefinitions.ToEnum( evt["allegiance"].Str("Unknown"));   // unknown if not there

                            station.Economy_Localised = evt["primary_economy"].StrNull();
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

                            station.Government_Localised = evt["government"].StrNull();
                            if (station.Government_Localised != null)
                                station.Government = GovernmentNameToEnum(station.Government_Localised);

                            var ss = evt["services"].Array();
                            if (ss != null)
                            {
                                station.StationServices = new StationDefinitions.StationServices[ss.Count];
                                int i = 0;
                                foreach (JObject sso in ss)
                                    station.StationServices[i++] = StationServiceNameToEnum( sso["name"].Str() );
                            }

                            if (evt.Contains("large_pads") || evt.Contains("medium_pads") || evt.Contains("small_pads"))     // at least one
                                station.LandingPads = new JournalDocked.LandingPadList() { Large = evt["large_pads"].Int(), Medium = evt["medium_pads"].Int(), Small = evt["small_pads"].Int() };

                            // prohibited commodities

                            station.MarketID = evt["market_id"].LongNull();

                            JArray market = evt["market"].Array();
                            if (market != null)
                            {
                                station.Market = new List<CCommodities>();
                                foreach (JObject commd in market)
                                {
                                    CCommodities cc = new CCommodities();
                                    if (cc.FromJsonSpansh(commd, false))
                                    {
                                        station.Market.Add(cc);
                                    }
                                }

                                station.MarketUpdateUTC = evt["market_updated_at"].DateTimeUTC();
                            }

                            station.HasMarket = evt["has_market"].Bool() || market != null;     // note if no services structure, as of 23/11/23, it won't print has_market (same for the other two).

                            JArray outfitting = evt["modules"].Array();
                            if (outfitting != null)
                            {
                                station.Outfitting = new List<Outfitting.OutfittingItem>();
                                foreach (JObject of in outfitting)
                                {
                                    var oi = new Outfitting.OutfittingItem { Name = of["ed_symbol"].Str().ToLowerInvariant(), BuyPrice = of["price"].Long() };
                                    oi.Normalise();
                                    station.Outfitting.Add(oi);
                                }

                                // sorted by English module type and english name
                                station.Outfitting.Sort(delegate (Outfitting.OutfittingItem left, Outfitting.OutfittingItem right) { int v = left.EnglishModTypeString.CompareTo(right.EnglishModTypeString); return v != 0 ? v : left.Name.CompareTo(right.Name); });
                                
                                station.OutfittingUpdateUTC = evt["outfitting_updated_at"].DateTimeUTC();
                            }

                            station.HasOutfitting = evt["has_outfitting"].Bool() || outfitting != null;

                            JArray shipyard = evt["ships"].Array();
                            if (shipyard != null)
                            {
                                station.Shipyard = new List<ShipYard.ShipyardItem>();
                                foreach (JObject ship in shipyard)
                                {
                                    string shipname = ship["name"].Str();
                                    string fdname = ItemData.ReverseShipLookup(shipname);
                                    if (fdname != null)
                                    {
                                        var si = new ShipYard.ShipyardItem { ShipType = fdname, ShipPrice = ship["price"].Long() };
                                        si.Normalise();
                                        station.Shipyard.Add(si);
                                    }
                                }

                                station.Shipyard.Sort(delegate (ShipYard.ShipyardItem left, ShipYard.ShipyardItem right) { int v = left.FriendlyShipType.CompareTo(right.FriendlyShipType); return v; });
                                station.ShipyardUpdateUTC = evt["shipyard_updated_at"].DateTimeUTC();
                            }

                            station.HasShipyard = evt["has_shipyard"].Bool() || shipyard != null;

                        //    station.FillInformation(out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"{info}\r\n{detailed}\r\n\r\n");

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

        #endregion
    }
}

