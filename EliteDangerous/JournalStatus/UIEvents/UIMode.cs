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
 *
 */
using System;

namespace EliteDangerousCore.UIEvents
{
    [System.Diagnostics.DebuggerDisplay("Mode {Mode} {MajorMode}")]
    public class UIMode : UIEvent, IEquatable<UIMode>
    {
        public UIMode(DateTime time, bool refresh) : base(UITypeEnum.Mode, time, refresh)
        {
        }

        public UIMode(ModeType type, MajorModeType mode, bool multicrew, bool taxi) : this(DateTime.MinValue,false)
        {
            Mode = type;
            MajorMode = mode;
            Multicrew = multicrew;
            Taxi = taxi;
        }

        public UIMode(ModeType type, MajorModeType mode, bool multicrew, bool taxi, DateTime time, bool refresh) : this(time, refresh)
        {
            Mode = type;
            MajorMode = mode;
            Multicrew = multicrew;
            Taxi = taxi;
        }

        // NOTE webserver reported this in 'Mode' JSON record, and the website checks the names. Be careful changing
        // And Free voice control uses Mode and MajorMode to determine voice set

        public enum ModeType        
        {      
            None,

            MainShipNormalSpace,            
            MainShipDockedStarPort,
            MainShipDockedPlanet,
            MainShipSupercruise,
            MainShipLanded,
            SRV,
            Fighter,
            OnFootStarPortHangar,
            OnFootStarPortSocialSpace,
            OnFootPlantaryPortHangar,
            OnFootPlantaryPortSocialSpace,
            OnFootInstallationInside,
            OnFootPlanet
        };

        // NOTE webserver reported this in 'ShipType' JSON record, and the website checks the names. Be careful changing
        public enum MajorModeType       
        {
            None,
            MainShip,
            SRV,
            Fighter,
            OnFoot, 
        }

        public ModeType Mode { get; private set; }              // detailed mode
        public MajorModeType MajorMode { get; private set; }    // major operating mode

        public bool Taxi { get; private set; }                  // if in taxi, will have MainShip* modes
        public bool Multicrew { get; private set; }             // if playing in multicrew

        public bool InFlight
        {
            get
            {
                return Mode == ModeType.MainShipNormalSpace || Mode == ModeType.MainShipSupercruise;
            }
        }
        public bool OnFoot
        {
            get
            {
                return Mode >= ModeType.OnFootStarPortHangar;
            }
        }

        public override string ToString()
        {
            return $"{MajorMode}, {Mode}, mc {Multicrew}, taxi {Taxi}";
        }

        public bool Equals(UIMode other)
        {
            return Mode == other.Mode && MajorMode == other.MajorMode && Taxi == other.Taxi && Multicrew == other.Multicrew;
        }
    }
}
