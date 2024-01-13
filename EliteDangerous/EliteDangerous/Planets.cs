/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    // names as per journal with spaces replaced by _
    public enum EDPlanet
    {
        Unknown_Body_Type = 0,
        Metal_rich_body = 1000,     
        High_metal_content_body,
        Rocky_body,
        Icy_body,
        Rocky_ice_body,
        Earthlike_body,
        Water_world,
        Ammonia_world,
        Water_giant,
        Water_giant_with_life,
        Gas_giant_with_water_based_life,
        Gas_giant_with_ammonia_based_life,
        Sudarsky_class_I_gas_giant,
        Sudarsky_class_II_gas_giant,
        Sudarsky_class_III_gas_giant,
        Sudarsky_class_IV_gas_giant,
        Sudarsky_class_V_gas_giant,
        Helium_rich_gas_giant,
        Helium_gas_giant,
    }

    [Flags]
    public enum EDAtmosphereProperty
    {
        None = 0,
        Rich = 1,
        Thick = 2,
        Thin = 4,
        Hot = 8,
    }

    public enum EDAtmosphereType   // from the journal
    {
        Earth_Like = 900,
        Ammonia = 1000,
        Water = 2000,
        Carbon_dioxide = 3000,
        Methane = 4000,
        Helium = 5000,
        Argon = 6000,
        Neon = 7000,
        Sulphur_dioxide = 8000,
        Nitrogen = 9000,
        Silicate_vapour = 10000,
        Metallic_vapour = 11000,
        Oxygen = 12000,

        Unknown = 0,
        No_atmosphere = 1,                        
    }


    [Flags]
    public enum EDVolcanismProperty
    {
        None = 0,
        Minor = 1,
        Major = 2,
    }

    public enum EDVolcanism
    {
        Unknown = 0,
        None,
        Water_Magma = 100,
        Sulphur_Dioxide_Magma = 200,
        Ammonia_Magma = 300,
        Methane_Magma = 400,
        Nitrogen_Magma = 500,
        Silicate_Magma = 600,
        Metallic_Magma = 700,
        Water_Geysers = 800,
        Carbon_Dioxide_Geysers = 900,
        Ammonia_Geysers = 1000,
        Methane_Geysers = 1100,
        Nitrogen_Geysers = 1200,
        Helium_Geysers = 1300,
        Silicate_Vapour_Geysers = 1400,
        Rocky_Magma = 1500,
    }

    public enum EDReserve
    {
        None = 0,
        Depleted,
        Low,
        Common,
        Major,
        Pristine,
    }

    public class Planets
    {
        private static Dictionary<string, EDPlanet> planetStr2EnumLookup = null;

        private static Dictionary<EDAtmosphereType, string> atmoscomparestrings = null;

        private static Dictionary<string, EDVolcanism> volcanismStr2EnumLookup = null;

        private static Dictionary<string, EDReserve> reserveStr2EnumLookup = null;

        public static void Prepopulate()
        {
            planetStr2EnumLookup = new Dictionary<string, EDPlanet>(StringComparer.InvariantCultureIgnoreCase);
            atmoscomparestrings = new Dictionary<EDAtmosphereType, string>();
            volcanismStr2EnumLookup = new Dictionary<string, EDVolcanism>(StringComparer.InvariantCultureIgnoreCase);
            reserveStr2EnumLookup = new Dictionary<string, EDReserve>(StringComparer.InvariantCultureIgnoreCase);

            foreach (EDPlanet atm in Enum.GetValues(typeof(EDPlanet)))
            {
                planetStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
            foreach (EDAtmosphereType atm in Enum.GetValues(typeof(EDAtmosphereType)))
            {
                atmoscomparestrings[atm] = atm.ToString().ToLowerInvariant().Replace("_", " ");
            }
            foreach (EDVolcanism atm in Enum.GetValues(typeof(EDVolcanism)))
            {
                volcanismStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
            foreach (EDReserve atm in Enum.GetValues(typeof(EDReserve)))
            {
                reserveStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
        }

        public static EDPlanet ToEnum(string planet)
        {
            if (planet.IsEmpty())
                return EDPlanet.Unknown_Body_Type;

            var searchstr = planet.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (planetStr2EnumLookup.ContainsKey(searchstr))
                return planetStr2EnumLookup[searchstr];

            return EDPlanet.Unknown_Body_Type;
        }

        public static EDAtmosphereType ToEnum(string v, out EDAtmosphereProperty atmprop)
        {
            atmprop = EDAtmosphereProperty.None;

            if (v.IsEmpty())
                return EDAtmosphereType.Unknown;

            if (v.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                return EDAtmosphereType.No_atmosphere;

            var searchstr = v.ToLowerInvariant();

            if (searchstr.Contains("rich"))
            {
                atmprop |= EDAtmosphereProperty.Rich;
            }
            if (searchstr.Contains("thick"))
            {
                atmprop |= EDAtmosphereProperty.Thick;
            }
            if (searchstr.Contains("thin"))
            {
                atmprop |= EDAtmosphereProperty.Thin;
            }
            if (searchstr.Contains("hot"))
            {
                atmprop |= EDAtmosphereProperty.Hot;
            }

            foreach( var kvp in atmoscomparestrings)
            {
                if (searchstr.Contains(kvp.Value))     // both are lower case, does it contain it?
                    return kvp.Key;
            }

            return EDAtmosphereType.Unknown;
        }

        public static EDVolcanism ToEnum(string v, out EDVolcanismProperty vprop )
        {
            vprop = EDVolcanismProperty.None;

            if (v.IsEmpty())
                return EDVolcanism.Unknown;

            string searchstr = v.ToLowerInvariant().Replace("_", "").Replace(" ", "").Replace("-", "").Replace("volcanism", "");

            if (searchstr.Contains("minor"))
            {
                vprop |= EDVolcanismProperty.Minor;
                searchstr = searchstr.Replace("minor", "");
            }
            if (searchstr.Contains("major"))
            {
                vprop |= EDVolcanismProperty.Major;
                searchstr = searchstr.Replace("major", "");
            }

            if (volcanismStr2EnumLookup.ContainsKey(searchstr))
                return volcanismStr2EnumLookup[searchstr];

            return EDVolcanism.Unknown;
        }

        public static EDReserve ReserveToEnum(string reservelevel)
        {
            if (reservelevel.IsEmpty())
                return EDReserve.None;

            var searchstr = reservelevel.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (reserveStr2EnumLookup.ContainsKey(searchstr))
                return reserveStr2EnumLookup[searchstr];

            return EDReserve.None;
        }

        // These should be translated to match the in-game planet types
        private static readonly Dictionary<EDPlanet, string> planetEnumToNameLookup = new Dictionary<EDPlanet, string>
        {
            [EDPlanet.Metal_rich_body] = "Metal-rich body".T(EDCTx.EDPlanet_Metalrichbody),
            [EDPlanet.High_metal_content_body] = "High metal content world".T(EDCTx.EDPlanet_Highmetalcontentbody),
            [EDPlanet.Rocky_body] = "Rocky body".T(EDCTx.EDPlanet_Rockybody),
            [EDPlanet.Icy_body] = "Icy body".T(EDCTx.EDPlanet_Icybody),
            [EDPlanet.Rocky_ice_body] = "Rocky ice world".T(EDCTx.EDPlanet_Rockyicebody),
            [EDPlanet.Earthlike_body] = "Earth-like world".T(EDCTx.EDPlanet_Earthlikebody),
            [EDPlanet.Water_world] = "Water world".T(EDCTx.EDPlanet_Waterworld),
            [EDPlanet.Ammonia_world] = "Ammonia world".T(EDCTx.EDPlanet_Ammoniaworld),
            [EDPlanet.Water_giant] = "Water giant".T(EDCTx.EDPlanet_Watergiant),
            [EDPlanet.Water_giant_with_life] = "Water giant with life".T(EDCTx.EDPlanet_Watergiantwithlife),
            [EDPlanet.Gas_giant_with_water_based_life] = "Gas giant with water-based life".T(EDCTx.EDPlanet_Gasgiantwithwaterbasedlife),
            [EDPlanet.Gas_giant_with_ammonia_based_life] = "Gas giant with ammonia-based life".T(EDCTx.EDPlanet_Gasgiantwithammoniabasedlife),
            [EDPlanet.Sudarsky_class_I_gas_giant] = "Class I gas giant".T(EDCTx.EDPlanet_SudarskyclassIgasgiant),
            [EDPlanet.Sudarsky_class_II_gas_giant] = "Class II gas giant".T(EDCTx.EDPlanet_SudarskyclassIIgasgiant),
            [EDPlanet.Sudarsky_class_III_gas_giant] = "Class III gas giant".T(EDCTx.EDPlanet_SudarskyclassIIIgasgiant),
            [EDPlanet.Sudarsky_class_IV_gas_giant] = "Class IV gas giant".T(EDCTx.EDPlanet_SudarskyclassIVgasgiant),
            [EDPlanet.Sudarsky_class_V_gas_giant] = "Class V gas giant".T(EDCTx.EDPlanet_SudarskyclassVgasgiant),
            [EDPlanet.Helium_rich_gas_giant] = "Helium-rich gas giant".T(EDCTx.EDPlanet_Heliumrichgasgiant),
            [EDPlanet.Helium_gas_giant] = "Helium gas giant".T(EDCTx.EDPlanet_Heliumgasgiant),
            [EDPlanet.Unknown_Body_Type] = "Unknown planet type".T(EDCTx.EDPlanet_Unknown),
        };

        public static string PlanetName(EDPlanet type)
        {
            string name;
            if (planetEnumToNameLookup.TryGetValue(type, out name))
            {
                return name;
            }
            else
            {
                return type.ToString().Replace("_", " ");
            }
        }

        public static bool AmmoniaWorld(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Ammonia_world; }
        public static bool Earthlike(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Earthlike_body; } 
        public static bool WaterWorld(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Water_world; } 
        public static bool SudarskyGasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Sudarsky_class_I_gas_giant && PlanetTypeID <= EDPlanet.Sudarsky_class_V_gas_giant; }
        public static bool GasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Gas_giant_with_water_based_life && PlanetTypeID <= EDPlanet.Gas_giant_with_ammonia_based_life; }
        public static bool WaterGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Water_giant && PlanetTypeID <= EDPlanet.Water_giant_with_life; }
        public static bool HeliumGasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Helium_rich_gas_giant && PlanetTypeID <= EDPlanet.Helium_gas_giant; }
        public static bool GasWorld(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Gas_giant_with_water_based_life && PlanetTypeID <= EDPlanet.Helium_gas_giant; }

        public static string SudarskyClass(EDPlanet PlanetTypeID) { return (new string[] { "I", "II", "III", "IV", "V" })[(int)(PlanetTypeID - EDPlanet.Sudarsky_class_I_gas_giant)]; }

        private static string[] ClassificationAbv = new string[]
        {
            "MR","HMC","R","I","R+I","E","W","A","WG","WGL","GWL","GAL","S-I","S-II","S-III","S-IV","S-V","HRG","HG"
        };

        public static string PlanetAbv(EDPlanet PlanetTypeID)
        {
            if (PlanetTypeID == EDPlanet.Unknown_Body_Type)
                return "U";
            else
                return ClassificationAbv[(int)PlanetTypeID - (int)EDPlanet.Metal_rich_body];
        }
    }
}
