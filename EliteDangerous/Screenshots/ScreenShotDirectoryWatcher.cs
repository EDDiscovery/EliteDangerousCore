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
        public event Action<string, Size> OnScreenshot;     // called on screenshot

        private Action<Action<ScreenShotImageConverter>> invokeonui;
        private FileSystemWatcher filesystemwatcher = null;

        // ones already processed by journal screenshot system
        private ConcurrentDictionary<string, JournalScreenshot> JournalScreenshotted = new ConcurrentDictionary<string, JournalScreenshot>(StringComparer.InvariantCultureIgnoreCase);

        // set of timers used per screenshot to delay processing them - detected by file watcher
        private ConcurrentDictionary<string, System.Threading.Timer> ScreenshotTimers = new ConcurrentDictionary<string, System.Threading.Timer>(StringComparer.InvariantCultureIgnoreCase);

        private Action<string> logit;
        private Func<Tuple<string, string,string>> getcurinfo;      // system, body, commander

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

        public void NewJournalEntry(JournalEntry je)       // will be in UI thread
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            if (je.EventTypeID == JournalTypeEnum.Screenshot)
            {
                JournalScreenshot ss = je as JournalScreenshot;
                System.Diagnostics.Trace.WriteLine("Journal Screenshot " + ss.Filename);

                string ssname = ss.Filename;

                if (ssname.StartsWith("\\ED_Pictures\\"))   // cut to basename for the ID
                    ssname = ssname.Substring(13);

                if (!JournalScreenshotted.ContainsKey(ssname))  // ensure no repeats
                {
                    JournalScreenshotted[ssname] = ss;      // record we processed it this way

                    invokeonui?.Invoke(cp => ProcessScreenshot(ss.Filename, ss.System, ss.Body, EDCommander.GetCommander(ss.CommanderId).Name ?? "Unknown", cp));

                    System.Diagnostics.Trace.WriteLine("Journal Screenshot over " + ss.Filename + " recorded as " + ssname);
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Journal Screenshot repeat and ignored " + ss.Filename + " recorded as " + ssname);
                }
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

            if (e.FullPath.ToLowerInvariant().EndsWith(".bmp"))     // cause its a frontier bmp file..
            {
                if (!ScreenshotTimers.ContainsKey(e.FullPath))      // just double check against repeated file watcher fires
                {
                    System.Diagnostics.Debug.WriteLine("File watch start a timer to wait for SS entry");
                    System.Threading.Timer timer = new System.Threading.Timer(s=>TimeOutForFileWatcher(e.FullPath), null, 5000, System.Threading.Timeout.Infinite);

                    if (!ScreenshotTimers.TryAdd(e.FullPath, timer))
                    {
                        timer.Dispose();
                    }
                }
            }
            else
            {
                if (!JournalScreenshotted.ContainsKey(Path.GetFileName(e.FullPath)))        // just make sure in case the order is wrong
                {
                    System.Diagnostics.Debug.WriteLine("Not a .bmp, not been journal screenshotted, go");
                    ProcessScreenshot(e.FullPath, null, null, null, cp);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Journal screenshot already did this one");
            }
        }

        // time is set up if the file is .bmp by the file watcher.. 

        private void TimeOutForFileWatcher(string filename)   // timer is executed on a background thread, go back to UI
        {
            if (!JournalScreenshotted.ContainsKey(Path.GetFileName(filename)))
            {
                System.Diagnostics.Debug.WriteLine("Timer timed out, no journal screen shot");
                this.invokeonui?.Invoke(cp => ProcessScreenshot(filename, null,null,null, cp)); //process on UI thread
            }
            else
                System.Diagnostics.Debug.WriteLine("Timer timed out, screenshot was processed by journal screen shot");
        }

        // called thru CalLWithConverter in UI main class to give us a ScreenShotImageConverter
        // fileid may not be the whole name if picked up thru ss system - like \EDPICTURES\

        private void ProcessScreenshot(string filenamepart, string sysname, string bodyname, string cmdrname, ScreenShotImageConverter cp)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // UI thread

            var r = getcurinfo();

            if (sysname == null)            // fill in if required
                sysname = r.Item1 ?? "Unknown";
            
            if ( bodyname == null)        
                bodyname = r.Item2 ?? "Unknown";

            if (cmdrname == null)
                cmdrname = r.Item3 ?? "Unknown";

            System.Diagnostics.Debug.WriteLine("Process {0} s={1} b={2} c={3}", filenamepart, sysname, bodyname, cmdrname);

            try
            {
                if (TryGetScreenshot(filenamepart, out Bitmap bmp, out string filepath, out DateTime timestamp))
                {
                    var fs = cp.Convert(bmp, filepath, outputfolder, timestamp, logit, bodyname, sysname, cmdrname);

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
                logit("Error in executing image conversion, try another screenshot, check output path settings. (Exception ".T(EDTx.ScreenshotDirectoryWatcher_Excp) + ex.Message + ")");
            }
        }

        private bool TryGetScreenshot(string filepart, out Bitmap bmp, out string filenameout, out DateTime timestamp)//, ref string store_name, ref Point finalsize, ref DateTime timestamp, out Bitmap bmp, out string readfilename, Action<string> logit, bool throwOnError = false)
        {
            timestamp = DateTime.Now;
            filenameout = null;
            bmp = null;

            for (int tries = 60; tries > 0; tries--)          // wait 30 seconds and then try it anyway.. 32K hires shots take a while to write.
            {
                filenameout = filepart;

                if (filepart.StartsWith("\\ED_Pictures\\"))     // if its an ss record, try and find it either in watchedfolder or in default loc
                {
                    filepart = filepart.Substring(13);
                    filenameout = Path.Combine(watchedfolder, filepart);

                    if (!File.Exists(filenameout))
                    {
                        string defaultInputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
                        filenameout = Path.Combine(defaultInputDir, filepart);
                    }
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine("Try read " + filenameout);
                    if (File.Exists(filenameout))
                    {
                        using (FileStream testfile = File.Open(filenameout, FileMode.Open, FileAccess.Read, FileShare.Read))        // throws if can't open
                        {
                            timestamp = new FileInfo(filenameout).CreationTimeUtc;
                            MemoryStream memstrm = new MemoryStream(); // will be owned by bitmap
                            testfile.CopyTo(memstrm);
                            memstrm.Seek(0, SeekOrigin.Begin);
                            bmp = new Bitmap(memstrm);
                        }
                    }
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
