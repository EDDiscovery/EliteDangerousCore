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
        static public Tuple<int, int, string, string,double> HowManyLeft(List<MaterialCommodityMicroResource> list, 
                            Dictionary<string, int> totals, Recipes.Recipe r, int tomake = 0, bool reducetotals = true)
        {
            int max = int.MaxValue;
            System.Text.StringBuilder needed = new System.Text.StringBuilder(256);
            System.Text.StringBuilder neededlong = new System.Text.StringBuilder(256);

            int itemsavailable = 0;

            for (int i = 0; i < r.Ingredients.Length; i++)
            {
                string ingredient = r.Ingredients[i].Shortname;

                int mi = list.FindIndex(x => x.Details.Shortname.Equals(ingredient));
                int got = (mi >= 0) ? totals[list[mi].Details.FDName] : 0;
                int sets = got / r.Amount[i];

                max = Math.Min(max, sets);

                int need = r.Amount[i] * tomake;

                itemsavailable += Math.Min(r.Amount[i], got);          // up to amount, how many of these have we got..

                if (got < need)
                {
                    string dispshort;
                    string displong;
                    if (mi > 0)     // if got one..
                    {
                        dispshort = (list[mi].Details.IsEncodedOrManufactured) ? " " + list[mi].Details.Name : list[mi].Details.Shortname;
                        displong = " " + list[mi].Details.Name;
                    }
                    else
                    {
                        MaterialCommodityMicroResourceType db = MaterialCommodityMicroResourceType.GetByShortName(ingredient);
                        dispshort = (db.Category == MaterialCommodityMicroResourceType.CatType.Encoded || db.Category == MaterialCommodityMicroResourceType.CatType.Manufactured) ? " " + db.Name : db.Shortname;
                        displong = " " + db.Name;
                    }

                    string sshort = (need - got).ToString() + dispshort;
                    string slong = (need - got).ToString() + " x " + displong + Environment.NewLine;

                    if (needed.Length == 0)
                    {
                        needed.Append("Need:" + sshort);
                        neededlong.Append("Need:" + Environment.NewLine + slong);
                    }
                    else
                    {
                        needed.Append(", " + sshort);
                        neededlong.Append(slong);
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
                    int mi = list.FindIndex(x => x.Details.Shortname.Equals(ingredient));
                    System.Diagnostics.Debug.Assert(mi != -1);
                    int used = r.Amount[i] * made;

                    if ( reducetotals)  // may not want to chain recipes
                        totals[ list[mi].Details.FDName] -= used;

                    string dispshort = (list[mi].Details.IsEncodedOrManufactured) ? " " + list[mi].Details.Name : list[mi].Details.Shortname;
                    string displong = " " + list[mi].Details.Name;

                    usedstrshort.AppendPrePad(used.ToString() + dispshort, ", ");
                    usedstrlong.AppendPrePad(used.ToString() + " x " + displong, Environment.NewLine);
                }

                needed.AppendPrePad("Used: " + usedstrshort.ToString(), ", ");
                neededlong.Append("Used: " + Environment.NewLine + usedstrlong.ToString());
            }

            return new Tuple<int, int, string, string,double>(max, made, needed.ToNullSafeString(), neededlong.ToNullSafeString(),itemsavailable*100.0/r.Amounts);
        }

        // return shopping list/count given receipe list, list of current materials.

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