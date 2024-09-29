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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Passengers)]
    public class JournalPassengers : JournalEntry
    {
        public class Passengers
        {
            public ulong MissionID { get; set; }
            public string Type { get; set; }          // Friendly name, not fdev
            public string FDType { get; set; }        // FDtype
            public bool VIP { get; set; }
            public bool Wanted { get; set; }
            public int Count { get; set; }

            public Passengers()
            { }
        }

        public JournalPassengers(JObject evt) : base(evt, JournalTypeEnum.Passengers)
        {
            Manifest = evt["Manifest"]?.ToObjectQ<Passengers[]>();

            if (Manifest != null )
            {
                foreach (Passengers p in Manifest)
                {
                    p.FDType = p.Type;
                    p.Type = JournalFieldNaming.PassengerType(p.FDType);
                }
            }
        }

        public Passengers[] Manifest { get; set; }

        public override string GetInfo() 
        {
            if (Manifest != null && Manifest.Length > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Passengers p in Manifest)
                {
                    sb.BuildPrePad(", ", "", p.Type, "< ", p.Count, "; (VIP)", p.VIP, ";(Wanted)".T(EDCTx.JournalEntry_Wanted), p.Wanted);
                }
                return sb.ToString();
            }
            else
                return "No Passengers".T(EDCTx.JournalEntry_NoPassengers);
        }
    }
}
