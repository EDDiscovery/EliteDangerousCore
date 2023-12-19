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
        public static long GetTotalSystems()            // this is SLOW try not to use
        {
            if (SystemsDatabase.Instance.RebuildRunning)
                return 0;

            return SystemsDatabase.Instance.DBRead(db =>
            {
                var cn = db;
                using (DbCommand cmd = cn.CreateCommand("select Count(1) from SystemTable"))
                {
                    return (long)cmd.ExecuteScalar();
                }
            });
        }

        // Beware with no extra conditions, you get them all..  Mostly used for debugging
        // use starreport to avoid storing the entries instead pass back one by one
        public static List<ISystem> ListStars(string where = null, string orderby = null, string limit = null,
                                                Action<ISystem> starreport = null)
        {
            List<ISystem> ret = new List<ISystem>();

            if (SystemsDatabase.Instance.RebuildRunning)
                return ret;

            return SystemsDatabase.Instance.DBRead(db =>
            {

                //BaseUtils.AppTicks.TickCountLap("Star");

                var cn = db;

                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed, where, orderby, limit: limit, joinlist: MakeSystemQueryNamedJoinList))
                {
                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        while (reader.Read())      // if there..
                        {
                            SystemClass s = MakeSystem(reader);
                            if (starreport != null)
                                starreport(s);
                            else
                                ret.Add(s);
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine("Find stars " + BaseUtils.AppTicks.TickCountLap("Star"));
                return ret;
            });
        }

        // randimised id % 100 < sercentage
        public static List<V> GetStarPositions<V>(int percentage, Func<int, int, int, V> tovect)  // return all star positions..
        {
            List<V> ret = new List<V>();

            if (SystemsDatabase.Instance.RebuildRunning)
                return ret;

            return SystemsDatabase.Instance.DBRead(db =>
            {

                var cn = db;

                using (DbCommand cmd = cn.CreateSelect("SystemTable s",
                                                       outparas: "s.x,s.y,s.z",
                                                       where: "((s.edsmid*2333)%100) <" + percentage.ToStringInvariant()
                                                       ))
                {
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret.Add(tovect(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
                        }
                    }
                }

                return ret;
            });
        }

        // function provides a V[] vector of positions and a text list of names in a given area, lower left bottom defined by x/y/z
        // it returns a too long vector, for speed reasons
        // may return zero entries with empty arrays if nothing is present
        // may return zero/null if system DB is being built
        // tovect is used to transform x,y,z,star type to a V type
        public static int GetSystemList<V>(float x, float y, float z, float blocksize, ref string[] names, ref V[] vectors, Func<int, int, int, EDStar, V> tovect,
                                            Func<V, string, string> additionaltext, int chunksize = 10000)
        {
            string[] namesout = null;
            V[] vectsout = null;
            int fill = 0;

            if (!SystemsDatabase.Instance.RebuildRunning) // use the cache is db is updating
            {
                SystemsDatabase.Instance.DBRead(db =>
                {
                    fill = GetSystemList<V>(db, x, y, z, blocksize, ref namesout, ref vectsout, tovect, additionaltext, chunksize);
                }, warnthreshold: 5000);
            }

            names = namesout;
            vectors = vectsout;
            return fill;
        }


        public static int GetSystemList<V>(SQLiteConnectionSystem cn, float x, float y, float z, float blocksize, ref string[] names, ref V[] vectors,
                                                Func<int, int, int, EDStar, V> tovect,
                                                Func<V, string, string> additionaltext, int chunksize)
        {
            names = new string[chunksize];
            vectors = new V[chunksize];
            int fillpos = 0;

            using (DbCommand cmd = cn.CreateSelect("SystemTable s",
                                                    outparas: "s.x, s.y, s.z, c.name, s.nameid, n.Name, s.info",
                                                    where: "s.x>=@p1 AND s.x<@p2 AND s.y>=@p3 AND s.y<@p4 AND s.z>=@p5 AND s.z<@p6",
                                                    paras: new Object[] {   SystemClass.DoubleToInt(x), SystemClass.DoubleToInt(x+blocksize),
                                                                            SystemClass.DoubleToInt(y), SystemClass.DoubleToInt(y+blocksize),
                                                                            SystemClass.DoubleToInt(z),SystemClass.DoubleToInt(z+blocksize) },
                                                    joinlist: MakeSystemQueryNamedJoinList
                                                    ))
            {
                //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(cmd));

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    //System.Diagnostics.Debug.WriteLine("sysLapStart : " + BaseUtils.AppTicks.TickCountLap());

                    Object[] data = new Object[4];

                    while (reader.Read())
                    {
                        if (fillpos == names.Length)    // if reached limit, increase
                        {
                            chunksize *= 2;             // increase chunksize each time
                            Array.Resize(ref names, names.Length + chunksize);
                            Array.Resize(ref vectors, vectors.Length + chunksize);
                        }

                        int sx = reader.GetInt32(0);
                        int sy = reader.GetInt32(1);
                        int sz = reader.GetInt32(2);
                        EDStar startype = reader.IsDBNull(6) ? EDStar.Unknown : (EDStar)reader.GetInt32(6);

                        vectors[fillpos] = tovect(sx, sy, sz, startype);

                        EliteNameClassifier ec = new EliteNameClassifier((ulong)reader.GetInt64(4));
                        ec.SectorName = reader.GetString(3);

                        if (ec.IsNamed)
                            ec.StarName = reader.GetString(5);
                        string name = ec.ToString();
                        if (additionaltext != null)
                            names[fillpos] = additionaltext(vectors[fillpos], name);
                        else
                            names[fillpos] = name;
                        fillpos++;

                    }
                }
            }

            return fillpos;
        }

    }
}


