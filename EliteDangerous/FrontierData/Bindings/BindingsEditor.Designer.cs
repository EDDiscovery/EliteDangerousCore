namespace EliteDangerousCore
{ 
    partial class BindingsEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BindingsEditor));
            this.dataViewScrollerPanel = new ExtendedControls.ExtPanelDataGridViewScroll();
            this.dataGridView = new BaseUtils.DataGridViewColumnControl();
            this.ColGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColUI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColValues = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColPrimaryDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColPrimaryKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColPrimaryModDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColPrimaryModKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColSecondaryDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColSecondaryKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColSecondaryModDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColSecondaryModKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.vScrollBarCustomMC = new ExtendedControls.ExtScrollBar();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.defineByKeyJoystickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapPrimaryAndSecondaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearPrimaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSecondaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFrontierNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extPanelTop = new ExtendedControls.ExtPanelGradientFill();
            this.extComboBoxFilter = new ExtendedControls.ExtComboBox();
            this.labelWarning = new System.Windows.Forms.Label();
            this.extButtonDeviceRename = new ExtendedControls.ExtButton();
            this.extButtonDeviceRemove = new ExtendedControls.ExtButton();
            this.extButtonDeviceNew = new ExtendedControls.ExtButton();
            this.extButtonEmpty = new ExtendedControls.ExtButton();
            this.extButtonFolder = new ExtendedControls.ExtButton();
            this.extButtonSetDefault = new ExtendedControls.ExtButton();
            this.extButtonDuplicate = new ExtendedControls.ExtButton();
            this.extButtonSave = new ExtendedControls.ExtButton();
            this.extComboBoxBindFiles = new ExtendedControls.ExtComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.dataViewScrollerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.extPanelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataViewScrollerPanel
            // 
            this.dataViewScrollerPanel.Controls.Add(this.dataGridView);
            this.dataViewScrollerPanel.Controls.Add(this.vScrollBarCustomMC);
            this.dataViewScrollerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataViewScrollerPanel.InternalMargin = new System.Windows.Forms.Padding(0);
            this.dataViewScrollerPanel.Location = new System.Drawing.Point(0, 36);
            this.dataViewScrollerPanel.Name = "dataViewScrollerPanel";
            this.dataViewScrollerPanel.ScrollBarWidth = 24;
            this.dataViewScrollerPanel.Size = new System.Drawing.Size(1526, 715);
            this.dataViewScrollerPanel.TabIndex = 1;
            this.dataViewScrollerPanel.VerticalScrollBarDockRight = true;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowRowHeaderVisibleSelection = false;
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView.AutoSortByColumnName = false;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnReorder = true;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColGroup,
            this.ColUI,
            this.ColName,
            this.ColValues,
            this.ColPrimaryDevice,
            this.ColPrimaryKey,
            this.ColPrimaryModDevice,
            this.ColPrimaryModKey,
            this.ColSecondaryDevice,
            this.ColSecondaryKey,
            this.ColSecondaryModDevice,
            this.ColSecondaryModKey});
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.PerColumnWordWrapControl = true;
            this.dataGridView.RowHeaderMenuStrip = null;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView.SingleRowSelect = false;
            this.dataGridView.Size = new System.Drawing.Size(1502, 715);
            this.dataGridView.TabIndex = 1;
            this.dataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_CellBeginEdit);
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellDoubleClick);
            this.dataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellEndEdit);
            this.dataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView_EditingControlShowing);
            this.dataGridView.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dataGridView_SortCompare);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            // 
            // ColGroup
            // 
            this.ColGroup.FillWeight = 80F;
            this.ColGroup.HeaderText = "Group";
            this.ColGroup.Name = "ColGroup";
            this.ColGroup.ReadOnly = true;
            // 
            // ColUI
            // 
            this.ColUI.FillWeight = 80F;
            this.ColUI.HeaderText = "UI";
            this.ColUI.Name = "ColUI";
            this.ColUI.ReadOnly = true;
            // 
            // ColName
            // 
            this.ColName.FillWeight = 150F;
            this.ColName.HeaderText = "Name";
            this.ColName.MinimumWidth = 100;
            this.ColName.Name = "ColName";
            this.ColName.ReadOnly = true;
            // 
            // ColValues
            // 
            this.ColValues.HeaderText = "Values";
            this.ColValues.Name = "ColValues";
            this.ColValues.ReadOnly = true;
            // 
            // ColPrimaryDevice
            // 
            this.ColPrimaryDevice.DisplayStyleForCurrentCellOnly = true;
            this.ColPrimaryDevice.HeaderText = "PrimaryDevice";
            this.ColPrimaryDevice.Name = "ColPrimaryDevice";
            // 
            // ColPrimaryKey
            // 
            this.ColPrimaryKey.DisplayStyleForCurrentCellOnly = true;
            this.ColPrimaryKey.HeaderText = "Key";
            this.ColPrimaryKey.Name = "ColPrimaryKey";
            this.ColPrimaryKey.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColPrimaryKey.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ColPrimaryModDevice
            // 
            this.ColPrimaryModDevice.DisplayStyleForCurrentCellOnly = true;
            this.ColPrimaryModDevice.HeaderText = "Mod Device";
            this.ColPrimaryModDevice.Name = "ColPrimaryModDevice";
            // 
            // ColPrimaryModKey
            // 
            this.ColPrimaryModKey.DisplayStyleForCurrentCellOnly = true;
            this.ColPrimaryModKey.HeaderText = "Key";
            this.ColPrimaryModKey.Name = "ColPrimaryModKey";
            // 
            // ColSecondaryDevice
            // 
            this.ColSecondaryDevice.DisplayStyleForCurrentCellOnly = true;
            this.ColSecondaryDevice.HeaderText = "Secondary Device";
            this.ColSecondaryDevice.Name = "ColSecondaryDevice";
            // 
            // ColSecondaryKey
            // 
            this.ColSecondaryKey.DisplayStyleForCurrentCellOnly = true;
            this.ColSecondaryKey.HeaderText = "Key";
            this.ColSecondaryKey.Name = "ColSecondaryKey";
            // 
            // ColSecondaryModDevice
            // 
            this.ColSecondaryModDevice.DisplayStyleForCurrentCellOnly = true;
            this.ColSecondaryModDevice.HeaderText = "Mod Device";
            this.ColSecondaryModDevice.Name = "ColSecondaryModDevice";
            // 
            // ColSecondaryModKey
            // 
            this.ColSecondaryModKey.DisplayStyleForCurrentCellOnly = true;
            this.ColSecondaryModKey.HeaderText = "Key";
            this.ColSecondaryModKey.Name = "ColSecondaryModKey";
            // 
            // vScrollBarCustomMC
            // 
            this.vScrollBarCustomMC.AlwaysHideScrollBar = false;
            this.vScrollBarCustomMC.ArrowBorderColor = System.Drawing.Color.LightBlue;
            this.vScrollBarCustomMC.ArrowButtonColor = System.Drawing.Color.LightGray;
            this.vScrollBarCustomMC.ArrowButtonColor2 = System.Drawing.Color.LightGray;
            this.vScrollBarCustomMC.ArrowDownDrawAngle = 270F;
            this.vScrollBarCustomMC.ArrowUpDrawAngle = 90F;
            this.vScrollBarCustomMC.BorderColor = System.Drawing.Color.White;
            this.vScrollBarCustomMC.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.vScrollBarCustomMC.HideScrollBar = false;
            this.vScrollBarCustomMC.LargeChange = 0;
            this.vScrollBarCustomMC.Location = new System.Drawing.Point(1502, 0);
            this.vScrollBarCustomMC.Maximum = -1;
            this.vScrollBarCustomMC.Minimum = 0;
            this.vScrollBarCustomMC.MouseOverButtonColor = System.Drawing.Color.Green;
            this.vScrollBarCustomMC.MouseOverButtonColor2 = System.Drawing.Color.Green;
            this.vScrollBarCustomMC.MousePressedButtonColor = System.Drawing.Color.Red;
            this.vScrollBarCustomMC.MousePressedButtonColor2 = System.Drawing.Color.Red;
            this.vScrollBarCustomMC.Name = "vScrollBarCustomMC";
            this.vScrollBarCustomMC.Size = new System.Drawing.Size(24, 715);
            this.vScrollBarCustomMC.SkinnyStyle = ExtendedControls.ExtScrollBar.ScrollStyle.Normal;
            this.vScrollBarCustomMC.SliderColor = System.Drawing.Color.DarkGray;
            this.vScrollBarCustomMC.SliderColor2 = System.Drawing.Color.DarkGray;
            this.vScrollBarCustomMC.SliderDrawAngle = 90F;
            this.vScrollBarCustomMC.SmallChange = 1;
            this.vScrollBarCustomMC.TabIndex = 0;
            this.vScrollBarCustomMC.ThumbBorderColor = System.Drawing.Color.Yellow;
            this.vScrollBarCustomMC.ThumbButtonColor = System.Drawing.Color.DarkBlue;
            this.vScrollBarCustomMC.ThumbButtonColor2 = System.Drawing.Color.DarkBlue;
            this.vScrollBarCustomMC.ThumbDrawAngle = 0F;
            this.vScrollBarCustomMC.Value = -1;
            this.vScrollBarCustomMC.ValueLimited = -1;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defineByKeyJoystickToolStripMenuItem,
            this.swapPrimaryAndSecondaryToolStripMenuItem,
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem,
            this.clearPrimaryToolStripMenuItem,
            this.clearSecondaryToolStripMenuItem,
            this.clearAllToolStripMenuItem,
            this.showFrontierNamesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(244, 158);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // defineByKeyJoystickToolStripMenuItem
            // 
            this.defineByKeyJoystickToolStripMenuItem.Name = "defineByKeyJoystickToolStripMenuItem";
            this.defineByKeyJoystickToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.defineByKeyJoystickToolStripMenuItem.Text = "Define by Device Input";
            this.defineByKeyJoystickToolStripMenuItem.Click += new System.EventHandler(this.defineByKeyJoystickToolStripMenuItem_Click);
            // 
            // swapPrimaryAndSecondaryToolStripMenuItem
            // 
            this.swapPrimaryAndSecondaryToolStripMenuItem.Name = "swapPrimaryAndSecondaryToolStripMenuItem";
            this.swapPrimaryAndSecondaryToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.swapPrimaryAndSecondaryToolStripMenuItem.Text = "Swap primary and secondary";
            this.swapPrimaryAndSecondaryToolStripMenuItem.Click += new System.EventHandler(this.swapPrimaryAndSecondaryToolStripMenuItem_Click);
            // 
            // moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem
            // 
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Name = "moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem";
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Text = "Move joystick entries to primary";
            this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem.Click += new System.EventHandler(this.moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem_Click);
            // 
            // clearPrimaryToolStripMenuItem
            // 
            this.clearPrimaryToolStripMenuItem.Name = "clearPrimaryToolStripMenuItem";
            this.clearPrimaryToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.clearPrimaryToolStripMenuItem.Text = "Clear Primary";
            this.clearPrimaryToolStripMenuItem.Click += new System.EventHandler(this.clearPrimaryToolStripMenuItem_Click);
            // 
            // clearSecondaryToolStripMenuItem
            // 
            this.clearSecondaryToolStripMenuItem.Name = "clearSecondaryToolStripMenuItem";
            this.clearSecondaryToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.clearSecondaryToolStripMenuItem.Text = "Clear secondary";
            this.clearSecondaryToolStripMenuItem.Click += new System.EventHandler(this.clearSecondaryToolStripMenuItem_Click);
            // 
            // clearAllToolStripMenuItem
            // 
            this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.clearAllToolStripMenuItem.Text = "Clear All";
            this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
            // 
            // showFrontierNamesToolStripMenuItem
            // 
            this.showFrontierNamesToolStripMenuItem.Checked = true;
            this.showFrontierNamesToolStripMenuItem.CheckOnClick = true;
            this.showFrontierNamesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showFrontierNamesToolStripMenuItem.Name = "showFrontierNamesToolStripMenuItem";
            this.showFrontierNamesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.showFrontierNamesToolStripMenuItem.Text = "Show Frontier Names";
            // 
            // extPanelTop
            // 
            this.extPanelTop.ChildrenThemed = true;
            this.extPanelTop.Controls.Add(this.extComboBoxFilter);
            this.extPanelTop.Controls.Add(this.labelWarning);
            this.extPanelTop.Controls.Add(this.extButtonDeviceRename);
            this.extPanelTop.Controls.Add(this.extButtonDeviceRemove);
            this.extPanelTop.Controls.Add(this.extButtonDeviceNew);
            this.extPanelTop.Controls.Add(this.extButtonEmpty);
            this.extPanelTop.Controls.Add(this.extButtonFolder);
            this.extPanelTop.Controls.Add(this.extButtonSetDefault);
            this.extPanelTop.Controls.Add(this.extButtonDuplicate);
            this.extPanelTop.Controls.Add(this.extButtonSave);
            this.extPanelTop.Controls.Add(this.extComboBoxBindFiles);
            this.extPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.extPanelTop.FlowDirection = null;
            this.extPanelTop.GradientDirection = 0F;
            this.extPanelTop.Location = new System.Drawing.Point(0, 0);
            this.extPanelTop.Name = "extPanelTop";
            this.extPanelTop.PaintTransparentColor = System.Drawing.Color.Transparent;
            this.extPanelTop.Size = new System.Drawing.Size(1526, 36);
            this.extPanelTop.TabIndex = 2;
            this.extPanelTop.ThemeColors = new System.Drawing.Color[] {
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.extPanelTop.ThemeColorSet = -1;
            this.extPanelTop.ThisThemed = true;
            // 
            // extComboBoxFilter
            // 
            this.extComboBoxFilter.BackColor2 = System.Drawing.Color.Red;
            this.extComboBoxFilter.BorderColor = System.Drawing.Color.White;
            this.extComboBoxFilter.ControlBackground = System.Drawing.SystemColors.Control;
            this.extComboBoxFilter.DataSource = null;
            this.extComboBoxFilter.DisableBackgroundDisabledShadingGradient = false;
            this.extComboBoxFilter.DisabledScaling = 0.5F;
            this.extComboBoxFilter.DisplayMember = "";
            this.extComboBoxFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.extComboBoxFilter.GradientDirection = 90F;
            this.extComboBoxFilter.Location = new System.Drawing.Point(259, 2);
            this.extComboBoxFilter.MouseOverScalingColor = 1.3F;
            this.extComboBoxFilter.Name = "extComboBoxFilter";
            this.extComboBoxFilter.SelectedIndex = -1;
            this.extComboBoxFilter.SelectedItem = null;
            this.extComboBoxFilter.SelectedValue = null;
            this.extComboBoxFilter.Size = new System.Drawing.Size(112, 21);
            this.extComboBoxFilter.TabIndex = 4;
            this.extComboBoxFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.extComboBoxFilter.ValueMember = "";
            // 
            // labelWarning
            // 
            this.labelWarning.AutoSize = true;
            this.labelWarning.Location = new System.Drawing.Point(711, 9);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(43, 13);
            this.labelWarning.TabIndex = 3;
            this.labelWarning.Text = "<code>";
            // 
            // extButtonDeviceRename
            // 
            this.extButtonDeviceRename.BackColor2 = System.Drawing.Color.Red;
            this.extButtonDeviceRename.ButtonDisabledScaling = 0.5F;
            this.extButtonDeviceRename.GradientDirection = 90F;
            this.extButtonDeviceRename.Location = new System.Drawing.Point(931, 3);
            this.extButtonDeviceRename.MouseOverScaling = 1.3F;
            this.extButtonDeviceRename.MouseSelectedScaling = 1.3F;
            this.extButtonDeviceRename.Name = "extButtonDeviceRename";
            this.extButtonDeviceRename.Size = new System.Drawing.Size(75, 23);
            this.extButtonDeviceRename.TabIndex = 2;
            this.extButtonDeviceRename.Text = "Device <>";
            this.toolTip.SetToolTip(this.extButtonDeviceRename, "Rename a device");
            this.extButtonDeviceRename.UseVisualStyleBackColor = true;
            this.extButtonDeviceRename.Click += new System.EventHandler(this.buttonDeviceRename_Click);
            // 
            // extButtonDeviceRemove
            // 
            this.extButtonDeviceRemove.BackColor2 = System.Drawing.Color.Red;
            this.extButtonDeviceRemove.ButtonDisabledScaling = 0.5F;
            this.extButtonDeviceRemove.GradientDirection = 90F;
            this.extButtonDeviceRemove.Location = new System.Drawing.Point(850, 3);
            this.extButtonDeviceRemove.MouseOverScaling = 1.3F;
            this.extButtonDeviceRemove.MouseSelectedScaling = 1.3F;
            this.extButtonDeviceRemove.Name = "extButtonDeviceRemove";
            this.extButtonDeviceRemove.Size = new System.Drawing.Size(75, 23);
            this.extButtonDeviceRemove.TabIndex = 2;
            this.extButtonDeviceRemove.Text = "Device -";
            this.toolTip.SetToolTip(this.extButtonDeviceRemove, "Remove a device and all assignments");
            this.extButtonDeviceRemove.UseVisualStyleBackColor = true;
            this.extButtonDeviceRemove.Click += new System.EventHandler(this.buttonRemoveDevice_Click);
            // 
            // extButtonDeviceNew
            // 
            this.extButtonDeviceNew.BackColor2 = System.Drawing.Color.Red;
            this.extButtonDeviceNew.ButtonDisabledScaling = 0.5F;
            this.extButtonDeviceNew.GradientDirection = 90F;
            this.extButtonDeviceNew.Location = new System.Drawing.Point(769, 3);
            this.extButtonDeviceNew.MouseOverScaling = 1.3F;
            this.extButtonDeviceNew.MouseSelectedScaling = 1.3F;
            this.extButtonDeviceNew.Name = "extButtonDeviceNew";
            this.extButtonDeviceNew.Size = new System.Drawing.Size(75, 23);
            this.extButtonDeviceNew.TabIndex = 2;
            this.extButtonDeviceNew.Text = "Device +";
            this.toolTip.SetToolTip(this.extButtonDeviceNew, resources.GetString("extButtonDeviceNew.ToolTip"));
            this.extButtonDeviceNew.UseVisualStyleBackColor = true;
            this.extButtonDeviceNew.Click += new System.EventHandler(this.buttonNewDevice_Click);
            // 
            // extButtonEmpty
            // 
            this.extButtonEmpty.BackColor2 = System.Drawing.Color.Red;
            this.extButtonEmpty.ButtonDisabledScaling = 0.5F;
            this.extButtonEmpty.GradientDirection = 90F;
            this.extButtonEmpty.Location = new System.Drawing.Point(1022, 3);
            this.extButtonEmpty.MouseOverScaling = 1.3F;
            this.extButtonEmpty.MouseSelectedScaling = 1.3F;
            this.extButtonEmpty.Name = "extButtonEmpty";
            this.extButtonEmpty.Size = new System.Drawing.Size(75, 23);
            this.extButtonEmpty.TabIndex = 2;
            this.extButtonEmpty.Text = "Clear";
            this.toolTip.SetToolTip(this.extButtonEmpty, "Remove all assignments");
            this.extButtonEmpty.UseVisualStyleBackColor = true;
            this.extButtonEmpty.Click += new System.EventHandler(this.extButtonEmpty_Click);
            // 
            // extButtonFolder
            // 
            this.extButtonFolder.BackColor2 = System.Drawing.Color.Red;
            this.extButtonFolder.ButtonDisabledScaling = 0.5F;
            this.extButtonFolder.GradientDirection = 90F;
            this.extButtonFolder.Location = new System.Drawing.Point(620, 3);
            this.extButtonFolder.MouseOverScaling = 1.3F;
            this.extButtonFolder.MouseSelectedScaling = 1.3F;
            this.extButtonFolder.Name = "extButtonFolder";
            this.extButtonFolder.Size = new System.Drawing.Size(75, 23);
            this.extButtonFolder.TabIndex = 2;
            this.extButtonFolder.Text = "Show Folder";
            this.toolTip.SetToolTip(this.extButtonFolder, "Show bindings folder in explorer");
            this.extButtonFolder.UseVisualStyleBackColor = true;
            this.extButtonFolder.Click += new System.EventHandler(this.extButtonFolder_Click);
            // 
            // extButtonSetDefault
            // 
            this.extButtonSetDefault.BackColor2 = System.Drawing.Color.Red;
            this.extButtonSetDefault.ButtonDisabledScaling = 0.5F;
            this.extButtonSetDefault.GradientDirection = 90F;
            this.extButtonSetDefault.Location = new System.Drawing.Point(539, 3);
            this.extButtonSetDefault.MouseOverScaling = 1.3F;
            this.extButtonSetDefault.MouseSelectedScaling = 1.3F;
            this.extButtonSetDefault.Name = "extButtonSetDefault";
            this.extButtonSetDefault.Size = new System.Drawing.Size(75, 23);
            this.extButtonSetDefault.TabIndex = 2;
            this.extButtonSetDefault.Text = "Set Default";
            this.toolTip.SetToolTip(this.extButtonSetDefault, "Tell Elite to use this bindings file");
            this.extButtonSetDefault.UseVisualStyleBackColor = true;
            this.extButtonSetDefault.Click += new System.EventHandler(this.extButtonSetDefault_Click);
            // 
            // extButtonDuplicate
            // 
            this.extButtonDuplicate.BackColor2 = System.Drawing.Color.Red;
            this.extButtonDuplicate.ButtonDisabledScaling = 0.5F;
            this.extButtonDuplicate.GradientDirection = 90F;
            this.extButtonDuplicate.Location = new System.Drawing.Point(458, 3);
            this.extButtonDuplicate.MouseOverScaling = 1.3F;
            this.extButtonDuplicate.MouseSelectedScaling = 1.3F;
            this.extButtonDuplicate.Name = "extButtonDuplicate";
            this.extButtonDuplicate.Size = new System.Drawing.Size(75, 23);
            this.extButtonDuplicate.TabIndex = 2;
            this.extButtonDuplicate.Text = "Duplicate";
            this.toolTip.SetToolTip(this.extButtonDuplicate, "Make a copy of the file with a new name. Use Save to commit");
            this.extButtonDuplicate.UseVisualStyleBackColor = true;
            this.extButtonDuplicate.Click += new System.EventHandler(this.extButtonDuplicate_Click);
            // 
            // extButtonSave
            // 
            this.extButtonSave.BackColor2 = System.Drawing.Color.Red;
            this.extButtonSave.ButtonDisabledScaling = 0.5F;
            this.extButtonSave.GradientDirection = 90F;
            this.extButtonSave.Location = new System.Drawing.Point(377, 3);
            this.extButtonSave.MouseOverScaling = 1.3F;
            this.extButtonSave.MouseSelectedScaling = 1.3F;
            this.extButtonSave.Name = "extButtonSave";
            this.extButtonSave.Size = new System.Drawing.Size(75, 23);
            this.extButtonSave.TabIndex = 2;
            this.extButtonSave.Text = "Save";
            this.toolTip.SetToolTip(this.extButtonSave, "Save changes to file. Create a backup file per save");
            this.extButtonSave.UseVisualStyleBackColor = true;
            this.extButtonSave.Click += new System.EventHandler(this.extButtonSave_Click);
            // 
            // extComboBoxBindFiles
            // 
            this.extComboBoxBindFiles.BackColor2 = System.Drawing.Color.Red;
            this.extComboBoxBindFiles.BorderColor = System.Drawing.Color.White;
            this.extComboBoxBindFiles.ControlBackground = System.Drawing.SystemColors.Control;
            this.extComboBoxBindFiles.DataSource = null;
            this.extComboBoxBindFiles.DisableBackgroundDisabledShadingGradient = false;
            this.extComboBoxBindFiles.DisabledScaling = 0.5F;
            this.extComboBoxBindFiles.DisplayMember = "";
            this.extComboBoxBindFiles.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.extComboBoxBindFiles.GradientDirection = 90F;
            this.extComboBoxBindFiles.Location = new System.Drawing.Point(4, 2);
            this.extComboBoxBindFiles.MouseOverScalingColor = 1.3F;
            this.extComboBoxBindFiles.Name = "extComboBoxBindFiles";
            this.extComboBoxBindFiles.SelectedIndex = -1;
            this.extComboBoxBindFiles.SelectedItem = null;
            this.extComboBoxBindFiles.SelectedValue = null;
            this.extComboBoxBindFiles.Size = new System.Drawing.Size(233, 21);
            this.extComboBoxBindFiles.TabIndex = 1;
            this.extComboBoxBindFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.extComboBoxBindFiles.ValueMember = "";
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // BindingsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataViewScrollerPanel);
            this.Controls.Add(this.extPanelTop);
            this.Name = "BindingsEditor";
            this.Size = new System.Drawing.Size(1526, 751);
            this.dataViewScrollerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.extPanelTop.ResumeLayout(false);
            this.extPanelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ExtendedControls.ExtPanelDataGridViewScroll dataViewScrollerPanel;
        private BaseUtils.DataGridViewColumnControl dataGridView;
        private ExtendedControls.ExtScrollBar vScrollBarCustomMC;
        private ExtendedControls.ExtPanelGradientFill extPanelTop;
        private ExtendedControls.ExtComboBox extComboBoxBindFiles;
        private ExtendedControls.ExtButton extButtonDuplicate;
        private ExtendedControls.ExtButton extButtonSave;
        private ExtendedControls.ExtButton extButtonEmpty;
        private ExtendedControls.ExtButton extButtonSetDefault;
        private ExtendedControls.ExtButton extButtonDeviceRename;
        private ExtendedControls.ExtButton extButtonDeviceRemove;
        private ExtendedControls.ExtButton extButtonDeviceNew;
        private ExtendedControls.ExtButton extButtonFolder;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColUI;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColValues;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColPrimaryDevice;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColPrimaryKey;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColPrimaryModDevice;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColPrimaryModKey;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColSecondaryDevice;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColSecondaryKey;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColSecondaryModDevice;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColSecondaryModKey;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem swapPrimaryAndSecondaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveJoystickEntriesBeforeKeysAndMouseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSecondaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFrontierNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearPrimaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defineByKeyJoystickToolStripMenuItem;
        private System.Windows.Forms.Label labelWarning;
        private ExtendedControls.ExtComboBox extComboBoxFilter;
    }
}
