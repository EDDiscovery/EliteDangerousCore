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
    public class UIMode : UIEvent
    {
        public UIMode(DateTime time, bool refresh) : base(UITypeEnum.Mode, time, refresh)
        {
        }

        public UIMode(ModeType type, MajorModeType mode) : base(UITypeEnum.Mode, DateTime.MinValue,false)
        {
            Mode = type;
            MajorMode = mode;
        }

        public UIMode(ModeType type, MajorModeType mode, DateTime time, bool refresh) : this(time, refresh)
        {
            Mode = type;
            MajorMode = mode;
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
            TaxiDocked,
            TaxiNormalSpace,
            TaxiSupercruise,
            TaxiDockedPlanet,
            MulticrewDockedStarPort,
            MulticrewDockedPlanet,
            MulticrewNormalSpace,
            MulticrewSupercruise,
            MulticrewLanded,
            MulticrewSRV,
            SRV,
            Fighter,
            OnFootStarPortHangar,
            OnFootStarPortSocialSpace,
            OnFootPlantaryPortHangar,
            OnFootPlantaryPortSocialSpace,
            OnFootInstallationInside,
            OnFootPlanet
        };

        public enum MajorModeType       // NOTE webserver reported this in 'ShipType' JSON record, and the website checks the names. Be careful changing
        {
            None,
            MainShip,
            Taxi,
            Multicrew,
            SRV,
            Fighter,
            OnFoot, 
        }

        
        public ModeType Mode { get; private set; }
        public MajorModeType MajorMode { get; private set; }

        public bool InFlight { get { return Mode == ModeType.MainShipNormalSpace ||
                                            Mode == ModeType.MainShipSupercruise ||
                                            Mode == ModeType.MulticrewNormalSpace ||
                                            Mode == ModeType.MulticrewSupercruise;
                                }}


        public override string ToString()
        {
            return $"{MajorMode}: {Mode}";
        }

    }
}
