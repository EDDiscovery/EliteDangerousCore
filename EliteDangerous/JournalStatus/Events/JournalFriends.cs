/*
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
 *
 *
 */
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Friends)]
    public class JournalFriends : JournalEntry
    {
        public JournalFriends(JObject evt) : base(evt, JournalTypeEnum.Friends)
        {
            Status = evt["Status"].Str();
            Name = evt["Name"].Str();

            var state = OnlineOffline(Status);
            OfflineCount = state.Item1 < 0 ? 1 : 0;
            OnlineCount = state.Item1 > 0 ? 1 : 0;
            OtherCount = state.Item1 == 0 ? 1 : 0;
        }

        public void AddFriend(JournalFriends next)
        {
            if (StatusList == null)     // if first time we added, move to status list format
            {
                StatusList = new List<FriendClass>() { new FriendClass() { Status = this.Status, Name = this.Name } };
                Status = Name = string.Empty;
            }

            var last = StatusList.FindLast(x => x.Name.Equals(next.Name));      // find last entry with same name

            if (last == null || last.Status != next.Status)       // if not last in list, or not the same status, add as a new status
            {
                StatusList.Add(new FriendClass() { Status = next.Status, Name = next.Name });
                //System.Diagnostics.Debug.WriteLine($"Friends {this.EventTimeUTC} Add new status {next.Status} {next.Name}");
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"Friends {this.EventTimeUTC} Duplicate status {next.Status} {next.Name}");
            }

            var flist = FriendState();      // get Friend status list
            OfflineCount = flist.Values.Count(x => x.Item1 < 0);
            OnlineCount = flist.Values.Count(x => x.Item1 > 0);
            OtherCount = flist.Values.Count(x => x.Item1 == 0);
        }


        public string Status { get; set; }              // for single entry only, empty for merged friends
        public string Name { get; set; }

        public static bool IsFriend(string status) { return status.EqualsIIC("Online") || status.EqualsIIC("Added"); }
        public static bool IsNotFriend(string status) { return status.EqualsIIC("Lost"); }
        public static bool IsOnline(string status) { return status.EqualsIIC("Online"); }
        public static bool IsOffline(string status) { return status.EqualsIIC("Offline"); }

        // EDD
        [System.Diagnostics.DebuggerDisplay("{Name} {Status}")]
        public class FriendClass
        {
            public string Status { get; set; }       // { Requested, Declined, Added, Lost, Offline, Online }
            public string Name { get; set; }
        }

        public List<FriendClass> StatusList { get; set; }        // EDD addition.. used when agregating, null if single entry
        public int OnlineCount { get; set; }            // always counts if online/offline
        public int OfflineCount { get; set; }
        public int OtherCount { get; set; }             // counts other status counts

        public override string GetInfo() 
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (StatusList != null)
            {
                var flist = FriendState();      // get consolidated list, ignoring repeat names

               // if ( flist.Count != OnlineCount + OfflineCount)
               //     return BaseUtils.FieldBuilder.Build("Number of Statuses: ".Tx(), flist.Count);

                if (OnlineCount > 0)
                {
                    sb.AppendPrePad("Online: ".Tx()+ OnlineCount.ToString(), ", ");

                    foreach (var f in flist.Where(x => x.Value.Item1 > 0))
                        sb.AppendPrePad(f.Key, ", ");
                }

                if (OfflineCount > 0)
                {
                    sb.AppendPrePad("Offline: ".Tx()+ OfflineCount.ToString(), ", ");

                    foreach (var f in flist.Where(x => x.Value.Item1 < 0))
                        sb.AppendPrePad(f.Key, ", ");
                }

                foreach( var f in flist.Where(x=>x.Value.Item1 == 0 ) )
                    sb.AppendPrePad(f.Key + ": " + f.Value.Item2, ", ");

                for ( int i = 0; i < StatusList.Count; i++ )
                    sb.AppendPrePad(Translate(StatusList[i].Name, StatusList[i].Status) , System.Environment.NewLine);
            }
            else
            {
                sb.Append(Translate(Name, Status));
            }

            return sb.ToString();

        }

        public List<FriendClass> Statuses()
        {
            if (StatusList == null)
                return new List<FriendClass> { new FriendClass() { Name = this.Name, Status = this.Status } };
            else
                return StatusList;
        }

        // return dictionary of friends and if they are online (=1) or offline (-1) or other (=0), and their last status
        public Dictionary<string, Tuple<int,string>> FriendState()
        {
            var list = new Dictionary<string, Tuple<int,string>>();
            if (StatusList == null)
            {
                list[Name] = OnlineOffline(Status);
            }
            else
            {
                foreach (var f in StatusList)
                {
                    if (list.TryGetValue(f.Name, out Tuple<int, string> value))
                        list[f.Name] = OnlineOffline(f.Status);     // note we overwrite the entry with the lastest status
                    else
                        list[f.Name] = OnlineOffline(f.Status);
                }
            }
            return list;
        }

        // compute the online offline tuple
        static private Tuple<int,string> OnlineOffline(string status)
        {
            int v = (IsOnline(status) ? 1 : 0) + (IsOffline(status) ? -1 : 0);
            return new Tuple<int, string>(v, status);
        }

        static private string Translate(string friendname, string status)
        {
            if (status.EqualsIIC("Online"))
                return "Online: ".Tx()+ friendname;
            else if (status.EqualsIIC("Offline"))
                return "Offline: ".Tx()+ friendname;
            else if (status.EqualsIIC("Lost"))
                return "Unfriended: ".Tx()+ friendname;
            else if (status.EqualsIIC("Declined"))
                return "Declined: ".Tx()+ friendname;
            else if (status.EqualsIIC("Requested"))
                return "Requested Friend: ".Tx()+ friendname;
            else if (status.EqualsIIC("Added"))
                return "Added Friend: ".Tx()+ friendname;
            else
                return "??";
        }
    }
}
