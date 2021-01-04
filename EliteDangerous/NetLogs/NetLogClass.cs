/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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
using EliteDangerousCore.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Common;
using BaseUtils.JSON;

namespace EliteDangerousCore
{
    public static class NetLogClass
    {
        static public void ParseFiles(string datapath, out string error, int defaultMapColour,
                                     Func<bool> cancelRequested, Action<int, string> updateProgress,
                                     bool forceReload, int currentcmdrid)
        {
            error = null;

            if (datapath == null)
            {
                error = "Netlog directory not set!";
                return;
            }

            if (!Directory.Exists(datapath))   // if logfiles directory is not found
            {
                error = "Netlog directory is not present!";
                return;
            }

            // list of systems in journal, sorted by time
            List<JournalLocOrJump> vsSystemsEnts = JournalEntry.GetAll(currentcmdrid).OfType<JournalLocOrJump>().OrderBy(j => j.EventTimeUTC).ToList();

            // order by file write time so we end up on the last one written
            FileInfo[] allFiles = Directory.EnumerateFiles(datapath, "netLog.*.log", SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

            List<NetLogFileReader> readersToUpdate = new List<NetLogFileReader>();
            List<TravelLogUnit> tlutoadd = new List<TravelLogUnit>();

            for (int i = 0; i < allFiles.Length; i++)
            {
                FileInfo fi = allFiles[i];

                var reader = OpenFileReader(fi.FullName, currentcmdrid);

                if (reader.ID == 0)            // if not present, add to commit add list
                {
                    tlutoadd.Add(reader.TravelLogUnit);
                }

                if (forceReload)        // Force a reload of the travel log
                {
                    reader.Pos = 0;
                }

                if (reader.Pos != fi.Length || i == allFiles.Length - 1)  // File not already in DB, or is the last one
                {
                    readersToUpdate.Add(reader);
                }
            }

            if (tlutoadd.Count > 0)                      // now, on spinning rust, this takes ages for 600+ log files first time, so transaction it
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbTransaction txn = cn.Connection.BeginTransaction())
                    {
                        foreach (var tlu in tlutoadd)
                        {
                            tlu.Add(cn.Connection, txn);
                        }

                        txn.Commit();
                    }
                });
            }

            for (int i = 0; i < readersToUpdate.Count; i++)
            {
                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    int ji = 0;

                    NetLogFileReader reader = readersToUpdate[i];
                    updateProgress(i * 100 / readersToUpdate.Count, reader.TravelLogUnit.FullName);

                    var systems = JournalEntry.GetAllByTLU(reader.ID, cn.Connection).OfType<JournalLocOrJump>().ToList();
                    var last = systems.LastOrDefault();     // find last system recorded for this TLU, may be null if no systems..

                    using (DbTransaction tn = cn.Connection.BeginTransaction())
                    {
                        var ienum = reader.ReadSystems(last, cancelRequested, currentcmdrid);
                        System.Diagnostics.Debug.WriteLine("Scanning TLU " + reader.ID + " " + reader.FullName);

                        foreach (JObject jo in ienum)
                        {
                            jo["EDDMapColor"] = defaultMapColour;

                            JournalLocOrJump je = new JournalFSDJump(jo);
                            je.SetTLUCommander(reader.TravelLogUnit.ID, currentcmdrid);

                            while (ji < vsSystemsEnts.Count && vsSystemsEnts[ji].EventTimeUTC < je.EventTimeUTC)
                            {
                                ji++;   // move to next entry which is bigger in time or equal to ours.
                            }

                            JournalLocOrJump prev = (ji > 0 && (ji - 1) < vsSystemsEnts.Count) ? vsSystemsEnts[ji - 1] : null;
                            JournalLocOrJump next = ji < vsSystemsEnts.Count ? vsSystemsEnts[ji] : null;

                            bool previssame = (prev != null && prev.StarSystem.Equals(je.StarSystem, StringComparison.CurrentCultureIgnoreCase) && (!prev.HasCoordinate || !je.HasCoordinate || (prev.StarPos - je.StarPos).LengthSquared < 0.01));
                            bool nextissame = (next != null && next.StarSystem.Equals(je.StarSystem, StringComparison.CurrentCultureIgnoreCase) && (!next.HasCoordinate || !je.HasCoordinate || (next.StarPos - je.StarPos).LengthSquared < 0.01));

                            // System.Diagnostics.Debug.WriteLine("{0} {1} {2}", ji, vsSystemsEnts[ji].EventTimeUTC, je.EventTimeUTC);

                            if (!(previssame || nextissame))
                            {
                                je.Add(jo, cn.Connection, tn);
                                System.Diagnostics.Debug.WriteLine("Add {0} {1}", je.EventTimeUTC, jo.ToString());
                            }
                        }

                        reader.TravelLogUnit.Update(cn.Connection, tn);

                        tn.Commit();
                    }

                    if (updateProgress != null)
                    {
                        updateProgress((i + 1) * 100 / readersToUpdate.Count, reader.TravelLogUnit.FullName);
                    }
                });
            }
        }

        private static NetLogFileReader OpenFileReader(string filepath, int cmdrid )
        {
            NetLogFileReader reader;

            if (TravelLogUnit.TryGet(filepath, out TravelLogUnit tlu))
            {
                reader = new NetLogFileReader(tlu);
            }
            else
            {
                reader = new NetLogFileReader(filepath);
                reader.TravelLogUnit.Type = TravelLogUnit.NetLogType;
                reader.TravelLogUnit.CommanderId = cmdrid;
            }

            return reader;
        }

   
    }
}
