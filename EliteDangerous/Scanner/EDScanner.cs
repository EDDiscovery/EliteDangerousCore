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
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BaseUtils;

namespace EliteDangerousCore
{
    // EDJournalScanner is above both the status and journal monitors
    // it sets them up and tears them down, and runs a thread which ticks and reads new journal/status events
    // then dispatches using these three callbacks, in the UI thread
    // it also has a function to rescan the journals of the watchers for the initial load.
    // Scanner is setup by EDD controller

    public class EDJournalUIScanner
    {
        public Action<JournalEntry, StatusReader> OnNewRawJournalEntry;     // all journal entries before filtering
        public Action<JournalEntry, StatusReader> OnNewFilteredJournalEntry;    // journal entries filtered out - all doing to the DB
        public Action<UIEvent,StatusReader> OnNewUIEvent;       // ui events from status or journal entries filtered into ui events

        public EDJournalUIScanner(Action<Action> invokeAsyncOnUiThread)
        {
            InvokeAsyncOnUiThread = invokeAsyncOnUiThread;
        }

        #region Start stop and scan

        public void StartMonitor(bool storetodb)
        {
            StopRequested = new ManualResetEvent(false);

            foreach (JournalMonitorWatcher mw in watchers)
            {
                mw.StartMonitor(storetodb);
            }

            ResetUIStatus();

            ScanThread = new Thread(ScanThreadProc) { Name = "Journal Monitor Thread", IsBackground = true };
            ScanThread.Start();
        }

        public void StopMonitor()
        {
            foreach (JournalMonitorWatcher mw in watchers)
            {
                mw.StopMonitor();
            }

            if (StopRequested != null)
            {
                lock (StopRequested) // Wait for ScanTickDone
                {
                    StopRequested.Set();
                }
            }

            if (ScanThread != null)
            {
                ScanThread.Join();
                ScanThread = null;
            }
        }

        // call to force reset of ui status
        public void ResetUIStatus()
        {
            foreach (var e in statuswatchers)
            {
                e.Reset();
            }
        }

        // Journal scanner main tick - every tick, do scan tick worker, pass anything found to foreground for dispatch

        private void ScanThreadProc()
        {
            ManualResetEvent stopRequested = StopRequested;

            while (!stopRequested.WaitOne(ScanTick))
            {
                var jlu = ScanTickWorker(() => stopRequested.WaitOne(0));

                if (jlu != null && jlu.Count>0 && !stopRequested.WaitOne(0))
                {
                    InvokeAsyncOnUiThread(() => IssueEventsInUIThread(jlu));
                }
            }
        }

        private class Event
        {
            public JournalEntry unfilteredje;  // holds either an unfiltered je, a filtered je, or an ui, plus its associated sr
            public JournalEntry je;        
            public UIEvent ui;
            public StatusReader sr;
        }

        private List<Event> ScanTickWorker(Func<bool> stopRequested)     // read the entries from all watchers..
        {
            var events = new List<Event>();

            for (int i = 0; i < watchers.Count; i++)
            {
                var mw = watchers[i];                           // watchers/statuswatchers come in pairs
                var sw = statuswatchers[i];

                var evret = mw.ScanForNewEntries();             // return tuple of list of journal events

                // split them into separate Event

                foreach (var ev in evret.Item1)
                {
                    events.Add(new Event() { unfilteredje = ev, sr = sw }); // feed an unfiltered event in
                }
                foreach (var ev in evret.Item2)
                {
                    events.Add(new Event() { je = ev, sr = sw }); // feed an event in
                }
                foreach (var ev in evret.Item3)
                {
                    events.Add(new Event() { ui = ev, sr = sw });   // find in an ui event
                }

                var uiret = sw.Scan();      // return list of ui events
                foreach (var ev in uiret)
                {
                    events.Add(new Event() { ui = ev, sr = sw });
                }

                if (stopRequested())
                {
                    return null;
                }
            }

            return events;
        }

        // in UI thread.. fire the events off
        private void IssueEventsInUIThread(List<Event> entries)       
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            foreach (var e in entries)
            {
                if (e.unfilteredje != null)
                    OnNewRawJournalEntry?.Invoke(e.unfilteredje, e.sr);
                if (e.je != null)
                    OnNewFilteredJournalEntry?.Invoke(e.je, e.sr);
                if (e.ui != null)
                    OnNewUIEvent?.Invoke(e.ui, e.sr);
                if (StopRequested.WaitOne(0))
                    return;
            }
        }

        #endregion

        #region History refresh calls this for a set up of watchers.

        // call to update/create watchers on joutnal and UI.  Do it with system stopped

        public void SetupWatchers(string[] stdfolders, string journalmatchpattern, DateTime mindateutc)
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            List<Tuple<string, bool>> folderlist = new List<Tuple<string, bool>>();

