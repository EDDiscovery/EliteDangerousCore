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
using System;
using BaseUtils.JSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.BuyExplorationData)]
    public class JournalBuyExplorationData : JournalEntry, ILedgerJournalEntry
    {
        public JournalBuyExplorationData(JObject evt ) : base(evt, JournalTypeEnum.BuyExplorationData)
        {
            System = evt["System"].Str();
            Cost = evt["Cost"].Long();

        }
        public string System { get; set; }
        public long Cost { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, System, -Cost);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("System: ".T(EDTx.JournalEntry_System), System, "Cost: ; cr;N0".T(EDTx.JournalEntry_Cost), Cost);
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.SellExplorationData)]
    public class JournalSellExplorationData : JournalEntry, ILedgerJournalEntry
    {
        private static DateTime TotalEarningsCorrectDate = new DateTime(2018, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        public JournalSellExplorationData(JObject evt) : base(evt, JournalTypeEnum.SellExplorationData)
        {
            Systems = evt["Systems"]?.ToObjectQ<string[]>() ?? new string[0];
            Discovered = evt["Discovered"]?.ToObjectQ<string[]>() ?? new string[0];
            BaseValue = evt["BaseValue"].Long();
            Bonus = evt["Bonus"].Long();
            TotalEarnings = evt["TotalEarnings"].Long(0);        // may not be present - get 0. also 3.02 has a bug with incorrect value - actually fed from the FD web server so may not be version tied
            if (TotalEarnings < BaseValue + Bonus && EventTimeUTC < TotalEarningsCorrectDate)        // so if less than the bv+bonus, it's either not there or bugged.  Fix
                TotalEarnings = BaseValue + Bonus;
        }

        public string[] Systems { get; set; }
        public string[] Discovered { get; set; }
        public long BaseValue { get; set; }
        public long Bonus { get; set; }
        public long TotalEarnings { get; set; }        // 3.0

        public void Ledger(Ledger mcl)
        {
            int count = (Systems?.Length ?? 0) + (Discovered?.Length ?? 0);

            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, count + " systems", TotalEarnings);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Amount: ; cr;N0".T(EDTx.JournalEntry_Amount), BaseValue, "Bonus: ; cr;N0".T(EDTx.JournalEntry_Bonus), Bonus,
                                "Total: ; cr;N0".T(EDTx.JournalSellExplorationData_Total), TotalEarnings);
            detailed = "";
            if (Systems != null && Systems.Length != 0)
            {
                detailed += "Scanned: ".T(EDTx.JournalEntry_Scanned);
                foreach (string s in Systems)
                    detailed += s + " ";
            }
            if (Discovered != null && Discovered.Length != 0)
            {
                detailed += System.Environment.NewLine + "Discovered: ".T(EDTx.JournalEntry_Discovered);
                foreach (string s in Discovered)
                    detailed += s + " ";
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.MultiSellExplorationData)]
    public class JournalMultiSellExplorationData : JournalEntry, ILedgerJournalEntry
    {
        public JournalMultiSellExplorationData(JObject evt) : base(evt, JournalTypeEnum.MultiSellExplorationData)   // 3.3
        {
            Systems = evt["Discovered"]?.ToObjectQ<Discovered[]>();
            BaseValue = evt["BaseValue"].Long();
            Bonus = evt["Bonus"].Long();
            TotalEarnings = evt["TotalEarnings"].Long(0);       
        }

        public class Discovered
        {
            public string SystemName { get; set; }
            public int NumBodies { get; set; }
        }

        public Discovered[] Systems { get; set; }
        public long BaseValue { get; set; }
        public long Bonus { get; set; }
        public long TotalEarnings { get; set; }

        public void Ledger(Ledger mcl)
        {
            int count = (Systems?.Length ?? 0);
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, count + " systems", TotalEarnings);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Amount: ; cr;N0".T(EDTx.JournalEntry_Amount), BaseValue, "Bonus: ; cr;N0".T(EDTx.JournalEntry_Bonus), Bonus,
                                "Total: ; cr;N0".T(EDTx.JournalMultiSellExplorationData_Total), TotalEarnings);
            detailed = "";
            if (Systems != null)
            {
                foreach (var s in Systems)
                    detailed += s.SystemName + " (" + s.NumBodies.ToString() + ") ";
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.SellOrganicData)]
    public class JournalSellOrganicData : JournalEntry, ILedgerJournalEntry
    {
        public JournalSellOrganicData(JObject evt) : base(evt, JournalTypeEnum.SellOrganicData)
        {
            MarketID = evt["MarketID"].Long();
            Bios = evt["BioData"]?.ToObjectQ<BioData[]>();
            TotalValue = 0;
            if ( Bios!=null)
            {
                foreach (var b in Bios)
                    TotalValue = b.Value + b.Bonus;
            }
        }

        public class BioData
        {
            public string Genus;
            public string Genus_Localised;
            public string Species;
            public string Species_Localised;
            public long Value;
            public long Bonus;
        };

        public BioData[] Bios;
        public long MarketID;
        public long TotalValue;

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Count: ".T(EDTx.JournalEntry_Count), Bios?.Length ?? 0, "Amount: ; cr;N0".T(EDTx.JournalEntry_Amount), TotalValue);
            detailed = "";
            if (Bios != null)
            {
                foreach (var b in Bios)
                    detailed = detailed.AppendPrePad(string.Format("{0} {1} : {2} {3}", b.Genus_Localised, b.Species_Localised, b.Value, b.Bonus), Environment.NewLine);
            }
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, (Bios?.Length??0).ToString("N0"), TotalValue);
        }

    }

    [JournalEntryType(JournalTypeEnum.ScanOrganic)]
    public class JournalScanOrganic : JournalEntry
    {
        public JournalScanOrganic(JObject evt) : base(evt, JournalTypeEnum.ScanOrganic)
        {
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public long SystemAddress { get; set; }
        public int Body;
        public string Genus;
        public string Genus_Localised;
        public string Species;
        public string Species_Localised;
        public string ScanType;     //Analyse, Log, Sample

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", ScanType, "<: ", Genus_Localised, "", Species_Localised, "@ ",whereami);
            detailed = "";
        }
    }


}
