﻿/*
 * Copyright 2021-2024 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    #region Class helpers

    // this class holds a microresource, read from the json

    public class MicroResource
    {
        public const int ShipLocker = 0;                        // index into MCMRList for types
        public const int BackPack = 1;

        public string Name { get; set; }                        // JSON, normalised to lower case, All
        public string Name_Localised { get; set; }              // JSON, All

        public ulong OwnerID { get; set; }                      // JSON, ShipLockerMaterials          |, CollectItems, DropItems
        public ulong MissionID { get; set; }                    // JSON, ShipLockerMaterials          |, DropItems, may be -1, invalid

        public int Count { get; set; }                          // JSON, ShipLockerMaterials, BuyMicroResource, SellMicroResource, TradeMicroResources

        [JsonNameAttribute("Type", "Category")]                 // for some crazy reason, they use type someplaces, category others. Use json name to allow both
        public string Category { get; set; }                    // JSON, BuyMicroResources, SellMicroResource, TradeMicroResources, TransferMicroResources. These call it type: BackPackChange, UseConsumable, CollectItems, DropItems   

        public string GetFriendlyName() { return MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);  }

        public void Normalise(string cat)
        {
            if (Name.HasChars())
            {
                //Name_Localised = JournalFieldNaming.CheckLocalisation(Name_Localised, Name);      // don't bother, its not used
                Name = Name.ToLowerInvariant();      // lower case it
                if (cat != null)
                    Category = cat;
            }
            else
            {
                Name = Name_Localised = "Missing Microresource Name - report";
                Category = "ERROR";
                System.Diagnostics.Trace.WriteLine("Microresource journal without Name detected");
            }
        }

        static public void Normalise(MicroResource[] a, string cat)
        {
            foreach (MicroResource m in a.EmptyIfNull())
                m.Normalise(cat);
        }

        static public void List(StringBuilder sb, MicroResource[] mat, string prefix = " ")
        {
            foreach (MicroResource m in mat.EmptyIfNull())
            {
                sb.Append(BaseUtils.FieldBuilder.Build(prefix, m.GetFriendlyName(), "; items".T(EDCTx.JournalEntry_items), m.Count));
                sb.AppendCR();
            }
        }
    }

    // baseclass for backpack, shiplocker, resupply.

    public class JournalMicroResourceState : JournalEntry
    {
        public JournalMicroResourceState(JObject evt, JournalTypeEnum en) : base(evt, en)
        {
            Rescan(evt);
        }

        public void Rescan(JObject evt)
        {
            // these collect Name, Name_Localised, MissionID, OwnerID, Count
            
            Items = evt["Items"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Items, "Items");
            Components = evt["Components"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Components, "Components");
            Consumables = evt["Consumables"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Consumables, "Consumables");
            Data = evt["Data"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Data, "Data");
        }

        // helper function for IAdditionalFiles
        // we look for the file with the eventype fileeventtype present
        // we store back with recordeventtype
        // when resupply is written, its resupply in the journal, backpack in the json of the file. we need to make sure its resupply in the json after reading

        public void UpdateFromAdditionalFile(string file, string fileeventtype, string recordeventtype)
        {
            if (Items == null || Components == null || Consumables == null || Data == null)     // if any null, try the file, otherwise its in the event
            {
                System.Diagnostics.Debug.WriteLine("{0} MCMR from file ", EventTimeUTC.ToString());
                JObject jnew = ReadAdditionalFile(file, fileeventtype);
                if (jnew != null)        // new json, rescan
                {
                    jnew["event"] = recordeventtype;            // fix it to the name we want in the json
                    Rescan(jnew);
                    UpdateJson(jnew);
                }
            }
        }

        public List<Tuple<string,int>> Merge(MicroResource[] array)         // array can have repeats if owned by others or mission id different, we don't care, merge
        {
            Dictionary<string, int> entries = new Dictionary<string, int>();
            foreach( var e in array)
            {
                entries[e.Name] = (entries.ContainsKey(e.Name) ? entries[e.Name] : 0) + e.Count;        // sum them
            }
            return entries.Select(x => new Tuple<string, int>(x.Key, x.Value)).ToList();
        }

        // helper function for IMicroResourceJournalEntry
        public void UpdateMCMR(MaterialCommoditiesMicroResourceList mc, int countindex)
        {
            if (Items != null)
            {
                var counts = Merge(Items);
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Item, counts, countindex);
            }

            if (Components != null)
            {
                var counts = Merge(Components);
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Component, counts, countindex);
            }

            if (Consumables != null)
            {
                var counts = Merge(Consumables);
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Consumable, counts, countindex);
            }

            if (Data != null)
            {
                var counts = Merge(Data);
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Data, counts, countindex);
            }
        }

        public MicroResource[] Items { get; set; }
        public MicroResource[] Components { get; set; }
        public MicroResource[] Consumables { get; set; }
        public MicroResource[] Data { get; set; }

        public override string GetInfo()
        {
            string info = "";

            if (Items != null && Items.Length > 0)
            {
                if (Items.Length > 10)
                   info += BaseUtils.FieldBuilder.Build("Items".T(EDCTx.JournalMicroResources_Items) + ": ", Items.Length);
            }

            if (Components != null && Components.Length > 0)
            {
                info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Components".T(EDCTx.JournalMicroResources_Components) + ": ", Components.Length), "; ");
            }

            if (Consumables != null && Consumables.Length > 0)
            {
                if (Items.Length > 10)
                    info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Consumables".T(EDCTx.JournalMicroResources_Consumables) + ": ", Consumables.Length), "; ");
                else
                    info = info.AppendPrePad(string.Join(", ", Consumables.Select(x => x.GetFriendlyName()).ToArray()), "; ");
            }

            if (Data != null && Data.Length > 0)
            {
                info = info.AppendPrePad(BaseUtils.FieldBuilder.Build("Data".T(EDCTx.JournalMicroResources_Data) + ": ", Data.Length), "; ");
            }

            return info;
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (Items != null && Items.Length > 0)
            {
                sb.Append("Items".T(EDCTx.JournalMicroResources_Items));
                sb.Append(": ");
                sb.AppendCR();
                MicroResource.List(sb, Items);
            }

            if (Components != null && Components.Length > 0)
            {
                sb.Append("Components".T(EDCTx.JournalMicroResources_Components));
                sb.AppendCR();
                MicroResource.List(sb, Components);
            }

            if (Consumables != null && Consumables.Length > 0)
            {
                sb.Append("Consumables".T(EDCTx.JournalMicroResources_Consumables));
                sb.Append(": ");
                sb.AppendCR();
                MicroResource.List(sb, Consumables);
            }

            if (Data != null && Data.Length > 0)
            {
                sb.Append("Data".T(EDCTx.JournalMicroResources_Data));
                sb.Append(": ");
                sb.AppendCR();
                MicroResource.List(sb, Data);
            }

            return sb.ToString();
        }

    }

    #endregion
    #region ShipLocker

    // written to tell the whole state of the ship locker

    [JournalEntryType(JournalTypeEnum.ShipLocker)]
    public class JournalShipLocker : JournalMicroResourceState, IAdditionalFiles, IMicroResourceJournalEntry
    {
        public JournalShipLocker(JObject evt) : base(evt, JournalTypeEnum.ShipLocker)
        {
        }

        public void ReadAdditionalFiles(string directory)
        {
            UpdateFromAdditionalFile(System.IO.Path.Combine(directory, "Shiplocker.json"), EventTypeStr, EventTypeStr);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)        // update all ship locker materials to these values
        {
            UpdateMCMR(mc, MicroResource.ShipLocker);
        }
    }


    [JournalEntryType(JournalTypeEnum.BuyMicroResources)]
    public class JournalBuyMicroResources : JournalEntry, IMicroResourceJournalEntry, ILedgerJournalEntry
    {
        public JournalBuyMicroResources(JObject evt) : base(evt, JournalTypeEnum.BuyMicroResources)
        {
            if ( evt.Contains("MicroResources") )       // new style, present in some records
            {
                Items = evt["MicroResources"]?.ToObjectQ<MicroResource[]>()?.ToArray();       // items may be null
            }
            else
            {                                       // single entry style
                Items = new MicroResource[1] { new MicroResource() };
                evt.ToObjectProtected(Items[0].GetType(), true, membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                                            initialobject:Items[0]);        // read fields named in this structure matching JSON names
            }

            // collect Name, Name_Localised, Category, Count
            MicroResource.Normalise(Items, null);
            Price = evt["Price"].Int();
            MarketID = evt["MarketID"].Long();
            if (Items != null)
                TotalCount = Items.Select(x => x.Count).Sum();          // do it this way, so its consistent across both forms
        }

        public MicroResource[] Items { get; set; } = null;      // may be null
        public int Price { get; set; }
        public int TotalCount { get; set; }
        public long MarketID { get; set; }

        public override string GetInfo()
        {
            if (Items != null && Items.Length > 0)
            {
                if (Items.Length == 1)
                {
                    return BaseUtils.FieldBuilder.Build("", Items[0].GetFriendlyName(), "", Items[0].Count, "< buy price ; cr;N0".T(EDCTx.JournalEntry_buyprice), Price);
                }
                else
                {
                    return BaseUtils.FieldBuilder.Build("Items".T(EDCTx.JournalMicroResources_Items) + ":; ", TotalCount, "< buy price ; cr;N0".T(EDCTx.JournalEntry_buyprice), Price);
                }
            }
            return "";
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            MicroResource.List(sb, Items);
            return sb.ToString();
        }

        public void Ledger(Ledger mcl)
        {
            if ( Items != null )
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, GetInfo(), -Price);
            }
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry previous)        
        {
            if (previous?.EventTypeID != JournalTypeEnum.ShipLocker && Items!=null)      // if we have a shiplocker before, its been taken off, so don't change.
            {
                foreach (var m in Items.EmptyIfNull())
                {
                    MaterialCommodityMicroResourceType.EnsurePresent(m.Category, m.Name, m.Name_Localised);
                    mc.ChangeMR(MicroResource.ShipLocker, EventTimeUTC, m.Category, m.Name, m.Count );
                }
            }

        }
    }

    [JournalEntryType(JournalTypeEnum.SellMicroResources)]
    public class JournalSellMicroResources : JournalEntry, IMicroResourceJournalEntry, ILedgerJournalEntry
    {
        public JournalSellMicroResources(JObject evt) : base(evt, JournalTypeEnum.SellMicroResources)
        {
            // collect Name, Name_Localised, Category, Count
            Items = evt["MicroResources"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Items,null);
            Price = evt["Price"].Long();
            MarketID = evt["MarketID"].Long();
            if (Items != null)
                TotalCount = Items.Select(x => x.Count).Sum();          // do it this way, so its consistent across both forms
        }

        public MicroResource[] Items { get; set; } = null;
        public long Price { get; set; }
        public long MarketID { get; set; }
        public int TotalCount { get; set; }

        public override string GetInfo()
        {
            if (Items != null && Items.Length > 0)
            {
                if (Items.Length == 1)
                {
                    return BaseUtils.FieldBuilder.Build("", Items[0].GetFriendlyName(), "", Items[0].Count, "< sell price ; cr;N0".T(EDCTx.JournalEntry_sellprice), Price);
                }
                else
                {
                    return BaseUtils.FieldBuilder.Build("Items".T(EDCTx.JournalMicroResources_Items) + ":; ", TotalCount, "< sell price ; cr;N0".T(EDCTx.JournalEntry_sellprice), Price);
                }
            }
            return "";
        }
        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            MicroResource.List(sb, Items);
            return sb.ToString();
        }

        public void Ledger(Ledger mcl)
        {
            if (Items != null)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, GetInfo(), Price);
            }
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry previous)        // Odyssey from 11/6/21 this comes after shiplocker, so don't change
        {
            if (previous?.EventTypeID != JournalTypeEnum.ShipLocker)
            {
                foreach (var m in Items.EmptyIfNull())
                {
                    MaterialCommodityMicroResourceType.EnsurePresent(m.Category, m.Name, m.Name_Localised);
                    mc.ChangeMR(MicroResource.ShipLocker, EventTimeUTC, m.Category, m.Name, -m.Count);
                }
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.TradeMicroResources)]
    public class JournalTradeMicroResources : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalTradeMicroResources(JObject evt) : base(evt, JournalTypeEnum.TradeMicroResources)
        {
            // Collect Name, Name_Localised, Category, Count
            Offered = evt["Offered"]?.ToObjectQ<MicroResource[]>()?.ToArray();
            MicroResource.Normalise(Offered,null);
            Received = evt["Received"].Str();
            Received_Localised = evt["Received_Localised"].Str();
            Category = evt["Category"].Str();
            Count = evt["Count"].Int();
            MarketID = evt["MarketID"].Long();

            Received_Localised = JournalFieldNaming.CheckLocalisation(Received_Localised, Received);
            Received = JournalFieldNaming.FDNameTranslation(Received);
            Received_FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Received);
        }

        public MicroResource[] Offered { get; set; }
        public string Received { get; set; }
        public string Received_Localised { get; set; }
        public string Received_FriendlyName { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
        public long MarketID { get; set; }

        public override string GetInfo()
        {
            int? itemcount = Count > 1 ? Count : default(int?);
            return BaseUtils.FieldBuilder.Build("", Received_FriendlyName, "; items".T(EDCTx.JournalEntry_items), itemcount);
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            MicroResource.List(sb, Offered);
            return sb.ToString();
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry previous)       
        {
            if (previous?.EventTypeID != JournalTypeEnum.ShipLocker)    // if shiplocker is before, its already updated
            {
                foreach (var m in Offered.EmptyIfNull())
                {
                    MaterialCommodityMicroResourceType.EnsurePresent(m.Category, m.Name, m.Name_Localised);
                    mc.ChangeMR(MicroResource.ShipLocker, EventTimeUTC, m.Category, m.Name, -m.Count);
                }

                if (Received.HasChars())
                {
                    MaterialCommodityMicroResourceType.EnsurePresent(Category, Received, Received_Localised);
                    mc.ChangeMR(MicroResource.ShipLocker, EventTimeUTC, Category, Received, Count);
                }
            }
        }
    }

    #endregion  
    #region Backpack changes

    [JournalEntryType(JournalTypeEnum.Backpack)]
    public class JournalBackpack : JournalMicroResourceState, IMicroResourceJournalEntry, IAdditionalFiles
    {
        public JournalBackpack(JObject evt) : base(evt, JournalTypeEnum.Backpack)
        {
            Rescan(evt);
        }

        public void ReadAdditionalFiles(string directory)
        {
            UpdateFromAdditionalFile(System.IO.Path.Combine(directory, "Backpack.json"), EventTypeStr,EventTypeStr);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)        // update all ship locker materials to these values
        {
            UpdateMCMR(mc, MicroResource.BackPack);
        }
    }

    [JournalEntryType(JournalTypeEnum.Resupply)]
    public class JournalResupply : JournalMicroResourceState, IMicroResourceJournalEntry, IAdditionalFiles
    {
        public JournalResupply(JObject evt) : base(evt, JournalTypeEnum.Resupply)
        {
            Rescan(evt);
        }

        public void ReadAdditionalFiles(string directory)
        {
            UpdateFromAdditionalFile(System.IO.Path.Combine(directory, "Backpack.json"), "Backpack", EventTypeStr);

        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)
        {
            UpdateMCMR(mc, MicroResource.BackPack);
        }
    }


    [JournalEntryType(JournalTypeEnum.BackpackChange)]
    public class JournalBackpackChange : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalBackpackChange(JObject evt) : base(evt, JournalTypeEnum.BackpackChange)
        {
            // collect Name, Name_localised, OwnerId, Count, Type
            Added = evt["Added"]?.ToObject<MicroResource[]>(false)?.ToArray();
            MicroResource.Normalise(Added,null);
            Removed = evt["Removed"]?.ToObject<MicroResource[]>(false)?.ToArray();
            MicroResource.Normalise(Removed,null);
        }

        public void Add(JournalBackpackChange other)
        {
            if (Added == null)              // nothing here, this is the other
                Added = other.Added;
            else if (other.Added != null)   // if other has data, concat
                Added = Added.Concat(other.Added).ToArray();

            if (Removed == null)
                Removed = other.Removed;
            else if (other.Removed != null)
                Removed = Removed.Concat(other.Removed).ToArray();
        }

        public MicroResource[] Added { get; set; }
        public MicroResource[] Removed { get; set; }

        public bool ThrowGrenade { get { return Removed != null && Added == null && Removed.Length == 1 && Removed[0].Name.Equals("amm_grenade_frag"); } }

        public override string GetInfo()
        {
            var sb = new System.Text.StringBuilder(256);
            if (Added != null)
            {
                foreach (var i in Added)
                {
                    int? c = i.Count > 1 ? i.Count : default(int?);
                    sb.AppendPrePad(BaseUtils.FieldBuilder.Build("+ ", i.GetFriendlyName(), "<:", c), ", ");
                }
            }
            if (Removed != null)
            {
                foreach (var i in Removed)
                {
                    int? c = i.Count > 1 ? i.Count : default(int?);
                    sb.AppendPrePad(BaseUtils.FieldBuilder.Build("- ", i.GetFriendlyName(), "<:", c), ", ");

                }
            }
            return sb.ToString();
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)
        {
            if (Added != null)
            {
                foreach (var i in Added)
                {
                    mc.ChangeMR(MicroResource.BackPack, EventTimeUTC, i.Category, i.Name, i.Count);
                }
            }
            if (Removed != null)
            {
                foreach (var i in Removed)
                {
                    mc.ChangeMR(MicroResource.BackPack, EventTimeUTC, i.Category, i.Name, -i.Count);
                }
            }

        }
    }

    // we rely for these below on BackpackChange to record deltas. See ReorderRemove in historylist.

    [JournalEntryType(JournalTypeEnum.CollectItems)]
    public class JournalCollectItems : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalCollectItems(JObject evt) : base(evt, JournalTypeEnum.CollectItems)
        {
            //Collect Name, Name_Localised,  Type, OwnerId, Count. Enable custom attributes to allow type to alias to category
            evt.ToObjectProtected(Resource.GetType(), true, membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, initialobject: Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise(null);
            Stolen = evt["Stolen"].Bool();
        }

        public JournalCollectItems(DateTime utc, MicroResource res, bool stolen, long tluid, int cmdr, long id) : base(utc, JournalTypeEnum.CollectItems)
        {
            SetTLUCommander(tluid, cmdr);
            SetJID(id);
            Resource = res;
            Stolen = stolen;
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public bool Stolen { get; set; }

        public override string GetInfo()
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Resource.Name);     // may be null
            int? itemcount = Resource.Count > 1 ? Resource.Count : default(int?);
            return BaseUtils.FieldBuilder.Build("", Resource.GetFriendlyName(), "< (;)", mcd?.TranslatedCategory, "< ; items".T(EDCTx.JournalEntry_MatC), itemcount, ";Stolen".T(EDCTx.JournalEntry_Stolen), Stolen);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)    // no action, BPC does the work, but mark as MR
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.DropItems)]
    public class JournalDropItems : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalDropItems(JObject evt) : base(evt, JournalTypeEnum.DropItems)
        {
            // Collect name, NameLocalised, Type,OwnerId, Count. Enable custom attributes to allow type to alias to category
            evt.ToObjectProtected(Resource.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise(null);
        }

        public JournalDropItems(DateTime utc, MicroResource res, long tluid, int cmdr, long id) : base(utc, JournalTypeEnum.DropItems)
        {
            SetTLUCommander(tluid, cmdr);
            SetJID(id);
            Resource = res;
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public override string GetInfo()
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Resource.Name);     // may be null
            int? itemcount = Resource.Count > 1 ? Resource.Count : default(int?);
            return BaseUtils.FieldBuilder.Build("", Resource.GetFriendlyName(), "< (;)", mcd?.TranslatedCategory, "< ; items".T(EDCTx.JournalEntry_MatC), itemcount);
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)    // no action, BPC does the work, but mark as MR
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.UseConsumable)]
    public class JournalUseConsumable : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalUseConsumable(JObject evt) : base(evt, JournalTypeEnum.UseConsumable)
        {
            // Collect name, NameLocalised, Type.  Enable custom attributes to allow type to alias to category
            evt.ToObjectProtected(Resource.GetType(), true,
                    membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    initialobject: Resource);        // read fields named in this structure matching JSON names
            Resource.Normalise(null);
        }

        // use to make a use consumable JE 
        public JournalUseConsumable(DateTime utc, MicroResource res, long tluid, int cmdr, long id) : base(utc, JournalTypeEnum.UseConsumable)
        {
            SetTLUCommander(tluid, cmdr);
            SetJID(id);
            Resource = res;
        }

        public MicroResource Resource { get; set; } = new MicroResource();

        public override string GetInfo()
        {
            return Resource.GetFriendlyName();
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)    // no action, BPC does the work, but mark as MR
        {
        }
    }

    [JournalEntryType(JournalTypeEnum.DeliverPowerMicroResources)]
    public class JournalDeliverPowerMicroResources : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalDeliverPowerMicroResources(JObject evt) : base(evt, JournalTypeEnum.DeliverPowerMicroResources)
        {
            TotalCount = evt["TotalCount"].Int();
            MarketID = evt["MarketID"].Long();
            MicroResources = evt["MicroResources"]?.ToObject<MicroResource[]>(false)?.ToArray();
            MicroResource.Normalise(MicroResources, null);
        }

        public int TotalCount { get; set; }
        public MicroResource[] MicroResources { get; set; }
        public long MarketID { get; set; }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (MicroResources != null && MicroResources.Length > 0)
            {
                sb.Append("Items".T(EDCTx.JournalMicroResources_Items));
                sb.Append(": ");
                MicroResource.List(sb, MicroResources,"");
            }

            return sb.ToString();
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)    
        {
            foreach (var i in MicroResources)
            {
                mc.ChangeMR(MicroResource.BackPack, EventTimeUTC, i.Category, i.Name, -i.Count);        // mark as removed
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.RequestPowerMicroResources)]
    public class JournalRequestPowerMicroResources : JournalEntry, IMicroResourceJournalEntry
    {
        public JournalRequestPowerMicroResources(JObject evt) : base(evt, JournalTypeEnum.RequestPowerMicroResources)
        {
            TotalCount = evt["TotalCount"].Int();
            MarketID = evt["MarketID"].Long();
            MicroResources = evt["MicroResources"]?.ToObject<MicroResource[]>(false)?.ToArray();
            MicroResource.Normalise(MicroResources, null);
        }

        public int TotalCount { get; set; }
        public MicroResource[] MicroResources { get; set; }
        public long MarketID { get; set; }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (MicroResources != null && MicroResources.Length > 0)
            {
                sb.Append("Items".T(EDCTx.JournalMicroResources_Items));
                sb.Append(": ");
                MicroResource.List(sb, MicroResources,"");
            }

            return sb.ToString();
        }

        public void UpdateMicroResource(MaterialCommoditiesMicroResourceList mc, JournalEntry unused)    // no action does not affect but need to mark as MR
        {
        }
    }


    #endregion
}
