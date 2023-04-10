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

using QuickJSON;
using EliteDangerousCore.UIEvents;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{StatusFolder}")]
    public class StatusReader
    {
        public string StatusFolder { get; set; }

        private string statusfile;

        public StatusReader(string datapath)
        {
            StatusFolder = datapath;
            statusfile = Path.Combine(StatusFolder, "status.json");
        }

        const int NotPresent = -1;

        public long Flags1 { get; private set; } = 0;
        public long Flags2 { get; private set; } = 0;
        public UIMode ShipType() { return ShipType(Flags1, Flags2); }
        public static bool CheckFlags(long flags, Object bit)  { return (flags & (1L << (int)bit)) != 0; }
        public int GUIFocus { get; private set; } = NotPresent;     // shows -1 when no status has been read, then always reads a valid value. On foot, there is no GUI field, so set to NoFocus
        public int FireGroup { get; private set; } = NotPresent;
        public double FuelLevel { get; private set; } = NotPresent;
        public double ReserveLevel { get; private set; } = NotPresent;
        public string LegalStatus { get; private set; } = null;
        public int CargoCount { get; private set; } = NotPresent;
        public UIEvents.UIPips.Pips PIPStatus { get; private set; } = new UIEvents.UIPips.Pips(null);
        public UIEvents.UIPosition.Position Position { get; private set; } = new UIEvents.UIPosition.Position();     
        public double Heading { get; private set; } = UIEvents.UIPosition.InvalidValue;    // this forces a pos report
        public double BodyRadius { get; private set; } = UIEvents.UIPosition.InvalidValue;    // this forces a pos report

        public string BodyName { get; private set; } = null;        // which you are present on, landed/on foot

        public double Oxygen { get; private set; } = NotPresent;        // odyssey
        public double Temperature { get; private set; } = NotPresent;
        public double Gravity { get; private set; } = NotPresent;
        public double Health { get; private set; } = NotPresent;
        public string SelectedWeapon { get; private set; } = null;
        public string SelectedWeaponLocalised { get; private set; } = null;

        public string DestinationName { get; private set; } = "";           // when you have set a target, a body, station, system
        public long DestinationSystemAddress { get; private set; } = 0;
        public int DestinationBodyID { get; private set; } = 0;

        private string prev_text = null;

        public void Reset()
        {
            Flags1 = Flags2 = 0;
            GUIFocus = FireGroup = NotPresent;
            FuelLevel = ReserveLevel = NotPresent;
            LegalStatus = null;
            CargoCount = NotPresent;
            PIPStatus = new UIEvents.UIPips.Pips(null);
            Position = new UIEvents.UIPosition.Position();
            Heading = BodyRadius = UIEvents.UIPosition.InvalidValue;
            BodyName = null;
            Oxygen = Temperature = Gravity = Health = NotPresent;
            SelectedWeapon = SelectedWeaponLocalised = null;
            DestinationName = "";
            DestinationSystemAddress = 0;
            DestinationBodyID = 0;
            prev_text = null;
        }

        public enum StatusFlags1Ship                             // Flags -> Events
        {
            Docked = 0, // (on a landing pad)
            Landed = 1, // (on planet surface)
            LandingGear = 2,
            Supercruise = 4,
            FlightAssist = 5,
            HardpointsDeployed = 6,
            InWing = 7,
            CargoScoopDeployed = 9,
            SilentRunning = 10,
            ScoopingFuel = 11,
            FsdMassLocked = 16,
            FsdCharging = 17,
            FsdCooldown = 18,
            OverHeating = 20,
            BeingInterdicted = 23,
            HUDInAnalysisMode = 27,     // 3.3
            FsdJump = 30,
        }

        public enum StatusFlags1SRV                              // Flags -> Events
        {
            SrvHandbrake = 12,
            SrvTurret = 13,
            SrvUnderShip = 14,
            SrvDriveAssist = 15,
            SrvHighBeam = 31,
        }

        public enum StatusFlags1All                             // Flags -> Events
        {
            ShieldsUp = 3,
            Lights = 8,
            LowFuel = 19,
            HasLatLong = 21,
            IsInDanger = 22,
            NightVision = 28,             // 3.3
        }

        private enum StatusFlags1NotDirectEvents
        {
            AltitudeFromAverageRadius = 29, // 3.4, via position
        }

        // shiptype (operating mode)

        public enum StatusFlags1ShipType                        // Flags for ship type
        {
            InMainShip = 24,        
            InFighter = 25,
            InSRV = 26,
            ShipMask = (1 << InMainShip) | (1 << InFighter) | (1 << InSRV),
        }

        public enum StatusFlags2ShipType                        // used to compute ship type
        {
            OnFoot = 0,
            InTaxi = 1,
            InMulticrew = 2,
            OnFootInStation = 3,
            OnFootOnPlanet = 4,
            OnFootInHangar = 13,
            OnFootInSocialSpace = 14,
            OnFootExterior = 15,
        }

        public enum StatusFlags2Events                          // flags -> Events
        {
            AimDownSight = 5,
            GlideMode = 12,
            BreathableAtmosphere = 16,
        }

        public enum StatusFlags2ReportedInOtherMessages         // these are reported as part of other messages
        {
            LowOxygen = 6,
            LowHealth = 7,
            Cold = 8,
            Hot = 9,
            VeryCold = 10,
            VeryHot = 11,
            TempBits = (1 << Cold) | (1 << Hot) | (1 << VeryCold) | (1 << VeryHot),
            FSDHyperdriveCharging = 19,         // U14 nov 22
        }

        public List<UIEvent> Scan()
        {
            if (File.Exists(statusfile))
            {
                //System.Diagnostics.Debug.WriteLine(Environment.TickCount % 100000 + "Check Status " + statusfile);

                JObject jo = null;

                Stream stream = null;
                try
                {
                    stream = File.Open(statusfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    StreamReader reader = new StreamReader(stream);

                    string text = reader.ReadToEnd();

                    stream.Close();

                    if (text.HasChars() && (prev_text == null || !text.Equals(prev_text)))        // if text not null, and prev text is null OR not equal
                    {
                        jo = JObject.Parse(text,JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL | JToken.ParseOptions.ThrowOnError | JToken.ParseOptions.IgnoreBadObjectValue);  // and of course the json could be crap
                        prev_text = text;       // set after successful parse
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Status reader exception " + ex);
                }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                }

                if (jo != null)
                {
                    DateTime EventTimeUTC = jo["timestamp"].DateTimeUTC();
                            
                    List<UIEvent> events = new List<UIEvent>();

                    UIMode shiptype = ShipType(Flags1, Flags2);

                    long curflags1 = jo["Flags"].Long(0);
                    long curflags2 = jo["Flags2"].Long(0);      // 0 is backwards compat with horizons

                    bool fireoverall = false;                   // fire the overall
                    bool changedmajormode = false;
                    long flagsdelta2 = 0;                       // what changed between prev and current

                    if (curflags1 != Flags1 || curflags2 != Flags2)
                    {
                        UIMode nextshiptype = ShipType(curflags1, curflags2);

                        //System.Diagnostics.Debug.WriteLine("UI Flags changed {0} {1} {2} -> {3} {4} {5}", prev_flags.Value, prev_flags2.Value, shiptype, curflags, curflags2, nextshiptype);

                        if ( nextshiptype.Mode != shiptype.Mode)        // changed ship situation..
                        {
                            changedmajormode = nextshiptype.MajorMode != shiptype.MajorMode;   // did we change major mode
                            events.Add(new UIEvents.UIMode(nextshiptype.Mode, nextshiptype.MajorMode, EventTimeUTC, changedmajormode));        // Generate an event for it
                            //System.Diagnostics.Debug.WriteLine($"UI Mode change to {nextshiptype.Mode} {nextshiptype.MajorMode}");
                        }

                        events.AddRange(ReportFlagState(typeof(StatusFlags2Events), curflags2, Flags2, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlags1Ship), curflags1, Flags1, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlags1SRV), curflags1, Flags1, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlags1All), curflags1, Flags1, EventTimeUTC, changedmajormode));

                        // special, if we changed this event, we add aux data onto it about hyperdrive. U14 nov 22.
                        UIEvent fsdcharging = events.Find(x => x is UIFsdCharging);
                        if ( fsdcharging != null)
                        {
                            ((UIFsdCharging)fsdcharging).FsdCharging = CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.FSDHyperdriveCharging);
                        }

                        fireoverall = true;

                        flagsdelta2 = curflags2 ^ Flags2;        // record the delta here for later processing, some of those flags go into the main reports

                        Flags1 = curflags1;
                        Flags2 = curflags2;
                        shiptype = nextshiptype;
                    }

                    int curguifocus = jo["GuiFocus"].Int((int)UIGUIFocus.Focus.NoFocus);            // in landed mode, its disappears, so its the same as NoFocus.
                    if (curguifocus != GUIFocus || changedmajormode)
                    {
                        events.Add(new UIEvents.UIGUIFocus(curguifocus, EventTimeUTC, changedmajormode));
                        GUIFocus = curguifocus;
                        fireoverall = true;
                    }

                    int[] pips = jo["Pips"]?.ToObjectQ<int[]>();            // may appear/disappear
                    UIEvents.UIPips.Pips curpips = new UIEvents.UIPips.Pips(pips);      // can accept null as input

                    if ( !curpips.Equal(PIPStatus) || changedmajormode)                  // if change in pips, or changed mode
                    {
                        events.Add(new UIEvents.UIPips(curpips, EventTimeUTC, changedmajormode));
                        PIPStatus = curpips;
                        fireoverall = true;
                    }

                    int curfiregroup = jo["FireGroup"].Int(NotPresent);      // may appear/disappear.

                    if (curfiregroup != FireGroup || changedmajormode)
                    {
                        events.Add(new UIEvents.UIFireGroup(curfiregroup + 1, EventTimeUTC, changedmajormode));
                        FireGroup = curfiregroup;
                        fireoverall = true;
                    }

                    JToken jfuel = jo["Fuel"];
                    double curfuel = jfuel != null ? jfuel["FuelMain"].Double(-1) : -1;
                    double curres = jfuel != null ? jfuel["FuelReservoir"].Double(-1) : -1;

                    // don't fire if small changes.  Reserve fires less if its above 0.2
                    if (Math.Abs(curfuel - FuelLevel) >= 0.1 || Math.Abs(curres - ReserveLevel) >= (curres>0.2?0.05:0.01) || changedmajormode)  
                    {
                        events.Add(new UIEvents.UIFuel(curfuel, curres, shiptype.Mode, CheckFlags(curflags1,StatusFlags1Ship.ScoopingFuel), EventTimeUTC, changedmajormode));
                        FuelLevel = curfuel;
                        ReserveLevel = curres;
                        fireoverall = true;
                    }

                    int curcargo = jo["Cargo"].Int(NotPresent);      // may appear/disappear and only introduced for 3.3
                    if (curcargo != CargoCount || changedmajormode)
                    {
                        events.Add(new UIEvents.UICargo(curcargo, shiptype.Mode, EventTimeUTC, changedmajormode));
                        CargoCount = curcargo;
                        fireoverall = true;
                    }

                    string cur_bodyname = jo["BodyName"].StrNull();
                    bool bodynamechanged = false;

                    if (cur_bodyname != BodyName || changedmajormode)
                    {
                        events.Add(new UIEvents.UIBodyName(cur_bodyname, EventTimeUTC, changedmajormode));
                        BodyName = cur_bodyname;
                        fireoverall = true;
                        bodynamechanged = true;
                    }

                    double jlat = jo["Latitude"].Double(UIEvents.UIPosition.InvalidValue);       // if not there, min value
                    double jlon = jo["Longitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jalt = jo["Altitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jheading = jo["Heading"].Double(UIEvents.UIPosition.InvalidValue);
                    if (jheading != UIEvents.UIPosition.InvalidValue)
                        jheading = (jheading + 360.0) % 360.0;          // april 23 seen on foot heading be negative, normalise to 0-360
                    double jpradius = jo["PlanetRadius"].Double(UIEvents.UIPosition.InvalidValue);       // 3.4

                    if (jlat != Position.Latitude || jlon != Position.Longitude || jalt != Position.Altitude || jheading != Heading || 
                                    jpradius != BodyRadius || bodynamechanged || changedmajormode)
                    {
                        UIEvents.UIPosition.Position newpos = new UIEvents.UIPosition.Position()
                        {
                            Latitude = jlat, Longitude = jlon,
                            Altitude = jalt, AltitudeFromAverageRadius = CheckFlags(curflags1, StatusFlags1NotDirectEvents.AltitudeFromAverageRadius)
                        };

                        events.Add(new UIEvents.UIPosition(newpos, jheading, jpradius, cur_bodyname, EventTimeUTC, changedmajormode));
                        Position = newpos;
                        Heading = jheading;
                        BodyRadius = jpradius;
                        fireoverall = true;
                    }

                    string cur_legalstatus = jo["LegalState"].StrNull();

                    if (cur_legalstatus != LegalStatus || changedmajormode)
                    {
                        events.Add(new UIEvents.UILegalStatus(cur_legalstatus, EventTimeUTC, changedmajormode));
                        LegalStatus = cur_legalstatus;
                        fireoverall = true;
                    }

                    JObject destination = jo["Destination"].Object();
                    if ( destination != null )
                    {
                        string newdestination = destination["Name"].Str();
                        int newbody = destination["Body"].Int(0);
                        long newsys = destination["System"].Long(0);

                        if ( newdestination != DestinationName || newbody != DestinationBodyID || newsys != DestinationSystemAddress)       // if changed
                        {
                            DestinationName = newdestination;
                            DestinationBodyID = newbody;
                            DestinationSystemAddress = newsys;
                            events.Add(new UIEvents.UIDestination(DestinationName, DestinationBodyID, DestinationSystemAddress, EventTimeUTC, changedmajormode));
                            fireoverall = true;
                        }
                    }
                    else if ( DestinationName.HasChars())
                    {
                        DestinationName = "";
                        DestinationBodyID = 0;
                        DestinationSystemAddress = 0;
                        events.Add(new UIEvents.UIDestination(DestinationName, DestinationBodyID, DestinationSystemAddress, EventTimeUTC, changedmajormode));
                        fireoverall = true;
                    }

                    string cur_weapon = jo["SelectedWeapon"].StrNull();                 // null if not there
                    string cur_weaponloc = jo["SelectedWeapon_Localised"].Str();        // empty if not there

                    if (cur_weapon != SelectedWeapon || changedmajormode)
                    {
                        events.Add(new UIEvents.UISelectedWeapon(cur_weapon, cur_weaponloc, EventTimeUTC, changedmajormode));
                        SelectedWeapon = cur_weapon;
                        SelectedWeaponLocalised = cur_weaponloc;
                        fireoverall = true;
                    }

                    double oxygen = jo["Oxygen"].Double(NotPresent);                //-1 is not present
                    oxygen = oxygen < 0 ? oxygen : oxygen * 100;                    // correct to 0-100%
                    bool lowoxygen = CheckFlags(curflags2, StatusFlags2ReportedInOtherMessages.LowOxygen);

                    if (oxygen != Oxygen || CheckFlags(flagsdelta2,StatusFlags2ReportedInOtherMessages.LowOxygen) || changedmajormode)
                    {
                        events.Add(new UIEvents.UIOxygen(oxygen, lowoxygen , EventTimeUTC, changedmajormode));
                        Oxygen = oxygen;
                        fireoverall = true;
                    }

                    double health = jo["Health"].Double(NotPresent);                //-1 is not present
                    health = health < 0 ? health : health * 100;                    // correct to 0-100%
                    bool lowhealth = CheckFlags(curflags2, StatusFlags2ReportedInOtherMessages.LowHealth);

                    if (health != Health || CheckFlags(flagsdelta2,StatusFlags2ReportedInOtherMessages.LowHealth) || changedmajormode)
                    {
                        events.Add(new UIEvents.UIHealth(health, lowhealth, EventTimeUTC, changedmajormode));
                        Health = health;
                        fireoverall = true;
                    }

                    double gravity = jo["Gravity"].Double(NotPresent);                //-1 is not present

                    if (gravity != Gravity || changedmajormode)
                    {
                        events.Add(new UIEvents.UIGravity(gravity, EventTimeUTC, changedmajormode));
                        Gravity = gravity;
                        fireoverall = true;
                    }

                    double temperature = jo["Temperature"].Double(NotPresent);       //-1 is not present

                    UIEvents.UITemperature.TempState tempstate =
                        CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.VeryCold) ? UIEvents.UITemperature.TempState.VeryCold :       // order important, you can get Cold | VeryCold
                        CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.VeryHot) ? UIEvents.UITemperature.TempState.VeryHot :
                        CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.Cold) ? UIEvents.UITemperature.TempState.Cold :
                        CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.Hot) ? UIEvents.UITemperature.TempState.Hot :
                                                            UIEvents.UITemperature.TempState.Normal;

                    if (Math.Abs(temperature-Temperature) >= 1 || (flagsdelta2 & (long)StatusFlags2ReportedInOtherMessages.TempBits) != 0 || changedmajormode)
                    {

                        events.Add(new UIEvents.UITemperature(temperature,tempstate, EventTimeUTC, changedmajormode));
                        Temperature = temperature;
                        fireoverall = true;
                    }

                    if ( fireoverall )
                    {
                        List<UITypeEnum> flagsset = ReportFlagState(typeof(StatusFlags1Ship), curflags1);
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlags1SRV), curflags1));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlags1All), curflags1));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlags2Events), curflags2));

                        bool glidemode = CheckFlags(curflags2,StatusFlags2Events.GlideMode);
                        bool breathableatmos = CheckFlags(curflags2,StatusFlags2Events.BreathableAtmosphere);

                        UIEvents.UIOverallStatus.FSDStateType fsdstate = UIEvents.UIOverallStatus.FSDStateType.Normal;
                        if (CheckFlags(curflags1, StatusFlags1Ship.FsdMassLocked))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.MassLock;
                        if (CheckFlags(curflags1, StatusFlags1Ship.FsdJump))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Jumping;
                        else if (CheckFlags(curflags1,StatusFlags1Ship.FsdCharging))            // older 3.8 will return Charging only
                            fsdstate = CheckFlags(curflags2,StatusFlags2ReportedInOtherMessages.FSDHyperdriveCharging) ? UIEvents.UIOverallStatus.FSDStateType.ChargingFSDFlagSet : UIEvents.UIOverallStatus.FSDStateType.Charging;
                        else if (CheckFlags(curflags2,StatusFlags2Events.GlideMode))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Gliding;
                        else if (CheckFlags(curflags1,StatusFlags1Ship.FsdCooldown))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Cooldown;

                        events.Add(new UIEvents.UIOverallStatus(shiptype.MajorMode, shiptype.Mode, flagsset, GUIFocus, PIPStatus, FireGroup, 
                                                                FuelLevel,ReserveLevel, CargoCount, Position, Heading, BodyRadius, LegalStatus, 
                                                                BodyName,
                                                                Health,lowhealth,gravity,Temperature,tempstate,Oxygen,lowoxygen,
                                                                SelectedWeapon, SelectedWeaponLocalised,
                                                                fsdstate, breathableatmos,
                                                                DestinationName, DestinationBodyID,DestinationSystemAddress,
                                                                EventTimeUTC, 
                                                                changedmajormode));        // overall list of flags set
                    }

                    //for debugging, keep
