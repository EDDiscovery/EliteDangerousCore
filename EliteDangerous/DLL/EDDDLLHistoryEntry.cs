/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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

using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.DLL
{
    static public class EDDDLLCallerHE
    {
        static public EDDDLLInterfaces.EDDDLLIF.JournalEntry CreateFromHistoryEntry(EliteDangerousCore.HistoryList hl, EliteDangerousCore.HistoryEntry he, bool storedflag = false)
        {
            if (he == null)
            {
                return new EDDDLLInterfaces.EDDDLLIF.JournalEntry() { ver = 1, indexno = -1 };
            }
            else
            {
                EDDDLLInterfaces.EDDDLLIF.JournalEntry je = new EDDDLLInterfaces.EDDDLLIF.JournalEntry()
                {
                    ver = 2,
                    indexno = he.EntryNumber,
                    utctime = he.EventTimeUTC.ToStringZulu(),
                    name = he.EventSummary,
                    systemname = he.System.Name,
                    x = he.System.X,
                    y = he.System.Y,
                    z = he.System.Z,
                    travelleddistance = he.TravelledDistance,
                    travelledseconds = (long)he.TravelledSeconds.TotalSeconds,
                    islanded = he.IsLanded,
                    isdocked = he.IsDocked,
                    whereami = he.WhereAmI,
                    shiptype = he.ShipType,
                    gamemode = he.GameMode,
                    group = he.Group,
                    credits = he.Credits,
                    eventid = he.journalEntry.EventTypeStr,
                    json = he.journalEntry.GetJsonString(),
                    cmdrname = he.Commander.Name,
                    cmdrfid = he.Commander.FID,
                    shipident = he.ShipInformation?.ShipUserIdent ?? "Unknown",
                    shipname = he.ShipInformation?.ShipUserName ?? "Unknown",
                    hullvalue = he.ShipInformation?.HullValue ?? 0,
                    rebuy = he.ShipInformation?.Rebuy ?? 0,
                    modulesvalue = he.ShipInformation?.ModulesValue ?? 0,
                    stored = storedflag
                };

                he.journalEntry.FillInformation(he.System, out je.info, out je.detailedinfo);



                je.materials = (from x in hl.MaterialCommoditiesMicroResources.GetMaterialsSorted(he.MaterialCommodity) select x.Details.Name + ":" + x.Count.ToStringInvariant() + ":" + x.Details.FDName).ToArray();
                je.commodities = (from x in hl.MaterialCommoditiesMicroResources.GetCommoditiesSorted(he.MaterialCommodity) select x.Details.Name + ":" + x.Count.ToStringInvariant() + ":" + x.Details.FDName).ToArray();

                var ml = hl.MissionListAccumulator.GetAllCurrentMissions(he.MissionList, he.EventTimeUTC);
                je.currentmissions = ml.Select(x=>x.DLLInfo()).ToArray();
                return je;
            }
        }
    }
}
