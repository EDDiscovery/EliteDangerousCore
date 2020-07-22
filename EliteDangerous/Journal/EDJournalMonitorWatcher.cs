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

    public class JournalMonitorWatcher
    {
        public string WatcherFolder { get; set; }

        private Dictionary<string, EDJournalReader> netlogreaders = new Dictionary<string, EDJournalReader>();
        private EDJournalReader lastnfi = null;          // last one read..
        private FileSystemWatcher m_Watcher;
        private int ticksNoActivity = 0;
        private ConcurrentQueue<string> m_netLogFileQueue;
        private const string journalfilematch = "Journal*.log";       // this picks up beta and normal logs

        public JournalMonitorWatcher(string folder)
        {
            WatcherFolder = folder;
        }

        #region Scan start stop and monitor

        public void StartMonitor()
        {
            if (m_Watcher == null)
            {
                try
                {
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

        public Tuple<List<JournalEntry>,List<UIEvent>> ScanForNewEntries()
        {
            var entries = new List<JournalEntry>();
            var uientries = new List<UIEvent>();

            try
            {
                string filename = null;

                if (lastnfi != null)                            // always give old file another go, even if we are going to change
                {
                    if (!File.Exists(lastnfi.FileName))         // if its been removed, null
                    {
                        lastnfi = null;
                    }
                    else
                    {
                        ScanReader(lastnfi, entries, uientries);

                        if (entries.Count > 0 || uientries.Count > 0 )
                        {
                            ticksNoActivity = 0;
                            return new Tuple<List<JournalEntry>, List<UIEvent>>(entries,uientries);     // feed back now don't change file
                        }
                    }
                }

                if (m_netLogFileQueue.TryDequeue(out filename))      // if a new one queued, we swap to using it
                {
                    lastnfi = OpenFileReader(new FileInfo(filename));
                    System.Diagnostics.Debug.WriteLine(string.Format("Change to scan {0}", lastnfi.FileName));
                    if (lastnfi != null)
                        ScanReader(lastnfi, entries, uientries);   // scan new one
                }
                // every few goes, if its not there or filepos is greater equal to length (so only done when fully up to date)
                else if ( ticksNoActivity >= 30 && (lastnfi == null || lastnfi.filePos >= new FileInfo(lastnfi.FileName).Length))
                {
                    HashSet<string> tlunames = new HashSet<string>(TravelLogUnit.GetAllNames());
                    string[] filenames = Directory.EnumerateFiles(WatcherFolder, journalfilematch, SearchOption.AllDirectories)
                                                  .Select(s => new { name = Path.GetFileName(s), fullname = s })
                                                  .Where(s => !tlunames.Contains(s.name))           // find any new ones..
                                                  .OrderBy(s => s.name)
                                                  .Select(s => s.fullname)
                                                  .ToArray();

                    foreach (var name in filenames)         // for any new filenames..
                    {
                        System.Diagnostics.Debug.WriteLine("No Activity but found new file " + name);
                        lastnfi = OpenFileReader(new FileInfo(name));
                        break;      // stop on first found
                    }

                    if (lastnfi != null)
                        ScanReader(lastnfi, entries , uientries);   // scan new one

                    ticksNoActivity = 0;
                }

                ticksNoActivity++;

                return new Tuple<List<JournalEntry>, List<UIEvent>>(entries, uientries);     // feed back now don't change file
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Net tick exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                return new Tuple<List<JournalEntry>, List<UIEvent>>(new List<JournalEntry>(), new List<UIEvent>());     // send out empty
            }
        }

        // Called by ScanForNewEntries (from EDJournalClass Scan Tick Worker) to scan a NFI for new entries

        private void ScanReader(EDJournalReader nfi, List<JournalEntry> entries, List<UIEvent> uientries )
        {
            int netlogpos = 0;

            try
            {
                if (nfi.TravelLogUnit.id == 0)
                {
                    nfi.TravelLogUnit.type = TravelLogUnit.JournalType;
                    nfi.TravelLogUnit.Add();
                }

                netlogpos = nfi.TravelLogUnit.Size;

                bool readanything = nfi.ReadJournal(out List<JournalEntry> ents, out List<UIEvent> uie, historyrefreshparsing: false, resetOnError: false );

                uientries.AddRange(uie);

                if (readanything)           // if we read, we must update the travel log pos
                {
                    //System.Diagnostics.Debug.WriteLine("ScanReader " + Path.GetFileName(nfi.FileName) + " read " + ents.Count + " ui " +uientries.Count + " size " + netlogpos);

                    UserDatabase.Instance.ExecuteWithDatabase(cn =>
                    {
                        using (DbTransaction txn = cn.Connection.BeginTransaction())
                        {
                            ents = ents.Where(jre => JournalEntry.FindEntry(jre, cn, jre.GetJson()).Count == 0).ToList();

                            foreach (JournalEntry jre in ents)
                            {
                                entries.Add(jre);
                                jre.Add(jre.GetJson(), cn.Connection, txn);
                            }

                            //System.Diagnostics.Debug.WriteLine("Wrote " + ents.Count() + " to db and updated TLU");

                            nfi.TravelLogUnit.Update(cn.Connection);

                            txn.Commit();
                        }
                    });
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.Message);
                // Revert and re-read the failed entries
                if (nfi != null && nfi.TravelLogUnit != null)
                {
                    nfi.TravelLogUnit.Size = netlogpos;
                }

                throw;
            }
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

            List<TravelLogUnit> toadd = new List<TravelLogUnit>();

            for (int i = 0; i < allFiles.Length; i++)
            {
                FileInfo fi = allFiles[i];

                var reader = OpenFileReader(fi, true);       // open it, but delay adding

                if (reader.TravelLogUnit.id == 0)            // if not present, add to commit add list
                    toadd.Add(reader.TravelLogUnit);

                if (!netlogreaders.ContainsKey(reader.TravelLogUnit.Name))
                {
                    netlogreaders[reader.TravelLogUnit.Name] = reader;
                }

                bool islast = (i == allFiles.Length - 1);

                if ( i >= allFiles.Length - reloadlastn )
                {
                    reader.TravelLogUnit.Size = 0;      // by setting the start zero (reader.filePos is the same as Size)
                }

                if (reader.filePos != fi.Length || islast )  // File not already in DB, or is the last one
                {
                    readersToUpdate.Add(reader);
                }
            }

            if ( toadd.Count > 0 )                      // now, on spinning rust, this takes ages for 600+ log files first time, so transaction it
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn => 
                    {
                        using (DbTransaction txn = cn.Connection.BeginTransaction())
                        {
                            foreach (var tlu in toadd)
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

                reader.ReadJournal(out List<JournalEntry> entries, out List<UIEvent> uievents, historyrefreshparsing: true, resetOnError: true);      // this may create new commanders, and may write to the TLU db

                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    if (entries.Count > 0 )
                    {
                        if (fireback != null)
                        {
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
                            ILookup<DateTime, JournalEntry> existing = JournalEntry.GetAllByTLU(reader.TravelLogUnit.id, cn.Connection).ToLookup(e => e.EventTimeUTC);

                            //System.Diagnostics.Trace.WriteLine(BaseUtils.AppTicks.TickCountLap("PJF"), i + " into db");

                            using (DbTransaction tn = cn.Connection.BeginTransaction())
                            {
                                foreach (JournalEntry jre in entries)
                                {
                                    if (!existing[jre.EventTimeUTC].Any(e => JournalEntry.AreSameEntry(jre, e, cn.Connection, ent1jo: jre.GetJson())))
                                    {
                                        jre.Add(jre.GetJson(), cn.Connection, tn);

                                        //System.Diagnostics.Trace.WriteLine(string.Format("Write Journal to db {0} {1}", jre.JournalEntry.EventTimeUTC, jre.JournalEntry.EventTypeStr));
                                    }
                                }

                                tn.Commit();
                            }
                        }
                    }

                    reader.TravelLogUnit.Update(cn.Connection);

                    updateProgress((i + 1) * 100 / readersToUpdate.Count, reader.TravelLogUnit.Name);

                    lastnfi = reader;
                });
            }

            updateProgress(-1, "");
        }

        #endregion

        #region Open

        // open a new file for watching, place it into the netlogreaders list

        private EDJournalReader OpenFileReader(FileInfo fi, bool delayadd = false)
        {
            EDJournalReader reader;
            TravelLogUnit tlu;

            //System.Diagnostics.Trace.WriteLine(string.Format("{0} Opening File {1}", Environment.TickCount % 10000, fi.FullName));

            if (netlogreaders.ContainsKey(fi.Name))
            {
                reader = netlogreaders[fi.Name];
            }
            else if (TravelLogUnit.TryGet(fi.Name, out tlu))
            {
                tlu.Path = fi.DirectoryName;
                reader = new EDJournalReader(tlu);
                netlogreaders[fi.Name] = reader;
            }
            else
            {
                reader = new EDJournalReader(fi.FullName);
                reader.TravelLogUnit.type = TravelLogUnit.JournalType;
                if (!delayadd)
                    reader.TravelLogUnit.Add();
                netlogreaders[fi.Name] = reader;
            }

            return reader;
        }

        #endregion

    }
}