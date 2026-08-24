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
using ExtendedConditionsForms;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static EliteDangerousCore.BindingsFile;

namespace EliteDangerousCore
{
    public partial class BindingsEditor : UserControl
    {
        public bool IsDirty => extButtonSave.Enabled;
        public Action<string> ChangedBindings { get; set; }           // saved this load
        public Action<string> ChangedDefault { get; set; }            // changed the start preset file
        public List<string> KnownDevices { get; set; } = null;        // add to list with any known devices. device name (optional comment) 

        // allows user to define translations between external frontier device names and ones use in here and bindings editor
        // add to this to handle other specialise device names.
        public Dictionary<string, string> ConvertDeviceNameList = new Dictionary<string, string>();

        public BindingsEditor()
        {
            InitializeComponent();
            ColValues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridView.MakeDoubleBuffered();

            showFrontierNamesToolStripMenuItem.Checked = false;
            showFrontierNamesToolStripMenuItem.Click += new System.EventHandler(this.showFrontierNamesToolStripMenuItem_Click);
        }


        // folder and preferredbindfile (can be null)
        public void Init(string folder, string preferredbindfile)
        {
            List<FileInfo> bindfiles = Directory.EnumerateFiles(folder, "*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToList();
            foreach (var x in bindfiles)
                extComboBoxBindFiles.Items.Add(x.Name);
            extComboBoxBindFiles.Tag = bindfiles.Select(x=>x.FullName).ToList();

            bf = new BindingsFile();

            foreach (var kvp in ConvertDeviceNameList)              // update its device name convert list
                bf.ConvertDeviceNameList[kvp.Key] = kvp.Value;

            if ( preferredbindfile != null) // if preferred bind file
                bf.Read(preferredbindfile);

            int index = bindfiles.FindIndex(x => x.FullName == preferredbindfile);
            if (index >= 0)
            {
                extComboBoxBindFiles.SelectedIndex = index;
                SetEnables(true, false);
            }
            else
            {
                extComboBoxBindFiles.Text = "No Bindings loaded";
                SetEnables(false, false);
            }

            extComboBoxBindFiles.SelectedIndexChanged += ExtComboBoxBindFiles_SelectedIndexChanged;

            Display();

            updatecheck.Tick += Updatecheck_Tick;
            updatecheck.Start();
        }

        public void Display()
        { 
            dataGridView.Rows.Clear();

            int orderno = 0;

            foreach (var entry in bf.Entries)
            {
                var row = dataGridView.RowTemplate.Clone() as DataGridViewRow;

                if ( entry.HasAnyKeys )
                {
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 0 group
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 1 ui
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 2 name
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 3 values

                    row.Cells.Add(new DataGridViewComboBoxCell());      // 4 primary device
                    row.Cells.Add(new DataGridViewComboBoxCell());      // 5 primary key

                    if (entry.Binding)
                    {
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells.Add(new DataGridViewTextBoxCell());
                    }
                    else
                    {
                        row.Cells.Add(new DataGridViewComboBoxCell());
                        row.Cells.Add(new DataGridViewComboBoxCell());
                        row.Cells.Add(new DataGridViewComboBoxCell());
                        row.Cells.Add(new DataGridViewComboBoxCell());
                        row.Cells.Add(new DataGridViewComboBoxCell());
                        row.Cells.Add(new DataGridViewComboBoxCell());
                    }

                    row.Tag = entry;
                    row.Cells[0].Value = entry.ClassMode.Item1.ToString().SplitCapsWordFull();
                    row.Cells[0].Tag = (int)entry.ClassMode.Item1;     // tag 0 gets class, first sort, tag 1 gets order this is in the file
                    row.Cells[1].Value = entry.ClassMode.Item2.ToString().SplitCapsWordFull();
                    row.Cells[2].Value = BetterName(entry.Name) + (FrontierBindingClassification.HoldButton(entry.Name) ? " (Hold)" : "");
                    row.Cells[2].Tag = orderno;

                    if (!showFrontierNamesToolStripMenuItem.Checked)
                        row.Cells[2].ToolTipText = "Assignment " + entry.Name;

                    row.Cells[3].Value = BindingEntry.ValuesAsList(entry.Name, entry.Values, true);

                    SetUpCells(row, entry);

                    dataGridView.Rows.Add(row);

                    if (entry.Binding)
                    {
                        for (int i = ColPrimaryModDevice.Index; i <= ColSecondaryModKey.Index; i++)
                            row.Cells[i].ReadOnly = true;
                    }
                }
                else
                {
                    for (int i = 0; i <= ColSecondaryModKey.Index; i++)
                        row.Cells.Add(new DataGridViewTextBoxCell());       // group
                    row.Tag = entry;
                    row.Cells[0].Value = entry.ClassMode.Item1.ToString().SplitCapsWordFull();
                    row.Cells[0].Tag = (int)entry.ClassMode.Item1;     // tag 0 gets class, first sort, tag 1 gets order this is in the file
                    row.Cells[1].Value = entry.ClassMode.Item2.ToString().SplitCapsWordFull();
                    row.Cells[2].Value = BetterName(entry.Name);
                    if (!showFrontierNamesToolStripMenuItem.Checked)
                        row.Cells[2].ToolTipText = "Value entry " + entry.Name;
                    row.Cells[3].Value = BindingEntry.ValuesAsList(entry.Name, entry.Attributes, false);
                    // row.Cells[4].Value = entry.Attributes["Value"]; //debug
                    row.Cells[2].Tag = orderno;
                    dataGridView.Rows.Add(row);
                    for (int i = 4; i <= ColSecondaryModKey.Index; i++)
                        row.Cells[i].ReadOnly = true;
                }

                orderno++;
            }


            dataGridView.Sort(ColGroup, ListSortDirection.Ascending );

            // seems you have to apply after add
            ColPrimaryDevice.DisplayStyleForCurrentCellOnly = ColPrimaryKey.DisplayStyleForCurrentCellOnly =
            ColPrimaryModDevice.DisplayStyleForCurrentCellOnly = ColPrimaryModKey.DisplayStyleForCurrentCellOnly = true;
            ColSecondaryDevice.DisplayStyleForCurrentCellOnly = ColSecondaryKey.DisplayStyleForCurrentCellOnly =
            ColSecondaryModDevice.DisplayStyleForCurrentCellOnly = ColSecondaryModKey.DisplayStyleForCurrentCellOnly = true;

            IndicateErrors();
        }

        #region UI Cell
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != ColValues.Index)
                return;

