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

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // if processing later, sys will be null

        public bool AddCodexEntryToSystem(JournalCodexEntry jsd, bool saveprocessinglater = true)
        {
            if (jsd.SystemAddress.HasValue && jsd.SystemAddress >= 100000)       // system address 1 is found in early entries, a bug, so reject if small (arbitary 1000)
            {
                if (ScanDataBySysaddr.TryGetValue(jsd.SystemAddress.Value, out SystemNode sn))       // if we have it
                {
                    if (!sn.CodexEntryList.Contains(jsd))
                        sn.CodexEntryList.Add(jsd);
                }
                else if (saveprocessinglater)
                {
                    SaveForProcessing(jsd, null);
                }
            }
            else
            {
               // System.Diagnostics.Debug.WriteLine($"Reject codex entry {jsd.EventTimeUTC} {jsd.SystemAddress}");
            }

            return false;
        }
    }
}
