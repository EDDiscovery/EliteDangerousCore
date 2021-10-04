using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Data;
using SQLLiteExtensions;
using System.Threading;
using EliteDangerousCore.DB;

namespace EliteDangerousCore.DB
{
    public class UserDatabase : SQLProcessingThread<SQLiteConnectionUser>
    {
        private UserDatabase()
        {
        }

        public static UserDatabase Instance { get; } = new UserDatabase();

        public void Initialize()
        {
            ExecuteWithDatabase(cn => { cn.UpgradeUserDB(); });
        }

        protected override SQLiteConnectionUser CreateConnection()
        {
            return new SQLiteConnectionUser();
        }

        // Register

        public bool KeyExists(string key)
        {
            return ExecuteWithDatabase(db => db.RegisterClass.keyExists(key));
        }

        public bool DeleteKey(string key)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.DeleteKey(key));
        }

        public T GetSetting<T>(string key, T defaultvalue)
        {
            return ExecuteWithDatabase(db => db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSetting<T>(string key, T defaultvalue)
        {
            return ExecuteWithDatabase(db => db.RegisterClass.PutSetting(key, defaultvalue));
        }

        public int GetSettingInt(string key, int defaultvalue)
        {
            return ExecuteWithDatabase(db => db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingInt(string key, int intvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.PutSetting(key, intvalue));
        }

        public double GetSettingDouble(string key, double defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDouble(string key, double doublevalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.PutSetting(key, doublevalue));
        }

        public bool GetSettingBool(string key, bool defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingBool(string key, bool boolvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.PutSetting(key, boolvalue));
        }

        public string GetSettingString(string key, string defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingString(string key, string strvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.PutSetting(key, strvalue));
        }

        public DateTime GetSettingDate(string key, DateTime defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDate(string key, DateTime value)
        {
            return ExecuteWithDatabase(db =>  db.RegisterClass.PutSetting(key, value));
        }

        public void RebuildIndexes(Action<string> logger)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ExecuteWithDatabase(db =>
                {
                    logger?.Invoke("Removing indexes");
                    db.DropUserDBTableIndexes();
                    logger?.Invoke("Rebuilding indexes, please wait");
                    db.CreateUserDBTableIndexes();
                    logger?.Invoke("Indexes rebuilt");
                });
            });
        }


    }
}
