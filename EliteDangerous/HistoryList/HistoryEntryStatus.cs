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
    [System.Diagnostics.DebuggerDisplay("HES {TravelState} {BodyName} {StationName} ")]
    public class HistoryEntryStatus
    {
        public enum TravelStateType {
            Unknown,

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
            Fighter,            // in fighter

            OnFootStarPort,      
            OnFootPlanetaryPort, 
            OnFootPlanet,
            OnFootFleetCarrier,
        };       

        public string BodyName { get; private set; }
        public int? BodyID { get; private set; }
        public bool HasBodyID { get { return BodyID.HasValue && BodyID.Value >= 0; } }
        public string BodyType { get; private set; }
        public string StationName { get; private set; }     // will be null when undocked
        public string StationType { get; private set; }     // will be null when undocked
        public string StationFaction { get; private set; }  // will be null when undocked
        public long? MarketId { get; private set; } 
        public TravelStateType TravelState { get; private set; } = TravelStateType.Unknown;  // travel state
        public bool OnFoot { get { return TravelState >= TravelStateType.OnFootStarPort; } }
        public bool OnFootFleetCarrier { get { return TravelState == TravelStateType.OnFootFleetCarrier; } }
        public ulong ShipID { get; private set; } = ulong.MaxValue;
        public string ShipType { get; private set; } = "Unknown";         // and the ship nice name
        public string ShipTypeFD { get; private set; } = "Unknown";      // FD name
        public bool IsSRV { get { return ItemData.IsSRV(ShipTypeFD); } }
        public bool IsFighter { get { return ItemData.IsFighter(ShipTypeFD); } }
        public string OnCrewWithCaptain { get; private set; } = null;     // if not null, your in another multiplayer ship
        public string GameMode { get; private set; } = "Unknown";         // game mode, from LoadGame event
        public string Group { get; private set; } = "";                   // group..
        public bool Wanted { get; private set; } = false;
        public bool BodyApproached { get; private set; } = false;         // set at approach body, cleared at leave body or fsd jump
        public bool BookedDropship { get; private set; } = false;         // cleared on embark - need this to tell difference between booking a taxi or a dropship when entering the taxi
        public bool BookedTaxi { get; private set; } = false;             // cleared on embark - need this to tell difference between booking a taxi or a dropship when entering the taxi

        public double CurrentBoost { get; private set; } = 1;             // current boost multiplier due to jet cones and synthesis

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
            BookedTaxi = prevstatus.BookedTaxi;
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
                        bool stationisfc = prev.StationType.HasChars() && prev.StationType.Equals("Fleet Carrier", System.StringComparison.CurrentCultureIgnoreCase);

                        TravelStateType t = (stationisfc && !jloc.Docked) ? TravelStateType.OnFootFleetCarrier : 
                           
                            jloc.Docked ? (jloc.Multicrew == true ? TravelStateType.MulticrewDocked : TravelStateType.Docked) :
                                                (jloc.InSRV == true) ? (jloc.Multicrew == true ? TravelStateType.MulticrewSRV : TravelStateType.SRV) :    
                                                    jloc.Taxi == true ? TravelStateType.TaxiNormalSpace :          // can't be in dropship, must be in normal space.
                                                        jloc.OnFoot == true ? (locinstation ? TravelStateType.OnFootStarPort : TravelStateType.OnFootPlanet) :
                                                            jloc.Latitude.HasValue ? (jloc.Multicrew == true ? TravelStateType.MulticrewLanded : TravelStateType.Landed) :
                                                                jloc.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                                    TravelStateType.NormalSpace;

                        hes = new HistoryEntryStatus(prev)     // Bodyapproach copy over we should be in the same state as last..
                        {
                            TravelState = t,
                            MarketId = jloc.MarketID,
                            BodyID = jloc.BodyID,
                            BodyType = jloc.BodyType,
                            BodyName = jloc.Body,
                            Wanted = jloc.Wanted,
                            StationName = stationisfc ? prev.StationName: (jloc.StationName.Alt(jloc.Docked || locinstation ? jloc.Body : null)),
                            StationType = stationisfc ? prev.StationType : ( jloc.StationType.Alt(prev.StationType).Alt(jloc.Docked || locinstation ? jloc.BodyType : null)),
                            StationFaction = jloc.StationFaction,          // may be null
                            CurrentBoost = 1,
                        };
                        break;
                    }

                case JournalTypeEnum.CarrierJump:       // missing from 4.0 odyssey
                    var jcj = (je as JournalCarrierJump);
                    hes = new HistoryEntryStatus(prev)     // we are docked or on foot on a carrier - travel state stays the same
                    {
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
                            BookedTaxi = false,
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
                            CurrentBoost = 1,
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
                        TravelState = jlg.InSuit ? (prev.TravelState) : 
                                         jlg.InTaxi ? TravelStateType.TaxiNormalSpace :
                                             jlg.InSRV ? TravelStateType.SRV : 
                                                    prev.TravelState != TravelStateType.Unknown ? prev.TravelState :
                                                        TravelStateType.Docked,     
                        ShipType = jlg.InShip ? jlg.Ship : prev.ShipType,
                        ShipID = jlg.InShip ? jlg.ShipId : prev.ShipID,
                        ShipTypeFD = jlg.InShip ? jlg.ShipFD : prev.ShipTypeFD,
                        BookedTaxi = false,
                        BookedDropship = false, //  to ensure
                        CurrentBoost = 1,
                    };
                    break;

                
                case JournalTypeEnum.Docked:        // Docked not seen when in Taxi.
                    {
                        JournalDocked jdocked = (JournalDocked)je;
                        //System.Diagnostics.Debug.WriteLine("{0} Docked {1} {2} {3}", jdocked.EventTimeUTC, jdocked.StationName, jdocked.StationType, jdocked.Faction);
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = jdocked.Taxi == true ? TravelStateType.TaxiDocked :
                                            jdocked.Multicrew == true ? TravelStateType.MulticrewDocked :
                                                TravelStateType.Docked,
                            MarketId = jdocked.MarketID,
                            Wanted = jdocked.Wanted,
                            StationName = jdocked.StationName.Alt("Unknown"),
                            StationType = jdocked.StationType.Alt("Station"),
                            StationFaction = jdocked.Faction,
                            CurrentBoost = 1,
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
                            BookedTaxi = false,
                            BookedDropship = false,
                        };
                        break;
                    }

                case JournalTypeEnum.Disembark:     // SRV/Ship -> on foot
                    var disem = (JournalDisembark)je;

                    bool instation = disem.StationType.HasChars() || prev.StationType.HasChars();
                    bool fc = prev.StationType.HasChars() && prev.StationType.Equals("Fleet Carrier", System.StringComparison.CurrentCultureIgnoreCase);

                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = fc ? TravelStateType.OnFootFleetCarrier : 
                                    disem.SRV ? TravelStateType.OnFootPlanet :
                                        disem.OnStation == true ?  TravelStateType.OnFootStarPort :
                                            disem.OnPlanet == true && instation ? TravelStateType.OnFootPlanetaryPort :
                                            TravelStateType.OnFootPlanet,
                        StationName = disem.StationType.HasChars() ? disem.StationName.Alt("Unknown") : prev.StationName,       // copying it over due to bug in alpha4
                        StationType = disem.StationType.HasChars() ? disem.StationType : prev.StationType,
                        CurrentBoost = 1,
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
                    var td = (JournalTouchdown)je;
                    if (td.PlayerControlled == true)        // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = prev.TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
                            BodyName = td.Body ?? prev.BodyName,
                            BodyID = td.BodyID.HasValue ? td.BodyID.Value :prev.BodyID,
                            CurrentBoost = 1,
                        };
                    }
                    else
                        hes = prev;
                    break;

                case JournalTypeEnum.Liftoff:
                    var loff = (JournalLiftoff)je;
                    if (loff.PlayerControlled == true)         // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(prev)      // not going to use Body, since we must already have it.
                        {
                            TravelState = prev.TravelState == TravelStateType.MulticrewLanded ? TravelStateType.MulticrewNormalSpace : TravelStateType.NormalSpace,
                        };
                    }
                    else
                        hes = prev;
                    break;


                case JournalTypeEnum.FighterDestroyed:
                case JournalTypeEnum.DockFighter:
                    {
                        if ( prev.TravelState == TravelStateType.Fighter)
                        {
                            hes = new HistoryEntryStatus(prev)
                            {
                                TravelState = TravelStateType.NormalSpace
                            };
                        }
                        break;
                    }


                case JournalTypeEnum.LaunchFighter:
                {
                    var j = je as JournalLaunchFighter;
                    if (j.PlayerControlled)
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = TravelStateType.Fighter,
                        };
                    }
                    break;
                }

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
                        BookedTaxi = false,
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
                    {
                        JournalShipyardSwap jsswap = (JournalShipyardSwap)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            ShipID = jsswap.ShipId,
                            ShipType = jsswap.ShipType,
                            ShipTypeFD = jsswap.ShipFD,
                        };
                        break;
                    }

                case JournalTypeEnum.BookDropship:
                    hes = new HistoryEntryStatus(prev)
                    {
                        BookedDropship = true,
                        BookedTaxi = false,
                    };
                    break;

                case JournalTypeEnum.CancelTaxi:
                case JournalTypeEnum.CancelDropship:
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            BookedDropship = false,
                            BookedTaxi = false,
                        };
                        break;
                    }

                case JournalTypeEnum.BookTaxi:
                    hes = new HistoryEntryStatus(prev)
                    {
                        BookedDropship = false,
                        BookedTaxi = true,
                    };
                    break;

                case JournalTypeEnum.JetConeBoost:
                    {
                        JournalJetConeBoost jjcb = (JournalJetConeBoost)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentBoost = jjcb.BoostValue
                        };
                    }
                    break;
                case JournalTypeEnum.Synthesis:
                    {
                        JournalSynthesis jsyn = (JournalSynthesis)je;
                        if (jsyn.FSDBoostValue > 0)     // only if FSD boost
                        {
                            hes = new HistoryEntryStatus(prev)
                            {
                                CurrentBoost = jsyn.FSDBoostValue
                        };
                    }
                    }
                    break;
            }

            return hes;
        }
    }

}