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
using QuickJSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{

    [JournalEntryType(JournalTypeEnum.BuyWeapon)]
    public class JournalBuyWeapon : JournalEntry, IWeaponInformation
    {
        public JournalBuyWeapon(JObject evt) : base(evt, JournalTypeEnum.BuyWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            Class = 1; // presume
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names

            if (Name.HasChars())
            {
                FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
                Name = Name.ToLowerInvariant(); // normalise
            }
        }

        public ulong SuitModuleID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Price { get; set; }
        public int Class { get; set; }
        public string[] WeaponMods { get; set; }    // may be null/empty

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            string wmod = WeaponMods != null ? string.Join(", ", WeaponMods.Select(x => Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            info = BaseUtils.FieldBuilder.Build("", FriendlyName, "Class: ".T(EDCTx.JournalEntry_Class), Class, "Mods: ".T(EDCTx.JournalEntry_Mods), wmod, "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Price);
            detailed = "";
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {
                shp.Buy(EventTimeUTC, SuitModuleID, Name, Name_Localised, Price, Class, WeaponMods);    
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.SellWeapon)]
    public class JournalSellWeapon : JournalEntry, IWeaponInformation
    {
        public JournalSellWeapon(JObject evt) : base(evt, JournalTypeEnum.SellWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            if (Name.HasChars())
            {
                FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
                Name = Name.ToLowerInvariant(); // normalise
            }
        }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Price { get; set; }
        public ulong SuitModuleID { get; set; }
        public int Class { get; set; }
        public string[] WeaponMods { get; set; }    // may be null/empty


        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< sell price ; cr;N0".T(EDCTx.JournalEntry_sellprice), Price);
            detailed = "";
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {
                shp.Sell(EventTimeUTC, SuitModuleID);
            }
        }

    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.UpgradeWeapon)]
    public class JournalUpgradeWeapon : JournalEntry, IWeaponInformation
    {
        public JournalUpgradeWeapon(JObject evt) : base(evt, JournalTypeEnum.UpgradeWeapon)
        {
            SuitModuleID = ulong.MaxValue;
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names
            if (Name.HasChars())
            {
                FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
            }
        }

        public ulong SuitModuleID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string FriendlyName { get; set; }
        public long Cost { get; set; }
        public int Class { get; set; }
        public string[] WeaponMods { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            string wmod = WeaponMods != null ? string.Join(", ", WeaponMods.Select(x => Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            long? p = Cost > 0 ? Cost : default(long?);
            info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< => " + "Class: ".T(EDCTx.JournalEntry_Class), Class, "Mods: ".T(EDCTx.JournalEntry_Mods), wmod, "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), p);
            detailed = "";
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID != ulong.MaxValue)
            {
                shp.Upgrade(EventTimeUTC, SuitModuleID, Class, WeaponMods);
            }
        }
    }

}


