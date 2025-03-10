/*
 * Copyright © 2015 - 2025 EDDiscovery development team
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

using EliteDangerousCore.DB;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EliteDangerousCore.DLL
{
    public partial class EDDDLLManager
    {
        // present and allow alloweddisallowed string to be edited. null if cancel
        public void DLLPermissionManager(Form form, Icon icon)
        {
            ExtendedControls.ConfigurableForm f = new ExtendedControls.ConfigurableForm();

            int width = 400;
            int margin = 20;
            int vpos = 40;

            foreach (string name in DLLPermissions.Keys)
            {
                f.Add(new ExtendedControls.ConfigurableEntryList.Entry(name, typeof(ExtendedControls.ExtCheckBox), name, new Point(margin, vpos), new Size(width - margin - 20, 20), null) { CheckBoxChecked = DLLPermissions[name] });
                vpos += 30;
            }

            vpos += 20;
            f.Add(new ExtendedControls.ConfigurableEntryList.Entry("CALL", typeof(ExtendedControls.ExtButton), "Remove All".TxID(EDCTx.RemoveAll), new Point(margin, vpos), new Size(100, 20), null));
            f.AddOK(new Point(width - margin - 100, vpos), "OK".TxID(EDCTx.OK));
            f.AddCancel(new Point(width - margin - 200, vpos), "Cancel".TxID(EDCTx.Cancel));

            f.Trigger += (dialogname, controlname, xtag) =>
            {
                if (controlname == "OK")
                {
                    f.ReturnResult(DialogResult.OK);
                }
                else if (controlname == "CALL")
                {
                    RemoveAllDLLPermissions();
                    f.ReturnResult(DialogResult.Cancel);
                }
                else if (controlname == "Cancel" || controlname == "Close")
                {
                    f.ReturnResult(DialogResult.Cancel);
                }
            };

            var res = f.ShowDialogCentred(form, icon, "DLL - applies at next restart", closeicon: true);
            if (res == DialogResult.OK)
            {
                foreach (var e in f.Entries.Entries.Where(x => x.ControlType == typeof(ExtendedControls.ExtCheckBox)))
                {
                    SetDLLPermission(e.Name, f.Get(e.Name) == "1");
                }
            }
        }

        // present and allow alloweddisallowed string to be edited. null if cancel
        public void DLLConfigure(Form form, Icon icon)
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
                    f.Add(new ExtendedControls.ConfigurableEntryList.Entry(dll.Name, typeof(ExtendedControls.ExtButton), dll.Name, new Point(margin, vpos), new Size(width - margin - 20, 20), null) );
                    vpos += 30;
                    hasconfig = true;
                }
            }

            if (!hasconfig)
            {
                f.Add(new ExtendedControls.ConfigurableEntryList.Entry("label", typeof(Label), "No DLLs support configuration", new Point(margin, vpos), new Size(width - margin - 20, 20), null));
                vpos += 30;
            }

            vpos += 20;
            f.AddCancel(new Point(width - margin - 100, vpos), "Cancel".TxID(EDCTx.Cancel));

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
                    string res = dllcaller.Config(UserDatabase.Instance.GetSettingString("DLLConfig_" + controlname,""),true);     // edit config
                    UserDatabase.Instance.PutSettingString("DLLConfig_" + controlname, res);
                    f.ReturnResult(DialogResult.Cancel);
                }
            };

            f.ShowDialogCentred(form, icon, "Configure ", closeicon: true);
        }


    }
}

