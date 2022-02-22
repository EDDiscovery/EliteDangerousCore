/*
 * Copyright © 2020 EDDiscovery development team
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EliteDangerousCore.ScreenShots
{
    public partial class ScreenShotConfigureForm : ExtendedControls.DraggableForm
    {
        public bool AutoConvert { get { return extCheckBoxEnabled.Checked; } }
        public string InputFolder { get { return textBoxScreenshotsDir.Text; } }
        public string OutputFolder { get { return textBoxOutputDir.Text; } }
        public ScreenShotImageConverter.OriginalImageOptions OriginalImageOption { get { return (ScreenShotImageConverter.OriginalImageOptions)extComboBoxOriginalImageSel.SelectedIndex; } }
        public string OriginalImageDirectory { get; set; }
        public ScreenShotConverter.InputTypes InputFileExtension { get { return (ScreenShotConverter.InputTypes)comboBoxScanFor.SelectedIndex; } }
        public ScreenShotImageConverter.OutputTypes OutputFileExtension { get { return (ScreenShotImageConverter.OutputTypes)comboBoxOutputAs.SelectedIndex; } }
        public int FileNameFormat { get { return comboBoxFileNameFormat.SelectedIndex; } }
        public int FolderNameFormat { get { return comboBoxSubFolder.SelectedIndex; } }
        public bool KeepMasterConvertedImage { get { return extCheckBoxKeepMasterConvertedImage.Checked; } }
        public ScreenShotImageConverter.CropResizeOptions CropResizeImage1 { get { return (ScreenShotImageConverter.CropResizeOptions)extComboBoxConvert1.SelectedIndex; } }
        public Rectangle CropResizeArea1 { get { return new Rectangle(numericUpDownLeft1.Value, numericUpDownTop1.Value, numericUpDownWidth1.Value, numericUpDownHeight1.Value); } }
        public ScreenShotImageConverter.CropResizeOptions CropResizeImage2 { get { return (ScreenShotImageConverter.CropResizeOptions)extComboBoxConvert2.SelectedIndex; } }
        public Rectangle CropResizeArea2 { get { return new Rectangle(extNumericUpDownLeft2.Value, extNumericUpDownTop2.Value, extNumericUpDownWidth2.Value, extNumericUpDownHeight2.Value); } }
        public bool HighRes {  get { return extCheckBoxHiRes.Checked; } }
        public ScreenShotImageConverter.ClipboardOptions ClipboardOption { get { return (ScreenShotImageConverter.ClipboardOptions)extComboBoxClipboard.SelectedIndex; } }
        public int Quality { get { return numericUpDownQuality.Value; } }

        string initialssfolder;

        public ScreenShotConfigureForm()
        {
            InitializeComponent();
            var theme = ExtendedControls.Theme.Current;
            bool winborder = theme.ApplyStd(this);
            panelTop.Visible = panelTop.Enabled = !winborder;
        }

        public void Init(ScreenShotImageConverter cf , bool enabled, string inputfolder, ScreenShotConverter.InputTypes it, string outputfolder)
        {
            comboBoxFileNameFormat.Items.AddRange(ScreenShotImageConverter.FileNameFormats);
            comboBoxFileNameFormat.SelectedIndex = cf.FileNameFormat;
            extCheckBoxEnabled.Checked = enabled;
            comboBoxOutputAs.Items.AddRange(Enum.GetNames(typeof(ScreenShotImageConverter.OutputTypes)));
            comboBoxOutputAs.SelectedIndex = (int)cf.OutputFileExtension;
            comboBoxScanFor.Items.AddRange(Enum.GetNames(typeof(ScreenShotConverter.InputTypes)));
            comboBoxScanFor.SelectedIndex = (int)it;
            initialssfolder = textBoxScreenshotsDir.Text = inputfolder;
            textBoxOutputDir.Text = outputfolder;
            comboBoxSubFolder.Items.AddRange(ScreenShotImageConverter.SubFolderSelections);
            comboBoxSubFolder.SelectedIndex = cf.FolderNameFormat;
            string[] opt = new string[] { "Off", "Crop", "Resize" };
            extComboBoxConvert1.Items.AddRange(opt);
            extComboBoxConvert2.Items.AddRange(opt);

            extCheckBoxHiRes.Checked = cf.HighRes;

            extComboBoxClipboard.Items.Add(ScreenShotImageConverter.ClipboardOptions.NoCopy.ToString().SplitCapsWordFull());
            extComboBoxClipboard.Items.Add(ScreenShotImageConverter.ClipboardOptions.CopyMaster.ToString().SplitCapsWordFull());
            extComboBoxClipboard.Items.Add(ScreenShotImageConverter.ClipboardOptions.CopyImage1.ToString().SplitCapsWordFull());
            extComboBoxClipboard.Items.Add(ScreenShotImageConverter.ClipboardOptions.CopyImage2.ToString().SplitCapsWordFull());
            extComboBoxClipboard.SelectedIndex = (int)cf.ClipboardOption;

            extComboBoxConvert1.SelectedIndex = (int)cf.CropResizeImage1;
            extComboBoxConvert2.SelectedIndex = (int)cf.CropResizeImage2;

            OriginalImageDirectory = cf.OriginalImageOptionDirectory;

            extComboBoxOriginalImageSel.Items.Add(ScreenShotImageConverter.OriginalImageOptions.Leave.ToString());
            extComboBoxOriginalImageSel.Items.Add(ScreenShotImageConverter.OriginalImageOptions.Delete.ToString());
            extComboBoxOriginalImageSel.Items.Add(ScreenShotImageConverter.OriginalImageOptions.Move.ToString() + "->" + OriginalImageDirectory);
            extComboBoxOriginalImageSel.SelectedIndex = (int)cf.OriginalImageOption;

            extCheckBoxKeepMasterConvertedImage.Checked = cf.KeepMasterConvertedImage;

            numericUpDownTop1.Value = cf.CropResizeArea1.Top;
            numericUpDownLeft1.Value = cf.CropResizeArea1.Left;
            numericUpDownWidth1.Value = cf.CropResizeArea1.Width;
            numericUpDownHeight1.Value = cf.CropResizeArea1.Height;

            extNumericUpDownTop2.Value = cf.CropResizeArea2.Top;
            extNumericUpDownLeft2.Value = cf.CropResizeArea2.Left;
            extNumericUpDownWidth2.Value = cf.CropResizeArea2.Width;
            extNumericUpDownHeight2.Value = cf.CropResizeArea2.Height;

            numericUpDownQuality.Value = cf.Quality;
            numericUpDownQuality.Visible = labelQuality.Visible = comboBoxOutputAs.SelectedIndex == (int)ScreenShotImageConverter.OutputTypes.jpg;

            SetNumEnabled();

            extComboBoxConvert1.SelectedIndexChanged += (s, e) => { SetNumEnabled(); };
            extComboBoxConvert2.SelectedIndexChanged += (s, e) => { SetNumEnabled(); };

            UpdateExample();

            BaseUtils.Translator.Instance.TranslateVerify(this, typeof(EDCTx));

            label_index.Text = this.Text;
        }

        private void ScreenShotConfigureForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void SetNumEnabled()
        {
            numericUpDownTop1.Enabled = numericUpDownLeft1.Enabled = extComboBoxConvert1.SelectedIndex == 1;
            numericUpDownWidth1.Enabled = numericUpDownHeight1.Enabled = extComboBoxConvert1.SelectedIndex > 0;
            extNumericUpDownTop2.Enabled = extNumericUpDownLeft2.Enabled = extComboBoxConvert2.SelectedIndex == 1;
            extNumericUpDownWidth2.Enabled = extNumericUpDownHeight2.Enabled = extComboBoxConvert2.SelectedIndex > 0;
        }

        private void panel_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void panel_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void captionControl_MouseDown(object sender, MouseEventArgs e)
        {
            OnCaptionMouseDown((Control)sender, e);
        }

        private void captionControl_MouseUp(object sender, MouseEventArgs e)
        {
            OnCaptionMouseUp((Control)sender, e);
        }

        private void buttonChangeEDScreenshot_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select screenshot folder".T(EDCTx.Screenshot_Folder);
                dlg.SelectedPath = textBoxScreenshotsDir.Text;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    initialssfolder = textBoxScreenshotsDir.Text = dlg.SelectedPath;
                }
            }
        }

        private void buttonChangeOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.Description = "Select screenshot folder".T(EDCTx.Screenshot_Folder);
            dlg.SelectedPath = textBoxOutputDir.Text;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                textBoxOutputDir.Text = dlg.SelectedPath;
            }
        }

        private void textBoxScreenshotsDir_Leave(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBoxScreenshotsDir.Text))
            {
                ExtendedControls.MessageBoxTheme.Show(this, "Folder specified does not exist".T(EDCTx.Screenshot_FolderNotExist));
                textBoxScreenshotsDir.Text = initialssfolder;
            }
            else
                initialssfolder = textBoxScreenshotsDir.Text;
        }

        private void textBoxScreenshotsDir_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBoxScreenshotsDir_Leave(sender, e);
            }
        }

        private void buttonExtOK_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxScreenshotsDir.Text) && Directory.Exists(textBoxOutputDir.Text))        // if both  input and output folders exist
            {
                try
                {
                    bool diffdir = !Path.GetFullPath(textBoxScreenshotsDir.Text).Equals(Path.GetFullPath(textBoxOutputDir.Text), StringComparison.InvariantCultureIgnoreCase);
                    bool diffext = !comboBoxScanFor.SelectedItem.ToString().Equals(comboBoxOutputAs.SelectedItem.ToString(), StringComparison.InvariantCultureIgnoreCase);

                    if (diffdir || diffext)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                        ExtendedControls.MessageBoxTheme.Show(this, "Cannot set input and output to identical types and the same folder".T(EDCTx.Screenshot_Identical));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in OK" + ex);
                    Close();
                }
            }
            else
                ExtendedControls.MessageBoxTheme.Show(this, "Folder specified does not exist".T(EDCTx.Screenshot_FolderNotExist));
        }

        private void buttonExtCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboBoxFileNameFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateExample();
        }

        private void extCheckBoxHiRes_CheckedChanged(object sender, EventArgs e)
        {
            UpdateExample();
        }

        private void comboBoxSubFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateExample();
        }

        private void comboBoxOutputAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDownQuality.Visible = labelQuality.Visible = comboBoxOutputAs.SelectedIndex == (int)ScreenShotImageConverter.OutputTypes.jpg;
            UpdateExample();
        }

        private void textBoxOutputDir_TextChanged(object sender, EventArgs e)
        {
            UpdateExample();
        }

        private void UpdateExample()
        {
            textBoxFileNameExample.Text = Path.Combine(ScreenShotImageConverter.SubFolder(comboBoxSubFolder.SelectedIndex, OutputFolder, "Sol","Jameson",DateTime.Now),
                                          ScreenShotImageConverter.CreateFileName("Sol", "Earth", "HighResScreenshot_0000.bmp", comboBoxFileNameFormat.SelectedIndex, extCheckBoxHiRes.Checked, DateTime.Now) + "." + comboBoxOutputAs.Text);
        }

        private void extButtonBrowseMoveOrg_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.Description = "Select screenshot folder".T(EDCTx.Screenshot_Folder);
            dlg.SelectedPath = OriginalImageDirectory;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                OriginalImageDirectory = dlg.SelectedPath;
                extComboBoxOriginalImageSel.Items[2] = ScreenShotImageConverter.OriginalImageOptions.Move.ToString() + "->" + OriginalImageDirectory;
                extComboBoxOriginalImageSel.SelectedIndex = 0;  // force change
                extComboBoxOriginalImageSel.SelectedIndex = 2;
            }

        }

    }
}
