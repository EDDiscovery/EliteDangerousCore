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
        public bool AddFSSBodySignalsToSystem(JournalFSSBodySignals jsaa, ISystem sys)
        {
            // jsd should always have a system address.  If it matches current system, we can go for an immediate add
            if (sys.SystemAddress.HasValue && jsaa.SystemAddress == sys.SystemAddress.Value)
            {
                bool ret = ProcessSignalsFound(jsaa.BodyID.Value,jsaa.BodyName,jsaa.Signals, sys);
                if (!ret)
                    SaveForProcessing(jsaa, sys);
                return ret;
            }
            else
            {
                SaveForProcessing(jsaa, sys);
                return false;
            }
        }

    }
}
