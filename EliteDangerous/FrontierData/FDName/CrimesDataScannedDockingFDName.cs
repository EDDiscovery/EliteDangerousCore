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
            [new CrimesFDName("assault")] = "Assault".PTx(),
            [new CrimesFDName("collidedatspeedinnofirezone")] = "Collided at speed in a no fire zone".PTx(),
            [new CrimesFDName("collidedatspeedinnofirezone_hulldamage")] = "Collided at speed in a no fire zone resulting in hull damage".PTx(),
            [new CrimesFDName("disobeypolice")] = "Disobeyed a order from the police".PTx(),
            [new CrimesFDName("dockingmajorblockingairlock")] = "Blocking an airlock".PTx(),
            [new CrimesFDName("dockingmajorblockinglandingpad")] = "Blocking a landing pad".PTx(),
            [new CrimesFDName("dockingmajortresspass")] = "Tresspass".PTx(),
            [new CrimesFDName("dockingminorblockingairlock")] = "Minor blocking of an airlock".PTx(),
            [new CrimesFDName("dockingminorblockinglandingpad")] = "Minor blocking of a landing pad".PTx(),
            [new CrimesFDName("dockingminortresspass")] = "Minor tresspass".PTx(),
            [new CrimesFDName("dumpingdangerous")] = "Ejecting goods in a dangerous place".PTx(),
            [new CrimesFDName("dumpingnearstation")] = "Ejecting goods near station".PTx(),
            [new CrimesFDName("fireinnofirezone")] = "Firing weapons in a no fire zone".PTx(),
            [new CrimesFDName("fireinstation")] = "Firing inside station".PTx(),
            [new CrimesFDName("illegalcargo")] = "Carrying illegal cargo".PTx(),
            [new CrimesFDName("interdiction")] = "Interdiction".PTx(),
            [new CrimesFDName("murder")] = "Murder of pilot on ship".PTx(),
            [new CrimesFDName("onfoot_assault")] = "Assaulting a Person".PTx(),
            [new CrimesFDName("onfoot_arccutteruse")] = "Using an arc cutter".PTx(),
            [new CrimesFDName("onfoot_breakingandentering")] = "Illegal Entry".PTx(),
            [new CrimesFDName("onfoot_carryingillegaldata")] = "Carrying illegal data".PTx(),
            [new CrimesFDName("onfoot_carryingillegalgoods")] = "Carrying illegal goods".PTx(),
            [new CrimesFDName("onfoot_carryingstolengoods")] = "Carrying stolen goods".PTx(),
            [new CrimesFDName("onfoot_damagingdefences")] = "Damaging station defenses".PTx(),
            [new CrimesFDName("onfoot_datatransfer")] = "Illegal transfer of data".PTx(),
            [new CrimesFDName("onfoot_detectionofweapon")] = "Carrying a weapon in violation of rules".PTx(),
            [new CrimesFDName("onfoot_ebreachuse")] = "Using an E Breach".PTx(),
            [new CrimesFDName("onfoot_failuretosubmittopolice")] = "Failure to submit to scan".PTx(),
            [new CrimesFDName("onfoot_identitytheft")] = "Identity theft".PTx(),
            [new CrimesFDName("onfoot_murder")] = "Murder of a person".PTx(),
            [new CrimesFDName("onfoot_overchargeintent")] = "Intending to overcharge an access port".PTx(),
            [new CrimesFDName("onfoot_overchargedport")] = "Illegal Overcharging an access port".PTx(),
            [new CrimesFDName("onfoot_profilecloningintent")] = "Cloning a persons security profile".PTx(),
            [new CrimesFDName("onfoot_propertytheft")] = "Theft of station property".PTx(),
            [new CrimesFDName("onfoot_recklessendangerment")] = "Reckless Endangerment".PTx(),
            [new CrimesFDName("onfoot_theft")] = "Theft of items".PTx(),
            [new CrimesFDName("onfoot_trespass")] = "Tresspass on station".PTx(),
            [new CrimesFDName("passengerwanted")] = "Wanted passenger".PTx(),
            [new CrimesFDName("piracy")] = "Piracy".PTx(),
            [new CrimesFDName("recklessweaponsdischarge")] = "Discharging a weapon".PTx(),
            [new CrimesFDName("shuttledestruction")] = "Destroying an APEX Shuttle".PTx(),
            [new CrimesFDName("stationtamperingminor")] = "Tampering with a station".PTx(),
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
