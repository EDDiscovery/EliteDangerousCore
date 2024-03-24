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

using BaseUtils;
using EliteDangerousCore.DB;
using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public static class SystemCache
    {
        #region Public Interface for Find System

#if !TESTHARNESS
        // in historylist, addtocache for all fsd jumps and navroutes.
        public static System.Threading.Tasks.Task<ISystem> FindSystemAsync(string name, GMO.GalacticMapping glist, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return FindSystem(name, glist, lookup);
            });
        }

        public static System.Threading.Tasks.Task<ISystem> FindSystemAsync(ISystem find, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return FindSystem(find, lookup);
            });
        }

        // find system name, from db, web if selected, optional GMO list
        public static ISystem FindSystem(string name, GMO.GalacticMapping glist, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            ISystem sys = FindSystem(name, lookup);

            if (sys == null && glist != null)
            {
                GMO.GalacticMapObject gmo = glist.FindSystem(name);

                if (gmo != null)
                    return gmo.StarSystem;
            }

            return sys;
        }
#endif

        public static ISystem FindSystem(string name, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            return FindSystem(new SystemClass(name), lookup);
        }

        // look up thru cache and db, and optionally edsm
        // thread safe
        public static ISystem FindSystem(ISystem find, WebExternalDataLookup lookup = WebExternalDataLookup.None)
        {
            ISystem found;

            if (SystemsDatabase.Instance.RebuildRunning) // Find the system in the cache if a rebuild is running
            {
                found = FindSystemInCacheDB(find, null);
            }
            else
            {
                found = SystemsDatabase.Instance.DBRead(conn => FindSystemInCacheDB(find, conn));

                // if not found, checking edsm, and its a good name
#if !TESTHARNESS
                if (found == null && lookup != WebExternalDataLookup.None && find.Name.HasChars() && find.Name != "UnKnown")
                {
                    if (lookup == WebExternalDataLookup.SpanshThenEDSM || lookup == WebExternalDataLookup.Spansh)
                    {
                        Spansh.SpanshClass sp = new Spansh.SpanshClass();       // proven 31 oct
                        found = sp.GetSystem(find.Name);
                    }

                    if (found == null && (lookup == WebExternalDataLookup.SpanshThenEDSM || lookup == WebExternalDataLookup.EDSM))
                    {
                        EDSM.EDSMClass edsm = new EDSM.EDSMClass();
                        found = edsm.GetSystem(find.Name)?.FirstOrDefault();     // this may return null, an empty list, so first or default, or it may return null
                    }

                    // if found one, and coords (paranoia), add back to our db so next time we have it

                    if (found != null && found.HasCoordinate)       // if its a good system
                    {
                        SystemsDatabase.Instance.StoreSystems(new List<ISystem> { found });     // won't do anything if rebuilding
                    }
                }
#endif
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

            // if we have a db and its okay, and either not found, or found but with no main star info and the db does have star type info, check the db
            if (cn != null && !SystemsDatabase.Instance.RebuildRunning && (found == null || (found.MainStarType == EDStar.Unknown && SystemsDatabase.Instance.HasStarType) ))
            {
                //System.Diagnostics.Debug.WriteLine("Look up from DB " + sys.name + " " + sys.id_edsm);

                bool findnameok = find.Name.HasChars() && find.Name != "UnKnown";
                ISystem dbfound = null;

                if (findnameok)            // if not found but has a good name
                {
                    var list = DB.SystemsDB.FindStars(find.Name, cn);   // find all by name, case insensitive

                    if (list.Count == 1 || (list.Count > 0 && !find.HasCoordinate))     // if we have 1 match only, or we have many matches, but no coord
                        dbfound = list[0];      // take the first entry
                }

                if (dbfound == null && find.HasCoordinate)        // finally, not found, but we have a co-ord, find it from the db  by distance
                    dbfound = DB.SystemsDB.GetSystemByPosition(find.X, find.Y, find.Z, cn);

                if (dbfound != null)                            // if we have a good db, go for it
                {
                    if (find.HasCoordinate)                     // if find has co-ordinate, it may be more up to date than the DB, so use it
                    {
                        dbfound.X = find.X; dbfound.Y = find.Y; dbfound.Z = find.Z;
                    }

                    lock (cachelockobject)          // lock to prevent multi change over these classes
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

            lock (cachelockobject)          // Rob seen instances of it being locked together in multiple star distance threads, we need to serialise access to these two dictionaries
            {                               // Concurrent dictionary no good, they could both be about to add the same thing at the same time and pass the contains test.
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

        // add system to cache - trying to improve information as we get the same system
        public static void AddSystemToCache(ISystem system)
        {
            var found = FindCachedSystem(system);

            // if not found, or found is synthesised or found has no star type, AND the system has coord and name, add as it may be better
            if ((found == null || found.Source == SystemSource.Synthesised || found.MainStarType == EDStar.Unknown) && system.HasCoordinate == true && system.Name.HasChars())
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
                    var list = DB.SystemsDB.FindStarsWildcard(name, cn, limit);
                    if (list != null)
                        systems.AddRange(list);
                });
            }

            return systems;
        }

        // system list with distances
        // may return slightly over maxitems, but only if systems were not in db but were in the cache due to the journal
        // if all systems are in the db, maxitems will be obeyed

        public static void GetSystemListBySqDistancesFrom(SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                                    int approxmaxitems,
                                                    double mindist, double maxdist, bool spherical,
                                                    HashSet<string> excludenames = null)
        {
            if (excludenames == null)
                excludenames = new HashSet<string>();       // empty set

            lock (cachelockobject)
            {
                if (spherical)
                {
                    var sphericalset = systemsByName.Values.SelectMany(s => s)
                                        .Where(sys => sys.DistanceSq(x, y, z) <= maxdist * maxdist && sys.DistanceSq(x, y, z) >= mindist * mindist && !excludenames.Contains(sys.Name))
                                        .Select(s => new { distsq = s.DistanceSq(x, y, z), sys = s })
                                        .ToList();

                    foreach (var s in sphericalset.Take(approxmaxitems))
                    {
                        distlist.Add(s.distsq, s.sys);      // add system and exclude for further use
                        excludenames.Add(s.sys.Name);
                    }
                }
                else
                {
                    var cubeset = systemsByName.Values.SelectMany(s => s)
                                .Where(sys => Math.Abs(sys.X - x) <= maxdist && Math.Abs(sys.Y - y) <= maxdist && Math.Abs(sys.Z - z) <= maxdist && sys.DistanceSq(x, y, z) >= mindist && !excludenames.Contains(sys.Name))
                                .Select(s => new { distsq = s.DistanceSq(x, y, z), sys = s })
                                .ToList();

                    foreach (var s in cubeset.Take(approxmaxitems))
                    {
                        distlist.Add(s.distsq, s.sys);
                        excludenames.Add(s.sys.Name);
                    }
                }
            }

            if (!SystemsDatabase.Instance.RebuildRunning) // Return from cache if rebuild is running
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    SystemsDB.GetSystemListBySqDistancesFrom(x, y, z, approxmaxitems, mindist, maxdist, spherical, cn,
                            (distsq, sys) =>
                            {
                                if (!excludenames.Contains(sys.Name))     // if not allowed or already there (if we run this twice, this should always trigger since they are all in the cache)
                            {
                                //System.Diagnostics.Debug.WriteLine($"Found from db {sys.Name}");
                                distlist.Add(distsq, sys);
                                    AddToCache(sys);
                                }
                            });
                });
            }
        }

