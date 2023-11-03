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

using BaseUtils;
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

        public static TimeSpan MaxCacheAge = new TimeSpan(7, 0, 0, 0);

        #region Browser
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
        static public void LaunchBrowserForSystem(string name)
        {
            SpanshClass sp = new SpanshClass();
            ISystem s = sp.GetSystem(name);
            if (s != null)
                LaunchBrowserForSystem(s.SystemAddress.Value);
        }

        #endregion

        #region Systems

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

        // find system info of name, case insensitive
        public ISystem GetSystem(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = RequestGet("search/systems" + query, handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            if ( json != null )
            {
                foreach( var body in json["results"].EmptyIfNull())
                {
                    string rname = body["name"].Str();
                    if ( rname.Equals(name,StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new SystemClass(rname, body["id64"].Long(), body["x"].Double(), body["y"].Double(), body["z"].Double(), SystemSource.FromSpansh);
                    }

                }
            }

            return null;
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

        #endregion

        #region Body list

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
                System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");

                JArray retresult = new JArray();

                foreach (var so in resultsarray)
                {
                    try
                    {
                        // this follows the order found in JournalScan constructor

                        so["stations"] = null;   System.Diagnostics.Debug.WriteLine($"Spansh Body JSON {so.ToString()}");

                        JObject evt = new JObject();
                        evt["ScanType"] = "Detailed";
                        evt["BodyName"] = so["name"];
                        evt["BodyID"] = so[dump ? "bodyId" : "body_id"];
                        evt["StarSystem"] = sys.Name;
                        evt["SystemAddress"] = sys.SystemAddress.HasValue ? sys.SystemAddress.Value : json["reference"]["id64"].Long();
                        evt["DistanceFromArrivalLS"] = so[dump ? "distanceToArrival" : "distance_to_arrival"];
                        evt["WasDiscovered"] = true;        // obv, since spansh has the data
                        evt["WasMapped"] = false;

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

                        // https://spansh.co.uk/api/bodies/field_values/rings
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
                                    ["RingClass"] = "eRingClass_" + node["type"].Str().Replace(" ",""),
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
                                        ac[node["name"].Str("?")] = node["share"].Double(1/100.0,0);
                                    }
                                }
                            }

                            evt["Volcanism"] = so[dump ? "volcanismType" : "volcanism_type"];

                            evt["SurfaceGravity"] = so["gravity"].Double(0) * BodyPhysicalConstants.oneGee_m_s2;        // its in G, convert back into m/s
                            evt["SurfacePressure"] = so[dump ? "surfacePressure" : "surface_pressure"].Double(0) * BodyPhysicalConstants.oneAtmosphere_Pa;
                            evt["Landable"] = so[dump ? "isLandable" : "is_landable"].Bool(false);
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
                                            ["Name"] = node["name"],
                                            ["Percent"] = node["share"],
                                        };
                                        mats.Add(entry);
                                    }
                                }
                            }

                            if (dump)       // only dump has a timestamp relevant, search does not. if it becomes a problem, we would need to look up id 64 and then always use dump
                                evt["EDDMeanAnomalyTimestamp"] = so["timestamps"].I("meanAnomaly");

                            evt["ReserveLevel"] = so[dump ? "reserveLevel" : "reserve_level"];
                        }

                        evt["EDDFromSpanshBody"] = true;

                        JournalScan js = new JournalScan(evt); System.Diagnostics.Debug.WriteLine($"Journal scan {js.DisplayString(0, includefront: true)}");

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

        public class GetBodiesResults
        {
            public List<JournalScan> Bodies { get; set; }
            public bool FromCache { get; set; }

            public GetBodiesResults(List<JournalScan> list, bool fromcache) { Bodies = list; FromCache = fromcache; }
        }


        // return list, if from cache, if web lookup occurred

        public async static System.Threading.Tasks.Task<GetBodiesResults> GetBodiesListAsync(ISystem sys, bool weblookup = true) 
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

        static public GetBodiesResults GetBodiesList(ISystem sys, bool weblookup = true)
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
                            return new GetBodiesResults(we, true);        // mark from cache
                    }

                    JArray jlist = null;
                    bool fromcache = false;

                    // calc name of cache file

                    string cachefile = EliteConfigInstance.InstanceOptions.ScanCachePath != null ?
                            System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"spansh_{(sys.SystemAddress.HasValue ? sys.SystemAddress.Value.ToStringInvariant() : sys.Name.SafeFileString())}.json") :
                            null;

                    if (cachefile != null && System.IO.File.Exists(cachefile))      // if we have that file
                    {
                        string cachedata = BaseUtils.FileHelpers.TryReadAllTextFromFile(cachefile); // try and read it
                        if (cachedata != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Spansh Cache File read on {sys.Name} {sys.SystemAddress} from {cachefile}");
                            jlist = JArray.Parse(cachedata, JToken.ParseOptions.CheckEOL);  // if so, try a conversion
                            fromcache = true;
                        }
                    }

                    if (jlist == null)          // no data as yet, look at web
                    {
                        if (!weblookup)         // must be set for a web lookup
                            return null;

                        System.Diagnostics.Debug.WriteLine($"Spansh Web lookup on {sys.Name} {sys.SystemAddress}");

                        SpanshClass sp = new SpanshClass();

                        jlist = sp.GetBodies(sys);          // get by id64, or name if required
                    }

                    if ( jlist != null)         // we have data from file or from web
                    {
                        List<JournalScan> bodies = new List<JournalScan>();

                        foreach ( JObject jo in jlist)
                        {
                            JournalScan js = new JournalScan(jo.Object());
                            
                            if ( jo.Contains("EDDMeanAnomalyTimestamp"))        // this name is used to carry time info which is not in the journal
                            {
                                DateTime t = jo["EDDMeanAnomalyTimestamp"].DateTimeUTC();
                                js.EventTimeUTC = t;
                            }

                            bodies.Add(js);
                        }

                        if ( cachefile != null )
                            BaseUtils.FileHelpers.TryWriteToFile(cachefile, jlist.ToString(true));      // save to file so we don't have to reload

                        BodyCache[sys.Name] = bodies;

                        System.Diagnostics.Debug.WriteLine($"Spansh Web/File Lookup complete {sys.Name} {bodies.Count} cache {fromcache}");
                        return new GetBodiesResults(bodies, fromcache);       
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

        #endregion

        #region Routing

        // api point and query string, null error
        private string RequestJob(string api, string query)
        {
            var response = RequestPost(query, api, handleException: true, contenttype: "application/x-www-form-urlencoded; charset=UTF-8");

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            if (response.Error)
            {
                return $"!{json?["error"].Str()}";
            }
            else
            {
                var jobname = json?["job"].StrNull();
                return jobname;
            }
        }

        // null means bad. Else it will return false=still pending, true = result got.
        private Tuple<bool, JToken> TryGetResponseToJob(string jobname)
        {
            var response = RequestGet("results/" + jobname, handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;


            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            //System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");
            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshresponse.txt", json?.ToString(true));

            if (json != null)
            {
                string status = json["status"].StrNull();
                if (status == "queued")
                {
                    return new Tuple<bool, JToken>(false, json);
                }
                else if (status == "ok")
                {
                    return new Tuple<bool, JToken>(true, json);
                }
            }

            return null;
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
                        string fb = FieldBuilder.Build("", ib["name"].StrNull().ReplaceIfStartsWith(name),
                                                   "", ib["type"].StrNull(), "", ib["subtype"].StrNull(),
                                                   "Distance:;ls;N1", ib["distance_to_arrival"].DoubleNull(),
                                                   "Map Value:", ib["estimated_mapping_value"].LongNull(), "Scan Value:", ib["estimated_scan_value"].LongNull());

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

        /// <summary>
        /// Return spansh job id
        /// </summary>
        public string RequestRoadToRichesAmmoniaEarthlikes( string from, string to, int jumprange, int radius, 
                                            int maxsystems, bool avoidthargoids, 
                                            bool loop, int maxlstoarrival, 
                                            int minscanvalue,
                                            bool? usemappingvalue = null,  
                                            string bodytypes = null)
        {
            string query = MakeQuery("radius" , radius , 
                           "range", jumprange,
                           "from", from,
                           "to", to,
                           "max_results", maxsystems,
                           "max_distance" , maxlstoarrival,
                           "min_value" , minscanvalue,
                           "use_mapping_value" , usemappingvalue,
                           "avoid_thargoids" , avoidthargoids,
                           "loop" , loop,
                           "body_types", bodytypes);

            return RequestJob("riches/route", query);
        }

        public Tuple<bool,List<ISystem>> TryGetRoadToRichesAmmonia(string jobname)
        {
            var res = TryGetResponseToJob(jobname);

            if (res == null)
                return null;
            else if (res.Item1 == true)
                return new Tuple<bool, List<ISystem>>(true, DecodeSystemsReturn(res.Item2));
            else
                return new Tuple<bool, List<ISystem>>(false, null);
        }


        public string RequestTradeRouter(string fromsystem, string fromstation,
                                            int max_hops, int max_hop_distance,
                                            long starting_capital,
                                            int max_cargo,
                                            int max_system_distance,
                                            int max_agesec,
                                            bool requires_large_pad, bool allow_prohibited, bool allow_planetary, bool avoid_loops, bool permit)
        {
            string query = MakeQuery(nameof(max_hops), max_hops, nameof(max_hop_distance), max_hop_distance,
                           "system", fromsystem, "station", fromstation,
                           nameof(starting_capital), starting_capital, nameof(max_cargo), max_cargo, nameof(max_system_distance), max_system_distance,
                           "max_price_age", max_agesec,
                           nameof(requires_large_pad), requires_large_pad, nameof(allow_prohibited), allow_prohibited, nameof(allow_planetary), allow_planetary,
                           "unique", avoid_loops, nameof(permit), permit);

            return RequestJob("trade/route", query);
        }

        public Tuple<bool, List<ISystem>> TryGetTradeRouter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);

            if (res == null)
            {

                return null;
            }
            else if (res.Item1 == true)
            {
                List<ISystem> syslist = new List<ISystem>();
                JArray deals = res.Item2["result"].Array();



                for( int i = 0; i < deals.Count; i++ )
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

                    if ( i == deals.Count-1)
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

                return new Tuple<bool, List<ISystem>>(true, syslist);
            }
            else
                return new Tuple<bool, List<ISystem>>(false, null);
        }

        // return SPANSH GUID search ID
        public string RequestNeutronRouter(string from, string to, int jumprange, int efficiency)
        {
            string query = MakeQuery("range", jumprange,
                           "from", from,
                           "to", to,
                           "efficiency",  efficiency);
            return RequestJob("route", query);
        }

        public Tuple<bool, List<ISystem>> TryGetNeutronRouter(string jobname)
        {
            var res = TryGetResponseToJob(jobname);

            if (res == null)
                return null;

            if (res.Item1 == true)
            {
                JObject result = res.Item2["result"].Object();
                JArray systems = result?["system_jumps"].Array();

                if ( systems != null )
                {
                    List<ISystem> syslist = new List<ISystem>();

                    foreach( JObject sys in systems)
                    {
                        long id64 = sys["id64"].Str("0").InvariantParseLong(0);
                        string name = sys["system"].Str();
                        double x = sys["x"].Double();
                        double y = sys["y"].Double();
                        double z = sys["z"].Double();
                        int jumps = sys["jumps"].Int();
                        bool neutron = sys["neutron_star"].Bool();
                        double distancejumped = sys["distance_jumped"].Double();

                        string notes = neutron ? "Neutron Star" : "";
                        if ( jumps>0)
                            notes = notes.AppendPrePad("Est Jumps:" + jumps.ToString(), Environment.NewLine);
                        if ( distancejumped>0)
                            notes = notes.AppendPrePad("Distance:" + distancejumped.ToString("N1"), Environment.NewLine);

                        var sc = new SystemClass(name, id64, x, y, z, SystemSource.FromSpansh);
                        sc.Tag = notes;
                        syslist.Add(sc);

                    }

                    return new Tuple<bool, List<ISystem>>(true, syslist);
                }

                return null;
            }
            else
                return new Tuple<bool, List<ISystem>>(false, null);
        }

        #endregion


        #region Stations

        public List<StationInfo> GetStations(string name, double distance, bool fleetcarriers = false)
        {
            JObject query = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["distance"] = new JObject()
                    {
                        ["min"] = 0,
                        ["max"] = distance,
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

            return IssueStationQuery(query, fleetcarriers);
        }

        private List<StationInfo> IssueStationQuery(JObject query, bool fleetcarriers)
        {
            System.Diagnostics.Debug.WriteLine($"Spansh post data for station {query.ToString()}");

            var response = RequestPost(query.ToString(), "stations/search", handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            //System.Diagnostics.Debug.WriteLine($"Spansh returns {json?.ToString(true)}");
            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshresponse.txt", json?.ToString(true));

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
                         
                            if (station.Faction == "FleetCarrier" && !fleetcarriers)
                                continue;


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

                            station.LandingPads = new JournalDocked.LandingPadList() { Large = evt["large_pads"].Int(), Medium = evt["medium_pads"].Int(), Small = evt["small_pads"].Int() };

                            // prohibited commodities

                            JArray market = evt["market"].Array();
                            if ( market != null )
                            {
                                station.Market = new List<CCommodities>();
                                foreach( JObject commd in market)
                                {
                                    CCommodities cc = new CCommodities();
                                    if ( cc.FromJsonSpansh(commd))
                                    {
                                        station.Market.Add(cc);
                                    }
                                }
                            }

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


        #endregion
    }
}

