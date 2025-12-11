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
using EliteDangerousCore.UIEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Called by Events

        // called to make sure we have a system. If system address, we use the address system, else we assign to the old tree
        public SystemNode GetOrAddSystem(ISystem sys)      
        {
            lock(masterlock)
            {
                (sys.Name.HasChars()).Assert("GetOrAddSystem Must have a name");

                if (sys.HasAddress)
                {
                    // System.Diagnostics.Debug.WriteLine($"SS2 Add {je.EventTimeUTC} {je.EventTypeID} {sys.Name} {sys.SystemAddress} {sys.HasCoordinate}");

                    // find system node by address, 

                    if (!systemNodesByAddress.TryGetValue(sys.SystemAddress.Value, out SystemNode node))
                    {
                        // if not found an address, check name
                        if (systemNodesByName.TryGetValue(sys.Name, out node))      // if we have a name
                        {
                            // if the node does not have address, or the addresses match, we match and update the system node address list

                            if ( !node.System.HasAddress || sys.SystemAddress == node.System.SystemAddress)   
                            {
                                systemNodesByAddress.Add(sys.SystemAddress.Value, node);        // add the new address into the node tree also
                            }
                            else
                            {
                                // here we found a name, but the system addresses do not match.. therefore a frontier double naming.

                                $"StarScan Two systems with same name {node.System.NameAddress} with {sys.NameAddress}".DO();

                                systemNodesByName.Remove(node.System.Name);                 // rename existing entry with NameAddress
                                systemNodesByName.Add(node.System.NameAddress, node);        

                                node = new SystemNode(sys);
                                systemNodesByAddress.Add(sys.SystemAddress.Value, node);    // make a new node
                                systemNodesByName[sys.NameAddress] = node;                  // store as name address pair
                            }
                        }
                        else if (!systemNodesByName.TryGetValue(sys.NameAddress, out node))      // if we don't have a nameAddress in there (due to a double name)
                        { 
                            // no address found, and no name, no name:address, new node
                            node = new SystemNode(sys);
                            systemNodesByAddress.Add(sys.SystemAddress.Value, node);        // make a new node
                            systemNodesByName[sys.Name] = node;                             // point name node to our new node
                        }
                    }
                    else
                    {
                        // we have a node with the address, primary key.
                        if ( sys.Name != node.System.Name)      // a system has been renamed by frontier..
                        {
                            $"StarScan Renamed system {node.System.Name} -> {sys.Name}".DO();
                            node.RenamedSystem(sys);
                            systemNodesByName[sys.Name] = node;     // we add it by name to this node, but leave the previous name also pointing to this node...
                        }
                    }
                    return node;
                }
                else if (sys.Name.HasChars())      
                {
                    // find by normal name
                    if (!systemNodesByName.TryGetValue(sys.Name, out SystemNode node))    // not a name
                    {
                        // may be in there as Name:Address 
                        var keyname = systemNodesByName.Keys.ToList().Find(x => x.StartsWith(sys.Name + ":"));      // look thru the keyname and try and find one starting with sys.Name

                        if (keyname == null)    // nope, new.1
                        {
                            node = new SystemNode(sys);
                            systemNodesByName.Add(sys.Name, node);
                        }
                        else
                            node = systemNodesByName[keyname];
                    }
                    return node;
                }
            }

            return null;
        }

        // called when we are at a location due to Approach/Leave body, Supercruise Exit, Location
        // and is called by callers below when they know they have a body
        public void AddLocation(IBodyFeature sc, ISystem sys)
        {
            // we need this info to proceed.

            if (sc.SystemAddress != null && sys.SystemAddress == sc.SystemAddress && sc.BodyID != null && sc.BodyName != null && true)
            {
                bool stdname = sc.BodyName.StartsWith(sys.Name) && sc.BodyName.Length > sys.Name.Length;
                string ownname = stdname ? sc.BodyName.Substring(sys.Name.Length).Trim() : sc.BodyName;

                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                //global::System.Diagnostics.Debug.WriteLine($"Add Body {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` -> `{ownname}` in `{sys.Name}` {sys.SystemAddress}");

                if (sc.BodyType == BodyDefinitions.BodyType.Station )            // Supercruise Exit/Location, orbiting station only
                {
                    (sc.EventTypeID == JournalTypeEnum.SupercruiseExit || sc.EventTypeID == JournalTypeEnum.Location).Assert("StarScan Unexpected Station type event");

                    //  $"Add Station {sc.EventTypeStr} {sc.EventTimeUTC} : {sc.BodyType} `{sc.BodyName}` {sc.BodyID} ".DO();
                    lock (sn)
                    {
                        sn.AddStation(sc);
                    }
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Planet)      // ApproachBody/Leave Body/SupercruiseExit/Location
                {
                    //$"Add Body {sc.EventTypeStr} {sc.EventTimeUTC} : {sc.BodyType} `{sc.BodyName}` {sc.BodyID} ".DO();
                    lock (sn)
                    {
                        if (stdname)
                            sn.GetOrMakeStandardNamePlanet(sc.BodyName, ownname, sc.BodyID, sys.Name);
                        else
                            sn.GetOrMakeDiscretePlanet(ownname, sc.BodyID, sys.Name);
                    }
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Star)          // SupercruiseExit/Location
                {
                    (sc.EventTypeID == JournalTypeEnum.SupercruiseExit || sc.EventTypeID == JournalTypeEnum.Location).Assert("StarScan Unexpected Star type event");

                    //$"Add Body {sc.EventTypeStr} {sc.EventTimeUTC} : {sc.BodyType} `{sc.BodyName}` {sc.BodyID} ".DO();
                    lock (sn)
                    {
                        sn.GetOrMakeDiscreteStar(sc.BodyName, sc.BodyID, sys.Name);
                    }
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.PlanetaryRing ||   // Supercruise Exit/Location
                         sc.BodyType == BodyDefinitions.BodyType.StellarRing ||      // Supercruise Exit/Location
                         sc.BodyType == BodyDefinitions.BodyType.AsteroidCluster ||  // Supercruise Exit/Location
                         sc.BodyType == BodyDefinitions.BodyType.Barycentre)     // Supercruise Exit/Location
                {
                    lock (sn)
                    {
                        sn.AddBodyIDToBody(sc);     // try and use this body id and assign it to an existing entry
                    }
                }
                else if ( sc.BodyType == BodyDefinitions.BodyType.SmallBody)        // super rare on comets, { "timestamp":"2020-10-03T15:31:57Z", "event":"SupercruiseExit", "StarSystem":"Liu Beserka", "SystemAddress":18265019196857, "Body":"Liu Beserka Comet 2", "BodyID":7, "BodyType":"SmallBody" }
                {
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);     // pick up missed types
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"Add NO SYS ADDR or mismatch {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress} {sys.SystemAddress}");

            }
        }

        // Add a scan
        // Three sorts of scan 1) Modern (sysaddr,bodyid,parents) 2) Older (bodyid,parents) and 3) Ancient (nothing)
        // And two sorts of names standard (Skaude AA-A h294 AB 1 a) or non standard "Earth"

        public void AddJournalScan(JournalScan sc, ISystem curlocsys)
        {
            if (curlocsys.Name == "Sol" && sc.BodyName.StartsWith("Procyon"))         // Robert had this situation
                return;

            SystemNode sn = null;

            // so, we are going to use sc.systemaddress as the primary lookup method, then the sc.starsystem, then only use curlocsys as a fall back for very old scans, and we must have a system already to use curlocsys
            // A log from StarTrash has scans occuring for the previous system in the current system - so we can't rely on curlocsys
            // A log from Wags had scans on hip 63809 occuring in another system..

            lock (masterlock)
            {
                // first try system address from the scan to find the entry.
                if (!sc.SystemAddress.HasValue || !systemNodesByAddress.TryGetValue(sc.SystemAddress.Value, out sn))
                {
                    // failed, lets use the get/add system method as it does more extensive checks on this
                    if (sc.StarSystem.HasChars() || sc.SystemAddress.HasValue)
                    {
                        sn = GetOrAddSystem(new SystemClass(sc.StarSystem, sc.SystemAddress));
                    }
                    else
                    {
                        sn = GetOrAddSystem(curlocsys);     // worse case
                    }
                }
            }

            if (sn != null )
            {
                ISystem scansystem = sn.System;

                bool hasparentbodyid = sc.BodyID != null && (sc.BodyID == 0 || sc.Parents != null);     // no body/parent tree makes the scan problematc to make

                bool stdname = sc.BodyName.StartsWith(scansystem.Name) && sc.BodyName.Length > scansystem.Name.Length;      // work out if its standard naming
                string ownname = stdname ? sc.BodyName.Substring(scansystem.Name.Length).Trim() : sc.BodyName;

                lock (sn)
                {
                    // triage for scans with full info
                    if (hasparentbodyid)                        // if we have modern info 
                    {
                        if (sn.OldScansPresent)                 // we don't mix old scans with new scans. Old scans without parents trees are problematic
                        {
                            sn.OldScansPresent = false;
                            sn.Clear();
                        }

                        if (stdname)
                        {
                            //    $"Add Scan Std format {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}".DO();
                            sn.GetOrMakeStandardBodyNodeFromScanWithParents(sc, ownname, scansystem.Name);
                        }
                        else
                        {
                            //     $"Add Scan NonStd format {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}".DO();
                            sn.GetOrMakeNonStandardBodyFromScanWithParents(sc, scansystem.Name);
                        }
                    }
                    else
                    {
                        sn.OldScansPresent = true;          // record it as an older scan node

                        if (stdname)
                        {
                            //   $"Add OLD Scan Std format no parents {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress}".DO();
                            sn.GetOrMakeStandardBodyNodeFromScanWithoutParents(sc, ownname, scansystem.Name);
                        }
                        else if (sc.IsStar)
                        {
                            BodyNode bn = sn.GetOrMakeDiscreteStar(sc.BodyName, -1, scansystem.Name);
                            bn.SetScan(sc);
                        }
                        else
                        {
                            //  $"Add OLD Scan NonStd format no parents {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress}".DO();
                            // we have no placement info, just store it.

                            if (sc.BodyType == BodyDefinitions.BodyType.Planet)     // only adding planets here
                            {
                                BodyNode bn = stdname ? sn.GetOrMakeStandardNamePlanet(sc.BodyName, ownname, sc.BodyID, scansystem.Name) : sn.GetOrMakeDiscretePlanet(ownname, sc.BodyID, scansystem.Name);
                                bn.SetScan(sc);
                            }
                        }
                    }
                }
            }
            else
            {
                $"StarScan Rejected Scan {sc.EventTimeUTC} {sc.BodyName} in `{sc.StarSystem}`:{sc.SystemAddress} found in `{curlocsys}` as missing evidence it was made in this system".DO();
            }

        }

        public void AddBarycentre(JournalScanBaryCentre sc, ISystem sys, bool saveit = true)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddBaryCentreScan(sc) == null)        // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        public void AddCodexEntryToSystem(JournalCodexEntry sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddCodexEntryToSystem(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }


        public void SetFSSDiscoveryScan(int? bodycount, int? nonbodycount, ISystem sys)
        {
            if (sys.SystemAddress != null )     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    sn.SetFSSDiscoveryScan(bodycount, nonbodycount);
                }
            }
        }

        // these might not be in the right system, if not, we need to pend it until it is.
        public bool AddFSSSignalsDiscovered(JournalFSSSignalDiscovered sc, bool pendit = true)
        {
            if (sc.Signals[0].SystemAddress.HasValue )
            {
                if (TryGetSystemNode(sc.Signals[0].SystemAddress.Value, out SystemNode sn))     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
                {
                    lock (sn)
                    {
                        sn.AddFSSSignalsDiscovered(sc.Signals);
                        return true;
                    }
                }
                else
                    AddPending(sc.Signals[0].SystemAddress.Value, sc);
            }
            return false;
        }

        public void AddScanOrganic(JournalScanOrganic sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddScanOrganicToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        // we get this for a planet
        public void AddTouchdown(JournalTouchdown sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                AddLocation(sc, sys);            // we can add a body 

                lock (sn)
                {
                    if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        // we get this for a planet
        public void AddApproachSettlement(JournalApproachSettlement sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                AddLocation(sc, sys);            // we can add a body 

                lock (sn)
                {
                    if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        // from planets only
        public void AddFSSBodySignalsToBody(JournalFSSBodySignals sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                AddLocation(sc, sys);       // its always a planet, so we can add it

                lock (sn)
                {
                    if (sn.AddFSSBodySignalsToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        // we can get this for a body HIP 17403 A 4 a, or a ring Borann A 2 B Ring
        // SAASignalsFound always had bodyid and system
        public void AddSAASignalsFound(JournalSAASignalsFound sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have filled in basic info
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                if (!BodyDefinitions.IsBodyNameRing(sc.BodyName))              // we can't add Rings, since we don't know the parent body ID and we don't have a cracker if its a std name
                {
                    AddLocation(sc, sys);            // we can add a body
                }

                lock (sn)
                {
                    if (sn.AddSAASignalsFound(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        // we can get this for a body HIP 17403 A 4 a, or a ring Borann A 2 B Ring
        public void AddSAAScanComplete(JournalSAAScanComplete sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress )     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                if (!BodyDefinitions.IsBodyNameRing(sc.BodyName))      // we can't add Rings, since we don't know the parent body ID and we don't have a cracker if its a std name
                {
                    AddLocation(sc, sys);            // we can add a body
                }

                lock (sn)
                {
                    if (sn.AddSAAScanComplete(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }
        
        // Add Docking, for a settlement we have augmented the information with BodyID/Body
        public void AddDocking(JournalDocked sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress )     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddDockingToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        public void AddDestinationSelected(JournalEDDDestinationSelected sc, ISystem sys)
        {
            if (sys.SystemAddress != null )
            {
                //System.Diagnostics.Debug.WriteLine($"StarScan got call to add EDD Destination Selected {sc.TargetName_Localised ?? sc.TargetName}");
            }
        }


        #endregion


        #region Helpers

        // Given a pending, reissue. SN is locked

        private bool AssignPending(SystemNode sn, List<JournalEntry> jelist)
        {
            List<JournalEntry> todelete = new List<JournalEntry>();

            foreach (var je in jelist)
            {
                if (je is JournalScanBaryCentre sb)
                {
                    if (sn.AddBaryCentreScan(sb) != null)
                        todelete.Add(je);
                }
                else if (je is JournalCodexEntry cd)
                {
                    if (sn.AddCodexEntryToSystem(cd) != null)
                        todelete.Add(je);
                }
                else if (je is JournalFSSBodySignals bs)
                {
                    if (sn.AddFSSBodySignalsToBody(bs) != null)
                        todelete.Add(je);
                }
                else if (je is JournalSAASignalsFound saa)
                {
                    if (sn.AddSAASignalsFound(saa) != null)
                        todelete.Add(je);
                }
                else if (je is JournalFSSSignalDiscovered sd)
                {
                    if (AddFSSSignalsDiscovered(sd, false))
                        todelete.Add(je);
                }
                else if (je is JournalScanOrganic so)
                {
                    if (sn.AddScanOrganicToBody(so) != null)
                        todelete.Add(je);
                }
                else if (je is JournalSAAScanComplete saasc)
                {
                    if (sn.AddSAAScanComplete(saasc) != null)
                        todelete.Add(je);
                }
                else 
                {
                    System.Diagnostics.Debug.Assert(false, "Star Scan 2 Not handled event");
                }


            }

            if (todelete.Count == jelist.Count) // if all gone
            {
                return true;
            }
            else
            {
                foreach (var sbe in todelete)       // can't delete all, just this
                    jelist.Remove(sbe);

                return false;
            }
        }


        private void AddPending(long systemaddress, JournalEntry sc)
        {
            lock(masterlock)
            {
                if (!pendingsystemaddressevents.TryGetValue(systemaddress, out List<JournalEntry> jelist))
                {
                    pendingsystemaddressevents[systemaddress] = new List<JournalEntry>();
                }
                pendingsystemaddressevents[systemaddress].Add(sc);
            }

        }

        #endregion


        #region vars
        private Dictionary<long, SystemNode> systemNodesByAddress { get; set; } = new Dictionary<long, SystemNode>();       // by address
        private Dictionary<string, SystemNode> systemNodesByName { get; set; } = new Dictionary<string, SystemNode>(StringComparer.InvariantCultureIgnoreCase);       // by name.

        private Dictionary<long, List<JournalEntry>> pendingsystemaddressevents = new Dictionary<long, List<JournalEntry>>();   // list of pending entries because their bodies are not yet available

        private object masterlock = new object();

        #endregion

    }
}

