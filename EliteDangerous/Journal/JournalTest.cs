/*
 * Copyright © 2024 - 2024 EDDiscovery development team
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EliteDangerousCore
{
    public static class JournalTest
    {
        public static void CheckAllJournalsGetInfoDescription(string path, string filename, string outfile)
        {
            FileInfo[] allFiles = Directory.EnumerateFiles(path, filename, SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(p => p.FullName).ToArray();

            using (StreamWriter st = new StreamWriter(outfile))
            {
                foreach (var fi in allFiles)
                {
                    System.Diagnostics.Debug.WriteLine($"Check {fi.FullName}");
                    using (StreamReader sr = new StreamReader(fi.FullName))         // read directly from file.. presume UTF8 no bom
                    {
                        int lineno = 1;
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != "")
                            {
                                var je = JournalEntry.CreateJournalEntry(line, false, true);
                                if (je != null)
                                {
                                    JournalEntry.FillInformationData fid = new JournalEntry.FillInformationData()
                                    { System = new SystemClass("Sol"), WhereAmI = "Earth", NextJumpSystemName = "Lembava", NextJumpSystemAddress = 1000 };

                                    string info = je.GetInfo() ?? je.GetInfo(fid);
                                    string detailed = je.GetDetailed() ?? je.GetDetailed(fid);

                                    st.WriteLine($"'{je.EventTypeStr}' {info}\r\n```{detailed ?? "Not present"}```");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"ERROR DECODING {fi.FullName}:{lineno} : {line}");
                                }
                            }

                            lineno++;
                        }
                    }
                }
            }
        }
        public static void DumpHistoryGetInfoDescription(HistoryList hl, string outfile)
        {
            using (StreamWriter st = new StreamWriter(outfile))
            {
                foreach (var he in hl.EntryOrder())
                {
                    string info = he.GetInfo();
                    string detailed = he.GetDetailed();

                    st.WriteLine($"\r\nEvent: {he.EventTimeUTC} '{he.EventSummary}'\r\nInfo:```{info}```\r\nDetailed: ```{detailed ?? "Not present"}```");
                }
            }
        }
    }
}
