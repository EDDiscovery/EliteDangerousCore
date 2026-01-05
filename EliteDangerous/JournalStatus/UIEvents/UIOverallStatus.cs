/*
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
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EliteDangerousCore.UIEvents
{
    public class UIOverallStatus : UIEvent
    {
        public UIOverallStatus( DateTime time, bool refresh) : base(UITypeEnum.OverallStatus, time, refresh)
        {
            Flags = new List<UITypeEnum>();     // empty list
        }

        public UIOverallStatus() : this(DateTime.UtcNow,false)
        {
        }

        public UIOverallStatus(UIMode uim, List<UITypeEnum> list, int focus, UIPips.Pips pips, int fg, double fuel, double res, int cargo,
            UIPosition.Position pos, double heading, double radius, string legalstate, string bodyname,
            double health, bool lowh, double gravity, double temp, UITemperature.TempState tempstate, double oxygen, bool lowox,
            string selw, string selwloc, 
            FSDStateType fsd, bool breathableatmosphere,
            string dname, string dnameloc, int? did, long? dsysaddr,
            DateTime time, bool refresh) : this(time, refresh)
        {
            UIMode = uim;
            Flags = list;
            Focus = (UIGUIFocus.Focus)focus;
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
            FSDState = fsd;
            BreathableAtmosphere = breathableatmosphere;
            SelectedWeapon = selw;
            SelectedWeapon_Localised = selwloc;
            HandItem = SelectedWeapon != null ? ItemData.GetWeaponOrHandItem(SelectedWeapon) : null;
            DestinationName = dname;
            DestinationName_Localised = dnameloc;
            DestinationBodyID = did;
            DestinationSystemAddress = dsysaddr;
        }

        public UIMode UIMode { get; private set; } = new UIMode(DateTime.MinValue, false);
        public UIMode.MajorModeType MajorMode { get { return UIMode.MajorMode; } }
        public UIMode.ModeType Mode { get { return UIMode.Mode; } }
        public bool Multicrew { get { return UIMode.Multicrew; } }
        public bool Taxi { get { return UIMode.Taxi; } }
        public List<UITypeEnum> Flags { get; private set; }
        public UIGUIFocus.Focus Focus { get; private set; }
        public UIPips.Pips Pips { get; private set; }
        public int Firegroup { get; private set; }
        public double Fuel { get; private set; }
        public double Reserve { get; private set; }
        public int Cargo { get; private set; }
        public UIPosition.Position Pos { get; private set; }
        public bool ValidHeading { get { return Heading != UIPosition.InvalidValue; } }
        public double Heading { get; private set; }
        public bool ValidRadius { get { return PlanetRadius != UIPosition.InvalidValue; } }
        public double PlanetRadius { get; private set; }
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
        public bool BreathableAtmosphere { get; private set; }
        public enum FSDStateType { Normal, Charging, Jumping, Gliding, Cooldown , MassLock, ChargingFSDFlagSet};
        public FSDStateType FSDState { get; private set; }
        public string SelectedWeapon { get; private set; }      // may be null
        public string SelectedWeapon_Localised { get; private set; }      // may be null
        public ItemData.HandItem HandItem { get; private set; } // Only set for weapons, and may be null even if SelectedWeapon set if unknown
        public string DestinationName { get; private set; }      // may be null
        public string DestinationName_Localised { get; private set; }      // only if name needs localisation
        public int? DestinationBodyID { get; private set; }      // may be null
        public long? DestinationSystemAddress { get; private set; } // may be null

        public override string ToString()
        {
            return $"MM: {MajorMode} M:{Mode} Body:{BodyName} FC:{Focus} Pips:{Pips} FG:{Firegroup} Fuel:{Fuel}/{Reserve} Cargo:{Cargo} Pos {Pos} Head:{Heading} Legal:{LegalState} Health:{Health} Gravity:{Gravity} Temp:{Temperature} FSD:{FSDState} Dest:{DestinationName} SelW:{SelectedWeapon}/{SelectedWeapon_Localised}/{HandItem?.Name}";
        }

    }
}
