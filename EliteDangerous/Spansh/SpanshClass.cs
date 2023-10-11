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
using System.Linq;
using System.Web;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public SpanshClass()
        {
            base.httpserveraddress = RootURL + "api/";
        }

        public const string RootURL = "https://spansh.co.uk/";
        public const int MaxReturnSize = 500;       // as coded by spansh

        static public void LaunchBrowserForSystem(long sysaddr)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "system/" + sysaddr.ToStringInvariant());
        }
        static public string URLForSystem(long sysaddr)
        {
            return RootURL + "system/" + sysaddr.ToStringInvariant();
        }

        static public void LaunchBrowserForStationByMarketID(long marketid)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "station/" + marketid.ToStringInvariant());
        }

        static public void LaunchBrowserForStationByFullBodyID(long fullbodyid)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "body/" + fullbodyid.ToStringInvariant());
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

            return ret != null ? ret.Select(x=>x.Item1).ToList() : null;
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
        public List<Tuple<ISystem, double>> GetSphereSystems(string name, double maxradius, double minradius)
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
                ["reference_system"] = name,
                ["size"] = MaxReturnSize, // this appears the max, oct 23
            };

            return IssueSystemsQuery(jo);
        }

        private List<Tuple<ISystem, double>> IssueSystemsQuery(JObject query)
        { 
            //System.Diagnostics.Debug.WriteLine($"Spansh post data for systems {jo.ToString()}");

            var response = RequestPost(query.ToString(), "systems/search", handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");

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

        // return dump of bodies in journal scan format
        // use https://spansh.co.uk/api/dump/<systemid64> if we have one
        // else use bodies/search with name if we dont
        // Of course, spansh has decided to code the fields differently between the two so it has to adapt
        // https://spansh.co.uk/api/body/<id64> for a single one
        public JArray GetBodies(ISystem sys)
        {
            BaseUtils.ResponseData response;
            bool dump = true;

            if (sys.SystemAddress.HasValue)
            {
                response = RequestGet("dump/" + sys.SystemAddress.ToStringInvariant(), handleException: true);
            }
            else
            {
                JObject query = new JObject()
                {
                    ["filters"] = new JObject()
                    {
                        ["system_name"] = new JObject()
                        {
                            ["value"] = sys.Name,
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

                response = RequestPost(query.ToString(),"bodies/search", handleException: true);
                dump = false;
            }

            if (response.Error)
                return null;

            var data = response.Body;

//            string data = BaseUtils.FileHelpers.TryReadAllTextFromFile(@"c:\code\soldump.txt");

            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            JArray resultsarray = null;

            if (dump)
            {
                resultsarray = json != null ? json["system"].I("bodies").Array() : null;
            }
            else
            {
                resultsarray = json != null ? json["results"].Array() : null;
            }

            if (resultsarray != null)
            {
                //System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");

                JArray retresult = new JArray();

                foreach (var so in resultsarray)
                {
                    try
                    {
                        // this follows the order found in JournalScan constructor

                        //so["stations"] = null;
                        //System.Diagnostics.Debug.WriteLine($"Spansh Body JSON {so.ToString()}");

                        JObject evt = new JObject();
                        evt["ScanType"] = "Detailed";
                        evt["BodyName"] = so["name"];
                        evt["BodyID"] = so[dump ? "bodyId" : "body_id"];
                        evt["StarSystem"] = sys.Name;
                        evt["SystemAddress"] = sys.SystemAddress.HasValue ? sys.SystemAddress.Value : json["reference"]["id64"].Long();
                        evt["DistanceFromArrivalLS"] = so[dump ? "distanceToArrival" : "distance_to_arrival"];
                        evt["WasDiscovered"] = true;        // obv, since spansh has the data
                        evt["WasMapped"] = true;

                        if (so["parents"] != null)
                        {
                            if (dump)
                                evt["Parents"] = so["parents"];     // seems to be a direct copy
                            else
                            {
                                JArray parents = new JArray();
                                evt["Parents"] = parents;
                                foreach (var node in so["parents"])
                                {
                                    JObject entry = new JObject
                                    {
                                        [node["type"].Str("?")] = node["id"]
                                    };
                                    parents.Add(entry);
                                }
                            }
                        }

                        evt["RotationPeriod"] = so[dump ? "rotationalPeriod" : "rotational_period"].Double(BodyPhysicalConstants.oneDay_s, 0);
                        evt["SurfaceTemperature"] = so[dump ? "surfaceTemperature" : "surface_temperature"];

                        if (so["solarRadius"] != null)
                            evt["Radius"] = so["solarRadius"].Double(BodyPhysicalConstants.oneSolRadius_m, 0);
                        else
                            evt["Radius"] = so["radius"].Double(0) * 1000;

                        if (so["rings"] != null)
                        {
                            JArray rings = new JArray();
                            evt["Rings"] = rings;
                            foreach (var node in so["rings"])
                            {
                                JObject entry = new JObject
                                {
                                    ["InnerRad"] = node["inner_radius"],
                                    ["OuterRad"] = node["outer_radius"],
                                    ["MassMT"] = node["mass"],
                                    ["Name"] = node["name"],
                                    ["RingClass"] = "eRingClass_" + node["type"],
                                };
                                rings.Add(entry);
                            }
                        }

                        if (so["type"].Str() == "Star")
                        {
                            evt["StarType"] = SpanshStarNameToEDStar(so[dump ? "subType" : "subtype"].Str()).ToString();

                            evt["StellarMass"] = so[dump ? "solarMasses" : "solar_masses"];
                            evt["AbsoluteMagnitude"] = so["absoluteMagnitude"];
                            evt["Luminosity"] = so[dump ? "luminosity" : "luminosity_class"];
                            if (so[dump ? "spectralClass" : "spectral_class"].Str().Length > 1)       // coded as G2
                                evt["Subclass"] = so[dump ? "spectralClass" : "spectral_class"].Str().Substring(1).InvariantParseIntNull();
                            evt["Age_MY"] = so["age"];
                        }
                        else if (so["type"].Str() == "Planet")
                        {
                            evt["PlanetClass"] = SpanshPlanetNameToEDPlanet(so[dump ? "subType" : "subtype"].Str()).ToString();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Spansh bodies ignore {so["type"].Str()}");
                            continue;    
                        }

                        if (so[dump ? "semiMajorAxis" : "semi_major_axis"] != null)
                        {
                            evt["SemiMajorAxis"] = so[dump ? "semiMajorAxis" : "semi_major_axis"].Double(BodyPhysicalConstants.oneAU_m, 0);
                            evt["Eccentricity"] = so[dump ? "orbitalEccentricity" : "orbital_eccentricity"];
                            evt["OrbitalInclination"] = so[dump ? "orbitalInclination" : "orbital_inclination"];
                            evt["Periapsis"] = so[dump ? "argOfPeriapsis" : "arg_of_periapsis"];
                            evt["MeanAnomaly"] = so[dump ? "meanAnomaly" : "mean_anomaly"];
                            evt["AscendingNode"] = so[dump ? "ascendingNode" : "ascending_node"];
                            evt["OrbitalPeriod"] = so[dump ? "orbitalPeriod" : "orbital_period"].Double(BodyPhysicalConstants.oneDay_s, 0);
                            evt["AxialTilt"] = so[dump ? "axialTilt" : "axis_tilt"];
                            evt["TidalLock"] = so[dump ? "rotationalPeriodTidallyLocked" : "is_rotational_period_tidally_locked"].Bool(false);
                        }

                        if (evt["PlanetClass"] != null)
                        {
                            evt["TerraformState"] = so[dump ? "terraformingState" : "terraforming_state"];

                            if (dump)
                            {
                                evt["AtmosphereComposition"] = so["atmosphereComposition"];
                                evt["AtmosphereType"] = so["atmosphereType"];
                            }
                            else
                            {
                                string atconsituents = "";
                                if (so["atmosphere_composition"] != null)
                                {
                                    JArray ac = new JArray();
                                    evt["AtmosphereComposition"] = ac;
                                    foreach (var node in so["atmosphere_composition"])
                                    {
                                        JObject entry = new JObject
                                        {
                                            ["Name"] = node["name"].Str("?"),
                                            ["Percent"] = node["share"].Double(0),
                                        };

                                        atconsituents = atconsituents.AppendPrePad(entry["Name"].Str(), ",");
                                        ac.Add(entry);
                                    }
                                }

                                string atmos = so["atmosphere"].Str();
                                if (atmos == "No atmosphere" && atconsituents.HasChars())
                                    atmos = atconsituents;

                                evt["Atmosphere"] = atmos;       // only use Atmosphere - JS uses this in preference to atmosphere type
                            }

                            if (dump)
                            {
                                if (so["solidComposition"] != null)
                                {
                                    JObject co = new JObject();
                                    evt["Composition"] = co;
                                    foreach (var entry in so["solidComposition"])
                                    {
                                        co[entry.Name] = so["solidComposition"][entry.Name].Double(1 / 100.0, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (so["solid_composition"] != null)
                                {
                                    JObject ac = new JObject();
                                    evt["Composition"] = ac;
                                    foreach (var node in so["solid_composition"])
                                    {
                                        ac[so["name"].Str("?")] = so["share"];
                                    }
                                }
                            }

                            evt["Volcanism"] = so[dump ? "volcanismType" : "volcanism_type"];

                            evt["SurfaceGravity"] = so["gravity"].Double(0) * BodyPhysicalConstants.oneGee_m_s2;        // its in G, convert back into m/s
                            evt["SurfacePressure"] = so[dump ? "surfacePressure" : "surface_pressure"].Double(0) * BodyPhysicalConstants.oneAtmosphere_Pa;
                            evt["Landable"] = so[dump ? "isLandable" : "is_lanable"].Bool(false);
                            evt["MassEM"] = so[dump ? "earthMasses" : "earth_masses"];

                            if (dump)
                                evt["Materials"] = so["materials"];
                            else
                            {
                                if (so["materials"] != null)
                                {
                                    JArray mats = new JArray();
                                    evt["Materials"] = mats;
                                    foreach (var node in so["materials"])
                                    {
                                        JObject entry = new JObject
                                        {
                                            ["Name"] = so["name"],
                                            ["Percent"] = so["share"],
                                        };
                                        mats.Add(entry);
                                    }
                                }
                            }

                            evt["ReserveLevel"] = so[dump ? "reserveLevel" : "reserve_level"];
                        }

                        evt["EDDFromSpanshBody"] = true;

                        //JournalScan js = new JournalScan(evt); System.Diagnostics.Debug.WriteLine($"Journal scan {js.DisplayString(0, includefront: true)}");

                        retresult.Add(evt);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Spansh read bodies exception {ex}");
                    }
                }

                return retresult;
            }

            return null;
        }

        public async static System.Threading.Tasks.Task<Tuple<List<JournalScan>, bool>> GetBodiesListAsync(ISystem sys, bool weblookup = true) 
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return GetBodiesList(sys, weblookup);
            });
        }

        // EDSMBodiesCache gets either the body list, or null marking no EDSM server data
        static private Dictionary<string, List<JournalScan>> BodyCache = new Dictionary<string, List<JournalScan>>();

        public static bool HasBodyLookupOccurred(string name)
        {
            return BodyCache.ContainsKey(name);
        }
        public static bool HasNoDataBeenStoredOnBody(string name)      // true if lookup occurred, but no data. false otherwise
        {
            return BodyCache.TryGetValue(name, out List<JournalScan> d) && d == null;
        }

        static public Tuple<List<JournalScan>, bool> GetBodiesList(ISystem sys, bool weblookup = true)
        {
            try
            {
                lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
                {
                    // System.Threading.Thread.Sleep(2000); //debug - delay to show its happening 
                    // System.Diagnostics.Debug.WriteLine("EDSM Cache check " + sys.EDSMID + " " + sys.SystemAddress + " " + sys.Name);

                    if (BodyCache.TryGetValue(sys.Name, out List<JournalScan> we))
                    {
                        System.Diagnostics.Debug.WriteLine($"Spansh Body Cache hit on {sys.Name} {we != null}");
                        if (we == null) // lookedup but not found
                            return null;
                        else
                            return new Tuple<List<JournalScan>, bool>(we, true);        // mark from cache
                    }

                    if (!weblookup)      // must be set for a web lookup
                        return null;

                    System.Diagnostics.Debug.WriteLine($"Spansh Web lookup on {sys.Name} {sys.SystemAddress}");

                    SpanshClass sp = new SpanshClass();

                    var jlist = sp.GetBodies(sys);          // get by id64, or name if required

                    if ( jlist != null)
                    {
                        List<JournalScan> bodies = new List<JournalScan>();

                        foreach ( var jo in jlist)
                        {
                            JournalScan js = new JournalScan(jo.Object());
                            bodies.Add(js);
                        }

                        BodyCache[sys.Name] = bodies;
                        System.Diagnostics.Debug.WriteLine($"Spansh Web Lookup complete {sys.Name} {bodies.Count}");
                        return new Tuple<List<JournalScan>, bool>(bodies, false);       // not from cache
                    }
                    else
                    {
                        BodyCache[sys.Name] = null;
                        System.Diagnostics.Debug.WriteLine($"Spansh Web Lookup complete no info {sys.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Exception: {ex.Message}");
            }

            return null;
        }


        public static EDStar? SpanshStarNameToEDStar(string name)
        {
            if (spanshtoedstar.TryGetValue(name, out EDStar value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"SPANSH failed to decode star {name}");
                return null;
            }
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

        public static EDPlanet? SpanshPlanetNameToEDPlanet(string name)
        {
            if (spanshtoedplanet.TryGetValue(name, out EDPlanet value))
                return value;
            else
            {
                System.Diagnostics.Debug.WriteLine($"SPANSH failed to decode planet {name}");
                return null;
            }
        }

        private static Dictionary<string, EDPlanet> spanshtoedplanet = new Dictionary<string, EDPlanet>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Ammonia world", EDPlanet.Ammonia_world },
            { "Class I gas giant", EDPlanet.Sudarsky_class_I_gas_giant },
            { "Class II gas giant", EDPlanet.Sudarsky_class_II_gas_giant },
            { "Class III gas giant", EDPlanet.Sudarsky_class_III_gas_giant },
            { "Class IV gas giant", EDPlanet.Sudarsky_class_IV_gas_giant },
            { "Class V gas giant", EDPlanet.Sudarsky_class_V_gas_giant },
            { "Earth-like world", EDPlanet.Earthlike_body },
            { "Gas giant with ammonia-based life", EDPlanet.Gas_giant_with_ammonia_based_life },
            { "Gas giant with water-based life", EDPlanet.Gas_giant_with_water_based_life },
            { "Helium gas giant", EDPlanet.Helium_gas_giant },
            { "Helium-rich gas giant", EDPlanet.Helium_rich_gas_giant },
            { "High metal content world", EDPlanet.High_metal_content_body },
            { "Metal-rich body", EDPlanet.Metal_rich_body },
            { "Icy body", EDPlanet.Icy_body },
            { "Rocky Ice world", EDPlanet.Rocky_ice_body },
            { "Rocky body", EDPlanet.Rocky_body },
            { "Water giant", EDPlanet.Water_giant },
            { "Water world", EDPlanet.Water_world },
        };

    }
}

