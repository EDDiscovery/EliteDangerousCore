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
 *
 *
 */
using QuickJSON;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Powerplay)]
    public class JournalPowerplay : JournalEntry
    {
        public JournalPowerplay(JObject evt) : base(evt, JournalTypeEnum.Powerplay)
        {
            Power = evt["Power"].Str();
            Rank = evt["Rank"].Int();
            Merits = evt["Merits"].Int();
            Votes = evt["Votes"].IntNull();
            TimePledged = evt["TimePledged"].Long();
            TimePledgedSpan = new TimeSpan((int)(TimePledged/60/60),(int)((TimePledged/60)%60),(int)(TimePledged%60));
            TimePledgedString = TimePledgedSpan.ToString();
        }

        public string Power { get; set; }
        public int Rank { get; set; }
        public int Merits { get; set; }
        public int? Votes { get; set; }     // votes removed in Ascendency V1 update
        public long TimePledged { get; set; }
        public TimeSpan TimePledgedSpan { get;}
        public DateTime TimeJoinedUTC { get { return EventTimeUTC.Subtract(TimePledgedSpan); } }
        public string TimePledgedString { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("", Power, "Rank".Tx()+": ", Rank, "Merits".Tx()+": ", Merits, "Votes".Tx()+": ", Votes, "Pledged".Tx()+": ", TimePledgedString);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayCollect)]
    public class JournalPowerplayCollect : JournalEntry, ICommodityJournalEntry
    {
        public JournalPowerplayCollect(JObject evt) : base(evt, JournalTypeEnum.PowerplayCollect)
        {
            Power = evt["Power"].Str();
            Type = JournalFieldNaming.FixCommodityName(evt["Type"].Str());
            Type_Localised = JournalFieldNaming.CheckLocalisation(evt["Type_Localised"].Str(), Type);
            Count = evt["Count"].Int();

        }
        public string Power { get; set; }
        public string Type { get; set; }
        public string Type_Localised { get; set; }
        public int Count { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Power, "Type".Tx()+": ", Type_Localised, "Count".Tx()+": ", Count);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, Type, Count, 0);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayDefect)]
    public class JournalPowerplayDefect : JournalEntry
    {
        public JournalPowerplayDefect(JObject evt) : base(evt, JournalTypeEnum.PowerplayDefect)
        {
            FromPower = evt["FromPower"].Str();
            ToPower = evt["ToPower"].Str();
        }

        public string FromPower { get; set; }
        public string ToPower { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("From Power".Tx()+": ", FromPower, "To Power".Tx()+": ", ToPower);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayDeliver)]
    public class JournalPowerplayDeliver : JournalEntry, ICommodityJournalEntry
    {
        public JournalPowerplayDeliver(JObject evt) : base(evt, JournalTypeEnum.PowerplayDeliver)
        {
            Power = evt["Power"].Str();
            Type = JournalFieldNaming.FixCommodityName(evt["Type"].Str());
            Type_Localised = JournalFieldNaming.CheckLocalisation(evt["Type_Localised"].Str(), Type);
            Count = evt["Count"].Int();
        }

        public string Power { get; set; }
        public string Type { get; set; }
        public string Type_Localised { get; set; }
        public int Count { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Power, "Type".Tx()+": ", Type_Localised, "Count".Tx()+": ", Count);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, Type, -Count, 0 );
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayFastTrack)]
    public class JournalPowerplayFastTrack : JournalEntry, ILedgerJournalEntry
    {
        public JournalPowerplayFastTrack(JObject evt) : base(evt, JournalTypeEnum.PowerplayFastTrack)
        {
            Power = evt["Power"].Str();
            Cost = evt["Cost"].Long();
        }

        public string Power { get; set; }
        public long Cost { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Power, -Cost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Power, "Cost: ; cr;N0".Tx(), Cost);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayJoin)]
    public class JournalPowerplayJoin : JournalEntry
    {
        public JournalPowerplayJoin(JObject evt) : base(evt, JournalTypeEnum.PowerplayJoin)
        {
            Power = evt["Power"].Str();
        }

        public string Power { get; set; }

        public override string GetInfo()
        {
            return Power;
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayLeave)]
    public class JournalPowerplayLeave : JournalEntry
    {
        public JournalPowerplayLeave(JObject evt) : base(evt, JournalTypeEnum.PowerplayLeave)
        {
            Power = evt["Power"].Str();
        }

        public string Power { get; set; }

        public override string GetInfo()
        {
            return Power;
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplaySalary)]
    public class JournalPowerplaySalary : JournalEntry, ILedgerJournalEntry
    {
        public JournalPowerplaySalary(JObject evt) : base(evt, JournalTypeEnum.PowerplaySalary)
        {
            Power = evt["Power"].Str();
            Amount = evt["Amount"].Long();
        }

        public string Power { get; set; }
        public long Amount { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Power, Amount);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Power, "Amount: ; cr;N0".Tx(), Amount);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayVote)]
    public class JournalPowerplayVote : JournalEntry
    {
        public JournalPowerplayVote(JObject evt) : base(evt, JournalTypeEnum.PowerplayVote)
        {
            Power = evt["Power"].Str();
            System = evt["System"].Str();
            Votes = evt["Votes"].Int();
        }

        public string Power { get; set; }
        public string System { get; set; }
        public int Votes { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Power, "System".Tx()+": ", System, "Votes".Tx()+": ", Votes);
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayVoucher)]
    public class JournalPowerplayVoucher : JournalEntry
    {
        public JournalPowerplayVoucher(JObject evt) : base(evt, JournalTypeEnum.PowerplayVoucher)
        {
            Power = evt["Power"].Str();
            Systems = evt["Systems"]?.ToObjectQ<string[]>();
        }

        public string Power { get; set; }
        public string[] Systems { get; set; }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Power);

            if (Systems != null)
            {
                sb.Append(", Systems".Tx()+": ");

                bool comma = false;
                foreach (string s in Systems)
                {
                    if (comma)
                        sb.AppendCS();
                    comma = true;
                    sb.Append(s);
                }
            }
            return sb.ToString();
        }
    }

    [JournalEntryType(JournalTypeEnum.PowerplayMerits)]
    public class JournalPowerplayMerits : JournalEntry
    {
        public JournalPowerplayMerits(JObject evt) : base(evt, JournalTypeEnum.PowerplayMerits)
        {
            Power = evt["Power"].Str();
            MeritsGained = evt["MeritsGained"].Double();
            TotalMerits = evt["TotalMerits"].Double();
        }

        public string Power { get; set; }
        public double MeritsGained { get; set; }
        public double TotalMerits { get; set; }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Power);
            sb.Append(" + ");
            sb.Append(MeritsGained);
            sb.Append(" = ");
            sb.Append(TotalMerits);
            return sb.ToString();
        }
    }


    [JournalEntryType(JournalTypeEnum.PowerplayRank)]
    public class JournalPowerplayRank : JournalEntry
    {
        public JournalPowerplayRank(JObject evt) : base(evt, JournalTypeEnum.PowerplayRank)
        {
            Power = evt["Power"].Str();
            Rank = evt["Rank"].Int();
        }

        public string Power { get; set; }
        public int Rank { get; set; }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Power);
            sb.Append(" ");
            sb.Append(Rank);
            return sb.ToString();
        }
    }


}
