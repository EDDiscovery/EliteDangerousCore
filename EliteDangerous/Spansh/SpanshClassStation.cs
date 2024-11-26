/*
 * Copyright © 2023-2024 EDDiscovery development team
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
using static EliteDangerousCore.EconomyDefinitions;
using static EliteDangerousCore.GovernmentDefinitions;
using static EliteDangerousCore.StationDefinitions;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        #region Dump get

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

            BaseUtils.HttpCom.Response response = RequestGet("dump/" + sys.SystemAddress.ToStringInvariant());

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            JObject jsystem = json?["system"].Object();

            if (jsystem == null)
                return null;

            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshstationdump.txt", json?.ToString(true));

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
                        var si = ReadStationInfoFromDump(evt.Object(), sys, bodyname, bodytype, bodysubtype);
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
                        var si = ReadStationInfoFromDump(evt.Object(), sys, "","","");
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
        private StationInfo ReadStationInfoFromDump(JObject evt, ISystem sys, string bodyname, string bodytype, string bodysubtype)
        {
            DateTime updatedat = evt["updateTime"].DateTimeUTC();
            StationInfo station = new StationInfo(updatedat);

            station.System = sys;

            station.StationName = evt["name"].StrNull();
            station.StationType = evt["type"].StrNull();

            station.Latitude = evt["latitude"].DoubleNull();
            station.Longitude = evt["longitude"].DoubleNull();

            if (station.StationType == null)
                station.StationType = station.Latitude.HasValue ? "Settlement" : "Unknown"; // must be getting nulls

            station.FDStationType = StarportTypeNameToEnum(station.StationType);        // set type

            station.IsFleetCarrier = station.StationType.Contains("Carrier");

            station.Allegiance = AllegianceDefinitions.ToEnum( evt["allegiance"].StrNull() );   //22/4/24

            station.Faction = evt["controllingFaction"].StrNull();

            station.StationState = StarportState.None; // don't know of a SPANSH return for this

            station.PowerplayState = PowerPlayDefinitions.State.Unknown;    // does not report this in dump

            // faction state

            station.DistanceToArrival = evt["distanceToArrival"].Double();

            station.Economy_Localised = evt["primaryEconomy"].StrNull();        // 22/4/24
            if ( station.Economy_Localised!=null)
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
                    var si = new ShipYard.ShipyardItem { id = ship["shipId"].Long(), ShipType = ship["symbol"].Str().ToLowerInvariant(), FriendlyShipType = ship["name"].Str()  };
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
            station.BodyType = bodytype;
            station.BodySubType = bodysubtype;

            station.IsPlanetary = station.BodyType == "Planet";

            station.StarSystem = station.System.Name;
            station.SystemAddress = station.System.SystemAddress;

            //debug            
            //station.FillInformation(out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"Station info {info}\r\n{detailed}\r\n\r\n");

            return station;
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
            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshstationsearch.txt", json?.ToString(true));

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

                            station.Faction = evt["controlling_minor_faction"].StrNull();

                            station.DistanceRefSystem = evt["distance"].Double();      // system info at base of object
                            station.System = new SystemClass(evt["system_name"].Str(), evt["system_id64"].LongNull(), evt["system_x"].Double(), evt["system_y"].Double(), evt["system_z"].Double(), SystemSource.FromSpansh);

                            station.BodyName = evt["body_name"].StrNull();
                            station.BodyType = evt["body_type"].StrNull();
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

        #region Translate

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

//            [Government.Imperial] = "Imperial",       // not in spansh

            [Government.Unknown] = "Unknown",      // addition to allow Unknown to be mapped
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



        #endregion

    }
}

