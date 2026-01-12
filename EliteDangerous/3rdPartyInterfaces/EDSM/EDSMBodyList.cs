/*
 * Copyright © 2016-2023 EDDiscovery development team
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

namespace EliteDangerousCore.EDSM
{
    public partial class EDSMClass
    {
        public class BodiesResults
        {
            public ISystem System { get; set; }
            public List<JournalScan> Bodies { get; set; }
            public BodiesResults(ISystem sys, List<JournalScan> list) { System = sys; Bodies = list;  }
        }

        // get this system
        public async static System.Threading.Tasks.Task<BodiesResults> GetBodiesListAsync(ISystem sys, bool edsmweblookup = true) 
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return GetBodiesList(sys, edsmweblookup);
            });
        }

        // EDSMBodiesCache gets either the body list, or null marking no EDSM server data
        static private Dictionary<string, BodiesResults> BodyCache = new Dictionary<string, BodiesResults>();

        public static void ClearBodyCache()
        {
            lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
            {
                BodyCache.Clear();
            }
        }

        public static bool HasBodyLookupOccurred(ISystem sys)
        {
            lock (BodyCache)
            {
                return BodyCache.ContainsKey(sys.Key);
            }
        }
        public static bool HasNoDataBeenStoredOnBody(ISystem sys)      // true if lookup occurred, but no data. false otherwise
        {
            lock (BodyCache)
            {
                return BodyCache.TryGetValue(sys.Key, out BodiesResults d) && d == null;
            }
        }

        // returns null if EDSM says not there, else if returns list of bodies and a flag indicating if from cache. 
        // all this is done in a lock inside a task - the only way to sequence the code and prevent multiple lookups in an await structure
        // so we must pass back all the info we can to tell the caller what happened.

        // System can be name, name and systemaddress, or systemaddress only (from jan 25)

        public static BodiesResults GetBodiesList(ISystem sys, bool weblookup = true)
        {
            try
            {
                lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
                {
                    // System.Threading.Thread.Sleep(2000); //debug - delay to show its happening 
                    // System.Diagnostics.Debug.WriteLine("EDSM Cache check " + sys.EDSMID + " " + sys.SystemAddress + " " + sys.Name);

                    if (BodyCache.TryGetValue(sys.Key, out BodiesResults we))
                    {
                        System.Diagnostics.Debug.WriteLine($"EDSM Body Cache hit on {sys.Name} {sys.SystemAddress} {we != null}");
                        // will return null, looked up not found, or bodies results found
                        return we;
                    }

                    JObject jobj = null;
                    bool fromcache = false;

                    // calc name of cache file

                    string cachefile = EliteConfigInstance.InstanceOptions.ScanCacheEnabled ?
                            System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"edsm_{sys.Key.SafeFileString()}.json") :
                            null;

                    if (cachefile != null && System.IO.File.Exists(cachefile))      // if we have that file
                    {
                        string cachedata = BaseUtils.FileHelpers.TryReadAllTextFromFile(cachefile); // try and read it
                        if (cachedata != null)
                        {
                            //System.Diagnostics.Debug.WriteLine($"EDSM Cache File read on {sys.Name} {sys.SystemAddress} from {cachefile}");
                            jobj = JObject.Parse(cachedata, JToken.ParseOptions.CheckEOL);  // if so, try a conversion
                            if (jobj?.Count > 0 && jobj.Contains("name") && jobj.Contains("bodies"))        // if we have a body list and name
                            {
                                fromcache = true;
                                sys = new SystemClass(jobj["name"].Str(), jobj["id64"].LongNull(), SystemSource.FromEDSM);
                            }
                            else
                                jobj = null;   
                        }
                    }

                    if (jobj == null)
                    {
                        if (!weblookup)      // must be set for a web lookup
                            return null;

                        // sys = new SystemClass("Solx"); // debug

                        System.Diagnostics.Debug.WriteLine($"EDSM Web lookup on `{sys.Name}` {sys.SystemAddress}");

                        EDSMClass edsm = new EDSMClass();

                        if (sys.SystemAddress != null && sys.SystemAddress > 0)
                            jobj = edsm.GetBodiesByID64(sys.SystemAddress.Value);
                        else if (sys.Name != null)
                            jobj = edsm.GetBodies(sys.Name);

                        // make sure we have valid result, and not just an empty object

                        if (jobj != null && jobj.Contains("name"))       
                        {
                            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\edsmbodies.json", jobj?.ToString(true));

                            // make sure we have a sys from the results not the system we came in with as it may lack data
                            sys = new SystemClass(jobj["name"].Str(), jobj["id64"].Long(), SystemSource.FromEDSM);
                        }
                        else
                            jobj = null;           // ensure null at this point
                    }

                    if (jobj != null && jobj["bodies"] != null)
                    {
                        List<JournalScan> bodies = new List<JournalScan>();

                        foreach (JObject edsmbody in jobj["bodies"])
                        {
                            try
                            {
                                JObject jbody = EDSMClass.ConvertFromEDSMBodies(edsmbody,sys.SystemAddress);

                                JournalScan js = new JournalScan(jbody);

                                //System.Diagnostics.Debug.WriteLine($"EDSM JS: {js.DisplayString(null, null)}");

                                bodies.Add(js);
                            }
                            catch (Exception ex)
                            {
                                WriteLog($"EDSM Decode Bodies Exception: {ex.Message}");
                            }
                        }

                        if (cachefile != null && fromcache == false)      // note its cached in EDSM format.. Spansh caches it in journal format
                        {
                            // give the finalised name to the cache file. If we have name only, we should be saving it under systemaddress
                            cachefile = System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, $"edsm_{sys.Key.SafeFileString()}.json");

                            BaseUtils.FileHelpers.TryWriteToFile(cachefile, jobj.ToString(true));      // save to file so we don't have to reload
                        }

                        var cdata = new BodiesResults(sys, bodies);
                        if (sys.Name.HasChars())
                            BodyCache[sys.Name.ToLowerInvariant()] = cdata;
                        if (sys.SystemAddress.HasValue)
                            BodyCache[sys.SystemAddress.Value.ToStringInvariant()] = cdata;

                        System.Diagnostics.Debug.WriteLine($"EDSM Lookup complete {sys.Name} {sys.SystemAddress} {bodies.Count} cache {fromcache}");
                        return cdata;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"EDSM Web Lookup complete no info {sys.Name}");
                        if (sys.Name.HasChars())
                            BodyCache[sys.Name.ToLowerInvariant()] = null;
                        if (sys.SystemAddress.HasValue)
                            BodyCache[sys.SystemAddress.Value.ToStringInvariant()] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"EDSM Body Extraction exception: {ex.Message}");
            }

            return null;
        }

        // Verified Nov 20,  by scan panel
        private static JObject ConvertFromEDSMBodies(JObject jo, long? sysaddr)        // protect yourself against bad JSON
        {
            //System.Diagnostics.Debug.WriteLine($"EDSM Body {jo.ToString(true)}");
            JObject jout = new JObject
            {
                ["timestamp"] = DateTime.UtcNow.ToStringZuluInvariant(),
                ["event"] = "Scan",
                ["EDDFromEDSMBodie"] = true,
                ["BodyName"] = jo["name"],
                ["WasDiscovered"] = true,
                ["WasMapped"] = false,
                ["ScanType"] = "Detailed",
            };

            if (sysaddr != null)
                jout["SystemAddress"] = sysaddr.Value;

            if (!jo["discovery"].IsNull())       // much more defense around this.. EDSM gives discovery=null back
            {
                jout["discovery"] = jo["discovery"];
            }

            if (jo["orbitalInclination"].DoubleNull() != null) jout["OrbitalInclination"] = jo["orbitalInclination"];
            if (jo["orbitalEccentricity"].DoubleNull() != null) jout["Eccentricity"] = jo["orbitalEccentricity"];
            if (jo["argOfPeriapsis"].DoubleNull() != null) jout["Periapsis"] = jo["argOfPeriapsis"];
            if (jo["semiMajorAxis"].Double() != 0) jout["SemiMajorAxis"] = jo["semiMajorAxis"].Double() * BodyPhysicalConstants.oneAU_m; // AU -> metres
            if (jo["orbitalPeriod"].Double() != 0) jout["OrbitalPeriod"] = jo["orbitalPeriod"].Double() * BodyPhysicalConstants.oneDay_s; // days -> seconds
            if (jo["rotationalPeriodTidallyLocked"].BoolNull() != null) jout["TidalLock"] = jo["rotationalPeriodTidallyLocked"];
            if (jo["axialTilt"].DoubleNull() != null) jout["AxialTilt"] = jo["axialTilt"].Double() * Math.PI / 180.0; // degrees -> radians
            if (jo["rotationalPeriod"].Double() != 0) jout["RotationalPeriod"] = jo["rotationalPeriod"].Double() * BodyPhysicalConstants.oneDay_s; // days -> seconds
            if (jo["surfaceTemperature"].DoubleNull() != null) jout["SurfaceTemperature"] = jo["surfaceTemperature"];
            if (jo["distanceToArrival"].DoubleNull() != null) jout["DistanceFromArrivalLS"] = jo["distanceToArrival"];
            if (jo["parents"].Array() != null) jout["Parents"] = jo["parents"];
            if (jo["id64"].ULongNull() != null) jout["BodyID"] = jo["id64"].Long() >> 55;

            if (!jo["type"].IsNull())
            {
                if (jo["type"].Str().Equals("Star"))
                {
                    jout["StarType"] = EDSMStar2JournalName(jo["subType"].StrNull());           // pass thru null to code, it will cope with it
                    jout["Age_MY"] = jo["age"];
                    jout["StellarMass"] = jo["solarMasses"];
                    jout["Radius"] = jo["solarRadius"].Double() * BodyPhysicalConstants.oneSolRadius_m; // solar-rad -> metres
                }
                else if (jo["type"].Str().Equals("Planet"))
                {
                    jout["Landable"] = jo["isLandable"];
                    jout["MassEM"] = jo["earthMasses"];

                    jout["SurfaceGravity"] = jo["gravity"].Double() * BodyPhysicalConstants.oneGee_m_s2;      // if not there, we get 0

                    string vol = jo["volcanismType"].StrNull();
                    if (vol.HasChars() && !vol.EqualsIIC("No volcanism"))       // journal has this missing if no volcanism, so don't include it
                    {
                        if (vol.IndexOf("volcanism", StringComparison.InvariantCultureIgnoreCase) == -1)
                            vol += " volcanism";
                        jout["Volcanism"] = vol;
                    }

                    string atmos = jo["atmosphereType"].StrNull();
                    if (atmos.HasChars() && !atmos.EqualsIIC("No atmosphere"))
                    {
                        //if (atmos.IndexOf("atmosphere", StringComparison.InvariantCultureIgnoreCase) == -1) // journal does not have atmosphere on some values
                        //    atmos += " atmosphere";
                        jout["Atmosphere"] = atmos;
                    }

                    //System.Diagnostics.Debug.WriteLine($"EDSM reads {jout["BodyName"]} atmos `{jout["Atmosphere"]}` vol `{jout["Volcanism"]}`");

                    jout["Radius"] = jo["radius"].Double() * 1000.0; // km -> metres
                    jout["PlanetClass"] = EDSMPlanet2JournalName(jo["subType"].Str());
                    if (jo["terraformingState"] != null) jout["TerraformState"] = jo["terraformingState"];
                    if (jo["surfacePressure"] != null) jout["SurfacePressure"] = jo["surfacePressure"].Double() * BodyPhysicalConstants.oneAtmosphere_Pa; // atmospheres -> pascals
                    if (jout["TerraformState"].Str() == "Candidate for terraforming")
                        jout["TerraformState"] = "Terraformable";
                }
            }


            JArray rings = (jo["belts"] ?? jo["rings"]) as JArray;

            if (!rings.IsNull())
            {
                JArray jring = new JArray();

                foreach (JObject ring in rings)
                {
                    jring.Add(new JObject
                    {
                        ["InnerRad"] = ring["innerRadius"].Double() * 1000,
                        ["OuterRad"] = ring["outerRadius"].Double() * 1000,
                        ["MassMT"] = ring["mass"],
                        ["RingClass"] = ring["type"].Str().Replace(" ", ""),// turn to string, and EDSM reports "Metal Rich" etc so get rid of space
                        ["Name"] = ring["name"]
                    });
                }

                jout["Rings"] = jring;
            }

            if (!jo["materials"].IsNull())  // Check if materials has stuff
            {
                Dictionary<string, double?> mats;
                Dictionary<string, double> mats2;
                mats = jo["materials"]?.ToObjectQ<Dictionary<string, double?>>();
                mats2 = new Dictionary<string, double>();

                foreach (string key in mats.Keys)
                {
                    if (mats[key] == null)
                        mats2[key.ToLowerInvariant()] = 0.0;
                    else
                        mats2[key.ToLowerInvariant()] = mats[key].Value;
                }

                jout["Materials"] = JObject.FromObject(mats2);
            }

            return jout;
        }

        private static Dictionary<string, string> EDSM2PlanetNames = new Dictionary<string, string>()
        {
            // EDSM name    (lower case)            Journal name                  
            { "rocky ice world",                    "Rocky ice body" },
            { "high metal content world" ,          "High metal content body"},
            { "class i gas giant",                  "Sudarsky class I gas giant"},
            { "class ii gas giant",                 "Sudarsky class II gas giant"},
            { "class iii gas giant",                "Sudarsky class III gas giant"},
            { "class iv gas giant",                 "Sudarsky class IV gas giant"},
            { "class v gas giant",                  "Sudarsky class V gas giant"},
            { "earth-like world",                   "Earthlike body" },
        };

        private static Dictionary<string, string> EDSM2StarNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            // EDSM name (lower case)               Journal name
            { "a (blue-white super giant) star", "A_BlueWhiteSuperGiant" },
            { "b (blue-white super giant) star", "B_BlueWhiteSuperGiant" },
            { "f (white super giant) star", "F_WhiteSuperGiant" },
            { "g (white-yellow super giant) star", "G_WhiteSuperGiant" },
            { "k (yellow-orange giant) star", "K_OrangeGiant" },
            { "m (red giant) star", "M_RedGiant" },
            { "m (red super giant) star", "M_RedSuperGiant" },
            { "black hole", "H" },
            { "c star", "C" },
            { "cj star", "CJ" },
            { "cn star", "CN" },
            { "herbig ae/be star", "AeBe" },
            { "ms-type star", "MS" },
            { "neutron star", "N" },
            { "s-type star", "S" },
            { "t tauri star", "TTS" },
            { "wolf-rayet c star", "WC" },
            { "wolf-rayet n star", "WN" },
            { "wolf-rayet nc star", "WNC" },
            { "wolf-rayet o star", "WO" },
            { "wolf-rayet star", "W" },
        };

        private static string EDSMPlanet2JournalName(string inname)
        {
            return EDSM2PlanetNames.ContainsKey(inname.ToLowerInvariant()) ? EDSM2PlanetNames[inname.ToLowerInvariant()] : inname;
        }

        private static string EDSMStar2JournalName(string startype)
        {
            if (startype == null)
                startype = "Unknown";
            else if (EDSM2StarNames.ContainsKey(startype))
                startype = EDSM2StarNames[startype];
            else if (startype.StartsWith("White Dwarf (", StringComparison.InvariantCultureIgnoreCase))
            {
                int start = startype.IndexOf("(") + 1;
                int len = startype.IndexOf(")") - start;
                if (len > 0)
                    startype = startype.Substring(start, len);
            }
            else   // Remove extra text from EDSM   ex  "F (White) Star" -> "F"
            {
                int index = startype.IndexOf("(");
                if (index > 0)
                    startype = startype.Substring(0, index).Trim();
            }
            return startype;
        }


    }
}
