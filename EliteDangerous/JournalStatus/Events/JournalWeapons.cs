/*
 * Copyright © 2021-2023 EDDiscovery development team
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
 *
 */
using QuickJSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{

    [JournalEntryType(JournalTypeEnum.BuyWeapon)]
    public class JournalBuyWeapon : JournalEntry, IWeaponInformation, ILedgerJournalEntry
    {
        public JournalBuyWeapon(JObject evt) : base(evt, JournalTypeEnum.BuyWeapon)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, 
                    initialobject: this);        // read fields named in this structure matching JSON names

            FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
        }

        [JsonAlwaysCreate]
        public WeaponID SuitModuleID { get; set; }    // may be missing
        public FDName Name { get; set; }                    // always there, force always there
        public string FriendlyName { get; set; }
        public string Name_Localised { get; set; }

        public long Price { get; set; }                     // always ther
        public int Class { get; set; } = 1;                 // missing early ones, presume
        public FDName[] WeaponMods { get; set; }            // may be null/empty

        public override string GetInfo()
        {
            string wmod = WeaponMods != null ? string.Join(", ", WeaponMods.Select(x => Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            return BaseUtils.FieldBuilder.Build("", FriendlyName, "Class".Tx()+": ", Class, "Mods".Tx()+": ", wmod, "Cost: ; cr;N0".Tx(), Price);
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID.IsValid)
            {
                shp.Buy(EventTimeUTC, SuitModuleID, Name, Name_Localised, Price, Class, WeaponMods);    
            }
        }
        public void Ledger(Ledger mcl)
        {
            if (Price != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, -Price);
        }

    }

    [JournalEntryType(JournalTypeEnum.SellWeapon)]
    public class JournalSellWeapon : JournalEntry, IWeaponInformation, ILedgerJournalEntry
    {
        public JournalSellWeapon(JObject evt) : base(evt, JournalTypeEnum.SellWeapon)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    initialobject: this);        // read fields named in this structure matching JSON names

            FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
        }

        [JsonAlwaysCreate]
        public WeaponID SuitModuleID { get; set; }    // may be missing

        [JsonAlwaysCreate]
        public FDName Name { get; set; }                // always there, force in case
        public string FriendlyName { get; set; }
        public string Name_Localised { get; set; }

        public long Price { get; set; }                 // always there
        public int Class { get; set; }                  // always there
        public string[] WeaponMods { get; set; }        // may be null/empty


        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", FriendlyName, "< sell price ; cr;N0".Tx(), Price);
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID.IsValid)
            {
                shp.Sell(EventTimeUTC, SuitModuleID);
            }
        }
        public void Ledger(Ledger mcl)
        {
            if (Price != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, Price);
        }


    }

    [JournalEntryType(JournalTypeEnum.UpgradeWeapon)]
    public class JournalUpgradeWeapon : JournalEntry, IWeaponInformation, ILedgerJournalEntry
    {
        public JournalUpgradeWeapon(JObject evt) : base(evt, JournalTypeEnum.UpgradeWeapon)
        {
            // Limit search to this class only using DeclaredOnly.
            evt.ToObjectProtected(this.GetType(), true, 
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    initialobject: this);        // read fields named in this structure matching JSON names

            FriendlyName = ItemData.GetWeapon(Name, Name_Localised)?.Name ?? Name_Localised;
        }

        [JsonAlwaysCreate]
        public WeaponID SuitModuleID { get; set; }    // may be missing

        [JsonAlwaysCreate]
        public FDName Name { get; set; }                    // always there, force in case
        public string FriendlyName { get; set; }
        public string Name_Localised { get; set; }

        public long Cost { get; set; }                  // always there
        public int Class { get; set; }                  // always there
        public FDName[] WeaponMods { get; set; }        // may be null or empty

        public override string GetInfo()
        {
            string wmod = WeaponMods != null ? string.Join(", ", WeaponMods.Select(x => Recipes.GetBetterNameForEngineeringRecipe(x))) : null;
            long? p = Cost > 0 ? Cost : default(long?);
            return BaseUtils.FieldBuilder.Build("", FriendlyName, "< => " + "Class".Tx()+": ", Class, "Mods".Tx()+": ", wmod, "Cost: ; cr;N0".Tx(), p);
        }

        public void WeaponInformation(SuitWeaponList shp, string whereami, ISystem system)
        {
            if (SuitModuleID.IsValid)
            {
                shp.Upgrade(EventTimeUTC, SuitModuleID, Class, WeaponMods);
            }
        }
        public void Ledger(Ledger mcl)
        {
            if (Cost != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised, -Cost);
        }
}

}


