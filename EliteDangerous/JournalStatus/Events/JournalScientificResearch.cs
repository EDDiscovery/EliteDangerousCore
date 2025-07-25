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
    [JournalEntryType(JournalTypeEnum.ScientificResearch)]
    public class JournalScientificResearch : JournalEntry
    {
        public JournalScientificResearch(JObject evt) : base(evt, JournalTypeEnum.ScientificResearch)
        {
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());
            Name_Localised = JournalFieldNaming.CheckLocalisation(evt["Name_Localised"].Str(), Name);
            Count = evt["Count"].Int();
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            MarketID = evt["MarketID"].LongNull();
        }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public int Count { get; set; }
        public string Category { get; set; }
        public long? MarketID { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("",Name_Localised, "Count: ".T(EDCTx.JournalEntry_Count),  Count , "Category: ".T(EDCTx.JournalEntry_Category), Category);
        }
    }
}
