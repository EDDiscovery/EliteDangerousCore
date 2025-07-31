/*
 * Copyright © 2016-2023 EDDiscovery development team
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
 *
 */
using QuickJSON;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Screenshot)]
    public class JournalScreenshot : JournalEntry
    {
        public JournalScreenshot(JObject evt ) : base(evt, JournalTypeEnum.Screenshot)
        {
            Filename = evt["Filename"].Str();
            Width = evt["Width"].Int();
            Height = evt["Height"].Int();
            System = evt["System"].Str();
            Body = evt["Body"].Str();
            nLatitude = evt["Latitude"].DoubleNull();
            nLongitude = evt["Longitude"].DoubleNull();
            nAltitude = evt["Altitude"].DoubleNull();
            nHeading = evt["Heading"].DoubleNull();

            EDDInputFile = evt["EDDInputFile"].Str();
            EDDOutputFile = evt["EDDOutputFile"].Str();
            EDDOutputWidth = evt["EDDOutputWidth"].Int();
            EDDOutputHeight = evt["EDDOutputHeight"].Int();
        }

        public string Filename { get; set; }        // orginal from elite
        public int Width { get; set; }
        public int Height { get; set; }
        public string System { get; set; }
        public string Body { get; set; }
        public double? nLatitude { get; set; }
        public double? nLongitude { get; set; }
        public double? nAltitude { get; set; }
        public double? nHeading { get; set; }

        public string EDDInputFile { get; set; }        // may be Deleted
        public string EDDOutputFile { get; set; }
        public int EDDOutputWidth { get; set; }
        public int EDDOutputHeight { get; set; }

        public override string GetInfo()  
        {
            return BaseUtils.FieldBuilder.Build("At ".Tx(), Body , "< in ".Tx(), System , "File".Tx()+": ", Filename, 
                        "Width".Tx()+": ", Width , "Height".Tx()+": ", Height,
                        "Latitude: ;°;F4".Tx(), nLatitude, "Longitude: ;°;F4".Tx(), nLongitude);

        }

        public void SetConvertedFilename(string input_filename, string output_filename, int width, int height)  // called when screenshot is captured, records info for later display
        {
            EDDInputFile = input_filename;
            EDDOutputFile = output_filename;
            EDDOutputWidth = width;
            EDDOutputHeight = height;

            JObject jo = GetJsonCloned();       // get a fresh copy as we are about to mod it
            if (jo != null)
            {
                jo["EDDInputFile"] = input_filename;
                jo["EDDOutputFile"] = output_filename;
                jo["EDDOutputWidth"] = width;
                jo["EDDOutputHeight"] = height;
                EliteDangerousCore.DB.UserDatabase.Instance.DBWrite(cn=> UpdateJsonEntry(jo,cn, null) );
            }
        }

        static public string DefaultInputDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
        }

        public Tuple<string,Size?> GetScreenshotPath(string convertedfolder = null)
        {
            if (EDDOutputFile.HasChars() && File.Exists(EDDOutputFile))
                return new Tuple<string,Size?>(EDDOutputFile,new Size(EDDOutputWidth,EDDOutputHeight));

            if (Filename.StartsWith("\\ED_Pictures\\"))     // if its an ss record, try and find it either in watchedfolder or in default loc
            {
                string filepart = Filename.Substring(13);

                if ( convertedfolder != null )
                {
                    string filenameout = Path.Combine(convertedfolder, filepart);
                    if (File.Exists(filenameout))
                    {
                        return new Tuple<string, Size?>(filenameout, null);
                    }
                }

                string defloc = Path.Combine(DefaultInputDir(), filepart);
                if (File.Exists(defloc))
                    return new Tuple<string, Size?>(defloc, null);
            }

            return null;
        }


    }
}
