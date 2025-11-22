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
using System.Threading;
using System.Web.UI.WebControls;

namespace EliteDangerousCore.StarScan2
{
    public partial class StarScan
    {
        #region Debug tools

        // all systems, produce a display of them in folder or just generate them if outputdir = null
        public void DrawAllSystemsToFolder(string outputdir)
        {
            System.Diagnostics.Debug.WriteLine($"Draw {systemNodesByName.Count} systems for Cmdr {EDCommander.Current.Name}");
            int threads = 1;
            List<SystemNode> nodes = systemNodesByName.Values.Take(5000).ToList();
            CountdownEvent cd = new CountdownEvent(threads);
            for (int i = 0; i < threads; i++)
            {
                Thread t1 = new Thread(new ParameterizedThreadStart(DrawIt));
                t1.Priority = ThreadPriority.Highest;
                t1.Name = $"DrawAll {i}";
                t1.Start(new Tuple<CountdownEvent, List<SystemNode>, int, int,string>(cd, nodes, nodes.Count / threads * i, nodes.Count / threads, outputdir));
            }

            cd.Wait();
            System.Diagnostics.Debug.WriteLine($"Draw {systemNodesByName.Count} systems for Cmdr {EDCommander.Current.Name} DONE DONE DONE");
        }

        static void DrawIt(Object o)
        {
            var control = (Tuple<CountdownEvent, List<SystemNode>, int, int,string>)o;

            int count = control.Item4;
            for (int i = control.Item3; count-- > 0; i++)
            {
                var sn = control.Item2[i];
                lock (sn)
                {
                    //System.Diagnostics.Debug.WriteLine($"Draw system {sn.System.Name} in thread");

                    string file = control.Item5 != null ? Path.Combine(control.Item5, sn.System.Name.SafeFileString()) + ".png" : null;

                    sn.DrawSystemToFile(file, 1920);

                }
            }
            control.Item1.Signal();
        }


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
            foreach (var mhe in mhs)
            {
                HistoryEntry he = mhe.Item2;

                if (he.journalEntry is IStarScan ss)
                {
                    if (he.journalEntry is JournalScan js)
                    {
                        System.Diagnostics.Debug.WriteLine($"\r\n{mhe.Item1} {he.EventTimeUTC} {he.EntryType}: `{js.BodyName}` ID: {js.BodyID} - {js.ParentList()};  in `{he.System.Name}`:{he.System.SystemAddress}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"\r\n{mhe.Item1} {he.EventTimeUTC} {he.EntryType}: in `{he.System.Name}`:{he.System.SystemAddress}");
                    }

                    (he.journalEntry as IStarScan).AddStarScan(this, he.System, he.Status);
                    perstep?.Invoke(this,mhe);
                    DumpTree();
                }
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

