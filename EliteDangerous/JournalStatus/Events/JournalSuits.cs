/*
 * Copyright 2021-2026 EDDiscovery development team
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
using QuickJSON;
using System;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.BuySuit)]
    public class JournalBuySuit : JournalEntry, ISuitInformation, ILedgerJournalEntry
    {
        public JournalBuySuit(JObject evt) : base(evt, JournalTypeEnum.BuySuit)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    initialobject: this);        // read fields named in this structure matching JSON names

            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name_Localised = JournalFieldNaming.CheckLocalisation(Name_Localised, FriendlyName);
        }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // may be missing in early ones

        [JsonAlwaysCreate]
        public FDName Name { get; set; }                // always there, set just in case
        public string FriendlyName { get; set; }
        public string Name_Localised { get; set; }
        public long Price { get; set; }
        public FDName[] SuitMods { get; set; }          // may be null or empty

        public override string GetInfo()
        {
            string smod = SuitMods != null ? string.Join(", ", SuitMods.Select(x=>Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            return BaseUtils.FieldBuilder.Build("", FriendlyName, "Mods".Tx()+": ", smod, "Cost: ; cr;N0".Tx(), Price);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Buy(EventTimeUTC, SuitID, Name, Name_Localised, Price, SuitMods);
            }
        }

        public void Ledger(Ledger mcl)
        {
            if (Price != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, -Price);
        }
    }

    [JournalEntryType(JournalTypeEnum.SellSuit)]
    public class JournalSellSuit : JournalEntry, ISuitInformation, ISuitLoadoutInformation, ILedgerJournalEntry
    {
        public JournalSellSuit(JObject evt) : base(evt, JournalTypeEnum.SellSuit)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    initialobject: this);        // read fields named in this structure matching JSON names

            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name_Localised = JournalFieldNaming.CheckLocalisation(Name_Localised, FriendlyName);
        }

        public JournalSellSuit(DateTime utc, ulong id, FDName fdname, string locname, long price, int cmdrid) : base(utc,JournalTypeEnum.SellSuit)
        {
            SuitID = id; Name = fdname; Name_Localised = locname; price = Price;
            SetCommander(cmdrid);
        }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // may be missing in early ones
        [JsonAlwaysCreate]
        public FDName Name { get; set; }            // always there, set just in case
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Price { get; set; }
        public string[] SuitMods { get; set; }      // may be null

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", FriendlyName, "< sell price ; cr;N0".Tx(), Price);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Sell(EventTimeUTC, SuitID);
            }
        }
        public void Ledger(Ledger mcl)
        {
            if (Price != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, Price);
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.DeleteLoadouts(EventTimeUTC, SuitID);   // all loadouts for this suit deleted
            }
        }

        public JObject Json()            // create JSON of this record..
        {
            JObject evt = new JObject();
            evt["timestamp"] = EventTimeUTC;
            evt["event"] = EventTypeStr;
            evt["SuitID"] = SuitID;
            evt["Name"] = Name.Str();
            evt["Name_Localised"] = Name_Localised;
            evt["Price"] = Price;
            return evt;
        }

    }

    [JournalEntryType(JournalTypeEnum.CreateSuitLoadout)]
    public class JournalCreateSuitLoadout : JournalEntry, ISuitInformation, ISuitLoadoutInformation, IWeaponInformation
    {
        public JournalCreateSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.CreateSuitLoadout)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names

            if (SuitName != null)           // early records has this missing
            {
                SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
                SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            }
            SuitLoadout.NormaliseModules(Modules);
        }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // indicating missing, early records had it missing
        public FDName SuitName { get; set; }                    // may be null for early records
        public string SuitFriendlyName { get; set; }            // may be null for early records
        public string SuitName_Localised { get; set; }          // may be null for early records

        public FDName[] SuitMods { get; set; }                  // may be null or empty

        public SuitLoadout.LoadoutModule[] Modules { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitName_Localised ?? "Unknown", "< ++> ", LoadoutName);
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)      // executed first
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.VerifyPresence(EventTimeUTC, SuitID, SuitName, SuitName_Localised, 0, SuitMods);
            }
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            foreach (var m in Modules.EmptyIfNull())
            {
                shp.VerifyPresence(EventTimeUTC, m.SuitModuleID, m.ModuleName, m.ModuleName_Localised, 0, m.Class, m.WeaponMods);
            }
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.CreateLoadout(EventTimeUTC, LoadoutID, LoadoutName, SuitID, Modules);
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.SuitLoadout)]
    public class JournalSuitLoadout : JournalEntry, IWeaponInformation, ISuitInformation, ISuitLoadoutInformation
    {
        public JournalSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.SuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags:System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, 
                initialobject: this);        // read fields named in this structure matching JSON names

            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            SuitLoadout.NormaliseModules(Modules);
        }

        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // indicating missing, early records had it missing
        [JsonAlwaysCreate]
        public FDName SuitName { get; set; }                    // always there
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }

        public FDName[] SuitMods { get; set; }          // may be null or empty

        public SuitLoadout.LoadoutModule[] Modules { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitID % 10000, "", LoadoutID % 10000, "", SuitFriendlyName, "< ==> ", LoadoutName);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)      // executed first
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.VerifyPresence(EventTimeUTC, SuitID, SuitName, SuitName_Localised, 0, SuitMods);
            }
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            foreach (var m in Modules.EmptyIfNull())
            {
                shp.VerifyPresence(EventTimeUTC, m.SuitModuleID, m.ModuleName, m.ModuleName_Localised, 0, m.Class, m.WeaponMods);
            }
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.VerifyPresence(EventTimeUTC, LoadoutID, LoadoutName, SuitID, Modules);
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.DeleteSuitLoadout)]
    public class JournalDeleteSuitLoadout : JournalEntry, ISuitLoadoutInformation
    {
        public JournalDeleteSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.DeleteSuitLoadout)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true,
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names
            
            if (SuitName != null)
            {
                SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
                SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            }
        }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // indicating missing, early records had it missing
        public FDName SuitName { get; set; }                    // may be null for early records
        public string SuitFriendlyName { get; set; }            // may be null for early records
        public string SuitName_Localised { get; set; }          // may be null for early records

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitName_Localised ?? "Unknown", "< --> ", LoadoutName);
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.DeleteLoadout(EventTimeUTC, LoadoutID);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.LoadoutEquipModule)]
    public class JournalLoadoutEquipModule : JournalEntry, ISuitLoadoutInformation, ISuitInformation, IWeaponInformation
    {
        public JournalLoadoutEquipModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutEquipModule)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names

            SlotFriendlyName = SuitLoadout.ToEnglish(SlotName);

            if (SuitName != null)
            {
                SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
                SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            }

            if (ModuleName != null)
            {
                ModuleNameFriendly = ItemData.GetWeapon(ModuleName, ModuleName_Localised)?.Name ?? ModuleName_Localised;
            }
        }

        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }
        public SuitLoadout.SuitSlot SlotName { get; set; }
        public string SlotFriendlyName { get; set; }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // indicating missing, early records had it missing 
        public FDName SuitName { get; set; }                    // may be null, missing in v.early ones, 
        public string SuitFriendlyName { get; set; }            // may be null, missing in v.early ones, 
        public string SuitName_Localised { get; set; }          // may be null, missing in v.early ones, 

        public FDName ModuleName { get; set; }                  // always there unless bug in journal
        public string ModuleNameFriendly { get; set; }
        public string ModuleName_Localised { get; set; }

        public int Class { get; set; }                          // may not be there
        public FDName[] WeaponMods { get; set; }                // fdname, may be null or empty
        public ulong SuitModuleID { get; set; }                 // aka weapon ID

        public override string GetInfo()
        {
            string wmod = WeaponMods != null ? string.Join(", ", WeaponMods.Select(x=>x.Str()).ToArray()) : null;
            return BaseUtils.FieldBuilder.Build("", SuitID % 10000, "", LoadoutID%10000, "", SuitFriendlyName, "<: ", LoadoutName, "<: ", SlotFriendlyName, "< ++> ", ModuleNameFriendly, "Class".Tx()+": ", Class, "Mods".Tx()+": ", wmod);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)      // executed first
        {
            if (SuitID != ulong.MaxValue && SuitName != null)
            {
                shp.VerifyPresence(EventTimeUTC, SuitID, SuitName, SuitName_Localised, 0, new FDName[] { });
            }
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)      // executed second
        {
            if (SuitID != ulong.MaxValue && SuitName != null)
            {
                shp.VerifyPresence(EventTimeUTC, SuitModuleID, ModuleName, ModuleName_Localised, 0, Class, WeaponMods);
            }
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)   // excuted third
        {
            if (SuitID != ulong.MaxValue && SuitName != null)
            {
                shp.VerifyPresence(EventTimeUTC, LoadoutID, LoadoutName, SuitID, null);

                //System.Diagnostics.Debug.WriteLine("{0} Equip suit {1} Loadout {2} slot {3} with {4} {5} {6}", EventTimeUTC.ToString(), SuitID, LoadoutID, SlotName, ModuleName, Class, string.Join(",", WeaponMods??new string[] { }) );
                shp.Equip(LoadoutID, SlotName, new SuitLoadout.LoadoutModule(SlotName, SuitModuleID, ModuleName, ModuleName_Localised, Class, WeaponMods));
            }
        }

    }


    [JournalEntryType(JournalTypeEnum.LoadoutRemoveModule)]
    public class JournalLoadoutRemoveModule : JournalEntry, ISuitLoadoutInformation
    {
        public JournalLoadoutRemoveModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutRemoveModule)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names

            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);

            if (ModuleName != null)
            {
                ModuleNameFriendly = ItemData.GetWeapon(ModuleName, ModuleName_Localised)?.Name ?? ModuleName_Localised;
            }

            SlotFriendlyName = SuitLoadout.ToEnglish(SlotName);
        }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // always
        [JsonAlwaysCreate]              
        public FDName SuitName { get; set; }                    // always
        public string SuitName_Localised { get; set; }          // always
        public string SuitFriendlyName { get; set; }           // always

        public ulong LoadoutID { get; set; }                // always
        public string LoadoutName { get; set; }             // always

        public SuitLoadout.SuitSlot SlotName { get; set; }        // always
        public string SlotFriendlyName { get; set; }            // always

        public FDName ModuleName { get; set; }                  
        public string ModuleNameFriendly { get; set; }
        public string ModuleName_Localised { get; set; }
        
        public ulong SuitModuleID { get; set; }         // aka weapon ID

        public int Class { get; set; }        // may not be there
        public string[] WeaponMods { get; set; }    // fdname, may be null or empty

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "<: ", LoadoutName, "<: ", SlotFriendlyName, "< --> ", ModuleNameFriendly);
            
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                var w = weap.weapons.GetLast(SuitModuleID);
                if (w != null && w.Sold == false)
                {
                    shp.VerifyPresence(EventTimeUTC, LoadoutID, LoadoutName, SuitID, null);
                    shp.Remove(LoadoutID, SlotName, w);
                }
                else
                    System.Diagnostics.Debug.WriteLine("No weapon in list found to remove " + SuitModuleID);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.RenameSuitLoadout)]
    public class JournalRenameSuitLoadout : JournalEntry, ISuitLoadoutInformation
    {
        public JournalRenameSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.RenameSuitLoadout)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names

            if (SuitName != null)
            {
                SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
                SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            }
        }

        public ulong SuitID { get; set; } = ulong.MaxValue; // always
        [JsonAlwaysCreate]
        public FDName SuitName { get; set; }                // always, ensure
        public string SuitFriendlyName { get; set; }        // always
        public string SuitName_Localised { get; set; }      // always

        public ulong LoadoutID { get; set; }                // always
        public string LoadoutName { get; set; }             // always

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "<: ==> ", LoadoutName);
            
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.VerifyPresence(EventTimeUTC, LoadoutID, LoadoutName, SuitID, null);
                shp.Rename(LoadoutID, LoadoutName);
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.SwitchSuitLoadout)]
    public class JournalSwitchSuitLoadout : JournalEntry, ISuitInformation, ISuitLoadoutInformation
    {
        public JournalSwitchSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.SwitchSuitLoadout)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names

            if (SuitName != null)
            {
                SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
                SuitName_Localised = JournalFieldNaming.CheckLocalisation(SuitName_Localised, SuitFriendlyName);
            }

            SuitLoadout.NormaliseModules(Modules);
        }
        public ulong LoadoutID { get; set; }                    // always
        public string LoadoutName { get; set; }                 // always

        public ulong SuitID { get; set; } = ulong.MaxValue;     // may not be present
        public FDName SuitName { get; set; }                    // may not be present
        public string SuitName_Localised { get; set; }          // may not be present
        public string SuitFriendlyName { get; set; }            // may not be present
        public FDName[] SuitMods { get; set; }                  // may be null or empty

        public SuitLoadout.LoadoutModule[] Modules;             // may be null or empty

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "< ==> ", LoadoutName);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue && SuitName != null)
            {
                shp.VerifyPresence(EventTimeUTC, SuitID, SuitName, SuitName_Localised, 0, SuitMods);
                shp.SwitchTo(EventTimeUTC, SuitID);
            }
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.VerifyPresence(EventTimeUTC, LoadoutID, LoadoutName, SuitID, Modules);
                shp.SwitchTo(EventTimeUTC, LoadoutID);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.UpgradeSuit)]
    public class JournalUpgradeSuit : JournalEntry, ISuitInformation, ILedgerJournalEntry
    {
        public JournalUpgradeSuit(JObject evt) : base(evt, JournalTypeEnum.UpgradeSuit)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this);        // read fields named in this structure matching JSON names
            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name_Localised = JournalFieldNaming.CheckLocalisation(Name_Localised, FriendlyName);
        }

        public ulong SuitID { get; set; } = ulong.MaxValue;     // may not be present
        [JsonAlwaysCreate]
        public FDName Name { get; set; }                        // always present, ensure
        public string FriendlyName { get; set; }                // always
        public string Name_Localised { get; set; }              // always
        public long Cost { get; set; }                          // always
        public int Class { get; set; }                          // always
        public FDName[] SuitMods { get; set; }                  // may be null or empty

        public override string GetInfo()
        {
            long? p = Cost > 0 ? Cost : default(long?);
            string smod = SuitMods != null ? string.Join(", ", SuitMods.Select(x => Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            return BaseUtils.FieldBuilder.Build("", Name_Localised, "< => ", Class, "Mods".Tx()+": ", smod, "Cost: ; cr;N0".Tx(), p);
            
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Upgrade(EventTimeUTC, SuitID, Name, Class, Cost);
            }
        }
        public void Ledger(Ledger mcl)
        {
            if (Cost != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, -Cost);
        }
    }
}


