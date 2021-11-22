/*
 * Copyright © 2016 EDDiscovery development team
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

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
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

    public enum SquadronRank       // these, as of 1/11/2018, are provisional
    {
        Unknown = -1,
        Leader = 0,
        Senior_Officer = 1,
        Officer = 2,
        Agent = 3,
        Rookie = 4,
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
}
