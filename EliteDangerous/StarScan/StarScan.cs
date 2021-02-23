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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // we store by the tuple<name:sysaddr> if it has an address, else we store by name only. We use the tuple just to distinguish it more
        //      names are searched first during store to see if we have an older entry there without a system address 
        //      then we search for name:sysaddr if we have a sys addr 
        //      then we search for name in ByNameSysAddr as the last resort, in case the Find Isystem is synthesised without a system address being available
        // we do not store all names in ByName, because we want to store ones with system address in it. We want maximum distiguishing of systems
        // star names are case sensitive.

        public Dictionary<Tuple<string, long?>, SystemNode> ScanDataByNameSysaddr { get; private set; } = new Dictionary<Tuple<string, long?>, SystemNode>();
        public Dictionary<string, SystemNode> scanDataByName { get; private set; } = new Dictionary<string, SystemNode>();

        public List<SystemNode> ScansSortedByName()
        {
            List<SystemNode> scans = scanDataByName.Values.ToList();
            scans.AddRange(ScanDataByNameSysaddr.Values.ToList());
            scans.Sort(delegate (SystemNode l, SystemNode r) { return l.System.Name.CompareTo(r.System.Name); });
            return scans;
        }

        private const string MainStar = "Main Star";

        // this tries to reprocess any JEs associated with a system node which did not have scan data at the time.
        // Seen to work with log from Issue #2983

        private void ProcessedSaved(SystemNode sn)
        {
            if (sn.ToProcess != null)
            {
                List<Tuple<JournalEntry, ISystem>> todelete = new List<Tuple<JournalEntry, ISystem>>();
                foreach (var e in sn.ToProcess)
                {
                    JournalSAAScanComplete jsaasc = e.Item1 as JournalSAAScanComplete;
                    if (jsaasc != null)
                    {
                        if (ProcessSAAScan(jsaasc, e.Item2, false))
                            todelete.Add(e);
                    }
                    JournalSAASignalsFound jsaasf = e.Item1 as JournalSAASignalsFound;
                    if (jsaasf != null)
                    {
                        if (ProcessSAASignalsFound(jsaasf, e.Item2, false))
                            todelete.Add(e);
                    }
                }

                foreach (var je in todelete)
                    sn.ToProcess.Remove(je);
            }
        }

        public bool HasWebLookupOccurred(ISystem sys)       // have we had a web checkup on this system?  false if sys does not exist
        {
            SystemNode sn = FindSystemNode(sys);
            return (sn != null && sn.EDSMWebChecked);
        }

        // ONLY use this if you must because the async await won't work in the call stack.  edsmweblookup here with true is strongly discouraged

        public SystemNode FindSystemSynchronous(ISystem sys, bool edsmweblookup)    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            SystemNode sn = FindSystemNode(sys);

            // System.Diagnostics.Debug.WriteLine("Scan Lookup " + sys.Name + " found " + (sn != null) + " web? " + edsmweblookup + " edsm lookup " + (sn?.EDSMAdded ?? false));

            if ((sys.EDSMID > 0 || (sys.SystemAddress != null && sys.SystemAddress > 0) || (sys.Name.HasChars())) && (sn == null || sn.EDSMCacheCheck == false || (edsmweblookup && !sn.EDSMWebChecked)))
            {
                var jl = EliteDangerousCore.EDSM.EDSMClass.GetBodiesList(sys, edsmweblookup); // lookup, with optional web

                //if (edsmweblookup) System.Diagnostics.Debug.WriteLine("EDSM WEB Lookup bodies " + sys.Name + " " + sys.EDSMID + " result " + (jl?.Count ?? -1));

                if (jl != null && jl.Item2 == false) // found some bodies, not from the cache
                {
                    foreach (JournalScan js in jl.Item1)
                    {
                        js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                        ProcessJournalScan(js, sys, true);
                    }
                }

                if (sn == null) // refind to make sure SN is set
                    sn = FindSystemNode(sys);

                if (sn != null) // if we found it, set to indicate we did a cache check
                {
                    sn.EDSMCacheCheck = true;

                    if (edsmweblookup)      // and if we did a web check, set it too..
                        sn.EDSMWebChecked = true;
                }
            }

            return sn;

        }

        // you must be returning void to use this..

        public async System.Threading.Tasks.Task<SystemNode> FindSystemAsync(ISystem sys, bool edsmweblookup )    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            SystemNode sn = FindSystemNode(sys);

            //string trace = Environment.StackTrace.StackTrace("FindSystemAsync", 4);

            //System.Diagnostics.Debug.WriteLine("Scan Lookup " + trace + " " + sys.Name + " found " + (sn != null) + " web? " + edsmweblookup + " edsm lookup " + (sn?.EDSMWebChecked ?? false));

            if ((sys.EDSMID > 0 || (sys.SystemAddress != null && sys.SystemAddress > 0) || (sys.Name.HasChars())) && (sn == null || sn.EDSMCacheCheck == false || (edsmweblookup && !sn.EDSMWebChecked)))
            {
                var jl = await EliteDangerousCore.EDSM.EDSMClass.GetBodiesListAsync(sys, edsmweblookup); // lookup, with optional web

                // return bodies and a flag indicating if from cache.
                // Scenario: Three panels are asking for data, one at a time, since its the foreground thread
                // each one awaits, sets and runs a task, blocks until tasks completes, foreground continues to next panel where it does the same
                // we have three tasks, any which could run in any order. 
                // The tasks all go thru GetBodiesListAsync, which locks.  Only 1 task gets to do the lookup, the one which got there first, because it did not see
                // a cached version
                // once that task completes the lookups, and it unlocks, the other tasks can run, and they will see the cache setup.  They won't do an EDSM web access
                // since the body is in the cache.  
                // for now, i can't guarantee that the task which gives back the bodies first runs on the foreground task.  It may be task2 gets the bodies.
                // so we will just add them in again

                if (jl != null && jl.Item1 != null)
                {
                    // removed - can't guarantee if (jl.Item2 == false)      // only want them if not previously cached
                    {
                        //System.Diagnostics.Debug.WriteLine("Process bodies from EDSM " + trace + " " + sys.Name + " " + sys.EDSMID + " result " + (jl.Item1?.Count ?? -1));
                        foreach (JournalScan js in jl.Item1)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                            ProcessJournalScan(js, sys, true);
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine("Lookup System node again");
                if (sn == null) // refind to make sure SN is set
                    sn = FindSystemNode(sys);

                if (sn != null) // if we found it, set to indicate we did a cache check
                {
                    sn.EDSMCacheCheck = true;

                    if (edsmweblookup)      // and if we did a web check, set it too..
                        sn.EDSMWebChecked = true;
                }
            }

            return sn;
        }
    }
}
