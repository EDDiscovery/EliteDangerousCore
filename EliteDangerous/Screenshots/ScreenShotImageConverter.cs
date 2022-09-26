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

using BaseUtils;
using EliteDangerousCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;


namespace EliteDangerousCore.ScreenShots
{
    public class ScreenShotImageConverter       // leaf class, no upper dependencies
    {
        // config

        public int FolderNameFormat { get; set; } = 0;
        public int FileNameFormat { get; set; } = 0;

        public static string[] SubFolderSelections = new string[]
        {
            "None",         //0
            "System Name",
            "YYYY-MM-DD",
            "DD-MM-YYYY",
            "MM-DD-YYYY",
            "YYYY-MM-DD Sysname",   //5
            "DD-MM-YYYY Sysname",
            "MM-DD-YYYY Sysname",
            "CMDRName",
            "CMDRName Sysname",
            "YYYY-MM-DD CMDRName Sysname",  //10
            "CMDRName\\Sysname"
        };

        private static string[] FileNameCtrl = new string[]
        {
            "Sysname (YYYYMMDD-HHMMSS)",  "%S (%yyyy%MM%dd-%HH%mm%ss)%H",
            "Sysname (Windows dateformat)", "%S (%WT)%H",
            "YYYY-MM-DD HH-MM-SS Sysname", "%yyyy-%MM-%dd %HH-%mm-%ss %S%H",
            "DD-MM-YYYY HH-MM-SS Sysname", "%dd-%MM-%yyyy %HH-%mm-%ss %S%H",
            "MM-DD-YYYY HH-MM-SS Sysname", "%MM-%dd-%yyyy %HH-%mm-%ss %S%H",            
            "HH-MM-SS Sysname", "%HH-%mm-%ss %S%H",            
            "HH-MM-SS", "%HH-%mm-%ss%H",
            "Sysname", "%S%H",            
            "Keep original", "%O",
            "Sysname BodyName (YYYYMMDD-HHMMSS)",   "%S %B (%yyyy%MM%dd-%HH%mm%ss)%H",
            "Sysname BodyName (Windows dateformat)", "%S %B (%WT)%H",
            "YYYY-MM-DD HH-MM-SS Sysname-BodyName", "%yyyy-%MM-%dd %HH-%mm-%ss %S%BD%H",
            "DD-MM-YYYY HH-MM-SS Sysname-BodyName","%dd-%MM-%yyyy %HH-%mm-%ss %S%BD%H",
            "MM-DD-YYYY HH-MM-SS Sysname-BodyName", "%MM-%dd-%yyyy %HH-%mm-%ss %S%BD%H",
            "HH-MM-SS Sysname-BodyName",        "%HH-%mm-%ss %S%BD%H",
            "Sysname-BodyName",               "%S%BD%H",
            "YYYY-MM-DD_HH-MM-SS_Sysname",    "%yyyy-%MM-%dd_%HH-%mm-%ss_%S%H",            
            "YYYY-MM-DD_HH-MM-SS_Sysname_BodyName",     "%yyyy-%MM-%dd_%HH-%mm-%ss_%S_%B%H",
            "YYYY-MM-DD HH-MM-SS Sysname @ BodyName",   "%yyyy-%MM-%dd %HH-%mm-%ss %S @ %B%H",
            "Sysname @ BodyName",   "%S @ %B%H",
            "BodyName @ Sysname",   "%B @ %S%H",
            "YYYY-MM-DD HH-MM-SS Sysname @ BodyName", "%yyyy-%MM-%dd %HH-%mm-%ss %S @ %B%H",
            "DD-MM-YYYY HH-MM-SS Sysname @ BodyName","%dd-%MM-%yyyy %HH-%mm-%ss %S @ %B%H",
            "MM-DD-YYYY HH-MM-SS Sysname @ BodyName", "%MM-%dd-%yyyy %HH-%mm-%ss %S @ %B%H",
            "Bodyname (YYYYMMDD-HHMMSS)",  "%B (%yyyy%MM%dd-%HH%mm%ss)%H",
            "Bodyname (Windows dateformat)", "%B (%WT)%H",
            "HH-MM-SS Bodyname", "%HH-%mm-%ss %B%H",
            "YYYY-MM-DD HH-MM-SS Bodyname", "%yyyy-%MM-%dd %HH-%mm-%ss %B%H",
            "DD-MM-YYYY HH-MM-SS Bodyname", "%dd-%MM-%yyyy %HH-%mm-%ss %B%H",
            "MM-DD-YYYY HH-MM-SS Bodyname", "%MM-%dd-%yyyy %HH-%mm-%ss %B%H",
            "Bodyname", "%B%H",
            "YYYY-MM-DD_HH-MM-SS_Bodyname",    "%yyyy-%MM-%dd_%HH-%mm-%ss_%B%H",
        };

