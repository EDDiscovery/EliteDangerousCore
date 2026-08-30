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

                        entry.ClearAll(bf.NoDeviceName);
                    }
                    else
                    {
                        // entry in BF is left alone, if you quit without setting the key it comes back

                        if (entry.Binding == false)
                        {
                            row.Cells[ci + 2].ReadOnly = false;    // else enable the mod device
                            row.Cells[ci + 4].ReadOnly = false;    // else enable the secondary device. Device will be already filled
                        }
                    }

                    SetDirty();
                }
                else if (ci == ColPrimaryKey.Index)
                {
                    entry.PrimaryKeys.Keys[0] = new DeviceKeyPair(row.Cells[ci - 1].Value.ToString(), newcellvalue);
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
                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColPrimaryModKey.Index)
                {
                    entry.PrimaryKeys.SetMod(row.Cells[ci - 1].Value.ToString(), newcellvalue);     // either add mod or change current mod
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

                        entry.SecondaryKeys.Clear(bf.NoDeviceName);
                    }
                    else
                    {
                        row.Cells[ci + 2].ReadOnly = row.Cells[ci + 3].ReadOnly = false;    // else enable the mod and key. Device will be already filled
                    }

                    SetDirty();
                }
                else if (ci == ColSecondaryKey.Index)
                {
                    entry.SecondaryKeys.Keys[0] = new DeviceKeyPair(row.Cells[ci - 1].Value.ToString(), newcellvalue);
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
                    AddKeyOptions(entry.Binding, newcellvalue, (DataGridViewComboBoxCell)row.Cells[ci + 1]);
                    SetDirty();
                }
                else if (ci == ColSecondaryModKey.Index)
                {
                    entry.SecondaryKeys.SetMod(row.Cells[ci - 1].Value.ToString(), newcellvalue);     // either add mod or change current mod
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
                bool nodevice = selected == bf.NoDeviceName;
                if (nodevice)
                    c.Value = "";       // this clears the cell here, can't do it in EDC selected index
            }
        }

        #endregion





        #region UI Cell
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !bf.IsEditable)
                return;

            var row = dataGridView.Rows[e.RowIndex];
            BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

            System.Diagnostics.Debug.WriteLine($"Entry {entry.ToString()}");

            // we only operate on column index of values

            if (e.ColumnIndex != ColValues.Index)
                return;

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
                row.Selected = false;
            }
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if ( e.RowIndex>=0 && e.ColumnIndex>= ColPrimaryDevice.Index)
            {
                DirectInput(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]);
            }

        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && dataGridView.CurrentCell != null && bf.IsEditable)
            {
                DirectInput(dataGridView.CurrentCell);
                e.Handled = true;
            }
        }

        private void DirectInput(DataGridViewCell c)
        {
            var row = dataGridView.CurrentRow;
            BindingsFile.BindingEntry entry = row.Tag as BindingsFile.BindingEntry;

            var dkp = DeviceInput?.Invoke(bf,entry);

            if (dkp != null)
            {
                System.Diagnostics.Debug.WriteLine($"Key press returned {dkp.Device} {dkp.FrontierKeyName}");

                int ci = dataGridView.CurrentCell.ColumnIndex;

                if (ci == ColPrimaryDevice.Index || ci == ColPrimaryKey.Index)
                {
                    SetPair(row, entry.Binding, ColPrimaryDevice.Index, dkp.Device, dkp.FrontierKeyName);
                    entry.PrimaryKeys.Keys[0] = new DeviceKeyPair(dkp.Device, dkp.FrontierKeyName);

                    if (entry.Binding == false)
                    {
                        row.Cells[ci + 2].ReadOnly = false;    // else enable the mod device
                        row.Cells[ci + 4].ReadOnly = false;    // else enable the secondary device. Device will be already filled
                    }
                }
                else if (ci == ColPrimaryModDevice.Index || ci == ColPrimaryModKey.Index)
                {
                    SetPair(row, false, ColPrimaryModDevice.Index, dkp.Device, dkp.FrontierKeyName);
                    entry.PrimaryKeys.SetMod(dkp.Device, dkp.FrontierKeyName);
                }

                else if (ci == ColSecondaryDevice.Index || ci == ColSecondaryKey.Index)
                {
                    SetPair(row, false, ColSecondaryDevice.Index, dkp.Device, dkp.FrontierKeyName);
                    entry.SecondaryKeys.Keys[0] = new DeviceKeyPair(dkp.Device, dkp.FrontierKeyName);

                    row.Cells[ColSecondaryModDevice.Index].ReadOnly = false;    // etheselse enable the mod device
                }
                else if (ci == ColSecondaryModDevice.Index || ci == ColSecondaryModKey.Index)
                {
                    SetPair(row, false, ColSecondaryModDevice.Index, dkp.Device, dkp.FrontierKeyName);
                    entry.SecondaryKeys.SetMod(dkp.Device, dkp.FrontierKeyName);
                }

                SetDirty();
            }

            System.Diagnostics.Debug.WriteLine($"Entry {entry.ToString()}");

            IndicateErrors();
        }

        #endregion

    }
}
