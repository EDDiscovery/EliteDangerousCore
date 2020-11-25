/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("MatC {Details.Category} {Details.Name} {Details.FDName} count {Count}")]
    public class MaterialCommodities               // in memory version of it
    {
        public int Count { get; set; }
        public double Price { get; set; }
        public MaterialCommodityData Details { get; set; }

        public MaterialCommodities(MaterialCommodityData c)
        {
            Count = 0;
            Price = 0;
            this.Details = c;
        }

        public MaterialCommodities(MaterialCommodities c)
        {
            Count = c.Count;        // clone these
            Price = c.Price;
            this.Details = c.Details;       // can copy this, its fixed
        }
    }

    public class MaterialCommoditiesList
    {
        public MaterialCommoditiesList()
        {
            List = new List<MaterialCommodities>();
        }

        public MaterialCommoditiesList(MaterialCommoditiesList other)       // clone from other
        {
            List = new List<MaterialCommodities>();
            other.List.ForEach(item =>
            {
                if (item.Count > 0)        // if items, or commodity and not clear zero, or material and not clear zero, add
                    List.Add(item);
            });
        }

        public bool ContainsRares() // function on purpose
        {
            return List.FindIndex(x => x.Details.IsRareCommodity && x.Count > 0) != -1;
        }

        public List<MaterialCommodities> List { get; }

        public MaterialCommodities Find(MaterialCommodityData other) { return List.Find(x => x.Details.FDName.Equals(other.FDName, StringComparison.InvariantCultureIgnoreCase)); }
        public MaterialCommodities FindFDName(string fdname) { return List.Find(x => x.Details.FDName.Equals(fdname, StringComparison.InvariantCultureIgnoreCase)); }

        public List<MaterialCommodities> Sort(bool commodity)
        {
            List<MaterialCommodities> ret = new List<MaterialCommodities>();

            if (commodity)
                ret = List.Where(x => x.Details.IsCommodity).OrderBy(x => x.Details.Type)
                           .ThenBy(x => x.Details.Name).ToList();
            else
                ret = List.Where(x => !x.Details.IsCommodity).OrderBy(x => x.Details.Name).ToList();

            return ret;
        }

        public int Count(MaterialCommodityData.CatType [] cats)    // for all types of cat, if item matches or does not, count
        {
            int total = 0;
            foreach (MaterialCommodities c in List)
            {
                if ( Array.IndexOf<MaterialCommodityData.CatType>(cats, c.Details.Category) != -1 )
                    total += c.Count;
            }

            return total;
        }

        public int DataCount { get { return Count(new MaterialCommodityData.CatType[] { MaterialCommodityData.CatType.Encoded }); } }
        public int MaterialsCount { get { return Count(new MaterialCommodityData.CatType[] { MaterialCommodityData.CatType.Raw, MaterialCommodityData.CatType.Manufactured }); } }
        public int CargoCount { get { return Count(new MaterialCommodityData.CatType[] { MaterialCommodityData.CatType.Commodity }); } }

        public int DataHash() { return List.GetHashCode(); }

        // ifnorecatonsearch is used if you don't know if its a material or commodity.. for future use.

        private MaterialCommodities GetNewCopyOf(MaterialCommodityData.CatType cat, string fdname, bool ignorecatonsearch = false)
        {
            int index = List.FindIndex(x => x.Details.FDName.Equals(fdname, StringComparison.InvariantCultureIgnoreCase) && (ignorecatonsearch || x.Details.Category == cat));

            if (index >= 0)
            {
                List[index] = new MaterialCommodities(List[index]);    // fresh copy..
                return List[index];
            }
            else
            {
                MaterialCommodityData mcdb = MaterialCommodityData.EnsurePresent(cat,fdname);    // get a MCDB of this
                MaterialCommodities mc = new MaterialCommodities(mcdb);        // make a new entry
                List.Add(mc);
                return mc;
            }
        }

        public void Change(DateTime utc, string catname, string fdname, int num, long price, bool ignorecatonsearch = false)
        {
            var cat = MaterialCommodityData.CategoryFrom(catname);
            if (cat.HasValue)
            {
                Change(utc, cat.Value, fdname, num, price, ignorecatonsearch);
            }
            else
                System.Diagnostics.Debug.WriteLine("Unknown Cat " + catname);
        }

        // ignore cat is only used if you don't know what it is 
        public void Change(DateTime utc, MaterialCommodityData.CatType cat, string fdname, int num, long price, bool ignorecatonsearch = false)
        {
            MaterialCommodities mc = GetNewCopyOf(cat, fdname, ignorecatonsearch);
       
            double costprev = mc.Count * mc.Price;
            double costnew = num * price;

            //if (mc.Count == 0 && num < 0) System.Diagnostics.Debug.WriteLine("{0} Error, removing {1} {2} but nothing counted", utc, fdname, num);

            mc.Count = Math.Max(mc.Count + num, 0);

            if (mc.Count > 0 && num > 0)      // if bought (defensive with mc.count)
                mc.Price = (costprev + costnew) / mc.Count;       // price is now a combination of the current cost and the new cost. in case we buy in tranches

            //System.Diagnostics.Debug.WriteLine("In {0} At {1} MC Change {2} {3} {4} = {5}", System.Threading.Thread.CurrentThread.Name, utc, cat, fdname, num,mc.Count);
        }

        public void Craft(string fdname, int num)
        {
            int index = List.FindIndex(x => x.Details.FDName.Equals(fdname, StringComparison.InvariantCultureIgnoreCase));

            if (index >= 0)
            {
                MaterialCommodities mc = new MaterialCommodities(List[index]);      // new clone of
                List[index] = mc;       // replace ours with new one
                mc.Count = Math.Max(mc.Count - num, 0);
            }
        }

        public void Died()
        {
            List.RemoveAll(x => x.Details.IsCommodity);      // empty the list of all commodities
        }

        public void Set(string catname, string fdname, int num, double price)
        {
            var cat = MaterialCommodityData.CategoryFrom(catname);
            if (cat.HasValue)
            {
                Set(cat.Value, fdname, num, price);
            }
            else
                System.Diagnostics.Debug.WriteLine("Unknown Cat " + catname);
        }

        public void Set(MaterialCommodityData.CatType cat, string fdname, int num, double price)
        {
            MaterialCommodities mc = GetNewCopyOf(cat, fdname);

            mc.Count = num;
            if (price > 0)
                mc.Price = price;
        }

        public void Clear(bool commodity)
        {
            for (int i = 0; i < List.Count; i++)
            {
                MaterialCommodities mc = List[i];
                if (commodity == mc.Details.IsCommodity)
                {
                    List[i] = new MaterialCommodities(List[i]);     // new clone of it we can change..
                    List[i].Count = 0;  // and clear it
                }
            }
        }

        static public MaterialCommoditiesList Process(JournalEntry je, MaterialCommoditiesList oldml)
        {
            MaterialCommoditiesList newmc = oldml ?? new MaterialCommoditiesList();

            if (je is ICommodityJournalEntry || je is IMaterialJournalEntry)    // could be both
            {
                newmc = new MaterialCommoditiesList(oldml);          // so we need a new one, makes a new list, but copies the items..

                if (je is ICommodityJournalEntry)
                {
                    ICommodityJournalEntry e = je as ICommodityJournalEntry;
                    e.UpdateCommodities(newmc);
                }

                if (je is IMaterialJournalEntry)
                {
                    IMaterialJournalEntry e = je as IMaterialJournalEntry;
                    e.UpdateMaterials(newmc);
                }
            }

            return newmc;
        }
    }
}
