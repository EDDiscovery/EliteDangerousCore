/*
 * Copyright 2016-2026 EDDiscovery development team
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

using QuickJSON;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EliteDangerousCore.Bindings
{
    public class FrontierDeviceMapping 
    {
        // Read the Frontier devicemappings and turn to JSON

        public static JArray ReadFrontierDeviceMappingFile(string exepath)
        {
            var list = Directory.EnumerateFiles(Path.GetDirectoryName(exepath), "DeviceMappings.xml", SearchOption.AllDirectories).FirstOrDefault();

            if ( list!=null)
            {
                try
                {
                    JArray retlist = new JArray();

                    XElement bindings = XElement.Load(list);

                    foreach (XElement rootelement in bindings.Elements())
                    {
                        string devicename = rootelement.Name.ToString();
                        
                        {
                            var pidt = rootelement.Elements().Where(x => x.Name == "PID").Select(x => x.Value).FirstOrDefault();
                            var vidt = rootelement.Elements().Where(x => x.Name == "VID").Select(x => x.Value).FirstOrDefault();
                            if (pidt != null && vidt != null)
                            {
                                var pid = Convert.ToInt32(pidt, 16);
                                var vid = Convert.ToInt32(vidt, 16);
                               // System.Diagnostics.Debug.WriteLine($"  {devicename} {pid:X} {vid:X}");
                                JArray je = new JArray() { devicename, vid, pid };
                                retlist.Add(je);
                            }
                        }

                        foreach (XElement subelement in rootelement.Elements().Where(x=>x.Name=="Alternative").EmptyIfNull())
                        {
                            {
                                var pidt = subelement.Elements().Where(x => x.Name == "PID").Select(x => x.Value).FirstOrDefault();
                                var vidt = subelement.Elements().Where(x => x.Name == "VID").Select(x => x.Value).FirstOrDefault();
                                if (pidt != null && vidt != null)
                                {
                                    var pid = Convert.ToInt32(pidt, 16);
                                    var vid = Convert.ToInt32(vidt, 16);
                                 //   System.Diagnostics.Debug.WriteLine($"  {devicename} {pid:X} {vid:X}");
                                    JArray je = new JArray() { devicename, vid, pid };
                                    retlist.Add(je);
                                }
                            }
                        }
                    }
                    //System.Diagnostics.Debug.WriteLine($"JSON {retlist.ToString(true)}");

                    return retlist;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception reading DeviceMappings {ex}");
                }
            }

            return null;
        }

        // global list of Device Mapping

        public static JArray DeviceMappingToPIDVID { get; set; } = new JArray();

        // given a device mapping JSON array of [ devicename, pid, vid] return devicename vs pid/vid
        public static string FindPIDVID(int pid, int vid)
        {
            foreach (JArray x in DeviceMappingToPIDVID.EmptyIfNull())
            {
                if (x[1].Int() == vid && x[2].Int() == pid)
                    return x[0].Str();
            }
            return null;
        }

        // given a device mapping JSON array of [ devicename, pid, vid] merge new into master
        public static int MergeMappings(JArray masterjsondevicemap, JArray newdevicemap)
        {
            int merges = 0;
            foreach (JArray nw in newdevicemap.EmptyIfNull())
            {
                string name = nw[0].Str();
                int vid = nw[1].Int();
                int pid = nw[2].Int();

                var found = masterjsondevicemap.Find(x => x[0].Str() == name && x[1].Int() == vid && x[2].Int() == pid);        // must all be there otherwise a new mapping set
                if (found == null)
                {
                    masterjsondevicemap.Add(nw);
                    merges++;
                }
            }
            return merges;
        }
    }
}
