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
        public const string RootURL = "https://spansh.co.uk/";
        public SpanshClass() : base(RootURL + "api/")
        {
        }

        public const int MaxReturnSize = 500;       // as coded by spansh

        public static TimeSpan MaxCacheAge = new TimeSpan(7, 0, 0, 0);

        #region Browser
        static public string URLForSystem(SystemAddress sysaddr)
        {
            return RootURL + "system/" + sysaddr.ToString();
        }

        static public bool LaunchBrowserForSystem(SystemAddress sysaddr)
        {
            return BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "system/" + sysaddr.ToString());
        }
        static public bool LaunchBrowserForStationByMarketID(MarketID marketid)
        {
            return BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "station/" + marketid.ToString());
        }

        static public bool LaunchBrowserForStationByFullBodyID(BodySystemAddress fullbodyid)
        {
            return BaseUtils.BrowserInfo.LaunchBrowser(RootURL + "body/" + fullbodyid.ToString());
        }

        static public bool LaunchBrowserForSystem(string name)
        {
            SpanshClass sp = new SpanshClass();
            ISystem s = sp.GetSystem(name);
            if (s != null)
                return LaunchBrowserForSystem(s.SystemAddress);
            return false;
        }

        static public bool LaunchBrowserForSystem(ISystem sys)
        {
            if (sys.SystemAddress.IsValid)
            {
                return LaunchBrowserForSystem(sys.SystemAddress);
            }
            else
                return LaunchBrowserForSystem(sys.Name);
        }

        #endregion


        #region Systems
        public JObject GetSystemNames(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = RequestGet("systems/field_values/system_names/" + query);

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

            var response = RequestGet("search/systems" + query);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            if (json != null)
            {
                foreach (var body in json["results"].EmptyIfNull())
                {
                    string rname = body["name"].Str();

                    var sys = new SystemClass(rname, new SystemAddress(body["id64"]), body["x"].Double(), body["y"].Double(), body["z"].Double(), SystemSource.FromSpansh);

                    if (rname.Equals(name, StringComparison.InvariantCultureIgnoreCase) && sys.Triage())
                    {
                        return sys;
                    }

                }
            }

            return null;
        }
        // find system info of address case insensitive
        public ISystem GetSystem(SystemAddress address)
        {
            var response = RequestGet($"dump/{address.ToString()}");

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            if (json != null)
            {
               // BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\getsystem.json", json?.ToString(true));
                if (json.Contains("system"))
                    return new SystemClass(json["system"].I("name").Str(),new SystemAddress(json["system"].I("id64")), SystemSource.FromSpansh);
            }

            return null;
        }

        // ensure we have a valid system address from a sys, null if can't
        public ISystem EnsureSystemAddressAndName(ISystem sys)
        {
            if (sys.SystemAddress == null)
            {
                SpanshClass sp = new SpanshClass();
                sys = sp.GetSystem(sys.Name);       // name and system address filled
            }
            else if ( sys.Name.IsEmpty())
            {
                SpanshClass sp = new SpanshClass();
                sys = sp.GetSystem(sys.SystemAddress);       // name and system address filled
            }

            return sys;
        }

        #endregion

    }
}

