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

using EliteDangerousCore.JournalEvents;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore.Spansh
{
    public partial class SpanshClass : BaseUtils.HttpCom
    {
        const string fileprefix = "spanshv4";

        #region Dump

        // get the dump file
        // System can be name, name and systemaddress, or systemaddress only (from jan 25)
        public static async System.Threading.Tasks.Task<JObject> GetSpanshDumpAsync(ISystem sys, bool weblookup = true, bool fromfilecache = true)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return GetSpanshDump(sys, weblookup, fromfilecache);
            });
        }

        // get the dump file
        // System can be name, name and systemaddress, or systemaddress only (from jan 25)
        public static JObject GetSpanshDump(ISystem sys, bool weblookup = true, bool fromfilecache = true)
        {
            JObject spanshdump = null;

            if (fromfilecache && EliteConfigInstance.InstanceOptions.ScanCacheEnabled)
                spanshdump = GetDumpFromFileCache(sys, fileprefix);

            if (spanshdump == null && weblookup)
            {
                SpanshClass sp = new SpanshClass();
                spanshdump = sp.GetSpanshSystemFromWeb(sys, out ISystem foundsystem);

                if (spanshdump != null && EliteConfigInstance.InstanceOptions.ScanCacheEnabled)        // if write file back..
                {
                    WriteToFileCache(spanshdump, foundsystem, fileprefix);
                }
            }

            return spanshdump;
        }

        #endregion

        #region Dump Decoded

        // class returning results
        [System.Diagnostics.DebuggerDisplay("System {System.Name} Bodies {Bodies} total {BodyCount}")]
        public class DumpResults
        {
            public ISystem System { get; set; }
            public List<JournalScan> Bodies { get; set; }       // always set to a list
            public int? BodyCount { get; set; }
            public List<StationInfo> Stations { get; set; }     // always set to a list
            public DumpResults(ISystem sys, List<JournalScan> list, int? bodycount, List<StationInfo> statlist) 
            { System = sys; Bodies = list; BodyCount = bodycount; Stations = statlist; }
        }

        public static void ClearBodyCache()
        {
            lock (BodyCache)
            {
                BodyCache.Clear();
            }
        }

        // only one request at a time going, this is to prevent multiple requests for the same body
        public static bool HasBodyLookupOccurred(ISystem sys)
        {
            lock (BodyCache)
            {
                return BodyCache.ContainsKey(sys.Key);
            }
        }
        // true if lookup occurred, but no data. false otherwise
        public static bool HasNoDataBeenStoredOnBody(ISystem sys)
        {
            lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
            {
                return BodyCache.TryGetValue(sys.Key, out DumpResults d) && d == null;
            }
        }

        // return list, if from cache, if web lookup occurred
        public async static System.Threading.Tasks.Task<DumpResults> ConvertDumpToJournalRecordsAsync(ISystem sys, bool weblookup = true)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                return ConvertDumpToJournalRecords(sys, weblookup);
            });
        }


        // System can be name, name and systemaddress, or systemaddress only (from jan 25)
        // sys can have name or address
        static public DumpResults ConvertDumpToJournalRecords(ISystem sys, bool weblookup = true)
        {
            try
            {
                lock (BodyCache) // only one request at a time going, this is to prevent multiple requests for the same body
                {
                    // System.Threading.Thread.Sleep(2000); //debug - delay to show its happening 
                    // System.Diagnostics.Debug.WriteLine("EDSM Cache check " + sys.EDSMID + " " + sys.SystemAddress + " " + sys.Name);

                    if (BodyCache.TryGetValue(sys.Key, out DumpResults we))
                    {
                        System.Diagnostics.Debug.WriteLine($"Spansh Body Cache hit on {sys.Name} {sys.SystemAddress} {we != null}");
                        // will return null, looked up not found, or bodies results found
                        return we;
                    }

                    JObject spanshdump = EliteConfigInstance.InstanceOptions.ScanCacheEnabled ? GetDumpFromFileCache(sys, fileprefix) : null;
                    bool lookedup = false;

                    if (spanshdump == null && weblookup)
                    {
                        SpanshClass sp = new SpanshClass();
                        spanshdump = sp.GetSpanshSystemFromWeb(sys, out ISystem foundsystem);
                        if (spanshdump != null)
                        {
                            sys = foundsystem;      // make sure its normalised
                            lookedup = true;
                        }
                        else
                        {
                            // System.Diagnostics.Debug.WriteLine($"Spansh Web Lookup complete no info {sys.Name} {sys.SystemAddress}");
                            // mark that we tried to lookup but we could not get any valid data
                            if (sys.HasName)
                                BodyCache[sys.Name.ToLowerInvariant()] = null;
                            if (sys.HasAddress)
                                BodyCache[sys.SystemAddress.Value.ToStringInvariant()] = null;
                        }
                    }

                    if (spanshdump != null)
                    {
                        var journalscans = GetJournalScansJsonsFromDump(spanshdump);

                        if (journalscans?.Count>0)         // we have data from file or from web
                        {
                            List<JournalScan> bodies = new List<JournalScan>();

                            SystemClass systemgot = new SystemClass(journalscans[0]["StarSystem"].Str(), journalscans[0]["SystemAddress"].LongNull());

                            foreach (JObject jo in journalscans)
                            {
                                JournalScan js = new JournalScan(jo.Object());

                                //System.Diagnostics.Debug.WriteLine($"Spansh JS: {js.DisplayString(null, null)}");

                                if (jo.Contains("EDDMeanAnomalyTimestamp"))        // this name is used to carry time info which is not in the journal
                                {
                                    DateTime t = jo["EDDMeanAnomalyTimestamp"].DateTimeUTC();
                                    js.EventTimeUTC = t;
                                }

                                bodies.Add(js);
                            }
                            
                            if (lookedup == true && EliteConfigInstance.InstanceOptions.ScanCacheEnabled)        // if write file back..
                            {
                                WriteToFileCache(spanshdump, sys, fileprefix);
                            }

                            int? bodycount = spanshdump["system"].I("bodyCount").IntNull();

                            var stations = GetStationsFromDump(spanshdump);

                            // place the body in the cache under both its name and its system address. We return system normalised, bodies and bodycount (if known)
                            var cdata = new DumpResults(systemgot, bodies, bodycount, stations);
                            BodyCache[systemgot.Name.ToLowerInvariant()] = cdata;
                            BodyCache[systemgot.SystemAddress.Value.ToStringInvariant()] = cdata;

                            // System.Diagnostics.Debug.WriteLine($"Spansh Web/File Lookup complete {sys.Name} {sys.SystemAddress} {bodies.Count} cache {fromcache}");
                            return cdata;
                        }
                        else
                        {
                            System.Diagnostics.Trace.WriteLine($"Spansh bodylist cannot decode dump");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Exception: {ex.Message}");
            }

            return null;
        }
 
        #endregion

        #region Implementation

        // use https://spansh.co.uk/api/dump/<systemid64> 

        private JObject GetSpanshSystemFromWeb(ISystem searchsystem, out ISystem foundsystem)
        {
            foundsystem = EnsureSystemAddressAndName(searchsystem);     // find the full system details incl name and system address

            if (foundsystem == null)        // if failed, return nothing
                return null;

            BaseUtils.HttpCom.Response response = RequestGet("dump/" + foundsystem.SystemAddress.ToStringInvariant());

            if (response.Error)
                return null;

            var data = response.Body;
            var spanshdump = JObject.Parse(data, JToken.ParseOptions.CheckEOL);

            //BaseUtils.FileHelpers.TryWriteToFile(@"c:\code\spanshbodies.json", spanshdump?.ToString(true));

            return spanshdump;
        }

        // get spansh data from the cache
        public static JObject GetDumpFromFileCache(ISystem searchsystem, string prefix)
        {
            // all files are enumerated to provide ability to search them for name and address
            var cachefiles = System.IO.Directory.EnumerateFiles(EliteConfigInstance.InstanceOptions.ScanCachePath, prefix + "__*.json").ToList();

            // find by name and address, using the pattern of naming to pick out the right parts
            int iname = cachefiles.FindIndex(x => x.Contains($"{prefix}__{searchsystem.Name.ToLowerInvariant().SafeFileString()}__"));     // by name, making sure we use the safe file string chars
            int iaddr = searchsystem.SystemAddress.HasValue ? cachefiles.FindIndex(x => x.Contains($"__{searchsystem.SystemAddress.Value.ToStringInvariant()}.json")) : -1;

            string cachefile = iaddr >= 0 ? cachefiles[iaddr] : iname >= 0 ? cachefiles[iname] : null;       // prefer address, else use name
            if (cachefile != null)
            {
                string cachedata = BaseUtils.FileHelpers.TryReadAllTextFromFile(cachefile); // try and read it
                if (cachedata != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"Spansh Cache File read on {sys.Name} {sys.SystemAddress} from {cachefile}");
                    JObject jo = JObject.Parse(cachedata, JToken.ParseOptions.CheckEOL);  // if so, try a conversion
                    if (jo != null)
                    {
                        return jo;
                    }
                }
            }

            return null;
        }

        public static void WriteToFileCache(JObject spanshdump, ISystem foundsystem, string prefix)
        {
            // give the finalised name to the cache file. 
            string name = $"{prefix}__{foundsystem.Name.ToLowerInvariant()}__{(foundsystem.SystemAddress ?? 0).ToStringInvariant()}.json";
            string cachefile = System.IO.Path.Combine(EliteConfigInstance.InstanceOptions.ScanCachePath, name.SafeFileString());
            BaseUtils.FileHelpers.TryWriteToFile(cachefile, spanshdump.ToString(true));      // save to file so we don't have to reload
        }

        public static JArray ReadFile(string file)
        {
            JToken tk = JToken.Parse( System.IO.File.ReadAllText(file));
            return GetJournalScansJsonsFromDump(tk.Object());
        }


        // BodyCache gets either the body results, or null marking no server data
        static private Dictionary<string, DumpResults> BodyCache = new Dictionary<string, DumpResults>();

        #endregion
    }
}

