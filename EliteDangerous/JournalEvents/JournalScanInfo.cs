/*
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
    public partial class JournalScan : JournalEntry
    {
        public string GetStarPlanetTypeImageName()
        {
            return IsStar ? StarTypeImageName : PlanetClassImageName;
        }

        public string StarTypeImageName                      // property so its gets outputted via JSON
        {
            get
            {
                // parameters needed for variations
                var sm = nStellarMass;
                var st = nSurfaceTemperature;

                string iconName = StarTypeID.ToString(); // fallback

                // black holes variations. According to theorethical papers, we should find four classes of black holes:
                // supermassive, intermediate, stellar and micro (https://en.wikipedia.org/wiki/Black_hole) gravitional collapses.
                // The in-game black hole population do not really fit quite well that nomenclature, they are somewhat capped, so we have to quantize the masses ranges to more reasonable limits.
                // For example, SagA* is known to have at least 4 millions solar masses, in real life: the in-game stats shows only 516.608, way smaller! The same for Great Annihilator: the biggest
                // of the couple should be more than some hundrends of thousands of solar masses, which is the expected mass range of an intermediate black holes...
                if (StarTypeID == EDStar.H)
                {
                    if (sm <= 7.0)
                        iconName = "H_stellar";
                    else if (sm < 70.0)
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
                    if (sm < 1.1)
                        iconName = "N";
                    else if (sm < 1.9)
                        iconName = "N_massive";
                    else
                        iconName = "N_veryMassive";
                }

                // white dwarfs variations
                if (StarTypeID == EDStar.D || StarTypeID == EDStar.DA || StarTypeID == EDStar.DAB || StarTypeID == EDStar.DAO || StarTypeID == EDStar.DAV || StarTypeID == EDStar.DAZ || StarTypeID == EDStar.DB ||
                    StarTypeID == EDStar.DBV || StarTypeID == EDStar.DBZ || StarTypeID == EDStar.DC || StarTypeID == EDStar.DCV || StarTypeID == EDStar.DO || StarTypeID == EDStar.DOV || StarTypeID == EDStar.DQ ||
                    StarTypeID == EDStar.DX)
                {
                    if (st <= 5500)
                        iconName = "D";
                    else if (st < 8000)
                        iconName = "D_hot";
                    else if (st < 14000)
                        iconName = "D_veryHot";
                    else if (st >= 14000)
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
                    if (st < 5000)
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
                    if (st < 3700)
                        iconName = "M";
                    else if (st < 5200)
                        iconName = "K";
                    else if (st < 6000)
                        iconName = "G";
                    else if (st < 7500)
                        iconName = "F";
                    else if (st < 10000)
                        iconName = "A";
                    else if (st < 30000)
                        iconName = "B";
                    else
                        iconName = "O";
                }

                // wolf-rayets stars
                // https://en.wikipedia.org/wiki/Wolf%E2%80%93Rayet_star
                if (StarTypeID == EDStar.W || StarTypeID == EDStar.WC || StarTypeID == EDStar.WN || StarTypeID == EDStar.WNC || StarTypeID == EDStar.WO)
                {
                    if (st < 50000)
                        iconName = "F";
                    if (st < 90000)
                        iconName = "A";
                    if (st < 140000)
                        iconName = "B";
                    if (st > 140000)
                        iconName = "O";
                }

                if (StarTypeID == EDStar.MS || StarTypeID == EDStar.S)
                    iconName = "M";

                if (StarTypeID == EDStar.Nebula)
                    return "Bodies.Nebula";

                if (StarTypeID == EDStar.StellarRemnantNebula)
                    return $"Bodies.StellarRemnantNebula";

                if (StarTypeID == EDStar.X || StarTypeID == EDStar.RoguePlanet)
                {
                    // System.Diagnostics.Debug.WriteLine(StarTypeID + ": " + iconName);
                    return "Bodies.Unknown";
                }
                else
                {
                    //   System.Diagnostics.Debug.WriteLine(StarTypeID + ": " + iconName);
                    return $"Bodies.Stars.{iconName}";
                }
            }
        }

        public string PlanetClassImageName       // property so its gets outputted via JSON
        {
            get
            {
                var st = nSurfaceTemperature;

                if (!IsPlanet)
                {
                    return $"Bodies.Unknown";
                }

                string iconName = PlanetTypeID.ToString();

                // Gas Giants variants
                if (PlanetTypeID.ToNullSafeString().ToLowerInvariant().Contains("giant"))
                {
                    iconName = "GG1v1"; // fallback

                    if (PlanetTypeID == EDPlanet.Gas_giant_with_ammonia_based_life)
                    {
                        if (st < 105)
                            iconName = "GGAv8";
                        else if (st < 110)
                            iconName = "GGAv11";
                        else if (st < 115)
                            iconName = "GGAv9";
                        else if (st < 120)
                            iconName = "GGAv2";
                        else if (st < 124)
                            iconName = "GGAv12";
                        else if (st < 128)
                            iconName = "GGAv14";
                        else if (st < 130)
                            iconName = "GGAv7";
                        else if (st < 134)
                            iconName = "GGAv13";
                        else if (st < 138)
                            iconName = "GGAv6";
                        else if (st < 142)
                            iconName = "GGAv1";
                        else if (st < 148)
                            iconName = "GGAv3";
                        else if (st < 152)
                            iconName = "GGAv5";
                        else
                            iconName = "GGAv4";
                    }

                    if (PlanetTypeID == (EDPlanet.Water_giant_with_life | EDPlanet.Gas_giant_with_water_based_life))
                    {
                        if (st < 152)
                            iconName = "GGWv24";
                        else if (st < 155)
                        {
                            if (AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("oxygen"))
                                iconName = "GGWv1";
                            else
                                iconName = "GGWv16";
                        }
                        else if (st < 158)
                            iconName = "GGWv3";
                        else if (st < 160)
                            iconName = "GGWv14";
                        else if (st < 162)
                            iconName = "GGWv22";
                        else if (st < 165)
                            iconName = "GGWv20";
                        else if (st < 172)
                            iconName = "GGWv25";
                        else if (st < 175)
                            iconName = "GGWv2";
                        else if (st < 180)
                            iconName = "GGWv13";
                        else if (st < 185)
                            iconName = "GGWv9";
                        else if (st < 190)
                            iconName = "GGWv21";
                        else if (st < 200)
                            iconName = "GGWv7";
                        else if (st < 205)
                            iconName = "GGWv8";
                        else if (st < 210)
                            iconName = "GGWv15";
                        else if (st < 213)
                            iconName = "GGWv17";
                        else if (st < 216)
                            iconName = "GGWv6";
                        else if (st < 219)
                            iconName = "GGWv18";
                        else if (st < 222)
                            iconName = "GGWv10";
                        else if (st < 225)
                            iconName = "GGWv11";
                        else if (st < 228)
                            iconName = "GGWv23";
                        else if (st < 232)
                            iconName = "GGWv5";
                        else if (st < 236)
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
                        if (st < 110)
                        {
                            if (AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("antimony") ||
                                AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("cadmium") ||
                                AtmosphereComposition.ToNullSafeString().ToLowerInvariant().Contains("niobium"))
                                iconName = "GGHv7";
                            else
                                iconName = "GGHv3";
                        }
                        else if (st < 125)
                            iconName = "GGHv6";
                        else if (st < 140)
                            iconName = "GGHv2";
                        else if (st < 180)
                            iconName = "GGHv5";
                        else if (st < 270)
                            iconName = "GGHv4";
                        else if (st < 600)
                            iconName = "GGHv1";
                        else if (st < 700)
                            iconName = "GGHv9";
                        else
                            iconName = "GGHv8";
                    }

                    if (PlanetTypeID == EDPlanet.Sudarsky_class_I_gas_giant)
                    {

                        if (st <= 30)
                            iconName = "GG1v12";
                        else if (st < 35)
                            iconName = "GG1v15";
                        else if (st < 40)
                            iconName = "GG1v13";
                        else if (st < 45)
                            iconName = "GG1v4"; // neptune
                        else if (st < 50)
                            iconName = "GG1v9";
                        else if (st < 55)
                            iconName = "GG1v2";
                        else if (st < 60)
                            iconName = "GG1v16"; // uranus
                        else if (st < 65)
                            iconName = "GG1v19";
                        else if (st < 70)
                            iconName = "GG1v18";
                        else if (st < 78)
                            iconName = "GG1v11";
                        else if (st < 85)
                            iconName = "GG1v3";
                        else if (st < 90)
                            iconName = "GG1v6";
                        else if (st < 100)
                            iconName = "GG1v8";
                        else if (st < 110)
                            iconName = "GG1v1";
                        else if (st < 130)
                            iconName = "GG1v5";
                        else if (st < 135)
                            iconName = "GG1v17";
                        else if (st < 140)
                            iconName = "GG1v20";
                        else if (st < 150)
                            iconName = "GG1v14"; // jupiter
                        else if (st < 170)
                            iconName = "GG1v7";
                        else
                            iconName = "GG1v10";
                    }

                    if (PlanetTypeID == EDPlanet.Sudarsky_class_II_gas_giant)
                    {
                        if (st < 160)
                            iconName = "GG2v4";
                        else if (st < 175)
                            iconName = "GG2v7";
                        else if (st < 200)
                            iconName = "GG2v5";
                        else if (st < 2450)
                            iconName = "GG2v8";
                        else if (st < 260)
                            iconName = "GG2v6";
                        else if (st < 275)
                            iconName = "GG2v1";
                        else if (st < 300)
                            iconName = "GG2v2";
                        else
                            iconName = "GG2v3";
                    }

                    if (PlanetTypeID == EDPlanet.Sudarsky_class_III_gas_giant)
                    {
                        if (st < 300)
                            iconName = "GG3v2";
                        else if (st < 340)
                            iconName = "GG3v3";
                        else if (st < 370)
                            iconName = "GG3v12";
                        else if (st < 400)
                            iconName = "GG3v1";
                        else if (st < 500)
                            iconName = "GG3v5";
                        else if (st < 570)
                            iconName = "GG3v4";
                        else if (st < 600)
                            iconName = "GG3v8";
                        else if (st < 620)
                            iconName = "GG3v10";
                        else if (st < 660)
                            iconName = "GG3v7";
                        else if (st < 700)
                            iconName = "GG3v9";
                        else if (st < 742)
                            iconName = "GG3v11";
                        else if (st < 760)
                            iconName = "GG3v13";
                        else
                            iconName = "GG3v6";
                    }

                    if (PlanetTypeID == EDPlanet.Sudarsky_class_IV_gas_giant)
                    {
                        if (st < 810)
                            iconName = "GG4v9";
                        else if (st < 830)
                            iconName = "GG4v6";
                        else if (st < 880)
                            iconName = "GG4v4";
                        else if (st < 950)
                            iconName = "GG4v10";
                        else if (st < 1010)
                            iconName = "GG4v3";
                        else if (st < 1070)
                            iconName = "GG4v1";
                        else if (st < 1125)
                            iconName = "GG4v7";
                        else if (st < 1200)
                            iconName = "GG4v2";
                        else if (st < 1220)
                            iconName = "GG4v13";
                        else if (st < 1240)
                            iconName = "GG4v11";
                        else if (st < 1270)
                            iconName = "GG4v8";
                        else if (st < 1300)
                            iconName = "GG4v12";
                        else
                            iconName = "GG4v5";
                    }

                    if (PlanetTypeID == EDPlanet.Sudarsky_class_V_gas_giant)
                    {
                        if (st < 1600)
                            iconName = "GG5v3";
                        else if (st < 1620)
                            iconName = "GG5v4";
                        else if (st < 1700)
                            iconName = "GG5v1";
                        else if (st < 1850)
                            iconName = "GG5v2";
                        else
                            iconName = "GG5v5";
                    }

                    if (PlanetTypeID == EDPlanet.Water_giant)
                    {
                        if (st < 155)
                            iconName = "WTGv6";
                        else if (st < 160)
                            iconName = "WTGv2";
                        else if (st < 165)
                            iconName = "WTGv1";
                        else if (st < 170)
                            iconName = "WTGv3";
                        else if (st < 180)
                            iconName = "WTGv4";
                        else if (st < 190)
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
                    else if (nLandable == true || (AtmosphereID == EDAtmosphereType.No_atmosphere && st < 140))
                        iconName = "AMWv5"; // kindly provided by CMDR CompleteNOOB
                    else if (st < 190)
                        iconName = "AMWv6";
                    else if (st < 200)
                        iconName = "AMWv3";
                    else if (st < 210)
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

                    if ((int)nMassEM == 1 && st == 288) // earth, or almost identical to
                    {
                        iconName = "ELWv1";
                    }
                    else
                    {
                        if (nTidalLock == true)
                            iconName = "ELWv7";
                        else
                        {
                            if (nMassEM < 0.15 && st < 262) // mars, or extremely similar to
                                iconName = "ELWv4";
                            else if (st < 270)
                                iconName = "ELWv8";
                            else if (st < 285)
                                iconName = "ELWv2";
                            else if (st < 300)
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
                    if (nLandable == true || AtmosphereID == EDAtmosphereType.No_atmosphere)
                    {
                        if (st < 300)
                        {
                            if (nTidalLock == true)
                                iconName = "HMCv30";
                            else
                                iconName = "HMCv27"; // kindly provided by CMDR CompleteNOOB
                        }
                        else if (st < 500)
                            iconName = "HMCv34";
                        else if (st < 700)
                            iconName = "HMCv32";
                        else if (st < 900)
                            iconName = "HMCv31";
                        else if (st < 1000)
                        {
                            if (nTidalLock == true)
                                iconName = "HMCv33";
                            else
                                iconName = "HMCv35";
                        }
                        else if (st >= 1000)
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
                        else if (AtmosphereID == EDAtmosphereType.Carbon_dioxide)
                        {
                            if (st < 220)
                                iconName = "HMCv9";
                            else if (st < 250)
                                iconName = "HMCv12";
                            else if (st < 285)
                                iconName = "HMCv6";
                            else if (st < 350)
                                iconName = "HMCv28";
                            else if (st < 400)
                            {
                                if (nTidalLock == true)
                                    iconName = "HMCv7";
                                else
                                    iconName = "HMCv8";
                            }
                            else if (st < 600)
                            {
                                if (nTidalLock == true)
                                    iconName = "HMCv1";
                                else
                                    iconName = "HMCv24";
                            }
                            else if (st < 700)
                                iconName = "HMCv3";
                            else if (st < 900)
                                iconName = "HMCv25";
                            else if (st > 1250)
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
                            if (st < 200)
                                iconName = "HMCv2";
                            else
                                iconName = "HMCv5";
                        }
                        else if (AtmosphereID == EDAtmosphereType.Sulphur_dioxide)
                        {
                            if (st < 700)
                                iconName = "HMCv23";
                            else
                                iconName = "HMCv37"; // kindly provided by CMDR CompleteNOOB
                        }
                        else if (AtmosphereID == EDAtmosphereType.Water)
                        {
                            if (st < 400)
                                iconName = "HMCv4";
                            else if (st < 700)
                                iconName = "HMCv13";
                            else if (st < 1000)
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
                            if (st < 55)
                                iconName = "ICYv6";
                            else
                                iconName = "ICYv9";
                        }
                        else if (AtmosphereID == EDAtmosphereType.Argon)
                        {
                            if (st < 100)
                                iconName = "ICYv1";
                            else
                                iconName = "ICYv5";
                        }
                        else if (AtmosphereID == EDAtmosphereType.Nitrogen)
                        {
                            if (st < 105)
                                iconName = "ICYv2";
                            else if (st < 150)
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
                        if (st < 1000)
                            iconName = "MRBv7";
                        else if (st < 1200)
                            iconName = "MRBv2";
                        else if (st < 2000)
                            iconName = "MRBv12";
                        else
                            iconName = "MRBv8";
                    }
                    else
                    {
                        if (st < 1600)
                            iconName = "MRBv9";
                        else if (st < 1800)
                            iconName = "MRBv3";
                        else if (st < 1900)
                            iconName = "MRBv4";
                        else if (st < 2000)
                            iconName = "MRBv10";
                        else if (st < 2200)
                            iconName = "MRBv11";
                        else if (st < 2400)
                            iconName = "MRBv14";
                        else if (st < 2600)
                            iconName = "MRBv8";
                        else if (st < 3500)
                            iconName = "MRBv13";
                        else if (st < 5000)
                            iconName = "MRBv1";
                        else if (st < 6000)
                            iconName = "MRBv5";
                        else
                            iconName = "MRBv6";
                    }
                }

                if (PlanetTypeID == EDPlanet.Rocky_body)
                {
                    iconName = "RBDv1"; // fallback

                    if (st == 55 && !IsLandable) // pluto (actually, pluto is a rocky-ice body, in real life; however, the game consider it a rocky body. Too bad...)
                        iconName = "RBDv6";
                    else if (st < 150)
                        iconName = "RBDv2";
                    else if (st < 300)
                        iconName = "RBDv1";
                    else if (st < 400)
                        iconName = "RBDv3";
                    else if (st < 500)
                        iconName = "RBDv4";
                    else // for high temperature rocky bodies
                        iconName = "RBDv5";
                }

                if (PlanetTypeID == EDPlanet.Rocky_ice_body)
                {
                    iconName = "RIBv1"; // fallback

                    if (st < 50)
                        iconName = "RIBv1";
                    else if (st < 150)
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

                    if (AtmosphereID == EDAtmosphereType.No_atmosphere)
                    {
                        iconName = "WTRv10"; // kindly provided by CMDR CompleteNOOB
                    }
                    else
                    {
                        if (AtmosphereID == EDAtmosphereType.Carbon_dioxide)
                        {
                            if (st < 260)
                                iconName = "WTRv6";
                            else if (st < 280)
                                iconName = "WTRv5";
                            else if (st < 300)
                                iconName = "WTRv7";
                            else if (st < 400)
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
                                if (st < 275)
                                    iconName = "WTRv1";
                                else if (st < 350)
                                    iconName = "WTRv13"; // kindly provided by CMDR Eahlstan
                                else if (st < 380)
                                    iconName = "WTRv9"; // kindly provided by CMDR CompleteNOOB
                                else
                                    iconName = "WTRv4";
                            }
                        }
                        else if (AtmosphereID == EDAtmosphereType.Nitrogen)
                        {
                            if (st < 250)
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
        }

        static public System.Drawing.Image GetPlanetImageNotScanned()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Bodies.Unknown");
        }

        static public System.Drawing.Image GetMoonImageNotScanned()
        {
            return BaseUtils.Icons.IconSet.GetIcon($"Bodies.Unknown");
        }

        public string ShortInformation()
        {
            if (IsStar)
            {
                return BaseUtils.FieldBuilder.Build("Mass: ;SM;0.00".T(EDCTx.JournalScan_MSM), nStellarMass,
                                                "Age: ;my;0.0".T(EDCTx.JournalScan_Age), nAge,
                                                "Radius: ".T(EDCTx.JournalScan_RS), RadiusText(),
                                                "Dist: ".T(EDCTx.JournalScan_DIST), DistanceFromArrivalText);
            }
            else
            {
                return BaseUtils.FieldBuilder.Build("Mass: ".T(EDCTx.JournalScan_MASS), MassEMText(),
                                                 "Radius: ".T(EDCTx.JournalScan_RS), RadiusText(),
                                                 "Dist: ".T(EDCTx.JournalScan_DIST), DistanceFromArrivalText);
            }
        }

        public string SurveyorInfoLine(ISystem sys,
                            bool hasminingsignals, bool hasgeosignals, bool hasbiosignals, bool hasthargoidsignals, bool hasguardiansignals, bool hashumansignals, bool hasothersignals,
                            bool hasscanorganics,
                            bool showvolcanism, bool showvalues, bool shortinfo, bool showGravity, bool showAtmos, bool showRings, 
                            int lowRadiusLimit, int largeRadiusLimit, double eccentricityLimit)
        {
            JournalScan js = this;

            var information = new StringBuilder();

            if (js.Mapped)
                information.Append("\u2713"); // let the cmdr see that this body is already mapped - this is a check

            string bodyname = js.BodyDesignationOrName.ReplaceIfStartsWith(sys.Name);

            // Name
            information.Append((bodyname) + @" is a ".T(EDCTx.JournalScanInfo_isa));

            // Additional information
            information.Append((js.IsStar) ? Bodies.StarName(js.StarTypeID) + "." : null);
            information.Append((js.CanBeTerraformable) ? @"terraformable ".T(EDCTx.JournalScanInfo_terraformable) : null);
            information.Append((js.IsPlanet) ? Bodies.PlanetTypeName(js.PlanetTypeID) + "." : null);
            information.Append((js.nRadius < lowRadiusLimit && js.IsPlanet) ? @" Is tiny ".T(EDCTx.JournalScanInfo_LowRadius) + "(" + RadiusText() + ")." : null);
            information.Append((js.nRadius > largeRadiusLimit && js.IsPlanet && js.IsLandable) ? @" Is large ".T(EDCTx.JournalScanInfo_LargeRadius) + "(" + RadiusText() + ")." : null);
            information.Append((js.IsLandable) ? @" Is landable.".T(EDCTx.JournalScanInfo_islandable) : null);
            information.Append((js.IsLandable && showGravity && js.nSurfaceGravityG.HasValue) ? @" (" + Math.Round(js.nSurfaceGravityG.Value, 2, MidpointRounding.AwayFromZero) + "g)" : null);
            information.Append((js.HasAtmosphericComposition && showAtmos) ? @" Atmosphere: ".T(EDCTx.JournalScanInfo_Atmosphere) + (js.Atmosphere?.Replace(" atmosphere", "") ?? "unknown".T(EDCTx.JournalScanInfo_unknownAtmosphere)) + "." : null);
            information.Append((js.HasMeaningfulVolcanism && showvolcanism) ? @" Has ".T(EDCTx.JournalScanInfo_Has) + js.Volcanism + "." : null);
            information.Append((hasminingsignals) ? " Has mining signals.".T(EDCTx.JournalScanInfo_Signals) : null);
            information.Append((hasgeosignals) ? " Has geological signals.".T(EDCTx.JournalScanInfo_GeoSignals) : null);
            information.Append((hasbiosignals) ? " Has biological signals.".T(EDCTx.JournalScanInfo_BioSignals) : null);
            information.Append((hasthargoidsignals) ? " Has thargoid signals.".T(EDCTx.JournalScanInfo_ThargoidSignals) : null);
            information.Append((hasguardiansignals) ? " Has guardian signals.".T(EDCTx.JournalScanInfo_GuardianSignals) : null);
            information.Append((hashumansignals) ? " Has human signals.".T(EDCTx.JournalScanInfo_HumanSignals) : null);
            information.Append((hasothersignals) ? " Has 'other' signals.".T(EDCTx.JournalScanInfo_OtherSignals) : null);
            information.Append((js.HasRingsOrBelts && showRings) ? @" Is ringed.".T(EDCTx.JournalScanInfo_Hasring) : null);
            information.Append((js.nEccentricity >= eccentricityLimit) ? @" Has an high eccentricity of ".T(EDCTx.JournalScanInfo_eccentricity) + js.nEccentricity + "." : null);
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
                    information.Append(' ').Append(ev.EstimatedValueFirstMappedEfficiently.ToString("N0")).Append(" cr");
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


        public void AccumulateJumponium(ref string jumponium, string sysname)
        {
            if (IsLandable == true && HasMaterials) // Landable bodies with valuable materials, collect into jumponimum
            {
                int basic = 0;
                int standard = 0;
                int premium = 0;

                foreach (KeyValuePair<string, double> mat in Materials)
                {
                    string usedin = Recipes.UsedInSythesisByFDName(mat.Key);
                    if (usedin.Contains("FSD-Basic"))
                        basic++;
                    if (usedin.Contains("FSD-Standard"))
                        standard++;
                    if (usedin.Contains("FSD-Premium"))
                        premium++;
                }

                if (basic > 0 || standard > 0 || premium > 0)
                {
                    int mats = basic + standard + premium;

                    StringBuilder jumpLevel = new StringBuilder();

                    if (basic != 0)
                        jumpLevel.AppendPrePad(basic + "/" + Recipes.FindSynthesis("FSD", "Basic").Count + " Basic".T(EDCTx.JournalScanInfo_BFSD), ", ");
                    if (standard != 0)
                        jumpLevel.AppendPrePad(standard + "/" + Recipes.FindSynthesis("FSD", "Standard").Count + " Standard".T(EDCTx.JournalScanInfo_SFSD), ", ");
                    if (premium != 0)
                        jumpLevel.AppendPrePad(premium + "/" + Recipes.FindSynthesis("FSD", "Premium").Count + " Premium".T(EDCTx.JournalScanInfo_PFSD), ", ");

                    jumponium = jumponium.AppendPrePad(string.Format("{0} has {1} level elements.".T(EDCTx.JournalScanInfo_LE), sysname, jumpLevel), Environment.NewLine);
                }
            }
        }
    }

}


