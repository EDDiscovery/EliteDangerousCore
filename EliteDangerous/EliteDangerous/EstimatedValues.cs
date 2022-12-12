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
 */

using System;

namespace EliteDangerousCore
{
    public class ScanEstimatedValues
    {
        public ScanEstimatedValues(DateTime utc, bool isstar , EDStar st, bool isplanet, EDPlanet pl, bool terraformable, double? massstar, double? massem, bool odyssey)
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

                double firstdiscovery = 2.6;

                double basevalue = StarValue32And33(kValue, massstar.HasValue ? massstar.Value : 1.0);

                EstimatedValueBase = (int)basevalue;
                EstimatedValueFirstDiscovered = (int)(basevalue * firstdiscovery);
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
                    double mapmultforfirstdiscoveredmapped = 3.699622554;
                    double mapmultforfirstmappedonly = 8.0956;
                    double mapmultforalreadymappeddiscovered = 3.3333333;

                    double basevalue = PlanetValue33(kValue, mass);

                    EstimatedValueBase = (int)basevalue;

                    EstimatedValueFirstDiscovered = (int)(basevalue * firstdiscovery);

                    EstimatedValueFirstDiscoveredFirstMapped = (int)(Ody(basevalue * mapmultforfirstdiscoveredmapped,odyssey) * firstdiscovery); 
                    EstimatedValueFirstDiscoveredFirstMappedEfficiently = (int)(Ody(basevalue * mapmultforfirstdiscoveredmapped,odyssey) * firstdiscovery * effmapped);  

                    EstimatedValueFirstMapped = (int)(Ody(basevalue * mapmultforfirstmappedonly,odyssey));                          
                    EstimatedValueFirstMappedEfficiently = (int)(Ody(basevalue * mapmultforfirstmappedonly, odyssey) * effmapped);

