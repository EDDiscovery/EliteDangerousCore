/*
 * Copyright © 2021 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EliteDangerousCore.EDAstro
{
    public static class EDAstroSync
    {
        public static void StopSync()
        {
            Exit = true;
            exitevent.Set();
            historyevent.Set();     // also trigger in case we are in thread hold
        }

        public static void SendEDAstroEvents(List<HistoryEntry> helist)
        {
            bool accepted = false;

            lock (acceptevents) // lock so it does not change under us
            {
                foreach (var he in helist)
                {
                    if (acceptevents.Count == 0 || acceptevents.Contains(he.journalEntry.EventTypeStr))    // if no list, or in list, accept
                    {
                        historylist.Enqueue(he);
                        accepted = true;
                    }
                }
            }

            if (accepted)   // if we got anything, make the thread start if required
            {
                historyevent.Set();

                // Start the sync thread if it's not already running
                if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
                {
                    Exit = false;
                    exitevent.Reset();
                    ThreadSync = new System.Threading.Thread(new System.Threading.ThreadStart(SyncThread));
                    ThreadSync.Name = "EDSM Journal Sync";
                    ThreadSync.IsBackground = true;
                    ThreadSync.Start();
                }
            }
        }

        private static void SyncThread()
        {
            try
            {
                running = 1;

                if (!checkedacceptedevents)
                {
                    lock (acceptevents)     // lock as we are changing it
                    {
                        EDAstroClass ac = new EDAstroClass();
                        var j = ac.GetJournalEventsToSend();
                        if (j != null)
                        {
                            acceptevents = new HashSet<string>(j);
                        }
                    }

                    checkedacceptedevents = true;
                }

                while (historylist.Count != 0)      // while stuff to send
                {
                    if (historylist.TryDequeue(out HistoryEntry he))        // next history event...
                    {
                        historyevent.Reset();

                        var jo = new List<JObject>();
                        bool odyssey = false;

                        do
                        {
                            if (acceptevents.Count == 0 || acceptevents.Contains(he.journalEntry.EventTypeStr))       // no need to lock, this thread only one which changes it
                            {
                                JObject json = he.journalEntry.GetJsonCloned();
                                json.RemoveWildcard("EDD*");        // remove any EDD specials
                                odyssey |= he.journalEntry.IsOdyssey;
                                jo.Add(json);
                            }
                        } while (jo.Count < maxEventsPerMessage && historylist.TryDequeue(out he));

                        EDAstroClass ac = new EDAstroClass();
                        ac.SendJournalEvents(jo, odyssey);
                    }

                    exitevent.WaitOne(10000);        // ms between groups

                    if (Exit)
                    {
                        return;
                    }
                }

                if (Exit)
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.Message);
            }
            finally
            {
                running = 0;
            }
        }

        private static Thread ThreadSync;
        private static int running = 0;
        private static bool Exit = false;
        private static ConcurrentQueue<HistoryEntry> historylist = new ConcurrentQueue<HistoryEntry>();
        private static AutoResetEvent historyevent = new AutoResetEvent(false);
        private static ManualResetEvent exitevent = new ManualResetEvent(false);
        private static int maxEventsPerMessage = 200;

        private static HashSet<string> acceptevents = new HashSet<string>();
        private static bool checkedacceptedevents = false;
    }

}
