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
using System.Linq;
using System.Data.Common;
using EMK.LightGeometry;
using System.Collections.Generic;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        // search the DB for stars, at x/y/z with min/max distances.
        // callback as found
        internal static void GetSystemListBySqDistancesFrom(double x, double y, double z,
                                                            int maxitems,
                                                            double mindist,         // 0 = no min dist, always spherical
                                                            double maxdist,
                                                            bool spherical,     // enforces sphere on maxdist, else its a cube for maxdist
                                                            SQLiteConnectionSystem cn,
                                                            Action<double, ISystem> callback
                                                        )

        {
            // System.Diagnostics.Debug.WriteLine("Time1 " + BaseUtils.AppTicks.TickCountLap("SDC"));

            int mindistint = mindist > 0 ? SystemClass.DoubleToInt(mindist) * SystemClass.DoubleToInt(mindist) : 0;

            // needs a xz index for speed

            using (DbCommand cmd = cn.CreateSelect("Systems s",
                MakeSystemQueryNamed,
                where:
                    "s.x >= @xv - @maxdist " +
                    "AND s.x <= @xv + @maxdist " +
                    "AND s.z >= @zv - @maxdist " +
                    "AND s.z <= @zv + @maxdist " +
                    "AND s.y >= @yv - @maxdist " +
                    "AND s.y <= @yv + @maxdist " +
                    (mindist > 0 ? ("AND (s.x-@xv)*(s.x-@xv)+(s.y-@yv)*(s.y-@yv)+(s.z-@zv)*(s.z-@zv)>=" + (mindistint).ToStringInvariant()) : ""),
                orderby: "(s.x-@xv)*(s.x-@xv)+(s.y-@yv)*(s.y-@yv)+(s.z-@zv)*(s.z-@zv)",         // just use squares to order
                joinlist: MakeSystemQueryNamedJoinList,
                limit: "@max"
                ))
            {
                cmd.AddParameterWithValue("@xv", SystemClass.DoubleToInt(x));
                cmd.AddParameterWithValue("@yv", SystemClass.DoubleToInt(y));
                cmd.AddParameterWithValue("@zv", SystemClass.DoubleToInt(z));
                cmd.AddParameterWithValue("@max", maxitems + 1);     // 1 more, because if we are on a System, that will be returned
                cmd.AddParameterWithValue("@maxdist", SystemClass.DoubleToInt(maxdist));

               // System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(cmd));

                int xi = SystemClass.DoubleToInt(x);
                int yi = SystemClass.DoubleToInt(y);
                int zi = SystemClass.DoubleToInt(z);
                long maxdistsqi = (long)SystemClass.DoubleToInt(maxdist) * (long)SystemClass.DoubleToInt(maxdist);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                  //  System.Diagnostics.Debug.WriteLine("Time1.5 " + BaseUtils.AppTicks.TickCountLap("SDC"));

                    while (reader.Read())      // already sorted, and already limited to max items
                    {
                        int sxi = reader.GetInt32(0);
                        int syi = reader.GetInt32(1);
                        int szi = reader.GetInt32(2);

                        long distsqi = (long)(xi - sxi) * (long)(xi - sxi) + (long)(yi - syi) * (long)(yi - syi) + (long)(zi - szi) * (long)(zi - szi);

                        if (!spherical || distsqi <= maxdistsqi)
                        {
                            SystemClass s = MakeSystem(reader);
                            double distnorm = ((double)distsqi) / SystemClass.XYZScalar / SystemClass.XYZScalar;
                            callback(distnorm, s);
                        }
                    }

                  //  System.Diagnostics.Debug.WriteLine("Time2 " + BaseUtils.AppTicks.TickCountLap("SDC") + "  count " + count);
                }
            }
        }

        // get system near x/y/z from DB only
        internal static ISystem GetSystemByPosition(double x, double y, double z, SQLiteConnectionSystem cn, double maxdist = 0.125)
        {
            BaseUtils.SortedListDoubleDuplicate<ISystem> distlist = new BaseUtils.SortedListDoubleDuplicate<ISystem>();
            GetSystemListBySqDistancesFrom(x, y, z, 1, 0, maxdist, true, cn, (d,s)=> { distlist.Add(d, s); }); // return 1 item, min dist 0, maxdist
            return (distlist.Count > 0) ? distlist.First().Value : null;
        }

        /////////////////////////////////////////////// Nearest to a point determined by a metric

        // either use CallBack or List
        internal static void GetSystemNearestTo(
                                                  Point3D currentpos,
                                                  Point3D wantedpos,
                                                  double maxfromcurpos,
                                                  double maxfromwanted,
                                                  int limitto,
                                                  SQLiteConnectionSystem cn,
                                                  Action<ISystem> CallBack = null,
                                                  List<ISystem> list = null)
        {
            System.Diagnostics.Debug.Assert(CallBack != null || list != null);

            using (DbCommand cmd = cn.CreateSelect("Systems s",
                        MakeSystemQueryNamed,
                        where:
                                "x >= @xc - @maxfromcurpos " +
                                "AND x <= @xc + @maxfromcurpos " +
                                "AND z >= @zc - @maxfromcurpos " +
                                "AND z <= @zc + @maxfromcurpos " +
                                "AND x >= @xw - @maxfromwanted " +
                                "AND x <= @xw + @maxfromwanted " +
                                "AND z >= @zw - @maxfromwanted " +
                                "AND z <= @zw + @maxfromwanted " +
                                "AND y >= @yc - @maxfromcurpos " +
                                "AND y <= @yc + @maxfromcurpos " +
                                "AND y >= @yw - @maxfromwanted " +
                                "AND y <= @yw + @maxfromwanted ",
                        orderby: "(s.x-@xw)*(s.x-@xw)+(s.y-@yw)*(s.y-@yw)+(s.z-@zw)*(s.z-@zw)",         // orderby distance from wanted
                        limit: limitto,
                        joinlist: MakeSystemQueryNamedJoinList))
            {
                cmd.AddParameterWithValue("@xw", SystemClass.DoubleToInt(wantedpos.X));         // easier to manage with named paras
                cmd.AddParameterWithValue("@yw", SystemClass.DoubleToInt(wantedpos.Y));
                cmd.AddParameterWithValue("@zw", SystemClass.DoubleToInt(wantedpos.Z));
                cmd.AddParameterWithValue("@maxfromwanted", SystemClass.DoubleToInt(maxfromwanted));
                cmd.AddParameterWithValue("@xc", SystemClass.DoubleToInt(currentpos.X));
                cmd.AddParameterWithValue("@yc", SystemClass.DoubleToInt(currentpos.Y));
                cmd.AddParameterWithValue("@zc", SystemClass.DoubleToInt(currentpos.Z));
                cmd.AddParameterWithValue("@maxfromcurpos", SystemClass.DoubleToInt(maxfromcurpos));

                //System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(cmd));

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sys = MakeSystem(reader);
                        if (CallBack!=null)
                            CallBack(sys);
                        else
                            list.Add(sys);
                    }
                }
            }
        }

    }
}


