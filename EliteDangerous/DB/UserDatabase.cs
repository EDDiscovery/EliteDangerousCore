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
    public class UserDatabaseConnection : IDisposable
    {
        internal SQLiteConnectionUser Connection { get; private set; }

        public UserDatabaseConnection()
        {
            Connection = new SQLiteConnectionUser();
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }

    public class UserDatabase : SQLProcessingThread<UserDatabaseConnection>
    {
        private UserDatabase()
        {
        }

        public static UserDatabase Instance { get; } = new UserDatabase();

        public void Initialize()
        {
            ExecuteWithDatabase(cn => { cn.Connection.UpgradeUserDB(); });
        }

        protected override UserDatabaseConnection CreateConnection()
        {
            return new UserDatabaseConnection();
        }

        // Register

        public bool KeyExists(string key)
        {
            return ExecuteWithDatabase(db => db.Connection.RegisterClass.keyExists(key));
        }

        public bool DeleteKey(string key)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.DeleteKey(key));
        }

        public T GetSetting<T>(string key, T defaultvalue)
        {
            return ExecuteWithDatabase(db => db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSetting<T>(string key, T defaultvalue)
        {
            return ExecuteWithDatabase(db => db.Connection.RegisterClass.PutSetting(key, defaultvalue));
        }

        public int GetSettingInt(string key, int defaultvalue)
        {
            return ExecuteWithDatabase(db => db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingInt(string key, int intvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.PutSetting(key, intvalue));
        }

        public double GetSettingDouble(string key, double defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDouble(string key, double doublevalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.PutSetting(key, doublevalue));
        }

        public bool GetSettingBool(string key, bool defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingBool(string key, bool boolvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.PutSetting(key, boolvalue));
        }

        public string GetSettingString(string key, string defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingString(string key, string strvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.PutSetting(key, strvalue));
        }

        public DateTime GetSettingDate(string key, DateTime defaultvalue)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.GetSetting(key, defaultvalue));
        }

        public bool PutSettingDate(string key, DateTime value)
        {
            return ExecuteWithDatabase(db =>  db.Connection.RegisterClass.PutSetting(key, value));
        }

        public void RebuildIndexes(Action<string> logger)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ExecuteWithDatabase(db =>
                {
                    logger?.Invoke("Removing indexes");
                    db.Connection.DropUserDBTableIndexes();
                    logger?.Invoke("Rebuilding indexes, please wait");
                    db.Connection.CreateUserDBTableIndexes();
                    logger?.Invoke("Indexes rebuilt");
                });
            });
        }


    }
}
