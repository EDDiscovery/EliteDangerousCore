/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        [DebuggerDisplay("SN {System.Name} {System.SystemAddress}")]
        public partial class SystemNode
        {
            public ISystem System { get; set; }

            public SortedList<string, ScanNode> StarNodes { get; private set; } = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());   // node list
            
            [QuickJSON.JsonIgnore()]
            public SortedList<int, ScanNode> NodesByID { get; private set; } = new SortedList<int, ScanNode>();               // by ID list
            public SortedList<int, JournalScanBaryCentre> Barycentres { get; private set; } = new SortedList<int, JournalScanBaryCentre>();
            public int? FSSTotalBodies { get; set; }         // if we have FSSDiscoveryScan, this will be set
            public int? FSSTotalNonBodies { get; set; }     // if we have FSSDiscoveryScan, this will be set
            public List<JournalFSSSignalDiscovered> FSSSignalList { get; private set; } = new List<JournalFSSSignalDiscovered>();       // List of FSS Signals journal entries on this node, by add order
            public List<JournalCodexEntry> CodexEntryList { get; private set; } = new List<JournalCodexEntry>();

            public SystemNode(ISystem sys)
            {
                System = sys;
            }

            public void ClearChildren()
            {
                StarNodes = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());
                NodesByID = new SortedList<int, ScanNode>();
            }

            public IEnumerable<ScanNode> Bodies()
            {
                foreach (ScanNode sn in StarNodes.Values.EmptyIfNull())     // StarNodes is never empty, but defend
                {
                    yield return sn;

                    foreach (ScanNode c in sn.Bodies())
                    {
                        yield return c;
                    }
                }
            }
 
            public long ScanValue(bool includewebvalue)
            {
                long value = 0;

                foreach (var body in Bodies())
                {
                    if (body?.ScanData != null)
                    {
                        if (includewebvalue || !body.ScanData.IsWebSourced)
                        {
                            value += body.ScanData.EstimatedValue;
                        }
                    }
                }

                return value;
            }

            // first is primary star. longform means full text, else abbreviation
            public string StarTypesFound(bool bracketit = true, bool longform = false)
            {
                var sortedset = (from x in Bodies() where x.ScanData != null && x.NodeType == ScanNodeType.star orderby x.ScanData.DistanceFromArrivalLS select longform ? x.ScanData.StarTypeText : x.ScanData.StarClassificationAbv).ToList();
                string s = string.Join("; ", sortedset);
                if (bracketit && s.HasChars())
                    s = "(" + s + ")";
                return s;
            }

            public int StarPlanetsWithData()      // This corresponds to FSSDiscoveryScan
            {
                return Bodies().Where(b => (b.NodeType == ScanNodeType.star || b.NodeType == ScanNodeType.body) && b.ScanData != null).Count();
            }
            public int StarPlanetsWithData(bool includewebbodies)        // includewebbodies gives same total as above, false means only include ones which we have scanned
            {
                return Bodies().Where(b => (b.NodeType == ScanNodeType.star || b.NodeType == ScanNodeType.body) && b.ScanData != null && (includewebbodies || !b.ScanData.IsWebSourced)).Count();
            }
            public int StarsScanned()      // only stars
            {
                return Bodies().Where(b => b.NodeType == ScanNodeType.star && b.ScanData != null).Count();
            }
            public int BeltClusters() // number of belt clusters
            {
                return Bodies().Where(b => b.NodeType == ScanNodeType.beltcluster && b.ScanData != null).Count();
            }

            // finds full name "Oochorrs KP-A c16-0 2 a"
            public ScanNode Find(string fullnameinclsystemname)
            {
                foreach (var b in Bodies())
                {
                    if (b.BodyDesignator.Equals(fullnameinclsystemname, StringComparison.InvariantCultureIgnoreCase))
                        return b;
                }
                return null;
            }

            // finds "Oochorrs KP-A c16-0 2 a" or "2 a" or a customname
            public ScanNode FindCustomNameOrFullname(string bodyname)
            {
                foreach (var b in Bodies())
                {
                    if (b.BodyName.EqualsIIC(bodyname) || b.SystemBodyName.EqualsIIC(bodyname) || b.BodyDesignator.EqualsIIC(bodyname))
                        return b;
                }
                return null;
            }

            public ScanNode Find(JournalScan s)
            {
                foreach (var b in Bodies())
                {
                    if (object.ReferenceEquals(b.ScanData,s))
                        return b;
                }

                return null;
            }
        }
    }
}