            foreach (string std in stdfolders)          // setup the std folders
            {
                var path = Path.GetFullPath(std).TrimEnd(Path.DirectorySeparatorChar);  // nov 24 ensure no trailing /

                if (Directory.Exists(path))             // make sure the bugger exists
                {
                    folderlist.Add(new Tuple<string, bool>(path, false));        // std folders are not sub folder scanned
                }
                else
                    System.Diagnostics.Trace.WriteLine($"Scanner Std Folder {path} does not exist");
            }

            foreach (var cmdr in EDCommander.GetListActiveCommanders())       // setup any commander folders first, so the watches are established with subfolder search
            {
                if (!cmdr.ConsoleCommander && cmdr.JournalDir.HasChars())
                {
                    var path = Path.GetFullPath(cmdr.JournalDir).TrimEnd(Path.DirectorySeparatorChar); // nov 24 ensure no trailing /

                    if (Directory.Exists(path))
                    {
                        var tp = folderlist.Find(x => x.Item1.Equals(cmdr.JournalDir, StringComparison.InvariantCultureIgnoreCase));

                        // if we have a previous one without subfolders, but we want subfolders, remove
                        if (tp != null && cmdr.IncludeSubFolders == true && tp.Item2 == false)      
                        {
                            folderlist.Remove(tp);
                            tp = null;
                        }

                        // and add if we don't have this folder
                        if (tp == null)
                            folderlist.Add(new Tuple<string, bool>(path, cmdr.IncludeSubFolders));
                    }
                    else
                        System.Diagnostics.Trace.WriteLine($"Scanner Cmdr Folder {path} does not exist");

                }
            }

            //foreach (var x in folderlist)  System.Diagnostics.Debug.WriteLine($"Monitor Folder {x.Item1} incl {x.Item2}");

            List<int> del = new List<int>();
            for (int i = 0; i < watchers.Count; i++)           // for all current watchers, are we still in use?
            {
                var exists = folderlist.Find(x => x.Item1.Equals(watchers[i].Folder, StringComparison.CurrentCultureIgnoreCase));

                if (exists == null || watchers[i].IncludeSubfolders != exists.Item2 )       // if not in list, or in list but sub folders are different..
                    del.Add(i);    
            }

            for (int j = 0; j < del.Count; j++)     // remove any unused watchers
            {
                int wi = del[j];
                JournalMonitorWatcher mw = watchers[wi];
                //System.Diagnostics.Debug.WriteLine($"Monitor Remove {mw.WatcherFolder} incl {mw.IncludeSubfolders}");
                mw.StopMonitor();          // just in case
                watchers.Remove(mw);
                StatusReader sw = statuswatchers[wi];
                statuswatchers.Remove(sw);
            }

            foreach (var tw in folderlist)      // make sure all folders are watched
            {
                var exists = watchers.Find(x => x.Folder.Equals(tw.Item1, StringComparison.InvariantCultureIgnoreCase));
                if (exists == null)     // if not watching in list, add
                {
                    try
                    {
                        JournalMonitorWatcher mw = new JournalMonitorWatcher(tw.Item1, journalmatchpattern, mindateutc, tw.Item2);
                        watchers.Add(mw);

                        StatusReader sw = new StatusReader(tw.Item1);
                        statuswatchers.Add(sw);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Watcher path exception {exists.Folder} {ex}");
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"Monitor existing watch {exists.WatcherFolder} incl {exists.IncludeSubfolders}");
                }
            }
        }


        #endregion

        #region History reload - scan of watchers for new files

        // Go thru all watchers and check to see if any new files have been found, if so, process them and either store to DB return a list of them (if jes != null)
        // options to force reload of last N files, to fireback instead of storing the last n

        public void ParseJournalFilesOnWatchers(Action<int, string> updateProgress, CancellationToken closerequested,
                                                DateTime minjournaldateutc, int reloadlastn, List<JournalEntry> jeslist = null
                                                )
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            List<JournalEntry> accumulatedjes = new List<JournalEntry>();

            //System.Diagnostics.Debug.WriteLine($"Parse Start {BaseUtils.AppTicks.TickCountLap("LL",true)}");

            for (int i = 0; i < watchers.Count; i++)             // parse files of all folders being watched
            {
                // may create new commanders at the end, but won't need any new watchers, because they will obv be in the same folder
                
                //System.Diagnostics.Debug.WriteLine($"Parse scan {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
                var list = watchers[i].ScanJournalFiles(minjournaldateutc,reloadlastn);
                
                //System.Diagnostics.Debug.WriteLine($"Parse process {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
                watchers[i].ProcessDetectedNewFiles(list, updateProgress, jeslist, closerequested);
                //System.Diagnostics.Debug.WriteLine($"Parse Process Finished {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
            }
        }

        #endregion

        #region vars

        private Thread ScanThread;
        private ManualResetEvent StopRequested;
        private Action<Action> InvokeAsyncOnUiThread;
        private List<JournalMonitorWatcher> watchers = new List<JournalMonitorWatcher>();
        private List<StatusReader> statuswatchers = new List<StatusReader>();

        const int ScanTick = 100;       // tick time to check journals and status


        #endregion
    }
}
