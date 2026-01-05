/*
 * Copyright 2016 - 2025 EDDiscovery development team
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
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan
    {
        public string DisplayText(List<MaterialCommodityMicroResource> historicmatlist = null,
                                                        List<MaterialCommodityMicroResource> currentmatlist = null,
                                                        bool includefront = true)
        {
            var sb = new StringBuilder(1024);
            DisplayText(sb, historicmatlist, currentmatlist, includefront);
            return sb.ToString();
        }

        public void DisplayText(StringBuilder sb, List<MaterialCommodityMicroResource> historicmatlist = null,
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
                    sb.Append("Mass".Tx() + ": ");
                    sb.AppendSPC();
                    sb.Append(MassEMMM);
                    sb.AppendCR();
                }

                if (nRadius.HasValue)
                {
                    sb.AppendFormat("Radius".Tx() + ": ");
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
                    DisplayComposition(sb, 4);
                }
            }

            if (SurfaceFeatures != null)
            {
                sb.Append("Surface features".Tx());
                sb.Append(": " + Environment.NewLine);
                DisplaySurfaceFeatures(sb, SurfaceFeatures, 4, true, Environment.NewLine);
                sb.AppendCR();
            }
            if (Signals != null)
            {
                sb.Append("Signals".Tx());
                sb.Append(": " + Environment.NewLine);
                JournalSAASignalsFound.SignalList(sb, Signals, 4, true, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Genuses != null)
            {
                sb.Append("Genuses".Tx());
                sb.Append(": " + Environment.NewLine);
                JournalSAASignalsFound.GenusList(sb, Genuses, 4, true, false, Environment.NewLine);
                sb.AppendCR();
            }
            if (Organics != null)
            {
                sb.Append("Organics".Tx());
                sb.Append(": " + Environment.NewLine);
                JournalScanOrganic.OrganicList(sb, Organics, 4, true, Environment.NewLine);
                sb.AppendCR();
            }
            if (CodexEntries != null)
            {
                sb.Append("Codex".Tx());
                sb.Append(": " + Environment.NewLine);
                JournalCodexEntry.CodexList(sb, CodexEntries, 4, true, Environment.NewLine);
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
                    sb.AppendFormat("Orbital Eccentricity".Tx() + ": " + "{0:0.000}", nEccentricity.Value);
                else
                    sb.AppendFormat("Orbital Eccentricity".Tx() + ": " + "{0:0.000000}", nEccentricity.Value);
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
                    sb.AppendFormat(Rings.Count() == 1 ? "Ring".Tx() : "Rings".Tx(), ""); // OLD translator files had "Rings{0}" so supply an empty string just in case
                else
                    sb.AppendFormat(Rings.Count() == 1 ? "Belt".Tx() : "Belts".Tx(), ""); // OLD translator files had "Belt{0}" so supply an empty string just in case

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
                HabZones hz = GetHabZones();
                if ( hz != null)
                    hz.HabZoneText(sb, true);
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

            sb.AppendFormat("Current value: {0:N0}".Tx() + "", ev.EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsWebSourced));
            sb.AppendCR();

            if (ev.EstimatedValueFirstDiscoveredFirstMapped > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                string msg = "First Discovered+Mapped value: {0:N0}/{1:N0}e".Tx() + "";
                sb.AppendFormat(msg, ev.EstimatedValueFirstDiscoveredFirstMapped, ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstMapped > 0 && (!WasMapped.HasValue || !WasMapped.Value))    // if was not mapped
            {
                sb.AppendFormat("First Mapped value: {0:N0}/{1:N0}e".Tx() + "", ev.EstimatedValueFirstMapped, ev.EstimatedValueFirstMappedEfficiently);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0 && (!WasDiscovered.HasValue || !WasDiscovered.Value))  // if we don't know, or its not discovered
            {
                sb.AppendFormat("First Discovered value: {0:N0}".Tx() + "", ev.EstimatedValueFirstDiscovered);
                sb.AppendCR();
            }

            if (ev.EstimatedValueFirstDiscovered > 0) // if we have extra details, on planets, show the base value
            {
                sb.AppendFormat("Mapped value: {0:N0}/{1:N0}e".Tx() + "", ev.EstimatedValueMapped, ev.EstimatedValueMappedEfficiently);
                sb.AppendCR();
                sb.AppendFormat("Base Estimated value: {0:N0}".Tx() + "", ev.EstimatedValueBase);
                sb.AppendCR();
            }

            if (WasDiscovered.HasValue && WasDiscovered.Value)
            {
                sb.AppendFormat("Already Discovered".Tx() + "");
                sb.AppendCR();
            }
            if (WasMapped.HasValue && WasMapped.Value)
            {
                sb.AppendFormat("Already Mapped".Tx() + "");
                sb.AppendCR();
            }


            if (ScanType.HasChars())        // early entries did not
            {
                sb.AppendFormat("Scan Type: {0}".Tx(), ScanType);
                sb.AppendCR();
            }

            sb.AppendLine($"Body ID {BodyID} : {ParentList()}");
        }

        public string SurveyorInfoLine(ISystem sys,
                               bool hasminingsignals, bool hasgeosignals, bool hasbiosignals, bool hasthargoidsignals, bool hasguardiansignals, bool hashumansignals, bool hasothersignals,
                               bool hasscanorganics,
                               bool showvolcanism, bool showvalues, bool shortinfo, bool showGravity, bool showAtmos, bool showTemp, bool showRings,
                               int lowRadiusLimit, int largeRadiusLimit, double eccentricityLimit)
        {
            JournalScan js = this;

            var information = new StringBuilder();

            string bodyname = js.BodyName.ReplaceIfStartsWith(sys.Name);

            // Name
            information.Append(bodyname);

            // symbols
            if (js.Mapped)  // let the cmdr see that this body is already mapped
                information.Append(" \u24C2");

            if (js.CountOrganicsScansAnalysed > 0) // if scanned
            {
                if (js.CountOrganicsScansAnalysed == js.CountBioSignals)   // and show organic scan situation - fully with a tick
                    information.Append(" \u232C\u2713");
                else
                    information.Append(" \u232C");  // partial
            }

            information.Append(" is a ".Tx());

            // Additional information
            information.Append((js.IsStar) ? Stars.ToLocalisedLanguage(js.StarTypeID) + "." : null);
            information.Append((js.CanBeTerraformable) ? @"terraformable ".Tx() : null);
            information.Append((js.IsPlanet) ? Planets.PlanetNameTranslated(js.PlanetTypeID) + "." : null);
            information.Append((js.nRadius < lowRadiusLimit && js.IsPlanet) ? @" Is tiny ".Tx() + "(" + RadiusText + ")." : null);
            information.Append((js.nRadius > largeRadiusLimit && js.IsPlanet && js.IsLandable) ? @" Is large ".Tx() + "(" + RadiusText + ")." : null);
            information.Append((js.IsLandable) ? @" Is landable.".Tx() : null);
            information.Append((js.IsLandable && showGravity && js.nSurfaceGravityG.HasValue) ? @" (" + Math.Round(js.nSurfaceGravityG.Value, 2, MidpointRounding.AwayFromZero) + "g)" : null);
            information.Append((js.HasAtmosphere && showAtmos) ? @" Atmosphere".Tx() + ": " + js.AtmosphereTranslated : null);
            information.Append((js.IsLandable && js.nSurfaceTemperature.HasValue && showTemp) ? (string.Format(" Surface temperature: {0} K.".Tx(), Math.Round(js.nSurfaceTemperature.Value, 1, MidpointRounding.AwayFromZero))) : null);
            information.Append((js.HasMeaningfulVolcanism && showvolcanism) ? @" Has ".Tx() + js.VolcanismTranslated + "." : null);
            information.Append((hasminingsignals) ? " Has mining signals.".Tx() : null);
            information.Append((hasgeosignals) ? (string.Format(" Geological signals: {0}.".Tx(), js.CountGeoSignals)) : null);
            information.Append((hasbiosignals) ? (string.Format(" Biological signals: {0}.".Tx(), js.CountBioSignals)) : null);
            information.Append((hasthargoidsignals) ? (string.Format(" Thargoid signals: {0}.".Tx(), js.CountThargoidSignals)) : null);
            information.Append((hasguardiansignals) ? (string.Format(" Guardian signals: {0}.".Tx(), js.CountGuardianSignals)) : null);
            information.Append((hashumansignals) ? (string.Format(" Human signals: {0}.".Tx(), js.CountHumanSignals)) : null);
            information.Append((hasothersignals) ? (string.Format(" 'Other' signals: {0}.".Tx(), js.CountOtherSignals)) : null);
            information.Append((js.HasRingsOrBelts && showRings) ? @" Is ringed.".Tx() : null);
            information.Append((js.nEccentricity >= eccentricityLimit) ? (string.Format(@" Has an high eccentricity of {0}.".Tx(), js.nEccentricity)) : null);
            information.Append(hasscanorganics ? " Has been scanned for organics.".Tx() : null);

            var ev = js.GetEstimatedValues();

            if (js.WasMapped == true && js.WasDiscovered == true)
            {
                information.Append(" (Mapped & Discovered)".Tx());
                if (showvalues)
                {
                    information.Append(' ').Append(ev.EstimatedValueMappedEfficiently.ToString("N0")).Append(" cr");
                }
            }
            else if (js.WasMapped == true && js.WasDiscovered == false)
            {
                information.Append(" (Mapped)".Tx());
                if (showvalues)
                {
                    information.Append(' ').Append(ev.EstimatedValueFirstMappedEfficiently.ToString("N0")).Append(" cr");
                }
            }
            else if (js.WasDiscovered == true && js.WasMapped == false)
            {
                information.Append(" (Discovered)".Tx());
                if (showvalues)
                {
                    information.Append(' ').Append((ev.EstimatedValueFirstMappedEfficiently > 0 ? ev.EstimatedValueFirstMappedEfficiently : ev.EstimatedValueBase).ToString("N0")).Append(" cr");
                }
            }
            else if (js.WasDiscovered == false && js.IsStar)
            {
                if (showvalues)
                {
                    information.Append(' ').Append((ev.EstimatedValueFirstDiscovered > 0 ? ev.EstimatedValueFirstDiscovered : ev.EstimatedValueBase).ToString("N0")).Append(" cr");
                }
            }
            else
            {
                if (showvalues)
                {
                    information.Append(' ').Append((ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently > 0 ? ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently : ev.EstimatedValueBase).ToString("N0")).Append(" cr");
                }
            }

            if (shortinfo)
            {
                information.Append(' ').Append(js.DisplayShortInformation());
            }
            else
                information.Append(' ').Append(js.DistanceFromArrivalText);

            return information.ToString();
        }

        public string DisplayShortInformation()
        {
            if (IsStar)
            {
                return BaseUtils.FieldBuilder.Build("Mass: ;SM;0.00".Tx(), nStellarMass,
                                                "Age: ;my;0.0".Tx(), nAge,
                                                "Radius".Tx() + ": ", RadiusText,
                                                "Dist".Tx() + ": ", DistanceFromArrivalLS > 0 ? DistanceFromArrivalText : null);
            }
            else
            {
                return BaseUtils.FieldBuilder.Build("Mass".Tx() + ": ", MassEMMM,
                                                 "Radius".Tx() + ": ", RadiusText,
                                                 "Dist".Tx() + ": ", DistanceFromArrivalLS > 0 ? DistanceFromArrivalText : null);
            }
        }



        // show material counts at the historic point and current.  Has trailing LF if text present.
        public void DisplayMaterials(StringBuilder sb, int indent = 0, List<MaterialCommodityMicroResource> historicmatlist = null, List<MaterialCommodityMicroResource> currentmatlist = null)
        {
            if (HasMaterials)
            {
                string indents = new string(' ', indent);

                sb.Append("Materials".Tx() + ": ");
                sb.AppendSPC();

                int index = 0;
                foreach (KeyValuePair<string, double> mat in Materials)
                {
                    if (index++ > 0)
                        sb.Append(indents);
                    DisplayMaterial(sb, mat.Key, mat.Value, historicmatlist, currentmatlist);
                }
            }
        }
        // has trailing LF
        public void DisplayMaterial(StringBuilder sb, string fdname, double percent, List<MaterialCommodityMicroResource> historicmatlist = null,
                                                                      List<MaterialCommodityMicroResource> currentmatlist = null)
        {
            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);

            if (mc != null && (historicmatlist != null || currentmatlist != null))
            {
                MaterialCommodityMicroResource historic = historicmatlist?.Find(x => x.Details == mc);
                MaterialCommodityMicroResource current = ReferenceEquals(historicmatlist, currentmatlist) ? null : currentmatlist?.Find(x => x.Details == mc);
                int? limit = mc.MaterialLimitOrNull();

                string matinfo = historic?.Count.ToString() ?? "0";
                if (limit != null)
                    matinfo += "/" + limit.Value.ToString();

                if (current != null && (historic == null || historic.Count != current.Count))
                    matinfo += " Cur " + current.Count.ToString();

                sb.AppendFormat("{0}: ({1}) {2} {3}% {4}", mc.TranslatedName, mc.Shortname, mc.TranslatedType, percent.ToString("N1"), matinfo);
            }
            else
                sb.AppendFormat("{0}: {1}%", System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fdname.ToLowerInvariant()),
                                                            percent.ToString("N1"));
            sb.AppendCR();
        }

        private void DisplayAtmosphere(StringBuilder sb, int indent = 0)     // has trailing LF
        {
            string indents = new string(' ', indent);

            sb.Append("Atmospheric Composition".Tx() + ": ");
            sb.AppendSPC();
            int index = 0;
            foreach (KeyValuePair<string, double> comp in AtmosphereComposition)
            {
                if (index++ > 0)
                    sb.Append(indents);

                sb.AppendFormat("{0}: {1}%", comp.Key, comp.Value.ToString("N2"));
                sb.AppendCR();
            }
        }

        private void DisplayComposition(StringBuilder sb, int indent = 0)   // has trailing LF
        {
            string indents = new string(' ', indent);

            sb.Append("Planetary Composition".Tx() + ": ");
            sb.AppendSPC();
            int index = 0;
            foreach (KeyValuePair<string, double> comp in PlanetComposition)
            {
                if (comp.Value > 0)
                {
                    if (index++ > 0)
                        sb.Append(indents);
                    sb.AppendFormat("{0}: {1}%", comp.Key, comp.Value.ToString("N2"));
                    sb.AppendCR();
                }
            }
        }

        static public void DisplaySurfaceFeatures(System.Text.StringBuilder sb, List<IBodyFeature> list, int indent, bool indentfirst, string separ = ", ")        // default is environment.newline
        {
            string inds = new string(' ', indent);

            int index = 0;
            foreach (var ibf in list)
            {
                //System.Diagnostics.Debug.WriteLine($"{s.ScanType} {s.Genus_Localised} {s.Species_Localised}");
                if (indent > 0 && (index > 0 || indentfirst))       // if indent, and its either not first or allowed to indent first
                    sb.Append(inds);

                sb.Append($"{EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(ibf.EventTimeUTC)} : {ibf.Name_Localised ?? ibf.Name ?? ibf.EventTypeStr} {ibf.Latitude:0.####}, {ibf.Longitude:0.####}");

                if (index++ < list.Count - 1)     // if another to go, separ
                    sb.Append(separ);
            }
        }

        [PropertyNameAttribute(null)] // cancel
        public string StarTypeImageName                      // property so its gets outputted via JSON
        {
            get
            {
                if (!IsStar)
                {
                    return $"Bodies.Unknown";
                }

                return BodyDefinitions.StarTypeImageName(StarTypeID, nStellarMass, nSurfaceTemperature);
            }
        }

        [PropertyNameAttribute(null)]       // cancel
        public string PlanetClassImageName       // property so its gets outputted via JSON
        {
            get
            {
                if (!IsPlanet)
                {
                    return $"Bodies.Unknown";
                }

                return BodyDefinitions.PlanetClassImageName(PlanetTypeID, nSurfaceTemperature, AtmosphereComposition, AtmosphereProperty, AtmosphereID,
                                                         Terraformable, nLandable, nMassEM, nTidalLock);
            }
        }

        public System.Drawing.Image GetImage()
        {
            return IsStar ? BaseUtils.Icons.IconSet.GetIcon(StarTypeImageName) :
                   IsPlanet ? BaseUtils.Icons.IconSet.GetIcon(PlanetClassImageName) :
                   IsBeltClusterBody ? BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.SizeLarge") :
                   BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingOnly");
        }
    }
}

