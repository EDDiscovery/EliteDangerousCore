/*`
 * Copyright © 2015 - 2021 EDDiscovery development team
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
using System.IO;
using System.Linq;

namespace EliteDangerousCore
{
    // class holding alternative names for body names due to lovely frontier strangeness

    public partial class BodyDesignations
    {
        // fudge tables
        private static Dictionary<string, Dictionary<string, string>> planetDesignationMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, Dictionary<string, string>> starDesignationMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, Dictionary<int, BodyDesignations.DesigMap>> bodyIdDesignationMap = new Dictionary<string, Dictionary<int, BodyDesignations.DesigMap>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, List<JournalScan>> primaryStarScans = new Dictionary<string, List<JournalScan>>(StringComparer.InvariantCultureIgnoreCase);

        public static void LoadBodyDesignationMap(string desigmapfile)
        {
            if (File.Exists(desigmapfile))
            {
                foreach (string line in File.ReadLines(desigmapfile))
                {
                    string[] fields = line.Split(',').Select(s => s.Trim('"')).ToArray();
                    if (fields.Length == 3)
                    {
                        string sysname = fields[0];
                        string bodyname = fields[1];
                        string desig = fields[2];
                        Dictionary<string, Dictionary<string, string>> desigmap = planetDesignationMap;

                        if (desig == sysname || (desig.Length == sysname.Length + 2 && desig[sysname.Length + 1] >= 'A' && desig[sysname.Length + 1] <= 'F'))
                        {
                            desigmap = starDesignationMap;
                        }

                        if (!desigmap.ContainsKey(sysname))
                        {
                            desigmap[sysname] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                        }

                        desigmap[sysname][bodyname] = desig;
                    }
                }
            }
            else
            {
                foreach (var skvp in BodyDesignations.Stars)
                {
                    if (!starDesignationMap.ContainsKey(skvp.Key))
                    {
                        starDesignationMap[skvp.Key] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    }

                    foreach (var bkvp in skvp.Value)
                    {
                        starDesignationMap[skvp.Key][bkvp.Key] = bkvp.Value;
                    }
                }

                foreach (var skvp in BodyDesignations.Planets)
                {
                    if (!planetDesignationMap.ContainsKey(skvp.Key))
                    {
                        planetDesignationMap[skvp.Key] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    }

                    foreach (var bkvp in skvp.Value)
                    {
                        planetDesignationMap[skvp.Key][bkvp.Key] = bkvp.Value;
                    }
                }

                foreach (var skvp in BodyDesignations.ByBodyId)
                {
                    if (!bodyIdDesignationMap.ContainsKey(skvp.Key))
                    {
                        bodyIdDesignationMap[skvp.Key] = new Dictionary<int, BodyDesignations.DesigMap>();
                    }

                    foreach (var bkvp in skvp.Value)
                    {
                        bodyIdDesignationMap[skvp.Key][bkvp.Key] = bkvp.Value;
                    }
                }
            }
        }

        // same as StarScanHelpers:GetBodyDesignation

        internal static string GetBodyDesignation(JournalScan je, string system)
        {
            if (je.IsStar && system.ToLowerInvariant() == "9 Aurigae")      // special star one here
            {
                if (je.BodyName == "9 Aurigae C")
                {
                    if (je.nSemiMajorAxis > 1e13)
                    {
                        return "9 Aurigae D";
                    }
                    else
                    {
                        return "9 Aurigae C";
                    }
                }
            }

            string s = GetBodyDesignation(je.BodyName, je.BodyID, je.IsStar, system, true);

            if (s == null)  // continue..
            {
                // catch a star with the same name as the system, if so, its the primary A star

                if (je.IsStar && je.BodyName.Equals(system, StringComparison.InvariantCultureIgnoreCase) && je.nOrbitalPeriod != null)
                {
                    return system + " A";
                }

                // if the bodyname == system, or bodyname starts with system name + " ", its the best

                if (je.BodyName.Equals(system, StringComparison.InvariantCultureIgnoreCase) || je.BodyName.StartsWith(system + " ", StringComparison.InvariantCultureIgnoreCase))
                {
                    return je.BodyName;
                }

                // this catches systems like Pachamana/Gliese 893.3 B
                // both are in system Pachamana, when Pachamana A is added its caught by line 105 of journalscans.cs, and adds it as a primary body
                // it then does a reprocess of the existing nodes to catch any names which need nerfing
                // Gliese 839.3 B, with the same orbital parameters, is then recognised as the B star of the system

                if (je.IsStar && primaryStarScans.ContainsKey(system))
                {
                    foreach (JournalScan primary in primaryStarScans[system])
                    {
                        // see if the cached primary star matches the orbital parameters of the JE, and its not the primary system itself.

                        if (CompareEpsilon(je.nOrbitalPeriod, primary.nOrbitalPeriod) &&
                            CompareEpsilon(je.nPeriapsis, primary.nPeriapsis, acceptNull: true, fb: b => ((double)b + 180) % 360.0) &&
                            CompareEpsilon(je.nOrbitalInclination, primary.nOrbitalInclination) &&
                            CompareEpsilon(je.nEccentricity, primary.nEccentricity) &&
                            !je.BodyName.Equals(primary.BodyName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return system + " B";       // if so, its really the B star of the system
                        }
                    }
                }
            }
            else
                return s;

            return je.BodyName;
        }

        // does the same as StarScanBodyID::GetBodyDesignation. Always returns a system.  bodyid can be null

        internal static string GetBodyDesignation(string bodyname, int? bodyid, bool isstar, string system, bool returnnullifnomatch = false)
        {
            System.Diagnostics.Debug.Assert(bodyname != null);

            // fixed nerfs by body id
            // if the body id exists, the body map has the system, the inner dictionary has the bodyid, and the name is equal, return its designation

            if (bodyid != null && bodyIdDesignationMap.ContainsKey(system) && bodyIdDesignationMap[system].ContainsKey(bodyid.Value) && bodyIdDesignationMap[system][bodyid.Value].NameEquals(bodyname))
            {
                return bodyIdDesignationMap[system][bodyid.Value].Designation;
            }

            // special case for m centauri
            if (isstar && system.ToLowerInvariant() == "m centauri")
            {
                if (bodyname == "m Centauri")
                {
                    return "m Centauri A";
                }
                else if ( bodyname == "M Centauri")
                {
                    return "m Centauri B";
                }
            }

            // Special case for Castellan Belt
            if (system.ToLowerInvariant() == "lave" && bodyname.StartsWith("Castellan Belt ", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Lave A Belt " + bodyname.Substring("Castellan Belt ".Length);
            }

            // use either the star or planet designation nerfs..

            Dictionary<string, Dictionary<string, string>> desigmap = isstar ? starDesignationMap : planetDesignationMap;

            // if we have a nerf matching the bodyname, use it

            if (desigmap.ContainsKey(system) && desigmap[system].ContainsKey(bodyname))
            {
                return desigmap[system][bodyname];
            }

            return returnnullifnomatch ? null : bodyname;
        }

        internal static void CachePrimaryStar(JournalScan je, ISystem sys)
        {
            string system = sys.Name;

            if (!primaryStarScans.ContainsKey(system))
            {
                primaryStarScans[system] = new List<JournalScan>();
            }

            if (!primaryStarScans[system].Any(s => CompareEpsilon(s.nAge, je.nAge) &&
                                                   CompareEpsilon(s.nEccentricity, je.nEccentricity) &&
                                                   CompareEpsilon(s.nOrbitalInclination, je.nOrbitalInclination) &&
                                                   CompareEpsilon(s.nOrbitalPeriod, je.nOrbitalPeriod) &&
                                                   CompareEpsilon(s.nPeriapsis, je.nPeriapsis) &&
                                                   CompareEpsilon(s.nRadius, je.nRadius) &&
                                                   CompareEpsilon(s.nRotationPeriod, je.nRotationPeriod) &&
                                                   CompareEpsilon(s.nSemiMajorAxis, je.nSemiMajorAxis) &&
                                                   CompareEpsilon(s.nStellarMass, je.nStellarMass)))
            {
                primaryStarScans[system].Add(je);
            }
        }

        private static bool CompareEpsilon(double? a, double? b, bool acceptNull = false, double epsilon = 0.001, Func<double?, double> fb = null)
        {
            if (a == null || b == null)
            {
                return !acceptNull;
            }

            double _a = (double)a;
            double _b = fb == null ? (double)b : fb(b);

            return _a == _b || (_a + _b != 0 && Math.Sign(_a + _b) == Math.Sign(_a) && Math.Abs((_a - _b) / (_a + _b)) < epsilon);
        }


    }
}
