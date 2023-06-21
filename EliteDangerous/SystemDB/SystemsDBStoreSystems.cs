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

namespace EliteDangerousCore.DB
{
    // EDSM Store - db.edsmid contains edsmid, and db.info is null
    // Spansh Store - db.edsmid contains the system address, and db.info is non null
    // tbd - what if the data we are replacing in spansh mode

    public partial class SystemsDB
    {
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

                    jlist.Add(jo);
                }
            }

            if (jlist.Count > 0)
            {
                SystemsDB.Loader3 loader3 = new SystemsDB.Loader3("", 10000, null, poverlapped:false, pdontoverwrite:true);
                long updates = loader3.ParseJSONFile(jlist.ToString(), () => false, (s) => System.Diagnostics.Debug.WriteLine($"Store Systems: {s}"));
                loader3.Finish();
                return updates;

            }

            return 0;
        }
    }
}



