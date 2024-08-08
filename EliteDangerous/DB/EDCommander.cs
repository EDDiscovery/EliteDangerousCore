/*
 * Copyright 2015-2024 EDDiscovery development team
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

using QuickJSON;
using EliteDangerousCore.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{Name} id:{Id} del:{Deleted} dir:{JournalDir}")]
    public class EDCommander
    {
        #region Instance

        // note setting these does not updated DB - you need to force that thru an update

        public int Id { set; get; }
        public string Name { set; get; } = "";

        public string RootName { get { return GetRootName(Name); } }

        // The root name is used by CAPI to associate legacy/live/beta commanders with one oAUTH login
        // Commanders 'name' and 'name (Legacy)' share a single oAUTH login, so in the CAPI folder the oAuth is saved in the 'name' file
        // note console commanders [C] name are purposely not removed by this, as we want them seperate, so they stay [C] name. That name gets placed in the downloaded journals, and is used in the capi login
        public static string GetRootName(string s)
        {
            return s.Replace(" (Legacy)", "").Replace("[BETA] ", "");
        }

        public static bool NameIsConsoleCommander(string name) { return name.StartsWith("[C] "); }
        public static string AddConsoleTagToName(string name) { return "[C] " + name; }
        public static string RemoveConsoleTagFromName(string name) { return name.ReplaceIfStartsWith("[C] ", ""); }
        public static string AddLegacyTagToName(string name) { return name + " (Legacy)"; }
        public bool NameIsBeta { get { return Name.StartsWith("[BETA] "); } }
        public static string AddBetaTagToName(string name) { return "[BETA] " + name; }

        public bool Deleted { set; get; }

        public string JournalDir { set; get; }

        // Nov 22 update 14 syncing to these are only for Live commanders
        public bool EDSMSupported { get { return !LegacyCommander && !ConsoleCommander; } }
        public bool SyncToEdsm { set { if (EDSMSupported) synctoedsm = value; } get { return EDSMSupported ? synctoedsm : false; } }
        public bool SyncFromEdsm { set { if (EDSMSupported) syncfromedsm = value; } get { return EDSMSupported ? syncfromedsm : false; } }

        public bool InaraSupported { get { return !LegacyCommander && !ConsoleCommander; } }
        public bool SyncToInara { set { if (InaraSupported) synctoinara = value; } get { return InaraSupported ? synctoinara : false; } }

        public string EdsmName { set; get; } = "";
        public string EDSMAPIKey { set; get; } = "";

        public string InaraName { set; get; } = "";
        public string InaraAPIKey { set; get; } = "";

        public bool SyncToEddn { set; get; }

        public JObject Options { set; get; } = new JObject();

        public bool ConsoleCommander { get { return Options.Contains("CONSOLE"); } set { if (value) Options["CONSOLE"] = true; else Options.Remove("CONSOLE"); } }
        public bool IncludeSubFolders { get { return Options["SUBFOLDERS"].Bool(true); } set { Options["SUBFOLDERS"] = value; } }       // default if key is not there is true, past behaviour
        public bool SyncToEDAstro { get { return Options["EDASTRO"].Bool(false); } set { Options["EDASTRO"] = value; } }
        public int LinkedCommanderID { get { return Options["LinkedCommander"].I("ID").Int(-1); } }         // if load previous commander exists, its id, else -1
        public DateTime LinkedCommanderEndTime { get { return Options["LinkedCommander"].I("EndTime").DateTimeUTC(); } }  // DateTime.Min if does not exist
        public void SetLinkedCommander(int id, DateTime endloadtimeutc)     // set linked commander with an end time load limit
        {
            Options["LinkedCommander"] = new JObject() { ["ID"] = id, ["EndTime"] = endloadtimeutc };
        }
        public bool LegacyCommander { get { return Options["Legacy"].Bool(); } set { Options["Legacy"] = value; } }         // indicate legacy commander

        public JObject ConsoleUploadHistory { get { return Options["ConsoleUpload"].Object(); } set { Options["ConsoleUpload"] = value; } }     // may be null

        public string HomeSystem { get { return homesystem; } set { homesystem = value; lookuphomesys = null; lastlookuphomename = null; } }

        public ISystem HomeSystemI
        {
            get
            {
                if (homesystem.HasChars())
                {
                    if (lastlookuphomename != homesystem)
                    {
                        lastlookuphomename = homesystem;
                        lookuphomesys = SystemCache.FindSystem(homesystem, EliteDangerousCore.WebExternalDataLookup.All);      // look up thru edsm. note we cache ISystem so once done, it won't be checked again
                    }
                }

                return lookuphomesys;
            }
        }

        public ISystem HomeSystemIOrSol { get { return HomeSystemI ?? new SystemClass("Sol", 10477373803, 0, 0, 0); } }

        public int MapColour { set; get; } = System.Drawing.Color.Red.ToArgb();

        public string Info
        {
            get
            {
                return BaseUtils.FieldBuilder.Build(";Console", ConsoleCommander, ";EDDN", SyncToEddn, ";EDSM", SyncToEdsm,
                                                    ";From EDSM", SyncFromEdsm, ";Inara", SyncToInara, ";EDAstro", SyncToEDAstro,
                                                    ";Linked", LinkedCommanderID >= 0
                                                    );
            }
        }

        public string FID { get; set; }      // Frontier ID, not persistent.

        public void Update()
        {
            Update(this);
        }

        #endregion

        #region Construction

        public EDCommander()
        {
            SyncToEddn = true;          // set it default to try and make them send it.
            IncludeSubFolders = false;  // and no subfolders as the default now
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

            MapColour = reader["MapColour"] is System.DBNull ? System.Drawing.Color.Red.ToArgb() : Convert.ToInt32(reader["MapColour"]);

            Options = JObject.Parse(Convert.ToString(reader["Options"]));
            if (Options == null)        // in case the string is garbarge, defend as we need a good Options object
                Options = new JObject();
        }

        public EDCommander(int id, string Name)
        {
            this.Id = id;
            this.Name = Name;
        }

        #endregion

        #region DB

        public static EDCommander Add(EDCommander other)
        {
            return Add(other.Name, other.EdsmName, other.EDSMAPIKey, other.JournalDir,
                                other.SyncToEdsm, other.SyncFromEdsm,
                                other.SyncToEddn,
                                other.SyncToInara, other.InaraName, other.InaraAPIKey,
                                other.HomeSystem, other.MapColour,
                                other.Options.ToString());
        }

        public static EDCommander Add(string name, string edsmName = null, string edsmApiKey = null, string journalpath = null,
                                        bool toedsm = false, bool fromedsm = false,
                                        bool toeddn = true,
                                        bool toinara = false, string inaraname = null, string inaraapikey = null,
                                        string homesystem = null, int mapcolour = -1,
                                        string options = "{\"SUBFOLDERS\":false}")      // default now is no subfolders
        {
            lock (locker)
            {
                EDCommander cmdr = UserDatabase.Instance.DBWrite<EDCommander>(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("INSERT INTO Commanders (Name,EdsmName,EdsmApiKey,JournalDir,Deleted, SyncToEdsm, SyncFromEdsm, SyncToEddn, NetLogDir, SyncToEGO, EGOName, EGOAPIKey, SyncToInara, InaraName, InaraAPIKey, HomeSystem, MapColour, MapCentreOnSelection, MapZoom, SyncToIGAU,Options) " +
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
                        cmd.AddParameterWithValue("@MapCentreOnSelection", false); // unused since 15.0
                        cmd.AddParameterWithValue("@MapZoom", 0); // unused since 15.0
                        cmd.AddParameterWithValue("@SyncToIGAU", false);    // removed 17.0.1 by request
                        cmd.AddParameterWithValue("@Options", options);
                        cmd.ExecuteNonQuery();
                    }

                    using (DbCommand cmd = cn.CreateCommand("SELECT * FROM Commanders WHERE rowid = last_insert_rowid()"))
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

                commanders[cmdr.Id] = cmdr; // update cache
                return cmdr;
            }
        }

        public static void Update(EDCommander cmdr)
        {
            lock (locker)
            {
                UserDatabase.Instance.DBWrite(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand(
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
                        cmd.AddParameterWithValue("@MapCentreOnSelection", false); // unused
                        cmd.AddParameterWithValue("@MapZoom", 0); // unused
                        cmd.AddParameterWithValue("@SyncToIGAU", false); // removed 17.0.1 by request
                        cmd.AddParameterWithValue("@Options", cmdr.Options.ToString());
                        cmd.ExecuteNonQuery();

                        commanders[cmdr.Id] = cmdr; // update cache
                    }
                });
            }
        }

        // if permdelete, its wiped. If not, its marked deleted 
        public static void Delete(int id, bool permdelete = false)
        {
            lock (locker)
            {
                if (permdelete)
                {
                    commanders.Remove(id);      // if perm delete, remove it completely
                }
                else
                {
                    commanders[id].Deleted = true;  // else mark deleted
                }

                if (ActiveCommanderListInt().Count() == 0)     // must have 1 active commander, make it so
                {
                    Add("Jameson (Default)", toeddn: false);
                }

                if (currentid == id)           // if the current commander is the one we are deleting
                {
                    SetIDInt(ActiveCommanderListInt().First().Id);    // first active commander ID
                }

                UserDatabase.Instance.DBWrite(cn =>
                {
                    if (permdelete)
                    {
                        using (DbCommand cmd = cn.CreateCommand("DELETE FROM Commanders WHERE Id = @Id"))
                        {
                            cmd.AddParameterWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (DbCommand cmd = cn.CreateCommand("UPDATE Commanders SET Deleted = 1 WHERE Id = @Id"))
                        {
                            cmd.AddParameterWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                });
            }
        }

        public static void UndeleteAllCommanders()
        {
            lock (locker)
            {
                foreach (var kvp in commanders)     // all deleted flags to zero
                    kvp.Value.Deleted = false;

                UserDatabase.Instance.DBWrite(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("UPDATE Commanders SET Deleted=0 WHERE Deleted=1"))
                    {
                        cmd.ExecuteNonQuery();
                    }
                });
            }
        }

        #endregion


        #region Static Properties

        // current ID, set or get
        public static int CurrentCmdrID
        {
            get
            {
                return currentid;
            }
            set
            {
                lock (locker)
                    SetIDInt(value);
            }
        }

        // Get Commander data for current
        public static EDCommander Current
        {
            get
            {
                lock (locker)
                    return commanders[CurrentCmdrID];
            }
        }

        // null if not valid - cope with it. Hidden/Deleted gets returned.
        public static EDCommander GetCommander(int id)
        {
            lock (locker)
                return commanders.ContainsKey(id) ? commanders[id] : null;
        }

        // gets a commander, including hidden or deleted
        // if two commanders have same name, for whatever reason, one is deleted, one is not, prefers active commander
        // null if not there
        public static EDCommander GetCommander(string name)
        {
            lock (locker)
            {
                var cmdr = commanders.Values.FirstOrDefault(c => c.Deleted == false && c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (cmdr == null)
                {
                    //System.Diagnostics.Debug.WriteLine($"Failed to find active commander, try deleted");
                    cmdr = commanders.Values.FirstOrDefault(c => c.Deleted == true && c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                }
                return cmdr;
            }
        }

        // gets a commander, including hidden or deleted
        public static bool IsCommanderPresent(string name)
        {
            lock (locker)
                return commanders.Values.ToList().FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != -1;
        }
        public static bool IsLegacyCommander(int id)
        {
            lock (locker)
                return GetCommander(id)?.LegacyCommander ?? false;
        }

        // Commander list active and deleted, not hidden log, sorted by name
        public static List<EDCommander> GetListActiveDeletedCommanders()
        {
            lock (locker)
                return commanders.Values.Where(x => x.Id >= 0).OrderBy(v => v.Name).ToList();
        }

        // Commander list active and hidden log, without deleted, sorted by name
        public static List<EDCommander> GetListActiveHiddenCommanders()
        {
            lock (locker)
                return commanders.Values.Where(x => x.Deleted == false).OrderBy(v => v.Name).ToList();
        }

        // Commander list without deleted or hidden, sorted by name
        public static List<EDCommander> GetListActiveCommanders()
        {
            lock (locker)
                return commanders.Values.Where(v => v.Id >= 0 && v.Deleted == false).OrderBy(v => v.Name).ToList();
        }

        // Commander list of deleted commanders, sorted by name
        public static List<EDCommander> GetListDeletedCommanders()
        {
            lock (locker)
                return commanders.Values.Where(x => x.Id >= 0 && x.Deleted == true).OrderBy(v => v.Name).ToList();
        }

        // count of adds or updates done during this session - helper value not changed by this code
        public static int AddOrUpdateCount { get { return addorupdatecount; } set { addorupdatecount++; } }

        // load all commanders (hidden, deleted, active) into memory and make sure current commander is set to an active commander
        public static void LoadCommanders()
        {
            lock (locker)
            {
                commanders = new Dictionary<int, EDCommander>();

                UserDatabase.Instance.DBRead(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("SELECT * FROM Commanders"))
                    {
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EDCommander edcmdr = new EDCommander(reader);
                                commanders.Add(edcmdr.Id, edcmdr);
                            }
                        }
                    }
                });

                if (ActiveCommanderListInt().Count() == 0)     // we must have 1 active commander
                {
                    Add("Jameson (Default)", toeddn: false);        
                }

                EDCommander hidden = new EDCommander(-1, "Hidden Log");     // -1 is the hidden commander, add to list
                commanders[-1] = hidden;                                    // so we give back a valid entry when its selected

                currentid = UserDatabase.Instance.GetSettingInt("ActiveCommander", 1);

                if (!commanders.ContainsKey(currentid) || commanders[currentid].Deleted)              // if not in list, or deleted (somehow without changing commander)
                {
                    SetIDInt(ActiveCommanderListInt().First().Id);    // first active commander ID
                }
            }
        }

        #endregion

        #region Private properties and methods

        private static object locker = new object();        // locker is needed because a thread during scanning can add a commander
        private static Dictionary<int, EDCommander> commanders; // list incl hidden and deleted
        private static int currentid = Int32.MinValue;
        private static int addorupdatecount = 0;        // add count during session - updated on new commander found by scanner
        
        private bool syncfromedsm = false;
        private bool synctoedsm = false;
        private bool synctoinara = false;
        private string homesystem = "";
        private ISystem lookuphomesys = null;
        private string lastlookuphomename = null;

        // active list, no lock, used internally
        private static List<EDCommander> ActiveCommanderListInt() { return commanders.Values.Where(v=>v.Id>=0 && v.Deleted == false).ToList(); }   

        // set ID, no lock, used internally
        private static void SetIDInt(int value)
        {
            if (value != currentid && commanders.ContainsKey(value) && commanders[value].Deleted == false)  // must be there and not deleted
            {
                currentid = value;
                UserDatabase.Instance.PutSettingInt("ActiveCommander", value);
            }
        }

        #endregion

    }
}