        public static string[] FileNameFormats = FileNameCtrl.Where((s,i)=>i%2 ==0).ToArray();      // every other is a selection

        // Configuration

        public enum CropResizeOptions { Off, Crop, Resize };
        public CropResizeOptions CropResizeImage1;
        public CropResizeOptions CropResizeImage2;
        public Rectangle CropResizeArea1 = new Rectangle();
        public Rectangle CropResizeArea2 = new Rectangle();

        public bool KeepMasterConvertedImage = false;

        public enum OutputTypes { png, jpg, bmp, tiff };
        public OutputTypes OutputFileExtension { get; set; } = OutputTypes.png;

        public enum OriginalImageOptions { Leave, Delete, Move };
        public OriginalImageOptions OriginalImageOption { get; set; } = OriginalImageOptions.Leave;
        public string OriginalImageOptionDirectory { get; set; } = @"c:\";

        public enum ClipboardOptions { NoCopy, CopyMaster, CopyImage1, CopyImage2 };
        public ClipboardOptions ClipboardOption { get; set; } =  ClipboardOptions.NoCopy;

        public bool HighRes { get; set; } = false;

        public int Quality { get; set; } = 85;

        // convert bmp from inputfilename with filetime
        // into outputfolder with properties body,system,cmdrname
        // return final inputfilename, output filename, size. or null if failed.

