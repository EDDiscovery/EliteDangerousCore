﻿/*
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

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Repair)]
    public class JournalRepair : JournalEntry, ILedgerJournalEntry
    {
        public JournalRepair(JObject evt ) : base(evt, JournalTypeEnum.Repair)
        {
            if (evt["Items"] is JArray)
            {
                Items = new List<RepairItem>();

                ItemLocalised = Item = ItemFD = "";

                foreach (var jitem in evt["Items"])
                {
                    var itemfd = JournalFieldNaming.NormaliseFDItemName(jitem.Str());
                    var item = JournalFieldNaming.GetBetterEnglishModuleName(itemfd);

                    var repairitem = new RepairItem
                    {
                        ItemFD = itemfd,
                        Item = item,
                        ItemLocalised = JournalFieldNaming.RepairType(item)
                    };

                    ItemLocalised = ItemLocalised.AppendPrePad(repairitem.ItemLocalised, ", "); // for the voice pack, keep this going
 
                    Items.Add(repairitem);
                }
            }
            else
            {
                ItemFD = JournalFieldNaming.NormaliseFDItemName(evt["Item"].Str());
                Item = JournalFieldNaming.GetBetterEnglishModuleName(ItemFD);
                ItemLocalised = JournalFieldNaming.CheckLocalisation(evt["Item_Localised"].Str(),Item);
            }

            Cost = evt["Cost"].Long();
        }

        public class RepairItem
        {
            public string Item { get; set; }        // English name
            public string ItemFD { get; set; }
            public string ItemLocalised { get; set; }
        }

        public string ItemFD { get; set; }      // older style ones, OR first entry of new ones
        public string Item { get; set; }
        public string ItemLocalised { get; set; }

        public List<RepairItem> Items { get; set; }
        public long Cost { get; set; }

        public void Ledger(Ledger mcl)
        {
            if ( Cost != 0)
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, JournalFieldNaming.GetForeignModuleName(ItemFD, ItemLocalised), -Cost);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", JournalFieldNaming.GetForeignModuleName(ItemFD,ItemLocalised), "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost) , Cost );
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
            return BaseUtils.FieldBuilder.Build("Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Cost);
        }
    }


    [JournalEntryType(JournalTypeEnum.AfmuRepairs)]
    public class JournalAfmuRepairs : JournalEntry
    {
        public JournalAfmuRepairs(JObject evt) : base(evt, JournalTypeEnum.AfmuRepairs)
        {
            ModuleFD = JournalFieldNaming.NormaliseFDItemName(evt["Module"].Str());
            Module = JournalFieldNaming.GetBetterEnglishModuleName(ModuleFD);
            ModuleLocalised = JournalFieldNaming.CheckLocalisation(evt["Module_Localised"].Str(), Module);
            FullyRepaired = evt["FullyRepaired"].Bool();
            Health = evt["Health"].Float() * 100.0F;
        }

        public string Module { get; set; }  // english
        public string ModuleFD { get; set; }
        public string ModuleLocalised { get; set; }
        public bool FullyRepaired { get; set; }
        public float Health { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", JournalFieldNaming.GetForeignModuleName(ModuleFD, ModuleLocalised), "Health: ;%", (int)Health, ";Fully Repaired", FullyRepaired);
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
