/*
 * Copyright 2021 - 2024 EDDiscovery development team
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
        public SuitID ID { get; private set; }                // its Frontier SuitID
        public FDName FDName { get; private set; }          // suit type
        public string Name_Localised { get; private set; }         // localised
        public string FriendlyName { get; private set; }
        public long Price { get; private set; }             // may be 0, not known
        public bool Sold { get; private set; }
        public FDName[] SuitMods { get; private set; }     // may be null or empty

        public Suit(DateTime time, SuitID id, FDName fdname, string locname, long price,FDName[] suitmods , bool sold )
        {
            EventTime = time; ID = id; FDName = fdname; Name_Localised = locname; Price = price; Sold = sold; SuitMods = suitmods;
            if ( fdname.IsValid() )
                FriendlyName = ItemData.GetSuit(fdname, Name_Localised)?.Name ?? Name_Localised;
        }
    }

    public class SuitList
    {
        public Dictionary<SuitID, Suit> Suits(uint gen) { return suits.Get(gen, x => x.Sold == false && x.FDName.IsValid()); }    // all valid unsold suits with valid names. fdname=null special entry

        public Suit Suit(SuitID suit, uint gen) { return suits.Get(suit, gen); }    // get suit at gen

        public SuitID CurrentID(uint gen) { return suits.Get(CURSUITID, gen)?.ID ?? new SuitID(); }

        public static SuitID CURSUITID = new SuitID(1111);          // special marker to track current suit.. 

        public SuitList()
        {
        }

        public void Buy(DateTime time, SuitID id, FDName fdname, string namelocalised, long price, FDName[] mods)
        {
            suits[id] = new Suit(time, id, fdname, namelocalised, price, mods, sold: false);
        }

        public bool VerifyPresence(DateTime time, SuitID id, FDName fdname, string namelocalised, long price, FDName[] mods)
        {
            var s = suits.GetLast(id);

            if (s == null)
            {
                //DebuggerHelpers.DP("SW","Missing Suit {0} {1} {2}", id, fdname.Str(), namelocalised);
                suits[id] = new Suit(time, id, fdname, namelocalised, price, mods, sold: false);
                return false;
            }
            else
            {
                if ((s.SuitMods == null && mods != null) || (s.SuitMods != null && mods != null && !s.SuitMods.SequenceEqual(mods)) || ( s.Name_Localised != namelocalised))
                {
                    //DebuggerHelpers.DP("SW","Update suit info {0} {1} {2}", id, fdname, namelocalised);
                    suits[id] = new Suit(time, id, fdname, namelocalised, s.Price, mods, sold: false);
                    return false;
                }
            }

            return true;
        }

        public void Sell(DateTime time, SuitID id)
        {
            if (suits.ContainsKey(id))
            {
                var last = suits.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    suits[id] = new Suit(time, id, last.FDName, last.Name_Localised, last.Price, last.SuitMods, sold:true);               // new entry with this time but sold
                }
                else
                    Debugger.DP("SW","Suits sold a suit already sold " + id);
            }
            else
                Debugger.DP("SW","Suits sold a suit not seen " + id);
        }

        public void SwitchTo(DateTime time, SuitID id)
        {
            suits[CURSUITID] = new Suit(time, id, null, null, 0, null, false);
        }

        public void Upgrade(DateTime time, SuitID id, FDName fdname, int newclass, long cost)
        {
            //DebuggerHelpers.DP("SW",$"Upgrade {id} to {newclass} for {cost}");

            if (suits.ContainsKey(id))
            {
                var last = suits.GetLast(id);
                if (last.Sold == false)       // if not sold
                {
                    var newsuit = ItemData.GetNextSuit(fdname, newclass);     // get suit info and use it. Use our name instead of name_localised in upgrade since its wrong in many journals
                    if ( newsuit != null)
                        suits[id] = new Suit(time, id, newsuit.Item1, newsuit.Item2.Name, last.Price + cost, last.SuitMods, sold:false); 
                    else
                        Debugger.DP("SW","Suits upgrade suit failed to find better suit " + id + " " + fdname);
                }
                else
                    Debugger.DP("SW","Suits upgrade a suit already sold " + id);
            }
            else
                Debugger.DP("SW","Suits upgrade a suit not seen " + id);

        }

        public uint Process(JournalEntry je, string whereami, ISystem system)
        {
            if (je is ISuitInformation)
            {
                suits.NextGeneration();     // increment number, its cheap operation even if nothing gets changed

                //DebuggerHelpers.DP("SW","***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitInformation;
                e.SuitInformation(this, whereami, system);

                if (suits.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                  //  DebuggerHelpers.DP("SW","{0} {1} No changes for Suit Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation);
                    suits.AbandonGeneration();
                }
                else
                {
                  //  DebuggerHelpers.DP("SW","{0} {1} Suit List Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Suits.Generation, Suits.UpdatesAtThisGeneration);
                }
            }

            return suits.Generation;        // return the generation we are on.
        }

        private GenerationalDictionary<SuitID, Suit>  suits { get; set; } = new GenerationalDictionary<SuitID, Suit>();
    }


}

