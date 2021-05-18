/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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
using System;
using System.Collections.Generic;

namespace EliteDangerousCore.UIEvents
{
    public class UIOverallStatus : UIEvent
    {
        public UIOverallStatus( DateTime time, bool refresh) : base(UITypeEnum.OverallStatus, time, refresh)
        {

        }

        public UIOverallStatus(UIEvents.UIShipType.Shiptype st, List<UITypeEnum> list, int focus, UIEvents.UIPips.Pips pips, int fg, double fuel, double res, int cargo,
            UIEvents.UIPosition.Position pos, double heading, double radius, string legalstate, string bodyname,
            double health, bool lowh, double gravity, double temp, UITemperature.TempState tempstate, double oxygen, bool lowox,
            string selw, string selwloc,
            bool glide, bool breathableatmosphere,
            DateTime time, bool refresh) : this(time, refresh)
        {
            ShipType = st;
            Flags = list;
            Focus = (UIEvents.UIGUIFocus.Focus)focus;
            Pips = pips;
            Firegroup = fg;
            Fuel = fuel;
            Reserve = res;
            Cargo = cargo;
            Pos = pos;
            Heading = heading;
            PlanetRadius = radius;
            LegalState = legalstate;
            BodyName = bodyname;
            Health = health;
            LowHealth = lowh;
            Gravity = gravity;
            Temperature = temp;
            TemperatureState = tempstate;
            Oxygen = oxygen;
            LowOxygen = lowox;
            GlideMode = glide;
            BreathableAtmosphere = breathableatmosphere;
            SelectedWeapon = selw;
            SelectedWeapon_Localised = selwloc;
        }

        public UIEvents.UIShipType.Shiptype ShipType { get; private set; }
        public List<UITypeEnum> Flags { get; private set; }
        public UIEvents.UIGUIFocus.Focus Focus { get; private set; }
        public UIEvents.UIPips.Pips Pips { get; private set; }
        public int Firegroup { get; private set; }
        public double Fuel { get; private set; }
        public double Reserve { get; private set; }
        public int Cargo { get; private set; }
        public UIEvents.UIPosition.Position Pos { get; private set; }
        public double Heading { get; private set; }
        public bool ValidHeading { get { return Heading != UIEvents.UIPosition.InvalidValue; } }
        public double PlanetRadius { get; private set; }
        public bool ValidRadius { get { return PlanetRadius != UIEvents.UIPosition.InvalidValue; } }
        public string LegalState { get; private set; }      // may be null
        public string BodyName { get; private set; }      // may be null
        // odyssey
        public double Health { get; private set; }
        public bool LowHealth { get; private set; }
        public double Gravity { get; private set; }
        public double Temperature { get; private set; }
        public UITemperature.TempState TemperatureState { get; private set; }
        public double Oxygen { get; private set; }
        public bool LowOxygen { get; private set; }
        public bool GlideMode { get; private set; }
        public bool BreathableAtmosphere { get; private set; }
        public string SelectedWeapon { get; private set; }      // may be null
        public string SelectedWeapon_Localised { get; private set; }      // may be null

    }
}
