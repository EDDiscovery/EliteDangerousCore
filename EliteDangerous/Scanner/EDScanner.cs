/*
 * Copyright 2016-2024 EDDiscovery development team
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

using EliteDangerousCore.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace EliteDangerousCore
{
    // EDJournalScanner is above both the status and journal monitors
    // it sets them up and tears them down, and runs a thread which ticks and reads new journal/status events
    // then dispatches using these three callbacks, in the UI thread
    // it also has a function to rescan the journals of the watchers for the initial load.
    // Scanner is setup by EDD controller

    public class EDJournalUIScanner
    {
        public Action<JournalEntry, StatusReader> OnNewJournalEntry;    // journal entries reported goind to the DB. Only a few events don't hit the DB now. There is no merge filtering etc
        public Action<UIEvent,StatusReader> OnNewUIEvent;               // ui events from status or journal entries converted into ui events

        private const int ScanTick = 100;                               // tick time to check journals and status

        public EDJournalUIScanner(Action<Action> invokeAsyncOnUiThread)
        {
            InvokeAsyncOnUiThread = invokeAsyncOnUiThread;
        }

        #region Start stop and scan

        // called to start monitoring all watchers.
        // Optional to store to db (EDDLite uses this)
        public void StartMonitor(bool storetodb)
        {
            StopRequested = new ManualResetEvent(false);

            foreach (JournalMonitorWatcher mw in journalwatchers)
            {
                mw.StartMonitor(storetodb);
            }

            ResetUIStatus();

            ScanThread = new Thread(ScanThreadProc) { Name = "Journal Monitor Thread", IsBackground = true };
            ScanThread.Start();
        }

        // called to stop monitoring all watchers.  Can double stop
        public void StopMonitor()
        {
            foreach (JournalMonitorWatcher mw in journalwatchers)
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

        // call to force reset of ui status so it will emit the full set on restart
        public void ResetUIStatus()
        {
            foreach (var e in statuswatchers)
            {
                e.Reset();
            }
        }

        #endregion

        #region Single Stepper

        public class SingleStepper
        {
            public SingleStepper(JournalEntry startpos)
            {
                lastjid = startpos.Id;
                cmdrid = startpos.CommanderId;
            }

            public TravelLogUnit tlu;
            public long lastjid;
            public int cmdrid;
            public EDJournalReader journalreader;
            public JournalEvents.JournalContinued lastcontinued;
        }

        // single stepping, the monitor is not running,
        // given the last journal entry, find the next one after that and pass thru the same process as if it was read from a file
        // true if stepped okay

        public bool SingleStep(SingleStepper stepper)
        {
            JournalEntry next = JournalEntry.GetNext(stepper.lastjid, stepper.cmdrid);

            if ( next != null )
            {
                if ( TravelLogUnit.TryGet(next.TLUId, out TravelLogUnit nexttlu) )
                {
                    stepper.lastjid = next.Id;
                    if (stepper.tlu != nexttlu || stepper.journalreader == null)
                    {
                        stepper.journalreader = new EDJournalReader(nexttlu);
                        stepper.tlu = nexttlu;
                        System.Diagnostics.Debug.WriteLine($"Single step change to TLU {stepper.tlu.FileName}");
                    }

                    List<JournalEntry> jevents = new List<JournalEntry>();
                    List<UIEvent> uievents = new List<UIEvent>();

                    // turn the JSON into journal/ui events, in batch mode
                    stepper.journalreader.ProcessLineIntoEvents(next.GetJson().ToString(), jevents, uievents, true, ref stepper.lastcontinued);

                    var sw = statuswatchers[0];     // bodge for now

                    var events = new List<Events>();
                    foreach (var ev in jevents)
                    {
                        events.Add(new Events() { je = ev, sr = sw }); // feed an event in
                       // System.Diagnostics.Debug.WriteLine($"Single step issue {ev.EventTypeID}");
                    }
                    foreach (var ev in uievents)
                    {
                        events.Add(new Events() { ui = ev, sr = sw });   // find in an ui event
                      //  System.Diagnostics.Debug.WriteLine($"Single step issue UI JE {ev.EventTypeID}");
                    }

                    if (events.Count > 0)
                    {
                        InvokeAsyncOnUiThread(() => IssueEventsInUIThread(events));
                    }

                    return true;
                }
            }
            
            return false;
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
            for (int i = 0; i < journalwatchers.Count; i++)           // for all current watchers, are we still in use?
            {
                var exists = folderlist.Find(x => x.Item1.Equals(journalwatchers[i].Folder, StringComparison.CurrentCultureIgnoreCase));

                if (exists == null || journalwatchers[i].IncludeSubfolders != exists.Item2 )       // if not in list, or in list but sub folders are different..
                    del.Add(i);    
            }

            for (int j = 0; j < del.Count; j++)     // remove any unused watchers
            {
                int wi = del[j];
                JournalMonitorWatcher mw = journalwatchers[wi];
                //System.Diagnostics.Debug.WriteLine($"Monitor Remove {mw.WatcherFolder} incl {mw.IncludeSubfolders}");
                mw.StopMonitor();          // just in case
                journalwatchers.Remove(mw);
                StatusReader sw = statuswatchers[wi];
                statuswatchers.Remove(sw);
            }

            foreach (var tw in folderlist)      // make sure all folders are watched
            {
                var exists = journalwatchers.Find(x => x.Folder.Equals(tw.Item1, StringComparison.InvariantCultureIgnoreCase));
                if (exists == null)     // if not watching in list, add
                {
                    try
                    {
                        JournalMonitorWatcher mw = new JournalMonitorWatcher(tw.Item1, journalmatchpattern, mindateutc, tw.Item2);
                        journalwatchers.Add(mw);

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

            for (int i = 0; i < journalwatchers.Count; i++)             // parse files of all folders being watched
            {
                // may create new commanders at the end, but won't need any new watchers, because they will obv be in the same folder
                
                //System.Diagnostics.Debug.WriteLine($"Parse scan {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
                var list = journalwatchers[i].ScanJournalFiles(minjournaldateutc,reloadlastn);
                
                //System.Diagnostics.Debug.WriteLine($"Parse process {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
                journalwatchers[i].ProcessDetectedNewFiles(list, updateProgress, jeslist, closerequested);
                //System.Diagnostics.Debug.WriteLine($"Parse Process Finished {BaseUtils.AppTicks.TickCountLap("LL")} {watchers[i].Folder}");
            }
        }

        #endregion

        #region Tick worker

        // Journal and UI scanner dispatch thread.
        // do scan tick worker at ScanTick rate, pass anything found to foreground for dispatch
        private void ScanThreadProc()
        {
            ManualResetEvent stopRequested = StopRequested;

            while (!stopRequested.WaitOne(ScanTick))        // ScanTick sets the pace of this thread firing
            {
                var eventlist = GatherEvents(() => stopRequested.WaitOne(0));

                if (eventlist != null && eventlist.Count > 0 && !stopRequested.WaitOne(0))
                {
                    InvokeAsyncOnUiThread(() => IssueEventsInUIThread(eventlist));
                }
            }
        }

        private class Events
        {
            public JournalEntry je;
            public UIEvent ui;
            public StatusReader sr;
        }

        // read the entries from all watchers, journal and UI
        private List<Events> GatherEvents(Func<bool> stopRequested)     
        {
            var events = new List<Events>();

            for (int i = 0; i < journalwatchers.Count; i++)
            {
                var mw = journalwatchers[i];                    // watchers/statuswatchers come in pairs
                var sw = statuswatchers[i];

                var evret = mw.ScanForNewEntries();             // return tuple of list of journal events

                // split them into separate Event

                foreach (var ev in evret.Item1)
                {
                    events.Add(new Events() { je = ev, sr = sw }); // feed an event in
                }
                foreach (var ev in evret.Item2)
                {
                    events.Add(new Events() { ui = ev, sr = sw });   // find in an ui event
                }

                var uiret = sw.Scan();      // return list of ui events
                foreach (var ev in uiret)
                {
                    events.Add(new Events() { ui = ev, sr = sw });
                }

                if (stopRequested())
                {
                    return null;
                }
            }

            return events;
        }

        // in UI thread.. fire the events off
        private void IssueEventsInUIThread(List<Events> entries)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            foreach (var e in entries)
            {
                if (e.je != null)
                    OnNewJournalEntry?.Invoke(e.je, e.sr);
                if (e.ui != null)
                    OnNewUIEvent?.Invoke(e.ui, e.sr);
            }
        }

        #endregion

        #region Vars

        private Thread ScanThread;
        private ManualResetEvent StopRequested;
        private Action<Action> InvokeAsyncOnUiThread;
        private List<JournalMonitorWatcher> journalwatchers = new List<JournalMonitorWatcher>();
        private List<StatusReader> statuswatchers = new List<StatusReader>();


        #endregion
    }
}

