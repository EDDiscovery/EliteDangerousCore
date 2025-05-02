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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        // multithreaded, locked on access
        private HashSet<Tuple<string, DateTime>> screenshotted = new HashSet<Tuple<string, DateTime>>();

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

                logit(string.Format("Scanning for {0} screenshots in {1}".T(EDCTx.ScreenshotDirectoryWatcher_Scan) ,ext, watchedfolder));
                return true;
            }
            else
            {
                logit("Folder specified for image conversion does not exist, check screenshot settings in the Settings panel".T(EDCTx.ScreenshotDirectoryWatcher_NOF));
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
                JournalScreenshot jescreenshot = je as JournalScreenshot;

                string filepath = jescreenshot.Filename;

                if (filepath.StartsWith("\\ED_Pictures\\"))     // if its an ss record, try and find it either in watchedfolder or in default loc
                {
                    filepath = filepath.Substring(13);
                    filepath = Path.Combine(watchedfolder, filepath);

                    if (!File.Exists(filepath))
                    {
                        string defaultInputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Frontier Developments", "Elite Dangerous");
                        filepath = Path.Combine(defaultInputDir, filepath);
                    }
                }

                if (!File.Exists(filepath))
                {
                    logit($"Journal screenshot {filepath} not found");
                    return;
                }

                bool done = false;
                lock (screenshotted)        // due to multi threads
                {
                    var filedatetime = File.GetLastWriteTimeUtc(filepath);
                    var key = new Tuple<string, DateTime>(filepath, filedatetime);
                    done = screenshotted.Contains(key);   // see if done,
                    System.Diagnostics.Debug.WriteLine($"Screenshot journal file {key} Done {done}");
                    screenshotted.Add(key);               // indicate we have done it somehow
                }

                if (!done)     // if we have not screenshotted it
                {
                    invokeonui?.Invoke(cp =>
                        {
                            ProcessScreenshot(filepath, jescreenshot, cp);
                        });
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Screenshot Journal entry received but already processed " + jescreenshot.Filename);
                }
            }
        }

        private void WatcherTripped(object sender, System.IO.FileSystemEventArgs e)     
        {
            invokeonui?.Invoke(cp => ProcessFilesystemEvent(sender, e, cp));
        }

        private void ProcessFilesystemEvent(object sender, System.IO.FileSystemEventArgs e, ScreenShotImageConverter cp) // on UI thread
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);

            if (e.FullPath.ToLowerInvariant().EndsWith(".bmp"))     // cause its a frontier bmp file..
            {
                System.Diagnostics.Debug.WriteLine($"Screenshot file watch start a timer to wait for SS entry for {e.FullPath}");

                System.Timers.Timer t = new System.Timers.Timer(watchdelay);  // use a timer since we can set it up slowly before we start it
                t.Elapsed += T_Elapsed;
                filewatchTimers[t] = e.FullPath;
                t.Start();
            }
            else
            {
                ProcessScreenshot(e.FullPath, null, cp);
            }
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)     // not in UI thread
        {
            var t = sender as System.Timers.Timer;
            t.Stop();

            if ( filewatchTimers.TryRemove(t,out string filepath) )     // find the filename associated with this timer
            {
                if (File.Exists(filepath))     // still there.. it may have been removed by journal screenshot moving it
                {
                    bool done = false;
                    lock (screenshotted)        // due to multi threads
                    {
                        var filedatetime = File.GetLastWriteTimeUtc(filepath);
                        var key = new Tuple<string, DateTime>(filepath, filedatetime);
                        done = screenshotted.Contains(key);   // see if done,
                        System.Diagnostics.Debug.WriteLine($"Screenshot filewatcher file {key} Done {done}");
                        screenshotted.Add(key);               // indicate we have done it somehow
                    }

                    if (!done)          // and its not in the peristent list dealt with by screenshot
                    {
                        this.invokeonui?.Invoke(cp => ProcessScreenshot(filepath, null, cp)); //process on UI thread
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Screenshot Timer timed out, screenshot {filepath} was processed by journal screen shot");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Screenshot Timer timed out, file {filepath} was not there");
                }
            }

            t.Dispose();
        }

        // called thru CallWithConverter in UI main class to give us a ScreenShotImageConverter
        // fileid may not be the whole name if picked up thru ss system - like \EDPICTURES\
        // ss is not null if it was a screenshot
        // return if original is left in place

        private bool ProcessScreenshot(string filepath, JournalScreenshot ss, ScreenShotImageConverter cp)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);  // UI thread

            var r = getcurinfo();

            string sysname = ss == null ? r.Item1 : ss.System;
            if (sysname.IsEmpty())      // ss.system may be "" when dead
                sysname = "Unknown";
            string bodyname = ss == null ? r.Item2 : ss.Body;
            if (bodyname.IsEmpty())     // same with body
                bodyname = "Unknown";
            string cmdrname = ss == null ? r.Item3 : EDCommander.GetCommander(ss.CommanderId)?.Name;
            if (cmdrname.IsEmpty())
                cmdrname = "Unknown Commander";

            System.Diagnostics.Debug.WriteLine("..Screenshot Process {0} s={1} b={2} c={3}", filepath, sysname, bodyname, cmdrname);

            try
            {
                if (ReadBMP(filepath, out Bitmap bmp, out DateTime timestamputc))
                {
                    // return input filename now, output filename and size
                    var fs = cp.Convert(bmp, filepath, timestamputc, outputfolder, bodyname, sysname, cmdrname, logit);

                    if ( fs != null )
                        OnScreenshot?.Invoke(fs.Item1, fs.Item2, fs.Item3, ss);

                    return cp.OriginalImageOption == ScreenShotImageConverter.OriginalImageOptions.Leave;
                }
                else
                    logit(string.Format("Failed to read screenshot {0}", filepath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Screenshot Exception watcher: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
                logit("Error in executing image conversion, try another screenshot, check output path settings. (Exception ".T(EDCTx.ScreenshotDirectoryWatcher_Excp) + ex.Message + ")");
            }
            return false;
        }

        // given a filepath, try and read the file and datetime to bmp/timestamp
        // return null or filenamein

        private bool ReadBMP(string filepath, out Bitmap bmp, out DateTime timestamputc)//, ref string store_name, ref Point finalsize, ref DateTime timestamp, out Bitmap bmp, out string readfilename, Action<string> logit, bool throwOnError = false)
        {
            timestamputc = DateTime.UtcNow;
            bmp = null;

            for (int tries = 60; tries > 0; tries--)          // wait 30 seconds and then try it anyway.. 32K hires shots take a while to write.
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("..Screenshot Try read " + filepath);
                    if (File.Exists(filepath))
                    {
                        using (FileStream testfile = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))        // throws if can't open
                        {
                            timestamputc = File.GetLastWriteTimeUtc(filepath);
                            MemoryStream memstrm = new MemoryStream(); // will be owned by bitmap
                            testfile.CopyTo(memstrm);
                            memstrm.Seek(0, SeekOrigin.Begin);
                            bmp = new Bitmap(memstrm);
                        }

                        return true;
                    }
                }
                catch 
                {
                    bmp?.Dispose();
                    bmp = null;
                }

                System.Threading.Thread.Sleep(500);     // every 500ms see if we can read the file, if we can, go, else wait..
            }

            return false;
        }
    }

}
