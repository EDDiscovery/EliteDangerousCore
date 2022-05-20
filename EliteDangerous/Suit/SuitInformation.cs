/*
 * Copyright © 2021 - 2021 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}:{FDName}:{FriendlyName}")]
    public class Suit
    {
        public DateTime EventTime { get; private set; }
        public ulong ID { get; private set; }                // its Frontier SuitID
        public string FDName { get; private set; }          // suit type
        public string Name_Localised { get; private set; }         // localised
        public string FriendlyName { get; private set; }
        public long Price { get; private set; }             // may be 0, not known
        public bool Sold { get; private set; }
        public string[] SuitMods { get; private set; }     // may be null or empty

        public Suit(DateTime time, ulong id, string fdname, string locname, long price,string[] suitmods , bool sold )
        {
            EventTime = time; ID = id; FDName = fdname; Name_Localised = locname; Price = price; Sold = sold; SuitMods = suitmods;
            if ( fdname.HasChars() )
                FriendlyName = ItemData.GetSuit(fdname, Name_Localised)?.Name ?? Name_Localised;
        }
    }

    public class SuitList
    {
        public Dictionary<ulong, Suit> Suits(uint gen) { return suits.Get(gen, x => x.Sold == false && x.FDName.HasChars()); }    // all valid unsold suits with valid names. fdname=null special entry
        public Suit Suit(ulong suit, uint gen) { return suits.Get(suit, gen); }    // get suit at gen

        public ulong CurrentID(uint gen) { return suits.Get(CURSUITID, gen)?.ID ?? 0; }

        public const ulong CURSUITID = 1111;          // special marker to track current suit.. 

        public SuitList()
        {
        }

        public void Buy(DateTime time, ulong id, string fdname, string namelocalised, long price, string[] mods)
        {
            suits[id] = new Suit(time, id, fdname, namelocalised, price, mods, sold: false);
        }

        public bool VerifyPresence(DateTime time, ulong id, string fdname, string namelocalised, long price, string[] mods)
        {
            var s = suits.GetLast(id);

            if (s == null)
            {
                System.Diagnostics.Debug.WriteLine("Missing Suit {0} {1} {2}", id, fdname, namelocalised);
                suits[id] = new Suit(time, id, fdname, namelocalised, price, mods, sold: false);
                return false;
            }
            else
            {
                if ((s.SuitMods == null && mods != null) || (s.SuitMods != null && mods != null && !s.SuitMods.SequenceEqual(mods)) || ( s.Name_Localised != namelocalised))
                {
                    //System.Diagnostics.Debug.WriteLine("Update suit info {0} {1} {2}", id, fdname, namelocalised);
                    suits[id] = new Suit(time, id, fdname, namelocalised, s.Price, mods, sold: false);
                    return false;
                }
            }

            return true;
        }

        public void Sell(DateTime time, ulong id)
        {
            if (suits.ContainsKey(id))
            {
                var last = suits.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    suits[id] = new Suit(time, id, last.FDName, last.Name_Localised, last.Price, last.SuitMods, sold:true);               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits sold a suit already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits sold a suit not seen " + id);
        }

        public void SwitchTo(DateTime time, ulong id)
        {
            suits[CURSUITID] = new Suit(time, id, null, null, 0, null, false);
        }

        public void Upgrade(DateTime time, ulong id, string fdname, int newclass, long cost)
        {
            //System.Diagnostics.Debug.WriteLine($"Upgrade {id} to {newclass} for {cost}");

            if (suits.ContainsKey(id))
            {
                var last = suits.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    var newsuit = ItemData.GetNextSuit(fdname, newclass);     // get suit info and use it. Use our name instead of name_localised in upgrade since its wrong in many journals
                    if ( newsuit != null)
                        suits[id] = new Suit(time, id, newsuit.Item1, newsuit.Item2.Name, last.Price + cost, last.SuitMods, sold:false); 
                    else
                        System.Diagnostics.Debug.WriteLine("Suits upgrade suit failed to find better suit " + id + " " + fdname);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits upgrade a suit already sold " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits upgrade a suit not seen " + id);

        }

        public uint Process(JournalEntry je, string whereami, ISystem system)
        {
            if (je is ISuitInformation)
            {
                suits.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitInformation;
                e.SuitInformation(this, whereami, system);

                if (suits.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Suit Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation);
                    suits.AbandonGeneration();
                }
                else
                {
                  //  System.Diagnostics.Debug.WriteLine("{0} {1} Suit List Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation, Suits.UpdatesAtThisGeneration);
                }
            }

            return suits.Generation;        // return the generation we are on.
        }

        private GenerationalDictionary<ulong, Suit> suits { get; set; } = new GenerationalDictionary<ulong, Suit>();
    }


}

