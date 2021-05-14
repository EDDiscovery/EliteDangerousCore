/*
 * Copyright © 2021 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using BaseUtils.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EliteDangerousCore.EDAstro
{
    public class EDAstroClass : BaseUtils.HttpCom
    {
        static public string SoftwareName { get; set; } = "EDDiscovery";

        private readonly string fromSoftwareVersion;
        private readonly string EDAstroServer = "https://edastro.com/api/";

        public EDAstroClass()
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];
            httpserveraddress = EDAstroServer;
        }

        private JObject Header(bool ody)
        {
            JObject header = new JObject();

            header["appName"] = SoftwareName;
            header["appVersion"] = fromSoftwareVersion;
            header["odyssey"] = ody;

            return header;
        }

        public List<string> GetJournalEventsToSend()     // protect yourself against bad JSON
        {
            string action = "accepting";
            var response = RequestGet(action);
            if (response.Body != null)
                return JArray.Parse(response.Body).Select(v => v.Str()).ToList();
            else
                return null;
        }

        public bool SendJournalEvents(List<JObject> entries, bool ody)    // protected against bad JSON
        {
            JArray message = new JArray(Header(ody));
            message.AddRange(entries);

            System.Diagnostics.Debug.WriteLine("EDAstro send " + message.ToString(true));

            var response = RequestPost(message.ToString(), "journal", handleException: true);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
