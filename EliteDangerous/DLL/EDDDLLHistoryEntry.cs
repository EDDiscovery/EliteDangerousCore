/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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
using System.Linq;

namespace EliteDangerousCore.DLL
{
    static public class EDDDLLCallerHE
    {
        static public EDDDLLInterfaces.EDDDLLIF.JournalEntry CreateFromHistoryEntry(EliteDangerousCore.HistoryList hl, EliteDangerousCore.HistoryEntry he,
                                                                                     bool storedflag = false)
        {
            if (he == null)
                return new EDDDLLInterfaces.EDDDLLIF.JournalEntry() { ver = 5, indexno = -1 };
            else
                return CreateFromHistoryEntry(he, hl.MaterialCommoditiesMicroResources.GetMaterialsSorted(he.MaterialCommodity),
                                              hl.MaterialCommoditiesMicroResources.GetCommoditiesSorted(he.MaterialCommodity),
                                              hl.MaterialCommoditiesMicroResources.GetMicroResourcesSorted(he.MaterialCommodity),
                                              hl.MissionListAccumulator.GetAllCurrentMissions(he.MissionList, he.EventTimeUTC),
                                              hl.Count,
                                              storedflag);
        }

        // EDDLite uses this
        static public EDDDLLInterfaces.EDDDLLIF.JournalEntry CreateFromHistoryEntry(EliteDangerousCore.HistoryEntry he, List<MaterialCommodityMicroResource> list,
                                                                                      List<MissionState> missionlist,
                                                                                      bool storedflag = false)
        {
            if (he == null)
                return new EDDDLLInterfaces.EDDDLLIF.JournalEntry() { ver = 3, indexno = -1 };
            else
            {
                var mats = list.Where(x => x.Details.IsMaterial).OrderBy(x => x.Details.Type).ToList();
                var commods = list.Where(x => x.Details.IsCommodity).OrderBy(x => x.Details.Type).ToList();
                var mr = list.Where(x => x.Details.IsMicroResources).OrderBy(x => x.Details.Type).ToList();
                return CreateFromHistoryEntry(he, mats, commods, mr, missionlist, 0, storedflag);
            }
        }

        static public int JournalVersion = 5;       // keep this up to date

        static private EDDDLLInterfaces.EDDDLLIF.JournalEntry CreateFromHistoryEntry(EliteDangerousCore.HistoryEntry he,
                                                                                         List<MaterialCommodityMicroResource> mats,
                                                                                         List<MaterialCommodityMicroResource> commds,
                                                                                         List<MaterialCommodityMicroResource> mr,
                                                                                         List<MissionState> missionlist,
                                                                                         int totalrecords,
                                                                                         bool storedflag = false)
        {
            JObject json = he.journalEntry.GetJsonCloned();
            json.RemoveWildcard("EDD*");        // remove any EDD specials

            EDDDLLInterfaces.EDDDLLIF.JournalEntry je = new EDDDLLInterfaces.EDDDLLIF.JournalEntry()
            {
                ver = 5,
                //v1
                indexno = he.EntryNumber,
                utctime = he.EventTimeUTC.ToStringZuluInvariant(),
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
                shiptype = he.Status.ShipType,      // 
                gamemode = he.GameMode,
                group = he.Group,
                credits = he.Credits,
                eventid = he.journalEntry.EventTypeStr,
                jid = he.journalEntry.Id,
                totalrecords = totalrecords,

                // v2
                json = json.ToString(),
                cmdrname = he.Commander.Name,
                cmdrfid = he.Commander.FID,
                shipident = he.ShipInformation?.ShipUserIdent ?? "Unknown",
                shipname = he.ShipInformation?.ShipUserName ?? "Unknown",
                hullvalue = he.ShipInformation?.HullValue ?? 0,
                rebuy = he.ShipInformation?.Rebuy ?? 0,
                modulesvalue = he.ShipInformation?.ModulesValue ?? 0,
                stored = storedflag,

                //v3
                travelstate = he.Status.TravelState.ToString(),

                //v4
                horizons = he.journalEntry.IsHorizons,
                odyssey = he.journalEntry.IsOdyssey,
                beta = he.journalEntry.IsBeta,

                //v5
                wanted = he.Status.Wanted,
                bodyapproached = he.Status.BodyApproached,
                bookeddropship = he.Status.BookedDropship,
                issrv = he.Status.IsSRV,
                isfighter = he.Status.IsFighter,
                onfoot = he.Status.OnFoot,
                bookedtaxi = he.Status.BookedTaxi,

                bodyname = he.Status.BodyName ?? "Unknown",
                bodytype = he.Status.BodyType ?? "Unknown",
                stationname = he.Status.StationName ?? "Unknown",
                stationtype = he.Status.StationType ?? "Unknown",
                stationfaction = he.Status.StationFaction ?? "Unknown",
                shiptypefd = he.Status.ShipTypeFD ?? "Unknown",
                oncrewwithcaptain = he.Status.OnCrewWithCaptain ?? "",
                shipid = he.Status.ShipID,
                bodyid = he.Status.BodyID ?? -1,
            };

            // v1
            he.journalEntry.FillInformation(he.System, he.WhereAmI, out je.info, out je.detailedinfo);
            je.materials = (from x in mats select x.Details.Name + ":" + x.Count.ToStringInvariant() + ":" + x.Details.FDName).ToArray();
            je.commodities = (from x in commds select x.Details.Name + ":" + x.Count.ToStringInvariant() + ":" + x.Details.FDName).ToArray();
            je.currentmissions = missionlist.Select(x=>x.DLLInfo()).ToArray();

            // v2
            je.microresources = (from x in mr select x.Details.Name + ":" + x.Counts[0].ToStringInvariant()+ ":" + x.Counts[1].ToStringInvariant() + ":" + x.Details.FDName).ToArray();
            return je;
        }
    }
}
