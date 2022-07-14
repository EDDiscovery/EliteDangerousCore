﻿/*
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

        public const string DefaultSearches = "Planet between inner and outer ringↈLandable and TerraformableↈLandable with High GↈLandable with RingsↈHotter than HadesↈPlanet has wide rings vs radiusↈClose orbit to parentↈClose to ringↈPlanet with a large number of MoonsↈMoons orbiting TerraformablesↈClose BinaryↈGas giant has a terraformable MoonↈTiny MoonↈFast Rotation of a non tidally locked bodyↈHigh Eccentric OrbitↈHigh number of Jumponium Materialsↈ";

        public enum QueryType { BuiltIn, User, Example };

        public class Query
        {
            public string Name { get; set; }
            public string Condition { get; set; }

            public QueryType QueryType { get; set; }

            public Query(string n, string c, QueryType qt) { Name = n; Condition = c; QueryType = qt; }

            public bool User { get { return QueryType == QueryType.User; } }
            public bool UserOrBuiltIn { get { return QueryType == QueryType.BuiltIn || QueryType == QueryType.User; } }
        }

        public List<Query> Searches = new List<Query>()
            {
                new Query("Planet inside inner ring","(IsOrbitingBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + //single body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[2].IsBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsFalse And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsTrue And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsInnerm And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                new Query("Planet inside rings","(IsOrbitingBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" +
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[2].IsBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsFalse And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsTrue And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                new Query("Planet between inner and outer ring","(IsOrbitingBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.RingsInnerm And nSemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" +
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[2].IsBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[1].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsFalse And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[2].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsTrue And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.RingsInnerm And Parents[3].Barycentre.SemiMajorAxis <= Parent.RingsOuterm And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                new Query("Planet between rings 1 and 2","(IsOrbitingBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.Rings[1].OuterRad And nSemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" +
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[1].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsFalse And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[2].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsTrue And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.Rings[1].OuterRad And Parents[3].Barycentre.SemiMajorAxis <= Parent.Rings[2].InnerRad And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                new Query("Planet between rings 2 and 3","(IsOrbitingBaryCentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And nSemiMajorAxis >= Parent.Rings[2].OuterRad And nSemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" +
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[2].IsBarycentre IsFalse And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[1].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[1].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" + // binary body
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsFalse And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[2].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[2].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)" + // trinary and ((O-O)-(O-O)) quarteries
                            " Or (IsOrbitingBaryCentre IsTrue And Parents[3].IsBaryCentre IsTrue And Parents[2].IsBaryCentre IsTrue And IsPlanet IsTrue And Parent.HasRings IsTrue And Parents[3].Barycentre.SemiMajorAxis >= Parent.Rings[2].OuterRad And Parents[3].Barycentre.SemiMajorAxis <= Parent.Rings[3].InnerRad And Parent.Level >= 1)", QueryType.BuiltIn ), // (((O-O)-O)-O) quartery
                

                new Query("Heavier than Sol","nStellarMass > 1", QueryType.BuiltIn ),
                new Query("Bigger than Sol","nRadius > 695700000", QueryType.BuiltIn ),

                new Query("Landable","IsPlanet IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Landable and Terraformable","IsPlanet IsTrue And IsLandable IsTrue And Terraformable IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Atmosphere","IsPlanet IsTrue And IsLandable IsTrue And HasAtmosphere IsTrue", QueryType.BuiltIn ),
                new Query("Landable with High G","IsPlanet IsTrue And IsLandable IsTrue And nSurfaceGravityG >= 3", QueryType.BuiltIn ),
                new Query("Landable large planet","IsPlanet IsTrue And IsLandable IsTrue And nRadius >= 12000000", QueryType.BuiltIn ),
                new Query("Landable with Rings","IsPlanet IsTrue And IsLandable IsTrue And HasRings IsTrue", QueryType.BuiltIn ),
                new Query("Has Volcanism","HasMeaningfulVolcanism IsTrue", QueryType.BuiltIn ),
                new Query("Landable with Volcanism","HasMeaningfulVolcanism IsTrue And IsLandable IsTrue", QueryType.BuiltIn ),
                new Query("Earth like planet","Earthlike IsTrue", QueryType.BuiltIn ),
                new Query("Bigger than Earth","IsPlanet IsTrue And nMassEM > 1", QueryType.BuiltIn ),
                new Query("Hotter than Hades","IsPlanet IsTrue And nSurfaceTemperature >= 2273", QueryType.BuiltIn ),

                new Query("Has Rings","HasRings IsTrue", QueryType.BuiltIn ),
                new Query("Star has Rings","HasRings IsTrue And IsStar IsTrue", QueryType.BuiltIn ),
                new Query("Has Belts","HasBelts IsTrue", QueryType.BuiltIn ),

                new Query("Planet has wide rings vs radius","(IsPlanet IsTrue And HasRings IsTrue ) And ( Rings[Iter1].OuterRad-Rings[Iter1].InnerRad >= nRadius*5)", QueryType.BuiltIn ),

                new Query("Close orbit to parent","IsPlanet IsTrue And Parent.IsPlanet IsTrue And IsOrbitingBaryCentre IsFalse And Parent.nRadius*3 > nSemiMajorAxis", QueryType.BuiltIn ),

                new Query("Close to ring",
                                "( IsPlanet IsTrue And Parent.IsPlanet IsTrue And Parent.HasRings IsTrue And IsOrbitingBaryCentre IsFalse ) And " +
                                "( \"Abs(Parent.Rings[Iter1].InnerRad-nSemiMajorAxis)\" < nRadius*10 Or  \"Abs(Parent.Rings[Iter1].OuterRad-nSemiMajorAxis)\" < nRadius*10 )"
                    , QueryType.BuiltIn ),
                new Query("Binary close to rings","(IsOrbitingBaryCentre IsTrue And Parents[2].IsBaryCentre IsFalse And Parent.HasRings IsTrue And IsPlanet IsTrue) And " + 
                            "(\"Abs(Parent.Rings[Iter1].InnerRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\" Or \"Abs(Parent.Rings[Iter1].OuterRad-Parents[1].Barycentre.SemiMajorAxis)\" < \"(nSemiMajorAxis+nRadius)*20\")", QueryType.BuiltIn ),

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
                new Query("Contains an Installation",  "CountInstallationSignals >= 1", QueryType.BuiltIn ),
                new Query("Contains a Carrier",  "CountCarrierSignals >= 1", QueryType.BuiltIn ),
                new Query("Contains a NSP",  "CountNotableStellarPhenomenaSignals >= 1", QueryType.BuiltIn ),

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
                Searches.Insert(0, new Query(userqueries[i], userqueries[i + 1], QueryType.User));
        }

        public void Set(string name, string expr, QueryType t)
        {
            var entry = Searches.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (entry != -1)
                Searches[entry] = new Query(name, expr, t);
            else
                Searches.Insert(0, new Query(name, expr, t));
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
            foreach (var t in ja)
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

            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventTimeUTC", "Date Time in UTC", BaseUtils.ConditionEntry.MatchType.DateAfter, "All"));     // add on a few from the base class..
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventTimeLocal", "Date Time in Local time", BaseUtils.ConditionEntry.MatchType.DateAfter, "All"));
         //   classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("EventType", "Event type", BaseUtils.ConditionEntry.MatchType.Equals, "All"));
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("SyncedEDSM", "Synced to EDSM, 1 = yes, 0 = not", BaseUtils.ConditionEntry.MatchType.IsTrue, "All"));

            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Level", "Level of body in system, 0 =star, 1 = Planet, 2 = moon, 3 = submoon", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Sibling.Count", "Number of siblings", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("Child.Count", "Number of child moons", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Scan"));     // add on ones we synthesise
            classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo("JumponiumCount", "Number of jumponium materials available", BaseUtils.ConditionEntry.MatchType.NumericGreaterEqual, "Scan"));     // add on ones we synthesise

            var defaultvars = new BaseUtils.Variables();
            defaultvars.AddPropertiesFieldsOfClass(new BodyPhysicalConstants(), "", null, 10);
            foreach (var v in defaultvars.NameEnumuerable)
                classnames.Add(new BaseUtils.TypeHelpers.PropertyNameInfo(v, "Constant", BaseUtils.ConditionEntry.MatchType.NumericEquals, "Constant"));

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

                if (v.StartsWith("Parent.") || v.StartsWith("Parent.Parent.") || v.StartsWith("Sibling") || v.StartsWith("Child"))      // specials, for scan
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

                    var res = BaseUtils.ConditionLists.CheckConditionsEvalIterate(cond.List, scandatavars, out string evalerrlist, out BaseUtils.ConditionLists.ErrorClass errclassunused, wantiter1 || wantiter2, debugit: debugit);

                    //if (res.Item1 == false && res.Item2.Last().ItemName.Contains("Parent.Rings[Iter1].OuterRad")) debugit = true;

                    if (wantreport)
                    {
                        JournalScan jsi = he.journalEntry as JournalScan;
                        resultinfo.AppendLine($"{he.EventTimeUTC} Journal type {he.EntryType} {jsi?.BodyName} : {res.Item1} : {evalerrlist} : Last {res.Item2.Last().ItemName} {res.Item2.Last().MatchCondition} {res.Item2.Last().MatchString}");
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
            var plist = node.ScanData?.Parents;
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


        public static void GenerateReportFields(string bodykey, List<HistoryEntry> hes, out string name, out string info, out string infotooltip, out string pinfo)
        {
            name = "";
            info = "";
            pinfo = "";
            infotooltip = "";

            HistoryEntry he = hes.Last();

            if (he.EntryType == JournalTypeEnum.Scan)
            {
                JournalScan js = he.journalEntry as JournalScan;
                name = js.BodyName;
                info = js.DisplayString();
                if (he.ScanNode?.Parent != null)
                {
                    var parentjs = he.ScanNode?.Parent?.ScanData;               // parent journal entry, may be null
                    pinfo = parentjs != null ? parentjs.DisplayString() : he.ScanNode.Parent.CustomNameOrOwnname + " " + he.ScanNode.Parent.NodeType;
                }
            }
            else if (he.EntryType == JournalTypeEnum.FSSBodySignals)
            {
                JournalFSSBodySignals jb = he.journalEntry as JournalFSSBodySignals;
                name = jb.BodyName;
                jb.FillInformation(he.System, "", out info, out string d);
            }
            else if (he.EntryType == JournalTypeEnum.SAASignalsFound)
            {
                JournalSAASignalsFound jbs = he.journalEntry as JournalSAASignalsFound;
                name = jbs.BodyName;
                jbs.FillInformation(he.System, "", out info, out string d);
            }
            else if (he.EntryType == JournalTypeEnum.FSSSignalDiscovered)
            {
                JournalFSSSignalDiscovered jfsd = he.journalEntry as JournalFSSSignalDiscovered;

                name = he.System.Name;
                foreach (var h in hes)
                {
                    string time = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(h.EventTimeUTC).ToString();
                    ((JournalFSSSignalDiscovered)h.journalEntry).FillInformation(he.System, "", 20, out string info2, out string detailed);
                    if (hes.Count > 1)
                        info = info.AppendPrePad(time + ": " + info2, Environment.NewLine);
                    else
                        info = info.AppendPrePad(info2, Environment.NewLine);

                    infotooltip += time + Environment.NewLine + detailed.LineIndentation("    ") + Environment.NewLine;
                }
            }
            else if (he.EntryType == JournalTypeEnum.CodexEntry)
            {
                name = bodykey;
                foreach (var h in hes)
                {
                    JournalCodexEntry ce = h.journalEntry as JournalCodexEntry;
                    string time = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(h.EventTimeUTC).ToString();
                    ce.FillInformation(he.System, "", out string info2, out string d);
                    if (hes.Count > 1)
                        info = info.AppendPrePad(time + ": " + info2, Environment.NewLine);
                    else
                        info = info.AppendPrePad(info2, Environment.NewLine);
                }
            }
            else if (he.EntryType == JournalTypeEnum.ScanOrganic)
            {
                name = bodykey;
                foreach (var h in hes)
                {
                    var so = h.journalEntry as JournalScanOrganic;
                    string time = EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(h.EventTimeUTC).ToString();

                    so.FillInformation(h.System, "", out string info2, out string d);
                    if (hes.Count > 1)
                        info = info.AppendPrePad(time + ": " + info2, Environment.NewLine);
                    else
                        info = info.AppendPrePad(info2, Environment.NewLine);
                }
            }
            else
                System.Diagnostics.Debug.Assert(false, "Missing journal type decode");
        }
    }

}
