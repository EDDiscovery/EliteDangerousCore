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
            public long id { get; set; }             // json fields
            public string Name { get; set; }         // json field is fdevid, then its English Text name after normalisation
            public long BuyPrice { get; set; }       // json

            public string FDName { get; set; }      // FDName normalised

            public ItemData.ShipModule ModuleInfo { get; set; }      // Module data from item data, may be null if module not found

            public string EnglishModTypeString { get { return ModuleInfo?.EnglishModTypeString ?? "Unknown"; } }
            public string TranslatedModTypeString { get { return ModuleInfo?.TranslatedModTypeString ?? "Unknown"; } }
            public string TranslatedModuleName { get { return ModuleInfo?.TranslatedModName ?? Name; } }

            public void Normalise()
            {
                FDName = JournalFieldNaming.NormaliseFDItemName(Name);          // clean name and move to FDName
                ItemData.TryGetShipModule(FDName, out ItemData.ShipModule m, true);    // find, or create
                ModuleInfo = m;
                Name = ModuleInfo?.EnglishModName ?? FDName;            // set text name
            }

            public bool Equals(OutfittingItem other)
            {
                return (id == other.id && string.Compare(Name, other.Name) == 0 && string.Compare(FDName, other.FDName) == 0 &&
                         BuyPrice == other.BuyPrice);
            }

            public string ToStringShort()
            {
                long? buyprice = BuyPrice > 0 ? BuyPrice : default(long?);
                return BaseUtils.FieldBuilder.Build("", TranslatedModTypeString , "<: ", TranslatedModuleName, "< @ ", buyprice);
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

        public List<OutfittingItem> FindByFDName(string fdname)
        { return (from x in Items where x.FDName.Equals(fdname) select x).ToList(); }

        // all items with this translated module type name
        public List<OutfittingItem> FindByTranslatedModuleType(string translatedmoduletypename)
        { return (from x in Items where x.ModuleInfo!=null && x.ModuleInfo.TranslatedModTypeString.Equals(translatedmoduletypename) select x).ToList(); }


    }

    [System.Diagnostics.DebuggerDisplay("Yards {OutfittingYards.Count}")]
    public class OutfittingList
    {
        public List<Outfitting> OutfittingYards { get; private set; }

        public OutfittingList()
        {
            OutfittingYards = new List<Outfitting>();
        }

        // return, latest first, a list of outfitting from all years, latest yards first
        public List<Outfitting> GetFilteredList(bool nolocrepeats = false, int timeout = 60 * 5)        
        {
            Outfitting last = null;
            List<Outfitting> outfittings = new List<Outfitting>();

            foreach (var yard in OutfittingYards.AsEnumerable().Reverse())        // give it to me in lastest one first..
            {
                if (!nolocrepeats || outfittings.Find(x => x.Location.Equals(yard.Location)) == null) // allow yard repeats or not in list
                {
                    // if no last or different name or time is older..
                    if (last == null || !yard.Location.Equals(last.Location) || (last.Datetimeutc - yard.Datetimeutc).TotalSeconds >= timeout)
                    {
                        outfittings.Add(yard);
                        last = yard;
                        //System.Diagnostics.Debug.WriteLine("OF return " + yard.Ident(true) + " " + yard.Datetime.ToString());
                    }
                }
            }

            return outfittings;
        }

        // given a translated module type, return modules matching 
        public List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>> GetItemTypeLocationsFromYardsWithoutRepeat(string translatedmoduletype, bool nolocrepeats = false, int timeout = 60 * 5)       // without repeats note
        {
            List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>> list = new List<Tuple<Outfitting, List<Outfitting.OutfittingItem>>>();

            List<Outfitting> yardswithoutrepeats = GetFilteredList(nolocrepeats, timeout);

            foreach (Outfitting yard in yardswithoutrepeats)
            {
                List<Outfitting.OutfittingItem> i = yard.FindByTranslatedModuleType(translatedmoduletype);
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

