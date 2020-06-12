using EliteDangerousCore.JournalEvents;
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
        public Action<string, Size> OnScreenShot;

        private ScreenShotImageConverter converter;
        private ScreenshotDirectoryWatcher Watcher;

        private Action<Action> invokeOnUiThread;

        public ScreenShotConverter()
        {
            converter = new ScreenShotImageConverter();

            string screenshotsDirdefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
            string outputDirdefault = Path.Combine(screenshotsDirdefault, "Converted");

            InputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("ImageHandlerScreenshotsDir", screenshotsDirdefault );
            if (!Directory.Exists(InputFolder))
                InputFolder = screenshotsDirdefault;

            OutputFolder = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("ImageHandlerOutputDir", outputDirdefault);
            if (!Directory.Exists(OutputFolder))
                OutputFolder = outputDirdefault;

            AutoConvert = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerAutoconvert", false);
            converter.RemoveOriginal = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("checkBoxRemove", false);
            converter.HighRes = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("checkBoxHires", false);
            converter.CopyToClipboard = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingBool("ImageHandlerClipboard", false);

            try
            {       // just in case
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
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingString("ImageHandlerOutputDir", OutputFolder);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingString("ImageHandlerScreenshotsDir", InputFolder);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingBool("ImageHandlerAutoconvert", AutoConvert );      // names are all over the place.. historical
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingBool("checkBoxRemove", converter.RemoveOriginal );
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingBool("checkBoxHires", converter.HighRes );
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingBool("ImageHandlerClipboard", converter.CopyToClipboard);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingBool("ImageHandlerKeepFullSized", converter.KeepMasterConvertedImage);

            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropResizeImage1", (int)converter.CropResizeImage1);      // fires the checked handler which sets the readonly mode of the controls
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropTop", converter.CropResizeArea1.Top);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropLeft", converter.CropResizeArea1.Left);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropWidth", converter.CropResizeArea1.Width);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropHeight", converter.CropResizeArea1.Height);

            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropResizeImage2", (int)converter.CropResizeImage2);      // fires the checked handler which sets the readonly mode of the controls
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropTop2", converter.CropResizeArea2.Top);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropLeft2", converter.CropResizeArea2.Left);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropWidth2", converter.CropResizeArea2.Width);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerCropHeight2", converter.CropResizeArea2.Height);

            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("comboBoxScanFor", (int)InputFileExtension);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ImageHandlerFormatNr", (int)converter.OutputFileExtension);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("comboBoxFileNameFormat", converter.FileNameFormat);
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("comboBoxSubFolder", converter.FolderNameFormat);

        }
   

        public bool Start(Action<Action> invokeOnUiThreadp, Action<string> logger,
                                        Func<Tuple<string, string, string>> currentloccmdr)
        {
            Stop();

            invokeOnUiThread = invokeOnUiThreadp;

            Watcher = new ScreenshotDirectoryWatcher(CallWithConverter,logger,currentloccmdr);   // pass function to get the convert going
            Watcher.OnScreenshot += ConvertCompleted;  // and function for it to call when its over..

            return Watcher.Start(InputFolder, InputFileExtension.ToString(),OutputFolder);       // you can restart a watcher without stopping it..
        }

        public void Stop()
        {
            if (Watcher != null)
            {
                Watcher.Stop();
                Watcher.Dispose();
                Watcher = null;
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

        private void ConvertCompleted(string file,Size sz) // Called by the watcher when a convert had completed, in UI thread
        {
            System.Diagnostics.Debug.Assert(Application.MessageLoop);
            OnScreenShot?.Invoke(file,sz);
        }
    }
}
