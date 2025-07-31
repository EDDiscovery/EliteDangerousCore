namespace EliteDangerousCore.Forms
{ 
    partial class CommanderForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommanderForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.panel_close = new ExtendedControls.ExtButtonDrawn();
            this.panel_minimize = new ExtendedControls.ExtButtonDrawn();
            this.label_index = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxCustomEDDNTo = new ExtendedControls.ExtCheckBox();
            this.checkBoxCustomEDSMFrom = new ExtendedControls.ExtCheckBox();
            this.checkBoxCustomEDSMTo = new ExtendedControls.ExtCheckBox();
            this.buttonExtBrowse = new ExtendedControls.ExtButton();
            this.textBoxBorderEDSMName = new ExtendedControls.ExtTextBox();
            this.textBoxBorderEDSMAPI = new ExtendedControls.ExtTextBox();
            this.textBoxBorderJournal = new ExtendedControls.ExtTextBox();
            this.textBoxBorderCmdr = new ExtendedControls.ExtTextBox();
            this.checkBoxCustomInara = new ExtendedControls.ExtCheckBox();
            this.textBoxBorderInaraName = new ExtendedControls.ExtTextBox();
            this.textBoxBorderInaraAPIKey = new ExtendedControls.ExtTextBox();
            this.panel_defaultmapcolor = new ExtendedControls.ColorPanel();
            this.groupBoxCustomEDDN = new ExtendedControls.ExtGroupBox();
            this.groupBoxCustomEDSM = new ExtendedControls.ExtGroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelEDSMAPI = new System.Windows.Forms.Label();
            this.labelEDSMN = new System.Windows.Forms.Label();
            this.groupBoxCustomJournal = new ExtendedControls.ExtGroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelCN = new System.Windows.Forms.Label();
            this.labelJL = new System.Windows.Forms.Label();
            this.extCheckBoxConsoleCommander = new ExtendedControls.ExtCheckBox();
            this.extCheckBoxIncludeSubfolders = new ExtendedControls.ExtCheckBox();
            this.buttonCancel = new ExtendedControls.ExtButton();
            this.buttonOK = new ExtendedControls.ExtButton();
            this.groupBoxCustomInara = new ExtendedControls.ExtGroupBox();
            this.tableLayoutPanelInara = new System.Windows.Forms.TableLayoutPanel();
            this.labelINARAN = new System.Windows.Forms.Label();
            this.labelInaraAPI = new System.Windows.Forms.Label();
            this.panelOK = new System.Windows.Forms.Panel();
            this.statusStripCustom = new ExtendedControls.ExtStatusStrip();
            this.extGroupBoxCommanderInfo = new ExtendedControls.ExtGroupBox();
            this.tableLayoutPanelCommanderInfo = new System.Windows.Forms.TableLayoutPanel();
            this.extTextBoxAutoCompleteHomeSystem = new ExtendedControls.ExtTextBoxAutoComplete();
            this.HomeSys = new System.Windows.Forms.Label();
            this.labelMapCol = new System.Windows.Forms.Label();
            this.extPanelScroll = new ExtendedControls.ExtPanelScroll();
            this.extScrollBar = new ExtendedControls.ExtScrollBar();
            this.panelGroups = new System.Windows.Forms.Panel();
            this.extGroupBoxEDAstro = new ExtendedControls.ExtGroupBox();
            this.extCheckBoxEDAstro = new ExtendedControls.ExtCheckBox();
            this.panelTop.SuspendLayout();
            this.groupBoxCustomEDDN.SuspendLayout();
            this.groupBoxCustomEDSM.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxCustomJournal.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxCustomInara.SuspendLayout();
            this.tableLayoutPanelInara.SuspendLayout();
            this.panelOK.SuspendLayout();
            this.extGroupBoxCommanderInfo.SuspendLayout();
            this.tableLayoutPanelCommanderInfo.SuspendLayout();
            this.extPanelScroll.SuspendLayout();
            this.panelGroups.SuspendLayout();
            this.extGroupBoxEDAstro.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.AutoSize = true;
            this.panelTop.Controls.Add(this.panel_close);
            this.panelTop.Controls.Add(this.panel_minimize);
            this.panelTop.Controls.Add(this.label_index);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(720, 27);
            this.panelTop.TabIndex = 31;
            this.panelTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_index_MouseDown);
            // 
            // panel_close
            // 
            this.panel_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_close.AutoEllipsis = false;
            this.panel_close.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel_close.BorderColor = System.Drawing.Color.Orange;
            this.panel_close.BorderWidth = 1;
            this.panel_close.ButtonDisabledScaling = 0.25F;
            this.panel_close.Image = null;
            this.panel_close.ImageSelected = ExtendedControls.ExtButtonDrawn.ImageType.Close;
            this.panel_close.Location = new System.Drawing.Point(697, 0);
            this.panel_close.MouseOverColor = System.Drawing.Color.White;
            this.panel_close.MouseSelectedColor = System.Drawing.Color.Green;
            this.panel_close.MouseSelectedColorEnable = true;
            this.panel_close.Name = "panel_close";
            this.panel_close.Padding = new System.Windows.Forms.Padding(6);
            this.panel_close.Selectable = false;
            this.panel_close.Size = new System.Drawing.Size(24, 24);
            this.panel_close.TabIndex = 27;
            this.panel_close.TabStop = false;
            this.panel_close.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.panel_close.UseMnemonic = true;
            this.panel_close.Click += new System.EventHandler(this.panel_close_Click);
            // 
            // panel_minimize
            // 
            this.panel_minimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_minimize.AutoEllipsis = false;
            this.panel_minimize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel_minimize.BorderColor = System.Drawing.Color.Orange;
            this.panel_minimize.BorderWidth = 1;
            this.panel_minimize.ButtonDisabledScaling = 0.25F;
            this.panel_minimize.Image = null;
            this.panel_minimize.ImageSelected = ExtendedControls.ExtButtonDrawn.ImageType.Minimize;
            this.panel_minimize.Location = new System.Drawing.Point(667, 0);
            this.panel_minimize.MouseOverColor = System.Drawing.Color.White;
            this.panel_minimize.MouseSelectedColor = System.Drawing.Color.Green;
            this.panel_minimize.MouseSelectedColorEnable = true;
            this.panel_minimize.Name = "panel_minimize";
            this.panel_minimize.Padding = new System.Windows.Forms.Padding(6);
            this.panel_minimize.Selectable = false;
            this.panel_minimize.Size = new System.Drawing.Size(24, 24);
            this.panel_minimize.TabIndex = 26;
            this.panel_minimize.TabStop = false;
            this.panel_minimize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.panel_minimize.UseMnemonic = true;
            this.panel_minimize.Click += new System.EventHandler(this.panel_minimize_Click);
            // 
            // label_index
            // 
            this.label_index.AutoSize = true;
            this.label_index.Location = new System.Drawing.Point(3, 8);
            this.label_index.Name = "label_index";
            this.label_index.Size = new System.Drawing.Size(43, 13);
            this.label_index.TabIndex = 23;
            this.label_index.Text = "<code>";
            this.label_index.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_index_MouseDown);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // checkBoxCustomEDDNTo
            // 
            this.checkBoxCustomEDDNTo.AutoSize = true;
            this.checkBoxCustomEDDNTo.ButtonGradientDirection = 90F;
            this.checkBoxCustomEDDNTo.CheckBoxColor = System.Drawing.Color.Gray;
            this.checkBoxCustomEDDNTo.CheckBoxGradientDirection = 225F;
            this.checkBoxCustomEDDNTo.CheckBoxInnerColor = System.Drawing.Color.White;
            this.checkBoxCustomEDDNTo.CheckColor = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDDNTo.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDDNTo.DisabledScaling = 0.5F;
            this.checkBoxCustomEDDNTo.ImageIndeterminate = null;
            this.checkBoxCustomEDDNTo.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.checkBoxCustomEDDNTo.ImageUnchecked = null;
            this.checkBoxCustomEDDNTo.Location = new System.Drawing.Point(9, 19);
            this.checkBoxCustomEDDNTo.MouseOverScaling = 1.3F;
            this.checkBoxCustomEDDNTo.MouseSelectedScaling = 1.3F;
            this.checkBoxCustomEDDNTo.Name = "checkBoxCustomEDDNTo";
            this.checkBoxCustomEDDNTo.Size = new System.Drawing.Size(183, 17);
            this.checkBoxCustomEDDNTo.TabIndex = 0;
            this.checkBoxCustomEDDNTo.Text = "Send Event Information to EDDN";
            this.checkBoxCustomEDDNTo.TickBoxReductionRatio = 0.75F;
            this.toolTip.SetToolTip(this.checkBoxCustomEDDNTo, "Click to send journal information to EDDN. EDDN feeds tools such as EDDB, EDSM, I" +
        "nara with data from commanders. All data is made anonymised");
            this.checkBoxCustomEDDNTo.UseVisualStyleBackColor = true;
            // 
            // checkBoxCustomEDSMFrom
            // 
            this.checkBoxCustomEDSMFrom.AutoSize = true;
            this.checkBoxCustomEDSMFrom.ButtonGradientDirection = 90F;
            this.checkBoxCustomEDSMFrom.CheckBoxColor = System.Drawing.Color.Gray;
            this.checkBoxCustomEDSMFrom.CheckBoxGradientDirection = 225F;
            this.checkBoxCustomEDSMFrom.CheckBoxInnerColor = System.Drawing.Color.White;
            this.checkBoxCustomEDSMFrom.CheckColor = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDSMFrom.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDSMFrom.DisabledScaling = 0.5F;
            this.checkBoxCustomEDSMFrom.ImageIndeterminate = null;
            this.checkBoxCustomEDSMFrom.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.checkBoxCustomEDSMFrom.ImageUnchecked = null;
            this.checkBoxCustomEDSMFrom.Location = new System.Drawing.Point(459, 32);
            this.checkBoxCustomEDSMFrom.MouseOverScaling = 1.3F;
            this.checkBoxCustomEDSMFrom.MouseSelectedScaling = 1.3F;
            this.checkBoxCustomEDSMFrom.Name = "checkBoxCustomEDSMFrom";
            this.checkBoxCustomEDSMFrom.Size = new System.Drawing.Size(110, 17);
            this.checkBoxCustomEDSMFrom.TabIndex = 3;
            this.checkBoxCustomEDSMFrom.Text = "Sync From EDSM";
            this.checkBoxCustomEDSMFrom.TickBoxReductionRatio = 0.75F;
            this.toolTip.SetToolTip(this.checkBoxCustomEDSMFrom, "Receive any FSD jumps from EDSM that are on their database but not in EDDiscovery" +
        "");
            this.checkBoxCustomEDSMFrom.UseVisualStyleBackColor = true;
            // 
            // checkBoxCustomEDSMTo
            // 
            this.checkBoxCustomEDSMTo.AutoSize = true;
            this.checkBoxCustomEDSMTo.ButtonGradientDirection = 90F;
            this.checkBoxCustomEDSMTo.CheckBoxColor = System.Drawing.Color.Gray;
            this.checkBoxCustomEDSMTo.CheckBoxGradientDirection = 225F;
            this.checkBoxCustomEDSMTo.CheckBoxInnerColor = System.Drawing.Color.White;
            this.checkBoxCustomEDSMTo.CheckColor = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDSMTo.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomEDSMTo.DisabledScaling = 0.5F;
            this.checkBoxCustomEDSMTo.ImageIndeterminate = null;
            this.checkBoxCustomEDSMTo.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.checkBoxCustomEDSMTo.ImageUnchecked = null;
            this.checkBoxCustomEDSMTo.Location = new System.Drawing.Point(459, 3);
            this.checkBoxCustomEDSMTo.MouseOverScaling = 1.3F;
            this.checkBoxCustomEDSMTo.MouseSelectedScaling = 1.3F;
            this.checkBoxCustomEDSMTo.Name = "checkBoxCustomEDSMTo";
            this.checkBoxCustomEDSMTo.Size = new System.Drawing.Size(96, 17);
            this.checkBoxCustomEDSMTo.TabIndex = 2;
            this.checkBoxCustomEDSMTo.Text = "Sync to EDSM";
            this.checkBoxCustomEDSMTo.TickBoxReductionRatio = 0.75F;
            this.toolTip.SetToolTip(this.checkBoxCustomEDSMTo, "Send your travel and ship data to EDSM");
            this.checkBoxCustomEDSMTo.UseVisualStyleBackColor = true;
            // 
            // buttonExtBrowse
            // 
            this.buttonExtBrowse.BackColor2 = System.Drawing.Color.Red;
            this.buttonExtBrowse.ButtonDisabledScaling = 0.5F;
            this.buttonExtBrowse.GradientDirection = 90F;
            this.buttonExtBrowse.Location = new System.Drawing.Point(459, 32);
            this.buttonExtBrowse.MouseOverScaling = 1.3F;
            this.buttonExtBrowse.MouseSelectedScaling = 1.3F;
            this.buttonExtBrowse.Name = "buttonExtBrowse";
            this.buttonExtBrowse.Size = new System.Drawing.Size(100, 23);
            this.buttonExtBrowse.TabIndex = 4;
            this.buttonExtBrowse.Text = "Browse";
            this.toolTip.SetToolTip(this.buttonExtBrowse, "Browse to the the journal folder");
            this.buttonExtBrowse.UseVisualStyleBackColor = true;
            this.buttonExtBrowse.Click += new System.EventHandler(this.buttonExtBrowse_Click);
            // 
            // textBoxBorderEDSMName
            // 
            this.textBoxBorderEDSMName.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderEDSMName.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderEDSMName.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderEDSMName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderEDSMName.ClearOnFirstChar = false;
            this.textBoxBorderEDSMName.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderEDSMName.EndButtonEnable = true;
            this.textBoxBorderEDSMName.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderEDSMName.EndButtonImage")));
            this.textBoxBorderEDSMName.EndButtonSize16ths = 10;
            this.textBoxBorderEDSMName.EndButtonVisible = false;
            this.textBoxBorderEDSMName.InErrorCondition = false;
            this.textBoxBorderEDSMName.Location = new System.Drawing.Point(231, 3);
            this.textBoxBorderEDSMName.Multiline = false;
            this.textBoxBorderEDSMName.Name = "textBoxBorderEDSMName";
            this.textBoxBorderEDSMName.ReadOnly = false;
            this.textBoxBorderEDSMName.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderEDSMName.SelectionLength = 0;
            this.textBoxBorderEDSMName.SelectionStart = 0;
            this.textBoxBorderEDSMName.Size = new System.Drawing.Size(197, 23);
            this.textBoxBorderEDSMName.TabIndex = 0;
            this.textBoxBorderEDSMName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderEDSMName.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderEDSMName, "Give the name this commander is known as in EDSM");
            this.textBoxBorderEDSMName.WordWrap = true;
            // 
            // textBoxBorderEDSMAPI
            // 
            this.textBoxBorderEDSMAPI.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderEDSMAPI.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderEDSMAPI.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderEDSMAPI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderEDSMAPI.ClearOnFirstChar = false;
            this.textBoxBorderEDSMAPI.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderEDSMAPI.EndButtonEnable = true;
            this.textBoxBorderEDSMAPI.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderEDSMAPI.EndButtonImage")));
            this.textBoxBorderEDSMAPI.EndButtonSize16ths = 10;
            this.textBoxBorderEDSMAPI.EndButtonVisible = false;
            this.textBoxBorderEDSMAPI.InErrorCondition = false;
            this.textBoxBorderEDSMAPI.Location = new System.Drawing.Point(231, 32);
            this.textBoxBorderEDSMAPI.Multiline = false;
            this.textBoxBorderEDSMAPI.Name = "textBoxBorderEDSMAPI";
            this.textBoxBorderEDSMAPI.ReadOnly = false;
            this.textBoxBorderEDSMAPI.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderEDSMAPI.SelectionLength = 0;
            this.textBoxBorderEDSMAPI.SelectionStart = 0;
            this.textBoxBorderEDSMAPI.Size = new System.Drawing.Size(197, 23);
            this.textBoxBorderEDSMAPI.TabIndex = 1;
            this.textBoxBorderEDSMAPI.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderEDSMAPI.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderEDSMAPI, "Enter the API key from the EDSM Website\r\nGet an EDSM API key from https://www.eds" +
        "m.net in \"My account\" menu");
            this.textBoxBorderEDSMAPI.WordWrap = true;
            // 
            // textBoxBorderJournal
            // 
            this.textBoxBorderJournal.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderJournal.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderJournal.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderJournal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderJournal.ClearOnFirstChar = false;
            this.textBoxBorderJournal.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderJournal.EndButtonEnable = true;
            this.textBoxBorderJournal.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderJournal.EndButtonImage")));
            this.textBoxBorderJournal.EndButtonSize16ths = 10;
            this.textBoxBorderJournal.EndButtonVisible = false;
            this.textBoxBorderJournal.InErrorCondition = false;
            this.textBoxBorderJournal.Location = new System.Drawing.Point(231, 32);
            this.textBoxBorderJournal.Multiline = false;
            this.textBoxBorderJournal.Name = "textBoxBorderJournal";
            this.textBoxBorderJournal.ReadOnly = false;
            this.textBoxBorderJournal.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderJournal.SelectionLength = 0;
            this.textBoxBorderJournal.SelectionStart = 0;
            this.textBoxBorderJournal.Size = new System.Drawing.Size(197, 23);
            this.textBoxBorderJournal.TabIndex = 1;
            this.textBoxBorderJournal.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderJournal.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderJournal, resources.GetString("textBoxBorderJournal.ToolTip"));
            this.textBoxBorderJournal.WordWrap = true;
            // 
            // textBoxBorderCmdr
            // 
            this.textBoxBorderCmdr.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderCmdr.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderCmdr.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderCmdr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderCmdr.ClearOnFirstChar = false;
            this.textBoxBorderCmdr.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderCmdr.EndButtonEnable = true;
            this.textBoxBorderCmdr.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderCmdr.EndButtonImage")));
            this.textBoxBorderCmdr.EndButtonSize16ths = 10;
            this.textBoxBorderCmdr.EndButtonVisible = false;
            this.textBoxBorderCmdr.InErrorCondition = false;
            this.textBoxBorderCmdr.Location = new System.Drawing.Point(231, 3);
            this.textBoxBorderCmdr.Multiline = false;
            this.textBoxBorderCmdr.Name = "textBoxBorderCmdr";
            this.textBoxBorderCmdr.ReadOnly = false;
            this.textBoxBorderCmdr.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderCmdr.SelectionLength = 0;
            this.textBoxBorderCmdr.SelectionStart = 0;
            this.textBoxBorderCmdr.Size = new System.Drawing.Size(197, 20);
            this.textBoxBorderCmdr.TabIndex = 0;
            this.textBoxBorderCmdr.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderCmdr.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderCmdr, "Enter commander name as used in Elite Dangerous");
            this.textBoxBorderCmdr.WordWrap = true;
            // 
            // checkBoxCustomInara
            // 
            this.checkBoxCustomInara.AutoSize = true;
            this.checkBoxCustomInara.ButtonGradientDirection = 90F;
            this.checkBoxCustomInara.CheckBoxColor = System.Drawing.Color.Gray;
            this.checkBoxCustomInara.CheckBoxGradientDirection = 225F;
            this.checkBoxCustomInara.CheckBoxInnerColor = System.Drawing.Color.White;
            this.checkBoxCustomInara.CheckColor = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomInara.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.checkBoxCustomInara.DisabledScaling = 0.5F;
            this.checkBoxCustomInara.ImageIndeterminate = null;
            this.checkBoxCustomInara.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.checkBoxCustomInara.ImageUnchecked = null;
            this.checkBoxCustomInara.Location = new System.Drawing.Point(459, 3);
            this.checkBoxCustomInara.MouseOverScaling = 1.3F;
            this.checkBoxCustomInara.MouseSelectedScaling = 1.3F;
            this.checkBoxCustomInara.Name = "checkBoxCustomInara";
            this.checkBoxCustomInara.Size = new System.Drawing.Size(89, 17);
            this.checkBoxCustomInara.TabIndex = 2;
            this.checkBoxCustomInara.Text = "Sync to Inara";
            this.checkBoxCustomInara.TickBoxReductionRatio = 0.75F;
            this.toolTip.SetToolTip(this.checkBoxCustomInara, "Sync with Inara");
            this.checkBoxCustomInara.UseVisualStyleBackColor = true;
            // 
            // textBoxBorderInaraName
            // 
            this.textBoxBorderInaraName.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderInaraName.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderInaraName.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderInaraName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderInaraName.ClearOnFirstChar = false;
            this.textBoxBorderInaraName.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderInaraName.EndButtonEnable = true;
            this.textBoxBorderInaraName.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderInaraName.EndButtonImage")));
            this.textBoxBorderInaraName.EndButtonSize16ths = 10;
            this.textBoxBorderInaraName.EndButtonVisible = false;
            this.textBoxBorderInaraName.InErrorCondition = false;
            this.textBoxBorderInaraName.Location = new System.Drawing.Point(231, 3);
            this.textBoxBorderInaraName.Multiline = false;
            this.textBoxBorderInaraName.Name = "textBoxBorderInaraName";
            this.textBoxBorderInaraName.ReadOnly = false;
            this.textBoxBorderInaraName.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderInaraName.SelectionLength = 0;
            this.textBoxBorderInaraName.SelectionStart = 0;
            this.textBoxBorderInaraName.Size = new System.Drawing.Size(197, 21);
            this.textBoxBorderInaraName.TabIndex = 0;
            this.textBoxBorderInaraName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderInaraName.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderInaraName, "Give the user name for this commander on Inara");
            this.textBoxBorderInaraName.WordWrap = true;
            // 
            // textBoxBorderInaraAPIKey
            // 
            this.textBoxBorderInaraAPIKey.BackErrorColor = System.Drawing.Color.Red;
            this.textBoxBorderInaraAPIKey.BorderColor = System.Drawing.Color.Transparent;
            this.textBoxBorderInaraAPIKey.BorderColor2 = System.Drawing.Color.Transparent;
            this.textBoxBorderInaraAPIKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBorderInaraAPIKey.ClearOnFirstChar = false;
            this.textBoxBorderInaraAPIKey.ControlBackground = System.Drawing.SystemColors.Control;
            this.textBoxBorderInaraAPIKey.EndButtonEnable = true;
            this.textBoxBorderInaraAPIKey.EndButtonImage = ((System.Drawing.Image)(resources.GetObject("textBoxBorderInaraAPIKey.EndButtonImage")));
            this.textBoxBorderInaraAPIKey.EndButtonSize16ths = 10;
            this.textBoxBorderInaraAPIKey.EndButtonVisible = false;
            this.textBoxBorderInaraAPIKey.InErrorCondition = false;
            this.textBoxBorderInaraAPIKey.Location = new System.Drawing.Point(231, 30);
            this.textBoxBorderInaraAPIKey.Multiline = false;
            this.textBoxBorderInaraAPIKey.Name = "textBoxBorderInaraAPIKey";
            this.textBoxBorderInaraAPIKey.ReadOnly = false;
            this.textBoxBorderInaraAPIKey.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxBorderInaraAPIKey.SelectionLength = 0;
            this.textBoxBorderInaraAPIKey.SelectionStart = 0;
            this.textBoxBorderInaraAPIKey.Size = new System.Drawing.Size(197, 23);
            this.textBoxBorderInaraAPIKey.TabIndex = 1;
            this.textBoxBorderInaraAPIKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBoxBorderInaraAPIKey.TextNoChange = "";
            this.toolTip.SetToolTip(this.textBoxBorderInaraAPIKey, "Enter the API key from the Inara Website\r\nGet an Inara API key from https://inara" +
        ".cz");
            this.textBoxBorderInaraAPIKey.WordWrap = true;
            // 
            // panel_defaultmapcolor
            // 
            this.panel_defaultmapcolor.AccessibleDescription = "";
            this.panel_defaultmapcolor.GradientDirection = 0F;
            this.panel_defaultmapcolor.Location = new System.Drawing.Point(231, 23);
            this.panel_defaultmapcolor.Name = "panel_defaultmapcolor";
            this.panel_defaultmapcolor.Size = new System.Drawing.Size(51, 15);
            this.panel_defaultmapcolor.TabIndex = 4;
            this.panel_defaultmapcolor.Tag = "";
            this.panel_defaultmapcolor.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.Color.Red,
        System.Drawing.Color.Red,
        System.Drawing.Color.Red,
        System.Drawing.Color.Red};
            this.toolTip.SetToolTip(this.panel_defaultmapcolor, "New travel entries get this colour on the map");
            // 
            // groupBoxCustomEDDN
            // 
            this.groupBoxCustomEDDN.AutoSize = true;
            this.groupBoxCustomEDDN.BorderColor = System.Drawing.Color.LightGray;
            this.groupBoxCustomEDDN.BorderColor2 = System.Drawing.Color.Gray;
            this.groupBoxCustomEDDN.ChildrenThemed = true;
            this.groupBoxCustomEDDN.Controls.Add(this.checkBoxCustomEDDNTo);
            this.groupBoxCustomEDDN.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCustomEDDN.GradientDirection = 0F;
            this.groupBoxCustomEDDN.Location = new System.Drawing.Point(0, 107);
            this.groupBoxCustomEDDN.Name = "groupBoxCustomEDDN";
            this.groupBoxCustomEDDN.Size = new System.Drawing.Size(691, 55);
            this.groupBoxCustomEDDN.TabIndex = 5;
            this.groupBoxCustomEDDN.TabStop = false;
            this.groupBoxCustomEDDN.Text = "EDDN";
            this.groupBoxCustomEDDN.TextPadding = 0;
            this.groupBoxCustomEDDN.TextStartPosition = -1;
            this.groupBoxCustomEDDN.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.groupBoxCustomEDDN.ThemeColorSet = -1;
            // 
            // groupBoxCustomEDSM
            // 
            this.groupBoxCustomEDSM.AutoSize = true;
            this.groupBoxCustomEDSM.BorderColor = System.Drawing.Color.LightGray;
            this.groupBoxCustomEDSM.BorderColor2 = System.Drawing.Color.Gray;
            this.groupBoxCustomEDSM.ChildrenThemed = true;
            this.groupBoxCustomEDSM.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxCustomEDSM.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCustomEDSM.GradientDirection = 0F;
            this.groupBoxCustomEDSM.Location = new System.Drawing.Point(0, 162);
            this.groupBoxCustomEDSM.Margin = new System.Windows.Forms.Padding(10);
            this.groupBoxCustomEDSM.Name = "groupBoxCustomEDSM";
            this.groupBoxCustomEDSM.Size = new System.Drawing.Size(691, 77);
            this.groupBoxCustomEDSM.TabIndex = 4;
            this.groupBoxCustomEDSM.TabStop = false;
            this.groupBoxCustomEDSM.Text = "EDSM Information (optional)";
            this.groupBoxCustomEDSM.TextPadding = 0;
            this.groupBoxCustomEDSM.TextStartPosition = -1;
            this.groupBoxCustomEDSM.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.groupBoxCustomEDSM.ThemeColorSet = -1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.checkBoxCustomEDSMFrom, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelEDSMAPI, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelEDSMN, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxCustomEDSMTo, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxBorderEDSMAPI, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxBorderEDSMName, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(685, 58);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // labelEDSMAPI
            // 
            this.labelEDSMAPI.AutoSize = true;
            this.labelEDSMAPI.Location = new System.Drawing.Point(3, 32);
            this.labelEDSMAPI.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelEDSMAPI.Name = "labelEDSMAPI";
            this.labelEDSMAPI.Size = new System.Drawing.Size(82, 13);
            this.labelEDSMAPI.TabIndex = 2;
            this.labelEDSMAPI.Text = "EDSM API Key";
            // 
            // labelEDSMN
            // 
            this.labelEDSMN.AutoSize = true;
            this.labelEDSMN.Location = new System.Drawing.Point(3, 3);
            this.labelEDSMN.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelEDSMN.Name = "labelEDSMN";
            this.labelEDSMN.Size = new System.Drawing.Size(72, 13);
            this.labelEDSMN.TabIndex = 2;
            this.labelEDSMN.Text = "EDSM Name";
            // 
            // groupBoxCustomJournal
            // 
            this.groupBoxCustomJournal.AutoSize = true;
            this.groupBoxCustomJournal.BorderColor = System.Drawing.Color.LightGray;
            this.groupBoxCustomJournal.BorderColor2 = System.Drawing.Color.Gray;
            this.groupBoxCustomJournal.ChildrenThemed = true;
            this.groupBoxCustomJournal.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxCustomJournal.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCustomJournal.GradientDirection = 0F;
            this.groupBoxCustomJournal.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCustomJournal.Name = "groupBoxCustomJournal";
            this.groupBoxCustomJournal.Size = new System.Drawing.Size(691, 107);
            this.groupBoxCustomJournal.TabIndex = 3;
            this.groupBoxCustomJournal.TabStop = false;
            this.groupBoxCustomJournal.Text = "Journal Related Information";
            this.groupBoxCustomJournal.TextPadding = 0;
            this.groupBoxCustomJournal.TextStartPosition = -1;
            this.groupBoxCustomJournal.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.groupBoxCustomJournal.ThemeColorSet = -1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.labelCN, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxBorderCmdr, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelJL, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonExtBrowse, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxBorderJournal, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.extCheckBoxConsoleCommander, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.extCheckBoxIncludeSubfolders, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(685, 88);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // labelCN
            // 
            this.labelCN.AutoSize = true;
            this.labelCN.Location = new System.Drawing.Point(3, 3);
            this.labelCN.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelCN.Name = "labelCN";
            this.labelCN.Size = new System.Drawing.Size(97, 13);
            this.labelCN.TabIndex = 2;
            this.labelCN.Text = "Commander Name";
            // 
            // labelJL
            // 
            this.labelJL.AutoSize = true;
            this.labelJL.Location = new System.Drawing.Point(3, 32);
            this.labelJL.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelJL.Name = "labelJL";
            this.labelJL.Size = new System.Drawing.Size(88, 13);
            this.labelJL.TabIndex = 2;
            this.labelJL.Text = "Journal Location";
            // 
            // extCheckBoxConsoleCommander
            // 
            this.extCheckBoxConsoleCommander.AutoSize = true;
            this.extCheckBoxConsoleCommander.ButtonGradientDirection = 90F;
            this.extCheckBoxConsoleCommander.CheckBoxColor = System.Drawing.Color.Gray;
            this.extCheckBoxConsoleCommander.CheckBoxGradientDirection = 225F;
            this.extCheckBoxConsoleCommander.CheckBoxInnerColor = System.Drawing.Color.White;
            this.extCheckBoxConsoleCommander.CheckColor = System.Drawing.Color.DarkBlue;
            this.extCheckBoxConsoleCommander.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.extCheckBoxConsoleCommander.DisabledScaling = 0.5F;
            this.extCheckBoxConsoleCommander.ImageIndeterminate = null;
            this.extCheckBoxConsoleCommander.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.extCheckBoxConsoleCommander.ImageUnchecked = null;
            this.extCheckBoxConsoleCommander.Location = new System.Drawing.Point(459, 3);
            this.extCheckBoxConsoleCommander.MouseOverScaling = 1.3F;
            this.extCheckBoxConsoleCommander.MouseSelectedScaling = 1.3F;
            this.extCheckBoxConsoleCommander.Name = "extCheckBoxConsoleCommander";
            this.extCheckBoxConsoleCommander.Size = new System.Drawing.Size(123, 17);
            this.extCheckBoxConsoleCommander.TabIndex = 5;
            this.extCheckBoxConsoleCommander.Text = "Console Commander";
            this.extCheckBoxConsoleCommander.TickBoxReductionRatio = 0.75F;
            this.extCheckBoxConsoleCommander.UseVisualStyleBackColor = true;
            // 
            // extCheckBoxIncludeSubfolders
            // 
            this.extCheckBoxIncludeSubfolders.AutoSize = true;
            this.extCheckBoxIncludeSubfolders.ButtonGradientDirection = 90F;
            this.extCheckBoxIncludeSubfolders.CheckBoxColor = System.Drawing.Color.Gray;
            this.extCheckBoxIncludeSubfolders.CheckBoxGradientDirection = 225F;
            this.extCheckBoxIncludeSubfolders.CheckBoxInnerColor = System.Drawing.Color.White;
            this.extCheckBoxIncludeSubfolders.CheckColor = System.Drawing.Color.DarkBlue;
            this.extCheckBoxIncludeSubfolders.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.extCheckBoxIncludeSubfolders.DisabledScaling = 0.5F;
            this.extCheckBoxIncludeSubfolders.ImageIndeterminate = null;
            this.extCheckBoxIncludeSubfolders.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.extCheckBoxIncludeSubfolders.ImageUnchecked = null;
            this.extCheckBoxIncludeSubfolders.Location = new System.Drawing.Point(231, 61);
            this.extCheckBoxIncludeSubfolders.MouseOverScaling = 1.3F;
            this.extCheckBoxIncludeSubfolders.MouseSelectedScaling = 1.3F;
            this.extCheckBoxIncludeSubfolders.Name = "extCheckBoxIncludeSubfolders";
            this.extCheckBoxIncludeSubfolders.Size = new System.Drawing.Size(114, 17);
            this.extCheckBoxIncludeSubfolders.TabIndex = 5;
            this.extCheckBoxIncludeSubfolders.Text = "Include Subfolders";
            this.extCheckBoxIncludeSubfolders.TickBoxReductionRatio = 0.75F;
            this.extCheckBoxIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor2 = System.Drawing.Color.Red;
            this.buttonCancel.ButtonDisabledScaling = 0.5F;
            this.buttonCancel.GradientDirection = 90F;
            this.buttonCancel.Location = new System.Drawing.Point(487, 3);
            this.buttonCancel.MouseOverScaling = 1.3F;
            this.buttonCancel.MouseSelectedScaling = 1.3F;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.BackColor2 = System.Drawing.Color.Red;
            this.buttonOK.ButtonDisabledScaling = 0.5F;
            this.buttonOK.GradientDirection = 90F;
            this.buttonOK.Location = new System.Drawing.Point(608, 3);
            this.buttonOK.MouseOverScaling = 1.3F;
            this.buttonOK.MouseSelectedScaling = 1.3F;
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxCustomInara
            // 
            this.groupBoxCustomInara.AutoSize = true;
            this.groupBoxCustomInara.BorderColor = System.Drawing.Color.LightGray;
            this.groupBoxCustomInara.BorderColor2 = System.Drawing.Color.Gray;
            this.groupBoxCustomInara.ChildrenThemed = true;
            this.groupBoxCustomInara.Controls.Add(this.tableLayoutPanelInara);
            this.groupBoxCustomInara.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCustomInara.GradientDirection = 0F;
            this.groupBoxCustomInara.Location = new System.Drawing.Point(0, 239);
            this.groupBoxCustomInara.Margin = new System.Windows.Forms.Padding(10);
            this.groupBoxCustomInara.Name = "groupBoxCustomInara";
            this.groupBoxCustomInara.Size = new System.Drawing.Size(691, 76);
            this.groupBoxCustomInara.TabIndex = 400;
            this.groupBoxCustomInara.TabStop = false;
            this.groupBoxCustomInara.Text = "Inara Information (optional)";
            this.groupBoxCustomInara.TextPadding = 0;
            this.groupBoxCustomInara.TextStartPosition = -1;
            this.groupBoxCustomInara.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.groupBoxCustomInara.ThemeColorSet = -1;
            // 
            // tableLayoutPanelInara
            // 
            this.tableLayoutPanelInara.AutoSize = true;
            this.tableLayoutPanelInara.ColumnCount = 3;
            this.tableLayoutPanelInara.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelInara.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelInara.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelInara.Controls.Add(this.labelINARAN, 0, 0);
            this.tableLayoutPanelInara.Controls.Add(this.labelInaraAPI, 0, 1);
            this.tableLayoutPanelInara.Controls.Add(this.checkBoxCustomInara, 2, 0);
            this.tableLayoutPanelInara.Controls.Add(this.textBoxBorderInaraAPIKey, 1, 1);
            this.tableLayoutPanelInara.Controls.Add(this.textBoxBorderInaraName, 1, 0);
            this.tableLayoutPanelInara.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelInara.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelInara.Name = "tableLayoutPanelInara";
            this.tableLayoutPanelInara.RowCount = 2;
            this.tableLayoutPanelInara.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.01961F));
            this.tableLayoutPanelInara.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.98039F));
            this.tableLayoutPanelInara.Size = new System.Drawing.Size(685, 57);
            this.tableLayoutPanelInara.TabIndex = 37;
            // 
            // labelINARAN
            // 
            this.labelINARAN.AutoSize = true;
            this.labelINARAN.Location = new System.Drawing.Point(3, 3);
            this.labelINARAN.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelINARAN.Name = "labelINARAN";
            this.labelINARAN.Size = new System.Drawing.Size(65, 13);
            this.labelINARAN.TabIndex = 2;
            this.labelINARAN.Text = "Inara Name";
            // 
            // labelInaraAPI
            // 
            this.labelInaraAPI.AutoSize = true;
            this.labelInaraAPI.Location = new System.Drawing.Point(3, 30);
            this.labelInaraAPI.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelInaraAPI.Name = "labelInaraAPI";
            this.labelInaraAPI.Size = new System.Drawing.Size(75, 13);
            this.labelInaraAPI.TabIndex = 2;
            this.labelInaraAPI.Text = "Inara API Key";
            // 
            // panelOK
            // 
            this.panelOK.AutoSize = true;
            this.panelOK.Controls.Add(this.buttonCancel);
            this.panelOK.Controls.Add(this.buttonOK);
            this.panelOK.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelOK.Location = new System.Drawing.Point(0, 565);
            this.panelOK.Margin = new System.Windows.Forms.Padding(10);
            this.panelOK.Name = "panelOK";
            this.panelOK.Size = new System.Drawing.Size(720, 29);
            this.panelOK.TabIndex = 36;
            // 
            // statusStripCustom
            // 
            this.statusStripCustom.Location = new System.Drawing.Point(0, 594);
            this.statusStripCustom.Name = "statusStripCustom";
            this.statusStripCustom.Size = new System.Drawing.Size(720, 22);
            this.statusStripCustom.TabIndex = 32;
            // 
            // extGroupBoxCommanderInfo
            // 
            this.extGroupBoxCommanderInfo.BorderColor = System.Drawing.Color.LightGray;
            this.extGroupBoxCommanderInfo.BorderColor2 = System.Drawing.Color.Gray;
            this.extGroupBoxCommanderInfo.ChildrenThemed = true;
            this.extGroupBoxCommanderInfo.Controls.Add(this.tableLayoutPanelCommanderInfo);
            this.extGroupBoxCommanderInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.extGroupBoxCommanderInfo.GradientDirection = 0F;
            this.extGroupBoxCommanderInfo.Location = new System.Drawing.Point(0, 368);
            this.extGroupBoxCommanderInfo.Name = "extGroupBoxCommanderInfo";
            this.extGroupBoxCommanderInfo.Size = new System.Drawing.Size(691, 60);
            this.extGroupBoxCommanderInfo.TabIndex = 37;
            this.extGroupBoxCommanderInfo.TabStop = false;
            this.extGroupBoxCommanderInfo.Text = "Other";
            this.extGroupBoxCommanderInfo.TextPadding = 0;
            this.extGroupBoxCommanderInfo.TextStartPosition = -1;
            this.extGroupBoxCommanderInfo.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.extGroupBoxCommanderInfo.ThemeColorSet = -1;
            // 
            // tableLayoutPanelCommanderInfo
            // 
            this.tableLayoutPanelCommanderInfo.ColumnCount = 3;
            this.tableLayoutPanelCommanderInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelCommanderInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelCommanderInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelCommanderInfo.Controls.Add(this.extTextBoxAutoCompleteHomeSystem, 1, 0);
            this.tableLayoutPanelCommanderInfo.Controls.Add(this.HomeSys, 0, 0);
            this.tableLayoutPanelCommanderInfo.Controls.Add(this.labelMapCol, 0, 1);
            this.tableLayoutPanelCommanderInfo.Controls.Add(this.panel_defaultmapcolor, 1, 1);
            this.tableLayoutPanelCommanderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelCommanderInfo.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelCommanderInfo.Name = "tableLayoutPanelCommanderInfo";
            this.tableLayoutPanelCommanderInfo.RowCount = 2;
            this.tableLayoutPanelCommanderInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCommanderInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCommanderInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelCommanderInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelCommanderInfo.Size = new System.Drawing.Size(685, 41);
            this.tableLayoutPanelCommanderInfo.TabIndex = 14;
            // 
            // extTextBoxAutoCompleteHomeSystem
            // 
            this.extTextBoxAutoCompleteHomeSystem.AutoCompleteCommentMarker = null;
            this.extTextBoxAutoCompleteHomeSystem.AutoCompleteTimeout = 500;
            this.extTextBoxAutoCompleteHomeSystem.BackErrorColor = System.Drawing.Color.Red;
            this.extTextBoxAutoCompleteHomeSystem.BorderColor = System.Drawing.Color.Transparent;
            this.extTextBoxAutoCompleteHomeSystem.BorderColor2 = System.Drawing.Color.Transparent;
            this.extTextBoxAutoCompleteHomeSystem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extTextBoxAutoCompleteHomeSystem.ClearOnFirstChar = false;
            this.extTextBoxAutoCompleteHomeSystem.ControlBackground = System.Drawing.SystemColors.Control;
            this.extTextBoxAutoCompleteHomeSystem.EndButtonEnable = false;
            this.extTextBoxAutoCompleteHomeSystem.EndButtonSize16ths = 10;
            this.extTextBoxAutoCompleteHomeSystem.EndButtonVisible = false;
            this.extTextBoxAutoCompleteHomeSystem.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.extTextBoxAutoCompleteHomeSystem.InErrorCondition = false;
            this.extTextBoxAutoCompleteHomeSystem.Location = new System.Drawing.Point(231, 3);
            this.extTextBoxAutoCompleteHomeSystem.Multiline = false;
            this.extTextBoxAutoCompleteHomeSystem.Name = "extTextBoxAutoCompleteHomeSystem";
            this.extTextBoxAutoCompleteHomeSystem.ReadOnly = false;
            this.extTextBoxAutoCompleteHomeSystem.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.extTextBoxAutoCompleteHomeSystem.SelectionLength = 0;
            this.extTextBoxAutoCompleteHomeSystem.SelectionStart = 0;
            this.extTextBoxAutoCompleteHomeSystem.Size = new System.Drawing.Size(152, 14);
            this.extTextBoxAutoCompleteHomeSystem.TabIndex = 0;
            this.extTextBoxAutoCompleteHomeSystem.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.extTextBoxAutoCompleteHomeSystem.TextChangedEvent = "";
            this.extTextBoxAutoCompleteHomeSystem.TextNoChange = "";
            this.extTextBoxAutoCompleteHomeSystem.WordWrap = true;
            // 
            // HomeSys
            // 
            this.HomeSys.AutoSize = true;
            this.HomeSys.Location = new System.Drawing.Point(3, 3);
            this.HomeSys.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.HomeSys.Name = "HomeSys";
            this.HomeSys.Size = new System.Drawing.Size(75, 13);
            this.HomeSys.TabIndex = 2;
            this.HomeSys.Text = "Home System";
            // 
            // labelMapCol
            // 
            this.labelMapCol.AutoSize = true;
            this.labelMapCol.Location = new System.Drawing.Point(3, 23);
            this.labelMapCol.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelMapCol.Name = "labelMapCol";
            this.labelMapCol.Size = new System.Drawing.Size(92, 13);
            this.labelMapCol.TabIndex = 13;
            this.labelMapCol.Text = "Default Map Color";
            // 
            // extPanelScroll
            // 
            this.extPanelScroll.Controls.Add(this.extScrollBar);
            this.extPanelScroll.Controls.Add(this.panelGroups);
            this.extPanelScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extPanelScroll.FlowControlsLeftToRight = false;
            this.extPanelScroll.Location = new System.Drawing.Point(0, 27);
            this.extPanelScroll.Name = "extPanelScroll";
            this.extPanelScroll.ScrollBarWidth = 48;
            this.extPanelScroll.Size = new System.Drawing.Size(720, 538);
            this.extPanelScroll.TabIndex = 40;
            this.extPanelScroll.VerticalScrollBarDockRight = true;
            // 
            // extScrollBar
            // 
            this.extScrollBar.AlwaysHideScrollBar = false;
            this.extScrollBar.ArrowBorderColor = System.Drawing.Color.LightBlue;
            this.extScrollBar.ArrowButtonColor = System.Drawing.Color.LightGray;
            this.extScrollBar.ArrowButtonColor2 = System.Drawing.Color.LightGray;
            this.extScrollBar.ArrowDownDrawAngle = 270F;
            this.extScrollBar.ArrowUpDrawAngle = 90F;
            this.extScrollBar.BorderColor = System.Drawing.Color.White;
            this.extScrollBar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.extScrollBar.HideScrollBar = false;
            this.extScrollBar.LargeChange = 10;
            this.extScrollBar.Location = new System.Drawing.Point(672, 0);
            this.extScrollBar.Maximum = -96;
            this.extScrollBar.Minimum = 0;
            this.extScrollBar.MouseOverButtonColor = System.Drawing.Color.Green;
            this.extScrollBar.MouseOverButtonColor2 = System.Drawing.Color.Green;
            this.extScrollBar.MousePressedButtonColor = System.Drawing.Color.Red;
            this.extScrollBar.MousePressedButtonColor2 = System.Drawing.Color.Red;
            this.extScrollBar.Name = "extScrollBar";
            this.extScrollBar.Size = new System.Drawing.Size(48, 538);
            this.extScrollBar.SkinnyStyle = false;
            this.extScrollBar.SliderColor = System.Drawing.Color.DarkGray;
            this.extScrollBar.SliderColor2 = System.Drawing.Color.DarkGray;
            this.extScrollBar.SliderDrawAngle = 90F;
            this.extScrollBar.SmallChange = 1;
            this.extScrollBar.TabIndex = 0;
            this.extScrollBar.ThumbBorderColor = System.Drawing.Color.Yellow;
            this.extScrollBar.ThumbButtonColor = System.Drawing.Color.DarkBlue;
            this.extScrollBar.ThumbButtonColor2 = System.Drawing.Color.DarkBlue;
            this.extScrollBar.ThumbDrawAngle = 0F;
            this.extScrollBar.Value = -96;
            this.extScrollBar.ValueLimited = -96;
            // 
            // panelGroups
            // 
            this.panelGroups.AutoSize = true;
            this.panelGroups.Controls.Add(this.extGroupBoxCommanderInfo);
            this.panelGroups.Controls.Add(this.extGroupBoxEDAstro);
            this.panelGroups.Controls.Add(this.groupBoxCustomInara);
            this.panelGroups.Controls.Add(this.groupBoxCustomEDSM);
            this.panelGroups.Controls.Add(this.groupBoxCustomEDDN);
            this.panelGroups.Controls.Add(this.groupBoxCustomJournal);
            this.panelGroups.Location = new System.Drawing.Point(0, 0);
            this.panelGroups.Name = "panelGroups";
            this.panelGroups.Size = new System.Drawing.Size(691, 433);
            this.panelGroups.TabIndex = 1;
            // 
            // extGroupBoxEDAstro
            // 
            this.extGroupBoxEDAstro.BorderColor = System.Drawing.Color.LightGray;
            this.extGroupBoxEDAstro.BorderColor2 = System.Drawing.Color.Gray;
            this.extGroupBoxEDAstro.ChildrenThemed = true;
            this.extGroupBoxEDAstro.Controls.Add(this.extCheckBoxEDAstro);
            this.extGroupBoxEDAstro.Dock = System.Windows.Forms.DockStyle.Top;
            this.extGroupBoxEDAstro.GradientDirection = 0F;
            this.extGroupBoxEDAstro.Location = new System.Drawing.Point(0, 315);
            this.extGroupBoxEDAstro.Name = "extGroupBoxEDAstro";
            this.extGroupBoxEDAstro.Size = new System.Drawing.Size(691, 53);
            this.extGroupBoxEDAstro.TabIndex = 39;
            this.extGroupBoxEDAstro.TabStop = false;
            this.extGroupBoxEDAstro.Text = "EDAstro";
            this.extGroupBoxEDAstro.TextPadding = 0;
            this.extGroupBoxEDAstro.TextStartPosition = -1;
            this.extGroupBoxEDAstro.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.extGroupBoxEDAstro.ThemeColorSet = -1;
            // 
            // extCheckBoxEDAstro
            // 
            this.extCheckBoxEDAstro.AutoSize = true;
            this.extCheckBoxEDAstro.ButtonGradientDirection = 90F;
            this.extCheckBoxEDAstro.CheckBoxColor = System.Drawing.Color.Gray;
            this.extCheckBoxEDAstro.CheckBoxGradientDirection = 225F;
            this.extCheckBoxEDAstro.CheckBoxInnerColor = System.Drawing.Color.White;
            this.extCheckBoxEDAstro.CheckColor = System.Drawing.Color.DarkBlue;
            this.extCheckBoxEDAstro.CheckColor2 = System.Drawing.Color.DarkBlue;
            this.extCheckBoxEDAstro.DisabledScaling = 0.5F;
            this.extCheckBoxEDAstro.ImageIndeterminate = null;
            this.extCheckBoxEDAstro.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.extCheckBoxEDAstro.ImageUnchecked = null;
            this.extCheckBoxEDAstro.Location = new System.Drawing.Point(9, 19);
            this.extCheckBoxEDAstro.MouseOverScaling = 1.3F;
            this.extCheckBoxEDAstro.MouseSelectedScaling = 1.3F;
            this.extCheckBoxEDAstro.Name = "extCheckBoxEDAstro";
            this.extCheckBoxEDAstro.Size = new System.Drawing.Size(141, 17);
            this.extCheckBoxEDAstro.TabIndex = 0;
            this.extCheckBoxEDAstro.Text = "Send Events to EDAstro";
            this.extCheckBoxEDAstro.TickBoxReductionRatio = 0.75F;
            this.extCheckBoxEDAstro.UseVisualStyleBackColor = true;
            // 
            // CommanderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 616);
            this.Controls.Add(this.extPanelScroll);
            this.Controls.Add(this.panelOK);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStripCustom);
            this.Name = "CommanderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CommanderForm";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.groupBoxCustomEDDN.ResumeLayout(false);
            this.groupBoxCustomEDDN.PerformLayout();
            this.groupBoxCustomEDSM.ResumeLayout(false);
            this.groupBoxCustomEDSM.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBoxCustomJournal.ResumeLayout(false);
            this.groupBoxCustomJournal.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBoxCustomInara.ResumeLayout(false);
            this.groupBoxCustomInara.PerformLayout();
            this.tableLayoutPanelInara.ResumeLayout(false);
            this.tableLayoutPanelInara.PerformLayout();
            this.panelOK.ResumeLayout(false);
            this.extGroupBoxCommanderInfo.ResumeLayout(false);
            this.tableLayoutPanelCommanderInfo.ResumeLayout(false);
            this.tableLayoutPanelCommanderInfo.PerformLayout();
            this.extPanelScroll.ResumeLayout(false);
            this.extPanelScroll.PerformLayout();
            this.panelGroups.ResumeLayout(false);
            this.panelGroups.PerformLayout();
            this.extGroupBoxEDAstro.ResumeLayout(false);
            this.extGroupBoxEDAstro.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ExtendedControls.ExtButton buttonOK;
        private ExtendedControls.ExtButton buttonCancel;
        private System.Windows.Forms.Label labelCN;
        private System.Windows.Forms.Label labelJL;
        private System.Windows.Forms.Label labelEDSMN;
        private System.Windows.Forms.Label labelEDSMAPI;
        private ExtendedControls.ExtTextBox textBoxBorderJournal;
        private ExtendedControls.ExtTextBox textBoxBorderCmdr;
        private ExtendedControls.ExtTextBox textBoxBorderEDSMAPI;
        private ExtendedControls.ExtTextBox textBoxBorderEDSMName;
        private ExtendedControls.ExtGroupBox groupBoxCustomJournal;
        private ExtendedControls.ExtButton buttonExtBrowse;
        private ExtendedControls.ExtGroupBox groupBoxCustomEDSM;
        private ExtendedControls.ExtGroupBox groupBoxCustomEDDN;
        private ExtendedControls.ExtCheckBox checkBoxCustomEDSMFrom;
        private ExtendedControls.ExtCheckBox checkBoxCustomEDSMTo;
        private ExtendedControls.ExtCheckBox checkBoxCustomEDDNTo;
        private System.Windows.Forms.Panel panelTop;
        private ExtendedControls.ExtButtonDrawn panel_close;
        private ExtendedControls.ExtButtonDrawn panel_minimize;
        private System.Windows.Forms.Label label_index;
        private System.Windows.Forms.ToolTip toolTip;
        private ExtendedControls.ExtGroupBox groupBoxCustomInara;
        private ExtendedControls.ExtCheckBox checkBoxCustomInara;
        private System.Windows.Forms.Label labelINARAN;
        private ExtendedControls.ExtTextBox textBoxBorderInaraName;
        private ExtendedControls.ExtTextBox textBoxBorderInaraAPIKey;
        private System.Windows.Forms.Label labelInaraAPI;
        private System.Windows.Forms.Panel panelOK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelInara;
        private ExtendedControls.ExtStatusStrip statusStripCustom;
        private ExtendedControls.ExtGroupBox extGroupBoxCommanderInfo;
        private ExtendedControls.ExtTextBoxAutoComplete extTextBoxAutoCompleteHomeSystem;
        private System.Windows.Forms.Label HomeSys;
        private System.Windows.Forms.Label labelMapCol;
        private ExtendedControls.ColorPanel panel_defaultmapcolor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCommanderInfo;
        private ExtendedControls.ExtCheckBox extCheckBoxConsoleCommander;
        private ExtendedControls.ExtPanelScroll extPanelScroll;
        private ExtendedControls.ExtScrollBar extScrollBar;
        private ExtendedControls.ExtGroupBox extGroupBoxEDAstro;
        private ExtendedControls.ExtCheckBox extCheckBoxEDAstro;
        private System.Windows.Forms.Panel panelGroups;
        private ExtendedControls.ExtCheckBox extCheckBoxIncludeSubfolders;
    }
}
