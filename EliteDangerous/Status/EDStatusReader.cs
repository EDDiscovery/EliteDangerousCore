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

        public long Flags { get; private set; } = 0;
        public long Flags2 { get; private set; } = 0;
        public UIMode ShipType() { return ShipType(Flags, Flags2); }
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

        public string BodyName { get; private set; } = null;

        public double Oxygen { get; private set; } = NotPresent;        // odyssey
        public double Temperature { get; private set; } = NotPresent;
        public double Gravity { get; private set; } = NotPresent;
        public double Health { get; private set; } = NotPresent;
        public string SelectedWeapon { get; private set; } = null;
        public string SelectedWeaponLocalised { get; private set; } = null;

        private string prev_text = null;

        public void Reset()
        {
            Flags = Flags2 = 0;
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
            prev_text = null;
        }

        public enum StatusFlagsShip                             // Flags
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

        public enum StatusFlagsSRV                              // Flags
        {
            SrvHandbrake = 12,
            SrvTurret = 13,
            SrvUnderShip = 14,
            SrvDriveAssist = 15,
            SrvHighBeam = 31,
        }

        public enum StatusFlagsAll                             // Flags
        {
            ShieldsUp = 3,
            Lights = 8,
            LowFuel = 19,
            HasLatLong = 21,
            IsInDanger = 22,
            NightVision = 28,             // 3.3
        }

        // these two below determine the shiptype (operating mode)

        public enum StatusFlagsShipType                        // Flags
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

        public enum StatusFlags2ShipType                   // used to compute ship type
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

        public enum StatusFlags2               // these are bool flags, reported sep.
        {
            AimDownSight = 5,
            GlideMode = 12,
            BreathableAtmosphere = 16,
        }

        public enum StatusFlags2OtherFlags     // these are states reported as part of other messages
        {
            LowOxygen = 6,
            LowHealth = 7,
            Cold = 8,
            Hot = 9,
            VeryCold = 10,
            VeryHot = 11,
            TempBits = (1<<Cold) | (1<<Hot) | ( 1<< VeryCold) | (1<<VeryHot),
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

                    UIMode shiptype = ShipType(Flags, Flags2);

                    long curflags = jo["Flags"].Long(0);
                    long curflags2 = jo["Flags2"].Long(0);      // 0 is backwards compat with horizons

                    bool fireoverall = false;                   // fire the overall
                    bool changedmajormode = false;
                    long flagsdelta2 = 0;                       // what changed between prev and current

                    if (curflags != Flags || curflags2 != Flags2)
                    {
                        UIMode nextshiptype = ShipType(curflags, curflags2);

                        //System.Diagnostics.Debug.WriteLine("UI Flags changed {0} {1} {2} -> {3} {4} {5}", prev_flags.Value, prev_flags2.Value, shiptype, curflags, curflags2, nextshiptype);

                        if ( nextshiptype.Mode != shiptype.Mode)        // changed ship situation..
                        {
                            changedmajormode = nextshiptype.MajorMode != shiptype.MajorMode;   // did we change major mode
                            events.Add(new UIEvents.UIMode(nextshiptype.Mode, nextshiptype.MajorMode, EventTimeUTC, changedmajormode));        // Generate an event for it
                        }

                        events.AddRange(ReportFlagState(typeof(StatusFlags2), curflags2, Flags2, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsShip), curflags, Flags, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsSRV), curflags, Flags, EventTimeUTC, changedmajormode));
                        events.AddRange(ReportFlagState(typeof(StatusFlagsAll), curflags, Flags, EventTimeUTC, changedmajormode));
                        fireoverall = true;

                        flagsdelta2 = curflags2 ^ Flags2;        // record the delta here for later processing, some of those flags go into the main reports

                        Flags = curflags;
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

                    if (Math.Abs(curfuel - FuelLevel) >= 0.1 || Math.Abs(curres - ReserveLevel) >= 0.01 || changedmajormode)  // don't fire if small changes
                    {
                        events.Add(new UIEvents.UIFuel(curfuel, curres, shiptype.Mode, EventTimeUTC, changedmajormode));
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

                    double jlat = jo["Latitude"].Double(UIEvents.UIPosition.InvalidValue);       // if not there, min value
                    double jlon = jo["Longitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jalt = jo["Altitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jheading = jo["Heading"].Double(UIEvents.UIPosition.InvalidValue);
                    double jpradius = jo["PlanetRadius"].Double(UIEvents.UIPosition.InvalidValue);       // 3.4

                    if (jlat != Position.Latitude || jlon != Position.Longitude || jalt != Position.Altitude || jheading != Heading || jpradius != BodyRadius || changedmajormode)
                    {
                        UIEvents.UIPosition.Position newpos = new UIEvents.UIPosition.Position()
                        {
                            Latitude = jlat, Longitude = jlon,
                            Altitude = jalt, AltitudeFromAverageRadius = CheckFlags(curflags, StatusFlagsReportedInOtherEvents.AltitudeFromAverageRadius)
                        };

                        events.Add(new UIEvents.UIPosition(newpos, jheading, jpradius, EventTimeUTC, changedmajormode));
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

                    string cur_bodyname = jo["BodyName"].StrNull();

                    if (cur_bodyname != BodyName || changedmajormode)
                    {
                        events.Add(new UIEvents.UIBodyName(cur_bodyname, EventTimeUTC, changedmajormode));
                        BodyName = cur_bodyname;
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
                    bool lowoxygen = CheckFlags(curflags2, StatusFlags2OtherFlags.LowOxygen);

                    if (oxygen != Oxygen || CheckFlags(flagsdelta2,StatusFlags2OtherFlags.LowOxygen) || changedmajormode)
                    {
                        events.Add(new UIEvents.UIOxygen(oxygen, lowoxygen , EventTimeUTC, changedmajormode));
                        Oxygen = oxygen;
                        fireoverall = true;
                    }

                    double health = jo["Health"].Double(NotPresent);                //-1 is not present
                    health = health < 0 ? health : health * 100;                    // correct to 0-100%
                    bool lowhealth = CheckFlags(curflags2, StatusFlags2OtherFlags.LowHealth);

                    if (health != Health || CheckFlags(flagsdelta2,StatusFlags2OtherFlags.LowHealth) || changedmajormode)
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
                        CheckFlags(curflags2,StatusFlags2OtherFlags.VeryCold) ? UIEvents.UITemperature.TempState.VeryCold :       // order important, you can get Cold | VeryCold
                        CheckFlags(curflags2,StatusFlags2OtherFlags.VeryHot) ? UIEvents.UITemperature.TempState.VeryHot :
                        CheckFlags(curflags2,StatusFlags2OtherFlags.Cold) ? UIEvents.UITemperature.TempState.Cold :
                        CheckFlags(curflags2,StatusFlags2OtherFlags.Hot) ? UIEvents.UITemperature.TempState.Hot :
                                                            UIEvents.UITemperature.TempState.Normal;

                    if (temperature != Temperature || (flagsdelta2 & (long)StatusFlags2OtherFlags.TempBits) != 0 || changedmajormode)
                    {

                        events.Add(new UIEvents.UITemperature(temperature,tempstate, EventTimeUTC, changedmajormode));
                        Temperature = temperature;
                        fireoverall = true;
                    }

                    if ( fireoverall )
                    {
                        List<UITypeEnum> flagsset = ReportFlagState(typeof(StatusFlagsShip), curflags);
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlagsSRV), curflags));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlagsAll), curflags));
                        flagsset.AddRange(ReportFlagState(typeof(StatusFlags2), curflags2));

                        bool glidemode = CheckFlags(curflags2,StatusFlags2.GlideMode);
                        bool breathableatmos = CheckFlags(curflags2,StatusFlags2.BreathableAtmosphere);

                        UIEvents.UIOverallStatus.FSDStateType fsdstate = UIEvents.UIOverallStatus.FSDStateType.Normal;
                        if (CheckFlags(curflags, StatusFlagsShip.FsdMassLocked))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.MassLock;
                        if (CheckFlags(curflags, StatusFlagsShip.FsdJump))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Jumping;
                        else if (CheckFlags(curflags,StatusFlagsShip.FsdCharging))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Charging;
                        else if (CheckFlags(curflags2,StatusFlags2.GlideMode))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Gliding;
                        else if (CheckFlags(curflags,StatusFlagsShip.FsdCooldown))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Cooldown;

                        events.Add(new UIEvents.UIOverallStatus(shiptype.MajorMode, shiptype.Mode, flagsset, GUIFocus, PIPStatus, FireGroup, 
                                                                FuelLevel,ReserveLevel, CargoCount, Position, Heading, BodyRadius, LegalStatus, 
                                                                BodyName,
                                                                Health,lowhealth,gravity,Temperature,tempstate,Oxygen,lowoxygen,
                                                                SelectedWeapon, SelectedWeaponLocalised,
                                                                fsdstate, breathableatmos,
                                                                EventTimeUTC, 
                                                                changedmajormode));        // overall list of flags set
                    }

                    //for debugging, keep
