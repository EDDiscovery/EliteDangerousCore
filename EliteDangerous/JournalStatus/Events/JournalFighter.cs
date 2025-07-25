﻿/*
 * Copyright © 2016-2023 EDDiscovery development team
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
using QuickJSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.DockFighter)]
    public class JournalDockFighter : JournalEntry,  IShipInformation
    {
        public int? ID { get; set; }

        public JournalDockFighter(JObject evt ) : base(evt, JournalTypeEnum.DockFighter)
        {
            ID = evt["ID"].IntNull();
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.DockFighter();
        }
    }


    [JournalEntryType(JournalTypeEnum.FighterDestroyed)]
    public class JournalFighterDestroyed : JournalEntry, IShipInformation
    {
        public int? ID { get; set; }

        public JournalFighterDestroyed(JObject evt) : base(evt, JournalTypeEnum.FighterDestroyed)
        {
            ID = evt["ID"].IntNull();
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.FighterDestroyed();
        }

    }

    [JournalEntryType(JournalTypeEnum.FighterRebuilt)]
    public class JournalFighterRebuilt : JournalEntry
    {
        public JournalFighterRebuilt(JObject evt) : base(evt, JournalTypeEnum.FighterRebuilt)
        {
            Loadout = evt["Loadout"].Str();
            ID = evt["ID"].IntNull();
        }

        public string Loadout { get; set; }
        public int? ID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Loadout);
        }
    }

    [JournalEntryType(JournalTypeEnum.LaunchFighter)]
    public class JournalLaunchFighter : JournalEntry, IShipInformation
    {
        public JournalLaunchFighter(JObject evt) : base(evt, JournalTypeEnum.LaunchFighter)
        {
            Loadout = evt["Loadout"].Str();
            PlayerControlled = evt["PlayerControlled"].Bool();
            ID = evt["ID"].IntNull();
        }
        public string Loadout { get; set; }
        public bool PlayerControlled { get; set; }
        public int? ID { get; set; }


        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.LaunchFighter(PlayerControlled);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Loadout: ".T(EDCTx.JournalEntry_Loadout), Loadout, "NPC Controlled;".T(EDCTx.JournalEntry_NPCControlled), PlayerControlled);
        }
    }


}
