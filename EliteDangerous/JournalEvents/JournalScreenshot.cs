/*
 * Copyright © 2016-2018 EDDiscovery development team
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
using BaseUtils.JSON;
using System;
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

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)  
        {
            info = BaseUtils.FieldBuilder.Build("At ".T(EDTx.JournalScreenshot_At), Body , "< in ".T(EDTx.JournalScreenshot_in), System , "File: ".T(EDTx.JournalScreenshot_File), Filename, 
                        "Width: ".T(EDTx.JournalScreenshot_Width), Width , "Height: ".T(EDTx.JournalScreenshot_Height), Height, "Latitude: ".T(EDTx.JournalEntry_Latitude), JournalFieldNaming.RLat(nLatitude), "Longitude: ".T(EDTx.JournalEntry_Longitude), JournalFieldNaming.RLong(nLongitude));
            detailed = "";
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
                EliteDangerousCore.DB.UserDatabase.Instance.ExecuteWithDatabase(cn=> UpdateJsonEntry(jo,cn.Connection) );
            }
        }
    }
}
