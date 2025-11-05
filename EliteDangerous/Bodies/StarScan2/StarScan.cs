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
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Public IF

        // Find system, optinally do weblookup
        // null if not found
        public SystemNode FindSystemSynchronous(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    
        {
            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if (sys.SystemAddress.HasValue || sys.Name.HasChars())      // we have good enough data (should have)
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
                            AddScan(js, sys);
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
                            AddScan(js, sys);
                        }
                    }
                }

                return FindSystem(sys);
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

            if (sys.SystemAddress.HasValue || sys.Name.HasChars())      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys))
                {
                    if (sys.Name.IsEmpty())
                        System.Diagnostics.Debug.WriteLine($"WARNING - Spansh lookup with empty name of system is liable to errors - cant set body designation properly");

                    var lookupres = await Spansh.SpanshClass.GetBodiesListAsync(sys, usespansh);          // see if spansh has it cached or optionally look it up

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            AddScan(js, sys);
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
                            AddScan(js, sys);
                        }
                    }
                }

                return FindSystem(sys);
            }

            return null;
        }

        // Find system, primary thru address, if not by name
        public SystemNode FindSystem(ISystem sys)
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

        // Find system, primary thru address, if not by name
        public bool TryGetSystem(long addr, out SystemNode sn)
        {
            lock (masterlock)
            {
                return systemNodesByAddress.TryGetValue(addr, out sn);
            }
        }

        // try and find the address, if we know it, else null
        public ISystem FindISystem(string sysname)
        {
            lock (masterlock)
            {
                return systemNodesByName.TryGetValue(sysname, out var node) ? node.System : null;
            }
        }
        public ISystem FindISystem(long addr)
        {
            lock (masterlock)
            {
                return systemNodesByAddress.TryGetValue(addr, out var node) ? node.System : null;
            }
        }

        public ISystem FindISystemWithCache(long addr, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            lock (masterlock)
            {
                if (systemNodesByAddress.TryGetValue(addr, out var node) )
                    return node.System;
                else
                    return SystemCache.FindSystem(new SystemClass("",addr), lookup);
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
                    SystemNode sn = systemNodesByAddress[kvp.Key];       // must be there, can't not be

                    lock (sn)
                    {
                        if (AssignPending(sn, kvp.Value))
                            todeletesys.Add(kvp.Key);
                    }
                }

                foreach (var sysaddress in todeletesys)
                    pendingsystemaddressevents.Remove(sysaddress);

                foreach (var kvp in pendingsystemaddressevents)
                {
                    System.Diagnostics.Debug.WriteLine($"StarScan Pending left system {kvp.Key} count {kvp.Value.Count}");
                }

#if DEBUG

                // Check barycentre info is properly stored in modern scans

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
                                    (p.Barycentre != null).Assert("Barycentre not set");
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

