/*
 * Copyright © 2021 - 2021 EDDiscovery development team
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
 
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // used by historylist directly for a single update during play, in foreground..  Also used by above.. so can be either in fore/back
        public bool AddBaryCentre(JournalScanBaryCentre jsa, ISystem sys, bool saveit = true)
        {
            if (ScanDataBySysaddr.TryGetValue(jsa.SystemAddress, out SystemNode sn))       // if we have it
            {
              //  System.Diagnostics.Debug.WriteLine($"Add barycentre {jsa.StarSystem} {jsa.BodyID}");
                sn.BaryCentres[jsa.BodyID] = jsa;        // add it

                // find any stored scans associated with this scanbarycentre and assign

                var scannodelist = sn.Bodies.Where(x => ((x.ScanData?.Parents?.Count ?? 0) > 0) && x.ScanData.Parents[0].BodyID == jsa.BodyID);     // find all

                foreach( var scannode in scannodelist)
                {
                    System.Diagnostics.Debug.WriteLine($"Barycentre in {sys.Name} BC {jsa.BodyID} attach to {scannode.ScanData.BodyName}");
                    scannode.ScanData.Barycentre = jsa;     // assign to barycentre
                }

                return true;
            }
            else 
            {
                if (saveit)
                {
              //      System.Diagnostics.Debug.WriteLine($"SAVE barycentre {jsa.StarSystem} {jsa.BodyID}");
                    SaveForProcessing(jsa, sys);
                }
                return false;
            }
        }
    }
}
