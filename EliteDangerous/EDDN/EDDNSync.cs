/*
 * Copyright © 2016 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using EliteDangerousCore.JournalEvents;

namespace EliteDangerousCore.EDDN
{
    public static class EDDNSync
    {
        private static Thread ThreadEDDNSync;
        private static int running = 0;
        private static bool Exit = false;
        private static ConcurrentQueue<HistoryEntry> hlscanunsyncedlist = new ConcurrentQueue<HistoryEntry>();
        private static AutoResetEvent hlscanevent = new AutoResetEvent(false);
        private static Action<string> logger;

        static public Action<int> SentEvents;       // called in thread when sync thread has finished and is terminating

        public static bool SendEDDNEvents(Action<string> log, IEnumerable<HistoryEntry> helist)
        {
            logger = log;

            foreach (HistoryEntry he in helist)
            {
                hlscanunsyncedlist.Enqueue(he);
            }

            hlscanevent.Set();

            // Start the sync thread if it's not already running
            if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
            {
                Exit = false;
                ThreadEDDNSync = new System.Threading.Thread(new System.Threading.ThreadStart(SyncThread));
                ThreadEDDNSync.Name = "EDDN Sync";
                ThreadEDDNSync.IsBackground = true;
                ThreadEDDNSync.Start();
            }

            return true;
        }

        public static void StopSync()
        {
            Exit = true;
            hlscanevent.Set();
        }

        private static void SyncThread()
        {
            running = 1;
            //mainForm.LogLine("Starting EDDN sync thread");

            while (hlscanunsyncedlist.Count != 0)
            {
                HistoryEntry he = null;

                int eventcount = 0;

                while (hlscanunsyncedlist.TryDequeue(out he))
                {
                    try
                    {
                        hlscanevent.Reset();

                        TimeSpan age = he.AgeOfEntry();

                        if (age.Days >= 1 )
                        {
                            System.Diagnostics.Debug.WriteLine("EDDN: Ignoring entry due to age");
                        }
                        else
                        {
                            bool? res = EDDNSync.SendToEDDN(he);

                            if (res != null)    // if attempted to send
                            {
                                if (res.Value == true)
                                {
                                    logger?.Invoke($"Sent {he.EntryType.ToString()} event to EDDN ({he.EventSummary})");
                                    eventcount++;
                                }
                                else
                                    logger?.Invoke($"Failed sending {he.EntryType.ToString()} event to EDDN ({he.EventSummary})");
                            }
                            else
                            {
                                continue; // skip the 1 second delay if nothing was sent
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.Message);
                        System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.StackTrace);
                        logger?.Invoke("EDDN sync Exception " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    if (Exit)
                    {
                        running = 0;
                        return;
                    }

                    Thread.Sleep(1000);   // Throttling to 1 per second to not kill EDDN network
                }

                SentEvents?.Invoke(eventcount);     // tell the system..

                if (hlscanunsyncedlist.IsEmpty)     // if nothing there..
                    hlscanevent.WaitOne(60000);     // Wait up to 60 seconds for another EDDN event to come in

                if (Exit)
                {
                    break;
                }
            }

            running = 0;
        }

        // Send to test vectors it to the beta server
        static public bool? SendToEDDN(HistoryEntry he, bool sendtotest = false)
        {
            EDDNClass eddn = new EDDNClass();

            if (he.Commander != null)
            {
                eddn.CommanderName = he.Commander.EdsmName;

                if (string.IsNullOrEmpty(eddn.CommanderName))
                    eddn.CommanderName = he.Commander.Name;

                if (he.Commander.Name.StartsWith("[BETA]", StringComparison.InvariantCultureIgnoreCase))
                    eddn.IsBeta = true;
            }

            if (he.journalEntry.IsBeta || sendtotest )      
                eddn.IsBeta = true;

            JournalEntry je = he.journalEntry;

            if (je == null)
            {
                je = JournalEntry.Get(he.Journalid);
            }

            BaseUtils.JSON.JObject msg = null;

            if (je.EventTypeID == JournalTypeEnum.FSDJump)
            {
                msg = eddn.CreateEDDNMessage(je as JournalFSDJump);
            }
            else if (je.EventTypeID == JournalTypeEnum.Location)
            {
                msg = eddn.CreateEDDNMessage(je as JournalLocation);
            }
            else if (je.EventTypeID == JournalTypeEnum.CarrierJump)
            {
                msg = eddn.CreateEDDNMessage(je as JournalCarrierJump);
            }
            else if (je.EventTypeID == JournalTypeEnum.Docked)
            {
                msg = eddn.CreateEDDNMessage(je as JournalDocked, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.Scan)
            {
                msg = eddn.CreateEDDNMessage(je as JournalScan, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.SAASignalsFound)
            {
                msg = eddn.CreateEDDNMessage(je as JournalSAASignalsFound, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.Outfitting)
            {
                msg = eddn.CreateEDDNOutfittingMessage(je as JournalOutfitting);
            }
            else if (je.EventTypeID == JournalTypeEnum.Shipyard)
            {
                msg = eddn.CreateEDDNShipyardMessage(je as JournalShipyard);
            }
            else if (je.EventTypeID == JournalTypeEnum.Market)
            {
                JournalMarket jm = je as JournalMarket;
                msg = eddn.CreateEDDNCommodityMessage(jm.Commodities, jm.IsOdyssey, jm.IsHorizons, jm.StarSystem, jm.Station, jm.MarketID, jm.EventTimeUTC);      // if its devoid of data, null returned
            }
            else if (je.EventTypeID == JournalTypeEnum.EDDCommodityPrices)
            {
                JournalEDDCommodityPrices jm = je as JournalEDDCommodityPrices;
                msg = eddn.CreateEDDNCommodityMessage(jm.Commodities, jm.IsOdyssey, jm.IsHorizons, jm.StarSystem, jm.Station, jm.MarketID, jm.EventTimeUTC);      // if its devoid of data, null returned
            }
            else if (je.EventTypeID == JournalTypeEnum.FSSDiscoveryScan)
            {
                msg = eddn.CreateEDDNFSSDiscoveryScan(je as JournalFSSDiscoveryScan, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.CodexEntry)
            {
                msg = eddn.CreateEDDNCodexEntry(je as JournalCodexEntry, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.NavBeaconScan)
            {
                msg = eddn.CreateEDDNNavBeaconScan(je as JournalNavBeaconScan, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.NavRoute)
            {
                msg = eddn.CreateEDDNNavRoute(je as JournalNavRoute, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.ScanBaryCentre)
            {
                msg = eddn.CreateEDDNScanBaryCentre(je as JournalScanBaryCentre, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.ApproachSettlement)
            {
               // msg = eddn.CreateEDDNApproachSettlement(je as JournalApproachSettlement, he.System);      // hold off until on main server
            }
            else if (je.EventTypeID == JournalTypeEnum.FSSAllBodiesFound)
            {
                // msg = eddn.CreateEDDNFSSAllBodiesFound(je as JournalFSSAllBodiesFound, he.System);
            }

            if (msg != null)
            {
                if (sendtotest) // make sure it looks fresh if send to test
                    msg["message"]["timestamp"] = DateTime.UtcNow.ToStringZuluInvariant();

                if (eddn.PostMessage(msg) )
                {
                    he.journalEntry.SetEddnSync();
                    return true;
                }
                else
                    return false;
            }
            else
                return null;
        }

    }
}
