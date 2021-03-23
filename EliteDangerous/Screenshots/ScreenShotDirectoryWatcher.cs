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
        public event Action<string, string, Size, JournalScreenshot> OnScreenshot;     // called on screenshot

        private Action<Action<ScreenShotImageConverter>> invokeonui;
        private FileSystemWatcher filesystemwatcher = null;

        // ones already processed by journal screenshot system and which the originals did not get removed
        private ConcurrentDictionary<string, JournalScreenshot> journalScreenshotted = new ConcurrentDictionary<string, JournalScreenshot>(StringComparer.InvariantCultureIgnoreCase);

        private ConcurrentDictionary<System.Timers.Timer, string> filewatchTimers = new ConcurrentDictionary<System.Timers.Timer, string>();

        private Action<string> logit;
        private Func<Tuple<string, string,string>> getcurinfo;      // system, body, commander

        private string outputfolder;
        private string watchedfolder;
        private int watchdelay;

        public ScreenshotDirectoryWatcher(Action<Action<ScreenShotImageConverter>> invokeonuip, Action<string> logger, 
                                        Func<Tuple<string,string,string>> currentloccmdr, int watchdelaytime)
        {
            invokeonui = invokeonuip;
            logit = logger;
            getcurinfo = currentloccmdr;
            watchdelay = watchdelaytime;
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
                filesystemwatcher.EnableRaisingEvents = false;
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

                string ssname = ss.Filename.StartsWith("\\ED_Pictures\\") ? ss.Filename.Substring(13) : ss.Filename;
                invokeonui?.Invoke(cp => 
                    {
                        bool leftinplace = ProcessScreenshot(ss.Filename, ss, cp);      
                        if (leftinplace)
                            journalScreenshotted[ssname] = ss;      // if we leave the file behind, tell the file watcher we have done it
                    });
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
                System.Diagnostics.Debug.WriteLine("File watch start a timer to wait for SS entry");

                System.Timers.Timer t = new System.Timers.Timer(watchdelay);  // use a timer since we can set it up slowly before we start it
                t.Elapsed += T_Elapsed;
                filewatchTimers[t] = e.FullPath;
                t.Start();
            }
            else
            {
                string name = Path.GetFileName(e.FullPath);
                ProcessScreenshot(e.FullPath, null, cp);
            }
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)     // not in UI thread
        {
            var t = sender as System.Timers.Timer;
            t.Stop();

            if ( filewatchTimers.TryRemove(t,out string filename) )     // find the filename associated with this timer
            {
                if (File.Exists(filename))     // still there.. it may have been removed by journal screenshot moving it
                {
                    if (!journalScreenshotted.ContainsKey(Path.GetFileName(filename)))     // and its not in the peristent list dealt with by screenshot
                    {
                        System.Diagnostics.Debug.WriteLine("Timer timed out, not journal screen shotted");
                        this.invokeonui?.Invoke(cp => ProcessScreenshot(filename, null, cp)); //process on UI thread
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Timer timed out, screenshot was processed by journal screen shot");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Timer timed out, file was not there");
                }
            }

            t.Dispose();
        }

        // called thru CallWithConverter in UI main class to give us a ScreenShotImageConverter
        // fileid may not be the whole name if picked up thru ss system - like \EDPICTURES\
        // ss is not null if it was a screenshot
        // return if original is left in place

        private bool ProcessScreenshot(string filenamepart, JournalScreenshot ss, ScreenShotImageConverter cp)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // UI thread

            var r = getcurinfo();

            string sysname = (ss == null ? r.Item1 : ss.System) ?? "Unknown";
            string bodyname = (ss == null ? r.Item2 : ss.Body) ?? "Unknown";
            string cmdrname = (ss == null ? r.Item3 : EDCommander.GetCommander(ss.CommanderId)?.Name) ?? "Unknown";

            System.Diagnostics.Debug.WriteLine("Process {0} s={1} b={2} c={3}", filenamepart, sysname, bodyname, cmdrname);

            try
            {
                string filein = TryGetScreenshot(filenamepart, out Bitmap bmp, out DateTime timestamputc);

                if (filein != null)
                {
                    // return input filename now, output filename and size
                    var fs = cp.Convert(bmp, filein, timestamputc, outputfolder, bodyname, sysname, cmdrname, logit);

                    if ( fs != null )
                        OnScreenshot?.Invoke(fs.Item1, fs.Item2, fs.Item3, ss);

                    return cp.OriginalImageOption == ScreenShotImageConverter.OriginalImageOptions.Leave;
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
            return false;
        }

        // given a filepart, try and read the file and datetime to bmp/timestamp
        // return null or filenamein

        private string TryGetScreenshot(string filepart, out Bitmap bmp, out DateTime timestamputc)//, ref string store_name, ref Point finalsize, ref DateTime timestamp, out Bitmap bmp, out string readfilename, Action<string> logit, bool throwOnError = false)
        {
            timestamputc = DateTime.UtcNow;
            string filenameout = null;
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
                            timestamputc = new FileInfo(filenameout).CreationTimeUtc;
                            MemoryStream memstrm = new MemoryStream(); // will be owned by bitmap
                            testfile.CopyTo(memstrm);
                            memstrm.Seek(0, SeekOrigin.Begin);
                            bmp = new Bitmap(memstrm);
                        }

                        return filenameout;
                    }
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

            return null;
        }
    }

}
