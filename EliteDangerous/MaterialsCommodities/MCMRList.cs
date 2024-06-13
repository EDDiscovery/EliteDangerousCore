/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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

using BaseUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("MatC {Details.Category} {Details.Name} {Details.FDName} count {Count}")]
    public class MaterialCommodityMicroResource
    {
        public const int NoCounts = 2;

        public int Count { get { return Counts[0]; } }       // backwards compatible with code when there was only 1 count
        public int[] Counts { get; set; }
        public bool NonZero { get { return Counts[0] != 0 || Counts[1] != 0; } }

        public int Total { get { return Counts.Sum(); } }

        static public int[] ZeroCounts { get { return new int[NoCounts]; } }

        public double Price { get; set; }
        public MaterialCommodityMicroResourceType Details { get; set; }

        public MaterialCommodityMicroResource(MaterialCommodityMicroResourceType c)
        {
            Counts = new int[NoCounts];
            Price = 0;
            this.Details = c;
        }

        public MaterialCommodityMicroResource(MaterialCommodityMicroResource c)
        {
            Counts = new int[NoCounts];
            Array.Copy(c.Counts, Counts, NoCounts);
            Price = c.Price;
            this.Details = c.Details;       // can copy this, its fixed
        }
    }

    public class MaterialCommoditiesMicroResourceList
    {
        private GenerationalDictionary<string, MaterialCommodityMicroResource> items = new GenerationalDictionary<string, MaterialCommodityMicroResource>();

        public MaterialCommoditiesMicroResourceList()
        {
        }

        public MaterialCommodityMicroResource Get(uint gen, string fdname)
        {
            return items.Get(fdname.ToLowerInvariant(), gen);
        }

        public MaterialCommodityMicroResource GetLast(string fdname)
        {
            return items.GetLast(fdname.ToLowerInvariant());
        }

        public List<MaterialCommodityMicroResource> GetLast()
        {
            return items.GetLastValues();
        }

        public List<MaterialCommodityMicroResource> Get(uint gen)
        {
            return items.GetValues(gen);
        }

        public Dictionary<string, MaterialCommodityMicroResource> GetDict(uint gen)
        {
            return items.Get(gen);
        }

        public List<MaterialCommodityMicroResource> GetMaterialsSorted(uint gen)
        {
            var list = items.GetValues(gen, x => x.Details.IsMaterial);
            return list.OrderBy(x => x.Details.Type).ThenBy(x => x.Details.TranslatedName).ToList();
        }

        public List<MaterialCommodityMicroResource> GetCommoditiesSorted(uint gen)
        {
            var list = items.GetValues(gen, x => x.Details.IsCommodity);
            return list.OrderBy(x => x.Details.Type).ThenBy(x => x.Details.TranslatedName).ToList();
        }

        public List<MaterialCommodityMicroResource> GetMicroResourcesSorted(uint gen)
        {
            var list = items.GetValues(gen, x => x.Details.IsMicroResources);
            return list.OrderBy(x => x.Details.Type).ThenBy(x => x.Details.TranslatedName).ToList();
        }

        public List<MaterialCommodityMicroResource> GetSorted(uint gen, bool commodityormaterial)       // true = commodity
        {
            var list = items.GetValues(gen);
            List<MaterialCommodityMicroResource> ret = null;

            if (commodityormaterial)
                ret = list.Where(x => x.Details.IsCommodity).OrderBy(x => x.Details.Type).ThenBy(x => x.Details.TranslatedName).ToList();
            else
                ret = list.Where(x => x.Details.IsMaterial).OrderBy(x => x.Details.TranslatedName).ToList();

            return ret;
        }

        static public int[] Count(List<MaterialCommodityMicroResource> list, int cn = 0)        // Counts of different types, array, use CatType to index it
        {
            System.Diagnostics.Debug.Assert(Enum.GetNames(typeof(MaterialCommodityMicroResourceType.CatType)).Length == 8);  // done purposely this way so only debug builds pay the price for getnames
            int[] ret = new int[8];
            foreach (MaterialCommodityMicroResource c in list)
                ret[(int)c.Details.Category] += c.Counts[cn];
            return ret;
        }

        public int CargoCount(uint gen) { return Count(Get(gen))[(int)MaterialCommodityMicroResourceType.CatType.Commodity]; }

        // change entry 0
        public void Change(DateTime utc, string catname, string fdname, int num, long price, int cnum = 0, bool setit = false)        
        {
            var cat = MaterialCommodityMicroResourceType.CategoryFrom(catname);
            if (cat.HasValue)
            {
                Change(utc, cat.Value, fdname, num, price, cnum, setit);
            }
            else
                System.Diagnostics.Debug.WriteLine("MCMRLIST Unknown Cat " + catname);
        }

        // change entry 0
        public void Change(DateTime utc, MaterialCommodityMicroResourceType.CatType cat, string fdname, int num, long price, int cnum = 0, bool setit = false)
        {
            var vsets = new bool[MaterialCommodityMicroResource.NoCounts];      // all set to false, change
            vsets[cnum] = setit;                                                // set cnum to change/set
            var varray = new int[MaterialCommodityMicroResource.NoCounts];      // all set to zero
            varray[cnum] = num;                                                 // set value on cnum
            Change(utc, cat, fdname, varray, vsets, price);
        }

        // counts/set array can be of length 1 to maximum number of counts
        // to set a value, set count/set=1 for that entry
        // to change a value, set count/set = 0 for that entry
        // to leave a value, set count=0,set=0 for that entry
        // set means set to value, else add to value
        public bool Change(DateTime utc, MaterialCommodityMicroResourceType.CatType cat, string fdname, int[] counts, bool[] set, long price)
        {
            fdname = fdname.ToLowerInvariant();

            MaterialCommodityMicroResource mc = items.GetLast(fdname);      // find last entry, may return null if none stored

            if (mc == null)     // not stored, make new
            {
                MaterialCommodityMicroResourceType mcdb = MaterialCommodityMicroResourceType.EnsurePresent(cat, fdname);    // get a MCDB of this
                mc = new MaterialCommodityMicroResource(mcdb);
            }
            else
            {
                mc = new MaterialCommodityMicroResource(mc);                // copy constructor, new copy of it
            }

            double costprev = mc.Counts[0] * mc.Price;
            double costofnew = counts[0] * price;
            bool changed = false;

            for (int i = 0; i < counts.Length; i++)
            {
                int newcount = set[i] ? counts[i] : Math.Max(mc.Counts[i] + counts[i], 0);       // don't let it go below zero if changing
                if (newcount != mc.Counts[i])
                {
                    changed = true;
                  //  System.Diagnostics.Debug.WriteLine("MCMRLIST {0} Gen {1} Changed {2}:{3} Entry {4} {5} -> {6} {7}", utc.ToString(), items.Generation, mc.Details.Category, mc.Details.FDName, i, mc.Counts[i], newcount, mc.Counts[i]<newcount ? "+++" : "---");
                 //   System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
                    mc.Counts[i] = newcount;
                }
            }

            if (changed)                                                    // only store back a new entry if material change to counts
            {
                if (mc.Counts[0] > 0 && counts[0] > 0)                      // if bought (defensive with mc.counts)
                    mc.Price = (costprev + costofnew) / mc.Counts[0];       // price is now a combination of the current cost and the new cost. in case we buy in tranches

                items[fdname] = mc;                                         // and set fdname to mc - this allows for any repeat adds due to frontier data repeating stuff in things like cargo
            }
            else
            {
                // System.Diagnostics.Debug.WriteLine("{0} Not changed {1} {2}", utc.ToString(), mc.Details.FDName, mc.Count);
            }

            return changed;
        }

        //always changes entry 0
        public void Craft(DateTime utc, string fdname, int num)       
        {
            MaterialCommodityMicroResource mc = items.GetLast(fdname.ToLowerInvariant());      // find last entry, may return null if none stored
            if ( mc != null )
            {
                mc = new MaterialCommodityMicroResource(mc);      // new clone of
                mc.Counts[0] = Math.Max(mc.Counts[0] - num, 0);
                items[mc.Details.FDName.ToLowerInvariant()] = mc;
                //System.Diagnostics.Debug.WriteLine("MCMRLIST {0} Craft {1} {2}", utc.ToString(), mc.Details.FDName, num);
            }
        }

        public void Clear(int cnum, params MaterialCommodityMicroResourceType.CatType[] cats)
        {
            foreach (var cat in cats)       // meow
            {
                var list = items.GetLastValues((x) => x.Details.Category == cat && x.Counts[cnum] > 0);     // find all of this cat with a count >0
                foreach (var e in list)
                {
                    var mc = new MaterialCommodityMicroResource(e);     // clone it
                    mc.Counts[cnum] = 0;
                    items[e.Details.FDName.ToLowerInvariant()] = mc;        // and add to end of list
                }
            }
        }

        // ensure a category has the same values as in values, for cnum entry.
        // All others in the same cat not mentioned in values go to zero
        // make sure values has name lower case.

        public int Update(DateTime utc, MaterialCommodityMicroResourceType.CatType cat, List<Tuple<string,int>> values, int cnum = 0)
        {
            var curlist = items.GetLastValues((x) => x.Details.Category == cat && x.Counts[cnum]>0);     // find all of this cat with a count >0

            var varray = new int[MaterialCommodityMicroResource.NoCounts];      // all set to zero
            var vsets = new bool[MaterialCommodityMicroResource.NoCounts];      // all set to false, change, with 0 in the varray, no therefore no change

            vsets[cnum] = true;                                                 // but set the cnum to set

            int changed = 0;

            //System.Diagnostics.Debug.WriteLine("Perform update for " + cat.ToString());
            foreach (var v in values)           
            {
                varray[cnum] = v.Item2;                         // set cnum value
                if (Change(utc, cat, v.Item1, varray, vsets, 0))      // set entry 
                {
                    //System.Diagnostics.Debug.WriteLine("MCMRLIST {0} updated {1} {2} to {3} (entry {4})", utc.ToString(), cat, v.Item1, v.Item2 , cnum);
                    changed++;                                 // indicated changed
                }
            }

            foreach( var c in curlist)                                          //go thru the non zero list of cat
            {
                if ( values.Find(x=>x.Item1.Equals(c.Details.FDName,StringComparison.InvariantCultureIgnoreCase)) == null )       // if not in updated list
                {
                    var mc = new MaterialCommodityMicroResource(c);     // clone it
                    mc.Counts[cnum] = 0;            // zero cnum
                    items[c.Details.FDName.ToLowerInvariant()] = mc;
                    //System.Diagnostics.Debug.WriteLine("MCMRLIST {0} Found {1}:{2} not in update list, zeroing", utc.ToString(), mc.Details.Category, mc.Details.FDName);
                    changed++;
                }
            }

            //if (changed > 0) System.Diagnostics.Debug.WriteLine("MCMRLIST {0} {1} fixed {2}", utc.ToString() , cat, changed);

            return changed;
        }

#if !TESTHARNESS
        public uint Process(JournalEntry je, JournalEntry lastentry, bool insrv )
        {
            if (je is ICommodityJournalEntry || je is IMaterialJournalEntry || je is IMicroResourceJournalEntry)
            {
                items.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                if (je is ICommodityJournalEntry)
                {
                    ICommodityJournalEntry e = je as ICommodityJournalEntry;
                    e.UpdateCommodities(this,insrv);
                }

                if (je is IMaterialJournalEntry)
                {
                    IMaterialJournalEntry e = je as IMaterialJournalEntry;
                    e.UpdateMaterials(this);
                }

                if (je is IMicroResourceJournalEntry)
                {
                    IMicroResourceJournalEntry e = je as IMicroResourceJournalEntry;
                    e.UpdateMicroResource(this, lastentry);
                }

                if (items.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                   // System.Diagnostics.Debug.WriteLine("*********************** {0} {1} No changes for Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, items.Generation);
                    items.AbandonGeneration();
                }
                else
                {
                 //   System.Diagnostics.Debug.WriteLine("*********************** {0} {1} Generation {2} Changes {3}",je.EventTimeUTC.ToString(), je.EventTypeStr, items.Generation,items.UpdatesAtThisGeneration);
                }
            }

            return items.Generation;        // return the generation we are on.
        }

#endif
    }
}
