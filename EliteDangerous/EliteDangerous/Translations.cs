using System;
using System.Collections.Generic;
using System.Globalization;

namespace EliteDangerousCore
{
    internal enum EDCTx
    {
        Warning, // Warning
        OK, // OK
        Cancel, // Cancel
        Unknown, // Unknown
        RemoveAll, // Remove All

        JournalTypeEnum_Docked,

        ScreenShotImageConverter_CNV, 
        ScreenshotDirectoryWatcher_Scan,
        ScreenshotDirectoryWatcher_NOF,
        ScreenshotDirectoryWatcher_Excp,

        Bodies_HMS, //"Luminous Hot Main Sequence {0} star" @
        Bodies_BMS, //"Luminous Blue Main Sequence {0} star" @
        Bodies_BWMS, //"Bluish-White Main Sequence {0} star" @
        Bodies_WMS, //"White Main Sequence {0} star" @
        Bodies_YMS, //"Yellow Main Sequence {0} star" @
        Bodies_OMS, //"Orange Main Sequence {0} star" @
        Bodies_RMS, //"Red Main Sequence {0} star" @
        Bodies_DRNS, //"Dark Red Non Main Sequence {0} star" @
        Bodies_MD, //"Methane Dwarf star" @
        Bodies_BD, //"Brown Dwarf star" @
        Bodies_WR, //"Wolf-Rayet {0} star" @
        Bodies_C, //"Carbon {0} star" @
        Bodies_IZ, //"Intermediate low Zirconium Monoxide Type star" @
        Bodies_CGZ, //"Cool Giant Zirconium Monoxide rich Type star" @
        Bodies_WD, //"White Dwarf {0} star" @
        Bodies_NS, //"Neutron Star" @
        Bodies_BH, //"Black Hole" @
        Bodies_EX, //"Exotic" @
        Bodies_SMBH, //"Super Massive Black Hole" @
        Bodies_ABSG, //"A Blue White Super Giant" @
        Bodies_BBSG, //"B Blue White Super Giant" @
        Bodies_FWSG, //"F White Super Giant" @
        Bodies_GWSG, //"G White Super Giant" @
        Bodies_MSR, //"M Red Super Giant" @
        Bodies_MOG, //"M Red Giant" @
        Bodies_KOG, //"K Orange Giant" @
        Bodies_RP, //"Rogue Planet" @
        Bodies_UNK, // "Class {0} star" @
        Bodies_Herbig, //"Herbig Ae/Be"
        Bodies_TTauri, //"T Tauri"
        Bodies_Nebula, //"Nebula" @
        Bodies_StellarRemnantNebula, //"Stellar Remnant Nebula" @
        Bodies_SUnknown, //"Unknown Star class" @

        MissionState_ToGo, // To Go:
        MissionState_Progress, // Progress:;%;N1

        EngineeringData_Engineer, // Engineer:
        EngineeringData_Blueprint, // Blueprint:
        EngineeringData_Level, // Level:
        EngineeringData_Quality, // Quality:
        EngineeringData_ExperimentalEffect, // Experimental Effect:
        EngineeringData_Original, // Original:;;N2
        EngineeringData_Worse, // < (Worse); (Better)

