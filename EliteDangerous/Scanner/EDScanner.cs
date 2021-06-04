/*
 * Copyright © 2016-2021 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EliteDangerousCore
{
    // Scanner is above both the status and journal monitors
    // it sets them up and tears them down, and runs a thread which ticks and reads new journal/status events
    // then dispatches it thru the UI thread
    // it also has a function to rescan the journals of the watchers for the initial load.

    public class EDJournalUIScanner
    {
        public Action<JournalEntry> OnNewJournalEntry;
        public Action<UIEvent> OnNewUIEvent;

        private Thread ScanThread;
        private ManualResetEvent StopRequested;
        private Action<Action> InvokeAsyncOnUiThread;
        private List<JournalMonitorWatcher> watchers = new List<JournalMonitorWatcher>();
        private List<StatusReader> statuswatchers = new List<StatusReader>();

        const int ScanTick = 100;       // tick time to check journals and status

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

        // Journal scanner main tick - every tick, do scan tick worker, pass anything found to foreground for dispatch

        private void ScanThreadProc()
        {
            ManualResetEvent stopRequested = StopRequested;

            while (!stopRequested.WaitOne(ScanTick))
            {
                var jlu = ScanTickWorker(() => stopRequested.WaitOne(0));

                if (jlu != null && (jlu.Item1.Count != 0 || jlu.Item2.Count != 0) && !stopRequested.WaitOne(0))
                {
                    InvokeAsyncOnUiThread(() => IssueEvents(jlu));
                }
            }
        }

        private Tuple<List<JournalEntry>, List<UIEvent>> ScanTickWorker(Func<bool> stopRequested)     // read the entries from all watchers..
        {
            var entries = new List<JournalEntry>();
            var uientries = new List<UIEvent>();

            foreach (JournalMonitorWatcher mw in watchers)
            {
                var evret = mw.ScanForNewEntries();
                entries.AddRange(evret.Item1);
                uientries.AddRange(evret.Item2);

                if (stopRequested())
                {
                    return null;
                }
            }

            foreach (var sw in statuswatchers)
            {
                var evret = sw.Scan();
                uientries.AddRange(evret);

                if (stopRequested())
                {
                    return null;
                }
            }

            return new Tuple<List<JournalEntry>, List<UIEvent>>(entries, uientries);
        }

        private void IssueEvents(Tuple<List<JournalEntry>, List<UIEvent>> entries)       // in UI thread..
        {
            ManualResetEvent stopRequested = StopRequested;

            if (entries != null && stopRequested != null)
            {
                foreach (var ent in entries.Item1)                    // pass them to the handler
                {
                    lock (stopRequested) // Make sure StopMonitor returns after this method returns
                    {
                        if (stopRequested.WaitOne(0))
                            return;

                       // System.Diagnostics.Debug.WriteLine("Issue " + ent.EventTypeStr);
                        OnNewJournalEntry?.Invoke(ent);
                    }
                }

                foreach (var uient in entries.Item2)                    // pass them to the handler
                {
                    lock (stopRequested) // Make sure StopMonitor returns after this method returns
                    {
                        if (stopRequested.WaitOne(0))
                            return;

                      //  System.Diagnostics.Debug.WriteLine("Issue  UI" + uient.EventTypeStr);
                        OnNewUIEvent?.Invoke(uient);
                    }
                }
            }
        }

        #endregion

        #region History refresh calls this for a set up of watchers.

        // call to update/create watchers on joutnal and UI.  Do it with system stopped

        public void SetupWatchers(string[] stdfolders, string journalmatchpattern, DateTime mindateutc)
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            List<int> watchersinuse = new List<int>();          // may contain -1s for rejected paths

            foreach (string std in stdfolders)          // setup the std folders
            {
                watchersinuse.Add(CheckAddPath(std, journalmatchpattern, mindateutc));
            }

            foreach (var cmdr in EDCommander.GetListCommanders())       // setup any commander folders
            {
                if (!cmdr.ConsoleCommander && cmdr.JournalDir.HasChars())    // not console commanders, and we have a path
                {
                    watchersinuse.Add(CheckAddPath(cmdr.JournalDir,journalmatchpattern, mindateutc));   // try adding
                }
            }

            List<int> del = new List<int>();
            for (int i = 0; i < watchers.Count; i++)           // for all current watchers, are we still in use?
            {
                if (!watchersinuse.Contains(i))
                    del.Add(i);
            }

            for (int j = 0; j < del.Count; j++)
            {
                int wi = del[j];
                System.Diagnostics.Trace.WriteLine(string.Format("Delete watch on {0}", watchers[wi].WatcherFolder));
                JournalMonitorWatcher mw = watchers[wi];
                mw.StopMonitor();          // just in case
                watchers.Remove(mw);
                StatusReader sw = statuswatchers[wi];
                statuswatchers.Remove(sw);
            }
        }

        // check path exists, and if not already in watchers folder, add it
        // return -1 if not good, else return index of watcher (and therefore status watcher as we add in parallel)
        private int CheckAddPath(string p, string journalmatchpattern, DateTime mindateutc)         
        {
            try
            {
                string path = Path.GetFullPath(p);      // make path normalised. this can throw, so we need to protect and reject the path

                if (Directory.Exists(path))     // sanity check in case folder disappeared
                {
                    int present = watchers.FindIndex(x => x.WatcherFolder.Equals(path, StringComparison.InvariantCultureIgnoreCase));

                    if (present < 0)  // and we are not watching it, add it
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("New watch on {0}", path));
                        JournalMonitorWatcher mw = new JournalMonitorWatcher(path,journalmatchpattern, mindateutc);
                        watchers.Add(mw);

                        StatusReader sw = new StatusReader(path);
                        statuswatchers.Add(sw);

                        present = watchers.Count - 1;
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Existing watch on {0}", path));
                    }

                    return present;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Watcher path exception" + ex);
            }

            return -1;
        }

        #endregion

        #region History reload - scan of watchers for new files

        // Go thru all watchers and check to see if any new files have been found, if so, process them and either store to DB or fireback
        // options to force reload of last N files, to fireback instead of storing the last n

        public void ParseJournalFilesOnWatchers(Action<int, string> updateProgress,
                                                DateTime minjournaldateutc, int reloadlastn,
                                                Action<JournalEntry, int, int, int, int> firebacknostore = null, int firebacklastn = 0)
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            for (int i = 0; i < watchers.Count; i++)             // parse files of all folders being watched
            {
                // may create new commanders at the end, but won't need any new watchers, because they will obv be in the same folder
                var list = watchers[i].ScanJournalFiles(minjournaldateutc,reloadlastn);
                watchers[i].ProcessDetectedNewFiles(list, updateProgress, firebacknostore, firebacklastn);
            }
        }

        #endregion
    }
}
