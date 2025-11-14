/*
 * Copyright 2022-2025 EDDiscovery development team
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
using EliteDangerousCore.StarScan2;
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
            public QueryType QueryType { get; }
            [JsonIgnore]
            public bool DefaultSearch { get; set; }

            public Query(string n, string c, QueryType qt, bool def = false, string sortcond = null, bool sortascending = false)
            { Name = n; Condition = c; QueryType = qt; DefaultSearch = def; SortCondition = sortcond; SortAscending = sortascending; }

            [JsonIgnore]
            public bool User { get { return QueryType == QueryType.User; } }
            [JsonIgnore]
            public bool UserOrBuiltIn { get { return QueryType == QueryType.BuiltIn || QueryType == QueryType.User; } }
        }

        public List<Query> Searches = new List<Query>()
            {
                // planet pos
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
                
                // planet properties

                new Query("Landable","IsPlanet IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Landable and Terraformable","IsPlanet IsTrue And IsLandable IsTrue And Terraformable IsTrue", QueryType.BuiltIn , true ),
                new Query("Landable with Atmosphere","IsPlanet IsTrue And IsLandable IsTrue And HasAtmosphere IsTrue", QueryType.BuiltIn ),
                new Query("Landable with High G","IsPlanet IsTrue And IsLandable IsTrue And nSurfaceGravityG >= 3", QueryType.BuiltIn, true, "Compare(left.nSurfaceGravityG,right.nSurfaceGravityG)", false),
                new Query("Landable large planet","IsPlanet IsTrue And IsLandable IsTrue And nRadiusKM >= 8000", QueryType.BuiltIn, false, "Compare(left.nRadius,right.nRadius)", false ),
                new Query("Landable with Rings","IsPlanet IsTrue And IsLandable IsTrue And HasRings IsTrue", QueryType.BuiltIn , true),
                new Query("Has Volcanism","HasMeaningfulVolcanism IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Volcanism","HasMeaningfulVolcanism IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Earth like planet","Earthlike IsTrue", QueryType.BuiltIn ),
                new Query("More mass than Earth","IsPlanet IsTrue And nMassEM > 1", QueryType.BuiltIn, false, "Compare(left.nMassKG,right.nMassKG)", false),
                new Query("Hotter than Hades","IsPlanet IsTrue And nSurfaceTemperature >= 2273", QueryType.BuiltIn , true, "Compare(left.nSurfaceTemperature,right.nSurfaceTemperature)", false),

                new Query("Has Rings","HasRings IsTrue", QueryType.BuiltIn ),

                new Query("Planet has wide rings vs radius","(IsPlanet IsTrue And HasRings IsTrue ) And ( Rings[Iter1].OuterRad-Rings[Iter1].InnerRad >= nRadius*5)", QueryType.BuiltIn , true, "Compare(left.RingsMaxWidth,right.RingsMaxWidth)", false),

                new Query("Close orbit to parent","IsPlanet IsTrue And Parent.IsPlanet IsTrue And IsOrbitingBarycentre IsFalse And Parent.nRadius*3 > nSemiMajorAxis", QueryType.BuiltIn, true ),

                new Query("Close to ring",
                                "( IsPlanet IsTrue And Parent.IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBarycentre IsFalse ) And " +
                                "( \"Abs(Parent.Rings[Iter1].InnerRad-nSemiMajorAxis)\" < nRadius*10 Or  \"Abs(Parent.Rings[Iter1].OuterRad-nSemiMajorAxis)\" < nRadius*10 )",
                    QueryType.BuiltIn, true ),

                new Query("Binary close to rings","(IsOrbitingBarycentre IsTrue And Parents[2].IsBarycentre IsFalse And Parent.HasRings IsTrue And IsPlanet IsTrue) And " +
                            "(\"Abs(Parent.Rings[Iter1].InnerRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\" Or \"Abs(Parent.Rings[Iter1].OuterRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\")",
                    QueryType.BuiltIn, true ),

                new Query("Planet with a large number of Moons","IsPlanet IsTrue And Child.Count >= 8", QueryType.BuiltIn, true, "Compare(left.Child.Count,right.Child.Count)" ),
                new Query("Moon of a Moon","Parent.IsPlanet IsTrue And Parent.Parent.IsPlanet IsTrue", QueryType.BuiltIn ),
                new Query("Moons orbiting Terraformables","Parent.Terraformable IsTrue And IsPlanet IsTrue", QueryType.BuiltIn, true ),
                new Query("Moons orbiting Earthlike","Parent.Earthlike IsTrue", QueryType.BuiltIn ),

                new Query("Close Binary","IsPlanet IsTrue And IsOrbitingBarycentre IsTrue And Sibling.Count == 1 And nRadius/nSemiMajorAxis > 0.4 And " +
                    "Sibling[1].nRadius/Sibling[1].nSemiMajorAxis > 0.4", QueryType.BuiltIn, true ),

                new Query("Gas giant has a terraformable Moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].Terraformable IsTrue )", QueryType.BuiltIn, true ),
                new Query("Gas giant has a large moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius >= 5000000 )", QueryType.BuiltIn ),
                new Query("Gas giant has a tiny moon","( SudarskyGasGiant IsTrue Or GasGiant IsTrue Or HeliumGasGiant IsTrue ) And ( Child[Iter1].nRadius <= 500000 )", QueryType.BuiltIn ),

                new Query("Tiny Moon","Parent.IsPlanet IsTrue And nRadius < 300000", QueryType.BuiltIn, true ),
                new Query("Fast Rotation of a non tidally locked body","IsPlanet IsTrue And nTidalLock IsFalse And Abs(nRotationPeriod) < 3600", QueryType.BuiltIn , true ),
                new Query("Planet with fast orbital period","IsPlanet IsTrue And nOrbitalPeriod < 28800", QueryType.BuiltIn, false, "Compare(left.nOrbitalPeriod,right.nOrbitalPeriod)", true),
                new Query("Planet with high Eccentric Orbit","IsPlanet IsTrue And nEccentricity > 0.9", QueryType.BuiltIn, true, "Compare(left.nEccentricity,right.nEccentricity)", false ),
                new Query("Planet with low Eccentricity Orbit","IsPlanet IsTrue  And nEccentricity <= 0.01", QueryType.BuiltIn , false, "Compare(left.nEccentricity,right.nEccentricity)", true ),
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

                new Query("Ring contains Painite",  "Signals[Iter1].Type Contains Painite", QueryType.BuiltIn ),

                // exo biology

                new Query("Scanned all organics on a planet","CountOrganicsScansAnalysed >= 1 And CountOrganicsScansAnalysed == CountBioSignals", QueryType.BuiltIn ),
                new Query("Exobiology possible Amphora body","PlanetTypeID $== Metal_rich_body And IsLandable IsTrue And ContainsBioSignals IsTrue And Bodies[Iter1].StarTypeID $== A And Bodies[Iter2].PlanetTypeID MatchCommaList Gas_giant_with_water_based_life,Earthlike_body,Gas_giant_with_ammonia_based_life", QueryType.BuiltIn ),

                // multiple bodies in systems
                new Query("Multiple Earth sized bodies in system","(nMassEM >= 0.8 And nMassEM <= 1.2 And IsPlanet IsTrue) And (\"BodiesExprCount(nMassEM>=0.8 && nMassEM<=1.2 && IsPlanet)\" >= 2)", QueryType.BuiltIn ),
                new Query("Multiple Earth like bodies in system","(Earthlike IsTrue) And (\"BodiesPropertyCount(\\\"Earthlike\\\")\" >= 2)", QueryType.BuiltIn ),
                new Query("Multiple Earth like bodies under same star","(Earthlike IsTrue) And (\"StarBodiesPropertyCount(\\\"Earthlike\\\")\" >= 2)", QueryType.BuiltIn ),
                new Query("Multiple Gas Giants in system","(GasWorld IsTrue) And (\"BodiesPropertyCount(\\\"GasWorld\\\")\" >= 4)", QueryType.BuiltIn ),
                new Query("Multiple Large Gas Giants in system","(GasWorld IsTrue And nMassEM >= 2000) And (\"BodiesExprCount(GasWorld && nMassEM>=2000)\" >= 2)", QueryType.BuiltIn ),

                // stars

                new Query("Star has Rings","HasRings IsTrue And IsStar IsTrue", QueryType.BuiltIn ),
                new Query("Star is Main Sequence","IsStar IsTrue And StarType MatchSemicolon O;B;A;F;G;K;M", QueryType.BuiltIn ),
                new Query("Star is Non Main Sequence","IsStar IsTrue And StarType NotMatchSemicolon O;B;A;F;G;K;M", QueryType.BuiltIn ),
                new Query("Star is brighter in magnitude than Sirius","nAbsoluteMagnitude <= 1.5", QueryType.BuiltIn, false, "Compare(left.nAbsoluteMagnitude,right.nAbsoluteMagnitude)", true  ),
                new Query("Star is super bright","nAbsoluteMagnitude <= -2", QueryType.BuiltIn , false, "Compare(left.nAbsoluteMagnitude,right.nAbsoluteMagnitude)", true  ),
                new Query("Star has same magnitude as Sol","nAbsoluteMagnitudeSol >= -0.5 And nAbsoluteMagnitudeSol <= 0.5", QueryType.BuiltIn, false, "Compare(left.nAbsoluteMagnitude,right.nAbsoluteMagnitude)", true),
                new Query("Star has belts","HasBelts IsTrue", QueryType.BuiltIn ),
                new Query("Star is heavier than Sol","nStellarMass > 1", QueryType.BuiltIn, false, "Compare(left.nStellarMass,right.nStellarMass)", false ),
                new Query("Star is wider than Sol","nRadius > 695700000", QueryType.BuiltIn, false, "Compare(left.nRadius,right.nRadius)" ,false ),

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
            JArray json = JArray.Parse(DB.UserDatabase.Instance.GetSetting(DbUserQueries, ""));
            if (json != null)
            {
                foreach (var t in json)
                {
                    Searches.Insert(0, new Query(t["Name"].Str("Unknown"), t["Condition"].Str("Unknown"), QueryType.User,
                        sortcond: t["SortCondition"].Str(), sortascending: t["SortAscending"].Bool()));
                }
            }
        }

        public string DefaultSearches(char splitmarkerin)
        {
            string[] names = Searches.Where(x => x.DefaultSearch).Select(x => x.Name).ToArray();
            return names.Join(splitmarkerin);
        }

        public void Set(string name, string expr, QueryType t, string sortcond = "", bool sortascending = false)
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
            DB.UserDatabase.Instance.PutSetting(DbUserQueries, json.ToString()); // allowed use
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
                        Set(name, condition, ty, sortcondition, sortascending);
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
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "Scan");

            List<BaseUtils.TypeHelpers.PropertyNameInfo> othernames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalFSSSignalDiscovered),
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "FSSSignalDiscovered");
            List<BaseUtils.TypeHelpers.PropertyNameInfo> saanames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalSAASignalsFound),
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "SAASignalsFound");
            othernames.AddRange(saanames);
            List<BaseUtils.TypeHelpers.PropertyNameInfo> fssbodynames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalFSSBodySignals),
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "FSSBodySignals");
            othernames.AddRange(fssbodynames);        // merge blind 
            List<BaseUtils.TypeHelpers.PropertyNameInfo> codexnames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalCodexEntry),
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                    classsepar: ".",
                    comment: "CodexEntry");
            othernames.AddRange(codexnames);        // merge blind
            List<BaseUtils.TypeHelpers.PropertyNameInfo> scanorganicnames = BaseUtils.TypeHelpers.GetPropertyFieldNames(typeof(JournalScanOrganic),
                    bindingflags: System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
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
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("SyncedEDDN", "Boolean value: 1 = true, 0 = false: Synced to EDDN", BaseUtils.ConditionEntry.MatchType.IsTrue, "All"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("IsOdyssey", "Boolean value: 1 = true, 0 = false: Odyssey journal entry", BaseUtils.ConditionEntry.MatchType.IsTrue, "All"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("IsHorizons", "Boolean value: 1 = true, 0 = false: Horizons journal entry", BaseUtils.ConditionEntry.MatchType.IsTrue, "All"));

            // add on ones we synthesise

            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Level", "Integer value: Level of body in system", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Sibling.Count", "Integer value: Number of siblings", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Bodies.Count", "Integer value: Number of bodies in the system", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("StarBodies.Count", "Integer value: Number of bodies in the star system the body is in", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Child.Count", "Integer value: Number of child moons", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("JumponiumCount", "Integer value: Number of jumponium materials available", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));

            // Add functions
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("BodiesPropertyCount(\"scan bool property\")", "Integer value: Number of bodies with scans with this boolean property true (Earthlike, HasAtmosphere etc)", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("StarBodiesPropertyCount(\"scan bool property\")", "Integer value: Number of bodies with scans in this star system with this boolean property true", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("BodiesExprCount(expression)", "Integer value: Number of bodies with scans with this expression evaluating to non zero (nMassEM>=1 && nMassEM<2) etc", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("StarBodiesExprCount(expression)", "Integer value: Number of bodies with scans in this star system with this expression evaluating to non zero", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));

            var defaultvars = new BaseUtils.Variables();
            defaultvars.AddPropertiesFieldsOfClass(new BodyPhysicalConstants(), "", null, 10);
            foreach (var v in defaultvars.NameEnumuerable)
                classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo(v, "Floating point value: Constant", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Constant"));

            classnames.Sort(delegate (BaseUtils.TypeHelpers.PropertyNameInfo left, BaseUtils.TypeHelpers.PropertyNameInfo right) { return left.Name.CompareTo(right.Name); });

            return classnames;
        }

        // Calculate from variables what is being searched for..
        static public HashSet<JournalTypeEnum> NeededSearchableTypes(HashSet<string> allvars, HashSet<string> allfuncs)
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
                        if (InComment(comment, "Scan", all))
                            res.Add(JournalTypeEnum.Scan);
                        if (InComment(comment, "FSSSignalDiscovered", all))
                            res.Add(JournalTypeEnum.FSSSignalDiscovered);
                        if (InComment(comment, "SAASignalsFound", all))
                            res.Add(JournalTypeEnum.SAASignalsFound);
                        if (InComment(comment, "FSSBodySignals", all))
                            res.Add(JournalTypeEnum.FSSBodySignals);
                        if (InComment(comment, "CodexEntry", all))
                            res.Add(JournalTypeEnum.CodexEntry);
                        if (InComment(comment, "ScanOrganic", all))
                            res.Add(JournalTypeEnum.ScanOrganic);
                    }
                }

                // specials, for scan
                if (v.StartsWith("Parent.") || v.StartsWith("Sibling") || v.StartsWith("Child") || v.StartsWith("Star.") || v.StartsWith("Bodies") || v.StartsWith("StarBodies"))
                    res.Add(JournalTypeEnum.Scan);
            }

            foreach(var v in allfuncs)
            {
                if ( v.StartsWith("Bodies") || v.StartsWith("StarBodies"))      // functions starting with these (sync with below) need scan
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
        public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist, Dictionary<string, List<ResultEntry>> results,
                            string filterdescription,
                            BaseUtils.Variables defaultvars,
                            StarScan2.StarScan starscan, 
                            bool wantreport)
        {
            var search = Searches.Find(x => x.Name.Equals(filterdescription));

            var cond = search != null ? new BaseUtils.ConditionLists(search.Condition) : null;

            if (cond == null)
                System.Diagnostics.Trace.WriteLine($"Queries did not find search {filterdescription} - probably the search list is out of date in surveyor");

            return Find(helist, results, filterdescription, cond, defaultvars, starscan, wantreport);
        }

        public class ResultEntry
        {
            public string FilterPassed { get; set; }
            public HistoryEntry HistoryEntry { get; set; }
        }


        // find using cond, async. return string of result info.
        // Fill in results dictionary (already made) with lock
        // default vars can be null
        // pass in the filterdescription which is the search name for the search, used to prevent repeats
        static public System.Threading.Tasks.Task<string> Find(List<HistoryEntry> helist,
                                   Dictionary<string, List<ResultEntry>> results, 
                                   string filterdescription,
                                   BaseUtils.ConditionLists cond,
                                   BaseUtils.Variables defaultvars,
                                   StarScan2.StarScan starscan, 
                                   bool wantreport)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                if (cond == null || cond.Count == 0)
                    return "Search Not Found";

                StringBuilder resultinfo = new StringBuilder(10000);

                var func = new QueryFunctionHandler();
                BaseUtils.Eval evl = new BaseUtils.Eval(func);          // evaluator, all true state, with a function handler..

                BaseUtils.Condition.InUse(cond.List, evl, out HashSet<string> allvars, out HashSet<string> _);

                int iterators = allvars.Where(x => x.StartsWith("Iter")).Select(x => x.Substring(4).InvariantParseInt(0)).DefaultIfEmpty(0).Max();
                bool wantjumponium = allvars.Contains("JumponiumCount");
                bool wantsiblingcount = allvars.Contains("Sibling.Count");
                bool wantchildcount = allvars.Contains("Child.Count");
                bool wantbodiescount = allvars.Contains("Bodies.Count");
                bool wantstarbodiescount = allvars.Contains("StarBodies.Count");
                bool wantlevel = allvars.Contains("Level");

                // extract variables needed to be filled in by the AddPropertiesFieldsOfClass function. We extract only the ones we need for speed reason.
                // Variables using the Name[] format, or Class_subclass naming system need to have the [ and _ text stripped off for the property expander to iterate thru them

                string[] stoptext = new string[] { "[", "." };

                HashSet<string> varsparent = new HashSet<string>();
                HashSet<string> varsgrandparent = new HashSet<string>();
                HashSet<string> varssiblings = new HashSet<string>();
                HashSet<string> varschildren = new HashSet<string>();
                HashSet<string> varsevent = new HashSet<string>();
                HashSet<string> varsstar = new HashSet<string>();
                HashSet<string> varsstarstar = new HashSet<string>();
                HashSet<string> varsbodies = new HashSet<string>();
                HashSet<string> varsstarbodies = new HashSet<string>();

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
                        varsgrandparent.Add(v.Substring(14, v.IndexOfOrLength(stoptext, startindex: 14) - 14));
                    }
                    else if (v.StartsWith("Parent."))
                    {
                        varsparent.Add(v.Substring(7, v.IndexOfOrLength(stoptext, startindex: 7) - 7));
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
                    else if (v.StartsWith("Bodies["))
                    {
                        var v1 = v.Substring(v.IndexOfOrLength("]", offset: 2));
                        varsbodies.Add(v1.Substring(0, v1.IndexOfOrLength(stoptext)));
                    }
                    else if (v.StartsWith("StarBodies["))
                    {
                        var v1 = v.Substring(v.IndexOfOrLength("]", offset: 2));
                        varsstarbodies.Add(v1.Substring(0, v1.IndexOfOrLength(stoptext)));
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
                //foreach (var v in varsbodies) System.Diagnostics.Debug.WriteLine($"Search Bodies Var {v}");
                //foreach (var v in varsstarbodies) System.Diagnostics.Debug.WriteLine($"Search Star Bodies Var {v}");

                Type[] ignoretypes = new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) };


                //foreach (var he in helist.GetRange(helist.Count-100,100))
                foreach (var he in helist)
                {
                    //if (he.System.Name != "Byeia Euq WU-O d6-26")
                    //    continue;

                    BaseUtils.Variables scandatavars = defaultvars != null ? new BaseUtils.Variables(defaultvars) : new BaseUtils.Variables();

                    bool debugit = false;

                    //if ( he.System.Name == "Lu Dongia") debugit = true;

                    scandatavars["EventType"] = he.EntryType.ToString();

                    scandatavars.AddPropertiesFieldsOfClass(he.journalEntry, "", ignoretypes, 5, varsevent, ensuredoublerep: true, classsepar: ".");

                    if (wantjumponium)
                    {
                        JournalScan js = he.journalEntry as JournalScan;

                        if (js != null)   // if its a journal scan
                        {
                            scandatavars["JumponiumCount"] = js.Jumponium().ToStringInvariant();
                        }
                    }

                    // if this is a journal scan, and we have a results on this bodyname
                    if (he.EntryType == JournalTypeEnum.Scan)
                    {
                        lock (results)      // parallel awaits, so lock
                        {
                            if (results.TryGetValue((he.journalEntry as JournalScan).BodyName, out List<ResultEntry> prevres))
                            {
                                // we have results on this body, see if any is a prev scan
                                var prevscan = prevres.Find(x => x.HistoryEntry.EntryType == JournalTypeEnum.Scan);

                                // we have a scan, and its got the same search ID name (note discoveries/surveyor reuse the results structure across search names, so need to screen it out)
                                if (prevscan != null && prevscan.FilterPassed.Equals(filterdescription))
                                {
                                    // then we nuke it
                                    string bodyname = (he.journalEntry as JournalScan).BodyName;

                                    // System.Diagnostics.Debug.WriteLine($"Detected repeated scan result on {bodyname}:`{filterdescription}` remove previous");
                                    prevres.Remove(prevscan);
                                    if (prevres.Count == 0)
                                        results.Remove(bodyname);
                                }
                            }
                        }
                    }

                    // concurrency with the foreground adding new scan nodes as we process

                    if (he.BodyNode != null)      // if it has a scan node
                    {
                        lock (he.BodyNode.SystemNode)   // no more changes to this system during processing
                        {
                            if (wantlevel)
                                scandatavars["Level"] = he.BodyNode.GetNameDepth().ToStringInvariant();

                            BodyNode parent = he.BodyNode.GetParentIgnoreBC();

                            // we may not have a parent if we are the top of the tree

                            if (parent != null)
                            {
                                if (varsparent.Count > 0)
                                {
                                    // parent journal entry, may be null. No concurrency issues 
                                    var parentjs = parent.Scan;

                                    if (parentjs != null) // if want parent scan data
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(parentjs, "Parent.", ignoretypes, 5, varsparent, ensuredoublerep: true, classsepar: ".");
                                        scandatavars["Parent.Level"] = parent.GetNameDepth().ToStringInvariant();
                                    }
                                }

                                BodyNode grandparent = parent.GetParentIgnoreBC();

                                if (varsgrandparent.Count > 0 && grandparent != null)        // if want parent.parent and we have one
                                {
                                    var parentparentjs = grandparent.Scan;

                                    if (parentparentjs != null) // if want parent scan data
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(parentparentjs, "Parent.Parent.", ignoretypes, 5, varsgrandparent, ensuredoublerep: true, classsepar: ".");
                                        scandatavars["Parent.Parent.Level"] = grandparent.GetNameDepth().ToStringInvariant();
                                    }
                                }
                            }

                            if (varsstar.Count > 0)
                            {
                                var parentstarjs = he.BodyNode.GetStarAboveScanned(0)?.Scan;

                                if (parentstarjs != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine($"{scandata.BodyName} is the Star parent");

                                    scandatavars.AddPropertiesFieldsOfClass(parentstarjs, "Star.",
                                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                            varsstar, ensuredoublerep: true, classsepar: ".");
                                }
                            }

                            if (varsstarstar.Count > 0)
                            {
                                var grandfatherjs = he.BodyNode.GetStarAboveScanned(1)?.Scan;

                                if (grandfatherjs != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine($"{scandata.BodyName} is the Star Star parent");

                                    scandatavars.AddPropertiesFieldsOfClass(grandfatherjs, "Star.Star.",
                                            new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                            varsstarstar, ensuredoublerep: true, classsepar: ".");
                                }
                            }


                            var siblings = wantsiblingcount || varssiblings.Count > 0 ? he.BodyNode.GetSiblingBodiesNoBarycentres() : null;

                            if (siblings !=null)
                            {
                                scandatavars["Sibling.Count"] = (siblings.Count-1).ToStringInvariant();  
                            }

                            if (siblings != null && varssiblings.Count > 0)        // if want sibling[
                            {
                                int cno = 1;
                                foreach (var bn in siblings)
                                {
                                    if (bn != he.BodyNode && bn.Scan != null)        // if not ours and has a scan
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(bn.Scan, $"Sibling[{cno}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varssiblings, ensuredoublerep: true, classsepar: ".");
                                        cno++;
                                    }
                                }
                            }

                            var children = wantchildcount || varschildren.Count > 0 ? he.BodyNode.GetChildBodiesNoBarycentres() : null;

                            if (children!=null)
                            {
                                scandatavars["Child.Count"] = he.BodyNode.ChildBodies.Count.ToStringInvariant();      // count of children
                            }

                            if (children != null && varschildren.Count > 0)        // if want children[
                            {
                                int cno = 1;
                                foreach (var sn in children)
                                {
                                    if (sn.Scan != null)        // if not ours and has a scan
                                    {
                                        int cc = scandatavars.Count;

                                        scandatavars.AddPropertiesFieldsOfClass(sn.Scan, $"Child[{cno}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varschildren, ensuredoublerep: true, classsepar: ".");

                                        cno++;
                                    }
                                }
                            }

                            if (wantbodiescount || varsbodies.Count > 0)
                            {
                                int count = 0;
                                foreach (var sn in he.BodyNode.SystemNode.Bodies())
                                {
                                    if (varsbodies.Count > 0 && sn.Scan != null)
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(sn.Scan, $"Bodies[{count + 1}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varsbodies, ensuredoublerep: true, classsepar: ".");

                                    }
                                    count++;
                                }

                                if (wantbodiescount)
                                    scandatavars["Bodies.Count"] = count.ToStringInvariant();
                            }


                            if (wantstarbodiescount || varsstarbodies.Count > 0)
                            {
                                int count = 0;
                                foreach (var sn in he.BodyNode.SystemNode.GetStarsScanned())     
                                {
                                    if (varsstarbodies.Count >0)
                                    {
                                        scandatavars.AddPropertiesFieldsOfClass(sn.Scan, $"StarBodies[{count + 1}].",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                varsstarbodies, ensuredoublerep: true, classsepar: ".");

                                    }
                                    count++;
                                }

                                if (wantstarbodiescount)
                                    scandatavars["StarBodies.Count"] = count.ToStringInvariant();
                            }
                        }
                    }       // end check of he.BodyNode

                    List<BaseUtils.ConditionEntry> testspassed = wantreport ? new List<BaseUtils.ConditionEntry>() : null;

                    //JournalScan jsc = he.journalEntry as JournalScan; if (jsc?.BodyName == "Blue Euq WA-E d12-32 7 a") debugit = true;

                    //System.Diagnostics.Debug.WriteLine($"Star {he.System.Name}");
                    //foreach (var v in scandatavars.NameEnumuerable) System.Diagnostics.Debug.WriteLine($"Search scandata var {v} = {scandatavars[v]}");

                    evl.ReturnSymbolValue = scandatavars;                   // point the eval at our scan data variables for this particular instance
                    func.SystemNode = he.BodyNode?.SystemNode;              // point the functions at system node, if it exists
                    func.ParentStar = he.BodyNode?.GetStarAboveScanned(0);  // point the functions at the scanned parent star node, if it exists

                    var res = BaseUtils.ConditionLists.CheckConditionsEvalIterate(evl, scandatavars, cond.List, iterators, debugit: debugit);

                    if (wantreport)
                    {
                        JournalScan jsi = he.journalEntry as JournalScan;
                        resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} {jsi?.BodyName} : {res.Item1} : Last {res.Item2.Last().ItemName} {res.Item2.Last().MatchCondition} {res.Item2.Last().MatchString} : {res.Item3.Last() ?? ""}");
                        //foreach ( var x in res.Item2)    resultinfo.AppendLine($"  {x.ItemName} {x.MatchCondition} {x.MatchString}");

                        // System.Diagnostics.Debug.WriteLine($"For entry type {he.EventTimeUTC} {he.EntryType} error: {resultinfo}");
                    }

                    if (res.Item1.Value == true)    // if passed
                    {
                        //if we have a je with a body name, use that to set the ret, else just use a incrementing decimal count name

                        string key = null;
                        if (he.EntryType == JournalTypeEnum.FSSSignalDiscovered)
                        {
                            // find real body name for signal, not the one in the history entry as it could be produced in the previous system

                            long? sysaddr = ((JournalFSSSignalDiscovered)he.journalEntry).Signals[0].SystemAddress;
                            if (sysaddr.HasValue && starscan.TryGetSystemNode(sysaddr.Value, out var sn))
                            {
                                key = sn.System.Name;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Query FSD No system found for {he.EventTimeUTC} {sysaddr}");
                            }
                        }
                        else if (he.journalEntry is IBodyFeature)
                        {
                            key = (he.journalEntry as IBodyFeature).BodyName;
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
                            if (starscan.TryGetSystemNode(so.SystemAddress, out StarScan2.SystemNode sn))
                            {
                                if (sn.TryGetBody(so.Body, out var bn))
                                    key = bn.Name();
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
                                var re = new ResultEntry { HistoryEntry = he, FilterPassed = filterdescription };
                                if (results.TryGetValue(key, out List<ResultEntry> value))       // if key already exists, maybe set HE to us, and update filters passed
                                {
                                    if ( value.Find(x=>x.FilterPassed == re.FilterPassed) == null)      // don't repeat entry per body
                                        value.Add(re);
                                    //  System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC} `{filterdescription}` matched {key} added to list");
                                }
                                else
                                {                                                       // else make a new list
                                    results[key] = new List<ResultEntry> { re };
                                    //   System.Diagnostics.Debug.WriteLine($"{he.EventTimeUTC} `{filterdescription}` matched {key} new body");
                                }
                            }
                        }
                    }
                }
                return resultinfo.ToString();
            });
        }

        public static void GenerateReportFields(string bodykey, List<ResultEntry> resultlist, out string name, out string info, out string infotooltip,
                                                bool pinfowanted, out string pinfo,
                                                bool ppinfowanted, out string ppinfo,
                                                bool sinfowanted, out string sinfo,
                                                bool ssinfowanted, out string ssinfo)
        {
            name = bodykey;

            info = pinfo = infotooltip = ppinfo = sinfo = ssinfo = "";

            // if we have a scan in the results list, do that first
            HistoryEntry hescan = resultlist.Select(sd => sd.HistoryEntry).Where(x => x.EntryType == JournalTypeEnum.Scan).LastOrDefault();

            if (hescan != null)
            {
                JournalScan js = hescan.journalEntry as JournalScan;

                info = js.DisplayText();

                if (hescan.BodyNode != null)
                {
                    if (pinfowanted )
                    {
                        var pnode = hescan.BodyNode.GetParentIgnoreBC();
                        if (pnode != null)
                        {
                            var parentjs = pnode.Scan;               // parent journal entry, may be null
                            pinfo = parentjs != null ? parentjs.DisplayText() : hescan.BodyNode.Parent.Name() + " " + hescan.BodyNode.Parent.BodyType;
                        }
                    }

                    if (ppinfowanted )
                    {
                        var pnode = hescan.BodyNode.GetParentIgnoreBC();
                        var ppnode = pnode?.GetParentIgnoreBC();

                        if (ppnode != null)
                        {
                            var parentparentjs = hescan.BodyNode.Parent.Parent.Scan;               // parent journal entry, may be null
                            ppinfo = parentparentjs != null ? parentparentjs.DisplayText() : hescan.BodyNode.Parent.Parent.Name() + " " + hescan.BodyNode.Parent.Parent.BodyType;
                        }
                    }

                    if (sinfowanted)
                    {
                        var starnode = hescan.BodyNode.GetStarAboveScanned(0);

                        if (starnode != null)
                        {
                            sinfo = starnode.Scan.DisplayText();
                        }
                    }

                    if (ssinfowanted)
                    {
                        var starnode = hescan.BodyNode.GetStarAboveScanned(1);

                        if (starnode != null)
                        {
                            ssinfo = starnode.Scan.DisplayText();
                        }
                    }
                }
            }

            foreach (var res in resultlist)
            {
                var he = res.HistoryEntry;

                if (he.EntryType != JournalTypeEnum.Scan)      // for all the rest of the results, ignoring scan
                {
                    string time = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(he.EventTimeUTC).ToString();
                    if (info.HasChars())
                        info = info.AppendPrePad(time + ": " + he.GetInfo(), Environment.NewLine);
                    else
                        info = info.AppendPrePad(he.GetInfo(), Environment.NewLine);

                    string detailed = he.GetDetailed();     // may be null
                    if (detailed.HasChars())
                    {
                        infotooltip += time + Environment.NewLine + detailed.LineIndentation("    ") + Environment.NewLine;
                    }
                }
            }
        }
    }

    // Extra special functions for this dialog
    public class QueryFunctionHandler : BaseUtils.BaseFunctionsForEval
    {
        public StarScan2.SystemNode SystemNode { get; set; }     // node we are querying, may be null
        public StarScan2.BodyNode ParentStar { get; set; }       // Parent star node we are querying, may be null

        public override object Execute(string name, BaseUtils.IEval evaluator, bool noop)
        {
            if (name == "BodiesPropertyCount" || name == "StarBodiesPropertyCount")
            {
                List<Object> plist = evaluator.Parameters(name, 1, new BaseUtils.IEvalParaListType[] { BaseUtils.IEvalParaListType.String });

                if (plist != null)
                {
                    string p1 = plist[0] as string;

                    if (noop)       // noop during func/sym collection, just return 0L
                    {
                        return 0L;
                    }
                    else if (SystemNode != null)
                    {
                        long count = 0;

                        lock (SystemNode)
                        {
                            var bodies = name == "BodiesPropertyCount" ? SystemNode.Bodies() : ParentStar.Bodies();

                            foreach (var b in bodies)
                            {
                                if (b.Scan != null)
                                {
                                    if (BaseUtils.TypeHelpers.TryGetValue(b.Scan, p1, out bool value))
                                    {
                                        if (value == true)
                                            count++;
                                    }
                                    else
                                        return new BaseUtils.StringParser.ConvertError(name + $"() property {p1} is not present or not bool");
                                }
                            }
                        }

                        //if (count > 0) System.Diagnostics.Debug.WriteLine($"BodiesCount {SystemNode.System.Name} = {count}");

                        return count;
                    }
                    else
                        return new BaseUtils.StringParser.ConvertError(name + "() No system node");
                }
                else
                    return new BaseUtils.StringParser.ConvertError(name + "() Missing string parameter of property name");
            }
            else if (name == "BodiesExprCount" || name == "StarBodiesExprCount")
            {
                List<Object> plist = evaluator.Parameters(name, 1, new BaseUtils.IEvalParaListType[] { BaseUtils.IEvalParaListType.CollectAsString});

                if (plist != null)
                {
                    string expr = plist[0] as string;

                    if (noop)       // noop during func/sym collection, just return 0L
                    {
                        return 0L;
                    }
                    else if (SystemNode != null)
                    {
                        if ( expr.HasChars() )
                        { 
                            BaseUtils.IEval neweval = evaluator.Clone(); // do not disturb the current eval

                            neweval.SymbolsFuncsInExpression(expr, out HashSet<string> allvars, out HashSet<string> _);

                            long count = 0;

                            lock (SystemNode)
                            {
                                var bodies = name == "BodiesExprCount" ? SystemNode.Bodies() : ParentStar.Bodies();

                                foreach (var b in bodies)
                                {
                                    if (b.Scan != null)
                                    {
                                        BaseUtils.Variables scandatavars = new BaseUtils.Variables();

                                        // enumerate this scan data into variable set

                                        scandatavars.AddPropertiesFieldsOfClass(b.Scan, "",
                                                new Type[] { typeof(System.Drawing.Icon), typeof(System.Drawing.Image), typeof(System.Drawing.Bitmap), typeof(QuickJSON.JObject) }, 5,
                                                allvars, ensuredoublerep: true, classsepar: ".");

                                        neweval.ReturnSymbolValue = scandatavars;        // redirect symbols to gathered scandatavars

                                        object res = neweval.Evaluate(expr);            // evaluate

                                        if (res is long && (long)res != 0)              // if non zero and int, its a match
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }

                            //if (count > 0) System.Diagnostics.Debug.WriteLine($"BodiesCount {SystemNode.System.Name} = {count}");

                            return count;
                        }
                        else
                            return new BaseUtils.StringParser.ConvertError(name + "() Expression empty");

                    }
                    else
                        return new BaseUtils.StringParser.ConvertError(name + "() No system node");
                }
                else
                    return new BaseUtils.StringParser.ConvertError(name + "() Missing string parameter of property name");
            }

            return base.Execute(name, evaluator, noop);
        }

    }
}
