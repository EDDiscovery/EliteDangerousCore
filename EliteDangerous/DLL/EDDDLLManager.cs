/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EliteDangerousCore.DLL
{
    public class EDDDLLManager
    {
        public int Count { get { return DLLs.Count; } }
        public List<EDDDLLCaller> DLLs { get; private set; } = new List<EDDDLLCaller>();

        // search directory for *.dll, 
        // return loaded, failed, new dlls not in the allowed/disallowed list, disabled
        // alloweddisallowed list is +allowed,-disallowed.. 
        // all Csharp assembly DLLs are loaded - only ones implementing *EDDClass class causes it to be added to the DLL list
        // only normal DLLs implementing EDDInitialise are kept loaded

        public Tuple<string, string, string,string> Load(string[] dlldirectories, bool[] disallowautomatically, string ourversion, string[] inoptions, 
                                EDDDLLInterfaces.EDDDLLIF.EDDCallBacks callbacks, ref string alloweddisallowed)
        {
            string loaded = "";
            string failed = "";
            string disabled = "";
            string newdlls = "";

            for (int i = 0 ; i < dlldirectories.Length; i++)
            {
                var dlldirectory = dlldirectories[i];

                if (dlldirectory != null)
                {
                    if (!Directory.Exists(dlldirectory))
                        failed = failed.AppendPrePad("DLL Folder " + dlldirectory + " does not exist", ",");
                    else
                    {
                        FileInfo[] allFiles = Directory.EnumerateFiles(dlldirectory, "*.dll", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

                        string[] allowedfiles = alloweddisallowed.Split(',');

                        foreach (FileInfo f in allFiles)
                        {
                            EDDDLLCaller caller = new EDDDLLCaller();

                            System.Diagnostics.Debug.WriteLine("Try to load " + f.FullName);

                            string filename = System.IO.Path.GetFileNameWithoutExtension(f.FullName);

                            bool isallowed = alloweddisallowed.Equals("All", StringComparison.InvariantCultureIgnoreCase) ||
                                             allowedfiles.Contains("+" + f.FullName, StringComparer.InvariantCultureIgnoreCase) ||      // full name is now used
                                             allowedfiles.Contains("+" + filename, StringComparer.InvariantCultureIgnoreCase);              // filename previously used

                            bool isdisallowed = allowedfiles.Contains("-" + f.FullName, StringComparer.InvariantCultureIgnoreCase) ||      // full name is now used
                                                allowedfiles.Contains("-" + filename, StringComparer.InvariantCultureIgnoreCase);              // filename previously used

                            if ( isdisallowed )     // disallowed
                            {
                                disabled = disabled.AppendPrePad(f.FullName, ",");
                            }
                            else if (isallowed)    // if allowed..
                            {
                                if (caller.Load(f.FullName))        // if loaded okay
                                {
                                    if (caller.Init(ourversion, inoptions, dlldirectory, callbacks))       // must init
                                    {
                                        DLLs.Add(caller);
                                        loaded = loaded.AppendPrePad(filename, ",");        // just use short name for reporting
                                    }
                                    else
                                    {
                                        string errstr = caller.Version.HasChars() ? (": " + caller.Version.Substring(1)) : "";
                                        failed = failed.AppendPrePad(f.FullName + errstr, ","); // long name for failure
                                    }
                                }
                            }
                            else
                            {       // not there
                                if (disallowautomatically[i])
                                {
                                    alloweddisallowed = alloweddisallowed.AppendPrePad("-" + f.FullName,",");
                                    disabled = disabled.AppendPrePad(f.FullName, ",");
                                }
                                else
                                {
                                    if (!newdlls.Contains(f.FullName))      // Eahlstan special - don't add twice to the newdlls in case we are scanning the same folder twice
                                    {
                                        newdlls = newdlls.AppendPrePad(f.FullName, ",");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new Tuple<string, string, string,string>(loaded, failed, newdlls,disabled);
        }

        public void UnLoad()
        {
            foreach (EDDDLLCaller caller in DLLs)
            {
                caller.UnLoad();
            }

            DLLs.Clear();
        }

        public void Refresh(string cmdr, EDDDLLInterfaces.EDDDLLIF.JournalEntry je)
        {
            foreach (EDDDLLCaller caller in DLLs)
            {
                caller.Refresh(cmdr, je);
            }
        }

        public void NewJournalEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry nje, bool stored)
        {
            foreach (EDDDLLCaller caller in DLLs)
            {
                caller.NewJournalEntry(nje, stored);
            }
        }

        public void NewUIEvent(string json)
        {
            foreach (EDDDLLCaller caller in DLLs)
            {
                caller.NewUIEvent(json);
            }
        }

        public EDDDLLCaller FindCaller(string name)
        {
            return DLLs.Find(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        // item1 = true if found, item2 = true if caller implements.
        public Tuple<bool, bool> ActionJournalEntry(string dllname, EDDDLLInterfaces.EDDDLLIF.JournalEntry nje)
        {
            if (dllname.Equals("All", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (EDDDLLCaller caller in DLLs)
                    caller.ActionJournalEntry(nje);

                return new Tuple<bool, bool>(true, true);
            }
            else
            {
                EDDDLLCaller caller = FindCaller(dllname);
                return caller != null ? new Tuple<bool, bool>(true, caller.ActionJournalEntry(nje)) : new Tuple<bool, bool>(false, false);
            }
        }

        // List of DLL results, empty if no DLLs were found
        // else list of results. bool = true no error, false error.  String contains error string, or result string
        public List<Tuple<bool, string, string>> ActionCommand(string dllname, string cmd, string[] paras)
        {
            List<Tuple<bool, string, string>> resultlist = new List<Tuple<bool, string, string>>();

            if (dllname.Equals("All", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (EDDDLLCaller caller in DLLs)
                    resultlist.Add(AC(caller, cmd, paras));
            }
            else
            {
                EDDDLLCaller caller = FindCaller(dllname);
                if (caller != null)
                    resultlist.Add(AC(caller, cmd, paras));
                else
                    resultlist.Add(new Tuple<bool, string, string>(false, dllname, "Cannot find DLL "));
            }

            return resultlist;
        }

        private Tuple<bool, string, string> AC(EDDDLLCaller caller, string cmd, string[] paras)
        {
            string r = caller.ActionCommand(cmd, paras);
            if (r == null)
                return new Tuple<bool, string, string>(false, caller.Name, "DLL does not implement ActionCommand");
            else if (r.Length > 0 && r[0] == '+')
                return new Tuple<bool, string, string>(true, caller.Name, r.Mid(1));
            else
                return new Tuple<bool, string, string>(false, caller.Name, r.Mid(1));
        }


        // present and allow alloweddisallowed string to be edited. null if cancel

        public static string DLLPermissionManager(Form form, Icon icon, string alloweddisallowed)
        {
            string[] allowedfiles = alloweddisallowed.Split(',');

            ExtendedControls.ConfigurableForm f = new ExtendedControls.ConfigurableForm();

            int width = 400;
            int margin = 20;
            int vpos = 30;

            foreach (string setting in allowedfiles)
            {
                if (setting.Length >= 2)    // double check
                {
                    string name = setting.Substring(1);
                    f.Add(new ExtendedControls.ConfigurableForm.Entry(name, typeof(ExtendedControls.ExtCheckBox), name, new Point(margin, vpos), new Size(width - margin - 20, 20), null) { checkboxchecked = setting[0] == '+' });
                    vpos += 30;
                }
            }

            f.Add(new ExtendedControls.ConfigurableForm.Entry("CALL", typeof(ExtendedControls.ExtButton), "Remove All".Tx(), new Point(margin, vpos), new Size(100, 20),null));
            f.AddOK(new Point(width - margin - 100, vpos), "OK".Tx());
            f.AddCancel(new Point(width - margin - 200, vpos), "Cancel".Tx());

            f.Trigger += (dialogname, controlname, xtag) =>
            {
                if (controlname == "OK")
                {
                    f.ReturnResult(DialogResult.OK);
                }
                else if (controlname == "CALL")
                {
                    f.ReturnResult(DialogResult.Abort);
                }
                else if (controlname == "Cancel" || controlname == "Close")
                {
                    f.ReturnResult(DialogResult.Cancel);
                }
            };

            var res = f.ShowDialogCentred(form, icon, "DLL - applies at next restart", closeicon: true);
            if (res == DialogResult.OK)
            {
                alloweddisallowed = "";
                foreach (var e in f.Entries.Where(x => x.controltype == typeof(ExtendedControls.ExtCheckBox)))
                    alloweddisallowed = alloweddisallowed.AppendPrePad((f.Get(e.controlname) == "1" ? "+" : "-") + e.controlname, ",");

                return alloweddisallowed;
            }
            else if ( res == DialogResult.Abort)
            {
                return "";
            }
            else
                return null;
        }


    }
}

