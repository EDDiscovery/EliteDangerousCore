﻿/*
 * Copyright © 2019-2023 EDDiscovery development team
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
using EliteDangerousCore.EDSM;
using EliteDangerousCore.Spansh;
using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class RoutePlotter
    {
        public bool StopPlotter { get; set; } = false;
        public float MaxRange;
        public Point3D Coordsfrom;
        public Point3D Coordsto;
        public string FromSystem;
        public string ToSystem;
        public SystemCache.SystemsNearestMetric RouteMethod;
        public bool UseFsdBoost;
        public WebExternalDataLookup WebLookup;
        public HashSet<long> DiscardList;

        public class ReturnInfo
        {
            public string name;             // always
            public double dist;             // always
            public Point3D pos;             // 3dpos can be null
            public double waypointdist;     // can be Nan
            public double deviation;        // can be Nan
            public ISystem system;          // only if its a real system

            public ReturnInfo(string s, double d, Point3D p = null, double way = double.NaN, double dev = double.NaN, ISystem sys = null)
            {
                name = s;dist = d;pos = p;waypointdist = way;deviation = dev; system = sys;
            }
        }

        public List<ISystem> RouteIterative(Action<ReturnInfo> info)
        {
            double traveldistance = Point3D.DistanceBetween(Coordsfrom, Coordsto);      // its based on a percentage of the traveldistance
            List<ISystem> routeSystems = new List<ISystem>();
            System.Diagnostics.Debug.WriteLine("From " + FromSystem + " to  " + ToSystem + ", using metric " + RouteMethod.ToString());

            ISystem startsystem = new SystemClass(FromSystem, null, Coordsfrom.X, Coordsfrom.Y, Coordsfrom.Z);
            ISystem startfromdb = SystemCache.FindSystem(startsystem, WebLookup); // see if the cache knows more about it, if so, use that..
            if (startfromdb != null)
                startsystem = startfromdb;
            routeSystems.Add(startsystem);

            info(new ReturnInfo(FromSystem, double.NaN, Coordsfrom,double.NaN,double.NaN,routeSystems[0]));

            Point3D curpos = Coordsfrom;
            int jump = 1;
            double actualdistance = 0;

            float maxfromwanted = (MaxRange<100) ? (MaxRange-1) : (100+MaxRange * 1 / 5);       // if <100, then just make sure we jump off by 1 yr, else its a 100+1/5
            maxfromwanted = Math.Min(maxfromwanted, MaxRange - 1);

            do
            {
                double distancetogo = Point3D.DistanceBetween(Coordsto, curpos);      // to go

                if (distancetogo <= MaxRange)                                         // within distance, we can go directly
                    break;

                Point3D travelvector = new Point3D(Coordsto.X - curpos.X, Coordsto.Y - curpos.Y, Coordsto.Z - curpos.Z); // vector to destination
                Point3D travelvectorperly = new Point3D(travelvector.X / distancetogo, travelvector.Y / distancetogo, travelvector.Z / distancetogo); // per ly travel vector
                Point3D expectedNextPosition = GetNextPosition(curpos, travelvectorperly, MaxRange);    // where we would like to be..

                System.Diagnostics.Debug.WriteLine($"\n{BaseUtils.AppTicks.MSd} Route Plotter Query {curpos} -> {expectedNextPosition}");

                ISystem bestsystem = GetBestJumpSystem(curpos, travelvectorperly, maxfromwanted, MaxRange);    // see if we can find a system near  our target

                if (bestsystem == null)
                {
                    if (WebLookup != EliteDangerousCore.WebExternalDataLookup.None)
                    {
                        System.Diagnostics.Debug.WriteLine($" .. did not find system, try web ");
                        bestsystem = GetBestWebSystem(curpos, travelvectorperly, maxfromwanted, MaxRange, WebLookup);
                        System.Diagnostics.Debug.WriteLine($" .. web returned `{bestsystem?.Name}` ");
                    }
                    else
                        System.Diagnostics.Debug.WriteLine($" .. did not find system, no web");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.MSd} .. found in EDD {bestsystem}");
                }

                // if we haven't found a system in range, let's try boosting
                int boostStrength = 0;
                while (UseFsdBoost && bestsystem == null && boostStrength < 4)
                {
                    boostStrength = 1 << boostStrength;
                    float maxRangeWithBoost = MaxRange * (1.0f + BoostPercentage(boostStrength));
                    float maxfromWantedWithBoost = maxfromwanted * (1.0f + BoostPercentage(boostStrength));
                    System.Diagnostics.Debug.WriteLine($" .. Try boost {boostStrength}  {BoostPercentage(boostStrength)} maxrange {maxRangeWithBoost} maxwanted {maxfromWantedWithBoost}");

                    ISystem bestSystemWithBoost = GetBestJumpSystem(curpos, travelvectorperly, maxfromWantedWithBoost, maxRangeWithBoost);
                    
                    if ( bestSystemWithBoost == null && WebLookup != WebExternalDataLookup.None)
                        bestSystemWithBoost = GetBestWebSystem(curpos, travelvectorperly, maxfromWantedWithBoost, maxRangeWithBoost, WebLookup);

                    if (bestSystemWithBoost != null)
                        bestsystem = bestSystemWithBoost;
                }

                Point3D nextpos = expectedNextPosition;    // where we really are going to be
                string sysname = "WAYPOINT";
                double deltafromwaypoint = 0;
                double deviation = 0;

                if (bestsystem != null)
                {
                    nextpos = new Point3D(bestsystem.X, bestsystem.Y, bestsystem.Z);
                    deltafromwaypoint = Point3D.DistanceBetween(nextpos, expectedNextPosition);     // how much in error
                    deviation = Point3D.DistanceBetween(curpos.InterceptPoint(expectedNextPosition, nextpos), nextpos);
                    sysname = bestsystem.Name;
                    string tag = "Dev: " + deviation.ToString("N1") +"ly";
                    if (boostStrength > 0)
                        tag += " " + Environment.NewLine + "Boost: " + BoostPercentage(boostStrength) * 100 + "%";      // space on purpose in case word wrap in table not on
                    bestsystem.Tag = tag;
                    routeSystems.Add(bestsystem);
                }

                info(new ReturnInfo(sysname, Point3D.DistanceBetween(curpos, nextpos), nextpos, deltafromwaypoint, deviation , bestsystem));

                actualdistance += Point3D.DistanceBetween(curpos, nextpos);
                curpos = nextpos;
                jump++;

            } while ( !StopPlotter);

            ISystem endsystem = new SystemClass(ToSystem, null, Coordsto.X, Coordsto.Y, Coordsto.Z);
            ISystem endfromdb = SystemCache.FindSystem(endsystem, WebLookup); // see if the cache knows more about it, if so, use that..
            if (endfromdb != null)
                endsystem = endfromdb;
            routeSystems.Add(endsystem);

            actualdistance += Point3D.DistanceBetween(curpos, Coordsto);

            info(new ReturnInfo(ToSystem, Point3D.DistanceBetween(curpos, Coordsto), Coordsto, double.NaN, double.NaN, routeSystems.Last()));

            info(new ReturnInfo("Straight Line Distance", traveldistance));
            info(new ReturnInfo("Travelled Distance", actualdistance));

            return routeSystems;
        }

        private ISystem GetBestJumpSystem(Point3D currentPosition, Point3D travelVectorPerLy, float maxDistanceFromWanted, float maxRange)
        {
            Point3D nextPosition = GetNextPosition(currentPosition, travelVectorPerLy, maxRange);
            System.Diagnostics.Debug.WriteLine($".. Get best jump position around {nextPosition} dist from cur {nextPosition.Distance(currentPosition)} at maxrange {maxRange} wanted distance {maxDistanceFromWanted}");
            ISystem bestSystem = SystemCache.GetSystemNearestTo(currentPosition, nextPosition, maxRange, maxDistanceFromWanted, RouteMethod, 1000, DiscardList);  // at least get 1/4 way there, otherwise waypoint.  Best 1000 from waypoint checked
            return bestSystem;
        }

        private static Point3D GetNextPosition(Point3D currentPosition, Point3D travelVectorPerLy, float maxRange)
        {
            return new Point3D(currentPosition.X + maxRange * travelVectorPerLy.X,
                currentPosition.Y + maxRange * travelVectorPerLy.Y,
                currentPosition.Z + maxRange * travelVectorPerLy.Z); // where we would like to be..
        }

        static BaseUtils.MSTicks ratelimiter = new BaseUtils.MSTicks();

        // return an EDSM ISystem or null based on parameters
        private static ISystem GetBestWebSystem( Point3D currentPosition, Point3D travelVectorPerLy, float maxDistanceFromWanted, float maxRange, WebExternalDataLookup weblookup)
        {
            int maxqueriesratems = weblookup == WebExternalDataLookup.EDSM ? 5000 : 200;

            if (ratelimiter.IsRunning)      // first time, won't be running
            {
                uint timerunning = ratelimiter.TimeRunning; // how long since ran?

                if (timerunning < maxqueriesratems)       // if it is less than query rate, pause
                {
                    int delay = maxqueriesratems - (int)timerunning;
                    System.Diagnostics.Debug.WriteLine($"{BaseUtils.AppTicks.MSd} Rate limit queries by pausing for {delay}");
                    System.Threading.Thread.Sleep(delay);
                }
            }

            ratelimiter.Run();      // mark the start

            Point3D next = GetNextPosition(currentPosition, travelVectorPerLy, maxRange);
            Point3D centrepos = GetNextPosition(currentPosition, travelVectorPerLy, maxRange - maxDistanceFromWanted / 2);        // centre of sphere is made here, at maxdistance-maxwanted/2
            System.Diagnostics.Debug.WriteLine($"Route Finder WebLookup next pos wanted {next} dist {next.Distance(currentPosition)} centrepos {centrepos} dist {centrepos.Distance(currentPosition)}");

            if (weblookup == WebExternalDataLookup.Spansh || weblookup == WebExternalDataLookup.SpanshThenEDSM)
            {
                SpanshClass spansh = new SpanshClass();     
                var response = spansh.GetSphereSystems(centrepos.X, centrepos.Y, centrepos.Z, maxDistanceFromWanted, 0);        // checked nov 1st 2023

                if (response != null) // it did reply. May not due to limiter or general internet stuffy
                {
                    foreach (var s in response) System.Diagnostics.Debug.WriteLine($".. {s.Item1.Name} from next {s.Item1.Distance(next.X, next.Y, next.Z)} from curpos {s.Item1.Distance(currentPosition.X, currentPosition.Y, currentPosition.Z)} ");

                    var list = response
                                    // ensure its not too far from current.. don't trust 
                                    .Where(x => (x.Item1.X - currentPosition.X) * (x.Item1.X - currentPosition.X) + (x.Item1.Y - currentPosition.Y) * (x.Item1.Y - currentPosition.Y) + (x.Item1.Z - currentPosition.Z) * (x.Item1.Z - currentPosition.Z) <= maxRange * maxRange)
                                    // and not too far from wanted
                                    .Where(x => (x.Item1.X - next.X) * (x.Item1.X - next.X) + (x.Item1.Y - next.Y) * (x.Item1.Y - next.Y) + (x.Item1.Z - next.Z) * (x.Item1.Z - next.Z) <= maxDistanceFromWanted * maxDistanceFromWanted)
                                    // order by distance from next ascending
                                    .OrderBy(x => (x.Item1.X - next.X) * (x.Item1.X - next.X) + (x.Item1.Y - next.Y) * (x.Item1.Y - next.Y) + (x.Item1.Z - next.Z) * (x.Item1.Z - next.Z)).
                                    ToList();

                    return list.Count > 0 ? list[0].Item1 : null;
                }
            }

            if (weblookup == WebExternalDataLookup.EDSM || weblookup == WebExternalDataLookup.SpanshThenEDSM)
            {
                EDSMClass edsm = new EDSMClass();
                var edsmresponse = edsm.GetSphereSystems(centrepos.X, centrepos.Y, centrepos.Z, maxDistanceFromWanted, 0);

                if (edsmresponse != null) // it did reply. May not due to limiter or general internet stuffy
                {
                    var list = edsmresponse
                                    // ensure its not too far from current.. don't trust 
                                    .Where(x => (x.Item1.X - currentPosition.X) * (x.Item1.X - currentPosition.X) + (x.Item1.Y - currentPosition.Y) * (x.Item1.Y - currentPosition.Y) + (x.Item1.Z - currentPosition.Z) * (x.Item1.Z - currentPosition.Z) <= maxRange * maxRange)
                                    // and not too far from wanted
                                    .Where(x => (x.Item1.X - next.X) * (x.Item1.X - next.X) + (x.Item1.Y - next.Y) * (x.Item1.Y - next.Y) + (x.Item1.Z - next.Z) * (x.Item1.Z - next.Z) <= maxDistanceFromWanted * maxDistanceFromWanted)
                                    // order by distance from next ascending
                                    .OrderBy(x => (x.Item1.X - next.X) * (x.Item1.X - next.X) + (x.Item1.Y - next.Y) * (x.Item1.Y - next.Y) + (x.Item1.Z - next.Z) * (x.Item1.Z - next.Z)).
                                    ToList();

    
                    return list.Count > 0 ? list[0].Item1 : null;
                }
            }

            return null;
        }

        private static float BoostPercentage(int boostStrength)
        {
            return boostStrength / 4.0f;
        }
    }

}
