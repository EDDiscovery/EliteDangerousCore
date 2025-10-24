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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Public IF

        // try and find the address, if we know it, else null
        public ISystem FindSystemInfo(string sysname)
        {
            return systemNodesByName.TryGetValue(sysname, out var node) ? node.System : null;
        }

        // Find system, optinally do weblookup
        // null if not found
        public SystemNode FindSystemSynchronous(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    
        {
            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if (sys.SystemAddress.HasValue || sys.Name.HasChars())      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys))
                {
                    var lookupres = Spansh.SpanshClass.GetBodiesList(sys, usespansh);          // see if spansh has it cached or optionally look it up
                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, lookupres.System.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemSync spansh add {lookupres.System.Name} {js.BodyName}");
                            AddScan(js, sys);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, lookupres.System);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys))
                {
                    var lookupres = EDSM.EDSMClass.GetBodiesList(sys, useedsm);            // see if edsm has it cached or optionally look it up
                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, lookupres.System.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemSync edsn add {lookupres.System.Name} {lookupres.System.SystemAddress} {js.BodyName}");
                            AddScan(js, sys);
                        }
                    }
                }

                if (sys.SystemAddress.HasValue && systemNodesByAddress.TryGetValue(sys.SystemAddress.Value, out var systemNode))
                {
                    return systemNode;
                }
                else if (systemNodesByName.TryGetValue(sys.Name, out var systemNode1))
                {
                    return systemNode1;
                }
            }

            return null;
        }

        // you must be returning void to use this..
        // Sys can be an address, a name, or a name and address. address takes precedence
        public async System.Threading.Tasks.Task<SystemNode> FindSystemAsync(ISystem sys, WebExternalDataLookup weblookup = WebExternalDataLookup.None)    // Find the system. Optionally do a EDSM web lookup
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // foreground only
            System.Diagnostics.Debug.Assert(sys != null);

            if (sys.SystemAddress.HasValue || sys.Name.HasChars())      // we have good enough data (should have)
            {
                bool usespansh = weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;
                bool useedsm = weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM || weblookup == WebExternalDataLookup.All;

                if (usespansh && !Spansh.SpanshClass.HasBodyLookupOccurred(sys))
                {
                    if (sys.Name.IsEmpty())
                        System.Diagnostics.Debug.WriteLine($"WARNING - Spansh lookup with empty name of system is liable to errors - cant set body designation properly");

                    var lookupres = await Spansh.SpanshClass.GetBodiesListAsync(sys, usespansh);          // see if spansh has it cached or optionally look it up

                    if (lookupres != null)
                    {
                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, lookupres.System.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemASync spansh add {lookupres.System.Name} {lookupres.System.SystemAddress} {js.BodyName} -> {js.BodyDesignation}");
                            AddScan(js, sys);
                        }

                        if (lookupres.BodyCount.HasValue)
                            SetFSSDiscoveryScan(lookupres.BodyCount, null, lookupres.System);

                        if (weblookup == WebExternalDataLookup.SpanshThenEDSM)      // we got something, for this option, we are fine, don't use edsm
                            useedsm = false;
                    }
                }

                if (useedsm && !EDSM.EDSMClass.HasBodyLookupOccurred(sys))     // using edsm, no lookup, go
                {
                    var lookupres = await EliteDangerousCore.EDSM.EDSMClass.GetBodiesListAsync(sys, useedsm);

                    if (lookupres != null)
                    {
                        if (sys.Name.IsEmpty())
                            System.Diagnostics.Debug.WriteLine($"WARNING - Spansh lookup with empty name of system is liable to errors - cant set body designation properly");

                        foreach (JournalScan js in lookupres.Bodies)
                        {
                            js.BodyDesignation = BodyDesignations.GetBodyDesignation(js, lookupres.System.Name);
                            //System.Diagnostics.Debug.WriteLine($"FindSystemSync edsn add {lookupres.System.Name} {lookupres.System.SystemAddress} {js.BodyName}");
                            AddScan(js, sys);
                        }
                    }
                }

                if (sys.SystemAddress.HasValue && systemNodesByAddress.TryGetValue(sys.SystemAddress.Value, out var systemNode))
                {
                    return systemNode;
                }
                else if (systemNodesByName.TryGetValue(sys.Name, out var systemNode1))
                {
                    return systemNode1;
                }
            }

            return null;
        }

        #endregion

        #region Called by Events

        // called to make sure we have a system. If system address, we use the address system, else we assign to the old tree
        public SystemNode GetOrAddSystem(ISystem sys)      
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

            return null;
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

                if (sc.BodyType == BodyDefinitions.BodyType.Station)
                {

                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Planet)
                {
                    sn.GetOrMakeDiscreteBody(ownname, sc.BodyID.Value, sys.Name);
                }
                else if (sc.BodyType == BodyDefinitions.BodyType.Star)
                {
                    sn.GetOrMakeDiscreteStar(sc.Body, sc.BodyID, sys.Name);
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
                    //System.Diagnostics.Debug.Assert(true);
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"Add NO SYS ADDR or mismatch {body.EventTimeUTC} bid:{body.BodyID} type:{body.BodyType} `{body.Body}` {body.SystemAddress} {sys.SystemAddress}");

            }
        }

        public void AddScan(JournalScan sc, ISystem sys)
        {
            // triage for scans with full info
            if (sc.BodyID != null && (sc.BodyID == 0 || sc.Parents != null) && sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have modern info 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                if (sn.OldScansPresent)                 // we don't mix old scans with new scans. Old scans without parents trees are problematic
                {
                    sn.OldScansPresent = false;
                    sn.Clear();
                }

                bool stdname = sc.BodyName.StartsWith(sys.Name) && sc.BodyName.Length > sys.Name.Length;
                string ownname = stdname ? sc.BodyName.Substring(sys.Name.Length).Trim() : sc.BodyName;

                if (stdname)
                {
                    System.Diagnostics.Debug.WriteLine($"Add Scan Std format {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                    sn.GetOrMakeStandardBodyNodeFromScan(sc, ownname, sc.BodyID, sys.Name);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Add Scan NonStd format {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                    sn.GetOrMakeNonStandardBodyFromScan(sc, sys, ownname);
                }
            }
            else if (sys.Name != null)      // else dump them into the older structure
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);

                bool stdname = sc.BodyName.StartsWith(sys.Name) && sc.BodyName.Length > sys.Name.Length;
                string ownname = stdname ? sc.BodyName.Substring(sys.Name.Length).Trim() : sc.BodyName;

                bool hasparentbodyid = sc.BodyID != null && (sc.BodyID == 0 || sc.Parents != null);     // no body/parent tree makes the scan problematc to make
                if (!hasparentbodyid)
                    sn.OldScansPresent = true;          // record it

                if (stdname)
                {
                    System.Diagnostics.Debug.WriteLine($"Add OLD Scan Std format {sc.EventTimeUTC} `{sc.BodyName}` ownname `{ownname}`:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                    sn.GetOrMakeStandardBodyNodeFromScan(sc, ownname, sc.BodyID, sys.Name);
                }
                else if (hasparentbodyid)
                {
                    System.Diagnostics.Debug.WriteLine($"Add OLD Scan NonStd format with Parents {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                    sn.GetOrMakeNonStandardBodyFromScan(sc, sys, ownname);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Add OLD Scan NonStd format {sc.EventTimeUTC} `{sc.BodyName}` bid:{sc.BodyID} `{sc.StarType}{sc.PlanetClass}` sa:{sc.SystemAddress}  in `{sys.Name}` {sys.SystemAddress} P: {sc.ParentList()}");
                    
                    // we have no placement info, just store it.
                    BodyNode bn = sn.GetOrMakeDiscreteBody(ownname, sc.BodyID, sys.Name);
                    bn.SetScan(sc);
                }
            }
        }

        public void AddBarycentre(JournalScanBaryCentre sc, ISystem sys, bool saveit = true)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddBaryCentreScan(sc) == null)        // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        public void AddCodexEntryToSystem(JournalCodexEntry sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if ( sn.AddCodexEntryToSystem(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        public void AddFSSBodySignalsToSystem(JournalFSSBodySignals sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddFSSBodySignalsToSystem(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        public void AddSAASignals(JournalSAASignalsFound sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddSAASignalsToSystem(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        public void SetFSSDiscoveryScan(int? bodycount, int? nonbodycount, ISystem sys)
        {
            if (sys.SystemAddress != null )     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                sn.SetCount(bodycount, nonbodycount);
            }
        }

        // these might not be in the right system, if not, we need to pend it until it is.
        public bool AddFSSSignalsDiscovered(JournalFSSSignalDiscovered sc, bool pendit = true)
        {
            if (sc.Signals[0].SystemAddress.HasValue)     // if we have basic info. If we don't have a system  address it pointless trying because we won't have bodyid
            {
                if (systemNodesByAddress.TryGetValue(sc.Signals[0].SystemAddress.Value, out SystemNode sn))     // if we have it
                {
                    sn.AddFSSSignalsDiscovered(sc.Signals);
                    return true;
                }
                else if ( pendit )
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
                if (sn.AddScanOrganicToBody(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }


        public void AddApproachSettlement(JournalApproachSettlement sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        public void AddTouchdown(JournalTouchdown sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress && sc.BodyID != null)     // if we have basic info. First ones did not have body ID
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddSurfaceFeatureToBody(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }


        public void AddSAAScan(JournalSAAScanComplete sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress )     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddSAAScanToBody(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }
        
        public void AddDocking(JournalDocked sc, ISystem sys)
        {
            if (sys.SystemAddress != null && sc.SystemAddress == sys.SystemAddress )     // if we have basic info. 
            {
                SystemNode sn = GetOrAddSystem(sys);
                System.Diagnostics.Debug.Assert(sn != null);
                if (sn.AddDockingToBody(sc) == null) // can't assign, store in pending
                    AddPending(sys.SystemAddress.Value, sc);
            }
        }

        #endregion

        #region Post creation clean up

        // Given the pending list, assign it to the database of systems
        public void AssignPending()
        {
            List<long> todeletesys = new List<long>();

            foreach (var kvp in pendingsystemaddressevents)
            {
                SystemNode sn = systemNodesByAddress[kvp.Key];       // must be there, can't not be

                if (AssignPending(sn, kvp.Value))
                    todeletesys.Add(kvp.Key);
            }

            foreach (var sysaddress in todeletesys)
                pendingsystemaddressevents.Remove(sysaddress);
         
            foreach (var kvp in pendingsystemaddressevents)
            {
                System.Diagnostics.Debug.WriteLine($"StarScan Pending left system {kvp.Key} count {kvp.Value.Count}");
            }
        }

        public void ClearPending()
        {
            pendingsystemaddressevents.Clear();
        }

        #endregion

        #region Debug tools

        // read in a set of JSON lines exported from say HistoryList.cs:294 and run it thru starscan 2 and the display system

        public static void ProcessAllFromDirectory(string dir, string filepattern, string displaytodir, int width)
        {
            FileInfo[] find = Directory.EnumerateFiles(dir, filepattern , SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

            foreach (var x in find)
            {
                ProcessFromFile(x.FullName, displaytodir, width);   
            }
        }

        public static void ProcessFromFile(string filename, string displaytodir, int width)
        {
            string[] jsonlines = FileHelpers.TryReadAllLinesFromFile(filename);
            string lastsystemname = "";

            List<HistoryEntry> helist = new List<HistoryEntry>();       // create He's
            HistoryEntry last = null;
            foreach (string line in jsonlines)
            {
                if (line.HasChars() && !line.StartsWith("//"))
                {
                    JournalEntry entry = JournalEntry.CreateJournalEntry(line);
                    if (entry is JournalFSDJump fsd)
                        lastsystemname = fsd.StarSystem;
                    HistoryEntry he1 = HistoryEntry.FromJournalEntry(entry, last, null);
                    helist.Add(he1);
                    last = he1;
                }
            }

            StarScan2.StarScan ss2 = new StarScan2.StarScan();
            ss2.ProcessFromHistory(helist);
            ss2.DumpTree();
            StarScan2.SystemDisplay sd = new StarScan2.SystemDisplay();
            sd.Font = new System.Drawing.Font("Arial", 10);
            sd.SetSize(64);
            sd.TextBackColor = Color.Transparent;

            ISystem syst = ss2.FindSystemInfo(lastsystemname);
            StarScan2.SystemNode sssol = ss2.FindSystemSynchronous(syst);
            ExtendedControls.ExtPictureBox imagebox = new ExtendedControls.ExtPictureBox();
            imagebox.FillColor = Color.AliceBlue;
            sd.DrawSystemRender(imagebox, width, sssol);
            imagebox.Image.Save(Path.Combine(displaytodir,$"{lastsystemname}.png"));
        }

        // run these history entries thru the star scanner
        public void ProcessFromHistory(List<HistoryEntry> hist)
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
                   // DumpTree();
                }
                else if ( he.journalEntry is JournalDocked dck)
                {
                    if (dck.BodyType == BodyDefinitions.BodyType.Settlement)        // if its a settlement, fill in missing body info
                    {
                        dck.BodyID = he.Status.BodyID;
                        dck.Body = he.Status.BodyName;
                    }

                    AddDocking(dck, he.System);
                }
                else if (he.journalEntry is IBodyNameAndID bi)
                {
                //    System.Diagnostics.Debug.WriteLine($"\r\n{he.EntryType}: `{bi.Body}`:{bi.BodyID} in {he.System.Name}");
                    AddBody(he.journalEntry as IBodyNameAndID, he.System);
                  //  DumpTree();
                }
            }

            AssignPending();
        }

        public void DumpTree()
        {
            foreach (var kvp in systemNodesByName)
            {
                kvp.Value.DumpTree();
            }
        }

        #endregion

        #region Helpers

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
            if (!pendingsystemaddressevents.TryGetValue(systemaddress, out List<JournalEntry> jelist))
            {
                pendingsystemaddressevents[systemaddress] = new List<JournalEntry>();
            }
            pendingsystemaddressevents[systemaddress].Add(sc);

        }

        #endregion

        #region vars
        private Dictionary<long, SystemNode> systemNodesByAddress { get; set; } = new Dictionary<long, SystemNode>();       // by address
        private Dictionary<string, SystemNode> systemNodesByName { get; set; } = new Dictionary<string, SystemNode>();       // by name.

        private Dictionary<long, List<JournalEntry>> pendingsystemaddressevents = new Dictionary<long, List<JournalEntry>>();   // list of pending entries because their bodies are not yet available

        #endregion

    }
}

