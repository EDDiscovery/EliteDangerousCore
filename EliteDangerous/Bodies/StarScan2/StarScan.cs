/*
 * Copyright 2025 - 2025 EDDiscovery development team
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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Public IF

        // Find system, optinally do weblookup
        // null if not found
        // Sys can be an address, a name, or a name and address. address takes precedence
        public SystemNode FindSystemSynchronous(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    
        {
            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if (sys.HasAddress || sys.HasName)      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys))
                {
                    var lookupres = Spansh.SpanshClass.GetBodiesList(sys, usespansh);          // see if spansh has it cached or optionally look it up

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            AddJournalScan(js, sys);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, lookupres.System);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys))
                {
                    var lookupres = EDSM.EDSMClass.GetBodiesList(sys, useedsm);            // see if edsm has it cached or optionally look it up
                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            AddJournalScan(js, sys);
                        }
                    }
                }

                return GetSystemNode(sys);
            }

            return null;
        }

        // Async Find system, optinally do weblookup
        // you must be returning void to use this..
        // Sys can be an address, a name, or a name and address. address takes precedence
        public async System.Threading.Tasks.Task<SystemNode> FindSystemAsync(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if (sys.HasAddress || sys.HasName)      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys))
                {
                    if (sys.Name.IsEmpty())
                        System.Diagnostics.Debug.WriteLine($"StarScan WARNING - Spansh lookup with empty name of system is liable to errors - cant set body designation properly");

                    var lookupres = await Spansh.SpanshClass.GetBodiesListAsync(sys, usespansh);          // see if spansh has it cached or optionally look it up

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            AddJournalScan(js, sys);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, lookupres.System);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys))     // using edsm, no lookup, go
                {
                    var lookupres = await EliteDangerousCore.EDSM.EDSMClass.GetBodiesListAsync(sys, useedsm);

                    if (lookupres != null)
                    {
                        if (sys.Name.IsEmpty())
                            System.Diagnostics.Debug.WriteLine($"WARNING - Spansh lookup with empty name of system is liable to errors - cant set body designation properly");

                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            AddJournalScan(js, sys);
                        }
                    }
                }

                return GetSystemNode(sys);
            }

            return null;
        }

        // Find system, primary thru address, if not by name
        public SystemNode GetSystemNode(ISystem sys)
        {
            lock (masterlock)
            {
                if (sys.SystemAddress.HasValue && systemNodesByAddress.TryGetValue(sys.SystemAddress.Value, out var systemNode))
                {
                    return systemNode;
                }
                else if (sys.Name.HasChars() && systemNodesByName.TryGetValue(sys.Name, out var systemNode1))
                {
                    return systemNode1;
                }
                else
                    return null;
            }
        }

        // Find system thru address
        public bool TryGetSystemNode(long addr, out SystemNode sn)
        {
            lock (masterlock)
            {
                return systemNodesByAddress.TryGetValue(addr, out sn);
            }
        }

        // Find system thru name
        public bool TryGetSystemNode(string name, out SystemNode sn)
        {
            lock (masterlock)
            {
                return systemNodesByName.TryGetValue(name, out sn);
            }
        }

        // try and find the ISystem of a name
        public ISystem GetISystem(string sysname)
        {
            lock (masterlock)
            {
                return systemNodesByName.TryGetValue(sysname, out var node) ? node.System : null;
            }
        }

        // try and find the ISystem of an address
        public ISystem GetISystem(long addr)
        {
            lock (masterlock)
            {
                return systemNodesByAddress.TryGetValue(addr, out var node) ? node.System : null;
            }
        }

        // try and find the system thru address, using StarScan, and if not using the SystemCache/DB/Lookup
        public ISystem GetISystemWithCache(long addr, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            lock (masterlock)
            {
                if (systemNodesByAddress.TryGetValue(addr, out var node) )
                    return node.System;
                else
                    return SystemCache.FindSystem(addr, lookup);
            }
        }

        public List<SystemNode> AllNamedSystems => systemNodesByName.Values.ToList();

        #endregion


        #region Post creation clean up

        // Given the pending list, assign it to the database of systems
        public void AssignPending()
        {
            lock (masterlock)
            {
                List<long> todeletesys = new List<long>();

                foreach (var kvp in pendingsystemaddressevents)
                {
                    if (systemNodesByAddress.TryGetValue(kvp.Key, out SystemNode sn))       // turns out we need to check, as i've seen FSSSignalDiscovered on a system that has never been entered
                    {
                        lock (sn)
                        {
                            if (AssignPending(sn, kvp.Value))
                                todeletesys.Add(kvp.Key);
                        }
                    }
                }

                foreach (var sysaddress in todeletesys)
                    pendingsystemaddressevents.Remove(sysaddress);

                // Debug, print out what is left

                foreach (var kvp in pendingsystemaddressevents)
                {
                    if (systemNodesByAddress.TryGetValue(kvp.Key, out SystemNode sn))       // turns out we need to check, as i've seen FSSSignalDiscovered on a system that has never been entered
                    {
                        System.Diagnostics.Trace.WriteLine($"StarScan Pending left system {sn.System} count {kvp.Value.Count}");
                        foreach (var entry in kvp.Value)
                        {
                            System.Diagnostics.Trace.WriteLine($" .. {entry.EventTimeUTC} {entry.EventTypeStr} {entry.GetInfo(sn.System)}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine($"StarScan Pending events no system stored for {kvp.Key} Events {kvp.Value.Count} possible FSS Signal discovered but never entered system");
                    }
                }

#if DEBUG
                // Check barycentre info is properly stored in modern scans- we may be missing them in the journal so its not a fatal error, just something to note in log

                foreach( var kvp in systemNodesByAddress)
                {
                    if (kvp.Value.BarycentreScans > 0)
                    {
                        foreach (var bn in kvp.Value.Bodies())
                        {
                            if (bn.Scan?.Parents != null)
                            {
                                foreach (var p in bn.Scan.Parents.Where(x=>x.BodyID!=0 && x.IsBarycentre))
                                {
                                    if (p.Barycentre == null)
                                    {
                                        string s = $"StarScan Barycentre not set in scan for {bn.Scan.EventTimeUTC} {bn.Scan.BodyName} {bn.Scan.BodyID} bid {bn.BodyID} {bn.Scan.ParentList()} {p.BodyID} {p.Type} {EDCommander.Current.Name}";
                                        System.Diagnostics.Trace.WriteLine(s);
                                    }
                                }
                            }
                        }
                    }
                }

#endif
            }
        }

        public void ClearPending()
        {
            pendingsystemaddressevents.Clear();
        }

        #endregion


    }
}

