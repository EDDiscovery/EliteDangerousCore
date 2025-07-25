﻿/*
 * Copyright © 2018-2023 EDDiscovery development team
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
    [System.Diagnostics.DebuggerDisplay("{Slot} {Item} {LocalisedItem}")]
    public class ShipModule
    {
        #region Information interface

        public string Slot { get; private set; }        // never null       - english name
        public ShipSlots.Slot SlotFD { get; private set; }    // never null    
        public string Item { get; private set; }        // never null       - nice name, used to track, english
        public string ItemFD { get; private set; }      // never null     - FD normalised ID name
        public string LocalisedItem { get; set; }       // Modulex events only supply this. so it may be null if we have not seen one of them pass by with this Item name

        public bool? Enabled { get; private set; }      // Loadout events, may be null
        public int? Priority { get; private set; }      // 0..4 not 1..5
        public int? Health { get; private set; }        //0-100
        public long? Value { get; private set; }

        public int? AmmoClip { get; private set; }      // from loadout event
        public int? AmmoHopper { get; private set; }

        public double? Power { get; private set; }      // ONLY via Modules Info

        public EngineeringData Engineering { get; private set; }       // may be NULL if module is not engineered or unknown
               
        public string PE()
        {
            string pe = "";
            if (Priority.HasValue)
                pe = "P" + (Priority.Value + 1).ToString();
            if (Enabled.HasValue)
                pe += Enabled.Value ? "E" : "D";

            return pe;
        }

        public bool IsFSDSlot { get { return SlotFD  == ShipSlots.Slot.FrameShiftDrive; } }

        // will return null if unknown module
        public ItemData.ShipModule GetModuleUnengineered()
        {
            return ItemData.TryGetShipModule(ItemFD, out ItemData.ShipModule sm, false) ? sm : null;
        }

        // take the unengineered module data and engineer it with the Engineering data
        // engineered may be null if we don't know the module
        // report will be filled if any errors occur
        public ItemData.ShipModule GetModuleEngineered(out string report, bool debugit = false)
        {
            report = "";

            var mdu = GetModuleUnengineered();

            if (mdu != null)        // recognised module
            {
                if (Engineering != null)    // has engineering
                {
                    return Engineering.EngineerModule(mdu, out report, ItemFD, SlotFD, debugit);
                }
                else
                {
                    return mdu;
                }
            }
            else
                return null;
        }

        public double Mass()
        {
            var unmass = GetModuleUnengineered();
            if (unmass?.Mass != null)
            {
                return GetModuleEngineered(out string _).Mass.Value;       
            }
            else
                return 0;
        }

        public bool Same(ShipModule other)      // ignore localisased item, it does not occur everywhere..
        {
            bool engsame = Engineering != null ? Engineering.Same(other.Engineering) : (other.Engineering == null);     // if null, both null, else use the same func

            return (Slot == other.Slot && Item == other.Item && Enabled == other.Enabled &&
                     Priority == other.Priority && //AmmoClip == other.AmmoClip && AmmoHopper == other.AmmoHopper &&
                     Health == other.Health && Value == other.Value && engsame);
        }

        #endregion

        #region Init

        public ShipModule()
        { }

        public ShipModule(string slotname, ShipSlots.Slot slotfdname, string itemname, string itemfdname,
                        bool? enabled, int? priority, 
                        int? ammoclip, int? ammohopper, 
                        double? health, long? value,
                        double? power,                  // only from Modules info
                        EngineeringData engineering)
        {
            Slot = slotname; SlotFD = slotfdname; Item = itemname; ItemFD = itemfdname; Enabled = enabled; Priority = priority; 
            AmmoClip = ammoclip; AmmoHopper = ammohopper;
            if (health.HasValue)
                Health = (int)(health * 100.0);
            Value = value;
            Power = power;
            Engineering = engineering;
        }

        public ShipModule( ShipModule other)
        {
            Slot = other.Slot; SlotFD = other.SlotFD; Item = other.Item; ItemFD = other.ItemFD;
            LocalisedItem = other.LocalisedItem;
            Enabled = other.Enabled; Priority = other.Priority; Health = other.Health; Value = other.Value;
            AmmoClip = other.AmmoClip; AmmoHopper = other.AmmoHopper; 
            Power = other.Power;
            Engineering = other.Engineering;
        }

        public ShipModule(string s, ShipSlots.Slot sfd, string i, string ifd, string l)
        {
            Slot = s; SlotFD = sfd; Item = i; ItemFD = ifd; LocalisedItem = l;
        }

        public void SetEngineering( EngineeringData eng )
        {
            Engineering = eng;
        }

        #endregion

    }
}
