/*
 * Copyright © 2016 - 2023 EDDiscovery development team
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

using EliteDangerousCore.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EliteDangerousCore
{
    public partial class HistoryList 
    {
        // for very old netlogs find positions
        // call if you want to ensure we have the best posibile position data on FSD Jumps.  Only occurs on pre 2.1 netlogs

        public void FillInPositionsFSDJumps(Action<string> logger)       
        {
            List<Tuple<HistoryEntry, ISystem>> updatesystems = new List<Tuple<HistoryEntry, ISystem>>();

            if (!SystemsDatabase.Instance.RebuildRunning)       // only run this when the system db is stable.. this prevents the UI from slowing to a standstill
            {
                foreach (HistoryEntry he in historylist)
                {
                    // try and load ones without position.. if its got pos we are happy.  If its 0,0,0 and its not sol, it may just be a stay entry

                    if (he.IsFSDCarrierJump)
                    {
                        //logger?.Invoke($"Checking system {he.System.Name}");

                        if (!he.System.HasCoordinate || (Math.Abs(he.System.X) < 1 && Math.Abs(he.System.Y) < 1 && Math.Abs(he.System.Z) < 0 && he.System.Name != "Sol"))
                        {
                            ISystem found = SystemCache.FindSystem(he.System, true);        // find, thru edsm if required

                            if (found != null)
                            {
                                logger?.Invoke($"System {he.System.Name} found system in EDSM");
                                updatesystems.Add(new Tuple<HistoryEntry, ISystem>(he, found));
                            }
                            else
                                logger?.Invoke($"System {he.System.Name} failed to find system in EDSM");
                        }
                    }
                }
            }

            if (updatesystems.Count > 0)
            {
                UserDatabase.Instance.DBWrite(cn =>
                {
                    using (DbTransaction txn = cn.BeginTransaction())        // take a transaction over this
                    {
                        foreach (Tuple<HistoryEntry, ISystem> hesys in updatesystems)
                        {
                            logger?.Invoke($"Update position of {hesys.Item1.System.Name} at {hesys.Item1.EntryNumber} in journal");
                            hesys.Item1.journalEntry.UpdateStarPosition(hesys.Item2, cn, txn);
                            hesys.Item1.UpdateSystem(hesys.Item2);
                        }

                        txn.Commit();
                    }
                });
            }
        }

        // call repeatedly to fill up historyentry.ScanNode to the top of history, used by surveyor/discoveries
        // scan node link is not set on history creation for speed reasons

        private int lastfilled = 0;
        public void FillInScanNode()        
        {
            while( lastfilled < historylist.Count)
            {
                var he = historylist[lastfilled];
                if ( he.EntryType == JournalTypeEnum.Scan && he.ScanNode == null)
                {
                    // if we have two scans of the same body, the starscan always replaces the journal scan with the latest.
                    // maybe we should use bodyid

                    var sysnode = StarScan.FindSystemSynchronous(he.System, false);                 // prob not null, but check

                    if ( sysnode != null)
                    {
                        var js = he.journalEntry as JournalEvents.JournalScan;
                        if (js.BodyID.HasValue && sysnode.NodesByID.TryGetValue(js.BodyID.Value, out StarScan.ScanNode sn))     // find by bodyid
                            he.ScanNode = sn;
                        else
                        {
                            var jscan = sysnode?.Find(he.journalEntry as JournalEvents.JournalScan);        // else find by journal entry
                            he.ScanNode = jscan;
                        }

                        if ( he.ScanNode == null )
                            System.Diagnostics.Debug.WriteLine($"Fill Scan Node failed for {he.System}");
                    }
                }
                lastfilled++;
            }
        }

        // use when you change start/stop, to recalculate the travel status.  from index to the end

        public void RecalculateTravel(int index)
        {
            for( int i = index; i < historylist.Count; i++)
            {
                historylist[i].UpdateTravelStatus(i > 0 ? historylist[i - 1] : null);
            }
        }

    }
}