#if true
                    foreach (var uient in events)
                    {
                        if (!(uient is UIOverallStatus))        // dont report this, its due to individual ones
                        {
                            System.Diagnostics.Trace.WriteLine($"UI Event {uient.EventTimeUTC} {uient.EventTypeStr} : {uient.ToString()}");
                            //BaseUtils.Variables v = new BaseUtils.Variables();
                            //v.AddPropertiesFieldsOfClass(uient, "", null, 2);
                            //foreach (var x in v.NameEnumuerable)
                            //    System.Diagnostics.Trace.WriteLine(string.Format("  {0} = {1}", x, v[x]));
                        }
                    }
#endif

                    return events;
                }
            }

            return new List<UIEvent>();
        }

        private List<UITypeEnum> ReportFlagState(Type enumtype, long curflags)
        {
            List<UITypeEnum> flags = new List<UITypeEnum>();
            foreach (string n in Enum.GetNames(enumtype))
            {
                int v = (int)Enum.Parse(enumtype, n);

                bool flag = ((curflags >> v) & 1) != 0;
                if (flag)
                    flags.Add((UITypeEnum)Enum.Parse(typeof(UITypeEnum), n));
            }

            return flags;
        }

        private List<UIEvent> ReportFlagState(Type enumtype, long curflags, long prev_flags, DateTime EventTimeUTC, bool refresh)
        {
            List<UIEvent> events = new List<UIEvent>();
            long delta = curflags ^ prev_flags;

            //System.Diagnostics.Debug.WriteLine("Flags changed to {0:x} from {1:x} delta {2:x}", curflags, prev_flags , delta);

            foreach (string name in Enum.GetNames(enumtype))
            {
                int v = (int)Enum.Parse(enumtype, name);

                bool flag = ((curflags >> v) & 1) != 0;

                if (refresh || ((delta >> v) & 1) != 0)
                {
                   // System.Diagnostics.Debug.WriteLine("..Flag " + n + " at " +v +" changed to " + flag);
                    var e = UIEvent.CreateEvent(name, EventTimeUTC, refresh, flag);
                    System.Diagnostics.Debug.Assert(e != null);
                    events.Add(e);
                }
            }

            return events;
        }

        private static UIMode ShipType(long flags1, long flags2)
        {
            if (CheckFlags(flags2, StatusFlags2ShipType.InMulticrew))
            {
                if (CheckFlags(flags1, StatusFlags1ShipType.InSRV))
                    return new UIMode( UIMode.ModeType.MulticrewSRV, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlags1Ship.Supercruise))
                    return new UIMode( UIMode.ModeType.MulticrewSupercruise, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlags1Ship.Docked))
                    return new UIMode( CheckFlags(flags1, StatusFlags1All.HasLatLong) ? UIEvents.UIMode.ModeType.MulticrewDockedPlanet : UIEvents.UIMode.ModeType.MulticrewDockedStarPort, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlags1Ship.Landed))
                    return new UIMode( UIMode.ModeType.MulticrewLanded, UIMode.MajorModeType.Multicrew);
                return new UIMode( UIMode.ModeType.MulticrewNormalSpace, UIMode.MajorModeType.Multicrew);
            }
            else if (CheckFlags(flags2, StatusFlags2ShipType.InTaxi))
            {
                if (CheckFlags(flags1, StatusFlags1Ship.Supercruise))
                    return new UIMode( UIMode.ModeType.TaxiSupercruise, UIMode.MajorModeType.Taxi);
                if (CheckFlags(flags1, StatusFlags1Ship.Docked))
                    return new UIMode( CheckFlags(flags1, StatusFlags1All.HasLatLong) ? UIEvents.UIMode.ModeType.TaxiDockedPlanet : UIEvents.UIMode.ModeType.TaxiDocked, UIMode.MajorModeType.Taxi);
                return new UIMode( UIMode.ModeType.TaxiNormalSpace, UIMode.MajorModeType.Taxi);
            }
            else if (CheckFlags(flags1, StatusFlags1ShipType.InFighter))
            {
                return new UIMode( UIMode.ModeType.Fighter, UIMode.MajorModeType.Fighter);
            }
            else if (CheckFlags(flags1, StatusFlags1ShipType.InSRV))
            {
                return new UIMode( UIMode.ModeType.SRV, UIMode.MajorModeType.SRV);
            }
            else if (CheckFlags(flags2, StatusFlags2ShipType.OnFoot))
            {
                if (CheckFlags(flags2, StatusFlags2ShipType.OnFootInStation))        // station means starport
                {
                    return new UIMode(CheckFlags(flags2, StatusFlags2ShipType.OnFootInHangar) ? UIEvents.UIMode.ModeType.OnFootStarPortHangar : UIEvents.UIMode.ModeType.OnFootStarPortSocialSpace, UIMode.MajorModeType.OnFoot);
                }
                else if (CheckFlags(flags2, StatusFlags2ShipType.OnFootInHangar))        // if set, but no station, its a planetary port
                {
                    return new UIMode( UIMode.ModeType.OnFootPlantaryPortHangar, UIMode.MajorModeType.OnFoot);
                }
                else if (CheckFlags(flags2, StatusFlags2ShipType.OnFootInSocialSpace))
                {
                    return new UIMode( UIMode.ModeType.OnFootPlantaryPortSocialSpace, UIMode.MajorModeType.OnFoot);
                }
                else if (CheckFlags(flags2, StatusFlags2ShipType.OnFootOnPlanet))
                {
                    return new UIMode(CheckFlags(flags2, StatusFlags2Events.BreathableAtmosphere) ? UIEvents.UIMode.ModeType.OnFootInstallationInside : UIEvents.UIMode.ModeType.OnFootPlanet, UIMode.MajorModeType.OnFoot);
                }
                else
                {
                    return new UIMode(UIEvents.UIMode.ModeType.OnFootPlanet, UIMode.MajorModeType.OnFoot);      // backup in case..
                }
            }
            else if(CheckFlags(flags1, StatusFlags1ShipType.InMainShip))
            {
                if (CheckFlags(flags1, StatusFlags1Ship.Supercruise))
                {
                    return new UIMode(UIMode.ModeType.MainShipSupercruise, UIMode.MajorModeType.MainShip);
                }
                else if (CheckFlags(flags1, StatusFlags1Ship.Docked))
                {
                    return new UIMode(CheckFlags(flags1, StatusFlags1All.HasLatLong) ? UIEvents.UIMode.ModeType.MainShipDockedPlanet : UIEvents.UIMode.ModeType.MainShipDockedStarPort, UIMode.MajorModeType.MainShip);
                }
                else if (CheckFlags(flags1, StatusFlags1Ship.Landed))
                {
                    return new UIMode(UIMode.ModeType.MainShipLanded, UIMode.MajorModeType.MainShip);
                }
                else
                {
                    return new UIMode(UIMode.ModeType.MainShipNormalSpace, UIMode.MajorModeType.MainShip);
                }
            }

            return new UIMode( UIMode.ModeType.None, UIMode.MajorModeType.None);
        }


    }
}
