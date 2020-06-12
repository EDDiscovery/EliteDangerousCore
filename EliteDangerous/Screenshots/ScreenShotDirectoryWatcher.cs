/*
 * Copyright © 2020 EDDiscovery development team
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
using EliteDangerousCore;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;

// same code as before

namespace EliteDangerousCore.ScreenShots
{
    public class ScreenshotDirectoryWatcher : IDisposable
    {
        private Action<Action<ScreenShotImageConverter>> invokeonui;
        private FileSystemWatcher filesystemwatcher = null;

        private ConcurrentDictionary<string, JournalScreenshot> JournalScreenshotsByName = new ConcurrentDictionary<string, JournalScreenshot>(StringComparer.InvariantCultureIgnoreCase);

        private ConcurrentDictionary<string, System.Threading.Timer> ScreenshotTimers = new ConcurrentDictionary<string, System.Threading.Timer>(StringComparer.InvariantCultureIgnoreCase);

        public event Action<string,Size> OnScreenshot;

        Action<string> logit;
        Func<Tuple<string, string,string>> getcurinfo;      // system, body, commander

        private string outputfolder;
        private string watchedfolder;

        public ScreenshotDirectoryWatcher(Action<Action<ScreenShotImageConverter>> invokeonuip, Action<string> logger, 
                                        Func<Tuple<string,string,string>> currentloccmdr)
        {
            invokeonui = invokeonuip;
            logit = logger;
            getcurinfo = currentloccmdr;
        }

        public bool Start(string watchedfolderp, string ext, string outputfolderp)
        {
            this.Stop();

            watchedfolder = watchedfolderp;
            outputfolder = outputfolderp;

            if (Directory.Exists(watchedfolder))
            {
                filesystemwatcher = new System.IO.FileSystemWatcher();
                filesystemwatcher.Path = watchedfolder;

                filesystemwatcher.Filter = "*." + ext;
                filesystemwatcher.NotifyFilter = NotifyFilters.FileName;
                filesystemwatcher.Created += WatcherTripped;
                filesystemwatcher.EnableRaisingEvents = true;

                logit(string.Format("Scanning for {0} screenshots in {1}".T(EDTx.ScreenshotDirectoryWatcher_Scan) ,ext, watchedfolder));
                return true;
            }
            else
            {
                logit("Folder specified for image conversion does not exist, check settings in the Screenshots tab".T(EDTx.ScreenshotDirectoryWatcher_NOF));
                return false;
            }
        }

        public void Stop()
        {
            if (filesystemwatcher != null)
            {
                filesystemwatcher.Dispose();
                filesystemwatcher = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void NewJournalEntry(JournalEntry je)       // will be in UI thread
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            if (je.EventTypeID == JournalTypeEnum.Screenshot)
            {
                JournalScreenshot ss = je as JournalScreenshot;
                System.Diagnostics.Trace.WriteLine("Journal Screenshot logged " + ss.Filename);

                string ssname = ss.Filename;

                if (ssname.StartsWith("\\ED_Pictures\\"))   // cut to basename for the ID
                    ssname = ssname.Substring(13);

                JournalScreenshotsByName[ssname] = ss;
                invokeonui?.Invoke(cp => ProcessScreenshot(ss.Filename, ss.System, ss.Body, cp));
                JournalScreenshotsByName[ssname] = null;
            }
        }


        private void WatcherTripped(object sender, System.IO.FileSystemEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Directory watcher picked up screenshot " + e.FullPath);
            invokeonui?.Invoke(cp => ProcessFilesystemEvent(sender, e, cp));
        }

        private void ProcessFilesystemEvent(object sender, System.IO.FileSystemEventArgs e, ScreenShotImageConverter cp) // on UI thread
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            if (e.FullPath.ToLowerInvariant().EndsWith(".bmp"))
            {
                if (!ScreenshotTimers.ContainsKey(e.FullPath))
                {
                    System.Threading.Timer timer = new System.Threading.Timer(s=>TimerForFileEvent(e.FullPath), null, 5000, System.Threading.Timeout.Infinite);

                    // Destroy the timer if OnScreenshot was run between the above check and adding the timer to the dictionary
                    if (!ScreenshotTimers.TryAdd(e.FullPath, timer))
                    {
                        timer.Dispose();
                    }
                }
            }
            else
            {
                ProcessScreenshot(e.FullPath, null, null, cp);
            }
        }

        // time is set up if the file is .bmp by the file watcher.. 

        private void TimerForFileEvent(string filename)   // timer is executed on a background thread, go back to UI
        {
            JournalScreenshot ss = null;
            JournalScreenshotsByName.TryGetValue(Path.GetFileName(filename), out ss); // see if a journal screenshot matches it..

            this.invokeonui?.Invoke(cp => ProcessScreenshot(filename, ss?.System, ss?.Body, cp)); //process on UI thread
        }

        // called thru CalLWithConverter in UI main class to give us a ScreenShotImageConverter
        // fileid may not be the whole name if picked up thru ss system - like \EDPICTURES\

        private void ProcessScreenshot(string filenamepart, string sysname, string bodyname, ScreenShotImageConverter cp)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // UI thread

            var r = getcurinfo();

            if (sysname == null)            // fill in if required
                sysname = r.Item1 ?? "Unknown";
            
            if ( bodyname == null)        
                bodyname = r.Item2 ?? "Unknown";

            try
            {
                if (TryGetScreenshot(filenamepart, out Bitmap bmp, out string filepath, out DateTime timestamp))
                {
                    var fs = cp.Convert(bmp, filepath, outputfolder, timestamp, logit, bodyname, sysname, r.Item3);

                    if ( fs.Item1 != null )
                        OnScreenshot?.Invoke(fs.Item1,fs.Item2);
                }
                else
                    logit(string.Format("Failed to read screenshot {0}", filenamepart));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception watcher: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
//tbd                logit("Error in executing image conversion, try another screenshot, check output path settings. (Exception ".T(EDTx.ScreenshotDirectoryWatcher_Excp) + ex.Message + ")");
            }
        }

        private bool TryGetScreenshot(string filepart, out Bitmap bmp, out string filenameout, out DateTime timestamp)//, ref string store_name, ref Point finalsize, ref DateTime timestamp, out Bitmap bmp, out string readfilename, Action<string> logit, bool throwOnError = false)
        {
            timestamp = DateTime.Now;
            filenameout = null;
            bmp = null;

            for (int tries = 60; tries > 0; tries--)          // wait 30 seconds and then try it anyway.. 32K hires shots take a while to write.
            {
                if (filepart.StartsWith("\\ED_Pictures\\"))     // if its an ss record, try and find it either in watchedfolder or in default loc
                {
                    filepart = filepart.Substring(13);
                    string filepath = Path.Combine(watchedfolder, filepart);

                    if (!File.Exists(filepath))
                    {
                        string defaultInputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
                        filepath = Path.Combine(defaultInputDir, filepart);

                        if (!File.Exists(filepath))
                        {
                            filepart = filepath;
                        }
                    }
                }

                try
                {
                    using (FileStream testfile = File.Open(filepart, FileMode.Open, FileAccess.Read, FileShare.Read))        // throws if can't open
                    {
                        timestamp = new FileInfo(filepart).CreationTimeUtc;
                        MemoryStream memstrm = new MemoryStream(); // will be owned by bitmap
                        testfile.CopyTo(memstrm);
                        memstrm.Seek(0, SeekOrigin.Begin);
                        bmp = new Bitmap(memstrm);
                    }

                    filenameout = filepart;
                    return true;
                }
                catch 
                {
                    if (bmp != null)
                    {
                        bmp.Dispose();
                        bmp = null;
                    }
                }

                System.Threading.Thread.Sleep(500);     // every 500ms see if we can read the file, if we can, go, else wait..
            }

            return false;
        }
    }

}
