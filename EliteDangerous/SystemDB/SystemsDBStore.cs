/*
 * Copyright 2015 - 2023 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Common;
using System.Data;
using System.IO.Compression;

namespace EliteDangerousCore.DB
{
    // EDSM Store - db.edsmid contains edsmid, and db.info is null
    // Spansh Store - db.edsmid contains the system address, and db.info is non null

    public partial class SystemsDB
    {
        #region Table Update from JSON FILE

        // store systems to DB.  Data is checked against the mode. Verified april 23 with a load against an empty db in spansh and edsm mode
        public static long StoreSystems(IEnumerable<ISystem> systems)
        {
            JArray jlist = new JArray();

            string currentdb = SystemsDatabase.Instance.GetDBSource();
            bool spansh = currentdb.Equals("SPANSH");

            foreach (var sys in systems)
            {
                // so we need coords, and if edsm db, we need an edsm id, or for spansh we need a system address
                if (sys.HasCoordinate && ((!spansh && sys.EDSMID.HasValue) || (spansh && sys.SystemAddress.HasValue)))  
                {
                    JObject jo = new JObject
                    {
                        ["name"] = sys.Name,
                        ["coords"] = new JObject { ["x"] = sys.X, ["y"] = sys.Y, ["z"] = sys.Z }
                    };

                    if ( spansh )       // we format either for a spansh DB or an EDSM db
                    {
                        jo["id64"] = sys.SystemAddress.Value;
                        jo["updateTime"] = DateTime.UtcNow;
                    }
                    else
                    {
                        jo["id"] = sys.EDSMID.Value;
                        jo["date"] = DateTime.UtcNow;
                    }

                    jlist.Add(jo);
                }
            }

            if ( jlist.Count>0)
            { 
                DateTime unusedate = DateTime.UtcNow;
                return SystemsDB.ParseJSONString(jlist.ToString(), null, ref unusedate, () => false, (t) => { }, "");
            }

            return 0;
        }

        public static long ParseJSONFile(string filename, bool[] grididallow, ref DateTime date, 
                                             Func<bool> cancelRequested, Action<string> reportProgress, 
                                             string tableposfix, bool presumeempty = false, string debugoutputfile = null)
        {
            // if the filename ends in .gz, then decompress it on the fly
            if (filename.EndsWith("gz"))
            {
                using (FileStream originalFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (GZipStream gz = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new StreamReader(gz))
                        {
                            return ParseJSONTextReader(sr, grididallow, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
                        }
                    }
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                    return ParseJSONTextReader(sr,  grididallow, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
            }
        }

        public static long ParseJSONString(string data, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, Action<string> reportProgress, string tableposfix, bool presumeempty = false, string debugoutputfile = null)
        {
            using (StringReader sr = new StringReader(data))         // read directly from file..
                return ParseJSONTextReader(sr,  grididallow, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
        }

        // set tempostfix to use another set of tables

        public static long ParseJSONTextReader(TextReader textreader,
                                        bool[] grididallowed,       // null = all, else grid bool value
                                        ref DateTime maxdate,       // updated with latest date
                                        Func<bool> cancelRequested,
                                        Action<string> reportProgress,
                                        string tablepostfix,        // set to add on text to table names to redirect to another table
                                        bool tablesareempty = false,     // set to presume table is empty, so we don't have to do look up queries
                                        string debugoutputfile = null
                                        )
        {
            var cache = new SectorCache();

            long updates = 0;

            int nextsectorid = GetNextSectorID();
            StreamWriter sw = null;

            try
            {
#if DEBUG
                try
                {
                    if (debugoutputfile != null) sw = new StreamWriter(debugoutputfile);
                }
                catch
                {
                }
#endif
                var parser = new QuickJSON.Utils.StringParserQuickTextReader(textreader, 32768);
                var enumerator = JToken.ParseToken(parser, JToken.ParseOptions.None).GetEnumerator();       // Parser may throw note

                while (true)
                {
                    if (cancelRequested())
                    {
                        updates = -1;
                        break;
                    }

                    int recordstostore = ProcessBlock(cache, enumerator, grididallowed, tablesareempty, tablepostfix, ref maxdate, ref nextsectorid, out bool jr_eof);

                    System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("L1")} Process Block {recordstostore} {maxdate}");

                    if (recordstostore > 0)
                    {
                        updates += StoreNewEntries(cache, tablepostfix, sw);

                        reportProgress?.Invoke("Star database updated " + recordstostore + " total so far " + updates);
                    }

                    if (jr_eof)
                        break;

                    System.Threading.Thread.Sleep(20);      // just sleepy for a bit to let others use the db
                }
            }
            catch ( Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception during SystemDB parse " + ex);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }

            System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("L1")} End Process {updates} maxdate {maxdate} maxsec {nextsectorid}");
            reportProgress?.Invoke("Star database updated " + updates);

            PutNextSectorID(nextsectorid);    // and store back

            return updates;
        }

        #endregion

        #region Table Update Helpers
        private static int ProcessBlock( SectorCache cache,
                                         IEnumerator<JToken> enumerator,
                                         bool[] grididallowed,       // null = all, else grid bool value
                                         bool tablesareempty,
                                         string tablepostfix,
                                         ref DateTime maxdate,       // updated with latest date
                                         ref int nextsectorid,
                                         out bool jr_eof)
        {
            int recordstostore = 0;
            DbCommand selectSectorCmd = null;
            DateTime cpmaxdate = maxdate;
            int cpnextsectorid = nextsectorid;
            const int BlockSize = 1000000;      // for 66mil stars, 20000 = 38.66m, 100000=34.67m, 1e6 = 28.02m
            int Limit = int.MaxValue;
            var unknownsectorentries = new List<TableWriteData>();
            jr_eof = false;

            while (true)
            {
                if ( !enumerator.MoveNext())        // get next token, if not, stop eof
                {
                    jr_eof = true;
                    break;
                }

                JToken t = enumerator.Current;

                if ( t.IsObject )                   // if start of object..
                {
                    StarFileEntry d = new StarFileEntry();

                    // if we have a valid record
                    if (d.Deserialize(enumerator))     
                    {
                        int gridid = GridId.Id128(d.x, d.z);
                        if (grididallowed == null || (grididallowed.Length > gridid && grididallowed[gridid]))    // allows a null or small grid
                        {
                            TableWriteData data = new TableWriteData() { starentry = d, classifier = new EliteNameClassifier(d.name), gridid = gridid };

                            // try and add data to sector
                            // if sector is not in cache, do not make it, return false, instead add to entries
                            // if sector is in cache, add it to the sector update list, return false,
                            // so this accumulates entries which need new sectors.
                            if (!TryCreateNewUpdate(cache, data, tablesareempty, ref cpmaxdate, ref cpnextsectorid, out Sector sector , false))
                            {
                                unknownsectorentries.Add(data); // unknown sector, process below
                            }

                            recordstostore++;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to deserialise");
                    }

                    if (--Limit == 0)
                    {
                        jr_eof = true;
                        break;
                    }

                    if (recordstostore >= BlockSize)
                        break;
                }
            }

            // for unknownsectorentries, create sectors in cache for them

            SystemsDatabase.Instance.DBRead( db =>
            {
                try
                {
                    var cn = db;

                    selectSectorCmd = cn.CreateSelect("Sectors" + tablepostfix, "id", "name = @sname AND gridid = @gid", null,
                                                            new string[] { "sname", "gid" }, new DbType[] { DbType.String, DbType.Int32 });

                    foreach (var entry in unknownsectorentries)
                    {
                        CreateSectorInCacheIfRequired(cache, selectSectorCmd, entry, tablesareempty, ref cpmaxdate, ref cpnextsectorid);
                    }
                }
                finally
                {
                    if (selectSectorCmd != null)
                    {
                        selectSectorCmd.Dispose();
                    }
                }
            });

            maxdate = cpmaxdate;
            nextsectorid = cpnextsectorid;

            return recordstostore;
        }


        // create a new entry for insert in the sector tables 
        // tablesareempty means the tables are fresh and this is the first read
        // makenewiftablesarepresent allows new sectors to be made
        // false means tables are not empty , not making new, and sector not found in cache.. 
        // true means sector is found, and entry is added to sector update list
        private static bool TryCreateNewUpdate(SectorCache cache, TableWriteData data, bool tablesareempty, ref DateTime maxdate, ref int nextsectorid, 
                                                out Sector t, bool makenewiftablesarepresent = false)
        {
            if (data.starentry.date > maxdate)                                   // for all, record last recorded date processed
                maxdate = data.starentry.date;

            Sector prev = null;

            t = null;

            if (!cache.SectorNameCache.ContainsKey(data.classifier.SectorName))   // if unknown to cache
            {
                if (!tablesareempty && !makenewiftablesarepresent)        // if the tables are NOT empty and we can't make new..
                {
                    return false;
                }

                cache.SectorNameCache[data.classifier.SectorName] = t = new Sector(data.classifier.SectorName, gridid: data.gridid);   // make a sector of sectorname and with gridID n , id == -1
            }
            else
            {
                t = cache.SectorNameCache[data.classifier.SectorName];        // find the first sector of name
                while (t != null && t.GId != data.gridid)        // if GID of sector disagrees
                {
                    prev = t;                          // go thru list
                    t = t.NextSector;
                }

                if (t == null)      // still not got it, its a new one.
                {
                    if (!tablesareempty && !makenewiftablesarepresent)
                    {
                        return false;
                    }

                    prev.NextSector = t = new Sector(data.classifier.SectorName, gridid: data.gridid);   // make a sector of sectorname and with gridID n , id == -1
                }
            }

            if (t.SId == -1)   // if unknown sector ID..
            {
                if (tablesareempty)     // if tables are empty, we can just presume its id
                {
                    t.SId = nextsectorid++;      // insert the sector with the guessed ID
                    t.insertsec = true;
                    cache.SectorIDCache[t.SId] = t;    // and cache
                    //System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                }
            }

            if (t.datalist == null)
                t.datalist = new List<TableWriteData>(5000);

            t.datalist.Add(data);                       // add to list of systems to process for this sector

            return true;
        }

        // add the data to the sector cache, making it if required.
        // If it was made (id==-1) then find sector, and if not there, assign an ID
        private static void CreateSectorInCacheIfRequired(SectorCache cache, DbCommand selectSectorCmd, TableWriteData data, bool tablesareempty, ref DateTime maxdate, ref int nextsectorid)
        {
            // force the entry into the sector cache.
            TryCreateNewUpdate(cache, data, tablesareempty, ref maxdate, ref nextsectorid, out Sector t, true);

            if (t.SId == -1)   // if unknown sector ID..
            {
                selectSectorCmd.Parameters[0].Value = t.Name;   
                selectSectorCmd.Parameters[1].Value = t.GId;

                using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // find name:gid
                {
                    if (reader.Read())      // if found name:gid
                    {
                        t.SId = (long)reader[0];
                    }
                    else
                    {
                        t.SId = nextsectorid++;      // insert the sector with the guessed ID
                        t.insertsec = true;
                    }

                    cache.SectorIDCache[t.SId] = t;                // and cache
                    //  System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                }
            }
        }

        private static long StoreNewEntries(SectorCache cache, string tablepostfix = "",        // set to add on text to table names to redirect to another table
                                           StreamWriter sw = null
                                        )
        {
            ////////////////////////////////////////////////////////////// push all new data to the db without any selects

            return SystemsDatabase.Instance.DBWrite(db =>
            {
                long updates = 0;

                DbTransaction txn = null;
                DbCommand replaceSectorCmd = null;
                DbCommand replaceSysCmd = null;
                DbCommand replaceNameCmd = null;
                try
                {
                    var cn = db;
                    txn = cn.BeginTransaction();

                    replaceSectorCmd = cn.CreateReplace("Sectors" + tablepostfix, new string[] { "name", "gridid", "id" }, new DbType[] { DbType.String, DbType.Int32, DbType.Int64 }, txn);

                    replaceSysCmd = cn.CreateReplace("Systems" + tablepostfix, new string[] { "sectorid", "nameid", "x", "y", "z", "edsmid", "info" },
                                        new DbType[] { DbType.Int64, DbType.Int64, DbType.Int32, DbType.Int32, DbType.Int32, DbType.Int64, DbType.Int64 }, txn);

                    replaceNameCmd = cn.CreateReplace("Names" + tablepostfix, new string[] { "name", "id" }, new DbType[] { DbType.String, DbType.Int64 }, txn);

                    foreach (var kvp in cache.SectorIDCache)                  // all sectors cached, id is unique so its got all sectors                           
                    {
                        Sector t = kvp.Value;

                        if (t.insertsec)         // if we have been told to insert the sector, do it
                        {
                            replaceSectorCmd.Parameters[0].Value = t.Name;     // make a new one so we can get the ID
                            replaceSectorCmd.Parameters[1].Value = t.GId;
                            replaceSectorCmd.Parameters[2].Value = t.SId;        // and we insert with ID, managed by us, and replace in case there are any repeat problems (which there should not be)
                            replaceSectorCmd.ExecuteNonQuery();
                            //System.Diagnostics.Debug.WriteLine("Written sector " + t.GId + " " +t.Name);
                            t.insertsec = false;
                        }

                        if (t.datalist != null)       // if updated..
                        {
#if DEBUG
                            t.datalist.Sort(delegate (TableWriteData left, TableWriteData right) { return left.starentry.id.CompareTo(right.starentry.id); });
#endif

                            foreach (var data in t.datalist)            // now write the star list in this sector
                            {
                                try
                                {
                                    if (data.classifier.IsNamed)    // if its a named entry, we need a name
                                    {
                                        data.classifier.NameIdNumeric = data.starentry.id;           // name is the id
                                        replaceNameCmd.Parameters[0].Value = data.classifier.StarName;       // insert a new name
                                        replaceNameCmd.Parameters[1].Value = data.starentry.id;      // we use the systems id as the nameid, and use replace to ensure that if a prev one is there, its replaced
                                        replaceNameCmd.ExecuteNonQuery();
                                        // System.Diagnostics.Debug.WriteLine("Make name " + data.classifier.NameIdNumeric);
                                    }

                                    replaceSysCmd.Parameters[0].Value = t.SId;
                                    replaceSysCmd.Parameters[1].Value = data.classifier.ID;
                                    replaceSysCmd.Parameters[2].Value = data.starentry.x;
                                    replaceSysCmd.Parameters[3].Value = data.starentry.y;
                                    replaceSysCmd.Parameters[4].Value = data.starentry.z;
                                    replaceSysCmd.Parameters[5].Value = data.starentry.id;       // in the event a new entry has the same id, the system table id is replace with new data
                                    replaceSysCmd.Parameters[6].Value = (object)data.starentry.startype ?? System.DBNull.Value;       // if we have a startype, send it in, else DBNull
                                    replaceSysCmd.ExecuteNonQuery();

                                    if (sw != null)
                                        sw.WriteLine(data.starentry.name + " " + data.starentry.x + "," + data.starentry.y + "," + data.starentry.z + ", ID:" + data.starentry.id + " Grid:" + data.gridid);

                                    updates++;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("general exception during insert - ignoring " + ex.ToString());
                                }

                            }
                        }

                        t.datalist = null;     // and delete back
                    }

                    txn.Commit();

                    return updates;
                }
                finally
                {
                    replaceSectorCmd?.Dispose();
                    replaceSysCmd?.Dispose();
                    replaceNameCmd?.Dispose();
                    txn?.Dispose();
                }
            },warnthreshold:5000);
        }

        #endregion

        #region Internal Vars and Classes

        private static int GetNextSectorID() { return SystemsDatabase.Instance.GetSectorIDNext(); }
        private static void PutNextSectorID(int v) { SystemsDatabase.Instance.SetSectorIDNext(v); }  

        private class SectorCache
        {
            public Dictionary<long, Sector> SectorIDCache { get; set; } = new Dictionary<long, Sector>();          // only used during store operation
            public Dictionary<string, Sector> SectorNameCache { get; set; } = new Dictionary<string, Sector>();
        }

        private class Sector
        {
            public long SId;
            public int GId;
            public string Name;

            public Sector NextSector;       // memory only field, link to next in list

            public Sector(string name, long id = -1, int gridid = -1 )
            {
                this.SId = id;
                this.GId = gridid;
                this.Name = name;
                this.NextSector = null;
            }

            // for write table purposes only

            public List<TableWriteData> datalist;
            public bool insertsec = false;
        };

        private class TableWriteData
        {
            public StarFileEntry starentry;
            public EliteNameClassifier classifier;
            public int gridid;
        }

        public class StarFileEntry
        {
            public bool Deserialize(IEnumerator<JToken> enumerator)
            {
                bool spansh = false;

                while (enumerator.MoveNext() && enumerator.Current.IsProperty)   // while more tokens, and JProperty
                {
                    var p = enumerator.Current;
                    string field = p.Name;

                    switch (field)
                    {
                        case "name":
                            name = p.StrNull();
                            break;
                        case "id":      // EDSM name
                            id = p.ULong();
                            break;
                        case "id64":      // EDSM and Spansh name. 
                            systemaddress = p.ULong();
                            break;
                        case "date":        // edsm name
                            date = p.DateTimeUTC();
                            spansh = false;
                            break;
                        case "updateTime":        // Spansh name
                            date = p.DateTimeUTC();
                            spansh = true;
                            break;
                        case "coords":
                            {
                                while (enumerator.MoveNext() && enumerator.Current.IsProperty)   // while more tokens, and JProperty
                                {
                                    var cp = enumerator.Current;
                                    field = cp.Name;
                                    double? v = cp.DoubleNull();
                                    if (v == null)
                                        return false;
                                    int vi = (int)(v * SystemClass.XYZScalar);

                                    switch (field)
                                    {
                                        case "x":
                                            x = vi;
                                            break;
                                        case "y":
                                            y = vi;
                                            break;
                                        case "z":
                                            z = vi;
                                            break;
                                    }
                                }

                                break;
                            }

                        case "mainStar":    // spansh
                            {
                                spansh = true;
                                string name = p.Str();
                                if (spanshtoedstar.TryGetValue(name, out EDStar value))
                                {
                                    startype = (int)value;
                                }
                                else
                                    System.Diagnostics.Debug.WriteLine($"DB read of spansh unknown star type {name}");
                                break;
                            }
                        default:        // any other, ignore
                            break;
                    }
                }

                if (spansh )                    // if detected spansh above
                {
                    if ( startype == null)      // for spansh, we always set startype non null, so that we know the db.edsmid is really the system address
                        startype = (int)EDStar.Unknown;
                    id = systemaddress;         // for spansh, the id
                }

                return id != ulong.MaxValue && name.HasChars() && x != int.MinValue && y != int.MinValue && z != int.MinValue && date != DateTime.MinValue;
            }

            public ulong id = ulong.MaxValue;                       //ID to use, either edsmid or system address
            public string name;
            public int x = int.MinValue;
            public int y = int.MinValue;
            public int z = int.MinValue;
            public int? startype;       // null default
            public DateTime date;

            private ulong systemaddress = ulong.MaxValue;

            // from https://spansh.co.uk/api/bodies/field_values/subtype
            static private Dictionary<string, EDStar> spanshtoedstar = new Dictionary<string, EDStar>
            {
                { "O (Blue-White) Star", EDStar.O },
                { "B (Blue-White) Star", EDStar.B },
                { "A (Blue-White) Star", EDStar.A },
                { "F (White) Star", EDStar.F },
                { "G (White-Yellow) Star", EDStar.G },
                { "K (Yellow-Orange) Star", EDStar.K },
                { "M (Red dwarf) Star", EDStar.M },

                { "L (Brown dwarf) Star", EDStar.L },
                { "T (Brown dwarf) Star", EDStar.T },
                { "Y (Brown dwarf) Star", EDStar.Y },

                { "Herbig Ae Be Star", EDStar.AeBe },
                { "T Tauri Star", EDStar.TTS },

                { "Wolf-Rayet Star", EDStar.W },
                { "Wolf-Rayet N Star", EDStar.WN },
                { "Wolf-Rayet NC Star", EDStar.WNC },
                { "Wolf-Rayet C Star", EDStar.WC },
                { "Wolf-Rayet O Star", EDStar.WO },

                // missing CS
                { "C Star", EDStar.C },
                { "CN Star", EDStar.CN },
                { "CJ Star", EDStar.CJ },
                // missing CHd

                { "MS-type Star", EDStar.MS },
                { "S-type Star", EDStar.S },

                { "White Dwarf (D) Star", EDStar.D },
                { "White Dwarf (DA) Star", EDStar.DA },
                { "White Dwarf (DAB) Star", EDStar.DAB },
                // missing DAO
                { "White Dwarf (DAZ) Star", EDStar.DAZ },
                { "White Dwarf (DAV) Star", EDStar.DAV },
                { "White Dwarf (DB) Star", EDStar.DB },
                { "White Dwarf (DBZ) Star", EDStar.DBZ },
                { "White Dwarf (DBV) Star", EDStar.DBV },
                // missing DO,DOV
                { "White Dwarf (DQ) Star", EDStar.DQ },
                { "White Dwarf (DC) Star", EDStar.DC },
                { "White Dwarf (DCV) Star", EDStar.DCV },
                // missing DX
                { "Neutron Star", EDStar.N },
                { "Black Hole", EDStar.H },
                // missing X but not confirmed with actual journal data


                { "A (Blue-White super giant) Star", EDStar.A_BlueWhiteSuperGiant },
                { "F (White super giant) Star", EDStar.F_WhiteSuperGiant },
                { "M (Red super giant) Star", EDStar.M_RedSuperGiant },
                { "M (Red giant) Star", EDStar.M_RedGiant},
                { "K (Yellow-Orange giant) Star", EDStar.K_OrangeGiant },
                // missing rogueplanet, nebula, stellarremanant
                { "Supermassive Black Hole", EDStar.SuperMassiveBlackHole },
                { "B (Blue-White super giant) Star", EDStar.B_BlueWhiteSuperGiant },
                { "G (White-Yellow super giant) Star", EDStar.G_WhiteSuperGiant },
            };

        }

        #endregion
    }
}


