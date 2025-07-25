﻿/*
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
 */

using EliteDangerousCore.JournalEvents;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public void AddApproachSettlement(JournalApproachSettlement je, ISystem sys)
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                // caller has already screened bodyid,systemaddress.  Also body has already been added so this should always work!

                if (systemnode.NodesByID.TryGetValue(je.BodyID.Value, out ScanNode node))
                {
                    if (node.SurfaceFeatures == null)
                        node.SurfaceFeatures = new List<IBodyFeature>();

                    // see if we have another settlement with this name (Note touchdowns get name "Touchdown bodyname @ UTC")

                    var existingnode = node.SurfaceFeatures.Find(x => x.Name == je.Name);

                    if (existingnode != null)       // already seen this
                    {
                        if (je.HasLatLong && existingnode.HasLatLong)      // bug in 16.1 - must both have value to make a call on changing the lat/long
                        {
                            if (System.Math.Abs(existingnode.Latitude.Value - je.Latitude.Value) > 0.01 || System.Math.Abs(existingnode.Longitude.Value - je.Longitude.Value) > 0.01)
                            {
                                //System.Diagnostics.Debug.WriteLine($"Starscan approach settlement new pos {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                                existingnode.Latitude = je.Latitude;
                                existingnode.Longitude = je.Longitude;
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine($"Starscan approach settlement duplicate {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                            }
                        }
                    }
                    else
                    {
                        // if we have lat long, lets see if we have a touchdown position record near it
                        // this handles je not having lat/long
                        existingnode = node.FindSurfaceFeatureNear(je.Latitude, je.Longitude);

                        if (existingnode is JournalTouchdown)             // okay we touched down near it, lets remove the touchdown and keep the approach
                        {
                            //System.Diagnostics.Debug.WriteLine($"Starscan approach settlement touchdown removal {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                            node.SurfaceFeatures.Remove(existingnode);
                        }

                        node.SurfaceFeatures.Add(je);       // add new approach

                        if (node.ScanData != null)
                        {
                            node.ScanData.SurfaceFeatures = node.SurfaceFeatures;       // make sure Scan node has same list as subnode
                        }

                        //System.Diagnostics.Debug.WriteLine($"Starscan new approach settlement {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Starscan approach no-body {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.BodyName} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                }
            }
        }

        public void AddTouchdown(JournalTouchdown je, ISystem sys)
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                // caller has already screened bodyid,systemaddress.  Also body has already been added so this should always work!

                if (systemnode.NodesByID.TryGetValue(je.BodyID.Value, out ScanNode node))
                {
                    // this handles je not having lat/long..
                    var existingibf = node.FindSurfaceFeatureNear(je.Latitude, je.Longitude);

                    if (existingibf == null)
                    {
                        if (node.SurfaceFeatures == null)
                            node.SurfaceFeatures = new List<IBodyFeature>();

                        node.SurfaceFeatures.Add(je);

                        if (node.ScanData != null)
                        {
                            node.ScanData.SurfaceFeatures = node.SurfaceFeatures;       // make sure Scan node has same list as subnode
                        }

                        // System.Diagnostics.Debug.WriteLine($"Starscan new touchdown {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude}");
                    }
                    else if (existingibf is JournalTouchdown)     // touchdown, after this touchdown
                    {
                        //System.Diagnostics.Debug.WriteLine($"Starscan touchdown near previous touchdown replaced it {existingibf.EventTimeUTC} -> {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude} {je.NearestDestination}");
                        // update the entry..
                        node.SurfaceFeatures[node.SurfaceFeatures.IndexOf(existingibf)] = je;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"Starscan touchdown rejected as near approach settlement {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Name} {je.Name_Localised} {je.Latitude} {je.Longitude} {je.NearestDestination}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Starscan touchdown no-body {je.EventTimeUTC} {je.CommanderId} {sys.Name} {je.BodyID} {je.Body} {je.Latitude} {je.Longitude}");
                }
            }
        }

        // print list, with optional indent, and separ.  Separ is not placed on last entry
        static public void SurfaceFeatureList(System.Text.StringBuilder sb, List<IBodyFeature> list, int indent, bool indentfirst, string separ = ", ")        // default is environment.newline
        {
            string inds = new string(' ', indent);

            int index = 0;
            foreach (var ibf in list)
            {
                //System.Diagnostics.Debug.WriteLine($"{s.ScanType} {s.Genus_Localised} {s.Species_Localised}");
                if (indent > 0 && (index > 0 || indentfirst) )       // if indent, and its either not first or allowed to indent first
                    sb.Append(inds);

                sb.Append($"{ EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(ibf.EventTimeUTC)} : {ibf.Name_Localised ?? ibf.Name} {ibf.Latitude:0.####}, {ibf.Longitude:0.####}");

                if (index++ < list.Count-1)     // if another to go, separ
                    sb.Append(separ);
            }
        }

        static public bool SurfaceFeatureListContainsSettlements(List<IBodyFeature> list)
        {
            return list != null && list.FindIndex(x => x is JournalApproachSettlement) >= 0;
        }

        static public int SurfaceFeatureListSettlementsCount(List<IBodyFeature> list)
        {
            return list != null ? list.Count(x => x is JournalApproachSettlement) : 0;
        }

    }
}
