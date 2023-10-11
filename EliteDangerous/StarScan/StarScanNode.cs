/*
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
            star,            // used for top level stars - stars around stars are called a body.
            barycentre,      // only used for top level barycentres (AB)
            body,            // all levels >0 except for below are called a body
            belt,            // used on level 1 under the star : HIP 23759 A Belt Cluster 4 -> elements Main Star,A Belt,Cluster 4
            beltcluster,     // each cluster under it gets this name at level 2
            ring             // rings at the last level : Selkana 9 A Ring : MainStar,9,A Ring
        };

        [DebuggerDisplay("SN {FullName} {NodeType} lv {Level} bid {BodyID}")]
        public partial class ScanNode
        {
            public ScanNodeType NodeType { get; internal set; }
            public string FullName{ get; private set; }                 // full name including star system
            public string OwnName{ get; private set; }                  // own name excluding star system
            public string CustomName{ get; internal set; }               // if we can extract from the journals a custom name of it, this holds it. Mostly null
            public SortedList<string, ScanNode> Children{ get; private set; }   // kids, may ne null
            public ScanNode Parent{ get; private set; }                 // Parent of this node 
            public SystemNode SystemNode{ get; private set; }           // the system node the entry is associated with
            public int Level{ get; private set; }                       // level within SystemNode
            public int? BodyID{ get; internal set; }
            public bool IsMapped{ get; private set; }                   // recorded here since the scan data can be replaced by a better version later.
            public bool WasMappedEfficiently{ get; private set; }
            public SystemSource DataSource{ get; private set; }         // where did it come from ? FromJournal, etc
            public JournalScan ScanData { get; private set; }            // can be null if no scan, its a place holder, else its a journal scan
            // did Web create the node? JournalScan sets it to ScanData.WebsourcedBody.
            public bool WebCreatedNode { get { return DataSource == SystemSource.FromEDSM || DataSource == SystemSource.FromSpansh; } }       

            public string CustomNameOrOwnname { get { return CustomName ?? OwnName; } }

            public JournalScan.StarPlanetRing BeltData { get; internal set; }    // can be null if not belt. if its a type==belt, it is populated with belt data
            public List<JournalSAASignalsFound.SAASignal> Signals { get; internal set; } // can be null if no signals for this node, else its a list of signals.
            public List<JournalSAASignalsFound.SAAGenus> Genuses { get; internal set; } // can be null if no genusus for this node, else its a list of genus
            public List<JournalScanOrganic> Organics { get; internal set; }  // can be null if nothing for this node, else a list of organics
            public List<IBodyFeature> SurfaceFeatures { get; internal set; }   // can be null if nothing for this node, else a list of journal entries

            public ScanNode() { }
            public ScanNode(string name, ScanNodeType node, int? bodyid) 
            { 
                FullName = OwnName = name; NodeType = node; BodyID = bodyid;  Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>()); 
            }
            public ScanNode(ScanNode other) // copy constructor, but not children
            {
                NodeType = other.NodeType; FullName = other.FullName; OwnName = other.OwnName; CustomName = other.CustomName;
                Level = other.Level; BodyID = other.BodyID; IsMapped = other.IsMapped; WasMappedEfficiently = other.WasMappedEfficiently;
                ScanData = other.ScanData; BeltData = other.BeltData; Signals = other.Signals; Organics = other.Organics;
                Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());
            }
            public ScanNode(string ownname, string fullname, ScanNodeType nodetype, int level, ScanNode parent, SystemNode sysn, SystemSource datasource)
            {
                OwnName = ownname; FullName = fullname; NodeType = nodetype; Level = level; Parent = parent; SystemNode = sysn; DataSource = datasource;
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
                Signals = oldnode.Signals;
                Genuses = oldnode.Genuses;
                Organics = oldnode.Organics;
                DataSource = oldnode.DataSource;      // we copy the creation flag
            }

            internal void OverrideDataSourceToJournal()
            {
                DataSource = SystemSource.FromJournal;
            }

            internal void SetScanDataIfBetter(JournalScan value)
            {
                if ( value != null )
                {
                    if ( ScanData == null || ((!value.IsWebSourced && value.ScanType != "Basic") || ScanData.ScanType == "Basic"))
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

            public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
            public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
            public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
            public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
            public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
            public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
            public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }

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
                if (WebCreatedNode)        // its web created, so answer is false
                    return false;

                if (Children != null)
                {
                    foreach (KeyValuePair<string, ScanNode> csn in Children)
                    {
                        if (csn.Value.DoesNodeHaveNonWebScansBelow() == true)  // we do have non web ones under this child
                            return true;
                    }
                }

                return true;
            }

            public bool IsBodyInFilter(string[] filternames, bool checkchildren)
            {
                if (IsBodyInFilter(filternames))
                    return true;

                if (checkchildren)
                {
                    foreach (var body in Descendants)
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
                    if (NodeType == ScanNodeType.star)
                        name = ScanData.StarTypeID.ToString();
                    else if (NodeType == ScanNodeType.body)
                        name = ScanData.PlanetTypeID.ToString();
                }

                return filternames.Contains(name, StringComparer.InvariantCultureIgnoreCase);
            }

            public IEnumerable<ScanNode> Descendants
            {
                get
                {
                    if (Children != null)                                   
                    {       
                        foreach (ScanNode sn in Children.Values)            
                        {
                            yield return sn;

                            foreach (ScanNode c in sn.Descendants)          // recurse back up to go as deep as required
                            {
                                yield return c;
                            }
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

        };
    }
}
