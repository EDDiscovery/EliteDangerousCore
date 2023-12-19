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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public SpanshClass()
        {
            base.httpserveraddress = RootURL + "api/";
        }

        public const string RootURL = "https://spansh.co.uk/";
        public const int MaxReturnSize = 500;       // as coded by spansh

        public static TimeSpan MaxCacheAge = new TimeSpan(7, 0, 0, 0);

        #region Browser
        static public void LaunchBrowserForSystem(long sysaddr)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "system/" + sysaddr.ToStringInvariant());
        }
        static public string URLForSystem(long sysaddr)
        {
            return RootURL + "system/" + sysaddr.ToStringInvariant();
        }

        static public void LaunchBrowserForStationByMarketID(long marketid)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "station/" + marketid.ToStringInvariant());
        }

        static public void LaunchBrowserForStationByFullBodyID(long fullbodyid)
        {
            BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "body/" + fullbodyid.ToStringInvariant());
        }


        static public void LaunchBrowserForSystem(string name)
        {
            SpanshClass sp = new SpanshClass();
            ISystem s = sp.GetSystem(name);
            if (s != null)
                LaunchBrowserForSystem(s.SystemAddress.Value);
        }

        static public void LaunchBrowserForSystem(ISystem sys)
        {
            if (sys.SystemAddress.HasValue)
                LaunchBrowserForSystem(sys.SystemAddress.Value);
            else
                LaunchBrowserForSystem(sys.Name);
        }

        #endregion


        #region Systems
        public JObject GetSystemNames(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = RequestGet("systems/field_values/system_names/" + query, handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            return json;
        }

        // find system info of name, case insensitive
        public ISystem GetSystem(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = RequestGet("search/systems" + query, handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            if (json != null)
            {
                foreach (var body in json["results"].EmptyIfNull())
                {
                    string rname = body["name"].Str();

                    var sys = new SystemClass(rname, body["id64"].Long(), body["x"].Double(), body["y"].Double(), body["z"].Double(), SystemSource.FromSpansh);

                    if (rname.Equals(name, StringComparison.InvariantCultureIgnoreCase) && sys.Triage())
                    {
                        return sys;
                    }

                }
            }

            return null;
        }

        // ensure we have a valid system address from a sys, null if can't
        public ISystem EnsureSystemAddress(ISystem sys)
        {
            if (sys.SystemAddress == null)
            {
                SpanshClass sp = new SpanshClass();
                sys = sp.GetSystem(sys.Name);
            }

            return sys;
        }

        #endregion

    }
}

