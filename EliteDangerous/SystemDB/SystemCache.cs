/*
 * Copyright 2015 - 2021 EDDiscovery development team
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

using BaseUtils;
using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.DB
{
    public static class SystemCache
    {
        // may return null if not found
        // by design, it keeps on trying.  Rob thought about caching the misses but the problem is, this is done at start up
        // the system db may not be full at that point.  So a restart would be required to clear the misses..
        // difficult

        #region Public Interface for Find System

        // in historylist, addtocache for all fsd jumps and navroutes.
        public static System.Threading.Tasks.Task<ISystem> FindSystemAsync(string name, EDSM.GalacticMapping glist, bool checkedsm = false)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                //if (checkedsm)
                //{
                //    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount % 10000} PAUSE");
                //    System.Threading.Thread.Sleep(5000);
                //    System.Diagnostics.Debug.WriteLine($"{Environment.TickCount % 10000} RESUME");
                //}
                return FindSystem(name, glist, checkedsm);
            });
        }

        public static System.Threading.Tasks.Task<ISystem> FindSystemAsync(ISystem find, bool checkedsm = false)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return FindSystem(find, checkedsm);
            });
        }

        public static ISystem FindSystem(string name, EDSM.GalacticMapping glist, bool checkedsm)
        {
            ISystem sys = FindSystem(name, checkedsm);

            if ( sys == null && glist != null)
            {
                EDSM.GalacticMapObject gmo = glist.Find(name, true);

                if (gmo != null && gmo.Points.Count > 0)                // valid item, and has position
                {
                    var sys1 = SystemCache.FindSystem(gmo.GalMapSearch);     // only thru the db/cache, as we checked above for edsm direct, may be null
                    if (sys1 != null)
                        sys = sys1;

                    return gmo.GetSystem(sys);                          // and return a ISystem.  If sys=null, we use the points pos, if sys is found, we use the cache position 
                }
            }

            return sys;
        }
    
        public static ISystem FindSystem(string name, bool checkedsm = false)
        {
            return FindSystem(new SystemClass(name), checkedsm);
        }

        // look up thru cache and db, and optionally edsm
        // thread safe
        public static ISystem FindSystem(ISystem find, bool checkedsm = false)
        {
            ISystem found;

            if (SystemsDatabase.Instance.RebuildRunning) // Find the system in the cache if a rebuild is running
            {
                found = FindSystemInCacheDB(find, null);
            }
            else
            {
                found = SystemsDatabase.Instance.DBRead(conn => FindSystemInCacheDB(find, conn));

                // we need to do this after the normal connection above, as if we find something, we need to go into read write mode (took a moment to realise this)

                // if not found, checking edsm, and its a good name

                if (found == null && checkedsm && find.Name.HasChars() && find.Name != "UnKnown")
                {
                    lock (edsmnotfoundlist)                                // lock it against threads
                    {
                        if (!edsmnotfoundlist.Contains(find.Name))        // if not in not found list - the ones we have already checked
                        {
                            EDSM.EDSMClass edsm = new EDSM.EDSMClass();
                            found = edsm.GetSystem(find.Name)?.FirstOrDefault();     // this may return an empty list, so first or default, or it may return null

                            // if found one, and EDSM ID/coords (paranoia), add back to our db so next time we have it

                            if (found != null && found.EDSMID > 0 && found.HasCoordinate)       // if its a good system
                            {
                                SystemsDatabase.Instance.StoreSystems(new List<ISystem> { found });     // won't do anything if rebuilding
                            }
                            else
                            {
                                edsmnotfoundlist.Add(find.Name);            // else exclude from futher checks. Checked 22/2/21
                            }
                        }
                    }
                }
            }

            return found;
        }

        // core find, with database
        // find in cache, find in db, add to cache

        public static ISystem FindSystemInCacheDB(ISystem find, SQLiteConnectionSystem cn)
        {
            ISystem orgsys = find;

            // Find it from the cache

            ISystem found = FindCachedSystem(find);

            // not found, or not from EDSM, AND we have a database and its not rebuilding
            // see if we can find it in the DB

            if ((found == null || found.Source != SystemSource.FromEDSM) && cn != null && !SystemsDatabase.Instance.RebuildRunning)   
            {
                //System.Diagnostics.Debug.WriteLine("Look up from DB " + sys.name + " " + sys.id_edsm);

                bool findnameok = find.Name.HasChars() && find.Name != "UnKnown";
                ISystem dbfound = null;

                if (find.EDSMID > 0)        // if we have an ID, look it up
                {
                    dbfound = DB.SystemsDB.FindStar(find.EDSMID,cn);

                    if (dbfound != null && findnameok )      // if we find it, use the find name in the return as the EDSM name may be out of date..
                        dbfound.Name = find.Name;
                }

                if (dbfound == null && findnameok)            // if not found but has a good name
                    dbfound = DB.SystemsDB.FindStar(find.Name,cn);   // find by name, no wildcards

                if (dbfound == null && find.HasCoordinate)        // finally, not found, but we have a co-ord, find it from the db  by distance
                    dbfound = DB.SystemsDB.GetSystemByPosition(find.X, find.Y, find.Z, cn);

                if (dbfound == null)
                {
                    long newid = DB.SystemsDB.FindAlias(find.EDSMID, find.Name , cn);   // is there a named alias in there due to a system being renamed..
                    if (newid >= 0)
                        dbfound = DB.SystemsDB.FindStar(newid,cn);  // find it using the new id
                }

                if (dbfound != null)                            // if we have a good db, go for it
                {
                    if (find.HasCoordinate)                     // if find has co-ordinate, it may be more up to date than the DB, so use it
                    {
                        dbfound.X = find.X; dbfound.Y = find.Y; dbfound.Z = find.Z;
                    }

                    lock(cachelockobject)          // lock to prevent multi change over these classes
                    {
                        if (systemsByName.ContainsKey(orgsys.Name))   // so, if name database already has name
                            systemsByName[orgsys.Name].Remove(orgsys);  // and remove the ISystem if present on that orgsys

                        AddToCache(dbfound);
                    }

                    found = dbfound;
                    //System.Diagnostics.Trace.WriteLine($"DB found {found.name} {found.id_edsm} sysid {found.id_edsm}");
                }

                return found;
            }
            else
            {                                               // from cache, or database is rebuilding
                if (found != null)                          // its in the cache, return it
                {
                    return found;
                }

                // not in the cache, see if the request has enough data to be added to the cache

                if (find.Source == SystemSource.FromJournal && find.Name != null && find.HasCoordinate == true)
                {
                    AddToCache(find);
                    return find;
                }

                //System.Diagnostics.Trace.WriteLine($"Cached reference to {found.name} {found.id_edsm}");
                return found;       // no need for extra work.
            }
        }

        // find in cache, thread safe
        public static ISystem FindCachedSystem(ISystem find)
        {
            List<ISystem> foundlist = new List<ISystem>();
            ISystem found = null;

            lock(cachelockobject)          // Rob seen instances of it being locked together in multiple star distance threads, we need to serialise access to these two dictionaries
            {                               // Concurrent dictionary no good, they could both be about to add the same thing at the same time and pass the contains test.

                if (find.EDSMID > 0 && systemsByEdsmId.ContainsKey(find.EDSMID))        // add to list
                {
                    ISystem s = systemsByEdsmId[find.EDSMID];
                    foundlist.Add(s);
                }

                if (systemsByName.ContainsKey(find.Name))            // and all names cached
                {
                    List<ISystem> s = systemsByName[find.Name];
                    foundlist.AddRange(s);
                }
            }

            if (find.HasCoordinate && foundlist.Count > 0)           // if sys has a co-ord, find the best match within 0.5 ly
                found = NearestTo(foundlist, find, 0.5);

            if (found == null && foundlist.Count == 1 && !find.HasCoordinate) // if we did not find one, but we have only 1 candidate, use it.
                found = foundlist[0];

            return found;
        }

        public static void AddSystemToCache(ISystem system)
        {
            var found = FindCachedSystem(system);

            // Existing finds with journal or edsm override the add..
            // if not found or found is not journal/edsm, AND the system has coord and name, add
            if ((found == null || (found.Source != SystemSource.FromJournal && found.Source != SystemSource.FromEDSM)) && system.HasCoordinate == true && system.Name.HasChars())
            {
                AddToCache(system, found);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"Systemcache reject {system.Name} {system.Source}");
            }
        }

        // return, maybe an empty, list of systems, non duplicated
        public static HashSet<ISystem> FindSystemWildcard(string name, int limit = int.MaxValue)
        {
            HashSet<ISystem> systems = new HashSet<ISystem>(new ISystemNameCompareCaseInsensitiveInvariantCulture());      // important, compare on name case insensite

            lock (cachelockobject)  // systems can exist in the cache as well as the db
            {
                name = name.ToLowerInvariant();

                systems.AddRange(systemsByName.Where(kvp => kvp.Key.ToLowerInvariant().StartsWith(name))
                                    .SelectMany(s => s.Value)
                                    .Take(limit)
                                    .ToList());
            }

            if (!SystemsDatabase.Instance.RebuildRunning) // Return from cache if rebuild is running
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    var list = DB.SystemsDB.FindStarWildcard(name, cn, limit);
                    if (list != null)
                        systems.AddRange(list);
                });
            }

            return systems;
        }

        // system list with distances

        public static void GetSystemListBySqDistancesFrom(BaseUtils.SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                                    int maxitems,
                                                    double mindist, double maxdist, bool spherical)
        {
            lock(cachelockobject)
            {
                var sysdist = systemsByName.Values
                                            .SelectMany(s => s)
                                            .Select(s => new { distsq = s.DistanceSq(x, y, z), sys = s })
                                            .OrderBy(s => s.distsq)
                                            .ToList();
                var minsq = mindist * mindist;
                var maxsq = maxdist * maxdist;

                foreach (var sd in sysdist)
                {
                    if (sd.distsq <= minsq && sd.distsq >= maxsq)
                    {
                        distlist.Add(sd.distsq, sd.sys);
                    }

                    if (distlist.Count >= maxitems)
                    {
                        break;
                    }
                }
            }

            if (!SystemsDatabase.Instance.RebuildRunning) // Return from cache if rebuild is running
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    DB.SystemsDB.GetSystemListBySqDistancesFrom(distlist, x, y, z, maxitems, mindist, maxdist, spherical, cn, (s) => AddToCache(s));
                });
            }
        }

        // find by position
        public static ISystem GetSystemByPosition(double x, double y, double z, uint warnthreshold = 500)
        {
            return FindNearestSystemTo(x, y, z, 0.125, warnthreshold);
        }

        // find nearest system
        public static ISystem FindNearestSystemTo(double x, double y, double z, double maxdistance, uint warnthreshold = 500)
        {
            ISystem sys = null;
            lock (cachelockobject)
            {
                sys = systemsByName.Values
                                    .SelectMany(s => s)
                                    .OrderBy(s => s.Distance(x, y, z))
                                    .FirstOrDefault(e => e.Distance(x, y, z) < maxdistance);
            }

            if (!SystemsDatabase.Instance.RebuildRunning)
            {
                ISystem dbsys = null;
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    dbsys = DB.SystemsDB.GetSystemByPosition(x, y, z, cn, maxdistance);
                });

                if (dbsys != null && sys != null && dbsys.Distance(x, y, z) < sys.Distance(x, y, z))
                {
                    sys = dbsys;
                }
            }

            return sys;
        }

        // return system nearest to, given range
        public static ISystem GetSystemNearestTo(Point3D currentpos,
                                                 Point3D wantedpos,
                                                 double maxfromcurpos,
                                                 double maxfromwanted,
                                                 SystemsDB.SystemsNearestMetric routemethod,
                                                 int limitto)
        {
            List<ISystem> candidates;

            lock (cachelockobject)
            {
                candidates = systemsByName.Values
                                 .SelectMany(s => s)
                                 .Select(s => new
                                 {
                                     dc = s.Distance(currentpos.X, currentpos.Y, currentpos.Z),
                                     dw = s.Distance(wantedpos.X, wantedpos.Y, wantedpos.Z),
                                     sys = s
                                 })
                                 .Where(s => s.dw < maxfromwanted && s.dc < maxfromcurpos)
                                 .OrderBy(s => s.dw)
                                 .Select(s => s.sys)
                                 .ToList();
            }

            if (!SystemsDatabase.Instance.RebuildRunning)    // return from cache is rebuild is running
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    var list = DB.SystemsDB.GetSystemNearestTo(currentpos, wantedpos, maxfromcurpos, maxfromwanted, routemethod, cn, (s) => AddToCache(s), limitto);
                    candidates.AddRange(list);
                });
            }

            return DB.SystemsDB.GetSystemNearestTo(candidates, currentpos, wantedpos, maxfromcurpos, maxfromwanted, routemethod);
        }

        #endregion

        #region Autocomplete

        // use this for additional autocompletes outside of the normal stars
        public static void AddToAutoCompleteList(List<string> t)
        {
            lock (AutoCompleteAdditionalList)
            {
                AutoCompleteAdditionalList.AddRange(t);
            }
        }

        public static void ReturnSystemAdditionalListForAutoComplete(string input, Object ctrl, SortedSet<string> set )
        {
            ReturnAdditionalAutoCompleteList(input, ctrl, set);
            ReturnSystemAutoCompleteList(input, ctrl, set);
        }

        public static void ReturnAdditionalAutoCompleteList(string input, Object ctrl, SortedSet<string> set)
        {
            if (input != null && input.Length > 0)
            {
                lock (AutoCompleteAdditionalList)
                {
                    foreach (string other in AutoCompleteAdditionalList)
                    {
                        if (other.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
                            set.Add(other);
                    }
                }
            }
        }

        public static int MaximumStars { get; set; } = 1000;

        public static void ReturnSystemAutoCompleteList(string input, Object ctrl, SortedSet<string> set)
        {
            if (input.HasChars())
            {
                if (!SystemsDatabase.Instance.RebuildRunning)           // DB okay, go and use it to find stars
                {
                    List<ISystem> systems = DB.SystemsDB.FindStarWildcard(input, MaximumStars);
                    foreach (var i in systems)
                    {
                        AddToCache(i);
                        set.Add(i.Name);
                    }

                    List<ISystem> aliases = DB.SystemsDB.FindAliasWildcard(input);
                    foreach (var i in aliases)
                    {
                        AddToCache(i);
                        set.Add(i.Name);
                    }
                }

                lock (cachelockobject)          // check out the cache object
                {
                    input = input.ToLowerInvariant();

                    foreach (var kvp in systemsByName)
                    {
                        if (kvp.Key.ToLowerInvariant().StartsWith(input))
                        {
                            foreach (var s in kvp.Value)
                            {
                                set.Add(s.Name);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        // add found to cache
        // add to edsm id list if edsmid set
        // find or create a star list under name
        // if orgsys is set, then we can use its position to try and find a star match in the name found list
        static private void AddToCache(ISystem found, ISystem orgsys = null)
        {
            lock(cachelockobject)
            {
                //System.Diagnostics.Debug.WriteLine($"SystemCache add {found.Name}");
                if (found.EDSMID > 0)
                    systemsByEdsmId[found.EDSMID] = found;  // must be definition the best ID found.. and if the update date of sys is better, its now been updated

                List<ISystem> byname;

                if (!systemsByName.TryGetValue(found.Name, out byname))
                {
                    systemsByName[found.Name] = byname = new List<ISystem>();
                }

                int idx = -1;

                if (found.EDSMID > 0)
                {
                    idx = byname.FindIndex(e => e.EDSMID == found.EDSMID);
                }

                if (idx < 0)
                {
                    idx = byname.FindIndex(e => e.Xi == found.Xi && e.Yi == found.Yi && e.Zi == found.Zi);
                }

                if (idx < 0 && orgsys != null)
                {
                    idx = byname.FindIndex(e => e.Xi == orgsys.Xi && e.Yi == orgsys.Yi && e.Zi == orgsys.Zi);
                }

                if (idx >= 0)
                {
                    byname[idx] = found;
                }
                else
                {
                    byname.Add(found);
                }
            }
        }

        static private ISystem NearestTo(List<ISystem> list, ISystem comparesystem, double mindist)
        {
            ISystem nearest = null;

            foreach (ISystem isys in list)
            {
                if (isys.HasCoordinate)
                {
                    double dist = isys.Distance(comparesystem);

                    if (dist < mindist)
                    {
                        mindist = dist;
                        nearest = isys;
                    }
                }
            }

            return nearest;
        }

        private static Object cachelockobject = new object();        // so we are agnostic about systemsbyEDSM/Name as the locker

        private static Dictionary<long, ISystem> systemsByEdsmId = new Dictionary<long, ISystem>();
        private static Dictionary<string, List<ISystem>> systemsByName = new Dictionary<string, List<ISystem>>(StringComparer.InvariantCultureIgnoreCase);

        private static HashSet<string> edsmnotfoundlist = new HashSet<string>();

        private static List<string> AutoCompleteAdditionalList = new List<string>();

        #endregion
    }

}

