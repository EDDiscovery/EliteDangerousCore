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
            public DateTime JumptimeUTC;        // jump time
            public double Distance;             
            public TimeSpan TravelTime = new TimeSpan();        // accumulated time to this point
        };

        public bool IsTravelling { get { return TravelStartTimeUTC != DateTime.MinValue; } }
        public DateTime TravelStartTimeUTC { get; private set; }           // MinDate for not travelling
        public DateTime TravelStopTimeUTC { get; private set; }            // MinDate until stopped
        public List<Jump> TravelJumps { get; private set; } = new List<Jump>();     // jump list
        public int TravelledMissingjump { get; private set; }               // any missing positions
        public TimeSpan TimeAccumulator { get; private set; } = new TimeSpan();     // time accumulated on this state, added to on each shutdown and final stop - total flight time of this state

        public int TravelledJumps(DateTime heutctime)          // 0 if before first jump, else count to date
        {
            if (IsTravelling)
            {
                int i = IndexOf(heutctime);               // will return -1 if before first jump, or 0 for first jump, etc.
                return i + 1;
            }
            else
                return 0;
        }
        public double TravelledDistance(DateTime heutctime)
        {
            if (IsTravelling)
            {
                int index = IndexOf(heutctime);           // will return -1 if before first jump, or 0 for first jump, etc.
                return index < 0 ? 0 : TravelJumps[index].Distance;     // <0, 0. else travel distance at index, unless index == length, in which case last
            }
            else
                return 0;
        }
        public TimeSpan TravelledTime(DateTime heutctime)
        {
            if (IsTravelling)
            {
                int index = IndexOf(heutctime);           // will return -1 if before first jump, or 0 for first jump, etc.
                if ( index >= 0 )
                    return TravelJumps[index].TravelTime;     
            }
            return new TimeSpan(0);
        }
        public string Stats(DateTime hetimeutc)
        {
            int index = IndexOf(hetimeutc);           // will return -1 if before first jump, or 0 for first jump, etc.

            if (index>=0)                            // if we have a jump..
            {
                TimeSpan ts = TravelJumps[index].TravelTime;     // travel time to last one

                if (index == TravelJumps.Count - 1)             // if at last entry, so we may be beyond in time this..
                {
                    if (hetimeutc > TravelJumps[index].JumptimeUTC)      // if past the last jump, then use the time accumulator, not the time of the last jump
                    {
                        ts = TimeAccumulator;                    
                    }
                }

                return $"{TravelJumps[index].Distance:0.0} ly, J {index+1}, \u0394" + (hetimeutc - TravelStartTimeUTC).ToString(@"d\:hh\:mm\:ss") +
                    " T\u0394" + ts.ToString(@"d\:hh\:mm\:ss");
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
            if (state == null || heprev == null)        // if these, knock out rest of tests, and reset the state
            {
                state = new HistoryTravelStatus();       // new one needed
            }
            else if (heprev.StopMarker)                 // previous was a stop
            {
                if (state.IsTravelling)                 // if we were travelling..
                {
                    if (state.LastLoadGameTime != DateTime.MinValue)  // and we not in a shutdown..
                    {
                        var delta = heprev.EventTimeUTC - state.LastLoadGameTime;
                        state.TimeAccumulator += delta;           // okay, the time between last load game and this stop is added to the accumulator to give the final time
                    }

                    state.TravelStopTimeUTC = heprev.EventTimeUTC;  // and we note the stop time

                    //System.Diagnostics.Debug.WriteLine($"Travel stopmarker {heprev.EventTimeUTC} {heprev.Index} {hecur.journalEntry.FileName} {state.TravelStartTimeUTC} -> {state.TravelStopTimeUTC} Total time {state.TimeAccumulator} -> {state.Stats(heprev.Index,heprev.EventTimeUTC)}");

                    state = new HistoryTravelStatus();       // new one needed
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"Travel stopmarker {heprev.EventTimeUTC} {heprev.Index} {hecur.journalEntry.FileName} ignored as not travelling");
                }
            }

            if (state.IsTravelling)                      // if we are travelling
            {
                if ( hecur.EntryType == JournalTypeEnum.Shutdown)       // if we have a shutdown
                {
                    if (state.LastLoadGameTime != DateTime.MinValue)    // ignore if already in shutdown, otherwise we are running, so 
                    {
                        var delta = hecur.EventTimeUTC - state.LastLoadGameTime;
                        state.TimeAccumulator += delta;  // the time between last load game and this shutdown is added to the accumulator
                        state.LastLoadGameTime = DateTime.MinValue;             // say we have ended.. next loadgame will set it
                        //System.Diagnostics.Debug.WriteLine($"Travel shutdown {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName} for {state.TravelStartTimeUTC} delta {delta} accumulated {state.TimeAccumulator}");
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"Travel shutdown {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName} for {state.TravelStartTimeUTC} while shutdown");
                    }
                }
                else if ( hecur.EntryType == JournalTypeEnum.LoadGame)  // when we have a loadgame
                {
                    if (state.LastLoadGameTime == DateTime.MinValue)    // if in a shutdown..
                    {
                        //System.Diagnostics.Debug.WriteLine($"Travel loadgame {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName} for {state.TravelStartTimeUTC} set last load game");
                        state.LastLoadGameTime = hecur.EventTimeUTC;    // the last load game timer
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"Travel loadgame {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName} for {state.TravelStartTimeUTC} ignored - multiple loadgames before shutdown");
                    }
                }

                if (!hecur.Status.IsInMultiPlayer && hecur.journalEntry is JournalFSDJump) // if not multiplayer, and fsd..
                {
                    double dist = ((JournalFSDJump)hecur.journalEntry).JumpDist;

                    if (dist <= 0)      // if not found
                    {
                        state.TravelledMissingjump++;       // mark missing distance..
                        dist = 0;
                    }

                    var jmp = new Jump()
                    {
                        Distance = (state.TravelJumps.Count > 0 ? state.TravelJumps.Last().Distance : 0) + dist,
                        JumptimeUTC = hecur.EventTimeUTC,
                        // work out the accumulated time, as long as the last load game time is set (it should be) we can add that on
                        TravelTime = state.TimeAccumulator + (state.LastLoadGameTime!=DateTime.MinValue ? (hecur.EventTimeUTC-state.LastLoadGameTime) : new TimeSpan(0)),
                    };

                    state.TravelJumps.Add(jmp);

                    //System.Diagnostics.Debug.WriteLine($"Travel fsd jump {jmp.JumptimeUTC} {jmp.Index} {jmp.Distance} {jmp.TravelTime} -> {state.Stats(hecur.Index, hecur.EventTimeUTC)}");
                }

                if ( hecur.StartMarker )
                {
                    //System.Diagnostics.Debug.WriteLine($"Travel startmarker {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName} ignored as multiple starts");
                }
            }
            else if ( hecur.StartMarker )               // not travelling, and start marker
            {
                //System.Diagnostics.Debug.WriteLine($"\r\nTravel startmarker {hecur.EventTimeUTC} {hecur.Index} {hecur.journalEntry.FileName}");
                state = new HistoryTravelStatus();      // a fresh entry 
                
                state.LastLoadGameTime = 
                state.TravelStartTimeUTC = hecur.EventTimeUTC; // set start time, and load game time,  to the event..
            }

            return state;
        }

        private int IndexOf(DateTime hetimeutc)                 // -1 if before first jump, else points to array index of last jump 
        {
            for (int i = 0; i < TravelJumps.Count; i++)
            {
                if (TravelJumps[i].JumptimeUTC > hetimeutc)     // if jump time is greater than hetime, we have got the last jump
                    return i - 1;                               // -1 if before, else index of last jump
            }

            return TravelJumps.Count-1;                         // hetimeutc is beyond all jumps, return last
        }

        private DateTime LastLoadGameTime = DateTime.MinValue;      // min value means we are in a shutdown
    }
}
