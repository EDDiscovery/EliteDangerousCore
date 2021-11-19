/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using BaseUtils.JSON;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public partial class ScanNode
        {
            // Scan Node debugs and information

            public static void DumpTree(ScanNode top, string key, int level)        // debug dump out
            {
                System.Diagnostics.Debug.WriteLine("                                                        ".Substring(0, level * 3) + key + ":" + top.BodyID + " " + top.FullName + " " + top.NodeType + " SD:" + (top.scandata != null));
                if (top.Children != null)
                {
                    foreach (var c in top.Children)
                        DumpTree(c.Value, c.Key, level + 1);
                }
            }

            public static JObject DumpOrbitElements(ScanNode top)        // use with SystemNodeTree for orbital elements
            {
                JObject obj = new JObject();
                obj["Name"] = top.OwnName;
                obj["FullName"] = top.FullName;
                obj["NodeType"] = top.NodeType.ToString();
                if (top.BodyID.HasValue)
                {
                    obj["ID"] = top.BodyID.Value;
                }
                if (top.ScanData != null)
                {
                    obj["Epoch"] = top.ScanData.EventTimeUTC;

                    if (top.ScanData.IsStar)
                    {
                        obj["StarClass"] = top.ScanData.StarClassificationAbv;
                    }
                    else if (top.ScanData.IsPlanet)
                    {
                        obj["PlanetClass"] = top.ScanData.PlanetTypeID.ToString();
                    }

                    if ( top.ScanData.nRadius.HasValue)
                        obj["Radius"] = top.ScanData.nRadius / 1000.0;  // in km

                    if (top.scandata.nSemiMajorAxis.HasValue)       // if we have semi major axis, we have orbital elements
                    {
                        obj["SemiMajorAxis"] = top.ScanData.nSemiMajorAxis / 1000.0;        // in km
                        obj["Eccentricity"] = top.ScanData.nEccentricity;  // degrees
                        obj["Inclination"] = top.ScanData.nOrbitalInclination;  // degrees
                        obj["AscendingNode"] = top.ScanData.nAscendingNode; // degrees
                        obj["Periapis"] = top.ScanData.nPeriapsis;// degrees
                        obj["MeanAnomaly"] = top.scandata.nMeanAnomaly;// degrees
                        obj["OrbitalPeriod"] = top.scandata.nOrbitalPeriod;// in seconds
                    }

                    if ( top.scandata.nAxialTilt.HasValue )
                        obj["AxialTilt"] = top.scandata.nAxialTiltDeg;  // degrees

                    if (top.scandata.nRotationPeriod.HasValue)      // seconds
                        obj["RotationPeriod"] = top.scandata.nRotationPeriod;

                    if ( top.scandata.nMassKG.HasValue)
                        obj["Mass"] = top.scandata.nMassKG; // kg

                }

                if (top.Children != null && top.Children.Count>0)
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
