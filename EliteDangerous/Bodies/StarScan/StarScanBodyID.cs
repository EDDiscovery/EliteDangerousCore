/*
 * Copyright © 2015 - 2023 EDDiscovery development team
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
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class StarScan
    {
        public bool AddBodyToBestSystem(IBodyNameAndID je, ISystem sys, int startindex, List<HistoryEntry> hl)
        {
            bool isstar = je.BodyType == BodyDefinitions.BodyType.Star;

            //  may have a system address.  If it matches current system, we can go for an immediate add
            if (je.SystemAddress == sys.SystemAddress)
            {
                // fix up body designation (nov 24 added it was missing)
                je.BodyDesignation = BodyDesignations.GetBodyDesignation(je.Body, je.BodyID, isstar, sys.Name);
                return ProcessBodyAndID(je, sys);
            }
            else
            {
                // look thru history to find best system match and return body designation and system
                var best = FindBestSystemAndName(startindex, hl, je.SystemAddress, je.Body, je.BodyID, isstar);

                if (best == null)
                {
                    System.Diagnostics.Debug.WriteLine($"AddBodyToBestSystem Failed Best System {je.Body} {je.BodyID} {je.BodyType}");
                    return false;
                }

                je.BodyDesignation = best.Item1;
                return ProcessBodyAndID(je, best.Item2);
            }
        }

        private bool ProcessBodyAndID(IBodyNameAndID sc, ISystem sys)  // background or foreground.. 
        {
            SystemNode systemnode = GetOrCreateSystemNode(sys);

            lock (systemnode)
            {
                ScanNode scannode = null;

                // try and find the scan related to the body name

                if (!sc.BodyID.HasValue || systemnode.NodesByID.TryGetValue(sc.BodyID.Value, out scannode) == false)     // if no ID, or can't find by ID
                {
                    // no designation, or same designation AND not a star
                    if ((sc.BodyDesignation == null || sc.BodyDesignation == sc.Body) && (sc.Body != sc.StarSystem || sc.BodyType != BodyDefinitions.BodyType.Star))
                    {
                        foreach (var body in systemnode.Bodies())
                        {
                            if ((body.BodyDesignator == sc.Body || body.BodyName == sc.Body) &&
                                (body.BodyDesignator != sc.StarSystem || (sc.BodyType == BodyDefinitions.BodyType.Star && body.Level == 0) || (sc.BodyType != BodyDefinitions.BodyType.Star && body.Level != 0)))
                            {
                                scannode = body;
                                sc.BodyDesignation = body.BodyDesignator;
                                break;
                            }
                        }
                    }

                    // else just try and find any matches
                    if (scannode == null)
                    {
                        foreach (var body in systemnode.Bodies())
                        {
                            if ((body.BodyDesignator == sc.Body || body.BodyName == sc.Body) &&
                                (body.BodyDesignator != sc.StarSystem || (sc.BodyType == BodyDefinitions.BodyType.Star && body.Level == 0) || (sc.BodyType != BodyDefinitions.BodyType.Star && body.Level != 0)))
                            {
                                scannode = body;
                                break;
                            }
                        }
                    }
                }

                if (scannode != null)   // We already have the node - don't need another one
                {
                    if (scannode.BodyID == null && sc.BodyID.HasValue)        // we may have made it before node IDs were introduced,so make sure its there
                    {
                        scannode.BodyID = sc.BodyID;
                        systemnode.NodesByID[sc.BodyID.Value] = scannode;
                        //System.Diagnostics.Debug.WriteLine($"ProcessBodyAndID found existing scan node and updated BID {sys.Name} {sc.Body} bid {sc.BodyID} vs bid {scannode.BodyID} {scannode.CustomNameOrOwnname}");
                    }
                    else
                    {
                        // System.Diagnostics.Debug.WriteLine($"ProcessBodyAndID found existing scan node for {sys.Name} {sc.Body} bid {sc.BodyID} vs bid {scannode.BodyID} {scannode.CustomNameOrOwnname}");
                    }

                    return true;
                }

                // System.Diagnostics.Debug.WriteLine($"AddBodyToBestSystem new body {sc.Body} {sc.BodyID} {sc.BodyType}");

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
                ScanNode node = ProcessElementsBodyAndID(sc, sys, systemnode, customname, elements, starscannodetype, isbeltcluster);

                if (node.BodyID != null)
                {
                    systemnode.NodesByID[(int)node.BodyID] = node;
                }
            }

            ProcessedSaved();  // any saved JEs due to no scan, add

            return true;
        }

        private List<string> ExtractElementsBodyAndID(IBodyNameAndID sc, ISystem sys, out bool isbeltcluster, out ScanNodeType starscannodetype)
        {
            starscannodetype = ScanNodeType.toplevelstar;
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

        private ScanNode ProcessElementsBodyAndID(IBodyNameAndID sc, ISystem sys, SystemNode systemnode, string customname, List<string> elements, ScanNodeType starscannodetype, bool isbeltcluster)
        {
            SortedList<string, ScanNode> currentnodelist = systemnode.StarNodes;                // current operating node list, always made
            ScanNode previousnode = null;                                               // trails subnode by 1 to point to previous node

            for (int lvl = 0; lvl < elements.Count; lvl++)
            {
                ScanNodeType sublvtype = starscannodetype;

                if (lvl > 0)
                {
                    if (isbeltcluster)
                        sublvtype = lvl == 1 ? ScanNodeType.belt : ScanNodeType.beltcluster;
                    else
                        sublvtype = ScanNodeType.planetmoonsubstar;
                }

                if (currentnodelist == null || !currentnodelist.TryGetValue(elements[lvl], out ScanNode subnode))
                {
                    if (currentnodelist == null)    // no node list, happens when we are at least 1 level down as systemnode always has a node list, make one 
                        currentnodelist = previousnode.MakeChildList();

                    string ownname = elements[lvl];

                    subnode = new ScanNode(ownname,
                                            lvl == 0 ? (sys.Name + (ownname.Contains("Main") ? "" : (" " + ownname))) : previousnode.BodyDesignator + " " + ownname,
                                            sublvtype,
                                            lvl,
                                            previousnode,
                                            systemnode,
                                            SystemSource.Synthesised);

                    currentnodelist.Add(ownname, subnode);
                   // System.Diagnostics.Debug.WriteLine($"StarScan BID Created subnode des:`{subnode.BodyDesignator}` on:`{subnode.OwnName}` bn:`{subnode.BodyName}`");
                }
                else
                {
                    //  System.Diagnostics.Debug.WriteLine($"StarScan BID Existing subnode {subnode.FullName}");
                }

                if (lvl == elements.Count - 1)
                {
                    subnode.BodyName = customname;

                    if (sc.BodyID != null)
                    {
                        subnode.BodyID = sc.BodyID;
                    }

                    if (sc.BodyType == BodyDefinitions.BodyType.Unknown || sc.BodyType == BodyDefinitions.BodyType.Barycentre)
                        subnode.NodeType = ScanNodeType.barycentre;
                    else if (sc.BodyType == BodyDefinitions.BodyType.Ring)
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

                if (customname == "" && sc.BodyType != BodyDefinitions.BodyType.Star)
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
