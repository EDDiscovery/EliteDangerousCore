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
using EliteDangerousCore.DB;
using System.Text;
using EliteDangerousCore.StarScan2;

namespace EliteDangerousCore.JournalEvents
{
    [System.Diagnostics.DebuggerDisplay("FSDJump {EventTimeUTC} JID {Id} `{BodyName}` @ `{StarSystem}`: `{BodyName}`")]
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
            FriendlyStarClass = (StarClass.Length > 0) ? Stars.ToLocalisedLanguage(Stars.ToEnum(StarClass)) : "";
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
            FriendlyStarClass = (StarClass.Length > 0) ? Stars.ToLocalisedLanguage(Stars.ToEnum(StarClass)) : "";
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
            {
                var sys = s.GetOrAddSystem(new SystemClass(StarSystem, SystemAddress));      // add so there is placeholder
                sys?.SetStarClass(EDStarClass);
            }
        }
    }


    //When written: when jumping with a fleet carrier
    [JournalEntryType(JournalTypeEnum.CarrierJump)]
    public class JournalCarrierJump : JournalLocOrJump, IJournalJumpColor, IStarScan, ICarrierStats
    {
        public JournalCarrierJump(JObject evt) : base(evt, JournalTypeEnum.CarrierJump)
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            var snl = JournalFieldNaming.GetStationNames(evt);
            Name = StationName = snl.Item1;                         // we set both StationName and IbodyName to the same value
            Name_Localised = StationName_Localised = snl.Item2;

            // keep type in case they introduce more than 1 carrier type
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].Str());
            StationType = StationDefinitions.ToEnglish(FDStationType);

            MarketID = evt["MarketID"].LongNull();

            // don't bother with StationGovernment, StationFaction, StationEconomy, StationEconomies

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);

            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            JToken jm = evt["EDDMapColor"];
            MapColor = jm.Int(EDCommander.Current.MapColour);
            if (jm.IsNull())
                evt["EDDMapColor"] = EDCommander.Current.MapColour;      // new entries get this default map colour if its not already there
        }

        public CarrierDefinitions.CarrierType CarrierType { get; } = CarrierDefinitions.CarrierType.UnknownType;        // stupid journal does not tell
        public string StationName { get; set; }         // from aprilish 2025, we get these even if we are not on it. Will be blank
        public string StationName_Localised { get; set; }
        public string StationType { get; set; } // friendly station type
        public double? DistFromStarLS { get; set; }
        public int MapColor { get; set; }
        public System.Drawing.Color MapColorARGB { get { return System.Drawing.Color.FromArgb(MapColor); } }

        public StationDefinitions.StationServices[] StationServices { get; set; }
        public EconomyDefinitions.Economies[] StationEconomyList { get; set; }        // may be null

        public override string SummaryName(ISystem sys)
        {
            if (Docked)
                return string.Format("Jumped with carrier {0} to {1}".Tx(), StationName, Body);
            else
                return string.Format("Carrier jumped to {0}".Tx(), Body);
        }
        public void AddStarScan(StarScan2.StarScan s, ISystem system)
        {
            s.GetOrAddSystem(new SystemClass(StarSystem, SystemAddress, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }

        public override string GetInfo()        // carrier jump
        {
            return BaseUtils.FieldBuilder.Build("", Body ?? StarSystem);
        }

        public override string GetDetailed()    // carrier jump
        {
            StringBuilder sb = new StringBuilder();
            sb.Build("<;(Wanted) ".Tx(), Wanted);

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

            return sb.ToString();
        }

        public void UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }



}
