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

        public bool AddFSSSignalsDiscoveredToSystem(JournalFSSSignalDiscovered jsd, ISystem sys, bool saveprocessinglater = true)  
        {
            foreach( var fs in jsd.Signals)
            {
                SystemNode sn = null;

                // if system = null, must look up (we are reprocessing)
                // OR if we have system address, but it differs

                if (sys == null || (fs.SystemAddress != null && sys.SystemAddress != null && fs.SystemAddress != sys.SystemAddress))
                {
                    //if (saveprocessinglater == true)           //DEBUG ONLY keep for now
                    //{
                    //    System.Diagnostics.Debug.WriteLine("DEBUG - mismatch, save for processing later" + fs.SystemAddress);
                    //    SaveForProcessing(jsd, new SystemClass(fs.SystemAddress, null));
                    //    return false;
                    //}

                    ISystem oldsys = sys;       // for debug

                    sys = null;
                    foreach (var i in ScanDataByNameSysaddr)        // manual for now, unfort. don't want another structure
                    {
                        if (i.Value.System.SystemAddress == fs.SystemAddress)
                        {
                            sys = i.Value.System;
                            break;
                        }
                    }

                    if ( sys == null )      // not found..
                    {
                        if (saveprocessinglater == true)           // if saving to list, store with a null system address
                        {
                            System.Diagnostics.Debug.WriteLine("FSS Signals Can't find - storing for later " + fs.SystemAddress);
                            SaveForProcessing(jsd, new SystemClass(fs.SystemAddress, null));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("FSS Signals process later can't find" + fs.SystemAddress);
                        }
                        return false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("FSS Signals sys address differs in {0} {1} found {2}", jsd.EventTimeUTC, oldsys?.Name, sys.Name);
                    }
                }

                sn = GetOrCreateSystemNode(sys); 

                // look at systemaddress via sys and determine if match, if not, save to processing list

                int indexprev = sn.FSSSignalList.FindIndex(x => x.IsSame(fs));
                if ( indexprev == -1)                       // if not found in signal list, store
                    sn.FSSSignalList.Add(fs);   
                else
                    sn.FSSSignalList[indexprev] = fs;       // replace, it may be more up to date info, such as a carrier info
            }

            return true;
        }
    }
}
