/*
 * Copyright © 2016 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EliteDangerousCore.JournalEvents;
using BaseUtils.JSON;
using BaseUtils;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}:{FDName}:{Name}")]
    public class Suit
    {
        public DateTime EventTime { get; private set; }
        public ulong ID { get; private set; }                // its Frontier SuitID   
        public string FDName { get; private set; }          // suit type
        public string Name_Localised { get; private set; }         // localised
        public string FriendlyName { get; private set; }
        public long Price { get; private set; }             // may be 0, not known
        public bool Sold { get; private set; }

        public Suit(DateTime time, ulong id, string fdname, string locname, long price, bool sold)
        {
            EventTime = time; ID = id; FDName = fdname; Name_Localised = locname; Price = price; Sold = sold;
            FriendlyName = ItemData.GetSuit(fdname, Name_Localised)?.Name ?? Name_Localised;
        }
    }

    public class SuitList
    {
        public GenerationalDictionary<ulong, Suit> Suits { get; private set; } = new GenerationalDictionary<ulong, Suit>();

        public ulong CurrentID(uint gen) { return Suits.Get(CURSUITID, gen)?.FDName.InvariantParseULong(0) ?? 0; }
        static public bool SpecialID(ulong id) { return id == CURSUITID; }

        public const ulong CURSUITID = 1111;          // special marker to track current suit.. use to ignore the current entry marker

        public SuitList()
        {
        }

        public void Buy(DateTime time, ulong id, string fdname, string namelocalised, long price)
        {
            Suits.Add(id, new Suit(time, id, fdname, namelocalised, price, false));
        }

        public void Sell(DateTime time, ulong id)
        {
            if (Suits.ContainsKey(id))
            {
                var last = Suits.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    Suits.Add(id, new Suit(time, id, last.FDName, last.Name_Localised, last.Price, true));               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits sold a suit already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits sold a suit not seen " + id);
        }

        public void SwitchTo(DateTime time, ulong id)
        {
            Suits.Add(CURSUITID, new Suit(time, CURSUITID, id.ToStringInvariant(), "$SUITID", 0, false));
        }

        public uint Process(JournalEntry je, string whereami, ISystem system)
        {
            if (je is ISuitInformation)
            {
                Suits.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitInformation;
                e.SuitInformation(this, whereami, system);

                if (Suits.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Suit Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation);
                    Suits.AbandonGeneration();
                }
                else
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} Suit List Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation, Suits.UpdatesAtThisGeneration);
                }
            }

            return Suits.Generation;        // return the generation we are on.
        }

    }


}

