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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.EDDCommodityPrices)]
    public class JournalEDDCommodityPrices : JournalCommodityPricesBase
    {
        public JournalEDDCommodityPrices(JObject evt) : base(evt, JournalTypeEnum.EDDCommodityPrices)
        {
            Station = evt["station"].Str();         // these are the old names used in EDD CP, not aligned to market (introduced before). keep
            StarSystem = evt["starsystem"].Str();
            MarketID = evt["MarketID"].LongNull();
            Rescan(evt["commodities"].Array());
        }

        public void Rescan(JArray jcommodities)
        {
            Commodities = new List<CCommodities>();

            if (jcommodities != null)
            {
                foreach (JObject commodity in jcommodities)
                {
                    CCommodities com = new CCommodities(commodity, CCommodities.ReaderType.CAPI);
                    Commodities.Add(com);
                }

                Commodities.Sort((l, r) => l.locName.CompareTo(r.locName));
            }
        }

        public JournalEDDCommodityPrices(System.DateTime utc, long? marketid, string station, string starsystem, int cmdrid, JArray commds) :
                                        base(utc, JournalTypeEnum.EDDCommodityPrices, marketid, station, starsystem, cmdrid)
        {
            Rescan(commds);
        }

        public JObject ToJSON()
        {
            JObject j = new JObject()
            {
                ["timestamp"] = EventTimeUTC.ToStringZuluInvariant(),
                ["event"] = EventTypeStr,
                ["starsystem"] = StarSystem,
                ["station"] = Station,
                ["MarketID"] = MarketID,
                ["commodities"] = JToken.FromObject(Commodities, true)
            };

            return j;
        }
    }

    public class JournalCommodityPricesBase : JournalEntry
    {
        public JournalCommodityPricesBase(JObject evt, JournalTypeEnum en) : base(evt, en)
        {
        }

        public JournalCommodityPricesBase(System.DateTime utc, JournalTypeEnum type, long? marketid, string station, string starsystem, int cmdrid)
                                            : base(utc, type, false)
        {
            MarketID = marketid;
            Station = station;
            StarSystem = starsystem;
            Commodities = new List<CCommodities>(); // always made..
            SetCommander(cmdrid);
        }

        public string Station { get; protected set; }
        public string StationType { get; protected set; } // may be "Unknown" on older events, and from CAPI
        public StationDefinitions.StarportTypes FDStationType { get; protected set; }         // FDName, may be null on older events, and from CAPI
        public string CarrierDockingAccess { get; protected set; }  // will be null when not in carrier or from CAPI
        public string StarSystem { get; set; }
        public long? MarketID { get; set; }
        public List<CCommodities> Commodities { get; protected set; }   // never null

        public bool HasCommodity(string fdname) { return Commodities.FindIndex(x => x.fdname.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase)) >= 0; }
        public bool HasCommodityToBuy(string fdname) { return Commodities.FindIndex(x => x.fdname.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase) && x.HasStock) >= 0; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Prices on ; items".T(EDCTx.JournalCommodityPricesBase_PON), Commodities.Count,
                                                "< at ".T(EDCTx.JournalCommodityPricesBase_CPBat), Station,
                                                "< in ".T(EDCTx.JournalCommodityPricesBase_CPBin), StarSystem);
        }

        public override string GetDetailed()
        {
            if (Commodities.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                var stocked = Commodities.Where(x => x.HasStock).OrderBy(x=>x.locName);

                if (stocked.Count() > 0)
                {
                    sb.Append("Items to buy: ".T(EDCTx.JournalCommodityPricesBase_Itemstobuy));
                    sb.AppendCR();

                    foreach (CCommodities c in stocked)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);

                        if (c.HasDemandAndPrice)
                        {
                            sb.Append("  ");
                            sb.Append(string.Format("{0}: {1} sell {2} Diff {3} {4}%  ".T(EDCTx.JournalCommodityPricesBase_CPBBuySell),
                                name, c.buyPrice, c.sellPrice, c.buyPrice - c.sellPrice,
                                ((double)(c.buyPrice - c.sellPrice) / (double)c.sellPrice * 100.0).ToString("0.#")));
                        }
                        else
                            sb.Append(string.Format("{0}: {1}  ".T(EDCTx.JournalCommodityPricesBase_CPBBuy), name, c.buyPrice));

                        sb.AppendCR();
                    }
                }

                var sellonly = Commodities.Where(x => !x.HasStock).OrderBy(x => x.locName); ;

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


        //When written: by EDD when a user manually sets an item count (material or commodity)
        [JournalEntryType(JournalTypeEnum.EDDItemSet)]
        public class JournalEDDItemSet : JournalEntry, ICommodityJournalEntry, IMaterialJournalEntry
        {
            public JournalEDDItemSet(JObject evt) : base(evt, JournalTypeEnum.EDDItemSet)
            {
                Materials = new MaterialListClass(evt["Materials"]?.ToObjectQ<MaterialItem[]>().ToList());
                Commodities = new CommodityListClass(evt["Commodities"]?.ToObjectQ<CommodityItem[]>().ToList());
            }

            public MaterialListClass Materials { get; set; }             // FDNAMES
            public CommodityListClass Commodities { get; set; }

            public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
            {
                if (Materials != null)
                {
                    foreach (MaterialItem m in Materials.Materials)
                        mc.Change(EventTimeUTC, m.Category, m.Name, m.Count, 0, 0, true);
                }
            }

            public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
            {
                if (Commodities != null)
                {
                    foreach (CommodityItem m in Commodities.Commodities)
                        mc.Change(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, m.Name, m.Count, (long)m.BuyPrice, 0, true);
                }
            }

            public override string GetInfo()
            {
                StringBuilder info = new StringBuilder();
                bool comma = false;
                if (Materials != null)
                {
                    foreach (MaterialItem m in Materials.Materials)
                    {
                        if (comma)
                            info.Append(", ");
                        comma = true;
                        info.Append(BaseUtils.FieldBuilder.Build("Name: ".T(EDCTx.JournalEntry_Name), MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(m.Name), "", m.Count));
                    }
                }

                if (Commodities != null)
                {
                    foreach (CommodityItem m in Commodities.Commodities)
                    {
                        if (comma)
                            info.Append(", ");
                        comma = true;
                        info.Append(BaseUtils.FieldBuilder.Build("Name: ".T(EDCTx.JournalEntry_Name), MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(m.Name), "", m.Count));
                    }
                }

                return info.ToString();
            }

            public class MaterialItem
            {
                public string Name;     //FDNAME
                public string Category;
                public int Count;
            }

            public class CommodityItem
            {
                public string Name;     //FDNAME
                public int Count;
                public double BuyPrice;
            }

            public class MaterialListClass
            {
                public MaterialListClass(System.Collections.Generic.List<MaterialItem> ma)
                {
                    Materials = ma ?? new System.Collections.Generic.List<MaterialItem>();
                    foreach (MaterialItem i in Materials)
                        i.Name = JournalFieldNaming.FDNameTranslation(i.Name);
                }

                public System.Collections.Generic.List<MaterialItem> Materials { get; protected set; }
            }

            public class CommodityListClass
            {
                public CommodityListClass(System.Collections.Generic.List<CommodityItem> ma)
                {
                    Commodities = ma ?? new System.Collections.Generic.List<CommodityItem>();
                    foreach (CommodityItem i in Commodities)
                        i.Name = JournalFieldNaming.FDNameTranslation(i.Name);
                }

                public System.Collections.Generic.List<CommodityItem> Commodities { get; protected set; }
            }
        }

    }
}


