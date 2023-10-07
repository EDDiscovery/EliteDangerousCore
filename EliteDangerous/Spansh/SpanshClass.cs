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
using System.Web;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        public SpanshClass()
        {
            base.httpserveraddress = "https://www.spansh.co.uk/api/";
        }

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

        public List<Tuple<ISystem, double>> GetSystemsByCoord(double x, double y, double z, double maxradius, double minradius, int max)
        {
            // POST, systems :  { "filters":{ "distance":{ "min":"0","max":"7"} },"sort":[{ "distance":{ "direction":"asc"}}],"size":10,"page":0,"reference_coords":{ "x":100,"y":100,"z":100} }: 
            //                  {"filters": { "distance":{"min":"0","max":"10"}}, "sort":[{"distance":{"direction":"asc"}}],"size":10,"reference_coords":{"x":0.0,"y":0.0,"z":0.0}}
            // size relates to number returned per page

            JObject jo = new JObject()
            {
                ["filters"] = new JObject()
                {
                    ["distance"] = new JObject()
                    {
                        ["min"] = minradius.ToStringInvariant(),
                        ["max"] = maxradius.ToStringInvariant(),
                    }
                },
                ["sort"] = new JArray()
                {
                    new JObject()
                    {
                        ["distance"] = new JObject()
                        {
                            ["direction"] = "asc"
                        }
                    }
                },
                ["reference_coords"] = new JObject()
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["z"] = z,
                }
            };

            System.Diagnostics.Debug.WriteLine($"Spansh post data for systems {jo.ToString()}");

            var response = RequestPost(jo.ToString(), "systems/search", handleException: true);

            if (response.Error)
                return null;

            var data = response.Body;
            var json = JObject.Parse(data, JToken.ParseOptions.CheckEOL);
            System.Diagnostics.Debug.Write($"SPansh returned {json.ToString(true)}");
            return null;
        }

    }
}
