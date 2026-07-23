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

        private static Dictionary<FDName, string> crimesFDToEnglish = new Dictionary<FDName, string>(new FDNameEqualityComparer())
        {
            ["assault".ToFD()] = "Assault",
            ["collidedatspeedinnofirezone".ToFD()] = "Collided at speed in a no fire zone",
            ["collidedatspeedinnofirezone_hulldamage".ToFD()] = "Collided at speed in a no fire zone resulting in hull damage",
            ["disobeypolice".ToFD()] = "Disobeyed a order from the police",
            ["dockingmajorblockingairlock".ToFD()] = "Blocking an airlock",
            ["dockingmajorblockinglandingpad".ToFD()] = "Blocking a landing pad",
            ["dockingmajortresspass".ToFD()] = "Tresspass",
            ["dockingminorblockingairlock".ToFD()] = "Minor blocking of an airlock",
            ["dockingminorblockinglandingpad".ToFD()] = "Minor blocking of a landing pad",
            ["dockingminortresspass".ToFD()] = "Minor tresspass",
            ["dumpingdangerous".ToFD()] = "Ejecting goods in a dangerous place",
            ["dumpingnearstation".ToFD()] = "Ejecting goods near station",
            ["fireinnofirezone".ToFD()] = "Firing weapons in a no fire zone",
            ["fireinstation".ToFD()] = "Firing inside station",
            ["illegalcargo".ToFD()] = "Carrying illegal cargo",
            ["interdiction".ToFD()] = "Interdiction",
            ["murder".ToFD()] = "Murder of pilot on ship",
            ["onfoot_assault".ToFD()] = "Assaulting a Person",
            ["onfoot_arccutteruse".ToFD()] = "Using an arc cutter",
            ["onfoot_breakingandentering".ToFD()] = "Illegal Entry",
            ["onfoot_carryingillegaldata".ToFD()] = "Carrying illegal data",
            ["onfoot_carryingillegalgoods".ToFD()] = "Carrying illegal goods",
            ["onfoot_carryingstolengoods".ToFD()] = "Carrying stolen goods",
            ["onfoot_damagingdefences".ToFD()] = "Damaging station defenses",
            ["onfoot_datatransfer".ToFD()] = "Illegal transfer of data",
            ["onfoot_detectionofweapon".ToFD()] = "Carrying a weapon in violation of rules",
            ["onfoot_ebreachuse".ToFD()] = "Using an E Breach",
            ["onfoot_failuretosubmittopolice".ToFD()] = "Failure to submit to scan",
            ["onfoot_identitytheft".ToFD()] = "Identity theft",
            ["onfoot_murder".ToFD()] = "Murder of a person",
            ["onfoot_overchargeintent".ToFD()] = "Intending to overcharge an access port",
            ["onfoot_overchargedport".ToFD()] = "Illegal Overcharging an access port",
            ["onfoot_profilecloningintent".ToFD()] = "Cloning a persons security profile",
            ["onfoot_propertytheft".ToFD()] = "Theft of station property",
            ["onfoot_recklessendangerment".ToFD()] = "Reckless Endangerment",
            ["onfoot_theft".ToFD()] = "Theft of items",
            ["onfoot_trespass".ToFD()] = "Tresspass on station",
            ["passengerwanted".ToFD()] = "Wanted passenger",
            ["piracy".ToFD()] = "Piracy",
            ["recklessweaponsdischarge".ToFD()] = "Discharging a weapon",
            ["shuttledestruction".ToFD()] = "Destroying an APEX Shuttle",
            ["stationtamperingminor".ToFD()] = "Tampering with a station",
        };

        // maps CrimeType FDname to an english string
        public static string ToEnglish( FDName fdname)
        {
            //foreach( var kvp in crimesFDToEnglish) System.Diagnostics.Trace.WriteLine($"[\"{kvp.Key.ToLowerInvariant()}\"] = \"{kvp.Value}\",");
            if (fdname == null)
            {
                BaseUtils.Debugger.TraceBreak($"**** NULL crime type error");
                return "Null Crime Type - ERROR";
            }
            else if (crimesFDToEnglish.TryGetValue(fdname, out string english))
            {
                return english;
            }
            else
            {
                BaseUtils.Debugger.TraceBreak($"**** Unknown crime type `{fdname}`");
                return fdname.SplitCapsWordFull();
            }
        }

        // localised language or english
        public static string ToLocalisedLanguage(FDName fdname )
        {
            return ToEnglish(fdname).Tx();
        }

    }
}


