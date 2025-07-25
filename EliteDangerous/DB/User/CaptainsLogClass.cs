﻿/*
 * Copyright 2019-2021 EDDiscovery development team
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
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore.DB
{
    [System.Diagnostics.DebuggerDisplay("{ID} {Commander} {TimeUTC} {SystemName} {BodyName} {Tags}")]
    public class CaptainsLogClass
    {
        public long ID { get; private set; }
        public int Commander { get; private set; }
        public DateTime TimeUTC { get; private set; }
        public string SystemName { get; private set; }
        public string BodyName { get; private set; }
        public string Note { get; private set; }
        public string Tags { get; private set; }     // may be null, tag<separ>tag<separ>
        public string Parameters { get; private set; }     // may be null (not currently used)

        public CaptainsLogClass()
        {
        }

        public CaptainsLogClass(DbDataReader dr)
        {
            ID = (long)dr["Id"];
            Commander = (int)(long)dr["Commander"];
            TimeUTC = (DateTime)dr["Time"];
            SystemName = (string)dr["SystemName"];
            BodyName = (string)dr["BodyName"];
            Note = (string)dr["Note"];

            if (System.DBNull.Value != dr["Tags"])
                Tags = (string)dr["Tags"];
            if (System.DBNull.Value != dr["Parameters"])
                Parameters = (string)dr["Parameters"];
        }

        public void Set( int cmdr, string system , string body , DateTime timeutc, string note, string tags = null, string paras = null)
        {
            Commander = cmdr;
            SystemName = system;
            BodyName = body;
            TimeUTC = timeutc;
            Note = note;
            Tags = tags;
            Parameters = paras;
        }

        internal bool Add()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(cn); });
        }

        private bool Add(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into CaptainsLog (Commander, Time, SystemName, BodyName, Note, Tags, Parameters) values (@c,@t,@s,@b,@n,@g,@p)"))
            {
                cmd.AddParameterWithValue("@c", Commander);
                cmd.AddParameterWithValue("@t", TimeUTC);
                cmd.AddParameterWithValue("@s", SystemName);
                cmd.AddParameterWithValue("@b", BodyName);
                cmd.AddParameterWithValue("@n", Note);
                cmd.AddParameterWithValue("@g", Tags);
                cmd.AddParameterWithValue("@p", Parameters);
                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from CaptainsLog"))
                {
                    ID = (long)cmd2.ExecuteScalar();
                }

                return true;
            }
        }

        internal bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        internal bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Update CaptainsLog set Commander=@c, Time=@t, SystemName=@s, BodyName=@b, Note=@n, Tags=@g, Parameters=@p where ID=@id"))
            {
                cmd.AddParameterWithValue("@id", ID);
                cmd.AddParameterWithValue("@c", Commander);
                cmd.AddParameterWithValue("@t", TimeUTC);
                cmd.AddParameterWithValue("@s", SystemName);
                cmd.AddParameterWithValue("@b", BodyName);
                cmd.AddParameterWithValue("@n", Note);
                cmd.AddParameterWithValue("@g", Tags);
                cmd.AddParameterWithValue("@p", Parameters);
                cmd.ExecuteNonQuery();

                return true;
            }
        }

        public bool Delete()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Delete(cn, ID); });
        }

        static private bool Delete(SQLiteConnectionUser cn, long id)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM CaptainsLog WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", id);
                cmd.ExecuteNonQuery();
                return true;
            }
        }

        // Update notes only
        public void UpdateNotes(string notes)
        {
            Note = notes;
            Update();
        }

        public static List<CaptainsLogClass> ReadLogs()
        {
            return UserDatabase.Instance.DBRead<List<CaptainsLogClass>>(cn =>
            {
                return ReadLogs(cn);
            });
        }

        public static List<CaptainsLogClass> ReadLogs(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("select * from CaptainsLog"))
            {
                List<CaptainsLogClass> logs = new List<CaptainsLogClass>();

                using (DbDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        logs.Add(new CaptainsLogClass(rdr));
                    }
                }

                return logs;
            }
        }
    }

    // EVERYTHING goes thru list class for adding/deleting log entries

    public class GlobalCaptainsLogList
    {
        public static bool Instanced { get { return gbl != null; } }
        public static GlobalCaptainsLogList Instance { get { return gbl; } }

        public List<CaptainsLogClass> LogEntries { get { return globallog; } }

        public List<CaptainsLogClass> LogEntriesCmdr(int cmdrid) { return globallog.Where(x => x.Commander == cmdrid).ToList(); }
        public List<CaptainsLogClass> LogEntriesCmdrTimeOrder(int cmdrid) { return globallog.Where(x => x.Commander == cmdrid).OrderBy(x=>x.TimeUTC).ToList(); }

        public Action<CaptainsLogClass, bool> OnLogEntryChanged;        // bool = true if deleted

        private static GlobalCaptainsLogList gbl = null;

        private List<CaptainsLogClass> globallog = new List<CaptainsLogClass>();

        public static void LoadLog()
        {
            System.Diagnostics.Debug.Assert(gbl == null);       // no double instancing!
            gbl = new GlobalCaptainsLogList();
            gbl.globallog = CaptainsLogClass.ReadLogs();
        }

        public CaptainsLogClass[] FindUTC(DateTime startutc, DateTime endutc, int cmdr)
        {
            return (from x in LogEntries where x.TimeUTC >= startutc && x.TimeUTC <= endutc && x.Commander == cmdr select x).ToArray();
        }

        // bk = null, new note, else update. Note systemname/bodyname are not unique.  Id it the only unique property

        public CaptainsLogClass AddOrUpdate(CaptainsLogClass bk, int commander, string systemname, string bodyname, DateTime timeutc, string notes, string tags = null, string parameters = null)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);
            bool addit = bk == null;

            if (addit)
            {
                bk = new CaptainsLogClass();
                globallog.Add(bk);
                System.Diagnostics.Debug.WriteLine("New log created");
            }

            bk.Set(commander, systemname, bodyname, timeutc, notes, tags, parameters);

            if (addit)
                bk.Add();
            else
            {
                System.Diagnostics.Debug.WriteLine(GlobalCaptainsLogList.Instance.LogEntries.Find((xx) => Object.ReferenceEquals(bk, xx)) != null);
                bk.Update();
            }

            //System.Diagnostics.Debug.WriteLine("Write captains log " + bk.SystemName + ":" + bk.BodyName + " Notes " + notes);

            OnLogEntryChanged?.Invoke(bk, false);

            return bk;
        }

        public void Delete(CaptainsLogClass bk)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);
            long id = bk.ID;
            bk.Delete();
            globallog.RemoveAll(x => x.ID == id);
            OnLogEntryChanged?.Invoke(bk, true);
        }

        public void TriggerChange(CaptainsLogClass bk)
        {
            OnLogEntryChanged?.Invoke(bk, true);
        }

        // return all taglists, bktags = true means bookmarks, else planet tags
        public List<string> GetAllTags(int cmdrid)
        {
            List<string> taglist = new List<string>();
            foreach (var x in globallog)
            {
                if (x.Tags.HasChars() && x.Commander == cmdrid)
                    taglist.Add(x.Tags);
            }
            return taglist;
        }

    }
}
