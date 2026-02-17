/*
 * Copyright 2016-2026 EDDiscovery development team
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
using System;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    // Location, CarrierJump, FSDJump
    public abstract class JournalLocOrJump : JournalEntry, IStatsJournalEntry, IBodyFeature
    {
        public string StarSystem { get; set; }
        public EMK.LightGeometry.Vector3 StarPos { get; set; }
        public long? SystemAddress { get; set; }
        public SystemSource LocOrJumpSource { get; set; } = SystemSource.FromJournal;     // this is the default..

        public string Body { get; set; }            // March 2019 introduced the destination body you jumped to
        public BodyDefinitions.BodyType BodyType { get; set; }      // as of JAN 26 : Barycentre, Star, StellarRing types
        public int? BodyID { get; set; }
        public bool Docked { get; set; } = false;       // can only be set for location

        public string Faction { get; set; }         // System Faction - keep name for backwards compat.
        public FactionDefinitions.State FactionState { get; set; }              //System Faction FDName
        public AllegianceDefinitions.Allegiance Allegiance { get; set; }        // System Allegiance FDName
        public EconomyDefinitions.Economy Economy { get; set; }                 // System Economy FDName
        public string Economy_Localised { get; set; }
        public EconomyDefinitions.Economy SecondEconomy { get; set; }           // System Economy FDName
        public string SecondEconomy_Localised { get; set; }
        public GovernmentDefinitions.Government Government { get; set; }        // System Government FDName
        public string Government_Localised { get; set; }
        public SecurityDefinitions.Security Security { get; set; }              // System Security FDName
        public string Security_Localised { get; set; }
        public long? Population { get; set; }
        public PowerPlayDefinitions.State PowerplayState { get; set; }      // seen in CarrierJump, Location and FSDJump.  Not in docked
        public double? PowerplayStateControlProgress { get; set; }          // update march 25
        public double? PowerplayStateReinforcement { get; set; }            // update march 25
        public double? PowerplayStateUndermining { get; set; }              // update march 25
        public PowerPlayDefinitions.PowerplayConflictProgress[] PowerplayConflictProgress {get;set;}                       // update march 25
        public string[] PowerplayPowers { get; set; }
        public string ControllingPower { get; set; }
        public bool Wanted { get; set; }
        public FactionDefinitions.FactionInformation[] Factions { get; set; }      // may be null for older entries
        public FactionDefinitions.ConflictInfo[] Conflicts { get; set; }           // may be null for older entries
        public ThargoidDefinitions.ThargoidWar ThargoidSystemState { get; set; }    // may be null for older entries or systems without thargoid war
        public bool HasCoordinate { get { return !float.IsNaN(StarPos.X); } }
        public bool IsTrainingEvent { get; private set; } // True if detected to be in training

        public bool HasFactionConflictThargoidInfo { get { return Factions != null || Conflicts != null || ThargoidSystemState != null; } }

        // IBodyFeature
        public double? Latitude { get; set; } = null;       // set when Location docked on planetary port
        public double? Longitude { get; set; } = null;
        public bool HasLatLong => Latitude != null && Longitude != null;
        public string BodyName => Body;
        public string Name { get; set; } = null;            // set when Location docked.
        public string Name_Localised { get; set; } = null;
        public long? MarketID { get; set; } = null;         // set when Location docked.
        public StationDefinitions.StarportTypes FDStationType { get; set; } = StationDefinitions.StarportTypes.Unknown;
        public string StationFaction { get; set; } = null; // 3.3.2 will be empty/null for previous logs.


        // Implementation

        protected JournalLocOrJump(DateTime utc, JournalTypeEnum jtype) : base(utc, jtype, 0)
        {
        }

        protected JournalLocOrJump(DateTime utc, ISystem sys, JournalTypeEnum jtype, bool edsmsynced ) : base(utc, jtype,  edsmsynced ? (int)SyncFlags.EDSM : 0)
        {
            StarSystem = sys.Name;
            SystemAddress = sys.SystemAddress;
            StarPos = new EMK.LightGeometry.Vector3((float)sys.X, (float)sys.Y, (float)sys.Z);
        }

        protected JournalLocOrJump(JObject evt, JournalTypeEnum jtype) : base(evt, jtype)
        {
            StarSystem = evt["StarSystem"].Str();
            LocOrJumpSource = evt["StarPosFromEDSM"].Bool(false) ? SystemSource.FromEDSM : SystemSource.FromJournal;

            EMK.LightGeometry.Vector3 pos = new EMK.LightGeometry.Vector3();

            JArray coords = evt["StarPos"].Array();
            if (coords!=null)
            {
                pos.X = coords[0].FloatNull() ?? float.NaN;
                pos.Y = coords[1].FloatNull() ?? float.NaN;
                pos.Z = coords[2].FloatNull() ?? float.NaN;
            }
            else
            {
                pos.X = pos.Y = pos.Z = float.NaN;
            }

            StarPos = pos;

            SystemAddress = evt["SystemAddress"].LongNull();

            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].IntNull();
            BodyType = BodyDefinitions.GetBodyType(evt["BodyType"].Str());

            JToken jk = evt["SystemFaction"];
            if (jk != null && jk.IsObject)                  // new 3.03
            {
                Faction = jk["Name"].Str();                // system faction pick up
                FactionState = FactionDefinitions.FactionStateToEnum(jk["FactionState"].Str("Unknown")).Value;      // 16 august 22, analysed journals, appears not written if faction state is none, so use as default
            }
            else
            {
                // old pre 3.3.3 had this - for system faction
                Faction = evt.MultiStr(new string[] { "SystemFaction", "Faction" });
                FactionState = FactionDefinitions.FactionStateToEnum(evt["FactionState"].Str("Unknown")).Value;           // PRE 2.3 .. not present in newer files, fixed up in next bit of code (but see 3.3.2 as its been incorrectly reintroduced)
            }

            if (evt.Contains("Factions"))      // if contains factions
                Factions = FactionDefinitions.FactionInformation.ReadJSON(evt["Factions"].Array(), EventTimeUTC, new SystemClass(StarSystem, SystemAddress));

            if (Factions != null)   // if read okay, make sure FactionState is set appropriately
            {
                int i = Array.FindIndex(Factions, x => x.Name == Faction);
                if (i != -1)
                    FactionState = Factions[i].FactionState;        // set to State of controlling faction
            }

            // don't moan if empty or not there as it could be due to training
            string al = evt.MultiStr(new string[] { "SystemAllegiance", "Allegiance" });
            
            Allegiance = al.HasChars() ? AllegianceDefinitions.ToEnum(al) : AllegianceDefinitions.Allegiance.Unknown;

            Economy = EconomyDefinitions.ToEnum( evt.MultiStr(new string[] { "SystemEconomy", "Economy" }) );
            Economy_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemEconomy_Localised", "Economy_Localised" }), EconomyDefinitions.ToEnglish(Economy));

            SecondEconomy = EconomyDefinitions.ToEnum(evt["SystemSecondEconomy"].StrNull());        // may not be there..
            SecondEconomy_Localised = JournalFieldNaming.CheckLocalisation(evt["SystemSecondEconomy_Localised"].Str(), EconomyDefinitions.ToEnglish(SecondEconomy));

            Government = GovernmentDefinitions.ToEnum( evt.MultiStr(new string[] { "SystemGovernment", "Government" }) );
            Government_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemGovernment_Localised", "Government_Localised" }), GovernmentDefinitions.ToEnglish(Government));

            Security = SecurityDefinitions.ToEnum( evt.MultiStr(new string[] { "SystemSecurity", "Security" }) );
            Security_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemSecurity_Localised", "Security_Localised" }), SecurityDefinitions.ToEnglish( Security));

            Population = evt["Population"].LongNull();

            ControllingPower = evt["ControllingPower"].StrNull();

            PowerplayState = PowerPlayDefinitions.ToEnum(evt["PowerplayState"].StrNull()); 
            PowerplayPowers = evt["Powers"]?.ToObjectQ<string[]>();

            PowerplayStateControlProgress = evt["PowerplayStateControlProgress"].DoubleNull();      // update march 25
            PowerplayStateReinforcement = evt["PowerplayStateReinforcement"].DoubleNull();
            PowerplayStateUndermining = evt["PowerplayStateUndermining"].DoubleNull();
            PowerplayConflictProgress = evt["PowerplayConflictProgress"]?.ToObjectQ<PowerPlayDefinitions.PowerplayConflictProgress[]>();

            Wanted = evt["Wanted"].Bool();      // if absence, your not wanted, by definition of frontier in journal (only present if wanted, see docked)

            if (evt.Contains("Conflicts"))      // if contains conflicts
                Conflicts = FactionDefinitions.ConflictInfo.ReadJSON(evt["Conflicts"].Array(), EventTimeUTC);

            if (evt.Contains("ThargoidWar"))      // if contains factions
                ThargoidSystemState = ThargoidDefinitions.ThargoidWar.ReadJSON(evt, EventTimeUTC);

        }
        public bool HasPowerPlayInfo { get {
                return ControllingPower != null ||  PowerplayState != PowerPlayDefinitions.State.Unknown || PowerplayPowers != null ||
                     PowerplayStateControlProgress != null || PowerplayStateReinforcement != null || PowerplayStateUndermining != null || PowerplayConflictProgress != null; } }

        public void FillPowerInfo(StringBuilder sb)
        {
            string powerplaystr = PowerplayPowers != null ? string.Join(", ", PowerplayPowers) : null;

            sb.Build("Power".Tx()+": ", ControllingPower,
                    "Power play State: ", PowerplayState,
                     "Power play Powers: " , powerplaystr,
                     "Power Control Progress: ", PowerplayStateControlProgress,
                     "Power Reinforcement: ", PowerplayStateReinforcement,
                     "Power Undermining: ", PowerplayStateUndermining
                    );

            if (PowerplayConflictProgress != null)
            {
                sb.AppendCR();
                sb.Append("Powerplay Conflict Progress: ");
                for(int i = 0; i < PowerplayConflictProgress.Length; i++)
                {
                    PowerplayConflictProgress[i].ToString(sb);
                    if (i < PowerplayConflictProgress.Length - 1)
                        sb.AppendCS();
                }
            }
        }

        public void FillFactionConflictThargoidPowerPlayConflictInfo(StringBuilder sb)
        {
            if (Factions != null)
            {
                foreach (FactionDefinitions.FactionInformation i in Factions)
                {
                    i.ToString(sb,false,true,true,true,true,true);
                }
            }

            if ( Conflicts != null )
            {
                foreach ( var cf in Conflicts)
                {
                    cf.ToString(sb);
                }
            }

            if (ThargoidSystemState != null)
            {
                ThargoidSystemState.ToString(sb);
            }

        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (Factions != null)
            {
                stats.UpdateFactions(system, this);
            }
        }
    }
}
