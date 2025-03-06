﻿/*
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
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{Ident(true)} {Ships.Length}")]
    public class ShipYard : IEquatable<ShipYard>
    {
        [System.Diagnostics.DebuggerDisplay("{ShipType_Localised} {ShipPrice}")]
        public class ShipyardItem : IEquatable<ShipyardItem>
        {           
            public long id { get; set; }                    // json from frontier shipyard.json
            public string ShipType { get; set; }            // json, frontier, FDName, direct from entries
            public string ShipType_Localised { get; set; }  // json, frontier
            public long ShipPrice { get; set; }             // json, frontier

            public string FriendlyShipType { get; set; }   // from our database

            public void Normalise()
            {
                ShipType = JournalFieldNaming.NormaliseFDShipName(ShipType);
                FriendlyShipType = JournalFieldNaming.GetBetterShipName(ShipType);
                ShipType_Localised = JournalFieldNaming.CheckLocalisation(ShipType_Localised,FriendlyShipType);
            }

            public bool Equals(ShipyardItem other)
            {
                return id == other.id && string.Compare(ShipType, other.ShipType) == 0 &&
                            string.Compare(ShipType_Localised, other.ShipType_Localised) == 0 && ShipPrice == other.ShipPrice;
            }

            public string ToStringShort()
            {
                long? buyprice = ShipPrice > 0 ? ShipPrice : default(long?);
                return BaseUtils.FieldBuilder.Build("", ShipType_Localised, "< @ ", buyprice);
            }
        }

        public string StationName { get; private set; }
        public string StarSystem { get; private set; }
        public DateTime DateTimeUTC { get; private set; }
        public ShipyardItem[] Ships { get; private set; }

        public ShipYard()
        {
        }

        public ShipYard(string st, string sy, DateTime utc , ShipyardItem[] it  )
        {
            StationName = st;
            StarSystem = sy;
            DateTimeUTC = utc;
            Ships = it;
            if ( Ships != null )
                foreach (ShipYard.ShipyardItem i in Ships)
                    i.Normalise();
        }

        public bool Equals(ShipYard other)
        {
            return string.Compare(StarSystem, other.StarSystem) == 0 && string.Compare(StationName, other.StationName) == 0 &&
                CollectionStaticHelpers.Equals(Ships,other.Ships);
        }

        public string Location { get { return StarSystem + ":" + StationName; } }

        public string Ident()
        {
            return StarSystem + ":" + StationName + " on " + EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(DateTimeUTC).ToString();
        }

        public List<string> ShipList() { return (from x1 in Ships select x1.ShipType_Localised).ToList(); }

        public ShipyardItem Find(string shiplocname) { return Array.Find(Ships, x => x.ShipType_Localised.Equals(shiplocname)); }

    }

    [System.Diagnostics.DebuggerDisplay("Yards {ShipYards.Count}")]
    public class ShipYardList
    {
        public bool? AllowCobraMkIV { get; private set; } = null;         // set when we get a shipyard

        public ShipYardList()
        {
            shipyards = new List<ShipYard>();
        }

        public ShipYard Get(string name)
        {
            return shipyards.Find(x => x.StationName.EqualsIIC(name));
        }

        // a list, sorted in date ascending order, with all repeats removed.
        public List<string> ShipListDistinctSorted()
        {
            List<string> ships = new List<string>();
            foreach (ShipYard yard in shipyards)
                ships.AddRange(yard.ShipList());
            var dislist = (from x in ships select x).Distinct().ToList();   // tbd? bet this is pointless
            dislist.Sort();
            return dislist;
        }

        // get a filtered list with optional no location repeats and no repeates withing timeout
        public List<ShipYard> GetFilteredList(bool nolocrepeats = false, int timeout = 60*5 )           // latest first..
        {
            ShipYard last = null;
            List<ShipYard> yards = new List<ShipYard>();

            foreach (var yard in shipyards.AsEnumerable().Reverse())        // give it to me in lastest one first..
            {
                if (!nolocrepeats || yards.Find(x => x.Location.Equals(yard.Location)) == null) // allow yard repeats or not in list
                {
                    // if no last or different name or time is older..
                    if (last == null || !yard.Location.Equals(last.Location) || (last.DateTimeUTC - yard.DateTimeUTC).TotalSeconds >= timeout)
                    {
                        yards.Add(yard);
                        last = yard;
                        //System.Diagnostics.Debug.WriteLine("return " + yard.Ident(true) + " " + yard.Datetime.ToString());
                    }
                }
            }

            return yards;
        }

        // gets ships of type ship without yard./.
        public List<Tuple<ShipYard, ShipYard.ShipyardItem>> GetShipLocations(string ship, bool nolocrepeats = false, int timeout = 60 * 5)      
        {
            List<Tuple<ShipYard, ShipYard.ShipyardItem>> list = new List<Tuple<ShipYard, ShipYard.ShipyardItem>>();
            List<ShipYard> yardswithoutrepeats = GetFilteredList(nolocrepeats, timeout);

            foreach ( ShipYard yard in yardswithoutrepeats)
            {
                ShipYard.ShipyardItem i = yard.Find(ship);
                if (i != null)
                    list.Add(new Tuple<ShipYard, ShipYard.ShipyardItem>(yard, i));
            }

            return list;
        }

        public void Process(JournalEntry je)
        {
            if ( je.EventTypeID == JournalTypeEnum.Shipyard)
            {
                JournalEvents.JournalShipyard js = je as JournalEvents.JournalShipyard;
                if (js.Yard.Ships != null)     // just in case we get a bad shipyard with no ship data or EDD did not see a matching shipyard.json vs the journal entry
                {
                    if ( js.IsJournalSourced)                            // if we have a sh
                        AllowCobraMkIV = js.AllowCobraMkIV;     // set the shipyard flag

                    //System.Diagnostics.Debug.WriteLine("Add yard data for " + js.Yard.StarSystem + ":" + js.Yard.StationName);
                    shipyards.Add(js.Yard);
                }
            }
        }

        private List<ShipYard> shipyards;
    }
}
