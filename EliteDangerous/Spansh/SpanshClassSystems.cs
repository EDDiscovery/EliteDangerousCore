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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
         // null, or list of found systems (may be empty)
        public List<ISystem> GetSystems(string systemname, bool wildcard = true)
        {
            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["name"] = new JObject()
                    {
                        ["value"] = systemname + (wildcard ? "*" : ""),
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

                ["size"] = MaxReturnSize, // this appears the max, oct 23
            };

            var ret = IssueSystemsQuery(jo);

            return ret != null ? ret.Select(x => x.Item1).ToList() : null;
        }


        // null, or list of found systems (may be empty)
        public List<Tuple<ISystem, double>> GetSphereSystems(double x, double y, double z, double maxradius, double minradius)
        {
            // POST, systems :  { "filters":{ "distance":{ "min":"0","max":"7"} },"sort":[{ "distance":{ "direction":"asc"}}],"size":10,"page":0,"reference_coords":{ "x":100,"y":100,"z":100} }: 
            //                  {"filters": { "distance":{"min":"0","max":"10"}}, "sort":[{"distance":{"direction":"asc"}}],"size":10,"reference_coords":{"x":0.0,"y":0.0,"z":0.0}}
            // size relates to number returned per page

            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["distance"] = new JObject()
                    {
                        ["min"] = minradius.ToStringInvariant(),
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
                ["reference_coords"] = new JObject()
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["z"] = z,
                },
                ["size"] = MaxReturnSize, // this appears the max, oct 23
            };

            return IssueSystemsQuery(jo);
        }


        // null, or list of found systems (may be empty)
        public List<Tuple<ISystem, double>> GetSphereSystems(string systemname, double maxradius, double minradius)
        {
            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["distance"] = new JObject()
                    {
                        ["min"] = minradius.ToStringInvariant(),
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
                ["size"] = MaxReturnSize, // this appears the max, oct 23
            };

            return IssueSystemsQuery(jo);
        }

 

        private List<Tuple<ISystem, double>> IssueSystemsQuery(JObject query)
        {
            //System.Diagnostics.Debug.WriteLine($"Spansh post data for search systems {query.ToString(true)}");

            var response = RequestPost(query.ToString(), "systems/search");

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

           // System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");
            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshsearch.txt", json?.ToString(true));


            if (json != null && json["results"] != null)
            {
                // structure tested oct 23

                try
                {
                    List<Tuple<ISystem, double>> systems = new List<Tuple<ISystem, double>>();

                    foreach (JToken list in json["results"].Array())    // array of objects
                    {
                        JObject sysobj = list.Object();
                        if (sysobj != null)
                        {
                            double distance = sysobj["distance"].Double();      // system info at base of object
                            double xr = sysobj["x"].Double();
                            double yr = sysobj["y"].Double();
                            double zr = sysobj["z"].Double();
                            string name = sysobj["name"].StrNull();
                            long? sa = sysobj["id64"].LongNull();

                            if (name != null && sa != null)     // triage out
                            {
                                EDStar startype = EDStar.Unknown;

                                JArray bodylist = sysobj["bodies"].Array();
                                if (bodylist != null)
                                {
                                    foreach (JObject body in bodylist)      // array of bodies, with main star noted
                                    {
                                        bool is_main_star = body["is_main_star"].Bool(false);
                                        string spanshname = body["subtype"].StrNull();
                                        if (is_main_star && spanshname != null)
                                        {
                                            var edstar = SpanshStarNameToEDStar(spanshname);
                                            if (edstar != null)
                                                startype = edstar.Value;
                                            else
                                                System.Diagnostics.Debug.WriteLine($"Spansh star did not decode {spanshname}");

                                            break;
                                        }
                                    }
                                }

                                SystemClass sy = new SystemClass(name, sa, xr, yr, zr, SystemSource.FromSpansh, startype);

                                if (sy.Triage())
                                {
                                    systems.Add(new Tuple<ISystem, double>(sy, distance));
                                }
                            }
                        }
                    }

                    return systems;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Spansh Sphere systems failed due to " + ex);
                }
            }

            return null;
        }
    }
}

