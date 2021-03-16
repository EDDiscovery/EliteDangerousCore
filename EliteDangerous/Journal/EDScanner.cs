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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace EliteDangerousCore
{
    public class EDJournalUIScanner
    {
        public Action<JournalEntry> OnNewJournalEntry; 
        public Action<UIEvent> OnNewUIEvent;

        private Thread ScanThread;
        private ManualResetEvent StopRequested;
        private Action<Action> InvokeAsyncOnUiThread;
        private List<JournalMonitorWatcher> watchers = new List<JournalMonitorWatcher>();
        private List<StatusMonitorWatcher> statuswatchers = new List<StatusMonitorWatcher>();

        const int ScanTick = 100;       // tick time to check journals and status

        public EDJournalUIScanner(Action<Action> invokeAsyncOnUiThread)
        {
            InvokeAsyncOnUiThread = invokeAsyncOnUiThread;
        }

        #region Start stop and scan

        public void StartMonitor(bool storetodb)
        {
            StopRequested = new ManualResetEvent(false);
            ScanThread = new Thread(ScanThreadProc) { Name = "Journal Monitor Thread", IsBackground = true };
            ScanThread.Start();

            foreach (JournalMonitorWatcher mw in watchers)
            {
                mw.StartMonitor(storetodb);
            }

            foreach (StatusMonitorWatcher mw in statuswatchers)
            {
                mw.StartMonitor();
            }
        }

        public void StopMonitor()
        {
            foreach (JournalMonitorWatcher mw in watchers)
            {
                mw.StopMonitor();
            }

            foreach (StatusMonitorWatcher mw in statuswatchers)
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

                if (jlu != null && ( jlu.Item1.Count != 0 || jlu.Item2.Count != 0) && !stopRequested.WaitOne(0))
                {
                    InvokeAsyncOnUiThread(() => ScanTickDone(jlu));
                }
            }
        }

        private Tuple<List<JournalEntry>, List<UIEvent>> ScanTickWorker(Func<bool> stopRequested)     // read the entries from all watcher..
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

            return new Tuple<List<JournalEntry>, List<UIEvent>>(entries, uientries);
        }

        private void ScanTickDone(Tuple<List<JournalEntry>, List<UIEvent>> entries)       // in UI thread..
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

                        OnNewJournalEntry?.Invoke(ent);
                    }
                }

                foreach (var uient in entries.Item2)                    // pass them to the handler
                {
                    lock (stopRequested) // Make sure StopMonitor returns after this method returns
                    {
                        if (stopRequested.WaitOne(0))
                            return;

                        //System.Diagnostics.Trace.WriteLine(string.Format("New UI entry from journal {0} {1}", uient.EventTimeUTC, uient.EventTypeStr));

                        OnNewUIEvent?.Invoke(uient);
                    }
                }
            }
        }

        #endregion

        #region History refresh calls this for a set up of watchers.. then a global reparse of all journal event folders during load history

        // call to update/create watchers on joutnal and UI.  Do it with system stopped

        public int CheckAddPath(string p)           // return index of watcher (and therefore status watcher as we add in parallel)
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
                        JournalMonitorWatcher mw = new JournalMonitorWatcher(path);
                        watchers.Add(mw);

                        StatusMonitorWatcher sw = new StatusMonitorWatcher(path, ScanTick);
                        sw.UIEventCallBack += UIEvent;
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
            catch ( Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Watcher path exception" + ex);
            }

            return -1;
        }

        public void SetupWatchers(string [] stdfolders)
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            List<int> watchersinuse = new List<int>();
            foreach (string std in stdfolders)          // setup the std folders
            {
                watchersinuse.Add(CheckAddPath(std));
            }

            foreach ( var cmdr in EDCommander.GetListCommanders())       // setup any commander folders
            {
                if (!cmdr.ConsoleCommander && cmdr.JournalDir.HasChars())    // not console commanders, and we have a path
                {
                    watchersinuse.Add(CheckAddPath(cmdr.JournalDir));   // try adding
                }
            }

            List<int> del = new List<int>();
            for( int i = 0; i < watchers.Count; i++ )           // find any watchers not in the watchersinuse list
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
                StatusMonitorWatcher sw = statuswatchers[wi];
                sw.StopMonitor();          // just in case
                statuswatchers.Remove(sw);
            }
        }

        // Go thru all watchers and check to see if any new files have been found, if so, process them and either store to DB or fireback
        // options to force reload of last N files, to fireback instead of storing the last n

        public void ParseJournalFilesOnWatchers(Action<int, string> updateProgress, 
                                                int reloadlastn,
                                                Action<JournalEntry, int, int, int, int> firebacknostore = null, int firebacklastn = 0)
        {
            System.Diagnostics.Debug.Assert(ScanThread == null);        // double check we are not scanning.

            for (int i = 0; i < watchers.Count; i++)             // parse files of all folders being watched
            {
                // may create new commanders at the end, but won't need any new watchers, because they will obv be in the same folder
                var list = watchers[i].ScanJournalFiles(reloadlastn);    
                watchers[i].ProcessDetectedNewFiles(list, updateProgress, firebacknostore, firebacklastn );
            }
        }

        #endregion

        #region UI processing

        public void UIEvent(ConcurrentQueue<UIEvent> events, string folder)     // callback, in Thread.. from monitor
        {
            InvokeAsyncOnUiThread(() => UIEventPost(events));
        }

        public void UIEventPost(ConcurrentQueue<UIEvent> events)       // UI thread
        {
            ManualResetEvent stopRequested = StopRequested;

            Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            if (stopRequested != null)
            {
                while (!events.IsEmpty)
                {
                    lock (stopRequested) // Prevent StopMonitor from returning until this method has returned
                    {
                        if (stopRequested.WaitOne(0))
                            return;

                        UIEvent e;

                        if (events.TryDequeue(out e))
                        {
                            //System.Diagnostics.Trace.WriteLine(string.Format("New UI entry from status {0} {1}", e.EventTimeUTC, e.EventTypeStr));
                            OnNewUIEvent?.Invoke(e);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
