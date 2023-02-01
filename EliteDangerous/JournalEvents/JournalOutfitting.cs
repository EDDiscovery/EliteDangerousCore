/*
 * Copyright © 2016-2018 EDDiscovery development team
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

        public JournalOutfitting(DateTime utc, string sn, string sys, long mid, Tuple<long, string, long>[] list, int cmdrid, bool horizons = true) :
            base(utc, JournalTypeEnum.Outfitting, false)
        {
            MarketID = mid;
            Horizons = horizons;
            var nlist = list.Select(x => new Outfitting.OutfittingItem { id = x.Item1, Name = x.Item2, BuyPrice = x.Item3 }).ToArray();
            YardInfo = new Outfitting(sn, sys, utc, nlist);
            SetCommander(cmdrid);
        }

        public void Rescan(JObject evt)
        {
            YardInfo = new Outfitting(evt["StationName"].Str(), evt["StarSystem"].Str(), EventTimeUTC, evt["Items"]?.ToObjectQ<Outfitting.OutfittingItem[]>());
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

        public override void FillInformation(FillInformationData fidunused, out string info, out string detailed) 
        {
            info = "";
            detailed = "";

            if (YardInfo.Items != null)
            {
                info = YardInfo.Items.Length.ToString() + " items available".T(EDCTx.JournalEntry_itemsavailable);
                int itemno = 0;
                foreach (Outfitting.OutfittingItem m in YardInfo.Items)
                {
                    detailed = detailed.AppendPrePad(m.Name + ":" + m.BuyPrice.ToString("N0"), (itemno % 3 < 2) ? ", " : System.Environment.NewLine);
                    itemno++;
                }
            }
                
        }
    }
}
