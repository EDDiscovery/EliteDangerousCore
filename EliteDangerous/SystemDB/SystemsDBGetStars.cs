/*
 * Copyright 2015-2023 EDDiscovery development team
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

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        ///////////////////////////////////////// By Name

        internal static ISystem FindStar(string name)
        {
            return SystemsDatabase.Instance.DBRead(cn => FindStar(name, cn));
        }

        internal static ISystem FindStar(string name, SQLiteConnectionSystem cn)
        {
            EliteNameClassifier ec = new EliteNameClassifier(name);

            if (ec.IsNamed)
            {
                // needs index on sectorid [nameid]. Relies on Names.id being the systems.edsmid

                using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                    "s.edsmid IN (Select id FROM Names WHERE name=@p1) AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.StarName, ec.SectorName },
                                                    joinlist: MakeSystemQueryNamedJoinList))
                {
                    //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MakeSystem(reader);        // read back and make name from db info due to case problems.
                        }
                    }
                }

            }
            else
            {
                // Numeric or Standard - all data in ID
                // needs index on Systems(sectorid, Nameid)

                using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSysStdNumericQuery,
                                                    "s.nameid = @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.ID, ec.SectorName },
                                                    joinlist: MakeSysStdNumericQueryJoinList))
                {
                  //  System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MakeSystem(reader, ec.ID); // read back .. sector name is taken from DB for case reasons
                        }
                    }

                }
            }

            return null;
        }

        ///////////////////////////////////////// By Wildcard

        internal static List<ISystem> FindStarWildcard(string name, int limit = int.MaxValue)
        {
            return SystemsDatabase.Instance.DBRead(cn => FindStarWildcard(name, cn, limit), 2000);
        }

        internal static List<ISystem> FindStarWildcard(string name, SQLiteConnectionSystem cn, int limit = int.MaxValue)
        {
            EliteNameClassifier ec = new EliteNameClassifier(name);

            List<ISystem> ret = new List<ISystem>();

            if (ec.IsStandardParts)     // normal Euk PRoc qc-l d2-3
            {
                // needs index on Systems(sectorid, Nameid)

                using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                    "s.nameid >= @p1 AND s.nameid <= @p2 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p3)",
                                                    new Object[] { ec.ID, ec.IDHigh, ec.SectorName },
                                                    limit:limit,
                                                    joinlist: MakeSystemQueryNamedJoinList))
                {
                    //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SystemClass sc = MakeSystem(reader);
                            ret.Add(sc);
                        }
                    }
                }
            }
            else if (ec.IsNumeric)        // HIP 29282
            {
                // checked select *,s.nameid & 0x3fffffffff , cast((s.nameid & 0x3fffffffff) as text) From Systems  s where (s.nameid & (1<<46)!=0) and s.sectorid=15568 USNO entries
                // beware, 1<<46 works, 0x40 0000 0000 does not.. 
                // needs index on Systems(sectorid, Nameid)

                using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                    "(s.nameid & (1<<46) != 0) AND cast((s.nameid & 0x3fffffffff) as text) LIKE @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.NameIdNumeric.ToStringInvariant() + "%", ec.SectorName },
                                                    limit:limit,
                                                    joinlist: MakeSystemQueryNamedJoinList))  
                {

                    //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SystemClass sc = MakeSystem(reader);
                            ret.Add(sc);
                        }
                    }
                }
            }
            else
            {
                // if we have a starname component and a sector name, look up sectorname + starname%
                // lengths based on no stars with 1 char and no sectors with 1 char

                if (ec.SectorName != EliteNameClassifier.NoSectorName)     // we have a sector name
                {
                    if (ec.SectorName.Length > 1)   // its a decent length
                    {
                        if (ec.StarName.Length > 0) // and we have a star name (of any length as its been split)
                        {
                           // System.Diagnostics.Debug.WriteLine($"******************** {BaseUtils.AppTicks.TickCountLap("SS1", true)} Search sector-name {ec.SectorName} {ec.StarName}");

                            // needs index on Systems(sectorid, Nameid)

                            using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                                "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                                new Object[] { ec.StarName + "%", ec.SectorName },
                                                                limit: limit,
                                                                joinlist: MakeSystemQueryNamedJoinList))
                            {
                                //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

                                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SystemClass sc = MakeSystem(reader);
                                        ret.Add(sc);
                                    }

                                   // System.Diagnostics.Debug.WriteLine($"************** {BaseUtils.AppTicks.TickCountLap("SS1")} Search sector-name result {ret.Count}");

                                    limit -= ret.Count;
                                }
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine($"Sector sector - noname {ec.SectorName}");

                          //  System.Diagnostics.Debug.WriteLine($"******************** {BaseUtils.AppTicks.TickCountLap("SS1", true)} Search sector-noname {ec.SectorName} {ec.StarName}");

                            using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                                "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                                new Object[] { ec.SectorName + "%" },
                                                                limit: limit,
                                                                joinlist: MakeSystemQueryNamedJoinList))
                            {
                                //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

                                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SystemClass sc = MakeSystem(reader);
                                        ret.Add(sc);
                                    }

                              //      System.Diagnostics.Debug.WriteLine($"************** {BaseUtils.AppTicks.TickCountLap("SS1")} Search sector-noname result {ret.Count}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ec.StarName.Length >= 2)     // min 2 chars for name
                    {
                       // System.Diagnostics.Debug.WriteLine($"************** {BaseUtils.AppTicks.TickCountLap("SS1", true)} Search-NoSector-name, check names {ec.StarName}");

                        using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                            "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) ",
                                                            new Object[] { ec.StarName + "%" },
                                                            limit: limit,
                                                            joinlist: MakeSystemQueryNamedJoinList))
                        {
                            //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

                            using (DbDataReader reader = selectSysCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    SystemClass sc = MakeSystem(reader);
                                    ret.Add(sc);
                                }

                          //      System.Diagnostics.Debug.WriteLine($"**************** {BaseUtils.AppTicks.TickCountLap("SS1")}Search-NoSector-name, check names {ret.Count}");

                                limit -= ret.Count;
                            }
                        }



                        if (limit > 0)
                        {
                           // System.Diagnostics.Debug.WriteLine($"****************** {BaseUtils.AppTicks.TickCountLap("SS2", true)} Search-nosector-name, check sectors {ec.StarName}");
                            using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                                "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                                new Object[] { ec.StarName + "%" },
                                                                limit: limit,
                                                                joinlist: MakeSystemQueryNamedJoinList))
                            {
                                //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

                                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SystemClass sc = MakeSystem(reader);
                                        ret.Add(sc);
                                    }

                                 //   System.Diagnostics.Debug.WriteLine($"**************** {BaseUtils.AppTicks.TickCountLap("SS2")} Search-NoSector-name, check sectors {ret.Count}");

                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public static void DebugListNamedSectorStars()
        {
            SystemsDatabase.Instance.DBRead(cn =>
            {
                using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                "s.nameid < 10000000",
                                                joinlist: MakeSystemQueryNamedJoinList))
                {
                    //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        Dictionary<string, int> prefixes = new Dictionary<string, int>();
                        while (reader.Read())
                        {
                            SystemClass sc = MakeSystem(reader);
                            int spc = sc.Name.IndexOf(' ');
                            if (spc >= 0)
                            {
                                string p = sc.Name.Substring(0, spc);
                                if (!prefixes.ContainsKey(p))
                                    prefixes[p] = 1;
                                else
                                    prefixes[p] = prefixes[p] + 1;
                            }
                        }

                        foreach (var kvp in prefixes)
                        {
                            if (kvp.Value > 1)
                                System.Diagnostics.Debug.WriteLine($"Prefix {kvp.Key} = {kvp.Value}");
                        }

                    }
                }
            });

        }



        #region Helpers for getting stars

        //                                     0   1   2   3        4      5        6 
        const string MakeSysStdNumericQuery = "s.x,s.y,s.z,s.edsmid,c.name,c.gridid,s.info";
        static string[] MakeSysStdNumericQueryJoinList = new string[] { "JOIN Sectors c on s.sectorid=c.id" };

        static SystemClass MakeSystem(DbDataReader reader, ulong nid)
        {
            EliteNameClassifier ec = new EliteNameClassifier(nid);
            ec.SectorName = reader.GetString(4);

            bool isspansh = !reader.IsDBNull(6);

            return new SystemClass(ec.ToString(), 
                                        reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),         // xyz
                                        isspansh ? reader.GetInt64(3) : default(long?),     // for spansh carries in s.edsmid the system address
                                        isspansh ? default(long?) : reader.GetInt64(3),     // for edsm carriers in s.edsmid the edsmid
                                        reader.GetInt32(5),
                                        isspansh ? (EDStar)reader.GetInt32(6) : EDStar.Unknown, // spansh records have star set
                                        SystemSource.FromDB);   // gridid
        }

        //                                   0   1   2   3        4      5        6        7      8            
        const string MakeSystemQueryNamed = "s.x,s.y,s.z,s.edsmid,c.name,c.gridid,s.nameid,n.Name,s.info";
        static string[] MakeSystemQueryNamedJoinList = new string[] { "LEFT OUTER JOIN Names n On s.nameid=n.id", "JOIN Sectors c on s.sectorid=c.id" };

        static SystemClass MakeSystem(DbDataReader reader)
        {
            EliteNameClassifier ec = new EliteNameClassifier((ulong)reader.GetInt64(6));
            ec.SectorName = reader.GetString(4);

            if (ec.IsNamed)
                ec.StarName = reader.GetString(7);

            bool isspansh = !reader.IsDBNull(8);

            return new SystemClass(ec.ToString(), 
                                        reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),     // xyz
                                        isspansh ? reader.GetInt64(3) : default(long?),     // for spansh carries in s.edsmid the system address
                                        isspansh ? default(long?) : reader.GetInt64(3),     // for edsm carriers in s.edsmid the edsmid
                                        reader.GetInt32(5),
                                        isspansh ? (EDStar)reader.GetInt32(8) : EDStar.Unknown, // for spansh, presence of s.info signals that its a spansh record
                                        SystemSource.FromDB);   // gridid
        }

        #endregion

    }
}


