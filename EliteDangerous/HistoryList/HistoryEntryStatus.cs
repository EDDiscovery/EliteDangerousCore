/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public class HistoryEntryStatus
    {
        public enum TravelStateType {
            Docked,         // in ship
            NormalSpace,
            Supercruise,    
            Landed,     

            TaxiDocked,      
            TaxiNormalSpace, 
            TaxiSupercruise,

            DropShipDocked,
            DropShipNormalSpace,    
            DropShipSupercruise,

            MulticrewDocked,        // in multicrew
            MulticrewNormalSpace,
            MulticrewSupercruise,
            MulticrewLanded,
            MulticrewSRV,

            SRV,            // in srv
            OnFootStation,          // on foot
            OnFootPlanet,
            Unknown
        };       

        public string BodyName { get; private set; }
        public int? BodyID { get; private set; }
        public bool HasBodyID { get { return BodyID.HasValue && BodyID.Value >= 0; } }
        public string BodyType { get; private set; }
        public string StationName { get; private set; }
        public string StationType { get; private set; }
        public long? MarketId { get; private set; }
        public TravelStateType TravelState { get; private set; } = TravelStateType.Unknown;  // travel state
        public ulong ShipID { get; private set; } = ulong.MaxValue;
        public string ShipType { get; private set; } = "Unknown";         // and the ship
        public string ShipTypeFD { get; private set; } = "unknown";
        public string OnCrewWithCaptain { get; private set; } = null;     // if not null, your in another multiplayer ship
        public string GameMode { get; private set; } = "Unknown";         // game mode, from LoadGame event
        public string Group { get; private set; } = "";                   // group..
        public bool Wanted { get; private set; } = false;
        public bool BodyApproached { get; private set; } = false;         // set at approach body, cleared at leave body or fsd jump
        public string StationFaction { get; private set; } = "";          // when docked
        public bool BookedDropship { get; private set; } = false;         // cleared on embark - need this to tell difference between booking a taxi or a dropship when entering the taxi

        private HistoryEntryStatus()
        {
        }

        public HistoryEntryStatus(HistoryEntryStatus prevstatus)
        {
            BodyName = prevstatus.BodyName;
            BodyID = prevstatus.BodyID;
            BodyType = prevstatus.BodyType;
            StationName = prevstatus.StationName;
            StationType = prevstatus.StationType;
            MarketId = prevstatus.MarketId;
            TravelState = prevstatus.TravelState;
            ShipID = prevstatus.ShipID;
            ShipType = prevstatus.ShipType;
            ShipTypeFD = prevstatus.ShipTypeFD;
            OnCrewWithCaptain = prevstatus.OnCrewWithCaptain;
            GameMode = prevstatus.GameMode;
            Group = prevstatus.Group;
            Wanted = prevstatus.Wanted;
            BodyApproached = prevstatus.BodyApproached;
            StationFaction = prevstatus.StationFaction;
            BookedDropship = prevstatus.BookedDropship;
        }

        public static HistoryEntryStatus Update(HistoryEntryStatus prev, JournalEntry je, string curStarSystem)
        {
            if (prev == null)
            {
                prev = new HistoryEntryStatus();
            }

            HistoryEntryStatus hes = prev;

            switch (je.EventTypeID)
            {
                case JournalTypeEnum.Location:
                    {
                        JournalLocation jloc = je as JournalLocation;

                        bool locinstation = jloc.StationType.HasChars() || prev.StationType.HasChars();     // second is required due to alpha 4 stationtype being missing

                        TravelStateType t = jloc.Docked ? TravelStateType.Docked :
                                                ((jloc.InSRV == null && jloc.Latitude.HasValue) || jloc.InSRV == true) ? (jloc.Multicrew == true ? TravelStateType.MulticrewSRV : TravelStateType.SRV) :      // lat is pre 4.0 check
                                                    jloc.Taxi == true ? TravelStateType.TaxiNormalSpace :          // can't be in dropship, must be in normal space.
                                                        jloc.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                            jloc.OnFoot == true ? (locinstation ? TravelStateType.OnFootStation : TravelStateType.OnFootPlanet) :
                                                                prev.TravelState == TravelStateType.OnFootPlanet && locinstation ? TravelStateType.OnFootStation :
                                                                    TravelStateType.NormalSpace;

                        hes = new HistoryEntryStatus(prev)     // Bodyapproach copy over we should be in the same state as last..
                        {
                            TravelState = t,
                            MarketId = jloc.MarketID,
                            BodyID = jloc.BodyID,
                            BodyType = jloc.BodyType,
                            BodyName = jloc.Body,
                            Wanted = jloc.Wanted,
                            StationName = jloc.StationName.Alt(null),       // if empty string, set to null
                            StationType = jloc.StationType.Alt(null),
                            StationFaction = jloc.StationFaction,          // may be null
                        };
                        break;
                    }

                case JournalTypeEnum.CarrierJump:
                    var jcj = (je as JournalCarrierJump);
                    hes = new HistoryEntryStatus(prev)     // we are docked on a carrier
                    {
                        TravelState = TravelStateType.Docked,
                        MarketId = jcj.MarketID,
                        BodyID = jcj.BodyID,
                        BodyType = jcj.BodyType,
                        BodyName = jcj.Body,
                        Wanted = jcj.Wanted,
                        StationName = jcj.StationName.Alt(null),       // if empty string, set to null
                        StationType = jcj.StationType.Alt(null),
                    };
                    break;

                case JournalTypeEnum.SupercruiseEntry:
                    {
                        var sc = je as JournalSupercruiseEntry;
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = sc.Taxi == true ? (prev.TravelState == TravelStateType.DropShipNormalSpace ? TravelStateType.DropShipSupercruise : TravelStateType.TaxiSupercruise ):
                                                sc.Multicrew == true ? TravelStateType.MulticrewSupercruise :
                                                            TravelStateType.Supercruise,

                            BodyName = !prev.BodyApproached ? curStarSystem : prev.BodyName,
                            BodyType = !prev.BodyApproached ? "Star" : prev.BodyType,
                            BodyID = !prev.BodyApproached ? -1 : prev.BodyID,
                            BookedDropship = false,
                            StationName = null,
                            StationType = null,
                            StationFaction = null, // to clear
                        };
                        break;
                    }
                case JournalTypeEnum.SupercruiseExit:
                    {
                        JournalSupercruiseExit jsexit = (JournalSupercruiseExit)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = jsexit.Taxi == true ? (prev.TravelState == TravelStateType.DropShipSupercruise ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                                jsexit.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                        TravelStateType.NormalSpace,

                            BodyName = (prev.BodyApproached) ? prev.BodyName : jsexit.Body,
                            BodyType = (prev.BodyApproached) ? prev.BodyType : jsexit.BodyType,
                            BodyID = (prev.BodyApproached) ? prev.BodyID : jsexit.BodyID,
                        };
                        break;
                    }

                case JournalTypeEnum.FSDJump:
                    {
                        var jfsd = (je as JournalFSDJump);
                        hes = new HistoryEntryStatus(prev)
                        {
                            // transition to XSupercruise
                            TravelState = jfsd.Taxi == true ? (prev.TravelState == TravelStateType.DropShipSupercruise || prev.TravelState == TravelStateType.DropShipNormalSpace ? TravelStateType.DropShipSupercruise : TravelStateType.TaxiSupercruise) :
                                            jfsd.Multicrew == true ? TravelStateType.MulticrewSupercruise :
                                                TravelStateType.Supercruise,
                            MarketId = null,
                            BodyID = -1,
                            BodyType = "Star",
                            BodyName = jfsd.StarSystem,
                            Wanted = jfsd.Wanted,
                            StationName = null,
                            StationType = null,
                            StationFaction = null, // to ensure
                            BodyApproached = false,
                        };
                        break;
                    }

                case JournalTypeEnum.LoadGame:
                    JournalLoadGame jlg = je as JournalLoadGame;

                    hes = new HistoryEntryStatus(prev) 
                    {
                        OnCrewWithCaptain = null,    // can't be in a crew at this point
                        GameMode = jlg.GameMode,      // set game mode
                        Group = jlg.Group,            // and group, may be empty
                        TravelState = jlg.InSuit ? (prev.TravelState != TravelStateType.OnFootPlanet && prev.TravelState!=TravelStateType.OnFootStation ? TravelStateType.OnFootStation: prev.TravelState) : 
                                         jlg.InTaxi ? TravelStateType.TaxiNormalSpace :
                                             jlg.InSRV ? TravelStateType.SRV : 
                                                    prev.TravelState != TravelStateType.Unknown ? prev.TravelState :
                                                        TravelStateType.Docked,     
                        ShipType = jlg.InShip ? jlg.Ship : prev.ShipType,
                        ShipID = jlg.InShip ? jlg.ShipId : prev.ShipID,
                        ShipTypeFD = jlg.InShip ? jlg.ShipFD : prev.ShipTypeFD,
                        BookedDropship = false, //  to ensure
                    };
                    break;

                
                case JournalTypeEnum.Docked:        // Docked not seen when in Taxi.
                    {
                        JournalDocked jdocked = (JournalDocked)je;
                        //System.Diagnostics.Debug.WriteLine("{0} Station {1} {2}", jdocked.EventTimeUTC, jdocked.StationName, jdocked.Faction);
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = jdocked.Taxi == true ? TravelStateType.TaxiDocked :
                                            jdocked.Multicrew == true ? TravelStateType.MulticrewDocked :
                                                TravelStateType.Docked,
                            MarketId = jdocked.MarketID,
                            Wanted = jdocked.Wanted,
                            StationName = jdocked.StationName.Alt("Unknown"),
                            StationType = jdocked.StationType,
                            StationFaction = jdocked.Faction,
                        };
                        break;
                    }
                case JournalTypeEnum.Undocked:      // undocked not seen when in taxi
                    {
                        var ju = je as JournalUndocked;
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = ju.Taxi == true ? (prev.TravelState == TravelStateType.DropShipDocked ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                            ju.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                TravelStateType.NormalSpace,
                            MarketId = null,
                            StationName = null,
                            StationType = null,
                            StationFaction = null, // to clear
                        };
                        break;
                    }
                case JournalTypeEnum.Embark:        // foot-> SRV/Ship in multicrew or not.
                    {
                        var em = (JournalEmbark)je;

                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = em.SRV ? (em.Multicrew ? TravelStateType.MulticrewSRV : TravelStateType.SRV) :
                                                em.Taxi ? (prev.BookedDropship ? TravelStateType.DropShipDocked : TravelStateType.TaxiDocked) :
                                                    em.Multicrew ? (prev.TravelState == TravelStateType.OnFootPlanet ? TravelStateType.MulticrewLanded : TravelStateType.MulticrewDocked ):
                                                        prev.TravelState == TravelStateType.OnFootPlanet ? TravelStateType.Landed:
                                                            TravelStateType.Docked,
                            BookedDropship = false,
                            // update others tbd
                        };
                        break;
                    }

                case JournalTypeEnum.Disembark:     // SRV/Ship -> on foot
                    var disem = (JournalDisembark)je;

                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = disem.SRV ? TravelStateType.OnFootPlanet :
                                        disem.Taxi || disem.StationType.HasChars() || prev.StationType.HasChars() ? TravelStateType.OnFootStation :       // taxi's or if it has station name, your at a station
                                            TravelStateType.OnFootPlanet,
                        StationName = disem.StationType.HasChars() ? disem.StationName.Alt("Unknown") : prev.StationName,       // copying it over due to bug in alpha4
                        StationType = disem.StationType.HasChars() ? disem.StationType : prev.StationType,
                    };
                    break;

                case JournalTypeEnum.DropshipDeploy:
                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = TravelStateType.OnFootPlanet,
                    };
                    break;

                case JournalTypeEnum.LaunchSRV:
                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = TravelStateType.SRV
                    };
                    break;


                case JournalTypeEnum.DockSRV:
                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = TravelStateType.Landed
                    };
                    break;
                case JournalTypeEnum.Touchdown:
                    // tbd do something with Body etc
                    var td = (JournalTouchdown)je;
                    if (td.PlayerControlled == true)        // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = prev.TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
                        };
                    }
                    else
                        hes = prev;
                    break;

                case JournalTypeEnum.Liftoff:
                    // tbd do something with Body etc
                    var loff = (JournalLiftoff)je;
                    if (loff.PlayerControlled == true)         // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = prev.TravelState == TravelStateType.MulticrewLanded ? TravelStateType.MulticrewNormalSpace : TravelStateType.NormalSpace,
                        };
                    }
                    else
                        hes = prev;
                    break;

                case JournalTypeEnum.ApproachBody:
                    JournalApproachBody jappbody = (JournalApproachBody)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        BodyApproached = true,
                        BodyType = jappbody.BodyType,
                        BodyName = jappbody.Body,
                        BodyID = jappbody.BodyID,
                    };
                    break;
                case JournalTypeEnum.ApproachSettlement:
                    JournalApproachSettlement jappsettlement = (JournalApproachSettlement)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        BodyApproached = true,
                        BodyType = jappsettlement.BodyType,
                        BodyName = jappsettlement.BodyName,
                        BodyID = jappsettlement.BodyID,
                    };
                    break;
                case JournalTypeEnum.LeaveBody:
                    JournalLeaveBody jlbody = (JournalLeaveBody)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        BodyApproached = false,
                        BodyType = "Star",
                        BodyName = curStarSystem,
                        BodyID = -1,
                    };
                    break;

                case JournalTypeEnum.ShipyardBuy:
                    hes = new HistoryEntryStatus(prev)
                    {
                        ShipID = ulong.MaxValue,
                        ShipType = ((JournalShipyardBuy)je).ShipType  // BUY does not have ship id, but the new entry will that is written later - journals 8.34
                    };
                    break;
                case JournalTypeEnum.JoinACrew:
                    hes = new HistoryEntryStatus(prev)
                    {
                        OnCrewWithCaptain = ((JournalJoinACrew)je).Captain
                    };
                    break;
                case JournalTypeEnum.QuitACrew:
                    hes = new HistoryEntryStatus(prev)
                    {
                        OnCrewWithCaptain = null
                    };
                    break;

                case JournalTypeEnum.Died:
                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = TravelStateType.Unknown,
                        OnCrewWithCaptain = null,
                        BodyApproached = false,     // we have to clear this, we can't tell if we are going back to another place..
                        BookedDropship = false,
                    };
                    break;

                case JournalTypeEnum.Loadout:
                    var jloadout = (JournalLoadout)je;
                    if (ItemData.IsShip(jloadout.ShipFD))     // if ship, make a new entry
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            ShipID = jloadout.ShipId,
                            ShipType = jloadout.Ship,
                            ShipTypeFD = jloadout.ShipFD,
                        };
                    }
                    break;
                case JournalTypeEnum.ShipyardNew:
                    JournalShipyardNew jsnew = (JournalShipyardNew)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        ShipID = jsnew.ShipId,
                        ShipType = jsnew.ShipType,
                        ShipTypeFD = jsnew.ShipFD,
                    };
                    break;

                case JournalTypeEnum.ShipyardSwap:
                    JournalShipyardSwap jsswap = (JournalShipyardSwap)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        ShipID = jsswap.ShipId,
                        ShipType = jsswap.ShipType,
                        ShipTypeFD = jsswap.ShipFD,
                    };
                    break;

                case JournalTypeEnum.BookDropship:
                    hes = new HistoryEntryStatus(prev)
                    {
                        BookedDropship = true,
                    };
                    break;

                case JournalTypeEnum.CancelDropship:
                case JournalTypeEnum.BookTaxi:
                    hes = new HistoryEntryStatus(prev)
                    {
                        BookedDropship = false,
                    };
                    break;

            }

            return hes;
        }
    }

}