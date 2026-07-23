/*
 * Copyright 2016-2026 EDDiscovery development team
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
            public enum PassengerType { Tourist, Refugee, Soldier, Explorer, Terrorist,  Business, AidWorker, Security, MinorCelebrity, Criminal, Politician, 
                                        Protester,
                                        Medical, HeadOfState, PoliticalPrisoner, Scientist, POW, Unknown };
            public PassengerType FDType { get; set; }        // FDtype
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
                    if (!p.Type.HasChars())
                        p.Type = "Tourist";     // a few have this missing in typical frontier style, just fill it in

                    p.FDType = Enum.TryParse(p.Type, true, out Passengers.PassengerType s) ? s : Passengers.PassengerType.Unknown;
                    if (p.FDType == Passengers.PassengerType.Unknown)
                    {
                        BaseUtils.Debugger.TraceBreak($"*** Unknown Passenger type {p.Type}");
                        ;
                    }
                    p.Type = p.FDType.ToString().SplitCapsWordFull();
                }
            }
        }

        public Passengers[] Manifest { get; set; }

        public override string GetInfo() 
        {
            if (Manifest != null && Manifest.Length > 0)
            {
                StringBuilder sb = new System.Text.StringBuilder();
                foreach (Passengers p in Manifest)
                {
                    sb.AppendSemiColonS().Build("", p.Type, "< ", p.Count, "; (VIP)", p.VIP, ";(Wanted)".Tx(), p.Wanted);
                }

                return sb.ToString();
            }
            else
                return "No Passengers".Tx();
        }
    }
}
