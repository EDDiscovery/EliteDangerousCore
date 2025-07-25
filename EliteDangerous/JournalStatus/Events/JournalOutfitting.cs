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

using EliteDangerousCore;
using QuickJSON;
using System.Linq;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Outfitting)]
    public class JournalOutfitting : JournalEntry, IAdditionalFiles
    {
        public JournalOutfitting(JObject evt) : base(evt, JournalTypeEnum.Outfitting)
        {
            Rescan(evt);
        }

        public JournalOutfitting(DateTime utc, string sn, string snloc, string sys, long mid, Tuple<long, string, long>[] list, int cmdrid, bool horizons = true) :
            base(utc, JournalTypeEnum.Outfitting)
        {
            MarketID = mid;
            Horizons = horizons;
            var nlist = list.Select(x => new Outfitting.OutfittingItem { id = x.Item1, Name = x.Item2, BuyPrice = x.Item3 }).ToArray();
            YardInfo = new Outfitting(sn, snloc, sys, utc, nlist);
            SetCommander(cmdrid);
        }

        public void Rescan(JObject evt)
        {
            var snloc = JournalFieldNaming.GetStationNames(evt);
            YardInfo = new Outfitting(snloc.Item1, snloc.Item2, evt["StarSystem"].Str(), EventTimeUTC, evt["Items"]?.ToObjectQ<Outfitting.OutfittingItem[]>());
            MarketID = evt["MarketID"].LongNull();
            Horizons = evt["Horizons"].BoolNull();
        }

        public void ReadAdditionalFiles(string directory)
        {
            JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "Outfitting.json"), EventTypeStr);
            if (jnew != null)        // new json, rescan
            {
                Rescan(jnew);
                UpdateJson(jnew);
            }
        }

        public JObject ToJSON()
        {
            JArray itemlist = new JArray(YardInfo.Items.Select(x => new JObject() { { "id", x.id }, { "Name", x.FDName }, { "BuyPrice", x.BuyPrice } }));

            JObject j = new JObject()
            {
                ["timestamp"] = EventTimeUTC.ToStringZuluInvariant(),
                ["event"] = EventTypeStr,
                ["StationName"] = YardInfo.StationName,
                ["StarSystem"] = YardInfo.StarSystem,
                ["MarketID"] = MarketID,
                ["Horizons"] = Horizons,
                ["Items"] = itemlist,
            };

            return j;
        }


        public Outfitting YardInfo;

        public long? MarketID { get; set; }
        public bool? Horizons { get; set; }

        public override string GetInfo() 
        {
            return YardInfo.Items != null ? (YardInfo.Items.Length.ToString() + " items available".T(EDCTx.JournalEntry_itemsavailable)) : "";
        }

        public override string GetDetailed()
        {
            if (YardInfo.Items != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Outfitting.OutfittingItem m in YardInfo.Items.OrderBy(x=>x.TranslatedModuleName))
                {
                    sb.Append(m.TranslatedModuleName);
                    sb.Append(":");
                    sb.Append(m.BuyPrice.ToString("N0"));
                    sb.AppendCR();
                }
                return sb.ToString();
            }
            else
                return null;
        }

    }
}
