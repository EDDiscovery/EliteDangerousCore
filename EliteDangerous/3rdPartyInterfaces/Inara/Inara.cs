/*
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
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace EliteDangerousCore.Inara
{
    public partial class InaraClass : BaseUtils.HttpCom
    {
        // use if you need an API/name pair to get info from Inara.  Not all queries need it
        public bool ValidCredentials { get { return !string.IsNullOrEmpty(commanderName) && !string.IsNullOrEmpty(apiKey); } }
        static public string SoftwareName { get; set; } = "EDDiscovery";

        private string commanderName;
        private string commanderFrontierID;
        private string apiKey;

        private readonly string fromSoftwareVersion;
        static private Dictionary<long, List<JournalScan>> DictEDSMBodies = new Dictionary<long, List<JournalScan>>();

        private string InaraAPI = "inapi/v1/";      // Action end point

        public InaraClass() : base("https://inara.cz/")
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];
            apiKey = EDCommander.Current.InaraAPIKey;
        }

        public InaraClass(EDCommander cmdr) : this()
        {
            commanderName = string.IsNullOrEmpty(EDCommander.Current.InaraName) ? EDCommander.Current.Name : EDCommander.Current.InaraName;

            if (cmdr != null)
            {
                apiKey = cmdr.InaraAPIKey;
                commanderFrontierID = cmdr.FID;
                commanderName = string.IsNullOrEmpty(cmdr.InaraName) ? cmdr.Name : cmdr.InaraName;
            }
        }

        public void LaunchBrowserForSystem(string system)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(URLForSystem(system));
        }

        public string URLForSystem(string system)
        {
            string url = ServerAddress + "elite/starsystem/?search=" + System.Web.HttpUtility.UrlEncode(system);
            return url;
        }

        public void LaunchBrowserForStation(string system, string station)
        {
            string url = ServerAddress + "elite/station/?search=" + System.Web.HttpUtility.UrlEncode(station) + " [" + System.Web.HttpUtility.UrlEncode(system) + "]";
            BaseUtils.BrowserInfo.LaunchBrowser(url);
        }

        #region Send and receive

        public string ToJSONString(List<JToken> events)
        {
            JToken finaljson = Request(events.ToArray());
            return finaljson.ToString(true);
        }

        public string Send(List<JToken> events)
        {
            return Send(events, out List<JObject> datalist);
        }

        public string Send(List<JToken> events, out List<JObject> datalist) // string returned is errors, null if none..
        {
            string ret = "Inara ";
            datalist = new List<JObject>();

            if (!ValidCredentials)
                return ret + "No valid Inara Credentials" + Environment.NewLine;

            string request = ToJSONString(events);

            //File.WriteAllText(@"c:\code\json.txt", request); 
            //System.Diagnostics.Debug.WriteLine(ret + "Send inara " + request);

            var response = RequestPost(request, InaraAPI, contenttype: "application/json");

            if (response.Error)
                return ret + "No Response" + Environment.NewLine;

            try
            {
                JObject resp = JObject.ParseThrowCommaEOL(response.Body);

                JObject header = resp["header"].Object();
                int headerstatus = header["eventStatus"].Int();     // this is the response to the input header - error if user credentials bad etc

                if (headerstatus >= 300 || headerstatus < 200)      // 2xx good
                {
                    return ret + "Rejected Send: " + header["eventStatusText"].Str() + Environment.NewLine;
                }
                else
                {
                    JArray responses = resp["events"].Array();

                    for( int i = 0; i < responses.Count;  i++)      // for each event.. check response..
                    {
                        JObject ro = responses[i].Object();
                        int eventstate = ro["eventStatus"].Int();

                        datalist.Add(ro["eventData"] as JObject);       // may be null!  if no data is returned by Inara.. Only a few data requestes exists

                        if (i >= events.Count)            // inara is giving more event responses than what we asked for!
                            ret += "Too many responses to events requested" + Environment.NewLine;
                        else if (eventstate == 204 && ro["eventStatusText"].Str("").Contains("in-game name"))       // add/del friends can 
                        {
                            string cmdr = events[i]["eventData"]["commanderName"].Str("Unknown");
                            ret += "Inara reports commander " + cmdr + " is not known to it" + Environment.NewLine;
                        }
                        else if (eventstate >= 300 || eventstate < 200)         // 2xx good
                            ret += "Error to request " + (events[i])["eventName"].Str() + " " + events[i].ToString() + " with " + ro["eventStatusText"].Str() + Environment.NewLine;
                    }

                    return ret.Equals("Inara ") ? $"{ret}Data sent successfully ({responses.Count} events)" : ret;
                }
            }
            catch( Exception e)
            {
                return "Inara Exception " + e.ToString() + Environment.NewLine;
            }
        }

        #endregion

        #region Formatters

        static public JToken getCommanderProfile(string name, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["searchName"] = name;
            return Event("getCommanderProfile", dt, eventData);
        }

        static public JToken addCommanderPermit(string starsystem, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            return Event("addCommanderPermit", dt, eventData);
        }

        static public JToken setCommanderCredits(long credits, long assets, long loan, DateTime dt )
        {
            JObject eventData = new JObject();
            eventData["commanderCredits"] = credits;
            eventData["commanderAssets"] = assets;
            eventData["commanderLoan"] = loan;
            return Event("setCommanderCredits", dt, eventData);
        }

        static public JToken setCommanderGameStatistics(JObject jsonfromstats, DateTime dt)
        {
            jsonfromstats.Remove("event");
            jsonfromstats.Remove("timestamp");
            return Event("setCommanderGameStatistics", dt, jsonfromstats);
        }

        static public JToken setCommanderRankEngineer(string name, string progress, int? value, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["engineerName"] = name;
            if ( progress.HasChars() )
                eventData["rankStage"] = progress;
            if ( value.HasValue )
                eventData["rankValue"] = value.Value;
            return Event("setCommanderRankEngineer", dt, eventData);
        }

        static public JToken setCommanderRankPilot(string name, int value, int progress, DateTime dt)   // progress is in journal units (0-100)
        {
            JObject eventData = new JObject();
            eventData["rankName"] = name;
            eventData["rankValue"] = value;
            if (progress != -1)
                eventData["rankProgress"] = progress / 100.0;
            return Event("setCommanderRankPilot", dt, eventData);
        }

        static public JToken setCommanderRankPower(string name, int value, int merits, DateTime dt)  
        {
            JObject eventData = new JObject();
            eventData["powerName"] = name;
            eventData["rankValue"] = value;       // from nov 24, rank here is 0-4, no +1
            eventData["meritsValue"] = merits;      // new from nov 24
            return Event("setCommanderRankPower", dt, eventData);
        }

        static public JToken setCommanderReputationMajorFaction(string name, double value, DateTime dt)     // value is in journal units (0-100)
        {
            JObject eventData = new JObject();
            eventData["majorfactionName"] = name;
            eventData["majorfactionReputation"] = value / 100.0;
            return Event("setCommanderReputationMajorFaction", dt, eventData);
        }

          static public JToken setCommanderInventoryItem(MaterialCommodityMicroResource x, DateTime dt, int cn = 0 , string loc = null)
        {
            JObject eventData = new JObject();
            eventData["itemName"] = x.Details.FDName;
            eventData["itemCount"] = x.Counts[cn];
            eventData["itemType"] = x.Details.Category.ToString();
            if (loc != null)
                eventData["itemLocation"] = loc;
            return Event("setCommanderInventoryItem", dt, eventData);
        }

        // can handle any category: A type of the item as defined by Frontier: 'Encoded', 'Raw', 'Manufactured', 'Item', 'Items', 'Component', 'Components', 'Data', 'Consumable', 'Consumables'. 
        // On top of that you can use a type 'Material' instead of Encoded, Raw and Manufactured and 'Commodity' for the commodities in the ships's cargo hold. 

        static public JToken setCommanderInventory(List<MaterialCommodityMicroResource> mats, DateTime dt, int cn = 0, string loc = null)
        {
            JArray items = new JArray();
            if (mats != null)
            {
                foreach (var x in mats)
                {
                    JObject data = new JObject();
                    data["itemName"] = x.Details.FDName;
                    data["itemCount"] = x.Counts[cn];
                    data["itemType"] = x.Details.Category.ToString();
                    if (loc != null)
                        data["itemLocation"] = loc;
                    items.Add(data);
                }
            }
            return Event("setCommanderInventory", dt, items);
        }


        static public JToken setCommanderStorageModules(IEnumerable<ShipModulesInStore.StoredModule> list, DateTime dt)
        {
            JObject eventData = new JObject();
            JArray items = new JArray();
            foreach (var x in list)
            {
                JObject data = new JObject();
                data["itemName"] = x.NameFD;
                data["itemValue"] = x.BuyPrice;
                data["isHot"] = x.Hot;
                data["starsystemName"] = x.StarSystem;
                data["marketID"] = x.MarketID;
                if (!x.EngineerModifications.IsEmpty())
                {
                    JObject eng = new JObject();
                    eng["blueprintName"] = x.EngineerModifications;
                    eng["blueprintLevel"] = x.Level;
                    eng["blueprintQuality"] = x.Quality;
                    eng["experimentalEffect"] = "";
                    data["engineering"] = eng;
                }

                items.Add(data);
            }
            return Event("setCommanderStorageModules", dt, items);
        }

        static public JToken addCommanderShip(string fdname, ulong id, string starsystem, string station, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            eventData["starsystemName"] = starsystem;
            eventData["stationName"] = station;
            return Event("addCommanderShip", dt, eventData);
        }

        static public JToken delCommanderShip(string fdname, ulong id, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            return Event("delCommanderShip", dt, eventData);
        }

        static public JToken setCommanderShip(string fdname, ulong id, DateTime dt,
                                              string username = null, string userid = null, bool? curship = null,
                                              bool? ishot = null,
                                              long? shipHullValue = null, long? shipModulesValue = null, long? shipRebuyCost = null,
                                              string starsystemName = null, string stationName = null, long? MarketID = null)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            if ( username.HasChars())
                eventData["shipName"] = username;
            if ( userid.HasChars())
                eventData["shipIdent"] = userid;
            if ( curship != null)
                eventData["isCurrentShip"] = curship.Value;
            if ( ishot.HasValue )
                eventData["isHot"] = ishot.Value;
            if (shipHullValue.HasValue && shipHullValue.Value > 0)
                eventData["shipHullValue"] = shipHullValue.Value;
            if (shipModulesValue.HasValue && shipModulesValue.Value > 0)
                eventData["shipModulesValue"] = shipModulesValue.Value;
            if (shipRebuyCost.HasValue && shipRebuyCost.Value > 0)
                eventData["shipRebuyCost"] = shipRebuyCost.Value;
            if (starsystemName.HasChars())
                eventData["starsystemName"] = starsystemName;
            if (stationName.HasChars())
                eventData["stationName"] = stationName;
            if (MarketID != null)
                eventData["marketID"] = MarketID;

            return Event("setCommanderShip", dt, eventData);
        }

        static public JToken setCommanderShipLoadout(string fdname, ulong id, IEnumerable<ShipModule> list, DateTime dt)
        {
            if (list.Count() == 0)      // no loadout, nothing to send..
                return null;

            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;

            JArray items = new JArray();
            foreach (var x in list)
            {
                JObject data = new JObject();
                data["slotName"] = x.SlotFD.ToString();
                data["itemName"] = x.ItemFD;
                if ( x.Value.HasValue )
                    data["itemValue"] = x.Value.Value;
                if ( x.Health.HasValue)
                    data["itemHealth"] = x.Health.Value;
                if (x.Enabled.HasValue)
                    data["isOn"] = x.Enabled.Value;
                if (x.Priority.HasValue)
                    data["itemPriority"] = x.Priority.Value;
                if (x.AmmoClip.HasValue)
                    data["itemAmmoClip"] = x.AmmoClip.Value;
                if (x.AmmoHopper.HasValue)
                    data["itemAmmoHopper"] = x.AmmoHopper.Value;
                if (x.Engineering != null )
                {
                    JObject eng = new JObject();
                    eng["blueprintName"] = x.Engineering.BlueprintName ?? "";
                    eng["blueprintLevel"] = x.Engineering.Level;
                    eng["blueprintQuality"] = x.Engineering.Quality;
                    eng["experimentalEffect"] = x.Engineering.ExperimentalEffect??"";

                    if ( x.Engineering.Modifiers != null )
                    {
                        JArray mods = new JArray();

                        foreach( var y in x.Engineering.Modifiers )
                        {
                            JObject mod = new JObject();
                            mod["name"] = y.Label;
                            if (y.ValueStr.HasChars())
                                mod["value"] = y.ValueStr;
                            else
                            {
                                mod["value"] = y.Value;
                                mod["originalValue"] = y.OriginalValue;
                                mod["lessIsGood"] = y.LessIsGood;
                            }

                            mods.Add(mod);
                        }

                        eng["modifiers"] = mods;
                    }

                    data["engineering"] = eng;
                }

                items.Add(data);
            }

            eventData["shipLoadout"] = items;
            return Event("setCommanderShipLoadout", dt, eventData);
        }

        static public JToken setCommanderShipTransfer(string fdname, ulong id, string starsystem, string station, long? marketid, int transfertimesec, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            eventData["starsystemName"] = starsystem;
            eventData["stationName"] = station;
            if (marketid != null)
                eventData["marketID"] = marketid;
            if (transfertimesec > 0)
                eventData["transferTime"] = transfertimesec;
            return Event("setCommanderShipTransfer", dt, eventData);
        }

        static public JToken addCommanderTravelDock(string fdname, ulong id, string starsystem, string station, long ? marketid, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            eventData["starsystemName"] = starsystem;
            eventData["stationName"] = station;
            if (marketid != null)
                eventData["marketID"] = marketid;
            return Event("addCommanderTravelDock", dt, eventData);
        }

        static public JToken addCommanderTravelFSDJump(string fdname, ulong id, string starsystem, double distance, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            eventData["starsystemName"] = starsystem;
            eventData["jumpDistance"] = distance;
            return Event("addCommanderTravelFSDJump", dt, eventData);
        }

        static public JToken addCommanderTravelCarrierJump(string fdname, ulong id, string starsystem, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["shipType"] = fdname;
            eventData["shipGameID"] = id;
            eventData["starsystemName"] = starsystem;
            return Event("addCommanderTravelCarrierJump", dt, eventData);
        }

        static public JToken setCommanderTravelLocation(string starsystem, string station, long? marketid, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            if (station.HasChars())
                eventData["stationName"] = station;
            if (marketid != null)
                eventData["marketID"] = marketid;
            return Event("setCommanderTravelLocation", dt, eventData);
        }

        static public JToken addCommanderMission(JournalMissionAccepted mission, DateTime dt, string starsystem, string station)
        {
            JObject eventData = new JObject();
            eventData["missionName"] = mission.FDName;
            eventData["missionGameID"] = mission.MissionId;
            eventData["missionExpiry"] = mission.Expiry.ToStringZuluInvariant();

            if (mission.Influence.HasChars())
                eventData["influenceGain"] = mission.Influence;

            if (mission.Reputation.HasChars())
                eventData["reputationGain"] = mission.Reputation;

            eventData["starsystemNameOrigin"] = starsystem;
            eventData["stationNameOrigin"] = station;

            eventData["minorfactionNameOrigin"] = mission.Faction;

            if (mission.DestinationSystem.HasChars())
                eventData["starsystemNameTarget"] = mission.DestinationSystem;
            if (mission.DestinationStation.HasChars())
                eventData["stationNameTarget"] = mission.DestinationStation;
            if (mission.DestinationSettlement.HasChars())
                eventData["stationNameTarget"] = mission.DestinationSettlement;

            if (mission.TargetFaction.HasChars())
                eventData["minorfactionNameTarget"] = mission.TargetFaction;

            if (mission.Commodity.HasChars())
                eventData["commodityName"] = mission.Commodity;
            if ( mission.Count != null)
                eventData["commodityCount"] = mission.Count.Value;

            if (mission.Target.HasChars())
                eventData["targetName"] = mission.Target;
            if (mission.TargetType.HasChars())
                eventData["targetType"] = mission.TargetType;
            if (mission.KillCount != null)
                eventData["killCount"] = mission.KillCount;

            if (mission.PassengerType.HasChars())
                eventData["passengerType"] = mission.PassengerType;     // fdname
            if (mission.PassengerCount != null)
                eventData["passengerCount"] = mission.PassengerCount.Value;
            if (mission.PassengerVIPs != null)
                eventData["passengerIsVIP"] = mission.PassengerVIPs.Value;
            if (mission.PassengerWanted != null)
                eventData["passengerIsWanted"] = mission.PassengerWanted.Value;

            return Event("addCommanderMission", dt, eventData);
        }

        static public JToken setCommanderMissionAbandoned(ulong id, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["missionGameID"] = id;
            return Event("setCommanderMissionAbandoned", dt, eventData);
        }

        static public JToken setCommanderMissionCompleted(JournalMissionCompleted mission)
        {
            JObject eventData = new JObject();
            eventData["missionGameID"] = mission.MissionId;

            if (mission.Donation != null)
                eventData["donationCredits"] = mission.Donation.Value;
            if (mission.Reward != null)
                eventData["rewardCredits"] = mission.Reward.Value;

            if (mission.PermitsAwarded != null && mission.PermitsAwarded.Length > 0)
            {
                JArray ent = new JArray();
                foreach (var p in mission.PermitsAwarded)
                {
                    JObject o = new JObject();
                    o["starSystemName"] = p;
                    ent.Add(o);
                }

                eventData["rewardPermits"] = ent;
            }
            if (mission.CommodityReward != null && mission.CommodityReward.Length > 0)
            {
                JArray ent = new JArray();
                foreach (var p in mission.CommodityReward)
                {
                    JObject o = new JObject();
                    o["itemName"] = p.Name;
                    o["itemCount"] = p.Count;
                    ent.Add(o);
                }

                eventData["rewardCommodities"] = ent;
            }
            if (mission.MaterialsReward != null && mission.MaterialsReward.Length > 0)
            {
                JArray ent = new JArray();
                foreach (var p in mission.MaterialsReward)
                {
                    JObject o = new JObject();
                    o["itemName"] = p.Name;
                    o["itemCount"] = p.Count;
                    ent.Add(o);
                }

                eventData["rewardMaterials"] = ent;
            }

            if (mission.FactionEffects != null && mission.FactionEffects.Length > 0)
            {
                JArray ent = new JArray();
                foreach (var p in mission.FactionEffects)
                {
                    JObject o = new JObject();
                    o["minorfactionName"] = p.Faction;
                    o["influenceGain"] = (p.Influence != null && p.Influence.Length>0) ? p.Influence[0].Influence : "";
                    o["reputationGain"] = p.Reputation;
                    ent.Add(o);
                }

                eventData["minorfactionEffects"] = ent;
            }

            return Event("setCommanderMissionCompleted", mission.EventTimeUTC, eventData);
        }

        static public JToken setCommanderMissionFailed(ulong id, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["missionGameID"] = id;
            return Event("setCommanderMissionFailed", dt, eventData);
        }

        static public JToken addCommanderCombatDeath(string starsystem, string[] killers, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            if ( killers != null )
            {
                if (killers.Length == 1)
                    eventData["opponentName"] = killers[0];
                else
                {
                    JArray ent = new JArray();
                    foreach (var p in killers)
                        ent.Add(p);

                    eventData["wingOpponentNames"] = ent;
                }
            }

            return Event("addCommanderCombatDeath", dt, eventData);
        }

        static public JToken addCommanderCombatInterdicted(string starsystem, string opponent, bool isplayer, bool issubmit, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            eventData["opponentName"] = opponent;
            eventData["isPlayer"] = isplayer;
            eventData["isSubmit"] = issubmit;
            return Event("addCommanderCombatInterdicted", dt, eventData);
        }

        static public JToken addCommanderCombatInterdiction(string starsystem, string opponent, bool isplayer, bool issuccess, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            eventData["opponentName"] = opponent;
            eventData["isPlayer"] = isplayer;
            eventData["isSuccess"] = issuccess;
            return Event("addCommanderCombatInterdiction", dt, eventData);
        }

        static public JToken addCommanderCombatInterdictionEscape(string starsystem, string opponent, bool isplayer, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            eventData["opponentName"] = opponent;
            eventData["isPlayer"] = isplayer;
            return Event("addCommanderCombatInterdictionEscape", dt, eventData);
        }

        static public JToken addCommanderCombatKill(string starsystem, string opponent, DateTime dt)
        {
            JObject eventData = new JObject();
            eventData["starsystemName"] = starsystem;
            eventData["opponentName"] = opponent;
            return Event("addCommanderCombatKill", dt, eventData);
        }

        static public JToken setCommunityGoal(JournalCommunityGoal.CommunityGoal goals, DateTime dt)
        {
            JObject eventData = new JObject();

            eventData["communitygoalGameID"] = goals.CGID;
            eventData["communitygoalName"] = goals.Title;
            eventData["starsystemName"] = goals.SystemName;
            eventData["stationName"] = goals.MarketName;
            eventData["goalExpiry"] = goals.Expiry.ToStringZuluInvariant();
            if (goals.TierReachedInt.HasValue)
                eventData["tierReached"] = goals.TierReachedInt.Value;
            if (goals.TopTierInt.HasValue)
                eventData["tierMax"] = goals.TopTierInt;
            if (goals.TopRankSize.HasValue)
                eventData["topRankSize"] = goals.TopRankSize.Value;
            eventData["isCompleted"] = goals.IsComplete;
            eventData["contributorsNum"] = goals.NumContributors;
            eventData["contributionsTotal"] = goals.CurrentTotal;
            if (goals.TopTierBonus.HasChars())
                eventData["completionBonus"] = goals.TopTierBonus;

            return Event("setCommunityGoal", dt, eventData);
        }

        static public JToken setCommandersCommunityGoalProgress(JournalCommunityGoal.CommunityGoal goals, DateTime dt)
        {
            if (goals.PlayerContribution > 0)       // no point if none
            {
                JObject eventData = new JObject();

                eventData["communitygoalGameID"] = goals.CGID;
                eventData["contribution"] = goals.PlayerContribution;
                eventData["percentileBand"] = goals.PlayerPercentileBand;

                if ( goals.Bonus.HasValue ) // may not be there
                    eventData["percentileBandReward"] = goals.Bonus.Value;

                if (goals.PlayerInTopRank.HasValue && goals.PlayerInTopRank == true)
                    eventData["isTopRank"] = true;

                return Event("setCommanderCommunityGoalProgress", dt, eventData);
            }
            else
                return null;

        }

        static public JToken addCommanderFriend(string commanderName, DateTime dt, string gamePlatform = "pc")
        {
            JObject eventData = new JObject();
            eventData["commanderName"] = commanderName;
            eventData["gamePlatform"] = gamePlatform;
            return Event("addCommanderFriend", dt, eventData);
        }

        static public JToken delCommanderFriend(string commanderName, DateTime dt, string gamePlatform = "pc")
        {
            JObject eventData = new JObject();
            eventData["commanderName"] = commanderName;
            eventData["gamePlatform"] = gamePlatform;
            return Event("delCommanderFriend", dt, eventData);
        }



        #endregion

        #region Helpers for Format

        static private JToken Event(string eventname, JToken eventData)       // an event
        {
            JObject jo = new JObject();
            jo["eventName"] = eventname;
            jo["eventTimestamp"] = DateTime.UtcNow.ToStringZuluInvariant();
            jo["eventData"] = eventData;
            return jo;
        }

        static private JToken Event(string eventname, DateTime utc, JToken eventData)       // an event
        {
            JObject jo = new JObject();
            jo["eventName"] = eventname;
            jo["eventTimestamp"] = utc;
            jo["eventData"] = eventData;
            return jo;
        }

        private JToken Header()         // Inara header
        {
            JObject jo = new JObject();
            jo["appName"] = SoftwareName;
            jo["appVersion"] = fromSoftwareVersion;
            jo["isDeveloped"] = false;
            jo["APIkey"] = apiKey;
            jo["commanderName"] = commanderName;
            if (commanderFrontierID != null)
                jo["commanderFrontierID"] = commanderFrontierID;
            return jo;
        }

        static private JArray EventArray(params JToken[] events)           // group events.  
        {
            JArray earray = new JArray();
            foreach (JToken t in events)
                earray.Add(t);
            return earray;
        }

        private JToken Request(JToken singleevent)              // request for a single event
        {
            JObject jouter = new JObject();
            jouter["header"] = Header();
            JArray ja = new JArray();
            ja.Add(singleevent);
            jouter["events"] = ja;
            return jouter;
        }

        private JToken Request(JToken[] events)                 // request for multi events
        {
            JObject jouter = new JObject();
            jouter["header"] = Header();
            jouter["events"] = EventArray(events);
            return jouter;
        }

        #endregion


    }
}
