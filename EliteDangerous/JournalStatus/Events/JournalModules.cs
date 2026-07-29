/*
 * Copyright © 2016-2024 EDDiscovery development team
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

namespace EliteDangerousCore.JournalEvents
{
    [System.Diagnostics.DebuggerDisplay("{ShipId} {ShipFD} {ShipModules.Count}")]
    [JournalEntryType(JournalTypeEnum.Loadout)]
    public class JournalLoadout : JournalEntry, IShipInformation, IShipNaming
    {
        public JournalLoadout(JObject evt) : base(evt, JournalTypeEnum.Loadout)
        {
            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string engname, this);
            ShipType = engname;
            ShipId = new ShipID(evt["ShipID"]);
            ShipName = evt["ShipName"].Str();
            ShipIdent = evt["ShipIdent"].Str();
            HullValue = evt["HullValue"].LongNull();
            HullHealth = evt["HullHealth"].DoubleNull();
            if (HullHealth != null)
                HullHealth *= 100.0;        // convert to 0-100
            ModulesValue = evt["ModulesValue"].LongNull();
            Rebuy = evt["Rebuy"].LongNull();
            Hot = evt["Hot"].BoolNull();    // 3.3
            UnladenMass = evt["UnladenMass"].DoubleNull(); // 3.4
            CargoCapacity = evt["CargoCapacity"].IntNull(); // 3.4
            MaxJumpRange = evt["MaxJumpRange"].DoubleNull(); // 3.4

            var fuelcap = evt["FuelCapacity"] as JObject; // 3.4

            if (fuelcap != null)
            {
                MainFuelCapacity = fuelcap["Main"].DoubleNull();
                ReserveFuelCapacity = fuelcap["Reserve"].DoubleNull();
            }

            bool debugout = false;

            if (debugout) System.Diagnostics.Debug.WriteLine($"Loadout {ShipFD} {ShipType}");       // useful debug

            ShipModules = new List<ShipModule>();

            JArray jmodules = (JArray)evt["Modules"];
            if (jmodules != null)       // paranoia
            {
                foreach (JObject jo in jmodules)
                {
                    EngineeringData engineering = null;

                    JObject jeng = (JObject)jo["Engineering"];
                    if (jeng != null)
                    {
                        engineering = new EngineeringData(jeng, this);
                        if (!engineering.IsValid)       // we get some bad engineering lines, if so, then remove the engineering
                        {
                            //System.Diagnostics.Debug.WriteLine($"Bad Engineering line loadout : {jo.ToString()}");
                            engineering = null;
                        }
                    }

                    ShipSlots.Slot slotfdname = ShipSlots.ToEnum(jo["Slot"].Str());

                    var itemfdname = ModFDName.Normalise(jo["Item"].Str(), out engname, this);

                    if ( debugout ) System.Diagnostics.Debug.WriteLine($"  Modules {slotfdname} {itemfdname} = {engname} {itemfdname.GetForeignModuleName(null,slotfdname)}");

                    ShipModule module = new ShipModule(ShipSlots.ToEnglish(slotfdname),
                                                        slotfdname,
                                                        engname,
                                                        itemfdname,
                                                        jo["On"].BoolNull(),
                                                        jo["Priority"].IntNull(),
                                                        jo["AmmoInClip"].IntNull(),
                                                        jo["AmmoInHopper"].IntNull(),
                                                        jo["Health"].DoubleNull(),
                                                        jo["Value"].IntNull(),
                                                        null,  //power not received here
                                                        engineering);
                    ShipModules.Add(module);
                }
            }
        }

        public string ShipType { get; set; }        // type, pretty name fer-de-lance
        public VehicleFDName ShipFD { get; set; }        // type,  fdname
        public ShipID ShipId { get; set; }
        public string ShipName { get; set; } // : user-defined ship name
        public string ShipIdent { get; set; } //   user-defined ship ID string
        public long? HullValue { get; set; }   //3.0
        public double? HullHealth { get; set; }   //3.3, 1.0-0.0, multipled by 100.0
        public long? ModulesValue { get; set; }   //3.0
        public long? Rebuy { get; set; }   //3.0
        public bool? Hot { get; set; }   //3.3
        public double? UnladenMass { get; set; }   // 3.4
        public double? MainFuelCapacity { get; set; }   // 3.4
        public double? ReserveFuelCapacity { get; set; }   // 3.4
        public int? CargoCapacity { get; set; }   // 3.4
        public double? MaxJumpRange { get; set; }   // 3.4


        public List<ShipModule> ShipModules;

        public void ShipInformation(ShipList shp, string _, ISystem __)
        {
            var shipproperties = ItemData.GetShipProperties(ShipFD);

            if (shipproperties!=null && !IsBeta)        // we know about the ship, and its not beta.  beta ships sometimes (TypeX) get changed in release
            {                                           // do it here since we know about BETA here, not before 
                foreach( var m in ShipModules )
                {
                    if (!shipproperties.HasSlot(m.SlotFD))
                        BaseUtils.Debugger.TraceBreak($"*** Ship data missing slot {m.SlotFD} for {ShipFD.Str()} : error in EDD ship data");
                }
            }

            shp.Loadout(ShipId, ShipType, ShipFD, ShipName, ShipIdent, ShipModules, HullValue ?? 0, ModulesValue ?? 0, Rebuy ?? 0,
                                UnladenMass ?? 0, ReserveFuelCapacity ?? 0, HullHealth ?? 0, Hot);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Ship".Tx()+": ", ShipType, "Name".Tx()+": ", ShipName, "Ident".Tx()+": ", ShipIdent, ";(Hot)".Tx(), Hot,
                "Modules".Tx()+": ", ShipModules.Count, "Hull Health: ;%;N1".Tx(), HullHealth, "Hull: ; cr;N0".Tx(), HullValue, "Modules: ; cr;N0".Tx(), ModulesValue, "Rebuy: ; cr;N0".Tx(), Rebuy);
        }


        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(4096);

            foreach (ShipModule m in ShipModules)
            {
                sb.AppendCR();
                sb.Build(
                            "", ShipSlots.ToLocalisedLanguage(m.SlotFD), 
                            "<: ", m.ItemFD.GetForeignModuleName(m.LocalisedItem), 
                            "", m.PE(),
                            "Blueprint".Tx()+": ", m.Engineering?.FriendlyBlueprintName, 
                            "<+", m.Engineering?.ExperimentalEffect_Localised, 
                            "< ", m.Engineering?.Engineer.Str());
            }

            return sb.ToString();
        }

    }


    [JournalEntryType(JournalTypeEnum.ModuleBuy)]
    public class JournalModuleBuy : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleBuy(JObject evt ) : base(evt, JournalTypeEnum.ModuleBuy)
        {
            SlotFD = ShipSlots.ToEnum(evt["Slot"].Str());
            Slot = ShipSlots.ToEnglish(SlotFD);

            BuyItemFD = ModFDName.Normalise(evt["BuyItem"].Str(), out string engname, this);
            BuyItem = engname;
            BuyItemLocalised = JournalFieldNaming.CheckLocalisation(evt["BuyItem_Localised"].Str(),BuyItem);
            BuyPrice = evt["BuyPrice"].Long();

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out engname, this);
            Ship = engname;
            ShipId = new ShipID(evt["ShipID"]);

            SellItemFD = ModFDName.Normalise(evt["SellItem"].Str(), out engname, this, true);       // allowed null
            if (SellItemFD != null)
            {
                SellItem = engname;
                SellPrice = evt["SellPrice"].LongNull();
                SellItemLocalised = JournalFieldNaming.CheckLocalisation(evt["SellItem_Localised"].Str(), SellItem);
            }

            StoredItemFD = ModFDName.Normalise(evt["StoredItem"].Str(), out engname, this,true);       // allowed null
            if (StoredItemFD != null)
            {
                StoredItem = engname;
                StoredItemLocalised = JournalFieldNaming.CheckLocalisation(evt["StoredItem_Localised"].Str(), StoredItem);
            }

            MarketID = new MarketID(evt["MarketID"]);
        }

        public string Slot { get; set; }                        // english name
        public ShipSlots.Slot SlotFD { get; set; }

        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }

        public string BuyItem { get; set; }                     // english name
        public ModFDName BuyItemFD { get; set; }
        public string BuyItemLocalised { get; set; }
        public long BuyPrice { get; set; }

        public ModFDName SellItemFD { get; set; }                  // if sold previous one, else null
        public string SellItem { get; set; }                    // if sold previous one, english name
        public string SellItemLocalised { get; set; }
        public long? SellPrice { get; set; }

        public ModFDName StoredItemFD { get; set; }                // if stored previous one, else null
        public string StoredItem { get; set; }                  // if stored previous one, english name
        public string StoredItemLocalised { get; set; }         // if stored previous one

        public MarketID MarketID { get; set; }

        public void Ledger(Ledger mcl)
        {
            long diff = -BuyPrice + (SellPrice ?? 0);

            if (diff != 0)
            {
                string s = (BuyItemLocalised.Length > 0) ? BuyItemLocalised : BuyItem;
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, s + " @ " + Ship, diff);
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleBuy(this, system);
        }

        public override string GetInfo() 
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Build("", BuyItemFD.GetForeignModuleName(BuyItemLocalised), "< into ".Tx(),
                                                        ShipSlots.ToLocalisedLanguage(SlotFD), "Cost: ; cr;N0".Tx(), BuyPrice);
            if (SellItemFD != null)
            {
                sb.AppendCS();
                sb.Build("Sold".Tx()+": ", SellItemFD.GetForeignModuleName(SellItemLocalised), "Price: ; cr;N0".Tx(), SellPrice);
            }

            if (StoredItemFD != null)
            {
                sb.AppendCS();
                sb.Build("Stored".Tx()+": ", StoredItemFD.GetForeignModuleName(StoredItemLocalised));
            }

            return sb.ToString();
        }
    }


    [JournalEntryType(JournalTypeEnum.ModuleBuyAndStore)]
    public class JournalModuleBuyAndStore : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleBuyAndStore(JObject evt) : base(evt, JournalTypeEnum.ModuleBuyAndStore)
        {
            BuyItemFD = ModFDName.Normalise(evt["BuyItem"].Str(), out string engname, this);
            BuyItem = engname;
            BuyItemLocalised = JournalFieldNaming.CheckLocalisation(evt["BuyItem_Localised"].Str(), BuyItem);

            MarketID = new MarketID(evt["MarketID"]);
            BuyPrice = evt["BuyPrice"].Long();

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname , this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);
        }

        public string BuyItem { get; set; }     // english name
        public ModFDName BuyItemFD { get; set; }
        public string BuyItemLocalised { get; set; }

        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }

        public MarketID MarketID { get; set; }
        public long BuyPrice { get; set; }

        public void Ledger(Ledger mcl)
        {
            string s = (BuyItemLocalised.Length > 0) ? BuyItemLocalised : BuyItem;

            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, s + " @ " + Ship, -BuyPrice);
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleBuyAndStore(this,system);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", BuyItemFD.GetForeignModuleName(BuyItemLocalised), "Cost: ; cr;N0".Tx(), BuyPrice);
        }
    }


    [JournalEntryType(JournalTypeEnum.ModuleSell)]
    public class JournalModuleSell : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleSell(JObject evt) : base(evt, JournalTypeEnum.ModuleSell)
        {
            SlotFD = ShipSlots.ToEnum(evt["Slot"].Str());
            Slot = ShipSlots.ToEnglish(SlotFD);

            SellItemFD = ModFDName.Normalise(evt["SellItem"].Str(), out string engname, this);
            SellItem = engname;
            SellItemLocalised = JournalFieldNaming.CheckLocalisation(evt["SellItem_Localised"].Str(), SellItem);

            SellPrice = evt["SellPrice"].Long();

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            MarketID = new MarketID(evt["MarketID"]);
        }

        public string Slot { get; set; }
        public ShipSlots.Slot SlotFD { get; set; }
        public string SellItem { get; set; }    // english
        public ModFDName SellItemFD { get; set; }
        public string SellItemLocalised { get; set; }
        public long SellPrice { get; set; }
        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }
        public MarketID MarketID { get; set; }

        public void Ledger(Ledger mcl)
        {
            if (SellPrice != 0)
            {
                string s = (SellItemLocalised.Length > 0) ? SellItemLocalised : SellItem;
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, s + " @ " + Ship, SellPrice);
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleSell(this);
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", SellItemFD.GetForeignModuleName(SellItemLocalised), "< from ".Tx(),
                                            ShipSlots.ToLocalisedLanguage(SlotFD), "Price: ; cr;N0".Tx(), SellPrice);
        }

    }

    [JournalEntryType(JournalTypeEnum.ModuleSellRemote)]
    public class JournalModuleSellRemote : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleSellRemote(JObject evt) : base(evt, JournalTypeEnum.ModuleSellRemote)
        {
            SlotNumber = evt["StorageSlot"].Int();

            SellItemFD = ModFDName.Normalise(evt["SellItem"].Str(), out string engname, this);
            SellItem = engname;
            SellItemLocalised = JournalFieldNaming.CheckLocalisation(evt["SellItem_Localised"].Str(), SellItem);

            SellPrice = evt["SellPrice"].Long();

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            ServerId = evt["ServerId"].Int();
        }

        public int SlotNumber { get; set; }
        public string SellItem { get; set; }    // english
        public ModFDName SellItemFD { get; set; }
        public string SellItemLocalised { get; set; }
        public long SellPrice { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public string Ship { get; set; }
        public ShipID ShipId { get; set; }
        public int ServerId { get; set; }

        public void Ledger(Ledger mcl)
        {
            if (SellPrice != 0)
            {
                string s = (SellItemLocalised.Length > 0) ? SellItemLocalised : SellItem;
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, " @ " + Ship, SellPrice);
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleSellRemote(this);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Item".Tx()+": ", SellItemFD.GetForeignModuleName(SellItemLocalised),
                                            "Price: ; cr;N0".Tx(), SellPrice);
        }

        public override string GetDetailed()
        {
            return BaseUtils.FieldBuilder.Build("Ship".Tx()+": ", Ship);
        }
    }


    [JournalEntryType(JournalTypeEnum.ModuleRetrieve)]
    public class JournalModuleRetrieve : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleRetrieve(JObject evt) : base(evt, JournalTypeEnum.ModuleRetrieve)
        {
            SlotFD = ShipSlots.ToEnum(evt["Slot"].Str());
            Slot = ShipSlots.ToEnglish(SlotFD);

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            // early entries had this bust
            RetrievedItemFD = ModFDName.Normalise(evt["RetrievedItem"].Str(), out string engname, this, true);
            if (RetrievedItemFD != null)
            {
                RetrievedItem = engname;
                RetrievedItemLocalised = JournalFieldNaming.CheckLocalisation(evt["RetrievedItem_Localised"].Str(), RetrievedItem);
            }

            SwapOutItemFD = ModFDName.Normalise(evt["SwapOutItem"].Str(), out engname, this, true); // allow null
            SwapOutItem = engname;
            if (SwapOutItemFD != null)
                SwapOutItemLocalised = JournalFieldNaming.CheckLocalisation(evt["SwapOutItem_Localised"].Str(), SwapOutItem);

            Cost = evt["Cost"].Long();

            Hot = evt["Hot"].BoolNull();
            Level = evt["Level"].IntNull();
            Quality = evt["Quality"].DoubleNull();

            MarketID = new MarketID(evt["MarketID"]);

            FDEngineerModifications = EngineeringRecipeFDName.Normalise(evt["EngineerModifications"].Str(), out engname, this, true);
            if (FDEngineerModifications != null)
                EngineerModifications = engname;
        }

        public ShipSlots.Slot SlotFD { get; set; }
        public string Slot { get; set; }        // english

        public string Ship { get; set; }            // always there
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }

        public ModFDName RetrievedItemFD { get; set; }                 // may be null for busted first ones
        public string RetrievedItem { get; set; }                   // english
        public string RetrievedItemLocalised { get; set; }

        public EngineeringRecipeFDName FDEngineerModifications { get; set; }         // FDName, may be null
        public string EngineerModifications { get; set; }           // Friendly, may be null

        public ModFDName SwapOutItemFD { get; set; }                   // may be null
        public string SwapOutItem { get; set; }                     // may be null english
        public string SwapOutItemLocalised { get; set; }            // may be null

        public long Cost { get; set; }
        public double? Quality { get; set; }
        public int? Level { get; set; }
        public bool? Hot { get; set; }
        public MarketID MarketID { get; set; }

        public void Ledger(Ledger mcl)
        {
            if (Cost != 0)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, RetrievedItemFD.GetForeignModuleName(RetrievedItemLocalised) + " @ " + Ship, -Cost);
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if ( RetrievedItemFD != null )     // make sure its a valid one
                shp.ModuleRetrieve(this,system);
        }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
            
            sb.Build("", RetrievedItemFD?.GetForeignModuleName(RetrievedItemLocalised),
                            "< into ".Tx(), ShipSlots.ToLocalisedLanguage(SlotFD), ";(Hot)".Tx(), Hot);
            if (Cost > 0)
            {
                sb.AppendCS();
                sb.Build("Cost: ; cr;N0".Tx(), Cost);
            }

            if (SwapOutItemFD!=null)
            {
                sb.AppendCS();
                sb.Build("Stored".Tx()+": ", SwapOutItemFD.GetForeignModuleName(SwapOutItemLocalised));
            }

            return sb.ToString();
        }

    }



    [JournalEntryType(JournalTypeEnum.ModuleStore)]
    public class JournalModuleStore : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalModuleStore(JObject evt) : base(evt, JournalTypeEnum.ModuleStore)
        {
            SlotFD = ShipSlots.ToEnum(evt["Slot"].Str());
            Slot = ShipSlots.ToEnglish(SlotFD);

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            StoredItemFD = ModFDName.Normalise(evt["StoredItem"].Str(), out string engname, this);
            StoredItem = engname;
            StoredItemLocalised = JournalFieldNaming.CheckLocalisation(evt["StoredItem_Localised"].Str(), StoredItem);

            ReplacementItemFD = ModFDName.Normalise(evt["ReplacementItem"].Str(), out engname, this, true);
            ReplacementItem = engname;
            if (ReplacementItemFD != null)
                ReplacementItemLocalised = JournalFieldNaming.CheckLocalisation(evt["ReplacementItem_Localised"].Str(), ReplacementItem);

            Cost = evt["Cost"].LongNull();

            Hot = evt["Hot"].BoolNull();
            Level = evt["Level"].IntNull();
            Quality = evt["Quality"].DoubleNull();

            MarketID = new MarketID(evt["MarketID"]);

            FDEngineerModifications = EngineeringRecipeFDName.Normalise(evt["EngineerModifications"].Str(), out engname, this, true);
            if (FDEngineerModifications != null)
                EngineerModifications = engname;
        }

        public string Slot { get; set; }
        public ShipSlots.Slot SlotFD { get; set; }
        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }
        public string StoredItem { get; set; }  // english
        public ModFDName StoredItemFD { get; set; }
        public string StoredItemLocalised { get; set; }
        public EngineeringRecipeFDName FDEngineerModifications { get; set; }     // may be null
        public string EngineerModifications { get; set; }       // may be null
        public ModFDName ReplacementItemFD { get; set; }           // null if not . In journal doc but july 26 no evidence of replacement items
        public string ReplacementItem { get; set; }             // null if not english
        public string ReplacementItemLocalised { get; set; }    // null if not
        public long? Cost { get; set; }
        public double? Quality { get; set; }
        public int? Level { get; set; }
        public bool? Hot { get; set; }
        public MarketID MarketID { get; set; }

        public void Ledger(Ledger mcl)
        {
            if (Cost.HasValue)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, StoredItemFD.GetForeignModuleName(StoredItemLocalised) + " @ ".Tx() + Ship, -(Cost.Value));
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleStore(this,system);
        }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);

            sb.Build("", StoredItemFD.GetForeignModuleName(StoredItemLocalised), "< from ".Tx(),
                               ShipSlots.ToLocalisedLanguage(SlotFD), ";(Hot)".Tx(), Hot, "Cost: ; cr;N0".Tx(), Cost);

            if (ReplacementItem!=null)
            {
                sb.BuildCont("Replaced by".Tx()+": ", ReplacementItemFD.GetForeignModuleName(ReplacementItemLocalised));
            }

            return sb.ToString();
        }

        public override string GetDetailed()
        {
            return BaseUtils.FieldBuilder.Build("Modifications".Tx()+": ", EngineerModifications);
        }

    }


    [JournalEntryType(JournalTypeEnum.ModuleSwap)]
    public class JournalModuleSwap : JournalEntry, IShipInformation
    {
        public JournalModuleSwap(JObject evt) : base(evt, JournalTypeEnum.ModuleSwap)
        {
            FromSlotFD = ShipSlots.ToEnum(evt["FromSlot"].Str());
            FromSlot = ShipSlots.ToEnglish(FromSlotFD);

            ToSlotFD = ShipSlots.ToEnum(evt["ToSlot"].Str());
            ToSlot = ShipSlots.ToEnglish(ToSlotFD);

            FromItemFD = ModFDName.Normalise(evt["FromItem"].Str(), out string engname, this);
            FromItem = engname;
            FromItemLocalised = JournalFieldNaming.CheckLocalisation(evt["FromItem_Localised"].Str(), FromItem);

            string s = evt["ToItem"].Str();
            if (s.EqualsIIC("null"))        // early bug, doing it this way stops NormaliseModules moan
            {
                ToItemFD = ModFDName.Empty;
                ToItem = ToItemFD.Str();
            }
            else
            {
                ToItemFD = ModFDName.Normalise(s, out engname, this);
                ToItem = engname;
            }
            ToItemLocalised = JournalFieldNaming.CheckLocalisation(evt["ToItem_Localised"].Str(), ToItem);        // if ToItem is null or not there, this won't be

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            MarketID = new MarketID(evt["MarketID"]);
        }

        public ShipSlots.Slot FromSlotFD { get; set; }
        public string FromSlot { get; set; }    // english
        public ModFDName FromItemFD { get; set; }
        public string FromItem { get; set; }    // English
        public string FromItemLocalised { get; set; }

        public ShipSlots.Slot ToSlotFD { get; set; }   
        public string ToSlot { get; set; }              
        public ModFDName ToItemFD { get; set; }        // will be set, may be invalid due to missing data
        public string ToItem { get; set; }          // 
        public string ToItemLocalised { get; set; }

        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }
        public MarketID MarketID { get; set; }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if ( ToItemFD.IsValid && FromItemFD.IsValid) 
                shp.ModuleSwap(this);
        }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);

            sb.Build( "Slot".Tx()+": ", ShipSlots.ToLocalisedLanguage(FromSlotFD), "< to ".Tx(), ShipSlots.ToLocalisedLanguage(ToSlotFD), 
                            "Item".Tx()+": ", FromItemFD.GetForeignModuleName(FromItemLocalised));
            if (ToItemFD.IsValid)
            {
                sb.Append(", Swapped with ".Tx());
                sb.Append(ToItemFD.GetForeignModuleName(ToItemLocalised));
            }

            return sb.ToString();
        }
    }



    [System.Diagnostics.DebuggerDisplay("{ShipId} {Ship} {ShipModules.Count}")]
    [JournalEntryType(JournalTypeEnum.ModuleInfo)]
    public class JournalModuleInfo : JournalEntry, IAdditionalFiles, IShipInformation
    {
        public JournalModuleInfo(JObject evt) : base(evt, JournalTypeEnum.ModuleInfo)
        {
            Rescan(evt);
        }

        public void Rescan(JObject evt)
        {
            ShipModules = new List<ShipModule>();

            JArray jmodules = (JArray)evt["Modules"];
            if (jmodules != null)
            {
                foreach (JObject jo in jmodules)
                {
                    ShipSlots.Slot SlotFDname = ShipSlots.ToEnum(jo["Slot"].Str());
                    var itemfdname = ModFDName.Normalise(jo["Item"].Str(), out string engname, this);

                    ShipModule module = new ShipModule( ShipSlots.ToEnglish(SlotFDname),
                                                        SlotFDname,
                                                        engname,
                                                        itemfdname,
                                                        null, // unknown
                                                        jo["Priority"].IntNull(),
                                                        null, // aclip
                                                        null, // ahooper
                                                        null, // health
                                                        null, // Value
                                                        jo["Power"].DoubleNull(),
                                                        null //engineering
                                                        );
                    ShipModules.Add(module);
                }
            }
        }
        public void ReadAdditionalFiles(string directory)
        {
            JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "ModulesInfo.json"), EventTypeStr);
            if (jnew != null)        // new json, rescan
            {
                Rescan(jnew);
                UpdateJson(jnew);
            }
        }

        public List<ShipModule> ShipModules;

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.ModuleInfo(ShipModules);
        }


        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Modules".Tx()+": ", ShipModules.Count);
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (ShipModule m in ShipModules)
            {
                int? priority = m.Priority.HasValue ? (m.Priority.Value+1) : default(int?);
                sb.AppendCR();
                sb.Build("", ShipSlots.ToLocalisedLanguage(m.SlotFD), "<: ", m.ItemFD.GetForeignModuleName(m.LocalisedItem), "; MW;0.###", m.Power, "P:", priority);
            }

            return sb.ToString();
        }
    }


    [JournalEntryType(JournalTypeEnum.StoredModules)]
    public class JournalStoredModules : JournalEntry, IShipInformation
    {
        public JournalStoredModules(JObject evt) : base(evt, JournalTypeEnum.StoredModules)
        {
            StationName = evt["StationName"].Str();
            StarSystem = evt["StarSystem"].Str();
            MarketID = new MarketID(evt["MarketID"]);

            ModuleItems = evt["Items"]?.ToObjectQ<ShipModulesInStore.StoredModule[]>();

            if (ModuleItems != null)
            {
                foreach (ShipModulesInStore.StoredModule i in ModuleItems)
                    i.Normalise(this);
            }
        }

        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public MarketID MarketID { get; set; }

        public ShipModulesInStore.StoredModule[] ModuleItems { get; set; }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.UpdateStoredModules(this);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Total".Tx()+": ", ModuleItems?.Count());
        }

        public override string GetDetailed()
        {
            if (ModuleItems != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (ShipModulesInStore.StoredModule m in ModuleItems)
                {
                    sb.AppendCR();
                    sb.Build("", m.NameFD.GetForeignModuleName(m.Name_Localised), "< at ".Tx(), m.StarSystem,
                                "Transfer Cost: ; cr;N0".Tx(), m.TransferCost,
                                "Time".Tx() + ": ", m.TransferTimeString,
                                "Value: ; cr;N0".Tx(), m.TransferCost, ";(Hot)".Tx(), m.Hot);
                }
                return sb.ToString();
            }
            else
                return null;
        }
    }


    [JournalEntryType(JournalTypeEnum.MassModuleStore)]
    public class JournalMassModuleStore : JournalEntry, IShipInformation
    {

        public JournalMassModuleStore(JObject evt) : base(evt, JournalTypeEnum.MassModuleStore)
        {
            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            ModuleItems = evt["Items"]?.ToObjectQ<ModuleItem[]>();
            MarketID = new MarketID(evt["MarketID"]);

            if (ModuleItems != null)
            {
                foreach (ModuleItem i in ModuleItems)       // Normalise
                {
                    i.SlotFD = ShipSlots.ToEnum(i.Slot);
                    i.Slot = ShipSlots.ToEnglish(i.SlotFD);
                    i.NameFD = ModFDName.Normalise(i.Name, out string bettername, this);
                    i.Name = bettername;
                }
            }
        }

        public string Ship { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public ShipID ShipId { get; set; }
        public MarketID MarketID { get; set; }

        public ModuleItem[] ModuleItems { get; set; }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.MassModuleStore(this,system);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Total modules".Tx()+": ", ModuleItems?.Count());
        }

        public override string GetDetailed()
        {
            if (ModuleItems != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (ModuleItem m in ModuleItems)
                {
                    sb.AppendCR();
                    sb.Build("", m.NameFD.GetForeignModuleName(m.Name_Localised), ";(Hot)".Tx(), m.Hot);
                }
                return sb.ToString();
            }
            else
                return null;
        }

        public class ModuleItem
        {
            public ShipSlots.Slot SlotFD;       
            public string Slot;                 // json, english text afterwards
            public ModFDName NameFD;               // fdname
            public string Name;                 // english name
            public string Name_Localised;
            public EngineeringRecipeFDName EngineerModifications;    // may be null
            public double? Quality { get; set; }
            public int? Level { get; set; }
            public bool? Hot { get; set; }
        }
    }

    [JournalEntryType(JournalTypeEnum.FetchRemoteModule)]
    public class JournalFetchRemoteModule : JournalEntry, ILedgerJournalEntry
    {
        public JournalFetchRemoteModule(JObject evt) : base(evt, JournalTypeEnum.FetchRemoteModule)
        {
            StorageSlot = evt["StorageSlot"].Str();          // Slot number, not a slot on our ship

            StoredItemFD = ModFDName.Normalise(evt["StoredItem"].Str(), out string bettername, this);
            StoredItem = bettername;
            StoredItemLocalised = JournalFieldNaming.CheckLocalisation(evt["StoredItem_Localised"].Str(), StoredItem);

            TransferCost = evt["TransferCost"].Long();

            ShipFD = VehicleFDName.Normalise(evt["Ship"].Str(), out string shipname, this);
            Ship = shipname;
            ShipId = new ShipID(evt["ShipID"]);

            ServerId = evt["ServerId"].Int();
            nTransferTime = evt["TransferTime"].IntNull();
            FriendlyTransferTime = nTransferTime.HasValue ? nTransferTime.Value.SecondsToString() : "";
        }

        public string StorageSlot { get; set; }
        public string StoredItem { get; set; }      // english name
        public ModFDName StoredItemFD { get; set; }
        public string StoredItemLocalised { get; set; }
        public long TransferCost { get; set; }
        public VehicleFDName ShipFD { get; set; }
        public string Ship { get; set; }
        public ShipID ShipId { get; set; }
        public int ServerId { get; set; }
        public int? nTransferTime { get; set; }
        public string FriendlyTransferTime { get; set; }

        public void Ledger(Ledger mcl)
        {
            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, StoredItemLocalised + " @ " + Ship, -TransferCost);
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", StoredItemFD.GetForeignModuleName(StoredItemLocalised), "Cost: ; cr;N0".Tx(), TransferCost, "Into ship".Tx()+": ", Ship, "Transfer Time".Tx()+": ", FriendlyTransferTime);
        }
    }


}
