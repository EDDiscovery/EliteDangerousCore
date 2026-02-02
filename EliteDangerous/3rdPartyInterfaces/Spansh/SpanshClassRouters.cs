/*
 * Copyright 2023-2025 EDDiscovery development team
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

using BaseUtils;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        /// <summary>
        /// Return spansh job id
        /// </summary>
        public string RequestRoadToRichesAmmoniaEarthlikes(string from, string to, double jumprange, int radius,
                                            int maxsystems, bool avoidthargoids,
                                            bool loop, int maxlstoarrival,
                                            int minscanvalue,
                                            bool? usemappingvalue = null,
                                            string[] bodytypes = null)
        {
            if (loop)               // don't give to if loop
                to = null;

            string query = HTTPExtensions.MakeQuery("radius", radius,
                           "range", jumprange,
                           "from", from,
                           "to", to,
                           "max_results", maxsystems,
                           "max_distance", maxlstoarrival,
                           "min_value", minscanvalue,
                           "use_mapping_value", usemappingvalue,
                           "avoid_thargoids", avoidthargoids,
                           "loop", loop,
                           "body_types", bodytypes);

            return RequestJob("riches/route", query);
        }

        // string = error string, or null if no error.
        // on success, string is null and Jtoken != null
        public Tuple<string, List<ISystem>> TryGetRoadToRichesAmmonia(string jobname)
        {
            var res = TryGetResponseToJob(jobname);
            if (res.Item1 == null && res.Item2 != null)
                return new Tuple<string, List<ISystem>>(null, DecodeSystemsReturn(res.Item2));
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }


        public string RequestTradeRouter( string system, string station,
                                            int max_hops, 
                                            double max_hop_distance,
                                            long starting_capital,
                                            int max_cargo,
                                            int max_system_distance,
                                            int max_price_age,
                                            bool requires_large_pad, bool allow_planetary, bool allow_player_owned, bool allow_restricted_access,
                                            bool allow_prohibited, bool unique, bool permit)
        {
            // Checked dec 23 2025
            // max_hops=5&max_hop_distance=50&system=Col+285+Sector+OJ-Q+d5-88&station=Solanas+Enterprise&starting_capital=1000&max_cargo=7
            // &max_system_distance=10000000&max_price_age=483209
            // &requires_large_pad=1&allow_prohibited=1&allow_planetary=1&allow_player_owned=1&allow_restricted_access=1&unique=1&permit=1
            // Query is `max_hops=5&max_hop_distance=25&system=Sol&station=Abimbola+Metallurgic+Reserve&starting_capital=1000&max_cargo=7
            // &max_system_distance=1000000&max_price_age=2592000
            // &requires_large_pad=1&allow_prohibited=1&allow_planetary=1&allow_player_owned=1&allow_restricted_access=1&unique=1&permit=1`

            string query = HTTPExtensions.MakeQuery(        // name and order as per spansh query
                           nameof(max_hops), max_hops, 
                           nameof(max_hop_distance), max_hop_distance,
                           nameof(system), system,                          
                           nameof(station), station,
                           nameof(starting_capital), starting_capital, 
                           nameof(max_cargo), max_cargo, 
                           nameof(max_system_distance), max_system_distance,
                           nameof(max_price_age), max_price_age,
                           nameof(requires_large_pad), requires_large_pad,
                           nameof(allow_prohibited), allow_prohibited,
                           nameof(allow_planetary), allow_planetary,
                           nameof(allow_player_owned), allow_player_owned,
                           nameof(allow_restricted_access), allow_restricted_access,
                           nameof(unique), unique, 
                           nameof(permit), permit);


            return RequestJob("trade/route", query);
        }

        // str
        public Tuple<string, List<ISystem>> TryGetTradeRouter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);
            if (res.Item1 == null && res.Item2 != null)
            {
                List<ISystem> syslist = new List<ISystem>();
                JArray deals = res.Item2["result"].Array();

                for (int i = 0; i < deals.Count; i++)
                {
                    var deal = deals[i];

                    JObject source = deal["source"].Object();

                    string notes = "";

                    notes = notes.AppendPrePad("Station: " + source["station"].Str(), Environment.NewLine);

                    foreach (var cm in deal["commodities"].Array().EmptyIfNull())
                    {
                        notes = notes.AppendPrePad($"{cm["name"].Str()} buy {cm["amount"].Int()} profit {cm["total_profit"].Int()}", Environment.NewLine);
                    }

                    notes = notes.AppendPrePad("Profit so far: " + deal["cumulative_profit"].Int(), Environment.NewLine);

                    {
                        long id64 = source["system_id64"].Long();
                        string name = source["system"].Str();
                        double x = source["x"].Double();
                        double y = source["y"].Double();
                        double z = source["z"].Double();

                        SystemClass sy = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sy.Tag = notes;
                        syslist.Add(sy);
                    }

                    if (i == deals.Count - 1)
                    {
                        JObject destination = deal["destination"].Object();
                        long id64 = destination["system_id64"].Long();
                        string name = destination["system"].Str();
                        double x = destination["x"].Double();
                        double y = destination["y"].Double();
                        double z = destination["z"].Double();

                        SystemClass sy = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sy.Tag = $"Fly to {destination["station"].Str()} and sell all";
                        syslist.Add(sy);
                    }
                }

                return new Tuple<string, List<ISystem>>(null, syslist);
            }
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }

        // return SPANSH GUID search ID
        public string RequestNeutronRouter(string from, string to, double jumprange, int efficiency, Ship si)
        {
            var fsdspec = si.GetFSDSpec();
            if (fsdspec != null)
            {
                string query = HTTPExtensions.MakeQuery("range", jumprange,
                               "from", from,
                               "to", to,
                               "supercharge_multiplier", fsdspec.NeutronMultipler,
                               "efficiency", efficiency);
                return RequestJob("route", query);
            }
            else
                return null;
        }

        public Tuple<string, List<ISystem>> TryGetNeutronRouter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);

            if (res.Item1 == null && res.Item2 != null)
            {
                JObject result = res.Item2["result"].Object();
                JArray systems = result?["system_jumps"].Array();

                if (systems != null)
                {
                    List<ISystem> syslist = new List<ISystem>();

                    foreach (JObject sys in systems)
                    {
                        long id64 = sys["id64"].Long();
                        string name = sys["system"].Str();
                        double x = sys["x"].Double();
                        double y = sys["y"].Double();
                        double z = sys["z"].Double();
                        int jumps = sys["jumps"].Int();
                        bool neutron = sys["neutron_star"].Bool();
                        double distancejumped = sys["distance_jumped"].Double();

                        string notes = neutron ? "Neutron Star" : "";
                        if (jumps > 0)
                            notes = notes.AppendPrePad("Est Jumps: " + jumps.ToString(), Environment.NewLine);
                        if (distancejumped > 0)
                            notes = notes.AppendPrePad("Distance: " + distancejumped.ToString("N1"), Environment.NewLine);

                        var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sc.Tag = notes;
                        syslist.Add(sc);

                    }

                    return new Tuple<string, List<ISystem>>(null, syslist);
                }
                else
                    return new Tuple<string, List<ISystem>>("Bad neutron router return", null);
            }
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }

        public string RequestFleetCarrierRouter(string source, IEnumerable<string> destinations,
                                            int capacity_used, bool calculate_starting_fuel, bool squadroncarrier)
        {
            int capacity = squadroncarrier ? 60000 : 25000;
            int mass = squadroncarrier ? 15000 : 25000;
            string query = HTTPExtensions.MakeQuery(nameof(source), source,
                                                    nameof(destinations), destinations.ToArray(),
                                                    nameof(capacity), capacity,
                                                    nameof(mass), mass,
                                                    nameof(capacity_used), capacity_used,
                                                    nameof(calculate_starting_fuel), calculate_starting_fuel);
            return RequestJob("fleetcarrier/route", query);
        }

        // str
        public Tuple<string, List<ISystem>> TryGetFleetCarrierRouter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);
            if (res.Item1 == null && res.Item2 != null)
            {
                JObject result = res.Item2["result"].Object();
                JArray jumps = result?["jumps"].Array();

                if (jumps != null)
                {
                    List<ISystem> syslist = new List<ISystem>();
                    //double totalused = 0, fuelcurrent = 0;

                    foreach (JObject sys in jumps)
                    {
                        long id64 = sys["id64"].Long();
                        string name = sys["name"].Str();
                        double x = sys["x"].Double();
                        double y = sys["y"].Double();
                        double z = sys["z"].Double();
                        double tank = sys["fuel_in_tank"].Double();
                        double used = sys["fuel_used"].Double();
                        double market = sys["tritium_in_market"].Double();
                        double restock = sys["restock_amount"].Int();

                        //totalused += used;
                        //fuelcurrent += restock - used;

                        string notes = "";

                        if (used > 0)
                            notes = notes.AppendPrePad($"Fuel used {used:N0}, left {tank + market:N0}", Environment.NewLine);

                        if (sys["has_icy_ring"].Bool())
                            notes = notes.AppendPrePad(sys["is_system_pristine"].Bool() ? "Has Pristine Icy ring" : "Has Icy ring", Environment.NewLine);

                        if (restock > 0)
                            notes = notes.AppendPrePad($"Must restock {restock}", Environment.NewLine);

                        //                        notes = notes.AppendPrePad($"Total fuel used {totalused:N0} available {fuelcurrent:N0}", Environment.NewLine);

                        var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sc.Tag = notes;
                        syslist.Add(sc);
                    }

                    return new Tuple<string, List<ISystem>>(null, syslist);
                }
                else
                    return new Tuple<string, List<ISystem>>("Bas Fleet carrier response", null);
            }
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }

        public string RequestGalaxyPlotter(string source, string destination, int cargo, bool is_supercharged, bool use_supercharge, 
                                    bool use_injections, bool exclude_secondary, bool refuel_every_scoopable,
                                        Ship si, string algorithm = "optimistic")
        {
            var fsdspec = si.GetFSDSpec();
            if (fsdspec != null)
            {
                var json = si.JSONLoadout(true);
                System.Diagnostics.Debug.WriteLine($"JSON export to Spansh {json.ToString(true)}");
                FileHelpers.TryWriteToFile(@"c:\code\galaxyplotter.json", json.ToString(true));

                string query = HTTPExtensions.MakeQuery(nameof(source), source, nameof(destination), destination, nameof(is_supercharged), is_supercharged, nameof(use_supercharge), use_supercharge,
                                nameof(use_injections), use_injections, nameof(exclude_secondary), exclude_secondary,
                                "fuel_power", fsdspec.PowerConstant, "fuel_multiplier", fsdspec.FuelMultiplier,
                                "optimal_mass", fsdspec.OptimalMass, "base_mass", si.UnladenMass, "tank_size", si.FuelCapacity, "internal_tank_size", si.ReserveFuelCapacity,
                                "max_fuel_per_jump", fsdspec.MaxFuelPerJump,
                                "refuel_every_scoopable", refuel_every_scoopable,
                                nameof(cargo), cargo,
                                nameof(algorithm), algorithm,
                                "supercharge_multiplier", fsdspec.NeutronMultipler,
                                "range_boost", fsdspec.FSDGuardianBoosterRange,
                                "ship_build", json.ToString()
                                );
                return RequestJob("generic/route", query);
            }
            else
                return null;
        }

        public Tuple<string, List<ISystem>> TryGetGalaxyPlotter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);
            if (res.Item1 == null && res.Item2 != null)
            {
                JObject result = res.Item2["result"].Object();
                JArray jumps = result?["jumps"].Array();

                if (jumps != null)
                {
                    List<ISystem> syslist = new List<ISystem>();

                    foreach (JObject sys in jumps)
                    {
                        long id64 = sys["id64"].Long(0);
                        string name = sys["name"].Str();
                        double x = sys["x"].Double();
                        double y = sys["y"].Double();
                        double z = sys["z"].Double();

                        double tank = sys["fuel_in_tank"].Double();
                        double used = sys["fuel_used"].Double();
                        bool has_neutron = sys["has_neutron"].Bool();
                        bool is_scoopable = sys["is_scoopable"].Bool();
                        bool must_refuel = sys["must_refuel"].Bool();

                        string notes = "";

                        if (used > 0)
                            notes = notes.AppendPrePad($"Fuel used {used:N2}", Environment.NewLine);

                        notes = notes.AppendPrePad($"Fuel in tank {tank:N2}", Environment.NewLine);

                        if (has_neutron)
                            notes = notes.AppendPrePad("Neutron star", Environment.NewLine);
                        if (is_scoopable)
                            notes = notes.AppendPrePad("Scoopable star", Environment.NewLine);
                        if (must_refuel)
                            notes = notes.AppendPrePad("Must refuel", Environment.NewLine);

                        var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sc.Tag = notes;
                        syslist.Add(sc);
                    }

                    return new Tuple<string, List<ISystem>>(null, syslist);
                }
                else
                    return new Tuple<string, List<ISystem>>("Bas Fleet carrier response", null);
            }
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }

        public string RequestExomastery(string from, string to, double jumprange, int radius,
                                            int maxsystems,
                                            bool loop, int maxlstoarrival, bool avoid_thargoids,
                                            int minscanvalue)
        {
            if (loop)               // don't give to if loop
                to = null;

            string query = HTTPExtensions.MakeQuery("radius", radius,
                           "range", jumprange,
                           "from", from,
                           "to", to,
                           "max_results", maxsystems,
                           "max_distance", maxlstoarrival,
                           "min_value", minscanvalue,
                           "avoid_thargoids", avoid_thargoids,
                           "loop", loop);

            return RequestJob("exobiology/route", query);
        }

        public Tuple<string, List<ISystem>> TryGetExomastery(string jobname)
        {
            var res = TryGetResponseToJob(jobname);
            if (res.Item1 == null && res.Item2 != null)
            {
                JArray results = res.Item2["result"].Array();

                if (results != null)
                {
                    List<ISystem> syslist = new List<ISystem>();

                    foreach (JObject sys in results)
                    {
                        long id64 = sys["id64"].Str("0").InvariantParseLong(0);
                        string name = sys["name"].Str();
                        double x = sys["x"].Double();
                        double y = sys["y"].Double();
                        double z = sys["z"].Double();

                        int jumps = sys["jumps"].Int();
                        string notes = "";

                        if (jumps > 1)
                            notes = notes.AppendPrePad("Jumps:" + jumps.ToString("D"), Environment.NewLine);

                        long total = 0;

                        foreach (var ib in sys["bodies"].EmptyIfNull())
                        {
                            string fb = FieldBuilder.Build("", ib["name"].StrNull().ReplaceIfStartsWith(name),
                                                       "", ib["subtype"].StrNull(),
                                                       "Distance: ;ls;N1", ib["distance_to_arrival"].DoubleNull(),
                                                       "Map Value: ", ib["estimated_mapping_value"].LongNull(),
                                                       "Scan Value: ", ib["estimated_scan_value"].LongNull(),
                                                       "Landmark Value: ", ib["landmark_value"].LongNull());

                            notes = notes.AppendPrePad(fb, Environment.NewLine);

                            foreach (var lm in ib["landmarks"].EmptyIfNull())
                            {
                                string lms = FieldBuilder.Build("", lm["type"].StrNull(),
                                                           "", lm["subtype"].StrNull(),
                                                           "x ", lm["count"].IntNull(),
                                                           "Value:", lm["value"].IntNull());

                                notes = notes.AppendPrePad(lms, Environment.NewLine);
                            }

                            total += ib["estimated_mapping_value"].Long() + ib["estimated_scan_value"].Long() + ib["landmark_value"].Long();
                        }

                        notes = notes.AppendPrePad("Total:" + total.ToString("D"), Environment.NewLine);

                        var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sc.Tag = notes;
                        syslist.Add(sc);
                    }

                    return new Tuple<string, List<ISystem>>(null, syslist);
                }
                else
                    return new Tuple<string, List<ISystem>>("Bas Fleet carrier response", null);
            }
            else
                return new Tuple<string, List<ISystem>>(res.Item1, null);
        }


        // api point and query string, !errormessage or job ID or null
        private string RequestJob(string api, string query)
        {
            //query = "radius=500&range=50&from=Phoi+Aurb+HG-X+d1-499&to=Outotz+ZI-M+c22-0&max_results=100&max_distance=50000&min_value=1&avoid_thargoids=1&loop=1&body_types=Earth-like+world";

            var response = RequestPost(query, api, contenttype: "application/x-www-form-urlencoded; charset=UTF-8");

            var data = response.Body;       // always get a response, may not get a body (#3805) if Spansh is dead

            JToken json = data != null ? JObject.Parse(data, JToken.ParseOptions.CheckEOL) : null;

            if (response.Error || json == null )
            {
                if (json != null)
                    return $"!{json?["error"].Str()}";
                else
                    return "!Bad response no Body data";
            }
            else
            {
                var jobname = json?["job"].StrNull();
                return jobname;
            }
        }

        // string = error string, or null if no error.
        // on success, string is null and Jtoken != null
        // always return a tuple
        private Tuple<string, JToken> TryGetResponseToJob(string jobname)
        {
            var response = RequestGet("results/" + jobname);
            var data = response.Body;
            var json = data != null ? JObject.Parse(data, JToken.ParseOptions.CheckEOL) : null;

            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshresponse.txt", json?.ToString(true));

            if (response.Error)     // return
            {
                return new Tuple<string, JToken>(json?["error"].Str("Unknown error"), null);
            }
            else
            {
                string status = json["status"].StrNull();

                if (status == "queued")
                    return new Tuple<string, JToken>(null, null);
                else if (status == "ok")
                    return new Tuple<string, JToken>(null, json);
                else
                    return new Tuple<string, JToken>("Unknown spansh response", null);
            }
        }

        private List<ISystem> DecodeSystemsReturn(JToken data)
        {
            List<ISystem> syslist = new List<ISystem>();

            JArray systems = data["result"].Array();

            foreach (var sys in systems.EmptyIfNull())
            {
                if (sys is JObject)
                {
                    long id64 = sys["id64"].Str("0").InvariantParseLong(0);
                    string name = sys["name"].Str();
                    double x = sys["x"].Double();
                    double y = sys["y"].Double();
                    double z = sys["z"].Double();
                    int jumps = sys["jumps"].Int();
                    string notes = "Jumps:" + jumps.ToString();
                    long total = 0;
                    foreach (var ib in sys["bodies"].EmptyIfNull())
                    {
                        string fb = FieldBuilder.Build(";: ", ib["name"].StrNull().ReplaceIfStartsWith(name),
                                                   "<", ib["subtype"].StrNull(),
                                                   "Distance: ;ls;N1", ib["distance_to_arrival"].DoubleNull(),
                                                   "Map Value: ", ib["estimated_mapping_value"].LongNull(), "Scan Value: ", ib["estimated_scan_value"].LongNull());

                        total += ib["estimated_mapping_value"].Long() + ib["estimated_scan_value"].Long();
                        notes = notes.AppendPrePad(fb, Environment.NewLine);
                    }

                    notes = notes.AppendPrePad("Total:" + total.ToString("D"), Environment.NewLine);

                    var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                    sc.Tag = notes;
                    syslist.Add(sc);
                }
            }

            return syslist;
        }

    }
}

