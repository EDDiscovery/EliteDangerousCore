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
    [JournalEntryType(JournalTypeEnum.Commander)]
    public class JournalCommander : JournalEntry
    {
        public JournalCommander(JObject evt ) : base(evt, JournalTypeEnum.Commander)
        {
            Name = JournalFieldNaming.SubsituteCommanderName(evt["Name"].Str());
            FID = JournalFieldNaming.SubsituteCommanderFID(evt["FID"].Str());     // 3.3 on
        }

        public string Name { get; set; }
        public string FID { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("Cmdr ", Name);
        }
    }

    [JournalEntryType(JournalTypeEnum.NewCommander)]
    public class JournalNewCommander : JournalEntry
    {
        public JournalNewCommander(JObject evt) : base(evt, JournalTypeEnum.NewCommander)
        {
            Name = JournalFieldNaming.SubsituteCommanderName(evt["Name"].Str());
            FID = JournalFieldNaming.SubsituteCommanderFID(evt["FID"].Str());     // 3.3 on
            Package = evt["Package"].Str();
        }

        public string Name { get; set; }
        public string Package { get; set; }
        public string FID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Cmdr ", Name, "Starting Package: ".T(EDCTx.JournalEntry_StartingPackage), Package);
        }
    }

    [JournalEntryType(JournalTypeEnum.ClearSavedGame)]
    public class JournalClearSavedGame : JournalEntry
    {
        public JournalClearSavedGame(JObject evt) : base(evt, JournalTypeEnum.ClearSavedGame)
        {
            Name = JournalFieldNaming.SubsituteCommanderName(evt["Name"].Str());
            FID = JournalFieldNaming.SubsituteCommanderFID(evt["FID"].Str());     // 3.3 on
        }

        public string Name { get; set; }
        public string FID { get; set; }
    }
}