            var row = dataGridView.Rows[e.RowIndex];
            BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

            bool valuemode = entry.Values.Count > 0;
            Dictionary<string, string> dict = valuemode ? entry.Values : entry.Attributes;

            if (dict.Count > 0)
            {
                row.Selected = true;

                Dictionary<string, string[]> fixedoptions = new Dictionary<string, string[]>();
                Variables vars = new Variables();
                foreach (var x in dict)
                {
                    vars[x.Key] = x.Value;
                    // see if we have a combobox option for this pair
                    var options = FrontierBindingClassification.GetValueOptions(valuemode? x.Key : entry.Name, x.Value);
                    if (options != null)
                        fixedoptions[x.Key] = options;
                }

                VariablesForm vf = new VariablesForm();
                vf.Height = 50;
                vf.DisableOuterBoxBorder = vf.DisableVariableBoxBorder = true;
                vf.AllowAddingMoreEntries = false;
                vf.DisableDeletion = true;
                vf.ComboBoxVariables = fixedoptions;
                vf.Init(vars, $"Values for {BetterName(entry.Name)}", this.FindForm().Icon);
                if (vf.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    foreach (var x in vf.Result.NameEnumuerable)
                    {
                        if (dict[x] != vf.Result[x])
                        {
                            dict[x] = vf.Result[x];
                            System.Diagnostics.Debug.WriteLine($"Set value {dict[x]} = {vf.Result[x]}");
                            SetDirty();
                        }
                    }
                    row.Cells[ColValues.Index].Value = BindingEntry.ValuesAsList(entry.Name, dict, valuemode);
                }
                row.Selected = false;
            }
        }


