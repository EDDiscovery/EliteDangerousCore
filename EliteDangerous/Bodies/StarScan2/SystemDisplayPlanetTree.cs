/*
 * Copyright 2025-2025 EDDiscovery development team
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

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemDisplay
    {

        // draw the planet, then the tree below. Planet is at 0,0, tree below and right
        private ExtPictureBox.ImageList DrawPlanetAndTree(NodePtr toplevelnode, double habzonestartls, double habzoneendls, 
                                                        List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats, string[] filter,
                                                        Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            bool habzone = false;

            if (ShowHabZone && toplevelnode.BodyNode.Scan != null && !toplevelnode.BodyNode.Scan.IsOrbitingBarycentre && toplevelnode.BodyNode.Scan.nSemiMajorAxis.HasValue)
            {
                double dist = toplevelnode.BodyNode.Scan.nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m;  // m , converted to LS
                habzone = dist >= habzonestartls && dist <= habzoneendls;
            }

            // Draw the planet node, centred at 0,0.

            var planetnode = DrawNode(toplevelnode.BodyNode, historicmats, curmats,
                                        planetsize, rnd, ContextMenuStripBodies, ContextMenuStripMats,
                                        //backwash: Color.FromArgb(64, 0, 255, 0));
                                        backwash: habzone ? Color.FromArgb(64, 0, 128, 0) : default(Color?));

            //System.Diagnostics.Debug.WriteLine($"  planetnode node {planetnode.Size} {planetnode.Min} .. {planetnode.Max}");

            // Draw the tree under the planet node at 0, planet node max + spacing
            Point subbodypos = new Point(0, planetnode.Max.Y + moonspacery + moonsize.Height / 2);
            var treeimages = DrawTree(toplevelnode, subbodypos, false, historicmats, curmats, filter, rnd, ContextMenuStripBodies, ContextMenuStripMats);
            planetnode.AddRange(treeimages);

            return planetnode;
        }


        // Draw a tree, return image list of tree. Top child body is at pos, others further down
        private ExtPictureBox.ImageList DrawTree(NodePtr parent, 
                                                 Point pos, bool shiftrightifreq,   // shift to pos, and maybe shift right to ensure no pixels left of pos.X
                                                 List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats, string[] filter,
                                                 Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            List<NodePtr> bodiestodisplay = parent.ChildBodies.Where(s => s.BodyNode.BodyType != BodyDefinitions.BodyType.PlanetaryRing).ToList();

            Point maxours = pos;

            ExtPictureBox.ImageList imageList = new ExtPictureBox.ImageList();

            if (bodiestodisplay.Count > 0 && ShowMoons)
            {
                for (int mn = 0; mn < bodiestodisplay.Count; mn++)
                {
                    NodePtr moonnode = bodiestodisplay[mn];

                    if (filter == null || moonnode.BodyNode.IsBodyTypeInFilter(filter, true) == true)       // if filter active, and active or children active in filter
                    {
                        bool nonwebscans = moonnode.BodyNode.DoesNodeHaveNonWebScansBelow();                // is there any scans here, either at this node or below?

                        if (nonwebscans || ShowWebBodies)
                        {
                            //System.Diagnostics.Debug.WriteLine($"Draw {moonnode.BodyNode.Name()} at {pos}");

                            // draw the node, at 0,0. 
                            var nodeimages = DrawNode(moonnode.BodyNode, historicmats, curmats, moonsize, rnd, rightclickplanet, rightclickmats);

                            if (shiftrightifreq)                                    // if ensuring we never have negative pixels
                                nodeimages.Shift(new Point(-nodeimages.Min.X, 0));  // shift by minimum X right, do it before positioning

                            nodeimages.Shift(pos);                                  // move to pos

                            if (moonnode.ChildBodies.Count > 0) 
                            {
                                Point submoonpos = new Point(nodeimages.Max.X + moonspacerx, pos.Y);      // same y, but to the right of this image

                                // draw moon images to the right, with shifting right to ensure it does not come back into us
                                var moonimages = DrawTree(moonnode, submoonpos, true, historicmats, curmats, filter, rnd, rightclickplanet, rightclickmats);

                                nodeimages.AddRange(moonimages);        // add moon images in to produce the overall W and H of this tree
                            }

                            pos = new Point(pos.X, pos.Y + nodeimages.Size.Height + moonspacery);

                            imageList.AddRange(nodeimages);
                        }
                    }
                }
            }

            return imageList;
        }
    }
}
