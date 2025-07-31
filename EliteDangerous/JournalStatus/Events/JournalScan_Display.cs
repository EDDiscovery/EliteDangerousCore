/*
 * Copyright 2016 - 2024 EDDiscovery development team
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
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan 
    {
        public string DisplayString(List<MaterialCommodityMicroResource> historicmatlist = null,
                                                        List<MaterialCommodityMicroResource> currentmatlist = null,
                                                        bool includefront = true)
        {
            var sb = new StringBuilder(1024);
            DisplaySummary(sb, historicmatlist, currentmatlist, includefront);
            return sb.ToString();
        }

        public void DisplaySummary(StringBuilder sb, List<MaterialCommodityMicroResource> historicmatlist = null,
                                                        List<MaterialCommodityMicroResource> currentmatlist = null,
                                                        bool includefront = true)
        {
            if (includefront)
            {
                sb.AppendFormat("{0} {1}", BodyName, IsWebSourced ? $" ({DataSourceName})" : "");
                sb.AppendCR();

                if (IsStar)
                {
                    sb.Append(StarTypeText);
                    sb.AppendSPC();
                    sb.AppendBracketed(StarClassification);
                    sb.AppendCR();
                }
                else if (IsPlanet)
                {
                    sb.Append(PlanetTypeText);

                    if (HasAtmosphere)
                    {
                        sb.AppendCS();
                        sb.Append(AtmosphereTranslated);
                    }

                    if (IsLandable)
                    {
                        sb.AppendCS();
                        sb.Append("Landable".Tx());
                    }
                    sb.AppendCR();
                }

                if (Terraformable)
                {
                    sb.Append("Candidate for terraforming".Tx());
                    sb.AppendCR();
                }

                if (nAge.HasValue)
                {
                    sb.AppendFormat("Age: {0} my".Tx(), nAge.Value.ToString("N0"));
                    sb.AppendCR();
                }

                if (nStellarMass.HasValue)
                {
                    sb.AppendFormat("Solar Masses: {0:N2}".Tx(), nStellarMass.Value);
                    sb.AppendCR();
                }

                if (nMassEM.HasValue)
                {
                    sb.Append("Mass: ".Tx());
                    sb.AppendSPC();
                    sb.Append(MassEMMM);
                    sb.AppendCR();
                }

                if (nRadius.HasValue)
                {
                    sb.AppendFormat("Radius: ".Tx());
                    sb.AppendSPC();
                    sb.Append(RadiusText);
                    sb.AppendCR();
                }

                if (DistanceFromArrivalLS > 0)
                {
                    sb.AppendFormat("Distance from Arrival Point {0:N1}ls".Tx(), DistanceFromArrivalLS);
                    sb.AppendCR();
                }

                if (HasAtmosphericComposition)
                {
                    DisplayAtmosphere(sb, 4);
                }

                if (HasPlanetaryComposition)
                {
                    DisplayComposition(sb,4);
                }
            }

            if (nSurfaceTemperature.HasValue)
            {
                sb.AppendFormat("Surface Temp: {0}K".Tx(), nSurfaceTemperature.Value.ToString("N0"));
                sb.AppendCR();
            }

            if (nSurfaceGravity.HasValue)
            {
                sb.AppendFormat("Gravity: {0:N2}g".Tx(), nSurfaceGravityG.Value);
                sb.AppendCR();
            }

            if (nSurfacePressure.HasValue && nSurfacePressure.Value > 0.00 && !GasWorld)        // don't print for gas worlds
            {
                if (nSurfacePressure.Value > 1000)
                {
                    sb.AppendFormat("Surface Pressure: {0} Atmospheres".Tx(), nSurfacePressureEarth.Value.ToString("N2"));
                }
                else
                {
                    sb.AppendFormat("Surface Pressure: {0} Pa".Tx(), (nSurfacePressure.Value).ToString("N2"));
                }
                sb.AppendCR();
            }

            if (HasMeaningfulVolcanism)
            {
                sb.AppendFormat("Volcanism: {0}".Tx(), VolcanismTranslated);
                sb.AppendCR();
            }


            if (nOrbitalPeriodDays.HasValue && nOrbitalPeriodDays > 0)
            {
                sb.AppendFormat("Orbital Period: {0} days".Tx(), nOrbitalPeriodDays.Value.ToString("0.0####"));
                sb.AppendCR();
            }

            if (nSemiMajorAxis.HasValue)
            {
                if (IsStar || nSemiMajorAxis.Value > BodyPhysicalConstants.oneAU_m / 10)
                    sb.AppendFormat("Semi Major Axis: {0:0.00}AU".Tx(), nSemiMajorAxisAU.Value);
                else
                    sb.AppendFormat("Semi Major Axis: {0}km".Tx(), (nSemiMajorAxis.Value / 1000).ToString("N1"));
                sb.AppendCR();
            }

            if (nEccentricity.HasValue)
            {
                if (nEccentricity < 0.9)
                    sb.AppendFormat("Orbital Eccentricity: ".Tx()+ "{0:0.000}", nEccentricity.Value);
                else 
                    sb.AppendFormat("Orbital Eccentricity: ".Tx()+ "{0:0.000000}", nEccentricity.Value);
                sb.AppendCR();
            }

            if (nOrbitalInclination.HasValue)
            {
                sb.AppendFormat("Orbital Inclination: {0:0.000}°".Tx(), nOrbitalInclination.Value);
                sb.AppendCR();
            }

            if (nAscendingNode.HasValue)
            {
                sb.AppendFormat("Ascending Node: {0:0.000}°".Tx(), nAscendingNode.Value);
                sb.AppendCR();
            }

            if (nPeriapsis.HasValue)
            {
                sb.AppendFormat("Arg Of Periapsis: {0:0.000}°".Tx(), nPeriapsis.Value);
                sb.AppendCR();
            }

            if (nMeanAnomaly.HasValue)
            {
                sb.AppendFormat("Mean Anomaly: {0:0.000}°".Tx(), nMeanAnomaly.Value);
                sb.AppendCR();
            }

            if (nAxialTiltDeg.HasValue)
            {
                sb.AppendFormat("Axial tilt: {0:0.00}°".Tx(), nAxialTiltDeg.Value);
                sb.AppendCR();
            }

            if (nRotationPeriodDays.HasValue)
            {
                sb.AppendFormat("Rotation Period: {0} days".Tx(), nRotationPeriodDays.Value.ToString("0.0####"));
                sb.AppendCR();
            }


            if (nAbsoluteMagnitude.HasValue)
            {
                sb.AppendFormat("Absolute Magnitude: {0:0.00}".Tx(), nAbsoluteMagnitude.Value);
                sb.AppendCR();
            }

            if (nTidalLock.HasValue && nTidalLock.Value)
            {
                sb.Append("Tidally locked".Tx());
                sb.AppendCR();
            }

            if (HasRingsOrBelts)
            {
                if (HasRings)
                    sb.AppendFormat(Rings.Count() == 1 ? "Ring".Tx(): "Rings".Tx(), ""); // OLD translator files had "Rings{0}" so supply an empty string just in case
                else
                    sb.AppendFormat(Rings.Count() == 1 ? "Belt".Tx(): "Belts".Tx(), ""); // OLD translator files had "Belt{0}" so supply an empty string just in case

                sb.Append(": ");

                for (int i = 0; i < Rings.Length; i++)
                    Rings[i].RingText(sb);
            }

            if (HasMaterials)
            {
                DisplayMaterials(sb, 4, historicmatlist, currentmatlist);
            }

            if (IsStar)
            {
                HabZoneText(sb, true);
            }

            if (Mapped)
            {
                sb.Append("Mapped".Tx());
                if (EfficientMapped)
                {
                    sb.AppendSPC();
                    sb.Append("Efficiently".Tx());
                }
                sb.AppendCR();
            }

            ScanEstimatedValues ev = GetEstimatedValues();

            sb.AppendFormat("Current value: {0:N0}".Tx()+ "", ev.EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsWebSourced));
            sb.AppendCR();

            if (ev.EstimatedValueFirstDiscoveredFirstMapped > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                string msg = "First Discovered+Mapped value: {0:N0}/{1:N0}e".Tx()+ "";
                sb.AppendFormat(msg, ev.EstimatedValueFirstDiscoveredFirstMapped, ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstMapped > 0 && (!WasMapped.HasValue || !WasMapped.Value))    // if was not mapped
            {
                sb.AppendFormat("First Mapped value: {0:N0}/{1:N0}e".Tx()+ "", ev.EstimatedValueFirstMapped, ev.EstimatedValueFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                sb.AppendFormat("First Discovered value: {0:N0}".Tx()+ "", ev.EstimatedValueFirstDiscovered);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0) // if we have extra details, on planets, show the base value
            {
                sb.AppendFormat("Mapped value: {0:N0}/{1:N0}e".Tx()+ "", ev.EstimatedValueMapped, ev.EstimatedValueMappedEfficiently);
                sb.AppendCR();
                sb.AppendFormat("Base Estimated value: {0:N0}".Tx()+ "", ev.EstimatedValueBase);
                sb.AppendCR();
            }

            if (WasDiscovered.HasValue && WasDiscovered.Value)
            {
                sb.AppendFormat("Already Discovered".Tx()+ "");
                sb.AppendCR();
            }
            if (WasMapped.HasValue && WasMapped.Value)
            {
                sb.AppendFormat("Already Mapped".Tx()+ "");
                sb.AppendCR();
            }

            if (EDSMDiscoveryCommander != null)
            {
                sb.AppendFormat("Discovered by {0} on {1}".Tx()+ "", EDSMDiscoveryCommander, EDSMDiscoveryUTC.ToStringZulu());
                sb.AppendCR();
            }

            if (SurfaceFeatures != null)
            {
                sb.Append("Surface features".Tx());
                sb.Append(": ");
                StarScan.SurfaceFeatureList(sb, SurfaceFeatures, 4, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Signals != null)
            {
                sb.Append("Signals".Tx());
                sb.Append(": ");
                JournalSAASignalsFound.SignalList(sb, Signals, 4, false, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Genuses != null)
            {
                sb.Append("Genuses".Tx());
                sb.Append(": ");
                JournalSAASignalsFound.GenusList(sb, Genuses, 4, false, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Organics != null)
            {
                sb.Append("Organics".Tx());
                sb.Append(": ");
                JournalScanOrganic.OrganicList(sb, Organics, 4, false, Environment.NewLine);
                sb.AppendCR();
            }

            if (ScanType.HasChars())        // early entries did not
            {
                sb.AppendFormat("Scan Type: {0}".Tx(), ScanType);
                sb.AppendCR();
            }

            if ( Parents!=null && Parents.Count > 0 && Parents[0].Barycentre != null)      // dec 22 bug here on edsm data received a empty parent array- be more defensive
            { 
                JournalScanBaryCentre barycentrejs = Parents[0].Barycentre;
                sb.AppendLine("Barycentre: " + barycentrejs.BodyID.ToString());
                sb.AppendLine("  " + barycentrejs.GetInfo());            // verified it exists
                sb.AppendLine("  " + barycentrejs.GetDetailed());        // verified it exists
            }


#if DEBUG
            if ( Parents != null)
            {
                sb.AppendLine($"Body ID {BodyID}");
                foreach (var x in Parents)
                    sb.AppendLine($"Parent {x.BodyID} {x.Type}");
            }

#endif

            //scanText.AppendFormat("BID+Parents: {0} - {1}", BodyID ?? -1, ParentList());
        }
    }
}


