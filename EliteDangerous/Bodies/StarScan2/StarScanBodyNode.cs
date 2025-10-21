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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("`{OwnName}` {BodyID} {BodyType}")]

    public partial class BodyNode
    {
       // public string BodyName { get; private set; }
        public string OwnName { get; private set; }
        public int BodyID { get; private set; }

        public enum BodyClass { System, Star, Barycentre, PlanetMoon, Ring, BeltCluster, BodyRing };
        public BodyClass BodyType { get; private set; }
        public List<BodyNode> ChildBodies { get; private set; } = new List<BodyNode>();

        public BodyNode Parent { get; private set; } = null;            // null if top level body

        public JournalScan Scan { get; private set; } = null;
        public JournalScanBaryCentre BarycentreScan { get; private set; } = null;

        public BodyNode(string ownname, BodyClass bc, int bid, BodyNode parent)
        {
            OwnName = ownname;
            BodyType = bc;
            BodyID = bid;
            Parent = parent;
        }

        public BodyNode(string ownname, JournalScan.BodyParent nt, int bid, BodyNode parent)
        {
            OwnName = ownname;
            BodyType = nt.IsStar ? BodyNode.BodyClass.Star : nt.IsBarycentre ? BodyNode.BodyClass.Barycentre : nt.IsRing ? BodyNode.BodyClass.Ring : BodyNode.BodyClass.PlanetMoon;
            BodyID = bid;
            Parent = parent;
        }

        public void ResetBodyID(int bid)
        {
            BodyID = bid;
        }

        public void ResetBodyName(string name)
        {
            OwnName = name;
        }
        public void ResetChildBodies(List<BodyNode> nodes)
        {
            ChildBodies = nodes;    
        }

        public void SetScan(JournalScan sc)
        {
            Scan = sc;
        }
        public void SetScan(JournalScanBaryCentre sc)
        {
            BarycentreScan = sc;
        }

        // return all bodies, or return all bodies matching a Predicate, or return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            foreach (BodyNode sn in ChildBodies)
            {
                // global::System.Diagnostics.Debug.WriteLine($"TYield {sn.OwnName} bid {sn.BodyID}");

                if (find == null || find(sn))
                {
                    yield return sn;
                    if (find != null && stoponfind)
                        yield break;
                }

                foreach (BodyNode c in sn.Bodies(find,stoponfind))          // recurse back up to go as deep as required
                {
                    if (find == null || find(c))
                    {
                        //  global::System.Diagnostics.Debug.WriteLine($"CYield {c.OwnName} bid {c.BodyID}");
                        yield return c;

                        if (find != null && stoponfind)
                            yield break;
                    }
                }
            }
        }

        public void DumpTree(int level)
        {
            var pad = new string(' ', level*2);
            System.Diagnostics.Debug.WriteLine($"{pad}`{OwnName}` type {BodyType} bid {BodyID} scname:`{Scan?.BodyName}` sma {Scan?.SemiMajorAxisLSKM ?? BarycentreScan?.SemiMajorAxisLSKM}");
            foreach(var x in ChildBodies) 
                x.DumpTree(level + 1);
        }
    }
}
