/*
 * Copyright 2026-2026 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in coSmpliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class CrimesFDName : FDName
    {
        public CrimesFDName(): base()
        {
        }

        public CrimesFDName(string fdname) : base(fdname) 
        {
        }

        public CrimesFDName(QuickJSON.JToken token) : base(token)
        {
        }
        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work

        private static Dictionary<CrimesFDName, string> crimesFDToEnglish = new Dictionary<CrimesFDName, string>()
        {
            [new CrimesFDName("assault")] = "Assault",
            [new CrimesFDName("collidedatspeedinnofirezone")] = "Collided at speed in a no fire zone",
            [new CrimesFDName("collidedatspeedinnofirezone_hulldamage")] = "Collided at speed in a no fire zone resulting in hull damage",
            [new CrimesFDName("disobeypolice")] = "Disobeyed a order from the police",
            [new CrimesFDName("dockingmajorblockingairlock")] = "Blocking an airlock",
            [new CrimesFDName("dockingmajorblockinglandingpad")] = "Blocking a landing pad",
            [new CrimesFDName("dockingmajortresspass")] = "Tresspass",
            [new CrimesFDName("dockingminorblockingairlock")] = "Minor blocking of an airlock",
            [new CrimesFDName("dockingminorblockinglandingpad")] = "Minor blocking of a landing pad",
            [new CrimesFDName("dockingminortresspass")] = "Minor tresspass",
            [new CrimesFDName("dumpingdangerous")] = "Ejecting goods in a dangerous place",
            [new CrimesFDName("dumpingnearstation")] = "Ejecting goods near station",
            [new CrimesFDName("fireinnofirezone")] = "Firing weapons in a no fire zone",
            [new CrimesFDName("fireinstation")] = "Firing inside station",
            [new CrimesFDName("illegalcargo")] = "Carrying illegal cargo",
            [new CrimesFDName("interdiction")] = "Interdiction",
            [new CrimesFDName("murder")] = "Murder of pilot on ship",
            [new CrimesFDName("onfoot_assault")] = "Assaulting a Person",
            [new CrimesFDName("onfoot_arccutteruse")] = "Using an arc cutter",
            [new CrimesFDName("onfoot_breakingandentering")] = "Illegal Entry",
            [new CrimesFDName("onfoot_carryingillegaldata")] = "Carrying illegal data",
            [new CrimesFDName("onfoot_carryingillegalgoods")] = "Carrying illegal goods",
            [new CrimesFDName("onfoot_carryingstolengoods")] = "Carrying stolen goods",
            [new CrimesFDName("onfoot_damagingdefences")] = "Damaging station defenses",
            [new CrimesFDName("onfoot_datatransfer")] = "Illegal transfer of data",
            [new CrimesFDName("onfoot_detectionofweapon")] = "Carrying a weapon in violation of rules",
            [new CrimesFDName("onfoot_ebreachuse")] = "Using an E Breach",
            [new CrimesFDName("onfoot_failuretosubmittopolice")] = "Failure to submit to scan",
            [new CrimesFDName("onfoot_identitytheft")] = "Identity theft",
            [new CrimesFDName("onfoot_murder")] = "Murder of a person",
            [new CrimesFDName("onfoot_overchargeintent")] = "Intending to overcharge an access port",
            [new CrimesFDName("onfoot_overchargedport")] = "Illegal Overcharging an access port",
            [new CrimesFDName("onfoot_profilecloningintent")] = "Cloning a persons security profile",
            [new CrimesFDName("onfoot_propertytheft")] = "Theft of station property",
            [new CrimesFDName("onfoot_recklessendangerment")] = "Reckless Endangerment",
            [new CrimesFDName("onfoot_theft")] = "Theft of items",
            [new CrimesFDName("onfoot_trespass")] = "Tresspass on station",
            [new CrimesFDName("passengerwanted")] = "Wanted passenger",
            [new CrimesFDName("piracy")] = "Piracy",
            [new CrimesFDName("recklessweaponsdischarge")] = "Discharging a weapon",
            [new CrimesFDName("shuttledestruction")] = "Destroying an APEX Shuttle",
            [new CrimesFDName("stationtamperingminor")] = "Tampering with a station",
        };

        // maps CrimeType FDname to an english string
        public static string ToEnglish(CrimesFDName fdname)
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
        public static string ToLocalisedLanguage(CrimesFDName fdname)
        {
            return ToEnglish(fdname).Tx();
        }

    }

    public class DockingFDName : FDName
    {
        public DockingFDName() : base()
        {
        }

        public DockingFDName(string fdname) : base(fdname)
        {
        }

        public DockingFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work

        public static DockingFDName Normalise(string fdname, out string engname)
        {
            engname = fdname.SplitCapsWordFull();
            return new DockingFDName(fdname);
        }
    }

    public class DataScannedFDName : FDName
    {
        public DataScannedFDName() : base()
        {
        }

        public DataScannedFDName(string fdname) : base(fdname)
        {
        }

        public DataScannedFDName(QuickJSON.JToken token) : base(token)
        {
        }
        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work

        public static DataScannedFDName Normalise(string fdname, out string engname)
        {
            if (fdname.Length >= 8 && fdname.StartsWithIIC("$Datascan_") && fdname.EndsWith(";", System.StringComparison.InvariantCultureIgnoreCase))
                fdname = fdname.Substring(10, fdname.Length - 1 - 10);        // remove decoration

            engname = fdname.SplitCapsWordFull();
            return new DataScannedFDName(fdname);
        }
    }


}
