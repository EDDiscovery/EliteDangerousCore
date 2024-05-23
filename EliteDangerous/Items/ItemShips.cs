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


        public class ShipProperties
        {
            public string FDID { get; set; }
            public string EDCDID { get; set; }
            public string Manufacturer { get; set; }
            public double HullMass { get; set; }
            public string Name { get; set; }
            public double Speed { get; set; }
            public int Boost { get; set; }
            public int HullCost { get; set; }
            public int Class { get; set; }
            public int Shields { get; set; }
            public int Armour { get; set; }
            public int BoostCost { get; set; }
            public double FuelReserve { get; set; }
            public int Hardness { get; set; }
            public int Crew { get; set; }

            public string ClassString { get { return Class == 1 ? "Small" : Class == 2 ? "Medium" : "Large"; } }
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
            BoostCost = 7,
            FuelReserve = 0.3,
            Hardness = 20,
            Crew = 1
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
            BoostCost = 8,
            FuelReserve = 0.34,
            Hardness = 28,
            Crew = 1
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
            BoostCost = 7,
            FuelReserve = 0.25,
            Hardness = 20,
            Crew = 1
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
            BoostCost = 8,
            FuelReserve = 0.36,
            Hardness = 35,
            Crew = 2
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
            BoostCost = 8,
            FuelReserve = 0.37,
            Hardness = 28,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.41,
            Hardness = 35,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.49,
            Hardness = 35,
            Crew = 2
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
            BoostCost = 10,
            FuelReserve = 0.46,
            Hardness = 35,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.49,
            Hardness = 40,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.51,
            Hardness = 35,
            Crew = 2
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
            BoostCost = 10,
            FuelReserve = 0.39,
            Hardness = 35,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.5,
            Hardness = 35,
            Crew = 1
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
            BoostCost = 13,
            FuelReserve = 0.52,
            Hardness = 42,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.41,
            Hardness = 30,
            Crew = 1
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
            BoostCost = 10,
            FuelReserve = 0.39,
            Hardness = 45,
            Crew = 2
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
            BoostCost = 13,
            FuelReserve = 0.47,
            Hardness = 52,
            Crew = 2
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
            BoostCost = 16,
            FuelReserve = 0.57,
            Hardness = 55,
            Crew = 2
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
            BoostCost = 13,
            FuelReserve = 0.63,
            Hardness = 52,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.83,
            Hardness = 60,
            Crew = 2
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
            BoostCost = 10,
            FuelReserve = 0.52,
            Hardness = 54,
            Crew = 1
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
            BoostCost = 19,
            FuelReserve = 0.77,
            Hardness = 65,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.72,
            Hardness = 60,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.74,
            Hardness = 60,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.77,
            Hardness = 65,
            Crew = 3
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
            BoostCost = 19,
            FuelReserve = 0.77,
            Hardness = 65,
            Crew = 2
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
            BoostCost = 23,
            FuelReserve = 0.82,
            Hardness = 60,
            Crew = 2
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
            BoostCost = 13,
            FuelReserve = 0.63,
            Hardness = 60,
            Crew = 2
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
            BoostCost = 13,
            FuelReserve = 0.63,
            Hardness = 55,
            Crew = 3
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
            BoostCost = 16,
            FuelReserve = 0.79,
            Hardness = 55,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.67,
            Hardness = 70,
            Crew = 2
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
            BoostCost = 16,
            FuelReserve = 0.5,
            Hardness = 70,
            Crew = 2
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
            BoostCost = 23,
            FuelReserve = 0.83,
            Hardness = 65,
            Crew = 2
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
            BoostCost = 19,
            FuelReserve = 0.77,
            Hardness = 65,
            Crew = 3
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
            BoostCost = 19,
            FuelReserve = 0.81,
            Hardness = 60,
            Crew = 3
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
            BoostCost = 19,
            FuelReserve = 0.77,
            Hardness = 75,
            Crew = 3
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
            BoostCost = 27,
            FuelReserve = 1.07,
            Hardness = 65,
            Crew = 3
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
            BoostCost = 27,
            FuelReserve = 1.13,
            Hardness = 70,
            Crew = 3
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
            BoostCost = 23,
            FuelReserve = 1.16,
            Hardness = 70,
            Crew = 3
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
            BoostCost = 20,
            FuelReserve = 0.83,
            Hardness = 70,
            Crew = 2
        };


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
