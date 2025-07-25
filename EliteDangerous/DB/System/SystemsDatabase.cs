/*
 * Copyright 2015-2023 EDDiscovery development team
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

using EliteDangerousCore.EDSM;
using QuickJSON;
using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;

namespace EliteDangerousCore.DB
{
    public class SystemsDatabase : SQLAdvProcessingThread<SQLiteConnectionSystem>
    {
        private SystemsDatabase(bool walmode)
        {
            RWLocks = (walmode == false);       // if walmode is off, we need reader/writer locks
        }

        public static bool WALMode { get; set; } = false;
        public string DBSource { get; private set; } = "Unknown";
        public bool HasStarType { get { return DBSource == "SPANSH"; } }
        public bool HasSystemAddresses { get { return DBSource == "SPANSH"; } }
        public bool DBUpdating { get; private set; } = true;                // we are updating the DB (unlikely to be wrong, but maybe due to race condition it may be just before a lock is taken)
        public HashSet<long> PermitSystems { get; private set; }            // list of permit systems

        public bool IsPermitSystem(ISystem s)
        {
            return HasSystemAddresses ? PermitSystems.Contains(s.SystemAddress ?? -1) : PermitSystems.Contains(s.EDSMID ?? -1);
        }

        public static SystemsDatabase Instance
        {
            get
            {
                if (instance == null)
                    instance = new SystemsDatabase(WALMode);
                return instance;
            }
        }

        private static SystemsDatabase instance;        // the DB instance

        object updatelocker = new object();     // ensure DB writing is not in parallel in all circumstances

        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem(RWLocks == true ? SQLExtConnection.JournalModes.DELETE : SQLExtConnection.JournalModes.WAL);
        }

        public static void Reset()
        {
            Instance?.Stop();
            instance = new SystemsDatabase(WALMode);
        }

        // will throw on error, cope with it.
        public void Initialize()
        {
            bool registrycreated = false;
            DBWrite(cn => { registrycreated = cn.CreateRegistry(); });

            if (registrycreated)
                ClearDownRestart();         // to stop the schema problem

            int dbno = 0;

            DBWrite(cn =>
            {
                dbno = cn.UpgradeSystemsDB();
                DBUpdating = false;
            });

            if (dbno > 0)
            {
                ClearDownRestart();         // to stop the schema problem
                DBWrite(cn =>
                {
                    SQLExtRegister reg = new SQLExtRegister(cn);
                    reg.PutSetting("DBVer", dbno);
                });
            }

            DBSource = GetDBSource();           // get what was set up and cache

            if ( DBSource == "EDSM")
            {
                SystemsDB.Remove(81517618); // test render
                SystemsDB.Remove(81496114); //single light test
                SystemsDB.Remove(81517627); //test
                SystemsDB.Remove(81498438);
            }
            else
            {
                SystemsDB.Remove(4099286239595);
            }

            PermitSystems = SystemsDB.GetPermitSystems();
        }


        // this deletes the current DB data, reloads from the file, and recreates the indexes etc
        public long CreateSystemDBFromJSONFile(string filename, bool[] gridids, int blocksize, System.Threading.CancellationToken cancelRequested, Action<string> reportProgress,
                                            string debugoutputfile = null, int method = 0)
        {
            lock (updatelocker)     // lock the DB update procedure 
            {
                const string TempTablePostfix = "temp"; // postfix for temp tables

                DBUpdating = true;

                DBWrite(action: conn =>
                {
                    conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                    conn.CreateStarTables(TempTablePostfix);     // and make new temp tables
                });

                long updates = 0;

                if (method == 3)
                {
                    SystemsDB.Loader3 loader = new SystemsDB.Loader3(TempTablePostfix, blocksize, gridids, true, false, debugoutputfile);   // overlap write with insert or replace
                    updates = loader.ParseJSONFile(filename, cancelRequested, reportProgress);
                    loader.Finish(cancelRequested);
                }
                else
                    System.Diagnostics.Debug.Assert(false);

                if (updates > 0)
                {
                    DBWrite(action: conn =>
                    {

                        System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Removing old data");
                        reportProgress?.Invoke("Remove old data");
                        conn.DropStarTables();     // drop the main ones - this also kills the indexes

                        System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Renaming tables");

                        conn.RenameStarTables(TempTablePostfix, "");     // rename the temp to main ones

                        System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Shrinking DB");
                        reportProgress?.Invoke("Shrinking database");
                        conn.Vacuum();

                        conn.WALCheckPoint(SQLExtConnection.CheckpointType.TRUNCATE);        // perform a WAL checkpoint to clean up the WAL file as the vacuum will have done stuff

                        System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Creating indexes");
                        reportProgress?.Invoke("Creating indexes");
                        conn.CreateSystemDBTableIndexes();

                        conn.WALCheckPoint(SQLExtConnection.CheckpointType.TRUNCATE);        // perform a WAL checkpoint to clean up the WAL file

                    });

                    System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} System DB Made");
                    ClearDownRestart();             // tables have changed, clear all connections down

                    PermitSystems = SystemsDB.GetPermitSystems();       // refresh permit systems

                    DBUpdating = false;
                    return updates;
                }
                else
                {
                    DBWrite(action: conn =>
                    {
                        conn.DropStarTables(TempTablePostfix);     // clean out half prepared tables
                    });

                    DBUpdating = false;
                    return -1;
                }
            }
        }

        
        public bool RemoveGridSystems(int[] gridids, Action<string> report = null)
        {
            if (!DBUpdating) // don't bother if something else is updating the DB
            {
                lock (updatelocker)
                {
                    DBUpdating = true;
                    SystemsDB.RemoveGridSystems(gridids, report);
                    DBUpdating = false;
                    return true;
                }
            }
            else
                return false;
        }

        // dynamically update db with found systems
        public long StoreSystems(IEnumerable<ISystem> systems)            
        {
            if (!DBUpdating)        // don't bother if something else is updating the DB.  This will in the majority catch the fact something else is running
            {
                lock (updatelocker) // but we lock the update to ensure - this could stall the thread but most of the time the above will catch it
                {
                    JArray jlist = new JArray();

                    string currentdb = SystemsDatabase.Instance.DBSource;
                    bool spansh = currentdb.Equals("SPANSH");

                    foreach (var sys in systems)
                    {
                        // so we need coords, and if edsm db, we need an edsm id, or for spansh we need a system address 
                        if (sys.HasCoordinate && ((!spansh && sys.EDSMID.HasValue) || (spansh && sys.SystemAddress.HasValue)))
                        {
                            JObject jo = new JObject
                            {
                                ["name"] = sys.Name,
                                ["coords"] = new JObject { ["x"] = sys.X, ["y"] = sys.Y, ["z"] = sys.Z }
                            };

                            if (spansh)       // we format either for a spansh DB or an EDSM db
                            {
                                jo["id64"] = sys.SystemAddress.Value;
                                jo["updateTime"] = DateTime.UtcNow;
                            }
                            else
                            {
                                jo["id"] = sys.EDSMID.Value;
                                jo["date"] = DateTime.UtcNow;
                            }

                            System.Diagnostics.Debug.WriteLine($"DB Store systems {jo.ToString()}");
                            jlist.Add(jo);
                        }
                    }

                    if (jlist.Count > 0)
                    {
                        // start loader, 10000 at a time, no overlapped so we don't load up the pc, and don't overwrite stuff already there

                        DBUpdating = true;

                        var cancel = new System.Threading.CancellationToken(); //can't be cancelled
                        SystemsDB.Loader3 loader3 = new SystemsDB.Loader3("", 10000, null, poverlapped: false, pdontoverwrite: true);
                        long updates = loader3.ParseJSONString(jlist.ToString(), cancel, (s) => System.Diagnostics.Debug.WriteLine($"Store Systems: {s}"));
                        loader3.Finish(cancel);

                        DBUpdating = false;
                        return updates;
                    }
                }
            }

            return 0;
        }

        // incrememental update from file
        public long UpdateSpanshSystemsFromJSONFile(string downloadfile, bool[] grids, CancellationToken PendingClose, Action<string> ReportSyncProgress)
        {
            lock (updatelocker)
            {
                DBUpdating = true;

                SystemsDB.Loader3 loader3 = new SystemsDB.Loader3("", 50000, grids, true, false);
                long count = loader3.ParseJSONFile(downloadfile, PendingClose, ReportSyncProgress);
                loader3.Finish(PendingClose);

                DBUpdating = false;
                return count;
            }
        }

        // incremental update from EDSM WEB
        public long UpdateEDSMSystemsFromWeb(bool[] grididallow, CancellationToken cancel, Action<string> ReportProgress, Action<string> LogLine, int ForceEDSMFullDownloadDays )
        {
            lock (updatelocker)
            {
                const int EDSMUpdateFetchHours = 12;           // for an update fetch, its these number of hours at a time (Feb 2021 moved to 6 due to EDSM new server)

                DBUpdating = true;

                // smallish block size, non overlap, allow overwrite
                SystemsDB.Loader3 loader3 = new SystemsDB.Loader3("", 50000, grididallow, false, false);

                DateTime maximumupdatetimewindow = DateTime.UtcNow.AddDays(-ForceEDSMFullDownloadDays);        // limit download to this amount of days
                if (loader3.LastDate < maximumupdatetimewindow)
                    loader3.LastDate = maximumupdatetimewindow;               // this stops crazy situations where somehow we have a very old date but the full sync did not take care of it

                long updates = 0;

                double fetchmult = 1;

                DateTime minimumfetchspan = DateTime.UtcNow.AddHours(-EDSMUpdateFetchHours / 2);        // we don't bother fetching if last record time is beyond this point

                while (loader3.LastDate < minimumfetchspan)                              // stop at X mins before now, so we don't get in a condition
                {                                                                           // where we do a set, the time moves to just before now, 
                                                                                            // and we then do another set with minimum amount of hours
                    if (cancel.IsCancellationRequested)
                        break;

                    if (updates == 0)
                        LogLine("Checking for updated EDSM systems (may take a few moments).");

                    EDSMClass edsm = new EDSMClass();

                    double hourstofetch = EDSMUpdateFetchHours;        //EDSM new server feb 2021, more capable, 

                    DateTime enddate = loader3.LastDate + TimeSpan.FromHours(hourstofetch * fetchmult);
                    if (enddate > DateTime.UtcNow)
                        enddate = DateTime.UtcNow;

                    LogLine($"Downloading systems from UTC {loader3.LastDate} to {enddate}");
                    System.Diagnostics.Debug.WriteLine($"Downloading systems from UTC {loader3.LastDate} to {enddate} {hourstofetch}");

                    string json = null;
                    BaseUtils.HttpCom.Response response;
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        response = edsm.RequestSystemsData(loader3.LastDate, enddate, timeout: 20000);
                        fetchmult = Math.Max(0.1, Math.Min(Math.Min(fetchmult * 1.1, 1.0), 5000.0 / sw.ElapsedMilliseconds));
                    }
                    catch (WebException ex)
                    {
                        ReportProgress($"EDSM request failed");
                        if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null && ex.Response is HttpWebResponse)
                        {
                            string status = ((HttpWebResponse)ex.Response).StatusDescription;
                            LogLine($"Download of EDSM systems from the server failed ({status}), will try next time program is run");
                        }
                        else
                        {
                            LogLine($"Download of EDSM systems from the server failed ({ex.Status.ToString()}), will try next time program is run");
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        ReportProgress($"EDSM request failed");
                        LogLine($"Download of EDSM systems from the server failed ({ex.Message}), will try next time program is run");
                        break;
                    }

                    if (response.Error)
                    {
                        if ((int)response.StatusCode == 429)
                        {
                            LogLine($"EDSM rate limit hit - waiting 2 minutes");
                            for (int sec = 0; sec < 120; sec++)
                            {
                                if (!cancel.IsCancellationRequested)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
                        }
                        else
                        {
                            LogLine($"Download of EDSM systems from the server failed ({response.StatusCode.ToString()}), will try next time program is run");
                            break;
                        }
                    }
                    json = response.Body;

                    if (json == null)
                    {
                        ReportProgress("EDSM request failed");
                        LogLine("Download of EDSM systems from the server failed (no data returned), will try next time program is run");
                        break;
                    }

                    // debug File.WriteAllText(@"c:\code\json.txt", json);

                    ReportProgress($"EDSM star database update from UTC " + loader3.LastDate.ToString());

                    var prevrectime = loader3.LastDate;

                    long updated = loader3.ParseJSONString(json, cancel, ReportProgress);

                    System.Diagnostics.Trace.WriteLine($"EDSM partial download updated {updated} to {loader3.LastDate}");

                    // if lastrecordtime did not change (=) or worse still, EDSM somehow moved the time back (unlikely)
                    if (loader3.LastDate <= prevrectime)
                    {
                        loader3.LastDate += TimeSpan.FromHours(12);       // Lets move on manually so we don't get stuck
                    }

                    updates += updated;

                    int delay = 10;     // Anthor's normal delay 
                    int ratelimitlimit;
                    int ratelimitremain;
                    int ratelimitreset;

                    if (response.Headers != null &&
                        response.Headers["X-Rate-Limit-Limit"] != null &&
                        response.Headers["X-Rate-Limit-Remaining"] != null &&
                        response.Headers["X-Rate-Limit-Reset"] != null &&
                        Int32.TryParse(response.Headers["X-Rate-Limit-Limit"], out ratelimitlimit) &&
                        Int32.TryParse(response.Headers["X-Rate-Limit-Remaining"], out ratelimitremain) &&
                        Int32.TryParse(response.Headers["X-Rate-Limit-Reset"], out ratelimitreset))
                    {
                        if (ratelimitremain < ratelimitlimit * 3 / 4)       // lets keep at least X remaining for other purposes later..
                            delay = ratelimitreset / (ratelimitlimit - ratelimitremain);    // slow down to its pace now.. example 878/(360-272) = 10 seconds per quota
                        else
                            delay = 0;

                        System.Diagnostics.Debug.WriteLine("EDSM Delay Parameters {0} {1} {2} => {3}s", ratelimitlimit, ratelimitremain, ratelimitreset, delay);
                    }

                    for (int sec = 0; sec < delay; sec++)
                    {
                        if (!cancel.IsCancellationRequested)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }

                loader3.Finish(cancel);

                DBUpdating = false;
                return updates;
            }
        }

        public bool RebuildIndexes(Action<string> logger)
        {
            if (!DBUpdating)
            {
                lock (updatelocker)        
                {
                    DBUpdating = true;
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        DBWrite((conn) =>
                        {
                            logger?.Invoke("Removing indexes");
                            conn.DropSystemDBTableIndexes();
                            logger?.Invoke("Rebuilding indexes, please wait");
                            conn.CreateSystemDBTableIndexes();
                            logger?.Invoke("Indexes rebuilt");
                        });

                    });

                    DBUpdating = false;
                    return true;
                }
            }
            else
                return false;
        }

        public void WALCheckPoint()                // full commit if in WAL mode, NOP if not https://www.sqlite.org/pragma.html
        {
            DBWrite((cn) => cn.WALCheckPoint(SQLLiteExtensions.SQLExtConnection.CheckpointType.TRUNCATE));
        }

        public string GetGridIDs()
        {
            return DBRead(db => db.RegisterClass.GetSetting("EDSMGridIDs", "Not Set"));        // keep old name for compatibility
        }

        public bool SetGridIDs(string value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("EDSMGridIDs", value));      // use old name
        }

        public DateTime GetEDSMGalMapLast()
        {
            return DBRead(db => db.RegisterClass.GetSetting("EDSMGalMapLast", DateTime.MinValue));
        }

        public bool SetEDSMGalMapLast(DateTime value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("EDSMGalMapLast", value));
        }
        public DateTime GetGECGalMapLast()
        {
            return DBRead(db => db.RegisterClass.GetSetting("GECGalMapLast", DateTime.MinValue));
        }

        public bool SetGECGalMapLast(DateTime value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("GECGalMapLast", value));
        }
        public bool SetDBSource(string name)
        {
            DBSource = name;
            return DBWrite((db) => db.RegisterClass.PutSetting("DBSource", name));
        }
        private string GetDBSource()
        {
            return DBRead((db) => db.RegisterClass.GetSetting("DBSource", "EDSM"));
        }

        #region Time markers

        // time markers - keeping the old code for now, not using better datetime funcs

        public void ForceFullUpdate()
        {
            DBWrite((db) => db.RegisterClass.PutSetting("EDSMLastSystems", "2010-01-01 00:00:00"));        // use old name
        }

        public DateTime GetLastRecordTimeUTC()
        {
            return DBRead(db =>
            {
                string rwsystime = db.RegisterClass.GetSetting("EDSMLastSystems", "2000-01-01 00:00:00"); // Latest time from RW file. Use old name
                DateTime edsmdate;

                if (!DateTime.TryParse(rwsystime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out edsmdate))
                    edsmdate = EliteReleaseDates.GammaStart;

                return edsmdate;
            });
        }

        public void SetLastRecordTimeUTC(DateTime time)
        {
            DBWrite(db =>
            {
                db.RegisterClass.PutSetting("EDSMLastSystems", time.ToString(CultureInfo.InvariantCulture));    // use old name
                System.Diagnostics.Debug.WriteLine("Last EDSM record " + time.ToString());
            });
        }

        public int GetMaxSectorID()        // what is the maximum sector id in use
        {
            return DBRead(db =>
            {
                long v = db.MaxIdOf("Sectors", "id");
                return (int)v;
            });
        }

        #endregion

        #region Check

        public bool VerifyTablesExist()
        {
            bool res = DBRead(db => {
                var tlist = db.Tables();
                return tlist.Contains("SystemTable") && tlist.Contains("Names") && tlist.Contains("Sectors") && tlist.Contains("Register");
            });

            return res;
        }

        #endregion
    }
}


#if false

        internal static void ExplainPlans(SQLiteConnectionSystem cn, int limit = int.MaxValue)
        {
            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.nameid >= @p1 AND s.nameid <= @p2 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p3)",
                                                new Object[] { 1, 2, "name" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                    "(s.nameid & (1<<46) != 0) AND cast((s.nameid & 0x3fffffffff) as text) LIKE @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { 10.ToStringInvariant() + "%", "name" },
                                                    limit: limit,
                                                    joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                 "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                 new Object[] { "Sol" + "%", 12 },
                                                 limit: limit,
                                                 joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                  "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                  new Object[] { "wkwk" + "%" },
                                                  limit: limit,
                                                  joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }


            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) ",
                                                new Object[] { "kwk" + "%" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                 "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                 new Object[] { "Sol" + "%" },
                                                 limit: limit,
                                                 joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            System.Diagnostics.Debug.Write("NEW!");

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.edsmid IN (Select id FROM Names WHERE name LIKE @p1)",
                                                new Object[] { "kwk" + "%" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

        }
#endif

