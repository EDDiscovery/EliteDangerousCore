/*
 * Copyright © 2016-2024 EDDiscovery development team
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

using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.GMO
{
    [System.Diagnostics.DebuggerDisplay("{TypeName} {Group} {VisibleType} {Description}")]
    public class GalMapType
    {
        public enum VisibleObjectsType            // IDs of visible objects
        {
            historicalLocation,
            nebula,
            planetaryNebula,
            stellarRemnant,
            blackHole,
            starCluster,
            pulsar,
            minorPOI,
            beacon,
            surfacePOI,
            cometaryBody,
            jumponiumRichSystem,
            planetFeatures,
            deepSpaceOutpost,
            mysteryPOI,
            restrictedSectors,
            independentOutpost,
            Regional,
            GeyserPOI,
            OrganicPOI,
            EDSMUnknown,
            InhabitedSystem,
            MarxNebula,
        }
        public enum GroupType
        {
            Markers = 1,
            Routes,
            Regions,
            Quadrants,
            Legacy,
        }

        [QuickJSON.JsonIgnore]
        static public List<GalMapType> GalTypes { get { if (galtypes == null) CreateTypes(); return galtypes; } }

        [QuickJSON.JsonIgnore]
        static public GalMapType[] VisibleTypes { get { return GalTypes.Where(x => x.VisibleType != null).ToArray(); } }

        public string TypeName { get; set; }                     // type name from below
        public string Description { get; set; }                  // description from below
        public GroupType Group { get; set; }                     // Group type
        public VisibleObjectsType? VisibleType { get; set; }     // if null, its not visible, else its the visible type
        [QuickJSON.JsonIgnore]
        public int Index { get; set; }                           // which index it is, used for visual lookup

        private GalMapType(string typename, string desc, GroupType g, VisibleObjectsType? te, int i)
        {
            TypeName = typename;
            Description = desc;
            Group = g;
            VisibleType = te;
            Index = i;
        }

        private static List<GalMapType> galtypes = null;

        private static void CreateTypes()
        {
            galtypes = new List<GalMapType>();

            int index = 0;

            // edsm types
            galtypes.Add(new GalMapType("historicalLocation", "η Historical Location", GroupType.Markers, VisibleObjectsType.historicalLocation, index++));
            galtypes.Add(new GalMapType("nebula", "α Nebula", GroupType.Markers, VisibleObjectsType.nebula, index++));
            galtypes.Add(new GalMapType("planetaryNebula", "β Planetary Nebula", GroupType.Markers, VisibleObjectsType.planetaryNebula, index++));
            galtypes.Add(new GalMapType("stellarRemnant", "γ Stellar Features", GroupType.Markers, VisibleObjectsType.stellarRemnant, index++));
            galtypes.Add(new GalMapType("blackHole", "δ Black Hole", GroupType.Markers, VisibleObjectsType.blackHole, index++));
            galtypes.Add(new GalMapType("starCluster", "σ Star Cluster", GroupType.Markers, VisibleObjectsType.starCluster, index++));
            galtypes.Add(new GalMapType("pulsar", "ζ Pulsar", GroupType.Markers, VisibleObjectsType.pulsar, index++));
            galtypes.Add(new GalMapType("minorPOI", "★ Minor POI or Star", GroupType.Markers, VisibleObjectsType.minorPOI, index++));
            galtypes.Add(new GalMapType("surfacePOI", "∅ Surface POI", GroupType.Markers, VisibleObjectsType.surfacePOI, index++));
            galtypes.Add(new GalMapType("jumponiumRichSystem", "☢ Jumponium-Rich System", GroupType.Markers, VisibleObjectsType.jumponiumRichSystem, index++));
            galtypes.Add(new GalMapType("planetFeatures", "∅ Planetary Features", GroupType.Markers, VisibleObjectsType.planetFeatures, index++));
            galtypes.Add(new GalMapType("deepSpaceOutpost", "Deep space outpost", GroupType.Markers, VisibleObjectsType.deepSpaceOutpost, index++));
            galtypes.Add(new GalMapType("mysteryPOI", "Mystery POI", GroupType.Markers, VisibleObjectsType.mysteryPOI, index++));
            galtypes.Add(new GalMapType("restrictedSectors", "Restricted Sectors", GroupType.Markers, VisibleObjectsType.restrictedSectors, index++));
            galtypes.Add(new GalMapType("independentOutpost", "Independent Outpost", GroupType.Markers, VisibleObjectsType.independentOutpost, index++));
            galtypes.Add(new GalMapType("regional", "Regional Marker", GroupType.Markers, VisibleObjectsType.Regional, index++));
            galtypes.Add(new GalMapType("geyserPOI", "Geyser", GroupType.Markers, VisibleObjectsType.GeyserPOI, index++));
            galtypes.Add(new GalMapType("organicPOI", "Organic Material", GroupType.Markers, VisibleObjectsType.OrganicPOI, index++));
            galtypes.Add(new GalMapType("EDSMUnknown", "EDSM other POI type", GroupType.Markers, VisibleObjectsType.EDSMUnknown, index++));

            // GEC additional types
            galtypes.Add(new GalMapType("GECSS", "Sights and Scenery", GroupType.Markers, VisibleObjectsType.historicalLocation, index++));     //?
            galtypes.Add(new GalMapType("GECMX", "Mystery and Xenology", GroupType.Markers, VisibleObjectsType.mysteryPOI, index++));     
            galtypes.Add(new GalMapType("GECTB", "Tourist Beacons", GroupType.Markers, VisibleObjectsType.beacon, index++));     
            galtypes.Add(new GalMapType("GECNSP", "Notable Stellar Phenomena", GroupType.Markers, VisibleObjectsType.historicalLocation, index++));     //?
            galtypes.Add(new GalMapType("GECCOMM", "Community", GroupType.Markers, VisibleObjectsType.minorPOI, index++));     
            galtypes.Add(new GalMapType("GECDSO", "Deep Space Outpost", GroupType.Markers, VisibleObjectsType.deepSpaceOutpost, index++));     
            galtypes.Add(new GalMapType("GECNEB", "Nebulae", GroupType.Markers, VisibleObjectsType.nebula, index++));    
            galtypes.Add(new GalMapType("GECMEM", "Memorials", GroupType.Markers, VisibleObjectsType.historicalLocation, index++));     //?
            galtypes.Add(new GalMapType("GECGGG", "Green Gas Giants", GroupType.Markers, VisibleObjectsType.planetFeatures, index++));     
            galtypes.Add(new GalMapType("GECPC", "Planetary Circumnavigation", GroupType.Markers, VisibleObjectsType.planetFeatures, index++));     
            galtypes.Add(new GalMapType("GECGLITCH", "Glitches", GroupType.Markers, VisibleObjectsType.minorPOI, index++));
            galtypes.Add(new GalMapType("GECSF", "System Features", GroupType.Markers, VisibleObjectsType.minorPOI, index++));     //?
            galtypes.Add(new GalMapType("GECIS", "Inhabited System", GroupType.Markers, VisibleObjectsType.InhabitedSystem, index++));     //?
            galtypes.Add(new GalMapType("GECSET", "Settlement", GroupType.Markers, VisibleObjectsType.InhabitedSystem, index++));     //?

            // not EDSM/GEC

            galtypes.Add(new GalMapType("MarxNebula", "Marx Nebula List", GroupType.Markers, VisibleObjectsType.MarxNebula, index++));

            // not visual
            galtypes.Add(new GalMapType("travelRoute", "Travel Route", GroupType.Routes , null,index++));
            galtypes.Add(new GalMapType("historicalRoute", "Historical Route", GroupType.Routes , null,index++));
            galtypes.Add(new GalMapType("minorRoute", "Minor Route", GroupType.Routes, null,index++));
            galtypes.Add(new GalMapType("neutronRoute", "Neutron highway", GroupType.Routes, null,index++));
            galtypes.Add(new GalMapType("region", "Region", GroupType.Regions, null,index++));
            galtypes.Add(new GalMapType("regionQuadrants", "Galactic Quadrants", GroupType.Quadrants , null,index++));
            galtypes.Add(new GalMapType("GECLEGACY", "Legacy", GroupType.Legacy, null, index++));     // GEC Legacy 3.8 version mark
        }
    }
}

