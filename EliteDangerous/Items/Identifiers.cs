/*
 * Copyright © 2023-2023 EDDiscovery development team
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

using System.Collections.Generic;

namespace EliteDangerousCore
{
    // caches Fdev identifiers vs localised names, from signals, from economies

    public class Identifiers
    {
        public static uint Generation { get; set; } = 0;

        public static Dictionary<string, string> Items { get; private set; } = new Dictionary<string, string>();

        // signals have
        // $name;
        // $name:#type=typename;:#index=8       $SAA_Unknown_Signal:#type=$SAA_SignalType_Geological;:#index=7;
        // $name:#index=8;                      $Settlement_Unflattened_WreckedUnknown:#index=1;
        // $name; $name:#threadlevel=1;         $POIScenario_Watson_Wreckage_Buggy_01_Salvage_Easy; $USS_ThreatLevel:#threatLevel=1;
        // $name; $name:#index=1;               $MARKET_POPULATION_Large; $FIXED_EVENT_DEBRIS:#index=1;

        public static void Add(string id, string text, bool alwaysadd = false)
        {
            if (id != text || alwaysadd)        // don't add the same stuff
            {
                string nid = id.ToLowerInvariant().Trim();

               // lock (identifiers)    // since only changed by HistoryList accumulate, and accessed by foreground, no need I think for a lock
                {
                    text = text.Replace("&NBSP;", " ");
                    //System.Diagnostics.Debug.WriteLine($"Identifier {id} -> {nid} -> {text}");
                    Items[nid] = text;        // keep updating even if a repeat so the latest identifiers is there
                    Generation++;
                }
            }
            else
            {
               // System.Diagnostics.Debug.WriteLine($"Rejected adding {id} vs {text}");
            }

        }

        // return null if 
        public static string Get(string id, bool returnnull = false)
        {
            string nid = id.ToLowerInvariant().Trim();

          //  lock (identifiers)
            {
                if (Items.TryGetValue(nid, out string str))
                {
                    return str;
                }
                else
                {
                    for (int i = 0; i < defdefs.Length; i = i + 3)
                    {
                        if (nid.StartsWith(defdefs[i]))
                        {
                            return defdefs[i + 1];
                        }
                    }

                    return returnnull ? null : id;
                }
            }
        }

        public static void Process(JournalEntry je)
        {
            if (je is IIdentifiers)
                (je as IIdentifiers).UpdateIdentifiers();
        }

        // from EDDI 17/11/23

        static string[] defdefs = new string[]
        {
            "$multiplayer_scenario42_title","Nav Beacon","Nav beacon",
            "$multiplayer_scenario80_title","Compromised Nav Beacon","Compromised nav beacon",
            "$uss","Unidentified Signal Source","Unidentified Signal Source",
            "$warzone_pointrace_high","High Intensity Conflict Zone","Conflict Zone (High Intensity)",
            "$warzone_pointrace_med","Medium Intensity Conflict Zone","Conflict Zone (Medium Intensity)",
            "$warzone_pointrace_low","Low Intensity Conflict Zone","Conflict Zone (Low Intensity)",
            "$uss_type_salvage","Degraded Emissions","Salvage signal",
            "$uss_type_aftermath","Combat Aftermath","Aftermatch signal",
            "$uss_type_anomaly","Anomaly","Anomaly signal",
            "$uss_type_ceremonial","Ceremonial Comms","Ceremonial signal",
            "$uss_type_veryvaluablesalvage","High Grade Emissions","VeryValuable signal",
            "$uss_type_valuablesalvage","Encoded Emissions","Valuable signal",
            "$uss_type_nonhuman","Non-Human Signal Source","NonHuman signal",
            "$uss_type_weaponsfire","Weapons Fire","WeaponsFire signal",
            "$uss_type_missiontarget","Mission Target","MissionTarget signal",
            "$uss_type_convoy","Convoy Dispersal Pattern","Convoy signal",
            "$uss_type_distresssignal","Distress Call","Distress signal",
            "$uss_type_tradingbeacon","Trading Beacon","TradingBeacon signal",
            "$multiplayer_scenario78_title","High Intensity Resource Extraction Site","ResourceExtraction Site (High Intensity)",
            "$multiplayer_scenario77_title","Low Intensity Resource Extraction Site","Resource Extraction site (Low)",
            "$multiplayer_scenario14_title","Resource Extraction Site","Resource Extraction Site",
            "$multiplayer_scenario79_title","Hazardous Resource Extraction Site","Resource Extraction Site [Hazardous]",
            "$numberstation","Unregistered Comms Beacon","NumberStation",
            "$warzone_tg","AX Conflict Zone","Thargoid conflict zone",
            "$listeningpost","Listening Post","ListeningPost",
            "$fixed_event_life_cloud","Notable Stellar Phenomena","Notable Stellar Phenomena",
            "$fixed_event_capship","Capital Ship","Capital Ship",
            "$fixed_event_life_ring","Notable Stellar Phenomena","Notable Stellar Phenomena",
            "$fixed_event_highthreatscenario_t5","Pirate Activity","Pirate Activity Detected [Threat 5]",
            "$saa_signaltype_biological","Biological Surface Signal","Biological Surface Signal",
            "$saa_signaltype_geological","Geological Surface Signal","Geological Surface Signal",
            "$saa_signaltype_human","Human Surface Signal","Human Surface Signal",
            "$saa_signaltype_thargoid","Thargoid Surface Signal","Thargoid Surface Signal",
            "$settlement_unflattened_unknown","Thargoid Barnacle","Thargoid Barnacle",
            "$fixed_event_checkpoint","Checkpoint","Checkpoint",
            "$fixed_event_convoy","Convoy Beacon","Convoy Beacon	",
            "$fixed_event_highthreatscenario_t6","Pirate Activity","Pirate Activity Detected [Threat 6]",
            "$fixed_event_highthreatscenario_t7","Pirate Activity","Pirate Activity Detected [Threat 7]",
            "$saa_signaltype_guardian","Guardian Surface Signal","Guardian Surface Signal",
            "$ancient","Ancient Ruins","Ancient (Guardian) Ruins",
            "$ancient_small","Guardian Structure","Guardian Structure",
            "$ancient_tiny","Guardian Structure","Guardian Structure",
            "$ancient_medium","Guardian Structure","Guardian Structure",
            "$fixed_event_debris","Debris Field","Debris field",
            "$saa_signaltype_other","Other Surface Signal","Other Surface Signal",
            "$settlement_unflattened_wreckedunknown","Crashed Thargoid ship","Thargoid crash site",
            "$multiplayer_scenario81_title","Salvageable Wreckage","Salvageable Wreckage signal source",
            "$fixed_event_distributioncentre","Distribution Center","Distribution Centre",
            "$aftermath_large","Distress Call","Distress Call",
            "$attackaftermath","Distress Call","Distress Call",
            "$abandoned_buggy","Distress Beacon","Distress Beacon",
            "$damaged_eagle_assassination","ENCRYPTED SIGNAL","ENCRYPTED SIGNAL",
            "$damaged_sidewinder_assassination","ENCRYPTED SIGNAL","ENCRYPTED SIGNAL",
            "$damaged_eagle","Distress Beacon","Distress Beacon",
            "$damaged_sidewinder","Distress Beacon","Distress Beacon",
            "$smugglers_cache","Irregular Markers","Irregular Markers",
            "$trap_cargo","Irregular Markers","Irregular Markers",
            "$wreckage_buggy","Minor Wreckage","Minor Wreckage",
            "$wreckage_probe","Impact Site","Impact Site",
            "$wreckage_satellite","Impact Site","Impact Site",
            "$wreckage_cargo","Minor Wreckage","Minor Wreckage",
            "$wrecks_eagle","Crash Site","Crash Site",
            "$wrecks_sidewinder","Crash Site","Crash Site",
            "$crashedship","Crashed Ship","POI CrashedShip",
            "$gro_controlscenariotitle","Armed Revolt","Gro_controlScenarioTitle",
            "$trap_data","Irregular Markers","Irregular Markers",
            "$perimeter","Active Power Source","Active Power Source",
            "$genericsignalsource","Signal Source","When the signal source may be one of several types",
            "$saa_signaltype_planetanomaly","Planetary Anomaly","Planetary Anomaly Surface Signal",
            "$cargo","Minor Wreckage","Minor Wreckage",
            "$warzone_tg_med","Medium Intensity AX Conflict Zone","Medium Intensity Thargoid Conflict Zone",
            "$warzone_tg_low","Low Intensity AX Conflict Zone","Low Intensity Thargoind Conflict Zone",
            "$warzone_tg_high","High Intensity AX Conflict Zone","High Intensity Thargoid Conflict Zone",
            "$warzone_tg_veryhigh","Very High Intensity AX Conflict Zone","Very High Intensity AX Conflict Zone",
            "$wreckage_ancientprobe","Minor Wreckage","Minor Wreckage",
        };
    }
}
