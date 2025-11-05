/*
 * Copyright 2025 - 2025 EDDiscovery development team
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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Debug tools

        // read in a set of JSON lines exported from say HistoryList.cs:294 and run it thru starscan 2 and the display system

        public static void ProcessAllFromDirectory(string dir, string filepattern, Action<StarScan,List<Tuple<int,HistoryEntry>>> persystemaction = null)
        {
            FileInfo[] find = Directory.EnumerateFiles(dir, filepattern, SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

            foreach (var x in find)
            {
                var mhs = HistoryEntry.CreateFromFile(x.FullName);
                StarScan ss = new StarScan();
                ss.ProcessFromHistory(mhs);
                persystemaction?.Invoke(ss,mhs);
            }
        }

  
        // run these history entries thru the star scanner
        public void ProcessFromHistory(List<Tuple<int,HistoryEntry>> mhs, Action<StarScan,Tuple<int,HistoryEntry>> perstep = null)
        {
            int step = 0;
            foreach (var mhe in mhs)
            {
                HistoryEntry he = mhe.Item2;

                if (he.journalEntry is JournalDocked dck)
                {
                    if (dck.BodyType == BodyDefinitions.BodyType.Settlement)        // if its a settlement, fill in missing body info
                    {
                        dck.BodyID = he.Status.BodyID;
                        dck.Body = he.Status.BodyName;
                    }

                    AddDocking(dck, he.System);
                    perstep?.Invoke(this, mhe);
                }
                if (he.journalEntry is IStarScan ss)
                {
                    if (he.journalEntry is JournalScan js)
                    {
                        System.Diagnostics.Debug.WriteLine($"\r\n{step} {he.EntryType}: `{js.BodyName}` ID: {js.BodyID} - {js.ParentList()} ");
                        (he.journalEntry as IStarScan).AddStarScan(this, he.System);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"\r\n{step} {he.EntryType}: in {he.System.Name}");
                        (he.journalEntry as IStarScan).AddStarScan(this, he.System);
                    }

                    perstep?.Invoke(this,mhe);
                    DumpTree();
                }

                step++;
            }

            AssignPending();
        }

  
        public void DumpTree()
        {
            foreach (var kvp in systemNodesByName)
            {
                kvp.Value.DumpTree();
            }
        }


        #endregion

    }
}

