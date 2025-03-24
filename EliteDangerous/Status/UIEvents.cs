﻿/*
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

using System;

namespace EliteDangerousCore
{
    public enum UITypeEnum
    {
        GUIFocus = 1,
        Music,
        Pips,
        Position,
        FireGroup,

        Docked, 
        Landed , 
        LandingGear , 
        ShieldsUp , 
        Supercruise , 
        FlightAssist ,
        HardpointsDeployed , 
        InWing ,
        Lights , 
        CargoScoopDeployed , 
        SilentRunning , 
        ScoopingFuel , 
        SrvHandbrake , 
        SrvTurret , 
        SrvUnderShip , 
        SrvDriveAssist , 
        FsdMassLocked , 
        FsdCharging , 
        FsdCooldown , 
        LowFuel , 
        OverHeating , 
        HasLatLong , 
        IsInDanger ,
        BeingInterdicted ,

        HUDInAnalysisMode, //3.3
        NightVision,             // 3.3
        Fuel,   // 3.3
        Cargo,  // 3.3

        LegalStatus, // 3.4

        // EDD Ones

        Mode,              
        OverallStatus,        
        
        // Redirected journal entries
        Command,
        ShipTargeted,
        UnderAttack,
        ReceiveText,
        FSDTarget,

        // Odyssey new

        AimDownSight,
        Oxygen,
        Health,
        Temperature,
        Gravity,
        SelectedWeapon,
        BreathableAtmosphere,
        GlideMode,

        SrvHighBeam,
        FsdJump,

        BodyName,

        NavRouteClear,      // redirected journal entries

        Destination,

        // New March 25

        SupercruiseOverdrive,
        SupercruiseAssist,
        NPCCrewActive,


    }

    public abstract class UIEvent
    {
        public UIEvent(UITypeEnum t, DateTime time, bool refresh)
        {
            EventTypeID = t;
            EventTimeUTC = time;
            EventRefresh = refresh;
        }

        public DateTime EventTimeUTC { get; set; }
        public UITypeEnum EventTypeID { get; set; }             // name of event. 
        public string EventTypeStr { get { return EventTypeID.ToString(); } }
        public bool EventRefresh { get; set; }                  // either at the start or a forced refresh

        static string UIRootClassname = typeof(UIEvents.UIDocked).Namespace;        // pick one at random to find out root classname

        // Flag Factory (others are created individually)

        static public Type TypeOfUIEvent(string name)
        {
            return Type.GetType(UIRootClassname + "." + name, false, true); // no exception, ignore case here
        }

        static public UIEvent CreateEvent(string name, DateTime time, bool refresh, bool? value = null)
        {
            string evname = "UI" + name;
            Type t = Type.GetType(UIRootClassname + "." + evname, false, true);
            if (t != null)
            {
                if ( value != null )
                    return (UIEvent)Activator.CreateInstance(t, new Object[] { value, time, refresh });
                else
                    return (UIEvent)Activator.CreateInstance(t, new Object[] { time, refresh });
            }
            else
                System.Diagnostics.Debug.Assert(true);

            return null;
        }

    }
}
