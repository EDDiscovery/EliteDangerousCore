/*
 * Copyright © 2016 EDDiscovery development team
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
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("Mission {Mission.Name} {State} {DestinationSystemStation()}")]
    public class MissionState
    {
        public enum StateTypes { InProgress, Completed, Abandoned, Failed, Died };    

        public JournalMissionAccepted Mission { get; private set; }                  // never null
        public JournalMissionCompleted Completed { get; private set; }               // null until completed properly, may be complete but with this missing
        public JournalMissionRedirected Redirected { get; private set; }             // null unless redirected
        public JournalCargoDepot CargoDepot { get; private set; }                    // null unless we received a CD on this mission

        public StateTypes State { get; private set; }
        public DateTime MissionEndTime { get; private set; }  // on Accepted, Expiry time, then actual finish time on Completed/Abandoned/Failed
        private ISystem sys;                                      // where it was found
        private string body;                                        // and body

        public bool InProgress { get { return (State == StateTypes.InProgress); } }
        public bool InProgressDateTime(DateTime compare) { return InProgress && DateTime.Compare(compare, Mission.Expiry)<0; }
        public ulong Id { get { return Mission.MissionId; } }                         // id of entry
        public string OriginatingSystem { get { return sys.Name; } }
        public string OriginatingStation { get { return body; } }

        public string DestinationSystemStation()        // allowing for redirection
        {
            return (Redirected != null) ? ("->" + Redirected.NewDestinationSystem.AppendPrePad(Redirected.NewDestinationStation, ":")) : Mission.DestinationSystem.AppendPrePad(Mission.DestinationStation, ":");
        }

        public string MissionInfoColumn()            // Missions panel uses this for info column
        {
            string info = Mission.MissionInfoColumn();

            if (CargoDepot != null)
            {
                info += Environment.NewLine + BaseUtils.FieldBuilder.Build("To Go:".T(EDTx.MissionState_ToGo), CargoDepot.ItemsToGo, "Progress:;%;N1".T(EDTx.MissionState_Progress), CargoDepot.ProgressPercent);
            }
            if (Completed != null)
            {
                info += Environment.NewLine + Completed.RewardInformation(true);
            }

            info = info.ReplaceIfEndsWith(Environment.NewLine, "");

            return info;
        }

        public string DLLInfo()    // DLL Uses this for mission info
        {
            string s = Mission.MissionBasicInfo(false) + "," + Mission.MissionDetailedInfo(false) + ", State: " + State.ToString();
            if (Completed != null)
                s += Environment.NewLine + Completed.RewardInformation(false);
            return s;
        }

        public string StateText
        {
            get
            {
                if (State != MissionState.StateTypes.Completed || Completed == null)
                    return State.ToString();
                else
                    return Completed.Value.ToString("N0");
            }
        }

        public long Value
        {
            get
            {
                if (State != MissionState.StateTypes.Completed || Completed == null)
                    return 0;
                else
                    return Completed.Value;
            }
        }

        public MissionState(JournalMissionAccepted m, ISystem s, string b)      // Start!
        {
            Mission = m;

            State = StateTypes.InProgress;
            MissionEndTime = m.Expiry;
            sys = s;
            body = b;
        }

        public MissionState(MissionState other, JournalMissionRedirected m)      // redirected mission
        {
            Mission = other.Mission;
            Redirected = m;                                                     // no completed, since we can't be
            CargoDepot = other.CargoDepot;

            State = other.State;
            MissionEndTime = other.MissionEndTime;
            sys = other.sys;
            body = other.body;
        }

        public MissionState(MissionState other, JournalMissionCompleted m)      // completed mission
        {
            Mission = other.Mission;
            Completed = m;                                                      // full set..
            Redirected = other.Redirected;
            CargoDepot = other.CargoDepot;

            State = StateTypes.Completed;
            MissionEndTime = m.EventTimeUTC;
            sys = other.sys;
            body = other.body;
        }

        public MissionState(MissionState other, JournalCargoDepot cd)           // cargo depot
        {
            Mission = other.Mission;
            Redirected = other.Redirected;                                      // no completed, since we can't be
            CargoDepot = cd;

            State = other.State;
            MissionEndTime = other.MissionEndTime;
            sys = other.sys;
            body = other.body;
        }

        public MissionState(MissionState other, StateTypes type, DateTime? endtime)           // changed to another state..
        {
            Mission = other.Mission;
            Redirected = other.Redirected;                                      // no completed, since we can't be - abandoned, failed, died, resurrected
            CargoDepot = other.CargoDepot;

            State = type;
            MissionEndTime = (endtime != null) ? endtime.Value : other.MissionEndTime;
            sys = other.sys;
            body = other.body;
        }
    }


    public class MissionListAccumulator
    {
        public List<MissionState> GetAllMissions()  // all missions stored, always return array
        {
            return history.GetLastValues();
        }

        public List<MissionState> GetMissionList(uint generation)   // missions at a generation. Always return array
        {
            return history.GetValues(generation).ToList();
        }

        public List<MissionState> GetAllCurrentMissions(uint generation, DateTime utc)  // ones current at this time in a generation. Always return array
        {
            return history.GetValues(generation, x => x.InProgressDateTime(utc)).OrderByDescending(y => y.Mission.EventTimeUTC).ToList();
        }

        public MissionState GetMission(string key) // null if not present
        {
            return history.GetLast(key);
        }

        public MissionState GetMission(uint generation, string key) // null if not present at that generation
        {
            return history.Get(key, generation);
        }

        static public List<MissionState> GetAllCombatMissionsLatestFirst(List<MissionState> ms)
        {
            return ms.Where(x => x.Mission.TargetType.Length > 0 && x.Mission.ExpiryValid).OrderByDescending(y => y.Mission.EventTimeUTC).ToList();
        }

        static public List<MissionState> GetAllCurrentMissions(List<MissionState> ms, DateTime curtime)
        {
            return ms.Where(x => x.InProgressDateTime(curtime)).OrderByDescending(y => y.Mission.EventTimeUTC).ToList();
        }

        static public List<MissionState> GetAllExpiredMissions(List<MissionState> ms, DateTime curtime)
        {
            return ms.Where(x => !x.InProgressDateTime(curtime)).OrderByDescending(y => y.Mission.EventTimeUTC).ToList();
        }

        public MissionListAccumulator()
        {
            history = new GenerationalDictionary<string, MissionState>();
        }

        public MissionListAccumulator(MissionListAccumulator other)
        {
            history = other.history;
        }

        public void Accepted(JournalMissionAccepted m, ISystem sys, string body)
        {
            string key = Key(m);
            history.NextGeneration();
            history[key] = new MissionState(m, sys, body);
        }

        public void Completed(JournalMissionCompleted c)
        {
            string key = Key(c);
            MissionState m = history.GetLast(key);       // we must have a last entry to add
            if (m != null)
            {
                history.NextGeneration();
                history[key] = new MissionState(m, c);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Missions Completed: Unknown " + key);
            }
        }

        public void CargoDepot(JournalCargoDepot cd)
        {
            string key = GetExistingKeyFromID(cd.MissionId);

            if (key != null)
            {
                MissionState m = history.GetLast(key);       // we must have a last entry to add
                if (m != null)
                {
                    history.NextGeneration();
                    history[key] = new MissionState(m, cd);
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Missions Cargo: Unknown " + key);
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Missions: Not found " + cd.MissionId);
            }
        }

        public void Abandoned(JournalMissionAbandoned a)
        {
            string key = Key(a);
            MissionState m = history.GetLast(key);       // we must have a last entry to add
            if (m != null)
            {
                history.NextGeneration();
                history[key] = new MissionState(m, MissionState.StateTypes.Abandoned, a.EventTimeUTC);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Missions Abandonded: Unknown " + key);
            }
        }

        public void Failed(JournalMissionFailed f)
        {
            string key = Key(f);
            MissionState m = history.GetLast(key);       // we must have a last entry to add
            if (m != null)
            {
                history.NextGeneration();
                history[key] = new MissionState(m, MissionState.StateTypes.Failed, f.EventTimeUTC);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Missions Failed: Unknown " + key);
            }
        }

        public void Redirected(JournalMissionRedirected r)
        {
            string key = Key(r);
            MissionState m = history.GetLast(key);       // we must have a last entry to add
            if (m != null)
            {
                history.NextGeneration();
                history[key] = new MissionState(m, r);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Missions Redirected: Unknown " + key);
            }
        }

        public void Died(DateTime diedtimeutc)
        {
            List<MissionState> affected = new List<MissionState>();
            foreach (var m in history.GetLast())
            {
                if (m.Value.InProgressDateTime(diedtimeutc))
                    affected.Add(m.Value);
            }

            if (affected.Count > 0)
            {
                history.NextGeneration();
                foreach (var m in affected)
                {
                    string key = Key(m.Mission);
                    history[key] = new MissionState(history.GetLast(key), MissionState.StateTypes.Died, diedtimeutc);
                }
            }
        }

        public void Resurrect(List<string> keys)
        {
            history.NextGeneration();
            foreach (string key in keys)
            {
                MissionState m = history.GetLast(key);       // we must have a last entry to resurrect
                if (m != null)
                {
                    history[key] = new MissionState(m, MissionState.StateTypes.InProgress, null); // copy previous mission info, resurrected, now!
                }
            }
        }

        public void ChangeStateIfInProgress(List<string> keys, MissionState.StateTypes state, DateTime missingtime)
        {
            history.NextGeneration();
            foreach (string key in keys)
            {
                MissionState m = history.GetLast(key);       // we must have a last entry to resurrect
                if (m != null && m.State == MissionState.StateTypes.InProgress)
                {
                    history[key] = new MissionState(m, state, missingtime);
                }
            }
        }

        public void Disappeared(List<string> keys, DateTime missingtime)
        {
            history.NextGeneration();
            foreach (string key in keys)
            {
                MissionState m = history.GetLast(key);       // we must have a last entry to resurrect

                if (m != null && m.State == MissionState.StateTypes.InProgress)
                {
                    // permits seem to be only 1 journal entry.. so its completed.
                    MissionState.StateTypes st = m.Mission.Name.Contains("permit", StringComparison.InvariantCultureIgnoreCase) ? MissionState.StateTypes.Completed : MissionState.StateTypes.Abandoned;
                    history[key] = new MissionState(m, st, missingtime);
                }
            }
        }

        public void Missions(JournalMissions m)
        {
            List<string> toresurrect = new List<string>();
            HashSet<string> active = new HashSet<string>();
            List<string> failed = new List<string>();
            List<string> completed = new List<string>();
            List<string> disappeared = new List<string>();

            foreach (var mi in m.ActiveMissions)
            {
                string kn = Key(mi.MissionID, mi.Name);
                active.Add(kn);
                //System.Diagnostics.Debug.WriteLine(m.EventTimeUTC.ToStringZulu() + " Mission " + kn + " is active");

                if (history.ContainsKey(kn))
                {
                    MissionState ms = history.GetLast(kn);

                    if (ms.State == MissionState.StateTypes.Died)  // if marked died... 
                    {
                        //System.Diagnostics.Debug.WriteLine("Missions in active list but marked died:" + kn);
                        toresurrect.Add(kn);
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Active mission '" + kn + "' But no Mission accepted");
                }
            }

            foreach (var mi in m.FailedMissions)
            {
                string kn = Key(mi.MissionID, mi.Name);
                failed.Add(kn);
            }

            foreach (var mi in m.CompletedMissions)
            {
                string kn = Key(mi.MissionID, mi.Name);
                completed.Add(kn);
            }

            foreach (var kvp in history.GetLast())
            {
                if (!active.Contains(kvp.Key) && !failed.Contains(kvp.Key) && !completed.Contains(kvp.Key) && kvp.Value.State == MissionState.StateTypes.InProgress)
                {
                   // System.Diagnostics.Debug.WriteLine(m.EventTimeUTC.ToStringZulu() + " Mission " + kvp.Value.Mission.EventTimeUTC.ToStringZulu() + " " + kvp.Key + " disappeared ****");
                    disappeared.Add(kvp.Key);
                }
            }

            if (toresurrect.Count > 0)       // if any..
            {
                Resurrect(toresurrect);
            }

            if (disappeared.Count > 0)
            {
                Disappeared(disappeared, m.EventTimeUTC);
            }

            if (failed.Count > 0)
            {
                ChangeStateIfInProgress(failed, MissionState.StateTypes.Failed, m.EventTimeUTC);
            }

            if (completed.Count > 0)
            {
                ChangeStateIfInProgress(failed, MissionState.StateTypes.Completed, m.EventTimeUTC);
            }
        }

        public string GetExistingKeyFromID(ulong id)       // some only have mission ID, generated after accept. Find on key, may return null
        {
            string frontpart = id.ToStringInvariant() + ":";

            foreach (var kvp in history.GetLast())
            {
                if (kvp.Key.StartsWith(frontpart))
                    return kvp.Key;
            }

//            System.Diagnostics.Debug.WriteLine("KEY MISSING "+ frontpart);

            return null;
        }

        public uint Process(JournalEntry je, ISystem sys, string body)
        {
            if (je is IMissions)
            {
                IMissions e = je as IMissions;
                e.UpdateMissions(this, sys, body);                                   // not cloned.. up to callers to see if they need to
            }

            return history.Generation;
        }

        public static string Key(JournalMissionFailed m) { return m.MissionId.ToStringInvariant() + ":" + m.Name; }
        public static string Key(JournalMissionCompleted m) { return m.MissionId.ToStringInvariant() + ":" + m.Name; }
        public static string Key(JournalMissionAccepted m) { return m.MissionId.ToStringInvariant() + ":" + m.Name; }
        public static string Key(JournalMissionRedirected m) { return m.MissionId.ToStringInvariant() + ":" + m.Name; }
        public static string Key(JournalMissionAbandoned m) { return m.MissionId.ToStringInvariant() + ":" + m.Name; }
        public static string Key(ulong id, string name) { return id.ToStringInvariant() + ":" + name; }

        private GenerationalDictionary<string, MissionState> history;
    }

}

