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
    [System.Diagnostics.DebuggerDisplay("Syn {FDName.Str()} {Level} `{Name}` {string.Join(\",\",Ingredients)}")]

    public class SynthesisRecipe : RecipeBase
    {
        public SynthesisRecipeFDName FDName { get; private set; }        // "Repair Basic", "Fuel Basic"
        public enum SynthesisLevel { Basic, Standard, Premium }
        public SynthesisLevel Level { get; set; }

        public SynthesisRecipe(string name, string fdname, SynthesisLevel level, string indg) : base(name, indg)
        {
            Level = level;
            FDName = new SynthesisRecipeFDName(fdname ?? "Unknown");
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

        public static SynthesisRecipe FindSynthesisFDName(SynthesisRecipeFDName recipename)
        {
            return synthesisRecipes.Find(x => x.FDName.Equals(recipename));
        }
        public static SynthesisRecipe FindSynthesisFDName(SynthesisRecipeFDName recipename, SynthesisRecipe.SynthesisLevel level)
        {
            return synthesisRecipes.Find(x => x.FDName.Equals(recipename) && x.Level.Equals(level));
        }


        // ones which players can synthesise - all at the moment (reflecting engineeering recipes)
        public static List<SynthesisRecipe> GetPlayerSynthesis()
        {
            return synthesisRecipes;
        }

        public static List<SynthesisRecipe> FindSynthesis(Dictionary<MCFDName, int> mats)
        {
            List<SynthesisRecipe> ret = new List<SynthesisRecipe>();

            foreach (var s in synthesisRecipes)
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

        private static List<SynthesisRecipe> synthesisRecipes = new List<SynthesisRecipe>()
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
            synthesisRecipes.SelectMany(r => r.Ingredients.Select(i => new { mat = i, recipe = r }))
                            .GroupBy(a => a.mat)
                            .ToDictionary(g => g.Key, g => g.Select(a => a.recipe).ToList());


    }
}