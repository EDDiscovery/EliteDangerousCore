/*
 * Copyright © 2021-2021 EDDiscovery development team
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
using BaseUtils.JSON;
using System;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.BuySuit)]
    public class JournalBuySuit : JournalEntry, ISuitInformation
    {
        public JournalBuySuit(JObject evt) : base(evt, JournalTypeEnum.BuySuit)
        {
            SuitID = ulong.MaxValue;        // pre alpha 4 this was missing.
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name = Name.ToLower(); // normalise
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< buy price ; cr;N0".T(EDTx.JournalEntry_buyprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Buy(EventTimeUTC, SuitID, Name, Name_Localised, Price);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.SellSuit)]
    public class JournalSellSuit : JournalEntry, ISuitInformation, ISuitLoadoutInformation
    {
        public JournalSellSuit(JObject evt) : base(evt, JournalTypeEnum.SellSuit)
        {
            SuitID = ulong.MaxValue;        // pre alpha 4 this was missing
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name = Name.ToLower(); // normalise
        }

        public JournalSellSuit(DateTime utc, ulong id, string fdname, string locname, long price, int cmdrid) : base(utc,JournalTypeEnum.SellSuit,false)
        {
            SuitID = id; Name = fdname; Name_Localised = locname; price = Price;
            SetCommander(cmdrid);
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< sell price ; cr;N0".T(EDTx.JournalEntry_sellprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Sell(EventTimeUTC, SuitID);
            }
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
            evt["Name"] = Name;
            evt["Name_Localised"] = Name_Localised;
            evt["Price"] = Price;
            return evt;
        }

    }

    [JournalEntryType(JournalTypeEnum.CreateSuitLoadout)]
    public class JournalCreateSuitLoadout : JournalEntry, ISuitLoadoutInformation
    {
        public JournalCreateSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.CreateSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names

            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise

            SuitLoadout.NormaliseModules(Modules);
          
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public string LoadoutName { get; set; }
        public ulong LoadoutID { get; set; }

        public SuitLoadout.LoadoutModule[] Modules;

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "< ++> ", LoadoutName);
            detailed = "";
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
    public class JournalSuitLoadout : JournalEntry, ISuitLoadoutInformation
    {
        public JournalSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.SuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names

            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise

            SuitLoadout.NormaliseModules(Modules);

        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public string LoadoutName { get; set; }
        public ulong LoadoutID { get; set; }

        public SuitLoadout.LoadoutModule[] Modules;

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "< ++> ", LoadoutName);
            detailed = "";
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.CreateLoadout(EventTimeUTC, LoadoutID, LoadoutName, SuitID, Modules);
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.DeleteSuitLoadout)]
    public class JournalDeleteSuitLoadout : JournalEntry, ISuitLoadoutInformation
    {
        public JournalDeleteSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.DeleteSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public string LoadoutName { get; set; }
        public ulong LoadoutID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "< --> ", LoadoutName);
            detailed = "";
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.DeleteLoadout(EventTimeUTC, LoadoutID);
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.LoadoutEquipModule)]
    public class JournalLoadoutEquipModule : JournalEntry, ISuitLoadoutInformation
    {
        public JournalLoadoutEquipModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutEquipModule)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names

            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise
            ModuleNameFriendly = ItemData.GetWeapon(ModuleName, ModuleName_Localised)?.Name ?? ModuleName_Localised;
            SlotFriendlyName = SlotName.SplitCapsWordFull();
            SlotName = SlotName.ToLower();
            ModuleName = ModuleName.ToLower();
        }

        public string LoadoutName { get; set; }
        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public ulong LoadoutID { get; set; }
        public string SlotName { get; set; }        // lower normalised
        public string SlotFriendlyName { get; set; }
        public string ModuleName { get; set; }      // lower normalised
        public string ModuleName_Localised { get; set; }
        public string ModuleNameFriendly { get; set; }
        public ulong SuitModuleID { get; set; }         // aka weapon ID

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "<: ", LoadoutName, "<: ", SlotFriendlyName, "< ++> ", ModuleNameFriendly);
            detailed = "";
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                System.Diagnostics.Debug.WriteLine("{0} Equip suit {1} Loadout {2} slot {3} with {4}", EventTimeUTC.ToString(), SuitID, LoadoutID, SlotName, ModuleName);
                shp.Equip(LoadoutID, SlotName, new SuitLoadout.LoadoutModule(SlotName, SuitModuleID, ModuleName, ModuleName_Localised));
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.LoadoutRemoveModule)]
    public class JournalLoadoutRemoveModule : JournalEntry, ISuitLoadoutInformation
    {
        public JournalLoadoutRemoveModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutRemoveModule)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise
            ModuleNameFriendly = ItemData.GetWeapon(ModuleName, ModuleName_Localised)?.Name ?? ModuleName_Localised;
            SlotFriendlyName = SlotName.SplitCapsWordFull();
            SlotName = SlotName.ToLower();
            ModuleName = ModuleName.ToLower();
        }

        public string LoadoutName { get; set; }
        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public ulong LoadoutID { get; set; }
        public string SlotName { get; set; }        // lower normalised
        public string SlotFriendlyName { get; set; }
        public string ModuleName { get; set; }      // lower normalised
        public string ModuleNameFriendly { get; set; }
        public string ModuleName_Localised { get; set; }
        public ulong SuitModuleID { get; set; }         // aka weapon ID

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "<: ", LoadoutName, "<: ", SlotFriendlyName, "< --> ", ModuleNameFriendly);
            detailed = "";
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                var w = weap.Weapons.GetLast(SuitModuleID);
                if (w != null && w.Sold == false)
                {
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
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "<: ==> ", LoadoutName);
            detailed = "";
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.Rename(LoadoutID, LoadoutName);
            }
        }

    }

    [JournalEntryType(JournalTypeEnum.SwitchSuitLoadout)]
    public class JournalSwitchSuitLoadout : JournalEntry, ISuitInformation, ISuitLoadoutInformation
    {
        public JournalSwitchSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.SwitchSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            SuitFriendlyName = ItemData.GetSuit(SuitName, SuitName_Localised)?.Name ?? SuitName_Localised;
            SuitName = SuitName.ToLower(); // normalise
            SuitLoadout.NormaliseModules(Modules);
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string SuitFriendlyName { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public SuitLoadout.LoadoutModule[] Modules;

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", SuitFriendlyName, "< ==> ", LoadoutName);
            detailed = "";
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.SwitchTo(EventTimeUTC, SuitID);
            }
        }

        public void LoadoutInformation(SuitLoadoutList shp, SuitWeaponList weap, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.SwitchTo(EventTimeUTC, LoadoutID);
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.UpgradeSuit)]
    public class JournalUpgradeSuit : JournalEntry, ISuitInformation
    {
        public JournalUpgradeSuit(JObject evt) : base(evt, JournalTypeEnum.UpgradeSuit)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            FriendlyName = ItemData.GetSuit(Name, Name_Localised)?.Name ?? Name_Localised;
            Name = Name.ToLower(); // normalise
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public int Class { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "TBD awaiting journal record please submit";
            detailed = "";
        }

        public void SuitInformation(SuitList shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
            }
        }


    }

}


