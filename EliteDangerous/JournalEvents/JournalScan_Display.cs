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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan 
    {

        // has no trailing LF
        public string DisplayString(int indent = 0, List<MaterialCommodityMicroResource> historicmatlist = null,
                                                        List<MaterialCommodityMicroResource> currentmatlist = null,
                                                        bool includefront = true)
        {
            string inds = new string(' ', indent);

            StringBuilder scanText = new StringBuilder();

            scanText.Append(inds);

            if (includefront)
            {
                scanText.AppendFormat("{0} {1}" + Environment.NewLine, BodyName, IsWebSourced ? $" ({DataSourceName})" : "");
                scanText.Append(Environment.NewLine);

                if (IsStar)
                {
                    scanText.AppendFormat(StarTypeText + " (" + StarClassification + ")\n");
                }
                else if (IsPlanet)
                {
                    scanText.AppendFormat("{0}", PlanetTypeText);

                    if (!GasWorld)      // all gas worlds have atmospheres, so don't add it on
                    {
                        scanText.AppendFormat(", " + (Atmosphere == "none" ? "No Atmosphere".T(EDCTx.JournalScan_NoAtmosphere) : Atmosphere));
                    }

                    if (IsLandable)
                        scanText.AppendFormat(", Landable".T(EDCTx.JournalScan_LandC));
                    scanText.AppendFormat("\n");
                }

                if (Terraformable)
                    scanText.Append("Candidate for terraforming\n".T(EDCTx.JournalScan_Candidateforterraforming));

                if (nAge.HasValue)
                    scanText.AppendFormat("Age: {0} my\n".T(EDCTx.JournalScan_AMY), nAge.Value.ToString("N0"));

                if (nStellarMass.HasValue)
                    scanText.AppendFormat("Solar Masses: {0:0.00}\n".T(EDCTx.JournalScan_SolarMasses), nStellarMass.Value);

                if (nMassEM.HasValue)
                    scanText.AppendFormat("Mass: ".T(EDCTx.JournalScan_MASS) + " " + MassEMMM + "\n");

                if (nRadius.HasValue)
                    scanText.AppendFormat("Radius: ".T(EDCTx.JournalScan_RS) + " " + RadiusText + "\n");

                if (DistanceFromArrivalLS > 0)
                    scanText.AppendFormat("Distance from Arrival Point {0:N1}ls\n".T(EDCTx.JournalScan_DistancefromArrivalPoint), DistanceFromArrivalLS);

                if (HasAtmosphericComposition)
                    scanText.Append(DisplayAtmosphere(4));

                if (HasPlanetaryComposition)
                    scanText.Append(DisplayComposition(4));
            }

            if (nSurfaceTemperature.HasValue)
                scanText.AppendFormat("Surface Temp: {0}K\n".T(EDCTx.JournalScan_SurfaceTemp), nSurfaceTemperature.Value.ToString("N0"));

            if (nSurfaceGravity.HasValue)
                scanText.AppendFormat("Gravity: {0:0.00}g\n".T(EDCTx.JournalScan_GV), nSurfaceGravityG.Value);

            if (nSurfacePressure.HasValue && nSurfacePressure.Value > 0.00 && !GasWorld)        // don't print for gas worlds
            {
                if (nSurfacePressure.Value > 1000)
                {
                    scanText.AppendFormat("Surface Pressure: {0} Atmospheres\n".T(EDCTx.JournalScan_SPA), nSurfacePressureEarth.Value.ToString("N2"));
                }
                else
                {
                    scanText.AppendFormat("Surface Pressure: {0} Pa\n".T(EDCTx.JournalScan_SPP), (nSurfacePressure.Value).ToString("N2"));
                }
            }

            if (Volcanism.HasChars())
                scanText.AppendFormat("Volcanism: {0}\n".T(EDCTx.JournalScan_Volcanism), Volcanism.IsEmpty() ? "No Volcanism".T(EDCTx.JournalScan_NoVolcanism) : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.
                                                                                            ToTitleCase(Volcanism.ToLowerInvariant()));


            if (nOrbitalPeriodDays.HasValue && nOrbitalPeriodDays > 0)
                scanText.AppendFormat("Orbital Period: {0} days\n".T(EDCTx.JournalScan_OrbitalPeriod), nOrbitalPeriodDays.Value.ToString("0.0####"));

            if (nSemiMajorAxis.HasValue)
            {
                if (IsStar || nSemiMajorAxis.Value > BodyPhysicalConstants.oneAU_m / 10)
                    scanText.AppendFormat("Semi Major Axis: {0:0.00}AU\n".T(EDCTx.JournalScan_SMA), nSemiMajorAxisAU.Value);
                else
                    scanText.AppendFormat("Semi Major Axis: {0}km\n".T(EDCTx.JournalScan_SMK), (nSemiMajorAxis.Value / 1000).ToString("N1"));
            }

            if (nEccentricity.HasValue)
            {
                if (nEccentricity < 0.9)
                    scanText.AppendFormat("Orbital Eccentricity: ".T(EDCTx.JournalScan_OrbitalEccentricity) + "{0:0.000}\n", nEccentricity.Value);
                else 
                    scanText.AppendFormat("Orbital Eccentricity: ".T(EDCTx.JournalScan_OrbitalEccentricity) + "{0:0.000000}\n", nEccentricity.Value);
            }

            if (nOrbitalInclination.HasValue)
                scanText.AppendFormat("Orbital Inclination: {0:0.000}°\n".T(EDCTx.JournalScan_OrbitalInclination), nOrbitalInclination.Value);

            if (nAscendingNode.HasValue)
                scanText.AppendFormat("Ascending Node: {0:0.000}°\n".T(EDCTx.JournalScan_AscendingNode), nAscendingNode.Value);

            if (nPeriapsis.HasValue)
                scanText.AppendFormat("Arg Of Periapsis: {0:0.000}°\n".T(EDCTx.JournalScan_ArgOfPeriapsis), nPeriapsis.Value);

            if (nMeanAnomaly.HasValue)
                scanText.AppendFormat("Mean Anomaly: {0:0.000}°\n".T(EDCTx.JournalScan_MeanAnomaly), nMeanAnomaly.Value);

            if (nAxialTiltDeg.HasValue)
                scanText.AppendFormat("Axial tilt: {0:0.00}°\n".T(EDCTx.JournalScan_Axialtilt), nAxialTiltDeg.Value);

            if (nRotationPeriodDays.HasValue)
                scanText.AppendFormat("Rotation Period: {0} days\n".T(EDCTx.JournalScan_RotationPeriod), nRotationPeriodDays.Value.ToString("0.0####"));


            if (nAbsoluteMagnitude.HasValue)
                scanText.AppendFormat("Absolute Magnitude: {0:0.00}\n".T(EDCTx.JournalScan_AbsoluteMagnitude), nAbsoluteMagnitude.Value);

            if (nTidalLock.HasValue && nTidalLock.Value)
                scanText.Append("Tidally locked\n".T(EDCTx.JournalScan_Tidallylocked));

            if (HasRingsOrBelts)
            {
                scanText.Append("\n");

                if (HasRings)
                    scanText.AppendFormat(Rings.Count() == 1 ? "Ring".T(EDCTx.JournalScan_Ring) : "Rings".T(EDCTx.JournalScan_Rings), ""); // OLD translator files had "Rings{0}" so supply an empty string just in case
                else
                    scanText.AppendFormat(Rings.Count() == 1 ? "Belt".T(EDCTx.JournalScan_Belt) : "Belts".T(EDCTx.JournalScan_Belts), ""); // OLD translator files had "Belt{0}" so supply an empty string just in case

                for (int i = 0; i < Rings.Length; i++)
                    scanText.Append("\n" + Rings[i].RingInformation());

                scanText.Append("\n");
            }

            if (HasMaterials)
            {
                scanText.Append(DisplayMaterials(4, historicmatlist, currentmatlist));
            }

            if (IsStar)
            {
                string czs = CircumstellarZonesString(true, CZPrint.CZAll);
                if (czs != null)
                    scanText.Append(czs);
            }

            if (Mapped)
            {
                scanText.Append("Mapped".T(EDCTx.JournalScan_MPI));
                if (EfficientMapped)
                    scanText.Append(" " + "Efficiently".T(EDCTx.JournalScan_MPIE));
                scanText.Append("\n");
            }

            ScanEstimatedValues ev = GetEstimatedValues();

            scanText.AppendFormat("Current value: {0:N0}".T(EDCTx.JournalScan_CV) + "\n", ev.EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsWebSourced));

            if (ev.EstimatedValueFirstDiscoveredFirstMapped > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                string msg = "First Discovered+Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVFD) + "\n";
                scanText.AppendFormat(msg, ev.EstimatedValueFirstDiscoveredFirstMapped, ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently);
            }

            if (ev.EstimatedValueFirstMapped > 0 && (!WasMapped.HasValue || !WasMapped.Value))    // if was not mapped
            {
                scanText.AppendFormat("First Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVFM) + "\n", ev.EstimatedValueFirstMapped, ev.EstimatedValueFirstMappedEfficiently);
            }

            if (ev.EstimatedValueFirstDiscovered > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                scanText.AppendFormat("First Discovered value: {0:N0}".T(EDCTx.JournalScan_FDV) + "\n", ev.EstimatedValueFirstDiscovered);
            }

            if (ev.EstimatedValueFirstDiscovered > 0) // if we have extra details, on planets, show the base value
            {
                scanText.AppendFormat("Mapped value: {0:N0}/{1:N0}e".T(EDCTx.JournalScan_EVM) + "\n", ev.EstimatedValueMapped, ev.EstimatedValueMappedEfficiently);
                scanText.AppendFormat("Base Estimated value: {0:N0}".T(EDCTx.JournalScan_EV) + "\n", ev.EstimatedValueBase);
            }

            if (WasDiscovered.HasValue && WasDiscovered.Value)
                scanText.AppendFormat("Already Discovered".T(EDCTx.JournalScan_EVAD) + "\n");
            if (WasMapped.HasValue && WasMapped.Value)
                scanText.AppendFormat("Already Mapped".T(EDCTx.JournalScan_EVAM) + "\n");

            if (EDSMDiscoveryCommander != null)
                scanText.AppendFormat("Discovered by {0} on {1}".T(EDCTx.JournalScan_DB) + "\n", EDSMDiscoveryCommander, EDSMDiscoveryUTC.ToStringZulu());

            if (SurfaceFeatures != null)
                scanText.AppendFormat("Surface features".T(EDCTx.ScanDisplayUserControl_SurfaceFeatures) + ":\n" + StarScan.SurfaceFeatureList(SurfaceFeatures, 4, "\n") + "\n");
            if (Signals != null)
                scanText.AppendFormat("Signals".T(EDCTx.ScanDisplayUserControl_Signals) + ":\n" + JournalSAASignalsFound.SignalList(Signals, 4, "\n") + "\n");
            if (Genuses != null)
                scanText.AppendFormat("Genuses".T(EDCTx.ScanDisplayUserControl_Genuses) + ":\n" + JournalSAASignalsFound.GenusList(Genuses, 4, "\n") + "\n");
            if (Organics != null)
                scanText.AppendFormat("Organics".T(EDCTx.ScanDisplayUserControl_Organics) + ":\n" + JournalScanOrganic.OrganicList(Organics, 4, "\n") + "\n");

            scanText.AppendFormat("Scan Type: {0}".T(EDCTx.JournalScan_SCNT) + "\n", ScanType);

            if ( Parents!=null && Parents.Count > 0 && Parents[0].Barycentre != null)      // dec 22 bug here on edsm data received a empty parent array- be more defensive
            { 
                JournalScanBaryCentre barycentrejs = Parents[0].Barycentre;
                scanText.AppendLine();
                scanText.AppendLine("Barycentre: " + barycentrejs.BodyID.ToString());
                barycentrejs.FillInformation(out string info, out string detailed);
                scanText.AppendLine(info);
                scanText.AppendLine(detailed);
            }


#if DEBUG
            if ( Parents != null)
            {
                scanText.AppendLine();
                scanText.AppendLine($"Body ID {BodyID}");
                foreach (var x in Parents)
                    scanText.AppendLine($"Parent {x.BodyID} {x.Type}");
            }

#endif

            //scanText.AppendFormat("BID+Parents: {0} - {1}\n", BodyID ?? -1, ParentList());

            if (scanText.Length > 0 && scanText[scanText.Length - 1] == '\n')
                scanText.Remove(scanText.Length - 1, 1);

            return scanText.ToNullSafeString().Replace("\n", "\n" + inds);
        }



    }
}


