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
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore.DB
{
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
        public string Path { get; set; }
        public string GameVersion { get; set; } = "";    // either empty, or gameversion from fileheader, overritten by loadgame
        public string Build { get; set; } = ""; // either empty, or gameversion from fileheader, overritten by loadgame

        private string filename;       

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
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(cn); });
        }

        internal bool Add(SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            FetchAll();
            System.Diagnostics.Debug.WriteLine($"Add TLU {Path} {filename}");

            using (DbCommand cmd = cn.CreateCommand(
            "Insert into TravelLogUnit (Name, type, size, Path, CommanderID, GameVersion, Build) values (@name, @type, @size, @Path, @CommanderID, @GameVersion, @Build)", tn))
            {
                cmd.AddParameterWithValue("@name", filename);
                cmd.AddParameterWithValue("@type", Type);
                cmd.AddParameterWithValue("@size", Size);
                cmd.AddParameterWithValue("@Path", Path);
                cmd.AddParameterWithValue("@CommanderID", CommanderId);
                cmd.AddParameterWithValue("@GameVersion", GameVersion);
                cmd.AddParameterWithValue("@Build", Build);

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from TravelLogUnit"))
                {
                    ID = (long)cmd2.ExecuteScalar();
                }

                //System.Diagnostics.Debug.WriteLine("Update cache with " + ID);
                cacheid[ID] = this;
                return true;
            }
        }

        public bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        internal bool Update(SQLiteConnectionUser cn, DbTransaction tn = null)
        {
            using (DbCommand cmd = cn.CreateCommand(
            "Update TravelLogUnit set Name=@Name, Type=@type, size=@size, Path=@Path, CommanderID=@CommanderID, GameVersion=@GameVersion, Build=@Build where ID=@id", tn))
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

        static private void FetchAll()
        {
            if (cacheid == null)
            {
                cacheid = new Dictionary<long, TravelLogUnit>();
                cachepath = new Dictionary<string, TravelLogUnit>();

                UserDatabase.Instance.DBRead(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("select * from TravelLogUnit"))
                    {
                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                TravelLogUnit sys = new TravelLogUnit(rdr);
                                System.Diagnostics.Debug.Assert(!cacheid.ContainsKey(sys.ID));
                                cacheid[sys.ID] = sys;
                                cachepath[sys.FullName.ToLowerInvariant()] = sys;       // name is v.important for speed
                            }
                        }
                    }
                });
            }
        }

        static public List<TravelLogUnit> GetAll()
        {
            FetchAll();
            return cacheid.Values.ToList();
        }

        public static List<string> GetAllNames()
        {
            FetchAll();
            return cacheid.Values.Select(x=>x.FullName).ToList();
        }

        public static TravelLogUnit Get(string pathfilename)
        {
            FetchAll();
            if (cachepath.TryGetValue(pathfilename.ToLowerInvariant(), out TravelLogUnit res))
                return res;
            else
                return null;
        }

        public static bool TryGet(string name, out TravelLogUnit tlu)
        {
            tlu = Get(name);
            return tlu != null;
        }

        public static TravelLogUnit Get(long id)
        {
            FetchAll();
            return cacheid.ContainsKey(id) ? cacheid[id] : null;
        }

        public static bool TryGet(long id, out TravelLogUnit tlu)
        {
            tlu = Get(id);
            return tlu != null;
        }

        public static Dictionary<long, TravelLogUnit> cacheid = null;
        public static Dictionary<string, TravelLogUnit> cachepath = null;
    }
}

