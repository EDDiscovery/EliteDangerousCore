/*
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
using System.Linq;


namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public bool AddBodyToBestSystem(IBodyNameAndID je, int startindex, List<HistoryEntry> hl)
        {
            // reject types which are not scannable

            if (je.Body == null || je.BodyType == "Station" || je.BodyType == "StellarRing" || je.BodyType == "PlanetaryRing" || je.BodyType == "SmallBody")
            {
                return false;
            }

            var best = FindBestSystem(startindex, hl, je.Body, je.BodyID, je.BodyType == "Star");

            if (best == null)
                return false;

            je.BodyDesignation = best.Item1;

            return ProcessBodyAndID(je, best.Item2);         
        }

        private bool ProcessBodyAndID(IBodyNameAndID sc, ISystem sys)  // background or foreground.. 
        {
            SystemNode sn = GetOrCreateSystemNode(sys);
            ScanNode relatedScan = null;

            // try and find the scan related to the body name

            // no designation, or same designation AND not a star
            if ((sc.BodyDesignation == null || sc.BodyDesignation == sc.Body) && (sc.Body != sc.StarSystem || sc.BodyType != "Star"))
            {
                foreach (var body in sn.Bodies)
                {
                    if ((body.FullName == sc.Body || body.CustomName == sc.Body) &&
                        (body.FullName != sc.StarSystem || (sc.BodyType == "Star" && body.Level == 0) || (sc.BodyType != "Star" && body.Level != 0)))
                    {
                        relatedScan = body;
                        sc.BodyDesignation = body.FullName;
                        break;
                    }
                }
            }

            // else just try and find any matches
            if (relatedScan == null)
            {
                foreach (var body in sn.Bodies)
                {
                    if ((body.FullName == sc.Body || body.CustomName == sc.Body) &&
                        (body.FullName != sc.StarSystem || (sc.BodyType == "Star" && body.Level == 0) || (sc.BodyType != "Star" && body.Level != 0)))
                    {
                        relatedScan = body;
                        break;
                    }
                }
            }

            if (relatedScan != null && relatedScan.ScanData == null)
            {
                return true; // We already have the node, with no scan, so reject
            }

            // Extract elements from name
            List<string> elements = ExtractElementsBodyAndID(sc, sys, out bool isbeltcluster, out ScanNodeType starscannodetype);

            // Bail out if no elements extracted
            if (elements.Count == 0)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to add body {sc.Body} to system {sys.Name} - not enough elements");
                return false;
            }
            // Bail out if more than 5 elements extracted
            else if (elements.Count > 5)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to add body {sc.Body} to system {sys.Name} - too deep");
                return false;
            }

            // Get custom name if different to designation
            string customname = GetCustomNameBodyAndID(sc, sys);

            // Process elements
            ScanNode node = ProcessElementsBodyAndID(sc, sys, sn, customname, elements, starscannodetype, isbeltcluster);

            if (node.BodyID != null)
            {
                sn.NodesByID[(int)node.BodyID] = node;
            }

            ProcessedSaved();  // any saved JEs due to no scan, add

            return true;
        }

        private List<string> ExtractElementsBodyAndID(IBodyNameAndID sc, ISystem sys, out bool isbeltcluster, out ScanNodeType starscannodetype)
        {
            starscannodetype = ScanNodeType.star;
            isbeltcluster = false;
            List<string> elements;

            string rest = IsStarNameRelatedReturnRest(sys.Name, sc.Body, sc.BodyDesignation);   // extract any relationship between the system we are in and the name, and return it if so

            if (rest != null)                                   // if we have a relationship..
            {
                if (rest.Length > 0)
                {
                    elements = rest.Split(' ').ToList();

                    if (elements.Count == 4 && elements[0].Length == 1 && char.IsLetter(elements[0][0]) &&           // A belt cluster N
                            elements[1].Equals("belt", StringComparison.InvariantCultureIgnoreCase) &&
                            elements[2].Equals("cluster", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elements = new List<string> { MainStar, elements[0] + " " + elements[1], elements[2] + " " + elements[3] }; // reform into Main Star | A belt | Cluster N
                        isbeltcluster = true;
                    }
                    else if (elements.Count == 5 && elements[0].Length >= 1 &&                                      // AA A belt cluster N
                            elements[1].Length == 1 && char.IsLetter(elements[1][0]) &&
                            elements[2].Equals("belt", StringComparison.InvariantCultureIgnoreCase) &&
                            elements[3].Equals("cluster", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elements = new List<string> { elements[0], elements[1] + " " + elements[2], elements[3] + " " + elements[4] };      // reform into <star> | A belt | Cluster N
                        isbeltcluster = true;
                    }

                    if (char.IsDigit(elements[0][0]))       // if digits, planet number, no star designator
                        elements.Insert(0, MainStar);       // no star designator, main star, add MAIN
                    else if (elements[0].Length > 1)        // designator, is it multiple chars.. 
                        starscannodetype = ScanNodeType.barycentre;
                }
                else
                {
                    elements = new List<string>();          // only 1 item, the star, which is the same as the system name..
                    elements.Add(MainStar);              // Sol / SN:Sol should come thru here
                }
            }
            else
            {                                               // so not part of starname        
                elements = sc.Body.Split(' ').ToList();     // not related in any way (earth) so assume all bodyparts, and 
                elements.Insert(0, MainStar);               // insert the MAIN designator as the star designator
            }

            return elements;
        }

        private ScanNode ProcessElementsBodyAndID(IBodyNameAndID sc, ISystem sys, SystemNode sn, string customname, List<string> elements, ScanNodeType starscannodetype, bool isbeltcluster)
        {
            SortedList<string, ScanNode> currentnodelist = sn.StarNodes;                // current operating node list, always made
            ScanNode previousnode = null;                                               // trails subnode by 1 to point to previous node

            for (int lvl = 0; lvl < elements.Count; lvl++)
            {
                ScanNodeType sublvtype = starscannodetype;

                if (lvl > 0)
                {
                    if (isbeltcluster)
                        sublvtype = lvl == 1 ? ScanNodeType.belt : ScanNodeType.beltcluster;
                    else
                        sublvtype = ScanNodeType.body;
                }

                if (currentnodelist == null || !currentnodelist.TryGetValue(elements[lvl], out ScanNode subnode))
                {
                    if (currentnodelist == null)    // no node list, happens when we are at least 1 level down as systemnode always has a node list, make one 
                        currentnodelist = previousnode.Children = new SortedList<string, ScanNode>(new CollectionStaticHelpers.BasicLengthBasedNumberComparitor<string>());

                    string ownname = elements[lvl];
                    
                    subnode = new ScanNode
                    {
                        OwnName = ownname,
                        FullName = lvl == 0 ? (sys.Name + (ownname.Contains("Main") ? "" : (" " + ownname))) : previousnode.FullName + " " + ownname,
                        ScanData = null,
                        Children = null,
                        NodeType = sublvtype,
                        Level = lvl,
                        Parent = previousnode,
                    };

                    currentnodelist.Add(ownname, subnode);
                }

                if (lvl == elements.Count - 1)
                {
                    subnode.CustomName = customname;

                    if (sc.BodyID != null)
                    {
                        subnode.BodyID = sc.BodyID;
                    }

                    if (sc.BodyType == "" || sc.BodyType == "Null" || sc.BodyType == "Barycentre")
                        subnode.NodeType = ScanNodeType.barycentre;
                    else if (sc.BodyType == "Belt")
                        subnode.NodeType = ScanNodeType.belt;
                }

                previousnode = subnode;
                currentnodelist = previousnode.Children;

            }

            return previousnode;
        }

        private string GetCustomNameBodyAndID(IBodyNameAndID sc, ISystem sys)
        {
            string rest = IsStarNameRelatedReturnRest(sys.Name, sc.Body, sc.BodyDesignation);
            string customname = null;

            if (sc.Body.StartsWith(sys.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                customname = sc.Body.Substring(sys.Name.Length).TrimStart(' ', '-');

                if (customname == "" && sc.BodyType != "Star")
                {
                    customname = sc.Body;
                }
                else if (customname == "" || customname == rest)
                {
                    customname = null;
                }
            }
            else if (rest == null || !sc.Body.EndsWith(rest))
            {
                customname = sc.Body;
            }

            return customname;
        }

    }
}
