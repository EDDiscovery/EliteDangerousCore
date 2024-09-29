/*
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
using System;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.BookDropship)]
    public class JournalBookDropship : JournalEntry
    {
        public JournalBookDropship(JObject evt) : base(evt, JournalTypeEnum.BookDropship)
        {
            DestinationSystem = evt["DestinationSystem"].StrNull();
            DestinationLocation = evt["DestinationLocation"].StrNull();
        }

        public string DestinationSystem { get; set; }
        public string DestinationLocation { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", DestinationSystem, "<: ", DestinationLocation );
        }
    }

    [JournalEntryType(JournalTypeEnum.CancelDropship)]
    public class JournalCancelDropship : JournalEntry
    {
        public JournalCancelDropship(JObject evt) : base(evt, JournalTypeEnum.CancelDropship)
        {
        }

    }

    [JournalEntryType(JournalTypeEnum.DropshipDeploy)]
    public class JournalDropshipDeploy : JournalEntry
    {
        public JournalDropshipDeploy(JObject evt) : base(evt, JournalTypeEnum.DropshipDeploy)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            Body = evt["StarSystem"].Str();
            BodyID = evt["BodyID"].IntNull();
            OnStation = evt["OnStation"].Bool();
            OnPlanet = evt["OnPlanet"].Bool();
        }

        public string StarSystem { get; set; }
        public long? SystemAddress { get; set; }
        public string Body { get; set; }
        public int? BodyID { get; set; }
        public bool OnStation { get; set; }
        public bool OnPlanet { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Body);
        }
    }

    [JournalEntryType(JournalTypeEnum.BookTaxi)]
    public class JournalBookTaxi : JournalEntry, ILedgerJournalEntry
    {
        public JournalBookTaxi(JObject evt) : base(evt, JournalTypeEnum.BookTaxi)
        {
            DestinationSystem = evt["DestinationSystem"].StrNull();
            DestinationLocation = evt["DestinationLocation"].StrNull();
            Cost = evt["Cost"].Long();
        }

        public string DestinationSystem { get; set; }
        public string DestinationLocation { get; set; }
        public long Cost { get; set; }

        public override string GetInfo()
        {
            long? cost = Cost > 0 ? Cost : default(long?);
            return BaseUtils.FieldBuilder.Build("", DestinationSystem, "<:", DestinationLocation, "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), cost);
        }

        public void Ledger(Ledger mcl)
        {
            if (Cost > 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "->" + DestinationSystem + ":" + DestinationLocation, -Cost);
        }
    }

    [JournalEntryType(JournalTypeEnum.CancelTaxi)]
    public class JournalCancelTaxi : JournalEntry, ILedgerJournalEntry
    {
        public JournalCancelTaxi(JObject evt) : base(evt, JournalTypeEnum.CancelTaxi)
        {
            Refund = evt["Refund"].Long();
        }

        public long Refund { get; set; }

        public override string GetInfo()
        {
            long? refund = Refund > 0 ? Refund : default(long?);
            return BaseUtils.FieldBuilder.Build("", refund);
        }

        public void Ledger(Ledger mcl)
        {
            if (Refund > 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "", Refund);
        }
    }


}


