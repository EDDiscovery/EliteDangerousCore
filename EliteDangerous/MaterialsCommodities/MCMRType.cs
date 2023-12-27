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
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("Mat {Category} {Type} {MaterialGroup} {Name} {FDName} {Shortname}")]
    public class MaterialCommodityMicroResourceType
    {
        enum MCMR
        {
            //---------------------------------------------------------- Commodity
            Advert1 = 0, AlacarakmoSkinArt, AlienEggs, AltairianSkin, BuckyballBeerMats, Clothing, ConsumerTechnology, CrystallineSpheres,
            DomesticAppliances, Duradrives, EleuThermals, EshuUmbrellas, EvacuationShelter, HavasupaiDreamCatcher, JaquesQuinentianStill, JaradharrePuzzlebox,
            JotunMookah, KaretiiCouture, KinagoInstruments, MomusBogSpaniel, NgunaModernAntiques, NjangariSaddles, OphiuchiExinoArtefacts, PersonalGifts,
            RajukruStoves, ShansCharisOrchid, SoontillRelics, SurvivalEquipment, TheHuttonMug, TiolceWaste2PasteUnits, UzumokuLowGWings, VidavantianLace,
            ZeesszeAntGlue, AgronomicTreatment, AnduligaFireWorks, DeltaPhoenicisPalms, Explosives, HIPOrganophosphates, HydrogenFuel, HydrogenPeroxide,
            KorroKungPellets, LiquidOxygen, MineralOil, NerveAgents, Pesticides, RockforthFertiliser, SurfaceStabilisers, SyntheticReagents,
            ToxandjiVirocide, Tritium, Water, Drones, AerialEdenApple, AlbinoQuechuaMammoth, Algae, AnimalMeat,
            AnyNaCoffee, AroucaConventualSweets, BakedGreebles, BaltahSineVacuumKrill, CD75CatCoffee, CeremonialHeikeTea, CetiAepyornisEgg, CetiRabbits,
            ChiEridaniMarinePaste, Coffee, CoquimSpongiformVictuals, DeuringasTruffles, DisoMaCorn, EsusekuCaviar, EthgrezeTeaBuds, Fish,
            FoodCartridges, FruitandVegetables, GiantIrukamaSnails, GomanYauponCoffee, Grain, HaidneBlackBrew, HelvetitjPearls, HIP10175BushMeat,
            HIP41181Squid, HR7221Wheat, JarouaRice, KarsukiLocusts, LFTVoidExtractCoffee, LiveHecateSeaWorms, LTTHyperSweet, MechucosHighTea,
            MokojingBeastFeast, MukusubiiChitinOs, MulachiGiantFungus, NeritusBerries, OchoengChillies, OrrerianViciousBrew, SanumaMEAT, SyntheticMeat,
            TanmarkTranquilTea, Tea, UszaianTreeGrub, UtgaroarMillenialEggs, WheemeteWheatCakes, WitchhaulKobeBeef, CeramicComposites, CMMComposite,
            CoolingHoses, InsulatingMembrane, MedbStarlube, Metaalloys, NeofabricInsulation, Polymers, Semiconductors, Superconductors,
            BastSnakeGin, Beer, BlueMilk, BootlegLiquor, BurnhamBileDistillate, CentauriMegaGin, ChateauDeAegaeon, EraninPearlWhisky,
            GerasianGueuzeBeer, IndiBourbon, KamitraCigars, KonggaAle, LavianBrandy, LeestianEvilJuice, Liquor, RusaniOldSmokey,
            SaxonWine, ThrutisCream, Tobacco, Wine, WuthieloKuFroth, YasoKondiLeaf, ArticulationMotors, AtmosphericExtractors,
            BuildingFabricators, CropHarvesters, EmergencyPowerCells, ExhaustManifold, GeologicalEquipment, GiantVerrix, HeatsinkInterlink, HeliostaticFurnaces,
            HNShockMount, IonDistributor, MagneticEmitterCoil, MarineSupplies, MineralExtractors, ModularTerminals, NonEuclidianExotanks, PowerConverter,
            PowerGenerators, PowerGridAssembly, PowerTransferConduits, RadiationBaffle, ReinforcedMountingPlate, SkimerComponents, ThermalCoolingUnits, VolkhabBeeDrones,
            WaterPurifiers, WulpaHyperboreSystems, AdvancedMedicines, AganippeRush, AgriculturalMedicines, AlyaBodilySoap, BasicMedicines, CombatStabilisers,
            FujinTea, HonestyPills, KachiriginLeaches, Nanomedicines, PantaaPrayerSticks, PerformanceEnhancers, ProgenitorCells, TauriChimes,
            TerraMaterBloodBores, VegaSlimWeed, VHerculisBodyRub, WatersOfShintara, Aluminium, Beryllium, Bismuth, Cobalt,
            Copper, Gallium, Gold, Hafnium178, Indium, Lanthanum, Lithium, Osmium,
            Palladium, Platinum, PlatinumAloy, Praseodymium, Samarium, Silver, SothisCrystallineGold, Tantalum,
            Thallium, Thorium, Titanium, Uranium, Alexandrite, Bauxite, Benitoite, Bertrandite,
            Bromellite, CherbonesBloodCrystals, Coltan, Cryolite, Gallite, Goslarite, Grandidierite, Indite,
            Jadeite, Lepidolite, LithiumHydroxide, lowtemperaturediamond, MethaneClathrate, methanolmonohydratecrystals, Moissanite, Monazite,
            Musgravite, NgadandariFireOpals, Opal, Painite, Pyrophyllite, Rhodplumsite, Rutile, Serendibite,
            Taaffeite, Uraninite, AnimalEffigies, ApaVietii, BasicNarcotics, GeawenDanceDust, HarmaSilverSeaRum, LyraeWeed,
            MotronaExperienceJelly, OnionHead, OnionHeadA, OnionHeadB, OnionHeadC, PavonisEarGrubs, TarachTorSpice, TransgenicOnionHead,
            Wolf1301Fesh, AislingMediaMaterials, AislingMediaResources, AislingPromotionalMaterials, AllianceLegaslativeContracts, AllianceLegaslativeRecords, AllianceTradeAgreements, CounterCultureSupport,
            FederalAid, FederalTradeContracts, GromCounterIntelligence, GromWarTrophies, IllicitConsignment, ImperialPrisoner, LavignyCorruptionDossiers, LavignyFieldSupplies,
            LavignyGarisonSupplies, LiberalCampaignMaterials, LoanedArms, MarkedSlaves, OnionheadDerivatives, OnionheadSamples, OutOfDateGoods, PatreusFieldSupplies,
            PatreusGarisonSupplies, RepublicanFieldSupplies, RepublicanGarisonSupplies, RestrictedIntel, RestrictedPackage, SiriusCommercialContracts, SiriusFranchisePackage, SiriusIndustrialEquipment,
            TorvalCommercialContracts, TorvalDeeds, UndergroundSupport, UnmarkedWeapons, UtopianDissident, UtopianFieldSupplies, UtopianPublicity, AiRelics,
            AncientCasket, AncientKey, AncientOrb, AncientRelic, AncientRelicTG, AncientTablet, AncientTotem, AncientUrn,
            AntimatterContainmentUnit, AntiqueJewellery, Antiquities, AssaultPlans, ClassifiedExperimentalEquipment, ComercialSamples, CoralSap, DamagedEscapePod,
            DataCore, DiplomaticBag, EarthRelics, EncriptedDataStorage, EncryptedCorrespondence, FossilRemnants, GalacticTravelGuide, GeneBank,
            GeologicalSamples, Hostage, LargeExplorationDatacash, M_TissueSample_Fluid, M_TissueSample_Nerves, M_TissueSample_Soft, M3_TissueSample_Membrane, M3_TissueSample_Mycelium,
            M3_TissueSample_Spores, MilitaryIntelligence, MysteriousIdol, OccupiedCryoPod, P_ParticulateSample, PersonalEffects, PoliticalPrisoner, PreciousGems,
            ProhibitedResearchMaterials, S_TissueSample_Cells, S_TissueSample_Core, S_TissueSample_Surface, S6_TissueSample_Cells, S6_TissueSample_Coenosarc, S6_TissueSample_Mesoglea, S9_TissueSample_Shell,
            Sap8CoreContainer, ScientificResearch, ScientificSamples, SmallExplorationDatacash, SpacePioneerRelics, TacticalData, ThargoidGeneratorTissueSample, ThargoidHeart,
            ThargoidPod, ThargoidScoutTissueSample, ThargoidTissueSampleType1, ThargoidTissueSampleType10a, ThargoidTissueSampleType10b, ThargoidTissueSampleType10c, ThargoidTissueSampleType2, ThargoidTissueSampleType3,
            ThargoidTissueSampleType4, ThargoidTissueSampleType5, ThargoidTissueSampleType6, ThargoidTissueSampleType7, ThargoidTissueSampleType9a, ThargoidTissueSampleType9b, ThargoidTissueSampleType9c, TimeCapsule,
            TrinketsOfFortune, UnknownArtifact, UnknownArtifact2, UnknownArtifact3, UnknownBiologicalMatter, UnknownResin, UnknownSack, UnknownTechnologySamples,
            UnocuppiedEscapePod, UnstableDataCore, USSCargoAncientArtefact, USSCargoBlackBox, USSCargoExperimentalChemicals, USSCargoMilitaryPlans, USSCargoPrototypeTech, USSCargoRareArtwork,
            USSCargoRebelTransmissions, USSCargoTechnicalBlueprints, USSCargoTradeData, WreckageComponents, ImperialSlaves, MasterChefs, Slaves, AdvancedCatalysers,
            AnimalMonitors, AquaponicSystems, autofabricators, AZCancriFormula42, BioreducingLichen, ComputerComponents, DiagnosticSensor, HazardousEnvironmentSuits,
            MedicalDiagnosticEquipment, MicroControllers, MutomImager, Nanobreakers, ResonatingSeparators, Robotics, StructuralRegulators, TelemetrySuite,
            TerrainEnrichmentSystems, XiheCompanions, BankiAmphibiousLeather, BelalansRayLeather, ChameleonCloth, ConductiveFabrics, DamnaCarapaces, Leather,
            MilitaryGradeFabrics, NaturalFabrics, RapaBaoSnakeSkins, SyntheticFabrics, TiegfriesSynthSilk, VanayequiRhinoFur, Biowaste, ChemicalWaste,
            Scrap, ToxicWaste, BattleWeapons, BorasetaniPathogenetics, GilyaSignatureWeapons, HIP118311Swarm, HolvaDuellingBlades, KamorinHistoricWeapons,
            Landmines, NonLethalWeapons, PersonalWeapons, ReactiveArmour,
            //---------------------------------------------------------- Raw
            Carbon = 1000, Iron, Lead, Nickel, Phosphorus, Rhenium, Sulphur, Arsenic,
            Chromium, Germanium, Manganese, Vanadium, Zinc, Zirconium, Boron, Cadmium,
            Mercury, Molybdenum, Niobium, Tin, Tungsten, Antimony, Polonium, Ruthenium,
            Selenium, Technetium, Tellurium, Yttrium,
            //---------------------------------------------------------- Encoded
            BulkScandata = 2000, DisruptedWakeEchoes, EncryptedFiles, LegacyFirmware, ScrambledeMissionData, ShieldCycleRecordings, ArchivedEmissionData, ConsumerFirmware,
            EncryptionCodes, FSDTelemetry, ScanArchives, ShieldSoakAnalysis, TG_StructuralData, EmissionData, IndustrialFirmware, ScanDataNanks,
            ShieldDensityReports, SymmetricKeys, TG_CompositionData, TG_ShipFlightData, UnknownShipSignature, WakeSolutions, AncientBiologicalData, AncientCulturalData,
            AncientHistoricalData, AncientLanguageData, AncientTechnologicalData, DecodedEmissionData, EncodedScandata, EncryptionArchives, Guardian_ModuleBlueprint, Guardian_WeaponBlueprint,
            HyperspaceTrajectories, SecurityFirmware, ShieldPatternAnalysis, TG_ResidueData, TG_ShipSystemsData, UnknownWakeData, AdaptiveEncryptors, ClassifiedScandata,
            CompactEmissionsData, DataminedWake, EmbeddedFirmware, Guardian_VesselBlueprint, ShieldFrequencyData,
            //---------------------------------------------------------- Manufactured
            BasicConductors = 3000, ChemicalStorageUnits, CompactComposites, CrystalShards, GridResistors, Guardian_PowerCell, Guardian_Sentinel_WreckageComponents, HeatConductionWiring,
            MechanicalScrap, SalvagedAlloys, TemperedAlloys, TG_Abrasion03, WornShieldEmitters, ChemicalProcessors, ConductiveComponents, FilamentComposites,
            GalvanisingAlloys, Guardian_PowerConduit, HeatDispersionPlate, HeatResistantCeramics, HybridCapacitors, MechanicalEquipment, ShieldEmitters, UncutFocusCrystals,
            UnknownCarapace, ChemicalDistillery, ConductiveCeramics, ElectrochemicalArrays, FocusCrystals, Guardian_Sentinel_WeaponParts, Guardian_TechComponent, HeatExchangers,
            HighDensityComposites, MechanicalComponents, PhaseAlloys, PrecipitatedAlloys, ShieldingSensors, TG_Abrasion02, TG_BioMechanicalConduits, TG_CausticGeneratorParts,
            TG_CausticShard, TG_InterdictionData, TG_ShutdownData, UnknownEnergycell, ChemicalManipulators, CompoundShielding, ConductivePolymers, ConfigurableComponents,
            FedProprietaryComposites, HeatVanes, PolymerCapacitors, ProtoLightAlloys, RefinedFocusCrystals, TG_CausticCrystal, TG_WeaponParts, ThermicAlloys,
            UnknownTechnologyComponents, BiotechConductors, ExquisiteFocusCrystals, FedCoreComposites, HeatExposureSpecimen, ImperialShielding, ImprovisedComponents, MilitaryGradeAlloys,
            MilitarySupercapacitors, PharmaceuticalIsolators, ProtoHeatRadiators, ProtoRadiolicAlloys, TG_PropulsionElement, Unknownenergysource, UnknownOrganicCircuitry,
            //---------------------------------------------------------- Item
            AgriculturalProcessSample = 4000, BiochemicalAgent, BuildingSchematic, Californium, CastFossil, ChemicalProcessSample, ChemicalSample, CompactLibrary,
            CompressionLiquefiedGas, DeepMantleSample, DegradedPowerRegulator, GeneticRepairMeds, GeneticSample, GMeds, HealthMonitor, Hush,
            InertiaCanister, infinity, InorganicContaminant, insight, InsightDataBank, InsightEntertainmentSuite, IonisedGas, LargeCapacityPowerRegulator,
            Lazarus, MicrobialInhibitor, MutagenicCatalyst, NutritionalConcentrate, PersonalComputer, PersonalDocuments, PetrifiedFossil, Push,
            PyrolyticCatalyst, RefinementProcessSample, ShipSchematic, SuitSchematic, SurveillanceEquipment, SyntheticGenome, SyntheticPathogen, TrueFormFossil,
            UniversalTranslator, VehicleSchematic, WeaponSchematic,
            //---------------------------------------------------------- Component
            Aerogel = 5000, CarbonFibrePlating, ChemicalCatalyst, ChemicalSuperbase, Circuitboard, CircuitSwitch, ElectricalFuse, ElectricalWiring,
            Electromagnet, EncryptedMemoryChip, Epinephrine, EpoxyAdhesive, Graphene, IonBattery, MemoryChip, MetalCoil,
            MicroElectrode, MicroHydraulics, MicroSupercapacitor, MicroThrusters, Microtransformer, Motor, OpticalFibre, OpticalLens,
            OxygenicBacteria, pHNeutraliser, RDX, Scrambler, TitaniumPlating, Transmitter, TungstenCarbide, ViscoElasticPolymer,
            WeaponComponent,
            //---------------------------------------------------------- Data
            AccidentLogs = 6000, AirqualityReports, AtmosphericData, AudioLogs, AXCombatLogs, BallisticsData, BiologicalWeaponData, BiometricData,
            BlacklistData, BloodtestResults, CampaignPlans, CatMedia, CensusData, ChemicalExperimentData, ChemicalFormulae, ChemicalInventory,
            ChemicalPatents, ChemicalWeaponData, ClassicEntertainment, CocktailRecipes, CombatantPerformance, CombatTrainingMaterial, ConflictHistory, CriminalRecords,
            CropYieldAnalysis, CulinaryRecipes, DigitalDesigns, DutyRota, EmployeeDirectory, EmployeeExpenses, EmployeeGeneticData, EmploymentHistory,
            EnhancedInterrogationRecordings, EspionageMaterial, EvacuationProtocols, ExplorationJournals, ExtractionYieldData, FactionAssociates, FactionDonatorList, FactionNews,
            FinancialProjections, FleetRegistry, GeneSequencingData, GeneticResearch, GeographicalData, GeologicalData, HydroponicData, IncidentLogs,
            InfluenceProjections, InternalCorrespondence, InterrogationRecordings, InterviewRecordings, JobApplications, Kompromat, LiteraryFiction, MaintenanceLogs,
            ManufacturingInstructions, MedicalRecords, MedicalTrialRecords, MeetingMinutes, MineralSurvey, MiningAnalytics, MultimediaEntertainment, NetworkAccessHistory,
            NetworkSecurityProtocols, NextofkinRecords, NOCData, OpinionPolls, PatientHistory, PatrolRoutes, PayrollInformation, PersonalLogs,
            PharmaceuticalPatents, PhotoAlbums, PlantGrowthCharts, PoliticalAffiliations, PperationalManual, PrisonerLogs, ProductionReports, ProductionSchedule,
            Propaganda, PurchaseRecords, PurchaseRequests, RadioactivityData, ReactorOutputReview, RecyclingLogs, ResidentialDirectory, RiskAssessments,
            SalesRecords, SecurityExpenses, SeedGeneaology, SettlementAssaultPlans, SettlementDefencePlans, ShareholderInformation, SlushFundLogs, SmearCampaignPlans,
            SpectralAnalysisData, Spyware, StellarActivityLogs, SurveilleanceLogs, TacticalPlans, TaxRecords, TopographicalSurveys, TravelPermits,
            TroopDeploymentRecords, UnionmemberShip, VaccinationRecords, VaccineResearch, VIPSecurityDetail, VirologyData, Virus, VisitorRegister,
            WeaponInventory, WeaponTestData, XenoDefenceProtocols,
            //---------------------------------------------------------- Consumable
            AMM_Grenade_EMP = 7000, AMM_Grenade_Frag, AMM_Grenade_Shield, Bypass, EnergyCell, HealthPack,
        }

        public enum CatType
        {
            Commodity, Raw, Encoded, Manufactured,          // all
            Item,                                           // odyssey 4.0.  Called goods in game
            Component,                                      // odyssey 4.0.  Called assets in game
            Data,                                           // odyssey 4.0.  
            Consumable,                                     // odyssey 4.0. 
        };
        public CatType Category { get; private set; }               // see above

        public string TranslatedCategory { get; private set; }      // translation of above..

        public string Name { get; private set; }                    // name of it in nice text. This gets translated
        public string EnglishName { get; private set; }             // name of it in English
        public string FDName { get; private set; }                  // fdname, lower case..
        public enum ItemType
        {
            VeryCommon, Common, Standard, Rare, VeryRare,           // materials
            Unknown,                                                // used for microresources
            ConsumerItems, Chemicals, Drones, Foods, IndustrialMaterials, LegalDrugs, Machinery, Medicines, Metals, Minerals, Narcotics, PowerPlay,     // commodities..
            Salvage, Slaves, Technology, Textiles, Waste, Weapons,
        };

        public ItemType Type { get; private set; }                  // and its type, for materials its commonality, for commodities its group ("Metals" etc), for microresources Unknown
        public string TranslatedType { get; private set; }          // translation of above..        

        public enum MaterialGroupType                               // Material trader group type
        {
            NA,
            RawCategory1, RawCategory2, RawCategory3, RawCategory4, RawCategory5, RawCategory6, RawCategory7,
            EncodedEmissionData, EncodedWakeScans, EncodedShieldData, EncodedEncryptionFiles, EncodedDataArchives, EncodedFirmware,
            ManufacturedChemical, ManufacturedThermic, ManufacturedHeat, ManufacturedConductive, ManufacturedMechanicalComponents,
            ManufacturedCapacitors, ManufacturedShielding, ManufacturedComposite, ManufacturedCrystals, ManufacturedAlloys,
        };


        public MaterialGroupType MaterialGroup { get; private set; } // only for materials, grouping
        public string TranslatedMaterialGroup { get; private set; }

        public string Shortname { get; private set; }               // short abv. name
        public Color Colour { get; private set; }                   // colour if its associated with one
        public bool Rarity { get; private set; }                    // if it is a rare commodity

        public bool IsCommodity { get { return Category == CatType.Commodity; } }
        public bool IsMaterial { get { return Category == CatType.Encoded || Category == CatType.Manufactured || Category == CatType.Raw; } }
        public bool IsRaw { get { return Category == CatType.Raw; } }
        public bool IsEncoded { get { return Category == CatType.Encoded; } }
        public bool IsManufactured { get { return Category == CatType.Manufactured; } }
        public bool IsEncodedOrManufactured { get { return Category == CatType.Encoded || Category == CatType.Manufactured; } }
        public bool IsRareCommodity { get { return Rarity && IsCommodity; } }
        public bool IsNormalCommodity { get { return !Rarity && IsCommodity; } }
        public bool IsCommonMaterial { get { return Type == ItemType.Common || Type == ItemType.VeryCommon; } }

        public bool IsMicroResources { get { return Category >= CatType.Item; } }     // odyssey 4.0
        public bool IsConsumable { get { return Category == CatType.Consumable; } }     // odyssey 4.0
        public bool IsData { get { return Category == CatType.Data; } }
        public bool IsItem { get { return Category == CatType.Item; } }
        public bool IsComponent { get { return Category == CatType.Component; } }

        public bool IsJumponium
        {
            get
            {
                return (FDName.Contains("arsenic") || FDName.Contains("cadmium") || FDName.Contains("carbon")
                    || FDName.Contains("germanium") || FDName.Contains("niobium") || FDName.Contains("polonium")
                    || FDName.Contains("vanadium") || FDName.Contains("yttrium"));
            }
        }

        // any case accepted
        static public bool IsJumponiumType(string fdname)
        {
            fdname = fdname.ToLowerInvariant();
            return (fdname.Contains("arsenic") || fdname.Contains("cadmium") || fdname.Contains("carbon")
                || fdname.Contains("germanium") || fdname.Contains("niobium") || fdname.Contains("polonium")
                || fdname.Contains("vanadium") || fdname.Contains("yttrium"));
        }

        static public CatType? CategoryFrom(string s)
        {
            if (Enum.TryParse<CatType>(s, true, out CatType res))
                return res;
            else
                return null;
        }

        public const int VeryCommonCap = 300;
        public const int CommonCap = 250;
        public const int StandardCap = 200;
        public const int RareCap = 150;
        public const int VeryRareCap = 100;

        public int? MaterialLimit()
        {
            if (Type == ItemType.VeryCommon) return VeryCommonCap;
            if (Type == ItemType.Common) return CommonCap;
            if (Type == ItemType.Standard) return StandardCap;
            if (Type == ItemType.Rare) return RareCap;
            if (Type == ItemType.VeryRare) return VeryRareCap;
            return null;
        }

        #region Static interface

        // name key is lower case normalised
        private static Dictionary<string, MaterialCommodityMicroResourceType> cachelist = null;
        private static Dictionary<string, MaterialCommodityMicroResourceType> mcmrlist = null;

        public static MaterialCommodityMicroResourceType GetByFDName(string fdname)
        {
            fdname = fdname.ToLowerInvariant();
            return cachelist.ContainsKey(fdname) ? cachelist[fdname] : null;
        }

        public static string GetNameByFDName(string fdname) // if we have it, give name, else give alt or splitcaps.  
        {
            fdname = fdname.ToLowerInvariant();
            return cachelist.ContainsKey(fdname) ? cachelist[fdname].Name : fdname.SplitCapsWordFull();
        }

        public static MaterialCommodityMicroResourceType GetByShortName(string shortname)
        {
            List<MaterialCommodityMicroResourceType> lst = cachelist.Values.ToList();
            int i = lst.FindIndex(x => x.Shortname.Equals(shortname, StringComparison.InvariantCultureIgnoreCase));
            return i >= 0 ? lst[i] : null;
        }

        public static MaterialCommodityMicroResourceType GetByEnglishName(string name)
        {
            List<MaterialCommodityMicroResourceType> lst = cachelist.Values.ToList();
            int i = lst.FindIndex(x => x.EnglishName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return i >= 0 ? lst[i] : null;
        }


        public static MaterialCommodityMicroResourceType[] GetAll()
        {
            return cachelist.Values.ToArray();
        }

        public enum SortMethod
        {
            None, Alphabetical, AlphabeticalRaresLast
        }

        // use this delegate to find them
        public static MaterialCommodityMicroResourceType[] Get(Func<MaterialCommodityMicroResourceType, bool> func, SortMethod sort)
        {
            MaterialCommodityMicroResourceType[] items = cachelist.Values.Where(func).ToArray();

            if (sort != SortMethod.None)
            {
                Array.Sort(items, delegate (MaterialCommodityMicroResourceType left, MaterialCommodityMicroResourceType right)     // in order, name
                {
                    if ( sort == SortMethod.AlphabeticalRaresLast)
                    {
                        if ( left.IsRareCommodity )
                        {
                            if (right.IsRareCommodity)
                                return left.Name.CompareTo(right.Name.ToString());
                            else
                                return 1;
                        }
                        else if ( right.IsRareCommodity )
                        {
                            if (left.IsRareCommodity)
                                return left.Name.CompareTo(right.Name.ToString());
                            else
                                return -1;
                        }
                        else
                            return left.Name.CompareTo(right.Name.ToString());
                    }
                    else
                        return left.Name.CompareTo(right.Name.ToString());
                });

            }

            return items;
        }

        public static MaterialCommodityMicroResourceType[] GetCommodities(SortMethod sorted)
        {
            return Get(x => x.IsCommodity, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetRareCommodities(SortMethod sorted)
        {
            return Get(x => x.IsRareCommodity, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetNormalCommodities(SortMethod sorted)
        {
            return Get(x => x.IsNormalCommodity, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetMaterials(SortMethod sorted)
        {
            return Get(x => x.IsMaterial, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetMicroResources(SortMethod sorted)
        {
            return Get(x => x.IsMicroResources, sorted);
        }

        public static Tuple<ItemType, string>[] GetTypes(Func<MaterialCommodityMicroResourceType, bool> func, bool sorted)        // given predate, return type/translated types combos.
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var types = mcs.Where(func).Select(x => new Tuple<ItemType, string>(x.Type, x.TranslatedType)).Distinct().ToArray();
            if (sorted)
                Array.Sort(types, delegate (Tuple<ItemType, string> l, Tuple<ItemType, string> r) { return l.Item2.CompareTo(r.Item2); });
            return types;
        }

        public static Tuple<CatType, string>[] GetCategories(Func<MaterialCommodityMicroResourceType, bool> func, bool sorted)   // given predate, return cat/translated cat combos.
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var types = mcs.Where(func).Select(x => new Tuple<CatType, string>(x.Category, x.TranslatedCategory)).Distinct().ToArray();
            if (sorted)
                Array.Sort(types, delegate (Tuple<CatType, string> l, Tuple<CatType, string> r) { return l.Item2.CompareTo(r.Item2); });
            return types;
        }

        public static MaterialCommodityMicroResourceType[] Get(Func<MaterialCommodityMicroResourceType, bool> func)   // given predate, return matching items
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var group = mcs.Where(func).Select(x => x).ToArray();
            return group;
        }

        public static string[] GetMembersOfType(ItemType typename, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var members = mcs.Where(x => x.Type == typename).Select(x => x.Name).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }

        public static string[] GetFDNameMembersOfType(ItemType typename, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            string[] members = mcs.Where(x => x.Type == typename).Select(x => x.FDName).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }


        public static string[] GetFDNameMembersOfCategory(CatType catname, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            string[] members = mcs.Where(x => x.Category == catname).Select(x => x.FDName).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }

        #endregion

        public MaterialCommodityMicroResourceType()
        {
        }

        public MaterialCommodityMicroResourceType(CatType cs, string englishtext, string fd, ItemType t, MaterialGroupType mtg, string shortn, Color cl, bool rare)
        {
            Category = cs;
            TranslatedCategory = (Category == CatType.Item) ? "Goods" : (Category == CatType.Component) ? "Assets" : Category.ToString();      // name is as the game does
            TranslatedCategory = TranslatedCategory.TxID(typeof(MaterialCommodityMicroResourceType), TranslatedCategory);        // valid to pass this thru the Tx( system
            EnglishName = Name = englishtext;
            FDName = fd;
            Type = t;
            TranslatedType = Type.ToString().SplitCapsWord().TxID(typeof(MaterialCommodityMicroResourceType), Type.ToString());                // valid to pass this thru the Tx( system
            MaterialGroup = mtg;
            TranslatedMaterialGroup = MaterialGroup.ToString().SplitCapsWordFull().TxID(typeof(MaterialCommodityMicroResourceType), MaterialGroup.ToString());                // valid to pass this thru the Tx( system
            Shortname = shortn;
            Colour = cl;
            Rarity = rare;
            //System.Diagnostics.Debug.WriteLine($"Added {FDName} {Name} {Shortname}");
        }

        private void SetCache()
        {
            try
            {
                cachelist.Add(FDName.ToLowerInvariant(), this);     // on purpose, an add to cause errors if dup
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"**** Duplicate MCD type {FDName} {ex}");
            }
        }

        public static MaterialCommodityMicroResourceType EnsurePresent(string catname, string fdname, string locname = "")  // By FDNAME
        {
            var cat = CategoryFrom(catname);
            if (cat.HasValue)
                return EnsurePresent(cat.Value, fdname, locname);
            else
                return null;
        }


        public static MaterialCommodityMicroResourceType EnsurePresent(CatType cat, string fdname, string locname = "Unavailable")  // By FDNAME
        {
            if (!cachelist.ContainsKey(fdname.ToLowerInvariant()))
            {
                MaterialCommodityMicroResourceType mcdb = new MaterialCommodityMicroResourceType(cat, fdname.SplitCapsWordFull(), fdname, ItemType.Unknown, MaterialGroupType.NA, "", Color.Green, false);
                mcdb.SetCache();

                string shortnameguess = "MR" + cat.ToString()[0];
                foreach (var c in locname)
                {
                    if (char.IsUpper(c))
                        shortnameguess += c;
                }

                System.Diagnostics.Debug.WriteLine("** Made MCMRType: {0},{1},{2},{3}", "?", fdname, cat.ToString(), locname);
                System.Diagnostics.Debug.WriteLine("** AddMicroResource(CatType.{0},\"{1}\",\"{2}\",\"{3}\");", cat.ToString(), locname, fdname, shortnameguess);
            }

            return cachelist[fdname.ToLowerInvariant()];
        }


        #region Initial setup

        static Color CByType(ItemType s)
        {
            if (s == ItemType.VeryRare)
                return Color.Red;
            if (s == ItemType.Rare)
                return Color.Yellow;
            if (s == ItemType.VeryCommon)
                return Color.Cyan;
            if (s == ItemType.Common)
                return Color.Green;
            if (s == ItemType.Standard)
                return Color.SandyBrown;
            if (s == ItemType.Unknown)
                return Color.Red;
            System.Diagnostics.Debug.Assert(false);
            return Color.Black;
        }

        // Mats

        private static bool AddRaw(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Raw, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        private static bool AddEnc(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Encoded, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        private static bool AddManu(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Manufactured, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        // Commods

        private static bool AddCommodityRare(string aliasname, ItemType typeofit, string fdname)
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, "", fdname, true);
        }

        private static bool AddCommodity(string aliasname, ItemType typeofit, string fdname)        // fdname only if not a list.
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, "", fdname);
        }

        private static bool AddCommoditySN(string aliasname, ItemType typeofit, string shortname, string fdname)
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, shortname, fdname);
        }

        // fdname only useful if aliasname is not a list.
        private static bool AddCommodityList(string aliasnamelist, ItemType typeofit)
        {
            string[] list = aliasnamelist.Split(';');

            foreach (string name in list)
            {
                if (name.Length > 0)   // just in case a semicolon slips thru
                {
                    if (!AddEntry(CatType.Commodity, Color.Green, name, typeofit, MaterialGroupType.NA, "", ""))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Odyssey microresources

        private static bool AddMicroResource(CatType cat, string locname, string fdname, string shortname)
        {
            return AddEntry(cat, Color.Green, locname, ItemType.Unknown, MaterialGroupType.NA, shortname, fdname);
        }

        // common adds

        public static string FDNameCnv(string normal)
        {
            string n = new string(normal.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
            return n;
        }

        private static bool AddEntry(CatType catname, Color colour, string aliasname, ItemType typeofit, MaterialGroupType mtg, string shortname, string fdName, bool comrare = false)
        {

            string fdn = (fdName.Length > 0) ? fdName.ToLowerInvariant() : FDNameCnv(aliasname).ToLowerInvariant();       // always lower case fdname

            MaterialCommodityMicroResourceType mc = new MaterialCommodityMicroResourceType(catname, aliasname, fdn, typeofit, mtg, shortname, colour, comrare);
            mc.SetCache();
            return true;
        }

        private static void Add(CatType catname, ItemType typeofit, MCMR id, string englishtext, bool rare = false) // p1
        {
            Add(catname, typeofit, MaterialGroupType.NA, id, englishtext, "", rare);
        }
        private static void Add(CatType catname, MCMR id, string englishtext, string shortname) // p4
        {
            Add(catname, ItemType.Unknown, id, englishtext, shortname);
        }
        private static void Add(CatType catname, ItemType typeofit, MCMR id, string englishtext, string shortname, bool rare = false) // p3
        {
            Add(catname, typeofit, MaterialGroupType.NA, id, englishtext, shortname, rare);
        }
        private static void Add(CatType catname, ItemType typeofit, MaterialGroupType mtg, MCMR id, string englishtext, string shortname, bool rare = false)
        {
            if (shortname.HasChars() && mcmrlist.Values.ToList().Find(x => x.Shortname.Equals(shortname, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                System.Diagnostics.Debug.WriteLine("**** Shortname repeat for " + id);
            }

            string fdname = id.ToString().ToLower();

            System.Diagnostics.Debug.Assert(mcmrlist.ContainsKey(fdname) == false, "Repeated entry " + fdname);

            var colour = typeofit < ItemType.Unknown ? CByType(typeofit) : Color.Green;
            MaterialCommodityMicroResourceType m = new MaterialCommodityMicroResourceType(catname, englishtext, fdname, typeofit, mtg, shortname, colour, rare);
            mcmrlist.Add(fdname,m);
        }

        public static void FillTable()
        {
            mcmrlist = new Dictionary<string, MaterialCommodityMicroResourceType>();

            #region Commodity

            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.Clothing, "Clothing");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.ConsumerTechnology, "Consumer Technology");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.DomesticAppliances, "Domestic Appliances");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.Duradrives, "Duradrives");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.EvacuationShelter, "Evacuation Shelter");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.SurvivalEquipment, "Survival Equipment");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.AgronomicTreatment, "Agronomic Treatment");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.Explosives, "Explosives");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.HydrogenFuel, "Hydrogen Fuel");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.HydrogenPeroxide, "Hydrogen Peroxide");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.LiquidOxygen, "Liquid Oxygen");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.MineralOil, "Mineral Oil");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.NerveAgents, "Nerve Agents");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.Pesticides, "Pesticides");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.RockforthFertiliser, "Rockforth Fertiliser");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.SurfaceStabilisers, "Surface Stabilisers");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.SyntheticReagents, "Synthetic Reagents");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.Tritium, "Tritium");
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.Water, "Water");
            Add(CatType.Commodity, ItemType.Drones, MCMR.Drones, "Drones");
            Add(CatType.Commodity, ItemType.Foods, MCMR.Algae, "Algae");
            Add(CatType.Commodity, ItemType.Foods, MCMR.AnimalMeat, "Animal Meat");
            Add(CatType.Commodity, ItemType.Foods, MCMR.Coffee, "Coffee");
            Add(CatType.Commodity, ItemType.Foods, MCMR.Fish, "Fish");
            Add(CatType.Commodity, ItemType.Foods, MCMR.FoodCartridges, "Food Cartridges");
            Add(CatType.Commodity, ItemType.Foods, MCMR.FruitandVegetables, "Fruit and Vegetables");
            Add(CatType.Commodity, ItemType.Foods, MCMR.Grain, "Grain");
            Add(CatType.Commodity, ItemType.Foods, MCMR.SyntheticMeat, "Synthetic Meat");
            Add(CatType.Commodity, ItemType.Foods, MCMR.Tea, "Tea");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.CeramicComposites, "Ceramic Composites");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.CMMComposite, "CMM Composite", "CMMC");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.CoolingHoses, "Micro-Weave Cooling Hoses", "MWCH");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.InsulatingMembrane, "Insulating Membrane");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.Metaalloys, "Meta-Alloys", "MA");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.NeofabricInsulation, "Neofabric Insulation", "NFI");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.Polymers, "Polymers");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.Semiconductors, "Semiconductors");
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.Superconductors, "Superconductors");
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.Beer, "Beer");
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.BootlegLiquor, "Bootleg Liquor");
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.Liquor, "Liquor");
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.Tobacco, "Tobacco");
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.Wine, "Wine");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.ArticulationMotors, "Articulation Motors", "AM");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.AtmosphericExtractors, "Atmospheric Processors");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.BuildingFabricators, "Building Fabricators");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.CropHarvesters, "Crop Harvesters");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.EmergencyPowerCells, "Emergency Power Cells");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.ExhaustManifold, "Exhaust Manifold");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.GeologicalEquipment, "Geological Equipment");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.HeatsinkInterlink, "Heatsink Interlink", "HSI");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.HeliostaticFurnaces, "Microbial Furnaces");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.HNShockMount, "HN Shock Mount", "HNSM");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.IonDistributor, "Ion Distributor", "IOD");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.MagneticEmitterCoil, "Magnetic Emitter Coil", "MEC");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.MarineSupplies, "Marine Equipment");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.MineralExtractors, "Mineral Extractors");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.ModularTerminals, "Modular Terminals");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.PowerConverter, "Power Converter", "PC");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.PowerGenerators, "Power Generators");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.PowerGridAssembly, "Energy Grid Assembly", "EGA");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.PowerTransferConduits, "Power Transfer Bus", "PTB");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.RadiationBaffle, "Radiation Baffle", "RB");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.ReinforcedMountingPlate, "Reinforced Mounting Plate", "RMP");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.SkimerComponents, "Skimmer Components");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.ThermalCoolingUnits, "Thermal Cooling Units", "TCU");
            Add(CatType.Commodity, ItemType.Machinery, MCMR.WaterPurifiers, "Water Purifiers", "WPURE");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.AdvancedMedicines, "Advanced Medicines");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.AgriculturalMedicines, "Agri-Medicines");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.BasicMedicines, "Basic Medicines");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.CombatStabilisers, "Combat Stabilisers");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.Nanomedicines, "Nanomedicines");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.PerformanceEnhancers, "Performance Enhancers");
            Add(CatType.Commodity, ItemType.Medicines, MCMR.ProgenitorCells, "Progenitor Cells");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Aluminium, "Aluminium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Beryllium, "Beryllium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Bismuth, "Bismuth");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Cobalt, "Cobalt");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Copper, "Copper");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Gallium, "Gallium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Gold, "Gold");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Hafnium178, "Hafnium 178");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Indium, "Indium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Lanthanum, "Lanthanum");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Lithium, "Lithium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Osmium, "Osmium", "OSM");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Palladium, "Palladium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Platinum, "Platinum");
            Add(CatType.Commodity, ItemType.Metals, MCMR.PlatinumAloy, "Platinum Alloy");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Praseodymium, "Praseodymium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Samarium, "Samarium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Silver, "Silver");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Tantalum, "Tantalum");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Thallium, "Thallium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Thorium, "Thorium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Titanium, "Titanium");
            Add(CatType.Commodity, ItemType.Metals, MCMR.Uranium, "Uranium");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Alexandrite, "Alexandrite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Bauxite, "Bauxite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Benitoite, "Benitoite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Bertrandite, "Bertrandite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Bromellite, "Bromellite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Coltan, "Coltan");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Cryolite, "Cryolite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Gallite, "Gallite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Goslarite, "Goslarite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Grandidierite, "Grandidierite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Indite, "Indite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Jadeite, "Jadeite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Lepidolite, "Lepidolite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.LithiumHydroxide, "Lithium Hydroxide");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.lowtemperaturediamond, "Low Temperature Diamonds");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.MethaneClathrate, "Methane Clathrate");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.methanolmonohydratecrystals, "Methanol Monohydrate Crystals");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Moissanite, "Moissanite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Monazite, "Monazite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Musgravite, "Musgravite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Opal, "Void Opal");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Painite, "Painite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Pyrophyllite, "Pyrophyllite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Rhodplumsite, "Rhodplumsite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Rutile, "Rutile");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Serendibite, "Serendibite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Taaffeite, "Taaffeite");
            Add(CatType.Commodity, ItemType.Minerals, MCMR.Uraninite, "Uraninite");
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.BasicNarcotics, "Narcotics");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AislingMediaMaterials, "Aisling Media Materials");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AislingMediaResources, "Aisling Sealed Contracts");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AislingPromotionalMaterials, "Aisling Programme Materials");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AllianceLegaslativeContracts, "Alliance Legislative Contracts");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AllianceLegaslativeRecords, "Alliance Legislative Records");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.AllianceTradeAgreements, "Alliance Trade Agreements");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.CounterCultureSupport, "Revolutionary supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.FederalAid, "Liberal Federal Aid");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.FederalTradeContracts, "Liberal Federal Packages");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.GromCounterIntelligence, "Grom Counter Intelligence");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.GromWarTrophies, "Yuri Grom's Military Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.IllicitConsignment, "Kumo Contraband Package");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.ImperialPrisoner, "Torval Political Prisoners");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.LavignyCorruptionDossiers, "Lavigny Corruption Reports");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.LavignyFieldSupplies, "Lavigny Field Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.LavignyGarisonSupplies, "Lavigny Garrison Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.LiberalCampaignMaterials, "Liberal Propaganda");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.LoanedArms, "Marked Military Arms");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.MarkedSlaves, "Marked Slaves");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.OnionheadDerivatives, "Onionhead Derivatives");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.OnionheadSamples, "Onionhead Samples");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.OutOfDateGoods, "Out Of Date Goods");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.PatreusFieldSupplies, "Patreus Field Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.PatreusGarisonSupplies, "Patreus Garrison Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.RepublicanFieldSupplies, "Hudson's Field Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.RepublicanGarisonSupplies, "Hudson Garrison Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.RestrictedIntel, "Hudson's Restricted Intel");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.RestrictedPackage, "Core Restricted Package");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.SiriusCommercialContracts, "Sirius Corporate Contracts");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.SiriusFranchisePackage, "Sirius Franchise Package");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.SiriusIndustrialEquipment, "Sirius Industrial Equipment");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.TorvalCommercialContracts, "Torval Trade Agreements");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.TorvalDeeds, "Torval Deeds");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.UndergroundSupport, "Grom Underground Support");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.UnmarkedWeapons, "Unmarked Military supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.UtopianDissident, "Utopian Dissident");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.UtopianFieldSupplies, "Utopian Supplies");
            Add(CatType.Commodity, ItemType.PowerPlay, MCMR.UtopianPublicity, "Utopian Publicity");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AiRelics, "Ai Relics");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientCasket, "Guardian Casket");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientKey, "Ancient Key");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientOrb, "Guardian Orb");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientRelic, "Guardian Relic");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientRelicTG, "Unclassified Relic", "ARTG");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientTablet, "Guardian Tablet");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientTotem, "Guardian Totem");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AncientUrn, "Guardian Urn");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AntimatterContainmentUnit, "Antimatter Containment Unit");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AntiqueJewellery, "Antique Jewellery");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.Antiquities, "Antiquities");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.AssaultPlans, "Assault Plans");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ComercialSamples, "Commercial Samples");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.CoralSap, "Coral Sap");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.DamagedEscapePod, "Damaged Escape Pod");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.DataCore, "Data Core");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.DiplomaticBag, "Diplomatic Bag");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.EncriptedDataStorage, "Encrypted Data Storage");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.EncryptedCorrespondence, "Encrypted Correspondence");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.FossilRemnants, "Fossil Remnants");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.GeneBank, "Gene Bank");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.GeologicalSamples, "Geological Samples");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.Hostage, "Hostages");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.LargeExplorationDatacash, "Large Survey Data Cache");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M_TissueSample_Fluid, "Mollusc Fluid");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M_TissueSample_Nerves, "Mollusc Brain Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M_TissueSample_Soft, "Mollusc Soft Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M3_TissueSample_Membrane, "Mollusc Membrane");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M3_TissueSample_Mycelium, "Mollusc Mycelium");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.M3_TissueSample_Spores, "Mollusc Spores");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.MilitaryIntelligence, "Military Intelligence");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.MysteriousIdol, "Mysterious Idol");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.OccupiedCryoPod, "Occupied Escape Pod");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.P_ParticulateSample, "Anomaly Particles");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.PersonalEffects, "Personal Effects");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.PoliticalPrisoner, "Political Prisoners");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.PreciousGems, "Precious Gems");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ProhibitedResearchMaterials, "Prohibited Research Materials");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S_TissueSample_Cells, "Pod Core Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S_TissueSample_Core, "Pod Surface Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S_TissueSample_Surface, "Pod Dead Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S6_TissueSample_Cells, "Pod Outer Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S6_TissueSample_Coenosarc, "Pod Shell Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S6_TissueSample_Mesoglea, "Pod Mesoglea");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.S9_TissueSample_Shell, "Pod Tissue");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.Sap8CoreContainer, "Sap 8 Core Container");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ScientificResearch, "Scientific Research");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ScientificSamples, "Scientific Samples");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.SmallExplorationDatacash, "Small Survey Data Cache");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.SpacePioneerRelics, "Space Pioneer Relics");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.TacticalData, "Tactical Data");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidGeneratorTissueSample, "Caustic Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidHeart, "Thargoid Heart");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidPod, "Xenobiological Prison Pod");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidScoutTissueSample, "Thargoid Scout Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType1, "Thargoid Cyclops Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType10a, "Titan Maw Deep Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType10b, "Titan Maw Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType10c, "Titan Maw Partial Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType2, "Thargoid Basilisk Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType3, "Thargoid Medusa Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType4, "Thargoid Hydra Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType5, "Thargoid Orthrus Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType6, "Thargoid Glaive Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType7, "Thargoid Scythe Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType9a, "Titan Deep Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType9b, "Titan Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ThargoidTissueSampleType9c, "Titan Partial Tissue Sample");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.TimeCapsule, "Time Capsule");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.TrinketsOfFortune, "Trinkets of Hidden Fortune");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownArtifact, "Thargoid Sensor");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownArtifact2, "Thargoid Probe");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownArtifact3, "Thargoid Link");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownBiologicalMatter, "Thargoid Biological Matter");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownResin, "Thargoid Resin");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownSack, "Protective Membrane Scrap");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnknownTechnologySamples, "Thargoid Technology Samples");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnocuppiedEscapePod, "Unoccupied Escape Pod");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.UnstableDataCore, "Unstable Data Core");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoAncientArtefact, "Ancient Artefact");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoBlackBox, "Black Box");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoExperimentalChemicals, "Experimental Chemicals");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoMilitaryPlans, "Military Plans");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoPrototypeTech, "Prototype Tech");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoRareArtwork, "Rare Artwork");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoRebelTransmissions, "Rebel Transmissions");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoTechnicalBlueprints, "Technical Blueprints");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.USSCargoTradeData, "Trade Data");
            Add(CatType.Commodity, ItemType.Salvage, MCMR.WreckageComponents, "Wreckage Components");
            Add(CatType.Commodity, ItemType.Slaves, MCMR.ImperialSlaves, "Imperial Slaves");
            Add(CatType.Commodity, ItemType.Slaves, MCMR.Slaves, "Slaves");
            Add(CatType.Commodity, ItemType.Technology, MCMR.AdvancedCatalysers, "Advanced Catalysers");
            Add(CatType.Commodity, ItemType.Technology, MCMR.AnimalMonitors, "Animal Monitors");
            Add(CatType.Commodity, ItemType.Technology, MCMR.AquaponicSystems, "Aquaponic Systems");
            Add(CatType.Commodity, ItemType.Technology, MCMR.autofabricators, "Auto-Fabricators");
            Add(CatType.Commodity, ItemType.Technology, MCMR.BioreducingLichen, "Bioreducing Lichen");
            Add(CatType.Commodity, ItemType.Technology, MCMR.ComputerComponents, "Computer Components");
            Add(CatType.Commodity, ItemType.Technology, MCMR.DiagnosticSensor, "Hardware Diagnostic Sensor", "DIS");
            Add(CatType.Commodity, ItemType.Technology, MCMR.HazardousEnvironmentSuits, "H.E. Suits");
            Add(CatType.Commodity, ItemType.Technology, MCMR.MedicalDiagnosticEquipment, "Medical Diagnostic Equipment");
            Add(CatType.Commodity, ItemType.Technology, MCMR.MicroControllers, "Micro Controllers", "MCC");
            Add(CatType.Commodity, ItemType.Technology, MCMR.MutomImager, "Muon Imager");
            Add(CatType.Commodity, ItemType.Technology, MCMR.Nanobreakers, "Nanobreakers");
            Add(CatType.Commodity, ItemType.Technology, MCMR.ResonatingSeparators, "Resonating Separators");
            Add(CatType.Commodity, ItemType.Technology, MCMR.Robotics, "Robotics");
            Add(CatType.Commodity, ItemType.Technology, MCMR.StructuralRegulators, "Structural Regulators");
            Add(CatType.Commodity, ItemType.Technology, MCMR.TelemetrySuite, "Telemetry Suite");
            Add(CatType.Commodity, ItemType.Technology, MCMR.TerrainEnrichmentSystems, "Land Enrichment Systems");
            Add(CatType.Commodity, ItemType.Textiles, MCMR.ConductiveFabrics, "Conductive Fabrics");
            Add(CatType.Commodity, ItemType.Textiles, MCMR.Leather, "Leather");
            Add(CatType.Commodity, ItemType.Textiles, MCMR.MilitaryGradeFabrics, "Military Grade Fabrics");
            Add(CatType.Commodity, ItemType.Textiles, MCMR.NaturalFabrics, "Natural Fabrics");
            Add(CatType.Commodity, ItemType.Textiles, MCMR.SyntheticFabrics, "Synthetic Fabrics");
            Add(CatType.Commodity, ItemType.Waste, MCMR.Biowaste, "Biowaste");
            Add(CatType.Commodity, ItemType.Waste, MCMR.ChemicalWaste, "Chemical Waste");
            Add(CatType.Commodity, ItemType.Waste, MCMR.Scrap, "Scrap");
            Add(CatType.Commodity, ItemType.Waste, MCMR.ToxicWaste, "Toxic Waste");
            Add(CatType.Commodity, ItemType.Weapons, MCMR.BattleWeapons, "Battle Weapons");
            Add(CatType.Commodity, ItemType.Weapons, MCMR.Landmines, "Landmines");
            Add(CatType.Commodity, ItemType.Weapons, MCMR.NonLethalWeapons, "Non-Lethal Weapons");
            Add(CatType.Commodity, ItemType.Weapons, MCMR.PersonalWeapons, "Personal Weapons");
            Add(CatType.Commodity, ItemType.Weapons, MCMR.ReactiveArmour, "Reactive Armour");
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.Advert1, "Ultra-Compact Processor Prototypes", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.AlacarakmoSkinArt, "Alacarakmo Skin Art", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.AlienEggs, "Leathery Eggs", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.AltairianSkin, "Altairian Skin", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.BuckyballBeerMats, "Buckyball Beer Mats", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.CrystallineSpheres, "Crystalline Spheres", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.EleuThermals, "Eleu Thermals", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.EshuUmbrellas, "Eshu Umbrellas", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.HavasupaiDreamCatcher, "Havasupai Dream Catcher", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.JaquesQuinentianStill, "Jaques Quinentian Still", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.JaradharrePuzzlebox, "Jaradharre Puzzle Box", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.JotunMookah, "Jotun Mookah", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.KaretiiCouture, "Karetii Couture", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.KinagoInstruments, "Kinago Violins", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.MomusBogSpaniel, "Momus Bog Spaniel", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.NgunaModernAntiques, "Nguna Modern Antiques", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.NjangariSaddles, "Njangari Saddles", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.OphiuchiExinoArtefacts, "Ophiuch Exino Artefacts", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.PersonalGifts, "Personal Gifts", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.RajukruStoves, "Rajukru Multi-Stoves", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.ShansCharisOrchid, "Shan's Charis Orchid", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.SoontillRelics, "Soontill Relics", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.TheHuttonMug, "The Hutton Mug", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.TiolceWaste2PasteUnits, "Tiolce Waste2Paste Units", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.UzumokuLowGWings, "Uzumoku Low-G Wings", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.VidavantianLace, "Vidavantian Lace", true);
            Add(CatType.Commodity, ItemType.ConsumerItems, MCMR.ZeesszeAntGlue, "Zeessze Ant Grub Glue", true);
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.AnduligaFireWorks, "Anduliga Fire Works", true);
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.DeltaPhoenicisPalms, "Delta Phoenicis Palms", true);
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.HIPOrganophosphates, "Hip Organophosphates", true);
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.KorroKungPellets, "Koro Kung Pellets", true);
            Add(CatType.Commodity, ItemType.Chemicals, MCMR.ToxandjiVirocide, "Toxandji Virocide", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.AerialEdenApple, "Eden Apples Of Aerial", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.AlbinoQuechuaMammoth, "Albino Quechua Mammoth Meat", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.AnyNaCoffee, "Any Na Coffee", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.AroucaConventualSweets, "Arouca Conventual Sweets", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.BakedGreebles, "Baked Greebles", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.BaltahSineVacuumKrill, "Baltah'sine Vacuum Krill", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.CD75CatCoffee, "CD-75 Kitten Brand Coffee", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.CeremonialHeikeTea, "Ceremonial Heike Tea", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.CetiAepyornisEgg, "Aepyornis Egg", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.CetiRabbits, "Ceti Rabbits", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.ChiEridaniMarinePaste, "Chi Eridani Marine Paste", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.CoquimSpongiformVictuals, "Coquim Spongiform Victuals", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.DeuringasTruffles, "Deuringas Truffles", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.DisoMaCorn, "Diso Ma Corn", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.EsusekuCaviar, "Esuseku Caviar", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.EthgrezeTeaBuds, "Ethgreze Tea Buds", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.GiantIrukamaSnails, "Giant Irukama Snails", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.GomanYauponCoffee, "Goman Yaupon Coffee", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.HaidneBlackBrew, "Haiden Black Brew", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.HelvetitjPearls, "Helvetitj Pearls", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.HIP10175BushMeat, "HIP 10175 Bush Meat", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.HIP41181Squid, "HIP Proto-Squid", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.HR7221Wheat, "HR 7221 Wheat", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.JarouaRice, "Jaroua Rice", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.KarsukiLocusts, "Karsuki Locusts", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.LFTVoidExtractCoffee, "Void Extract Coffee", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.LiveHecateSeaWorms, "Live Hecate Sea Worms", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.LTTHyperSweet, "LTT Hyper Sweet", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.MechucosHighTea, "Mechucos High Tea", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.MokojingBeastFeast, "Mokojing Beast Feast", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.MukusubiiChitinOs, "Mukusubii Chitin-os", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.MulachiGiantFungus, "Mulachi Giant Fungus", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.NeritusBerries, "Neritus Berries", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.OchoengChillies, "Ochoeng Chillies", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.OrrerianViciousBrew, "Orrerian Vicious Brew", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.SanumaMEAT, "Sanuma Decorative Meat", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.TanmarkTranquilTea, "Tanmark Tranquil Tea", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.UszaianTreeGrub, "Uszaian Tree Grub", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.UtgaroarMillenialEggs, "Utgaroar Millennial Eggs", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.WheemeteWheatCakes, "Wheemete Wheat Cakes", true);
            Add(CatType.Commodity, ItemType.Foods, MCMR.WitchhaulKobeBeef, "Witchhaul Kobe Beef", true);
            Add(CatType.Commodity, ItemType.IndustrialMaterials, MCMR.MedbStarlube, "Medb Starlube", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.BastSnakeGin, "Bast Snake Gin", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.BlueMilk, "Azure Milk", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.BurnhamBileDistillate, "Burnham Bile Distillate", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.CentauriMegaGin, "Centauri Mega Gin", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.ChateauDeAegaeon, "Chateau De Aegaeon", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.EraninPearlWhisky, "Eranin Pearl Whisky", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.GerasianGueuzeBeer, "Gerasian Gueuze Beer", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.IndiBourbon, "Indi Bourbon", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.KamitraCigars, "Kamitra Cigars", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.KonggaAle, "Kongga Ale", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.LavianBrandy, "Lavian Brandy", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.LeestianEvilJuice, "Leestian Evil Juice", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.RusaniOldSmokey, "Rusani Old Smokey", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.SaxonWine, "Saxon Wine", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.ThrutisCream, "Thrutis Cream", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.WuthieloKuFroth, "Wuthielo Ku Froth", true);
            Add(CatType.Commodity, ItemType.LegalDrugs, MCMR.YasoKondiLeaf, "Yaso Kondi Leaf", true);
            Add(CatType.Commodity, ItemType.Machinery, MCMR.GiantVerrix, "Giant Verrix", true);
            Add(CatType.Commodity, ItemType.Machinery, MCMR.NonEuclidianExotanks, "Non Euclidian Exotanks", true);
            Add(CatType.Commodity, ItemType.Machinery, MCMR.VolkhabBeeDrones, "Volkhab Bee Drones", true);
            Add(CatType.Commodity, ItemType.Machinery, MCMR.WulpaHyperboreSystems, "Wulpa Hyperbore Systems", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.AganippeRush, "Aganippe Rush", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.AlyaBodilySoap, "Alya Body Soap", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.FujinTea, "Fujin Tea", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.HonestyPills, "Honesty Pills", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.KachiriginLeaches, "Kachirigin Filter Leeches", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.PantaaPrayerSticks, "Pantaa Prayer Sticks", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.TauriChimes, "Tauri Chimes", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.TerraMaterBloodBores, "Terra Mater Blood Bores", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.VegaSlimWeed, "Vega Slimweed", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.VHerculisBodyRub, "V Herculis Body Rub", true);
            Add(CatType.Commodity, ItemType.Medicines, MCMR.WatersOfShintara, "The Waters Of Shintara", true);
            Add(CatType.Commodity, ItemType.Metals, MCMR.SothisCrystallineGold, "Sothis Crystalline Gold", true);
            Add(CatType.Commodity, ItemType.Minerals, MCMR.CherbonesBloodCrystals, "Cherbones Blood Crystals", true);
            Add(CatType.Commodity, ItemType.Minerals, MCMR.NgadandariFireOpals, "Ngadandari Fire Opals", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.AnimalEffigies, "Crom Silver Fesh", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.ApaVietii, "Apa Vietii", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.GeawenDanceDust, "Geawen Dance Dust", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.HarmaSilverSeaRum, "Harma Silver Sea Rum", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.LyraeWeed, "Lyrae Weed", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.MotronaExperienceJelly, "Motrona Experience Jelly", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.OnionHead, "Onionhead", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.OnionHeadA, "Onionhead Alpha Strain", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.OnionHeadB, "Onionhead Beta Strain", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.OnionHeadC, "Onionhead Gamma Strain", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.PavonisEarGrubs, "Pavonis Ear Grubs", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.TarachTorSpice, "Tarach Spice", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.TransgenicOnionHead, "Lucan Onionhead", true);
            Add(CatType.Commodity, ItemType.Narcotics, MCMR.Wolf1301Fesh, "Wolf Fesh", true);
            Add(CatType.Commodity, ItemType.Salvage, MCMR.ClassifiedExperimentalEquipment, "Classified Experimental Equipment", true);
            Add(CatType.Commodity, ItemType.Salvage, MCMR.EarthRelics, "Earth Relics", true);
            Add(CatType.Commodity, ItemType.Salvage, MCMR.GalacticTravelGuide, "Galactic Travel Guide", true);
            Add(CatType.Commodity, ItemType.Slaves, MCMR.MasterChefs, "Master Chefs", true);
            Add(CatType.Commodity, ItemType.Technology, MCMR.AZCancriFormula42, "AZ Cancri Formula 42", true);
            Add(CatType.Commodity, ItemType.Technology, MCMR.XiheCompanions, "Xihe Biomorphic Companions", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.BankiAmphibiousLeather, "Banki Amphibious Leather", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.BelalansRayLeather, "Belalans Ray Leather", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.ChameleonCloth, "Chameleon Cloth", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.DamnaCarapaces, "Damna Carapaces", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.RapaBaoSnakeSkins, "Rapa Bao Snake Skins", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.TiegfriesSynthSilk, "Tiegfries Synth Silk", true);
            Add(CatType.Commodity, ItemType.Textiles, MCMR.VanayequiRhinoFur, "Vanayequi Ceratomorpha Fur", true);
            Add(CatType.Commodity, ItemType.Weapons, MCMR.BorasetaniPathogenetics, "Borasetani Pathogenetics", true);
            Add(CatType.Commodity, ItemType.Weapons, MCMR.GilyaSignatureWeapons, "Gilya Signature Weapons", true);
            Add(CatType.Commodity, ItemType.Weapons, MCMR.HIP118311Swarm, "HIP 118311 Swarm", true);
            Add(CatType.Commodity, ItemType.Weapons, MCMR.HolvaDuellingBlades, "Holva Duelling Blades", true);
            Add(CatType.Commodity, ItemType.Weapons, MCMR.KamorinHistoricWeapons, "Kamorin Historic Weapons", true);

            #endregion

            #region Raw Data - keep in this order

            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory1, MCMR.Carbon, "Carbon", "C");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory2, MCMR.Phosphorus, "Phosphorus", "P");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory3, MCMR.Sulphur, "Sulphur", "S");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory4, MCMR.Iron, "Iron", "Fe");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory5, MCMR.Nickel, "Nickel", "Ni");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory6, MCMR.Rhenium, "Rhenium", "Re");
            Add(CatType.Raw, ItemType.VeryCommon, MaterialGroupType.RawCategory7, MCMR.Lead, "Lead", "Pb");

            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory1, MCMR.Vanadium, "Vanadium", "V");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory2, MCMR.Chromium, "Chromium", "Cr");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory3, MCMR.Manganese, "Manganese", "Mn");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory4, MCMR.Zinc, "Zinc", "Zn");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory5, MCMR.Germanium, "Germanium", "Ge");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory6, MCMR.Arsenic, "Arsenic", "As");
            Add(CatType.Raw, ItemType.Common, MaterialGroupType.RawCategory7, MCMR.Zirconium, "Zirconium", "Zr");

            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory1, MCMR.Niobium, "Niobium", "Nb");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory2, MCMR.Molybdenum, "Molybdenum", "Mo");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory3, MCMR.Cadmium, "Cadmium", "Cd");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory4, MCMR.Tin, "Tin", "Sn");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory5, MCMR.Tungsten, "Tungsten", "W");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory6, MCMR.Mercury, "Mercury", "Hg");
            Add(CatType.Raw, ItemType.Standard, MaterialGroupType.RawCategory7, MCMR.Boron, "Boron", "B");

            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory1, MCMR.Yttrium, "Yttrium", "Y");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory2, MCMR.Technetium, "Technetium", "Tc");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory3, MCMR.Ruthenium, "Ruthenium", "Ru");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory4, MCMR.Selenium, "Selenium", "Se");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory5, MCMR.Tellurium, "Tellurium", "Te");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory6, MCMR.Polonium, "Polonium", "Po");
            Add(CatType.Raw, ItemType.Rare, MaterialGroupType.RawCategory7, MCMR.Antimony, "Antimony", "Sb");

            #endregion

            // TBD materialtrader does it still work?

            #region Encoded - keep in this order for the Materialtrader.cs. 

            // the order is 1/3 ratio, the first is the cheapest, the last is the most expensive
            // so for a particular type (say EncodedEmissionsData), it will pick the verycommon first, common.. etc to veryrare
            // see the code around 190 in materialtrader.cs - it wants it in verycommon - > veryrare order for a paricualt MaterialGroupType.

            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedEmissionData, MCMR.ScrambledeMissionData, "Exceptional Scrambled Emission Data", "ESED");
            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedWakeScans, MCMR.DisruptedWakeEchoes, "Atypical Disrupted Wake Echoes", "ADWE");
            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedShieldData, MCMR.ShieldCycleRecordings, "Distorted Shield Cycle Recordings", "DSCR");
            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedEncryptionFiles, MCMR.EncryptedFiles, "Unusual Encrypted Files", "UEF");
            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedDataArchives, MCMR.BulkScandata, "Anomalous Bulk Scan Data", "ABSD");
            Add(CatType.Encoded, ItemType.VeryCommon, MaterialGroupType.EncodedFirmware, MCMR.LegacyFirmware, "Specialised Legacy Firmware", "SLF");

            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedEmissionData, MCMR.ArchivedEmissionData, "Irregular Emission Data", "IED");
            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedWakeScans, MCMR.FSDTelemetry, "Anomalous FSD Telemetry", "AFT");
            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedShieldData, MCMR.ShieldSoakAnalysis, "Inconsistent Shield Soak Analysis", "ISSA");
            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedEncryptionFiles, MCMR.EncryptionCodes, "Tagged Encryption Codes", "TEC");
            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedDataArchives, MCMR.ScanArchives, "Unidentified Scan Archives", "USA");
            Add(CatType.Encoded, ItemType.Common, MaterialGroupType.EncodedFirmware, MCMR.ConsumerFirmware, "Modified Consumer Firmware", "MCF");

            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedEmissionData, MCMR.EmissionData, "Unexpected Emission Data", "UED");
            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedWakeScans, MCMR.WakeSolutions, "Strange Wake Solutions", "SWS");
            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedShieldData, MCMR.ShieldDensityReports, "Untypical Shield Scans", "USS");
            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedEncryptionFiles, MCMR.SymmetricKeys, "Open Symmetric Keys", "OSK");
            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedDataArchives, MCMR.ScanDataNanks, "Classified Scan Databanks", "CSD");
            Add(CatType.Encoded, ItemType.Standard, MaterialGroupType.EncodedFirmware, MCMR.IndustrialFirmware, "Cracked Industrial Firmware", "CIF");

            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedEmissionData, MCMR.DecodedEmissionData, "Decoded Emission Data", "DED");
            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedWakeScans, MCMR.HyperspaceTrajectories, "Eccentric Hyperspace Trajectories", "EHT");
            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedShieldData, MCMR.ShieldPatternAnalysis, "Aberrant Shield Pattern Analysis", "ASPA");
            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedEncryptionFiles, MCMR.EncryptionArchives, "Atypical Encryption Archives", "AEA");
            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedDataArchives, MCMR.EncodedScandata, "Divergent Scan Data", "DSD");
            Add(CatType.Encoded, ItemType.Rare, MaterialGroupType.EncodedFirmware, MCMR.SecurityFirmware, "Security Firmware Patch", "SFP");

            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedEmissionData, MCMR.CompactEmissionsData, "Abnormal Compact Emissions Data", "CED");
            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedWakeScans, MCMR.DataminedWake, "Datamined Wake Exceptions", "DWEx");
            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedShieldData, MCMR.ShieldFrequencyData, "Peculiar Shield Frequency Data", "PSFD");
            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedEncryptionFiles, MCMR.AdaptiveEncryptors, "Adaptive Encryptors Capture", "AEC");
            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedDataArchives, MCMR.ClassifiedScandata, "Classified Scan Fragment", "CFSD");
            Add(CatType.Encoded, ItemType.VeryRare, MaterialGroupType.EncodedFirmware, MCMR.EmbeddedFirmware, "Modified Embedded Firmware", "EFW");

            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedChemical, MCMR.ChemicalStorageUnits, "Chemical Storage Units", "CSU");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedThermic, MCMR.TemperedAlloys, "Tempered Alloys", "TeA");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedHeat, MCMR.HeatConductionWiring, "Heat Conduction Wiring", "HCW");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedConductive, MCMR.BasicConductors, "Basic Conductors", "BaC");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedMechanicalComponents, MCMR.MechanicalScrap, "Mechanical Scrap", "MS");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedCapacitors, MCMR.GridResistors, "Grid Resistors", "GR");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedShielding, MCMR.WornShieldEmitters, "Worn Shield Emitters", "WSE");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedComposite, MCMR.CompactComposites, "Compact Composites", "CC");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedCrystals, MCMR.CrystalShards, "Crystal Shards", "CS");
            Add(CatType.Manufactured, ItemType.VeryCommon, MaterialGroupType.ManufacturedAlloys, MCMR.SalvagedAlloys, "Salvaged Alloys", "SAll");

            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedChemical, MCMR.ChemicalProcessors, "Chemical Processors", "CP");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedThermic, MCMR.HeatResistantCeramics, "Heat Resistant Ceramics", "HRC");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedHeat, MCMR.HeatDispersionPlate, "Heat Dispersion Plate", "HDP");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedConductive, MCMR.ConductiveComponents, "Conductive Components", "CCo");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedMechanicalComponents, MCMR.MechanicalEquipment, "Mechanical Equipment", "ME");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedCapacitors, MCMR.HybridCapacitors, "Hybrid Capacitors", "HC");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedShielding, MCMR.ShieldEmitters, "Shield Emitters", "SHE");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedComposite, MCMR.FilamentComposites, "Filament Composites", "FiC");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedCrystals, MCMR.UncutFocusCrystals, "Flawed Focus Crystals", "FFC");
            Add(CatType.Manufactured, ItemType.Common, MaterialGroupType.ManufacturedAlloys, MCMR.GalvanisingAlloys, "Galvanising Alloys", "GA");

            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedChemical, MCMR.ChemicalDistillery, "Chemical Distillery", "CHD");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedThermic, MCMR.PrecipitatedAlloys, "Precipitated Alloys", "PAll");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedHeat, MCMR.HeatExchangers, "Heat Exchangers", "HE");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedConductive, MCMR.ConductiveCeramics, "Conductive Ceramics", "CCe");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedMechanicalComponents, MCMR.MechanicalComponents, "Mechanical Components", "MC");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedCapacitors, MCMR.ElectrochemicalArrays, "Electrochemical Arrays", "EA");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedShielding, MCMR.ShieldingSensors, "Shielding Sensors", "SS");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedComposite, MCMR.HighDensityComposites, "High Density Composites", "HDC");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedCrystals, MCMR.FocusCrystals, "Focus Crystals", "FoC");
            Add(CatType.Manufactured, ItemType.Standard, MaterialGroupType.ManufacturedAlloys, MCMR.PhaseAlloys, "Phase Alloys", "PA");

            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedChemical, MCMR.ChemicalManipulators, "Chemical Manipulators", "CM");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedThermic, MCMR.ThermicAlloys, "Thermic Alloys", "ThA");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedHeat, MCMR.HeatVanes, "Heat Vanes", "HV");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedConductive, MCMR.ConductivePolymers, "Conductive Polymers", "CPo");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedMechanicalComponents, MCMR.ConfigurableComponents, "Configurable Components", "CCom");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedCapacitors, MCMR.PolymerCapacitors, "Polymer Capacitors", "PCa");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedShielding, MCMR.CompoundShielding, "Compound Shielding", "CoS");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedComposite, MCMR.FedProprietaryComposites, "Proprietary Composites", "FPC");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedCrystals, MCMR.RefinedFocusCrystals, "Refined Focus Crystals", "RFC");
            Add(CatType.Manufactured, ItemType.Rare, MaterialGroupType.ManufacturedAlloys, MCMR.ProtoLightAlloys, "Proto Light Alloys", "PLA");

            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedChemical, MCMR.PharmaceuticalIsolators, "Pharmaceutical Isolators", "PI");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedThermic, MCMR.MilitaryGradeAlloys, "Military Grade Alloys", "MGA");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedHeat, MCMR.ProtoHeatRadiators, "Proto Heat Radiators", "PHR");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedConductive, MCMR.BiotechConductors, "Biotech Conductors", "BiC");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedMechanicalComponents, MCMR.ImprovisedComponents, "Improvised Components", "IC");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedCapacitors, MCMR.MilitarySupercapacitors, "Military Supercapacitors", "MSC");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedShielding, MCMR.ImperialShielding, "Imperial Shielding", "IS");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedComposite, MCMR.FedCoreComposites, "Core Dynamics Composites", "FCC");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedCrystals, MCMR.ExquisiteFocusCrystals, "Exquisite Focus Crystals", "EFC");
            Add(CatType.Manufactured, ItemType.VeryRare, MaterialGroupType.ManufacturedAlloys, MCMR.ProtoRadiolicAlloys, "Proto Radiolic Alloys", "PRA");

            #endregion

            #region Other Encoded

            Add(CatType.Encoded, ItemType.VeryRare, MCMR.Guardian_VesselBlueprint, "Guardian Vessel Blueprint Fragment", "GMVB");
            Add(CatType.Encoded, ItemType.Rare, MCMR.AncientBiologicalData, "Pattern Alpha Obelisk Data", "PAOD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.AncientCulturalData, "Pattern Beta Obelisk Data", "PBOD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.AncientHistoricalData, "Pattern Gamma Obelisk Data", "PGOD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.AncientLanguageData, "Pattern Delta Obelisk Data", "PDOD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.AncientTechnologicalData, "Pattern Epsilon Obelisk Data", "PEOD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.Guardian_ModuleBlueprint, "Guardian Module Blueprint Fragment", "GMBS");
            Add(CatType.Encoded, ItemType.Rare, MCMR.Guardian_WeaponBlueprint, "Guardian Weapon Blueprint Fragment", "GWBS");
            Add(CatType.Encoded, ItemType.Rare, MCMR.TG_ResidueData, "Thargoid Residue Data", "URDA");
            Add(CatType.Encoded, ItemType.Rare, MCMR.TG_ShipSystemsData, "Ship Systems Data", "SSD");
            Add(CatType.Encoded, ItemType.Rare, MCMR.UnknownWakeData, "Thargoid Wake Data", "UWD");
            Add(CatType.Encoded, ItemType.Common, MCMR.TG_StructuralData, "Thargoid Structural Data", "UKSD");
            Add(CatType.Encoded, ItemType.Standard, MCMR.TG_CompositionData, "Thargoid Material Composition Data", "UMCD");
            Add(CatType.Encoded, ItemType.Standard, MCMR.TG_ShipFlightData, "Ship Flight Data", "SFD");
            Add(CatType.Encoded, ItemType.Standard, MCMR.UnknownShipSignature, "Thargoid Ship Signature", "USSig");

            #endregion

            #region Other Manufactured

            Add(CatType.Manufactured, ItemType.VeryCommon, MCMR.Guardian_PowerCell, "Guardian Power Cell", "GPCe");
            Add(CatType.Manufactured, ItemType.VeryCommon, MCMR.Guardian_Sentinel_WreckageComponents, "Guardian Wreckage Components", "GSWC");
            Add(CatType.Manufactured, ItemType.VeryCommon, MCMR.TG_Abrasion03, "Hardened Surface Fragments", "HSF");
            Add(CatType.Manufactured, ItemType.Common, MCMR.Guardian_PowerConduit, "Guardian Power Conduit", "GPC");
            Add(CatType.Manufactured, ItemType.Common, MCMR.UnknownCarapace, "Thargoid Carapace", "UKCP");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.Guardian_Sentinel_WeaponParts, "Guardian Sentinel Weapon Parts", "GSWP");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.Guardian_TechComponent, "Guardian Technology Component", "GTC");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_Abrasion02, "Phasing Membrane Residue", "PMR");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_BioMechanicalConduits, "Bio-Mechanical Conduits", "BMC");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_CausticGeneratorParts, "Corrosive Mechanisms", "COMEC");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_CausticShard, "Caustic Shard", "CASH");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_InterdictionData, "Thargoid Interdiction Telemetry", "TIT");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.TG_ShutdownData, "Massive Energy Surge Analytics", "MESA");
            Add(CatType.Manufactured, ItemType.Standard, MCMR.UnknownEnergycell, "Thargoid Energy Cell", "UKEC");
            Add(CatType.Manufactured, ItemType.Rare, MCMR.TG_CausticCrystal, "Caustic Crystal", "CACR");
            Add(CatType.Manufactured, ItemType.Rare, MCMR.TG_WeaponParts, "Weapon Parts", "WP");
            Add(CatType.Manufactured, ItemType.Rare, MCMR.UnknownTechnologyComponents, "Thargoid Technological Components", "UKTC");
            Add(CatType.Manufactured, ItemType.VeryRare, MCMR.HeatExposureSpecimen, "Heat Exposure Specimen", "TG_Abrasion01");
            Add(CatType.Manufactured, ItemType.VeryRare, MCMR.TG_PropulsionElement, "Propulsion Elements", "PE");
            Add(CatType.Manufactured, ItemType.VeryRare, MCMR.Unknownenergysource, "Sensor Fragment", "UES");
            Add(CatType.Manufactured, ItemType.VeryRare, MCMR.UnknownOrganicCircuitry, "Thargoid Organic Circuitry", "UKOC");

            #endregion

            #region MR

            Add(CatType.Item, MCMR.AgriculturalProcessSample, "Agricultural Process Sample", "MRIAPS");
            Add(CatType.Item, MCMR.BiochemicalAgent, "Biochemical Agent", "MRIBA");
            Add(CatType.Item, MCMR.BuildingSchematic, "Building Schematic", "MRIBS");
            Add(CatType.Item, MCMR.Californium, "Californium", "MRIC");
            Add(CatType.Item, MCMR.CastFossil, "Cast Fossil", "MRICF");
            Add(CatType.Item, MCMR.ChemicalProcessSample, "Chemical Process Sample", "MRICPS");
            Add(CatType.Item, MCMR.ChemicalSample, "Chemical Sample", "MRICS");
            Add(CatType.Item, MCMR.CompactLibrary, "Compact Library", "MRICL");
            Add(CatType.Item, MCMR.CompressionLiquefiedGas, "Compression-Liquefied Gas", "MRICLG");
            Add(CatType.Item, MCMR.DeepMantleSample, "Deep Mantle Sample", "MRIDMS");
            Add(CatType.Item, MCMR.DegradedPowerRegulator, "Degraded Power Regulator", "MRIDPR");
            Add(CatType.Item, MCMR.GeneticRepairMeds, "Genetic Repair Meds", "MRIGRM");
            Add(CatType.Item, MCMR.GeneticSample, "Biological Sample", "MRIBIOSAMP");
            Add(CatType.Item, MCMR.GMeds, "G-Meds", "MRIGM");
            Add(CatType.Item, MCMR.HealthMonitor, "Health Monitor", "MRIHM");
            Add(CatType.Item, MCMR.Hush, "Hush", "MRIH");
            Add(CatType.Item, MCMR.InertiaCanister, "Inertia Canister", "MRIIC");
            Add(CatType.Item, MCMR.infinity, "Infinity", "MRII");
            Add(CatType.Item, MCMR.InorganicContaminant, "Inorganic Contaminant", "MRIINORGANC");
            Add(CatType.Item, MCMR.insight, "Insight", "MRInsight");
            Add(CatType.Item, MCMR.InsightDataBank, "Insight Data Bank", "MRIIDB");
            Add(CatType.Item, MCMR.InsightEntertainmentSuite, "Insight Entertainment Suite", "MRIIES");
            Add(CatType.Item, MCMR.IonisedGas, "Ionised Gas", "MRIIG");
            Add(CatType.Item, MCMR.LargeCapacityPowerRegulator, "Power Regulator", "MRIPR");
            Add(CatType.Item, MCMR.Lazarus, "Lazarus", "MRIL");
            Add(CatType.Item, MCMR.MicrobialInhibitor, "Microbial Inhibitor", "MRIMI");
            Add(CatType.Item, MCMR.MutagenicCatalyst, "Mutagenic Catalyst", "MRIMC");
            Add(CatType.Item, MCMR.NutritionalConcentrate, "Nutritional Concentrate", "MRINC");
            Add(CatType.Item, MCMR.PersonalComputer, "Personal Computer", "MRIPC");
            Add(CatType.Item, MCMR.PersonalDocuments, "Personal Documents", "MRIPD");
            Add(CatType.Item, MCMR.PetrifiedFossil, "Petrified Fossil", "MRIPF");
            Add(CatType.Item, MCMR.Push, "Push", "MRIP");
            Add(CatType.Item, MCMR.PyrolyticCatalyst, "Pyrolytic Catalyst", "MRIPRYOCAT");
            Add(CatType.Item, MCMR.RefinementProcessSample, "Refinement Process Sample", "MRIRPS");
            Add(CatType.Item, MCMR.ShipSchematic, "Ship Schematic", "MRIShipSch");
            Add(CatType.Item, MCMR.SuitSchematic, "Suit Schematic", "MRISuitSch");
            Add(CatType.Item, MCMR.SurveillanceEquipment, "Surveillance Equipment", "MRISE");
            Add(CatType.Item, MCMR.SyntheticGenome, "Synthetic Genome", "MRISG");
            Add(CatType.Item, MCMR.SyntheticPathogen, "Synthetic Pathogen", "MRISP");
            Add(CatType.Item, MCMR.TrueFormFossil, "True Form Fossil", "MRITFF");
            Add(CatType.Item, MCMR.UniversalTranslator, "Universal Translator", "MRIUT");
            Add(CatType.Item, MCMR.VehicleSchematic, "Vehicle Schematic", "MRIVS");
            Add(CatType.Item, MCMR.WeaponSchematic, "Weapon Schematic", "MRIWS");
            Add(CatType.Component, MCMR.Aerogel, "Aerogel", "MRCA");
            Add(CatType.Component, MCMR.CarbonFibrePlating, "Carbon Fibre Plating", "MRCCFP");
            Add(CatType.Component, MCMR.ChemicalCatalyst, "Chemical Catalyst", "MRCCC");
            Add(CatType.Component, MCMR.ChemicalSuperbase, "Chemical Superbase", "MRCCS");
            Add(CatType.Component, MCMR.Circuitboard, "Circuit Board", "MRCCB");
            Add(CatType.Component, MCMR.CircuitSwitch, "Circuit Switch", "MRCCIRSWITCH");
            Add(CatType.Component, MCMR.ElectricalFuse, "Electrical Fuse", "MRCEF");
            Add(CatType.Component, MCMR.ElectricalWiring, "Electrical Wiring", "MRCEW");
            Add(CatType.Component, MCMR.Electromagnet, "Electromagnet", "MRCE");
            Add(CatType.Component, MCMR.EncryptedMemoryChip, "Encrypted Memory Chip", "MRCEMC");
            Add(CatType.Component, MCMR.Epinephrine, "Epinephrine", "MRCEPINE");
            Add(CatType.Component, MCMR.EpoxyAdhesive, "Epoxy Adhesive", "MRCEA");
            Add(CatType.Component, MCMR.Graphene, "Graphene", "MRCG");
            Add(CatType.Component, MCMR.IonBattery, "Ion Battery", "MRCIB");
            Add(CatType.Component, MCMR.MemoryChip, "Memory Chip", "MRCMC");
            Add(CatType.Component, MCMR.MetalCoil, "Metal Coil", "MRCMETALCOIL");
            Add(CatType.Component, MCMR.MicroElectrode, "Microelectrode", "MRCMICROELEC");
            Add(CatType.Component, MCMR.MicroHydraulics, "Micro Hydraulics", "MRCMH");
            Add(CatType.Component, MCMR.MicroSupercapacitor, "Micro Supercapacitor", "MRCMS");
            Add(CatType.Component, MCMR.MicroThrusters, "Micro Thrusters", "MRCMT");
            Add(CatType.Component, MCMR.Microtransformer, "Micro Transformer", "MRCMICTRANS");
            Add(CatType.Component, MCMR.Motor, "Motor", "MRCMOTOR");
            Add(CatType.Component, MCMR.OpticalFibre, "Optical Fibre", "MRCOF");
            Add(CatType.Component, MCMR.OpticalLens, "Optical Lens", "MRCOL");
            Add(CatType.Component, MCMR.OxygenicBacteria, "Oxygenic Bacteria", "MRCOXYBAC");
            Add(CatType.Component, MCMR.pHNeutraliser, "pH Neutraliser", "MRCPHN");
            Add(CatType.Component, MCMR.RDX, "RDX", "MRCR");
            Add(CatType.Component, MCMR.Scrambler, "Scrambler", "MRCS");
            Add(CatType.Component, MCMR.TitaniumPlating, "Titanium Plating", "MRCTP");
            Add(CatType.Component, MCMR.Transmitter, "Transmitter", "MRCTX");
            Add(CatType.Component, MCMR.TungstenCarbide, "Tungsten Carbide", "MRCTC");
            Add(CatType.Component, MCMR.ViscoElasticPolymer, "Viscoelastic Polymer", "MRCVP");
            Add(CatType.Component, MCMR.WeaponComponent, "Weapon Component", "MRCWC");
            Add(CatType.Data, MCMR.AccidentLogs, "Accident Logs", "MRDACCLOGS");
            Add(CatType.Data, MCMR.AirqualityReports, "Air Quality Reports", "MRDAQR");
            Add(CatType.Data, MCMR.AtmosphericData, "Atmospheric Data", "MRDAD");
            Add(CatType.Data, MCMR.AudioLogs, "Audio Logs", "MRDAL");
            Add(CatType.Data, MCMR.AXCombatLogs, "AX Combat Logs", "MRDACL");
            Add(CatType.Data, MCMR.BallisticsData, "Ballistics Data", "MRDBALD");
            Add(CatType.Data, MCMR.BiologicalWeaponData, "Biological Weapon Data", "MRDBWD");
            Add(CatType.Data, MCMR.BiometricData, "Biometric Data", "MRDBIOD");
            Add(CatType.Data, MCMR.BlacklistData, "Blacklist Data", "MRDBD");
            Add(CatType.Data, MCMR.BloodtestResults, "Blood Test Results", "MRDBTR");
            Add(CatType.Data, MCMR.CampaignPlans, "Campaign Plans", "MRDCP");
            Add(CatType.Data, MCMR.CatMedia, "Cat Media", "MRDCATMED");
            Add(CatType.Data, MCMR.CensusData, "Census Data", "MRDCD");
            Add(CatType.Data, MCMR.ChemicalExperimentData, "Chemical Experiment Data", "MRDCED");
            Add(CatType.Data, MCMR.ChemicalFormulae, "Chemical Formulae", "MRDCF");
            Add(CatType.Data, MCMR.ChemicalInventory, "Chemical Inventory", "MRDCI");
            Add(CatType.Data, MCMR.ChemicalPatents, "Chemical Patents", "MRDCHEMPAT");
            Add(CatType.Data, MCMR.ChemicalWeaponData, "Chemical Weapon Data", "MRDCWD");
            Add(CatType.Data, MCMR.ClassicEntertainment, "Classic Entertainment", "MRDCE");
            Add(CatType.Data, MCMR.CocktailRecipes, "Cocktail Recipes", "MRDCREC");
            Add(CatType.Data, MCMR.CombatantPerformance, "Combatant Performance", "MRDCOMBPERF");
            Add(CatType.Data, MCMR.CombatTrainingMaterial, "Combat Training Material", "MRDCTM");
            Add(CatType.Data, MCMR.ConflictHistory, "Conflict History", "MRDCH");
            Add(CatType.Data, MCMR.CriminalRecords, "Criminal Records", "MRDCRIMREC");
            Add(CatType.Data, MCMR.CropYieldAnalysis, "Crop Yield Analysis", "MRDCYA");
            Add(CatType.Data, MCMR.CulinaryRecipes, "Culinary Recipes", "MRDCULREC");
            Add(CatType.Data, MCMR.DigitalDesigns, "Digital Designs", "MRDDD");
            Add(CatType.Data, MCMR.DutyRota, "Duty Rota", "MRDDR");
            Add(CatType.Data, MCMR.EmployeeDirectory, "Employee Directory", "MRDED");
            Add(CatType.Data, MCMR.EmployeeExpenses, "Employee Expenses", "MRDEE");
            Add(CatType.Data, MCMR.EmployeeGeneticData, "Employee Genetic Data", "MRDEGD");
            Add(CatType.Data, MCMR.EmploymentHistory, "Employment History", "MRDEH");
            Add(CatType.Data, MCMR.EnhancedInterrogationRecordings, "Enhanced Interrogation Recordings", "MRDEIR");
            Add(CatType.Data, MCMR.EspionageMaterial, "Espionage Material", "MRDEM");
            Add(CatType.Data, MCMR.EvacuationProtocols, "Evacuation Protocols", "MRDEP");
            Add(CatType.Data, MCMR.ExplorationJournals, "Exploration Journals", "MRDEJ");
            Add(CatType.Data, MCMR.ExtractionYieldData, "Extraction Yield Data", "MRDEYD");
            Add(CatType.Data, MCMR.FactionAssociates, "Faction Associates", "MRDFA");
            Add(CatType.Data, MCMR.FactionDonatorList, "Faction Donator List", "MRDFDL");
            Add(CatType.Data, MCMR.FactionNews, "Faction News", "MRDFN");
            Add(CatType.Data, MCMR.FinancialProjections, "Financial Projections", "MRDFP");
            Add(CatType.Data, MCMR.FleetRegistry, "Fleet Registry", "MRDFR");
            Add(CatType.Data, MCMR.GeneSequencingData, "Gene Sequencing Data", "MRDGSD");
            Add(CatType.Data, MCMR.GeneticResearch, "Genetic Research", "MRDGR");
            Add(CatType.Data, MCMR.GeographicalData, "Geographical Data", "MRDGEOD");
            Add(CatType.Data, MCMR.GeologicalData, "Geological Data", "MRDGD");
            Add(CatType.Data, MCMR.HydroponicData, "Hydroponic Data", "MRDHD");
            Add(CatType.Data, MCMR.IncidentLogs, "Incident Logs", "MRDIL");
            Add(CatType.Data, MCMR.InfluenceProjections, "Influence Projections", "MRDIP");
            Add(CatType.Data, MCMR.InternalCorrespondence, "Internal Correspondence", "MRDIC");
            Add(CatType.Data, MCMR.InterrogationRecordings, "Interrogation Recordings", "MRDINTERREC");
            Add(CatType.Data, MCMR.InterviewRecordings, "Interview Recordings", "MRDINTERVREC");
            Add(CatType.Data, MCMR.JobApplications, "Job Applications", "MRDJA");
            Add(CatType.Data, MCMR.Kompromat, "Kompromat", "MRDK");
            Add(CatType.Data, MCMR.LiteraryFiction, "Literary Fiction", "MRDLF");
            Add(CatType.Data, MCMR.MaintenanceLogs, "Maintenance Logs", "MRDML");
            Add(CatType.Data, MCMR.ManufacturingInstructions, "Manufacturing Instructions", "MRDMI");
            Add(CatType.Data, MCMR.MedicalRecords, "Medical Records", "MRDMR");
            Add(CatType.Data, MCMR.MedicalTrialRecords, "Clinical Trial Records", "MRDCTR");
            Add(CatType.Data, MCMR.MeetingMinutes, "Meeting Minutes", "MRDMM");
            Add(CatType.Data, MCMR.MineralSurvey, "Mineral Survey", "MRDMS");
            Add(CatType.Data, MCMR.MiningAnalytics, "Mining Analytics", "MRDMA");
            Add(CatType.Data, MCMR.MultimediaEntertainment, "Multimedia Entertainment", "MRDME");
            Add(CatType.Data, MCMR.NetworkAccessHistory, "Network Access History", "MRDNAH");
            Add(CatType.Data, MCMR.NetworkSecurityProtocols, "Network Security Protocols", "MRDNSP");
            Add(CatType.Data, MCMR.NextofkinRecords, "Next of Kin Records", "MRDNKR");
            Add(CatType.Data, MCMR.NOCData, "NOC Data", "MRDNOCD");
            Add(CatType.Data, MCMR.OpinionPolls, "Opinion Polls", "MRDOP");
            Add(CatType.Data, MCMR.PatientHistory, "Patient History", "MRDPH");
            Add(CatType.Data, MCMR.PatrolRoutes, "Patrol Routes", "MRDPATR");
            Add(CatType.Data, MCMR.PayrollInformation, "Payroll Information", "MRDPI");
            Add(CatType.Data, MCMR.PersonalLogs, "Personal Logs", "MRDPL");
            Add(CatType.Data, MCMR.PharmaceuticalPatents, "Pharmaceutical Patents", "MRDPP");
            Add(CatType.Data, MCMR.PhotoAlbums, "Photo Albums", "MRDPHOTO");
            Add(CatType.Data, MCMR.PlantGrowthCharts, "Plant Growth Charts", "MRDPGC");
            Add(CatType.Data, MCMR.PoliticalAffiliations, "Political Affiliations", "MRDPOL");
            Add(CatType.Data, MCMR.PperationalManual, "Operational Manual", "MRDOM");
            Add(CatType.Data, MCMR.PrisonerLogs, "Prisoner Logs", "MRDPRISONL");
            Add(CatType.Data, MCMR.ProductionReports, "Production Reports", "MRDPRODREP");
            Add(CatType.Data, MCMR.ProductionSchedule, "Production Schedule", "MRDPS");
            Add(CatType.Data, MCMR.Propaganda, "Propaganda", "MRPROPG");
            Add(CatType.Data, MCMR.PurchaseRecords, "Purchase Records", "MRDPRCD");
            Add(CatType.Data, MCMR.PurchaseRequests, "Purchase Requests", "MRDPR");
            Add(CatType.Data, MCMR.RadioactivityData, "Radioactivity Data", "MRDRD");
            Add(CatType.Data, MCMR.ReactorOutputReview, "Reactor Output Review", "MRDROR");
            Add(CatType.Data, MCMR.RecyclingLogs, "Recycling Logs", "MRDRL");
            Add(CatType.Data, MCMR.ResidentialDirectory, "Residential Directory", "MRDRDIR");
            Add(CatType.Data, MCMR.RiskAssessments, "Risk Assessments", "MRDRA");
            Add(CatType.Data, MCMR.SalesRecords, "Sales Records", "MRDSR");
            Add(CatType.Data, MCMR.SecurityExpenses, "Security Expenses", "MRSECEXP");
            Add(CatType.Data, MCMR.SeedGeneaology, "Seed Geneaology", "MRDSG");
            Add(CatType.Data, MCMR.SettlementAssaultPlans, "Settlement Assault Plans", "MRDSAP");
            Add(CatType.Data, MCMR.SettlementDefencePlans, "Settlement Defence Plans", "MRDSDP");
            Add(CatType.Data, MCMR.ShareholderInformation, "Shareholder Information", "MRDSI");
            Add(CatType.Data, MCMR.SlushFundLogs, "Slush Fund Logs", "MRDSFL");
            Add(CatType.Data, MCMR.SmearCampaignPlans, "Smear Campaign Plans", "MRDSCP");
            Add(CatType.Data, MCMR.SpectralAnalysisData, "Spectral Analysis Data", "MRDSAD");
            Add(CatType.Data, MCMR.Spyware, "Spyware", "MRDS");
            Add(CatType.Data, MCMR.StellarActivityLogs, "Stellar Activity Logs", "MRDSAL");
            Add(CatType.Data, MCMR.SurveilleanceLogs, "Surveillance Logs", "MRDSL");
            Add(CatType.Data, MCMR.TacticalPlans, "Tactical Plans", "MRDTACP");
            Add(CatType.Data, MCMR.TaxRecords, "Tax Records", "MRDTR");
            Add(CatType.Data, MCMR.TopographicalSurveys, "Topographical Surveys", "MRDTS");
            Add(CatType.Data, MCMR.TravelPermits, "Travel Permits", "MRDTP");
            Add(CatType.Data, MCMR.TroopDeploymentRecords, "Troop Deployment Records", "MRDTDR");
            Add(CatType.Data, MCMR.UnionmemberShip, "Union Membership", "MRDUM");
            Add(CatType.Data, MCMR.VaccinationRecords, "Vaccination Records", "MRDVR");
            Add(CatType.Data, MCMR.VaccineResearch, "Vaccine Research", "MRDVACRES");
            Add(CatType.Data, MCMR.VIPSecurityDetail, "VIP Security Detail", "MRDVSD");
            Add(CatType.Data, MCMR.VirologyData, "Virology Data", "MRDVD");
            Add(CatType.Data, MCMR.Virus, "Virus", "MRDV");
            Add(CatType.Data, MCMR.VisitorRegister, "Visitor Register", "MRDVISREG");
            Add(CatType.Data, MCMR.WeaponInventory, "Weapon Inventory", "MRDWI");
            Add(CatType.Data, MCMR.WeaponTestData, "Weapon Test Data", "MRWTD");
            Add(CatType.Data, MCMR.XenoDefenceProtocols, "Xeno-Defence Protocols", "MRDXDP");
            Add(CatType.Consumable, MCMR.AMM_Grenade_EMP, "Shield Disruptor", "MRCSD");
            Add(CatType.Consumable, MCMR.AMM_Grenade_Frag, "Frag Grenade", "MRCFG");
            Add(CatType.Consumable, MCMR.AMM_Grenade_Shield, "Shield Projector", "MRCSP");
            Add(CatType.Consumable, MCMR.Bypass, "E-Breach", "MRCEB");
            Add(CatType.Consumable, MCMR.EnergyCell, "Energy Cell", "MRCEC");
            Add(CatType.Consumable, MCMR.HealthPack, "Medkit", "MRCM");


            #endregion


            cachelist = new Dictionary<string, MaterialCommodityMicroResourceType>();



            // NOTE KEEP IN ORDER BY Rarity and then Material Group Type
            #region Materials  


            // very common raw

            AddRaw("Carbon", ItemType.VeryCommon, MaterialGroupType.RawCategory1, "C");
            AddRaw("Phosphorus", ItemType.VeryCommon, MaterialGroupType.RawCategory2, "P");
            AddRaw("Sulphur", ItemType.VeryCommon, MaterialGroupType.RawCategory3, "S");
            AddRaw("Iron", ItemType.VeryCommon, MaterialGroupType.RawCategory4, "Fe");
            AddRaw("Nickel", ItemType.VeryCommon, MaterialGroupType.RawCategory5, "Ni");
            AddRaw("Rhenium", ItemType.VeryCommon, MaterialGroupType.RawCategory6, "Re");
            AddRaw("Lead", ItemType.VeryCommon, MaterialGroupType.RawCategory7, "Pb");

            // common raw

            AddRaw("Vanadium", ItemType.Common, MaterialGroupType.RawCategory1, "V");
            AddRaw("Chromium", ItemType.Common, MaterialGroupType.RawCategory2, "Cr");
            AddRaw("Manganese", ItemType.Common, MaterialGroupType.RawCategory3, "Mn");
            AddRaw("Zinc", ItemType.Common, MaterialGroupType.RawCategory4, "Zn");
            AddRaw("Germanium", ItemType.Common, MaterialGroupType.RawCategory5, "Ge");
            AddRaw("Arsenic", ItemType.Common, MaterialGroupType.RawCategory6, "As");
            AddRaw("Zirconium", ItemType.Common, MaterialGroupType.RawCategory7, "Zr");

            // standard raw

            AddRaw("Niobium", ItemType.Standard, MaterialGroupType.RawCategory1, "Nb");        // realign to Anthors standard
            AddRaw("Molybdenum", ItemType.Standard, MaterialGroupType.RawCategory2, "Mo");
            AddRaw("Cadmium", ItemType.Standard, MaterialGroupType.RawCategory3, "Cd");
            AddRaw("Tin", ItemType.Standard, MaterialGroupType.RawCategory4, "Sn");
            AddRaw("Tungsten", ItemType.Standard, MaterialGroupType.RawCategory5, "W");
            AddRaw("Mercury", ItemType.Standard, MaterialGroupType.RawCategory6, "Hg");
            AddRaw("Boron", ItemType.Standard, MaterialGroupType.RawCategory7, "B");

            // rare raw

            AddRaw("Yttrium", ItemType.Rare, MaterialGroupType.RawCategory1, "Y");
            AddRaw("Technetium", ItemType.Rare, MaterialGroupType.RawCategory2, "Tc");
            AddRaw("Ruthenium", ItemType.Rare, MaterialGroupType.RawCategory3, "Ru");
            AddRaw("Selenium", ItemType.Rare, MaterialGroupType.RawCategory4, "Se");
            AddRaw("Tellurium", ItemType.Rare, MaterialGroupType.RawCategory5, "Te");
            AddRaw("Polonium", ItemType.Rare, MaterialGroupType.RawCategory6, "Po");
            AddRaw("Antimony", ItemType.Rare, MaterialGroupType.RawCategory7, "Sb");

            // very common data
            AddEnc("Exceptional Scrambled Emission Data", ItemType.VeryCommon, MaterialGroupType.EncodedEmissionData, "ESED", "ScrambledeMissionData");
            AddEnc("Atypical Disrupted Wake Echoes", ItemType.VeryCommon, MaterialGroupType.EncodedWakeScans, "ADWE", "DisruptedWakeEchoes");
            AddEnc("Distorted Shield Cycle Recordings", ItemType.VeryCommon, MaterialGroupType.EncodedShieldData, "DSCR", "ShieldCycleRecordings");
            AddEnc("Unusual Encrypted Files", ItemType.VeryCommon, MaterialGroupType.EncodedEncryptionFiles, "UEF", "EncryptedFiles");
            AddEnc("Anomalous Bulk Scan Data", ItemType.VeryCommon, MaterialGroupType.EncodedDataArchives, "ABSD", "BulkScandata");
            AddEnc("Specialised Legacy Firmware", ItemType.VeryCommon, MaterialGroupType.EncodedFirmware, "SLF", "LegacyFirmware");

            // common data
            AddEnc("Irregular Emission Data", ItemType.Common, MaterialGroupType.EncodedEmissionData, "IED", "ArchivedEmissionData");
            AddEnc("Anomalous FSD Telemetry", ItemType.Common, MaterialGroupType.EncodedWakeScans, "AFT", "FSDTelemetry");
            AddEnc("Inconsistent Shield Soak Analysis", ItemType.Common, MaterialGroupType.EncodedShieldData, "ISSA", "ShieldSoakAnalysis");
            AddEnc("Tagged Encryption Codes", ItemType.Common, MaterialGroupType.EncodedEncryptionFiles, "TEC", "EncryptionCodes");
            AddEnc("Unidentified Scan Archives", ItemType.Common, MaterialGroupType.EncodedDataArchives, "USA", "ScanArchives");
            AddEnc("Modified Consumer Firmware", ItemType.Common, MaterialGroupType.EncodedFirmware, "MCF", "ConsumerFirmware");

            // standard data

            AddEnc("Unexpected Emission Data", ItemType.Standard, MaterialGroupType.EncodedEmissionData, "UED", "EmissionData");
            AddEnc("Strange Wake Solutions", ItemType.Standard, MaterialGroupType.EncodedWakeScans, "SWS", "WakeSolutions");
            AddEnc("Untypical Shield Scans", ItemType.Standard, MaterialGroupType.EncodedShieldData, "USS", "ShieldDensityReports");
            AddEnc("Open Symmetric Keys", ItemType.Standard, MaterialGroupType.EncodedEncryptionFiles, "OSK", "SymmetricKeys");
            AddEnc("Classified Scan Databanks", ItemType.Standard, MaterialGroupType.EncodedDataArchives, "CSD", "ScanDataNanks");
            AddEnc("Cracked Industrial Firmware", ItemType.Standard, MaterialGroupType.EncodedFirmware, "CIF", "IndustrialFirmware");

            // rare data
            AddEnc("Decoded Emission Data", ItemType.Rare, MaterialGroupType.EncodedEmissionData, "DED");
            AddEnc("Eccentric Hyperspace Trajectories", ItemType.Rare, MaterialGroupType.EncodedWakeScans, "EHT", "HyperspaceTrajectories");
            AddEnc("Aberrant Shield Pattern Analysis", ItemType.Rare, MaterialGroupType.EncodedShieldData, "ASPA", "ShieldPatternAnalysis");
            AddEnc("Atypical Encryption Archives", ItemType.Rare, MaterialGroupType.EncodedEncryptionFiles, "AEA", "EncryptionArchives");
            AddEnc("Divergent Scan Data", ItemType.Rare, MaterialGroupType.EncodedDataArchives, "DSD", "EncodedScandata");
            AddEnc("Security Firmware Patch", ItemType.Rare, MaterialGroupType.EncodedFirmware, "SFP", "SecurityFirmware");

            // very rare data

            AddEnc("Abnormal Compact Emissions Data", ItemType.VeryRare, MaterialGroupType.EncodedEmissionData, "CED", "CompactEmissionsData");
            AddEnc("Datamined Wake Exceptions", ItemType.VeryRare, MaterialGroupType.EncodedWakeScans, "DWEx", "DataminedWake");
            AddEnc("Peculiar Shield Frequency Data", ItemType.VeryRare, MaterialGroupType.EncodedShieldData, "PSFD", "ShieldFrequencyData");
            AddEnc("Adaptive Encryptors Capture", ItemType.VeryRare, MaterialGroupType.EncodedEncryptionFiles, "AEC", "AdaptiveEncryptors");
            AddEnc("Classified Scan Fragment", ItemType.VeryRare, MaterialGroupType.EncodedDataArchives, "CFSD", "ClassifiedScandata");
            AddEnc("Modified Embedded Firmware", ItemType.VeryRare, MaterialGroupType.EncodedFirmware, "EFW", "EmbeddedFirmware");

            // very common manu

            AddManu("Chemical Storage Units", ItemType.VeryCommon, MaterialGroupType.ManufacturedChemical, "CSU");
            AddManu("Tempered Alloys", ItemType.VeryCommon, MaterialGroupType.ManufacturedThermic, "TeA");
            AddManu("Heat Conduction Wiring", ItemType.VeryCommon, MaterialGroupType.ManufacturedHeat, "HCW");
            AddManu("Basic Conductors", ItemType.VeryCommon, MaterialGroupType.ManufacturedConductive, "BaC");
            AddManu("Mechanical Scrap", ItemType.VeryCommon, MaterialGroupType.ManufacturedMechanicalComponents, "MS");
            AddManu("Grid Resistors", ItemType.VeryCommon, MaterialGroupType.ManufacturedCapacitors, "GR");
            AddManu("Worn Shield Emitters", ItemType.VeryCommon, MaterialGroupType.ManufacturedShielding, "WSE");
            AddManu("Compact Composites", ItemType.VeryCommon, MaterialGroupType.ManufacturedComposite, "CC");
            AddManu("Crystal Shards", ItemType.VeryCommon, MaterialGroupType.ManufacturedCrystals, "CS");
            AddManu("Salvaged Alloys", ItemType.VeryCommon, MaterialGroupType.ManufacturedAlloys, "SAll");

            // common manu

            AddManu("Chemical Processors", ItemType.Common, MaterialGroupType.ManufacturedChemical, "CP");
            AddManu("Heat Resistant Ceramics", ItemType.Common, MaterialGroupType.ManufacturedThermic, "HRC");
            AddManu("Heat Dispersion Plate", ItemType.Common, MaterialGroupType.ManufacturedHeat, "HDP");
            AddManu("Conductive Components", ItemType.Common, MaterialGroupType.ManufacturedConductive, "CCo");
            AddManu("Mechanical Equipment", ItemType.Common, MaterialGroupType.ManufacturedMechanicalComponents, "ME");
            AddManu("Hybrid Capacitors", ItemType.Common, MaterialGroupType.ManufacturedCapacitors, "HC");
            AddManu("Shield Emitters", ItemType.Common, MaterialGroupType.ManufacturedShielding, "SHE");
            AddManu("Filament Composites", ItemType.Common, MaterialGroupType.ManufacturedComposite, "FiC");
            AddManu("Flawed Focus Crystals", ItemType.Common, MaterialGroupType.ManufacturedCrystals, "FFC", "UncutFocusCrystals");
            AddManu("Galvanising Alloys", ItemType.Common, MaterialGroupType.ManufacturedAlloys, "GA");

            // Standard manu

            AddManu("Chemical Distillery", ItemType.Standard, MaterialGroupType.ManufacturedChemical, "CHD");
            AddManu("Precipitated Alloys", ItemType.Standard, MaterialGroupType.ManufacturedThermic, "PAll");
            AddManu("Heat Exchangers", ItemType.Standard, MaterialGroupType.ManufacturedHeat, "HE");
            AddManu("Conductive Ceramics", ItemType.Standard, MaterialGroupType.ManufacturedConductive, "CCe");
            AddManu("Mechanical Components", ItemType.Standard, MaterialGroupType.ManufacturedMechanicalComponents, "MC");
            AddManu("Electrochemical Arrays", ItemType.Standard, MaterialGroupType.ManufacturedCapacitors, "EA");
            AddManu("Shielding Sensors", ItemType.Standard, MaterialGroupType.ManufacturedShielding, "SS");
            AddManu("High Density Composites", ItemType.Standard, MaterialGroupType.ManufacturedComposite, "HDC");
            AddManu("Focus Crystals", ItemType.Standard, MaterialGroupType.ManufacturedCrystals, "FoC");
            AddManu("Phase Alloys", ItemType.Standard, MaterialGroupType.ManufacturedAlloys, "PA");

            // rare manu 

            AddManu("Chemical Manipulators", ItemType.Rare, MaterialGroupType.ManufacturedChemical, "CM");
            AddManu("Thermic Alloys", ItemType.Rare, MaterialGroupType.ManufacturedThermic, "ThA");
            AddManu("Heat Vanes", ItemType.Rare, MaterialGroupType.ManufacturedHeat, "HV");
            AddManu("Conductive Polymers", ItemType.Rare, MaterialGroupType.ManufacturedConductive, "CPo");
            AddManu("Configurable Components", ItemType.Rare, MaterialGroupType.ManufacturedMechanicalComponents, "CCom");
            AddManu("Polymer Capacitors", ItemType.Rare, MaterialGroupType.ManufacturedCapacitors, "PCa");
            AddManu("Compound Shielding", ItemType.Rare, MaterialGroupType.ManufacturedShielding, "CoS");
            AddManu("Proprietary Composites", ItemType.Rare, MaterialGroupType.ManufacturedComposite, "FPC", "FedProprietaryComposites");
            AddManu("Refined Focus Crystals", ItemType.Rare, MaterialGroupType.ManufacturedCrystals, "RFC");
            AddManu("Proto Light Alloys", ItemType.Rare, MaterialGroupType.ManufacturedAlloys, "PLA");

            // very rare manu

            AddManu("Pharmaceutical Isolators", ItemType.VeryRare, MaterialGroupType.ManufacturedChemical, "PI");
            AddManu("Military Grade Alloys", ItemType.VeryRare, MaterialGroupType.ManufacturedThermic, "MGA");
            AddManu("Proto Heat Radiators", ItemType.VeryRare, MaterialGroupType.ManufacturedHeat, "PHR");
            AddManu("Biotech Conductors", ItemType.VeryRare, MaterialGroupType.ManufacturedConductive, "BiC");
            AddManu("Improvised Components", ItemType.VeryRare, MaterialGroupType.ManufacturedMechanicalComponents, "IC");
            AddManu("Military Supercapacitors", ItemType.VeryRare, MaterialGroupType.ManufacturedCapacitors, "MSC");
            AddManu("Imperial Shielding", ItemType.VeryRare, MaterialGroupType.ManufacturedShielding, "IS");
            AddManu("Core Dynamics Composites", ItemType.VeryRare, MaterialGroupType.ManufacturedComposite, "FCC", "FedCoreComposites");
            AddManu("Exquisite Focus Crystals", ItemType.VeryRare, MaterialGroupType.ManufacturedCrystals, "EFC");
            AddManu("Proto Radiolic Alloys", ItemType.VeryRare, MaterialGroupType.ManufacturedAlloys, "PRA");

            // Obelisk

            AddEnc("Pattern Alpha Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PAOD", "AncientBiologicalData");
            AddEnc("Pattern Beta Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PBOD", "AncientCulturalData");
            AddEnc("Pattern Gamma Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PGOD", "AncientHistoricalData");
            AddEnc("Pattern Delta Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PDOD", "AncientLanguageData");
            AddEnc("Pattern Epsilon Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PEOD", "AncientTechnologicalData");

            // new to 3.1 frontier data

            AddManu("Guardian Power Cell", ItemType.VeryCommon, MaterialGroupType.NA, "GPCe", "Guardian_PowerCell");
            AddManu("Guardian Power Conduit", ItemType.Common, MaterialGroupType.NA, "GPC", "Guardian_PowerConduit");
            AddManu("Guardian Technology Component", ItemType.Standard, MaterialGroupType.NA, "GTC", "Guardian_TechComponent");
            AddManu("Guardian Sentinel Weapon Parts", ItemType.Standard, MaterialGroupType.NA, "GSWP", "Guardian_Sentinel_WeaponParts");
            AddManu("Guardian Wreckage Components", ItemType.VeryCommon, MaterialGroupType.NA, "GSWC", "Guardian_Sentinel_WreckageComponents");
            AddEnc("Guardian Weapon Blueprint Fragment", ItemType.Rare, MaterialGroupType.NA, "GWBS", "Guardian_WeaponBlueprint");
            AddEnc("Guardian Module Blueprint Fragment", ItemType.Rare, MaterialGroupType.NA, "GMBS", "Guardian_ModuleBlueprint");

            // new to 3.2 frontier data
            AddEnc("Guardian Vessel Blueprint Fragment", ItemType.VeryRare, MaterialGroupType.NA, "GMVB", "Guardian_VesselBlueprint");
            AddManu("Bio-Mechanical Conduits", ItemType.Standard, MaterialGroupType.NA, "BMC", "TG_BioMechanicalConduits");
            AddManu("Propulsion Elements", ItemType.VeryRare, MaterialGroupType.NA, "PE", "TG_PropulsionElement");
            AddManu("Weapon Parts", ItemType.Rare, MaterialGroupType.NA, "WP", "TG_WeaponParts");
            AddEnc("Ship Flight Data", ItemType.Standard, MaterialGroupType.NA, "SFD", "TG_ShipFlightData");
            AddEnc("Ship Systems Data", ItemType.Rare, MaterialGroupType.NA, "SSD", "TG_ShipSystemsData");

            // new to update 15 - inara/devtalk
            AddManu("Hardened Surface Fragments", ItemType.VeryCommon, MaterialGroupType.NA, "HSF", "TG_Abrasion03");
            AddManu("Phasing Membrane Residue", ItemType.Standard, MaterialGroupType.NA, "PMR", "TG_Abrasion02");
            AddManu("Heat Exposure Specimen", ItemType.VeryRare, MaterialGroupType.NA, "TG_Abrasion01");

            AddManu("Caustic Crystal", ItemType.Rare, MaterialGroupType.NA, "CACR", "TG_CausticCrystal");       // inara
            AddManu("Caustic Shard", ItemType.Standard, MaterialGroupType.NA, "CASH", "TG_CausticShard");
            AddManu("Corrosive Mechanisms", ItemType.Standard, MaterialGroupType.NA, "COMEC", "TG_CausticGeneratorParts");
            AddManu("Massive Energy Surge Analytics", ItemType.Standard, MaterialGroupType.NA, "MESA", "TG_ShutdownData");
            AddManu("Thargoid Interdiction Telemetry", ItemType.Standard, MaterialGroupType.NA, "TIT", "TG_InterdictionData");

            ItemType sv = ItemType.Salvage;
            AddCommodity("Thargoid Sensor", sv, "UnknownArtifact");
            AddCommodity("Thargoid Probe", sv, "UnknownArtifact2");
            AddCommodity("Thargoid Link", sv, "UnknownArtifact3");
            AddCommodity("Thargoid Resin", sv, "UnknownResin");
            AddCommodity("Thargoid Biological Matter", sv, "UnknownBiologicalMatter");
            AddCommodity("Thargoid Technology Samples", sv, "UnknownTechnologySamples");

            AddManu("Thargoid Carapace", ItemType.Common, MaterialGroupType.NA, "UKCP", "UnknownCarapace");
            AddManu("Thargoid Energy Cell", ItemType.Standard, MaterialGroupType.NA, "UKEC", "UnknownEnergycell");
            AddManu("Thargoid Organic Circuitry", ItemType.VeryRare, MaterialGroupType.NA, "UKOC", "UnknownOrganicCircuitry");
            AddManu("Thargoid Technological Components", ItemType.Rare, MaterialGroupType.NA, "UKTC", "UnknownTechnologyComponents");
            AddManu("Sensor Fragment", ItemType.VeryRare, MaterialGroupType.NA, "UES", "Unknownenergysource");

            AddEnc("Thargoid Material Composition Data", ItemType.Standard, MaterialGroupType.NA, "UMCD", "TG_CompositionData");
            AddEnc("Thargoid Structural Data", ItemType.Common, MaterialGroupType.NA, "UKSD", "TG_StructuralData");
            AddEnc("Thargoid Residue Data", ItemType.Rare, MaterialGroupType.NA, "URDA", "TG_ResidueData");
            AddEnc("Thargoid Ship Signature", ItemType.Standard, MaterialGroupType.NA, "USSig", "UnknownShipSignature");
            AddEnc("Thargoid Wake Data", ItemType.Rare, MaterialGroupType.NA, "UWD", "UnknownWakeData");

            #endregion

            #region Commodities 

            AddCommodity("Rockforth Fertiliser", ItemType.Chemicals, "RockforthFertiliser");
            AddCommodity("Agronomic Treatment", ItemType.Chemicals, "AgronomicTreatment");
            AddCommodity("Tritium", ItemType.Chemicals, "Tritium");
            AddCommodityList("Explosives;Hydrogen Fuel;Hydrogen Peroxide;Liquid Oxygen;Mineral Oil;Nerve Agents;Pesticides;Surface Stabilisers;Synthetic Reagents;Water", ItemType.Chemicals);

            ItemType ci = ItemType.ConsumerItems;
            AddCommodityList("Clothing;Consumer Technology;Domestic Appliances;Evacuation Shelter;Survival Equipment", ci);
            AddCommodity("Duradrives", ci, "Duradrives");

            ItemType fd = ItemType.Foods;
            AddCommodityList("Algae;Animal Meat;Coffee;Fish;Food Cartridges;Fruit and Vegetables;Grain;Synthetic Meat;Tea", fd);

            ItemType im = ItemType.IndustrialMaterials;
            AddCommodityList("Ceramic Composites;Insulating Membrane;Polymers;Semiconductors;Superconductors", im);
            AddCommoditySN("Meta-Alloys", im, "MA", "Metaalloys");
            AddCommoditySN("Micro-Weave Cooling Hoses", im, "MWCH", "CoolingHoses");
            AddCommoditySN("Neofabric Insulation", im, "NFI", "");
            AddCommoditySN("CMM Composite", im, "CMMC", "");

            ItemType ld = ItemType.LegalDrugs;
            AddCommodityList("Beer;Bootleg Liquor;Liquor;Tobacco;Wine", ld);

            ItemType m = ItemType.Machinery;
            AddCommodity("Atmospheric Processors", m, "AtmosphericExtractors");
            AddCommodity("Marine Equipment", m, "MarineSupplies");
            AddCommodity("Microbial Furnaces", m, "HeliostaticFurnaces");
            AddCommodity("Skimmer Components", m, "SkimerComponents");
            AddCommodityList("Building Fabricators;Crop Harvesters;Emergency Power Cells;Exhaust Manifold;Geological Equipment", m);
            AddCommoditySN("HN Shock Mount", m, "HNSM", "");
            AddCommodityList("Mineral Extractors;Modular Terminals;Power Generators", m);
            AddCommoditySN("Thermal Cooling Units", m, "TCU", "");
            AddCommoditySN("Water Purifiers", m, "WPURE", "");
            AddCommoditySN("Heatsink Interlink", m, "HSI", "");
            AddCommoditySN("Energy Grid Assembly", m, "EGA", "PowerGridAssembly");
            AddCommoditySN("Radiation Baffle", m, "RB", "");
            AddCommoditySN("Magnetic Emitter Coil", m, "MEC", "");
            AddCommoditySN("Articulation Motors", m, "AM", "");
            AddCommoditySN("Reinforced Mounting Plate", m, "RMP", "");
            AddCommoditySN("Power Transfer Bus", m, "PTB", "PowerTransferConduits");
            AddCommoditySN("Power Converter", m, "PC", "");
            AddCommoditySN("Ion Distributor", m, "IOD", "");

            ItemType md = ItemType.Medicines;
            AddCommodityList("Advanced Medicines;Basic Medicines;Combat Stabilisers;Performance Enhancers;Progenitor Cells", md);
            AddCommodity("Agri-Medicines", md, "AgriculturalMedicines");
            AddCommodity("Nanomedicines", md, "Nanomedicines"); // not in frontier data. Keep for now Jan 2020

            ItemType mt = ItemType.Metals;
            AddCommodityList("Aluminium;Beryllium;Bismuth;Cobalt;Copper;Gallium;Gold;Hafnium 178;Indium;Lanthanum;Lithium;Palladium;Platinum;Praseodymium;Samarium;Silver;Tantalum;Thallium;Thorium;Titanium;Uranium", mt);
            AddCommoditySN("Osmium", mt, "OSM", "Osmium");
            AddCommodity("Platinum Alloy", mt, "PlatinumAloy");

            ItemType mi = ItemType.Minerals;
            AddCommodityList("Bauxite;Bertrandite;Bromellite;Coltan;Cryolite;Gallite;Goslarite;Methane Clathrate", mi);
            AddCommodityList("Indite;Jadeite;Lepidolite;Lithium Hydroxide;Moissanite;Painite;Pyrophyllite;Rutile;Taaffeite;Uraninite", mi);
            AddCommodity("Methanol Monohydrate Crystals", mi, "methanolmonohydratecrystals");
            AddCommodity("Low Temperature Diamonds", mi, "lowtemperaturediamond");
            AddCommodity("Void Opal", mi, "Opal");
            AddCommodity("Rhodplumsite", mi, "Rhodplumsite");
            AddCommodity("Serendibite", mi, "Serendibite");
            AddCommodity("Monazite", mi, "Monazite");
            AddCommodity("Musgravite", mi, "Musgravite");
            AddCommodity("Benitoite", mi, "Benitoite");
            AddCommodity("Grandidierite", mi, "Grandidierite");
            AddCommodity("Alexandrite", mi, "Alexandrite");

            // Salvage

            AddCommodity("Trinkets of Hidden Fortune", sv, "TrinketsOfFortune");
            AddCommodity("Gene Bank", sv, "GeneBank");
            AddCommodity("Time Capsule", sv, "TimeCapsule");
            AddCommodity("Damaged Escape Pod", sv, "DamagedEscapePod");
            AddCommodity("Thargoid Heart", sv, "ThargoidHeart");
            AddCommodity("Thargoid Cyclops Tissue Sample", sv, "ThargoidTissueSampleType1");
            AddCommodity("Thargoid Basilisk Tissue Sample", sv, "ThargoidTissueSampleType2");
            AddCommodity("Thargoid Medusa Tissue Sample", sv, "ThargoidTissueSampleType3");
            AddCommodity("Thargoid Scout Tissue Sample", sv, "ThargoidScoutTissueSample");
            AddCommodity("Wreckage Components", sv, "WreckageComponents");
            AddCommodity("Antique Jewellery", sv, "AntiqueJewellery");
            AddCommodity("Thargoid Hydra Tissue Sample", sv, "ThargoidTissueSampleType4");
            AddCommodity("Ancient Key", sv, "AncientKey");

            AddCommodityList("Ai Relics;Antimatter Containment Unit;Antiquities;Assault Plans;Data Core;Diplomatic Bag;Encrypted Correspondence;Fossil Remnants", sv);
            AddCommodityList("Geological Samples;Military Intelligence;Mysterious Idol;Personal Effects;Precious Gems;Prohibited Research Materials", sv);
            AddCommodityList("Sap 8 Core Container;Scientific Research;Scientific Samples;Space Pioneer Relics;Tactical Data;Unstable Data Core", sv);
            AddCommodity("Large Survey Data Cache", sv, "LargeExplorationDatacash");
            AddCommodity("Small Survey Data Cache", sv, "SmallExplorationDatacash");
            AddCommodity("Ancient Artefact", sv, "USSCargoAncientArtefact");
            AddCommodity("Black Box", sv, "USSCargoBlackBox");
            AddCommodity("Political Prisoners", sv, "PoliticalPrisoner");
            AddCommodity("Hostages", sv, "Hostage");
            AddCommodity("Commercial Samples", sv, "ComercialSamples");
            AddCommodity("Encrypted Data Storage", sv, "EncriptedDataStorage");
            AddCommodity("Experimental Chemicals", sv, "USSCargoExperimentalChemicals");
            AddCommodity("Military Plans", sv, "USSCargoMilitaryPlans");
            AddCommodity("Occupied Escape Pod", sv, "OccupiedCryoPod");
            AddCommodity("Prototype Tech", sv, "USSCargoPrototypeTech");
            AddCommodity("Rare Artwork", sv, "USSCargoRareArtwork");
            AddCommodity("Rebel Transmissions", sv, "USSCargoRebelTransmissions");
            AddCommodity("Technical Blueprints", sv, "USSCargoTechnicalBlueprints");
            AddCommodity("Trade Data", sv, "USSCargoTradeData");
            AddCommodity("Guardian Relic", sv, "AncientRelic");
            AddCommoditySN("Unclassified Relic", sv, "ARTG", "AncientRelicTG");
            AddCommodity("Guardian Orb", sv, "AncientOrb");
            AddCommodity("Guardian Casket", sv, "AncientCasket");
            AddCommodity("Guardian Tablet", sv, "AncientTablet");
            AddCommodity("Guardian Urn", sv, "AncientUrn");
            AddCommodity("Guardian Totem", sv, "AncientTotem");

            AddCommodity("Mollusc Soft Tissue", sv, "M_TissueSample_Soft");
            AddCommodity("Pod Core Tissue", sv, "S_TissueSample_Cells");
            AddCommodity("Pod Dead Tissue", sv, "S_TissueSample_Surface");
            AddCommodity("Pod Surface Tissue", sv, "S_TissueSample_Core");
            AddCommodity("Mollusc Membrane", sv, "M3_TissueSample_Membrane");
            AddCommodity("Mollusc Mycelium", sv, "M3_TissueSample_Mycelium");
            AddCommodity("Mollusc Spores", sv, "M3_TissueSample_Spores");
            AddCommodity("Pod Shell Tissue", sv, "S6_TissueSample_Coenosarc");
            AddCommodity("Pod Mesoglea", sv, "S6_TissueSample_Mesoglea");
            AddCommodity("Pod Outer Tissue", sv, "S6_TissueSample_Cells");
            AddCommodity("Mollusc Fluid", ItemType.Salvage, "M_TissueSample_Fluid");
            AddCommodity("Mollusc Brain Tissue", ItemType.Salvage, "M_TissueSample_Nerves");
            AddCommodity("Pod Tissue", ItemType.Salvage, "S9_TissueSample_Shell");
            AddCommodity("Anomaly Particles", sv, "P_ParticulateSample");
            AddCommodity("Titan Maw Partial Tissue Sample", sv, "ThargoidTissueSampleType10c");
            AddCommodity("Thargoid Orthrus Tissue Sample", sv, "ThargoidTissueSampleType5");
            AddCommodity("Caustic Tissue Sample", sv, "ThargoidGeneratorTissueSample");
            AddCommodity("Unoccupied Escape Pod", sv, "UnocuppiedEscapePod");

            // update 15
            AddCommodity("Thargoid Glaive Tissue Sample", sv, "ThargoidTissueSampleType6");
            AddCommodity("Titan Deep Tissue Sample", sv, "ThargoidTissueSampleType9a");
            AddCommodity("Titan Tissue Sample", sv, "ThargoidTissueSampleType9b");
            AddCommodity("Titan Partial Tissue Sample", sv, "ThargoidTissueSampleType9c");
            AddCommodity("Titan Maw Deep Tissue Sample", sv, "ThargoidTissueSampleType10a");
            AddCommodity("Titan Maw Tissue Sample", sv, "ThargoidTissueSampleType10b");

            // update 16, devtalk
            AddCommodity("Thargoid Scythe Tissue Sample", sv, "ThargoidTissueSampleType7");
            AddCommodity("Protective Membrane Scrap", sv, "UnknownSack");
            AddCommodity("Xenobiological Prison Pod", sv, "ThargoidPod");
            AddCommodity("Coral Sap", sv, "CoralSap");

            ItemType nc = ItemType.Narcotics;
            AddCommodity("Narcotics", nc, "BasicNarcotics");

            ItemType sl = ItemType.Slaves;
            AddCommodityList("Imperial Slaves;Slaves", sl);

            ItemType tc = ItemType.Technology;
            AddCommodityList("Advanced Catalysers;Animal Monitors;Aquaponic Systems;Bioreducing Lichen;Computer Components", tc);
            AddCommodity("Auto-Fabricators", tc, "autofabricators");
            AddCommoditySN("Micro Controllers", tc, "MCC", "MicroControllers");
            AddCommodityList("Medical Diagnostic Equipment", tc);
            AddCommodityList("Nanobreakers;Resonating Separators;Robotics;Structural Regulators;Telemetry Suite", tc);
            AddCommodity("H.E. Suits", tc, "HazardousEnvironmentSuits");
            AddCommoditySN("Hardware Diagnostic Sensor", tc, "DIS", "DiagnosticSensor");
            AddCommodity("Muon Imager", tc, "MutomImager");
            AddCommodity("Land Enrichment Systems", tc, "TerrainEnrichmentSystems");

            ItemType tx = ItemType.Textiles;
            AddCommodityList("Conductive Fabrics;Leather;Military Grade Fabrics;Natural Fabrics;Synthetic Fabrics", tx);

            ItemType ws = ItemType.Waste;
            AddCommodityList("Biowaste;Chemical Waste;Scrap;Toxic Waste", ws);

            ItemType wp = ItemType.Weapons;
            AddCommodityList("Battle Weapons;Landmines;Personal Weapons;Reactive Armour", wp);
            AddCommodity("Non-Lethal Weapons", wp, "NonLethalWeapons");

            #endregion

            #region Rare Commodities 

            AddCommodityRare("Apa Vietii", ItemType.Narcotics, "ApaVietii");
            AddCommodityRare("The Hutton Mug", ItemType.ConsumerItems, "TheHuttonMug");
            AddCommodityRare("Eranin Pearl Whisky", ItemType.LegalDrugs, "EraninPearlWhisky");
            AddCommodityRare("Lavian Brandy", ItemType.LegalDrugs, "LavianBrandy");
            AddCommodityRare("HIP 10175 Bush Meat", ItemType.Foods, "HIP10175BushMeat");
            AddCommodityRare("Albino Quechua Mammoth Meat", ItemType.Foods, "AlbinoQuechuaMammoth");
            AddCommodityRare("Utgaroar Millennial Eggs", ItemType.Foods, "UtgaroarMillenialEggs");
            AddCommodityRare("Witchhaul Kobe Beef", ItemType.Foods, "WitchhaulKobeBeef");
            AddCommodityRare("Karsuki Locusts", ItemType.Foods, "KarsukiLocusts");
            AddCommodityRare("Giant Irukama Snails", ItemType.Foods, "GiantIrukamaSnails");
            AddCommodityRare("Baltah'sine Vacuum Krill", ItemType.Foods, "BaltahSineVacuumKrill");
            AddCommodityRare("Ceti Rabbits", ItemType.Foods, "CetiRabbits");
            AddCommodityRare("Kachirigin Filter Leeches", ItemType.Medicines, "KachiriginLeaches");
            AddCommodityRare("Lyrae Weed", ItemType.Narcotics, "LyraeWeed");
            AddCommodityRare("Onionhead", ItemType.Narcotics, "OnionHead");
            AddCommodityRare("Tarach Spice", ItemType.Narcotics, "TarachTorSpice");
            AddCommodityRare("Wolf Fesh", ItemType.Narcotics, "Wolf1301Fesh");
            AddCommodityRare("Borasetani Pathogenetics", ItemType.Weapons, "BorasetaniPathogenetics");
            AddCommodityRare("HIP 118311 Swarm", ItemType.Weapons, "HIP118311Swarm");
            AddCommodityRare("Kongga Ale", ItemType.LegalDrugs, "KonggaAle");
            AddCommodityRare("Wuthielo Ku Froth", ItemType.LegalDrugs, "WuthieloKuFroth");
            AddCommodityRare("Alacarakmo Skin Art", ItemType.ConsumerItems, "AlacarakmoSkinArt");
            AddCommodityRare("Eleu Thermals", ItemType.ConsumerItems, "EleuThermals");
            AddCommodityRare("Eshu Umbrellas", ItemType.ConsumerItems, "EshuUmbrellas");
            AddCommodityRare("Karetii Couture", ItemType.ConsumerItems, "KaretiiCouture");
            AddCommodityRare("Njangari Saddles", ItemType.ConsumerItems, "NjangariSaddles");
            AddCommodityRare("Any Na Coffee", ItemType.Foods, "AnyNaCoffee");
            AddCommodityRare("CD-75 Kitten Brand Coffee", ItemType.Foods, "CD75CatCoffee");
            AddCommodityRare("Goman Yaupon Coffee", ItemType.Foods, "GomanYauponCoffee");
            AddCommodityRare("Volkhab Bee Drones", ItemType.Machinery, "VolkhabBeeDrones");
            AddCommodityRare("Kinago Violins", ItemType.ConsumerItems, "KinagoInstruments");
            AddCommodityRare("Nguna Modern Antiques", ItemType.ConsumerItems, "NgunaModernAntiques");
            AddCommodityRare("Rajukru Multi-Stoves", ItemType.ConsumerItems, "RajukruStoves");
            AddCommodityRare("Tiolce Waste2Paste Units", ItemType.ConsumerItems, "TiolceWaste2PasteUnits");
            AddCommodityRare("Chi Eridani Marine Paste", ItemType.Foods, "ChiEridaniMarinePaste");
            AddCommodityRare("Esuseku Caviar", ItemType.Foods, "EsusekuCaviar");
            AddCommodityRare("Live Hecate Sea Worms", ItemType.Foods, "LiveHecateSeaWorms");
            AddCommodityRare("Helvetitj Pearls", ItemType.Foods, "HelvetitjPearls");
            AddCommodityRare("HIP Proto-Squid", ItemType.Foods, "HIP41181Squid");
            AddCommodityRare("Coquim Spongiform Victuals", ItemType.Foods, "CoquimSpongiformVictuals");
            AddCommodityRare("Eden Apples Of Aerial", ItemType.Foods, "AerialEdenApple");
            AddCommodityRare("Neritus Berries", ItemType.Foods, "NeritusBerries");
            AddCommodityRare("Ochoeng Chillies", ItemType.Foods, "OchoengChillies");
            AddCommodityRare("Deuringas Truffles", ItemType.Foods, "DeuringasTruffles");
            AddCommodityRare("HR 7221 Wheat", ItemType.Foods, "HR7221Wheat");
            AddCommodityRare("Jaroua Rice", ItemType.Foods, "JarouaRice");
            AddCommodityRare("Belalans Ray Leather", ItemType.Textiles, "BelalansRayLeather");
            AddCommodityRare("Damna Carapaces", ItemType.Textiles, "DamnaCarapaces");
            AddCommodityRare("Rapa Bao Snake Skins", ItemType.Textiles, "RapaBaoSnakeSkins");
            AddCommodityRare("Vanayequi Ceratomorpha Fur", ItemType.Textiles, "VanayequiRhinoFur");
            AddCommodityRare("Bast Snake Gin", ItemType.LegalDrugs, "BastSnakeGin");
            AddCommodityRare("Thrutis Cream", ItemType.LegalDrugs, "ThrutisCream");
            AddCommodityRare("Wulpa Hyperbore Systems", ItemType.Machinery, "WulpaHyperboreSystems");
            AddCommodityRare("Aganippe Rush", ItemType.Medicines, "AganippeRush");
            AddCommodityRare("Terra Mater Blood Bores", ItemType.Medicines, "TerraMaterBloodBores");
            AddCommodityRare("Holva Duelling Blades", ItemType.Weapons, "HolvaDuellingBlades");
            AddCommodityRare("Kamorin Historic Weapons", ItemType.Weapons, "KamorinHistoricWeapons");
            AddCommodityRare("Gilya Signature Weapons", ItemType.Weapons, "GilyaSignatureWeapons");
            AddCommodityRare("Delta Phoenicis Palms", ItemType.Chemicals, "DeltaPhoenicisPalms");
            AddCommodityRare("Toxandji Virocide", ItemType.Chemicals, "ToxandjiVirocide");
            AddCommodityRare("Xihe Biomorphic Companions", ItemType.Technology, "XiheCompanions");
            AddCommodityRare("Sanuma Decorative Meat", ItemType.Foods, "SanumaMEAT");
            AddCommodityRare("Ethgreze Tea Buds", ItemType.Foods, "EthgrezeTeaBuds");
            AddCommodityRare("Ceremonial Heike Tea", ItemType.Foods, "CeremonialHeikeTea");
            AddCommodityRare("Tanmark Tranquil Tea", ItemType.Foods, "TanmarkTranquilTea");
            AddCommodityRare("AZ Cancri Formula 42", ItemType.Technology, "AZCancriFormula42");
            AddCommodityRare("Sothis Crystalline Gold", ItemType.Metals, "SothisCrystallineGold");
            AddCommodityRare("Kamitra Cigars", ItemType.LegalDrugs, "KamitraCigars");
            AddCommodityRare("Rusani Old Smokey", ItemType.LegalDrugs, "RusaniOldSmokey");
            AddCommodityRare("Yaso Kondi Leaf", ItemType.LegalDrugs, "YasoKondiLeaf");
            AddCommodityRare("Chateau De Aegaeon", ItemType.LegalDrugs, "ChateauDeAegaeon");
            AddCommodityRare("The Waters Of Shintara", ItemType.Medicines, "WatersOfShintara");
            AddCommodityRare("Ophiuch Exino Artefacts", ItemType.ConsumerItems, "OphiuchiExinoArtefacts");
            AddCommodityRare("Baked Greebles", ItemType.Foods, "BakedGreebles");
            AddCommodityRare("Aepyornis Egg", ItemType.Foods, "CetiAepyornisEgg");
            AddCommodityRare("Saxon Wine", ItemType.LegalDrugs, "SaxonWine");
            AddCommodityRare("Centauri Mega Gin", ItemType.LegalDrugs, "CentauriMegaGin");
            AddCommodityRare("Anduliga Fire Works", ItemType.Chemicals, "AnduligaFireWorks");
            AddCommodityRare("Banki Amphibious Leather", ItemType.Textiles, "BankiAmphibiousLeather");
            AddCommodityRare("Cherbones Blood Crystals", ItemType.Minerals, "CherbonesBloodCrystals");
            AddCommodityRare("Motrona Experience Jelly", ItemType.Narcotics, "MotronaExperienceJelly");
            AddCommodityRare("Geawen Dance Dust", ItemType.Narcotics, "GeawenDanceDust");
            AddCommodityRare("Gerasian Gueuze Beer", ItemType.LegalDrugs, "GerasianGueuzeBeer");
            AddCommodityRare("Haiden Black Brew", ItemType.Foods, "HaidneBlackBrew");
            AddCommodityRare("Havasupai Dream Catcher", ItemType.ConsumerItems, "HavasupaiDreamCatcher");
            AddCommodityRare("Burnham Bile Distillate", ItemType.LegalDrugs, "BurnhamBileDistillate");
            AddCommodityRare("Hip Organophosphates", ItemType.Chemicals, "HIPOrganophosphates");
            AddCommodityRare("Jaradharre Puzzle Box", ItemType.ConsumerItems, "JaradharrePuzzlebox");
            AddCommodityRare("Koro Kung Pellets", ItemType.Chemicals, "KorroKungPellets");
            AddCommodityRare("Void Extract Coffee", ItemType.Foods, "LFTVoidExtractCoffee");
            AddCommodityRare("Honesty Pills", ItemType.Medicines, "HonestyPills");
            AddCommodityRare("Non Euclidian Exotanks", ItemType.Machinery, "NonEuclidianExotanks");
            AddCommodityRare("LTT Hyper Sweet", ItemType.Foods, "LTTHyperSweet");
            AddCommodityRare("Mechucos High Tea", ItemType.Foods, "MechucosHighTea");
            AddCommodityRare("Medb Starlube", ItemType.IndustrialMaterials, "MedbStarlube");
            AddCommodityRare("Mokojing Beast Feast", ItemType.Foods, "MokojingBeastFeast");
            AddCommodityRare("Mukusubii Chitin-os", ItemType.Foods, "MukusubiiChitinOs");
            AddCommodityRare("Mulachi Giant Fungus", ItemType.Foods, "MulachiGiantFungus");
            AddCommodityRare("Ngadandari Fire Opals", ItemType.Minerals, "NgadandariFireOpals");
            AddCommodityRare("Tiegfries Synth Silk", ItemType.Textiles, "TiegfriesSynthSilk");
            AddCommodityRare("Uzumoku Low-G Wings", ItemType.ConsumerItems, "UzumokuLowGWings");
            AddCommodityRare("V Herculis Body Rub", ItemType.Medicines, "VHerculisBodyRub");
            AddCommodityRare("Wheemete Wheat Cakes", ItemType.Foods, "WheemeteWheatCakes");
            AddCommodityRare("Vega Slimweed", ItemType.Medicines, "VegaSlimWeed");
            AddCommodityRare("Altairian Skin", ItemType.ConsumerItems, "AltairianSkin");
            AddCommodityRare("Pavonis Ear Grubs", ItemType.Narcotics, "PavonisEarGrubs");
            AddCommodityRare("Jotun Mookah", ItemType.ConsumerItems, "JotunMookah");
            AddCommodityRare("Giant Verrix", ItemType.Machinery, "GiantVerrix");
            AddCommodityRare("Indi Bourbon", ItemType.LegalDrugs, "IndiBourbon");
            AddCommodityRare("Arouca Conventual Sweets", ItemType.Foods, "AroucaConventualSweets");
            AddCommodityRare("Tauri Chimes", ItemType.Medicines, "TauriChimes");
            AddCommodityRare("Zeessze Ant Grub Glue", ItemType.ConsumerItems, "ZeesszeAntGlue");
            AddCommodityRare("Pantaa Prayer Sticks", ItemType.Medicines, "PantaaPrayerSticks");
            AddCommodityRare("Fujin Tea", ItemType.Medicines, "FujinTea");
            AddCommodityRare("Chameleon Cloth", ItemType.Textiles, "ChameleonCloth");
            AddCommodityRare("Orrerian Vicious Brew", ItemType.Foods, "OrrerianViciousBrew");
            AddCommodityRare("Uszaian Tree Grub", ItemType.Foods, "UszaianTreeGrub");
            AddCommodityRare("Momus Bog Spaniel", ItemType.ConsumerItems, "MomusBogSpaniel");
            AddCommodityRare("Diso Ma Corn", ItemType.Foods, "DisoMaCorn");
            AddCommodityRare("Leestian Evil Juice", ItemType.LegalDrugs, "LeestianEvilJuice");
            AddCommodityRare("Azure Milk", ItemType.LegalDrugs, "BlueMilk");
            AddCommodityRare("Leathery Eggs", ItemType.ConsumerItems, "AlienEggs");
            AddCommodityRare("Alya Body Soap", ItemType.Medicines, "AlyaBodilySoap");
            AddCommodityRare("Vidavantian Lace", ItemType.ConsumerItems, "VidavantianLace");
            AddCommodityRare("Lucan Onionhead", ItemType.Narcotics, "TransgenicOnionHead");
            AddCommodityRare("Jaques Quinentian Still", ItemType.ConsumerItems, "JaquesQuinentianStill");
            AddCommodityRare("Soontill Relics", ItemType.ConsumerItems, "SoontillRelics");
            AddCommodityRare("Onionhead Alpha Strain", ItemType.Narcotics, "OnionHeadA");
            AddCommodityRare("Onionhead Beta Strain", ItemType.Narcotics, "OnionHeadB");
            AddCommodityRare("Onionhead Gamma Strain", ItemType.Narcotics, "OnionHeadC");
            AddCommodityRare("Galactic Travel Guide", sv, "GalacticTravelGuide");
            AddCommodityRare("Crom Silver Fesh", ItemType.Narcotics, "AnimalEffigies");
            AddCommodityRare("Shan's Charis Orchid", ItemType.ConsumerItems, "ShansCharisOrchid");
            AddCommodityRare("Buckyball Beer Mats", ItemType.ConsumerItems, "BuckyballBeerMats");
            AddCommodityRare("Master Chefs", ItemType.Slaves, "MasterChefs");
            AddCommodityRare("Personal Gifts", ItemType.ConsumerItems, "PersonalGifts");
            AddCommodityRare("Crystalline Spheres", ItemType.ConsumerItems, "CrystallineSpheres");
            AddCommodityRare("Ultra-Compact Processor Prototypes", ItemType.ConsumerItems, "Advert1");
            AddCommodityRare("Harma Silver Sea Rum", ItemType.Narcotics, "HarmaSilverSeaRum");
            AddCommodityRare("Earth Relics", sv, "EarthRelics");
            AddCommodityRare("Classified Experimental Equipment", sv, "ClassifiedExperimentalEquipment");

            #endregion

            #region Powerplay 

            AddCommodity("Aisling Media Materials", ItemType.PowerPlay, "AislingMediaMaterials");
            AddCommodity("Aisling Sealed Contracts", ItemType.PowerPlay, "AislingMediaResources");
            AddCommodity("Aisling Programme Materials", ItemType.PowerPlay, "AislingPromotionalMaterials");
            AddCommodity("Alliance Trade Agreements", ItemType.PowerPlay, "AllianceTradeAgreements");
            AddCommodity("Alliance Legislative Contracts", ItemType.PowerPlay, "AllianceLegaslativeContracts");
            AddCommodity("Alliance Legislative Records", ItemType.PowerPlay, "AllianceLegaslativeRecords");
            AddCommodity("Lavigny Corruption Reports", ItemType.PowerPlay, "LavignyCorruptionDossiers");
            AddCommodity("Lavigny Field Supplies", ItemType.PowerPlay, "LavignyFieldSupplies");
            AddCommodity("Lavigny Garrison Supplies", ItemType.PowerPlay, "LavignyGarisonSupplies");
            AddCommodity("Core Restricted Package", ItemType.PowerPlay, "RestrictedPackage");
            AddCommodity("Liberal Propaganda", ItemType.PowerPlay, "LiberalCampaignMaterials");
            AddCommodity("Liberal Federal Aid", ItemType.PowerPlay, "FederalAid");
            AddCommodity("Liberal Federal Packages", ItemType.PowerPlay, "FederalTradeContracts");
            AddCommodity("Marked Military Arms", ItemType.PowerPlay, "LoanedArms");
            AddCommodity("Patreus Field Supplies", ItemType.PowerPlay, "PatreusFieldSupplies");
            AddCommodity("Patreus Garrison Supplies", ItemType.PowerPlay, "PatreusGarisonSupplies");
            AddCommodity("Hudson's Restricted Intel", ItemType.PowerPlay, "RestrictedIntel");
            AddCommodity("Hudson's Field Supplies", ItemType.PowerPlay, "RepublicanFieldSupplies");
            AddCommodity("Hudson Garrison Supplies", ItemType.PowerPlay, "RepublicanGarisonSupplies");
            AddCommodity("Sirius Franchise Package", ItemType.PowerPlay, "SiriusFranchisePackage");
            AddCommodity("Sirius Corporate Contracts", ItemType.PowerPlay, "SiriusCommercialContracts");
            AddCommodity("Sirius Industrial Equipment", ItemType.PowerPlay, "SiriusIndustrialEquipment");
            AddCommodity("Torval Trade Agreements", ItemType.PowerPlay, "TorvalCommercialContracts");
            AddCommodity("Torval Political Prisoners", ItemType.PowerPlay, "ImperialPrisoner");
            AddCommodity("Utopian Publicity", ItemType.PowerPlay, "UtopianPublicity");
            AddCommodity("Utopian Supplies", ItemType.PowerPlay, "UtopianFieldSupplies");
            AddCommodity("Utopian Dissident", ItemType.PowerPlay, "UtopianDissident");
            AddCommodity("Kumo Contraband Package", ItemType.PowerPlay, "IllicitConsignment");
            AddCommodity("Unmarked Military supplies", ItemType.PowerPlay, "UnmarkedWeapons");
            AddCommodity("Onionhead Samples", ItemType.PowerPlay, "OnionheadSamples");
            AddCommodity("Revolutionary supplies", ItemType.PowerPlay, "CounterCultureSupport");
            AddCommodity("Onionhead Derivatives", ItemType.PowerPlay, "OnionheadDerivatives");
            AddCommodity("Out Of Date Goods", ItemType.PowerPlay, "OutOfDateGoods");
            AddCommodity("Grom Underground Support", ItemType.PowerPlay, "UndergroundSupport");
            AddCommodity("Grom Counter Intelligence", ItemType.PowerPlay, "GromCounterIntelligence");
            AddCommodity("Yuri Grom's Military Supplies", ItemType.PowerPlay, "GromWarTrophies");
            AddCommodity("Marked Slaves", ItemType.PowerPlay, "MarkedSlaves");
            AddCommodity("Torval Deeds", ItemType.PowerPlay, "TorvalDeeds");

            #endregion

            #region Microresources

            AddMicroResource(CatType.Consumable, "E-Breach", "Bypass", "MRCEB");
            AddMicroResource(CatType.Consumable, "Energy Cell", "EnergyCell", "MRCEC");
            AddMicroResource(CatType.Consumable, "Medkit", "HealthPack", "MRCM");
            AddMicroResource(CatType.Consumable, "Frag Grenade", "AMM_Grenade_Frag", "MRCFG");
            AddMicroResource(CatType.Consumable, "Shield Disruptor", "AMM_Grenade_EMP", "MRCSD");
            AddMicroResource(CatType.Consumable, "Shield Projector", "AMM_Grenade_Shield", "MRCSP");

            AddMicroResource(CatType.Item, "Power Regulator", "LargeCapacityPowerRegulator", "MRIPR");
            AddMicroResource(CatType.Item, "Compact Library", "CompactLibrary", "MRICL");
            AddMicroResource(CatType.Item, "Infinity", "infinity", "MRII");
            AddMicroResource(CatType.Item, "Insight Entertainment Suite", "InsightEntertainmentSuite", "MRIIES");
            AddMicroResource(CatType.Item, "Lazarus", "Lazarus", "MRIL");
            AddMicroResource(CatType.Item, "Universal Translator", "UniversalTranslator", "MRIUT");
            AddMicroResource(CatType.Item, "Biochemical Agent", "BiochemicalAgent", "MRIBA");
            AddMicroResource(CatType.Item, "Degraded Power Regulator", "DegradedPowerRegulator", "MRIDPR");
            AddMicroResource(CatType.Item, "Hush", "Hush", "MRIH");
            AddMicroResource(CatType.Item, "Push", "Push", "MRIP");
            AddMicroResource(CatType.Item, "Synthetic Pathogen", "SyntheticPathogen", "MRISP");
            AddMicroResource(CatType.Item, "Building Schematic", "BuildingSchematic", "MRIBS");
            AddMicroResource(CatType.Item, "Insight", "insight", "MRInsight");
            AddMicroResource(CatType.Item, "Compression-Liquefied Gas", "CompressionLiquefiedGas", "MRICLG");
            AddMicroResource(CatType.Item, "Health Monitor", "HealthMonitor", "MRIHM");
            AddMicroResource(CatType.Item, "Inertia Canister", "InertiaCanister", "MRIIC");
            AddMicroResource(CatType.Item, "Ionised Gas", "IonisedGas", "MRIIG");
            AddMicroResource(CatType.Item, "Ship Schematic", "ShipSchematic", "MRIShipSch");
            AddMicroResource(CatType.Item, "Suit Schematic", "SuitSchematic", "MRISuitSch");
            AddMicroResource(CatType.Item, "Vehicle Schematic", "VehicleSchematic", "MRIVS");
            AddMicroResource(CatType.Item, "Weapon Schematic", "WeaponSchematic", "MRIWS");
            AddMicroResource(CatType.Item, "True Form Fossil", "TrueFormFossil", "MRITFF");
            AddMicroResource(CatType.Item, "Agricultural Process Sample", "AgriculturalProcessSample", "MRIAPS");
            AddMicroResource(CatType.Item, "Biological Sample", "GeneticSample", "MRIBIOSAMP");
            AddMicroResource(CatType.Item, "Californium", "Californium", "MRIC");
            AddMicroResource(CatType.Item, "Cast Fossil", "CastFossil", "MRICF");
            AddMicroResource(CatType.Item, "Chemical Process Sample", "ChemicalProcessSample", "MRICPS");
            AddMicroResource(CatType.Item, "Chemical Sample", "ChemicalSample", "MRICS");
            AddMicroResource(CatType.Item, "Deep Mantle Sample", "DeepMantleSample", "MRIDMS");
            AddMicroResource(CatType.Item, "Genetic Repair Meds", "GeneticRepairMeds", "MRIGRM");
            AddMicroResource(CatType.Item, "G-Meds", "GMeds", "MRIGM");
            AddMicroResource(CatType.Item, "Inorganic Contaminant", "InorganicContaminant", "MRIINORGANC");
            AddMicroResource(CatType.Item, "Insight Data Bank", "InsightDataBank", "MRIIDB");
            AddMicroResource(CatType.Item, "Microbial Inhibitor", "MicrobialInhibitor", "MRIMI");
            AddMicroResource(CatType.Item, "Mutagenic Catalyst", "MutagenicCatalyst", "MRIMC");
            AddMicroResource(CatType.Item, "Nutritional Concentrate", "NutritionalConcentrate", "MRINC");
            AddMicroResource(CatType.Item, "Personal Computer", "PersonalComputer", "MRIPC");
            AddMicroResource(CatType.Item, "Personal Documents", "PersonalDocuments", "MRIPD");
            AddMicroResource(CatType.Item, "Petrified Fossil", "PetrifiedFossil", "MRIPF");
            AddMicroResource(CatType.Item, "Pyrolytic Catalyst", "PyrolyticCatalyst", "MRIPRYOCAT");
            AddMicroResource(CatType.Item, "Refinement Process Sample", "RefinementProcessSample", "MRIRPS");
            AddMicroResource(CatType.Item, "Surveillance Equipment", "SurveillanceEquipment", "MRISE");
            AddMicroResource(CatType.Item, "Synthetic Genome", "SyntheticGenome", "MRISG");

            AddMicroResource(CatType.Component, "Carbon Fibre Plating", "CarbonFibrePlating", "MRCCFP");
            AddMicroResource(CatType.Component, "Encrypted Memory Chip", "EncryptedMemoryChip", "MRCEMC");
            AddMicroResource(CatType.Component, "Epoxy Adhesive", "EpoxyAdhesive", "MRCEA");
            AddMicroResource(CatType.Component, "Graphene", "Graphene", "MRCG");
            AddMicroResource(CatType.Component, "Memory Chip", "MemoryChip", "MRCMC");
            AddMicroResource(CatType.Component, "Microelectrode", "MicroElectrode", "MRCMICROELEC");
            AddMicroResource(CatType.Component, "Micro Hydraulics", "MicroHydraulics", "MRCMH");
            AddMicroResource(CatType.Component, "Micro Thrusters", "MicroThrusters", "MRCMT");
            AddMicroResource(CatType.Component, "Optical Fibre", "OpticalFibre", "MRCOF");
            AddMicroResource(CatType.Component, "Optical Lens", "OpticalLens", "MRCOL");
            AddMicroResource(CatType.Component, "RDX", "RDX", "MRCR");
            AddMicroResource(CatType.Component, "Titanium Plating", "TitaniumPlating", "MRCTP");
            AddMicroResource(CatType.Component, "Tungsten Carbide", "TungstenCarbide", "MRCTC");
            AddMicroResource(CatType.Component, "Weapon Component", "WeaponComponent", "MRCWC");
            AddMicroResource(CatType.Component, "Aerogel", "Aerogel", "MRCA");
            AddMicroResource(CatType.Component, "Chemical Superbase", "ChemicalSuperbase", "MRCCS");
            AddMicroResource(CatType.Component, "Circuit Board", "Circuitboard", "MRCCB");
            AddMicroResource(CatType.Component, "Circuit Switch", "CircuitSwitch", "MRCCIRSWITCH");
            AddMicroResource(CatType.Component, "Electrical Wiring", "ElectricalWiring", "MRCEW");
            AddMicroResource(CatType.Component, "Electromagnet", "Electromagnet", "MRCE");
            AddMicroResource(CatType.Component, "Metal Coil", "MetalCoil", "MRCMETALCOIL");
            AddMicroResource(CatType.Component, "Motor", "Motor", "MRCMOTOR");
            AddMicroResource(CatType.Component, "Chemical Catalyst", "ChemicalCatalyst", "MRCCC");
            AddMicroResource(CatType.Component, "Micro Transformer", "Microtransformer", "MRCMICTRANS");
            AddMicroResource(CatType.Component, "Electrical Fuse", "ElectricalFuse", "MRCEF");
            AddMicroResource(CatType.Component, "Micro Supercapacitor", "MicroSupercapacitor", "MRCMS");
            AddMicroResource(CatType.Component, "Viscoelastic Polymer", "ViscoElasticPolymer", "MRCVP");
            AddMicroResource(CatType.Component, "Ion Battery", "IonBattery", "MRCIB");
            AddMicroResource(CatType.Component, "Scrambler", "Scrambler", "MRCS");
            AddMicroResource(CatType.Component, "Oxygenic Bacteria", "OxygenicBacteria", "MRCOXYBAC");
            AddMicroResource(CatType.Component, "Epinephrine", "Epinephrine", "MRCEPINE");
            AddMicroResource(CatType.Component, "pH Neutraliser", "pHNeutraliser", "MRCPHN");
            AddMicroResource(CatType.Component, "Transmitter", "Transmitter", "MRCTX");

            AddMicroResource(CatType.Data, "Chemical Inventory", "ChemicalInventory", "MRDCI");
            AddMicroResource(CatType.Data, "Duty Rota", "DutyRota", "MRDDR");
            AddMicroResource(CatType.Data, "Evacuation Protocols", "EvacuationProtocols", "MRDEP");
            AddMicroResource(CatType.Data, "Exploration Journals", "ExplorationJournals", "MRDEJ");
            AddMicroResource(CatType.Data, "Faction News", "FactionNews", "MRDFN");

            AddMicroResource(CatType.Data, "Financial Projections", "FinancialProjections", "MRDFP");
            AddMicroResource(CatType.Data, "Sales Records", "SalesRecords", "MRDSR");
            AddMicroResource(CatType.Data, "Union Membership", "UnionmemberShip", "MRDUM");
            AddMicroResource(CatType.Data, "Maintenance Logs", "MaintenanceLogs", "MRDML");
            AddMicroResource(CatType.Data, "Patrol Routes", "PatrolRoutes", "MRDPATR");

            AddMicroResource(CatType.Data, "Settlement Defence Plans", "SettlementDefencePlans", "MRDSDP");
            AddMicroResource(CatType.Data, "Surveillance Logs", "SurveilleanceLogs", "MRDSL");
            AddMicroResource(CatType.Data, "Operational Manual", "PperationalManual", "MRDOM");
            AddMicroResource(CatType.Data, "Blacklist Data", "BlacklistData", "MRDBD");
            AddMicroResource(CatType.Data, "Air Quality Reports", "AirqualityReports", "MRDAQR");


            AddMicroResource(CatType.Data, "Employee Directory", "EmployeeDirectory", "MRDED");
            AddMicroResource(CatType.Data, "Faction Associates", "FactionAssociates", "MRDFA");
            AddMicroResource(CatType.Data, "Meeting Minutes", "MeetingMinutes", "MRDMM");
            AddMicroResource(CatType.Data, "Multimedia Entertainment", "MultimediaEntertainment", "MRDME");
            AddMicroResource(CatType.Data, "Network Access History", "NetworkAccessHistory", "MRDNAH");

            AddMicroResource(CatType.Data, "Purchase Records", "PurchaseRecords", "MRDPRCD");
            AddMicroResource(CatType.Data, "Radioactivity Data", "RadioactivityData", "MRDRD");
            AddMicroResource(CatType.Data, "Residential Directory", "ResidentialDirectory", "MRDRDIR");
            AddMicroResource(CatType.Data, "Shareholder Information", "ShareholderInformation", "MRDSI");
            AddMicroResource(CatType.Data, "Travel Permits", "TravelPermits", "MRDTP");

            AddMicroResource(CatType.Data, "Accident Logs", "AccidentLogs", "MRDACCLOGS");
            AddMicroResource(CatType.Data, "Campaign Plans", "CampaignPlans", "MRDCP");
            AddMicroResource(CatType.Data, "Combat Training Material", "CombatTrainingMaterial", "MRDCTM");
            AddMicroResource(CatType.Data, "Internal Correspondence", "InternalCorrespondence", "MRDIC");
            AddMicroResource(CatType.Data, "Payroll Information", "PayrollInformation", "MRDPI");

            AddMicroResource(CatType.Data, "Personal Logs", "PersonalLogs", "MRDPL");
            AddMicroResource(CatType.Data, "Weapon Inventory", "WeaponInventory", "MRDWI");
            AddMicroResource(CatType.Data, "Atmospheric Data", "AtmosphericData", "MRDAD");
            AddMicroResource(CatType.Data, "Topographical Surveys", "TopographicalSurveys", "MRDTS");
            AddMicroResource(CatType.Data, "Literary Fiction", "LiteraryFiction", "MRDLF");


            AddMicroResource(CatType.Data, "Reactor Output Review", "ReactorOutputReview", "MRDROR");
            AddMicroResource(CatType.Data, "Next of Kin Records", "NextofkinRecords", "MRDNKR");
            AddMicroResource(CatType.Data, "Purchase Requests", "PurchaseRequests", "MRDPR");
            AddMicroResource(CatType.Data, "Tax Records", "TaxRecords", "MRDTR");
            AddMicroResource(CatType.Data, "Visitor Register", "VisitorRegister", "MRDVISREG");
            AddMicroResource(CatType.Data, "Pharmaceutical Patents", "PharmaceuticalPatents", "MRDPP");

            AddMicroResource(CatType.Data, "Vaccine Research", "VaccineResearch", "MRDVACRES");
            AddMicroResource(CatType.Data, "Virology Data", "VirologyData", "MRDVD");
            AddMicroResource(CatType.Data, "Vaccination Records", "VaccinationRecords", "MRDVR");
            AddMicroResource(CatType.Data, "Census Data", "CensusData", "MRDCD");

            AddMicroResource(CatType.Data, "Mineral Survey", "MineralSurvey", "MRDMS");
            AddMicroResource(CatType.Data, "Chemical Formulae", "ChemicalFormulae", "MRDCF");
            AddMicroResource(CatType.Data, "Chemical Experiment Data", "ChemicalExperimentData", "MRDCED");
            AddMicroResource(CatType.Data, "Chemical Patents", "ChemicalPatents", "MRDCHEMPAT");
            AddMicroResource(CatType.Data, "Production Reports", "ProductionReports", "MRDPRODREP");

            AddMicroResource(CatType.Data, "Production Schedule", "ProductionSchedule", "MRDPS");
            AddMicroResource(CatType.Data, "Blood Test Results", "BloodtestResults", "MRDBTR");
            AddMicroResource(CatType.Data, "Combatant Performance", "CombatantPerformance", "MRDCOMBPERF");
            AddMicroResource(CatType.Data, "Troop Deployment Records", "TroopDeploymentRecords", "MRDTDR");

            AddMicroResource(CatType.Data, "Cat Media", "CatMedia", "MRDCATMED");
            AddMicroResource(CatType.Data, "Employee Genetic Data", "EmployeeGeneticData", "MRDEGD");
            AddMicroResource(CatType.Data, "Faction Donator List", "FactionDonatorList", "MRDFDL");
            AddMicroResource(CatType.Data, "NOC Data", "NOCData", "MRDNOCD");
            AddMicroResource(CatType.Data, "Manufacturing Instructions", "ManufacturingInstructions", "MRDMI");

            AddMicroResource(CatType.Data, "Propaganda", "Propaganda", "MRPROPG");
            AddMicroResource(CatType.Data, "Security Expenses", "SecurityExpenses", "MRSECEXP");
            AddMicroResource(CatType.Data, "Weapon Test Data", "WeaponTestData", "MRWTD");
            AddMicroResource(CatType.Data, "Audio Logs", "AudioLogs", "MRDAL");

            AddMicroResource(CatType.Data, "AX Combat Logs", "AXCombatLogs", "MRDACL");
            AddMicroResource(CatType.Data, "Ballistics Data", "BallisticsData", "MRDBALD");
            AddMicroResource(CatType.Data, "Biological Weapon Data", "BiologicalWeaponData", "MRDBWD");
            AddMicroResource(CatType.Data, "Biometric Data", "BiometricData", "MRDBIOD");

            AddMicroResource(CatType.Data, "Chemical Weapon Data", "ChemicalWeaponData", "MRDCWD");
            AddMicroResource(CatType.Data, "Classic Entertainment", "ClassicEntertainment", "MRDCE");
            AddMicroResource(CatType.Data, "Cocktail Recipes", "CocktailRecipes", "MRDCREC");
            AddMicroResource(CatType.Data, "Conflict History", "ConflictHistory", "MRDCH");
            AddMicroResource(CatType.Data, "Criminal Records", "CriminalRecords", "MRDCRIMREC");
            AddMicroResource(CatType.Data, "Crop Yield Analysis", "CropYieldAnalysis", "MRDCYA");
            AddMicroResource(CatType.Data, "Culinary Recipes", "CulinaryRecipes", "MRDCULREC");
            AddMicroResource(CatType.Data, "Digital Designs", "DigitalDesigns", "MRDDD");
            AddMicroResource(CatType.Data, "Employee Expenses", "EmployeeExpenses", "MRDEE");
            AddMicroResource(CatType.Data, "Employment History", "EmploymentHistory", "MRDEH");
            AddMicroResource(CatType.Data, "Enhanced Interrogation Recordings", "EnhancedInterrogationRecordings", "MRDEIR");
            AddMicroResource(CatType.Data, "Espionage Material", "EspionageMaterial", "MRDEM");
            AddMicroResource(CatType.Data, "Extraction Yield Data", "ExtractionYieldData", "MRDEYD");
            AddMicroResource(CatType.Data, "Fleet Registry", "FleetRegistry", "MRDFR");
            AddMicroResource(CatType.Data, "Gene Sequencing Data", "GeneSequencingData", "MRDGSD");
            AddMicroResource(CatType.Data, "Genetic Research", "GeneticResearch", "MRDGR");
            AddMicroResource(CatType.Data, "Geological Data", "GeologicalData", "MRDGD");
            AddMicroResource(CatType.Data, "Hydroponic Data", "HydroponicData", "MRDHD");
            AddMicroResource(CatType.Data, "Incident Logs", "IncidentLogs", "MRDIL");
            AddMicroResource(CatType.Data, "Influence Projections", "InfluenceProjections", "MRDIP");
            AddMicroResource(CatType.Data, "Interrogation Recordings", "InterrogationRecordings", "MRDINTERREC");
            AddMicroResource(CatType.Data, "Interview Recordings", "InterviewRecordings", "MRDINTERVREC");
            AddMicroResource(CatType.Data, "Job Applications", "JobApplications", "MRDJA");
            AddMicroResource(CatType.Data, "Kompromat", "Kompromat", "MRDK");
            AddMicroResource(CatType.Data, "Medical Records", "MedicalRecords", "MRDMR");
            AddMicroResource(CatType.Data, "Clinical Trial Records", "MedicalTrialRecords", "MRDCTR");
            AddMicroResource(CatType.Data, "Mining Analytics", "MiningAnalytics", "MRDMA");
            AddMicroResource(CatType.Data, "Network Security Protocols", "NetworkSecurityProtocols", "MRDNSP");
            AddMicroResource(CatType.Data, "Opinion Polls", "OpinionPolls", "MRDOP");
            AddMicroResource(CatType.Data, "Patient History", "PatientHistory", "MRDPH");
            AddMicroResource(CatType.Data, "Photo Albums", "PhotoAlbums", "MRDPHOTO");
            AddMicroResource(CatType.Data, "Plant Growth Charts", "PlantGrowthCharts", "MRDPGC");
            AddMicroResource(CatType.Data, "Political Affiliations", "PoliticalAffiliations", "MRDPOL");
            AddMicroResource(CatType.Data, "Prisoner Logs", "PrisonerLogs", "MRDPRISONL");
            AddMicroResource(CatType.Data, "Recycling Logs", "RecyclingLogs", "MRDRL");
            AddMicroResource(CatType.Data, "Risk Assessments", "RiskAssessments", "MRDRA");
            AddMicroResource(CatType.Data, "Seed Geneaology", "SeedGeneaology", "MRDSG");
            AddMicroResource(CatType.Data, "Settlement Assault Plans", "SettlementAssaultPlans", "MRDSAP");
            AddMicroResource(CatType.Data, "Slush Fund Logs", "SlushFundLogs", "MRDSFL");
            AddMicroResource(CatType.Data, "Smear Campaign Plans", "SmearCampaignPlans", "MRDSCP");
            AddMicroResource(CatType.Data, "Spectral Analysis Data", "SpectralAnalysisData", "MRDSAD");
            AddMicroResource(CatType.Data, "Spyware", "Spyware", "MRDS");
            AddMicroResource(CatType.Data, "Stellar Activity Logs", "StellarActivityLogs", "MRDSAL");
            AddMicroResource(CatType.Data, "Tactical Plans", "TacticalPlans", "MRDTACP");
            AddMicroResource(CatType.Data, "VIP Security Detail", "VIPSecurityDetail", "MRDVSD");
            AddMicroResource(CatType.Data, "Virus", "Virus", "MRDV");
            AddMicroResource(CatType.Data, "Xeno-Defence Protocols", "XenoDefenceProtocols", "MRDXDP");

            // not in frontier spreadsheet, but seen in alpha logs
            AddMicroResource(CatType.Data, "Geographical Data", "GeographicalData", "MRDGEOD");

            #endregion


            AddCommodity("Drones", ItemType.Drones, "Drones");

            int cmmds = cachelist.Where(x => x.Value.IsCommodity).Count();
            int mats = cachelist.Where(x => x.Value.IsMaterial).Count();
            int micro = cachelist.Where(x => x.Value.IsMicroResources).Count();
            System.Diagnostics.Debug.WriteLine($"Commds {cmmds} Mats {mats} MRs {micro}");

            foreach (var x in cachelist.Values)
            {
                x.Name = x.Name.TxID(typeof(MaterialCommodityMicroResourceType), x.FDName);
            }

            //var cachecopy = new Dictionary<string, MaterialCommodityMicroResourceType>();

            //foreach (var x in cachelist.OrderBy(x=>x.Value.Category).ThenBy(x => x.Value.Rarity).ThenBy(x=>x.Value.Type).ThenBy(x => x.Value.MaterialGroup).ThenBy(x => x.Value.FDName))
            //{
            //    cachecopy.Add(x.Key,x.Value);
            //}

            //foreach (var x in cachecopy)
            //{
            //    if (x.Value.Rarity)
            //    {
            //        System.Diagnostics.Debug.Assert(x.Value.Shortname.Length == 0);
            //        System.Diagnostics.Debug.WriteLine($"Add(CatType.{x.Value.Category},ItemType.{x.Value.Type},MCMR.{x.Value.FDNameUC},\"{x.Value.EnglishName}\",true);"); // P1
            //    }
            //    else if (x.Value.MaterialGroup != MaterialGroupType.NA)
            //    { // P2
            //        System.Diagnostics.Debug.WriteLine($"Add(CatType.{x.Value.Category},ItemType.{x.Value.Type},MaterialGroupType.{x.Value.MaterialGroup},MCMR.{x.Value.FDNameUC},\"{x.Value.EnglishName}\",\"{x.Value.Shortname}\");");
            //    }
            //    else if (x.Value.Type != ItemType.Unknown)
            //    {
            //        if ( x.Value.Shortname.HasChars() )
            //        { // p3
            //            System.Diagnostics.Debug.WriteLine($"Add(CatType.{x.Value.Category},ItemType.{x.Value.Type},MCMR.{x.Value.FDNameUC},\"{x.Value.EnglishName}\",\"{x.Value.Shortname}\");"); // p3
            //        }
            //        else
            //            System.Diagnostics.Debug.WriteLine($"Add(CatType.{x.Value.Category},ItemType.{x.Value.Type},MCMR.{x.Value.FDNameUC},\"{x.Value.EnglishName}\");");  // P1

            //    }
            //    else if (x.Value.Shortname.Length > 0)
            //        System.Diagnostics.Debug.WriteLine($"Add(CatType.{x.Value.Category},MCMR.{x.Value.FDNameUC},\"{x.Value.EnglishName}\",\"{x.Value.Shortname}\");"); // p4
            //    else
            //        System.Diagnostics.Debug.Assert(false);

            //}

            foreach (var x in cachelist.Values)
            {
                if (mcmrlist.TryGetValue(x.FDName, out MaterialCommodityMicroResourceType value))
                {
                    System.Diagnostics.Debug.Assert(x.Category == value.Category);
                    System.Diagnostics.Debug.Assert(x.Type == value.Type);
                    System.Diagnostics.Debug.Assert(x.EnglishName == value.EnglishName);
                    System.Diagnostics.Debug.Assert(x.Rarity == value.Rarity);
                    System.Diagnostics.Debug.Assert(x.MaterialGroup == value.MaterialGroup);
                    System.Diagnostics.Debug.Assert(x.Shortname == value.Shortname);
                    System.Diagnostics.Debug.Assert(x.Colour == value.Colour);
                }
                else
                    System.Diagnostics.Debug.Assert(false);
            }

            cachelist = mcmrlist;

            //var lastcat = CatType.Component;
            //int ii = 0;
            //foreach (var x in cachecopy)
            //{
            //    if (lastcat != x.Value.Category)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"\n//---------------------------------------------------------- {x.Value.Category}");
            //        lastcat = x.Value.Category;
            //        ii = 0;
            //        System.Diagnostics.Debug.Write($"{x.Value.FDNameUC}={((int)x.Value.Category) * 1000},");
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.Write($"{x.Value.FDNameUC},");

            //    }

            //    if (ii++ % 8 == 7)
            //        System.Diagnostics.Debug.WriteLine("");

            //}


            // foreach (MaterialCommodityData d in cachelist.Values) System.Diagnostics.Debug.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", d.Category, d.Type.ToString().SplitCapsWord(), d.MaterialGroup.ToString(), d.FDName, d.Name, d.Shortname, d.Rarity ));
            //  }
        }


        public static Dictionary<string, string> fdnamemangling = new Dictionary<string, string>() // Key: old_identifier, Value: new_identifier
        {
            //2.2 to 2.3 changed some of the identifier names.. change the 2.2 ones to 2.3!  Anthor data from his materials db file

            // July 2018 - removed many, changed above, to match FD 3.1 excel output - we use their IDs.  Netlogentry frontierdata checks these..

            { "aberrantshieldpatternanalysis"       ,  "shieldpatternanalysis" },
            { "adaptiveencryptorscapture"           ,  "adaptiveencryptors" },
            { "alyabodysoap"                        ,  "alyabodilysoap" },
            { "anomalousbulkscandata"               ,  "bulkscandata" },
            { "anomalousfsdtelemetry"               ,  "fsdtelemetry" },
            { "atypicaldisruptedwakeechoes"         ,  "disruptedwakeechoes" },
            { "atypicalencryptionarchives"          ,  "encryptionarchives" },
            { "azuremilk"                           ,  "bluemilk" },
            { "cd-75kittenbrandcoffee"              ,  "cd75catcoffee" },
            { "crackedindustrialfirmware"           ,  "industrialfirmware" },
            { "dataminedwakeexceptions"             ,  "dataminedwake" },
            { "distortedshieldcyclerecordings"      ,  "shieldcyclerecordings" },
            { "eccentrichyperspacetrajectories"     ,  "hyperspacetrajectories" },
            { "edenapplesofaerial"                  ,  "aerialedenapple" },
            { "eraninpearlwhiskey"                  ,  "eraninpearlwhisky" },
            { "exceptionalscrambledemissiondata"    ,  "scrambledemissiondata" },
            { "inconsistentshieldsoakanalysis"      ,  "shieldsoakanalysis" },
            { "kachiriginfilterleeches"             ,  "kachiriginleaches" },
            { "korokungpellets"                     ,  "korrokungpellets" },
            { "leatheryeggs"                        ,  "alieneggs" },
            { "lucanonionhead"                      ,  "transgeniconionhead" },
            { "modifiedconsumerfirmware"            ,  "consumerfirmware" },
            { "modifiedembeddedfirmware"            ,  "embeddedfirmware" },
            { "opensymmetrickeys"                   ,  "symmetrickeys" },
            { "peculiarshieldfrequencydata"         ,  "shieldfrequencydata" },
            { "rajukrumulti-stoves"                 ,  "rajukrustoves" },
            { "sanumadecorativemeat"                ,  "sanumameat" },
            { "securityfirmwarepatch"               ,  "securityfirmware" },
            { "specialisedlegacyfirmware"           ,  "legacyfirmware" },
            { "strangewakesolutions"                ,  "wakesolutions" },
            { "taggedencryptioncodes"               ,  "encryptioncodes" },
            { "unidentifiedscanarchives"            ,  "scanarchives" },
            { "unusualencryptedfiles"               ,  "encryptedfiles" },
            { "utgaroarmillennialeggs"              ,  "utgaroarmillenialeggs" },
            { "xihebiomorphiccompanions"            ,  "xihecompanions" },
            { "zeesszeantgrubglue"                  ,  "zeesszeantglue" },

            {"micro-weavecoolinghoses","coolinghoses"},
            {"energygridassembly","powergridassembly"},

            {"methanolmonohydrate","methanolmonohydratecrystals"},
            {"muonimager","mutomimager"},
            {"hardwarediagnosticsensor","diagnosticsensor"},

        };

        static public string FDNameTranslation(string old)
        {
            old = old.ToLowerInvariant();
            if (fdnamemangling.ContainsKey(old))
            {
                //System.Diagnostics.Debug.WriteLine("Sub " + old);
                return fdnamemangling[old];
            }
            else
                return old;
        }

#endregion
    }
}


