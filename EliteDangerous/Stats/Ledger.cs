/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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

using EliteDangerousCore.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class Ledger
    {
        [System.Diagnostics.DebuggerDisplay("Tx {EventTimeUTC} {EventType} {CashAdjust} {CashTotal} {Notes}")]
        public class Transaction
        {
            public long JID { get; set; }
            public DateTime EventTimeUTC { get; set; }                           // when it was done.
            public JournalTypeEnum EventType { get; set; }                       // what caused it..
            public string Notes { get; set; }                                    // notes about the entry
            public long CashAdjust { get; set; }                                 // cash adjustment for this transaction 0 if none (negative for cost, positive for profit)
            public long Profit { get; set; }                                     // profit on this transaction
            public double ProfitPerUnit { get; set; }                            // profit per unit
            public long CashTotal { get; set; }                                  // cash total at this point
        }

        // transactions are in ascending time order
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public long CashTotal { get; set; } = 0;
        public long Assets { get; set; } = 0;
        public long Loan { get; set; } = 0;

        public Ledger()
        {
        }

        public Transaction TransactionBefore(DateTime dateutc)
        {
            int index = Transactions.FindLastIndex(x => x.EventTimeUTC < dateutc);
            return index >= 0 ? Transactions[index] : null;
        }

        public void AddEvent(long jidn, DateTime t, JournalTypeEnum j, string n, long ca)      // event with cash adjust but no profit
        {
            if (ca == 0)
            {
                System.Diagnostics.Debug.WriteLine($"Ledger no cash transaction detected {j} {n} ");
            }
            AddEvent(jidn, t, j, n, ca, 0 , 0);
        }

        public void AddEvent(long jidn, DateTime t, JournalTypeEnum j, string text, long adjust, long profitp, double ppu) // full monty
        {
            long newcashtotal = CashTotal + adjust;
            //System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} = {4}", j.ToString(), n, CashTotal, ca , newcashtotal);
            CashTotal = newcashtotal;

            Transaction tr = new Transaction
            {
                JID = jidn,
                EventTimeUTC = t,
                EventType = j,
                Notes = text,
                CashAdjust = adjust,
                Profit = profitp,
                CashTotal = CashTotal,
                ProfitPerUnit = ppu
            };

            Transactions.Add(tr);
        }

        public void Process(JournalEntry je)
        {
            if (je is ILedgerJournalEntry)
            {
                ((ILedgerJournalEntry)je).Ledger(this);
            }

        }

    }

}
