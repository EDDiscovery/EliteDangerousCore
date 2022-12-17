/*
 * Copyright 2016-2021 EDDiscovery development team
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
using System.Data.Common;
using System.Linq;
using SQLLiteExtensions;

namespace EliteDangerousCore.DB
{
    public class SQLiteConnectionUser : SQLExtConnectionRegister
    {
        public SQLiteConnectionUser() : base(EliteDangerousCore.EliteConfigInstance.InstanceOptions.UserDatabasePath, utctimeindicator:true)
        {
        }

        // will throw on error, cope with it.
        // returns 0 DB was good, else version number
        public int UpgradeUserDB()
        {
            int dbver = RegisterClass.GetSetting("DBVer", (int)1);        // use the constring one, as don't want to go back into ConnectionString code. Int is to force type

            const int lastver = 130;

            if (dbver < lastver)
            {
                DropOldUserTables();

                if (dbver < 2)
                    UpgradeUserDB2();

                if (dbver < 4)
                    UpgradeUserDB4();

                if (dbver < 7)
                    UpgradeUserDB7();

                if (dbver < 10)
                    UpgradeUserDB10();

                if (dbver < 12)
                    UpgradeUserDB12();

                if (dbver < 16)
                    UpgradeUserDB16();

                if (dbver < 102)
                    UpgradeUserDB102();

                if (dbver < 103)
                    UpgradeUserDB103();

                if (dbver < 104)
                    UpgradeUserDB104();

                if (dbver < 105)
                    UpgradeUserDB105();

                if (dbver < 106)
                    UpgradeUserDB106();

                if (dbver < 107)
                    UpgradeUserDB107();

                if (dbver < 108)
                    UpgradeUserDB108();

                if (dbver < 110)
                    UpgradeUserDB110();

                if (dbver < 115)
                    UpgradeUserDB115();

                if (dbver < 116)
                    UpgradeUserDB116();

                if (dbver < 117)
                    UpgradeUserDB117();

                if (dbver < 118)
                    UpgradeUserDB118();

                if (dbver < 119)
                    UpgradeUserDB119();

                if (dbver < 120)
                    UpgradeUserDB120();

                if (dbver < 121)
                    UpgradeUserDB121();

                if (dbver < 122)
                    UpgradeUserDB122();

                if (dbver < 123)
                    UpgradeUserDB123();

                if (dbver < 126)
                    UpgradeUserDB126();

                if (dbver < 127)
                    UpgradeUserDB127();

                if (dbver < 128)
                    UpgradeUserDB128();

                if (dbver < 129)
                    UpgradeUserDB129();
                
                UpgradeUserDB130();

                CreateUserDBTableIndexes();

                return lastver;
            }
            else
                return 0;
        }

        private void UpgradeUserDB2()
        {
            string query = "CREATE TABLE SystemNote (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , Name TEXT NOT NULL , Time DATETIME NOT NULL )";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB4()
        {
            string query = "ALTER TABLE SystemNote ADD COLUMN Note TEXT";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB7()
        {
            string query = "CREATE TABLE TravelLogUnit(id INTEGER PRIMARY KEY  NOT NULL, type INTEGER NOT NULL, name TEXT NOT NULL, size INTEGER, path TEXT)";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB10()
        {
            string query = "CREATE TABLE wanted_systems (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, systemname TEXT UNIQUE NOT NULL)";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB12()
        {
            string query1 = "CREATE TABLE routes_expeditions (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, name TEXT UNIQUE NOT NULL, start DATETIME, end DATETIME)";
            string query2 = "CREATE TABLE route_systems (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, routeid INTEGER NOT NULL, systemname TEXT NOT NULL)";
            ExecuteNonQueries(query1, query2);
        }


        private void UpgradeUserDB16()
        {
            string query = "CREATE TABLE Bookmarks (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , StarName TEXT, x double NOT NULL, y double NOT NULL, z double NOT NULL, Time DATETIME NOT NULL, Heading TEXT, Note TEXT NOT Null )";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB102()
        {
            string query = "CREATE TABLE Commanders (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, EdsmApiKey TEXT NOT NULL, NetLogDir TEXT, Deleted INTEGER NOT NULL)";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB103()
        {
            string query = "CREATE TABLE JournalEntries ( " +
                 "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                 "JournalId INTEGER NOT NULL REFERENCES Journals(Id), " +
                 "EventTypeId INTEGER NOT NULL, " +
                 "EventType TEXT, " +
                 "EventTime DATETIME NOT NULL, " +
                 "EventData TEXT, " + //--JSON String of complete line" +
                 "EdsmId INTEGER, " + //--0 if not set yet." +
                 "Synced INTEGER " +
                 ")";

            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB104()
        {
            string query = "ALTER TABLE SystemNote ADD COLUMN journalid Integer NOT NULL DEFAULT 0";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB105()
        {
            string query1 = "ALTER TABLE TravelLogUnit ADD COLUMN CommanderId INTEGER REFERENCES Commanders(Id) ";
            string query2 = "DROP TABLE IF EXISTS JournalEntries";
            string query3 = "CREATE TABLE JournalEntries ( " +
                 "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                 "TravelLogId INTEGER NOT NULL REFERENCES TravelLogUnit(Id), " +
                 "CommanderId INTEGER NOT NULL DEFAULT 0," +
                 "EventTypeId INTEGER NOT NULL, " +
                 "EventType TEXT, " +
                 "EventTime DATETIME NOT NULL, " +
                 "EventData TEXT, " + //--JSON String of complete line" +
                 "EdsmId INTEGER, " + //--0 if not set yet." +
                 "Synced INTEGER " +
                 ")";

            ExecuteNonQueries(query1, query2, query3);
        }

        private void UpgradeUserDB106()
        {
            string query = "ALTER TABLE SystemNote ADD COLUMN EdsmId INTEGER NOT NULL DEFAULT -1";
            ExecuteNonQuery(query);
        }
        private void UpgradeUserDB107()
        {
            string query1 = "ALTER TABLE Commanders ADD COLUMN SyncToEdsm INTEGER NOT NULL DEFAULT 1";
            string query2 = "ALTER TABLE Commanders ADD COLUMN SyncFromEdsm INTEGER NOT NULL DEFAULT 0";
            string query3 = "ALTER TABLE Commanders ADD COLUMN SyncToEddn INTEGER NOT NULL DEFAULT 1";
            ExecuteNonQueries(query1, query2, query3);
        }

        private void UpgradeUserDB108()
        {
            string query = "ALTER TABLE Commanders ADD COLUMN JournalDir TEXT";
            ExecuteNonQuery(query);
            try
            {
                List<int> commandersToMigrate = new List<int>();
                using (DbCommand cmd = CreateCommand("SELECT Id, NetLogDir, JournalDir FROM Commanders"))
                {
                    using (DbDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            int nr = Convert.ToInt32(rdr["Id"]);
                            object netlogdir = rdr["NetLogDir"];
                            object journaldir = rdr["JournalDir"];

                            if (netlogdir != DBNull.Value && journaldir == DBNull.Value)
                            {
                                string logdir = Convert.ToString(netlogdir);

                                if (logdir != null && System.IO.Directory.Exists(logdir) && System.IO.Directory.EnumerateFiles(logdir, "journal*.log").Any())
                                {
                                    commandersToMigrate.Add(nr);
                                }
                            }
                        }
                    }
                }

                using (DbCommand cmd2 = CreateCommand("UPDATE Commanders SET JournalDir=NetLogDir WHERE Id=@Nr"))
                {
                    cmd2.AddParameter("@Nr", System.Data.DbType.Int32);

                    foreach (int nr in commandersToMigrate)
                    {
                        cmd2.Parameters["@Nr"].Value = nr;
                        cmd2.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("UpgradeUser108 exception: " + ex.Message);
            }
        }


        private void UpgradeUserDB110()
        {
            string query1 = "ALTER TABLE Commanders ADD COLUMN EdsmName TEXT";
            ExecuteNonQueries(query1);
        }

        private void UpgradeUserDB115()
        {
            string query1 = "ALTER TABLE Commanders ADD COLUMN SyncToEGO INT NOT NULL DEFAULT 0";
            string query2 = "ALTER TABLE Commanders ADD COLUMN EGOName TEXT";
            string query3 = "ALTER TABLE Commanders ADD COLUMN EGOAPIKey TEXT";
            ExecuteNonQueries(query1, query2, query3);
        }

        private void UpgradeUserDB116()
        {
            string query = "ALTER TABLE Bookmarks ADD COLUMN PlanetMarks TEXT DEFAULT NULL";
            ExecuteNonQuery(query);
        }
        private void UpgradeUserDB117()
        {
            string query = "ALTER TABLE routes_expeditions ADD COLUMN Status INT DEFAULT 0";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB118()
        {
            string query1 = "ALTER TABLE Commanders ADD COLUMN SyncToInara INT NOT NULL DEFAULT 0";
            string query2 = "ALTER TABLE Commanders ADD COLUMN InaraAPIKey TEXT";
            ExecuteNonQueries(query1, query2);
        }

        private void UpgradeUserDB119()
        {
            string query = "ALTER TABLE Commanders ADD COLUMN InaraName TEXT";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB120()
        {
            string query = "CREATE TABLE CaptainsLog ( " +
                "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                "Commander INTEGER NOT NULL, " +
                "Time DATETIME NOT NULL, " +
                "SystemName TEXT NOT NULL COLLATE NOCASE, " +
                "BodyName TEXT NOT NULL COLLATE NOCASE, " +
                "Note TEXT NOT NULL, " +
                "Tags TEXT DEFAULT NULL, " +
                "Parameters TEXT DEFAULT NULL" +
                ") ";

            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB121()
        {
            string query = "ALTER TABLE Commanders ADD COLUMN HomeSystem TEXT";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB122()
        {
            string query1 = "ALTER TABLE Commanders ADD COLUMN MapColour INT";
            string query2 = "ALTER TABLE Commanders ADD COLUMN MapCentreOnSelection INT";
            string query3 = "ALTER TABLE Commanders ADD COLUMN MapZoom REAL";
            ExecuteNonQueries(query1, query2, query3);
        }

        private void UpgradeUserDB123()
        {
            string query = "ALTER TABLE Commanders ADD COLUMN SyncToIGAU INTEGER NOT NULL DEFAULT 0";
            ExecuteNonQuery(query);
        }

        private void UpgradeUserDB126()         // Oh dear put this back for backwards compat
        {
            string query1 = "DROP TABLE IF EXISTS MaterialsCommodities";
            string query2 = "CREATE TABLE MaterialsCommodities ( " +
                "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                "Category TEXT NOT NULL, " +
                "Name TEXT NOT NULL COLLATE NOCASE, " +
                "FDName TEXT NOT NULL COLLATE NOCASE DEFAULT '', " +
                "Type TEXT NOT NULL COLLATE NOCASE, " +
                "ShortName TEXT NOT NULL COLLATE NOCASE DEFAULT '', " +
                "Flags INT NOT NULL DEFAULT 0, " +
                "Colour INT NOT NULL DEFAULT 15728640, " +
                "UNIQUE(Category,Name)" +
                ") ";
            ExecuteNonQueries(query1,query2);
        }

        private void UpgradeUserDB127()
        {
            string query = "ALTER TABLE Commanders ADD COLUMN Options TEXT NOT NULL DEFAULT \"{}\"";
            ExecuteNonQuery(query);
        }
        private void UpgradeUserDB128()
        {
            string query1 = "ALTER TABLE TravelLogUnit ADD COLUMN GameVersion TEXT DEFAULT \"\"";
            string query2 = "ALTER TABLE TravelLogUnit ADD COLUMN Build TEXT DEFAULT \"\"";
            ExecuteNonQueries(query1, query2);
        }
        private void UpgradeUserDB129()
        {
            string query1 = "ALTER TABLE SystemNote ADD COLUMN UTCTime DATETIME";
            string query2 = "ALTER TABLE SystemNote ADD COLUMN JournalText TEXT DEFAULT \"\"";
            ExecuteNonQueries(query1, query2);
        }
        private void UpgradeUserDB130()
        {
            string query1 = "ALTER TABLE route_systems ADD COLUMN note TEXT DEFAULT \"\"";
            ExecuteNonQueries(query1);
        }

        private void DropOldUserTables()
        {
            string[] queries = new[]
            {
                "DROP TABLE IF EXISTS Systems",
                "DROP TABLE IF EXISTS SystemAliases",
                "DROP TABLE IF EXISTS Distances",
                "DROP TABLE IF EXISTS Stations",
                "DROP TABLE IF EXISTS station_commodities",
                "DROP TABLE IF EXISTS Journals",
                "DROP TABLE IF EXISTS VisitedSystems",
                "DROP TABLE IF EXISTS Objects",
            };

            ExecuteNonQueries(queries);
        }

        public void CreateUserDBTableIndexes()
        {
            string[] queries = new[]
            {
                "CREATE INDEX IF NOT EXISTS TravelLogUnit_Name ON TravelLogUnit (Name)",
                "CREATE INDEX IF NOT EXISTS TravelLogUnit_Commander ON TravelLogUnit(CommanderId)",

                "CREATE INDEX IF NOT EXISTS JournalEntry_CommanderId ON JournalEntries (CommanderId)",
                "CREATE INDEX IF NOT EXISTS JournalEntry_TravelLogId ON JournalEntries (TravelLogId)",
                "CREATE INDEX IF NOT EXISTS JournalEntry_EventTypeId ON JournalEntries (EventTypeId)",
                "CREATE INDEX IF NOT EXISTS JournalEntry_EventType ON JournalEntries (EventType)",
                "CREATE INDEX IF NOT EXISTS JournalEntry_EventTime ON JournalEntries (EventTime)",
            };

            ExecuteNonQueries(queries);
        }

        public void DropUserDBTableIndexes()
        {
            string[] queries = new[]
            {
                "DROP INDEX IF EXISTS TravelLogUnit_Name",
                "DROP INDEX IF EXISTS TravelLogUnit_Commander",

                "DROP INDEX IF EXISTS JournalEntry_CommanderId",
                "DROP INDEX IF EXISTS JournalEntry_TravelLogId",
                "DROP INDEX IF EXISTS JournalEntry_EventTypeId",
                "DROP INDEX IF EXISTS JournalEntry_EventType",
                "DROP INDEX IF EXISTS JournalEntry_EventTime",
            };

            ExecuteNonQueries(queries);
        }

        public void ClearJournal()
        {
            string[] queries = new[]
            {
                "DELETE from JournalEntries",
                "DELETE FROM TravelLogUnit",
            };

            ExecuteNonQueries(queries);
        }
        public void ClearCommanderTable()
        {
            string[] queries = new[]
            {
                "DELETE from Commanders",
            };

            ExecuteNonQueries(queries);
        }
    }
}
