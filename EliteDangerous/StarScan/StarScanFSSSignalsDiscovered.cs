/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public bool AddFSSSignalsDiscoveredToSystem(JournalFSSSignalDiscovered jsd, ISystem sys)  // background or foreground.. FALSE if you can't process it
        {
            SystemNode sn = GetOrCreateSystemNode(sys);

            foreach( var fs in jsd.Signals)
            {
                int indexprev = sn.FSSSignalList.FindIndex(x => x.IsSame(fs));
                if ( indexprev == -1)                       // if not found in signal list, store
                    sn.FSSSignalList.Add(fs);   
                else
                    sn.FSSSignalList[indexprev] = fs;       // replace, it may be more up to date info, such as a carrier info
            }

            return false;
        }
    }
}
