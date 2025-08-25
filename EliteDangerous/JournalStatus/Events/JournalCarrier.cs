/*
 * Copyright © 2022-2022 EDDiscovery development team
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
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    // holds carrier state
    public class CarrierState
    {
        public CarrierState() { }
        public CarrierState(CarrierState other)     // copy constructor
        {
            CarrierID = other.CarrierID;
            Callsign = other.Callsign;
            Name = other.Name;
            CarrierType = other.CarrierType;
            DockingAccess = other.DockingAccess;
            AllowNotorious = other.AllowNotorious;
            FuelLevel = other.FuelLevel;
            JumpRangeCurr = other.JumpRangeCurr;
            JumpRangeMax = other.JumpRangeMax;
            PendingDecommission = other.PendingDecommission;

            SpaceUsage = new SpaceUsageClass(other.SpaceUsage);
            Finance = new FinanceClass(other.Finance);

            if (other.Services != null)
            {
                Services = new List<ServicesClass>(other.Services);         // Crew are values, can be copied
            }
            if (other.ShipPacks != null)
            {
                ShipPacks = new List<PackClass>(other.ShipPacks);
            }
            if (other.ModulePacks != null)
            {
                ModulePacks = new List<PackClass>(other.ModulePacks);
            }
        }

        public bool HaveCarrier { get { return CarrierID != 0 && Callsign != null; } }      // set if we have ever bought a carrier, even if decommissioned

        public long CarrierID { get; set; }     // carrier buy also sets this
        public string Callsign { get; set; }    // carrier buy also sets this
        public string Name { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }

        public string DockingAccess { get; set; }
        public string DockingAccessSplittable { get { return DockingAccess == "squadronfriends" ? "Squadron Friends" : DockingAccess; } }

        public bool AllowNotorious { get; set; }
        public int FuelLevel { get; set; }
        public double JumpRangeCurr { get; set; }
        public double JumpRangeMax { get; set; }
        public bool PendingDecommission { get; set; }

        [System.Diagnostics.DebuggerDisplay("Space Usage {TotalCapacity} {Crew} {Cargo} {CargoSpaceReserved} {ShipPacks} {ModulePacks} {FreeSpace}")]
        public class SpaceUsageClass
        {
            public SpaceUsageClass(){}
            public SpaceUsageClass(SpaceUsageClass other)
            {
                TotalCapacity = other.TotalCapacity;
                Crew = other.Crew;
                Cargo = other.Cargo;
                CargoSpaceReserved = other.CargoSpaceReserved;
                ShipPacks = other.ShipPacks;
                ModulePacks = other.ModulePacks;
                FreeSpace = other.FreeSpace;
            }

            public int TotalCapacity { get; set; }
            public int Crew { get; set; }
            public int Cargo { get; set; }
            public int CargoSpaceReserved { get; set; }
            public int ShipPacks { get; set; }
            public int ModulePacks { get; set; }
            public int FreeSpace { get; set; }
        };

        public SpaceUsageClass SpaceUsage { get; set; } = new SpaceUsageClass();

        [System.Diagnostics.DebuggerDisplay("Finance {CarrierBalance} r{ReserveBalance} a{AvailableBalance}")]
        public class FinanceClass
        {
            public FinanceClass() { }
            public FinanceClass(FinanceClass other)
            {
                CarrierBalance = other.CarrierBalance;
                ReserveBalance = other.ReserveBalance;
                AvailableBalance = other.AvailableBalance;
                ReservePercent = other.ReservePercent;
                TaxRatePioneersupplies = other.TaxRatePioneersupplies;
                TaxRateShipyard = other.TaxRateShipyard;
                TaxRateRearm = other.TaxRateRearm;
                TaxRateOutfitting = other.TaxRateOutfitting;
                TaxRateRefuel = other.TaxRateRefuel;
                TaxRateRepair = other.TaxRateRepair;
            }
            public long CarrierBalance { get; set; }
            public long ReserveBalance { get; set; }
            public long AvailableBalance { get; set; }
            public double ReservePercent { get; set; }
            public double? TaxRatePioneersupplies { get; set; }     // tax rates may be missing
            public double? TaxRateShipyard { get; set; }
            public double? TaxRateRearm { get; set; }
            public double? TaxRateOutfitting { get; set; }
            public double? TaxRateRefuel { get; set; }
            public double? TaxRateRepair { get; set; }
        }

        public FinanceClass Finance { get; set; } = new FinanceClass();

        [System.Diagnostics.DebuggerDisplay("Services {CrewRole} {CrewName} a{Activated} e{Enabled}")]
        public class ServicesClass
        {
            public string CrewRole { get; set; }
            public bool Activated { get; set; }
            public bool Enabled { get; set; }
            public string CrewName { get; set; }
            public JournalCarrierCrewServices.ServiceType ServiceType() { return JournalCarrierCrewServices.GetServiceType(CrewRole); }
        }

        public List<ServicesClass> Services { get; set; }       // may be null - called 'Crew' in journal buts its all about services
        public ServicesClass GetService(JournalCarrierCrewServices.ServiceType t) // may be null.  Core services are not listed
        {
            return Services?.Find(x => x.ServiceType() == t);
        }

        public long GetServicesCost()
        {
            long res = 0;
            foreach( var s in Services.EmptyIfNull())
            {
                JournalCarrierCrewServices.ServicesData si = JournalCarrierCrewServices.GetDataOnServiceType(s.ServiceType());
                if (si != null)
                {
                    long delta = s.Activated ? (s.Enabled ? si.UpkeepCost : si.SuspendedUpkeepCost) : 0;
                    System.Diagnostics.Debug.WriteLine($"Service cost {si.Service} {si.UpkeepCost} {si.SuspendedUpkeepCost} = {delta}");
                    res += delta;
                }
            }

            return res;
        }

        public long GetCoreCost() { return 5000000; }

        [System.Diagnostics.DebuggerDisplay("Pack {PackTheme} {PackTier}")]
        public class PackClass
        {
            public string PackTheme { get; set; }
            public int PackTier { get; set; }
        }


        public List<PackClass> ShipPacks { get; set; }  // may be null
        public int ShipPacksCount() { return ShipPacks?.Count() ?? 0; }
        public List<PackClass> ModulePacks { get; set; }    // may be null
        public int ModulePacksCount() { return ModulePacks?.Count() ?? 0; }

    }

    [JournalEntryType(JournalTypeEnum.CarrierBuy)]
    public class JournalCarrierBuy : JournalEntry, ILedgerJournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public long BoughtAtMarket { get; set; }        // market id 
        public string Location { get; set; }        // starsystem
        public long SystemAddress { get; set; }
        public long Price { get; set; }
        public string Variant { get; set; }
        public string Callsign { get; set; }

        public JournalCarrierBuy(JObject evt) : base(evt, JournalTypeEnum.CarrierBuy)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            BoughtAtMarket = evt["BoughtAtMarket"].Long();
            Location = evt["Location"].Str();
            SystemAddress = evt["SystemAddress"].Long();
            Price = evt["Price"].Long();
            Variant = evt["Variant"].Str();
            Callsign = evt["Callsign"].Str();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("At ".T(EDCTx.JournalCarrier_At), Location,
                                              "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Price,
                                              "Call Sign: ".T(EDCTx.JournalCarrier_Callsign), Callsign);
        }

        public void Ledger(Ledger mcl)
        {
            string x = "Call Sign: ".T(EDCTx.JournalCarrier_Callsign) + Callsign;
            mcl.AddEvent(Id, EventTimeUTC, JournalTypeEnum.CarrierBuy, x, -Price);
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierStats)]
    
    public class JournalCarrierStats : JournalEntry, ICarrierStats
    {
        public CarrierState State { get; private set; }
        public CarrierDefinitions.CarrierType CarrierType { get { return State.CarrierType; } }

        public JournalCarrierStats(JObject evt) : base(evt, JournalTypeEnum.CarrierStats)
        {
            State = new CarrierState();
            State.CarrierID = evt["CarrierID"].Long();
            State.Callsign = evt["Callsign"].Str();
            State.Name = evt["Name"].Str();
            State.CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            State.DockingAccess = evt["DockingAccess"].Str();
            State.AllowNotorious = evt["AllowNotorious"].Bool();
            State.FuelLevel = evt["FuelLevel"].Int();
            State.JumpRangeCurr = evt["JumpRangeCurr"].Double();
            State.JumpRangeMax = evt["JumpRangeMax"].Double();
            State.PendingDecommission = evt["PendingDecommission"].Bool();

            var spaceusage = evt["SpaceUsage"];
            if (spaceusage != null)
            {
                State.SpaceUsage.TotalCapacity = spaceusage["TotalCapacity"].Int();
                State.SpaceUsage.Crew = spaceusage["Crew"].Int();
                State.SpaceUsage.Cargo = spaceusage["Cargo"].Int();
                State.SpaceUsage.CargoSpaceReserved = spaceusage["CargoSpaceReserved"].Int();
                State.SpaceUsage.ShipPacks = spaceusage["ShipPacks"].Int();
                State.SpaceUsage.ModulePacks = spaceusage["ModulePacks"].Int();
                State.SpaceUsage.FreeSpace = spaceusage["FreeSpace"].Int();
            }

            var finance = evt["Finance"];
            if (finance != null)
            {
                State.Finance.CarrierBalance = finance["CarrierBalance"].Long();
                State.Finance.ReserveBalance = finance["ReserveBalance"].Long();
                State.Finance.AvailableBalance = finance["AvailableBalance"].Long();
                State.Finance.ReservePercent = finance["ReservePercent"].Double();
                State.Finance.TaxRatePioneersupplies = finance["TaxRate_pioneersupplies"].DoubleNull();
                State.Finance.TaxRateShipyard = finance["TaxRate_shipyard"].DoubleNull();
                State.Finance.TaxRateRearm = finance["TaxRate_rearm"].DoubleNull();
                State.Finance.TaxRateOutfitting = finance["TaxRate_outfitting"].DoubleNull();
                State.Finance.TaxRateRefuel = finance["TaxRate_refuel"].DoubleNull();
                State.Finance.TaxRateRepair = finance["TaxRate_repair"].DoubleNull();
            }

            var ca = evt["Crew"]?.ToObjectQ<CarrierState.ServicesClass[]>();
            if (ca != null)
                State.Services = ca.ToList();

            var sp = evt["ShipPacks"]?.ToObjectQ<CarrierState.PackClass[]>();
            if (sp != null)
                State.ShipPacks = sp.ToList();

            var mp = evt["ModulePacks"]?.ToObjectQ<CarrierState.PackClass[]>();
            if (mp != null)
                State.ModulePacks = mp.ToList();
        }


        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Name: ".T(EDCTx.JournalCarrier_Name), State.Name,
                                                "Call Sign: ".T(EDCTx.JournalCarrier_Callsign), State.Callsign,
                                                "Carrier Type" + ": ", CarrierDefinitions.ToLocalisedLanguage(State.CarrierType),
                                                "Fuel Level: ;;N0".T(EDCTx.JournalCarrier_FuelLevel), State.FuelLevel,
                                                "Jump Range: ; ly;0.0".T(EDCTx.JournalCarrier_JumpRange), State.JumpRangeCurr,
                                                "Carrier Balance: ; cr;N0".T(EDCTx.JournalCarrier_Balance), State.Finance.CarrierBalance,
                                                "Reserve Balance: ; cr;N0".T(EDCTx.JournalCarrier_ReserveBalance), State.Finance.ReserveBalance,
                                                "Available Balance: ; cr;N0".T(EDCTx.JournalCarrier_AvailableBalance), State.Finance.AvailableBalance,
                                                "Reserve Percent: ;;N1".T(EDCTx.JournalCarrier_ReservePercent), State.Finance.ReservePercent,
                                                "Tax Rate Pioneersupplies: ;;N1".T(EDCTx.JournalCarrier_TaxRatePioneersupplies), State.Finance.TaxRatePioneersupplies,
                                                "Tax Rate Shipyard: ;;N1".T(EDCTx.JournalCarrier_TaxRateShipyard), State.Finance.TaxRateShipyard,
                                                "Tax Rate Rearm: ;;N1".T(EDCTx.JournalCarrier_TaxRateRearm), State.Finance.TaxRateRearm,
                                                "Tax Rate Outfitting: ;;N1".T(EDCTx.JournalCarrier_TaxRateOutfitting), State.Finance.TaxRateOutfitting,
                                                "Tax Rate Refuel: ;;N1".T(EDCTx.JournalCarrier_TaxRateRefuel), State.Finance.TaxRateRefuel,
                                                "Tax Rate Repair: ;;N1".T(EDCTx.JournalCarrier_TaxRateRepair), State.Finance.TaxRateRepair
                                                );
        }


        public override string GetDetailed()
        {
            var sb = new System.Text.StringBuilder(256);

            sb.Build("Total Capacity: ".T(EDCTx.JournalCarrier_TotalCapacity), State.SpaceUsage.TotalCapacity,
                                                    "Crew: ".T(EDCTx.JournalCarrier_Crew), State.SpaceUsage.Crew,
                                                    "Cargo: ".T(EDCTx.JournalCarrier_Cargo), State.SpaceUsage.Cargo,
                                                    "Cargo Space Reserved: ".T(EDCTx.JournalCarrier_CargoReserved), State.SpaceUsage.CargoSpaceReserved,
                                                    "Ship Packs: ".T(EDCTx.JournalCarrier_ShipPacks), State.SpaceUsage.ShipPacks,
                                                    "Module Packs: ".T(EDCTx.JournalCarrier_ModulePacks), State.SpaceUsage.ModulePacks,
                                                    "Free Space: ".T(EDCTx.JournalCarrier_FreeSpace), State.SpaceUsage.FreeSpace);

            if (State.Services != null && State.Services.Count>0)
            {
                foreach (var v in State.Services)
                {
                    sb.AppendCR();
                    if (v.Activated)
                        sb.Build("Activated:", v.CrewRole, "", v.CrewName, "< (Disabled);", v.Enabled);
                    else
                        sb.Build("Not Activated:", v.CrewRole);
                }
            }

            if (State.ShipPacks != null && State.ShipPacks.Count > 0)
            {
                foreach (var v in State.ShipPacks)
                {
                    sb.AppendCR();
                    sb.Build("Pack: ", v.PackTheme, "", v.PackTier);
                }
            }

            if (State.ModulePacks != null && State.ModulePacks.Count>0)
            {
                foreach (var v in State.ModulePacks)
                {
                    sb.AppendCR();
                    sb.Build("Module: ", v.PackTheme, "", v.PackTier);
                }

            }

            return sb.ToString();
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierJumpRequest)]
    public class JournalCarrierJumpRequest : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string SystemName { get; set; }
        public long SystemAddress { get; set; }
        public string Body { get; set; }        // if to system, journal seems to write Body==System Name. Body will always be non null
        public int BodyID { get; set; }         // will be 0 or the body id

        public DateTime? DepartureTime { get; set; } // pre u14 not there

        public JournalCarrierJumpRequest(JObject evt) : base(evt, JournalTypeEnum.CarrierJumpRequest)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            SystemName = evt["SystemName"].Str();
            Body = evt["Body"].Str();
            SystemAddress = evt["SystemAddress"].Long();
            BodyID = evt["BodyID"].Int();
            if (evt["DepartureTime"]!=null)
                DepartureTime = evt["DepartureTime"].DateTimeUTC();
        }

        public override string GetInfo()
        {
            DateTime? dtime = null;
            if (DepartureTime.HasValue)
                dtime = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(DepartureTime.Value);

            return BaseUtils.FieldBuilder.Build("To ".T(EDCTx.JournalCarrier_ToSystem), SystemName,
                                                "Body ".T(EDCTx.JournalCarrier_Body), Body,
                                                "@ ", dtime
                                                );
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierDecommission)]
    public class JournalCarrierDecommission : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public long ScrapRefund { get; set; }
        public long ScrapTime { get; set; }
        public DateTime ScrapDateTimeUTC { get; set; }

        public JournalCarrierDecommission(JObject evt) : base(evt, JournalTypeEnum.CarrierDecommission)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            ScrapRefund = evt["ScrapRefund"].Long();
            ScrapTime = evt["ScrapTime"].Long();
            ScrapDateTimeUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ScrapTime);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Refund: ; cr;N0".T(EDCTx.JournalCarrier_Refund), ScrapRefund,
                                                "at UTC ".T(EDCTx.JournalCarrier_RefundTime), ScrapDateTimeUTC
                                                );
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierCancelDecommission)]
    public class JournalCarrierCancelDecommission : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public JournalCarrierCancelDecommission(JObject evt) : base(evt, JournalTypeEnum.CarrierCancelDecommission)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
        }

        public override string GetInfo()
        {
            return "";
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierBankTransfer)]
    public class JournalCarrierBankTransfer : JournalEntry, ILedgerJournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public long Deposit { get; set; }
        public long Withdraw { get; set; }
        public long PlayerBalance { get; set; }
        public long CarrierBalance { get; set; }

        public JournalCarrierBankTransfer(JObject evt) : base(evt, JournalTypeEnum.CarrierBankTransfer)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Deposit = evt["Deposit"].Long();
            Withdraw = evt["Withdraw"].Long();
            PlayerBalance = evt["PlayerBalance"].Long();
            CarrierBalance = evt["CarrierBalance"].Long();
        }

        public override string GetInfo()
        {
            if (Deposit > 0)
                return BaseUtils.FieldBuilder.Build("Deposit: ; cr;N0".T(EDCTx.JournalCarrier_Deposit), Deposit, "Carrier Balance: ; cr;N0".T(EDCTx.JournalCarrier_Balance), CarrierBalance);
            else
                return BaseUtils.FieldBuilder.Build("Withdraw: ; cr;N0".T(EDCTx.JournalCarrier_Withdraw), Withdraw, "Carrier Balance: ; cr;N0".T(EDCTx.JournalCarrier_Balance), CarrierBalance);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, JournalTypeEnum.CarrierBankTransfer, "" , Withdraw - Deposit);
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierDepositFuel)]
    public class JournalCarrierDepositFuel : JournalEntry, ICommodityJournalEntry, IStatsJournalEntryMatCommod, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public int Amount { get; set; }     
        public int Total { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = "tritium", Count = -Amount } }; } }

        public string FDNameOfItem { get { return "Carrier"; } }        // implement IStatsJournalEntryMatCommod
        public int CountOfItem { get { return Amount; } }

        public JournalCarrierDepositFuel(JObject evt) : base(evt, JournalTypeEnum.CarrierDepositFuel)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Amount = evt["Amount"].Int();
            Total = evt["Total"].Int();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Amount: ;;N0".T(EDCTx.JournalCarrier_Amount), Amount,
                                                "Fuel Level: ;;N0".T(EDCTx.JournalCarrier_FuelLevel), Total);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, "tritium", -Amount, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system, "tritium", -Amount, 0, stationfaction);
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierCrewServices)]
    public class JournalCarrierCrewServices : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string Operation { get; set; }
        public string CrewRole { get; set; }
        public string FriendlyCrewRole { get; set; }
        public string CrewName { get; set; }

        // as per frontier CrewRole Entry
        public enum ServiceType
        {
            BridgeCrew, CommodityTrading, TritiumDepot,        // not listed in crew services, but core items
            Refuel, Repair, Rearm, VoucherRedemption, Shipyard, Outfitting, BlackMarket, Exploration, Bartender, VistaGenomics, PioneerSupplies,
            Unknown
        };

        // turn CrewRole into Service type
        public ServiceType GetServiceType() { return Enum.TryParse(CrewRole, true, out ServiceType typefound) ? typefound : ServiceType.Unknown; }
        static public ServiceType GetServiceType(string name) { return Enum.TryParse(name, true, out ServiceType typefound) ? typefound : ServiceType.Unknown; }

        public enum OperationType { Activate, Deactivate, Pause, Resume, Replace, Unknown }
        public OperationType GetOperation() { return Enum.TryParse(Operation, true, out OperationType typefound) ? typefound : OperationType.Unknown; }

        static public string GetTranslatedServiceName(ServiceType t) { return translatedname[(int)t]; }

        static public bool IsOptionalService(ServiceType t) { return t >= ServiceType.Refuel && t != ServiceType.Unknown; }
        static public bool IsValidService(ServiceType t) { return t != ServiceType.Unknown; }

        static public int GetServiceCount() { var entries = Enum.GetValues(typeof(EliteDangerousCore.JournalEvents.JournalCarrierCrewServices.ServiceType)); return entries.Length - 1; }      // ignore Unknown

        // as per frontier Operation Entry

        [System.Diagnostics.DebuggerDisplay("{Service} {CargoSize}t {InstallCost}cr up {UpkeepCost}")]
        public class ServicesData       // https://elite-dangerous.fandom.com/wiki/Drake-Class_Carrier
        {
            public ServicesData(ServiceType t, long cost, long upkeep, long suspendedcost, int cargosize) 
            { Service = t; InstallCost = cost; UpkeepCost = upkeep; SuspendedUpkeepCost = suspendedcost; CargoSize = cargosize; }
            public ServiceType Service { get; set; }
            public long InstallCost { get; set; }
            public long UpkeepCost { get; set; }
            public long SuspendedUpkeepCost { get; set; }
            public long CargoSize { get; set; }
        }

        // may return null if names don't match in future.
        public static ServicesData GetDataOnServiceType(ServiceType t) { return Array.Find(ServiceInformation, x => x.Service == t); }

        // get data on journal service type
        public ServicesData GetDataOnService { get { var t = GetServiceType();  return Array.Find(ServiceInformation, x => x.Service == t); } }

        public JournalCarrierCrewServices(JObject evt) : base(evt, JournalTypeEnum.CarrierCrewServices)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            CrewRole = evt["CrewRole"].Str();
            FriendlyCrewRole = JournalFieldNaming.CrewRole(CrewRole);
            Operation = evt["Operation"].Str();
            CrewName = evt["CrewName"].Str();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Role: ".T(EDCTx.JournalEntry_Role), FriendlyCrewRole,
                                                "Operation: ".T(EDCTx.JournalCarrier_Operation), Operation,
                                                "Crew Member: ".T(EDCTx.JournalEntry_CrewMember), CrewName
                                                );
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }

        private static ServicesData[] ServiceInformation = new ServicesData[]        // verified with game oct 22
        {
            new ServicesData(ServiceType.Refuel,40000000,1500000,750000,500),
            new ServicesData(ServiceType.Repair,50000000,1500000,750000,180),
            new ServicesData(ServiceType.Rearm,95000000,1500000,750000,250),
            new ServicesData(ServiceType.VoucherRedemption,150000000,1850000,850000,100),
            new ServicesData(ServiceType.Shipyard,250000000,6500000,1800000,3000),
            new ServicesData(ServiceType.Outfitting,250000000,5000000,1500000,1750),
            new ServicesData(ServiceType.BlackMarket,165000000,2000000,1250000,250),
            new ServicesData(ServiceType.Exploration,150000000,1850000,700000,120),
            new ServicesData(ServiceType.Bartender,200000000,1750000,1250000,150),
            new ServicesData(ServiceType.VistaGenomics,150000000,1500000,700000,120),
            new ServicesData(ServiceType.PioneerSupplies,250000000,5000000,1500000,200),
        };


        private static string[] translatedname = new string[] {
            "Bridge Crew".TxID(EDCTx.Services_BridgeCrew),
            "Commodity Trading".TxID(EDCTx.Services_CommodityTrading),
            "Tritium Depot".TxID(EDCTx.Services_TritiumDepot),
            "Refuel Station".TxID(EDCTx.Services_Refuel),
            "Repair Crews".TxID(EDCTx.Services_Repair),
            "Armoury".TxID(EDCTx.Services_Rearm),
            "Redemption Office".TxID(EDCTx.Services_VoucherRedemption),
            "Shipyard".TxID(EDCTx.Services_Shipyard),
            "Outfitting".TxID(EDCTx.Services_Outfitting),
            "Secure Warehouse".TxID(EDCTx.Services_BlackMarket),
            "Universal Cartographics".TxID(EDCTx.Services_Exploration),
            "Concourse Bar".TxID(EDCTx.Services_Bartender),
            "Vista Genomics".TxID(EDCTx.Services_VistaGenomics),
            "Pioneer Supplies".TxID(EDCTx.Services_PioneerSupplies),
            "Unknown",
        };


    }

    [JournalEntryType(JournalTypeEnum.CarrierFinance)]
    public class JournalCarrierFinance : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }

        public CarrierState.FinanceClass Finance { get; set; } = new CarrierState.FinanceClass();

        public JournalCarrierFinance(JObject evt) : base(evt, JournalTypeEnum.CarrierFinance)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Finance.TaxRatePioneersupplies = evt["TaxRate_pioneersupplies"].Double();
            Finance.TaxRateShipyard = evt["TaxRate_shipyard"].Double();
            Finance.TaxRateRearm = evt["TaxRate_rearm"].Double();
            Finance.TaxRateOutfitting = evt["TaxRate_outfitting"].Double();
            Finance.TaxRateRefuel = evt["TaxRate_refuel"].Double();
            Finance.TaxRateRepair = evt["TaxRate_repair"].Double();
            Finance.CarrierBalance = evt["CarrierBalance"].Long();
            Finance.ReserveBalance = evt["ReserveBalance"].Long();
            Finance.AvailableBalance = evt["AvailableBalance"].Long();
            Finance.ReservePercent = evt["ReservePercent"].Double();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Carrier Balance: ; cr;N0".T(EDCTx.JournalCarrier_Balance), Finance.CarrierBalance,
                                                "Reserve Balance: ; cr;N0".T(EDCTx.JournalCarrier_ReserveBalance), Finance.ReserveBalance,
                                                "Available Balance: ; cr;N0".T(EDCTx.JournalCarrier_AvailableBalance), Finance.AvailableBalance,
                                                "Reserve Percent: ;;N1".T(EDCTx.JournalCarrier_ReservePercent), Finance.ReservePercent,
                                                "Tax Rate Pioneersupplies: ;;N1".T(EDCTx.JournalCarrier_TaxRatePioneersupplies), Finance.TaxRatePioneersupplies,
                                                "Tax Rate Shipyard: ;;N1".T(EDCTx.JournalCarrier_TaxRateShipyard), Finance.TaxRateShipyard,
                                                "Tax Rate Rearm: ;;N1".T(EDCTx.JournalCarrier_TaxRateRearm), Finance.TaxRateRearm,
                                                "Tax Rate Outfitting: ;;N1".T(EDCTx.JournalCarrier_TaxRateOutfitting), Finance.TaxRateOutfitting,
                                                "Tax Rate Refuel: ;;N1".T(EDCTx.JournalCarrier_TaxRateRefuel), Finance.TaxRateRefuel,
                                                "Tax Rate Repair: ;;N1".T(EDCTx.JournalCarrier_TaxRateRepair), Finance.TaxRateRepair
                                                );
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierShipPack)]
    public class JournalCarrierShipPack : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string Operation { get; set; }       // BuyPack, SellPack
        public string FriendlyOperation { get; set; }       // BuyPack, SellPack
        public string PackTheme { get; set; }
        public int PackTier { get; set; }
        public long? Cost { get; set; }
        public long? Refund { get; set; }

        public JournalCarrierShipPack(JObject evt) : base(evt, JournalTypeEnum.CarrierShipPack)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Operation = evt["Operation"].Str();
            FriendlyOperation = JournalFieldNaming.ShipPackOperation(Operation);
            PackTheme = evt["PackTheme"].Str();
            PackTier = evt["PackTier"].Int();
            Cost = evt["Cost"].LongNull();
            Refund = evt["Refund"].LongNull();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build(
                                                "", FriendlyOperation,
                                                "", PackTheme,
                                                "Tier: ".T(EDCTx.JournalCarrier_Tier), PackTier,
                                                "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Cost,
                                                "Refund: ; cr;N0".T(EDCTx.JournalCarrier_Refund), Refund
                                                );

        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierModulePack)]
    public class JournalCarrierModulePack : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string Operation { get; set; }
        public string FriendlyOperation { get; set; }
        public string PackTheme { get; set; }
        public int PackTier { get; set; }
        public long? Cost { get; set; }
        public long? Refund { get; set; }

        public JournalCarrierModulePack(JObject evt) : base(evt, JournalTypeEnum.CarrierModulePack)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Operation = evt["Operation"].Str();
            FriendlyOperation = JournalFieldNaming.ModulePackOperation(Operation);
            PackTheme = evt["PackTheme"].Str();
            PackTier = evt["PackTier"].Int();
            Cost = evt["Cost"].LongNull();
            Refund = evt["Refund"].LongNull();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", FriendlyOperation,
                                                "", PackTheme,
                                                "Tier: ".T(EDCTx.JournalCarrier_Tier), PackTier,
                                                "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Cost,
                                                "Refund: ; cr;N0".T(EDCTx.JournalCarrier_Refund), Refund
                                                );

        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierTradeOrder)]
    public class JournalCarrierTradeOrder : JournalEntry, ICarrierStats
    {
        public TradeOrder Order { get; set; } = new TradeOrder();
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public bool? CancelTrade { get; set; }

        [System.Diagnostics.DebuggerDisplay("TO {Commodity} cost{Price} p{PurchaseOrder} s{SaleOrder} bm{BlackMarket}")]
        public class TradeOrder
        {
            public bool BlackMarket { get; set; }
            public string Commodity { get; set; }
            public string Commodity_Localised { get; set; }
            public int Price { get; set; }
            public int? PurchaseOrder { get; set; }     // non null if purchase order
            public int? SaleOrder { get; set; }         // non null if sale order

            public DateTime Placed { get; set; }        // additional field

            public TradeOrder() { }
            public TradeOrder(TradeOrder other)
            {
                BlackMarket = other.BlackMarket;
                Commodity = other.Commodity;
                Commodity_Localised = other.Commodity_Localised;
                Price = other.Price;
                PurchaseOrder = other.PurchaseOrder;
                SaleOrder = other.SaleOrder;
                Placed = other.Placed;
            }
            public bool Equals(TradeOrder other)    // based on Blackmarket and names
            {
                return BlackMarket == other.BlackMarket && Commodity == other.Commodity;
            }
        }

        public JournalCarrierTradeOrder(JObject evt) : base(evt, JournalTypeEnum.CarrierTradeOrder)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            CancelTrade = evt["CancelTrade"].BoolNull();

            Order.BlackMarket = evt["BlackMarket"].Bool();
            Order.Commodity = evt["Commodity"].Str();
            Order.Commodity_Localised =JournalFieldNaming.CheckLocalisation(evt["Commodity_Localised"].Str(), Order.Commodity);
            Order.PurchaseOrder = evt["PurchaseOrder"].IntNull();
            Order.SaleOrder = evt["SaleOrder"].IntNull();
            Order.Price = evt["Price"].Int();
            Order.Placed = this.EventTimeUTC;
        }

        public override string GetInfo()
        {
            if (Order.PurchaseOrder != null)
            {
                return BaseUtils.FieldBuilder.Build("Purchase: ".T(EDCTx.JournalCarrier_Purchase), Order.Commodity_Localised,
                                                    "", Order.PurchaseOrder,
                                                    "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Order.Price,
                                                    "<; (Blackmarket)", Order.BlackMarket);
            }
            else if (Order.SaleOrder != null)
            {
                return BaseUtils.FieldBuilder.Build("Sell: ".T(EDCTx.JournalCarrier_Sell), Order.Commodity_Localised,
                                                    "", Order.SaleOrder,
                                                    "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Order.Price,
                                                    "<; (Blackmarket)", Order.BlackMarket); 
            }
            else if ( CancelTrade != null && CancelTrade.Value == true )
            {
                return BaseUtils.FieldBuilder.Build("Cancel Sell of: ".T(EDCTx.JournalCarrier_CancelSell), Order.Commodity_Localised, "<; (Blackmarket)", Order.BlackMarket);
            }
            else
            {
                return "Incorrect options for this entry, report journal entry to EDD Team";
            }
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierDockingPermission)]
    public class JournalCarrierDockingPermission : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string DockingAccess { get; set; }
        public bool AllowNotorious { get; set; }

        public JournalCarrierDockingPermission(JObject evt) : base(evt, JournalTypeEnum.CarrierDockingPermission)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            DockingAccess = evt["DockingAccess"].Str();
            AllowNotorious = evt["AllowNotorious"].Bool();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Access: ".T(EDCTx.JournalCarrier_Access), DockingAccess,
                                                ";Allow Notorious".T(EDCTx.JournalCarrier_AllowNotorious), AllowNotorious);
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierNameChange)]
    public class JournalCarrierNameChange : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string Callsign { get; set; }
        public string Name { get; set; }

        public JournalCarrierNameChange(JObject evt) : base(evt, JournalTypeEnum.CarrierNameChange)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Callsign = evt["Callsign"].Str();
            Name = evt["Name"].Str();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Name: ".T(EDCTx.JournalCarrier_Name), Name, "Call Sign: ".T(EDCTx.JournalCarrier_Callsign), Callsign);
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.CarrierJumpCancelled)]
    public class JournalCarrierJumpCancelled : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }

        public JournalCarrierJumpCancelled(JObject evt) : base(evt, JournalTypeEnum.CarrierJumpCancelled)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
    }

    [JournalEntryType(JournalTypeEnum.FCMaterials)]
    public class JournalFCMaterials : JournalEntry, IAdditionalFiles
    {
        public JournalFCMaterials(JObject evt) : base(evt, JournalTypeEnum.FCMaterials)
        {
            Rescan(evt);
        }

        public void Rescan(JObject evt)
        {
            MarketID = evt["MarketID"].Long();
            CarrierID = evt["CarrierID"].Str();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            CarrierName = evt["CarrierName"].Str();
            Items = new List<CCommodities>(); // always made..

            JArray jitems = (JArray)evt["Items"];
            if (jitems != null)
            {
                foreach (JObject commodity in jitems)
                {
                    CCommodities com = new CCommodities(commodity, CCommodities.ReaderType.FCMaterials);
                    Items.Add(com);
                }

                Items.Sort((l, r) => l.locName.CompareTo(r.locName));
            }
        }

        public void ReadAdditionalFiles(string directory)
        {
            JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "FCMaterials.json"), EventTypeStr);
            if (jnew != null)        // new json, rescan
            {
                Rescan(jnew);
                UpdateJson(jnew);
            }
        }

        public long MarketID { get; set; }
        public string CarrierID { get; set; }       // NOTE different to other carrier events
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string CarrierName { get; set; }
        public List<CCommodities> Items { get; set; }       // may be null


        public override string SummaryName(ISystem sys) { return "Bartender Materials".TxID(EDCTx.JournalEntry_BartenderMaterials); }

        public override string GetInfo()
        {
            if (Items == null)
            {
                return BaseUtils.FieldBuilder.Build("", CarrierName);
            }
            else
            {
                return BaseUtils.FieldBuilder.Build("", CarrierName, "Prices on ; items".T(EDCTx.JournalCommodityPricesBase_PON), Items.Count);
            }
        }


        public override string GetDetailed()
        {
            if (Items != null)
            {
                var sb = new System.Text.StringBuilder();

                var stocked = Items.Where(x => x.HasStock);
                if (stocked.Count() > 0)
                {
                    sb.Append("Items to buy: ".T(EDCTx.JournalCommodityPricesBase_Itemstobuy));
                    sb.AppendCR();
                    foreach (CCommodities c in stocked)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);
                        sb.Append("  ");
                        sb.Append(string.Format("{0}: {1}  ".T(EDCTx.JournalCommodityPricesBase_CPBBuy), name, c.buyPrice));
                        sb.AppendCR();
                    }
                }

                var sellonly = Items.Where(x => !x.HasStock);

                if (sellonly.Count() > 0)
                {
                    sb.Append("Sell only Items: ".T(EDCTx.JournalCommodityPricesBase_SO));
                    sb.AppendCR();

                    foreach (CCommodities c in sellonly)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);
                        sb.Append("  ");
                        sb.Append(string.Format("{0}: {1}  ".T(EDCTx.JournalCommodityPricesBase_CPBBuy), name, c.sellPrice));
                        sb.AppendCR();
                    }
                }

                return sb.ToString();
            }
            else
                return null;
        }

        // pattern also used in journaldocking stationinfo
        public bool HasItem(string fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase)) >= 0; }
        public bool HasItemToBuy(string fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase) && x.HasStock) >= 0; }
        public bool HasItemToSell(string fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase) && x.HasDemandAndPrice) >= 0; }
    }

    [JournalEntryType(JournalTypeEnum.CarrierLocation)]
    public class JournalCarrierLocation : JournalEntry, ICarrierStats
    {
        public long CarrierID { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
        public int BodyID { get; set; }         // will be 0 or the body id

        public JournalCarrierLocation(JObject evt) : base(evt, JournalTypeEnum.CarrierLocation)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            StarSystem = evt["StarSystem"].Str("Unknown");
            SystemAddress = evt["SystemAddress"].Long();
            BodyID = evt["BodyID"].Int();
        }

        public override string GetInfo()
        {
            return "@ " + StarSystem + ", " + "Carrier Type" + ": " + CarrierDefinitions.ToLocalisedLanguage(CarrierType);
        }

        public void UpdateCarrierStats(CarrierStats s, bool _)
        {
        }
    }

    //When written: when jumping with a fleet carrier
    [JournalEntryType(JournalTypeEnum.CarrierJump)]
    public class JournalCarrierJump : JournalLocOrJump, IBodyNameAndID, IJournalJumpColor, IStarScan, ICarrierStats
    {
        public JournalCarrierJump(JObject evt) : base(evt, JournalTypeEnum.CarrierJump)
        {
            // base class does StarSystem/StarPos/Faction/Powerplay

            Docked = evt["Docked"].Bool();
            var snl = JournalFieldNaming.GetStationNames(evt);
            StationName = snl.Item1;
            StationName_Localised = snl.Item2;

            // keep type in case they introduce more than 1 carrier type
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].Str());
            StationType = StationDefinitions.ToEnglish(FDStationType);

            MarketID = evt["MarketID"].LongNull();

            // don't bother with StationGovernment, StationFaction, StationEconomy, StationEconomies

            StationServices = StationDefinitions.ReadServicesFromJson(evt["StationServices"]);

            Body = evt["Body"].Str();
            BodyID = evt["BodyID"].IntNull();
            BodyType = JournalFieldNaming.NormaliseBodyType(evt["BodyType"].Str());
            DistFromStarLS = evt["DistFromStarLS"].DoubleNull();

            JToken jm = evt["EDDMapColor"];
            MapColor = jm.Int(EDCommander.Current.MapColour);
            if (jm.IsNull())
                evt["EDDMapColor"] = EDCommander.Current.MapColour;      // new entries get this default map colour if its not already there
        }

        public CarrierDefinitions.CarrierType CarrierType { get; } = CarrierDefinitions.CarrierType.UnknownType;        // stupid journal does not tell
        public bool Docked { get; set; }
        public string StationName { get; set; }         // from aprilish 2025, we get these even if we are not on it. Will be blank
        public string StationName_Localised { get; set; }       
        public string StationType { get; set; } // friendly station type
        public StationDefinitions.StarportTypes FDStationType { get; set; } // fdname
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public string BodyType { get; set; }
        public string BodyDesignation { get; set; }
        public double? DistFromStarLS { get; set; }

        public long? MarketID { get; set; }
        public int MapColor { get; set; }
        public System.Drawing.Color MapColorARGB { get { return System.Drawing.Color.FromArgb(MapColor); } }

        public StationDefinitions.StationServices[] StationServices { get; set; }
        public EconomyDefinitions.Economies[] StationEconomyList { get; set; }        // may be null

        public override string SummaryName(ISystem sys)
        {
            if (Docked)
                return string.Format("Jumped with carrier {0} to {1}".T(EDCTx.JournalCarrierJump_JumpedWith), StationName, Body);
            else
                return string.Format("Carrier jumped to {0}".T(EDCTx.JournalCarrierJump_JumpedItself), Body);
        }
        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddLocation(new SystemClass(StarSystem, SystemAddress, StarPos.X, StarPos.Y, StarPos.Z));     // we use our data to fill in 
        }

        public override string GetInfo()        // carrier jump
        {
            return BaseUtils.FieldBuilder.Build("", Body ?? StarSystem);
        }

        public override string GetDetailed()    // carrier jump
        {
            StringBuilder sb = new StringBuilder();
            sb.Build("<;(Wanted) ".T(EDCTx.JournalLocOrJump_Wanted), Wanted);

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

