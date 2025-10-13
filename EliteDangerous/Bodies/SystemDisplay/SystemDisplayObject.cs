/*
 * Copyright © 2019-2023 EDDiscovery development team
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

using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static EliteDangerousCore.StarScan;

namespace EliteDangerousCore
{
    public partial class SystemDisplay 
    {
        // Clears the imagebox, create the objects, render to imagebox
        public void DrawSingleObject(ExtendedControls.ExtPictureBox imagebox, StarScan.SystemNode systemnode, StarScan.ScanNode node,
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                                       Point? startpoint = null)
        {
            imagebox.ClearImageList();  // does not clear the image, render will do that
            CreateSingleObject(imagebox, systemnode, node, historicmats, curmats,startpoint);
            imagebox.Render();
        }

        // draw a single object to the imagebox
        public void CreateSingleObject(ExtendedControls.ExtPictureBox imagebox, StarScan.SystemNode systemnode, StarScan.ScanNode node,
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                                       Point? startpoint = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);

            Point leftmiddle = new Point(startpoint?.X ?? leftmargin, (startpoint?.Y ?? topmargin) + StarSize.Height * nodeheightratio / 2 / noderatiodivider);

            List<ExtPictureBox.ImageElement> starcontrols = new List<ExtPictureBox.ImageElement>();

            Image notscannedbitmap = BodyDefinitions.GetStarImageNotScanned();
            Image barycentre = BodyDefinitions.GetBarycentreImage();

            Random rnd = new Random(node.BodyDesignator.GetHashCode());         // always start with same seed so points are in same places

            if (node.NodeType == StarScan.ScanNodeType.toplevelstar || node.NodeType == StarScan.ScanNodeType.barycentre)
            {
                DisplayAreaUsed = DrawNode(starcontrols, node, historicmats, curmats,
                        (node.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre : notscannedbitmap,
                        leftmiddle, false, true, out Rectangle starimagepos, out int _, StarSize, DrawLevel.TopLevelStar, rnd, ContextMenuStripStars, ContextMenuStripMats);

                if (systemnode.FSSSignalList.Count > 0 || systemnode.CodexEntryList.Count > 0)
                {
                    Point maxsignalpos = DrawSignals(starcontrols, new Point(starimagepos.Right + moonspacerx, leftmiddle.Y),
                                                    systemnode.FSSSignalList, systemnode.CodexEntryList,
                                                    StarSize.Height * 6 / 4, 16, ContextMenuStripSignals);

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxsignalpos.X), Math.Max(DisplayAreaUsed.Y, maxsignalpos.Y));
                }
            }
            else if (node.NodeType == StarScan.ScanNodeType.planetmoonsubstar )
            {
                DisplayAreaUsed = DrawNode(starcontrols, node, historicmats, curmats,
                                    (node.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre : BodyDefinitions.GetPlanetImageNotScanned(),
                                    leftmiddle, false, true, out Rectangle imagerect, out int planetcentrex, planetsize,
                                    node.Parent.NodeType == ScanNodeType.toplevelstar ? DrawLevel.PlanetLevel : DrawLevel.MoonLevel, rnd, 
                                    ContextMenuStripPlanetsMoons, ContextMenuStripMats
                                    );
            }
            else if (node.NodeType == ScanNodeType.belt)
            {
                Image beltsi = BodyDefinitions.GetBeltImage();

                DisplayAreaUsed = DrawNode(starcontrols, node, historicmats, curmats, beltsi, leftmiddle, false, true, out Rectangle _, out int _,
                                beltsize, DrawLevel.PlanetLevel, rnd, ContextMenuStripBelts, null);
            }

            imagebox.AddRange(starcontrols);
        }
    }
}

