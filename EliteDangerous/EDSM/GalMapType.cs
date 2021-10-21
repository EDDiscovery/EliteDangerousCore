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
using System.Drawing;

namespace EliteDangerousCore.EDSM
{
    public enum GalMapTypeEnum
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


    }

    public class GalMapType
    {
        public enum GalMapGroup
        {
            Markers = 1,
            Routes,
            Regions,
            Quadrants,
        }

        public string Typeid;
        public string Description;
        public Image Image;
        public GalMapGroup Group;
        public bool Animate;
        public int Index;

        public GalMapType(string id, string desc, GalMapGroup g, Image b, bool animate, int i)
        {
            Typeid = id;
            Description = desc;
            Group = g;
            Image = b;
            Animate = animate;
            Index = i;
        }

        public static IReadOnlyDictionary<GalMapTypeEnum, Image> GalMapTypeIcons { get; } = new BaseUtils.Icons.IconGroup<GalMapTypeEnum>("GalMap");

        static public List<GalMapType> GetTypes()
        {
            List<GalMapType> type = new List<GalMapType>();

            int index = 0;

            type.Add(new GalMapType("historicalLocation", "η Historical Location", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.historicalLocation], false, index++));
            type.Add(new GalMapType("nebula", "α Nebula", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.nebula], true, index++));
            type.Add(new GalMapType("planetaryNebula", "β Planetary Nebula", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.planetaryNebula], true, index++));
            type.Add(new GalMapType("stellarRemnant", "γ Stellar Features", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.stellarRemnant], true, index++));
            type.Add(new GalMapType("blackHole", "δ Black Hole", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.blackHole], true, index++));
            type.Add(new GalMapType("starCluster", "σ Star Cluster", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.starCluster], true, index++));
            type.Add(new GalMapType("pulsar", "ζ Pulsar", GalMapGroup.Markers , GalMapTypeIcons[GalMapTypeEnum.pulsar], true, index++));
            type.Add(new GalMapType("minorPOI", "★ Minor POI or Star", GalMapGroup.Markers , GalMapTypeIcons[GalMapTypeEnum.minorPOI], false, index++));
            type.Add(new GalMapType("beacon", "⛛ Beacon", GalMapGroup.Markers , GalMapTypeIcons[GalMapTypeEnum.beacon], false, index++));
            type.Add(new GalMapType("surfacePOI", "∅ Surface POI", GalMapGroup.Markers , GalMapTypeIcons[GalMapTypeEnum.surfacePOI], false, index++));
            type.Add(new GalMapType("cometaryBody", "☄ Cometary Body", GalMapGroup.Markers , GalMapTypeIcons[GalMapTypeEnum.cometaryBody], true, index++));
            type.Add(new GalMapType("jumponiumRichSystem", "☢ Jumponium-Rich System", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.jumponiumRichSystem], false, index++));
            type.Add(new GalMapType("planetFeatures", "∅ Planetary Features", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.planetFeatures], false, index++));
            type.Add(new GalMapType("deepSpaceOutpost", "Deep space outpost", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.deepSpaceOutpost], false, index++));
            type.Add(new GalMapType("mysteryPOI", "Mystery POI", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.mysteryPOI], false, index++));
            type.Add(new GalMapType("restrictedSectors", "Restricted Sectors", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.restrictedSectors], false, index++));
            type.Add(new GalMapType("independentOutpost", "Independent Outpost", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.independentOutpost], false, index++));
            type.Add(new GalMapType("regional", "Regional Marker", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.Regional], false, index++));
            type.Add(new GalMapType("geyserPOI", "Geyser", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.GeyserPOI], false, index++));
            type.Add(new GalMapType("organicPOI", "Organic Material", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.OrganicPOI], false, index++));
            type.Add(new GalMapType("EDSMUnknown", "EDSM other POI type", GalMapGroup.Markers, GalMapTypeIcons[GalMapTypeEnum.EDSMUnknown], false, index++));

            // not visual
            type.Add(new GalMapType("travelRoute", "Travel Route", GalMapGroup.Routes , null, false, index++));
            type.Add(new GalMapType("historicalRoute", "Historical Route", GalMapGroup.Routes , null, false, index++));
            type.Add(new GalMapType("minorRoute", "Minor Route", GalMapGroup.Routes, null, false, index++));
            type.Add(new GalMapType("neutronRoute", "Neutron highway", GalMapGroup.Routes, null, false, index++));
            type.Add(new GalMapType("region", "Region", GalMapGroup.Regions, null, false, index++));
            type.Add(new GalMapType("regionQuadrants", "Galactic Quadrants", GalMapGroup.Quadrants , null, false, index++));


            return type;
        }
    }
}

