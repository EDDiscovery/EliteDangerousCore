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

using SQLLiteExtensions;
using System;

namespace EliteDangerousCore.DB
{
    public class UserDatabase : SQLAdvProcessingThread<SQLiteConnectionUser>
    {
        private UserDatabase()
        {
        }

        public static UserDatabase Instance { get; } = new UserDatabase();

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
                dbno = cn.UpgradeUserDB(); 
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

        }

        protected override SQLiteConnectionUser CreateConnection()
        {
            return new SQLiteConnectionUser();
        }

        // Register

        public bool KeyExists(string key)
        {
            return DBRead(db => db.RegisterClass.keyExists(key));
        }

        public bool DeleteKey(string key)
        {
            return DBWrite(db =>  db.RegisterClass.DeleteKey(key));
        }

        public T GetSetting<T>(string key, T defaultvalue)
        {
            return DBRead(db => db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSetting<T>(string key, T defaultvalue)
        {
            return DBWrite(db => db.RegisterClass.PutSetting(key, defaultvalue));
        }

        public int GetSettingInt(string key, int defaultvalue)
        {
            return DBRead(db => db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingInt(string key, int intvalue)
        {
            return DBWrite(db =>  db.RegisterClass.PutSetting(key, intvalue));
        }

        public double GetSettingDouble(string key, double defaultvalue)
        {
            return DBRead(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDouble(string key, double doublevalue)
        {
            return DBWrite(db =>  db.RegisterClass.PutSetting(key, doublevalue));
        }

        public bool GetSettingBool(string key, bool defaultvalue)
        {
            return DBRead(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingBool(string key, bool boolvalue)
        {
            return DBWrite(db =>  db.RegisterClass.PutSetting(key, boolvalue));
        }

        public string GetSettingString(string key, string defaultvalue)
        {
            return DBRead(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingString(string key, string strvalue)
        {
            return DBWrite(db =>  db.RegisterClass.PutSetting(key, strvalue));
        }

        public DateTime GetSettingDate(string key, DateTime defaultvalue)
        {
            return DBRead(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDate(string key, DateTime value)
        {
            return DBWrite(db =>  db.RegisterClass.PutSetting(key, value));
        }

        public void RebuildIndexes(Action<string> logger)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                DBWrite(db =>
                {
                    logger?.Invoke("Removing indexes");
                    db.DropUserDBTableIndexes();
                    logger?.Invoke("Rebuilding indexes, please wait");
                    db.CreateUserDBTableIndexes();
                    logger?.Invoke("Indexes rebuilt");
                });
            });
        }

        public void ClearJournals()
        {
            DBWrite(db =>
            {
                db.ClearJournal();
            });
        }
    }
}
