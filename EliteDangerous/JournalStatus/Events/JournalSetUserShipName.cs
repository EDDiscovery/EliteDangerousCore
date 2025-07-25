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
    [JournalEntryType(JournalTypeEnum.SetUserShipName)]
    public class JournalSetUserShipName : JournalEntry , IShipInformation
    {
        public JournalSetUserShipName(JObject evt) : base(evt, JournalTypeEnum.SetUserShipName)
        {
            ShipFD = JournalFieldNaming.NormaliseFDShipName(evt["Ship"].Str());
            Ship = JournalFieldNaming.GetBetterShipName(ShipFD);
            ShipID = evt["ShipID"].ULong();
            ShipName = evt["UserShipName"].Str();// name to match LoadGame
            ShipIdent = evt["UserShipId"].Str();     // name to match LoadGame
        }

        public string Ship { get; set; }
        public string ShipFD { get; set; }
        public ulong ShipID { get; set; }
        public string ShipName { get; set; }
        public string ShipIdent { get; set; }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.SetUserShipName(this);
        }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("",ShipName,"", ShipIdent, "On: ".T(EDCTx.JournalSetUserShipName_On) , Ship);
        }
    }
}
