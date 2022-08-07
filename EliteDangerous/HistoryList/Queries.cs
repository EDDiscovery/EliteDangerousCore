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

        public static HashSet<JournalTypeEnum> AllSearchableJournalTypes { get; } = new HashSet<JournalTypeEnum> 
            { JournalTypeEnum.Scan, JournalTypeEnum.FSSBodySignals, JournalTypeEnum.SAASignalsFound, JournalTypeEnum.FSSSignalDiscovered , JournalTypeEnum.CodexEntry ,JournalTypeEnum.ScanOrganic };

        public enum QueryType { BuiltIn, User, Example };

        public class Query
        {
            public string Name { get; }
            public string Condition { get; }
            public string SortCondition { get; }
            public bool SortAscending { get; }

            [JsonIgnore]
            public QueryType QueryType { get;  }
            [JsonIgnore]
            public bool DefaultSearch { get; set; }

            public Query(string n, string c, QueryType qt, bool def = false, string sortcond= null, bool sortascending = false) 
            { Name = n; Condition = c; QueryType = qt; DefaultSearch = def; SortCondition = sortcond; SortAscending = SortAscending; }

            [JsonIgnore]
            public bool User { get { return QueryType == QueryType.User; } }
            [JsonIgnore]
            public bool UserOrBuiltIn { get { return QueryType == QueryType.BuiltIn || QueryType == QueryType.User; } }
        }

        public List<Query> Searches = new List<Query>()
            {
                new Query("Planet inside inner ring","(IsOrbitingBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + //single body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsFalse And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsTrue And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
               
                new Query("Planet inside rings","(IsOrbitingBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" +
                            " Or (IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsFalse And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsTrue And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
               
                new Query("Planet between inner and outer ring","(IsOrbitingBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.RingsInnerm And nSemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" +
                            " Or (IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsFalse And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsTrue And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)", QueryType.BuiltIn, true ), // (((O-O)-O)-O) quartery
                
                new Query("Planet between rings 1 and 2","(IsOrbitingBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.Rings[1].OuterRad And nSemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" +
                            " Or (IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[1].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsFalse And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[2].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsTrue And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[3].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                
                new Query("Planet between rings 2 and 3","(IsOrbitingBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.Rings[2].OuterRad And nSemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" +
                            " Or (IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[1].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsFalse And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[2].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBarycentre IsTrue And Parents[3].IsBarycentre IsTrue And Parents[2].IsBarycentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[3].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                
                new Query("Landable","IsPlanet IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Landable and Terraformable","IsPlanet IsTrue And IsLandable IsTrue And Terraformable IsTrue", QueryType.BuiltIn , true ),
                new Query("Landable with Atmosphere","IsPlanet IsTrue And IsLandable IsTrue And HasAtmosphere IsTrue", QueryType.BuiltIn ),
                new Query("Landable with High G","IsPlanet IsTrue And IsLandable IsTrue And nSurfaceGravityG >= 3", QueryType.BuiltIn, true, "Compare(left.nSurfaceGravityG,right.nSurfaceGravityG)", false ),
                new Query("Landable large planet","IsPlanet IsTrue And IsLandable IsTrue And nRadius >= 12000000", QueryType.BuiltIn, false, "Compare(left.nRadius,right.nRadius)", false ),
                new Query("Landable with Rings","IsPlanet IsTrue And IsLandable IsTrue And HasRings IsTrue", QueryType.BuiltIn , true),
                new Query("Has Volcanism","HasMeaningfulVolcanism IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Volcanism","HasMeaningfulVolcanism IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Earth like planet","Earthlike IsTrue", QueryType.BuiltIn ),
                new Query("More mass than Earth","IsPlanet IsTrue And nMassEM > 1", QueryType.BuiltIn ),
                new Query("Hotter than Hades","IsPlanet IsTrue And nSurfaceTemperature >= 2273", QueryType.BuiltIn , true),

                new Query("Has Rings","HasRings IsTrue", QueryType.BuiltIn ),

                new Query("Planet has wide rings vs radius","(IsPlanet IsTrue And HasRings IsTrue ) And ( Rings[Iter1].OuterRad-Rings[Iter1].InnerRad >= nRadius*5)", QueryType.BuiltIn , true),

                new Query("Close orbit to parent","IsPlanet IsTrue And Parent.IsPlanet IsTrue And IsOrbitingBarycentre IsFalse And Parent.nRadius*3 > nSemiMajorAxis", QueryType.BuiltIn, true ),

                new Query("Close to ring",
                                "( IsPlanet IsTrue And Parent.IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBarycentre IsFalse ) And " +
                                "( \"Abs(Parent.Rings[Iter1].InnerRad-nSemiMajorAxis)\" < nRadius*10 Or  \"Abs(Parent.Rings[Iter1].OuterRad-nSemiMajorAxis)\" < nRadius*10 )",
                    QueryType.BuiltIn, true ),

                new Query("Binary close to rings","(IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And Parent.HasRings IsTrue And IsPlanet IsTrue) And " + 
                            "(\"Abs(Parent.Rings[Iter1].InnerRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\" Or \"Abs(Parent.Rings[Iter1].OuterRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\")", 
                    QueryType.BuiltIn, true ),

                new Query("Planet with a large number of Moons","IsPlanet IsTrue And Child.Count >= 8", QueryType.BuiltIn, true ),
                new Query("Moon of a Moon","Parent.IsPlanet IsTrue And Parent.Parent.IsPlanet IsTrue", QueryType.BuiltIn ),
                new Query("Moons orbiting Terraformables","Parent.Terraformable IsTrue", QueryType.BuiltIn, true ),
                new Query("Moons orbiting Earthlike","Parent.Earthlike IsTrue", QueryType.BuiltIn ),

                new Query("Close Binary","IsPlanet IsTrue And IsOrbitingBarycentre IsTrue And Sibling.Count == 1 And nRadius/nSemiMajorAxis > 0.4 And " +
                    "Sibling[1].nRadius/Sibling[1].nSemiMajorAxis > 0.4", QueryType.BuiltIn, true ),

                new Query("Gas giant has a terraformable Moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].Terraformable IsTrue )", QueryType.BuiltIn, true ),
                new Query("Gas giant has a large moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius >= 5000000 )", QueryType.BuiltIn ),
                new Query("Gas giant has a tiny moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius <= 500000 )", QueryType.BuiltIn ),

                new Query("Tiny Moon","Parent.IsPlanet IsTrue And nRadius < 300000", QueryType.BuiltIn, true ),
                new Query("Fast Rotation of a non tidally locked body","IsPlanet IsTrue And nTidalLock IsFalse And Abs(nRotationPeriod) < 3600", QueryType.BuiltIn , true ),
                new Query("Planet with fast orbital period","IsPlanet IsTrue And nOrbitalPeriod < 28800", QueryType.BuiltIn ),
                new Query("Planet with high Eccentric Orbit","IsPlanet IsTrue And nEccentricity > 0.9", QueryType.BuiltIn, true ),
                new Query("Planet with low Eccentricity Orbit","IsPlanet IsTrue  And nEccentricity <= 0.01", QueryType.BuiltIn ),
                new Query("Tidal Lock","IsPlanet IsTrue And nTidalLock IsTrue", QueryType.BuiltIn ),

                new Query("High number of Jumponium Materials","IsLandable IsTrue And JumponiumCount >= 5", QueryType.BuiltIn, true ),

                new Query("Contains Geo Signals",            "ContainsGeoSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Bio Signals",            "ContainsBioSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Thargoid Signals",       "ContainsThargoidSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Guardian Signals",       "ContainsGuardianSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Human Signals",          "ContainsHumanSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Other Signals",          "ContainsOtherSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains Uncategorised Signals",  "ContainsUncategorisedSignals IsTrue", QueryType.BuiltIn ),
                new Query("Contains an Installation",  "CountInstallationSignals >= 1", QueryType.BuiltIn ),
                new Query("Contains a Carrier",  "CountCarrierSignals >= 1", QueryType.BuiltIn ),
                new Query("Contains a NSP",  "CountNotableStellarPhenomenaSignals >= 1", QueryType.BuiltIn ),

                new Query("Scanned all organics on a planet","CountOrganicsScansAnalysed >= 1 And CountOrganicsScansAnalysed == CountBioSignals", QueryType.BuiltIn ),

                new Query("Star has Rings","HasRings IsTrue And IsStar IsTrue", QueryType.BuiltIn ),
                new Query("Star is brighter magnitude than Sirius","nAbsoluteMagnitude <= 1.5", QueryType.BuiltIn ),
                new Query("Star is super bright","nAbsoluteMagnitude <= -2", QueryType.BuiltIn ),
                new Query("Star has belts","HasBelts IsTrue", QueryType.BuiltIn ),
                new Query("Star has same magnitude as Sol","nAbsoluteMagnitudeSol >= -0.5 And nAbsoluteMagnitudeSol <= 0.5", QueryType.BuiltIn ),
                new Query("Star is heavier than Sol","nStellarMass > 1", QueryType.BuiltIn ),
                new Query("Star is wider than Sol","nRadius > 695700000", QueryType.BuiltIn ),

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

        private HistoryListQueries()
        {
            JArray json = JArray.Parse(DB.UserDatabase.Instance.GetSettingString(DbUserQueries, ""));
            if ( json != null )
            {
                foreach( var t in json )
                {
                    Searches.Insert(0, new Query(t["Name"].Str("Unknown"), t["Condition"].Str("Unknown"), QueryType.User, 
                        sortcond:t["SortCondition"].Str(), sortascending:t["SortAscending"].Bool()));
                }
            }
        }

        public string DefaultSearches(char splitmarkerin)
        {
            string[] names = Searches.Where(x => x.DefaultSearch).Select(x => x.Name).ToArray();
            return names.Join(splitmarkerin);
        }

        public void Set(string name, string expr, QueryType t, string sortcond = "", bool sortascending= false)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
                Searches[entry] = new Query(name, expr, t, false, sortcond, sortascending);
            else
                Searches.Insert(0, new Query(name, expr, t, false, sortcond, sortascending));
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
            var list = Searches.Where(x => x.QueryType == QueryType.User).ToArray();
            JToken json = JToken.FromObject(list);
            DB.UserDatabase.Instance.PutSettingString(DbUserQueries, json.ToString()); // allowed use
            //System.IO.File.WriteAllText(@"c:\code\json.txt", json.ToString(true));
        }

        public JArray QueriesInJSON(QueryType t)
        {
            var list = Searches.Where(x => x.QueryType == t).ToArray();
            return JToken.FromObject(list).Array();
        }
        public bool ReadJSONQueries(JArray ja, QueryType ty)
        {
            foreach (var t in ja)
            {
                JObject to = t.Object();
                if (to != null)
                {
                    string name = to["Name"].StrNull();
                    string condition = to["Condition"].StrNull();
                    if (name != null && condition != null)
                    {
                        string sortcondition = to["SortCondition"].Str();
                        bool sortascending = to["SortAscending"].Bool();
                        Set(name, condition, ty,sortcondition,sortascending);
                    }
                    else
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        // Get the list of properties
        static public List<BaseUtils.TypeHelpers.PropertyNameInfo> PropertyList()
        {
            List<BaseUtils.TypeHelpers.PropertyNameInfo> classnames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalScan),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "Scan");

            List<BaseUtils.TypeHelpers.PropertyNameInfo> othernames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalFSSSignalDiscovered),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "FSSSignalDiscovered");
            List<BaseUtils.TypeHelpers.PropertyNameInfo> saanames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalSAASignalsFound),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "SAASignalsFound");
            othernames.AddRange(saanames);
            List<BaseUtils.TypeHelpers.PropertyNameInfo> fssbodynames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalFSSBodySignals),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "FSSBodySignals");
            othernames.AddRange(fssbodynames);        // merge blind 
            List<BaseUtils.TypeHelpers.PropertyNameInfo> codexnames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalCodexEntry),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "CodexEntry");
            othernames.AddRange(codexnames);        // merge blind
            List<BaseUtils.TypeHelpers.PropertyNameInfo> scanorganicnames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalScanOrganic),
                    bf: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "ScanOrganic");
            othernames.AddRange(scanorganicnames);        // merge blind

            foreach (var v in othernames)
            {
                var merged = classnames.Find(x => x.Name == v.Name);        // if we have the same, just merge the comments, so we don't get lots of repeats.
                if (merged != null)
                {
                    merged.Comment += ", " + v.Comment;
                }
                else
                    classnames.Add(v);
            }

            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventTimeUTC", "Date time value, US format: UTC", BaseUtils.ConditionEntry.MatchType.DateAfter, "All"));     // add on a few from the base class..
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventTimeLocal", "Date time value, US format: Local", BaseUtils.ConditionEntry.MatchType.DateAfter, "All"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventType", "String value: Event type (Scan etc)", BaseUtils.ConditionEntry.MatchType.Equals, "All"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("SyncedEDSM", "Boolean value: 1 = true, 0 = false: Synced to EDSM", BaseUtils.ConditionEntry.MatchType.IsTrue, "All"));

            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Level", "Integer value: Level of body in system", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Sibling.Count", "Integer value: Number of siblings", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Child.Count", "Integer value: Number of child moons", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("JumponiumCount", "Integer value: Number of jumponium materials available", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));     // add on ones we synthesise

            var defaultvars = new BaseUtils.Variables();
            defaultvars.AddPropertiesFieldsOfClass(new BodyPhysicalConstants(), "", null, 10);
            foreach (var v in defaultvars.NameEnumuerable)
                classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo(v, "Floating point value: Constant", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Constant"));

            classnames.Sort(delegate (BaseUtils.TypeHelpers.PropertyNameInfo left, BaseUtils.TypeHelpers.PropertyNameInfo right) { return left.Name.CompareTo(right.Name); });

            return classnames;
        }

        // Calculate from variables what is being searched for..
        static public HashSet<JournalTypeEnum> NeededSearchableTypes(HashSet<string> allvars)
        {
            var propertynames = PropertyList();

            var res = new HashSet<JournalTypeEnum>();

            string[] stoptext = new string[] { "[", "." };

            foreach (var v in allvars)
            {
                string v1 = v.Substring(0, v.IndexOfOrLength(stoptext));        // cut off the [ and class stuff to get to the root

                for (int i = 0; i < propertynames.Count; i++)        // do all propertynames
                {
                    if (propertynames[i].Name.StartsWith(v1))        // and it starts with
                    {
                        var comment = propertynames[i].Comment;
                        bool all = comment.Contains("All");
                        if (InComment(comment,"Scan",all))
                            res.Add(JournalTypeEnum.Scan);
                        if (InComment(comment, "FSSSignalDiscovered",all))
                            res.Add(JournalTypeEnum.FSSSignalDiscovered);
                        if (InComment(comment,"SAASignalsFound",all))
                            res.Add(JournalTypeEnum.SAASignalsFound);
                        if (InComment(comment,"FSSBodySignals",all))
                            res.Add(JournalTypeEnum.FSSBodySignals);
                        if (InComment(comment,"CodexEntry",all))
                            res.Add(JournalTypeEnum.CodexEntry);
                        if (InComment(comment,"ScanOrganic",all))
                            res.Add(JournalTypeEnum.ScanOrganic);
                    }
                }

                if (v.StartsWith("Parent.") || v.StartsWith("Sibling") || v.StartsWith("Child") || v.StartsWith("Star.") )      // specials, for scan
                    res.Add(JournalTypeEnum.Scan);
            }

            foreach (var v in res)
                System.Diagnostics.Debug.WriteLine($"Search types {v.ToString()}");

            return res;
        }

        static private bool InComment(string comment, string tag, bool all)
        {
            return comment.Contains(tag + ",") || comment.EndsWith(tag) || all;
        }


        //find a named search, async
        public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist, Dictionary<string, Results> results, string searchname, BaseUtils.Variables defaultvars,
                            StarScan starscan, bool wantdebug)
        {
            var search = Searches.Find(x => x.Name.Equals(searchname));

            var cond = search != null ? new BaseUtils.ConditionLists(search.Condition) : null;

            if (cond == null)
                System.Diagnostics.Trace.WriteLine($"Search missing {searchname}");

            return Find(helist, results, searchname, cond, defaultvars, starscan, wantdebug);
        }

        public class Results
        {
            public List<string> FiltersPassed { get; set; }
            public List<HistoryEntry> EntryList { get; set; }
        }


        // find using cond, async. return string of result info.  Fill in results dictionary (already made)
        // default vars can be null
        static public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist,
                                   Dictionary<string, Results> results, string filterdescription,
                                   BaseUtils.ConditionLists cond, BaseUtils.Variables defaultvars, StarScan starscan, bool wantreport)
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

                string[] stoptext = new string[] { "[", "." };

                HashSet<string> varsparent = new HashSet<string>();
                HashSet<string> varsparentparent = new HashSet<string>();
                HashSet<string> varssiblings = new HashSet<string>();
                HashSet<string> varschildren = new HashSet<string>();
                HashSet<string> varsevent = new HashSet<string>();
                HashSet<string> varsstar = new HashSet<string>();
                HashSet<string> varsstarstar = new HashSet<string>();

                foreach (var v in allvars)
                {
                    if (v.StartsWith("Star.Star."))
                    {
                        varsstarstar.Add(v.Substring(10, v.IndexOfOrLength(stoptext, startindex: 10) - 10));
                    }
                    else if (v.StartsWith("Star."))
                    {
                        varsstar.Add(v.Substring(5, v.IndexOfOrLength(stoptext, startindex: 5) - 5));
                    }
                    else if (v.StartsWith("Parent.Parent."))
                    {
                        varsparentparent.Add(v.Substring(14, v.IndexOfOrLength(stoptext, startindex: 14) - 14));
                    }
                    else if (v.StartsWith("Parent."))
                    {
                        varsparent.Add(v.Substring(7, v.IndexOfOrLength(stoptext, startindex:7) - 7));
                    }
                    else if (v.StartsWith("Sibling["))
                    {
                        var v1 = v.Substring(v.IndexOfOrLength("]", offset: 2));        // remove up to the []
                        varssiblings.Add(v1.Substring(0, v1.IndexOfOrLength(stoptext)));    // then add, remove after stop text
                    }
                    else if (v.StartsWith("Child["))
                    {
                        var v1 = v.Substring(v.IndexOfOrLength("]", offset: 2));
                        varschildren.Add(v1.Substring(0, v1.IndexOfOrLength(stoptext)));
                    }
                    else
                    {
                        varsevent.Add(v.Substring(0, v.IndexOfOrLength(stoptext)));
                    }
                }

                //foreach (var v in varsevent) System.Diagnostics.Debug.WriteLine($"Search Event Var {v}");
                //foreach (var v in varsstar) System.Diagnostics.Debug.WriteLine($"Search Star Var {v}");
                //foreach (var v in varsstarstar) System.Diagnostics.Debug.WriteLine($"Search Star Star Var {v}");
                //foreach (var v in varsparent) System.Diagnostics.Debug.WriteLine($"Search Parent Var {v}");
                //foreach (var v in varsparentparent) System.Diagnostics.Debug.WriteLine($"Search Parent Parent Var {v}");
                //foreach (var v in varssiblings) System.Diagnostics.Debug.WriteLine($"Search Sibling Var {v}");
                //foreach (var v in varschildren) System.Diagnostics.Debug.WriteLine($"Search Child Var {v}");

                Type[] ignoretypes = new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) };


                //foreach (var he in helist.GetRange(helist.Count-100,100))
                foreach (var he in helist)
                {
                    BaseUtils.Variables scandatavars = defaultvars != null ? new BaseUtils.Variables(defaultvars) : new BaseUtils.Variables();

                    bool debugit = false;

                    //if ( he.System.Name == "Lu Dongia") debugit = true;

                    scandatavars["EventType"] = he.EntryType.ToString();

                    scandatavars.AddPropertiesFieldsOfClass(he.journalEntry, "",ignoretypes, 5, varsevent, ensuredoublerep: true, classsepar:".");

                    if (wantjumponium)
                    {
                        JournalScan js = he.journalEntry as JournalScan;

                        if (js != null)   // if its a journal scan
                        {
                            scandatavars["JumponiumCount"] = js.Jumponium().ToStringInvariant();
                        }
                    }

                    if (he.ScanNode != null)      // if it has a scan node
                    {
                        if (wantlevel)
                            scandatavars["Level"] = he.ScanNode.Level.ToStringInvariant();

                        if (he.ScanNode.Parent != null) // if we have a parent..
                        {
                            if (varsparent.Count > 0)
                            {
                                var parentjs = he.ScanNode.Parent.ScanData;               // parent journal entry, may be null

                                if (parentjs != null) // if want parent scan data
                                {
                                    scandatavars.AddPropertiesFieldsOfClass(parentjs, "Parent.", ignoretypes, 5,varsparent, ensuredoublerep: true, classsepar: ".");
                                    scandatavars["Parent.Level"] = he.ScanNode.Parent.Level.ToStringInvariant();
                                }
                            }

                            if (varsparentparent.Count > 0 && he.ScanNode.Parent.Parent != null)        // if want parent.parent and we have one
                            {
                                var parentparentjs = he.ScanNode.Parent.Parent.ScanData;               // parent journal entry, may be null

                                if (parentparentjs != null) // if want parent scan data
                                {
                                    scandatavars.AddPropertiesFieldsOfClass(parentparentjs, "Parent.Parent.", ignoretypes, 5, varsparentparent, ensuredoublerep: true, classsepar: ".");
                                    scandatavars["Parent.Parent.Level"] = he.ScanNode.Parent.Level.ToStringInvariant();
                                }
                            }

                            if (varsstar.Count > 0)
                            {
                                var scandata = FindStarOf(he.ScanNode, 0);

                                if (scandata != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine($"{scandata.BodyName} is the Star parent");

                                    scandatavars.AddPropertiesFieldsOfClass(scandata, "Star.",
                                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                            varsstar, ensuredoublerep: true, classsepar: ".");
                                }
                            }


                            if (varsstarstar.Count > 0)
                            {
                                var scandata = FindStarOf(he.ScanNode, 1);

                                if (scandata != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine($"{scandata.BodyName} is the Star Star parent");

                                    scandatavars.AddPropertiesFieldsOfClass(scandata, "Star.Star.",
                                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                            varsstarstar, ensuredoublerep: true, classsepar: ".");
                                }
                            }


                            if (wantsiblingcount)
                                scandatavars["Sibling.Count"] = ((he.ScanNode.Parent.Children.Count - 1)).ToStringInvariant();      // count of children or parent less ours

                            if (varssiblings.Count > 0)        // if want sibling[
                            {
                                int cno = 1;
                                foreach (var sn in he.ScanNode.Parent.Children)
                                {
                                    if (sn.Value != he.ScanNode && sn.Value.ScanData != null)        // if not ours and has a scan
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(sn.Value.ScanData, $"Sibling[{cno}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varssiblings, ensuredoublerep: true, classsepar: ".");
                                        cno++;
                                    }
                                }
                            }
                        }

                        if (wantchildcount)
                            scandatavars["Child.Count"] = ((he.ScanNode.Children?.Count ?? 0)).ToStringInvariant();      // count of children

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
                                            varschildren, ensuredoublerep: true, classsepar: ".");

                                    cno++;
                                }
                            }
                        }
                    }

                    if (wantiter1)      // set up default iter1
                        scandatavars["Iter1"] = "1";
                    if (wantiter2)      // set up default iter2
                        scandatavars["Iter2"] = "1";

                    List<BaseUtils.ConditionEntry> testspassed = wantreport ? new List<BaseUtils.ConditionEntry>() : null;

                    //JournalScan jsc = he.journalEntry as JournalScan; if (jsc?.BodyName == "Blue Euq WA-E d12-32 7 a") debugit = true;

                    //foreach (var v in scandatavars.NameEnumuerable) System.Diagnostics.Debug.WriteLine($"Search scandata var {v} = {scandatavars[v]}");

                    var res = BaseUtils.ConditionLists.CheckConditionsEvalIterate(cond.List, scandatavars, wantiter1 || wantiter2, debugit: debugit);
                    
                    //var resold = BaseUtils.ConditionLists.CheckConditionsEvalIterate(cond.List, scandatavars, out string errlist, out BaseUtils.ConditionLists.ErrorClass errcls, wantiter1 || wantiter2, debugit: debugit);

                    //if ( res.Item1 != resold.Item1)
                    //{
                    //}

                    //if (res.Item1 == false && res.Item2.Last().ItemName.Contains("Parent.Rings[Iter1].OuterRad")) debugit = true;

                    if (wantreport)
                    {
                        JournalScan jsi = he.journalEntry as JournalScan;
                        resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} {jsi?.BodyName} : {res.Item1} : Last {res.Item2.Last().ItemName} {res.Item2.Last().MatchCondition} {res.Item2.Last().MatchString} : {res.Item3.Last()??""}");
                        //foreach ( var x in res.Item2)    resultinfo.AppendLine($"  {x.ItemName} {x.MatchCondition} {x.MatchString}");

                        // System.Diagnostics.Debug.WriteLine($"For entry type {he.EventTimeUTC} {he.EntryType} error: {resultinfo}");
                    }

                    if (res.Item1.Value == true)    // if passed
                    {
                        //if we have a je with a body name, use that to set the ret, else just use a incrementing decimal count name

                        string key = null;
                        if (he.EntryType == JournalTypeEnum.FSSSignalDiscovered)
                        {
                            long? sysaddr = ((JournalFSSSignalDiscovered)he.journalEntry).Signals[0].SystemAddress;
                            if (sysaddr.HasValue && starscan.ScanDataBySysaddr.TryGetValue(sysaddr.Value, out StarScan.SystemNode sn))
                            {
                                key = sn.System.Name;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Query FSD No system found for {he.EventTimeUTC} {sysaddr}");
                            }
                        }
                        else if (he.journalEntry is IBodyNameIDOnly)
                        {
                            key = (he.journalEntry as IBodyNameIDOnly).BodyName;
                        }
                        else if (he.EntryType == JournalTypeEnum.CodexEntry)
                        {
                            var ce = he.journalEntry as JournalCodexEntry;
                            key = ce.EDDBodyName ?? ce.System;
                        }
                        else if (he.EntryType == JournalTypeEnum.ScanOrganic)
                        {
                            var so = he.journalEntry as JournalScanOrganic;
                            key = he.System.Name;       // default
                            if (starscan.ScanDataBySysaddr.TryGetValue(so.SystemAddress, out StarScan.SystemNode sn))
                            {
                                if (sn.NodesByID.TryGetValue(so.Body, out StarScan.ScanNode ssn))
                                    key = ssn.FullName;
                            }
                        }
                        else
                            System.Diagnostics.Debug.Assert(false);

                        if (key != null)
                        {
                            if (wantreport)
                                resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} : Matched {key}");

                            lock (results)     // may be spawing a lot of parallel awaits, make sure the shared resource is locked
                            {
                                if (results.TryGetValue(key, out Results value))       // if key already exists, maybe set HE to us, and update filters passed
                                {
                                    value.EntryList.Add(he);
                                    if (!value.FiltersPassed.Contains(filterdescription))      // we may scan and find the same body twice with the same filter, do not dup add
                                        value.FiltersPassed.Add(filterdescription);
                                }
                                else
                                {                                                       // else make a new key
                                    results[key] = new Results() { EntryList = new List<HistoryEntry>() { he }, FiltersPassed = new List<string>() { filterdescription } };
                                }
                            }
                        }
                    }
                }

                return resultinfo.ToString();
            });
        }

        private static JournalScan FindStarOf(StarScan.ScanNode node, int stardepth)
        {
            var plist = node?.ScanData?.Parents;
            if (plist != null)
            {
                for (int i = 0; i < plist.Count; i++)
                {
                    if (plist[i].IsStar && stardepth-- == 0)    // use bodyid to find it in parents list to get a definitive parent id, accounting for star depth
                    {
                        var pnode = node.Parent;    // now lets see if we can find it
                        while (pnode != null && pnode.BodyID != plist[i].BodyID)        // look up the star node list and see if we have a body id to match
                        {
                            pnode = pnode.Parent;
                        }

                        return pnode?.ScanData;
                    }
                }
            }

            return null;
        }


        public static void GenerateReportFields(string bodykey, List<HistoryEntry> hes, out string name, out string info, out string infotooltip, 
                                                bool pinfowanted, out string pinfo, 
                                                bool ppinfowanted, out string ppinfo, 
                                                bool sinfowanted, out string sinfo, 
                                                bool ssinfowanted, out string ssinfo)
        {
            name = bodykey;
            
            info = pinfo = infotooltip = ppinfo = sinfo = ssinfo = "";

            HistoryEntry hescan = hes.Find(x => x.EntryType == JournalTypeEnum.Scan); // if we have a scan in the results list, do that first

            if ( hescan != null)
            {
                JournalScan js = hescan.journalEntry as JournalScan;
                info = js.DisplayString();

                if (pinfowanted && hescan.ScanNode?.Parent != null)
                {
                    var parentjs = hescan.ScanNode?.Parent?.ScanData;               // parent journal entry, may be null
                    pinfo = parentjs != null ? parentjs.DisplayString() : hescan.ScanNode.Parent.CustomNameOrOwnname + " " + hescan.ScanNode.Parent.NodeType;
                }

                if (ppinfowanted && hescan.ScanNode?.Parent?.Parent != null)        // if want parent.parent and we have one
                {
                    var parentparentjs = hescan.ScanNode.Parent.Parent.ScanData;               // parent journal entry, may be null

                    ppinfo = parentparentjs != null ? parentparentjs.DisplayString() : hescan.ScanNode.Parent.Parent.CustomNameOrOwnname + " " + hescan.ScanNode.Parent.Parent.NodeType;
                }

                if (sinfowanted)
                {
                    var scandata = FindStarOf(hescan.ScanNode, 0);

                    if (scandata != null)
                    {
                        sinfo = scandata.DisplayString();
                    }
                }

                if (ssinfowanted)
                {
                    var scandata = FindStarOf(hescan.ScanNode, 1);

                    if (scandata != null)
                    {
                        ssinfo = scandata.DisplayString();
                    }
                }
            }

            foreach (var he in hes)
            {
                if (he.EntryType != JournalTypeEnum.Scan)      // for all the rest of the results, ignoring scan
                {
                    string time = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(he.EventTimeUTC).ToString();
                    he.journalEntry.FillInformation(he.System, "", out string info2, out string detailed);
                    if (info.HasChars())
                        info = info.AppendPrePad(time + ": " + info2, Environment.NewLine);
                    else
                        info = info.AppendPrePad(info2, Environment.NewLine);

                    if (detailed.HasChars())
                    {
                        infotooltip += time + Environment.NewLine + detailed.LineIndentation("    ") + Environment.NewLine;
                    }
                }
            }

        }
    }

}
