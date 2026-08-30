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

            BindingEntry entry = null;

            foreach ( var row in rcrows)
            {
                dataGridView.SetCurrentAndSelectAllCellsOnRow(row.Index, true);     // select all cells to light them up
                entry = (BindingEntry)row.Tag;
                doeshaveanykeys |= entry.Assigned;
                doeshavesecondary |= entry.HasPrimaryAndSecondaryAssigned;
                System.Diagnostics.Debug.WriteLine($"Row check {row.Index} {entry.Name}");
            }

            defineByKeyJoystickToolStripMenuItem.Enabled = dataGridView.HitColumn >= ColPrimaryDevice.Index && rcrows.Length== 1
                            && DeviceInput != null && entry.KeyOrBinding;

            clearPrimaryToolStripMenuItem.Enabled =
            clearAllToolStripMenuItem.Enabled = doeshaveanykeys;

            moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Enabled =
            clearSecondaryToolStripMenuItem.Enabled =
            swapPrimaryAndSecondaryToolStripMenuItem.Enabled = doeshavesecondary;
        }

        private void defineByKeyJoystickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var entry = (BindingEntry)rcrows[0].Tag;
            //DeviceInput?.Invoke(entry.Name, entry.Binding);
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
                    System.Diagnostics.Debug.WriteLine($"Entry is {entry.ToString()}");
                }
            }

            IndicateErrors();
        }

        private void clearPrimaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;

                if (entry.KeyOrBinding)
                {
                    if (entry.HasPrimaryAndSecondaryAssigned)
                    {
                        entry.SwapPrimarySecondary();
                        entry.SecondaryKeys.Clear(bf.NoDeviceName);
                    }
                    else
                    {
                        entry.PrimaryKeys.Clear(bf.NoDeviceName);
                    }

                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Entry is {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void clearSecondaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.HasPrimaryAndSecondaryAssigned)
                {
                    entry.SecondaryKeys.Clear(bf.NoDeviceName);
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Entry is {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;
                if (entry.HasPrimaryAndSecondaryAssigned && entry.SecondaryKeys.IsJoystick(bf.NoDeviceName))
                {
                    entry.SwapPrimarySecondary();
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Entry is {entry.ToString()}");
                }
            }
            IndicateErrors();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in rcrows)
            {
                var entry = (BindingEntry)row.Tag;

                if (entry.Assigned)
                {
                    entry.ClearAll(bf.NoDeviceName);
                    SetUpCells(row, entry);
                    SetDirty();
                    System.Diagnostics.Debug.WriteLine($"Entry is {entry.ToString()}");
                }
            }
            IndicateErrors();
        }
        private void showFrontierNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Display();
        }

    }
}
