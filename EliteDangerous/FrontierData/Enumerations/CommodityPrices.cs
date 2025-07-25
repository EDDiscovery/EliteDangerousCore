﻿/*
 * Copyright © 2020-2023 EDDiscovery development team
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
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class CCommodities : System.IEquatable<CCommodities>
    {
        // beware case change - this case is matched to frontier CAPI
        public long id { get; private set; }

        [JsonName("name")]                                  // Oct 22: No sign of a JSON.FromObject use.  I think this is old but may as well maintain. Maintain CAPI output names when emitting, even though we use a different naming
        public string fdname { get; private set; }          // EDDN use : name is lower cased in CAPI but thats all to match Marketing use of it
        [JsonIgnore]
        public string fdname_unnormalised { get; private set; }  // unnormalised, with FD decoration, if present
        public string locName { get; private set; }

        [JsonName("categoryname")]
        public string category { get; private set; }        // as per market entry, its $MARKET_category_<x>; for market and CAPI. For spansh, its the name (Techonology etc)
        public string loccategory { get; private set; }     // in this context, it means, its type (Metals).. as per MaterialCommoditiesDB
        public string legality { get; private set; }        // CAPI only

        // marketjson:
        // a BUY from market entry:
        // { "id":128049204, "Name":"$explosives_name;", "Name_Localised":"Explosives", "Category":"$MARKET_category_chemicals;", "Category_Localised":"Chemicals",
        //  "BuyPrice":232,     -- display shows this is cost to buy
        //  "SellPrice":208,
        //  "MeanPrice":513,
        //  "StockBracket":3,
        //  "DemandBracket":0,
        //  "Stock":1139919,
        //  "Demand":1,
        //  "Consumer":false, "Producer":true, "Rare":false },

        // a SELL to market entry:
        // { "id":128924334, "Name":"$agronomictreatment_name;", "Name_Localised":"Agronomic Treatment", "Category":"$MARKET_category_chemicals;", "Category_Localised":"Chemicals",
        // "BuyPrice":0,
        // "SellPrice":3727,
        // "MeanPrice":3105,
        // "StockBracket":0,
        // "DemandBracket":3,
        // "Stock":0,
        // "Demand":1299,
        // "Consumer":true, "Producer":false, "Rare":false },

        // BUY from station
        public int buyPrice { get; private set; }       // Price to be paid to buy from market
        public int stock { get; private set; }          // how much station has
        public int stockBracket { get; private set; }
        [JsonIgnore]
        public bool HasStock { get { return stock > 0; } }

        // SELL to station
        public int sellPrice { get; private set; }      // price station will pay for it
        public int demand { get; private set; }         // station demand for it
        public int demandBracket { get; private set; }
        [JsonIgnore]
        public bool HasDemandAndPrice { get { return sellPrice > 0 && demand > 0; } }
        [JsonIgnore]
        public bool HasDemand { get { return demand > 0; } }
        [JsonIgnore]
        public bool HasGoodDemand { get { return demand > 1; } }        // 1 seems the default for a lot of entries..

        // Mean
        public int meanPrice { get; private set; }


        // can carry flags, such as Consumer/Producer/Rare
        public List<string> statusFlags { get; private set; }

        [JsonIgnore]
        public string ComparisionLR { get; private set; }       // NOT in Frontier data, used for market data UC during merge
        [JsonIgnore]
        public string ComparisionRL { get; private set; }       // NOT in Frontier data, used for market data UC during merge
        [JsonIgnore]
        public bool ComparisionRightOnly { get; private set; }  // NOT in Frontier data, Exists in right data only
        [JsonIgnore]
        public bool ComparisionBuy { get; private set; }        // NOT in Frontier data, its for sale at either left or right
        [JsonIgnore]
        public int CargoCarried { get; set; }                  // NOT in Frontier data, cargo currently carried for this item

        private const string marketmarker = "$MARKET_category_";

        public CCommodities()
        {
        }

        public enum ReaderType { Market, CAPI, FCMaterials, Spansh }
        public CCommodities(JObject jo, ReaderType ty )
        {
            if (ty == ReaderType.Market)
                FromJsonMarket(jo);
            else if (ty == ReaderType.CAPI)
                FromJsonCAPI(jo);
            else if (ty == ReaderType.Spansh)
                FromJsonCAPI(jo);
            else
                FromJsonFCMaterials(jo);
        }

        public CCommodities(CCommodities other)             // main fields copied, not the extra data ones
        {
            id = other.id;

            fdname = other.fdname;
            locName = other.locName;
            category = other.category;
            loccategory = other.loccategory;

            buyPrice = other.buyPrice;
            sellPrice = other.sellPrice;
            meanPrice = other.meanPrice;
            demandBracket = other.demandBracket;
            stockBracket = other.stockBracket;
            stock = other.stock;
            demand = other.demand;

            statusFlags = new List<string>(other.statusFlags);

            ComparisionLR = ComparisionRL = "";
        }

        public CCommodities(long id, string fdname, string locname, string cat, string loccat, int buyprice, int sellprice, int demandbracket, int stockbracket, int stock, int demand)
        {
            this.id = id;
            this.fdname_unnormalised = this.fdname = fdname;
            this.locName = locname;
            this.category = cat;
            if (!category.StartsWith(marketmarker))     // check its not already been fixed
                category = marketmarker + category;
            this.loccategory = loccat;
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
            this.meanPrice = (buyPrice + sellPrice) / 2;
            this.demandBracket = demandBracket;
            this.stockBracket = stockBracket;
            this.stock = stock;
            this.demand = demand;
            this.statusFlags = new List<string>();
            ComparisionLR = ComparisionRL = "";
        }

        public bool Equals(CCommodities other)
        {
            return (id == other.id && string.Compare(fdname, other.fdname) == 0 && string.Compare(locName, other.locName) == 0 &&
                     string.Compare(category, other.category) == 0 && string.Compare(loccategory, other.loccategory) == 0 &&
                     buyPrice == other.buyPrice && sellPrice == other.sellPrice && meanPrice == other.meanPrice &&
                     demandBracket == other.demandBracket && stockBracket == other.stockBracket && stock == other.stock && demand == other.demand);
        }

        public bool FromJsonCAPI(JObject jo)
        {
            try
            {
                id = jo["id"].Long();
                fdname_unnormalised = jo["name"].Str();
                fdname = fdname_unnormalised.ToLowerInvariant();

                locName = jo["locName"].Str();
                locName = locName.Alt(fdname.SplitCapsWord());      // use locname, if not there, make best loc name possible

                category = jo["categoryname"].Str();
                loccategory = jo["loccategory"].StrNull();

                if (loccategory == null)          // CAPI does not have this, so make it up.
                {
                    loccategory = category;
                    category = category.Replace(" ", "_").ToLowerInvariant().Replace("narcotics", "drugs"); // CAPI does not have a loccategory unlike market
                }

                // this normalises the category name to the same used in the market Journal entry
                if (!category.StartsWith(marketmarker))     // check its not already been fixed.. will happen when EDD entry is reread
                    category = marketmarker + category;

                legality = jo["legality"].Str();

                buyPrice = jo["buyPrice"].Int();
                sellPrice = jo["sellPrice"].Int();
                meanPrice = jo["meanPrice"].Int();
                demandBracket = jo["demandBracket"].Int();
                stockBracket = jo["stockBracket"].Int();
                stock = jo["stock"].Int();
                demand = jo["demand"].Int();

                this.statusFlags = new List<string>();
                if (jo["statusFlags"] != null)
                {
                    foreach (dynamic statusFlag in jo["statusFlags"])
                    {
                        statusFlags.Add((string)statusFlag);
                    }
                }


                ComparisionLR = ComparisionRL = "";

                // System.Diagnostics.Debug.WriteLine("CAPI field fd:'{0}' loc:'{1}' of type '{2}' '{3}'", fdname, locName, category , loccategory);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FromJsonMarket(JObject jo)
        {
            try
            {
                id = jo["id"].Long();
                fdname_unnormalised = jo["Name"].Str();
                fdname = JournalFieldNaming.FixCommodityName(fdname_unnormalised);
                locName = jo["Name_Localised"].Str();
                if (locName.IsEmpty())
                    locName = fdname.SplitCapsWordFull();

                loccategory = jo["Category_Localised"].Str();
                category = jo["Category"].Str();

                legality = "";  // not in market data

                buyPrice = jo["BuyPrice"].Int();
                sellPrice = jo["SellPrice"].Int();
                meanPrice = jo["MeanPrice"].Int();
                demandBracket = jo["DemandBracket"].Int();
                stockBracket = jo["StockBracket"].Int();
                stock = jo["Stock"].Int();
                demand = jo["Demand"].Int();

                List<string> StatusFlags = new List<string>();

                if (jo["Consumer"].Bool())
                    StatusFlags.Add("Consumer");

                if (jo["Producer"].Bool())
                    StatusFlags.Add("Producer");

                if (jo["Rare"].Bool())
                    StatusFlags.Add("Rare");

                this.statusFlags = StatusFlags;
                //System.Diagnostics.Debug.WriteLine("Market field fd:'{0}' loc:'{1}' of type '{2}' '{3}'", fdname, locName, category, loccategory);

                ComparisionLR = ComparisionRL = "";
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool FromJsonFCMaterials(JObject jo)
        {
            try
            {
                id = jo["id"].Long();
                fdname_unnormalised = jo["Name"].Str();
                fdname = JournalFieldNaming.FixCommodityName(fdname_unnormalised);
                locName = jo["Name_Localised"].Str();
                if (locName.IsEmpty())
                    locName = fdname.SplitCapsWordFull();

                loccategory = "";
                category = "";   
                legality = ""; 
                
                meanPrice = sellPrice = buyPrice = jo["Price"].Int();
                demandBracket = 0;
                stockBracket = 0;

                stock = jo["Stock"].Int();
                demand = jo["Demand"].Int();
                this.statusFlags = new List<string>(); // not present
                //System.Diagnostics.Debug.WriteLine("Market field fd:'{0}' loc:'{1}' of type '{2}' '{3}'", fdname, locName, category, loccategory);

                ComparisionLR = ComparisionRL = "";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FromJsonSpansh(JObject jo, bool dump)
        {
            try
            {
                string spanshname = jo[dump ? "name" : "commodity"].Str();
                
                var mcd = MaterialCommodityMicroResourceType.GetByEnglishName(spanshname);
                if ( mcd != null )
                {
                    fdname = fdname_unnormalised = mcd.FDName;
                    locName = mcd.TranslatedName;
                    category = mcd.Type.ToString();
                    loccategory = mcd.TranslatedType;
                }
                else
                {
                    fdname = fdname_unnormalised = locName = spanshname;
                    loccategory = category = jo["category"].Str();
                }

                legality = "";

                meanPrice = buyPrice = jo[dump ? "buyPrice" : "buy_price"].Int();
                demandBracket = 0;
                stockBracket = 0;

                sellPrice = jo[dump ? "sellPrice" : "sell_price"].Int();
                stock = jo["supply"].Int();
                demand = jo["demand"].Int();
                this.statusFlags = new List<string>(); // not present
                //System.Diagnostics.Debug.WriteLine("Market field fd:'{0}' loc:'{1}' of type '{2}' '{3}'", fdname, locName, category, loccategory);

                ComparisionLR = ComparisionRL = "";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} Buy {2} Sell {3} Mean {4}" + System.Environment.NewLine +
                                 "Stock {5} Demand {6} " + System.Environment.NewLine +
                                 "Stock Bracket {6} Demand Bracket {7}"
                                 , loccategory, locName, buyPrice, sellPrice, meanPrice, stock, demand, stockBracket, demandBracket);
        }
        public string ToStringShort()
        {
            return string.Format("{0}: {1} Buy {2} Sell {3} Stock {4} Demand {5}" , loccategory, locName, buyPrice, sellPrice, stock, demand);
        }

        public static void Sort(List<CCommodities> list)
        {
            list.Sort(delegate (CCommodities left, CCommodities right)
            {
                int cat = left.category.CompareTo(right.category);
                if (cat == 0)
                    cat = left.fdname.CompareTo(right.fdname);
                return cat;
            });
        }

        // return merged list of left and right, copied, originals left alone
        public static List<CCommodities> Merge(List<CCommodities> left, List<CCommodities> right)
        {
            List<CCommodities> merged = new List<CCommodities>();

            foreach (CCommodities l in left)
            {
                CCommodities m = new CCommodities(l);
                CCommodities r = right.Find(x => x.fdname == l.fdname);
                if (r != null)
                {
                    if (l.HasStock)     // if we can buy it..
                    {
                        m.ComparisionLR = (r.sellPrice - l.buyPrice).ToString();
                        m.ComparisionBuy = true;
                    }

                    if (r.HasStock)     // if we can buy it..
                    {
                        m.ComparisionRL = (l.sellPrice - r.buyPrice).ToString();
                        m.ComparisionBuy = true;
                    }
                }
                else
                {                                   // not found in right..
                    if (l.HasStock)             // if we can buy it here, note you can't price it in right
                        m.ComparisionLR = "No Price";
                }

                merged.Add(m);
            }

            foreach (CCommodities r in right)
            {
                CCommodities m = merged.Find(x => x.fdname == r.fdname);        // see if any in right we have not merged

                if (m == null)  // not in left list,add
                {
                    m = new CCommodities(r);
                    m.ComparisionRightOnly = true;

                    if (r.HasStock)                             // if we can buy it there, but its not in the left list
                    {
                        m.ComparisionBuy = true;
                        m.ComparisionRL = "No price";
                    }
                    merged.Add(m);
                }
            }

            Sort(merged);
            return merged;
        }

    }
}
