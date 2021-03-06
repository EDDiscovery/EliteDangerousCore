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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using EliteDangerousCore.JournalEvents;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // if processing later, sys will be null

        public bool AddFSSSignalsDiscoveredToSystem(JournalFSSSignalDiscovered jsd, bool saveprocessinglater = true)
        {
            foreach (var fs in jsd.Signals)
            {
                if (fs.SystemAddress.HasValue)      // must have value, all FSS Signals Discovered always had a system address, and the Journal Scan should have it too
                {
                    if (ScanDataBySysaddr.TryGetValue(fs.SystemAddress.Value, out SystemNode sn))       // if we have it
                    {
                        int indexprev = sn.FSSSignalList.FindIndex(x => x.IsSame(fs));
                        if (indexprev == -1)                       // if not found in signal list, store
                            sn.FSSSignalList.Add(fs);
                        else
                            sn.FSSSignalList[indexprev] = fs;       // replace, it may be more up to date info, such as a carrier info
                    }
                    else if (saveprocessinglater)
                    {
                        SaveForProcessing(jsd, null);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
