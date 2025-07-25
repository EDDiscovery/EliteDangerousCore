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
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.RepairDrone)]
    public class JournalRepairDrone : JournalEntry
    {
        public JournalRepairDrone(JObject evt ) : base(evt, JournalTypeEnum.RepairDrone)
        {
            HullRepaired = evt["HullRepaired"].Double();
            CockpitRepaired = evt["CockpitRepaired"].Double();
            CorrosionRepaired = evt["CorrosionRepaired"].Double();
        }

        public double HullRepaired { get; set; }
        public double CockpitRepaired { get; set; }
        public double CorrosionRepaired { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("Hull: ".T(EDCTx.JournalRepairDrone_Hull), HullRepaired.ToString("0.0"), "Cockpit: ".T(EDCTx.JournalEntry_Cockpit), CockpitRepaired.ToString("0.0"), 
                                "Corrosion: ".T(EDCTx.JournalEntry_Corrosion), CorrosionRepaired.ToString("0.0"));
        }
    }


    [JournalEntryType(JournalTypeEnum.BuyDrones)]
    public class JournalBuyDrones : JournalEntry, ILedgerJournalEntry, ICommodityJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalBuyDrones(JObject evt) : base(evt, JournalTypeEnum.BuyDrones)
        {
            Type = evt["Type"].Str();
            Count = evt["Count"].Int();
            BuyPrice = evt["BuyPrice"].Long();
            TotalCost = evt["TotalCost"].Long();

        }
        public string Type { get; set; }
        public int Count { get; set; }
        public long BuyPrice { get; set; }
        public long TotalCost { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type, Count = Count } }; } }

        public string FDNameOfItem { get { return Type; } }        // implement IStatsJournalEntryMatCommod
        public int CountOfItem { get { return Count; } }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, "drones", Count, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system, "drones", Count, 0, stationfaction);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Type + " " + Count + " drones", -TotalCost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Type: ".T(EDCTx.JournalEntry_Type), Type, "Count: ".T(EDCTx.JournalEntry_Count), Count, "Total Cost: ; cr;N0".T(EDCTx.JournalEntry_TotalCost), TotalCost, "each: ; cr;N0".T(EDCTx.JournalEntry_each), BuyPrice);
        }
    }


    [JournalEntryType(JournalTypeEnum.SellDrones)]
    public class JournalSellDrones : JournalEntry, ILedgerJournalEntry, ICommodityJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalSellDrones(JObject evt) : base(evt, JournalTypeEnum.SellDrones)
        {
            Type = evt["Type"].Str();
            Count = evt["Count"].Int();
            SellPrice = evt["SellPrice"].Long();
            TotalSale = evt["TotalSale"].Long();
        }
        public string Type { get; set; }
        public int Count { get; set; }
        public long SellPrice { get; set; }
        public long TotalSale { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type, Count = -Count } }; } }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, "drones", -Count, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system, "drones", -Count, 0, stationfaction);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Count.ToString() + " " + "Drones".T(EDCTx.JournalEntry_Drones), TotalSale);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Type, "Count: ".T(EDCTx.JournalEntry_Count), Count, "Price: ; cr;N0".T(EDCTx.JournalEntry_Price), SellPrice, "Amount: ; cr;N0".T(EDCTx.JournalEntry_Amount), TotalSale);
        }
    }

    [JournalEntryType(JournalTypeEnum.LaunchDrone)]
    public class JournalLaunchDrone : JournalEntry, ICommodityJournalEntry
    {
        public JournalLaunchDrone(JObject evt) : base(evt, JournalTypeEnum.LaunchDrone)
        {
            Type = evt["Type"].Enumeration<DroneType>(DroneType.Prospector);
            FriendlyType = Type.ToString();
        }

        public enum DroneType { Prospector, Collection, Hatchbreaker, FuelTransfer, Repair, Research, Decontamination , Recon }

        public DroneType Type { get; set; }
        public string FriendlyType { get; set; }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv) 
        {
            mc.ChangeCommd( EventTimeUTC, "drones", -1, 0);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Type: ".T(EDCTx.JournalEntry_Type), FriendlyType);
        }
    }


}
