﻿/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    [System.Diagnostics.DebuggerDisplay("Event {EventTypeStr} {EventTimeUTC} {BodyName} {BodyDesignation} s{IsStar} p{IsPlanet}")]
    [JournalEntryType(JournalTypeEnum.Scan)]
    public partial class JournalScan : JournalEntry, IStarScan, IBodyNameIDOnly
    {
        public bool IsStar { get { return StarType != null; } }
        public bool IsBeltCluster { get { return StarType == null && PlanetClass == null; } }
        public bool IsPlanet { get { return PlanetClass != null; } }

        public string BodyDesignation { get; set; }     // nerfed name

        public string BodyDesignationOrName { get { return BodyDesignation ?? BodyName; } }

        // ALL
        [PropertyNameAttribute("Scan type, Basic, Detailed, NavBeacon, NavBeaconDetail, AutoScan, may be empty for very old scans")]
        public string ScanType { get; private set; }                        // 3.0 scan type  Basic, Detailed, NavBeacon, NavBeaconDetail, (3.3) AutoScan, or empty for older ones
        public string BodyName { get; private set; }                        // direct (meaning no translation)
        [PropertyNameAttribute("Internal Frontier ID, is empty for older scans")]
        public int? BodyID { get; private set; }                            // direct
        public string StarSystem { get; private set; }                      // direct (3.5)
        [PropertyNameAttribute("Internal Frontier ID, is empty for older scans")]
        public long? SystemAddress { get; private set; }                    // direct (3.5)
        public double DistanceFromArrivalLS { get; private set; }           // direct
        [PropertyNameAttribute("In meters")]
        public double DistanceFromArrivalm { get { return DistanceFromArrivalLS * BodyPhysicalConstants.oneLS_m; } }
        public string DistanceFromArrivalText { get { return string.Format("{0:0.00}AU ({1:0.0}ls)", DistanceFromArrivalLS / BodyPhysicalConstants.oneAU_LS, DistanceFromArrivalLS); } }
        [PropertyNameAttribute("Seconds, may be negative indicating reverse rotation")]
        public double? nRotationPeriod { get; private set; }                // direct, can be negative indi
        [PropertyNameAttribute("Days, may be negative indicating reverse rotation")]
        public double? nRotationPeriodDays { get { if (nRotationPeriod.HasValue) return nRotationPeriod.Value / BodyPhysicalConstants.oneDay_s; else return null; } }
        [PropertyNameAttribute("K")]
        public double? nSurfaceTemperature { get; private set; }            // direct
        [PropertyNameAttribute("Meters")]
        public double? nRadius { get; private set; }                        // direct
        public double? nRadiusSols { get { if (nRadius.HasValue) return nRadius.Value / BodyPhysicalConstants.oneSolRadius_m; else return null; } }
        public double? nRadiusEarths { get { if (nRadius.HasValue) return nRadius.Value / BodyPhysicalConstants.oneEarthRadius_m; else return null; } }

        public bool HasRingsOrBelts { get { return Rings != null && Rings.Length > 0; } }
        public bool HasBelts { get { return HasRingsOrBelts && Rings[0].Name.Contains("Belt"); } }
        public bool HasRings { get { return HasRingsOrBelts && !Rings[0].Name.Contains("Belt"); } }

        [PropertyNameAttribute("Rings[1 to N] _Name, _RingClass, _MassMT, _InnerRad (m), _OuterRad. Also RingCount")]
        public StarPlanetRing[] Rings { get; private set; }

        [PropertyNameAttribute("Distance of inner ring from planet, Meters, null if no rings")]
        public double? RingsInnerm { get { if (Rings != null) return Rings.Select(x => x.InnerRad).Min(); else return null; } }
        [PropertyNameAttribute("Distance of final outer ring from planet, Meters, null if no rings")]
        public double? RingsOuterm { get { if (Rings != null) return Rings.Select(x => x.OuterRad).Max(); else return null; } }

        [PropertyNameAttribute("Parents[1 to N] _Type (Null=Barycentre, Star, Planet), _BodyID. Also ParentsCount. First is nearest body")]
        public List<BodyParent> Parents { get; private set; }
        public bool IsOrbitingBaryCentre { get { return Parents?.FirstOrDefault()?.Type == "Null"; } }
        public bool IsOrbitingPlanet { get { return Parents?.FirstOrDefault()?.Type == "Planet"; } }
        public bool IsOrbitingStar { get { return Parents?.FirstOrDefault()?.Type == "Star"; } }

        public bool? WasDiscovered { get; private set; }                    // direct, 3.4, indicates whether the body has already been discovered
        public bool IsNotPreviouslyDiscovered { get { return WasDiscovered.HasValue && WasDiscovered == false; } } // true if its there, and its not mapped
        public bool IsPreviouslyDiscovered { get { return WasDiscovered.HasValue && WasDiscovered == true; } } // true if its there, and its discovered

        public bool? WasMapped { get; private set; }                        // direct, 3.4, indicates whether the body has already been mapped
        public bool IsNotPreviouslyMapped { get { return WasMapped.HasValue && WasMapped == false; } }    // true if its there, and its not mapped
        public bool IsPreviouslyMapped { get { return WasMapped.HasValue && WasMapped == true; } }    // true if its there, and its mapped

        // STAR
        [PropertyNameAttribute("Short Name (K,O etc)")]
        public string StarType { get; private set; }                        // null if no StarType, direct from journal, K, A, B etc
        [PropertyNameAttribute("OBAFGAKM,LTY,AeBe,N Neutron,H Black Hole,Proto (TTS,AeBe), Wolf (W,WN,WNC,WC,WO), Carbon (CS,C,CN,CJ,CHD), White Dwarfs (D,DA,DAB,DAO,DAZ,DAV,DB,DBZ,DBV,DO,DOV,DQ,DC,DCV,DX), others")]
        public EDStar StarTypeID { get; }                           // star type -> identifier
        [PropertyNameAttribute("Long Name (Orange Main Sequence..) localised")]
        public string StarTypeText { get { return IsStar ? Bodies.StarName(StarTypeID) : ""; } }   // Long form star name, from StarTypeID, localised
        [PropertyNameAttribute("Ratio of Sol")]
        public double? nStellarMass { get; private set; }                   // direct
        public double? nAbsoluteMagnitude { get; private set; }             // direct
        [PropertyNameAttribute("Yerkes Spectral Classification")]
        public string Luminosity { get; private set; }                      // character string (I,II,.. V)
        public int? StarSubclass { get; private set; }                      // star Subclass, direct, 3.4
        [PropertyNameAttribute("StarType (long) Subclass Luminosity (K3Vab)")]
        public string StarClassification { get { return (StarType ?? "") + (StarSubclass?.ToStringInvariant() ?? "") + (Luminosity ?? ""); } }
        [PropertyNameAttribute("StarType (Abreviated) Subclass Luminosity (K3Vab)")]
        public string StarClassificationAbv { get { return new string((StarType ?? "").Where(x => char.IsUpper(x) || x == '_').Select(x => x).ToArray()) + (StarSubclass?.ToStringInvariant() ?? "") + (Luminosity ?? ""); } }
        [PropertyNameAttribute("Million Years")]
        public double? nAge { get; private set; }                           // direct, in million years

        // All orbiting bodies (Stars/Planets), not main star

        public double DistanceAccountingForBarycentre { get { return nSemiMajorAxis.HasValue && !IsOrbitingBaryCentre ? nSemiMajorAxis.Value : DistanceFromArrivalLS * BodyPhysicalConstants.oneLS_m; } } // in metres

        [PropertyNameAttribute("Meters")]
        public double? nSemiMajorAxis { get; private set; }                 // direct, m
        public double? nSemiMajorAxisAU { get { if (nSemiMajorAxis.HasValue) return nSemiMajorAxis.Value / BodyPhysicalConstants.oneAU_m; else return null; } }
        public string SemiMajorAxisLSKM { get { return nSemiMajorAxis.HasValue ? (nSemiMajorAxis >= BodyPhysicalConstants.oneLS_m / 10 ? ((nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m).ToString("N1") + "ls") : ((nSemiMajorAxis.Value / 1000).ToString("N0") + "km")) : ""; } }

        public double? nEccentricity { get; private set; }                  // direct
        [PropertyNameAttribute("Degrees")]
        public double? nOrbitalInclination { get; private set; }            // direct, degrees
        [PropertyNameAttribute("Degrees")]
        public double? nPeriapsis { get; private set; }                     // direct, degrees
        [PropertyNameAttribute("Seconds")]
        public double? nOrbitalPeriod { get; private set; }                 // direct, seconds
        public double? nOrbitalPeriodDays { get { if (nOrbitalPeriod.HasValue) return nOrbitalPeriod.Value / BodyPhysicalConstants.oneDay_s; else return null; } }
        [PropertyNameAttribute("Degrees")]
        public double? nAscendingNode { get; private set; }                  // odyssey update 7 22/9/21, degrees
        [PropertyNameAttribute("Degrees")]
        public double? nMeanAnomaly { get; private set; }                    // odyssey update 7 22/9/21, degrees

        public double? nMassKG { get { return IsPlanet ? nMassEM * BodyPhysicalConstants.oneEarth_KG : nStellarMass * BodyPhysicalConstants.oneSol_KG; } }

        public double? nAxialTilt { get; private set; }                     // direct, radians
        public double? nAxialTiltDeg { get { if (nAxialTilt.HasValue) return nAxialTilt.Value * 180.0 / Math.PI; else return null; } }
        public bool? nTidalLock { get; private set; }                       // direct

        // Planets
        [PropertyNameAttribute("Long text name from journal")]
        public string PlanetClass { get; private set; }                     // planet class, direct. If belt cluster, null. Try to avoid. Not localised
        public EDPlanet PlanetTypeID { get; }                       // planet class -> ID
        [PropertyNameAttribute("Localised Name")]
        public string PlanetTypeText { get { return IsPlanet ? Bodies.PlanetTypeName(PlanetTypeID) : ""; } }   // Use in preference to planet class for display

        public bool AmmoniaWorld { get { return Bodies.AmmoniaWorld(PlanetTypeID); } }
        public bool Earthlike { get { return Bodies.Earthlike(PlanetTypeID); } }
        public bool WaterWorld { get { return Bodies.WaterWorld(PlanetTypeID); } }
        public bool SudarskyGasGiant { get { return Bodies.SudarskyGasGiant(PlanetTypeID); } }
        public bool GasGiant { get { return Bodies.GasGiant(PlanetTypeID); } }
        public bool WaterGiant { get { return Bodies.WaterGiant(PlanetTypeID); } }
        public bool HeliumGasGiant { get { return Bodies.HeliumGasGiant(PlanetTypeID); } }
        public bool GasWorld { get { return Bodies.GasWorld(PlanetTypeID); } }          // any type of gas world

        [PropertyNameAttribute("Empty, Terraformable, Terraformed, Terraforming")]
        public string TerraformState { get; private set; }                  // direct, can be empty or a string
        public bool Terraformable { get { return TerraformState != null && new[] { "terraformable", "terraforming", "terraformed" }.Contains(TerraformState, StringComparer.InvariantCultureIgnoreCase); } }
        public bool CanBeTerraformable { get { return TerraformState != null && new[] { "terraformable", "terraforming" }.Contains(TerraformState, StringComparer.InvariantCultureIgnoreCase); } }

        public bool HasAtmosphere { get { return Atmosphere != "none"; } }  // none is used if no atmosphere
        public string Atmosphere { get; private set; }                      // processed data, always there, may be "none"
        public EDAtmosphereType AtmosphereID { get; }                       // Atmosphere -> ID (Ammonia, Carbon etc)
        public EDAtmosphereProperty AtmosphereProperty { get; private set; }  // Atomsphere -> Property (None, Rich, Thick , Thin, Hot)
        public bool HasAtmosphericComposition { get { return AtmosphereComposition != null && AtmosphereComposition.Any(); } }
        [PropertyNameAttribute("Not Searchable")]
        public Dictionary<string, double> AtmosphereComposition { get; private set; }
        public List<KeyValuePair<string, double>> SortedAtmosphereComposition()     // highest first
        {
            if (AtmosphereComposition != null)
            {
                var sorted = AtmosphereComposition.ToList();
                sorted.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));
                return sorted;
            }
            else
                return null;
        }

        [PropertyNameAttribute("Not Searchable")]
        public Dictionary<string, double> PlanetComposition { get; private set; }
        public bool HasPlanetaryComposition { get { return PlanetComposition != null && PlanetComposition.Any(); } }

        public string Volcanism { get; private set; }                       // direct from journal - can be null or a blank string
        public EDVolcanism VolcanismID { get; }                     // Volcanism -> ID (Water_Magma, Nitrogen_Magma etc)
        public bool HasMeaningfulVolcanism { get { return VolcanismID != EDVolcanism.None && VolcanismID != EDVolcanism.Unknown; } }
        public EDVolcanismProperty VolcanismProperty { get; private set; }               // Volcanism -> Property (None, Major, Minor)
        [PropertyNameAttribute("m/s")]
        public double? nSurfaceGravity { get; private set; }                // direct
        public double? nSurfaceGravityG { get { if (nSurfaceGravity.HasValue) return nSurfaceGravity.Value / BodyPhysicalConstants.oneGee_m_s2; else return null; } }
        [PropertyNameAttribute("Pascals")]
        public double? nSurfacePressure { get; private set; }               // direct
        public double? nSurfacePressureEarth { get { if (nSurfacePressure.HasValue) return nSurfacePressure.Value / BodyPhysicalConstants.oneAtmosphere_Pa; else return null; } }
        public bool? nLandable { get; private set; }                        // direct
        public bool IsLandable { get { return nLandable.HasValue && nLandable.Value; } }
        [PropertyNameAttribute("Earths")]
        public double? nMassEM { get; private set; }                        // direct, not in description of event, mass in EMs
        [PropertyNameAttribute("Moons")]
        public double? nMassMM { get { if (nMassEM.HasValue) return nMassEM * BodyPhysicalConstants.oneEarthMoonMassRatio; else return null; } }

        public bool HasMaterials { get { return Materials != null && Materials.Any(); } }
        [PropertyNameAttribute("Not Searchable")]
        public Dictionary<string, double> Materials { get; private set; }       // fdname and name is the same for materials on planets.  name is lower case
        public bool HasMaterial(string name) { return Materials != null && Materials.ContainsKey(name.ToLowerInvariant()); }
        [PropertyNameAttribute("List of materials, comma separated")]
        public string MaterialList { get { if (Materials != null) { var na = (from x in Materials select x.Key).ToArray(); return String.Join(",", na); } else return null; } }

        public EDReserve ReserveLevel { get; private set; }

        // EDD additions
        [PropertyNameAttribute("Body loaded from ESDM")]
        public bool IsEDSMBody { get; private set; }
        [PropertyNameAttribute("EDSM first commander")]
        public string EDSMDiscoveryCommander { get; private set; }      // may be null if not known
        [PropertyNameAttribute("EDSM first reported time UTC")]
        public DateTime EDSMDiscoveryUTC { get; private set; }

        public bool Mapped { get; private set; }                        // WE Mapped it - affects prices
        public bool EfficientMapped { get; private set; }               // WE efficiently mapped it - affects prices

        public void SetMapped(bool m, bool e)
        {
            Mapped = m; EfficientMapped = e;
        }

        public int EstimatedValue { get { return GetEstimatedValues().EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsEDSMBody); } }     // Direct access to its current EstimatedValue, provides backwards compatibility for code and action packs.
        public int MaximumEstimatedValue { get { return GetEstimatedValues().EstimatedValue(WasDiscovered, WasMapped, true, true,false); } }     // Direct access to its current EstimatedValue, provides backwards compatibility for code and action packs.

        public int HasSameParents(JournalScan other)     // return -1 if not, or index of last match , 0,1,2
        {
            if (Parents != null && other.Parents != null)
            {
                for (int i = 0; ; i++)
                {
                    int p1 = Parents.Count - 1 - i;
                    int p2 = other.Parents.Count - 1 - i;

                    if (p1 < 0 || p2 < 0)     // if out of parents, return how many of p2 are left
                        return i;

                    if (Parents[p1].BodyID != other.Parents[p2].BodyID || Parents[p1].Type != other.Parents[p2].Type)
                        return -1;
                }
            }
            else
                return -1;
        }

        public string ParentList() { return Parents != null ? string.Join(",", Parents.Select(x => x.Type + ":" + x.BodyID)) : ""; }     // not get on purpose

        public class StarPlanetRing
        {
            public string Name;     // may be null
            public string RingClass;    // may be null
            public double MassMT;
            public double InnerRad;
            public double OuterRad;

            // has trailing LF
            public string RingInformation(double scale = 1, string scaletype = " MT", bool parentIsStar = false, string frontpad = "  ")
            {
                StringBuilder scanText = new StringBuilder();
                scanText.AppendFormat(frontpad + "{0} ({1})\n", Name.Alt("Unknown".T(EDCTx.Unknown)), DisplayStringFromRingClass(RingClass));
                scanText.AppendFormat(frontpad + "Mass: {0:N4}{1}\n".T(EDCTx.StarPlanetRing_Mass), MassMT * scale, scaletype);
                if (parentIsStar && InnerRad > 3000000)
                {
                    scanText.AppendFormat(frontpad + "Inner Radius: {0:0.00}ls".T(EDCTx.StarPlanetRing_InnerRadius) + Environment.NewLine, (InnerRad / BodyPhysicalConstants.oneLS_m));
                    scanText.AppendFormat(frontpad + "Outer Radius: {0:0.00}ls".T(EDCTx.StarPlanetRing_OuterRadius) + Environment.NewLine, (OuterRad / BodyPhysicalConstants.oneLS_m));
                }
                else
                {
                    scanText.AppendFormat(frontpad + "Inner Radius: {0}km".T(EDCTx.StarPlanetRing_IK) + Environment.NewLine, (InnerRad / 1000).ToString("N0"));
                    scanText.AppendFormat(frontpad + "Outer Radius: {0}km".T(EDCTx.StarPlanetRing_OK) + " \u0394 {1}" + Environment.NewLine, (OuterRad / 1000).ToString("N0"), ((OuterRad-InnerRad)/1000).ToString("N0"));
                }
                return scanText.ToNullSafeString();
            }

            // has trailing LF
            public string RingInformationMoons(bool parentIsStar = false, string frontpad = "  ")
            {
                // mega ton is 1E6 tons = 1E9
                return RingInformation(1 / BodyPhysicalConstants.oneMoon_KG / 1E9, " Moons".T(EDCTx.StarPlanetRing_Moons), parentIsStar, frontpad);
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

        public class BodyParent
        {
            public string Type;
            public int BodyID;
            public bool BaryCentre { get { return Type.Equals("Null", StringComparison.InvariantCultureIgnoreCase); } }
        }

        public JournalScan(JObject evt) : base(evt, JournalTypeEnum.Scan)
        {
            ScanType = evt["ScanType"].Str();                               // ALL
            BodyName = evt["BodyName"].Str();                               // ALL
            BodyID = evt["BodyID"].IntNull();                               // ALL
            StarSystem = evt["StarSystem"].StrNull();                       // ALL    
            SystemAddress = evt["SystemAddress"].LongNull();                // ALL    
            DistanceFromArrivalLS = evt["DistanceFromArrivalLS"].Double();  // ALL 
            WasDiscovered = evt["WasDiscovered"].BoolNull();                // ALL new 3.4
            WasMapped = evt["WasMapped"].BoolNull();                        // ALL new 3.4

            JArray parents = evt["Parents"].Array();                        // ALL will be null if parents is not an array (also if its Null)
            if (!parents.IsNull() && parents.IsArray)
            {
                Parents = new List<BodyParent>();

                foreach (JObject parent in parents)
                {
                    if (parent.IsObject)
                    {
                        foreach (var kvp in parent)
                        {
                            Parents.Add(new BodyParent { Type = kvp.Key, BodyID = kvp.Value.Int() });
                        }
                    }
                }
            }

            nRotationPeriod = evt["RotationPeriod"].DoubleNull();           // Stars/Planets, not belt clusters
            nSurfaceTemperature = evt["SurfaceTemperature"].DoubleNull();   // Stars/Planets, not belt clusters
            nRadius = evt["Radius"].DoubleNull();                           // Stars/Planets, not belt clusters    

            Rings = evt["Rings"]?.ToObjectQ<StarPlanetRing[]>();            // Stars/Planets, may be Null, not belt clusters

            StarType = evt["StarType"].StrNull();                           // stars have this field

            if (IsStar)     // based on StarType
            {
                StarTypeID = Bodies.StarStr2Enum(StarType);

                nStellarMass = evt["StellarMass"].DoubleNull();
                nAbsoluteMagnitude = evt["AbsoluteMagnitude"].DoubleNull();
                Luminosity = evt["Luminosity"].StrNull();
                StarSubclass = evt["Subclass"].IntNull();
                nAge = evt["Age_MY"].DoubleNull();

            }
            else
                PlanetClass = evt["PlanetClass"].StrNull();                 // try and read planet class, this might be null as well, in which case its a belt cluster

            // All orbiting bodies

            nSemiMajorAxis = evt["SemiMajorAxis"].DoubleNull();             // Stars/Planets

            if (nSemiMajorAxis.HasValue)
            {
                nEccentricity = evt["Eccentricity"].DoubleNull();       // keplarian values..
                nOrbitalInclination = evt["OrbitalInclination"].DoubleNull();
                nPeriapsis = evt["Periapsis"].DoubleNull();
                nMeanAnomaly = evt["MeanAnomaly"].DoubleNull();         // Odyssey rel 7 onwards
                nAscendingNode = evt["AscendingNode"].DoubleNull();     // Odyssey rel 7 onwards

                nOrbitalPeriod = evt["OrbitalPeriod"].DoubleNull();     // will allow central mass to be estimated if required
                nAxialTilt = evt["AxialTilt"].DoubleNull();
                nTidalLock = evt["TidalLock"].Bool();
            }

            if (IsPlanet)
            {
                PlanetTypeID = Bodies.PlanetStr2Enum(PlanetClass);
                // Fix naming to standard and fix case..
                PlanetClass = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.
                                        ToTitleCase(PlanetClass.ToLowerInvariant()).Replace("Ii ", "II ").Replace("Iv ", "IV ").Replace("Iii ", "III ");

                TerraformState = evt["TerraformState"].StrNull();
                if (TerraformState != null && TerraformState.Equals("Not Terraformable", StringComparison.InvariantCultureIgnoreCase)) // EDSM returns this, normalise to journal
                    TerraformState = String.Empty;



                JToken atmos = evt["AtmosphereComposition"];
                if (!atmos.IsNull())
                {
                    if (atmos.IsObject)
                    {
                        AtmosphereComposition = atmos?.ToObjectQ<Dictionary<string, double>>();
                    }
                    else if (atmos.IsArray)
                    {
                        AtmosphereComposition = new Dictionary<string, double>();
                        foreach (JObject jo in atmos)
                        {
                            AtmosphereComposition[jo["Name"].Str("Default")] = jo["Percent"].Double();
                        }
                    }
                }

                Atmosphere = evt["Atmosphere"].StrNull();               // can be null, or empty

                if ( Atmosphere == "thick  atmosphere" )            // obv a frontier bug, atmosphere type has the missing text
                {
                    Atmosphere = "thick " + evt["AtmosphereType"].Str().SplitCapsWord() + " atmosphere";
                }
                else if ( Atmosphere == "thin  atmosphere")             
                {
                    Atmosphere = "thin " + evt["AtmosphereType"].Str().SplitCapsWord() + " atmosphere";
                }
                else if ( Atmosphere.IsEmpty())                         // try type.
                    Atmosphere = evt["AtmosphereType"].StrNull();       // it may still be null here or empty string

                if (Atmosphere.IsEmpty())       // null or empty - nothing in either, see if there is composition
                {
                    if ((AtmosphereComposition?.Count ?? 0) > 0)    // if we have some composition, synthesise name
                    {
                        foreach( var e in Enum.GetNames(typeof(EDAtmosphereType)))
                        {
                            if ( AtmosphereComposition.ContainsKey(e.ToString()))       // pick first match in ID
                            {
                                Atmosphere = e.ToString().SplitCapsWord().ToLower();
                             //   System.Diagnostics.Debug.WriteLine("Computed Atmosphere '" + Atmosphere + "'");
                                break;
                            }
                        }
                    }

                    if ( Atmosphere.IsEmpty())          // still nothing, set to None
                        Atmosphere = "none";
                }
                else
                {
                    Atmosphere = Atmosphere.Replace("sulfur", "sulphur").SplitCapsWord().ToLower();      // fix frontier spelling mistakes
                 //   System.Diagnostics.Debug.WriteLine("Atmosphere '" + Atmosphere + "'");
                }

                //System.IO.File.AppendAllText(@"c:\code\atmos.txt", $"Atmosphere {evt["Atmosphere"]} type {evt["AtmosphereType"]} => {Atmosphere}\r\n");

                System.Diagnostics.Debug.Assert(Atmosphere.HasChars());

                AtmosphereID = Bodies.AtmosphereStr2Enum(Atmosphere.ToLower(), out EDAtmosphereProperty ap);  // convert to internal ID
                AtmosphereProperty = ap;

                if (AtmosphereID == EDAtmosphereType.Unknown)
                {
                    System.Diagnostics.Debug.WriteLine("*** Atmos not recognised {0} '{1}' '{2}'", Atmosphere, evt["Atmosphere"].Str(), evt["AtmosphereType"].Str());
                }

                JObject composition = evt["Composition"].Object();
                if (!composition.IsNull() && composition.IsObject)
                {
                    PlanetComposition = new Dictionary<string, double>();
                    foreach (var kvp in composition)
                    {
                        PlanetComposition[kvp.Key] = kvp.Value.Double();
                    }
                }

                Volcanism = evt["Volcanism"].StrNull();
                VolcanismID = Bodies.VolcanismStr2Enum(Volcanism, out EDVolcanismProperty vp);
                VolcanismProperty = vp;

                nSurfaceGravity = evt["SurfaceGravity"].DoubleNull();
                nSurfacePressure = evt["SurfacePressure"].DoubleNull();
                nLandable = evt["Landable"].BoolNull();
                nMassEM = evt["MassEM"].DoubleNull();

                JToken mats = evt["Materials"];
                if (mats != null)
                {
                    if (mats.IsObject)
                    {
                        Materials = mats.ToObjectQ<Dictionary<string, double>>();  // name in fd logs is lower case
                    }
                    else if (mats.IsArray)
                    {
                        Materials = new Dictionary<string, double>();
                        foreach (JObject jo in mats)                                        // name in fd logs is lower case
                        {
                            Materials[jo["Name"].Str("Default").ToLowerInvariant()] = jo["Percent"].Double();
                        }
                    }
                }

                ReserveLevel = Bodies.ReserveStr2Enum(evt["ReserveLevel"].Str());
            }
            else
                PlanetTypeID = EDPlanet.Unknown_Body_Type;

            // EDSM bodies fields

            IsEDSMBody = evt["EDDFromEDSMBodie"].Bool(false);           // Bodie? Who is bodie?  Did you mean Body Finwen ;-)

            JToken discovery = evt["discovery"];
            if (!discovery.IsNull())
            {
                EDSMDiscoveryCommander = discovery["commander"].StrNull();
                EDSMDiscoveryUTC = discovery["date"].DateTimeUTC();
            }
        }

        // special, for star scan, create a scan record with the contents of the journal scan bary centre info
        public JournalScan(JournalScanBaryCentre js) : base(DateTime.Now, JournalTypeEnum.ScanBaryCentre, false)
        {
            BodyID = js.BodyID;
            nSemiMajorAxis = js.SemiMajorAxis;
            nEccentricity = js.Eccentricity;
            nOrbitalInclination = js.OrbitalInclination;
            nPeriapsis = js.Periapsis;
            nOrbitalPeriod = js.OrbitalPeriod;
            nAscendingNode = js.AscendingNode;
            nMeanAnomaly = js.MeanAnomaly;
        }

        #region Information Returns

        public string RadiusText()  // null if not set, or the best representation
        {
            if (nRadius != null)
            {
                if (nRadius >= BodyPhysicalConstants.oneSolRadius_m / 5)
                    return nRadiusSols.Value.ToString("0.#" + " SR");
                else
                    return (nRadius.Value / 1000).ToString("0.#") + " km";
            }
            else
                return null;
        }

        public string MassEMText()
        {
            if (nMassEM.HasValue)
            {
                if (nMassEM.Value < 0.01)
                    return nMassMM.Value.ToString("0.####") + " MM";
                else
                    return nMassEM.Value.ToString("0.##") + " EM";
            }
            else
                return null;
        }


        public override string SummaryName(ISystem sys)
        {
            string text = "Scan of {0}".T(EDCTx.JournalScan_Scanof);
            if (ScanType == "AutoScan")
                text = "Autoscan of {0}".T(EDCTx.JournalScan_Autoscanof);
            else if (ScanType == "Detailed")
                text = "Detailed scan of {0}".T(EDCTx.JournalScan_Detailedscanof);
            else if (ScanType == "Basic")
                text = "Basic scan of {0}".T(EDCTx.JournalScan_Basicscanof);
            else if (ScanType.Contains("Nav"))
                text = "Nav scan of {0}".T(EDCTx.JournalScan_Navscanof);

            return string.Format(text, BodyName.ReplaceIfStartsWith(sys.Name));
        }

        [PropertyNameAttribute("Scan Auto, Scan Basic, Scan Nav, Scan")]
        public override string EventFilterName
        {
            get
            {
                if (ScanType == "AutoScan")
                    return "Scan Auto";
                else if (ScanType == "Basic")
                    return "Scan Basic";
                else if (ScanType.Contains("Nav"))
                    return "Scan Nav";
                else
                    return base.EventFilterName;
            }
        }

        static public List<Tuple<string, string, Image>> FilterItems()
        {
            return new List<Tuple<string, string, Image>>()
            {
                new Tuple<string, string,Image>( "Scan Auto", "Scan Auto".T(EDCTx.JournalScan_ScanAuto), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
                new Tuple<string,string,Image>( "Scan Basic", "Scan Basic".T(EDCTx.JournalScan_ScanBasic), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
                new Tuple<string,string,Image>( "Scan Nav", "Scan Nav".T(EDCTx.JournalScan_ScanNav), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
            };
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            if (IsStar)
            {
                info = BaseUtils.FieldBuilder.Build("", StarTypeText, "Mass: ;SM;0.00".T(EDCTx.JournalScan_MSM), nStellarMass,
                                                "Age: ;my;0.0".T(EDCTx.JournalScan_Age), nAge,
                                                "Radius: ".T(EDCTx.JournalScan_RS), RadiusText(),
                                                "Dist: ;ls;0.0".T(EDCTx.JournalScan_DISTA), DistanceFromArrivalLS,
                                                "Name: ".T(EDCTx.JournalScan_BNME), BodyName.ReplaceIfStartsWith(sys.Name));
            }
            else
            {
                info = BaseUtils.FieldBuilder.Build("", PlanetTypeText, "Mass: ".T(EDCTx.JournalScan_MASS), MassEMText(),
                                                "<;, Landable".T(EDCTx.JournalScan_Landable), IsLandable,
                                                "<;, Terraformable".T(EDCTx.JournalScan_Terraformable), TerraformState == "Terraformable", "", Atmosphere,
                                                 "Gravity: ;G;0.00".T(EDCTx.JournalScan_Gravity), nSurfaceGravityG,
                                                 "Radius: ".T(EDCTx.JournalScan_RS), RadiusText(),
                                                 "Dist: ;ls;0.0".T(EDCTx.JournalScan_DISTA), DistanceFromArrivalLS,
                                                 "Name: ".T(EDCTx.JournalScan_SNME), BodyName.ReplaceIfStartsWith(sys.Name));
            }

            detailed = DisplayString(0, includefront: false);
        }

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
                scanText.AppendFormat("{0} {1}" + Environment.NewLine, BodyName, IsEDSMBody ? " (EDSM)" : "");
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
                    scanText.AppendFormat("Mass: ".T(EDCTx.JournalScan_MASS) + " " + MassEMText() + "\n");

                if (nRadius.HasValue)
                    scanText.AppendFormat("Radius: ".T(EDCTx.JournalScan_RS) + " " + RadiusText() + "\n");

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
                scanText.AppendFormat("Orbital Eccentricity: {0:0.000}\n".T(EDCTx.JournalScan_OrbitalEccentricity), nEccentricity.Value);

            if (nOrbitalInclination.HasValue)
                scanText.AppendFormat("Orbital Inclination: {0:0.000}°\n".T(EDCTx.JournalScan_OrbitalInclination), nOrbitalInclination.Value);

            if (nAscendingNode.HasValue)
                scanText.AppendFormat("Ascending Node: {0:0.000}°\n".T(EDCTx.JournalScan_AscendingNode), nAscendingNode.Value);

            if (nPeriapsis.HasValue)
                scanText.AppendFormat("Arg Of Periapsis: {0:0.000}°\n".T(EDCTx.JournalScan_ArgOfPeriapsis), nPeriapsis.Value);

            if ( nMeanAnomaly.HasValue)
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
                if (IsStar)
                {
                    scanText.AppendFormat(Rings.Count() == 1 ? "Belt".T(EDCTx.JournalScan_Belt) : "Belts".T(EDCTx.JournalScan_Belts), ""); // OLD translator files had "Belt{0}" so supply an empty string just in case
                    for (int i = 0; i < Rings.Length; i++)
                    {
                        if (Rings[i].MassMT > (BodyPhysicalConstants.oneMoon_KG / 1e9/ 10000))
                        {
                            // its in mega tons, so convert KG into mega (1E6) tons
                            scanText.Append("\n" + RingInformation(i, 1.0 / (BodyPhysicalConstants.oneMoon_KG /1E9), " Moons".T(EDCTx.JournalScan_Moons)));
                        }
                        else
                        {
                            scanText.Append("\n" + RingInformation(i));
                        }
                    }
                }
                else
                {
                    scanText.AppendFormat(Rings.Count() == 1 ? "Ring".T(EDCTx.JournalScan_Ring) : "Rings".T(EDCTx.JournalScan_Rings), ""); // OLD translator files had "Rings{0}" so supply an empty string just in case

                    for (int i = 0; i < Rings.Length; i++)
                        scanText.Append("\n" + RingInformation(i));
                }
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

            scanText.AppendFormat("Current value: {0:N0}".T(EDCTx.JournalScan_CV) + "\n", ev.EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsEDSMBody));

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

            scanText.AppendFormat("Scan Type: {0}".T(EDCTx.JournalScan_SCNT) + "\n", ScanType);

            //scanText.AppendFormat("BID+Parents: {0} - {1}\n", BodyID ?? -1, ParentList());

            if (scanText.Length > 0 && scanText[scanText.Length - 1] == '\n')
                scanText.Remove(scanText.Length - 1, 1);

            return scanText.ToNullSafeString().Replace("\n", "\n" + inds);
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


        // show material counts at the historic point and current.  Has trailing LF if text present.
        public string DisplayMaterials(int indent = 0, List<MaterialCommodityMicroResource> historicmatlist = null, List<MaterialCommodityMicroResource> currentmatlist = null)
        {
            StringBuilder scanText = new StringBuilder();

            if (HasMaterials)
            {
                string indents = new string(' ', indent);

                scanText.Append("Materials:\n".T(EDCTx.JournalScan_Materials));
                foreach (KeyValuePair<string, double> mat in Materials)
                {
                    scanText.Append(indents + DisplayMaterial(mat.Key, mat.Value, historicmatlist, currentmatlist));
                }
            }

            return scanText.ToNullSafeString();
        }

        public string DisplayMaterial(string fdname, double percent, List<MaterialCommodityMicroResource> historicmatlist = null,
                                                                      List<MaterialCommodityMicroResource> currentmatlist = null)  // has trailing LF
        {
            StringBuilder scanText = new StringBuilder();

            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);

            if (mc != null && (historicmatlist != null || currentmatlist != null))
            {
                MaterialCommodityMicroResource historic = historicmatlist?.Find(x => x.Details == mc);
                MaterialCommodityMicroResource current = ReferenceEquals(historicmatlist, currentmatlist) ? null : currentmatlist?.Find(x => x.Details == mc);
                int? limit = mc.MaterialLimit();

                string matinfo = historic?.Count.ToString() ?? "0";
                if (limit != null)
                    matinfo += "/" + limit.Value.ToString();

                if (current != null && (historic == null || historic.Count != current.Count))
                    matinfo += " Cur " + current.Count.ToString();

                scanText.AppendFormat("{0} ({1}) {2} {3}% {4}\n", mc.Name, mc.Shortname, mc.TranslatedType, percent.ToString("N1"), matinfo);
            }
            else
                scanText.AppendFormat("{0} {1}%\n", System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fdname.ToLowerInvariant()),
                                                            percent.ToString("N1"));

            return scanText.ToNullSafeString();
        }

        // adds to mats hash (if required) if one found.
        // returns number of jumponiums in body
        public int Jumponium(HashSet<string> mats = null)
        {
            int count = 0;

            foreach (var m in Materials.EmptyIfNull())
            {
                string n = m.Key.ToLowerInvariant();
                if (MaterialCommodityMicroResourceType.IsJumponiumType(n))
                {
                    count++;
                    if (mats != null && !mats.Contains(n))      // and we have not counted it
                    {
                        mats.Add(n);
                    }
                }
            }

            return count;
        }

        private string DisplayAtmosphere(int indent = 0)     // has trailing LF
        {
            StringBuilder scanText = new StringBuilder();
            string indents = new string(' ', indent);

            scanText.Append("Atmospheric Composition:\n".T(EDCTx.JournalScan_AtmosphericComposition));
            foreach (KeyValuePair<string, double> comp in AtmosphereComposition)
            {
                scanText.AppendFormat(indents + "{0} - {1}%\n", comp.Key, comp.Value.ToString("N2"));
            }

            return scanText.ToNullSafeString();
        }

        private string DisplayComposition(int indent = 0)   // has trailing LF
        {
            StringBuilder scanText = new StringBuilder();
            string indents = new string(' ', indent);

            scanText.Append("Planetary Composition:\n".T(EDCTx.JournalScan_PlanetaryComposition));
            foreach (KeyValuePair<string, double> comp in PlanetComposition)
            {
                if (comp.Value > 0)
                    scanText.AppendFormat(indents + "{0} - {1}%\n", comp.Key, (comp.Value * 100).ToString("N2"));
            }

            return scanText.ToNullSafeString();
        }

        // Has Trailing LF
        private string RingInformation(int ringno, double scale = 1, string scaletype = " MT")
        {
            StarPlanetRing ring = Rings[ringno];
            return ring.RingInformation(scale, scaletype, IsStar);
        }

        public StarPlanetRing FindRing(string name)
        {
            if (Rings != null)
                return Array.Find(Rings, x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            else
                return null;
        }

        public double GetMaterial(string v)
        {
            if (Materials == null)
                return 0.0;

            if (!Materials.ContainsKey(v.ToLowerInvariant()))
                return 0.0;

            return Materials[v.ToLowerInvariant()];
        }

        public double? GetAtmosphereComponent(string c)
        {
            if (!HasAtmosphericComposition)
                return null;

            if (!AtmosphereComposition.ContainsKey(c))
                return 0.0;

            return AtmosphereComposition[c];

        }

        public double? GetCompositionPercent(string c)
        {
            if (!HasPlanetaryComposition)
                return null;

            if (!PlanetComposition.ContainsKey(c))
                return 0.0;

            return PlanetComposition[c] * 100;
        }

        // is the name in starname 

        public bool IsStarNameRelated(string starname,  long? sysaddr, string designation )
        {
            if (StarSystem != null && SystemAddress != null && sysaddr != null)     // if we are a star system AND sysaddr in the scan is set, and we got passed a system addr, direct compare
            {
                return starname.Equals(StarSystem, StringComparison.InvariantCultureIgnoreCase) && sysaddr == SystemAddress;    // star is the same name and sys addr same
            }

            if (designation.Length >= starname.Length)      // and compare against starname root
            {
                string s = designation.Substring(0, starname.Length);
                return starname.Equals(s, StringComparison.InvariantCultureIgnoreCase);
            }
            else
                return false;
        }

        public string IsStarNameRelatedReturnRest(string starname, long? sysaddr )          // null if not related, else rest of string
        {
            string designation = BodyDesignation ?? BodyName;
            string desigrest = null;

            if (this.StarSystem != null && this.SystemAddress != null && sysaddr != null)
            {
                if (starname != this.StarSystem || sysaddr != this.SystemAddress)
                {
                    return null;        // no relationship between starname and system in JE
                }

                desigrest = designation;
            }

            if (designation.Length >= starname.Length)
            {
                string s = designation.Substring(0, starname.Length);
                if (starname.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    desigrest = designation.Substring(starname.Length).Trim();
            }

            return desigrest;
        }


        #endregion

        #region Estimated Value

        private ScanEstimatedValues EstimatedValues = null;

        public ScanEstimatedValues GetEstimatedValues()
        {
            if (EstimatedValues == null)
                EstimatedValues = new ScanEstimatedValues(EventTimeUTC, IsStar, StarTypeID, IsPlanet, PlanetTypeID, Terraformable, nStellarMass, nMassEM, IsOdyssey);
            return EstimatedValues;
        }

        public ScanEstimatedValues RecalcEstimatedValues()
        {
            return new ScanEstimatedValues(EventTimeUTC, IsStar, StarTypeID, IsPlanet, PlanetTypeID, Terraformable, nStellarMass, nMassEM, IsOdyssey);
        }

        public void AddStarScan(StarScan s, ISystem system)     // no action in this class, historylist.cs does the adding itself instead of using this. 
        {                                                       // Class interface is marked so you know its part of the gang
        }
    }

    #endregion

    public class ScansAreForSameBody : EqualityComparer<JournalScan>
    {
        public override bool Equals(JournalScan x, JournalScan y)
        {
            return x.BodyName == y.BodyName;
        }

        public override int GetHashCode(JournalScan obj)
        {
            return obj.BodyName.GetHashCode();
        }
    }

}