        JournalEntry_Taxi, // Taxi
        JournalEntry_TME,
        JournalEntry_Class,
        JournalEntry_Mods,
        JournalEntry_OnFootAt, 
        JournalEntry_LegBounty, // {0} total {1:N0}
        JournalEntry_Target, // Target:
        JournalEntry_Victimfaction, // Victim faction:
        JournalEntry_Faction, // Faction:
        JournalEntry_from, // < from
        JournalEntry_dueto, // < , due to
        JournalEntry_0, // {0} on {1}
        JournalEntry_onfaction, // < on faction
        JournalEntry_Against, // Against
        JournalEntry_Cost, // Cost:; cr;N0
        JournalEntry_Bounty, // Bounty:; cr;N0
        JournalEntry_BountyOnly, // Bounty
        JournalEntry_Offender, // Offender
        JournalEntry_Reward, // Reward:; cr;N0
        JournalEntry_to, // < to
        JournalEntry_Brokertook, // , Broker took {0:N0}%
        JournalEntry_Type, // Type:
        JournalEntry_Amount, // Amount:; cr;N0
        JournalEntry_System, // System:
        JournalEntry_NoCargo, // No Cargo
        JournalEntry_Cargo, // Cargo, {0} items
        JournalEntry_items, // ; items
        JournalEntry_MissionCargo, // <; (Mission Cargo)
        JournalEntry_Count, // Count:
        JournalEntry_Abandoned, // ;Abandoned
        JournalEntry_PowerPlay, // PowerPlay:
        JournalEntry_Collected, // Collected:
        JournalEntry_of, // < of
        JournalEntry_ofa, // < of
        JournalEntry_Total, // Total:
        JournalEntry_Progress, // Progress:;%;N1
        JournalEntry_Delivered, // Delivered:
        JournalEntry_Update, // Update, Collected:
        JournalEntry_ToGo, // To Go:
        JournalEntry_ProgressLeft, // Progress Left:;%;N1
        JournalEntry_Stolen, // ;Stolen
        JournalEntry_StartingPackage, // Starting Package:
        JournalEntry_CGS, // < at ; Star System
        JournalEntry_torole, // < to role ;
        JournalEntry_fired, // ; fired
        JournalEntry_Hired, // Hired:;
        JournalEntry_offaction, // < of faction
        JournalEntry_Rank, // Rank:
        JournalEntry_Crew, // Crew:
        JournalEntry_Telepresence, // ;Telespresence
        JournalEntry_Role, // Role:
        JournalEntry_CrewMember, // Crew Member:
        JournalEntry_DuetoCrime, // ;Due to Crime
        JournalEntry_Captain, // Captain:
        JournalEntry_fromfaction, // < from faction
        JournalEntry_Died, // {0} in ship type {1} rank {2}
        JournalEntry_Killedby, // Killed by
        JournalEntry_Boom, // Boom!
        JournalEntry_Option, // Option:
        JournalEntry_Bankrupt, // ;Bankrupt
        JournalEntry_Dscan, // New bodies discovered:
        JournalEntry_Bodies, // Bodies:
        JournalEntry_insystem, // < in system
        JournalEntry_Wanted, // ;(Wanted)
        JournalEntry_ActiveFine, // ;Active Fine
        JournalEntry_instate, // < in state
        JournalEntry_Allegiance, // Allegiance:
        JournalEntry_Economy, // Economy:
        JournalEntry_Government, // Government:
        JournalEntry_Stationservices, // Station services:
        JournalEntry_Economies, // Economies:
        JournalEntry_onpad, // < on pad
        JournalEntry_Cockpit, // Cockpit:
        JournalEntry_Corrosion, // Corrosion:
        JournalEntry_TotalCost, // Total Cost: ; cr;N0
        JournalEntry_TotalSale, // Total Sale: ; cr;N0
        JournalEntry_each, // each:; cr;N0
        JournalEntry_Drones, // Drones
        JournalEntry_Price, // Price:; cr;N0
        JournalEntry_Name, // Name:
        JournalEntry_Blueprint, // Blueprint:
        JournalEntry_Level, // Level:
        JournalEntry_Override, // Override:
        JournalEntry_Commodity, // Commodity:
        JournalEntry_Material, // Material:
        JournalEntry_Quantity, // Quantity:
        JournalEntry_TotalQuantity, // TotalQuantity:
        JournalEntry_InSlot, // In Slot:
        JournalEntry_By, // By:
        JournalEntry_Bonus, // Bonus:; cr;N0
        JournalEntry_Scanned, // Scanned:
        JournalEntry_Discovered, // Discovered:
        JournalEntry_Loadout, // Loadout:
        JournalEntry_NPCControlled, // NPC Controlled;
        JournalEntry_Version, // Version:
        JournalEntry_Build, // Build:
        JournalEntry_Part, // Part:
        JournalEntry_Ship, // Ship:
        JournalEntry_Ident, // Ident:
        JournalEntry_Credits, // Credits:;;N0
        JournalEntry_Mode, // Mode:
        JournalEntry_Group, // Group:
        JournalEntry_NotLanded, // Not Landed;Landed
        JournalEntry_Fuel, // Fuel:; tons;0.0
        JournalEntry_FuelLevel, // Fuel Level:;;0.0
        JournalEntry_Capacity, // Capacity:;;0.0
        JournalEntry_Cashtotaldiffers, // Cash total differs, adjustment
        JournalEntry_NumberofStatuses, // Number of Statuses:
        JournalEntry_Online, // Online:
        JournalEntry_Offline, // Offline:
        JournalEntry_Unfriended,
        JournalEntry_Declined,
        JournalEntry_RequestedFriend,
        JournalEntry_AddedFriend,
        JournalEntry_Submitted, // ;Submitted
        JournalEntry_NPC, // < (NPC);(Player)
        JournalEntry_Power, // Power:
        JournalEntry_Failedtointerdict, // Failed to interdict;Interdicted
        JournalEntry_Boost, // Boost:;;0.0
        JournalEntry_buyprice, // < buy price ; cr;N0
        JournalEntry_sellprice, // < sell price ; cr;N0
        JournalEntry_Profit, // Profit:; cr;N0
        JournalEntry_Legal, // Legal;Illegal
        JournalEntry_NotStolen, // Not Stolen;Stolen
        JournalEntry_Market, // Market;BlackMarket
        JournalEntry_MatC, // < ; items
        JournalEntry_Sold, // Sold:
        JournalEntry_Received, // Received:
        JournalEntry_Active, // Active:
        JournalEntry_Failed, // Failed:
        JournalEntry_Completed, // Completed:
        JournalEntry_Station, // Station:
        JournalEntry_Settlement, // Settlement:
        JournalEntry_TargetFaction, // Target Faction:
        JournalEntry_Donation, // Donation:
        JournalEntry_Permits, // Permits:
        JournalEntry_Rewards, // Rewards:
        JournalEntry_Fine, // Fine:
        JournalEntry_Missionname, // Mission name:
        JournalEntry_Hot, // ;(Hot)
        JournalEntry_HullHealth, // Hull Health;%;N1
        JournalEntry_Hull, // Hull:; cr;N0
        JournalEntry_Modules, // Modules:; cr;N0
        JournalEntry_Rebuy, // Rebuy:; cr;N0
        JournalEntry_into, // < into
        JournalEntry_Stored, // Stored:
        JournalEntry_Item, // Item:
        JournalEntry_on, //  on
        JournalEntry_Replacedby, // Replaced by:
        JournalEntry_Modifications, // Modifications:
        JournalEntry_Slot, // Slot:
        JournalEntry_Swappedwith, // , Swapped with
        JournalEntry_TransferCost, // Transfer Cost:; cr;N0
        JournalEntry_Time, // Time:
        JournalEntry_Value, // Value:; cr;N0
        JournalEntry_Totalmodules, // Total modules:
        JournalEntry_Intoship, // Into ship:
        JournalEntry_TransferTime, // Transfer Time:
        JournalEntry_MusicTrack, // Music Track:
        JournalEntry_Wagesfor, // Wages for
        JournalEntry_itemsavailable, //  items available
        JournalEntry_NoPassengers, // No Passengers
        JournalEntry_Merits, // Merits:
        JournalEntry_Votes, // Votes:
        JournalEntry_Pledged, // Pledged:
        JournalEntry_FromPower, // From Power:
        JournalEntry_ToPower, // To Power:
        JournalEntry_Systems, // , Systems:
        JournalEntry_Shield, // Shield ;;N1
        JournalEntry_LostTarget, // Lost Target
        JournalEntry_ShieldsDown, // Shields Down;Shields Up
        JournalEntry_Category, // Category:
        JournalEntry_Latitude, // Latitude:
        JournalEntry_Longitude, // Longitude:
        JournalEntry_Num, // Num:
        JournalEntry_Ships, // Ships
        JournalEntry_Distance, // Distance:; ly;0.0
        JournalEntry_Threat, // Threat:
        JournalEntry_New, // New:
        JournalEntry_Old, // New:
        JournalEntry_Wealth, // Wealth:; cr;N
        JournalEntry_NotorietyIndex, // Notoriety Index:;;N0
        JournalEntry_RemainingJumps,  // Remaining Jumps
        JournalEntry_Nearest, // Nearest:
        JournalEntry_Near, // Near:
        JournalEntry_BartenderMaterials, 
        JournalEntry_ScanOrganicsValue, // Value: ; cr;N0
        JournalEntry_ScanOrganicsPotentialValue, // Value: ; cr;N0


        JournalLocOrJump_Type, // "Type " @
        JournalLocOrJump_insystem, // "< in system " @
        JournalLocOrJump_Security, // "Security, //" @
        JournalLocOrJump_Happiness, // "Happiness, //" @
        JournalLocOrJump_Reputation, // "Reputation, //;%;N1" @
        JournalLocOrJump_SquadronSystem, // ";Squadron System" @
        JournalLocOrJump_HappiestSystem, // ";Happiest System" @
        JournalLocOrJump_HomeSystem, // ";Home System" @
        JournalLocOrJump_ActiveState, // "Active State:" @

        JournalLocOrJump_Faction, // "Faction, //" @
        JournalLocOrJump_Wanted, // "<;(Wanted) " @
        JournalLocOrJump_State, // "State, //" @
        JournalLocOrJump_Allegiance, // "Allegiance, //" @
        JournalLocOrJump_Economy, // "Economy, //" @
        JournalLocOrJump_Population, // "Population, //" @
        JournalLocOrJump_Government, // "Government, //" @
        JournalLocOrJump_Inf, // "Inf, //;%" @
        JournalLocOrJump_PendingState, // "Pending State, //" @
        JournalLocOrJump_RecoveringState, // "Recovering State:" @


