﻿/*
 * Copyright 2016-2023 EDDiscovery development team
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
using System.Windows.Forms;

namespace EliteDangerousCore.DB
{
    // basic access to get and put settings
    public interface IUserDatabaseSettingsSaver
    {
        T GetSetting<T>(string key, T defaultvalue, bool writebackifdefault = false);
        bool PutSetting<T>(string key, T value);

        bool DGVLoadColumnLayout(System.Windows.Forms.DataGridView dgv, string auxname = "", bool rowheaderselection = false);
        void DGVSaveColumnLayout(System.Windows.Forms.DataGridView dgv, string auxname = "");
    }

    public class UserDatabase : SQLAdvProcessingThread<SQLiteConnectionUser>, IUserDatabaseSettingsSaver
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

        // number of keys deleted
        public int DeleteKey(string key)
        {
            return DBWrite(db =>  db.RegisterClass.DeleteKey(key));
        }

        public T GetSetting<T>(string key, T defaultvalue, bool writebackifdefault = false)
        {
            bool usedef = false;

            T value = DBRead(db => { return db.RegisterClass.GetSetting(key, defaultvalue, out usedef); });

            if ( usedef && writebackifdefault )
            {
                DBWrite(db => db.RegisterClass.PutSetting(key, defaultvalue));
            }
            return value;
        }

        public bool PutSetting<T>(string key, T value)
        {
            return DBWrite(db => db.RegisterClass.PutSetting(key, value));
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
                db.Vacuum();
            });
        }
        public void ClearCommanderTable()
        {
            DBWrite(db =>
            {
                db.ClearCommanderTable();
            });
        }

        // should not use these directly, always thru another class
        public bool DGVLoadColumnLayout(DataGridView dgv, string auxname = "", bool rowheaderselection = false)
        {
            throw new NotImplementedException();
        }


        public void DGVSaveColumnLayout(DataGridView dgv, string auxname = "")
        {
            throw new NotImplementedException();
        }
    }


    // instance this class and you can pass the class for saving settings with a defined rootname

    public class UserDatabaseSettingsSaver : IUserDatabaseSettingsSaver     
    {                                        
        public UserDatabaseSettingsSaver(IUserDatabaseSettingsSaver b, string rootname)
        {
            root = rootname;
            ba = b;
        }
        public T GetSetting<T>(string key, T defaultvalue, bool writebackifdefault = false)
        {
            return ba.GetSetting(root + key, defaultvalue, writebackifdefault);
        }

        public bool PutSetting<T>(string key, T value)
        {
            return ba.PutSetting(root + key, value);
        }

        public bool DGVLoadColumnLayout(DataGridView dgv, string auxname = "", bool rowheaderselection = false)
        {
            return dgv.LoadColumnSettings(root + "_DGV_" + auxname, rowheaderselection,
                                        (a) => EliteDangerousCore.DB.UserDatabase.Instance.GetSetting(a, int.MinValue),
                                        (b) => EliteDangerousCore.DB.UserDatabase.Instance.GetSetting(b, double.MinValue));
        }


        public void DGVSaveColumnLayout(DataGridView dgv, string auxname = "")
        {
            dgv.SaveColumnSettings(root + "_DGV_" + auxname,
                                        (a, b) => EliteDangerousCore.DB.UserDatabase.Instance.PutSetting(a, b),
                                        (c, d) => EliteDangerousCore.DB.UserDatabase.Instance.PutSetting(c, d));
        }


        private string root;
        private IUserDatabaseSettingsSaver ba;
    }


}
