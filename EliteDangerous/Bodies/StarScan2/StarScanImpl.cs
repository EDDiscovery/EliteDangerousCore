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
using EliteDangerousCore.UIEvents;
using System.Collections.Generic;

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
                // if we have older system info without systemaddress, we created the nodes in systemnodesbyname. 
                // as soon the system has a system address, we will abandon the old info and recreate using the new one, as we can't rely on the old stuff
                if (sys.SystemAddress.HasValue)
                {
                    // System.Diagnostics.Debug.WriteLine($"SS2 Add {je.EventTimeUTC} {je.EventTypeID} {sys.Name} {sys.SystemAddress} {sys.HasCoordinate}");

                    // find system node by address, 
                    if (!systemNodesByAddress.TryGetValue(sys.SystemAddress.Value, out SystemNode node))
                    {
                        // if not found, check name
                        if (systemNodesByName.TryGetValue(sys.Name, out node))     // if previously by name only
                        {
                            systemNodesByAddress.Add(sys.SystemAddress.Value, node);        // add the new address into the node tree also
                        }
                        else
                        {
                            node = new SystemNode(sys);
                            systemNodesByAddress.Add(sys.SystemAddress.Value, node);        // make a new node
                            systemNodesByName[sys.Name] = node;                             // point name node to our new node
                        }
                    }

                    return node;
                }
                else if (sys.Name.HasChars())      // name only ones
                {
                    if (!systemNodesByName.TryGetValue(sys.Name, out SystemNode node))
                    {
                        node = new SystemNode(sys);
                        systemNodesByName.Add(sys.Name, node);
                    }
                    return node;
                }
            }

            return null;
        }

        // called when we have a body in this system
        public bool AddBody(IBodyNameAndID sc, ISystem sys)
        {
            // we need this info to proceed.

            if (sc.SystemAddress != null && sys.SystemAddress == sc.SystemAddress && sc.BodyID != null && sc.Body != null && true)
            {
                bool stdname = sc.Body.StartsWith(sys.Name) && sc.Body.Length > sys.Name.Length;
                string ownname = stdname ? sc.Body.Substring(sys.Name.Length).Trim() : sc.Body;

                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                //global::System.Diagnostics.Debug.WriteLine($"Add Body {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` -> `{ownname}` in `{sys.Name}` {sys.SystemAddress}");

                if (sc.BodyType == BodyDefinitions.BodyType.Station)
                {
                    //tbd
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Planet)
                {
                    lock (sn)
                    {
                        sn.GetOrMakeDiscreteBody(ownname, sc.BodyID.Value, sys.Name);
                        return true;
                    }
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Star)
                {
                    lock (sn)
                    {
                        sn.GetOrMakeDiscreteStar(sc.Body, sc.BodyID, sys.Name);
                        return true;
                    }
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.PlanetaryRing)
                {
                    //  System.Diagnostics.Debug.WriteLine($"Add Planet Ring {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress}");
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Barycentre)
                {
                    //System.Diagnostics.Debug.WriteLine($"Add Barycentre {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress}");
                }
                else
                {
                    System.Diagnostics.Debug.Assert(true);
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"Add NO SYS ADDR or mismatch {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress} {sys.SystemAddress}");

            }

            return false;
        }

        // called when we have a new UI destination.
        // not sure how to handle this, as we don't know if its a new location or body, and we don't get lat/long etc.
        public void NewUIDestination(UIDestination ui)
        {
            if (ui.SystemAddress.HasValue && ui.BodyID.HasValue && TryGetSystem(ui.SystemAddress.Value, out SystemNode sn))
            {
                System.Diagnostics.Debug.WriteLine($"StarScan We could add {ui.SystemAddress.Value} : {ui.BodyID} {ui.Name}");
            }
        }


        // Add a scan
        // Three sorts of scan 1) Modern (sysaddr,bodyid,parents) 2) Older (bodyid,parents) and 3) Ancient (nothing)
        // And two sorts of names standard (Skaude AA-A h294 AB 1 a) or non standard "Earth"

        public void AddScan(JournalScan sc, ISystem sys)
        {
            if (sys.Name == "Sol" && sc.BodyName.StartsWith("Procyon"))         // Robert had this situation
                return;

            bool hasparentbodyid = sc.BodyID != null && (sc.BodyID == 0 || sc.Parents != null);     // no body/parent tree makes the scan problematc to make

            bool stdname = sc.BodyName.StartsWith(sys.Name) && sc.BodyName.Length > sys.Name.Length;
            string ownname = stdname ? sc.BodyName.Substring(sys.Name.Length).Trim() : sc.BodyName;

            // get system, either by full address or by name.
            SystemNode sn = GetOrAddSystem(sys);
            System.Diagnostics.Debug.Assert(sn != null);

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
                        sn.GetOrMakeStandardBodyNodeFromScanWithParents(sc, ownname, sys.Name);
                    }
                    else
                    {
                   //     $"Add Scan NonStd format {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}".DO();
                        sn.GetOrMakeNonStandardBodyFromScanWithParents(sc, sys.Name, ownname);
                    }
                }
                else
                {
                    sn.OldScansPresent = true;          // record it as an older scan node

                    if (stdname )
                    {
                     //   $"Add OLD Scan Std format no parents {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress}".DO();
                        sn.GetOrMakeStandardBodyNodeFromScanWithoutParents(sc, ownname, sys.Name);
                    }
                    else if ( sc.IsStar)
                    {
                        BodyNode bn = sn.GetOrMakeDiscreteStar(sc.BodyName, -1, sys.Name);
                        bn.SetScan(sc);
                    }
                    else
                    {
                        //  $"Add OLD Scan NonStd format no parents {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress}".DO();
                        // we have no placement info, just store it.
                        BodyNode bn = sn.GetOrMakeDiscreteBody(ownname, sc.BodyID, sys.Name);
                        bn.SetScan(sc);
                    }
                }
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

        public void AddFSSBodySignalsToSystem(JournalFSSBodySignals sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddFSSBodySignalsToSystem(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        public void AddSAASignals(JournalSAASignalsFound sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddSAASignalsToSystem(sc) == null) // can't assign, store in pending
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
                    sn.SetCount(bodycount, nonbodycount);
                }
            }
        }

        // these might not be in the right system, if not, we need to pend it until it is.
        public bool AddFSSSignalsDiscovered(JournalFSSSignalDiscovered sc, bool pendit = true)
        {
            if (sc.Signals[0].SystemAddress.HasValue)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(new SystemClass(null, sc.Signals[0].SystemAddress));     // find by address only

                if  (sn != null)
                { 
                    lock (sn)
                    {
                        sn.AddFSSSignalsDiscovered(sc.Signals);
                        return true;
                    }
                }
                else if (pendit)
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


        public void AddApproachSettlement(JournalApproachSettlement sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }

        public void AddTouchdown(JournalTouchdown sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }


        public void AddSAAScan(JournalSAAScanComplete sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress )     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                lock (sn)
                {
                    if (sn.AddSAAScanToBody(sc) == null) // can't assign, store in pending
                        AddPending(sys.SystemAddress.Value, sc);
                }
            }
        }
        
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
                    if (sn.AddFSSBodySignalsToSystem(bs) != null)
                        todelete.Add(je);
                }
                else if (je is JournalSAASignalsFound saa)
                {
                    if (sn.AddSAASignalsToSystem(saa) != null)
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
                else if (je is IBodyFeature ass)
                {
                    if (sn.AddSurfaceFeatureToBody(ass) != null)
                        todelete.Add(je);
                }
                else if (je is JournalSAAScanComplete saasc)
                {
                    if (sn.AddSAAScanToBody(saasc) != null)
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
        private Dictionary<string, SystemNode> systemNodesByName { get; set; } = new Dictionary<string, SystemNode>();       // by name.

        private Dictionary<long, List<JournalEntry>> pendingsystemaddressevents = new Dictionary<long, List<JournalEntry>>();   // list of pending entries because their bodies are not yet available

        private object masterlock = new object();

        #endregion

    }
}

