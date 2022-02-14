/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EliteDangerousCore.DLL
{
    public partial class EDDDLLManager
    {
        // present and allow alloweddisallowed string to be edited. null if cancel
        public static string DLLPermissionManager(Form form, Icon icon, string alloweddisallowed)
        {
            string[] allowedfiles = alloweddisallowed.Split(',');

            ExtendedControls.ConfigurableForm f = new ExtendedControls.ConfigurableForm();

            int width = 400;
            int margin = 20;
            int vpos = 40;

            foreach (string setting in allowedfiles)
            {
                if (setting.Length >= 2)    // double check
                {
                    string name = setting.Substring(1);
                    f.Add(new ExtendedControls.ConfigurableForm.Entry(name, typeof(ExtendedControls.ExtCheckBox), name, new Point(margin, vpos), new Size(width - margin - 20, 20), null) { checkboxchecked = setting[0] == '+' });
                    vpos += 30;
                }
            }

            vpos += 20;
            f.Add(new ExtendedControls.ConfigurableForm.Entry("CALL", typeof(ExtendedControls.ExtButton), "Remove All".Tx(), new Point(margin, vpos), new Size(100, 20), null));
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
            else if (res == DialogResult.Abort)
            {
                return "";
            }
            else
                return null;
        }

        // present and allow alloweddisallowed string to be edited. null if cancel
        public void DLLConfigure(Form form, Icon icon, Func<string, string> getconfig, Action<string, string> setconfig)
        {
            ExtendedControls.ConfigurableForm f = new ExtendedControls.ConfigurableForm();

            int width = 300;
            int margin = 10;
            int vpos = 40;

            bool hasconfig = false;

            foreach (var dll in DLLs)
            {
                if (dll.HasConfig())
                {
                    f.Add(new ExtendedControls.ConfigurableForm.Entry(dll.Name, typeof(ExtendedControls.ExtButton), dll.Name, new Point(margin, vpos), new Size(width - margin - 20, 20), null) );
                    vpos += 30;
                    hasconfig = true;
                }
            }

            if (!hasconfig)
            {
                f.Add(new ExtendedControls.ConfigurableForm.Entry("label", typeof(Label), "No DLLs support configuration", new Point(margin, vpos), new Size(width - margin - 20, 20), null));
                vpos += 30;
            }

            vpos += 20;
            f.AddCancel(new Point(width - margin - 100, vpos), "Cancel".Tx());

            f.Trigger += (dialogname, controlname, xtag) =>
            {
                if (controlname == "OK")
                {
                    f.ReturnResult(DialogResult.OK);
                }
                else if (controlname == "Cancel" || controlname == "Close")
                {
                    f.ReturnResult(DialogResult.Cancel);
                }
                else
                {
                    var dllcaller = FindCaller(controlname);
                    string res = dllcaller.Config(getconfig(controlname),true);     // edit config
                    setconfig(controlname, res);
                    f.ReturnResult(DialogResult.Cancel);
                }
            };

            f.ShowDialogCentred(form, icon, "Configure ", closeicon: true);
        }


    }
}

