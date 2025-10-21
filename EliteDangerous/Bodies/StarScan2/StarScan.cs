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

using BaseUtils.Win32Constants;
using EliteDangerousCore.JournalEvents;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        private Dictionary<long, SystemNode> SystemNodes { get; set; } = new Dictionary<long, SystemNode>();
        
        private List<JournalScanBaryCentre> pendingbarycentres = new List<JournalScanBaryCentre>();    

        // called when we have a new system. 
        public SystemNode GetOrAddSystem(ISystem sys)      
        {
            if (sys.SystemAddress.HasValue)
            {
                // System.Diagnostics.Debug.WriteLine($"SS2 Add {je.EventTimeUTC} {je.EventTypeID} {sys.Name} {sys.SystemAddress} {sys.HasCoordinate}");

                // find system node, if not, make it.  If its there, see if we have xyz co-ords
                if (!SystemNodes.TryGetValue(sys.SystemAddress.Value, out SystemNode node))
                {
                    node = new SystemNode(sys);
                    SystemNodes.Add(sys.SystemAddress.Value, node);
                }
                else if (sys.HasCoordinate && !node.System.HasCoordinate)
                {
                    node.ResetSystem(sys);
                    //System.Diagnostics.Debug.WriteLine($"SS2 Update co-ords of {je.EventTimeUTC} {je.EventTypeID} {sys.Name}");
                }

                return node;
            }
            else
            {
                // System.Diagnostics.Debug.WriteLine($"SS2 Can't Add {je.EventTimeUTC} {je.EventTypeID} {sys.Name} {sys.HasCoordinate}");
                return null;
            }
        }

        // called when we have a body in this system
        public void AddBody(IBodyNameAndID sc, ISystem sys)      
        {
            // we need this info to proceed.
            if (sc.SystemAddress != null && sys.SystemAddress == sc.SystemAddress && sc.BodyID != null && sc.Body != null && true)
            {
                bool stdname = sc.Body.StartsWith(sys.Name) && sc.Body.Length > sys.Name.Length;
                string ownname = stdname ? sc.Body.Substring(sys.Name.Length).Trim() : sc.Body;

                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                //System.Diagnostics.Debug.WriteLine($"Add Body {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` -> `{ownname}` in `{sys.Name}` {sys.SystemAddress}");

                if (sc.BodyType == "Station")
                {

                }
                else if (sc.BodyType == "Planet")
                {
                    if (stdname)
                    {
                        // its a standard name patterned after star
                        sn.GetOrMakeStandardBodyNode(ownname, sc.BodyID, sys.Name);
                    }
                    else
                    {
                        // non standard
                        sn.GetOrMakeNonStandardBody(ownname, sc.BodyID, sys.Name);
                    }
                }
                else if (sc.BodyType == "Star")
                {
                    if (stdname)
                    {
                        sn.GetOrMakeStandardBodyNode(ownname, sc.BodyID, sys.Name);
                    }
                    else
                    {
                        sn.GetOrMakeNonStandardStar(sc.Body, sc.BodyID, sys.Name);
                    }
                }
                else if (sc.BodyType == "PlanetaryRing")
                {
                    //  System.Diagnostics.Debug.WriteLine($"Add Planet Ring {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress}");
                }
                else if (sc.BodyType == "Barycentre")
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
        }


        public void AddScan(JournalScan sc, ISystem sys)
        {
            // reject older scans without bodyid, system address

            if (sc.BodyID != null && (sc.BodyID == 0 || sc.Parents != null) && sys.SystemAddress != null && sc.SystemAddress != null && true)     // if we have basic info
            {
                if (sc.SystemAddress != null && sc.SystemAddress != sys.SystemAddress)          // if we are arguing about the system address
                {
                    System.Diagnostics.Debug.WriteLine($"Add Scan Differ SystemAddress {sc.EventTimeUTC} bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress}");
                }
                else
                {
                    SystemNode sn = GetOrAddSystem(sys);
                    System.Diagnostics.Debug.Assert(sn != null);

                    bool stdname = sc.BodyName.StartsWith(sys.Name) && sc.BodyName.Length > sys.Name.Length;
                    string ownname = stdname ? sc.BodyName.Substring(sys.Name.Length).Trim() : sc.BodyName;

                    if (stdname)
                    {
                        System.Diagnostics.Debug.WriteLine($"Add Scan Std format {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                        sn.GetOrMakeStandardBodyNode(ownname, sc.BodyID, sys.Name, sc);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Add Scan NonStd format {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                        sn.GetOrMakeNonStandardBodyFromScan(sc, sys, ownname);
                    }

                    // any scan barycentres we may be able to process now for this system

                    List<JournalScanBaryCentre> todelete = new List<JournalScanBaryCentre>();
                    foreach (JournalScanBaryCentre sbe in pendingbarycentres.Where(x=>x.SystemAddress == sys.SystemAddress))
                    {
                        if ( sn.AddBaryCentreScan(sbe) != null)
                            todelete.Add(sbe);
                    }

                    foreach(var sbe in todelete)
                        pendingbarycentres.Remove(sbe);
                }
            }
        }
        public void AddBarycentre(JournalScanBaryCentre sc, ISystem sys, bool saveit = true)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddBaryCentreScan(sc) == null)        // can't assign, process
                    pendingbarycentres.Add(sc);
            }
        }

        public bool AddCodexEntryToSystem(JournalCodexEntry jsd, bool saveprocessinglater = true)
        {
            return false;
        }

        public bool AddFSSBodySignalsToSystem(JournalFSSBodySignals jsaa, ISystem sys)
        {
            return false;
        }

        public void SetFSSDiscoveryScan(int? bodycount, int? nonbodycount, ISystem sys)
        {

        }

        public bool AddFSSSignalsDiscovered(JournalFSSSignalDiscovered jsd, bool saveprocessinglater = true)
        {
            return false;
        }

        public bool AddSAASignals(JournalSAASignalsFound jsaa, ISystem sys)
        {
            return false;
        }

        public bool AddScanOrganic(JournalScanOrganic jsaa, ISystem sys)
        {
            return false;
        }


        public bool AddSAAScan(JournalSAAScanComplete jsaa, ISystem sys)
        {
            return false;
        }
        
        public void AddDocking(JournalDocked jd, ISystem sys, int bodyid)
        {

        }
        public void AddDocking(JournalDocked jd, ISystem sys)
        {

        }

        public void AddApproachSettlement(JournalApproachSettlement jd, ISystem sys)
        {

        }

        public void AddTouchdown(JournalTouchdown jd, ISystem sys)
        {

        }

        // IBodyNameAndID : ApproachBody, LeaveBody, CarrierJump, Location, SupercruiseExit
        // IBodyFeature : ApproachSettlement, BJournalDocked, JournalTouchdown

        // debug - run these thru
        public void Debug(List<HistoryEntry> hist)
        {
            foreach (var he in hist)
            {
                if (he.journalEntry is IStarScan ss)
                {
                    if (he.journalEntry is JournalScan js)
                        System.Diagnostics.Debug.WriteLine($"\r\n{he.EntryType}: `{js.BodyName}` ID: {js.BodyID} - {js.ParentList()} ");
                    else
                        System.Diagnostics.Debug.WriteLine($"\r\n{he.EntryType}: in {he.System.Name}");
                    (he.journalEntry as IStarScan).AddStarScan(this, he.System);
                    //DumpTree();
                }
                else if ( he.journalEntry is JournalDocked dck)
                {
                    if (StationDefinitions.IsPlanetaryPort(he.Status.FDStationType ?? StationDefinitions.StarportTypes.Unknown) && he.Status.BodyID.HasValue)
                    {
                        dck.BodyID = he.Status.BodyID;
                        dck.BodyType = "Settlement";
                        dck.Body = he.Status.BodyName;

                        AddDocking(he.journalEntry as JournalDocked, he.System, he.Status.BodyID.Value);
                        //   StarScan2.AddDocking(jd, he.System, he.Status.BodyID.Value);
                    }
                    else
                    {
                        dck.BodyType = "Station";
                        AddDocking(he.journalEntry as JournalDocked, he.System);
                        //    StarScan2.AddDocking(jd, he.System);
                    }
                }
                else if (he.journalEntry is IBodyNameAndID bi)
                {
                    System.Diagnostics.Debug.WriteLine($"\r\n{he.EntryType}: `{bi.Body}`:{bi.BodyID} in {he.System.Name}");
                    AddBody(he.journalEntry as IBodyNameAndID, he.System);
                    DumpTree();
                }
            }
        }

        public void DumpTree()
        {
            foreach (var kvp in SystemNodes)
            {
                kvp.Value.DumpTree();

                //foreach (var b in kvp.Value.Bodies(x=>x.BodyID>=2,false))
                //{
                //    System.Diagnostics.Debug.WriteLine($"Yield body {b.OwnName} id {b.BodyID}");
                //}
            }
        }

    }
}