        JournalStatistics_BankAccount, // Bank Account
        JournalStatistics_Combat, // Combat
        JournalStatistics_Crime, // Crime
        JournalStatistics_Smuggling, // Smuggling
        JournalStatistics_Trading, // Trading
        JournalStatistics_Mining, // Mining
        JournalStatistics_Exploration, // Exploration
        JournalStatistics_Passengers, // Passengers
        JournalStatistics_SearchandRescue, // Search and Rescue
        JournalStatistics_Engineers, // Engineers
        JournalStatistics_Crew, // Crew
        JournalStatistics_Multicrew, // Multicrew
        JournalStatistics_MaterialsandCommodityTrading, // Materials and Commodity Trading
        JournalStatistics_CQC, // CQC
        JournalStatistics_FLEETCARRIER, // Fleetcarrier
        JournalStatistics_Exobiology, // Exobiology
        BankAccountClass_Wealth, // Wealth: ; cr;N0
        BankAccountClass_SpentonShips, // Spent on Ships: ; cr;N0
        BankAccountClass_SpentonOutfitting, // Spent on Outfitting: ; cr;N0
        BankAccountClass_SpentonRepairs, // Spent on Repairs: ; cr;N0
        BankAccountClass_SpentonFuel, // Spent on Fuel: ; cr;N0
        BankAccountClass_SpendonAmmo, // Spend on Ammo: ; cr;N0
        BankAccountClass_InsuranceClaims, // Insurance Claims: ;;N0
        BankAccountClass_SpentonInsurance, // Spent on Insurance: ; cr;N0
        BankAccountClass_OwnedShipCount, // Owned ships: ;;No
        BankAccountClass_SpentOnSuits, // Spent on Suits: ; cr;N0
        BankAccountClass_SpentOnWeapons, // Spent on Weapons: ; cr;N0
        BankAccountClass_SpentOnSuitConsumables, // Spent on Suit Consumables: ; cr;N0
        BankAccountClass_SuitsOwned, // Suits Owned: ;;N0
        BankAccountClass_WeaponsOwned, // Weapons Owned: ;;N0
        BankAccountClass_SpentOnPremiumStock, // Spent on Premium Stock: ; cr;N0
        BankAccountClass_PremiumStockBought, // Premium Stock bought: ;;N0
        CombatClass_Bounties, // Bounties: ;;N0
        CombatClass_BountyProfits, // Bounty Profits: ; cr;N0
        CombatClass_CombatBonds, // Combat Bonds: ;;N0
        CombatClass_CombatBondProfits, // Combat Bond Profits: ; cr;N0
        CombatClass_Assassinations, // Assassinations: ;;N0
        CombatClass_AssassinationProfits, // Assassination Profits: ; cr;N0
        CombatClass_HighestReward, // Highest Reward: ; cr;N0
        CombatClass_SkimmersKilled, // Skimmers Killed: ;;N0
        CombatClass_OnFootCombatBonds, // "Surface Combat Bonds: ;;N0"
        CombatClass_OnFootCombatBondsProfits, // "Surface Combat Bonds Profits: ; cr;N0"
        CombatClass_OnFootVehiclesDestroyed, // "Vehicles Destroyed on Foot: ;;N0"
        CombatClass_OnFootShipsDestroyed, // "Ships Destroyed on Foot: ;;N0"
        CombatClass_DropshipsTaken, // Dropships Taken: ;;N0
        CombatClass_DropshipsBooked, //Dropships Booked: ;;N0
        CombatClass_DropshipsCancelled, //Dropships Cancelled: ;;N0
        CombatClass_ConflictZoneHigh, // High Intensity Conflict Zones fought: ;;N0
        CombatClass_ConflictZoneMedium, // Medium Intensity Conflict Zones fought: ;;N0
        CombatClass_ConflictZoneLow, // Low Intensity Conflict Zones fought: ;;N0
        CombatClass_ConflictZoneTotal, // Total Conflict Zones fought: ;;N0
        CombatClass_ConflictZoneHighWins, // High Intensity Conflict Zones won: ;;N0
        CombatClass_ConflictZoneMediumWins, // Medium Intensity Conflict Zones won: ;;N0
        CombatClass_ConflictZoneLowWins, // Low Intensity Conflict Zones won: ;;N0
        CombatClass_ConflictZoneTotalWins, // Total Conflict Zones won: ;;N0
        CombatClass_SettlementDefended, // Settlements Defended: ;;N0
        CombatClass_SettlementConquered, // Settlements Conquered: ;;N0
        CombatClass_OnFootSkimmersKilled, // Skimmers Killed on Foot: ;;N0
        CombatClass_OnFootScavsKilled, // Scavengers Killed on Foot: ;;N0
        CrimeClass_NotorietyIndex, // Notoriety Index:;;N0
        CrimeClass_Fines, // Fines: ;;N0
        CrimeClass_TotalFines, // Total Fines: ; cr;N0
        CrimeClass_Bounties, // Bounties: ;;N0
        CrimeClass_TotalBounties, // Total Bounties: ; cr;N0
        CrimeClass_HighestBounty, // Highest Bounty: ; cr;N0
        CrimeClass_MalwareUploaded, // Malware Uploaded: ;;N0
        CrimeClass_SettlementsStateShutdown, // Settlements shut down: ;;N0
        CrimeClass_ProductionSabotage, // Production Sabotaged: ;;N0
        CrimeClass_ProductionTheft, // Production Thefts: ;;N0
        CrimeClass_TotalMurders, // Total Murders: ;;N0
        CrimeClass_CitizensMurdered, // Citizens Murdered: ;;N0
        CrimeClass_OmnipolMurdered, // Omnipol Murdered: ;;N0  
        CrimeClass_GuardsMurdered, //Guards Murdered: ;;N0
        CrimeClass_DataStolen, // Data Stolen: ;;N0
        CrimeClass_GoodsStolen, // Goods Stolen: ;;N0
        CrimeClass_TotalStolen, // Total Inventory Items Stolen: ;;N0
        CrimeClass_TurretsDestroyed, // Turrets Destroyed: ;;N0
        CrimeClass_TurretsOverloaded, // Turrets Overloaded: ;;N0
        CrimeClass_TurretsTotal, // Total Turrets shut down: ;;N0
        CrimeClass_ValueStolenStateChange, // Stolen Items Value: ; cr;N0
        CrimeClass_ProfilesCloned, // Profiles Cloned: ;;N0
        SmugglingClass_BlackMarkets, // Black Markets: ;;N0
        SmugglingClass_BlackMarketProfits, // Black Market Profits: ; cr;N0
        SmugglingClass_ResourcesSmuggled, // Resources Smuggled: ;;N0
        SmugglingClass_AverageProfit, // Average Profit: ; cr;N0
        SmugglingClass_HighestSingleTransaction, // Highest Single Transaction: ; cr;N0
        TradingClass_MarketsTraded, // Markets Traded: ;;N0
        TradingClass_Profits, // Profits: ; cr;N0
        TradingClass_No, // No. of Resources: ;;N0
        TradingClass_AverageProfit, // Average Profit: ; cr;N0
        TradingClass_HighestSingleTransaction, // Highest Single Transaction: ; cr;N0
        TradingClass_DataSold, // Data Sold: ;;N0
        TradingClass_GoodsSold, // Goods Sold: ;;N0
        TradingClass_AssetsSold, // Assets Sold: ;;N0
        MiningClass_Profits, // Profits: ; cr;N0
        MiningClass_Quantity, // Quantity: ;;N0
        MiningClass_MaterialsTypesCollected, // Materials Collected: ;;N0
        ExplorationClass_SystemsVisited, // Systems Visited: ;;N0
        ExplorationClass_Profits, // Profits: ; cr;N0
        ExplorationClass_Level2Scans, // Level 2 Scans: ;;N0
        ExplorationClass_Level3Scans, // Level 3 Scans: ;;N0
        ExplorationClass_HighestPayout, // Highest Payout: ; cr;N0
        ExplorationClass_TotalDistance, // Total Distance: ;;N0
        ExplorationClass_NoofJumps, // No of Jumps: ;;N0
        ExplorationClass_GreatestDistance, // Greatest Distance: ;;N0
        ExplorationClass_TimePlayed, // Time Played:
        ExplorationClass_OnFootDistanceTravelled, // "Distance Travelled on Foot: ;m;N0"
        ExplorationClass_EfficientScans, // Efficient Scans: ;;N0
        ExplorationClass_ShuttleJourneys, // Shuttle Journeys: ;;N0
        ExplorationClass_ShuttleDistanceTravelled, // Shuttle Distance Travelled: ;ly,N0
        ExplorationClass_SpentOnShuttles, // Credits Spent on Shuttles: ; cr;N0
        ExplorationClass_FirstFootfalls, // First Footfalls: ;;N0
        ExplorationClass_PlanetFootfalls, // Planets walked on: ;;N0
        ExplorationClass_SettlementsVisited, // Settlements docked at: ;;N0
        PassengerMissionsClass_Accepted, // Accepted:;;N0
        PassengerMissionsClass_BulkMissionPassengers, // Total Bulk Passengers Delivered: ;;N0
        PassengerMissionsClass_VIPMissionPassengers, // Total VIPs Delivered: ;;N0
        PassengerMissionsClass_PassengersDelivered, // Total Delivered: ;;N0
        PassengerMissionsClass_PassengersEjected, // Total Ejected: ;;N0
        PassengerMissionsClass_PassengersDisgrunted, // Total Disgrunted: ;;N0
        SearchAndRescueClass_TotalItemsRescued, // Total Items Rescued: ;;N0
        SearchAndRescueClass_Profit, // Profit: ; cr;N0
        SearchAndRescueClass_TotalRescueTransactions, // Total Rescue Transactions: ;;N0
        SearchAndRescueClass_SalvageLegalPOI, // Legal Salvage Value - Surface: ;cr,N0
        SearchAndRescueClass_SalvageLegalSettlements, // Legal Salvage Value - Settlements: ; cr;N0
        SearchAndRescueClass_SalvageIllegalPOI, // Illegal Salvage Value - Surface: ; cr;N0
        SearchAndRescueClass_SalvageIllegalSettlements, // Illegal Salvage Value - Settlements: ; cr;N0
        SearchAndRescueClass_MaglocksOpened, // Maglocks cut: ;;N0
        SearchAndRescueClass_PanelsOpened, // Panels cut: ;;N0
        SearchAndRescueClass_SettlementsStateFireOut, // Settlement Fires extinguished: ;;N0
        SearchAndRescueClass_SettlementsStateReboot, // Settlements rebooted: ;;N0
        CraftingClass_EngineersUsed, // Engineers Used: ;;N0
        CraftingClass_Blueprints, // Blueprints: ;;N0
        CraftingClass_AtLevel1, // At Level 1: ;;N0
        CraftingClass_AtLevel2, // At Level 2: ;;N0
        CraftingClass_AtLevel3, // At Level 3: ;;N0
        CraftingClass_AtLevel4, // At Level 4: ;;N0
        CraftingClass_AtLevel5, // At Level 5: ;;N0
        CraftingClass_SuitModsApplied, // Suit Modifications Applied: ;;N0
        CraftingClass_WeaponModsApplied, // Weapon Modifications Applied: ;;N0
        CraftingClass_SuitsUpgraded, // Suit Upgrades Applied: ;;N0
        CraftingClass_WeaponsUpgraded, // Weapon Upgrades Applied: ;;N0
        CraftingClass_SuitsUpgradedFull, // Suits fully Upgraded: ;;N0
        CraftingClass_WeaponsUpgradedFull, // Weapons fully Upgraded: ;;N0
        CraftingClass_SuitModsAppliedFull, // Suits fully Modified: ;;N0
        CraftingClass_WeaponModsAppliedFull, // Weapons fully Modified: ;;N0        
        CrewClass_TotalWages, // Total Wages: ; cr;N0
        CrewClass_Hired, // Hired: ;;N0
        CrewClass_Fired, // Fired: ;;N0
        CrewClass_KilledinAction, // Killed in Action: ;;N0
        MulticrewClass_TotalTime, // Total Time:
        MulticrewClass_GunnerTime, // Gunner Time:
        MulticrewClass_FighterTime, // Fighter Time:
        MulticrewClass_Credits, // Credits: ; cr;N0
        MulticrewClass_Fines, // Fines: ; cr;N0
        MaterialTraderStatsClass_CommodityTrades, // Material Trades Completed: ;;N0
        MaterialTraderStatsClass_MaterialTraded, // Material Traded: ;;N0
        MaterialTraderStatsClass_EncodedMaterialsTraded, // Encoded Materials Traded: ;;N0
        MaterialTraderStatsClass_RawMaterialsTraded, // Raw Materials Traded: ;;N0
        MaterialTraderStatsClass_G1MaterialsTraded, // Grade 1 Materials Traded: ;;N0
        MaterialTraderStatsClass_G2MaterialsTraded, // Grade 2 Materials Traded: ;;N0
        MaterialTraderStatsClass_G3MaterialsTraded, // Grade 3 Materials Traded: ;;N0
        MaterialTraderStatsClass_G4MaterialsTraded, // Grade 4 Materials Traded: ;;N0
        MaterialTraderStatsClass_G5MaterialsTraded, // Grade 5 Materials Traded: ;;N0
        MaterialTraderStatsClass_AssetsTradedIn, // "Assets Gained in Trade: ;;N0"
        MaterialTraderStatsClass_AssetsTradedOut, // "Assets Spent in Trade: ;;N0"
        CQCClass_CreditsEarned, // Profits from CQC: ; cr;N0
        CQCClass_TimePlayed, // Time Played: ;;N0
        CQCClass_KDRatio, // K/D Ratio: ;;N2
        CQCClass_Kills, // Kills: ;;N0
        CQCClass_Win, // Win/Loss Ratio: ;;N2
        FLEETCARRIERClass_EXPORTTOTAL, // Total Commodities Exported: ;;N0
        FLEETCARRIERClass_IMPORTTOTAL, // Total Commodities Imported: ;;N0
        FLEETCARRIERClass_TRADEPROFITTOTAL, // Credits earned from Commodities: ; cr;N0
        FLEETCARRIERClass_TRADESPENDTOTAL, // Credits spent on Commodities: ; cr;N0
        FLEETCARRIERClass_STOLENPROFITTOTAL, // Credits earned from Stolen Goods: ; cr;N0
        FLEETCARRIERClass_STOLENSPENDTOTAL, // Credits spent on Stolen Goods: ; cr;N0
        FLEETCARRIERClass_DISTANCETRAVELLED, // Total Travel Distance: ; ly;N0
        FLEETCARRIERClass_TOTALJUMPS, // Number of Carrier Jumps: ;;N0
        FLEETCARRIERClass_SHIPYARDSOLD, // Total Ships Sold: ;;N0
        FLEETCARRIERClass_SHIPYARDPROFIT, // Credits earned from Shipyard: ; cr;N0
        FLEETCARRIERClass_OUTFITTINGSOLD, // Total Modules Sold: ;;N0
        FLEETCARRIERClass_OUTFITTINGPROFIT, // Credits earned from Outfitting: ; cr;N0
        FLEETCARRIERClass_REARMTOTAL, // Total Ships Restocked: ;;N0
        FLEETCARRIERClass_REFUELTOTAL, // Total Ships Refuelled: ;;N0
        FLEETCARRIERClass_REFUELPROFIT, // Credits earned from Refuelling: ; cr;N0
        FLEETCARRIERClass_REPAIRSTOTAL, // Total Ships Repaired: ;;N0
        FLEETCARRIERClass_VOUCHERSREDEEMED, // Redemption Office Exchanges: ;;N0
        FLEETCARRIERClass_VOUCHERSPROFIT, // Redemption Office Payouts: ; cr;N0
        ExobiologyClass_OrganicGenusEncountered, // Unique Genus Encountered: ;;N0
        ExobiologyClass_OrganicSpeciesEncountered, // Unique Species Encountered: ;;N0
        ExobiologyClass_OrganicVariantEncountered, // Unique Variants Encountered: ;;N0
        ExobiologyClass_OrganicDataProfits, // Profit from Organic Data: ; cr;N0
        ExobiologyClass_OrganicData, // Organic Data Registered: ;;N0
        ExobiologyClass_FirstLoggedProfits, // Profit from First Logged: ; cr;N0
        ExobiologyClass_FirstLogged, // First Logged: ;;N0
        ExobiologyClass_OrganicSystems, // Systems with Organic Life: ;;N0
        ExobiologyClass_OrganicPlanets, // Planets with Organic Life: ;;N0
        ExobiologyClass_OrganicGenus, // Unique Genus Data Logged: ;;N0
        ExobiologyClass_OrganicSpecies, // Unique Species Data Logged: ;;N0

