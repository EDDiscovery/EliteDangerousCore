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
    public static class Recipes
    {
        public class Recipe
        {
            public string Name { get; private set; }        // name of receipe ie "Lightweight Hull Reinforcement"
            public MaterialCommodityMicroResourceType[] Ingredients { get; private set; }
            public int[] Amount { get; private set; }
            public int Count { get { return Ingredients.Length; } }
            public int Amounts { get { return Amount.Sum(); } }     // number of items

            public Recipe(string name, string ingredientsstring)
            {
                Name = name;
                if (ingredientsstring.HasChars())
                {
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

        }

        public class SynthesisRecipe : Recipe
        {
            public SynthesisRecipeFDName FDName { get; private set; }        // "Repair Basic", "Fuel Basic"
            public enum SynthesisLevel { Basic, Standard, Premium }
            public SynthesisLevel Level { get; set; }

            public SynthesisRecipe(string name, string fdname, SynthesisLevel level, string indg)
                : base(name, indg)
            {
                Level = level;
                FDName = new SynthesisRecipeFDName(fdname ?? "Unknown");
            }
        }


        [System.Diagnostics.DebuggerDisplay("Rec {FDName} {Level} {ModuleList} {string.Join(\",\",Engineers)}")]
        public class EngineeringRecipe : Recipe
        {
            public int? Level { get; set; } = null;
            public ItemData.ShipModule.ModuleTypes[] ModuleType { get; private set; }
            public string[] Engineers { get ; private set; }
            public EngineeringRecipeFDName FDName { get; private set; }       // only certain types have a fdname, others are null
            public int MercCoins { get; set; }              // future use
            public string LevelAsString => Level?.ToString() ?? "NA";
            public string ModuleListSplitCaps => string.Join(",", ModuleType.Select(x => x.ToString().SplitCapsWordFull()));
            public string ModuleList=> string.Join(",", ModuleType.Select(x => x.ToString()));

            public EngineeringRecipe(string name, string fdname, string ingredientlist, ItemData.ShipModule.ModuleTypes moduletype, int lvl, string engnrs, int merccoins = 0)   // normal recipes
                : base(name, ingredientlist)
            {
                this.FDName = new EngineeringRecipeFDName(fdname);
                Level= lvl;
                ModuleType = new ItemData.ShipModule.ModuleTypes[] { moduletype };
                Engineers = engnrs.Split(',');
                MercCoins = merccoins;
            }

            public EngineeringRecipe(string name, string fdname, string type, ItemData.ShipModule.ModuleTypes moduletype, string ingredientlist)        // for tech broker
                : base(name, ingredientlist)
            {
                this.FDName = new EngineeringRecipeFDName(fdname);
                ModuleType = new ItemData.ShipModule.ModuleTypes[] { moduletype };
                Engineers = type.Split(',');
            }

            public EngineeringRecipe(string n, string fdname, string moduletypelist, string ingredientlist)        // for special effects
                : base(n, ingredientlist)
            {
                this.FDName = new EngineeringRecipeFDName(fdname);
                string[] modlist = moduletypelist.Split(",");
                ModuleType = new ItemData.ShipModule.ModuleTypes[modlist.Length];
                for (int i = 0; i < modlist.Length; i++)
                {
                    var t = Enum.TryParse<ItemData.ShipModule.ModuleTypes>(modlist[i], true, out ItemData.ShipModule.ModuleTypes res) ? res : ItemData.ShipModule.ModuleTypes.UnknownType;
                    System.Diagnostics.Debug.Assert(t != ItemData.ShipModule.ModuleTypes.UnknownType);
                    ModuleType[i] = t;
                }
                Engineers = new string[] { "Special Effect" };
            }

            public EngineeringRecipe(string n, string fdname, ItemData.ShipModule.ModuleTypes moduletype, string ingredientlist)        // for special effects
                : base(n, ingredientlist)
            {
                this.FDName = new EngineeringRecipeFDName(fdname);
                ModuleType = new ItemData.ShipModule.ModuleTypes[] { moduletype };
                Engineers = new string[] { "Special Effect" };
            }

            public EngineeringRecipe(ItemData.ShipModule.ModuleTypes moduletype, string manu, int lvl, string ingredientlist)        // for suit/weapon upgrades
                : base(manu, ingredientlist)
            {
                Level = lvl;
                ModuleType = new ItemData.ShipModule.ModuleTypes[] { moduletype };
                Engineers = new string[] { moduletype.ToString() };
            }

            public EngineeringRecipe(ItemData.ShipModule.ModuleTypes moduletype, string fdname, string manu, string name, int cost, string ingredientlist, string eng)        // for suit/weapon engineer mods
                : base(name + (manu != "All" ? (": " + manu) : ""), ingredientlist)
            {
                ModuleType = new ItemData.ShipModule.ModuleTypes[] { moduletype };
                this.FDName = new EngineeringRecipeFDName(fdname);
                Engineers = eng.Split(',');
            }
        }

        // always returns a string, may be empty
        public static string UsedInRecipesByFDName(MCFDName fdname, string join = ", ")
        {
            string s = Recipes.UsedInEngineeringByFDName(fdname, join);
            s = s.AppendPrePad(Recipes.UsedInSythesisByFDName(fdname, join), join);
            return s;
        }

        // always returns a string, may be empty
        public static string UsedInSythesisByFDName(MCFDName fdname, string join = ", ")
        {
            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);
            if (mc != null && SynthesisRecipesByMaterial.ContainsKey(mc))
            {
                string str = string.Join(join, SynthesisRecipesByMaterial[mc].Select(x => x.Name + "-" + x.Level + ": " + x.IngredientsStringLong));
                return str;
            }
            else
                return "";
        }

        // always returns a string, may be empty
        public static string UsedInEngineeringByFDName(MCFDName fdname, string join = ", ")
        {
            MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(fdname);
            if (mc != null && EngineeringRecipesByMaterial.ContainsKey(mc))
            {
                string str = String.Join(join, EngineeringRecipesByMaterial[mc].Select(x => x.ModuleListSplitCaps + " " + x.Name + (x.LevelAsString!="NA" ? ("-" + x.LevelAsString):"") + ": " + x.IngredientsStringLong + " @ " + string.Join(",", x.Engineers)));
                return str;
            }
            else
                return "";
        }

        public static string GetBetterNameForEngineeringRecipe(EngineeringRecipeFDName fdname)
        {
            var f = EngineeringRecipes.Find(x => x.FDName != null && x.FDName == fdname);
            if (f == null)
                return fdname.SplitCapsWordFull();
            else
                return f.Name;
        }

        public static SynthesisRecipe FindSynthesis(SynthesisRecipeFDName recipename)
        {
            return SynthesisRecipes.Find(x => x.FDName.Equals(recipename));
        }
        public static SynthesisRecipe FindSynthesis(SynthesisRecipeFDName recipename, SynthesisRecipe.SynthesisLevel level)
        {
            return SynthesisRecipes.Find(x => x.FDName.Equals(recipename) && x.Level.Equals(level));
        }

        public static List<SynthesisRecipe> FindSynthesis(Dictionary<MCFDName,int> mats)
        {
            List<SynthesisRecipe> ret = new List<SynthesisRecipe>();

            foreach( var s in SynthesisRecipes)
            {
                if (s.Ingredients.Length == mats.Count)
                {
                    bool matched = true;
                    for (int i = 0; i < s.Ingredients.Length; i++)
                    {
                        if (mats.TryGetValue(s.Ingredients[i].FDName, out int v) == false || v != s.Amount[i])
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        ret.Add(s);
                    }
                }
            }
            return ret;
        }


        // find by fdname, return null if not found
        public static EngineeringRecipe FindEngineering(EngineeringRecipeFDName fdname)
        {
            lock (FDNameLookup)         // in case multithreaded
            {
                if ( FDNameLookup.Count == 0 )                  // if not filled, auto fill it
                {
                    for( int i = 0; i < EngineeringRecipes.Count; i++ )
                    {
                        var x = EngineeringRecipes[i];
                        if ( x.FDName!=null)
                        {
                            if (!FDNameLookup.ContainsKey(x.FDName))        // add first instance of name
                            {
                                FDNameLookup[x.FDName] = i;
                                // System.Diagnostics.Debug.WriteLine($"Add {x.FDName.Str()} {i}");
                            }

                            int underscore = x.FDName.Str().LastIndexOf('_');

                            if (x.ModuleType.Length > 0 && underscore > 0)
                            {
                                string first = x.FDName.Str().Substring(0, underscore);
                                string last = x.FDName.Str().Substring(underscore);
                                string modname = x.ModuleType[0].ToString();

                                EngineeringRecipeFDName composite = new EngineeringRecipeFDName(first + "_" + modname + last);            // Sensor_sensor_lightweight
                                if (!FDNameLookup.ContainsKey(composite))        // add first instance of name
                                {
                                    FDNameLookup[composite] = i;        // also this added
                                    //System.Diagnostics.Debug.WriteLine($"Add1 composite {composite.Str()} {i}");
                                }

                                if (modname == "AutoFieldMaintenanceUnit")      // we have different names so they can be read better
                                    modname = "AFM";
                                else if (modname == "CollectorLimpetController")
                                    modname = "CollectionLimpet";

                                composite = new EngineeringRecipeFDName(modname + last);            // Heatsinklauncher_lightweight , AFM_lightweight
                                if (!FDNameLookup.ContainsKey(composite))        // add first instance of name
                                {
                                    FDNameLookup[composite] = i;        // also this added
                                    //System.Diagnostics.Debug.WriteLine($"Add2 composite {composite.Str()} {i}");
                                }
                            }
                        }
                    }
                }

                if (FDNameLookup.TryGetValue(fdname, out int rpi))
                    return EngineeringRecipes[rpi];
                else
                {
                    return null;
                }
            }
        }

        private static List<SynthesisRecipe> SynthesisRecipes = new List<SynthesisRecipe>()
        {            
            // inara https://inara.cz/elite/synthesis
            
            new SynthesisRecipe( "AFM Refill", "Module Repair Basic", SynthesisRecipe.SynthesisLevel.Basic,"3V,2Ni,2Cr,2Zn" ),
            new SynthesisRecipe( "AFM Refill", "Module Repair Standard", SynthesisRecipe.SynthesisLevel.Standard,"6V,2Mn,1Mo,1Zr,1Sn" ),
            new SynthesisRecipe( "AFM Refill", "Module Repair Premium", SynthesisRecipe.SynthesisLevel.Premium,"6V,4Cr,2Zn,2Zr,1Te,1Ru" ),

            new SynthesisRecipe( "AX Explosive Munitions", "AX Dumbfire Ammo Basic",SynthesisRecipe.SynthesisLevel.Basic, "3Fe,3Ni,4C,3PE" ),
            new SynthesisRecipe( "AX Explosive Munitions", "AX Dumbfire Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "6S,6P,2Hg,4UKOC,4PE" ),
            new SynthesisRecipe( "AX Explosive Munitions", "AX Dumbfire Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "5W,4Hg,2Po,5BMC,5PE,6SFD" ),

            new SynthesisRecipe( "AX Remote Flak Munitions", "AX Remote Flak Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "4Ni,3C,2S" ),
            new SynthesisRecipe( "AX Remote Flak Munitions", "AX Remote Flak Ammo Standard",SynthesisRecipe.SynthesisLevel.Standard, "2Sn,3Zn,1As,3UKTC,2TGWC" ),
            new SynthesisRecipe( "AX Remote Flak Munitions", "AX Remote Flak Ammo Premium",SynthesisRecipe.SynthesisLevel.Premium, "8Zn,2W,1As,3UES,4UKTC,1WP" ),

            new SynthesisRecipe( "AX Small Calibre Munitions", "AX Multi-cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "2Fe,1Ni,2S,2WP" ),
            new SynthesisRecipe( "AX Small Calibre Munitions", "AX Multi-cannon Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "2Fe,2P,2Zr,3UES,4WP" ),
            new SynthesisRecipe( "AX Small Calibre Munitions", "AX Multi-cannon Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "3Fe,2P,2Zr,4UES,2UKCP,6WP" ),

            new SynthesisRecipe( "Caustic Sinks", "Caustic Sink Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "1CSU, 1GA, 4CASH, 2COMEC" ),

            new SynthesisRecipe( "Chaff", "Chaff Premium", SynthesisRecipe.SynthesisLevel.Premium, "1CC,2FiC,1ThA,1PRA"),
            new SynthesisRecipe( "Chaff", "Chaff Standard",SynthesisRecipe.SynthesisLevel.Standard, "1CC,2FiC,1ThA"),
            new SynthesisRecipe( "Chaff", "Chaff Basic",SynthesisRecipe.SynthesisLevel.Basic, "1CC,1FiC"),

            new SynthesisRecipe( "Configurable Explosive Munitions", "AXDAM Missile Ammo Basic" , SynthesisRecipe.SynthesisLevel.Basic, "3Fe,3Ni,4C,4S" ),
            new SynthesisRecipe( "Configurable Explosive Munitions", "AXDAM Missile Ammo Standard" , SynthesisRecipe.SynthesisLevel.Standard, "6P,4As,2Hg,1GPCe,1GPC,1GTC" ),

            new SynthesisRecipe( "Configurable Small Calibre Munitions", "AXDAM Multi-cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "2Fe,1Ni,2S"),
            new SynthesisRecipe( "Configurable Small Calibre Munitions", "AXDAM Multi-cannon Ammo Standard",SynthesisRecipe.SynthesisLevel.Standard, "2Sn,3Zn,3P,1GPCe,1GPC,1GTC" ),

            // unknown FDEV name here
            new SynthesisRecipe( "Enzyme Missile Launcher Munitions", null, SynthesisRecipe.SynthesisLevel.Basic, "3Fe,3S,4BMC,3PE,3WP,2Pb" ),
            new SynthesisRecipe( "Enzyme Missile Launcher Munitions", null, SynthesisRecipe.SynthesisLevel.Standard, "6S,4W,5BMC,6PE,4WP,4Pb" ),
            new SynthesisRecipe( "Enzyme Missile Launcher Munitions", null, SynthesisRecipe.SynthesisLevel.Premium, "5P,4W,6BMC,5PE,4WP,6Pb" ),

            new SynthesisRecipe( "Explosive Munitions", "Missile Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic,"4S,3Fe,3Ni,4C" ),
            new SynthesisRecipe( "Explosive Munitions", "Missile Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard,"6P,6S,4As,2Hg" ),
            new SynthesisRecipe( "Explosive Munitions", "Missile Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium,"5P,4As,5Hg,5Nb,5Po" ),

            new SynthesisRecipe( "Flechette Launcher Munitions", "Flechette Launcher Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "1W,3EA,2MC,2B" ),
            new SynthesisRecipe( "Flechette Launcher Munitions", "Flechette Launcher Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "4W,6EA,4MC,4B" ),
            new SynthesisRecipe( "Flechette Launcher Munitions", "Flechette Launcher Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "6W,5EA,9MC,6B" ),

            new SynthesisRecipe( "FSD", "FSD Basic", SynthesisRecipe.SynthesisLevel.Basic,"1C,1V,1Ge" ),
            new SynthesisRecipe( "FSD", "FSD Standard", SynthesisRecipe.SynthesisLevel.Standard,"1C,1V,1Ge,1Cd,1Nb" ),
            new SynthesisRecipe( "FSD", "FSD Premium", SynthesisRecipe.SynthesisLevel.Premium,"1C,1Ge,1Nb,1As,1Po,1Y" ),

            new SynthesisRecipe( "Guardian Gauss Cannon Munitions", "Guardian Gauss Cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "3Mn,2FoC,2GPC,4GSWC" ),
            new SynthesisRecipe( "Guardian Gauss Cannon Munitions", "Guardian Gauss Cannon Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "5Mn,3HRC,5FoC,4GPC,3GSWP" ),
            new SynthesisRecipe( "Guardian Gauss Cannon Munitions", "Guardian Gauss Cannon Ammo Premium" , SynthesisRecipe.SynthesisLevel.Premium, "8Mn,6GTC,6FiC,10FoC" ),

            new SynthesisRecipe( "Guardian Plasma Charger Munitions", "Guardian Plasma Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "3Cr,2HDP,3GPC,4GSWC" ),
            new SynthesisRecipe( "Guardian Plasma Charger Munitions", "Guardian Plasma Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "4Cr,2HE,2PA,2GPCe,2GTC" ),
            new SynthesisRecipe( "Guardian Plasma Charger Munitions", "Guardian Plasma Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "6Cr,2Zr,4HE,6PA,4GPCe,3GSWP" ),

            new SynthesisRecipe( "Guardian Shard Cannon Munitions", "Guardian Shard Cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "3C,2V,3CS,3GPCe,5GSWC" ),
            new SynthesisRecipe( "Guardian Shard Cannon Munitions", "Guardian Shard Cannon Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "4CS,2GPCe,2GSWP" ),
            new SynthesisRecipe( "Guardian Shard Cannon Munitions", "Guardian Shard Cannon Ammo Premium" , SynthesisRecipe.SynthesisLevel.Premium, "8C,6GPCe,4V,8CS" ),

            new SynthesisRecipe( "Heat Sinks", "Heat Sink Premium", SynthesisRecipe.SynthesisLevel.Premium, "2BaC,2HCW,2HE,1PHR"),
            new SynthesisRecipe( "Heat Sinks", "Heat Sink Standard",SynthesisRecipe.SynthesisLevel.Standard, "2BaC,2HCW,2HE"),
            new SynthesisRecipe( "Heat Sinks", "Heat Sink Basic",SynthesisRecipe.SynthesisLevel.Basic, "2BaC,2HCW"),

            new SynthesisRecipe( "High Velocity Munitions", "Railgun Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic,"2Fe,1V" ),
            new SynthesisRecipe( "High Velocity Munitions", "Railgun Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard,"4Fe,3V,2Zr,2W" ),
            new SynthesisRecipe( "High Velocity Munitions", "Railgun Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium,"4V,2Zr,4W,2Y" ),

            new SynthesisRecipe( "Large Calibre Munitions", "Cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic,"2S,4Ni,3C" ),
            new SynthesisRecipe( "Large Calibre Munitions", "Cannon Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard,"3P,2Zr,3Zn,1As,2Sn" ),
            new SynthesisRecipe( "Large Calibre Munitions", "Cannon Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium,"8Zn,1As,1Hg,2W,2Sb" ),

            new SynthesisRecipe( "Life Support", "Life Support Basic",  SynthesisRecipe.SynthesisLevel.Basic, "2Fe,1Ni"),

            new SynthesisRecipe( "Limpets", "Limpet Basic", SynthesisRecipe.SynthesisLevel.Basic, "10Fe,10Ni"),

            new SynthesisRecipe( "Nanite Munitions", "Guardian Nanite Torpedo Pylon", SynthesisRecipe.SynthesisLevel.Basic, "2GPCe,5HEXS,5PMR"),

            new SynthesisRecipe( "Plasma Munitions", "Plasma Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic,"4P,3S,1Mn" ),
            new SynthesisRecipe( "Plasma Munitions", "Plasma Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard,"5P,1Se,3Mn,4Mo" ),
            new SynthesisRecipe( "Plasma Munitions", "Plasma Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "5Se,4Mo,4Cd,2Tc" ),

            new SynthesisRecipe( "Seismic Charge Munitions", "Seismic Charge Ammo", SynthesisRecipe.SynthesisLevel.Basic, "2Fe,2Ni,2S,3P,1Hg" ),

            new SynthesisRecipe( "Shock Cannon Munitions", "Shock Cannon Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic, "3GR,2HDP,2FoC,2PA,2Pb" ),
            new SynthesisRecipe( "Shock Cannon Munitions", "Shock Cannon Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard, "5GR,3HDP,4FoC,5PA,3Pb" ),
            new SynthesisRecipe( "Shock Cannon Munitions", "Shock Cannon Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium, "7GR,4HDP,6FoC,8PA,5Pb" ),

            new SynthesisRecipe( "Small Calibre Munitions", "Ammo Basic", SynthesisRecipe.SynthesisLevel.Basic,"2S,2Fe,1Ni" ),
            new SynthesisRecipe( "Small Calibre Munitions", "Ammo Standard", SynthesisRecipe.SynthesisLevel.Standard,"2P,2Fe,2Zr,2Zn,2Se" ),
            new SynthesisRecipe( "Small Calibre Munitions", "Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium,"2P,2S,2Zr,2Hg,2W,1Sb" ),

            new SynthesisRecipe( "SRV Ammo", "Ammo Basic",SynthesisRecipe.SynthesisLevel.Basic,"1P,2S" ),
            new SynthesisRecipe( "SRV Ammo", "Ammo Standard",SynthesisRecipe.SynthesisLevel.Standard,"1P,1Se,1Mn,1Mo" ),
            new SynthesisRecipe( "SRV Ammo", "Ammo Premium", SynthesisRecipe.SynthesisLevel.Premium,"2P,2Se,1Mo,1Tc" ),

            new SynthesisRecipe( "SRV Refuel", "Fuel Basic",SynthesisRecipe.SynthesisLevel.Basic,"1P,1S" ),
            new SynthesisRecipe( "SRV Refuel", "Fuel Standard",SynthesisRecipe.SynthesisLevel.Standard,"1P,1S,1As,1Hg" ),
            new SynthesisRecipe( "SRV Refuel", "Fuel Premium", SynthesisRecipe.SynthesisLevel.Premium,"1S,1As,1Hg,1Tc" ),

            new SynthesisRecipe( "SRV Repair", "Repair Basic",SynthesisRecipe.SynthesisLevel.Basic,"2Fe,1Ni" ),
            new SynthesisRecipe( "SRV Repair", "Repair Standard",SynthesisRecipe.SynthesisLevel.Standard,"3Ni,2V,1Mn,1Mo" ),
            new SynthesisRecipe( "SRV Repair", "Repair Premium", SynthesisRecipe.SynthesisLevel.Premium,"2V,1Zn,2Cr,1W,1Te" ),

            new SynthesisRecipe( "Sub-Surface Displacement Munitions","Sub-surface Displacement Ammo", SynthesisRecipe.SynthesisLevel.Basic, "3Ni,3C,3S,2W" ),
        };

        private static Dictionary<MaterialCommodityMicroResourceType, List<SynthesisRecipe>> SynthesisRecipesByMaterial =
            SynthesisRecipes.SelectMany(r => r.Ingredients.Select(i => new { mat = i, recipe = r }))
                            .GroupBy(a => a.mat)
                            .ToDictionary(g => g.Key, g => g.Select(a => a.recipe).ToList());

        private static Dictionary<EngineeringRecipeFDName, int> FDNameLookup = new Dictionary<EngineeringRecipeFDName, int>();

        private static List<EngineeringRecipe> EngineeringRecipes = new List<EngineeringRecipe>()
        {

        #region Engineering Recipes

        new EngineeringRecipe("Decorative Green", "decorative_green", "1P" , ItemData.ShipModule.ModuleTypes.RemoteReleaseFlakLauncher, 5, "N/A"),
        new EngineeringRecipe("Decorative Yellow", "decorative_yellow", "1P" , ItemData.ShipModule.ModuleTypes.RemoteReleaseFlakLauncher, 5, "N/A"),
        new EngineeringRecipe("Decorative Red", "decorative_red", "1P" , ItemData.ShipModule.ModuleTypes.RemoteReleaseFlakLauncher, 5, "N/A"),
        new EngineeringRecipe("Decorative Pink", "decorative_pink", "1P" , ItemData.ShipModule.ModuleTypes.RemoteReleaseFlakLauncher, 5, "N/A"),

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.AutoFieldMaintenanceUnit, 1, "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.AutoFieldMaintenanceUnit, 2, "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.AutoFieldMaintenanceUnit, 3, "Bill Turner,Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.AutoFieldMaintenanceUnit, 4, "Lori Jameson,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.AutoFieldMaintenanceUnit, 5, "Petra Olmanova" ),

        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe", ItemData.ShipModule.ModuleTypes.Armour, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.Armour, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Fe,1CCo,1HDC", ItemData.ShipModule.ModuleTypes.Armour, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1Ge,1CCe,1FPC", ItemData.ShipModule.ModuleTypes.Armour, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Armour", "armour_advanced", "1CCe,1Sn,1MGA", ItemData.ShipModule.ModuleTypes.Armour, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1Ni", ItemData.ShipModule.ModuleTypes.Armour, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1C,1Zn", ItemData.ShipModule.ModuleTypes.Armour, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1SAll,1V,1Zr", ItemData.ShipModule.ModuleTypes.Armour, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1GA,1W,1Hg", ItemData.ShipModule.ModuleTypes.Armour, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Armour", "armour_explosive", "1PA,1Mo,1Ru", ItemData.ShipModule.ModuleTypes.Armour, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C", ItemData.ShipModule.ModuleTypes.Armour, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C,1SHE", ItemData.ShipModule.ModuleTypes.Armour, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.Armour, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.Armour, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Armour", "armour_heavyduty", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.Armour, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1Ni", ItemData.ShipModule.ModuleTypes.Armour, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1Ni,1V", ItemData.ShipModule.ModuleTypes.Armour, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1SAll,1V,1HDC", ItemData.ShipModule.ModuleTypes.Armour, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1GA,1W,1FPC", ItemData.ShipModule.ModuleTypes.Armour, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Armour", "armour_kinetic", "1PA,1Mo,1FCC", ItemData.ShipModule.ModuleTypes.Armour, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1HCW", ItemData.ShipModule.ModuleTypes.Armour, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1Ni,1HDP", ItemData.ShipModule.ModuleTypes.Armour, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1SAll,1V,1HE", ItemData.ShipModule.ModuleTypes.Armour, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1GA,1W,1HV", ItemData.ShipModule.ModuleTypes.Armour, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Armour", "armour_thermic", "1PA,1Mo,1PHR", ItemData.ShipModule.ModuleTypes.Armour, 5, "Selene Jean,Petra Olmanova" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.BeamLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.BeamLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.BeamLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.BeamLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.BeamLaser, 5, "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.BurstLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.BurstLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.BurstLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.BurstLaser, 4, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.BurstLaser, 5, "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.Cannon, 1, "Marsha Hicks,Tod 'The Blaster' McQuinn,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.Cannon, 2, "Marsha Hicks,Tod 'The Blaster' McQuinn,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.Cannon, 3, "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.Cannon, 4, "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.Cannon, 5, "Marsha Hicks,The Sarge" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.Cannon, 1, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.Cannon, 2, "The Sarge,Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.Cannon, 3, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.Cannon, 4, "The Sarge,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.Cannon, 5, "The Sarge" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", ItemData.ShipModule.ModuleTypes.CargoScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", ItemData.ShipModule.ModuleTypes.CargoScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", ItemData.ShipModule.ModuleTypes.CargoScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", ItemData.ShipModule.ModuleTypes.CargoScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", ItemData.ShipModule.ModuleTypes.CargoScanner, 5, "Tiana Fortune" ),

        new EngineeringRecipe("Chaff Ammo Capacity", "misc_chaffcapacity", "1MS", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 5, "Ram Tah" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 5, "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.ChaffLauncher, 5, "Ram Tah" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.CollectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 5, "Ram Tah" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 5, "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.ElectronicCountermeasure, 5, "Ram Tah" ),

        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF", ItemData.ShipModule.ModuleTypes.Thrusters, 1, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF,1ME", ItemData.ShipModule.ModuleTypes.Thrusters, 2, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1SLF,1Cr,1MC", ItemData.ShipModule.ModuleTypes.Thrusters, 3, "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1MCF,1Se,1CCom", ItemData.ShipModule.ModuleTypes.Thrusters, 4, "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Dirty Drive Tuning", "engine_dirty", "1CIF,1Cd,1PI", ItemData.ShipModule.ModuleTypes.Thrusters, 5, "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1C", ItemData.ShipModule.ModuleTypes.Thrusters, 1, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HCW,1V", ItemData.ShipModule.ModuleTypes.Thrusters, 2, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HCW,1V,1SS", ItemData.ShipModule.ModuleTypes.Thrusters, 3, "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HDP,1HDC,1CoS", ItemData.ShipModule.ModuleTypes.Thrusters, 4, "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Drive Strengthening", "engine_reinforced", "1HE,1FPC,1IS", ItemData.ShipModule.ModuleTypes.Thrusters, 5, "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1S", ItemData.ShipModule.ModuleTypes.Thrusters, 1, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1SLF,1CCo", ItemData.ShipModule.ModuleTypes.Thrusters, 2, "Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1SLF,1CCo,1UED", ItemData.ShipModule.ModuleTypes.Thrusters, 3, "Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1MCF,1CCe,1DED", ItemData.ShipModule.ModuleTypes.Thrusters, 4, "Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Clean Drive Tuning", "engine_tuned", "1CCe,1Sn,1CED", ItemData.ShipModule.ModuleTypes.Thrusters, 5, "Professor Palin,Mel Brandon,Chloe Sedesi" ),

        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C,1ME", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1C,1ME,1CIF", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1V,1MC,1SFP", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Double Shot", "weapon_doubleshot", "1HDC,1CCom,1EFW", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.FragmentCannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.FragmentCannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.FragmentCannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.FragmentCannon, 4, "Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.FragmentCannon, 5, "Zacariah Nemo" ),

        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 1, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR,1Cr", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 2, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1GR,1HDP,1Se", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 3, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1HC,1HE,1Cd", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 4, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Faster FSD Boot Sequence", "fsd_fastboot", "1EA,1HV,1Te", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 5, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1ADWE", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 1, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1ADWE,1CP", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 2, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1P,1CP,1SWS", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 3, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1Mn,1CHD,1EHT", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 4, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Increased FSD Range", "fsd_longrange", "1As,1CM,1DWEx", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 5, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1Ni", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 1, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 2, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1C,1Zn,1SS", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 3, "Colonel Bris Dekker,Elvira Martuuk,Felicity Farseer,Professor Palin,Mel Brandon,Chloe Sedesi" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1V,1HDC,1CoS", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 4, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),
        new EngineeringRecipe("Shielded FSD", "fsd_shielded", "1W,1FPC,1IS", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, 5, "Elvira Martuuk,Felicity Farseer,Mel Brandon" ),

        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1MS", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 1, "Colonel Bris Dekker,Felicity Farseer,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1UEF,1ME", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 2, "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1GR,1TEC,1MC", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 3, "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1ME,1SWS,1DSD", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 4, "Colonel Bris Dekker,Mel Brandon" ),
        new EngineeringRecipe("Expanded FSD Interdictor Capture Arc", "fsdinterdictor_expanded", "1MC,1EHT,1CFSD", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 5, "Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1UEF", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 1, "Colonel Bris Dekker,Felicity Farseer,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1ADWE,1TEC", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 2, "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1ABSD,1AFT,1OSK", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 3, "Colonel Bris Dekker,Tiana Fortune,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1USA,1SWS,1AEA", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 4, "Colonel Bris Dekker,Mel Brandon" ),
        new EngineeringRecipe("Longer Range FSD Interdictor", "fsdinterdictor_longrange", "1CSD,1EHT,1AEC", ItemData.ShipModule.ModuleTypes.FrameShiftDriveInterdictor, 5, "Mel Brandon" ),

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.FuelScoop, 1, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.FuelScoop, 2, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.FuelScoop, 3, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.FuelScoop, 4, "Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.FuelScoop, 5, "Marsha Hicks" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 5, "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 5, "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.FuelTransferLimpetController, 5, "The Sarge,Tiana Fortune" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 4, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 5, "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 4, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 5, "The Sarge,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 4, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.HatchBreakerLimpetController, 5, "The Sarge,Tiana Fortune" ),

        new EngineeringRecipe("Heatsink Ammo Capacity", "misc_heatsinkcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 5, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 5, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, 5, "Ram Tah,Petra Olmanova" ),

        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Fe,1CCo,1HDC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1Ge,1CCe,1FPC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight Hull Reinforcement", "hullreinforcement_advanced", "1CCe,1Sn,1MGA", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1Ni", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1C,1Zn", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1SAll,1V,1Zr", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1GA,1W,1Hg", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Blast Resistant Hull Reinforcement", "hullreinforcement_explosive", "1PA,1Mo,1Ru", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C,1SHE", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Heavy Duty Hull Reinforcement", "hullreinforcement_heavyduty", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1Ni", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1Ni,1V", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1SAll,1V,1HDC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1GA,1W,1FPC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Kinetic Resistant Hull Reinforcement", "hullreinforcement_kinetic", "1PA,1Mo,1FCC", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 5, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1HCW", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 1, "Liz Ryder,Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1Ni,1HDP", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 2, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1SAll,1V,1HE", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 3, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1GA,1W,1HV", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 4, "Selene Jean,Petra Olmanova" ),
        new EngineeringRecipe("Thermal Resistant Hull Reinforcement", "hullreinforcement_thermic", "1PA,1Mo,1PHR", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, 5, "Selene Jean,Petra Olmanova" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", ItemData.ShipModule.ModuleTypes.KillWarrantScanner, 5, "Tiana Fortune" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Etienne Dorn" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Juri Ishmaak" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Juri Ishmaak" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Juri Ishmaak" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.LifeSupport, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.LifeSupport, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.LifeSupport, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.LifeSupport, 4, "Juri Ishmaak,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.LifeSupport, 5, "Juri Ishmaak" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.MissileRack, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.MissileRack, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.MissileRack, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.MissileRack, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.MissileRack, 5, "Liz Ryder" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.MissileRack, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.MissileRack, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.MissileRack, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.MissileRack, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.MissileRack, 5, "Liz Ryder" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.MissileRack, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.MissileRack, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.MissileRack, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.MissileRack, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.MissileRack, 5, "Liz Ryder" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.MissileRack, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.MissileRack, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.MissileRack, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.MissileRack, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.MissileRack, 5, "Liz Ryder" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 1, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 2, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 3, "Tod 'The Blaster' McQuinn,Zacariah Nemo,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 4, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.Multi_Cannon, 5, "Tod 'The Blaster' McQuinn,Marsha Hicks" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 1, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 2, "Bill Turner,Zacariah Nemo,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 3, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 4, "Bill Turner,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, 5, "Bill Turner" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.PointDefence, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.PointDefence, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.PointDefence, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.PointDefence, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.PointDefence, 5, "Ram Tah" ),
        new EngineeringRecipe("Point Defence Ammo Capacity", "misc_pointdefensecapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.PointDefence, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.PointDefence, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.PointDefence, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.PointDefence, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.PointDefence, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.PointDefence, 5, "Ram Tah" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.PointDefence, 1, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.PointDefence, 2, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.PointDefence, 3, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.PointDefence, 4, "Ram Tah,Petra Olmanova" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.PointDefence, 5, "Ram Tah" ),

        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1S", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1SLF,1Cr", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1SLF,1Cr,1HDC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1MCF,1Se,1FPC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("High Charge Capacity Power Distributor", "powerdistributor_highcapacity", "1CIF,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1SLF", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1SLF,1CP", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1GR,1MCF,1CHD", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1HC,1CIF,1CM", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("Charge Enhanced Power Distributor", "powerdistributor_highfrequency", "1CIF,1CM,1EFC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1S", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1S,1CCo", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1ABSD,1Cr,1EA", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1USA,1Se,1PCa", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("Engine Focused Power Distributor", "powerdistributor_priorityengines", "1CSD,1Cd,1MSC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1S", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1S,1CCo", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1ABSD,1Cr,1EA", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1USA,1Se,1PCa", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("System Focused Power Distributor", "powerdistributor_prioritysystems", "1CSD,1Cd,1MSC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1S", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1S,1CCo", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1ABSD,1HC,1Se", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1USA,1EA,1Cd", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("Weapon Focused Power Distributor", "powerdistributor_priorityweapons", "1CSD,1PCa,1Te", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.PowerDistributor, 1, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.PowerDistributor, 2, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 3, "Hera Tani,Marco Qwent,The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 4, "The Dweller" ),
        new EngineeringRecipe("Shielded Power Distributor", "powerdistributor_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.PowerDistributor, 5, "The Dweller" ),

        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1WSE", ItemData.ShipModule.ModuleTypes.PowerPlant, 1, "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1C,1SHE", ItemData.ShipModule.ModuleTypes.PowerPlant, 2, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.PowerPlant, 3, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.PowerPlant, 4, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Armoured Power Plant", "powerplant_armoured", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.PowerPlant, 5, "Hera Tani,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1S", ItemData.ShipModule.ModuleTypes.PowerPlant, 1, "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HCW,1CCo", ItemData.ShipModule.ModuleTypes.PowerPlant, 2, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HCW,1CCo,1Se", ItemData.ShipModule.ModuleTypes.PowerPlant, 3, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1HDP,1CCe,1Cd", ItemData.ShipModule.ModuleTypes.PowerPlant, 4, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Overcharged Power Plant", "powerplant_boosted", "1CCe,1CM,1Te", ItemData.ShipModule.ModuleTypes.PowerPlant, 5, "Hera Tani,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe", ItemData.ShipModule.ModuleTypes.PowerPlant, 1, "Felicity Farseer,Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe,1IED", ItemData.ShipModule.ModuleTypes.PowerPlant, 2, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Fe,1IED,1HE", ItemData.ShipModule.ModuleTypes.PowerPlant, 3, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Ge,1UED,1HV", ItemData.ShipModule.ModuleTypes.PowerPlant, 4, "Hera Tani,Marco Qwent,Etienne Dorn" ),
        new EngineeringRecipe("Low Emissions Power Plant", "powerplant_stealth", "1Nb,1DED,1PHR", ItemData.ShipModule.ModuleTypes.PowerPlant, 5, "Hera Tani,Etienne Dorn" ),

        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 1, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 2, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 3, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 4, "Ram Tah,The Sarge,Tiana Fortune,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.ProspectorLimpetController, 5, "The Sarge,Tiana Fortune,Marsha Hicks" ),

        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1S,1HDP", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1ESED,1Cr,1HE", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1IED,1Se,1HV", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Efficient Weapon", "weapon_efficient", "1UED,1Cd,1PHR", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Fe,1Cr,1CCe", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Ge,1FoC,1PCa", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Focused Weapon", "weapon_focused", "1Nb,1RFC,1MSC", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Ni,1CCo,1EA", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zn,1CCe,1PCa", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Overcharged Weapon", "weapon_overcharged", "1Zr,1CPo,1EFW", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MS,1HDP", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1SLF,1ME,1PAll", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1MCF,1MC,1ThA", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Rapid Fire Modification", "weapon_rapidfire", "1PAll,1CCom,1Tc", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.PulseLaser, 1, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.PulseLaser, 2, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.PulseLaser, 3, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.PulseLaser, 4, "Broo Tarquin,The Dweller,Mel Brandon" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.PulseLaser, 5, "Broo Tarquin,Mel Brandon" ),

        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS", ItemData.ShipModule.ModuleTypes.RailGun, 1, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V", ItemData.ShipModule.ModuleTypes.RailGun, 2, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MS,1V,1Nb", ItemData.ShipModule.ModuleTypes.RailGun, 3, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1ME,1HDC,1Sn", ItemData.ShipModule.ModuleTypes.RailGun, 4, "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("High Capacity Magazine", "weapon_highcapacity", "1MC,1FPC,1MSC", ItemData.ShipModule.ModuleTypes.RailGun, 5, "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.RailGun, 1, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.RailGun, 2, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.RailGun, 3, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.RailGun, 4, "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.RailGun, 5, "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S", ItemData.ShipModule.ModuleTypes.RailGun, 1, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF", ItemData.ShipModule.ModuleTypes.RailGun, 2, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1S,1MCF,1FoC", ItemData.ShipModule.ModuleTypes.RailGun, 3, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1MCF,1FoC,1CPo", ItemData.ShipModule.ModuleTypes.RailGun, 4, "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Weapon", "weapon_longrange", "1CIF,1ThA,1BiC", ItemData.ShipModule.ModuleTypes.RailGun, 5, "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni", ItemData.ShipModule.ModuleTypes.RailGun, 1, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF", ItemData.ShipModule.ModuleTypes.RailGun, 2, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1Ni,1MCF,1EA", ItemData.ShipModule.ModuleTypes.RailGun, 3, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1MCF,1EA,1CPo", ItemData.ShipModule.ModuleTypes.RailGun, 4, "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Short-Range Blaster", "weapon_shortrange", "1CIF,1CCom,1BiC", ItemData.ShipModule.ModuleTypes.RailGun, 5, "Tod 'The Blaster' McQuinn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.RailGun, 1, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.RailGun, 2, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.RailGun, 3, "The Sarge,Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.RailGun, 4, "Tod 'The Blaster' McQuinn,Etienne Dorn" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.RailGun, 5, "Tod 'The Blaster' McQuinn" ),

        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.Refinery, 1, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.Refinery, 2, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.Refinery, 3, "Bill Turner,Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.Refinery, 4, "Lori Jameson,Marsha Hicks" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.Refinery, 5, "Marsha Hicks" ),

        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1P", ItemData.ShipModule.ModuleTypes.Sensor, 1, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.Sensor, 2, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.Sensor, 3, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.Sensor, 4, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Light Weight Scanner", "sensor_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.Sensor, 5, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", ItemData.ShipModule.ModuleTypes.Sensor, 1, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", ItemData.ShipModule.ModuleTypes.Sensor, 2, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", ItemData.ShipModule.ModuleTypes.Sensor, 3, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", ItemData.ShipModule.ModuleTypes.Sensor, 4, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", ItemData.ShipModule.ModuleTypes.Sensor, 5, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", ItemData.ShipModule.ModuleTypes.Sensor, 1, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", ItemData.ShipModule.ModuleTypes.Sensor, 2, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", ItemData.ShipModule.ModuleTypes.Sensor, 3, "Felicity Farseer,Lei Cheung,Hera Tani,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", ItemData.ShipModule.ModuleTypes.Sensor, 4, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", ItemData.ShipModule.ModuleTypes.Sensor, 5, "Lei Cheung,Juri Ishmaak,Tiana Fortune,Bill Turner,Lori Jameson,Etienne Dorn" ),

        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe", ItemData.ShipModule.ModuleTypes.ShieldBooster, 1, "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe,1CCo", ItemData.ShipModule.ModuleTypes.ShieldBooster, 2, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Fe,1CCo,1FoC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 3, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Ge,1USS,1RFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 4, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Blast Resistant Shield Booster", "shieldbooster_explosive", "1Nb,1ASPA,1EFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 5, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1GR", ItemData.ShipModule.ModuleTypes.ShieldBooster, 1, "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1DSCR,1HC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 2, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1DSCR,1HC,1Nb", ItemData.ShipModule.ModuleTypes.ShieldBooster, 3, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1ISSA,1EA,1Sn", ItemData.ShipModule.ModuleTypes.ShieldBooster, 4, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Heavy Duty Shield Booster", "shieldbooster_heavyduty", "1USS,1PCa,1Sb", ItemData.ShipModule.ModuleTypes.ShieldBooster, 5, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1Fe", ItemData.ShipModule.ModuleTypes.ShieldBooster, 1, "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1GR,1Ge", ItemData.ShipModule.ModuleTypes.ShieldBooster, 2, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1SAll,1HC,1FoC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 3, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1GA,1USS,1RFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 4, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shield Booster", "shieldbooster_kinetic", "1PA,1ASPA,1EFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 5, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P", ItemData.ShipModule.ModuleTypes.ShieldBooster, 1, "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P,1CCo", ItemData.ShipModule.ModuleTypes.ShieldBooster, 2, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1P,1CCo,1FoC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 3, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1Mn,1CCe,1RFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 4, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Resistance Augmented Shield Booster", "shieldbooster_resistive", "1CCe,1RFC,1IS", ItemData.ShipModule.ModuleTypes.ShieldBooster, 5, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1Fe", ItemData.ShipModule.ModuleTypes.ShieldBooster, 1, "Didi Vatermann,Felicity Farseer,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HCW,1Ge", ItemData.ShipModule.ModuleTypes.ShieldBooster, 2, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HCW,1HDP,1FoC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 3, "Didi Vatermann,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HDP,1USS,1RFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 4, "Didi Vatermann,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shield Booster", "shieldbooster_thermic", "1HE,1ASPA,1EFC", ItemData.ShipModule.ModuleTypes.ShieldBooster, 5, "Didi Vatermann,Mel Brandon" ),

        new EngineeringRecipe("Rapid Charge ShieldCellBank Bank", "shieldcellbank_rapid", "1S", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 1, "Elvira Martuuk,Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge ShieldCellBank Bank", "shieldcellbank_rapid", "1GR,1Cr", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 2, "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge ShieldCellBank Bank", "shieldcellbank_rapid", "1S,1HC,1PAll", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 3, "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Rapid Charge ShieldCellBank Bank", "shieldcellbank_rapid", "1Cr,1EA,1ThA", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 4, "Mel Brandon" ),
        new EngineeringRecipe("Specialised ShieldCellBank Bank", "shieldcellbank_specialised", "1SLF", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 1, "Elvira Martuuk,Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised ShieldCellBank Bank", "shieldcellbank_specialised", "1SLF,1CCo", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 2, "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised ShieldCellBank Bank", "shieldcellbank_specialised", "1ESED,1CCo,1CIF", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 3, "Lori Jameson,Mel Brandon" ),
        new EngineeringRecipe("Specialised ShieldCellBank Bank", "shieldcellbank_specialised", "1CCo,1CIF,1Y", ItemData.ShipModule.ModuleTypes.ShieldCellBank, 4, "Mel Brandon" ),

        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 1, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR,1MCF", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 2, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1DSCR,1MCF,1Se", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 3, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1ISSA,1FoC,1Hg", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 4, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Kinetic Resistant Shields", "shieldgenerator_kinetic", "1USS,1RFC,1Ru", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 5, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 1, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR,1Ge", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 2, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1DSCR,1Ge,1PAll", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 3, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1ISSA,1Nb,1ThA", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 4, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Enhanced, Low Power Shields", "shieldgenerator_optimised", "1USS,1Sn,1MGA", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 5, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 1, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P,1CCo", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 2, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1P,1CCo,1MC", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 3, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1Mn,1CCe,1CCom", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 4, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Reinforced Shields", "shieldgenerator_reinforced", "1As,1CPo,1IC", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 5, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 1, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR,1Ge", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 2, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1DSCR,1Ge,1Se", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 3, "Didi Vatermann,Elvira Martuuk,Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1ISSA,1FoC,1Hg", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 4, "Lei Cheung,Mel Brandon" ),
        new EngineeringRecipe("Thermal Resistant Shields", "shieldgenerator_thermic", "1USS,1RFC,1Ru", ItemData.ShipModule.ModuleTypes.ShieldGenerator, 5, "Lei Cheung,Mel Brandon" ),

        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 1, "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS,1Ge", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 2, "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MS,1Ge,1PA", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 3, "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Tiana Fortune,Felicity Farseer,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1ME,1Nb,1PLA", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 4, "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Hera Tani" ),
        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_expanded", "1MC,1Sn,1PRA", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 5, "Etienne Dorn,Bill Turner,Juri Ishmaak,Lei Cheung,Lori Jameson,Hera Tani" ),

        new EngineeringRecipe("Expanded Probe Scanning Radius", "sensor_fastscan", "", ItemData.ShipModule.ModuleTypes.SurfaceScanner, 5, "" ),     // its in the logs but can't match to Inara

        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1P", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Light Weight Mount", "weapon_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 5, "Liz Ryder" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 1, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 2, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 3, "Juri Ishmaak,Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 4, "Liz Ryder,Petra Olmanova" ),
        new EngineeringRecipe("Sturdy Mount", "weapon_sturdy", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.TorpedoPylon, 5, "Liz Ryder" ),

        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1P,1FFC,1OSK", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1Mn,1FoC,1AEA", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Fast Scanner", "sensor_fastscan", "1As,1RFC,1AEC", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1P", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1SAll,1Mn,1CCe", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCo,1PA,1PLA", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Lightweight", "misc_lightweight", "1CCe,1PLA,1PRA", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Fe,1HC,1UED", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Ge,1EA,1DED", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Long-Range Scanner", "sensor_longrange", "1Nb,1PCa,1CED", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Ni,1SHE,1W", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1Zn,1W,1Mo", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Reinforced", "misc_reinforced", "1HDC,1Mo,1Tc", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1WSE", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1C,1SHE,1HDC", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1V,1SS,1FPC", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Shielded", "misc_shielded", "1W,1CoS,1FCC", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS", ItemData.ShipModule.ModuleTypes.WakeScanner, 1, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge", ItemData.ShipModule.ModuleTypes.WakeScanner, 2, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MS,1Ge,1CSD", ItemData.ShipModule.ModuleTypes.WakeScanner, 3, "Bill Turner,Juri Ishmaak,Lori Jameson,Tiana Fortune" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1ME,1Nb,1DSD", ItemData.ShipModule.ModuleTypes.WakeScanner, 4, "Tiana Fortune,Etienne Dorn" ),
        new EngineeringRecipe("Wide Angle Scanner", "sensor_wideangle", "1MC,1Sn,1CFSD", ItemData.ShipModule.ModuleTypes.WakeScanner, 5, "Tiana Fortune" ),

        // pre-engineered cargo racks
        new EngineeringRecipe("Increase Capcacity", "cargorack_increasedcapacity", "", ItemData.ShipModule.ModuleTypes.CargoRack,2, "Tiana Fortune,Ram Tah,Bill Turner" ),
        new EngineeringRecipe("Increase Capcacity", "cargorack_increasedcapacity", "", ItemData.ShipModule.ModuleTypes.CargoRack,3, "Tiana Fortune,Ram Tah,Bill Turner" ),
        new EngineeringRecipe("Increase Capcacity", "cargorack_increasedcapacity", "", ItemData.ShipModule.ModuleTypes.CargoRack,4, "Tiana Fortune,Ram Tah,Bill Turner"),
        new EngineeringRecipe("Increase Capcacity", "cargorack_increasedcapacity", "", ItemData.ShipModule.ModuleTypes.CargoRack,5, "Tiana Fortune,Ram Tah,Bill Turner"),

        // obsolete
        new EngineeringRecipe("Guardian Upgrade Obsolete", "guardianweapon_sturdy", "1C", ItemData.ShipModule.ModuleTypes.UnknownType, 5, "Tiana Fortune" ),  // wrong data, don't worry
        new EngineeringRecipe("Guardian Upgrade Obsolete", "guardianmodule_sturdy", "1C", ItemData.ShipModule.ModuleTypes.UnknownType, 5, "Tiana Fortune" ),  // wrong data, don't worry

            #endregion

            #region Tech broker - some of these have unknown IDs

            // inara, https://inara.cz/elite/techbroker/#tab_techbrokerslot1 left column august 2024

            new EngineeringRecipe("Sirius AX Missile Rack Fixed Large","unknown","Human",ItemData.ShipModule.ModuleTypes.MissileRack,"20ME,9HDP,13V,10HDC,10MSC,3Tc"),
            new EngineeringRecipe("Sirius AX Missile Rack Fixed Medium","unknown","Human",ItemData.ShipModule.ModuleTypes.MissileRack,"12ME,6HDP,8V,5HDC,5MSC"),
            new EngineeringRecipe("Azimuth Enhanced AX Multi Cannon Gimballed Large","unknown","Human",ItemData.ShipModule.ModuleTypes.Multi_Cannon,"11Fe,16Zr,9BMC,12TGWC,17WP,6SSD"),
            new EngineeringRecipe("Azimuth Enhanced AX Multi Cannon Gimballed Medium","unknown","Human",ItemData.ShipModule.ModuleTypes.Multi_Cannon,"9Fe,11Zr,6BMC,7TGWC,14WP"),
            new EngineeringRecipe("Bobblehead","unknown","Human",ItemData.ShipModule.ModuleTypes.VanityType,"10MA,1THH"),
            new EngineeringRecipe("Corrosion Resistant Cargo Rack Size 4 Class 1","int_corrosionproofcargorack_size4_class1","Human",ItemData.ShipModule.ModuleTypes.CargoRack,"16MA,26Fe,18CM,22RB,12NFI"),
            new EngineeringRecipe("Detailed Surface Scanner (Engineered V1)","int_detailedsurfacescanner_tiny","Human",ItemData.ShipModule.ModuleTypes.SurfaceScanner, "22Ge,24Nb,28MC,26MS"),
            new EngineeringRecipe("Enzyme Missile Rack Fixed Medium","hpt_causticmissile_fixed_medium","Human",ItemData.ShipModule.ModuleTypes.MissileRack,"16UKEC,18UKOC,16Mo,15W,6RB"),
            new EngineeringRecipe("Engineered FSD V1 (Class 5)","int_hyperdrive_size5_class5","Human",ItemData.ShipModule.ModuleTypes.FrameShiftDrive,"18DWEx,26Te,26EA,28CP"),
            new EngineeringRecipe("Sirius Heat Sink Launcher","unknown","Human",ItemData.ShipModule.ModuleTypes.HeatSinkLauncher, "8MS,6Nb,6V,5MC"),
            new EngineeringRecipe("Meta Alloy Hull Reinforcement Size 1 Class 1","int_metaalloyhullreinforcement_size1_class1","Human",ItemData.ShipModule.ModuleTypes.HullReinforcementPackage,"16MA,15FoC,22ASPA,20CCom,12RMP"),
            new EngineeringRecipe("Mining Laser (Fixed Small)","?","Human",ItemData.ShipModule.ModuleTypes.MiningLaser,"16OSM,20As,24Re,28P"),

            // inara, https://inara.cz/elite/techbroker/#tab_techbrokerslot1 right column august 2024

            new EngineeringRecipe("Flechette Launcher Fixed Medium","hpt_flechettelauncher_fixed_medium","Human",ItemData.ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,"30Fe,24Mo,22Re,26Ge,8CMMC"),
            new EngineeringRecipe("Flechette Launcher Turret Medium","hpt_flechettelauncher_turret_medium","Human",ItemData.ShipModule.ModuleTypes.RemoteReleaseFlechetteLauncher,"28Fe,28Mo,20Re,24Ge,10AM"),
            new EngineeringRecipe("Engineered SeekerMissileRack Rack V1 (Fixed, Class 2)","?","Human",ItemData.ShipModule.ModuleTypes.MissileRack,"10OSM,16PRA,24CCe,26HC,28P"),
            new EngineeringRecipe("Plasma Shock Cannon Fixed Large","hpt_plasmashockcannon_fixed_large","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"28V,26W,24Re,26Tc,8PC"),
            new EngineeringRecipe("Plasma Shock Cannon Fixed Medium","hpt_plasmashockcannon_fixed_medium","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"24V,26W,20Re,28Tc,6IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Fixed Small","hpt_plasmashockcannon_fixed_small","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"8V,10W,8Re,12Tc,4PC"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Large","hpt_plasmashockcannon_turret_large","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"26V,28W,22Re,24Tc,10IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Medium","hpt_plasmashockcannon_turret_medium","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"24V,22W,20Re,28Tc,8PTB"),
            new EngineeringRecipe("Plasma Shock Cannon Turret Small","hpt_plasmashockcannon_turret_small","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"8V,12W,10Re,10Tc,4IOD"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Large","hpt_plasmashockcannon_gimbal_large","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"28V,24W,24Re,22Tc,12PTB"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Medium","hpt_plasmashockcannon_gimbal_medium","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"24V,22W,20Re,28Tc,10PC"),
            new EngineeringRecipe("Plasma Shock Cannon Gimbal Small","hpt_plasmashockcannon_gimbal_small","Human",ItemData.ShipModule.ModuleTypes.ShockCannon,"10V,11W,8Re,10Tc,4PTB"),
            new EngineeringRecipe("TG Pulse Neutraliser","hpt_antiunknownshutdown_tiny_v2","Human",ItemData.ShipModule.ModuleTypes.Module,"5MESA,5PE,5UES,1ARTG"),

            // Inara https://inara.cz/elite/techbroker/#tab_techbrokerslot2  August 2024

            new EngineeringRecipe("Guardian FSD Booster Size 1","int_guardianfsdbooster_size1","Guardian",ItemData.ShipModule.ModuleTypes.FrameShiftDrive,"1GMBS,21GPCe,21GTC,24FoC,8HNSM"),
            new EngineeringRecipe("Guardian Hull Reinforcement Size 1 Class 1","int_guardianhullreinforcement_size1_class1","Guardian",ItemData.ShipModule.ModuleTypes.HullReinforcementPackage,"1GMBS,21GSWC,16PBOD,16PGOD,12RMP"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 1","gdn_hybrid_fighter_v1","Guardian",ItemData.ShipModule.ModuleTypes.Fighter,"1GMVB,25GPCe,26PEOD,18PBOD,25GTC"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 2","gdn_hybrid_fighter_v2","Guardian",ItemData.ShipModule.ModuleTypes.Fighter,"1GMVB,25GPCe,26PEOD,18GSWC,25GTC"),
            new EngineeringRecipe("Guardian Hybrid Fighter V 3","gdn_hybrid_fighter_v3","Guardian",ItemData.ShipModule.ModuleTypes.Fighter,"1GMVB,25GPCe,26PEOD,18GSWP,25GTC"),
            new EngineeringRecipe("Guardian Module Reinforcement Size 1 Class 1","int_guardianmodulereinforcement_size1_class1","Guardian",ItemData.ShipModule.ModuleTypes.Module,"1GMBS,18GSWC,15PEOD,20GPC,9RMP"),
            new EngineeringRecipe("Guardian Power Distributor Size 1","int_guardianpowerdistributor_size1","Guardian",ItemData.ShipModule.ModuleTypes.PowerDistributor,"1GMBS,20PAOD,24GPCe,18PA,6HSI"),
            new EngineeringRecipe("Guardian Power Plant Size 2","int_guardianpowerplant_size2","Guardian",ItemData.ShipModule.ModuleTypes.PowerPlant,"1GMBS,18GPC,21PEOD,15HRC,10EGA"),
            new EngineeringRecipe("Guardian Shield Reinforcement Size 1 Class 1","int_guardianshieldreinforcement_size1_class1","Guardian",ItemData.ShipModule.ModuleTypes.ShieldGenerator,"1GMBS,17GPCe,20GTC,24PDOD,8DIS"),

            // Inara https://inara.cz/elite/techbroker/#tab_techbrokerslot3 left column August 2024

            new EngineeringRecipe("Guardian Gauss Cannon Fixed Medium","hpt_guardian_gausscannon_fixed_medium", "Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianGaussCannon,"1GWBS,18GPCe,20GTC,15Mn,6MEC"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Medium Modified","?","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianGaussCannon,"6TCU,18GPCe,20GTC,15Nb,1GWBS"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Small","hpt_guardian_gausscannon_fixed_small","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianGaussCannon,"1GWBS,12GPC,12GSWC,15GSWP"),
            new EngineeringRecipe("Guardian Gauss Cannon Fixed Small Modified","?","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianGaussCannon,"12GPC,12GSWC,15GSWP,9Nb,1GWBS"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Large","hpt_guardian_plasmalauncher_fixed_large","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"1GWBS,28GPC,20GSWP,28Cr,10MWCH"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Medium","hpt_guardian_plasmalauncher_fixed_medium","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"1GWBS,18GPC,16GSWP,14Cr,8MWCH"),
            new EngineeringRecipe("Guardian Plasma Launcher Fixed Small","hpt_guardian_plasmalauncher_fixed_small","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"1GWBS,12GPCe,12GSWP,15GTC"),
            new EngineeringRecipe("Guardian Plasma Launcher Turret Large","hpt_guardian_plasmalauncher_turret_large","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"2GWBS,20GPC,24GSWP,26Cr,10AM"),
            new EngineeringRecipe("Guardian Plasma Launcher Turret Medium","hpt_guardian_plasmalauncher_turret_medium","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"2GWBS,21GPC,20GSWP,16Cr,8AM"),

            // Inara https://inara.cz/elite/techbroker/#tab_techbrokerslot3 right column August 2024

            new EngineeringRecipe("Guardian Plasma Launcher Turret Small","hpt_guardian_plasmalauncher_turret_small","Guardian Weapons",ItemData.ShipModule.ModuleTypes.PlasmaAccelerator,"1GWBS,12GPCe,12GTC,15GSWP"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Large","hpt_guardian_shardcannon_fixed_large","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"1GWBS,20GSWC,28GTC,20C,18MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Medium Modified","?","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"12PC,20GSWC,18GTC,14Ge,1GWBS"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Medium","hpt_guardian_shardcannon_fixed_medium","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"1GWBS,20GSWC,18GTC,14C,12PTB"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Small Modified","?","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"12GPC,12GSWC,15GSWP,6Ge,1GWBS"),
            new EngineeringRecipe("Guardian Shard Cannon Fixed Small","hpt_guardian_shardcannon_fixed_small","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"1GWBS,12GPC,12GTC,15GSWP"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Large","hpt_guardian_shardcannon_turret_large","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"2GWBS,20GSWC,26GTC,28C,12MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Medium","hpt_guardian_shardcannon_turret_medium","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"2GWBS,16GSWC,20GTC,15C,12MCC"),
            new EngineeringRecipe("Guardian Shard Cannon Turret Small","hpt_guardian_shardcannon_turret_small","Guardian Weapons",ItemData.ShipModule.ModuleTypes.GuardianShardCannon,"1GWBS,12GPC,15GTC,12GSWP"),

            // Special internal modules not associated with engineers - a few on https://inara.cz/elite/blueprints/#tab_outfittingslot3

            new EngineeringRecipe("Anti-Guardian Zone Resistance FSD", "guardian_fsd_unknown", "Guardian", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "2HSF,1CACR,1TCC" ),
            new EngineeringRecipe("Anti-Guardian Zone Resistance Hull", "guardian_hull_unknown", "Guardian", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, "2HSF,1CACR,1TCC" ),
            new EngineeringRecipe("Anti-Guardian Zone Resistance Modules", "guardian_modules_unknown", "Guardian", ItemData.ShipModule.ModuleTypes.Module, "2HSF,1CACR,1TCC" ),

            // Pre-engineered SCO FSDs

            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 2)", "int_hyperdrive_overcharge_size2_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "10DWEx,14Te,6EA,9CP,4PE,1TDC"),
            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 3)", "int_hyperdrive_overcharge_size3_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "12DWEx,16Te,9EA,11CP,5PE,1TDC"),
            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 4)", "int_hyperdrive_overcharge_size4_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "13DWEx,20Te,9EA,13CP,6PE,1TDC"),
            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 5)", "int_hyperdrive_overcharge_size5_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "15DWEx,24Te,12EA,14CP,7PE,1TDC"),
            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 6)", "int_hyperdrive_overcharge_size6_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "15DWEx,28Te,12EA,16CP,8PE,1TDC"),
            new EngineeringRecipe("Engineered FSD (SCO) V1 (Class 7)", "int_hyperdrive_overcharge_size7_class5", "Human", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "16DWEx,28Te,14EA,18CP,12PE,1TDC"),

            #endregion

            #region Special Effects

            new EngineeringRecipe("Angled Plating", "special_armour_kinetic", ItemData.ShipModule.ModuleTypes.Armour, "5CC,3HDC,3Zr"),
            new EngineeringRecipe("Angled Plating", "special_hullreinforcement_kinetic", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, "5TeA,3Zr,5C,3HDC"),
            new EngineeringRecipe("Auto Loader", "special_auto_loader", "Cannon,Multi_Cannon", "4ME,3MC,3HDC"),
            new EngineeringRecipe("Blast Block", "special_shieldbooster_explosive", ItemData.ShipModule.ModuleTypes.ShieldBooster, "5ISSA,3HRC,3HDP,2Se"),
            new EngineeringRecipe("Boss Cells", "special_shieldcell_oversized", "ShieldCellBank", "5CSU,3Cr,1PCa"),
            new EngineeringRecipe("Cluster Capacitors", "special_powerdistributor_capacity", ItemData.ShipModule.ModuleTypes.PowerDistributor, "5P,3HRC,1Cd"),
            new EngineeringRecipe("Concordant Sequence", "special_concordant_sequence", "PulseLaser,BurstLaser,BeamLaser", "5FoC,3EFW,1Zr"),
            new EngineeringRecipe("Corrosive Shell", "special_corrosive_shell", "Multi_Cannon,FragmentCannon", "5CSU,4PAll,3As"),
            new EngineeringRecipe("Dazzle Shell", "special_blinding_shell", "PlasmaAccelerator,FragmentCannon", "5MS,4Mn,5HC,5MS"),
            new EngineeringRecipe("Deep Charge", "special_fsd_fuelcapacity", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "5ADWE,3GA,1EHT"),
            new EngineeringRecipe("Deep Plating", "special_armour_chunky", ItemData.ShipModule.ModuleTypes.Armour, "5CC,3ME,2Mo"),
            new EngineeringRecipe("Deep Plating", "special_hullreinforcement_chunky", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, "5CC,3Mo,2Ru"),
            new EngineeringRecipe("Dispersal Field", "special_dispersal_field", "PlasmaAccelerator,Cannon", "5CCo,5HC,5IED,5WSE"),
            new EngineeringRecipe("Double Braced", "special_engine_toughened", ItemData.ShipModule.ModuleTypes.Thrusters, "5Fe,3HC,1FPC"),
            new EngineeringRecipe("Double Braced", "special_fsd_toughened", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "5ADWE,3GA,1CCom"),
            new EngineeringRecipe("Double Braced", "special_powerdistributor_toughened", ItemData.ShipModule.ModuleTypes.PowerDistributor, "5P,3HRC,1FPC"),
            new EngineeringRecipe("Double Braced", "special_powerplant_toughened", ItemData.ShipModule.ModuleTypes.PowerPlant, "5GR,3V,1FPC"),
            new EngineeringRecipe("Double Braced", "special_shield_toughened", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1CCom"),
            new EngineeringRecipe("Double Braced", "special_shieldbooster_toughened", ItemData.ShipModule.ModuleTypes.ShieldBooster, "5DSCR,3GA,3SHE"),
            new EngineeringRecipe("Double Braced", "special_shieldcell_toughened", "ShieldCellBank", "5CSU,3Cr,1Y"),
            new EngineeringRecipe("Double Braced", "special_weapon_toughened", ItemData.ShipModule.ModuleTypes.Weapon, "5MS,5CC,3V"),
            new EngineeringRecipe("Drag Drives", "special_engine_overloaded", ItemData.ShipModule.ModuleTypes.Thrusters, "5Fe,3HC,1SFP"),
            new EngineeringRecipe("Drag Munitions", "special_drag_munitions", "FragmentCannon,SeekerMissileRack", "5C,5GR,2Mo"),
            new EngineeringRecipe("Drive Distributors", "special_engine_haulage", ItemData.ShipModule.ModuleTypes.Thrusters, "5Fe,3HC,1SFP"),
            new EngineeringRecipe("Emissive Munitions", "special_emissive_munitions", "PulseLaser,Multi_Cannon,SeekerMissileRack,MissileRack,MineLauncher", "4ME,3UED,3HE,3Mn"),
            new EngineeringRecipe("Fast Charge", "special_shield_regenerative", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1CoS"),
            new EngineeringRecipe("Feedback Cascade", "special_feedback_cascade_cooled", ItemData.ShipModule.ModuleTypes.RailGun, "5OSK,5SHE,5FiC"),
            new EngineeringRecipe("Feedback Cascade", "special_feedback_cascade", ItemData.ShipModule.ModuleTypes.RailGun, "5OSK,5SHE,5FiC"),
            new EngineeringRecipe("Flow Control", "special_powerdistributor_efficient", ItemData.ShipModule.ModuleTypes.PowerDistributor, "5P,3HRC,1CPo"),
            new EngineeringRecipe("Flow Control", "special_shieldbooster_efficient", ItemData.ShipModule.ModuleTypes.ShieldBooster, "5ISSA,3SFP,3FoC,3Nb"),
            new EngineeringRecipe("Flow Control", "special_shieldcell_efficient", "ShieldCellBank", "5CSU,3Cr,1CPo"),
            new EngineeringRecipe("Flow Control", "special_weapon_efficient", ItemData.ShipModule.ModuleTypes.Weapon, "5MS,3HC,1EFW"),
            new EngineeringRecipe("Force Block", "special_shield_kinetic", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1DED"),
            new EngineeringRecipe("Force Block", "special_shieldbooster_kinetic", ItemData.ShipModule.ModuleTypes.ShieldBooster, "5USA,3SS,2ASPA"),
            new EngineeringRecipe("Force Shell", "special_force_shell", ItemData.ShipModule.ModuleTypes.Cannon, "5MS,5Zn,3PA,3HCW"),
            new EngineeringRecipe("FSD Interrupt", "special_fsd_interrupt", "MissileRack", "3SWS,5AFT,5ME,3CCom"),
            new EngineeringRecipe("Hi-Cap", "special_shield_health", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1CPo"),
            new EngineeringRecipe("High Yield Shell", "special_high_yield_shell", ItemData.ShipModule.ModuleTypes.Cannon, "5MS,3PLA,3CM,5Ni"),
            new EngineeringRecipe("Incendiary Rounds", "special_incendiary_rounds", "Multi_Cannon,FragmentCannon", "5HCW,5P,5S,3PA"),
            new EngineeringRecipe("Inertial Impact", "special_distortion_field", ItemData.ShipModule.ModuleTypes.BurstLaser, "5FFC,5DSCR,5ADWE"),
            new EngineeringRecipe("Ion Disruption", "special_choke_canister", ItemData.ShipModule.ModuleTypes.LifeSupport, "5S,5P,3CHD,3EA"),
            new EngineeringRecipe("Layered Plating", "special_armour_explosive", ItemData.ShipModule.ModuleTypes.Armour, "5HCW,3HDC,1Nb"),
            new EngineeringRecipe("Layered Plating", "special_hullreinforcement_explosive", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, "5HCW,3SS,3W"),
            new EngineeringRecipe("Lo-draw", "special_shield_efficient", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1CPo"),
            new EngineeringRecipe("Mass Lock Munition", "special_mass_lock", ItemData.ShipModule.ModuleTypes.TorpedoPylon, "5ME,3HDC,3ASPA"),
            new EngineeringRecipe("Mass Manager", "special_fsd_heavy", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "5ADWE,3GA,1EHT"),
            new EngineeringRecipe("Monstered", "special_powerplant_highcharge", ItemData.ShipModule.ModuleTypes.PowerPlant, "5GR,3V,1PCa"),
            new EngineeringRecipe("Multi-servos", "special_weapon_rateoffire", "PulseLaser,BurstLaser,Cannon,Multi_Cannon,PlasmaAccelerator,RailGun,FragmentCannon,MissileRack", "5MS,4FoC,2CPo,2CCom"),
            new EngineeringRecipe("Multi-weave", "special_shield_resistive", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1ASPA"),
            new EngineeringRecipe("Overload Munitions", "special_overload_munitions", "SeekerMissileRack,MissileRack,MineLauncher", "5FiC,4TEC,2ASPA,3Ge"),
            new EngineeringRecipe("Oversized", "special_weapon_damage", ItemData.ShipModule.ModuleTypes.Weapon, "5MS,3MC,1Ru"),
            new EngineeringRecipe("Penetrator Munitions", "special_penetrator_munitions", "MissileRack", "5GA,3EA,3Zr"),
            new EngineeringRecipe("Penetrator Payload", "special_deep_cut_payload", ItemData.ShipModule.ModuleTypes.TorpedoPylon, "3MC,3W,5ABSD,3Se"),
            new EngineeringRecipe("Phasing Sequence", "special_phasing_sequence", "PulseLaser,BurstLaser,PlasmaAccelerator", "5FoC,3ASPA,3Nb,3CCom"),
            new EngineeringRecipe("Plasma Slug", "special_plasma_slug", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, "3HE,2EFW,2RFC,4Hg"),
            new EngineeringRecipe("Plasma Slug", "special_plasma_slug_cooled", ItemData.ShipModule.ModuleTypes.RailGun, "3HE,2EFW,2RFC,4Hg"),
            new EngineeringRecipe("Radiant Canister", "special_radiant_canister", ItemData.ShipModule.ModuleTypes.LifeSupport, "1Po,3PA,4HDP"),
            new EngineeringRecipe("Recycling Cell", "special_shieldcell_gradual", "ShieldCellBank", "5CSU,3Cr,1CCom"),
            new EngineeringRecipe("Reflective Plating", "special_armour_thermic", ItemData.ShipModule.ModuleTypes.Armour, "5CC,3HDP,2ThA"),
            new EngineeringRecipe("Reflective Plating", "special_hullreinforcement_thermic", ItemData.ShipModule.ModuleTypes.HullReinforcementPackage, "5HCW,3HDP,1PLA,4Zn"),
            new EngineeringRecipe("Regeneration Sequence", "special_regeneration_sequence", ItemData.ShipModule.ModuleTypes.BeamLaser, "3RFC,4SS,1PSFD"),
            new EngineeringRecipe("Reverberating Cascade", "special_reverberating_cascade", "TorpedoPylon,MineLauncher", "2CCom,3CSD,4FiC,4Cr"),
            new EngineeringRecipe("Scramble Spectrum", "special_scramble_spectrum", "PulseLaser,BurstLaser", "5CS,3USS,5ESED"),
            new EngineeringRecipe("Screening Shell", "special_screening_shell", ItemData.ShipModule.ModuleTypes.FragmentCannon, "5MS,5DSCR,5MCF,3Nb"),
            new EngineeringRecipe("Shift-lock Canister", "special_shiftlock_canister", ItemData.ShipModule.ModuleTypes.LifeSupport, "5TeA,3SWS,5SAll"),
            new EngineeringRecipe("Smart Rounds", "special_smart_rounds", "Cannon,Multi_Cannon", "5MS,3SFP,3DED,3CSD"),
            new EngineeringRecipe("Stripped Down", "special_engine_lightweight", ItemData.ShipModule.ModuleTypes.Thrusters, "5Fe,3HC,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_fsd_lightweight", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "5ADWE,3GA,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_powerdistributor_lightweight", ItemData.ShipModule.ModuleTypes.PowerDistributor, "5P,3HRC,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_powerplant_lightweight", ItemData.ShipModule.ModuleTypes.PowerPlant, "5GR,3V,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_shield_lightweight", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_shieldcell_lightweight", "ShieldCellBank", "5CSU,3Cr,1PLA"),
            new EngineeringRecipe("Stripped Down", "special_weapon_lightweight", ItemData.ShipModule.ModuleTypes.Weapon, "5SAll,5C,1Sn"),
            new EngineeringRecipe("Super Capacitors", "special_shieldbooster_chunky", ItemData.ShipModule.ModuleTypes.ShieldBooster, "3USS,5CC,2Cd"),
            new EngineeringRecipe("Super Conduits", "special_powerdistributor_fast", ItemData.ShipModule.ModuleTypes.PowerDistributor, "5P,3HRC,1SFP"),
            new EngineeringRecipe("Super Penetrator", "special_super_penetrator_cooled", ItemData.ShipModule.ModuleTypes.RailGun, "3PLA,3RFC,3Zr,5USS"),
            new EngineeringRecipe("Super Penetrator", "special_super_penetrator", ItemData.ShipModule.ModuleTypes.RailGun, "3PLA,3RFC,3Zr,5USS"),
            new EngineeringRecipe("Target Lock Breaker", "special_lock_breaker", ItemData.ShipModule.ModuleTypes.PlasmaAccelerator, "5Se,3SFP,1AEC"),
            new EngineeringRecipe("Thermal Cascade", "special_thermal_cascade", "Cannon,SeekerMissileRack,MissileRack", "5HCW,4HC,3HDC,5P"),
            new EngineeringRecipe("Thermal Conduit", "special_thermal_conduit", "BeamLaser,PlasmaAccelerator", "5HDP,5S,5TeA"),
            new EngineeringRecipe("Thermal Shock", "special_thermalshock", "PulseLaser,BurstLaser,BeamLaser,Multi_Cannon", "5FFC,3HRC,3CCo,3W"),
            new EngineeringRecipe("Thermal Spread", "special_engine_cooled", ItemData.ShipModule.ModuleTypes.Thrusters, "5Fe,3HC,1HV"),
            new EngineeringRecipe("Thermal Spread", "special_fsd_cooled", ItemData.ShipModule.ModuleTypes.FrameShiftDrive, "5ADWE,3GA,1HV,3GR"),
            new EngineeringRecipe("Thermal Spread", "special_powerplant_cooled", ItemData.ShipModule.ModuleTypes.PowerPlant, "5GR,3V,1HV"),
            new EngineeringRecipe("Thermal Vent", "special_thermal_vent", ItemData.ShipModule.ModuleTypes.BeamLaser, "5FFC,3CPo,3PAll"),
            new EngineeringRecipe("Thermo Block", "special_shield_thermic", ItemData.ShipModule.ModuleTypes.ShieldGenerator, "5WSE,3FFC,1HV"),
            new EngineeringRecipe("Thermo Block", "special_shieldbooster_thermic", ItemData.ShipModule.ModuleTypes.ShieldBooster, "5ABSD,3CCe,3HV"),
            
            #endregion

            #region suit/weapon levels

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Remlok",2,"1MRISuitSch,1MRIHM,1MRDMI,2MRCCFP,2MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Remlok",3,"2MRISuitSch,2MRIHM,2MRDMI,5MRCCFP,5MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Remlok",4,"4MRISuitSch,4MRIHM,4MRDMI,9MRCCFP,9MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Remlok",5,"5MRISuitSch,5MRIHM,5MRDMI,12MRCCFP,12MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Supratech",2,"1MRISuitSch,1MRIHM,1MRDMI,2MRCA,2MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Supratech",3,"2MRISuitSch,2MRIHM,2MRDMI,5MRCA,5MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Supratech",4,"4MRISuitSch,4MRIHM,4MRDMI,9MRCA,9MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Supratech",5,"5MRISuitSch,5MRIHM,5MRDMI,12MRCA,12MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Manticore",2,"1MRISuitSch,1MRIHM,1MRDMI,2MRCTP,2MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Manticore",3,"2MRISuitSch,2MRIHM,2MRDMI,5MRCTP,5MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Manticore",4,"4MRISuitSch,4MRIHM,4MRDMI,9MRCTP,9MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"Manticore",5,"5MRISuitSch,5MRIHM,5MRDMI,12MRCTP,12MRCG"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Kinematic Armaments",2,"1MRIWS,1MRICLG,1MRDMI,2MRCTC,2MRCWC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Kinematic Armaments",3,"2MRIWS,2MRICLG,2MRDMI,5MRCTC,5MRCWC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Kinematic Armaments",4,"4MRIWS,4MRICLG,4MRDMI,9MRCTC,9MRCWC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Kinematic Armaments",5,"5MRIWS,5MRICLG,5MRDMI,12MRCTC,12MRCWC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Manticore",2,"1MRIWS,1MRIIG,1MRDMI,2MRCCS,2MRCMICROELEC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Manticore",3,"2MRIWS,2MRIIG,2MRDMI,5MRCCS,5MRCMICROELEC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Manticore",4,"4MRIWS,4MRIIG,4MRDMI,9MRCCS,9MRCMICROELEC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Manticore",5,"5MRIWS,5MRIIG,5MRDMI,12MRCCS,12MRCMICROELEC"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Takada",2,"1MRIWS,1MRIIG,1MRDMI,2MRCMICROELEC,2MRCOF"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Takada",3,"2MRIWS,2MRIIG,2MRDMI,5MRCMICROELEC,5MRCOF"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Takada",4,"4MRIWS,4MRIIG,4MRDMI,9MRCMICROELEC,9MRCOF"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"Takada",5,"5MRIWS,5MRIIG,5MRDMI,12MRCMICROELEC,12MRCOF"),

            #endregion

            #region Engineer mods - orignally from frontier data, excepting engineer list manually added, now since august 2024 from artie dump

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_reducedtoolbatteryconsumption","All","Reduced Tool Battery Consumption",500000,"3MRCEF,5MRCMICTRANS,8MRCEW,5MRDROR","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedbatterycapacity","All","Improved Battery Capacity",750000,"3MRCIB,5MRCMS,5MRCEW,5MRDROR,8MRDML","Wellington Beck,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedshieldregen","All","Faster Shield Regen",750000,"3MRCIB,8MRCMICTRANS,8MRCEW,5MRDROR","Kit Fowler,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_improvedarmourrating","All","Damage Resistance",750000,"3MRCTP,3MRCCFP,8MRCEA,5MRDWI,5MRDBALD","Jude Navarro,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedo2capacity","All","Increased Air Reserves",750000,"5MRCOXYBAC,8MRCPHN,3MRDPP,8MRDAQR","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_nightvision","All","Night Vision",1000000,"5MRISE,5MRCCIRSWITCH,3MRDSL,3MRDRD,3MRDNOCD","Oden Geiger,Yi Shen"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_improvedradar","All","Enhanced Tracking",750000,"3MRCTX,3MRCCB,5MRDTS,5MRDSAL,5MRDSAD","Domino Green,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_backpackcapacity","All","Extra Backpack Capacity",750000,"5MRCEA,3MRCMC,5MRDWI,5MRDCI,5MRDDD","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedammoreserves","All","Extra Ammo Capacity",750000,"3MRCWC,8MRDRL,5MRWTD,5MRDPRODREP","Kit Fowler,Jude Navarro,Eleanor Bresa"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_improvedjumpassist","All","Improved Jump Assist",750000,"5MRIGM,3MRCMT,5MRCMOTOR,5MRDTS","Yarden Bond,Hero Ferrari,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedsprintduration","All","Increased Sprint Duration",750000,"5MRCOXYBAC,8MRCCC,3MRDTDR,3MRDGSD,3MRDCTR","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_adsmovementspeed","All","Combat Movement Speed",750000,"5MRCEPINE,8MRCPHN,5MRDEP,3MRDGR","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_quieterfootsteps","All","Quieter Footsteps",1000000,"3MRCMH,8MRCVP,3MRDSAP,5MRDTACP,5MRDPATR","Yarden Bond,Yi Shen"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Suit,"suit_increasedmeleedamage","All","Added Melee Damage",500000,"5MRCEPINE,8MRCMT,5MRDCTM,5MRDCOMBPERF","Kit Fowler,Jude Navarro,Eleanor Bresa"),

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_suppression_pressurised","All","Noise Suppressor",1000000,"8MRCVP,3MRCWC,5MRDAD,5MRDMA","Terra Velasquez,Hero Ferrari,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_suppression_unpressurised","All","Audio Masking",1000000,"5MRCS,8MRCTX,3MRCCB,3MRDAL,5MRDPATR","Yarden Bond,Yi Shen"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_stability","All","Stability",500000,"5MRCVP,5MRCMH,5MRDMA,8MRDRA","Domino Green,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_handling","All","Faster Handling",500000,"3MRCVP,5MRDOM,5MRDCOMBPERF,5MRDCTM","Yarden Bond,Hero Ferrari,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_reloadspeed","All","Reload Speed",500000,"5MRCMH,5MRCE,5MRDOM,5MRDPRODREP,5MRDCTM","Jude Navarro,Uma Laszlo,Eleanor Bresa"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_clipsize","All","Magazine Size",750000,"3MRCWC,3MRCTC,5MRCMETALCOIL,5MRWTD,3MRSECEXP","Kit Fowler,Jude Navarro,Eleanor Bresa"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_scope","All","Scope",500000,"5MRCOL,3MRCOF,5MRDSAD,3MRDBIOD","Wellington Beck,Oden Geiger,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_backpackreloading","All","Stowed reloading",1000000,"3MRCCB,8MRCEMC,5MRDDD,5MRDOM,5MRDPS","Kit Fowler,Uma Laszlo,Eleanor Bresa"),

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_accuracy","Kinematic Armaments","Higher Accuracy: Kinematic Armaments",500000,"5MRCVP,5MRCR,5MRDEYD,3MRDBIOD,5MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_range","Kinematic Armaments","Greater Range: Kinematic Armaments",500000,"5MRCMETALCOIL,5MRCR,3MRCWC,5MRDBALD,5MRDTS","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_headshotdamage","Kinematic Armaments","Headshot damage: Kinematic Armaments",500000,"5MRCCC,8MRCR,3MRCWC,5MRWTD,3MRDMR","Uma Laszlo,Yi Shen"),

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_accuracy","Manticore","Higher Accuracy: Kinematic Armaments",500000,"5MRCCC,5MRCE,5MRCMETALCOIL,3MRDCHEMPAT,5MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_range","Manticore","Greater Range: Kinematic Armaments",500000,"3MRCMOTOR,5MRCE,3MRCEF,5MRDCF,8MRDMS","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_headshotdamage","Manticore","Headshot damage: Kinematic Armaments",500000,"5MRCIB,5MRCE,8MRCMS,5MRDCED,3MRDBTR","Uma Laszlo,Yi Shen"),

            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_accuracy","Takada","Higher Accuracy: Kinematic Armaments",500000,"5MRCMETALCOIL,3MRCOL,8MRCEW,3MRDRD,5MRDCOMBPERF","Yarden Bond,Terra Velasquez,Baltanos"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_range","Takada","Greater Range: Kinematic Armaments",500000,"8MRCMICTRANS,3MRCOL,3MRCCB,5MRDSAL,8MRDRA","Domino Green,Wellington Beck,Rosa Dayette"),
            new EngineeringRecipe(ItemData.ShipModule.ModuleTypes.Weapon,"weapon_headshotdamage","Takada","Headshot damage: Kinematic Armaments",750000,"5MRCIB,3MRCOL,5MRCS,5MRDSAD,3MRDBIOD","Uma Laszlo,Yi Shen"),

            #endregion



        };

        public static Dictionary<MaterialCommodityMicroResourceType, List<EngineeringRecipe>> EngineeringRecipesByMaterial =
            EngineeringRecipes.SelectMany(r => r.Ingredients.Select(i => new { mat = i, recipe = r }))
                              .GroupBy(a => a.mat)
                              .ToDictionary(g => g.Key, g => g.Select(a => a.recipe).ToList());

    }
}