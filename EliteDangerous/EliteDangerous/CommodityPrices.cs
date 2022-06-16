/*
 * Copyright © 2020 EDDiscovery development team
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using QuickJSON;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class CCommodities : System.IEquatable<CCommodities>
    {
        public int id { get; private set; }

        [JsonName("name")]                                  // Maintain CAPI output names when emitting, even though we use a different naming
        public string fdname { get; private set; }          // EDDN use : name is lower cased in CAPI but thats all to match Marketing use of it
        public string locName { get; private set; }

        [JsonName("categoryname")]
        public string category { get; private set; }        // in this context, it means, its type (Metals).. as per MaterialCommoditiesDB
        public string loccategory { get; private set; }     // in this context, it means, its type (Metals).. as per MaterialCommoditiesDB
        public string legality { get; private set; }        // CAPI only

        [JsonIgnore]
        public bool CanBeBought { get { return buyPrice > 0 && stock > 0; } }
        [JsonIgnore]
        public bool CanBeSold { get { return sellPrice > 0 && demand > 0; } }
        [JsonIgnore]
        public bool HasDemand { get { return demand > 1; } }        // 1 because lots of them are marked as 1, as in, they want it, but not much

        public int buyPrice { get; private set; }

        public int sellPrice { get; private set; }
        public int meanPrice { get; private set; }
        public int demandBracket { get; private set; }
        public int stockBracket { get; private set; }
        public int stock { get; private set; }
        public int demand { get; private set; }

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

        public enum ReaderType { Market, CAPI, FCMaterials }
        public CCommodities(JObject jo, ReaderType ty )
        {
            if (ty == ReaderType.Market)
                FromJsonMarket(jo);
            else if (ty == ReaderType.CAPI)
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
                id = jo["id"].Int();
                fdname = jo["name"].Str().ToLowerInvariant();

                locName = jo["locName"].Str();
                locName = locName.Alt(fdname.SplitCapsWord());      // use locname, if not there, make best loc name possible

                category = jo["categoryname"].Str();
                loccategory = jo["loccategory"].StrNull();

                if (loccategory == null)          // CAPI does not have this, so make it up.
                {
                    loccategory = category;
                    category = category.Replace(" ", "_").ToLowerInvariant().Replace("narcotics", "drugs"); // CAPI does not have a loccategory unlike market
                }

                const string marketmarker = "$MARKET_category_";
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
                foreach (dynamic statusFlag in jo["statusFlags"])
                {
                    statusFlags.Add((string)statusFlag);
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
                id = jo["id"].Int();
                fdname = JournalFieldNaming.FixCommodityName(jo["Name"].Str());
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
                fdname = JournalFieldNaming.FixCommodityName(jo["Name"].Str());
                locName = jo["Name_Localised"].Str();
                if (locName.IsEmpty())
                    locName = fdname.SplitCapsWordFull();

                loccategory = "";
                category = "Microresources";        // fixed

                legality = "";  // not in market data
                
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

        public override string ToString()
        {
            return string.Format("{0} : {1} Buy {2} Sell {3} Mean {4}" + System.Environment.NewLine +
                                 "Stock {5} Demand {6} " + System.Environment.NewLine + 
                                 "Stock Bracket {6} Demand Bracket {7}"
                                 , loccategory, locName, buyPrice, sellPrice, meanPrice, stock, demand, stockBracket, demandBracket );
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

        public static List<CCommodities> Merge(List<CCommodities> left, List<CCommodities> right)
        {
            List<CCommodities> merged = new List<CCommodities>();

            foreach (CCommodities l in left)
            {
                CCommodities m = new CCommodities(l);
                CCommodities r = right.Find(x => x.fdname == l.fdname);
                if (r != null)
                {
                    if (l.CanBeBought)     // if we can buy it..
                    {
                        m.ComparisionLR = (r.sellPrice - l.buyPrice).ToString();
                        m.ComparisionBuy = true;
                    }

                    if (r.CanBeBought)     // if we can buy it..
                    {
                        m.ComparisionRL = (l.sellPrice - r.buyPrice).ToString();
                        m.ComparisionBuy = true;
                    }
                }
                else
                {                                   // not found in right..
                    if (l.CanBeBought)             // if we can buy it here, note you can't price it in right
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

                    if (r.CanBeBought)                             // if we can buy it there, but its not in the left list
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
