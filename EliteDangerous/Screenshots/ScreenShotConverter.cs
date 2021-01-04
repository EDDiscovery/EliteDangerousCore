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

namespace EliteDangerousCore.ScreenShots
{
    public class ScreenShotConverter
    {
        public string InputFolder { get; set; }
        public enum InputTypes { bmp, jpg, png };
        public InputTypes InputFileExtension { get; set; } = InputTypes.bmp;
        public string OutputFolder { get; set; }
        public bool AutoConvert { get; set; }
        private Func<IScreenShotConfigureForm> ConfigureFormCreator;

        // called on screenshot. inputfilename location (may be null if deleted), outputfilename location, image size, screenshot (may be null)
        public event Action<string, string, Size, JournalEvents.JournalScreenshot> OnScreenshot;     

        public ScreenShotImageConverter converter;
        private ScreenshotDirectoryWatcher watcher;

        private Action<Action> invokeOnUiThread;

        public ScreenShotConverter(Func<IScreenShotConfigureForm> configformcreator, Action<Action> invokeOnUiThreadp, Action<Image> copytoclipboard)
        {
            converter = new ScreenShotImageConverter();
            converter.OnCopyToClipboard += copytoclipboard;
            ConfigureFormCreator = configformcreator;
            invokeOnUiThread = invokeOnUiThreadp;

            string screenshotsDirdefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
            string outputDirdefault = Path.Combine(screenshotsDirdefault, "Converted");

            InputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("ImageHandlerScreenshotsDir", screenshotsDirdefault );
            if (!Directory.Exists(InputFolder))
                InputFolder = screenshotsDirdefault;

            OutputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("ImageHandlerOutputDir", outputDirdefault);
            if (!Directory.Exists(OutputFolder))
                OutputFolder = outputDirdefault;

            AutoConvert = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerAutoconvert", false);


            try
            {       // just in case
                if (EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("checkBoxRemove", false))    // backward compatible
                {
                    converter.OriginalImageOption = ScreenShotImageConverter.OriginalImageOptions.Delete;
                }
                else
                    converter.OriginalImageOption = (ScreenShotImageConverter.OriginalImageOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerOriginalImageOption", 0);

                EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("checkBoxRemove");

                converter.OriginalImageOptionDirectory = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("ImageHandlerOriginalDirectory", @"c:\");

                if (EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerClipboard", false))    // backward compatible
                {
                    converter.ClipboardOption = ScreenShotImageConverter.ClipboardOptions.CopyMaster;
                }
                else
                    converter.ClipboardOption = (ScreenShotImageConverter.ClipboardOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerClipboardOption", 0);

                EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("ImageHandlerClipboard");

                converter.HighRes = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("checkBoxHires", false);
                if (EliteDangerousCore.DB.UserDatabase.Instance.KeyExists("ImageHandlerCropImage"))
                {
                    converter.CropResizeImage1 = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerCropImage", false) ? ScreenShotImageConverter.CropResizeOptions.Crop : ScreenShotImageConverter.CropResizeOptions.Off;
                    EliteDangerousCore.DB.UserDatabase.Instance.DeleteKey("ImageHandlerCropImage");
                }
                else
                    converter.CropResizeImage1 = (ScreenShotImageConverter.CropResizeOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropResizeImage1", 0);

                converter.CropResizeImage2 = (ScreenShotImageConverter.CropResizeOptions)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropResizeImage2", 0);
                converter.CropResizeArea1 = new Rectangle(EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropLeft", 0), EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropTop", 0),
                                        EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropWidth", 1920), EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropHeight", 1024));
                converter.CropResizeArea2 = new Rectangle(EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropLeft2", 0), EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropTop2", 0),
                                        EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropWidth2", 1920), EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerCropHeight2", 1024));

                InputFileExtension = (InputTypes)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("comboBoxScanFor", 0);
                converter.OutputFileExtension = (ScreenShotImageConverter.OutputTypes)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ImageHandlerFormatNr", 0);

                converter.KeepMasterConvertedImage = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerKeepFullSized", false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.ToString());
            }

            converter.FileNameFormat = Math.Min(Math.Max(0, EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("comboBoxFileNameFormat", 0)), ScreenShotImageConverter.FileNameFormats.Length - 1);
            converter.FolderNameFormat = Math.Min(Math.Max(0, EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("comboBoxSubFolder", 0)), ScreenShotImageConverter.SubFolderSelections.Length - 1);
        }

        public void SaveSettings()
        {
            DB.UserDatabase.Instance.PutSettingString("ImageHandlerOutputDir", OutputFolder);
            DB.UserDatabase.Instance.PutSettingString("ImageHandlerScreenshotsDir", InputFolder);
            DB.UserDatabase.Instance.PutSettingBool("ImageHandlerAutoconvert", AutoConvert );      // names are all over the place.. historical
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerOriginalImageOption", (int)converter.OriginalImageOption);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingString("ImageHandlerOriginalDirectory", converter.OriginalImageOptionDirectory);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerClipboardOption", (int)converter.ClipboardOption);
            DB.UserDatabase.Instance.PutSettingBool("checkBoxHires", converter.HighRes );
            DB.UserDatabase.Instance.PutSettingBool("ImageHandlerKeepFullSized", converter.KeepMasterConvertedImage);

            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropResizeImage1", (int)converter.CropResizeImage1);      // fires the checked handler which sets the readonly mode of the controls
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropTop", converter.CropResizeArea1.Top);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropLeft", converter.CropResizeArea1.Left);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropWidth", converter.CropResizeArea1.Width);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropHeight", converter.CropResizeArea1.Height);

            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropResizeImage2", (int)converter.CropResizeImage2);      // fires the checked handler which sets the readonly mode of the controls
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropTop2", converter.CropResizeArea2.Top);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropLeft2", converter.CropResizeArea2.Left);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropWidth2", converter.CropResizeArea2.Width);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropHeight2", converter.CropResizeArea2.Height);

            DB.UserDatabase.Instance.PutSettingInt("comboBoxScanFor", (int)InputFileExtension);
            DB.UserDatabase.Instance.PutSettingInt("ImageHandlerFormatNr", (int)converter.OutputFileExtension);
            DB.UserDatabase.Instance.PutSettingInt("comboBoxFileNameFormat", converter.FileNameFormat);
            DB.UserDatabase.Instance.PutSettingInt("comboBoxSubFolder", converter.FolderNameFormat);

        }

        public void Configure()
        {
            IScreenShotConfigureForm frm = ConfigureFormCreator();

            if (frm.Show(converter, AutoConvert, InputFolder, InputFileExtension, OutputFolder))
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

                if (watcher != null)
                    watcher.Start(InputFolder,InputFileExtension.ToString(),OutputFolder);
            }
        }


        public bool Start(Action<string> logger, Func<Tuple<string, string, string>> currentloccmdr)
        {
            Stop();

            watcher = new ScreenshotDirectoryWatcher(CallWithConverter,logger,currentloccmdr);   // pass function to get the convert going
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
            invokeOnUiThread(() =>
            {
                if (AutoConvert)
                {
                    cb(converter);                                  // call the processor the system wants. Function needs an image converter.  Back to processScreenshot
                }
            });
        }

        private void ConvertCompleted(string infile, string outfile, Size sz, JournalEvents.JournalScreenshot ss) // Called by the watcher when a convert had completed, in UI thread
        {
            OnScreenshot?.Invoke(infile,outfile, sz, ss);
        }
    }
}