#if !TESTHARNESS
        //// find nearest system, from cache, from db, from web (opt), and from gmo (opt)
        public static ISystem FindNearestSystemTo(double x, double y, double z, double maxdistance, 
                                        WebExternalDataLookup weblookup, GMO.GalacticMapping glist = null)
        {
            ISystem cachesys = null;
            ISystem dbsys = null;
            ISystem websys = null;
            GMO.GalacticMapObject glistobj = null;

            lock (cachelockobject)
            {
                cachesys = systemsByName.Values
                                    .SelectMany(s => s)
                                    .OrderBy(s => s.Distance(x, y, z))
                                    .FirstOrDefault(e => e.Distance(x, y, z) < maxdistance);        // find one in cache which matches, or null
            }

            if (!SystemsDatabase.Instance.RebuildRunning)
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    dbsys = DB.SystemsDB.GetSystemByPosition(x, y, z, cn, maxdistance);         // need to check the db as well, as it may have a closer one than the cache
                });
            }

            if (weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM)
            {
                Spansh.SpanshClass sp = new Spansh.SpanshClass();
                var res = sp.GetSphereSystems(x, y, z, maxdistance, 0);
                if (res?.Count > 0)
                {
                    websys = res[0].Item1;
                }
            }

            if (websys == null && (weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM))
            {
                EDSM.EDSMClass edsm = new EDSM.EDSMClass();
                var res = edsm.GetSphereSystems(x, y, z, maxdistance, 0);
                if (res?.Count > 0)
                {
                    websys = res[0].Item1;
                }
            }

            if ( glist != null )
            {
                glistobj = glist.FindNearest(x, y, z, maxdistance);
            }

            ISystem retsys = cachesys;

            // if we have a gobj with a star system, and either we don't have a result, or glist is closer..
            if (glistobj?.StarSystem != null && (retsys == null || glistobj.StarSystem.Distance(x, y, z) < retsys.Distance(x, y, z)))
            {
                retsys = glistobj.StarSystem;
            }

            if (dbsys != null && (retsys == null || dbsys.Distance(x, y, z) < retsys.Distance(x, y, z)))
            {
                retsys = dbsys;
            }

            if (websys != null && (retsys == null || websys.Distance(x, y, z) < retsys.Distance(x, y, z)))
            {
                retsys = websys;
            }

            return retsys;
        }