        public Tuple<string, string, Size> Convert(Bitmap bmp, string inputfilename, DateTime filetime, string outputfolder, 
                                                   string bodyname, string systemname, string cmdrname, Action<string> logit ) // can call independent of watcher, pass in bmp to convert
        {
            outputfolder = SubFolder(FolderNameFormat, outputfolder, systemname, cmdrname, filetime);

            if (!Directory.Exists(outputfolder))
                Directory.CreateDirectory(outputfolder);

            // bmp is the original bitmap at full res

            int index = 0;

            string outputfilename;
            string secondfilename, thirdfilename;

            do                                          // add _N on the filename for index>0, to make them unique.
            {
                string fn = CreateFileName(systemname, bodyname, inputfilename, FileNameFormat, HighRes, filetime) + (index == 0 ? "" : "_" + index);

                outputfilename = Path.Combine(outputfolder, fn + "." + OutputFileExtension.ToString());
                secondfilename = Path.Combine(outputfolder, fn + "-1." + OutputFileExtension.ToString());
                thirdfilename = Path.Combine(outputfolder, fn + "-2." + OutputFileExtension.ToString());
                index++;
            } while (File.Exists(outputfilename));          // if name exists, pick another

            if ( outputfilename.Equals(inputfilename,StringComparison.InvariantCultureIgnoreCase))
            {
                logit(string.Format(("Cannot convert {0} to {1} as names clash" + Environment.NewLine + "Pick a different conversion folder or a different output format"),inputfilename, outputfilename));
                return null;
            }

            // the OutputFilename should point to the best screenshot, and FinalSize points to this

            Size finalsize = Size.Empty;

            if (CropResizeImage1 == CropResizeOptions.Off || KeepMasterConvertedImage) // if resize 1 off, or keep full size.
            {
                WriteBMP(bmp, outputfilename, filetime);
                finalsize = bmp.Size;        // this is our image to use in the rest of the system

                if (ClipboardOption == ClipboardOptions.CopyMaster)
                {
                    CopyClipboardSafe(bmp, logit);
                }
            }

            if (CropResizeImage1 != CropResizeOptions.Off)
            {
                Bitmap converted = null;

                if (CropResizeImage1 == CropResizeOptions.Crop)
                {
                    converted  = bmp.CropImage(CropResizeArea1);
                }
                else
                {
                    converted = bmp.ResizeImage(CropResizeArea1.Width, CropResizeArea1.Height);
                }

                string nametouse = KeepMasterConvertedImage ? secondfilename : outputfilename;     // if keep full sized off, we use this one as our image
                WriteBMP(converted, nametouse, filetime);

                if (!KeepMasterConvertedImage)       // if not keeping the full sized one, its final
                    finalsize = converted.Size;

                if (ClipboardOption == ClipboardOptions.CopyImage1)
                {
                    CopyClipboardSafe(converted, logit);
                }

                converted.Dispose();
            }

            if (CropResizeImage2 != CropResizeOptions.Off)
            {
                Bitmap converted = null;

                if (CropResizeImage2 == CropResizeOptions.Crop)
                {
                    converted = bmp.CropImage(CropResizeArea2);
                }
                else
                {
                    converted = bmp.ResizeImage(CropResizeArea2.Width, CropResizeArea2.Height);
                }

                WriteBMP(converted, thirdfilename, filetime);

                if (ClipboardOption == ClipboardOptions.CopyImage2)
                {
                    CopyClipboardSafe(converted, logit);
                }

                converted.Dispose();
            }

            if (OriginalImageOption == OriginalImageOptions.Delete)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Delete {0}", inputfilename);
                    File.Delete(inputfilename);
                    inputfilename = null;
                }
                catch
                {
                    logit($"Unable to remove file {inputfilename}");
                }

            }
            else if (OriginalImageOption == OriginalImageOptions.Move)
            {
                string outfile = Path.Combine(OriginalImageOptionDirectory, Path.GetFileNameWithoutExtension(outputfilename) + Path.GetExtension(inputfilename));
                int indexi = 1;
                while (true)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Move {0} to {1}", inputfilename, outfile);
                        File.Move(inputfilename, outfile);
                        inputfilename = outfile;
                        break;
                    }
                    catch
                    {
                        outfile = Path.Combine(OriginalImageOptionDirectory, Path.GetFileNameWithoutExtension(outfile) + "-" + indexi++.ToString() + Path.GetExtension(outfile));
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Convert " + inputfilename + " at " + systemname + " to " + outputfilename);

            logit(string.Format("Converted {0} to {1}".T(EDCTx.ScreenShotImageConverter_CNV), Path.GetFileName(inputfilename) , outputfilename));

            return new Tuple<string, string, Size>(inputfilename, outputfilename, finalsize);
        }

