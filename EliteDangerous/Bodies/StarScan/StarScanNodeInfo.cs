/*
 * Copyright ® 2015 - 2022 EDDiscovery development team
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
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public partial class ScanNode
        {
            // Scan Node debugs and information

            public static void DumpTree(ScanNode top, string introtext, int level)        // debug dump out
            {
                string pad = new string(' ', 64);
                System.Diagnostics.Debug.WriteLine(pad.Substring(0, level * 3) + introtext + ": ID " + top.BodyID + " " + top.BodyDesignator + " : " + top.NodeType + (top.ScanData != null ? " : sd" : ""));
                if (top.Children != null)
                {
                    foreach (var c in top.Children)
                        DumpTree(c.Value, c.Key, level + 1);
                }
            }

            public static bool AllWithScanData(ScanNode top)
            {
                if (top.ScanData == null)
                    return false;
                if (top.Children != null)
                {
                    foreach (var c in top.Children)
                        if (!AllWithScanData(c.Value))
                            return false;
                }
                return true;
            }

            // Dumps out orbital parameters. Distance is km, as per Kepler Orbital Parameters/Horizons
            public static JObject DumpOrbitElements(ScanNode top)
            {
                JObject obj = new JObject();
                obj["NodeType"] = top.NodeType.ToString();
                obj["OwnName"] = top.OwnName;
                obj["BodyName"] = top.BodyNameOrBodyDesignator;
                obj["BodyDesignator"] = top.BodyDesignator;
                if (top.BodyID.HasValue)
                {
                    obj["ID"] = top.BodyID.Value;
                }

                obj["ScanDataPresent"] = top.ScanData != null;         // one key to indicate data is available

                if (top.ScanData != null)
                {
                    obj["Epoch"] = top.ScanData.EventTimeUTC;
                    obj["DistanceFromArrival"] = top.ScanData.DistanceFromArrivalm / 1000.0;        // in km

                    if (top.NodeType == ScanNodeType.belt)
                        obj["BodyType"] = "Belt";
                    else if (top.NodeType == ScanNodeType.beltcluster)
                        obj["BodyType"] = "BeltCluster";
                    else if (top.NodeType == ScanNodeType.ring)
                        obj["BodyType"] = "Ring";
                    else if (top.ScanData.IsStar)
                    {
                        obj["BodyType"] = "Star";
                        obj["StarType"] = top.ScanData.StarTypeID.ToString();
                        obj["StarImage"] = top.ScanData.StarTypeImageName;
                        obj["StarClass"] = top.ScanData.StarClassificationAbv;
                        obj["StarAbsMag"] = top.ScanData.nAbsoluteMagnitude;
                        obj["StarAge"] = top.ScanData.nAge;
                    }
                    else if (top.ScanData.IsPlanet)
                    {
                        obj["BodyType"] = "Planet";
                        obj["PlanetClass"] = top.ScanData.PlanetTypeID.ToString();
                        obj["PlanetImage"] = top.ScanData.PlanetClassImageName;
                    }

                    if (top.ScanData.nRadius.HasValue)
                        obj["Radius"] = top.ScanData.nRadius / 1000.0;  // in km

                    if (top.ScanData.nSemiMajorAxis.HasValue)       // if we have semi major axis, we have orbital elements
                    {
                        obj["SemiMajorAxis"] = top.ScanData.nSemiMajorAxis / 1000.0;        // in km
                        obj["Eccentricity"] = top.ScanData.nEccentricity;  // degrees
                        obj["Inclination"] = top.ScanData.nOrbitalInclination;  // degrees
                        obj["AscendingNode"] = top.ScanData.nAscendingNodeKepler; // degrees
                        obj["Periapis"] = top.ScanData.nPeriapsisKepler;// degrees
                        obj["MeanAnomaly"] = top.ScanData.nMeanAnomaly;// degrees
                        obj["OrbitalPeriod"] = top.ScanData.nOrbitalPeriod;// in seconds
                    }

                    if (top.ScanData.nAxialTilt.HasValue)
                        obj["AxialTilt"] = top.ScanData.nAxialTiltDeg;  // degrees

                    if (top.ScanData.nRotationPeriod.HasValue)      // seconds
                        obj["RotationPeriod"] = top.ScanData.nRotationPeriod;

                    if (top.ScanData.nMassKG.HasValue)
                        obj["Mass"] = top.ScanData.nMassKG; // kg

                    if (top.ScanData.nSurfaceTemperature.HasValue)
                        obj["SurfaceTemp"] = top.ScanData.nSurfaceTemperature;

                    if (top.ScanData.nSurfaceTemperature.HasValue)
                        obj["SurfaceTemp"] = top.ScanData.nSurfaceTemperature;

                    if (top.ScanData.HasRings)
                    {
                        JArray ringarray = new JArray();
                        foreach (var r in top.ScanData.Rings)
                        {
                            JObject jo = new JObject()
                            {
                                ["Type"] = r.RingClassID.ToString(),
                                ["InnerRad"] = r.InnerRad / 1000.0,  // in km
                                ["OuterRad"] = r.OuterRad / 1000.0,    // in km
                                ["MassMT"] = r.MassMT
                            };
                            ringarray.Add(jo);
                        }
                        obj["Rings"] = ringarray;
                    }

                }

                if (top.Children != null && top.Children.Count > 0)
                {
                    JArray ja = new JArray();
                    foreach (var kvp in top.Children)
                    {
                        ja.Add(DumpOrbitElements(kvp.Value));
                    }

                    obj["Bodies"] = ja;
                }

                return obj;
            }

        };


    }
}
