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


        public enum ShipPropID { FDID, HullMass, Name, Manu, Speed, Boost, HullCost, Class, EDCDName, Shields, Armour, BoostCost, FuelReserve, Hardness, Crew }

        // get properties of a ship, case insensitive, may be null
        static public Dictionary<ShipPropID, IModuleInfo> GetShipProperties(string fdshipname)        
        {
            fdshipname = fdshipname.ToLowerInvariant();
            if (spaceships.ContainsKey(fdshipname))
                return spaceships[fdshipname];
            else if (srvandfighters.ContainsKey(fdshipname))
                return srvandfighters[fdshipname];
            else
                return null;
        }

        public static string ReverseShipLookup(string englishname)
        {
            englishname = englishname.Replace(" ", "");     // remove spaces to make things like Viper Mk III and MkIII match
            foreach (var kvp in spaceships)
            {
                var name = ((ShipInfoString)kvp.Value[ShipPropID.Name]).Value.Replace(" ", "");
                if (englishname.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            foreach (var kvp in srvandfighters)
            {
                var name = ((ShipInfoString)kvp.Value[ShipPropID.Name]).Value.Replace(" ", "");
                if (englishname.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                    return kvp.Key;
            }

            System.Diagnostics.Trace.WriteLine($"*** Reverse lookup shipname failed {englishname}");
            return null;
        }


        // get array of spaceships

        static public Dictionary<ShipPropID, IModuleInfo>[] GetSpaceships()
        {
            var ships = spaceships.Values.ToArray();
            return ships;
        }
        
        // get property of a ship, case insensitive.  may be null
        static public IModuleInfo GetShipProperty(string fdshipname, ShipPropID property)        
        {
            Dictionary<ShipPropID, IModuleInfo> info = GetShipProperties(fdshipname);
            return info != null ? (info.ContainsKey(property) ? info[property] : null) : null;
        }

        // get property of a ship, case insensitive.
        // format/fp is used for ints/doubles and must be provided. Not used for string.
        // May be null
        static public string GetShipPropertyAsString(string fdshipname, ShipPropID property, string format, IFormatProvider fp)        
        {
            Dictionary<ShipPropID, IModuleInfo> info = GetShipProperties(fdshipname);
            if ( info != null && info.TryGetValue(property, out IModuleInfo i))
            {
                if (i is ShipInfoString)
                    return ((ShipInfoString)i).Value;
                else if (i is ShipInfoDouble)
                    return ((ShipInfoDouble)i).Value.ToString(format, fp);
                else if (i is ShipInfoInt)
                    return ((ShipInfoInt)i).Value.ToString(format, fp);
            }
            return null;
        }

        [System.Diagnostics.DebuggerDisplay("{Value}")]
        public class ShipInfoString : IModuleInfo
        {
            public string Value;
            public ShipInfoString(string s) { Value = s; }
        };
        [System.Diagnostics.DebuggerDisplay("{Value}")]
        public class ShipInfoInt : IModuleInfo
        {
            public int Value;
            public ShipInfoInt(int i) { Value = i; }
        };
        [System.Diagnostics.DebuggerDisplay("{Value}")]
        public class ShipInfoDouble : IModuleInfo
        {
            public double Value;
            public ShipInfoDouble(double d) { Value = d; }
        };

        #region ships

        private static void AddExtraShipInfo()
        {
            Dictionary<string, string> Manu = new Dictionary<string, string>        // add manu info
            {
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
                spaceships[kvp.Key.ToLowerInvariant()].Add(ShipPropID.Manu, new ShipInfoString(kvp.Value));
            }

            // Add EDCD name overrides
            cobramkiii.Add(ShipPropID.EDCDName, new ShipInfoString("Cobra MkIII"));
            cobramkiv.Add(ShipPropID.EDCDName, new ShipInfoString("Cobra MkIV"));
            krait_mkii.Add(ShipPropID.EDCDName, new ShipInfoString("Krait MkII"));
            viper.Add(ShipPropID.EDCDName, new ShipInfoString("Viper MkIII"));
            viper_mkiv.Add(ShipPropID.EDCDName, new ShipInfoString("Viper MkIV"));
        }

        private static Dictionary<ShipPropID, IModuleInfo> sidewinder = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("SideWinder") },
        { ShipPropID.HullMass, new ShipInfoDouble(25F)},
        { ShipPropID.Name, new ShipInfoString("Sidewinder")},
        { ShipPropID.Speed, new ShipInfoInt(220)},
        { ShipPropID.Boost, new ShipInfoInt(320)},
        { ShipPropID.HullCost, new ShipInfoInt(5070)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(40)},
        { ShipPropID.Armour, new ShipInfoInt(60)},
        { ShipPropID.BoostCost, new ShipInfoInt(7)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.3)},
        { ShipPropID.Hardness, new ShipInfoInt(20)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> eagle = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Eagle") },
        { ShipPropID.HullMass, new ShipInfoDouble(50F)},
        { ShipPropID.Name, new ShipInfoString("Eagle")},
        { ShipPropID.Speed, new ShipInfoInt(240)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(7490)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(60)},
        { ShipPropID.Armour, new ShipInfoInt(40)},
        { ShipPropID.BoostCost, new ShipInfoInt(8)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.34)},
        { ShipPropID.Hardness, new ShipInfoInt(28)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> hauler = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Hauler") },
        { ShipPropID.HullMass, new ShipInfoDouble(14F)},
        { ShipPropID.Name, new ShipInfoString("Hauler")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(8160)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(50)},
        { ShipPropID.Armour, new ShipInfoInt(100)},
        { ShipPropID.BoostCost, new ShipInfoInt(7)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.25)},
        { ShipPropID.Hardness, new ShipInfoInt(20)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> adder = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Adder") },
        { ShipPropID.HullMass, new ShipInfoDouble(35F)},
        { ShipPropID.Name, new ShipInfoString("Adder")},
        { ShipPropID.Speed, new ShipInfoInt(220)},
        { ShipPropID.Boost, new ShipInfoInt(320)},
        { ShipPropID.HullCost, new ShipInfoInt(18710)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(60)},
        { ShipPropID.Armour, new ShipInfoInt(90)},
        { ShipPropID.BoostCost, new ShipInfoInt(8)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.36)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> empire_eagle = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Empire_Eagle") },
        { ShipPropID.HullMass, new ShipInfoDouble(50F)},
        { ShipPropID.Name, new ShipInfoString("Imperial Eagle")},
        { ShipPropID.Speed, new ShipInfoInt(300)},
        { ShipPropID.Boost, new ShipInfoInt(400)},
        { ShipPropID.HullCost, new ShipInfoInt(50890)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(80)},
        { ShipPropID.Armour, new ShipInfoInt(60)},
        { ShipPropID.BoostCost, new ShipInfoInt(8)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.37)},
        { ShipPropID.Hardness, new ShipInfoInt(28)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> viper = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Viper") },
        { ShipPropID.HullMass, new ShipInfoDouble(50F)},
        { ShipPropID.Name, new ShipInfoString("Viper Mk III")},
        { ShipPropID.Speed, new ShipInfoInt(320)},
        { ShipPropID.Boost, new ShipInfoInt(400)},
        { ShipPropID.HullCost, new ShipInfoInt(74610)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(105)},
        { ShipPropID.Armour, new ShipInfoInt(70)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.41)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> cobramkiii = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("CobraMkIII") },
        { ShipPropID.HullMass, new ShipInfoDouble(180F)},
        { ShipPropID.Name, new ShipInfoString("Cobra Mk III")},
        { ShipPropID.Speed, new ShipInfoInt(280)},
        { ShipPropID.Boost, new ShipInfoInt(400)},
        { ShipPropID.HullCost, new ShipInfoInt(186260)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(80)},
        { ShipPropID.Armour, new ShipInfoInt(120)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.49)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> viper_mkiv = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Viper_MkIV") },
        { ShipPropID.HullMass, new ShipInfoDouble(190F)},
        { ShipPropID.Name, new ShipInfoString("Viper Mk IV")},
        { ShipPropID.Speed, new ShipInfoInt(270)},
        { ShipPropID.Boost, new ShipInfoInt(340)},
        { ShipPropID.HullCost, new ShipInfoInt(290680)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(150)},
        { ShipPropID.Armour, new ShipInfoInt(150)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.46)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> diamondback = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("DiamondBack") },
        { ShipPropID.HullMass, new ShipInfoDouble(170F)},
        { ShipPropID.Name, new ShipInfoString("Diamondback Scout")},
        { ShipPropID.Speed, new ShipInfoInt(280)},
        { ShipPropID.Boost, new ShipInfoInt(380)},
        { ShipPropID.HullCost, new ShipInfoInt(441800)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(120)},
        { ShipPropID.Armour, new ShipInfoInt(120)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.49)},
        { ShipPropID.Hardness, new ShipInfoInt(40)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> cobramkiv = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("CobraMkIV") },
        { ShipPropID.HullMass, new ShipInfoDouble(210F)},
        { ShipPropID.Name, new ShipInfoString("Cobra Mk IV")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(584200)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(120)},
        { ShipPropID.Armour, new ShipInfoInt(120)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.51)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> type6 = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Type6") },
        { ShipPropID.HullMass, new ShipInfoDouble(155F)},
        { ShipPropID.Name, new ShipInfoString("Type-6 Transporter")},
        { ShipPropID.Speed, new ShipInfoInt(220)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(858010)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(90)},
        { ShipPropID.Armour, new ShipInfoInt(180)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.39)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> dolphin = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Dolphin") },
        { ShipPropID.HullMass, new ShipInfoDouble(140F)},
        { ShipPropID.Name, new ShipInfoString("Dolphin")},
        { ShipPropID.Speed, new ShipInfoInt(250)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(1095780)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(110)},
        { ShipPropID.Armour, new ShipInfoInt(110)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.5)},
        { ShipPropID.Hardness, new ShipInfoInt(35)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> diamondbackxl = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("DiamondBackXL") },
        { ShipPropID.HullMass, new ShipInfoDouble(260F)},
        { ShipPropID.Name, new ShipInfoString("Diamondback Explorer")},
        { ShipPropID.Speed, new ShipInfoInt(260)},
        { ShipPropID.Boost, new ShipInfoInt(340)},
        { ShipPropID.HullCost, new ShipInfoInt(1616160)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(150)},
        { ShipPropID.Armour, new ShipInfoInt(150)},
        { ShipPropID.BoostCost, new ShipInfoInt(13)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.52)},
        { ShipPropID.Hardness, new ShipInfoInt(42)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> empire_courier = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Empire_Courier") },
        { ShipPropID.HullMass, new ShipInfoDouble(35F)},
        { ShipPropID.Name, new ShipInfoString("Imperial Courier")},
        { ShipPropID.Speed, new ShipInfoInt(280)},
        { ShipPropID.Boost, new ShipInfoInt(380)},
        { ShipPropID.HullCost, new ShipInfoInt(2462010)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(80)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.41)},
        { ShipPropID.Hardness, new ShipInfoInt(30)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> independant_trader = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Independant_Trader") },
        { ShipPropID.HullMass, new ShipInfoDouble(180F)},
        { ShipPropID.Name, new ShipInfoString("Keelback")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(2937840)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(135)},
        { ShipPropID.Armour, new ShipInfoInt(270)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.39)},
        { ShipPropID.Hardness, new ShipInfoInt(45)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> asp_scout = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Asp_Scout") },
        { ShipPropID.HullMass, new ShipInfoDouble(150F)},
        { ShipPropID.Name, new ShipInfoString("Asp Scout")},
        { ShipPropID.Speed, new ShipInfoInt(220)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(3811220)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(120)},
        { ShipPropID.Armour, new ShipInfoInt(180)},
        { ShipPropID.BoostCost, new ShipInfoInt(13)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.47)},
        { ShipPropID.Hardness, new ShipInfoInt(52)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> vulture = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Vulture") },
        { ShipPropID.HullMass, new ShipInfoDouble(230F)},
        { ShipPropID.Name, new ShipInfoString("Vulture")},
        { ShipPropID.Speed, new ShipInfoInt(210)},
        { ShipPropID.Boost, new ShipInfoInt(340)},
        { ShipPropID.HullCost, new ShipInfoInt(4670100)},
        { ShipPropID.Class, new ShipInfoInt(1)},
        { ShipPropID.Shields, new ShipInfoInt(240)},
        { ShipPropID.Armour, new ShipInfoInt(160)},
        { ShipPropID.BoostCost, new ShipInfoInt(16)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.57)},
        { ShipPropID.Hardness, new ShipInfoInt(55)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> asp = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Asp") },
        { ShipPropID.HullMass, new ShipInfoDouble(280F)},
        { ShipPropID.Name, new ShipInfoString("Asp Explorer")},
        { ShipPropID.Speed, new ShipInfoInt(250)},
        { ShipPropID.Boost, new ShipInfoInt(340)},
        { ShipPropID.HullCost, new ShipInfoInt(6137180)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(140)},
        { ShipPropID.Armour, new ShipInfoInt(210)},
        { ShipPropID.BoostCost, new ShipInfoInt(13)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.63)},
        { ShipPropID.Hardness, new ShipInfoInt(52)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> federation_dropship = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Federation_Dropship") },
        { ShipPropID.HullMass, new ShipInfoDouble(580F)},
        { ShipPropID.Name, new ShipInfoString("Federal Dropship")},
        { ShipPropID.Speed, new ShipInfoInt(180)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(13501480)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(300)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.83)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> type7 = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Type7") },
        { ShipPropID.HullMass, new ShipInfoDouble(350F)},
        { ShipPropID.Name, new ShipInfoString("Type-7 Transporter")},
        { ShipPropID.Speed, new ShipInfoInt(180)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(16774470)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(156)},
        { ShipPropID.Armour, new ShipInfoInt(340)},
        { ShipPropID.BoostCost, new ShipInfoInt(10)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.52)},
        { ShipPropID.Hardness, new ShipInfoInt(54)},
        { ShipPropID.Crew, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> typex = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("TypeX") },
        { ShipPropID.HullMass, new ShipInfoDouble(400F)},
        { ShipPropID.Name, new ShipInfoString("Alliance Chieftain")},
        { ShipPropID.Speed, new ShipInfoInt(230)},
        { ShipPropID.Boost, new ShipInfoInt(330)},
        { ShipPropID.HullCost, new ShipInfoInt(18603850)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(280)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.77)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> federation_dropship_mkii = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Federation_Dropship_MkII") },
        { ShipPropID.HullMass, new ShipInfoDouble(480F)},
        { ShipPropID.Name, new ShipInfoString("Federal Assault Ship")},
        { ShipPropID.Speed, new ShipInfoInt(210)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(19102490)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(300)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.72)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> empire_trader = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Empire_Trader") },
        { ShipPropID.HullMass, new ShipInfoDouble(400F)},
        { ShipPropID.Name, new ShipInfoString("Imperial Clipper")},
        { ShipPropID.Speed, new ShipInfoInt(300)},
        { ShipPropID.Boost, new ShipInfoInt(380)},
        { ShipPropID.HullCost, new ShipInfoInt(21108270)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(180)},
        { ShipPropID.Armour, new ShipInfoInt(270)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.74)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> typex_2 = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("TypeX_2") },
        { ShipPropID.HullMass, new ShipInfoDouble(500F)},
        { ShipPropID.Name, new ShipInfoString("Alliance Crusader")},
        { ShipPropID.Speed, new ShipInfoInt(180)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(22087940)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(300)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.77)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> typex_3 = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("TypeX_3") },
        { ShipPropID.HullMass, new ShipInfoDouble(450F)},
        { ShipPropID.Name, new ShipInfoString("Alliance Challenger")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(310)},
        { ShipPropID.HullCost, new ShipInfoInt(29561170)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(220)},
        { ShipPropID.Armour, new ShipInfoInt(300)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.77)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> federation_gunship = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Federation_Gunship") },
        { ShipPropID.HullMass, new ShipInfoDouble(580F)},
        { ShipPropID.Name, new ShipInfoString("Federal Gunship")},
        { ShipPropID.Speed, new ShipInfoInt(170)},
        { ShipPropID.Boost, new ShipInfoInt(280)},
        { ShipPropID.HullCost, new ShipInfoInt(34806280)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(250)},
        { ShipPropID.Armour, new ShipInfoInt(350)},
        { ShipPropID.BoostCost, new ShipInfoInt(23)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.82)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> krait_light = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Krait_Light") },
        { ShipPropID.HullMass, new ShipInfoDouble(270F)},
        { ShipPropID.Name, new ShipInfoString("Krait Phantom")},
        { ShipPropID.Speed, new ShipInfoInt(250)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(35732880)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(200)},
        { ShipPropID.Armour, new ShipInfoInt(180)},
        { ShipPropID.BoostCost, new ShipInfoInt(13)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.63)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> krait_mkii = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Krait_MkII") },
        { ShipPropID.HullMass, new ShipInfoDouble(320F)},
        { ShipPropID.Name, new ShipInfoString("Krait Mk II")},
        { ShipPropID.Speed, new ShipInfoInt(240)},
        { ShipPropID.Boost, new ShipInfoInt(330)},
        { ShipPropID.HullCost, new ShipInfoInt(44152080)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(220)},
        { ShipPropID.Armour, new ShipInfoInt(220)},
        { ShipPropID.BoostCost, new ShipInfoInt(13)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.63)},
        { ShipPropID.Hardness, new ShipInfoInt(55)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> orca = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Orca") },
        { ShipPropID.HullMass, new ShipInfoDouble(290F)},
        { ShipPropID.Name, new ShipInfoString("Orca")},
        { ShipPropID.Speed, new ShipInfoInt(300)},
        { ShipPropID.Boost, new ShipInfoInt(380)},
        { ShipPropID.HullCost, new ShipInfoInt(47792090)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(220)},
        { ShipPropID.Armour, new ShipInfoInt(220)},
        { ShipPropID.BoostCost, new ShipInfoInt(16)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.79)},
        { ShipPropID.Hardness, new ShipInfoInt(55)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> ferdelance = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("FerDeLance") },
        { ShipPropID.HullMass, new ShipInfoDouble(250F)},
        { ShipPropID.Name, new ShipInfoString("Fer-de-Lance")},
        { ShipPropID.Speed, new ShipInfoInt(260)},
        { ShipPropID.Boost, new ShipInfoInt(350)},
        { ShipPropID.HullCost, new ShipInfoInt(51126980)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(300)},
        { ShipPropID.Armour, new ShipInfoInt(225)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.67)},
        { ShipPropID.Hardness, new ShipInfoInt(70)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> mamba = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Mamba") },
        { ShipPropID.HullMass, new ShipInfoDouble(250F)},
        { ShipPropID.Name, new ShipInfoString("Mamba")},
        { ShipPropID.Speed, new ShipInfoInt(310)},
        { ShipPropID.Boost, new ShipInfoInt(380)},
        { ShipPropID.HullCost, new ShipInfoInt(55434290)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(270)},
        { ShipPropID.Armour, new ShipInfoInt(230)},
        { ShipPropID.BoostCost, new ShipInfoInt(16)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.5)},
        { ShipPropID.Hardness, new ShipInfoInt(70)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> python = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Python") },
        { ShipPropID.HullMass, new ShipInfoDouble(350F)},
        { ShipPropID.Name, new ShipInfoString("Python")},
        { ShipPropID.Speed, new ShipInfoInt(230)},
        { ShipPropID.Boost, new ShipInfoInt(300)},
        { ShipPropID.HullCost, new ShipInfoInt(55316050)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(260)},
        { ShipPropID.Armour, new ShipInfoInt(260)},
        { ShipPropID.BoostCost, new ShipInfoInt(23)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.83)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> type9 = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Type9") },
        { ShipPropID.HullMass, new ShipInfoDouble(850F)},
        { ShipPropID.Name, new ShipInfoString("Type-9 Heavy")},
        { ShipPropID.Speed, new ShipInfoInt(130)},
        { ShipPropID.Boost, new ShipInfoInt(200)},
        { ShipPropID.HullCost, new ShipInfoInt(72108220)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(240)},
        { ShipPropID.Armour, new ShipInfoInt(480)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.77)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> belugaliner = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("BelugaLiner") },
        { ShipPropID.HullMass, new ShipInfoDouble(950F)},
        { ShipPropID.Name, new ShipInfoString("Beluga Liner")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(280)},
        { ShipPropID.HullCost, new ShipInfoInt(79686090)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(280)},
        { ShipPropID.Armour, new ShipInfoInt(280)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.81)},
        { ShipPropID.Hardness, new ShipInfoInt(60)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> type9_military = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Type9_Military") },
        { ShipPropID.HullMass, new ShipInfoDouble(1200F)},
        { ShipPropID.Name, new ShipInfoString("Type-10 Defender")},
        { ShipPropID.Speed, new ShipInfoInt(180)},
        { ShipPropID.Boost, new ShipInfoInt(220)},
        { ShipPropID.HullCost, new ShipInfoInt(121486140)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(320)},
        { ShipPropID.Armour, new ShipInfoInt(580)},
        { ShipPropID.BoostCost, new ShipInfoInt(19)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.77)},
        { ShipPropID.Hardness, new ShipInfoInt(75)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> anaconda = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Anaconda") },
        { ShipPropID.HullMass, new ShipInfoDouble(400F)},
        { ShipPropID.Name, new ShipInfoString("Anaconda")},
        { ShipPropID.Speed, new ShipInfoInt(180)},
        { ShipPropID.Boost, new ShipInfoInt(240)},
        { ShipPropID.HullCost, new ShipInfoInt(142447820)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(350)},
        { ShipPropID.Armour, new ShipInfoInt(525)},
        { ShipPropID.BoostCost, new ShipInfoInt(27)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(1.07)},
        { ShipPropID.Hardness, new ShipInfoInt(65)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> federation_corvette = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Federation_Corvette") },
        { ShipPropID.HullMass, new ShipInfoDouble(900F)},
        { ShipPropID.Name, new ShipInfoString("Federal Corvette")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(260)},
        { ShipPropID.HullCost, new ShipInfoInt(183147460)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(555)},
        { ShipPropID.Armour, new ShipInfoInt(370)},
        { ShipPropID.BoostCost, new ShipInfoInt(27)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(1.13)},
        { ShipPropID.Hardness, new ShipInfoInt(70)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> cutter = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Cutter") },
        { ShipPropID.HullMass, new ShipInfoDouble(1100F)},
        { ShipPropID.Name, new ShipInfoString("Imperial Cutter")},
        { ShipPropID.Speed, new ShipInfoInt(200)},
        { ShipPropID.Boost, new ShipInfoInt(320)},
        { ShipPropID.HullCost, new ShipInfoInt(200484780)},
        { ShipPropID.Class, new ShipInfoInt(3)},
        { ShipPropID.Shields, new ShipInfoInt(600)},
        { ShipPropID.Armour, new ShipInfoInt(400)},
        { ShipPropID.BoostCost, new ShipInfoInt(23)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(1.16)},
        { ShipPropID.Hardness, new ShipInfoInt(70)},
        { ShipPropID.Crew, new ShipInfoInt(3)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> python_nx = new Dictionary<ShipPropID, IModuleInfo>
        {
        { ShipPropID.FDID, new ShipInfoString("Python_NX") },
        { ShipPropID.HullMass, new ShipInfoDouble(450F)},
        { ShipPropID.Name, new ShipInfoString("Python Mk II")},
        { ShipPropID.Speed, new ShipInfoInt(256)},
        { ShipPropID.Boost, new ShipInfoInt(345)},
        { ShipPropID.HullCost, new ShipInfoInt(66161981)},
        { ShipPropID.Class, new ShipInfoInt(2)},
        { ShipPropID.Shields, new ShipInfoInt(335)},
        { ShipPropID.Armour, new ShipInfoInt(280)},
        { ShipPropID.BoostCost, new ShipInfoInt(20)},
        { ShipPropID.FuelReserve, new ShipInfoDouble(0.83)},
        { ShipPropID.Hardness, new ShipInfoInt(70)},
        { ShipPropID.Crew, new ShipInfoInt(2)},
        };



        private static Dictionary<string, Dictionary<ShipPropID, IModuleInfo>> spaceships = new Dictionary<string, Dictionary<ShipPropID, IModuleInfo>>
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

        private static Dictionary<ShipPropID, IModuleInfo> imperial_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Empire_Fighter")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Imperial Fighter")},
            { ShipPropID.Manu, new ShipInfoString("Gutamaya")},
            { ShipPropID.Speed, new ShipInfoInt(312)},
            { ShipPropID.Boost, new ShipInfoInt(540)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> federation_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Federation_Fighter")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("F63 Condor")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(316)},
            { ShipPropID.Boost, new ShipInfoInt(536)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };


        private static Dictionary<ShipPropID, IModuleInfo> taipan_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Independent_Fighter")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Taipan")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(0)},
            { ShipPropID.Boost, new ShipInfoInt(0)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> GDN_Hybrid_v1_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("GDN_Hybrid_Fighter_V1")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Guardian Hybrid Fighter V1")},
            { ShipPropID.Manu, new ShipInfoString("Unknown")},
            { ShipPropID.Speed, new ShipInfoInt(0)},
            { ShipPropID.Boost, new ShipInfoInt(0)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> GDN_Hybrid_v2_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("GDN_Hybrid_Fighter_V2")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Guardian Hybrid Fighter V2")},
            { ShipPropID.Manu, new ShipInfoString("Unknown")},
            { ShipPropID.Speed, new ShipInfoInt(0)},
            { ShipPropID.Boost, new ShipInfoInt(0)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> GDN_Hybrid_v3_fighter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("GDN_Hybrid_Fighter_V3")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Guardian Hybrid Fighter V3")},
            { ShipPropID.Manu, new ShipInfoString("Unknown")},
            { ShipPropID.Speed, new ShipInfoInt(0)},
            { ShipPropID.Boost, new ShipInfoInt(0)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> srv = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("TestBuggy")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Scarab SRV")},
            { ShipPropID.Manu, new ShipInfoString("Vodel")},
            { ShipPropID.Speed, new ShipInfoInt(38)},
            { ShipPropID.Boost, new ShipInfoInt(38)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<ShipPropID, IModuleInfo> combatsrv = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Combat_Multicrew_SRV_01")},
            { ShipPropID.HullMass, new ShipInfoDouble(0F)},
            { ShipPropID.Name, new ShipInfoString("Scorpion Combat SRV")},
            { ShipPropID.Manu, new ShipInfoString("Vodel")},
            { ShipPropID.Speed, new ShipInfoInt(32)},
            { ShipPropID.Boost, new ShipInfoInt(32)},
            { ShipPropID.HullCost, new ShipInfoInt(0)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<string, Dictionary<ShipPropID, IModuleInfo>> srvandfighters = new Dictionary<string, Dictionary<ShipPropID, IModuleInfo>>
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
