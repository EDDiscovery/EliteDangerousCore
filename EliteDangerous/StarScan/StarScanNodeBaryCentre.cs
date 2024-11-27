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
                ScanNode barycentrelist = new ScanNode();
                barycentrelist.Children = new SortedList<string, ScanNode>();      

                // first, in barycentrelist, we go thru all nodes, look at the parent array, and extract a list of all barycentres

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
                                    bodynode = nodes.Find((x) => x.BodyID == bodyid);       // see if its in the node list, may fail of course if not scanned etc

                                    if (bodynode == null && sd.Parents[i - 1].Type == "Null")   // if can't find, and its another barycentre, add a new dummy barycentre node
                                    {
                                        bodynode = new ScanNode() { BodyID = bodyid, NodeType = ScanNodeType.barycentre, BodyDesignator = "Created Barynode " + bodyid,
                                                                    OwnName = bodyid.ToString("00000") };
                                    }
                                }
                                else
                                {
                                    // we do not alter sn at all, so we can directly use it
                                    bodynode = sn;      // its directly above the body, so the node is the scan node (Node-barycentre)
                                }

                                // we have the bodynode under the barycentre

                                if (bodynode != null)       
                                {
                                    string barykey = sp.BodyID.ToStringInvariant("00000"); // sp is a barycentre, so make its body id
                                    ScanNode topbarycentrenode = null;

                                    // so we add the barycentre to the top node, so all barycentres are at the top-child level

                                    if (barycentrelist.Children.ContainsKey(barykey))      // if list has this barycentre already, add another child to it
                                    {
                                        topbarycentrenode = barycentrelist.Children[barykey];        // already made..
                                    }
                                    else
                                    {
                                        // make a new barycentre for the list
                                        topbarycentrenode = new ScanNode() { BodyID = sp.BodyID, NodeType = ScanNodeType.barycentre, BodyDesignator = "Created Barynode " + sp.BodyID };    // make it
                                        topbarycentrenode.Children = new SortedList<string, ScanNode>();
                                        barycentrelist.Children[barykey] = topbarycentrenode;
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

                //DumpTree(barycentrelist, "BA", 0);

                // we now have a list of all the barycentres in the game, but flat, at the top level. we want it heirachically arranged, so we go thru it and move it around
                // then remove them from the top level

                List<string> keystodelete = new List<string>();

                foreach (var n in barycentrelist.Children)
                {
                    //System.Diagnostics.Debug.WriteLine("Top Node  " + n.Value.BodyID);
                    var delkeys = ExpandRecurivelyBarynodeTree(barycentrelist, n.Value);       // recurse thru the nodes of n.Value, adding barycentres to the top
                    keystodelete.AddRange(delkeys);      // move bary-node on the top to their positions in the tree
                }

                foreach (var k in keystodelete)     // remove any moved keys
                    barycentrelist.Children.Remove(k);

                //DumpTree(barycentrelist, "BC", 0);
                return barycentrelist;
            }

            // go down tree, moving any barycentres found in the recurse from the barycentrelist to their proper positions
            static private List<string> ExpandRecurivelyBarynodeTree(ScanNode barycentrelist, ScanNode pos)     
            {
                List<string> keystodelete = new List<string>();

                foreach (var sn in pos.Children)     // all children of the scan node
                {
                    string keyid = sn.Value.BodyID.ToStringInvariant("00000");   // key from bodyid

                    // its a barycentre (which was created by us, above, as the star scan does not have barycentres in it)
                    // and the top has the barycentre

                    if (sn.Value.NodeType == ScanNodeType.barycentre && barycentrelist.Children.ContainsKey(keyid)) 
                    {
                        //System.Diagnostics.Debug.WriteLine(".. barycenter  " + keyid);
                        ScanNode tocopy = barycentrelist.Children[keyid];

                        System.Diagnostics.Debug.Assert(sn.Value.BodyDesignator.Contains("Created Barynode"));    // double check we are only altering stuff we made

                        if (sn.Value.Children == null)                      // we are altering sn, which is a barycentre, and made by us.
                            sn.Value.Children = new SortedList<string, ScanNode>();

                        foreach (var cc in tocopy.Children)
                        {
                            string cckey = cc.Key;
                            if (!sn.Value.Children.ContainsKey(cckey))                               // may have been moved already, because we don't remove top keys until finished
                            {
                                // System.Diagnostics.Debug.WriteLine(".. " + cckey + " " + cc.Value.fullname + " onto " + keyid);
                                sn.Value.Children.Add(cckey, cc.Value);
                                ExpandRecurivelyBarynodeTree(barycentrelist, sn.Value);     // we go down, but we don't record the list back, since we are only interested in ones moved from the barycentrelist
                            }
                            else
                            {
                                // System.Diagnostics.Debug.WriteLine(".. Dup move " + cckey + " " + cc.Value.fullname + " onto " + keyid);
                            }
                        }

                        keystodelete.Add(keyid);        // remove keyid from barycentrelist
                    }
                }

                return keystodelete;
            }
        } 

    }
}
