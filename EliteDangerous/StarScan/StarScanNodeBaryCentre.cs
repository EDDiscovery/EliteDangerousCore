﻿/*
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

using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public partial class ScanNode
        {
            // Generate a barycentre node tree from the list of nodes given
            // consists of a top node, a list of barynodes under the top node, and the barycentre children under them.
            // this is for the star scan display

            static public ScanNode PopulateBarycentres(List<ScanNode> nodes)
            {
                ScanNode top = new ScanNode();
                top.Children = new SortedList<string, ScanNode>();      

                foreach (var sn in nodes)
                {
                    // if scan has data, and scan parents structure containing parent list exists

                    if (sn.ScanData?.Parents != null)
                    {
                        //System.Diagnostics.Debug.WriteLine("Scan " + sn.ScanData.BodyName + ":" + sn.ScanData.BodyID + " " + sn.ScanData.ParentList());

                        for (int i = 0; i < sn.ScanData.Parents.Count; i++) // go thru all parents of the body, from parent to grandfather order
                        {
                            var sd = sn.ScanData;
                            var sp = sd.Parents[i];

                            if (sp.Type == "Null")      // any barycenters found in Parents array..
                            {
                                ScanNode bodynode = null;   // this is the body under the barynode

                                if (i > 0)              // so its not the last barycentre (remembering its in reverse order- last entry is the deepest (say Star) node
                                {
                                    int bodyid = sd.Parents[i - 1].BodyID;                  // pick up the body ID of the previous entry
                                    bodynode = nodes.Find((x) => x.BodyID == bodyid);       // see if its in the node list

                                    if (bodynode == null && sd.Parents[i - 1].Type == "Null")   // if can't find, and its another barycentre, add a new dummy barycentre node
                                    {
                                        bodynode = new ScanNode() { BodyID = bodyid, NodeType = ScanNodeType.barycentre, FullName = "Created Barynode " + bodyid,
                                                                    OwnName = bodyid.ToString("00000") };
                                    }
                                }
                                else
                                {
                                    // not happy with this.
                                    bodynode = sn;      // its directly above the body, so the node is the scan node (Node-barycentre)
                                }

                                if (bodynode != null)       // we have the bodynode under the barycentre
                                {
                                    string barykey = sp.BodyID.ToStringInvariant("00000"); // sp is a barycentre, so get its body id
                                    ScanNode topbarycentrenode = null;

                                    // so we add the barycentre to the top node, so all barycentres are at the top-child level

                                    if (top.Children.ContainsKey(barykey))      // if top has this barycentre..
                                    {
                                        topbarycentrenode = top.Children[barykey];        // already made..
                                    }
                                    else
                                    {
                                        // make a new top-child barycentre
                                        topbarycentrenode = new ScanNode() { BodyID = sp.BodyID, NodeType = ScanNodeType.barycentre, FullName = "Created Barynode " + sp.BodyID };    // make it
                                        topbarycentrenode.Children = new SortedList<string, ScanNode>();
                                        top.Children[barykey] = topbarycentrenode;
                                    }

                                    //System.Diagnostics.Debug.WriteLine("Scan add " + entry + " to " + barykey);

                                    // if not repeating.. add the bodynode to the barycentre child list
                                    if (!topbarycentrenode.Children.ContainsKey(bodynode.OwnName))     
                                    {
                                        topbarycentrenode.Children[bodynode.OwnName] = bodynode;
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
        } 

    }
}
