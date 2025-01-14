/*
 * Copyright © 2015 - 2023 EDDiscovery development team
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
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public Dictionary<long, SystemNode> ScanDataBySysaddr { get; private set; } = new Dictionary<long, SystemNode>();       // primary store - may not be there
        public Dictionary<string, SystemNode> ScanDataByName { get; private set; } = new Dictionary<string, SystemNode>();      // by name, always there

        private const string MainStar = "Main Star";

        // this tries to reprocess any JEs associated with a system node which did not have scan data at the time.
        // Seen to work with log from Issue #2983

        public List<Tuple<JournalEntry, ISystem>> ToProcess = new List<Tuple<JournalEntry, ISystem>>();     // entries seen but yet to be processed due to no scan node (used by reports which do not create scan nodes)

        public void SaveForProcessing(JournalEntry je, ISystem sys)
        {
            ToProcess.Add(new Tuple<JournalEntry, ISystem>(je, sys));
        }

        private void ProcessedSaved()
        {
            List<Tuple<JournalEntry, ISystem>> todelete = new List<Tuple<JournalEntry, ISystem>>();
            foreach (var e in ToProcess)
            {
                if (e.Item1.EventTypeID == JournalTypeEnum.SAAScanComplete)
                { 
                    if (ProcessSAAScan((JournalSAAScanComplete)e.Item1, e.Item2, false))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.SAASignalsFound)
                {
                    var jsaa = (JournalSAASignalsFound)e.Item1;
                    if (ProcessSignalsFound(jsaa.BodyID.Value,jsaa.BodyName,jsaa.Signals, jsaa.Genuses, e.Item2))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
                {
                    if (AddFSSSignalsDiscoveredToSystem((JournalFSSSignalDiscovered)e.Item1, false))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.FSSBodySignals)
                {
                    var jsaa = (JournalFSSBodySignals)e.Item1;
                    if (ProcessSignalsFound(jsaa.BodyID.Value, jsaa.BodyName, jsaa.Signals, null, e.Item2))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.CodexEntry)
                {
                    if (AddCodexEntryToSystem((JournalCodexEntry)e.Item1, false))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.ScanOrganic)
                {
                    if (ProcessScanOrganicFound((JournalScanOrganic)e.Item1, e.Item2))
                        todelete.Add(e);
                }
                else if (e.Item1.EventTypeID == JournalTypeEnum.ScanBaryCentre)
                {
                    if (AddBarycentre((JournalScanBaryCentre)e.Item1, e.Item2, false))
                        todelete.Add(e);
                }
            }

            foreach (var je in todelete)
                ToProcess.Remove(je);
        }

        // ONLY use this if you must because the async await won't work in the call stack.  
        public SystemNode FindSystemSynchronous(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if ((sys.SystemAddress.HasValue && sys.SystemAddress > 0) || sys.Name.HasChars())      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if ( usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys.Name))
                {
                    var lookupres = Spansh.SpanshClass.GetBodiesList(sys, usespansh);          // see if spansh has it cached or optionally look it up
                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemSync spansh add {sys.Name} {js.BodyName}");
                            ProcessJournalScan(js, sys, true);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, sys);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys.Name))
                {
                    var lookupres = EDSM.EDSMClass.GetBodiesList(sys, useedsm);            // see if edsm has it cached or optionally look it up
                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemSync edsn add {sys.Name} {js.BodyName}");
                            ProcessJournalScan(js, sys, true);
                        }
                    }
                }
            }

            SystemNode sn = FindSystemNode(sys);
            return sn;
        }

        // you must be returning void to use this..
        public async System.Threading.Tasks.Task<SystemNode> FindSystemAsync(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if ((sys.SystemAddress.HasValue && sys.SystemAddress > 0) || sys.Name.HasChars())      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys.Name))
                {
                    var lookupres = await Spansh.SpanshClass.GetBodiesListAsync(sys, usespansh);          // see if spansh has it cached or optionally look it up

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                           // System.Diagnostics.Debug.WriteLine($"FindSystemASync spansh add {sys.Name} {js.BodyName}");
                            ProcessJournalScan(js, sys, true);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, sys);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys.Name))     // using edsm, no lookup, go
                {
                    var lookupres = await EliteDangerousCore.EDSM.EDSMClass.GetBodiesListAsync(sys, useedsm);

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

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, sys.Name);
                         //   System.Diagnostics.Debug.WriteLine($"FindSystemASync edsm add {sys.Name} {js.BodyName}");
                            ProcessJournalScan(js, sys, true);
                        }
                    }
                }


            }

            SystemNode sn = FindSystemNode(sys);
            return sn;
        }
    }
}
