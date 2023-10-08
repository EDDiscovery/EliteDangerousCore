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
using System.Web;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public SpanshClass()
        {
            base.httpserveraddress = "https://www.spansh.co.uk/api/";
        }

        public JObject GetSystemNames(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = RequestGet("systems/field_values/system_names/" + query, handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            return json;
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
                }
            };

            //System.Diagnostics.Debug.WriteLine($"Spansh post data for systems {jo.ToString()}");

            var response = RequestPost(jo.ToString(), "systems/search", handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            if ( json != null && json["results"] != null)
            {
                // structure tested oct 23

                try
                {
                    List<Tuple<ISystem, double>> systems = new List<Tuple<ISystem, double>>();

                    int? count = json["Count"].IntNull();
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

                                SystemClass sy = new SystemClass(name, sa, xr, yr, zr, SystemSource.FromSpansh , startype);

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


        public static EDStar? SpanshStarNameToEDStar(string name)
        {
            if (spanshtoedstar.TryGetValue(name, out EDStar value))
                return value;
            else
                return null;
        }

        // from https://spansh.co.uk/api/bodies/field_values/subtype
        private static Dictionary<string, EDStar> spanshtoedstar = new Dictionary<string, EDStar>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "O (Blue-White) Star", EDStar.O },
                { "B (Blue-White) Star", EDStar.B },
                { "A (Blue-White) Star", EDStar.A },
                { "F (White) Star", EDStar.F },
                { "G (White-Yellow) Star", EDStar.G },
                { "K (Yellow-Orange) Star", EDStar.K },
                { "M (Red dwarf) Star", EDStar.M },

                { "L (Brown dwarf) Star", EDStar.L },
                { "T (Brown dwarf) Star", EDStar.T },
                { "Y (Brown dwarf) Star", EDStar.Y },
                { "Herbig Ae Be Star", EDStar.AeBe },
                { "Herbig Ae/Be Star", EDStar.AeBe },
                { "T Tauri Star", EDStar.TTS },

                { "Wolf-Rayet Star", EDStar.W },
                { "Wolf-Rayet N Star", EDStar.WN },
                { "Wolf-Rayet NC Star", EDStar.WNC },
                { "Wolf-Rayet C Star", EDStar.WC },
                { "Wolf-Rayet O Star", EDStar.WO },

                // missing CS
                { "C Star", EDStar.C },
                { "CN Star", EDStar.CN },
                { "CJ Star", EDStar.CJ },
                // missing CHd

                { "MS-type Star", EDStar.MS },
                { "S-type Star", EDStar.S },

                { "White Dwarf (D) Star", EDStar.D },
                { "White Dwarf (DA) Star", EDStar.DA },
                { "White Dwarf (DAB) Star", EDStar.DAB },
                // missing DAO
                { "White Dwarf (DAZ) Star", EDStar.DAZ },
                { "White Dwarf (DAV) Star", EDStar.DAV },
                { "White Dwarf (DB) Star", EDStar.DB },
                { "White Dwarf (DBZ) Star", EDStar.DBZ },
                { "White Dwarf (DBV) Star", EDStar.DBV },
                // missing DO,DOV
                { "White Dwarf (DQ) Star", EDStar.DQ },
                { "White Dwarf (DC) Star", EDStar.DC },
                { "White Dwarf (DCV) Star", EDStar.DCV },
                // missing DX
                { "Neutron Star", EDStar.N },
                { "Black Hole", EDStar.H },
                // missing X but not confirmed with actual journal data


                { "A (Blue-White super giant) Star", EDStar.A_BlueWhiteSuperGiant },
                { "F (White super giant) Star", EDStar.F_WhiteSuperGiant },
                { "M (Red super giant) Star", EDStar.M_RedSuperGiant },
                { "M (Red giant) Star", EDStar.M_RedGiant},
                { "K (Yellow-Orange giant) Star", EDStar.K_OrangeGiant },
                // missing rogueplanet, nebula, stellarremanant
                { "Supermassive Black Hole", EDStar.SuperMassiveBlackHole },
                { "B (Blue-White super giant) Star", EDStar.B_BlueWhiteSuperGiant },
                { "G (White-Yellow super giant) Star", EDStar.G_WhiteSuperGiant },
            };

    }
}
