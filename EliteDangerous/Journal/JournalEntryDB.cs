/*
 * Copyright © 2016-2019 EDDiscovery development team
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

using BaseUtils.JSON;
using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    // DB accessor part of this class

    public abstract partial class JournalEntry
    {
        static protected JournalEntry CreateJournalEntry(DbDataReader dr)
        {
            string json = (string)dr["EventData"];

            JournalEntry jr = JournalEntry.CreateJournalEntry(json);

            jr.Id = (int)(long)dr["Id"];
            jr.TLUId = (int)(long)dr["TravelLogId"];
            jr.CommanderId = (int)(long)dr["CommanderId"];
            jr.Synced = (int)(long)dr["Synced"];
            return jr;
        }

        public bool Add(JObject jo)
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(jo, cn); });
        }

        internal bool Add(JObject jo, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            // note we don't use EDMSID any more, but we write a zero into it to keep 11.9.3 and before happy

            using (DbCommand cmd = cn.CreateCommand("Insert into JournalEntries (EventTime, TravelLogID, CommanderId, EventTypeId , EventType, EventData, Synced, EdsmId) values (@EventTime, @TravelLogID, @CommanderID, @EventTypeId , @EventStrName, @EventData, @Synced, 0)", tn))
            {
                cmd.AddParameterWithValue("@EventTime", EventTimeUTC);           // MUST use UTC connection
                cmd.AddParameterWithValue("@TravelLogID", TLUId);
                cmd.AddParameterWithValue("@CommanderID", CommanderId);
                cmd.AddParameterWithValue("@EventTypeId", EventTypeID);
                cmd.AddParameterWithValue("@EventStrName", EventTypeStr);
                cmd.AddParameterWithValue("@EventData", jo.ToString());
                cmd.AddParameterWithValue("@Synced", Synced);

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from JournalEntries"))
                {
                    Id = (long)cmd2.ExecuteScalar();
                }
                return true;
            }
        }

        public bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        private bool Update(SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set EventTime=@EventTime, TravelLogID=@TravelLogID, CommanderID=@CommanderID, EventTypeId=@EventTypeId, EventType=@EventStrName, Synced=@Synced where ID=@id", tn))
            {
                cmd.AddParameterWithValue("@ID", Id);
                cmd.AddParameterWithValue("@EventTime", EventTimeUTC);  // MUST use UTC connection
                cmd.AddParameterWithValue("@TravelLogID", TLUId);
                cmd.AddParameterWithValue("@CommanderID", CommanderId);
                cmd.AddParameterWithValue("@EventTypeId", EventTypeID);
                cmd.AddParameterWithValue("@EventStrName", EventTypeStr);
                cmd.AddParameterWithValue("@Synced", Synced);
                cmd.ExecuteNonQuery();

                return true;
            }
        }

        internal void UpdateJsonEntry(JObject jo, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            if ( JsonCached != null )
            {
                JsonCached = jo;
            }

            using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set EventData=@EventData where ID=@id", tn))
            {
                cmd.AddParameterWithValue("@ID", Id);
                cmd.AddParameterWithValue("@EventData", jo.ToString());
                cmd.ExecuteNonQuery();
            }
        }

        static public void Delete(long idvalue)
        {
            UserDatabase.Instance.DBWrite(cn => { Delete(idvalue,cn); });
        }

        static private void Delete(long idvalue, SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM JournalEntries WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", idvalue);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateMapColour(int v)
        {
            JournalFSDJump fsd = this as JournalFSDJump;        // only update if fsd
            if (fsd != null)
                fsd.SetMapColour(v);
        }

        internal void UpdateStarPosition(ISystem pos, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            JObject jo = GetJson(cn,tn);
                                                                                
            if (jo != null )
            {
                jo["StarPos"] = new JArray() { pos.X, pos.Y, pos.Z };
                jo["StarPosFromEDSM"] = true;

                using (DbCommand cmd2 = cn.CreateCommand("Update JournalEntries set EventData = @EventData where ID = @ID", tn))
                {
                    cmd2.AddParameterWithValue("@EventData", jo.ToString());
                    cmd2.AddParameterWithValue("@ID", Id);
                    cmd2.ExecuteNonQuery();
                }
            }
        }

        private void UpdateSyncFlagBit(SyncFlags bit1, bool value1, SyncFlags bit2, bool value2, SQLiteConnectionUser cn , DbTransaction txn = null)
        {
            if (value1)
                Synced |= (int)bit1;
            else
                Synced &= ~(int)bit1;

            if (value2)
                Synced |= (int)bit2;
            else
                Synced &= ~(int)bit2;

            using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set Synced = @sync where ID=@journalid", txn))
            {
                cmd.AddParameterWithValue("@journalid", Id);
                cmd.AddParameterWithValue("@sync", Synced);
                //System.Diagnostics.Trace.WriteLine(string.Format("Update sync flag ID {0} with {1}", Id, Synced));
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateCommanderID(int cmdrid)
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set CommanderID = @cmdrid where ID=@journalid"))
                {
                    cmd.AddParameterWithValue("@journalid", Id);
                    cmd.AddParameterWithValue("@cmdrid", cmdrid);
                    System.Diagnostics.Trace.WriteLine(string.Format("Update cmdr id ID {0} with map colour", Id));
                    cmd.ExecuteNonQuery();
                    CommanderId = cmdrid;
                }
            });
        }

        static public bool ResetCommanderID(int from, int to)
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                using (DbCommand cmd = cn.CreateCommand("Update JournalEntries set CommanderID = @cmdridto where CommanderID=@cmdridfrom"))
                {
                    if (from == -1)
                        cmd.CommandText = "Update JournalEntries set CommanderID = @cmdridto";

                    cmd.AddParameterWithValue("@cmdridto", to);
                    cmd.AddParameterWithValue("@cmdridfrom", from);
                    System.Diagnostics.Trace.WriteLine(string.Format("Update cmdr id ID {0} with {1}", from, to));
                    cmd.ExecuteNonQuery();
                }
            });
            return true;
        }

        private JObject JsonCached { get; set; }             // New entries scanned thru the readers get this, ones from history rely on looking up in DB 

        public string GetJsonString()       // null if no JSON - not likely.
        {
            return GetJson()?.ToString();
        }

        public JObject GetJsonCloned()      // you may modify this
        {
            return (JObject)GetJson().Clone();
        }

        public JObject GetJson()
        {
            if (JsonCached == null)
                return UserDatabase.Instance.DBRead<JObject>(cn => { return GetJson(cn); });
            else
                return JsonCached;
        }

        internal JObject GetJson(SQLiteConnectionUser cn, DbTransaction tn = null)            // do not modify
        {
            if (JsonCached == null)
            {
                JsonCached = GetJson(Id, cn, tn);
            }
            return JsonCached;
        }

        public void UpdateJson(JObject jo)
        {
            JsonCached = jo;
        }

        static internal JObject GetJson(long journalid, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("select EventData from JournalEntries where ID=@journalid", tn))
            {
                cmd.AddParameterWithValue("@journalid", journalid);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string json = (string)reader["EventData"];
                        return JObject.Parse(json, JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL);
                    }
                }
            }

            return null;
        }

        static public JournalEntry Get(long journalid)
        {
            return UserDatabase.Instance.DBRead<JournalEntry>(cn => { return Get(journalid, cn); });
        }

        static internal JournalEntry Get(long journalid, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand("select * from JournalEntries where ID=@journalid", tn))
            {
                cmd.AddParameterWithValue("@journalid", journalid);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return CreateJournalEntry(reader);
                    }
                }
            }

            return null;
        }

        static public List<JournalEntry> Get(string eventtype)            // any commander, find me an event of this type..
        {
            return UserDatabase.Instance.DBRead<List<JournalEntry>>(cn => { return Get(eventtype, cn); });
        }

        static internal List<JournalEntry> Get(string eventtype, SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            Dictionary<long, TravelLogUnit> tlus = TravelLogUnit.GetAll().ToDictionary(t => t.ID);

            using (DbCommand cmd = cn.CreateCommand("select * from JournalEntries where EventType=@ev", tn))
            {
                cmd.AddParameterWithValue("@ev", eventtype);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    List<JournalEntry> entries = new List<JournalEntry>();

                    while (reader.Read())
                    {
                        JournalEntry je = CreateJournalEntry(reader);
                        entries.Add(je);
                    }

                    return entries;
                }
            }
        }

        // Primary method to fill historylist
        // Get All journals matching parameters. 
        // if callback set, then each JE is passed back thru callback and not accumulated. Callback is in this thread. Callback can stop the accumulation if it returns false

        static public JournalEntry[] GetAll(int commander = -999, DateTime? startdateutc = null, DateTime? enddateutc = null,
                            JournalTypeEnum[] ids = null, DateTime? allidsafterutc = null, Func<JournalEntry, Object, bool> callback = null, Object callbackobj = null,
                            int chunksize = 1000, Action<string> reportProgress = null, Func<bool> cancelRequested = null)
        {
            // in the connection thread, construct the command and execute a read..

            Stopwatch sw = new Stopwatch(); sw.Start();

            var retlist = UserDatabase.Instance.DBRead<List<Tuple<long, long, int, string, int>>>(cn =>
            {
                DbCommand cmd = cn.CreateCommand("select Id,TravelLogId,CommanderId,EventData,Synced from JournalEntries");
                string cnd = "";
                if (commander != -999)
                {
                    cnd = cnd.AppendPrePad("CommanderID = @commander", " and ");
                    cmd.AddParameterWithValue("@commander", commander);
                }
                if (startdateutc != null)
                {
                    cnd = cnd.AppendPrePad("EventTime >= @after", " and ");
                    cmd.AddParameterWithValue("@after", startdateutc.Value);
                }
                if (enddateutc != null)
                {
                    cnd = cnd.AppendPrePad("EventTime <= @before", " and ");
                    cmd.AddParameterWithValue("@before", enddateutc.Value);
                }
                if (ids != null)
                {
                    int[] array = Array.ConvertAll(ids, x => (int)x);
                    if (allidsafterutc != null)
                    {
                        cmd.AddParameterWithValue("@idafter", allidsafterutc.Value);
                        cnd = cnd.AppendPrePad("(EventTypeId in (" + string.Join(",", array) + ") Or EventTime>=@idafter)", " and ");
                    }
                    else
                    {
                        cnd = cnd.AppendPrePad("EventTypeId in (" + string.Join(",", array) + ")", " and ");
                    }
                }

                if (cnd.HasChars())
                    cmd.CommandText += " where " + cnd;

                cmd.CommandText += " Order By EventTime,Id ASC";

                List<Tuple<long, long, int, string, int>> jdata = new List<Tuple<long, long, int, string, int>>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read() && !(cancelRequested?.Invoke()??false))
                    {
                        jdata.Add(new Tuple<long, long, int, string, int>((long)reader[0], (long)reader[1], (int)(long)reader[2], (string)reader[3], (int)(long)reader[4]));
                    }
                }

                return jdata;
            });

            System.Diagnostics.Debug.WriteLine($"P1 {sw.ElapsedMilliseconds} {retlist.Count}");

            JournalEntry[] jlist = new JournalEntry[retlist.Count];

            int eno = 0;
            foreach(var e in retlist )
            {
                if ( eno % 10000 == 0 )
                {
                    if (cancelRequested?.Invoke() ?? false)     // if cancelling, stop processing
                        break;
                    reportProgress?.Invoke($"Creating Journal Entries {eno}/{retlist.Count}");
                }

                jlist[eno++] = CreateJournalEntry(e.Item1, e.Item2, e.Item3, e.Item4, e.Item5);
            }

            retlist = null;

            System.Diagnostics.Debug.WriteLine($"P2 {sw.ElapsedMilliseconds} {jlist.Length} {cancelRequested?.Invoke()}");

            if (callback != null)               // collated, now process them, if callback, feed them thru callback procedure
            {
                foreach (var e in jlist)
                {
                    if (!callback.Invoke(e, callbackobj))     // if indicate stop
                    {
                        break;
                    }
                }
                jlist = null;
                return null;
            }
            else
            {
                return jlist;
            }
        }

        public static List<JournalEntry> GetByEventType(JournalTypeEnum eventtype, int commanderid, DateTime startutc, DateTime stoputc)
        {
            Dictionary<long, TravelLogUnit> tlus = TravelLogUnit.GetAll().ToDictionary(t => t.ID);
            List<JournalEntry> entries = new List<JournalEntry>();

            // in the connection thread, execute the 
            return UserDatabase.Instance.DBRead(cn =>
            {
                var cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE EventTypeID = @eventtype and  CommanderID=@commander and  EventTime >=@start and EventTime<=@Stop ORDER BY EventTime ASC");
                cmd.AddParameterWithValue("@eventtype", (int)eventtype);
                cmd.AddParameterWithValue("@commander", (int)commanderid);
                cmd.AddParameterWithValue("@start", startutc);
                cmd.AddParameterWithValue("@stop", stoputc);

                using (var reader = cmd.ExecuteReader())
                {
                    List<JournalEntry> vsc = new List<JournalEntry>();

                    while (vsc.Count < 1000 && reader.Read())
                    {
                        JournalEntry je = CreateJournalEntry(reader);
                        vsc.Add(je);
                    }

                    return vsc;
                }
            });
        }
               
        internal static List<JournalEntry> GetAllByTLU(long tluid, SQLiteConnectionUser cn)
        {
            TravelLogUnit tlu = TravelLogUnit.Get(tluid);
            List<JournalEntry> vsc = new List<JournalEntry>();

            using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE TravelLogId = @source ORDER BY EventTime ASC"))
            {
                cmd.AddParameterWithValue("@source", tluid);
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JournalEntry je = CreateJournalEntry(reader);
                        vsc.Add(je);
                    }
                }
            }
            return vsc;
        }

        public static JournalEntry GetLast(DateTime beforeutc, Func<JournalEntry, bool> filter)
        {
            return UserDatabase.Instance.DBRead<JournalEntry>(cn =>
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE EventTime < @time ORDER BY EventTime DESC"))
                {
                    cmd.AddParameterWithValue("@time", beforeutc);
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            JournalEntry ent = CreateJournalEntry(reader);
                            if (filter(ent))
                            {
                                return ent;
                            }
                        }
                    }
                }
                return null;
            });
        }

        public static T GetLast<T>(DateTime beforeutc, Func<T, bool> filter = null)
            where T : JournalEntry
        {
            return (T)GetLast(beforeutc, e => e is T && (filter == null || filter((T)e)));
        }

        public static List<JournalEntry> FindEntry(JournalEntry ent, SQLiteConnectionUser cn , JObject entjo = null)      // entjo is not changed.
        {
            if (entjo == null)
            {
                entjo = GetJson(ent.Id,cn);
            }

            entjo = RemoveEDDGeneratedKeys(entjo);

            List<JournalEntry> entries = new List<JournalEntry>();

            using (DbCommand cmd = cn.CreateCommand("SELECT * FROM JournalEntries WHERE CommanderId = @cmdrid AND EventTime = @time AND TravelLogId = @tluid AND EventTypeId = @evttype ORDER BY Id ASC"))
            {
                cmd.AddParameterWithValue("@cmdrid", ent.CommanderId);
                cmd.AddParameterWithValue("@time", ent.EventTimeUTC);
                cmd.AddParameterWithValue("@tluid", ent.TLUId);
                cmd.AddParameterWithValue("@evttype", ent.EventTypeID);
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JournalEntry jent = CreateJournalEntry(reader);
                        if (AreSameEntry(ent, jent, cn, entjo))
                        {
                            entries.Add(jent);
                        }
                    }
                }
            }

            return entries;
        }

        public static int RemoveDuplicateFSDEntries(int currentcmdrid)
        {
            // list of systems in journal, sorted by time
            List<JournalFSDJump> vsSystemsEnts = GetAll(currentcmdrid, ids: new[] { JournalTypeEnum.FSDJump }).OfType<JournalFSDJump>().OrderBy(j => j.EventTimeUTC).ToList();

            int count = 0;
            UserDatabase.Instance.DBWrite(cn =>
            {
                for (int ji = 1; ji < vsSystemsEnts.Count; ji++)
                {
                    JournalEvents.JournalFSDJump prev = vsSystemsEnts[ji - 1] as JournalEvents.JournalFSDJump;
                    JournalEvents.JournalFSDJump current = vsSystemsEnts[ji] as JournalEvents.JournalFSDJump;

                    if (prev != null && current != null)
                    {
                        bool previssame = (prev.StarSystem.Equals(current.StarSystem, StringComparison.CurrentCultureIgnoreCase) && (!prev.HasCoordinate || !current.HasCoordinate || (prev.StarPos - current.StarPos).LengthSquared < 0.01));

                        if (previssame)
                        {
                            Delete(prev.Id, cn);
                            count++;
                            System.Diagnostics.Debug.WriteLine("Dup {0} {1} {2} {3}", prev.Id, current.Id, prev.StarSystem, current.StarSystem);
                        }
                    }
                }
            });

            return count;
        }

    }
}

