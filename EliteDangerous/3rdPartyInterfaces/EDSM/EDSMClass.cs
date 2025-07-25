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

        public const string RootURL = "https://www.edsm.net/";

        public EDSMClass() : base(RootURL)
        {
            var assemblyFullName = Assembly.GetEntryAssembly().FullName;
            fromSoftwareVersion = assemblyFullName.Split(',')[1].Split('=')[1];

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

            var response = RequestPost("{ \"data\": " + query + " }", "api-v1/submit-distances");
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
            var response = RequestGet("api-v1/" + query);
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

            var response = RequestGet(query, timeout: timeout);
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
        public BaseUtils.HttpCom.Response RequestSystemsData(DateTime startdate, DateTime enddate, int timeout = 5000)      // protect yourself against JSON errors!
        {
            if (startdate < EDDFixesDates.EDSMMinimumSystemsDate)
                startdate = EDDFixesDates.EDSMMinimumSystemsDate;

            string query = "api-v1/systems" +
                "?startdatetime=" + HttpUtility.UrlEncode(startdate.ToUniversalTime().ToStringYearFirstInvariant()) +
                "&enddatetime=" + HttpUtility.UrlEncode(enddate.ToUniversalTime().ToStringYearFirstInvariant()) +
                "&coords=1&known=1&showId=1";
            return RequestGet(query, timeout: timeout);
        }

        public string GetHiddenSystems(string file, System.Threading.CancellationToken cancel)   // Verfied Nov 20
        {
            if (DownloadFile(cancel,"api-v1/hidden-systems?showId=1", file, false, out bool newfile))
            {
                string json = BaseUtils.FileHelpers.TryReadAllTextFromFile(file);
                return json;
            }
            else
                return null;
        }

        #endregion

        #region Comment sync

        private string GetComments(DateTime starttime)
        {
            if (!ValidCredentials)
                return null;

            string query = "get-comments?startdatetime=" + HttpUtility.UrlEncode(starttime.ToStringYearFirstInvariant()) + "&apiKey=" + apiKey + "&commanderName=" + HttpUtility.UrlEncode(commanderName) + "&showId=1";
            var response = RequestGet("api-logs-v1/" + query);

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

            var response = RequestPost(query, "api-logs-v1/set-comment", contenttype: "application/x-www-form-urlencoded");

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
                            out DateTime logstarttime, out DateTime logendtime, out BaseUtils.HttpCom.Response response, Action<string> statusupdate)
        {
            fsdjumps = new List<JournalFSDJump>();
            logstarttime = DateTime.MaxValue;
            logendtime = DateTime.MinValue;
            response = new BaseUtils.HttpCom.Response(HttpStatusCode.Unauthorized);

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

            response = RequestGet("api-logs-v1/" + query);

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

                    statusupdate($"EDSM Log Fetcher got {logs.Count()} log entries from EDSM, from UTC {logstarttime} to {logendtime}");

                    var systems = new List<Tuple<ISystem,JObject>>();

                    // since EDSM does not give xyz in the log response, first see if any systems are in our DB

                    SystemsDatabase.Instance.DBRead(db =>
                    {
                        foreach (JObject jo in logs)
                        {
                            string name = jo["system"].Str();
                            ISystem sc = SystemCache.FindSystemInCacheDB(new SystemClass(name), db);      // find in our DB only.  may be null
                            if (sc != null)     // yes it is
                            {
                                if (!sc.SystemAddress.HasValue)                                     // fill in any values
                                    sc.SystemAddress = jo["systemId64"].LongNull();
                                if (!sc.EDSMID.HasValue)
                                    sc.EDSMID = jo["systemId"].LongNull();
                            }
                            else
                            {
                            }
                            systems.Add(new Tuple<ISystem,JObject>(sc,jo));    // sc may be null
                        }
                    });

                    // now some systems may not be in the database, or the database is empty
                    // we can fill in with edsm queries

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
                            DateTime etutc = DateTime.ParseExact(jo["date"].Str(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal); // UTC time
                            JournalFSDJump fsd = new JournalFSDJump(etutc, systems[i].Item1, EDCommander.Current.MapColour, true);
                            fsd.LocOrJumpSource = SystemSource.FromEDSM;
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

                var response = RequestGet(query);
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

            var response = RequestGet(query);
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
                    SystemClass sys = new SystemClass(sysname["name"].Str("Unknown"), sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);

                    if (sys.Name.Equals(systemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        JObject co = (JObject)sysname["coords"];

                        if (co != null)
                        {
                            sys.X = co["x"].Double();
                            sys.Y = co["y"].Double();
                            sys.Z = co["z"].Double();
                        }

                        if ( sys.Triage())
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

        // Verified April 23 with ID64 system address return. Sorted by distance
        public List<Tuple<ISystem, double>> GetSphereSystems(String systemName, double maxradius, double minradius)      // may return null
        {
            // api does not state id64, but tested 20/4/23 it supports it
            string query = String.Format("api-v1/sphere-systems?systemName={0}&radius={1}&minRadius={2}&showCoordinates=1&showId=1",
                                Uri.EscapeDataString(systemName), maxradius.ToStringInvariant(), minradius.ToStringInvariant());

            var response = RequestGet(query, timeout: 30000);
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
                            SystemClass sys = new SystemClass(sysname["name"].Str("Unknown"),sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);        // make a system from EDSM
                            JObject co = (JObject)sysname["coords"];
                            if (co != null)
                            {
                                sys.X = co["x"].Double();
                                sys.Y = co["y"].Double();
                                sys.Z = co["z"].Double();
                            }

                            if (sys.Triage())
                                systems.Add(new Tuple<ISystem, double>(sys, sysname["distance"].Double()));
                        }

                        // ensure sorted by distance
                        systems.Sort(delegate (Tuple<ISystem, double> left, Tuple<ISystem, double> right) { return left.Item2.CompareTo(right.Item2); });

                        return systems;
                    }
                }
                catch (Exception ex)      // json may be garbage
                {
                    System.Diagnostics.Debug.WriteLine("EDSM Sphere names failed due to " + ex);
                }
            }

            return null;
        }

        // Verified April 23 with ID64 system address return.  Sorted by distance
        public List<Tuple<ISystem, double>> GetSphereSystems(double x, double y, double z, double maxradius, double minradius)      // may return null
        {
            // api does not state id64, but tested 20/4/23 it supports it
            string query = String.Format("api-v1/sphere-systems?x={0}&y={1}&z={2}&radius={3}&minRadius={4}&showCoordinates=1&showId=1",
                                x.ToStringInvariant("0.##"), y.ToStringInvariant("0.##"), z.ToStringInvariant("0.##"), maxradius.ToStringInvariant("0.#"), minradius.ToStringInvariant("0.#"));

            System.Diagnostics.Debug.WriteLine($"EDSM Query sphere {x} {y} {z} at {minradius} - {maxradius} ly");

            var response = RequestGet(query, timeout: 30000);
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
                            SystemClass sys = new SystemClass(sysname["name"].Str("Unknown"), sysname["id"].Long(), sysname["id64"].LongNull(), SystemSource.FromEDSM);   // make a EDSM system
                            JObject co = (JObject)sysname["coords"];
                            if (co != null)
                            {
                                sys.X = co["x"].Double();
                                sys.Y = co["y"].Double();
                                sys.Z = co["z"].Double();
                            }

                            if (sys.Triage())
                                systems.Add(new Tuple<ISystem, double>(sys, sysname["distance"].Double()));

                            //System.Diagnostics.Debug.WriteLine($"  EDSM returned sphere {sys.Name} {sys.X} {sys.Y} {sys.Z} dist {dist}");
                        }

                        // ensure sorted by distance
                        systems.Sort(delegate (Tuple<ISystem, double> left, Tuple<ISystem, double> right) { return left.Item2.CompareTo(right.Item2); });

                       return systems;
                    }
                }
                catch (Exception ex)      // json may be garbage
                {
                    System.Diagnostics.Debug.WriteLine("EDSM Sphere systems failed due to " + ex);
                }
            }

            return null;
        }

        public string GetUrlToSystem(string sysName)            // get a direct name, no check if exists
        {
            string encodedSys = HttpUtility.UrlEncode(sysName);
            string url = ServerAddress + "system?systemName=" + encodedSys;
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
            var response = RequestGet("api-v1/" + query);
            if (response.Error)
                return "";

            JObject jo = response.Body?.JSONParseObject(JToken.ParseOptions.CheckEOL);   // null if no body, or not object

            if (jo != null)
                id = jo["id"].Long(-1);

            if (id == -1)
                return "";

            string url = ServerAddress + "system/id/" + id.ToStringInvariant() + "/name/" + encodedSys;
            return url;
        }

        // Verified april 23 with xyz return and edsmid/system 64 return
        public JObject GetSystemByAddress(long id64)
        {
            string query = "?systemId64=" + id64.ToStringInvariant() + "&showInformation=1&includeHidden=1&showCoordinates=1&&showId=1";
            var response = RequestGet("api-v1/system" + query);
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
            var response = RequestGet("api-v1/system" + query);
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
            var response = RequestGet("api-system-v1/" + query);
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
            var response = RequestGet("api-system-v1/" + query);
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
            var response = RequestGet("api-system-v1/" + query);
            if (response.Error)
                return null;

            var json = response.Body;
            if (json == null || json.ToString() == "[]")
                return null;

            JObject msg = JObject.Parse(json);
            return msg;
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
                              "&message=" + message.ToString().URIEscapeLongDataString();

            // System.Diagnostics.Debug.WriteLine("EDSM Send " + message.ToString());

            var response = RequestPost(postdata, "api-journal-v1", contenttype: "application/x-www-form-urlencoded");

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

        public static bool DownloadGMOFileFromEDSM(string file, System.Threading.CancellationToken cancel)
        {
            EDSMClass edsm = new EDSMClass();
            return edsm.DownloadFile(cancel, "en/galactic-mapping/json-edd", file, false, out bool _);
        }
    }
}
