/*
 * Copyright © 2016-2024 EDDiscovery development team
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
using EliteDangerousCore.DB;
using System.Linq;
using System.Text;
using System.Data.Common;
using EliteDangerousCore.StarScan2;

namespace EliteDangerousCore.JournalEvents
{
    public abstract class JournalLocOrJump : JournalEntry, IStatsJournalEntry
    {
        public string StarSystem { get; set; }
        public EMK.LightGeometry.Vector3 StarPos { get; set; }
        public long? SystemAddress { get; set; }
        public SystemSource LocOrJumpSource { get; set; } = SystemSource.FromJournal;     // this is the default..

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

        public bool HasFactionConflictThargoidInfo { get { return Factions != null || Conflicts != null || ThargoidSystemState != null; } }

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

    // allows commonality of information between Location (when docked) and Docked events
    public interface ILocDocked
    {
        bool Docked { get; }
        string StarSystem { get; }
        long? SystemAddress { get; }
        string StationName { get;  }      
        string StationName_Localised { get; }          
        StationDefinitions.StarportTypes FDStationType { get;  }  // only on later events, else Unknown
        string StationType { get; } // english, only on later events, else Unknown
        long? MarketID { get;  }
        StationDefinitions.Classification MarketClass();
        string StationFaction { get;}
        FactionDefinitions.State StationFactionState { get; }       //may be null, FDName
        string StationFactionStateTranslated { get;  }
        GovernmentDefinitions.Government StationGovernment { get;  }
        string StationGovernment_Localised { get; }
        AllegianceDefinitions.Allegiance StationAllegiance { get;  }   // fdname
        StationDefinitions.StationServices[] StationServices { get; }   // may be null
        EconomyDefinitions.Economies[] StationEconomyList { get;}        // may be null
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //When written: at startup, or when being resurrected at a station
    [JournalEntryType(JournalTypeEnum.Location)]
    public class JournalLocation : JournalLocOrJump, IBodyNameAndID, IStarScan, ILocDocked
    {
        public JournalLocation(JObject evt) : base(evt, JournalTypeEnum.Location)      // all have evidence 16/3/2017
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            if (Docked)
            {
                var snl = JournalFieldNaming.GetStationNames(evt);
                StationName = snl.Item1;
                StationName_Localised = snl.Item2;
                string st = evt["StationType"].StrNull();
                if (st != null && st.Length == 0)        // seem empty ones
                    st = null;
                FDStationType = StationDefinitions.StarportTypeToEnum(st);    // may not be there
                StationType = StationDefinitions.ToEnglish(FDStationType);
            }
            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = BodyDefinitions.GetBodyType(evt["BodyType"].Str());
            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            Latitude = evt["Latitude"].DoubleNull();
            Longitude = evt["Longitude"].DoubleNull();

            MarketID = evt["MarketID"].LongNull();

            // station data only if docked..

            JObject jk = evt["StationFaction"].Object();  // 3.3.3 post

            if ( jk != null )
            {
                StationFaction = jk["Name"].Str();                // system faction pick up
                StationFactionState = FactionDefinitions.FactionStateToEnum(jk["FactionState"].Str("Unknown")).Value;

                if (StationFactionState == FactionDefinitions.State.Unknown && Factions != null) // no data on this in event, but we have factions..
                {
                    int i = Array.FindIndex(Factions, x => x.Name == Faction);
                    if (i != -1)
                        StationFactionState = Factions[i].FactionState;        // set to State of controlling faction
                }

                StationFactionStateTranslated = FactionDefinitions.ToLocalisedLanguage(StationFactionState); // null if not present
            }
            else
            {
            }

            StationGovernment = GovernmentDefinitions.ToEnum(evt["StationGovernment"].StrNull());       // may be missing
            StationGovernment_Localised = JournalFieldNaming.CheckLocalisation(evt["StationGovernment_Localised"].Str(), GovernmentDefinitions.ToEnglish(Government));

            StationAllegiance = AllegianceDefinitions.ToEnum(evt["StationAllegiance"].StrNull());    // may be missing, due to training

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);          // need to read thru these functions to do some name mangling
            StationEconomyList = EconomyDefinitions.ReadEconomiesClassFromJson(evt["StationEconomies"]);

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();
            InSRV = evt["InSRV"].BoolNull();
            OnFoot = evt["OnFoot"].BoolNull();
        }

        public bool Docked { get; set; }
        public string StationName { get; set; } // will be null if not docked, 
        public string StationName_Localised { get; set; } // will be null if not docked
        public string StationType { get; set; } // will be null if not docked, english name
        public StationDefinitions.StarportTypes FDStationType { get; set; } // will be Unknown if not docked
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public BodyDefinitions.BodyType BodyType { get; set; }
        public double? DistFromStarLS { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public long? MarketID { get; set; }
        public StationDefinitions.Classification MarketClass() { return MarketID != null ? StationDefinitions.Classify(MarketID.Value, FDStationType) : StationDefinitions.Classification.Unknown; }

        // 3.3.2 will be empty/null for previous logs.
        public string StationFaction { get; set; }  
        public FactionDefinitions.State StationFactionState { get; set; } // fdname
        public string StationFactionStateTranslated { get; set; }
        public GovernmentDefinitions.Government StationGovernment { get; set; }
        public string StationGovernment_Localised { get; set; }
        public AllegianceDefinitions.Allegiance StationAllegiance { get; set; }   // fdname
        public StationDefinitions.StationServices[] StationServices { get; set; }   // may be null
        public EconomyDefinitions.Economies[] StationEconomyList { get; set; }        // may be null

        //4.0 alpha 4
        public bool? Taxi { get; set; }
        public bool? Multicrew { get; set; }
        public bool? InSRV { get; set; }
        public bool? OnFoot { get; set; }

        public override string SummaryName(ISystem sys)     // Location
        {
            if (Docked)
                return string.Format("At {0}".Tx(), StationName_Localised);
            else
            {
                string bodyname = Body.HasChars() ? Body.ReplaceIfStartsWith(StarSystem) : StarSystem;
                if ( OnFoot == true )
                    return string.Format("On Foot at {0}".Tx(), bodyname);
                else if (Latitude.HasValue && Longitude.HasValue)
                    return string.Format("Landed on {0}".Tx(), bodyname);
                else
                    return string.Format("At {0}".Tx(), bodyname);
            }

        }

        public override string GetInfo()        // Location
        {
            if (Docked)
            {
                return BaseUtils.FieldBuilder.Build("Type ".Tx(), StationDefinitions.ToLocalisedLanguage(FDStationType),
                            "< in system ".Tx(), StarSystem);
            }
            else if (Latitude.HasValue && Longitude.HasValue)
            {
                return BaseUtils.FieldBuilder.Build("", Body, "< @ " + "Latitude: ;°;F4".Tx(), Latitude, "Longitude: ;°;F4".Tx(), Longitude);
            }
            else
            {
                return "Near".Tx()+": "+ " " + BodyType + " " + Body;     // remove JournalLocOrJump_Inspacenear
            }
        }

        public override string GetDetailed() // Location
        {
            StringBuilder sb = new StringBuilder();

            if (Docked)
            {
                sb.Build("<;(Wanted) ".Tx(), Wanted,
                        "Faction".Tx()+": ", StationFaction,
                        "State".Tx()+": ", StationFactionStateTranslated,
                        "Allegiance".Tx()+": ", AllegianceDefinitions.ToLocalisedLanguage(StationAllegiance),
                        "Economy".Tx()+": ", EconomyDefinitions.ToLocalisedLanguage(Economy),
                        "Government".Tx()+": ", GovernmentDefinitions.ToLocalisedLanguage(Government),
                        "Security".Tx()+": ", SecurityDefinitions.ToLocalisedLanguage(Security));
            }

            if (HasPowerPlayInfo)
            {
                sb.AppendCR();
                FillPowerInfo(sb);
            }

            if (HasFactionConflictThargoidInfo)
            {
                sb.AppendCR();
                FillFactionConflictThargoidPowerPlayConflictInfo(sb);
            }

            return sb.Length>0 ? sb.ToString() : null;
        }

        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            var sys = new SystemClass(StarSystem, SystemAddress, StarPos.X, StarPos.Y, StarPos.Z);
            s.GetOrAddSystem( sys);     // we use our data to fill in 
            s.AddBody(this, sys);
        }

    }


    [JournalEntryType(JournalTypeEnum.FSDJump)]
    public class JournalFSDJump : JournalLocOrJump, IShipInformation, IJournalJumpColor, IStarScan
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
            BodyType = BodyDefinitions.GetBodyType(evt["BodyType"].Str());

            Taxi = evt["Taxi"].BoolNull();
            Multicrew = evt["Multicrew"].BoolNull();

            JToken jm = evt["EDDMapColor"];
            MapColor = jm.Int(EDCommander.Current.MapColour);
            if (jm.IsNull())
                evt["EDDMapColor"] = EDCommander.Current.MapColour;      // new entries get this default map colour if its not already there
        }

        public JournalFSDJump(DateTime utc, ISystem sys, int colour, bool edsmsynced) : base(utc, sys, JournalTypeEnum.FSDJump, edsmsynced)
        {
            MapColor = colour;
        }

        public double JumpDist { get; set; }
        public double FuelUsed { get; set; }
        public double FuelLevel { get; set; }
        public int BoostUsed { get; set; }          // 1 = basic (25% x1.25), 2 = standard (50% x1.5), 3 = premium (100% x2 ), 4 = neutron (x4)
        public int MapColor { get; set; }
        public System.Drawing.Color MapColorARGB { get { return System.Drawing.Color.FromArgb(MapColor); } }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public BodyDefinitions.BodyType BodyType { get; set; }

        public bool? Taxi { get; set; }
        public bool? Multicrew { get; set; }

        public override string SummaryName(ISystem sys) { return string.Format("Jump to {0}".Tx(), StarSystem); }
        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            s.GetOrAddSystem(new SystemClass(StarSystem, SystemAddress, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }

        public override string GetInfo()        // fsdjump
        {
            double? tempdist = JumpDist > 0 ? JumpDist : default(double?);
            double? tempused = FuelUsed > 0 ? FuelUsed : default(double?);
            double? templevel = FuelLevel > 0 ? FuelLevel : default(double?);

            var sb = new System.Text.StringBuilder(256);

            sb.Build("; ly;N2".Tx(), tempdist, "Fuel used: ; t;N2".Tx(), tempused, "Fuel left: ; t;N2".Tx(), templevel);

            if (Faction.HasChars() || Allegiance != AllegianceDefinitions.Allegiance.Unknown || Economy != EconomyDefinitions.Economy.Unknown)
            {
                sb.BuildCont(
                    "Faction".Tx()+": ", Faction, "<;(Wanted) ".Tx(), Wanted,
                    "State".Tx()+": ", FactionDefinitions.ToLocalisedLanguage(FactionState),
                    "Allegiance".Tx()+": ", AllegianceDefinitions.ToLocalisedLanguage(Allegiance),
                    "Economy".Tx()+": ", EconomyDefinitions.ToLocalisedLanguage(Economy),
                    "Population".Tx()+": ", Population);
            }
            
            if (HasPowerPlayInfo)
            {
                sb.AppendCR();
                FillPowerInfo(sb);
            }

            return sb.ToString();
        }

        public override string GetDetailed()        // fsdjump
        {
            StringBuilder sb = new StringBuilder();
            FillFactionConflictThargoidPowerPlayConflictInfo(sb);
            return sb.Length>0 ? sb.ToString() : null;
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
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
                    UpdateJsonEntry(jo, cn, null);
                    MapColor = mapcolour;
                }
            });
        }

        public JObject CreateJsonOfFSDJournalEntry()          // minimal version, not the whole schebang
        {
            JObject jo = new JObject();
            jo["timestamp"] = EventTimeUTC.ToStringZuluInvariant();
            jo["event"] = "FSDJump";
            jo["StarSystem"] = StarSystem;
            jo["StarPos"] = new JArray(StarPos.X, StarPos.Y, StarPos.Z);
            jo["EDDMapColor"] = MapColor;
            jo["StarPosFromEDSM"] = true;       // mark as non journal sourced
            return jo;
        }
    }


    [JournalEntryType(JournalTypeEnum.FSDTarget)]
    public class JournalFSDTarget : JournalEntry, IStarScan
    {
        public JournalFSDTarget(JObject evt) : base(evt, JournalTypeEnum.FSDTarget)
        {
            StarSystem = evt["Name"].Str();
            StarClass = evt["StarClass"].Str();
            EDStarClass = Stars.ToEnum(StarClass);
            SystemAddress = evt["SystemAddress"].Long();
            RemainingJumpsInRoute = evt["RemainingJumpsInRoute"].IntNull();
            FriendlyStarClass = (StarClass.Length > 0) ? Stars.StarName(Stars.ToEnum(StarClass)) : "";
        }

        public string StarSystem { get; set; }
        public string StarClass { get; set; }
        public EDStar EDStarClass { get; set; }
        public long SystemAddress { get; set; }
        public int? RemainingJumpsInRoute { get; set; }
        public string FriendlyStarClass { get; set; }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.GetOrAddSystem(new SystemClass(StarSystem, SystemAddress));
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", StarSystem,"",StarClass,"Remaining Jumps".Tx(), RemainingJumpsInRoute);
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
            EDStarClass = Stars.ToEnum(StarClass);
            FriendlyStarClass = (StarClass.Length > 0) ? Stars.StarName(Stars.ToEnum(StarClass)) : "";
            SystemAddress = evt["SystemAddress"].LongNull();
            InTaxi = evt["Taxi"].BoolNull();
        }

        public string JumpType { get; set; }            // Hyperspace, Supercruise
        public bool IsHyperspace { get; set; }
        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string StarClass { get; set; }       
        public EDStar EDStarClass { get; set; }
        public string FriendlyStarClass { get; set; }
        public bool? InTaxi { get; set; }        // update 15+

        public override string SummaryName(ISystem sys) { return "Charging FSD".Tx(); }

        public override string GetInfo()
        {
            if (IsHyperspace)
                return "Hyperspace".Tx()+ BaseUtils.FieldBuilder.Build("< to ".Tx(), StarSystem, "", FriendlyStarClass);
            else
                return "Supercruise".Tx();
        }

        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            if (IsHyperspace)
                s.GetOrAddSystem( new SystemClass(StarSystem, SystemAddress));      // add so there is placeholder
        }
    }
}
