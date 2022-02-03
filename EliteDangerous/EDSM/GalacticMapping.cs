/*
 * Copyright © 2016-2021 EDDiscovery development team
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

using EliteDangerousCore.DB;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseUtils;

namespace EliteDangerousCore.EDSM
{
    public class GalacticMapping
    {
        public List<GalacticMapObject> GalacticMapObjects = null;
        public GalacticMapObject[] VisibleMapObjects { get { return GalacticMapObjects.Where(x => x.GalMapType.VisibleType != null).ToArray(); } }

        public bool Loaded { get { return GalacticMapObjects.Count > 0; } }

        public GalacticMapping()
        {
            GalacticMapObjects = new List<GalacticMapObject>();
        }

        public bool LoadMarxObjects()
        {
            using (StringReader t = new StringReader(EliteDangerous.Properties.Resources.Marx_Nebula_List_26_10_21))
            {
                CSVFile csv = new CSVFile();

                return csv.Read(t, true, (r, rw) => {
                    var pos = new EMK.LightGeometry.Vector3((float)(rw[2].InvariantParseDoubleNull() ?? 0),
                                                        (float)(rw[3].InvariantParseDoubleNull() ?? 0),
                                                        (float)(rw[4].InvariantParseDoubleNull() ?? 0));
                    if (pos.X != 0 && pos.Y != 0 && pos.Z != 0)
                    {
                        var gmo = new GalacticMapObject("MarxNebula", rw[0] + " Nebula", "Marx sourced nebula", pos);
                        GalacticMapObjects.Add(gmo);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Reject Marx {rw[0]}");
                    }
                });
            }
        }

        public bool ParseEDSMFile(string file)
        {
            try
            {
                string json = File.ReadAllText(file);
                return ParseEDSMJson(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("GalacticMapping.parsedata exception:" + ex.Message);
            }

            return false;
        }

        public bool ParseEDSMJson(string json)
        {
            try
            {
                if (json.HasChars())
                {
                    JArray galobjects = JArray.ParseThrowCommaEOL(json);

                    foreach (JObject jo in galobjects)
                    {
                        GalacticMapObject galobject = new GalacticMapObject(jo);
                        GalacticMapObjects.Add(galobject);

                        if (galobject.Points.Count == 1 && galobject.GalMapSearch != null && galobject.GalMapUrl != null)
                        {
                            var gms = new GalacticMapSystem(galobject);
                            SystemCache.AddSystemToCache(gms);
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("GalacticMapping.parsedata exception:" + ex.Message);
            }

            return false;
        }

        public GalacticMapObject Find(string name, bool contains = false)
        {
            if (GalacticMapObjects != null && name.HasChars())
            {
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    if (gmo.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || (contains && gmo.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    {
                        return gmo;
                    }
                }
            }

            return null;
        }

        public GalacticMapObject FindNearest(double x, double y, double z)
        {
            GalacticMapObject nearest = null;

            if (GalacticMapObjects != null)
            {
                double mindist = double.MaxValue;
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    if ( gmo.Points.Count == 1 )        // only for single point  bits
                    {
                        double distsq = (gmo.Points[0].X - x) * (gmo.Points[0].X - x) + (gmo.Points[0].Y - y) * (gmo.Points[0].Y - y) + (gmo.Points[0].Z - z) * (gmo.Points[0].Z - z);
                        if ( distsq < mindist)
                        {
                            mindist = distsq;
                            nearest = gmo;
                        }
                    }
                }
            }

            return nearest;
        }

        public List<string> GetGMONames()
        {
            List<string> ret = new List<string>();

            if (GalacticMapObjects != null)
            {
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    ret.Add(gmo.Name);
                }
            }

            return ret;
        }
            
    }
}
