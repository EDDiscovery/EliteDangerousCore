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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using static EliteDangerousCore.BindingsFile;

namespace EliteDangerousCore
{
    public partial class BindingsEditor : UserControl
    {
        DataGridViewRow[] rcrows;

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            bool doeshaveanykeys = false, doeshavesecondary = false;

            // build up rows to address

            rcrows = null;

            int[] selectedrows = dataGridView.SelectedRows(true, true);     // see how many rows we have in selection
            if ( selectedrows.Length > 1)       // if more than 1, then we use this as the selection
            {   
                rcrows = dataGridView.SelectedCells.OfType<DataGridViewCell>().OrderBy(x=>x.RowIndex).Select(x => x.OwningRow).Distinct().ToArray();
            }
            else
            {
                rcrows = new DataGridViewRow[] { dataGridView.Rows[dataGridView.HitIndex] };
            }

            dataGridView.ClearSelection();

            HashSet<string> devicespresent = new HashSet<string>();     // build a list of devices across the range

            // go thru the range, select the cells, build up options
            foreach ( var row in rcrows)
            {
                dataGridView.SetCurrentAndSelectAllCellsOnRow(row.Index, true);
                var rowentry = (BindingEntry)row.Tag;
                doeshaveanykeys |= rowentry.IsAssigned;
                doeshavesecondary |= rowentry.IsPrimaryAndSecondaryAssigned;
                System.Diagnostics.Debug.WriteLine($"Row check {row.Index} {rowentry.Name}");
                foreach (var d in rowentry.Devices())
                    devicespresent.Add(d);
            }

            defineByKeyJoystickToolStripMenuItem.Visible = dataGridView.HitColumn >= ColPrimaryDevice.Index && rcrows.Length== 1
                            && DeviceInput != null && ((BindingEntry)rcrows[0].Tag).IsKeyOrBinding;

            clearPrimaryToolStripMenuItem.Visible = clearAllToolStripMenuItem.Visible = doeshaveanykeys;

            moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Visible =
            clearSecondaryToolStripMenuItem.Visible =
            swapPrimaryAndSecondaryToolStripMenuItem.Visible = doeshavesecondary;

            // if devices are present in range, build up options to delete them
            removeDeviceToolStripMenuItem.Visible = devicespresent.Count > 0;
            if (devicespresent.Count > 0)
            {
                removeDeviceToolStripMenuItem.DropDownItems.Clear();
                foreach (var x in devicespresent )
                {
                    ToolStripMenuItem tsi = new ToolStripMenuItem() { Name = x, Text = BetterDevice(x)};
                    tsi.Click += (s1, e1) => {
                        foreach (DataGridViewRow row in rcrows)
                        {
                            var rowentry = (BindingEntry)row.Tag;
                            if (rowentry.RemoveDevice(x))
                            {
                                SetUpCells(row, rowentry);
                                SetDirty();
                                System.Diagnostics.Debug.WriteLine($"Removed device on entry {rowentry.ToString()}");
                            }
                        }
                    };

                    removeDeviceToolStripMenuItem.DropDownItems.Add(tsi);
                }
            }
        }

        private void defineByKeyJoystickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( dataGridView.HitIndex>=0 && dataGridView.HitColumn >= ColPrimaryDevice.Index )
            {
                DirectInput(dataGridView.Rows[dataGridView.HitIndex].Cells[dataGridView.HitColumn]);
            }
        }

        private void swapPrimaryAndSecondaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.SwapPrimarySecondary())
                {
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Swap entries {entry.ToString()}");
                }
            }

            IndicateErrors();
        }

        private void clearPrimaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;

                if (entry.IsKeyOrBinding)
                {
                    if (entry.IsPrimaryAndSecondaryAssigned)
                    {
                        entry.SwapPrimarySecondary();
                        entry.SecondaryKeys.Clear();
                    }
                    else
                    {
                        entry.PrimaryKeys.Clear();
                    }

                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Clear primary {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void clearSecondaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.IsPrimaryAndSecondaryAssigned)
                {
                    entry.SecondaryKeys.Clear();
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Clear secondary {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.IsPrimaryAndSecondaryAssigned && entry.SecondaryKeys.IsJoystick())
                {
                    entry.SwapPrimarySecondary();
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Move joystick {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;

                if (entry.IsAssigned)
                {
                    entry.ClearAll();
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Clear all {entry.ToString()}");
                }
            }
            IndicateErrors();
        }
        private void showFrontierNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Display();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach( DataGridViewRow row in dataGridView.Rows)
                dataGridView.SetCurrentAndSelectAllCellsOnRow(row.Index, true);


        }

    }
}
