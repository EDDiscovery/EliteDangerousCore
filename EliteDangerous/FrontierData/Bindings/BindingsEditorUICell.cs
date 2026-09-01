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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static EliteDangerousCore.BindingsFile;

namespace EliteDangerousCore
{
    public partial class BindingsEditor : UserControl
    {
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

                System.Diagnostics.Debug.WriteLine($"Selected index changed {editingcell.RowIndex} {editingcell.ColumnIndex} = {newcellvalue}");

                int ci = editingcell.ColumnIndex;

                BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

                // get entry device name for frontier. This depends if we are editing a key or a device cell
                string dev = OriginalDeviceName((ci-ColPrimaryDevice.Index)%2==0 ? newcellvalue : row.Cells[ci - 1].Value.ToString());
                bool nodevice = DeviceKeyPair.IsNoDevice(dev);

                if (ci == ColPrimaryDevice.Index)
                {
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    row.Cells[ci + 1].ReadOnly = nodevice;

                    AddKeyOptions(entry.IsBinding, dev, (DataGridViewComboBoxCell)row.Cells[ci + 1]);

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

                        if (entry.IsBinding == false)
                        {
                            row.Cells[ci + 2].ReadOnly = false;    // else enable the mod device
                            row.Cells[ci + 4].ReadOnly = false;    // else enable the secondary device. Device will be already filled
                        }
                    }

