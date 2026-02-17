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
        public enum TravelStateType
        {
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
        
        public bool IsNormalSpace { get { return TravelState == TravelStateType.NormalSpace || TravelState == TravelStateType.TaxiNormalSpace || TravelState == TravelStateType.MulticrewNormalSpace || TravelState == TravelStateType.DropShipNormalSpace; } }
        public bool IsLanded { get { return TravelState == TravelStateType.Landed || TravelState == TravelStateType.MulticrewLanded; } }
        public bool IsSRV { get { return TravelState == TravelStateType.SRV || TravelState == TravelStateType.MulticrewSRV; } }
        public bool IsFighter { get { return TravelState == TravelStateType.Fighter || TravelState == TravelStateType.MulticrewFighter; } }

        public bool IsLandedInShipOrSRV { get { return TravelState == TravelStateType.Landed || TravelState == TravelStateType.SRV || 
                    TravelState == TravelStateType.MulticrewLanded || TravelState == TravelStateType.MulticrewSRV; }}

        // true for OnCrewWithCaptain entries (they seem  to be marked with Multicrew) and true if entries are in physical multicrew
        public bool IsInMultiCrew { get { return TravelState >= TravelStateType.MulticrewDocked && TravelState <= TravelStateType.MulticrewFighter; } }

        public TravelStateType TravelState { get; private set; } = TravelStateType.Unknown;  // travel state

        // last jump into the system, FSDJump or Carrier. Contains system info
        public JournalLocOrJump LastFSDJump { get; private set; }                // MAY BE NULL

        // the current location, Journal Location, SupercruiseExit, FSDJump, CarrierJump, Docked, ApproachBody, ApproachSettlement
        // For all, it does not imply the travel state, as we hold the best location even if you change modes, such as disembark at a planetary port or starport
        public IBodyFeature CurrentLocation { get; private set; }               // MAY BE NULL
        public string WhereAmI => CurrentLocation?.Name_Localised ?? CurrentLocation?.BodyName ?? CurrentLocation?.StarSystem ?? "Unknown";
        public double? Latitude => CurrentLocation?.Latitude;
        public double? Longitude => CurrentLocation?.Longitude;
        public string BodyName => CurrentLocation?.BodyName;
        public BodyDefinitions.BodyType BodyType => CurrentLocation?.BodyType ?? BodyDefinitions.BodyType.Unknown;
        public int? BodyID => CurrentLocation?.BodyID;
        public bool HasBodyID { get { return BodyID >= 0; } }
        public string StationName_Localised => CurrentLocation?.Name_Localised;
        public StationDefinitions.StarportTypes? FDStationType => CurrentLocation?.FDStationType;
        public long? MarketID => CurrentLocation?.MarketID;
        public string StationFaction => CurrentLocation?.StationFaction;

        // Non null when approached a body. ApproachBody has the same fields in as IBodyFeature so use this to make more compatible
        public IBodyFeature LastApproachBody { get; private set; }   // MAY BE NULL. 
        public bool BodyApproached => LastApproachBody != null;


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
        public StationDefinitions.StarportTypes DockingStationType => LastDockingGranted?.FDStationType ?? StationDefinitions.StarportTypes.Unknown;    
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

        // non null when booked a taxi or dropship, up to embark, then null
        public ITaxiDropship LastTaxiDropship { get; private set; } // MAY BE NULL
        public bool BookedDropship => LastTaxiDropship is JournalBookDropship;
        public bool BookedTaxi => LastTaxiDropship is JournalBookTaxi;

        public double CurrentBoost { get; private set; } = 1;             // current boost multiplier due to jet cones and synthesis

        public HistoryEntryStatus()
        {
        }

        public HistoryEntryStatus(HistoryEntryStatus prevstatus)
        {
            TravelState = prevstatus.TravelState;
            LastFSDJump = prevstatus.LastFSDJump;
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

        public HistoryEntryStatus Update(JournalEntry je, ISystem sys)
        {
            HistoryEntryStatus hes = this;

            bool ignored = false;
            bool handled = true;
            var day = new System.DateTime(2022, 5, 13);
            bool debugrange = true;// jeH.EventTimeUTC >= day && je.EventTimeUTC < day.AddDays(1);

            switch (je.EventTypeID)
            {
                #region Location changes

                case JournalTypeEnum.Location:
                    {
                        JournalLocation jloc = je as JournalLocation;

                        // Location comes out on a Fleetcarrier when on foot with docked=false, on foot= true, detect it and reject using this location, keep with old
                        bool weareonafleetcarrieronfoot = FDStationType == StationDefinitions.StarportTypes.FleetCarrier && jloc.Docked == false && jloc.OnFoot == true;

                        // if we are on foot at a plantary base, and we have a location, we have more info already
                        bool weareonaplanterarybase = jloc.OnFoot == true && !jloc.Docked && (TravelState == TravelStateType.OnFootPlanetaryPort || TravelState == TravelStateType.OnFootPlanet);

                        //if (je.EventTimeUTC == new System.DateTime(2022, 5, 13, 12, 25, 14))  { }

                        // Location when we are onfoot or outside station just gives 'Station'. if we know the type CORRECT
                        if ( jloc.FDStationType == StationDefinitions.StarportTypes.Station && CurrentLocation.FDStationType != StationDefinitions.StarportTypes.Unknown)
                        {
                            jloc.FDStationType = CurrentLocation.FDStationType;
                            jloc.StationType = StationDefinitions.ToEnglish(jloc.FDStationType);
                        }

                        if (!weareonafleetcarrieronfoot && !weareonaplanterarybase)
                        {
                            bool locinstation = jloc.FDStationType != StationDefinitions.StarportTypes.Unknown;

                            TravelStateType t =
                                            jloc.Docked ? (jloc.Multicrew == true ? TravelStateType.MulticrewDocked : TravelStateType.Docked) :
                                                (jloc.InSRV == true) ? (jloc.Multicrew == true ? TravelStateType.MulticrewSRV : TravelStateType.SRV) :
                                                    jloc.Taxi == true ? TravelStateType.TaxiNormalSpace :          // can't be in dropship, must be in normal space.
                                                        jloc.OnFoot == true ? (locinstation ? TravelStateType.OnFootStarPort : TravelState == TravelStateType.OnFootPlanetaryPort ? TravelState : TravelStateType.OnFootPlanet) :
                                                            jloc.Latitude.HasValue ? (jloc.Multicrew == true ? TravelStateType.MulticrewLanded : TravelStateType.Landed) :
                                                                jloc.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                                    TravelStateType.NormalSpace;

                            hes = new HistoryEntryStatus(this)     // Bodyapproach copy over we should be in the same state as last..
                            {
                                TravelState = t,
                                CurrentLocation = jloc,
                                Wanted = jloc.Wanted,
                                CurrentBoost = 1,
                                JumpSequence = null,
                                LastDockingGranted = null,
                            };
                        }
                        else
                        {
                            ignored = true;
                        }

                        if ( hes.TravelState == TravelStateType.OnFootStarPort || hes.TravelState == TravelStateType.OnFootPlanetaryPort)
                        {

                        }

                        break;
                    }

                case JournalTypeEnum.FSDJump:
                    {
                        var jfsd = (je as JournalFSDJump);
                        hes = new HistoryEntryStatus(this)
                        {
                            // transition to Supercruise
                            TravelState = jfsd.Taxi == true ? (TravelState == TravelStateType.DropShipSupercruise || TravelState == TravelStateType.DropShipNormalSpace ? TravelStateType.DropShipSupercruise : TravelStateType.TaxiSupercruise) :
                                            jfsd.Multicrew == true ? TravelStateType.MulticrewSupercruise :
                                                TravelStateType.Supercruise,
                            LastFSDJump = jfsd,      // system info is fsdjump
                            CurrentLocation = jfsd, // location is fsdjump
                            Wanted = jfsd.Wanted,
                            LastApproachBody = null,
                            LastDockingGranted = null,
                            CurrentBoost = 1,
                            JumpSequence = null,
                        };

                        break;
                    }

                case JournalTypeEnum.CarrierJump:       // missing from 4.0 odyssey
                    var jcarrierjump = (je as JournalCarrierJump);
                    hes = new HistoryEntryStatus(this)     // we are docked or on foot on a carrier - travel state stays the same
                    {
                        LastFSDJump = jcarrierjump,      // system info is fsdjump
                        CurrentLocation = jcarrierjump,
                        Wanted = jcarrierjump.Wanted,
                        LastApproachBody = null,
                        LastDockingGranted = null,
                        CurrentBoost = 1,
                        JumpSequence = null,
                    };
                    break;

                case JournalTypeEnum.Docked:        // Docked not seen when in Taxi.
                    {
                        JournalDocked jdocked = (JournalDocked)je;
                        //System.Diagnostics.Debug.WriteLine("{0} Docked {1} {2} {3}", jdocked.EventTimeUTC, jdocked.StationName, jdocked.StationType, jdocked.Faction);

                        if (HasBodyID && BodyName != null)        // Augment the docked with any body info we have got
                        {
                            jdocked.BodyID = BodyID;
                            jdocked.BodyName = BodyName;
                            jdocked.BodyType = BodyType;
                            jdocked.Latitude = Latitude;
                            jdocked.Longitude = Longitude;
                        }

                        hes = new HistoryEntryStatus(this)
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

                case JournalTypeEnum.Undocked:      // undocked not seen when in taxi (still 26?)
                    {
                        var ju = je as JournalUndocked;
                        hes = new HistoryEntryStatus(this)
                        {
                            TravelState = ju.Taxi == true ? (TravelState == TravelStateType.DropShipDocked ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                            ju.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                TravelStateType.NormalSpace,
                        };
                        break;
                    }


                case JournalTypeEnum.ApproachBody:
                    if (IsInSupercruise || IsNormalSpace)     // ignore unless in flight
                    {
                        JournalApproachBody jappbody = (JournalApproachBody)je;
                        // approach body is worse than Approach settlement as this has more info
                        bool apsbetter = CurrentLocation is JournalApproachSettlement jas && jas.BodyName == jappbody.BodyName;

                        // approach body is worse than supercruise exit as this has bodytype
                        bool scebetter = CurrentLocation is JournalSupercruiseExit jse && jse.BodyName == jappbody.BodyName;

                        //System.Diagnostics.Debug.WriteLine($"ApproachBody approachsettlement {apsbetter} scebetter {scebetter}");

                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentLocation = apsbetter || scebetter ? CurrentLocation : jappbody,
                            LastApproachBody = jappbody,
                        };
                    }
                    else
                    {
                        ignored = true;
                    }
                    break;

                case JournalTypeEnum.LeaveBody:
                    JournalLeaveBody jlbody = (JournalLeaveBody)je;
                    hes = new HistoryEntryStatus(this)
                    {
                        LastApproachBody = null,
                        CurrentLocation = hes.LastFSDJump,       // leave body, lets go back to the system info which we have since we are star space
                    };
                    break;

                case JournalTypeEnum.ApproachSettlement:

                    if (IsInSupercruise || IsNormalSpace)     // we can get these after re-logging when on plantary base, ignore unless in flight
                    {
                        JournalApproachSettlement jappsettlement = (JournalApproachSettlement)je;

                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentLocation = jappsettlement,
                        };
                    }
                    else
                    {
                        ignored = true;
                    }
                    break;

                case JournalTypeEnum.SupercruiseEntry:
                    {
                        var sc = je as JournalSupercruiseEntry;
                        hes = new HistoryEntryStatus(this)
                        {
                            TravelState = sc.Taxi == true ? (TravelState == TravelStateType.DropShipNormalSpace ? TravelStateType.DropShipSupercruise : TravelStateType.TaxiSupercruise) :
                                                sc.Multicrew == true ? TravelStateType.MulticrewSupercruise :
                                                            TravelStateType.Supercruise,
                            CurrentLocation = hes.LastApproachBody != null ? hes.LastApproachBody : hes.LastFSDJump,        // if we are still approached, thats the best, else its the star
                            LastTaxiDropship = null,
                            LastDockingGranted = null,
                        };

                        break;
                    }

                case JournalTypeEnum.SupercruiseExit:
                    {
                        JournalSupercruiseExit jsexit = (JournalSupercruiseExit)je;

                        // we are presuming the approach is better that scexit since scexit does not have name of station
                        bool apsbetter = CurrentLocation is JournalApproachSettlement jas && jas.BodyName == jsexit.BodyName;

                        // we note if we exited at station
                        bool exitedatstation = jsexit.BodyType == BodyDefinitions.BodyType.Station;

                        hes = new HistoryEntryStatus(this)
                        {
                            TravelState = jsexit.Taxi == true ? (TravelState == TravelStateType.DropShipSupercruise ? TravelStateType.DropShipNormalSpace : TravelStateType.TaxiNormalSpace) :
                                                jsexit.Multicrew == true ? TravelStateType.MulticrewNormalSpace :
                                                        TravelStateType.NormalSpace,

                            // from 2016, SE had body and bodytype. Keep last location. Augment the exit with info
                            CurrentLocation = apsbetter ? CurrentLocation :
                                                        new JournalSupercruiseExit(jsexit.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              jsexit.Body ?? CurrentLocation?.BodyName,
                                                              jsexit.BodyType != BodyDefinitions.BodyType.Unknown ? jsexit.BodyType : (CurrentLocation?.BodyType ?? BodyDefinitions.BodyType.Unknown),
                                                              jsexit.BodyID ?? BodyID,
                                                              exitedatstation ? jsexit.BodyName : CurrentLocation?.Name,
                                                              exitedatstation ? jsexit.BodyName : CurrentLocation?.Name_Localised, 
                                                              jsexit.FDStationType,
                                                              jsexit.Taxi, jsexit.Multicrew),

                            LastDockingGranted = null,
                        };

                        break;
                    }

                case JournalTypeEnum.Touchdown:
                    var td = (JournalTouchdown)je;
                    if (td.PlayerControlled == true)        // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentLocation = new JournalLocation(td.EventTimeUTC, sys.Name, sys.SystemAddress,
                                                              td.Body ?? CurrentLocation?.BodyName,
                                                              BodyDefinitions.BodyType.Planet,
                                                              td.BodyID.HasValue ? td.BodyID.Value : CurrentLocation?.BodyID,
                                                              null, null, StationDefinitions.StarportTypes.Unknown,
                                                              CurrentLocation?.Name, CurrentLocation?.Name_Localised,
                                                              td.Latitude, td.Longitude),
                            TravelState = TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
                            CurrentBoost = 1,
                        };
                    }
                    else
                        ignored = true;

                    break;

                case JournalTypeEnum.Liftoff:
                    var loff = (JournalLiftoff)je;
                    if (loff.PlayerControlled == true)         // can get this when not player controlled
                    {
                        hes = new HistoryEntryStatus(this)      // not going to use Body, since we must already have it.
                        {
                            TravelState = TravelState == TravelStateType.MulticrewLanded ? TravelStateType.MulticrewNormalSpace : TravelStateType.NormalSpace,
                            CurrentLocation = hes.LastApproachBody ?? CurrentLocation,   // Last approach body should be set, but if not, back up
                        };
                    }
                    else
                        ignored = true;

                    break;


                #endregion


                #region Travel State Changes

                case JournalTypeEnum.LoadGame:
                    JournalLoadGame jlg = je as JournalLoadGame;

                    hes = new HistoryEntryStatus(this)
                    {
                        LastLoadGame = jlg,
                        OnCrewWithCaptain = null,    // can't be in a crew at this point

                        TravelState = jlg.InSuit ? (TravelState) :
                                         jlg.InTaxi ? TravelStateType.TaxiNormalSpace :
                                             jlg.InSRV ? TravelStateType.SRV :
                                                    TravelState != TravelStateType.Unknown ? TravelState :
                                                        TravelStateType.Docked,
                        CurrentShip = jlg.InShip ? jlg : CurrentShip,
                        LastTaxiDropship = null,
                        CurrentBoost = 1,
                        LastDockingGranted = null,
                    };
                    break;


                case JournalTypeEnum.Embark:        // foot-> SRV/Ship in multicrew or not.
                    {
                        var em = (JournalEmbark)je;
                        hes = new HistoryEntryStatus(this)
                        {
                            TravelState = em.SRV ? (em.Multicrew ? TravelStateType.MulticrewSRV : TravelStateType.SRV) :
                                                em.Taxi ? (BookedDropship ? TravelStateType.DropShipDocked : TravelStateType.TaxiDocked) :
                                                    em.Multicrew ? (TravelState == TravelStateType.OnFootPlanet ? TravelStateType.MulticrewLanded : TravelStateType.MulticrewDocked) :
                                                        TravelState == TravelStateType.OnFootPlanet ? TravelStateType.Landed :
                                                            TravelStateType.Docked,
                            LastTaxiDropship = null,
                        };
                        break;
                    }

                case JournalTypeEnum.Disembark:     // SRV/Ship -> on foot
                    var disem = (JournalDisembark)je;

                    // due to bug, stationname/type is now always given
                    bool instation = disem.HasStationTypeName || FDStationType != StationDefinitions.StarportTypes.Unknown;       
                    bool fc = FDStationType == StationDefinitions.StarportTypes.FleetCarrier;
                    
                    //if ( debugrange) System.Diagnostics.Debug.WriteLine($"Disembark {disem.HasStationTypeName} {FDStationType} {fc}");

                    hes = new HistoryEntryStatus(this)
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
                    hes = new HistoryEntryStatus(this)
                    {
                        TravelState = TravelStateType.OnFootPlanet,
                    };
                    break;

                case JournalTypeEnum.LaunchSRV:
                    hes = new HistoryEntryStatus(this)
                    {
                        TravelState = TravelState == TravelStateType.MulticrewLanded ? TravelStateType.MulticrewSRV : TravelStateType.SRV,
                    };
                    break;


                case JournalTypeEnum.DockSRV:
                    hes = new HistoryEntryStatus(this)
                    {
                        TravelState = TravelState == TravelStateType.MulticrewSRV ? TravelStateType.MulticrewLanded : TravelStateType.Landed,
                    };
                    break;


                case JournalTypeEnum.FighterDestroyed:
                case JournalTypeEnum.DockFighter:
                    {
                        if (TravelState == TravelStateType.Fighter)
                        {
                            hes = new HistoryEntryStatus(this)
                            {
                                TravelState = TravelStateType.NormalSpace
                            };
                        }
                        else if (TravelState == TravelStateType.MulticrewFighter)
                        {
                            hes = new HistoryEntryStatus(this)
                            {
                                TravelState = TravelStateType.MulticrewNormalSpace
                            };
                        }
                        else
                            ignored = true;
                        break;
                    }


                case JournalTypeEnum.LaunchFighter:
                    {
                        var j = je as JournalLaunchFighter;
                        if (j.PlayerControlled)
                        {
                            hes = new HistoryEntryStatus(this)
                            {
                                TravelState = TravelState == TravelStateType.MulticrewNormalSpace ? TravelStateType.MulticrewFighter : TravelStateType.Fighter,
                            };
                        }
                        else
                            ignored = true;
                        break;
                    }

                case JournalTypeEnum.Died:
                    hes = new HistoryEntryStatus(this)
                    {
                        TravelState = TravelStateType.Unknown,
                        OnCrewWithCaptain = null,
                        LastApproachBody = null,
                        LastTaxiDropship = null,
                        LastDockingGranted = null,
                    };
                    break;

                #endregion

                #region Other

                case JournalTypeEnum.StartJump:
                    {
                        var jsj = (je as JournalStartJump);
                        if (jsj.IsHyperspace)
                        {
                            hes = new HistoryEntryStatus(this)
                            {
                                JumpSequence = jsj,
                            };
                        }
                        else
                            ignored = true;
                        break;
                    }

                case JournalTypeEnum.JoinACrew:
                    hes = new HistoryEntryStatus(this)
                    {
                        OnCrewWithCaptain = ((JournalJoinACrew)je).Captain
                    };
                    break;
                case JournalTypeEnum.QuitACrew:
                    hes = new HistoryEntryStatus(this)
                    {
                        OnCrewWithCaptain = null
                    };
                    break;

                case JournalTypeEnum.Loadout:
                    var jloadout = (JournalLoadout)je;
                    if (ItemData.IsShip(jloadout.ShipFD))     // if ship, make a new entry
                    {
                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentShip = jloadout,
                        };
                    }
                    else
                        ignored = true;
                    break;
                case JournalTypeEnum.ShipyardBuy:
                    {
                        var sb = (JournalShipyardBuy)je;
                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentShip = sb,
                        };

                        break;
                    }
                case JournalTypeEnum.ShipyardNew:
                    JournalShipyardNew jsnew = (JournalShipyardNew)je;
                    hes = new HistoryEntryStatus(this)
                    {
                        CurrentShip = jsnew,
                    };
                    break;

                case JournalTypeEnum.ShipyardSwap:
                    {
                        JournalShipyardSwap jsswap = (JournalShipyardSwap)je;
                        hes = new HistoryEntryStatus(this)
                        {
                            CurrentShip = jsswap,
                        };
                        break;
                    }

                case JournalTypeEnum.BookDropship:
                    hes = new HistoryEntryStatus(this)
                    {
                        LastTaxiDropship = je as ITaxiDropship,
                    };
                    break;

                case JournalTypeEnum.CancelTaxi:
                case JournalTypeEnum.CancelDropship:
                    {
                        hes = new HistoryEntryStatus(this)
                        {
                            LastTaxiDropship = null,
                        };
                        break;
                    }

                case JournalTypeEnum.BookTaxi:
                    hes = new HistoryEntryStatus(this)
                    {
                        LastTaxiDropship = je as ITaxiDropship,
                    };
                    break;

                case JournalTypeEnum.JetConeBoost:
                    {
                        JournalJetConeBoost jjcb = (JournalJetConeBoost)je;
                        hes = new HistoryEntryStatus(this)
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
                            hes = new HistoryEntryStatus(this)
                            {
                                CurrentBoost = jsyn.FSDBoostValue
                            };
                        }
                        else
                            ignored = true;
                    }
                    break;

                case JournalTypeEnum.DockingGranted:
                    {
                        JournalDockingGranted dg = (JournalDockingGranted)je;
                        hes = new HistoryEntryStatus(this)
                        {
                            LastDockingGranted = dg,
                        };
                    }
                    break;

                case JournalTypeEnum.DockingCancelled:
                    {
                        if (DockingPad > 0)
                            hes = new HistoryEntryStatus(this) { LastDockingGranted = null };
                        else
                            ignored = true;
                    }
                    break;

                case JournalTypeEnum.DockingTimeout:
                    {
                        if (DockingPad > 0)
                            hes = new HistoryEntryStatus(this) { LastDockingGranted = null };
                        else
                            ignored = true;
                    }
                    break;

                case JournalTypeEnum.Shutdown:
                    break;      // debugging purposes to show its an event

                default:
                    handled = false;
                    break;

                #endregion
            }

