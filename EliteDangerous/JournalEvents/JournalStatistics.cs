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
using BaseUtils.JSON;
using System;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Statistics)]
    public class JournalStatistics : JournalEntry
    {
        public JournalStatistics(JObject evt ) : base(evt, JournalTypeEnum.Statistics)
        {
            BankAccount = evt["Bank_Account"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<BankAccountClass>() ?? new BankAccountClass();
            Combat = evt["Combat"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<CombatClass>() ?? new CombatClass();
            Crime = evt["Crime"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<CrimeClass>() ?? new CrimeClass();
            Smuggling = evt["Smuggling"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<SmugglingClass>() ?? new SmugglingClass();
            Trading = evt["Trading"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<TradingClass>() ?? new TradingClass();
            Mining = evt["Mining"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<MiningClass>() ?? new MiningClass();
            Exploration = evt["Exploration"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<ExplorationClass>() ?? new ExplorationClass();
            PassengerMissions = evt["Passengers"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("PassengersMissions")?.ToObjectQ<PassengerMissionsClass>() ?? new PassengerMissionsClass();
            SearchAndRescue = evt["Search_And_Rescue"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("SearchRescue")?.ToObjectQ<SearchAndRescueClass>() ?? new SearchAndRescueClass();
            Crafting = evt["Crafting"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<CraftingClass>() ?? new CraftingClass();
            Crew = evt["Crew"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("NpcCrew")?.ToObjectQ<CrewClass>() ?? new CrewClass();
            Multicrew = evt["Multicrew"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("Multicrew")?.ToObjectQ<MulticrewClass>() ?? new MulticrewClass();
            MaterialTraderStats = evt["Material_Trader_Stats"]?.RenameObjectFieldsUnderscores()?.ToObjectQ<MaterialTraderStatsClass>() ?? new MaterialTraderStatsClass();
            CQC = evt["CQC"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("CQC")?.ToObjectQ<CQCClass>() ?? new CQCClass();
            Exobiology = evt["Exobiology"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("Exobiology")?.ToObject<ExobiologyClass>() ?? new ExobiologyClass();

            FLEETCARRIER = evt["FLEETCARRIER"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("FLEETCARRIER")?.ToObject<FLEETCARRIERClass>(true,true) ?? new FLEETCARRIERClass();
            JToken dt = evt["FLEETCARRIER"].I("FLEETCARRIER_DISTANCE_TRAVELLED");   // this is a classic frontier eff up
            if ( dt !=null)
            {
                if (dt.IsString)        // used to be 292929 LY
                {
                    string s = dt.Str("0 LY");
                    int i = s.IndexOf(" ");
                    if (i >= 0)
                        FLEETCARRIER.DISTANCETRAVELLED = s.Substring(0, i).InvariantParseDouble(0);
                }
                else
                    FLEETCARRIER.DISTANCETRAVELLED = dt.Double(0);
            }
        }

        public BankAccountClass BankAccount { get; set; }
        public CombatClass Combat { get; set; }
        public CrimeClass Crime { get; set; }
        public SmugglingClass Smuggling { get; set; }
        public TradingClass Trading { get; set; }
        public MiningClass Mining { get; set; }
        public ExplorationClass Exploration { get; set; }
        public PassengerMissionsClass PassengerMissions { get; set; }
        public SearchAndRescueClass SearchAndRescue { get; set; }
        public CraftingClass Crafting { get; set; }
        public CrewClass Crew { get; set; }
        public MulticrewClass Multicrew { get; set; }
        public MaterialTraderStatsClass MaterialTraderStats { get; set; }
        public CQCClass CQC { get; set; }
        public FLEETCARRIERClass FLEETCARRIER { get; set; }
        public ExobiologyClass Exobiology { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("Wealth: ;cr;N0".T(EDTx.JournalEntry_Wealth), BankAccount.CurrentWealth, "Notoriety Index: ;;N0".T(EDTx.JournalEntry_NotorietyIndex), Crime.Notoriety);

            detailed = "Bank Account".T(EDTx.JournalStatistics_BankAccount) + Environment.NewLine + BankAccount?.Format() + Environment.NewLine +
                        "Combat".T(EDTx.JournalStatistics_Combat) + Environment.NewLine + Combat?.Format() + Environment.NewLine +
                        "Crime".T(EDTx.JournalStatistics_Crime) + Environment.NewLine + Crime?.Format() + Environment.NewLine +
                        "Smuggling".T(EDTx.JournalStatistics_Smuggling) + Environment.NewLine + Smuggling?.Format() + Environment.NewLine +
                        "Trading".T(EDTx.JournalStatistics_Trading) + Environment.NewLine + Trading?.Format() + Environment.NewLine +
                        "Mining".T(EDTx.JournalStatistics_Mining) + Environment.NewLine + Mining?.Format() + Environment.NewLine +
                        "Exploration".T(EDTx.JournalStatistics_Exploration) + Environment.NewLine + Exploration?.Format() + Environment.NewLine +
                        "Passengers".T(EDTx.JournalStatistics_Passengers) + Environment.NewLine + PassengerMissions?.Format() + Environment.NewLine +
                        "Search and Rescue".T(EDTx.JournalStatistics_SearchandRescue) + Environment.NewLine + SearchAndRescue?.Format() + Environment.NewLine +
                        "Engineers".T(EDTx.JournalStatistics_Engineers) + Environment.NewLine + Crafting?.Format() + Environment.NewLine +
                        "Crew".T(EDTx.JournalStatistics_Crew) + Environment.NewLine + Crew?.Format() + Environment.NewLine +
                        "Multicrew".T(EDTx.JournalStatistics_Multicrew) + Environment.NewLine + Multicrew?.Format() + Environment.NewLine +
                        "Materials and Commodity Trading".T(EDTx.JournalStatistics_MaterialsandCommodityTrading) + Environment.NewLine + MaterialTraderStats?.Format() + Environment.NewLine +
                        "CQC".T(EDTx.JournalStatistics_CQC) + Environment.NewLine + CQC?.Format() + Environment.NewLine +
                        "FLEETCARRIER".T(EDTx.JournalStatistics_FLEETCARRIER) + Environment.NewLine + FLEETCARRIER?.Format() + Environment.NewLine +
                        "Exobiology".T(EDTx.JournalStatistics_Exobiology) + Environment.NewLine + Exobiology.Format();
        }

        public class BankAccountClass
        {
            public long CurrentWealth { get; set; }
            public long SpentOnShips { get; set; }
            public long SpentOnOutfitting { get; set; }
            public long SpentOnRepairs { get; set; }
            public long SpentOnFuel { get; set; }
            public long SpentOnAmmoConsumables { get; set; }
            public int InsuranceClaims { get; set; }
            public long SpentOnInsurance { get; set; }
            public int OwnedShipCount { get; set; }
            public long SpentOnSuits { get; set; }
            public long SpentOnWeapons { get; set; }
            public long SpentOnSuitConsumables { get; set; }
            public int SuitsOwned { get; set; }
            public int WeaponsOwned { get; set; }
            public long SpentOnPremiumStock { get; set; }
            public int PremiumStockBought { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline, 
                    "Wealth: ;cr;N0".T(EDTx.BankAccountClass_Wealth),  CurrentWealth, 
                    "Spent on Ships: ;cr;N0".T(EDTx.BankAccountClass_SpentonShips), SpentOnShips,
                    "Spent on Outfitting: ;cr;N0".T(EDTx.BankAccountClass_SpentonOutfitting), SpentOnOutfitting, 
                    "Spent on Repairs: ;cr;N0".T(EDTx.BankAccountClass_SpentonRepairs), SpentOnRepairs, 
                    "Spent on Fuel: ;cr;N0".T(EDTx.BankAccountClass_SpentonFuel), SpentOnFuel,
                    "Spent on Ammo: ;cr;N0".T(EDTx.BankAccountClass_SpendonAmmo), SpentOnAmmoConsumables, 
                    "Insurance Claims: ;;N0".T(EDTx.BankAccountClass_InsuranceClaims), InsuranceClaims,
                    "Spent on Insurance: ;cr;N0".T(EDTx.BankAccountClass_SpentonInsurance), SpentOnInsurance,
                    "Owned ships: ;;N0".T(EDTx.BankAccountClass_OwnedShipCount), OwnedShipCount,
                    "Spent on Suits: ;cr;N0".T(EDTx.BankAccountClass_SpentOnSuits), SpentOnSuits,
                    "Spent on Weapons: ;cr;N0".T(EDTx.BankAccountClass_SpentOnWeapons), SpentOnWeapons,
                    "Spent on Suit Consumables: ;cr;N0".T(EDTx.BankAccountClass_SpentOnSuitConsumables), SpentOnSuitConsumables,
                    "Suits Owned: ;;N0".T(EDTx.BankAccountClass_SuitsOwned), SuitsOwned,
                    "WeaponsOwned: ;;N0".T(EDTx.BankAccountClass_WeaponsOwned), WeaponsOwned,
                    "Spent on Premium Stock: ;cr;N0".T(EDTx.BankAccountClass_SpentOnPremiumStock), SpentOnPremiumStock,
                    "Premium Stock bought: ;;N0".T(EDTx.BankAccountClass_PremiumStockBought), PremiumStockBought);
            }
        }

        public class CombatClass
        {
            public int BountiesClaimed { get; set; }
            public long BountyHuntingProfit { get; set; }
            public int CombatBonds { get; set; }
            public long CombatBondProfits { get; set; }
            public int Assassinations { get; set; }
            public long AssassinationProfits { get; set; }
            public long HighestSingleReward { get; set; }
            public int SkimmersKilled { get; set; }
            public int OnFootCombatBonds { get; set; }
            public long OnFootCombatBondsProfits { get; set; }
            public int OnFootVehiclesDestroyed { get; set; }
            public int OnFootShipsDestroyed { get; set; }
            public int DropshipsTaken { get; set; }
            public int DropShipsBooked { get; set; }
            public int DropshipsCancelled { get; set; }
            public int ConflictZoneHigh { get; set; }
            public int ConflictZoneMedium { get; set; }
            public int ConflictZoneLow { get; set; }
            public int ConflictZoneTotal { get; set; }
            public int ConflictZoneHighWins { get; set; }
            public int ConflictZoneMediumWins { get; set; }
            public int ConflictZoneLowWins { get; set; }
            public int ConflictZoneTotalWins { get; set; }
            public int SettlementDefended { get; set; }
            public int SettlementConquered { get; set; }
            public int OnFootSkimmersKilled { get; set; }
            public int OnFootScavsKilled { get; set; }

        public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Bounties: ;;N0".T(EDTx.CombatClass_Bounties), BountiesClaimed,
                    "Bounty Profits: ;cr;N0".T(EDTx.CombatClass_BountyProfits), BountyHuntingProfit,
                    "Combat Bonds: ;;N0".T(EDTx.CombatClass_CombatBonds), CombatBonds,
                    "Combat Bond Profits: ;cr;N0".T(EDTx.CombatClass_CombatBondProfits), CombatBondProfits,
                    "Assassinations: ;;N0".T(EDTx.CombatClass_Assassinations), Assassinations,
                    "Assassination Profits: ;cr;N0".T(EDTx.CombatClass_AssassinationProfits), AssassinationProfits,
                    "Highest Reward: ;cr;N0".T(EDTx.CombatClass_HighestReward), HighestSingleReward,
                    "Skimmers Killed: ;;N0".T(EDTx.CombatClass_SkimmersKilled), SkimmersKilled,
                    "Surface Combat Bonds: ;;N0".T(EDTx.CombatClass_OnFootCombatBonds), OnFootCombatBonds,
                    "Surface Combat Bonds Profits: ;cr;N0".T(EDTx.CombatClass_OnFootCombatBondsProfits), OnFootCombatBondsProfits,
                    "Vehicles Destroyed on Foot: ;;N0".T(EDTx.CombatClass_OnFootVehiclesDestroyed), OnFootVehiclesDestroyed,
                    "Ships Destroyed on Foot: ;;N0".T(EDTx.CombatClass_OnFootShipsDestroyed), OnFootShipsDestroyed,
                    "Dropships Taken: ;;N0".T(EDTx.CombatClass_DropshipsTaken), DropshipsTaken,
                    "Dropships Booked: ;;N0".T(EDTx.CombatClass_DropshipsBooked), DropShipsBooked,
                    "Dropships Cancelled: ;;N0".T(EDTx.CombatClass_DropshipsCancelled), DropshipsCancelled,
                    "High Intensity Conflict Zones fought: ;;N0".T(EDTx.CombatClass_ConflictZoneHigh), ConflictZoneHigh,
                    "Medium Intensity Conflict Zones fought: ;;N0".T(EDTx.CombatClass_ConflictZoneMedium), ConflictZoneMedium,
                    "Low Intensity Conflict Zones fought: ;;N0".T(EDTx.CombatClass_ConflictZoneLow), ConflictZoneLow,
                    "Total Conflict Zones fought: ;;N0".T(EDTx.CombatClass_ConflictZoneTotal), ConflictZoneTotal,
                    "High Intensity Conflict Zones won: ;;N0".T(EDTx.CombatClass_ConflictZoneHighWins), ConflictZoneHighWins,
                    "Medium Intensity ConflictZones won: ;;N0".T(EDTx.CombatClass_ConflictZoneMediumWins), ConflictZoneMediumWins,
                    "Low Intensity Conflict Zones won: ;;N0".T(EDTx.CombatClass_ConflictZoneLowWins), ConflictZoneLowWins,
                    "Total Conflict Zones won: ;;N0".T(EDTx.CombatClass_ConflictZoneTotalWins), ConflictZoneTotalWins,
                    "Settlements Defended: ;;N0".T(EDTx.CombatClass_SettlementDefended), SettlementDefended,
                    "Settlements Conquered: ;;N0".T(EDTx.CombatClass_SettlementConquered), SettlementConquered,
                    "Skimmers Killed on Foot: ;;N0".T(EDTx.CombatClass_OnFootSkimmersKilled), OnFootSkimmersKilled,
                    "Scavengers Killed on Foot: ;;N0".T(EDTx.CombatClass_OnFootScavsKilled), OnFootScavsKilled);
            }
        }

        public class CrimeClass
        {
            public double Notoriety { get; set; }
            public int Fines { get; set; }
            public long TotalFines { get; set; }
            public int BountiesReceived { get; set; }
            public long TotalBounties { get; set; }
            public long HighestBounty { get; set; }
            public int MalwareUploaded { get; set; }
            public int SettlementsStateShutdown { get; set; }
            public int ProductionSabotage { get; set; }
            public int ProductionTheft { get; set; }
            public int TotalMurders { get; set; }
            public int CitizensMurdered { get; set; }
            public int OmnipolMurdered { get; set; }
            public int GuardsMurdered { get; set; }
            public int DataStolen { get; set; }
            public int GoodsStolen { get; set; }
            public int SampleStolen { get; set; }
            public int TotalStolen { get; set; }
            public int TurretsDestroyed { get; set; }
            public int TurretsOverloaded { get; set; }
            public int TurretsTotal { get; set; }
            public long ValueStolenStateChange { get; set; }
            public int ProfilesCloned { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Notoriety Index: ;;N0".T(EDTx.CrimeClass_NotorietyIndex), Notoriety,
                    "Fines: ;;N0".T(EDTx.CrimeClass_Fines), Fines,
                    "Total Fines: ;cr;N0".T(EDTx.CrimeClass_TotalFines), TotalFines,
                    "Bounties: ;;N0".T(EDTx.CrimeClass_Bounties), BountiesReceived,
                    "Total Bounties: ;cr;N0".T(EDTx.CrimeClass_TotalBounties), TotalBounties,
                    "Highest Bounty: ;cr;N0".T(EDTx.CrimeClass_HighestBounty), HighestBounty,
                    "Malware Uploaded: ;;N0".T(EDTx.CrimeClass_MalwareUploaded), MalwareUploaded,
                    "Settlements shut down: ;;N0".T(EDTx.CrimeClass_SettlementsStateShutdown), SettlementsStateShutdown,
                    "Production Sabotaged: ;;N0".T(EDTx.CrimeClass_ProductionSabotage), ProductionSabotage,
                    "Production Thefts: ;;N0".T(EDTx.CrimeClass_ProductionTheft), ProductionTheft,
                    "Total Murders: ;;N0".T(EDTx.CrimeClass_TotalMurders), TotalMurders,
                    "Citizens Murdered: ;;N0".T(EDTx.CrimeClass_CitizensMurdered), CitizensMurdered,
                    "Omnipol Murdered: ;;N0".T(EDTx.CrimeClass_OmnipolMurdered), OmnipolMurdered,
                    "Guards Murdered: ;;N0".T(EDTx.CrimeClass_GuardsMurdered), GuardsMurdered,
                    "Data Stolen: ;;N0".T(EDTx.CrimeClass_DataStolen), DataStolen,
                    "Goods Stolen: ;;N0".T(EDTx.CrimeClass_GoodsStolen), GoodsStolen,
                    "Production Samples Stolen: ;;N0".T(EDTx.CrimeClass_ProductionTheft), ProductionTheft,
                    "Total Inventory Items Stolen: ;;N0".T(EDTx.CrimeClass_TotalStolen), TotalStolen,
                    "Turrets Destroyed: ;;N0".T(EDTx.CrimeClass_TurretsDestroyed), TurretsDestroyed,
                    "Turrets Overloaded: ;;N0".T(EDTx.CrimeClass_TurretsOverloaded), TurretsOverloaded,
                    "Total Turrets shut down: ;;N0".T(EDTx.CrimeClass_TurretsTotal), TurretsTotal,
                    "Stolen Items Value: ;cr;N0".T(EDTx.CrimeClass_ValueStolenStateChange), ValueStolenStateChange,
                    "ProfilesCloned: ;;N0".T(EDTx.CrimeClass_ProfilesCloned), ProfilesCloned);
            }
        }

        public class SmugglingClass
        {
            public int BlackMarketsTradedWith { get; set; }
            public long BlackMarketsProfits { get; set; }
            public int ResourcesSmuggled { get; set; }
            public double AverageProfit { get; set; }
            public long HighestSingleTransaction { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Black Markets: ;;N0".T(EDTx.SmugglingClass_BlackMarkets), BlackMarketsTradedWith, 
                    "Black Market Profits: ;cr;N0".T(EDTx.SmugglingClass_BlackMarketProfits), BlackMarketsProfits,
                    "Resources Smuggled: ;;N0".T(EDTx.SmugglingClass_ResourcesSmuggled), ResourcesSmuggled, 
                    "Average Profit: ;cr;N0".T(EDTx.SmugglingClass_AverageProfit), AverageProfit,
                    "Highest Single Transaction: ;cr;N0".T(EDTx.SmugglingClass_HighestSingleTransaction), HighestSingleTransaction);
            }
        }

        public class TradingClass
        {
            public int MarketsTradedWith { get; set; }
            public long MarketProfits { get; set; }
            public int ResourcesTraded { get; set; }
            public double AverageProfit { get; set; }
            public long HighestSingleTransaction { get; set; }
            public int DataSold { get; set; }
            public int GoodsSold { get; set; }
            public int AssetsSold { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                        "Markets Traded: ;;N0".T(EDTx.TradingClass_MarketsTraded), MarketsTradedWith,
                        "Profits: ;cr;N0".T(EDTx.TradingClass_Profits), MarketProfits,
                        "No. of Resources: ;;N0".T(EDTx.TradingClass_No), ResourcesTraded,
                        "Average Profit: ;cr;N0".T(EDTx.TradingClass_AverageProfit), AverageProfit,
                        "Highest Single Transaction: ;cr;N0".T(EDTx.TradingClass_HighestSingleTransaction), HighestSingleTransaction,
                        "Data Sold: ;;N0".T(EDTx.TradingClass_DataSold), DataSold,
                        "Goods Sold: ;;N0".T(EDTx.TradingClass_GoodsSold), GoodsSold,
                        "Assets Sold: ;;N0".T(EDTx.TradingClass_AssetsSold), AssetsSold);
            }
        }

        public class MiningClass
        {
            public long MiningProfits { get; set; }
            public int QuantityMined { get; set; }
            public int MaterialsCollected { get; set; }
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                        "Profits: ;cr;N0".T(EDTx.MiningClass_Profits), MiningProfits, 
                        "Quantity: ;;N0".T(EDTx.MiningClass_Quantity), QuantityMined, 
                        "Materials Types Collected: ;;N0".T(EDTx.MiningClass_MaterialsTypesCollected), MaterialsCollected);


            }
        }

        public class ExplorationClass
        {
            public int SystemsVisited { get; set; }
            public long ExplorationProfits { get; set; }
            public int PlanetsScannedToLevel2 { get; set; }
            public int PlanetsScannedToLevel3 { get; set; }
            public int EfficientScans { get; set; }
            public long HighestPayout { get; set; }
            public long TotalHyperspaceDistance { get; set; }
            public int TotalHyperspaceJumps { get; set; }
            public double GreatestDistanceFromStart { get; set; }
            public int TimePlayed { get; set; }
            public long OnFootDistanceTravelled { get; set; }
            public int ShuttleJourneys { get; set; }
            public long ShuttleDistanceTravelled { get; set; }
            public long SpentOnShuttles { get; set; }
            public int FirstFootfalls { get; set; }
            public int PlanetFootfalls { get; set; }
            public int SettlementsVisited { get; set; }



public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                        "Systems Visited: ;;N0".T(EDTx.ExplorationClass_SystemsVisited), SystemsVisited,
                        "Profits: ;cr;N0".T(EDTx.ExplorationClass_Profits), ExplorationProfits,
                        "Level 2 Scans: ;;N0".T(EDTx.ExplorationClass_Level2Scans), PlanetsScannedToLevel2,
                        "Level 3 Scans: ;;N0".T(EDTx.ExplorationClass_Level3Scans), PlanetsScannedToLevel3,
                        "Efficient Scans: ;;N0".T(EDTx.ExplorationClass_EfficientScans), EfficientScans,
                        "Highest Payout: ;cr;N0".T(EDTx.ExplorationClass_HighestPayout), HighestPayout,
                        "Total Distance: ;ly;N0".T(EDTx.ExplorationClass_TotalDistance), TotalHyperspaceDistance,
                        "No of Jumps: ;;N0".T(EDTx.ExplorationClass_NoofJumps), TotalHyperspaceJumps,
                        "Greatest Distance: ;ly;N0".T(EDTx.ExplorationClass_GreatestDistance), GreatestDistanceFromStart,
                        "Time Played: ".T(EDTx.ExplorationClass_TimePlayed), TimePlayed.SecondsToDHMString(),
                        "Distance Travelled on Foot: ;m;N0".T(EDTx.ExplorationClass_OnFootDistanceTravelled), OnFootDistanceTravelled,
                        "Shuttle Journeys: ;;N0".T(EDTx.ExplorationClass_ShuttleJourneys), ShuttleJourneys,
                        "Shuttle Distance Travelled: ;ly;N0".T(EDTx.ExplorationClass_ShuttleDistanceTravelled), ShuttleDistanceTravelled,
                        "Credits Spent on Shuttles: ;cr;N0".T(EDTx.ExplorationClass_SpentOnShuttles), SpentOnShuttles,
                        "First Footfalls: ;;N0".T(EDTx.ExplorationClass_FirstFootfalls), FirstFootfalls,
                        "Planets walked on: ;;N0".T(EDTx.ExplorationClass_PlanetFootfalls), PlanetFootfalls,
                        "Settlements docked at: ;;N0".T(EDTx.ExplorationClass_SettlementsVisited), SettlementsVisited);
            }
        }

        public class PassengerMissionsClass
        {
            public int Accepted { get; set; }
            public int Disgruntled { get; set; }
            public int Bulk { get; set; }
            public int VIP { get; set; }
            
            public int Delivered { get; set; }
            public int Ejected { get; set; }
             
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Accepted Passenger Missions: ;;N0".T(EDTx.PassengerMissionsClass_Accepted), Accepted,
                    "Passengers Disgruntled: ;;N0".T(EDTx.PassengerMissionsClass_PassengersDisgrunted), Disgruntled,
                    "Total Bulk Passengers Delivered: ;;N0".T(EDTx.PassengerMissionsClass_BulkMissionPassengers), Bulk,
                    "Total VIPs Delivered: ;;N0".T(EDTx.PassengerMissionsClass_VIPMissionPassengers), VIP,
                    "Total Delivered: ;;N0".T(EDTx.PassengerMissionsClass_PassengersDelivered), Delivered,
                    "Total Ejected: ;;N0".T(EDTx.PassengerMissionsClass_PassengersEjected), Ejected);
                    
            }
        }

        public class SearchAndRescueClass
        {
            public long Traded { get; set; }
            public long Profit { get; set; }
            public int Count { get; set; }
            public long SalvageLegalPOI { get; set; }
            public long SalvageLegalSettlements { get; set; }
            public long SalvageIllegalPOI { get; set; }
            public long SalvageIllegalSettlements { get; set; }
            public int MaglocksOpened { get; set; }
            public int PanelsOpened { get; set; }
            public int SettlementsStateFireOut { get; set; }
            public int SettlementsStateReboot { get; set; }
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Total Items Rescued: ;;N0".T(EDTx.SearchAndRescueClass_TotalItemsRescued), Traded,
                    "Profit: ;cr;N0".T(EDTx.SearchAndRescueClass_Profit), Profit,
                    "Total Rescue Transactions: ;;N0".T(EDTx.SearchAndRescueClass_TotalRescueTransactions), Count,
                    "Legal Salvage Value - Surface: ;cr;N0".T(EDTx.SearchAndRescueClass_SalvageLegalPOI), SalvageLegalPOI,
                    "Legal Salvage Value - Settlements: ;cr;N0".T(EDTx.SearchAndRescueClass_SalvageLegalSettlements), SalvageLegalSettlements,
                    "Illegal Salvage Value - Surface: ;cr;N0".T(EDTx.SearchAndRescueClass_SalvageIllegalPOI), SalvageIllegalPOI,
                    "Illegal Salvage Value - Settlements: ;cr;N0".T(EDTx.SearchAndRescueClass_SalvageIllegalSettlements), SalvageIllegalSettlements,
                    "Maglocks cut: ;;N0".T(EDTx.SearchAndRescueClass_MaglocksOpened), MaglocksOpened,
                    "Panels cut: ;;N0".T(EDTx.SearchAndRescueClass_PanelsOpened), PanelsOpened,
                    "Settlement Fires extinguished: ;;N0".T(EDTx.SearchAndRescueClass_SettlementsStateFireOut), SettlementsStateFireOut,
                    "Settlements rebooted: ;;N0".T(EDTx.SearchAndRescueClass_SettlementsStateReboot), SettlementsStateReboot);
            }
        }

        public class CraftingClass
        {
            public int CountOfUsedEngineers { get; set; }
            public int RecipesGenerated { get; set; }
            public int RecipesGeneratedRank1 { get; set; }
            public int RecipesGeneratedRank2 { get; set; }
            public int RecipesGeneratedRank3 { get; set; }
            public int RecipesGeneratedRank4 { get; set; }
            public int RecipesGeneratedRank5 { get; set; }
            public int SuitModsApplied { get; set; }
            public int WeaponModsApplied { get; set; }
            public int SuitsUpgraded { get; set; }
            public int WeaponsUpgraded { get; set; }
            public int SuitsUpgradedFull { get; set; }
            public int WeaponsUpgradedFull { get; set; }
            public int SuitModsAppliedFull { get; set; }
            public int WeaponModsAppliedFull { get; set; }
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Engineers Used: ;;N0".T(EDTx.CraftingClass_EngineersUsed), CountOfUsedEngineers,
                    "Blueprints: ;;N0".T(EDTx.CraftingClass_Blueprints), RecipesGenerated,
                    "At Level 1: ;;N0".T(EDTx.CraftingClass_AtLevel1), RecipesGeneratedRank1,
                    "At Level 2: ;;N0".T(EDTx.CraftingClass_AtLevel2), RecipesGeneratedRank2,
                    "At Level 3: ;;N0".T(EDTx.CraftingClass_AtLevel3), RecipesGeneratedRank3,
                    "At Level 4: ;;N0".T(EDTx.CraftingClass_AtLevel4), RecipesGeneratedRank4,
                    "At Level 5: ;;N0".T(EDTx.CraftingClass_AtLevel5), RecipesGeneratedRank5,
                    "Suit Modifications Applied: ;;N0".T(EDTx.CraftingClass_SuitModsApplied), SuitModsApplied,
                    "Weapon Modifications Applied: ;;N0".T(EDTx.CraftingClass_WeaponModsApplied), WeaponModsApplied,
                    "Suit Upgrades Applied: ;;N0".T(EDTx.CraftingClass_SuitsUpgraded), SuitsUpgraded,
                    "Weapon Upgrades Applied: ;;N0".T(EDTx.CraftingClass_WeaponsUpgraded), WeaponsUpgraded,
                    "Suits fully Upgraded: ;;N0".T(EDTx.CraftingClass_SuitsUpgradedFull), SuitsUpgradedFull,
                    "Weapons fully Upgraded: ;;N0".T(EDTx.CraftingClass_WeaponsUpgradedFull), WeaponsUpgradedFull,
                    "Suits fully Modified: ;;N0".T(EDTx.CraftingClass_SuitModsAppliedFull), SuitModsAppliedFull,
                    "Weapons fully Modified: ;;N0".T(EDTx.CraftingClass_WeaponModsAppliedFull), WeaponModsAppliedFull);
            }
        }

        public class CrewClass
        {
            public long TotalWages { get; set; }
            public int Hired { get; set; }
            public int Fired { get; set; }
            public int Died { get; set; }
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Total Wages: ;cr;N0".T(EDTx.CrewClass_TotalWages), TotalWages,
                    "Hired: ;;N0".T(EDTx.CrewClass_Hired), Hired, 
                    "Fired: ;;N0".T(EDTx.CrewClass_Fired), Fired,
                    "Killed in Action: ;;N0".T(EDTx.CrewClass_KilledinAction), Died);
            }
        }

        public class MulticrewClass
        {
            public int TimeTotal { get; set; }
            public int GunnerTimeTotal { get; set; }
            public int FighterTimeTotal { get; set; }
            public long CreditsTotal { get; set; }
            public long FinesTotal { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Total Time: ".T(EDTx.MulticrewClass_TotalTime), TimeTotal.SecondsToDHMString(),
                    "Gunner Time: ".T(EDTx.MulticrewClass_GunnerTime), GunnerTimeTotal.SecondsToDHMString(),
                    "Fighter Time: ".T(EDTx.MulticrewClass_FighterTime), FighterTimeTotal.SecondsToDHMString(),
                    "Credits: ;cr;N0".T(EDTx.MulticrewClass_Credits), CreditsTotal,
                    "Fines: ;cr;N0".T(EDTx.MulticrewClass_Fines), FinesTotal);
            }
        }

        public class MaterialTraderStatsClass
        {
            public int TradesCompleted { get; set; }
            public int MaterialsTraded { get; set; }
            public int EncodedMaterialsTraded { get; set; }     
            public int RawMaterialsTraded { get; set; }         
            public int Grade1MaterialsTraded { get; set; }      
            public int Grade2MaterialsTraded { get; set; }
            public int Grade3MaterialsTraded { get; set; }
            public int Grade4MaterialsTraded { get; set; }
            public int Grade5MaterialsTraded { get; set; }
            public int AssetsTradedIn { get; set; }
            public int AssetsTradedOut { get; set; }
            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Material Trades Completed: ;;N0".T(EDTx.MaterialTraderStatsClass_CommodityTrades), TradesCompleted,
                    "Material Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_MaterialTraded), MaterialsTraded,
                    "Encoded Materials traded: ;;N0".T(EDTx.MaterialTraderStatsClass_EncodedMaterialsTraded), EncodedMaterialsTraded,
                    "Raw Materials traded: ;;N0".T(EDTx.MaterialTraderStatsClass_RawMaterialsTraded), RawMaterialsTraded,
                    "Grade 1 Materials Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_G1MaterialsTraded), Grade1MaterialsTraded,
                    "Grade 2 Materials Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_G2MaterialsTraded), Grade2MaterialsTraded,
                    "Grade 3 Materials Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_G3MaterialsTraded), Grade3MaterialsTraded,
                    "Grade 4 Materials Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_G4MaterialsTraded), Grade4MaterialsTraded,
                    "Grade 5 Materials Traded: ;;N0".T(EDTx.MaterialTraderStatsClass_G5MaterialsTraded), Grade5MaterialsTraded,
                    "Assets Gained in Trade: ;;N0".T(EDTx.MaterialTraderStatsClass_AssetsTradedIn), AssetsTradedIn,
                    "Assets Spent in Trade: ;;N0".T(EDTx.MaterialTraderStatsClass_AssetsTradedOut), AssetsTradedOut);                     
            }
        }

        public class CQCClass
        {
            public long CreditsEarned { get; set; }
            public int TimePlayed { get; set; }
            public float KD { get; set; }
            public int Kills { get; set; }
            public float WL { get; set; }
            

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Profits from CQC: ;cr;N0".T(EDTx.CQCClass_CreditsEarned), CreditsEarned,
                    "Time Played: ".T(EDTx.CQCClass_TimePlayed), TimePlayed.SecondsToDHMString(),
                    "K/D Ratio: ;;N2".T(EDTx.CQCClass_KDRatio), KD,
                    "Kills: ;;N0".T(EDTx.CQCClass_Kills), Kills,
                    "Win/Loss: ;;N2".T(EDTx.CQCClass_Win), WL);
            }
        }

        public class FLEETCARRIERClass
        {
            public int EXPORTTOTAL { get; set; }
            public int IMPORTTOTAL { get; set; }
            public long TRADEPROFITTOTAL { get; set; }
            public long TRADESPENDTOTAL { get; set; }
            public long STOLENPROFITTOTAL { get; set; }
            public int STOLENSPENDTOTAL { get; set; }
            [JsonIgnore]        // ignore it for auto convert due to frontier changing from string to double
            public double DISTANCETRAVELLED { get; set; }
            public int TOTALJUMPS { get; set; }
            public int SHIPYARDSOLD { get; set; }
            public long SHIPYARDPROFIT { get; set; }
            public int OUTFITTINGSOLD { get; set; }
            public long OUTFITTINGPROFIT { get; set; }
            public int REARMTOTAL { get; set; }
            public int REFUELTOTAL { get; set; }
            public long REFUELPROFIT { get; set; }
            public int REPAIRSTOTAL { get; set; }
            public int VOUCHERSREDEEMED { get; set; }
            public long VOUCHERSPROFIT { get; set; }


            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Total Commodities Exported: ;;N0".T(EDTx.FLEETCARRIERClass_EXPORTTOTAL), EXPORTTOTAL,
                    "Total Commodities Imported: ;;N0".T(EDTx.FLEETCARRIERClass_IMPORTTOTAL), IMPORTTOTAL,
                    "Credits earned from Commodities: ;cr;N0".T(EDTx.FLEETCARRIERClass_TRADEPROFITTOTAL), TRADEPROFITTOTAL,
                    "Credits spent on Commodities: ;cr;N0".T(EDTx.FLEETCARRIERClass_TRADESPENDTOTAL), TRADESPENDTOTAL,
                    "Credits earned from Stolen Goods: ;cr;N0".T(EDTx.FLEETCARRIERClass_STOLENPROFITTOTAL), STOLENPROFITTOTAL,
                    "Credits spent on Stolen Goods: ;cr;N0".T(EDTx.FLEETCARRIERClass_STOLENSPENDTOTAL), STOLENSPENDTOTAL,
                    "Total Travel Distance: ;ly;N0".T(EDTx.FLEETCARRIERClass_DISTANCETRAVELLED), DISTANCETRAVELLED,
                    "Number of Carrier Jumps: ;;N0".T(EDTx.FLEETCARRIERClass_TOTALJUMPS), TOTALJUMPS,
                    "Total Ships Sold: ;;N0".T(EDTx.FLEETCARRIERClass_SHIPYARDSOLD), SHIPYARDSOLD,
                    "Credits earned from Shipyard: ;cr;N0".T(EDTx.FLEETCARRIERClass_SHIPYARDPROFIT), SHIPYARDPROFIT,
                    "Total Modules Sold: ;;N0".T(EDTx.FLEETCARRIERClass_OUTFITTINGSOLD), OUTFITTINGSOLD,
                    "Credits earned from Outfitting: ;cr;N0".T(EDTx.FLEETCARRIERClass_OUTFITTINGPROFIT), OUTFITTINGPROFIT,
                    "Total Ships Restocked: ;;N0".T(EDTx.FLEETCARRIERClass_REARMTOTAL), REARMTOTAL,
                    "Total Ships Refuelled: ;;N0".T(EDTx.FLEETCARRIERClass_REFUELTOTAL), REFUELTOTAL,
                    "Credits earned from Refuelling: ;cr;N0".T(EDTx.FLEETCARRIERClass_REFUELPROFIT), REFUELPROFIT,
                    "Total Ships Repaired: ;;N0".T(EDTx.FLEETCARRIERClass_REPAIRSTOTAL), REPAIRSTOTAL,
                    "Redemption Office Exchanges: ;;N0".T(EDTx.FLEETCARRIERClass_VOUCHERSREDEEMED), VOUCHERSREDEEMED,
                    "Redemption Office Payouts: ;cr;N0".T(EDTx.FLEETCARRIERClass_VOUCHERSPROFIT), VOUCHERSPROFIT);
            }
        }

        public class ExobiologyClass
        {
            public int OrganicGenusEncountered { get; set; }
            public int OrganicSpeciesEncountered { get; set; }
            public int OrganicVariantEncountered { get; set; }
            public long OrganicDataProfits { get; set; }
            public int OrganicData { get; set; }
            public long FirstLoggedProfits { get; set; }
            public int FirstLogged { get; set; }
            public int OrganicSystems { get; set; }
            public int OrganicPlanets { get; set; }
            public int OrganicGenus { get; set; }
            public int OrganicSpecies { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Unique Genus Encountered: ;;N0".T(EDTx.ExobiologyClass_OrganicGenusEncountered), OrganicGenusEncountered,
                    "Unique Species Encountered: ;;N0".T(EDTx.ExobiologyClass_OrganicSpeciesEncountered), OrganicSpeciesEncountered,
                    "Unique Variants Encountered: ;;N0".T(EDTx.ExobiologyClass_OrganicVariantEncountered), OrganicVariantEncountered,
                    "Profit from Organic Data: ;cr;N0".T(EDTx.ExobiologyClass_OrganicDataProfits), OrganicDataProfits,
                    "Organic Data Registered: ;;N0".T(EDTx.ExobiologyClass_OrganicData), OrganicData,
                    "Profit from First Logged: ;cr;N0".T(EDTx.ExobiologyClass_FirstLoggedProfits), FirstLoggedProfits,
                    "First Logged: ;;N0".T(EDTx.ExobiologyClass_FirstLogged), FirstLogged,
                    "Systems with Organic Life: ;;N0".T(EDTx.ExobiologyClass_OrganicSystems), OrganicSystems,
                    "Planets with Organic Life: ;;N0".T(EDTx.ExobiologyClass_OrganicPlanets), OrganicPlanets,
                    "Unique Genus Data Logged: ;;N0".T(EDTx.ExobiologyClass_OrganicGenus), OrganicGenus,
                    "Unique Species Data Logged: ;;N0".T(EDTx.ExobiologyClass_OrganicSpecies), OrganicSpecies);
            }
        }
    }
}
