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

//define VANITYADD

using System;
using System.Collections.Generic;
using System.Linq;
using BaseUtils;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        public static void Initialise()
        {
            CreateModules();

            TranslateModules();

            AddExtraShipInfo();

            // this ensures all is checked - use it against example.ex and check translator-ids.log
            //foreach (var m in GetShipModules(true, true, true, true, true)) { string s = $"{m.Key} = {m.Value.TranslatedModName} {m.Value.TranslatedModTypeString}";  }

            // for translator example.ex

            //foreach (ShipSlots.Slot x in Enum.GetValues(typeof(ShipSlots.Slot))) System.Diagnostics.Debug.WriteLine($".{x}: {ShipSlots.ToEnglish(x).AlwaysQuoteString()} @");

            //foreach ( StationDefinitions.StationServices x in Enum.GetValues(typeof(StationDefinitions.StationServices))) System.Diagnostics.Debug.WriteLine($".{x}: {StationDefinitions.ToEnglish(x).AlwaysQuoteString()} @");

#if VANITYADD
            string infile = @"c:\code\newvanity.lst";
            if (System.IO.File.Exists(infile))
            {
                string[] toadd = System.IO.File.ReadAllLines(infile);
                bool changedvms = false;

                foreach (var line in toadd)
                {
                    if (line.Contains("new ShipModule"))
                    {
                        if (line.Contains("{"))
                        {
                            StringParser sp = new StringParser(line.Substring(line.IndexOf("{") + 1));
                            string id = sp.NextQuotedWordComma();
                            if (id != null)
                            {
                                sp.SkipUntil(new char[] { '(' });
                                sp.MoveOn(1);
                                int? fid = sp.NextIntComma(" ,");
                                string type = sp.NextWordComma();
                                string text = sp.NextQuotedWord();
                                System.Diagnostics.Debug.Assert(text != null && type != null && fid != null);

                                ShipModule.ModuleTypes modtype = (ShipModule.ModuleTypes)Enum.Parse(typeof(ShipModule.ModuleTypes), type.Substring(type.LastIndexOf(".") + 1));

                                ShipModule sm = new ShipModule(fid.Value, modtype, GenerateCandidateModuleName(text));

                                if (!vanitymodules.ContainsKey(id) && !shipmodules.ContainsKey(id) && !othershipmodules.ContainsKey(id))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Added new module {fid.Value}, {sm.ModuleID} {sm.ModType} {sm.EnglishModName}");
                                    vanitymodules[id] = sm;
                                    changedvms = true;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Module exists {fid.Value}, {sm.ModuleID} {sm.ModType} {sm.EnglishModName}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Bad line {line}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Bad line missing {{ {line}");
                        }
                    }
                    else if ( line.Trim().Length>0)
                        System.Diagnostics.Debug.WriteLine($"No data on this line {line}");

                }

                List<string> keylist = vanitymodules.Keys.ToList();
                foreach (var kc in keylist)
                    System.Diagnostics.Debug.Assert(!keylist.Contains(" "));        // just check no spaces are in the IDs due to grep replace

                foreach (string id in keylist)
                {
                    // ids ending in _00 always seem to come in 6's so make sure all are there

                    if (id.Length > 3 && id[id.Length - 3] == '_' && char.IsDigit(id[id.Length - 2]) && char.IsDigit(id[id.Length - 1]))
                    {
                        for (int i = 1; i <= 6; i++)
                        {
                            string newid = id.Substring(0, id.Length - 1) + i.ToStringInvariant();
                            if (!vanitymodules.ContainsKey(newid))
                            {
                                string text = vanitymodules[id].EnglishModName;
                                ShipModule sm2 = new ShipModule(vanitymodules[id].ModuleID, vanitymodules[id].ModType, text.Substring(0, text.Length - 1) + i.ToStringInvariant());
                                System.Diagnostics.Debug.WriteLine($"Added estimated module {newid}");
                                vanitymodules.Add(newid, sm2);
                                changedvms = true;
                            }
                        }
                    }

                    // These always seem to have 4 of them

                    string[] checklist4 = new string[] { "_bumper", "_spoiler", "_tail", "_wings" };
                    foreach (var cl4 in checklist4)
                    {
                        int pos = id.IndexOf(cl4);
                        if (pos > 0 && pos+cl4.Length == id.Length-1)
                        {
                            for (int i = 1; i <= 4; i++)
                            {
                                string newid = id.Substring(0, id.Length - 1) + i.ToStringInvariant();
                                if (!vanitymodules.ContainsKey(newid))
                                {
                                    string text = vanitymodules[id].EnglishModName;
                                    ShipModule sm2 = new ShipModule(vanitymodules[id].ModuleID, vanitymodules[id].ModType, text.Substring(0, text.Length - 1) + i.ToStringInvariant());
                                    System.Diagnostics.Debug.WriteLine($"Added estimated module {newid}");
                                    vanitymodules.Add(newid, sm2);
                                    changedvms = true;
                                }
                            }


                        }
                    }
                }

                foreach( var vm in vanitymodules)
                {
                    string org = vm.Value.EnglishModName;
                    string text = GenerateCandidateModuleName(org);
                    if (text != org)
                    {
                        System.Diagnostics.Debug.WriteLine($"*** Want to modify {org} -> {text}");
                        vm.Value.EnglishModName = text;
                        changedvms = true;
                    }
                }

                if (changedvms)
                {
                    System.Diagnostics.Trace.WriteLine($"*** NEW MODULES!");

                    List<string> vanitynames = vanitymodules.Keys.ToList();
                    vanitynames.Sort();

                    // output to file
                    string outfile = @"c:\code\vanity.lst";

                    string tout = "";
                    foreach (var key in vanitynames)
                        tout += $"                {{{key.AlwaysQuoteString()}, new ShipModule({vanitymodules[key].ModuleID},ShipModule.ModuleTypes.{vanitymodules[key].ModType},{vanitymodules[key].EnglishModName.AlwaysQuoteString()}) }},\r\n";
                    BaseUtils.FileHelpers.TryWriteToFile(outfile, tout);

                    // auto update cs file - this breaks the debugger note and causes it to notice text updates. Just ignore

                    string csfile = @"c:\code\eddiscovery2\elitedangerouscore\elitedangerous\items\itemmodules.cs";

                    if (System.IO.File.Exists(csfile))
                    {
                        string[] itemmodules = System.IO.File.ReadAllLines(csfile);
                        List<string> newfile = new List<string>();
                        bool done = false;
                        for (int i = 0; i < itemmodules.Length; i++)
                        {
                            if (!done && itemmodules[i].Contains("vanitymodules = new Dictionary"))
                            {
                                newfile.Add(itemmodules[i]);
                                newfile.Add("            {");
                                foreach (var keya in vanitynames)
                                    newfile.Add($"                {{{keya.AlwaysQuoteString()}, new ShipModule({vanitymodules[keya].ModuleID},ShipModule.ModuleTypes.{vanitymodules[keya].ModType},{vanitymodules[keya].EnglishModName.AlwaysQuoteString()}) }},");

                                while (!itemmodules[++i].Contains("};"))        // go to line with };
                                    ;

                                done = true;
                            }

                            newfile.Add(itemmodules[i]);
                        }

                        System.Diagnostics.Debug.Assert(done == true);
                        System.IO.File.WriteAllLines(csfile, newfile);

                        System.Diagnostics.Trace.WriteLine($"*** UPDATED CS FILE");
                    }
                }
            }

#endif

        }
    }
}