    JournalNavRoute_Jumps, // "{0} jumps: "

        JournalScan_Autoscanof, // Autoscan of {0}
        JournalScan_Detailedscanof, // Detailed scan of {0}
        JournalScan_Basicscanof, // Basic scan of {0}
        JournalScan_Navscanof, // Nav scan of {0}
        JournalScan_ScanAuto, // Scan Auto
        JournalScan_ScanBasic, // Scan Basic
        JournalScan_ScanNav, // Scan Nav
        JournalScan_Scanof, // Scan of {0}
        JournalScan_MSM, // Mass:;SM;0.00
        JournalScan_Age, // Age:;my;0.0
        JournalScan_RS, // Radius:
        JournalScan_Landable, // <;, Landable
        JournalScan_Terraformable, // <;, Terraformable
        JournalScan_Gravity, // Gravity:;G;0.0
        JournalScan_NoAtmosphere, // No Atmosphere
        JournalScan_LandC, // , Landable
        JournalScan_AMY, // Age: {0} my
        JournalScan_SolarMasses, // Solar Masses: {0:0.00}
        JournalScan_SurfaceTemp, // Surface Temp: {0}K
        JournalScan_GV, // Gravity: {0:0.0}g
        JournalScan_SPA, // Surface Pressure: {0} Atmospheres
        JournalScan_SPP, // Surface Pressure: {0} Pa
        JournalScan_Volcanism, // Volcanism: {0}
        JournalScan_NoVolcanism, // No Volcanism
        JournalScan_DistancefromArrivalPoint, // Distance from Arrival Point {0:N1}ls
        JournalScan_OrbitalPeriod, // Orbital Period: {0} days
        JournalScan_SMA, // Semi Major Axis: {0:0.00}AU
        JournalScan_SMK, // Semi Major Axis: {0}km
        JournalScan_OrbitalEccentricity, // Orbital Eccentricity: {0:0.000}
        JournalScan_OrbitalInclination, // Orbital Inclination: {0:0.000}´┐¢
        JournalScan_ArgOfPeriapsis, // Arg Of Periapsis: {0:0.000}´┐¢
        JournalScan_AscendingNode, 
        JournalScan_MeanAnomaly, 
        JournalScan_AbsoluteMagnitude, // Absolute Magnitude: {0:0.00}
        JournalScan_Axialtilt, // Axial tilt: {0:0.00}´┐¢
        JournalScan_RotationPeriod, // Rotation Period: {0} days
        JournalScan_Tidallylocked, // Tidally locked
        JournalScan_Candidateforterraforming, // Candidate for terraforming
        JournalScan_Moons, //  Moons
        JournalScan_Belt, // Belt
        JournalScan_Belts, // Belts
        JournalScan_Ring, // Ring
        JournalScan_Rings, // Rings
        JournalScan_CV, //  Current value: {0:N0}
        JournalScan_EV, //  Estimated value: {0:N0}
        JournalScan_FDV, //  First Discovered Value {0:N0}
        JournalScan_DB, //   Discovered by {0} on {1}
        JournalScan_Othersstarsnotconsidered, //  (Others stars not considered)
        JournalScan_Materials, // Materials:
        JournalScan_AtmosphericComposition, // Atmospheric Composition:
        JournalScan_PlanetaryComposition, // Planetary Composition:
        JournalScan_HabitableZone, //  - Habitable Zone, {0} ({1}-{2} AU),
        JournalScan_MetalRichplanets, //  - Metal Rich planets, {0} ({1}-{2} AU),
        JournalScan_WaterWorlds, //  - Water Worlds, {0} ({1}-{2} AU),
        JournalScan_EarthLikeWorlds, //  - Earth Like Worlds, {0} ({1}-{2} AU),
        JournalScan_AmmoniaWorlds, //  - Ammonia Worlds, {0} ({1}-{2} AU),
        JournalScan_IcyPlanets, //  - Icy Planets, {0} (from {1} AU)
        JournalScan_DISTA, // Dist:;ls;0.0
        JournalScan_DIST, // Dist:
        JournalScan_BNME, // Name:
        JournalScan_SNME, // Name:
        JournalScan_MPI, // Mapped
        JournalScan_MPIE, // Efficiently
        JournalScan_EVFD, //  First Discovered+Mapped value: {0:N0}
        JournalScan_EVFM, //  First Mapped value: {0:N0}
        JournalScan_EVM, //  Mapped value: {0:N0}
        JournalScan_SCNT, //  Scan Type: {0}
        JournalScan_EVAD, //  Already Discovered
        JournalScan_EVAM, //  Already Mapped
        JournalScan_InferredCircumstellarzones, // Inferred Circumstellar zones:
        JournalScan_MASS, // Mass:

        StarPlanetRing_Mass, //   Mass: {0:N4}{1}
        StarPlanetRing_InnerRadius, //   Inner Radius: {0:0.00}ls
        StarPlanetRing_OuterRadius, //   Outer Radius: {0:0.00}ls
        StarPlanetRing_IK, //   Inner Radius: {0}km
        StarPlanetRing_OK, //   Outer Radius: {0}km
        StarPlanetRing_Icy, // Icy
        StarPlanetRing_Rocky, // Rocky
        StarPlanetRing_MetalRich, // Metal Rich
        StarPlanetRing_Metallic, // Metallic
        StarPlanetRing_RockyIce, // Rocky Ice

        JournalApproachBody_In, // In
        JournalLeaveBody_In, // In
        JournalCommitCrime_Fine, //  Fine {0:N0}
        JournalCommitCrime_Bounty, //  Bounty {0:N0}

        FSSSignal_State, // State:
        FSSSignal_Faction, // Faction:
        FSSSignal_ThreatLevel, //  Threat Level:
        FSSSignal_StationBool, // ;Station:
        FSSSignal_CarrierBool, // ;Carrier:
        FSSSignal_MegashipBool, // ;Megaship:
        FSSSignal_InstallationBool, // ;Installation:
        FSSSignal_LastSeen, // Last Seen

