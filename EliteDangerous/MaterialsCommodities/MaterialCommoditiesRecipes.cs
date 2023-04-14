/*
 * Copyright © 2016-2021 EDDiscovery development team
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

namespace EliteDangerousCore
{
    // helper to handle recipe data with material commodities lists

    public static class MaterialCommoditiesRecipe
    {
        static public Dictionary<string, int> TotalList(List<MaterialCommodityMicroResource> mcl)      // holds totals list by FDName, used during computation of below functions
        {
            var used = new Dictionary<string, int>();
            foreach (var x in mcl)
                used.Add(x.Details.FDName, x.Count);
            return used;
        }

        // return maximum can make, how many made, needed string, needed string long format, and the % to having one recipe
        // select if totals is reduced by the making
        static public Tuple<int, int, string, string,double> HowManyLeft(
                            Recipes.Recipe r,       // the receipe to do..
                            int tomake,         // how many to make..
                            List<MaterialCommodityMicroResource> currentcontents,   
                            Dictionary<string, int> available,     // current available totals, started at what we have, then decreased if required as you process 
                            Dictionary<MaterialCommodityMicroResourceType, int> totalneeded = null,      // This is the total amount needed of items
                            bool reducetotals = true)
        {
            int max = int.MaxValue;

            System.Text.StringBuilder neededstr = new System.Text.StringBuilder(256);
            System.Text.StringBuilder neededstrverbose = new System.Text.StringBuilder(256);

            int itemsavailable = 0;

            for (int i = 0; i < r.Ingredients.Length; i++)
            {
                MaterialCommodityMicroResourceType ingmct = MaterialCommodityMicroResourceType.GetByFDName(r.Ingredients[i].FDName);  

                if ( ingmct != null )       // must be known
                {
                    var curmct = currentcontents.Find(x => x.Details == ingmct);       // have we got it?
                    
                    int got = curmct!=null ? available[curmct.Details.FDName] : 0;      // what we have got
                    int sets = got / r.Amount[i];

                    max = Math.Min(max, sets);

                    int need = r.Amount[i] * tomake;

                    if (totalneeded != null)
                    {
                        if (totalneeded.TryGetValue(ingmct, out int curneeded))     // update the total amount needed
                            totalneeded[ingmct] += need;
                        else
                            totalneeded[ingmct] = need;
                    }

                    itemsavailable += Math.Min(r.Amount[i], got);          // up to amount, how many of these have we got..

                    if (got < need)
                    {
                        string sshort = (need - got).ToString() + (ingmct.IsEncodedOrManufactured ? " " + ingmct.Name : ingmct.Shortname);
                        string slong = (need - got).ToString() + " x " + ingmct.Name + Environment.NewLine;

                        if (neededstr.Length == 0)
                        {
                            neededstr.Append("Need:" + sshort);
                            neededstrverbose.Append("Need:" + Environment.NewLine + slong);
                        }
                        else
                        {
                            neededstr.Append(", " + sshort);
                            neededstrverbose.Append(slong);
                        }
                    }
                }
            }

          //  System.Diagnostics.Debug.WriteLine($"Recipe {r.Name} total ing {r.Ingredients.Length} No.Ing {r.Amounts} Got {itemsavailable}");

            int made = 0;

            if (max > 0 && tomake > 0)             // if we have a set, and use it up
            {
                made = Math.Min(max, tomake);                // can only make this much
                System.Text.StringBuilder usedstrshort = new System.Text.StringBuilder(64);
                System.Text.StringBuilder usedstrlong = new System.Text.StringBuilder(64);

                for (int i = 0; i < r.Ingredients.Length; i++)
                {
                    string ingredient = r.Ingredients[i].Shortname;
                    int mi = currentcontents.FindIndex(x => x.Details.Shortname.Equals(ingredient));
                    System.Diagnostics.Debug.Assert(mi != -1);
                    int used = r.Amount[i] * made;

                    if ( reducetotals)  // may not want to chain recipes
                        available[ currentcontents[mi].Details.FDName] -= used;

                    string dispshort = (currentcontents[mi].Details.IsEncodedOrManufactured) ? " " + currentcontents[mi].Details.Name : currentcontents[mi].Details.Shortname;
                    string displong = " " + currentcontents[mi].Details.Name;

                    usedstrshort.AppendPrePad(used.ToString() + dispshort, ", ");
                    usedstrlong.AppendPrePad(used.ToString() + " x " + displong, Environment.NewLine);
                }

                neededstr.AppendPrePad("Used: " + usedstrshort.ToString(), ", ");
                neededstrverbose.Append("Used: " + Environment.NewLine + usedstrlong.ToString());
            }

            return new Tuple<int, int, string, string,double>(max, made, neededstr.ToNullSafeString(), neededstrverbose.ToNullSafeString(),itemsavailable*100.0/r.Amounts);
        }

        // return shopping list/count given receipe list, list of current materials.
        // get rid of..

        static public List<Tuple<MaterialCommodityMicroResource,int>> GetShoppingList(List<Tuple<Recipes.Recipe, int>> wantedrecipes, List<MaterialCommodityMicroResource> list)
        {
            var shoppingList = new List<Tuple<MaterialCommodityMicroResource, int>>();
            var totals = TotalList(list);

            foreach (Tuple<Recipes.Recipe, int> want in wantedrecipes)
            {
                Recipes.Recipe r = want.Item1;
                int wanted = want.Item2;

                for (int i = 0; i < r.Ingredients.Length; i++)
                {
                    string ingredient = r.Ingredients[i].Shortname;

                    int mi = list.FindIndex(x => x.Details.Shortname.Equals(ingredient));   // see if we have any in list

                    MaterialCommodityMicroResource matc = mi != -1 ? list[mi] : new MaterialCommodityMicroResource( MaterialCommodityMicroResourceType.GetByShortName(ingredient) );   // if not there, make one
                    if (mi == -1)               // if not there, make an empty total entry
                        totals[matc.Details.FDName] = 0;

                    int got = totals[matc.Details.FDName];      // what we have left from totals
                    int need = r.Amount[i] * wanted;
                    int left = got - need;

                    if (left < 0)     // if not enough
                    {
                        int shopentry = shoppingList.FindIndex(x => x.Item1.Details.Shortname.Equals(ingredient));      // have we already got it in the shopping list

                        if (shopentry >= 0)     // found, update list with new wanted total
                        {
                            shoppingList[shopentry] = new Tuple<MaterialCommodityMicroResource, int>(shoppingList[shopentry].Item1, shoppingList[shopentry].Item2 - left);       // need this more
                        }
                        else
                        {
                            shoppingList.Add(new Tuple<MaterialCommodityMicroResource, int>(matc, -left));  // a new shop entry with this many needed
                        }

                        totals[matc.Details.FDName] = 0;            // clear count
                    }
                    else
                    {
                        totals[matc.Details.FDName] -= need;        // decrement total
                    }
                }
            }

            shoppingList.Sort(delegate (Tuple<MaterialCommodityMicroResource,int> left, Tuple<MaterialCommodityMicroResource,int> right) { return left.Item1.Details.Name.CompareTo(right.Item1.Details.Name); });

            return shoppingList;
        }

    }
}