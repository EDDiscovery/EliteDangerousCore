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
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        [DebuggerDisplay("SN {system.Name}")]
        public class SystemNode
        {
            public ISystem System;

            public SortedList<string, ScanNode> StarNodes;                                              // node list
            public SortedList<int, ScanNode> NodesByID = new SortedList<int, ScanNode>();               // by ID list

            public bool EDSMCacheCheck = false; // check flags
            public bool EDSMWebChecked = false;

            public int? FSSTotalBodies;         // if we have FSSDiscoveryScan, this will be set

            public List<JournalFSSSignalDiscovered.FSSSignal> FSSSignalList = new List<JournalFSSSignalDiscovered.FSSSignal>();     // set by FSSSignalsDiscovered

            public int MaxTopLevelBodyID = 0;       // body Ids seen
            public int MinPlanetBodyID = 512;

            public List<Tuple<JournalEntry, ISystem>> ToProcess;     // entries seen but yet to be processed due to no scan node (used by reports which do not create scan nodes)

            public IEnumerable<ScanNode> Bodies
            {
                get
                {
                    if (StarNodes != null)
                    {
                        foreach (ScanNode sn in StarNodes.Values)
                        {
                            yield return sn;

                            foreach (ScanNode c in sn.Descendants)
                            {
                                yield return c;
                            }
                        }
                    }
                }
            }

            public long ScanValue(bool includeedsmvalue)
            {
                long value = 0;

                foreach (var body in Bodies)
                {
                    if (body?.ScanData != null)
                    {
                        if (includeedsmvalue || !body.ScanData.IsEDSMBody)
                        {
                            value += body.ScanData.EstimatedValue;
                        }
                    }
                }

                return value;
            }

            public string StarTypesFound(bool bracketit = true) // first is primary star
            {
                var sortedset = (from x in Bodies where x.ScanData != null && x.NodeType == ScanNodeType.star orderby x.ScanData.DistanceFromArrivalLS select x.ScanData.StarTypeID.ToString()).ToList();
                string s = string.Join("; ", sortedset);
                if (bracketit && s.HasChars())
                    s = "(" + s + ")";
                return s;
            }

            public int StarPlanetsScanned()      // not include anything but these.  This corresponds to FSSDiscoveryScan
            {
                return Bodies.Where(b => (b.NodeType == ScanNodeType.star || b.NodeType == ScanNodeType.body) && b.ScanData != null).Count();
            }
            public int StarsScanned()      // only stars
            {
                return Bodies.Where(b => b.NodeType == ScanNodeType.star && b.ScanData != null).Count();
            }

            public ScanNode Find(string bodyname)
            {
                foreach (var b in Bodies)
                {
                    if (b.FullName.Equals(bodyname, StringComparison.InvariantCultureIgnoreCase))
                        return b;
                }
                return null;
            }

            public void SaveForProcessing(JournalEntry je, ISystem sys)
            {
                if (ToProcess == null)
                    ToProcess = new List<Tuple<JournalEntry, ISystem>>();
                ToProcess.Add(new Tuple<JournalEntry, ISystem>(je, sys));
            }

        };

    }
}