        private System.Drawing.Imaging.ImageCodecInfo GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == format.Guid);
        }

        private void WriteBMP(Bitmap bmp, string filename, DateTime datetimeutc)
        {
            using (var memstream = new MemoryStream())
            {
                if (OutputFileExtension == OutputTypes.jpg)
                {
                    var encodeparams = new System.Drawing.Imaging.EncoderParameters();
                    encodeparams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
                    bmp.Save(memstream, GetCodec(System.Drawing.Imaging.ImageFormat.Jpeg), encodeparams);
                }
                else if (OutputFileExtension == OutputTypes.tiff)
                {
                    bmp.Save(memstream, System.Drawing.Imaging.ImageFormat.Tiff);
                }
                else if (OutputFileExtension == OutputTypes.bmp)
                {
                    bmp.Save(memstream, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else
                {
                    bmp.Save(memstream, System.Drawing.Imaging.ImageFormat.Png);
                }

                File.WriteAllBytes(filename, memstream.ToArray());
                File.SetCreationTime(filename, datetimeutc);
            }
        }

        // helpers for above

        public static string SubFolder(int foldernameformat, string outputfolder, string systemname, string cmdrname, DateTime timestamputc)
        {
            timestamputc = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(timestamputc);       // convert UTC to selected local time

            if (String.IsNullOrWhiteSpace(outputfolder))
            {
                outputfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous", "Converted");
            }

            switch (foldernameformat)
            {
                case 1:     // system name
                    outputfolder = Path.Combine(outputfolder, systemname.SafeFileString());
                    break;

                case 2:     // "YYYY-MM-DD"
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("yyyy-MM-dd"));
                    break;
                case 3:     // "DD-MM-YYYY"
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("dd-MM-yyyy"));
                    break;
                case 4:     // "MM-DD-YYYY"
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("MM-dd-yyyy"));
                    break;

                case 5:  //"YYYY-MM-DD Sysname",
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("yyyy-MM-dd") + " " + systemname.SafeFileString());
                    break;

                case 6:  //"DD-MM-YYYY Sysname",
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("dd-MM-yyyy") + " " + systemname.SafeFileString());
                    break;

                case 7: //"MM-DD-YYYY Sysname"
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("MM-dd-yyyy") + " " + systemname.SafeFileString());
                    break;

                case 8: // CMDR name
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString());
                    break;

                case 9: // CMDR name at sysname
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString() + " at " + systemname.SafeFileString());
                    break;

                case 10: // YYYY - MM - DD CMDR name at sysname
                    outputfolder = Path.Combine(outputfolder, timestamputc.ToString("yyyy-MM-dd") + " " +
                          cmdrname.SafeFileString() + " at " + systemname.SafeFileString());
                    break;

                case 11: // CMDR Name \ SystemName
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString(), systemname.SafeFileString());
                    break;
            }

            return outputfolder;
        }


        public static string CreateFileName(string cur_sysname, string cur_bodyname, string inputfile, int formatindex, bool hires, DateTime timestamputc)
        {
            timestamputc = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(timestamputc);       // convert UTC to selected local time

            cur_sysname = cur_sysname.SafeFileString();
            cur_bodyname = cur_bodyname.SafeFileString();
            bool hasbodyname = cur_bodyname.Length > 0;

            string ctrl = (formatindex >= 0 && formatindex < FileNameFormats.Length) ? FileNameCtrl[formatindex * 2 + 1] : "%O";    // being paranoid

            System.Text.StringBuilder b = new System.Text.StringBuilder();
            for (int i = 0; i < ctrl.Length; i++)
            {
                string part = ctrl.Substring(i);
                if (part.StartsWith("%yyyy") )
                {
                    b.Append(timestamputc.ToString("yyyy"));
                    i += 4;
                }
                else if (part.StartsWith("%MM") || part.StartsWith("%dd") || part.StartsWith("%HH") || part.StartsWith("%mm") || part.StartsWith("%ss") )
                {
                    b.Append(timestamputc.ToString(part.Substring(1,2)));
                    i += 2;
                }
                else if (part.StartsWith("%WT"))
                {
                    string time = timestamputc.ToString();
                    time = time.Replace(":", "-");
                    time = time.Replace("/", "-");          // Rob found it was outputting 21/2/2020 on mine, so we need more replaces
                    time = time.Replace("\\", "-");
                    time = time.SafeFileString();
                    b.Append(time);
                    i += 2;
                }
                else if (part.StartsWith("%BD"))
                {
                    if (hasbodyname)
                        b.Append("-" + cur_bodyname);
                    i += 2;
                }
                else if (part.StartsWith("%B"))
                {
                    b.Append(cur_bodyname);
                    i += 1;
                }
                else if (part.StartsWith("%S"))
                {
                    b.Append(cur_sysname);
                    i += 1;
                }
                else if (part.StartsWith("%H"))
                {
                    if (hires && Path.GetFileName(inputfile).Contains("HighRes"))
                        b.Append(" (HighRes)");
                    i += 1;
                }
                else if (part.StartsWith("%O"))
                {
                    b.Append(Path.GetFileNameWithoutExtension(inputfile));
                    i += 1;
                }
                else
                {
                    b.Append(ctrl[i]);
                }
            }

            return b.ToString();
        }

        static void CopyClipboardSafe(Bitmap bmp, Action<string> logit)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetImage(bmp);
            }
            catch
            {
                logit("Copying image to clipboard failed");
            }
        }
    }
}

