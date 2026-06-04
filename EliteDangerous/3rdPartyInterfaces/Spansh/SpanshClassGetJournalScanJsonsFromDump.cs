/*
 * Copyright 2023-2026 EDDiscovery development team
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

using QuickJSON;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        // return a JARRAY of Journal compatible JSON of bodies from the spansh dump format
        // may return null
        private static JArray GetJournalScansJsonsFromDump(JObject spanshdump)
        {
            //System.Diagnostics.Debug.WriteLine($"Spansh bodies found {foundsystem.Name} {foundsystem.SystemAddress}");

            JArray resultsarray;
            string sysname = null;
            long? sysaddr = null;

            resultsarray = spanshdump != null ? spanshdump["system"].I("bodies").Array() : null;
            sysname = spanshdump["system"].I("name").StrNull();                       // must have these now
            sysaddr = spanshdump["system"].I("id64").LongNull();

            if (resultsarray != null && sysname.HasChars() && sysaddr.HasValue)
            {
                JArray retresult = new JArray();

                foreach (var so in resultsarray)
                {
                    JObject scan = ConvertToJournalScan(so, sysname, sysaddr.Value);
                    if ( scan != null )
                    {
                        retresult.Add(scan);
                    }
                }

                return retresult;
            }

            return null;
        }
    }
}

