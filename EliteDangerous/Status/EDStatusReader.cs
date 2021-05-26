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

using BaseUtils.JSON;
using EliteDangerousCore.UIEvents;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{WatcherFolder}")]
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

        long prev_flags = 0;
        long prev_flags2 = 0;
        int prev_guifocus = NotPresent;                 
        int prev_firegroup = NotPresent;
        double prev_curfuel = NotPresent;
        double prev_curres = NotPresent;
        string prev_legalstatus = null;
        int prev_cargo = NotPresent;
        UIEvents.UIPips.Pips prev_pips = new UIEvents.UIPips.Pips(null);
        UIEvents.UIPosition.Position prev_pos = new UIEvents.UIPosition.Position();     // default is MinValue
        double prev_heading = UIEvents.UIPosition.InvalidValue;    // this forces a pos report
        double prev_jpradius = UIEvents.UIPosition.InvalidValue;    // this forces a pos report

        string prev_bodyname = null;

        double prev_oxygen = NotPresent;        // odyssey
        double prev_temperature = NotPresent;
        double prev_gravity = NotPresent;
        double prev_health = NotPresent;
        string prev_selectedweapon = null;
        string prev_selectedweaponloc = null;

        private enum StatusFlagsShip                        // PURPOSELY PRIVATE - don't want users to get into low level detail of BITS
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

        private enum StatusFlagsSRV
        {
            SrvHandbrake = 12,
            SrvTurret = 13,
            SrvUnderShip = 14,
            SrvDriveAssist = 15,
            SrvHighBeam = 31,
        }

        private enum StatusFlagsAll
        {
            ShieldsUp = 3,
            Lights = 8,
            LowFuel = 19,
            HasLatLong = 21,
            IsInDanger = 22,
            NightVision = 28,             // 3.3
        }

        // these two below determine the shiptype (operating mode)

        private enum StatusFlagsShipType
        {
            InMainShip = 24,        
            InFighter = 25,
            InSRV = 26,
            ShipMask = (1 << InMainShip) | (1 << InFighter) | (1 << InSRV),
        }

        private enum StatusFlagsReportedInOtherEvents       // reported via other mechs than flags 
        {
            AltitudeFromAverageRadius = 29, // 3.4, via position
        }

        private enum StatusFlags2ShipType                   // used to compute ship type
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

        private enum StatusFlags2               // these are bool flags, reported sep.
        {
            AimDownSight = 5,
            GlideMode = 12,
            BreathableAtmosphere = 16,
        }

        private enum StatusFlags2OtherFlags     // these are states reported as part of other messages
        {
            LowOxygen = 6,
            LowHealth = 7,
            Cold = 8,
            Hot = 9,
            VeryCold = 10,
            VeryHot = 11,
            TempBits = (1<<Cold) | (1<<Hot) | ( 1<< VeryCold) | (1<<VeryHot),
        }

        string prev_text = null;

        public List<UIEvent> Scan()
        {
          //  System.Diagnostics.Debug.WriteLine(Environment.TickCount % 100000 + "Check " + statusfile);

            if (File.Exists(statusfile))
            {
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
                        jo = JObject.ParseThrowCommaEOL(text);  // and of course the json could be crap
                        prev_text = text;       // set after successful parse
                    }
                }
                catch
                { }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                }

                if (jo != null)
                {
                    DateTime EventTimeUTC = jo["timestamp"].DateTimeUTC();
                            
                    List<UIEvent> events = new List<UIEvent>();

                    UIMode shiptype = ShipType(prev_flags, prev_flags2);

                    long curflags = jo["Flags"].Long(0);
                    long curflags2 = jo["Flags2"].Long(0);      // 0 is backwards compat with horizons

                    bool fireoverall = false;                   // fire the overall
                    bool changedmajormode = false;
                    long flagsdelta2 = 0;                       // what changed between prev and current

                    if (curflags != prev_flags || curflags2 != prev_flags2)
                    {
                        UIMode nextshiptype = ShipType(curflags, curflags2);

                        //System.Diagnostics.Debug.WriteLine("UI Flags changed {0} {1} {2} -> {3} {4} {5}", prev_flags.Value, prev_flags2.Value, shiptype, curflags, curflags2, nextshiptype);

                        if ( nextshiptype.Mode != shiptype.Mode)        // changed ship situation..
                        {
                            changedmajormode = nextshiptype.MajorMode != shiptype.MajorMode;   // did we change major mode
                            events.Add(new UIEvents.UIMode(nextshiptype.Mode, nextshiptype.MajorMode, EventTimeUTC, changedmajormode));        // Generate an event for it
                        }

                        events.AddRange(ReportFlagState(typeof(StatusFlags2), curflags2, prev_flags2, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsShip), curflags, prev_flags, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsSRV), curflags, prev_flags, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsAll), curflags, prev_flags, EventTimeUTC, changedmajormode));
                        fireoverall = true;

                        flagsdelta2 = curflags2 ^ prev_flags2;        // record the delta here for later processing, some of those flags go into the main reports

                        prev_flags = curflags;
                        prev_flags2 = curflags2;
                        shiptype = nextshiptype;
                    }

                    int curguifocus = jo["GuiFocus"].Int(NotPresent);
                    if (curguifocus != prev_guifocus || changedmajormode)
                    {
                        events.Add(new UIEvents.UIGUIFocus(curguifocus, EventTimeUTC, changedmajormode));
                        prev_guifocus = curguifocus;
                        fireoverall = true;
                    }

                    int[] pips = jo["Pips"]?.ToObjectQ<int[]>();            // may appear/disappear
                    UIEvents.UIPips.Pips curpips = new UIEvents.UIPips.Pips(pips);      // can accept null as input

                    if ( !curpips.Equal(prev_pips) || changedmajormode)                  // if change in pips, or changed mode
                    {
                        events.Add(new UIEvents.UIPips(curpips, EventTimeUTC, changedmajormode));
                        prev_pips = curpips;
                        fireoverall = true;
                    }

                    int curfiregroup = jo["FireGroup"].Int(NotPresent);      // may appear/disappear.

                    if (curfiregroup != prev_firegroup || changedmajormode)
                    {
                        events.Add(new UIEvents.UIFireGroup(curfiregroup + 1, EventTimeUTC, changedmajormode));
                        prev_firegroup = curfiregroup;
                        fireoverall = true;
                    }

                    JToken jfuel = jo["Fuel"];
                    double curfuel = jfuel != null ? jfuel["FuelMain"].Double(-1) : -1;
                    double curres = jfuel != null ? jfuel["FuelReservoir"].Double(-1) : -1;

                    if (Math.Abs(curfuel - prev_curfuel) >= 0.1 || Math.Abs(curres - prev_curres) >= 0.01 || changedmajormode)  // don't fire if small changes
                    {
                        events.Add(new UIEvents.UIFuel(curfuel, curres, shiptype.Mode, EventTimeUTC, changedmajormode));
                        prev_curfuel = curfuel;
                        prev_curres = curres;
                        fireoverall = true;
                    }

                    int curcargo = jo["Cargo"].Int(NotPresent);      // may appear/disappear and only introduced for 3.3
                    if (curcargo != prev_cargo || changedmajormode)
                    {
                        events.Add(new UIEvents.UICargo(curcargo, shiptype.Mode, EventTimeUTC, changedmajormode));
                        prev_cargo = curcargo;
                        fireoverall = true;
                    }

                    double jlat = jo["Latitude"].Double(UIEvents.UIPosition.InvalidValue);       // if not there, min value
                    double jlon = jo["Longitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jalt = jo["Altitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jheading = jo["Heading"].Double(UIEvents.UIPosition.InvalidValue);
                    double jpradius = jo["PlanetRadius"].Double(UIEvents.UIPosition.InvalidValue);       // 3.4

                    if (jlat != prev_pos.Latitude || jlon != prev_pos.Longitude || jalt != prev_pos.Altitude || jheading != prev_heading || jpradius != prev_jpradius || changedmajormode)
                    {
                        UIEvents.UIPosition.Position newpos = new UIEvents.UIPosition.Position()
                        {
                            Latitude = jlat, Longitude = jlon,
                            Altitude = jalt, AltitudeFromAverageRadius = Flags(curflags, StatusFlagsReportedInOtherEvents.AltitudeFromAverageRadius)
                        };

                        events.Add(new UIEvents.UIPosition(newpos, jheading, jpradius, EventTimeUTC, changedmajormode));
                        prev_pos = newpos;
                        prev_heading = jheading;
                        prev_jpradius = jpradius;
                        fireoverall = true;
                    }

                    string cur_legalstatus = jo["LegalState"].StrNull();

                    if (cur_legalstatus != prev_legalstatus || changedmajormode)
                    {
                        events.Add(new UIEvents.UILegalStatus(cur_legalstatus, EventTimeUTC, changedmajormode));
                        prev_legalstatus = cur_legalstatus;
                        fireoverall = true;
                    }

                    string cur_bodyname = jo["BodyName"].StrNull();

                    if (cur_bodyname != prev_bodyname || changedmajormode)
                    {
                        events.Add(new UIEvents.UIBodyName(cur_bodyname, EventTimeUTC, changedmajormode));
                        prev_bodyname = cur_bodyname;
                        fireoverall = true;
                    }

                    string cur_weapon = jo["SelectedWeapon"].StrNull();                 // null if not there
                    string cur_weaponloc = jo["SelectedWeapon_Localised"].Str();        // empty if not there

                    if (cur_weapon != prev_selectedweapon || changedmajormode)
                    {
                        events.Add(new UIEvents.UISelectedWeapon(cur_weapon, cur_weaponloc, EventTimeUTC, changedmajormode));
                        prev_selectedweapon = cur_weapon;
                        prev_selectedweaponloc = cur_weaponloc;
                        fireoverall = true;
                    }

                    double oxygen = jo["Oxygen"].Double(NotPresent);                //-1 is not present
                    oxygen = oxygen < 0 ? oxygen : oxygen * 100;                    // correct to 0-100%
                    bool lowoxygen = Flags(curflags2, StatusFlags2OtherFlags.LowOxygen);

                    if (oxygen != prev_oxygen || Flags(flagsdelta2,StatusFlags2OtherFlags.LowOxygen) || changedmajormode)
                    {
                        events.Add(new UIEvents.UIOxygen(oxygen, lowoxygen , EventTimeUTC, changedmajormode));
                        prev_oxygen = oxygen;
                        fireoverall = true;
                    }

                    double health = jo["Health"].Double(NotPresent);                //-1 is not present
                    health = health < 0 ? health : health * 100;                    // correct to 0-100%
                    bool lowhealth = Flags(curflags2, StatusFlags2OtherFlags.LowHealth);

                    if (health != prev_health || Flags(flagsdelta2,StatusFlags2OtherFlags.LowHealth) || changedmajormode)
                    {
                        events.Add(new UIEvents.UIHealth(health, lowhealth, EventTimeUTC, changedmajormode));
                        prev_health = health;
                        fireoverall = true;
                    }

                    double gravity = jo["Gravity"].Double(NotPresent);                //-1 is not present

                    if (gravity != prev_gravity || changedmajormode)
                    {
                        events.Add(new UIEvents.UIGravity(gravity, EventTimeUTC, changedmajormode));
                        prev_gravity = gravity;
                        fireoverall = true;
                    }

                    double temperature = jo["Temperature"].Double(NotPresent);       //-1 is not present

                    UIEvents.UITemperature.TempState tempstate =
                        Flags(curflags2,StatusFlags2OtherFlags.VeryCold) ? UIEvents.UITemperature.TempState.VeryCold :       // order important, you can get Cold | VeryCold
                        Flags(curflags2,StatusFlags2OtherFlags.VeryHot) ? UIEvents.UITemperature.TempState.VeryHot :
                        Flags(curflags2,StatusFlags2OtherFlags.Cold) ? UIEvents.UITemperature.TempState.Cold :
                        Flags(curflags2,StatusFlags2OtherFlags.Hot) ? UIEvents.UITemperature.TempState.Hot :
                                                            UIEvents.UITemperature.TempState.Normal;

                    if (temperature != prev_temperature || (flagsdelta2 & (long)StatusFlags2OtherFlags.TempBits) != 0 || changedmajormode)
                    {

                        events.Add(new UIEvents.UITemperature(temperature,tempstate, EventTimeUTC, changedmajormode));
                        prev_temperature = temperature;
                        fireoverall = true;
                    }

                    if ( fireoverall )
                    {
                        List<UITypeEnum> flagsset = ReportFlagState(typeof(StatusFlagsShip), curflags);
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlagsSRV), curflags));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlagsAll), curflags));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlags2), curflags2));

                        bool glidemode = Flags(curflags2,StatusFlags2.GlideMode);
                        bool breathableatmos = Flags(curflags2,StatusFlags2.BreathableAtmosphere);

                        UIEvents.UIOverallStatus.FSDStateType fsdstate = UIEvents.UIOverallStatus.FSDStateType.Normal;
                        if (Flags(curflags, StatusFlagsShip.FsdMassLocked))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.MassLock;
                        if (Flags(curflags, StatusFlagsShip.FsdJump))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Jumping;
                        else if (Flags(curflags,StatusFlagsShip.FsdCharging))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Charging;
                        else if (Flags(curflags2,StatusFlags2.GlideMode))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Gliding;
                        else if (Flags(curflags,StatusFlagsShip.FsdCooldown))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Cooldown;

                        events.Add(new UIEvents.UIOverallStatus(shiptype.MajorMode, shiptype.Mode, flagsset, prev_guifocus, prev_pips, prev_firegroup, 
                                                                prev_curfuel,prev_curres, prev_cargo, prev_pos, prev_heading, prev_jpradius, prev_legalstatus, 
                                                                prev_bodyname,
                                                                prev_health,lowhealth,gravity,prev_temperature,tempstate,prev_oxygen,lowoxygen,
                                                                prev_selectedweapon, prev_selectedweaponloc,
                                                                fsdstate, breathableatmos,
                                                                EventTimeUTC, 
                                                                changedmajormode));        // overall list of flags set
                    }

                    //for debugging, keep
