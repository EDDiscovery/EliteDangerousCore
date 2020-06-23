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

        public static string[] FileNameFormats = new string[]
        {
            "Sysname (YYYYMMDD-HHMMSS)",            //0
            "Sysname (Windows dateformat)",
            "YYYY-MM-DD HH-MM-SS Sysname",
            "DD-MM-YYYY HH-MM-SS Sysname",
            "MM-DD-YYYY HH-MM-SS Sysname",          //4
            "HH-MM-SS Sysname",
            "HH-MM-SS",
            "Sysname",
            "Keep original",                        // 8
            "Sysname BodyName (YYYYMMDD-HHMMSS)",       //9
            "Sysname BodyName (Windows dateformat)",
            "YYYY-MM-DD HH-MM-SS Sysname BodyName",     //11
            "DD-MM-YYYY HH-MM-SS Sysname BodyName",
            "MM-DD-YYYY HH-MM-SS Sysname BodyName",     //13
            "HH-MM-SS Sysname BodyName",        //14
            "Sysname BodyName",                 //15
        };

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

        public Tuple<string,Size> Convert(Bitmap bmp, string inputfilename, string outputfolder, DateTime filetime, Action<string> logit, string bodyname, string systemname, string cmdrname ) // can call independent of watcher, pass in bmp to convert
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
                return new Tuple<string, Size>(null,Size.Empty);
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
                    finalsize = bmp.Size;

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
                        break;
                    }
                    catch
                    {
                        outfile = Path.Combine(OriginalImageOptionDirectory, Path.GetFileNameWithoutExtension(outfile) + "-" + indexi++.ToString() + Path.GetExtension(outfile));
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Convert " + inputfilename + " at " + systemname + " to " + outputfilename);

            logit(string.Format("Converted {0} to {1}".T(EDTx.ScreenShotImageConverter_CNV), Path.GetFileName(inputfilename) , outputfilename));

            return new Tuple<string, Size>(outputfilename, finalsize);
        }

        private void WriteBMP(Bitmap bmp, string filename, DateTime datetime)
        {
            using (var memstream = new MemoryStream())
            {
                if (OutputFileExtension == OutputTypes.jpg)
                {
                    bmp.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);
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
                File.SetCreationTime(filename, datetime);
            }
        }

        // helpers for above

        public static string SubFolder(int foldernameformat, string outputfolder, string systemname, string cmdrname, DateTime timestamp)
        {
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
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("yyyy-MM-dd"));
                    break;
                case 3:     // "DD-MM-YYYY"
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("dd-MM-yyyy"));
                    break;
                case 4:     // "MM-DD-YYYY"
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("MM-dd-yyyy"));
                    break;

                case 5:  //"YYYY-MM-DD Sysname",
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("yyyy-MM-dd") + " " + systemname.SafeFileString());
                    break;

                case 6:  //"DD-MM-YYYY Sysname",
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("dd-MM-yyyy") + " " + systemname.SafeFileString());
                    break;

                case 7: //"MM-DD-YYYY Sysname"
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("MM-dd-yyyy") + " " + systemname.SafeFileString());
                    break;

                case 8: // CMDR name
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString());
                    break;

                case 9: // CMDR name at sysname
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString() + " at " + systemname.SafeFileString());
                    break;

                case 10: // YYYY - MM - DD CMDR name at sysname
                    outputfolder = Path.Combine(outputfolder, timestamp.ToString("yyyy-MM-dd") + " " +
                          cmdrname.SafeFileString() + " at " + systemname.SafeFileString());
                    break;

                case 11: // CMDR Name \ SystemName
                    outputfolder = Path.Combine(outputfolder, cmdrname.SafeFileString(), systemname.SafeFileString());
                    break;
            }

            return outputfolder;
        }


        public static string CreateFileName(string cur_sysname, string cur_bodyname, string inputfile, int formatindex, bool hires, DateTime timestamp)
        {
            cur_sysname = cur_sysname.SafeFileString();
            cur_bodyname = cur_bodyname.SafeFileString();

            string postfix = (hires && Path.GetFileName(inputfile).Contains("HighRes")) ? " (HighRes)" : "";
            string bodyinsert = (formatindex >= 9 && formatindex <= 15 && cur_bodyname.Length > 0) ? ("-" + cur_bodyname) : "";

            switch (formatindex)
            {
                case 0:
                case 9:
                    return cur_sysname + bodyinsert + " (" + timestamp.ToString("yyyyMMdd-HHmmss") + ")" + postfix;

                case 1:
                case 10:
                    {
                        string time = timestamp.ToString();
                        time = time.Replace(":", "-");
                        time = time.Replace("/", "-");          // Rob found it was outputting 21/2/2020 on mine, so we need more replaces
                        time = time.Replace("\\", "-");
                        return cur_sysname + bodyinsert + " (" + time + ")" + postfix;
                    }
                case 2:
                case 11:
                    {
                        string time = timestamp.ToString("yyyy-MM-dd HH-mm-ss");
                        return time + " " + cur_sysname + bodyinsert + postfix;
                    }
                case 3:
                case 12:
                    {
                        string time = timestamp.ToString("dd-MM-yyyy HH-mm-ss");
                        return time + " " + cur_sysname + bodyinsert + postfix;
                    }
                case 4:
                case 13:
                    {
                        string time = timestamp.ToString("MM-dd-yyyy HH-mm-ss");
                        return time + " " + cur_sysname + bodyinsert + postfix;
                    }

                case 5:
                case 14:
                    {
                        string time = timestamp.ToString("HH-mm-ss");
                        return time + " " + cur_sysname + bodyinsert + postfix;
                    }

                case 6:
                    {
                        string time = timestamp.ToString("HH-mm-ss");
                        return time + postfix;
                    }

                case 7:
                case 15:
                    {
                        return cur_sysname + bodyinsert + postfix;
                    }

                default:
                    return Path.GetFileNameWithoutExtension(inputfile);
            }
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

