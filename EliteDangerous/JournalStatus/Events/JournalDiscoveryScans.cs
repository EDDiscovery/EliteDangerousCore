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

        public override string GetInfo(FillInformationData fid)
        {
            return BaseUtils.FieldBuilder.Build("New bodies discovered".Tx()+": ", Bodies,
                                                "@ ", fid.System.Name);
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
            SystemAddress = evt["SystemAddress"].LongNull();        // appeared later
            SystemName = evt["SystemName"].StrNull();               // appeared later
        }

        public double Progress { get; set; }
        public int BodyCount { get; set; }
        public int NonBodyCount { get; set; }
        public string SystemName { get; set; }      // not always present, may be null
        public long? SystemAddress { get; set; }
        public void AddStarScan(StarScan s, ISystem system)
        {
            s.SetFSSDiscoveryScan(BodyCount, NonBodyCount, system);
        }

        public override string GetInfo(FillInformationData fid)
        {
            return BaseUtils.FieldBuilder.Build("Progress: ;%;N1".Tx(), Progress, 
                "Bodies".Tx()+": ", BodyCount, 
                "Others".Tx()+": ", NonBodyCount,
                "@ ", fid.System.Name);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{SignalNames()}")]
    [JournalEntryType(JournalTypeEnum.FSSSignalDiscovered)]
    public class JournalFSSSignalDiscovered : JournalEntry, IStarScan, IIdentifiers
    {
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
        public List<FSSSignal> Signals { get; set; }            // name used in action packs not changeable. Never null 

        // JSON export ZMQ, DLL, Web via JournalScan

        [JsonIgnore]
        public ISystem EDDNSystem { get; set; }                 // set if FSS has been detected in the wrong system                  

        [JsonIgnore]
        [PropertyNameAttribute("Count of station signals")]
        public int CountStationSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Station).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of installation signals")]
        public int CountInstallationSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Installation).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of NSP signals")]
        public int CountNotableStellarPhenomenaSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.NotableStellarPhenomena).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of conflict zone signals")]
        public int CountConflictZoneSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.ConflictZone).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of extraction zone signals")]
        public int CountResourceExtractionZoneSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.ResourceExtraction).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of carrier signals")]
        public int CountCarrierSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Carrier).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of USS signals")]
        public int CountUSSSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.USS).Count() ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Other).Count() ?? 0; } }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddFSSSignalsDiscoveredToSystem(this);
        }

        public override string GetInfo(FillInformationData fid)
        {
            const int maxsignals = 20;

            var sb = new System.Text.StringBuilder(1024);

            if (fid.NextJumpSystemName != null)
            {
                sb.Append("@ ");
                sb.Append(fid.NextJumpSystemName);
                sb.Append(": ");
            }

            if (Signals.Count > 1)
            {
                sb.Build("Detected ; signals".Tx(), Signals.Count);

                // resort the list, when first printed, it will reorder this, then it will be a low power operation
                FSSSignal.Sort(Signals);

                if (Signals.Count < maxsignals)
                {
                    foreach (var s in Signals)
                    {
                        if (s.ClassOfSignal == SignalDefinitions.Classification.USS)
                            sb.AppendPrePadCS(s.USSTypeLocalised);
                        else
                            sb.AppendPrePadCS(s.SignalName_Localised);
                    }
                }
            }
            else
            {
                sb.Append( Signals[0].ToString(false));
            }
            return sb.ToString();
        }
        public override string GetDetailed(FillInformationData fid)
        {
            if (Signals.Count > 1)
            {
                // resort the list, when first printed, it will reorder this, then it will be a low power operation
                FSSSignal.Sort(Signals);
                
                var sb = new System.Text.StringBuilder(1024);
                foreach (var s in Signals)
                    sb.AppendPrePadCR(s.ToString(false));

                return sb.ToString();
            }
            else
                return null;
        }

        // return signals, removing duplicates, and starting with the latest jsd.
        // jsd is in add order, so latest one is at end
        // expensive, only done on scan and surveyor display as of dec 22
        static public List<FSSSignal> SignalList( List<JournalFSSSignalDiscovered> jsd)
        {
            List<FSSSignal> list = new List<FSSSignal>();
            for(int i = jsd.Count-1; i>=0; i--)
            {
                var j = jsd[i];
                foreach (var s in j.Signals)
                {
                    int present = list.FindIndex(x => x.IsSame(s));
                    if (present == -1)
                    {
                        list.Add(s);
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine($"Rejected signal {s.SignalName}");
                    }
                }
            }

            return list;
        }

        public void UpdateIdentifiers()
        {
            System.Diagnostics.Debug.Assert(Signals.Count == 1);    // check we are calling this before any merger

            foreach ( var s in Signals)
            {
                if ( s.SignalName.HasChars() && s.SignalName_Localised.HasChars() )
                {
                    Identifiers.Add(s.SignalName, s.SignalName_Localised);
                }
            }
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

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Bodies".Tx()+": ", NumBodies);
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
            return base.SummaryName(sys) + " " + "of ".Tx()+ BodyName.ReplaceIfStartsWith(sys.Name);
        }

        public override string GetInfo(FillInformationData fid)
        {
            string name = BodyName.Contains(fid.System.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : fid.System.Name + ":" + BodyName;
            return BaseUtils.FieldBuilder.Build("Probes".Tx()+": ", ProbesUsed,
                                                "Efficiency Target".Tx()+": ", EfficiencyTarget,
                                                "@ ", name);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{BodyName} {BodyID} {SignalNames()}")]
    [JournalEntryType(JournalTypeEnum.SAASignalsFound)]
    public class JournalSAASignalsFound : JournalEntry, IStarScan, IBodyNameIDOnly, IIdentifiers
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
                    s.Type_Localised = JournalFieldNaming.CheckLocalisation(s.Type_Localised, JournalFieldNaming.Signals(s.Type));
                }
            }
            Genuses = evt["Genuses"].ToObjectQ<List<SAAGenus>>();
            if (Genuses != null)
            {
                foreach (var g in Genuses)      // some don't have localisation
                {
                    g.Genus_Localised = JournalFieldNaming.CheckLocalisation(g.Genus_Localised,  JournalFieldNaming.Genus(g.Genus));
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

            // JSON export ZMQ, DLL, Web via JournalScan

            [JsonIgnore]
            [PropertyNameAttribute("Is geo signal")]
            public bool IsGeo { get { return Type.Contains("$SAA_SignalType_Geological;"); } }
            [JsonIgnore]
            [PropertyNameAttribute("Is bio signal")]
            public bool IsBio { get { return Type.Contains("$SAA_SignalType_Biological;"); } }
            [JsonIgnore]
            [PropertyNameAttribute("Is thargoid signal")]           // note Anonmaly is associated with thargoid interactions
            public bool IsThargoid { get { return Type.Contains("$SAA_SignalType_Thargoid;") || Type.Contains("$SAA_SignalType_PlanetAnomaly;"); } }
            [JsonIgnore]
            [PropertyNameAttribute("Is guardian signal")]
            public bool IsGuardian { get { return Type.Contains("$SAA_SignalType_Guardian;"); } }
            [JsonIgnore]
            [PropertyNameAttribute("Is human signal")]
            public bool IsHuman { get { return Type.Contains("$SAA_SignalType_Human;"); } }
            [JsonIgnore]
            [PropertyNameAttribute("Is other signal")]
            public bool IsOther { get { return Type.Contains("$SAA_SignalType_Other;"); } }
            [JsonIgnore]
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
            return base.SummaryName(sys) + " " + "of ".Tx()+ BodyName.ReplaceIfStartsWith(sys.Name);
        }


        private string SignalNames() { return string.Join(",", Signals?.Select(x => x.Type)); }       // for debugger

        // print list, with optional indent, and separ.  Separ is not placed on last entry
        // logtype = false localised, true ID
        static public void SignalList(System.Text.StringBuilder sb, List<SAASignal> list, int indent, bool indentfirst, bool logtype, string separ = ", ")
        {
            if (list != null)
            {
                string inds = new string(' ', indent);
                int index = 0;
                foreach (var x in list)
                {
                    if (indent > 0 && (index > 0 || indentfirst))       // if indent, and its either not first or allowed to indent first
                        sb.Append(inds);

                    sb.Append(logtype ? x.Type : x.Type_Localised.Alt(x.Type));
                    sb.Append(": ");
                    sb.Append(x.Count.ToString("N0"));

                    if (index++ < list.Count - 1)     // if another to go, separ
                        sb.Append(separ);
                }
            }
        }

        static public string SignalListString(List<SAASignal> list, int indent, bool indentfirst, bool logtype, string separ = ", ")
        {
            var sb = new System.Text.StringBuilder(1024);
            SignalList(sb, list, indent, indentfirst, logtype, separ);
            return sb.ToString();
        }

        // print list, with optional indent, and separ.  Separ is not placed on last entry
        // logtype = false localised, true ID
        static public void GenusList(System.Text.StringBuilder sb, List<SAAGenus> list, int indent, bool indentfirst, bool logtype, string separ = ", ")
        {
            if (list != null)
            {
                string inds = new string(' ', indent);
                int index = 0;
                foreach (var x in list)
                {
                    if (indent > 0 && (index > 0 || indentfirst))       // if indent, and its either not first or allowed to indent first
                        sb.Append(inds);
                    sb.AppendPrePad(logtype ? x.Genus : x.Genus_Localised.Alt(x.Genus));

                    if (index++ < list.Count - 1)     // if another to go, separ
                        sb.Append(separ);
                }
            }
        }

        static public string GenusListString(List<SAAGenus> list, int indent, bool indentfirst, bool logtype, string separ = ", ")
        {
            var sb = new System.Text.StringBuilder(1024);
            GenusList(sb, list, indent, indentfirst, logtype, separ );
            return sb.ToString();
        }

        static public bool ContainsBio(List<SAASignal> list)
        {
            return list.Find(x => x.IsBio) != null;
        }
        static public bool ContainsGeo(List<SAASignal> list)
        {
            return list.Find(x => x.IsGeo) != null;
        }

        public override string GetInfo(FillInformationData fid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            SignalList(sb, Signals, 0,false,false);
            string name = BodyName.Contains(fid.System.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : fid.System.Name + ":" + BodyName;
            sb.AppendPrePad("@ " + name, ", ");
            if (Genuses != null)
                GenusList(sb, Genuses, 0, false, false);
            return sb.ToString();
        }

        public int Contains(string fdname)      // give count if contains fdname, else zero
        {
            int index = Signals?.FindIndex((x) => x.Type.Equals(fdname, System.StringComparison.InvariantCultureIgnoreCase)) ?? -1;
            return (index >= 0) ? Signals[index].Count : 0;
        }

        public object ContainsStr(string fdname, bool showit = true)      // give count if contains fdname, else empty string
        {
            int contains = Contains(fdname);
            return showit && contains > 0 ? (object)contains : "";
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddSAASignalsFoundToBestSystem(this, system);
        }

        public void UpdateIdentifiers()
        {
            foreach (var s in Signals)
            {
                if (s.Type.HasChars() && s.Type_Localised.HasChars())
                {
                    Identifiers.Add(s.Type, s.Type_Localised);
                }
            }
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

        public override string GetInfo()
        {
            return Count.ToString() + " @ " + SystemName;
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
                    s.Type_Localised = JournalFieldNaming.CheckLocalisation(s.Type_Localised, JournalFieldNaming.BodySignals(s.Type));
                }
            }
        }

        // JSON export ZMQ, DLL, Web via JournalScan

        [PropertyNameAttribute("Frontier system address")]
        public long SystemAddress { get; set; }
        [PropertyNameAttribute("Body name")]
        public string BodyName { get; set; }
        [PropertyNameAttribute("Frontier body ID")]
        public int? BodyID { get; set; }        // acutally always set, set to ? to correspond to previous journal event types where BodyID may be missing
        [PropertyNameAttribute("List of signals")]
        public List<JournalSAASignalsFound.SAASignal> Signals { get; set; }

        [JsonIgnore]
        [PropertyNameAttribute("Does it have geo signals")]
        public bool ContainsGeoSignals { get { return Signals?.Count(x => x.IsGeo) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have bio signals")]
        public bool ContainsBioSignals { get { return Signals?.Count(x => x.IsBio) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have thargoid signals")]
        public bool ContainsThargoidSignals { get { return Signals?.Count(x => x.IsThargoid) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have guardian signals")]
        public bool ContainsGuardianSignals { get { return Signals?.Count(x => x.IsGuardian) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have human signals")]
        public bool ContainsHumanSignals { get { return Signals?.Count(x => x.IsHuman) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have other signals")]
        public bool ContainsOtherSignals { get { return Signals?.Count(x => x.IsOther) > 0 ? true : false; } }
        [JsonIgnore]
        [PropertyNameAttribute("Does it have uncategorised signals")]
        public bool ContainsUncategorisedSignals { get { return Signals?.Count(x => x.IsUncategorised) > 0 ? true : false; } }

        [JsonIgnore]
        [PropertyNameAttribute("Count of geo signals")]
        public int CountGeoSignals { get { return Signals?.Where(x => x.IsGeo).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of bio signals")]
        public int CountBioSignals { get { return Signals?.Where(x => x.IsBio).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of thargoid signals")]
        public int CountThargoidSignals { get { return Signals?.Where(x => x.IsThargoid).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of guardian signals")]
        public int CountGuardianSignals { get { return Signals?.Where(x => x.IsGuardian).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of human signals")]
        public int CountHumanSignals { get { return Signals?.Where(x => x.IsHuman).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of other signals")]
        public int CountOtherSignals { get { return Signals?.Where(x => x.IsOther).Sum(y => y.Count) ?? 0; } }
        [JsonIgnore]
        [PropertyNameAttribute("Count of uncategorised signals")]
        public int CountUncategorisedSignals { get { return Signals?.Where(x => x.IsUncategorised).Sum(y => y.Count) ?? 0; } }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddFSSBodySignalsToSystem(this,system);
        }

        private string SignalNames() { return string.Join(",", Signals?.Select(x => x.Type)); }       // for debugger

        public override string SummaryName(ISystem sys)
        {
            return base.SummaryName(sys) + " " + "of ".Tx()+ BodyName.ReplaceIfStartsWith(sys.Name);
        }

        public override string GetInfo(FillInformationData fid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JournalSAASignalsFound.SignalList(sb, Signals, 0, false, false);
            string name = BodyName.Contains(fid.System.Name, StringComparison.InvariantCultureIgnoreCase) ? BodyName : fid.System.Name + ":" + BodyName;
            sb.AppendPrePad("@ " + name, ", ");
            return sb.ToString();
        }

    }

    [System.Diagnostics.DebuggerDisplay("{Body} {Genus} {Species}")]
    [JournalEntryType(JournalTypeEnum.ScanOrganic)]
    public class JournalScanOrganic : JournalEntry, IStarScan
    {
        public JournalScanOrganic(JObject evt) : base(evt, JournalTypeEnum.ScanOrganic)
        {
            evt.ToObjectProtected(this.GetType(), true, 
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this,
                process:(t,x)=> {
                            return (ScanTypeEnum)Enum.Parse(typeof(ScanTypeEnum),x,true);       // only enum, and we just use Parse
                        }
                );        // read fields named in this structure matching JSON names

            Species = Species.Alt("Unknown");       // seen entries with empty entries for these, set to unknown.
            Species_Localised = Species_Localised.Alt(Species);
            Genus = Genus.Alt("Unknown");
            Genus_Localised = Genus_Localised.Alt(Genus);
            WasLogged = evt["WasLogged"].BoolNull();

            OrganicEstimatedValues.Values value = EventTimeUTC < EliteReleaseDates.Odyssey14 ? OrganicEstimatedValues.GetValuePreU14(Species) : OrganicEstimatedValues.GetValuePostU14(Species);

            if (value != null)
            {
                if (ScanType == ScanTypeEnum.Analyse)
                    EstimatedValue = value.Value;
                else
                    PotentialEstimatedValue = value.Value;
            }
        }

        [PropertyNameAttribute("Frontier internal system address")]
        public long SystemAddress { get; set; }
        [PropertyNameAttribute("Internal Frontier ID")]
        public int Body { get; set; }
        [PropertyNameAttribute("Frontier Genus ID")]
        public string Genus { get; set; }                       // never null
        [PropertyNameAttribute("Genus in localised text")]
        public string Genus_Localised { get; set; }                 // never null
        [PropertyNameAttribute("Frontier Species ID")]
        public string Species { get; set; }                     // never null
        [PropertyNameAttribute("Species in localised text")]
        public string Species_Localised { get; set; }               // never null
        [PropertyNameAttribute("Species in localised text without Genus")]
        public string Species_Localised_Short { get { return Species_Localised.Alt(Species).ReplaceIfStartsWith(Genus_Localised + " "); } }
        [PropertyNameAttribute("Frontier Variant ID, may be null/empty")]
        public string Variant { get; set; }                         // update 15, before it will be null
        [PropertyNameAttribute("Variant in localised text, may be null/empty")]
        public string Variant_Localised { get; set; }                // update 15, before it will be null
        [PropertyNameAttribute("Variant in localised text without Species, or empty string if not present")]
        public string Variant_Localised_Short { get { return Variant_Localised.Alt(Variant)?.ReplaceIfStartsWith(Species_Localised + " -") ?? ""; } }
        public enum ScanTypeEnum { Log, Sample, Analyse };
        [PropertyNameAttribute("Log type")]
        public ScanTypeEnum ScanType { get; set; }     //Analyse, Log, Sample
        [PropertyNameAttribute("Was it logged before")]
        public bool? WasLogged { get; set; }
        [PropertyNameAttribute("Estimated realisable value cr")]
        public int? EstimatedValue { get; set; }       // set on analyse
        [PropertyNameAttribute("Potential value cr")]
        public int? PotentialEstimatedValue { get; set; }  // set on non analyse
        [PropertyNameAttribute("Estimated or potential value cr")]
        public int Value { get { return EstimatedValue.HasValue ? EstimatedValue.Value : PotentialEstimatedValue.HasValue ? PotentialEstimatedValue.Value : 0; } }

        public void AddStarScan(StarScan s, ISystem system)
        {
            //System.Diagnostics.Debug.WriteLine($"Add ScanOrganic {ScanType} {Genus_Localised} {Species_Localised}");
            s.AddScanOrganicToSystem(this,system);
        }

        public override string GetInfo(FillInformationData fid)
        {
            int? ev = ScanType == ScanTypeEnum.Analyse ? EstimatedValue : null;     // if analyse, its estimated value
            int? pev = ev == null ? PotentialEstimatedValue : null;                 // if not at analyse, its potential value
            return BaseUtils.FieldBuilder.Build("", ScanType.ToString(), "<: ", Genus_Localised, "", Species_Localised_Short, "", Variant_Localised_Short, "; cr;N0", ev, "(;) cr;N0", pev, "", WasLogged == null ? "" : WasLogged == false ? "Was not logged".Tx() : "Was logged".Tx(), "< @ ", fid.WhereAmI);
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
                var key = so.Genus + ":" + so.Species + ":" + (so.Variant??"");     // add variant to key, if not set, its empty.

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

        // print list, with optional indent, and separ.  Separ is not placed on last entry
        static public void OrganicList(System.Text.StringBuilder sb, List<JournalScanOrganic> unsortedlist, int indent, bool indentfirst, string separ = ", ")        // default is environment.newline
        {
            var list = SortList(unsortedlist);
            string inds = new string(' ', indent);

            int index = 0;
            foreach (var t in list)
            {
                if ((index > 0 || indentfirst) && indent > 0)
                    sb.Append(inds);

                var s = t.Item2;
                sb.Build( ";/3", t.Item1, "", s.ScanType, 
                            "<: ", s.Genus_Localised, 
                            "", s.Species_Localised_Short, 
                            "", s.Variant_Localised_Short,
                            "Value: ; cr;N0".Tx(), s.EstimatedValue, 
                            "Potential Value: ; cr;N0".Tx(), s.PotentialEstimatedValue);

                if (index++ < list.Count - 1)     // if another to go, separ
                    sb.Append(separ);
            }
        }

        static public string OrganicListString(List<JournalScanOrganic> list, int indent, bool indentfirst, string separ = ", ")
        {
            var sb = new System.Text.StringBuilder(1024);
            OrganicList(sb, list, indent, indentfirst, separ);
            return sb.ToString();
        }
    }

}