                    SetDirty();
                }
                else if (ci == ColPrimaryKey.Index)
                {
                    entry.PrimaryKeys.Keys[0] = new DeviceKeyPair(dev,  newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColPrimaryModDevice.Index)
                {
                    if (nodevice)     // if no device, clear the mod
                        entry.PrimaryKeys.ClearMod();

                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ReadOnly = nodevice;
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    AddKeyOptions(entry.IsBinding, dev, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColPrimaryModKey.Index)
                {
                    entry.PrimaryKeys.SetMod(dev, newcellvalue);     // either add mod or change current mod
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColSecondaryDevice.Index)
                {
                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    row.Cells[ci + 1].ReadOnly = nodevice;

                    AddKeyOptions(entry.IsBinding, dev, (DataGridViewComboBoxCell)row.Cells[ci + 1]);

                    if (nodevice)                  // no device clears everything to the right for primary
                    {
                        for (int i = 2; i < 4; i++)
                        {
                            row.Cells[ci + i].ReadOnly = true;
                            row.Cells[ci + i].Value = "";
                            row.Cells[ci + i].ErrorText = "";
                        }

                        entry.SecondaryKeys.Clear();
                    }
                    else
                    {
                        row.Cells[ci + 2].ReadOnly = row.Cells[ci + 3].ReadOnly = false;    // else enable the mod and key. Device will be already filled
                    }

                    SetDirty();
                }
                else if (ci == ColSecondaryKey.Index)
                {
                    entry.SecondaryKeys.Keys[0] = new DeviceKeyPair(dev, newcellvalue);
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }
                else if (ci == ColSecondaryModDevice.Index)
                {
                    if (nodevice)     // if no device, clear the mod
                        entry.SecondaryKeys.ClearMod();

                    row.Cells[ci + 1].Value = "";
                    row.Cells[ci + 1].ReadOnly = nodevice;
                    row.Cells[ci + 1].ErrorText = nodevice ? null : "Invalid - enter a value";
                    AddKeyOptions(entry.IsBinding, dev, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColSecondaryModKey.Index)
                {
                    entry.SecondaryKeys.SetMod(dev, newcellvalue);     // either add mod or change current mod
                    row.Cells[ci].ErrorText = null;
                    SetDirty();
                }

                System.Diagnostics.Debug.WriteLine($"Entry {entry.ToString()}");

                IndicateErrors();
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (edc != null)
                edc.SelectedIndexChanged -= Edc_SelectedIndexChanged;

            if (e.ColumnIndex == ColPrimaryModDevice.Index || e.ColumnIndex == ColSecondaryDevice.Index || e.ColumnIndex == ColSecondaryModDevice.Index)
            {
                DataGridViewComboBoxCell c = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;
                string selected = c.Value as string;
                bool nodevice = selected == DeviceKeyPair.NoDeviceName;
                if (nodevice)
                    c.Value = "";       // this clears the cell here, can't do it in EDC selected index
            }
        }

        #endregion


        #region UI Cell
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // unused for now
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && bf.IsEditable)
            {
                var row = dataGridView.Rows[e.RowIndex];
                int ci = e.ColumnIndex;

                if (ci == ColValues.Index)
                    ValueEdit(row);     // will reject non attribute entry
                else
                    DirectInput(row.Cells[ci]);     // will reject any not allowed
            }
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter )
            {
                e.Handled = true;       // stops normal DGV op
            }
        }

        private void dataGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && dataGridView.CurrentCell != null && bf.IsEditable)
            {
                int ci = dataGridView.CurrentCell.ColumnIndex;
                if (ci == ColValues.Index)
                    ValueEdit(dataGridView.Rows[dataGridView.CurrentCell.RowIndex]);     // will reject non attribute entry
                else
                    DirectInput(dataGridView.CurrentCell);     // will reject any not allowed
                e.Handled = true;
            }
        }

        // Direct Input of key/button/axis from controllers. Will reject if c is not compatible
        private void DirectInput(DataGridViewCell c)
        {
            var row = dataGridView.Rows[c.RowIndex];
            BindingEntry entry = row.Tag as BindingEntry;
            int ci = c.ColumnIndex;

            // need to have a key or binding, be either in primary position or be a key for further on

            if (entry.IsKeyOrBinding && (ci == ColPrimaryDevice.Index || ci == ColPrimaryKey.Index || (ci >= ColPrimaryModDevice.Index && entry.IsKey)))
            {
                var dkp = DeviceInput?.Invoke(bf, entry);

                if (dkp != null)
                {
                    // dkp contains converted device name, not the binding file name

                    DeviceKeyPair extdkp = new DeviceKeyPair(OriginalDeviceName(dkp.Device), dkp.FrontierKeyName);

                    System.Diagnostics.Debug.WriteLine($"Direct Input {c.RowIndex}: {ci} Key press returned {dkp.Device} {dkp.FrontierKeyName} -> {extdkp.Device} {extdkp.FrontierKeyName}");

                    if (ci == ColPrimaryDevice.Index || ci == ColPrimaryKey.Index)
                    {
                        SetPair(row, entry.IsBinding, ColPrimaryDevice.Index, dkp.Device, dkp.FrontierKeyName);
                        entry.PrimaryKeys.Keys[0] = extdkp;

                        if (entry.IsBinding == false)
                        {
                            row.Cells[ci + 2].ReadOnly = false;    // else enable the mod device
                            row.Cells[ci + 4].ReadOnly = false;    // else enable the secondary device. Device will be already filled
                        }
                    }
                    else if (ci == ColPrimaryModDevice.Index || ci == ColPrimaryModKey.Index)
                    {
                        SetPair(row, false, ColPrimaryModDevice.Index, dkp.Device, dkp.FrontierKeyName);
                        entry.PrimaryKeys.SetMod(extdkp.Device, extdkp.FrontierKeyName);
                    }

                    else if (ci == ColSecondaryDevice.Index || ci == ColSecondaryKey.Index)
                    {
                        SetPair(row, false, ColSecondaryDevice.Index, dkp.Device, dkp.FrontierKeyName);
                        entry.SecondaryKeys.Keys[0] = extdkp;

                        row.Cells[ColSecondaryModDevice.Index].ReadOnly = false;    // etheselse enable the mod device
                    }
                    else if (ci == ColSecondaryModDevice.Index || ci == ColSecondaryModKey.Index)
                    {
                        SetPair(row, false, ColSecondaryModDevice.Index, dkp.Device, dkp.FrontierKeyName);
                        entry.SecondaryKeys.SetMod(extdkp.Device, extdkp.FrontierKeyName);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    SetDirty();
                }

                System.Diagnostics.Debug.WriteLine($"Entry {entry.ToString()}");

                IndicateErrors();
            }
        }

        // edit values in row. Will reject if does not have attributes
        private void ValueEdit(DataGridViewRow row)
        {
            BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

            // pick values or attributes
            bool valuemode = entry.Values.Count > 0;
            Dictionary<string, string> dict = valuemode ? entry.Values : entry.Attributes;

            if (dict.Count > 0)
            {
                row.Selected = true;

                // find any fixed options and indicate to dialog for combobox options

                Dictionary<string, string[]> fixedoptions = new Dictionary<string, string[]>();
                Variables vars = new Variables();
                foreach (var x in dict)
                {
                    vars[x.Key] = x.Value;
                    // see if we have a combobox option for this pair
                    var options = FrontierBindingClassification.GetValueOptions(valuemode ? x.Key : entry.Name, x.Value);
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
            }

        }

        #endregion

    }
}
