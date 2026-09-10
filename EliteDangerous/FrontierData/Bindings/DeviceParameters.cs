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

namespace EliteDangerousCore.Bindings
{
    [System.Diagnostics.DebuggerDisplay("{FrontierName} {BetterName} : {AxisList} {Pov} {Buttons} K{Keyboard} M{Mouse}")]
    public class DeviceParameters
    {
        public DeviceParameters(string name, string bestname, string[] axis, int pov, int buttons)
        {
            FrontierName = name;
            BetterName = bestname;
            Axis = axis;
            Pov = pov;
            Buttons = buttons;
        }
        public DeviceParameters(string name, bool keyboard, bool mouse)
        {
            BetterName = FrontierName = name;
            Mouse = mouse;
            Keyboard = keyboard;
        }
        public DeviceParameters(string bettername)
        {
            FrontierName = "{NoDevice}";
            BetterName = bettername;
        }

        public string AxisList => Axis != null ? string.Join(", ", Axis) : "";

        public bool IsNoDevice => FrontierName == "{NoDevice}";

        static public string[] DefaultAxis = new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "U", "V" };

        public string FrontierName { get; set; }
        public string BetterName { get; set; }
        public string[] Axis { get; set; }
        public int Pov { get; set; }
        public int Buttons { get; set; }
        public bool Mouse { get; set; }
        public bool Keyboard { get; set; }
    }
}
