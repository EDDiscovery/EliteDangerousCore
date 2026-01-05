/*
 * Copyright 2025-2025 EDDiscovery development team
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

namespace EliteDangerousCore
{
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

        public HabZones(double radius, double surfacetemp)
        {
            HabitableZoneInner = DistanceForBlackBodyTemperature(radius, surfacetemp, 315); // this is the goldilocks zone, where is possible to expect to find planets with liquid water.
            HabitableZoneOuter = DistanceForBlackBodyTemperature(radius, surfacetemp, 223);
            MetalRichZoneInner = DistanceForNoMaxTemperatureBody(BodyPhysicalConstants.oneSolRadius_m); // we don't know the maximum temperature that the galaxy simulation take as possible...
            MetalRichZoneOuter = DistanceForBlackBodyTemperature(radius, surfacetemp, 1100);
            WaterWrldZoneInner = DistanceForBlackBodyTemperature(radius, surfacetemp, 307);
            WaterWrldZoneOuter = DistanceForBlackBodyTemperature(radius, surfacetemp, 156);
            EarthLikeZoneInner = DistanceForBlackBodyTemperature(radius, surfacetemp, 281); // I enlarged a bit the range to fit my and other CMDRs discoveries.
            EarthLikeZoneOuter = DistanceForBlackBodyTemperature(radius, surfacetemp, 227);
            AmmonWrldZoneInner = DistanceForBlackBodyTemperature(radius, surfacetemp, 193);
            AmmonWrldZoneOuter = DistanceForBlackBodyTemperature(radius, surfacetemp, 117);
            IcyPlanetZoneInner = DistanceForBlackBodyTemperature(radius, surfacetemp, 150);
        }

        public void HabZoneText_Hab(StringBuilder sb)
        {
            sb.AppendFormat(" - Habitable Zone, {0} ({1}-{2} AU),".Tx(),
                 $"{HabitableZoneInner:N0}-{HabitableZoneOuter:N0}ls",
                 (HabitableZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                 (HabitableZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText_MRP(StringBuilder sb)
        {
            sb.AppendFormat(" - Metal Rich planets, {0} ({1}-{2} AU),".Tx(),
                             $"{MetalRichZoneInner:N0}-{MetalRichZoneOuter:N0}ls",
                             (MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (MetalRichZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText_WW(StringBuilder sb)
        {
            sb.AppendFormat(" - Water Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{WaterWrldZoneInner:N0}-{WaterWrldZoneOuter:N0}ls",
                             (WaterWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (WaterWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_EL(StringBuilder sb)
        {
            sb.AppendFormat(" - Earth Like Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{EarthLikeZoneInner:N0}-{EarthLikeZoneOuter:N0}ls",
                             (EarthLikeZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (EarthLikeZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_AW(StringBuilder sb)
        {
            sb.AppendFormat(" - Ammonia Worlds, {0} ({1}-{2} AU),".Tx(),
                             $"{AmmonWrldZoneInner:N0}-{AmmonWrldZoneOuter:N0}ls",
                             (AmmonWrldZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"),
                             (AmmonWrldZoneOuter / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }
        public void HabZoneText_ZIP(StringBuilder sb)
        {
            sb.AppendFormat(" - Icy Planets, {0} (from {1} AU)".Tx(),
                             $"{IcyPlanetZoneInner:N0}ls to ~",
                             (IcyPlanetZoneInner / BodyPhysicalConstants.oneAU_LS).ToString("N2"));
        }

        public void HabZoneText(StringBuilder sb, bool title)
        {
            if (title)
            {
                sb.AppendLine("Inferred Circumstellar zones".Tx() + ": ");
            }

            HabZoneText_Hab(sb);
            sb.AppendCR();
            HabZoneText_MRP(sb);
            sb.AppendCR();
            HabZoneText_WW(sb);
            sb.AppendCR();
            HabZoneText_EL(sb);
            sb.AppendCR();
            HabZoneText_AW(sb);
            sb.AppendCR();
            HabZoneText_ZIP(sb);
            sb.AppendCR();
        }

        public string GetHabZoneStringLs()
        {
            return $"{HabitableZoneInner:N0}-{HabitableZoneOuter:N0}ls";
        }

        // Habitable zone calculations, formula cribbed from JackieSilver's HabZone Calculator with permission
        private double DistanceForBlackBodyTemperature(double radius, double surfacetemp, double targetTemp)
        {
            double top = Math.Pow(radius, 2.0) * Math.Pow(surfacetemp, 4.0);
            double bottom = 4.0 * Math.Pow(targetTemp, 4.0);
            double radius_metres = Math.Pow(top / bottom, 0.5);
            return radius_metres / BodyPhysicalConstants.oneLS_m;
        }
        private double DistanceForNoMaxTemperatureBody(double radius)
        {
            return radius / BodyPhysicalConstants.oneLS_m;
        }
    }

}
