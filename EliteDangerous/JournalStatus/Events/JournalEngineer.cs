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

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.EngineerApply)]
    public class JournalEngineerApply : JournalEntry
    {
        public JournalEngineerApply(JObject evt) : base(evt, JournalTypeEnum.EngineerApply)
        {
            Engineer = evt["Engineer"].Str();
            FDBlueprint = evt["Blueprint"].Str();
            Blueprint = JournalFieldNaming.Blueprint(FDBlueprint);
            Level = evt["Level"].Int();
            Override = evt["Override"].Str();
        }

        public string Engineer { get; set; }
        public string Blueprint { get; set; }       // friendly not fdev
        public string FDBlueprint { get; set; }       // fdname
        public int Level { get; set; }
        public string Override { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Engineer, "Blueprint".Tx()+": ", Blueprint, "Level".Tx()+": ", Level, "Override".Tx()+": ", Override);
        }
    }

    [JournalEntryType(JournalTypeEnum.EngineerContribution)]
    public class JournalEngineerContribution : JournalEntry, ILedgerJournalEntry, ICommodityJournalEntry, IMaterialJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalEngineerContribution(JObject evt) : base(evt, JournalTypeEnum.EngineerContribution)
        {
            Engineer = evt["Engineer"].Str();
            EngineerID = evt["EngineerID"].LongNull();
            Type = evt["Type"].Str();

            Commodity = evt["Commodity"].Str();
            Commodity = JournalFieldNaming.FDNameTranslation(Commodity);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyCommodity = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Commodity);
            Commodity_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Commodity_Localised"].Str(), FriendlyCommodity);

            Material = evt["Material"].Str();
            Material = JournalFieldNaming.FDNameTranslation(Material);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyMaterial = MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(Material);
            Material_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Material_Localised"].Str(), FriendlyMaterial);

            Quantity = evt["Quantity"].Int();
            TotalQuantity = evt["TotalQuantity"].Int();
        }

        public string Engineer { get; set; }
        public long? EngineerID { get; set; }
        public string Type { get; set; }

        public string FriendlyCommodity { get; set; }
        public string Commodity { get; set; }
        public string Commodity_Localised { get; set; }     // always set

        public string FriendlyMaterial { get; set; }
        public string Material { get; set; }
        public string Material_Localised { get; set; }      // always set

        public int Quantity { get; set; }
        public int TotalQuantity { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type == "Materials" ? Material : Commodity, Count = -Quantity } }; } }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Type.Equals("Materials"))
                mc.ChangeMat(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Raw.ToString(), Material, -Quantity);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            if (Type.Equals("Commodity"))
                mc.ChangeCommd(EventTimeUTC, Commodity, -Quantity, 0);
        }

        public void UpdateStats(Stats stats, ISystem system, string unusedstationfaction)
        {
            if (Type.Equals("Materials"))
                stats.UpdateEngineerMaterial(system, Engineer, Material, Quantity);
            if (Type.Equals("Commodity"))
                stats.UpdateEngineerCommodity(system, Engineer, Commodity, Quantity);
        }

        public void Ledger(Ledger mcl)
        {
            if (Type.Equals("Credits"))
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "Engineer Contribution Credits", -Quantity);
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Engineer, "Type".Tx()+": ", Type, "Commodity".Tx()+": ", Commodity_Localised,
                    "Material".Tx()+": ", Material_Localised, "Quantity".Tx()+": ", Quantity, "TotalQuantity".Tx()+": ", TotalQuantity);
        }
    }



    // Base class used for craft and legacy

    public class JournalEngineerCraftBase : JournalEntry, IMaterialJournalEntry, IShipInformation
    {
        public JournalEngineerCraftBase(JObject evt, JournalTypeEnum en) : base(evt, en)
        {
            SlotFD = ShipSlots.ToEnum(evt["Slot"].StrNull());       // may not be present, pass in null to indicate okay and set it to unknown
            Slot = ShipSlots.ToEnglish(SlotFD);

            ModuleFD = JournalFieldNaming.NormaliseFDItemName(evt["Module"].Str()); // may not be present
            Module = JournalFieldNaming.GetBetterEnglishModuleName(ModuleFD);

            Engineering = new EngineeringData(evt);
            if (!Engineering.IsValid)       // various frontier records across commanders show crap output
            {
                // System.Diagnostics.Trace.WriteLine($"Bad Engineering line Craft {evt.ToString()}");
                Engineering = null;
            }

            IsPreview = evt["IsPreview"].BoolNull();
            JToken ingredients = (JToken)evt["Ingredients"];

            if (ingredients != null)
            {
                Ingredients = new List<Ingrediant>();

                if (ingredients.IsObject)
                {
                    Dictionary<string, int> temp = ingredients?.ToObjectQ<Dictionary<string, int>>();

                    if (temp != null)
                    {
                        foreach (var kvp in temp)
                        {
                            string fdname = JournalFieldNaming.FDNameTranslation(kvp.Key);
                            string name = MaterialCommodityMicroResourceType.GetByFDName(fdname)?.EnglishName ?? fdname;
                            var i = new Ingrediant()
                            {
                                NameFD = fdname.ToLowerInvariant(),
                                Name_Localised = name,
                                Name = name,
                                Count = kvp.Value
                            };

                            Ingredients.Add(i);
                        };
                    }
                }
                else
                {
                    foreach (JObject jo in (JArray)ingredients)
                    {
                        string fdname = jo["Name"].StrNull();
                        if (fdname != null)     // must be present and non null
                        {
                            string name = MaterialCommodityMicroResourceType.GetByFDName(fdname)?.EnglishName ?? fdname;

                            var i = new Ingrediant()
                            {
                                NameFD = fdname.ToLowerInvariant(),
                                Name_Localised = jo["Name_Localised"].Str(name),
                                Name = name,
                                Count = jo["Count"].Int()
                            };

                            Ingredients.Add(i);
                        }
                    }
                }
            }

        }

        public string Slot { get; set; }        // English name, not present in v1 of this version
        public ShipSlots.Slot SlotFD { get; set; }
        public string Module { get; set; }      // English module name, not present in V1 of this version
        public string ModuleFD { get; set; }

        public EngineeringData Engineering { get; set; }        // may be null if engineering invalid, which some frontier modules have 

        public bool? IsPreview { get; set; }            // Only for legacy convert

        public class Ingrediant
        {
            public string Name { get; set; }            // json, then english name
            public string Name_Localised { get; set; }  // localised, or Name
            public int Count { get; set; }              // count

            public string NameFD { get; set; }          // normalised name
        }


        public List<Ingrediant> Ingredients { get; set; }  // always set
        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Ingredients != null)
            {
                foreach (var k in Ingredients)        // may be commodities or materials but mostly materials so we put it under this
                    mc.Craft(EventTimeUTC, k.NameFD, k.Count);
            }
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            if ((IsPreview == null || IsPreview.Value == false) && Engineering != null)
            {
                shp.EngineerCraft(this);
            }
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("In Slot".Tx()+": ", ShipSlots.ToLocalisedLanguage(SlotFD),
                "", JournalFieldNaming.GetForeignModuleName(ModuleFD, null),
                "By".Tx()+": ", Engineering?.Engineer,
                "Blueprint".Tx()+": ", Engineering?.FriendlyBlueprintName,
                "Level".Tx()+": ", Engineering?.Level);
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (Ingredients != null)
            {
                foreach (var i in Ingredients)        // may be commodities or materials
                {
                    sb.BuildCont("", MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(i.NameFD), "<:", i.Count);
                }
            }

            if (Engineering != null)
            {
                sb.AppendCR();
                Engineering.Build(sb);
            }

            return sb.ToString();
        }
    }

    [JournalEntryType(JournalTypeEnum.EngineerCraft)]
    public class JournalEngineerCraft : JournalEngineerCraftBase
    {
        public JournalEngineerCraft(JObject evt) : base(evt, JournalTypeEnum.EngineerCraft)
        {
        }
    }


    [JournalEntryType(JournalTypeEnum.EngineerLegacyConvert)]
    public class JournalLegacyConvert : JournalEngineerCraftBase
    {
        public JournalLegacyConvert(JObject evt) : base(evt, JournalTypeEnum.EngineerLegacyConvert)     // same as craft.
        {
        }
    }


    [JournalEntryType(JournalTypeEnum.EngineerProgress)]
    public class JournalEngineerProgress : JournalEntry
    {
        public class ProgressInformation
        {
            public string Engineer { get; set; } = "Unknown";           // some journals seen on 17/9/22 have no Engineer or EngineerID
            public long EngineerID { get; set; }
            public int? Rank { get; set; }       // only when unlocked
            public string Progress { get; set; }
            public int? RankProgress { get; set; }  // newish 3.x only when unlocked

            public bool Valid { get { return Engineer.HasChars() && !Engineer.EqualsIIC("Unknown"); } }    // valid..
        }

        public JournalEngineerProgress(JObject evt) : base(evt, JournalTypeEnum.EngineerProgress)
        {
            Engineers = evt["Engineers"]?.ToObjectQ<ProgressInformation[]>()?.OrderBy(x => x.Engineer)?.ToArray();       // 3.3 introduced this at startup

            if (Engineers == null)
            {
                Engineers = new ProgressInformation[1];
                Engineers[0] = new ProgressInformation();
                Engineers[0].Engineer = evt["Engineer"].Str();
                Engineers[0].EngineerID = evt["EngineerID"].Long();
                Engineers[0].Rank = evt["Rank"].IntNull();
                Engineers[0].Progress = evt["Progress"].Str();
                Engineers[0].RankProgress = evt["RankProgress"].IntNull();
            }
        }

        public ProgressInformation[] Engineers { get; set; }      // may be NULL if not startup or pre 3.3

        public override string GetInfo()
        {
            if (Engineers.Length == 1)
                return BaseUtils.FieldBuilder.Build("", Engineers[0].Engineer, "", Engineers[0].Progress, "Rank".Tx()+": ", Engineers[0].Rank, ";%", Engineers[0].RankProgress);
            else
                return BaseUtils.FieldBuilder.Build("Progress on ; Engineers".Tx(), Engineers.Length);

        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

            foreach (var p in Engineers)
            {
                sb.AppendCR();
                sb.Build("", p.Engineer, "", p.Progress, "Rank".Tx()+": ", p.Rank, ";%", p.RankProgress);
            }

            return sb.ToString();
        }

        public enum InviteState { UnknownEngineer, None, Invited, Unlocked };
        public InviteState Progress(string engineer)        // use in case text changed in frontier data
        {
            int found = Array.FindIndex(Engineers, x => x.Engineer.Equals(engineer, StringComparison.InvariantCultureIgnoreCase));
            if (found >= 0)
            {
                if (Engineers[found].Progress.Equals("Unlocked", StringComparison.InvariantCultureIgnoreCase))
                    return InviteState.Unlocked;
                if (Engineers[found].Progress.Equals("Invited", StringComparison.InvariantCultureIgnoreCase))
                    return InviteState.Invited;

                return InviteState.None;
            }
            else
                return InviteState.UnknownEngineer;
        }

        public string[] ApplyProgress(string[] engineers)
        {
            string[] ret = new string[engineers.Length];
            for (int i = 0; i < engineers.Length; i++)
            {
                ret[i] = engineers[i];

                int found = Array.FindIndex(Engineers, x => x.Engineer.Equals(engineers[i], StringComparison.InvariantCultureIgnoreCase));
                if (found >= 0)
                {
                    if (Engineers[found].Progress.Equals("Unlocked", StringComparison.InvariantCultureIgnoreCase))
                        ret[i] += "++";
                    if (Engineers[found].Progress.Equals("Invited", StringComparison.InvariantCultureIgnoreCase))
                        ret[i] += "~";
                }
            }

            return ret;
        }
    }

}
