/*
 * Copyright © 2016-2021 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Linq;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.DiscoveryScan)]
    public class JournalDiscoveryScan : JournalEntry
    {
        public JournalDiscoveryScan(JObject evt) : base(evt, JournalTypeEnum.DiscoveryScan)
        {
            SystemAddress = evt["SystemAddress"].Long();
            Bodies = evt["Bodies"].Int();
        }

        public long SystemAddress { get; set; }
        public int Bodies { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("New bodies discovered: ".T(EDCTx.JournalEntry_Dscan), Bodies,
                                                "@ ", sys.Name);
            detailed = "";
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Progress} {BodyCount} {NonBodyCount}")]
    [JournalEntryType(JournalTypeEnum.FSSDiscoveryScan)]
    public class JournalFSSDiscoveryScan : JournalEntry, IStarScan
    {
        public JournalFSSDiscoveryScan(JObject evt) : base(evt, JournalTypeEnum.FSSDiscoveryScan)
        {
            Progress = evt["Progress"].Double() * 100.0;
            BodyCount = evt["BodyCount"].Int();
            NonBodyCount = evt["NonBodyCount"].Int();
        }

        public double Progress { get; set; }
        public int BodyCount { get; set; }
        public int NonBodyCount { get; set; }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.SetFSSDiscoveryScan(this, system);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Progress: ;%;N1".T(EDCTx.JournalFSSDiscoveryScan_Progress), Progress, 
                "Bodies: ".T(EDCTx.JournalFSSDiscoveryScan_Bodies), BodyCount, 
                "Others: ".T(EDCTx.JournalFSSDiscoveryScan_Others), NonBodyCount,
                "@ ", sys.Name);
            detailed = "";
        }
    }

    [System.Diagnostics.DebuggerDisplay("{SignalNames()}")]
    [JournalEntryType(JournalTypeEnum.FSSSignalDiscovered)]
    public class JournalFSSSignalDiscovered : JournalEntry, IStarScan
    {
        [System.Diagnostics.DebuggerDisplay("{ClassOfSignal} {SignalName}")]
        public class FSSSignal
        {
            [PropertyNameAttribute("Signal type string, FDName")]
            public string SignalName { get; set; }
            [PropertyNameAttribute("Signal type localised")]
            public string SignalName_Localised { get; set; }
            [PropertyNameAttribute("Spawing state")]
            public string SpawningState { get; set; }
            [PropertyNameAttribute("Signal state localised")]
            public string SpawningState_Localised { get; set; }
            [PropertyNameAttribute("Signal faction, FDName")]
            public string SpawningFaction { get; set; }
            [PropertyNameAttribute("Signal faction, Localised")]
            public string SpawningFaction_Localised { get; set; }
            [PropertyNameAttribute("Optional time remaining seconds")]
            public double? TimeRemaining { get; set; }          // null if not expiring
            [PropertyNameAttribute("Optional Frontier system address")]
            public long? SystemAddress { get; set; }

            [PropertyNameAttribute("Is it a station")]
            public bool? IsStation { get; set; }
            
            [PropertyNameAttribute("Threat level")]
            public int? ThreatLevel { get; set; }
            [PropertyNameAttribute("Optional USS Type, FDName")]
            public string USSType { get; set; }
            [PropertyNameAttribute("Optional USS Type, Localised")]
            public string USSTypeLocalised { get; set; }

            [PropertyNameAttribute("When signal was recorded")]
            public System.DateTime RecordedUTC { get; set; }        // when it was recorded

            [PropertyNameAttribute("Optional signal expiry time, UTC")]
            public System.DateTime ExpiryUTC { get; set; }
            [PropertyNameAttribute("Optional signal expiry time, Local")]
            public System.DateTime ExpiryLocal { get; set; }

            public enum Classification { Station,Installation, NotableStellarPhenomena, ConflictZone, ResourceExtraction, Carrier, USS, Other};
            [PropertyNameAttribute("Signal class")]
            public Classification ClassOfSignal { get; set; }

            const int CarrierExpiryTime = 10 * (60 * 60 * 24);              // days till we consider the carrier signal expired..

            public FSSSignal(JObject evt, System.DateTime EventTimeUTC)
            {
                SignalName = evt["SignalName"].Str();
                string loc = evt["SignalName_Localised"].Str();     // not present for stations/installations
                SignalName_Localised = loc.Alt(SignalName);         // don't mangle if no localisation, its prob not there because its a proper name

                SpawningState = evt["SpawningState"].Str();          // USS only, checked
                SpawningState_Localised = JournalFieldNaming.CheckLocalisation(evt["SpawningState_Localised"].Str(), SpawningState);

                SpawningFaction = evt["SpawningFaction"].Str();      // USS only, checked
                SpawningFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["SpawningFaction_Localised"].Str(), SpawningFaction);

                USSType = evt["USSType"].Str();                     // USS Only, checked
                USSTypeLocalised = JournalFieldNaming.CheckLocalisation(evt["USSType_Localised"].Str(), USSType);

                ThreatLevel = evt["ThreatLevel"].IntNull();         // USS only, checked

                TimeRemaining = evt["TimeRemaining"].DoubleNull();  // USS only, checked

                SystemAddress = evt["SystemAddress"].LongNull();

                IsStation = evt["IsStation"].BoolNull();

                if (IsStation == true)          // station flag
                {
                    int dash = SignalName.LastIndexOf('-');
                    if (SignalName.Length >= 5 && dash == SignalName.Length - 4 && char.IsLetterOrDigit(SignalName[dash + 1]) && char.IsLetterOrDigit(SignalName[dash - 1]))
                    {
                        ClassOfSignal = Classification.Carrier;
                        TimeRemaining = CarrierExpiryTime;
                    }
                    else
                        ClassOfSignal = Classification.Station;
                }
                else if (loc.Length == 0 )      // other types, and old station entries, don't have localisation, so its an installation
                    ClassOfSignal = Classification.Installation;
                else if (SignalName.StartsWith("$USS", StringComparison.InvariantCultureIgnoreCase) || SignalName.StartsWith("$RANDOM", StringComparison.InvariantCultureIgnoreCase))
                    ClassOfSignal = Classification.USS;
                else if (SignalName.StartsWith("$Warzone", StringComparison.InvariantCultureIgnoreCase))
                    ClassOfSignal = Classification.ConflictZone;
                else if (SignalName.StartsWith("$Fixed_Event_Life", StringComparison.InvariantCultureIgnoreCase))
                    ClassOfSignal = Classification.NotableStellarPhenomena;
                else if (SignalName.StartsWith("$MULTIPLAYER_SCENARIO14", StringComparison.InvariantCultureIgnoreCase) || SignalName.StartsWith("$MULTIPLAYER_SCENARIO7", StringComparison.InvariantCultureIgnoreCase))
                    ClassOfSignal = Classification.ResourceExtraction;
                else
                    ClassOfSignal = Classification.Other;

                RecordedUTC = EventTimeUTC;

                if (TimeRemaining != null)
                {
                    ExpiryUTC = EventTimeUTC.AddSeconds(TimeRemaining.Value);
                    ExpiryLocal = ExpiryUTC.ToLocalTime();
                }
            }

            public bool IsSame(FSSSignal other)     // is this signal the same as the other one
            {
                return SignalName.Equals(other.SignalName) && SpawningFaction.Equals(other.SpawningFaction) && SpawningState.Equals(other.SpawningState) &&
                       USSType.Equals(other.USSType) && ThreatLevel == other.ThreatLevel && ClassOfSignal == other.ClassOfSignal &&
                       (ClassOfSignal == Classification.Carrier || ExpiryUTC == other.ExpiryUTC);       // note carriers have our own expiry on it, so we don't
            }

            public string ToString( bool showseentime)
            {
                DateTime? outoftime = null;
                if (TimeRemaining != null && ClassOfSignal != Classification.Carrier)       // ignore carrier timeout for printing
                    outoftime = ExpiryLocal;

                DateTime? seen = null;
                if (showseentime && ClassOfSignal == Classification.Carrier)
                    seen = RecordedUTC;

                string signname = ClassOfSignal == Classification.USS ? null : SignalName_Localised;        // signal name for USS is boring, remove

                string spstate = SpawningState_Localised != null ? SpawningState_Localised.Truncate(0, 32, "..") : null;

                return BaseUtils.FieldBuilder.Build(
                            ";Station: ".T(EDCTx.FSSSignal_StationBool), ClassOfSignal == Classification.Station,
                            ";Carrier: ".T(EDCTx.FSSSignal_CarrierBool), ClassOfSignal == Classification.Carrier,
                            ";Installation: ".T(EDCTx.FSSSignal_InstallationBool), ClassOfSignal == Classification.Installation,
                            "<", signname,
                            "", USSTypeLocalised,
                            "Threat Level: ".T(EDCTx.FSSSignal_ThreatLevel), ThreatLevel,
                            "Faction: ".T(EDCTx.FSSSignal_Faction), SpawningFaction_Localised,
                            "State: ".T(EDCTx.FSSSignal_State), spstate,
                            "Time: ".T(EDCTx.JournalEntry_Time), outoftime,
                            "Last Seen: ".T(EDCTx.FSSSignal_LastSeen), seen
                            );
            }
        }

        public JournalFSSSignalDiscovered(JObject evt) : base(evt, JournalTypeEnum.FSSSignalDiscovered)
        {
            Signals = new List<FSSSignal>();
            Signals.Add(new FSSSignal(evt, EventTimeUTC));
        }

        public void Add(JournalFSSSignalDiscovered next )
        {
            Signals.Add(next.Signals[0]);
        }

        private string SignalNames() { return string.Join(",", Signals?.Select(x => x.SignalName)); }       // for debugger

        [PropertyNameAttribute("List of FSS signals")]
        public List<FSSSignal> Signals { get; set; }            // name used in action packs not changeable

        public ISystem EDDNSystem { get; set; }                 // set if FSS has been detected in the wrong system                  

        [PropertyNameAttribute("Count of station signals")]
        public int CountStationSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.Station).Count() ?? 0; } }
        [PropertyNameAttribute("Count of installation signals")]
        public int CountInstallationSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.Installation).Count() ?? 0; } }
        [PropertyNameAttribute("Count of NSP signals")]
        public int CountNotableStellarPhenomenaSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.NotableStellarPhenomena).Count() ?? 0; } }
        [PropertyNameAttribute("Count of conflict zone signals")]
        public int CountConflictZoneSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.ConflictZone).Count() ?? 0; } }
        [PropertyNameAttribute("Count of extraction zone signals")]
        public int CountResourceExtractionZoneSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.ResourceExtraction).Count() ?? 0; } }
        [PropertyNameAttribute("Count of carrier signals")]
        public int CountCarrierSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.Carrier).Count() ?? 0; } }
        [PropertyNameAttribute("Count of USS signals")]
        public int CountUSSSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.USS).Count() ?? 0; } }
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.ClassOfSignal == FSSSignal.Classification.Other).Count() ?? 0; } }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddFSSSignalsDiscoveredToSystem(this);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            FillInformation(sys, whereami, 20, out info, out detailed);
        }

        public void FillInformation(ISystem sys, string whereami, int maxsignals, out string info, out string detailed)
        {
            detailed = "";

            if (Signals.Count > 1)
            {
                info = BaseUtils.FieldBuilder.Build("Detected ; signals".T(EDCTx.JournalFSSSignalDiscovered_Detected), Signals.Count);

                if (Signals.Count < maxsignals)
                {
                    foreach (var s in Signals)
                    {
                        if (s.SignalName.StartsWith("$USS_"))
                            info += ", " + s.USSTypeLocalised;
                        else
                            info += ", " + s.SignalName_Localised;
                    }
                }

                info = info.AppendPrePad("@ " + sys.Name, ", ");

                foreach (var s in Signals)
                    detailed = detailed.AppendPrePad(s.ToString(false), System.Environment.NewLine);
            }
            else
            {
                info = Signals[0].ToString(false);
            }
        }

        static public List<FSSSignal> SignalList( List<JournalFSSSignalDiscovered> jsd)      // from a set of jsd's, extract all signals which are not the same
        {
            List<FSSSignal> list = new List<FSSSignal>();
            foreach( var j in jsd)
            {
                foreach( var s in j.Signals)
                {
                    int present = list.FindIndex(x => x.IsSame(s));
                    if (present == -1)
                        list.Add(s);
                }
            }

            return list;
        }
    }


    [System.Diagnostics.DebuggerDisplay("{NumBodies}")]
    [JournalEntryType(JournalTypeEnum.NavBeaconScan)]
    public class JournalNavBeaconScan : JournalEntry
    {
        public JournalNavBeaconScan(JObject evt) : base(evt, JournalTypeEnum.NavBeaconScan)
        {
            NumBodies = evt["NumBodies"].Int();
            SystemAddress = evt["SystemAddress"].LongNull();
        }

        public int NumBodies { get; set; }
        public long? SystemAddress { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Bodies: ".T(EDCTx.JournalEntry_Bodies), NumBodies);
            detailed = "";
        }
    }

    [System.Diagnostics.DebuggerDisplay("{BodyName} {BodyID} {ProbesUsed} {EfficiencyTarget}")]
    [JournalEntryType(JournalTypeEnum.SAAScanComplete)]
    public class JournalSAAScanComplete : JournalEntry, IStarScan
    {
        public JournalSAAScanComplete(JObject evt) : base(evt, JournalTypeEnum.SAAScanComplete) // event came in about 12/12/18
        {
            BodyName = evt["BodyName"].Str();
            BodyID = evt["BodyID"].Int();
            ProbesUsed = evt["ProbesUsed"].Int();
            EfficiencyTarget = evt["EfficiencyTarget"].Int();
            SystemAddress = evt["SystemAddress"].LongNull();        // Early ones did not have it (before 11/12/19)
        }

        public int BodyID { get; set; }
        public string BodyName { get; set; }
        public int ProbesUsed { get; set; }
        public int EfficiencyTarget { get; set; }
        public long? SystemAddress { get; set; }    // 3.5

        public void AddStarScan(StarScan s, ISystem system)     // no action in this class, historylist.cs does the adding itself instead of using this. 
        {                                                       // Class interface is marked so you know its part of the gang
        }

        public override string SummaryName(ISystem sys)
        {
            return base.SummaryName(sys) + " " + "of ".T(EDCTx.JournalEntry_ofa) + BodyName.ReplaceIfStartsWith(sys.Name);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            string name = BodyName.Contains(sys.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : sys.Name + ":" + BodyName;
            info = BaseUtils.FieldBuilder.Build("Probes: ".T(EDCTx.JournalSAAScanComplete_Probes), ProbesUsed,
                                                "Efficiency Target: ".T(EDCTx.JournalSAAScanComplete_EfficiencyTarget), EfficiencyTarget,
                                                "@ ", name);
            detailed = "";
        }
    }

    [System.Diagnostics.DebuggerDisplay("{BodyName} {BodyID} {SignalNames()}")]
    [JournalEntryType(JournalTypeEnum.SAASignalsFound)]
    public class JournalSAASignalsFound : JournalEntry, IStarScan, IBodyNameIDOnly
    {
        public JournalSAASignalsFound(JObject evt) : base(evt, JournalTypeEnum.SAASignalsFound)
        {
            SystemAddress = evt["SystemAddress"].Long();
            BodyName = evt["BodyName"].Str();
            BodyID = evt["BodyID"].Int();
            Signals = evt["Signals"].ToObjectQ<List<SAASignal>>();
            if (Signals != null)
            {
                foreach (var s in Signals)      // some don't have localisation
                {
                    s.Type_Localised = JournalFieldNaming.CheckLocalisation(s.Type_Localised, s.Type.Replace("$SAA_","").SplitCapsWordFull());
                }
            }
            Genuses = evt["Genuses"].ToObjectQ<List<SAAGenus>>();
            if (Genuses != null)
            {
                foreach (var g in Genuses)      // some don't have localisation
                {
                    g.Genus_Localised = JournalFieldNaming.CheckLocalisation(g.Genus_Localised, g.Genus.Replace("_Name;","").Replace("$Codex_","").SplitCapsWordFull());
                }
            }
        }

        [PropertyNameAttribute("Frontier system address")]
        public long SystemAddress { get; set; }
        [PropertyNameAttribute("Body name")]
        public string BodyName { get; set; }
        [PropertyNameAttribute("Frontier body ID")]
        public int? BodyID { get; set; }        // acutally always set, set to ? to correspond to previous journal event types where BodyID may be missing
        [PropertyNameAttribute("List of signals")]
        public List<SAASignal> Signals { get; set; }
        [PropertyNameAttribute("List of Genus (4.0v13+)")]
        public List<SAAGenus> Genuses { get; set; }
        [PropertyNameAttribute("Does it have geo signals")]
        public bool ContainsGeoSignals { get { return Signals?.Count(x => x.IsGeo) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have bio signals")]
        public bool ContainsBioSignals { get { return Signals?.Count(x => x.IsBio) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have thargoid signals")]
        public bool ContainsThargoidSignals { get { return Signals?.Count(x => x.IsThargoid) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have guardian signals")]
        public bool ContainsGuardianSignals { get { return Signals?.Count(x => x.IsGuardian) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have human signals")]
        public bool ContainsHumanSignals { get { return Signals?.Count(x => x.IsHuman) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have other signals")]
        public bool ContainsOtherSignals { get { return Signals?.Count(x => x.IsOther) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have uncategorised signals")]
        public bool ContainsUncategorisedSignals { get { return Signals?.Count(x => x.IsUncategorised) > 0 ? true : false; } }

        [PropertyNameAttribute("Count of geo signals")]
        public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of bio signals")]
        public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of thargoid signals")]
        public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of guardian signals")]
        public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of human signals")]
        public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of uncategorised signals")]
        public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }

        [System.Diagnostics.DebuggerDisplay("{Type} {Count}")]
        public class SAASignal 
        {
            [PropertyNameAttribute("Signal type string, FDName")]
            public string Type { get; set; }        // material fdname, or $SAA_SignalType..
            [PropertyNameAttribute("Signal type string, localised")]
            public string Type_Localised { get; set; }
            [PropertyNameAttribute("Count of signals")]
            public int Count { get; set; }

            [PropertyNameAttribute("Is geo signal")]
            public bool IsGeo { get { return Type.Contains("$SAA_SignalType_Geological;"); } }
            [PropertyNameAttribute("Is bio signal")]
            public bool IsBio { get { return Type.Contains("$SAA_SignalType_Biological;"); } }
            [PropertyNameAttribute("Is thargoid signal")]           // note Anonmaly is associated with thargoid interactions
            public bool IsThargoid { get { return Type.Contains("$SAA_SignalType_Thargoid;") || Type.Contains("$SAA_SignalType_PlanetAnomaly;"); } }
            [PropertyNameAttribute("Is guardian signal")]
            public bool IsGuardian { get { return Type.Contains("$SAA_SignalType_Guardian;"); } }
            [PropertyNameAttribute("Is human signal")]
            public bool IsHuman { get { return Type.Contains("$SAA_SignalType_Human;"); } }
            [PropertyNameAttribute("Is other signal")]
            public bool IsOther { get { return Type.Contains("$SAA_SignalType_Other;"); } }
            [PropertyNameAttribute("Is uncategorised signal")]
            public bool IsUncategorised { get { return !Type.Contains("$SAA_SignalType"); } }       // probably a material, but you can never tell with FD
        }

        [System.Diagnostics.DebuggerDisplay("{Genus} {Genus_Localised}")]
        public class SAAGenus
        {
            [PropertyNameAttribute("Genus type string, FDName")]
            public string Genus { get; set; }        // $Codex_Ent_Bacterial_Genus_Name;
            [PropertyNameAttribute("Genus type string, localised")]
            public string Genus_Localised { get; set; }
        }

        public override string SummaryName(ISystem sys)
        {
            return base.SummaryName(sys) + " " + "of ".T(EDCTx.JournalEntry_ofa) + BodyName.ReplaceIfStartsWith(sys.Name);
        }


        private string SignalNames() { return string.Join(",", Signals?.Select(x => x.Type)); }       // for debugger

        static public string SignalList(List<SAASignal> list, int indent = 0, string separ = ", ", bool logtype = false)
        {
            string inds = new string(' ', indent);

            string info = "";
            if (list != null)
            {
                foreach (var x in list)
                {
                    info = info.AppendPrePad(inds + (logtype ? x.Type : x.Type_Localised.Alt(x.Type)) + ": " + x.Count.ToString("N0"), separ);
                }
            }

            return info;
        }
        static public string GenusList(List<SAAGenus> list, int indent = 0, string separ = ", ", bool logtype = false)
        {
            string inds = new string(' ', indent);

            string info = "";
            if (list != null)
            {
                foreach (var x in list)
                {
                    info = info.AppendPrePad(inds + (logtype ? x.Genus : x.Genus_Localised.Alt(x.Genus)), separ);
                }
            }

            return info;
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = SignalList(Signals);
            string name = BodyName.Contains(sys.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : sys.Name + ":" + BodyName;
            info = info.AppendPrePad("@ " + name, ", ");
            if (Genuses != null)
                info = info.AppendPrePad(GenusList(Genuses), "; ");
            detailed = "";
        }

        public int Contains(string fdname)      // give count if contains fdname, else zero
        {
            int index = Signals?.FindIndex((x) => x.Type.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase)) ?? -1;
            return (index >= 0) ? Signals[index].Count : 0;
        }

        public string ContainsStr(string fdname, bool showit = true)      // give count if contains fdname, else empty string
        {
            int contains = Contains(fdname);
            return showit && contains > 0 ? contains.ToStringInvariant() : "";
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddSAASignalsFoundToBestSystem(this, system);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{SystenName} {Count}")]
    [JournalEntryType(JournalTypeEnum.FSSAllBodiesFound)]
    public class JournalFSSAllBodiesFound : JournalEntry
    {
        public JournalFSSAllBodiesFound(JObject evt) : base(evt, JournalTypeEnum.FSSAllBodiesFound)
        {
            SystemName = evt["SystemName"].Str();
            SystemAddress = evt["SystemAddress"].Long();
            Count = evt["Count"].Int();
        }

        public long SystemAddress { get; set; }
        public string SystemName { get; set; }
        public int Count { get; set; }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = Count.ToString() + " @ " + SystemName;
            detailed = "";
        }
    }

    [System.Diagnostics.DebuggerDisplay("{BodyName} {BodyID} {SignalNames()}")]
    [JournalEntryType(JournalTypeEnum.FSSBodySignals)]
    public class JournalFSSBodySignals : JournalEntry, IStarScan, IBodyNameIDOnly
    {
        public JournalFSSBodySignals(JObject evt) : base(evt, JournalTypeEnum.FSSBodySignals)
        {
            SystemAddress = evt["SystemAddress"].Long();
            BodyName = evt["BodyName"].Str();
            BodyID = evt["BodyID"].Int();
            Signals = evt["Signals"].ToObjectQ<List<JournalSAASignalsFound.SAASignal>>();
            if (Signals != null)
            {
                foreach (var s in Signals)      // some don't have localisation
                {
                    s.Type_Localised = JournalFieldNaming.CheckLocalisation(s.Type_Localised, s.Type.SplitCapsWordFull());
                }
            }
        }

        [PropertyNameAttribute("Frontier system address")]
        public long SystemAddress { get; set; }
        [PropertyNameAttribute("Body name")]
        public string BodyName { get; set; }
        [PropertyNameAttribute("Frontier body ID")]
        public int? BodyID { get; set; }        // acutally always set, set to ? to correspond to previous journal event types where BodyID may be missing
        [PropertyNameAttribute("List of signals")]
        public List<JournalSAASignalsFound.SAASignal> Signals { get; set; }

        [PropertyNameAttribute("Does it have geo signals")]
        public bool ContainsGeoSignals { get { return Signals?.Count(x => x.IsGeo) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have bio signals")]
        public bool ContainsBioSignals { get { return Signals?.Count(x => x.IsBio) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have thargoid signals")]
        public bool ContainsThargoidSignals { get { return Signals?.Count(x => x.IsThargoid) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have guardian signals")]
        public bool ContainsGuardianSignals { get { return Signals?.Count(x => x.IsGuardian) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have human signals")]
        public bool ContainsHumanSignals { get { return Signals?.Count(x => x.IsHuman) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have other signals")]
        public bool ContainsOtherSignals { get { return Signals?.Count(x => x.IsOther) > 0 ? true : false; } }
        [PropertyNameAttribute("Does it have uncategorised signals")]
        public bool ContainsUncategorisedSignals { get { return Signals?.Count(x => x.IsUncategorised) > 0 ? true : false; } }

        [PropertyNameAttribute("Count of geo signals")]
        public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of bio signals")]
        public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of thargoid signals")]
        public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of guardian signals")]
        public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of human signals")]
        public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
        [PropertyNameAttribute("Count of uncategorised signals")]
        public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddFSSBodySignalsToSystem(this,system);
        }

        private string SignalNames() { return string.Join(",", Signals?.Select(x => x.Type)); }       // for debugger

        public override string SummaryName(ISystem sys)
        {
            return base.SummaryName(sys) + " " + "of ".T(EDCTx.JournalEntry_ofa) + BodyName.ReplaceIfStartsWith(sys.Name);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            info = JournalSAASignalsFound.SignalList(Signals);
            string name = BodyName.Contains(sys.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : sys.Name + ":" + BodyName;
            info = info.AppendPrePad("@ " + name, ", ");
            detailed = "";
        }

    }

    [System.Diagnostics.DebuggerDisplay("{Body} {Genus} {Species}")]
    [JournalEntryType(JournalTypeEnum.ScanOrganic)]
    public class JournalScanOrganic : JournalEntry, IStarScan
    {
        public JournalScanOrganic(JObject evt) : base(evt, JournalTypeEnum.ScanOrganic)
        {
            evt.ToObjectProtected(this.GetType(), true, false, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly, this);        // read fields named in this structure matching JSON names

            Species = Species.Alt("Unknown");       // seen entries with no genus/species, set to unknown.
            Genus = Genus.Alt("Unknown");

            if (ScanType == ScanTypeEnum.Analyse)
            {
                var value = OrganicEstimatedValues.GetValue(Species);
                if (value != null)
                    EstimatedValue = value.Value;
            }
        }

        [PropertyNameAttribute("Frontier internal system address")]
        public long SystemAddress { get; set; }
        [PropertyNameAttribute("Internal Frontier ID")]
        public int Body { get; set; }
        [PropertyNameAttribute("Frontier Genus ID")]
        public string Genus { get; set; }
        [PropertyNameAttribute("Genus in localised text")]
        public string Genus_Localised { get; set; }
        [PropertyNameAttribute("Frontier Species ID")]
        public string Species { get; set; }
        [PropertyNameAttribute("Species in localised text")]
        public string Species_Localised { get; set; }
        public enum ScanTypeEnum { Log, Sample, Analyse };
        [PropertyNameAttribute("Log type")]
        public ScanTypeEnum ScanType { get; set; }     //Analyse, Log, Sample
        [PropertyNameAttribute("cr")]
        public int? EstimatedValue { get; set; }       // Value, if known

        public void AddStarScan(StarScan s, ISystem system)
        {
            //System.Diagnostics.Debug.WriteLine($"Add ScanOrganic {ScanType} {Genus_Localised} {Species_Localised}");
            s.AddScanOrganicToSystem(this,system);
        }

        public override void FillInformation(ISystem sys, string whereami, out string info, out string detailed)
        {
            int? ev = ScanType == ScanTypeEnum.Analyse ? EstimatedValue : null;
            info = BaseUtils.FieldBuilder.Build("", ScanType.ToString(), "<: ", Genus_Localised, "", Species_Localised, "; cr;N0", ev, "< @ ", whereami);
            detailed = "";
        }

        // this sorts the list by date/time, then runs the algorithm that returns only the latest sample state for each key
        // Note that if you don't complete a log-sample-sample-analyse, and do another log, then that previous one gets wiped

        static public List<Tuple<string,JournalScanOrganic>> SortList(List<JournalScanOrganic> list)
        {
            list.Sort(delegate (JournalScanOrganic l, JournalScanOrganic r)     // get it in time order
            {
                return (l.EventTimeUTC.CompareTo(r.EventTimeUTC));
            });

            Dictionary<string, Tuple<string, JournalScanOrganic>> stage = new Dictionary<string, Tuple<string, JournalScanOrganic>>();

            string currentkey = null;
            foreach( var so in list)
            {
                var key = so.Genus + ":" + so.Species;
                if (currentkey == null || currentkey == key)
                {
                }
                else if (currentkey != key)     // changed type, remove any which are not at analyse
                {
                    List<string> toremove = new List<string>();
                    foreach( var kvp in stage)
                    {
                        if (kvp.Value.Item2.ScanType != ScanTypeEnum.Analyse)
                            toremove.Add(kvp.Key);
                    }

                    foreach (var k in toremove)
                        stage.Remove(k);
                }

                currentkey = key;
                string c = ((int)so.ScanType + 1).ToString();
                if (stage.ContainsKey(key) && stage[key].Item2.ScanType == ScanTypeEnum.Sample && so.ScanType == ScanTypeEnum.Sample)
                    c = "2+";
                stage[key] = new Tuple<string,JournalScanOrganic>(c,so);        // should go log, sample, sample,analyse
            }

            return stage.Values.ToList();
        }

        static public string OrganicList(List<JournalScanOrganic> list, int indent = 0, string separ = null)        // default is environment.newline
        {
            var listsorted = SortList(list);
            string inds = new string(' ', indent);
            string res = "";

            foreach (var t in listsorted)
            {
                var s = t.Item2;
                //System.Diagnostics.Debug.WriteLine($"{s.ScanType} {s.Genus_Localised} {s.Species_Localised}");
                res = res.AppendPrePad(inds + BaseUtils.FieldBuilder.Build(";/3", t.Item1, "", s.ScanType, "<: ", s.Genus_Localised, "", s.Species_Localised), separ ?? Environment.NewLine);
            }

            return res;
        }

    }

}
