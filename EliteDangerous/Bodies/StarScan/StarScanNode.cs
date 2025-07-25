﻿/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public enum ScanNodeType 
        {  
            toplevelstar,       // used for top level stars - stars around stars are called a body.
            barycentre,         // only used for top level barycentres (AB)
            planetmoonsubstar,  // all levels >0 except for below are called a body
            belt,            // used on level 1 under the star : HIP 23759 A Belt Cluster 4 -> elements Main Star,A Belt,Cluster 4
            beltcluster,     // each cluster under it gets this name at level 2
            ring             // rings at the last level : Selkana 9 A Ring : MainStar,9,A Ring
        };

        [DebuggerDisplay("SN {FullName} {NodeType} lv {Level} bid {BodyID}")]
        public partial class ScanNode
        {
            public ScanNodeType NodeType { get; internal set; }
            public string BodyDesignator { get; private set; }          // full name including star system. Such as Eowyg Auscs FG-Y d34 or Sol 3
            public string OwnName { get; private set; }                 // Just the name of the body under the parent, so "2" or "a" for 2 a etc. used for child index. Not the Elite custom name (Earth)
            // name of the body in the system (2 A)
            public string SystemBodyName { get { return BodyDesignator.Length > SystemNode.System.Name.Length + 1 ? BodyDesignator.Substring(SystemNode.System.Name.Length + 1) : BodyDesignator; } }
            public string BodyName { get; internal set; }               // if we can extract from the journals a custom name of it, this holds it. Mostly null. Named bodies use this (earth)
            public SortedList<string, ScanNode> Children{ get; private set; }   // kids, may ne null, indexed by OwnName
            public ScanNode Parent{ get; private set; }                 // Parent of this node 
            public SystemNode SystemNode{ get; private set; }           // the system node the entry is associated with
            public ScanNode ParentStar { get { return FindStarNode(this); } }
            public int Level{ get; private set; }                       // level within SystemNode
            public int? BodyID{ get; internal set; }
            public bool IsMapped{ get; private set; }                   // recorded here since the scan data can be replaced by a better version later.
            public bool WasMappedEfficiently{ get; private set; }
            public SystemSource DataSource{ get; private set; }         // where did it come from ? FromJournal, etc
            public JournalScan ScanData { get; private set; }            // can be null if no scan, its a place holder, else its a journal scan
            // did Web create the node? JournalScan sets it to ScanData.WebsourcedBody.
            public bool WebCreatedNode { get { return DataSource == SystemSource.FromEDSM || DataSource == SystemSource.FromSpansh; } }       

            public string BodyNameOrOwnName { get { return BodyName ?? OwnName; } }

            public JournalScan.StarPlanetRing BeltData { get; internal set; }    // can be null if not belt. if its a type==belt, it is populated with belt data
            public List<JournalSAASignalsFound.SAASignal> Signals { get; internal set; } // can be null if no signals for this node, else its a list of signals.
            public List<JournalSAASignalsFound.SAAGenus> Genuses { get; internal set; } // can be null if no genusus for this node, else its a list of genus
            public List<JournalScanOrganic> Organics { get; internal set; }  // can be null if nothing for this node, else a list of organics scans performed on the planet
            public List<IBodyFeature> SurfaceFeatures { get; internal set; }   // can be null if nothing for this node, else a list of journal entries

            public ScanNode() { }
            public ScanNode(string name, ScanNodeType node, int? bodyid) 
            { 
                BodyDesignator = OwnName = name; NodeType = node; BodyID = bodyid;  Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>()); 
            }
            public ScanNode(ScanNode other) // copy constructor, but not children
            {
                NodeType = other.NodeType; BodyDesignator = other.BodyDesignator; OwnName = other.OwnName; BodyName = other.BodyName;
                Level = other.Level; BodyID = other.BodyID; IsMapped = other.IsMapped; WasMappedEfficiently = other.WasMappedEfficiently;
                ScanData = other.ScanData; BeltData = other.BeltData; Signals = other.Signals; Organics = other.Organics;
                Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());
            }
            public ScanNode(string ownname, string bodydesignator, ScanNodeType nodetype, int level, ScanNode parent, SystemNode sysn, SystemSource datasource)
            {
                OwnName = ownname; BodyDesignator = bodydesignator; NodeType = nodetype; Level = level; Parent = parent; SystemNode = sysn; DataSource = datasource;
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
            public IBodyFeature FindSurfaceFeatureNear( double? latitude, double ?longitude, double delta = 0.1)
            {
                if (latitude.HasValue && longitude.HasValue && SurfaceFeatures != null)
                    return SurfaceFeatures.Find(x => x.HasLatLong &&
                                                           System.Math.Abs(x.Latitude.Value - latitude.Value) < delta && System.Math.Abs(x.Longitude.Value - longitude.Value) < delta);
                else
                    return null;
            }

            // does node have any non web scans (or empty scans) below it
            public bool DoesNodeHaveNonWebScansBelow()
            {
                if (Children != null)
                {
                    foreach (KeyValuePair<string, ScanNode> csn in Children)
                    {
                        if (csn.Value.DoesNodeHaveNonWebScansBelow() == true)  // we do have non web ones under this child
                            return true;
                    }
                }

                return WebCreatedNode == false;
            }

            public bool IsBodyInFilter(string[] filternames, bool checkchildren)
            {
                if (IsBodyInFilter(filternames))
                    return true;

                if (checkchildren)
                {
                    foreach (var body in Bodies())
                    {
                        if (body.IsBodyInFilter(filternames))
                            return true;
                    }
                }
                return false;
            }

            public bool IsBodyInFilter(string[] filternames)    // stars/bodies use the xID type, others use the type
            {
                if (filternames.Contains("All"))
                    return true;
                string name = NodeType.ToString();      // star etc..
                if (ScanData != null)
                {
                    if (NodeType == ScanNodeType.toplevelstar)
                        name = ScanData.StarTypeID.ToString();
                    else if (NodeType == ScanNodeType.planetmoonsubstar)
                        name = ScanData.IsStar ? ScanData.StarTypeID.ToString() : ScanData.PlanetTypeID.ToString();
                }

                return filternames.Contains(name, StringComparer.InvariantCultureIgnoreCase);
            }

            public IEnumerable<ScanNode> Bodies()
            {
                if (Children != null)                                   
                {       
                    foreach (ScanNode sn in Children.Values)            
                    {
                        yield return sn;

                        foreach (ScanNode c in sn.Bodies())          // recurse back up to go as deep as required
                        {
                            yield return c;
                        }
                    }
                }
            }

            // Using info here, and the indicated journal scan node, return suryeyor info on this node
            public string SurveyorInfoLine(ISystem sys, bool showsignals, bool showorganics, bool showvolcanism, bool showvalues, bool shortinfo, bool showGravity, bool showAtmos, bool showTemp, bool showRings,
                            int lowRadiusLimit, int largeRadiusLimit, double eccentricityLimit)
            {
                if (ScanData != null)
                {
                    bool hasthargoidsignals = Signals?.Find(x => x.IsThargoid) != null && showsignals;
                    bool hasguardiansignals = Signals?.Find(x => x.IsGuardian) != null && showsignals;
                    bool hashumansignals = Signals?.Find(x => x.IsHuman) != null && showsignals;
                    bool hasothersignals = Signals?.Find(x => x.IsOther) != null && showsignals;
                    bool hasminingsignals = Signals?.Find(x => x.IsUncategorised) != null && showsignals;
                    bool hasgeosignals = Signals?.Find(x => x.IsGeo) != null && showsignals;
                    bool hasbiosignals = Signals?.Find(x => x.IsBio) != null && showsignals;
                    bool hasscanorganics = Organics != null && showorganics;

                    return ScanData.SurveyorInfoLine(sys, hasminingsignals, hasgeosignals, hasbiosignals,
                                hasthargoidsignals, hasguardiansignals, hashumansignals, hasothersignals, hasscanorganics,
                                showvolcanism, showvalues, shortinfo, showGravity, showAtmos, showTemp, showRings,
                                lowRadiusLimit,largeRadiusLimit, eccentricityLimit);
                }
                else
                    return string.Empty;
            }


            private ScanNode FindStarNode(ScanNode p)
            {
                if (p.Parent == null)
                    return p;
                else
                    return FindStarNode(p.Parent);
            }

            internal SortedList<string, ScanNode> MakeChildList()
            {
                Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());
                return Children;
            }

            internal void CopyExternalDataFromOldNode(ScanNode oldnode)
            {
                IsMapped = oldnode.IsMapped;
                WasMappedEfficiently = oldnode.WasMappedEfficiently;
                if (oldnode.Signals != null)
                {
                    if (Signals == null)
                        Signals = new List<JournalSAASignalsFound.SAASignal>();
                    Signals.AddRange(oldnode.Signals);
                }
                if (oldnode.Genuses != null)
                {
                    if (Genuses == null)
                        Genuses = new List<JournalSAASignalsFound.SAAGenus>();
                    Genuses.AddRange(oldnode.Genuses);
                }
                if (oldnode.Organics != null)
                {
                    if (Organics == null)
                        Organics = new List<JournalScanOrganic>();
                    Organics.AddRange(oldnode.Organics);
                }
                if (oldnode.SurfaceFeatures != null)
                {
                    if (SurfaceFeatures == null)
                        SurfaceFeatures = new List<IBodyFeature>();
                    SurfaceFeatures.AddRange(oldnode.SurfaceFeatures);
                }
                DataSource = oldnode.DataSource;      // we copy the creation flag
            }

            internal void OverrideDataSourceToJournal()
            {
                DataSource = SystemSource.FromJournal;
            }

            internal void SetScanDataIfBetter(JournalScan value)
            {
                if (value != null)
                {
                    if (ScanData == null || ((!value.IsWebSourced && value.ScanType != "Basic") || ScanData.ScanType == "Basic"))
                        ScanData = value;
                }
            }

            internal void SetMapped(bool efficently)
            {
                IsMapped = true; WasMappedEfficiently = efficently;
            }

            internal void CopyChildren(ScanNode other)
            {
                Children = other.Children;
            }

        };
    }
}
