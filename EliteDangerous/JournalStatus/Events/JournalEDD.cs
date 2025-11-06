/*
 * Copyright 2016-2023 EDDiscovery development team
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

using EliteDangerousCore.StarScan2;
using QuickJSON;
using System;
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

    [JournalEntryType(JournalTypeEnum.EDDDestinationSelected)]
    public class JournalEDDDestinationSelected : JournalEntry, IStarScan
    {
        public JournalEDDDestinationSelected(JObject evt) : base(evt, JournalTypeEnum.EDDDestinationSelected)
        {
            Target_BodyID = evt["BodyID"].Int(-1);
            Target_SystemAddress = evt["SystemAddress"].Long(-1);
            TargetName = evt["TargetName"].Str();
            TargetName_Localised = evt["TargetName_Localised"].StrNull();
        }

        public JournalEDDDestinationSelected(DateTime utc, int cmdrid, long systemaddress, int bodyid, string targetname, string targetname_localised) : base(utc,JournalTypeEnum.EDDDestinationSelected)
        {
            Target_SystemAddress = systemaddress;
            Target_BodyID = bodyid;
            TargetName = targetname;
            TargetName_Localised = TargetName_Localised;
            SetCommander(cmdrid);
        }

        public string TargetName { get; set; }          // these are set when UI Destination changes
        public string TargetName_Localised { get; set; }
        public long Target_SystemAddress { get; set; }
        public int  Target_BodyID { get; set; }

        public string SystemName { get; set; }      // seen in, set by starscan
        public long? SystemAddress { get; set; }     // seen in, set by starscan

        public override string SummaryName(ISystem sys)
        {
            return "Destination Selected";
        }
        public override string GetInfo()
        {
            return $"{Target_SystemAddress}:{Target_BodyID} {TargetName_Localised??TargetName} in {SystemName}";
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            SystemName = system.Name;
            SystemAddress = system.SystemAddress;
            s.AddDestinationSelected(this, system);
        }

        public JObject ToJSON()
        {
            JObject j = new JObject()
            {
                ["timestamp"] = EventTimeUTC.ToStringZuluInvariant(),
                ["event"] = EventTypeStr,
                ["BodyID"] = Target_BodyID,
                ["SystemAddress"] = Target_SystemAddress,
                ["TargetName"] = TargetName,
                ["TargetName_Localised"] = TargetName_Localised,
            };

            return j;
        }
    }


}


