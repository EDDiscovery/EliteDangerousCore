/*
 * Copyright 2016 - 2023 EDDiscovery development team
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
using System.Text;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan 
    {
        public class StarPlanetRing
        {
            [PropertyNameAttribute("Name")]
            public string Name { get; set; }        
            [PropertyNameAttribute("Ring class")]
            public string RingClass { get; set; }               // FDName 
            public enum RingClassEnum { Unknown , Rocky, Metallic, Icy, MetalRich }

            [PropertyNameAttribute("Ring class as enumeration")]
            public RingClassEnum RingClassID { get; set; }      // Default will be unknown

            [PropertyNameAttribute("Mass in mega tons (1e6)t")]
            public double MassMT { get; set; }
            [PropertyNameAttribute("Inner radius m")]
            public double InnerRad { get; set; }
            [PropertyNameAttribute("Outer radius m")]
            public double OuterRad { get; set; }
            [PropertyNameAttribute("Width m")]
            public double Width { get { return OuterRad - InnerRad; } }
            public string SemiMajorAxisLSKM { get { return (InnerRad >= BodyPhysicalConstants.oneLS_m / 10 ? ((InnerRad / BodyPhysicalConstants.oneLS_m).ToString("N1") + "ls") : ((InnerRad / 1000).ToString("N0") + "km")); } }

            public void Normalise()
            {
                // typos  on metalic is in the journals, edsm/spansh seems to give metallic
                if (Enum.TryParse<RingClassEnum>(RingClass.ReplaceIfStartsWith("eRingClass_").Replace("Metalic","Metallic"), true, out RingClassEnum rc))     // may not have eringclass on it if from EDSM
                    RingClassID = rc;
                else
                {
                    System.Diagnostics.Debug.WriteLine($"!!! Unknown Ring Class {RingClass}");
                }
            }

            // has trailing LF
            public void RingText(StringBuilder sb, string frontpad = "  ")
            {
                sb.Append(frontpad);
                sb.AppendFormat("{0} ({1})", Name.Alt("Unknown".Tx()), TranslatedRingClass());
                sb.AppendCR();

                if (MassMT > (BodyPhysicalConstants.oneMoon_KG / 1e9 / 1000))
                    sb.AppendFormat(frontpad + "Mass: {0:N4}{1}".Tx(), MassMT / (BodyPhysicalConstants.oneMoon_KG / 1E9), " Moons".Tx());
                else
                    sb.AppendFormat(frontpad + "Mass: {0:N4}{1}".Tx(), MassMT, " MT");

                sb.AppendCR();

                if (InnerRad > BodyPhysicalConstants.oneAU_m / 10)       // more than 0.1AU, its in ls
                {
                    sb.AppendFormat(frontpad + "Inner Radius: {0:0.00}ls".Tx(), (InnerRad / BodyPhysicalConstants.oneLS_m));
                    sb.AppendCR();
                    sb.AppendFormat(frontpad + "Outer Radius: {0:0.00}ls".Tx(), (OuterRad / BodyPhysicalConstants.oneLS_m));
                    sb.AppendCR();
                }
                else
                {
                    sb.AppendFormat(frontpad + "Inner Radius: {0}km".Tx(), (InnerRad / 1000).ToString("N0"));
                    sb.AppendCR();
                    sb.AppendFormat(frontpad + "Outer Radius: {0}km".Tx()+ " \u0394 {1}" , (OuterRad / 1000).ToString("N0"), ((OuterRad - InnerRad) / 1000).ToString("N0"));
                    sb.AppendCR();
                }
            }

            public string TranslatedRingClass() 
            {
                switch (RingClassID)
                {
                    default:
                        return "Unknown".Tx();
                    case RingClassEnum.Icy:
                        return "Icy".Tx();
                    case RingClassEnum.Rocky:
                        return "Rocky".Tx();
                    case RingClassEnum.MetalRich:
                        return "Metal Rich".Tx();
                    case RingClassEnum.Metallic:
                        return "Metallic".Tx();
                }
            }
        }

        [System.Diagnostics.DebuggerDisplay("BodyParent {Type} {BodyID}")]
        public class BodyParent
        {
            public enum BodyType { Planet, Null, Star, Ring , Unknown};    
            [PropertyNameAttribute("Type of node, Null = barycentre, Planet, Star, Ring (Beltcluster)")]
            public BodyType Type { get; set; }
            [PropertyNameAttribute("Frontier body ID")]
            public int BodyID { get; set; }
            [PropertyNameAttribute("Is node a barycentre")]
            public bool IsBarycentre { get { return Type == BodyType.Null; } }
            [PropertyNameAttribute("Is node a star")]
            public bool IsStar { get { return Type == BodyType.Star; } }
            [PropertyNameAttribute("Is node a planet")]
            public bool IsPlanet { get { return Type == BodyType.Planet; } }
            [PropertyNameAttribute("Is node a ring (Beltcluster)")]
            public bool IsRing { get { return Type == BodyType.Ring; } }
            //[PropertyNameAttribute("Properties of the barycentre")]
            //public JournalScanBaryCentre Barycentre { get; set; }        // set by star scan system if its a barycentre

            public BodyParent()
            { }
            public BodyParent(BodyType t, int id)
            { Type = t; BodyID = id; }
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

        public void HabZoneText_Hab(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Habitable Zone, {0} ({1}-{2} AU),".Tx(),
                 $"{hz.HabitableZoneInner:N0}-{hz.HabitableZoneOuter:N0}ls",
                 (hz.HabitableZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                 (hz.HabitableZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText_MRP(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Metal Rich planets, {0} ({1}-{2} AU),".Tx(),
                             $"{hz.MetalRichZoneInner:N0}-{hz.MetalRichZoneOuter:N0}ls",
                             (hz.MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (hz.MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText_WW(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Water Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{hz.WaterWrldZoneInner:N0}-{hz.WaterWrldZoneOuter:N0}ls",
                             (hz.WaterWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (hz.WaterWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_EL(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Earth Like Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{hz.EarthLikeZoneInner:N0}-{hz.EarthLikeZoneOuter:N0}ls",
                             (hz.EarthLikeZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (hz.EarthLikeZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_AW(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Ammonia Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{hz.AmmonWrldZoneInner:N0}-{hz.AmmonWrldZoneOuter:N0}ls",
                             (hz.AmmonWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (hz.AmmonWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_ZIP(HabZones hz, StringBuilder sb)
        {
            sb.AppendFormat(" - Icy Planets, {0} (from {1} AU)".Tx(),
                             $"{hz.IcyPlanetZoneInner:N0}ls to ~",
                             (hz.IcyPlanetZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText(StringBuilder sb, bool title)
        {
            HabZones hz = GetHabZones();

            if (hz != null)
            {
                if (title)
                {
                    sb.AppendLine("Inferred Circumstellar zones".Tx()+": ");
                }

                HabZoneText_Hab(hz, sb);
                sb.AppendCR();
                HabZoneText_MRP(hz, sb);
                sb.AppendCR();
                HabZoneText_WW(hz, sb);
                sb.AppendCR();
                HabZoneText_EL(hz, sb);
                sb.AppendCR();
                HabZoneText_AW(hz, sb);
                sb.AppendCR();
                HabZoneText_ZIP(hz, sb);
                sb.AppendCR();

                if (title)
                {
                    if (nSemiMajorAxis.HasValue && nSemiMajorAxis.Value > 0)
                    {
                        sb.Append(" - Others stars not considered".Tx());
                        sb.AppendCR();
                    }
                }
            }
        }
    }
}


