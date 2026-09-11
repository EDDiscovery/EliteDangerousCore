/*
 * Copyright 2016-2026 EDDiscovery development team
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
using System.Collections;
using System.Collections.Generic;

namespace EliteDangerousCore.Bindings
{
    [System.Diagnostics.DebuggerDisplay("{FrontierName} {BetterName} : {AxisList} {Pov} {Buttons} K{Keyboard} M{Mouse}")]
    public class Device : IEquatable<Device>, IEqualityComparer<Device>
    {
        public Device(string name, string bestname, string[] axis, int pov, int buttons)
        {
            FrontierName = name;
            BetterName = bestname;
            Axis = axis;
            Pov = pov;
            Buttons = buttons;
        }

        public Device()
        {
            FrontierName = "{NoDevice}";
            BetterName = "---";
        }

        public string AxisList => Axis != null ? string.Join(", ", Axis) : "";

        public bool HasBetterName => BetterName != FrontierName;

        // these are fixed names at this level, the other names have to be handled at bindingfile level
        public bool IsNoDevice => FrontierName == NoDeviceName;
        public bool IsDevice => FrontierName != NoDeviceName;
        public bool IsKeyboard => FrontierName == KeyboardDeviceName;
        public bool IsMouse => FrontierName == MouseDeviceName;
        public bool IsJoystick => !IsNoDevice && !IsKeyboard && !IsMouse;

        public const string KeyboardDeviceName = "Keyboard";
        public const string MouseDeviceName = "Mouse";
        public const string NoDeviceName = "{NoDevice}";

        static public string[] DefaultAxis = new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "U", "V" };

        public string FrontierName { get; set; }
        public string BetterName { get; set; }
        public string[] Axis { get; set; }      // may be null
        public int Pov { get; set; }
        public int Buttons { get; set; }
        public bool Mouse => FrontierName == MouseDeviceName;
        public bool Keyboard => FrontierName == KeyboardDeviceName;

        public bool Equals(Device other)
        {
            return other.FrontierName == FrontierName;
        }

        public bool Equals(Device x, Device y)
        {
            return x == null && y == null ? true : x == null || y == null ? false : x.FrontierName == y.FrontierName;
        }

        public int GetHashCode(Device obj)
        {
            return obj.FrontierName.GetHashCode();
        }
    }
}
