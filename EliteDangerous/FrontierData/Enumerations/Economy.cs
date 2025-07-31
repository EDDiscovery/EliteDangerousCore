/*
 * Copyright © 2023-2023 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public class EconomyDefinitions
    {
        // from EDCD 
        // localisation can be provided via the Identifiers caching of $economy

        public enum Economy
        {
            Unknown,

            Agri,
            Colony,
            Extraction,
            High_Tech,
            Industrial,
            Military,
            None,
            Refinery,
            Service,
            Terraforming,
            Tourism,
            Prison,
            Damaged,
            Rescue,
            Repair,
            Carrier,
            Engineer,

            Undefined,      // Jugom logs
        }

        // maps the $economy_id; to an enum
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static Economy ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return Economy.Unknown;

            string fdm = fdname.ToLowerInvariant().Replace("$economy_", "").Replace(" ", "").Replace(";", "");

            if (parselist.TryGetValue(fdm.ToLowerInvariant(), out Economy value))
                return value;
            else if (fdm == "hightech")
                return Economy.High_Tech;
            else if (fdm == "unknown_value")     // this has been found in a few 2017/2018 journal files
                return Economy.Unknown;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Economy is unknown `{fdname}`");
                return Economy.Unknown;
            }
        }

 
        public static string ToDecorated(Economy ec)
        {
            return "$economy_" + ec.ToString() + ";";
        }

        public static string ToEnglish(Economy ec)
        {
            return ec.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(Economy ec)
        {
            return ToEnglish(ec).Tx();
        }

        public static Economy[] ValidStates()
        {
            var list = (Economy[])Enum.GetValues(typeof(Economy));
            return list.Where(x => x != Economy.Unknown && x!=Economy.None && x!=Economy.Undefined).ToArray();
        }

        static Dictionary<string, Economy> parselist;
        static EconomyDefinitions()
        {
            parselist = new Dictionary<string, Economy>();
            foreach (var v in Enum.GetValues(typeof(Economy)))
                parselist[v.ToString().ToLowerInvariant()] = (Economy)v;
        }

        // found in Docking, Bodysettlement, etc
        public class Economies
        {
            [JsonName("name", "Name")]                  //name is for spansh, Name is for journal
            public EconomyDefinitions.Economy Name;     // fdname
            public string Name_Localised;
            [JsonName("Proportion", "share")]           //share is for spansh, proportion is for journal
            public double Proportion;                   // 0-1
        }

        // we need to read the economy list from json and preprocess the economy names, removing the decoration
        public static Economies[] ReadEconomiesClassFromJson(JToken evt)
        {
            if (evt != null)
            {
                var ret = evt?.ToObject<Economies[]>(false, process: (t, x) => {
                    return EconomyDefinitions.ToEnum(x);      // for enums, we need to process them ourselves
                });

                return ret;
            }
            else
                return null;
        }


        public static void Build(System.Text.StringBuilder sb, bool title, Economies[] list)
        {
            if (title)
                sb.Append("Economies".Tx()+": ");

            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0)
                {
                    if (i % 10 == 0)
                        sb.AppendCR();
                    else
                        sb.AppendCS();
                }

                sb.Append(ToLocalisedLanguage(list[i].Name));
                sb.AppendSPC();
                sb.Append((list[i].Proportion * 100).ToString("0.#"));
                sb.Append("%");
            }
        }

    }
}


