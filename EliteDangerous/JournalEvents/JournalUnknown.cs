/*
 * Copyright © 2016-2018 EDDiscovery development team
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

 using BaseUtils.JSON;
using System;

namespace EliteDangerousCore.JournalEvents
{
    public class JournalUnimplemented: JournalEntry
    {
        public JObject json;
        public string eventname;
        public bool withdrawn;

        public JournalUnimplemented(JObject evt, JournalTypeEnum e, bool withdrawn) : base(evt, e)
        {
            json = evt;
            eventname = evt["event"].Str("No event tag");
            this.withdrawn = withdrawn;
        }

        public override string SummaryName(ISystem sys)
        {
            return json["event"].Str("Unknown").SplitCapsWordFull();
        }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            JObject jt = json.Clone().Object();
            jt.Remove("timestamp");
            jt.Remove("event");
            info = (withdrawn ? "Obsolete Event:" : "Unknown Event:") + jt.ToStringLiteral();
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.Unknown)]
    public class JournalUnknown : JournalUnimplemented
    {
        public JournalUnknown(JObject evt) : base(evt, JournalTypeEnum.Unknown,false)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.TransferMicroResources)]
    public class JournalTransferMicroResources : JournalUnimplemented
    {
        public JournalTransferMicroResources(JObject evt) : base(evt, JournalTypeEnum.TransferMicroResources,true)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.ShipLockerMaterials)]
    public class JournalShipLockerMaterials : JournalUnimplemented
    {
        public JournalShipLockerMaterials(JObject evt) : base(evt, JournalTypeEnum.ShipLockerMaterials,true)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.BackPack)]
    public class JournalBackPack : JournalUnimplemented
    {
        public JournalBackPack(JObject evt) : base(evt, JournalTypeEnum.BackPack, true)
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.BackPackMaterials)]
    public class JournalBackPackMaterials : JournalUnimplemented
    {
        public JournalBackPackMaterials(JObject evt) : base(evt, JournalTypeEnum.BackPack, true)
        {
        }
    }


}
