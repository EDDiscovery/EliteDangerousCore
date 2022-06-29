/*
 * Copyright © 2022 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore
{
    public class HistoryListQueries       // search an HE list for items using the quries
    {
        public static HistoryListQueries Instance      // one instance across all profiles/panels so list is unified - if required.
        {
            get
            {
                if (instance == null)
                    instance = new HistoryListQueries();
                return instance;
            }
        }

        public static HashSet<JournalTypeEnum> SearchableJournalTypes { get; } = new HashSet<JournalTypeEnum> { JournalTypeEnum.Scan, JournalTypeEnum.FSSBodySignals, JournalTypeEnum.SAASignalsFound };

        public enum QueryType { BuiltIn, User, Example };

        public class Query
        {
            public string Name { get; set; }
            public string Condition { get; set; }

            public QueryType QueryType { get; set; }

            public Query(string n, string c, QueryType qt) { Name = n;Condition = c; QueryType = qt; }

            public bool User { get { return QueryType == QueryType.User; } }
            public bool UserOrBuiltIn { get { return QueryType == QueryType.BuiltIn || QueryType == QueryType.User; } }
        }

        public List<Query> Searches = new List<Query>()
            {
                new Query("Planet inside inner ring","IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis <= Parent.RingsInnerm And Parent.IsPlanet IsTrue", QueryType.BuiltIn ),
                new Query("Planet inside rings","IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis <= Parent.RingsOuterm And Parent.IsPlanet IsTrue", QueryType.BuiltIn ),
                new Query("Planet between inner and outer ring","IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis >= Parent.RingsInnerm And nSemiMajorAxis <= Parent.RingsOuterm And Parent.IsPlanet IsTrue", QueryType.BuiltIn ),
                new Query("Planet between rings 1 and 2","IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis >= Parent.Rings[1]_OuterRad And nSemiMajorAxis <= Parent.Rings[2]_InnerRad", QueryType.BuiltIn ),
                new Query("Planet between rings 2 and 3","IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis >= Parent.Rings[2]_OuterRad And nSemiMajorAxis <= Parent.Rings[3]_InnerRad", QueryType.BuiltIn ),

                new Query("Heavier than Sol","nStellarMass > 1", QueryType.BuiltIn ),
                new Query("Bigger than Sol","nRadius > 695700000", QueryType.BuiltIn ),

                new Query("Landable","IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Landable and Terraformable","IsPlanet IsTrue And IsLandable IsTrue And Terraformable IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Atmosphere","IsPlanet IsTrue And IsLandable IsTrue And HasAtmosphere IsTrue", QueryType.BuiltIn ),
                new Query("Landable with High G","IsPlanet IsTrue And IsLandable IsTrue And nSurfaceGravityG >= 3", QueryType.BuiltIn ),
                new Query("Landable large planet","IsPlanet IsTrue And IsLandable IsTrue And nRadius >= 12000000", QueryType.BuiltIn ),
                new Query("Landable with Rings","IsPlanet IsTrue And IsLandable IsTrue And HasRings IsTrue", QueryType.BuiltIn ),
                new Query("Has Volcanism","HasMeaningfulVolcanism IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Volcanism","HasMeaningfulVolcanism IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Earth Like planet","Earthlike IsTrue", QueryType.BuiltIn ),
                new Query("Bigger than Earth","IsPlanet IsTrue And nMassEM > 1", QueryType.BuiltIn ),
                new Query("Hotter than Hades","IsPlanet IsTrue And nSurfaceTemperature >= 2273", QueryType.BuiltIn ),

                new Query("Has Rings","HasRings IsTrue", QueryType.BuiltIn ),
                new Query("Star has Rings","HasRings IsTrue And IsStar IsTrue", QueryType.BuiltIn ),
                new Query("Has Belts","HasBelts IsTrue", QueryType.BuiltIn ),

                new Query("Planet has wide rings vs radius","(IsPlanet IsTrue And HasRings IsTrue ) And ( Rings[Iter1]_OuterRad-Rings[Iter1]_InnerRad >= nRadius*5)", QueryType.BuiltIn ),

                new Query("Close orbit to parent","IsPlanet IsTrue And Parent.IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And Parent.nRadius*3 > nSemiMajorAxis", QueryType.BuiltIn ),

                new Query("Close to ring",
                                "( IsPlanet IsTrue And Parent.IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse ) And " +
                                "( \"Abs(Parent.Rings[Iter1]_InnerRad-nSemiMajorAxis)\" < nRadius*10 Or  \"Abs(Parent.Rings[Iter1]_OuterRad-nSemiMajorAxis)\" < nRadius*10 )"
                    , QueryType.BuiltIn ),

                new Query("Planet with a large number of Moons","IsPlanet IsTrue And Child.Count >= 8", QueryType.BuiltIn ),
                new Query("Moon of a Moon","Level == 3", QueryType.BuiltIn ),
                new Query("Moons orbiting Terraformables","Level >= 2 And Parent.Terraformable IsTrue", QueryType.BuiltIn ),
                new Query("Moons orbiting Earthlike","Level >= 2 And Parent.Earthlike IsTrue", QueryType.BuiltIn ),

                new Query("Close Binary","IsPlanet IsTrue And IsOrbitingBaryCentre IsTrue And Sibling.Count == 1 And nRadius/nSemiMajorAxis > 0.4 And " +
                    "Sibling[1].nRadius/Sibling[1].nSemiMajorAxis > 0.4", QueryType.BuiltIn ),

                new Query("Gas giant has a terraformable Moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].Terraformable IsTrue )", QueryType.BuiltIn ),
                new Query("Gas giant has a large moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius >= 5000000 )", QueryType.BuiltIn ),
                new Query("Gas giant has a tiny moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius <= 500000 )", QueryType.BuiltIn ),

                new Query("Tiny Moon","Level >= 2 And nRadius < 300000", QueryType.BuiltIn ),
                new Query("Fast Rotation of a non tidally locked body","Level >= 1 And nTidalLock IsFalse And Abs(nRotationPeriod) < 3600", QueryType.BuiltIn ),
                new Query("Fast Orbital period","Level >= 1 And nOrbitalPeriod < 28800", QueryType.BuiltIn ),
                new Query("High Eccentric Orbit","Level >= 1 And nEccentricity > 0.9", QueryType.BuiltIn ),
                new Query("Low Eccentricity Orbit","Level >= 1 And nEccentricity <= 0.01", QueryType.BuiltIn ),
                new Query("Tidal Lock","IsPlanet IsTrue And nTidalLock == 1", QueryType.BuiltIn ),

                new Query("High number of Jumponium Materials","IsLandable IsTrue And JumponiumCount >= 5", QueryType.BuiltIn ),

                new Query("Contains Geo Signals",            "ContainsGeoSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Bio Signals",            "ContainsBioSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Thargoid Signals",       "ContainsThargoidSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Guardian Signals",       "ContainsGuardianSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Human Signals",          "ContainsHumanSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Other Signals",          "ContainsOtherSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Uncategorised Signals",  "ContainsUncategorisedSignals IsTrue", QueryType.BuiltIn ),

                new Query("Body Name","BodyName contains <name>", QueryType.Example ),
                new Query("Scan Type","ScanType contains Detailed", QueryType.Example ),
                new Query("Distance (ls)","DistanceFromArrivalLS >= 20", QueryType.Example ),
                new Query("Rotation Period (s)","nRotationPeriod >= 30", QueryType.Example ),
                new Query("Rotation Period (days)","nRotationPeriodDays >= 1", QueryType.Example ),
                new Query("Radius (m)","nRadius >= 100000", QueryType.Example ),
                new Query("Radius (sols)","nRadiusSols >= 1", QueryType.Example ),
                new Query("Radius (Earth)","nRadiusEarths >= 1", QueryType.Example ),
                new Query("Semi Major Axis (m)","nSemiMajorAxis >= 20000000", QueryType.Example ),
                new Query("Semi Major Axis (AU)","nSemiMajorAxisAU >= 1", QueryType.Example ),
                new Query("Orbital Inclination (Deg)","nOrbitalInclination > 1", QueryType.Example ),
                new Query("Periapsis (Deg)","nPeriapsis > 1", QueryType.Example ),
                new Query("Orbital period (s)","nOrbitalPeriod > 200", QueryType.Example ),
                new Query("Orbital period (days)","nOrbitalPeriodDays > 200", QueryType.Example ),
                new Query("Axial Tilt (Deg)","nAxialTiltDeg > 1", QueryType.Example ),

                new Query("Star Type","StarType $== A", QueryType.Example ),
                new Query("Star Magnitude","nAbsoluteMagnitude >= 1", QueryType.Example ),
                new Query("Star Age (MY)","nAge >= 2000", QueryType.Example ),
                new Query("Star Luminosity","Luminosity $== V", QueryType.Example ),

                new Query("Planet Materials","MaterialList contains \"iron\"", QueryType.Example ),
                new Query("Planet Class","PlanetClass $== \"High metal content body\"", QueryType.Example ),
                new Query("Terraformable","Terraformable Isfalse", QueryType.Example ),
                new Query("Atmosphere","Atmosphere $== \"thin sulfur dioxide atmosphere\"", QueryType.Example ),
                new Query("Atmosphere ID","AtmosphereID $== \"Carbon_dioxide\"", QueryType.Example ),
                new Query("Atmosphere Property","AtmosphereProperty $== \"Rich\"", QueryType.Example ),
                new Query("Volcanism","Volcanism $== \"minor metallic magma volcanism\"", QueryType.Example ),
                new Query("Volcanism ID","VolcanismID $== \"Ammonia_Magma\"", QueryType.Example ),
                new Query("Surface Gravity m/s","nSurfaceGravity >= 9.6", QueryType.Example ),
                new Query("Surface Gravity G","nSurfaceGravityG >= 1.0", QueryType.Example ),
                new Query("Surface Pressure (Pa)","nSurfacePressure >= 101325", QueryType.Example ),
                new Query("Surface Pressure (Earth Atmos)","nSurfacePressureEarth >= 1", QueryType.Example ),
            };

        static private HistoryListQueries instance = null;
        private string DbUserQueries { get { return "UCSearchScansUserQuery"; } }  // not keyed to profile or to panel, global

        private char splitmarker = (char)0x2b1c; // horrible but i can't be bothered to do a better implementation at this point

        private HistoryListQueries()
        {
            string[] userqueries = DB.UserDatabase.Instance.GetSettingString(DbUserQueries, "").Split(new char[] { splitmarker }); // allowed use

            for (int i = 0; i + 1 < userqueries.Length; i += 2)
                Searches.Add(new Query(userqueries[i], userqueries[i + 1],QueryType.User));
        }

        public void Set(string name, string expr, QueryType t)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
                Searches[entry] = new Query(name, expr, t);
            else
                Searches.Add(new Query(name, expr, t));
        }

        public void Delete(string name)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
            {
                Searches.RemoveAt(entry);
            }
        }

        public void SaveUserQueries()
        {
            string userqueries = "";
            foreach (var q in Searches.Where(x => x.User))
            {
                userqueries += q.Name + splitmarker + q.Condition + splitmarker;
            }

            DB.UserDatabase.Instance.PutSettingString(DbUserQueries, userqueries); // allowed use
        }

        public JArray QueriesInJSON(QueryType t)
        {
            JArray ja = new JArray();
            foreach (var q in Searches.Where(x => x.QueryType == t))
            {
                JObject query = new JObject();
                query["Name"] = q.Name;
                query["Condition"] = q.Condition;
                query["Type"] = q.QueryType.ToString();
                ja.Add(query);
            }

            return ja;
        }
        public bool ReadJSONQueries(JArray ja)
        {
            foreach( var t in ja)
            {
                JObject to = t.Object();
                if (to != null)
                {
                    string name = to["Name"].StrNull();
                    string condition = to["Condition"].StrNull();
                    if (name != null && condition != null && Enum.TryParse<QueryType>(to["Type"].Str(), true, out QueryType qt))
                    {
                        Set(name, condition, qt);
                    }
                    else
                        return false;
                }
                else
                    return false;
            }

            return true;
        }


        //find a named search, async
        public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist, Dictionary<string, Results> results, string searchname, BaseUtils.Variables defaultvars, bool wantdebug)
        {
            var search = Searches.Find(x => x.Name.Equals(searchname));

            var cond = search != null ? new BaseUtils.ConditionLists(search.Condition) : null;

            if (cond == null)
                System.Diagnostics.Trace.WriteLine($"Search missing {searchname}");

            return Find(helist, results, searchname, cond, defaultvars, wantdebug);
        }

        public class Results
        {
            public HistoryEntry HistoryEntry { get; set; }
            public List<string> FiltersPassed { get; set; }
        }


        // find using cond, async. return string of result info.  Fill in results dictionary (already made)
        // default vars can be null
        static public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist, 
                                   Dictionary<string,Results> results, string filterdescription,
                                   BaseUtils.ConditionLists cond, BaseUtils.Variables defaultvars, bool wantreport)
        {

            return System.Threading.Tasks.Task.Run(() =>
            {
                if (cond == null || cond.Count == 0)
                    return "Search Not Found";

                StringBuilder resultinfo = new StringBuilder(10000);

                var allvars = BaseUtils.Condition.EvalVariablesUsed(cond.List);

                bool wantiter1 = allvars.Contains("Iter1");
                bool wantiter2 = allvars.Contains("Iter2");
                bool wantjumponium = allvars.Contains("JumponiumCount");
                bool wantsiblingcount = allvars.Contains("Sibling.Count");
                bool wantchildcount = allvars.Contains("Child.Count");
                bool wantlevel = allvars.Contains("Level");

                // extract variables needed to be filled in by the AddPropertiesFieldsOfClass function. We extract only the ones we need for speed reason.
                // Variables using the Name[] format, or Class_subclass naming system need to have the [ and _ text stripped off for the property expander to iterate thru them

                string[] stoptext = new string[] { "[", "_" };
                HashSet<string> varsparent = allvars.Where(x => x.StartsWith("Parent.")).Select(x => x.Substring(7, x.IndexOfOrLength(stoptext) - 7)).ToHashSet();
                HashSet<string> varssiblings = allvars.Where(x => x.StartsWith("Sibling[")).Select(x => x.Substring(x.IndexOfOrLength("]", offset: 2))).Select(x => x.Substring(0, x.IndexOfOrLength(stoptext))).ToHashSet();
                HashSet<string> varschildren = allvars.Where(x => x.StartsWith("Child[")).Select(x => x.Substring(x.IndexOfOrLength("]", offset: 2))).Select(x => x.Substring(0, x.IndexOfOrLength(stoptext))).ToHashSet();
                HashSet<string> varsevent = allvars.Where(x => !x.StartsWith("Parent.") && !x.StartsWith("Sibling") && !x.StartsWith("Child[")).Select(x => x.Substring(0, x.IndexOfOrLength(stoptext))).ToHashSet();

                foreach (var he in helist)
                {
                    BaseUtils.Variables scandatavars = defaultvars != null ? new BaseUtils.Variables(defaultvars) : new BaseUtils.Variables();

                  //  if (he.EntryType != JournalTypeEnum.Scan) continue;

                    scandatavars.AddPropertiesFieldsOfClass(he.journalEntry, "",
                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                            varsevent);

                    if ( wantjumponium )
                    {
                        JournalScan js = he.journalEntry as JournalScan;

                        if (js != null)   // if its a journal scan
                        {
                            scandatavars["JumponiumCount"] = js.Jumponium().ToStringInvariant();
                        }
                    }

                    if (he.ScanNode != null)      // if it has a scan node
                    {
                        if ( wantlevel )
                            scandatavars["Level"] = he.ScanNode.Level.ToStringInvariant();

                        if (he.ScanNode.Parent != null) // if we have a parent..
                        {
                            if (varsparent.Count > 0)
                            {
                                var parentjs = he.ScanNode.Parent.ScanData;               // parent journal entry, may be null

                                if (parentjs != null) // if want parent scan data
                                {
                                    scandatavars.AddPropertiesFieldsOfClass(parentjs, "Parent.",
                                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                            varsparent);
                                    scandatavars["Parent.Level"] = he.ScanNode.Parent.Level.ToStringInvariant();
                                }
                            }

                            if ( wantsiblingcount )
                                scandatavars["Sibling.Count"] = ((he.ScanNode.Parent.Children.Count - 1)).ToStringInvariant();      // count of children or parent less ours
                            
                            if ( wantchildcount)
                                scandatavars["Child.Count"] = ((he.ScanNode.Children?.Count ?? 0)).ToStringInvariant();      // count of children

                            if (varssiblings.Count > 0)        // if want sibling[
                            {
                                int cno = 1;
                                foreach (var sn in he.ScanNode.Parent.Children)
                                {
                                    if (sn.Value != he.ScanNode && sn.Value.ScanData != null)        // if not ours and has a scan
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(sn.Value.ScanData, $"Sibling[{cno}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varssiblings);
                                        cno++;
                                    }
                                }
                            }

                            if (varschildren.Count > 0)        // if want children[
                            {
                                int cno = 1;
                                foreach (var sn in he.ScanNode.Children.EmptyIfNull())
                                {
                                    if (sn.Value.ScanData != null)        // if not ours and has a scan
                                    {
                                        int cc = scandatavars.Count;

                                        scandatavars.AddPropertiesFieldsOfClass(sn.Value.ScanData, $"Child[{cno}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varschildren);

                                        if (scandatavars.Count > cc)
                                        {

                                        }
                                        cno++;
                                    }
                                }
                            }
                        }
                    }

                    if (wantiter1)      // set up default iter1
                        scandatavars["Iter1"] = "1";
                    if (wantiter2)      // set up default iter2
                        scandatavars["Iter2"] = "1";

                    bool debugit = false;

                    //if (he.ScanNode?.ScanData?.Barycentre != null)
                    //{
                    //}
                    //if (scandatavars.Contains("IsOrbitingBaryCentre") && scandatavars["IsOrbitingBaryCentre"] == "1")
                    //{
                    //}
                    //JournalScan js2 = he.journalEntry as JournalScan;
                    //if (js2.BodyName.Equals("Oufaish IG-Y e26 3"))  
                    //    debugit = true;

                    bool? res = BaseUtils.ConditionLists.CheckConditionsEvalIterate(cond.List, scandatavars, out string evalerrlist, out BaseUtils.ConditionLists.ErrorClass errclassunused, wantiter1 || wantiter2 , debugit: debugit);

                    if (wantreport && evalerrlist.HasChars())
                    {
                        resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} : {evalerrlist}");
                        // System.Diagnostics.Debug.WriteLine($"For entry type {he.EventTimeUTC} {he.EntryType} error: {resultinfo}");
                    }

                    if (res.HasValue && res.Value == true)
                    {
                        //if we have a je with a body name, use that to set the ret, else just use a incrementing decimal count name
                        string key = he.journalEntry is IBodyNameIDOnly ? (he.journalEntry as IBodyNameIDOnly).BodyName : results.Count.ToStringInvariant();

                        if ( wantreport)
                            resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} : Matched {key}");

                        lock (results)     // may be spawing a lot of parallel awaits, make sure the shared resource is locked
                        {
                            if (results.TryGetValue(key, out Results value))       // if key already exists, set HE to us, and update filters passed
                            {
                                value.HistoryEntry = he;
                                if (!value.FiltersPassed.Contains(filterdescription))      // we may scan and find the same body twice with the same filter, do not dup add
                                    value.FiltersPassed.Add(filterdescription);
                            }
                            else
                            {                                                       // else make a new key
                                results[key] = new Results() { HistoryEntry = he, FiltersPassed = new List<string>() { filterdescription } };
                            }
                        }
                    }
                }

                return resultinfo.ToString();
            });
        }


    };


}