        JournalEntry_Hyperspace,
        JournalEntry_Supercruise,

        JournalFSSDiscoveryScan_Progress, // Progress:;%;N1
        JournalFSSDiscoveryScan_Bodies, // Bodies:
        JournalFSSDiscoveryScan_Others, // Others:

        JournalFSSSignalDiscovered_Detected, // Detected ; signals
        JournalSAAScanComplete_Probes, // Probes:
        JournalSAAScanComplete_EfficiencyTarget, // Efficiency Target:

        JournalCargo_CargoShip, // Ship
        JournalCargo_CargoSRV, // SRV

        JournalCodexEntry_At, // At
        JournalCodexEntry_in, // in
        JournalCodexEntry_NewEntry, // ;New Entry
        JournalCodexEntry_Traits, // ;Traits

        JournalCarrier_At, 
        JournalCarrier_Callsign, 
        JournalCarrier_Name,
        JournalCarrier_JumpRange, 
        JournalCarrier_FuelLevel, 
        JournalCarrier_ToSystem, 
        JournalCarrier_Body,
        JournalCarrier_Refund, 
        JournalCarrier_RefundTime, 
        JournalCarrier_Deposit, 
        JournalCarrier_Withdraw,
        JournalCarrier_Balance, 
        JournalCarrier_ReserveBalance, 
        JournalCarrier_AvailableBalance,
        JournalCarrier_ReservePercent,
        JournalCarrier_TaxRate, // Tax Rate: ;;N1
        JournalCarrier_TaxRatePioneersupplies, // Tax Rate Pioneersupplies: ;;N1
        JournalCarrier_TaxRateShipyard, // Tax Rate Shipyard: ;;N1
        JournalCarrier_TaxRateRearm, // Tax Rate Rearm: ;;N1
        JournalCarrier_TaxRateOutfitting, // Tax Rate Outfitting: ;;N1
        JournalCarrier_TaxRateRefuel, // Tax Rate Refuel: ;;N1
        JournalCarrier_TaxRateRepair, // Tax Rate Repair: ;;N1
        JournalCarrier_Amount, 
        JournalCarrier_Operation, 
        JournalCarrier_Tier,
        JournalCarrier_Purchase,
        JournalCarrier_Sell,
        JournalCarrier_CancelSell,
        JournalCarrier_AllowNotorious,
        JournalCarrier_Access,

        JournalCarrier_TotalCapacity, 
        JournalCarrier_Crew,
        JournalCarrier_Cargo,
        JournalCarrier_CargoReserved,
        JournalCarrier_ShipPacks,
        JournalCarrier_ModulePacks,
        JournalCarrier_FreeSpace,

        CommunityGoal_Title, // Title:
        CommunityGoal_System, // System:
        CommunityGoal_At, // At:
        CommunityGoal_Expires, // Expires:
        CommunityGoal_NotComplete, // Not Complete;Complete
        CommunityGoal_CurrentTotal, // Current Total:
        CommunityGoal_Contribution, // Contribution:
        CommunityGoal_NumContributors, // Num Contributors:
        CommunityGoal_Player, // Player % Band:
        CommunityGoal_TopRank, // Top Rank:
        CommunityGoal_NotInTopRank, // Not In Top Rank;In Top Rank
        CommunityGoal_TierReached, // Tier Reached:
        CommunityGoal_Bonus, // Bonus:
        CommunityGoal_TopTierName, // Top Tier Name
        CommunityGoal_TT, // TT. Bonus

        JournalDocked_At, // At {0}

        JournalRepairDrone_Hull, // Hull:
        JournalReputation_Federation, // Federation:;;0.#
        JournalReputation_Empire, // Empire:;;0.#
        JournalReputation_Independent, // Independent:;;0.#
        JournalReputation_Alliance, // Alliance:;;0.#

        JournalCommodityPricesBase_PON, // Prices on ; items
        JournalCommodityPricesBase_CPBat, // < at
        JournalCommodityPricesBase_CPBin, // < in
        JournalCommodityPricesBase_Itemstobuy, // Items to buy:
        JournalCommodityPricesBase_CPBBuySell, // {0}: {1} sell {2} Diff {3} {4}%
        JournalCommodityPricesBase_CPBBuy, // {0}: {1}
        JournalCommodityPricesBase_SO, // Sell only Items:

        JournalEngineerProgress_Progresson, // Progress on ; Engineers
        JournalEngineerProgress_Rank, // Rank:

        JournalLocation_AtStat, // At {0}
        JournalLocation_LND, // Landed on {0}
        JournalLocation_AtStar, // At {0}

        JournalCarrierJump_JumpedWith, // Jumped with carrier {0} to {1}

        JournalFSDJump_Jumpto, // Jump to {0}
        JournalFSDJump_Fuel, //  Fuel
        JournalFSDJump_left, //  left

        JournalStartJump_ChargingFSD, // Charging FSD

        JournalShipTargeted_Hull, // Hull ;;N1
        JournalShipTargeted_at, // < at ;;N1
        JournalShipTargeted_MC, //  Target Events
        JournalShipTargeted_in, // < in

        JournalUnderAttack_ACOUNT, // times

        JournalMaterialDiscovered_DN, // , Discovery {0}

        JournalMicroResources_Items, 
        JournalMicroResources_Components, 
        JournalMicroResources_Consumables, 
        JournalMicroResources_Data, 

        JournalMaterials_Raw, // Raw:
        JournalMaterials_Manufactured, // Manufactured:
        JournalMaterials_Encoded, // Encoded:

        JournalShipyardSell_At, // At:
        JournalShipyardSwap_Swap, // Swap
        JournalShipyardSwap_fora, // < for a
        JournalShipyardTransfer_Of, // Of

