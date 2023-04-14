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
using System.Linq;

namespace EliteDangerousCore
{
    public static class Recipes
    {
        public class Recipe
        {
            public string Name;
            public MaterialCommodityMicroResourceType[] Ingredients;
            public int[] Amount;

            public int Count { get { return Ingredients.Length; } }

            public int Amounts { get { return Amount.Sum(); } }     // number of items

            public Recipe(string n, string ingredientsstring)
            {
                Name = n;
                string[] ilist = ingredientsstring.Split(',');
                Ingredients = new MaterialCommodityMicroResourceType[ilist.Length];
                Amount = new int[ilist.Length];

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
                    var ing = (from x in Ingredients select Amount[Array.IndexOf(Ingredients, x)].ToString() + " " + x.Name).ToArray();
                    return string.Join(", ", ing);
                }
            }

        }

        public class SynthesisRecipe : Recipe
        {
            public string level;

            public SynthesisRecipe(string n, string l, string indg)
                : base(n, indg)
            {
                level = l;
            }
        }


        [System.Diagnostics.DebuggerDisplay("Rec {fdname} {level} {modulesstring} {string.Join(\",\",engineers)}")]
        public class EngineeringRecipe : Recipe
        {
            public string level;
            public int LevelInt { get { return level.InvariantParseInt(-1); } }     
            public string modulesstring;
            public string[] modules;
            public string[] engineers;
            public string fdname;       // only certain types have a fdname

            public EngineeringRecipe(string n, string fdname, string indg, string mod, string lvl, string engnrs)
                : base(n, indg)
            {
                this.fdname = fdname;
                level = lvl;
                modulesstring = mod;
                modules = modulesstring.Split(',');
                engineers = engnrs.Split(',');
            }

            public EngineeringRecipe(string n, string fdname, string type, string mod, string indg)        // for tech broker
                : base(n, indg)
            {
                level = "NA";
                this.fdname = fdname;
                modulesstring = mod;
                modules = modulesstring.Split(',');
                engineers = type.Split(',');
            }

            public EngineeringRecipe(string n, string mod, string indg)        // for special effects
                : base(n, indg)
            {
                level = "NA";
                modulesstring = mod;
                modules = modulesstring.Split(',');
                engineers = new string[] { "Special Effect" };
            }

            public EngineeringRecipe(string type, string manu, int lvl, string indg)        // for suit/weapon upgrades
                : base(manu, indg)
            {
                level = lvl.ToString();
                modulesstring = type;
                modules = modulesstring.Split(',');
                engineers = type.Split(',');
            }

            public EngineeringRecipe(string type, string fdname, string manu, string n, int cost, string indg, string eng)        // for suit/weapon engineer mods
                : base(n + (manu != "All" ? (": " + manu) : ""), indg)
            {
                level = "NA";
                modulesstring = type;
                this.fdname = fdname;
                modules = modulesstring.Split(',');
                engineers = eng.Split(',');
            }
        }

        public static string UsedInRecipesByFDName(string fdname, string join = ", ")
        {
            string s = Recipes.UsedInEngineeringByFDName(fdname, join);
            s = s.AppendPrePad(Recipes.UsedInSythesisByFDName(fdname, join), join);
            return s;
        }


