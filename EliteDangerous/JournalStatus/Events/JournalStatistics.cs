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
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Statistics)]
    public class JournalStatistics : JournalEntry, ILedgerJournalEntry
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
            Thargoids = evt["TG_ENCOUNTERS"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("TGENCOUNTER")?.ToObjectQ<ThargoidsClass>() ?? new ThargoidsClass();

            //if (evt["TG_ENCOUNTERS"] != null)                 System.Diagnostics.Debug.WriteLine($"Thargoid read {Thargoids.Format("  ")}");

            FLEETCARRIER = evt["FLEETCARRIER"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("FLEETCARRIER")?.ToObject<FLEETCARRIERClass>(true) ?? new FLEETCARRIERClass();
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
        public ThargoidsClass Thargoids { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Wealth: ; cr;N0".Tx(), BankAccount.CurrentWealth, "Notoriety Index: ;;N0".Tx(), Crime.Notoriety);
        }


        public override string GetDetailed()
        {
            return "Bank Account".Tx()+ Environment.NewLine + BankAccount?.Format() + Environment.NewLine +
                        "Combat".Tx()+ Environment.NewLine + Combat?.Format() + Environment.NewLine +
                        "Crime".Tx()+ Environment.NewLine + Crime?.Format() + Environment.NewLine +
                        "Smuggling".Tx()+ Environment.NewLine + Smuggling?.Format() + Environment.NewLine +
                        "Trading".Tx()+ Environment.NewLine + Trading?.Format() + Environment.NewLine +
                        "Mining".Tx()+ Environment.NewLine + Mining?.Format() + Environment.NewLine +
                        "Exploration".Tx()+ Environment.NewLine + Exploration?.Format() + Environment.NewLine +
                        "Passengers".Tx()+ Environment.NewLine + PassengerMissions?.Format() + Environment.NewLine +
                        "Search and Rescue".Tx()+ Environment.NewLine + SearchAndRescue?.Format() + Environment.NewLine +
                        "Engineers".Tx()+ Environment.NewLine + Crafting?.Format() + Environment.NewLine +
                        "Crew".Tx()+ Environment.NewLine + Crew?.Format() + Environment.NewLine +
                        "Multicrew".Tx()+ Environment.NewLine + Multicrew?.Format() + Environment.NewLine +
                        "Materials and Commodity Trading".Tx()+ Environment.NewLine + MaterialTraderStats?.Format() + Environment.NewLine +
                        "CQC".Tx()+ Environment.NewLine + CQC?.Format() + Environment.NewLine +
                        "Fleetcarrier".Tx()+ Environment.NewLine + FLEETCARRIER?.Format() + Environment.NewLine +
                        "Exobiology".Tx()+ Environment.NewLine + Exobiology.Format() + Environment.NewLine +
                        "Thargoids".Tx()+ Environment.NewLine + Thargoids.Format();
        }

        public void Ledger(Ledger mcl)
        {
            mcl.Assets = BankAccount?.CurrentWealth ?? 0;
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
                    "Wealth: ; cr;N0".Tx(),  CurrentWealth, 
                    "Spent on Ships: ; cr;N0".Tx(), SpentOnShips,
                    "Spent on Outfitting: ; cr;N0".Tx(), SpentOnOutfitting, 
                    "Spent on Repairs: ; cr;N0".Tx(), SpentOnRepairs, 
                    "Spent on Fuel: ; cr;N0".Tx(), SpentOnFuel,
                    "Spent on Ammo: ; cr;N0".Tx(), SpentOnAmmoConsumables, 
                    "Insurance Claims: ;;N0".Tx(), InsuranceClaims,
                    "Spent on Insurance: ; cr;N0".Tx(), SpentOnInsurance,
                    "Owned ships: ;;N0".Tx(), OwnedShipCount,
                    "Spent on Suits: ; cr;N0".Tx(), SpentOnSuits,
                    "Spent on Weapons: ; cr;N0".Tx(), SpentOnWeapons,
                    "Spent on Suit Consumables: ; cr;N0".Tx(), SpentOnSuitConsumables,
                    "Suits Owned: ;;N0".Tx(), SuitsOwned,
                    "Weapons Owned: ;;N0".Tx(), WeaponsOwned,
                    "Spent on Premium Stock: ; cr;N0".Tx(), SpentOnPremiumStock,
                    "Premium Stock bought: ;;N0".Tx(), PremiumStockBought);
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
                    "Bounties: ;;N0".Tx(), BountiesClaimed,
                    "Bounty Profits: ; cr;N0".Tx(), BountyHuntingProfit,
                    "Combat Bonds: ;;N0".Tx(), CombatBonds,
                    "Combat Bond Profits: ; cr;N0".Tx(), CombatBondProfits,
                    "Assassinations: ;;N0".Tx(), Assassinations,
                    "Assassination Profits: ; cr;N0".Tx(), AssassinationProfits,
                    "Highest Reward: ; cr;N0".Tx(), HighestSingleReward,
                    "Skimmers Killed: ;;N0".Tx(), SkimmersKilled,
                    "Surface Combat Bonds: ;;N0".Tx(), OnFootCombatBonds,
                    "Surface Combat Bonds Profits: ; cr;N0".Tx(), OnFootCombatBondsProfits,
                    "Vehicles Destroyed on Foot: ;;N0".Tx(), OnFootVehiclesDestroyed,
                    "Ships Destroyed on Foot: ;;N0".Tx(), OnFootShipsDestroyed,
                    "Dropships Taken: ;;N0".Tx(), DropshipsTaken,
                    "Dropships Booked: ;;N0".Tx(), DropShipsBooked,
                    "Dropships Cancelled: ;;N0".Tx(), DropshipsCancelled,
                    "High Intensity Conflict Zones fought: ;;N0".Tx(), ConflictZoneHigh,
                    "Medium Intensity Conflict Zones fought: ;;N0".Tx(), ConflictZoneMedium,
                    "Low Intensity Conflict Zones fought: ;;N0".Tx(), ConflictZoneLow,
                    "Total Conflict Zones fought: ;;N0".Tx(), ConflictZoneTotal,
                    "High Intensity Conflict Zones won: ;;N0".Tx(), ConflictZoneHighWins,
                    "Medium Intensity Conflict Zones won: ;;N0".Tx(), ConflictZoneMediumWins,
                    "Low Intensity Conflict Zones won: ;;N0".Tx(), ConflictZoneLowWins,
                    "Total Conflict Zones won: ;;N0".Tx(), ConflictZoneTotalWins,
                    "Settlements Defended: ;;N0".Tx(), SettlementDefended,
                    "Settlements Conquered: ;;N0".Tx(), SettlementConquered,
                    "Skimmers Killed on Foot: ;;N0".Tx(), OnFootSkimmersKilled,
                    "Scavengers Killed on Foot: ;;N0".Tx(), OnFootScavsKilled);
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
                    "Notoriety Index: ;;N0".Tx(), Notoriety,
                    "Fines: ;;N0".Tx(), Fines,
                    "Total Fines: ; cr;N0".Tx(), TotalFines,
                    "Bounties: ;;N0".Tx(), BountiesReceived,
                    "Total Bounties: ; cr;N0".Tx(), TotalBounties,
                    "Highest Bounty: ; cr;N0".Tx(), HighestBounty,
                    "Malware Uploaded: ;;N0".Tx(), MalwareUploaded,
                    "Settlements shut down: ;;N0".Tx(), SettlementsStateShutdown,
                    "Production Sabotaged: ;;N0".Tx(), ProductionSabotage,
                    "Production Thefts: ;;N0".Tx(), ProductionTheft,
                    "Total Murders: ;;N0".Tx(), TotalMurders,
                    "Citizens Murdered: ;;N0".Tx(), CitizensMurdered,
                    "Omnipol Murdered: ;;N0".Tx(), OmnipolMurdered,
                    "Guards Murdered: ;;N0".Tx(), GuardsMurdered,
                    "Data Stolen: ;;N0".Tx(), DataStolen,
                    "Goods Stolen: ;;N0".Tx(), GoodsStolen,
                    "Total Inventory Items Stolen: ;;N0".Tx(), TotalStolen,
                    "Turrets Destroyed: ;;N0".Tx(), TurretsDestroyed,
                    "Turrets Overloaded: ;;N0".Tx(), TurretsOverloaded,
                    "Total Turrets shut down: ;;N0".Tx(), TurretsTotal,
                    "Stolen Items Value: ; cr;N0".Tx(), ValueStolenStateChange,
                    "Profiles Cloned: ;;N0".Tx(), ProfilesCloned);
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
                    "Black Markets: ;;N0".Tx(), BlackMarketsTradedWith, 
                    "Black Market Profits: ; cr;N0".Tx(), BlackMarketsProfits,
                    "Resources Smuggled: ;;N0".Tx(), ResourcesSmuggled, 
                    "Average Profit: ; cr;N0".Tx(), AverageProfit,
                    "Highest Single Transaction: ; cr;N0".Tx(), HighestSingleTransaction);
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
                        "Markets Traded: ;;N0".Tx(), MarketsTradedWith,
                        "Profits: ; cr;N0".Tx(), MarketProfits,
                        "No. of Resources: ;;N0".Tx(), ResourcesTraded,
                        "Average Profit: ; cr;N0".Tx(), AverageProfit,
                        "Highest Single Transaction: ; cr;N0".Tx(), HighestSingleTransaction,
                        "Data Sold: ;;N0".Tx(), DataSold,
                        "Goods Sold: ;;N0".Tx(), GoodsSold,
                        "Assets Sold: ;;N0".Tx(), AssetsSold);
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
                        "Profits: ; cr;N0".Tx(), MiningProfits, 
                        "Quantity: ;;N0".Tx(), QuantityMined, 
                        "Materials Collected: ;;N0".Tx(), MaterialsCollected);


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
                        "Systems Visited: ;;N0".Tx(), SystemsVisited,
                        "Profits: ; cr;N0".Tx(), ExplorationProfits,
                        "Level 2 Scans: ;;N0".Tx(), PlanetsScannedToLevel2,
                        "Level 3 Scans: ;;N0".Tx(), PlanetsScannedToLevel3,
                        "Efficient Scans: ;;N0".Tx(), EfficientScans,
                        "Highest Payout: ; cr;N0".Tx(), HighestPayout,
                        "Total Distance: ; ly;N0".Tx(), TotalHyperspaceDistance,
                        "No of Jumps: ;;N0".Tx(), TotalHyperspaceJumps,
                        "Greatest Distance: ; ly;N0".Tx(), GreatestDistanceFromStart,
                        "Time Played: ".Tx(), TimePlayed.SecondsToDHMString(),
                        "Distance Travelled on Foot: ; m;N0".Tx(), OnFootDistanceTravelled,
                        "Shuttle Journeys: ;;N0".Tx(), ShuttleJourneys,
                        "Shuttle Distance Travelled: ; ly;N0".Tx(), ShuttleDistanceTravelled,
                        "Credits Spent on Shuttles: ; cr;N0".Tx(), SpentOnShuttles,
                        "First Footfalls: ;;N0".Tx(), FirstFootfalls,
                        "Planets walked on: ;;N0".Tx(), PlanetFootfalls,
                        "Settlements docked at: ;;N0".Tx(), SettlementsVisited);
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
                    "Accepted Passenger Missions: ;;N0".Tx(), Accepted,
                    "Passengers Disgruntled: ;;N0".Tx(), Disgruntled,
                    "Total Bulk Passengers Delivered: ;;N0".Tx(), Bulk,
                    "Total VIPs Delivered: ;;N0".Tx(), VIP,
                    "Total Delivered: ;;N0".Tx(), Delivered,
                    "Total Ejected: ;;N0".Tx(), Ejected);
                    
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
                    "Total Items Rescued: ;;N0".Tx(), Traded,
                    "Profit: ; cr;N0".Tx(), Profit,
                    "Total Rescue Transactions: ;;N0".Tx(), Count,
                    "Legal Salvage Value - Surface: ; cr;N0".Tx(), SalvageLegalPOI,
                    "Legal Salvage Value - Settlements: ; cr;N0".Tx(), SalvageLegalSettlements,
                    "Illegal Salvage Value - Surface: ; cr;N0".Tx(), SalvageIllegalPOI,
                    "Illegal Salvage Value - Settlements: ; cr;N0".Tx(), SalvageIllegalSettlements,
                    "Maglocks cut: ;;N0".Tx(), MaglocksOpened,
                    "Panels cut: ;;N0".Tx(), PanelsOpened,
                    "Settlement Fires extinguished: ;;N0".Tx(), SettlementsStateFireOut,
                    "Settlements rebooted: ;;N0".Tx(), SettlementsStateReboot);
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
                    "Engineers Used: ;;N0".Tx(), CountOfUsedEngineers,
                    "Blueprints: ;;N0".Tx(), RecipesGenerated,
                    "At Level 1: ;;N0".Tx(), RecipesGeneratedRank1,
                    "At Level 2: ;;N0".Tx(), RecipesGeneratedRank2,
                    "At Level 3: ;;N0".Tx(), RecipesGeneratedRank3,
                    "At Level 4: ;;N0".Tx(), RecipesGeneratedRank4,
                    "At Level 5: ;;N0".Tx(), RecipesGeneratedRank5,
                    "Suit Modifications Applied: ;;N0".Tx(), SuitModsApplied,
                    "Weapon Modifications Applied: ;;N0".Tx(), WeaponModsApplied,
                    "Suit Upgrades Applied: ;;N0".Tx(), SuitsUpgraded,
                    "Weapon Upgrades Applied: ;;N0".Tx(), WeaponsUpgraded,
                    "Suits fully Upgraded: ;;N0".Tx(), SuitsUpgradedFull,
                    "Weapons fully Upgraded: ;;N0".Tx(), WeaponsUpgradedFull,
                    "Suits fully Modified: ;;N0".Tx(), SuitModsAppliedFull,
                    "Weapons fully Modified: ;;N0".Tx(), WeaponModsAppliedFull);
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
                    "Total Wages: ; cr;N0".Tx(), TotalWages,
                    "Hired: ;;N0".Tx(), Hired, 
                    "Fired: ;;N0".Tx(), Fired,
                    "Killed in Action: ;;N0".Tx(), Died);
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
                    "Total Time: ".Tx(), TimeTotal.SecondsToDHMString(),
                    "Gunner Time: ".Tx(), GunnerTimeTotal.SecondsToDHMString(),
                    "Fighter Time: ".Tx(), FighterTimeTotal.SecondsToDHMString(),
                    "Credits: ; cr;N0".Tx(), CreditsTotal,
                    "Fines: ; cr;N0".Tx(), FinesTotal);
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
                    "Material Trades Completed: ;;N0".Tx(), TradesCompleted,
                    "Material Traded: ;;N0".Tx(), MaterialsTraded,
                    "Encoded Materials traded: ;;N0".Tx(), EncodedMaterialsTraded,
                    "Raw Materials traded: ;;N0".Tx(), RawMaterialsTraded,
                    "Grade 1 Materials Traded: ;;N0".Tx(), Grade1MaterialsTraded,
                    "Grade 2 Materials Traded: ;;N0".Tx(), Grade2MaterialsTraded,
                    "Grade 3 Materials Traded: ;;N0".Tx(), Grade3MaterialsTraded,
                    "Grade 4 Materials Traded: ;;N0".Tx(), Grade4MaterialsTraded,
                    "Grade 5 Materials Traded: ;;N0".Tx(), Grade5MaterialsTraded,
                    "Assets Gained in Trade: ;;N0".Tx(), AssetsTradedIn,
                    "Assets Spent in Trade: ;;N0".Tx(), AssetsTradedOut);                     
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
                    "Profits from CQC: ; cr;N0".Tx(), CreditsEarned,
                    "Time Played: ".Tx(), TimePlayed.SecondsToDHMString(),
                    "K/D Ratio: ;;N2".Tx(), KD,
                    "Kills: ;;N0".Tx(), Kills,
                    "Win/Loss Ratio: ;;N2".Tx(), WL);
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
                    "Total Commodities Exported: ;;N0".Tx(), EXPORTTOTAL,
                    "Total Commodities Imported: ;;N0".Tx(), IMPORTTOTAL,
                    "Credits earned from Commodities: ; cr;N0".Tx(), TRADEPROFITTOTAL,
                    "Credits spent on Commodities: ; cr;N0".Tx(), TRADESPENDTOTAL,
                    "Credits earned from Stolen Goods: ; cr;N0".Tx(), STOLENPROFITTOTAL,
                    "Credits spent on Stolen Goods: ; cr;N0".Tx(), STOLENSPENDTOTAL,
                    "Total Travel Distance: ; ly;N0".Tx(), DISTANCETRAVELLED,
                    "Number of Carrier Jumps: ;;N0".Tx(), TOTALJUMPS,
                    "Total Ships Sold: ;;N0".Tx(), SHIPYARDSOLD,
                    "Credits earned from Shipyard: ; cr;N0".Tx(), SHIPYARDPROFIT,
                    "Total Modules Sold: ;;N0".Tx(), OUTFITTINGSOLD,
                    "Credits earned from Outfitting: ; cr;N0".Tx(), OUTFITTINGPROFIT,
                    "Total Ships Restocked: ;;N0".Tx(), REARMTOTAL,
                    "Total Ships Refuelled: ;;N0".Tx(), REFUELTOTAL,
                    "Credits earned from Refuelling: ; cr;N0".Tx(), REFUELPROFIT,
                    "Total Ships Repaired: ;;N0".Tx(), REPAIRSTOTAL,
                    "Redemption Office Exchanges: ;;N0".Tx(), VOUCHERSREDEEMED,
                    "Redemption Office Payouts: ; cr;N0".Tx(), VOUCHERSPROFIT);
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
                    "Unique Genus Encountered: ;;N0".Tx(), OrganicGenusEncountered,
                    "Unique Species Encountered: ;;N0".Tx(), OrganicSpeciesEncountered,
                    "Unique Variants Encountered: ;;N0".Tx(), OrganicVariantEncountered,
                    "Profit from Organic Data: ; cr;N0".Tx(), OrganicDataProfits,
                    "Organic Data Registered: ;;N0".Tx(), OrganicData,
                    "Profit from First Logged: ; cr;N0".Tx(), FirstLoggedProfits,
                    "First Logged: ;;N0".Tx(), FirstLogged,
                    "Systems with Organic Life: ;;N0".Tx(), OrganicSystems,
                    "Planets with Organic Life: ;;N0".Tx(), OrganicPlanets,
                    "Unique Genus Data Logged: ;;N0".Tx(), OrganicGenus,
                    "Unique Species Data Logged: ;;N0".Tx(), OrganicSpecies);
            }
        }

        public class ThargoidsClass
        {
            public int WAKES { get; set; }
            public int KILLED { get; set; }   //from patch 17 on, seems to replace scout count
            public int IMPRINT { get; set; }
            public int TOTAL { get; set; }
            public string TOTALLASTSYSTEM { get; set; } = "";
            public string TOTALLASTTIMESTAMP { get; set; } = "";
            public string TOTALLASTSHIP { get; set; } = "";
            public int TGSCOUTCOUNT { get; set; } //up to patch 17, seems to be replaced by encounter killed. May be null

            public string Format(string frontline = "    ", bool showblanks = false)
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPadShowBlanks(Environment.NewLine + frontline, showblanks,
                    "Thargoid wakes scanned: ;;N0".Tx(), WAKES,
                    "Thargoids killed: ;;N0".Tx(), KILLED + TGSCOUTCOUNT,
                    "Thargoid structures: ;;N0".Tx(), IMPRINT,
                    "Total encounters: ;;N0".Tx(), TOTAL,
                    "Last seen in: ".Tx(), TOTALLASTSYSTEM,
                    "Last seen on: ".Tx(), TOTALLASTTIMESTAMP,
                    "Last ship involved: ".Tx(), TOTALLASTSHIP);
            }
        }
    }
}
