/*
 * Copyright 2025 - 2025 EDDiscovery development team
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
using System.Web.UI.WebControls;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // works with ApproachTouchdown to fill in a surface list

        public void AddDocking(JournalDocked dck, ISystem sys, int bodyid)
        {
            //System.Diagnostics.Debug.WriteLine($"Docked at planet port {bodyid} {dck.StationName}");

            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                // caller has already screened bodyid,systemaddress.  Also body has already been added so this should always work!

                if (systemnode.NodesByID.TryGetValue(bodyid, out ScanNode node))
                {
                    if (node.SurfaceFeatures == null)
                        node.SurfaceFeatures = new List<IBodyFeature>();

                    // see if we have another settlement with this name (Note touchdowns get name "Touchdown bodyname @ UTC")

                    var existingnode = node.SurfaceFeatures.Find(x => x.Name == dck.StationName);

                    if (existingnode != null)                           // already seen this, we have a docking. 
                    {
                        dck.Latitude = existingnode.Latitude;           // copy data from these into docked to fill it up (first approach will be there, then dock,dock,dock repeat)
                        dck.Longitude = existingnode.Longitude;
                        dck.Body = existingnode.Body;
                        dck.BodyType = existingnode.BodyType;
                        dck.BodyID = existingnode.BodyID;
                        dck.BodyDesignation = existingnode.BodyDesignation;

                        node.SurfaceFeatures.Remove(existingnode);      // replace both types with docking - fuller data
                        node.SurfaceFeatures.Add(dck);

                        if (node.ScanData != null)
                        {
                            node.ScanData.SurfaceFeatures = node.SurfaceFeatures;       // make sure Scan node has same list as subnode
                        }

                       // System.Diagnostics.Debug.WriteLine($"StarScan Augmented docking {existingnode.Name} {dck.Latitude} {dck.Longitude}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"StarScan Docking Not Found existing entry {dck.StationName}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"StarScan Docking Not Found body {bodyid}");
                }
            }
        }

        public void AddDocking(JournalDocked dck, ISystem sys)
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                var existingnode = systemnode.Stations.Find(x => x.Name == dck.StationName);

                if (existingnode != null)                           // already seen this, we have a docking. 
                    systemnode.Stations.Remove(existingnode);

                systemnode.Stations.Add(dck);
               // System.Diagnostics.Debug.WriteLine($"StarScan Augmented docking {dck.Name} for {sys.Name}");
            }
        }


    }
}
