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

using EliteDangerousCore.JournalEvents;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // if processing later, sys will be null

        public bool AddScanOrganicToSystem(JournalScanOrganic jsaa, ISystem sys)
        {
            // jsaa should always have a system address.  If it matches current system, we can go for an immediate add
            if (sys.SystemAddress.HasValue && jsaa.SystemAddress == sys.SystemAddress.Value)
            {
                bool ret = ProcessScanOrganicFound(jsaa, sys);
                if (!ret)
                    SaveForProcessing(jsaa, sys);
                return ret;
            }
            else
            {
                SaveForProcessing(jsaa, sys);
            }
            return false;
        }

        private bool ProcessScanOrganicFound(JournalScanOrganic jsaa, ISystem sys)  // background or foreground.. FALSE if you can't process it
        {
            SystemNode sn = GetOrCreateSystemNode(sys);
            ScanNode relatednode = null;

            if (sn.NodesByID.ContainsKey((int)jsaa.Body)) // find by ID - thats the only thing we have
            {
                relatednode = sn.NodesByID[(int)jsaa.Body];
            }

            if (relatednode != null)
            {
                //System.Diagnostics.Debug.WriteLine("Setting Scan Organic Found for " + jsaa.Body + " @ " + sys.Name );
                if (relatednode.Organics == null)
                    relatednode.Organics = new List<JournalScanOrganic>();

                relatednode.Organics.Add(jsaa);     // we add, even if its a repeat, since we get multiple sample events
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
