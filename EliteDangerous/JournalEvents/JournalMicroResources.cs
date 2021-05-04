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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    public class MicroResource
    {
        public string Name { get; set; }
        public string Name_Localised { get; set; }

        public string FriendlyName { get; set; }                // computed

        public ulong OwnerID { get; set; }                      // Backpackmaterials, ShipLockerMaterials, CollectItems, DropItems
        public ulong MissionID { get; set; }                    // Backpackmaterials, ShipLockerMaterials, DropItems, may be -1, invalid
        public int Count { get; set; }                          // Backpackmaterials, ShipLockerMaterials, CollectItems, DropItems

        public string Category { get; set; }                    // BuyMicroResources, SellMicroResources, TradeMicroresources
        public string Type { get; set; }                        // UseConsumable, CollectItems, DropItems   (Type is Category, normalise moves it across, use Category)

        public enum DirectionType { None, ToShipLocker, ToBackpack }        // must match names in event
        public DirectionType Direction { get; set; }                   // TransferMicroResources.

        public void Normalise()
        {
            Name_Localised = JournalFieldNaming.CheckLocalisation(Name_Localised, Name);
            Name = JournalFieldNaming.FDNameTranslation(Name);
            FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            Category = Category.Alt(Type);      // for some reason category is called type in places..
        }

        static public void Normalise(MicroResource[] a)
        {
            if (a != null)
            {
                foreach (MicroResource m in a)
                    m.Normalise();
            }
        }

        static public string List(MicroResource[] mat)
        {
            StringBuilder sb = new StringBuilder(64);

            foreach (MicroResource m in mat)
            {
                sb.Append(Environment.NewLine);
                sb.Append(BaseUtils.FieldBuilder.Build(" ", m.FriendlyName, "; items".T(EDTx.JournalEntry_items), m.Count));
                if (m.Direction != DirectionType.None)
                    sb.Append("->" + (m.Direction == DirectionType.ToShipLocker ? "Locker".T(EDTx.TBD) : "Backpack".T(EDTx.TBD)));
            }
            return sb.ToString();
        }
    }

    public class JournalMicroResourceState : JournalEntry
    {
        public JournalMicroResourceState(JObject evt, JournalTypeEnum en) : base(evt, en)
        {
            Items = evt["Items"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Items);
            Components = evt["Components"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Components);
            Consumables = evt["Consumables"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Consumables);
            Data = evt["Data"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Data);
        }

        public MicroResource[] Items { get; set; }            
        public MicroResource[] Components { get; set; }
        public MicroResource[] Consumables { get; set; }
        public MicroResource[] Data { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";

            if (Items != null && Items.Length > 0)
            {
                if ( Items.Length>10 )
                    info = BaseUtils.FieldBuilder.Build("Items".T(EDTx.JournalMicroResources_Items) + ":; ", Items.Length);

                detailed = "Items".T(EDTx.JournalMicroResources_Items) + ":" + MicroResource.List(Items) + Environment.NewLine;
            }

            if (Components != null && Components.Length > 0)
            {
                info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Components".T(EDTx.JournalMicroResources_Components) + ":; ", Components.Length), "; ");
                detailed += "Components".T(EDTx.JournalMicroResources_Components) + ":" + MicroResource.List(Components) + Environment.NewLine;
            }

            if (Consumables != null && Consumables.Length > 0)
            {
                if (Items.Length > 10)
                    info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Consumables".T(EDTx.JournalMicroResources_Consumables) + ":; ", Consumables.Length), "; ");
                else
                    info = info.AppendPrePad(string.Join(", ", Consumables.Select(x => x.FriendlyName).ToArray()), "; ");

                detailed += "Consumables".T(EDTx.JournalMicroResources_Consumables) + ":" + MicroResource.List(Consumables) + Environment.NewLine;
            }

            if (Data != null && Data.Length > 0)
            {
                info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Data".T(EDTx.JournalMicroResources_Data) + ":; ", Data.Length), "; ");
                detailed += "Data".T(EDTx.JournalMicroResources_Data) + ":" + MicroResource.List(Data);
            }

        }
    }


    // for now, we only hold list of ship locker materials, not what your currently carrying

    [JournalEntryType(JournalTypeEnum.BackPackMaterials)]
    public class JournalBackPackMaterials : JournalMicroResourceState, IMicroResourceJournalEntry
    {
        public JournalBackPackMaterials(JObject evt) : base(evt, JournalTypeEnum.BackPackMaterials)
        {
        }
        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)    // unused for now
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.ShipLockerMaterials)]
    public class JournalShipLockerMaterials : JournalMicroResourceState, IMicroResourceJournalEntry
    {
        public JournalShipLockerMaterials(JObject evt) : base(evt, JournalTypeEnum.ShipLockerMaterials)
        {
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            //mc.Clear(0, MaterialCommodityMicroResourceType.CatType.Item, MaterialCommodityMicroResourceType.CatType.Component,
            //                        MaterialCommodityMicroResourceType.CatType.Consumable, MaterialCommodityMicroResourceType.CatType.Data);

            //if (Items != null)
            //{
            //    foreach (var i in Items)
            //    {
            //        MaterialCommodityMicroResourceType.EnsurePresent("Item", i.Name, i.Name_Localised);
            //        mc.Set(MaterialCommodityMicroResourceType.CatType.Item, i.Name, i.Count, 0);
            //    }
            //}

            //if (Components != null)
            //{
            //    foreach (var i in Components)
            //    {
            //        MaterialCommodityMicroResourceType.EnsurePresent("Component", i.Name, i.Name_Localised);
            //        mc.Set(MaterialCommodityMicroResourceType.CatType.Component, i.Name, i.Count, 0);
            //    }
            //}

            //if (Consumables != null)
            //{
            //    foreach (var i in Consumables)
            //    {
            //        MaterialCommodityMicroResourceType.EnsurePresent("Consumables", i.Name, i.Name_Localised);
            //        mc.Set(MaterialCommodityMicroResourceType.CatType.Consumable, i.Name, i.Count, 0);

            //    }
            //}

            //if (Data != null)
            //{
            //    foreach (var i in Data)
            //    {
            //        MaterialCommodityMicroResourceType.EnsurePresent("Data", i.Name, i.Name_Localised);
            //        mc.Set(MaterialCommodityMicroResourceType.CatType.Data, i.Name, i.Count, 0);
            //    }
            //}
        }
    }

    [JournalEntryType(JournalTypeEnum.BuyMicroResources)]
    public class JournalBuyMicroResources : JournalEntry, IMicroResourceJournalEntry, ILedgerJournalEntry
    {
        public JournalBuyMicroResources(JObject evt) : base(evt, JournalTypeEnum.BuyMicroResources)
        {
            evt.ToObjectProtected(Resource.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise();
            Price = evt["Price"].Int();
            MarketID = evt["MarketID"].Long();
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public int Price { get; set; }
        public long MarketID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Resource.FriendlyName, "", Resource.Count, "< buy price ; cr;N0".T(EDTx.JournalEntry_buyprice), Price);
            detailed = "";
        }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Resource.FriendlyName + " " + Resource.Count, -Price);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            MaterialCommodityMicroResourceType.EnsurePresent(Resource.Category, Resource.Name, Resource.Name_Localised);
            mc.Change(EventTimeUTC, Resource.Category, Resource.Name, Resource.Count, Price);
        }
    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.CollectItems)]
    public class JournalCollectItems : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalCollectItems(JObject evt) : base(evt, JournalTypeEnum.CollectItems)
        {
            evt.ToObjectProtected(Resource.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise();
            Stolen = evt["Stolen"].Bool();
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public bool Stolen { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Resource.Name);
            info = BaseUtils.FieldBuilder.Build("", Resource.FriendlyName, "< (", mcd.TranslatedCategory, "< ; items".T(EDTx.JournalEntry_MatC), Resource.Count, ";Stolen".T(EDTx.TBD), Stolen);
            detailed = "";
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            MaterialCommodityMicroResourceType.EnsurePresent(Resource.Category, Resource.Name, Resource.Name_Localised);
            mc.Change(EventTimeUTC, Resource.Category, Resource.Name, Resource.Count, 0);
        }
    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.DropItems)]
    public class JournalDropItems : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalDropItems(JObject evt) : base(evt, JournalTypeEnum.DropItems)
        {
            evt.ToObjectProtected(Resource.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise();
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Resource.Name);
            info = BaseUtils.FieldBuilder.Build("", Resource.FriendlyName, "< (;)", mcd.TranslatedCategory, "< ; items".T(EDTx.JournalEntry_MatC), Resource.Count);
            detailed = "";
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            MaterialCommodityMicroResourceType.EnsurePresent(Resource.Category, Resource.Name, Resource.Name_Localised);
            mc.Change(EventTimeUTC, Resource.Category, Resource.Name, -Resource.Count, 0);
        }
    }

    [JournalEntryType(JournalTypeEnum.SellMicroResources)]
    public class JournalSellMicroResources : JournalEntry, IMicroResourceJournalEntry, ILedgerJournalEntry
    {
        public JournalSellMicroResources(JObject evt) : base(evt, JournalTypeEnum.SellMicroResources)
        {
            Items = evt["MicroResources"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Items);
            Price = evt["Price"].Long();
            MarketID = evt["MarketID"].Long();
        }

        public MicroResource[] Items { get; set; } = null;
        public long Price { get; set; }
        public long MarketID { get; set; }

         public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";

            if (Items != null && Items.Length > 0)
            {
                info += BaseUtils.FieldBuilder.Build("Items".T(EDTx.JournalMicroResources_Items) + ":; ", Items.Length, "< sell price ; cr;N0".T(EDTx.JournalEntry_sellprice), Price);
                detailed += "Items".T(EDTx.JournalMicroResources_Items) + ":" + MicroResource.List(Items);
            }
        }

        public void Ledger(Ledger mcl)
        {
            if (Items != null)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Items.Length + " " + Price + "cr", Price);
            }
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            if (Items != null )
            {
                foreach (var m in Items)
                {
                    MaterialCommodityMicroResourceType.EnsurePresent(m.Category, m.Name, m.Name_Localised);
                    mc.Change(EventTimeUTC, m.Category, m.Name, -m.Count, 0);
                }
            }
        }
    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.TradeMicroResources)]
    public class JournalTradeMicroResources : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalTradeMicroResources(JObject evt) : base(evt, JournalTypeEnum.TradeMicroResources)
        {
            Offered = evt["Offered"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Offered);
            Received = evt["Received"].Str();
            Received_Localised = evt["Received_Localised"].Str();
            Category = evt["Category"].Str();
            Count = evt["Count"].Int();
            MarketID = evt["MarketID"].Long();

            Received_Localised = JournalFieldNaming.CheckLocalisation(Received_Localised, Received);
            Received = JournalFieldNaming.FDNameTranslation(Received);
            Received_FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Received);
        }

        public MicroResource[] Offered { get; set; }
        public string Received { get; set; }
        public string Received_Localised { get; set; }
        public string Received_FriendlyName { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
        public long MarketID { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build(" ", Received_FriendlyName, "; items".T(EDTx.JournalEntry_items), Count);
            detailed = MicroResource.List(Offered);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            //TBD
        }
    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.TransferMicroResources)]
    public class JournalTransferMicroResources : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalTransferMicroResources(JObject evt) : base(evt, JournalTypeEnum.TransferMicroResources)
        {
            Items = evt["Transfers"]?.ToObjectQ<MicroResource[]>()?.OrderBy(x => x.Name)?.ToArray();
            MicroResource.Normalise(Items);
        }

        public MicroResource[] Items { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = "";
            detailed = "";

            if (Items != null && Items.Length > 0)
            {
                if (Items.Length > 10)
                    info += BaseUtils.FieldBuilder.Build("Count".T(EDTx.JournalEntry_Count) + ":; ", Items.Length);
                else
                    info += string.Join(", ", Items.Select(x => x.FriendlyName).ToArray());

                detailed += MicroResource.List(Items);
            }
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)    // unused for now
        {
        }
    }

    // TBD Test
    [JournalEntryType(JournalTypeEnum.UseConsumable)]
    public class JournalUseConsumable : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalUseConsumable(JObject evt) : base(evt, JournalTypeEnum.UseConsumable)
        {
            evt.ToObjectProtected(Resource.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise();
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = Resource.FriendlyName;
            detailed = "";
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc)
        {
            MaterialCommodityMicroResourceType.EnsurePresent(Resource.Category, Resource.Name, Resource.Name_Localised);
            mc.Change(EventTimeUTC, Resource.Category, Resource.Name, -1, 0);
        }
    }

}


