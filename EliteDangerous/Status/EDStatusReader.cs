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

        long? prev_flags = null;        // force at least one out here by invalid values
        long? prev_flags2 = null;
        int prev_guifocus = NotPresent;                 
        int prev_firegroup = NotPresent;
        double prev_curfuel = NotPresent;
        double prev_curres = NotPresent;
        string prev_legalstatus = null;
        int prev_cargo = NotPresent;
        UIEvents.UIPips.Pips prev_pips = new UIEvents.UIPips.Pips();
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

                    if (text != null && (prev_text == null || !text.Equals(prev_text)))        // if text not null, and prev text is null OR not equal
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

                    if (prev_flags == null)                     // if first run, set prev flags to impossible type.
                    {
                        prev_flags = (long)StatusFlagsShipType.ShipMask;      // set an impossible ship type to start the ball rolling
                        prev_flags2 = 0;
                    }

                    UIEvents.UIShipType.Shiptype shiptype = ShipType(prev_flags.Value, prev_flags2.Value);

                    long curflags = jo["Flags"].Long();
                    long curflags2 = jo["Flags2"].Long(0);      // 0 is backwards compat with horizons

                    bool fireoverall = false;
                    bool fireoverallrefresh = prev_guifocus == NotPresent;     //meaning its a refresh

                    long flagsdelta2 = 0;                       // what changed between prev and current

                    if (curflags != prev_flags.Value || curflags2 != prev_flags2.Value)
                    {
                        UIEvents.UIShipType.Shiptype nextshiptype = ShipType(curflags, curflags2);

                        //System.Diagnostics.Debug.WriteLine("UI Flags changed {0} {1} {2} -> {3} {4} {5}", prev_flags.Value, prev_flags2.Value, shiptype, curflags, curflags2, nextshiptype);

                        bool refresh = shiptype == UIEvents.UIShipType.Shiptype.None;   // refresh if prev ship was none..

                        if (shiptype != nextshiptype)
                        {
                            events.Add(new UIEvents.UIShipType(nextshiptype, EventTimeUTC, refresh));        // CHANGE of ship/on foot/taxi etc
                            //prev_flags = ~curflags;       // force re-reporting (don't think its ness)
                            //prev_flags2 = ~curflags2;
                            refresh = true;
                        }

                        if (nextshiptype != UIEvents.UIShipType.Shiptype.None)
                        { 
                            events.AddRange(ReportFlagState(typeof(StatusFlags2), curflags2, prev_flags2.Value, EventTimeUTC, refresh));
                            events.AddRange(ReportFlagState(typeof(StatusFlagsShip), curflags, prev_flags.Value, EventTimeUTC, refresh));
                            events.AddRange(ReportFlagState(typeof(StatusFlagsSRV), curflags, prev_flags.Value, EventTimeUTC, refresh));
                            events.AddRange(ReportFlagState(typeof(StatusFlagsAll), curflags, prev_flags.Value, EventTimeUTC, refresh));
                        }

                        flagsdelta2 = curflags2 ^ prev_flags2.Value;        // record the delta here for later processing, some of those flags go into the main reports

                        prev_flags = curflags;
                        prev_flags2 = curflags2;
                        shiptype = nextshiptype;
                        fireoverall = true;
                    }

                    int curguifocus = jo["GuiFocus"].Int(NotPresent);
                    if (curguifocus != prev_guifocus)
                    {
                        events.Add(new UIEvents.UIGUIFocus(curguifocus, EventTimeUTC, prev_guifocus == NotPresent));
                        prev_guifocus = curguifocus;
                        fireoverall = true;
                    }

                    int[] pips = jo["Pips"]?.ToObjectQ<int[]>();

                    if (pips != null)
                    {
                        double sys = pips[0] / 2.0;     // convert to normal, instead of half pips
                        double eng = pips[1] / 2.0;
                        double wep = pips[2] / 2.0;
                        if (sys != prev_pips.Systems || wep != prev_pips.Weapons || eng != prev_pips.Engines)
                        {
                            UIEvents.UIPips.Pips newpips = new UIEvents.UIPips.Pips() { Systems = sys, Engines = eng, Weapons = wep };
                            events.Add(new UIEvents.UIPips(newpips, EventTimeUTC, prev_pips.Engines < 0));
                            prev_pips = newpips;
                            fireoverall = true;
                        }
                    }
                    else if ( prev_pips.Valid )     // missing pips, if we are valid.. need to clear them
                    {
                        UIEvents.UIPips.Pips newpips = new UIEvents.UIPips.Pips();
                        events.Add(new UIEvents.UIPips(newpips, EventTimeUTC, prev_pips.Engines < 0));
                        prev_pips = newpips;
                        fireoverall = true;
                    }

                    int? curfiregroup = jo["FireGroup"].IntNull();      // may appear/disappear.

                    if (curfiregroup != null && curfiregroup != prev_firegroup)
                    {
                        events.Add(new UIEvents.UIFireGroup(curfiregroup.Value + 1, EventTimeUTC, prev_firegroup == NotPresent));
                        prev_firegroup = curfiregroup.Value;
                        fireoverall = true;
                    }

                    JToken jfuel = jo["Fuel"];

                    if (jfuel != null && jfuel.IsObject)        // because they changed its type in 3.3.2
                    {
                        double? curfuel = jfuel["FuelMain"].DoubleNull();
                        double? curres = jfuel["FuelReservoir"].DoubleNull();
                        if (curfuel != null && curres != null)
                        {
                            if (Math.Abs(curfuel.Value - prev_curfuel) >= 0.1 || Math.Abs(curres.Value - prev_curres) >= 0.01)  // don't fire if small changes
                            {
                                //System.Diagnostics.Debug.WriteLine("UIEvent Fuel " + curfuel.Value + " " + prev_curfuel + " Res " + curres.Value + " " + prev_curres);
                                events.Add(new UIEvents.UIFuel(curfuel.Value, curres.Value, shiptype, EventTimeUTC, prev_firegroup == NotPresent));
                                prev_curfuel = curfuel.Value;
                                prev_curres = curres.Value;
                                fireoverall = true;
                            }
                        }
                    }

                    int? curcargo = jo["Cargo"].IntNull();      // may appear/disappear and only introduced for 3.3
                    if (curcargo != null && curcargo.Value != prev_cargo)
                    {
                        events.Add(new UIEvents.UICargo(curcargo.Value, shiptype, EventTimeUTC, prev_firegroup == NotPresent));
                        prev_cargo = curcargo.Value;
                        fireoverall = true;
                    }

                    double jlat = jo["Latitude"].Double(UIEvents.UIPosition.InvalidValue);       // if not there, min value
                    double jlon = jo["Longitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jalt = jo["Altitude"].Double(UIEvents.UIPosition.InvalidValue);
                    double jheading = jo["Heading"].Double(UIEvents.UIPosition.InvalidValue);
                    double jpradius = jo["PlanetRadius"].Double(UIEvents.UIPosition.InvalidValue);       // 3.4

                    if (jlat != prev_pos.Latitude || jlon != prev_pos.Longitude || jalt != prev_pos.Altitude || jheading != prev_heading || jpradius != prev_jpradius)
                    {
                        UIEvents.UIPosition.Position newpos = new UIEvents.UIPosition.Position()
                        {
                            Latitude = jlat, Longitude = jlon,
                            Altitude = jalt, AltitudeFromAverageRadius = Flags(curflags, StatusFlagsReportedInOtherEvents.AltitudeFromAverageRadius)
                        };

                        events.Add(new UIEvents.UIPosition(newpos, jheading, jpradius, EventTimeUTC, prev_pos.ValidPosition == false));
                        prev_pos = newpos;
                        prev_heading = jheading;
                        prev_jpradius = jpradius;
                        fireoverall = true;
                    }

                    string cur_legalstatus = jo["LegalState"].StrNull();

                    if (cur_legalstatus != prev_legalstatus)
                    {
                        events.Add(new UIEvents.UILegalStatus(cur_legalstatus, EventTimeUTC, prev_legalstatus == null));
                        prev_legalstatus = cur_legalstatus;
                        fireoverall = true;
                    }

                    string cur_bodyname = jo["BodyName"].StrNull();

                    if (cur_bodyname != prev_bodyname)
                    {
                        events.Add(new UIEvents.UIBodyName(cur_bodyname, EventTimeUTC, prev_bodyname == null));
                        prev_bodyname = cur_bodyname;
                        fireoverall = true;
                    }

                    string cur_weapon = jo["SelectedWeapon"].StrNull();                 // null if not there
                    string cur_weaponloc = jo["SelectedWeapon_Localised"].Str();        // empty if not there

                    if (cur_weapon != prev_selectedweapon)
                    {
                        events.Add(new UIEvents.UISelectedWeapon(cur_weapon, cur_weaponloc, EventTimeUTC, prev_selectedweapon == null));
                        prev_selectedweapon = cur_weapon;
                        prev_selectedweaponloc = cur_weaponloc;
                        fireoverall = true;
                    }

                    double oxygen = jo["Oxygen"].Double(NotPresent);                //-1 is not present
                    oxygen = oxygen < 0 ? oxygen : oxygen * 100;                    // correct to 0-100%
                    bool lowoxygen = Flags(curflags2, StatusFlags2OtherFlags.LowOxygen);

                    if (oxygen != prev_oxygen || Flags(flagsdelta2,StatusFlags2OtherFlags.LowOxygen))
                    {
                        events.Add(new UIEvents.UIOxygen(oxygen, lowoxygen , EventTimeUTC, prev_oxygen < 0));
                        prev_oxygen = oxygen;
                        fireoverall = true;
                    }

                    double health = jo["Health"].Double(NotPresent);                //-1 is not present
                    health = health < 0 ? health : health * 100;                    // correct to 0-100%
                    bool lowhealth = Flags(curflags2, StatusFlags2OtherFlags.LowHealth);

                    if (health != prev_health || Flags(flagsdelta2,StatusFlags2OtherFlags.LowHealth))
                    {
                        events.Add(new UIEvents.UIHealth(health, lowhealth, EventTimeUTC, prev_health < 0));
                        prev_health = health;
                        fireoverall = true;
                    }

                    double gravity = jo["Gravity"].Double(NotPresent);                //-1 is not present

                    if (gravity != prev_gravity )
                    {
                        events.Add(new UIEvents.UIGravity(gravity, EventTimeUTC, prev_gravity < 0));
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

                    if (temperature != prev_temperature || (flagsdelta2 & (long)StatusFlags2OtherFlags.TempBits) != 0)
                    {

                        events.Add(new UIEvents.UITemperature(temperature,tempstate, EventTimeUTC, prev_temperature < 0));
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
                        if (Flags(curflags, StatusFlagsShip.FsdJump))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Jumping;
                        else if (Flags(curflags,StatusFlagsShip.FsdCharging))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Charging;
                        else if (Flags(curflags2,StatusFlags2.GlideMode))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Gliding;
                        else if (Flags(curflags,StatusFlagsShip.FsdCooldown))
                            fsdstate = UIEvents.UIOverallStatus.FSDStateType.Cooldown;

                        events.Add(new UIEvents.UIOverallStatus(shiptype, flagsset, prev_guifocus, prev_pips, prev_firegroup, 
                                                                prev_curfuel,prev_curres, prev_cargo, prev_pos, prev_heading, prev_jpradius, prev_legalstatus, 
                                                                prev_bodyname,
                                                                prev_health,lowhealth,gravity,prev_temperature,tempstate,prev_oxygen,lowoxygen,
                                                                prev_selectedweapon, prev_selectedweaponloc,
                                                                fsdstate, breathableatmos,
                                                                EventTimeUTC, fireoverallrefresh));        // overall list of flags set
                    }

                    //for debugging, keep
#if false
                    foreach (var uient in events)
                        {
                            BaseUtils.Variables v = new BaseUtils.Variables();
                            v.AddPropertiesFieldsOfClass(uient, "", null, 2);
                            System.Diagnostics.Trace.WriteLine(string.Format("New UI entry from journal {0} {1}", uient.EventTimeUTC, uient.EventTypeStr));
                            foreach (var x in v.NameEnumuerable)
                                System.Diagnostics.Trace.WriteLine(string.Format("  {0} = {1}", x, v[x]));
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

        List<UIEvent> ReportFlagState(Type enumtype, long curflags, long prev_flags, DateTime EventTimeUTC, bool refresh)
        {
            List<UIEvent> events = new List<UIEvent>();
            long delta = curflags ^ prev_flags;

            //System.Diagnostics.Debug.WriteLine("Flags changed to {0:x} from {1:x} delta {2:x}", curflags, prev_flags , delta);

            foreach (string n in Enum.GetNames(enumtype))
            {
                int v = (int)Enum.Parse(enumtype, n);

                bool flag = ((curflags >> v) & 1) != 0;

                if (((delta >> v) & 1) != 0)
                {
                   // System.Diagnostics.Debug.WriteLine("..Flag " + n + " at " +v +" changed to " + flag);
                    var e = UIEvent.CreateEvent(n, EventTimeUTC, refresh, flag);
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


        static private UIEvents.UIShipType.Shiptype ShipType(long flags1, long flags2)
        {
            if (Flags(flags2, StatusFlags2ShipType.InMulticrew))
            {
                if (Flags(flags1, StatusFlagsShipType.InSRV))
                    return UIEvents.UIShipType.Shiptype.MulticrewSRV;
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                    return UIEvents.UIShipType.Shiptype.MulticrewSupercruise;
                if (Flags(flags1, StatusFlagsShip.Docked))
                    return Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIShipType.Shiptype.MulticrewDockedPlanet : UIEvents.UIShipType.Shiptype.MulticrewDockedStarPort;
                if (Flags(flags1, StatusFlagsShip.Landed))
                    return UIEvents.UIShipType.Shiptype.MulticrewLanded;
                return UIEvents.UIShipType.Shiptype.MulticrewNormalSpace;
            }
            else if (Flags(flags2, StatusFlags2ShipType.InTaxi))
            {
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                    return UIEvents.UIShipType.Shiptype.TaxiSupercruise;
                if (Flags(flags1, StatusFlagsShip.Docked))
                    return Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIShipType.Shiptype.TaxiDockedPlanet : UIEvents.UIShipType.Shiptype.TaxiDocked;
                return UIEvents.UIShipType.Shiptype.TaxiNormalSpace;
            }
            else if (Flags(flags1, StatusFlagsShipType.InFighter))
            {
                return UIEvents.UIShipType.Shiptype.Fighter;
            }
            else if (Flags(flags1, StatusFlagsShipType.InSRV))
            {
                return UIEvents.UIShipType.Shiptype.SRV;
            }
            else if (Flags(flags2, StatusFlags2ShipType.OnFoot))
            {
                if (Flags(flags2, StatusFlags2ShipType.OnFootInStation))        // station means starport
                {
                    return Flags(flags2, StatusFlags2ShipType.OnFootInHangar) ? UIEvents.UIShipType.Shiptype.OnFootStarPortHangar : UIEvents.UIShipType.Shiptype.OnFootStarPortSocialSpace;
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootInHangar))        // if set, but no station, its a planetary port
                {
                    return UIEvents.UIShipType.Shiptype.OnFootPlantaryPortHangar;
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootInSocialSpace))
                {
                    return UIEvents.UIShipType.Shiptype.OnFootPlantaryPortSocialSpace;
                }
                else if (Flags(flags2, StatusFlags2ShipType.OnFootOnPlanet))
                {
                    return Flags(flags2, StatusFlags2.BreathableAtmosphere) ? UIEvents.UIShipType.Shiptype.OnFootInstallationInside : UIEvents.UIShipType.Shiptype.OnFootPlanet;
                }
            }
            else if(Flags(flags1, StatusFlagsShipType.InMainShip))
            {
                if (Flags(flags1, StatusFlagsShip.Supercruise))
                {
                    return UIEvents.UIShipType.Shiptype.MainShipSupercruise;
                }
                if (Flags(flags1, StatusFlagsShip.Docked))
                {
                    return Flags(flags1, StatusFlagsAll.HasLatLong) ? UIEvents.UIShipType.Shiptype.MainShipDockedPlanet : UIEvents.UIShipType.Shiptype.MainShipDockedStarPort;
                }
                if (Flags(flags1, StatusFlagsShip.Landed))
                {
                    return UIEvents.UIShipType.Shiptype.MainShipLanded;
                }
                return UIEvents.UIShipType.Shiptype.MainShipNormalSpace;
            }

            return UIEvents.UIShipType.Shiptype.None;
        }


    }
}
