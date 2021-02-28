using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EliteDangerousCore.DLL
{
    public static class DLLPermissionManager
    {
        // present and allow alloweddisallowed string to be edited. null if cancel

        public static string ShowDialog(Form form, Icon icon, string alloweddisallowed)
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

            f.AddOK(new Point(width - margin - 100, vpos), "OK".Tx());
            f.AddCancel(new Point(width - margin - 200, vpos), "Cancel".Tx());

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
            };

            if (f.ShowDialogCentred(form, icon, "DLL", closeicon: true) == DialogResult.OK)
            {
                alloweddisallowed = "";
                foreach (var e in f.Entries.Where(x => x.controltype == typeof(ExtendedControls.ExtCheckBox)))
                    alloweddisallowed = alloweddisallowed.AppendPrePad((f.Get(e.controlname) == "1" ? "+" : "-") + e.controlname, ",");

                return alloweddisallowed;
            }
            else
                return null;
        }
    }
}
