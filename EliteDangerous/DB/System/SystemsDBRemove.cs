﻿/*
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
using System.Data.Common;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        public static void RemoveGridSystems(int[] gridids, Action<string> report = null)
        {
            SystemsDatabase.Instance.DBWrite( db =>
            {
                var cn = db;
                int gridspergo = 4;

                report?.Invoke("Delete System Information from sector:");
                for (int i = 0; i < gridids.Length; i += gridspergo)
                {
                    int left = Math.Min(gridids.Length - i, gridspergo);     // could do it all at once, but this way, some visual feedback
                    int[] todo = new int[left];
                    Array.Copy(gridids, i, todo, 0, left);

                    report?.Invoke(" " + string.Join(" ", todo));

                    using (DbCommand cmd = cn.CreateDelete("SystemTable", "sectorid IN (Select id FROM Sectors WHERE gridid IN (" + string.Join(",", todo) + "))"))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    System.Threading.Thread.Sleep(200);

                    using (DbCommand cmd = cn.CreateDelete("Sectors", "gridid IN (" + string.Join(",", todo) + ")"))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    System.Threading.Thread.Sleep(200);
                }

                report?.Invoke(Environment.NewLine);
            });
        }

        public static void Vacuum()
        {
            SystemsDatabase.Instance.DBWrite(db =>
            {
                db.Vacuum();
            });
        }

        public static void Remove(long id)
        {
            SystemsDatabase.Instance.DBWrite(db =>
            {
                using (DbCommand cmd = db.CreateDelete("SystemTable", "edsmid=@p1", new string[] { "p1:int64" }))
                {
                    cmd.AddParameterWithValue("p1", id);
                    cmd.ExecuteNonQuery();
                }
            });
        }
    }
}


