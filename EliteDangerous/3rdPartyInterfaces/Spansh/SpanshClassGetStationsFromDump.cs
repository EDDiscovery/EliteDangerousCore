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
using System.Collections.Generic;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        // convert to a list of StationInfo from spansh dump
        // always returns a list

        public static List<StationInfo> GetStationsFromDump(JToken spanshdump)
        {
            List<StationInfo> stationinfo = new List<StationInfo>();

            JObject jsystem = spanshdump?["system"].Object();

            if (jsystem == null)
                return stationinfo;

            string sysname = jsystem["name"].StrNull();                       // must have these now
            long? sysaddr = jsystem["id64"].LongNull();

            if (sysname == null || sysaddr == null)
                return stationinfo;

            SystemClass sys = new SystemClass(sysname, sysaddr);
            sys.X = jsystem["coords"].I("x").Double();
            sys.Y = jsystem["coords"].I("y").Double();
            sys.Z = jsystem["coords"].I("z").Double();

            JArray stationarray = jsystem["stations"].Array();

            if (stationarray != null)
            {
                foreach (var evt in stationarray)
                {
                    var si = ConvertToStationInfo(evt.Object(), sys);
                    if (si != null)
                    {
                        stationinfo.Add(si);
                    }

                }
            }

            JArray bodyarray = jsystem["bodies"].Array();

            if (bodyarray != null)
            {
                foreach (var body in bodyarray)
                {
                    string bodyname = body["name"].StrNull();
                    int bodyid = body["bodyId"].Int();
                    string bodytype = body["type"].StrNull();
                    string bodysubtype = body["subType"].StrNull();

                    foreach (var evt in body["stations"].EmptyIfNull())
                    {
                        var si = ConvertToStationInfo(evt.Object(), sys, bodyname, bodyid, bodytype, bodysubtype);
                        if (si != null)
                        {
                            stationinfo.Add(si);
                        }
                    }
                }


            }

            return stationinfo;
        }

    }
}

