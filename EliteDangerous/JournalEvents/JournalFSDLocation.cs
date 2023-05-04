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
using EliteDangerousCore.DB;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace EliteDangerousCore.JournalEvents
{
    public abstract class JournalLocOrJump : JournalEntry, ISystemStationEntry
    {
        public string StarSystem { get; set; }
        public EMK.LightGeometry.Vector3 StarPos { get; set; }
        public long? SystemAddress { get; set; }
        public bool StarPosFromEDSM { get; set; }

        public string Faction { get; set; }         // System Faction - keep name for backwards compat.
        public string FactionState { get; set; }    // System Faction State - keep name for backwards compat.
        public string Allegiance { get; set; }
        public string Economy { get; set; }
        public string Economy_Localised { get; set; }
        public string SecondEconomy { get; set; }
        public string SecondEconomy_Localised { get; set; }
        public string Government { get; set; }
        public string Government_Localised { get; set; }
        public string Security { get; set; }
        public string Security_Localised { get; set; }
        public long? Population { get; set; }
        public string PowerplayState { get; set; }
        public string[] PowerplayPowers { get; set; }
        public bool Wanted { get; set; }

        public FactionInformation[] Factions { get; set; }

        public ConflictInfo[] Conflicts { get; set; }

        public ThargoidWar ThargoidSystemState { get; set; }

        public bool HasCoordinate { get { return !float.IsNaN(StarPos.X); } }

        public bool IsTrainingEvent { get; private set; } // True if detected to be in training

        public class FactionInformation
        {
            public string Name { get; set; }
            public string FactionState { get; set; }    
            public string Government { get; set; }
            public double Influence { get; set; }
            public string Allegiance { get; set; }
            public string Happiness { get; set; }   //3.3, may be null
            public string Happiness_Localised { get; set; } //3.3, may be null
            public double? MyReputation { get; set; } //3.3, may be null

            public PowerStatesInfo[] PendingStates { get; set; }
            public PowerStatesInfo[] RecoveringStates { get; set; }
            public ActiveStatesInfo[] ActiveStates { get; set; }    //3.3, may be null

            public bool? SquadronFaction;       //3.3, may be null
            public bool? HappiestSystem;       //3.3, may be null
            public bool? HomeSystem;       //3.3, may be null
        }

        public class PowerStatesInfo
        {
            public string State { get; set; }
            public int Trend { get; set; }
        }

        public class ActiveStatesInfo       // Howard info via discord .. just state
        {
            public string State { get; set; }
        }

        public class ConflictFactionInfo   // 3.4
        {
            public string Name { get; set; }
            public string Stake { get; set; }
            public int WonDays { get; set; }
        }

        public class ConflictInfo   // 3.4
        {
            public string WarType { get; set; }
            public string Status { get; set; }
            public ConflictFactionInfo Faction1 { get; set; }
            public ConflictFactionInfo Faction2 { get; set; }
        }

        public class ThargoidWar
        {
            // CurrentState and NextState are the same state values that are listed in factions, "Lockdown", "Bust", etc.. 
            // new states related to thargoid systems are "Thargoid_Probing", "Thargoid_Harvest", "Thargoid_Controlled", "Thargoid_Stronghold", "Thargoid_Recovery"
            public string CurrentState { get; set; }
            public string NextStateSuccess {get;set; }       
            public string NextStateFailure {get;set;}
            public bool SuccessStateReached {get;set;}
            public double WarProgress {get;set;}        // 0 to 1
            public int RemainingPorts {get;set;}        
            public double EstimatedRemainingTime {get;set;}    // days               
        }

        protected JournalLocOrJump(DateTime utc, ISystem sys, JournalTypeEnum jtype, bool edsmsynced ) : base(utc, jtype, edsmsynced)
        {
            StarSystem = sys.Name;
            StarPos = new EMK.LightGeometry.Vector3((float)sys.X, (float)sys.Y, (float)sys.Z);
        }

        protected JournalLocOrJump(JObject evt, JournalTypeEnum jtype) : base(evt, jtype)
        {
            StarSystem = evt["StarSystem"].Str();
            StarPosFromEDSM = evt["StarPosFromEDSM"].Bool(false);

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

            JToken jk = evt["SystemFaction"];
            if (jk != null && jk.IsObject)                  // new 3.03
            {
                Faction = jk["Name"].Str();                // system faction pick up
                FactionState = jk["FactionState"].Str("None");      // 16 august 22, analysed journals, appears not written if faction state is none, so use as default
            }
            else
            {
                // old pre 3.3.3 had this - for system faction
                Faction = evt.MultiStr(new string[] { "SystemFaction", "Faction" });
                FactionState = evt["FactionState"].Str();           // PRE 2.3 .. not present in newer files, fixed up in next bit of code (but see 3.3.2 as its been incorrectly reintroduced)
            }

            Factions = evt["Factions"]?.ToObjectQ<FactionInformation[]>()?.OrderByDescending(x => x.Influence)?.ToArray();  // POST 2.3

            if (Factions != null)
            {
                int i = Array.FindIndex(Factions, x => x.Name == Faction);
                if (i != -1)
                    FactionState = Factions[i].FactionState;        // set to State of controlling faction

                foreach( var x in Factions)     // normalise localised
                    x.Happiness_Localised = JournalFieldNaming.CheckLocalisation(x.Happiness_Localised, x.Happiness);
            }

            Allegiance = evt.MultiStr(new string[] { "SystemAllegiance", "Allegiance" });

            Economy = evt.MultiStr(new string[] { "SystemEconomy", "Economy" });
            Economy_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemEconomy_Localised", "Economy_Localised" }), Economy);

            SecondEconomy = evt["SystemSecondEconomy"].Str();
            SecondEconomy_Localised = JournalFieldNaming.CheckLocalisation(evt["SystemSecondEconomy_Localised"].Str(), SecondEconomy);

            Government = evt.MultiStr(new string[] { "SystemGovernment", "Government" });
            Government_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemGovernment_Localised", "Government_Localised" }), Government);

            Security = evt.MultiStr(new string[] { "SystemSecurity", "Security" });
            Security_Localised = JournalFieldNaming.CheckLocalisation(evt.MultiStr(new string[] { "SystemSecurity_Localised", "Security_Localised" }), Security);

            Population = evt["Population"].LongNull();

            PowerplayState = evt["PowerplayState"].Str();            // NO evidence
            PowerplayPowers = evt["Powers"]?.ToObjectQ<string[]>();

            Wanted = evt["Wanted"].Bool();      // if absence, your not wanted, by definition of frontier in journal (only present if wanted, see docked)

            Conflicts = evt["Conflicts"]?.ToObjectQ<ConflictInfo[]>();   // 3.4

            ThargoidSystemState = evt["ThargoidWar"].ToObjectQ<ThargoidWar>();  // Odyssey update 15

            // Allegiance without Faction only occurs in Training
            if (!String.IsNullOrEmpty(Allegiance) && Faction == null && EventTimeUTC <= EliteFixesDates.ED_No_Training_Timestamp && (EventTimeUTC <= EliteFixesDates.ED_No_Faction_Timestamp || EventTypeID != JournalTypeEnum.FSDJump || StarSystem == "Eranin"))
            {
                IsTrainingEvent = true;
            }
        }

    }


    //When written: at startup, or when being resurrected at a station
    [JournalEntryType(JournalTypeEnum.Location)]
    public class JournalLocation : JournalLocOrJump, ISystemStationEntry, IBodyNameAndID, IStarScan, ICarrierStats
    {
        public JournalLocation(JObject evt) : base(evt, JournalTypeEnum.Location)      // all have evidence 16/3/2017
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            StationName = evt["StationName"].Str();
            StationType = evt["StationType"].Str().SplitCapsWord();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = JournalFieldNaming.NormaliseBodyType(evt["BodyType"].Str());
            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            Latitude = evt["Latitude"].DoubleNull();
            Longitude = evt["Longitude"].DoubleNull();

            MarketID = evt["MarketID"].LongNull();

            // station data only if docked..

            JObject jk = evt["StationFaction"].Object();  // 3.3.3 post

            if ( jk != null )
            {
                StationFaction = jk["Name"].Str();                // system faction pick up
                StationFactionState = jk["FactionState"].Str();
            }

            StationGovernment = evt["StationGovernment"].Str();       // 3.3.2 empty before
            StationGovernment_Localised = evt["StationGovernment_Localised"].Str();       // 3.3.2 empty before
            StationAllegiance = evt["StationAllegiance"].Str();       // 3.3.2 empty before
                                                                      // tbd bug in journal over FactionState - its repeated twice..
            StationServices = evt["StationServices"]?.ToObjectQ<string[]>();
            StationEconomyList = evt["StationEconomies"]?.ToObjectQ<JournalDocked.Economies[]>();

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
            InSRV = evt["InSRV"].BoolNull();
            OnFoot = evt["OnFoot"].BoolNull();
        }

        public bool Docked { get; set; }
        public string StationName { get; set; }
        public string StationType { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get; set; }
        public string BodyDesignation { get; set; }
        public double? DistFromStarLS { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public long? MarketID { get; set; }

        // 3.3.2 will be empty/null for previous logs.
        public string StationFaction { get; set; }
        public string StationFactionState { get; set; }
        public string StationGovernment { get; set; }
        public string StationGovernment_Localised { get; set; }
        public string StationAllegiance { get; set; }
        public string[] StationServices { get; set; }
        public JournalDocked.Economies[] StationEconomyList { get; set; }        // may be null

        //4.0 alpha 4
        public bool? Taxi { get; set; }
        public bool? Multicrew { get; set; }
        public bool? InSRV { get; set; }
        public bool? OnFoot { get; set; }

        public override string SummaryName(ISystem sys) 
        {
            if (Docked)
                return string.Format("At {0}".T(EDCTx.JournalLocation_AtStat), StationName);
            else
            {
                string bodyname = Body.HasChars() ? Body.ReplaceIfStartsWith(StarSystem) : StarSystem;
                if ( OnFoot == true )
                    return string.Format("On Foot at {0}".T(EDCTx.JournalEntry_OnFootAt), bodyname);
                else if (Latitude.HasValue && Longitude.HasValue)
                    return string.Format("Landed on {0}".T(EDCTx.JournalLocation_LND), bodyname);
                else
                    return string.Format("At {0}".T(EDCTx.JournalLocation_AtStar), bodyname);
            }

        }

        public override void FillInformation(out string info, out string detailed) 
        {
            if (Docked)
            {
                info = BaseUtils.FieldBuilder.Build("Type ".T(EDCTx.JournalLocOrJump_Type), StationType, "< in system ".T(EDCTx.JournalLocOrJump_insystem), StarSystem);

                detailed = BaseUtils.FieldBuilder.Build("<;(Wanted) ".T(EDCTx.JournalLocOrJump_Wanted), Wanted, "Faction: ".T(EDCTx.JournalLocOrJump_Faction), StationFaction, "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), Allegiance, "Economy: ".T(EDCTx.JournalLocOrJump_Economy), Economy_Localised, "Government: ".T(EDCTx.JournalLocOrJump_Government), Government_Localised, "Security: ".T(EDCTx.JournalLocOrJump_Security), Security_Localised);

                if (Factions != null)
                {
                    foreach (FactionInformation f in Factions)
                    {
                        detailed += Environment.NewLine;
                        detailed += BaseUtils.FieldBuilder.Build("", f.Name, "State: ".T(EDCTx.JournalLocOrJump_State), f.FactionState.SplitCapsWord(), "Government: ".T(EDCTx.JournalLocOrJump_Government), f.Government,
                            "Inf: ;%".T(EDCTx.JournalLocOrJump_Inf), (int)(f.Influence * 100), "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), f.Allegiance, "Happiness: ".T(EDCTx.JournalLocOrJump_Happiness), f.Happiness_Localised,
                            "Reputation: ;%;N1".T(EDCTx.JournalLocOrJump_Reputation), f.MyReputation,
                            ";Squadron System".T(EDCTx.JournalLocOrJump_SquadronSystem), f.SquadronFaction, 
                            ";Happiest System".T(EDCTx.JournalLocOrJump_HappiestSystem), f.HappiestSystem, 
                            ";Home System".T(EDCTx.JournalLocOrJump_HomeSystem), f.HomeSystem
                            );

                        if (f.PendingStates != null)
                        {
                            detailed += BaseUtils.FieldBuilder.Build(",", "Pending State: ".T(EDCTx.JournalLocOrJump_PendingState));
                            foreach (JournalLocation.PowerStatesInfo state in f.PendingStates)
                                detailed += BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend);

                            if (f.RecoveringStates != null)
                            {
                                detailed += BaseUtils.FieldBuilder.Build(",", "Recovering State: ".T(EDCTx.JournalLocOrJump_RecoveringState));
                                foreach (JournalLocation.PowerStatesInfo state in f.RecoveringStates)
                                    detailed += BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend);
                            }

                            if (f.ActiveStates != null)    
                            {
                                detailed += BaseUtils.FieldBuilder.Build(",", "Active State: ".T(EDCTx.JournalLocOrJump_ActiveState));
                                foreach (JournalLocation.ActiveStatesInfo state in f.ActiveStates)
                                    detailed += BaseUtils.FieldBuilder.Build(" ", state.State);
                            }
                        }
                    }
                }
            }
            else if (Latitude.HasValue && Longitude.HasValue)
            {
                info = "At " + JournalFieldNaming.RLat(Latitude.Value) + " " + JournalFieldNaming.RLong(Longitude.Value);
                detailed = "";
            }
            else 
            {
                info = "Near: ".T(EDCTx.JournalEntry_Near) + " " + BodyType + " " + Body;     // remove JournalLocOrJump_Inspacenear
                detailed = "";
            }
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddLocation(new SystemClass(SystemAddress, StarSystem, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }

        public void UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrier)
        {
            s.Update(this,onfootfleetcarrier);
        }
    }

    //When written: when jumping with a fleet carrier
    [JournalEntryType(JournalTypeEnum.CarrierJump)]
    public class JournalCarrierJump : JournalLocOrJump, ISystemStationEntry, IBodyNameAndID, IJournalJumpColor, IStarScan, ICarrierStats
    {
        public JournalCarrierJump(JObject evt) : base(evt, JournalTypeEnum.CarrierJump)
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            StationName = evt["StationName"].Str();
            StationType = evt["StationType"].Str().SplitCapsWord();
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = JournalFieldNaming.NormaliseBodyType(evt["BodyType"].Str());
            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            Latitude = evt["Latitude"].DoubleNull();
            Longitude = evt["Longitude"].DoubleNull();

            MarketID = evt["MarketID"].LongNull();

            // station data only if docked..

            JObject jk = evt["StationFaction"].Object();  // 3.3.3 post

            if (jk != null )
            {
                StationFaction = jk["Name"].Str();                // system faction pick up
                StationFactionState = jk["FactionState"].Str();
            }

            StationGovernment = evt["StationGovernment"].Str();       // 3.3.2 empty before
            StationGovernment_Localised = evt["StationGovernment_Localised"].Str();       // 3.3.2 empty before
            StationAllegiance = evt["StationAllegiance"].Str();       // 3.3.2 empty before
                                                                      // tbd bug in journal over FactionState - its repeated twice..
            StationServices = evt["StationServices"]?.ToObjectQ<string[]>();
            StationEconomyList = evt["StationEconomies"]?.ToObjectQ<JournalDocked.Economies[]>();

            JToken jm = evt["EDDMapColor"];
            MapColor = jm.Int(EDCommander.Current.MapColour);
            if (jm.IsNull())
                evt["EDDMapColor"] = EDCommander.Current.MapColour;      // new entries get this default map colour if its not already there
        }

        public bool Docked { get; set; }
        public string StationName { get; set; }
        public string StationType { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get; set; }
        public string BodyDesignation { get; set; }
        public double? DistFromStarLS { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public long? MarketID { get; set; }
        public int MapColor { get; set; }
        public System.Drawing.Color MapColorARGB { get { return System.Drawing.Color.FromArgb(MapColor); } }

        public string StationFaction { get; set; }
        public string StationFactionState { get; set; }
        public string StationGovernment { get; set; }
        public string StationGovernment_Localised { get; set; }
        public string StationAllegiance { get; set; }
        public string[] StationServices { get; set; }
        public JournalDocked.Economies[] StationEconomyList { get; set; }        // may be null

        public override string SummaryName(ISystem sys)
        {
            return string.Format("Jumped with carrier {0} to {1}".T(EDCTx.JournalCarrierJump_JumpedWith), StationName, Body);
        }
        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddLocation(new SystemClass(SystemAddress, StarSystem, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }


        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Type ".T(EDCTx.JournalLocOrJump_Type), StationType, "< in system ".T(EDCTx.JournalLocOrJump_insystem), StarSystem);

            detailed = BaseUtils.FieldBuilder.Build("<;(Wanted) ".T(EDCTx.JournalLocOrJump_Wanted), Wanted, "Faction: ".T(EDCTx.JournalLocOrJump_Faction), StationFaction, "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), Allegiance, "Economy: ".T(EDCTx.JournalLocOrJump_Economy), Economy_Localised, "Government: ".T(EDCTx.JournalLocOrJump_Government), Government_Localised, "Security: ".T(EDCTx.JournalLocOrJump_Security), Security_Localised);

            if (Factions != null)
            {
                foreach (FactionInformation f in Factions)
                {
                    detailed += Environment.NewLine;
                    detailed += BaseUtils.FieldBuilder.Build("", f.Name, "State: ".T(EDCTx.JournalLocOrJump_State), f.FactionState.SplitCapsWord(), "Government: ".T(EDCTx.JournalLocOrJump_Government), f.Government,
                        "Inf: ;%".T(EDCTx.JournalLocOrJump_Inf), (int)(f.Influence * 100), "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), f.Allegiance, "Happiness: ".T(EDCTx.JournalLocOrJump_Happiness), f.Happiness_Localised,
                        "Reputation: ;%;N1".T(EDCTx.JournalLocOrJump_Reputation), f.MyReputation,
                        ";Squadron System".T(EDCTx.JournalLocOrJump_SquadronSystem), f.SquadronFaction,
                        ";Happiest System".T(EDCTx.JournalLocOrJump_HappiestSystem), f.HappiestSystem,
                        ";Home System".T(EDCTx.JournalLocOrJump_HomeSystem), f.HomeSystem
                        );

                    if (f.PendingStates != null)
                    {
                        detailed += BaseUtils.FieldBuilder.Build(",", "Pending State: ".T(EDCTx.JournalLocOrJump_PendingState));
                        foreach (JournalLocation.PowerStatesInfo state in f.PendingStates)
                            detailed += BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend);

                        if (f.RecoveringStates != null)
                        {
                            detailed += BaseUtils.FieldBuilder.Build(",", "Recovering State: ".T(EDCTx.JournalLocOrJump_RecoveringState));
                            foreach (JournalLocation.PowerStatesInfo state in f.RecoveringStates)
                                detailed += BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend);
                        }

                        if (f.ActiveStates != null)
                        {
                            detailed += BaseUtils.FieldBuilder.Build(",", "Active State: ".T(EDCTx.JournalLocOrJump_ActiveState));
                            foreach (JournalLocation.ActiveStatesInfo state in f.ActiveStates)
                                detailed += BaseUtils.FieldBuilder.Build(" ", state.State);
                        }
                    }
                }
            }
        }

        public void UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.FSDJump)]
    public class JournalFSDJump : JournalLocOrJump, IShipInformation, ISystemStationEntry, IJournalJumpColor, IStarScan
    {
        public JournalFSDJump(JObject evt) : base(evt, JournalTypeEnum.FSDJump)
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            JumpDist = evt["JumpDist"].Double();
            FuelUsed = evt["FuelUsed"].Double();
            FuelLevel = evt["FuelLevel"].Double();
            BoostUsed = evt["BoostUsed"].Int();         
            Body = evt["Body"].StrNull();
            BodyID = evt["BodyID"].IntNull();
            BodyType = JournalFieldNaming.NormaliseBodyType(evt["BodyType"].Str());

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();

            JToken jm = evt["EDDMapColor"];
            MapColor = jm.Int(EDCommander.Current.MapColour);
            if (jm.IsNull())
                evt["EDDMapColor"] = EDCommander.Current.MapColour;      // new entries get this default map colour if its not already there

            EDSMFirstDiscover = evt["EDD_EDSMFirstDiscover"].Bool(false);
        }

        public JournalFSDJump(DateTime utc, ISystem sys, int colour, bool first, bool edsmsynced) : base(utc, sys, JournalTypeEnum.FSDJump, edsmsynced)
        {
            MapColor = colour;
            EDSMFirstDiscover = first;
        }

        public double JumpDist { get; set; }
        public double FuelUsed { get; set; }
        public double FuelLevel { get; set; }
        public int BoostUsed { get; set; }          // 1 = basic, 2 = standard, 3 = premium, 4 = ?
        public int MapColor { get; set; }
        public System.Drawing.Color MapColorARGB { get { return System.Drawing.Color.FromArgb(MapColor); } }
        public bool EDSMFirstDiscover { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get; set; }

        public bool? Taxi { get; set; }
        public bool? Multicrew { get; set; }

        public override string SummaryName(ISystem sys) { return string.Format("Jump to {0}".T(EDCTx.JournalFSDJump_Jumpto), StarSystem); }
        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddLocation(new SystemClass(SystemAddress, StarSystem, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }

        public override void FillInformation(out string info, out string detailed)
        {
            StringBuilder sb = new StringBuilder();

            if (JumpDist > 0)
                sb.Append(JumpDist.ToString("N2") + " ly");
            if (FuelUsed > 0)
                sb.Append(", Fuel ".T(EDCTx.JournalFSDJump_Fuel) + FuelUsed.ToString("N2") + "t");
            if (FuelLevel > 0)
                sb.Append(" left ".T(EDCTx.JournalFSDJump_left) + FuelLevel.ToString("N2") + "t");

            string econ = Economy_Localised.Alt(Economy);
            if (econ.Equals("None"))
                econ = "";

            sb.Append(" ");
            sb.Append(BaseUtils.FieldBuilder.Build("Faction: ".T(EDCTx.JournalLocOrJump_Faction), Faction, "<;(Wanted) ".T(EDCTx.JournalLocOrJump_Wanted), Wanted,
                                                    "State: ".T(EDCTx.JournalLocOrJump_State), FactionState, "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), Allegiance,
                                                    "Economy: ".T(EDCTx.JournalLocOrJump_Economy), econ, "Population: ".T(EDCTx.JournalLocOrJump_Population), Population));
            info = sb.ToString();

            sb.Clear();

            if (Factions != null)
            {
                foreach (FactionInformation i in Factions)
                {
                    sb.Append(BaseUtils.FieldBuilder.Build("", i.Name, "State: ".T(EDCTx.JournalLocOrJump_State), i.FactionState,
                                                                    "Government: ".T(EDCTx.JournalLocOrJump_Government), i.Government,
                                                                    "Inf: ;%".T(EDCTx.JournalLocOrJump_Inf), (i.Influence * 100.0).ToString("0.0"),
                                                                    "Allegiance: ".T(EDCTx.JournalLocOrJump_Allegiance), i.Allegiance,
                                                                    "Happiness: ".T(EDCTx.JournalLocOrJump_Happiness), i.Happiness_Localised,
                                                                    "Reputation: ;%;N1".T(EDCTx.JournalLocOrJump_Reputation), i.MyReputation,
                                                                    ";Squadron System".T(EDCTx.JournalLocOrJump_SquadronSystem), i.SquadronFaction, 
                                                                    ";Happiest System".T(EDCTx.JournalLocOrJump_HappiestSystem), i.HappiestSystem, 
                                                                    ";Home System".T(EDCTx.JournalLocOrJump_HomeSystem), i.HomeSystem
                                                                    ));

                    if (i.PendingStates != null)
                    {
                        sb.Append(BaseUtils.FieldBuilder.Build(",", "Pending State: ".T(EDCTx.JournalLocOrJump_PendingState)));

                        foreach (JournalLocation.PowerStatesInfo state in i.PendingStates)
                            sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend));

                    }

                    if (i.RecoveringStates != null)
                    {
                        sb.Append(BaseUtils.FieldBuilder.Build(",", "Recovering State: ".T(EDCTx.JournalLocOrJump_RecoveringState)));

                        foreach (JournalLocation.PowerStatesInfo state in i.RecoveringStates)
                            sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State, "<(;)", state.Trend));
                    }

                    if (i.ActiveStates != null)
                    {
                        sb.Append(BaseUtils.FieldBuilder.Build(",", "Active State: ".T(EDCTx.JournalLocOrJump_ActiveState)));

                        foreach (JournalLocation.ActiveStatesInfo state in i.ActiveStates)
                            sb.Append(BaseUtils.FieldBuilder.Build(" ", state.State));
                    }
                    sb.Append(Environment.NewLine);

                }
            }

            detailed = sb.ToString();
        }

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.FSDJump(this);
        }

        public void SetMapColour(int mapcolour)
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                JObject jo = GetJson(Id, cn);

                if (jo != null)
                {
                    jo["EDDMapColor"] = mapcolour;
                    UpdateJsonEntry(jo, cn);
                    MapColor = mapcolour;
                }
            });
        }

        public void UpdateFirstDiscover(bool value)
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                UpdateFirstDiscover(value, cn);
            });
        }

        internal void UpdateFirstDiscover(bool value, SQLiteConnectionUser cn, DbTransaction txnl = null)
        {
            JObject jo = GetJson(Id, cn, txnl);

            if (jo != null)
            {
                jo["EDD_EDSMFirstDiscover"] = value;
                UpdateJsonEntry(jo, cn, txnl);
                EDSMFirstDiscover = value;
            }
        }

        public JObject CreateFSDJournalEntryJson()          // minimal version, not the whole schebang
        {
            JObject jo = new JObject();
            jo["timestamp"] = EventTimeUTC.ToStringZuluInvariant();
            jo["event"] = "FSDJump";
            jo["StarSystem"] = StarSystem;
            jo["StarPos"] = new JArray(StarPos.X, StarPos.Y, StarPos.Z);
            jo["EDDMapColor"] = MapColor;
            jo["EDD_EDSMFirstDiscover"] = EDSMFirstDiscover;
            return jo;
        }
    }


    [JournalEntryType(JournalTypeEnum.FSDTarget)]
    public class JournalFSDTarget : JournalEntry
    {
        public JournalFSDTarget(JObject evt) : base(evt, JournalTypeEnum.FSDTarget)
        {
            StarSystem = evt["Name"].Str();
            StarClass = evt["StarClass"].Str();
            SystemAddress = evt["SystemAddress"].Long();
            RemainingJumpsInRoute = evt["RemainingJumpsInRoute"].IntNull();
            FriendlyStarClass = (StarClass.Length > 0) ? Bodies.StarName(Bodies.StarStr2Enum(StarClass)) : "";
        }

        public string StarSystem { get; set; }
        public string StarClass { get; set; }
        public long SystemAddress { get; set; }
        public int? RemainingJumpsInRoute { get; set; }
        public string FriendlyStarClass { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", StarSystem,"",StarClass,"Remaining Jumps".T(EDCTx.JournalEntry_RemainingJumps), RemainingJumpsInRoute);
            detailed = "";
        }
    }



    [JournalEntryType(JournalTypeEnum.StartJump)]
    public class JournalStartJump : JournalEntry, IStarScan
    {
        public JournalStartJump(JObject evt) : base(evt, JournalTypeEnum.StartJump)
        {
            JumpType = evt["JumpType"].Str();
            IsHyperspace = JumpType.Equals("Hyperspace", System.StringComparison.InvariantCultureIgnoreCase);
            StarSystem = evt["StarSystem"].Str();
            StarClass = evt["StarClass"].Str();
            FriendlyStarClass = (StarClass.Length > 0) ? Bodies.StarName(Bodies.StarStr2Enum(StarClass)) : "";
            SystemAddress = evt["SystemAddress"].LongNull();
            InTaxi = evt["Taxi"].BoolNull();
        }

        public string JumpType { get; set; }            // Hyperspace, Supercruise
        public bool IsHyperspace { get; set; }
        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string StarClass { get; set; }
        public string FriendlyStarClass { get; set; }
        public bool? InTaxi { get; set; }        // update 15+

        public override string SummaryName(ISystem sys) { return "Charging FSD".T(EDCTx.JournalStartJump_ChargingFSD); }

        public override void FillInformation(out string info, out string detailed)
        {
            if (IsHyperspace)
                info = "Hyperspace".T(EDCTx.JournalEntry_Hyperspace) + BaseUtils.FieldBuilder.Build("< to ".T(EDCTx.JournalEntry_to), StarSystem, "", FriendlyStarClass);
            else
                info = "Supercruise".T(EDCTx.JournalEntry_Supercruise);

            detailed = "";
        }
        public void AddStarScan(StarScan s, ISystem system)
        {
            if ( IsHyperspace )
                s.AddLocation(new SystemClass(SystemAddress, StarSystem));      // add so there is placeholder
        }
    }
}
