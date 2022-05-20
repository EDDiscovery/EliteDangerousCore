/*
 * Copyright © 2021 - 2022 EDDiscovery development team
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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class EngineeringList
    {
        public const string TechBrokerID = "Tech Broker";

        // Get vertical slice of engineer or tech broker. In add order. Note it can hold any of the journal types involved in engineering
        public List<HistoryEntry> Get(uint gen, string engineer) { return crafting.GetHistoryOfKey(gen, engineer)?.Values.ToList(); }

        public EngineeringList()
        {
        }

        public uint Process(HistoryEntry he)
        {
            if (he.journalEntry is JournalEngineerCraftBase)
            {
                var jecb = he.journalEntry as JournalEngineerCraftBase;
                crafting.NextGeneration();
                crafting.Add(jecb.Engineering.Engineer, he);
            }
            else if (he.EntryType == JournalTypeEnum.TechnologyBroker)
            {
                crafting.NextGeneration();
                crafting.Add(TechBrokerID, he);        // add under tech broker
            }

            return crafting.Generation;        // return the generation we are on.
        }

        private GenerationalDictionary<string, HistoryEntry> crafting { get; set; } = new GenerationalDictionary<string, HistoryEntry>();
    }
}

