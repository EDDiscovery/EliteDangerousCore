/*
 * Copyright © 2016-2021 EDDiscovery development team
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
 *
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace EliteDangerousCore.Forms
{
    public partial class CommanderForm : ExtendedControls.DraggableForm
    {
        public bool Valid { get { return textBoxBorderCmdr.Text != ""; } }
        public string CommanderName { get { return (extCheckBoxConsoleCommander.Checked ? "[C] " : "") + textBoxBorderCmdr.Text; } }

        // CAPI root name
        public string CommanderRootName { get { return EDCommander.GetRootName(CommanderName); } }

        public CommanderForm(List<ExtendedControls.ExtGroupBox> additionalcontrols = null)
        {
            InitializeComponent();

            var enumlist = new Enum[] { EDCTx.CommanderForm, EDCTx.CommanderForm_extGroupBoxCommanderInfo, EDCTx.CommanderForm_HomeSys, EDCTx.CommanderForm_labelMapCol, EDCTx.CommanderForm_groupBoxCustomIGAU, EDCTx.CommanderForm_checkBoxIGAUSync, EDCTx.CommanderForm_extGroupBoxEDAstro, EDCTx.CommanderForm_extCheckBoxEDAstro, EDCTx.CommanderForm_groupBoxCustomInara, EDCTx.CommanderForm_labelINARAN, EDCTx.CommanderForm_labelInaraAPI, EDCTx.CommanderForm_checkBoxCustomInara, EDCTx.CommanderForm_groupBoxCustomEDSM, EDCTx.CommanderForm_checkBoxCustomEDSMFrom, EDCTx.CommanderForm_labelEDSMAPI, EDCTx.CommanderForm_labelEDSMN, EDCTx.CommanderForm_checkBoxCustomEDSMTo, 
                                        EDCTx.CommanderForm_groupBoxCustomEDDN, EDCTx.CommanderForm_checkBoxCustomEDDNTo, EDCTx.CommanderForm_groupBoxCustomJournal, 
                                        EDCTx.CommanderForm_labelCN, EDCTx.CommanderForm_labelJL, EDCTx.CommanderForm_buttonExtBrowse, EDCTx.CommanderForm_extCheckBoxConsoleCommander,
                                        EDCTx.CommanderForm_extCheckBoxIncludeSubfolders};
            var enumlisttt = new Enum[] { EDCTx.CommanderForm_panel_defaultmapcolor_ToolTip, EDCTx.CommanderForm_checkBoxIGAUSync_ToolTip, EDCTx.CommanderForm_checkBoxCustomInara_ToolTip, EDCTx.CommanderForm_textBoxBorderInaraAPIKey_ToolTip, EDCTx.CommanderForm_textBoxBorderInaraName_ToolTip, EDCTx.CommanderForm_checkBoxCustomEDSMFrom_ToolTip, EDCTx.CommanderForm_checkBoxCustomEDSMTo_ToolTip, EDCTx.CommanderForm_textBoxBorderEDSMAPI_ToolTip, EDCTx.CommanderForm_textBoxBorderEDSMName_ToolTip, EDCTx.CommanderForm_checkBoxCustomEDDNTo_ToolTip, EDCTx.CommanderForm_textBoxBorderCmdr_ToolTip, EDCTx.CommanderForm_buttonExtBrowse_ToolTip, EDCTx.CommanderForm_textBoxBorderJournal_ToolTip };

            BaseUtils.Translator.Instance.TranslateControls(this, enumlist);          // before additional controls
            BaseUtils.Translator.Instance.TranslateTooltip(toolTip, enumlisttt, this);

            if (additionalcontrols != null)
            {
                foreach (Control c in additionalcontrols)
                    c.Dock = DockStyle.Top;

                panelGroups.Controls.InsertRangeBefore(groupBoxCustomInara, additionalcontrols);
            }

            var theme = ExtendedControls.Theme.Current;
            bool winborder = theme.ApplyDialog(this);
            panelTop.Visible = panelTop.Enabled = !winborder;

            label_index.Text = this.Text;
        }

        private void InitInt(bool enablecmdredit, bool disablefromedsm, bool disable3dmapsettings, bool disableconsolesupport)
        {
            textBoxBorderCmdr.Enabled = enablecmdredit;
            checkBoxCustomEDSMFrom.Visible = !disablefromedsm;
            if ( disable3dmapsettings)
            {
                panelGroups.Controls.Remove(extGroupBoxCommanderInfo);
            }
            checkBoxCustomEDDNTo.Checked = true;        // default EDDN on
            extCheckBoxConsoleCommander.Visible = !disableconsolesupport;
        }

        public void Init(bool enablecmdredit, bool disablefromedsm = false, bool disable3dmapsettings = false, bool disableconsolesupport = false)
        {
            InitInt(enablecmdredit, disablefromedsm, disable3dmapsettings, disableconsolesupport);
            extCheckBoxConsoleCommander.CheckedChanged += ExtCheckBoxConsoleCommander_CheckedChanged;
        }

        public void Init(EDCommander cmdr, bool enablecmdredit, bool disablefromedsm = false, bool disable3dmapsettings = false, bool disableconsolesupport = false)
        {
            InitInt(enablecmdredit, disablefromedsm, disable3dmapsettings, disableconsolesupport);

            textBoxBorderCmdr.Text = cmdr.Name.ReplaceIfStartsWith("[C] ", "");

            if (!disableconsolesupport)
            {
                extCheckBoxConsoleCommander.Checked = cmdr.ConsoleCommander;
                extCheckBoxConsoleCommander.Enabled = false;
                if (cmdr.ConsoleCommander)
                    textBoxBorderJournal.Enabled = buttonExtBrowse.Enabled = false;
                else
                    textBoxBorderJournal.Text = cmdr.JournalDir;
            }

            textBoxBorderEDSMName.Text = cmdr.EdsmName;
            textBoxBorderEDSMAPI.Text = cmdr.EDSMAPIKey;
            checkBoxCustomEDSMFrom.Checked = cmdr.SyncFromEdsm;
            checkBoxCustomEDSMTo.Checked = cmdr.SyncToEdsm;
            checkBoxCustomEDDNTo.Checked = cmdr.SyncToEddn;
            checkBoxIGAUSync.Checked = cmdr.SyncToIGAU;
            textBoxBorderInaraAPIKey.Text = cmdr.InaraAPIKey;
            textBoxBorderInaraName.Text = cmdr.InaraName;
            checkBoxCustomInara.Checked = cmdr.SyncToInara;
            extCheckBoxEDAstro.Checked = cmdr.SyncToEDAstro;
            extCheckBoxIncludeSubfolders.Checked = cmdr.IncludeSubFolders;

            extTextBoxAutoCompleteHomeSystem.Text = cmdr.HomeSystem;
            extTextBoxAutoCompleteHomeSystem.SetAutoCompletor(EliteDangerousCore.DB.SystemCache.ReturnSystemAutoCompleteList, true);

            panel_defaultmapcolor.BackColor = System.Drawing.Color.FromArgb(cmdr.MapColour);
            panel_defaultmapcolor.Click += Panel_defaultmapcolor_Click;

            // nov 22 update 14 inara/edsm don't want legacy data

            textBoxBorderEDSMAPI.Enabled = textBoxBorderEDSMName.Enabled = 
            textBoxBorderInaraAPIKey.Enabled = textBoxBorderInaraName.Enabled =
            checkBoxCustomEDSMFrom.Enabled = checkBoxCustomEDSMTo.Enabled = checkBoxCustomInara.Enabled = !cmdr.LegacyCommander;
        }

        public bool Update(EDCommander cmdr)
        {
            bool update = cmdr.JournalDir != textBoxBorderJournal.Text ||                   // changing these means need to resync system and start up stuff
                          cmdr.EdsmName != textBoxBorderEDSMName.Text ||
                          cmdr.EDSMAPIKey != textBoxBorderEDSMAPI.Text ||
                          cmdr.SyncFromEdsm != checkBoxCustomEDSMFrom.Checked ||
                          cmdr.SyncToEdsm != checkBoxCustomEDSMTo.Checked ||
                          cmdr.IncludeSubFolders != extCheckBoxIncludeSubfolders.Checked;

            cmdr.Name = CommanderName;
            if (extCheckBoxConsoleCommander.Checked)
                cmdr.ConsoleCommander = true;
            else
                cmdr.JournalDir = textBoxBorderJournal.Text;
            cmdr.EdsmName = textBoxBorderEDSMName.Text;
            cmdr.EDSMAPIKey = textBoxBorderEDSMAPI.Text;
            cmdr.SyncFromEdsm = checkBoxCustomEDSMFrom.Checked;
            cmdr.SyncToEdsm = checkBoxCustomEDSMTo.Checked;
            cmdr.SyncToEddn = checkBoxCustomEDDNTo.Checked;
            cmdr.SyncToIGAU = checkBoxIGAUSync.Checked;
            cmdr.InaraAPIKey = textBoxBorderInaraAPIKey.Text;
            cmdr.InaraName = textBoxBorderInaraName.Text;
            cmdr.SyncToInara = checkBoxCustomInara.Checked;
            cmdr.SyncToEDAstro = extCheckBoxEDAstro.Checked;
            cmdr.HomeSystem = extTextBoxAutoCompleteHomeSystem.Text;
            cmdr.MapColour = panel_defaultmapcolor.BackColor.ToArgb();
            cmdr.IncludeSubFolders = extCheckBoxIncludeSubfolders.Checked;

            return update;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                extPanelScroll.Dock = System.Windows.Forms.DockStyle.None;
                panelOK.Dock = System.Windows.Forms.DockStyle.None;
                int bh = 50;
                int vsplit = ClientRectangle.Height-bh;
                int width = ClientRectangle.Width;
                extPanelScroll.Bounds = new System.Drawing.Rectangle(0,0,width,vsplit);
                panelOK.Bounds = new System.Drawing.Rectangle(0,vsplit,width,bh);

            }
        }


        #region UI

        private void Panel_defaultmapcolor_Click(object sender, EventArgs e)
        {
            ColorDialog mapColorDialog = new ColorDialog();
            mapColorDialog.AllowFullOpen = true;
            mapColorDialog.FullOpen = true;
            mapColorDialog.Color = panel_defaultmapcolor.BackColor;
            if (mapColorDialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                panel_defaultmapcolor.BackColor = mapColorDialog.Color;
            }
        }

        private void buttonExtBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select folder where Journal*.log files are stored by Frontier in".T(EDCTx.CommanderForm_LF);

            if (fbd.ShowDialog(this) == DialogResult.OK)
                textBoxBorderJournal.Text = fbd.SelectedPath;

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxBorderJournal.Text.HasChars() && !Directory.Exists(textBoxBorderJournal.Text))
            {
                ExtendedControls.MessageBoxTheme.Show(this, "Folder does not exist".T(EDCTx.CommanderForm_ND), "Warning".TxID(EDCTx.Warning), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Window Control

        private void label_index_MouseDown(object sender, MouseEventArgs e)
        {
            OnCaptionMouseDown((Control)sender, e);
        }

        private void panel_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ExtCheckBoxConsoleCommander_CheckedChanged(object sender, EventArgs e)
        {
            if (extCheckBoxConsoleCommander.Checked)
                textBoxBorderJournal.Text = "";

            textBoxBorderJournal.Enabled = buttonExtBrowse.Enabled = !extCheckBoxConsoleCommander.Checked;
        }

        private void panel_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
