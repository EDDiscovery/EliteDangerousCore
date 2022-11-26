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
    public class UIFuel : UIEvent
    {
        public UIFuel(DateTime time, bool refresh) : base(UITypeEnum.Fuel, time, refresh)
        {
        }

        public UIFuel(double value, double res, UIMode.ModeType shiptype, bool scooping, DateTime time, bool refresh) : this( time, refresh)
        {
            Fuel = value;
            FuelRes = res;
            Mode = shiptype;
            Scooping = scooping;
        }

        public double Fuel { get; private set; }     // level,
        public double FuelRes { get; private set; }     // level 3.3.2
        public bool Scooping { get; private set; }     // scooping

        public bool Valid { get { return Fuel >= 0; } }

        public UIMode.ModeType Mode { get; private set; }   // Ship type flags.. per flags

        public override string ToString()
        {
            return $"{Fuel:N2} res {FuelRes:N2} scooping {Scooping}";
        }
    }
}
