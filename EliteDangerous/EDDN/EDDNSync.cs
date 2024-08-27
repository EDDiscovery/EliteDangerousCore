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
        private static ManualResetEvent Exit = new ManualResetEvent(false);

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

            // Start the sync thread if it's not already running
            if (ThreadEDDNSync == null )
            {
                ThreadEDDNSync = new System.Threading.Thread(new System.Threading.ThreadStart(SyncThread));
                ThreadEDDNSync.Name = "EDDN Sync";
                ThreadEDDNSync.IsBackground = true;
                ThreadEDDNSync.Start();
            }

            return true;
        }

        public static void StopSync()
        {
            if ( ThreadEDDNSync != null )       // may never have started
            {
                Exit.Set();                     // but if so, set to trigger thread to stop, and join
                ThreadEDDNSync.Join();
            }
        }

        private static void SyncThread()
        {
            System.Diagnostics.Debug.WriteLine("EDDN thread active");

            while( Exit.WaitOne(5000) == false )        // while not told to exit.. if its set, its an exit. Else we wake up every N ms to process
            {
                int eventcount = 0;

                while (hlscanunsyncedlist.TryDequeue(out HistoryEntry he))   // if we have an HE, process it
                {
                    if (Exit.WaitOne(0) == true)     // if signalled, immediate stop
                        return;

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
                            System.Diagnostics.Debug.WriteLine($"EDDN Send of {he.EventTimeUTC} {he.EventSummary}");
                            bool? res = EDDNSync.SendToEDDN(he);

                            if (res != null)    // if attempted to send
                            {
                                if (res.Value == true)
                                {
                                    logger?.Invoke($"[EDDN] Sent {he.EntryType.ToString()} event ({he.EventSummary})");
                                    eventcount++;
                                    Thread.Sleep(500);     // just a little pause between sending to space them out
                                }
                                else
                                    logger?.Invoke($"[EDDN] Failed sending {he.EntryType.ToString()} event ({he.EventSummary})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.Message);
                        System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.StackTrace);
                        logger?.Invoke("[EDDN] Sync exception " + ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                }   // end while

                if ( eventcount > 0) 
                    SentEvents?.Invoke(eventcount);     // tell the system we sent this number of events

            }   // around to wait for another timeperiod before testing again
        }

        // Send to test vectors it to the beta server
        static public bool? SendToEDDN(HistoryEntry he, bool sendtotest = false)
        {
            if (he.Commander == null || he.journalEntry == null)     // why ever? but protect since code in here did.  But its probably v.old code reasons not valid now
                return null;

            if ( he.journalEntry.Build.IsEmpty() || he.journalEntry.GameVersion.IsEmpty() )     // should never happen unless the game files are somehow corrupted and lacking fileheader/loadgame
            {
                System.Diagnostics.Trace.WriteLine("*** EDDN message not sent due to empty Build/GameVersion");
                return null;
            }

            EDDNClass eddn = new EDDNClass(he.Commander.Name);

            JournalEntry je = he.journalEntry;

            QuickJSON.JObject msg = null;

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
            else if (je.EventTypeID == JournalTypeEnum.Market)      // from the journal
            {
                // tbd
                JournalMarket jm = je as JournalMarket;
                msg = eddn.CreateEDDNCommodityMessage(jm.GameVersion,jm.Build, jm.Commodities, jm.IsOdyssey, jm.IsHorizons, jm.StarSystem, 
                    jm.Station, jm.FDStationType, jm.CarrierDockingAccess, jm.MarketID, jm.EventTimeUTC);      // if its devoid of data, null returned
            }
            else if (je.EventTypeID == JournalTypeEnum.EDDCommodityPrices)  // synthesised EDD
            {
                JournalEDDCommodityPrices jm = je as JournalEDDCommodityPrices;
                bool legacy = EDCommander.IsLegacyCommander(je.CommanderId);
                msg = eddn.CreateEDDNCommodityMessage( legacy ? "CAPI-Legacy-market" : "CAPI-Live-market", "", jm.Commodities, jm.IsOdyssey, jm.IsHorizons, jm.StarSystem, 
                            jm.Station, jm.FDStationType, jm.CarrierDockingAccess, jm.MarketID, jm.EventTimeUTC);      // if its devoid of data, null returned
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
                msg = eddn.CreateEDDNNavRoute(je as JournalNavRoute);
            }
            else if (je.EventTypeID == JournalTypeEnum.ScanBaryCentre)
            {
                msg = eddn.CreateEDDNScanBaryCentre(je as JournalScanBaryCentre, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.ApproachSettlement)
            {
                msg = eddn.CreateEDDNApproachSettlement(je as JournalApproachSettlement, he.System);      // hold off until on main server
            }
            else if (je.EventTypeID == JournalTypeEnum.FSSAllBodiesFound)
            {
                msg = eddn.CreateEDDNFSSAllBodiesFound(je as JournalFSSAllBodiesFound, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.FSSSignalDiscovered)
            {
                // if EDDNSystem is set, we use that, else use system
                var fss = je as JournalFSSSignalDiscovered;
                msg = eddn.CreateEDDNFSSSignalDiscovered(fss, fss.EDDNSystem != null ? fss.EDDNSystem : he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.FCMaterials)     
            {
                msg = eddn.CreateEDDNFCMaterials(je as JournalFCMaterials, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.DockingGranted)    
            {
                msg = eddn.CreateEDDNDockingGranted(je as JournalDockingGranted, he.System);
            }
            else if (je.EventTypeID == JournalTypeEnum.DockingDenied)      
            {
                msg = eddn.CreateEDDNDockingDenied(je as JournalDockingDenied, he.System);
            }

            if (msg != null)
            {
                if (sendtotest) // make sure it looks fresh if send to test
                    msg["message"]["timestamp"] = DateTime.UtcNow.ToStringZuluInvariant();

                bool beta = he.Commander.NameIsBeta || he.journalEntry.IsBeta;      // either beta it

                if (eddn.PostMessage(msg,beta,sendtotest))
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