        public static string UsedInSythesisByFDName(string fdname, string join = ", ")
        {
            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);
            if (mc != null && SynthesisRecipesByMaterial.ContainsKey(mc))
                return String.Join(join, SynthesisRecipesByMaterial[mc].Select(x => x.Name + "-" + x.level + ": " + x.IngredientsStringLong));
            else
                return "";
        }

        public static string UsedInEngineeringByFDName(string fdname, string join = ", ")
        {
            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);
            if (mc != null && EngineeringRecipesByMaterial.ContainsKey(mc))
                return String.Join(join, EngineeringRecipesByMaterial[mc].Select(x => x.modulesstring + " " + x.Name + "-" + x.level + ": " + x.IngredientsStringLong + " @ " + string.Join(",",x.engineers)));
            else
                return "";
        }

        public static string GetBetterNameForEngineeringRecipe(string fdname)
        {
            var lfdname = fdname.ToLowerInvariant();
            var f = EngineeringRecipes.Find(x => x.fdname == lfdname);
            return f == null ? fdname : f.Name;
        }

        public static SynthesisRecipe FindSynthesis(string recipename, string level)
        {
            return SynthesisRecipes.Find(x => x.Name.Equals(recipename, StringComparison.InvariantCultureIgnoreCase) && x.level.Equals(level, StringComparison.InvariantCultureIgnoreCase));
        }

        public static List<SynthesisRecipe> SynthesisRecipes = new List<SynthesisRecipe>()
        {
            new SynthesisRecipe( "FSD", "Premium","1C,1Ge,1Nb,1As,1Po,1Y" ),
            new SynthesisRecipe( "FSD", "Standard","1C,1V,1Ge,1Cd,1Nb" ),
            new SynthesisRecipe( "FSD", "Basic","1C,1V,1Ge" ),

            new SynthesisRecipe( "AFM Refill", "Premium","6V,4Cr,2Zn,2Zr,1Te,1Ru" ),
            new SynthesisRecipe( "AFM Refill", "Standard","6V,2Mn,1Mo,1Zr,1Sn" ),
            new SynthesisRecipe( "AFM Refill", "Basic","3V,2Ni,2Cr,2Zn" ),

            new SynthesisRecipe( "SRV Ammo", "Premium","2P,2Se,1Mo,1Tc" ),
            new SynthesisRecipe( "SRV Ammo", "Standard","1P,1Se,1Mn,1Mo" ),
            new SynthesisRecipe( "SRV Ammo", "Basic","1P,2S" ),

            new SynthesisRecipe( "SRV Repair", "Premium","2V,1Zn,2Cr,1W,1Te" ),
            new SynthesisRecipe( "SRV Repair", "Standard","3Ni,2V,1Mn,1Mo" ),
            new SynthesisRecipe( "SRV Repair", "Basic","2Fe,1Ni" ),

            new SynthesisRecipe( "SRV Refuel", "Premium","1S,1As,1Hg,1Tc" ),
            new SynthesisRecipe( "SRV Refuel", "Standard","1P,1S,1As,1Hg" ),
            new SynthesisRecipe( "SRV Refuel", "Basic","1P,1S" ),

            new SynthesisRecipe( "Plasma Munitions", "Premium", "5Se,4Mo,4Cd,2Tc" ),
            new SynthesisRecipe( "Plasma Munitions", "Standard","5P,1Se,3Mn,4Mo" ),
            new SynthesisRecipe( "Plasma Munitions", "Basic","4P,3S,1Mn" ),

            new SynthesisRecipe( "Explosive Munitions", "Premium","5P,4As,5Hg,5Nb,5Po" ),
            new SynthesisRecipe( "Explosive Munitions", "Standard","6P,6S,4As,2Hg" ),
            new SynthesisRecipe( "Explosive Munitions", "Basic","4S,3Fe,3Ni,4C" ),

            new SynthesisRecipe( "Small Calibre Munitions", "Premium","2P,2S,2Zr,2Hg,2W,1Sb" ),
            new SynthesisRecipe( "Small Calibre Munitions", "Standard","2P,2Fe,2Zr,2Zn,2Se" ),
            new SynthesisRecipe( "Small Calibre Munitions", "Basic","2S,2Fe,1Ni" ),

            new SynthesisRecipe( "High Velocity Munitions", "Premium","4V,2Zr,4W,2Y" ),
            new SynthesisRecipe( "High Velocity Munitions", "Standard","4Fe,3V,2Zr,2W" ),
            new SynthesisRecipe( "High Velocity Munitions", "Basic","2Fe,1V" ),

            new SynthesisRecipe( "Large Calibre Munitions", "Premium","8Zn,1As,1Hg,2W,2Sb" ),
            new SynthesisRecipe( "Large Calibre Munitions", "Standard","3P,2Zr,3Zn,1As,2Sn" ),
            new SynthesisRecipe( "Large Calibre Munitions", "Basic","2S,4Ni,3C" ),

            new SynthesisRecipe( "Limpets", "Basic", "10Fe,10Ni"),

            new SynthesisRecipe( "Chaff", "Premium", "1CC,2FiC,1ThA,1PRA"),
            new SynthesisRecipe( "Chaff", "Standard", "1CC,2FiC,1ThA"),
            new SynthesisRecipe( "Chaff", "Basic", "1CC,1FiC"),

            new SynthesisRecipe( "Heat Sinks", "Premium", "2BaC,2HCW,2HE,1PHR"),
            new SynthesisRecipe( "Heat Sinks", "Standard", "2BaC,2HCW,2HE"),
            new SynthesisRecipe( "Heat Sinks", "Basic", "1BaC,1HCW"),

            new SynthesisRecipe( "Life Support", "Basic", "2Fe,1Ni"),

            new SynthesisRecipe("AX Small Calibre Munitions", "Basic", "2Fe,1Ni,2S,2WP"),
            new SynthesisRecipe("AX Small Calibre Munitions", "Standard", "2Fe,2P,2Zr,3UES,4WP" ),
            new SynthesisRecipe("AX Small Calibre Munitions", "Premium", "3Fe,2P,2Zr,4UES,2UKCP,6WP" ),

            new SynthesisRecipe("Guardian Plasma Charger Munitions", "Basic", "3Cr,2HDP,3GPC,4GSWC"),
            new SynthesisRecipe("Guardian Plasma Charger Munitions", "Standard", "4Cr,2HE,2PA,2GPCe,2GTC"),
            new SynthesisRecipe("Guardian Plasma Charger Munitions", "Premium", "6Cr,2Zr,4HE,6PA,4GPCe,3GSWP"),

            new SynthesisRecipe("Guardian Gauss Cannon Munitions", "Basic", "3Mn,2FoC,2GPC,4GSWC"),
            new SynthesisRecipe("Guardian Gauss Cannon Munitions", "Standard", "5Mn,3HRC,5FoC,4GPC,3GSWP"),
            new SynthesisRecipe("Guardian Gauss Cannon Munitions", "Premium", "8Mn,4HRC,6FiC,10FoC"),

            new SynthesisRecipe("Enzyme Missile Launcher Munitions", "Basic", "3Fe,3S,4BMC,3PE,3WP,2Pb"),
            new SynthesisRecipe("Enzyme Missile Launcher Munitions", "Standard", "6S,4W,5BMC,6PE,4WP,4Pb"),
            new SynthesisRecipe("Enzyme Missile Launcher Munitions", "Premium", "5P,4W,6BMC,5PE,4WP,6Pb"),

            new SynthesisRecipe("AX Remote Flak Munitions", "Basic", "4Ni,3C,2S"),
            new SynthesisRecipe("AX Remote Flak Munitions", "Standard", "2Sn,3Zn,1As,3UKTC,2WP"),
            new SynthesisRecipe("AX Remote Flak Munitions", "Premium", "8Zn,2W,1As,3UES,4UKTC,1WP"),

            new SynthesisRecipe("Flechette Launcher Munitions", "Basic", "1W,3EA,2MC,2B"),
            new SynthesisRecipe("Flechette Launcher Munitions", "Standard", "4W,6EA,4MC,4B"),
            new SynthesisRecipe("Flechette Launcher Munitions", "Premium", "6W,5EA,9MC,6B"),

            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Basic", "3C,2V,3CS,3GPCe,5GSWC"),
            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Standard", "4CS,2GPCe,2GSWP"),
            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Premium", "8C,3Se,4V,8CS"),

            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Basic", "3GR,2HDP,2FoC,2PA,2Pb"),
            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Standard", "5GR,3HDP,4FoC,5PA,3Pb"),
            new SynthesisRecipe("Guardian Shard Cannon Munitions", "Premium", "7GR,4HDP,6FoC,8PA,5Pb"),

            new SynthesisRecipe("AX Explosive Munitions", "Basic", "3Fe,3Ni,4C,3PE"),
            new SynthesisRecipe("AX Explosive Munitions", "Standard", "6S,6P,2Hg,4UKOC,4PE"),
            new SynthesisRecipe("AX Explosive Munitions", "Premium", "5W,4Hg,2Po,5BMC,5PE,6SFD"),
        };

        public static Dictionary<MaterialCommodityMicroResourceType, List<SynthesisRecipe>> SynthesisRecipesByMaterial =
            SynthesisRecipes.SelectMany(r => r.Ingredients.Select(i => new { mat = i, recipe = r }))
                            .GroupBy(a => a.mat)
                            .ToDictionary(g => g.Key, g => g.Select(a => a.recipe).ToList());

        public static List<EngineeringRecipe> EngineeringRecipes = new List<EngineeringRecipe>()
        {

#region Engineering Recipes

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "AFM", "1", "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "AFM", "2", "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "AFM", "3", "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "AFM", "4", "Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "AFM", "5", "Petra Olmanova" ),

        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe", "Armour", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe,1CCo", "Armour", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe,1CCo,1HDC", "Armour", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Ge,1CCe,1FPC", "Armour", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1CCe,1Sn,1MGA", "Armour", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1Ni", "Armour", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1C,1Zn", "Armour", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1SAll,1V,1Zr", "Armour", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1GA,1W,1Hg", "Armour", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1PA,1Mo,1Ru", "Armour", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C", "Armour", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C,1SHE", "Armour", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C,1SHE,1HDC", "Armour", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1V,1SS,1FPC", "Armour", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1W,1CoS,1FCC", "Armour", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1Ni", "Armour", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1Ni,1V", "Armour", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1SAll,1V,1HDC", "Armour", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1GA,1W,1FPC", "Armour", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1PA,1Mo,1FCC", "Armour", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1HCW", "Armour", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1Ni,1HDP", "Armour", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1SAll,1V,1HE", "Armour", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1GA,1W,1HV", "Armour", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1PA,1Mo,1PHR", "Armour", "5", "Selene Jean,Petra Olmanova" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Beam Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Beam Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Beam Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Beam Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Beam Laser", "5", "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Burst Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Burst Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Burst Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Burst Laser", "4", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Burst Laser", "5", "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Cannon", "1", "Marsha Hicks,Tod 'The Blaster' McQuinn,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Cannon", "2", "Marsha Hicks,Tod 'The Blaster' McQuinn,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Cannon", "3", "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Cannon", "4", "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Cannon", "5", "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Cannon", "5", "The Sarge" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Cannon", "1", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Cannon", "2", "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Cannon", "3", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Cannon", "4", "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Cannon", "5", "The Sarge" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", "Cargo Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Cargo Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", "Cargo Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Cargo Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Cargo Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", "Cargo Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", "Cargo Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", "Cargo Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", "Cargo Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", "Cargo Scanner", "5", "Tiana Fortune" ),

        new EngineeringRecipe("Chaff Ammo Capacity", "misc_chaffcapacity", "1MS", "Chaff Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Chaff Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Chaff Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Chaff Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Chaff Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Chaff Launcher", "5", "Ram Tah" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Chaff Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Chaff Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Chaff Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Chaff Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Chaff Launcher", "5", "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Chaff Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Chaff Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Chaff Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Chaff Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Chaff Launcher", "5", "Ram Tah" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Collection Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Collection Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Collection Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Collection Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Collection Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Collection Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Collection Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Collection Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Collection Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Collection Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Collection Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Collection Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Collection Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Collection Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Collection Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "ECM", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "ECM", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "ECM", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "ECM", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "ECM", "5", "Ram Tah" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "ECM", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "ECM", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "ECM", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "ECM", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "ECM", "5", "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "ECM", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "ECM", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "ECM", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "ECM", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "ECM", "5", "Ram Tah" ),

        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF", "Engine", "1", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF,1ME", "Engine", "2", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF,1Cr,1MC", "Engine", "3", "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1MCF,1Se,1CCom", "Engine", "4", "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1CIF,1Cd,1PI", "Engine", "5", "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1C", "Engine", "1", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HCW,1V", "Engine", "2", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HCW,1V,1SS", "Engine", "3", "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HDP,1HDC,1CoS", "Engine", "4", "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HE,1FPC,1IS", "Engine", "5", "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1S", "Engine", "1", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1SLF,1CCo", "Engine", "2", "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1SLF,1CCo,1UED", "Engine", "3", "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1MCF,1CCe,1DED", "Engine", "4", "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1CCe,1Sn,1CED", "Engine", "5", "Professor Palin,Mel Brandon,Chloe Sedesi" ),

        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C,1ME", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C,1ME,1CIF", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1V,1MC,1SFP", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1HDC,1CCom,1EFW", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Frag Cannon", "5", "Zacariah Nemo" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Frag Cannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Frag Cannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Frag Cannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Frag Cannon", "4", "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Frag Cannon", "5", "Zacariah Nemo" ),

        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR", "FSD", "1", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR,1Cr", "FSD", "2", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR,1HDP,1Se", "FSD", "3", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1HC,1HE,1Cd", "FSD", "4", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1EA,1HV,1Te", "FSD", "5", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1ADWE", "FSD", "1", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1ADWE,1CP", "FSD", "2", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1P,1CP,1SWS", "FSD", "3", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1Mn,1CHD,1EHT", "FSD", "4", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1As,1CM,1DWEx", "FSD", "5", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1Ni", "FSD", "1", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1C,1SHE", "FSD", "2", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1C,1Zn,1SS", "FSD", "3", "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1V,1HDC,1CoS", "FSD", "4", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1W,1FPC,1IS", "FSD", "5", "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),

        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1MS", "FSD Interdictor", "1", "Colonel Bris Dekker,Felicity Farseer,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1UEF,1ME", "FSD Interdictor", "2", "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1GR,1TEC,1MC", "FSD Interdictor", "3", "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1ME,1SWS,1DSD", "FSD Interdictor", "4", "Colonel Bris Dekker,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1MC,1EHT,1CFSD", "FSD Interdictor", "5", "Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1UEF", "FSD Interdictor", "1", "Colonel Bris Dekker,Felicity Farseer,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1ADWE,1TEC", "FSD Interdictor", "2", "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1ABSD,1AFT,1OSK", "FSD Interdictor", "3", "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1USA,1SWS,1AEA", "FSD Interdictor", "4", "Colonel Bris Dekker,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1CSD,1EHT,1AEC", "FSD Interdictor", "5", "Mel Brandon" ),

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Fuel Scoop", "1", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Fuel Scoop", "2", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Fuel Scoop", "3", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Fuel Scoop", "4", "Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Fuel Scoop", "5", "Marsha Hicks" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Fuel Transfer Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Fuel Transfer Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Fuel Transfer Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Fuel Transfer Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Fuel Transfer Limpet", "5", "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Fuel Transfer Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Fuel Transfer Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Fuel Transfer Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Fuel Transfer Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Fuel Transfer Limpet", "5", "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Fuel Transfer Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Fuel Transfer Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Fuel Transfer Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Fuel Transfer Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Fuel Transfer Limpet", "5", "The Sarge,Tiana Fortune" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Hatch Breaker Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Hatch Breaker Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Hatch Breaker Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Hatch Breaker Limpet", "4", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Hatch Breaker Limpet", "5", "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Hatch Breaker Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Hatch Breaker Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Hatch Breaker Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Hatch Breaker Limpet", "4", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Hatch Breaker Limpet", "5", "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Hatch Breaker Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Hatch Breaker Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Hatch Breaker Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Hatch Breaker Limpet", "4", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Hatch Breaker Limpet", "5", "The Sarge,Tiana Fortune" ),

        new EngineeringRecipe("Heatsink Ammo Capacity", "misc_heatsinkcapacity", "1MS,1V,1Nb", "Heat Sink Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Heat Sink Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Heat Sink Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Heat Sink Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Heat Sink Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Heat Sink Launcher", "5", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Heat Sink Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Heat Sink Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Heat Sink Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Heat Sink Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Heat Sink Launcher", "5", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Heat Sink Launcher", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Heat Sink Launcher", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Heat Sink Launcher", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Heat Sink Launcher", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Heat Sink Launcher", "5", "Ram Tah,Petra Olmanova" ),

        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe", "Hull Reinforcement", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe,1CCo", "Hull Reinforcement", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe,1CCo,1HDC", "Hull Reinforcement", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Ge,1CCe,1FPC", "Hull Reinforcement", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1CCe,1Sn,1MGA", "Hull Reinforcement", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1Ni", "Hull Reinforcement", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1C,1Zn", "Hull Reinforcement", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1SAll,1V,1Zr", "Hull Reinforcement", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1GA,1W,1Hg", "Hull Reinforcement", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1PA,1Mo,1Ru", "Hull Reinforcement", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C", "Hull Reinforcement", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C,1SHE", "Hull Reinforcement", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C,1SHE,1HDC", "Hull Reinforcement", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1V,1SS,1FPC", "Hull Reinforcement", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1W,1CoS,1FCC", "Hull Reinforcement", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1Ni", "Hull Reinforcement", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1Ni,1V", "Hull Reinforcement", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1SAll,1V,1HDC", "Hull Reinforcement", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1GA,1W,1FPC", "Hull Reinforcement", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1PA,1Mo,1FCC", "Hull Reinforcement", "5", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1HCW", "Hull Reinforcement", "1", "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1Ni,1HDP", "Hull Reinforcement", "2", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1SAll,1V,1HE", "Hull Reinforcement", "3", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1GA,1W,1HV", "Hull Reinforcement", "4", "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1PA,1Mo,1PHR", "Hull Reinforcement", "5", "Selene Jean,Petra Olmanova" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", "Kill Warrant Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", "Kill Warrant Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", "Kill Warrant Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", "Kill Warrant Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Kill Warrant Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Kill Warrant Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Kill Warrant Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Kill Warrant Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", "Kill Warrant Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", "Kill Warrant Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", "Kill Warrant Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", "Kill Warrant Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Kill Warrant Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Kill Warrant Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Kill Warrant Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Kill Warrant Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Kill Warrant Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Kill Warrant Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Kill Warrant Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Kill Warrant Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", "Kill Warrant Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", "Kill Warrant Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", "Kill Warrant Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", "Kill Warrant Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", "Kill Warrant Scanner", "5", "Tiana Fortune" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Life Support", "1", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Life Support", "2", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Life Support", "3", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Life Support", "4", "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Life Support", "5", "Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Life Support", "1", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Life Support", "2", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Life Support", "3", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Life Support", "4", "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Life Support", "5", "Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Life Support", "1", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Life Support", "2", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Life Support", "3", "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Life Support", "4", "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Life Support", "5", "Etienne Dorn" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Mine", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Mine", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Mine", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Mine", "4", "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Mine", "5", "Juri Ishmaak" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Mine", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Mine", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Mine", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Mine", "4", "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Mine", "5", "Juri Ishmaak" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Mine", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Mine", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Mine", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Mine", "4", "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Mine", "5", "Juri Ishmaak" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Mine", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Mine", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Mine", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Mine", "4", "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Mine", "5", "Juri Ishmaak" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Missile", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Missile", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Missile", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Missile", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Missile", "5", "Liz Ryder" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Missile", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Missile", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Missile", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Missile", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Missile", "5", "Liz Ryder" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Missile", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Missile", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Missile", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Missile", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Missile", "5", "Liz Ryder" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Missile", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Missile", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Missile", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Missile", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Missile", "5", "Liz Ryder" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Multicannon", "1", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Multicannon", "2", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Multicannon", "3", "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Multicannon", "4", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Multicannon", "5", "Tod 'The Blaster' McQuinn,Marsha Hicks" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Plasma Accelerator", "5", "Bill Turner" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Plasma Accelerator", "1", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Plasma Accelerator", "2", "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Plasma Accelerator", "3", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Plasma Accelerator", "4", "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Plasma Accelerator", "5", "Bill Turner" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Point Defence", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Point Defence", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Point Defence", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Point Defence", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Point Defence", "5", "Ram Tah" ),
        new EngineeringRecipe("Point Defence Ammo Capacity", "misc_pointdefensecapacity", "1MS,1V,1Nb", "Point Defence", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Point Defence", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Point Defence", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Point Defence", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Point Defence", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Point Defence", "5", "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Point Defence", "1", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Point Defence", "2", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Point Defence", "3", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Point Defence", "4", "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Point Defence", "5", "Ram Tah" ),

        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1S", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1SLF,1Cr", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1SLF,1Cr,1HDC", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1MCF,1Se,1FPC", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1CIF,1FPC,1MSC", "Power Distributor", "5", "The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1SLF", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1SLF,1CP", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1GR,1MCF,1CHD", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1HC,1CIF,1CM", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1CIF,1CM,1EFC", "Power Distributor", "5", "The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1S", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1S,1CCo", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1ABSD,1Cr,1EA", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1USA,1Se,1PCa", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1CSD,1Cd,1MSC", "Power Distributor", "5", "The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1S", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1S,1CCo", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1ABSD,1Cr,1EA", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1USA,1Se,1PCa", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1CSD,1Cd,1MSC", "Power Distributor", "5", "The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1S", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1S,1CCo", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1ABSD,1HC,1Se", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1USA,1EA,1Cd", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1CSD,1PCa,1Te", "Power Distributor", "5", "The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1WSE", "Power Distributor", "1", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1C,1SHE", "Power Distributor", "2", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1C,1SHE,1HDC", "Power Distributor", "3", "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1V,1SS,1FPC", "Power Distributor", "4", "The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1W,1CoS,1FCC", "Power Distributor", "5", "The Dweller" ),

        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1WSE", "Power Plant", "1", "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1C,1SHE", "Power Plant", "2", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1C,1SHE,1HDC", "Power Plant", "3", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1V,1SS,1FPC", "Power Plant", "4", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1W,1CoS,1FCC", "Power Plant", "5", "Hera Tani,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1S", "Power Plant", "1", "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HCW,1CCo", "Power Plant", "2", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HCW,1CCo,1Se", "Power Plant", "3", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HDP,1CCe,1Cd", "Power Plant", "4", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1CCe,1CM,1Te", "Power Plant", "5", "Hera Tani,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe", "Power Plant", "1", "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe,1IED", "Power Plant", "2", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe,1IED,1HE", "Power Plant", "3", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Ge,1UED,1HV", "Power Plant", "4", "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Nb,1DED,1PHR", "Power Plant", "5", "Hera Tani,Etienne Dorn" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Prospecting Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Prospecting Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Prospecting Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Prospecting Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Prospecting Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Prospecting Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Prospecting Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Prospecting Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Prospecting Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Prospecting Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Prospecting Limpet", "1", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Prospecting Limpet", "2", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Prospecting Limpet", "3", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Prospecting Limpet", "4", "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Prospecting Limpet", "5", "The Sarge,Tiana Fortune,Marsha Hicks" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Pulse Laser", "1", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Pulse Laser", "2", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Pulse Laser", "3", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Pulse Laser", "4", "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Pulse Laser", "5", "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", "Rail Gun", "1", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", "Rail Gun", "2", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", "Rail Gun", "3", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", "Rail Gun", "4", "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", "Rail Gun", "5", "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Rail Gun", "1", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Rail Gun", "2", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Rail Gun", "3", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Rail Gun", "4", "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Rail Gun", "5", "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", "Rail Gun", "1", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", "Rail Gun", "2", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", "Rail Gun", "3", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", "Rail Gun", "4", "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", "Rail Gun", "5", "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", "Rail Gun", "1", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", "Rail Gun", "2", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", "Rail Gun", "3", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", "Rail Gun", "4", "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", "Rail Gun", "5", "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Rail Gun", "1", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Rail Gun", "2", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Rail Gun", "3", "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Rail Gun", "4", "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Rail Gun", "5", "Tod 'The Blaster' McQuinn" ),

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Refineries", "1", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Refineries", "2", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Refineries", "3", "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Refineries", "4", "Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Refineries", "5", "Marsha Hicks" ),

        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1P", "Sensor", "1", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1SAll,1Mn", "Sensor", "2", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1SAll,1Mn,1CCe", "Sensor", "3", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1CCo,1PA,1PLA", "Sensor", "4", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1CCe,1PLA,1PRA", "Sensor", "5", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", "Sensor", "1", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", "Sensor", "2", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", "Sensor", "3", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", "Sensor", "4", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", "Sensor", "5", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", "Sensor", "1", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", "Sensor", "2", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", "Sensor", "3", "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", "Sensor", "4", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", "Sensor", "5", "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),

        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe", "Shield Booster", "1", "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe,1CCo", "Shield Booster", "2", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe,1CCo,1FoC", "Shield Booster", "3", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Ge,1USS,1RFC", "Shield Booster", "4", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Nb,1ASPA,1EFC", "Shield Booster", "5", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1GR", "Shield Booster", "1", "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1DSCR,1HC", "Shield Booster", "2", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1DSCR,1HC,1Nb", "Shield Booster", "3", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1ISSA,1EA,1Sn", "Shield Booster", "4", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1USS,1PCa,1Sb", "Shield Booster", "5", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1Fe", "Shield Booster", "1", "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1GR,1Ge", "Shield Booster", "2", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1SAll,1HC,1FoC", "Shield Booster", "3", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1GA,1USS,1RFC", "Shield Booster", "4", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1PA,1ASPA,1EFC", "Shield Booster", "5", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P", "Shield Booster", "1", "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P,1CCo", "Shield Booster", "2", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P,1CCo,1FoC", "Shield Booster", "3", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1Mn,1CCe,1RFC", "Shield Booster", "4", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1CCe,1RFC,1IS", "Shield Booster", "5", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1Fe", "Shield Booster", "1", "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HCW,1Ge", "Shield Booster", "2", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HCW,1HDP,1FoC", "Shield Booster", "3", "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HDP,1USS,1RFC", "Shield Booster", "4", "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HE,1ASPA,1EFC", "Shield Booster", "5", "Didi Vatermann,Mel Brandon" ),

        new EngineeringRecipe("Rapid Charge Shield Cell Bank", "shieldcellbank_rapid", "1S", "Shield Cell Bank", "1", "Elvira Martuuk,Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge Shield Cell Bank", "shieldcellbank_rapid", "1GR,1Cr", "Shield Cell Bank", "2", "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge Shield Cell Bank", "shieldcellbank_rapid", "1S,1HC,1PAll", "Shield Cell Bank", "3", "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge Shield Cell Bank", "shieldcellbank_rapid", "1Cr,1EA,1ThA", "Shield Cell Bank", "4", "Mel Brandon" ),
        new EngineeringRecipe("Specialised Shield Cell Bank", "shieldcellbank_specialised", "1SLF", "Shield Cell Bank", "1", "Elvira Martuuk,Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised Shield Cell Bank", "shieldcellbank_specialised", "1SLF,1CCo", "Shield Cell Bank", "2", "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised Shield Cell Bank", "shieldcellbank_specialised", "1ESED,1CCo,1CIF", "Shield Cell Bank", "3", "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised Shield Cell Bank", "shieldcellbank_specialised", "1CCo,1CIF,1Y", "Shield Cell Bank", "4", "Mel Brandon" ),

        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR", "Shield Generator", "1", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR,1MCF", "Shield Generator", "2", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR,1MCF,1Se", "Shield Generator", "3", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1ISSA,1FoC,1Hg", "Shield Generator", "4", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1USS,1RFC,1Ru", "Shield Generator", "5", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR", "Shield Generator", "1", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR,1Ge", "Shield Generator", "2", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR,1Ge,1PAll", "Shield Generator", "3", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1ISSA,1Nb,1ThA", "Shield Generator", "4", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1USS,1Sn,1MGA", "Shield Generator", "5", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P", "Shield Generator", "1", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P,1CCo", "Shield Generator", "2", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P,1CCo,1MC", "Shield Generator", "3", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1Mn,1CCe,1CCom", "Shield Generator", "4", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1As,1CPo,1IC", "Shield Generator", "5", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR", "Shield Generator", "1", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR,1Ge", "Shield Generator", "2", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR,1Ge,1Se", "Shield Generator", "3", "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1ISSA,1FoC,1Hg", "Shield Generator", "4", "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1USS,1RFC,1Ru", "Shield Generator", "5", "Lei Cheung,Mel Brandon" ),

        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS", "Surface Scanner", "1", "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS,1Ge", "Surface Scanner", "2", "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS,1Ge,1PA", "Surface Scanner", "3", "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1ME,1Nb,1PLA", "Surface Scanner", "4", "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MC,1Sn,1PRA", "Surface Scanner", "5", "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Hera Tani" ),

        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", "Torpedo", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", "Torpedo", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", "Torpedo", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", "Torpedo", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", "Torpedo", "5", "Liz Ryder" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", "Torpedo", "1", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", "Torpedo", "2", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", "Torpedo", "3", "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", "Torpedo", "4", "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", "Torpedo", "5", "Liz Ryder" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", "Wake Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", "Wake Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", "Wake Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", "Wake Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", "Wake Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", "Wake Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", "Wake Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", "Wake Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", "Wake Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", "Wake Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", "Wake Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", "Wake Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", "Wake Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", "Wake Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", "Wake Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", "Wake Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", "Wake Scanner", "1", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", "Wake Scanner", "2", "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", "Wake Scanner", "3", "Bill Turner,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", "Wake Scanner", "5", "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", "Wake Scanner", "1", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", "Wake Scanner", "2", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", "Wake Scanner", "3", "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", "Wake Scanner", "4", "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", "Wake Scanner", "5", "Tiana Fortune" ),

            #endregion

            #region Tech broker - some of these have unknown IDs

            // inara, 10, 

            // bobblehead - Don't have thargoid heart in list. 
            new EngineeringRecipe("Corrosion Resistant Cargo Rack Size 4 Class 1","int_corrosionproofcargorack_size4_class1","Human","Cargo Rack","16MA,26Fe,18CM,22RB,12NFI"),
            new EngineeringRecipe("Detailed Surface Scanner (Engineered V1)","int_detailedsurfacescanner_tiny","Human","Surface Scanner", "22Ge,24Nb,28MC,26MS"),
            new EngineeringRecipe("Enzyme Missile Rack Fixed Medium","hpt_causticmissile_fixed_medium","Human","Missile","16UKEC,18UKOC,16Mo,15W,6RB"),
            new EngineeringRecipe("Engineered FSD V1 (Class 5)","int_hyperdrive_size5_class5","Human","FSD","18DWEx,26Te,26EA,28CP"),
            new EngineeringRecipe("Meta Alloy Hull Reinforcement Size 1 Class 1","int_metaalloyhullreinforcement_size1_class1","Human","Hull Reinforcement","16MA,15FoC,22ASPA,20CCom,12RMP"),
            new EngineeringRecipe("Mining Laser (Fixed Small)","?","Human","Mining Laser","16OSM,20As,24Re,28P"),
            new EngineeringRecipe("Flechette Launcher Fixed Medium","hpt_flechettelauncher_fixed_medium","Human","Flechette","30Fe,24Mo,22Re,26Ge,8CMMC"),
            new EngineeringRecipe("Flechette Launcher Turret Medium","hpt_flechettelauncher_turret_medium","Human","Flechette","28Fe,28Mo,20Re,24Ge,10AM"),
            new EngineeringRecipe("Engineered Seeker Missile Rack V1 (Fixed, Class 2)","?","Human","Missile","10OSM,16PRA,24CCe,26HC,28P"),

            // Inara, 9, right column, correct
            new EngineeringRecipe("Plasma Shock Cannon Fixed Large","hpt_plasmashockcannon_fixed_large","Human","Shock Cannon","28V,26W,24Re,26Tc,8PC"),
            new EngineeringRecipe("Plasma Shock Cannon Fixed Medium","hpt_plasmashockcannon_fixed_medium","Human","Shock Cannon","24V,26W,20Re,28Tc,6IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Fixed Small","hpt_plasmashockcannon_fixed_small","Human","Shock Cannon","8V,10W,8Re,12Tc,4PC"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Large","hpt_plasmashockcannon_turret_large","Human","Shock Cannon","26V,28W,22Re,24Tc,10IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Medium","hpt_plasmashockcannon_turret_medium","Human","Shock Cannon","24V,22W,20Re,28Tc,8PTB"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Small","hpt_plasmashockcannon_turret_small","Human","Shock Cannon","8V,12W,10Re,10Tc,4IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Large","hpt_plasmashockcannon_gimbal_large","Human","Shock Cannon","28V,24W,24Re,22Tc,12PTB"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Medium","hpt_plasmashockcannon_gimbal_medium","Human","Shock Cannon","24V,22W,20Re,28Tc,10PC"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Small","hpt_plasmashockcannon_gimbal_small","Human","Shock Cannon","10V,11W,8Re,10Tc,4PTB"),

            // Inara 9

            new EngineeringRecipe("Guardian FSD Booster Size 1","int_guardianfsdbooster_size1","Guardian","FSD","1GMBS,21GPCe,21GTC,24FoC,8HNSM"),
            new EngineeringRecipe("Guardian Hull Reinforcement Size 1 Class 1","int_guardianhullreinforcement_size1_class1","Guardian","Hull Reinforcement","1GMBS,21GSWC,16PBOD,16PGOD,12RMP"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 1","gdn_hybrid_fighter_v1","Guardian","Fighter","1GMVB,25GPCe,26PEOD,18PBOD,25GTC"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 2","gdn_hybrid_fighter_v2","Guardian","Fighter","1GMVB,25GPCe,26PEOD,18GSWC,25GTC"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 3","gdn_hybrid_fighter_v3","Guardian","Fighter","1GMVB,25GPCe,26PEOD,18GSWP,25GTC"),
            new EngineeringRecipe("Guardian Module Reinforcement Size 1 Class 1","int_guardianmodulereinforcement_size1_class1","Guardian","Module","1GMBS,18GSWC,15PEOD,20GPC,9RMP"),
            new EngineeringRecipe("Guardian Power Distributor Size 1","int_guardianpowerdistributor_size1","Guardian","Power Distributor","1GMBS,20PAOD,24GPCe,18PA,6HSI"),
            new EngineeringRecipe("Guardian Power Plant Size 2","int_guardianpowerplant_size2","Guardian","Power Plant","1GMBS,18GPC,21PEOD,15HRC,10EGA"),
            new EngineeringRecipe("Guardian Shield Reinforcement Size 1 Class 1","int_guardianshieldreinforcement_size1_class1","Guardian","Shield Generator","1GMBS,17GPCe,20GTC,24PDOD,8DIS"),

            // Inara 9 left
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Medium","hpt_guardian_gausscannon_fixed_medium", "Guardian Weapons","Gauss Cannon","1GWBS,18GPCe,20GTC,15Mn,6MEC"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Medium Modified","?","Guardian Weapons","Gauss Cannon","6TCU,18GPCe,20GTC,15Nb,1GWBS"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Small","hpt_guardian_gausscannon_fixed_small","Guardian Weapons","Gauss Cannon","1GWBS,12GPC,12GSWC,15GSWP"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Small Modified","?","Guardian Weapons","Gauss Cannon","12GPC,12GSWC,15GSWP,9Nb,1GWBS"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Large","hpt_guardian_plasmalauncher_fixed_large","Guardian Weapons","Plasma Accelerator","1GWBS,28GPC,20GSWP,28Cr,10MWCH"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Medium","hpt_guardian_plasmalauncher_fixed_medium","Guardian Weapons","Plasma Accelerator","1GWBS,18GPC,16GSWP,14Cr,8MWCH"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Small","hpt_guardian_plasmalauncher_fixed_small","Guardian Weapons","Plasma Accelerator","1GWBS,12GPCe,12GSWP,15GTC"),
            new EngineeringRecipe("Guardian Plasma Launcher Turret Large","hpt_guardian_plasmalauncher_turret_large","Guardian Weapons","Plasma Accelerator","2GWBS,20GPC,24GSWP,26Cr,10AM"),
            new EngineeringRecipe("Guardian Plasma Launcher Turret Medium","hpt_guardian_plasmalauncher_turret_medium","Guardian Weapons","Plasma Accelerator","2GWBS,21GPC,20GSWP,16Cr,8AM"),

            // Inara 9 right
            new EngineeringRecipe("Guardian Plasma Launcher Turret Small","hpt_guardian_plasmalauncher_turret_small","Guardian Weapons","Plasma Accelerator","1GWBS,12GPCe,12GTC,15GSWP"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Large","hpt_guardian_shardcannon_fixed_large","Guardian Weapons","Shard Cannon","1GWBS,20GSWC,28GTC,20C,18MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Medium Modified","?","Guardian Weapons","Shard Cannon","12PC,20GSWC,18GTC,14Ge,1GWBS"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Medium","hpt_guardian_shardcannon_fixed_medium","Guardian Weapons","Shard Cannon","1GWBS,20GSWC,18GTC,14C,12PTB"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Small Modified","?","Guardian Weapons","Shard Cannon","12GPC,12GSWC,15GSWP,6Ge,1GWBS"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Small","hpt_guardian_shardcannon_fixed_small","Guardian Weapons","Shard Cannon","1GWBS,12GPC,12GTC,15GSWP"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Large","hpt_guardian_shardcannon_turret_large","Guardian Weapons","Shard Cannon","2GWBS,20GSWC,26GTC,28C,12MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Medium","hpt_guardian_shardcannon_turret_medium","Guardian Weapons","Shard Cannon","2GWBS,16GSWC,20GTC,15C,12MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Small","hpt_guardian_shardcannon_turret_small","Guardian Weapons","Shard Cannon","1GWBS,12GPC,15GTC,12GSWP"),
            #endregion

            #region Special Effects

            new EngineeringRecipe("Angled Plating", "Armour", "5CC,3HDC,3Zr"),
            new EngineeringRecipe("Angled Plating", "Hull Reinforcement", "5TeA,3Zr,5C,3HDC"),
            new EngineeringRecipe("Auto Loader", "Cannon,Multicannon", "4ME,3MC,3HDC"),
            new EngineeringRecipe("Blast Block", "Shield Booster", "5ISSA,3HRC,3HDP,2Se"),
            new EngineeringRecipe("Boss Cells", "Shield Cell", "5CSU,3Cr,1PCa"),
            new EngineeringRecipe("Cluster Capacitors", "Power Distributor", "5P,3HRC,1Cd"),
            new EngineeringRecipe("Concordant Sequence", "Pulse Laser,Burst Laser,Beam Laser", "5FoC,3EFW,1Zr"),
            new EngineeringRecipe("Corrosive Shell", "Multicannon,Frag Cannon", "5CSU,4PAll,3As"),
            new EngineeringRecipe("Dazzle Shell", "Plasma Accelerator,Frag Cannon", "5MS,4Mn,5HC,5MS"),
            new EngineeringRecipe("Deep Charge", "FSD", "5ADWE,3GA,1EHT"),
            new EngineeringRecipe("Deep Plating", "Armour", "5CC,3ME,2Mo"),
            new EngineeringRecipe("Deep Plating", "Hull Reinforcement", "5CC,3Mo,2Ru"),
            new EngineeringRecipe("Dispersal Field", "Plasma Accelerator,Cannon", "5CCo,5HC,5IED,5WSE"),
            new EngineeringRecipe("Double Braced", "Weapon", "5MS,5CC,3V"),
            new EngineeringRecipe("Double Braced", "FSD", "5ADWE,3GA,1CCom"),
            new EngineeringRecipe("Double Braced", "Power Plant", "5GR,3V,1FPC"),
            new EngineeringRecipe("Double Braced", "Shield Cell", "5CSU,3Cr,1Y"),
            new EngineeringRecipe("Double Braced", "Engine", "5Fe,3HC,1FPC"),
            new EngineeringRecipe("Double Braced", "Shield Generator", "5WSE,3FFC,1CCom"),
            new EngineeringRecipe("Double Braced", "Power Distributor", "5P,3HRC,1FPC"),
            new EngineeringRecipe("Double Braced", "Shield Booster", "5DSCR,3GA,3SHE"),
            new EngineeringRecipe("Drag Drives", "Engine", "5Fe,3HC,1SFP"),
            new EngineeringRecipe("Drag Munitions", "Frag Cannon,Seeker Missile", "5C,5GR,2Mo"),
            new EngineeringRecipe("Drive Distributors", "Engine", "5Fe,3HC,1SFP"),
            new EngineeringRecipe("Emissive Munitions", "Pulse Laser,Multicannon,Seeker Missile,Dumb Missile,Mine", "4ME,3UED,3HE,3Mn"),
            new EngineeringRecipe("Fast Charge", "Shield Generator", "5WSE,3FFC,1CoS"),
            new EngineeringRecipe("Feedback Cascade", "Rail Gun", "5OSK,5SHE,5FiC"),
            new EngineeringRecipe("Flow Control", "Power Distributor", "5P,3HRC,1CPo"),
            new EngineeringRecipe("Flow Control", "Weapon", "5MS,3HC,1EFW"),
            new EngineeringRecipe("Flow Control", "Shield Booster", "5ISSA,3SFP,3FoC,3Nb"),
            new EngineeringRecipe("Flow Control", "Shield Cell", "5CSU,3Cr,1CPo"),
            new EngineeringRecipe("Force Block", "Shield Booster", "5USA,3SS,2ASPA"),
            new EngineeringRecipe("Force Block", "Shield Generator", "5WSE,3FFC,1DED"),
            new EngineeringRecipe("Force Shell", "Cannon", "5MS,5Zn,3PA,3HCW"),
            new EngineeringRecipe("FSD Interrupt", "Dumb Missile", "3SWS,5AFT,5ME,3CCom"),
            new EngineeringRecipe("Hi-Cap", "Shield Generator", "5WSE,3FFC,1CPo"),
            new EngineeringRecipe("High Yield Shell", "Cannon", "5MS,3PLA,3CM,5Ni"),
            new EngineeringRecipe("Incendiary Rounds", "Multicannon,Frag Cannon", "5HCW,5P,5S,3PA"),
            new EngineeringRecipe("Inertial Impact", "Burst Laser", "5FFC,5DSCR,5ADWE"),
            new EngineeringRecipe("Ion Disruption", "Mine", "5S,5P,3CHD,3EA"),
            new EngineeringRecipe("Layered Plating", "Armour", "5HCW,3HDC,1Nb"),
            new EngineeringRecipe("Layered Plating", "Hull Reinforcement", "5HCW,3SS,3W"),
            new EngineeringRecipe("Lo-draw", "Shield Generator", "5WSE,3FFC,1CPo"),
            new EngineeringRecipe("Mass Lock Munition", "Torpedo", "5ME,3HDC,3ASPA"),
            new EngineeringRecipe("Mass Manager", "FSD", "5ADWE,3GA,1EHT"),
            new EngineeringRecipe("Monstered", "Power Plant", "5GR,3V,1PCa"),
            new EngineeringRecipe("Multi-servos", "Pulse Laser,Burst Laser,Cannon,Multicannon,Plasma Accelerator,Rail Gun,Frag Cannon,Missile", "5MS,4FoC,2CPo,2CCom"),
            new EngineeringRecipe("Multi-weave", "Shield Generator", "5WSE,3FFC,1ASPA"),
            new EngineeringRecipe("Overload Munitions", "Seeker Missile,Dumb Missile,Mine", "5FiC,4TEC,2ASPA,3Ge"),
            new EngineeringRecipe("Oversized", "Weapon", "5MS,3MC,1Ru"),
            new EngineeringRecipe("Penetrator Munitions", "Dumb Missile", "5GA,3EA,3Zr"),
            new EngineeringRecipe("Penetrator Payload", "Torpedo", "3MC,3W,5ABSD,3Se"),
            new EngineeringRecipe("Phasing Sequence", "Pulse Laser,Burst Laser,Plasma Accelerator", "5FoC,3ASPA,3Nb,3CCom"),
            new EngineeringRecipe("Plasma Slug", "Plasma Accelerator", "3HE,2EFW,2RFC,4Hg"),
            new EngineeringRecipe("Plasma Slug", "Rail Gun", "3HE,2EFW,2RFC,4Hg"),
            new EngineeringRecipe("Radiant Canister", "Mine", "1Po,3PA,4HDP"),
            new EngineeringRecipe("Recycling Cell", "Shield Cell", "5CSU,3Cr,1CCom"),
            new EngineeringRecipe("Reflective Plating", "Armour", "5CC,3HDP,2ThA"),
            new EngineeringRecipe("Reflective Plating", "Hull Reinforcement", "5HCW,3HDP,1PLA,4Zn"),
            new EngineeringRecipe("Regeneration Sequence", "Beam Laser", "3RFC,4SS,1PSFD"),
            new EngineeringRecipe("Reverberating Cascade", "Torpedo,Mine", "2CCom,3CSD,4FiC,4Cr"),
            new EngineeringRecipe("Scramble Spectrum", "Pulse Laser,Burst Laser", "5CS,3USS,5ESED"),
            new EngineeringRecipe("Screening Shell", "Frag Cannon", "5MS,5DSCR,5MCF,3Nb"),
            new EngineeringRecipe("Shift-lock Canister", "Mine", "5TeA,3SWS,5SAll"),
            new EngineeringRecipe("Smart Rounds", "Cannon,Multicannon", "5MS,3SFP,3DED,3CSD"),
            new EngineeringRecipe("Stripped Down", "Engine", "5Fe,3HC,1PLA"),
            new EngineeringRecipe("Stripped Down", "Power Distributor", "5P,3HRC,1PLA"),
            new EngineeringRecipe("Stripped Down", "Weapon", "5SAll,5C,1Sn"),
            new EngineeringRecipe("Stripped Down", "Shield Cell", "5CSU,3Cr,1PLA"),
            new EngineeringRecipe("Stripped Down", "FSD", "5ADWE,3GA,1PLA"),
            new EngineeringRecipe("Stripped Down", "Power Plant", "5GR,3V,1PLA"),
            new EngineeringRecipe("Stripped Down", "Shield Generator", "5WSE,3FFC,1PLA"),
            new EngineeringRecipe("Super Capacitors", "Shield Booster", "3USS,5CC,2Cd"),
            new EngineeringRecipe("Super Conduits", "Power Distributor", "5P,3HRC,1SFP"),
            new EngineeringRecipe("Super Penetrator", "Rail Gun", "3PLA,3RFC,3Zr,5USS"),
            new EngineeringRecipe("Target Lock Breaker", "Plasma Accelerator", "5Se,3SFP,1AEC"),
            new EngineeringRecipe("Thermal Cascade", "Cannon,Seeker Missile,Dumb Missile", "5HCW,4HC,3HDC,5P"),
            new EngineeringRecipe("Thermal Conduit", "Beam Laser,Plasma Accelerator", "5HDP,5S,5TeA"),
            new EngineeringRecipe("Thermal Shock", "Pulse Laser,Burst Laser,Beam Laser,Multicannon", "5FFC,3HRC,3CCo,3W"),
            new EngineeringRecipe("Thermal Spread", "Power Plant", "5GR,3V,1HV"),
            new EngineeringRecipe("Thermal Spread", "FSD", "5ADWE,3GA,1HV,3GR"),
            new EngineeringRecipe("Thermal Spread", "Engine", "5Fe,3HC,1HV"),
            new EngineeringRecipe("Thermal Vent", "Beam Laser", "5FFC,3CPo,3PAll"),
            new EngineeringRecipe("Thermo Block", "Shield Generator", "5WSE,3FFC,1HV"),
            new EngineeringRecipe("Thermo Block", "Shield Booster", "5ABSD,3CCe,3HV"),

            #endregion

            #region suit/weapon levels

            new EngineeringRecipe("Suit","Remlok",2,"1MRISuitSch,1MRIHM,1MRIPR,1MRDMI,5MRCCFP,5MRCG"),
            new EngineeringRecipe("Suit","Remlok",3,"5MRISuitSch,5MRIHM,5MRIPR,5MRDMI,15MRCCFP,15MRCG"),
            new EngineeringRecipe("Suit","Remlok",4,"10MRISuitSch,10MRIHM,10MRIPR,10MRDMI,25MRCCFP,25MRCG"),
            new EngineeringRecipe("Suit","Remlok",5,"15MRISuitSch,15MRIHM,15MRIPR,15MRDMI,35MRCCFP,35MRCG"),
            new EngineeringRecipe("Suit","Supratech",2,"1MRISuitSch,1MRIHM,1MRIPR,1MRDMI,5MRCA,5MRCG"),
            new EngineeringRecipe("Suit","Supratech",3,"5MRISuitSch,5MRIHM,5MRIPR,5MRDMI,15MRCA,15MRCG"),
            new EngineeringRecipe("Suit","Supratech",4,"10MRISuitSch,10MRIHM,10MRIPR,10MRDMI,25MRCA,25MRCG"),
            new EngineeringRecipe("Suit","Supratech",5,"15MRISuitSch,15MRIHM,15MRIPR,15MRDMI,35MRCA,35MRCG"),
            new EngineeringRecipe("Suit","Manticore",2,"1MRISuitSch,1MRIHM,1MRIPR,1MRDMI,5MRCTP,5MRCG"),
            new EngineeringRecipe("Suit","Manticore",3,"5MRISuitSch,5MRIHM,5MRIPR,5MRDMI,15MRCTP,15MRCG"),
            new EngineeringRecipe("Suit","Manticore",4,"10MRISuitSch,10MRIHM,10MRIPR,10MRDMI,25MRCTP,25MRCG"),
            new EngineeringRecipe("Suit","Manticore",5,"15MRISuitSch,15MRIHM,15MRIPR,15MRDMI,35MRCTP,35MRCG"),
            new EngineeringRecipe("Weapon","Kinematic Armaments",2,"1MRIWS,1MRICLG"),
            new EngineeringRecipe("Weapon","Kinematic Armaments",3,"5MRIWS,5MRICLG"),
            new EngineeringRecipe("Weapon","Kinematic Armaments",4,"10MRIWS,10MRICLG"),
            new EngineeringRecipe("Weapon","Kinematic Armaments",5,"15MRIWS,15MRICLG"),
            new EngineeringRecipe("Weapon","Manticore",2,"1MRIWS,1MRIIG"),
            new EngineeringRecipe("Weapon","Manticore",3,"5MRIWS,5MRIIG"),
            new EngineeringRecipe("Weapon","Manticore",4,"10MRIWS,10MRIIG"),
            new EngineeringRecipe("Weapon","Manticore",5,"15MRIWS,15MRIIG"),
            new EngineeringRecipe("Weapon","Takada",2,"1MRIWS,1MRIIG"),
            new EngineeringRecipe("Weapon","Takada",3,"5MRIWS,5MRIIG"),
            new EngineeringRecipe("Weapon","Takada",4,"10MRIWS,10MRIIG"),
            new EngineeringRecipe("Weapon","Takada",5,"15MRIWS,15MRIIG"),

            #endregion

            #region Engineer mods - again from frontier data, excepting engineer list manually added.

            new EngineeringRecipe("Suit","suit_reducedtoolbatteryconsumption","All","Reduced Tool Battery Consumption",500000,"5MRCEF,10MRCMICTRANS,15MRCEW,10MRDROR","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe("Suit","suit_increasedbatterycapacity","All","Improved Battery Capacity",750000,"5MRCIB,10MRCMS,10MRCEW,10MRDROR,15MRDML","Wellington Beck,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe("Suit","suit_increasedshieldregen","All","Faster Shield Regen",750000,"5MRCIB,15MRCMICTRANS,15MRCEW,10MRDROR","Kit Fowler,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe("Suit","suit_improvedarmourrating","All","Damage Resistance",750000,"5MRCTP,5MRCCFP,15MRCEA,10MRDWI,10MRDBALD","Jude Navarro,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe("Suit","suit_increasedo2capacity","All","Increased Air Reserves",750000,"10MRCOXYBAC,15MRCPHN,5MRDPP,15MRDAQR","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe("Suit","suit_nightvision","All","Night Vision",1000000,"10MRISE,10MRCCIRSWITCH,5MRDSL,5MRDRD,5MRDNOCD","Oden Geiger,Yi Shen"),
            new EngineeringRecipe("Suit","suit_improvedradar","All","Enhanced Tracking",750000,"5MRCTX,5MRCCB,10MRDTS,10MRDSAL,10MRDSAD","Domino Green,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe("Suit","suit_backpackcapacity","All","Extra Backpack Capacity",750000,"10MRCEA,5MRCMC,10MRDWI,10MRDCI,10MRDDD","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe("Suit","suit_increasedammoreserves","All","Extra Ammo Capacity",750000,"5MRCWC,15MRDRL,10MRWTD,10MRDPRODREP","Kit Fowler,Jude Navarro,Eleanor Bresa"),
            new EngineeringRecipe("Suit","suit_improvedjumpassist","All","Improved Jump Assist",750000,"10MRIGM,5MRCMT,10MRCMOTOR,10MRDTS","Yarden Bond,Hero Ferrari,Baltanos"),
            new EngineeringRecipe("Suit","suit_increasedsprintduration","All","Increased Sprint Duration",750000,"10MRCOXYBAC,15MRCCC,5MRDTDR,5MRDGSD,5MRDMR","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe("Suit","suit_adsmovementspeed","All","Combat Movement Speed",750000,"10MRCEPINE,15MRCPHN,10MRDEP,5MRDGR","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe("Suit","suit_quieterfootsteps","All","Quieter Footsteps",1000000,"5MRCMH,15MRCVP,5MRDSAP,10MRDTACP,10MRDPATR","Yarden Bond,Yi Shen"),
            new EngineeringRecipe("Suit","suit_increasedmeleedamage","All","Added Melee Damage",500000,"10MRCEPINE,15MRCMT,10MRDCTM,10MRDCOMBPERF","Kit Fowler,Jude Navarro,Eleanor Bresa"),

            new EngineeringRecipe("Weapon","weapon_suppression_pressurised","All","Noise Suppressor",1000000,"15MRCVP,5MRCWC,10MRDAD,10MRDMA","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe("Weapon","weapon_suppression_unpressurised","All","Audio Masking",1000000,"10MRCS,15MRCTX,5MRCCB,5MRDAL,10MRDPATR","Yarden Bond,Yi Shen"),
            new EngineeringRecipe("Weapon","weapon_stability","All","Stability",500000,"10MRCVP,10MRCMH,10MRDMA,15MRDRA","Domino Green,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe("Weapon","weapon_handling","All","Faster Handling",500000,"5MRCVP,10MRDOM,10MRDCOMBPERF,10MRDCTM","Yarden Bond,Hero Ferrari,Baltanos"),
            new EngineeringRecipe("Weapon","weapon_reloadspeed","All","Reload Speed",500000,"10MRCMH,10MRCE,10MRDOM,10MRDPRODREP,10MRDCTM","Jude Navarro,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe("Weapon","weapon_clipsize","All","Magazine Size",750000,"5MRCWC,5MRCTC,10MRCMETALCOIL,10MRWTD,5MRSECEXP","Kit Fowler,Jude Navarro,Eleanor Bresa"),
            new EngineeringRecipe("Weapon","weapon_scope","All","Scope",500000,"10MRCOL,5MRCOF,10MRDSAD,5MRDBIOD","Wellington Beck,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe("Weapon","weapon_backpackreloading","All","Stowed reloading",1000000,"5MRCCB,15MRCEMC,10MRDDD,10MRDOM,10MRDPS","Kit Fowler,Uma Laszlo,Eleanor Bresa"),

            new EngineeringRecipe("Weapon","weapon_accuracy","Kinematic Armaments","Higher Accuracy",500000,"10MRCVP,10MRCR,10MRDEYD,5MRDBIOD,10MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe("Weapon","weapon_range","Kinematic Armaments","Greater Range",500000,"10MRCMETALCOIL,10MRCR,5MRCWC,10MRDBALD,10MRDTS","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe("Weapon","weapon_headshotdamage","Kinematic Armaments","Headshot damage",500000,"10MRCCC,15MRCR,5MRCWC,10MRWTD,5MRDMR","Uma Laszlo,Yi Shen"),

            new EngineeringRecipe("Weapon","weapon_accuracy","Manticore","Higher Accuracy",500000,"10MRCCC,10MRCE,10MRCMETALCOIL,5MRDCHEMPAT,10MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe("Weapon","weapon_range","Manticore","Greater Range",500000,"5MRCMOTOR,10MRCE,5MRCEF,10MRDCF,15MRDMS","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe("Weapon","weapon_headshotdamage","Manticore","Headshot damage",500000,"10MRCIB,10MRCE,15MRCMS,10MRDCED,5MRDBTR","Uma Laszlo,Yi Shen"),

            new EngineeringRecipe("Weapon","weapon_accuracy","Takada","Higher Accuracy",500000,"10MRCMETALCOIL,5MRCOL,15MRCEW,5MRDRD,10MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe("Weapon","weapon_range","Takada","Greater Range",500000,"15MRCMICTRANS,5MRCOL,5MRCCB,10MRDSAL,15MRDRA","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe("Weapon","weapon_headshotdamage","Takada","Headshot damage",750000,"10MRCIB,5MRCOL,10MRCS,10MRDSAD,5MRDBIOD","Uma Laszlo,Yi Shen"),

            #endregion



        };

        public static Dictionary<MaterialCommodityMicroResourceType, List<EngineeringRecipe>> EngineeringRecipesByMaterial =
            EngineeringRecipes.SelectMany(r => r.Ingredients.Select(i => new { mat = i, recipe = r }))
                              .GroupBy(a => a.mat)
                              .ToDictionary(g => g.Key, g => g.Select(a => a.recipe).ToList());

    }
}