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

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.CrewAssign)]
    public class JournalCrewAssign : JournalEntry
    {
        public JournalCrewAssign(JObject evt) : base(evt, JournalTypeEnum.CrewAssign)
        {
            Name = evt["Name"].Str();
            Role = evt["Role"].Str();
            NpcCrewID = evt["CrewID"].Long();
        }

        public long NpcCrewID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("", Name, "< to role ;".Tx(), Role);
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewFire)]
    public class JournalCrewFire : JournalEntry
    {
        public JournalCrewFire(JObject evt) : base(evt, JournalTypeEnum.CrewFire)
        {
            Name = evt["Name"].Str();
            NpcCrewID = evt["CrewID"].Long();
        }

        public long NpcCrewID { get; set; }
        public string Name { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("; fired".Tx(), Name);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewHire)]
    public class JournalCrewHire : JournalEntry, ILedgerJournalEntry
    {
        public JournalCrewHire(JObject evt) : base(evt, JournalTypeEnum.CrewHire)
        {
            Name = evt["Name"].Str();
            Faction = evt["Faction"].Str();
            Cost = evt["Cost"].Long();
            CombatRank = (RankDefinitions.CombatRank)evt["CombatRank"].Int();
            NpcCrewID = evt["CrewID"].Long();
        }

        public long NpcCrewID { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public long Cost { get; set; }
        public RankDefinitions.CombatRank CombatRank { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name + " " + Faction, -Cost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Hired: ;".Tx(), Name, "< of faction ".Tx(), 
                            Faction, "Rank: ".Tx(), RankDefinitions.FriendlyName(CombatRank), "Cost: ; cr;N0".Tx(), Cost);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewLaunchFighter)]
    public class JournalCrewLaunchFighter : JournalEntry
    {
        public JournalCrewLaunchFighter(JObject evt) : base(evt, JournalTypeEnum.CrewLaunchFighter)
        {
            Crew = evt["Crew"].Str();
            ID = evt["ID"].IntNull();
            Telepresence = evt["Telepresence"].Bool();

        }
        public string Crew { get; set; }
        public int? ID { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Crew: ".Tx(), Crew, ";Telepresence".Tx(), Telepresence);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewMemberJoins)]
    public class JournalCrewMemberJoins : JournalEntry
    {
        public JournalCrewMemberJoins(JObject evt) : base(evt, JournalTypeEnum.CrewMemberJoins)
        {
            Crew = evt["Crew"].Str();
            Telepresence = evt["Telepresence"].Bool();

        }
        public string Crew { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Crew: ".Tx(), Crew, ";Telepresence".Tx(), Telepresence);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewMemberQuits)]
    public class JournalCrewMemberQuits : JournalEntry
    {
        public JournalCrewMemberQuits(JObject evt) : base(evt, JournalTypeEnum.CrewMemberQuits)
        {
            Crew = evt["Crew"].Str();
            Telepresence = evt["Telepresence"].Bool();

        }
        public string Crew { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Crew: ".Tx(), Crew, ";Telepresence".Tx(), Telepresence);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.CrewMemberRoleChange)]
    public class JournalCrewMemberRoleChange : JournalEntry
    {
        public JournalCrewMemberRoleChange(JObject evt) : base(evt, JournalTypeEnum.CrewMemberRoleChange)
        {
            Crew = evt["Crew"].Str();
            FDRole = evt["Role"].Str();
            Role = JournalFieldNaming.CrewRole(FDRole);
            Telepresence = evt["Telepresence"].Bool();
        }

        public string Crew { get; set; }
        public string Role { get; set; }
        public string FDRole { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Crew: ".Tx(), Crew, "Role: ".Tx(), Role, ";Telepresence".Tx(), Telepresence);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.KickCrewMember)]
    public class JournalKickCrewMember : JournalEntry
    {
        public JournalKickCrewMember(JObject evt) : base(evt, JournalTypeEnum.KickCrewMember)
        {
            Crew = evt["Crew"].Str();
            OnCrime = evt["OnCrime"].Bool();
            Telepresence = evt["Telepresence"].Bool();
        }

        public string Crew { get; set; }
        public bool OnCrime { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Crew Member: ".Tx(), Crew, ";Due to Crime".Tx(), OnCrime, ";Telepresence".Tx(), Telepresence);
            
        }
    }

    [JournalEntryType(JournalTypeEnum.JoinACrew)]
    public class JournalJoinACrew : JournalEntry
    {
        public JournalJoinACrew(JObject evt) : base(evt, JournalTypeEnum.JoinACrew)
        {
            Captain = evt["Captain"].Str();
            Telepresence = evt["Telepresence"].Bool();

        }
        public string Captain { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {

            return BaseUtils.FieldBuilder.Build("Captain: ".Tx(), Captain, ";Telepresence".Tx(), Telepresence);
            
        }
    }


    [JournalEntryType(JournalTypeEnum.ChangeCrewRole)]
    public class JournalChangeCrewRole : JournalEntry
    {
        public JournalChangeCrewRole(JObject evt) : base(evt, JournalTypeEnum.ChangeCrewRole)
        {
            FDRole = evt["Role"].Str();
            Role = JournalFieldNaming.CrewRole(FDRole);
            Telepresence = evt["Telepresence"].Bool();
        }

        public string Role { get; set; }
        public string FDRole { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Role: ".Tx(), Role, ";Telepresence".Tx(), Telepresence);
            
        }
    }


    [JournalEntryType(JournalTypeEnum.EndCrewSession)]
    public class JournalEndCrewSession : JournalEntry
    {
        public JournalEndCrewSession(JObject evt) : base(evt, JournalTypeEnum.EndCrewSession)
        {
            OnCrime = evt["OnCrime"].Bool();
            Telepresence = evt["Telepresence"].Bool();

        }
        public bool OnCrime { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("; Due to Crime".Tx(), OnCrime, ";Telepresence".Tx(), Telepresence);
            
        }
    }


    [JournalEntryType(JournalTypeEnum.QuitACrew)]
    public class JournalQuitACrew : JournalEntry
    {
        public JournalQuitACrew(JObject evt) : base(evt, JournalTypeEnum.QuitACrew)
        {
            Captain = evt["Captain"].Str();
            Telepresence = evt["Telepresence"].Bool();
        }

        public string Captain { get; set; }
        public bool Telepresence { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Captain: ".Tx(), Captain, ";Telepresence".Tx(), Telepresence);
            
        }

    }


}
