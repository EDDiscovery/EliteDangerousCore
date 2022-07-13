/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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

using System;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan 
    {
        public class StarPlanetRing
        {
            public string Name;     // may be null
            public string RingClass;    // may be null
            public double MassMT;
            public double InnerRad;
            public double OuterRad;

            // has trailing LF
            public string RingInformation(string frontpad = "  ")
            {
                StringBuilder scanText = new StringBuilder();

                scanText.AppendFormat(frontpad + "{0} ({1})\n", Name.Alt("Unknown".T(EDCTx.Unknown)), DisplayStringFromRingClass(RingClass));

                if (MassMT > (BodyPhysicalConstants.oneMoon_KG / 1e9 / 1000))
                    scanText.AppendFormat(frontpad + "Mass: {0:N4}{1}\n".T(EDCTx.StarPlanetRing_Mass), MassMT / (BodyPhysicalConstants.oneMoon_KG / 1E9), " Moons".T(EDCTx.JournalScan_Moons));
                else
                    scanText.AppendFormat(frontpad + "Mass: {0:N0}{1}\n".T(EDCTx.StarPlanetRing_Mass), MassMT, " MT");

                if (InnerRad > BodyPhysicalConstants.oneAU_m / 10)       // more than 0.1AU, its in ls
                {
                    scanText.AppendFormat(frontpad + "Inner Radius: {0:0.00}ls".T(EDCTx.StarPlanetRing_InnerRadius) + Environment.NewLine, (InnerRad / BodyPhysicalConstants.oneLS_m));
                    scanText.AppendFormat(frontpad + "Outer Radius: {0:0.00}ls".T(EDCTx.StarPlanetRing_OuterRadius) + Environment.NewLine, (OuterRad / BodyPhysicalConstants.oneLS_m));
                }
                else
                {
                    scanText.AppendFormat(frontpad + "Inner Radius: {0}km".T(EDCTx.StarPlanetRing_IK) + Environment.NewLine, (InnerRad / 1000).ToString("N0"));
                    scanText.AppendFormat(frontpad + "Outer Radius: {0}km".T(EDCTx.StarPlanetRing_OK) + " \u0394 {1}" + Environment.NewLine, (OuterRad / 1000).ToString("N0"), ((OuterRad - InnerRad) / 1000).ToString("N0"));
                }

                return scanText.ToNullSafeString();
            }

            public static string DisplayStringFromRingClass(string ringClass)   // no trailing LF
            {
                switch (ringClass)
                {
                    case null:
                        return "Unknown".T(EDCTx.Unknown);
                    case "eRingClass_Icy":
                        return "Icy".T(EDCTx.StarPlanetRing_Icy);
                    case "eRingClass_Rocky":
                        return "Rocky".T(EDCTx.StarPlanetRing_Rocky);
                    case "eRingClass_MetalRich":
                        return "Metal Rich".T(EDCTx.StarPlanetRing_MetalRich);
                    case "eRingClass_Metalic":
                        return "Metallic".T(EDCTx.StarPlanetRing_Metallic);
                    case "eRingClass_RockyIce":
                        return "Rocky Ice".T(EDCTx.StarPlanetRing_RockyIce);
                    default:
                        return ringClass.Replace("eRingClass_", "");
                }
            }

            public string RingClassNormalised()
            {
                return RingClass.Replace("eRingClass_", "").SplitCapsWordFull();
            }
        }

        [System.Diagnostics.DebuggerDisplay("BodyParent {Type} {BodyID}")]
        public class BodyParent
        {
            public string Type { get; set; }
            public int BodyID { get; set; }
            public bool IsBaryCentre { get { return Type.Equals("Null", StringComparison.InvariantCultureIgnoreCase); } }
            public bool IsStar { get { return Type.Equals("Star", StringComparison.InvariantCultureIgnoreCase); } }
            public bool IsPlanet { get { return Type.Equals("Planet", StringComparison.InvariantCultureIgnoreCase); } }
            public JournalScanBaryCentre Barycentre { get; set; }        // set by star scan system if its a barycentre
        }


        public class HabZones
        {
            public double HabitableZoneInner { get; set; }             // in AU
            public double HabitableZoneOuter { get; set; }             // in AU
            public double MetalRichZoneInner { get; set; }             // in AU etc
            public double MetalRichZoneOuter { get; set; }
            public double WaterWrldZoneInner { get; set; }
            public double WaterWrldZoneOuter { get; set; }
            public double EarthLikeZoneInner { get; set; }
            public double EarthLikeZoneOuter { get; set; }
            public double AmmonWrldZoneInner { get; set; }
            public double AmmonWrldZoneOuter { get; set; }
            public double IcyPlanetZoneInner { get; set; }
        }

        public HabZones GetHabZones()
        {
            if (IsStar && nRadius.HasValue && nSurfaceTemperature.HasValue)
            {
                HabZones hz = new HabZones();

                // values initially calculated by Jackie Silver (https://forums.frontier.co.uk/member.php/37962-Jackie-Silver)

                hz.HabitableZoneInner = DistanceForBlackBodyTemperature(315); // this is the goldilocks zone, where is possible to expect to find planets with liquid water.
                hz.HabitableZoneOuter = DistanceForBlackBodyTemperature(223);
                hz.MetalRichZoneInner = DistanceForNoMaxTemperatureBody(BodyPhysicalConstants.oneSolRadius_m); // we don't know the maximum temperature that the galaxy simulation take as possible...
                hz.MetalRichZoneOuter = DistanceForBlackBodyTemperature(1100);
                hz.WaterWrldZoneInner = DistanceForBlackBodyTemperature(307);
                hz.WaterWrldZoneOuter = DistanceForBlackBodyTemperature(156);
                hz.EarthLikeZoneInner = DistanceForBlackBodyTemperature(281); // I enlarged a bit the range to fit my and other CMDRs discoveries.
                hz.EarthLikeZoneOuter = DistanceForBlackBodyTemperature(227);
                hz.AmmonWrldZoneInner = DistanceForBlackBodyTemperature(193);
                hz.AmmonWrldZoneOuter = DistanceForBlackBodyTemperature(117);
                hz.IcyPlanetZoneInner = DistanceForBlackBodyTemperature(150);
                return hz;
            }
            else
                return null;
        }

        // Habitable zone calculations, formula cribbed from JackieSilver's HabZone Calculator with permission
        private double DistanceForBlackBodyTemperature(double targetTemp)
        {
            double top = Math.Pow(nRadius.Value, 2.0) * Math.Pow(nSurfaceTemperature.Value, 4.0);
            double bottom = 4.0 * Math.Pow(targetTemp, 4.0);
            double radius_metres = Math.Pow(top / bottom, 0.5);
            return radius_metres / BodyPhysicalConstants.oneLS_m;
        }

        private double DistanceForNoMaxTemperatureBody(double radius)
        {
            return radius / BodyPhysicalConstants.oneLS_m;
        }

        // goldilocks zone. No trailing LF
        public string GetHabZoneStringLs()
        {
            HabZones hz = GetHabZones();
            return hz != null ? $"{hz.HabitableZoneInner:N0}-{hz.HabitableZoneOuter:N0}ls" : "";
        }

        public enum CZPrint { CZAll, CZHab, CZMR, CZWW, CZEL, CZAW, CZIP };

        // trailing LF if titles are on, else not.
        public string CircumstellarZonesString(bool titles, CZPrint p)
        {
            HabZones hz = GetHabZones();

            if (hz != null)
            {
                StringBuilder habZone = new StringBuilder();

                if (titles)
                    habZone.Append("Inferred Circumstellar zones:\n".T(EDCTx.JournalScan_InferredCircumstellarzones));

                if (p == CZPrint.CZAll || p == CZPrint.CZHab)
                {
                    habZone.AppendFormat(" - Habitable Zone, {0} ({1}-{2} AU),\n".T(EDCTx.JournalScan_HabitableZone),
                                     $"{hz.HabitableZoneInner:N0}-{hz.HabitableZoneOuter:N0}ls",
                                     (hz.HabitableZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                                     (hz.HabitableZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (p == CZPrint.CZAll || p == CZPrint.CZMR)
                {
                    habZone.AppendFormat(" - Metal Rich planets, {0} ({1}-{2} AU),\n".T(EDCTx.JournalScan_MetalRichplanets),
                                     $"{hz.MetalRichZoneInner:N0}-{hz.MetalRichZoneOuter:N0}ls",
                                     (hz.MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                                     (hz.MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (p == CZPrint.CZAll || p == CZPrint.CZWW)
                {
                    habZone.AppendFormat(" - Water Worlds, {0} ({1}-{2} AU),\n".T(EDCTx.JournalScan_WaterWorlds),
                                     $"{hz.WaterWrldZoneInner:N0}-{hz.WaterWrldZoneOuter:N0}ls",
                                     (hz.WaterWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                                     (hz.WaterWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (p == CZPrint.CZAll || p == CZPrint.CZEL)
                {
                    habZone.AppendFormat(" - Earth Like Worlds, {0} ({1}-{2} AU),\n".T(EDCTx.JournalScan_EarthLikeWorlds),
                                     $"{hz.EarthLikeZoneInner:N0}-{hz.EarthLikeZoneOuter:N0}ls",
                                     (hz.EarthLikeZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                                     (hz.EarthLikeZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (p == CZPrint.CZAll || p == CZPrint.CZAW)
                {
                    habZone.AppendFormat(" - Ammonia Worlds, {0} ({1}-{2} AU),\n".T(EDCTx.JournalScan_AmmoniaWorlds),
                                     $"{hz.AmmonWrldZoneInner:N0}-{hz.AmmonWrldZoneOuter:N0}ls",
                                     (hz.AmmonWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                                     (hz.AmmonWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (p == CZPrint.CZAll || p == CZPrint.CZIP)
                {
                    habZone.AppendFormat(" - Icy Planets, {0} (from {1} AU)\n".T(EDCTx.JournalScan_IcyPlanets),
                                     $"{hz.IcyPlanetZoneInner:N0}ls to ~",
                                     (hz.IcyPlanetZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
                }

                if (titles)
                {
                    if (nSemiMajorAxis.HasValue && nSemiMajorAxis.Value > 0)
                        habZone.Append(" - Others stars not considered\n".T(EDCTx.JournalScan_Othersstarsnotconsidered));

                    return habZone.ToNullSafeString();
                }
                else
                {
                    if (habZone.Length > 2)
                        habZone.Remove(habZone.Length - 2, 2);      // remove ,\n

                    string s = habZone.ToNullSafeString();
                    if (s.StartsWith(" - "))        // mangle the translated string - can't do it above for backwards compat reasons
                        s = s.Substring(3);

                    return s;
                }

            }
            else
                return null;
        }





    }

}


