/*
 * Copyright © 2016-2024 EDDiscovery development team
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
using System.IO;
using System.Linq;
using BaseUtils;

namespace EliteDangerousCore.GMO
{
    public class GalacticMapping
    {
        public List<GalacticMapObject> GalacticMapObjects = null;

        // these need have have a Gal map type, a VisibleType in entry 0 AND have an associated star system
        public GalacticMapObject[] VisibleMapObjects { get { return GalacticMapObjects.Where(x => x.GalMapTypes[0] != null && x.GalMapTypes[0].VisibleType != null && x.StarSystem != null).ToArray(); } }
        public GalacticMapObject[] AllObjects { get { return GalacticMapObjects.ToArray(); } }

        public bool Loaded { get { return GalacticMapObjects.Count > 0; } }

        public GalacticMapping()
        {
            GalacticMapObjects = new List<GalacticMapObject>();
        }

        // expects a headerless CSV file (comma) with A = name, B = any descriptive text, C/D/E = x/y/z
        // nameposfix text is added to name
        // defaultdescription is used if B is empty
        public bool LoadCSV(string csvfile, string type, string namepostfix, string defaultdescription)
        {
            return LoadCSV(new StringReader(csvfile), type, namepostfix, defaultdescription);
        }

        public bool LoadCSV(StringReader t, string type, string namepostfix, string defaultdescription)
        { 
            CSVFile csv = new CSVFile();

            return csv.Read(t, (r, rw) => {

                var pos = new EMK.LightGeometry.Vector3((float)(rw[2].InvariantParseDouble(-999999)),
                                                    (float)(rw[3].InvariantParseDouble(-999999)),
                                                    (float)(rw[4].InvariantParseDouble(-999999)));

                if (pos.X != -999999 && pos.Y != -999999 && pos.Z != -999999)
                {
                    var previousstored = GalacticMapObjects.Find(x => Math.Abs(x.Points[0].X - pos.X) < 0.25f &&
                                                                    Math.Abs(x.Points[0].Y - pos.Y) < 0.25f &&
                                                                    Math.Abs(x.Points[0].Z - pos.Z) < 0.25f);

                    string nameofobject = rw[0] + namepostfix;
                    string descriptivetext = rw[1].Length > 0 ? rw[1] : defaultdescription;
                    descriptivetext = descriptivetext.WordWrap(80);

                    if (previousstored != null)
                    {
                        previousstored.AddDuplicate1(type, nameofobject,Environment.NewLine+descriptivetext);
                    }
                    else
                    {
                        var gmo = new GalacticMapObject(type, nameofobject, rw[0], descriptivetext, pos);
                        GalacticMapObjects.Add(gmo);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Load CSV GMO objects reject due to bad co-ords  {rw[0]}");
                }
            });
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
                            if (newgmo.DescriptiveNames[0] == "Great Annihilator Black Hole")  // manually remove
                            {
                                continue;
                            }

                            var previousstored = GalacticMapObjects.Find(x => Math.Abs(x.Points[0].X - newgmo.Points[0].X) < 0.25f &&
                                                                         Math.Abs(x.Points[0].Y - newgmo.Points[0].Y) < 0.25f &&
                                                                         Math.Abs(x.Points[0].Z - newgmo.Points[0].Z) < 0.25f);


                            if (previousstored != null && newgmo.Points.Count == 1)
                            {
                                if ((newgmo.DescriptiveNames[0] == "Great Annihilator" || newgmo.DescriptiveNames[0] == "Galactic Centre")) // these take precedence
                                {
                                    string gmodesc = Environment.NewLine + "+++ " + previousstored.DescriptiveNames[0] + Environment.NewLine + previousstored.Description;
                                    newgmo.AddDuplicate1(previousstored.GalMapTypes[0].TypeName, previousstored.DescriptiveNames[0],gmodesc);
                                    GalacticMapObjects.Remove(previousstored);
                                    GalacticMapObjects.Add(newgmo);
                                  //  System.Diagnostics.Debug.WriteLine($"GMO Priority name store {newgmo.NameList} removing previous {previousstored.NameList}");
                                }
                                else if ( !previousstored.NameList.Contains(newgmo.DescriptiveNames[0],StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string gmodesc = Environment.NewLine + "+++ " + newgmo.DescriptiveNames[0] + Environment.NewLine + newgmo.Description;
                                    previousstored.AddDuplicate1(newgmo.GalMapTypes[0].TypeName, newgmo.DescriptiveNames[0], gmodesc);
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

        // find descriptive name, first entry
        // wildcard = true, contains ignored
        // wildcard = false, either exact match or if contains = true then part of it
        public GalacticMapObject FindDescriptiveName(string descripivename, bool wildcard, bool contains)
        {
            var list = FindDescriptiveNames(descripivename, wildcard, contains);
            return list.Count > 0 ? list[0] : null;
        }

        // find list of descriptive names matching
        public List<GalacticMapObject> FindDescriptiveNames(string descripivename, bool wildcard, bool contains)
        {
            List<GalacticMapObject> gmos = new List<GalacticMapObject>();
            if (GalacticMapObjects != null && descripivename.HasChars())
            {
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    if (gmo.IsDescriptiveName(descripivename, wildcard, contains))
                        gmos.Add(gmo);
                }
            }

            return gmos;
        }

        // find star system exact match, first entry
        public GalacticMapObject FindSystem(string name)
        {
            var list = FindSystems(name);
            return list.Count > 0 ? list[0] : null;
        }

        // find all objects with system name set to this - since we now merge systems, should be only 1.
        public List<GalacticMapObject> FindSystems(string name)
        {
            List<GalacticMapObject> gmos = new List<GalacticMapObject>();
            if (GalacticMapObjects != null && name.HasChars())
            {
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    if (gmo.StarSystem?.Name.EqualsIIC(name) ?? false)      // if it has an associated star system, and its name matches, return
                        gmos.Add(gmo);
                }
            }

            return gmos;
        }

        public GalacticMapObject FindDescriptiveNameOrSystem(string descripivenameorsystem, bool wildcard = false)
        {
            var gmo = FindSystem(descripivenameorsystem);
            if (gmo == null)
                gmo = FindDescriptiveName(descripivenameorsystem, wildcard,false);
            return gmo;
        }

        public GalacticMapObject FindNearest(double x, double y, double z, double maxdist)
        {
            GalacticMapObject nearest = null;

            if (GalacticMapObjects != null)
            {
                double mindist = double.MaxValue;
                foreach (GalacticMapObject gmo in GalacticMapObjects)
                {
                    if (gmo.Points.Count == 1)        // only for single point  bits
                    {
                        double distsq = (gmo.Points[0].X - x) * (gmo.Points[0].X - x) + (gmo.Points[0].Y - y) * (gmo.Points[0].Y - y) + (gmo.Points[0].Z - z) * (gmo.Points[0].Z - z);
                        if (distsq <= maxdist * maxdist && distsq < mindist)
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
                    foreach (var gmoname in gmo.DescriptiveNames)
                        ret.Add(gmoname);
                }
            }

            return ret;
        }

        public void Dump()
        {
            foreach (GalacticMapObject gmo in GalacticMapObjects)
            {
                System.Diagnostics.Debug.WriteLine($"Name: {string.Join(",", gmo.DescriptiveNames)} : {gmo.GalMapTypes.Count} : {string.Join(",", gmo.GalMapTypes.Select(x => x.TypeName))} : {gmo.Points[0].ToString()}");
            }
        }
    }
}
