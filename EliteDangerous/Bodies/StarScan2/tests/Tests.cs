/*
 * Copyright 2025 - 2025 EDDiscovery development team
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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EliteDangerousCore.StarScan2
{
    public static class Tests
    {
        static public void TestAlign()
        {
            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                        new BodyParent(BodyParent.BodyType.Planet, 2),
                                                                        new BodyParent(BodyParent.BodyType.Null, 1),
                                                                        new BodyParent(BodyParent.BodyType.Star, 0),
                                                                        },
                                                                        "10 b", out List<string> pl); //"Prieluia QI-Q c19-31 10 b"
                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Star") && pl[1] == "Unknown Barycentre" && pl[2].Contains("10") && pl[3] == "b");
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                            new BodyParent(BodyParent.BodyType.Star, 3),
                                                                            new BodyParent(BodyParent.BodyType.Null, 0),
                                                                            },
                                                                            "AB 1 b", out List<string> pl);
                System.Diagnostics.Debug.Assert(pl[0].Contains("AB") && pl[1] == "1" && pl[2] == "b");  // Skaude AA-A h294 AB 1 a
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                            new BodyParent(BodyParent.BodyType.Null, 1),
                                                                            new BodyParent(BodyParent.BodyType.Star, 0),
                                                                            new BodyParent(BodyParent.BodyType.Null, 1),
                                                                            },
                                                                            "A 1", out List<string> pl);         // HIP 1885 A 1
                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Bary") && pl[1] == "A" && pl[2].Contains("Unknown Bary") && pl[3] == "1");
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                        new BodyParent(BodyParent.BodyType.Null, 1),
                                                                        },
                                                                            "A", out List<string> pl);           // HIP 1885 A
                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Bary") && pl[1] == "A");
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                        new BodyParent(BodyParent.BodyType.Ring, 7),
                                                                        new BodyParent(BodyParent.BodyType.Star, 1),
                                                                        },
                                                                        "B Belt Cluster 4", out List<string> pl);    // Scheau Prao ME-M c22-21 B Belt Cluster 4

                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Star") && pl[1] == "B Belt Cluster" && pl[2] == "4");
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                        new BodyParent(BodyParent.BodyType.Ring, 7),
                                                                        new BodyParent(BodyParent.BodyType.Null, 3),
                                                                        new BodyParent(BodyParent.BodyType.Star, 1),
                                                                        new BodyParent(BodyParent.BodyType.Null, 0),
                                                                        },
                                                                        "B Belt Cluster 4", out List<string> pl);
                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Bary") && pl[1].Contains("Unknown Star") && pl[2] == "Unknown Barycentre" && pl[3] == "B Belt Cluster" && pl[4] == "4");
            }

            {
                EliteDangerousCore.StarScan2.SystemNode.AlignParentsName(new System.Collections.Generic.List<BodyParent> {
                                                                        new BodyParent(BodyParent.BodyType.Null, 2),
                                                                        new BodyParent(BodyParent.BodyType.Star, 1),
                                                                        new BodyParent(BodyParent.BodyType.Null, 0)}, "1", out List<string> pl);
                System.Diagnostics.Debug.Assert(pl[0].Contains("Unknown Bary") && pl[1].Contains("Unknown Star") && pl[2] == "Unknown Barycentre" && pl[3] == "1");
            }
        }

        static public void TestScans()
        {
            string outputdir = @"c:\code\AA";
            string path = @"c:\code\eddiscovery\elitedangerouscore\elitedangerous\bodies\starscan2\tests";
            EliteDangerousCore.StarScan2.StarScan.ProcessAllFromDirectory(path, "*.json", (ss2, mhs) =>
                    {
                        EliteDangerousCore.StarScan2.SystemNode sssol = ss2.FindSystemSynchronous(mhs.Last().Item2.System);
                        sssol.DrawSystemToFolder(1920, outputdir, 0);
                    });

        }

        static public void TestScan(string system, string folder, string pictureoutfolder)
        {
            string file = Path.Combine(folder, $"{system}.json");
            EliteDangerousCore.StarScan2.StarScan ss = new EliteDangerousCore.StarScan2.StarScan();
            DebuggerHelpers.OutputControl += "StarScan";

            uint gen = 1817272;
            var hist = HistoryEntry.CreateFromFile(file);
            ss.ProcessFromHistory(hist, (ss2, mhe) =>
            {
                ISystem syst = ss2.FindISystem(system);
                if (syst != null)
                {
                    EliteDangerousCore.StarScan2.SystemNode sssol = ss2.FindSystemSynchronous(syst);

                    if (sssol.BodyGeneration != gen)
                    {
                        gen = sssol.BodyGeneration;
                        sssol.DrawSystemToFolder(1920, pictureoutfolder, mhe.Item1);
                    }
                }
            });
            ss.DumpTree();
            ss.AssignPending();

            //{ 
            //    ISystem syst = ss.FindISystem(system);
            //    EliteDangerousCore.StarScan2.SystemNode sss = ss.FindSystemSynchronous(syst);
            //    var bt = sss.BodiesNoBarycentres(sss.systemBodies,null);
            //    sss.Dump(bt, "", 0);
            //}

        }
    }
}