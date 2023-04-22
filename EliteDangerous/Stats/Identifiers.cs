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
 
using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class Identifiers
    {
        public uint Generation { get; set; } = 0;

        private Dictionary<string, string> identifiers = new Dictionary<string, string>();

        public void Add(string id, string text, bool alwaysadd = false)
        {
            if (id != text || alwaysadd)        // don't add the same stuff
            {
                string nid = id.ToLowerInvariant().Trim();

                int i = nid.IndexOf(":#");
                if (i > 0)
                    nid = nid.Substring(0, i);

               // lock (identifiers)    // since only changed by HistoryList accumulate, and accessed by foreground, no need I think for a lock
                {
                    text = text.Replace("&NBSP;", " ");
                    System.Diagnostics.Debug.WriteLine($"Identifier {id} -> {nid} -> {text}");
                    identifiers[nid] = text;        // keep updating even if a repeat so the latest identifiers is there
                    Generation++;
                }
            }
            else
            {
               // System.Diagnostics.Debug.WriteLine($"Rejected adding {id} vs {text}");
            }

        }

        // return null if 
        public string Get(string id, bool returnnull = false)
        {
            string nid = id.ToLowerInvariant().Trim();

            int i = nid.IndexOf(":#");
            if (i > 0)
                nid = nid.Substring(0, i);

          //  lock (identifiers)
            {
                if (identifiers.TryGetValue(nid, out string str))
                    return str;
                else
                    return returnnull ? null : id;
            }
        }

        public void Process(JournalEntry je)
        {
            if (je is IIdentifiers)
                (je as IIdentifiers).UpdateIdentifiers(this);

        }
    }
}
