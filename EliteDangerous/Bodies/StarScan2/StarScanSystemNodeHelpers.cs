/*
 * Copyright 2025 - 2025 EDDiscovery development team - Robby
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemNode
    {

        #region Helpers
    
        // aim is to fill out the partnames with the same number of parents and scan (so 3 parents = 4 entries) and align them
        // this is more complicated to get right that it ever looks.
        static public void AlignParentsName(List<BodyParent> parents, string subname, out List<string> partnames)
        {
            partnames = BodyNode.ExtractPartsFromStandardName(subname);

            //$"Split Name to: {string.Join(", ", partnames)}".DO(lvl);

            // parents[0] is the parent of the scan, [1] is the grandfather, etc.. last is the star/null
            // partnames[0] is the furthest parent, [1] is the next, [n-2] is the father, [n-1] is the scan planet/moon/star/ring
            // so the two arrays are in the opposite order.
            // we move the partno backwards, ignoring the last entry since its the name of the scan planet/moon/star/ring

            int partno = partnames.Count - 1 - 1;       
           
            for (int i = 0; i < parents.Count; i++)
            {
                var nt = parents[i];

                // we only need to see if its a potential bary name if the node is a barycentre.
                // this is picking up stuff like Skaude AA-A h294 AB 1 a where the barycentre name is mentioned
                bool itsabarycentre = nt.IsBarycentre && partno >= 0 && partnames[partno].Length > 1 ? partnames[partno].HasAll(x => char.IsUpper(x)) : false;

                if (nt.IsBarycentre && (partno > 0 || !itsabarycentre))      // barycenter, either at sub part of name (past start) or its not a barycentre name at root
                {
                    //$"Skip insert barycentre at {i}".DO(lvl);
                    partnames.Insert(partno + 1, BodyNode.DefaultNameOfBC);
                }
                else if (partno >= 0)
                {
                    //$"Match {partsout[partno]} with {parents[i].Type}".DO(lvl);
                    partno--;
                }
                else
                {
                    //$"Out of name parts for {parents[i].Type}".DO(lvl);
                    partnames.Insert(0, nt.IsStar ? BodyNode.DefaultNameOfUnknownStar : nt.IsBarycentre ? BodyNode.DefaultNameOfBC : BodyNode.DefaultNameOfUnknownBody);
                }
            }

            //  $"Scan Name Normalised: {string.Join(", ", partnames)}".DO(lvl);
        }

        // stars declare belt clusters (called Belt)
        // planets declare planetary rings (which we add but actually do not display in system display)
        // Added as children of the body
        // the belt data is recorded in each body
        // belt data due to stupidity does not contain body id
        private void ProcessBeltsOrRings(BodyNode body, JournalScan sc, string bodyname, string systemname)
        {
            if (sc.HasRingsOrBelts)
            {
                foreach (StarPlanetRing ring in sc.Rings.DefaultIfEmpty())
                {
                    string name = ring.Name;

                    // for beltclusters, we simplify the naming to make them match between Rings[] structure and the name given in the scan for the beltclusterbody
                    // for rings, we ensure the ownname is A-D Ring

                    if (name.EndsWith("A Ring", StringComparison.InvariantCultureIgnoreCase))
                        name = "A Ring";
                    else if (name.EndsWith("B Ring", StringComparison.InvariantCultureIgnoreCase))
                        name = "B Ring";
                    else if (name.EndsWith("C Ring", StringComparison.InvariantCultureIgnoreCase))
                        name = "C Ring";
                    else if (name.EndsWith("D Ring", StringComparison.InvariantCultureIgnoreCase))
                        name = "D Ring";
                    else if (name.EndsWith("A Belt", StringComparison.InvariantCultureIgnoreCase))
                        name = "A Belt Cluster";
                    else if (name.EndsWith("B Belt", StringComparison.InvariantCultureIgnoreCase))
                        name = "B Belt Cluster";
                    else if (name.EndsWith("Galle Ring", StringComparison.InvariantCultureIgnoreCase) ||                // specials, just to remove warning message
                            name.EndsWith("Jupiter Halo Ring", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Asteroid Belt", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("The Belt", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Anahit Ring", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Vulcan Ring", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Castellan Belt", StringComparison.InvariantCultureIgnoreCase)
                            )
                    { 
                    }
                    else
                    {
                        string s = $"StarScan {body.Name()} Unconventional ring name {name}";
                        global::System.Diagnostics.Trace.WriteLine(s);
                    }

                    var belt = body.ChildBodies.Find(x => x.OwnName.EqualsIIC(name));           // so, it will be named for a standard naming scheme

                    if (belt == null)                                                   // can't find it by name
                    {
                        belt = new BodyNode(name, body.BodyType == BodyDefinitions.BodyType.Planet ? BodyDefinitions.BodyType.PlanetaryRing : BodyDefinitions.BodyType.StellarRing, BodyNode.BodyIDMarkerForAutoBodyBeltCluster, body, this, ring.Name);
                        body.ChildBodies.Add(belt);
                        $"  Add {belt.BodyType} object {name} to `{body.OwnName}`:{body.BodyID}".DO(debugid);
                    }

                    belt.SetScan(ring);
                }

                Sort(body);
            }
        }

        // body in wrong place. Reassign to new parent
        // Set the newparent to point to the child
        private void ReassignParent(BodyNode body, BodyNode newparent, int newbodyid)
        {
            (body != newparent).Assert("Reassign error");
            $"  Reassign {System.Name} body `{body.Name()}`:{body.BodyID} to `{newparent.Name()}` with new {newbodyid}".DO(debugid);

            CheckTree();

            BodyNode cur = body; // crucial Robert, don't mess about with body!

            while (cur.Parent != null)     // System has a parent of null, so this should stop it
            {
                //$"  Remove Incorrect body `{cur.Name()}`:{cur.BodyID}".DO();

                bodybyid.Remove(cur.BodyID);           // ensure body ID list is removed from body list - this will be added back in below for body code is simpler this way

                cur.Parent.ChildBodies.Remove(cur);       // remove this body at that point

                if (cur.Parent.BodyID < 0 && cur.Parent.ChildBodies.Count == 0)   // if the parent does not have a valid bodyid, and it has no children now, recurse up to it
                {
                    cur= cur.Parent;
                    //$"  .. recurse up to `{prevassigned.OwnName}`:{prevassigned.BodyID}".DO();
                }
                else
                    break;
            }

            body.ResetParent(newparent);        // point at new parent
            body.ResetBodyID(newbodyid);
            newparent.ChildBodies.Add(body);    // point at body
            if ( newbodyid>=0)
                bodybyid[body.BodyID] = body;   // ensure in bodybyid
        }

        // Extract, sort barycentre subnames into a list, remake the name of the BC 
        private static void AddBarycentreName(BodyNode cur, string subpart)
        {
            bool defname = cur.OwnName.StartsWith(BodyNode.DefaultNameOfBC);
            if (defname || cur.OwnName.StartsWith(BodyNode.BCNamingPrefix))       // if autonamed
            {
                string scut = cur.OwnName.Substring(defname ? BodyNode.DefaultNameOfBC.Length : BodyNode.BCNamingPrefix.Length);
                SortedSet<string> names = new SortedSet<string>(Comparer<string>.Create((a, b) => { return a.CompareAlphaInt(b); }));

                string[] list = scut.SplitNoEmptyStartFinish(',');
                foreach (var x in list)
                    names.Add(x.Trim());

                names.Add(subpart); // will remove dups

                string n = BodyNode.BCNamingPrefix + string.Join(", ", names);

                cur.ResetBodyName(n);
            }
        }

        // check the scan parents, and if they are a barycentre, see if we have the node and if so set up its barycentre ptr.
        // We may not have it yet due to sequencing (baryscan onto pending, scan in but node not assigned, pending calls AssignBaryCentreScanToScans(JournalScanBaryCentre sc) which then fills it in)
        private void AssignBaryCentreScanToScans(JournalScan sc)
        {
            foreach(var x in sc.Parents.EmptyIfNull())
            {
                if (x.IsBarycentre && bodybyid.TryGetValue(x.BodyID, out BodyNode bn) && bn.BarycentreScan != null)
                {
                    x.Barycentre = bn.BarycentreScan;
                    $"   .. Scan parent is a barycentre {x.BodyID} and we can assign a bary scan to it".DO(debugid);
                }
            }
        }

        // given a baryscentre scan, find all current scans which reference it and update the scan structure with this scan
        private void AssignBaryCentreScanToScans(JournalScanBaryCentre sc)
        {
            // find all other bodies with a parent list which mention this barycentre
            var scannodelist = Bodies(x => x.Scan?.Parents != null && x.Scan.Parents.FindIndex(y => y.BodyID == sc.BodyID) >= 0).ToList();

            if (scannodelist.Count == 0)
            {
                $"   .. No scans found with this barycentre in bodies list in {System.Name}".DO(debugid);
            }

            foreach (var scannode in scannodelist)
            {
                for (int i = 0; i < scannode.Scan.Parents.Count; i++)   // look thru the list, and assign at the correct level
                {
                    if (scannode.Scan.Parents[i].BodyID == sc.BodyID)
                    {
                        $"   .. Assign barycentre to scan node {scannode.Scan.BodyName}".DO(debugid);
                        scannode.Scan.Parents[i].Barycentre = sc;
                    }
                }
            }
        }

        // Sort tree
        private static void Sort(BodyNode cur)
        {
           // $"Sort tree for {cur.OwnName}:{cur.BodyID}".DO();
            cur.ChildBodies.Sort(delegate (BodyNode left, BodyNode right) { return left.CompareTo(right,false); });
        }

        #endregion

        #region System Node variables

        private Dictionary<int, BodyNode> bodybyid;
        private BodyNode systemBodies;
        private const string debugid = "StarScan";

        #endregion

        #region Debug

        public void DumpTree()
        {
            global::System.Diagnostics.Trace.WriteLine($"System `{System.Name}` {System.SystemAddress}: bodies {Bodies().Count()} ids {bodybyid.Count}");
            systemBodies.DumpTree("S", 0);
            CheckTree();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        public void CheckTree()
        { 
            int totalbodieswithids = 0;
            foreach( var x in Bodies())
            {
                (x.SystemNode == this).Assert($"StarScan System Node not assigned to this {x.Name()}");
                if ( x.BodyID>=0 )
                {
                    totalbodieswithids++;

                    if (bodybyid.TryGetValue(x.BodyID, out BodyNode v))
                    {
                        (v == x).Assert($"StarScan bodybyid not pointing to same place as ID for {x.Name()}");
                    }
                    else
                        false.Assert($"StarScan Missing bodyid in bodybyid {x.Name()}");
                }
            }

            (totalbodieswithids == bodybyid.Count).Assert($"StarScan {System.Name} Not the same number of bodyids as nodes {totalbodieswithids} with Ids in bodybyid {bodybyid.Count}");

            CheckParents(systemBodies);
        }

        public void CheckParents(BodyNode body)
        {
            foreach( var x in body.ChildBodies)
            {
                (x.Parent == body).Assert($"StarScan bodybyid not pointing to same place as ID for {x.Name()}");
                CheckParents(x);
            }
        }

        // output the image to a file (or just create it if path=null)
        public bool DrawSystemToFile(string path, int width = 1920,  bool materials = true)
        {
            StarScan2.SystemDisplay sd = new StarScan2.SystemDisplay();
            sd.Font = new System.Drawing.Font("Arial", 10);
            sd.SetSize(64);
            sd.ShowMaterials = materials;
            sd.TextBackColor = Color.Transparent;
            ExtendedControls.ExtPictureBox imagebox = new ExtendedControls.ExtPictureBox();
            imagebox.FillColor = Color.AliceBlue;
            sd.DrawSystemRender(imagebox, width, this);

            if (path != null && imagebox.Image != null)
                imagebox.Image.Save(path);

            return imagebox.Image != null;
        }

        #endregion

    }
}
