/*
 * Copyright 2023 - 2023 EDDiscovery development team
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

using System.Collections.Generic;
using System.Data.Common;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        // return hashset of edsmid/system id (dep on DB) of permit systems
        static public HashSet<long> GetPermitSystems()
        {
            HashSet<long> ret = new HashSet<long>();
            SystemsDatabase.Instance.DBRead(db =>
            {
                using (var selectSectorCmd = db.CreateSelect("PermitSystems", "edsmid"))
                {
                    using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // Read all sectors into the cache
                    {
                        while (reader.Read())
                        {
                            ret.Add((long)reader[0]);
                        }
                    };
                };
            });

            return ret;
        }

        static public List<ISystem> GetListPermitSystems()
        {
            List<ISystem> ret = new List<ISystem>();
            SystemsDatabase.Instance.DBRead(cn =>
            {
                using (DbCommand selectSysCmd = cn.CreateSelect("SystemTable s", MakeSystemQueryNamed, "s.edsmid IN (Select edsmid From PermitSystems)", joinlist: MakeSystemQueryNamedJoinList))
                {
                    using (DbDataReader reader = selectSysCmd.ExecuteReader())
                    {
                        while (reader.Read())      // if there..
                        {
                            SystemClass s = MakeSystem(reader);
                            ret.Add(s);
                        }
                    }
                }
            });

            return ret;
        }
    }
}


