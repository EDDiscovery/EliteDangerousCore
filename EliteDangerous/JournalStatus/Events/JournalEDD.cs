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
            Station = evt["station"].Str();
            Station_Localised = JournalFieldNaming.CheckLocalisation(evt["station_localised"].Str(), Station);
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

        public JournalEDDCommodityPrices(System.DateTime utc, long? marketid, string station, string station_localised, string starsystem, int cmdrid, JArray commds) :
                                        base(utc, JournalTypeEnum.EDDCommodityPrices, marketid, station, station_localised, starsystem, cmdrid)
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
                ["station_localised"] = Station_Localised,
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

        public JournalCommodityPricesBase(System.DateTime utc, JournalTypeEnum type, long? marketid, string station, string station_localised, string starsystem, int cmdrid)
                                            : base(utc, type)
        {
            MarketID = marketid;
            Station = station;
            Station_Localised = station_localised;
            StarSystem = starsystem;
            Commodities = new List<CCommodities>(); // always made..
            SetCommander(cmdrid);
        }

        public string Station { get; protected set; }
        public string Station_Localised { get; set; }
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
            return BaseUtils.FieldBuilder.Build("Prices on ; items".Tx(), Commodities.Count,
                                                "< at ".Tx(), Station_Localised ?? Station,
                                                "< in ".Tx(), StarSystem);
        }

        public override string GetDetailed()
        {
            if (Commodities.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                var stocked = Commodities.Where(x => x.HasStock).OrderBy(x=>x.locName);

                if (stocked.Count() > 0)
                {
                    sb.Append("Items to buy: ".Tx());
                    sb.AppendCR();

                    foreach (CCommodities c in stocked)
                    {
                        string name = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.fdname);

                        if (c.HasDemandAndPrice)
                        {
                            sb.Append("  ");
                            sb.Append(string.Format("{0}: {1} sell {2} Diff {3} {4}%  ".Tx(),
                                name, c.buyPrice, c.sellPrice, c.buyPrice - c.sellPrice,
                                ((double)(c.buyPrice - c.sellPrice) / (double)c.sellPrice * 100.0).ToString("0.#")));
                        }
                        else
                            sb.Append(string.Format("{0}: {1}  ".Tx(), name, c.buyPrice));

                        sb.AppendCR();
                    }
                }

                var sellonly = Commodities.Where(x => !x.HasStock).OrderBy(x => x.locName); ;

                if (sellonly.Count() > 0)
                {
                    sb.Append("Sell only Items: ".Tx());
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


    }
}


