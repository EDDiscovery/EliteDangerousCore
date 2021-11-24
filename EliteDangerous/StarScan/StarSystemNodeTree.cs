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
        public partial class SystemNode
        {
            // generate a node tree with barycentres properly positioned for display
            public ScanNode OrderedSystemTree()
            {
                ScanNode rootnode = null;

                foreach (var kvp in StarNodes)  // first process star nodes to find root node
                {
                    ScanNode snode = kvp.Value;

                    // need some scan data with a valid body id, then we can discover the root node
                    if (snode.ScanData != null && snode.BodyID.HasValue)
                    {
                        Debug.WriteLine($"Star {snode.BodyID} '{snode.OwnName}' {snode.ScanData?.ParentList()}");

                        if (snode.ScanData.Parents != null) // if it has a parent list, it will tell us the root node as the last entry to it.
                        {
                            var pnode = snode.ScanData.Parents[snode.ScanData.Parents.Count - 1];
                            int pid = pnode.BodyID;

                            // make an entry for the top level object, which will be a barycentre (but check in case)
                            rootnode = new ScanNode("Node" + pid.ToStringInvariant(), pnode.BaryCentre ? ScanNodeType.barycentre : ScanNodeType.body, pid);  
                        }
                        else  // no parent list, this is a top level star, confirmed that entries without parent list are top level stars with BID = 0
                        {
                            rootnode = new ScanNode(snode);     // top level node created from snode with data
                        }
                    }
                }

                if ( rootnode != null )     // we have a root node, a star or a barycentre
                {
                    foreach (var kvp in StarNodes)  // go thru star list again
                    {
                        ScanNode snode = kvp.Value;

                        // if we have parents list, we find the parent, and assign as child
                        // if we did not have a parents list, then the rootnode is the top level star, and it already has the scan data assigned above

                        if (snode.ScanData?.Parents != null) 
                        {
                            var parentnode = FindParentAddToTree(rootnode, snode.ScanData.Parents, snode.ScanData.Parents.Count - 1);
                            string name = "Node" + snode.BodyID.ToStringInvariant();
                            var node = new ScanNode(snode);
                            parentnode.Children.Add(name, node);
                        }

                        if (snode.Children != null)
                            AddChildren(rootnode, snode.Children);
                    }
                }

                return rootnode;
            }

            private void AddChildren(ScanNode rootnode, SortedList<string, ScanNode> children)
            {
                foreach( var kvp in children)
                {
                    if (kvp.Value.ScanData?.Parents != null)    // need heirarchy
                    {
                        // find the parent of this body, using the parent list to find it. It will make parents to fit
                        ScanNode parent = FindParentAddToTree(rootnode, kvp.Value.ScanData.Parents, kvp.Value.ScanData.Parents.Count - 1);

                        string name = "Node" + kvp.Value.BodyID.ToStringInvariant();

                        var newnode = new ScanNode(kvp.Value);

                        if (parent.Children.ContainsKey(name))      // if parent already has child, it was made because we pre created the node tree
                        {                                           // probably does not happen due to layering already done in original structure. but best to cope with it
                            newnode.Children = parent.Children[name].Children;  // copy current children in, since we must have made it and have children
                            parent.Children[name] = newnode;        // reassign name to newnode
                        }
                        else
                        {
                            parent.Children.Add(name, newnode);
                        }

                        if (kvp.Value.Children != null)     // then go thru children
                            AddChildren(rootnode, kvp.Value.Children);
                    }
                }
            }


            // find or create the parent of the node. Pass the root node and level = plist.count-1
            // create nodes along the tree if not found.
            private ScanNode FindParentAddToTree(ScanNode node, List<JournalScan.BodyParent> plist, int level)
            {
                if (level > 0)          // level 0 is directly parent (node) so no need to continue
                {
                    var pnode = plist[level - 1];
                    int cid = pnode.BodyID;
                    int c = 0;
                    for (; c < node.Children.Count; c++)        // check children for cid
                    {
                        if (node.Children.Values[c].BodyID == cid)
                        {
                            return FindParentAddToTree(node.Children.Values[c], plist, level - 1);  // if so, recurse down
                        }
                    }

                    string name = "Node" + cid.ToStringInvariant();     // no cid, make one
                    ScanNode newnode = new ScanNode(name, pnode.BaryCentre ? ScanNodeType.barycentre : ScanNodeType.body, cid);

                    // if we have data on a barycentre, add a new JournalScan for it
                    if (BaryCentres.ContainsKey(cid))
                    {
                        newnode.ScanData = new JournalScan(BaryCentres[cid]);
                    }
                    else
                    {
                        Debug.WriteLine($"{node.OwnName} No scan data");
                    }

                    node.Children.Add(name, newnode);
                    return FindParentAddToTree(newnode, plist, level - 1);
                }
                else
                    return node;
            }



        }
    }
}
