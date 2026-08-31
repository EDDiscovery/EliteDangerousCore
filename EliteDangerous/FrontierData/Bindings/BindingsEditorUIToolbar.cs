/*
 * Copyright 2026-2026 EDDiscovery development team
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

using BaseUtils;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    public partial class BindingsEditor : UserControl
    {
        #region UI Bar

        private void ExtComboBoxBindFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            extComboBoxBindFiles.SelectedIndexChanged -= ExtComboBoxBindFiles_SelectedIndexChanged;
            var list = extComboBoxBindFiles.Tag as List<string>;

            if (CheckAskDirty())
            {
                string fullpathnewfile = list[extComboBoxBindFiles.SelectedIndex];

                if (!File.Exists(bf.FileName))     // remove if not saved and does not exist on file
                {
                    extComboBoxBindFiles.Items.Remove(Path.GetFileName(bf.FileName));
                    list.Remove(bf.FileName);
                }

                bf = new BindingsFile(otherdevicesknown);
                bf.Read(fullpathnewfile);

                extComboBoxBindFiles.SelectedIndex = list.IndexOf(fullpathnewfile);
                SetEnables(true, false);
                Display();
                ClearDirty();
                updatecheck.Start();        // start the clock in case it was stopped
            }
            else
            {
                extComboBoxBindFiles.SelectedIndex = list.IndexOf(bf.FileName);
            }

            extComboBoxBindFiles.SelectedIndexChanged += ExtComboBoxBindFiles_SelectedIndexChanged;
        }

        private void ExtComboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void extButtonReload_Click(object sender, EventArgs e)
        {
            if (CheckAskDirty())
            {
                string curfile = bf.FileName;
                bf = new BindingsFile(otherdevicesknown);
                bf.Read(curfile);
                Display();
                ClearDirty();

            }
        }

        private void ApplyFilter()
        {
            dataGridView.FilterGridView((r) => extComboBoxFilter.SelectedIndex == 0 || 
                ((string)r.Cells[extComboBoxFilter.SelectedIndex >= filtercomboboxmodestart ? ColUI.Index : ColGroup.Index].Value) == (string)extComboBoxFilter.SelectedItem);
        }


        private void extButtonSave_Click(object sender, EventArgs e)
        {
            if (ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} overwrite (a .Bak will be created), please confirm", "Replace bindings", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                if (bf.Save(true))
                {
                    ClearDirty();
                    ChangedBindings?.Invoke(bf.FileName);           // tell someone we edited the bindings
                    updatecheck.Start();                        // start the update check again if it was paused because a user did not want to write
                }
                else
                {
                    ExtendedControls.MessageBoxTheme.Show(this, $"{bf.FileName} cannot be written to", "Failed to write file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void extButtonDuplicate_Click(object sender, EventArgs e)
        {
            string newname = ExtendedControls.PromptSingleLine.ShowDialog(this, "Name:", "", "Duplicate", this.FindForm().Icon);
            if (newname != null)
            {
                extComboBoxBindFiles.SelectedIndexChanged -= ExtComboBoxBindFiles_SelectedIndexChanged;
                var list = extComboBoxBindFiles.Tag as List<string>;
                int index = list.IndexOf(bf.FileName);
                bf.Rename(newname);
                list.Add(bf.FileName);
                extComboBoxBindFiles.Items.Add(Path.GetFileName(bf.FileName));      // add to list in memory and in control at end
                extComboBoxBindFiles.SelectedIndex = extComboBoxBindFiles.Items.Count - 1;
                SetDirty();
                extComboBoxBindFiles.SelectedIndexChanged += ExtComboBoxBindFiles_SelectedIndexChanged;
            }
        }

        private void extButtonSetDefault_Click(object sender, EventArgs e)
        {
            if (bf.IsLoaded)
            {
                if (ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} will become the default for all sections\r\nConfirm Selection", "Set Default binding file", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    string folder = Path.GetDirectoryName(bf.FileName);
                    var presetfile = BindingsFile.FindStartPreset(folder, true);

                    if (presetfile == null)
                    {
                        ExtendedControls.MessageBoxTheme.Show(this, $"No StartPreset file present, creating new file", "Set Default binding file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        presetfile = Tuple.Create(Path.Combine(folder, "StartPreset.4.start"), DateTime.UtcNow, 4);
                    }

                    if (!BaseUtils.FileHelpers.TryWriteToFile(presetfile.Item1, string.Format("{0}\r\n{0}\r\n{0}\r\n{0}\r\n", bf.PresetName)))
                    {
                        ExtendedControls.MessageBoxTheme.Show(this, $"Could not write to preset file {presetfile.Item1}", "Set Default binding file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    ChangedDefault?.Invoke(bf.FileName);           // tell someone we edited the bindings
                }
            }
            else
                ExtendedControls.MessageBoxTheme.Show(this, $"Bindings not loaded", "Set Default binding file", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void extButtonFolder_Click(object sender, EventArgs e)
        {
            Processes.Explorer(Path.GetDirectoryName(bf.FileName));
        }

        private void buttonNewDevice_Click(object sender, EventArgs e)
        {
            string s = ExtendedControls.PromptSingleLine.ShowDialog(this, "Device:", "", "Enter new device name", this.FindForm().Icon);
            if (s != null)
            {
                bf.AddDevice(s);
                Display();
            }
        }

        private void buttonDeviceRename_Click(object sender, EventArgs e)
        {
            ExtendedControls.CheckedIconNewListBoxForm displayfilter = new CheckedIconNewListBoxForm();
            var items = bf.DeviceListNoKeyboardMouseDevice;

            if (items.Count > 0)
            {
                foreach (var x in items)
                    displayfilter.UC.AddButton(x, BetterDevice(x));       // tag is internal name, text is rename

                displayfilter.CloseBoundaryRegion = new Size(32, extButtonDeviceRename.Height);
                displayfilter.UC.ImageSize = new Size(24, 24);
                displayfilter.UC.ScreenMargin = new Size(0, 0);
                displayfilter.PositionBelow(extButtonDeviceRename);
                displayfilter.UC.ButtonPressed += (i, s1, s2, o, m) =>      // called on click of button
                {
                    string newname = ExtendedControls.PromptSingleLine.ShowDialog(this, "Device:", "", $"Enter new device name for {s1}", this.FindForm().Icon);
                    if (newname != null)
                        bf.RenameDevice(s1, newname);

                    SetDirty();
                    Display();
                    displayfilter.Close();
                };
                displayfilter.Show(this);
            }
        }

        #endregion
    }
}
