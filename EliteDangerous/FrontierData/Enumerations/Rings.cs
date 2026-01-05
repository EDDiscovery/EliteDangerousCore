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
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore
{
    public class StarPlanetRing
    {
        [PropertyNameAttribute("Name")]
        public string Name { get; set; }
        [PropertyNameAttribute("Ring class")]
        public string RingClass { get; set; }               // FDName 
        public enum RingClassEnum { Unknown, Rocky, Metallic, Icy, MetalRich }

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
            if (Enum.TryParse<RingClassEnum>(RingClass.ReplaceIfStartsWith("eRingClass_").Replace("Metalic", "Metallic"), true, out RingClassEnum rc))     // may not have eringclass on it if from EDSM
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
                sb.AppendFormat(frontpad + "Outer Radius: {0}km".Tx() + " \u0394 {1}", (OuterRad / 1000).ToString("N0"), ((OuterRad - InnerRad) / 1000).ToString("N0"));
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
}
