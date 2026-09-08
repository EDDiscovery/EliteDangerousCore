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

using QuickJSON;
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
        public List<string> DevicesNamesConverted => bf.DeviceList.Select(x=>BetterDeviceName(x)).ToList();

        public BindingsEditor()
        {
            InitializeComponent();
            ColValues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridView.MakeDoubleBuffered();

            showFrontierNamesToolStripMenuItem.Checked = false;
            showFrontierNamesToolStripMenuItem.Click += new System.EventHandler(this.showFrontierNamesToolStripMenuItem_Click);
        }

       
        // folder and preferredbindfile (can be null)
        public void Init(string folder, string preferredbindfile, List<string> otherdevicesknown, string jsondevicekeynames, string defaultdevicekeynames = null)
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

            // load stored key renames

            if ( jsondevicekeynames!=null)
                keyrenames.Set(jsondevicekeynames);

            if ( defaultdevicekeynames!=null)           // if we have a default list, see if it needs to populate into standard list
            {
                DeviceKeyNames defrenames = new DeviceKeyNames();
                defrenames.Set(defaultdevicekeynames);
                foreach(DeviceKeyNames.DeviceEntry key in defrenames)
                {
                    if ( keyrenames.Get(key.DeviceList) == null)
                    {
                        keyrenames.Add(key);
                    }
                }
            }

            ComboBoxFilterFill();

            System.Diagnostics.Debug.WriteLine($"Key list {keyrenames.Get()}");
            Display();

            updatecheck.Tick += Updatecheck_Tick;
            updatecheck.Start();
        }

        public string KeyNames()
        {
            return keyrenames.Get();
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
                dc.Items.Add(BetterDeviceName(device));           // always add the device list in

            if (dkp != null)
            {
                dc.Tag = dkp;
                SetDevice(dc, dkp.Device);

                if (!dkp.IsDevice)              // if no device, cell is clear
                {
                    dk.Value = null;
                    dk.ReadOnly = true;
                }
                else
                {
                    SetKey(dk, binding, dkp.Device, dkp.FrontierKeyName);
                }

            }
            else
            {
                dk.ReadOnly = true;
            }

            SetHint(row, index, dkp?.Device, dkp?.FrontierKeyName);

            if (!editable)
            {
                dc.ReadOnly = dk.ReadOnly = true;
            }
        }

        // set up a device cell, with a better name than frontierdevice
        private void SetDevice(DataGridViewCell dc, string frontierdevicename)
        {
            string bdn = BetterDeviceName(frontierdevicename);
            SetValue(dc, bdn);     // set up device valu
            //System.Diagnostics.Debug.WriteLine($"Changed Device {frontierdevicename}");
        }

        // set up a key cell, given the binding and frontier device name. 
        private void SetKey(DataGridViewComboBoxCell dk, bool binding, string frontierdevicename, string frontierkeyname)
        {
            dk.Value = null;        // need to clear before clearing items
            AddKeyOptions(binding, frontierdevicename, dk);
            string bk = keyrenames.GetRename(frontierdevicename, frontierkeyname);
            SetValue(dk, bk);
            dk.ErrorText = null;
            //System.Diagnostics.Debug.WriteLine($"Changed Key {binding} {frontierdevicename} {frontierkeyname}");
        }

        // set hints on device/key pair.  You may call with both nulls, or just device, or both
        private void SetHint(DataGridViewRow row, int deviceindex, string frontierdevicename = null, string frontierkeyname = null)
        {
            row.Cells[deviceindex].ToolTipText = row.Cells[deviceindex + 1].ToolTipText = null;

            if (frontierdevicename != null)
            {
                string nl = Environment.NewLine;    // for debugging change to "|"
                string bdn = BetterDeviceName(frontierdevicename);
                row.Cells[deviceindex].ToolTipText = DeviceKeyPair.IsJoystickDevice(frontierdevicename) && bdn != frontierdevicename ? (bdn + nl + "Frontier Name " + frontierdevicename) : null;

                if (frontierkeyname != null)
                {
                    string bk = keyrenames.GetRename(frontierdevicename, frontierkeyname);
                    string hint = keyrenames.GetHint(frontierdevicename, frontierkeyname);
                    row.Cells[deviceindex + 1].ToolTipText = bk != frontierkeyname ? (hint + nl + frontierkeyname) : null;
                }
            }
            //System.Diagnostics.Debug.WriteLine($"Changed Hint {row.Index} {deviceindex} : `{row.Cells[deviceindex].ToolTipText}` `{row.Cells[deviceindex + 1].ToolTipText}`");
        }

        private void SetValue(DataGridViewCell cell, string value)
        {
            var c = cell as DataGridViewComboBoxCell;
            if (!c.Items.Contains(value))
                c.Items.Add(value);
            c.Value = value;
        }

        // use renamed device in here
        private void SetPair(DataGridViewRow row, bool binding, int index, string frontierdevicename, string frontierkeyname)
        {
            SetDevice(row.Cells[index], frontierdevicename);
            SetKey(row.Cells[index + 1] as DataGridViewComboBoxCell, binding, frontierdevicename, frontierkeyname);
            SetHint(row,index,frontierdevicename,frontierkeyname);
        }

        // add options to key, based on binding, the bindings file external device name
        private void AddKeyOptions(bool hasaxis, string frontierdevicename, DataGridViewComboBoxCell c)
        {
            // so we can use the device naming system to find 
            bool isjoystick = DeviceKeyPair.IsJoystickDevice(frontierdevicename);
            var devkname = keyrenames.Get(frontierdevicename);
            int joystickbuttons = isjoystick ? devkname?.Buttons ?? 128 : 0;
            int joystickpov = isjoystick ? devkname?.POV ?? 2 : 0;
            string[] joystickaxis = isjoystick ? devkname?.Axis ?? DeviceKeyNames.DeviceEntry.DefaultAxis : null;
            var ret = FrontierKeyConversion.FrontierKeyNames(bf.KeyboardLayout, joystickaxis , joystickbuttons, joystickpov, DeviceKeyPair.IsMouseDevice(frontierdevicename), DeviceKeyPair.IsKeyboardDevice(frontierdevicename));
            c.Items.Clear();
            foreach (var x in ret)
                c.Items.Add( keyrenames.GetRename(frontierdevicename,x));
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
            string clash = "Keys clash with";

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                row.Cells[ColPrimaryKey.Index].Style.BackColor = Color.Empty;       // reset
                row.Cells[ColSecondaryKey.Index].Style.BackColor = Color.Empty;
                if (row.Cells[ColPrimaryKey.Index].ToolTipText?.Contains(clash) == true)        // clear any clash tooltip
                    row.Cells[ColPrimaryKey.Index].ToolTipText = null;
                if (row.Cells[ColSecondaryKey.Index].ToolTipText?.Contains(clash) == true)
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
                                   // System.Diagnostics.Debug.WriteLine($"Checked Clash: `{entry1.ToString()}` vs `{entry2.ToString()}`");
                                    var cell1 = row.Cells[same.Item1 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell1.Style.BackColor = Color.DarkRed;
                                    cell1.ToolTipText = clash + $" {BetterName(entry2.Name)} {(same.Item2 == 1 ? "Primary" : "Secondary")}";
                                    var cell2 = rowcompare.Cells[same.Item2 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell2.Style.BackColor = Color.DarkRed;
                                    cell2.ToolTipText = clash + $" {BetterName(entry1.Name)} {(same.Item1 == 1 ? "Primary" : "Secondary")}";
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

        public string BetterName(string name)
        {
            return showFrontierNamesToolStripMenuItem.Checked ? name : name.SplitCapsWordFull().Replace("Buggy", "SRV").Replace("Turret", "SRV Turret").Replace("Humanoid", "On Foot").ReplaceIfStartsWith("Cam ", "Galaxy Map ");
        }
        public string BetterDeviceName(string name)
        {
            return ConvertDeviceNameList.TryGetValue(name, out var bettername) ? bettername : name;
        }
        public string OriginalDeviceName(string name)
        {
            return ConvertDeviceNameList.Where(kvp => kvp.Value == name).Select(x => x.Key).FirstOrDefault() ?? name;
        }

        #endregion


        private BindingsFile bf;
        private Timer updatecheck = new Timer() { Interval = 1000 };

        private DataGridViewCellCancelEventArgs editingcell;
        private DataGridViewComboBoxEditingControl edc;
        string initialcellvalue;
        private List<string> otherdevicesknown;
        private int filtercomboboxuistart = 0;
        private int filtercomboboxmodestart = 0;

        private DeviceKeyNames keyrenames = new DeviceKeyNames();

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
