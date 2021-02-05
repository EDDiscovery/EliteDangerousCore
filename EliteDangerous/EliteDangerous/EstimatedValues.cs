/*
 * Copyright © 2021 EDDiscovery development team
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

using System;

namespace EliteDangerousCore
{
    public class ScanEstimatedValues
    {
        public ScanEstimatedValues(DateTime utc, bool isstar , EDStar st, bool isplanet, EDPlanet pl, bool terraformable, double? massstar, double? massem)
        {
            // see https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae/ for detail

            if (utc < new DateTime(2017, 4, 11, 12, 0, 0, 0, DateTimeKind.Utc))
            {
                EstimatedValueBase = EstimatedValueED22(isstar, st, isplanet, pl, terraformable, massstar, massem);
                return;
            }

            if (utc < new DateTime(2018, 12, 11, 9, 0, 0, DateTimeKind.Utc))
            {
                EstimatedValueBase = EstimatedValue32(isstar, st, isplanet, pl, terraformable, massstar, massem);
                return;
            }

            // 3.3 onwards

            //System.Diagnostics.Debug.WriteLine("Scan calc " + mapped + " ef " + efficient + " Current " + EstimatedValue);

            double kValue;

            if (isstar)
            {
                switch (st)
                {
                    // white dwarf
                    case EDStar.D:
                    case EDStar.DA:
                    case EDStar.DAB:
                    case EDStar.DAO:
                    case EDStar.DAZ:
                    case EDStar.DAV:
                    case EDStar.DB:
                    case EDStar.DBZ:
                    case EDStar.DBV:
                    case EDStar.DO:
                    case EDStar.DOV:
                    case EDStar.DQ:
                    case EDStar.DC:
                    case EDStar.DCV:
                    case EDStar.DX:
                        kValue = 14057;
                        break;

                    case EDStar.N:
                    case EDStar.H:
                        kValue = 22628;
                        break;

                    case EDStar.SuperMassiveBlackHole:
                        // this is applying the same scaling to the 3.2 value as a normal black hole, not confirmed in game
                        kValue = 33.5678;
                        break;

                    default:
                        kValue = 1200;
                        break;
                }

                EstimatedValueBase = (int)StarValue32And33(kValue, massstar.HasValue ? massstar.Value : 1.0);
            }
            else
            {
                EstimatedValueBase = 0;

                if (isplanet)  //Asteroid belt is null
                {
                    switch (pl)
                    {
                        case EDPlanet.Metal_rich_body:
                            // CFT value is scaled same as WW/ELW from 3.2, not confirmed in game
                            // They're like hen's teeth anyway....
                            kValue = 21790;
                            if (terraformable) kValue += 65631;
                            break;
                        case EDPlanet.Ammonia_world:
                            kValue = 96932;
                            break;
                        case EDPlanet.Sudarsky_class_I_gas_giant:
                            kValue = 1656;
                            break;
                        case EDPlanet.Sudarsky_class_II_gas_giant:
                        case EDPlanet.High_metal_content_body:
                            kValue = 9654;
                            if (terraformable) kValue += 100677;
                            break;
                        case EDPlanet.Water_world:
                            kValue = 64831;
                            if (terraformable) kValue += 116295;
                            break;
                        case EDPlanet.Earthlike_body:
                            // Always terraformable so WW + bonus
                            kValue = 64831 + 116295;
                            break;
                        default:
                            kValue = 300;
                            if (terraformable) kValue += 93328;
                            break;
                    }

                    double mass = massem.HasValue ? massem.Value : 1.0;
                    double effmapped = 1.25;
                    double firstdiscovery = 2.6;

                    double basevalue = PlanetValue33(kValue, mass);

                    EstimatedValueBase = (int)basevalue;

                    EstimatedValueFirstDiscovered = (int)(basevalue * firstdiscovery);

                    EstimatedValueFirstDiscoveredFirstMapped = (int)(basevalue * firstdiscovery * 3.699622554);
                    EstimatedValueFirstDiscoveredFirstMappedEfficiently = (int)(basevalue * firstdiscovery * 3.699622554 * effmapped);

                    EstimatedValueFirstMapped = (int)(basevalue * 8.0956);
                    EstimatedValueFirstMappedEfficiently = (int)(basevalue * 8.0956 * effmapped);

                    EstimatedValueMapped = (int)(basevalue * 3.3333333333);
                    EstimatedValueMappedEfficiently = (int)(basevalue * 3.3333333333 * effmapped);
                }
            }
        }

        private double StarValue32And33(double k, double m)
        {
            return k + (m * k / 66.25);
        }

        private double PlanetValue33(double k, double m)
        {
            const double q = 0.56591828;
            return Math.Max((k + (k * Math.Pow(m, 0.2) * q)), 500);
        }

        #region ED 3.2 values

        private int EstimatedValue32(bool isstar, EDStar st, bool isplanet, EDPlanet pl, bool terraformable, double? massstar, double? massem)
        {
            double kValue;
            double kBonus = 0;

            if (isstar)
            {
                switch (st)      // http://elite-dangerous.wikia.com/wiki/Explorer
                {
                    // white dwarf
                    case EDStar.D:
                    case EDStar.DA:
                    case EDStar.DAB:
                    case EDStar.DAO:
                    case EDStar.DAZ:
                    case EDStar.DAV:
                    case EDStar.DB:
                    case EDStar.DBZ:
                    case EDStar.DBV:
                    case EDStar.DO:
                    case EDStar.DOV:
                    case EDStar.DQ:
                    case EDStar.DC:
                    case EDStar.DCV:
                    case EDStar.DX:
                        kValue = 33737;
                        break;

                    case EDStar.N:
                    case EDStar.H:
                        kValue = 54309;
                        break;

                    case EDStar.SuperMassiveBlackHole:
                        kValue = 80.5654;
                        break;

                    default:
                        kValue = 2880;
                        break;
                }

                return (int)StarValue32And33(kValue, massstar.HasValue ? massstar.Value : 1.0);
            }
            else if (!isplanet)  //Asteroid belt
                return 0;
            else   // Planet
            {
                switch (pl)      // http://elite-dangerous.wikia.com/wiki/Explorer
                {

                    case EDPlanet.Metal_rich_body:
                        kValue = 52292;
                        if (terraformable) { kBonus = 245306; }
                        break;
                    case EDPlanet.High_metal_content_body:
                    case EDPlanet.Sudarsky_class_II_gas_giant:
                        kValue = 23168;
                        if (terraformable) { kBonus = 241607; }
                        break;
                    case EDPlanet.Earthlike_body:
                        kValue = 155581;
                        kBonus = 279088;
                        break;
                    case EDPlanet.Water_world:
                        kValue = 155581;
                        if (terraformable) { kBonus = 279088; }
                        break;
                    case EDPlanet.Ammonia_world:
                        kValue = 232619;
                        break;
                    case EDPlanet.Sudarsky_class_I_gas_giant:
                        kValue = 3974;
                        break;
                    default:
                        kValue = 720;
                        if (terraformable) { kBonus = 223971; }
                        break;
                }

                double mass = massem.HasValue ? massem.Value : 1.0;       // some old entries don't have mass, so just presume 1

                int val = (int)PlanetValueED32(kValue, mass);
                if (terraformable || pl == EDPlanet.Earthlike_body)
                {
                    val += (int)PlanetValueED32(kBonus, mass);
                }

                return val;
            }
        }

        private double PlanetValueED32(double k, double m)
        {
            return k + (3 * k * Math.Pow(m, 0.199977) / 5.3);
        }

        #endregion

        #region ED 22

        private int EstimatedValueED22(bool isstar, EDStar st, bool isplanet, EDPlanet pl, bool terraformable, double? massstar, double? massem)
        {
            if (isstar)
            {
                switch (st)      // http://elite-dangerous.wikia.com/wiki/Explorer
                {
                    case EDStar.O:
                        //low = 3677;
                        //high = 4465;
                        return 4170;

                    case EDStar.B:
                        //low = 2992;
                        //high = 3456;
                        return 3098;

                    case EDStar.A:
                        //low = 2938;
                        //high = 2986;
                        return 2950;

                    case EDStar.F:
                        //low = 2915;
                        //high = 2957;
                        return 2932;

                    case EDStar.G:
                        //low = 2912;
                        //high = 2935;
                        // also have a G8V
                        return 2923;

                    case EDStar.K:
                        //low = 2898;
                        //high = 2923;
                        return 2911;
                    case EDStar.M:
                        //low = 2887;
                        //high = 2905;
                        return 2911;

                    // dwarfs
                    case EDStar.L:
                        //low = 2884;
                        //high = 2890;
                        return 2887;
                    case EDStar.T:
                        //low = 2881;
                        //high = 2885;
                        return 2883;
                    case EDStar.Y:
                        //low = 2880;
                        //high = 2882;
                        return 2881;

                    // proto stars
                    case EDStar.AeBe:    // Herbig
                                         //                ??
                                         //low = //high = 0;
                        return 2500;
                    case EDStar.TTS:
                        //low = 2881;
                        //high = 2922;
                        return 2900;

                    // wolf rayet
                    case EDStar.W:
                    case EDStar.WN:
                    case EDStar.WNC:
                    case EDStar.WC:
                    case EDStar.WO:
                        //low = //high = 7794;
                        return 7794;

                    // Carbon
                    case EDStar.CS:
                    case EDStar.C:
                    case EDStar.CN:
                    case EDStar.CJ:
                    case EDStar.CHd:
                        //low = //high = 2920;
                        return 2920;

                    case EDStar.MS: //seen in log
                    case EDStar.S:   // seen in log
                                     //                ??
                                     //low = //high = 0;
                        return 2000;


                    // white dwarf
                    case EDStar.D:
                    case EDStar.DA:
                    case EDStar.DAB:
                    case EDStar.DAO:
                    case EDStar.DAZ:
                    case EDStar.DAV:
                    case EDStar.DB:
                    case EDStar.DBZ:
                    case EDStar.DBV:
                    case EDStar.DO:
                    case EDStar.DOV:
                    case EDStar.DQ:
                    case EDStar.DC:
                    case EDStar.DCV:
                    case EDStar.DX:
                        //low = 25000;
                        //high = 27000;

                        return 26000;

                    case EDStar.N:
                        //low = 43276;
                        //high = 44619;
                        return 43441;

                    case EDStar.H:
                        //low = 44749;
                        //high = 80305;
                        return 61439;

                    case EDStar.X:
                    case EDStar.A_BlueWhiteSuperGiant:
                    case EDStar.F_WhiteSuperGiant:
                    case EDStar.M_RedSuperGiant:
                    case EDStar.M_RedGiant:
                    case EDStar.K_OrangeGiant:
                    case EDStar.RoguePlanet:

                    default:
                        //low = 0;
                        //high = 0;
                        return 2000;
                }
            }
            else   // Planet
            {
                switch (pl)      // http://elite-dangerous.wikia.com/wiki/Explorer
                {
                    case EDPlanet.Icy_body:
                        //low = 792; // (0.0001 EM)
                        //high = 1720; // 89.17
                        return 933; // 0.04

                    case EDPlanet.Rocky_ice_body:
                        //low = 792; // (0.0001 EM)
                        //high = 1720; // 89.17
                        return 933; // 0.04

                    case EDPlanet.Rocky_body:
                        if (terraformable)
                        {
                            //low = 36000;
                            //high = 36500;
                            return 37000;
                        }
                        else
                        {
                            //low = 792; // (0.0001 EM)
                            //high = 1720; // 89.17
                            return 933; // 0.04
                        }
                    case EDPlanet.Metal_rich_body:
                        //low = 9145; // (0.0002 EM)
                        //high = 14562; // (4.03 EM)
                        return 12449; // 0.51 EM
                    case EDPlanet.High_metal_content_body:
                        if (terraformable)
                        {
                            //low = 36000;
                            //high = 54000;
                            return 42000;
                        }
                        else
                        {
                            //low = 4966; // (0.0015 EM)
                            //high = 9632;  // 31.52 EM
                            return 6670; // 0.41
                        }

                    case EDPlanet.Earthlike_body:
                        //low = 65000; // 0.24 EM
                        //high = 71885; // 196.60 EM
                        return 67798; // 0.47 EM

                    case EDPlanet.Water_world:
                        //low = 26589; // (0.09 EM)
                        //high = 43437; // (42.77 EM)
                        return 30492; // (0.82 EM)
                    case EDPlanet.Ammonia_world:
                        //low = 37019; // 0.09 EM
                        //high = 71885; //(196.60 EM)
                        return 40322; // (0.41 EM)
                    case EDPlanet.Sudarsky_class_I_gas_giant:
                        //low = 2472; // (2.30 EM)
                        //high = 4514; // (620.81 EM
                        return 3400;  // 62.93 EM

                    case EDPlanet.Sudarsky_class_II_gas_giant:
                        //low = 8110; // (5.37 EM)
                        //high = 14618; // (949.98 EM)
                        return 12319;  // 260.84 EM

                    case EDPlanet.Sudarsky_class_III_gas_giant:
                        //low = 1368; // (10.16 EM)
                        //high = 2731; // (2926 EM)
                        return 2339; // 990.92 EM

                    case EDPlanet.Sudarsky_class_IV_gas_giant:
                        //low = 2739; //(2984 EM)
                        //high = 2827; // (3697 EM)
                        return 2782; // 3319 em

                    case EDPlanet.Sudarsky_class_V_gas_giant:
                        //low = 2225; // 688.2 EM
                        //high = 2225;
                        return 2225;

                    case EDPlanet.Water_giant:
                    case EDPlanet.Water_giant_with_life:
                    case EDPlanet.Gas_giant_with_water_based_life:
                    case EDPlanet.Gas_giant_with_ammonia_based_life:
                    case EDPlanet.Helium_rich_gas_giant:
                    case EDPlanet.Helium_gas_giant:
                        //low = 0;
                        //high = 0;
                        return 2000;

                    default:
                        //low = 0;
                        //high = 2000;
                        return 0;
                }
            }
        }

        #endregion

        public int EstimatedValue(bool? wasdiscovered, bool? wasmapped, bool mapped, bool efficientlymapped)
        {
            if (EstimatedValueFirstDiscovered > 0)      // for previous scans before 3.3 and stars, these are not set.
            {
                bool wasnotpreviousdiscovered = wasdiscovered.HasValue && wasdiscovered == false;
                bool wasnotpreviousmapped = wasmapped.HasValue && wasmapped == false;

                if ( wasnotpreviousdiscovered && wasmapped == true)       // this is the situation pointed out in PR#31, discovered is there and false, but mapped is true
                    return efficientlymapped ? EstimatedValueFirstMappedEfficiently : EstimatedValueFirstMapped;

                // if def not discovered (flag is there) and not mapped (flag is there), and we mapped it
                if (wasnotpreviousdiscovered && wasnotpreviousmapped && mapped)
                    return efficientlymapped ? EstimatedValueFirstDiscoveredFirstMappedEfficiently : EstimatedValueFirstDiscoveredFirstMapped;

                // if def not mapped, and we mapped it
                else if (wasnotpreviousmapped && mapped)
                    return efficientlymapped ?  EstimatedValueFirstMappedEfficiently : EstimatedValueFirstMapped;

                // if def not discovered
                else if (wasnotpreviousdiscovered)
                    return EstimatedValueFirstDiscovered;

                // if we mapped it, it was discovered/mapped before
                else if (mapped)
                    return efficientlymapped ? EstimatedValueMappedEfficiently : EstimatedValueMapped;
            }

            return EstimatedValueBase;
        }

        public int EstimatedValueBase { get; private set; }     // Estimated value without mapping or first discovery - all types, all versions
        public int EstimatedValueFirstDiscovered { get; private set; }     // Estimated value with first discovery  - 3.3 onwards for these for planets only
        public int EstimatedValueFirstDiscoveredFirstMapped { get; private set; }           // with both
        public int EstimatedValueFirstDiscoveredFirstMappedEfficiently { get; private set; }           // with both efficiently
        public int EstimatedValueFirstMapped { get; private set; }             // with just mapped
        public int EstimatedValueFirstMappedEfficiently { get; private set; }             // with just mapped
        public int EstimatedValueMapped { get; private set; }             // with just mapped
        public int EstimatedValueMappedEfficiently { get; private set; }             // with just mapped
    }
}
