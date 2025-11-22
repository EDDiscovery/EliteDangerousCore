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

using BaseUtils;
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
        // Draw the node and submoons. return image list of tree. Top parent body is at pos
        // level 0 means draw under, level 1 to the right, level 2+ is to the right with right shift on

        private ExtendedControls.ImageElement.List DrawTree(NodePtr node, Point parentpos, int displaylevel,  
                                                 double habzonestartls, double habzoneendls,
                                                 List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats, string[] bodytypefilters,
                                                 Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            bool habzone = false;

            if (ShowHabZone && node.BodyNode.Scan != null && !node.BodyNode.Scan.IsOrbitingBarycentre && node.BodyNode.Scan.nSemiMajorAxis.HasValue)
            {
                double dist = node.BodyNode.Scan.nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m;  // m , converted to LS
                habzone = dist >= habzonestartls && dist <= habzoneendls;
            }


            var imagelist = DrawNode(node.BodyNode, historicmats, curmats,
                                        planetsize, rnd, ContextMenuStripBodies, ContextMenuStripMats,
                                        //backwash: Color.FromArgb(64, 0, 255, 0));
                                        backwash: habzone ? Color.FromArgb(64, 0, 128, 0) : default(Color?));

            // if we shift down, calc amount, which is image size + moonsize to centre

            int shiftdownamount = imagelist.Size.Height + moonspacery + moonsize.Height / 2;            

            //string pad = new string(' ', displaylevel % 100 + (displaylevel >= 200 ? 4 : 0));
            //System.Diagnostics.Debug.WriteLine($"{pad} {displaylevel} DrawNode of {node.BodyNode.Name()} {node.BodyNode.BodyType} at {parentpos} shiftdown {shiftdownamount}");

            imagelist.Tag = new Tuple<BodyNode,int>(node.BodyNode,shiftdownamount);

            if (ShowMoons)
            {
                // the 100 mod is just for sanity in debugging so when the debug strings are turned on you can get a nice debug output
                // go back to level xx0 painting if barycentre so it paints under the barycentre icon
                int sublevel = node.BodyNode.IsBarycentre ? (displaylevel/100*100+100) + 0: displaylevel + 1;      

                // pick the line for the bodies, level 0 is directly below (child of top parent) and the rest are right of the image.
                // Note this is a left point, the shiftright below ensures shifting away for levels 2+
                Point subbodypos = (displaylevel%100) >= 1 && !node.BodyNode.IsBarycentre ? new Point(imagelist.Max.X + moonspacerx, 0) :   // right
                                                                                            new Point(0, shiftdownamount);      // below

                List<NodePtr> bodiestodisplay = node.ChildBodies.Where(b => b.BodyNode.BodyType != BodyDefinitions.BodyType.PlanetaryRing &&
                                                                             (bodytypefilters == null || b.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == true) &&
                                                                             (b.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                                                                        ).ToList();

                if (bodiestodisplay.Count > 0)
                {
                   // System.Diagnostics.Debug.WriteLine($"{pad} {displaylevel} Draw moons of {node.BodyNode.Name()} starting at {subbodypos}");

                    for (int i = 0; i < bodiestodisplay.Count; i++)
                    {
                        var moon = bodiestodisplay[i];

                        if (i > 0 && node.BodyNode.IsBarycentre)          // put a -x on top of second further barys of children
                        {
                            var img = BodyDefinitions.GetImageBarycentreLeftBar().CloneLocked();
                            var barymarker2 = CreateImageAndLabel(img, moonsize, null, "");
                            // shift the image at 0,0 to the same centre as the bary1 first image (which is the bary marker image) then shift upwards the image
                            barymarker2.Shift(subbodypos);
                            barymarker2.Shift(new Point(0, -shiftdownamount));
                            imagelist.AddRange(barymarker2);
                        }

                        var moonimages = DrawTree(moon, subbodypos, sublevel, 0, 0, historicmats, curmats, bodytypefilters, rnd, rightclickplanet, rightclickmats);
                        imagelist.AddRange(moonimages);        // add moon images in to produce the overall W and H of this tree

                        // barycentres moons move right
                        if (node.BodyNode.IsBarycentre)
                        {
                            subbodypos = new Point(subbodypos.X + moonimages.Size.Width + moonspacerx, subbodypos.Y);
                        }
                        else
                        {
                            // else move down, clearing over the image pixels.  
                            subbodypos = new Point(subbodypos.X, moonimages.Max.Y + moonspacery + moonsize.Height);
                        }
                    }

                   // System.Diagnostics.Debug.WriteLine($"{pad} {displaylevel} END Draw moons of {node.BodyNode.Name()}");
                }

            }

            if ((displaylevel%100) >= 2 )        // objects under 1 get shifted right in total to make sure they don't hit our image
            {
                imagelist.ShiftRight();         // we are still in 0,0 land with the parent at this position
            }

            imagelist.Shift(parentpos);         // now shift into parent pos requested

          //  System.Diagnostics.Debug.WriteLine($"{pad} {displaylevel} EndNode {node.BodyNode.Name()} at {parentpos} Sz {imagelist.Size}  {imagelist.Min} .. {imagelist.Max} ");
            return imagelist;
        }

    }
}
