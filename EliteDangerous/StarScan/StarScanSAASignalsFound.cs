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
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // used by historylist directly for a single update during play, in foreground..  Also used by above.. so can be either in fore/back
        public bool AddSAASignalsFoundToBestSystem(JournalSAASignalsFound jsaa, ISystem sys)
        {
            // jsaa should always have a system address.  If it matches current system, we can go for an immediate add
            if (sys.SystemAddress.HasValue && jsaa.SystemAddress == sys.SystemAddress.Value)
            {
                bool ret = ProcessSignalsFound(jsaa.BodyID.Value, jsaa.BodyName, jsaa.Signals, jsaa.Genuses, sys);
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

        // add signals from various sources (SAASignals, FSSBodySignals) to node
        // genuses only present in odyssey 4.0 v13+ SAASignals, so it may be null

        private bool ProcessSignalsFound(int bodyid, string bodyname,
                                            List<JournalSAASignalsFound.SAASignal> signals,
                                            List<JournalSAASignalsFound.SAAGenus> genuses,
                                            ISystem sys)  // background or foreground.. FALSE if you can't process it
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                ScanNode relatednode = null;

                if (systemnode.NodesByID.ContainsKey((int)bodyid)) // find by ID
                {
                    relatednode = systemnode.NodesByID[(int)bodyid];
                }

                if (relatednode != null && relatednode.NodeType == ScanNodeType.ring && relatednode.ScanData != null && relatednode.ScanData.Parents != null && systemnode.NodesByID.ContainsKey(relatednode.ScanData.Parents[0].BodyID))
                {
                    relatednode = systemnode.NodesByID[relatednode.ScanData.Parents[0].BodyID];
                }

                if (relatednode == null || relatednode.NodeType == ScanNodeType.ring)
                {
                    bool ringname = bodyname.EndsWith("A Ring") || bodyname.EndsWith("B Ring") || bodyname.EndsWith("C Ring") || bodyname.EndsWith("D Ring");
                    string ringcutname = ringname ? bodyname.Left(bodyname.Length - 6).TrimEnd() : null;

                    foreach (var body in systemnode.Bodies)
                    {
                        if ((body.FullName == bodyname || body.CustomName == bodyname) &&
                            (body.FullName != sys.Name || body.Level != 0))
                        {
                            relatednode = body;
                            break;
                        }
                        else if (ringcutname != null && body.FullName.Equals(ringcutname))
                        {
                            relatednode = body;
                            break;
                        }
                    }
                }

                if (relatednode != null)
                {
                    //  System.Diagnostics.Debug.WriteLine("Setting SAA Signals Found for " + bodyname + " @ " + sys.Name + " body "  + jsaa.BodyDesignation);
                    if (signals != null && signals.Count > 0)
                    {
                        if (relatednode.Signals == null)
                            relatednode.Signals = new List<JournalSAASignalsFound.SAASignal>();

                        foreach (var x in signals)
                        {
                            if (relatednode.Signals.Find(y => y.Type == x.Type && y.Count == x.Count) == null)
                            {
                                relatednode.Signals.Add(x);
                            }
                        }
                    }

                    if (genuses != null && genuses.Count > 0)        // if we have any - ones before Odyssey v4.0 r 13 did not
                    {
                        if (relatednode.Genuses == null)
                            relatednode.Genuses = new List<JournalSAASignalsFound.SAAGenus>();

                        foreach (var x in genuses)
                        {
                            if (relatednode.Genuses.Find(y => y.Genus == x.Genus) == null)
                            {
                                relatednode.Genuses.Add(x);
                            }
                        }
                    }

                    if (relatednode.ScanData != null)
                    {
                        relatednode.ScanData.Signals = relatednode.Signals;       // make sure Scan node has same list as subnode
                        relatednode.ScanData.Genuses = relatednode.Genuses;
                        // System.Diagnostics.Debug.WriteLine($"Assign SAA signal list {string.Join(",", relatednode.Signals.Select(x => x.Type).ToList())} to {relatednode.FullName}");
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
