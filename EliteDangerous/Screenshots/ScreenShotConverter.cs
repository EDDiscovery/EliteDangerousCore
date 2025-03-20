/*
 * Copyright 2016-2024 EDDiscovery development team
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EliteDangerousCore.ScreenShots
{
    public class ScreenShotConverter
    {
        public string InputFolder { get; set; }
        public enum InputTypes { bmp, jpg, png };
        public InputTypes InputFileExtension { get; set; } = InputTypes.bmp;
        public string OutputFolder { get; set; }
        public bool AutoConvert { get; set; }

        // called on screenshot. inputfilename location (may be null if deleted), outputfilename location, image size, screenshot (may be null)
        public event Action<string, string, Size, JournalEvents.JournalScreenshot> OnScreenshot;     

        public ScreenShotImageConverter converter;
        private ScreenshotDirectoryWatcher watcher;

        private Action<Action> invokeOnUiThread;

        public ScreenShotConverter()
        {
            converter = new ScreenShotImageConverter();

            string screenshotsDirdefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
            string outputDirdefault = Path.Combine(screenshotsDirdefault, "Converted");

            InputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerScreenshotsDir", screenshotsDirdefault );
            if (!Directory.Exists(InputFolder))
                InputFolder = screenshotsDirdefault;

            OutputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerOutputDir", outputDirdefault);
            if (!Directory.Exists(OutputFolder))
                OutputFolder = outputDirdefault;

            AutoConvert = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerAutoconvert", false);


            try
            {       // just in case
                if (EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("checkBoxRemove", false))    // backward compatible
                {
                    converter.OriginalImageOption = ScreenShotImageConverter.OriginalImageOptions.Delete;
                }
                else
                    converter.OriginalImageOption = (ScreenShotImageConverter.OriginalImageOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerOriginalImageOption", 0);

                EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("checkBoxRemove");

                converter.OriginalImageOptionDirectory = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerOriginalDirectory", @"c:\");

                if (EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerClipboard", false))    // backward compatible
                {
                    converter.ClipboardOption = ScreenShotImageConverter.ClipboardOptions.CopyMaster;
                }
                else
                    converter.ClipboardOption = (ScreenShotImageConverter.ClipboardOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerClipboardOption", 0);

                EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("ImageHandlerClipboard");

                converter.HighRes = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("checkBoxHires", false);
                if (EliteDangerousCore.DB.UserDatabase.Instance.KeyExists("ImageHandlerCropImage"))
                {
                    converter.CropResizeImage1 = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropImage", false) ? ScreenShotImageConverter.CropResizeOptions.Crop : ScreenShotImageConverter.CropResizeOptions.Off;
                    EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("ImageHandlerCropImage");
                }
                else
                    converter.CropResizeImage1 = (ScreenShotImageConverter.CropResizeOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropResizeImage1", 0);

                converter.CropResizeImage2 = (ScreenShotImageConverter.CropResizeOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropResizeImage2", 0);
                converter.CropResizeArea1 = new Rectangle(EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropLeft", 0), EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropTop", 0),
                                        EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropWidth", 1920), EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropHeight", 1024));
                converter.CropResizeArea2 = new Rectangle(EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropLeft2", 0), EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropTop2", 0),
                                        EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropWidth2", 1920), EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerCropHeight2", 1024));

                InputFileExtension = (InputTypes)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("comboBoxScanFor", 0);
                converter.OutputFileExtension = (ScreenShotImageConverter.OutputTypes)EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerFormatNr", 0);

                converter.KeepMasterConvertedImage = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerKeepFullSized", false);
                converter.Quality = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("ImageHandlerQuality", 85);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.ToString());
            }

            converter.FileNameFormat = Math.Min(Math.Max(0, EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("comboBoxFileNameFormat", 0)), ScreenShotImageConverter.FileNameFormats.Length - 1);
            converter.FolderNameFormat = Math.Min(Math.Max(0, EliteDangerousCore.DB.UserDatabase.Instance.GetSetting("comboBoxSubFolder", 0)), ScreenShotImageConverter.SubFolderSelections.Length - 1);
        }

        public void SaveSettings()
        {
            DB.UserDatabase.Instance.PutSetting("ImageHandlerOutputDir", OutputFolder);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerScreenshotsDir", InputFolder);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerAutoconvert", AutoConvert );      // names are all over the place.. historical
            DB.UserDatabase.Instance.PutSetting("ImageHandlerOriginalImageOption", (int)converter.OriginalImageOption);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSetting("ImageHandlerOriginalDirectory", converter.OriginalImageOptionDirectory);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerClipboardOption", (int)converter.ClipboardOption);
            DB.UserDatabase.Instance.PutSetting("checkBoxHires", converter.HighRes );
            DB.UserDatabase.Instance.PutSetting("ImageHandlerKeepFullSized", converter.KeepMasterConvertedImage);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerQuality", converter.Quality);

            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropResizeImage1", (int)converter.CropResizeImage1);      // fires the checked handler which sets the readonly mode of the controls
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropTop", converter.CropResizeArea1.Top);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropLeft", converter.CropResizeArea1.Left);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropWidth", converter.CropResizeArea1.Width);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropHeight", converter.CropResizeArea1.Height);

            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropResizeImage2", (int)converter.CropResizeImage2);      // fires the checked handler which sets the readonly mode of the controls
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropTop2", converter.CropResizeArea2.Top);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropLeft2", converter.CropResizeArea2.Left);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropWidth2", converter.CropResizeArea2.Width);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerCropHeight2", converter.CropResizeArea2.Height);

            DB.UserDatabase.Instance.PutSetting("comboBoxScanFor", (int)InputFileExtension);
            DB.UserDatabase.Instance.PutSetting("ImageHandlerFormatNr", (int)converter.OutputFileExtension);
            DB.UserDatabase.Instance.PutSetting("comboBoxFileNameFormat", converter.FileNameFormat);
            DB.UserDatabase.Instance.PutSetting("comboBoxSubFolder", converter.FolderNameFormat);

        }

        public void Configure(Form parent)
        {
            ScreenShotConfigureForm frm = new ScreenShotConfigureForm();
            frm.Init(converter, AutoConvert, InputFolder, InputFileExtension, OutputFolder);

            if ( frm.ShowDialog(parent) == DialogResult.OK)
            {
                if (watcher != null)
                    watcher.Stop();

                AutoConvert = frm.AutoConvert;
                InputFolder = frm.InputFolder;
                OutputFolder = frm.OutputFolder;
                converter.OriginalImageOption = frm.OriginalImageOption;
                converter.OriginalImageOptionDirectory = frm.OriginalImageDirectory;
                InputFileExtension = frm.InputFileExtension;
                converter.OutputFileExtension = frm.OutputFileExtension;
                converter.FolderNameFormat = frm.FolderNameFormat;
                converter.FileNameFormat = frm.FileNameFormat;
                converter.KeepMasterConvertedImage = frm.KeepMasterConvertedImage;
                converter.CropResizeImage1 = frm.CropResizeImage1;
                converter.CropResizeImage2 = frm.CropResizeImage2;
                converter.CropResizeArea1 = frm.CropResizeArea1;
                converter.CropResizeArea2 = frm.CropResizeArea2;
                converter.HighRes = frm.HighRes;
                converter.ClipboardOption = frm.ClipboardOption;
                converter.Quality = frm.Quality;

                if (watcher != null)
                    watcher.Start(InputFolder,InputFileExtension.ToString(),OutputFolder);
            }
        }


        public bool Start(Action<Action> invokeOnUiThreadp, Action<string> logger,
                                        Func<Tuple<string, string, string>> currentloccmdr, int watchdelaytime)
        {
            Stop();

            invokeOnUiThread = invokeOnUiThreadp;

            watcher = new ScreenshotDirectoryWatcher(CallWithConverter,logger,currentloccmdr, watchdelaytime);   // pass function to get the convert going
            watcher.OnScreenshot += ConvertCompleted;  // and function for it to call when its over..

            return watcher.Start(InputFolder, InputFileExtension.ToString(),OutputFolder);       // you can restart a watcher without stopping it..
        }

        public void Stop()
        {
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }
        }

        public void NewJournalEntry(JournalEntry je)       // will be in UI thread
        {
            if (watcher != null)
            {
                watcher.NewJournalEntry(je);
            }
        }

        private void CallWithConverter(Action<ScreenShotImageConverter> cb)           // called by Watcher with a function to run in the UI main thread..
        {

            if ( !Application.MessageLoop)
            {
                invokeOnUiThread(() => { CallWithConverter(cb); });
            }
            else
            {
                System.Diagnostics.Debug.Assert(Application.MessageLoop);

                if (AutoConvert)
                {
                    cb(converter);                                  // call the processor the system wants. Function needs an image converter.  Back to processScreenshot
                }
            }
        }

        private void ConvertCompleted(string infile, string outfile, Size sz, JournalEvents.JournalScreenshot ss) // Called by the watcher when a convert had completed, in UI thread
        {
            System.Diagnostics.Debug.Assert(Application.MessageLoop);
            OnScreenshot?.Invoke(infile,outfile, sz, ss);
        }
    }
}
