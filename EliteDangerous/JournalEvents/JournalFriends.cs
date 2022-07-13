/*
 * Copyright © 2016-2018 EDDiscovery development team
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
using QuickJSON;
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
            StatusEnum = evt["Status"].EnumStr(FriendStatus.Offline);
            Name = evt["Name"].Str();
            OfflineCount = StatusEnum == FriendStatus.Offline ? 1 : 0;
            OnlineCount = StatusEnum == FriendStatus.Online ? 1 : 0;
        }

        public void AddFriend(JournalFriends next)
        {
            if (StatusList == null)     // if first time we added, move to status list format
            {
                StatusList = new List<FriendStatus>() { StatusEnum };
                NameList = new List<string>() { Name };
                Status = Name = string.Empty;
            }

            StatusList.Add(next.StatusEnum);
            NameList.Add(next.Name);

            OfflineCount = next.StatusEnum == FriendStatus.Offline ? 1 : 0;
            OnlineCount = next.StatusEnum == FriendStatus.Online ? 1 : 0;
        }

        public enum FriendStatus { Requested, Declined, Added, Lost, Offline, Online}
        public FriendStatus StatusEnum { get; set; }      // used for single entries.. empty if list.  Used for VP backwards compat

        public string Status { get; set; }              // backwards compat
        public string Name { get; set; }
        public int OnlineCount { get; set; }            // always counts
        public int OfflineCount { get; set; }

        public List<FriendStatus> StatusList { get; set; }        // EDD addition.. used when agregating, null if single entry
        public List<string> NameList { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed) 
        {
            detailed = "";

            if (StatusList != null)
            {
                info = "";
                if (OfflineCount + OnlineCount < NameList.Count)
                    info = BaseUtils.FieldBuilder.Build("Number of Statuses: ".T(EDCTx.JournalEntry_NumberofStatuses), NameList.Count);

                if (OnlineCount > 0)
                    info = info.AppendPrePad("Online: ".T(EDCTx.JournalEntry_Online) + OnlineCount.ToString(), ", ");

                if (OfflineCount > 0)
                    info = info.AppendPrePad("Offline: ".T(EDCTx.JournalEntry_Offline) + OfflineCount.ToString(), ", ");

                for ( int i = 0; i < StatusList.Count; i++ )
                    detailed = detailed.AppendPrePad(ST(NameList[i], StatusList[i]) , System.Environment.NewLine);
            }
            else
            {
                info = ST(Name, StatusEnum);
            }
        }

        static private string ST(string friendname, FriendStatus stat)
        {
            if (stat == FriendStatus.Online)
                return "Online: ".T(EDCTx.JournalEntry_Online) + friendname;
            else if (stat == FriendStatus.Offline)
                return "Offline: ".T(EDCTx.JournalEntry_Offline) + friendname;
            else if (stat == FriendStatus.Lost)
                return "Unfriended: ".T(EDCTx.JournalEntry_Unfriended) + friendname;
            else if (stat == FriendStatus.Declined)
                return "Declined: ".T(EDCTx.JournalEntry_Declined) + friendname;
            else if (stat == FriendStatus.Requested)
                return "Requested Friend: ".T(EDCTx.JournalEntry_RequestedFriend) + friendname;
            else if (stat == FriendStatus.Added)
                return "Added Friend: ".T(EDCTx.JournalEntry_AddedFriend) + friendname;
            else
                return "??";
        }
    }
}
