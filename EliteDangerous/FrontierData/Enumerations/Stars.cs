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
    // Naming is as per Journal 15.2
    public enum EDStar
    {
        Unknown = 0,
        O = 1,
        B,
        A,
        F,
        G,
        K,
        M,

        // Dwarf
        L,
        T,
        Y,

        // proto stars
        AeBe,
        TTS,


        // wolf rayet
        W,
        WN,
        WNC,
        WC,
        WO,

        // Carbon
        CS,
        C,
        CN,
        CJ,
        CHd,


        MS,  //seen in log
        S,   // seen in log

        // white dwarf
        D,
        DA,
        DAB,
        DAO,
        DAZ,
        DAV,
        DB,
        DBZ,
        DBV,
        DO,
        DOV,
        DQ,
        DC,
        DCV,
        DX,

        N,   // Neutron

        H,   // Black Hole

        X,    // currently speculative, not confirmed with actual data... in journal

        A_BlueWhiteSuperGiant,
        F_WhiteSuperGiant,
        M_RedSuperGiant,
        M_RedGiant,
        K_OrangeGiant,
        RoguePlanet,
        Nebula,
        StellarRemnantNebula,
        SuperMassiveBlackHole,
        B_BlueWhiteSuperGiant,
        G_WhiteSuperGiant,
    };

    public partial class Stars
    {
        private static Dictionary<string, EDStar> starStr2EnumLookup = null;

        public static void Prepopulate()
        {
            starStr2EnumLookup = new Dictionary<string, EDStar>(StringComparer.InvariantCultureIgnoreCase);

            foreach (EDStar atm in Enum.GetValues(typeof(EDStar)))
            {
                starStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
        }

        public static EDStar ToEnum(string star)
        {
            if (star.IsEmpty())
                return EDStar.Unknown;

            var searchstr = star.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (starStr2EnumLookup.ContainsKey(searchstr))
                return starStr2EnumLookup[searchstr];

            return EDStar.Unknown;
        }

        public static string ToEnglish(EDStar star)
        {
            return star.ToString().SplitCapsWordFull();
        }

        public static string StarName( EDStar id )
        {
            switch (id)       // see journal, section 11.2
            {
                case EDStar.O:
                    return string.Format("Luminous Hot {0} class star".Tx(), id.ToString());

                case EDStar.B:
                    // also have an B1V
                    return string.Format("Luminous Blue {0} class star".Tx(), id.ToString());

                case EDStar.A:
                    // also have an A3V..
                    return string.Format("Bluish-White {0} class star".Tx(), id.ToString());

                case EDStar.F:
                    return string.Format("White {0} class star".Tx(), id.ToString());

                case EDStar.G:
                    // also have a G8V
                    return string.Format("Yellow {0} class star".Tx(), id.ToString());

                case EDStar.K:
                    // also have a K0V
                    return string.Format("Orange {0} class star".Tx(), id.ToString());
                case EDStar.M:
                    // also have a M1VA
                    return string.Format("Red {0} class star".Tx(), id.ToString());

                // dwarfs
                case EDStar.L:
                    return string.Format("Dark Red {0} class star".Tx(), id.ToString());
                case EDStar.T:
                    return string.Format("Methane Dwarf T class star".Tx());
                case EDStar.Y:
                    return string.Format("Brown Dwarf Y class star".Tx());

                // proto stars
                case EDStar.AeBe:    // Herbig
                    return "Herbig Ae/Be class star".Tx();
                case EDStar.TTS:     // seen in logs
                    return "T Tauri star".Tx();

                // wolf rayet
                case EDStar.W:
                case EDStar.WN:
                case EDStar.WNC:
                case EDStar.WC:
                case EDStar.WO:
                    return string.Format("Wolf-Rayet {0} class star".Tx(), id.ToString());

                // Carbon
                case EDStar.CS:
                case EDStar.C:
                case EDStar.CN:
                case EDStar.CJ:
                case EDStar.CHd:
                    return string.Format("Carbon {0} class star".Tx(), id.ToString());

                case EDStar.MS: //seen in log https://en.wikipedia.org/wiki/S-type_star
                    return string.Format("Intermediate low Zirconium Monoxide MS class star".Tx());

                case EDStar.S:   // seen in log, data from http://elite-dangerous.wikia.com/wiki/Stars
                    return string.Format("Cool Giant Zirconium Monoxide rich S class star".Tx());

                // white dwarf
                case EDStar.D:
                case EDStar.DA:
                case EDStar.DAB:
                case EDStar.DAO:
                case EDStar.DAZ:
                case EDStar.DAV:
                case EDStar.DB:
                case EDStar.DBZ:
                case EDStar.DBV:
                case EDStar.DO:
                case EDStar.DOV:
                case EDStar.DQ:
                case EDStar.DC:
                case EDStar.DCV:
                case EDStar.DX:
                    return string.Format("White Dwarf {0} class star".Tx(), id.ToString());

                case EDStar.N:
                    return "Neutron Star".Tx();

                case EDStar.H:

                    return "Black Hole".Tx();

                case EDStar.X:
                    // currently speculative, not confirmed with actual data... in journal
                    return "Exotic".Tx();

                // Journal.. really?  need evidence these actually are formatted like this.

                case EDStar.SuperMassiveBlackHole:
                    return "Super Massive Black Hole".Tx();
                case EDStar.A_BlueWhiteSuperGiant:
                    return "A Blue White Super Giant".Tx();
                case EDStar.B_BlueWhiteSuperGiant:
                    return "B Blue White Super Giant".Tx();
                case EDStar.F_WhiteSuperGiant:
                    return "F White Super Giant".Tx();
                case EDStar.G_WhiteSuperGiant:
                    return "G White Super Giant".Tx();
                case EDStar.M_RedSuperGiant:
                    return "M Red Super Giant".Tx();
                case EDStar.M_RedGiant:
                    return "M Red Giant".Tx();
                case EDStar.K_OrangeGiant:
                    return "K Orange Giant".Tx();
                case EDStar.Nebula:
                    return "Nebula".Tx();
                case EDStar.StellarRemnantNebula:
                    return "Stellar Remnant Nebula".Tx();
                case EDStar.RoguePlanet:
                    return "Rogue Planet".Tx();
                case EDStar.Unknown:
                    return "Unknown star class".Tx();

                default:
                    return string.Format("Class {0} star".Tx(), id.ToString());
            }
        }

    }
}
