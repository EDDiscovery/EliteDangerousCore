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
    [JournalEntryType(JournalTypeEnum.Reputation)]
    public class JournalReputation : JournalEntry
    {
        public JournalReputation(JObject evt ) : base(evt, JournalTypeEnum.Reputation)
        {
            Federation = evt["Federation"].DoubleNull();
            Empire = evt["Empire"].DoubleNull();
            Independent = evt["Independent"].DoubleNull();
            Alliance = evt["Alliance"].DoubleNull();
        }

        public double? Federation { get; set; }
        public double? Empire { get; set; }
        public double? Independent { get; set; }
        public double? Alliance { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("Federation: ;;0.#".T(EDCTx.JournalReputation_Federation), Federation , "Empire: ;;0.#".T(EDCTx.JournalReputation_Empire), Empire, "Independent: ;;0.#".T(EDCTx.JournalReputation_Independent), Independent , "Alliance: ;;0.#".T(EDCTx.JournalReputation_Alliance), Alliance);
        }

    }
}
