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
        public JArray GetBodies(ISystem searchsystem, out int? bodycount, out ISystem foundsystem)
        {
            bodycount = null;
            foundsystem = null;

            bool dump = true;       // previously, we supported search. Now lets always use dump, but for now, keep search code below for safety

            foundsystem = EnsureSystemAddressAndName(searchsystem);     // find the full system details incl name and system address

            if (foundsystem == null)        // if failed, return nothing
                return null;

            //System.Diagnostics.Debug.WriteLine($"Spansh bodies found {foundsystem.Name} {foundsystem.SystemAddress}");

            BaseUtils.HttpCom.Response response = RequestGet("dump/" + foundsystem.SystemAddress.ToStringInvariant());

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshbodies.json", json?.ToString(true));

            JArray resultsarray;

            if (dump)
            {
                if (json["system"].Object()?.Contains("bodyCount") ?? false)
                    bodycount = json["system"]["bodyCount"].IntNull();

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
                        evt["StarSystem"] = foundsystem.Name;
                        evt["SystemAddress"] = foundsystem.SystemAddress.HasValue ? foundsystem.SystemAddress.Value : json["reference"]["id64"].Long();
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
                            string spanshst = so[dump ? "subType" : "subtype"].Str();
                            EDStar? startype = SpanshStarNameToEDStar(spanshst);
                            evt["StarType"] = startype != null ? Stars.ToEnglish(startype.Value) : spanshst;        // so if we recognise it, use the text version of the enum. ToEnum should turn it back
                            //System.Diagnostics.Debug.WriteLine($"Spansh star type {evt["StarType"].Str()}");

                            evt["StellarMass"] = so[dump ? "solarMasses" : "solar_masses"];
                            evt["AbsoluteMagnitude"] = so["absoluteMagnitude"];
                            evt["Luminosity"] = so[dump ? "luminosity" : "luminosity_class"];
                            if (so[dump ? "spectralClass" : "spectral_class"].Str().Length > 1)       // coded as G2
                                evt["Subclass"] = so[dump ? "spectralClass" : "spectral_class"].Str().Substring(1).InvariantParseIntNull();
                            evt["Age_MY"] = so["age"];
                        }
                        else if (so["type"].Str() == "Planet")
                        {
                            string spanshpc = so[dump ? "subType" : "subtype"].Str();
                            EDPlanet? planet = SpanshPlanetNameToEDPlanet(spanshpc);
                            evt["PlanetClass"] =planet != null ? Planets.ToEnglish(planet.Value) : spanshpc;      // if recognised, turn back to string, with _ removed.

                            //System.Diagnostics.Debug.WriteLine($"Spansh  planet class {spanshpc} -> {evt["PlanetClass"]}");
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

        public class BodiesResults
        {
            public ISystem System { get; set; }
            public List<JournalScan> Bodies { get; set; }
            public int? BodyCount { get; set; }
            public BodiesResults(ISystem sys, List<JournalScan> list, int? bodycount) { System = sys; Bodies = list; BodyCount = bodycount; }
        }


        // BodyCache gets either the body list, or null marking no server data
        static private Dictionary<string, BodiesResults> BodyCache = new Dictionary<string, BodiesResults>();

        public static void ClearBodyCache()
        {
            lock (BodyCache) 
            {
                BodyCache.Clear();
            }
        }

        // only one request at a time going, this is to prevent multiple requests for the same body
        public static bool HasBodyLookupOccurred(ISystem sys)
        {
            lock (BodyCache) 
            {
                return BodyCache.ContainsKey(sys.Key);
            }
        }
        // true if lookup occurred, but no data. false otherwise
        public static bool HasNoDataBeenStoredOnBody(ISystem sys)      
        {
            lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
            {
                return BodyCache.TryGetValue(sys.Key, out BodiesResults d) && d == null;
            }
        }

        // return list, if from cache, if web lookup occurred
        public async static System.Threading.Tasks.Task<BodiesResults> GetBodiesListAsync(ISystem sys, bool weblookup = true)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return GetBodiesList(sys, weblookup);
            });
        }

        // System can be name, name and systemaddress, or systemaddress only (from jan 25)
        static public BodiesResults GetBodiesList(ISystem sys, bool weblookup = true)
        {
            try
            {
                lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
                {
                    // System.Threading.Thread.Sleep(2000); //debug - delay to show its happening 
                    // System.Diagnostics.Debug.WriteLine("EDSM Cache check " + sys.EDSMID + " " + sys.SystemAddress + " " + sys.Name);

                    if (BodyCache.TryGetValue(sys.Key, out BodiesResults we))
                    {
                        System.Diagnostics.Debug.WriteLine($"Spansh Body Cache hit on {sys.Name} {sys.SystemAddress} {we != null}");
                        // will return null, looked up not found, or bodies results found
                        return we;
                    }

                    JArray jlist = null;
                    bool fromcache = false;

                    // calc name of cache file (14 jan 25 changed to v2)

                    string cachefile = EliteConfigInstance.InstanceOptions.ScanCacheEnabled ?
                            System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"spansh_{sys.Key.SafeFileString()}_v2.json") :
                            null;

                    int? bodycount = null;      // we may get this set from cache, from file or from lookup

                    if (cachefile != null && System.IO.File.Exists(cachefile))      // if we have that file
                    {
                        string cachedata = BaseUtils.FileHelpers.TryReadAllTextFromFile(cachefile); // try and read it
                        if (cachedata != null)
                        {
                            //System.Diagnostics.Debug.WriteLine($"Spansh Cache File read on {sys.Name} {sys.SystemAddress} from {cachefile}");
                            JObject jo = JObject.Parse(cachedata, JToken.ParseOptions.CheckEOL);  // if so, try a conversion
                            if (jo != null && jo.Contains("Bodies"))
                            {
                                jlist = jo["Bodies"].Array();               // v2 write an object with bodies array and bodycount int
                                if (jlist?.Count > 0)                       // if we have a valid list of bodies..
                                {
                                    bodycount = jo["BodyCount"].IntNull();
                                    fromcache = true;
                                    sys = new SystemClass(jlist[0].I("StarSystem").Str(), jlist[0].I("SystemAddress").LongNull(), SystemSource.FromSpansh);
                                }
                                else
                                    jlist = null;
                            }
                        }
                    }

                    if (jlist == null)          // no data as yet, look at web
                    {
                        if (!weblookup)         // must be set for a web lookup
                            return null;

                        //System.Diagnostics.Debug.WriteLine($"Spansh Web lookup on {sys.Name} {sys.SystemAddress}");

                        SpanshClass sp = new SpanshClass();

                        // get by id64, or name if required. sys will be replaced by the one from spansh
                        jlist = sp.GetBodies(sys , out bodycount, out ISystem foundsystem);

                        if (jlist != null)          // if we got a hit, the found system holds the full details
                            sys = foundsystem;
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

                        if (cachefile != null && fromcache == false)        // if write file back..
                        {
                            // give the finalised name to the cache file. If we have name only, we should be saving it under systemaddress
                            cachefile = System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"spansh_{(sys.Key.SafeFileString())}_v2.json");

                            JObject jo = new JObject();         // object with Bodies and BodyCount (v2)
                            jo["Bodies"] = jlist;   
                            if ( bodycount!=null)
                                jo["BodyCount"] = bodycount;

                            BaseUtils.FileHelpers.TryWriteToFile(cachefile, jo.ToString(true));      // save to file so we don't have to reload
                        }

                        // place the body in the cache under both its name and its system address. We return system normalised, bodies and bodycount (if known)
                        var cdata = new BodiesResults(sys, bodies, bodycount);
                        if (sys.Name.HasChars())
                            BodyCache[sys.Name] = cdata;
                        if (sys.SystemAddress.HasValue)
                            BodyCache[sys.SystemAddress.Value.ToStringInvariant()] = cdata;

                        // System.Diagnostics.Debug.WriteLine($"Spansh Web/File Lookup complete {sys.Name} {sys.SystemAddress} {bodies.Count} cache {fromcache}");
                        return cdata;
                    }
                    else
                    {
                        // System.Diagnostics.Debug.WriteLine($"Spansh Web Lookup complete no info {sys.Name} {sys.SystemAddress}");

                        if (sys.Name.HasChars())
                            BodyCache[sys.Name] = null;
                        if (sys.SystemAddress.HasValue)
                            BodyCache[sys.SystemAddress.Value.ToStringInvariant()] = null;
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
                System.Diagnostics.Trace.WriteLine($"*** SPANSH failed to decode star {name}");
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
                System.Diagnostics.Trace.WriteLine($"*** SPANSH failed to decode planet {name}");
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

