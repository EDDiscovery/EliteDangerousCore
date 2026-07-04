/*
 * Copyright © 2015 - 2024 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public static class JournalFieldNaming
    {
        // ensure good fdname


        static public FDName FDNameTranslation(FDName fdname)
        {
            return MaterialCommodityMicroResourceType.FDNameTranslation(fdname);
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

        static public string GetBetterMissionName(string inname)
        {
            return inname.Replace("_name", "").SplitCapsWordFull();
        }

        static public string ShortenMissionName(string inname)
        {
            return inname.Replace("Mission ", "", StringComparison.InvariantCultureIgnoreCase);
        }

        static public string GetBetterEnglishModuleName(FDName fdid, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)           
        {
            // screen out empty string, some of the fields are purposely blank from the journal because they are not set for a particular transaction
            // do create as this is used by Loadout, ModuleBuy

            if (fdid?.Valid == true && ItemData.TryGetShipModule(fdid, out ItemData.ShipModule item, true, slot))
            {
                return item.EnglishModName;
            }
            else
                return fdid.Str();
        }

        static public string GetForeignModuleName(FDName fdid, string localised, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)
        {
            if (fdid.Valid && ItemData.TryGetShipModule(fdid, out ItemData.ShipModule item, true , slot))
            {
                return item.TranslatedModName;
            }
            else
                return localised ?? fdid.Str();
        }

        static public string GetForeignModuleType(FDName fdid)
        {
            if (fdid.Valid && ItemData.TryGetShipModule(fdid, out ItemData.ShipModule item, true))
            {
                return item.TranslatedModTypeString();
            }
            else
                return "Unknown";
        }


        // use when an identifier should be a ship
        static public string GetBetterShipName(FDName inname)
        {
            if (inname?.Valid == false)
                return "No Ship Name Given";

            string i = ItemData.GetShipName(inname);

            if (i != null)
            {
                return i;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("*** Unknown Ship Name:" + inname);
                return inname.SplitCapsWordFull();
            }
        }

        // use when an identifier could be a ship, an actor or a suit
        static public string GetBetterShipSuitActorName(FDName inname)
        {
            if (!inname.Valid)
                return "No Ship/Actor/Suit Name Given";

            string i = ItemData.GetShipName(inname);

            if (i != null)
            {
                return i;
            }
            else if (ItemData.IsActor(inname))
            {
                string n = ItemData.GetActor(inname).Name;
                return n;
            }
            else if (ItemData.IsSuit(inname))
            {
                return ItemData.GetSuit(inname).Name;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown Ship/Suit/Actor: {{ \"{inname}\", new Actor(\"{inname.SplitCapsWordFull()}\") }},");
                return inname.SplitCapsWordFull();
            }
        }

        // use when you know its a ship


        static public string GetBetterTargetTypeName(string s)      // has to deal with $ and underscored
        {
            //string x = s;
            if (s.StartsWith("$"))      // remove $ at start
                s = s.Substring(1);         
            if (s.EndsWith(";"))            // semi at end
                s = s.Substring(0,s.Length-1);
            return s.SplitCapsWordFull();
        }

        static public Tuple<string, string> GetStationNames(JObject evt, string root = "StationName")
        {
            var sn = evt[root].Str();
            var snloc = evt[root+"_Localised"].StrNull();
            if (snloc == null)
            {
                string cs = "$EXT_PANEL_ColonisationShip;";
                if (sn.StartsWith(cs))
                {
                    snloc = sn.Substring(cs.Length).Trim();
                    //System.Diagnostics.Debug.WriteLine($"Station Name `{sn}` loc `{snloc}`");
                }
                else
                {
                    snloc = sn;
                    if (sn.Contains("$"))
                        System.Diagnostics.Debug.WriteLine($"Localisation of Station Name `{sn}` Failed");
                }
            }
            return new Tuple<string, string>(sn, snloc);
        }
        static public Tuple<string, string> GetStationNames(JObject evt)
        {
            var sn = evt["StationName"].Str();
            var snloc = evt["StationName_Localised"].StrNull();
            if (snloc == null)
            {
                string cs = "$EXT_PANEL_ColonisationShip;";
                if (sn.StartsWith(cs))
                {
                    snloc = sn.Substring(cs.Length).Trim();
                    //System.Diagnostics.Debug.WriteLine($"Station Name `{sn}` loc `{snloc}`");
                }
                else
                {
                    snloc = sn;
                    if (sn.Contains("$"))
                        System.Diagnostics.Debug.WriteLine($"Localisation of Station Name `{sn}` Failed");
                }
            }
            return new Tuple<string, string>(sn, snloc);
        }

        static public string CheckLocalisation(string loc, string alt)      
        {
            if ( alt != null  )  // no point if alt is null
            { 
                bool invalid = loc.IsEmpty() || loc.StartsWith("$int", StringComparison.InvariantCultureIgnoreCase) || loc.StartsWith("$hpt", StringComparison.InvariantCultureIgnoreCase) ||
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

            return loc != null ? loc.Replace("&nbsp;", " ") : loc; //Frontier returns spaces as separator as &nbsp; so let's replace it to make it readable
            
        }
        
        static public string CheckLocalisationTranslation(string loc, string alt)      
        {
            if (BaseUtils.TranslatorMkII.Instance.Translating)          // if we are translating, use the alt name as its the most valid..
                return alt;
            else
                return CheckLocalisation(loc, alt);
        }

        // attempt to find a better name for name as its a body name
        static public string SignalBodyName(string name)
        {
            var res = Identifiers.Get(name);

            if (res.StartsWith("$SAA_RingHotspot",StringComparison.InvariantCultureIgnoreCase))        // if still id
            {
                int indexof = res.IndexOf("#type=");
                if ( indexof>0 && res.Length > indexof+6)
                {
                    string mintype = res.Substring(indexof + 6).Replace(";", "").Replace("_name","").Replace("$", "");
                    var mcd = MaterialCommodityMicroResourceType.GetByFDName(new FDName(mintype));
                    if (mcd != null)    // if we find it, translate it, else leave it alone
                        mintype = mcd.TranslatedName;

                    res = "Ring Hot Spot of type ".Tx()+ mintype;
                }
            }
            return res;
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
            return string.Format("{0} days {1} hours {2} minutes".Tx(), time.Days, time.Hours, time.Minutes);
        }

        public static string DockingDeniedReason(string fdname)
        {
            return fdname.SplitCapsWordFull();
        }
        public static string CrimeType(string fdname)
        {
            return Crimes.ToEnglish(fdname);
        }
        public static string RedeemVoucherType(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string CrewRole(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string ModulePackOperation(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string ShipPackOperation(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string DataLinkType(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string ResurrectOption(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string Signals(string fdname)
        {
            return fdname?.Replace("$SAA_SignalType_", "").Replace(";", "").SplitCapsWordFull() ?? null;
        }
        public static string BodySignals(string fdname)
        {
            return fdname?.Replace("$SAA_SignalType_", "").Replace(";", "").SplitCapsWordFull() ?? null;
        }
        public static string Genus(string fdname)
        {
            return fdname?.Replace("$Codex_Ent_", "").Replace("_Name;", "").Replace(";", "").Replace("$Codex_", "").SplitCapsWordFull() ?? null;
        }
        public static string Blueprint(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string Synthesis(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string EngineerMods(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string PassengerType(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string RepairType(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string ScanType(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
        public static string SuitSlot(string fdname)
        {
            return fdname?.SplitCapsWordFull() ?? null;
        }
    }
}
