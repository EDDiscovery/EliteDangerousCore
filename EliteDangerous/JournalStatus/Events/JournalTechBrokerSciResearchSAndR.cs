/*
 * Copyright 2016-2025 EDDiscovery development team
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
using System.Linq;
using System.Windows.Forms.VisualStyles;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.TechnologyBroker)]
    public class JournalTechnologyBroker : JournalEntry, ICommodityJournalEntry, IMaterialJournalEntry
    {
        public JournalTechnologyBroker(JObject evt) : base(evt, JournalTypeEnum.TechnologyBroker)
        {
            string bt = evt["BrokerType"].Str("");
            BrokerType = Enum.TryParse<BrokerTypes>(bt,true,out BrokerTypes res) ? res : BrokerTypes.Unknown;
            if (bt.HasChars() && BrokerType == BrokerTypes.Unknown)
            {
                BaseUtils.Debugger.TraceBreak($"*** Unknown broker type `{evt["BrokerType"].Str()}` {EventTimeUTC}");
            }

            MarketID = new MarketID(evt["MarketID"]);

            ItemsUnlocked = evt["ItemsUnlocked"]?.ToObjectQ<Unlocked[]>();      //3.03 entry
            CommodityList = evt["Commodities"]?.ToObjectQ<Commodities[]>();
            MaterialList = evt["Materials"]?.ToObject<Materials[]>(false, process: MaterialCommodityMicroResourceType.ToCategory)?.ToArray();

            if (ItemsUnlocked != null)
            {
                foreach (Unlocked u in ItemsUnlocked)
                    u.Name_Localised = JournalFieldNaming.CheckLocalisation(u.Name_Localised ?? "", u.Name.ID);
            }

            if (CommodityList != null)
            {
                foreach (Commodities c in CommodityList)
                    c.FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(c.Name);
            }

            if (MaterialList != null)
            { 
                foreach (Materials m in MaterialList)
                {
                    m.FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(m.Name);
                }
            }

            string oldentry = evt["ItemUnlocked"].StrNull();        // 3.02 journal entry
            if (ItemsUnlocked == null && oldentry != null)
                ItemsUnlocked = new Unlocked[] { new Unlocked() { Name = new EngineerFDName(oldentry), Name_Localised = oldentry } };
        }


        public enum BrokerTypes
        {
            Unknown, Guardian, Human, Mixed, Rescue,  Salvation, Sirius, TorvalMining
        };

        public BrokerTypes BrokerType { get; set; }      
        public MarketID MarketID { get; set; }
        public Unlocked[] ItemsUnlocked { get; set; }
        public Materials[] MaterialList { get; set; }
        public Commodities[] CommodityList { get; set; }

        public class Unlocked
        {
            public EngineerFDName Name;
            public string Name_Localised;
        }

        public class Commodities
        {
            [JsonAlwaysCreate]
            public MCFDName Name;
            public string Name_Localised;
            public string FriendlyName;
            public int Count;
        }

        public class Materials
        {
            [JsonAlwaysCreate]
            public MCFDName Name;
            public string Name_Localised;
            public string FriendlyName;
            public MaterialCommodityMicroResourceType.CatType Category;
            public int Count;
        }

        public override string GetInfo() 
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Build("Type".Tx()+": ", BrokerType);

            if (ItemsUnlocked != null)
            {
                foreach (Unlocked u in ItemsUnlocked)
                    sb.AppendPrePad(u.Name_Localised, ", ");
            }

            return sb.ToString();
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (CommodityList != null)
                foreach (Commodities c in CommodityList)
                    sb.AppendPrePad(c.FriendlyName + ": " + c.Count.ToString(), ", ");

            if (MaterialList != null)
                foreach (Materials m in MaterialList)
                    sb.AppendPrePad(m.FriendlyName + ": " + m.Count.ToString(), ", ");

            return sb.Length>0 ? sb.ToString() : null;
        }
        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool insrv)
        {
            foreach (var cmd in CommodityList.EmptyIfNull())
                mc.ChangeCommd(EventTimeUTC, cmd.Name, -cmd.Count, 0);
        }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            foreach (var mat in MaterialList.EmptyIfNull())
                mc.ChangeMat(EventTimeUTC, mat.Category, mat.Name, -mat.Count);
        }
    }


    [JournalEntryType(JournalTypeEnum.ScientificResearch)]
    public class JournalScientificResearch : JournalEntry
    {
        public JournalScientificResearch(JObject evt) : base(evt, JournalTypeEnum.ScientificResearch)
        {
            Name = MCFDName.Normalise(evt["Name"].Str(), out string matname, this);
            FriendlyName = matname;
            Name_Localised = JournalFieldNaming.CheckLocalisation(evt["Name_Localised"].Str(), matname);
            Count = evt["Count"].Int();
            Category = MaterialCommodityMicroResourceType.ToCategory(evt["Category"].Str());
        }

        public MCFDName Name { get; set; }
        public string FriendlyName { get; set; }
        public string Name_Localised { get; set; }
        public int Count { get; set; }
        public MaterialCommodityMicroResourceType.CatType Category { get; set; }
        public MarketID MarketID { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name_Localised, "Count".Tx() + ": ", Count, "Category".Tx() + ": ", Category);
        }
    }

    [JournalEntryType(JournalTypeEnum.SearchAndRescue)]
    public class JournalSearchAndRescue : JournalEntry, ICommodityJournalEntry, ILedgerJournalEntry
    {
        public JournalSearchAndRescue(JObject evt) : base(evt, JournalTypeEnum.SearchAndRescue)
        {
            FDName = MCFDName.Normalise(evt["Name"].Str(), out string engname, this);
            FriendlyName = engname;
            Name_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Name_Localised"].Str(), FriendlyName);         // always ensure we have one
            Count = evt["Count"].Int();
            Reward = evt["Reward"].Long();
            MarketID = new MarketID(evt["MarketID"]);
        }

        public MCFDName FDName { get; set; }            // Hyperspace, Supercruise
        public string Name_Localised { get; set; }            // Hyperspace, Supercruise
        public string FriendlyName { get; set; }            // Hyperspace, Supercruise
        public int Count { get; set; }
        public long Reward { get; set; }
        public MarketID MarketID { get; set; }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.ChangeCommd(EventTimeUTC, FDName, -Count, 0);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Name_Localised, "Num".Tx() + ": ", Count, "Reward".Tx() + ": ", Reward);
        }

        public void Ledger(Ledger mcl)
        {
            if (Reward > 0)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name_Localised + " " + Count, Reward);
            }
        }
    }

}
