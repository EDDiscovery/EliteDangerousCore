/*
 * Copyright © 2020 EDDiscovery development team
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
using System.Diagnostics;
using System.Reflection;

namespace EliteDangerousCore.IGAU
{
    public class IGAUClass : BaseUtils.HttpCom
    {
        static public string SoftwareName { get; set; } = "EDDiscovery";

        private readonly string App_Version;

        private readonly string igau_address = "https://ddss70885k.execute-api.us-west-1.amazonaws.com/Prod";

        public IGAUClass()
        {
            httpserveraddress = igau_address;
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            App_Version = assemblyFullName.Split(',')[1].Split('=')[1];
        }
        
        public JObject CreateIGAUMessageCodexMessage(string timestamp, string EntryID, string Name, string Name_Localised, string System, string SystemAddress)
        {
            var stripped = Name?.ToLowerInvariant()?.Replace("$", "")?.Replace("_name;", "");

            JObject detail = new JObject();
            detail["timestamp"] = timestamp;
            detail["EntryID"] = EntryID.ToString();
            detail["Name"] = stripped;
            detail["Name_Localised"] = Name_Localised;
            detail["System"] = System;
            detail["SystemAddress"] = SystemAddress.ToString();
            detail["App_Name"] = SoftwareName;
            detail["App_Version"] = App_Version;
            return detail;
        }

        public JObject CreateIGAUMessageScanOrganicMessage(string timestamp, string species, string speciesloc, string System, string SystemAddress)
        {
            var strippedspecies = species?.ToLowerInvariant()?.Replace("$", "")?.Replace("_name;", "");

            JObject detail = new JObject();
            detail["timestamp"] = timestamp;
            detail["EntryID"] = "-";
            detail["Name"] = strippedspecies;
            detail["Name_Localised"] = speciesloc;
            detail["System"] = System;
            detail["SystemAddress"] = SystemAddress.ToString();
            detail["App_Name"] = SoftwareName;
            detail["App_Version"] = App_Version;
            return detail;
        }

        public bool PostMessage(JObject msg)
        {
            if (igau_address.IsEmpty())
                return false;

            try
            {
                BaseUtils.ResponseData resp = RequestPost(msg.ToString(), "");

                var result = JToken.ParseThrowCommaEOL(resp.Body);

                if (result.Str() == "SUCCESS")
                {
                    return true;
                }
                else
                {
                    var res = result["response"];
                    var errmsg = res?.Str("errorMessage");
                    Trace.WriteLine($"IGAU message post failed: {errmsg}");
                    return false;
                }
            }
            catch (System.Net.WebException ex)
            {
                System.Net.HttpWebResponse response = ex.Response as System.Net.HttpWebResponse;
                System.Diagnostics.Trace.WriteLine($"IGAU message post failed - status: {response?.StatusCode.ToString() ?? ex.Status.ToString()}\nIGAU Message: {msg.ToString()}");
                return false;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"IGAU exception - status: {ex}");
                return false;
            }
        }
    }
}
