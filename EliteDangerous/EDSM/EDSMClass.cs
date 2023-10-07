/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;

namespace EliteDangerousCore.EDSM
{
    public partial class EDSMClass : BaseUtils.HttpCom
    {
        // use if you need an API/name pair to get info from EDSM.  Not all queries need it
        public bool ValidCredentials { get { return !string.IsNullOrEmpty(commanderName) && !string.IsNullOrEmpty(apiKey); } }

        static public string SoftwareName { get; set; } = "EDDiscovery";
        private string commanderName;
        private string apiKey;

        private readonly string fromSoftwareVersion;

        public EDSMClass()
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];

            base.httpserveraddress = ServerAddress;

            apiKey = EDCommander.Current.EDSMAPIKey;
            commanderName = string.IsNullOrEmpty(EDCommander.Current.EdsmName) ? EDCommander.Current.Name : EDCommander.Current.EdsmName;
        }

        public EDSMClass(EDCommander cmdr) : this()
        {
            if (cmdr != null)
            {
                apiKey = cmdr.EDSMAPIKey;
                commanderName = string.IsNullOrEmpty(cmdr.EdsmName) ? cmdr.Name : cmdr.EdsmName;
            }
        }


        static string edsm_server_address = "https://www.edsm.net/";
        public static string ServerAddress { get { return edsm_server_address; } set { edsm_server_address = value; } }
        public static bool IsServerAddressValid { get { return edsm_server_address.Length > 0; } }

        #region For Trilateration

        public string SubmitDistances(string from, Dictionary<string, double> distances)
        {
            string query = "{\"ver\":2," + " \"commander\":\"" + commanderName + "\", \"fromSoftware\":\"" + SoftwareName + "\",  \"fromSoftwareVersion\":\"" + fromSoftwareVersion + "\", \"p0\": { \"name\": \"" + from + "\" },   \"refs\": [";

            var counter = 0;
            foreach (var item in distances)
            {
                if (counter++ > 0)
                {
                    query += ",";
                }

                var to = item.Key;
                var distance = item.Value.ToString("0.00", CultureInfo.InvariantCulture);

                query += " { \"name\": \"" + to + "\",  \"dist\": " + distance + " } ";
            }

            query += "] } ";

            MimeType = "application/json; charset=utf-8";
            var response = RequestPost("{ \"data\": " + query + " }", "api-v1/submit-distances", handleException: true);
            if (response.Error)
                return null;
            var data = response.Body;
            return response.Body;
        }


        public bool ShowDistanceResponse(string json, out string respstr, out Boolean trilOK)
        {
            bool retval = true;
            JObject edsm = null;
            trilOK = false;

            respstr = "";

            try
            {
                if (json == null)
                    return false;

                edsm = JObject.Parse(json);

                if (edsm == null)
                    return false;

                JObject basesystem = (JObject)edsm["basesystem"];
                JArray distances = (JArray)edsm["distances"];

                if (distances != null)
                {
                    foreach (var st in distances)
                    {
                        int statusnum = st["msgnum"].Int();

                        if (statusnum == 201)
                            retval = false;

                        respstr += "Status " + statusnum.ToString() + " : " + st["msg"].Str() + Environment.NewLine;
                    }
                }

                if (basesystem != null)
                {
                    int statusnum = basesystem["msgnum"].Int();

                    if (statusnum == 101)
                        retval = false;

                    if (statusnum == 102 || statusnum == 104)
                        trilOK = true;

                    respstr += "System " + statusnum.ToString() + " : " + basesystem["msg"].Str() + Environment.NewLine;
                }

                return retval;
            }
            catch (Exception ex)
            {
                respstr += "Exception in ShowDistanceResponse: " + ex.Message;
                return false;
            }
        }


        public bool IsKnownSystem(string sysName)       // Verified Nov 20
        {
            string query = "system?systemName=" + HttpUtility.UrlEncode(sysName);
            string json = null;
            var response = RequestGet("api-v1/" + query, handleException: true);
            if (response.Error)
                return false;
            json = response.Body;

            if (json == null)
                return false;

            return json.ToString().Contains("\"name\":");
        }

        public List<string> GetPushedSystems()                                  // Verified Nov 20
        {
            string query = "api-v1/systems?pushed=1";
            return getSystemsForQuery(query);
        }

        public List<string> GetUnknownSystemsForSector(string sectorName)       // Verified Nov 20
        {
            string query = $"api-v1/systems?systemName={HttpUtility.UrlEncode(sectorName)}%20&onlyUnknownCoordinates=1";
            // 5s is occasionally slightly short for core sectors returning the max # systems (1000)
            return getSystemsForQuery(query, 10000);
        }

        List<string> getSystemsForQuery(string query, int timeout = 5000)       // Verified Nov 20
        {
            List<string> systems = new List<string>();

            var response = RequestGet(query, handleException: true, timeout: timeout);
            if (response.Error)
                return systems;

            var json = response.Body;
            if (json == null)
                return systems;

            JArray msg = JArray.Parse(json);

            if (msg != null)
            {
                foreach (JObject sysname in msg)
                {
                    systems.Add(sysname["name"].Str("Unknown"));
                }
            }

            return systems;
        }

        #endregion

        #region For System DB update

        // Verified Nov 20 - EDSM update working
        public BaseUtils.ResponseData RequestSystemsData(DateTime startdate, DateTime enddate, int timeout = 5000)      // protect yourself against JSON errors!
        {
            if (startdate < EDDFixesDates.EDSMMinimumSystemsDate)
                startdate = EDDFixesDates.EDSMMinimumSystemsDate;

            string query = "api-v1/systems" +
                "?startdatetime=" + HttpUtility.UrlEncode(startdate.ToUniversalTime().ToStringYearFirstInvariant()) +
                "&enddatetime=" + HttpUtility.UrlEncode(enddate.ToUniversalTime().ToStringYearFirstInvariant()) +
                "&coords=1&known=1&showId=1";
            return RequestGet(query, handleException: true, timeout: timeout);
        }

        public string GetHiddenSystems(string file)   // Verfied Nov 20
        {
            try
            {
                if (BaseUtils.DownloadFile.HTTPDownloadFile(base.httpserveraddress + "api-v1/hidden-systems?showId=1", file, false, out bool newfile))
                {
                    string json = BaseUtils.FileHelpers.TryReadAllTextFromFile(file);
                    return json;
                }
                else
                    return null;
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"Exception: {ex.Message}");
                Trace.WriteLine($"ETrace: {ex.StackTrace}");
                return null;
            }

        }

        #endregion

        #region Comment sync

        private string GetComments(DateTime starttime)
        {
            if (!ValidCredentials)
                return null;

            string query = "get-comments?startdatetime=" + HttpUtility.UrlEncode(starttime.ToStringYearFirstInvariant()) + "&apiKey=" + apiKey + "&commanderName=" + HttpUtility.UrlEncode(commanderName) + "&showId=1";
            var response = RequestGet("api-logs-v1/" + query, handleException: true);

            if (response.Error)
                return null;

            return response.Body;
        }

        public void GetComments(Action<string> logout = null)           // Protected against bad JSON.. Verified Nov 2020
        {
            var json = GetComments(new DateTime(2011, 1, 1));

            if (json != null)
            {
                try
                {
                    JObject msg = JObject.ParseThrowCommaEOL(json);                  // protect against bad json - seen in the wild
                    int msgnr = msg["msgnum"].Int();

                    JArray comments = (JArray)msg["comments"];
                    if (comments != null)
                    {
                        int commentsadded = 0;

                        foreach (JObject jo in comments)
                        {
                            string systemname = jo["system"].Str();
                            string note = jo["comment"].Str();
                            DateTime utctime = jo["lastUpdate"].DateTime(DateTime.UtcNow, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                            int edsmid = jo["systemId"].Int(0);
                            var localtime = utctime.ToLocalTime();

                            if (note.HasChars())
                            {
                                SystemNoteClass curnote = SystemNoteClass.GetSystemNote(systemname);

                                if (curnote != null)                // curnote uses local time to store
                                {
                                    if (localtime.Ticks > curnote.LocalTimeLastCreatedEdited.Ticks)   // if newer, add on (verified with EDSM 29/9/2016 + 25/11/22)
                                    {
                                        curnote.UpdateNote(curnote.Note + Environment.NewLine + note, localtime, curnote.JournalText);  // keep same journal text
                                        commentsadded++;
                                    }
                                }
                                else
                                {
                                    SystemNoteClass.MakeNote(note, localtime, systemname, 0, "EDSM");   // new one! we use EDSM in the journal text to indicate
                                    commentsadded++;
                                }
                            }
                        }

                        logout?.Invoke(string.Format("EDSM Comments downloaded/updated {0}", commentsadded));
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("EDSM Get comments failed due to " + e.ToString());
                }
            }
        }

        public string SetComment(string systemName, string note, long edsmid = 0)  // Verified Nov 20
        {
            if (!ValidCredentials)
                return null;

            string query;
            query = "systemName=" + HttpUtility.UrlEncode(systemName) + "&commanderName=" + HttpUtility.UrlEncode(commanderName) + "&apiKey=" + apiKey + "&comment=" + HttpUtility.UrlEncode(note);

            if (edsmid > 0)
            {
                // For future use when EDSM adds the ability to link a comment to a system by EDSM ID
                query += "&systemId=" + edsmid;
            }

            MimeType = "application/x-www-form-urlencoded";
            var response = RequestPost(query, "api-logs-v1/set-comment", handleException: true);

            if (response.Error)
                return null;

            return response.Body;
        }

        public static void SendComments(string star, string note, long edsmid = 0, EDCommander cmdr = null) // (verified with EDSM 29/9/2016)
        {
            System.Diagnostics.Debug.WriteLine("Send note to EDSM " + star + " " + edsmid + " " + note);
            EDSMClass edsm = new EDSMClass(cmdr);

            if (!edsm.ValidCredentials)
                return;

            System.Threading.Tasks.Task taskEDSM = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                edsm.SetComment(star, note, edsmid);
            });
        }

        #endregion

        #region Log Sync for log fetcher

        // return fsd jumps, logstarttime,logendtime.  return is HTTP response code
        // forms best fsd jumps possible
        // Protected against bad JSON
        // Verified and recoded april 23

        public int GetLogs(DateTime? starttimeutc, DateTime? endtimeutc, out List<JournalFSDJump> fsdjumps, 
                            out DateTime logstarttime, out DateTime logendtime, out BaseUtils.ResponseData response)
        {
            fsdjumps = new List<JournalFSDJump>();
            logstarttime = DateTime.MaxValue;
            logendtime = DateTime.MinValue;
            response = new BaseUtils.ResponseData { Error = true, StatusCode = HttpStatusCode.Unauthorized };

            if (!ValidCredentials)
                return 0;

            // does not have system address, only internal id

            string query = "get-logs?showId=1&showCoordinates=1&apiKey=" + apiKey + "&commanderName=" + HttpUtility.UrlEncode(commanderName);

            if (starttimeutc != null)
            {
                var st = starttimeutc.Value.ToStringYearFirstInvariant();
                query += "&startDateTime=" + HttpUtility.UrlEncode(st);
            }

            if (endtimeutc != null)
            {
                var et = endtimeutc.Value.ToStringYearFirstInvariant();
                query += "&endDateTime=" + HttpUtility.UrlEncode(et);
            }

            response = RequestGet("api-logs-v1/" + query, handleException: true);

            if (response.Error)
            {
                if ((int)response.StatusCode == 429)
                    return 429;
                else
                    return 0;
            }

            var json = response.Body;

            if (json == null)
                return 0;

            try
            {

                JObject msg = JObject.ParseThrowCommaEOL(json);
                int msgnr = msg["msgnum"].Int(0);

                JArray logs = (JArray)msg["logs"];

                if (logs != null)
                {
                    string startdatestr = msg["startDateTime"].Str();
                    string enddatestr = msg["endDateTime"].Str();
                    if (startdatestr == null || !DateTime.TryParseExact(startdatestr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out logstarttime))
                        logstarttime = DateTime.MaxValue;
                    if (enddatestr == null || !DateTime.TryParseExact(enddatestr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out logendtime))
                        logendtime = DateTime.MinValue;

                    var systems = new List<Tuple<ISystem,JObject>>();

                    // since EDSM does not give xyz in the log response, first see if any systems are in our DB
                    SystemsDatabase.Instance.DBRead(db =>
                    {
                        foreach (JObject jo in logs)
                        {
                            string name = jo["system"].Str();
                            ISystem sc = DB.SystemCache.FindSystemInCacheDB(new SystemClass(name), db);      // find in our DB only.  may be null
                            if (sc != null)     // yes it is
                            {
                                if (!sc.SystemAddress.HasValue)                                     // fill in any values
                                    sc.SystemAddress = jo["systemId64"].LongNull();
                                if (!sc.EDSMID.HasValue)
                                    sc.EDSMID = jo["systemId"].LongNull();
                            }
                            systems.Add(new Tuple<ISystem,JObject>(sc,jo));    // sc may be null
                        }
                    });

                    // now some systems may be missing, so we can fill in with edsm queries
                    for (int i = 0; i < systems.Count; i++)
                    {
                        if ( systems[i].Item1 == null || !systems[i].Item1.HasCoordinate )  // if not known or no locations
                        {
                            long id = systems[i].Item2["systemId"].Long();
                            var found = GetSystemByEDSMID(id);     // this may return null
                            if ( found != null )
                            {
                                JObject jo = systems[i].Item2;
                                SystemClass sc = new SystemClass(jo["system"].Str(), jo["systemId"].Long(), jo["systemId64"].LongNull(), SystemSource.FromEDSM);
                                sc.X = found["coords"].I("x").Double(0);
                                sc.Y = found["coords"].I("y").Double(0);
                                sc.Z = found["coords"].I("z").Double(0);
                                systems[i] = new Tuple<ISystem, JObject>(sc, systems[i].Item2);
                            }
                        }
                    }

                    // now make the FSD jumps up from good systems with co-ords

                    fsdjumps = new List<JournalFSDJump>();
                    for (int i = 0; i < systems.Count; i++)
                    {
                        if (systems[i].Item1 != null )    // we need a good system to add.. we may have failed EDSM check above (unlikely)
                        {
                            JObject jo = systems[i].Item2;
                            bool firstdiscover = jo["firstDiscover"].Bool();
                            DateTime etutc = DateTime.ParseExact(jo["date"].Str(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal); // UTC time
                            JournalFSDJump fsd = new JournalFSDJump(etutc, systems[i].Item1, EDCommander.Current.MapColour, firstdiscover, true);
                            fsd.StarPosFromEDSM = true;
                            fsdjumps.Add(fsd);
                        }
                    }
                }

                return msgnr;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Failed due to " + e.ToString());
                return 499;     // BAD JSON
            }
        }

        #endregion

        #region System Information

        // given a list of names, get ISystems associated..   may return null, or empty list if edsm responded with nothing
        // ISystem list may not be in same order, or even have the same number of entries than sysNames.
        // systems unknown to EDSM in sysNames are just ignored and not reported in the returned object
        // Verified April 23 with ID64 system address return

        public List<ISystem> GetSystems(List<string> sysNames)                      // verified feb 21
        {
            List<ISystem> list = new List<ISystem>();

            int pos = 0;

            while (pos < sysNames.Count)
            {
                int left = sysNames.Count - pos;
                List<string> toprocess = sysNames.GetRange(pos, Math.Min(20, left));     // N is arbitary to limit length of query
                pos += toprocess.Count;

                // does not return system address
                string query = "api-v1/systems?onlyKnownCoordinates=1&showId=1&showCoordinates=1&";

                bool first = true;
                foreach (string s in toprocess)
                {
                    if (first)
                        first = false;
                    else
                        query = query + "&";
                    query = query + $"systemName[]={HttpUtility.UrlEncode(s)}";
                }

                var response = RequestGet(query, handleException: true);
                if (response.Error)
                    return null;

                var json = response.Body;
                if (json == null)
                    return null;

                JArray msg = JArray.Parse(json);

                if (msg != null)
                {
                    //System.Diagnostics.Debug.WriteLine("Return " + msg.ToString(true));

                    foreach (JObject s in msg)
                    {
                        JObject coords = s["coords"].Object();
                        if (coords != null)
                        {
                            SystemClass sys = new SystemClass(s["name"].Str("Unknown"), s["id"].Long(), s["id64"].LongNull(),
                                                            coords["x"].Double(), coords["y"].Double(), coords["z"].Double(), SystemSource.FromEDSM);
                            list.Add(sys);
                        }
                    }
                }
            }

            return list;
        }

        // cache of lookups, either null not found or list
        static private Dictionary<string, List<ISystem>> EDSMGetSystemCache = new Dictionary<string, List<ISystem>>();

        static public bool HasSystemLookedOccurred(string name)
        {
            lock (EDSMGetSystemCache)       // only lock over test, its unlikely that two queries with the same name will come at the same time
            {
                return EDSMGetSystemCache.ContainsKey(name);
            }
        }

        // lookup, through the cache, a system name and return a system list of matching names
        // will return null if not found, or the list.
        // Verified April 23 with ID64 system address return
        public List<ISystem> GetSystem(string systemName)
        {
            lock (EDSMGetSystemCache)       // only lock over test, its unlikely that two queries with the same name will come at the same time
            {
                if (EDSMGetSystemCache.TryGetValue(systemName, out List<ISystem> res))  // if cache has the name
                {
                    return res;     // will return null or list
                }
            }

            string query = String.Format("api-v1/systems?systemName={0}&showCoordinates=1&showId=1&showInformation=1&showPermit=1", Uri.EscapeDataString(systemName));

            var response = RequestGet(query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null)
                return null;

            JArray msg = JArray.Parse(json);

            if (msg != null)
            {
                List<ISystem> systems = new List<ISystem>();

                foreach (JObject sysname in msg)
                {
                    // tbd sys addr
                    ISystem sys = new SystemClass(sysname["name"].Str("Unknown"), sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);

                    if (sys.Name.Equals(systemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        JObject co = (JObject)sysname["coords"];

                        if (co != null)
                        {
                            sys.X = co["x"].Double();
                            sys.Y = co["y"].Double();
                            sys.Z = co["z"].Double();
                        }

                        systems.Add(sys);
                    }
                }

                if (systems.Count == 0) // no systems, set to null so stored as such
                    systems = null;

                lock (EDSMGetSystemCache)
                {
                    EDSMGetSystemCache[systemName] = systems;
                }
                return systems;
            }

            return null;
        }

        // Verified April 23 with ID64 system address return
        public List<Tuple<ISystem, double>> GetSphereSystems(String systemName, double maxradius, double minradius)      // may return null
        {
            // api does not state id64, but tested 20/4/23 it supports it
            string query = String.Format("api-v1/sphere-systems?systemName={0}&radius={1}&minRadius={2}&showCoordinates=1&showId=1",
                                Uri.EscapeDataString(systemName), maxradius.ToStringInvariant(), minradius.ToStringInvariant());

            var response = RequestGet(query, handleException: true, timeout: 30000);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json != null)
            {
                try
                {
                    List<Tuple<ISystem, double>> systems = new List<Tuple<ISystem, double>>();

                    JArray msg = JArray.Parse(json);        // allow for crap from EDSM or empty list

                    if (msg != null)
                    {
                        foreach (JObject sysname in msg)
                        {
                            // tbd sys addr
                            SystemClass sys = new SystemClass(sysname["name"].Str("Unknown"),sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);        // make a system from EDSM
                            JObject co = (JObject)sysname["coords"];
                            if (co != null)
                            {
                                sys.X = co["x"].Double();
                                sys.Y = co["y"].Double();
                                sys.Z = co["z"].Double();
                            }

                            if (sys.Triage())
                            {
                                systems.Add(new Tuple<ISystem, double>(sys, sysname["distance"].Double()));
                            }
                        }

                        return systems;
                    }
                }
                catch (Exception e)      // json may be garbage
                {
                    System.Diagnostics.Debug.WriteLine("Failed due to " + e.ToString());
                }
            }

            return null;
        }

        // Verified April 23 with ID64 system address return
        public List<Tuple<ISystem, double>> GetSphereSystems(double x, double y, double z, double maxradius, double minradius)      // may return null
        {
            // api does not state id64, but tested 20/4/23 it supports it
            string query = String.Format("api-v1/sphere-systems?x={0}&y={1}&z={2}&radius={3}&minRadius={4}&showCoordinates=1&showId=1",
                                x.ToStringInvariant("0.##"), y.ToStringInvariant("0.##"), z.ToStringInvariant("0.##"), maxradius.ToStringInvariant("0.#"), minradius.ToStringInvariant("0.#"));

            System.Diagnostics.Debug.WriteLine($"EDSM Query sphere {x} {y} {z} at {minradius} - {maxradius} ly");

            var response = RequestGet(query, handleException: true, timeout: 30000);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json != null)
            {
                try
                {
                    List<Tuple<ISystem, double>> systems = new List<Tuple<ISystem, double>>();

                    JArray msg = JArray.Parse(json);        // allow for crap from EDSM or empty list

                    if (msg != null)
                    {
                        foreach (JObject sysname in msg)
                        {
                            // tbd sys addr
                            SystemClass sys = new SystemClass(sysname["name"].Str("Unknown"), sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);   // make a EDSM system
                            JObject co = (JObject)sysname["coords"];
                            if (co != null)
                            {
                                sys.X = co["x"].Double();
                                sys.Y = co["y"].Double();
                                sys.Z = co["z"].Double();
                            }

                            if (sys.Triage())
                            {
                                systems.Add(new Tuple<ISystem, double>(sys, sysname["distance"].Double()));
                            }
                            //System.Diagnostics.Debug.WriteLine($"  EDSM returned sphere {sys.Name} {sys.X} {sys.Y} {sys.Z} dist {dist}");
                        }

                        return systems;
                    }
                }
                catch (Exception e)      // json may be garbage
                {
                    System.Diagnostics.Debug.WriteLine("Failed due to " + e.ToString());
                }
            }

            return null;
        }

        public string GetUrlToSystem(string sysName)            // get a direct name, no check if exists
        {
            string encodedSys = HttpUtility.UrlEncode(sysName);
            string url = base.httpserveraddress + "system?systemName=" + encodedSys;
            return url;
        }

        public bool ShowSystemInEDSM(string sysName)      // Verified Nov 20, checks it exists
        {
            string url = GetUrlCheckSystemExists(sysName);
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            else
            {
                BaseUtils.BrowserInfo.LaunchBrowser(url);
            }
            return true;
        }

        public string GetUrlCheckSystemExists(string sysName)      // Check if sysname exists
        {
            long id = -1;
            string encodedSys = HttpUtility.UrlEncode(sysName);

            string query = "system?systemName=" + encodedSys + "&showId=1";
            var response = RequestGet("api-v1/" + query, handleException: true);
            if (response.Error)
                return "";

            JObject jo = response.Body?.JSONParseObject(JToken.ParseOptions.CheckEOL);   // null if no body, or not object

            if (jo != null)
                id = jo["id"].Long(-1);

            if (id == -1)
                return "";

            string url = base.httpserveraddress + "system/id/" + id.ToStringInvariant() + "/name/" + encodedSys;
            return url;
        }

        // Verified april 23 with xyz return and edsmid/system 64 return
        public JObject GetSystemByAddress(long id64)
        {
            string query = "?systemId64=" + id64.ToStringInvariant() + "&showInformation=1&includeHidden=1&showCoordinates=1&&showId=1";
            var response = RequestGet("api-v1/system" + query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
        }

        // Verified april 23 with xyz return and edsmid/system 64 return
        public JObject GetSystemByEDSMID(long edsmid)
        {
            string query = "?systemId=" + edsmid.ToStringInvariant() + "&showInformation=1&includeHidden=1&showCoordinates=1&&showId=1";
            var response = RequestGet("api-v1/system" + query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
        }

        #endregion

        #region Body info

        private JObject GetBodies(string sysName)       // Verified Nov 20, null if bad json
        {
            string encodedSys = HttpUtility.UrlEncode(sysName);

            string query = "bodies?systemName=" + sysName;
            var response = RequestGet("api-system-v1/" + query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
        }

        private JObject GetBodiesByID64(long id64)       // Verified Nov 20, null if bad json
        {
            string query = "bodies?systemId64=" + id64.ToStringInvariant();
            var response = RequestGet("api-system-v1/" + query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
        }

        private JObject GetBodies(long edsmID)          // Verified Nov 20, null if bad json
        {
            string query = "bodies?systemId=" + edsmID.ToString();
            var response = RequestGet("api-system-v1/" + query, handleException: true);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
        }

        public async static System.Threading.Tasks.Task<Tuple<List<JournalScan>, bool>> GetBodiesListAsync(ISystem sys, bool edsmweblookup = true) // get this edsmid,  optionally lookup web protected against bad json
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return GetBodiesList(sys, edsmweblookup);
            });
        }

        // EDSMBodiesCache gets either the body list, or null marking no EDSM server data
        static private Dictionary<string, List<JournalScan>> EDSMBodiesCache = new Dictionary<string, List<JournalScan>>();

        public static bool HasBodyLookupOccurred(string name)
        {
            return EDSMBodiesCache.ContainsKey(name);
        }
        public static bool HasNoDataBeenStoredOnBody(string name)      // true if lookup occurred, but no data. false otherwise
        {
            return EDSMBodiesCache.TryGetValue(name, out List<JournalScan> d) && d == null;
        }

        // returns null if EDSM says not there, else if returns list of bodies and a flag indicating if from cache. 
        // all this is done in a lock inside a task - the only way to sequence the code and prevent multiple lookups in an await structure
        // so we must pass back all the info we can to tell the caller what happened.
        // Verified Nov 21

        public static Tuple<List<JournalScan>, bool> GetBodiesList(ISystem sys, bool edsmweblookup = true)
        {
            try
            {
                lock (EDSMBodiesCache) // only one request at a time going, this is to prevent multiple requests for the same body
                {
                    // System.Threading.Thread.Sleep(2000); //debug - delay to show its happening 
                    // System.Diagnostics.Debug.WriteLine("EDSM Cache check " + sys.EDSMID + " " + sys.SystemAddress + " " + sys.Name);

                    if (EDSMBodiesCache.TryGetValue(sys.Name, out List<JournalScan> we))
                    {
                        System.Diagnostics.Debug.WriteLine($"EDSM Bodies Cache hit on {sys.Name} {we != null}");
                        if (we == null) // lookedup but not found
                            return null;
                        else
                            return new Tuple<List<JournalScan>, bool>(we, true);        // mark from cache
                    }

                    if (!edsmweblookup)      // must be set for a web lookup
                        return null;

                    System.Diagnostics.Debug.WriteLine($"EDSM Web lookup on {sys.Name}");

                    List<JournalScan> bodies = new List<JournalScan>();

                    EDSMClass edsm = new EDSMClass();

                    JObject jo = null;

                    if (sys.SystemAddress != null && sys.SystemAddress > 0)
                        jo = edsm.GetBodiesByID64(sys.SystemAddress.Value);
                    else if (sys.Name != null)
                        jo = edsm.GetBodies(sys.Name);

                    if (jo != null && jo["bodies"] != null)
                    {
                        foreach (JObject edsmbody in jo["bodies"])
                        {
                            try
                            {
                                JObject jbody = EDSMClass.ConvertFromEDSMBodies(edsmbody);

                                JournalScan js = new JournalScan(jbody);

                                bodies.Add(js);
                            }
                            catch (Exception ex)
                            {
                                BaseUtils.HttpCom.WriteLog($"Exception Loop: {ex.Message}", "");
                                BaseUtils.HttpCom.WriteLog($"ETrace: {ex.StackTrace}", "");
                                Trace.WriteLine($"Exception Loop: {ex.Message}");
                                Trace.WriteLine($"ETrace: {ex.StackTrace}");
                            }
                        }

                        EDSMBodiesCache[sys.Name] = bodies;

                        System.Diagnostics.Debug.WriteLine("EDSM Web Lookup complete " + sys.Name + " " + bodies.Count);
                        return new Tuple<List<JournalScan>, bool>(bodies, false);       // not from cache
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("EDSM Web Lookup complete no info");
                        EDSMBodiesCache[sys.Name] = null;
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception: {ex.Message}");
                Trace.WriteLine($"ETrace: {ex.StackTrace}");
            }

            return null;
        }

        // Verified Nov 20,  by scan panel
        private static JObject ConvertFromEDSMBodies(JObject jo)        // protect yourself against bad JSON
        {
            //System.Diagnostics.Debug.WriteLine($"EDSM Body {jo.ToString(true)}");
            JObject jout = new JObject
            {
                ["timestamp"] = DateTime.UtcNow.ToStringZuluInvariant(),
                ["event"] = "Scan",
                ["EDDFromEDSMBodie"] = true,
                ["BodyName"] = jo["name"],
                ["SystemAddress"] = jo["id64"].Long(0),
                ["WasDiscovered"] = true,
                ["WasMapped"] = false,
                ["ScanType"] = "Detailed",
            };

            if (!jo["discovery"].IsNull())       // much more defense around this.. EDSM gives discovery=null back
            {
                jout["discovery"] = jo["discovery"];
            }

            if (jo["orbitalInclination"].DoubleNull() != null) jout["OrbitalInclination"] = jo["orbitalInclination"];
            if (jo["orbitalEccentricity"].DoubleNull() != null) jout["Eccentricity"] = jo["orbitalEccentricity"];
            if (jo["argOfPeriapsis"].DoubleNull() != null) jout["Periapsis"] = jo["argOfPeriapsis"];
            if (jo["semiMajorAxis"].Double() != 0) jout["SemiMajorAxis"] = jo["semiMajorAxis"].Double() * BodyPhysicalConstants.oneAU_m; // AU -> metres
            if (jo["orbitalPeriod"].Double() != 0) jout["OrbitalPeriod"] = jo["orbitalPeriod"].Double() * BodyPhysicalConstants.oneDay_s; // days -> seconds
            if (jo["rotationalPeriodTidallyLocked"].BoolNull() != null) jout["TidalLock"] = jo["rotationalPeriodTidallyLocked"];
            if (jo["axialTilt"].DoubleNull() != null) jout["AxialTilt"] = jo["axialTilt"].Double() * Math.PI / 180.0; // degrees -> radians
            if (jo["rotationalPeriod"].Double() != 0) jout["RotationalPeriod"] = jo["rotationalPeriod"].Double() * BodyPhysicalConstants.oneDay_s; // days -> seconds
            if (jo["surfaceTemperature"].DoubleNull() != null) jout["SurfaceTemperature"] = jo["surfaceTemperature"];
            if (jo["distanceToArrival"].DoubleNull() != null) jout["DistanceFromArrivalLS"] = jo["distanceToArrival"];
            if (jo["parents"].Array() != null) jout["Parents"] = jo["parents"];
            if (jo["id64"].ULongNull() != null) jout["BodyID"] = jo["id64"].Long() >> 55;

            if (!jo["type"].IsNull())
            {
                if (jo["type"].Str().Equals("Star"))
                {
                    jout["StarType"] = EDSMStar2JournalName(jo["subType"].StrNull());           // pass thru null to code, it will cope with it
                    jout["Age_MY"] = jo["age"];
                    jout["StellarMass"] = jo["solarMasses"];
                    jout["Radius"] = jo["solarRadius"].Double() * BodyPhysicalConstants.oneSolRadius_m; // solar-rad -> metres
                }
                else if (jo["type"].Str().Equals("Planet"))
                {
                    jout["Landable"] = jo["isLandable"];
                    jout["MassEM"] = jo["earthMasses"];

                    jout["SurfaceGravity"] = jo["gravity"].Double() * BodyPhysicalConstants.oneGee_m_s2;      // if not there, we get 0

                    jout["Volcanism"] = jo["volcanismType"];
                    string atmos = jo["atmosphereType"].StrNull();
                    if (atmos != null && atmos.IndexOf("atmosphere", StringComparison.InvariantCultureIgnoreCase) == -1)
                        atmos += " atmosphere";
                    jout["Atmosphere"] = atmos;
                    jout["Radius"] = jo["radius"].Double() * 1000.0; // km -> metres
                    jout["PlanetClass"] = EDSMPlanet2JournalName(jo["subType"].Str());
                    if (jo["terraformingState"] != null) jout["TerraformState"] = jo["terraformingState"];
                    if (jo["surfacePressure"] != null) jout["SurfacePressure"] = jo["surfacePressure"].Double() * BodyPhysicalConstants.oneAtmosphere_Pa; // atmospheres -> pascals
                    if (jout["TerraformState"].Str() == "Candidate for terraforming")
                        jout["TerraformState"] = "Terraformable";
                }
            }


            JArray rings = (jo["belts"] ?? jo["rings"]) as JArray;

            if (!rings.IsNull())
            {
                JArray jring = new JArray();

                foreach (JObject ring in rings)
                {
                    jring.Add(new JObject
                    {
                        ["InnerRad"] = ring["innerRadius"].Double() * 1000,
                        ["OuterRad"] = ring["outerRadius"].Double() * 1000,
                        ["MassMT"] = ring["mass"],
                        ["RingClass"] = ring["type"].Str().Replace(" ", ""),// turn to string, and EDSM reports "Metal Rich" etc so get rid of space
                        ["Name"] = ring["name"]
                    });
                }

                jout["Rings"] = jring;
            }

            if (!jo["materials"].IsNull())  // Check if materials has stuff
            {
                Dictionary<string, double?> mats;
                Dictionary<string, double> mats2;
                mats = jo["materials"]?.ToObjectQ<Dictionary<string, double?>>();
                mats2 = new Dictionary<string, double>();

                foreach (string key in mats.Keys)
                {
                    if (mats[key] == null)
                        mats2[key.ToLowerInvariant()] = 0.0;
                    else
                        mats2[key.ToLowerInvariant()] = mats[key].Value;
                }

                jout["Materials"] = JObject.FromObject(mats2);
            }

            return jout;
        }

        private static Dictionary<string, string> EDSM2PlanetNames = new Dictionary<string, string>()
        {
            // EDSM name    (lower case)            Journal name                  
            { "rocky ice world",                    "Rocky ice body" },
            { "high metal content world" ,          "High metal content body"},
            { "class i gas giant",                  "Sudarsky class I gas giant"},
            { "class ii gas giant",                 "Sudarsky class II gas giant"},
            { "class iii gas giant",                "Sudarsky class III gas giant"},
            { "class iv gas giant",                 "Sudarsky class IV gas giant"},
            { "class v gas giant",                  "Sudarsky class V gas giant"},
            { "earth-like world",                   "Earthlike body" },
        };

        private static Dictionary<string, string> EDSM2StarNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            // EDSM name (lower case)               Journal name
            { "a (blue-white super giant) star", "A_BlueWhiteSuperGiant" },
            { "b (blue-white super giant) star", "B_BlueWhiteSuperGiant" },
            { "f (white super giant) star", "F_WhiteSuperGiant" },
            { "g (white-yellow super giant) star", "G_WhiteSuperGiant" },
            { "k (yellow-orange giant) star", "K_OrangeGiant" },
            { "m (red giant) star", "M_RedGiant" },
            { "m (red super giant) star", "M_RedSuperGiant" },
            { "black hole", "H" },
            { "c star", "C" },
            { "cj star", "CJ" },
            { "cn star", "CN" },
            { "herbig ae/be star", "AeBe" },
            { "ms-type star", "MS" },
            { "neutron star", "N" },
            { "s-type star", "S" },
            { "t tauri star", "TTS" },
            { "wolf-rayet c star", "WC" },
            { "wolf-rayet n star", "WN" },
            { "wolf-rayet nc star", "WNC" },
            { "wolf-rayet o star", "WO" },
            { "wolf-rayet star", "W" },
        };

        private static string EDSMPlanet2JournalName(string inname)
        {
            return EDSM2PlanetNames.ContainsKey(inname.ToLowerInvariant()) ? EDSM2PlanetNames[inname.ToLowerInvariant()] : inname;
        }

        private static string EDSMStar2JournalName(string startype)
        {
            if (startype == null)
                startype = "Unknown";
            else if (EDSM2StarNames.ContainsKey(startype))
                startype = EDSM2StarNames[startype];
            else if (startype.StartsWith("White Dwarf (", StringComparison.InvariantCultureIgnoreCase))
            {
                int start = startype.IndexOf("(") + 1;
                int len = startype.IndexOf(")") - start;
                if (len > 0)
                    startype = startype.Substring(start, len);
            }
            else   // Remove extra text from EDSM   ex  "F (White) Star" -> "F"
            {
                int index = startype.IndexOf("(");
                if (index > 0)
                    startype = startype.Substring(0, index).Trim();
            }
            return startype;
        }

        #endregion

        #region Journal Events

        public List<string> GetJournalEventsToDiscard()     // protect yourself against bad JSON
        {
            string action = "api-journal-v1/discard";
            var response = RequestGet(action);
            if (response.Body != null)
                return JArray.Parse(response.Body).Select(v => v.Str()).ToList();
            else
                return null;
        }

        // Visual inspection Nov 20

        public List<JObject> SendJournalEvents(List<JObject> entries, string gameversion, string gamebuild, out string errmsg)
        {
            JArray message = new JArray(entries);

            string postdata = "commanderName=" + Uri.EscapeDataString(commanderName) +
                              "&apiKey=" + Uri.EscapeDataString(apiKey) +
                              "&fromSoftware=" + Uri.EscapeDataString(SoftwareName) +
                              "&fromSoftwareVersion=" + Uri.EscapeDataString(fromSoftwareVersion) +
                              "&fromGameVersion=" + Uri.EscapeDataString(gameversion) +
                              "&fromGameBuild=" + Uri.EscapeDataString(gamebuild) +
                              "&message=" + EscapeLongDataString(message.ToString());

            // System.Diagnostics.Debug.WriteLine("EDSM Send " + message.ToString());

            MimeType = "application/x-www-form-urlencoded";
            var response = RequestPost(postdata, "api-journal-v1", handleException: true);

            if (response.Error)
            {
                errmsg = response.StatusCode.ToString();
                return null;
            }

            try
            {
                JObject resp = JObject.ParseThrowCommaEOL(response.Body);
                errmsg = resp["msg"].Str();

                int msgnr = resp["msgnum"].Int();

                if (msgnr >= 200 || msgnr < 100)
                {
                    return null;
                }

                return resp["events"].Select(e => (JObject)e).ToList();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Failed due to " + e.ToString());
                errmsg = e.ToString();
                return null;
            }
        }

        #endregion

        public static bool DownloadGMOFileFromEDSM(string file)
        {
            try
            {
                EDSMClass edsm = new EDSMClass();
                string url = EDSMClass.ServerAddress + "en/galactic-mapping/json-edd";
                bool newfile;

                return BaseUtils.DownloadFile.HTTPDownloadFile(url, file, false, out newfile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("DownloadFromEDSM exception:" + ex.Message);
            }

            return false;
        }



    }
}
