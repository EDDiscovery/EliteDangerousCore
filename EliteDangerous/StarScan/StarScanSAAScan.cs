/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // used by historylist directly for a single update during play, in foreground..  Also used by above.. so can be either in fore/back
        public bool AddSAAScanToBestSystem(JournalSAAScanComplete jsaa, ISystem sys, int startindex, List<HistoryEntry> hl)
        {
            // jsaa may have a system address.  If it matches current system, we can go for an immediate add
            if (jsaa.SystemAddress == sys.SystemAddress)
            {
                return ProcessSAAScan(jsaa, sys);
            }
            else
            {
                if (jsaa.BodyName == null)
                    return false;

                var best = FindBestSystem(startindex, hl, jsaa.SystemAddress, jsaa.BodyName, jsaa.BodyID, false);

                if (best == null)
                    return false;
                else
                    return ProcessSAAScan(jsaa, best.Item2);
            }
        }

        private bool ProcessSAAScan(JournalSAAScanComplete jsaa, ISystem sys, bool saveprocessinglater = true)  // background or foreground.. FALSE if you can't process it
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                ScanNode relatednode = null;

                if (systemnode.NodesByID.ContainsKey((int)jsaa.BodyID))
                {
                    relatednode = systemnode.NodesByID[(int)jsaa.BodyID];
                }
                else
                {
                    foreach (var body in systemnode.Bodies)
                    {
                        if ((body.FullName == jsaa.BodyName || body.CustomName == jsaa.BodyName) &&
                            (body.FullName != sys.Name || body.Level != 0))
                        {
                            relatednode = body;
                            break;
                        }
                    }
                }

                if (relatednode != null)
                {
                    relatednode.SetMapped(jsaa.ProbesUsed <= jsaa.EfficiencyTarget);
                    //System.Diagnostics.Debug.WriteLine("Setting SAA Scan for " + jsaa.BodyName + " " + sys.Name + " to Mapped: " + relatednode.WasMappedEfficiently);

                    if (relatednode.ScanData != null)       // if we have a scan, set its values - this keeps the calculation self contained in the class.
                    {
                        relatednode.ScanData.SetMapped(relatednode.IsMapped, relatednode.WasMappedEfficiently);
                        //System.Diagnostics.Debug.WriteLine(".. passing down to scan " + relatedScan.ScanData.ScanType);
                    }
                    else if (saveprocessinglater)
                    {
                        SaveForProcessing(jsaa, sys);
                    }

                    return true; // We already have the scan
                }
                else
                {
                    if (saveprocessinglater)
                        SaveForProcessing(jsaa, sys);
                    //System.Diagnostics.Debug.WriteLine("No body to attach data found for " + jsaa.BodyName + " @ " + sys.Name + " body " + jsaa.BodyDesignation);
                }
            }

            return false;
        }


        #region FSS DISCOVERY *************************************************************

        public void SetFSSDiscoveryScan(JournalFSSDiscoveryScan je, ISystem sys)
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);
            lock (systemnode )
                systemnode.FSSTotalBodies = je.BodyCount;
                systemnode.FSSTotalNonBodies = je.NonBodyCount;
        }

        #endregion

    }
}
