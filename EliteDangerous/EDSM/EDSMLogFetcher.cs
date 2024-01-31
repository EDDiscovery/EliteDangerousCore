/*
 * Copyright © 2016-2023 EDDiscovery development team
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
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EliteDangerousCore.JournalEvents;
using EliteDangerousCore.DB;

namespace EliteDangerousCore.EDSM
{
    public class EDSMLogFetcher
    {
        public EDSMLogFetcher(Action<string> logline, Action<string> statusLineUpdate)
        {
            LogLine = logline;
            StatusLineUpdate = statusLineUpdate;
        }

        private string KeyName(string value="") { return "EDSMLogFetcher_" + CommanderId.ToStringInvariant() + value; }

        public void Start( EDCommander cmdr )
        {
            ExitRequested.Reset();

            System.Diagnostics.Trace.WriteLine($"EDSM Fetch logs start with cmdr {cmdr.Id}");

            Commander = cmdr;

            if ((ThreadEDSMFetchLogs == null || !ThreadEDSMFetchLogs.IsAlive) && Commander.SyncFromEdsm && EDSMClass.IsServerAddressValid)
            {
                ThreadEDSMFetchLogs = new Thread(FetcherThreadProc) { IsBackground = true, Name = "EDSM Log Fetcher" };
                ThreadEDSMFetchLogs.Start();
            }
        }

        public void AsyncStop()
        {
            ExitRequested.Set();
        }

        public void StopCheck()
        {
            if (ThreadEDSMFetchLogs != null)
            {
                ExitRequested.Set();
                ThreadEDSMFetchLogs.Join(); // wait for exit.
                ThreadEDSMFetchLogs = null;
                StatusLineUpdate("");
            }
        }

        public void ResetFetch()
        {
            if (Commander != null)
            {
                UserDatabase.Instance.DeleteKey(KeyName());
            }
        }

        private void FetcherThreadProc()
        {
            System.Diagnostics.Trace.WriteLine($"EDSM Thread logs start");
            DateTime lastCommentFetch = DateTime.MinValue;

            int waittime = 1000; // initial waittime, will be reestimated later

            DateTime curtime = DateTime.UtcNow;
            int logscollected = 0;

            bool displayedstatus = false;

            while (!ExitRequested.WaitOne(waittime))
            {
                if (ExitRequested.WaitOne(0))
                {
                    return;
                }

                if (displayedstatus)
                {
                    StatusLineUpdate($"");
                    displayedstatus = false;
                }

                EDSMClass edsm = new EDSMClass(Commander);

                // logic checked 21/12/2018 RJP

                if (edsm.ValidCredentials && Commander.SyncFromEdsm)
                {
                    if (DateTime.UtcNow > lastCommentFetch.AddHours(1))
                    {
                        edsm.GetComments(l => System.Diagnostics.Trace.WriteLine(l));
                        lastCommentFetch = DateTime.UtcNow;
                    }

                    DateTime lastqueryendtime = UserDatabase.Instance.GetSettingDate(KeyName(), EliteReleaseDates.EDSMRelease);
                    //DateTime lastqueryendtime = UserDatabase.Instance.GetSettingDate(KeyName(), new DateTime(2023,6,12));
                    //lastqueryendtime = new DateTime(2016, 1, 12);

                    if ( (DateTime.UtcNow-lastqueryendtime).TotalMinutes >= EDSMMaxLogAgeMinutes) // if time to check
                    {
                        lastqueryendtime = lastqueryendtime.AddDays(7);                         // move 7 days forward. This may move it past current time, but will be corrected below

                        System.Diagnostics.Trace.WriteLine($"EDSM Log Fetcher ask for week up to {lastqueryendtime}");

                        int res = edsm.GetLogs(null, lastqueryendtime, out List<JournalFSDJump> edsmlogs, 
                                        out DateTime logstarttime, out DateTime logendtime, 
                                        out BaseUtils.ResponseData response, (s) => StatusLineUpdate(s));

                        //int res = 100;  DateTime logstarttime = lastqueryendtime.AddDays(-7);  DateTime logendtime = lastqueryendtime.AddDays(-1); List<JournalFSDJump> edsmlogs = new List<JournalFSDJump>();

                        if (res == 100)   // hunky dory - note if Anthor faults, we just retry again and again
                        {
                            if (edsmlogs?.Count > 0)     // if anything to process..
                            {
                                logscollected += Process(edsmlogs, logstarttime, logendtime);
                            }
                            else
                                StatusLineUpdate($"EDSM Log Fetcher checked to UTC {lastqueryendtime} No logs");

                            displayedstatus = true;

                            UserDatabase.Instance.PutSettingDate(KeyName(), lastqueryendtime);      // save back for now in case it did not move it past
                        }
                        else if (res != -1)
                        {
                            System.Diagnostics.Debug.WriteLine("EDSM Log request rejected with " + res);
                        }

                        if (response.Headers != null &&
                            response.Headers["X-Rate-Limit-Limit"] != null &&
                            response.Headers["X-Rate-Limit-Remaining"] != null &&
                            response.Headers["X-Rate-Limit-Reset"] != null &&
                            Int32.TryParse(response.Headers["X-Rate-Limit-Limit"], out int ratelimitlimit) &&
                            Int32.TryParse(response.Headers["X-Rate-Limit-Remaining"], out int ratelimitremain) &&
                            Int32.TryParse(response.Headers["X-Rate-Limit-Reset"], out int ratelimitreset))
                        {
                            if (ratelimitremain < ratelimitlimit * 2 / 4)       // lets keep at least X remaining for other purposes later..
                                waittime = 1000 * ratelimitreset / (ratelimitlimit - ratelimitremain);    // slow down to its pace now.. example 878/(360-272) = 10 seconds per quota
                            else
                                waittime = 1000;        // 1 second so we don't thrash

                            System.Diagnostics.Trace.WriteLine($"EDSM Log Fetcher Delay Parameters {ratelimitlimit} {ratelimitremain} {ratelimitreset} {waittime}ms");
                        }

                        if (lastqueryendtime > DateTime.UtcNow)                                 // we have asked beyond our date, so we are at the end of the game
                        {
                            lastqueryendtime = DateTime.UtcNow;                                 // limit to now
                            UserDatabase.Instance.PutSettingDate(KeyName(), lastqueryendtime);  // and save back

                            if (logscollected > 0)                                              // if we collected logs, invoke the call back    
                            {
                                LogLine($"EDSM Log Fetcher got {logscollected} log entries from EDSM, Refresh history");
                                OnDownloadedSystems?.Invoke();
                                logscollected = 0;
                            }
                        }

                    }
                }
            }
        }

        private int Process(List<JournalFSDJump> edsmlogs, DateTime logstarttime, DateTime logendtime)
        {
            // Get all of the local entries now that we have the entries from EDSM
            // Moved here to avoid the race that could have been causing duplicate entries
            // EDSM only returns FSD entries, so only look for them.  Tested 27/4/2018 after the HE optimisations

            List<HistoryEntry> hlfsdlist = JournalEntry.GetAll(Commander.Id, logstarttime.AddDays(-1), logendtime.AddDays(1)).
                OfType<JournalLocOrJump>().OrderBy(je => je.EventTimeUTC).
                Select(je => HistoryEntry.FromJournalEntry(je, null, null)).ToList();   

            List<JournalFSDJump> toadd = new List<JournalFSDJump>();

            int previdx = -1;
            foreach (JournalFSDJump jfsd in edsmlogs)      // find out list of ones not present
            {
                int index = hlfsdlist.FindIndex(x => x.System.Name.Equals(jfsd.StarSystem, StringComparison.InvariantCultureIgnoreCase) && x.EventTimeUTC.Ticks == jfsd.EventTimeUTC.Ticks);

                if (index < 0)      // not found, see if its around that date..
                {
                    // Look for any entries where DST may have thrown off the time
                    foreach (var vi in hlfsdlist.Select((v, i) => new { v = v, i = i }).Where(vi => vi.v.System.Name.Equals(jfsd.StarSystem, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (vi.i > previdx)
                        {
                            double hdiff = vi.v.EventTimeUTC.Subtract(jfsd.EventTimeUTC).TotalHours;
                            if (hdiff >= -2 && hdiff <= 2 && hdiff == Math.Floor(hdiff))
                            {
                                index = vi.i;       // same system, nearly same time..
                                break;
                            }
                        }
                    }
                }

                if (index < 0)      // its not a duplicate, add to db
                {
                    toadd.Add(jfsd);
                }
                else
                {                   // it is a duplicate, check if the first discovery flag is set right
                    JournalFSDJump existingfsd = hlfsdlist[index].journalEntry as JournalFSDJump;

                    if (existingfsd != null && existingfsd.EDSMFirstDiscover != jfsd.EDSMFirstDiscover)    // if we have a FSD one, and first discover is different
                    {
                        existingfsd.UpdateFirstDiscover(jfsd.EDSMFirstDiscover);
                    }

                    previdx = index;
                }
            }

            if (toadd.Count > 0)  // if we have any, we can add 
            {
                System.Diagnostics.Debug.WriteLine($"Adding EDSM logs count {toadd.Count}");

                TravelLogUnit tlu = new TravelLogUnit("EDSM\\EDSM-" + DateTime.Now.ToStringYearFirstInvariant());    // need a tlu for it
                tlu.Type = TravelLogUnit.EDSMType;  // EDSM
                tlu.CommanderId = EDCommander.CurrentCmdrID;
                tlu.Add();  // Add to Database

                UserDatabase.Instance.DBWrite(cn =>
                {
                    foreach (JournalFSDJump jfsd in toadd)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Add {0} {1}", jfsd.EventTimeUTC, jfsd.StarSystem));
                        jfsd.SetTLUCommander(tlu.ID, tlu.CommanderId.Value);        // update its TLU id to the TLU made above
                        jfsd.Add(jfsd.CreateFSDJournalEntryJson(), cn, null);     // add it to the db with the JSON created
                    }
                });
            }

            return toadd.Count;
        }

        private static int EDSMMaxLogAgeMinutes = 15;

        private Thread ThreadEDSMFetchLogs;
        private ManualResetEvent ExitRequested = new ManualResetEvent(false);
        private Action<string> LogLine;
        private Action<string> StatusLineUpdate;

        public delegate void EDSMDownloadedSystems();
        public event EDSMDownloadedSystems OnDownloadedSystems;

        private EDCommander Commander = null;
        private int CommanderId { get { return Commander.Id; } }

    }
}
