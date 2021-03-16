/*
 * Copyright © 2016-2020 EDDiscovery development team
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

using EliteDangerousCore.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace EliteDangerousCore
{
    // watches a journal for changes, reads it, 

    [System.Diagnostics.DebuggerDisplay("{WatcherFolder}")]
    public class JournalMonitorWatcher
    {
        public string WatcherFolder { get; set; }

        private Dictionary<string, EDJournalReader> netlogreaders = new Dictionary<string, EDJournalReader>();
        private EDJournalReader lastnfi = null;          // last one read..
        private FileSystemWatcher m_Watcher;
        private int ticksNoActivity = 0;
        private ConcurrentQueue<string> m_netLogFileQueue;
        private const string journalfilematch = "Journal*.log";       // this picks up beta and normal logs
        private bool StoreToDBDuringUpdateRead = false;

        public JournalMonitorWatcher(string folder)
        {
            WatcherFolder = folder;
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
                    m_Watcher.Path = WatcherFolder + Path.DirectorySeparatorChar;
                    m_Watcher.Filter = journalfilematch;
                    m_Watcher.IncludeSubdirectories = false;
                    m_Watcher.NotifyFilter = NotifyFilters.FileName;
                    m_Watcher.Changed += new FileSystemEventHandler(OnNewFile);
                    m_Watcher.Created += new FileSystemEventHandler(OnNewFile);
                    m_Watcher.EnableRaisingEvents = true;

                    System.Diagnostics.Trace.WriteLine("Start Monitor on " + WatcherFolder);
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

                System.Diagnostics.Trace.WriteLine("Stop Monitor on " + WatcherFolder);
            }
        }

        // OS calls this when new file is available, add to list

        private void OnNewFile(object sender, FileSystemEventArgs e)        // only picks up new files
        {                                                                   // and it can kick in before any data has had time to be written to it...
            string filename = e.FullPath;
            m_netLogFileQueue.Enqueue(filename);
        }


        // Called by EDJournalClass periodically to scan for journal entries

        public Tuple<List<JournalEntry>, List<UIEvent>> ScanForNewEntries()
        {
            var entries = new List<JournalEntry>();
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

                    ScanReader(entries, uientries);

                    if (entries.Count > 0 || uientries.Count > 0)
                    {
                       // System.Diagnostics.Debug.WriteLine("ScanFornew read " + entries.Count() + " ui " + uientries.Count());
                        ticksNoActivity = 0;
                        return new Tuple<List<JournalEntry>, List<UIEvent>>(entries, uientries);     // feed back now don't change file
                    }
                }
            }

            if (m_netLogFileQueue.TryDequeue(out filename))      // if a new one queued, we swap to using it
            {
                lastnfi = OpenFileReader(filename);
                System.Diagnostics.Debug.WriteLine(string.Format("Change to scan {0}", lastnfi.FullName));
                ScanReader(entries, uientries);
            }
            else if (ticksNoActivity >= 30 && (lastnfi == null || lastnfi.Pos >= new FileInfo(lastnfi.FullName).Length))
            {
                // every few goes, if its not there or filepos is greater equal to length (so only done when fully up to date)
                // scan all files in the folder, pick out any new logs, and process the first that is found.

                try
                {
                    HashSet<string> tlunames = new HashSet<string>(TravelLogUnit.GetAllNames());
                    string[] filenames = Directory.EnumerateFiles(WatcherFolder, journalfilematch, SearchOption.AllDirectories)
                                                    .Select(s => new { name = Path.GetFileName(s), fullname = s })
                                                    .Where(s => !tlunames.Contains(s.fullname))           // find any new ones..
                                                    .OrderBy(s => s.name)
                                                    .Select(s => s.fullname)
                                                    .ToArray();

                    if (filenames.Length > 0)
                    {
                        lastnfi = OpenFileReader(filenames[0]);     // open first one
                        System.Diagnostics.Debug.WriteLine(string.Format("Found new file {0}", lastnfi.FullName));
                        ScanReader(entries, uientries);
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

            return new Tuple<List<JournalEntry>, List<UIEvent>>(entries, uientries);     // feed back now don't change file
        }

        // Called by ScanForNewEntries (from EDJournalClass Scan Tick Worker) to scan a NFI for new entries

        private void ScanReader(List<JournalEntry> entries, List<UIEvent> uientries)
        {
            System.Diagnostics.Debug.Assert(lastnfi.ID != 0);       // must have committed it at this point, prev code checked for it but it must have been done
            System.Diagnostics.Debug.Assert(netlogreaders.ContainsKey(lastnfi.FullName));       // must have added to netlogreaders.. double check

            bool readanything = lastnfi.ReadJournal(entries, uientries, historyrefreshparsing: false);

            if (StoreToDBDuringUpdateRead)
            {
                if (entries.Count > 0 || readanything )
                {
                    UserDatabase.Instance.ExecuteWithDatabase(cn =>
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        using (DbTransaction txn = cn.Connection.BeginTransaction())
                        {
                            if (entries.Count > 0)
                            {
                                entries = entries.Where(jre => JournalEntry.FindEntry(jre, cn, jre.GetJson(cn.Connection,txn)).Count == 0).ToList();

                                foreach (JournalEntry jre in entries)
                                {
                                    var json = jre.GetJson(cn.Connection, txn);
                                    jre.Add(json,cn.Connection,txn);
                                }
                            }

                            lastnfi.TravelLogUnit.Update(cn.Connection, txn);       // update TLU pos

                            txn.Commit();
                        }

                        if (sw.ElapsedMilliseconds >= 50)        // this is written to the log to try and debug bad DB behaviour
                        {
                            System.Diagnostics.Trace.WriteLine("Warning access to DB to write new journal entries slow " + sw.ElapsedMilliseconds);
                            foreach (var e in entries)
                                System.Diagnostics.Trace.WriteLine(".." + e.EventTimeUTC + " " + e.EventTypeStr);
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

        public List<EDJournalReader> ScanJournalFiles(int reloadlastn)
        {
            //            System.Diagnostics.Trace.WriteLine(BaseUtils.AppTicks.TickCountLap("PJF", true), "Scanned " + WatcherFolder);

            // order by file write time so we end up on the last one written
            FileInfo[] allFiles = Directory.EnumerateFiles(WatcherFolder, journalfilematch, SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

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
                UserDatabase.Instance.ExecuteWithDatabase(cn => 
                    {
                        using (DbTransaction txn = cn.Connection.BeginTransaction())
                        {
                            foreach (var tlu in tlutoadd)
                            {
                                tlu.Add(cn.Connection, txn);
                            }

                            txn.Commit();
                        }
                    });
            }

            return readersToUpdate;
        }

        // given a list of files to reparse, read them and store to db or fire them back (and set firebacklastn to make it work)

        public void ProcessDetectedNewFiles(List<EDJournalReader> readersToUpdate,  Action<int, string> updateProgress, 
                                            Action<JournalEntry, int,int,int,int> fireback = null, int firebacklastn = 0)
        {
            for (int i = 0; i < readersToUpdate.Count; i++)
            {
                EDJournalReader reader = readersToUpdate[i];

                List<JournalEntry> entries = new List<JournalEntry>();
                List<UIEvent> uievents = new List<UIEvent>();
                bool readanything = reader.ReadJournal(entries, uievents, historyrefreshparsing: true);      // this may create new commanders, and may write to the TLU

                if (fireback != null)
                {
                    if (readanything)               // need to update TLU pos if read anything
                        reader.TravelLogUnit.Update();

                    if (i >= readersToUpdate.Count - firebacklastn) // if within fireback window
                    {
                        for( int e = 0; e < entries.Count; e++ )
                        {
                            //System.Diagnostics.Debug.WriteLine("Fire {0} {1} {2} {3} {4} {5}", entries[e].CommanderId, i, readersToUpdate.Count, e, entries.Count, entries[e].EventTypeStr );
                            fireback(entries[e], i, readersToUpdate.Count, e, entries.Count);
                        }
                    }
                }
                else
                {
                    UserDatabase.Instance.ExecuteWithDatabase(cn =>
                    {
                        // only lookup TLUs if there is actually anything to compare against
                        ILookup<DateTime, JournalEntry> existing = entries.Count > 0 ? JournalEntry.GetAllByTLU(reader.ID, cn.Connection).ToLookup(e => e.EventTimeUTC) : null;

                        //System.Diagnostics.Trace.WriteLine(BaseUtils.AppTicks.TickCountLap("PJF"), i + " into db");

                        using (DbTransaction tn = cn.Connection.BeginTransaction())
                        {
                            foreach (JournalEntry jre in entries)
                            {
                                //System.Diagnostics.Trace.WriteLine(string.Format("--- Check {0} {1} Existing {2} : {3}", jre.EventTimeUTC, jre.EventTypeStr, existing[jre.EventTimeUTC].Count(), jre.GetJson().ToString()));

                                if (!existing[jre.EventTimeUTC].Any(e => JournalEntry.AreSameEntry(jre, e, cn.Connection, ent1jo: jre.GetJson(cn.Connection,tn))))
                                {
                                    //foreach (var x in existing[jre.EventTimeUTC]) { System.Diagnostics.Trace.WriteLine(string.Format(" passed vs {0} Deepequals {1}", x.GetJson().ToString(), x.GetJson().DeepEquals(jre.GetJson()))); }

                                    BaseUtils.JSON.JObject jo = jre.GetJson(cn.Connection, tn);
                                    jre.Add(jo, cn.Connection, tn);

                                    //System.Diagnostics.Trace.WriteLine(string.Format("Write Journal to db {0} {1}", jre.EventTimeUTC, jre.EventTypeStr));
                                }
                                else
                                {
                                    //System.Diagnostics.Trace.WriteLine(string.Format("Duplicate Journal to db {0} {1}", jre.EventTimeUTC, jre.EventTypeStr));
                                }
                            }

                            if (readanything)
                                reader.TravelLogUnit.Update(cn.Connection, tn);

                            tn.Commit();
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

        private EDJournalReader OpenFileReader(string filepath, bool delayadd = false)
        {
            EDJournalReader reader;

            //System.Diagnostics.Trace.WriteLine(string.Format("{0} Opening File {1}", Environment.TickCount % 10000, fi.FullName));

            if (netlogreaders.ContainsKey(filepath))        // cache
            {
                reader = netlogreaders[filepath];
            }
            else if (TravelLogUnit.TryGet(filepath, out TravelLogUnit tlu))   // from db
            {
                reader = new EDJournalReader(tlu);
                netlogreaders[filepath] = reader;
            }
            else
            {
                reader = new EDJournalReader(filepath);
                reader.TravelLogUnit.Type = TravelLogUnit.JournalType;
                if (!delayadd)
                    reader.TravelLogUnit.Add();
                netlogreaders[filepath] = reader;
            }

            return reader;
        }

        #endregion

    }
}