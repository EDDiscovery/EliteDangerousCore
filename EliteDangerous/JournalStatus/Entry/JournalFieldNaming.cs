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
        static public string ShortenMissionName(string inname)
        {
            return inname.Replace("Mission ", "", StringComparison.InvariantCultureIgnoreCase);
        }

        // handle changes in how Frontier writes station info
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

        // handle changes in how Frontier writes station info
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

    }
}
