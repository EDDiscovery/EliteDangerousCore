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
                return SystemsDB.ParseJSONString(jlist.ToString(), null, 10000, ref unusedate, () => false, (t) => { }, "");
            }

            return 0;
        }

        public static long ParseJSONFile(string filename, bool[] grididallow, int maxblocksize, ref DateTime date, 
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
                            return ParseJSONTextReader(sr, grididallow, maxblocksize, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
                        }
                    }
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                    return ParseJSONTextReader(sr,  grididallow, maxblocksize, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
            }
        }

        public static long ParseJSONString(string data, bool[] grididallow, int maxblocksize, ref DateTime date, Func<bool> cancelRequested, Action<string> reportProgress, string tableposfix, bool presumeempty = false, string debugoutputfile = null)
        {
            using (StringReader sr = new StringReader(data))         // read directly from file..
                return ParseJSONTextReader(sr,  grididallow, maxblocksize, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
        }

        // set tempostfix to use another set of tables

        public static long ParseJSONTextReader(TextReader textreader,
                                        bool[] grididallowed,       // null = all, else grid bool value
                                        int maxblocksize,
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

            System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS",true)} System DB store start");

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

                    // read records from JSON,

                    int recordstostore = ReadBlockFromJSON(cache, enumerator, grididallowed, maxblocksize, tablesareempty, tablepostfix, ref maxdate, ref nextsectorid, out bool jr_eof);

                    System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Process Block {recordstostore} {maxdate}");

                    if (recordstostore > 0)
                    {
                        updates += StoreNewEntries(cache, tablepostfix, sw);

                        System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} .. Update Block finished");

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

            System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} System DB End {updates} maxdate {maxdate} maxsec {nextsectorid}");
            reportProgress?.Invoke("Star database updated " + updates);

            PutNextSectorID(nextsectorid);    // and store back

            return updates;
        }

        #endregion

        #region Table Update Helpers
        private static int ReadBlockFromJSON( SectorCache sectorcache,
                                         IEnumerator<JToken> enumerator,
                                         bool[] grididallowed,       // null = all, else grid bool value
                                         int maxstoresize,
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

                            // try to add tablewritedata to sectorcache list
                            // do not make new sectors if tables are present, instead return false, and we add to unknownsectorentries
                            // if sector is in cache, add data to sectorcache.datalist

                            if (!TryCreateSectorCacheEntry(sectorcache, data, tablesareempty, ref cpmaxdate, ref cpnextsectorid, out Sector sector , false))
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

                    if (recordstostore >= maxstoresize)
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

                    
                    foreach (var data in unknownsectorentries) // for each star system
                    {
                        // add the data to the sector list.  Create the sector if required.

                        TryCreateSectorCacheEntry(sectorcache, data, tablesareempty, ref cpmaxdate, ref cpnextsectorid, out Sector sector, true);

                        if (sector.SId == -1)   // if unknown sector ID..
                        {
                            selectSectorCmd.Parameters[0].Value = sector.Name;
                            selectSectorCmd.Parameters[1].Value = sector.GId;

                            using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // find name:gid
                            {
                                if (reader.Read())      // if found name:gid
                                {
                                    sector.SId = (long)reader[0];
                                }
                                else
                                {
                                    sector.SId = cpnextsectorid++;      // insert the sector with the guessed ID
                                    sector.insertsec = true;
                                }

                                sectorcache.SectorIDCache[sector.SId] = sector;                // and cache
                                //System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                            }
                        }
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


        // given sectorcache, and system data
        // if sector given in system data is present, and gridid matches sector, then store data into sector datalist 
        // if sector is not present with/or wrong grid id, either create a new sector with the right gridid, or return false saying we need to do it later
        // tablesareempty means the tables are fresh and this is the first read, which means we can just go and make a new sector
        // makenewiftablesarepresent allows new sectors to be made even with the tables being non empty
        // false means tables are not empty , not making new, and sector not found in cache.. 
        // true means sector is found, and entry is added to sector data update list
        private static bool TryCreateSectorCacheEntry(SectorCache sectorcache, TableWriteData systemdata, bool tablesareempty, ref DateTime maxdate, ref int nextsectorid, 
                                                out Sector sector, bool makenewiftablesarepresent = false)
        {
            if (systemdata.starentry.date > maxdate)                                   // for all, record last recorded date processed
                maxdate = systemdata.starentry.date;

            Sector prev = null;

            sector = null;

            // if unknown name

            if (!sectorcache.SectorNameCache.ContainsKey(systemdata.classifier.SectorName))   
            {
                if (!tablesareempty && !makenewiftablesarepresent)        // if the tables are NOT empty and we can't make new ones, return false, saying you deal with it
                {
                    return false;
                }

                // make a sector of sectorname and with gridID n , sector id == -1

                sectorcache.SectorNameCache[systemdata.classifier.SectorName] = sector = new Sector(systemdata.classifier.SectorName, gridid: systemdata.gridid);   
            }
            else
            {
                // find the sector head by name
                sector = sectorcache.SectorNameCache[systemdata.classifier.SectorName];        

                while (sector != null && sector.GId != systemdata.gridid)        // if GID of sector disagrees, go thru the linked list
                {
                    prev = sector;                     
                    sector = sector.NextSector;
                }

                if (sector == null)      // still not got it, its a new one.
                {
                    if (!tablesareempty && !makenewiftablesarepresent) // if the tables are NOT empty and we can't make new ones, return false, saying you deal with it
                    {
                        return false;
                    }

                    // make a sector of sectorname and with gridID n , sector id == -1, and link the previous sector to it to form a linked list

                    prev.NextSector = sector = new Sector(systemdata.classifier.SectorName, gridid: systemdata.gridid);   
                }
            }

            if (sector.SId == -1)   // if unknown sector ID..
            {
                if (tablesareempty)     // if tables are empty, we can just presume its id
                {
                    sector.SId = nextsectorid++;      // insert the sector with the guessed ID
                    sector.insertsec = true;
                    sectorcache.SectorIDCache[sector.SId] = sector;    // and cache
                    //System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                }
            }

            if (sector.datalist == null)
                sector.datalist = new List<TableWriteData>(5000);

            sector.datalist.Add(systemdata);                       // add to list of systems to process for this sector

            return true;
        }

        // Given the sector cache, update the DB
        private static long StoreNewEntries(SectorCache sectorcache, string tablepostfix = "", StreamWriter debugfile = null )
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

                    replaceSysCmd = cn.CreateReplace("SystemTable" + tablepostfix, new string[] { "sectorid", "nameid", "x", "y", "z", "edsmid", "info" },
                                        new DbType[] { DbType.Int64, DbType.Int64, DbType.Int32, DbType.Int32, DbType.Int32, DbType.Int64, DbType.Int64 }, txn);

                    replaceNameCmd = cn.CreateReplace("Names" + tablepostfix, new string[] { "name", "id" }, new DbType[] { DbType.String, DbType.Int64 }, txn);

                    foreach (var kvp in sectorcache.SectorIDCache)                  // all sectors cached, id is unique so its got all sectors                           
                    {
                        Sector sector = kvp.Value;

                        if (sector.insertsec)         // if we have been told to insert the sector, do it
                        {
                            replaceSectorCmd.Parameters[0].Value = sector.Name;     // make a new one so we can get the ID
                            replaceSectorCmd.Parameters[1].Value = sector.GId;
                            replaceSectorCmd.Parameters[2].Value = sector.SId;        // and we insert with ID, managed by us, and replace in case there are any repeat problems (which there should not be)
                            replaceSectorCmd.ExecuteNonQuery();
                            //System.Diagnostics.Debug.WriteLine("Written sector " + t.GId + " " +t.Name);
                            sector.insertsec = false;
                        }

                        if (sector.datalist != null)       // if updated..
                        {
#if DEBUG
                            sector.datalist.Sort(delegate (TableWriteData left, TableWriteData right) { return left.starentry.id.CompareTo(right.starentry.id); });
#endif

                            foreach (var data in sector.datalist)            // now write the star list in this sector
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

                                    replaceSysCmd.Parameters[0].Value = sector.SId;
                                    replaceSysCmd.Parameters[1].Value = data.classifier.ID;
                                    replaceSysCmd.Parameters[2].Value = data.starentry.x;
                                    replaceSysCmd.Parameters[3].Value = data.starentry.y;
                                    replaceSysCmd.Parameters[4].Value = data.starentry.z;
                                    replaceSysCmd.Parameters[5].Value = data.starentry.id;       // in the event a new entry has the same id, the system table id is replace with new data
                                    replaceSysCmd.Parameters[6].Value = (object)data.starentry.startype ?? System.DBNull.Value;       // if we have a startype, send it in, else DBNull
                                    replaceSysCmd.ExecuteNonQuery();

                                    if (debugfile != null)
                                        debugfile.WriteLine(data.starentry.name + " " + data.starentry.x + "," + data.starentry.y + "," + data.starentry.z + ", ID:" + data.starentry.id + " Grid:" + data.gridid);

                                    updates++;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("general exception during insert - ignoring " + ex.ToString());
                                }

                            }
                        }

                        sector.datalist = null;     // and delete back
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


