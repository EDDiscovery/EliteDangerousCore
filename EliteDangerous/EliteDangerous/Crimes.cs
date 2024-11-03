/*
 * Copyright © 2024-2024 EDDiscovery development team
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

using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public static class Crimes
    {
        // from EDCD 

        private static Dictionary<string, string> crimesFDToEnglish = new Dictionary<string, string>
        {
            ["assault"] = "Assault",
            ["collidedatspeedinnofirezone"] = "Collided at speed in a no fire zone",
            ["collidedatspeedinnofirezone_hulldamage"] = "Collided at speed in a no fire zone resulting in hull damage",
            ["disobeypolice"] = "Disobeyed a order from the police",
            ["dockingmajorblockingairlock"] = "Blocking an airlock",
            ["dockingmajorblockinglandingpad"] = "Blocking a landing pad",
            ["dockingmajortresspass"] = "Tresspass",
            ["dockingminorblockingairlock"] = "Minor blocking of an airlock",
            ["dockingminorblockinglandingpad"] = "Minor blocking of a landing pad",
            ["dockingminortresspass"] = "Minor tresspass",
            ["dumpingdangerous"] = "Ejecting goods in a dangerous place",
            ["dumpingnearstation"] = "Ejecting goods near station",
            ["fireinnofirezone"] = "Firing weapons in a no fire zone",
            ["fireinstation"] = "Firing inside station",
            ["illegalcargo"] = "Carrying illegal cargo",
            ["interdiction"] = "Interdiction",
            ["murder"] = "Murder of pilot on ship",
            ["onfoot_assault"] = "Assaulting a Person",
            ["onfoot_arccutteruse"] = "Using an arc cutter",
            ["onfoot_breakingandentering"] = "Illegal Entry",
            ["onfoot_carryingillegaldata"] = "Carrying illegal data",
            ["onfoot_carryingillegalgoods"] = "Carrying illegal goods",
            ["onfoot_carryingstolengoods"] = "Carrying stolen goods",
            ["onfoot_damagingdefences"] = "Damaging station defenses",
            ["onfoot_datatransfer"] = "Illegal transfer of data",
            ["onfoot_detectionofweapon"] = "Carrying a weapon in violation of rules",
            ["onfoot_ebreachuse"] = "Using an E Breach",
            ["onfoot_failuretosubmittopolice"] = "Failure to submit to scan",
            ["onfoot_identitytheft"] = "Identity theft",
            ["onfoot_murder"] = "Murder of a person",
            ["onfoot_overchargeintent"] = "Intending to overcharge an access port",
            ["onfoot_overchargedport"] = "Illegal Overcharging an access port",
            ["onfoot_profilecloningintent"] = "Cloning a persons security profile",
            ["onfoot_propertytheft"] = "Theft of station property",
            ["onfoot_recklessendangerment"] = "Reckless Endangerment",
            ["onfoot_theft"] = "Theft of items",
            ["onfoot_trespass"] = "Tresspass on station",
            ["passengerwanted"] = "Wanted passenger",
            ["piracy"] = "Piracy",
            ["recklessweaponsdischarge"] = "Discharging a weapon",
            ["shuttledestruction"] = "Destroying an APEX Shuttle",
            ["stationTamperingMinor"] = "Tampering with a station",
        };

        // maps CrimeType FDname to an english string
        public static string ToEnglish( string fdname)
        {
            //foreach( var kvp in crimesFDToEnglish) System.Diagnostics.Trace.WriteLine($"[\"{kvp.Key.ToLowerInvariant()}\"] = \"{kvp.Value}\",");
            if (fdname == null)
            {
                System.Diagnostics.Trace.WriteLine($"**** NULL crime type error");
                return "Null Crime Type - ERROR";
            }
            else if (crimesFDToEnglish.TryGetValue(fdname.ToLowerInvariant(), out string english))
            {
                return english;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"**** Unknown crime type {fdname}");
                return fdname.SplitCapsWordFull();
            }
        }

        // localised language or english
        public static string ToLocalisedLanguage(string fdname )
        {
            string id = "Crimes." + fdname.ToLowerInvariant();
            return BaseUtils.Translator.Instance.Translate(ToEnglish(fdname),id);
        }

        public static void IDSTx()
        {
            foreach (var kvp in crimesFDToEnglish) 
                System.Diagnostics.Trace.WriteLine($".{kvp.Key.ToLowerInvariant()}: \"{ToEnglish(kvp.Key)}\" @");
        }
    }
}