        JournalStoredShips_Atstarport, // At starport:
        JournalStoredShips_Otherlocations, // Other locations:
        JournalStoredShips_SSP, // ; cr;N0
        JournalStoredShips_Remote, // Remote:
        JournalStoredShips_intransit, // <; in transit
        JournalStoredShips_at, // < at

        JournalSupercruiseExit_in, // < in
        JournalSupercruiseExit_At, // At

        JournalReceiveText_From, // From:
        JournalReceiveText_Text, //  Texts
        JournalReceiveText_FC, // from
        JournalReceiveText_on, // < on

        JournalSendText_To, // To:
        JournalSendText_Msg, // Msg:

        JournalScreenshot_At, // At
        JournalScreenshot_in, // < in
        JournalScreenshot_File, // File:
        JournalScreenshot_Width, // Width:
        JournalScreenshot_Height, // Height:
        JournalSearchAndRescue_Reward, // Reward:
        JournalSellExplorationData_Total, // Total:; cr;N0
        JournalSetUserShipName_On, // On:
        JournalMultiSellExplorationData_Total, // Total:; cr;N0
        JournalSellOrganics_Detailed, // Genus: {0}, Species: {1}, Reward: {2} cr, First sample bonus: {3} cr, Total reward: {4} cr

        JournalMissionAccepted_from, // < from
        JournalMissionAccepted_Expiry, // Expiry:
        JournalMissionAccepted_Influence, // Influence:
        JournalMissionAccepted_Reputation, // Reputation:
        JournalMissionAccepted_Reward, // Reward:; cr;N0
        JournalMissionAccepted_Wing, // ; (Wing)
        JournalMissionAccepted_Deliver, // Deliver:
        JournalMissionAccepted_TargetType, // Target Type:
        JournalMissionAccepted_KillCount, // Kill Count:
        JournalMissionAccepted_Passengers, // Passengers:
        JournalMissionAccepted_Count, // Count:
        JournalMissionRedirected_From, // From:
        JournalMissionRedirected_To, // To:
        MissionItem_Passenger, // <;(Passenger)
        MissionItem_Expires, // Expires:

        JournalProspectedAsteroid_Remaining, // Remaining:;%;N1

        JournalPromotion_Combat, // Combat
        JournalPromotion_Trade, // Trade
        JournalPromotion_Exploration, // Exploration
        JournalPromotion_ExoBiologist,
        JournalPromotion_Soldier,
        JournalPromotion_Empire, // Empire
        JournalPromotion_Federation, // Federation
        JournalPromotion_CQC, // CQC

        JournalLoadout_Modules, // Modules:
        JournalModuleInfo_Modules, // Modules:
        JournalStoredModules_at, // < at

        JournalFuelScoop_Total, // Total:;t;0.0
        JournalReservoirReplenished_Main, // Main:;t;0.0
        JournalReservoirReplenished_Reservoir, // Reservoir:;t;0.0
        
        EDPlanet_Metalrichbody, // Metal-rich body
        EDPlanet_Highmetalcontentbody, // High metal content world
        EDPlanet_Rockybody, // Rocky body
        EDPlanet_Icybody, // Icy body
        EDPlanet_Rockyicebody, // Rocky ice world
        EDPlanet_Earthlikebody, // Earth-like world
        EDPlanet_Waterworld, // Water world
        EDPlanet_Ammoniaworld, // Ammonia world
        EDPlanet_Watergiant, // Water giant
        EDPlanet_Watergiantwithlife, // Water giant with life
        EDPlanet_Gasgiantwithwaterbasedlife, // Gas giant with water-based life
        EDPlanet_Gasgiantwithammoniabasedlife, // Gas giant with ammonia-based life
        EDPlanet_SudarskyclassIgasgiant, // Class I gas giant
        EDPlanet_SudarskyclassIIgasgiant, // Class II gas giant
        EDPlanet_SudarskyclassIIIgasgiant, // Class III gas giant
        EDPlanet_SudarskyclassIVgasgiant, // Class IV gas giant
        EDPlanet_SudarskyclassVgasgiant, // Class V gas giant
        EDPlanet_Heliumrichgasgiant, // Helium-rich gas giant
        EDPlanet_Heliumgasgiant, // Helium gas giant
        EDPlanet_Unknown, // Unknown planet type

        CommanderForm, // Control 'CommanderForm'
        CommanderForm_extGroupBoxCommanderInfo, // Control 'Other'
        CommanderForm_HomeSys, // Control 'Home System:'
        CommanderForm_labelMapCol, // Control 'Default Map Color'
        CommanderForm_groupBoxCustomIGAU, // Control 'Intergalactic Astronomical Union [IGAU]'
        CommanderForm_checkBoxIGAUSync, // Control 'Send Codex Entry Discovery Data to IGAU'
        CommanderForm_extGroupBoxEDAstro, // Control 'EDAstro'
        CommanderForm_extCheckBoxEDAstro, // Control 'Send Events to EDAstro'
        CommanderForm_groupBoxCustomInara, // Control 'Inara Information (optional)'
        CommanderForm_labelINARAN, // Control 'Inara Name:'
        CommanderForm_labelInaraAPI, // Control 'Inara API Key:'
        CommanderForm_checkBoxCustomInara, // Control 'Sync to Inara'
        CommanderForm_groupBoxCustomEDSM, // Control 'EDSM Information (optional)'
        CommanderForm_checkBoxCustomEDSMFrom, // Control 'Sync From EDSM'
        CommanderForm_labelEDSMAPI, // Control 'EDSM API Key:'
        CommanderForm_labelEDSMN, // Control 'EDSM Name:'
        CommanderForm_checkBoxCustomEDSMTo, // Control 'Sync to EDSM'
        CommanderForm_groupBoxCustomEDDN, // Control 'EDDN'
        CommanderForm_checkBoxCustomEDDNTo, // Control 'Send Event Information to EDDN'
        CommanderForm_groupBoxCustomJournal, // Control 'Journal Related Information'
        CommanderForm_labelCN, // Control 'Commander Name:'
        CommanderForm_labelJL, // Control 'Journal Location:'
        CommanderForm_buttonExtBrowse, // Control 'Browse'
        CommanderForm_extCheckBoxConsoleCommander, // Control 'Console Commander'
        CommanderForm_panel_defaultmapcolor_ToolTip, // ToolTip 'New travel entries get this colour on the map'
        CommanderForm_checkBoxIGAUSync_ToolTip, // ToolTip 'https://github.com/Elite-IGAU/publications/blob/master/IGAU_Codex.csv'
        CommanderForm_checkBoxCustomInara_ToolTip, // ToolTip 'Sync with Inara'
        CommanderForm_textBoxBorderInaraAPIKey_ToolTip, // ToolTip 'Enter the API key from the Inara Website\\nGet an Inara API key from https://inara.cz'
        CommanderForm_textBoxBorderInaraName_ToolTip, // ToolTip 'Give the user name for this commander on Inara'
        CommanderForm_checkBoxCustomEDSMFrom_ToolTip, // ToolTip 'Receive any FSD jumps from EDSM that are on their database but not in EDDiscovery'
        CommanderForm_checkBoxCustomEDSMTo_ToolTip, // ToolTip 'Send your travel and ship data to EDSM'
        CommanderForm_textBoxBorderEDSMAPI_ToolTip, // ToolTip 'Enter the API key from the EDSM Website\\nGet an EDSM API key from https://www.edsm.net in "My account" menu'
        CommanderForm_textBoxBorderEDSMName_ToolTip, // ToolTip 'Give the name this commander is known as in EDSM'
        CommanderForm_checkBoxCustomEDDNTo_ToolTip, // ToolTip 'Click to send journal information to EDDN. EDDN feeds tools such as EDDB, EDSM, Inara with data from commanders. All data is made anonymised'
        CommanderForm_textBoxBorderCmdr_ToolTip, // ToolTip 'Enter commander name as used in Elite Dangerous'
        CommanderForm_buttonExtBrowse_ToolTip, // ToolTip 'Browse to the the journal folder'
        CommanderForm_textBoxBorderJournal_ToolTip, // ToolTip 'Enter the journal location folder.  Normally leave this field blank only if you are using EDD on another computer than your play computer\\nLeave override journal location blank to use the standard Frontier location for journals'
        CommanderForm_extCheckBoxIncludeSubfolders,
        CommanderForm_LF, // Select folder where Journal*.log files are stored by Frontier in
        CommanderForm_ND, // Folder does not exist

