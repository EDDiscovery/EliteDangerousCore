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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("{System.Name} {System.SystemAddress} Top level bodies {SystemBodies.ChildBodies.Count}")]
    public partial class SystemNode
    {
        public ISystem System { get; private set; }     // may not have XYZ, will always have name and systemaddress

        public bool HasCentreBarycentre { get { return SystemBodies.ChildBodies.Count == 1 && SystemBodies.ChildBodies[0].BodyType == BodyNode.BodyClass.Barycentre; } }

        public BodyNode SystemBodies { get; private set; } = new BodyNode("System", "System" , BodyNode.BodyClass.System, -1, null);       // root of all bodies


        public int? FSSTotalBodies { get; private set; }         // if we have FSSDiscoveryScan, this will be set
        public int? FSSTotalNonBodies { get; private set; }     // if we have FSSDiscoveryScan, this will be set

        public bool OldScansPresent { get; set; }               // if set, we have old scans

        public SystemNode(ISystem sys)
        {
            System = sys;
        }

        public void Clear()
        {
            SystemBodies = new BodyNode("System", "System", BodyNode.BodyClass.System, -1, null);       // clear
            FSSTotalBodies = FSSTotalNonBodies = null;
            bodybyid.Clear();
            global::System.Diagnostics.Debug.WriteLine($"Clear Scans of {System.SystemAddress} {System.Name}");
        }

        public BodyNode FindBodyInTree(int id)
        {
            bodybyid.TryGetValue(id, out BodyNode value);
            return value;
        }

        public BodyNode FindBodyInTree(int id,string fdname)
        {
            if (bodybyid.TryGetValue(id, out BodyNode value))
                return value;
            else
                return Bodies(x=>x.FDName.EqualsIIC(fdname), true).FirstOrDefault();
        }

        // return all bodies, or return all bodies matching a Predicate, or return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            return SystemBodies.Bodies(find, stoponfind);
        }
        

        // aim is to fill out the partnames with the same number of parents and scan (so 3 parents = 4 entries) and align them
        // this is more complicated to get right that it ever looks.

        static public void AlignParentsName(List<JournalScan.BodyParent> parents, string subname, out List<string> partnames)
        {
            partnames = new List<string>();

            StringParser sp = new StringParser(subname);

            while (!sp.IsEOL)
            {
                if (sp.IsStringMoveOn(out string found,StringComparison.InvariantCultureIgnoreCase,true, "A Belt Cluster", "B Belt Cluster","A Ring","B Ring"))
                {
                    partnames.Add(found);
                }
                else
                {
                    partnames.Add(sp.NextWord());
                }
            }

            //global::System.Diagnostics.Debug.WriteLine($"Split Name to: {string.Join(", ", partnames)}");

            int partno = partnames.Count - 1;       // backwards, since parents is

            // parents[0] is the parent of the scan, [1] is the grandfather, etc.. last is the star/null

            for (int i = -1; i < parents.Count; i++)
            {
                if (i == -1)          //  entry 0 is not in parents array (don't want to change it) and represents the scan ID itself, which the name must have
                {
                    //global::System.Diagnostics.Debug.WriteLine($"Match {partsout[partno]} with SCAN");
                    partno--;
                }
                else 
                {
                    var nt = parents[i];

                    // we only need to see if its a potential bary name if the node is a barycentre.
                    // this is picking up stuff like Skaude AA-A h294 AB 1 a where the barycentre name is mentioned
                    bool itsabarycentre = nt.IsBarycentre && partno >= 0 && partnames[partno].Length>1 ? partnames[partno].HasAll(x => char.IsUpper(x)) : false;

                    if (nt.IsBarycentre && (partno > 0 || !itsabarycentre))      // barycenter, either at sub part of name (past start) or its not a barycentre name at root
                    {
                        //global::System.Diagnostics.Debug.WriteLine($"Skip insert barycentre at {i}");
                        partnames.Insert(partno+1, DefaultNameOfBC);
                    }
                    else if (partno >= 0)
                    {
                        //global::System.Diagnostics.Debug.WriteLine($"Match {partsout[partno]} with {parents[i].Type}");
                        partno--;
                    }
                    else
                    {
                        //global::System.Diagnostics.Debug.WriteLine($"Out of name parts for {parents[i].Type}");
                        partnames.Insert(0, nt.IsStar ? DefaultNameOfUnknownStar : nt.IsBarycentre ? DefaultNameOfBC : DefaultNameOfUnknownBody);
                    }
                }
            }

          //  global::System.Diagnostics.Debug.WriteLine($"Scan Name Normalised: {string.Join(", ", partnames)}");
        }


        // Get or make a standard named body node Scheau Prao ME-M c22-21 A 1 a etc with a parents list
        // given a own name like "A 1 a" or "1 a" or "1" make nodes down the tree if they don't exist, return final node.
        // we never use this to make a top level star
        public BodyNode GetOrMakeStandardBodyNodeFromScan(JournalScan sc, string subname, string systemname)
        {
            // we align the parents field and the subname parts together

            AlignParentsName(sc.Parents, subname, out List<string> partnames);

            global::System.Diagnostics.Debug.WriteLine($"Scan `{sc.BodyName}`:{sc.BodyID} Name Normalised: {string.Join(", ", partnames)}");


            int pno = sc.Parents.Count - 1;
            BodyNode cur = SystemBodies;
            int partno = 0;

            // we go backwards thru the parents field, forwards thru the partname fields so they align, and pick them off

            while (pno >= 0)        
            {
                var nt = sc.Parents[pno];

                // checking we have not put it in the wrong place before (due to discrete adds)
            
                BodyNode prevassigned = FindBodyInTree(nt.BodyID);
                if (prevassigned != null && prevassigned.Parent != cur)
                {
                    RemoveIncorrectBody(prevassigned);

                }

                // see if its there..

                var subbody = cur.ChildBodies.Find(x => x.BodyID == nt.BodyID);

                //if (subbody == null) don't think need this
                    //subbody = cur.ChildBodies.Find(x => x.OwnName == partnames[partno]);

                // if not, make it

                if (subbody == null)
                {
                    subbody = new BodyNode(partnames[partno], partnames[partno], nt, nt.BodyID, cur);
                    cur.ChildBodies.Add(subbody);
                    bodybyid[nt.BodyID] = subbody;
                    global::System.Diagnostics.Debug.WriteLine($"  Add {subbody.BodyType} `{subbody.OwnName}` `{subbody.FDName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}");
                }
                else
                {
                    //if (subbody.BodyID != nt.BodyID)      // no need only finding by ID
                    //{
                    //    global::System.Diagnostics.Debug.Assert(subbody.BodyID == -1);
                    //    subbody.ResetBodyID(nt.BodyID);     
                    //    bodybyid[nt.BodyID] = subbody;
                    //}

                    // see if we now have a better name for the thing
                    if (subbody.FDName.StartsWith("Unknown ") && !partnames[partno].StartsWith("Unknown ") )
                    {
                        global::System.Diagnostics.Debug.WriteLine($"  Rename `{subbody.OwnName}`:{subbody.BodyID} `{subbody.FDName}` to fdname `{partnames[partno]}` in {systemname}");
                        subbody.ResetBodyName(subbody.OwnName, partnames[partno]);
                    }
                }

                cur = subbody;
                pno--;
                partno++;
            }

            // final part of the scan

            string ownname = partnames.Last();

            // check to see if in wrong place

            BodyNode body = FindBodyInTree(sc.BodyID.Value);
            if (body != null && body.Parent != cur)
            {
                RemoveIncorrectBody(body);
                body = null;
            }

            if (body == null)                                        // else try and match by name
                body = cur.ChildBodies.Find(x => x.OwnName.EqualsIIC(ownname));

            if (body == null)
            {
                // if we found it, but we did not find it here, its been placed in a default place by the system in
                // GetOrMakeNonStandardBodyNode.  We need to remove it, and possibly remove that auto star as well

                BodyNode prevassigned = FindBodyInTree(sc.BodyID.Value, sc.BodyName);

                if (prevassigned != null)
                    RemoveIncorrectBody(prevassigned);

                body = new BodyNode(ownname, subname, sc.IsStar ? BodyNode.BodyClass.Star : sc.IsPlanet ? BodyNode.BodyClass.PlanetMoon : BodyNode.BodyClass.PlanetaryRing, sc.BodyID.Value, cur);
                cur.ChildBodies.Add(body);
                bodybyid[sc.BodyID.Value] = body;

                global::System.Diagnostics.Debug.WriteLine($"  Add {body.BodyType} `{body.OwnName}` `{body.FDName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}");
            }
            else
            {
                if (body.BodyID != sc.BodyID.Value)     // check here to see if we have a bid for this node, and its not been set before. This occurs due to scans with parent arrays
                {
                    global::System.Diagnostics.Debug.Assert(body.BodyID == -1); // error we previously set it

                    //global::System.Diagnostics.Debug.WriteLine($"Reset body id of body `{body.OwnName}` from {body.BodyID} to {pbid}");
                    body.ResetBodyID(sc.BodyID.Value);
                    bodybyid[sc.BodyID.Value] = body;
                }

                body.ResetBodyName(ownname, subname);
            }

            if (cur.BodyType == BodyNode.BodyClass.Barycentre)     // we can adjust the name of the BC above if possible
            {
                AddBaryCentreName(cur, ownname);
            }

            if (sc != null)     // set scan before sort
                body.SetScan(sc);

            Sort(cur);          // resort parent

            ProcessBeltsOrRings(body, sc, sc.BodyName,systemname);     // finally any belts/cluster or planetary rings need adding

            return body;
        }

        // we have a lower level body with a parents tree
        public BodyNode GetOrMakeNonStandardBodyFromScan(JournalScan sc, string systemname, string ownname)
        {
            BodyNode cur = SystemBodies;

            int pno = (sc.Parents?.Count ?? 0) - 1;

            while(pno >= 0)
            {
                var nt = sc.Parents[pno];

                var plistbody = cur.ChildBodies.Find(x => x.BodyID == nt.BodyID);

                // if not, make it
                if (plistbody == null)
                {
                    // maybe we need to check for these as well... just see if its there somewhere but not here
                    BodyNode previdassigned = FindBodyInTree(nt.BodyID);

                    plistbody = new BodyNode(previdassigned?.OwnName ?? (nt.IsBarycentre ? DefaultNameOfBC : DefaultNameOfUnknownBody), previdassigned?.FDName ?? "",nt, nt.BodyID, cur);

                    if (previdassigned != null)
                    {
                        //previdassigned.MoveStarDataTo(plistbody);
                        RemoveIncorrectBody(previdassigned);
                    }

                    cur.ChildBodies.Add(plistbody);
                    bodybyid[nt.BodyID] = plistbody;
                    global::System.Diagnostics.Debug.WriteLine($"  Add Parent Tree Body {nt.BodyID} type {plistbody.BodyType} in {systemname} below `{cur.OwnName}`:{cur.BodyID}");
                }

                cur = plistbody;
                pno--;
            }

            // now at the parent node, see if we made it before
            var body = cur.ChildBodies.Find(x => x.BodyID == sc.BodyID.Value);

            if (body == null)
            {
                // have we previously made it, record that its there

                body = new BodyNode(sc.BodyName, sc.BodyName, sc.IsStar ? BodyNode.BodyClass.Star : sc.IsPlanet ? BodyNode.BodyClass.PlanetMoon : BodyNode.BodyClass.BeltClusterBody, sc.BodyID.Value, cur);

                // if we found it, but we did not find it here, its been placed in a default place by the system in
                // GetOrMakeNonStandardxxx.  We need to remove it, and possibly remove that auto star as well

                BodyNode prevassigned = FindBodyInTree(sc.BodyID.Value, sc.BodyName);
                if (prevassigned != null)
                    RemoveIncorrectBody(prevassigned);

                cur.ChildBodies.Add(body);
                bodybyid[sc.BodyID.Value] = body;
                global::System.Diagnostics.Debug.WriteLine($"  Add `{sc.BodyName}`:{sc.BodyID} type {body.BodyType} Scan below `{cur.OwnName}`:{cur.BodyID}");
            }
            else
            {
                if ( body.OwnName != sc.BodyName)
                    body.ResetBodyName(sc.BodyName,sc.BodyName);
            }

            if (cur.BodyType == BodyNode.BodyClass.Barycentre)     // we can adjust the name of the BC above if possible
            {
                AddBaryCentreName(cur, sc.BodyName);
            }
            
            ProcessBeltsOrRings(body,sc, sc.BodyName, systemname);

            body.SetScan(sc); // update or add scan BEFORE sorting - we may have added it before without a scan
            Sort(cur);          // then sort with into

            return body;
        }

        // assign, we def need a previous entry to be able to assign.  Null if not found
        public BodyNode AddBaryCentreScan(JournalScanBaryCentre sc)
        {
            BodyNode prevassigned = FindBodyInTree(sc.BodyID);
            if (prevassigned != null)
            {
                prevassigned.SetScan(sc);
                Sort(prevassigned.Parent);      // and resort
                global::System.Diagnostics.Debug.WriteLine($"  Add Baryscan to {sc.BodyID}");
                return prevassigned;
            }
            else
                return null;
        }


        // for IBodyNames without parent lists, marked as planet. 
        // check if its there by id or fdname, and then if not, make something up to hold it
        public BodyNode GetOrMakeDiscreteBody(string fdname, int? bid, string systemname)
        {
            BodyNode prevassigned = FindBodyInTree(bid??-99, fdname);

            if ( prevassigned == null)
            {
                BodyNode starbody = Bodies(x => x.BodyType == BodyNode.BodyClass.Star, true).FirstOrDefault();       // find a star, any star, anywhere

                if (starbody == null)
                {
                    starbody = new BodyNode(DefaultNameOfUnknownStar, "", BodyNode.BodyClass.Star, BodyIDMarkerForAutoStar, SystemBodies);       // assign under the main node
                    SystemBodies.ChildBodies.Add(starbody);
                    global::System.Diagnostics.Debug.WriteLine($"  Add Unknown Star for {fdname}:{bid} in {systemname}");
                }

                var body = new BodyNode(fdname, fdname, BodyNode.BodyClass.PlanetMoon, bid ?? -1, starbody);
                starbody.ChildBodies.Add(body);
                if (bid>=0)
                    bodybyid[bid.Value] = body;

                Sort(starbody);
                return body;
            }
            else
            {
                return prevassigned;
            }
        }

        //for IBodyNames without parent lists, marked as star
        //try and find it, if not, make something
        public BodyNode GetOrMakeDiscreteStar(string subname, int? bid, string systemname)
        {
            string cutname = subname.ReplaceIfStartsWith(systemname);

            BodyNode starbody = FindBodyInTree(bid ?? -99);
            if ( starbody == null) 
                starbody = Bodies(x => x.BodyType == BodyNode.BodyClass.Star && (x.FDName == subname || x.OwnName == cutname), true).FirstOrDefault();       // find a star with this name

            if (starbody == null)
            {
                starbody = new BodyNode(subname, subname, BodyNode.BodyClass.Star, bid ?? -1, SystemBodies);       // assign under the main node
                SystemBodies.ChildBodies.Add(starbody);
                if ( bid>=0)
                    bodybyid[bid.Value] = starbody;
                global::System.Diagnostics.Debug.WriteLine($"  Add Star `{subname}`:{bid} in {systemname}");

                var prevautomadestar = SystemBodies.ChildBodies.Where(x => x.BodyID == BodyIDMarkerForAutoStar).FirstOrDefault();       // find a previous auto star
                if (prevautomadestar != null)
                {
                    global::System.Diagnostics.Debug.WriteLine($"  Move tree of auto star to `{subname}`:{bid} in {systemname}");
                    prevautomadestar.MoveStarDataTo(starbody);
                }
            }

            return starbody;
        }

        // Add a codex entry to best place, if body id, find it, else system bodies
        public BodyNode AddCodexEntryToSystem(JournalCodexEntry sc)
        {
            if (sc.BodyID.HasValue)
            {
                BodyNode body = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
                if (body != null)
                {
                    body.AddCodex(sc);
                    return body;
                }
                else
                    return null;

            }
            else
            {
                SystemBodies.AddCodex(sc);          // SYstem bodies hold global info
                return SystemBodies;
            }
        }

        public BodyNode AddFSSBodySignalsToSystem(JournalFSSBodySignals sc)
        {
            if (sc.BodyID.HasValue)
            {
                BodyNode body = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
                if (body != null)
                {
                    body.AddSignals(sc.Signals);
                    return body;
                }
                else
                    return null;
            }
            else
                return null;
        }
        public BodyNode AddSAASignalsToSystem(JournalSAASignalsFound sc)
        {
            if (sc.BodyID.HasValue)
            {
                BodyNode body = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
                if (body != null)
                {
                    body.AddSignals(sc.Signals);
                    if (sc.Genuses?.Count > 0)
                        body.AddGenuses(sc.Genuses);
                    return body;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public void SetCount(int? bodycount, int? nonbodycount)
        {
            FSSTotalBodies = bodycount;
            FSSTotalNonBodies = nonbodycount;
        }

        public void AddFSSSignalsDiscovered(List<FSSSignal> signals)
        {
            SystemBodies.AddFSSSignals(signals);
        }
        public BodyNode AddScanOrganicToBody(JournalScanOrganic sc)
        {
            BodyNode body = Bodies(x => x.BodyID == sc.Body, true).FirstOrDefault();       // find a body
            if (body != null)
            {
                body.AddScanOrganics(sc);
                return body;
            }
            else
                return null;
        }
        public BodyNode AddSurfaceFeatureToBody(IBodyFeature sc)
        {
            BodyNode body = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
            if (body != null)
            {
                body.AddSurfaceFeature(sc);
                return body;
            }
            else
                return null;
        }

        // will only return non null if we have a scan, so we can
        public BodyNode AddSAAScanToBody(JournalSAAScanComplete sc)
        {
            BodyNode body = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
            if (body != null)
            {
                body.SetMapped(sc.ProbesUsed <= sc.EfficiencyTarget);
                if (body.Scan != null)      // if the scan is there, we can set the value, otherwise lets pretend we did not find it and let the pending system deal with it
                {
                    body.Scan.SetMapped(body.IsMapped, body.WasMappedEfficiently);
                    return body;
                }
            }

            return null;
        }

        public BodyNode AddDockingToBody(JournalDocked sc)
        {
            BodyNode bd = null;
            if (sc.BodyID.HasValue)
            {
                bd = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();       // find a body
                if (bd == null)
                    return null;            // don't have it now, so return try again
            }
            else
                bd = SystemBodies;

            bd.AddDocking(sc);
            return bd;
        }

        #region Helpers

        // Any rings around the bodies are added, including stars and planets, as children of the body
        // the belt data is recorded in each body
        private void ProcessBeltsOrRings(BodyNode body, JournalScan sc, string bodyname, string systemname)
        {
            if (sc.HasRingsOrBelts)
            {
                foreach (JournalScan.StarPlanetRing ring in sc.Rings)
                {
                    string beltname = ring.Name;

                    // naming fixes from previous code

                    if (beltname.StartsWith(bodyname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        beltname = beltname.Substring(bodyname.Length).Trim();
                    }
                    else if (bodyname.ToLowerInvariant() == "lave" && beltname.ToLowerInvariant() == "castellan belt")
                    {
                        beltname = "A Belt";
                    }

                    // naming fix to call it Belt Cluster, so it matches the name given in standard naming format used in the journal scan standard naming convention

                    if (beltname.Contains("Belt") && !beltname.Contains("Belt Cluster"))
                        beltname = beltname.Replace("Belt", "Belt Cluster");

                    global::System.Diagnostics.Debug.WriteLine($"  Add Belt/Ring object {beltname} to `{body.OwnName}`:{body.BodyID}");

                    var belt = body.ChildBodies.Find(x => x.OwnName == beltname);
                    if (belt == null)
                    {
                        string name = sc.BodyName.ReplaceIfStartsWith(systemname);
                        belt = new BodyNode(beltname, name + " " + beltname, body.BodyType == BodyNode.BodyClass.PlanetMoon ? BodyNode.BodyClass.PlanetaryRing : BodyNode.BodyClass.BeltCluster , -1, body);
                        body.ChildBodies.Add(belt);
                    }

                    belt.SetScan(ring);
                }

                Sort(body);
            }
        }

        // body in wrong place, remove it, and possible remove the star above if its an autostar
        // autostars are used if no other stars are available to hold a body
        private void RemoveIncorrectBody(BodyNode prevassigned)
        {
            if (prevassigned?.Parent != null)
            {
                global::System.Diagnostics.Debug.WriteLine($"  Remove Incorrect body `{prevassigned.OwnName}`:{prevassigned.BodyID}");

                prevassigned.Parent.ChildBodies.Remove(prevassigned);       // remove this body at that point

                if (prevassigned.Parent.BodyID == BodyIDMarkerForAutoStar && prevassigned.Parent.Parent != null)      // if it was made due to this
                {
                    global::System.Diagnostics.Debug.WriteLine($"  Remove Incorrect star");
                    prevassigned.Parent.Parent.ChildBodies.Remove(prevassigned.Parent);
                }
            }
        }

        // Extract, sort barycentre subnames into a list, remake the name of the BC 
        private static void AddBaryCentreName(BodyNode cur, string subpart)
        {
            bool defname = cur.OwnName.StartsWith(DefaultNameOfBC);
            if (defname || cur.OwnName.StartsWith(BCNamingPrefix))       // if autonamed
            {
                string scut = cur.OwnName.Substring(defname ? DefaultNameOfBC.Length : BCNamingPrefix.Length);
                SortedSet<string> names = new SortedSet<string>(Comparer<string>.Create((a, b) => { return a.CompareAlphaInt(b); }));

                string[] list = scut.SplitNoEmptyStartFinish(',');
                foreach (var x in list)
                    names.Add(x.Trim());

                names.Add(subpart); // will remove dups

                string n = BCNamingPrefix + string.Join(", ", names);

                cur.ResetBodyName(n,cur.FDName);
            }
        }

        private static void Sort(BodyNode cur)
        {
          //  global::System.Diagnostics.Debug.WriteLine($"Sort tree for {cur.OwnName}:{cur.BodyID}");
            Sort(cur.ChildBodies);
        //    cur.DumpTree(2);
        }

        private static void Sort(List<BodyNode> cur)
        { 
            cur.Sort(delegate (BodyNode left, BodyNode right)
                {
                    string lt = left.OwnName;
                    string rt = right.OwnName;
                    bool bcleft = false;
                    bool bcright = false;

                    // remove the BC of.. prefix to find the first name in the BC list - used for sort

                    if (lt.StartsWith(BCNamingPrefix))
                    {
                        lt = lt.Substring(BCNamingPrefix.Length);
                        int i = lt.IndexOf(',');
                        if (i >= 0)
                            lt = lt.Substring(0, i).Trim();
                        bcleft = true;
                    }

                    if (rt.StartsWith(BCNamingPrefix))
                    {
                        rt = rt.Substring(BCNamingPrefix.Length);
                        int i = rt.IndexOf(',');
                        if (i >= 0)
                            rt = rt.Substring(0, i).Trim();
                        bcright = true;
                    }

                   // global::System.Diagnostics.Debug.WriteLine($"Sort Compare `{lt}` with `{rt}`");

                    if (lt.Length == 1 && rt.Length == 1)      // 1-2-3 or a b c sort direct just comparing value
                    {
                        return lt.CompareTo(rt);
                    }
                    else
                    {
                        int? lv = lt.InvariantParseIntNull();
                        int? rv = rt.InvariantParseIntNull();
                        if (lv.HasValue && rv.HasValue)
                        {
                            return lv.Value.CompareTo(rv.Value);
                        }
                        else
                        {
                            double? smal = left.SMA;         // grab SMA from anything we have
                            double? smar = right.SMA;

                            if ( smal.HasValue && smar.HasValue)
                            {
                                //global::System.Diagnostics.Debug.WriteLine($"Sort Compare SMA `{lt}`:{smal}` with `{rt}`:{smar}");
                                return smal.Value.CompareTo(smar.Value);
                            }

                            if (lt.Contains("Belt Cluster"))        // clusters first
                            {
                                if (rt.Contains("Belt Cluster"))
                                    return lt.CompareTo(rt);
                                else
                                    return -1;
                            }
                            else if (rt.Contains("Belt Cluster"))
                                return 1;
                            else if (bcleft)                        // bc's to the end if they don't have SMAs
                            {
                                if (bcright)
                                    return lt.CompareTo(rt);            // default alpha
                                else
                                    return 1;
                            }
                            else if (bcright)
                                return -1;
                            else
                                return lt.CompareTo(rt);            // default alpha
                        }
                    }

                });

        }

        private const string DefaultNameOfBC = "Unknown Barycentre";
        private const string DefaultNameOfUnknownBody = "Unknown Body";
        private const string DefaultNameOfUnknownStar = "Unknown Star";
        private const string BCNamingPrefix = "BC of ";
        private const int BodyIDMarkerForAutoStar = -2;
        private Dictionary<int, BodyNode> bodybyid = new Dictionary<int, BodyNode>();

        public void DumpTree()
        {
            global::System.Diagnostics.Debug.WriteLine($"System `{System.Name}` {System.SystemAddress}: bodies {Bodies().Count()} ids {bodybyid.Count}");
            SystemBodies.DumpTree("S");
            //foreach(var kvp in bodybyid)
              //  global::System.Diagnostics.Debug.WriteLine($"  Map {kvp.Key} to `{kvp.Value.OwnName}` `{kvp.Value.FDName}`:{kvp.Value.BodyID}");
            
            //check bodyid mapping
            foreach( var x in Bodies())
            {
                if ( x.BodyID>=0 )
                {
                    if (bodybyid.TryGetValue(x.BodyID, out BodyNode v))
                    {
                        global::System.Diagnostics.Debug.Assert(v == x);
                    }
                    else
                        global::System.Diagnostics.Debug.Assert(false, "Missing bodyid");
                }
            }
        }

        #endregion

    }
}
