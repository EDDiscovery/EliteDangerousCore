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
        public class Transaction
        {
            public long jid;
            public DateTime utctime;                                // when it was done.
            public JournalTypeEnum jtype;                           // what caused it..
            public string notes;                                    // notes about the entry
            public long cashadjust;                                 // cash adjustment for this transaction 0 if none (negative for cost, positive for profit)
            public long profit;                                     // profit on this transaction
            public double profitperunit;                            // profit per unit
            public long cash;                                       // cash total at this point

            public bool IsJournalEventInEventFilter(string[] events)        // events are the uncompressed journal names ModuleBuy etc.
            {
                return events.Contains(jtype.ToString());
            }
        }

        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public long CashTotal { get; set; } = 0;
        public long Assets { get; set; } = 0;
        public long Loan { get; set; } = 0;

        public Ledger()
        {
        }

        public void AddEvent(long jidn, DateTime t, JournalTypeEnum j, string n, long? ca)      // event with cash adjust but no profit
        {
            AddEvent(jidn, t, j, n, ca ?? 0, 0 , 0);
        }

        public void AddEvent(long jidn, DateTime t, JournalTypeEnum j, string n)        // event with no adjust
        {
            AddEvent(jidn, t, j, n, 0, 0, 0);
        }

        public void AddEvent(long jidn, DateTime t, JournalTypeEnum j, string text, long adjust, long profitp, double ppu) // full monty
        {
            long newcashtotal = CashTotal + adjust;
            //System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} = {4}", j.ToString(), n, CashTotal, ca , newcashtotal);
            CashTotal = newcashtotal;

            Transaction tr = new Transaction
            {
                jid = jidn,
                utctime = t,
                jtype = j,
                notes = text,
                cashadjust = adjust,
                profit = profitp,
                cash = CashTotal,
                profitperunit = ppu
            };

            Transactions.Add(tr);
        }

        public void Process(JournalEntry je)
        {
            if (je is ILedgerJournalEntry)
            {
                ((ILedgerJournalEntry)je).Ledger(this);
            }
            else if (je is ILedgerNoCashJournalEntry)
            {
                ((ILedgerNoCashJournalEntry)je).LedgerNC(this);
            }
        }

    }

}
