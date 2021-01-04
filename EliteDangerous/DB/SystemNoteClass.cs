/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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

namespace EliteDangerousCore.DB
{
    public class SystemNoteClass
    {
        public long id;
        public long Journalid;              // if Journalid <> 0, its a journal marker.  SystemName can be set or clear
        public string SystemName;           // with JournalId=0, this is a system marker
        public DateTime Time;
        public string Note { get; private set; }

        public bool Dirty;                  // NOT DB changed but uncommitted
        public bool FSDEntry;               // is a FSD entry.. used to mark it for EDSM send purposes
        
        public static List<SystemNoteClass> globalSystemNotes = new List<SystemNoteClass>();        // global cache, kept updated

        public SystemNoteClass()
        {
        }

        public SystemNoteClass(DbDataReader dr)
        {
            id = (long)dr["id"];
            Journalid = (long)dr["journalid"];
            SystemName = (string)dr["Name"];
            Time = (DateTime)dr["Time"];
            Note = (string)dr["Note"];
        }


        private bool AddToDbAndGlobal()
        {
            return UserDatabase.Instance.ExecuteWithDatabase<bool>(cn => { return AddToDbAndGlobal(cn.Connection); });
        }

        private bool AddToDbAndGlobal(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into SystemNote (Name, Time, Note, journalid) values (@name, @time, @note, @journalid)"))
            {
                cmd.AddParameterWithValue("@name", SystemName);
                cmd.AddParameterWithValue("@time", Time);
                cmd.AddParameterWithValue("@note", Note);
                cmd.AddParameterWithValue("@journalid", Journalid);

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from SystemNote"))
                {
                    id = (long)cmd2.ExecuteScalar();
                }

                globalSystemNotes.Add(this);

                Dirty = false;

                return true;
            }
        }

        private bool Update()
        {
            return UserDatabase.Instance.ExecuteWithDatabase<bool>(cn => { return Update(cn.Connection); });
        }

        private bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Update SystemNote set Name=@Name, Time=@Time, Note=@Note, Journalid=@journalid where ID=@id"))
            {
                cmd.AddParameterWithValue("@ID", id);
                cmd.AddParameterWithValue("@Name", SystemName);
                cmd.AddParameterWithValue("@Note", Note);
                cmd.AddParameterWithValue("@Time", Time);
                cmd.AddParameterWithValue("@journalid", Journalid);

                cmd.ExecuteNonQuery();

                Dirty = false;
            }

            return true;
        }

        public bool Delete()
        {
            return UserDatabase.Instance.ExecuteWithDatabase<bool>(cn => { return Delete(cn.Connection); });
        }

        private bool Delete(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM SystemNote WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", id);
                cmd.ExecuteNonQuery();

                globalSystemNotes.RemoveAll(x => x.id == id);     // remove from list any containing id.
                return true;
            }
        }

        public static bool GetAllSystemNotes()
        {
            try
            {
                return UserDatabase.Instance.ExecuteWithDatabase<bool>(cn =>
                {
                    using (DbCommand cmd = cn.Connection.CreateCommand("select * from SystemNote"))
                    {
                        List<SystemNoteClass> notes = new List<SystemNoteClass>();

                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                notes.Add(new SystemNoteClass(rdr));
                            }
                        }

                        if (notes.Count == 0)
                        {
                            return false;
                        }
                        else
                        {
                            foreach (var sys in notes)
                            {
                                globalSystemNotes.Add(sys);
                            }

                            return true;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.ToString());
                return false;
            }
        }

        public static void CommitDirtyNotes( Action<SystemNoteClass> actionondirty )   // can be null
        {
            foreach (SystemNoteClass snc in globalSystemNotes)
            {
                if (snc.Dirty)
                {
                    System.Diagnostics.Debug.WriteLine("Commit dirty note " + snc.Journalid + " " + snc.SystemName + " " + snc.Note);
                    snc.Update();       // clears the dirty flag
                    actionondirty?.Invoke(snc);     // pass back in case it needs to do something with it
                }
            }
        }

        public static SystemNoteClass GetNoteOnSystem(string name)      // case insensitive.. null if not there
        {
            return globalSystemNotes.FindLast(x => x.SystemName.Equals(name, StringComparison.InvariantCultureIgnoreCase) );
        }

        public static SystemNoteClass GetNoteOnJournalEntry(long jid)
        {
            if (jid > 0)
                return globalSystemNotes.FindLast(x => x.Journalid == jid);
            else
                return null;
        }

        public static SystemNoteClass GetSystemNote(long journalid, string systemname = null)
        {
            SystemNoteClass systemnote = SystemNoteClass.GetNoteOnJournalEntry(journalid);

            if (systemnote == null && systemname != null)      // this is for older system name notes
            {
                systemnote = SystemNoteClass.GetNoteOnSystem(systemname);  
            }

            return systemnote;
        }

//        public static SystemNoteClass MakeSystemNote(string text, DateTime time, string sysname, long journalid, long edsmid , bool fsdentry )
        public static SystemNoteClass MakeSystemNote(string text, DateTime time, string sysname, long journalid, bool fsdentry )
        {
            SystemNoteClass sys = new SystemNoteClass();
            sys.Note = text;
            sys.Time = time;
            sys.SystemName = sysname;
            sys.Journalid = journalid;                          // any new ones gets a journal id, making the Get always lock it to a journal entry
            sys.FSDEntry = fsdentry;
            sys.AddToDbAndGlobal();  // adds it to the global cache AND the db
            System.Diagnostics.Debug.WriteLine("made note " + sys.Journalid + " " + sys.SystemName + " " + sys.Note);
            return sys;
        }

        // we update our note, time, and set dirty true.  
        // if commit = true, we write the note to the db, which clears the dirty flag
        public SystemNoteClass UpdateNote(string s, bool commit, DateTime time, bool fsdentry)
        {
            Note = s;
            Time = time;
            FSDEntry = fsdentry;

            Dirty = true;

            if (commit)
            {
                if (s.Length == 0)        // empty ones delete the note
                {
                    System.Diagnostics.Debug.WriteLine("Delete note " + Journalid + " " + SystemName + " " + Note);
                    Delete();           // delete and remove note
                    return null;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Update note " + Journalid + " " + SystemName + " " + Note);
                    Update();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Note edit in memory " + Journalid + " " + SystemName + " " + Note);
            }

            return this;
        }


    }
}
