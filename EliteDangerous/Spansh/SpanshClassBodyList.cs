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
        // return dump of bodies in journal scan format
        // use https://spansh.co.uk/api/dump/<systemid64> 
        public JArray GetBodies(ISystem sys)
        {
            bool dump = true;       // previously, we supported search. Now lets always use dump, but for now, keep search code below for safety

            sys = EnsureSystemAddress(sys);

            if (sys == null)
                return null;

            BaseUtils.HttpCom.Response response = RequestGet("dump/" + sys.SystemAddress.ToStringInvariant());

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshbodies.txt", json?.ToString(true));

            JArray resultsarray;

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
                JArray retresult = new JArray();

                foreach (var so in resultsarray)
                {
                    try
                    {
                        // this follows the order found in JournalScan constructor

                        // so["stations"] = null;   System.Diagnostics.Debug.WriteLine($"Spansh Body JSON {so.ToString()}");

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
                                    ["RingClass"] = "eRingClass_" + node["type"].Str().Replace(" ", ""),
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

                            evt["AtmosphereComposition"] = dump ? so["atmosphereComposition"] : so["atmosphere_composition"]; // direct object to object

                            string atmostype = dump ? so["atmosphereType"].Str() : so["atmosphere"].Str();
                            if (atmostype.IsEmpty() || atmostype.EqualsIIC("No atmosphere"))
                            {
                                //System.Diagnostics.Debug.WriteLine($"Atmos {atmostype} {evt["PlanetClass"].Str()}");
                                if (evt["PlanetClass"].Str().ContainsIIC("Earthlike"))
                                    atmostype = "Earth Like";
                            }

                            else if (!atmostype.ContainsIIC("atmosphere"))
                                atmostype += " atmosphere";

                            evt["Atmosphere"] = atmostype;       // only use Atmosphere - JS uses this in preference to atmosphere type

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
                                        ac[node["name"].Str("?")] = node["share"].Double(1 / 100.0, 0);
                                    }
                                }
                            }

                            string vol = so[dump ? "volcanismType" : "volcanism_type"].StrNull();

                            if (vol.HasChars())
                            {
                                if (!vol.ContainsIIC("volcanism"))
                                    vol += " volcanism";
                                evt["Volcanism"] = vol;
                            }

                            //System.Diagnostics.Debug.WriteLine($"Spansh reads {evt["BodyName"]} atmos `{evt["Atmosphere"]}` vol `{evt["Volcanism"]}`");

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

        public static void ClearBodyCache()
        {
            lock (BodyCache) 
            {
                BodyCache.Clear();
            }
        }

        // only one request at a time going, this is to prevent multiple requests for the same body
        public static bool HasBodyLookupOccurred(string name)
        {
            lock (BodyCache) 
            {
                return BodyCache.ContainsKey(name);
            }
        }
        // true if lookup occurred, but no data. false otherwise
        public static bool HasNoDataBeenStoredOnBody(string name)      
        {
            lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
            {
                return BodyCache.TryGetValue(name, out List<JournalScan> d) && d == null;
            }
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

                    string cachefile = EliteConfigInstance.InstanceOptions.ScanCacheEnabled ?
                            System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"spansh_{(sys.SystemAddress.HasValue ? sys.SystemAddress.Value.ToStringInvariant() : sys.Name.SafeFileString())}.json") :
                            null;

                    if (cachefile != null && System.IO.File.Exists(cachefile))      // if we have that file
                    {
                        string cachedata = BaseUtils.FileHelpers.TryReadAllTextFromFile(cachefile); // try and read it
                        if (cachedata != null)
                        {
                            //System.Diagnostics.Debug.WriteLine($"Spansh Cache File read on {sys.Name} {sys.SystemAddress} from {cachefile}");
                            jlist = JArray.Parse(cachedata, JToken.ParseOptions.CheckEOL);  // if so, try a conversion
                            fromcache = true;
                        }
                    }

                    if (jlist == null)          // no data as yet, look at web
                    {
                        if (!weblookup)         // must be set for a web lookup
                            return null;

                       // System.Diagnostics.Debug.WriteLine($"Spansh Web lookup on {sys.Name} {sys.SystemAddress}");

                        SpanshClass sp = new SpanshClass();

                        jlist = sp.GetBodies(sys);          // get by id64, or name if required
                    }

                    if (jlist != null)         // we have data from file or from web
                    {
                        List<JournalScan> bodies = new List<JournalScan>();

                        foreach (JObject jo in jlist)
                        {
                            JournalScan js = new JournalScan(jo.Object());

                            //System.Diagnostics.Debug.WriteLine($"Spansh JS: {js.DisplayString(null, null)}");

                            if (jo.Contains("EDDMeanAnomalyTimestamp"))        // this name is used to carry time info which is not in the journal
                            {
                                DateTime t = jo["EDDMeanAnomalyTimestamp"].DateTimeUTC();
                                js.EventTimeUTC = t;
                            }

                            bodies.Add(js);
                        }

                        if (cachefile != null)
                            BaseUtils.FileHelpers.TryWriteToFile(cachefile, jlist.ToString(true));      // save to file so we don't have to reload

                        BodyCache[sys.Name] = bodies;

                       // System.Diagnostics.Debug.WriteLine($"Spansh Web/File Lookup complete {sys.Name} {bodies.Count} cache {fromcache}");
                        return new GetBodiesResults(bodies, fromcache);
                    }
                    else
                    {
                        BodyCache[sys.Name] = null;
                       // System.Diagnostics.Debug.WriteLine($"Spansh Web Lookup complete no info {sys.Name}");
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

