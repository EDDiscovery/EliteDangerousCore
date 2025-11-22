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
using System.IO;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemNode
    {

        #region Event Handlers

        // Get or make a standard named body node Scheau Prao ME-M c22-21 A 1 a etc with a parents list
        // given a own name like "A 1 a" or "1 a" or "1" make nodes down the tree if they don't exist, return final node.
        // we never use this to make a top level star
        public BodyNode GetOrMakeStandardBodyNodeFromScanWithParents(JournalScan sc, string subname, string systemname)
        {
            global::System.Diagnostics.Debug.Assert(sc.BodyID == 0 || sc.Parents != null);

            // we align the parents field and the subname parts together

            AlignParentsName(sc.Parents, subname, out List<string> partnames);

            $"Scan {sc.EventTimeUTC} `{sc.BodyName}`:{sc.BodyID} Name Normalised: {string.Join(", ", partnames)}".DO(debugid);

            CheckTree();

            int pno = sc.Parents.Count - 1;     // parents go backwards
            int partno = 0;                     // parts go forward
            BodyNode cur = systemBodies;

            // we go backwards thru the parents field, forwards thru the partname fields so they align, and pick them off

            while (pno >= 0)
            {
                var nt = sc.Parents[pno];
                string subbodyname = partnames[partno];

                // checking we have not put it in the wrong place before (due to discrete adds)

                BodyNode subbody = FindBody(nt.BodyID);           // find ID anywhere.. 

                // the bodyid should have found it. But for belts, they may have been added with an AutoID from the star ring info, but still be there with this name
                // we need to only check belts because other items such as Unknown Barycentres all have the same names and may be picked up in error
                if (subbody == null && nt.IsStellarRing)
                    subbody = cur.ChildBodies.Find(x => x.OwnName == subbodyname);

                // not there..

                if (subbody == null)
                {
                    subbody = new BodyNode(subbodyname, nt, nt.BodyID, cur, this);
                    cur.ChildBodies.Add(subbody);
                    bodybyid[nt.BodyID] = subbody;
                    Sort(cur);      // added, needs sorting
                    $"  Add {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }
                else
                {
                    if (subbody.Parent != cur)                  // if there but not in correct place, move to correct place
                    {
                        ReassignParent(subbody, cur, nt.BodyID);
                        Sort(cur);      // added, needs sorting
                        $"  Move {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }

                    // If we have a difference in name, the new name is not an unknown name, and the subbody is not a BC name
                    if (subbody.OwnName != subbodyname && !subbodyname.StartsWith(BodyNode.UnknownMarker) && !subbody.OwnName.StartsWith(BodyNode.BCNamingPrefix)) 
                    {
                        subbody.ResetBodyName(subbodyname);
                        Sort(cur);      // changed name, sort
                        $"  Rename {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }

                    if ( subbody.BodyID!=nt.BodyID)                         // for a belt, added by the star, the bodyID would be set to -N, so here we correct it back. 
                    {
                        (subbody.BodyID < 0).Assert();                      // double check we are not being double troubled by picking up something else which has an dupl name (such as Unknown Barycentre)
                        subbody.ResetBodyID(nt.BodyID);
                        bodybyid[nt.BodyID] = subbody;                      // changing ID does not change sort
                        $"  Rebodyid {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }
                }

                CheckTree();
                cur = subbody;
                pno--;
                partno++;
            }

            // final part of the scan

            string ownname = partnames.Last();

            // check to see if in wrong place

            BodyNode body = FindBody(sc.BodyID.Value);        // find ID anywhere

            if (body == null)
                body = FindCanonicalBodyNameType(sc.BodyName,sc.BodyType);  // see if its anywhere else due to a misplace using the scan name, checking all scan names and ownnames in case its an autoplaced object

            if ( body == null )
            {
                body = new BodyNode(ownname, sc, sc.BodyID.Value, cur, this);
                cur.ChildBodies.Add(body);
                bodybyid[sc.BodyID.Value] = body;
                $"  Add {body.BodyType} `{body.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
            }
            else
            {
                if (body.Parent != cur)                 // if there but not in correct place, move to correct place
                {
                    ReassignParent(body, cur, sc.BodyID.Value);
                    $"  Move {body.BodyType} `{body.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }

                if (ownname != body.OwnName)
                {
                    body.ResetBodyName(ownname);                        // make sure we are calling it by this part - this is the real name always
                    $"  Rename {body.BodyType} `{body.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }
            }

            BodyGeneration++;

            if (cur.BodyType == BodyDefinitions.BodyType.Barycentre)     // we can adjust the name of the BC above if possible
            {
                AddBarycentreName(cur, ownname);
            }

            body.SetScan(sc);

            Sort(cur);          // resort parent

            ProcessBeltsOrRings(body, sc, sc.BodyName,systemname);     // finally any belts/cluster or planetary rings need adding

            CheckTree();

            return body;
        }


        // Non standard name with Parents
        public BodyNode GetOrMakeNonStandardBodyFromScanWithParents(JournalScan sc, string ownname, string systemname)
        {
            global::System.Diagnostics.Debug.Assert(sc.BodyID == 0 || sc.Parents != null);

            $"Scan {sc.EventTimeUTC} `{sc.BodyName}`:{sc.BodyID} Non Standard with Parents {sc.ParentList()} ".DO(debugid);

            CheckTree();

            BodyNode cur = systemBodies;
            int pno = (sc.Parents?.Count ?? 0) - 1;

            while(pno >= 0)
            {
                var nt = sc.Parents[pno];

                BodyNode subbody = FindBody(nt.BodyID);

                // Note: A star adds a belt clusters with ID<0 as its not given.  When a belt cluster body is defined however, it will be handled by the standard naming code (<star> A Belt Cluster 3)
                // since the format is in std naming and the belt cluster ID will be corrected.  We should not be here going thru a ring definition sequence and encoutering a child with an autoid

                // a stellar ring (belt cluster) should not have any bodies below with bodyidmarkers set..
                if (nt.IsStellarRing && cur.ChildBodies.Find(x => x.BodyID == BodyNode.BodyIDMarkerForAutoBodyBeltCluster) != null)
                {
                    false.Assert($"StarScan belt cluster ID error for {sc.BodyName} in {systemname}");
                }

                if (subbody == null)
                {
                    subbody = new BodyNode(nt.IsBarycentre ? BodyNode.DefaultNameOfBC : BodyNode.DefaultNameOfUnknownBody, nt, nt.BodyID, cur, this);
                    cur.ChildBodies.Add(subbody);
                    bodybyid[nt.BodyID] = subbody;
                    Sort(cur);
                    $"  Add {subbody.BodyType} `{subbody.OwnName}`:{subbody.BodyID} below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }
                else
                {
                    if (subbody.Parent != cur)
                    {
                        ReassignParent(subbody, cur, nt.BodyID);
                        Sort(cur);
                        $"  Move {subbody.BodyType} `{subbody.OwnName}`:{subbody.BodyID} below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }
                }

                CheckTree();

                cur = subbody;
                pno--;
            }

            CheckTree();

            BodyNode body = FindBody(sc.BodyID.Value);        // find ID anywhere
            
            if (body == null)                                       // no, check if its misplaced anywhere by the scan name
                body = FindCanonicalBodyNameType(sc.BodyName,sc.BodyType);

            if (body == null)
            {
                body = new BodyNode(sc.BodyName, sc, sc.BodyID.Value, cur, this);
                cur.ChildBodies.Add(body);
                bodybyid[sc.BodyID.Value] = body;
                $"  Add {body.BodyType} `{body.OwnName}`:{body.BodyID} below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
            }
            else
            {
                if (body.Parent != cur)                 // if there but not in correct place, move to correct place
                {
                    ReassignParent(body, cur, sc.BodyID.Value);
                    $"  Move {body.BodyType} `{body.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }

                if (body.OwnName != sc.BodyName)
                {
                    body.ResetBodyName(sc.BodyName);        // force the name we know on it
                    $"  Rename {body.BodyType} `{body.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }
            }

            BodyGeneration++;

            if (cur.BodyType == BodyDefinitions.BodyType.Barycentre)     // we can adjust the name of the BC above if possible
            {
                AddBarycentreName(cur, sc.BodyName);
            }

            body.SetScan(sc); // update or add scan BEFORE sorting - we may have added it before without a scan
            
            Sort(cur);          // then sort with into

            CheckTree();

            ProcessBeltsOrRings(body, sc, sc.BodyName, systemname);

            CheckTree();

            return body;
        }

        // Get or make a standard named body node Scheau Prao ME-M c22-21 A 1 a etc without a parent list
        // given a own name like "A 1 a" or "1 a" or "1" make nodes down the tree if they don't exist, return final node.
        public BodyNode GetOrMakeStandardBodyNodeFromScanWithoutParents(JournalScan sc, string subname, string systemname)
        {
            // extract all named parts

            var partnames = ExtractParts(subname);

            $"Scan {sc.EventTimeUTC} `{sc.BodyName}`:{sc.BodyID} No Parents Name Normalised: {string.Join(", ", partnames)}".DO(debugid);

            CheckTree();

            BodyNode cur = systemBodies;

            bool haveastar = false;     // we keep track if we passed or know we have a star

            for (int partno = 0; true; partno++)         // all nodes incl end node
            {
                string ownname = partnames[partno];
                
                // classify as best as possible
                bool itsabarycentre = partno == 0 && ownname.HasAll(x => char.IsUpper(x));
                bool itsastar = partno == 0 && ownname.Length == 1 && char.IsUpper(partnames[partno][0]);
                bool itsabeltcluster = ownname.ContainsIIC("Belt Cluster");

                if (itsabarycentre)         // if we have a barycentre AB, then a star will be implied under it.
                {
                    haveastar = true;
                }
                else if (!itsastar && !haveastar)         // not a star and we don't have a star yet, we need to start with a star
                {
                    // find any star at top level
                    var starbody = cur.ChildBodies.Find(x => x.BodyType == BodyDefinitions.BodyType.Star);

                    if ( starbody == null)       // if can't find a star
                    {
                        starbody = new BodyNode(BodyNode.DefaultNameOfUnknownStar, BodyDefinitions.BodyType.Star, BodyNode.BodyIDMarkerForAutoBody, cur, this);
                        cur.ChildBodies.Add(starbody);
                        $"  Add {starbody.BodyType} `{starbody.OwnName}`:{starbody.BodyID} below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }
                    
                    cur = starbody;

                    haveastar = true;
                }

                bool last = partno == partnames.Count - 1;

                BodyNode subbody = last ? FindCanonicalBodyNameType(sc.BodyName,sc.BodyType) : null;     // see if its anywhere else due to a misplace for the last entry only

                // see if the ownname is under our node
                if ( subbody == null)   
                    subbody = cur.ChildBodies.Find(x => x.OwnName.EqualsIIC(ownname));

                // if not, make it.
                if (subbody == null)
                {
                    // best try classifcation, by sc is last, else by other info from above
                    BodyDefinitions.BodyType ty = last && sc.IsStar ? BodyDefinitions.BodyType.Star : 
                                            last && sc.IsPlanet ? BodyDefinitions.BodyType.Planet :
                                            last && sc.IsBeltClusterBody ? BodyDefinitions.BodyType.AsteroidCluster :
                                            last && sc.IsPlanetaryRing ? BodyDefinitions.BodyType.PlanetaryRing:
                                            itsabeltcluster ? BodyDefinitions.BodyType.StellarRing : 
                                            itsabarycentre ? BodyDefinitions.BodyType.Barycentre : 
                                            itsastar ? BodyDefinitions.BodyType.Star : 
                                            BodyDefinitions.BodyType.Unknown;

                    subbody = new BodyNode(ownname, ty, BodyNode.BodyIDMarkerForAutoBody, cur, this);
                    cur.ChildBodies.Add(subbody);

                    if (!last)      // sort, if not last. If last it will be sorted below
                        Sort(cur);

                    $"  Add {subbody.BodyType} `{subbody.OwnName}`:{subbody.BodyID} below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                }
                else
                {
                    if ( subbody.Parent != cur)
                    {
                        ReassignParent(subbody, cur, BodyNode.BodyIDMarkerForAutoBody);
                        $"  Move {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }

                    // we always have real names for each part

                    if (subbody.OwnName != ownname && !subbody.OwnName.StartsWith(BodyNode.BCNamingPrefix))                     
                    {
                        subbody.ResetBodyName(ownname);     // just in case we made an incorrect one before
                        Sort(cur);
                        $"  Rename {subbody.BodyType} `{subbody.OwnName}` below `{cur.OwnName}`:{cur.BodyID} in {systemname}".DO(debugid);
                    }
                }

                CheckTree();

                // if last node, we need to do work and stop
                if (last)
                {
                    // force class. We may have called it unknown before because it was in the middle of the naming list.
                    BodyDefinitions.BodyType ty = sc.IsStar ? BodyDefinitions.BodyType.Star : sc.IsPlanet ? BodyDefinitions.BodyType.Planet : BodyDefinitions.BodyType.AsteroidCluster;
                    subbody.ResetClass(ty);

                    if (cur.BodyType == BodyDefinitions.BodyType.Barycentre)     // we can adjust the name of the BC above if possible
                    {
                        AddBarycentreName(cur, ownname);
                    }

                    BodyGeneration++;

                    subbody.SetScan(sc);            // set scan and canonical name

                    Sort(cur);          // resort parent

                    ProcessBeltsOrRings(subbody, sc, sc.BodyName, systemname);     // finally any belts/cluster or planetary rings need adding

                    return subbody;

                }

                cur = subbody;
            }
        }

        // for IBodyNames without parent lists, marked as planet. 
        // check if its there by id or fdname, and then if not, make something up to hold it
        public BodyNode GetOrMakeDiscretePlanet(string fdname, int? bid, string systemname)
        {
            CheckTree();

            BodyNode body = bid.HasValue ? FindBody(bid.Value) : null;

            if (body == null)
                body = FindCanonicalBodyNameType(fdname, BodyDefinitions.BodyType.Planet);

            if (body == null)
            {
                BodyNode starbody = Bodies(x => x.BodyType == BodyDefinitions.BodyType.Star, true).FirstOrDefault();       // find a star, any star, anywhere

                if (starbody == null)
                {
                    starbody = new BodyNode(BodyNode.DefaultNameOfUnknownStar, BodyDefinitions.BodyType.Star, BodyNode.BodyIDMarkerForAutoBody, systemBodies, this);       // assign under the main node
                    systemBodies.ChildBodies.Add(starbody);
                    $"  Add Unknown Star for {fdname}:{bid} in {systemname}".DO(debugid);
                }

                body = new BodyNode(fdname, BodyDefinitions.BodyType.Planet, bid ?? -1, starbody, this);
                body.SetCanonicalName(fdname);          // we set its canonical name to the fdname given
                BodyGeneration++;

                starbody.ChildBodies.Add(body);
                if (bid >= 0)
                    bodybyid[bid.Value] = body;

                Sort(starbody);

                CheckTree();
                return body;
            }
            else
            {
                return body;
            }
        }

        //for IBodyNames without parent lists, marked as star
        //try and find it by ID or fdname, if not, make something
        public BodyNode GetOrMakeDiscreteStar(string fdname, int? bid, string systemname)
        {
            CheckTree();

            string cutname = fdname.ReplaceIfStartsWith(systemname);

            BodyNode starbody = bid.HasValue ? FindBody(bid.Value) : null;

            if (starbody == null)
                starbody = FindCanonicalBodyNameType(fdname,BodyDefinitions.BodyType.Star);

            if (starbody == null)
                starbody = systemBodies.ChildBodies.Find(x => x.BodyType == BodyDefinitions.BodyType.Star && x.BodyID == BodyNode.BodyIDMarkerForAutoBody);          // try an autostar

            if (starbody == null)
            {
                starbody = new BodyNode(fdname, BodyDefinitions.BodyType.Star, bid ?? BodyNode.BodyIDMarkerForAutoBody, systemBodies, this);       // we don't know its placement, so we just place it under the systemnode, and we mark it as unknown (even if we know its BID because we want to mark it as autoplaced)
                starbody.SetCanonicalName(fdname);          // we set its canonical name to the fdname given
                $"  Add Star `{fdname}`:-2 in {systemname}".DO(debugid);
                BodyGeneration++;
                systemBodies.ChildBodies.Add(starbody);
            }
            else
            {
                starbody.ResetBodyName(fdname);
                starbody.SetCanonicalName(fdname);          // we set its canonical name to the fdname given
                if (bid >= 0)
                    starbody.ResetBodyID(bid.Value);
            }

            if (bid >= 0)
                bodybyid[bid.Value] = starbody;

            Sort(systemBodies);
            CheckTree();

            return starbody;
        }



        // assign, we def need a previous entry to be able to assign.  Null if not found
        public BodyNode AddBaryCentreScan(JournalScanBaryCentre sc)
        {
            BodyNode prevassigned = FindBody(sc.BodyID);
            if (prevassigned != null)
            {
                prevassigned.SetScan(sc);
                Sort(prevassigned.Parent);      // and resort
                $"  Add Baryscan to BodyID {sc.BodyID} in {System.Name}".DO(debugid);
                BodyGeneration++;
                BarycentreScans++;

                // all entries where JSA BodyID occurs in parents list, lets add the barycentre info to it for use by queries
                var scannodelist = Bodies(x => x.Scan?.Parents != null && x.Scan.Parents.FindIndex(y => y.BodyID == sc.BodyID) >= 0).ToList();

                if ( scannodelist.Count == 0)
                    $"   .. No scans found with this barycentre in bodies list in {System.Name}".DO(debugid);

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

                return prevassigned;
            }
            else
                return null;
        }


        // Add a codex entry to best place, if body id, find it, else system bodies
        public BodyNode AddCodexEntryToSystem(JournalCodexEntry sc)
        {
            if (sc.BodyID.HasValue)
            {
                BodyNode body = FindBody(sc.BodyID.Value);

                if (body != null)
                {
                    body.AddCodex(sc);
                    SignalGeneration++;
                    return body;
                }
                else
                    return null;
            }
            else
            {
                systemBodies.AddCodex(sc);          // SYstem bodies hold global info
                return systemBodies;
            }
        }

        // We have a body id, lets try and see if we can assign it to a bodyid less body!
        public BodyNode AddBodyIDToBody(IBodyFeature sc)
        {
            if (sc.BodyID.HasValue)
            {
                BodyNode body = FindBody(sc.BodyID.Value);

                if (body == null)       // if not found, we may be able to do add the body id to it 
                {
                    body = FindCanonicalBodyNameType(sc.BodyName,sc.BodyType);  // see if we can find it

                    if (body != null && body.BodyID<0)      // found it, and not set, set it
                    {
                        (body.BodyID < 0).Assert("StarScan Body is not null");
                        $"  Assign body ID {sc.BodyID} {sc.BodyName} in {System.Name}".DO(debugid);
                        body.ResetBodyID(sc.BodyID.Value);
                        bodybyid[sc.BodyID.Value] = body;
                        BodyGeneration++;
                        return body;
                    }
                }
            }

            return null;
        }

        // Always has a BodyID
        public BodyNode AddFSSBodySignalsToBody(JournalFSSBodySignals sc)
        {
            BodyNode body = FindBody(sc.BodyID);
            if (body != null)
            {
                body.AddSignals(sc.Signals);
                SignalGeneration++;
                return body;
            }
            else
                return null;
        }

        // SAASignalsFound are on bodies or planetary rings
        // will return null if we can't find it
        // Note that we could have had a ring added by a body with the ring scan info, which does not have bodyid in the field, so body id is reset if found using name
        public BodyNode AddSAASignalsFound(JournalSAASignalsFound sc)
        {
            BodyNode body = FindBody(sc.BodyID);

            if (body == null)
            {
                body = FindCanonicalBodyNameType(sc.BodyName, sc.BodyType);
                if (body != null)
                {
                    (body.BodyID < 0).Assert("Reset SAA Signals Found without ID but ID is set");
                    body.ResetBodyID(sc.BodyID);
                    bodybyid[sc.BodyID] = body;
                }
            }

            if (body != null)
            {
                body.AddSignals(sc.Signals);
                SignalGeneration++;
                if (sc.Genuses?.Count > 0)
                    body.AddGenuses(sc.Genuses);
                return body;
            }
            else
                return null;
        }


        // ScanComplete are on bodies or planetary rings
        // will return null if we can't find it
        // Note that we could have had a ring added by a body with the ring scan info, which does not have bodyid in the field, so body id is reset if found using name
        public BodyNode AddSAAScanComplete(JournalSAAScanComplete sc)
        {
            BodyNode body = FindBody(sc.BodyID);

            if (body == null)
            {
                //body = FindCanonicalBodyNameType(sc.BodyName, sc.BodyType);
                body = FindCanonicalBodyNameType(sc.BodyName,sc.BodyType);

                if (body != null)
                {
                    (body.BodyID < 0).Assert("Reset SAA Rings Found without ID but ID is set");
                    body.ResetBodyID(sc.BodyID);
                    bodybyid[sc.BodyID] = body;
                }
            }

            if (body != null)
            {
                body.SetMapped(sc.ProbesUsed <= sc.EfficiencyTarget);       // record in body the mapped state

                if (body.Scan != null)      // if the scan is there, we can set the value.  If another scan comes in the scan comes in the mapped flags will be copied from body. See SetScan
                {
                    body.Scan.SetMapped(body.IsMapped, body.WasMappedEfficiently);
                }

                SignalGeneration++;
                return body;
            }

            return null;
        }

//        tbd system bodies should show up on scan display

        // Location or Supercruise Exi, Station is only given to orbiting stations, and we don't know where they are, so system bodies
        public BodyNode AddStation( IBodyFeature loc)
        {
            (loc.BodyType == BodyDefinitions.BodyType.Station).Assert("Error called add station with another type");

            BodyNode bn = FindBody(loc.BodyID.Value);
            if (bn == null)
            {
                if (systemBodies.AddFeatureOnlyIfNew(loc))
                {
                   // $"Station is new added to system bodies `{loc.BodyName}` {loc.BodyType} {loc.BodyID}".DO();
                }
                else
                {
                   // $"Station already there in system bodies `{loc.BodyName}` {loc.BodyType} {loc.BodyID}".DO();
                }
                return systemBodies;        // Do nothing, return this to indicate its processed.
            }
            else
            {
               // $"Station already there `{loc.BodyName}` {loc.BodyType} {loc.BodyID} at {bn.Name()}".DO();
                return bn;
            }
        }

        public void SetFSSDiscoveryScan(int? bodycount, int? nonbodycount)
        {
            $"Add FSS Discovery Scan Count {bodycount} {nonbodycount} to System in `{System.Name}`:{System.SystemAddress}".DO(debugid);
            FSSTotalBodies = bodycount;
            FSSTotalNonBodies = nonbodycount;
            BodyGeneration++;
        }

        public void AddFSSSignalsDiscovered(List<FSSSignal> signals)
        {
            $"Add FSS Signals {signals.Count} to SystemBodies in `{System.Name}`:{System.SystemAddress}".DO(debugid);
            systemBodies.AddFSSSignals(signals);
            SignalGeneration++;
        }

        public BodyNode AddScanOrganicToBody(JournalScanOrganic sc)
        {
            BodyNode body = FindBody(sc.Body);
            if (body != null)
            {
                body.AddScanOrganics(sc);
                SignalGeneration++;
                return body;
            }
            else
                return null;
        }

        // Touchdown, approachsettlement
        public BodyNode AddSurfaceFeatureToBody(IBodyFeature sc)
        {
            BodyNode body = FindBody(sc.BodyID.Value);
            if (body != null)
            {
                body.AddFeatureOnlyIfNew(sc);
                SignalGeneration++;
                return body;
            }
            else
                return null;
        }

        // If its a settlement, we have augmented the docking event with BodyID/BodyName. Else we don't have body id
        public BodyNode AddDockingToBody(JournalDocked sc)
        {
            BodyNode bd = null;
            if (sc.BodyID.HasValue)         // this makes it a settlement
            {
                bd = FindBody(sc.BodyID.Value);
                if (bd == null)
                    return null;            // don't have it now, so return try again
            }
            else
                bd = systemBodies;          // else we don't know where it is, so assign to main list

            //$"Add docking {sc.StationName} to body {bd.Name()}".DO();
            bd.AddDocking(sc);
            SignalGeneration++;
            return bd;
        }

        #endregion

        #region Helpers


        // Extract the part names from the subname, recognised composite text parts like belt clusters
        static public List<string> ExtractParts(string subname)
        {
            var partnames = new List<string>();

            StringParser sp = new StringParser(subname);

            while (!sp.IsEOL)
            {
                // in Scans, its called A Belt
                // other places its called A Belt Cluster
                // split both off, checking belt cluster first in case we preprocessed it

                if (sp.IsStringMoveOn(out string found, StringComparison.InvariantCultureIgnoreCase, true, "A Belt Cluster", "B Belt Cluster", "A Belt", "B Belt", "A Ring", "B Ring" , "C Ring", "D Ring"))
                {
                    partnames.Add(found);
                }
                else
                {
                    partnames.Add(sp.NextWord());
                }
            }

            return partnames;
        }


        // aim is to fill out the partnames with the same number of parents and scan (so 3 parents = 4 entries) and align them
        // this is more complicated to get right that it ever looks.
        static public void AlignParentsName(List<BodyParent> parents, string subname, out List<string> partnames)
        {
            partnames = ExtractParts(subname);

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
                    else if (name.EndsWith("Galle Ring", StringComparison.InvariantCultureIgnoreCase) ||                // specials, just to remove debug assert
                            name.EndsWith("Jupiter Halo Ring", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Asteroid Belt", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("The Belt", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Anahit Ring", StringComparison.InvariantCultureIgnoreCase) ||
                            name.EndsWith("Vulcan Ring", StringComparison.InvariantCultureIgnoreCase)
                            )
                    { }
                    else
                    {
                        string s = $"StarScan {body.Name()} Unconventional ring name {name}";
                        global::System.Diagnostics.Trace.WriteLine(s);
                    }

                    $"  Add Belt/Ring object {name} to `{body.OwnName}`:{body.BodyID}".DO(debugid);

                    var belt = body.ChildBodies.Find(x => x.OwnName == name);
                    if (belt == null)
                    {
                        belt = new BodyNode(name, body.BodyType == BodyDefinitions.BodyType.Planet ? BodyDefinitions.BodyType.PlanetaryRing : BodyDefinitions.BodyType.StellarRing , BodyNode.BodyIDMarkerForAutoBodyBeltCluster, body, this);
                        belt.SetCanonicalName(ring.Name);
                        body.ChildBodies.Add(belt);
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

        private static void Sort(BodyNode cur)
        {
           // $"Sort tree for {cur.OwnName}:{cur.BodyID}".DO();
            cur.ChildBodies.Sort(delegate (BodyNode left, BodyNode right) { return left.CompareTo(right,false); });
        //    cur.DumpTree(2);
        }


        private Dictionary<int, BodyNode> bodybyid;
        private BodyNode systemBodies;

        const string debugid = "StarScan";

        #endregion

        #region Debug

        public void DumpTree()
        {
            global::System.Diagnostics.Trace.WriteLine($"System `{System.Name}` {System.SystemAddress}: bodies {Bodies().Count()} ids {bodybyid.Count}");
            systemBodies.DumpTree("S", 0);
            CheckTree();
        }

        [System.Diagnostics.Conditional("DEBUG")]
      //  [System.Diagnostics.DebuggerHidden]
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