#endif
        // return system nearest to wantedpos, with ranges from curpos/wantedpos, with a route method
        // discard list is supported to knock out unwanted system by ID (EDSMID or System address)
        // used by the route plotter.
        public static ISystem GetSystemNearestTo(Point3D currentpos,
                                                 Point3D wantedpos,
                                                 double maxfromcurpos,
                                                 double maxfromwanted,
                                                 SystemsNearestMetric routemethod,
                                                 int limitto,
                                                 HashSet<long> discard)
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
                                 .Where(s => s.dw < maxfromwanted &&
                                             s.dc < maxfromcurpos &&
                                             (discard == null || (s.sys.EDSMID.HasValue ? !discard.Contains(s.sys.EDSMID.Value) : !discard.Contains(s.sys.SystemAddress ?? 0))))
                                 .OrderBy(s => s.dw)
                                 .Select(s => s.sys)
                                 .ToList();
            }

            if (!SystemsDatabase.Instance.RebuildRunning)    // return from cache is rebuild is running
            {
                SystemsDatabase.Instance.DBRead(cn =>
                {
                    DB.SystemsDB.GetSystemNearestTo(currentpos, wantedpos, maxfromcurpos, maxfromwanted, limitto, cn, (s) =>
                    {
                        AddToCache(s);
                        if (discard == null || (s.EDSMID.HasValue ? !discard.Contains(s.EDSMID.Value) : !discard.Contains(s.SystemAddress ?? 0)))
                        {
                            candidates.Add(s);
                        }
                        else
                            System.Diagnostics.Debug.WriteLine($"GetSystem discarded {s.SystemAddress} {s.Name}");
                    });
                });
            }

            return PickSystemFromCandidatesUsingMetric(candidates, currentpos, wantedpos, maxfromcurpos, maxfromwanted, routemethod);
        }

        public enum SystemsNearestMetric
        {
            IterativeNearestWaypoint,
            IterativeMinDevFromPath,
            IterativeMaximumDev100Ly,
            IterativeMaximumDev250Ly,
            IterativeMaximumDev500Ly,
            IterativeWaypointDevHalf,
        }

        internal static ISystem PickSystemFromCandidatesUsingMetric(IEnumerable<ISystem> systems,
                                                   Point3D currentpos,
                                                   Point3D wantedpos,
                                                   double maxfromcurpos,
                                                   double maxfromwanted,
                                                   SystemsNearestMetric routemethod)
        {
            double bestmindistance = double.MaxValue;
            ISystem nearestsystem = null;

            foreach (var s in systems)
            {
                Point3D syspos = new Point3D(s.X, s.Y, s.Z);
                double distancefromwantedx2 = Point3D.DistanceBetweenX2(wantedpos, syspos); // range between the wanted point and this, ^2
                double distancefromcurposx2 = Point3D.DistanceBetweenX2(currentpos, syspos);    // range between the wanted point and this, ^2

                // ENSURE its withing the circles now
                if (distancefromcurposx2 <= (maxfromcurpos * maxfromcurpos) && distancefromwantedx2 <= (maxfromwanted * maxfromwanted))
                {
                    if (routemethod == SystemsNearestMetric.IterativeNearestWaypoint)
                    {
                        if (distancefromwantedx2 < bestmindistance)
                        {
                            nearestsystem = s;
                            bestmindistance = distancefromwantedx2;
                        }
                    }
                    else
                    {
                        Point3D interceptpoint = currentpos.InterceptPoint(wantedpos, syspos);      // work out where the perp. intercept point is..
                        double deviation = Point3D.DistanceBetween(interceptpoint, syspos);
                        double metric = 1E39;

                        if (routemethod == SystemsNearestMetric.IterativeMinDevFromPath)
                            metric = deviation;
                        else if (routemethod == SystemsNearestMetric.IterativeMaximumDev100Ly)
                            metric = (deviation <= 100) ? distancefromwantedx2 : metric;        // no need to sqrt it..
                        else if (routemethod == SystemsNearestMetric.IterativeMaximumDev250Ly)
                            metric = (deviation <= 250) ? distancefromwantedx2 : metric;
                        else if (routemethod == SystemsNearestMetric.IterativeMaximumDev500Ly)
                            metric = (deviation <= 500) ? distancefromwantedx2 : metric;
                        else if (routemethod == SystemsNearestMetric.IterativeWaypointDevHalf)
                            metric = Math.Sqrt(distancefromwantedx2) + deviation / 2;
                        else
                            throw new ArgumentOutOfRangeException(nameof(routemethod));

                        if (metric < bestmindistance)
                        {
                            nearestsystem = s;
                            bestmindistance = metric;
                        }
                    }
                }
            }

            return nearestsystem;
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

        public static void ReturnSystemAdditionalListForAutoComplete(string input, Object ctrl, SortedSet<string> set)
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
                    List<ISystem> systems = DB.SystemsDB.FindStarsWildcard(input, MaximumStars);
                    foreach (var i in systems)
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
            lock (cachelockobject)
            {
               // System.Diagnostics.Debug.WriteLine($"SystemCache add {found.Name}");

                List<ISystem> byname;

                if (!systemsByName.TryGetValue(found.Name, out byname))
                {
                    systemsByName[found.Name] = byname = new List<ISystem>();
                    //System.Diagnostics.Debug.WriteLine($"Added to cache {found.Name}");
                }

                int idx = byname.FindIndex(e => e.Xi == found.Xi && e.Yi == found.Yi && e.Zi == found.Zi);

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

        private static Dictionary<string, List<ISystem>> systemsByName = new Dictionary<string, List<ISystem>>(StringComparer.InvariantCultureIgnoreCase);

        private static List<string> AutoCompleteAdditionalList = new List<string>();

#endregion
    }

}

