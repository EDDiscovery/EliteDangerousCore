/*
 * Copyright 2016-2022 EDDiscovery development team
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
using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.DB
{
    [DebuggerDisplay("{Name} {Id} {Systems.Count}")]
    public class SavedRouteClass : IEquatable<SavedRouteClass>
    {
        public class SystemEntry
        {
            public string Name { get; set; }
            public string Note { get; set; }
            public SystemEntry(string sys, string note = "", double x = NotKnown, double y = NotKnown, double z = NotKnown) 
            { Name = sys; Note = note; X = x; Y = y; Z = z; }

            public const double NotKnown = -99999999;       // X = if not known
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public bool HasCoordinate { get { return X != NotKnown && Y != NotKnown && Z != NotKnown; } }

            public SystemClass System { get { return HasCoordinate ? new SystemClass(Name, null, X, Y, Z) : new SystemClass(Name); } }
        }

        public SavedRouteClass()
        {
            this.Id = -1;
            this.Systems = new List<SystemEntry>();
        }

        // create from name and no notes
        public SavedRouteClass(string name, List<SystemEntry> systems)
        {
            this.Id = -1;
            this.Name = name;
            this.Systems = systems;
        }

        public SavedRouteClass(DbDataReader dr, List<SystemEntry> syslist)
        {
            this.Id = (long)dr["id"];
            this.Name = (string)dr["name"];
            if (dr["start"] != DBNull.Value)
                this.StartDateUTC = ((DateTime)dr["start"]);
            if (dr["end"] != DBNull.Value)
                this.EndDateUTC = ((DateTime)dr["end"]);

            this.Systems = syslist;

            int statusbits = (int)dr["Status"];
            this.EDSM = (statusbits & 2) != 0;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        [JsonName("StartDate")]                                // fixed aug 2020, broken since name changed
        public DateTime? StartDateUTC { get; set; }            // UTC times now since jan 2020, changed to make compatible with gametime
        [JsonName("EndDate")]
        public DateTime? EndDateUTC { get; set; }
        public List<SystemEntry> Systems { get; private set; }
        public bool EDSM { get; set; }          // supplied by EDSM

        public bool Equals(SavedRouteClass other)
        {
            if (other == null)
            {
                return false;
            }

            return (this.Name == other.Name || (String.IsNullOrEmpty(this.Name) && String.IsNullOrEmpty(other.Name))) &&
                   this.StartDateUTC == other.StartDateUTC &&
                   this.EndDateUTC == other.EndDateUTC &&
                   this.Systems.Select(x => x.Name).SequenceEqual(other.Systems.Select(x => x.Name)) &&
                   this.Systems.Select(x => x.Note).SequenceEqual(other.Systems.Select(x => x.Note) 
                   );
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is SavedRouteClass)
            {
                return this.Equals((SavedRouteClass)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Name == null) ? 0 : Name.GetHashCode();
        }

        public void ReverseSystemList()
        {
            Systems.Reverse();
        }

        public bool Add()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(cn); });
        }

        private bool Add(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into routes_expeditions (name, start, end, Status) values (@name, @start, @end, @stat)"))
            {
                cmd.AddParameterWithValue("@name", Name);
                cmd.AddParameterWithValue("@start", StartDateUTC);
                cmd.AddParameterWithValue("@end", EndDateUTC);
                cmd.AddParameterWithValue("@stat", (EDSM ? 2 : 0));

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from routes_expeditions"))
                {
                    Id = (long)cmd2.ExecuteScalar();
                }

                using (DbCommand cmd3 = cn.CreateCommand("INSERT INTO route_systems (routeid, systemname, note, X, Y, Z) VALUES (@routeid, @name, @note, @X, @Y, @Z)"))
                {
                    cmd3.AddParameter("@routeid", DbType.String);
                    cmd3.AddParameter("@name", DbType.String);
                    cmd3.AddParameter("@note", DbType.String);
                    cmd3.AddParameter("@X", DbType.Double);
                    cmd3.AddParameter("@Y", DbType.Double);
                    cmd3.AddParameter("@Z", DbType.Double);

                    foreach (var sys in Systems)
                    {
                        cmd3.Parameters["@routeid"].Value = Id;
                        cmd3.Parameters["@name"].Value = sys.Name;
                        cmd3.Parameters["@note"].Value = sys.Note;
                        cmd3.Parameters["@X"].Value = sys.X;
                        cmd3.Parameters["@Y"].Value = sys.Y;
                        cmd3.Parameters["@Z"].Value = sys.Z;
                        cmd3.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }

        public bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        private bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("UPDATE routes_expeditions SET name=@name, start=@start, end=@end, Status=@stat WHERE id=@id"))
            {
                cmd.AddParameterWithValue("@id", Id);
                cmd.AddParameterWithValue("@name", Name);
                cmd.AddParameterWithValue("@start", StartDateUTC);
                cmd.AddParameterWithValue("@end", EndDateUTC);
                cmd.AddParameterWithValue("@stat", (EDSM ? 2 : 0));
                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("DELETE FROM route_systems WHERE routeid=@routeid"))
                {
                    cmd2.AddParameterWithValue("@routeid", Id);
                    cmd2.ExecuteNonQuery();
                }

                using (DbCommand cmd2 = cn.CreateCommand("INSERT INTO route_systems (routeid, systemname, note, X, Y, Z) VALUES (@routeid, @name, @note, @X, @Y, @Z)"))
                {
                    cmd2.AddParameter("@routeid", DbType.String);
                    cmd2.AddParameter("@name", DbType.String);
                    cmd2.AddParameter("@note", DbType.String);
                    cmd2.AddParameter("@X", DbType.Double);
                    cmd2.AddParameter("@Y", DbType.Double);
                    cmd2.AddParameter("@Z", DbType.Double);

                    foreach (var sys in Systems)
                    {
                        cmd2.Parameters["@routeid"].Value = Id;
                        cmd2.Parameters["@name"].Value = sys.Name;
                        cmd2.Parameters["@note"].Value = sys.Note;
                        cmd2.Parameters["@X"].Value = sys.X;
                        cmd2.Parameters["@Y"].Value = sys.Y;
                        cmd2.Parameters["@Z"].Value = sys.Z;
                        cmd2.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }

        public bool Delete()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Delete(cn); });
        }

        private bool Delete(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM routes_expeditions WHERE id=@id"))
            {
                cmd.AddParameterWithValue("@id", Id);
                cmd.ExecuteNonQuery();
            }

            using (DbCommand cmd = cn.CreateCommand("DELETE FROM route_systems WHERE routeid=@routeid"))
            {
                cmd.AddParameterWithValue("@routeid", Id);
                cmd.ExecuteNonQuery();
            }

            return true;
        }


        public static List<SavedRouteClass> GetAllSavedRoutes()
        {
            List<SavedRouteClass> retVal = new List<SavedRouteClass>();

            try
            {
                UserDatabase.Instance.DBRead(cn =>
                {
                    Dictionary<int, List<SystemEntry>> routesystems = new Dictionary<int, List<SystemEntry>>();

                    using (DbCommand cmd = cn.CreateCommand("SELECT routeid, systemname, note, X, Y, Z FROM route_systems ORDER BY id ASC"))
                    {
                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int routeid = (int)(long)rdr[0];
                                string sysname = (string)rdr[1];
                                string note = (rdr[2] as string) ?? "";     // paranoia, should not be saved with null
                                double x = (double)rdr[3];
                                double y = (double)rdr[4];
                                double z = (double)rdr[5];
                                if (!routesystems.ContainsKey(routeid))
                                {
                                    routesystems[routeid] = new List<SystemEntry>();
                                }
                                routesystems[routeid].Add(new SystemEntry(sysname,note,x,y,z));
                            }
                        }
                    }

                    using (DbCommand cmd = cn.CreateCommand("SELECT id, name, start, end, Status FROM routes_expeditions"))
                    {
                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int routeid = (int)(long)rdr[0];
                                List<SystemEntry> syslist = routesystems.ContainsKey(routeid) ? routesystems[routeid] : new List<SystemEntry>();
                                SavedRouteClass sys = new SavedRouteClass(rdr, syslist);
                                retVal.Add(sys);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.ToString());
            }

            return retVal;
        }

        // call when selected a route to fill in any system co-ords - note lots of predefined ones don't have them
        public bool FillInCoordinates()
        {
            bool changed = false;
            foreach( var se in Systems)
            {
                if ( !se.HasCoordinate)
                {
                    ISystem s = SystemCache.FindSystem(se.Name);     
                    if (s != null)
                    {
                        se.X = s.X;
                        se.Y = s.Y;
                        se.Z = s.Z;
                        changed = true;
                    }
                }
            }

            if (changed)        // if we have estimated some, write it back
                Update();

            return changed;
        }

        // list of systems with co-ordinates.  int holds original index of entry from Systems
        // return SystemEntry and Isystem for historic reasons
        public List<Tuple<ISystem, int, SystemEntry>> SystemsWithCoordinates()
        {
            var newlist = new List<Tuple<ISystem, int, SystemEntry>>();
            for(int i = 0; i < Systems.Count; i++)
            {
                if (Systems[i].HasCoordinate)
                    newlist.Add(new Tuple<ISystem, int, SystemEntry>(Systems[i].System, i, Systems[i]));
            }
            return newlist;
        }

        // distance along route from start, given knownsystems
        // optional first system to measure from
        public double CumulativeDistance(ISystem start = null, List<Tuple<ISystem, int, SystemEntry>> knownsystems = null)   
        {
            if ( knownsystems == null )
                knownsystems = SystemsWithCoordinates();

            double distance = 0;
            int i = 0;

            if (start != null)
            {
                i = knownsystems.FindIndex(x => x.Item1.Name.Equals(start.Name, StringComparison.InvariantCultureIgnoreCase));
                if (i == -1)
                    return -1;
            }

            for (i++; i < knownsystems.Count; i++)                          // from 1, or 1 past found, to end, accumulate distance
                distance += knownsystems[i].Item1.Distance(knownsystems[i - 1].Item1);

            return distance;
        }

        public ISystem PosAlongRoute(double percentage, int error = 0)             // go along route and give me a co-ord along it..
        {
            var knownsystems = SystemsWithCoordinates();

            double totaldist = CumulativeDistance(null, knownsystems);
            double distleft = totaldist * percentage / 100.0;

            if (knownsystems.Count < 2)     // need a path
                return null;

            Point3D syspos = null;
            string name = "";

            if (percentage < 0 || percentage > 100)                         // not on route, interpolate to/from
            {
                int i = (percentage < 0) ? 0 : knownsystems.Count - 2;      // take first two, or last two.

                Point3D pos1 = P3D(knownsystems[i].Item1);
                Point3D pos2 = P3D(knownsystems[i + 1].Item1);
                double p12dist = pos1.Distance(pos2);
                double pospath = (percentage > 100) ? (1.0 + (percentage - 100) * totaldist / p12dist / 100.0) : (percentage * totaldist / p12dist / 100.0);
                syspos = pos1.PointAlongPath(pos2, pospath);       // amplify percentage by totaldist/this path dist
                name = "System at " + percentage.ToString("N1");
            }
            else
            {
                for (int i = 1; i < knownsystems.Count; i++)
                {
                    double d = knownsystems[i].Item1.Distance(knownsystems[i - 1].Item1);

                    if (distleft < d || (i == knownsystems.Count - 1))        // if left, OR last system (allows for some rounding errors on floats)
                    {
                        d = distleft / d;
                        //System.Diagnostics.Debug.WriteLine(percentage + " " + d + " last:" + last.X + " " + last.Y + " " + last.Z + " s:" + s.X + " " + s.Y + " " + s.Z);
                        syspos = new Point3D(knownsystems[i - 1].Item1.X + (knownsystems[i].Item1.X - knownsystems[i - 1].Item1.X) * d, knownsystems[i - 1].Item1.Y + (knownsystems[i].Item1.Y - knownsystems[i - 1].Item1.Y) * d, knownsystems[i - 1].Item1.Z + (knownsystems[i].Item1.Z - knownsystems[i - 1].Item1.Z) * d);
                        name = "WP" + knownsystems[i - 1].Item2.ToString() + "-" + "WP" + knownsystems[i].Item2.ToString() + "-" + d.ToString("#.00") + $" @ {syspos.X:0.0},{syspos.Y:0.0},{syspos.Z:0.0}";
                        break;
                    }

                    distleft -= d;
                }
            }

            if (error > 0)
                return new SystemClass(name, null,syspos.X + rnd.Next(error), syspos.Y + rnd.Next(error), syspos.Z + rnd.Next(error));
            else
                return new SystemClass(name, null,syspos.X, syspos.Y, syspos.Z);
        }

        Random rnd = new Random();

        // given the system list, which is the next waypoint to go to.  return the system (or null if not available or past end) and the waypoint.. (0 based) and the distance on the path left..

        [DebuggerDisplay("{system.Name} w {waypoint} d {deviation} cwpd {cumulativewpdist} distto {disttowaypoint}")]
        public class ClosestInfo
        {
            public ISystem lastsystem;              // from
            public ISystem nextsystem;              // to
            public ISystem firstsystem;
            public ISystem finalsystem;
            public int waypoint;                    // index of Systems
            public string waypointnote;             // note on waypoint
            public double deviation;                // -1 if not on path
            public double cumulativewpdist;         // distance to end, 0 means no more WPs after this
            public double disttowaypoint;           // distance to WP
            public ClosestInfo(ISystem s, ISystem p, ISystem first, ISystem final, int w, string wpnote, double dv, double wdl, double dtwp)
            { lastsystem = s; nextsystem = p; firstsystem = first; finalsystem = final; waypoint = w; waypointnote = wpnote;  
                deviation = dv; cumulativewpdist = wdl; disttowaypoint = dtwp; }
        }

        static Point3D P3D(ISystem s)
        {
            return new Point3D(s.X, s.Y, s.Z);
        }

        public ClosestInfo ClosestTo(ISystem currentsystem)
        {
            Point3D currentsystemp3d = P3D(currentsystem);

            var knownsystems = SystemsWithCoordinates();

            if (knownsystems.Count < 1)     // need at least one
                return null;

            double mininterceptdist = Double.MaxValue;
            int wpto = -1;     // which waypoint are we between, N.. N+1

            double closesttodist = Double.MaxValue;
            int closestto = -1;

            for (int i = 0; i < knownsystems.Count; i++)
            {
                if (i > 0)
                {
                    Point3D lastp3d = P3D(knownsystems[i - 1].Item1);
                    Point3D top3d = P3D(knownsystems[i].Item1);

                    double distbetween = lastp3d.Distance(top3d);

                    double interceptpercent = lastp3d.InterceptPercentageDistance(top3d, currentsystemp3d, out double dist);       //dist to intercept point on line note.
                    //System.Diagnostics.Debug.WriteLine("From " + knownsystems[i - 1].ToString() + " to " + knownsystems[i].ToString() + " Percent " + interceptpercent + " Distance " + dist);

                    // allow a little margin in the intercept point for randomness, must be min dist, and not stupidly far.
                    if (interceptpercent >= -0.01 && interceptpercent < 1.01 && dist <= mininterceptdist && dist < distbetween)
                    {
                        wpto = i;
                        mininterceptdist = dist;
                    }
                }

                double disttofirstpoint = currentsystemp3d.Distance(P3D(knownsystems[i].Item1));

                if (disttofirstpoint < closesttodist)       // find the closest waypoint
                {
                    closesttodist = disttofirstpoint;
                    closestto = i;
                }
            }

            if (wpto == -1)        // if not on path
            {
                wpto = closestto;
                mininterceptdist = -1;
                //System.Diagnostics.Debug.WriteLine("Not on path, closest to" + knownsystems[closestto].ToString());
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Lies on line to WP" + interceptendpoint + " " + knownsystems[interceptendpoint].ToString());
            }

            double distto = currentsystemp3d.Distance(P3D(knownsystems[wpto].Item1));
            double cumldist = CumulativeDistance(knownsystems[wpto].Item1, knownsystems);

            return new ClosestInfo(wpto > 0 ? knownsystems[wpto - 1].Item1 : null,
                                    knownsystems[wpto].Item1,
                                    knownsystems[0].Item1,
                                    knownsystems.Last().Item1,
                                    knownsystems[wpto].Item2,
                                    knownsystems[wpto].Item3.Note,
                                    mininterceptdist,       // deviation from path
                                    cumldist,
                                    distto);
        }

        // Given a set of expedition files, update the DB.  Add any new ones, and make sure the EDSM marker is on.

        public static bool UpdateDBFromExpeditionFiles(string expeditiondir)
        {
            bool changed = false;

            FileInfo[] allfiles = Directory.EnumerateFiles(expeditiondir, "*.json", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

            foreach (FileInfo f in allfiles)        // iterate thru all files found
            {
                try
                {
                    string text = File.ReadAllText(f.FullName); // may except

                    if (text != null && text.Length > 0)      // use a blank file to overwrite an old one you want to get rid of
                    {
                        var ja = JArray.Parse(text, JToken.ParseOptions.AllowTrailingCommas);        // JSON has trailing commas illegally but keep for backwards compat

                        List<SavedRouteClass> stored = SavedRouteClass.GetAllSavedRoutes();

                        foreach (var route in ja)           // for each defined route object
                        {
                            SavedRouteClass rt = new SavedRouteClass();
                            rt.Name = route["Name"].Str();
                            rt.StartDateUTC = route["StartDate"].DateTimeUTC();
                            rt.EndDateUTC = route["EndDate"].DateTimeUTC();
                            rt.EDSM = true;

                            var sysa = route["Systems"].Array();
                            if (sysa != null)
                            {
                                foreach (var syse in sysa)
                                {
                                    rt.Systems.Add(new SystemEntry(syse.Str("Unknown")));
                                }

                                SavedRouteClass storedentry = stored.Find(x => x.Name.Equals(rt.Name));     // find it..

                                if (storedentry != null)        // if found.. and different, delete and update
                                {
                                    if (!storedentry.Systems.Select(x => x.Name).SequenceEqual(rt.Systems.Select(y => y.Name))) // systems changed, we need to reset..
                                    {
                                        storedentry.Delete();
                                        rt.Add();
                                        changed = true;
                                    }
                                }
                                else
                                {
                                    rt.Add();
                                    changed = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Read DB files excepted {ex}");
                }
            }

            return changed;
        }

        public void TestHarness()       // fly the route and debug the closestto.. keep this for testing
        {
            for (double percent = -10; percent < 110; percent += 0.1)
            {
                ISystem cursys = PosAlongRoute(percent, 100);
                System.Diagnostics.Debug.WriteLine(Environment.NewLine + "Sys {0} {1} {2} {3}", cursys.X, cursys.Y, cursys.Z, cursys.Name);

                if (cursys != null)
                {
                    ClosestInfo closest = ClosestTo(cursys);

                    if (closest != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Next {0} {1} {2} {3}, index {4} dev {5} dist to wp {6} cumul left {7}", closest.lastsystem?.X, closest.lastsystem?.Y, closest.lastsystem?.Z, closest.lastsystem?.Name, closest.waypoint, closest.deviation, closest.disttowaypoint, closest.cumulativewpdist);
                    }
                }
            }
        }
    }
}
