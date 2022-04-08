/*
 * Copyright © 2015 - 2017 EDDiscovery development team
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
    public static class JournalFieldNaming
    {
        public static string FixCommodityName(string fdname)      // instances in log on mining and mission entries of commodities in this form, back into fd form
        {
            if (fdname.Length >= 8 && fdname.StartsWith("$") && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                fdname = fdname.Substring(1, fdname.Length - 7); // 1 for '$' plus 6 for '_name;'

            return fdname;
        }

        static public string FDNameTranslation(string old)
        {
            return MaterialCommodityMicroResourceType.FDNameTranslation(old);
        }

        static public string NormaliseMaterialCategory(string cat)
        {
            switch (cat.ToLowerInvariant())
            {
                case "raw":
                case "encoded":
                case "manufactured":
                    return cat;
                case "$microresource_category_encoded;":
                    return "Encoded";
                case "$microresource_category_elements;":
                    return "Raw";
                case "$microresource_category_manufactured;":
                    return "Manufactured";
            }

            // Fallback decoding
            if (cat.Contains("$"))
            {
                int i = cat.LastIndexOf('_');
                if (i != -1 && i < cat.Length - 1)
                    cat = cat.Substring(i + 1).Replace(";", "");
            }

            return cat;
        }

        public static string RLat(double? lv)
        {
            if (lv.HasValue)
                return RLat(lv.Value);
            else
                return null;
        }


        public static string RLat(double lv)      
        {
            return string.Format("{0:F4}°", lv);
        }

        public static string RLong(double? lv)
        {
            if (lv.HasValue)
                return RLong(lv.Value);
            else
                return null;
        }

        public static string RLong(double lv)      
        {
            return string.Format("{0:F4}°", lv);
        }

        static public string GetBetterMissionName(string inname)
        {
            return inname.Replace("_name", "").SplitCapsWordFull();
        }

        static public string ShortenMissionName(string inname)
        {
            return inname.Replace("Mission ", "", StringComparison.InvariantCultureIgnoreCase);
        }

        static Dictionary<string, string> replaceslots = new Dictionary<string, string>
        {
            {"Engines",     "Thrusters"},
        };

        static public string GetBetterSlotName(string s)
        {
            return s.SplitCapsWordFull(replaceslots);
        }

        static public string GetBetterItemName(string s)           
        {
            if (s.Length>0)         // accept empty string, some of the fields are purposely blank from the journal because they are not set for a particular transaction
            {
                ItemData.ShipModule item = ItemData.Instance.GetShipModuleProperties(s);
                return item.ModName;
            }
            else
                return s;
        }

        static public string GetBetterShipName(string inname)
        {
            ItemData.IModuleInfo i = ItemData.Instance.GetShipProperty(inname, ItemData.ShipPropID.Name);

            if (i != null)
                return (i as ItemData.ShipInfoString).Value;
            else
            {
                System.Diagnostics.Debug.WriteLine("Unknown FD ship ID:" + inname);
                return inname.SplitCapsWordFull();
            }
        }

        static public string GetBetterTargetTypeName(string s)      // has to deal with $ and underscored
        {
            //string x = s;
            if (s.StartsWith("$"))      // remove $ at start
                s = s.Substring(1);         
            if (s.EndsWith(";"))            // semi at end
                s = s.Substring(0,s.Length-1);
            return s.SplitCapsWordFull();
        }

        static public string NormaliseFDItemName(string s)      // has to deal with $int and $hpt.. This takes the FD name and keeps it, but turns it into the form
        {                                                       // used by Coriolis/Frontier API
            //string x = s;
            if (s.StartsWith("$int_"))
                s = s.Replace("$int_", "Int_");
            if (s.StartsWith("int_"))
                s = s.Replace("int_", "Int_");
            if (s.StartsWith("$hpt_"))
                s = s.Replace("$hpt_", "Hpt_");
            if (s.StartsWith("hpt_"))
                s = s.Replace("hpt_", "Hpt_");
            if (s.Contains("_armour_"))
                s = s.Replace("_armour_", "_Armour_");      // normalise to Armour upper cas.. its a bit over the place with case..
            if (s.EndsWith("_name;", StringComparison.InvariantCultureIgnoreCase))
            {
                //System.Diagnostics.Debug.WriteLine("Correct " + s);
                s = s.Substring(0, s.Length - 6);
            }
            if (s.StartsWith("$"))                          // seen instances of $python_armour..
                s = s.Substring(1);

            return s;
        }

        static public string NormaliseFDSlotName(string s)            // FD slot name, anything to do.. leave in as there might be in the future
        {
            return s;
        }

        static public string NormaliseBodyType(string s)            // FD slot name, anything to do.. leave in as there might be in the future
        {
            if (s == null)
                return "Unknown";
            else if (s.Equals("Null",StringComparison.InvariantCultureIgnoreCase))
                s = "Barycentre";
            return s;
        }

        static public string NormaliseFDShipName(string inname)            // FD ship names.. tend to change case.. Fix
        {
            ItemData.IModuleInfo i = ItemData.Instance.GetShipProperty(inname, ItemData.ShipPropID.FDID);
            if (i != null)
                return (i as ItemData.ShipInfoString).Value;
            else
            {
                System.Diagnostics.Debug.WriteLine("Unknown FD ship ID:" + inname);
                return inname;
            }
        }

        static public string CheckLocalisation(string loc, string alt)      
        {
            if ( alt != null  )  // no point if alt is null
            { 
                bool invalid = loc == null || loc.Length < 2 || loc.StartsWith("$int", StringComparison.InvariantCultureIgnoreCase) || loc.StartsWith("$hpt", StringComparison.InvariantCultureIgnoreCase) ||
                                  (loc.StartsWith("$") && loc.EndsWith(";"));

                if (invalid)
                {
                    if (alt.Length > 0)
                    {
                        if (alt.StartsWith("$") && alt.EndsWith(";")) // identifier
                        {
                            alt = alt.Substring(1, alt.Length - 2).SplitCapsWordFull();
                           // System.IO.File.AppendAllText(@"c:\code\loc.txt", $"Substitute identifier loc '{loc}' for '{alt}'\r\n");
                        }
                        else if (alt.StartsWith("HPT_", StringComparison.InvariantCultureIgnoreCase) || alt.StartsWith("INT_", StringComparison.InvariantCultureIgnoreCase))
                        {
                            alt = alt.Substring(4).SplitCapsWordFull();
                          //  System.IO.File.AppendAllText(@"c:\code\loc.txt", $"Substitute int/hpt loc '{loc}' for '{alt}'\r\n");
                        }
                        else
                        {
                           // System.IO.File.AppendAllText(@"c:\code\loc.txt", $"Substitute loc '{loc}' for '{alt}'\r\n");
                        }
                    }

                    return alt;
                }
            }
            
            return loc;
        }
        
        static public string CheckLocalisationTranslation(string loc, string alt)      
        {
            if (BaseUtils.Translator.Instance.Translating)          // if we are translating, use the alt name as its the most valid..
                return alt;
            else
                return CheckLocalisation(loc, alt);
        }

        public static string SubsituteCommanderName(string cmdrin)      // only for debugging, subsitute a commander name
        {
            return cmdrin;
        }

        public static string SubsituteCommanderFID(string cmdrin)       // only for debugging, subsitute a commander name
        {
            return cmdrin;
        }

        public static string SecondsToDHMString(this int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return string.Format("{0} days {1} hours {2} minutes".T(EDCTx.JournalEntry_TME), time.Days, time.Hours, time.Minutes);
        }
    }
}
