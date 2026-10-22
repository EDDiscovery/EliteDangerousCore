/*
 * Copyright 2016-2026 EDDiscovery development team
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
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore.DB
{
    [System.Diagnostics.DebuggerDisplay("{ID} {Type} {FullName} {Size}")]
    public class TravelLogUnit
    {
        public const int NetLogType = 1;
        public const int EDSMType = 2;
        public const int JournalType = 3;
        public const int TypeMask = 0xff;
        public const int BetaMarker = 0x8000;
        public const int OdysseyMarker = 0x4000;
        public const int HorizonsMarker = 0x2000;

        public long ID { get; set; }
        public int Type { get; set; }           // bit 15-8 are flags, bits 0-7 are type, see above
        public int Size { get; set; }
        public int? CommanderId { get; set; }
        public string FullName { get { return System.IO.Path.Combine(Path, filename); } }
        public string FileName { get { return filename; } }
        public string Path { get; set; }
        public string GameVersion { get; set; } = "";    // either empty, or gameversion from fileheader, overritten by loadgame
        public string Build { get; set; } = ""; // either empty, or gameversion from fileheader, overritten by loadgame

        // decode file name encoded in date to date
        // JournalAlpha.210510202154.01.log 
        // Journal.210128154806.01.log
        // Journal.2023-04-22T144621.01.log
        public string DateS
        {
            get
            {
                int jpos = FileName.LastIndexOf("Journal");
                if (jpos >= 0) jpos = FileName.IndexOf(".", jpos + 7);
                if (jpos >= 0)
                {
                    int epos = FileName.IndexOf(".", jpos + 1);
                    if (epos >= 0)
                        return FileName.Substring(jpos + 1, epos - jpos - 1);
                }
                return "";
            }
        }
        public DateTime Date { get
            {
                string s = DateS;
                if (s.Contains("T") && s.Length == 17)
                    s = s.Substring(0, 13) + ":" + s.Substring(13, 2) + ":" + s.Substring(15, 2);
                else if (!s.Contains("T") && s.Length == 12)
                    s = "20" + s.Substring(0, 2) + "-" + s.Substring(2, 2) + "-" + s.Substring(4, 2) + "T" + s.Substring(6, 2) + ":" + s.Substring(8, 2) + ":" + s.Substring(10, 2);
                if (!DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime tt))
                    System.Diagnostics.Debug.WriteLine($"TLU Date does not parse {s}");
                return tt;
            }
        }


        public TravelLogUnit()
        {
        }

        public TravelLogUnit(string s)
        {
            filename = System.IO.Path.GetFileName(s);
            Path = System.IO.Path.GetDirectoryName(s);
        }

        public TravelLogUnit(DbDataReader dr)
        {
            Object obj;
            ID = (long)dr["id"];
            filename = (string)dr["Name"];
            Type = (int)(long)dr["type"];
            Size = (int)(long)dr["size"];
            Path = (string)dr["Path"];
            GameVersion = (string)dr["GameVersion"];
            Build = (string)dr["Build"];

            obj = dr["CommanderId"];

            if (obj == DBNull.Value)
                CommanderId = null;  
            else
                CommanderId = (int)(long)dr["CommanderId"];
        }

        public bool IsBeta { get { return ((Path != null && Path.Contains("PUBLIC_TEST_SERVER")) || (Type & BetaMarker) == BetaMarker); } }
        public bool IsHorizons { get { return (Type & HorizonsMarker) != 0; } }
        public bool IsOdyssey { get { return (Type & OdysseyMarker) != 0; } }
        public bool IsBetaFlag { get { return (Type & BetaMarker) == BetaMarker; } }

        public bool Add()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(cn,null); });
        }

        // Monitor watcher adds TLUs into the DB in LastWrite order - see EDJournalMonitorWatcher line 240 

        internal bool Add(SQLiteConnectionUser cn, DbTransaction txn)
        {
            System.Diagnostics.Debug.WriteLine($"Add TLU {Path} {filename}");

            using (DbCommand cmd = cn.CreateCommand(
                                    "Insert into TravelLogUnit (Name, type, size, Path, CommanderID, GameVersion, Build) values (@name, @type, @size, @Path, @CommanderID, @GameVersion, @Build)"))
            {
                cmd.AddParameterWithValue("@name", filename);
                cmd.AddParameterWithValue("@type", Type);
                cmd.AddParameterWithValue("@size", Size);
                cmd.AddParameterWithValue("@Path", Path);
                cmd.AddParameterWithValue("@CommanderID", CommanderId);
                cmd.AddParameterWithValue("@GameVersion", GameVersion);
                cmd.AddParameterWithValue("@Build", Build);

                cmd.ExecuteNonQuery(txn);

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from TravelLogUnit"))
                {
                    ID = (long)cmd2.ExecuteScalar();
                }

                //System.Diagnostics.Debug.WriteLine("Update cache with " + ID);
                cacheid[ID] = this;
                cachepath[FullName] = this;       // name is v.important for speed.   Store under original Path case
                return true;
            }
        }

        public bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        internal bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand(
            "Update TravelLogUnit set Name=@Name, Type=@type, size=@size, Path=@Path, CommanderID=@CommanderID, GameVersion=@GameVersion, Build=@Build where ID=@id"))
            {
                cmd.AddParameterWithValue("@ID", ID);
                cmd.AddParameterWithValue("@Name", filename);
                cmd.AddParameterWithValue("@Type", Type);
                cmd.AddParameterWithValue("@size", Size);
                cmd.AddParameterWithValue("@Path", Path);
                cmd.AddParameterWithValue("@CommanderID", CommanderId);
                cmd.AddParameterWithValue("@GameVersion", GameVersion);
                cmd.AddParameterWithValue("@Build", Build);

                //System.Diagnostics.Debug.WriteLine("TLU Update " + Name + " " + Size);
                cmd.ExecuteNonQuery();

                return true;
            }
        }

        // Monitor watcher adds TLUs into the DB in LastWrite order so the TLU caches will be in LastWrite order (dictionary keeps insert order)

        public static void Initialise()
        {
            if (cacheid == null)        // prevent double init
            {
                cacheid = new Dictionary<long, TravelLogUnit>();
                cachepath = new Dictionary<string, TravelLogUnit>();

                UserDatabase.Instance.DBRead(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("select * from TravelLogUnit Order By id"))
                    {
                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                TravelLogUnit sys = new TravelLogUnit(rdr);
                                System.Diagnostics.Debug.Assert(!cacheid.ContainsKey(sys.ID));
                                cacheid[sys.ID] = sys;
                                cachepath[sys.FullName] = sys;       // name is v.important for speed.  Keep case of filename for linux
                            }
                        }
                    }
                });
            }
        }

        public static List<string> GetAllNames()
        {
            return cacheid.Values.Select(x=>x.FullName).ToList();
        }

        // can be used before Initialise is called
        // case sensitive.
        public static bool TryGet(string pathfilename, out TravelLogUnit tlu)
        {
            tlu = null;
            return cachepath != null ? cachepath.TryGetValue(pathfilename, out tlu) : false;
        }

        // can be used before Initialise is called
        public static TravelLogUnit Get(long id)
        {
            return cacheid != null && cacheid.ContainsKey(id) ? cacheid[id] : null;
        }

        // can be used before Initialise is called
        public static bool TryGet(long id, out TravelLogUnit tlu)
        {
            tlu = null;
            return cacheid != null && cacheid.TryGetValue(id, out tlu);
        }

        // will be in last write order due to insert order
        public static List<TravelLogUnit> GetCommander(int cmdrid)
        {
            return cacheid.Where(x => x.Value.CommanderId == cmdrid).Select(x=>x.Value).ToList();
        }

        private string filename;

        public static Dictionary<long, TravelLogUnit> cacheid = null;
        // key is in original filename case - do not ToLower it due to linux 
        public static Dictionary<string, TravelLogUnit> cachepath = null;      
    }
}

