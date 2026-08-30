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
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public partial class BindingsFile 
    {
        // best match of device name from Devices to a device given by name/guid,usb ids
        public string FindDevice(string name, Guid instanceguid, Guid productguid, int productid, int vendorid)
        {
            string bestmatch = null;
            int besttotal = 0;

            // try the frontier device naming table
            string frontiername = usbvendorproductidtofrontier.ContainsKey(new Tuple<int, int>(productid, vendorid)) ? usbvendorproductidtofrontier[new Tuple<int, int>(productid, vendorid)] : null;

            foreach (string dv in devices)
            {
                if (dv.Equals(name, StringComparison.InvariantCultureIgnoreCase))      // exact match
                    return dv;

                if (frontiername != null && dv.Equals(frontiername, StringComparison.InvariantCultureIgnoreCase))
                    return dv;

                if (dv.Equals(GuidExtract(instanceguid, false), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(instanceguid, true), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(productguid, false), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(productguid, true), StringComparison.InvariantCultureIgnoreCase))
                    return dv;

                int total = dv.SplitCapsWord().ToLowerInvariant().ApproxMatch(name.ToLowerInvariant(), 4);
                if (total > besttotal)
                {
                    besttotal = total;
                    bestmatch = dv;
                }
            }

            if (bestmatch != null)
                return bestmatch;

            return null;
        }

        // The frontier table, what is its name if its there, or null
        public static string FrontierDeviceName(int productid, int vendorid)
        {
            return usbvendorproductidtofrontier.TryGetValue(Tuple.Create(productid, vendorid), out string name) ? name : null;
        }

        private string GuidExtract(Guid g, bool rev)
        {
            string s = g.ToString();
            int slash = s.IndexOf('-');
            if (slash >= 0)
                s = s.Substring(0, slash);

            if (rev && s.Length == 8)
            {
                s = s.Substring(4, 4) + s.Substring(0, 4);
            }

            return s;
        }

        // From frontier DeviceMapping.xml table using EDDTest devicemappings import, 8/4/22
        private static Dictionary<Tuple<int, int>, string> usbvendorproductidtofrontier = new Dictionary<Tuple<int, int>, string>()
        {
        {  new Tuple<int,int>(0x28E, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x28F, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x2FF, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x5D04, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x2A1, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x4716, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x213, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x291, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x719, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0xF07, 0x44F), "GamePad" },
        {  new Tuple<int,int>(0xB326, 0x44F), "GamePad" },
        {  new Tuple<int,int>(0xC21D, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC21E, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC21F, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC242, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xCA84, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0x4540, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4556, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4718, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4726, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4728, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4738, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4740, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x6040, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xB726, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xBEEF, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xCB02, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xCB03, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xF738, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x8802, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x8809, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x880A, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x5, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x6, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x105, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x113, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x201, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x21F, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x301, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x401, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x201, 0xE8F), "GamePad" },
        {  new Tuple<int,int>(0x3008, 0xE8F), "GamePad" },
        {  new Tuple<int,int>(0xA, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0xD, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0x16, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0x202, 0xF30), "GamePad" },
        {  new Tuple<int,int>(0x8888, 0xF30), "GamePad" },
        {  new Tuple<int,int>(0xFF0C, 0x102C), "GamePad" },
        {  new Tuple<int,int>(0x4, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x301, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x8809, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x4748, 0x1430), "GamePad" },
        {  new Tuple<int,int>(0x8888, 0x1430), "GamePad" },
        {  new Tuple<int,int>(0x601, 0x146B), "GamePad" },
        {  new Tuple<int,int>(0x37, 0x1532), "GamePad" },
        {  new Tuple<int,int>(0x3F00, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0x3F0A, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0x3F10, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0xBEEF, 0x162E), "GamePad" },
        {  new Tuple<int,int>(0xFD00, 0x1689), "GamePad" },
        {  new Tuple<int,int>(0xFD01, 0x1689), "GamePad" },
        {  new Tuple<int,int>(0x2, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0x3, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF016, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF023, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF028, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF038, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF900, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF901, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF903, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0x5000, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5300, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5303, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5500, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5501, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5506, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5B02, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x2D1, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x317, 0x7B5), "BlackWidow" },
        {  new Tuple<int,int>(0xC28, 0x6A3), "SaitekAV8R03" },
        {  new Tuple<int,int>(0x461, 0x6A3), "SaitekAV8R03" },
        {  new Tuple<int,int>(0x2215, 0x738), "SaitekX55Joystick" },
        {  new Tuple<int,int>(0x2221, 0x738), "SaitekX56Joystick" },
        {  new Tuple<int,int>(0xA215, 0x738), "SaitekX55Throttle" },
        {  new Tuple<int,int>(0xA221, 0x738), "SaitekX56Throttle" },
        {  new Tuple<int,int>(0x762, 0x6A3), "SaitekX52Pro" },
        {  new Tuple<int,int>(0x75C, 0x6A3), "SaitekX52" },
        {  new Tuple<int,int>(0x255, 0x6A3), "SaitekX52" },
        {  new Tuple<int,int>(0xC2AA, 0x46D), "LogitechG940Pedals" },
        {  new Tuple<int,int>(0xC2A8, 0x46D), "LogitechG940Joystick" },
        {  new Tuple<int,int>(0xC2A9, 0x46D), "LogitechG940Throttle" },
        {  new Tuple<int,int>(0x402, 0x44F), "ThrustMasterWarthogJoystick" },
        {  new Tuple<int,int>(0x404, 0x44F), "ThrustMasterWarthogThrottle" },
        {  new Tuple<int,int>(0xFFFF, 0x44F), "ThrustMasterWarthogCombined" },
        {  new Tuple<int,int>(0x1302, 0x738), "SaitekFLY5" },
        {  new Tuple<int,int>(0xB108, 0x44F), "ThrustMasterTFlightHOTASX" },
        {  new Tuple<int,int>(0xB67C, 0x44F), "ThrustMasterHOTAS4" },
        {  new Tuple<int,int>(0xB67B, 0x44F), "ThrustMasterHOTAS4" },
        {  new Tuple<int,int>(0xB68D, 0x44F), "TFlightHotasOne" },
        {  new Tuple<int,int>(0xB10A, 0x44F), "T16000M" },
        {  new Tuple<int,int>(0xB687, 0x44F), "T16000MTHROTTLE" },
        {  new Tuple<int,int>(0xB679, 0x44F), "T-Rudder" },
        //{  new Tuple<int,int>(0xFFFF, 0x44F), "TMwTARGET" },  // removed duplicate
        {  new Tuple<int,int>(0xC215, 0x46D), "LogitechExtreme3DPro" },
        {  new Tuple<int,int>(0xC121, 0x25F0), "GioteckPS3WiredController" },
        {  new Tuple<int,int>(0x53C, 0x6A3), "SaitekX45" },
        {  new Tuple<int,int>(0x8037, 0x2341), "EDTracker" },
        {  new Tuple<int,int>(0xC219, 0x46D), "Logitech710WirelessGamepad" },
        {  new Tuple<int,int>(0xC285, 0x46D), "LogitechWingManStrikeForce3D" },
        {  new Tuple<int,int>(0x5F0D, 0x6A3), "SaitekP2600RumbleForce" },
        {  new Tuple<int,int>(0xFF0C, 0x6A3), "SaitekP2500RumbleForce" },
        {  new Tuple<int,int>(0x353E, 0x6A3), "SaitekCyborgEvoWireless" },
        {  new Tuple<int,int>(0x764, 0x6A3), "SaitekProFlightCombatRudderPedals" },
        {  new Tuple<int,int>(0x763, 0x6A3), "SaitekProFlightRudderPedals" },
        {  new Tuple<int,int>(0xBEAD, 0x1234), "vJoy" },
        {  new Tuple<int,int>(0xF1, 0x68E), "CHProThrottle1" },
        {  new Tuple<int,int>(0xC0F1, 0x68E), "CHProThrottle2" },
        {  new Tuple<int,int>(0xF3, 0x68E), "CHFighterStick" },
        {  new Tuple<int,int>(0xC0F2, 0x68E), "CHProPedals" },
        {  new Tuple<int,int>(0xC0F4, 0x68E), "CHCombatStick" },
        {  new Tuple<int,int>(0xF4, 0x68E), "CHCombatStick" },
        {  new Tuple<int,int>(0x3, 0xE8F), "TrustPredator" },
        {  new Tuple<int,int>(0x268, 0x54C), "Playstation3Controller" },
        {  new Tuple<int,int>(0x460, 0x6A3), "SaitekST290Pro" },
        {  new Tuple<int,int>(0x3001, 0x47D), "XterminatorDualControl" },
        {  new Tuple<int,int>(0x1B, 0x45E), "SideWinderForceFeedback2" },
        {  new Tuple<int,int>(0x8036, 0x2341), "ArduinoLeonardo" },
        {  new Tuple<int,int>(0x11, 0x4D8), "SlawFlightControlRudder" },
        {  new Tuple<int,int>(0x211, 0x2833), "OculusTouch" },
        {  new Tuple<int,int>(0xBA0, 0x54C), "DualShock4" },
        {  new Tuple<int,int>(0x5C4, 0x54C), "DualShock4" },
        {  new Tuple<int,int>(0x9CC, 0x54C), "DualShock4" },

        };



    }
}
