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
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("`{BodyNode.OwnName}` `{BodyNode.CanonicalName}` {BodyNode.BodyID} {BodyNode.BodyType} : CB{BodyNode.ChildBodies.Count} Cx{BodyNode.CodexEntries?.Count} FSS{BodyNode.FSSSignalList?.Count} Org{BodyNode.Organics?.Count} Gen {BodyNode.Genuses?.Count}")]
    public class NodePtr
    {
        public BodyNode BodyNode { get; set; }          // pointer to node
        public List<NodePtr> ChildBodies { get; set; } = new List<NodePtr>();   // to its child bodies
        public NodePtr Parent { get; set; }         // and its parent

        // GO down tree and make node ptrs
        public static NodePtr Bodies(BodyNode bn, NodePtr parent)
        {
            var x = new NodePtr();
            x.BodyNode = bn;
            x.Parent = parent;
            foreach (var bnc in bn.ChildBodies)
            {
                var bp = Bodies(bnc, x);
                x.ChildBodies.Add(bp);
            }

            return x;
        }

        // GO down tree and remove all barycentres, return BodyNodePtr tree
        public static NodePtr BodiesNoBarycentres(BodyNode bn, NodePtr parent)
        {
            var x = new NodePtr();
            x.BodyNode = bn;
            x.Parent = parent;
            foreach (var bnc in bn.ChildBodies)
            {
                var bp = BodiesNoBarycentres(bnc, x);

                if (bn.BodyType == BodyDefinitions.BodyType.Barycentre && x.Parent != null)      // if this is a BC, and we have a parent, move the items up 1 level to the parent
                    x.Parent.ChildBodies.Add(bp);
                else if (bp.BodyNode.BodyType != BodyDefinitions.BodyType.Barycentre)        // exclude all BCs from adds
                    x.ChildBodies.Add(bp);
            }

            return x;
        }

        public void Dump(string bid, int level = 0)
        {
            if ( level > 0)
            {
                if (BodyNode.BodyID < 0)
                    bid += ".??";
                else
                    bid += $".{BodyNode.BodyID,2}";

            }

            BodyNode.Dump(bid, level);

            foreach (var x in ChildBodies)
                x.Dump(bid, level + 1);
        }

        public static void Move(IEnumerable<Tuple<NodePtr,NodePtr>> movelist)
        {
            foreach( var kvp in movelist)
            {
                kvp.Item2.ChildBodies.Add(kvp.Item1);
                kvp.Item1.Parent.ChildBodies.Remove(kvp.Item1);
            }
        }

        public static void Sort(NodePtr np, bool ignoresma)
        {
            np.ChildBodies.Sort(delegate (NodePtr left, NodePtr right) { return left.BodyNode.CompareTo(right.BodyNode,ignoresma); });
        }


    }
}
