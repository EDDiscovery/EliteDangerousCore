/*
 * Copyright © 2023-2024 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ClassOfSignal} {SignalName}")]
    public class Signal
    {
        [PropertyNameAttribute("Signal name string, FDName")]
        public SignalFDName SignalName { get; set; }          // can be "Lewis Dock", "$...", no real form
        [PropertyNameAttribute("Signal name localised")]
        public string SignalName_Localised { get; set; }
        [PropertyNameAttribute("Signal type, may not be present in old data")]
        public string SignalType { get; set; }  // may be null/empty on older records
        [PropertyNameAttribute("Spawing state, USS Only")]
        public string SpawningState { get; set; }
        [PropertyNameAttribute("Signal state localised, USS Only")]
        public string SpawningState_Localised { get; set; }
        [PropertyNameAttribute("Signal faction, FDName, USS only")]
        public string SpawningFaction { get; set; }
        [PropertyNameAttribute("Signal faction, Localised, USS only")]
        public string SpawningFaction_Localised { get; set; }
        [PropertyNameAttribute("Optional time remaining seconds for USS types")]
        public double? TimeRemaining { get; set; }          // null if not expiring
        [PropertyNameAttribute("Optional Frontier system address")]
        public SystemAddress SystemAddress { get; set; }

        [PropertyNameAttribute("Is it a station")]
        public bool? IsStation { get; set; }

        [PropertyNameAttribute("Threat level, USS Only")]
        public int? ThreatLevel { get; set; }
        [PropertyNameAttribute("Optional USS Type, FDName")]
        public string USSType { get; set; }     // only for signal types of USS
        [PropertyNameAttribute("Optional USS Type, Localised")]
        public string USSTypeLocalised { get; set; }

        [PropertyNameAttribute("Ascendency, Optional Spawning Power")]
        public string SpawningPower { get; set; }

        [PropertyNameAttribute("Ascendency, Optional Opposing Power")]
        public string OpposingPower { get; set; }

        [PropertyNameAttribute("When signal was recorded")]
        public System.DateTime RecordedUTC { get; set; }        // when it was recorded

        [PropertyNameAttribute("Optional signal expiry time, UTC, USS types")]
        public System.DateTime ExpiryUTC { get; set; }
        [PropertyNameAttribute("Optional signal expiry time, Local, USS types")]
        public System.DateTime ExpiryLocal { get; set; }

        [PropertyNameAttribute("EDD Definition of signal classification")]
        public Classification ClassOfSignal { get; set; }
        public enum Classification
        {
            Station, Installation, NotableStellarPhenomena, ConflictZone,
            ResourceExtraction, Carrier, USS, Megaship,
            Other, NavBeacon, Titan, TouristBeacon,
            Codex
        };

        const int CarrierExpiryTime = 10 * (60 * 60 * 24);              // days till we consider the carrier signal expired..

        // Make a signal description from JSON
        public Signal(JObject evt, System.DateTime EventTimeUTC)
        {
            SignalName = new SignalFDName(evt["SignalName"].Str());
            string signalnamelocalised = evt["SignalName_Localised"].Str();     // not present for stations/installations
            SignalName_Localised = signalnamelocalised.Alt(SignalName.Str());         // don't mangle if no localisation, its prob not there because its a proper name
            SignalType = evt["SignalType"].Str();

            SpawningState = evt["SpawningState"].Str();          // USS only, checked
            SpawningState_Localised = JournalFieldNaming.CheckLocalisation(evt["SpawningState_Localised"].Str(), SpawningState);

            SpawningFaction = evt["SpawningFaction"].Str();      // USS only, checked
            SpawningFaction_Localised = JournalFieldNaming.CheckLocalisation(evt["SpawningFaction_Localised"].Str(), SpawningFaction);
            //if ( SpawningFaction.HasChars() ) System.Diagnostics.Debug.WriteLine($"DS {SpawningFaction} {SpawningFaction_Localised}");

            if (SpawningFaction.EqualsIIC("$faction_none;"))       // kill these none entries
                SpawningFaction = SpawningFaction_Localised = "";

            USSType = evt["USSType"].Str();                     // USS Only, checked
            USSTypeLocalised = JournalFieldNaming.CheckLocalisation(evt["USSType_Localised"].Str(), USSType);

            ThreatLevel = evt["ThreatLevel"].IntNull();         // USS only, checked

            TimeRemaining = evt["TimeRemaining"].DoubleNull();  // USS only, checked

            SystemAddress = new SystemAddress(evt["SystemAddress"]);

            IsStation = evt["IsStation"].BoolNull();

            ClassOfSignal = GetClassification(SignalName, SignalType, IsStation == true, signalnamelocalised);

            if (TimeRemaining == null)
                TimeRemaining = ExpiryTimes[(int)ClassOfSignal].TotalSeconds;

            RecordedUTC = EventTimeUTC;

            ExpiryUTC = EventTimeUTC.AddSeconds(TimeRemaining.Value);
            ExpiryLocal = ExpiryUTC.ToLocalTime();
        }

        public bool IsSame(Signal other)     // is this signal the same as the other one (assuming same system)
        {
            return SignalName.Equals(other.SignalName) && SpawningFaction.Equals(other.SpawningFaction) && SpawningState.Equals(other.SpawningState) &&
                   USSType.Equals(other.USSType) && ThreatLevel == other.ThreatLevel;
        }

        public bool HasNotExpired(bool overrideit)
        {
            return overrideit || ExpiryUTC >= DateTime.UtcNow;
        }

        public override string ToString()
        {
            DateTime? outoftime = null;
            DateTime? seen = null;

            if (ExpiryUTC < DateTime.UtcNow)      // show dates on expired entries
                outoftime = ExpiryLocal;
            //both move in and out of systems, so show last seen, only if not expired
            else if (ClassOfSignal == Classification.Carrier || ClassOfSignal == Classification.Megaship)
                seen = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(RecordedUTC);

            string signname = ClassOfSignal == Classification.USS ? null : SignalName_Localised;        // signal name for USS is boring, remove

            string spstate = SpawningState_Localised != null ? SpawningState_Localised.Truncate(0, 32, "..") : null;

            return BaseUtils.FieldBuilder.Build(
                        ";Station".Tx() + ": ", ClassOfSignal == Classification.Station,
                        ";Carrier".Tx() + ": ", ClassOfSignal == Classification.Carrier,
                        ";Megaship".Tx() + ": ", ClassOfSignal == Classification.Megaship,
                        ";Installation".Tx() + ": ", ClassOfSignal == Classification.Installation,
                        "<", signname,
                        "", USSTypeLocalised,
                        "Threat Level".Tx() + ": ", ThreatLevel,
                        "Faction".Tx() + ": ", SpawningFaction_Localised,
                        "Power".Tx() + ": ", SpawningPower,
                        "vs " + "Power".Tx() + ": ", OpposingPower,
                        "State".Tx() + ": ", spstate,
                        "Expiry".Tx() + ": ", outoftime,
                        "Last Seen".Tx() + ": ", seen
                        );
        }

        static public void Sort(List<Signal> signals)
        {
            signals.Sort(delegate (Signal left, Signal right)
            {
                var ret = left.ClassOfSignal.CompareTo(right.ClassOfSignal);        // by class
                if (ret == 0)
                {
                    ret = left.RecordedUTC.CompareTo(right.RecordedUTC);        // then time

                }
                return ret;
            });
        }
        static public List<Signal> NotExpiredSorted(List<Signal> signals)
        {
            var notexpired = signals.Where(x => x.ExpiryUTC >= DateTime.UtcNow).ToList();
            Sort(notexpired);
            return notexpired;
        }
        static public List<Signal> ExpiredSorted(List<Signal> signals)
        {
            var expired = signals.Where(x => x.ExpiryUTC < DateTime.UtcNow).ToList();
            Sort(expired);
            return expired;
        }

        private static Classification GetClassification(SignalFDName fdsignalname, string fdsignaltype, bool isstation, string signalnamelocalised)
        {
            Classification signalclass = Classification.Other;

            if (fdsignaltype.HasChars())
            {
                if (fdsignaltype.Contains("Station", StringComparison.InvariantCultureIgnoreCase) || (fdsignaltype.Equals("Outpost", StringComparison.InvariantCultureIgnoreCase)))
                    signalclass = Classification.Station;
                else if (fdsignaltype.Equals("FleetCarrier", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Carrier;
                else if (fdsignaltype.Equals("Installation", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Installation;
                else if (fdsignaltype.Equals("Megaship", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Megaship;
                else if (fdsignaltype.Equals("Combat", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ConflictZone;
                else if (fdsignaltype.Equals("ResourceExtraction", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ResourceExtraction;
                else if (fdsignaltype.Equals("NavBeacon", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NavBeacon;
                else if (fdsignaltype.Equals("Titan", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Titan;
                else if (fdsignaltype.Equals("TouristBeacon", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.TouristBeacon;
                else if (fdsignaltype.Equals("USS", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.USS;
                else if (fdsignaltype.Equals("Generic", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Other;
                else if (fdsignaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase) && fdsignalname.StartsWith("$Fixed_Event_Life"))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (fdsignaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Codex;
                else
                    signalclass = Classification.Other;
            }
            else
            {
                if (isstation == true)          // station flag
                    signalclass = ClassifyStationName(fdsignalname);
                else if (fdsignalname.StartsWith("$USS") || fdsignalname.StartsWith("$RANDOM"))
                    signalclass = Classification.USS;
                else if (fdsignalname.StartsWith("$Warzone"))
                    signalclass = Classification.ConflictZone;
                else if (fdsignalname.StartsWith("$Fixed_Event_Life"))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (fdsignalname.StartsWith("$MULTIPLAYER_SCENARIO14") || fdsignalname.StartsWith("$MULTIPLAYER_SCENARIO7"))
                    signalclass = Classification.ResourceExtraction;
                else if (fdsignalname.Contains("-class"))
                    signalclass = Classification.Megaship;
                else if (signalnamelocalised.Length == 0)      // other types, and old station entries, don't have localisation, so its an installation, put at end of list because other things than installations have no localised name too
                    signalclass = Classification.Installation;
                else
                    signalclass = Classification.Other;
            }

            return signalclass;
        }
        
        public static Classification ClassifyStationName(SignalFDName signal)
        {
            string fdsignalname = signal.ToLower();
            int dash = fdsignalname.LastIndexOf('-');
            if (fdsignalname.Length >= 5 && dash == fdsignalname.Length - 4 && char.IsLetterOrDigit(fdsignalname[dash + 1]) && char.IsLetterOrDigit(fdsignalname[dash - 1]))
                return Classification.Carrier;
            else
                return Classification.Station;
        }
        
        private static TimeSpan[] ExpiryTimes =
        {
            new TimeSpan(365*1,0,0,0,0),  // station
            new TimeSpan(365*1,0,0,0,0),  // installation
            new TimeSpan(365*1,0,0,0,0),  // NSP
            new TimeSpan(14,0,0,0,0),  // Conflict zone

            new TimeSpan(365*1,0,0,0,0),  // RES
            new TimeSpan(14,0,0,0,0),  // Carrier
            new TimeSpan(14,0,0,0,0),  // USS (carries its own timeout anyway)
            new TimeSpan(14,0,0,0,0),  // megaship

            new TimeSpan(14,0,0,0,0),  // Other
            new TimeSpan(365*1,0,0,0,0),  // navbeacon
            new TimeSpan(14,0,0,0,0),  // titan
            new TimeSpan(365*10,0,0,0,0),  // tourist beacon

            new TimeSpan(365*1,0,0,0,0),  // codex
        };
    }

    [System.Diagnostics.DebuggerDisplay("{Type} {Count}")]
    public class SAASignal
    {
        [PropertyNameAttribute("Signal type string, FDName")]
        [JsonAlwaysCreate]
        public SignalFDName Type { get; set; }        // material fdname, or $SAA_SignalType..
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
        [JsonAlwaysCreate]
        public GenusFDName Genus { get; set; }        // $Codex_Ent_Bacterial_Genus_Name;
        [PropertyNameAttribute("Genus type string, localised")]
        public string Genus_Localised { get; set; }


    }


}


