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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    // JSON export thru ZMQ/DLLs/Web

    [System.Diagnostics.DebuggerDisplay("Event {EventTypeStr} {EventTimeUTC} {BodyName} {BodyType}")]
    [JournalEntryType(JournalTypeEnum.Scan)]
    public partial class JournalScan : JournalEntry, IStarScan, IBodyFeature
    {
        // we can have scans of Planets, Stars, AsteroidClusters,Planetary rings, Stellar rings as of Nov 25
        [PropertyNameAttribute("Is it a star")]
        public bool IsStar => BodyType == BodyDefinitions.BodyType.Star;
        [PropertyNameAttribute("Is it a belt cluster body")]
        public bool IsBeltClusterBody => BodyType == BodyDefinitions.BodyType.AsteroidCluster;
        [PropertyNameAttribute("Is it a planetary ring")]
        public bool IsPlanetaryRing => BodyType == BodyDefinitions.BodyType.PlanetaryRing;
        [PropertyNameAttribute("Is it a stellar ring")]
        public bool IsStellarRing => BodyType == BodyDefinitions.BodyType.StellarRing;
        [PropertyNameAttribute("Is it a planet")]
        public bool IsPlanet => BodyType == BodyDefinitions.BodyType.Planet;

        ////////////////////////////////////////////////////////////////////// ALL

        [PropertyNameAttribute("Scan type, Basic, Detailed, NavBeacon, NavBeaconDetail, AutoScan, may be empty for very old scans")]
        public string ScanType { get; private set; }                        // 3.0 scan type  Basic, Detailed, NavBeacon, NavBeaconDetail, (3.3) AutoScan, or empty for older ones
        [PropertyNameAttribute("Frontier body name")]
        public string BodyName { get; private set; }                        // direct (meaning no translation)

        [PropertyNameAttribute("Basic type, Planet, Star, Ring")]
        public BodyDefinitions.BodyType BodyType { get; private set; }

        [PropertyNameAttribute("Internal Frontier ID, is empty for older scans")]
        public int? BodyID { get; private set; }                            // direct
        [PropertyNameAttribute("Name of star")]
        public string StarSystem { get; private set; }                      // direct (3.5)
        [PropertyNameAttribute("Internal Frontier ID, is empty for older scans")]
        public long? SystemAddress { get; private set; }                    // direct (3.5)
        [PropertyNameAttribute("In light seconds")]
        public double DistanceFromArrivalLS { get; private set; }           // direct
        [PropertyNameAttribute("In meters")]
        public double DistanceFromArrivalm { get { return DistanceFromArrivalLS * BodyPhysicalConstants.oneLS_m; } }
        [PropertyNameAttribute("Astronomic Unit (AU) and Ls")]
        public string DistanceFromArrivalText { get { return string.Format("{0:N2}AU ({1:N1}ls)", DistanceFromArrivalLS / BodyPhysicalConstants.oneAU_LS, DistanceFromArrivalLS); } }
        [PropertyNameAttribute("Seconds, negatives indicating reverse rotation")]
        public double? nRotationPeriod { get; private set; }                // direct, can be negative indi
        [PropertyNameAttribute("Days, will always be positive")]
        public double? nRotationPeriodDays { get { if (nRotationPeriod.HasValue) return Math.Abs(nRotationPeriod.Value) / BodyPhysicalConstants.oneDay_s; else return null; } }
        [PropertyNameAttribute("K")]
        public double? nSurfaceTemperature { get; private set; }            // direct
        [PropertyNameAttribute("Meters")]
        public double? nRadius { get; private set; }                        // direct
        [PropertyNameAttribute("km")]
        public double? nRadiusKM { get { if (nRadius.HasValue) return nRadius.Value / 1000.0; else return null; } }
        [PropertyNameAttribute("Radius in units of Sol")]
        public double? nRadiusSols { get { if (nRadius.HasValue) return nRadius.Value / BodyPhysicalConstants.oneSolRadius_m; else return null; } }
        [PropertyNameAttribute("Radius in units of Earth")]
        public double? nRadiusEarths { get { if (nRadius.HasValue) return nRadius.Value / BodyPhysicalConstants.oneEarthRadius_m; else return null; } }
        [PropertyNameAttribute(null)]       // not useful for queries etc. Return in solar or km
        public string RadiusText { get { if (nRadius != null) { if (nRadius >= BodyPhysicalConstants.oneSolRadius_m / 5) return nRadiusSols.Value.ToString("0.#" + " SR"); else return (nRadius.Value / 1000).ToString("0.#") + " km"; } else return null; } }

        [PropertyNameAttribute("Does the body have rings or belts")]
        public bool HasRingsOrBelts { get { return Rings != null && Rings.Length > 0; } }
        [PropertyNameAttribute("Does the body have belts")]
        public bool HasBelts { get { return HasRingsOrBelts && Rings[0].Name.Contains("Belt"); } }
        [PropertyNameAttribute("Does the body have rings")]
        public bool HasRings { get { return HasRingsOrBelts && !Rings[0].Name.Contains("Belt"); } }

        [PropertyNameAttribute("Ring information")]
        public StarPlanetRing[] Rings { get; private set; }

        [PropertyNameAttribute("Distance of inner ring from planet, Meters, null if no rings")]
        public double? RingsInnerm { get { if (Rings != null) return Rings.Select(x => x.InnerRad).Min(); else return null; } }
        [PropertyNameAttribute("Distance of final outer ring from planet, Meters, null if no rings")]
        public double? RingsOuterm { get { if (Rings != null) return Rings.Select(x => x.OuterRad).Max(); else return null; } }
        [PropertyNameAttribute("Max ring width, Meters, 0 if no rings")]
        public double? RingsMaxWidth { get { if (Rings != null) return Rings.Select(x => x.Width).Max(); else return 0; } }
        [PropertyNameAttribute("Min ring width, Meters, 0 if no rings")]
        public double? RingsMinWidth { get { if (Rings != null) return Rings.Select(x => x.Width).Min(); else return 0; } }

        [PropertyNameAttribute("Parent body information. First is nearest body")]
        public List<BodyParent> Parents { get; private set; }               // NOTE Augmented by StarScan setting Parents[i].Barycentre
        public string ParentList() { return Parents != null ? string.Join(",", Parents.Select(x => x.Type + ":" + x.BodyID)) : ""; }     // not get on purpose

        [PropertyNameAttribute("Orbiting a barycentre")]
        public bool IsOrbitingBarycentre { get { return Parents?.FirstOrDefault()?.Type == BodyParent.ParentBodyType.Null; } }
        [PropertyNameAttribute("Orbiting a planet")]
        public bool IsOrbitingPlanet { get { return BodyParent.IsOrbiting(Parents, BodyParent.ParentBodyType.Planet); } }
        [PropertyNameAttribute("Orbiting a star")]
        public bool IsOrbitingStar { get { return BodyParent.IsOrbiting(Parents, BodyParent.ParentBodyType.Star); ; } }

        [PropertyNameAttribute("No Parents or All Parents are barycentres")]
        public bool IsAllParentsBarycentre { get { return Parents == null || Parents.Count(x => x.IsBarycentre) == Parents.Count; } }
  
        [PropertyNameAttribute("Has the body been previously discovered - older scans do not have this field")]
        public bool? WasDiscovered { get; private set; }                    // direct, 3.4, indicates whether the body has already been discovered
        [PropertyNameAttribute("Has the body not been previously discovered")]
        public bool IsNotPreviouslyDiscovered { get { return WasDiscovered.HasValue && WasDiscovered == false; } } // true if its there, and its not mapped
        [PropertyNameAttribute("Has the body been previously discovered")]
        public bool IsPreviouslyDiscovered { get { return WasDiscovered.HasValue && WasDiscovered == true; } } // true if its there, and its discovered

        [PropertyNameAttribute("Has the body been previously mapped - older scans do not have this field")]
        public bool? WasMapped { get; private set; }                        // direct, 3.4, indicates whether the body has already been mapped
        [PropertyNameAttribute("Has the body not been previously mapped")]
        public bool IsNotPreviouslyMapped { get { return WasMapped.HasValue && WasMapped == false; } }    // true if its there, and its not mapped
        [PropertyNameAttribute("Has the body been previously mapped")]
        public bool IsPreviouslyMapped { get { return WasMapped.HasValue && WasMapped == true; } }    // true if its there, and its mapped

        [PropertyNameAttribute("Was there a previous footfall on the body")]
        public bool? WasFootfalled { get; private set; }

        [PropertyNameAttribute("Mass of star or planet in KG")]
        public double? nMassKG { get { return IsPlanet ? nMassEM * BodyPhysicalConstants.oneEarth_KG : nStellarMass * BodyPhysicalConstants.oneSol_KG; } }

        ////////////////////////////////////////////////////////////////////// STAR

        [PropertyNameAttribute("Short Name (K,O etc)")]
        public string StarType { get; private set; }                        // null if no StarType, direct from journal, K, A, B etc
        [PropertyNameAttribute("OBAFGAKM,LTY,AeBe,N Neutron,H Black Hole,Proto (TTS,AeBe), Wolf (W,WN,WNC,WC,WO), Carbon (CS,C,CN,CJ,CHD), White Dwarfs (D,DA,DAB,DAO,DAZ,DAV,DB,DBZ,DBV,DO,DOV,DQ,DC,DCV,DX), others")]
        public EDStar StarTypeID { get; }                           // star type -> identifier
        [PropertyNameAttribute("Long Name (Orange Main Sequence..) localised")]
        public string StarTypeText { get { return IsStar ? Stars.ToLocalisedLanguage(StarTypeID) : ""; } }   // Long form star name, from StarTypeID, localised
        [PropertyNameAttribute("Is it the main star")]
        public bool IsMainStar { get { return IsStar && (BodyName == StarSystem || BodyName == StarSystem + " A"); } }
        [PropertyNameAttribute("Ratio of Sol")]
        public double? nStellarMass { get; private set; }                   // direct
        [PropertyNameAttribute("Absolute magnitude (less is more, Sol = 4.83)")]
        public double? nAbsoluteMagnitude { get; private set; }             // direct
        [PropertyNameAttribute("Absolute magnitude referenced to Sol (0=Sol)")]     // From https://en.wikipedia.org/wiki/Absolute_magnitude and from the journal Sol entry
        public double? nAbsoluteMagnitudeSol { get { return nAbsoluteMagnitude != null ? nAbsoluteMagnitude - 4.829987 : null; } }    
        [PropertyNameAttribute("Yerkes Spectral Classification, may not be present")]
        public string Luminosity { get; private set; }                      // character string (I,II,.. V)
        [PropertyNameAttribute("Star subclass number, may not be present")]
        public int? StarSubclass { get; private set; }                      // star Subclass, direct, 3.4
        [PropertyNameAttribute("StarType (long) Subclass Luminosity (K3Vab)")]
        public string StarClassification { get { return (StarType ?? "") + (StarSubclass?.ToStringInvariant() ?? "") + (Luminosity ?? ""); } }
        [PropertyNameAttribute("StarType (Abreviated) Subclass Luminosity (K3Vab)")]
        public string StarClassificationAbv { get { return new string((StarType ?? "").Where(x => char.IsUpper(x) || x == '_').Select(x => x).ToArray()) + (StarSubclass?.ToStringInvariant() ?? "") + (Luminosity ?? ""); } }
        [PropertyNameAttribute("Million Years")]
        public double? nAge { get; private set; }                           // direct, in million years

        ////////////////////////////////////////////////////////////////////// All orbiting bodies (Stars/Planets), not main star

        [PropertyNameAttribute("Estimated distance allowing for a barycentre, m")]
        public double DistanceAccountingForBarycentre { get { return nSemiMajorAxis.HasValue && !IsOrbitingBarycentre ? nSemiMajorAxis.Value : DistanceFromArrivalLS * BodyPhysicalConstants.oneLS_m; } } // in metres

        [PropertyNameAttribute("Meters")]
        public double? nSemiMajorAxis { get; private set; }                 // direct, m
        [PropertyNameAttribute("AU")]
        public double? nSemiMajorAxisAU { get { if (nSemiMajorAxis.HasValue) return nSemiMajorAxis.Value / BodyPhysicalConstants.oneAU_m; else return null; } }
        [PropertyNameAttribute("Light seconds")]
        public double? nSemiMajorAxisLS { get { if (nSemiMajorAxis.HasValue) return nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m; else return null; } }
        [PropertyNameAttribute("LS is greater than .1 ls, else KM")]
        public string SemiMajorAxisLSKM { get { return nSemiMajorAxis.HasValue ? (nSemiMajorAxis >= BodyPhysicalConstants.oneLS_m / 10 ? ((nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m).ToString("N1") + "ls") : ((nSemiMajorAxis.Value / 1000).ToString("N0") + "km")) : ""; } }

        [PropertyNameAttribute("Eccentricity of orbit")]
        public double? nEccentricity { get; private set; }                  // direct
        [PropertyNameAttribute("Degrees")]
        public double? nOrbitalInclination { get; private set; }            // direct, degrees
        [PropertyNameAttribute("Degrees")]
        public double? nPeriapsis { get; private set; }                     // direct, degrees
        [PropertyNameAttribute("Kepler Periapsis Degrees")]
        public double? nPeriapsisKepler { get { if (nPeriapsis.HasValue) return (360.0 - nPeriapsis.Value) % 360.0; else return null; } set { nPeriapsis = (360.0 - value) % 360; } }
        [PropertyNameAttribute("Seconds")]
        public double? nOrbitalPeriod { get; private set; }                 // direct, seconds
        [PropertyNameAttribute("Days")]
        public double? nOrbitalPeriodDays { get { if (nOrbitalPeriod.HasValue) return nOrbitalPeriod.Value / BodyPhysicalConstants.oneDay_s; else return null; } }
        [PropertyNameAttribute("Degrees")]
        public double? nAscendingNode { get; private set; }                  // odyssey update 7 22/9/21, degrees
        [PropertyNameAttribute("Kepler AN Degrees")]
        public double? nAscendingNodeKepler { get { if (nAscendingNode.HasValue) return (360.0 - nAscendingNode.Value) % 360.0; else return null; } set { nAscendingNode = (360.0 - value) % 360; } }
        [PropertyNameAttribute("Degrees")]
        public double? nMeanAnomaly { get; private set; }                    // odyssey update 7 22/9/21, degrees

        [PropertyNameAttribute("Tilt, radians")]
        public double? nAxialTilt { get; private set; }                     // direct, radians
        [PropertyNameAttribute("Tilt, degrees")]
        public double? nAxialTiltDeg { get { if (nAxialTilt.HasValue) return nAxialTilt.Value * 180.0 / Math.PI; else return null; } }
        [PropertyNameAttribute("Is in tidal lock")]
        public bool? nTidalLock { get; private set; }                       // direct

        ////////////////////////////////////////////////////////////////////// Planets
        ///
        [PropertyNameAttribute("Long text name from journal")]
        public string PlanetClass { get; private set; }                     // planet class, direct. If belt cluster, null. Try to avoid. Not localised. Such as "Icy Body" , "Water World" plain text 
        [PropertyNameAttribute("EDD Enum")]
        public EDPlanet PlanetTypeID { get; }                       // planet class -> ID
        [PropertyNameAttribute("Localised Name")]
        public string PlanetTypeText { get { return IsPlanet ? Planets.PlanetNameTranslated(PlanetTypeID) : ""; } }   // Use in preference to planet class for display

        [JsonIgnore]
        [PropertyNameAttribute("Is it an ammonia world")]
        public bool AmmoniaWorld { get { return Planets.AmmoniaWorld(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it an earth like world")]
        public bool Earthlike { get { return Planets.Earthlike(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it a water world")]
        public bool WaterWorld { get { return Planets.WaterWorld(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it a sudarsky gas giant world")]
        public bool SudarskyGasGiant { get { return Planets.SudarskyGasGiant(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it a gas giant world")]
        public bool GasGiant { get { return Planets.GasGiant(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it a water giant world")]
        public bool WaterGiant { get { return Planets.WaterGiant(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it an helium gas world")]
        public bool HeliumGasGiant { get { return Planets.HeliumGasGiant(PlanetTypeID); } }
        [JsonIgnore]
        [PropertyNameAttribute("Is it an gas world")]
        public bool GasWorld { get { return Planets.GasWorld(PlanetTypeID); } }          // any type of gas world

        [PropertyNameAttribute("Empty, Terraformable, Terraformed, Terraforming")]
        public string TerraformState { get; private set; }                  // direct, can be empty or a string
        [PropertyNameAttribute("Is it terraformable or been terraformed")]
        public bool Terraformable { get { return TerraformState != null && new[] { "terraformable", "terraforming", "terraformed" }.Contains(TerraformState, StringComparer.InvariantCultureIgnoreCase); } }
        [PropertyNameAttribute("Can it be teraformed")]
        public bool CanBeTerraformable { get { return TerraformState != null && new[] { "terraformable", "terraforming" }.Contains(TerraformState, StringComparer.InvariantCultureIgnoreCase); } }

        [PropertyNameAttribute("Does it have atmosphere")]
        public bool HasAtmosphere { get { return AtmosphereID > EDAtmosphereType.No; } }  
        [PropertyNameAttribute("Atmosphere string, can be none")]
        public string Atmosphere { get; private set; }                      // EDD then processed, No atmosphere is "none" else its the atmosphere from the journal, which may or may not include the word atmosphere
        [PropertyNameAttribute("EDD ID")]
        public EDAtmosphereType AtmosphereID { get; }                       // Atmosphere -> ID (Ammonia, Carbon etc)
        [PropertyNameAttribute("EDD atmospheric property")]
        public EDAtmosphereProperty AtmosphereProperty { get; private set; }  // Atomsphere -> Property (None, Rich, Thick , Thin, Hot)
        [PropertyNameAttribute("Translated name of Atmosphere")]
        public string AtmosphereTranslated
        {
            get
            {
                {
                    string mainpart = AtmosphereID.ToString().Replace("_", " ") + ((AtmosphereProperty & EDAtmosphereProperty.Rich) != 0 ? " Rich" : "") + " Atmosphere";
                    EDAtmosphereProperty apnorich = AtmosphereProperty & ~(EDAtmosphereProperty.Rich);
                    string final = apnorich != EDAtmosphereProperty.None ? apnorich.ToString().Replace(",", "") + " " + mainpart : mainpart;
                    return final.Tx();
                }
            }
        }

        [PropertyNameAttribute("Dictionary of atmosphere composition, in %. Use an Iter variable to search it")]
        public Dictionary<string, double> AtmosphereComposition { get; private set; }       // from journal. value is in % (0-100)
        [PropertyNameAttribute("Atmospheric composition list, comma separated")]
        public string AtmosphericCompositionList { get { return AtmosphereComposition != null ? string.Join(", ", AtmosphereComposition.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key} {kvp.Value:N2}%")) : ""; } }
        [PropertyNameAttribute("Does body have atmosphere composition list")]
        public bool HasAtmosphericComposition { get { return AtmosphereComposition != null && AtmosphereComposition.Any(); } }
        [PropertyNameAttribute("Dictionary of planet composition, in %")]
        public Dictionary<string, double> PlanetComposition { get; private set; }
        [PropertyNameAttribute("Does it have planetary composition stats")]
        public bool HasPlanetaryComposition { get { return PlanetComposition != null && PlanetComposition.Any(); } }
        [PropertyNameAttribute("Journal volcanism string")]
        public string Volcanism { get; private set; }                       // direct from journal - will be a blank string for no volcanism
        [PropertyNameAttribute("EDD Volcanism ID")]
        public EDVolcanism VolcanismID { get; }                     // Volcanism -> ID (Water_Magma, Nitrogen_Magma etc)
        [PropertyNameAttribute("Has volcanism, excluding unknowns")]
        public bool HasMeaningfulVolcanism { get { return VolcanismID > EDVolcanism.No; } }
        [PropertyNameAttribute("EDD Volcanism type")]
        public EDVolcanismProperty VolcanismProperty { get; private set; }               // Volcanism -> Property (None, Major, Minor)

        [PropertyNameAttribute("Translated name of Volcanism")]
        public string VolcanismTranslated
        {
            get
            {
                string mainpart = VolcanismID.ToString().Replace("_", " ") + " Volcanism";
                string final = VolcanismProperty != EDVolcanismProperty.None ? VolcanismProperty.ToString() + " " + mainpart : mainpart;
                return final.Tx();
            }
        }

        [PropertyNameAttribute("m/s")]
        public double? nSurfaceGravity { get; private set; }                // direct
        [PropertyNameAttribute("Fractions of earth gravity")]
        public double? nSurfaceGravityG { get { if (nSurfaceGravity.HasValue) return nSurfaceGravity.Value / BodyPhysicalConstants.oneGee_m_s2; else return null; } }
        [PropertyNameAttribute("Pascals")]
        public double? nSurfacePressure { get; private set; }               // direct
        [PropertyNameAttribute("Fractions of earth atmosphere")]
        public double? nSurfacePressureEarth { get { if (nSurfacePressure.HasValue) return nSurfacePressure.Value / BodyPhysicalConstants.oneAtmosphere_Pa; else return null; } }
        [PropertyNameAttribute("Is it landable (may be null if not valid for body)")]
        public bool? nLandable { get; private set; }                        // direct
        [PropertyNameAttribute("Is it def landable")]
        public bool IsLandable { get { return nLandable.HasValue && nLandable.Value && (!HasAtmosphericComposition || (HasAtmosphericComposition && IsOdyssey)); } }
        public bool IsLandableOdyssey { get { return nLandable.HasValue && nLandable.Value && HasAtmosphericComposition; } }
        [PropertyNameAttribute("Mass in Earths")]
        public double? nMassEM { get; private set; }                        // direct, not in description of event, mass in EMs
        [PropertyNameAttribute("Mass in Moons")]
        public double? nMassMM { get { if (nMassEM.HasValue) return nMassEM * BodyPhysicalConstants.oneEarthMoonMassRatio; else return null; } }
        [PropertyNameAttribute(null)]       // this excludes it from any queries etc, as its not really useful. Mass in moons or earths
        public string MassEMMM { get { if (nMassEM.HasValue) { if (nMassEM.Value < 0.01) return nMassMM.Value.ToString("N4") + " MM"; else return nMassEM.Value.ToString("N2") + " EM"; } else return null; } }
        [PropertyNameAttribute("Does it have materials list")]
        public bool HasMaterials { get { return Materials != null && Materials.Any(); } }
        [PropertyNameAttribute("Materials dictionary, in %")]
        public Dictionary<string, double> Materials { get; private set; }       // fdname and name is the same for materials on planets.  name is lower case
        public bool HasMaterial(string name) { return Materials != null && Materials.ContainsKey(name.ToLowerInvariant()); }
        [PropertyNameAttribute("List of materials, comma separated")]
        public string MaterialList { get { if (Materials != null) { var na = (from x in Materials select x.Key).ToArray(); return String.Join(",", na); } else return null; } }

        [PropertyNameAttribute("What is the reserve level of the ring")]
        public EDReserve ReserveLevel { get; private set; }

        ///////////////////////////////////////////////////////////////////////// EDD additions
        ///////////////////////////////////////////////////////////////////////// EDD additions
        ///////////////////////////////////////////////////////////////////////// EDD additions

        [PropertyNameAttribute("Body data source")]
        public SystemSource DataSource { get; private set; } = SystemSource.FromJournal;        // FromJournal, FromEDSM, FromSpansh
        [PropertyNameAttribute("Web data source name (Empty if not)")]
        public string DataSourceName { get { return DataSource != SystemSource.FromJournal ? DataSource.ToString().Replace("From", "") : ""; } }
        [PropertyNameAttribute("Is scan web sourced?")]
        public bool IsWebSourced { get { return DataSource != SystemSource.FromJournal; } }


        [PropertyNameAttribute("Signal information")]
        public List<JournalSAASignalsFound.SAASignal> Signals { get; set; }             // NOTE Augmented by StarScan - may be null
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain geo signals")]
        public bool ContainsGeoSignals { get { return Signals?.Count(x => x.IsGeo) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain bio signals")]
        public bool ContainsBioSignals { get { return Signals?.Count(x => x.IsBio) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain thargoid signals")]
        public bool ContainsThargoidSignals { get { return Signals?.Count(x => x.IsThargoid) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain guardian signals")]
        public bool ContainsGuardianSignals { get { return Signals?.Count(x => x.IsGuardian) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain human signals")]
        public bool ContainsHumanSignals { get { return Signals?.Count(x => x.IsHuman) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain other signals")]
        public bool ContainsOtherSignals { get { return Signals?.Count(x => x.IsOther) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it contain uncategorised signals")]
        public bool ContainsUncategorisedSignals { get { return Signals?.Count(x => x.IsUncategorised) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of geo signals")]
        public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of bio signals")]
        public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of thargoid signals")]
        public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of guardian signals")]
        public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of human signals")]
        public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of uncategorised signals")]
        public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }


        [PropertyNameAttribute("Genuses information")]
        public List<JournalSAASignalsFound.SAAGenus> Genuses { get; set; }          // NOTE Augmented by StarScan - may be null
        [PropertyNameAttribute("Any Genuses")]
        public bool ContainsGenusus { get { return (Genuses?.Count ?? 0) > 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of all genuses")]
        public int CountGenusus { get { return Genuses?.Count ?? 0; } }


        [PropertyNameAttribute("Organics information")]
        public List<JournalScanOrganic> Organics { get; set; }  // NOTE Augmented by StarScan - may be null
        [JsonIgnore]
        [PropertyNameAttribute("Any organic scans")]
        public bool ContainsOrganicsScans { get { return (Organics?.Count ?? 0) > 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of all organic scans")]
        public int CountOrganicsScans { get { return Organics?.Count ?? 0; } }
        [PropertyNameAttribute("Count of fully analysed scans")]
        public int CountOrganicsScansAnalysed { get { return Organics?.Where(x => x.ScanType == JournalScanOrganic.ScanTypeEnum.Analyse).Count() ?? 0; } }
        [PropertyNameAttribute("Are all organics on this body analysed")]
        public bool OrganicsFullyAnalysed { get { return (CountOrganicsScansAnalysed == CountBioSignals && CountBioSignals > 0); } }
        [PropertyNameAttribute("Are there organics that haven't been analysed")]
        public bool UnanalysedBiosPresent { get { return CountOrganicsScansAnalysed != CountBioSignals; } }
        [PropertyNameAttribute("Surface features information")]


        public List<IBodyFeature> SurfaceFeatures { get; set; } // NOTE Augmented by StarScan - may be null
        [PropertyNameAttribute("Count of surface featurss")]
        public int CountSurfaceFeatures { get { return SurfaceFeatures?.Count ?? 0; } }

        [PropertyNameAttribute("Codex information")]
        public List<JournalCodexEntry> CodexEntries {get;set; }          // NOTE Augmented by StarScan - may be null
        [PropertyNameAttribute("Are there codex entries against this body")]
        public bool HasCodexEntries => CodexEntries?.Count > 0;   

        public void SetMapped(bool m, bool e)                           // NOTE Augmented by StarScan - on SAA SCAN Complete, data stored in body node, copied to this class
        {
            Mapped = m; EfficientMapped = e;
        }

        [PropertyNameAttribute("Have we mapped it")]
        public bool Mapped { get; private set; }                        // WE Mapped it - affects prices
        [PropertyNameAttribute("Have we efficiently mapped it")]
        public bool EfficientMapped { get; private set; }               // WE efficiently mapped it - affects prices

        [PropertyNameAttribute("cr. Estimated value now")]
        public int EstimatedValue { get { return GetEstimatedValues().EstimatedValue(WasDiscovered, WasMapped, Mapped, EfficientMapped, IsWebSourced); } }     // Direct access to its current EstimatedValue, provides backwards compatibility for code and action packs.
        [PropertyNameAttribute("cr. Best estimated value possible")]
        public int MaximumEstimatedValue { get { return GetEstimatedValues().EstimatedValue(WasDiscovered, WasMapped, true, true,false); } }     // Direct access to its current EstimatedValue, provides backwards compatibility for code and action packs.

        public void ResetParents(List<BodyParent> other)        // StarScan uses this if another scan comes in, to make sure we don't lose barycentre info
        {
            Parents = other;
        }

        
        [PropertyNameAttribute("N/A")]
        public string ShipIDForStatsOnly { get; set; }         // used in stats computation only.  Not in main code.

        
        // IBodyNameAndID additional interfaces.
        int? IBodyFeature.BodyID => BodyID;
        public double? Latitude { get => null; set { } }
        public double? Longitude { get => null; set { } }
        public bool HasLatLong => false;
        public string Name => null;
        public string Name_Localised => null;
        public long? MarketID => null;
        public StationDefinitions.StarportTypes FDStationType => StationDefinitions.StarportTypes.Unknown;
        public string StationFaction => null;

        // Create the entry from JSON

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
            WasFootfalled = evt["WasFootfalled"].BoolNull();                // ALL new 4.2.1

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
                            if (Enum.TryParse<BodyParent.ParentBodyType>(kvp.Key, true, out BodyParent.ParentBodyType res))
                            {
                                Parents.Add(new BodyParent { Type = res, BodyID = kvp.Value.Int() });
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false, "Bad parent body type");
                            }
                        }
                    }
                }
            }

            nRotationPeriod = evt["RotationPeriod"].DoubleNull();           // Stars/Planets, not belt clusters
            nSurfaceTemperature = evt["SurfaceTemperature"].DoubleNull();   // Stars/Planets, not belt clusters
            nRadius = evt["Radius"].DoubleNull();                           // Stars/Planets, not belt clusters    

            Rings = evt["Rings"]?.ToObjectQ<StarPlanetRing[]>();            // Stars/Planets, may be Null, not belt clusters
            foreach (var r in Rings.EmptyIfNull())
                r.Normalise();

            StarType = evt["StarType"].StrNull();                           // stars have this field

            if (StarType != null)     // based on StarType
            {
                BodyType = BodyDefinitions.BodyType.Star;
                StarTypeID = Stars.ToEnum(StarType);

                nStellarMass = evt["StellarMass"].DoubleNull();
                nAbsoluteMagnitude = evt["AbsoluteMagnitude"].DoubleNull();
                Luminosity = evt["Luminosity"].StrNull();
                StarSubclass = evt["Subclass"].IntNull();
                nAge = evt["Age_MY"].DoubleNull();
            }
            else
            {
                PlanetClass = evt["PlanetClass"].StrNull();                 // try and read planet class, this might be null as well, in which case its a belt cluster
                
                if (PlanetClass != null)                                    // now we can set BodyType correctly
                    BodyType = BodyDefinitions.BodyType.Planet;
                else if ((Parents?.Count >= 1 && Parents[0].IsStellarRing) || BodyDefinitions.IsBodyNameABeltCluster(BodyName))       // parent is a stellar ring, its a asteroid cluster
                    BodyType = BodyDefinitions.BodyType.AsteroidCluster;
                else if (Parents?.Count < 1 || Parents[0].IsPlanet)
                    BodyType = BodyDefinitions.BodyType.PlanetaryRing;
                else
                    BodyType = BodyDefinitions.BodyType.StellarRing;
            }

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
                PlanetTypeID = Planets.ToEnum(PlanetClass);
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

                if (Atmosphere.IsEmpty())                               // try type.
                {
                    Atmosphere = evt["AtmosphereType"].StrNull();       // it may still be null here or empty string
                }
                
                if ( Atmosphere.EqualsIIC("thick  atmosphere") )            // obv a frontier bug, atmosphere type has the missing text
                {
                    Atmosphere = "thick " + evt["AtmosphereType"].Str().SplitCapsWord() + " atmosphere";
                }
                else if (Atmosphere.EqualsIIC("thin  atmosphere"))
                {
                    Atmosphere = "thin " + evt["AtmosphereType"].Str().SplitCapsWord() + " atmosphere";
                }
                else if (Atmosphere.EqualsIIC("No Atmosphere"))
                {
                    Atmosphere = "none";
                }
                else if (Atmosphere.HasChars() && !Atmosphere.EqualsIIC("none") && !Atmosphere.ContainsIIC("atmosphere") )
                {
                    Atmosphere += " atmosphere";
                }

                if (Atmosphere.IsEmpty())       // null or empty - nothing in either, see if there is composition
                {
                    if ((AtmosphereComposition?.Count ?? 0) > 0)    // if we have some composition, synthesise name
                    {
                        foreach( var e in Enum.GetNames(typeof(EDAtmosphereType)))
                        {
                            if ( AtmosphereComposition.ContainsKey(e.ToString()))       // pick first match in ID
                            {
                                Atmosphere = e.ToString().SplitCapsWord().ToLowerInvariant();
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
                    Atmosphere = Atmosphere.Replace("sulfur", "sulphur").SplitCapsWord().ToLowerInvariant();      // fix frontier spelling mistakes
                 //   System.Diagnostics.Debug.WriteLine("Atmosphere '" + Atmosphere + "'");
                }

                //System.IO.File.AppendAllText(@"c:\code\atmos.txt", $"Atmosphere {evt["Atmosphere"]} type {evt["AtmosphereType"]} => {Atmosphere}\r\n");

                System.Diagnostics.Debug.Assert(Atmosphere.HasChars());

                AtmosphereID = Planets.ToEnum(Atmosphere.ToLowerInvariant(), out EDAtmosphereProperty ap);  // convert to internal ID
                AtmosphereProperty = ap;

                if (AtmosphereID == EDAtmosphereType.Unknown)
                {
                    System.Diagnostics.Trace.WriteLine($"** Atmos not recognised {Atmosphere} '{evt["Atmosphere"].Str()}' '{evt["AtmosphereType"].Str()}'");
                }

                JObject composition = evt["Composition"].Object();
                if (!composition.IsNull() && composition.IsObject)
                {
                    PlanetComposition = new Dictionary<string, double>();
                    foreach (var kvp in composition)
                    {
                        PlanetComposition[kvp.Key] = kvp.Value.Double() * 100.0;        // convert to %
                    }
                }

                Volcanism = evt["Volcanism"].Str();     // blank string empty
                VolcanismID = Planets.ToEnum(Volcanism, out EDVolcanismProperty vp);
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

                ReserveLevel = Planets.ReserveToEnum(evt["ReserveLevel"].Str().Replace("Resources", ""));
            }
            else
                PlanetTypeID = EDPlanet.Unknown_Body_Type;

            // scans are presumed FromJournal. These markers show that either EDSMClass made it for SpanshClass
            if (evt["EDDFromEDSMBodie"].Bool(false))                    // Note the Finwenism!
                DataSource = SystemSource.FromEDSM;
            else if (evt["EDDFromSpanshBody"].Bool(false))
                DataSource = SystemSource.FromSpansh;
        }

        // special, for star scan node tree only, create a scan record with the contents of the journal scan bary centre info
        public JournalScan(JournalScanBaryCentre js) : base(DateTime.Now, JournalTypeEnum.ScanBaryCentre)
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

        #region Overrides, interfaces, etc

        public override string SummaryName(ISystem sys)
        {
            string text = "Scan of {0}".Tx();
            if (ScanType == "AutoScan")
                text = "Autoscan of {0}".Tx();
            else if (ScanType == "Detailed")
                text = "Detailed scan of {0}".Tx();
            else if (ScanType == "Basic")
                text = "Basic scan of {0}".Tx();
            else if (ScanType.Contains("Nav"))
                text = "Nav scan of {0}".Tx();

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

        public override string GetInfo(FillInformationData fid)
        {
            if (IsStar)
            {
                return BaseUtils.FieldBuilder.Build("", StarTypeText, "Mass: ;SM;0.00".Tx(), nStellarMass,
                                                "Age: ;my;0.0".Tx(), nAge,
                                                "Radius".Tx()+": ", RadiusText,
                                                "Dist: ;ls;0.0".Tx(), DistanceFromArrivalLS,
                                                "Name".Tx()+": ", BodyName.ReplaceIfStartsWith(fid.System.Name));
            }
            else if (IsPlanet)
            {
                return BaseUtils.FieldBuilder.Build("", PlanetTypeText, "Mass".Tx()+": ", MassEMMM,
                                                "<;, Landable".Tx(), IsLandable,
                                                "<;, Terraformable".Tx(), TerraformState == "Terraformable", "", HasAtmosphere ? AtmosphereTranslated : null,
                                                 "Gravity: ;G;0.00".Tx(), nSurfaceGravityG,
                                                 "Radius".Tx()+": ", RadiusText,
                                                 "Dist: ;ls;0.0".Tx(), DistanceFromArrivalLS,
                                                 "Name".Tx()+": ", BodyName.ReplaceIfStartsWith(fid.System.Name));
            }
            else 
            {
                return BaseUtils.FieldBuilder.Build("Mass".Tx()+": ", MassEMMM,
                                                 "Dist: ;ls;0.0".Tx(), DistanceFromArrivalLS,
                                                 "Name".Tx()+": ", BodyName.ReplaceIfStartsWith(fid.System.Name));
            }
        }

        public override string GetDetailed()
        {
            return DisplayText(includefront: true);
        }

        // this structure is reflected by JournalFilterSelector to allow a journal class to add extra filter items to the journal filter lists. Its in use!
        static public List<Tuple<string, string, Image>> FilterItems()
        {
            return new List<Tuple<string, string, Image>>()
            {
                new Tuple<string, string,Image>( "Scan Auto", "Scan Auto".Tx(), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
                new Tuple<string,string,Image>( "Scan Basic", "Scan Basic".Tx(), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
                new Tuple<string,string,Image>( "Scan Nav", "Scan Nav".Tx(), JournalEntry.JournalTypeIcons[JournalTypeEnum.Scan] ),
            };
        }

        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            s.AddJournalScan(this, system); 
        }

        #endregion

    }

}


