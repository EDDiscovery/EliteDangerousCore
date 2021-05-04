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
using BaseUtils.JSON;
using System;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.BuySuit)]
    public class JournalBuySuit : JournalEntry, ISuitInformation
    {
        public JournalBuySuit(JObject evt) : base(evt, JournalTypeEnum.BuySuit)
        {
            SuitID = ulong.MaxValue;        // pre alpha 4 this was missing.
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Name_Localised, "< buy price ; cr;N0".T(EDTx.JournalEntry_buyprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.BuySuit(SuitID, Name, Name_Localised, Price);
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.SellSuit)]
    public class JournalSellSuit : JournalEntry, ISuitInformation
    {
        public JournalSellSuit(JObject evt) : base(evt, JournalTypeEnum.SellSuit)
        {
            SuitID = ulong.MaxValue;        // pre alpha 4 this was missing
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Name_Localised, "< sell price ; cr;N0".T(EDTx.JournalEntry_sellprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {
                shp.SellSuit(SuitID);
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.BuyWeapon)]
    public class JournalBuyWeapon : JournalEntry, ISuitInformation
    {
        public JournalBuyWeapon(JObject evt) : base(evt, JournalTypeEnum.BuyWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitModuleID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Name_Localised, "< buy price ; cr;N0".T(EDTx.JournalEntry_buyprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {
                shp.BuyWeapon(SuitModuleID, Name, Name_Localised, Price);
            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.SellWeapon)]
    public class JournalSellWeapon : JournalEntry, ISuitInformation
    {
        public JournalSellWeapon(JObject evt) : base(evt, JournalTypeEnum.SellWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public long Price { get; set; }
        public ulong SuitModuleID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Name_Localised, "< sell price ; cr;N0".T(EDTx.JournalEntry_sellprice), Price);
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {
                shp.SellWeapon(SuitModuleID);
            }
        }

    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.CreateSuitLoadout)]
    public class JournalCreateSuitLoadout : JournalEntry, ISuitInformation
    {
        public JournalCreateSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.CreateSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string LoadoutName { get; set; }
        public ulong LoadoutID { get; set; }

        // TBD Modules

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", LoadoutName);       //TBD
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }

    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.DeleteSuitLoadout)]
    public class JournalDeleteSuitLoadout : JournalEntry, ISuitInformation
    {
        public JournalDeleteSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.DeleteSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public string LoadoutName { get; set; }
        public ulong LoadoutID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", LoadoutName);       //TBD
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }
    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.LoadoutEquipModule)]
    public class JournalLoadoutEquipModule : JournalEntry, ISuitInformation
    {
        public JournalLoadoutEquipModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutEquipModule)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleName_Localised { get; set; }
        public ulong SuitModuleID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";  // tbd
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }

    }

    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.LoadoutRemoveModule)]
    public class JournalLoadoutRemoveModule : JournalEntry, ISuitInformation
    {
        public JournalLoadoutRemoveModule(JObject evt) : base(evt, JournalTypeEnum.LoadoutRemoveModule)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleName_Localised { get; set; }
        public ulong SuitModuleID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }
    }
    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.RenameSuitLoadout)]
    public class JournalRenameSuitLoadout : JournalEntry, ISuitInformation
    {
        public JournalRenameSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.RenameSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }


        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = BaseUtils.FieldBuilder.Build("<to ".T(EDTx.JournalEntry_to), "");
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }

    }
    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.SwitchSuitLoadout)]
    public class JournalSwitchSuitLoadout : JournalEntry, ISuitInformation
    {
        public JournalSwitchSuitLoadout(JObject evt) : base(evt, JournalTypeEnum.SwitchSuitLoadout)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string SuitName { get; set; }
        public string SuitName_Localised { get; set; }
        public ulong LoadoutID { get; set; }
        public string LoadoutName { get; set; }
        // TBD Modules

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }

    }
    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.UpgradeSuit)]
    public class JournalUpgradeSuit : JournalEntry, ISuitInformation
    {
        public JournalUpgradeSuit(JObject evt) : base(evt, JournalTypeEnum.UpgradeSuit)
        {
            SuitID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public int Class { get; set; }
        public long Price { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitID != ulong.MaxValue)
            {

            }
        }


    }
    // TBD Write, Test
    [JournalEntryType(JournalTypeEnum.UpgradeWeapon)]
    public class JournalUpgradeWeapon : JournalEntry, ISuitInformation
    {
        public JournalUpgradeWeapon(JObject evt) : base(evt, JournalTypeEnum.UpgradeWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
        }

        public ulong SuitModuleID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public int Class { get; set; }
        public long Cost { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";
        }

        public void SuitInformation(SuitWeaponsLoadout shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {

            }
        }
    }

}


