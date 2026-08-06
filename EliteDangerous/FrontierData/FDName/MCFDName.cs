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

    public class MCFDName : FDName
    {
        public MCFDName() : base()
        {
        }

        public MCFDName(string fdname) : base(fdname) 
        {
        }

        public MCFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public MCFDName Clone()
        {
            return new MCFDName(ID);
        }
        public override string ToString() => ID;    // we override (but prefer to use the explicit ID) so that the variable enumeration will work

        public int CompareTo(MCFDName other)
        {
            return base.CompareTo(other);
        }

        public static MCFDName Normalise(string fdname, out string matname, JournalEntry ev, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    matname = null;
                    return null;
                }
                else
                {
                    matname = "Unknown Material/Commodity";

                    if (ev?.EventTimeUTC > EliteReleaseDates.ComplainTime)
                        BaseUtils.Debugger.TraceBreak($"*** Missing Material {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");

                    return new MCFDName("Unknown Material/Commodity");
                }
            }
            else
            {
                if (fdname.Length >= 8 && fdname[0] == '$' && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                    fdname = fdname.Substring(1, fdname.Length - 7);        // remove decoration

                if (fdnamemangling.TryGetValue(fdname.ToLower(), out string value))     // fix some renaming issues
                    fdname = value;

                if (MaterialCommodityMicroResourceType.TryGet(fdname, out MaterialCommodityMicroResourceType item))
                {
                    matname = item.EnglishName;
                }
                else
                {
                    if (ev?.EventTimeUTC > EliteReleaseDates.ComplainTime)
                        BaseUtils.Debugger.TraceBreak($"*** Unknown Mat/Commod `{fdname}` {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}", true);

                    matname = fdname.SplitCapsWordFull();
                }

                return new MCFDName(fdname);
            }
        }

        private static Dictionary<string, string> fdnamemangling = new Dictionary<string, string>() // Key: old_identifier, Value: new_identifier
        {
            //2.2 to 2.3 changed some of the identifier names.. change the 2.2 ones to 2.3!  Anthor data from his materials db file

            // July 2018 - removed many, changed above, to match FD 3.1 excel output - we use their IDs.  Netlogentry frontierdata checks these..

            { "aberrantshieldpatternanalysis"       ,  "shieldpatternanalysis" },
            { "adaptiveencryptorscapture"           ,  "adaptiveencryptors" },
            { "alyabodysoap"                        ,  "alyabodilysoap" },
            { "anomalousbulkscandata"               ,  "bulkscandata" },
            { "anomalousfsdtelemetry"               ,  "fsdtelemetry" },
            { "atypicaldisruptedwakeechoes"         ,  "disruptedwakeechoes" },
            { "atypicalencryptionarchives"          ,  "encryptionarchives" },
            { "azuremilk"                           ,  "bluemilk" },
            { "cd-75kittenbrandcoffee"              ,  "cd75catcoffee" },
            { "crackedindustrialfirmware"           ,  "industrialfirmware" },
            { "dataminedwakeexceptions"             ,  "dataminedwake" },
            { "distortedshieldcyclerecordings"      ,  "shieldcyclerecordings" },
            { "eccentrichyperspacetrajectories"     ,  "hyperspacetrajectories" },
            { "edenapplesofaerial"                  ,  "aerialedenapple" },
            { "eraninpearlwhiskey"                  ,  "eraninpearlwhisky" },
            { "exceptionalscrambledemissiondata"    ,  "scrambledemissiondata" },
            { "inconsistentshieldsoakanalysis"      ,  "shieldsoakanalysis" },
            { "kachiriginfilterleeches"             ,  "kachiriginleaches" },
            { "korokungpellets"                     ,  "korrokungpellets" },
            { "leatheryeggs"                        ,  "alieneggs" },
            { "lucanonionhead"                      ,  "transgeniconionhead" },
            { "modifiedconsumerfirmware"            ,  "consumerfirmware" },
            { "modifiedembeddedfirmware"            ,  "embeddedfirmware" },
            { "opensymmetrickeys"                   ,  "symmetrickeys" },
            { "peculiarshieldfrequencydata"         ,  "shieldfrequencydata" },
            { "rajukrumulti-stoves"                 ,  "rajukrustoves" },
            { "sanumadecorativemeat"                ,  "sanumameat" },
            { "securityfirmwarepatch"               ,  "securityfirmware" },
            { "specialisedlegacyfirmware"           ,  "legacyfirmware" },
            { "strangewakesolutions"                ,  "wakesolutions" },
            { "taggedencryptioncodes"               ,  "encryptioncodes" },
            { "unidentifiedscanarchives"            ,  "scanarchives" },
            { "unusualencryptedfiles"               ,  "encryptedfiles" },
            { "utgaroarmillennialeggs"              ,  "utgaroarmillenialeggs" },
            { "xihebiomorphiccompanions"            ,  "xihecompanions" },
            { "zeesszeantgrubglue"                  ,  "zeesszeantglue" },

            {"micro-weavecoolinghoses","coolinghoses"},
            {"energygridassembly","powergridassembly"},

            {"methanolmonohydrate","methanolmonohydratecrystals"},
            {"muonimager","mutomimager"},
            {"hardwarediagnosticsensor","diagnosticsensor"},

        };

    }
}
