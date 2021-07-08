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
    [System.Diagnostics.DebuggerDisplay("{ID}:{Name}")]
    public class SuitLoadout
    {
        public DateTime EventTime { get; private set; }
        public ulong ID { get; private set; }                // its Frontier LoadoutID
        public ulong SuitID { get; private set; }                // its associated Frontier SuitID
        public string Name { get; set; }         // loadout name
        public bool Deleted { get; private set; }

        [System.Diagnostics.DebuggerDisplay("{SlotName}:{SuitModuleID}:{ModuleName}")]
        public class LoadoutModule              // matches names used in journal for module lists
        {
            public string SlotName;
            public ulong SuitModuleID;
            public string ModuleName;
            public string ModuleName_Localised;
            public int Class;               // may be zero meaning not there
            public string[] WeaponMods;     // may be empty/null
            public string FriendlyName;

            public LoadoutModule() { }
            public LoadoutModule(string slot, ulong suitmoduleid,string modulename, string locname, int cls, string[] weaponmods)
            {
                SlotName = slot; SuitModuleID = suitmoduleid; ModuleName = modulename; ModuleName_Localised = locname;
                Class = cls; WeaponMods = weaponmods;
                FriendlyName = ItemData.GetWeapon(ModuleName)?.Name ?? ModuleName_Localised;
            }

            public string WeaponModList()
            {
                return (WeaponMods != null) ? string.Join(", ", WeaponMods) : "";
            }
        }

        public Dictionary<string, LoadoutModule> Modules { get; private set; }      // may be empty if not known, never null

        static public void NormaliseModules( LoadoutModule [] list)
        {
            foreach (var m in list.EmptyIfNull())
            {
                m.ModuleName = m.ModuleName.ToLower();
                m.SlotName = m.SlotName.ToLower();
                m.FriendlyName = ItemData.GetWeapon(m.ModuleName)?.Name ?? m.ModuleName_Localised;
            }
        }

        public bool CompareModules(LoadoutModule[] other)
        {
            var mlist = Modules.Values.ToList();
            foreach( var m in other)
            {
                var o = Array.Find(other, x => m.SlotName.Equals(x.SlotName));
                if (o == null || o.Class != m.Class || o.ModuleName != m.ModuleName)        // TBD Maybe more
                    return false;
            }
            return true;
        }

        public string GetModuleDescription( string slotname )
        {
            if ( Modules.TryGetValue(slotname, out LoadoutModule m))
            {
                string wml = m.WeaponModList();
                return m.FriendlyName + ":" + m.Class.ToStringInvariant() + (wml.HasChars() ? " " + wml : "");
            }

            return "";
        }


        public SuitLoadout(DateTime time, ulong id, string name, ulong suitID, bool deleted)
        {
            EventTime = time; ID = id; Name = name; SuitID = suitID; Deleted = deleted;
            Modules = new Dictionary<string, LoadoutModule>();    // shallow clone
        }

        public SuitLoadout(SuitLoadout other)
        {
            EventTime = other.EventTime; ID = other.ID; Name = other.Name; SuitID = other.SuitID; Deleted = other.Deleted;
            Modules = new Dictionary<string, LoadoutModule>(other.Modules);    // shallow clone
        }
    }

    public class SuitLoadoutList
    {
        public Dictionary<ulong, SuitLoadout> Loadouts(uint gen) { return loadouts.Get(gen, x => x.Name.HasChars()); }    // all valid loadouts. Name=null indicates special entry
        public SuitLoadout Loadout(ulong id, uint gen) { return loadouts.Get(id, gen); }    // get loadout at gen

        public ulong CurrentID(uint gen) { return loadouts.Get(CURLOADOUTID, gen)?.ID ?? 0; }

        public const ulong CURLOADOUTID = 1111;          // special marker to track current suit.. use to ignore the current entry marker

        public SuitLoadoutList()
        {
        }

        public void CreateLoadout(DateTime time, ulong id, string name, ulong suitid, SuitLoadout.LoadoutModule[] modules) // modules may be null
        {
            var s = new SuitLoadout(time, id, name, suitid, false);
            foreach (var m in modules.EmptyIfNull())
                s.Modules[m.SlotName] = m;
            loadouts[id] = s;
        }

        public bool VerifyPresence(DateTime time, ulong id, string name, ulong suitid, SuitLoadout.LoadoutModule[] modules)// modules may be null
        {
            var s = loadouts.GetLast(id);

            if ( s == null )
            {
                System.Diagnostics.Debug.WriteLine("Missing Loadout {0} {1} {2}", id, name, suitid);
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
                    //System.Diagnostics.Debug.WriteLine("Update Loadout {0} {1} {2}", id, name, suitid);
                    s = new SuitLoadout(time, id, name, suitid, false);
                    foreach (var m in modules.EmptyIfNull())
                        s.Modules[m.SlotName] = m;
                    loadouts[id] = s;
                    return false;
                }
            }

            return true;
        }

        public void DeleteLoadout(DateTime time, ulong id)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                if (last.Deleted == false)       // if not deleted
                {
                    loadouts[id] = new SuitLoadout(time, id, last.Name, last.SuitID, true);               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits deleted a loadout already deleted " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits deleted an unknown loadout " + id);
        }

        public void DeleteLoadouts(DateTime time, ulong suitid)
        {
            var loadoutstoremove = loadouts.GetLast(x => x.SuitID == suitid);       // all with this suit id
            foreach (var l in loadoutstoremove)
                DeleteLoadout(time, l.Value.ID);      // TBD to test
        }

        public void Equip(ulong id, string slotname, SuitLoadout.LoadoutModule weap)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Modules[slotname] = weap;
                loadouts[id] = ld;
                //System.Diagnostics.Debug.WriteLine("Suits Equip {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.ModuleName_Localised);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits equip an unknown loadout " + id);
        }


        public void Remove(ulong id, string slotname, SuitWeapon weap)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                if (last.Modules.ContainsKey(slotname))
                {
                    var ld = new SuitLoadout(last);
                    ld.Modules.Remove(slotname);
                    loadouts[id] = ld;
                    //System.Diagnostics.Debug.WriteLine("Suits Remove {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits Remove Failed {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits remove an unknown loadout " + id);
        }

        public void Rename(ulong id, string newname)
        {
            if (loadouts.ContainsKey(id))
            {
                var last = loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Name = newname;
                loadouts[id] = ld;
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits remove an unknown loadout " + id);
        }

        public void SwitchTo(DateTime utc, ulong id)
        {
            loadouts[CURLOADOUTID] = new SuitLoadout(utc, id, null, 0, false);
        }

        public Dictionary<ulong, SuitLoadout> GetLoadoutsForSuit(uint gen, ulong suitid)
        {
            //System.Diagnostics.Debug.WriteLine("Lookup at gen {0} suitid {1}", gen, suitid);
            var ret = loadouts.Get(gen, x => x.SuitID == suitid && x.Deleted == false);
            //if ( ret != null )
            //{
            //    foreach( var kvp in ret)
            //    {
            //        System.Diagnostics.Debug.WriteLine("..{0} {1}", kvp.Key, kvp.Value.Name);
            //    }
            //}
            return ret;
        }

        public uint Process(JournalEntry je, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (je is ISuitLoadoutInformation)
            {
                loadouts.NextGeneration();

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitLoadoutInformation;
                e.LoadoutInformation(this, weap, whereami, system);

                if (loadouts.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                   // System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Loadouts Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation);
                    loadouts.AbandonGeneration();
                }
                else
                {
                   // System.Diagnostics.Debug.WriteLine("{0} {1} Loadouts Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation, Loadouts.UpdatesAtThisGeneration);
                }
            }

            return loadouts.Generation;        // return the generation we are on.
        }

        private GenerationalDictionary<ulong, SuitLoadout> loadouts { get; set; } = new GenerationalDictionary<ulong, SuitLoadout>();

    }


}

