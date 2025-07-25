﻿/*
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
using QuickJSON;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.NavRoute)]
    public class JournalNavRoute : JournalEntry, IAdditionalFiles, IStarScan
    {
        public JournalNavRoute(JObject evt) : base(evt, JournalTypeEnum.NavRoute)
        {
            Rescan(evt);
        }

        private void Rescan(JObject evt)
        {
            var route = evt["Route"] as JArray;

            if (route != null)
            {
                var routeents = new List<NavRouteEntry>();

                foreach (JObject jo in route)
                {
                    var starsys = jo["StarSystem"];         // beta: address, 3.7 : StarSystem 
                    var sysaddr = jo["SystemAddress"];      // beta: not present, 3.7 address
                    var starpos = new EMK.LightGeometry.Vector3(
                        jo["StarPos"][0].Float(),
                        jo["StarPos"][1].Float(),
                        jo["StarPos"][2].Float()
                    );
                    var starclass = jo["StarClass"].Str();

                    if (sysaddr == null)                    // if no SystemAddress, its beta
                    {
                        routeents.Add(new NavRouteEntry
                        {
                            SystemAddress = starsys.Long(), // yes the beta had it in there
                            StarPos = starpos,
                            StarClass = starclass
                        });
                    }
                    else
                    {
                        var sedsc = Stars.ToEnum(starclass);

                        routeents.Add(new NavRouteEntry     // 3.7 will have this
                        {
                            StarSystem = starsys.Str(),
                            SystemAddress = sysaddr.Long(),
                            StarPos = starpos,
                            StarClass = starclass,
                            EDStarClass = sedsc
                        }) ;

                        SystemClass s = new SystemClass(routeents.Last().StarSystem, sysaddr.Long(), starpos.X, starpos.Y, starpos.Z, SystemSource.FromJournal, sedsc);
                        SystemCache.AddSystemToCache(s);     // inform cache of this known system
                    }
                }

                Route = routeents.ToArray();
            }
        }

        public NavRouteEntry[] Route { get; set; }      // check route is not null

        public void ReadAdditionalFiles(string directory)
        {
            if (Route == null)
            {
                JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "NavRoute.json"), EventTypeStr);  // check timestamp..
                if (jnew != null)        // new json, rescan. returns null if cargo in the folder is not related to this entry by time.
                {
                    Rescan(jnew);
                    UpdateJson(jnew);
                }
            }
        }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (Route != null)
            {
                sb.AppendFormat("{0} jumps: ".T(EDCTx.JournalNavRoute_Jumps), Route.Length - 1);

                for (int i = 1; i < Route.Length; i++)
                {
                    var r = Route[i];
                    string n = r.StarSystem ?? r.SystemAddress.ToStringInvariant();     // star system has been seen to be empty

                    if (i == 1)         // first one, just the system, no append
                    {
                        sb.Append(n);
                    }
                    else if (Route.Length >= 18)       // if route is long printed, 15 = .. , -2/-1 printed
                    {
                        if (i < 15 || i >= Route.Length - 2)
                        {
                            sb.AppendCS();
                            sb.Append(n);
                        }
                        else if (i == 15)
                            sb.Append(", .. ");
                    }
                    else
                    {
                        sb.AppendCS();
                        sb.Append(n);
                    }
                }

                return sb.ToString();
            }
            else
                return null;
        }


        public override string GetDetailed()
        {
            if (Route != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for (int i = 1; i < Route.Length; i++)
                {
                    var r = Route[i];
                    string n = r.StarSystem ?? r.SystemAddress.ToStringInvariant();     // star system has been seen to be empty
                    sb.AppendPrePad(n + " @ " + r.StarPos.X.ToString("N1") + "," + r.StarPos.Y.ToString("N1") + "," + r.StarPos.Z.ToString("N1") + " " + r.StarClass, System.Environment.NewLine);
                }
                return sb.ToString();
            }
            else
                return null;

        }

        public bool Equals(JournalNavRoute other)
        {
            if (Route != null && other.Route != null && other.Route.Length == Route.Length)
            {
                for (int i = 0; i < Route.Length; i++)
                {
                    if (Route[i].StarSystem != other.Route[i].StarSystem || Route[i].SystemAddress != other.Route[i].SystemAddress)
                        return false;
                }

                return true;
            }

            return false;
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            foreach( var star in Route.EmptyIfNull())
            {
                s.AddLocation(new SystemClass(star.StarSystem, star.SystemAddress, star.StarPos.X, star.StarPos.Y, star.StarPos.Z));     // we use our data to fill in 
            }
        }

        public class NavRouteEntry
        {
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }
            public EMK.LightGeometry.Vector3 StarPos { get; set; }
            public string StarClass { get; set; }
            public EDStar EDStarClass { get; set; }
        }
    }

    [JournalEntryType(JournalTypeEnum.NavRouteClear)]
    public class JournalNavRouteClear : JournalEntry
    {
        public JournalNavRouteClear(JObject evt) : base(evt, JournalTypeEnum.NavRouteClear)
        {
        }
    }
}
