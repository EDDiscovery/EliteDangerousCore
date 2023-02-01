/*
 * Copyright © 2016-2022 EDDiscovery development team
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

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Rank)]
    public class JournalRank : JournalEntry
    {
        public JournalRank(JObject evt ) : base(evt, JournalTypeEnum.Rank)
        {
            Combat = (CombatRank)evt["Combat"].Int();
            Trade = (TradeRank)evt["Trade"].Int();
            Explore = (ExplorationRank)evt["Explore"].Int();
            Soldier = (SoldierRank)evt["Soldier"].Int();
            ExoBiologist = (ExoBiologistRank)evt["Exobiologist"].Int();
            Empire = (EmpireRank)evt["Empire"].Int();
            Federation = (FederationRank)evt["Federation"].Int();
            CQC = (CQCRank)evt["CQC"].Int();
        }

        public CombatRank Combat { get; set; }
        public TradeRank Trade { get; set; }
        public ExplorationRank Explore { get; set; }
        public SoldierRank Soldier { get; set; }
        public ExoBiologistRank ExoBiologist { get; set; }
        public EmpireRank Empire { get; set; }
        public FederationRank Federation { get; set; }
        public CQCRank CQC { get; set; }

        public override void FillInformation(FillInformationData fidunused, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("", Combat.ToString().Replace("_", " "),
                                      "", Trade.ToString().Replace("_", " "),
                                      "", Explore.ToString().Replace("_", " "),
                                      "", Soldier.ToString().Replace("_", " "),
                                      "", ExoBiologist.ToString().Replace("_", " "),
                                      "", Empire.ToString().Replace("_", " "),
                                      "", Federation.ToString().Replace("_", " "),                                      
                                      "", CQC.ToString().Replace("_", " "));
            detailed = "";
        }

        public bool Equals(JournalRank other)
        {
            return Combat == other.Combat && Trade == other.Trade && Explore == other.Explore && Soldier == other.Soldier && ExoBiologist == other.ExoBiologist &&
                            Empire == other.Empire && Federation == other.Federation && CQC == other.CQC;
                            
        }

        static public string[] TranslatedRankNames()      // list of rank names in standard order
        {
            return new string[]
            {
                "Combat".T(EDCTx.JournalPromotion_Combat),
                "Trade".T(EDCTx.JournalPromotion_Trade),
                "Exploration".T(EDCTx.JournalPromotion_Exploration),
                "Soldier".T(EDCTx.JournalPromotion_Soldier),
                "ExoBiologist".T(EDCTx.JournalPromotion_ExoBiologist),
                "Empire".T(EDCTx.JournalPromotion_Empire),
                "Federation".T(EDCTx.JournalPromotion_Federation),
                "CQC".T(EDCTx.JournalPromotion_CQC),
            };
        }
    }


    [JournalEntryType(JournalTypeEnum.Promotion)]
    public class JournalPromotion : JournalEntry
    {
        public JournalPromotion(JObject evt) : base(evt, JournalTypeEnum.Promotion)
        {
            int? c = evt["Combat"].IntNull();
            if (c.HasValue)
                Combat = (CombatRank)c.Value;

            int? t = evt["Trade"].IntNull();
            if (t.HasValue)
                Trade = (TradeRank)t;

            int? e = evt["Explore"].IntNull();
            if (e.HasValue)
                Explore = (ExplorationRank)e;

            int? b = evt["Exobiologist"].IntNull();
            if (b.HasValue)
                ExoBiologist = (ExoBiologistRank)b;

            int? s = evt["Soldier"].IntNull();
            if (s.HasValue)
                Soldier = (SoldierRank)s;

            int? q = evt["CQC"].IntNull();
            if (q.HasValue)
                CQC = (CQCRank)q;

            int? f = evt["Federation"].IntNull();
            if (f.HasValue)
                Federation = (FederationRank)f;

            int? evilempire = evt["Empire"].IntNull();
            if (evilempire.HasValue)
                Empire = (EmpireRank)evilempire;
        }

        public CombatRank? Combat { get; set; }
        public TradeRank? Trade { get; set; }
        public ExplorationRank? Explore { get; set; }
        public SoldierRank? Soldier { get; set; }
        public ExoBiologistRank? ExoBiologist { get; set; }
        public EmpireRank? Empire { get; set; }
        public FederationRank? Federation { get; set; }
        public CQCRank? CQC { get; set; }

        public override void FillInformation(FillInformationData fidunused, out string info, out string detailed)
        {
            var names = JournalRank.TranslatedRankNames();

            info = BaseUtils.FieldBuilder.Build(names[0] + ": ", Combat.HasValue ? Combat.ToString().Replace("_", " ") : null,
                                      names[1] + ": ", Trade.HasValue ? Trade.ToString().Replace("_", " ") : null,
                                      names[2] + ": ", Explore.HasValue ? Explore.ToString().Replace("_", " ") : null,
                                      names[3] + ": ", Soldier.HasValue ? Soldier.ToString().Replace("_", " ") : null,
                                      names[4] + ": ", ExoBiologist.HasValue ? ExoBiologist.ToString().Replace("_", " ") : null,
                                      names[5] + ": ", Empire.HasValue ? Empire.ToString().Replace("_", " ") : null,
                                      names[6] + ": ", Federation.HasValue ? Federation.ToString().Replace("_", " ") : null,
                                      names[7] + ": ", CQC.HasValue ? CQC.ToString().Replace("_", " ") : null);
            detailed = "";
        }

    }

    [JournalEntryType(JournalTypeEnum.Progress)]
    public class JournalProgress : JournalEntry
    {
        public JournalProgress(JObject evt) : base(evt, JournalTypeEnum.Progress)
        {
            Combat = evt["Combat"].Int();
            Trade = evt["Trade"].Int();
            Explore = evt["Explore"].Int();
            Soldier = evt["Soldier"].Int();
            ExoBiologist = evt["Exobiologist"].Int();
            Empire = evt["Empire"].Int();
            Federation = evt["Federation"].Int();
            CQC = evt["CQC"].Int();
        }

        public int Combat { get; set; }         // keep ints for backwards compat
        public int Trade { get; set; }
        public int Explore { get; set; }
        public int Soldier { get; set; }
        public int ExoBiologist { get; set; }
        public int Empire { get; set; }
        public int Federation { get; set; }
        public int CQC { get; set; }

        public override void FillInformation(FillInformationData fidunused, out string info, out string detailed)
        {
            var names = JournalRank.TranslatedRankNames();
            info = BaseUtils.FieldBuilder.Build(
                                       names[0] + ": ;%", Combat,
                                      names[1] + ": ;%", Trade,
                                      names[2] + ": ;%", Explore,
                                      names[3] + ": ;%", Soldier,
                                      names[4] + ": ;%", ExoBiologist,
                                      names[5] + ": ;%", Empire,
                                      names[6] + ": ;%", Federation,
                                      names[7] + ": ;%", CQC);
            detailed = "";
        }
    }

}
