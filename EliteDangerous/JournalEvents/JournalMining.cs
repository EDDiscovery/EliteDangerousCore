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
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.MiningRefined)]
    public class JournalMiningRefined : JournalEntry, ICommodityJournalEntry
    {
        public JournalMiningRefined(JObject evt) : base(evt, JournalTypeEnum.MiningRefined)
        {
            Type = JournalFieldNaming.FixCommodityName(evt["Type"].Str());          // instances of $.._name, translate to FDNAME
            Type = JournalFieldNaming.FDNameTranslation(Type);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyType = MaterialCommodityMicroResourceType.GetNameByFDName(Type);
            Type_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Type_Localised"].Str(), FriendlyType);
        }

        public string Type { get; set; }                                        // FIXED fdname always.. vital it stays this way
        public string FriendlyType { get; set; }
        public string Type_Localised { get; set; }

        public int Total { get; set; }      // found from MCL

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.Change( EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, Type, 1, 0);
            Total = mc.GetLast(Type)?.Count ?? 0;
        }

        public override void FillInformation(out string info, out string detailed)
        {
            MaterialCommodityMicroResourceType mcd = MaterialCommodityMicroResourceType.GetByFDName(Type);
            if (mcd != null)
                info = BaseUtils.FieldBuilder.Build("", Type_Localised, "< (", mcd.TranslatedCategory, ";)", mcd.TranslatedType, "Total: ".T(EDCTx.JournalEntry_Total), Total);
            else
                info = Type_Localised;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.AsteroidCracked)]
    public class JournalAsteroidCracked : JournalEntry
    {
        public JournalAsteroidCracked(JObject evt) : base(evt, JournalTypeEnum.AsteroidCracked)
        {
            Body = evt["Body"].Str();
        }

        public string Body { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = Body;
            detailed = "";
        }
    }

    [JournalEntryType(JournalTypeEnum.ProspectedAsteroid)]
    public class JournalProspectedAsteroid : JournalEntry
    {
        public class Material
        {
            public string Name { get; set; }        //FDNAME
            public string Name_Localised { get; set; }     
            public string FriendlyName { get; set; }        //friendly
            public double Proportion { get; set; }      // 0-100

            public void Normalise()
            {
                Name = JournalFieldNaming.FDNameTranslation(Name);
                FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
            }
        }

        public JournalProspectedAsteroid(JObject evt) : base(evt, JournalTypeEnum.ProspectedAsteroid)
        {
            Content = evt["Content"].Enumeration<AsteroidContent>(AsteroidContent.Low, x=>x.Replace("$AsteroidMaterialContent_","").Replace(";",""));
            Content_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["Content_Localised"].Str(), Content.ToString());

            MotherlodeMaterial = JournalFieldNaming.FDNameTranslation(evt["MotherlodeMaterial"].Str());
            FriendlyMotherlodeMaterial = MaterialCommodityMicroResourceType.GetNameByFDName(MotherlodeMaterial);
            MotherlodeMaterial_Localised = JournalFieldNaming.CheckLocalisationTranslation(evt["MotherlodeMaterial_Localised"].Str(),FriendlyMotherlodeMaterial);

            Remaining = evt["Remaining"].Double();      // 0-100
            Materials = evt["Materials"]?.ToObjectQ<Material[]>().OrderBy(x => x.Name)?.ToArray();

            if ( Materials != null )
            {
                foreach (Material m in Materials)
                    m.Normalise();
            }
        }

        public enum AsteroidContent { Low, Medium, High };

        public AsteroidContent Content { get; set; }
        public string Content_Localised { get; set; }

        public string MotherlodeMaterial { get; set; }
        public string MotherlodeMaterial_Localised { get; set; }
        public string FriendlyMotherlodeMaterial { get; set; }

        public double Remaining { get; set; }
        public Material[] Materials { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", FriendlyMotherlodeMaterial, "", Content_Localised, "Remaining: ;%;N1".T(EDCTx.JournalProspectedAsteroid_Remaining), Remaining);
            
            if ( Materials != null )
            {
                info += " ";
                foreach (Material m in Materials)
                {
                    info = info.AppendPrePad( BaseUtils.FieldBuilder.Build("", m.FriendlyName, "< ;%;N1", m.Proportion), System.Environment.NewLine );
                }
            }

            detailed = "";
        }
    }

}
