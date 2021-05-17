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
            public string FriendlyName;

            public LoadoutModule() { }
            public LoadoutModule(string slot, ulong suitmoduleid,string modulename, string locname)
            {
                SlotName = slot; SuitModuleID = suitmoduleid; ModuleName = modulename; ModuleName_Localised = locname;
                FriendlyName = ItemData.GetWeapon(ModuleName)?.Name ?? ModuleName_Localised;
            }
        }

        public Dictionary<string, LoadoutModule> Modules { get; private set; }

        static public void NormaliseModules( LoadoutModule [] list)
        {
            foreach (var m in list.EmptyIfNull())
            {
                m.ModuleName = m.ModuleName.ToLower();
                m.SlotName = m.SlotName.ToLower();
                m.FriendlyName = ItemData.GetWeapon(m.ModuleName)?.Name ?? m.ModuleName_Localised;
            }
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
        public GenerationalDictionary<ulong, SuitLoadout> Loadouts { get; private set; } = new GenerationalDictionary<ulong, SuitLoadout>();
        public ulong CurrentLoadoutID { get; private set; } = 0; // may be zero if can't find loadout

        public SuitLoadoutList()
        {
        }

        public void CreateLoadout(DateTime time, ulong id, string name, ulong suitid, SuitLoadout.LoadoutModule[] modules)
        {
            var s = new SuitLoadout(time, id, name, suitid, false);
            foreach (var m in modules.EmptyIfNull())
                s.Modules[m.ModuleName] = m;
            Loadouts.Add(id,s);
        }

        public void DeleteLoadout(DateTime time, ulong id)
        {
            if (Loadouts.ContainsKey(id))
            {
                var last = Loadouts.GetLast(id);
                if (last.Deleted == false)       // if not deleted
                {
                    Loadouts.Add(id, new SuitLoadout(time, id, last.Name, last.SuitID, true));               // new entry with this time but sold
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits deleted a loadout already deleted " + id);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits deleted an unknown loadout " + id);
        }

        public void DeleteLoadouts(DateTime time, ulong suitid)
        {
            var loadoutstoremove = Loadouts.GetLast(x => x.SuitID == suitid);       // all with this suit id
            foreach (var l in loadoutstoremove)
                DeleteLoadout(time, l.Value.ID);      // TBD to test
        }

        public void Equip(ulong id, string slotname, SuitLoadout.LoadoutModule weap)
        {
            if (Loadouts.ContainsKey(id))
            {
                var last = Loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Modules[slotname] = weap;
                Loadouts.Add(id, ld);
                System.Diagnostics.Debug.WriteLine("Suits Equip {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.ModuleName_Localised);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits equip an unknown loadout " + id);
        }


        public void Remove(ulong id, string slotname, SuitWeapon weap)
        {
            if (Loadouts.ContainsKey(id))
            {
                var last = Loadouts.GetLast(id);
                if (last.Modules.ContainsKey(slotname))
                {
                    var ld = new SuitLoadout(last);
                    ld.Modules.Remove(slotname);
                    Loadouts.Add(id, ld);
                    System.Diagnostics.Debug.WriteLine("Suits Remove {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Suits Remove Failed {0}-{1}-{2} with {3}", last.ID, last.Name, slotname, weap.Name_Localised);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits remove an unknown loadout " + id);
        }

        public void Rename(ulong id, string newname)
        {
            if (Loadouts.ContainsKey(id))
            {
                var last = Loadouts.GetLast(id);
                var ld = new SuitLoadout(last);
                ld.Name = newname;
                Loadouts.Add(id, ld);
            }
            else
                System.Diagnostics.Debug.WriteLine("Suits remove an unknown loadout " + id);
        }

        public void SwitchTo(ulong id)
        {
            CurrentLoadoutID = Loadouts.ContainsKey(id) ? id : 0;
        }

        public Dictionary<ulong, SuitLoadout> GetLoadoutsForSuit(uint gen, ulong suitid)
        {
            System.Diagnostics.Debug.WriteLine("Lookup at gen {0} suitid {1}", gen, suitid);
            var ret = Loadouts.Get(gen, x => x.SuitID == suitid && x.Deleted == false);
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
                Loadouts.NextGeneration();

                //System.Diagnostics.Debug.WriteLine("***********************" + je.EventTimeUTC + " GENERATION " + items.Generation);

                var e = je as ISuitLoadoutInformation;
                e.LoadoutInformation(this, weap, whereami, system);

                if (Loadouts.UpdatesAtThisGeneration == 0)         // if nothing changed, abandon it.
                {
                   // System.Diagnostics.Debug.WriteLine("{0} {1} No changes for Loadouts Generation {2} Abandon", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation);
                    Loadouts.AbandonGeneration();
                }
                else
                {
                   // System.Diagnostics.Debug.WriteLine("{0} {1} Loadouts Generation {2} Changes {3}", je.EventTimeUTC.ToString(), je.EventTypeStr, Loadouts.Generation, Loadouts.UpdatesAtThisGeneration);
                }
            }

            return Loadouts.Generation;        // return the generation we are on.
        }

    }


}

