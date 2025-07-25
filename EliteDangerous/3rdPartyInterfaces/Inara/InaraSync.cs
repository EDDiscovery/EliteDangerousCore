﻿/*
 * Copyright © 2018 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuickJSON;

namespace EliteDangerousCore.Inara
{
    public static class InaraSync
    {
        private class InaraQueueEntry
        {
            public JToken eventinfo;
            public Action<string> logger;
            public EDCommander cmdr;
            public string cmdrfid;
            public bool verbose;
        }

        private static ConcurrentQueue<InaraQueueEntry> eventqueue = new ConcurrentQueue<InaraQueueEntry>();
        private static Thread ThreadInaraSync;
        private static int running = 0;
        private static bool Exit = false;
        private static AutoResetEvent queuedevents = new AutoResetEvent(false);
        private static ManualResetEvent exitevent = new ManualResetEvent(false);
        private static long CmdrCredits = 0;

        private static JournalRank ranksave;            // save rank, apply at next progress

        #region Public IF

        public static bool Refresh(Action<string> logger, HistoryEntry lasthe, EDCommander cmdr)
        {
            List<JToken> events = RefreshList(lasthe);
            if (events.Count > 0)
                Submit(events,logger, cmdr, true);
            return true;
        }

        public static bool HistoricData(Action<string> logger, HistoryList history, EDCommander cmdr)
        {
            List<JToken> events = HistoricList(history);
            if (events.Count > 0)
                Submit(events, logger, cmdr, true);

            return true;
        }

        public static bool NewEvent(Action<string> logger, HistoryList hl, HistoryEntry he)
        {
            var mcmr = hl.MaterialCommoditiesMicroResources.GetDict(he.MaterialCommodity);
            List<JToken> events = NewEntryList(he, he.EventTimeUTC, mcmr);
            if (events.Count > 0)
                Submit(events, logger, he.Commander, false);
            return true;
        }

        public static bool NewEvent(Action<string> logger, HistoryEntry he, Dictionary<string,MaterialCommodityMicroResource> mcmr)
        {
            List<JToken> events = NewEntryList(he, he.EventTimeUTC, mcmr);
            if (events.Count > 0)
                Submit(events, logger, he.Commander, false);
            return true;
        }

        public static void StopSync()
        {
            Exit = true;
            exitevent.Set();
            queuedevents.Set();     // also trigger in case we are in thread hold
        }

        #endregion

        #region Formatters

        public static List<JToken> RefreshList(HistoryEntry last)         // may create NULL entries if some material items not found
        {
            List<JToken> eventstosend = new List<JToken>();

            if (last != null && last.EventTimeUTC > DateTime.UtcNow.AddDays(-30))
            {
                var si = last.ShipInformation;

                if (si != null)
                {
                    eventstosend.Add(InaraClass.setCommanderShip(si.ShipFD, si.ID, last.EventTimeUTC,
                                                                si.ShipUserName, si.ShipUserIdent, true, si.Hot,
                                                                si.HullValue, si.ModulesValue, si.Rebuy, last.System.Name,
                                                                last.Status.IsDocked ? last.WhereAmI : null, last.Status.IsDocked ? last.Status.MarketID : null));
                }

                eventstosend.Add(InaraClass.setCommanderCredits(last.Credits, last.Assets, last.Loan, last.EventTimeUTC));

                eventstosend.Add(InaraClass.setCommanderTravelLocation(last.System.Name, last.Status.IsDocked ? last.WhereAmI : null, last.Status.MarketID.HasValue ? last.Status.MarketID : null, last.EventTimeUTC));

                CmdrCredits = last.Credits;
            }

            return eventstosend;
        }

        public static List<JToken> HistoricList(HistoryList history)         // may create NULL entries if some material items not found
        {
            List<JToken> eventstosend = new List<JToken>();

            HistoryEntry last = history.GetLast;

            if (last != null && last.EventTimeUTC > DateTime.UtcNow.AddDays(-30))
            {
                DateTime tb = DateTime.UtcNow.AddSeconds(-120);     // we space out the time to avoid Inaras checking of duplicate sends (Jan 24 discovery)
                int sendno = 0;

                foreach( var s in history.ShipInformationList.Ships )
                {
                    Ship si = s.Value;
                    if ( si.State == Ship.ShipState.Owned && ItemData.IsShip(si.ShipFD))
                    {
                        // loadout may be null if nothing in it.
                        eventstosend.Add(InaraClass.setCommanderShipLoadout(si.ShipFD, si.ID, si.Modules.Values, tb.AddSeconds(sendno++)));

                        eventstosend.Add(InaraClass.setCommanderShip(si.ShipFD, si.ID, tb.AddSeconds(sendno++),
                                                                si.ShipUserName, si.ShipUserIdent, last.ShipInformation.ID == si.ID, null,
                                                                si.HullValue, si.ModulesValue, si.Rebuy, si.StoredAtSystem, si.StoredAtStation));
                    }
                }

                eventstosend.Add(InaraClass.setCommanderStorageModules(last.StoredModules.StoredModules, tb.AddSeconds(sendno++)));
            }

            eventstosend = eventstosend.Where(x => x != null).ToList();     // remove any nulls

            return eventstosend;
        }


        public static List<JToken> NewEntryList(HistoryEntry he, DateTime heutc, Dictionary<string, MaterialCommodityMicroResource> mcmr)         // may create NULL entries if some material items not found
        {
            List<JToken> eventstosend = new List<JToken>();

            switch (he.journalEntry.EventTypeID)
            {
                case JournalTypeEnum.ShipyardBuy: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalShipyardBuy;

                        if (je.StoreOldShipFD != null && je.StoreOldShipId.HasValue)
                            eventstosend.Add(InaraClass.setCommanderShip(je.StoreOldShipFD, je.StoreOldShipId.Value, heutc, curship: false, starsystemName: he.System.Name, stationName: he.WhereAmI));
                        if (je.SellOldShipFD != null && je.SellOldShipId.HasValue)
                            eventstosend.Add(InaraClass.delCommanderShip(je.SellOldShipFD, je.SellOldShipId.Value, heutc));

                        eventstosend.Add(InaraClass.setCommanderCredits(he.Credits, he.Assets, he.Loan, heutc));
                        CmdrCredits = he.Credits;

                        break;
                    }

                case JournalTypeEnum.ShipyardNew:       // into a new ship // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalShipyardNew;
                        eventstosend.Add(InaraClass.setCommanderShip(je.ShipFD, je.ShipId, heutc, curship: true, starsystemName: he.System.Name, stationName: he.WhereAmI));
                        break;
                    }

                case JournalTypeEnum.ShipyardSell: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalShipyardSell;
                        eventstosend.Add(InaraClass.delCommanderShip(je.ShipTypeFD, je.SellShipId, heutc));
                        eventstosend.Add(InaraClass.setCommanderCredits(he.Credits, he.Assets, he.Loan, heutc));
                        CmdrCredits = he.Credits;
                        break;
                    }

                case JournalTypeEnum.ShipyardSwap: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalShipyardSwap;
                        eventstosend.Add(InaraClass.setCommanderShip(je.StoreOldShipFD, je.StoreShipId.Value, heutc, curship: false, starsystemName: he.System.Name, stationName: he.WhereAmI));
                        eventstosend.Add(InaraClass.setCommanderShip(je.ShipFD, je.ShipId, heutc, curship: true, starsystemName: he.System.Name, stationName: he.WhereAmI));
                        break;
                    }

                case JournalTypeEnum.ShipyardTransfer: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalShipyardTransfer;
                        eventstosend.Add(InaraClass.setCommanderShipTransfer(je.ShipTypeFD, je.ShipId, he.System.Name, he.WhereAmI, he.Status.MarketID, je.nTransferTime ?? 0, heutc));
                        break;
                    }

                case JournalTypeEnum.Loadout: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalLoadout;
                        var si = he.ShipInformation;
                        if (si != null && je.ShipFD.HasChars() && ItemData.IsShip(je.ShipFD)) // if it has an FDname (defensive) and is not SRV/Fighter
                        {
                            if (je.ShipId == si.ID)
                            {
                                eventstosend.Add(InaraClass.setCommanderShipLoadout(je.ShipFD, je.ShipId, si.Modules.Values, heutc));
                                eventstosend.Add(InaraClass.setCommanderShip(je.ShipFD, je.ShipId, heutc,
                                                                        je.ShipName, je.ShipIdent, true, null,
                                                                        je.HullValue, je.ModulesValue, je.Rebuy));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR IN EDD Inara system:Current ship does not match loadout");
                            }
                        }
                        break;
                    }

                case JournalTypeEnum.StoredModules: // VERIFIED 18/5/2018 from historic upload test
                    {
                        eventstosend.Add(InaraClass.setCommanderStorageModules(he.StoredModules.StoredModules, heutc));
                        break;
                    }

                case JournalTypeEnum.SetUserShipName: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalSetUserShipName;
                        eventstosend.Add(InaraClass.setCommanderShip(je.ShipFD, je.ShipID, heutc, curship: true, username: je.ShipName, userid: je.ShipIdent, starsystemName: he.System.Name, stationName: he.WhereAmI));
                        break;
                    }

                case JournalTypeEnum.Docked:  // VERIFIED 18/5/2018 from historic upload test
                    {
                        if (he.ShipInformation != null)     // PR2754 error - a empty list can end up with he.shipinformation = null if all is hidden.
                        {
                            var je = he.journalEntry as JournalDocked;
                            eventstosend.Add(InaraClass.addCommanderTravelDock(he.ShipInformation.ShipFD, he.ShipInformation.ID, je.StarSystem, je.StationName, je.MarketID, heutc));
                        }
                        break;
                    }

                case JournalTypeEnum.FSDJump: // VERIFIED 18/5/2018
                    {
                        if (he.ShipInformation != null)     // PR2754 error - a empty list can end up with he.shipinformation = null if all is hidden.
                        {
                            var je = he.journalEntry as JournalFSDJump;
                            eventstosend.Add(InaraClass.addCommanderTravelFSDJump(he.ShipInformation.ShipFD, he.ShipInformation.ID, je.StarSystem, je.JumpDist, heutc));
                        }
                        break;
                    }

                case JournalTypeEnum.CarrierJump: // NEW! 26/5/2020
                    {
                        if (he.ShipInformation != null)     // PR2754 error - a empty list can end up with he.shipinformation = null if all is hidden.
                        {
                            var je = he.journalEntry as JournalCarrierJump;
                            eventstosend.Add(InaraClass.addCommanderTravelCarrierJump(he.ShipInformation.ShipFD, he.ShipInformation.ID, je.StarSystem, heutc));
                        }
                        break;
                    }

                case JournalTypeEnum.Location: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalLocation;
                        eventstosend.Add(InaraClass.setCommanderTravelLocation(je.StarSystem, je.Docked ? je.StationName : null, je.Docked ? je.MarketID : null, heutc));
                        break;
                    }

                case JournalTypeEnum.MissionAccepted: // VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalMissionAccepted;
                        eventstosend.Add(InaraClass.addCommanderMission(je, heutc, he.System.Name, he.WhereAmI));
                        break;
                    }
                case JournalTypeEnum.MissionAbandoned:// VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalMissionAbandoned;
                        eventstosend.Add(InaraClass.setCommanderMissionAbandoned(je.MissionId, heutc));
                        break;
                    }
                case JournalTypeEnum.MissionFailed:// VERIFIED 18/5/2018
                    {
                        var je = he.journalEntry as JournalMissionFailed;
                        eventstosend.Add(InaraClass.setCommanderMissionFailed(je.MissionId, heutc));
                        break;
                    }

                case JournalTypeEnum.MissionCompleted: // VERIFIED 18/5/2018 - updated 8/12/24 to send materials or commodity counts as well
                    {
                        var je = he.journalEntry as JournalMissionCompleted;
                        eventstosend.Add(InaraClass.setCommanderMissionCompleted(je));

                        foreach (var mat in je.MaterialsReward.EmptyIfNull())
                        {
                            if (mcmr.TryGetValue(mat.Name, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));

                        }
                        foreach (var commd in je.CommodityReward.EmptyIfNull())
                        {
                            if (mcmr.TryGetValue(commd.Name, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));

                        }
                        break;
                    }

                case JournalTypeEnum.Rank:          //rank before progress, cache
                    {
                        var je = he.journalEntry as JournalRank;
                        ranksave = je;
                        break;
                    }

                case JournalTypeEnum.Progress:      // progress comes after rank in journal logs
                    {
                        if (ranksave != null)
                        {
                            JournalProgress progress = he.journalEntry as JournalProgress;

                            eventstosend.Add(InaraClass.setCommanderRankPilot("combat", (int)ranksave.Combat, progress?.Combat ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("trade", (int)ranksave.Trade, progress?.Trade ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("explore", (int)ranksave.Explore, progress?.Explore ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("soldier", (int)ranksave.Soldier, progress?.Soldier ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("exobiologist", (int)ranksave.ExoBiologist, progress?.ExoBiologist ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("empire", (int)ranksave.Empire, progress?.Empire ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("federation", (int)ranksave.Federation, progress?.Federation ?? -1, ranksave.EventTimeUTC));
                            eventstosend.Add(InaraClass.setCommanderRankPilot("cqc", (int)ranksave.CQC, progress?.CQC ?? -1, ranksave.EventTimeUTC));
                        }

                        break;
                    }

                case JournalTypeEnum.Promotion:     // promotion
                    {
                        var promotion = he.journalEntry as JournalPromotion;
                        if (promotion.Combat != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("combat", (int)promotion.Combat, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.Trade != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("trade", (int)promotion.Trade, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.Explore != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("explore", (int)promotion.Explore, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.ExoBiologist != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("exobiologist", (int)promotion.ExoBiologist, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.Soldier != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("soldier", (int)promotion.Soldier, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.Empire != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("empire", (int)promotion.Empire, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.Federation != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("federation", (int)promotion.Federation, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0
                        if (promotion.CQC != null)
                            eventstosend.Add(InaraClass.setCommanderRankPilot("cqc", (int)promotion.CQC, 0, promotion.EventTimeUTC));     // by definition, since your promoted, progress = 0

                        break;
                    }

                case JournalTypeEnum.Reputation: // VERIFIED 16/5/18
                    {
                        var reputation = he.journalEntry as JournalReputation;
                        eventstosend.Add(InaraClass.setCommanderReputationMajorFaction("federation", reputation.Federation.HasValue ? reputation.Federation.Value : 0, reputation.EventTimeUTC));
                        eventstosend.Add(InaraClass.setCommanderReputationMajorFaction("empire", reputation.Empire.HasValue ? reputation.Empire.Value : 0, reputation.EventTimeUTC));
                        eventstosend.Add(InaraClass.setCommanderReputationMajorFaction("independent", reputation.Independent.HasValue ? reputation.Independent.Value : 0, reputation.EventTimeUTC));
                        eventstosend.Add(InaraClass.setCommanderReputationMajorFaction("alliance", reputation.Alliance.HasValue ? reputation.Alliance.Value : 0, reputation.EventTimeUTC));
                        break;
                    }

                case JournalTypeEnum.Powerplay: // VERIFIED 16/5/18
                    {
                        JournalPowerplay power = he.journalEntry as JournalPowerplay;
                        eventstosend.Add(InaraClass.setCommanderRankPower(power.Power, power.Rank, power.Merits, power.EventTimeUTC));
                        break;
                    }

                case JournalTypeEnum.PowerplayLeave: // New Nov 24
                    {
                        JournalPowerplayLeave power = he.journalEntry as JournalPowerplayLeave;
                        eventstosend.Add(InaraClass.setCommanderRankPower(power.Power, -1, 0, power.EventTimeUTC));
                        break;
                    }

                case JournalTypeEnum.PowerplayDefect: // New Nov 24
                    {
                        JournalPowerplayDefect power = he.journalEntry as JournalPowerplayDefect;
                        eventstosend.Add(InaraClass.setCommanderRankPower(power.FromPower, -1, 0, power.EventTimeUTC));
                        eventstosend.Add(InaraClass.setCommanderRankPower(power.ToPower, 0, 0, power.EventTimeUTC));
                        break;
                    }

                case JournalTypeEnum.EngineerProgress:      //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalEngineerProgress;
                        foreach( var x in je.Engineers )
                        {
                            if (x.Valid)      // Frontier lovely logs again - check for validity
                                eventstosend.Add(InaraClass.setCommanderRankEngineer(x.Engineer, x.Progress, x.Rank, heutc));
                        }
                        break;
                    }

                case JournalTypeEnum.Died: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalDied;
                        string[] killers = je.Killers != null ? je.Killers.Select(x => x.Name).ToArray() : null;
                        eventstosend.Add(InaraClass.addCommanderCombatDeath(he.System.Name, killers, heutc));
                        break;
                    }

                case JournalTypeEnum.Interdicted: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalInterdicted;
                        eventstosend.Add(InaraClass.addCommanderCombatInterdicted(he.System.Name, je.Interdictor, je.IsPlayer, je.Submitted, heutc));
                        break;
                    }
                case JournalTypeEnum.Interdiction: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalInterdiction;
                        eventstosend.Add(InaraClass.addCommanderCombatInterdiction(he.System.Name, je.Interdicted.HasChars() ? je.Interdicted : je.Faction, je.IsPlayer, je.Success, heutc));
                        break;
                    }

                case JournalTypeEnum.EscapeInterdiction: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalEscapeInterdiction;
                        eventstosend.Add(InaraClass.addCommanderCombatInterdictionEscape(he.System.Name, je.Interdictor, je.IsPlayer, heutc));
                        break;
                    }

                case JournalTypeEnum.PVPKill: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalPVPKill;
                        eventstosend.Add(InaraClass.addCommanderCombatKill(he.System.Name, je.Victim, heutc));
                        break;
                    }

                case JournalTypeEnum.CargoDepot: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalCargoDepot;
                        if (je.CargoType.HasChars() && je.Count > 0)
                        {
                            if (mcmr.TryGetValue(je.CargoType, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        break;
                    }
                case JournalTypeEnum.CollectCargo: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalCollectCargo;
                        if (mcmr.TryGetValue(je.Type, out MaterialCommodityMicroResource item))
                        {
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        break;
                    }
                case JournalTypeEnum.EjectCargo: //VERIFIED 16/5/18
                    { 
                        var je = he.journalEntry as JournalEjectCargo;
                        if (mcmr.TryGetValue(je.Type, out MaterialCommodityMicroResource item))
                        {
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        break;
                    }
                case JournalTypeEnum.EngineerContribution: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalEngineerContribution;
                        if (je.Commodity.HasChars())
                        {
                            if (mcmr.TryGetValue(je.Commodity, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        if (je.Material.HasChars())
                        {
                            if (mcmr.TryGetValue(je.Material, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        break;
                    }
                case JournalTypeEnum.MarketBuy: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalMarketBuy;
                        if (mcmr.TryGetValue(je.Type, out MaterialCommodityMicroResource item))
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        break;
                    }
                case JournalTypeEnum.MarketSell: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalMarketSell;
                        if (mcmr.TryGetValue(je.Type, out MaterialCommodityMicroResource item))
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        break;
                    }
                case JournalTypeEnum.Cargo: //VERIFIED 16/5/18 - 8/12/24 lets use this to send MRs as well - shiplocker is too verbose to use as a trigger
                    {
                        var commod = mcmr.Values.Where(x => x.Details.IsCommodity).ToList();
                        if (commod.Count > 0)
                            eventstosend.Add(InaraClass.setCommanderInventory(commod, heutc));

                        var mr = mcmr.Values.Where(x => x.Details.IsMicroResources).ToList();
                        if (mr.Count > 0)
                            eventstosend.Add(InaraClass.setCommanderInventory(mr, heutc, 0, "ShipLocker"));
                        break;
                    }

                case JournalTypeEnum.Materials: //VERIFIED 16/5/18
                    {
                        var mat= mcmr.Values.Where(x => x.Details.IsMaterial).ToList();
                        if (mat.Count > 0)
                            eventstosend.Add(InaraClass.setCommanderInventory(mat, heutc));
                        break;
                    }

                case JournalTypeEnum.MaterialCollected:
                    {
                        var je = he.journalEntry as JournalMaterialCollected;
                        if (mcmr.TryGetValue(je.Name, out MaterialCommodityMicroResource item))
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        break;
                    }
                case JournalTypeEnum.MaterialDiscarded:
                    {
                        var je = he.journalEntry as JournalMaterialDiscarded;
                        if (mcmr.TryGetValue(je.Name, out MaterialCommodityMicroResource item))
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item,heutc));
                        break;
                    }
                case JournalTypeEnum.MiningRefined:
                    {
                        var je = he.journalEntry as JournalMiningRefined;
                        if (mcmr.TryGetValue(je.Type, out MaterialCommodityMicroResource item))
                            eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        break;
                    }

                case JournalTypeEnum.MaterialTrade: // one out, one in.. //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalMaterialTrade;
                        if (je.Paid != null)
                        {
                            if (mcmr.TryGetValue(je.Paid.Material, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }
                        if (je.Received != null)
                        {
                            if (mcmr.TryGetValue(je.Received.Material, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                        }

                        break;
                    }

                case JournalTypeEnum.EngineerCraft: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalEngineerCraft;

                        if (je.Ingredients != null)
                        {
                            foreach (var i in je.Ingredients)
                            {
                                if (mcmr.TryGetValue(i.NameFD, out MaterialCommodityMicroResource item))
                                    eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                            }
                        }
                        break;
                    }
                case JournalTypeEnum.Synthesis: //VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalSynthesis;

                        if (je.Materials != null)
                        {
                            foreach (KeyValuePair<string, int> k in je.Materials)
                            {
                                if (mcmr.TryGetValue(k.Key, out MaterialCommodityMicroResource item))
                                    eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc));
                            }
                        }
                        break;
                    }

                case JournalTypeEnum.Statistics://VERIFIED 16/5/18
                    {
                        JournalStatistics stats = he.journalEntry as JournalStatistics;
                        eventstosend.Add(InaraClass.setCommanderCredits(he.Credits, he.Assets, he.Loan, heutc));
                        eventstosend.Add(InaraClass.setCommanderGameStatistics(stats.GetJsonCloned(), stats.EventTimeUTC));
                        break;
                    }

                case JournalTypeEnum.CommunityGoal://VERIFIED 16/5/18
                    {
                        var je = he.journalEntry as JournalCommunityGoal;
                        foreach (var c in je.CommunityGoals)
                        {
                            eventstosend.Add(InaraClass.setCommunityGoal(c, heutc));
                            eventstosend.Add(InaraClass.setCommandersCommunityGoalProgress(c, heutc));
                        }

                        break;
                    }

                case JournalTypeEnum.Friends:
                    {
                        var je = he.journalEntry as JournalFriends;
                        foreach( var f in je.Statuses())
                        {
                            if ( JournalFriends.IsFriend(f.Status))
                                eventstosend.Add(InaraClass.addCommanderFriend(f.Name, heutc));
                            else if (JournalFriends.IsNotFriend(f.Status))
                                eventstosend.Add(InaraClass.delCommanderFriend(f.Name, heutc));
                        }
                        break;
                    }

                case JournalTypeEnum.Embark:
                case JournalTypeEnum.Disembark:
                    {
                        var mr = mcmr.Values.Where(x => x.Details.IsMicroResources).ToList();
                        if (mr.Count > 0)
                            eventstosend.Add(InaraClass.setCommanderInventory(mr, heutc, 0, "ShipLocker"));
                        break;
                    }

                case JournalTypeEnum.BuyMicroResources:
                    {
                        foreach (var mritem in ((JournalBuyMicroResources)he.journalEntry).Items.EmptyIfNull())
                        {
                            if (mcmr.TryGetValue(mritem.Name, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc, 0, "ShipLocker"));
                        }
                        break;
                    }

                case JournalTypeEnum.SellMicroResources:
                    {
                        foreach (var mritem in ((JournalSellMicroResources)he.journalEntry).Items.EmptyIfNull())
                        {
                            if (mcmr.TryGetValue(mritem.Name, out MaterialCommodityMicroResource item))
                                eventstosend.Add(InaraClass.setCommanderInventoryItem(item, heutc, 0, "ShipLocker"));
                        }
                        break;
                    }

                case JournalTypeEnum.TradeMicroResources:
                    {
                        var mr = mcmr.Values.Where(x => x.Details.IsMicroResources).ToList();
                        if (mr.Count > 0)
                            eventstosend.Add(InaraClass.setCommanderInventory(mr, heutc, 0, "ShipLocker"));      // just send all here
                        break;
                    }

            }


            if ( Math.Abs(CmdrCredits-he.Credits) > 500000 )
            {
                eventstosend.Add(InaraClass.setCommanderCredits(he.Credits, he.Assets, he.Loan, heutc));
                CmdrCredits = he.Credits;
            }

            eventstosend = eventstosend.Where(x => x != null).ToList();     // remove any nulls

            return eventstosend;
        }

#endregion

#region Thread

        public static void Submit(List<JToken> list, Action<string> logger, EDCommander cmdrn, bool verbose)
        {
            string cmdrfidp = cmdrn.FID;

            foreach (var x in list)
                eventqueue.Enqueue(new InaraQueueEntry() { eventinfo = x, cmdr = cmdrn , cmdrfid = cmdrfidp, logger = logger, verbose = verbose});

            queuedevents.Set();

            // Start the sync thread if it's not already running
            if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
            {
                Exit = false;
                exitevent.Reset();
                ThreadInaraSync = new System.Threading.Thread(new System.Threading.ThreadStart(SyncThread));
                ThreadInaraSync.Name = "Inara Journal Sync";
                ThreadInaraSync.IsBackground = true;
                ThreadInaraSync.Start();
            }
        }

        private static void SyncThread()
        {
            try
            {
                running = 1;

                while (eventqueue.Count != 0)      // while stuff to send
                {
                    exitevent.WaitOne(10000);       // wait in case others are being generated

                    if (Exit)
                        break;

                    if (eventqueue.TryDequeue(out InaraQueueEntry firstheq))
                    {
                        List<JToken> tosend = new List<JToken>() { firstheq.eventinfo };

                        int maxpergo = 50;
                        bool verbose = false;

                        // if not too many, and we have another, and the commander is the same 
                        while (tosend.Count < maxpergo && eventqueue.TryPeek(out InaraQueueEntry nextheq) && nextheq.cmdr.Id == firstheq.cmdr.Id)
                        {
                            eventqueue.TryDequeue(out nextheq);     // and remove it
                            tosend.Add(nextheq.eventinfo);
                            verbose |= nextheq.verbose;
                        }

                        InaraClass inara = new InaraClass(firstheq.cmdr);
                        string response = inara.Send(tosend);
                        System.Diagnostics.Debug.WriteLine(response);
                        firstheq?.logger(response);
                    }

                    exitevent.WaitOne(30000);       // space out events well

                    if (Exit)
                        break;

                    if (eventqueue.IsEmpty)
                        queuedevents.WaitOne(120000);       // wait for another event keeping the thread open.. Note stop also sets this

                    if (Exit)
                        break;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception ex:" + ex.Message);
            }
            finally
            {
                running = 0;
            }
        }

#endregion
    }
}

