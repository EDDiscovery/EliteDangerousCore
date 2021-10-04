/*
 * Copyright 2015-2021 EDDiscovery development team
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

using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EliteDangerousCore.DB
{
    public class SystemsDatabase : SQLAdvProcessingThread<SQLiteConnectionSystem>
    {
        private SystemsDatabase()
        {
        }

        public static SystemsDatabase Instance { get; } = new SystemsDatabase();        //STATIC constructor, make once at start of program

        public void Initialize()
        {
            DBWrite(cn => 
            {
                cn.UpgradeSystemsDB();
                RebuildRunning = false;
            });
        }

        const string TempTablePostfix = "temp"; // postfix for temp tables
        //const string DebugOutfile = @"c:\code\edsm\Jsonprocess.lst";        // null off
        const string DebugOutfile = null;

        public bool RebuildRunning { get; private set; } = true;                // we are rebuilding until we have the system db table in there

        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem();
        }

        public long UpgradeSystemTableFromFile(string filename, bool[] gridids, Func<bool> cancelRequested, Action<string> reportProgress)
        {
            DBWrite( action: conn =>
            {
                conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                conn.CreateStarTables(TempTablePostfix);     // and make new temp tables
            });

            DateTime maxdate = DateTime.MinValue;
            long updates = SystemsDB.ParseEDSMJSONFile(filename, gridids, ref maxdate, cancelRequested, reportProgress, TempTablePostfix, presumeempty: true, debugoutputfile: DebugOutfile);

            if (updates > 0)
            {
                DBWrite(action: conn =>
                {
                    RebuildRunning = true;

                    reportProgress?.Invoke("Remove old data");
                    conn.DropStarTables();     // drop the main ones - this also kills the indexes

                    conn.RenameStarTables(TempTablePostfix, "");     // rename the temp to main ones

                    reportProgress?.Invoke("Shrinking database");
                    conn.Vacuum();

                    reportProgress?.Invoke("Creating indexes");
                    conn.CreateSystemDBTableIndexes();

                    RebuildRunning = false;
                });

                SetLastEDSMRecordTimeUTC(maxdate);          // record last data stored in database

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

        public long StoreSystems( List<ISystem> systems)            // dynamically update db
        {
            long count = 0;
            if (!RebuildRunning)
            {
                RebuildRunning = true;
                count = SystemsDB.StoreSystems(systems);
                RebuildRunning = false;
            }

            return count;
        }

        public void UpgradeSystemTableFrom102TypeDB(Func<bool> cancelRequested, Action<string> reportProgress, bool fullsyncrequested)
        {
            bool executeupgrade = false;

            // first work out if we can upgrade, if so, create temp tables

            DBWrite(action: conn =>
            {
                var list = conn.Tables();    // this gets table list

                if (list.Contains("EdsmSystems"))
                {
                    conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                    conn.CreateStarTables(TempTablePostfix);     // and make new temp tables
                    executeupgrade = true;
                }
            });

            //drop connection, execute upgrade in another connection, this solves an issue with SQL 17 error

            if (executeupgrade)
            {
                if (!fullsyncrequested)     // if we did not request a full upgrade, we can use the current data and transmute
                {
                    int maxgridid = int.MaxValue;// 109;    // for debugging

                    long updates = SystemsDB.UpgradeDB102to200(cancelRequested, reportProgress, TempTablePostfix, tablesareempty: true, maxgridid: maxgridid);

                    DBWrite(action: conn =>
                    {
                        if (updates >= 0) // a cancel will result in -1
                        {
                            RebuildRunning = true;

                            // keep code for checking

                            //if (false)   // demonstrate replacement to show rows are overwitten and not duplicated in the edsmid column and that speed is okay
                            //{
                            //    long countrows = conn.CountOf("Systems" + tablepostfix, "edsmid");
                            //    long countnames = conn.CountOf("Names" + tablepostfix, "id");
                            //    long countsectors = conn.CountOf("Sectors" + tablepostfix, "id");

                            //    // replace takes : Sector 108 took 44525 U1 + 116 store 5627 total 532162 0.02061489 cumulative 11727

                            //    SystemsDB.UpgradeDB102to200(cancelRequested, reportProgress, tablepostfix, tablesareempty: false, maxgridid: maxgridid);
                            //    System.Diagnostics.Debug.Assert(countrows == conn.CountOf("Systems" + tablepostfix, "edsmid"));
                            //    System.Diagnostics.Debug.Assert(countnames * 2 == conn.CountOf("Names" + tablepostfix, "id"));      // names are duplicated.. so should be twice as much
                            //    System.Diagnostics.Debug.Assert(countsectors == conn.CountOf("Sectors" + tablepostfix, "id"));
                            //    System.Diagnostics.Debug.Assert(1 == conn.CountOf("Systems" + tablepostfix, "edsmid", "edsmid=6719254"));
                            //}

                            conn.DropStarTables();     // drop the main ones - this also kills the indexes

                            conn.RenameStarTables(TempTablePostfix, "");     // rename the temp to main ones

                            reportProgress?.Invoke("Removing old system tables");

                            conn.ExecuteNonQueries(new string[]
                            {
                                "DROP TABLE IF EXISTS EdsmSystems",
                                "DROP TABLE IF EXISTS SystemNames",
                            });

                            reportProgress?.Invoke("Shrinking database");
                            conn.Vacuum();

                            reportProgress?.Invoke("Creating indexes");         // NOTE the date should be the same so we don't rewrite
                            conn.CreateSystemDBTableIndexes();

                            RebuildRunning = false;
                        }
                        else
                        {
                            conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                        }
                    });
                }
                else
                {       // newer data is needed, so just remove
                    DBWrite( action: conn =>
                    {
                        reportProgress?.Invoke("Removing old system tables");

                        conn.ExecuteNonQueries(new string[]
                        {
                            "DROP TABLE IF EXISTS EdsmSystems",
                            "DROP TABLE IF EXISTS SystemNames",
                        });
                    });
                }
            }
        }

        public void RebuildIndexes(Action<string> logger )
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

        public string GetEDSMGridIDs()
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMGridIDs", "Not Set"));
        }

        public bool SetEDSMGridIDs(string value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("EDSMGridIDs", value));
        }

        public DateTime GetEDSMGalMapLast()
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMGalMapLast", DateTime.MinValue));
        }

        public bool SetEDSMGalMapLast(DateTime value)
        {
            return DBWrite( (db) => db.RegisterClass.PutSetting("EDSMGalMapLast", value));
        }

        #region Time markers

        // time markers - keeping the old code for now, not using better datetime funcs

        public void ForceEDSMFullUpdate()
        {
            DBWrite( (db) => db.RegisterClass.PutSetting("EDSMLastSystems", "2010-01-01 00:00:00"));
        }

        public DateTime GetLastEDSMRecordTimeUTC()
        {
            return DBRead( db =>
            {
                string rwsystime = db.RegisterClass.GetSetting("EDSMLastSystems", "2000-01-01 00:00:00"); // Latest time from RW file.
                DateTime edsmdate;

                if (!DateTime.TryParse(rwsystime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out edsmdate))
                    edsmdate = new DateTime(2000, 1, 1);

                return edsmdate;
            });
        }

        public void SetLastEDSMRecordTimeUTC(DateTime time)
        {
            DBWrite( db =>
            {
                db.RegisterClass.PutSetting("EDSMLastSystems", time.ToString(CultureInfo.InvariantCulture));
                System.Diagnostics.Debug.WriteLine("Last EDSM record " + time.ToString());
            });
        }

        public DateTime GetLastAliasDownloadTime()
        {
            return DBRead(db => db.RegisterClass.GetSetting("EDSMAliasLastDownloadTime", DateTime.MinValue));
        }

        public void SetLastEDSMAliasDownloadTime()
        {
            DBWrite(db => db.RegisterClass.PutSetting("EDSMAliasLastDownloadTime", DateTime.UtcNow));
        }

        public void ForceEDSMAliasFullUpdate()
        {
            DBWrite(db => db.RegisterClass.PutSetting("EDSMAliasLastDownloadTime", DateTime.MinValue));
        }

        public int GetEDSMSectorIDNext()
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMSectorIDNext", 1));
        }

        public void SetEDSMSectorIDNext(int val)
        {
            DBWrite(db => db.RegisterClass.PutSetting("EDSMSectorIDNext", val));
        }

        #endregion
    }
}
