/*
 * Copyright © 2016-2024 EDDiscovery development team
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;

namespace EliteDangerousCore
{
    // watches a journal for changes, reads it, 

    [System.Diagnostics.DebuggerDisplay("{Folder} {journalfilematch}")]
    public class JournalMonitorWatcher
    {
        public string Folder { get; set; }
        public bool IncludeSubfolders;

        public JournalMonitorWatcher(string folder, string journalmatchpattern, DateTime mindateutcp, bool pincludesubfolders)
        {
            Folder = folder;
            journalfilematch = journalmatchpattern;
            mindateutc = mindateutcp;
            IncludeSubfolders = pincludesubfolders;
            //System.Diagnostics.Debug.WriteLine($"Monitor new watch {Folder} incl {IncludeSubfolders} match {journalfilematch} date {mindateutc} ");
        }

        #region Scan start stop and monitor

        public void StartMonitor(bool storetodb)
        {
            if (m_Watcher == null)
            {
                try
                {

                    StoreToDBDuringUpdateRead = storetodb;
                    m_netLogFileQueue = new ConcurrentQueue<string>();
                    m_Watcher = new System.IO.FileSystemWatcher();
                    m_Watcher.Path = Folder + Path.DirectorySeparatorChar;
                    m_Watcher.Filter = journalfilematch;
                    m_Watcher.IncludeSubdirectories = IncludeSubfolders;
                    m_Watcher.NotifyFilter = NotifyFilters.FileName;
                    m_Watcher.Changed += new FileSystemEventHandler(OnNewFile);
                    m_Watcher.Created += new FileSystemEventHandler(OnNewFile);
                    m_Watcher.EnableRaisingEvents = true;

                    System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCount} Start Monitor on {Folder} incl {IncludeSubfolders}");
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Start Monitor exception : " + ex.Message, "EDDiscovery Error");
                    System.Diagnostics.Trace.WriteLine("Start Monitor exception : " + ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                }
            }
        }

        public void StopMonitor()
        {
            if (m_Watcher != null)
            {
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Dispose();
                m_Watcher = null;

                System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCount} Stop Monitor on {Folder} incl {IncludeSubfolders}");
            }
        }

        // OS calls this when new file is available, add to list

        private void OnNewFile(object sender, FileSystemEventArgs e)        // only picks up new files
        {                                                                   // and it can kick in before any data has had time to be written to it...
            string filename = Path.IsPathRooted(e.Name) ? e.Name : e.FullPath; // Work around https://github.com/mono/mono/issues/21677
            m_netLogFileQueue.Enqueue(filename);
        }


        // Called by EDScanner periodically to scan for journal entries
        // returns unfiltered jevents, filtered jevents, uievents

        public Tuple<List<JournalEntry>, List<JournalEntry>, List<UIEvent>> ScanForNewEntries()
        {
            var unfilteredentries = new List<JournalEntry>();
            var filteredentries = new List<JournalEntry>();
            var uientries = new List<UIEvent>();

            string filename = null;

            if (lastnfi != null)                            // always give old file another go, even if we are going to change
            {
                if (!File.Exists(lastnfi.FullName))         // if its been removed, null
                {
                    lastnfi = null;
                }
                else
                {
                    //var notdone = new FileInfo(lastnfi.FullName).Length != lastnfi.Pos ? "**********" : ""; System.Diagnostics.Debug.WriteLine($"Scan last nfi {lastnfi.FullName} from {lastnfi.Pos} Length file is {new FileInfo(lastnfi.FullName).Length} {notdone} ");

                    ScanReader(unfilteredentries,filteredentries, uientries);

                    if (unfilteredentries.Count > 0 || filteredentries.Count > 0 || uientries.Count > 0)
                    {
                       // System.Diagnostics.Debug.WriteLine("ScanFornew read " + entries.Count() + " ui " + uientries.Count());
                        ticksNoActivity = 0;
                        return new Tuple<List<JournalEntry>, List<JournalEntry>, List<UIEvent>>(unfilteredentries, filteredentries, uientries);     // feed back now don't change file
                    }
                }
            }

            if (m_netLogFileQueue.TryDequeue(out filename))      // if a new one queued, we swap to using it
            {
                lastnfi = OpenFileReader(filename);
                System.Diagnostics.Debug.WriteLine(string.Format("Change to scan {0}", lastnfi.FullName));
                ScanReader(unfilteredentries, filteredentries, uientries);
            }
            else if (ticksNoActivity >= 30 && (lastnfi == null || lastnfi.Pos >= new FileInfo(lastnfi.FullName).Length))
            {
                // every few goes, if its not there or filepos is greater equal to length (so only done when fully up to date)
                // scan all files in the folder, pick out any new logs, and process the first that is found.

                try
                {
                    HashSet<string> tlunames = new HashSet<string>(TravelLogUnit.GetAllNames());
                    string[] filenames = Directory.EnumerateFiles(Folder, journalfilematch, IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                                    .Select(s => new { name = Path.GetFileName(s), fullname = s })
                                                    .Where(s => !tlunames.Contains(s.fullname))           // find any new ones..
                                                    .Where(g => new FileInfo(g.fullname).LastWriteTime >= mindateutc)
                                                    .OrderBy(s => s.name)
                                                    .Select(s => s.fullname)
                                                    .ToArray();

                    if (filenames.Length > 0)
                    {
                        lastnfi = OpenFileReader(filenames[0]);     // open first one
                        System.Diagnostics.Debug.WriteLine(string.Format("Found new file {0}", lastnfi.FullName));
                        ScanReader(unfilteredentries, filteredentries, uientries);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception during monitor watch file found " + ex);
                }
                finally
                {
                    ticksNoActivity = 0;
                }
            }

            ticksNoActivity++;

            return new Tuple<List<JournalEntry>, List<JournalEntry>, List<UIEvent>>(unfilteredentries,filteredentries, uientries);     // feed back now don't change file
        }

        // Called by ScanForNewEntries (from EDJournalClass Scan Tick Worker) to scan a NFI for new entries

        private void ScanReader(List<JournalEntry> unfilteredentries,List<JournalEntry> filteredentries, List<UIEvent> uientries)
        {
            System.Diagnostics.Debug.Assert(lastnfi.ID != 0);       // must have committed it at this point, prev code checked for it but it must have been done
            System.Diagnostics.Debug.Assert(netlogreaders.ContainsKey(lastnfi.FullName));       // must have added to netlogreaders.. double check

            bool readanything = lastnfi.ReadJournal(unfilteredentries,filteredentries, uientries, historyrefreshparsing: false);

            if (StoreToDBDuringUpdateRead)
            {
                if (filteredentries.Count > 0 || readanything )
                {
                    UserDatabase.Instance.DBWrite(cn =>
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        using (DbTransaction txn = cn.BeginTransaction())
                        {
                            if (filteredentries.Count > 0)
                            {
                                filteredentries = filteredentries.Where(jre => JournalEntry.FindEntry(jre, cn, jre.GetJson(cn)).Count == 0).ToList();

                                foreach (JournalEntry jre in filteredentries)
                                {
                                    var json = jre.GetJson(cn);
                                    jre.Add(json,cn,txn);
                                }
                            }

                            lastnfi.TravelLogUnit.Update(cn);       // update TLU pos

                            txn.Commit();
                        }

                        if (sw.ElapsedMilliseconds >= 50)        // this is written to the log to try and debug bad DB behaviour
                        {
                            System.Diagnostics.Trace.WriteLine($"Warning access to DB to write new journal entries slow {sw.ElapsedMilliseconds} for {filteredentries.Count}");
                            //foreach (var e in entries)   System.Diagnostics.Trace.WriteLine(".." + e.EventTimeUTC + " " + e.EventTypeStr);
                        }

                    });
                }
            }
            else
            {
                if (readanything)
                    lastnfi.TravelLogUnit.Update();
            }

            //if ( entries.Count()>0 || uientries.Count()>0) System.Diagnostics.Debug.WriteLine("ScanRead " + entries.Count() + " " + uientries.Count());
        }

        #endregion

        #region Called during history refresh, by EDJournalClass, for a reparse.

        // look thru all files in the location, in date ascending order, work out if we need to scan them, assemble a list of JournalReaders representing one
        // file to parse.  We can order the reload of the last N files

        public List<EDJournalReader> ScanJournalFiles(DateTime minjournaldateutc, int reloadlastn)
        {

            // order by file write time so we end up on the last one written
            // EnumerateFile preserves the casing of Folder and adds on known files below it preserving case
            // do not ToLower filenames due to linux
            FileInfo[] allFiles = Directory.EnumerateFiles(Folder, journalfilematch, IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).
                                        Select(f => new FileInfo(f)).Where(g=>g.LastWriteTime >= minjournaldateutc).OrderBy(p => p.LastWriteTime).ToArray();
            
            System.Diagnostics.Debug.WriteLine($"ScanJournalFiles {Folder}  {journalfilematch} Found {allFiles.Length}");

            List<EDJournalReader> readersToUpdate = new List<EDJournalReader>();

            List<TravelLogUnit> tlutoadd = new List<TravelLogUnit>();

            for (int i = 0; i < allFiles.Length; i++)
            {
                FileInfo fi = allFiles[i];

                var reader = OpenFileReader(allFiles[i].FullName, true);       // open it, but delay adding

                if (reader.ID == 0)            // if not present, add to commit add list
                {
                    tlutoadd.Add(reader.TravelLogUnit);
                }

                if ( i >= allFiles.Length - reloadlastn )
                {
                    reader.Pos = 0;      // by setting the start zero (reader.filePos is the same as Size)
                }

                bool islast = (i == allFiles.Length - 1);

                if (reader.Pos != fi.Length || islast )  // File not already in DB, or is the last one
                {
                    readersToUpdate.Add(reader);
                }
            }


            if ( tlutoadd.Count > 0 )                      // now, on spinning rust, this takes ages for 600+ log files first time, so transaction it
            {
                System.Diagnostics.Debug.WriteLine($"ScanJournalFiles Commit {tlutoadd.Count} TLUs");

                UserDatabase.Instance.DBWrite(cn => 
                    {
                        using (DbTransaction txn = cn.BeginTransaction())
                        {
                            foreach (var tlu in tlutoadd)
                            {
                                tlu.Add(cn,txn);
                            }

                            txn.Commit();
                        }
                    });
            }

            return readersToUpdate;
        }

        // given a list of files to reparse, either return a list of them (if jes != null), or store them in the db
        public void ProcessDetectedNewFiles(List<EDJournalReader> readersToUpdate,  Action<int, string> updateProgress, List<JournalEntry> jeslist,CancellationToken cancel)
        {
            for (int i = 0; i < readersToUpdate.Count; i++)
            {
                if (cancel.IsCancellationRequested)
                    break;

                EDJournalReader reader = readersToUpdate[i];

                //System.Diagnostics.Debug.WriteLine($"Processing {reader.FullName}");

                List<JournalEntry> unfilteredwasted = new List<JournalEntry>();     // we don't care about the raw unfiltered ones
                List<JournalEntry> dbentries = new List<JournalEntry>();            // entries found for db
                List<UIEvent> uieventswasted = new List<UIEvent>();                 // any UI events converted are wasted here

                bool readanything = reader.ReadJournal(unfilteredwasted, jeslist != null ? jeslist : dbentries, uieventswasted, historyrefreshparsing: true);      // this may create new commanders, and may write (but not commit) to the TLU
                    
                if ( jeslist != null )                  // if updating jes list
                {
                    if (readanything)               // need to update TLU pos if read anything
                        reader.TravelLogUnit.Update();
                }
                else
                {
                    UserDatabase.Instance.DBWrite(cn =>
                    {
                        // only lookup TLUs if there is actually anything to compare against
                        ILookup<DateTime, JournalEntry> existing = dbentries.Count > 0 ? JournalEntry.GetAllByTLU(reader.ID, cn).ToLookup(e => e.EventTimeUTC) : null;

                        //System.Diagnostics.Trace.WriteLine(BaseUtils.AppTicks.TickCountLap("PJF"), i + " into db");

                        using (DbTransaction txn = cn.BeginTransaction())
                        {
                            foreach (JournalEntry jre in dbentries)
                            {
                                //System.Diagnostics.Trace.WriteLine(string.Format("--- Check {0} {1} Existing {2} : {3}", jre.EventTimeUTC, jre.EventTypeStr, existing[jre.EventTimeUTC].Count(), jre.GetJson().ToString()));

                                if (!existing[jre.EventTimeUTC].Any(e => JournalEntry.AreSameEntry(jre, e, cn, ent1jo: jre.GetJson(cn))))
                                {
                                    //foreach (var x in existing[jre.EventTimeUTC]) { System.Diagnostics.Trace.WriteLine(string.Format(" passed vs {0} Deepequals {1}", x.GetJson().ToString(), x.GetJson().DeepEquals(jre.GetJson()))); }

                                    QuickJSON.JObject jo = jre.GetJson(cn);
                                    jre.Add(jo, cn, txn);

                                    //System.Diagnostics.Trace.WriteLine(string.Format("Write Journal to db {0} {1}", jre.EventTimeUTC, jre.EventTypeStr));
                                }
                                else
                                {
                                    //System.Diagnostics.Trace.WriteLine(string.Format("Duplicate Journal to db {0} {1}", jre.EventTimeUTC, jre.EventTypeStr));
                                }
                            }

                            if (readanything)
                                reader.TravelLogUnit.Update(cn);

                            txn.Commit();
                        }
                    });
                }

                updateProgress((i + 1) * 100 / readersToUpdate.Count, reader.FullName);

                lastnfi = reader;
            }

            updateProgress(-1, "");
        }

        #endregion

        #region Open

        // open a new file for watching, place it into the netlogreaders list. Always return a reader
        // delayadd will hold the DB write
        private EDJournalReader OpenFileReader(string filepath, bool delayadd = false)
        {
            EDJournalReader reader;

            if (netlogreaders.ContainsKey(filepath))        //  if we already have a net log reader
            {
                reader = netlogreaders[filepath];
            }
            else if (TravelLogUnit.TryGet(filepath, out TravelLogUnit tlu))   // if we have a TLU in the db, we can make one from that
            { 
                System.Diagnostics.Debug.Assert(tlu != null);
                reader = new EDJournalReader(tlu);
                System.Diagnostics.Debug.Assert(reader != null);
                netlogreaders[filepath] = reader;
                //System.Diagnostics.Trace.WriteLine($"Scanner {Folder}:{IncludeSubfolders} Add to its netlogreaders {filepath}");
            }
            else
            {
                // new one to netlogreaders and optionally store in cache
                reader = new EDJournalReader(filepath);
                
                reader.TravelLogUnit.Type = TravelLogUnit.JournalType;
                var filename = Path.GetFileName(filepath);
                if (filename.StartsWith("JournalBeta.", StringComparison.InvariantCultureIgnoreCase) ||
                    filename.StartsWith("JournalGamma.", StringComparison.InvariantCultureIgnoreCase) ||
                    filename.StartsWith("JournalAlpha.", StringComparison.InvariantCultureIgnoreCase))
                {
                    reader.TravelLogUnit.Type |= TravelLogUnit.BetaMarker;
                }

                if (!delayadd)                      // if we add immediately, add to db
                    reader.TravelLogUnit.Add();

                netlogreaders[filepath] = reader;

                //System.Diagnostics.Trace.WriteLine($"Create New TLU in DB {filepath}");
            }

            return reader;
        }

        #endregion

        #region Vars

        // key is file name in original case
        private Dictionary<string, EDJournalReader> netlogreaders = new Dictionary<string, EDJournalReader>();

        private EDJournalReader lastnfi = null;          // last one read..
        private FileSystemWatcher m_Watcher;
        private int ticksNoActivity = 0;
        private ConcurrentQueue<string> m_netLogFileQueue;
        private bool StoreToDBDuringUpdateRead = false;
        private string journalfilematch;
        private DateTime mindateutc;

        #endregion
    }
}
