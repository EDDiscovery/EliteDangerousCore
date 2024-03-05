/*
 * Copyright 2016-2024 EDDiscovery development team
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
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EliteDangerousCore.DB
{
    public class PlanetMarks
    {
        public class Location
        {
            public string Name { get; set; }
            public string Comment { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Tags { get; set; } = "";     // tag;tag;  Preset to "" as older entries won't have it in json

            [JsonIgnore]
            public bool IsWholePlanetBookmark { get { return Latitude == 0 && Longitude == 0; } }
        }

        public class Planet
        {
            public string Name { get; set; }
            public List<Location> Locations { get; set; }            // may be null from reader..
        }

        public List<Planet> Planets { get; set; }                    // may be null if no planets

        public bool hasMarks { get { return Planets != null && Planets.Count > 0 && Planets.Where(pl => pl.Locations.Count > 0).Any(); } }

        public PlanetMarks(string json)
        {
            try // prevent crashes
            {
                JObject jo = JObject.ParseThrowCommaEOL(json);
                if (jo["Marks"] != null)
                {
                    Planets = jo["Marks"].ToObject<List<Planet>>();        //verified with basutils.json 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BK PM " + ex.ToString());
            }
        }

        public PlanetMarks()
        {
        }

        // May return null, if no Planets in JSON
        public string ToJsonString()
        {
            if (Planets != null)
            {
                JArray ja = new JArray();
                foreach (Planet p in Planets)
                    ja.Add(JObject.FromObject(p));     

                JObject overall = new JObject();
                overall["Marks"] = ja;
                return overall.ToString();
            }
            else
                return null;
        }

        public IEnumerator<Tuple<Planet,Location>> GetEnumerator()
        {
            foreach (Planet pl in Planets)
            {
                foreach (Location loc in pl.Locations)
                {
                    yield return new Tuple<Planet,Location>(pl,loc);
                }
            }
        }

        public Planet GetPlanet(string planet)  // null if planet does not exist.. else array
        {
            return Planets?.Find(x => x.Name.Equals(planet, StringComparison.InvariantCultureIgnoreCase));
        }

        public Location GetLocation(Planet p, string placename)  // null if planet or place does not exist..
        {
            return p?.Locations?.Find(x => x.Name.Equals(placename, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddOrUpdateLocation(string planet, string placename, string comment, double latp, double longp, string tags)
        {
            Planet p = GetPlanet(planet);            // p = null if planet does not exist, else list of existing places

            if (p == null)      // no planet, make one up
            {
                if (Planets == null)
                    Planets = new List<Planet>();       // new planet list

                p = new Planet() { Name = planet };
                Planets.Add(p);
            }

            if (p.Locations == null)                    // done here, just in case we read a planet not locations in json.
                p.Locations = new List<Location>();

            Location l = GetLocation(p, placename);     // location on planet by name

            if (l == null)                      // no location.. make one up and add
            {
                l = new Location() { Name = placename, Comment = comment, Latitude = latp, Longitude = longp, Tags = tags };
                p.Locations.Add(l);
            }
            else
            {
                l.Comment = comment;        // update fields which may have changed
                l.Latitude = latp;
                l.Longitude = longp;
                l.Tags = tags;
            }
        }

        public void AddOrUpdateLocation(string planet, Location loc)
        {
            AddOrUpdateLocation(planet, loc.Name, loc.Comment, loc.Latitude, loc.Longitude, loc.Tags);
        }

        public void AddOrUpdatePlanetBookmark(string planet, string comment, string tags)
        {
            AddOrUpdateLocation(planet, "", comment, 0,0, tags );
        }

        public bool DeleteLocation(string planet, string placename)
        {
            Planet p = GetPlanet(planet);            // p = null if planet does not exist, else list of existing places
            Location l = GetLocation(p, placename); // if p != null, find placename 
            if (l != null)
            {
                p.Locations.Remove(l);
                if (p.Locations.Count == 0) // nothing left?
                    Planets.Remove(p);  // remove planet.
            }
            return l != null;
        }

        public bool HasLocation(string planet, string placename)
        {
            Planet p = GetPlanet(planet);            // p = null if planet does not exist, else list of existing places
            Location l = GetLocation(p, placename); // if p != null, find placenameYour okay, its 
            return l != null;
        }

        public bool UpdateComment(string planet, string placename, string comment)
        {
            Planet p = GetPlanet(planet);            // p = null if planet does not exist, else list of existing places
            Location l = GetLocation(p, placename); // if p != null, find placenameYour okay, its 
            if (l != null)
            {
                l.Comment = comment;
                return true;
            }
            else
                return false;
        }
        public bool UpdateTags(string planet, string placename, string tags)
        {
            Planet p = GetPlanet(planet);            // p = null if planet does not exist, else list of existing places
            Location l = GetLocation(p, placename); // if p != null, find placenameYour okay, its 
            if (l != null)
            {
                l.Tags = tags;
                return true;
            }
            else
                return false;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Name} {x} {y} {z} {Note}")]
    public class BookmarkClass
    {
        public long ID { get; set; }
        public string StarName { get; set; }         // set if associated with a star, else null
        public double X { get; set; }                // x/y/z always set for render purposes
        public double Y { get; set; }
        public double Z { get; set; }
        public DateTime TimeUTC { get; set; }
        public string Heading { get; set; }          // set if region bookmark, else null if its a star
        public string Note { get; set; }
        public PlanetMarks PlanetaryMarks { get; set; }   // may be null
        public string Tags { get; set; } = "";      // tag;tag;  Preset to "" as older entries won't have it in json

        public bool IsRegion { get { return Heading != null; } }
        public bool IsStar { get { return Heading == null; } }
        public string Name { get { return Heading == null ? StarName : Heading; } }

        public bool HasPlanetaryMarks
        { get { return PlanetaryMarks != null && PlanetaryMarks.hasMarks; } }

        public BookmarkClass()
        {
        }

        public BookmarkClass(DbDataReader dr)
        {
            ID = (long)dr["id"];
            if (System.DBNull.Value != dr["StarName"])
                StarName = (string)dr["StarName"];
            X = (double)dr["x"];
            Y = (double)dr["y"];
            Z = (double)dr["z"];

            DateTime t = (DateTime)dr["Time"];
            if (t < EDDFixesDates.BookmarkUTCswitchover)      // dates before this was stupidly recorded in here in local time.
            {
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, DateTimeKind.Local);
                t = t.ToUniversalTime();
            }
            TimeUTC = t;

            if (System.DBNull.Value != dr["Heading"])
                Heading = (string)dr["Heading"];
            Note = (string)dr["Note"];
            if (System.DBNull.Value != dr["PlanetMarks"])
            {
                //System.Diagnostics.Debug.WriteLine("Planet mark {0} {1}", StarName, (string)dr["PlanetMarks"]);
                PlanetaryMarks = new PlanetMarks((string)dr["PlanetMarks"]);
            }

            Tags = (string)dr["Tags"];
        }

        internal bool Add()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Add(cn); });
        }

        private bool Add(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Insert into Bookmarks (StarName, x, y, z, Time, Heading, Note, PlanetMarks, Tags) values (@sname, @xp, @yp, @zp, @time, @head, @note, @pmarks,@tags)"))
            {
                DateTime tme = TimeUTC;
                if (TimeUTC < EDDFixesDates.BookmarkUTCswitchover)
                    tme = TimeUTC.ToLocalTime();

                cmd.AddParameterWithValue("@sname", StarName);
                cmd.AddParameterWithValue("@xp", X);
                cmd.AddParameterWithValue("@yp", Y);
                cmd.AddParameterWithValue("@zp", Z);
                cmd.AddParameterWithValue("@time", tme);
                cmd.AddParameterWithValue("@head", Heading);
                cmd.AddParameterWithValue("@note", Note);
                cmd.AddParameterWithValue("@pmarks", PlanetaryMarks?.ToJsonString());
                cmd.AddParameterWithValue("@tags", Tags);

                cmd.ExecuteNonQuery();

                using (DbCommand cmd2 = cn.CreateCommand("Select Max(id) as id from Bookmarks"))
                {
                    ID = (long)cmd2.ExecuteScalar();
                }

                return true;
            }
        }

        internal bool Update()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Update(cn); });
        }

        private bool Update(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("Update Bookmarks set StarName=@sname, x = @xp, y = @yp, z = @zp, Time=@time, Heading = @head, Note=@note, PlanetMarks=@pmarks, Tags=@tags  where ID=@id"))
            {
                DateTime tme = TimeUTC;
                if (TimeUTC < EDDFixesDates.BookmarkUTCswitchover)
                    tme = TimeUTC.ToLocalTime();

                cmd.AddParameterWithValue("@ID", ID);
                cmd.AddParameterWithValue("@sname", StarName);
                cmd.AddParameterWithValue("@xp", X);
                cmd.AddParameterWithValue("@yp", Y);
                cmd.AddParameterWithValue("@zp", Z);
                cmd.AddParameterWithValue("@time", tme);
                cmd.AddParameterWithValue("@head", Heading);
                cmd.AddParameterWithValue("@note", Note);
                cmd.AddParameterWithValue("@pmarks", PlanetaryMarks?.ToJsonString());
                cmd.AddParameterWithValue("@tags", Tags);

                cmd.ExecuteNonQuery();

                return true;
            }
        }

        internal bool Delete()
        {
            return UserDatabase.Instance.DBWrite<bool>(cn => { return Delete(cn); });
        }

        private bool Delete(SQLiteConnectionUser cn)
        {
            using (DbCommand cmd = cn.CreateCommand("DELETE FROM Bookmarks WHERE id = @id"))
            {
                cmd.AddParameterWithValue("@id", ID);
                cmd.ExecuteNonQuery();
                return true;
            }
        }

        // with a found bookmark.. add locations in the system
        public void AddOrUpdateLocation(string planet, string placename, string comment, double latp, double longp, string tags)
        {
            if (PlanetaryMarks == null)
                PlanetaryMarks = new PlanetMarks();
            PlanetaryMarks.AddOrUpdateLocation(planet, placename, comment, latp, longp, tags);
            Update();
        }

        public void AddOrUpdatePlanetBookmark(string planet, string comment, string tags)
        {
            if (PlanetaryMarks == null)
                PlanetaryMarks = new PlanetMarks();
            PlanetaryMarks.AddOrUpdatePlanetBookmark(planet, comment,tags);
            Update();
        }

        public void UpdateNote(string notes)
        {
            Note = notes;
            Update();
        }
        public void UpdateTags(string tags)
        {
            Tags = tags;
            Update();
        }

        public bool HasLocation(string planet, string placename)
        {
            return PlanetaryMarks != null && PlanetaryMarks.HasLocation(planet, placename);
        }

        public bool DeleteLocation(string planet, string placename)
        {
            if (PlanetaryMarks != null && PlanetaryMarks.DeleteLocation(planet, placename))
            {
                Update();
                return true;
            }
            else
                return false;
        }

        public bool UpdateLocationComment(string planet, string placename, string comment)
        {
            if (PlanetaryMarks != null && PlanetaryMarks.UpdateComment(planet, placename, comment))
            {
                Update();
                return true;
            }
            else
                return false;
        }
        public bool UpdateLocationTags(string planet, string placename, string tags)
        {
            if (PlanetaryMarks != null && PlanetaryMarks.UpdateTags(planet, placename, tags))
            {
                Update();
                return true;
            }
            else
                return false;
        }
    }

    // EVERYTHING goes thru list class for adding/deleting bookmarks

    public class GlobalBookMarkList
    {
        public static bool Instanced { get { return gbl != null; } }
        public static GlobalBookMarkList Instance { get { return gbl; } }

        public List<BookmarkClass> Bookmarks { get { return globalbookmarks; } }

        public Action<BookmarkClass, bool> OnBookmarkChange;        // bool = true if deleted

        private static GlobalBookMarkList gbl = null;

        private List<BookmarkClass> globalbookmarks = new List<BookmarkClass>();

        public static bool LoadBookmarks()
        {
            System.Diagnostics.Debug.Assert(gbl == null);       // no double instancing!
            gbl = new GlobalBookMarkList();

            try
            {
                List<BookmarkClass> bookmarks = new List<BookmarkClass>();

                UserDatabase.Instance.DBRead(cn =>
                {
                    using (DbCommand cmd = cn.CreateCommand("select * from Bookmarks"))
                    {
                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                bookmarks.Add(new BookmarkClass(rdr));
                            }
                        }
                    }
                });


                if (bookmarks.Count == 0)
                {
                    return false;
                }
                else
                {
                    foreach (var bc in bookmarks)
                    {
                        gbl.globalbookmarks.Add(bc);
                    }
                    return true;
                }
            }
            catch( Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex.ToString());
                return false;
            }
        }

        // return any mark
        public BookmarkClass FindBookmarkOnRegion(string name)   
        {
            BookmarkClass bc = globalbookmarks.Find(x => x.Heading != null && x.Heading.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return bc;
        }

        public BookmarkClass FindBookmarkOnSystem(string name)
        {
            // star name may be null if its a region mark
            return globalbookmarks.Find(x => x.StarName != null && x.StarName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
        public BookmarkClass FindBookmark(string name , bool region)
        {
            // star name may be null if its a region mark
            return (region) ? FindBookmarkOnRegion(name) : FindBookmarkOnSystem(name);
        }

        // bk = null, new bookmark, else update.  isstar = true, region = false.
        public BookmarkClass AddOrUpdateBookmark(BookmarkClass bk, bool isstar, string name, double x, double y, double z, DateTime timeutc, 
                                                 string notes = null,       // if null, don't update notes
                                                 string tags = null,        // if null, don't update tags
                                                 PlanetMarks planetMarks = null)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);
            bool addit = bk == null;

            if (addit)
            {
                bk = new BookmarkClass();
                bk.Note = "";       // set empty, in case notes==null
                globalbookmarks.Add(bk);
                System.Diagnostics.Debug.WriteLine("New bookmark created");
            }

            if (isstar)
                bk.StarName = name;
            else
                bk.Heading = name;

            bk.X = x;
            bk.Y = y;
            bk.Z = z;
            bk.TimeUTC = timeutc;            bk.PlanetaryMarks = planetMarks ?? bk.PlanetaryMarks;
            bk.Note = notes ?? bk.Note; // only override if its set.
            bk.Tags = tags ?? bk.Tags;// only override if its set.

            if (addit)
                bk.Add();
            else
            {
                System.Diagnostics.Debug.WriteLine(GlobalBookMarkList.Instance.Bookmarks.Find((xx) => Object.ReferenceEquals(bk, xx)) != null);
                bk.Update();
            }

            System.Diagnostics.Debug.WriteLine($"Write bookmark {bk.Name} Notes {notes} Tags {tags}");

            OnBookmarkChange?.Invoke(bk,false);

            return bk;
		}	

        public void Delete(BookmarkClass bk)
        {
            System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);
            long id = bk.ID;
            bk.Delete();
            globalbookmarks.RemoveAll(x => x.ID == id);
            OnBookmarkChange?.Invoke(bk, true);
        }

        public void TriggerChange(BookmarkClass bk)
        {
            OnBookmarkChange?.Invoke(bk, true);
        }
    }
}
