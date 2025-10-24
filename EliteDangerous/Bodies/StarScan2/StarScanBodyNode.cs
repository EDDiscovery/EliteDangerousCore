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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using static EliteDangerousCore.StarScan;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("`{OwnName}` `{FDName}` {BodyID} {BodyType} : CB{ChildBodies.Count} Cx{CodexEntries?.Count} FSS{FSSSignalList?.Count} Org{Organics?.Count} Gen {Genuses?.Count}")]

    public partial class BodyNode
    {
        // public string BodyName { get; private set; }
        public string OwnName { get; private set; }
        public string FDName { get; private set; }
        public int BodyID { get; private set; }

        public enum BodyClass { System,             // top level Bodies object in SystemNode
                                Star,               // a top level star, or a substar
                                PlanetMoon,         // a planet or moon
                                Barycentre,         // a barycentre ('Null' type in parents array)
                                BeltCluster,        // a 'A Belt Cluster' name (`Ring` type in the parents array) - has BeltClusterBody underneath it
                                                    // also from the journalscan of a star, the rings/belts of the star broken out into children. BeltData is set. See ProcessBelts.      
                                BeltClusterBody,    // items under a belt cluster
                                PlanetaryRing,      // Called "A Ring" or "B Ring", from a journalscan of the ring (Scan will be set), or from the journalscan of a planet with its ring structure broken into children, BeltData is set.
                              
                            };
        public BodyClass BodyType { get; private set; }

        // tbd need to set
        public SystemSource DataSource { get; private set; } = SystemSource.FromJournal; // where did it come from ? FromJournal, etc
        public bool WebCreatedNode { get { return DataSource == SystemSource.FromEDSM || DataSource == SystemSource.FromSpansh; } }

        public List<BodyNode> ChildBodies { get; private set; } = new List<BodyNode>();

        public BodyNode Parent { get; private set; } = null;            // null if top level body

        public JournalScan Scan { get; private set; } = null;                           // type Star/PlanetMoon/BeltClusterBody/PlanetaryRing
        public JournalScanBaryCentre BarycentreScan { get; private set; } = null;       // type Barycentre
        public JournalScan.StarPlanetRing BeltData { get; private set; } = null;        // type StellarBelts
        public double? SMA { get { return Scan != null ? Scan.nSemiMajorAxis : BarycentreScan != null ? BarycentreScan.SemiMajorAxis : BeltData != null ? BeltData.InnerRad : default(double?); } }

        public List<JournalCodexEntry> CodexEntries { get; private set; } = null;
        public List<JournalSAASignalsFound.SAASignal> Signals { get; private set; } = null;
        public List<JournalSAASignalsFound.SAAGenus> Genuses { get; private set; } = null;
        public List<FSSSignal> FSSSignalList { get; private set; } = null;       // only for SystemBody in StarScanSystemNode
        public List<JournalScanOrganic> Organics { get; internal set; } = null;
        public List<IBodyFeature> SurfaceFeatures { get; internal set; } = null;
        public bool IsMapped { get; private set; }                   // recorded here since the scan data can be replaced by a better version later.
        public bool WasMappedEfficiently { get; private set; }
        public BodyNode(string ownname, string fdname, BodyClass bc, int bid, BodyNode parent)
        {
            OwnName = ownname;
            FDName = fdname;
            BodyType = bc;
            BodyID = bid;
            Parent = parent;
        }

        public BodyNode(string ownname, string fdname, JournalScan.BodyParent nt, int bid, BodyNode parent)
        {
            OwnName = ownname;
            FDName = fdname;
            BodyType = nt.IsStar ? BodyNode.BodyClass.Star : nt.IsBarycentre ? BodyNode.BodyClass.Barycentre : nt.IsRing ? BodyNode.BodyClass.BeltCluster : BodyNode.BodyClass.PlanetMoon;
            BodyID = bid;
            Parent = parent;
        }

        public void ResetBodyID(int bid)
        {
            BodyID = bid;
        }

        public void ResetBodyName(string name, string fdname)
        {
            OwnName = name;
            FDName = fdname;
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
            nd.SurfaceFeatures = SurfaceFeatures;
            nd.DataSource = DataSource;
            nd.IsMapped = IsMapped;
            nd.WasMappedEfficiently = WasMappedEfficiently;
        }

        public void SetScan(JournalScan sc)
        {
            Scan = sc;
        }
        public void SetScan(JournalScanBaryCentre sc)
        {
            BarycentreScan = sc;
        }
        public void SetScan(JournalScan.StarPlanetRing sc)
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

        public void AddSurfaceFeature(IBodyFeature sc)
        {
            if (SurfaceFeatures == null)
                SurfaceFeatures = new List<IBodyFeature>();

            var prev = SurfaceFeatures.Find(x => x.Name == sc.Name);        // have we got it before?, if so, don't replace
            if (prev == null)   
                SurfaceFeatures.Add(sc);
        }

        public void SetMapped(bool efficently)
        {
            IsMapped = true; WasMappedEfficiently = efficently;
        }

        public void AddDocking(JournalDocked sc)
        {
            if (SurfaceFeatures == null)
                SurfaceFeatures = new List<IBodyFeature>();

            var prev = SurfaceFeatures.Find(x => x.Name == sc.Name);        // have we got it before?
            if (prev != null)
                prev = sc;
            else
                SurfaceFeatures.Add(sc);
        }

        // is this type found above - PlanetMoon/Star only
        public bool IsTopLevel(BodyClass bc)
        {
            BodyNode bn = Parent;
            while(bn != null)
            {
                if (bc == BodyClass.PlanetMoon && bn.BodyType == BodyClass.PlanetMoon)
                    return false;
                if (bc == BodyClass.Star && bn.BodyType == BodyClass.Star)
                    return false;

                bn = bn.Parent;
            }

            return true;
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
                    if (body.IsBodyTypeInFilter(filternames))
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
            return SurfaceFeatures?.Count(x => x is JournalApproachSettlement || x is JournalDocked) ?? 0;
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
            if (latitude.HasValue && longitude.HasValue && SurfaceFeatures != null)
                return SurfaceFeatures.Find(x => x.HasLatLong &&
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

                foreach (BodyNode c in sn.Bodies(find,stoponfind))          // recurse back up to go as deep as required
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

        public void DumpTree(string bid)
        {
            if (BodyType != BodyClass.System)
            {
                if (BodyID < 0)
                    bid += ".?";
                else
                    bid += $".{BodyID}";
                string pad = new string(' ', bid.Length + 3);

                System.Diagnostics.Debug.WriteLine($"{bid} : `{OwnName}` `{FDName}` {BodyType} scname:`{Scan?.BodyName}` P:{Scan?.ParentList()} sma {SMA} isMapped {IsMapped} Effmap {WasMappedEfficiently}");
                foreach (var x in CodexEntries.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}CX:{x.GetInfo()}");
                foreach (var x in Signals.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}S:{x.Type_Localised ?? x.Type} {x.Count}");
                foreach (var x in Organics.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}O:{x.GetInfo()}");
                foreach (var x in Genuses.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}G:{x.Genus_Localised ?? x.Genus}");
                foreach (var x in FSSSignalList.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}FSS:{x.SignalName_Localised ?? x.SignalName} {x.USSType}");
                foreach (var x in SurfaceFeatures.EmptyIfNull())
                    System.Diagnostics.Debug.WriteLine($"{pad}SF:{x.BodyType} {x.Name_Localised ?? x.Name}");
            }

            foreach (var x in ChildBodies) 
                x.DumpTree(bid);
        }
    }
}
