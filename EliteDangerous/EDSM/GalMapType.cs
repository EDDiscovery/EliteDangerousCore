/*
 * Copyright © 2016-2021 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.EDSM
{
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

            MarxNebula, // non EDSM
        }
        public enum GroupType
        {
            Markers = 1,
            Routes,
            Regions,
            Quadrants,
        }
        static public List<GalMapType> GalTypes { get; private set; }  = CreateTypes();     // all the types.
        static public GalMapType[] VisibleTypes { get { return GalTypes.Where(x => x.VisibleType != null).ToArray(); } }

        public string TypeName;         // mostly, EDSM type name
        public string Description;
        public GroupType Group;
        public VisibleObjectsType? VisibleType;
        public int Index;

        private GalMapType(string id, string desc, GroupType g, VisibleObjectsType? te, int i)
        {
            TypeName = id;
            Description = desc;
            Group = g;
            VisibleType = te;
            Index = i;
        }

        private static List<GalMapType> CreateTypes()
        {
            List<GalMapType> type = new List<GalMapType>();

            int index = 0;

            type.Add(new GalMapType("historicalLocation", "η Historical Location", GroupType.Markers, VisibleObjectsType.historicalLocation,index++));
            type.Add(new GalMapType("nebula", "α Nebula", GroupType.Markers, VisibleObjectsType.nebula, index++));
            type.Add(new GalMapType("planetaryNebula", "β Planetary Nebula", GroupType.Markers, VisibleObjectsType.planetaryNebula, index++));
            type.Add(new GalMapType("stellarRemnant", "γ Stellar Features", GroupType.Markers, VisibleObjectsType.stellarRemnant, index++));
            type.Add(new GalMapType("blackHole", "δ Black Hole", GroupType.Markers, VisibleObjectsType.blackHole, index++));
            type.Add(new GalMapType("starCluster", "σ Star Cluster", GroupType.Markers, VisibleObjectsType.starCluster, index++));
            type.Add(new GalMapType("pulsar", "ζ Pulsar", GroupType.Markers , VisibleObjectsType.pulsar, index++));
            type.Add(new GalMapType("minorPOI", "★ Minor POI or Star", GroupType.Markers , VisibleObjectsType.minorPOI,index++));
            type.Add(new GalMapType("beacon", "⛛ Beacon", GroupType.Markers , VisibleObjectsType.beacon,index++));
            type.Add(new GalMapType("surfacePOI", "∅ Surface POI", GroupType.Markers , VisibleObjectsType.surfacePOI,index++));
            type.Add(new GalMapType("cometaryBody", "☄ Cometary Body", GroupType.Markers , VisibleObjectsType.cometaryBody, index++));
            type.Add(new GalMapType("jumponiumRichSystem", "☢ Jumponium-Rich System", GroupType.Markers, VisibleObjectsType.jumponiumRichSystem,index++));
            type.Add(new GalMapType("planetFeatures", "∅ Planetary Features", GroupType.Markers, VisibleObjectsType.planetFeatures,index++));
            type.Add(new GalMapType("deepSpaceOutpost", "Deep space outpost", GroupType.Markers, VisibleObjectsType.deepSpaceOutpost,index++));
            type.Add(new GalMapType("mysteryPOI", "Mystery POI", GroupType.Markers, VisibleObjectsType.mysteryPOI,index++));
            type.Add(new GalMapType("restrictedSectors", "Restricted Sectors", GroupType.Markers, VisibleObjectsType.restrictedSectors,index++));
            type.Add(new GalMapType("independentOutpost", "Independent Outpost", GroupType.Markers, VisibleObjectsType.independentOutpost,index++));
            type.Add(new GalMapType("regional", "Regional Marker", GroupType.Markers, VisibleObjectsType.Regional,index++));
            type.Add(new GalMapType("geyserPOI", "Geyser", GroupType.Markers, VisibleObjectsType.GeyserPOI,index++));
            type.Add(new GalMapType("organicPOI", "Organic Material", GroupType.Markers, VisibleObjectsType.OrganicPOI,index++));
            type.Add(new GalMapType("EDSMUnknown", "EDSM other POI type", GroupType.Markers, VisibleObjectsType.EDSMUnknown,index++));

            // not EDSM

            type.Add(new GalMapType("MarxNebula", "Marx Nebula List", GroupType.Markers, VisibleObjectsType.MarxNebula, index++));

            // not visual
            type.Add(new GalMapType("travelRoute", "Travel Route", GroupType.Routes , null,index++));
            type.Add(new GalMapType("historicalRoute", "Historical Route", GroupType.Routes , null,index++));
            type.Add(new GalMapType("minorRoute", "Minor Route", GroupType.Routes, null,index++));
            type.Add(new GalMapType("neutronRoute", "Neutron highway", GroupType.Routes, null,index++));
            type.Add(new GalMapType("region", "Region", GroupType.Regions, null,index++));
            type.Add(new GalMapType("regionQuadrants", "Galactic Quadrants", GroupType.Quadrants , null,index++));


            return type;
        }
    }
}

