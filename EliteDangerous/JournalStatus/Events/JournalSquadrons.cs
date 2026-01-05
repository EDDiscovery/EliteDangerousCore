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
using System.Linq;
using System.Text;
using static BaseUtils.IntRangeList;

namespace EliteDangerousCore.JournalEvents
{
    public class JournalSquadronBase : JournalEntry
    {
        public JournalSquadronBase(JObject evt, JournalTypeEnum e) : base(evt, e)
        {
            Name = evt["SquadronName"].Str();
            SquadronID = evt["SquadronID"].Int();
        }

        public string Name { get; set; }
        public int SquadronID { get; set; }

        protected RankDefinitions.SquadronRank GetRank(JToken evt, bool newrank, string idfield)
        {
            //bool oldrank = EDCommander.IsLegacyCommander(CommanderId) || EventTimeUTC < EliteReleaseDates.Vanguards;
            int value = evt[idfield].Int() + (newrank ? (int)RankDefinitions.SquadronRank.Rank0 : 0);
            return (RankDefinitions.SquadronRank)value;
        }
    }

    [JournalEntryType(JournalTypeEnum.AppliedToSquadron)]
    public class JournalAppliedToSquadron : JournalSquadronBase
    {
        public JournalAppliedToSquadron(JObject evt) : base(evt, JournalTypeEnum.AppliedToSquadron)
        {
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.DisbandedSquadron)]
    public class JournalDisbandedSquadron : JournalSquadronBase
    {
        public JournalDisbandedSquadron(JObject evt) : base(evt, JournalTypeEnum.DisbandedSquadron)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.InvitedToSquadron)]
    public class JournalInvitedToSquadron : JournalSquadronBase
    {
        public JournalInvitedToSquadron(JObject evt) : base(evt, JournalTypeEnum.InvitedToSquadron)
        {
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.JoinedSquadron)]
    public class JournalJoinedSquadron : JournalSquadronBase
    {
        public JournalJoinedSquadron(JObject evt) : base(evt, JournalTypeEnum.JoinedSquadron)
        {
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.KickedFromSquadron)]
    public class JournalKickedFromSquadron : JournalSquadronBase
    {
        public JournalKickedFromSquadron(JObject evt) : base(evt, JournalTypeEnum.KickedFromSquadron)
        {
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.LeftSquadron)]
    public class JournalLeftSquadron : JournalSquadronBase
    {
        public JournalLeftSquadron(JObject evt) : base(evt, JournalTypeEnum.LeftSquadron)
        {
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.SharedBookmarkToSquadron)]
    public class JournalSharedBookmarkToSquadron : JournalSquadronBase
    {
        public JournalSharedBookmarkToSquadron(JObject evt) : base(evt, JournalTypeEnum.SharedBookmarkToSquadron)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.SquadronCreated)]
    public class JournalSquadronCreated : JournalSquadronBase
    {
        public JournalSquadronCreated(JObject evt) : base(evt, JournalTypeEnum.SquadronCreated)
        {
        }
    }

    public class JournalSquadronRankBase : JournalSquadronBase
    {
        public JournalSquadronRankBase(JObject evt, JournalTypeEnum e) : base(evt, e)
        {
            bool newformat = evt.Contains("OldRankName") || evt.Contains("NewRankName");

            OldRank = GetRank(evt, newformat, "OldRank");
            NewRank = GetRank(evt, newformat, "NewRank");

            // Vanguards + have these fields, older ones don't, but fill in
            OldRankName = evt["OldRankName"].Str(RankDefinitions.FriendlyName(OldRank));
            NewRankName = evt["NewRankName"].Str(RankDefinitions.FriendlyName(NewRank));

            OldRankName_Localised = JournalFieldNaming.CheckLocalisation(evt["OldRankName_Localised"].Str(), OldRankName);
            NewRankName_Localised = JournalFieldNaming.CheckLocalisation(evt["NewRankName_Localised"].Str(), NewRankName);
        }
        
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name, "Old".Tx() + ": ", OldRankName_Localised,
                        "New".Tx() + ": ", NewRankName_Localised);
        }
        
        public RankDefinitions.SquadronRank OldRank { get; set; }
        public RankDefinitions.SquadronRank NewRank { get; set; }
        public string OldRankName { get; set; }                 // always filled,even for older squadrons
        public string NewRankName { get; set; }
        public string OldRankName_Localised { get; set; }
        public string NewRankName_Localised { get; set; }
    }


    [JournalEntryType(JournalTypeEnum.SquadronDemotion)]
    public class JournalSquadronDemotion : JournalSquadronRankBase
    {
        public JournalSquadronDemotion(JObject evt) : base(evt, JournalTypeEnum.SquadronDemotion)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.SquadronPromotion)]
    public class JournalSquadronPromotion : JournalSquadronRankBase
    {
        public JournalSquadronPromotion(JObject evt) : base(evt, JournalTypeEnum.SquadronPromotion)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.WonATrophyForSquadron)]
    public class JournalWonATrophyForSquadron : JournalSquadronBase
    {
        public JournalWonATrophyForSquadron(JObject evt) : base(evt, JournalTypeEnum.WonATrophyForSquadron)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.SquadronStartup)]
    public class JournalSquadronStartup : JournalSquadronBase
    {
        public JournalSquadronStartup(JObject evt) : base(evt, JournalTypeEnum.SquadronStartup)
        {
            CurrentRank = GetRank(evt, evt.Contains("CurrentRankName"), "CurrentRank");
            CurrentRankName = evt["CurrentRankName"].Str(RankDefinitions.FriendlyName(CurrentRank));
            CurrentRankName_Localised = JournalFieldNaming.CheckLocalisation(evt["CurrentRankName_Localised"].Str(), CurrentRankName);
        }

        public RankDefinitions.SquadronRank CurrentRank { get; set; }
        public string CurrentRankName { get; set; }     // always filled in
        public string CurrentRankName_Localised { get; set; }     // always filled in

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name, "Rank".Tx() + ": ", CurrentRankName_Localised);
        }
    }

    [JournalEntryType(JournalTypeEnum.CancelledSquadronApplication)]
    public class JournalCancelledSquadronApplication : JournalSquadronBase
    {
        public JournalCancelledSquadronApplication(JObject evt) : base(evt, JournalTypeEnum.CancelledSquadronApplication)
        {
        }

        public override string GetInfo()
        {
            return Name;
        }
    }

    [JournalEntryType(JournalTypeEnum.SquadronApplicationApproved)]
    public class JournalSquadronApplicationApproved : JournalSquadronBase
    {
        public JournalSquadronApplicationApproved(JObject evt) : base(evt, JournalTypeEnum.SquadronApplicationApproved)
        {
        }

        public override string GetInfo()
        {
            return Name;
        }
    }

    [JournalEntryType(JournalTypeEnum.SquadronApplicationRejected)]
    public class JournalSquadronApplicationRejected : JournalSquadronBase
    {
        public JournalSquadronApplicationRejected(JObject evt) : base(evt, JournalTypeEnum.SquadronApplicationRejected)
        {
        }

        public override string GetInfo()
        {
            return Name;
        }
    }

}
