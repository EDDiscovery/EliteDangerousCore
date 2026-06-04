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

using QuickJSON;
using System;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        // convert a spansh scan node to a JournalScan compatible JSON
        // may return null if not liked
        private static JObject ConvertToJournalScan(JToken so, string sysname, long sysaddr)
        {
            bool dump = true;
            try
            {
                // this follows the order found in JournalScan constructor

                // so["stations"] = null;   System.Diagnostics.Debug.WriteLine($"Spansh Body JSON {so.ToString()}");

                JObject evt = new JObject();
                evt["ScanType"] = "Detailed";
                evt["BodyName"] = so["name"];

                evt["BodyID"] = so[dump ? "bodyId" : "body_id"];
                evt["StarSystem"] = sysname;
                evt["SystemAddress"] = sysaddr;
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
                            ["InnerRad"] = node["innerRadius"],
                            ["OuterRad"] = node["outerRadius"],
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
                    if (spanshst.HasChars())
                    {
                        EDStar? startype = SpanshStarNameToEDStar(spanshst);
                        evt["StarType"] = startype != null ? Stars.ToEnglish(startype.Value) : spanshst;        // so if we recognise it, use the text version of the enum. ToEnum should turn it back
                    }
                    else
                        evt["StarType"] = "Unknown";

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

                    if (spanshpc.HasChars())
                    {
                        EDPlanet? planet = SpanshPlanetNameToEDPlanet(spanshpc);
                        evt["PlanetClass"] = planet != null ? Planets.ToEnglish(planet.Value) : spanshpc;      // if recognised, turn back to string, with _ removed.
                    }
                    else
                        evt["PlanetClass"] = "Unknown";

                    //System.Diagnostics.Debug.WriteLine($"Spansh  planet class {spanshpc} -> {evt["PlanetClass"]}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Spansh bodies ignore {so["type"].Str()}");
                    return null;
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

                return evt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Spansh read bodies exception {ex}");
            }

            return null;
        }
    }
}

