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
 *
 */

using System;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan 
    {
        public string SurveyorInfoLine(ISystem sys,
                            bool hasminingsignals, bool hasgeosignals, bool hasbiosignals, bool hasthargoidsignals, bool hasguardiansignals, bool hashumansignals, bool hasothersignals,
                            bool hasscanorganics,
                            bool showvolcanism, bool showvalues, bool shortinfo, bool showGravity, bool showAtmos, bool showTemp, bool showRings,
                            int lowRadiusLimit, int largeRadiusLimit, double eccentricityLimit)
        {
            JournalScan js = this;            

            var information = new StringBuilder();

            string bodyname = js.BodyDesignationOrName.ReplaceIfStartsWith(sys.Name);

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

            information.Append(" is a ".T(EDCTx.JournalScanInfo_isa));

            // Additional information
            information.Append((js.IsStar) ? Stars.StarName(js.StarTypeID) + "." : null);
            information.Append((js.CanBeTerraformable) ? @"terraformable ".T(EDCTx.JournalScanInfo_terraformable) : null);
            information.Append((js.IsPlanet) ? Planets.PlanetNameTranslated(js.PlanetTypeID) + "." : null);
            information.Append((js.nRadius < lowRadiusLimit && js.IsPlanet) ? @" Is tiny ".T(EDCTx.JournalScanInfo_LowRadius) + "(" + RadiusText + ")." : null);
            information.Append((js.nRadius > largeRadiusLimit && js.IsPlanet && js.IsLandable) ? @" Is large ".T(EDCTx.JournalScanInfo_LargeRadius) + "(" + RadiusText + ")." : null);
            information.Append((js.IsLandable) ? @" Is landable.".T(EDCTx.JournalScanInfo_islandable) : null);
            information.Append((js.IsLandable && showGravity && js.nSurfaceGravityG.HasValue) ? @" (" + Math.Round(js.nSurfaceGravityG.Value, 2, MidpointRounding.AwayFromZero) + "g)" : null);
            information.Append((js.HasAtmosphere && showAtmos) ? @" Atmosphere: ".T(EDCTx.JournalScanInfo_Atmosphere) + js.AtmosphereTranslated : null);
            information.Append((js.IsLandable && js.nSurfaceTemperature.HasValue && showTemp) ? (string.Format(" Surface temperature: {0} K.".T(EDCTx.JournalScanInfo_SurfaceTemperature), Math.Round(js.nSurfaceTemperature.Value, 1, MidpointRounding.AwayFromZero))) : null);
            information.Append((js.HasMeaningfulVolcanism && showvolcanism) ? @" Has ".T(EDCTx.JournalScanInfo_Has) + js.VolcanismTranslated + "." : null);
            information.Append((hasminingsignals) ? " Has mining signals.".T(EDCTx.JournalScanInfo_Signals) : null);
            information.Append((hasgeosignals) ? (string.Format(" Geological signals: {0}.".T(EDCTx.JournalScanInfo_GeoSignals), js.CountGeoSignals)) : null);
            information.Append((hasbiosignals) ? (string.Format(" Biological signals: {0}.".T(EDCTx.JournalScanInfo_BioSignals), js.CountBioSignals)) : null);
            information.Append((hasthargoidsignals) ? (string.Format(" Thargoid signals: {0}.".T(EDCTx.JournalScanInfo_ThargoidSignals), js.CountThargoidSignals)) : null);
            information.Append((hasguardiansignals) ? (string.Format(" Guardian signals: {0}.".T(EDCTx.JournalScanInfo_GuardianSignals), js.CountGuardianSignals)) : null);
            information.Append((hashumansignals) ? (string.Format(" Human signals: {0}.".T(EDCTx.JournalScanInfo_HumanSignals), js.CountHumanSignals)) : null);
            information.Append((hasothersignals) ? (string.Format(" 'Other' signals: {0}.".T(EDCTx.JournalScanInfo_OtherSignals), js.CountOtherSignals)) : null);
            information.Append((js.HasRingsOrBelts && showRings) ? @" Is ringed.".T(EDCTx.JournalScanInfo_Hasring) : null);
            information.Append((js.nEccentricity >= eccentricityLimit) ? (string.Format(@" Has an high eccentricity of {0}.".T(EDCTx.JournalScanInfo_eccentricity), js.nEccentricity)) : null);
            information.Append(hasscanorganics ? " Has been scanned for organics.".T(EDCTx.JournalScanInfo_scanorganics) : null);

            var ev = js.GetEstimatedValues();

            if (js.WasMapped == true && js.WasDiscovered == true)
            {
                information.Append(" (Mapped & Discovered)".T(EDCTx.JournalScanInfo_MandD));
                if (showvalues)
                {
                    information.Append(' ').Append(ev.EstimatedValueMappedEfficiently.ToString("N0")).Append(" cr");
                }
            }
            else if (js.WasMapped == true && js.WasDiscovered == false)
            {
                information.Append(" (Mapped)".T(EDCTx.JournalScanInfo_MP));
                if (showvalues)
                {
                    information.Append(' ').Append(ev.EstimatedValueFirstMappedEfficiently.ToString("N0")).Append(" cr");
                }
            }
            else if (js.WasDiscovered == true && js.WasMapped == false)
            {
                information.Append(" (Discovered)".T(EDCTx.JournalScanInfo_DIS));
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
                information.Append(' ').Append(js.ShortInformation());
            }
            else
                information.Append(' ').Append(js.DistanceFromArrivalText);

            return information.ToString();
        }

     



    }
}


