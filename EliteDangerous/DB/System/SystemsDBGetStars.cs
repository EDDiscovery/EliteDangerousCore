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


        // return list of stars matching name, case insensitive
        internal static List<ISystem> FindStars(string name)
        {
            return SystemsDatabase.Instance.DBRead(cn => FindStars(name, cn));
        }

        // return list of stars matching name, case insensitive
        // always returns a list, may be empty.
        internal static List<ISystem> FindStars(string name, SQLiteConnectionSystem cn)
        {
            EliteNameClassifier ec = new EliteNameClassifier(name);

            if (ec.IsNamed)
            {
                // needs index on sectorid [nameid]. Relies on Names.id being the systems.edsmid

                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                    "s.edsmid IN (Select id FROM Names WHERE name=@p1) AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.StarName, ec.SectorName },
                                                    joinlist: MakeSystemQueryNamedJoinList))
                {
                    //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        List<ISystem> systems = new List<ISystem>();
                        while (reader.Read())
                        {
                            systems.Add(MakeSystem(reader));        // read back and make name from db info due to case problems.
                        }

                        return systems;
                    }
                }

            }
            else
            {
                // Numeric or Standard - all data in ID
                // needs index on Systems(sectorid, Nameid)

                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSysStdNumericQuery,
                                                    "s.nameid = @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.ID, ec.SectorName },
                                                    joinlist: MakeSysStdNumericQueryJoinList))
                {
                    //  System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        List<ISystem> systems = new List<ISystem>();
                        while (reader.Read())
                        {
                            systems.Add(MakeSystem(reader, ec.ID)); // read back .. sector name is taken from DB for case reasons
                        }

                        return systems;
                    }
                }
            }
        }

        ///////////////////////////////////////// By Wildcard

        internal static List<ISystem> FindStarsWildcard(string name, int limit = int.MaxValue)
        {
            return SystemsDatabase.Instance.DBRead(cn => FindStarsWildcard(name, cn, limit), 2000);
        }


        // find stars using a star pattern
        // if its a standard pattern Euk PRoc qc-l d2-3 you can drop certain parts
        // wildcard pattern uses SQL % operator
        // always returns a system list, may be empty

        internal static List<ISystem> FindStarsWildcard(string name, SQLiteConnectionSystem cn, int limit = int.MaxValue)
        {
            EliteNameClassifier ec = new EliteNameClassifier(name);

            List<ISystem> ret = new List<ISystem>();

           // System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW",true)} Find {name}");

            if (ec.IsStandardParts)     // normal Euk PRoc qc-l d2-3
            {
                // needs index on Systems(sectorid, Nameid)
                //Select ID 6 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)
                //Select ID 10 Order 0 From 0: LIST SUBQUERY 1
                //Select ID 12 Order 10 From 0: SEARCH c USING COVERING INDEX SectorName(name=?)
                //Select ID 28 Order 0 From 0: SEARCH s USING INDEX SystemsSectorName(sectorid =? AND nameid >? AND nameid <?)
                //Select ID 40 Order 0 From 0: REUSE LIST SUBQUERY 1
                //Select ID 47 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN

                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                    "s.nameid >= @p1 AND s.nameid <= @p2 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p3)",
                                                    new Object[] { ec.ID, ec.IDHigh, ec.SectorName },
                                                    limit: limit,
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
                // needs index on Systems(sectorid, Nameid) just using sectorid
                //Select ID 6 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)
                //Select ID 10 Order 0 From 0: LIST SUBQUERY 1
                //Select ID 12 Order 10 From 0: SEARCH c USING COVERING INDEX SectorName(name=?)
                //Select ID 28 Order 0 From 0: SEARCH s USING INDEX SystemsSectorName(sectorid =?)
                //Select ID 42 Order 0 From 0: REUSE LIST SUBQUERY 1
                //Select ID 49 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN

                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                    "(s.nameid & (1<<46) != 0) AND cast((s.nameid & 0x3fffffffff) as text) LIKE @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { ec.NameIdNumeric.ToStringInvariant() + "%", ec.SectorName },
                                                    limit: limit,
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
                            //System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW")} Sector name given search {name}");

                            // needs index on Systems(sectorid, Nameid)
                            //Select ID 6 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)
                            //Select ID 10 Order 0 From 0: LIST SUBQUERY 2
                            //Select ID 12 Order 10 From 0: SEARCH c USING COVERING INDEX SectorName(name=?)
                            //Select ID 28 Order 0 From 0: SEARCH s USING INDEX SystemsSectorName(sectorid =? AND nameid =?)
                            //Select ID 33 Order 0 From 0: LIST SUBQUERY 1
                            //Select ID 36 Order 33 From 0: SEARCH Names USING COVERING INDEX NamesName(Name>? AND Name <?)
                            //Select ID 62 Order 0 From 0: REUSE LIST SUBQUERY 2
                            //Select ID 69 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN

                            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
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
                            //Select ID 6 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)
                            //Select ID 10 Order 0 From 0: LIST SUBQUERY 1
                            //Select ID 13 Order 10 From 0: SEARCH c USING COVERING INDEX SectorName(name>? AND name <?)
                            //Select ID 34 Order 0 From 0: SEARCH s USING INDEX SystemsSectorName(sectorid =?)
                            //Select ID 40 Order 0 From 0: REUSE LIST SUBQUERY 1
                            //Select ID 47 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN

                            //System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW")} Sector no-name search {name}");

                            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
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
                        //System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW")} Name search {name}");

                        using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                            "s.edsmid IN (Select id FROM Names WHERE name LIKE @p1) ",      // BUG here 4/10/23 should be edsmid so it uses primary index!
                                                            new Object[] { ec.StarName + "%" },
                                                            limit: limit,
                                                            joinlist: MakeSystemQueryNamedJoinList))
                        {
                            //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

                            //Select ID 5 Order 0 From 0: SEARCH s USING INTEGER PRIMARY KEY(rowid=?)
                            //Select ID 9 Order 0 From 0: LIST SUBQUERY 1
                            //Select ID 12 Order 9 From 0: SEARCH Names USING COVERING INDEX NamesName(Name>? AND Name <?)
                            //Select ID 33 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN
                            //Select ID 38 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)

                            using (DbDataReader reader = selectSysCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    SystemClass sc = MakeSystem(reader);
                                    ret.Add(sc);
                                }

                                //System.Diagnostics.Debug.WriteLine($"**************** {BaseUtils.AppTicks.TickCountLap("SS1")}Search-NoSector-name, check names {ret.Count}");

                                limit -= ret.Count;
                            }
                        }

                        if (limit > 0)
                        {
                            //System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW")} Sector search {name}");

                            //Select ID 6 Order 0 From 0: SEARCH c USING INTEGER PRIMARY KEY(rowid=?)
                            //Select ID 10 Order 0 From 0: LIST SUBQUERY 1
                            //Select ID 13 Order 10 From 0: SEARCH c USING COVERING INDEX SectorName(name>? AND name <?)
                            //Select ID 34 Order 0 From 0: SEARCH s USING INDEX SystemsSectorName(sectorid =?)
                            //Select ID 40 Order 0 From 0: REUSE LIST SUBQUERY 1
                            //Select ID 47 Order 0 From 0: SEARCH n USING INTEGER PRIMARY KEY(rowid=?) LEFT - JOIN

                            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
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

           // System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.TickCountLap("FSW")} Finish {name}");
            return ret;
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


