/*
 * Copyright 2016-2026 EDDiscovery development team
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
    [JournalEntryType(JournalTypeEnum.EngineerApply)]
    public class JournalEngineerApply : JournalEntry
    {
        // now obsoleted by 3.0
        public JournalEngineerApply(JObject evt) : base(evt, JournalTypeEnum.EngineerApply)
        {
            Engineer = new EngineerFDName(evt["Engineer"].Str());
            Level = evt["Level"].Int();

            FDOverride = RecipeFDName.Normalise(evt["Override"].Str(), out string engname, this, true);      // may not be present
            if (FDOverride != null)
                Override = engname;

            FDBlueprint = RecipeFDName.Normalise(evt["Blueprint"].Str(), out engname, this);
            Blueprint = engname;
        }

        public EngineerFDName Engineer { get; set; }
        public RecipeFDName FDBlueprint { get; set; }        // fdname
        public string Blueprint { get; set; }           // friendly not fdev
        public int Level { get; set; }
        public RecipeFDName FDOverride { get; set; }        // may be null
        public string Override { get; set; }        // may be null

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("", Engineer.Str(), "Blueprint".Tx()+": ", Blueprint, "Level".Tx()+": ", Level, "Override".Tx()+": ", Override);
        }
    }

    [JournalEntryType(JournalTypeEnum.EngineerContribution)]
    public class JournalEngineerContribution : JournalEntry, ILedgerJournalEntry, ICommodityJournalEntry, IMaterialJournalEntry, IStatsJournalEntryMatCommod
    {
        public JournalEngineerContribution(JObject evt) : base(evt, JournalTypeEnum.EngineerContribution)
        {
            Engineer = new EngineerFDName(evt["Engineer"].Str());
            EngineerID = evt["EngineerID"].LongNull();

            Type = evt["Type"].Enumeration<ContributionType>(ContributionType.Unknown);
            if (Type == ContributionType.Unknown)
            {
                BaseUtils.Debugger.TraceBreak($"*** Unknown engineer type {evt["Type"].Str()}");
                ;
            }

            Commodity = MCFDName.Normalise(evt["Commodity"].Str(), out string engname, this, true);
            if (Commodity != null)
            {
                FriendlyCommodity = engname;
                Commodity_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Commodity_Localised"].Str(), FriendlyCommodity);
            }

            Material = MCFDName.Normalise(evt["Material"].Str(), out engname, this, true);
            if (Material != null)
            {
                FriendlyMaterial = engname;
                Material_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Material_Localised"].Str(), FriendlyMaterial);
            }

            Quantity = evt["Quantity"].Int();
            TotalQuantity = evt["TotalQuantity"].Int();
        }

        public EngineerFDName Engineer { get; set; }
        public long? EngineerID { get; set; }

        public enum ContributionType { Unknown, Bond, Bounty, Commodity, Credits, Materials };
        public ContributionType Type { get; set; }        // Commodity, Bounty, Bond, Materials

        public MCFDName Commodity { get; set; }           // may be null
        public string FriendlyCommodity { get; set; }   // may be null
        public string Commodity_Localised { get; set; }     // may be null

        public MCFDName Material { get; set; }            // may be null
        public string FriendlyMaterial { get; set; }    // may be null
        public string Material_Localised { get; set; }      // may be null

        public int Quantity { get; set; }
        public int TotalQuantity { get; set; }

        // Istats
        public List<IStatsItemsInfo> ItemsList { get { return new List<IStatsItemsInfo>() { new IStatsItemsInfo() { FDName = Type == ContributionType.Materials ? Material : Commodity, Count = -Quantity } }; } }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (Type.Equals("Materials"))
                mc.ChangeMat(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Raw, Material, -Quantity);
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
            return BaseUtils.FieldBuilder.Build("", Engineer.Str(), "Type".Tx()+": ", Type, "Commodity".Tx()+": ", Commodity_Localised,
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

            ModuleFD = ModFDName.Normalise(evt["Module"].Str(), out string engname, this, true);       // can be missing
            if ( ModuleFD!=null)
                Module = engname;

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
                            var fdname = MCFDName.Normalise(kvp.Key, out engname, this, true);
                            if (fdname != null)
                            {
                                var i = new Ingrediant()
                                {
                                    NameFD = fdname,
                                    Name_Localised = engname,
                                    Name = engname,
                                    Count = kvp.Value
                                };

                                Ingredients.Add(i);
                            }
                        };
                    }
                }
                else
                {
                    foreach (JObject jo in (JArray)ingredients)
                    {
                        var fdname = MCFDName.Normalise(jo["Name"].Str(), out engname, this, true);
                        if (fdname != null)     // must be present and non null
                        {
                            var i = new Ingrediant()
                            {
                                NameFD = fdname,
                                Name_Localised = jo["Name_Localised"].Str(engname),
                                Name = engname,
                                Count = jo["Count"].Int()
                            };

                            Ingredients.Add(i);
                        }
                    }
                }
            }

            Engineering = new EngineeringData(evt, this);
            if (!Engineering.IsValid)       // various frontier records across commanders show crap output
            {
                // System.Diagnostics.Trace.WriteLine($"Bad Engineering line Craft {evt.ToString()}");
                Engineering = null;
            }
        }

        public ShipSlots.Slot SlotFD { get; set; }      // may be unknown
        public string Slot { get; set; }        // English name, not present in v1 of this version

        public ModFDName ModuleFD { get; set; }    // may be null on very early ones
        public string Module { get; set; }      // English module name, not present in V1 of this version

        public EngineeringData Engineering { get; set; }        // may be null if engineering invalid, which some frontier modules have 

        public bool? IsPreview { get; set; }            // Only for legacy convert

        public class Ingrediant
        {
            public MCFDName NameFD { get; set; }          // normalised name
            public string Name { get; set; }            // json, then english name
            public string Name_Localised { get; set; }  // localised, or Name
            public int Count { get; set; }              // count
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
            if ((IsPreview == null || IsPreview.Value == false) && Engineering != null && ModuleFD != null)     // check all fields, some early ones had data missing
            {
                shp.EngineerCraft(this);
            }
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("In Slot".Tx()+": ", ShipSlots.ToLocalisedLanguage(SlotFD),
                "", ModuleFD?.GetForeignModuleName(),
                "By".Tx()+": ", Engineering?.Engineer.Str(),
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
        public enum ProgressType { Unknown, Known, Unlocked, Invited };

        public class ProgressInformation
        {
            [JsonAlwaysCreate]
            public EngineerFDName Engineer { get; set; }           // some journals seen on 17/9/22 have no Engineer or EngineerID so force creation with unknown
            public long EngineerID { get; set; }
            public int? Rank { get; set; }       // only when unlocked

            public ProgressType Progress { get; set; }
            public int? RankProgress { get; set; }  // newish 3.x only when unlocked

            public bool Valid { get { return Engineer.IsValid == true; } }    // valid..
        }

        public JournalEngineerProgress(JObject evt) : base(evt, JournalTypeEnum.EngineerProgress)
        {
            Engineers = evt["Engineers"]?.ToObjectQ<ProgressInformation[]>()?.OrderBy(x => x.Engineer)?.ToArray();       // 3.3 introduced this at startup

            if (Engineers == null)      // older records
            {
                Engineers = new ProgressInformation[1];
                Engineers[0] = new ProgressInformation();
                Engineers[0].Engineer = new EngineerFDName(evt["Engineer"].Str());
                Engineers[0].EngineerID = evt["EngineerID"].Long();
                Engineers[0].Rank = evt["Rank"].IntNull();
                Engineers[0].Progress = evt["Progress"].Enumeration<ProgressType>(ProgressType.Unknown);
                Engineers[0].RankProgress = evt["RankProgress"].IntNull();
            }
        }

        public ProgressInformation[] Engineers { get; set; }      // may be NULL if not startup or pre 3.3

        public override string GetInfo()
        {
            if (Engineers.Length == 1)
                return BaseUtils.FieldBuilder.Build("", Engineers[0].Engineer.Str(), "", Engineers[0].Progress, "Rank".Tx()+": ", Engineers[0].Rank, ";%", Engineers[0].RankProgress);
            else
                return BaseUtils.FieldBuilder.Build("Progress on ; Engineers".Tx(), Engineers.Length);

        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

            foreach (var p in Engineers)
            {
                sb.AppendCR();
                sb.Build("", p.Engineer.Str(), "", p.Progress, "Rank".Tx()+": ", p.Rank, ";%", p.RankProgress);
            }

            return sb.ToString();
        }

        public ProgressType? GetProgress(EngineerFDName engineer)        // use in case text changed in frontier data
        {
            int found = Array.FindIndex(Engineers, x => x.Engineer == engineer);
            if (found >= 0)
            {
                return Engineers[found].Progress;
            }
            else
                return null;
        }

        public string[] ApplyProgress(EngineerFDName[] engineers)
        {
            string[] ret = new string[engineers.Length];
            for (int i = 0; i < engineers.Length; i++)
            {
                ret[i] = engineers[i].Str();

                int found = Array.FindIndex(Engineers, x => x.Engineer == engineers[i]);
                if (found >= 0)
                {
                    if (Engineers[found].Progress == ProgressType.Unlocked)
                        ret[i] += "++";
                    if (Engineers[found].Progress == ProgressType.Invited)
                        ret[i] += "~";
                }
            }

            return ret;
        }
    }

}
