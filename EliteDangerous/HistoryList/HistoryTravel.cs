/*
 * Copyright © 2016 - 2023 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Linq;
using EliteDangerousCore.JournalEvents;

namespace EliteDangerousCore
{
    public class HistoryTravelStatus
    {
        public class Jump
        {
            public DateTime JumptimeUTC;
            public double Distance;
            public int Index;
        };

        public DateTime TravelStartTimeUTC { get; private set; }           // MinDate for not
        public List<Jump> TravelJumps { get; private set; } = new List<Jump>();
        public int TravelledMissingjump { get; private set; }
        public bool IsTravelling { get { return TravelStartTimeUTC != DateTime.MinValue; } }

        public int TravelledJumps(int heindex)          // 0 if before first jump, else count to date
        {
            if (IsTravelling)
            {
                int i = IndexOf(heindex);               // will return -1 if before first jump, or 0 for first jump, etc.
                return i + 1;
            }
            else
                return 0;
        }
        public double TravelledDistance(int heindex)
        {
            if (IsTravelling)
            {
                int index = IndexOf(heindex);           // will return -1 if before first jump, or 0 for first jump, etc.
                return index < 0 ? 0 : TravelJumps[index].Distance;     // <0, 0. else travel distance at index, unless index == length, in which case last
            }
            else
                return 0;
        }
        public string Stats(int heindex, DateTime hetime, string prefix = "")
        {
            if (IsTravelling)
            {
                return prefix + $"{TravelledDistance(heindex):0.0} ly, J {TravelledJumps(heindex)}, \u0394" + (hetime - TravelStartTimeUTC).ToString(@"d\:hh\:mm\:ss");
            }
            else
                return "";
        }

        public HistoryTravelStatus()
        {
            TravelStartTimeUTC = DateTime.MinValue;
        }

        // previous and heprev can be null, hecur is always set

        public static HistoryTravelStatus Update(HistoryTravelStatus state, HistoryEntry heprev, HistoryEntry hecur)
        {
            if (hecur.StartMarker || hecur.StopMarker)
                System.Diagnostics.Debug.WriteLine($"Travelling {hecur.EventTimeUTC} start {hecur.StartMarker} stop {hecur.StopMarker}");

            if (state == null || (heprev?.StopMarker ?? false))       // if prev is null (start of list) OR previous ordered a stop, new list
            {
                state = new HistoryTravelStatus();       // not travelling
            }

            if (state.IsTravelling)                      // if we are travelling
            {
                if (!hecur.MultiPlayer && hecur.journalEntry is JournalFSDJump) // if not multiplayer, and fsd..
                {
                    double dist = ((JournalFSDJump)hecur.journalEntry).JumpDist;

                    if (dist <= 0)      // if not found
                    {
                        state.TravelledMissingjump++;       // mark missing distance..
                        dist = 0;
                    }

                    state.TravelJumps.Add( new Jump() { Distance = (state.TravelJumps.Count > 0 ? state.TravelJumps.Last().Distance : 0) + dist, 
                                                        JumptimeUTC = hecur.EventTimeUTC,
                                                        Index = hecur.Index});
                }
            }
            else if ( hecur.StartMarker )               // else if we started..
            {
                state = new HistoryTravelStatus();      // a fresh entry 
                state.TravelStartTimeUTC = hecur.EventTimeUTC; // set start time
            }

            return state;
        }

        private int IndexOf(int heindex)                 // -1 if before first jump, else points to last jump 
        {
            for (int i = 0; i < TravelJumps.Count; i++)
            {
                if (TravelJumps[i].Index > heindex)
                    return i - 1;
            }
            return TravelJumps.Count-1;
        }

    }
}
