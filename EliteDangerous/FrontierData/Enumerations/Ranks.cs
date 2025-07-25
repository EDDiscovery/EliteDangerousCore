/*
 * Copyright 2024-2024 EDDiscovery development team
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

using System;


namespace EliteDangerousCore
{
    public class RankDefinitions
    {
        public enum CombatRank
        {
            Unknown = -1,
            Harmless = 0,
            Mostly_Harmless,
            Novice,
            Competent,
            Expert,
            Master,
            Dangerous,
            Deadly,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static CombatRank CombatRankToEnum(string rank)
        {
            if (rank != null && Enum.TryParse<RankDefinitions.CombatRank>(rank.Replace(" ", "_"), true, out RankDefinitions.CombatRank cr))
                return cr;
            else
            {
                System.Diagnostics.Trace.WriteLine($"**** Unknown rank `{rank}`");
                return CombatRank.Unknown;
            }
        }

        public static string FriendlyName(CombatRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(CombatRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum TradeRank
        {
            Unknown = -1,
            Penniless = 0,
            Mostly_Penniless,
            Peddler,
            Dealer,
            Merchant,
            Broker,
            Entrepreneur,
            Tycoon,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static string FriendlyName(TradeRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }

        public static string FriendlyName(TradeRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum ExplorationRank
        {
            Unknown = -1,
            Aimless = 0,
            Mostly_Aimless,
            Scout,
            Surveyor,
            Explorer,
            Pathfinder,
            Ranger,
            Pioneer,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static string FriendlyName(ExplorationRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(ExplorationRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum FederationRank
        {
            Unknown = -1,
            None = 0,
            Recruit,
            Cadet,
            Midshipman,
            Petty_Officer,
            Chief_Petty_Officer,
            Warrant_Officer,
            Ensign,
            Lieutenant,
            Lt_Commander,
            Post_Commander,
            Post_Captain,
            Rear_Admiral,
            Vice_Admiral,
            Admiral
        }
        public static string FriendlyName(FederationRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(FederationRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }
        public enum EmpireRank
        {
            Unknown = -1,
            None = 0,
            Outsider,
            Serf,
            Master,
            Squire,
            Knight,
            Lord,
            Baron,
            Viscount,
            Count,
            Earl,
            Marquis,
            Duke,
            Prince,
            King
        }
        public static string FriendlyName(EmpireRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(EmpireRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum CQCRank
        {
            Unknown = -1,
            Helpless = 0,
            Mostly_Helpless,
            Amateur,
            Semi_Professional,
            Professional,
            Champion,
            Hero,
            Legend,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static string FriendlyName(CQCRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(CQCRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }


        public enum SquadronRank       // these, as of 1/11/2018, are provisional
        {
            Unknown = -1,
            Leader = 0,
            Senior_Officer = 1,
            Officer = 2,
            Agent = 3,
            Rookie = 4,
        }
        public static string FriendlyName(SquadronRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(SquadronRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum ExoBiologistRank
        {
            Directionless = 0,
            Mostly_Directionless,
            Compiler,
            Collector,
            Cataloguer,
            Taxonomist,
            Ecologist,
            Geneticist,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static string FriendlyName(ExoBiologistRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }
        public static string FriendlyName(ExoBiologistRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

        public enum SoldierRank
        {
            Defenceless = 0,
            Mostly_Defenceless,
            Rookie,
            Soldier,
            Gunslinger,
            Warrior,
            Gladiator,
            Deadeye,
            Elite,
            Elite_I,
            Elite_II,
            Elite_III,
            Elite_IV,
            Elite_V
        }

        public static string FriendlyName(SoldierRank cr)
        {
            return cr.ToString().Replace("_", " ");
        }

        public static string FriendlyName(SoldierRank? cr, string unknown = "?")
        {
            return cr != null ? cr.ToString().Replace("_", " ") : unknown;
        }

    }
}
