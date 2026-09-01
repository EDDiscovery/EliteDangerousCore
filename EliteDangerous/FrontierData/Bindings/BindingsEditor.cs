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

        // called to pop up a way of the user pressing key/joystick. Return DKP with the names as presented in the pop down list, ie. Via ConvertDeviceNameList
        public Func<BindingsFile, BindingEntry, DeviceKeyPair> DeviceInput { get; set; }   

        // allows user to define translations between external frontier device names and ones use in here
        public Dictionary<string, string> ConvertDeviceNameList = new Dictionary<string, string>();

        public List<string> DevicesNamesConverted => bf.DeviceList.Select(x=>BetterDevice(x)).ToList();        
        
        public BindingsEditor()
        {
            InitializeComponent();
            ColValues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridView.MakeDoubleBuffered();

            showFrontierNamesToolStripMenuItem.Checked = false;
            showFrontierNamesToolStripMenuItem.Click += new System.EventHandler(this.showFrontierNamesToolStripMenuItem_Click);

            extComboBoxFilter.Items.Add("All");
            foreach (var x in Enum.GetNames(typeof(FrontierBindingClassification.Classification)))
                extComboBoxFilter.Items.Add(x.ToString().SplitCapsWordFull());
            filtercomboboxmodestart = extComboBoxFilter.Items.Count;
            foreach (var x in Enum.GetNames(typeof(FrontierBindingClassification.Mode)))
                extComboBoxFilter.Items.Add(x.ToString().SplitCapsWordFull());
            extComboBoxFilter.SelectedIndex = 0;
            extComboBoxFilter.SelectedIndexChanged += ExtComboBoxFilter_SelectedIndexChanged;
        }

       
        // folder and preferredbindfile (can be null)
        public void Init(string folder, string preferredbindfile, List<string> otherdevicesknown)
        {
            List<FileInfo> bindfiles = Directory.EnumerateFiles(folder, "*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToList();
            foreach (var x in bindfiles)
                extComboBoxBindFiles.Items.Add(x.Name);
            extComboBoxBindFiles.Tag = bindfiles.Select(x=>x.FullName).ToList();

            this.otherdevicesknown = otherdevicesknown;

            bf = new BindingsFile(otherdevicesknown);

            if (preferredbindfile != null) // if preferred bind file
            {
                bf.Read(preferredbindfile);
                if (bf.IsLoaded)
                {
                    System.Diagnostics.Debug.WriteLine($"Read file {bf.FileName} `{bf.KeyboardCulture}` `{bf.KeyboardLayout}`");
                }
            }

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
                //if (entry.Name != "CamPitchUp")   continue;
              //  if (!entry.Name.StartsWith("UI"))   continue;

                var row = dataGridView.RowTemplate.Clone() as DataGridViewRow;

                if ( entry.IsKeyOrBinding )
                {
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 0 group
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 1 ui
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 2 name
                    row.Cells.Add(new DataGridViewTextBoxCell());       // 3 values

                    row.Cells.Add(new DataGridViewComboBoxCell());      // 4 primary device
                    row.Cells.Add(new DataGridViewComboBoxCell());      // 5 primary key

                    if (entry.IsBinding)
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

                    if (entry.IsBinding)
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
                    if (entry.Attributes.Count == 0)
                        row.Cells[3].Value = entry.Value;
                    else
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

            extButtonDeviceNew.Visible = extButtonDeviceRename.Visible = bf.IsEditable;
            labelWarning.Text = bf.IsEditable ? "" : $"Unknown Keyboard Layout {bf.KeyboardCulture} {InputLanguage.CurrentInputLanguage.LayoutName} {InputLanguage.CurrentInputLanguage.Culture.Name}. File is not editable";

            dataGridView.ContextMenuStrip = bf.IsEditable ? this.contextMenuStrip : null;

            IndicateErrors();

            ApplyFilter();
        }

        //provide access to the find device of bindings file for device mapping purposes
        public string FindDevice(string name, Guid instanceguid, Guid productguid, int productid, int vendorid)
        {
            return bf.FindDevice(name,instanceguid, productguid, productid, vendorid);
        }

        #region Helpers

        void SetUpCells(DataGridViewRow row, BindingEntry entry)
        {
            if (entry.IsBinding)
            {
                SetUpCells(row, bf.IsEditable, true, bf.DeviceListNoKeyboardMouse, 4, entry.PrimaryKeys.Keys[0]);
            }
            else
            {
                bool primaryisdevice = entry.PrimaryKeys.Keys[0].IsDevice;
                bool secondaryisdevice = entry.SecondaryKeys.Keys[0].IsDevice;

                SetUpCells(row, bf.IsEditable, false, bf.DeviceList, 4, entry.PrimaryKeys.Keys[0]);
                SetUpCells(row, bf.IsEditable && primaryisdevice, false, bf.DeviceList, 6, primaryisdevice && entry.PrimaryKeys.Count > 1 ? entry.PrimaryKeys.Keys[1] : null);
                SetUpCells(row, bf.IsEditable, false, bf.DeviceList, 8, primaryisdevice && entry.SecondaryKeys.Count > 0 ? entry.SecondaryKeys.Keys[0] : null);
                SetUpCells(row, bf.IsEditable && secondaryisdevice, false, bf.DeviceList, 10, secondaryisdevice && primaryisdevice && entry.SecondaryKeys.Count > 1 ? entry.SecondaryKeys.Keys[1] : null);
            }
        }

        void SetUpCells(DataGridViewRow row, bool editable, bool binding, List<string> devices, int index, BindingsFile.DeviceKeyPair dkp)
        {
            var dc = row.Cells[index] as DataGridViewComboBoxCell;
            var dk = row.Cells[index + 1] as DataGridViewComboBoxCell;

            dc.Value = null;
            dk.Value = null;

            dc.Items.Clear();
            foreach (var device in devices) 
                dc.Items.Add(BetterDevice(device));           // always add the device list in

            if (dkp != null)
            {
                row.Cells[index].Tag = dkp;
                SetValue(row.Cells[index], BetterDevice(dkp.Device));     // set up device value

                if (!dkp.IsDevice)              // if no device, cell is clear
                {
                    row.Cells[index + 1].Value = null;
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

            if (!editable)
            {
                row.Cells[index].ReadOnly = row.Cells[index + 1].ReadOnly = true;
            }
        }
        private void SetValue(DataGridViewCell cell, string value)
        {
            var c = cell as DataGridViewComboBoxCell;
            if (!c.Items.Contains(value))
                c.Items.Add(value);
            c.Value = value;
        }

        // add options to key, based on binding, the bindings file external device name
        private void AddKeyOptions(bool binding, string extdevicename, DataGridViewComboBoxCell c)
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
            else if (extdevicename == "Keyboard")
            {
                if (bf.IsEditable)
                {
                    foreach (var x in FrontierKeyConversion.FrontierKeyNames(bf.KeyboardLayout))
                        c.Items.Add(x);

                    c.Items.Remove("Key_Escape");       // can't use this so remove from selection box. Keep it in the frontier name system though for safety
                }
            }
            else if (extdevicename == "Mouse")
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
            else if (extdevicename != DeviceKeyPair.NoDeviceName)
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

        private void SetPair(DataGridViewRow row, bool binding, int index, string bindingrenameddevice, string key)
        {
            row.Cells[index].Value = bindingrenameddevice;
            row.Cells[index + 1].Value = null;
            AddKeyOptions(binding, OriginalDeviceName(bindingrenameddevice), (DataGridViewComboBoxCell)row.Cells[index + 1]);
            row.Cells[index + 1].Value = key;
            row.Cells[index + 1].ErrorText = null;
            row.Cells[index + 1].ReadOnly = false;
        }

        private bool CheckAskDirty()
        {
            bool ok = !IsDirty || ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} has been modified, abandon changes?", "Changed Binding", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            return ok;
        }

        private void SetDirty()
        {
            extButtonSave.Enabled = extButtonReload.Enabled = true;
        }
        private void ClearDirty()
        {
            extButtonSave.Enabled = extButtonReload.Enabled = false;
        }

        private void SetEnables(bool active, bool saveit)
        {
            extButtonDuplicate.Enabled = extButtonSetDefault.Enabled = active;
            extButtonDeviceNew.Enabled = extButtonReload.Enabled = extButtonDeviceRename.Enabled = active;
            extButtonReload.Enabled = extButtonSave.Enabled = saveit;
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

                if (entry1.IsKeyOrBinding)
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
                                    System.Diagnostics.Debug.WriteLine($"Checked Clash: `{entry1.ToString()}` vs `{entry2.ToString()}`");
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
                }

                e.SortResult = left.CompareTo(right);

                e.Handled = true;
            }
        }

        public string BetterName(string name)
        {
            return showFrontierNamesToolStripMenuItem.Checked ? name : name.SplitCapsWordFull().Replace("Buggy", "SRV").Replace("Turret", "SRV Turret").Replace("Humanoid", "On Foot").ReplaceIfStartsWith("Cam ", "Galaxy Map ");
        }
        public string BetterDevice(string name)
        {
            return ConvertDeviceNameList.TryGetValue(name, out var bettername) ? bettername : name;
        }
        public string OriginalDeviceName(string name)
        {
            return ConvertDeviceNameList.Where(kvp => kvp.Value == name).Select(x => x.Key).FirstOrDefault() ?? name;
        }

        private void Updatecheck_Tick(object sender, EventArgs e)
        {
            if (bf.IsOutOfDate() && !dataGridView.IsCurrentCellInEditMode)
            {
                updatecheck.Stop();
                if (ExtendedControls.MessageBoxTheme.Show(this, $"{bf.PresetName} has been changed externally, do you wish to update?", "Warning - Binding File changed", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    bf.Read(bf.FileName);       // does not change device list btw
                    ClearDirty();
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
        private List<string> otherdevicesknown;
        private int filtercomboboxmodestart = 0;

    }
}