                    EstimatedValueMapped = (int)Ody(basevalue * mapmultforalreadymappeddiscovered, odyssey);     // already mapped/discovered
                    EstimatedValueMappedEfficiently = (int)(Ody(basevalue * mapmultforalreadymappeddiscovered, odyssey) * effmapped);
                }
            }
        }

        private double Ody(double v, bool odyssey)
        {
            return v + (odyssey ? Math.Max(v * 0.3, 555) : 0);
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

        public int EstimatedValue(bool? wasdiscovered, bool? wasmapped, bool mapped, bool efficientlymapped, bool isedsm)
        {
            if (isedsm)     // no value if its an edsm body - we have not scanned it.
                return 0;

            if (EstimatedValueFirstDiscovered > 0)      // for previous scans before 3.3 and stars, these are not set.
            {
                bool wasnotpreviousdiscovered = wasdiscovered.HasValue && wasdiscovered == false;
                bool wasnotpreviousmapped = wasmapped.HasValue && wasmapped == false;

                // the next two cope with the situation in PR#31, where we have a body with previousdiscovered = false but previouslymapped = true.
                if ( wasnotpreviousdiscovered && wasmapped == true && mapped == false)       // we did not map it
                    return EstimatedValueBase;

                if (wasnotpreviousdiscovered && wasmapped == true && mapped == true)       // we did map it
                    return efficientlymapped ? EstimatedValueMappedEfficiently : EstimatedValueMapped;

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

    public class OrganicEstimatedValues
    {
        public class Values
        {
            public string Name;
            public int Value;
            public string Codexname;
            public Values(string n, int v, string c) { Name = n; Value = v; Codexname = c; }
        }

        public static Values GetValue(string codexname)     // null if not found
        {
            return Array.Find(valuelist, x => codexname.StartsWith(x.Codexname));
        }

        private static Values[] valuelist = new Values[] {
            new Values("Aleoida Arcus",379300,"$Codex_Ent_Aleoids_01"),
            new Values("Aleoida Coronamus ",339100,"$Codex_Ent_Aleoids_02"),
            new Values("Aleoida Spica",208900,"$Codex_Ent_Aleoids_03"),
            new Values("Aleoida Laminiae",208900,"$Codex_Ent_Aleoids_04"),
            new Values("Aleoida Gravis",596500,"$Codex_Ent_Aleoids_05"),
            new Values("Bacterium Aurasus",78500,"$Codex_Ent_Bacterial_01"),
            new Values("Bacterium Nebulus",296300,"$Codex_Ent_Bacterial_02"),
            new Values("Bacterium Scopulum",280600,"$Codex_Ent_Bacterial_03"),
            new Values("Bacterium Acies",50000,"$Codex_Ent_Bacterial_04"),
            new Values("Bacterium Vesicula",56100,"$Codex_Ent_Bacterial_05"),
            new Values("Bacterium Alcyoneum",119500,"$Codex_Ent_Bacterial_06"),
            new Values("Bacterium Tela",135600,"$Codex_Ent_Bacterial_07"),
            new Values("Bacterium Informem",426200,"$Codex_Ent_Bacterial_08"),
            new Values("Bacterium Volu",400500,"$Codex_Ent_Bacterial_09"),
            new Values("Bacterium Bullaris",89900,"$Codex_Ent_Bacterial_10"),
            new Values("Bacterium Omentum",267400,"$Codex_Ent_Bacterial_11"),
            new Values("Bacterium Cerbrus",121300,"$Codex_Ent_Bacterial_12"),
            new Values("Bacterium Verrata",233300,"$Codex_Ent_Bacterial_13"),
            new Values("Cactoida Cortexum",222500,"$Codex_Ent_Cactoid_01"),
            new Values("Cactoida Lapis",164000,"$Codex_Ent_Cactoid_02"),
            new Values("Cactoida Vermis",711500,"$Codex_Ent_Cactoid_03"),
            new Values("Cactoida Pullulanta",222500,"$Codex_Ent_Cactoid_04"),
            new Values("Cactoida Peperatis",164000,"$Codex_Ent_Cactoid_05"),
            new Values("Clypeus Lacrimam",426200,"$Codex_Ent_Clypeus_01"),
            new Values("Clypeus Margaritus",557800,"$Codex_Ent_Clypeus_02"),
            new Values("Clypeus Speculumi",711500,"$Codex_Ent_Clypeus_03"),
            new Values("Concha Renibus",264300,"$Codex_Ent_Conchas_01"),
            new Values("Concha Aureolas",400500,"$Codex_Ent_Conchas_02"),
            new Values("Concha Labiata",157100,"$Codex_Ent_Conchas_03"),
            new Values("Concha Biconcavis",806300,"$Codex_Ent_Conchas_04"),
            new Values("Bark Mound",108900,"$Codex_Ent_Cone"),
            new Values("Electricae Pluma",339100,"$Codex_Ent_Electricae_01"),
            new Values("Electricae Radialem",339100,"$Codex_Ent_Electricae_02"),
            new Values("Fonticulua Segmentatus",806300,"$Codex_Ent_Fonticulus_01"),
            new Values("Fonticulua Campestris",63600,"$Codex_Ent_Fonticulus_02"),
            new Values("Fonticulua Upupam",315300,"$Codex_Ent_Fonticulus_03"),
            new Values("Fonticulua Lapida",195600,"$Codex_Ent_Fonticulus_04"),
            new Values("Fonticulua Fluctus",900000,"$Codex_Ent_Fonticulus_05"),
            new Values("Fonticulua Digitos",127700,"$Codex_Ent_Fonticulus_06"),
            new Values("Fumerola Carbosis",339100,"$Codex_Ent_Fumerolas_01"),
            new Values("Fumerola Extremus",711500,"$Codex_Ent_Fumerolas_02"),
            new Values("Fumerola Nitris",389400,"$Codex_Ent_Fumerolas_03"),
            new Values("Fumerola Aquatis",339100,"$Codex_Ent_Fumerolas_04"),
            new Values("Fungoida Setisis",120200,"$Codex_Ent_Fungoids_01"),
            new Values("Fungoida Stabitis",174000,"$Codex_Ent_Fungoids_02"),
            new Values("Fungoida Bullarum",224100,"$Codex_Ent_Fungoids_03"),
            new Values("Fungoida Gelata",206300,"$Codex_Ent_Fungoids_04"),
            new Values("Crystalline Shards",117900,"$Codex_Ent_Ground_Struct_Ice"),
            new Values("Osseus Fractus",239400,"$Codex_Ent_Osseus_01"),
            new Values("Osseus Discus",596500,"$Codex_Ent_Osseus_02"),
            new Values("Osseus Spiralis",159900,"$Codex_Ent_Osseus_03"),
            new Values("Osseus Pumice",197800,"$Codex_Ent_Osseus_04"),
            new Values("Osseus Cornibus",109500,"$Codex_Ent_Osseus_05"),
            new Values("Osseus Pellebantus",477700,"$Codex_Ent_Osseus_06"),
            new Values("Recepta Umbrux",596500,"$Codex_Ent_Recepta_01"),
            new Values("Recepta Deltahedronix",711500,"$Codex_Ent_Recepta_02"),
            new Values("Recepta Conditivus",645700,"$Codex_Ent_Recepta_03"),
            new Values("Roseum Brain Tree",115900,"$Codex_Ent_Seed"),
            new Values("Gypseeum Brain Tree",115900,"$Codex_Ent_SeedABCD_01"),
            new Values("Ostrinum Brain Tree",115900,"$Codex_Ent_SeedABCD_02"),
            new Values("Viride Brain Tree",115900,"$Codex_Ent_SeedABCD_03"),
            new Values("Aureum Brain Tree",115900,"$Codex_Ent_SeedEFGH_01"),
            new Values("Puniceum Brain Tree",115900,"$Codex_Ent_SeedEFGH_02"),
            new Values("Lindigoticum Brain Tree",115900,"$Codex_Ent_SeedEFGH_03"),
            new Values("Lividum Brain Tree",115900,"$Codex_Ent_SeedEFGH"),
            new Values("Frutexa Flabellum",127900,"$Codex_Ent_Shrubs_01"),
            new Values("Frutexa Acus",400500,"$Codex_Ent_Shrubs_02"),
            new Values("Frutexa Metallicum",118100,"$Codex_Ent_Shrubs_03"),
            new Values("Frutexa Flammasis",500100,"$Codex_Ent_Shrubs_04"),
            new Values("Frutexa Fera",118100,"$Codex_Ent_Shrubs_05"),
            new Values("Frutexa Sponsae",326500,"$Codex_Ent_Shrubs_06"),
            new Values("Frutexa Collum",118500,"$Codex_Ent_Shrubs_07"),
            new Values("Luteolum Anemone",110500,"$Codex_Ent_Sphere"),
            new Values("Croceum Anemone",110500,"$Codex_Ent_SphereABCD_01"),
            new Values("Puniceum Anemone",110500,"$Codex_Ent_SphereABCD_02"),
            new Values("Roseum Anemone",110500,"$Codex_Ent_SphereABCD_03"),
            new Values("Rubeum Bioluminescent Anemone",129900,"$Codex_Ent_SphereEFGH_01"),
            new Values("Prasinum Bioluminescent Anemone",110500,"$Codex_Ent_SphereEFGH_02"),
            new Values("Roseum Bioluminescent Anemone",110500,"$Codex_Ent_SphereEFGH_03"),
            new Values("Blatteum Bioluminescent Anemone",110500,"$Codex_Ent_SphereEFGH"),
            new Values("Stratum Excutitus",162200,"$Codex_Ent_Stratum_01"),
            new Values("Stratum Paleas",102500,"$Codex_Ent_Stratum_02"),
            new Values("Stratum Laminamus",179500,"$Codex_Ent_Stratum_03"),
            new Values("Stratum Araneamus",162200,"$Codex_Ent_Stratum_04"),
            new Values("Stratum Limaxus",102500,"$Codex_Ent_Stratum_05"),
            new Values("Stratum Cucumisis",711500,"$Codex_Ent_Stratum_06"),
            new Values("Stratum Tectonicas",806300,"$Codex_Ent_Stratum_07"),
            new Values("Stratum Frigus",171900,"$Codex_Ent_Stratum_08"),
            new Values("Roseum Sinuous Tubers",111300,"$Codex_Ent_Tube"),
            new Values("Prasinum Sinuous Tubers",111300,"$Codex_Ent_TubeABCD_01"),
            new Values("Albidum Sinuous Tubers",111300,"$Codex_Ent_TubeABCD_02"),
            new Values("Caeruleum Sinuous Tubers",111300,"$Codex_Ent_TubeABCD_03"),
            new Values("Lindigoticum Sinuous Tubers",111300,"$Codex_Ent_TubeEFGH_01"),
            new Values("Violaceum Sinuous Tubers",111300,"$Codex_Ent_TubeEFGH_02"),
            new Values("Viride Sinuous Tubers",111300,"$Codex_Ent_TubeEFGH_03"),
            new Values("Blatteum Sinuous Tubers",111300,"$Codex_Ent_TubeEFGH"),
            new Values("Tubus Conifer",315300,"$Codex_Ent_Tubus_01"),
            new Values("Tubus Sororibus",557800,"$Codex_Ent_Tubus_02"),
            new Values("Tubus Cavas",171900,"$Codex_Ent_Tubus_03"),
            new Values("Tubus Rosarium",400500,"$Codex_Ent_Tubus_04"),
            new Values("Tubus Compagibus",102700,"$Codex_Ent_Tubus_05"),
            new Values("Tussock Pennata",320700,"$Codex_Ent_Tussocks_01"),
            new Values("Tussock Ventusa",201300,"$Codex_Ent_Tussocks_02"),
            new Values("Tussock Ignis",130100,"$Codex_Ent_Tussocks_03"),
            new Values("Tussock Cultro",125600,"$Codex_Ent_Tussocks_04"),
            new Values("Tussock Catena",125600,"$Codex_Ent_Tussocks_05"),
            new Values("Tussock Pennatis",59600,"$Codex_Ent_Tussocks_06"),
            new Values("Tussock Serrati",258700,"$Codex_Ent_Tussocks_07"),
            new Values("Tussock Albata",202500,"$Codex_Ent_Tussocks_08"),
            new Values("Tussock Propagito",71300,"$Codex_Ent_Tussocks_09"),
            new Values("Tussock Divisa",125600,"$Codex_Ent_Tussocks_10"),
            new Values("Tussock Caputus",213100,"$Codex_Ent_Tussocks_11"),
            new Values("Tussock Triticum",400500,"$Codex_Ent_Tussocks_12"),
            new Values("Tussock Stigmasis",806300,"$Codex_Ent_Tussocks_13"),
            new Values("Tussock Virgam",645700,"$Codex_Ent_Tussocks_14"),
            new Values("Tussock Capillum",370000,"$Codex_Ent_Tussocks_15"),
            new Values("Amphora Plant",117900,"$Codex_Ent_Vents"),
        };
    }

        public class OrganicEstimatedValues414
        {
            public class Values
            {
                public string Name;
                public int Value;
                public string Codexname;
                public Values(string n, int v, string c) { Name = n; Value = v; Codexname = c; }
            }

            public static Values GetValue(string codexname)     // null if not found
            {
                return Array.Find(valuelist, x => codexname.StartsWith(x.Codexname));
            }

            private static Values[] valuelist = new Values[] {
                new Values("Aleoida Arcus", 7252500, "$Codex_Ent_Aleoids_01"),
                new Values("Aleoida Coronamus ", 6284600, "$Codex_Ent_Aleoids_02"),
                new Values("Aleoida Spica", 3385200, "$Codex_Ent_Aleoids_03"),
                new Values("Aleoida Laminiae", 3385200, "$Codex_Ent_Aleoids_04"),
                new Values("Aleoida Gravis", 12934900, "$Codex_Ent_Aleoids_05"),
                new Values("Bacterium Aurasus", 1000000, "$Codex_Ent_Bacterial_01"),
                new Values("Bacterium Nebulus", 9116600, "$Codex_Ent_Bacterial_02"),
                new Values("Bacterium Scopulum", 4934500, "$Codex_Ent_Bacterial_03"),
                new Values("Bacterium Acies", 1000000, "$Codex_Ent_Bacterial_04"),
                new Values("Bacterium Vesicula", 1000000, "$Codex_Ent_Bacterial_05"),
                new Values("Bacterium Alcyoneum", 1658500, "$Codex_Ent_Bacterial_06"),
                new Values("Bacterium Tela", 1949000, "$Codex_Ent_Bacterial_07"),
                new Values("Bacterium Informem", 8418000, "$Codex_Ent_Bacterial_08"),
                new Values("Bacterium Volu", 7774700, "$Codex_Ent_Bacterial_09"),
                new Values("Bacterium Bullaris", 1152500, "$Codex_Ent_Bacterial_10"),
                new Values("Bacterium Omentum", 4638900, "$Codex_Ent_Bacterial_11"),
                new Values("Bacterium Cerbrus", 1689800, "$Codex_Ent_Bacterial_12"),
                new Values("Bacterium Verrata", 3897000, "$Codex_Ent_Bacterial_13"),
                new Values("Cactoida Cortexum", 3667600, "$Codex_Ent_Cactoid_01"),
                new Values("Cactoida Lapis", 2483600, "$Codex_Ent_Cactoid_02"),
                new Values("Cactoida Vermis", 16202800, "$Codex_Ent_Cactoid_03"),
                new Values("Cactoida Pullulanta", 3667600, "$Codex_Ent_Cactoid_04"),
                new Values("Cactoida Peperatis", 2483600, "$Codex_Ent_Cactoid_05"),
                new Values("Clypeus Lacrimam", 8418000, "$Codex_Ent_Clypeus_01"),
                new Values("Clypeus Margaritus", 11873200, "$Codex_Ent_Clypeus_02"),
                new Values("Clypeus Speculumi", 16202800, "$Codex_Ent_Clypeus_03"),
                new Values("Concha Renibus", 4572400, "$Codex_Ent_Conchas_01"),
                new Values("Concha Aureolas", 7774700, "$Codex_Ent_Conchas_02"),
                new Values("Concha Labiata", 2352400, "$Codex_Ent_Conchas_03"),
                new Values("Concha Biconcavis", 16777215, "$Codex_Ent_Conchas_04"),
                new Values("Bark Mound", 1471900, "$Codex_Ent_Cone"),
                new Values("Electricae Pluma", 6284600, "$Codex_Ent_Electricae_01"),
                new Values("Electricae Radialem", 6284600, "$Codex_Ent_Electricae_02"),
                new Values("Fonticulua Segmentatus", 19010800, "$Codex_Ent_Fonticulus_01"),
                new Values("Fonticulua Campestris", 1000000, "$Codex_Ent_Fonticulus_02"),
                new Values("Fonticulua Upupam", 5727600, "$Codex_Ent_Fonticulus_03"),
                new Values("Fonticulua Lapida", 3111000, "$Codex_Ent_Fonticulus_04"),
                new Values("Fonticulua Fluctus", 20000000, "$Codex_Ent_Fonticulus_05"),
                new Values("Fonticulua Digitos", 1804100, "$Codex_Ent_Fonticulus_06"),
                new Values("Fumerola Carbosis", 6284600, "$Codex_Ent_Fumerolas_01"),
                new Values("Fumerola Extremus", 16202800, "$Codex_Ent_Fumerolas_02"),
                new Values("Fumerola Nitris", 7500900, "$Codex_Ent_Fumerolas_03"),
                new Values("Fumerola Aquatis", 6284600, "$Codex_Ent_Fumerolas_04"),
                new Values("Fungoida Setisis", 1670100, "$Codex_Ent_Fungoids_01"),
                new Values("Fungoida Stabitis", 2680300, "$Codex_Ent_Fungoids_02"),
                new Values("Fungoida Bullarum", 3703200, "$Codex_Ent_Fungoids_03"),
                new Values("Fungoida Gelata", 3330300, "$Codex_Ent_Fungoids_04"),
                new Values("Crystalline Shards", 1628800, "$Codex_Ent_Ground_Struct_Ice"),
                new Values("Osseus Fractus", 4027800, "$Codex_Ent_Osseus_01"),
                new Values("Osseus Discus", 12934900, "$Codex_Ent_Osseus_02"),
                new Values("Osseus Spiralis", 2404700, "$Codex_Ent_Osseus_03"),
                new Values("Osseus Pumice", 3156300, "$Codex_Ent_Osseus_04"),
                new Values("Osseus Cornibus", 1483000, "$Codex_Ent_Osseus_05"),
                new Values("Osseus Pellebantus", 9739000, "$Codex_Ent_Osseus_06"),
                new Values("Recepta Umbrux", 12934900, "$Codex_Ent_Recepta_01"),
                new Values("Recepta Deltahedronix", 16202800, "$Codex_Ent_Recepta_02"),
                new Values("Recepta Conditivus", 14313700, "$Codex_Ent_Recepta_03"),
                new Values("Roseum Brain Tree", 1593700, "$Codex_Ent_Seed"),
                new Values("Gypseeum Brain Tree", 1593700, "$Codex_Ent_SeedABCD_01"),
                new Values("Ostrinum Brain Tree", 1593700, "$Codex_Ent_SeedABCD_02"),
                new Values("Viride Brain Tree", 1593700, "$Codex_Ent_SeedABCD_03"),
                new Values("Aureum Brain Tree", 1593700, "$Codex_Ent_SeedEFGH_01"),
                new Values("Puniceum Brain Tree", 1593700, "$Codex_Ent_SeedEFGH_02"),
                new Values("Lindigoticum Brain Tree", 1593700, "$Codex_Ent_SeedEFGH_03"),
                new Values("Lividum Brain Tree", 1593700, "$Codex_Ent_SeedEFGH"),
                new Values("Frutexa Flabellum", 1808900, "$Codex_Ent_Shrubs_01"),
                new Values("Frutexa Acus", 7774700, "$Codex_Ent_Shrubs_02"),
                new Values("Frutexa Metallicum", 1632500, "$Codex_Ent_Shrubs_03"),
                new Values("Frutexa Flammasis", 10326000, "$Codex_Ent_Shrubs_04"),
                new Values("Frutexa Fera", 1632500, "$Codex_Ent_Shrubs_05"),
                new Values("Frutexa Sponsae", 5988000, "$Codex_Ent_Shrubs_06"),
                new Values("Frutexa Collum", 1639800, "$Codex_Ent_Shrubs_07"),
                new Values("Luteolum Anemone", 1499900, "$Codex_Ent_Sphere"),
                new Values("Croceum Anemone", 1499900, "$Codex_Ent_SphereABCD_01"),
                new Values("Puniceum Anemone", 1499900, "$Codex_Ent_SphereABCD_02"),
                new Values("Roseum Anemone", 1499900, "$Codex_Ent_SphereABCD_03"),
                new Values("Rubeum Bioluminescent Anemone", 1499900, "$Codex_Ent_SphereEFGH_01"),
                new Values("Prasinum Bioluminescent Anemone", 1499900, "$Codex_Ent_SphereEFGH_02"),
                new Values("Roseum Bioluminescent Anemone", 1499900, "$Codex_Ent_SphereEFGH_03"),
                new Values("Blatteum Bioluminescent Anemone", 1499900, "$Codex_Ent_SphereEFGH"),
                new Values("Stratum Excutitus", 2448900, "$Codex_Ent_Stratum_01"),
                new Values("Stratum Paleas", 1362000, "$Codex_Ent_Stratum_02"),
                new Values("Stratum Laminamus", 2788300, "$Codex_Ent_Stratum_03"),
                new Values("Stratum Araneamus", 2448900, "$Codex_Ent_Stratum_04"),
                new Values("Stratum Limaxus", 1362000, "$Codex_Ent_Stratum_05"),
                new Values("Stratum Cucumisis", 16202800, "$Codex_Ent_Stratum_06"),
                new Values("Stratum Tectonicas", 19010800, "$Codex_Ent_Stratum_07"),
                new Values("Stratum Frigus", 2637500, "$Codex_Ent_Stratum_08"),
                new Values("Roseum Sinuous Tubers", 111300, "$Codex_Ent_Tube"),
                new Values("Prasinum Sinuous Tubers", 1514500, "$Codex_Ent_TubeABCD_01"),
                new Values("Albidum Sinuous Tubers", 111300, "$Codex_Ent_TubeABCD_02"),
                new Values("Caeruleum Sinuous Tubers", 1514500, "$Codex_Ent_TubeABCD_03"),
                new Values("Lindigoticum Sinuous Tubers", 3425600, "$Codex_Ent_TubeEFGH_01"),
                new Values("Violaceum Sinuous Tubers", 3425600, "$Codex_Ent_TubeEFGH_02"),
                new Values("Viride Sinuous Tubers", 3425600, "$Codex_Ent_TubeEFGH_03"),
                new Values("Blatteum Sinuous Tubers", 3425600, "$Codex_Ent_TubeEFGH"),
                new Values("Tubus Conifer", 2415500, "$Codex_Ent_Tubus_01"),
                new Values("Tubus Sororibus", 5727600, "$Codex_Ent_Tubus_02"),
                new Values("Tubus Cavas", 11873200, "$Codex_Ent_Tubus_03"),
                new Values("Tubus Rosarium", 2637500, "$Codex_Ent_Tubus_04"),
                new Values("Tubus Compagibus", 7774700, "$Codex_Ent_Tubus_05"),
                new Values("Tussock Pennata", 5853800, "$Codex_Ent_Tussocks_01"),
                new Values("Tussock Ventusa", 3227700, "$Codex_Ent_Tussocks_02"),
                new Values("Tussock Ignis", 1849000, "$Codex_Ent_Tussocks_03"),
                new Values("Tussock Cultro", 1766600, "$Codex_Ent_Tussocks_04"),
                new Values("Tussock Catena", 1766600, "$Codex_Ent_Tussocks_05"),
                new Values("Tussock Pennatis", 1000000, "$Codex_Ent_Tussocks_06"),
                new Values("Tussock Serrati", 4447100, "$Codex_Ent_Tussocks_07"),
                new Values("Tussock Albata", 3252500, "$Codex_Ent_Tussocks_08"),
                new Values("Tussock Propagito", 1000000, "$Codex_Ent_Tussocks_09"),
                new Values("Tussock Divisa", 1766600, "$Codex_Ent_Tussocks_10"),
                new Values("Tussock Caputus", 3472400, "$Codex_Ent_Tussocks_11"),
                new Values("Tussock Triticum", 7774700, "$Codex_Ent_Tussocks_12"),
                new Values("Tussock Stigmasis", 19010800, "$Codex_Ent_Tussocks_13"),
                new Values("Tussock Virgam", 14313700, "$Codex_Ent_Tussocks_14"),
                new Values("Tussock Capillum", 7025800, "$Codex_Ent_Tussocks_15"),
                new Values("Amphora Plant", 3626400, "$Codex_Ent_Vents"),
            };
    


    }
}
