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
using BaseUtils.Win32Constants;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Web.UI.WebControls.WebParts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("{System.Name} {System.SystemAddress} {StarNodes.Count}")]
    public partial class SystemNode
    {
        public ISystem System { get; private set; }     // may not have XYZ, will always have name and systemaddress

        public bool HasCentreBarycentre { get { return SystemBodies.ChildBodies.Count == 1 && SystemBodies.ChildBodies[0].BodyType == BodyNode.BodyClass.Barycentre; } }

        // Sorted by star name
        // if a top level barycentre is found (body=0) it will be the first entry
        // Any bodies which can't be assigned to a star will be stored in a node with bodyid = -1

        public BodyNode SystemBodies { get; set; } = new BodyNode("System", BodyNode.BodyClass.System, -1, null);

        public SystemNode(ISystem sys)
        {
            System = sys;
        }

        public void ResetSystem(ISystem sys)
        {
            System = sys;
        }

        // return all bodies, or return all bodies matching a Predicate, or return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            return SystemBodies.Bodies(find, stoponfind);
        }

        private const string DefaultNameOfBC = "Unknown Barycentre";
        private const string DefaultNameOfUnknownBody = "Unknown Body";
        private const string DefaultNameOfUnknownStar = "Unknown Star";
        private const string BCNamingPrefix = "BC of ";
        private const int BodyIDMarkerForAutoStar = -2;


        // Get or make a standard named body node Scheau Prao ME-M c22-21 A 1 a etc
        // given a own name like "A 1 a" or "1 a" or "1" make nodes down the tree if they don't exist, return final node.
        // we never use this to make a top level star
        // journal scan is optional so it can be used for purposes other than scans
        // journal scan parents is optional so it can be used for older scans
        public BodyNode GetOrMakeStandardBodyNode(string subname, int? bid , string systemname, JournalScan sc = null)
        {
            BodyNode prevassigned = Bodies(x => x.OwnName.EqualsIIC(sc.BodyName) || (x.Scan?.BodyName.EqualsIIC(sc.BodyName) ?? false) || (bid.HasValue && x.BodyID == bid), true).FirstOrDefault();

            StringParser sp = new StringParser(subname);

            BodyNode cur = SystemBodies;

            // work out parents list, if scan has it
            var parents = sc?.Parents;
            int pno = (parents?.Count ?? 0) - 1;

            // for every part of the std name

            int partcount = 0;
            while (!sp.IsEOL)
            {
                string part = null;

                // detect the type from the name

                BodyNode.BodyClass cls = BodyNode.BodyClass.PlanetMoon;

                if (cur.BodyType == BodyNode.BodyClass.BeltCluster)     // if on a belt cluster it must be a ring
                {
                    cls = BodyNode.BodyClass.Ring;
                }
                if (sp.IsStringMoveOn("A Belt Cluster", StringComparison.InvariantCultureIgnoreCase))
                {
                    part = "A Belt Cluster";
                    cls = BodyNode.BodyClass.BeltCluster;
                }
                else if (sp.IsStringMoveOn("B Belt Cluster", StringComparison.InvariantCultureIgnoreCase))
                {
                    part = "B Belt Cluster";
                    cls = BodyNode.BodyClass.BeltCluster;
                }
                else if (sp.IsStringMoveOn("A Ring", StringComparison.InvariantCultureIgnoreCase))
                {
                    part = "A Ring";
                    cls = BodyNode.BodyClass.BodyRing;
                }
                else if (sp.IsStringMoveOn("B Ring", StringComparison.InvariantCultureIgnoreCase))
                {
                    part = "B Ring";
                    cls = BodyNode.BodyClass.BodyRing;
                }
                else
                {
                    part = sp.NextWord();

                    if (partcount == 0)
                    {
                        if (part.Length > 1)
                        {
                            bool itsabarycentre = true;
                            foreach (char x in part)
                            {
                                if (x < 'A' || x > 'Z')     // all upper case name its a barycentre
                                {
                                    itsabarycentre = false;
                                    break;
                                }
                            }

                            if (itsabarycentre)
                                cls = BodyNode.BodyClass.Barycentre;
                        }
                        else if (part.Length == 1 && char.IsLetter(part[0]))        // single char letter names at start are stars
                        {
                            cls = BodyNode.BodyClass.Star;
                        }
                    }
                }

                // we need to get the pno to the correct place first off
                // the name never includes the centre barycentre
                // the name only ever includes the last BC (AB) etc
                // if its '2' (.Body) then the star is implied and we need to move past the star
                // if its 'A' (.Star) then the star is given 

                if ( partcount == 0 )
                {
                    while (pno >= 0)
                    {
                        var nt = sc.Parents[pno];
                        var nextnt = pno>0 ? sc.Parents[pno-1] : null;

                        // done this way for sanity

                        // if name is BC, we are on a Barycentre, next is not a barycentre
                        bool stop1 = cls == BodyNode.BodyClass.Barycentre && (nt.IsBarycentre && (nextnt==null || !nextnt.IsBarycentre));

                        // if name is star, we want to stop on a star
                        bool stop2 = cls == BodyNode.BodyClass.Star && nt.IsStar;

                        // if name is a ring, we want to stop on a ring
                        bool stop3 = cls == BodyNode.BodyClass.BeltCluster && nt.IsRing;

                        // if name is a planet, we want to stop on a planet
                        bool stop4 = cls == BodyNode.BodyClass.PlanetMoon && nt.IsPlanet;

                        if ( !stop1 && !stop2 && !stop3 && !stop4 )
                        {
                            var subbody = cur.ChildBodies.Find(x => x.BodyID == nt.BodyID);

                            // if not, make it
                            if (subbody == null)
                            {
                                subbody = new BodyNode((nt.IsBarycentre ? DefaultNameOfBC : DefaultNameOfUnknownBody), nt, nt.BodyID, cur);
                                cur.ChildBodies.Add(subbody);
                                global::System.Diagnostics.Debug.WriteLine($"  Add {nt.BodyID} type {subbody.BodyType} in {systemname} below {cur.OwnName}");
                            }

                            cur = subbody;
                            pno--;
                        }
                        else
                            break;
                    }
                }

                bool lastpart = sp.IsEOL;
                int fbid = pno >= 0 ? parents[pno].BodyID : lastpart ? (bid ?? -1) : -1;

                BodyNode body = null;

                if (fbid != -1)                                           // if we have an ID
                    body = cur.ChildBodies.Find(x => x.BodyID == fbid);        // first try and match using BID, as we have instances of strange naming of stars

                if (cur == null)                                        // else try and match by name
                    body = cur.ChildBodies.Find(x => x.OwnName.EqualsIIC(part));

                if (body == null)
                {
                    // if we found it, but we did not find it here, its been placed in a default place by the system in
                    // GetOrMakeNonStandardBodyNode.  We need to remove it, and possibly remove that auto star as well
                    if (prevassigned?.Parent != null)
                        RemoveIncorrectBody(prevassigned);

                    body = new BodyNode(part, cls, fbid, cur);

                    cur.ChildBodies.Add(body);
                    global::System.Diagnostics.Debug.WriteLine($"  Add {cls} `{part}` name `{part}`:{fbid} below `{cur.OwnName}`:{cur.BodyID} in {systemname}");
                }
                else
                {
                    if (fbid != -1 && body.BodyID != fbid)     // check here to see if we have a bid for this node, and its not been set before. This occurs due to scans with parent arrays
                    {
                        global::System.Diagnostics.Debug.Assert(body.BodyID == -1); // error we previously set it

                        //global::System.Diagnostics.Debug.WriteLine($"Reset body id of body `{body.OwnName}` from {body.BodyID} to {pbid}");
                        body.ResetBodyID(fbid);
                    }
                }

                if (cur.BodyType == BodyNode.BodyClass.Barycentre)     // we can adjust the name of the BC above if possible
                {
                    AddBaryCentreName(cur, part);
                }

                if (lastpart && sc != null)     // set scan before sort
                    body.SetScan(sc);

                Sort(cur);          // resort

                cur = body;
                pno--;
                partcount++;
            }

            return cur;
        }

        // we have a lower level body with a parents tree
        public BodyNode GetOrMakeNonStandardBodyFromScan(JournalScan sc, ISystem sys, string ownname)
        {
            BodyNode cur = SystemBodies;

            int pno = (sc.Parents?.Count ?? 0) - 1;

            while(pno >= 0)
            {
                var nt = sc.Parents[pno];

                var body = cur.ChildBodies.Find(x => x.BodyID == nt.BodyID);

                // if not, make it
                if (body == null)
                {
                    // maybe we need to check for these as well... just see if its there somewhere but not here
                    BodyNode previdassigned = Bodies(x => x.BodyID == nt.BodyID, true).FirstOrDefault();

                    body = new BodyNode(previdassigned?.OwnName ?? (nt.IsBarycentre ? DefaultNameOfBC : DefaultNameOfUnknownBody), nt, nt.BodyID, cur);

                    if (previdassigned != null)
                    {
                        if (previdassigned.Scan != null)               // paranoid copying of scan data
                            body.SetScan(previdassigned.Scan);
                        if (previdassigned.BarycentreScan != null)
                            body.SetScan(previdassigned.BarycentreScan);

                        RemoveIncorrectBody(previdassigned);
                    }

                    cur.ChildBodies.Add(body);
                    global::System.Diagnostics.Debug.WriteLine($"  Add Unknown Body {nt.BodyID} type {body.BodyType} in {sys.Name} below {cur.OwnName}");
                }

                cur = body;
                pno--;
            }

            // now at the parent node, see if we made it before
            var newbody = cur.ChildBodies.Find(x => x.BodyID == sc.BodyID.Value);

            if (newbody == null)
            {
                // have we previously made it, record that its there

                newbody = new BodyNode(sc.BodyName, sc.IsStar ? BodyNode.BodyClass.Star : sc.IsPlanet ? BodyNode.BodyClass.PlanetMoon : BodyNode.BodyClass.Ring, sc.BodyID.Value, cur);

                // if we found it, but we did not find it here, its been placed in a default place by the system in
                // GetOrMakeNonStandardxxx.  We need to remove it, and possibly remove that auto star as well
                BodyNode prevassigned = Bodies(x => x.OwnName.EqualsIIC(sc.BodyName) || (x.Scan?.BodyName.EqualsIIC(sc.BodyName) ?? false) || (x.BodyID == sc.BodyID), true).FirstOrDefault();
                if (prevassigned?.Parent != null)
                    RemoveIncorrectBody(prevassigned);

                cur.ChildBodies.Add(newbody);
                global::System.Diagnostics.Debug.WriteLine($"  Add NonStd Body `{sc.BodyName}`:{sc.BodyID} type {newbody.BodyType} Scan below {cur.OwnName}");
            }
            else
            {
                if ( newbody.OwnName != sc.BodyName)
                    newbody.ResetBodyName(sc.BodyName);
            }

            if (cur.BodyType == BodyNode.BodyClass.Barycentre)     // we can adjust the name of the BC above if possible
            {
                AddBaryCentreName(cur, sc.BodyName);
            }

            newbody.SetScan(sc); // update or add scan BEFORE sorting - we may have added it before without a scan
            Sort(cur);          // then sort with into

            return newbody;
        }

        // assign, we def need a previous entry to be able to assign.  Null if not found
        public BodyNode AddBaryCentreScan(JournalScanBaryCentre sc)
        {
            BodyNode prevassigned = Bodies(x => x.BodyID == sc.BodyID, true).FirstOrDefault();
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


        // for IBodyNames without parent lists, marked as planet
        //it has a name like Miterrand Hollow, so assign to top star.This is the best we can do without Parent list.
        public BodyNode GetOrMakeNonStandardBody(string subname, int? bid, string systemname)
        {
            BodyNode starbody = Bodies(x => x.BodyType == BodyNode.BodyClass.Star, true).FirstOrDefault();       // find a star, any star
            if (starbody == null)
            {
                starbody = new BodyNode(DefaultNameOfUnknownStar, BodyNode.BodyClass.Star, BodyIDMarkerForAutoStar, SystemBodies);       // assign under the main node
                SystemBodies.ChildBodies.Add(starbody);
                global::System.Diagnostics.Debug.WriteLine($"  Add Unknown Star for {subname}:{bid} in {systemname}");
            }

            var body = bid.HasValue ? Bodies(x=>x.BodyID == bid).FirstOrDefault(): null;                                 // if has bid, search on bid

            if ( body == null )
                body = Bodies(x => x.OwnName.EqualsIIC(subname)).FirstOrDefault();                   // else search name

            if (body == null)
            {
                body = new BodyNode(subname, BodyNode.BodyClass.PlanetMoon, bid ?? -1, starbody);
                starbody.ChildBodies.Add(body);
                Sort(starbody);
            }

            return body;
        }

        // for IBodyNames without parent lists, marked as star
        //it has a name like Sol, so assign as top star.This is the best we can do without Parent list.
        public BodyNode GetOrMakeNonStandardStar(string subname, int? bid, string systemname)
        {
            BodyNode starbody = Bodies(x => x.BodyType == BodyNode.BodyClass.Star && x.OwnName == subname, true).FirstOrDefault();       // find a star with this name
            if (starbody == null)
            {
                starbody = new BodyNode(subname, BodyNode.BodyClass.Star, bid ?? -1, SystemBodies);       // assign under the main node
                SystemBodies.ChildBodies.Add(starbody);
                global::System.Diagnostics.Debug.WriteLine($"  Add Star {subname}:{bid} in {systemname}");

                var prevautomadestar = SystemBodies.ChildBodies.Where(x => x.BodyID == BodyIDMarkerForAutoStar).FirstOrDefault();       // find a previous auto star
                if (prevautomadestar != null)
                {
                    global::System.Diagnostics.Debug.WriteLine($"  Move tree of auto star to {subname}:{bid} in {systemname}");
                    starbody.ResetChildBodies(prevautomadestar.ChildBodies);                                  // if so, copy across to named star
                }
            }

            return starbody;
        }


        #region Helpers

        private void RemoveIncorrectBody(BodyNode prevassigned)
        {
            if (prevassigned?.Parent != null)
            {
                global::System.Diagnostics.Debug.WriteLine($"  Remove Incorrect body {prevassigned.OwnName}:{prevassigned.BodyID}");

                prevassigned.Parent.ChildBodies.Remove(prevassigned);       // remove this body at that point

                if (prevassigned.Parent.BodyID == BodyIDMarkerForAutoStar && prevassigned.Parent.Parent != null)      // if it was made due to this
                    prevassigned.Parent.Parent.ChildBodies.Remove(prevassigned.Parent);
            }
        }

        // Extract, sort barycentre subnames into a list
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

                cur.ResetBodyName(BCNamingPrefix + string.Join(", ", names));
            }
        }

        private static void Sort(BodyNode cur)
        {
            global::System.Diagnostics.Debug.WriteLine($"Sort tree for {cur.OwnName}:{cur.BodyID}");
            Sort(cur.ChildBodies);
            cur.DumpTree(2);
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
                            double? smal = left.Scan?.nSemiMajorAxis ?? left.BarycentreScan?.SemiMajorAxis;         // grab SMA from anything we have
                            double? smar = right.Scan?.nSemiMajorAxis ?? right.BarycentreScan?.SemiMajorAxis;

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

        public void DumpTree()
        {
            global::System.Diagnostics.Debug.WriteLine($"System {System.Name} {System.SystemAddress}:");
            SystemBodies.DumpTree(1);
        }

        #endregion
    }
}
