/*
 * Copyright © 2016-2023 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Repair)]
    public class JournalRepair : JournalEntry, ILedgerJournalEntry
    {
        public JournalRepair(JObject evt ) : base(evt, JournalTypeEnum.Repair)
        {
            Items = new List<RepairItem>();

            if (evt["Items"] is JArray)
            {
                foreach (var jitem in evt["Items"])
                {
                    var ModuleFD = FDNameHelpers.NormaliseModules(jitem.Str(), out string engname, this);
                    Items.Add(new RepairItem() { ItemFD = ModuleFD, Item = engname, ItemLocalised = engname });
                }
            }
            else
            {
                var ModuleFD = FDNameHelpers.NormaliseModules(evt["Item"].Str(), out string engname, this);
                Items.Add(new RepairItem() { ItemFD = ModuleFD, Item = engname, ItemLocalised = JournalFieldNaming.CheckLocalisation(evt["Item_Localised"].Str(), engname) });
            }

            Cost = evt["Cost"].Long();
        }

        public class RepairItem
        {
            public string Item { get; set; }        // English name
            public FDName ItemFD { get; set; }
            public string ItemLocalised { get; set; }
        }
        public List<RepairItem> Items { get; set; }

        // For the voice pack keep these on first entry
        public FDName ItemFD => Items.Count > 0 ? Items[0].ItemFD : FDName.Empty;
        public string Item => Items.Count > 0 ? Items[0].Item : "Unknown";
        public string ItemLocalised => Items.Count > 0 ? Items[0].ItemLocalised : "Unknown";

        public long Cost { get; set; }

        public void Ledger(Ledger mcl)
        {
            if ( Cost != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, ItemFD.GetForeignModuleName(ItemLocalised), -Cost);
        }

        public override string GetInfo()
        {
            if (Items.Count > 1)
                return BaseUtils.FieldBuilder.Build("Repaired: ", Items.Count, "Cost: ; cr;N0".Tx(), Cost);
            else
                return BaseUtils.FieldBuilder.Build("", ItemFD.GetForeignModuleName(ItemLocalised), "Cost: ; cr;N0".Tx(), Cost);
        }
        public override string GetDetailed()
        {
            StringBuilder sb = new StringBuilder();
            foreach( var item in Items)
            {
                sb.Append(item.ItemFD.GetForeignModuleName(item.ItemLocalised));
                sb.AppendCR();
            }

            return sb.ToString() + BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".Tx(), Cost);
        }
    }


    [JournalEntryType(JournalTypeEnum.RepairAll)]
    public class JournalRepairAll : JournalEntry, ILedgerJournalEntry
    {
        public JournalRepairAll(JObject evt) : base(evt, JournalTypeEnum.RepairAll)
        {
            Cost = evt["Cost"].Long();
        }

        public long Cost { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "", -Cost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".Tx(), Cost);
        }
    }


    [JournalEntryType(JournalTypeEnum.AfmuRepairs)]
    public class JournalAfmuRepairs : JournalEntry
    {
        public JournalAfmuRepairs(JObject evt) : base(evt, JournalTypeEnum.AfmuRepairs)
        {
            ModuleFD = FDNameHelpers.NormaliseModules(evt["Module"].Str(), out string engname, this);
            Module = engname;
            ModuleLocalised = JournalFieldNaming.CheckLocalisation(evt["Module_Localised"].Str(), Module);
            FullyRepaired = evt["FullyRepaired"].Bool();
            Health = evt["Health"].Float() * 100.0F;
        }

        public string Module { get; set; }  // english
        public FDName ModuleFD { get; set; }
        public string ModuleLocalised { get; set; }
        public bool FullyRepaired { get; set; }
        public float Health { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", ModuleFD.GetForeignModuleName(ModuleLocalised), "Health: ;%", (int)Health, ";Fully Repaired", FullyRepaired);
        }
    }

    [JournalEntryType(JournalTypeEnum.RebootRepair)]
    public class JournalRebootRepair : JournalEntry
    {
        public JournalRebootRepair(JObject evt) : base(evt, JournalTypeEnum.RebootRepair)
        {
            Slots = evt["Modules"]?.ToObject<ShipSlots.Slot[]>(false,process:(t,x)=> 
            {
                return ShipSlots.ToEnum(x);
            });

            if (Slots != null)
            {
                FriendlySlots = new string[Slots.Length];
                for (int i = 0; i < Slots.Length; i++)
                    FriendlySlots[i] = ShipSlots.ToEnglish(Slots[i]);
            }
        }

        public ShipSlots.Slot[] Slots { get; set; }            
        public string[] FriendlySlots { get; set; }     // English name

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (FriendlySlots != null)
            {
                for (int i = 0; i < FriendlySlots.Length; i++)
                {
                    sb.AppendPrePad(ShipSlots.ToLocalisedLanguage(Slots[i]), ", ");
                }
            }

            return sb.ToString();
        }
    }


    [JournalEntryType(JournalTypeEnum.SystemsShutdown)]
    public class JournalSystemsShutdown : JournalEntry
    {
        public JournalSystemsShutdown(JObject evt) : base(evt, JournalTypeEnum.SystemsShutdown) { }
    }

}
