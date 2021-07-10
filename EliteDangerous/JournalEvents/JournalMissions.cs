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
using EliteDangerousCore.DB;
using BaseUtils.JSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Missions)]
    public class JournalMissions : JournalEntry, IMissions
    {
        public JournalMissions(JObject evt) : base(evt, JournalTypeEnum.Missions)
        {
            ActiveMissions = evt["Active"]?.ToObjectQ<MissionItem[]>();
            Normalise(ActiveMissions);
            FailedMissions = evt["Failed"]?.ToObjectQ<MissionItem[]>();
            Normalise(FailedMissions);
            CompletedMissions = evt["Complete"]?.ToObjectQ<MissionItem[]>();
            Normalise(CompletedMissions);
        }

        public MissionItem[] ActiveMissions { get; set; }
        public MissionItem[] FailedMissions { get; set; }
        public MissionItem[] CompletedMissions { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("Active: ".T(EDTx.JournalEntry_Active), ActiveMissions?.Length, "Failed: ".T(EDTx.JournalEntry_Failed), FailedMissions?.Length, "Completed: ".T(EDTx.JournalEntry_Completed), CompletedMissions?.Length);
            detailed = "";
            if (ActiveMissions != null && ActiveMissions.Length>0)
            {
                detailed = detailed.AppendPrePad("Active: ".T(EDTx.JournalEntry_Active), Environment.NewLine);
                foreach (var x in ActiveMissions)
                    detailed = detailed.AppendPrePad("    " + x.Format(), Environment.NewLine);
            }
            if (FailedMissions != null && FailedMissions.Length>0)
            {
                detailed = detailed.AppendPrePad("Failed: ".T(EDTx.JournalEntry_Failed), Environment.NewLine);
                foreach (var x in FailedMissions)
                    detailed = detailed.AppendPrePad("    " + x.Format(), Environment.NewLine);
            }
            if (CompletedMissions != null && CompletedMissions.Length > 0)
            {
                detailed = detailed.AppendPrePad("Completed: ".T(EDTx.JournalEntry_Completed), Environment.NewLine);
                foreach (var x in CompletedMissions)
                    detailed = detailed.AppendPrePad("    " + x.Format(), Environment.NewLine);
            }

        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Missions(this);       // check vs our mission list
        }


        public void Normalise(MissionItem[] array)
        {
            if (array != null)
                foreach (var x in array)
                    x.Normalise(EventTimeUTC);
        }

        public class MissionItem
        {
            public ulong MissionID;  
            public string Name;
            public bool PassengerMission;
            public int Expires;

            DateTime ExpiryTimeUTC;

            public void Normalise(DateTime utcnow)
            {
                ExpiryTimeUTC = utcnow.AddSeconds(Expires);
                Name = JournalFieldNaming.GetBetterMissionName(Name);       // Names are normalised, per MissionAccepted
            }

            public string Format()
            {
                return BaseUtils.FieldBuilder.Build("", Name, "<;(Passenger)".T(EDTx.MissionItem_Passenger), PassengerMission, " " + "Expires: ".T(EDTx.MissionItem_Expires), ExpiryTimeUTC.ToLocalTime());
            }
        }
    }


    [JournalEntryType(JournalTypeEnum.MissionAccepted)]
    public class JournalMissionAccepted : JournalEntry, IMissions, ICommodityJournalEntry
    {
        public JournalMissionAccepted(JObject evt) : base(evt, JournalTypeEnum.MissionAccepted)
        {
            Faction = evt["Faction"].Str();
            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.GetBetterMissionName(FDName);
            LocalisedName = JournalFieldNaming.CheckLocalisation(evt["LocalisedName"].Str(),Name); 

            TargetType = evt["TargetType"].Str();
            TargetTypeFriendly = JournalFieldNaming.GetBetterTargetTypeName(TargetType);    // remove $, underscore it
            TargetTypeLocalised = JournalFieldNaming.CheckLocalisation(evt["TargetType_Localised"].Str(), TargetTypeFriendly);

            TargetFaction = evt["TargetFaction"].Str();

            Target = evt["Target"].Str();
            TargetFriendly = JournalFieldNaming.GetBetterTargetTypeName(Target);        // remove $, underscore it
            TargetLocalised = JournalFieldNaming.CheckLocalisation(evt["Target_Localised"].Str(), TargetFriendly);        // not all

            KillCount = evt["KillCount"].IntNull();

            DestinationSystem = evt["DestinationSystem"].Str().Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", ",")
                                                              .Replace("$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;", ",");       // multi missions get this strange list;
            DestinationStation = evt["DestinationStation"].Str();

            Influence = evt["Influence"].Str();
            Reputation = evt["Reputation"].Str();

            MissionId = evt["MissionID"].ULong();

            Commodity = JournalFieldNaming.FixCommodityName(evt["Commodity"].Str());        // instances of $_name, fix to fdname
            FriendlyCommodity = MaterialCommodityMicroResourceType.GetNameByFDName(Commodity);
            CommodityLocalised = JournalFieldNaming.CheckLocalisationTranslation(evt["Commodity_Localised"].Str(), FriendlyCommodity);

            Count = evt["Count"].IntNull();
            Expiry = evt["Expiry"].DateTimeUTC();

            PassengerCount = evt["PassengerCount"].IntNull();
            PassengerVIPs = evt["PassengerVIPs"].BoolNull();
            PassengerWanted = evt["PassengerWanted"].BoolNull();
            PassengerType = evt["PassengerType"].StrNull();

            Reward = evt["Reward"].IntNull();   // not in DOC V13, but present in latest journal entries

            Wing = evt["Wing"].BoolNull();      // new 3.02

        }

        public ulong MissionId { get; private set; }

        public string Faction { get; private set; }                 // in MissionAccepted order
        public string Name { get; private set; }
        public string LocalisedName { get; private set; }
        public string FDName { get; private set; }

        public string DestinationSystem { get; private set; }
        public string DestinationStation { get; private set; }

        public string TargetType { get; private set; }
        public string TargetTypeFriendly { get; private set; }
        public string TargetTypeLocalised { get; private set; }
        public string TargetFaction { get; private set; }
        public string Target { get; private set; }
        public string TargetFriendly { get; private set; }
        public string TargetLocalised { get; private set; }     // not all.. only for radars etc.
        public int? KillCount { get; private set; }

        public DateTime Expiry { get; private set; }            // MARKED as 2000 if not there..
        public bool ExpiryValid { get { return Expiry.Year >= 2014; } }

        public string Influence { get; private set; }
        public string Reputation { get; private set; }

        public string Commodity { get; private set; }               //fdname, this is for delivery missions, stuff being transported
        public string CommodityLocalised { get; private set; }
        public string FriendlyCommodity { get; private set; }       //db name
        public int? Count { get; private set; }

        public int? PassengerCount { get; private set; }            // for passenger missions
        public bool? PassengerVIPs { get; private set; }
        public bool? PassengerWanted { get; private set; }
        public string PassengerType { get; private set; }

        public int? Reward { get; private set; }

        public bool? Wing { get; private set; }     // 3.02

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = MissionBasicInfo(true);
            detailed = MissionDetailedInfo(true);
        }

        public string MissionBasicInfo(bool translate)          // MissionList::FullInfo uses this. Journal Entry info brief uses this
        {
            DateTime exp = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(Expiry);

            return BaseUtils.FieldBuilder.Build("", LocalisedName,
                                      EDTranslatorExtensions.TCond(translate, "< from ", EDTx.JournalMissionAccepted_from), Faction,
                                      EDTranslatorExtensions.TCond(translate, "System: ",EDTx.JournalEntry_System ), DestinationSystem,
                                      EDTranslatorExtensions.TCond(translate, "Station: ",EDTx.JournalEntry_Station ), DestinationStation,
                                      EDTranslatorExtensions.TCond(translate, "Expiry: ",EDTx.JournalMissionAccepted_Expiry ), exp,
                                      EDTranslatorExtensions.TCond(translate, "Influence: ",EDTx.JournalMissionAccepted_Influence ), Influence,
                                      EDTranslatorExtensions.TCond(translate, "Reputation: ",EDTx.JournalMissionAccepted_Reputation ), Reputation,
                                      EDTranslatorExtensions.TCond(translate, "Reward: ; cr;N0",EDTx.JournalMissionAccepted_Reward ), Reward,
                                      EDTranslatorExtensions.TCond(translate, "; (Wing)",EDTx.JournalMissionAccepted_Wing ), Wing);
        }

        public string MissionDetailedInfo(bool translate)          // MissionList::FullInfo (DLL uses this), Journal Entry detailed info
        {
            return BaseUtils.FieldBuilder.Build(
                                           EDTranslatorExtensions.TCond(translate, "Deliver: ", EDTx.JournalMissionAccepted_Deliver ), CommodityLocalised,
                                           EDTranslatorExtensions.TCond(translate, "Count: ", EDTx.JournalMissionAccepted_Count ), Count,
                                           EDTranslatorExtensions.TCond(translate, "Target: ", EDTx.JournalEntry_Target ), TargetLocalised,
                                           EDTranslatorExtensions.TCond(translate, "Type: ", EDTx.JournalEntry_Type ), TargetTypeFriendly,
                                           EDTranslatorExtensions.TCond(translate, "Target Faction: ", EDTx.JournalEntry_TargetFaction ), TargetFaction,
                                           EDTranslatorExtensions.TCond(translate, "Target Type: ", EDTx.JournalMissionAccepted_TargetType ), TargetTypeLocalised,
                                           EDTranslatorExtensions.TCond(translate, "Kill Count: ", EDTx.JournalMissionAccepted_KillCount ), KillCount,
                                           EDTranslatorExtensions.TCond(translate, "Passengers: ", EDTx.JournalMissionAccepted_Passengers ), PassengerCount);
        }

        public string MissionInfoColumn()          //  MissionList:info, used for MissionList:Info, used in mission panels.
        {
            return BaseUtils.FieldBuilder.Build(
                                        "Influence: ".T(EDTx.JournalMissionAccepted_Influence), Influence,
                                        "Reputation: ".T(EDTx.JournalMissionAccepted_Reputation), Reputation,
                                        "Deliver: ".T(EDTx.JournalMissionAccepted_Deliver), CommodityLocalised,
                                        "Target: ".T(EDTx.JournalEntry_Target), TargetLocalised,
                                        "Type: ".T(EDTx.JournalEntry_Type), TargetTypeFriendly,
                                        "Target Faction: ".T(EDTx.JournalEntry_TargetFaction), TargetFaction,
                                        "Target Type: ".T(EDTx.JournalMissionAccepted_TargetType), TargetTypeLocalised,
                                        "Passengers: ".T(EDTx.JournalMissionAccepted_Passengers), PassengerCount,
                                        "Count: ".T(EDTx.JournalMissionAccepted_Count), Count);

        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Accepted(this, sys, body);
        }

        private static List<string> DeliveryMissions = new List<string>()
        {
            "Mission_Delivery",
            "Chain_HelpFinishTheOrder",
        };

        private static DateTime ED32Date = new DateTime(2018, 8, 28, 10, 0, 0);

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            if (Commodity != null && Count != null && EventTimeUTC < ED32Date)           // after this we will rely on Cargo to update us, only safe way to know if something has been stowed
            {
                if (DeliveryMissions.StartsWith(FDName, StringComparison.InvariantCultureIgnoreCase)>=0 )   // before, we accept only these as mission deliveries
                {
                    mc.Change(EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, Commodity, (int)Count, 0);
                }
                else
                {
                 //   System.Diagnostics.Debug.WriteLine("{0} Rejected {1} {2} {3}", EventTimeUTC, FDName, Commodity, Count);
                }
            }
        }
    }

    [JournalEntryType(JournalTypeEnum.MissionCompleted)]
    public class JournalMissionCompleted : JournalEntry, ICommodityJournalEntry, IMaterialJournalEntry, ILedgerJournalEntry, IMissions
    {
        public JournalMissionCompleted(JObject evt) : base(evt, JournalTypeEnum.MissionCompleted)
        {
            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.GetBetterMissionName(FDName);
            Faction = evt["Faction"].Str();

            Commodity = JournalFieldNaming.FixCommodityName(evt["Commodity"].Str());             // evidence of $_name problem, fix to fdname
            Commodity = JournalFieldNaming.FDNameTranslation(Commodity);     // pre-mangle to latest names, in case we are reading old journal records
            FriendlyCommodity = MaterialCommodityMicroResourceType.GetNameByFDName(Commodity);
            CommodityLocalised = JournalFieldNaming.CheckLocalisationTranslation(evt["Commodity_Localised"].Str(), FriendlyCommodity);

            Count = evt["Count"].IntNull();

            TargetType = evt["TargetType"].Str();
            TargetTypeFriendly = JournalFieldNaming.GetBetterTargetTypeName(TargetType);        // remove $, underscores etc
            TargetTypeLocalised = JournalFieldNaming.CheckLocalisation(evt["TargetTypeLocalised"].Str(), TargetTypeFriendly);     // may be empty..

            TargetFaction = evt["TargetFaction"].Str();

            Target = evt["Target"].Str();
            TargetFriendly = JournalFieldNaming.GetBetterTargetTypeName(Target);        // remove $, underscores etc
            TargetLocalised = JournalFieldNaming.CheckLocalisation(evt["Target_Localised"].Str(), TargetFriendly);        // copied from Accepted.. no evidence

            Reward = evt["Reward"].LongNull();
            JToken dtk = evt["Donation"];
            if ( dtk != null )
            {
                if (dtk.IsString)      // some records have donations as strings incorrectly
                    Donation = dtk.Str().InvariantParseLongNull();
                else
                    Donation = dtk.LongNull();
            }

            MissionId = evt["MissionID"].ULong();

            DestinationSystem = evt["DestinationSystem"].Str().Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", ",")
                                                              .Replace("$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;", ",");       // multi missions get this strange list;
            DestinationStation = evt["DestinationStation"].Str();

            PermitsAwarded = evt["PermitsAwarded"]?.ToObjectQ<string[]>();

            // 7/3/2018 journal 16 3.02

            CommodityReward = evt["CommodityReward"]?.ToObjectQ<CommodityRewards[]>();

            if (CommodityReward != null)
            {
                foreach (CommodityRewards c in CommodityReward)
                    c.Normalise();
            }

            MaterialsReward = evt["MaterialsReward"]?.ToObjectQ<MaterialRewards[]>();

            if (MaterialsReward != null)
            {
                foreach (MaterialRewards m in MaterialsReward)
                    m.Normalise();
            }

            FactionEffects = evt["FactionEffects"]?.ToObjectQ<FactionEffectsEntry[]>();      // NEEDS TEST
        }

        public string Name { get; set; }
        public string FDName { get; set; }
        public string Faction { get; set; }

        public string Commodity { get; set; }               // The thing shipped. But in pre3.0, this could also be a commodity reward, which was not clear.
        public string CommodityLocalised { get; set; }
        public string FriendlyCommodity { get; set; }
        public int? Count { get; set; }

        public string Target { get; set; }
        public string TargetLocalised { get; set; }
        public string TargetFriendly { get; set; }
        public string TargetType { get; set; }
        public string TargetTypeLocalised { get; set; }
        public string TargetTypeFriendly { get; set; }
        public string TargetFaction { get; set; }

        public string DestinationSystem { get; set; }
        public string DestinationStation { get; set; }

        public long? Reward { get; set; }
        public long? Donation { get; set; }
        public string[] PermitsAwarded { get; set; }
        public ulong MissionId { get; set; }

        public CommodityRewards[] CommodityReward { get; set; }
        public MaterialRewards[] MaterialsReward { get; set; }

        public FactionEffectsEntry[] FactionEffects;

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            // removed update on Commodity/Count (which is unreliable about if you should remove them from cargo)
            // rely on Cargo to update stats, its emitted directly after MissionCompleted.

            if (CommodityReward != null)
            {
                foreach (CommodityRewards c in CommodityReward)
                    mc.Change( EventTimeUTC, MaterialCommodityMicroResourceType.CatType.Commodity, c.Name, c.Count, 0);    // commodities are traded by faction
            }
        }

        public void UpdateMaterials(MaterialCommoditiesMicroResourceList mc)
        {
            if (MaterialsReward != null)
            {
                foreach (MaterialRewards m in MaterialsReward)                 // 7/3/2018 not yet fully proven.. changed in 3.02
                {
                    mc.Change( EventTimeUTC, m.Category.Alt("Raw"), m.Name, m.Count, 0);      // mats from faction of mission
                }
            }
        }

        public void Ledger(Ledger mcl)
        {
            long rv = Reward.HasValue ? Reward.Value : 0;
            long dv = Donation.HasValue ? Donation.Value : 0;

            mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Name, (rv - dv));
        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Completed(this);
        }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {

            info = BaseUtils.FieldBuilder.Build("", Name,
                                        "< from ".T(EDTx.JournalEntry_from), Faction,
                                        "Reward: ; cr;N0".T(EDTx.JournalEntry_Reward), Reward,
                                        "Donation: ".T(EDTx.JournalEntry_Donation), Donation,
                                        "System: ".T(EDTx.JournalEntry_System), DestinationSystem,
                                        "Station: ".T(EDTx.JournalEntry_Station), DestinationStation);

            detailed = BaseUtils.FieldBuilder.Build("Commodity: ".T(EDTx.JournalEntry_Commodity), CommodityLocalised,
                                            "Count: ".T(EDTx.JournalMissionAccepted_Count), Count,
                                            "Target: ".T(EDTx.JournalEntry_Target), TargetLocalised,
                                            "Type: ".T(EDTx.JournalEntry_Type), TargetTypeLocalised,
                                            "Target Faction: ".T(EDTx.JournalEntry_TargetFaction), TargetFaction);

            detailed = detailed.AppendPrePad(RewardInformation(true),Environment.NewLine);

            detailed= detailed.ReplaceIfEndsWith(Environment.NewLine, "");

        }

        public string RewardInformation(bool translate)          
        {
            return PermitsList(translate, true) + CommoditiesList(translate, true) + MaterialList(translate, true) + FactionEffectsList(translate,true);
        }

        public string PermitsList(bool translate, bool pretty)
        {
            string detailed = "";
            if (PermitsAwarded != null && PermitsAwarded.Length > 0)
            {
                if (pretty)
                    detailed += EDTranslatorExtensions.TCond(translate,"Permits: ",EDTx.JournalEntry_Permits);

                for (int i = 0; i < PermitsAwarded.Length; i++)
                    detailed += ((i > 0) ? "," : "") + PermitsAwarded[i];

                if (pretty)
                    detailed += System.Environment.NewLine;
            }
            return detailed;
        }

        public string CommoditiesList(bool translate , bool pretty )
        {
            string detailed = "";
            if (CommodityReward != null && CommodityReward.Length > 0)
            {
                if (pretty)
                    detailed += EDTranslatorExtensions.TCond(translate,"Rewards: ",EDTx.JournalEntry_Rewards);

                for (int i = 0; i < CommodityReward.Length; i++)
                {
                    CommodityRewards c = CommodityReward[i];
                    detailed += ((i > 0) ? "," : "") + c.Name_Localised + " " + CommodityReward[i].Count.ToString();
                }

                if (pretty)
                    detailed += System.Environment.NewLine;
            }
            return detailed;
        }

        public string MaterialList(bool translate, bool pretty)
        {
            string detailed = "";
            if (MaterialsReward != null && MaterialsReward.Length > 0)
            {
                if (pretty)
                    detailed += EDTranslatorExtensions.TCond(translate, "Rewards: ", EDTx.JournalEntry_Rewards);

                for (int i = 0; i < MaterialsReward.Length; i++)
                {
                    MaterialRewards m = MaterialsReward[i];
                    detailed += ((i > 0) ? "," : "") + m.Name_Localised + " " + MaterialsReward[i].Count.ToString();
                }

                if (pretty)
                    detailed += System.Environment.NewLine;
            }
            return detailed;
        }

        public string FactionEffectsList(bool translate, bool pretty)
        {
            string detailed = "";
            if (FactionEffects != null && FactionEffects.Length>0)
            {
                for (int i = 0; i < FactionEffects.Length; i++)
                {
                    detailed += FactionEffects[i].Faction + " " + FactionEffects[i].ReputationTrend + " " + FactionEffects[i].Reputation;

                    string effects = "";
                    foreach (var x in FactionEffects[i].Effects)
                    {
                        effects = effects.AppendPrePad(x.Effect.Replace("$MISSIONUTIL_","").Replace(";","") + " " + x.Trend,",");
                    }

                    string influence = "";
                    foreach (var x in FactionEffects[i].Influence)
                    {
                        influence = influence.AppendPrePad(x.Trend + " " + x.Influence,",");
                    }

                    string inf = "";
                    if (effects.HasChars())
                    {
                        inf += "E: " + effects;

                        if (influence.HasChars())
                           inf += "; ";
                    }

                    if (influence.HasChars())
                        inf += "I: " + influence;

                    if ( inf.HasChars() )
                        detailed += " {" + inf + "}";

                    if ( pretty )    
                        detailed += Environment.NewLine;
                }
            }

            return detailed;
        }

        public bool HasReceivedReward(string fdname)
        {
            var m = MaterialsReward != null && Array.Find(MaterialsReward, (x) => x.Name.Equals(fdname, StringComparison.InvariantCultureIgnoreCase)) != null;
            var c = CommodityReward != null && Array.Find(CommodityReward, (x) => x.Name.Equals(fdname, StringComparison.InvariantCultureIgnoreCase)) != null;
            return m || c;
        }

        public string RewardOrDonation { get { return Reward.HasValue ? Reward.Value.ToString("N0") : (Donation.HasValue ? (-Donation.Value).ToString("N0") : ""); } }
        public long Value { get { return Reward.HasValue ? Reward.Value : (Donation.HasValue ? (-Donation.Value) : 0); } }

        public class MaterialRewards
        {
            public string Name; // fdname
            public string FriendlyName; // our conversion
            public string Name_Localised;       // may be null on reading
            public string Category; // may be null
            public string Category_Localised; // may be null
            public int Count;

            public void Normalise()
            {
                Name = JournalFieldNaming.FDNameTranslation(Name);
                FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
                Name_Localised = JournalFieldNaming.CheckLocalisationTranslation(Name_Localised ?? "", FriendlyName);

                if (Category != null)
                {
                    Category = JournalFieldNaming.NormaliseMaterialCategory(Category);
                    Category_Localised = JournalFieldNaming.CheckLocalisation(Category_Localised ?? "", Category);
                }
            }
        }

        public class CommodityRewards
        {
            public string Name; // fdname
            public string FriendlyName; // our conversion
            public string Name_Localised;   // may be null
            public int Count;

            public void Normalise()
            {
                Name = JournalFieldNaming.FDNameTranslation(Name);
                FriendlyName = MaterialCommodityMicroResourceType.GetNameByFDName(Name);
                Name_Localised = Name_Localised.Alt(FriendlyName);
            }
        }

        public class EffectTrend
        {
            public string Effect;
            public string Effect_Localised;
            public string Trend;
        }

        public class InfluenceTrend
        {
            public long SystemAddress;
            public string Trend;
            public string Influence; // not in very early ones
        }

        public class FactionEffectsEntry
        {
            public string Faction;
            public EffectTrend[] Effects;
            public InfluenceTrend[] Influence;
            public string Reputation;
            public string ReputationTrend;
        }
    }


    [JournalEntryType(JournalTypeEnum.MissionFailed)]
    public class JournalMissionFailed : JournalEntry, IMissions
    {
        public JournalMissionFailed(JObject evt) : base(evt, JournalTypeEnum.MissionFailed)
        {
            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.GetBetterMissionName(FDName);
            MissionId = evt["MissionID"].ULong();
            Fine = evt["Fine"].LongNull();
        }

        public string Name { get; set; }
        public string FDName { get; set; }
        public ulong MissionId { get; set; }
        public long? Fine { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {

            info = BaseUtils.FieldBuilder.Build("", Name, "Fine: ".T(EDTx.JournalEntry_Fine), Fine);
            detailed = "";
        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Failed(this);
        }

    }


    [JournalEntryType(JournalTypeEnum.MissionRedirected)]
    public class JournalMissionRedirected : JournalEntry, IMissions
    {
        public JournalMissionRedirected(JObject evt) : base(evt, JournalTypeEnum.MissionRedirected)
        {
            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.GetBetterMissionName(FDName);
            MissionId = evt["MissionID"].ULong();
            NewDestinationStation = evt["NewDestinationStation"].Str();
            OldDestinationStation = evt["OldDestinationStation"].Str();
            NewDestinationSystem = evt["NewDestinationSystem"].Str();
            OldDestinationSystem = evt["OldDestinationSystem"].Str();
        }

        public string NewDestinationStation { get; set; }
        public string OldDestinationStation { get; set; }
        public string NewDestinationSystem { get; set; }
        public string OldDestinationSystem { get; set; }

        public ulong MissionId { get; set; }
        public string Name { get; set; }
        public string FDName { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = info = BaseUtils.FieldBuilder.Build("Mission name: ".T(EDTx.JournalEntry_Missionname), Name,
                                      "From: ".T(EDTx.JournalMissionRedirected_From), OldDestinationSystem,
                                      "", OldDestinationStation,
                                      "To: ".T(EDTx.JournalMissionRedirected_To), NewDestinationSystem,
                                      "", NewDestinationStation);

            detailed = "";
        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Redirected(this);
        }

    }



    [JournalEntryType(JournalTypeEnum.MissionAbandoned)]
    public class JournalMissionAbandoned : JournalEntry, IMissions
    {
        public JournalMissionAbandoned(JObject evt) : base(evt, JournalTypeEnum.MissionAbandoned)
        {
            FDName = evt["Name"].Str();
            Name = JournalFieldNaming.GetBetterMissionName(FDName);
            MissionId = evt["MissionID"].ULong();
            Fine = evt["Fine"].LongNull();
        }

        public string Name { get; set; }
        public string FDName { get; set; }
        public ulong MissionId { get; set; }
        public long? Fine { get; set; }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("", Name, "Fine: ".T(EDTx.JournalEntry_Fine), Fine);
            detailed = "";
        }

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Abandoned(this);
        }

    }


}
