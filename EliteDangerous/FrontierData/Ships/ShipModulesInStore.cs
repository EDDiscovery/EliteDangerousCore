/*
 * Copyright © 2016-2024 EDDiscovery development team
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
 */

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class ShipModulesInStore
    {
        public class StoredModule: IEquatable<StoredModule> // storage used by journal event..
        {
            public int StorageSlot{ get; set; }
            public string NameFD{ get; set; }
            public string Name{ get; set; }         // English name, keyed on this
            public string Name_Localised{ get; set; }
            public string StarSystem{ get; set; }       // not while in transit
            public long MarketID{ get; set; }       // not while in transit
            public long TransferCost{ get; set; }   // not while in transit
            public int TransferTime{ get; set; }    // not while in transit
            public string EngineerModifications{ get; set; }    // null if none present
            public double Quality{ get; set; }      // may not be there
            public int Level{ get; set; }           // may not be there
            public bool Hot{ get; set; }
            public bool InTransit{ get; set; }
            public int BuyPrice{ get; set; }

            public System.TimeSpan TransferTimeSpan{ get; set; }        // computed
            public string TransferTimeString{ get; set; } // computed, empty if not in tranfer (time<=0)

            public double Mass
            {
                get
                {
                    ItemData.TryGetShipModule(NameFD, out ItemData.ShipModule smd, false);    // find
                    return smd?.Mass ?? 0;
                }
            }

            public void Normalise()
            {
                NameFD = JournalFieldNaming.NormaliseFDItemName(Name);          // Name comes in with strange characters, normalise out
                Name = JournalFieldNaming.GetBetterEnglishModuleName(NameFD);      // and look up a better name
                Name_Localised = Name_Localised.Alt(Name);
                TransferTimeSpan = new System.TimeSpan((int)(TransferTime / 60 / 60), (int)((TransferTime / 60) % 60), (int)(TransferTime % 60));
                TransferTimeString = TransferTime > 0 ? TransferTimeSpan.ToString() : "";
                //System.Diagnostics.Debug.WriteLine($"SD Normalise '{NameFD}' '{Name}' '{Name_Localised}'");
            }

            public StoredModule(string fdname, string englishname, string item_localised, string system, string eng, int? level , double? quality, bool? hot)
            {
                NameFD = fdname;
                Name = englishname;
                Name_Localised = item_localised.Alt(Name);
                //System.Diagnostics.Debug.WriteLine($"SD Make '{NameFD}' '{Name}' '{Name_Localised}'");
                StarSystem = system;
                EngineerModifications = eng;
                if (level.HasValue)
                    Level = level.Value;
                if (quality.HasValue)
                    Quality = quality.Value;
                if (hot.HasValue)
                    Hot = hot.Value;
            }

            public StoredModule()
            {
            }

            public bool Equals(StoredModule other)
            {
                return (StorageSlot == other.StorageSlot && string.Compare(Name, other.Name) == 0 && string.Compare(Name_Localised, other.Name_Localised) == 0 &&
                         string.Compare(StarSystem, other.StarSystem) == 0 && MarketID == other.MarketID && TransferCost == other.TransferCost &&
                         TransferTime == other.TransferTime && string.Compare(EngineerModifications, other.EngineerModifications) == 0 &&
                         Quality == other.Quality && Level == other.Level && Hot == other.Hot && InTransit == other.InTransit && BuyPrice == other.BuyPrice);
            }
        }


        public List<StoredModule> StoredModules { get; private set; }     

        public ShipModulesInStore()
        {
            StoredModules = new List<StoredModule>();
        }

        public ShipModulesInStore(List<StoredModule> list)
        {
            StoredModules = new List<StoredModule>(list);
        }

        // ModuleBuy, ModuleBuyAndStore , ModuleRetrieve
        public ShipModulesInStore StoreModule(string fdname, string englishname, string namelocalised, ISystem sys)
        {
            ShipModulesInStore mis = this.ShallowClone();
            mis.StoredModules.Add(new StoredModule(fdname, englishname, namelocalised ,sys.Name, "", null, null, null));
            return mis;
        }

        // ModuleStore (has more info)
        public ShipModulesInStore StoreModule(JournalModuleStore e, ISystem sys)
        {
            ShipModulesInStore mis = this.ShallowClone();
            mis.StoredModules.Add(new StoredModule(e.StoredItemFD,e.StoredItem, e.StoredItemLocalised,sys.Name, e.EngineerModifications,e.Level,e.Quality,e.Hot));
            return mis;
        }

        // MassModuleStore
        public ShipModulesInStore StoreModule(JournalMassModuleStore.ModuleItem[] items, Dictionary<string, string> itemlocalisation, ISystem sys)
        {
            ShipModulesInStore mis = this.ShallowClone();
            foreach (var it in items)
            {
                string local = itemlocalisation.ContainsKey(it.Name) ? itemlocalisation[it.Name] : it.Name;
                mis.StoredModules.Add(new StoredModule(it.NameFD, it.Name, local, sys.Name, it.EngineerModifications, it.Level, it.Quality, it.Hot ));
            }
            return mis;
        }

        // ModuleRetrieve, ModuleSellRemote, remove on english name of module
        public ShipModulesInStore RemoveModuleUsingEnglishName(string englishname)
        {
            int index = StoredModules.FindIndex(x => x.Name.Equals(englishname, StringComparison.InvariantCultureIgnoreCase));  // if we have an item of this name
            if (index != -1)
            {
                //System.Diagnostics.Debug.WriteLine("Remove module '" + item + "'  '" + StoredModules[index].Name_Localised + "'");
                ShipModulesInStore mis = this.ShallowClone();
                mis.StoredModules.RemoveAt(index);
                return mis;
            }
            else
                return this;
        }

        // StoredModules
        public ShipModulesInStore UpdateStoredModules(StoredModule[] newlist)
        {
            ShipModulesInStore mis = new ShipModulesInStore(newlist.ToList());      // copy constructor ..
            return mis;
        }

        public ShipModulesInStore ShallowClone()          // shallow clone.. does not clone the ship modules, just the dictionary
        {
            ShipModulesInStore mis = new ShipModulesInStore(this.StoredModules);
            return mis;
        }
    }
}
