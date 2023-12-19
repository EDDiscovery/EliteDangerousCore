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
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{Ident()} {Items.Length}")]
    public class Outfitting : IEquatable<Outfitting>
    {
        [System.Diagnostics.DebuggerDisplay("{Name} {BuyPrice}")]
        public class OutfittingItem: IEquatable<OutfittingItem>
        {
            public long id;             // json fields
            public string Name;         // Text name after normalisation
            public long BuyPrice;
            public string FDName;       // FDName normalised

            public ItemData.ShipModule ModuleInfo;      // Module data from item data, may be null if module not found
            public string ModType { get { return ModuleInfo?.ModTypeString ?? "Unknown"; } }

            public void Normalise()
            {
                FDName = JournalFieldNaming.NormaliseFDItemName(Name);          // clean name and move to FDName
                ItemData.TryGetShipModule(FDName, out ItemData.ShipModule m, true);    // find, or create
                ModuleInfo = m;
                Name = ModuleInfo?.ModName ?? FDName;            // set text name
            }

            public bool Equals(OutfittingItem other)
            {
                return (id == other.id && string.Compare(Name, other.Name) == 0 && string.Compare(FDName, other.FDName) == 0 &&
                         BuyPrice == other.BuyPrice);
            }

            public string ToStringShort()
            {
                long? buyprice = BuyPrice > 0 ? BuyPrice : default(long?);
                return BaseUtils.FieldBuilder.Build("", ModType , "<: ", Name, "< @ ", buyprice);
            }
        }

        public OutfittingItem[] Items { get; private set; }     // can be null if its retrospecively read
        public string StationName { get; private set; }
        public string StarSystem { get; private set; }
        public DateTime Datetimeutc { get; private set; }

        public Outfitting(string stationname, string starsystem, DateTime dt, OutfittingItem[] it = null)        // items can be null if no data captured at the point
        {
            StationName = stationname;
            StarSystem = starsystem;
            Datetimeutc = dt;
            Items = it;
            if (it != null)
            {
                foreach (Outfitting.OutfittingItem i in Items)
                    i.Normalise();
            }
        }

        public bool Equals(Outfitting other)
        {
            return string.Compare(StarSystem, other.StarSystem) == 0 && string.Compare(StationName, other.StationName) == 0 &&
                CollectionStaticHelpers.Equals(Items, other.Items);
        }

        public string Location { get { return StarSystem + ":" + StationName; } }

        public string Ident()
        {
            return StarSystem + ":" + StationName + " on " + EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(Datetimeutc).ToString();
        }

        public List<string> ItemList() { return (from x1 in Items select x1.Name).ToList(); }

        public OutfittingItem Find(string item) { return Array.Find(Items, x => x.Name.Equals(item)); }
        public List<OutfittingItem> FindType(string itemtype) { return (from x in Items where x.ModType.Equals(itemtype) select x).ToList(); }

    }

    [System.Diagnostics.DebuggerDisplay("Yards {OutfittingYards.Count}")]
    public class OutfittingList
    {
        public List<Outfitting> OutfittingYards { get; private set; }

        public OutfittingList()
        {
            OutfittingYards = new List<Outfitting>();
        }

        public List<string> OutfittingItemList()
        {
            List<string> items = new List<string>();
            foreach (Outfitting yard in OutfittingYards)
                items.AddRange(yard.ItemList());
            var dislist = (from x in items select x).Distinct().ToList();
            dislist.Sort();
            return dislist;
        }

        public List<Outfitting> GetFilteredList(bool nolocrepeats = false, int timeout = 60 * 5)           // latest first..
        {
            Outfitting last = null;
            List<Outfitting> yards = new List<Outfitting>();

            foreach (var yard in OutfittingYards.AsEnumerable().Reverse())        // give it to me in lastest one first..
            {
                if (!nolocrepeats || yards.Find(x => x.Location.Equals(yard.Location)) == null) // allow yard repeats or not in list
                {
                    // if no last or different name or time is older..
                    if (last == null || !yard.Location.Equals(last.Location) || (last.Datetimeutc - yard.Datetimeutc).TotalSeconds >= timeout)
                    {
                        yards.Add(yard);
                        last = yard;
                        //System.Diagnostics.Debug.WriteLine("OF return " + yard.Ident(true) + " " + yard.Datetime.ToString());
                    }
                }
            }

            return yards;
        }

        public List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>> GetItemTypeLocationsFromYardsWithoutRepeat(string itemtype, bool nolocrepeats = false, int timeout = 60 * 5)       // without repeats note
        {
            List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>> list = new List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>>();

            List<Outfitting> yardswithoutrepeats = GetFilteredList(nolocrepeats, timeout);

            foreach (Outfitting yard in yardswithoutrepeats)
            {
                List<Outfitting.OutfittingItem> i = yard.FindType(itemtype);
                if ( i != null)
                    list.Add(new Tuple<Outfitting, List<Outfitting.OutfittingItem>>(yard, i));
            }

            return list;
        }

        public void Process(JournalEntry je)
        {
            if (je.EventTypeID == JournalTypeEnum.Outfitting)
            {
                JournalEvents.JournalOutfitting js = je as JournalEvents.JournalOutfitting;
                if (js.YardInfo.Items != null)     // just in case we get a bad Outfitting with no data or its one which was not caught by the EDD at the time
                {
                    OutfittingYards.Add(js.YardInfo);
                }
            }
        }
    }
}

