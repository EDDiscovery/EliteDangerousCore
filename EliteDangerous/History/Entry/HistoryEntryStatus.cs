/*
 * Copyright 2016 - 2025 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("HES {TravelState} `{CurrentLocation?.StarSystem}` : `{CurrentLocation?.BodyName}` : `{CurrentLocation?.Name}` ")]
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
            MulticrewFighter,       // see below for gettings relying on order

            SRV,                // in srv
            Fighter,            // in fighter

            OnFootStarPort,      
            OnFootPlanetaryPort, 
            OnFootPlanet,
            OnFootFleetCarrier,
        };

        public bool OnFoot { get { return TravelState >= TravelStateType.OnFootStarPort; } }
        public bool OnFootFleetCarrier { get { return TravelState == TravelStateType.OnFootFleetCarrier; } }

        public bool IsDocked { get { return TravelState == TravelStateType.Docked || TravelState == TravelStateType.MulticrewDocked || 
                                            TravelState == TravelStateType.TaxiDocked || TravelState == TravelStateType.DropShipDocked; } }

        public bool IsInSupercruise { get { return TravelState == TravelStateType.Supercruise || TravelState == TravelStateType.TaxiSupercruise || 
                                            TravelState == TravelStateType.MulticrewSupercruise || TravelState == TravelStateType.DropShipSupercruise; } }

        public bool IsNormalSpace { get { return TravelState == TravelStateType.NormalSpace|| TravelState == TravelStateType.TaxiNormalSpace || TravelState == TravelStateType.MulticrewNormalSpace || TravelState == TravelStateType.DropShipNormalSpace; } }
        public bool IsLanded { get { return TravelState == TravelStateType.Landed || TravelState == TravelStateType.MulticrewLanded; } }
        public bool IsSRV { get { return TravelState == TravelStateType.SRV || TravelState == TravelStateType.MulticrewSRV; } }
        public bool IsFighter { get { return TravelState == TravelStateType.Fighter || TravelState == TravelStateType.MulticrewFighter; } }

        public bool IsLandedInShipOrSRV { get { return TravelState == TravelStateType.Landed || TravelState == TravelStateType.SRV || 
                    TravelState == TravelStateType.MulticrewLanded || TravelState == TravelStateType.MulticrewSRV; }}

        // true for OnCrewWithCaptain entries (they seem  to be marked with Multicrew) and true if entries are in physical multicrew
        public bool IsInMultiCrew { get { return TravelState >= TravelStateType.MulticrewDocked && TravelState <= TravelStateType.MulticrewFighter; } }     

        public TravelStateType TravelState { get; private set; } = TravelStateType.Unknown;  // travel state

        // last jump into the system, FSDJump or Carrier, or a Location which is in space.  Holds star system faction info
        public JournalLocOrJump SystemInfo { get; private set; }                // MAY BE NULL

        // the current location
        public IBodyFeature CurrentLocation { get; private set; }               // MAY BE NULL
        public string WhereAmI => CurrentLocation?.Name_Localised ?? CurrentLocation?.BodyName ?? CurrentLocation?.StarSystem ?? "Unknown";
        public double? Latitude => CurrentLocation?.Latitude;
        public double? Longitude => CurrentLocation?.Longitude;
        public string BodyName=> CurrentLocation?.BodyName;
        public BodyDefinitions.BodyType BodyType => CurrentLocation?.BodyType ?? BodyDefinitions.BodyType.Unknown;
        public int? BodyID => CurrentLocation?.BodyID;
        public bool HasBodyID { get { return BodyID >= 0; } }
        public string StationName_Localised => CurrentLocation?.Name_Localised;
        public StationDefinitions.StarportTypes? FDStationType => CurrentLocation?.FDStationType;
        public long? MarketID => CurrentLocation?.MarketID;
        public string StationFaction => CurrentLocation?.StationFaction;

        // always a ship, never a SRV or fighter
        public IShipNaming CurrentShip { get; private set; }            // MAY BE NULL
        public ulong ShipID => CurrentShip?.ShipId ?? ulong.MaxValue;
        public string ShipType => CurrentShip?.ShipType ?? "Unknown";
        public string ShipTypeFD => CurrentShip?.ShipFD ?? "Unknown";

        public bool Wanted { get; private set; } = false;       // Set and kept on jumping into a new system
        
        public string OnCrewWithCaptain { get; private set; } = null;     // if not null, your in another multiplayer ship, set by JournalJoinACrew
        public bool IsOnCrewWithCaptain { get { return OnCrewWithCaptain != null; } }
        public bool IsInMultiPlayer { get { return IsOnCrewWithCaptain || IsInMultiCrew; } }       // we can be OnCrewWithCaptain with multicrew markers true, or just in physical multicrew 

        // Records the last load game with game info present
        public JournalLoadGame LastLoadGame { get; private set; }       // MAY BE NULL
        public string GameMode => LastLoadGame?.GameMode ?? "Unknown";
        public string Group => LastLoadGame?.Group ?? "";
        public string GameModeGroup { get { return GameMode + (Group.HasChars() ? (":" + Group) : ""); } }
        public string GameModeGroupMulticrew { get { return GameMode + (Group.HasChars() ? (":" + Group) : "") + (OnCrewWithCaptain.HasChars() ? " @ Cmdr " + OnCrewWithCaptain : ""); } }

        // Records the last docking granted message, non null between this and docked/timeout/etc
        public JournalDockingGranted LastDockingGranted { get; private set; }  // MAY BE NULL
        public int DockingPad => LastDockingGranted?.LandingPad ?? 0;
        public StationDefinitions.StarportTypes DockingStationType => LastDockingGranted?.FDStationType ?? StationDefinitions.StarportTypes.Unknown;    // set at Docking granted, cleared at docked, fsd etc
        public bool IsDockingStationTypeCoriolisEtc
        {
            get
            {
                return      // a normal landing pad config
                DockingStationType == StationDefinitions.StarportTypes.Orbis ||
                    DockingStationType == StationDefinitions.StarportTypes.Coriolis || DockingStationType == StationDefinitions.StarportTypes.Bernal ||
                    DockingStationType == StationDefinitions.StarportTypes.Ocellus || DockingStationType == StationDefinitions.StarportTypes.AsteroidBase;
            }
        }
        public bool IsDockingStationTypeCarrier { get { return DockingStationType == StationDefinitions.StarportTypes.FleetCarrier; } }

        // non null when in jump sequence
        public JournalStartJump JumpSequence { get; private set; }  // MAY BE NULL
        public string FSDJumpNextSystemName => JumpSequence?.StarSystem;
        public long? FSDJumpNextSystemAddress => JumpSequence?.SystemAddress;
        public bool FSDJumpSequence => JumpSequence != null;    // true from startjump until location/fsdjump

        // non null when in taxi or dropship, up to embark
        public ITaxiDropship LastTaxiDropship { get; private set; } // MAY BE NULL
        public bool BookedDropship => LastTaxiDropship is JournalBookDropship;
        public bool BookedTaxi => LastTaxiDropship is JournalBookTaxi;

        // Non null when approached a body
        public JournalApproachBody LastApproachBody { get; private set; }   // MAY BE NULL     
        public bool BodyApproached => LastApproachBody != null;    

        public double CurrentBoost { get; private set; } = 1;             // current boost multiplier due to jet cones and synthesis


        private HistoryEntryStatus()
        {
        }

        public HistoryEntryStatus(HistoryEntryStatus prevstatus)
        {
            TravelState = prevstatus.TravelState;
            SystemInfo = prevstatus.SystemInfo;
            CurrentLocation = prevstatus.CurrentLocation;
            CurrentShip = prevstatus.CurrentShip;
            LastLoadGame = prevstatus.LastLoadGame;
            LastDockingGranted = prevstatus.LastDockingGranted;
            JumpSequence = prevstatus.JumpSequence;
            LastTaxiDropship = prevstatus.LastTaxiDropship;
            LastApproachBody = prevstatus.LastApproachBody;

            OnCrewWithCaptain = prevstatus.OnCrewWithCaptain;
            Wanted = prevstatus.Wanted;
            CurrentBoost = prevstatus.CurrentBoost;
        }

        public static HistoryEntryStatus Update(HistoryEntryStatus prev, JournalEntry je, ISystem sys)
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

                        // Location comes out on a Fleetcarrier when on foot with docked=false, on foot= true, detect it and reject using this location, keep with old

                        bool weareonafconfoot = prev.FDStationType == StationDefinitions.StarportTypes.FleetCarrier && jloc.Docked == false && jloc.OnFoot == true;

                        if (!weareonafconfoot)
                        {
                            bool locinstation = jloc.FDStationType != StationDefinitions.StarportTypes.Unknown;     

                            TravelStateType t =
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
                                SystemInfo = !locinstation ? jloc : prev.SystemInfo,        // update this is we are not in station, therefore we will have system info (TBC)
                                CurrentLocation = jloc,
                                Wanted = jloc.Wanted,
                                CurrentBoost = 1,
                                JumpSequence = null,
                                LastDockingGranted = null,
                            };
                        }

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
                            SystemInfo = jfsd,
                            CurrentLocation = jfsd,
                            Wanted = jfsd.Wanted,
                            LastApproachBody = null,
                            LastDockingGranted = null,
                            CurrentBoost = 1,
                            JumpSequence = null,
                        };
                        break;
                    }

                case JournalTypeEnum.CarrierJump:       // missing from 4.0 odyssey
                    var jcj = (je as JournalCarrierJump);
                    hes = new HistoryEntryStatus(prev)     // we are docked or on foot on a carrier - travel state stays the same
                    {
                        SystemInfo = jcj,
                        CurrentLocation = jcj,
                        Wanted = jcj.Wanted,
                        LastDockingGranted = null,
                    };
                    break;

                case JournalTypeEnum.Docked:        // Docked not seen when in Taxi.
                    {
                        JournalDocked jdocked = (JournalDocked)je;
                        //System.Diagnostics.Debug.WriteLine("{0} Docked {1} {2} {3}", jdocked.EventTimeUTC, jdocked.StationName, jdocked.StationType, jdocked.Faction);

                        if ( prev.HasBodyID && prev.BodyName != null)        // Augment the docked with any body info we have got
                        {
                            jdocked.BodyID = prev.BodyID;
                            jdocked.BodyName = prev.BodyName;
                            jdocked.BodyType = prev.BodyType;
                            jdocked.Latitude = prev.Latitude;
                            jdocked.Longitude = prev.Longitude;
                        }

                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = jdocked.Taxi == true ? TravelStateType.TaxiDocked :
                                            jdocked.Multicrew == true ? TravelStateType.MulticrewDocked :
                                                TravelStateType.Docked,
                            CurrentLocation = jdocked,

                            Wanted = jdocked.Wanted,
                            CurrentBoost = 1,
                            LastDockingGranted = null,
                        };

                        break;
                    }

                case JournalTypeEnum.SupercruiseEntry:
                    {
                        var sc = je as JournalSupercruiseEntry;
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = sc.Taxi == true ? (prev.TravelState == TravelStateType.DropShipNormalSpace ? TravelStateType.DropShipSupercruise : TravelStateType.TaxiSupercruise) :
                                                sc.Multicrew == true ? TravelStateType.MulticrewSupercruise :
                                                            TravelStateType.Supercruise,
                            CurrentLocation = new JournalLocation(sc.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              sc.StarSystem,
                                                              BodyDefinitions.BodyType.Star,
                                                              null),

                            LastTaxiDropship = null,
                            LastDockingGranted = null,

                        };
                        break;
                    }

                case JournalTypeEnum.StartJump:
                    {
                        var jsj = (je as JournalStartJump);
                        if (jsj.IsHyperspace)
                        {
                            hes = new HistoryEntryStatus(prev)
                            {
                                JumpSequence = jsj,
                            };
                        }
                        break;
                    }

                case JournalTypeEnum.LoadGame:
                    JournalLoadGame jlg = je as JournalLoadGame;

                    hes = new HistoryEntryStatus(prev)
                    {
                        LastLoadGame = jlg,
                        OnCrewWithCaptain = null,    // can't be in a crew at this point

                        TravelState = jlg.InSuit ? (prev.TravelState) :
                                         jlg.InTaxi ? TravelStateType.TaxiNormalSpace :
                                             jlg.InSRV ? TravelStateType.SRV :
                                                    prev.TravelState != TravelStateType.Unknown ? prev.TravelState :
                                                        TravelStateType.Docked,
                        CurrentShip = jlg.InShip ? jlg : prev.CurrentShip,
                        LastTaxiDropship = null,
                        CurrentBoost = 1,
                        LastDockingGranted = null,
                    };
                    break;


                case JournalTypeEnum.Undocked:      // undocked not seen when in taxi
                    {
                        var ju = je as JournalUndocked;
                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = ju.Taxi == true ? (prev.TravelState == TravelStateType.DropShipDocked ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                            ju.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                TravelStateType.NormalSpace,
                            CurrentLocation = new JournalLocation(ju.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                                prev.CurrentLocation?.BodyName,
                                                                prev.CurrentLocation?.BodyType ?? BodyDefinitions.BodyType.Unknown,  
                                                                prev.CurrentLocation?.BodyID,
                                                                prev.CurrentLocation?.Name, 
                                                                prev.CurrentLocation?.Name_Localised, 
                                                                prev.CurrentLocation?.FDStationType ?? StationDefinitions.StarportTypes.Unknown
                                                               ),
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
                                                    em.Multicrew ? (prev.TravelState == TravelStateType.OnFootPlanet ? TravelStateType.MulticrewLanded : TravelStateType.MulticrewDocked) :
                                                        prev.TravelState == TravelStateType.OnFootPlanet ? TravelStateType.Landed :
                                                            TravelStateType.Docked,
                            LastTaxiDropship = null,
                        };
                        break;
                    }

                case JournalTypeEnum.Disembark:     // SRV/Ship -> on foot
                    var disem = (JournalDisembark)je;

                    bool instation = disem.HasStationTypeName || prev.FDStationType != StationDefinitions.StarportTypes.Unknown;       // due to bug, stationname/type is now always given
                    bool fc = prev.FDStationType == StationDefinitions.StarportTypes.FleetCarrier;

                    //System.Diagnostics.Debug.WriteLine($"Disembark JID {disem.Id} instation {instation} fc {fc} d.Onstation {disem.OnStation} d.Onplanet {disem.OnPlanet} d.SN `{disem.StationName}` d.ST `{disem.StationType}`");

                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = fc ? TravelStateType.OnFootFleetCarrier :
                                    disem.SRV ? TravelStateType.OnFootPlanet :
                                        disem.OnStation == true ? TravelStateType.OnFootStarPort :
                                            disem.OnPlanet == true && instation ? TravelStateType.OnFootPlanetaryPort :
                                            TravelStateType.OnFootPlanet,
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
                        TravelState = prev.TravelState == TravelStateType.MulticrewLanded ? TravelStateType.MulticrewSRV : TravelStateType.SRV,
                    };
                    break;


                case JournalTypeEnum.DockSRV:
                    hes = new HistoryEntryStatus(prev)
                    {
                        TravelState = prev.TravelState == TravelStateType.MulticrewSRV ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
                    };
                    break;

                case JournalTypeEnum.Touchdown:
                    var td = (JournalTouchdown)je;
                    if (td.PlayerControlled == true)        // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentLocation = new JournalLocation(td.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              td.Body ?? prev.CurrentLocation?.BodyName,
                                                              BodyDefinitions.BodyType.Planet,
                                                              td.BodyID.HasValue ? td.BodyID.Value : prev.BodyID,
                                                              null,null,StationDefinitions.StarportTypes.Unknown,
                                                              td.Latitude, td.Longitude),
                            TravelState = prev.TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
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
                        if (prev.TravelState == TravelStateType.Fighter)
                        {
                            hes = new HistoryEntryStatus(prev)
                            {
                                TravelState = TravelStateType.NormalSpace
                            };
                        }
                        else if (prev.TravelState == TravelStateType.MulticrewFighter)
                        {
                            hes = new HistoryEntryStatus(prev)
                            {
                                TravelState = TravelStateType.MulticrewNormalSpace
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
                                TravelState = prev.TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewFighter : TravelStateType.Fighter,
                            };
                        }
                        break;
                    }

                case JournalTypeEnum.ApproachBody:
                    {
                        JournalApproachBody jappbody = (JournalApproachBody)je;
                        // approach body is worse than Approach settlement as this has more info
                        bool apsbetter = prev.CurrentLocation is JournalApproachSettlement jas && jas.BodyName == jappbody.BodyName;

                        // approach body is worse than supercruise exit as this has bodytype
                        bool scebetter = prev.CurrentLocation is JournalSupercruiseExit jse && jse.BodyName == jappbody.BodyName;

                        //System.Diagnostics.Debug.WriteLine($"ApproachBody approachsettlement {apsbetter} scebetter {scebetter}");

                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentLocation = apsbetter || scebetter ? prev.CurrentLocation : jappbody,
                            LastApproachBody = jappbody,
                        };
                        break;
                    }
                
                case JournalTypeEnum.ApproachSettlement:
                    JournalApproachSettlement jappsettlement = (JournalApproachSettlement)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        CurrentLocation = jappsettlement,
                    };
                    break;

                case JournalTypeEnum.SupercruiseExit:
                    {
                        JournalSupercruiseExit jsexit = (JournalSupercruiseExit)je;

                        // we are presuming the approach is better that scexit since scexit does not have name of station
                        bool apsbetter = prev.CurrentLocation is JournalApproachSettlement jas && jas.BodyName == jsexit.BodyName;

                        hes = new HistoryEntryStatus(prev)
                        {
                            TravelState = jsexit.Taxi == true ? (prev.TravelState == TravelStateType.DropShipSupercruise ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                                jsexit.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                        TravelStateType.NormalSpace,

                            // from 2016, SE had body and bodytype. Keep last location
                            CurrentLocation = apsbetter ? prev.CurrentLocation :
                                                        new JournalSupercruiseExit(jsexit.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              jsexit.Body ?? prev.CurrentLocation?.BodyName,
                                                              jsexit.BodyType != BodyDefinitions.BodyType.Unknown ? jsexit.BodyType : (prev.CurrentLocation?.BodyType ?? BodyDefinitions.BodyType.Unknown),
                                                              jsexit.BodyID ?? prev.BodyID,
                                                              prev.CurrentLocation?.Name,
                                                              prev.CurrentLocation?.Name_Localised,
                                                              prev.CurrentLocation?.FDStationType ?? StationDefinitions.StarportTypes.Unknown,
                                                              jsexit.Taxi, jsexit.Multicrew),

                            LastDockingGranted = null,
                        };

                        break;
                    }
                case JournalTypeEnum.LeaveBody:
                    JournalLeaveBody jlbody = (JournalLeaveBody)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        CurrentLocation = new JournalLocation(jlbody.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              null,
                                                              BodyDefinitions.BodyType.Star,
                                                              null),
                        LastApproachBody = null,
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
                        LastApproachBody = null,
                        LastTaxiDropship = null,
                        LastDockingGranted = null,
                    };
                    break;

                case JournalTypeEnum.Loadout:
                    var jloadout = (JournalLoadout)je;
                    if (ItemData.IsShip(jloadout.ShipFD))     // if ship, make a new entry
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentShip = jloadout,
                        };
                    }
                    break;
                case JournalTypeEnum.ShipyardBuy:
                    {
                        var sb = (JournalShipyardBuy)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentShip = sb,
                        };

                        break;
                    }
                case JournalTypeEnum.ShipyardNew:
                    JournalShipyardNew jsnew = (JournalShipyardNew)je;
                    hes = new HistoryEntryStatus(prev)
                    {
                        CurrentShip = jsnew,
                    };
                    break;

                case JournalTypeEnum.ShipyardSwap:
                    {
                        JournalShipyardSwap jsswap = (JournalShipyardSwap)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            CurrentShip = jsswap,
                        };
                        break;
                    }

                case JournalTypeEnum.BookDropship:
                    hes = new HistoryEntryStatus(prev)
                    {
                        LastTaxiDropship = je as ITaxiDropship,
                    };
                    break;

                case JournalTypeEnum.CancelTaxi:
                case JournalTypeEnum.CancelDropship:
                    {
                        hes = new HistoryEntryStatus(prev)
                        {
                            LastTaxiDropship = null,
                        };
                        break;
                    }

                case JournalTypeEnum.BookTaxi:
                    hes = new HistoryEntryStatus(prev)
                    {
                        LastTaxiDropship = je as ITaxiDropship,
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

                case JournalTypeEnum.DockingGranted:
                    {
                        JournalDockingGranted dg = (JournalDockingGranted)je;
                        hes = new HistoryEntryStatus(prev)
                        {
                            LastDockingGranted = dg,
                        };
                    }
                    break;

                case JournalTypeEnum.DockingCancelled:
                    {
                        if (prev.DockingPad > 0)
                            hes = new HistoryEntryStatus(prev) { LastDockingGranted = null};
                    }
                    break;

                case JournalTypeEnum.DockingTimeout:
                    {
                        if (prev.DockingPad > 0)
                            hes = new HistoryEntryStatus(prev) { LastDockingGranted = null };
                    }
                    break;

            }

            //System.Diagnostics.Debug.Assert(hes.NBodyApproached == hes.BodyApproached);
            //System.Diagnostics.Debug.Assert(hes.NBookedDropship == hes.BookedDropship);
            return hes;
        }
    }

}