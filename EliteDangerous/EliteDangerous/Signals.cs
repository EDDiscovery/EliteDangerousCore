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
    public class SignalDefinitions
    {
        public enum Classification
        {
            Station, Installation, NotableStellarPhenomena, ConflictZone, ResourceExtraction,
            Carrier, USS, Megaship, Other, NavBeacon, Titan, TouristBeacon, Codex
        };

        // SignalType could be null/empty, in which case its based on SignalName/IsStation/Localised string
        // older entries did not have SignalType.
        public static Classification GetClassification(string fdsignalname, string fdsignaltype, bool isstation, string signalnamelocalised)
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
                else if (fdsignaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase) && fdsignalname.StartsWith("$Fixed_Event_Life", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (fdsignaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Codex;
                else
                    signalclass = Classification.Other;
            }
            else
            {
                if (isstation == true)          // station flag
                {
                    int dash = fdsignalname.LastIndexOf('-');
                    if (fdsignalname.Length >= 5 && dash == fdsignalname.Length - 4 && char.IsLetterOrDigit(fdsignalname[dash + 1]) && char.IsLetterOrDigit(fdsignalname[dash - 1]))
                        signalclass = Classification.Carrier;
                    else
                        signalclass = Classification.Station;
                }
                else if (fdsignalname.StartsWith("$USS", StringComparison.InvariantCultureIgnoreCase) || fdsignalname.StartsWith("$RANDOM", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.USS;
                else if (fdsignalname.StartsWith("$Warzone", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ConflictZone;
                else if (fdsignalname.StartsWith("$Fixed_Event_Life", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (fdsignalname.StartsWith("$MULTIPLAYER_SCENARIO14", StringComparison.InvariantCultureIgnoreCase) || fdsignalname.StartsWith("$MULTIPLAYER_SCENARIO7", StringComparison.InvariantCultureIgnoreCase))
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
    }

    [System.Diagnostics.DebuggerDisplay("{ClassOfSignal} {SignalName}")]
    public class FSSSignal
    {
        [PropertyNameAttribute("Signal name string, FDName")]
        public string SignalName { get; set; }
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
        public long? SystemAddress { get; set; }

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
        public SignalDefinitions.Classification ClassOfSignal { get; set; }

        const int CarrierExpiryTime = 10 * (60 * 60 * 24);              // days till we consider the carrier signal expired..

        // Make a signal description from JSON
        public FSSSignal(JObject evt, System.DateTime EventTimeUTC)
        {
            SignalName = evt["SignalName"].Str();
            string signalnamelocalised = evt["SignalName_Localised"].Str();     // not present for stations/installations
            SignalName_Localised = signalnamelocalised.Alt(SignalName);         // don't mangle if no localisation, its prob not there because its a proper name
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

            SystemAddress = evt["SystemAddress"].LongNull();

            IsStation = evt["IsStation"].BoolNull();

            ClassOfSignal = SignalDefinitions.GetClassification(SignalName, SignalType, IsStation == true, signalnamelocalised);

            if (ClassOfSignal == SignalDefinitions.Classification.Carrier)
                TimeRemaining = CarrierExpiryTime;

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
                   USSType.Equals(other.USSType) && ThreatLevel == other.ThreatLevel &&
                   (ClassOfSignal == SignalDefinitions.Classification.Carrier || ExpiryUTC == other.ExpiryUTC);       // note carriers have our own expiry on it, so we don't
        }

        public string ToString(bool showseentime)
        {
            DateTime? outoftime = null;
            if (TimeRemaining != null && ClassOfSignal != SignalDefinitions.Classification.Carrier)       // ignore carrier timeout for printing
                outoftime = ExpiryLocal;

            DateTime? seen = null;
            if (showseentime && (ClassOfSignal == SignalDefinitions.Classification.Carrier || ClassOfSignal == SignalDefinitions.Classification.Megaship)) //both move in and out of systems, so show last seen
                seen = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(RecordedUTC);

            string signname = ClassOfSignal == SignalDefinitions.Classification.USS ? null : SignalName_Localised;        // signal name for USS is boring, remove

            string spstate = SpawningState_Localised != null ? SpawningState_Localised.Truncate(0, 32, "..") : null;

            return BaseUtils.FieldBuilder.Build(
                        ";Station: ".T(EDCTx.FSSSignal_StationBool), ClassOfSignal == SignalDefinitions.Classification.Station,
                        ";Carrier: ".T(EDCTx.FSSSignal_CarrierBool), ClassOfSignal == SignalDefinitions.Classification.Carrier,
                        ";Megaship: ".T(EDCTx.FSSSignal_MegashipBool), ClassOfSignal == SignalDefinitions.Classification.Megaship,
                        ";Installation: ".T(EDCTx.FSSSignal_InstallationBool), ClassOfSignal == SignalDefinitions.Classification.Installation,
                        "<", signname,
                        "", USSTypeLocalised,
                        "Threat Level: ".T(EDCTx.FSSSignal_ThreatLevel), ThreatLevel,
                        "Faction: ".T(EDCTx.FSSSignal_Faction), SpawningFaction_Localised,
                        "Power: ".T(EDCTx.JournalEntry_Power), SpawningPower,
                        "vs " + "Power: ".T(EDCTx.JournalEntry_Power), OpposingPower,
                        "State: ".T(EDCTx.FSSSignal_State), spstate,
                        "Time: ".T(EDCTx.JournalEntry_Time), outoftime,
                        "Last Seen: ".T(EDCTx.FSSSignal_LastSeen), seen
                        );
        }

        static public List<FSSSignal> NotExpiredSorted(List<FSSSignal> signals)
        {
            var notexpired = signals.Where(x => !x.TimeRemaining.HasValue || x.ExpiryUTC >= DateTime.UtcNow).ToList();
            notexpired.Sort(delegate (FSSSignal l, FSSSignal r) { return l.ClassOfSignal.CompareTo(r.ClassOfSignal); });
            return signals;
        }
        static public void Sort(List<FSSSignal> signals)
        {
            signals.Sort(delegate (FSSSignal left, FSSSignal right) 
            { 
                var ret = left.ClassOfSignal.CompareTo(right.ClassOfSignal);        // by class
                if (ret == 0)
                {
                    ret = left.RecordedUTC.CompareTo(right.RecordedUTC);        // then time

                }
                return ret;
            });
        }
        static public List<FSSSignal> ExpiredSorted(List<FSSSignal> signals)
        {
            var expired = signals.Where(x => x.TimeRemaining.HasValue && x.ExpiryUTC < DateTime.UtcNow).ToList();
            expired.Sort(delegate (FSSSignal l, FSSSignal r) { return l.ClassOfSignal.CompareTo(r.ClassOfSignal); });
            return signals;
        }
    }
}