#if false
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

        private List<UIEvent> ReportFlagState(Type enumtype, long curflags, long prev_flags, DateTime EventTimeUTC, bool changedmajormode)
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

        private static UIMode ShipType(long flags1, long flags2)
        {
            if (CheckFlags(flags2, StatusFlags2ShipType.InMulticrew))
            {
                if (CheckFlags(flags1, StatusFlagsShipType.InSRV))
                    return new UIMode( UIMode.ModeType.MulticrewSRV, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlagsShip.Supercruise))
                    return new UIMode( UIMode.ModeType.MulticrewSupercruise, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlagsShip.Docked))
                    return new UIMode( CheckFlags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.MulticrewDockedPlanet : UIEvents.UIMode.ModeType.MulticrewDockedStarPort, UIMode.MajorModeType.Multicrew);
                if (CheckFlags(flags1, StatusFlagsShip.Landed))
                    return new UIMode( UIMode.ModeType.MulticrewLanded, UIMode.MajorModeType.Multicrew);
                return new UIMode( UIMode.ModeType.MulticrewNormalSpace, UIMode.MajorModeType.Multicrew);
            }
            else if (CheckFlags(flags2, StatusFlags2ShipType.InTaxi))
            {
                if (CheckFlags(flags1, StatusFlagsShip.Supercruise))
                    return new UIMode( UIMode.ModeType.TaxiSupercruise, UIMode.MajorModeType.Taxi);
                if (CheckFlags(flags1, StatusFlagsShip.Docked))
                    return new UIMode( CheckFlags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.TaxiDockedPlanet : UIEvents.UIMode.ModeType.TaxiDocked, UIMode.MajorModeType.Taxi);
                return new UIMode( UIMode.ModeType.TaxiNormalSpace, UIMode.MajorModeType.Taxi);
            }
            else if (CheckFlags(flags1, StatusFlagsShipType.InFighter))
            {
                return new UIMode( UIMode.ModeType.Fighter, UIMode.MajorModeType.Fighter);
            }
            else if (CheckFlags(flags1, StatusFlagsShipType.InSRV))
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
                    return new UIMode(CheckFlags(flags2, StatusFlags2.BreathableAtmosphere) ? UIEvents.UIMode.ModeType.OnFootInstallationInside : UIEvents.UIMode.ModeType.OnFootPlanet, UIMode.MajorModeType.OnFoot);
                }
                else
                {
                    return new UIMode(UIEvents.UIMode.ModeType.OnFootPlanet, UIMode.MajorModeType.OnFoot);      // backup in case..
                }
            }
            else if(CheckFlags(flags1, StatusFlagsShipType.InMainShip))
            {
                if (CheckFlags(flags1, StatusFlagsShip.Supercruise))
                {
                    return new UIMode(UIMode.ModeType.MainShipSupercruise, UIMode.MajorModeType.MainShip);
                }
                else if (CheckFlags(flags1, StatusFlagsShip.Docked))
                {
                    return new UIMode(CheckFlags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIMode.ModeType.MainShipDockedPlanet : UIEvents.UIMode.ModeType.MainShipDockedStarPort, UIMode.MajorModeType.MainShip);
                }
                else if (CheckFlags(flags1, StatusFlagsShip.Landed))
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
