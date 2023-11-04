/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using EliteDangerousCore.DB;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseUtils;

namespace EliteDangerousCore.GMO
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

                return csv.Read(t, (r, rw) => {
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

        public bool ParseGMPFile(string file, int idoffset)
        {
            try
            {
                string json = File.ReadAllText(file);
                return ParseGMPJson(json,idoffset);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("GalacticMapping.parsedata exception:" + ex.Message);
            }

            return false;
        }

        public bool ParseGMPJson(string json, int idoffset)
        {
            try
            {
                if (json.HasChars())
                {
                    JArray galobjects = JArray.ParseThrowCommaEOL(json);

                    foreach (JObject jo in galobjects)
                    {
                        GalacticMapObject newgmo = new GalacticMapObject(jo,idoffset);

                        if (newgmo.Points.Count > 0) // need some sort of position
                        {
                            var previousstored = GalacticMapObjects.Find(x => Math.Abs(x.Points[0].X - newgmo.Points[0].X) < 0.25f &&
                                                                         Math.Abs(x.Points[0].Y - newgmo.Points[0].Y) < 0.25f &&
                                                                         Math.Abs(x.Points[0].Z - newgmo.Points[0].Z) < 0.25f);
                            
                            if (newgmo.Names[0] == "Great Annihilator Black Hole")  // manually remove
                            {
                                continue;
                            }

                            if (previousstored != null && newgmo.Points.Count == 1)
                            {
                                if ((newgmo.Names[0] == "Great Annihilator" || newgmo.Names[0] == "Galactic Centre")) // these take precedence
                                {
                                    string gmodesc = Environment.NewLine + "+++ " + previousstored.Names[0] + Environment.NewLine + previousstored.Description;
                                    newgmo.AddDuplicateGMODescription(previousstored.Names[0],gmodesc);
                                    GalacticMapObjects.Remove(previousstored);
                                    GalacticMapObjects.Add(newgmo);
                                  //  System.Diagnostics.Debug.WriteLine($"GMO Priority name store {newgmo.NameList} removing previous {previousstored.NameList}");
                                }
                                else if ( !previousstored.NameList.Contains(newgmo.Names[0],StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string gmodesc = Environment.NewLine + "+++ " + newgmo.Names[0] + Environment.NewLine + newgmo.Description;
                                    previousstored.AddDuplicateGMODescription(newgmo.Names[0], gmodesc);
                               //     SystemCache.AddSystemToCache(newgmo.GetSystem());        // also add this name
                                   // System.Diagnostics.Debug.WriteLine($"GMO Merge name {newgmo.NameList} with previous {previousstored.NameList}");
                                }
                                else
                                {
                                   // System.Diagnostics.Debug.WriteLine($"GMO Duplicate name {newgmo.NameList}");
                                }
                            }
                            else
                            {
                               // System.Diagnostics.Debug.WriteLine($"GMO Add {newgmo.NameList} {newgmo.Type}");
                                GalacticMapObjects.Add(newgmo);
                             //   SystemCache.AddSystemToCache(newgmo.GetSystem());        // also add this name
                            }
                        }
                        else
                        {

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
                    if (gmo.IsName(name,contains))
                        return gmo;
                }
            }

            return null;
        }

        public GalacticMapObject FindNearest(double x, double y, double z, double maxdist)
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
                        if ( distsq <= maxdist*maxdist && distsq < mindist)
                        {
                            mindist = distsq;
                            nearest = gmo;
                        }
                    }
                }
            }

            return nearest;
        }

        public List<string> GetGMPNames()
        {
            List<string> ret = new List<string>();

            if (GalacticMapObjects != null)
            {
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    foreach (var gmoname in gmo.Names)
                        ret.Add(gmoname);
                }
            }

            return ret;
        }
    }
}
