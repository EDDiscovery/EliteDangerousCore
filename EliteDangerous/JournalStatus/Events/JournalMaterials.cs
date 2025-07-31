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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Materials)]
    public class JournalMaterials : JournalEntry, IMaterialJournalEntry
    {
        [System.Diagnostics.DebuggerDisplay("Mat {Name} {Count}")]
        public class Material
        {
            public string Name { get; set; }        //FDNAME
            public string Name_Localised { get; set; }    
            public string FriendlyName { get; set; }        //friendly
            public int Count { get; set; }

            public void Normalise()
            {
                Name = JournalFieldNaming.FDNameTranslation(Name);
                FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);
            }
        }

        public JournalMaterials(JObject evt) : base(evt, JournalTypeEnum.Materials)
        {
            Raw = evt["Raw"]?.ToObjectQ<Material[]>()?.ToArray();
            FixNames(Raw);
            Manufactured = evt["Manufactured"]?.ToObjectQ<Material[]>()?.ToArray();
            FixNames(Manufactured);
            Encoded = evt["Encoded"]?.ToObjectQ<Material[]>()?.ToArray();
            FixNames(Encoded);
        }

        public Material[] Raw { get; set; }             //FDNAMES on purpose
        public Material[] Manufactured { get; set; }
        public Material[] Encoded { get; set; }

        void FixNames(Material[] a)
        {
            if (a != null)
            {
                foreach (Material m in a)
                    m.Normalise();
            }
        }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (Raw != null && Raw.Length > 0)
            {
                sb.Build("Raw: ".Tx()+ "; ", Raw.Length);
            }
            if (Manufactured != null && Manufactured.Length > 0)
            {
                sb.BuildCont("Manufactured: ".Tx()+ "; ", Manufactured.Length);
            }
            if (Encoded != null && Encoded.Length > 0)
            {
                sb.BuildCont("Encoded: ".Tx()+ "; ", Encoded.Length);
            }
            return sb.ToString();
        }


        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (Raw != null && Raw.Length>0)
            {
                sb.Append("Raw: ".Tx());
                List(sb,Raw);
            }
            if (Manufactured != null && Manufactured.Length > 0)
            {
                sb.Append("Manufactured: ".Tx());
                List(sb, Manufactured);
            }
            if (Encoded != null && Encoded.Length > 0)
            {
                sb.Append("Encoded: ".Tx());
                List(sb, Encoded);
            }

            return sb.ToString();
        }

        public void List(StringBuilder sb, Material[] mat)
        {
            foreach (Material m in mat)
            {
                sb.Append(BaseUtils.FieldBuilder.Build(" ", m.FriendlyName, "; items".Tx(), m.Count));
                sb.AppendCR();
            }
        }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Raw != null)
            {
                List<Tuple<string, int>> counts = Raw.Select(x => new Tuple<string, int>(x.Name.ToLowerInvariant(), x.Count)).ToList();
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Raw, counts);
            }
            if ( Manufactured != null )
            {
                List<Tuple<string, int>> counts = Manufactured.Select(x => new Tuple<string, int>(x.Name.ToLowerInvariant(), x.Count)).ToList();
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Manufactured, counts);
            }
            if ( Encoded != null )
            {
                List<Tuple<string, int>> counts = Encoded.Select(x => new Tuple<string, int>(x.Name.ToLowerInvariant(), x.Count)).ToList();
                mc.Update(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Encoded, counts);
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.MaterialCollected)]
    public class JournalMaterialCollected : JournalEntry, IMaterialJournalEntry
    {
        public JournalMaterialCollected(JObject evt) : base(evt, JournalTypeEnum.MaterialCollected)
        {
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);
            Count = evt["Count"].Int(1);
        }
        public string Category { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

        public int Total { get; set; }      // found from MCL

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            mc.ChangeMat( EventTimeUTC, Category, Name, Count);

            Total = mc.GetLast(Name)?.Count ?? 0;
        }

        public override string GetInfo()
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                return BaseUtils.FieldBuilder.Build("", FriendlyName, "< (", mcd.TranslatedCategory, ";)", mcd.TranslatedType, "< ; items".Tx(), Count, "Total: ".Tx(), Total);
            else
                return BaseUtils.FieldBuilder.Build("", FriendlyName, "< ; items".Tx(), Count);
        }
    }

    [JournalEntryType(JournalTypeEnum.MaterialDiscarded)]
    public class JournalMaterialDiscarded : JournalEntry, IMaterialJournalEntry
    {
        public JournalMaterialDiscarded(JObject evt) : base(evt, JournalTypeEnum.MaterialDiscarded)
        {
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);
            Count = evt["Count"].Int();
        }

        public string Category { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }    // FDName
        public int Count { get; set; }

        public int Total { get; set; }      // found from MCL

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            mc.ChangeMat( EventTimeUTC, Category, Name, -Count);
            Total = mc.GetLast(Name)?.Count ?? 0;
        }

        public override string GetInfo()
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                return BaseUtils.FieldBuilder.Build("", FriendlyName, "< (", mcd.TranslatedCategory, ";)", mcd.TranslatedType, "< ; items".Tx(), Count, "Total: ".Tx(), Total);
            else
                return BaseUtils.FieldBuilder.Build("", FriendlyName, "< ; items".Tx(), Count);
        }
    }

    [JournalEntryType(JournalTypeEnum.MaterialDiscovered)]
    public class JournalMaterialDiscovered : JournalEntry
    {
        public JournalMaterialDiscovered(JObject evt) : base(evt, JournalTypeEnum.MaterialDiscovered)
        {
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyName = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Name);
            DiscoveryNumber = evt["DiscoveryNumber"].Int();
        }

        public string Category { get; set; }
        public string Name { get; set; }    // FDName
        public string FriendlyName { get; set; }
        public int DiscoveryNumber { get; set; }

        public override string GetInfo()
        {
            string info = BaseUtils.FieldBuilder.Build("", FriendlyName);
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                info += BaseUtils.FieldBuilder.Build(" (", mcd.TranslatedCategory, ";)", mcd.TranslatedType);

            if (DiscoveryNumber > 0)
                info += string.Format(", Discovery {0}".Tx(), DiscoveryNumber);

            return info;
        }
    }


    [JournalEntryType(JournalTypeEnum.MaterialTrade)]
    public class JournalMaterialTrade : JournalEntry, IMaterialJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalMaterialTrade(JObject evt) : base(evt, JournalTypeEnum.MaterialTrade)
        {
            MarketID = evt["MarketID"].LongNull();
            TraderType = evt["TraderType"].Str();

            Paid = evt["Paid"]?.ToObjectQ<Traded>();
            if (Paid != null)
                Paid.Normalise();

            Received = evt["Received"]?.ToObjectQ<Traded>();
            if (Received != null)
                Received.Normalise();
        }

        public string TraderType { get; set; }
        public long? MarketID { get; set; }
        public Traded Paid { get; set; }      // may be null
        public Traded Received { get; set; } // may be null

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Paid.Material, Count = -Paid.Quantity },
                                                                                            new IStatsItemsInfo() { FDName = Received.Material, Count = Received.Quantity }
                                                                                            }; } }
        public class Traded
        {
            public string Material;     //fdname
            public string FriendlyMaterial; // our name
            public string Material_Localised;   // their localised name if present
            public string Category;     // journal says always there.  If not, use tradertype
            public string Category_Localised;
            public int Quantity;

            public void Normalise()
            {
                Material = JournalFieldNaming.FDNameTranslation(Material);
                FriendlyMaterial = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Material);
                Material_Localised = JournalFieldNaming.CheckLocalisationTranslation(Material_Localised ?? "", FriendlyMaterial);       // ensure.

                if (Category != null)       // some entries do not have this
                {
                    Category = JournalFieldNaming.NormaliseMaterialCategory(Category);  // fix up any strangeness
                    Category_Localised = JournalFieldNaming.CheckLocalisation(Category_Localised ?? "", Category);
                }
            }
        }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Paid != null && Received != null)
            {
                mc.ChangeMat( EventTimeUTC, Paid.Category.Alt(TraderType), Paid.Material, -Paid.Quantity);        // use faction - person your using to swap
                mc.ChangeMat( EventTimeUTC, Received.Category.Alt(TraderType), Received.Material, Received.Quantity);
            }
        }


        public void UpdateStats(Stats stats, ISystem system, string stationfaction)
        {
            if (stationfaction.HasChars())
            {
                stats.UpdateMaterial(system, Paid.Material, -Paid.Quantity, stationfaction);
                stats.UpdateMaterial(system, Received.Material, Received.Quantity, stationfaction);
            }
        }

        public override string GetInfo()
        {
            if (Paid != null && Received != null)
            {
                return BaseUtils.FieldBuilder.Build("Sold: ".Tx(), Paid.Quantity, "< ", Paid.Material_Localised,
                                                    "Received: ".Tx(), Received.Quantity, "< ", Received.Material_Localised);
            }
            else
                return "";
        }
    }


    [JournalEntryType(JournalTypeEnum.Synthesis)]
    public class JournalSynthesis : JournalEntry, IMaterialJournalEntry
    {
        public JournalSynthesis(JObject evt) : base(evt, JournalTypeEnum.Synthesis)
        {
            Materials = null;

            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.Synthesis(FDName);
            JToken mats = (JToken)evt["Materials"];

            if (mats != null)
            {
                Materials = new Dictionary<string, int>();

                if (mats.IsObject)
                {
                    Dictionary<string, int> temp = mats?.ToObjectQ<Dictionary<string, int>>();

                    if (temp != null)
                    {
                        foreach (string key in temp.Keys)
                            Materials[JournalFieldNaming.FDNameTranslation(key)] = temp[key];
                    }
                }
                else
                {
                    foreach (JObject ja in (JArray)mats)
                    {
                        Materials[JournalFieldNaming.FDNameTranslation(ja["Name"].Str("Default"))] = ja["Count"].Int();
                    }
                }
            }

            // for quick use, work out some extra info for jet cone boosts

            if (Name.Contains("FSD Basic", StringComparison.InvariantCultureIgnoreCase))
                FSDBoostValue = 1.25;
            else if (Name.Contains("FSD Standard", StringComparison.InvariantCultureIgnoreCase))
                FSDBoostValue = 1.5;
            else if (Name.Contains("FSD Premium", StringComparison.InvariantCultureIgnoreCase))
                FSDBoostValue = 2;

        }
        public string Name { get; set; }        // Friendly name
        public string FDName { get; set; }        // FDName
        public Dictionary<string, int> Materials { get; set; }

        public double FSDBoostValue { get; set; }           // set non zero if its a FSD injection
        
        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Materials != null)
            {
                foreach (KeyValuePair<string, int> k in Materials)        // may be commodities or materials
                    mc.Craft(EventTimeUTC, k.Key, k.Value);        // same as this, uses up materials
            }

            if (Name.Contains("Limpet", StringComparison.InvariantCultureIgnoreCase) )      // hard code limpets mean 1 more cargo of them
            {
                mc.ChangeCommd( EventTimeUTC, "drones", 1, 0);   // ignore faction - synthesis
            }
        }

        public override string GetInfo()
        {
            var sb = new System.Text.StringBuilder(256);
            sb.Append(Name);

            if (Materials != null)
            {
                foreach (KeyValuePair<string, int> k in Materials)
                    sb.AppendPrePad(MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(k.Key) + ": " + k.Value.ToString(), ", ");

            }
            return sb.ToString();
        }
    }


}
