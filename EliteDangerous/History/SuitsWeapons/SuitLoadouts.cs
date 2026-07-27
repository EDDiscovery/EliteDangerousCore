/*
 * Copyright 2021 - 2026 EDDiscovery development team
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
    [System.Diagnostics.DebuggerDisplay("{ID}:{Name}")]
    public class SuitLoadout
    {
        public DateTime EventTime { get; private set; }
        public LoadoutID ID { get; private set; }                // its Frontier LoadoutID
        public SuitID SuitID { get; private set; }                // its associated Frontier SuitID
        public string Name { get; set; }         // loadout name
        public bool Deleted { get; private set; }

        public enum SuitSlot { PrimaryWeapon1, PrimaryWeapon2, SecondaryWeapon };

        public static string ToEnglish(SuitSlot ss)
        {
            return ss.ToString().SplitCapsWordFull();
        }

        [System.Diagnostics.DebuggerDisplay("{SlotName}:{SuitModuleID}:{ModuleName}")]
        public class LoadoutModule              // matches names used in journal for module lists
        {
            public SuitSlot SlotName;
            [QuickJSON.JsonAlwaysCreate]
            public WeaponID SuitModuleID;
            [QuickJSON.JsonAlwaysCreate]
            public HandItemFDName ModuleName;
            public string ModuleName_Localised;
            public int Class;               // may be zero meaning not there
            public RecipeFDName[] WeaponMods;     // may be empty/null
            public string FriendlyName;

            public LoadoutModule() { }
            public LoadoutModule(SuitSlot slot, WeaponID suitmoduleid,HandItemFDName modulename, string locname, int cls, RecipeFDName[] weaponmods)
            {
                SlotName = slot; SuitModuleID = suitmoduleid; ModuleName = modulename; ModuleName_Localised = locname;
                Class = cls; WeaponMods = weaponmods;
                FriendlyName = ItemData.GetWeapon(ModuleName)?.Name ?? ModuleName_Localised;
            }

            public string WeaponModList() // nice names
            {
                return (WeaponMods != null) ? string.Join(", ", WeaponMods.Select(x=>Recipes.GetBetterNameForEngineeringRecipe(x))) : "";
            }
        }

        public Dictionary<SuitSlot, LoadoutModule> Modules { get; private set; }      // may be empty if not known, never null

        static public void NormaliseModules( LoadoutModule [] list)
        {
            foreach (var m in list.EmptyIfNull())
            {
                m.FriendlyName = ItemData.GetWeapon(m.ModuleName)?.Name ?? m.ModuleName_Localised;
            }
        }

        public bool CompareModules(LoadoutModule[] other)
        {
            var mlist = Modules.Values.ToList();
            foreach( var m in other)
            {
                var o = Array.Find(other, x => m.SlotName.Equals(x.SlotName));
                if (o == null || o.Class != m.Class || o.ModuleName != m.ModuleName)      
                    return false;
            }
            return true;
        }

        public string GetModuleDescription( SuitSlot slotname )
        {
            if ( Modules.TryGetValue(slotname, out LoadoutModule m))
            {
                string wml = m.WeaponModList();
                return m.FriendlyName + ":" + m.Class.ToStringInvariant() + (wml.HasChars() ? ":" + wml : "");
            }

            return "";
        }


        public SuitLoadout(DateTime time, LoadoutID id, string name, SuitID suitID, bool deleted)
        {
            EventTime = time; ID = id; Name = name; SuitID = suitID; Deleted = deleted;
            Modules = new Dictionary<SuitSlot, LoadoutModule>();    // shallow clone
        }

        public SuitLoadout(SuitLoadout other)
        {
            EventTime = other.EventTime; ID = other.ID; Name = other.Name; SuitID = other.SuitID; Deleted = other.Deleted;
            Modules = new Dictionary<SuitSlot, LoadoutModule>(other.Modules);    // shallow clone
        }
    }

    public class SuitLoadoutList
    {
        public Dictionary<LoadoutID, SuitLoadout> Loadouts(uint gen) { return loadouts.Get(gen, x => x.Name.HasChars()); }    // all valid loadouts. Name=null indicates special entry
        public SuitLoadout Loadout(LoadoutID id, uint gen) { return loadouts.Get(id, gen); }    // get loadout at gen

        public LoadoutID CurrentID(uint gen) { return loadouts.Get(CURLOADOUTID, gen)?.ID ?? new LoadoutID(); }

        public static LoadoutID CURLOADOUTID = new LoadoutID(1111);          // special marker to track current suit.. use to ignore the current entry marker

        public SuitLoadoutList()
        {
        }

        public void CreateLoadout(DateTime time, LoadoutID id, string name, SuitID suitid, SuitLoadout.LoadoutModule[] modules) // modules may be null
        {
            var s = new SuitLoadout(time, id, name, suitid, false);
            foreach (var m in modules.EmptyIfNull())
                s.Modules[m.SlotName] = m;
            loadouts[id] = s;
        }

        public bool VerifyPresence(DateTime time, LoadoutID id, string name, SuitID suitid, SuitLoadout.LoadoutModule[] modules)// modules may be null
        {
            var s = loadouts.GetLast(id);

            if ( s == null )
            {
                Debugger.DP("SW",$"Missing Loadout {id}, {name}, {suitid}");
                s = new SuitLoadout(time, id, name, suitid, false);
                foreach (var m in modules.EmptyIfNull())
                    s.Modules[m.SlotName] = m;
                loadouts[id] = s;
                return false;
            }
            else
            {
                if ( modules != null && (modules.Length != s.Modules.Count || !s.CompareModules(modules) ))
                {
                    //DebuggerHelpers.DP("SW","Update Loadout {0} {1} {2}", id, name, suitid);
                    s = new SuitLoadout(time, id, name, suitid, false);
                    foreach (var m in modules.EmptyIfNull())
                        s.Modules[m.SlotName] = m;
                    loadouts[id] = s;
                    return false;
                }
            }

            return true;
        }

        public void DeleteLoadout(DateTime time, LoadoutID id)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                if (last.Deleted == false)       // if not deleted
                {
                    loadouts[id] = new SuitLoadout(time, id, last.Name, last.SuitID, true);               // new entry with this time but sold
                }
                else
                {
                    //DebuggerHelpers.DP("SW","Suits deleted a loadout already deleted " + id);
                }
            }
            else
                Debugger.DP("SW","Suits deleted an unknown loadout " + id);
        }

        public void DeleteLoadouts(DateTime time, SuitID suitid)
        {
            var loadoutstoremove = loadouts.GetLast(x => x.SuitID == suitid);       // all with this suit id
            foreach (var l in loadoutstoremove)
                DeleteLoadout(time, l.Value.ID);      
        }

        public void Equip(LoadoutID id, SuitLoadout.SuitSlot slotname, SuitLoadout.LoadoutModule weap)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Modules[slotname] = weap;
                loadouts[id] = ld;
                //DebuggerHelpers.DP("SW","Suits Equip {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.ModuleName_Localised);
            }
            else
                Debugger.DP("SW","Suits equip an unknown loadout " + id);
        }


        public void Remove(LoadoutID id, SuitLoadout.SuitSlot slotname, SuitWeapon weap)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                if (last.Modules.ContainsKey(slotname))
                {
                    var ld = new SuitLoadout(last);
                    ld.Modules.Remove(slotname);
                    loadouts[id] = ld;
                    //DebuggerHelpers.DP("SW","Suits Remove {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
                }
                else
                {
                    //DebuggerHelpers.DP("SW","Suits Remove Failed {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
                }
            }
            else
                Debugger.DP("SW","Suits remove an unknown loadout " + id);
        }

        public void Rename(LoadoutID id, string newname)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Name = newname;
                loadouts[id] = ld;
            }
            else
                Debugger.DP("SW","Suits remove an unknown loadout " + id);
        }

        public void SwitchTo(DateTime utc, LoadoutID id)
        {
            loadouts[CURLOADOUTID] = new SuitLoadout(utc, id, null, new SuitID(), false);       // fake way of storing the current loadout ID
        }

        public Dictionary<LoadoutID, SuitLoadout> GetLoadoutsForSuit(uint gen, SuitID suitid)
        {
            //DebuggerHelpers.DP("SW","Lookup at gen {0} suitid {1}", gen, suitid);
            var ret = loadouts.Get(gen, x => x.SuitID == suitid && x.Deleted == false);
            //if ( ret != null )
            //{
            //    foreach( var kvp in ret)
            //    {
            //        DebuggerHelpers.DP("SW","..{0} {1}", kvp.Key, kvp.Value.Name);
            //    }
            //}
            return ret;
        }

        public uint Process(JournalEntry je, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (je is ISuitLoadoutInformation)
            {
                loadouts.NextGeneration();

                //DebuggerHelpers.DP("SW","***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitLoadoutInformation;
                e.LoadoutInformation(this, weap, whereami, system);

                if (loadouts.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                   // DebuggerHelpers.DP("SW","{0} {1} No changes for Loadouts Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation);
                    loadouts.AbandonGeneration();
                }
                else
                {
                   // DebuggerHelpers.DP("SW","{0} {1} Loadouts Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation, Loadouts.UpdatesAtThisGeneration);
                }
            }

            return loadouts.Generation;        // return the generation we are on.
        }

        private GenerationalDictionary<LoadoutID, SuitLoadout> loadouts { get; set; } = new GenerationalDictionary<LoadoutID, SuitLoadout>();

    }


}

