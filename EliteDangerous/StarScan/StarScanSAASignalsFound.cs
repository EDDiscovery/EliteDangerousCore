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

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // used by historylist directly for a single update during play, in foreground..  Also used by above.. so can be either in fore/back
        public bool AddSAASignalsFoundToBestSystem(JournalSAASignalsFound jsaa, int startindex, List<HistoryEntry> hl)
        {
            if (jsaa.Signals == null || jsaa.BodyName == null)       // be paranoid, don't add if null signals
                return false;

            var best = FindBestSystem(startindex, hl, jsaa.BodyName, jsaa.BodyID, false);

            if (best == null)
                return false;

            jsaa.BodyDesignation = best.Item1;

            return ProcessSAASignalsFound(jsaa, best.Item2);
        }

        private bool ProcessSAASignalsFound(JournalSAASignalsFound jsaa, ISystem sys, bool saveprocessinglater = true)  // background or foreground.. FALSE if you can't process it
        {
            SystemNode sn = GetOrCreateSystemNode(sys);
            ScanNode relatednode = null;

            if (sn.NodesByID.ContainsKey((int)jsaa.BodyID)) // find by ID
            {
                relatednode = sn.NodesByID[(int)jsaa.BodyID];
                if (relatednode.ScanData != null && relatednode.ScanData.BodyDesignation != null)
                {
                    jsaa.BodyDesignation = relatednode.ScanData.BodyDesignation;
                }
            }
            else if (jsaa.BodyDesignation != null && jsaa.BodyDesignation != jsaa.BodyName)
            {
                foreach (var body in sn.Bodies)
                {
                    if (body.FullName == jsaa.BodyDesignation)
                    {
                        relatednode = body;
                        break;
                    }
                }
            }

            if (relatednode != null && relatednode.NodeType == ScanNodeType.ring && relatednode.ScanData != null && relatednode.ScanData.Parents != null && sn.NodesByID.ContainsKey(relatednode.ScanData.Parents[0].BodyID))
            {
                relatednode = sn.NodesByID[relatednode.ScanData.Parents[0].BodyID];
            }

            if (relatednode == null || relatednode.NodeType == ScanNodeType.ring)
            {
                bool ringname = jsaa.BodyName.EndsWith("A Ring") || jsaa.BodyName.EndsWith("B Ring") || jsaa.BodyName.EndsWith("C Ring") || jsaa.BodyName.EndsWith("D Ring");
                string ringcutname = ringname ? jsaa.BodyName.Left(jsaa.BodyName.Length - 6).TrimEnd() : null;

                foreach (var body in sn.Bodies)
                {
                    if ((body.FullName == jsaa.BodyName || body.CustomName == jsaa.BodyName) &&
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

            if (relatednode != null )
            {
              //  System.Diagnostics.Debug.WriteLine("Setting SAA Signals Found for " + jsaa.BodyName + " @ " + sys.Name + " body "  + jsaa.BodyDesignation);
                if (relatednode.Signals == null)
                    relatednode.Signals = new List<JournalSAASignalsFound.SAASignal>();

                relatednode.Signals.AddRange(jsaa.Signals); // add signals to list of signals of this entity

                return true; // all ok
            }
            else
            {
                if (saveprocessinglater)
                    SaveForProcessing(jsaa,sys);
              //  System.Diagnostics.Debug.WriteLine("No body to attach data found for " + jsaa.BodyName + " @ " + sys.Name + " body " + jsaa.BodyDesignation);
            }

            return false;
        }
    }
}
