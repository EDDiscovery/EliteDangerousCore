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

        public class Query
        {
            public string Name { get; set; }
            public string Condition { get; set; }
            public bool Standard { get; set; }
            public bool User { get; set; }

            public Query(string n, string c, bool std = false, bool user = false) { Name = n;Condition = c;Standard = std; User = user; }
        }

        public List<Query> Searches = new List<Query>()
            {
                new Query("Planet inside outer ring","IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis <= Parent.RingsOuterm And Parent.IsPlanet IsTrue",true ),
                new Query("Planet inside inner ring","IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis <= Parent.RingsInnerm And Parent.IsPlanet IsTrue",true ),
                new Query("Planet inside the rings","IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And nSemiMajorAxis >= Parent.RingsInnerm And nSemiMajorAxis <= Parent.RingsOuterm And Parent.IsPlanet IsTrue",true ),

                new Query("Heavier than Sol","nStellarMass > 1", true ),
                new Query("Bigger than Sol","nRadius > 695700000", true ),

                new Query("Landable","IsLandable IsTrue", true ),
                new Query("Landable and Terraformable","IsPlanet IsTrue And IsLandable IsTrue And Terraformable IsTrue",true ),
                new Query("Landable with Atmosphere","IsPlanet IsTrue And IsLandable IsTrue And HasAtmosphere IsTrue",true ),
                new Query("Landable with High G","IsPlanet IsTrue And IsLandable IsTrue And nSurfaceGravityG >= 3",true ),
                new Query("Landable large planet","IsPlanet IsTrue And IsLandable IsTrue And nRadius >= 12000000",true ),
                new Query("Landable with Rings","IsPlanet IsTrue And IsLandable IsTrue And HasRings IsTrue",true ),
                new Query("Has Volcanism","HasMeaningfulVolcanism IsTrue", true ),
                new Query("Landable with Volcanism","HasMeaningfulVolcanism IsTrue And IsLandable IsTrue", true ),
                new Query("Earth Like planet","Earthlike IsTrue", true ),
                new Query("Has Rings","HasRings IsTrue", true ),
                new Query("Has Belts","HasBelts IsTrue", true ),
                new Query("Bigger than Earth","IsPlanet IsTrue And nMassEM > 1", true ),
                new Query("Hotter than Hades","IsPlanet IsTrue And nSurfaceTemperature >= 350", true ),

                new Query("Planet has wide rings vs radius","(IsPlanet IsTrue And HasRings IsTrue ) And ( Rings[Iter1]_OuterRad-Rings[Iter1]_InnerRad >= nRadius*5)",true ),

                new Query("Close orbit to parent","IsPlanet IsTrue And Parent.IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And Parent.nRadius*3 > nSemiMajorAxis",true ),

                new Query("Close to ring",
                                "( IsPlanet IsTrue And Parent.IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse ) And " +
                                "( \"Abs(Parent.Rings[Iter1]_InnerRad-nSemiMajorAxis)\" < nRadius*10 Or  \"Abs(Parent.Rings[Iter1]_OuterRad-nSemiMajorAxis)\" < nRadius*10 )"
                    ,true ),

                new Query("Planets with a large number of Moons","IsPlanet IsTrue And Child.Count >= 8",true ),
                new Query("Moon of a Moon","Level == 3",true ),
                new Query("Moons orbiting Terraformables","Level >= 2 And Parent.Terraformable IsTrue",true ),
                new Query("Moons orbiting Earthlike","Level >= 2 And Parent.Earthlike IsTrue",true ),

                new Query("Close Binary","IsPlanet IsTrue And IsOrbitingBaryCentre IsTrue And Sibling.Count == 1 And nRadius/nSemiMajorAxis > 0.4 And " +
                    "Sibling[1].nRadius/Sibling[1].nSemiMajorAxis > 0.4",true ),

                new Query("Gas giant has a terraformable Moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].Terraformable IsTrue )",true ),
                new Query("Gas giant has a large moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius >= 5000000 )",true ),
                new Query("Gas giant has a tiny moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius <= 500000 )",true ),

                new Query("Tiny Moon","Level >= 2 And nRadius < 300000",true ),
                new Query("Fast Rotation of a non tidally locked body","Level >= 1 And nTidalLock IsFalse And Abs(nRotationPeriod) < 3600",true ),
                new Query("Fast Orbital period","Level >= 1 And nOrbitalPeriod < 28800",true ),
                new Query("High Eccentric Orbit","Level >= 1 And nEccentricity > 0.9",true ),
                new Query("Low Eccentricity Orbit","Level >= 1 And nEccentricity <= 0.01", true ),
                new Query("Tidal Lock","IsPlanet IsTrue And nTidalLock == 1",true ),

                new Query("High number of Jumponium Materials","IsLandable IsTrue And JumponiumCount >= 5",true ),

                new Query("Contains Geo Signals",            "ContainsGeoSignals IsTrue",true ),
                new Query("Contains Bio Signals",            "ContainsBioSignals IsTrue",true ),
                new Query("Contains Thargoid Signals",       "ContainsThargoidSignals IsTrue",true ),
                new Query("Contains Guardian Signals",       "ContainsGuardianSignals IsTrue",true ),
                new Query("Contains Human Signals",          "ContainsHumanSignals IsTrue",true ),
                new Query("Contains Other Signals",          "ContainsOtherSignals IsTrue",true ),
                new Query("Contains Uncategorised Signals",  "ContainsUncategorisedSignals IsTrue",true ),

                new Query("Body Name","BodyName contains <name>",false ),
                new Query("Scan Type","ScanType contains Detailed",false ),
                new Query("Distance (ls)","DistanceFromArrivalLS >= 20",false ),
                new Query("Rotation Period (s)","nRotationPeriod >= 30",false ),
                new Query("Rotation Period (days)","nRotationPeriodDays >= 1",false ),
                new Query("Radius (m)","nRadius >= 100000",false ),
                new Query("Radius (sols)","nRadiusSols >= 1",false ),
                new Query("Radius (Earth)","nRadiusEarths >= 1",false ),
                new Query("Semi Major Axis (m)","nSemiMajorAxis >= 20000000",false ),
                new Query("Semi Major Axis (AU)","nSemiMajorAxisAU >= 1",false ),
                new Query("Orbital Inclination (Deg)","nOrbitalInclination > 1",false ),
                new Query("Periapsis (Deg)","nPeriapsis > 1",false ),
                new Query("Orbital period (s)","nOrbitalPeriod > 200",false ),
                new Query("Orbital period (days)","nOrbitalPeriodDays > 200",false ),
                new Query("Axial Tilt (Deg)","nAxialTiltDeg > 1",false ),

                new Query("Star Type","StarType $== A",false ),
                new Query("Star Magnitude","nAbsoluteMagnitude >= 1",false ),
                new Query("Star Age (MY)","nAge >= 2000",false ),
                new Query("Star Luminosity","Luminosity $== V",false ),

                new Query("Planet Materials","MaterialList contains \"iron\"",false ),
                new Query("Planet Class","PlanetClass $== \"High metal content body\"",false ),
                new Query("Terraformable","Terraformable Isfalse",false ),
                new Query("Atmosphere","Atmosphere $== \"thin sulfur dioxide atmosphere\"",false ),
                new Query("Atmosphere ID","AtmosphereID $== \"Carbon_dioxide\"",false ),
                new Query("Atmosphere Property","AtmosphereProperty $== \"Rich\"",false ),
                new Query("Volcanism","Volcanism $== \"minor metallic magma volcanism\"",false ),
                new Query("Volcanism ID","VolcanismID $== \"Ammonia_Magma\"",false ),
                new Query("Surface Gravity m/s","nSurfaceGravity >= 9.6",false ),
                new Query("Surface Gravity G","nSurfaceGravityG >= 1.0",false ),
                new Query("Surface Pressure (Pa)","nSurfacePressure >= 101325",false ),
                new Query("Surface Pressure (Earth Atmos)","nSurfacePressureEarth >= 1",false ),
            };

        static private HistoryListQueries instance = null;
        private string DbUserQueries { get { return "UCSearchScansUserQuery"; } }  // not keyed to profile or to panel, global

        private char splitmarker = (char)0x2b1c; // horrible but i can't be bothered to do a better implementation at this point

        private HistoryListQueries()
        {
            string[] userqueries = DB.UserDatabase.Instance.GetSettingString(DbUserQueries, "").Split(new char[] { splitmarker }); // allowed use

            for (int i = 0; i + 1 < userqueries.Length; i += 2)
                Searches.Add(new Query(userqueries[i], userqueries[i + 1],false,true));
        }

        public void Save()
        {
            string userqueries = "";
            for (int i = 0; i < Searches.Count; i++)
            {
                if ( Searches[i].User)
                    userqueries += Searches[i].Name + splitmarker + Searches[i].Condition + splitmarker;
            }

            DB.UserDatabase.Instance.PutSettingString(DbUserQueries, userqueries); // allowed use
        }

        public void Update(string name, string expr)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
                Searches[entry] = new Query(name, expr, false, true);
            else
                Searches.Add(new Query(name, expr, false, true));
        }

        public void Delete(string name)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
                Searches.RemoveAt(entry);
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

                bool iter1 = allvars.Contains("Iter1");
                bool iter2 = allvars.Contains("Iter2");
                bool jumponium = allvars.Contains("JumponiumCount");
                bool wantsiblingcount = allvars.Contains("Sibling.Count");
                bool wantchildcount = allvars.Contains("Child.Count");
                bool wantlevel = allvars.Contains("Level");

                HashSet<string> varsevent = allvars.Where(x => !x.StartsWith("Parent.") && !x.StartsWith("Sibling")).Select(x => x.Substring(0, x.IndexOfOrLength("["))).ToHashSet();
                HashSet<string> varsparent = allvars.Where(x => x.StartsWith("Parent.")).Select(x => x.Substring(7, x.IndexOfOrLength("[") - 7)).ToHashSet();
                HashSet<string> varssiblings = allvars.Where(x => x.StartsWith("Sibling[")).Select(x => x.Substring(x.IndexOfOrLength("]", offset: 2))).Select(x => x.Substring(0, x.IndexOfOrLength("["))).ToHashSet();
                HashSet<string> varschildren = allvars.Where(x => x.StartsWith("Child[")).Select(x => x.Substring(x.IndexOfOrLength("]", offset: 2))).Select(x => x.Substring(0, x.IndexOfOrLength("["))).ToHashSet();

                foreach (var he in helist)
                {
                    BaseUtils.Variables scandatavars = defaultvars != null ? new BaseUtils.Variables(defaultvars) : new BaseUtils.Variables();

                    //if (he.EntryType != JournalTypeEnum.Scan) continue;

                    scandatavars.AddPropertiesFieldsOfClass(he.journalEntry, "",
                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                            varsevent);

                    if ( jumponium )
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

                    if (iter1)      // set up default iter1
                        scandatavars["Iter1"] = "1";
                    if (iter2)      // set up default iter2
                        scandatavars["Iter2"] = "1";

                    bool debugit = false;

                    //if (js.BodyName.Equals("Borann A 2 a"))  debugit = true;

                    bool? res = BaseUtils.ConditionLists.CheckConditionsEvalIterate(cond.List, scandatavars, out string evalerrlist, out BaseUtils.ConditionLists.ErrorClass errclassunused, iter1 || iter2 , debugit: debugit);

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

                return resultinfo.ToString();
            });
        }


    };


}
