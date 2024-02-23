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
using System.Data.Common;

namespace EliteDangerousCore.DB
{
    // EDSM Store - db.edsmid contains edsmid, and db.info is null
    // Spansh Store - db.edsmid contains the system address, and db.info is non null

    public partial class SystemsDB
    {
        // store systems to DB.  Data is checked against the mode. Verified april 23 with a load against an empty db in spansh and edsm mode
        public static long StoreSystems(IEnumerable<ISystem> systems)
        {
            JArray jlist = new JArray();

            string currentdb = SystemsDatabase.Instance.DBSource;
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

                    if (spansh)       // we format either for a spansh DB or an EDSM db
                    {
                        jo["id64"] = sys.SystemAddress.Value;
                        jo["updateTime"] = DateTime.UtcNow;
                    }
                    else
                    {
                        jo["id"] = sys.EDSMID.Value;
                        jo["date"] = DateTime.UtcNow;
                    }

                    System.Diagnostics.Debug.WriteLine($"DB Store systems {jo.ToString()}");
                    jlist.Add(jo);
                }
            }

            if (jlist.Count > 0)
            {
                // start loader, 10000 at a time, no overlapped so we don't load up the pc, and don't overwrite stuff already there

                var cancel = new System.Threading.CancellationToken(); //can't be cancelled
                SystemsDB.Loader3 loader3 = new SystemsDB.Loader3("", 10000, null, poverlapped: false, pdontoverwrite: true);
                long updates = loader3.ParseJSONString(jlist.ToString(), cancel, (s) => System.Diagnostics.Debug.WriteLine($"Store Systems: {s}"));
                loader3.Finish(cancel);
                return updates;

            }

            return 0;
        }


        #region debug

        internal static void ExplainPlans(SQLiteConnectionSystem cn, int limit = int.MaxValue)
        {
            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.nameid >= @p1 AND s.nameid <= @p2 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p3)",
                                                new Object[] { 1, 2, "name" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                    "(s.nameid & (1<<46) != 0) AND cast((s.nameid & 0x3fffffffff) as text) LIKE @p1 AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                    new Object[] { 10.ToStringInvariant() + "%", "name" },
                                                    limit: limit,
                                                    joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                 "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) AND s.sectorid IN (Select id FROM Sectors c WHERE c.name=@p2)",
                                                 new Object[] { "Sol" + "%", 12 },
                                                 limit: limit,
                                                 joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                  "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                  new Object[] { "wkwk" + "%" },
                                                  limit: limit,
                                                  joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }


            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.nameid IN (Select id FROM Names WHERE name LIKE @p1) ",
                                                new Object[] { "kwk" + "%" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                 "s.sectorid IN (Select id FROM Sectors c WHERE c.name LIKE @p1)",
                                                 new Object[] { "Sol" + "%" },
                                                 limit: limit,
                                                 joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));
            }

            System.Diagnostics.Debug.Write("NEW!");

            using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed,
                                                "s.edsmid IN (Select id FROM Names WHERE name LIKE @p1)",
                                                new Object[] { "kwk" + "%" },
                                                limit: limit,
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                System.Diagnostics.Debug.WriteLine(cn.ExplainQueryPlanString(selectSysCmd));

            }

        }

        #endregion

    }
}



