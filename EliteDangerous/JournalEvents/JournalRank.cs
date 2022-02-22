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
using System.Linq;

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
            SoldierRank = (SoldierRank)evt["Soldier"].Int();
            ExoBiologistRank = (ExoBiologistRank)evt["Exobiologist"].Int();
            Empire = (EmpireRank)evt["Empire"].Int();
            Federation = (FederationRank)evt["Federation"].Int();
            CQC = (CQCRank)evt["CQC"].Int();
        }

        public CombatRank Combat { get; set; }
        public TradeRank Trade { get; set; }
        public ExplorationRank Explore { get; set; }
        public EmpireRank Empire { get; set; }
        public FederationRank Federation { get; set; }
        public CQCRank CQC { get; set; }
        public SoldierRank SoldierRank { get; set; }
        public ExoBiologistRank ExoBiologistRank { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("", Combat.ToString().Replace("_", " "),
                                      "", Trade.ToString().Replace("_", " "),
                                      "", Explore.ToString().Replace("_", " "),
                                      "", SoldierRank.ToString().Replace("_", " "),
                                      "", ExoBiologistRank.ToString().Replace("_", " "),
                                      "", Empire.ToString().Replace("_", " "),
                                      "", Federation.ToString().Replace("_", " "),                                      
                                      "", CQC.ToString().Replace("_", " "));
            detailed = "";
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
        public CQCRank? CQC { get; set; }
        public FederationRank? Federation { get; set; }
        public EmpireRank? Empire { get; set; }
        public SoldierRank? Soldier { get; set; }
        public ExoBiologistRank? ExoBiologist { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Combat: ".T(EDCTx.JournalPromotion_Combat), Combat.HasValue ? Combat.ToString().Replace("_", " ") : null,
                                      "Trade: ".T(EDCTx.JournalPromotion_Trade), Trade.HasValue ? Trade.ToString().Replace("_", " ") : null,
                                      "Exploration: ".T(EDCTx.JournalPromotion_Exploration), Explore.HasValue ? Explore.ToString().Replace("_", " ") : null,
                                      "Soldier: ".T(EDCTx.JournalPromotion_Soldier), Soldier.HasValue ? Soldier.ToString().Replace("_", " ") : null,
                                      "ExoBiologist: ".T(EDCTx.JournalPromotion_ExoBiologist), ExoBiologist.HasValue ? ExoBiologist.ToString().Replace("_", " ") : null,
                                      "Empire: ".T(EDCTx.JournalPromotion_Empire), Empire.HasValue ? Empire.ToString().Replace("_", " ") : null,
                                      "Federation: ".T(EDCTx.JournalPromotion_Federation), Federation.HasValue ? Federation.ToString().Replace("_", " ") : null,
                                      "CQC: ".T(EDCTx.JournalPromotion_CQC), CQC.HasValue ? CQC.ToString().Replace("_", " ") : null);
            detailed = "";
        }
    }

}
