/*
 * Copyright © 2016-2018 EDDiscovery development team
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
                FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            }
        }

        public JournalMaterials(JObject evt) : base(evt, JournalTypeEnum.Materials)
        {
            Raw = evt["Raw"]?.ToObjectQ<Material[]>()?.OrderBy(x => x.Name)?.ToArray();
            FixNames(Raw);
            Manufactured = evt["Manufactured"]?.ToObjectQ<Material[]>()?.OrderBy(x => x.Name)?.ToArray();
            FixNames(Manufactured);
            Encoded = evt["Encoded"]?.ToObjectQ<Material[]>()?.OrderBy(x => x.Name)?.ToArray();
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

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)  
        {
           
            info = "";
            detailed = "";
            if (Raw != null && Raw.Length>0)
            {
                info += BaseUtils.FieldBuilder.Build("Raw: ".T(EDCTx.JournalMaterials_Raw) + "; ", Raw.Length);
                detailed += "Raw: ".T(EDCTx.JournalMaterials_Raw) + List(Raw);
            }
            if (Manufactured != null && Manufactured.Length>0)
            {
                info += BaseUtils.FieldBuilder.Build("Manufactured: ".T(EDCTx.JournalMaterials_Manufactured) + "; ", Manufactured.Length);// NOT DONE
                if (detailed.Length > 0)
                    detailed += Environment.NewLine;
                detailed += "Manufactured: ".T(EDCTx.JournalMaterials_Manufactured) + List(Manufactured);
            }
            if (Encoded != null && Encoded.Length > 0)
            {
                info += BaseUtils.FieldBuilder.Build("Encoded: ".T(EDCTx.JournalMaterials_Encoded) + "; ", Encoded.Length);// NOT DONE
                if (detailed.Length > 0)
                    detailed += Environment.NewLine;
                detailed += "Encoded: ".T(EDCTx.JournalMaterials_Encoded) + List(Encoded);
            }
        }

        public string List(Material[] mat)
        {
            StringBuilder sb = new StringBuilder(64);

            foreach (Material m in mat)
            {
                sb.Append(Environment.NewLine);
                sb.Append(BaseUtils.FieldBuilder.Build(" ", m.FriendlyName, "; items".T(EDCTx.JournalEntry_items), m.Count));
            }
            return sb.ToString();
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
            FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            Count = evt["Count"].Int(1);
        }
        public string Category { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

        public int Total { get; set; }      // found from MCL

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            mc.Change( EventTimeUTC, Category, Name, Count, 0);

            Total = mc.GetLast(Name)?.Count ?? 0;
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< (", mcd.TranslatedCategory, ";)", mcd.TranslatedType, "< ; items".T(EDCTx.JournalEntry_MatC), Count, "Total: ".T(EDCTx.JournalEntry_Total), Total);
            else
                info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< ; items".T(EDCTx.JournalEntry_MatC), Count);

            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.MaterialDiscarded)]
    public class JournalMaterialDiscarded : JournalEntry, IMaterialJournalEntry
    {
        public JournalMaterialDiscarded(JObject evt) : base(evt, JournalTypeEnum.MaterialDiscarded)
        {
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            Count = evt["Count"].Int();
        }

        public string Category { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }    // FDName
        public int Count { get; set; }

        public int Total { get; set; }      // found from MCL

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            mc.Change( EventTimeUTC, Category, Name, -Count, 0);
            Total = mc.GetLast(Name)?.Count ?? 0;
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< (", mcd.TranslatedCategory, ";)", mcd.TranslatedType, "< ; items".T(EDCTx.JournalEntry_MatC), Count, "Total: ".T(EDCTx.JournalEntry_Total), Total);
            else
                info = BaseUtils.FieldBuilder.Build("", FriendlyName, "< ; items".T(EDCTx.JournalEntry_MatC), Count);
    
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.MaterialDiscovered)]
    public class JournalMaterialDiscovered : JournalEntry
    {
        public JournalMaterialDiscovered(JObject evt) : base(evt, JournalTypeEnum.MaterialDiscovered)
        {
            Category = JournalFieldNaming.NormaliseMaterialCategory(evt["Category"].Str());
            Name = JournalFieldNaming.FDNameTranslation(evt["Name"].Str());     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            DiscoveryNumber = evt["DiscoveryNumber"].Int();
        }

        public string Category { get; set; }
        public string Name { get; set; }    // FDName
        public string FriendlyName { get; set; }
        public int DiscoveryNumber { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", FriendlyName);
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Name);
            if (mcd != null)
                info += BaseUtils.FieldBuilder.Build(" (", mcd.TranslatedCategory, ";)", mcd.TranslatedType);

            if (DiscoveryNumber > 0)
                info += string.Format(", Discovery {0}".T(EDCTx.JournalMaterialDiscovered_DN), DiscoveryNumber);
            detailed = "";
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
                FriendlyMaterial = MaterialCommodityMicroResourceType.GetNameByFDName(Material);
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
                mc.Change( EventTimeUTC, Paid.Category.Alt(TraderType), Paid.Material, -Paid.Quantity, 0);        // use faction - person your using to swap
                mc.Change( EventTimeUTC, Received.Category.Alt(TraderType), Received.Material, Received.Quantity, 0);
            }
        }


        public void UpdateStats(Stats stats, string stationfaction)
        {
            if (stationfaction.HasChars())
            {
                stats.UpdateMaterial(Paid.Material, -Paid.Quantity, stationfaction);
                stats.UpdateMaterial(Received.Material, Received.Quantity, stationfaction);
            }
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = detailed = "";

            if (Paid != null && Received != null)
            {
                info = BaseUtils.FieldBuilder.Build("Sold: ".T(EDCTx.JournalEntry_Sold), Paid.Quantity, "< ", Paid.Material_Localised,
                                                    "Received: ".T(EDCTx.JournalEntry_Received), Received.Quantity, "< ", Received.Material_Localised);
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.Synthesis)]
    public class JournalSynthesis : JournalEntry, IMaterialJournalEntry
    {
        public JournalSynthesis(JObject evt) : base(evt, JournalTypeEnum.Synthesis)
        {
            Materials = null;

            Name = evt["Name"].Str().SplitCapsWordFull();
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
        public string Name { get; set; }
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
                mc.Change( EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, "drones", 1, 0);   // ignore faction - synthesis
            }
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = Name;
            if (Materials != null)
                foreach (KeyValuePair<string, int> k in Materials)
                    info += ", " + MaterialCommodityMicroResourceType.GetNameByFDName(k.Key) + ": " + k.Value.ToString();

            detailed = "";
        }
    }


}
