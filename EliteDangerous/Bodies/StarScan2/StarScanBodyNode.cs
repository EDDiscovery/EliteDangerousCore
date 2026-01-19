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

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("{BodyID} `{OwnName}` `{CanonicalName}` {BodyType} : CB{ChildBodies.Count} Cx{CodexEntries?.Count} FSS{FSSSignalList?.Count} Org{Organics?.Count} Gen {Genuses?.Count} Sig {Signals?.Count}")]
    public partial class BodyNode
    {
        #region Public Interface

        // own name (1,2,A,Mitterand Hollow) for scan names, or computed Barycentre name BC of A,B,C
        public string OwnName { get; private set; }     

        // set on scans to js.BodyName, and on discrete body adds of names (Miterrand Hollow)
        // will contain the whole name (HIP 1885 A 5 b, HIP 1885 A 2 A Ring, HIP 1885 A A Belt Cluster 1)
        // will not be set on sub part names/barycentres. Be Null
        public string CanonicalName { get; private set; }  
                                                           
        // always not null
        public string CanonicalNameOrOwnName => CanonicalName ?? OwnName;

        // without system name, may be null
        public string CanonicalNameNoSystemName() { return CanonicalName?.ReplaceIfStartsWith(SystemNode.System.Name); }

        // best name, either Canonicalname without system, or ownname, never null
        public string Name() { return CanonicalName?.ReplaceIfStartsWith(SystemNode.System.Name) ?? OwnName; }

        // set if a standard name to name depth. -1 if non standard naming, else 0 are top level objects, 1 = planet, etc
        public int CanonicalNameDepth { get; private set; } = -1;       

        public int BodyID { get; private set; }

        public bool PlacedWithoutParents { get; set; } = false;

        // we share the bodytype with frontier, even though their names are a bit wierd
        // the root node is called System and is held in StarScanSystemNode
        public BodyDefinitions.BodyType BodyType { get; private set; }

        public bool IsStar { get { return BodyType == BodyDefinitions.BodyType.Star; } }
        public bool IsStarOrPlanet { get { return BodyType == BodyDefinitions.BodyType.Star || BodyType == BodyDefinitions.BodyType.Planet; } }
        public bool IsPlanetOrMoon { get { return BodyType == BodyDefinitions.BodyType.Planet; } }
        public bool IsBarycentre { get { return BodyType == BodyDefinitions.BodyType.Barycentre; } }
        public bool IsBodyOrbitingStar { get { return BodyType == BodyDefinitions.BodyType.Planet || BodyType == BodyDefinitions.BodyType.StellarRing || BodyType == BodyDefinitions.BodyType.SmallBody; } }

        public bool WebCreatedNode { get { return Scan?.DataSource == SystemSource.FromEDSM || Scan?.DataSource == SystemSource.FromSpansh; } }

        // list of child bodies, sorted
        public List<BodyNode> ChildBodies { get; private set; } = new List<BodyNode>();
        public BodyNode Parent { get; private set; } = null;            // null if bodytype=system, else always set
        public SystemNode SystemNode { get; private set; } = null;      // null if bodytype=system, else always set

        // Data on body
        public JournalScan Scan { get; private set; } = null;                           // type Star/Planet/AsteroidCluster/PlanetaryRing
        public JournalScanBaryCentre BarycentreScan { get; private set; } = null;       // type Barycentre
        public StarPlanetRing BeltData { get; private set; } = null;                    // type AsteroidCluster or PlanetaryRing

        public double? SMA { get { return Scan != null ? Scan.nSemiMajorAxis : BarycentreScan != null ? BarycentreScan.SemiMajorAxis : BeltData != null ? BeltData.InnerRad : default(double?); } }

        public List<JournalSAASignalsFound.SAASignal> Signals { get; private set; } = null;
        public List<JournalSAASignalsFound.SAAGenus> Genuses { get; private set; } = null;
        public List<JournalScanOrganic> Organics { get; internal set; } = null;
        public List<IBodyFeature> Features { get; internal set; } = null;               // for SystemBodies the orbiting stations, for other bodies touchdown/settlements/docking etc
        public List<JournalCodexEntry> CodexEntries { get; private set; } = null;
        public List<FSSSignal> FSSSignalList { get; private set; } = null;              // only for SystemBodies in StarScan.SystemNode
        
        public bool IsMapped { get; private set; }                                      // recorded here since the scan data can be replaced by a better version later.
        public bool WasMappedEfficiently { get; private set; }

        // is this type at the top level of the tree, no others of this type above. false if one above
        public bool IsTopLevel(BodyDefinitions.BodyType bc)
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

        // calculate name depth of body
        public int GetNameDepth()
        {
            return CanonicalNameDepth == -1 ? GetDepthIgnoreBC() : CanonicalNameDepth;        // 0 is first
        }

        // calculate depth of body with no BCs counting
        public int GetDepthIgnoreBC()
        {
            int depth = 0;
            BodyNode bn = Parent;
            while (bn != null && bn.BodyType != BodyDefinitions.BodyType.System)
            {
                if (bn.BodyType != BodyDefinitions.BodyType.Barycentre)      // don't count the top level barycentre or system as depth
                    depth++;

                bn = bn.Parent;
            }
            return depth;
        }

        // find body parent above ignoring BCs
        public BodyNode GetParentIgnoreBC()
        {
            BodyNode bn = Parent;
            while (bn != null && bn.BodyType != BodyDefinitions.BodyType.System)
            {
                if (bn.BodyType != BodyDefinitions.BodyType.Barycentre)      // don't count the top level barycentre or the top system node
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

        // accumulate all child bodies excluding barycentres, and if a barycentre is a sibling, returns its child bodies
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
            return Bodies(x => (x.BodyType == BodyDefinitions.BodyType.Star || x.BodyType == BodyDefinitions.BodyType.Planet) && x.Scan != null).Count();
        }
        public int StarPlanetsScanned(bool includewebbodies)        // includewebbodies gives same total as above, false means only include ones which we have scanned
        {
            return Bodies(x => (x.BodyType == BodyDefinitions.BodyType.Star || x.BodyType == BodyDefinitions.BodyType.Planet) && x.Scan != null && (includewebbodies || !x.Scan.IsWebSourced)).Count();
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
            return Bodies(b => b.BodyType == BodyDefinitions.BodyType.StellarRing).Count();
        }
        public int BeltClusterBodies() // number of belt clusters bodies
        {
            return Bodies(b => b.BodyType == BodyDefinitions.BodyType.AsteroidCluster).Count();
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
                return Features.Find(x => x.HasLatLong && System.Math.Abs(x.Latitude.Value - latitude.Value) < delta && System.Math.Abs(x.Longitude.Value - longitude.Value) < delta);
            else
                return null;
        }


        // return all child bodies down the tree, or return all bodies matching a Predicate, with either all or just return first match
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

        // Return the surveyor info line from this node
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

        // recursively generate json dump of bodies
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

            if (top.BodyType != BodyDefinitions.BodyType.System)
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

        public const string BCNamingPrefix = "BC of ";
        public const string DefaultNameOfBC = "Unknown Barycentre";
        public const string DefaultNameOfUnknownBody = "Unknown Body";
        public const string DefaultNameOfUnknownStar = "Unknown Star";
        public const string UnknownMarker = "Unknown";
        public const int BodyIDMarkerForAutoBody = -2;
        public const int BodyIDMarkerForAutoBodyBeltCluster = -3;

        public BodyNode(string ownname, BodyDefinitions.BodyType bc, int bid, BodyNode parent, SystemNode sys, string cn = null)
        {
            OwnName = ownname;
            BodyType = bc;
            BodyID = bid;
            Parent = parent;
            SystemNode = sys;
            CanonicalName = cn;
            CalculateCNDepth();
        }

        public BodyNode(string ownname, JournalScan sc, int bid, BodyNode parent, SystemNode sys) :
            this(ownname, sc.BodyType, bid, parent, sys)
        {
        }

        public BodyNode(string ownname, BodyParent nt, int bid, BodyNode parent, SystemNode sys) :
            this(ownname, nt.IsStar? BodyDefinitions.BodyType.Star : nt.IsBarycentre? BodyDefinitions.BodyType.Barycentre : nt.IsStellarRing? BodyDefinitions.BodyType.StellarRing : BodyDefinitions.BodyType.Planet, bid, parent, sys )
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
        public void ResetBodyType(BodyDefinitions.BodyType ty)
        {
            BodyType = ty;
        }
        public void ResetParent(BodyNode p)
        {
            Parent = p;
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

            Scan.Signals = Signals;                     // we point Signals, Organics, Genuses, CodexEntries in Scan to us
            Scan.Genuses = Genuses;                     // If they are null at this point the New will set them to the scan - bug found
            Scan.Organics = Organics;                   // If these change by this code they will change in sympathy
            Scan.SurfaceFeatures = Features;    
            Scan.CodexEntries = CodexEntries;           
            
            Scan.SetMapped(IsMapped, WasMappedEfficiently);  // and we set the mapped/was mapped flag to the nodes setting set up by AddSAAScanComplete
                
            CanonicalName = Scan.BodyName;              // set the Canonical name given by frontier, and recalc the canonical name depth
            CalculateCNDepth();
        }

        public void ResetCanonicalName(string s)
        {
            CanonicalName = s;
            CalculateCNDepth();
        }

        // Extract the part names from the standard naming, recognised composite text parts like belt clusters
        static public List<string> ExtractPartsFromStandardName(string subname)
        {
            var partnames = new List<string>();

            StringParser sp = new StringParser(subname);

            while (!sp.IsEOL)
            {
                // in Scans, its called A Belt
                // other places its called A Belt Cluster
                // split both off, checking belt cluster first in case we preprocessed it

                if (sp.IsStringMoveOn(out string found, StringComparison.InvariantCultureIgnoreCase, true, "A Belt Cluster", "B Belt Cluster", "A Belt", "B Belt", "A Ring", "B Ring", "C Ring", "D Ring"))
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

        public void SetScan(JournalScanBaryCentre sc)
        {
            BarycentreScan = sc;
        }
        public void SetScan(StarPlanetRing sc)
        {
            BeltData = sc;
        }
        public void AddSignals(List<JournalSAASignalsFound.SAASignal> sc)
        {
            if (Signals == null)
            {
                Signals = new List<JournalSAASignalsFound.SAASignal>();
                if (Scan != null)               // if we have a scan, we need to point the scan node to the Signals so it can see them
                    Scan.Signals = Signals;
            }

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
            {
                Genuses = new List<JournalSAASignalsFound.SAAGenus>();
                if (Scan != null)               // if we have a scan, we need to point the scan node to the item so it can see them
                    Scan.Genuses = Genuses;
            }

            foreach (var g in sc)
            {
                int present = Genuses.FindIndex(x => x.Genus == g.Genus);
                if (present == -1)
                    Genuses.Add(g);
            }
        }

        public void AddScanOrganics(JournalScanOrganic organic)
        {
            if (Organics == null)
            {
                Organics = new List<JournalScanOrganic>();
                if (Scan != null)               // if we have a scan, we need to point the scan node to the item so it can see them
                    Scan.Organics = Organics;
            }

            Organics.Add(organic);
        }

        public bool AddFeatureOnlyIfNew(IBodyFeature sc)
        {
            if (Features == null)
            {
                Features = new List<IBodyFeature>();
                if (Scan != null)
                    Scan.SurfaceFeatures = Features;
            }

            var prev = Features.Find(x => x.Name == sc.Name);        // have we got it before?, if so, don't replace
            if (prev == null)
            {
                Features.Add(sc);
                return true;
            }
            else
                return false;
        }

        public void AddDocking(JournalDocked sc)
        {
            if (Features == null)
            {
                Features = new List<IBodyFeature>();
                if (Scan != null)
                    Scan.SurfaceFeatures = Features;
            }

            int index = Features.FindIndex(x => x.Name == sc.Name);

            if (index >= 0) // got before, replace as we want a newer docking to be in there
                Features[index] = sc;
            else
                Features.Add(sc);
        }

        public void AddCodex(JournalCodexEntry sc)
        {
            if (CodexEntries == null)
            {
                CodexEntries = new List<JournalCodexEntry>();
                if (Scan != null)     // if we have a scan, we need to point the scan node to the Signals so it can see them
                    Scan.CodexEntries = CodexEntries;
            }
            CodexEntries.Add(sc);
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
        public void SetMapped(bool efficently)
        {
            IsMapped = true; WasMappedEfficiently = efficently;
        }

        // tell me what the sort order of this vs the right one is.
        public int CompareTo(BodyNode right, bool ignoresma)
        {
            BodyNode left = this;

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

            // $"Sort Compare `{lt}` with `{rt}`".DO(lvl);

            double? smal = left.SMA;         // grab SMA from anything we have
            double? smar = right.SMA;

            if (smal.HasValue && smar.HasValue && !ignoresma)
            {
                //$"Body Compare SMA {left.OwnName} vs {right.OwnName} : {left.SMA} vs {right.SMA} ".DO();
                return smal.Value.CompareTo(smar.Value);
            }
            else 
            if (lt.Length == 1 && rt.Length == 1)      // 1-2-3 or a b c sort direct just comparing value
            {
                //$"Body Compare 1Char {left.OwnName} vs {right.OwnName} : {left.SMA} vs {right.SMA} ".DO();
                return lt.CompareTo(rt);
            }
            else
            {
                int? lv = lt.InvariantParseIntNull();
                int? rv = rt.InvariantParseIntNull();
                if (lv.HasValue && rv.HasValue)
                {
                    //$"Body Compare Number {left.OwnName} vs {right.OwnName} : {left.SMA} vs {right.SMA} ".DO();
                    return lv.Value.CompareTo(rv.Value);
                }
                else
                {
                    //$"Body Compare Other {left.OwnName} vs {right.OwnName} : {left.SMA} vs {right.SMA} ".DO();
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
            string front = bid.PadRight(20);
            string names = (new string(' ', level) + (OwnName + " | " + (CanonicalNameNoSystemName() ?? "-"))).PadRight(35);
            string pad = new string(' ', front.Length + 3 + level + 3);
            int cn = GetNameDepth();
            string cnl = cn >= 0 ? cn.ToString() : "";
            string sma = SMA != null ? ((SMA / 1000.0).ToStringInvariant("N0") +"km") : "";

            System.Diagnostics.Trace.WriteLine($"{front} : {cnl.PadRight(2)} : {names} : {BodyType.ToString().PadRight(15)} {(PlacedWithoutParents?"PWP":"")} sma {sma}");
            foreach (var x in CodexEntries.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}CX:{x.GetInfo()}");
            foreach (var x in Signals.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}S:{x.Type_Localised ?? x.Type} {x.Count}");
            foreach (var x in Organics.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}O:{x.GetInfo()}");
            foreach (var x in Genuses.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}G:{x.Genus_Localised ?? x.Genus}");
            foreach (var x in FSSSignalList.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}FSS:{x.SignalName_Localised ?? x.SignalName} {x.USSType}");
            foreach (var x in Features.EmptyIfNull())
                System.Diagnostics.Trace.WriteLine($"{pad}F:{x.BodyType} `{x.Name_Localised ?? x.Name}` {x.BodyID}");
        }

        #endregion

        #region Helpers

        // We calculate for Eahlstan the name depth of the object if its a standard naming part!
        private void CalculateCNDepth()
        {
            CanonicalNameDepth = -1;

            bool stdname = CanonicalName?.StartsWith(SystemNode.System.Name) ?? false;          // if Canonical name == null, its false as well

            if (stdname)
            {
                if (CanonicalName == SystemNode.System.Name)        // if same name, its a level 0 star
                {
                    CanonicalNameDepth = 0;
                }
                else
                {
                    // split, into A 1 a etc, accounting for belt cluster ring combined names
                    var parts = ExtractPartsFromStandardName(CanonicalName.Substring(SystemNode.System.Name.Length));


                    if (parts.Count > 0)        // protect against nonsense
                    {
                        // if it starts with a star or barycentre, reduce count by 1

                        bool isstar = parts[0].Length == 1 && char.IsLetter(parts[0][0]);       // see StarScanSystemNodeImpt: GetOrMakeStandardBodyNodeFromScanWithoutParents
                        bool isbarycentre = parts[0].Length > 1 && parts[0].HasAll(x => char.IsUpper(x));

                        if (isstar || isbarycentre)
                            CanonicalNameDepth = parts.Count - 1;
                        else
                            CanonicalNameDepth = parts.Count;
                    }
                }
            }
        }

        #endregion
    }
}
