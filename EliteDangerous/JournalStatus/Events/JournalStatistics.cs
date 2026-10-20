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
            Squadron = evt["Squadron"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("Squadron")?.ToObjectQ<SquadronClass>() ?? new SquadronClass();

            //if (evt["TG_ENCOUNTERS"] != null)                 System.Diagnostics.Debug.WriteLine($"Thargoid read {Thargoids.Format("  ")}");

            // frontier, in their stupidity, wrote travelleddistance sometimes in double sometimes in "double LY". Deal with it directly.

            FLEETCARRIER = evt["FLEETCARRIER"]?.RenameObjectFieldsUnderscores().RemoveObjectFieldsKeyPrefix("FLEETCARRIER")
                        ?.ToObject<FLEETCARRIERClass>(true,
                            customconverter: (mi, v) => {
                                var d = (v is string vs) ? vs.Replace(" LY", "", StringComparison.InvariantCultureIgnoreCase).InvariantParseDouble(0) : v;
                                //System.Diagnostics.Debug.WriteLine($"Fleet custom {v} -> {d}"); 
                                return d;
                            }) ?? new FLEETCARRIERClass();

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
        public SquadronClass Squadron { get; set; }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Wealth".Tx() + ": ; cr;N0", BankAccount.CurrentWealth, "Notoriety Index".Tx() + ": ;;N0", Crime.Notoriety);
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
                        "Thargoids".Tx()+ Environment.NewLine + Thargoids.Format() + Environment.NewLine +
                        "Squadron".Tx() + Environment.NewLine + Squadron.Format();
        }

        public void Ledger(Ledger mcl)
        {
            mcl.Assets = BankAccount?.CurrentWealth ?? 0;

            if (BankAccount!=null &&  mcl.MercCoins != BankAccount.MercCoinsCurrent)
            {
                mcl.AddMercEvent(Id, EventTimeUTC, EventTypeID, "Merc total updated", BankAccount.MercCoinsCurrent);
            }
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
            public int MercCoinsCurrent {  get; set; }
            public int MerCoinsTotalEarned { get; set; }
            public int MercCoinsTotalSpent { get; set; }
            public int MercCoinsSpentOnMercGear { get; set; }
            public int MercCoinsSpentOnEngineering { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Wealth".Tx() + ": ; cr;N0", CurrentWealth,
                    "Spent on Ships".Tx() + ": ; cr;N0", SpentOnShips,
                    "Spent on Outfitting".Tx() + ": ; cr;N0", SpentOnOutfitting,
                    "Spent on Repairs".Tx() + ": ; cr;N0", SpentOnRepairs,
                    "Spent on Fuel".Tx() + ": ; cr;N0", SpentOnFuel,
                    "Spent on Ammo".Tx() + ": ; cr;N0", SpentOnAmmoConsumables,
                    "Insurance Claims".Tx() + ": ;;N0", InsuranceClaims,
                    "Spent on Insurance".Tx() + ": ; cr;N0", SpentOnInsurance,
                    "Owned ships".Tx() + ": ;;N0", OwnedShipCount,
                    "Spent on Suits".Tx() + ": ; cr;N0", SpentOnSuits,
                    "Spent on Weapons".Tx() + ": ; cr;N0", SpentOnWeapons,
                    "Spent on Suit Consumables".Tx() + ": ; cr;N0", SpentOnSuitConsumables,
                    "Suits Owned".Tx() + ": ;;N0", SuitsOwned,
                    "Weapons Owned".Tx() + ": ;;N0", WeaponsOwned,
                    "Spent on Premium Stock".Tx() + ": ; cr;N0", SpentOnPremiumStock,
                    "Premium Stock bought".Tx() + ": ;;N0", PremiumStockBought,
                    "Current Merc Coins".Tx() + ": ;;N0", MercCoinsCurrent,
                    "Total Merc Coins earned".Tx() + ": ;;N0", MerCoinsTotalEarned,
                    "Total Merc Coins spent".Tx() + ": ;;N0", MercCoinsTotalSpent,
                    "Merc Coins spent on Merc Gear".Tx() + ": ;;N0", MercCoinsSpentOnMercGear,
                    "Merc Coins spent on Engineering".Tx() + ": ;;N0", MercCoinsSpentOnEngineering);
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
                    "Bounties".Tx() + ": ;;N0", BountiesClaimed,
                    "Bounty Profits".Tx() + ": ; cr;N0", BountyHuntingProfit,
                    "Combat Bonds".Tx() + ": ;;N0", CombatBonds,
                    "Combat Bond Profits".Tx() + ": ; cr;N0", CombatBondProfits,
                    "Assassinations".Tx() + ": ;;N0", Assassinations,
                    "Assassination Profits".Tx() + ": ; cr;N0", AssassinationProfits,
                    "Highest Reward".Tx() + ": ; cr;N0", HighestSingleReward,
                    "Skimmers Killed".Tx() + ": ;;N0", SkimmersKilled,
                    "Surface Combat Bonds".Tx() + ": ;;N0", OnFootCombatBonds,
                    "Surface Combat Bonds Profits".Tx() + ": ; cr;N0", OnFootCombatBondsProfits,
                    "Vehicles Destroyed on Foot".Tx() + ": ;;N0", OnFootVehiclesDestroyed,
                    "Ships Destroyed on Foot".Tx() + ": ;;N0", OnFootShipsDestroyed,
                    "Dropships Taken".Tx() + ": ;;N0", DropshipsTaken,
                    "Dropships Booked".Tx() + ": ;;N0", DropShipsBooked,
                    "Dropships Cancelled".Tx() + ": ;;N0", DropshipsCancelled,
                    "High Intensity Conflict Zones fought".Tx() + ": ;;N0", ConflictZoneHigh,
                    "Medium Intensity Conflict Zones fought".Tx() + ": ;;N0", ConflictZoneMedium,
                    "Low Intensity Conflict Zones fought".Tx() + ": ;;N0", ConflictZoneLow,
                    "Total Conflict Zones fought".Tx() + ": ;;N0", ConflictZoneTotal,
                    "High Intensity Conflict Zones won".Tx() + ": ;;N0", ConflictZoneHighWins,
                    "Medium Intensity Conflict Zones won".Tx() + ": ;;N0", ConflictZoneMediumWins,
                    "Low Intensity Conflict Zones won".Tx() + ": ;;N0", ConflictZoneLowWins,
                    "Total Conflict Zones won".Tx() + ": ;;N0", ConflictZoneTotalWins,
                    "Settlements Defended".Tx() + ": ;;N0", SettlementDefended,
                    "Settlements Conquered".Tx() + ": ;;N0", SettlementConquered,
                    "Skimmers Killed on Foot".Tx() + ": ;;N0", OnFootSkimmersKilled,
                    "Scavengers Killed on Foot".Tx() + ": ;;N0", OnFootScavsKilled);
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
                    "Notoriety Index".Tx() + ": ;;N0", Notoriety,
                    "Fines".Tx() + ": ;;N0", Fines,
                    "Total Fines".Tx() + ": ; cr;N0", TotalFines,
                    "Bounties".Tx() + ": ;;N0", BountiesReceived,
                    "Total Bounties".Tx() + ": ; cr;N0", TotalBounties,
                    "Highest Bounty".Tx() + ": ; cr;N0", HighestBounty,
                    "Malware Uploaded".Tx() + ": ;;N0", MalwareUploaded,
                    "Settlements shut down".Tx() + ": ;;N0", SettlementsStateShutdown,
                    "Production Sabotaged".Tx() + ": ;;N0", ProductionSabotage,
                    "Production Thefts".Tx() + ": ;;N0", ProductionTheft,
                    "Total Murders".Tx() + ": ;;N0", TotalMurders,
                    "Citizens Murdered".Tx() + ": ;;N0", CitizensMurdered,
                    "Omnipol Murdered".Tx() + ": ;;N0", OmnipolMurdered,
                    "Guards Murdered".Tx() + ": ;;N0", GuardsMurdered,
                    "Data Stolen".Tx() + ": ;;N0", DataStolen,
                    "Goods Stolen".Tx() + ": ;;N0", GoodsStolen,
                    "Total Inventory Items Stolen".Tx() + ": ;;N0", TotalStolen,
                    "Turrets Destroyed".Tx() + ": ;;N0", TurretsDestroyed,
                    "Turrets Overloaded".Tx() + ": ;;N0", TurretsOverloaded,
                    "Total Turrets shut down".Tx() + ": ;;N0", TurretsTotal,
                    "Stolen Items Value".Tx() + ": ; cr;N0", ValueStolenStateChange,
                    "Profiles Cloned".Tx() + ": ;;N0", ProfilesCloned);
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
                    "Black Markets".Tx() + ": ;;N0", BlackMarketsTradedWith, 
                    "Black Market Profits".Tx() + ": ; cr;N0", BlackMarketsProfits,
                    "Resources Smuggled".Tx() + ": ;;N0", ResourcesSmuggled, 
                    "Average Profit".Tx() + ": ; cr;N0", AverageProfit,
                    "Highest Single Transaction".Tx() + ": ; cr;N0", HighestSingleTransaction);
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
                        "Markets Traded".Tx() + ": ;;N0", MarketsTradedWith,
                        "Profits".Tx() + ": ; cr;N0", MarketProfits,
                        "No. of Resources".Tx() + ": ;;N0", ResourcesTraded,
                        "Average Profit".Tx() + ": ; cr;N0", AverageProfit,
                        "Highest Single Transaction".Tx() + ": ; cr;N0", HighestSingleTransaction,
                        "Data Sold".Tx() + ": ;;N0", DataSold,
                        "Goods Sold".Tx() + ": ;;N0", GoodsSold,
                        "Assets Sold".Tx() + ": ;;N0", AssetsSold);
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
                        "Profits".Tx() + ": ; cr;N0", MiningProfits, 
                        "Quantity".Tx() + ": ;;N0", QuantityMined, 
                        "Materials Collected".Tx() + ": ;;N0", MaterialsCollected);


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
                        "Systems Visited".Tx() + ": ;;N0", SystemsVisited,
                        "Profits".Tx() + ": ; cr;N0", ExplorationProfits,
                        "Level 2 Scans".Tx() + ": ;;N0", PlanetsScannedToLevel2,
                        "Level 3 Scans".Tx() + ": ;;N0", PlanetsScannedToLevel3,
                        "Efficient Scans".Tx() + ": ;;N0", EfficientScans,
                        "Highest Payout".Tx() + ": ; cr;N0", HighestPayout,
                        "Total Distance".Tx() + ": ; ly;N0", TotalHyperspaceDistance,
                        "No of Jumps".Tx() + ": ;;N0", TotalHyperspaceJumps,
                        "Greatest Distance".Tx() + ": ; ly;N0", GreatestDistanceFromStart,
                        "Time Played".Tx()+"".Tx() + ": ", TimePlayed.SecondsToDHMString(),
                        "Distance Travelled on Foot".Tx() + ": ; m;N0", OnFootDistanceTravelled,
                        "Shuttle Journeys".Tx() + ": ;;N0", ShuttleJourneys,
                        "Shuttle Distance Travelled".Tx() + ": ; ly;N0", ShuttleDistanceTravelled,
                        "Credits Spent on Shuttles".Tx() + ": ; cr;N0", SpentOnShuttles,
                        "First Footfalls".Tx() + ": ;;N0", FirstFootfalls,
                        "Planets walked on".Tx() + ": ;;N0", PlanetFootfalls,
                        "Settlements docked at".Tx() + ": ;;N0", SettlementsVisited);
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
                    "Accepted Passenger Missions".Tx() + ": ;;N0", Accepted,
                    "Passengers Disgruntled".Tx() + ": ;;N0", Disgruntled,
                    "Total Bulk Passengers Delivered".Tx() + ": ;;N0", Bulk,
                    "Total VIPs Delivered".Tx() + ": ;;N0", VIP,
                    "Total Delivered".Tx() + ": ;;N0", Delivered,
                    "Total Ejected".Tx() + ": ;;N0", Ejected);
                    
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
                    "Total Items Rescued".Tx() + ": ;;N0", Traded,
                    "Profit".Tx() + ": ; cr;N0", Profit,
                    "Total Rescue Transactions".Tx() + ": ;;N0", Count,
                    "Legal Salvage Value - Surface".Tx() + ": ; cr;N0", SalvageLegalPOI,
                    "Legal Salvage Value - Settlements".Tx() + ": ; cr;N0", SalvageLegalSettlements,
                    "Illegal Salvage Value - Surface".Tx() + ": ; cr;N0", SalvageIllegalPOI,
                    "Illegal Salvage Value - Settlements".Tx() + ": ; cr;N0", SalvageIllegalSettlements,
                    "Maglocks cut".Tx() + ": ;;N0", MaglocksOpened,
                    "Panels cut".Tx() + ": ;;N0", PanelsOpened,
                    "Settlement Fires extinguished".Tx() + ": ;;N0", SettlementsStateFireOut,
                    "Settlements rebooted".Tx() + ": ;;N0", SettlementsStateReboot);
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
                    "Engineers Used".Tx() + ": ;;N0", CountOfUsedEngineers,
                    "Blueprints".Tx() + ": ;;N0", RecipesGenerated,
                    "At Level 1".Tx() + ": ;;N0", RecipesGeneratedRank1,
                    "At Level 2".Tx() + ": ;;N0", RecipesGeneratedRank2,
                    "At Level 3".Tx() + ": ;;N0", RecipesGeneratedRank3,
                    "At Level 4".Tx() + ": ;;N0", RecipesGeneratedRank4,
                    "At Level 5".Tx() + ": ;;N0", RecipesGeneratedRank5,
                    "Suit Modifications Applied".Tx() + ": ;;N0", SuitModsApplied,
                    "Weapon Modifications Applied".Tx() + ": ;;N0", WeaponModsApplied,
                    "Suit Upgrades Applied".Tx() + ": ;;N0", SuitsUpgraded,
                    "Weapon Upgrades Applied".Tx() + ": ;;N0", WeaponsUpgraded,
                    "Suits fully Upgraded".Tx() + ": ;;N0", SuitsUpgradedFull,
                    "Weapons fully Upgraded".Tx() + ": ;;N0", WeaponsUpgradedFull,
                    "Suits fully Modified".Tx() + ": ;;N0", SuitModsAppliedFull,
                    "Weapons fully Modified".Tx() + ": ;;N0", WeaponModsAppliedFull);
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
                    "Total Wages".Tx() + ": ; cr;N0", TotalWages,
                    "Hired".Tx() + ": ;;N0", Hired, 
                    "Fired".Tx() + ": ;;N0", Fired,
                    "Killed in Action".Tx() + ": ;;N0", Died);
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
                    "Total Time".Tx()+": ", TimeTotal.SecondsToDHMString(),
                    "Gunner Time".Tx()+": ", GunnerTimeTotal.SecondsToDHMString(),
                    "Fighter Time".Tx()+": ", FighterTimeTotal.SecondsToDHMString(),
                    "Credits".Tx() + ": ; cr;N0", CreditsTotal,
                    "Fines".Tx() + ": ; cr;N0", FinesTotal);
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
                    "Material Trades Completed".Tx() + ": ;;N0", TradesCompleted,
                    "Material Traded".Tx() + ": ;;N0", MaterialsTraded,
                    "Encoded Materials traded".Tx() + ": ;;N0", EncodedMaterialsTraded,
                    "Raw Materials traded".Tx() + ": ;;N0", RawMaterialsTraded,
                    "Grade 1 Materials Traded".Tx() + ": ;;N0", Grade1MaterialsTraded,
                    "Grade 2 Materials Traded".Tx() + ": ;;N0", Grade2MaterialsTraded,
                    "Grade 3 Materials Traded".Tx() + ": ;;N0", Grade3MaterialsTraded,
                    "Grade 4 Materials Traded".Tx() + ": ;;N0", Grade4MaterialsTraded,
                    "Grade 5 Materials Traded".Tx() + ": ;;N0", Grade5MaterialsTraded,
                    "Assets Gained in Trade".Tx() + ": ;;N0", AssetsTradedIn,
                    "Assets Spent in Trade".Tx() + ": ;;N0", AssetsTradedOut);                     
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
                    "Profits from CQC".Tx() + ": ; cr;N0", CreditsEarned,
                    "Time Played".Tx()+": ", TimePlayed.SecondsToDHMString(),
                    "K/D Ratio".Tx() + ": ;;N2", KD,
                    "Kills".Tx() + ": ;;N0", Kills,
                    "Win/Loss Ratio".Tx() + ": ;;N2", WL);
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
            [JsonCustomFormat]        
            public double DISTANCETRAVELLED { get; set; }       // needs special processing as Frontier sometimes wrote "2 LY"
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
                    "Total Commodities Exported".Tx() + ": ;;N0", EXPORTTOTAL,
                    "Total Commodities Imported".Tx() + ": ;;N0", IMPORTTOTAL,
                    "Credits earned from Commodities".Tx() + ": ; cr;N0", TRADEPROFITTOTAL,
                    "Credits spent on Commodities".Tx() + ": ; cr;N0", TRADESPENDTOTAL,
                    "Credits earned from Stolen Goods".Tx() + ": ; cr;N0", STOLENPROFITTOTAL,
                    "Credits spent on Stolen Goods".Tx() + ": ; cr;N0", STOLENSPENDTOTAL,
                    "Total Travel Distance".Tx() + ": ; ly;N0", DISTANCETRAVELLED,
                    "Number of Carrier Jumps".Tx() + ": ;;N0", TOTALJUMPS,
                    "Total Ships Sold".Tx() + ": ;;N0", SHIPYARDSOLD,
                    "Credits earned from Shipyard".Tx() + ": ; cr;N0", SHIPYARDPROFIT,
                    "Total Modules Sold".Tx() + ": ;;N0", OUTFITTINGSOLD,
                    "Credits earned from Outfitting".Tx() + ": ; cr;N0", OUTFITTINGPROFIT,
                    "Total Ships Restocked".Tx() + ": ;;N0", REARMTOTAL,
                    "Total Ships Refuelled".Tx() + ": ;;N0", REFUELTOTAL,
                    "Credits earned from Refuelling".Tx() + ": ; cr;N0", REFUELPROFIT,
                    "Total Ships Repaired".Tx() + ": ;;N0", REPAIRSTOTAL,
                    "Redemption Office Exchanges".Tx() + ": ;;N0", VOUCHERSREDEEMED,
                    "Redemption Office Payouts".Tx() + ": ; cr;N0", VOUCHERSPROFIT);
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
                    "Unique Genus Encountered".Tx() + ": ;;N0", OrganicGenusEncountered,
                    "Unique Species Encountered".Tx() + ": ;;N0", OrganicSpeciesEncountered,
                    "Unique Variants Encountered".Tx() + ": ;;N0", OrganicVariantEncountered,
                    "Profit from Organic Data".Tx() + ": ; cr;N0", OrganicDataProfits,
                    "Organic Data Registered".Tx() + ": ;;N0", OrganicData,
                    "Profit from First Logged".Tx() + ": ; cr;N0", FirstLoggedProfits,
                    "First Logged".Tx() + ": ;;N0", FirstLogged,
                    "Systems with Organic Life".Tx() + ": ;;N0", OrganicSystems,
                    "Planets with Organic Life".Tx() + ": ;;N0", OrganicPlanets,
                    "Unique Genus Data Logged".Tx() + ": ;;N0", OrganicGenus,
                    "Unique Species Data Logged".Tx() + ": ;;N0", OrganicSpecies);
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
                    "Thargoid wakes scanned".Tx() + ": ;;N0", WAKES,
                    "Thargoids killed".Tx() + ": ;;N0", KILLED + TGSCOUTCOUNT,
                    "Thargoid structures".Tx() + ": ;;N0", IMPRINT,
                    "Total encounters".Tx() + ": ;;N0", TOTAL,
                    "Last seen in".Tx() + ": ", TOTALLASTSYSTEM,
                    "Last seen on".Tx() + ": ", TOTALLASTTIMESTAMP,
                    "Last ship involved".Tx() + ": ", TOTALLASTSHIP);
            }
        }

        public class SquadronClass
        {
            public long BankCreditsDeposited { get; set; }
            public long BankCreditsWithdrawn { get; set; }
            public int BankCommoditiesDepositedNum { get; set; }
            public long BankCommoditiesDepositedValue { get; set; }
            public int BankCommoditiesWithdrawnNum { get; set; }
            public long BankCommoditiesWithdrawnValue { get; set; }
            public int BankPersonalAssetsDepositedNum { get; set; }
            public long BankPersonalAssetsDepositedValue { get; set; }
            public int BankPersonalAssetsWithdrawnNum { get; set; }
            public long BankPersonalAssetsWithdrawnValue { get; set; }
            public int BankShipsDepositedNum { get; set; }
            public long BankShipsDepositedValue { get; set; }
            public long Leaderboardaegishighestcontribution { get; set; }
            public long Leaderboardbgshighestcontribution { get; set; }
            public long Leaderboardbountyhighestcontribution { get; set; }
            public long Leaderboardcolonisationcontributionhighestcontribution { get; set; }
            public long Leaderboardcombathighestcontribution { get; set; }
            public long Leaderboardcqchighestcontribution { get; set; }
            public long Leaderboardexplorationhighestcontribution { get; set; }
            public long Leaderboardmininghighestcontribution { get; set; }
            public long Leaderboardpowerplayhighestcontribution { get; set; }
            public long Leaderboardtradehighestcontribution { get; set; }
            public long Leaderboardtradeillicithighestcontribution { get; set; }
            public long Leaderboardoperationscorehighestcontribution { get; set; }
            public int Leaderboardpodiums { get; set; }

            public string Format(string frontline = "    ")
            {
                return frontline + BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine + frontline,
                    "Credits deposited in bank".Tx() + ": ; cr;N0", BankCreditsDeposited,
                    "Credits withdrawn from bank".Tx() + ": ; cr;N0", BankCreditsWithdrawn,
                    "Times commodities were deposited".Tx() + ": ;;N0", BankCommoditiesDepositedNum,
                    "Value of deposited commodities".Tx() + ": ; cr;N0", BankCommoditiesDepositedValue,
                    "Times commodities were withdrawn".Tx() + ": ;;N0", BankCommoditiesWithdrawnNum,
                    "Value of withdrawn commodities".Tx() + ": ; cr;N0", BankCommoditiesWithdrawnValue,
                    "Times personal assets were deposited".Tx() + ": ;;N0", BankPersonalAssetsDepositedNum,
                    "Value of deposited personal assets".Tx() + ": ; cr;N0", BankPersonalAssetsDepositedValue,
                    "Times personal assets were withdrawn".Tx() + ": ;;N0", BankPersonalAssetsWithdrawnNum,
                    "Value of withdrawn personal assets".Tx() + ": ; cr;N0", BankPersonalAssetsWithdrawnValue,
                    "Times ships were deposited".Tx() + ": ;;N0", BankShipsDepositedNum,
                    "Value of deposited ships".Tx() + ": ; cr;N0", BankShipsDepositedValue,
                    "Highest contribution for Aegis leaderboard".Tx() + ": ;;N0", Leaderboardaegishighestcontribution,
                    "Highest contribution for BGS leaderboard".Tx() + ": ;;N0", Leaderboardbgshighestcontribution,
                    "Highest contribution for Bounty leaderboard".Tx() + ": ;;N0", Leaderboardbountyhighestcontribution,
                    "Highest contribution for Colonisation Contribution leaderboard".Tx() + ": ;;N0", Leaderboardcolonisationcontributionhighestcontribution,
                    "Highest contribution for Combat leaderboard".Tx() + ": ;;N0", Leaderboardcombathighestcontribution,
                    "Highest contribution for CQC leaderboard".Tx() + ": ;;N0", Leaderboardcqchighestcontribution,
                    "Highest contribution for Exploration leaderboard".Tx() + ": ;;N0", Leaderboardexplorationhighestcontribution,
                    "Highest contribution for Mining leaderboard".Tx() + ": ;;N0", Leaderboardmininghighestcontribution,
                    "Highest contribution for Powerplay leaderboard".Tx() + ": ;;N0", Leaderboardpowerplayhighestcontribution,
                    "Highest contribution for Trade leaderboard".Tx() + ": ;;N0", Leaderboardtradehighestcontribution,
                    "Highest contribution for Illicit Trade leaderboard".Tx() + ": ;;N0", Leaderboardtradeillicithighestcontribution,
                    "Highest contribution for Operations leaderboard".Tx() + ": ;;N0", Leaderboardoperationscorehighestcontribution,
                    "Times squadron got a podium place".Tx() + ": ;;N0", Leaderboardpodiums);                
            }
        }
    }
}
