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

using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EliteDangerousCore.DB
{
    public class SystemsDatabase : SQLAdvProcessingThread<SQLiteConnectionSystem>
    {
        private SystemsDatabase(bool walmode)
        {
            RWLocks = (walmode == false);       // if walmode is off, we need reader/writer locks
            System.Diagnostics.Debug.WriteLine($"Make new system DB RWLocks = {RWLocks}");
        }

        public static bool WALMode { get; set; } = false;
        public string DBSource { get; private set; } = "Unknown";
        public bool HasStarType { get { return DBSource == "SPANSH"; } }
        public bool HasSystemAddresses { get { return DBSource == "SPANSH"; } }
        public bool RebuildRunning { get; private set; } = true;                // we are rebuilding until we have the system db table in there
        public HashSet<long> PermitSystems { get; private set; }                             // list of permit systems

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

        private static SystemsDatabase instance;

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
                RebuildRunning = false;
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

        const string TempTablePostfix = "temp"; // postfix for temp tables



        // this deletes the current DB data, reloads from the file, and recreates the indexes etc

        public long MakeSystemTableFromFile(string filename, bool[] gridids, int blocksize, System.Threading.CancellationToken cancelRequested, Action<string> reportProgress,
                                            string debugoutputfile = null, int method = 0)
        {
            DBWrite(action: conn =>
            {
                conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                conn.CreateStarTables(TempTablePostfix);     // and make new temp tables
            });

            long updates = 0;
            //if (method == 0)
            //{
            //    DateTime maxdate = DateTime.MinValue;
            //    updates = SystemsDB.ParseJSONFile(filename, gridids, blocksize, ref maxdate, cancelRequested, reportProgress, TempTablePostfix, true, debugoutputfile);
            //    SetLastRecordTimeUTC(maxdate);          // record last data stored in database
            //}
            //else if (method == 1)
            //{
            //    SystemsDB.Loader1 loader = new SystemsDB.Loader1(TempTablePostfix, blocksize, gridids, true, debugoutputfile);   // overlap write
            //    updates = loader.ParseJSONFile(filename, cancelRequested, reportProgress);
            //    loader.Finish();
            //}
            //else if (method == 2)
            //{
            //    SystemsDB.Loader2 loader = new SystemsDB.Loader2(TempTablePostfix, blocksize, gridids, true, debugoutputfile);   // overlap write
            //    updates = loader.ParseJSONFile(filename, cancelRequested, reportProgress);
            //    loader.Finish();
            //}
            //else 
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
                    RebuildRunning = true;

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

                    RebuildRunning = false;
                });

                System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} System DB Made");
                ClearDownRestart();             // tables have changed, clear all connections down

                PermitSystems = SystemsDB.GetPermitSystems();       // refresh permit systems

                return updates;
            }
            else
            {
                DBWrite(action: conn =>
                {
                    conn.DropStarTables(TempTablePostfix);     // clean out half prepared tables
                });

                return -1;
            }
        }

        public void RemoveGridSystems(int[] gridids, Action<string> report = null)
        {
            RebuildRunning = true;
            SystemsDB.RemoveGridSystems(gridids, report);
            RebuildRunning = false;
        }

        public long StoreSystems(IEnumerable<ISystem> systems)            // dynamically update db
        {
            long count = 0;
            if (!RebuildRunning)
            {
                count = SystemsDB.StoreSystems(systems);
            }

            return count;
        }
        public void RebuildIndexes(Action<string> logger)
        {
            if (!RebuildRunning)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    RebuildRunning = true;

                    DBWrite((conn) =>
                    {
                        logger?.Invoke("Removing indexes");
                        conn.DropSystemDBTableIndexes();
                        logger?.Invoke("Rebuilding indexes, please wait");
                        conn.CreateSystemDBTableIndexes();
                        logger?.Invoke("Indexes rebuilt");
                    });

                    RebuildRunning = false;
                });
            }
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
