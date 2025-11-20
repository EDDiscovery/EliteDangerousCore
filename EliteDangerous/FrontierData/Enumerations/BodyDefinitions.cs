/*
 * Copyright 2023-2025 EDDiscovery development team
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

using BaseUtils;
using BaseUtils.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore
{
    public class BodyDefinitions
    {
        public enum BodyType
        {
            // shared between StarScan and Frontier BodyType fields
            Planet,             // a Planet or moon
            Star,               // a top level star, or a substar
            Barycentre,         // a barycentre ('Null' type in parents array)
            StellarRing,        // a belt cluster, 'A Belt Cluster' name (`Ring` type in the parents array) - has AsteroidCluster in StarScan underneath it
            AsteroidCluster,    // a body under the belt cluster
            PlanetaryRing,      // planet ring.  Called "A Ring" or "B Ring", from a journalscan of the ring (Scan will be set), or from the journalscan of a planet with its ring structure broken into children, BeltData is set.

            // Frontier Bodytype only
            Station,    // at a station
            SmallBody,  // saw for comets, very few

            // EDD Only
            Unknown,
            System,          // top level SystemBodies object in SystemNode
        };

        static public BodyType GetBodyType(string bt)
        {
            if (bt == null || bt.Length == 0)           // not present
                return BodyType.Unknown;

            Enum.TryParse<BodyType>(bt, true, out BodyType btn);
            
            if ( btn == BodyType.Unknown && bt.Equals("Null", StringComparison.InvariantCultureIgnoreCase))
                btn = BodyType.Barycentre;

            System.Diagnostics.Debug.Assert(btn != BodyType.Unknown);
            return btn;
        }

        static public bool IsBodyNameRing(string bodyname)
        {
            if (bodyname.HasChars())
            {
                var elements = bodyname.ToLowerInvariant().Split(' ').ToList();        // split into spaced parts
                // ends in ring and previous is a single character alpha letter (see starscanjournalscans.cs line 213 ish)
                if (elements.Count > 0 && elements[elements.Count - 1].Equals("ring") && elements[elements.Count - 2].Length == 1 && char.IsLetter(elements[elements.Count - 2][0]))
                    return true;
            }

            return false;
        }

        static public BodyType BodyTypeFromBodyNameRingOrPlanet(string bodyname)
        {
            return IsBodyNameRing(bodyname) ? BodyType.PlanetaryRing : BodyType.Planet;
        }

        static public string StarTypeImageName(EDStar StarTypeID, double? nStellarMass, double? nSurfaceTemperature)
        {
            string iconName = StarTypeID.ToString(); // fallback

            // black holes variations. According to theorethical papers, we should find four classes of black holes:
            // supermassive, intermediate, stellar and micro (https://en.wikipedia.org/wiki/Black_hole) gravitional collapses.
            // The in-game black hole population do not really fit quite well that nomenclature, they are somewhat capped, so we have to quantize the masses ranges to more reasonable limits.
            // For example, SagA* is known to have at least 4 millions solar masses, in real life: the in-game stats shows only 516.608, way smaller! The same for Great Annihilator: the biggest
            // of the couple should be more than some hundrends of thousands of solar masses, which is the expected mass range of an intermediate black holes...
            if (StarTypeID == EDStar.H)
            {
                if (nStellarMass <= 7.0)
                    iconName = "H_stellar";
                else if (nStellarMass < 70.0)
                    iconName = "H";
                else
                    iconName = "H_intermediate";
            }

            // Sagittarius A* receive it's own icon, by David Braben himself.
            if (StarTypeID == EDStar.SuperMassiveBlackHole)
                iconName = "SuperMassiveBlackHole";

            // neutron stars variations.
            // Uses theorethical masses: https://en.wikipedia.org/wiki/Neutron_star
            if (StarTypeID == EDStar.N)
            {
                if (nStellarMass < 1.1)
                    iconName = "N";
                else if (nStellarMass < 1.9)
                    iconName = "N_massive";
                else
                    iconName = "N_veryMassive";
            }

            // white dwarfs variations
            if (StarTypeID == EDStar.D || StarTypeID == EDStar.DA || StarTypeID == EDStar.DAB || StarTypeID == EDStar.DAO || StarTypeID == EDStar.DAV || StarTypeID == EDStar.DAZ || StarTypeID == EDStar.DB ||
                StarTypeID == EDStar.DBV || StarTypeID == EDStar.DBZ || StarTypeID == EDStar.DC || StarTypeID == EDStar.DCV || StarTypeID == EDStar.DO || StarTypeID == EDStar.DOV || StarTypeID == EDStar.DQ ||
                StarTypeID == EDStar.DX)
            {
                if (nSurfaceTemperature <= 5500)
                    iconName = "D";
                else if (nSurfaceTemperature < 8000)
                    iconName = "D_hot";
                else if (nSurfaceTemperature < 14000)
                    iconName = "D_veryHot";
                else if (nSurfaceTemperature >= 14000)
                    iconName = "D_extremelyHot";
            }

            // carbon stars
            if (StarTypeID == EDStar.C || StarTypeID == EDStar.CHd || StarTypeID == EDStar.CJ || StarTypeID == EDStar.CN || StarTypeID == EDStar.CS)
                iconName = "C";

            // Herbig AeBe
            // https://en.wikipedia.org/wiki/Herbig_Ae/Be_star
            // This kind of star classes show a spectrum of an A or B star class. It all depend on their surface temperature
            if (StarTypeID == EDStar.AeBe)
            {
                if (nSurfaceTemperature < 5000)
                    iconName = "A";
                else
                    iconName = "B";
            }

            // giants and supergiants can use the same icons of their classes, so we'll use them, to avoid duplication. In case we really want, we can force a bigger size in scan panel...
            // better: huge corona overlay? ;)
            if (StarTypeID == EDStar.A_BlueWhiteSuperGiant)
            {
                iconName = "A";
            }
            else if (StarTypeID == EDStar.B_BlueWhiteSuperGiant)
            {
                iconName = "B";
            }
            else if (StarTypeID == EDStar.F_WhiteSuperGiant)
            {
                iconName = "F";
            }
            else if (StarTypeID == EDStar.G_WhiteSuperGiant)
            {
                iconName = "G";
            }
            else if (StarTypeID == EDStar.K_OrangeGiant)
            {
                iconName = "K";
            }
            else if (StarTypeID == EDStar.M_RedGiant)
            {
                iconName = "M";
            }
            else if (StarTypeID == EDStar.M_RedSuperGiant)
            {
                iconName = "M";
            }

            // t-tauri shows spectral colours related to their surface temperature...
            // They are pre-main sequence stars, so their spectrum shows similarities to main-sequence stars with the same surface temperature; are usually brighter, however, because of the contraction process.
            // https://en.wikipedia.org/wiki/Stellar_classification
            // https://en.wikipedia.org/wiki/T_Tauri_star
            if (StarTypeID == EDStar.TTS)
            {
                if (nSurfaceTemperature < 3700)
                    iconName = "M";
                else if (nSurfaceTemperature < 5200)
                    iconName = "K";
                else if (nSurfaceTemperature < 6000)
                    iconName = "G";
                else if (nSurfaceTemperature < 7500)
                    iconName = "F";
                else if (nSurfaceTemperature < 10000)
                    iconName = "A";
                else if (nSurfaceTemperature < 30000)
                    iconName = "B";
                else
                    iconName = "O";
            }

            // wolf-rayets stars
            // https://en.wikipedia.org/wiki/Wolf%E2%80%93Rayet_star
            if (StarTypeID == EDStar.W || StarTypeID == EDStar.WC || StarTypeID == EDStar.WN || StarTypeID == EDStar.WNC || StarTypeID == EDStar.WO)
            {
                if (nSurfaceTemperature < 50000)
                    iconName = "F";
                if (nSurfaceTemperature < 90000)
                    iconName = "A";
                if (nSurfaceTemperature < 140000)
                    iconName = "B";
                if (nSurfaceTemperature > 140000)
                    iconName = "O";
            }

            if (StarTypeID == EDStar.MS || StarTypeID == EDStar.S)
                iconName = "M";

            if (StarTypeID == EDStar.Nebula)
                return "Bodies.Stars.Nebula";

            if (StarTypeID == EDStar.StellarRemnantNebula)
                return $"Bodies.Stars.StellarRemnantNebula";

            if (StarTypeID == EDStar.Unknown)
                return $"Bodies.Stars.Unknown";
            
            if (StarTypeID == EDStar.RoguePlanet)
                return $"Bodies.Stars.RoguePlanet";

            //   System.Diagnostics.Debug.WriteLine(StarTypeID + ": " + iconName);
            return $"Bodies.Stars.{iconName}";
        }

        static public string PlanetClassImageName(EDPlanet PlanetTypeID, double? nSurfaceTemperature,
                                            Dictionary<string, double> AtmosphereComposition, EDAtmosphereProperty AtmosphereProperty, EDAtmosphereType AtmosphereID,
                                            bool Terraformable, bool? nLandable, double? nMassEM, bool? nTidalLock)
        {
            string iconName = PlanetTypeID.ToString();

            // Gas Giants variants
            if (PlanetTypeID.ToNullSafeString().ToLowerInvariant().Contains("giant"))
            {
                iconName = "GG1v1"; // fallback

                if (PlanetTypeID == EDPlanet.Gas_giant_with_ammonia_based_life)
                {
                    if (nSurfaceTemperature < 105)
                        iconName = "GGAv8";
                    else if (nSurfaceTemperature < 110)
                        iconName = "GGAv11";
                    else if (nSurfaceTemperature < 115)
                        iconName = "GGAv9";
                    else if (nSurfaceTemperature < 120)
                        iconName = "GGAv2";
                    else if (nSurfaceTemperature < 124)
                        iconName = "GGAv12";
                    else if (nSurfaceTemperature < 128)
                        iconName = "GGAv14";
                    else if (nSurfaceTemperature < 130)
                        iconName = "GGAv7";
                    else if (nSurfaceTemperature < 134)
                        iconName = "GGAv13";
                    else if (nSurfaceTemperature < 138)
                        iconName = "GGAv6";
                    else if (nSurfaceTemperature < 142)
                        iconName = "GGAv1";
                    else if (nSurfaceTemperature < 148)
                        iconName = "GGAv3";
                    else if (nSurfaceTemperature < 152)
                        iconName = "GGAv5";
                    else
                        iconName = "GGAv4";
                }

                if (PlanetTypeID == (EDPlanet.Water_giant_with_life | EDPlanet.Gas_giant_with_water_based_life))
                {
                    if (nSurfaceTemperature < 152)
                        iconName = "GGWv24";
                    else if (nSurfaceTemperature < 155)
                    {
                        if (AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("oxygen"))
                            iconName = "GGWv1";
                        else
                            iconName = "GGWv16";
                    }
                    else if (nSurfaceTemperature < 158)
                        iconName = "GGWv3";
                    else if (nSurfaceTemperature < 160)
                        iconName = "GGWv14";
                    else if (nSurfaceTemperature < 162)
                        iconName = "GGWv22";
                    else if (nSurfaceTemperature < 165)
                        iconName = "GGWv20";
                    else if (nSurfaceTemperature < 172)
                        iconName = "GGWv25";
                    else if (nSurfaceTemperature < 175)
                        iconName = "GGWv2";
                    else if (nSurfaceTemperature < 180)
                        iconName = "GGWv13";
                    else if (nSurfaceTemperature < 185)
                        iconName = "GGWv9";
                    else if (nSurfaceTemperature < 190)
                        iconName = "GGWv21";
                    else if (nSurfaceTemperature < 200)
                        iconName = "GGWv7";
                    else if (nSurfaceTemperature < 205)
                        iconName = "GGWv8";
                    else if (nSurfaceTemperature < 210)
                        iconName = "GGWv15";
                    else if (nSurfaceTemperature < 213)
                        iconName = "GGWv17";
                    else if (nSurfaceTemperature < 216)
                        iconName = "GGWv6";
                    else if (nSurfaceTemperature < 219)
                        iconName = "GGWv18";
                    else if (nSurfaceTemperature < 222)
                        iconName = "GGWv10";
                    else if (nSurfaceTemperature < 225)
                        iconName = "GGWv11";
                    else if (nSurfaceTemperature < 228)
                        iconName = "GGWv23";
                    else if (nSurfaceTemperature < 232)
                        iconName = "GGWv5";
                    else if (nSurfaceTemperature < 236)
                        iconName = "GGWv12";
                    else
                    {
                        if (AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("oxygen"))
                            iconName = "GGWv19";
                        else
                            iconName = "GGWv4";
                    }
                }

                if (PlanetTypeID == (EDPlanet.Helium_gas_giant | EDPlanet.Helium_rich_gas_giant))
                {
                    if (nSurfaceTemperature < 110)
                    {
                        if (AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("antimony") ||
                            AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("cadmium") ||
                            AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("niobium"))
                            iconName = "GGHv7";
                        else
                            iconName = "GGHv3";
                    }
                    else if (nSurfaceTemperature < 125)
                        iconName = "GGHv6";
                    else if (nSurfaceTemperature < 140)
                        iconName = "GGHv2";
                    else if (nSurfaceTemperature < 180)
                        iconName = "GGHv5";
                    else if (nSurfaceTemperature < 270)
                        iconName = "GGHv4";
                    else if (nSurfaceTemperature < 600)
                        iconName = "GGHv1";
                    else if (nSurfaceTemperature < 700)
                        iconName = "GGHv9";
                    else
                        iconName = "GGHv8";
                }

                if (PlanetTypeID == EDPlanet.Sudarsky_class_I_gas_giant)
                {

                    if (nSurfaceTemperature <= 30)
                        iconName = "GG1v12";
                    else if (nSurfaceTemperature < 35)
                        iconName = "GG1v15";
                    else if (nSurfaceTemperature < 40)
                        iconName = "GG1v13";
                    else if (nSurfaceTemperature < 45)
                        iconName = "GG1v4"; // neptune
                    else if (nSurfaceTemperature < 50)
                        iconName = "GG1v9";
                    else if (nSurfaceTemperature < 55)
                        iconName = "GG1v2";
                    else if (nSurfaceTemperature < 60)
                        iconName = "GG1v16"; // uranus
                    else if (nSurfaceTemperature < 65)
                        iconName = "GG1v19";
                    else if (nSurfaceTemperature < 70)
                        iconName = "GG1v18";
                    else if (nSurfaceTemperature < 78)
                        iconName = "GG1v11";
                    else if (nSurfaceTemperature < 85)
                        iconName = "GG1v3";
                    else if (nSurfaceTemperature < 90)
                        iconName = "GG1v6";
                    else if (nSurfaceTemperature < 100)
                        iconName = "GG1v8";
                    else if (nSurfaceTemperature < 110)
                        iconName = "GG1v1";
                    else if (nSurfaceTemperature < 130)
                        iconName = "GG1v5";
                    else if (nSurfaceTemperature < 135)
                        iconName = "GG1v17";
                    else if (nSurfaceTemperature < 140)
                        iconName = "GG1v20";
                    else if (nSurfaceTemperature < 150)
                        iconName = "GG1v14"; // jupiter
                    else if (nSurfaceTemperature < 170)
                        iconName = "GG1v7";
                    else
                        iconName = "GG1v10";
                }

                if (PlanetTypeID == EDPlanet.Sudarsky_class_II_gas_giant)
                {
                    if (nSurfaceTemperature < 160)
                        iconName = "GG2v4";
                    else if (nSurfaceTemperature < 175)
                        iconName = "GG2v7";
                    else if (nSurfaceTemperature < 200)
                        iconName = "GG2v5";
                    else if (nSurfaceTemperature < 2450)
                        iconName = "GG2v8";
                    else if (nSurfaceTemperature < 260)
                        iconName = "GG2v6";
                    else if (nSurfaceTemperature < 275)
                        iconName = "GG2v1";
                    else if (nSurfaceTemperature < 300)
                        iconName = "GG2v2";
                    else
                        iconName = "GG2v3";
                }

                if (PlanetTypeID == EDPlanet.Sudarsky_class_III_gas_giant)
                {
                    if (nSurfaceTemperature < 300)
                        iconName = "GG3v2";
                    else if (nSurfaceTemperature < 340)
                        iconName = "GG3v3";
                    else if (nSurfaceTemperature < 370)
                        iconName = "GG3v12";
                    else if (nSurfaceTemperature < 400)
                        iconName = "GG3v1";
                    else if (nSurfaceTemperature < 500)
                        iconName = "GG3v5";
                    else if (nSurfaceTemperature < 570)
                        iconName = "GG3v4";
                    else if (nSurfaceTemperature < 600)
                        iconName = "GG3v8";
                    else if (nSurfaceTemperature < 620)
                        iconName = "GG3v10";
                    else if (nSurfaceTemperature < 660)
                        iconName = "GG3v7";
                    else if (nSurfaceTemperature < 700)
                        iconName = "GG3v9";
                    else if (nSurfaceTemperature < 742)
                        iconName = "GG3v11";
                    else if (nSurfaceTemperature < 760)
                        iconName = "GG3v13";
                    else
                        iconName = "GG3v6";
                }

                if (PlanetTypeID == EDPlanet.Sudarsky_class_IV_gas_giant)
                {
                    if (nSurfaceTemperature < 810)
                        iconName = "GG4v9";
                    else if (nSurfaceTemperature < 830)
                        iconName = "GG4v6";
                    else if (nSurfaceTemperature < 880)
                        iconName = "GG4v4";
                    else if (nSurfaceTemperature < 950)
                        iconName = "GG4v10";
                    else if (nSurfaceTemperature < 1010)
                        iconName = "GG4v3";
                    else if (nSurfaceTemperature < 1070)
                        iconName = "GG4v1";
                    else if (nSurfaceTemperature < 1125)
                        iconName = "GG4v7";
                    else if (nSurfaceTemperature < 1200)
                        iconName = "GG4v2";
                    else if (nSurfaceTemperature < 1220)
                        iconName = "GG4v13";
                    else if (nSurfaceTemperature < 1240)
                        iconName = "GG4v11";
                    else if (nSurfaceTemperature < 1270)
                        iconName = "GG4v8";
                    else if (nSurfaceTemperature < 1300)
                        iconName = "GG4v12";
                    else
                        iconName = "GG4v5";
                }

                if (PlanetTypeID == EDPlanet.Sudarsky_class_V_gas_giant)
                {
                    if (nSurfaceTemperature < 1600)
                        iconName = "GG5v3";
                    else if (nSurfaceTemperature < 1620)
                        iconName = "GG5v4";
                    else if (nSurfaceTemperature < 1700)
                        iconName = "GG5v1";
                    else if (nSurfaceTemperature < 1850)
                        iconName = "GG5v2";
                    else
                        iconName = "GG5v5";
                }

                if (PlanetTypeID == EDPlanet.Water_giant)
                {
                    if (nSurfaceTemperature < 155)
                        iconName = "WTGv6";
                    else if (nSurfaceTemperature < 160)
                        iconName = "WTGv2";
                    else if (nSurfaceTemperature < 165)
                        iconName = "WTGv1";
                    else if (nSurfaceTemperature < 170)
                        iconName = "WTGv3";
                    else if (nSurfaceTemperature < 180)
                        iconName = "WTGv4";
                    else if (nSurfaceTemperature < 190)
                        iconName = "WTGv5";
                    else
                        iconName = "WTGv7";
                }

                //System.Diagnostics.Debug.WriteLine(PlanetTypeID + ": " + iconName);
                return $"Bodies.Planets.Giant.{iconName}";
            }

            // Terrestrial planets variants

            // Ammonia world
            if (PlanetTypeID == EDPlanet.Ammonia_world)
            {
                iconName = "AMWv1"; // fallback

                if (Terraformable) // extremely rare, but they exists
                    iconName = "AMWv2";
                else if (AtmosphereProperty == EDAtmosphereProperty.Thick || AtmosphereProperty == EDAtmosphereProperty.Hot)
                    iconName = "AMWv3";
                else if (AtmosphereProperty == EDAtmosphereProperty.Rich)
                    iconName = "AMWv4"; // kindly provided by CMDR CompleteNOOB
                else if (nLandable == true || (AtmosphereID == EDAtmosphereType.No && nSurfaceTemperature < 140))
                    iconName = "AMWv5"; // kindly provided by CMDR CompleteNOOB
                else if (nSurfaceTemperature < 190)
                    iconName = "AMWv6";
                else if (nSurfaceTemperature < 200)
                    iconName = "AMWv3";
                else if (nSurfaceTemperature < 210)
                {
                    iconName = "AMWv1";
                }
                else
                    iconName = "AMWv4";
            }

            // Earth world
            if (PlanetTypeID == EDPlanet.Earthlike_body)
            {
                iconName = "ELWv5"; // fallback

                if ((int)nMassEM == 1 && nSurfaceTemperature == 288) // earth, or almost identical to
                {
                    iconName = "ELWv1";
                }
                else
                {
                    if (nTidalLock == true)
                        iconName = "ELWv7";
                    else
                    {
                        if (nMassEM < 0.15 && nSurfaceTemperature < 262) // mars, or extremely similar to
                            iconName = "ELWv4";
                        else if (nSurfaceTemperature < 270)
                            iconName = "ELWv8";
                        else if (nSurfaceTemperature < 285)
                            iconName = "ELWv2";
                        else if (nSurfaceTemperature < 300)
                            iconName = "ELWv3";
                        else
                            iconName = "ELWv5"; // kindly provided by CMDR CompleteNOOB
                    }
                }
            }

            if (PlanetTypeID == EDPlanet.High_metal_content_body)
            {
                iconName = "HMCv3"; // fallback

                // landable, atmosphere-less high metal content bodies
                if (nLandable == true || AtmosphereID == EDAtmosphereType.No)
                {
                    if (nSurfaceTemperature < 300)
                    {
                        if (nTidalLock == true)
                            iconName = "HMCv30";
                        else
                            iconName = "HMCv27"; // kindly provided by CMDR CompleteNOOB
                    }
                    else if (nSurfaceTemperature < 500)
                        iconName = "HMCv34";
                    else if (nSurfaceTemperature < 700)
                        iconName = "HMCv32";
                    else if (nSurfaceTemperature < 900)
                        iconName = "HMCv31";
                    else if (nSurfaceTemperature < 1000)
                    {
                        if (nTidalLock == true)
                            iconName = "HMCv33";
                        else
                            iconName = "HMCv35";
                    }
                    else if (nSurfaceTemperature >= 1000)
                        iconName = "HMCv36";
                }
                // non landable, high metal content bodies with atmosphere
                else if (nLandable == false)
                {
                    if (AtmosphereID == EDAtmosphereType.Ammonia)
                    {
                        if (nTidalLock == true)
                            iconName = "HMCv29";
                        else
                            iconName = "HMCv17";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Argon)
                        iconName = "HMCv26";
                    else if (AtmosphereID == EDAtmosphereType.Carbon_Dioxide)
                    {
                        if (nSurfaceTemperature < 220)
                            iconName = "HMCv9";
                        else if (nSurfaceTemperature < 250)
                            iconName = "HMCv12";
                        else if (nSurfaceTemperature < 285)
                            iconName = "HMCv6";
                        else if (nSurfaceTemperature < 350)
                            iconName = "HMCv28";
                        else if (nSurfaceTemperature < 400)
                        {
                            if (nTidalLock == true)
                                iconName = "HMCv7";
                            else
                                iconName = "HMCv8";
                        }
                        else if (nSurfaceTemperature < 600)
                        {
                            if (nTidalLock == true)
                                iconName = "HMCv1";
                            else
                                iconName = "HMCv24";
                        }
                        else if (nSurfaceTemperature < 700)
                            iconName = "HMCv3";
                        else if (nSurfaceTemperature < 900)
                            iconName = "HMCv25";
                        else if (nSurfaceTemperature > 1250)
                            iconName = "HMCv14";
                        else
                            iconName = "HMCv18"; // kindly provided by CMDR CompleteNOOB
                    }
                    else if (AtmosphereID == EDAtmosphereType.Methane)
                    {
                        if (nTidalLock == true)
                            iconName = "HMCv19";
                        else
                            iconName = "HMCv11";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Nitrogen)
                    {
                        if (nSurfaceTemperature < 200)
                            iconName = "HMCv2";
                        else
                            iconName = "HMCv5";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Sulphur_Dioxide)
                    {
                        if (nSurfaceTemperature < 700)
                            iconName = "HMCv23";
                        else
                            iconName = "HMCv37"; // kindly provided by CMDR CompleteNOOB
                    }
                    else if (AtmosphereID == EDAtmosphereType.Water)
                    {
                        if (nSurfaceTemperature < 400)
                            iconName = "HMCv4";
                        else if (nSurfaceTemperature < 700)
                            iconName = "HMCv13";
                        else if (nSurfaceTemperature < 1000)
                            iconName = "HMCv16";
                        else
                            iconName = "HMCv20";
                    }
                    else // for all other non yet available atmospheric types
                        iconName = "HMCv3"; // fallback
                }
                else
                    iconName = "HMCv3"; // fallback
            }

            if (PlanetTypeID == EDPlanet.Icy_body)
            {
                iconName = "ICYv4"; // fallback

                if (nLandable == true)
                {
                    iconName = "ICYv7";
                }
                else
                {
                    if (AtmosphereID == EDAtmosphereType.Helium)
                        iconName = "ICYv10";
                    else if (AtmosphereID == EDAtmosphereType.Neon || AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("neon"))
                    {
                        if (nSurfaceTemperature < 55)
                            iconName = "ICYv6";
                        else
                            iconName = "ICYv9";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Argon)
                    {
                        if (nSurfaceTemperature < 100)
                            iconName = "ICYv1";
                        else
                            iconName = "ICYv5";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Nitrogen)
                    {
                        if (nSurfaceTemperature < 105)
                            iconName = "ICYv2";
                        else if (nSurfaceTemperature < 150)
                            iconName = "ICYv3";
                        else
                            iconName = "ICYv4";
                    }
                    else if (AtmosphereID == EDAtmosphereType.Methane)
                    {
                        if (nTidalLock == true)
                            iconName = "ICYv3";
                        else
                            iconName = "ICYv8";
                    }
                    else
                        iconName = "ICYv5";
                }
            }

            if (PlanetTypeID == EDPlanet.Metal_rich_body)
            {
                iconName = "MRBv1"; // fallback

                if (nLandable == true)
                {
                    if (nSurfaceTemperature < 1000)
                        iconName = "MRBv7";
                    else if (nSurfaceTemperature < 1200)
                        iconName = "MRBv2";
                    else if (nSurfaceTemperature < 2000)
                        iconName = "MRBv12";
                    else
                        iconName = "MRBv8";
                }
                else
                {
                    if (nSurfaceTemperature < 1600)
                        iconName = "MRBv9";
                    else if (nSurfaceTemperature < 1800)
                        iconName = "MRBv3";
                    else if (nSurfaceTemperature < 1900)
                        iconName = "MRBv4";
                    else if (nSurfaceTemperature < 2000)
                        iconName = "MRBv10";
                    else if (nSurfaceTemperature < 2200)
                        iconName = "MRBv11";
                    else if (nSurfaceTemperature < 2400)
                        iconName = "MRBv14";
                    else if (nSurfaceTemperature < 2600)
                        iconName = "MRBv8";
                    else if (nSurfaceTemperature < 3500)
                        iconName = "MRBv13";
                    else if (nSurfaceTemperature < 5000)
                        iconName = "MRBv1";
                    else if (nSurfaceTemperature < 6000)
                        iconName = "MRBv5";
                    else
                        iconName = "MRBv6";
                }
            }

            if (PlanetTypeID == EDPlanet.Rocky_body)
            {
                iconName = "RBDv1"; // fallback

                if (nSurfaceTemperature == 55 && nLandable != true) // pluto (actually, pluto is a rocky-ice body, in real life; however, the game consider it a rocky body. Too bad...)
                    iconName = "RBDv6";
                else if (nSurfaceTemperature < 150)
                    iconName = "RBDv2";
                else if (nSurfaceTemperature < 300)
                    iconName = "RBDv1";
                else if (nSurfaceTemperature < 400)
                    iconName = "RBDv3";
                else if (nSurfaceTemperature < 500)
                    iconName = "RBDv4";
                else // for high temperature rocky bodies
                    iconName = "RBDv5";
            }

            if (PlanetTypeID == EDPlanet.Rocky_ice_body)
            {
                iconName = "RIBv1"; // fallback

                if (nSurfaceTemperature < 50)
                    iconName = "RIBv1";
                else if (nSurfaceTemperature < 150)
                    iconName = "RIBv2";
                else
                    iconName = "RIBv4";

                if (nTidalLock == true)
                    iconName = "RIBv3";
                else
                {
                    if (AtmosphereProperty == (EDAtmosphereProperty.Thick | EDAtmosphereProperty.Rich))
                    {
                        iconName = "RIBv4";
                    }
                    else if (AtmosphereProperty == (EDAtmosphereProperty.Hot | EDAtmosphereProperty.Thin))
                        iconName = "RIBv1";
                }
            }

            if (PlanetTypeID == EDPlanet.Water_world)
            {
                iconName = "WTRv7"; // fallback

                if (AtmosphereID == EDAtmosphereType.No)
                {
                    iconName = "WTRv10"; // kindly provided by CMDR CompleteNOOB
                }
                else
                {
                    if (AtmosphereID == EDAtmosphereType.Carbon_Dioxide)
                    {
                        if (nSurfaceTemperature < 260)
                            iconName = "WTRv6";
                        else if (nSurfaceTemperature < 280)
                            iconName = "WTRv5";
                        else if (nSurfaceTemperature < 300)
                            iconName = "WTRv7";
                        else if (nSurfaceTemperature < 400)
                            iconName = "WTRv2";
                        else
                            iconName = "WTRv11"; // kindly provided by CMDR Eahlstan
                    }
                    else if (AtmosphereID == EDAtmosphereType.Ammonia)
                    {
                        if (nTidalLock == true)
                            iconName = "WTRv12"; // kindly provided by CMDR Eahlstan
                        else
                        {
                            if (nSurfaceTemperature < 275)
                                iconName = "WTRv1";
                            else if (nSurfaceTemperature < 350)
                                iconName = "WTRv13"; // kindly provided by CMDR Eahlstan
                            else if (nSurfaceTemperature < 380)
                                iconName = "WTRv9"; // kindly provided by CMDR CompleteNOOB
                            else
                                iconName = "WTRv4";
                        }
                    }
                    else if (AtmosphereID == EDAtmosphereType.Nitrogen)
                    {
                        if (nSurfaceTemperature < 250)
                            iconName = "WTRv3";
                        else
                            iconName = "WTRv8";
                    }
                    else
                        iconName = "WTRv7"; // fallback
                }
            }

            //System.Diagnostics.Debug.WriteLine(PlanetTypeID + ": " + iconName);
            return $"Bodies.Planets.Terrestrial.{iconName}";
        }

        static public System.Drawing.Image GetImageAtmosphere()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Controls.Scan.Bodies.Atmosphere");
        }
        static public System.Drawing.Image GetImageLandable()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Landable");
        }
        static public System.Drawing.Image GetImageRingGap()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingGap");
        }
        static public System.Drawing.Image GetImageRingOnly()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingOnly");
        }
        static public System.Drawing.Image GetImageTerraFormable()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Terraformable");
        }
        static public System.Drawing.Image GetImageVolcanism()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Volcanism");
        }
        static public System.Drawing.Image GetImageHighValue()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.HighValue");
        }
        static public System.Drawing.Image GetImageMapped()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Mapped");
        }
        static public System.Drawing.Image GetImageMappedByOthers()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.MappedByOthers");
        }
        static public System.Drawing.Image GetImageDiscoveredByOthers()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.DiscoveredByOthers");
        }
        static public System.Drawing.Image GetImageCodexEntry()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Journal.CodexEntry");
        }
        static public System.Drawing.Image GetImageMoreMaterials()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.MaterialMore");
        }
        static public System.Drawing.Image GetImageOrganicsScanned()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Journal.ScanOrganic");
        }
        static public System.Drawing.Image GetImageOrganicsIncomplete()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.OrganicIncomplete");
        }
        static public System.Drawing.Image GetImageSignals()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Signals");
        }
        static public System.Drawing.Image GetImageGeoBioSignals()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.SignalsGeoBio");
        }
        static public System.Drawing.Image GetImageBioSignals()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.SignalsBio");
        }
        static public System.Drawing.Image GetImageGeoSignals()
        {
            return BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.SignalsGeo");
        }
        static public System.Drawing.Image GetImageNotScanned()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Bodies.NotScanned");
        }
        static public System.Drawing.Image GetImageBarycentre()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Controls.Scan.Bodies.Barycentre");
        }
        static public System.Drawing.Image GetImageBarycentreLeftBar()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Controls.Scan.Bodies.BarycentreLeftBar");
        }
        static public System.Drawing.Image GetImageBeltCluster()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Controls.Scan.Bodies.Belt");
        }
        static public System.Drawing.Image GetImageBeltBody()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Controls.Scan.SizeLarge");
        }

        // return bitmaps of stars, cropped.  You own the bitmaps afterwards as images are cloned
        static public Bitmap[] StarBitmaps(RectangleF croparea)
        {
            Bitmap[] bitmaps = new Bitmap[Enum.GetNames(typeof(EDStar)).Length];
            int bm = 0;
            foreach (EDStar star in Enum.GetValues(typeof(EDStar)))
            {
                string name = StarTypeImageName(star, 1, 5000);
                bitmaps[bm] = ((Bitmap)BaseUtils.Icons.IconSet.GetIcon(name)).CropImage(croparea);
                bm++;
            }

            return bitmaps;
        }

        static public Dictionary<EDStar, Color> StarColourKey()
        {
            Dictionary<EDStar, Color> map = new Dictionary<EDStar, Color>();

            foreach (EDStar star in Enum.GetValues(typeof(EDStar)))
            {
                string name = StarTypeImageName(star, 1, 5000);
                Bitmap b = (Bitmap)BaseUtils.Icons.IconSet.GetIcon(name);
                Color c = b.AverageColour(new RectangleF(20, 20, 60, 60));
                //System.Diagnostics.Debug.WriteLine($"Star {star} name {name} Colour {c}");
                map[star] = Color.FromArgb(255, c);
            }

            return map;
        }

        // use for checking..
        static public void DebugDisplayStarColourKey(ExtendedControls.ExtPictureBox imagebox, Font font)
        {
            var sil = BodyDefinitions.StarColourKey();
            int i = 0;
            foreach (var kvp in sil)
            {
                int x = (i % 10) * 150;
                int y = (i / 10) * 50 + 4;
                imagebox.AddOwnerDraw((g, e) => {
                    using (var b = new SolidBrush(kvp.Value))
                    {
                        using (var bt = new SolidBrush(Color.FromArgb(255, 0, 255, 255)))
                        {
                            g.FillRectangle(b, e.Location);
                            g.DrawString($"{kvp.Key}", font, bt, new Point(e.Location.X, e.Location.Y + 20));
                        }
                    }
                }, new Rectangle(x, y, 140, 40));

                i++;
            }
        }

    }

}