#if false
            if (debugrange)
            {
                if (hes.LastFSDJump != LastFSDJump && hes.LastFSDJump != null)
                {
                    //here, field width
                    System.Diagnostics.Debug.WriteLine($"HES {je.EventTimeUTC.ToStringZuluInvariant()}:{je.EventTypeStr,20} {hes.TravelState,20} FSDJump {hes.LastFSDJump.EventTypeStr,20} : `{hes.LastFSDJump.BodyName}`:{hes.LastFSDJump.BodyID}:{hes.LastFSDJump.BodyType} , {hes.LastFSDJump.Name} , st: {hes.LastFSDJump.FDStationType}");
                }

                //bool print = je is JournalLoadGame || je is JournalShutdown || je is JournalDisembark || je is JournalEmbark || (hes.CurrentLocation != CurrentLocation && hes.CurrentLocation != null) || ignored;
                if (handled && hes.CurrentLocation!=null)
                {
                    //here, field width
                    System.Diagnostics.Debug.WriteLine($"HES {je.EventTimeUTC.ToStringZuluInvariant()}:{je.EventTypeStr,20} {hes.TravelState,20} {(ignored? "IGNORED" : "Loc    ")} {hes.CurrentLocation.EventTypeStr,20} : `{hes.CurrentLocation.BodyName}`:{hes.CurrentLocation.BodyID}:{hes.CurrentLocation.BodyType} , name `{hes.CurrentLocation.Name}` , latlon {hes.CurrentLocation.Latitude}:{hes.CurrentLocation.Longitude} st: {hes.CurrentLocation.FDStationType}");
                }
            }
#else
            ignored = !ignored; handled = !handled; debugrange = !debugrange;
#endif
            return hes;
        }

    }

}