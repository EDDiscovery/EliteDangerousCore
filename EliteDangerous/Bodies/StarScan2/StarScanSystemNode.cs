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
using System.Drawing;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    [System.Diagnostics.DebuggerDisplay("{System.Name} {System.SystemAddress} Top level bodies {systemBodies.ChildBodies.Count}")]
    public partial class SystemNode
    {
        #region Public
        public ISystem System { get; private set; }     // may not have XYZ, will always have name and systemaddress
        public bool HasCentreBarycentre { get { return systemBodies.ChildBodies.Count == 1 && systemBodies.ChildBodies[0].BodyType == BodyNode.BodyClass.Barycentre; } }
        public int? FSSTotalBodies { get; private set; }         // if we have FSSDiscoveryScan, this will be set
        public int? FSSTotalNonBodies { get; private set; }     // if we have FSSDiscoveryScan, this will be set
        public bool OldScansPresent { get; set; }               // if set, we have old scans
        public int BarycentreScans { get; set; }                 // accumulated barycentre scans, used for tracking barycentre storage

        public uint BodyGeneration { get; private set; } = 0;        // changed after each body change
        public uint SignalGeneration { get; private set; } = 0;        // changed after each codex/signal/fss/genus change
        public List<FSSSignal> FSSSignals { get { return systemBodies.FSSSignalList; } }     // may be null, held in top level body
        public List<JournalCodexEntry> CodexEntries { get { return systemBodies.CodexEntries; } }     // may be null, held in top level body
        public List<IBodyFeature> OrbitingStations { get { return systemBodies.Features; } }     // may be null, held in top level body, Stations..
        public BodyNode TopLevelBody() { return systemBodies.ChildBodies.Count == 1 && systemBodies.ChildBodies[0].BodyType == BodyNode.BodyClass.Barycentre ? systemBodies.ChildBodies[0] : systemBodies; }
        public BodyNode TopLevel() { return systemBodies; }

        public SystemNode(ISystem sys)
        {
            System = sys;
          //  DebuggerHelpers.OutputControl += debugid;
        }

        public void Clear()
        {
            systemBodies = new BodyNode("System", BodyNode.BodyClass.System, -1, null, null);       // clear
            FSSTotalBodies = FSSTotalNonBodies = null;
            bodybyid.Clear();
            BodyGeneration = 0;
            $"Clear Scans of {System.SystemAddress} {System.Name}".DO(debugid);
        }

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
        public BodyNode FindCanonicalBodyNameWithWithoutSystem(string fdname)       // matches HIP 1885 A 5 b, A 5 b, HIP 1885 A 2 A Ring, A 2 A Ring etc
        {
            return Bodies(x => x.CanonicalName?.EqualsIIC(fdname) == true || x.CanonicalName?.ReplaceIfStartsWith(System.Name).EqualsIIC(fdname) == true, true).FirstOrDefault();
        }

        // return all bodies, or return all bodies matching a Predicate, or return first match
        public IEnumerable<BodyNode> Bodies(Predicate<BodyNode> find = null, bool stoponfind = false)
        {
            return systemBodies.Bodies(find, stoponfind);
        }

        // GO down tree and remove all barycentres, return NodePtr tree
        public BodyNode.NodePtr BodiesNoBarycentres()
        {
            return systemBodies.BodiesNoBarycentres();
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

        public int StarPlanetsScanned()      // This corresponds to FSSDiscoveryScan
        {
            return systemBodies.StarPlanetsScanned();
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
            IBodyFeature sysfeature = OrbitingStations.Find(x => x.Name.EqualsIIC(feature));
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
    }
}
