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
    public abstract class RecipeBase
    {
        public string Name { get; private set; }        // name of receipe ie "Lightweight Hull Reinforcement"
        public MaterialCommodityMicroResourceType[] Ingredients { get; private set; }
        public int[] Amount { get; private set; }
        public int Count { get { return Ingredients.Length; } }     // number of different items
        public int TotalItems { get { return Amount.Sum(); } }     // total sum of items needed

        public RecipeBase(string name, string ingredientsstring)
        {
            Name = name;
            if (ingredientsstring.HasChars())
            {
                string[] ilist = ingredientsstring.Split(',');
                Ingredients = new MaterialCommodityMicroResourceType[ilist.Length];
                Amount = new int[ilist.Length];

                if ( Name == "Fast Scanner")
                {

                }
                for (int i = 0; i < ilist.Length; i++)
                {
                    string s = new string(ilist[i].TakeWhile(c => !Char.IsLetter(c)).ToArray());
                    string iname = ilist[i].Substring(s.Length);
                    Ingredients[i] = MaterialCommodityMicroResourceType.GetByShortName(iname);
                    System.Diagnostics.Debug.Assert(Ingredients[i] != null, "Not found ingredient " + Name + " " + ingredientsstring + " i=" + i + " " + Ingredients[i]);

                    // if (Ingredients[i].Category == MaterialCommodityMicroResourceType.CatType.Commodity) System.Diagnostics.Debug.WriteLine($"Recipe {Name} {ingredientsstring} has a commodity {Ingredients[i].Name}");
                    // if (Ingredients[i].IsMicroResources) System.Diagnostics.Debug.WriteLine($"Recipe {Name} {ingredientsstring} has a MR {Ingredients[i].Name} {Ingredients[i].Category}");

                    bool countsuccess = int.TryParse(s, out Amount[i]);
                    System.Diagnostics.Debug.Assert(countsuccess, "Count missing from ingredient");
                }
            }
            else
                Ingredients = new MaterialCommodityMicroResourceType[0];
        }

        public string IngredientsString
        {
            get
            {
                var ing = (from x in Ingredients select Amount[Array.IndexOf(Ingredients, x)].ToString() + x.Shortname).ToArray();
                return string.Join(", ", ing);
            }
        }
        public string IngredientsStringvsCurrent(List<MaterialCommodityMicroResource> cur)
        {
            var ing = (from x in Ingredients select Amount[Array.IndexOf(Ingredients, x)].ToString() + x.Shortname + "(" + (cur.Find((z) => z.Details.FDName == x.FDName)?.Count ?? 0).ToStringInvariant() + ")").ToArray();
            return string.Join(", ", ing);
        }

        public string IngredientsStringLong
        {
            get
            {
                var ing = (from x in Ingredients select Amount[Array.IndexOf(Ingredients, x)].ToString() + " " + x.TranslatedName).ToArray();
                return string.Join(", ", ing);
            }
        }

        // always returns a string, may be empty
        public static string UsedInRecipesByFDName(MCFDName fdname, string join = ", ")
        {
            string s = EngineeringRecipe.UsedInEngineeringByFDName(fdname, join);
            s = s.AppendPrePad(SynthesisRecipe.UsedInSythesisByFDName(fdname, join), join);
            return s;
        }
    }
}