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
        private SystemsDatabase()
        {
        }

        public static SystemsDatabase Instance { get; private set; } = new SystemsDatabase();        //STATIC constructor, make once at start of program

        public static void Reset()
        {
            Instance?.Stop();
            Instance = new SystemsDatabase();
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

            if ( dbno > 0 )
            {
                ClearDownRestart();         // to stop the schema problem
                DBWrite(cn =>
                {
                    SQLExtRegister reg = new SQLExtRegister(cn);
                    reg.PutSetting("DBVer", dbno);
                });
            }
        }

        const string TempTablePostfix = "temp"; // postfix for temp tables

        public bool RebuildRunning { get; private set; } = true;                // we are rebuilding until we have the system db table in there

        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem();
        }

        // this deletes the current DB data, reloads from the file, and recreates the indexes etc

        public long MakeSystemTableFromFile(string filename, bool[] gridids, Func<bool> cancelRequested, Action<string> reportProgress, string debugoutputfile = null)
        {
            DBWrite( action: conn =>
            {
                conn.DropStarTables(TempTablePostfix);     // just in case, kill the old tables
                conn.CreateStarTables(TempTablePostfix);     // and make new temp tables
            });

            DateTime maxdate = DateTime.MinValue;
            long updates = SystemsDB.ParseJSONFile(filename, gridids, 500000, ref maxdate, cancelRequested, reportProgress, TempTablePostfix, true, debugoutputfile);

            if (updates > 0)
            {
                DBWrite(action: conn =>
                {
                    RebuildRunning = true;

                    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} Removing old data");
                    reportProgress?.Invoke("Remove old data");
                    conn.DropStarTables();     // drop the main ones - this also kills the indexes

                    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} Renaming tables");

                    conn.RenameStarTables(TempTablePostfix, "");     // rename the temp to main ones

                    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} Shrinking DB");
                    reportProgress?.Invoke("Shrinking database");
                    conn.Vacuum();

                    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} Creating indexes");
                    reportProgress?.Invoke("Creating indexes");
                    conn.CreateSystemDBTableIndexes();

                    RebuildRunning = false;
                });

                ClearDownRestart();             // tables have changed, clear all connections down

                SetLastRecordTimeUTC(maxdate);          // record last data stored in database

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

        public long StoreSystems( IEnumerable<ISystem> systems)            // dynamically update db
        {
            long count = 0;
            if (!RebuildRunning)
            {
                System.Diagnostics.Debug.WriteLine("********************** STORE SYSTEMS NEEDS REWORKING");
                count = SystemsDB.StoreSystems(systems);
            }

            return count;
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

        public string GetGridIDs()
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMGridIDs", "Not Set"));        // keep old name for compatibility
        }

        public bool SetGridIDs(string value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("EDSMGridIDs", value));      // use old name
        }

        public DateTime GetEDSMGalMapLast()
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMGalMapLast", DateTime.MinValue));
        }

        public bool SetEDSMGalMapLast(DateTime value)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("EDSMGalMapLast", value));
        }
        public bool SetDBSource(string name)
        {
            return DBWrite((db) => db.RegisterClass.PutSetting("DBSource", name));
        }
        public string GetDBSource()
        {
            return DBRead((db) => db.RegisterClass.GetSetting("DBSource", "EDSM"));
        }

        #region Time markers

        // time markers - keeping the old code for now, not using better datetime funcs

        public void ForceFullUpdate()
        {
            DBWrite( (db) => db.RegisterClass.PutSetting("EDSMLastSystems", "2010-01-01 00:00:00"));        // use old name
        }

        public DateTime GetLastRecordTimeUTC()
        {
            return DBRead( db =>
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
            DBWrite( db =>
            {
                db.RegisterClass.PutSetting("EDSMLastSystems", time.ToString(CultureInfo.InvariantCulture));    // use old name
                System.Diagnostics.Debug.WriteLine("Last EDSM record " + time.ToString());
            });
        }

        public int GetSectorIDNext()        // what is the next allocate sector ID to use?
        {
            return DBRead( db => db.RegisterClass.GetSetting("EDSMSectorIDNext", 1));       // use old name
        }

        public void SetSectorIDNext(int val)
        {
            DBWrite(db => db.RegisterClass.PutSetting("EDSMSectorIDNext", val));
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
