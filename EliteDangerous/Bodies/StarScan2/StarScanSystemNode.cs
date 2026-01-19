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
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("{System.Name} {System.SystemAddress} Top level bodies {systemBodies.ChildBodies.Count}")]
    public partial class SystemNode
    {
        #region Public
        public ISystem System { get; private set; }     // may not have XYZ, will always have name and systemaddress
        public bool HasCentreBarycentre { get { return systemBodies.ChildBodies.Count == 1 && systemBodies.ChildBodies[0].BodyType == BodyDefinitions.BodyType.Barycentre; } }
        public EDStar StarClass { get; private set; }   // set by navroute and by startjump
        public int? FSSTotalBodies { get; private set; }         // if we have FSSDiscoveryScan, this will be set
        public int? FSSTotalNonBodies { get; private set; }     // if we have FSSDiscoveryScan, this will be set
        public bool OldScansPresent { get; set; }               // if set, we have old scans
        public int BarycentreScans { get; set; }                 // accumulated barycentre scans, used for debug purposes to know we have stored a barycentre Scan in the tree
        public uint BodyGeneration { get; private set; } = 0;        // changed after each body change
        public uint SignalGeneration { get; private set; } = 0;        // changed after each codex/signal/fss/genus/etc change
        public List<FSSSignal> FSSSignals { get { return systemBodies.FSSSignalList; } }     // may be null, held in top level body
        public List<JournalCodexEntry> CodexEntries { get { return systemBodies.CodexEntries; } }     // System Codex entries, may be null, held in top level body. Codex entries can also be against a body
        public List<IBodyFeature> OrbitingStations { get { return systemBodies.Features; } }     // System orbiting stations, may be null, held in top level body.

        // top level, whole tree
        public BodyNode TopLevel() { return systemBodies; }

        // Top level body list, ignoring the central barycentre
        public BodyNode TopLevelBody() { return systemBodies.ChildBodies.Count == 1 && systemBodies.ChildBodies[0].BodyType == BodyDefinitions.BodyType.Barycentre ? systemBodies.ChildBodies[0] : systemBodies; }

        public SystemNode(ISystem sys)
        {
            $"StarScan Make new System Node {sys.Name}:{sys.SystemAddress}".DO(debugid);
            System = sys;
            Clear();
        }

        public void Clear()
        {
            systemBodies = new BodyNode("System", BodyDefinitions.BodyType.System, -1, null, null);
            bodybyid = new Dictionary<int, BodyNode>();
            FSSTotalBodies = FSSTotalNonBodies = null;
            BodyGeneration = 0;
        }

        // Find body, various ways

        public BodyNode FindBody(int id)
        {
            bodybyid.TryGetValue(id, out BodyNode value);
            return value;
        }
        public bool TryGetBody(int id, out BodyNode v)
        {
            return bodybyid.TryGetValue(id, out v);
        }

        public BodyNode FindCanonicalBodyName(string fdname)        // matches HIP 1885 A 5 b, HIP 1885 A 2 A Ring etc
        {
            return Bodies(x => x.CanonicalName.EqualsIIC(fdname), true).FirstOrDefault();
        }

        // bt can be PlantaryOrStellarRing and it will match both
        public BodyNode FindCanonicalBodyNameType(string fdname, BodyDefinitions.BodyType bt)        
        {
            return Bodies(x => x.CanonicalName.EqualsIIC(fdname) && x.BodyType == bt, true).FirstOrDefault();
        }
        // bt can be PlantaryOrStellarRing and it will match both, only matches if it was made without parents
        public BodyNode FindCanonicalMisplacedBodyNameType(string fdname, BodyDefinitions.BodyType bt)       
        {
            return Bodies(x => x.CanonicalName.EqualsIIC(fdname) && x.BodyType == bt && x.PlacedWithoutParents==true, true).FirstOrDefault();
        }
        public BodyNode FindCanonicalBodyNameWithWithoutSystem(string fdname)       // matches HIP 1885 A 5 b, A 5 b, HIP 1885 A 2 A Ring, A 2 A Ring etc
        {
            return Bodies(x => x.CanonicalName?.EqualsIIC(fdname) == true || x.CanonicalName?.ReplaceIfStartsWith(System.Name).EqualsIIC(fdname) == true, true).FirstOrDefault();
        }

        // return all bodies, or return all bodies matching a Predicate, or return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            return systemBodies.Bodies(find, stoponfind);
        }

        // Go down tree and remove all barycentres, return NodePtr tree
        public NodePtr BodiesNoBarycentres()
        {
            return NodePtr.BodiesNoBarycentres(systemBodies, null);
        }

        // Go down tree and remove all barycentres, fix other stuff, return NodePtr tree
        public NodePtr BodiesSimplified(bool simplify = true)
        {
            NodePtr topnode = NodePtr.Bodies(TopLevelBody(), null);

            if (simplify)
            {
                //nodelist.Dump("", 0);

                var movelist = new List<Tuple<NodePtr, NodePtr>>();

                // merge all crazy frontier barycentres with the same names together

                // distinct barycentre names
                var barycentres = topnode.ChildBodies.Where(x => x.BodyNode.IsBarycentre).GroupBy(y=>y.BodyNode.OwnName).ToList();

                // every distinct bary name..
                foreach( var bary in barycentres )
                {
                    int i = 0;
                    foreach (var node in bary)      // every bary node with this bary.Key name
                    {
                        if (i++ > 0)                // second on gets merged into first
                        {
                            foreach(var child in node.ChildBodies)
                                movelist.Add(new Tuple<NodePtr, NodePtr>(child, bary.First()));
                        }
                    }

                    // any orbiting bodies with the bary name.. frontier stuff is so weird

                    var orbitingbodies = topnode.ChildBodies.Where(x => x.BodyNode.IsBodyOrbitingStar && x.BodyNode.CanonicalNameNoSystemName()?.StartsWith(bary.Key) == true).ToList();       // these should not be directly under the primary barycentre

                    foreach( var node in orbitingbodies)
                        movelist.Add(new Tuple<NodePtr, NodePtr>(node, bary.First()));
                }

                NodePtr.Move(movelist);
                movelist.Clear();

                // everything we give a name depth of 0 and is a star to must be at the top..

                foreach (var np in topnode.Bodies())
                {
                    if (np.BodyNode.IsStar && np.BodyNode.GetNameDepth() == 0 && np.Parent != topnode)
                    {
                        movelist.Add(new Tuple<NodePtr, NodePtr>(np, topnode));
                    }
                }

                NodePtr.Move(movelist);

                topnode.ChildBodies.RemoveAll(y => y.ChildBodies.Count == 0 && y.BodyNode.IsBarycentre);        // empty barycentre clean up

                NodePtr.Sort(topnode, true);

                //nodelist.Dump("", 0);
            }

            return topnode;
        }

        public long ScanValue(bool includewebvalue)
        {
            return systemBodies.ScanValue(includewebvalue);
        }

        public string StarTypesScanned(bool bracketit = true, bool longform = false)
        {
            var sortedset = Bodies(x => x.Scan?.IsStar == true).OrderBy(x => x.Scan.DistanceFromArrivalLS).Select(x => longform ? x.Scan.StarTypeText : x.Scan.StarClassificationAbv).ToList();
            string s = string.Join("; ", sortedset);
            if (bracketit && s.HasChars())
                s = "(" + s + ")";
            return s;
        }

        public int StarPlanetsScanned(bool includewebbodies)        // includewebbodies gives same total as above, false means only include ones which we have scanned
        {
            return systemBodies.StarPlanetsScanned(includewebbodies);
        }

        public int StarsScanned()      // stars scanned
        {
            return systemBodies.StarsScanned();
        }
        public IEnumerable<BodyNode> GetStarsScanned()
        {
            return systemBodies.GetStarsScanned();
        }
        public int PlanetsWithScan()      // planets scanned
        {
            return systemBodies.PlanetsWithScan();
        }
        public int BeltClusters() // number of belt clusters
        {
            return systemBodies.BeltClusters();
        }
        public int BeltClusterBodies() // number of belt clusters bodies
        {
            return systemBodies.BeltClusterBodies();
        }

        // All codex entries across all bodies
        public List<JournalCodexEntry> Codexes()
        {
            var list = new List<JournalCodexEntry>();
            var codex = Bodies(x => x.CodexEntries != null).Select(x => x.CodexEntries);
            foreach (var c in codex)
                list.AddRange(c);
            return list;
        }

        public IBodyFeature GetFeature(string feature)
        {
            IBodyFeature sysfeature = OrbitingStations?.Find(x => x.Name.EqualsIIC(feature));
            if (sysfeature != null)
                return sysfeature;

            foreach (var b in Bodies())
            {
                IBodyFeature f = b.Features?.Find(x => x.Name.EqualsIIC(feature) || x.Name_Localised.EqualsIIC(feature));
                if (f != null)
                    return f;
            }

            return null;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["System"] = System.Name;
            if (System.SystemAddress != null)
            {
                json["Address"] = System.SystemAddress.Value;
            }
            json["FSSTotal"] = FSSTotalBodies;
            json["FSSNonBodies"] = FSSTotalNonBodies;
            BodyNode.ToJSON(json, TopLevel());
            return json;
        }

        #endregion

        #region Implementation

        // called if detected a system name change
        public void RenamedSystem(ISystem sys)
        {
            System = sys;
        }

        #endregion
    }
}