#if true
                    foreach (var uient in events)
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("New UI entry from journal {0} {1} refresh {2}", uient.EventTimeUTC, uient.EventTypeStr, changedmajormode));
                            //BaseUtils.Variables v = new BaseUtils.Variables();
                            //v.AddPropertiesFieldsOfClass(uient, "", null, 2);
                            //foreach (var x in v.NameEnumuerable)
                            //    System.Diagnostics.Trace.WriteLine(string.Format("  {0} = {1}", x, v[x]));
                        }
#endif

                    return events;
                }
            }

            return new List<UIEvent>();
        }

        List<UITypeEnum> ReportFlagState(Type enumtype, long curflags)
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

        List<UIEvent> ReportFlagState(Type enumtype, long curflags, long prev_flags, DateTime EventTimeUTC, bool changedmajormode)
        {
            List<UIEvent> events = new List<UIEvent>();
            long delta = curflags ^ prev_flags;

            //System.Diagnostics.Debug.WriteLine("Flags changed to {0:x} from {1:x} delta {2:x}", curflags, prev_flags , delta);

            foreach (string n in Enum.GetNames(enumtype))
            {
                int v = (int)Enum.Parse(enumtype, n);

                bool flag = ((curflags >> v) & 1) != 0;

                if (changedmajormode || ((delta >> v) & 1) != 0)
                {
                   // System.Diagnostics.Debug.WriteLine("..Flag " + n + " at " +v +" changed to " + flag);
                    var e = UIEvent.CreateEvent(n, EventTimeUTC, changedmajormode, flag);
                    System.Diagnostics.Debug.Assert(e != null);
                    events.Add(e);
                }
            }

            return events;
        }

        static bool Flags(long flags, Object bit)
        {
            return (flags & (1L << (int)bit)) != 0;
        }


        static private UIMode ShipType(long flags1, long flags2)
        {
            if (Flags(flags2, StatusFlags2ShipType.InMulticrew))
            {
                if (Flags(flags1, StatusFlagsShipType.InSRV))
                    return new UIMode( UIMode.ModeType.MulticrewSRV, UIMode.MajorModeType.Multicrew);
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                    return new UIMode( UIMode.ModeType.MulticrewSupercruise, UIMode.MajorModeType.Multicrew);
                if (Flags(flags1, StatusFlagsShip.Docked))
                    return new UIMode( Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.MulticrewDockedPlanet : UIEvents.UIMode.ModeType.MulticrewDockedStarPort, UIMode.MajorModeType.Multicrew);
                if (Flags(flags1, StatusFlagsShip.Landed))
                    return new UIMode( UIMode.ModeType.MulticrewLanded, UIMode.MajorModeType.Multicrew);
                return new UIMode( UIMode.ModeType.MulticrewNormalSpace, UIMode.MajorModeType.Multicrew);
            }
            else if (Flags(flags2, StatusFlags2ShipType.InTaxi))
            {
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                    return new UIMode( UIMode.ModeType.TaxiSupercruise, UIMode.MajorModeType.Taxi);
                if (Flags(flags1, StatusFlagsShip.Docked))
                    return new UIMode( Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.TaxiDockedPlanet : UIEvents.UIMode.ModeType.TaxiDocked, UIMode.MajorModeType.Taxi);
                return new UIMode( UIMode.ModeType.TaxiNormalSpace, UIMode.MajorModeType.Taxi);
            }
            else if (Flags(flags1, StatusFlagsShipType.InFighter))
            {
                return new UIMode( UIMode.ModeType.Fighter, UIMode.MajorModeType.Fighter);
            }
            else if (Flags(flags1, StatusFlagsShipType.InSRV))
            {
                return new UIMode( UIMode.ModeType.SRV, UIMode.MajorModeType.SRV);
            }
            else if (Flags(flags2, StatusFlags2ShipType.OnFoot))
            {
                if (Flags(flags2, StatusFlags2ShipType.OnFootInStation))        // station means starport
                {
                    return new UIMode(Flags(flags2, StatusFlags2ShipType.OnFootInHangar) ? UIEvents.UIMode.ModeType.OnFootStarPortHangar : UIEvents.UIMode.ModeType.OnFootStarPortSocialSpace, UIMode.MajorModeType.OnFoot);
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootInHangar))        // if set, but no station, its a planetary port
                {
                    return new UIMode( UIMode.ModeType.OnFootPlantaryPortHangar, UIMode.MajorModeType.OnFoot);
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootInSocialSpace))
                {
                    return new UIMode( UIMode.ModeType.OnFootPlantaryPortSocialSpace, UIMode.MajorModeType.OnFoot);
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootOnPlanet))
                {
                    return new UIMode( Flags(flags2, StatusFlags2.BreathableAtmosphere) ? UIEvents.UIMode.ModeType.OnFootInstallationInside : UIEvents.UIMode.ModeType.OnFootPlanet, UIMode.MajorModeType.OnFoot);
                }
            }
            else if(Flags(flags1, StatusFlagsShipType.InMainShip))
            {
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                {
                    return new UIMode( UIMode.ModeType.MainShipSupercruise, UIMode.MajorModeType.Ship);
                }
                if (Flags(flags1, StatusFlagsShip.Docked))
                {
                    return new UIMode( Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.MainShipDockedPlanet : UIEvents.UIMode.ModeType.MainShipDockedStarPort, UIMode.MajorModeType.Ship);
                }
                if (Flags(flags1, StatusFlagsShip.Landed))
                {
                    return new UIMode( UIMode.ModeType.MainShipLanded, UIMode.MajorModeType.Ship);
                }
                return new UIMode( UIMode.ModeType.MainShipNormalSpace, UIMode.MajorModeType.Ship);
            }

            return new UIMode( UIMode.ModeType.None, UIMode.MajorModeType.None);
        }


    }
}
