/*
 * Copyright 2015-2021 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Data.Common;
using System.Drawing;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        // protect above from SystemsDatabase.Instance.RebuildRunning

        public static void GetSystemVector<V>(int gridid, ref V[] vertices1, ref uint[] colours1,
                                                          int percentage, Func<int, int, int, V> tovect)
        {
            V[] cpvertices1 = vertices1;
            uint[] cpcolours1 = colours1;

            SystemsDatabase.Instance.DBRead(db =>
            {
                GetSystemVector<V>(gridid, ref cpvertices1, ref cpcolours1, percentage, tovect, db);
            },warnthreshold:5000);

            vertices1 = cpvertices1;
            colours1 = cpcolours1;
        }

        private static void GetSystemVector<V>(int gridid, ref V[] vertices1, ref uint[] colours1,
                                                          int percentage, Func<int, int, int, V> tovect,
                                                          SQLiteConnectionSystem cn)
                                                          
        {
            int numvertices1 = 0;
            vertices1 = null;
            colours1 = null;

            Color[] fixedc = new Color[4];
            fixedc[0] = Color.Red;
            fixedc[1] = Color.Orange;
            fixedc[2] = Color.Yellow;
            fixedc[3] = Color.White;

            //System.Diagnostics.Debug.WriteLine("sysLap : " + BaseUtils.AppTicks.TickCountLap());

            // tried xz comparision but slower than grid select
            using (DbCommand cmd = cn.CreateSelect("Systems s",
                                                    outparas: "s.edsmid,s.x,s.y,s.z" ,
                                                    where: "s.sectorid IN (Select id FROM Sectors c WHERE c.gridid = @p1)" +
                                                            (percentage < 100 ? (" AND ((s.edsmid*2333)%100) <" + percentage.ToStringInvariant()) : ""),
                                                    paras: new Object[] { gridid }
                                                    ))
            {
                //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(cmd));

                vertices1 = new V[250000];
                colours1 = new uint[250000];

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    //System.Diagnostics.Debug.WriteLine("sysLapStart : " + BaseUtils.AppTicks.TickCountLap());

                    Object[] data = new Object[4];

                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);       // quicker than cast
                        int x = reader.GetInt32(1);
                        int y = reader.GetInt32(2);
                        int z = reader.GetInt32(3);

                        Color basec = fixedc[(id) & 3];
                        int fade = 100 - (((int)id >> 2) & 7) * 8;
                        byte red = (byte)(basec.R * fade / 100);
                        byte green = (byte)(basec.G * fade / 100);
                        byte blue = (byte)(basec.B * fade / 100);

                        if (numvertices1 == vertices1.Length)
                        {
                            Array.Resize(ref vertices1, vertices1.Length *2);
                            Array.Resize(ref colours1, colours1.Length *2);
                        }

                        colours1[numvertices1] = BitConverter.ToUInt32(new byte[] { red, green, blue, 255 }, 0);
                        vertices1[numvertices1++] = tovect(x, y, z);
                    }

              //      System.Diagnostics.Debug.WriteLine("sysLapEnd : " + BaseUtils.AppTicks.TickCountLap());
                }

                Array.Resize(ref vertices1, numvertices1);
                Array.Resize(ref colours1, numvertices1);

                if (gridid == GridId.SolGrid && vertices1 != null)    // BODGE do here, better once on here than every star for every grid..
                {                       // replace when we have a better naming system
                    int solindex = Array.IndexOf(vertices1, tovect(0, 0, 0));
                    if (solindex >= 0)
                        colours1[solindex] = 0x00ffff;   //yellow
                }
            }
        }
    }
}


