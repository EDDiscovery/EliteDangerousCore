/*
 * Copyright 2015-2022 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore.DB
{
    public class SystemNoteClass
    {
        public bool IsSystemNote { get { return JournalID <= 0; } } 

        public long ID { get; private set; }
        public long JournalID { get; private set; }
        public string SystemName { get; private set; }
        public DateTime LocalTimeLastCreatedEdited { get; private set; }     // when created or last edited.
        public DateTime UTCTimeCreated { get; private set; }     // Introduced nov 22 - utc time created. May be MinDate
        public string JournalText { get; private set; }     // Introduced nov 22 - may be null
        public string Note { get; private set; }

        public SystemNoteClass()
        {
        }

        public SystemNoteClass(DbDataReader dr)
        {
            ID = (long)dr["id"];
            JournalID = (long)dr["journalid"];
            SystemName = (string)dr["Name"];
            LocalTimeLastCreatedEdited = ((DateTime)dr["Time"]).ToLocalKind();
            if (System.DBNull.Value != dr["UTCTime"])
                UTCTimeCreated = ((DateTime)dr["UTCTime"]);//.ToUniversalKind();
            else
                UTCTimeCreated = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);           // default don't have value

            if (System.DBNull.Value != dr["JournalText"])
                JournalText = (string)dr["JournalText"];
            Note = (string)dr["Note"];
        }

        private bool AddToDbAndGlobal()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return AddToDbAndGlobal(cn); });
        }

        private bool AddToDbAndGlobal(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into SystemNote (Name, Time, Note, journalid,UTCTime,JournalText) values (@name, @time, @note, @journalid,@utc,@jt)"))
            {
                cmd.AddParameterWithValue("@name", SystemName);
                cmd.AddParameterWithValue("@time", LocalTimeLastCreatedEdited);
                cmd.AddParameterWithValue("@note", Note);
                cmd.AddParameterWithValue("@journalid", JournalID);
                cmd.AddParameterWithValue("@utc", UTCTimeCreated);
                cmd.AddParameterWithValue("@jt", JournalText);

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from SystemNote"))
                {
                    ID = (long)cmd2.ExecuteScalar();
                }

                if (IsSystemNote)
                    notesbyname[SystemName.ToLowerInvariant()] = this;
                else
                    notesbyjid[JournalID] = this;

                return true;
            }
        }

        private bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        private bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Update SystemNote set Name=@Name, Time=@Time, Note=@Note, Journalid=@journalid, JournalText=@jt, UTCTime=@utc where ID=@id"))
            {
                cmd.AddParameterWithValue("@ID", ID);
                cmd.AddParameterWithValue("@Name", SystemName);
                cmd.AddParameterWithValue("@Note", Note);
                cmd.AddParameterWithValue("@Time", LocalTimeLastCreatedEdited);
                cmd.AddParameterWithValue("@journalid", JournalID);
                cmd.AddParameterWithValue("@utc", UTCTimeCreated);
                cmd.AddParameterWithValue("@jt", JournalText);

                cmd.ExecuteNonQuery();
            }

            return true;
        }
        public bool Delete()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Delete(cn); });
        }

        private bool Delete(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM SystemNote WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", ID);
                cmd.ExecuteNonQuery();

                if (IsSystemNote)
                    notesbyname.Remove(SystemName.ToLowerInvariant());
                else
                    notesbyjid.Remove(JournalID);

                return true;
            }
        }

        public static void GetAllSystemNotes()
        {
            var list = UserDatabase.Instance.DBRead<List<SystemNoteClass>>(cn =>
            {
                using (DbCommand cmd = cn.CreateCommand("select * from SystemNote"))
                {
                    List<SystemNoteClass> notes = new List<SystemNoteClass>();

                    using (DbDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            notes.Add(new SystemNoteClass(rdr));
                        }
                    }

                    return notes;
                }
            });

            foreach (var sys in list)
            {
                if (sys.IsSystemNote)
                    notesbyname[sys.SystemName.ToLowerInvariant()] = sys;
                else
                    notesbyjid[sys.JournalID] = sys;
            }
        }


        // return all text notes on this system, newline spaced. Returns empty string if none
        public static string GetTextNotesOnSystem(string systemname)      // case insensitive.. null if not there
        {
            var jidlist = notesbyjid.Values.Where(x => x.SystemName.Equals(systemname, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Note).ToList();

            notesbyname.TryGetValue(systemname.ToLowerInvariant(), out SystemNoteClass snc);

            if (jidlist.Count > 0)
            {
                if (snc != null)
                    jidlist.Add(snc.Note);

                return string.Join(Environment.NewLine, jidlist);
            }
            else
                return snc != null ? snc.Note : String.Empty;
        }

        // get a JID note
        public static SystemNoteClass GetJIDNote(long jid)
        {
            notesbyjid.TryGetValue(jid, out SystemNoteClass snc);
            return snc;
        }

        // get a system note on this system - system notes are now only made by EDSM
        public static SystemNoteClass GetSystemNote(string systemname)
        {
            notesbyname.TryGetValue(systemname.ToLowerInvariant(), out SystemNoteClass snc);
            return snc;
        }

        // set journalid = 0 for a system note
        public static SystemNoteClass MakeNote(string text, DateTime localtime, string sysname, long journalid, string journaltext )
        {
            SystemNoteClass sys = new SystemNoteClass();
            sys.Note = text;
            sys.LocalTimeLastCreatedEdited = localtime;
            sys.UTCTimeCreated = DateTime.UtcNow;
            sys.SystemName = sysname;
            sys.JournalID = journalid;                          // any new ones gets a journal id, making the Get always lock it to a journal entry
            sys.JournalText = journaltext;
            sys.AddToDbAndGlobal();  // adds it to the global cache AND the db
            System.Diagnostics.Debug.WriteLine($"System Note New note {sys.LocalTimeLastCreatedEdited} {sys.UTCTimeCreated} {sys.JournalID} {sys.SystemName} {sys.Note}");
            return sys;
        }

        // we update our note, time, and set dirty true.  
        // commit to DB and to globals
        public SystemNoteClass UpdateNote(string s, DateTime localtime, string journaltext)
        {
            Note = s;
            LocalTimeLastCreatedEdited = localtime;
            JournalText = journaltext;
            if ( UTCTimeCreated.Year < 2014)            // if an old note, create now
                UTCTimeCreated = DateTime.UtcNow;

            if (s.Length == 0)        // empty ones delete the note
            {
                System.Diagnostics.Debug.WriteLine($"System Note Delete {LocalTimeLastCreatedEdited} {JournalID} {SystemName} {Note}");
                Delete();           // delete and remove note
                return null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"System Note Update {LocalTimeLastCreatedEdited} {JournalID} {SystemName} {Note}");
                Update();           // update note
                return this;
            }
        }

        private static Dictionary<long, SystemNoteClass> notesbyjid = new Dictionary<long, SystemNoteClass>();
        private static Dictionary<string, SystemNoteClass> notesbyname = new Dictionary<string, SystemNoteClass>();


    }
}