        #endregion

        #region Editing comboboxes

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Cell Begin Edit {e.RowIndex} {e.ColumnIndex}");
            editingcell = e;
        }

        private void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"EDC Showing {editingcell.RowIndex} {editingcell.ColumnIndex}");
            if (e.Control is DataGridViewComboBoxEditingControl c)
            {
                edc = c;
                edc.SelectedIndexChanged += Edc_SelectedIndexChanged;
                initialcellvalue = edc.Text;
            }
        }

        private void Edc_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newcellvalue = edc.Text;

            if (newcellvalue != initialcellvalue)       // no action if we don't change stuff
            {
                initialcellvalue = newcellvalue;        // update so if changed again it knows

                var row = dataGridView.Rows[editingcell.RowIndex];

                bool nodevice = newcellvalue == bf.NoDeviceName;

                System.Diagnostics.Debug.WriteLine($"Selected index changed {editingcell.RowIndex} {editingcell.ColumnIndex} = {newcellvalue}");

                int ci = editingcell.ColumnIndex;

                BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

                if (ci == ColPrimaryDevice.Index)
                {
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    row.Cells[ci + 1].ReadOnly = nodevice;

                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);

                    if (nodevice)                  // no device clears everything to the right for primary and sets it read only
                    {
                        for (int i = 2; i < 8; i++)
                        {
                            row.Cells[ci + i].ReadOnly = true;
                            row.Cells[ci + i].Value = "";
                            row.Cells[ci + i].ErrorText = "";
                        }

                        entry.ClearAll();
                    }
                    else
                    {
                        // entry in BF is left alone, if you quit without setting the key it comes back
                     
                        if (entry.Binding == false)
                        {
                            row.Cells[ci + 2].ReadOnly = row.Cells[ci + 3].ReadOnly = false;    // else enable the mod and key. Device will be already filled
                            row.Cells[ci + 4].ReadOnly = false;    // else enable the secondary device. Device will be already filled
                        }
                    }

                    SetDirty();
                }
                else if (ci == ColPrimaryKey.Index)
                {
                    entry.SetPrimary(row.Cells[ci - 1].Value.ToString(), newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColPrimaryModDevice.Index)
                {
                    entry.ClearPrimaryMod();            // entry itself gets cleared of value
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ReadOnly = nodevice;
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColPrimaryModKey.Index)
                {
                    entry.SetPrimaryMod(row.Cells[ci - 1].Value.ToString(), newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColSecondaryDevice.Index)
                {
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    row.Cells[ci + 1].ReadOnly = nodevice;

                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);

                    if (nodevice)                  // no device clears everything to the right for primary
                    {
                        for (int i = 2; i < 4; i++)
                        {
                            row.Cells[ci + i].ReadOnly = true;
                            row.Cells[ci + i].Value = "";
                            row.Cells[ci + i].ErrorText = "";
                        }

                        entry.ClearSecondary();
                    }
                    else
                    {
                        row.Cells[ci + 2].ReadOnly = row.Cells[ci + 3].ReadOnly = false;    // else enable the mod and key. Device will be already filled
                    }

                    SetDirty();
                }
                else if (ci == ColSecondaryKey.Index)
                {
                    entry.SetSecondary(row.Cells[ci - 1].Value.ToString(), newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColSecondaryModDevice.Index)
                {
                    entry.ClearSecondaryMod();
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ReadOnly = nodevice;
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColSecondaryModKey.Index)
                {
                    entry.SetSecondaryMod(row.Cells[ci - 1].Value.ToString(), newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }

                System.Diagnostics.Debug.WriteLine($"Entry for {entry.Name} is now Primary ({entry.PrimaryFrontierKeyList()}) Secondary ({entry.SecondaryFrontierKeyList()})");

                IndicateErrors();
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if ( edc != null)
                edc.SelectedIndexChanged -= Edc_SelectedIndexChanged;

            if (e.ColumnIndex == ColPrimaryModDevice.Index || e.ColumnIndex == ColSecondaryDevice.Index || e.ColumnIndex == ColSecondaryModDevice.Index)
            {
                DataGridViewComboBoxCell c = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;
                string selected = c.Value as string;
                bool nodevice = selected == bf.NoDeviceName;
                if ( nodevice )
                    c.Value = "";       // this clears the cell here, can't do it in EDC selected index
            }
        }

        #endregion

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

                bf = new BindingsFile();

                foreach (var kvp in ConvertDeviceNameList)
                    bf.ConvertDeviceNameList[kvp.Key] = kvp.Value;

                bf.Read(fullpathnewfile);

                extComboBoxBindFiles.SelectedIndex = list.IndexOf(fullpathnewfile);
                SetEnables(true, false);
                Display();

                updatecheck.Start();        // start the clock in case it was stopped
            }
            else
            {
                extComboBoxBindFiles.SelectedIndex = list.IndexOf(bf.FileName);
            }

            extComboBoxBindFiles.SelectedIndexChanged += ExtComboBoxBindFiles_SelectedIndexChanged;
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
            if (CheckAskDirty())
            {
                string newname = ExtendedControls.PromptSingleLine.ShowDialog(this, "Name:", "", "Enter new binds name", this.FindForm().Icon);
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
            ExtendedControls.CheckedIconNewListBoxForm displayfilter = new CheckedIconNewListBoxForm();
            foreach (var x in KnownDevices.EmptyIfNull())
            {
                string tag = x;
                int bracket = x.IndexOf("(");               // if list is id (explanation) then the tag is id, the 
                if (bracket >= 0)
                    tag = x.Substring(0, bracket).Trim();

                if (!bf.DeviceList.Contains(tag))
                    displayfilter.UC.AddButton(tag, x);       // tag is same as name
            }


            if (displayfilter.UC.Count>0)
            {
                displayfilter.UC.AddButton("new-dev", "Add user named device");
                displayfilter.CloseBoundaryRegion = new Size(32, extButtonDeviceNew.Height);
                displayfilter.UC.ImageSize = new Size(24, 24);
                displayfilter.UC.ScreenMargin = new Size(0, 0);
                displayfilter.PositionBelow(extButtonDeviceNew);
                displayfilter.UC.ButtonPressed += (i, s1, s2, o, m) =>      // called on click of button
                {
                    if (s1 == "new-dev")
                    {
                        string s = ExtendedControls.PromptSingleLine.ShowDialog(this, "Device:", "", "Enter new device name", this.FindForm().Icon);
                        if (s != null)
                            bf.AddDevice(s);
                    }
                    else
                        bf.AddDevice(s1);

                    Display();
                    displayfilter.Close();
                };

                displayfilter.Show(this);
            }
            else
            {
                string s = ExtendedControls.PromptSingleLine.ShowDialog(this, "Device:", "", "Enter new device name", this.FindForm().Icon);
                if (s != null)
                {
                    bf.AddDevice(s);
                    Display();
                }
            }
        }

        private void buttonRemoveDevice_Click(object sender, EventArgs e)
        {
            RemoveRenameDevice(extButtonDeviceRemove);
        }

        private void buttonDeviceRename_Click(object sender, EventArgs e)
        {
            RemoveRenameDevice(extButtonDeviceRemove, true);
        }

        private void RemoveRenameDevice(Control c, bool rename = false)
        {
            ExtendedControls.CheckedIconNewListBoxForm displayfilter = new CheckedIconNewListBoxForm();
            var items = bf.DeviceListNoKeyboardMouseDevice;

            if (items.Count > 0)
            {
                foreach (var x in items)
                    displayfilter.UC.AddButton(x, x);       // tag is same as name

                displayfilter.CloseBoundaryRegion = new Size(32, c.Height);
                displayfilter.UC.ImageSize = new Size(24, 24);
                displayfilter.UC.ScreenMargin = new Size(0, 0);
                displayfilter.PositionBelow(c);
                displayfilter.UC.ButtonPressed += (i, s1, s2, o, m) =>      // called on click of button
                {
                    if (rename)
                    {
                        string newname = ExtendedControls.PromptSingleLine.ShowDialog(this, "Device:", "", $"Enter new device name for {s1}", this.FindForm().Icon);
                        if ( newname != null)
                            bf.RenameDevice(s1, newname);
                    }
                    else
                    {
                        bf.RemoveDevice(s1);
                    }

                    SetDirty();
                    Display();
                    displayfilter.Close();
                };
                displayfilter.Show(this);
            }

        }
        private void extButtonEmpty_Click(object sender, EventArgs e)
        {
            if (ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} clear all entries, please confirm", "Remove all bindings", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                bf.Clear();
                SetDirty();
                Display();
            }
        }

        #endregion

        #region CMS
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            bool doeshaveanykeys = false, doeshavesecondary = false;
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                doeshaveanykeys |= entry.HasAnyKeys;
                doeshavesecondary |= entry.HasPrimaryAndSecondary;
            }

            clearPrimaryToolStripMenuItem.Enabled = 
            clearAllToolStripMenuItem.Enabled =  doeshaveanykeys;

            moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Enabled =
            clearSecondaryToolStripMenuItem.Enabled =
            swapPrimaryAndSecondaryToolStripMenuItem.Enabled = doeshavesecondary;
        }

        private void swapPrimaryAndSecondaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.SwapPrimarySecondary())
                {
                    SetUpCells(row, entry);
                    SetDirty();
                }
            }
        }

        private void clearPrimaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.HasAnyKeys)
                {
                    if (entry.HasPrimaryAndSecondary)
                        entry.SwapPrimarySecondary();
                    entry.ClearSecondary();
                    SetUpCells(row, entry);
                    SetDirty();
                }
            }
        }

        private void clearSecondaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.HasPrimaryAndSecondary)
                {
                    entry.ClearSecondary();
                    SetUpCells(row, entry);
                    SetDirty();
                }
            }
        }

        private void moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                if ( entry.HasPrimaryAndSecondary &&  bf.IsJoystick(entry.PrimaryKeys[0].Device))
                {
                    entry.SwapPrimarySecondary();
                    SetUpCells(row, entry);
                    SetDirty();
                }
            }
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.HasAnyKeys)
                {
                    entry.ClearAll();
                    SetUpCells(row, entry);
                    SetDirty();
                }
            }
        }
        private void showFrontierNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Display();
        }

        #endregion

        #region Helpers

        void SetUpCells(DataGridViewRow row, BindingEntry entry)
        {
            if (entry.Binding)
            {
                SetUpCells(row, true, false, bf.DeviceListNoKeyboardMouse, 4, entry.PrimaryKeys[0]);
            }
            else
            {
                bool primaryisnodevice = bf.IsNoDevice(entry.PrimaryKeys[0].Device);

                SetUpCells(row, false, false, bf.DeviceList, 4, entry.PrimaryKeys[0]);
                SetUpCells(row, false, primaryisnodevice, bf.DeviceList, 6, entry.PrimaryKeys.Count > 1 ? entry.PrimaryKeys[1] : null);
                SetUpCells(row, false, bf.IsNoDevice(entry.SecondaryKeys[0].Device) || primaryisnodevice, bf.DeviceList, 8, entry.SecondaryKeys.Count > 0 ? entry.SecondaryKeys[0] : null);
                SetUpCells(row, false, bf.IsNoDevice(entry.SecondaryKeys[0].Device) || primaryisnodevice, bf.DeviceList, 10, entry.SecondaryKeys.Count > 1 ? entry.SecondaryKeys[1] : null);
            }
        }

        void SetUpCells(DataGridViewRow row, bool binding, bool disableit, List<string> devices, int index, BindingsFile.DeviceKeyPair dkp)
        {
            var dc = row.Cells[index] as DataGridViewComboBoxCell;
            var dk = row.Cells[index + 1] as DataGridViewComboBoxCell;

            dc.Value = null;
            dk.Value = null;

            dc.Items.Clear();
            foreach (var device in devices) 
                dc.Items.Add(device);           // always add the device list in

            if (disableit)
            {
                row.Cells[index].ReadOnly = row.Cells[index + 1].ReadOnly = true;
            }
            else if (dkp != null)
            {
                row.Cells[index].Tag = dkp;
                SetValue(row.Cells[index], dkp.Device);     // set up device value

                if (bf.IsNoDevice(dkp.Device))              // if no device, cell is clear
                {
                    row.Cells[index + 1].Value = "";
                    row.Cells[index + 1].ReadOnly = true;
                }
                else
                {
                    AddKeyOptions(binding, dkp.Device, dk);
                    SetValue(row.Cells[index + 1], dkp.FrontierKeyName);
                }
            }
            else
            {
                row.Cells[index + 1].ReadOnly = true;
            }
        }
        private void SetValue(DataGridViewCell cell, string value)
        {
            var c = cell as DataGridViewComboBoxCell;
            if (!c.Items.Contains(value))
                c.Items.Add(value);
            c.Value = value;
        }

        private void AddKeyOptions(bool binding, string devicename, DataGridViewComboBoxCell c)
        {
            c.Items.Clear();
            if (binding)
            {
                c.Items.Add($"Joy_XAxis");          // joy axis
                c.Items.Add($"Joy_YAxis");
                c.Items.Add($"Joy_ZAxis");
                c.Items.Add($"Joy_RXAxis");
                c.Items.Add($"Joy_RYAxis");
                c.Items.Add($"Joy_RZAxis");
                c.Items.Add($"Joy_UAxis");
                c.Items.Add($"Joy_VAxis");
            }
            else if (devicename == "Keyboard")
            {
                foreach (var x in FrontierKeyConversion.FrontierKeyNames())
                    c.Items.Add(x);
            }
            else if (devicename == "Mouse")
            {
                c.Items.Add($"Mouse_1");
                c.Items.Add($"Mouse_2");
                c.Items.Add($"Mouse_3");
                c.Items.Add($"Mouse_4");
                c.Items.Add($"Mouse_5");
                c.Items.Add($"Mouse_6");
                c.Items.Add($"Mouse_7");
                c.Items.Add($"Mouse_8");
                c.Items.Add($"Neg_Mouse_ZAxis");
                c.Items.Add($"Pos_Mouse_ZAxis");
            }
            else if (!bf.IsNoDevice(devicename))
            {
                for (int i = 1; i < 32; i++)
                    c.Items.Add($"Joy_{i}");
                c.Items.Add($"Joy_POV1Left");
                c.Items.Add($"Joy_POV1Right");
                c.Items.Add($"Joy_POV1Left");
                c.Items.Add($"Joy_POV1Up");
                c.Items.Add($"Joy_POV1Down");
                c.Items.Add($"Joy_POV2Right");
                c.Items.Add($"Joy_POV2Left");
                c.Items.Add($"Joy_POV2Up");
                c.Items.Add($"Joy_POV2Down");

            }
        }

        private bool CheckAskDirty()
        {
            bool ok = !IsDirty || ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} has been modified, abandon changes?", "Changed Binding", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            return ok;
        }

        private void SetDirty()
        {
            extButtonSave.Enabled = true;
        }
        private void ClearDirty()
        {
            extButtonSave.Enabled = false;
        }

        private void SetEnables(bool dupit, bool saveit)
        {
            extButtonDuplicate.Enabled = extButtonSetDefault.Enabled = dupit;
            extButtonDeviceNew.Enabled = extButtonDeviceRemove.Enabled = extButtonDeviceRename.Enabled = dupit;
            extButtonSave.Enabled = saveit;
        }


        private void IndicateErrors()
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                row.Cells[ColPrimaryKey.Index].Style.BackColor = Color.Empty;       // reset
                row.Cells[ColSecondaryKey.Index].Style.BackColor = Color.Empty;
                row.Cells[ColPrimaryKey.Index].ToolTipText = null;
                row.Cells[ColSecondaryKey.Index].ToolTipText = null;
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                BindingsFile.BindingEntry entry1 = row.Tag as BindingsFile.BindingEntry;

                if (entry1.HasAnyKeys)
                {

                    for (int comparerowno = row.Index + 1; comparerowno < dataGridView.Rows.Count; comparerowno++)
                    {
                        DataGridViewRow rowcompare = dataGridView.Rows[comparerowno];
                        BindingsFile.BindingEntry entry2 = rowcompare.Tag as BindingsFile.BindingEntry;

                        if (entry1.ClassMode != null && entry2.ClassMode != null && entry1.ClassMode.Item2 == entry2.ClassMode.Item2)    // if same binding set
                        {
                            var same = entry1.HasAnyKeysInCommon(entry2);

                            if (same != null)
                            {
                                if ( !FrontierBindingClassification.HoldButton(entry1.Name) && !FrontierBindingClassification.HoldButton(entry2.Name))
                                { 
                                    System.Diagnostics.Debug.WriteLine($"Checked {entry1.Name} vs {entry2.Name} Clash {same} : `{entry1.PrimaryFrontierKeyList()}` `{entry1.SecondaryFrontierKeyList()}` vs `{entry2.PrimaryFrontierKeyList()}` `{entry2.SecondaryFrontierKeyList()}`");
                                    var cell1 = row.Cells[same.Item1 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell1.Style.BackColor = Color.DarkRed;
                                    cell1.ToolTipText = $"Keys clash with {BetterName(entry2.Name)} {(same.Item2 == 1 ? "Primary" : "Secondary")}";
                                    var cell2 = rowcompare.Cells[same.Item2 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell2.Style.BackColor = Color.DarkRed;
                                    cell2.ToolTipText = $"Keys clash with {BetterName(entry1.Name)} {(same.Item1 == 1 ? "Primary" : "Secondary")}";
                                }

                            }
                        }
                    }
                }
            }
        }

        private void dataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column == ColGroup)
            {
                int left = (int)dataGridView.Rows[e.RowIndex1].Cells[ColGroup.Index].Tag;
                int right = (int)dataGridView.Rows[e.RowIndex2].Cells[ColGroup.Index].Tag;
                if (left == right)
                {
                    left = (int)dataGridView.Rows[e.RowIndex1].Cells[ColName.Index].Tag;
                    right = (int)dataGridView.Rows[e.RowIndex2].Cells[ColName.Index].Tag;
                    //string itemleft = (string)dataGridView.Rows[e.RowIndex1].Cells[ColName.Index].Value;
                    //string itemright = (string)dataGridView.Rows[e.RowIndex2].Cells[ColName.Index].Value;
                    //e.SortResult =itemleft.CompareTo(itemright);
                }

                e.SortResult = left.CompareTo(right);

                e.Handled = true;
            }
        }

        public string BetterName(string name)
        {
            return showFrontierNamesToolStripMenuItem.Checked ? name : name.SplitCapsWordFull().Replace("Buggy", "SRV").Replace("Turret", "SRV Turret").Replace("Humanoid", "On Foot");
        }

        private void Updatecheck_Tick(object sender, EventArgs e)
        {
            if (bf.IsOutOfDate() && !dataGridView.IsCurrentCellInEditMode)
            {
                updatecheck.Stop();
                if (ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} has been changed externally, do you wish to update?", "Warning - Binding File changed", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    bf.Read(bf.FileName);
                    Display();
                    
                    updatecheck.Start();        // start the clock in case it was stopped
                }
                else
                {
                    SetDirty();                 // we set it dirty since its out of sync with the file now and leave the update check now off
                }
            }
        }

        #endregion


        private BindingsFile bf;
        private Timer updatecheck = new Timer() { Interval = 1000 };

        private DataGridViewCellCancelEventArgs editingcell;
        private DataGridViewComboBoxEditingControl edc;
        string initialcellvalue;

    }
}
