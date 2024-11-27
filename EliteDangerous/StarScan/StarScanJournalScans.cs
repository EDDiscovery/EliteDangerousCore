/*ye
 * Copyright © 2015 - 2023 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        // find best system for scan.  Note startindex might be -1, if only 1 entry is in the list
        public bool AddScanToBestSystem(JournalScan je, int startindex, List<HistoryEntry> hl, out HistoryEntry he, out JournalLocOrJump jl)
        {
            he = null;
            jl = null;

            if (je?.BodyName == null)
                return false;

            // go thru the list of history entries looking for a Loc 

            for (int j = startindex; j >= 0; j--)   // same as FindBestSystem
            {
                he = hl[j];

                if (he.IsLocOrJump)
                {
                    jl = (JournalLocOrJump)he.journalEntry;

                    // get the body designation, given the je/system name

                    string designation = BodyDesignations.GetBodyDesignation(je, he.System.Name);
                    System.Diagnostics.Debug.Assert(designation != null);

                    // either the name/sys address matches, or the designation matches the star of the system name
                    if (je.IsStarNameRelated(he.System.Name,he.System.SystemAddress, designation))       
                    {
                        je.BodyDesignation = designation;
                        return ProcessJournalScan(je, he.System, true);
                    }
                    else if (jl.StarSystem != null && je.IsStarNameRelated(jl.StarSystem, jl.SystemAddress, designation)) // if we have a starsystem name, and its related, its a rename, ignore it
                    {
                        System.Diagnostics.Trace.WriteLine($"Rejecting body {designation} ({je.BodyName}) in system {he.System.Name} => {jl.StarSystem} due to system rename");
                        return false;
                    }
                }
            }

            startindex = Math.Max(0, startindex);       // if only 1 in list, startindex will be -1, so just pick first one

            je.BodyDesignation = BodyDesignations.GetBodyDesignation(je, hl[startindex].System.Name);

            return ProcessJournalScan(je, hl[startindex].System, true);         // no relationship, add..
        }

        // take the journal scan and add it to the node tree
        // handle Earth, starname = Sol
        // handle Eol Prou LW-L c8-306 A 4 a and Eol Prou LW-L c8-306
        // handle Colonia 4 , starname = Colonia, planet 4
        // handle Aurioum B A BELT
        // Kyloasly OY-Q d5-906 13 1

        private bool ProcessJournalScan(JournalScan sc, ISystem sys, bool reprocessPrimary = false, ScanNode oldnode = null)  // background or foreground.. FALSE if you can't process it
        {
          //  System.Diagnostics.Debug.WriteLine($"ProcessJournalScan `{sc.BodyName}` => `{sc.BodyDesignation}` : `{sys.Name}` {sys.SystemAddress}");

            if (sc.SystemAddress.HasValue)
            {
                if (sys.SystemAddress.HasValue && sc.SystemAddress.Value != sys.SystemAddress.Value)
                {
                    System.Diagnostics.Trace.WriteLine($"StarScan Rejected {sc.BodyName} in {sc.SystemAddress} {sc.StarSystem} due to not matching current system address {sys.SystemAddress} {sys.Name}");
                    return false;
                }
            }
            else if ( sc.BodyName.StartsWith("Procyon") && sys.Name == "Sol")       // this occurred in 2018 in Robby's journals
            {
                System.Diagnostics.Trace.WriteLine($"StarScan Rejected {sc.BodyName} in {sc.SystemAddress} {sc.StarSystem} due to bad scan in {sys.SystemAddress} {sys.Name}");
                return false;
            }

            // Extract elements from name, and extract if belt, top node type, if ring 
            List<string> elements = ExtractElementsJournalScan(sc, sys, out ScanNodeType starscannodetype, out bool isbeltcluster, out bool isring);

            // Bail out if no elements extracted
            if (elements.Count == 0)
            {
                System.Diagnostics.Trace.WriteLine($"StarScan Failed to add body {sc.BodyName} to system {sys.Name} - not enough elements");
                return false;
            }
            // Bail out if more than 5 elements extracted
            else if (elements.Count > 5)
            {
                System.Diagnostics.Trace.WriteLine($"StarScan Failed to add body {sc.BodyName} to system {sys.Name} - Versip");
                return false;
            }

            // goodish system, create node

            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                // Get custom name if different to designation. Will be null if not an unique name
                string bodyname = GetBodyNameJournalScan(sc, sys);

                // Process elements, 
                ScanNode node = ProcessElementsJournalScan(sc, sys, systemnode, bodyname, elements, starscannodetype, isbeltcluster, isring, oldnode);

                if (node.BodyID != null)
                {
                    systemnode.NodesByID[(int)node.BodyID] = node;
                }

                // Process top-level star
                if (elements.Count == 1)
                {
                    // Process any belts if present
                    ProcessBelts(sc, systemnode, node);

                    // Process primary star in multi-star system
                    if (elements[0].Equals("A", StringComparison.InvariantCultureIgnoreCase))
                    {
                        BodyDesignations.CachePrimaryStar(sc, sys);

                        // Reprocess if we've encountered the primary (A) star and we already have a "Main Star", we reprocess to 
                        // allow any updates to PrimaryCache to make a difference

                        if (reprocessPrimary && systemnode.StarNodes.Any(n => n.Key.Length > 1 && n.Value.NodeType == ScanNodeType.star))
                        {
                            // get bodies with scans
                            List<ScanNode> bodies = systemnode.Bodies().Where(b => b.ScanData != null).ToList();

                            // reset the nodes to zero
                            systemnode.ClearChildren();

                            foreach (var body in bodies)              // replay into process the body scans.. using the newly updated body designation (primary star cache) to correct any errors
                            {
                                var js = body.ScanData;
                                js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, systemnode.System.Name);
                                ProcessJournalScan(js, systemnode.System, oldnode: body);
                            }
                        }
                    }
                }
            }

            ProcessedSaved();  // any saved JEs due to no scan, add

            return true;
        }

        // extract elements of the name and form into an element array
        // element[0] is the star or barycentre
        // element[0] star/barycentre [ element[1] planet [ element[2] submoon [ element[3] subsubmoon ]]]
        // if belt cluster, we get [0] = star, [1] = belt, [2] = cluster N
        // if ring, we get [0] = star, [2] = body, ... [last-1] = ring name

        private List<string> ExtractElementsJournalScan(JournalScan sc, ISystem sys, out ScanNodeType starscannodetype, out bool isbeltcluster, out bool isring)
        {
            starscannodetype = ScanNodeType.star;
            isbeltcluster = false;
            isring = false;
            List<string> elements;

            // extract any relationship between the system we are in and the name, and return it
            // body designation is preferred (like Sol 1) over bodyname, which is what frontier call it
            
            string rest = sc.RemoveStarnameFromDesignation(sys.Name, sys.SystemAddress);      

            if (rest != null)                                   // if we have a relationship between the system name and the body name
            {
                if (sc.IsStar && !sc.IsWebSourced && sc.DistanceFromArrivalLS == 0 && rest.Length >= 2)       // star, primary, with name >= 2 (AB)
                {
                    elements = new List<string> { rest };       // its a star, default
                }
                else if (rest.Length > 0)                       // we have characters in rest, and its related to the system name
                {
                    elements = rest.Split(' ').ToList();        // split into spaced parts

                    if (elements.Count == 4 && elements[0].Length == 1 && char.IsLetter(elements[0][0]) &&          // A belt cluster N
                            elements[1].Equals("belt", StringComparison.InvariantCultureIgnoreCase) &&
                            elements[2].Equals("cluster", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elements = new List<string> { MainStar, elements[0] + " " + elements[1], elements[2] + " " + elements[3] };     // reform into Main Star | A belt | Cluster N
                        isbeltcluster = true;
                        //System.Diagnostics.Debug.WriteLine($"Belt cluster depth 1 {sc.BodyName} : {string.Join(",", elements)}");
                    }
                    else if (elements.Count == 5 && elements[0].Length >= 1 &&                                      // AA A belt cluster N
                            elements[1].Length == 1 && char.IsLetter(elements[1][0]) &&
                            elements[2].Equals("belt", StringComparison.InvariantCultureIgnoreCase) &&
                            elements[3].Equals("cluster", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elements = new List<string> { elements[0], elements[1] + " " + elements[2], elements[3] + " " + elements[4] };      // reform into <star> | A belt | Cluster N
                        isbeltcluster = true;
                        //System.Diagnostics.Debug.WriteLine($"Belt cluster depth 2 {sc.BodyName} : {string.Join(",", elements)}");
                    }
                    else if (elements.Count >= 3 &&
                             elements[elements.Count - 1].Equals("ring", StringComparison.InvariantCultureIgnoreCase) &&        // A 2 A ring
                             elements[elements.Count - 2].Length == 1 &&
                             char.IsLetter(elements[elements.Count - 2][0]))
                    {                                                                                               // reform into A | 2 | A ring three level or A A ring
                        elements = elements.Take(elements.Count - 2).Concat(new string[] { elements[elements.Count - 2] + " " + elements[elements.Count - 1] }).ToList();
                        isring = true;
                        //System.Diagnostics.Debug.WriteLine($"Ring depth N-1 {sc.BodyName} : {string.Join(",", elements)}");
                    }

                    if (char.IsDigit(elements[0][0]))                                   // if digits, planet number, no star designator
                        elements.Insert(0, MainStar);                                   // no star designator, main star, add MAIN
                    else if (elements[0].Length > 1 && elements[0] != MainStar)         // designator, is it multiple chars.. its a barycentre ABC
                        starscannodetype = ScanNodeType.barycentre;                     // override node type to barycentre
                }
                else
                {
                    elements = new List<string>();                                      // only 1 item, the star, which is the same as the system name..
                    elements.Add(MainStar);                                             // Sol / SN:Sol should come thru here
                }
            }
            else if (sc.IsStar && !sc.IsWebSourced && sc.DistanceFromArrivalLS == 0)      // name has no relationship to system (Gliese..) and its a star at LS=0
            {
                elements = new List<string> { sc.BodyName };                            // its a star
            }
            else
            {                                                                           // name has no relationship to system (Earth) but its not at LS=0
                elements = sc.BodyName.Split(' ').ToList();                             // use all bodyparts, and 
                elements.Insert(0, MainStar);                                           // insert the MAIN designator as the star designator
            }

            //System.Diagnostics.Debug.WriteLine($".. Extract Elements `{sc.BodyName}` => `{sc.BodyDesignation}` -> `{rest}` -> { starscannodetype} isbc:{isbeltcluster} isring:{isring} -> {string.Join(" | ",elements)}");
            return elements;
        }

        // see above for elements
        // protected by global lock on system node

        private ScanNode ProcessElementsJournalScan(JournalScan sc, ISystem sys, SystemNode systemnode, string bodyname, List<string> elements, 
                                                    ScanNodeType starscannodetype, bool isbeltcluster, bool isring, ScanNode oldnode = null)
        {
            //System.Diagnostics.Debug.WriteLine($".. ProcessElements bodyname:{bodyname} ID:{sc.BodyID}");

            List<JournalScan.BodyParent> ancestors = sc.Parents?.AsEnumerable()?.ToList();      // this includes Rings, Barycentres(Nulls) that frontier put into the list..

            // remove all rings and barycenters first, since thats not in our element list. We just want the bodies and belts
            List<JournalScan.BodyParent> ancestorbodies = ancestors?.Where(a => a.Type == "Star" || a.Type == "Planet" || a.Type == "Belt")?.Reverse()?.ToList();

            // but we need to add back the barycenter at the top, since we do add that that in the element list
            if (ancestorbodies != null && ancestorbodies.Count>0 && starscannodetype == ScanNodeType.barycentre)      
            {                                                                               
               // this checks out, but disable for safety.  System.Diagnostics.Debug.Assert(ancestors[ancestors.Count - 1].Type == "Null");     // double check its a barycentre, it should be
                ancestorbodies.Insert(0, ancestors[ancestors.Count - 1]);
            }

            // for each element we process into the tree

            SortedList<string, ScanNode> currentnodelist = systemnode.StarNodes;            // current operating node list, always made
            ScanNode previousnode = null;                                                   // trails subnode by 1 to point to previous node

            for (int lvl = 0; lvl < elements.Count; lvl++)                                  // run thru the found elements..
            {
                ScanNodeType sublvtype = starscannodetype;                                  // top level, element[0] type is starscannode (star/barycentre)    

                if (lvl > 0)                                                                // levels > 0, we need to determine what it is    
                {
                    if (isbeltcluster)                                                      // a belt cluster is in three levels (star, belt, cluster)
                    {
                        if (lvl == 1)                                                       // level 1, call it belt - belt clusters come out with elements Main Star,A Belt,Cluster 4
                            sublvtype = ScanNodeType.belt;              
                        else
                            sublvtype = ScanNodeType.beltcluster;                           // level 2, its called a cluster
                    }
                    else if (isring && lvl == elements.Count - 1)                           // if its a ring, and we are at the end of the list, mark as a ring
                    {
                        sublvtype = ScanNodeType.ring;
                    }
                    else
                        sublvtype = ScanNodeType.body;                                      // default is body for levels 1 on
                }

                // if not got a node list (only happens when we have a scannode from another scannode), or we are not in the node list

                bool madenew = false;

                if (currentnodelist == null || !currentnodelist.TryGetValue(elements[lvl], out ScanNode subnode)) // either no nodes, or not found the element name in the node list.
                {
                    if (currentnodelist == null)                            // no node list, happens when we are at least 1 level down as systemnode always has a node list, make one 
                        currentnodelist = previousnode.MakeChildList();

                    string ownname = elements[lvl];

                    subnode = new ScanNode(ownname,
                                            previousnode == null ? (sys.Name + (ownname.Contains("Main") ? "" : (" " + ownname))) : previousnode.BodyDesignator + " " + ownname,
                                            sublvtype,
                                            lvl,
                                            previousnode,
                                            systemnode,
                                            sc.DataSource);


                    currentnodelist.Add(ownname, subnode);
                    madenew = true;
                      //  System.Diagnostics.Debug.WriteLine($"StarScan JS Created subnode {subnode.FullName}");
                }
                else
                {
                   // System.Diagnostics.Debug.WriteLine($"StarScan JS Existing subnode {subnode.FullName} {subnode.EDSMCreatedNode}");
                }

                if (ancestorbodies != null && lvl < ancestorbodies.Count)       // if we have the ancestor list, we can fill in the bodyid for each part.
                {
                    subnode.BodyID = ancestorbodies[lvl].BodyID;
                    systemnode.NodesByID[(int)subnode.BodyID] = subnode;
                }

                if (lvl == elements.Count - 1)                                  // if we are at the end node..
                {
                    // if we are replacing the node, make sure we dup across some info which was injected into it by other events

                    if (oldnode != null && oldnode.BodyDesignator == subnode.BodyDesignator && !object.ReferenceEquals(subnode, oldnode))      
                    {
                        subnode.CopyExternalDataFromOldNode(oldnode);
                    }
    
                    // if its a belt cluster we are adding, then the previous node is a belt but it does not have a scan data, therefore it was artifically created and has no body id
                    // if we have a parent list, we can push the body id of it up. makes the IDs look better

                    if (subnode.NodeType == ScanNodeType.beltcluster && sc.Parents != null)
                        previousnode.BodyID = sc.Parents[0].BodyID;

                    // an older node, with a new scan which is not websourced, but the current one is websourced,
                    // we set the data source to journal, as we have in effect created it again

                    if (!madenew && sc.IsWebSourced == false && subnode.WebCreatedNode == true)
                    {
                        subnode.OverrideDataSourceToJournal();
                    }

                    // only overwrites if scan is better
                    subnode.SetScanDataIfBetter(sc);

                    subnode.ScanData.SetMapped(subnode.IsMapped, subnode.WasMappedEfficiently);      // pass this data to node, as we may have previously had a SAA Scan

                    subnode.BodyName = bodyname;                                // and its body name if its known

                    //System.Diagnostics.Debug.WriteLine($"StarScan Journal Created Subnode `{subnode.BodyDesignator}` ownname:`{subnode.OwnName}` bodyname:`{bodyname}`");

                    if (sc.BodyID != null)                                      // if scan has a body ID, pass it to the node
                    {
                        subnode.BodyID = sc.BodyID;
                    }

                    if (sc.Parents != null)
                    {
                        for (int i = 0; i < sc.Parents.Count - 1; i++)   // look thru the list, and assign at the correct level
                        {
                            if (systemnode.Barycentres.TryGetValue(sc.Parents[i].BodyID, out JournalScanBaryCentre jsa))
                            {
                                sc.Parents[i].Barycentre = jsa;
                            }
                        }
                    }

                    if (subnode.Signals != null)            // make sure Scan node has same list as subnode
                    {
                        // System.Diagnostics.Debug.WriteLine($"Assign JS signal list {string.Join(",", subnode.Signals.Select(x => x.Type).ToList())} to {subnode.FullName}");
                        sc.Signals = subnode.Signals;       
                    }
                    if (subnode.Genuses != null)            // make sure Scan node has same list as subnode
                    {
                        sc.Genuses = subnode.Genuses;       
                    }
                    if (subnode.Organics != null)           // make sure Scan node has same list as subnode
                    {
                        // System.Diagnostics.Debug.WriteLine($"Assign JS organic list {string.Join(",", subnode.Organics.Select(x => x.Species).ToList())} to {subnode.FullName}");
                        sc.Organics = subnode.Organics;       
                    }
                    if (subnode.SurfaceFeatures != null)    // mask sure scan node has same list as subnode
                    {
                        sc.SurfaceFeatures = subnode.SurfaceFeatures;
                    }
                }

                previousnode = subnode;                                         // move forward 1 step
                currentnodelist = previousnode.Children;
            }

            return previousnode;
        }

        // asteroid belts, not rings, are assigned to sub nodes of the star in the node heirarchy as type==belt.

        private void ProcessBelts(JournalScan sc, SystemNode sn, ScanNode node)
        {
            if (sc.HasRingsOrBelts)
            {
                foreach (JournalScan.StarPlanetRing ring in sc.Rings)
                {
                    string beltname = ring.Name;
                    string stardesig = sc.BodyDesignation ?? sc.BodyName;

                    if (beltname.StartsWith(stardesig, StringComparison.InvariantCultureIgnoreCase))
                    {
                        beltname = beltname.Substring(stardesig.Length).Trim();
                    }
                    else if (stardesig.ToLowerInvariant() == "lave" && beltname.ToLowerInvariant() == "castellan belt")
                    {
                        beltname = "A Belt";
                    }

                    if (node.Children == null || !node.Children.TryGetValue(beltname, out ScanNode belt))
                    {
                        if (node.Children == null)
                            node.MakeChildList();

                        belt = new ScanNode(beltname, node.BodyDesignator + " " + beltname, ScanNodeType.belt, 1, node, sn, SystemSource.FromJournal);
                        belt.BodyName = ring.Name;
                        belt.BeltData = ring;
                        node.Children.Add(beltname, belt);
                    }

                    belt.BeltData = ring;
                }
            }
        }

        // find a better name for the body
        private string GetBodyNameJournalScan(JournalScan sc, ISystem sys)
        {
            string rest = sc.RemoveStarnameFromDesignation(sys.Name, sys.SystemAddress);      // this can be null
            string name = null;

            if (sc.BodyName.StartsWith(sys.Name, StringComparison.InvariantCultureIgnoreCase))  // if body starts with system name
            {
                name = sc.BodyName.Substring(sys.Name.Length).TrimStart(' ', '-');    // cut out system name

                if (name == "" && !sc.IsStar)                                         // if empty, and not star, customname is just the body name
                {
                    name = sc.BodyName;
                }
                else if (name == "" || name == rest)                            // if empty, or its the same as the star name related, its not got a customname
                {
                    name = null;
                }
            }
            else if (rest == null || !sc.BodyName.EndsWith(rest))                           // not related to star, or not related to bodyname, set back to body name
            {
                name = sc.BodyName;
            }

            return name;
        }
    }
}
