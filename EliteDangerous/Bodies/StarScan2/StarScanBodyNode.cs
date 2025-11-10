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
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("`{OwnName}` `{CanonicalName}` {BodyID} {BodyType} : CB{ChildBodies.Count} Cx{CodexEntries?.Count} FSS{FSSSignalList?.Count} Org{Organics?.Count} Gen {Genuses?.Count}")]

    public partial class BodyNode
    {
        #region Public

        public string OwnName { get; private set; }     // own name (1,2,A,Mitterand Hollow) for scan names, or computed Barycentre name BC of A,B,C
        public string CanonicalName { get; private set; }  // set on scans to js.BodyName, and on discrete body adds of names (Miterrand Hollow)
                                                           // will contain the whole name (HIP 1885 A 5 b, HIP 1885 A 2 A Ring, HIP 1885 A A Belt Cluster 1)
                                                           // will not be set on sub part names/barycentres. Be Null

        public string CanonicalNameOrOwnName => CanonicalName ?? OwnName;

        // without system name
        public string CanonicalNameNoSystemName() { return CanonicalName?.ReplaceIfStartsWith(SystemNode.System.Name); }

        // best name, either Canonicalname without system, or ownname
        public string Name() { return CanonicalName?.ReplaceIfStartsWith(SystemNode.System.Name) ?? OwnName; }

        public int BodyID { get; private set; }

        public enum BodyClass { System,             // top level SystemBodies object in SystemNode
                                Star,               // a top level star, or a substar
                                PlanetMoon,         // a planet or moon
                                Barycentre,         // a barycentre ('Null' type in parents array)
                                BeltCluster,        // a 'A Belt Cluster' name (`Ring` type in the parents array) - has BeltClusterBody underneath it
                                                    // also from the journalscan of a star, the rings/belts of the star broken out into children. BeltData is set. See ProcessBelts.      
                                BeltClusterBody,    // items under a belt cluster
                                PlanetaryRing,      // Called "A Ring" or "B Ring", from a journalscan of the ring (Scan will be set), or from the journalscan of a planet with its ring structure broken into children, BeltData is set.
                                Unknown,            // for older scans for tree bodies above scan, we mark Unknown
                              
                            };
        public BodyClass BodyType { get; private set; }

        public bool IsStarOrPlanet { get { return BodyType == BodyClass.Star || BodyType == BodyClass.PlanetMoon; } }
        public bool IsPlanetOrMoon { get { return BodyType == BodyClass.PlanetMoon; } }
        public bool IsBarycentre { get { return BodyType == BodyClass.Barycentre; } }

        public bool WebCreatedNode { get { return Scan?.DataSource == SystemSource.FromEDSM || Scan?.DataSource == SystemSource.FromSpansh; } }

        public List<BodyNode> ChildBodies { get; private set; } = new List<BodyNode>();

        public BodyNode Parent { get; private set; } = null;            // null if SystemNode.SystemBodies, else always set
        public SystemNode SystemNode { get; private set; } = null;      // null if SystemNode.SystemBodies, else always set

        public JournalScan Scan { get; private set; } = null;                           // type Star/PlanetMoon/BeltClusterBody
        public JournalScanBaryCentre BarycentreScan { get; private set; } = null;       // type Barycentre
        public StarPlanetRing BeltData { get; private set; } = null;        // type BeltClusterBody or PlanetaryRing
        public double? SMA { get { return Scan != null ? Scan.nSemiMajorAxis : BarycentreScan != null ? BarycentreScan.SemiMajorAxis : BeltData != null ? BeltData.InnerRad : default(double?); } }

        public List<JournalCodexEntry> CodexEntries { get; private set; } = null;
        public List<JournalSAASignalsFound.SAASignal> Signals { get; private set; } = null;
        public List<JournalSAASignalsFound.SAAGenus> Genuses { get; private set; } = null;
        public List<FSSSignal> FSSSignalList { get; private set; } = null;       // only for SystemBodies in StarScan.SystemNode
        public List<JournalScanOrganic> Organics { get; internal set; } = null;
        public List<IBodyFeature> Features { get; internal set; } = null;        // for SystemBodies, the orbiting stations, for other bodies touchdown/settlements
        public bool IsMapped { get; private set; }                   // recorded here since the scan data can be replaced by a better version later.
        public bool WasMappedEfficiently { get; private set; }

        // is this type at the top level of the tree. false if one above
        public bool IsTopLevel(BodyClass bc)
        {
            BodyNode bn = Parent;
            while (bn != null)
            {
                if (bn.BodyType == bc)
                    return false;

                bn = bn.Parent;
            }

            return true;
        }

        // return a star above which has a scan, 0,1,2 times above
        public BodyNode GetStarAboveWithScan(int times = 0)
        {
            BodyNode bn = Parent;
            while (bn != null)
            {
                if (bn.Scan?.IsStar ?? false)       // scan, is star
                {
                    if (times-- == 0)
                        return bn;
                }

                bn = bn.Parent;
            }

            return null;
        }

        // calculate depth of body
        public int GetDepthIgnoreBC()
        {
            int depth = 0;
            BodyNode bn = Parent;
            while (bn != null && bn.BodyType != BodyClass.System)
            {
                if (bn.BodyType != BodyClass.Barycentre)      // don't count the top level barycentre or system as depth
                    depth++;

                bn = bn.Parent;
            }
            return depth;
        }

        public BodyNode GetParentIgnoreBC()
        {
            BodyNode bn = Parent;
            while (bn != null && bn.BodyType != BodyClass.System)
            {
                if (bn.BodyType != BodyClass.Barycentre)      // don't count the top level barycentre or the top system node
                    return bn;

                bn = bn.Parent;
            }
            return null;
        }

        // accumulate all sibling bodies excluding barycentres, and if a barycentre is a sibling, returns its child bodies
        public List<BodyNode> GetSiblingBodiesNoBarycentres()
        {
            List<BodyNode> bnl = new List<BodyNode>(Parent.ChildBodies.Where(x => !x.IsBarycentre));

            foreach (var bnc in Parent.ChildBodies.Where(x => x.IsBarycentre))
            {
                foreach (var y in bnc.ChildBodies.Where(x => !x.IsBarycentre))
                {
                    bnl.Add(y);
                }
            }

            return bnl;
        }
        // accumulate all sibling bodies excluding barycentres, and if a barycentre is a sibling, returns its child bodies
        public List<BodyNode> GetChildBodiesNoBarycentres()
        {
            List<BodyNode> bnl = new List<BodyNode>(ChildBodies.Where(x => !x.IsBarycentre));

            foreach (var bnc in ChildBodies.Where(x => x.IsBarycentre))
            {
                foreach (var y in bnc.ChildBodies.Where(x => !x.IsBarycentre))
                {
                    bnl.Add(y);
                }
            }

            return bnl;
        }

        // give a body, return its star above which has a scan, N times.
        public BodyNode GetStarAboveScanned(int times)
        {
            var bn = GetStarAboveWithScan(times);
            if (bn == null && times == 0)
            {
                var star = SystemNode.GetStarsScanned().FirstOrDefault();        // did not find above, so just get the stats with scan list, and pick the first
                return star;
            }
            else
                return bn;
        }

        // filternames are either the Star/Planet IDs, or its the BodyType for other ones
        public bool IsBodyTypeInFilter(string[] filternames, bool checkchildren)
        {
            if (IsBodyTypeInFilter(filternames))
                return true;

            if (checkchildren)
            {
                foreach (var body in Bodies())
                {
                    if (body.IsBodyTypeInFilter(filternames, checkchildren))
                        return true;
                }
            }
            return false;
        }

        public bool IsBodyTypeInFilter(string[] filternames)
        {
            if (filternames.Contains("All"))
                return true;

            string name = BodyType.ToString();

            if (Scan != null)
            {
                if (Scan.IsStar)
                    name = Scan.StarTypeID.ToString();
                else if (Scan.IsPlanet)
                    name = Scan.PlanetTypeID.ToString();
            }

            return filternames.Contains(name, StringComparer.InvariantCultureIgnoreCase);
        }

        public bool DoesNodeHaveNonWebScansBelow()
        {
            foreach (var csn in ChildBodies)
            {
                if (csn.DoesNodeHaveNonWebScansBelow() == true)  // we do have non web ones under this child
                    return true;
            }

            return WebCreatedNode == false;
        }

        public int SurfaceFeatureListSettlementsCount()
        {
            return Features?.Count(x => x is JournalApproachSettlement || x is JournalDocked) ?? 0;
        }

        public long ScanValue(bool includewebvalue)
        {
            long value = 0;

            foreach (var body in Bodies())
            {
                if (body?.Scan != null)
                {
                    if (includewebvalue || !body.Scan.IsWebSourced)
                    {
                        value += body.Scan.EstimatedValue;
                    }
                }
            }

            return value;
        }

        public int StarPlanetsScanned()      // This corresponds to FSSDiscoveryScan
        {
            return Bodies(x => (x.BodyType == BodyNode.BodyClass.Star || x.BodyType == BodyNode.BodyClass.PlanetMoon) && x.Scan != null).Count();
        }
        public int StarPlanetsScanned(bool includewebbodies)        // includewebbodies gives same total as above, false means only include ones which we have scanned
        {
            return Bodies(x => (x.BodyType == BodyNode.BodyClass.Star || x.BodyType == BodyNode.BodyClass.PlanetMoon) && x.Scan != null && (includewebbodies || !x.Scan.IsWebSourced)).Count();
        }
        public int StarsScanned()      // stars scanned
        {
            return Bodies(b => b.Scan?.IsStar ?? false).Count();
        }
        public IEnumerable<BodyNode> GetStarsScanned()
        {
            return Bodies(b => b.Scan?.IsStar ?? false);
        }
        public int PlanetsWithScan()      // planets scanned
        {
            return Bodies(b => b.Scan?.IsPlanet ?? false).Count();
        }
        public int BeltClusters() // number of belt clusters
        {
            return Bodies(b => b.BodyType == BodyNode.BodyClass.BeltCluster).Count();
        }
        public int BeltClusterBodies() // number of belt clusters bodies
        {
            return Bodies(b => b.BodyType == BodyNode.BodyClass.BeltClusterBody).Count();
        }

        public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
        public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
        public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
        public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
        public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
        public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
        public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }
        public int CountOrganicsScansAnalysed { get { return Organics?.Where(x => x.ScanType == JournalScanOrganic.ScanTypeEnum.Analyse).Count() ?? 0; } }

        // which feature is nearby?  Handles no surface features
        public IBodyFeature FindSurfaceFeatureNear(double? latitude, double? longitude, double delta = 0.1)
        {
            if (latitude.HasValue && longitude.HasValue && Features != null)
                return Features.Find(x => x.HasLatLong &&
                                                       System.Math.Abs(x.Latitude.Value - latitude.Value) < delta && System.Math.Abs(x.Longitude.Value - longitude.Value) < delta);
            else
                return null;
        }


        // return all child bodies down the tree, or return all bodies matching a Predicate, or just return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            foreach (BodyNode sn in ChildBodies)        // check children
            {
                // global::System.Diagnostics.Debug.WriteLine($"TYield {sn.OwnName} bid {sn.BodyID}");

                if (find == null || find(sn))
                {
                    yield return sn;
                    if (find != null && stoponfind)
                        yield break;
                }

                foreach (BodyNode c in sn.Bodies(find, stoponfind))          // recurse back up to go as deep as required
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

        // Using info here, and the indicated journal scan node, return suryeyor info on this node
        public string SurveyorInfoLine(ISystem sys, bool showsignals, bool showorganics, bool showvolcanism, bool showvalues, bool shortinfo, bool showGravity, bool showAtmos, bool showTemp, bool showRings,
                        int lowRadiusLimit, int largeRadiusLimit, double eccentricityLimit)
        {
            if (Scan != null)
            {
                bool hasthargoidsignals = Signals?.Find(x => x.IsThargoid) != null && showsignals;
                bool hasguardiansignals = Signals?.Find(x => x.IsGuardian) != null && showsignals;
                bool hashumansignals = Signals?.Find(x => x.IsHuman) != null && showsignals;
                bool hasothersignals = Signals?.Find(x => x.IsOther) != null && showsignals;
                bool hasminingsignals = Signals?.Find(x => x.IsUncategorised) != null && showsignals;
                bool hasgeosignals = Signals?.Find(x => x.IsGeo) != null && showsignals;
                bool hasbiosignals = Signals?.Find(x => x.IsBio) != null && showsignals;
                bool hasscanorganics = Organics != null && showorganics;

                return Scan.SurveyorInfoLine(sys, hasminingsignals, hasgeosignals, hasbiosignals,
                            hasthargoidsignals, hasguardiansignals, hashumansignals, hasothersignals, hasscanorganics,
                            showvolcanism, showvalues, shortinfo, showGravity, showAtmos, showTemp, showRings,
                            lowRadiusLimit, largeRadiusLimit, eccentricityLimit);
            }
            else
                return string.Empty;
        }

        public static void ToJSON(JObject jo, BodyNode node)
        {
            node.ToJson(jo);
            if (node.ChildBodies.Count > 0)
            {
                JArray children = new JArray();
                foreach (var x in node.ChildBodies)
                {
                    JObject ji = new JObject();
                    ToJSON(ji, x);
                    children.Add(ji);
                }

                jo.Add("Children", children);
            }
        }

        // Dumps out orbital parameters. Distance is km, as per Kepler Orbital Parameters/Horizons
        public void ToJson(JObject obj)
        {
            var top = this;
            obj["NodeType"] = top.BodyType.ToString();

            if (top.BodyType != BodyClass.System)
            {
                obj["OwnName"] = top.OwnName;
                obj["BodyName"] = top.Name();
                obj["FullName"] = top.CanonicalNameOrOwnName;
                if (top.BodyID >= 0)
                {
                    obj["ID"] = top.BodyID;
                }

                obj["ScanDataPresent"] = top.Scan != null;         // one key to indicate data is available

                if (top.Scan != null)
                {
                    obj["Epoch"] = top.Scan.EventTimeUTC;
                    obj["DistanceFromArrival"] = top.Scan.DistanceFromArrivalm / 1000.0;        // in km


                    if (top.Scan.IsStar)
                    {
                        obj["StarType"] = top.Scan.StarTypeID.ToString();
                        obj["StarImage"] = top.Scan.StarTypeImageName;
                        obj["StarClass"] = top.Scan.StarClassificationAbv;
                        obj["StarAbsMag"] = top.Scan.nAbsoluteMagnitude;
                        obj["StarAge"] = top.Scan.nAge;
                    }
                    else if (top.Scan.IsPlanet)
                    {
                        obj["PlanetClass"] = top.Scan.PlanetTypeID.ToString();
                        obj["PlanetImage"] = top.Scan.PlanetClassImageName;
                    }

                    if (top.Scan.nRadius.HasValue)
                        obj["Radius"] = top.Scan.nRadius / 1000.0;  // in km

                    if (top.Scan.nSemiMajorAxis.HasValue)       // if we have semi major axis, we have orbital elements
                    {
                        obj["SemiMajorAxis"] = top.Scan.nSemiMajorAxis / 1000.0;        // in km
                        obj["Eccentricity"] = top.Scan.nEccentricity;  // degrees
                        obj["Inclination"] = top.Scan.nOrbitalInclination;  // degrees
                        obj["AscendingNode"] = top.Scan.nAscendingNodeKepler; // degrees
                        obj["Periapis"] = top.Scan.nPeriapsisKepler;// degrees
                        obj["MeanAnomaly"] = top.Scan.nMeanAnomaly;// degrees
                        obj["OrbitalPeriod"] = top.Scan.nOrbitalPeriod;// in seconds
                    }

                    if (top.Scan.nAxialTilt.HasValue)
                        obj["AxialTilt"] = top.Scan.nAxialTiltDeg;  // degrees

                    if (top.Scan.nRotationPeriod.HasValue)      // seconds
                        obj["RotationPeriod"] = top.Scan.nRotationPeriod;

                    if (top.Scan.nMassKG.HasValue)
                        obj["Mass"] = top.Scan.nMassKG; // kg

                    if (top.Scan.nSurfaceTemperature.HasValue)
                        obj["SurfaceTemp"] = top.Scan.nSurfaceTemperature;

                    if (top.Scan.nSurfaceTemperature.HasValue)
                        obj["SurfaceTemp"] = top.Scan.nSurfaceTemperature;

                    if (top.Scan.HasRings)
                    {
                        JArray ringarray = new JArray();
                        foreach (var r in top.Scan.Rings)
                        {
                            JObject jo = new JObject()
                            {
                                ["Type"] = r.RingClassID.ToString(),
                                ["InnerRad"] = r.InnerRad / 1000.0,  // in km
                                ["OuterRad"] = r.OuterRad / 1000.0,    // in km
                                ["MassMT"] = r.MassMT
                            };
                            ringarray.Add(jo);
                        }
                        obj["Rings"] = ringarray;
                    }

                    // other stuff.. tbd features etc.
                }
            }
        }

        #endregion

        #region Implementation For Creation

        public BodyNode(string ownname, BodyClass bc, int bid, BodyNode parent, SystemNode sys)
        {
            OwnName = ownname;
            BodyType = bc;
            BodyID = bid;
            Parent = parent;
            SystemNode = sys;
        }

        public BodyNode(string ownname, JournalScan sc, int bid, BodyNode parent, SystemNode sys) :
            this(ownname, sc.IsStar ? BodyNode.BodyClass.Star : sc.IsPlanet ? BodyNode.BodyClass.PlanetMoon : sc.IsPlanetaryRing ? BodyNode.BodyClass.PlanetaryRing : BodyNode.BodyClass.BeltClusterBody, bid, parent, sys)
        {
        }

        public BodyNode(string ownname, BodyParent nt, int bid, BodyNode parent, SystemNode sys) :
            this(ownname, nt.IsStar? BodyNode.BodyClass.Star : nt.IsBarycentre? BodyNode.BodyClass.Barycentre : nt.IsBeltCluster? BodyNode.BodyClass.BeltCluster : BodyNode.BodyClass.PlanetMoon, bid, parent, sys )
        {
        }

        public void ResetBodyID(int bid)
        {
            BodyID = bid;
        }
        public void ResetBodyName(string name)
        {
            OwnName = name;
        }
        public void ResetClass(BodyClass ty)
        {
            BodyType = ty;
        }
        public void ResetParent(BodyNode p)
        {
            Parent = p;
        }


        // copy all data to nd
        public void MoveStarDataTo(BodyNode nd)     
        {
            nd.ChildBodies = ChildBodies;
            nd.Scan = Scan;
            nd.BarycentreScan = BarycentreScan;
            nd.BeltData = BeltData;
            nd.CodexEntries = CodexEntries;
            nd.Signals = Signals;
            nd.Genuses = Genuses;
            nd.FSSSignalList = FSSSignalList;
            nd.Organics = Organics;
            nd.Features = Features;
            nd.IsMapped = IsMapped;
            nd.WasMappedEfficiently = WasMappedEfficiently;
        }

        public void SetScan(JournalScan sc)
        {
            if (Scan != null)           // if previously scanned, we may have set some information into Parents list, which we need to keep
            {
                if ( Scan.Parents!=null)        // if previous had parents
                {
                    BodyParent.AreParentsSame(Scan.Parents, sc.Parents).Assert("StarScan Set Scan noted parents list changed on new scan- whats up Frontier!");
                    sc.ResetParents(Scan.Parents);      // copy parents across to new scan, so we don't loose the barycentre info - bug found during testing 
                }
            }
            
            Scan = sc;                                  // copy across

            Scan.Signals = Signals;                     // we point Signals and Genuses at the nodes signals and genuses for the Search system.  
            Scan.Genuses = Genuses;                     // If these change by AddFSSBodySignals / AddFSSBodySignals they will change in sympathy
            Scan.CodexEntries = CodexEntries;                 // set codex as well
            Scan.SetMapped(IsMapped, WasMappedEfficiently);  // and we set the mapped/was mapped flag to the nodes setting set up by AddSAAScanComplete

            CanonicalName = Scan.BodyName;
        }

        public void SetCanonicalName(string s)
        {
            CanonicalName = s;
        }

        public void SetScan(JournalScanBaryCentre sc)
        {
            BarycentreScan = sc;
        }
        public void SetScan(StarPlanetRing sc)
        {
            BeltData = sc;
        }
        public void AddCodex(JournalCodexEntry sc)
        {
            if ( CodexEntries == null)
                CodexEntries = new List<JournalCodexEntry>();
            CodexEntries.Add(sc);
        }
        public void AddSignals(List<JournalSAASignalsFound.SAASignal> sc)
        {
            if (Signals == null)
                Signals = new List<JournalSAASignalsFound.SAASignal>();    

            foreach (var g in sc)
            {
                int present = Signals.FindIndex(x => x.Type == g.Type && x.Count == g.Count);
                if (present == -1)
                    Signals.Add(g);
            }
        }
        public void AddGenuses(List<JournalSAASignalsFound.SAAGenus> sc)
        {
            if (Genuses == null)
                Genuses = new List<JournalSAASignalsFound.SAAGenus>();

            foreach (var g in sc)
            {
                int present = Genuses.FindIndex(x => x.Genus == g.Genus);
                if (present == -1)
                    Genuses.Add(g);
            }
        }

        public void AddFSSSignals(List<FSSSignal> signals)
        {
            if (FSSSignalList == null)
                FSSSignalList = new List<FSSSignal>();

            foreach (var s in signals)
            {
                int present = FSSSignalList.FindIndex(x => x.IsSame(s));
                if (present < 0)        // don't repeat
                    FSSSignalList.Add(s);
            }
        }
        public void AddScanOrganics(JournalScanOrganic organic)
        {
            if ( Organics == null )
                Organics = new List<JournalScanOrganic>();

            Organics.Add(organic);
        }

        public bool AddFeatureOnlyIfNew(IBodyFeature sc)
        {
            if (Features == null)
                Features = new List<IBodyFeature>();

            var prev = Features.Find(x => x.Name == sc.Name);        // have we got it before?, if so, don't replace
            if (prev == null)
            {
                Features.Add(sc);
                return true;
            }
            else
                return false;
        }

        public void SetMapped(bool efficently)
        {
            IsMapped = true; WasMappedEfficiently = efficently;
        }

        public void AddDocking(JournalDocked sc)
        {
            if (Features == null)
                Features = new List<IBodyFeature>();

            int index = Features.FindIndex(x => x.Name == sc.Name);

            if (index >= 0) // got before, replace as we want a newer docking to be in there
                Features[index]  = sc;
            else
                Features.Add(sc);
        }


        public void DumpTree(string bid, int level)
        {
            if (level > 0)      // 0 is system level dump
            {
                if (BodyID < 0)
                    bid += ".??";
                else
                    bid += $".{BodyID,2}";
            }

            Dump(bid, level);

            foreach (var x in ChildBodies)
                x.DumpTree(bid, level + 1);
        }

        public void Dump(string bid, int level = 0)
        {
            string front = bid.PadRight(15);
            string names = (new string(' ', level) + (OwnName + " | " + (CanonicalName ?? "-"))).PadRight(40);
            string pad = new string(' ', front.Length + 3 + level + 3);

            System.Diagnostics.Debug.WriteLine($"{front} : {names} {BodyType.ToString().PadRight(15)} scname:`{Scan?.BodyName}` P:{Scan?.ParentList()} sma {SMA} isMapped {IsMapped} Effmap {WasMappedEfficiently}");
        //    foreach (var x in CodexEntries.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}CX:{x.GetInfo()}");
        //    foreach (var x in Signals.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}S:{x.Type_Localised ?? x.Type} {x.Count}");
        //    foreach (var x in Organics.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}O:{x.GetInfo()}");
        //    foreach (var x in Genuses.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}G:{x.Genus_Localised ?? x.Genus}");
        //    foreach (var x in FSSSignalList.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}FSS:{x.SignalName_Localised ?? x.SignalName} {x.USSType}");
        //    foreach (var x in Features.EmptyIfNull())
        //        System.Diagnostics.Debug.WriteLine($"{pad}F:{x.BodyType} `{x.Name_Localised ?? x.Name}` {x.BodyID}");
        }

        #endregion

        #region Body Nodes Pointers - used to exclude barycentres without distrubing the main tree

        public class NodePtr
        {
            public BodyNode BodyNode { get; set; }          // pointer to node
            public List<NodePtr> ChildBodies { get; set; } = new List<NodePtr>();   // to its child bodies
            public NodePtr Parent { get; set; }         // and its parent
        }

        // GO down tree and remove all barycentres, return BodyNodePtr tree
        public NodePtr BodiesNoBarycentres()
        {
            return BodiesNoBarycentres(this, null);
        }

        // GO down tree and remove all barycentres, return BodyNodePtr tree
        public static NodePtr BodiesNoBarycentres(BodyNode bn, NodePtr parent)
        {
            var x = new NodePtr();
            x.BodyNode = bn;
            x.Parent = parent;
            foreach (var bnc in bn.ChildBodies)
            {
                var bp = BodiesNoBarycentres(bnc, x);

                if (bn.BodyType == BodyNode.BodyClass.Barycentre && x.Parent != null)      // if this is a BC, and we have a parent, move the items up 1 level to the parent
                    x.Parent.ChildBodies.Add(bp);
                else if (bp.BodyNode.BodyType != BodyNode.BodyClass.Barycentre)        // exclude all BCs from adds
                    x.ChildBodies.Add(bp);
            }

            return x;
        }

        public static void Dump(NodePtr p, string bid, int level = 0)
        {
            //if (p.BodyNode.BodyType != BodyNode.BodyClass.System)
            {
                if (p.BodyNode.BodyID < 0)
                    bid += ".??";
                else
                    bid += $".{p.BodyNode.BodyID,2}";

                p.BodyNode.Dump(bid, level);
            }

            foreach (var x in p.ChildBodies)
            {
                Dump(x, bid, level + 1);
            }

        }

        #endregion


    }
}