        ScreenShotConfigureForm, // Control 'Screen Shot Configure'
        ScreenShotConfigureForm_labelQuality, // Control 'JPEG Quality'
        ScreenShotConfigureForm_extCheckBoxEnabled, // Control 'Conversion Enabled'
        ScreenShotConfigureForm_extCheckBoxHiRes, // Control 'Mark HiRes Files'
        ScreenShotConfigureForm_extCheckBoxKeepMasterConvertedImage, // Control 'Keep master converted image'
        ScreenShotConfigureForm_extGroupBox1, // Control 'Crop/Resize Settings'
        ScreenShotConfigureForm_labelHeight2, // Control 'Height'
        ScreenShotConfigureForm_labelWidth2, // Control 'Width'
        ScreenShotConfigureForm_labelLeft2, // Control 'Left'
        ScreenShotConfigureForm_labelTop2, // Control 'Top'
        ScreenShotConfigureForm_groupBoxCropSettings, // Control 'Crop/Resize Settings'
        ScreenShotConfigureForm_labelHeight, // Control 'Height'
        ScreenShotConfigureForm_labelWidth, // Control 'Width'
        ScreenShotConfigureForm_labelLeft, // Control 'Left'
        ScreenShotConfigureForm_labelTop, // Control 'Top'
        ScreenShotConfigureForm_labelFolder, // Control 'ED/Steam Screenshot folder'
        ScreenShotConfigureForm_buttonChangeScreenshotsFolder, // Control 'Browse'
        ScreenShotConfigureForm_labelSubfolder, // Control 'In Sub folder'
        ScreenShotConfigureForm_labelImage2, // Control 'Image 2'
        ScreenShotConfigureForm_labelImage1, // Control 'Image 1'
        ScreenShotConfigureForm_labelCropResizeOptions, // Control 'Crop/Resize Options'
        ScreenShotConfigureForm_labelFileNameFormat, // Control 'In Filename format'
        ScreenShotConfigureForm_labelStoreFolder, // Control 'Store Converted pictures'
        ScreenShotConfigureForm_extButtonBrowseMoveOrg, // Control 'Browse'
        ScreenShotConfigureForm_buttonEDChangeOutputFolder, // Control 'Browse'
        ScreenShotConfigureForm_labelClipboard, // Control 'Clipboard'
        ScreenShotConfigureForm_labelOriginal, // Control 'Original Image'
        ScreenShotConfigureForm_labelEnabled, // Control 'Enable'
        ScreenShotConfigureForm_labelScanFor, // Control 'Scan for'
        ScreenShotConfigureForm_labelSaveAs, // Control 'Save as'
        Screenshot_Folder, // Select screenshot folder
        Screenshot_Identical, // Cannot set input..
        Screenshot_FolderNotExist, // Folder specified does not exist

        JournalTypeEnum_CapShipBond, // Capital Ship Bond
        JournalTypeEnum_FactionKillBond, // Faction Kill Bond
        JournalTypeEnum_Touchdown, // Touchdown

        UserControlScan_Expired,
        ScanDisplayUserControl_NSD, // No scan data available
        ScanDisplayUserControl_BC, // Barycentre of {0}
        ScanDisplayUserControl_SurfaceFeatures, // Surface Features
        ScanDisplayUserControl_Signals, // Signals
        ScanDisplayUserControl_Genuses, // Signals
        ScanDisplayUserControl_Organics, // Organics
        ScanDisplayUserControl_Codex,

        JournalScanInfo_isa, // is a(n)
        JournalScanInfo_Atmosphere, // Atmosphere
        JournalScanInfo_SurfaceTemperature, //Surface temperature: 
        JournalScanInfo_terraformable, // terraformable
        JournalScanInfo_LowRadius, //  Low Radius.
        JournalScanInfo_LargeRadius, //  Large Radius.
        JournalScanInfo_Signals, //  Has mining signals.
        JournalScanInfo_BioSignals, //  Biological signals:
        JournalScanInfo_GeoSignals, //  Geological signals:
        JournalScanInfo_ThargoidSignals, //  Thargoid signals:
        JournalScanInfo_GuardianSignals, //  Guardian signals:
        JournalScanInfo_HumanSignals, //  Human signals:
        JournalScanInfo_OtherSignals, //  'Other' signals:
        JournalScanInfo_MandD, //  (Mapped & Discovered)
        JournalScanInfo_MP, //  (Mapped)
        JournalScanInfo_DIS, //  (Discovered)
        JournalScanInfo_islandable, // is landable
        JournalScanInfo_Hasring, //  Has ring.
        JournalScanInfo_Has, //  Has
        JournalScanInfo_eccentricity, // high eccentricity
        JournalScanInfo_unknownAtmosphere, //unknown
        JournalScanInfo_BFSD, //  Basic
        JournalScanInfo_SFSD, //  Standard
        JournalScanInfo_PFSD, //  Premium
        JournalScanInfo_LE, // {0} has {1} level elements.
        JournalScanInfo_scanorganics, // Has been scanned for organics

        Services_BridgeCrew, Services_CommodityTrading, Services_TritiumDepot,        // not listed in crew services, but core items
        Services_Refuel, Services_Repair, Services_Rearm, Services_VoucherRedemption, Services_Shipyard, Services_Outfitting, Services_BlackMarket,
        Services_Exploration, Services_Bartender, Services_VistaGenomics, Services_PioneerSupplies,

        Signals_RingHotSpot,
    }

    internal static class EDTranslatorExtensions
    {
        static public string T(this string s, EDCTx value)       // use the enum.  This was invented before the shift to all Enums of feb 22
        {
            return s.TxID(value);
        }
        static public string TCond(bool translate, string s, EDCTx value)         
        {
            return translate ? s.TxID(value) : s;
        }
    }
}
