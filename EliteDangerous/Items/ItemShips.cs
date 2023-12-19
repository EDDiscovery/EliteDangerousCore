/*
 * Copyright © 2016-2023 EDDiscovery development team
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
 * Some data courtesy of Coriolis.IO https://github.com/EDCD/coriolis , data is intellectual property and copyright of Frontier Developments plc ('Frontier', 'Frontier Developments') and are subject to their terms and conditions.
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
            shipfdname = shipfdname.ToLowerInvariant();
            return shipfdname.Equals("federation_fighter") || shipfdname.Equals("empire_fighter") || shipfdname.Equals("independent_fighter") || shipfdname.Contains("hybrid_fighter");
        }

        static public bool IsSRVOrFighter(string shipfdname)
        {
            return IsSRV(shipfdname) || IsFighter(shipfdname);
        }


        public enum ShipPropID { FDID, HullMass, Name, Manu, Speed, Boost, HullCost, Class, EDCDName }

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

            System.Diagnostics.Debug.WriteLine($"*** Reverse lookup shipname failed {englishname}");
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

        public class ShipInfoString : IModuleInfo
        {
            public string Value;
            public ShipInfoString(string s) { Value = s; }
        };
        public class ShipInfoInt : IModuleInfo
        {
            public int Value;
            public ShipInfoInt(int i) { Value = i; }
        };
        public class ShipInfoDouble : IModuleInfo
        {
            public double Value;
            public ShipInfoDouble(double d) { Value = d; }
        };

        #region ship data FROM COROLIS - use the netlogentry scanner

        private static Dictionary<ShipPropID, IModuleInfo> adder = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Adder")},
            { ShipPropID.HullMass, new ShipInfoDouble(35F)},
            { ShipPropID.Name, new ShipInfoString("Adder")},
            { ShipPropID.Manu, new ShipInfoString("Zorgon Peterson")},
            { ShipPropID.Speed, new ShipInfoInt(220)},
            { ShipPropID.Boost, new ShipInfoInt(320)},
            { ShipPropID.HullCost, new ShipInfoInt(40000)},
            { ShipPropID.Class, new ShipInfoInt(1)},            // Class is the landing pad size
        };
        private static Dictionary<ShipPropID, IModuleInfo> alliance_challenger = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("TypeX_3")},
            { ShipPropID.HullMass, new ShipInfoDouble(450F)},
            { ShipPropID.Name, new ShipInfoString("Alliance Challenger")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(204)},
            { ShipPropID.Boost, new ShipInfoInt(310)},
            { ShipPropID.HullCost, new ShipInfoInt(28041035)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> alliance_chieftain = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("TypeX")},
            { ShipPropID.HullMass, new ShipInfoDouble(400F)},
            { ShipPropID.Name, new ShipInfoString("Alliance Chieftain")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(230)},
            { ShipPropID.Boost, new ShipInfoInt(330)},
            { ShipPropID.HullCost, new ShipInfoInt(18182883)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> alliance_crusader = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("TypeX_2")},
            { ShipPropID.HullMass, new ShipInfoDouble(500F)},
            { ShipPropID.Name, new ShipInfoString("Alliance Crusader")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(180)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(22866341)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> anaconda = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Anaconda")},
            { ShipPropID.HullMass, new ShipInfoDouble(400F)},
            { ShipPropID.Name, new ShipInfoString("Anaconda")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(180)},
            { ShipPropID.Boost, new ShipInfoInt(240)},
            { ShipPropID.HullCost, new ShipInfoInt(141889930)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> asp = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Asp")},
            { ShipPropID.HullMass, new ShipInfoDouble(280F)},
            { ShipPropID.Name, new ShipInfoString("Asp Explorer")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(250)},
            { ShipPropID.Boost, new ShipInfoInt(340)},
            { ShipPropID.HullCost, new ShipInfoInt(6135660)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> asp_scout = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Asp_Scout")},
            { ShipPropID.HullMass, new ShipInfoDouble(150F)},
            { ShipPropID.Name, new ShipInfoString("Asp Scout")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(220)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(3818240)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> beluga = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("BelugaLiner")},
            { ShipPropID.HullMass, new ShipInfoDouble(950F)},
            { ShipPropID.Name, new ShipInfoString("Beluga Liner")},
            { ShipPropID.Manu, new ShipInfoString("Saud Kruger")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(280)},
            { ShipPropID.HullCost, new ShipInfoInt(79654610)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> cobra_mk_iii = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("CobraMkIII")},
            { ShipPropID.HullMass, new ShipInfoDouble(180F)},
            { ShipPropID.Name, new ShipInfoString("Cobra Mk III")},
            { ShipPropID.EDCDName, new ShipInfoString("Cobra MkIII")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(280)},
            { ShipPropID.Boost, new ShipInfoInt(400)},
            { ShipPropID.HullCost, new ShipInfoInt(205800)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> cobra_mk_iv = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("CobraMkIV")},
            { ShipPropID.HullMass, new ShipInfoDouble(210F)},
            { ShipPropID.Name, new ShipInfoString("Cobra Mk IV")},
            { ShipPropID.EDCDName, new ShipInfoString("Cobra MkIV")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(603740)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> diamondback_explorer = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("DiamondBackXL")},
            { ShipPropID.HullMass, new ShipInfoDouble(260F)},
            { ShipPropID.Name, new ShipInfoString("Diamondback Explorer")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(260)},
            { ShipPropID.Boost, new ShipInfoInt(340)},
            { ShipPropID.HullCost, new ShipInfoInt(1635700)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> diamondback = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("DiamondBack")},
            { ShipPropID.HullMass, new ShipInfoDouble(170F)},
            { ShipPropID.Name, new ShipInfoString("Diamondback Scout")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(280)},
            { ShipPropID.Boost, new ShipInfoInt(380)},
            { ShipPropID.HullCost, new ShipInfoInt(461340)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> dolphin = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Dolphin")},
            { ShipPropID.HullMass, new ShipInfoDouble(140F)},
            { ShipPropID.Name, new ShipInfoString("Dolphin")},
            { ShipPropID.Manu, new ShipInfoString("Saud Kruger")},
            { ShipPropID.Speed, new ShipInfoInt(250)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(1115330)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> eagle = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Eagle")},
            { ShipPropID.HullMass, new ShipInfoDouble(50F)},
            { ShipPropID.Name, new ShipInfoString("Eagle")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(240)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(10440)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> federal_assault_ship = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Federation_Dropship_MkII")},
            { ShipPropID.HullMass, new ShipInfoDouble(480F)},
            { ShipPropID.Name, new ShipInfoString("Federal Assault Ship")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(210)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(19072000)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> federal_corvette = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Federation_Corvette")},
            { ShipPropID.HullMass, new ShipInfoDouble(900F)},
            { ShipPropID.Name, new ShipInfoString("Federal Corvette")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(260)},
            { ShipPropID.HullCost, new ShipInfoInt(182589570)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> federal_dropship = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Federation_Dropship")},
            { ShipPropID.HullMass, new ShipInfoDouble(580F)},
            { ShipPropID.Name, new ShipInfoString("Federal Dropship")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(180)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(13469990)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> federal_gunship = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Federation_Gunship")},
            { ShipPropID.HullMass, new ShipInfoDouble(580F)},
            { ShipPropID.Name, new ShipInfoString("Federal Gunship")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(170)},
            { ShipPropID.Boost, new ShipInfoInt(280)},
            { ShipPropID.HullCost, new ShipInfoInt(34774790)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> fer_de_lance = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("FerDeLance")},
            { ShipPropID.HullMass, new ShipInfoDouble(250F)},
            { ShipPropID.Name, new ShipInfoString("Fer-de-Lance")},
            { ShipPropID.Manu, new ShipInfoString("Zorgon Peterson")},
            { ShipPropID.Speed, new ShipInfoInt(260)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(51232230)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> hauler = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Hauler")},
            { ShipPropID.HullMass, new ShipInfoDouble(14F)},
            { ShipPropID.Name, new ShipInfoString("Hauler")},
            { ShipPropID.Manu, new ShipInfoString("Zorgon Peterson")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(29790)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> imperial_clipper = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Empire_Trader")},
            { ShipPropID.HullMass, new ShipInfoDouble(400F)},
            { ShipPropID.Name, new ShipInfoString("Imperial Clipper")},
            { ShipPropID.Manu, new ShipInfoString("Gutamaya")},
            { ShipPropID.Speed, new ShipInfoInt(300)},
            { ShipPropID.Boost, new ShipInfoInt(380)},
            { ShipPropID.HullCost, new ShipInfoInt(21077780)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> imperial_courier = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Empire_Courier")},
            { ShipPropID.HullMass, new ShipInfoDouble(35F)},
            { ShipPropID.Name, new ShipInfoString("Imperial Courier")},
            { ShipPropID.Manu, new ShipInfoString("Gutamaya")},
            { ShipPropID.Speed, new ShipInfoInt(280)},
            { ShipPropID.Boost, new ShipInfoInt(380)},
            { ShipPropID.HullCost, new ShipInfoInt(2481550)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> imperial_cutter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Cutter")},
            { ShipPropID.HullMass, new ShipInfoDouble(1100F)},
            { ShipPropID.Name, new ShipInfoString("Imperial Cutter")},
            { ShipPropID.Manu, new ShipInfoString("Gutamaya")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(320)},
            { ShipPropID.HullCost, new ShipInfoInt(199926890)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> imperial_eagle = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Empire_Eagle")},
            { ShipPropID.HullMass, new ShipInfoDouble(50F)},
            { ShipPropID.Name, new ShipInfoString("Imperial Eagle")},
            { ShipPropID.Manu, new ShipInfoString("Gutamaya")},
            { ShipPropID.Speed, new ShipInfoInt(300)},
            { ShipPropID.Boost, new ShipInfoInt(400)},
            { ShipPropID.HullCost, new ShipInfoInt(72180)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> keelback = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Independant_Trader")},
            { ShipPropID.HullMass, new ShipInfoDouble(180F)},
            { ShipPropID.Name, new ShipInfoString("Keelback")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(200)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(2943870)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> krait_mkii = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Krait_MkII")},
            { ShipPropID.HullMass, new ShipInfoDouble(320F)},
            { ShipPropID.Name, new ShipInfoString("Krait Mk II")},
            { ShipPropID.EDCDName, new ShipInfoString("Krait MkII")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(240)},
            { ShipPropID.Boost, new ShipInfoInt(330)},
            { ShipPropID.HullCost, new ShipInfoInt(42409425)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> krait_phantom = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Krait_Light")},
            { ShipPropID.HullMass, new ShipInfoDouble(270F)},
            { ShipPropID.Name, new ShipInfoString("Krait Phantom")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(250)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(42409425)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> mamba = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Mamba")},
            { ShipPropID.HullMass, new ShipInfoDouble(250F)},
            { ShipPropID.Name, new ShipInfoString("Mamba")},
            { ShipPropID.Manu, new ShipInfoString("Zorgon Peterson")},
            { ShipPropID.Speed, new ShipInfoInt(310)},
            { ShipPropID.Boost, new ShipInfoInt(380)},
            { ShipPropID.HullCost, new ShipInfoInt(55866341)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> orca = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Orca")},
            { ShipPropID.HullMass, new ShipInfoDouble(290F)},
            { ShipPropID.Name, new ShipInfoString("Orca")},
            { ShipPropID.Manu, new ShipInfoString("Saud Kruger")},
            { ShipPropID.Speed, new ShipInfoInt(300)},
            { ShipPropID.Boost, new ShipInfoInt(380)},
            { ShipPropID.HullCost, new ShipInfoInt(47790590)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> python = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Python")},
            { ShipPropID.HullMass, new ShipInfoDouble(350F)},
            { ShipPropID.Name, new ShipInfoString("Python")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(230)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(55171380)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> sidewinder = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("SideWinder")},
            { ShipPropID.HullMass, new ShipInfoDouble(25F)},
            { ShipPropID.Name, new ShipInfoString("Sidewinder")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(220)},
            { ShipPropID.Boost, new ShipInfoInt(320)},
            { ShipPropID.HullCost, new ShipInfoInt(4070)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> type_10_defender = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Type9_Military")},
            { ShipPropID.HullMass, new ShipInfoDouble(1200F)},
            { ShipPropID.Name, new ShipInfoString("Type-10 Defender")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(179)},
            { ShipPropID.Boost, new ShipInfoInt(219)},
            { ShipPropID.HullCost, new ShipInfoInt(121454173)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> type_6_transporter = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Type6")},
            { ShipPropID.HullMass, new ShipInfoDouble(155F)},
            { ShipPropID.Name, new ShipInfoString("Type-6 Transporter")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(220)},
            { ShipPropID.Boost, new ShipInfoInt(350)},
            { ShipPropID.HullCost, new ShipInfoInt(865790)},
            { ShipPropID.Class, new ShipInfoInt(2)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> type_7_transport = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Type7")},
            { ShipPropID.HullMass, new ShipInfoDouble(350F)},
            { ShipPropID.Name, new ShipInfoString("Type-7 Transporter")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(180)},
            { ShipPropID.Boost, new ShipInfoInt(300)},
            { ShipPropID.HullCost, new ShipInfoInt(16780510)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> type_9_heavy = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Type9")},
            { ShipPropID.HullMass, new ShipInfoDouble(850F)},
            { ShipPropID.Name, new ShipInfoString("Type-9 Heavy")},
            { ShipPropID.Manu, new ShipInfoString("Lakon")},
            { ShipPropID.Speed, new ShipInfoInt(130)},
            { ShipPropID.Boost, new ShipInfoInt(200)},
            { ShipPropID.HullCost, new ShipInfoInt(73255150)},
            { ShipPropID.Class, new ShipInfoInt(3)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> viper = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Viper")},
            { ShipPropID.HullMass, new ShipInfoDouble(50F)},
            { ShipPropID.Name, new ShipInfoString("Viper Mk III")},
            { ShipPropID.EDCDName, new ShipInfoString("Viper MkIII")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(320)},
            { ShipPropID.Boost, new ShipInfoInt(400)},
            { ShipPropID.HullCost, new ShipInfoInt(95900)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> viper_mk_iv = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Viper_MkIV")},
            { ShipPropID.HullMass, new ShipInfoDouble(190F)},
            { ShipPropID.Name, new ShipInfoString("Viper Mk IV")},
            { ShipPropID.EDCDName, new ShipInfoString("Viper MkIV")},
            { ShipPropID.Manu, new ShipInfoString("Faulcon DeLacy")},
            { ShipPropID.Speed, new ShipInfoInt(270)},
            { ShipPropID.Boost, new ShipInfoInt(340)},
            { ShipPropID.HullCost, new ShipInfoInt(310220)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };
        private static Dictionary<ShipPropID, IModuleInfo> vulture = new Dictionary<ShipPropID, IModuleInfo>
        {
            { ShipPropID.FDID, new ShipInfoString("Vulture")},
            { ShipPropID.HullMass, new ShipInfoDouble(230F)},
            { ShipPropID.Name, new ShipInfoString("Vulture")},
            { ShipPropID.Manu, new ShipInfoString("Core Dynamics")},
            { ShipPropID.Speed, new ShipInfoInt(210)},
            { ShipPropID.Boost, new ShipInfoInt(340)},
            { ShipPropID.HullCost, new ShipInfoInt(4689640)},
            { ShipPropID.Class, new ShipInfoInt(1)},
        };

        private static Dictionary<string, Dictionary<ShipPropID, IModuleInfo>> spaceships = new Dictionary<string, Dictionary<ShipPropID, IModuleInfo>>
        {
            { "adder",adder},
            { "typex_3",alliance_challenger},
            { "typex",alliance_chieftain},
            { "typex_2",alliance_crusader},
            { "anaconda",anaconda},
            { "asp",asp},
            { "asp_scout",asp_scout},
            { "belugaliner",beluga},
            { "cobramkiii",cobra_mk_iii},
            { "cobramkiv",cobra_mk_iv},
            { "diamondbackxl",diamondback_explorer},
            { "diamondback",diamondback},
            { "dolphin",dolphin},
            { "eagle",eagle},
            { "federation_dropship_mkii",federal_assault_ship},
            { "federation_corvette",federal_corvette},
            { "federation_dropship",federal_dropship},
            { "federation_gunship",federal_gunship},
            { "ferdelance",fer_de_lance},
            { "hauler",hauler},
            { "empire_trader",imperial_clipper},
            { "empire_courier",imperial_courier},
            { "cutter",imperial_cutter},
            { "empire_eagle",imperial_eagle},
            { "independant_trader",keelback},
            { "krait_mkii",krait_mkii},
            { "krait_light",krait_phantom},
            { "mamba",mamba},
            { "orca",orca},
            { "python",python},
            { "sidewinder",sidewinder},
            { "type9_military",type_10_defender},
            { "type6",type_6_transporter},
            { "type7",type_7_transport},
            { "type9",type_9_heavy},
            { "viper",viper},
            { "viper_mkiv",viper_mk_iv},
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
