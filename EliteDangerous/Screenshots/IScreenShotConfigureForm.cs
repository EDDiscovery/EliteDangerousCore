using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace EliteDangerousCore.ScreenShots
{
    public interface IScreenShotConfigureForm
    {
        bool AutoConvert { get; }
        string InputFolder { get; }
        string OutputFolder { get; }
        ScreenShotConverter.InputTypes InputFileExtension { get; }
        ScreenShotImageConverter.OutputTypes OutputFileExtension { get; }
        ScreenShotImageConverter.OriginalImageOptions OriginalImageOption { get; }
        string OriginalImageDirectory { get; }
        ScreenShotImageConverter.ClipboardOptions ClipboardOption { get; }
        int FileNameFormat { get; }
        int FolderNameFormat { get; }
        bool KeepMasterConvertedImage { get; }
        ScreenShotImageConverter.CropResizeOptions CropResizeImage1 { get; }
        Rectangle CropResizeArea1 { get; }
        ScreenShotImageConverter.CropResizeOptions CropResizeImage2 { get; }
        Rectangle CropResizeArea2 { get; }
        bool HighRes { get; }
        bool Show(ScreenShotImageConverter cf, bool autoconvert, string inputfolder, ScreenShotConverter.InputTypes it, string outputfolder);
    }
}
