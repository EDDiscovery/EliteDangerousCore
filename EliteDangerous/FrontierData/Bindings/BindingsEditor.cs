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

using ExtendedControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static EliteDangerousCore.Bindings.BindingsFile;

namespace EliteDangerousCore.Bindings
{
    public partial class BindingsEditor : UserControl
    {
        public bool IsDirty => extButtonSave.Enabled;
        public Action<string> ChangedBindings { get; set; }           // saved this load
        public Action<string> ChangedDefault { get; set; }            // changed the start preset file
        public Action ResetKeyNames { get; set; }                     // request keyname reset

        // called to pop up a way of the user pressing key/joystick.
        // tuple returned is frontier device, frontier key name, joystick direction positive
        public Func<BindingsFile, BindingEntry, Tuple<string, string, bool>> DeviceInput { get; set; }

        public BindingsEditor()
        {
            InitializeComponent();
           // ColValues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridView.MakeDoubleBuffered();

            showFrontierNamesToolStripMenuItem.Checked = false;
            showFrontierNamesToolStripMenuItem.Click += new System.EventHandler(this.showFrontierNamesToolStripMenuItem_Click);
        }


        // folder and preferredbindfile (can be null)
        // give known devices and properties
        // give known device name mappings
        public void Init(string folder, string preferredbindfile, List<Device> deviceparas, DeviceKeyNames keynames)
        {
            this.deviceparas = deviceparas;
            this.devicekeynames = keynames;
            this.bindingfolder = folder;

            List<FileInfo> bindfiles = Directory.EnumerateFiles(folder, "*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToList();

            foreach (var x in bindfiles)
                extComboBoxBindFiles.Items.Add(x.Name);
            extComboBoxBindFiles.Tag = bindfiles.Select(x => x.FullName).ToList();

            bf = new BindingsFile(deviceparas);

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

            ComboBoxFilterFill();

            //System.Diagnostics.Debug.WriteLine($"Key list {keynames.Get()}");

            dataGridView.SetWordWrap(true);
            dataGridView.RowTemplate.MinimumHeight = Font.ScalePixels(40);

            Display();

            updatecheck.Tick += Updatecheck_Tick;
            updatecheck.Start();
        }

        public string KeyNames()
        {
            return devicekeynames.Get();
        }

        public void Display()
        {
            dataGridView.Rows.Clear();
            dataViewScrollerPanel.Suspend();

            var sortstate = dataGridView.GetSort(ColGroup.Index);        // default sort of this ascending

            int orderno = 0;

            foreach (var entry in bf.Entries)
            {
                //if (entry.Name != "CamPitchUp")   continue;
                //  if (!entry.Name.StartsWith("UI"))   continue;

                var row = dataGridView.RowTemplate.Clone() as DataGridViewRow;

                if (entry.IsKeyOrBinding)
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
                    row.Cells[2].Value = BetterBindingName(entry.Name) + (FrontierBindingClassification.HoldButton(entry.Name) ? " (Hold)" : "");
                    row.Cells[2].Tag = orderno;

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
                    row.Cells[2].Value = BetterBindingName(entry.Name);
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

                row.Visible = Filter(row);          // filter out this row if required

                orderno++;
            }

            dataGridView.Sort(sortstate);           // restate sort

            // seems you have to apply after add
            ColPrimaryDevice.DisplayStyleForCurrentCellOnly = ColPrimaryKey.DisplayStyleForCurrentCellOnly =
            ColPrimaryModDevice.DisplayStyleForCurrentCellOnly = ColPrimaryModKey.DisplayStyleForCurrentCellOnly = true;
            ColSecondaryDevice.DisplayStyleForCurrentCellOnly = ColSecondaryKey.DisplayStyleForCurrentCellOnly =
            ColSecondaryModDevice.DisplayStyleForCurrentCellOnly = ColSecondaryModKey.DisplayStyleForCurrentCellOnly = true;

            extButtonDeviceNew.Visible = extButtonDeviceRemap.Visible = bf.IsEditable;
            labelWarning.Text = bf.IsEditable ? "" : $"Unknown Keyboard Layout {bf.KeyboardCulture} {InputLanguage.CurrentInputLanguage.LayoutName} {InputLanguage.CurrentInputLanguage.Culture.Name}. File is not editable";

            dataGridView.ContextMenuStrip = bf.IsEditable ? this.contextMenuStrip : null;

            IndicateErrors();

            dataViewScrollerPanel.Resume();
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

        void SetUpCells(DataGridViewRow row, bool editable, bool binding, List<Device> devices, int index, BindingsFile.DeviceKeyPair dkp)
        {
            var dc = row.Cells[index] as DataGridViewComboBoxCell;
            var dk = row.Cells[index + 1] as DataGridViewComboBoxCell;

            dc.Value = null;
            dk.Value = null;

            dc.Items.Clear();
            foreach (var device in devices)
                dc.Items.Add(device.BetterName);           // always add the device list in

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
        private void SetDevice(DataGridViewCell dc, Device device)
        {
            SetValue(dc, device.BetterName);     // set up device valu
            //System.Diagnostics.Debug.WriteLine($"Changed Device {frontierdevicename}");
        }

        // set up a key cell, given the binding and frontier device name. 
        private void SetKey(DataGridViewComboBoxCell dk, bool binding, Device device, string frontierkeyname)
        {
            dk.Value = null;        // need to clear before clearing items
            AddKeyOptions(binding, device, dk);
            string bk = devicekeynames.GetRename(device.FrontierName, frontierkeyname);
            SetValue(dk, bk);
            dk.ErrorText = null;
            //System.Diagnostics.Debug.WriteLine($"Changed Key {binding} {frontierdevicename} {frontierkeyname}");
        }

        // set hints on device/key pair.  You may call with both nulls, or just device, or both
        private void SetHint(DataGridViewRow row, int deviceindex, Device device = null, string frontierkeyname = null)
        {
            BindingEntry entry = row.Tag as BindingEntry;
            row.Cells[2].ToolTipText = !showFrontierNamesToolStripMenuItem.Checked ? (entry.Name + Environment.NewLine) : "";
            row.Cells[2].ToolTipText += entry.IsKeyOrBinding ? (entry.PrimaryKeys.KeyDescription() + (entry.IsKey ? " : " + entry.SecondaryKeys.KeyDescription() : "")) : "";

            row.Cells[deviceindex].ToolTipText = row.Cells[deviceindex + 1].ToolTipText = null;

            if (device != null)
            {
                string nl = Environment.NewLine;    // for debugging change to "|"
                row.Cells[deviceindex].ToolTipText = device.IsJoystick && device.HasBetterName ? (device.BetterName + nl + "Frontier Name " + device.FrontierName) : null;

                if (frontierkeyname != null)
                {
                    string bk = devicekeynames.GetRename(device.FrontierName, frontierkeyname);
                    string hint = devicekeynames.GetHint(device.FrontierName, frontierkeyname);
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
        private void SetPair(DataGridViewRow row, bool binding, int index, Device device, string frontierkeyname)
        {
            SetDevice(row.Cells[index], device);
            SetKey(row.Cells[index + 1] as DataGridViewComboBoxCell, binding, device, frontierkeyname);
            SetHint(row, index, device, frontierkeyname);
        }

        // add options to key, based on binding and the device
        private void AddKeyOptions(bool isaxis, Device device, DataGridViewComboBoxCell c)
        {
            // we are not going to use keynams to set joystick limits, just physical detected devices
            var ret = FrontierKeyConversion.FrontierKeyNames(device, isaxis, !isaxis, bf.KeyboardLayout);

            c.Items.Clear();
            foreach (var x in ret)
                c.Items.Add(devicekeynames.GetRename(device.FrontierName, x));
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
            extButtonDeviceNew.Enabled = extButtonReload.Enabled = extButtonDeviceRemap.Enabled = active;
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
                                if (!FrontierBindingClassification.HoldButton(entry1.Name) && !FrontierBindingClassification.HoldButton(entry2.Name))
                                {
                                    // System.Diagnostics.Debug.WriteLine($"Checked Clash: `{entry1.ToString()}` vs `{entry2.ToString()}`");
                                    var cell1 = row.Cells[same.Item1 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell1.Style.BackColor = Color.DarkRed;
                                    cell1.ToolTipText = clash + $" {BetterBindingName(entry2.Name)} {(same.Item2 == 1 ? "Primary" : "Secondary")}";
                                    var cell2 = rowcompare.Cells[same.Item2 == 1 ? ColPrimaryKey.Index : ColSecondaryKey.Index];
                                    cell2.Style.BackColor = Color.DarkRed;
                                    cell2.ToolTipText = clash + $" {BetterBindingName(entry1.Name)} {(same.Item1 == 1 ? "Primary" : "Secondary")}";
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

        public string BetterBindingName(string name)
        {
            return showFrontierNamesToolStripMenuItem.Checked ? name : name.SplitCapsWordFull().Replace("Buggy", "SRV").Replace("Turret", "SRV Turret").
                            Replace("Humanoid", "On Foot").ReplaceIfStartsWith("Cam ", "Galaxy Map ").Replace("Toggle Button Up Input", "Silent Running");
        }

        // return the frontier name associated with this physical device
        public string GetDeviceName(string physicalname, Guid instanceguid, Guid productguid, int productid, int vendorid)
        {
            return bf.GetDeviceName(physicalname, instanceguid, productguid, productid, vendorid);
        }

        #endregion

        #region Paint 
        private void dataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];
            var entry = row.Tag as BindingEntry;

            //System.Diagnostics.Debug.WriteLine($"Paint Row {row.Index}");

            // if its a key, see if the primary/secondary keys are there, and then call the paint func below

            if (entry?.IsKeyOrBinding == true)
            {
                if (entry.PrimaryKeys?.Assigned == true )
                {
                    PaintKey(entry.PrimaryKeys.Keys[0].Device, entry.PrimaryKeys.Keys[0].FrontierKeyName, row.Index, ColPrimaryKey.Index, e.RowBounds, e.Graphics);
                    if (entry.PrimaryKeys.HasMod)
                        PaintKey(entry.PrimaryKeys.Keys[1].Device, entry.PrimaryKeys.Keys[1].FrontierKeyName, row.Index, ColPrimaryModKey.Index, e.RowBounds, e.Graphics);

                }
                if (entry.SecondaryKeys?.Assigned == true)
                {
                    PaintKey(entry.SecondaryKeys.Keys[0].Device, entry.SecondaryKeys.Keys[0].FrontierKeyName, row.Index, ColSecondaryKey.Index, e.RowBounds, e.Graphics);
                    if (entry.SecondaryKeys.HasMod)
                        PaintKey(entry.SecondaryKeys.Keys[1].Device, entry.SecondaryKeys.Keys[1].FrontierKeyName, row.Index, ColSecondaryModKey.Index, e.RowBounds, e.Graphics);

                }
            }
        }

        private void PaintKey(Device dev, string key, int row, int col, Rectangle rowbounds, Graphics gr)
        {
            var crow = dataGridView.CurrentCell?.RowIndex ?? -1;
            var ccol = dataGridView.CurrentCell?.ColumnIndex ?? -1;

            if (crow != row || ccol != col)
            {
                Image bk = devicekeynames.GetIcon(dev.FrontierName, key, false);
                if (bk != null)
                {
                    var colrect = dataGridView.GetColumnDisplayRectangle(col, false);
                    int pad = 1;
                    var p1 = new Rectangle(colrect.X + colrect.Width - rowbounds.Height - pad, rowbounds.Y + pad, rowbounds.Height - pad * 2, rowbounds.Height - pad * 2);
                    //System.Diagnostics.Debug.WriteLine($"Row has main icon {p1}");
                    gr.DrawImage(bk, p1);

                }
            }
        }

        #endregion

        #region Vars

        private string bindingfolder;
        private BindingsFile bf;
        private Timer updatecheck = new Timer() { Interval = 1000 };

        private DataGridViewCellCancelEventArgs editingcell;
        private DataGridViewComboBoxEditingControl edc;
        string initialcellvalue;

        private int filtercomboboxuistart = 0;
        private int filtercomboboxmodestart = 0;

        private DeviceKeyNames devicekeynames;
        private List<Device> deviceparas;

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // debug - should never happen
        }

        #endregion

    }
}
