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

                    if (!GasWorld)      // all gas worlds have atmospheres, so don't add it on
                    {
                        sb.AppendCS();
                        sb.Append(Atmosphere == "none" ? "No Atmosphere".T(EDCTx.JournalScan_NoAtmosphere) : Atmosphere);
                    }

                    if (IsLandable)
                    {
                        sb.AppendCS();
                        sb.Append("Landable".T(EDCTx.JournalScan_LandC));
                    }
                    sb.AppendCR();
                }

                if (Terraformable)
                {
                    sb.Append("Candidate for terraforming".T(EDCTx.JournalScan_Candidateforterraforming));
                    sb.AppendCR();
                }

                if (nAge.HasValue)
                {
                    sb.AppendFormat("Age: {0} my".T(EDCTx.JournalScan_AMY), nAge.Value.ToString("N0"));
                    sb.AppendCR();
                }

                if (nStellarMass.HasValue)
                {
                    sb.AppendFormat("Solar Masses: {0:N2}".T(EDCTx.JournalScan_SolarMasses), nStellarMass.Value);
                    sb.AppendCR();
                }

                if (nMassEM.HasValue)
                {
                    sb.Append("Mass: ".T(EDCTx.JournalScan_MASS));
                    sb.AppendSPC();
                    sb.Append(MassEMMM);
                    sb.AppendCR();
                }

                if (nRadius.HasValue)
                {
                    sb.AppendFormat("Radius: ".T(EDCTx.JournalScan_RS));
                    sb.AppendSPC();
                    sb.Append(RadiusText);
                    sb.AppendCR();
                }

                if (DistanceFromArrivalLS > 0)
                {
                    sb.AppendFormat("Distance from Arrival Point {0:N1}ls".T(EDCTx.JournalScan_DistancefromArrivalPoint), DistanceFromArrivalLS);
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
                sb.AppendFormat("Surface Temp: {0}K".T(EDCTx.JournalScan_SurfaceTemp), nSurfaceTemperature.Value.ToString("N0"));
                sb.AppendCR();
            }

            if (nSurfaceGravity.HasValue)
            {
                sb.AppendFormat("Gravity: {0:N2}g".T(EDCTx.JournalScan_GV), nSurfaceGravityG.Value);
                sb.AppendCR();
            }

            if (nSurfacePressure.HasValue && nSurfacePressure.Value > 0.00 && !GasWorld)        // don't print for gas worlds
            {
                if (nSurfacePressure.Value > 1000)
                {
                    sb.AppendFormat("Surface Pressure: {0} Atmospheres".T(EDCTx.JournalScan_SPA), nSurfacePressureEarth.Value.ToString("N2"));
                }
                else
                {
                    sb.AppendFormat("Surface Pressure: {0} Pa".T(EDCTx.JournalScan_SPP), (nSurfacePressure.Value).ToString("N2"));
                }
                sb.AppendCR();
            }

            if (Volcanism.HasChars())
            {
                sb.AppendFormat("Volcanism: {0}".T(EDCTx.JournalScan_Volcanism), Volcanism.IsEmpty() ? "No Volcanism".T(EDCTx.JournalScan_NoVolcanism) : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.
                                                                                            ToTitleCase(Volcanism.ToLowerInvariant()));
                sb.AppendCR();
            }


            if (nOrbitalPeriodDays.HasValue && nOrbitalPeriodDays > 0)
            {
                sb.AppendFormat("Orbital Period: {0} days".T(EDCTx.JournalScan_OrbitalPeriod), nOrbitalPeriodDays.Value.ToString("0.0####"));
                sb.AppendCR();
            }

            if (nSemiMajorAxis.HasValue)
            {
                if (IsStar || nSemiMajorAxis.Value > BodyPhysicalConstants.oneAU_m / 10)
                    sb.AppendFormat("Semi Major Axis: {0:0.00}AU".T(EDCTx.JournalScan_SMA), nSemiMajorAxisAU.Value);
                else
                    sb.AppendFormat("Semi Major Axis: {0}km".T(EDCTx.JournalScan_SMK), (nSemiMajorAxis.Value / 1000).ToString("N1"));
                sb.AppendCR();
            }

            if (nEccentricity.HasValue)
            {
                if (nEccentricity < 0.9)
                    sb.AppendFormat("Orbital Eccentricity: ".T(EDCTx.JournalScan_OrbitalEccentricity) + "{0:0.000}", nEccentricity.Value);
                else 
                    sb.AppendFormat("Orbital Eccentricity: ".T(EDCTx.JournalScan_OrbitalEccentricity) + "{0:0.000000}", nEccentricity.Value);
                sb.AppendCR();
            }

            if (nOrbitalInclination.HasValue)
            {
                sb.AppendFormat("Orbital Inclination: {0:0.000}°".T(EDCTx.JournalScan_OrbitalInclination), nOrbitalInclination.Value);
                sb.AppendCR();
            }

            if (nAscendingNode.HasValue)
            {
                sb.AppendFormat("Ascending Node: {0:0.000}°".T(EDCTx.JournalScan_AscendingNode), nAscendingNode.Value);
                sb.AppendCR();
            }

            if (nPeriapsis.HasValue)
            {
                sb.AppendFormat("Arg Of Periapsis: {0:0.000}°".T(EDCTx.JournalScan_ArgOfPeriapsis), nPeriapsis.Value);
                sb.AppendCR();
            }

            if (nMeanAnomaly.HasValue)
            {
                sb.AppendFormat("Mean Anomaly: {0:0.000}°".T(EDCTx.JournalScan_MeanAnomaly), nMeanAnomaly.Value);
                sb.AppendCR();
            }

            if (nAxialTiltDeg.HasValue)
            {
                sb.AppendFormat("Axial tilt: {0:0.00}°".T(EDCTx.JournalScan_Axialtilt), nAxialTiltDeg.Value);
                sb.AppendCR();
            }

            if (nRotationPeriodDays.HasValue)
            {
                sb.AppendFormat("Rotation Period: {0} days".T(EDCTx.JournalScan_RotationPeriod), nRotationPeriodDays.Value.ToString("0.0####"));
                sb.AppendCR();
            }


            if (nAbsoluteMagnitude.HasValue)
            {
                sb.AppendFormat("Absolute Magnitude: {0:0.00}".T(EDCTx.JournalScan_AbsoluteMagnitude), nAbsoluteMagnitude.Value);
                sb.AppendCR();
            }

            if (nTidalLock.HasValue && nTidalLock.Value)
            {
                sb.Append("Tidally locked".T(EDCTx.JournalScan_Tidallylocked));
                sb.AppendCR();
            }

            if (HasRingsOrBelts)
            {
                if (HasRings)
                    sb.AppendFormat(Rings.Count() == 1 ? "Ring".T(EDCTx.JournalScan_Ring) : "Rings".T(EDCTx.JournalScan_Rings), ""); // OLD translator files had "Rings{0}" so supply an empty string just in case
                else
                    sb.AppendFormat(Rings.Count() == 1 ? "Belt".T(EDCTx.JournalScan_Belt) : "Belts".T(EDCTx.JournalScan_Belts), ""); // OLD translator files had "Belt{0}" so supply an empty string just in case

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
                sb.Append("Mapped".T(EDCTx.JournalScan_MPI));
                if (EfficientMapped)
                {
                    sb.AppendSPC();
                    sb.Append("Efficiently".T(EDCTx.JournalScan_MPIE));
                }
                sb.AppendCR();
            }

            ScanEstimatedValues ev = GetEstimatedValues();

            sb.AppendFormat("Current value: {0:N0}".T(EDCTx.JournalScan_CV) + "", ev.EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsWebSourced));
            sb.AppendCR();

            if (ev.EstimatedValueFirstDiscoveredFirstMapped > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                string msg = "First Discovered+Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVFD) + "";
                sb.AppendFormat(msg, ev.EstimatedValueFirstDiscoveredFirstMapped, ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstMapped > 0 && (!WasMapped.HasValue || !WasMapped.Value))    // if was not mapped
            {
                sb.AppendFormat("First Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVFM) + "", ev.EstimatedValueFirstMapped, ev.EstimatedValueFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                sb.AppendFormat("First Discovered value: {0:N0}".T(EDCTx.JournalScan_FDV) + "", ev.EstimatedValueFirstDiscovered);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0) // if we have extra details, on planets, show the base value
            {
                sb.AppendFormat("Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVM) + "", ev.EstimatedValueMapped, ev.EstimatedValueMappedEfficiently);
                sb.AppendCR();
                sb.AppendFormat("Base Estimated value: {0:N0}".T(EDCTx.JournalScan_EV) + "", ev.EstimatedValueBase);
                sb.AppendCR();
            }

            if (WasDiscovered.HasValue && WasDiscovered.Value)
            {
                sb.AppendFormat("Already Discovered".T(EDCTx.JournalScan_EVAD) + "");
                sb.AppendCR();
            }
            if (WasMapped.HasValue && WasMapped.Value)
            {
                sb.AppendFormat("Already Mapped".T(EDCTx.JournalScan_EVAM) + "");
                sb.AppendCR();
            }

            if (EDSMDiscoveryCommander != null)
            {
                sb.AppendFormat("Discovered by {0} on {1}".T(EDCTx.JournalScan_DB) + "", EDSMDiscoveryCommander, EDSMDiscoveryUTC.ToStringZulu());
                sb.AppendCR();
            }

            if (SurfaceFeatures != null)
            {
                sb.Append("Surface features".T(EDCTx.ScanDisplayUserControl_SurfaceFeatures));
                sb.Append(": ");
                StarScan.SurfaceFeatureList(sb, SurfaceFeatures, 4, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Signals != null)
            {
                sb.Append("Signals".T(EDCTx.ScanDisplayUserControl_Signals));
                sb.Append(": ");
                JournalSAASignalsFound.SignalList(sb, Signals, 4, false, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Genuses != null)
            {
                sb.Append("Genuses".T(EDCTx.ScanDisplayUserControl_Genuses));
                sb.Append(": ");
                JournalSAASignalsFound.GenusList(sb, Genuses, 4, false, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Organics != null)
            {
                sb.Append("Organics".T(EDCTx.ScanDisplayUserControl_Organics));
                sb.Append(": ");
                JournalScanOrganic.OrganicList(sb, Organics, 4, false, Environment.NewLine);
                sb.AppendCR();
            }

            if (ScanType.HasChars())        // early entries did not
            {
                sb.AppendFormat("Scan Type: {0}".T(EDCTx.JournalScan_SCNT), ScanType);
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


