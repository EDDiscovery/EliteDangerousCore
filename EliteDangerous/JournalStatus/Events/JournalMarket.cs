﻿/*
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

// use to provide more data points for testing
//#define FAKEMARKETDATA

using QuickJSON;
using System.Collections.Generic;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Market)]
    public class JournalMarket : JournalCommodityPricesBase, IAdditionalFiles
    {
        public JournalMarket(JObject evt) : base(evt, JournalTypeEnum.Market)
        {
            Rescan(evt);
        }

        public void Rescan(JObject evt)
        {
            var snl = JournalFieldNaming.GetStationNames(evt);
            Station = snl.Item1;
            Station_Localised = snl.Item2;
            FDStationType = StationDefinitions.StarportTypeToEnum(evt["StationType"].StrNull());    // may not be present
            StationType = StationDefinitions.ToEnglish(FDStationType);
            CarrierDockingAccess = evt["CarrierDockingAccess"].StrNull();
            StarSystem = evt["StarSystem"].Str();
            MarketID = evt["MarketID"].LongNull();

            Commodities = new List<CCommodities>(); // always made..
            JArray jcommodities = (JArray)evt["Items"];
            if (jcommodities != null )
            {
                foreach (JObject commodity in jcommodities)
                {
                    CCommodities com = new CCommodities(commodity, CCommodities.ReaderType.Market);
                    Commodities.Add(com);
                }
            }
            else
            {
#if FAKEMARKETDATA
                var commds = MaterialCommodityMicroResourceType.GetCommodities(MaterialCommodityMicroResourceType.SortMethod.None);
                foreach (var x in commds)
                {
                    if (rnd.Next(5) == 0)
                    {
                        int buyprice = 100 + rnd.Next(200);
                        int stock = 12000 + rnd.Next(12000);
                        Commodities.Add(new CCommodities(-1, x.FDName, x.TranslatedName, x.Type.ToString(), x.TranslatedType, buyprice, buyprice - 100, 1, 1, stock, 1));
                    }
                }
#endif

            }
        }

        public bool Equals(JournalMarket other)
        {
            return string.Compare(Station, other.Station) == 0 && string.Compare(StarSystem, other.StarSystem) == 0 && CollectionStaticHelpers.Equals(Commodities, other.Commodities);
        }

        public void ReadAdditionalFiles(string directory)
        {
            JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "Market.json"), EventTypeStr);
            if (jnew != null)        // new json, rescan
            {
                Rescan(jnew);
                UpdateJson(jnew);
            }
        }

#if FAKEMARKETDATA
        System.Random rnd = new System.Random(1);
#endif


    }


    [JournalEntryType(JournalTypeEnum.MarketBuy)]
    public class JournalMarketBuy : JournalEntry, ICommodityJournalEntry, ILedgerJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalMarketBuy(JObject evt) : base(evt, JournalTypeEnum.MarketBuy)
        {
            MarketID = evt["MarketID"].LongNull();
            Type = evt["Type"].Str();        // must be FD name
            Type = JournalFieldNaming.FDNameTranslation(Type);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyType = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Type);           // our translation..
            Type_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Type_Localised"].Str(), FriendlyType);         // always ensure we have one
            Count = evt["Count"].Int();
            BuyPrice = evt["BuyPrice"].Long();
            TotalCost = evt["TotalCost"].Long();
        }

        public string Type { get; set; }                // FDNAME
        public string Type_Localised { get; set; }      // Always set
        public string FriendlyType { get; set; }        // translated name
        public int Count { get; set; }
        public long BuyPrice { get; set; }
        public long TotalCost { get; set; }
        public long? MarketID { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type, Count = Count } }; } }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, Type, Count, BuyPrice);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system,Type, Count, 0, stationfaction);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, FriendlyType + " " + Count, -TotalCost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", FriendlyType, "", Count, "< buy price ; cr;N0".T(EDCTx.JournalEntry_buyprice), BuyPrice, "Total Cost: ; cr;N0".T(EDCTx.JournalEntry_TotalCost), TotalCost);
        }
    }


    [JournalEntryType(JournalTypeEnum.MarketSell)]
    public class JournalMarketSell : JournalEntry, ICommodityJournalEntry, ILedgerJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalMarketSell(JObject evt) : base(evt, JournalTypeEnum.MarketSell)
        {
            MarketID = evt["MarketID"].LongNull();
            Type = evt["Type"].Str();                           // FDNAME
            Type = JournalFieldNaming.FDNameTranslation(Type);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyType = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Type); // goes thru the translator..
            Type_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Type_Localised"].Str(), FriendlyType);         // always ensure we have one
            Count = evt["Count"].Int();
            SellPrice = evt["SellPrice"].Long();
            TotalSale = evt["TotalSale"].Long();
            AvgPricePaid = evt["AvgPricePaid"].Long();
            IllegalGoods = evt["IllegalGoods"].Bool();
            StolenGoods = evt["StolenGoods"].Bool();
            BlackMarket = evt["BlackMarket"].Bool();
        }

        public string Type { get; set; }
        public string FriendlyType { get; set; }
        public string Type_Localised { get; set; }      // always set

        public int Count { get; set; }
        public long SellPrice { get; set; }
        public long TotalSale { get; set; }
        public long AvgPricePaid { get; set; }
        public bool IllegalGoods { get; set; }
        public bool StolenGoods { get; set; }
        public bool BlackMarket { get; set; }
        public long? MarketID { get; set; }

        public long Profit { get { return (SellPrice - AvgPricePaid) * Count; } }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type, Count = -Count, Profit = Profit } }; } }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, Type, -Count, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
                stats.UpdateCommodity(system, Type, -Count, Profit, stationfaction);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, FriendlyType + " " + Count + " Avg " + AvgPricePaid, TotalSale, Profit, (double)(SellPrice - AvgPricePaid));
        }

        public override string GetInfo()
        {
            long profit = TotalSale - (AvgPricePaid * Count);
            return BaseUtils.FieldBuilder.Build("", FriendlyType, "", Count, "< sell price ; cr;N0".T(EDCTx.JournalEntry_sellprice), SellPrice, "Total Sale: ; cr;N0".T(EDCTx.JournalEntry_TotalSale), TotalSale, "Profit: ; cr;N0".T(EDCTx.JournalEntry_Profit), profit);
        }

        public override string GetDetailed()
        {
            return BaseUtils.FieldBuilder.Build("Legal;Illegal".T(EDCTx.JournalEntry_Legal), IllegalGoods, "Not Stolen;Stolen".T(EDCTx.JournalEntry_NotStolen), StolenGoods, "Market;BlackMarket".T(EDCTx.JournalEntry_Market), BlackMarket);
        }
    }

}
