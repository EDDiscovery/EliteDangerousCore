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
        public enum ScanNodeType { star, barycentre, body, belt, beltcluster, ring };

        [DebuggerDisplay("SN {FullName} {NodeType} lv {Level} bid {BodyID}")]
        public class ScanNode
        {
            public ScanNodeType NodeType;
            public string FullName;                 // full name including star system
            public string OwnName;                  // own name excluding star system
            public string CustomName;               // if we can extract from the journals a custom name of it, this holds it. Mostly null
            public SortedList<string, ScanNode> Children;         // kids
            public int Level;                       // level within SystemNode
            public int? BodyID;
            public bool IsMapped;                   // recorded here since the scan data can be replaced by a better version later.
            public bool WasMappedEfficiently;

            public string CustomNameOrOwnname { get { return CustomName ?? OwnName; } }

            private JournalScan scandata;            // can be null if no scan, its a place holder, else its a journal scan
            private JournalScan.StarPlanetRing beltdata;    // can be null if not belt. if its a type==belt, it is populated with belt data
            private List<JournalSAASignalsFound.SAASignal> signals; // can be null if no signals for this node, else its a list of signals.
            private List<JournalScanOrganic> organics;  // can be null if nothing for this node, else a list of organics

            public JournalScan ScanData
            {
                get
                {
                    return scandata;
                }

                set
                {
                    if (value == null)
                        return;

                    if (scandata == null)
                        scandata = value;
                    else if ((!value.IsEDSMBody && value.ScanType != "Basic") || scandata.ScanType == "Basic") // Always overwrtite if its a journalscan (except basic scans)
                    {
                        //System.Diagnostics.Debug.WriteLine(".. overwrite " + scandata.ScanType + " with " + value.ScanType + " for " + scandata.BodyName);
                        scandata = value;
                    }
                }
            }

            public JournalScan.StarPlanetRing BeltData
            {
                get
                {
                    return beltdata;
                }

                set
                {
                    if (value == null)
                        return;

                    beltdata = value;
                }
            }

            public List<JournalSAASignalsFound.SAASignal> Signals       // can be null
            {
                get
                {
                    return signals;
                }
                set
                {
                    if (value == null)
                        return;

                    signals = value;
                }
            }

            public List<JournalScanOrganic> Organics        // can be null
            {
                get
                {
                    return organics;
                }
                set
                {
                    if (value == null)
                        return;

                    organics = value;
                }
            }

            public bool DoesNodeHaveNonEDSMScansBelow()
            {
                if (ScanData != null && ScanData.IsEDSMBody == false)
                    return true;

                if (Children != null)
                {
                    foreach (KeyValuePair<string, ScanNode> csn in Children)
                    {
                        if (csn.Value.DoesNodeHaveNonEDSMScansBelow())
                            return true;
                    }
                }

                return false;
            }

            public bool IsBodyInFilter(string[] filternames, bool checkchildren)
            {
                if (IsBodyInFilter(filternames))
                    return true;

                if (checkchildren)
                {
                    foreach (var body in Descendants)
                    {
                        if (body.IsBodyInFilter(filternames))
                            return true;
                    }
                }
                return false;
            }

            public bool IsBodyInFilter(string[] filternames)    // stars/bodies use the xID type, others use the type
            {
                if (filternames.Contains("All"))
                    return true;
                string name = NodeType.ToString();      // star etc..
                if (scandata != null)
                {
                    if (NodeType == ScanNodeType.star)
                        name = scandata.StarTypeID.ToString();
                    else if (NodeType == ScanNodeType.body)
                        name = scandata.PlanetTypeID.ToString();
                }

                return filternames.Contains(name, StringComparer.InvariantCultureIgnoreCase);
            }

            public IEnumerable<ScanNode> Descendants
            {
                get
                {
                    if (Children != null)
                    {
                        foreach (ScanNode sn in Children.Values)
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


            // given a list of scannodes, construst a tree of barynodes with their scans underneath.
            // reconstructs the node tree and inserts barynodes into it from the parent info

            static public ScanNode PopulateBarycentres(List<ScanNode> nodes)
            {
                ScanNode top = new ScanNode();
                top.Children = new SortedList<string, ScanNode>();

                foreach (var sn in nodes)
                {
                    if (sn.ScanData?.Parents != null)
                    {
                        //System.Diagnostics.Debug.WriteLine("Scan " + sn.ScanData.BodyName + ":" + sn.ScanData.BodyID + " " + sn.ScanData.ParentList());

                        for (int i = 0; i < sn.ScanData.Parents.Count; i++) // go thru all parents of the body
                        {
                            var sd = sn.ScanData;
                            var sp = sd.Parents[i];

                            if (sp.Type == "Null")      // any barycenters, process
                            {
                                ScanNode bodynode = null;

                                if (i > 0)              // so its not the last barycentre (remembering its in reverse order- last entry is the deepest (say Star) node
                                {
                                    int bodyid = sd.Parents[i - 1].BodyID;                  // pick up the body ID of the previous entry
                                    bodynode = nodes.Find((x) => x.BodyID == bodyid);       // see if its in the scan database

                                    if (bodynode == null && sd.Parents[i - 1].Type == "Null")   // if can't find, and its another barycentre, add a new dummy barycentre node
                                    {
                                        bodynode = new ScanNode() { BodyID = bodyid, NodeType = ScanNodeType.barycentre, FullName = "Created Barynode " + bodyid, OwnName = bodyid.ToString("00000") };
                                    }
                                }
                                else
                                {
                                    bodynode = sn;      // its directly under the body, so the node is the scan node (Node-barycentre)
                                }

                                if (bodynode != null)
                                {
                                    string barykey = sp.BodyID.ToStringInvariant("00000"); // sp is a barycentre, so get its body id
                                    ScanNode cur = null;

                                    if (top.Children.ContainsKey(barykey))      // if top has this barycentre..
                                        cur = top.Children[barykey];
                                    else
                                    {
                                        cur = new ScanNode() { BodyID = sp.BodyID, NodeType = ScanNodeType.barycentre, FullName = "Created Barynode " + sp.BodyID };    // make it
                                        cur.Children = new SortedList<string, ScanNode>();
                                        top.Children[barykey] = cur;
                                    }

                                    //System.Diagnostics.Debug.WriteLine("Scan add " + entry + " to " + barykey);

                                    if (!cur.Children.ContainsKey(bodynode.OwnName))
                                    {
                                        cur.Children[bodynode.OwnName] = bodynode;
                                    }
                                }
                            }
                        }
                    }
                }

                List<string> keystodelete = new List<string>();

                foreach (var n in top.Children)
                {
                    //System.Diagnostics.Debug.WriteLine("Top Node  " + n.Value.BodyID);
                    keystodelete.AddRange(ExpandRecurivelyBarynodeTree(top, n.Value));      // move bary-node on the top to their positions in the tree
                }

                foreach (var k in keystodelete)     // remove any moved keys
                    top.Children.Remove(k);

                return top;
            }

            static private List<string> ExpandRecurivelyBarynodeTree(ScanNode top, ScanNode pos)     // go down tree, moving nodes from the top to their positions
            {
                List<string> keystodelete = new List<string>();

                foreach (var k in pos.Children)     // all children of top
                {
                    string keyid = k.Value.BodyID.ToStringInvariant("00000");   // key from bodyid

                    if (k.Value.NodeType == ScanNodeType.barycentre && top.Children.ContainsKey(keyid)) // its a barycentre, and top has that barycentre, move it to here
                    {
                        //System.Diagnostics.Debug.WriteLine(".. barycenter  " + keyid);
                        ScanNode tocopy = top.Children[keyid];

                        if (k.Value.Children == null)
                            k.Value.Children = new SortedList<string, ScanNode>();

                        foreach (var cc in tocopy.Children)
                        {
                            string cckey = cc.Key;
                            if (!k.Value.Children.ContainsKey(cckey))                               // may have been moved already, because we don't remove top keys until finished
                            {
                                // System.Diagnostics.Debug.WriteLine(".. " + cckey + " " + cc.Value.fullname + " onto " + keyid);
                                k.Value.Children.Add(cckey, cc.Value);
                                ExpandRecurivelyBarynodeTree(top, k.Value);
                            }
                            else
                            {
                                // System.Diagnostics.Debug.WriteLine(".. Dup move " + cckey + " " + cc.Value.fullname + " onto " + keyid);
                            }
                        }

                        keystodelete.Add(keyid);
                    }
                }

                return keystodelete;
            }

            public static void DumpTree(ScanNode top, string key, int level)        // debug dump out
            {
                System.Diagnostics.Debug.WriteLine("                                                        ".Substring(0, level * 3) + key + ":" + top.BodyID + " " + top.FullName + " " + top.NodeType);
                if (top.Children != null)
                {
                    foreach (var c in top.Children)
                        DumpTree(c.Value, c.Key, level + 1);
                }
            }

        };


    }
}
