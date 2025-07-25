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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Cargo)]
    public class JournalCargo : JournalEntry, ICommodityJournalEntry, IAdditionalFiles
    {
        public class Cargo
        {
            public string Name { get; set; }            // FDNAME
            public string Name_Localised { get; set; }
            public string FriendlyName { get; set; }            
            public int Count { get; set; }
            public int Stolen { get; set; }
            public ulong? MissionID { get; set; }             // if applicable

            public void Normalise()
            {
                Name = JournalFieldNaming.FDNameTranslation(Name);
                FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);
            }
        }

        public JournalCargo(JObject evt) : base(evt, JournalTypeEnum.Cargo)
        {
            //System.Diagnostics.Debug.WriteLine("Cargo at " + EventTimeUTC);
            Rescan(evt);
        }

        void Rescan(JObject evt)
        {
            Vessel = evt["Vessel"].Str("Ship");         // ship is the default.. present in 3.3 only.  Other value SRV

            Inventory = evt["Inventory"]?.ToObjectQ<Cargo[]>()?.ToArray();

            EDDFromFile = evt["EDDFromFile"].Bool(false);  // EDD marker its from file - will only be present if cargo > Nov 20 and it was read from file

            if (Inventory != null)
            {
                foreach (Cargo c in Inventory)
                    c.Normalise();
            }
        }
        
        public void ReadAdditionalFiles(string directory)
        {
            if (Inventory == null)  // so, if cargo contained info, we use that.. else we try for cargo.json.
            {
                //System.Diagnostics.Debug.WriteLine("Cargo with no data, checking file.." + historyrefreshparse);

                JObject jnew = ReadAdditionalFile(System.IO.Path.Combine(directory, "Cargo.json"), EventTypeStr);  

                if (jnew != null)        // new json, rescan. returns null if cargo in the folder is not related to this entry by time.
                {
                    jnew["EDDFromFile"] = true;  // mark its from file
                    Rescan(jnew);
                    UpdateJson(jnew);
                }
            }
        }

        public bool SameAs(JournalCargo other)      // concerning itself only with count, stolen and name
        {
            if (Inventory != null && other.Inventory != null && Inventory.Length == other.Inventory.Length)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    int otherindex = Array.FindIndex(other.Inventory, x => x.Name.Equals(Inventory[i].Name));
                    if (otherindex == -1)
                        return false;
                    if (Inventory[i].Count != other.Inventory[otherindex].Count || Inventory[i].Stolen != other.Inventory[otherindex].Stolen)
                        return false;
                }

                return true;
            }

            return false;
        }


        public string Vessel { get; set; }          // always set, Ship or SRV.
        public Cargo[] Inventory { get; set; }      // may be NULL
        public bool EDDFromFile { get; set; }       // set if from file, but only from nov 2020

        public override string GetInfo()
        {
            if (Inventory != null && Inventory.Length > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

                int total = 0;
                foreach (Cargo c in Inventory)
                    total += c.Count;

                sb.Append(Vessel.Equals("Ship") ? "Ship".T(EDCTx.JournalCargo_CargoShip) : "SRV".T(EDCTx.JournalCargo_CargoSRV));
                sb.AppendSPC();
                sb.AppendFormat("Cargo, {0} items".T(EDCTx.JournalEntry_Cargo), total);
                return sb.ToString();
            }
            else
                return "No Cargo".T(EDCTx.JournalEntry_NoCargo);
        }
        public override string GetDetailed()
        {
            if (Inventory != null && Inventory.Length > 0)
            {
                var sb = new System.Text.StringBuilder(1024);
                foreach (Cargo c in Inventory)
                {
                    int? stolen = null;
                    if (c.Stolen > 0)
                        stolen = c.Stolen;
                    sb.AppendCR();
                    sb.Build("", c.FriendlyName, "<: "+ "; items".T(EDCTx.JournalEntry_items), c.Count, "(;)", stolen, "<; (Mission Cargo)".T(EDCTx.JournalEntry_MissionCargo), c.MissionID != null);
                }
                return sb.ToString();
            }
            else
                return null;
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            if (Vessel.Equals("Ship"))      // only want ship cargo to change lists..
            {
                if (Inventory != null)
                {
                    List<Tuple<string, int>> counts = Inventory.Select(x => new Tuple<string, int>(x.Name.ToLowerInvariant(), x.Count)).ToList();
                    mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, counts);
                }
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.EjectCargo)]
    public class JournalEjectCargo : JournalEntry, ICommodityJournalEntry
    {
        public JournalEjectCargo(JObject evt) : base(evt, JournalTypeEnum.EjectCargo)
        {
            Type = evt["Type"].Str();       // fdname
            Type = JournalFieldNaming.FDNameTranslation(Type);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyType = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Type);
            Type_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Type_Localised"].Str(), FriendlyType);         // always ensure we have one

            Count = evt["Count"].Int();
            Abandoned = evt["Abandoned"].Bool();
            PowerplayOrigin = evt["PowerplayOrigin"].Str();
            MissionID = evt["MissionID"].ULongNull();
        }

        public string Type { get; set; }                    // FDName
        public string FriendlyType { get; set; }            // translated name
        public string Type_Localised { get; set; }            // always set

        public int Count { get; set; }
        public bool Abandoned { get; set; }
        public string PowerplayOrigin { get; set; }
        public ulong? MissionID { get; set; }             // if applicable

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd( EventTimeUTC, Type, -Count, 0);   // same in the srv or ship, we lose count
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Type_Localised, "Count: ".T(EDCTx.JournalEntry_Count), Count,
                            "<; (Mission Cargo)".T(EDCTx.JournalEntry_MissionCargo), MissionID != null,
                            ";Abandoned".T(EDCTx.JournalEntry_Abandoned), Abandoned, "PowerPlay: ".T(EDCTx.JournalEntry_PowerPlay), PowerplayOrigin);
        }
    }

    [JournalEntryType(JournalTypeEnum.CargoDepot)]
    public class JournalCargoDepot : JournalEntry, ICommodityJournalEntry, IMissions
    {
        public JournalCargoDepot(JObject evt) : base(evt, JournalTypeEnum.CargoDepot)
        {
            MissionId = evt["MissionID"].ULong();
            UpdateType = evt["UpdateType"].Str();        // must be FD name
            System.Enum.TryParse<UpdateTypeEnum>(UpdateType, out UpdateTypeEnum u);
            UpdateEnum = u;
            CargoType = evt["CargoType"].Str();     // item counts
            FriendlyCargoType = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(CargoType);
            Count = evt["Count"].Int(0);
            StartMarketID = evt["StartMarketID"].Long();
            EndMarketID = evt["EndMarketID"].Long();
            ItemsCollected = evt["ItemsCollected"].Int();
            ItemsDelivered = evt["ItemsDelivered"].Int();
            TotalItemsToDeliver = evt["TotalItemsToDeliver"].Int();
            ItemsToGo = TotalItemsToDeliver - ItemsDelivered;
            ProgressPercent = evt["Progress"].Double() * 100;

            if (ProgressPercent < 0.01)
                ProgressPercent = ((double)System.Math.Max(ItemsCollected, ItemsDelivered) / (double)TotalItemsToDeliver) * 100;
        }

        public enum UpdateTypeEnum { Unknown, Collect, Deliver, WingUpdate }

        public ulong MissionId { get; set; }
        public string UpdateType { get; set; }
        public UpdateTypeEnum UpdateEnum { get; set; }

        public string CargoType { get; set; } // 3.03       deliver/collect only    - what you have done now.  Blank if not known (<3.03)
        public string FriendlyCargoType { get; set; }
        public int Count { get; set; }  // 3.03         deliver/collect only.  0 if not known.

        public long StartMarketID { get; set; }
        public long EndMarketID { get; set; }

        public int ItemsCollected { get; set; }             // current total stats
        public int ItemsDelivered { get; set; }
        public int ItemsToGo { get; set; }
        public int TotalItemsToDeliver { get; set; }
        public double ProgressPercent { get; set; }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            if (CargoType.Length > 0 && Count > 0)
                mc.ChangeCommd( EventTimeUTC, CargoType, (UpdateEnum == UpdateTypeEnum.Collect) ? Count : -Count, 0);
        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.CargoDepot(this);
        }

        public override string GetInfo()
        {
            if (UpdateEnum == UpdateTypeEnum.Collect)
            {
                return BaseUtils.FieldBuilder.Build("Collected: ".T(EDCTx.JournalEntry_Collected), Count, "< of ".T(EDCTx.JournalEntry_of), FriendlyCargoType, "Total: ".T(EDCTx.JournalEntry_Total), ItemsDelivered, "To Go:".T(EDCTx.JournalEntry_ToGo), ItemsToGo, "Progress: ;%;N1".T(EDCTx.JournalEntry_Progress), ProgressPercent);
            }
            else if (UpdateEnum == UpdateTypeEnum.Deliver)
            {
                return BaseUtils.FieldBuilder.Build("Delivered: ".T(EDCTx.JournalEntry_Delivered), Count, "< of ".T(EDCTx.JournalEntry_of), FriendlyCargoType, "Total: ".T(EDCTx.JournalEntry_Total), ItemsDelivered, "To Go:".T(EDCTx.JournalEntry_ToGo), ItemsToGo, "Progress: ;%;N1".T(EDCTx.JournalEntry_Progress), ProgressPercent);
            }
            else if (UpdateEnum == UpdateTypeEnum.WingUpdate)
            {
                return BaseUtils.FieldBuilder.Build("Update, Collected: ".T(EDCTx.JournalEntry_Update), ItemsCollected, "Delivered: ".T(EDCTx.JournalEntry_Delivered), ItemsDelivered, "To Go:".T(EDCTx.JournalEntry_ToGo), ItemsToGo, "Progress Left: ;%;N1".T(EDCTx.JournalEntry_ProgressLeft), ProgressPercent);
            }
            else
            {
                return "Unknown CargoDepot type " + UpdateType;
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.CollectCargo)]
    public class JournalCollectCargo : JournalEntry, ICommodityJournalEntry
    {
        public JournalCollectCargo(JObject evt) : base(evt, JournalTypeEnum.CollectCargo)
        {
            Type = evt["Type"].Str();                               //FDNAME
            Type = JournalFieldNaming.FDNameTranslation(Type);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyType = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Type);
            Type_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Type_Localised"].Str(), FriendlyType);         // always ensure we have one
            Stolen = evt["Stolen"].Bool();
            MissionID = evt["MissionID"].ULongNull();
        }

        public string Type { get; set; }                    // FDNAME..
        public string FriendlyType { get; set; }            // translated name
        public string Type_Localised { get; set; }            // always set
        public bool Stolen { get; set; }
        public ulong? MissionID { get; set; }             // if applicable

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool innormalspace) 
        {
            mc.ChangeCommd( EventTimeUTC, Type, 1, 0);     // collecting cargo in srv same as collecting cargo in ship. srv autotransfers it to ship
        }
        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Type_Localised, ";Stolen".T(EDCTx.JournalEntry_Stolen), Stolen, "<; (Mission Cargo)".T(EDCTx.JournalEntry_MissionCargo), MissionID != null);
        }
    }

    [JournalEntryType(JournalTypeEnum.CargoTransfer)]
    public class JournalCargoTransfer : JournalEntry, ICommodityJournalEntry
    {
        public class TransferClass
        {
            public string Type { get; set; }
            public string Type_Localised { get; set; }
            public ulong MissionID { get; set; }            // only on some types of transfers, not in journal doc, found in logs.
            public int Count { get; set; }
            public string Direction { get; set; }       // tocarrier , toship, tosrv
        }

        public TransferClass[] Transfers { get; set; }

        public JournalCargoTransfer(JObject evt) : base(evt, JournalTypeEnum.CargoTransfer)
        {
            Transfers = evt["Transfers"].ToObjectQ<TransferClass[]>();
            if (Transfers != null)
            {
                foreach (var t in Transfers)
                    t.Type_Localised = JournalFieldNaming.CheckLocalisation(t.Type_Localised, t.Type);
            }
        }

        public override string GetInfo()
        {
            if (Transfers != null)
            {
                var sb = new System.Text.StringBuilder(256);
                foreach (var t in Transfers)
                {
                    string d = t.Direction.Replace("to", "To ", StringComparison.InvariantCultureIgnoreCase);
                    sb.AppendPrePad(t.Type_Localised + "->" + d, ", ");
                }
                return sb.ToString();
            }
            else
                return null;
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool insrv) 
        {
            if (Transfers != null && !insrv)        // being in the srv is the same as the ships hold, so ignore transfers
            {
                foreach (var t in Transfers)
                {
                    if (t.Direction.Contains("ship", StringComparison.InvariantCultureIgnoreCase))     // toship, with some leaway to allow fd to change their formatting in future
                        mc.ChangeCommd( EventTimeUTC, t.Type, t.Count, 0);
                    else
                        mc.ChangeCommd( EventTimeUTC, t.Type, -t.Count, 0);
                }
            }
        }
    }
}
