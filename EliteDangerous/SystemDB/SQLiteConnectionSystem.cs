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

namespace EliteDangerousCore.DB
{
    public class SQLiteConnectionSystem : SQLExtConnectionRegister
    {
        public SQLiteConnectionSystem(JournalModes journalmode) : base(EliteDangerousCore.EliteConfigInstance.InstanceOptions.SystemDatabasePath, utctimeindicator: true, journalmode: journalmode)
        {
        }

        #region Init

        const string tablepostfix = "temp"; // postfix for temp tables

        // will throw on error
        // returns 0 DB was good, else version number
        public int UpgradeSystemsDB()
        {
            // BE VERY careful with connections when creating/deleting tables - you end up with SQL Schema errors or it not seeing the table

            SQLExtRegister reg = new SQLExtRegister(this);
            int dbver = reg.GetSetting("DBVer", (int)0);      // use reg, don't use the built in func as they create new connections and confuse the schema
            int dborg = dbver;

            if (dbver < 210)    // less than 210, delete the lot and start again
            {
                ExecuteNonQueries(new string[]                  // always set up
                    {
                    "DROP TABLE IF EXISTS Distances",
                    "DROP TABLE IF EXISTS EddbSystems",
                    // keep edsmsystems
                    "DROP TABLE IF EXISTS Stations",
                    "DROP TABLE IF EXISTS SystemAliases",
                    // don't drop Systemnames
                    "DROP TABLE IF EXISTS station_commodities",
                    "CREATE TABLE IF NOT EXISTS Aliases (edsmid INTEGER PRIMARY KEY NOT NULL, edsmid_mergedto INTEGER, name TEXT COLLATE NOCASE)"
                    });

                var tablesql = this.SQLMasterQuery("table");
                int index = tablesql.FindIndex(x => x.TableName == "Systems");
                bool oldsystems = index >= 0 && tablesql[index].SQL.Contains("commandercreate");

                if (dbver < 200 || oldsystems)
                {
                    ExecuteNonQueries(new string[]             // always kill these old tables 
                    {
                    "DROP TABLE IF EXISTS Systems", // New! this is an hold over which never got deleted when we moved to the 102 schema
                    });
                }

                if (dbver < 200)                           // is it older than 200, now unusable.  Removed 102-200 reformat code as its been long enough
                {
                    ExecuteNonQueries(new string[]         // older than 102, not supporting, remove
                    {
                        "DROP TABLE IF EXISTS EdsmSystems",
                        "DROP TABLE IF EXISTS SystemNames",
                    });

                    reg.DeleteKey("EDSMLastSystems");       // no EDSM system time
                }

                CreateStarTables();                     // ensure we have
                CreateSystemDBTableIndexes();           // ensure they are there 
                DropStarTables(tablepostfix);           // clean out any temp tables half prepared 

                dbver = 212;                            // this makes the whole schebang up to 212
            }

            if (dbver < 211)                            // if we had 210, but not 211, we need to add a new column in but keep the data
            {
                ExecuteNonQueries(new string[]            
                    {
                        "ALTER TABLE Systems ADD COLUMN info INT DEFAULT NULL",
                        "ALTER TABLE Systems RENAME TO SystemTable",
                    });

                dbver = 212;
            }

            if (dbver < 212)                            // if we had 211, then rename the systems to systemtable from now on, to cause previous versions to break on this DB
            {
                ExecuteNonQueries(new string[]            
                    {
                        "ALTER TABLE Systems RENAME TO SystemTable",
                    });

                CreateSystemDBTableIndexes();           // ensure they are there 
                dbver = 212;
            }

            if (dbver < 213)                            // if not 213, add the permit systems table
            {
                ExecuteNonQueries(new string[]            
                    {
                        "CREATE TABLE IF NOT EXISTS PermitSystems (edsmid INTEGER PRIMARY KEY NOT NULL)",
                    });

                CreateSystemDBTableIndexes();           // ensure they are there 
                dbver = 213;
            }

            return dbver != dborg ? dbver : 0;
        }

        #endregion

        #region Helpers

        public void CreateStarTables(string postfix = "")
        {
            ExecuteNonQueries(new string[]
            {
                // purposely not using autoincrement or unique on primary keys - this slows it down.

                "CREATE TABLE IF NOT EXISTS Sectors" + postfix + " (id INTEGER PRIMARY KEY NOT NULL, gridid INTEGER, name TEXT NOT NULL COLLATE NOCASE)",
                "CREATE TABLE IF NOT EXISTS SystemTable" + postfix + " (edsmid INTEGER PRIMARY KEY NOT NULL , sectorid INTEGER, nameid INTEGER, x INTEGER, y INTEGER, z INTEGER, info INTEGER DEFAULT NULL)",
                "CREATE TABLE IF NOT EXISTS Names" + postfix + " (id INTEGER PRIMARY KEY NOT NULL , Name TEXT NOT NULL COLLATE NOCASE )",
                "CREATE TABLE IF NOT EXISTS PermitSystems" + postfix + " (edsmid INTEGER PRIMARY KEY NOT NULL)",
            });
        }

        public void DropStarTables(string postfix = "")
        {
            ExecuteNonQueries(new string[]
            {
                "DROP TABLE IF EXISTS Sectors" + postfix,       // dropping the tables kills the indexes
                "DROP TABLE IF EXISTS SystemTable" + postfix,
                "DROP TABLE IF EXISTS Names" + postfix,
                "DROP TABLE IF EXISTS PermitSystems" + postfix,
            });
        }

        public void RenameStarTables(string frompostfix, string topostfix)
        {
            ExecuteNonQueries(new string[]
            {
                "ALTER TABLE Sectors" + frompostfix + " RENAME TO Sectors" + topostfix,
                "ALTER TABLE SystemTable" + frompostfix + " RENAME TO SystemTable" + topostfix,
                "ALTER TABLE Names" + frompostfix + " RENAME TO Names" + topostfix,
                "ALTER TABLE PermitSystems" + frompostfix + " RENAME TO PermitSystems" + topostfix,
            });
        }

        public void CreateSystemDBTableIndexes()
        {
            string[] queries = new[]
            {
                "CREATE INDEX IF NOT EXISTS SystemsSectorName ON SystemTable (sectorid,nameid)",        // worth it for lookups of stars
                "CREATE INDEX IF NOT EXISTS SystemsXZY ON SystemTable (x,z,y)",        // speeds up searching. 
               
                "CREATE INDEX IF NOT EXISTS NamesName ON Names (Name)",            // improved speed from 9038 (named)/1564 (std) to 516/446ms at minimal cost

                "CREATE INDEX IF NOT EXISTS SectorName ON Sectors (name)",         // name - > entry
                "CREATE INDEX IF NOT EXISTS SectorGridid ON Sectors (gridid)",     // gridid -> entry
            };

            ExecuteNonQueries(queries);
        }

        public void DropSystemDBTableIndexes()
        {
            string[] queries = new[]
            {
                "DROP INDEX IF EXISTS SystemsSectorName",        // worth it for lookups of stars
                "DROP INDEX IF EXISTS SystemsXZY",        // speeds up searching. 
               
                "DROP INDEX IF EXISTS NamesName",            // improved speed from 9038 (named)/1564 (std) to 516/446ms at minimal cost

                "DROP INDEX IF EXISTS SectorName",         // name - > entry
                "DROP INDEX IF EXISTS SectorGridid",     // gridid -> entry
            };

            ExecuteNonQueries(queries);

        }

        #endregion

    }
}

