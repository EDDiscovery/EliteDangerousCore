/*
 * Copyright 2016-2024 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        public class ShipProperties
        {
            public string FDID { get; set; }
            public string EDCDID { get; set; }
            public string Manufacturer { get; set; }
            public double HullMass { get; set; }
            public string Name { get; set; }
            public int HullCost { get; set; }
            public int Class { get; set; }
            public int Shields { get; set; }
            public int Armour { get; set; }
            public double Speed { get; set; }
            public int Boost { get; set; }
            public int BoostCost { get; set; }
            public double FuelCost { get; set; }
            public double FuelReserve { get; set; }
            public int Hardness { get; set; }
            public int Crew { get; set; }
            public double MinThrust { get; set; }
            public double HeatCap { get; set; }
            public double HeatDispMin { get; set; }
            public double HeatDispMax { get; set; }
            public double FwdAcc { get; set; }
            public double RevAcc { get; set; }
            public double LatAcc { get; set; }

            public string ClassString { get { return Class == 1 ? "Small" : Class == 2 ? "Medium" : "Large"; } }
        }
        static public bool IsTaxi(string shipfdname)       // If a taxi
        {
            return shipfdname.Contains("_taxi", StringComparison.InvariantCultureIgnoreCase);
        }

        static public bool IsShip(string shipfdname)      // any which are not one of the others is called a ship, to allow for new unknown ships
        {
            return shipfdname.HasChars() && !IsSRVOrFighter(shipfdname) && !IsSuit(shipfdname) && !IsTaxi(shipfdname) && !IsActor(shipfdname);
        }

        static public bool IsShipSRVOrFighter(string shipfdname)
        {
            return shipfdname.HasChars() && !IsSuit(shipfdname) && !IsTaxi(shipfdname);
        }

        static public bool IsSRV(string shipfdname)
        {
            return shipfdname.Equals("testbuggy", StringComparison.InvariantCultureIgnoreCase) || shipfdname.Contains("_SRV", StringComparison.InvariantCultureIgnoreCase);
        }

        static public bool IsFighter(string shipfdname)
        {
            return shipfdname.Contains("_fighter", StringComparison.InvariantCultureIgnoreCase);
        }

        static public bool IsSRVOrFighter(string shipfdname)
        {
            return IsSRV(shipfdname) || IsFighter(shipfdname);
        }



        // get properties of a ship, case insensitive, may be null
        static public ShipProperties GetShipProperties(string fdshipname)        
        {
            fdshipname = fdshipname.ToLowerInvariant();
            if (spaceships.ContainsKey(fdshipname))
                return spaceships[fdshipname];
            else if (srvandfighters.ContainsKey(fdshipname))
                return srvandfighters[fdshipname];
            else
                return null;
        }

        // get name of ship, or null
        static public string GetShipName(string fdshipname)
        {
            var sp = GetShipProperties(fdshipname);
            return sp?.Name;
        }

        // get normalised FDID of ship, or null
        static public string GetShipFDID(string fdshipname)
        {
            var sp = GetShipProperties(fdshipname);
            return sp?.FDID;
        }

        public static string ReverseShipLookup(string englishname)
        {
            englishname = englishname.Replace(" ", "");     // remove spaces to make things like Viper Mk III and MkIII match
            foreach (var kvp in spaceships)
            {
                var name = kvp.Value.Name.Replace(" ", "");
                if (englishname.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            foreach (var kvp in srvandfighters)
            {
                var name = kvp.Value.Name.Replace(" ", "");
                if (englishname.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Trace.WriteLine($"*** Reverse lookup shipname failed {englishname}");
            return null;
        }


        // get array of spaceships

        static public ShipProperties[] GetSpaceships()
        {
            var ships = spaceships.Values.ToArray();
            return ships;
        }

        #region ships

        private static void AddExtraShipInfo()
        {
            Dictionary<string, string> Manu = new Dictionary<string, string>        // add manu info, done this way ON PURPOSE
            {                                                                       // DO NOT BE TEMPTED TO CHANGE IT!
                ["Adder"] = "Zorgon Peterson",
                ["TypeX_3"] = "Lakon",
                ["TypeX"] = "Lakon",
                ["TypeX_2"] = "Lakon",
                ["Anaconda"] = "Faulcon DeLacy",
                ["Asp"] = "Lakon",
                ["Asp_Scout"] = "Lakon",
                ["BelugaLiner"] = "Saud Kruger",
                ["CobraMkIII"] = "Faulcon DeLacy",
                ["CobraMkIV"] = "Faulcon DeLacy",
                ["DiamondBackXL"] = "Lakon",
                ["DiamondBack"] = "Lakon",
                ["Dolphin"] = "Saud Kruger",
                ["Eagle"] = "Core Dynamics",
                ["Federation_Dropship_MkII"] = "Core Dynamics",
                ["Federation_Corvette"] = "Core Dynamics",
                ["Federation_Dropship"] = "Core Dynamics",
                ["Federation_Gunship"] = "Core Dynamics",
                ["FerDeLance"] = "Zorgon Peterson",
                ["Hauler"] = "Zorgon Peterson",
                ["Empire_Trader"] = "Gutamaya",
                ["Empire_Courier"] = "Gutamaya",
                ["Cutter"] = "Gutamaya",
                ["Empire_Eagle"] = "Gutamaya",
                ["Independant_Trader"] = "Lakon",
                ["Krait_MkII"] = "Faulcon DeLacy",
                ["Krait_Light"] = "Faulcon DeLacy",
                ["Mamba"] = "Zorgon Peterson",
                ["Orca"] = "Saud Kruger",
                ["Python"] = "Faulcon DeLacy",
                ["Python_NX"] = "Faulcon DeLacy",
                ["SideWinder"] = "Faulcon DeLacy",
                ["Type9_Military"] = "Lakon",
                ["Type6"] = "Lakon",
                ["Type7"] = "Lakon",
                ["Type9"] = "Lakon",
                ["Viper"] = "Faulcon DeLacy",
                ["Viper_MkIV"] = "Faulcon DeLacy",
                ["Vulture"] = "Core Dynamics",
            };

            foreach (var kvp in Manu)
            {
                spaceships[kvp.Key.ToLowerInvariant()].Manufacturer = kvp.Value;
            }

            // Add EDCD name overrides
            cobramkiii.EDCDID = "Cobra MkIII";
            cobramkiv.EDCDID = "Cobra MkIV";
            krait_mkii.EDCDID = "Krait MkII";
            viper.EDCDID = "Viper MkIII";
            viper_mkiv.EDCDID = "Viper MkIV";
        }


    
        private static ShipProperties sidewinder = new ShipProperties()
        {
            FDID = "SideWinder",
            EDCDID = "SideWinder",
            Manufacturer = "<code>",
            HullMass = 25F,
            Name = "Sidewinder",
            Speed = 220,
            Boost = 320,
            HullCost = 5070,
            Class = 1,
            Shields = 40,
            Armour = 60,
            MinThrust = 45.454,
            BoostCost = 7,
            FuelReserve = 0.3,
            HeatCap = 140,
            HeatDispMin = 1.18,
            HeatDispMax = 18.15,
            FuelCost = 50,
            Hardness = 20,
            Crew = 1,
            FwdAcc = 44.39,
            RevAcc = 29.96,
            LatAcc = 29.96
        };

        private static ShipProperties eagle = new ShipProperties()
        {
            FDID = "Eagle",
            EDCDID = "Eagle",
            Manufacturer = "<code>",
            HullMass = 50F,
            Name = "Eagle",
            Speed = 240,
            Boost = 350,
            HullCost = 7490,
            Class = 1,
            Shields = 60,
            Armour = 40,
            MinThrust = 75,
            BoostCost = 8,
            FuelReserve = 0.34,
            HeatCap = 165,
            HeatDispMin = 1.38,
            HeatDispMax = 21.48,
            FuelCost = 50,
            Hardness = 28,
            Crew = 1,
            FwdAcc = 43.97,
            RevAcc = 29.97,
            LatAcc = 29.86
        };

        private static ShipProperties hauler = new ShipProperties()
        {
            FDID = "Hauler",
            EDCDID = "Hauler",
            Manufacturer = "<code>",
            HullMass = 14F,
            Name = "Hauler",
            Speed = 200,
            Boost = 300,
            HullCost = 8160,
            Class = 1,
            Shields = 50,
            Armour = 100,
            MinThrust = 35,
            BoostCost = 7,
            FuelReserve = 0.25,
            HeatCap = 123,
            HeatDispMin = 1.06,
            HeatDispMax = 16.2,
            FuelCost = 50,
            Hardness = 20,
            Crew = 1,
            FwdAcc = 39.87,
            RevAcc = 29.95,
            LatAcc = 29.95
        };

        private static ShipProperties adder = new ShipProperties()
        {
            FDID = "Adder",
            EDCDID = "Adder",
            Manufacturer = "<code>",
            HullMass = 35F,
            Name = "Adder",
            Speed = 220,
            Boost = 320,
            HullCost = 18710,
            Class = 1,
            Shields = 60,
            Armour = 90,
            MinThrust = 45.454,
            BoostCost = 8,
            FuelReserve = 0.36,
            HeatCap = 170,
            HeatDispMin = 1.45,
            HeatDispMax = 22.6,
            FuelCost = 50,
            Hardness = 35,
            Crew = 2,
            FwdAcc = 39.41,
            RevAcc = 27.73,
            LatAcc = 27.86
        };

        private static ShipProperties empire_eagle = new ShipProperties()
        {
            FDID = "Empire_Eagle",
            EDCDID = "Empire_Eagle",
            Manufacturer = "<code>",
            HullMass = 50F,
            Name = "Imperial Eagle",
            Speed = 300,
            Boost = 400,
            HullCost = 50890,
            Class = 1,
            Shields = 80,
            Armour = 60,
            MinThrust = 70,
            BoostCost = 8,
            FuelReserve = 0.37,
            HeatCap = 163,
            HeatDispMin = 1.5,
            HeatDispMax = 21.2,
            FuelCost = 50,
            Hardness = 28,
            Crew = 1,
            FwdAcc = 34.54,
            RevAcc = 27.84,
            LatAcc = 27.84
        };

        private static ShipProperties viper = new ShipProperties()
        {
            FDID = "Viper",
            EDCDID = "Viper",
            Manufacturer = "<code>",
            HullMass = 50F,
            Name = "Viper Mk III",
            Speed = 320,
            Boost = 400,
            HullCost = 74610,
            Class = 1,
            Shields = 105,
            Armour = 70,
            MinThrust = 62.5,
            BoostCost = 10,
            FuelReserve = 0.41,
            HeatCap = 195,
            HeatDispMin = 1.69,
            HeatDispMax = 26.2,
            FuelCost = 50,
            Hardness = 35,
            Crew = 1,
            FwdAcc = 53.98,
            RevAcc = 29.7,
            LatAcc = 24.95
        };

        private static ShipProperties cobramkiii = new ShipProperties()
        {
            FDID = "CobraMkIII",
            EDCDID = "CobraMkIII",
            Manufacturer = "<code>",
            HullMass = 180F,
            Name = "Cobra Mk III",
            Speed = 280,
            Boost = 400,
            HullCost = 186260,
            Class = 1,
            Shields = 80,
            Armour = 120,
            MinThrust = 50,
            BoostCost = 10,
            FuelReserve = 0.49,
            HeatCap = 225,
            HeatDispMin = 1.92,
            HeatDispMax = 30.63,
            FuelCost = 50,
            Hardness = 35,
            Crew = 2,
            FwdAcc = 35.03,
            RevAcc = 25.16,
            LatAcc = 20.02
        };

        private static ShipProperties viper_mkiv = new ShipProperties()
        {
            FDID = "Viper_MkIV",
            EDCDID = "Viper_MkIV",
            Manufacturer = "<code>",
            HullMass = 190F,
            Name = "Viper Mk IV",
            Speed = 270,
            Boost = 340,
            HullCost = 290680,
            Class = 1,
            Shields = 150,
            Armour = 150,
            MinThrust = 64.815,
            BoostCost = 10,
            FuelReserve = 0.46,
            HeatCap = 209,
            HeatDispMin = 1.82,
            HeatDispMax = 28.98,
            FuelCost = 50,
            Hardness = 35,
            Crew = 1,
            FwdAcc = 53.84,
            RevAcc = 30.14,
            LatAcc = 24.97
        };

        private static ShipProperties diamondback = new ShipProperties()
        {
            FDID = "DiamondBack",
            EDCDID = "DiamondBack",
            Manufacturer = "<code>",
            HullMass = 170F,
            Name = "Diamondback Scout",
            Speed = 280,
            Boost = 380,
            HullCost = 441800,
            Class = 1,
            Shields = 120,
            Armour = 120,
            MinThrust = 60.714,
            BoostCost = 10,
            FuelReserve = 0.49,
            HeatCap = 346,
            HeatDispMin = 2.42,
            HeatDispMax = 48.05,
            FuelCost = 50,
            Hardness = 40,
            Crew = 1,
            FwdAcc = 39.57,
            RevAcc = 29.82,
            LatAcc = 25.19
        };

        private static ShipProperties cobramkiv = new ShipProperties()
        {
            FDID = "CobraMkIV",
            EDCDID = "CobraMkIV",
            Manufacturer = "<code>",
            HullMass = 210F,
            Name = "Cobra Mk IV",
            Speed = 200,
            Boost = 300,
            HullCost = 584200,
            Class = 1,
            Shields = 120,
            Armour = 120,
            MinThrust = 50,
            BoostCost = 10,
            FuelReserve = 0.51,
            HeatCap = 228,
            HeatDispMin = 1.99,
            HeatDispMax = 31.68,
            FuelCost = 50,
            Hardness = 35,
            Crew = 2,
            FwdAcc = 27.84,
            RevAcc = 19.91,
            LatAcc = 15.03
        };

        private static ShipProperties type6 = new ShipProperties()
        {
            FDID = "Type6",
            EDCDID = "Type6",
            Manufacturer = "<code>",
            HullMass = 155F,
            Name = "Type-6 Transporter",
            Speed = 220,
            Boost = 350,
            HullCost = 858010,
            Class = 2,
            Shields = 90,
            Armour = 180,
            MinThrust = 40.909,
            BoostCost = 10,
            FuelReserve = 0.39,
            HeatCap = 179,
            HeatDispMin = 1.7,
            HeatDispMax = 24.55,
            FuelCost = 50,
            Hardness = 35,
            Crew = 1,
            FwdAcc = 20.1,
            RevAcc = 14.96,
            LatAcc = 15.07
        };

        private static ShipProperties dolphin = new ShipProperties()
        {
            FDID = "Dolphin",
            EDCDID = "Dolphin",
            Manufacturer = "<code>",
            HullMass = 140F,
            Name = "Dolphin",
            Speed = 250,
            Boost = 350,
            HullCost = 1095780,
            Class = 1,
            Shields = 110,
            Armour = 110,
            MinThrust = 48,
            BoostCost = 10,
            FuelReserve = 0.5,
            HeatCap = 245,
            HeatDispMin = 1.91,
            HeatDispMax = 56,
            FuelCost = 50,
            Hardness = 35,
            Crew = 1,
            FwdAcc = 39.63,
            RevAcc = 30.01,
            LatAcc = 14.97
        };

        private static ShipProperties diamondbackxl = new ShipProperties()
        {
            FDID = "DiamondBackXL",
            EDCDID = "DiamondBackXL",
            Manufacturer = "<code>",
            HullMass = 260F,
            Name = "Diamondback Explorer",
            Speed = 260,
            Boost = 340,
            HullCost = 1616160,
            Class = 1,
            Shields = 150,
            Armour = 150,
            MinThrust = 61.538,
            BoostCost = 13,
            FuelReserve = 0.52,
            HeatCap = 351,
            HeatDispMin = 2.46,
            HeatDispMax = 50.55,
            FuelCost = 50,
            Hardness = 42,
            Crew = 1,
            FwdAcc = 34.63,
            RevAcc = 25.06,
            LatAcc = 19.89
        };

        private static ShipProperties empire_courier = new ShipProperties()
        {
            FDID = "Empire_Courier",
            EDCDID = "Empire_Courier",
            Manufacturer = "<code>",
            HullMass = 35F,
            Name = "Imperial Courier",
            Speed = 280,
            Boost = 380,
            HullCost = 2462010,
            Class = 1,
            Shields = 200,
            Armour = 80,
            MinThrust = 78.571,
            BoostCost = 10,
            FuelReserve = 0.41,
            HeatCap = 230,
            HeatDispMin = 1.62,
            HeatDispMax = 25.05,
            FuelCost = 50,
            Hardness = 30,
            Crew = 1,
            FwdAcc = 57.53,
            RevAcc = 30.02,
            LatAcc = 24.88
        };

        private static ShipProperties independant_trader = new ShipProperties()
        {
            FDID = "Independant_Trader",
            EDCDID = "Independant_Trader",
            Manufacturer = "<code>",
            HullMass = 180F,
            Name = "Keelback",
            Speed = 200,
            Boost = 300,
            HullCost = 2937840,
            Class = 2,
            Shields = 135,
            Armour = 270,
            MinThrust = 45,
            BoostCost = 10,
            FuelReserve = 0.39,
            HeatCap = 215,
            HeatDispMin = 1.87,
            HeatDispMax = 29.78,
            FuelCost = 50,
            Hardness = 45,
            Crew = 2,
            FwdAcc = 20.22,
            RevAcc = 15.07,
            LatAcc = 15.03
        };

        private static ShipProperties asp_scout = new ShipProperties()
        {
            FDID = "Asp_Scout",
            EDCDID = "Asp_Scout",
            Manufacturer = "<code>",
            HullMass = 150F,
            Name = "Asp Scout",
            Speed = 220,
            Boost = 300,
            HullCost = 3811220,
            Class = 2,
            Shields = 120,
            Armour = 180,
            MinThrust = 50,
            BoostCost = 13,
            FuelReserve = 0.47,
            HeatCap = 210,
            HeatDispMin = 1.8,
            HeatDispMax = 29.65,
            FuelCost = 50,
            Hardness = 52,
            Crew = 2,
            FwdAcc = 35.02,
            RevAcc = 20.1,
            LatAcc = 20.03
        };

        private static ShipProperties vulture = new ShipProperties()
        {
            FDID = "Vulture",
            EDCDID = "Vulture",
            Manufacturer = "<code>",
            HullMass = 230F,
            Name = "Vulture",
            Speed = 210,
            Boost = 340,
            HullCost = 4670100,
            Class = 1,
            Shields = 240,
            Armour = 160,
            MinThrust = 90.476,
            BoostCost = 16,
            FuelReserve = 0.57,
            HeatCap = 237,
            HeatDispMin = 1.87,
            HeatDispMax = 35.63,
            FuelCost = 50,
            Hardness = 55,
            Crew = 2,
            FwdAcc = 39.55,
            RevAcc = 29.88,
            LatAcc = 19.98
        };

        private static ShipProperties asp = new ShipProperties()
        {
            FDID = "Asp",
            EDCDID = "Asp",
            Manufacturer = "<code>",
            HullMass = 280F,
            Name = "Asp Explorer",
            Speed = 250,
            Boost = 340,
            HullCost = 6137180,
            Class = 2,
            Shields = 140,
            Armour = 210,
            MinThrust = 48,
            BoostCost = 13,
            FuelReserve = 0.63,
            HeatCap = 272,
            HeatDispMin = 2.34,
            HeatDispMax = 39.9,
            FuelCost = 50,
            Hardness = 52,
            Crew = 2,
            FwdAcc = 23.64,
            RevAcc = 15.04,
            LatAcc = 14.97
        };

        private static ShipProperties federation_dropship = new ShipProperties()
        {
            FDID = "Federation_Dropship",
            EDCDID = "Federation_Dropship",
            Manufacturer = "<code>",
            HullMass = 580F,
            Name = "Federal Dropship",
            Speed = 180,
            Boost = 300,
            HullCost = 13501480,
            Class = 2,
            Shields = 200,
            Armour = 300,
            MinThrust = 55.556,
            BoostCost = 19,
            FuelReserve = 0.83,
            HeatCap = 331,
            HeatDispMin = 2.6,
            HeatDispMax = 46.5,
            FuelCost = 50,
            Hardness = 60,
            Crew = 2,
            FwdAcc = 29.99,
            RevAcc = 20.34,
            LatAcc = 10.19
        };

        private static ShipProperties type7 = new ShipProperties()
        {
            FDID = "Type7",
            EDCDID = "Type7",
            Manufacturer = "<code>",
            HullMass = 350F,
            Name = "Type-7 Transporter",
            Speed = 180,
            Boost = 300,
            HullCost = 16774470,
            Class = 3,
            Shields = 156,
            Armour = 340,
            MinThrust = 33.333,
            BoostCost = 10,
            FuelReserve = 0.52,
            HeatCap = 226,
            HeatDispMin = 2.17,
            HeatDispMax = 32.45,
            FuelCost = 50,
            Hardness = 54,
            Crew = 1,
            FwdAcc = 20.11,
            RevAcc = 15.02,
            LatAcc = 15.13
        };

        private static ShipProperties typex = new ShipProperties()
        {
            FDID = "TypeX",
            EDCDID = "TypeX",
            Manufacturer = "<code>",
            HullMass = 400F,
            Name = "Alliance Chieftain",
            Speed = 230,
            Boost = 330,
            HullCost = 18603850,
            Class = 2,
            Shields = 200,
            Armour = 280,
            MinThrust = 65.217,
            BoostCost = 19,
            FuelReserve = 0.77,
            HeatCap = 289,
            HeatDispMin = 2.6,
            HeatDispMax = 46.5,
            FuelCost = 50,
            Hardness = 65,
            Crew = 2,
            FwdAcc = 37.84,
            RevAcc = 25.84,
            LatAcc = 20.01
        };

        private static ShipProperties federation_dropship_mkii = new ShipProperties()
        {
            FDID = "Federation_Dropship_MkII",
            EDCDID = "Federation_Dropship_MkII",
            Manufacturer = "<code>",
            HullMass = 480F,
            Name = "Federal Assault Ship",
            Speed = 210,
            Boost = 350,
            HullCost = 19102490,
            Class = 2,
            Shields = 200,
            Armour = 300,
            MinThrust = 71.429,
            BoostCost = 19,
            FuelReserve = 0.72,
            HeatCap = 286,
            HeatDispMin = 2.53,
            HeatDispMax = 45.23,
            FuelCost = 50,
            Hardness = 60,
            Crew = 2,
            FwdAcc = 39.81,
            RevAcc = 20.04,
            LatAcc = 15.07
        };

        private static ShipProperties empire_trader = new ShipProperties()
        {
            FDID = "Empire_Trader",
            EDCDID = "Empire_Trader",
            Manufacturer = "<code>",
            HullMass = 400F,
            Name = "Imperial Clipper",
            Speed = 300,
            Boost = 380,
            HullCost = 21108270,
            Class = 3,
            Shields = 180,
            Armour = 270,
            MinThrust = 60,
            BoostCost = 19,
            FuelReserve = 0.74,
            HeatCap = 304,
            HeatDispMin = 2.63,
            HeatDispMax = 46.8,
            FuelCost = 50,
            Hardness = 60,
            Crew = 2,
            FwdAcc = 24.74,
            RevAcc = 20.05,
            LatAcc = 10.1
        };

        private static ShipProperties typex_2 = new ShipProperties()
        {
            FDID = "TypeX_2",
            EDCDID = "TypeX_2",
            Manufacturer = "<code>",
            HullMass = 500F,
            Name = "Alliance Crusader",
            Speed = 180,
            Boost = 300,
            HullCost = 22087940,
            Class = 2,
            Shields = 200,
            Armour = 300,
            MinThrust = 61.11,
            BoostCost = 19,
            FuelReserve = 0.77,
            HeatCap = 316,
            HeatDispMin = 2.53,
            HeatDispMax = 45.23,
            FuelCost = 50,
            Hardness = 65,
            Crew = 3,
            FwdAcc = 29.78,
            RevAcc = 24.78,
            LatAcc = 18.96
        };

        private static ShipProperties typex_3 = new ShipProperties()
        {
            FDID = "TypeX_3",
            EDCDID = "TypeX_3",
            Manufacturer = "<code>",
            HullMass = 450F,
            Name = "Alliance Challenger",
            Speed = 200,
            Boost = 310,
            HullCost = 29561170,
            Class = 2,
            Shields = 220,
            Armour = 300,
            MinThrust = 65,
            BoostCost = 19,
            FuelReserve = 0.77,
            HeatCap = 316,
            HeatDispMin = 2.87,
            HeatDispMax = 51.4,
            FuelCost = 50,
            Hardness = 65,
            Crew = 2,
            FwdAcc = 31.65,
            RevAcc = 25.94,
            LatAcc = 20.09
        };

        private static ShipProperties federation_gunship = new ShipProperties()
        {
            FDID = "Federation_Gunship",
            EDCDID = "Federation_Gunship",
            Manufacturer = "<code>",
            HullMass = 580F,
            Name = "Federal Gunship",
            Speed = 170,
            Boost = 280,
            HullCost = 34806280,
            Class = 2,
            Shields = 250,
            Armour = 350,
            MinThrust = 58.824,
            BoostCost = 23,
            FuelReserve = 0.82,
            HeatCap = 325,
            HeatDispMin = 2.87,
            HeatDispMax = 51.4,
            FuelCost = 50,
            Hardness = 60,
            Crew = 2,
            FwdAcc = 24.61,
            RevAcc = 17.83,
            LatAcc = 10.08
        };

        private static ShipProperties krait_light = new ShipProperties()
        {
            FDID = "Krait_Light",
            EDCDID = "Krait_Light",
            Manufacturer = "<code>",
            HullMass = 270F,
            Name = "Krait Phantom",
            Speed = 250,
            Boost = 350,
            HullCost = 35732880,
            Class = 2,
            Shields = 200,
            Armour = 180,
            MinThrust = 64,
            BoostCost = 13,
            FuelReserve = 0.63,
            HeatCap = 300,
            HeatDispMin = 2.68,
            HeatDispMax = 52.05,
            FuelCost = 50,
            Hardness = 60,
            Crew = 2,
            FwdAcc = -999,
            RevAcc = -999,
            LatAcc = -999
        };

        private static ShipProperties krait_mkii = new ShipProperties()
        {
            FDID = "Krait_MkII",
            EDCDID = "Krait_MkII",
            Manufacturer = "<code>",
            HullMass = 320F,
            Name = "Krait Mk II",
            Speed = 240,
            Boost = 330,
            HullCost = 44152080,
            Class = 2,
            Shields = 220,
            Armour = 220,
            MinThrust = 62.5,
            BoostCost = 13,
            FuelReserve = 0.63,
            HeatCap = 300,
            HeatDispMin = 2.68,
            HeatDispMax = 52.05,
            FuelCost = 50,
            Hardness = 55,
            Crew = 3,
            FwdAcc = 28.01,
            RevAcc = 18.04,
            LatAcc = 15.12
        };

        private static ShipProperties orca = new ShipProperties()
        {
            FDID = "Orca",
            EDCDID = "Orca",
            Manufacturer = "<code>",
            HullMass = 290F,
            Name = "Orca",
            Speed = 300,
            Boost = 380,
            HullCost = 47792090,
            Class = 3,
            Shields = 220,
            Armour = 220,
            MinThrust = 66.667,
            BoostCost = 16,
            FuelReserve = 0.79,
            HeatCap = 262,
            HeatDispMin = 2.3,
            HeatDispMax = 42.68,
            FuelCost = 50,
            Hardness = 55,
            Crew = 2,
            FwdAcc = 29.66,
            RevAcc = 25.08,
            LatAcc = 19.95
        };

        private static ShipProperties ferdelance = new ShipProperties()
        {
            FDID = "FerDeLance",
            EDCDID = "FerDeLance",
            Manufacturer = "<code>",
            HullMass = 250F,
            Name = "Fer-de-Lance",
            Speed = 260,
            Boost = 350,
            HullCost = 51126980,
            Class = 2,
            Shields = 300,
            Armour = 225,
            MinThrust = 84.615,
            BoostCost = 19,
            FuelReserve = 0.67,
            HeatCap = 224,
            HeatDispMin = 2.05,
            HeatDispMax = 41.63,
            FuelCost = 50,
            Hardness = 70,
            Crew = 2,
            FwdAcc = 29.31,
            RevAcc = 24.34,
            LatAcc = 20.04
        };

        private static ShipProperties mamba = new ShipProperties()
        {
            FDID = "Mamba",
            EDCDID = "Mamba",
            Manufacturer = "<code>",
            HullMass = 250F,
            Name = "Mamba",
            Speed = 310,
            Boost = 380,
            HullCost = 55434290,
            Class = 2,
            Shields = 270,
            Armour = 230,
            MinThrust = 77.42,
            BoostCost = 16,
            FuelReserve = 0.5,
            HeatCap = 165,
            HeatDispMin = 2.05,
            HeatDispMax = 41.63,
            FuelCost = 50,
            Hardness = 70,
            Crew = 2,
            FwdAcc = -999,
            RevAcc = -999,
            LatAcc = -999
        };

        private static ShipProperties python = new ShipProperties()
        {
            FDID = "Python",
            EDCDID = "Python",
            Manufacturer = "<code>",
            HullMass = 350F,
            Name = "Python",
            Speed = 230,
            Boost = 300,
            HullCost = 55316050,
            Class = 2,
            Shields = 260,
            Armour = 260,
            MinThrust = 60.87,
            BoostCost = 23,
            FuelReserve = 0.83,
            HeatCap = 300,
            HeatDispMin = 2.68,
            HeatDispMax = 52.05,
            FuelCost = 50,
            Hardness = 65,
            Crew = 2,
            FwdAcc = 29.59,
            RevAcc = 18.02,
            LatAcc = 15.92
        };

        private static ShipProperties type9 = new ShipProperties()
        {
            FDID = "Type9",
            EDCDID = "Type9",
            Manufacturer = "<code>",
            HullMass = 850F,
            Name = "Type-9 Heavy",
            Speed = 130,
            Boost = 200,
            HullCost = 72108220,
            Class = 3,
            Shields = 240,
            Armour = 480,
            MinThrust = 30.769,
            BoostCost = 19,
            FuelReserve = 0.77,
            HeatCap = 289,
            HeatDispMin = 3.1,
            HeatDispMax = 48.35,
            FuelCost = 50,
            Hardness = 65,
            Crew = 3,
            FwdAcc = 20.03,
            RevAcc = 10.11,
            LatAcc = 10.03
        };

        private static ShipProperties belugaliner = new ShipProperties()
        {
            FDID = "BelugaLiner",
            EDCDID = "BelugaLiner",
            Manufacturer = "<code>",
            HullMass = 950F,
            Name = "Beluga Liner",
            Speed = 200,
            Boost = 280,
            HullCost = 79686090,
            Class = 3,
            Shields = 280,
            Armour = 280,
            MinThrust = 55,
            BoostCost = 19,
            FuelReserve = 0.81,
            HeatCap = 283,
            HeatDispMin = 2.6,
            HeatDispMax = 50.85,
            FuelCost = 50,
            Hardness = 60,
            Crew = 3,
            FwdAcc = 20.01,
            RevAcc = 17.12,
            LatAcc = 15.03
        };

        private static ShipProperties type9_military = new ShipProperties()
        {
            FDID = "Type9_Military",
            EDCDID = "Type9_Military",
            Manufacturer = "<code>",
            HullMass = 1200F,
            Name = "Type-10 Defender",
            Speed = 180,
            Boost = 220,
            HullCost = 121486140,
            Class = 3,
            Shields = 320,
            Armour = 580,
            MinThrust = 83.333,
            BoostCost = 19,
            FuelReserve = 0.77,
            HeatCap = 335,
            HeatDispMin = 3.16,
            HeatDispMax = 67.15,
            FuelCost = 50,
            Hardness = 75,
            Crew = 3,
            FwdAcc = 17.96,
            RevAcc = 10.04,
            LatAcc = 10.09
        };

        private static ShipProperties anaconda = new ShipProperties()
        {
            FDID = "Anaconda",
            EDCDID = "Anaconda",
            Manufacturer = "<code>",
            HullMass = 400F,
            Name = "Anaconda",
            Speed = 180,
            Boost = 240,
            HullCost = 142447820,
            Class = 3,
            Shields = 350,
            Armour = 525,
            MinThrust = 44.444,
            BoostCost = 27,
            FuelReserve = 1.07,
            HeatCap = 334,
            HeatDispMin = 3.16,
            HeatDispMax = 67.15,
            FuelCost = 50,
            Hardness = 65,
            Crew = 3,
            FwdAcc = 19.85,
            RevAcc = 10.03,
            LatAcc = 10.05
        };

        private static ShipProperties federation_corvette = new ShipProperties()
        {
            FDID = "Federation_Corvette",
            EDCDID = "Federation_Corvette",
            Manufacturer = "<code>",
            HullMass = 900F,
            Name = "Federal Corvette",
            Speed = 200,
            Boost = 260,
            HullCost = 183147460,
            Class = 3,
            Shields = 555,
            Armour = 370,
            MinThrust = 50,
            BoostCost = 27,
            FuelReserve = 1.13,
            HeatCap = 333,
            HeatDispMin = 3.28,
            HeatDispMax = 70.33,
            FuelCost = 50,
            Hardness = 70,
            Crew = 3,
            FwdAcc = 19.87,
            RevAcc = 10.08,
            LatAcc = 9.98
        };

        private static ShipProperties cutter = new ShipProperties()
        {
            FDID = "Cutter",
            EDCDID = "Cutter",
            Manufacturer = "<code>",
            HullMass = 1100F,
            Name = "Imperial Cutter",
            Speed = 200,
            Boost = 320,
            HullCost = 200484780,
            Class = 3,
            Shields = 600,
            Armour = 400,
            MinThrust = 80,
            BoostCost = 23,
            FuelReserve = 1.16,
            HeatCap = 327,
            HeatDispMin = 3.27,
            HeatDispMax = 72.58,
            FuelCost = 50,
            Hardness = 70,
            Crew = 3,
            FwdAcc = 29.37,
            RevAcc = 10.04,
            LatAcc = 6.06
        };

        private static ShipProperties python_nx = new ShipProperties()
        {
            FDID = "Python_NX",
            EDCDID = "Python_NX",
            Manufacturer = "<code>",
            HullMass = 450F,
            Name = "Python Mk II",
            Speed = 256,
            Boost = 345,
            HullCost = 66161981,
            Class = 2,
            Shields = 335,
            Armour = 280,
            MinThrust = 85.85,
            BoostCost = 20,
            FuelReserve = 0.83,
            HeatCap = 260,
            HeatDispMin = 2.68,
            HeatDispMax = 52.05,
            FuelCost = 50,
            Hardness = 70,
            Crew = 2,
            FwdAcc = -999,
            RevAcc = -999,
            LatAcc = -999
        };


        // MUST be after ship definitions else they are not constructed

        private static Dictionary<string, ShipProperties> spaceships = new Dictionary<string, ShipProperties>
        {
            { "adder",adder},
            { "typex_3",typex_3},
            { "typex",typex},
            { "typex_2",typex_2},
            { "anaconda",anaconda},
            { "asp",asp},
            { "asp_scout",asp_scout},
            { "belugaliner",belugaliner},
            { "cobramkiii",cobramkiii},
            { "cobramkiv",cobramkiv},
            { "diamondbackxl",diamondbackxl},
            { "diamondback",diamondback},
            { "dolphin",dolphin},
            { "eagle",eagle},
            { "federation_dropship_mkii", federation_dropship_mkii},
            { "federation_corvette",federation_corvette},
            { "federation_dropship",federation_dropship},
            { "federation_gunship",federation_gunship},
            { "ferdelance",ferdelance},
            { "hauler",hauler},
            { "empire_trader",empire_trader},
            { "empire_courier",empire_courier},
            { "cutter",cutter},
            { "empire_eagle",empire_eagle},
            { "independant_trader",independant_trader},
            { "krait_mkii",krait_mkii},
            { "krait_light",krait_light},
            { "mamba",mamba},
            { "orca",orca},
            { "python",python},
            { "python_nx",python_nx},
            { "sidewinder",sidewinder},
            { "type9_military",type9_military},
            { "type6",type6},
            { "type7",type7},
            { "type9",type9},
            { "viper",viper},
            { "viper_mkiv",viper_mkiv},
            { "vulture",vulture},
        };

        #endregion

        #region Not in Corolis Data

        private static ShipProperties imperial_fighter = new ShipProperties()
        {
            FDID = "Empire_Fighter",
            HullMass = 0F,
            Name = "Imperial Fighter",
            Manufacturer = "Gutamaya",
            Speed = 312,
            Boost = 540,
            HullCost = 0,
            Class = 1,
        };

        private static ShipProperties federation_fighter = new ShipProperties()
        {
            FDID = "Federation_Fighter",
            HullMass = 0F,
            Name = "F63 Condor",
            Manufacturer = "Core Dynamics",
            Speed = 316,
            Boost = 536,
            HullCost = 0,
            Class = 1,
        };


        private static ShipProperties taipan_fighter = new ShipProperties()
        {
            FDID = "Independent_Fighter",
            HullMass = 0F,
            Name = "Taipan",
            Manufacturer = "Faulcon DeLacy",
            Speed = 0,
            Boost = 0,
            HullCost = 0,
            Class = 1,
        };

        private static ShipProperties GDN_Hybrid_v1_fighter = new ShipProperties()
        {
            FDID = "GDN_Hybrid_Fighter_V1",
            HullMass = 0F,
            Name = "Guardian Hybrid Fighter V1",
            Manufacturer = "Unknown",
            Speed = 0,
            Boost = 0,
            HullCost = 0,
            Class = 1,
        };
        private static ShipProperties GDN_Hybrid_v2_fighter = new ShipProperties()
        {
            FDID = "GDN_Hybrid_Fighter_V2",
            HullMass = 0F,
            Name = "Guardian Hybrid Fighter V2",
            Manufacturer = "Unknown",
            Speed = 0,
            Boost = 0,
            HullCost = 0,
            Class = 1,
        };
        private static ShipProperties GDN_Hybrid_v3_fighter = new ShipProperties()
        {
            FDID = "GDN_Hybrid_Fighter_V3",
            HullMass = 0F,
            Name = "Guardian Hybrid Fighter V3",
            Manufacturer = "Unknown",
            Speed = 0,
            Boost = 0,
            HullCost = 0,
            Class = 1,
        };

        private static ShipProperties srv = new ShipProperties()
        {
            FDID = "TestBuggy",
            HullMass = 0F,
            Name = "Scarab SRV",
            Manufacturer = "Vodel",
            Speed = 38,
            Boost = 38,
            HullCost = 0,
            Class = 1,
        };

        private static ShipProperties combatsrv = new ShipProperties()
        {
            FDID = "Combat_Multicrew_SRV_01",
            HullMass = 0F,
            Name = "Scorpion Combat SRV",
            Manufacturer = "Vodel",
            Speed = 32,
            Boost = 32,
            HullCost = 0,
            Class = 1,
        };

        private static Dictionary<string, ShipProperties> srvandfighters = new Dictionary<string, ShipProperties>
        {
            { "empire_fighter",  imperial_fighter},
            { "federation_fighter",  federation_fighter},
            { "independent_fighter",  taipan_fighter},       //EDDI evidence
            { "testbuggy",  srv},
            { "combat_multicrew_srv_01",  combatsrv},
            { "gdn_hybrid_fighter_v1",  GDN_Hybrid_v1_fighter},
            { "gdn_hybrid_fighter_v2",  GDN_Hybrid_v2_fighter},
            { "gdn_hybrid_fighter_v3",  GDN_Hybrid_v3_fighter},
        };

        #endregion


    }
}
