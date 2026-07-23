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
            public CarrierDefinitions.ServiceType CrewRole { get; set; }
            public bool Activated { get; set; }
            public bool Enabled { get; set; }
            public string CrewName { get; set; }
        }

        public List<ServicesClass> Services { get; set; }       // may be null - called 'Crew' in journal buts its all about services
        public ServicesClass GetService(CarrierDefinitions.ServiceType t) // may be null.  Core services are not listed
        {
            return Services?.Find(x => x.CrewRole == t);
        }

        public long GetServicesCost()
        {
            long res = 0;
            foreach( var s in Services.EmptyIfNull())
            {
                var si = CarrierDefinitions.GetDataOnServiceType(s.CrewRole);
                if (si != null)
                {
                    long delta = s.Activated ? (s.Enabled ? si.UpkeepCost : si.SuspendedUpkeepCost) : 0;
                    //System.Diagnostics.Debug.WriteLine($"Service cost {si.Service} {si.UpkeepCost} {si.SuspendedUpkeepCost} = {delta}");
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
            return BaseUtils.FieldBuilder.Build("At ".Tx(), Location,
                                              "Cost: ; cr;N0".Tx(), Price,
                                              "Call Sign".Tx()+": ", Callsign);
        }

        public void Ledger(Ledger mcl)
        {
            string x = "Call Sign".Tx()+": "+ Callsign;
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

            var ca = evt["Crew"]?.ToObject<CarrierState.ServicesClass[]>();
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
            return BaseUtils.FieldBuilder.Build("Name".Tx()+": ", State.Name,
                                                "Call Sign".Tx()+": ", State.Callsign,
                                                "Carrier Type".Tx()+": ", CarrierDefinitions.ToLocalisedLanguage(State.CarrierType),
                                                "Fuel Level: ;;N0".Tx(), State.FuelLevel,
                                                "Jump Range: ; ly;0.0".Tx(), State.JumpRangeCurr,
                                                "Carrier Balance: ; cr;N0".Tx(), State.Finance.CarrierBalance,
                                                "Reserve Balance: ; cr;N0".Tx(), State.Finance.ReserveBalance,
                                                "Available Balance: ; cr;N0".Tx(), State.Finance.AvailableBalance,
                                                "Reserve Percent: ;;N1".Tx(), State.Finance.ReservePercent,
                                                "Tax Rate Pioneersupplies: ;;N1".Tx(), State.Finance.TaxRatePioneersupplies,
                                                "Tax Rate Shipyard: ;;N1".Tx(), State.Finance.TaxRateShipyard,
                                                "Tax Rate Rearm: ;;N1".Tx(), State.Finance.TaxRateRearm,
                                                "Tax Rate Outfitting: ;;N1".Tx(), State.Finance.TaxRateOutfitting,
                                                "Tax Rate Refuel: ;;N1".Tx(), State.Finance.TaxRateRefuel,
                                                "Tax Rate Repair: ;;N1".Tx(), State.Finance.TaxRateRepair
                                                );
        }


        public override string GetDetailed()
        {
            var sb = new System.Text.StringBuilder(256);

            sb.Build("Total Capacity".Tx()+": ", State.SpaceUsage.TotalCapacity,
                                                    "Crew".Tx()+": ", State.SpaceUsage.Crew,
                                                    "Cargo".Tx()+": ", State.SpaceUsage.Cargo,
                                                    "Cargo Space Reserved".Tx()+": ", State.SpaceUsage.CargoSpaceReserved,
                                                    "Ship Packs".Tx()+": ", State.SpaceUsage.ShipPacks,
                                                    "Module Packs".Tx()+": ", State.SpaceUsage.ModulePacks,
                                                    "Free Space".Tx()+": ", State.SpaceUsage.FreeSpace);

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

            return BaseUtils.FieldBuilder.Build("To ".Tx(), SystemName,
                                                "Body ".Tx(), Body,
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
            return BaseUtils.FieldBuilder.Build("Refund: ; cr;N0".Tx(), ScrapRefund,
                                                "at UTC ".Tx(), ScrapDateTimeUTC
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
        public long Deposit { get; set; }
        public long Withdraw { get; set; }
        public long PlayerBalance { get; set; }
        public long CarrierBalance { get; set; }
        public CarrierDefinitions.CarrierType CarrierType { get; set; }

        public JournalCarrierBankTransfer(JObject evt) : base(evt, JournalTypeEnum.CarrierBankTransfer)
        {
            CarrierID = evt["CarrierID"].Long();
            Deposit = evt["Deposit"].Long();
            Withdraw = evt["Withdraw"].Long();
            PlayerBalance = evt["PlayerBalance"].Long();
            CarrierBalance = evt["CarrierBalance"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
        }

        public override string GetInfo()
        {
            if (Deposit > 0)
                return BaseUtils.FieldBuilder.Build("Deposit: ; cr;N0".Tx(), Deposit, "Carrier Balance: ; cr;N0".Tx(), CarrierBalance);
            else
                return BaseUtils.FieldBuilder.Build("Withdraw: ; cr;N0".Tx(), Withdraw, "Carrier Balance: ; cr;N0".Tx(), CarrierBalance);
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
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = MaterialCommodityMicroResourceType.Tritium, Count = -Amount } }; } }

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
            return BaseUtils.FieldBuilder.Build("Amount: ;;N0".Tx(), Amount,
                                                "Fuel Level: ;;N0".Tx(), Total);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, MaterialCommodityMicroResourceType.Tritium, -Amount, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system, MaterialCommodityMicroResourceType.Tritium, -Amount, 0, stationfaction);
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
        public CarrierDefinitions.ServiceOperationType Operation { get; set; }
        public CarrierDefinitions.ServiceType CrewRole { get; set; }
        public string FriendlyCrewRole { get; set; }
        public string CrewName { get; set; }
        public CarrierDefinitions.ServicesData GetDataOnService => CarrierDefinitions.GetDataOnServiceType(CrewRole);

        public JournalCarrierCrewServices(JObject evt) : base(evt, JournalTypeEnum.CarrierCrewServices)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            CrewRole = CarrierDefinitions.ToEnumServiceType(evt["CrewRole"].Str());
            FriendlyCrewRole = CrewRole.ToString().SplitCapsWordFull();
            Operation = Enum.TryParse(evt["Operation"].Str(), true, out CarrierDefinitions.ServiceOperationType op) ? op : CarrierDefinitions.ServiceOperationType.Unknown;
            if (Operation == CarrierDefinitions.ServiceOperationType.Unknown)
            {
                BaseUtils.Debugger.TraceBreak($"*** Unknown crew service operation {(evt["Operation"].Str())}");
                ;
            }
            CrewName = evt["CrewName"].Str();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Role".Tx()+": ", FriendlyCrewRole,
                                                "Operation".Tx()+": ", Operation,
                                                "Crew Member".Tx()+": ", CrewName
                                                );
        }

        public void  UpdateCarrierStats(CarrierStats s, bool onfootfleetcarrierunused)
        {
            s.Update(this);
        }
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
            return BaseUtils.FieldBuilder.Build("Carrier Balance: ; cr;N0".Tx(), Finance.CarrierBalance,
                                                "Reserve Balance: ; cr;N0".Tx(), Finance.ReserveBalance,
                                                "Available Balance: ; cr;N0".Tx(), Finance.AvailableBalance,
                                                "Reserve Percent: ;;N1".Tx(), Finance.ReservePercent,
                                                "Tax Rate Pioneersupplies: ;;N1".Tx(), Finance.TaxRatePioneersupplies,
                                                "Tax Rate Shipyard: ;;N1".Tx(), Finance.TaxRateShipyard,
                                                "Tax Rate Rearm: ;;N1".Tx(), Finance.TaxRateRearm,
                                                "Tax Rate Outfitting: ;;N1".Tx(), Finance.TaxRateOutfitting,
                                                "Tax Rate Refuel: ;;N1".Tx(), Finance.TaxRateRefuel,
                                                "Tax Rate Repair: ;;N1".Tx(), Finance.TaxRateRepair
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
        public CarrierDefinitions.ShipPackOperationType Operation { get; set; }       // BuyPack, SellPack
        public string FriendlyOperation { get; set; }       // BuyPack, SellPack
        public string PackTheme { get; set; }
        public int PackTier { get; set; }
        public long? Cost { get; set; }
        public long? Refund { get; set; }

        public JournalCarrierShipPack(JObject evt) : base(evt, JournalTypeEnum.CarrierShipPack)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Operation = Enum.TryParse(evt["Operation"].Str(), true, out CarrierDefinitions.ShipPackOperationType op) ? op : CarrierDefinitions.ShipPackOperationType.Unknown;
            if (Operation == CarrierDefinitions.ShipPackOperationType.Unknown)
            {
                BaseUtils.Debugger.TraceBreak($"*** Unknown module service operation {(evt["Operation"].Str())}");
                ;
            }
            FriendlyOperation = Operation.ToString().SplitCapsWordFull();
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
                                                "Tier".Tx()+": ", PackTier,
                                                "Cost: ; cr;N0".Tx(), Cost,
                                                "Refund: ; cr;N0".Tx(), Refund
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
        public CarrierDefinitions.ModulePackOperationType Operation { get; set; }
        public string FriendlyOperation { get; set; }
        public string PackTheme { get; set; }
        public int PackTier { get; set; }
        public long? Cost { get; set; }
        public long? Refund { get; set; }

        public JournalCarrierModulePack(JObject evt) : base(evt, JournalTypeEnum.CarrierModulePack)
        {
            CarrierID = evt["CarrierID"].Long();
            CarrierType = CarrierDefinitions.ToEnum(evt["CarrierType"].Str());
            Operation = Enum.TryParse(evt["Operation"].Str(), true, out CarrierDefinitions.ModulePackOperationType op) ? op : CarrierDefinitions.ModulePackOperationType.Unknown;
            FriendlyOperation = Operation.ToString().SplitCapsWordFull();
            PackTheme = evt["PackTheme"].Str();
            PackTier = evt["PackTier"].Int();
            Cost = evt["Cost"].LongNull();
            Refund = evt["Refund"].LongNull();
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", FriendlyOperation,
                                                "", PackTheme,
                                                "Tier".Tx()+": ", PackTier,
                                                "Cost: ; cr;N0".Tx(), Cost,
                                                "Refund: ; cr;N0".Tx(), Refund
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
            public FDName Commodity { get; set; }
            public string Commodity_Localised { get; set; }
            public int Price { get; set; }
            public int? PurchaseOrder { get; set; }     // non null if purchase order
            public int? SaleOrder { get; set; }         // non null if sale order

            public DateTime Placed { get; set; }        // additional field

            public TradeOrder() { }
            public TradeOrder(TradeOrder other)
            {
                BlackMarket = other.BlackMarket;
                Commodity = other.Commodity.Clone();
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
            Order.Commodity = FDNameHelpers.NormaliseMatCommods(evt["Commodity"].Str(), out string engname, this);
            Order.Commodity_Localised =JournalFieldNaming.CheckLocalisation(evt["Commodity_Localised"].Str(), engname);
            Order.PurchaseOrder = evt["PurchaseOrder"].IntNull();
            Order.SaleOrder = evt["SaleOrder"].IntNull();
            Order.Price = evt["Price"].Int();
            Order.Placed = this.EventTimeUTC;
        }

        public override string GetInfo()
        {
            if (Order.PurchaseOrder != null)
            {
                return BaseUtils.FieldBuilder.Build("Purchase".Tx()+": ", Order.Commodity_Localised,
                                                    "", Order.PurchaseOrder,
                                                    "Cost: ; cr;N0".Tx(), Order.Price,
                                                    "<; (Blackmarket)", Order.BlackMarket);
            }
            else if (Order.SaleOrder != null)
            {
                return BaseUtils.FieldBuilder.Build("Sell".Tx()+": ", Order.Commodity_Localised,
                                                    "", Order.SaleOrder,
                                                    "Cost: ; cr;N0".Tx(), Order.Price,
                                                    "<; (Blackmarket)", Order.BlackMarket); 
            }
            else if ( CancelTrade != null && CancelTrade.Value == true )
            {
                return BaseUtils.FieldBuilder.Build("Cancel Sell of".Tx()+": ", Order.Commodity_Localised, "<; (Blackmarket)", Order.BlackMarket);
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
            return BaseUtils.FieldBuilder.Build("Access".Tx()+": ", DockingAccess,
                                                ";Allow Notorious".Tx(), AllowNotorious);
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
            return BaseUtils.FieldBuilder.Build("Name".Tx()+": ", Name, "Call Sign".Tx()+": ", Callsign);
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


        public override string SummaryName(ISystem sys) { return "Bartender Materials".Tx(); }

        public override string GetInfo()
        {
            if (Items == null)
            {
                return BaseUtils.FieldBuilder.Build("", CarrierName);
            }
            else
            {
                return BaseUtils.FieldBuilder.Build("", CarrierName, "Prices on ; items".Tx(), Items.Count);
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
                    sb.Append("Items to buy".Tx()+": ");
                    sb.AppendCR();
                    foreach (CCommodities c in stocked)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);
                        sb.Append("  ");
                        sb.Append(string.Format("{0}: {1}  ".Tx(), name, c.buyPrice));
                        sb.AppendCR();
                    }
                }

                var sellonly = Items.Where(x => !x.HasStock);

                if (sellonly.Count() > 0)
                {
                    sb.Append("Sell only Items".Tx()+": ");
                    sb.AppendCR();

                    foreach (CCommodities c in sellonly)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);
                        sb.Append("  ");
                        sb.Append(string.Format("{0}: {1}  ".Tx(), name, c.sellPrice));
                        sb.AppendCR();
                    }
                }

                return sb.ToString();
            }
            else
                return null;
        }

        // pattern also used in journaldocking stationinfo
        public bool HasItem(FDName fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname)) >= 0; }
        public bool HasItemToBuy(FDName fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname) && x.HasStock) >= 0; }
        public bool HasItemToSell(FDName fdname) { return Items != null && Items.FindIndex(x => x.fdname.Equals(fdname) && x.HasDemandAndPrice) >= 0; }
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
            return "@ " + StarSystem + ", " + "Carrier Type".Tx() + ": " + CarrierDefinitions.ToLocalisedLanguage(CarrierType);
        }

        public void UpdateCarrierStats(CarrierStats s, bool _)
        {
            s.Update(this);
        }
    }

}

