/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{

    public partial class StarScan
    {
        // check system address, then check name

        private SystemNode GetOrCreateSystemNode(ISystem sys)
        {
            if (sys.SystemAddress.HasValue && ScanDataBySysaddr.TryGetValue(sys.SystemAddress.Value, out SystemNode sn))
                return sn;

            if (ScanDataByName.TryGetValue(sys.Name, out sn))            // try name, it may have been stored with an old entry without sys address 
            {
                if ( sys.SystemAddress.HasValue)
                {
                    sn.System = sys;            // refresh our node knowledge of the system
                    ScanDataBySysaddr[sys.SystemAddress.Value] = sn;    // previously without a known system address, now we have it, assign
                 //   System.Diagnostics.Debug.WriteLine($"StarScan {sys.Name} now has address {sys.SystemAddress}" );
                }

                return sn;                                              
            }

            // not found, make a new node
            sn = new SystemNode(sys);

            // if it has a system address, we store it to the list that way. Else we add to name list
            if (sys.SystemAddress.HasValue)
            {
                ScanDataBySysaddr[sys.SystemAddress.Value] = sn;
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"StarScan {sys.Name} no address");
            }

            ScanDataByName[sys.Name] = sn;

            return sn;
        }

        public bool NameButNoAddress(string name, long sysaddr )
        {
            return ScanDataByName.TryGetValue(name, out StarScan.SystemNode v1) && !ScanDataBySysaddr.TryGetValue(sysaddr, out StarScan.SystemNode v2);
        }

        private SystemNode FindSystemNode(ISystem sys)
        {
            if (sys.SystemAddress.HasValue && ScanDataBySysaddr.TryGetValue(sys.SystemAddress.Value, out SystemNode sn))
                return sn;

            if (ScanDataByName.TryGetValue(sys.Name, out sn))            // try name, it may have been stored with an old entry without sys address 
                return sn;                                                          // so we check to see if we already have that first

            return null;
        }

        // bodyid can be null, bodyname must be set.
        // scan the history and try and find the best star system this bodyname is associated with
        // find best system for scan.  Note startindex might be -1, if only 1 entry is in the list

        private static Tuple<string, ISystem> FindBestSystem(int startindex, List<HistoryEntry> hl, string bodyname, int? bodyid, bool isstar )
        {
            System.Diagnostics.Debug.Assert(bodyname != null);

            for (int j = startindex; j >= 0; j--)
            {
                HistoryEntry he = hl[j];

                if (he.IsLocOrJump)
                {
                    JournalLocOrJump jl = (JournalLocOrJump)he.journalEntry;
                    string designation = BodyDesignations.GetBodyDesignation(bodyname, bodyid, isstar, he.System.Name);

                    if (IsStarNameRelated(he.System.Name, designation))       // if its part of the name, use it
                    {
                        return new Tuple<string, ISystem>(designation, he.System);
                    }
                    else if (jl != null && IsStarNameRelated(jl.StarSystem, designation))
                    {
                        // Ignore scans where the system name has changed
                        System.Diagnostics.Trace.WriteLine($"Rejecting body {designation} ({bodyname}) in system {he.System.Name} => {jl.StarSystem} due to system rename");
                        return null;
                    }
                }
            }

            startindex = Math.Max(0, startindex);       // if only 1 in list, startindex will be -1, so just pick first one

            return new Tuple<string, ISystem>(BodyDesignations.GetBodyDesignation(bodyname, bodyid, false, hl[startindex].System.Name), hl[startindex].System);
        }

        private static bool IsStarNameRelated(string starname, string bodyname )
        {
            if (bodyname.Length >= starname.Length)
            {
                string s = bodyname.Substring(0, starname.Length);
                return starname.Equals(s, StringComparison.InvariantCultureIgnoreCase);
            }
            else
                return false;
        }

        public static string IsStarNameRelatedReturnRest(string starname, string bodyname, string designation )          // null if not related, else rest of string
        {
            if (designation == null)
            {
                designation = bodyname;
            }

            if (designation.Length >= starname.Length)
            {
                string s = designation.Substring(0, starname.Length);
                if (starname.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    return designation.Substring(starname.Length).Trim();
            }

            return null;
        }


    }
}
