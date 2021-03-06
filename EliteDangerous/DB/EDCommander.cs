﻿/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{Name} id:{Id} dir:{JournalDir}")]
    public class EDCommander
    {
        #region Instance

        // note setting these does not updated DB - you need to force that thru an update

        public int Id { set; get; }
        public string Name { set; get; } = "";
        public bool Deleted { set; get; }

        public string JournalDir { set; get; }

        public bool SyncToEdsm { set; get; }
        public bool SyncFromEdsm { set; get; }
        public string EdsmName { set; get; } = "";
        public string EDSMAPIKey { set; get; } = "";

        public bool SyncToInara { set; get; }
        public string InaraName { set; get; } = "";
        public string InaraAPIKey { set; get; } = "";

        public bool SyncToEddn { set; get; }

        public bool SyncToIGAU { set; get; }

        public JObject Options { set; get; } = new JObject();

        public bool ConsoleCommander { get { return Options.Contains("CONSOLE"); } set { if (value) Options["CONSOLE"] = true; else Options.Remove("CONSOLE"); } }

        public bool SyncToEDAstro { get { return Options["EDASTRO"].Bool(false); } set { Options["EDASTRO"] = value; } }

        // may be null
        public JObject ConsoleUploadHistory { get { return Options["ConsoleUpload"].Object(); } set { Options["ConsoleUpload"] = value; } }

        private string homesystem = "";
        private ISystem lookuphomesys = null;
        private string lastlookuphomename = null;
        public string HomeSystem { get { return homesystem; } set { homesystem = value; lookuphomesys = null; lastlookuphomename = null; } }
        public string HomeSystemTextOrSol { get { return homesystem.HasChars() ? homesystem : "Sol"; } }

        public ISystem HomeSystemI
        {
            get
            {
                if (homesystem.HasChars())
                {
                    if (lastlookuphomename != homesystem)
                    {
                        lastlookuphomename = homesystem;
                        lookuphomesys = SystemCache.FindSystem(homesystem, true);      // look up thru edsm. note we cache ISystem so once done, it won't be checked again
                    }
                }

                return lookuphomesys;
            }
        }

        public ISystem HomeSystemIOrSol { get { return HomeSystemI ?? new SystemClass("Sol", 0, 0, 0); } }

        public float MapZoom { set; get; } = 1.0f;
        public int MapColour { set; get; } = System.Drawing.Color.Red.ToArgb();
        public bool MapCentreOnSelection { set; get; } = true;

        public string Info
        {
            get
            {
                return BaseUtils.FieldBuilder.Build(";Console", ConsoleCommander, ";To EDDN", SyncToEddn, ";To EDSM", SyncToEdsm, ";From EDSM", SyncFromEdsm, ";To Inara", SyncToInara, ";To IGAU", SyncToIGAU );
            }
        }

        public string FID { get; set; }      // Frontier ID, not persistent.

        public void Update()
        {
            Update(this);
        }

        public void Delete()
        {
            Delete(this);
        }

        #endregion

        #region DB

        public static EDCommander Add(EDCommander other)
        {
            return Add(other.Name, other.EdsmName, other.EDSMAPIKey, other.JournalDir,
                                other.SyncToEdsm, other.SyncFromEdsm,
                                other.SyncToEddn,
                                other.SyncToInara, other.InaraName, other.InaraAPIKey,
                                other.HomeSystem, other.MapZoom, other.MapCentreOnSelection, other.MapColour,
                                other.SyncToIGAU, other.Options.ToString());
        }

        public static EDCommander Add(string name, string edsmName = null, string edsmApiKey = null, string journalpath = null,
                                        bool toedsm = false, bool fromedsm = false,
                                        bool toeddn = true,
                                        bool toinara = false, string inaraname = null, string inaraapikey = null,
                                        string homesystem = null, float mapzoom = 1.0f, bool mapcentreonselection = true, int mapcolour = -1,
                                        bool toigau = false, string options = "{}")
        {
            EDCommander cmdr = UserDatabase.Instance.ExecuteWithDatabase<EDCommander>(cn =>
            {
                using (DbCommand cmd = cn.Connection.CreateCommand("INSERT INTO Commanders (Name,EdsmName,EdsmApiKey,JournalDir,Deleted, SyncToEdsm, SyncFromEdsm, SyncToEddn, NetLogDir, SyncToEGO, EGOName, EGOAPIKey, SyncToInara, InaraName, InaraAPIKey, HomeSystem, MapColour, MapCentreOnSelection, MapZoom, SyncToIGAU,Options) " +
                                                          "VALUES (@Name,@EdsmName,@EdsmApiKey,@JournalDir,@Deleted, @SyncToEdsm, @SyncFromEdsm, @SyncToEddn, @NetLogDir, @SyncToEGO, @EGOName, @EGOApiKey, @SyncToInara, @InaraName, @InaraAPIKey, @HomeSystem, @MapColour, @MapCentreOnSelection, @MapZoom, @SyncToIGAU,@Options)"))
                {

                    cmd.AddParameterWithValue("@Name", name ?? "");
                    cmd.AddParameterWithValue("@EdsmName", edsmName ?? name ?? "");
                    cmd.AddParameterWithValue("@EdsmApiKey", edsmApiKey ?? "");
                    cmd.AddParameterWithValue("@JournalDir", journalpath ?? "");
                    cmd.AddParameterWithValue("@Deleted", false);
                    cmd.AddParameterWithValue("@SyncToEdsm", toedsm);
                    cmd.AddParameterWithValue("@SyncFromEdsm", fromedsm);
                    cmd.AddParameterWithValue("@SyncToEddn", toeddn);
                    cmd.AddParameterWithValue("@NetLogDir", "");        // Unused field, null out
                    cmd.AddParameterWithValue("@SyncToEGO", false); // Unused field, null out
                    cmd.AddParameterWithValue("@EGOName", ""); // Unused field, null out
                    cmd.AddParameterWithValue("@EGOApiKey", "");// Unused field, null out
                    cmd.AddParameterWithValue("@SyncToInara", toinara);
                    cmd.AddParameterWithValue("@InaraName", inaraname ?? "");
                    cmd.AddParameterWithValue("@InaraApiKey", inaraapikey ?? "");
                    cmd.AddParameterWithValue("@HomeSystem", homesystem ?? "");
                    cmd.AddParameterWithValue("@MapColour", mapcolour == -1 ? System.Drawing.Color.Red.ToArgb() : mapcolour);
                    cmd.AddParameterWithValue("@MapCentreOnSelection", mapcentreonselection);
                    cmd.AddParameterWithValue("@MapZoom", mapzoom);
                    cmd.AddParameterWithValue("@SyncToIGAU", toigau);
                    cmd.AddParameterWithValue("@Options", options);
                    cmd.ExecuteNonQuery();
                }

                using (DbCommand cmd = cn.Connection.CreateCommand("SELECT * FROM Commanders WHERE rowid = last_insert_rowid()"))
                {
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        {
                            cmdr = new EDCommander(reader);
                        }
                    }
                }

                return cmdr;
            });

            Commanders[cmdr.Id] = cmdr;
            return cmdr;
        }

        public static void Update(EDCommander cmdr)
        {
            UserDatabase.Instance.ExecuteWithDatabase(cn =>
            {
                using (DbCommand cmd = cn.Connection.CreateCommand(
                    "UPDATE Commanders SET Name=@Name, EdsmName=@EdsmName, EdsmApiKey=@EdsmApiKey, NetLogDir=@NetLogDir, JournalDir=@JournalDir, " +
                    "SyncToEdsm=@SyncToEdsm, SyncFromEdsm=@SyncFromEdsm, SyncToEddn=@SyncToEddn, SyncToEGO=@SyncToEGO, EGOName=@EGOName, " +
                    "EGOAPIKey=@EGOApiKey, SyncToInara=@SyncToInara, InaraName=@InaraName, InaraAPIKey=@InaraAPIKey, HomeSystem=@HomeSystem, " +
                    "MapColour=@MapColour, MapCentreOnSelection=@MapCentreOnSelection, MapZoom=@MapZoom, SyncToIGAU=@SyncToIGAU, Options=@Options " +
                    "WHERE Id=@Id"))
                {
                    cmd.AddParameterWithValue("@Id", cmdr.Id);
                    cmd.AddParameterWithValue("@Name", cmdr.Name);
                    cmd.AddParameterWithValue("@EdsmName", cmdr.EdsmName);
                    cmd.AddParameterWithValue("@EdsmApiKey", cmdr.EDSMAPIKey != null ? cmdr.EDSMAPIKey : "");
                    cmd.AddParameterWithValue("@NetLogDir", ""); // unused field
                    cmd.AddParameterWithValue("@JournalDir", cmdr.JournalDir != null ? cmdr.JournalDir : "");
                    cmd.AddParameterWithValue("@SyncToEdsm", cmdr.SyncToEdsm);
                    cmd.AddParameterWithValue("@SyncFromEdsm", cmdr.SyncFromEdsm);
                    cmd.AddParameterWithValue("@SyncToEddn", cmdr.SyncToEddn);
                    cmd.AddParameterWithValue("@SyncToEGO", false);
                    cmd.AddParameterWithValue("@EGOName", "");
                    cmd.AddParameterWithValue("@EGOApiKey", "");
                    cmd.AddParameterWithValue("@SyncToInara", cmdr.SyncToInara);
                    cmd.AddParameterWithValue("@InaraName", cmdr.InaraName != null ? cmdr.InaraName : "");
                    cmd.AddParameterWithValue("@InaraAPIKey", cmdr.InaraAPIKey != null ? cmdr.InaraAPIKey : "");
                    cmd.AddParameterWithValue("@HomeSystem", cmdr.homesystem != null ? cmdr.homesystem : "");
                    cmd.AddParameterWithValue("@MapColour", cmdr.MapColour);
                    cmd.AddParameterWithValue("@MapCentreOnSelection", cmdr.MapCentreOnSelection);
                    cmd.AddParameterWithValue("@MapZoom", cmdr.MapZoom);
                    cmd.AddParameterWithValue("@SyncToIGAU", cmdr.SyncToIGAU);
                    cmd.AddParameterWithValue("@Options", cmdr.Options.ToString());
                    cmd.ExecuteNonQuery();

                    Commanders[cmdr.Id] = cmdr;
                }
            });
        }

        public static void Delete(EDCommander cmdr)
        {
            Commanders.Remove(cmdr.Id);

            UserDatabase.Instance.ExecuteWithDatabase(cn =>
            {
                using (DbCommand cmd = cn.Connection.CreateCommand("UPDATE Commanders SET Deleted = 1 WHERE Id = @Id"))
                {
                    cmd.AddParameterWithValue("@Id", cmdr.Id);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        
        #endregion

        #region Properties

        public static Dictionary<int, EDCommander> Commanders { get { if (intcmdrdict == null) LoadCommanders();  return intcmdrdict; } }

        public static int CurrentCmdrID
        {
            get
            {
                if (currentcommander == Int32.MinValue)
                {
                    currentcommander = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("ActiveCommander", 0);
                }

                if (currentcommander >= 0 && !Commanders.ContainsKey(currentcommander) && Commanders.Count>0) // if not in list, pick first
                {
                    currentcommander = Commanders.Values.First().Id;
                }

                return currentcommander;
            }
            set
            {
                if (value != currentcommander && Commanders.ContainsKey(value))
                {
                    currentcommander = value;
                    EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("ActiveCommander", value);
                }
            }
        }

        public static EDCommander Current           // always returns
        {
            get
            {
                return Commanders[CurrentCmdrID];
            }
        }

        public static int NumberOfCommanders
        {
            get
            {
                return Commanders.Count;
            }
        }

        public static EDCommander GetCommander(int id)      // null if not valid - cope with it. Hidden gets returned.
        {
            if (Commanders.ContainsKey(id))
            {
                return Commanders[id];
            }
            else
            {
                return null;
            }
        }

        public static EDCommander GetCommander(string name)
        {
            return Commanders.Values.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsCommanderPresent(string name)
        {
            return Commanders.Values.ToList().FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != -1;
        }

        public static List<EDCommander> GetListInclHidden()
        {
            return Commanders.Values.OrderBy(v => v.Id).ToList();
        }

        public static List<EDCommander> GetListCommanders()
        {
            return Commanders.Values.Where(v => v.Id >= 0).OrderBy(v => v.Id).ToList();
        }

        #endregion

        #region Construction

        public EDCommander()
        {
            SyncToEddn = true;          // set it default to try and make them send it.
        }

        public EDCommander(DbDataReader reader)
        {
            Id = Convert.ToInt32(reader["Id"]);
            Name = Convert.ToString(reader["Name"]);
            Deleted = Convert.ToBoolean(reader["Deleted"]);

            JournalDir = Convert.ToString(reader["JournalDir"]) ?? "";

            SyncToEdsm = Convert.ToBoolean(reader["SyncToEdsm"]);
            SyncFromEdsm = Convert.ToBoolean(reader["SyncFromEdsm"]);
            EdsmName = reader["EDSMName"] == DBNull.Value ? Name : Convert.ToString(reader["EDSMName"]) ?? Name;
            EDSMAPIKey = Convert.ToString(reader["EdsmApiKey"]);

            SyncToInara = Convert.ToBoolean(reader["SyncToInara"]);
            InaraName = Convert.ToString(reader["InaraName"]);
            InaraAPIKey = Convert.ToString(reader["InaraAPIKey"]);

            SyncToEddn = Convert.ToBoolean(reader["SyncToEddn"]);

            HomeSystem = Convert.ToString(reader["HomeSystem"]) ?? "";        // may be null

            MapZoom = reader["MapZoom"] is System.DBNull ? 1.0f : (float)Convert.ToDouble(reader["MapZoom"]);
            MapColour = reader["MapColour"] is System.DBNull ? System.Drawing.Color.Red.ToArgb() : Convert.ToInt32(reader["MapColour"]);
            MapCentreOnSelection = reader["MapCentreOnSelection"] is System.DBNull ? true : Convert.ToBoolean(reader["MapCentreOnSelection"]);

            SyncToIGAU = Convert.ToBoolean(reader["SyncToIGAU"]);

            Options = JObject.Parse(Convert.ToString(reader["Options"]));
        }

        public EDCommander(int id, string Name)
        {
            this.Id = id;
            this.Name = Name;
        }

        #endregion

        #region Methods

        public static void LoadCommanders()
        {
            lock (locker)
            {
                intcmdrdict = new Dictionary<int, EDCommander>();

                UserDatabase.Instance.ExecuteWithDatabase(cn =>
                {
                    using (DbCommand cmd = cn.Connection.CreateCommand("SELECT * FROM Commanders Where Deleted=0"))
                    {
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EDCommander edcmdr = new EDCommander(reader);
                                intcmdrdict.Add(edcmdr.Id,edcmdr);
                            }
                        }
                    }
                });

                if (intcmdrdict.Count == 0)
                {
                    Add("Jameson (Default)");
                }

                EDCommander hidden = new EDCommander(-1, "Hidden Log");     // -1 is the hidden commander, add to list to make it
                intcmdrdict[-1] = hidden;        // so we give back a valid entry when its selected
            }
        }

        #endregion

        #region Private properties and methods

        private static Object locker = new object();
        private static Dictionary<int, EDCommander> intcmdrdict = null;
        private static int currentcommander = Int32.MinValue;

        #endregion

    }
}
