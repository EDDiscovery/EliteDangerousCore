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
        //// Draw a tree.
        //// update maxtreepos and return it

        private Point DrawTree(List<ExtPictureBox.ImageElement> pc, BodyNode parent, Point pos, 
                               bool xiscentre, bool shiftrightifreq, bool moveright,
                               Point maxtreepos,
                               List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats, string[] filter,
                               Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            List<BodyNode> bodiestodisplay = parent.ChildBodies.Where(s => s.BodyType != BodyDefinitions.BodyType.PlanetaryRing).ToList();

            if (bodiestodisplay.Count > 0 && ShowMoons)
            {
                for (int mn = 0; mn < bodiestodisplay.Count; mn++)
                {
                    BodyNode moonnode = bodiestodisplay[mn];

                    if (filter == null || moonnode.IsBodyTypeInFilter(filter, true) == true)       // if filter active, and active or children active in filter
                    {
                        bool nonwebscans = moonnode.DoesNodeHaveNonWebScansBelow();                // is there any scans here, either at this node or below?

                        if (nonwebscans || ShowWebBodies)
                        {
                            // draw moon controlled by caller for positioning.
                            // the first moon under a planet will be xcentre=true rightshift=false
                            // the second moon will be xcentre=true rightshift=false
                            // the first submoon under a moon will be xcentre=false, rightshift=true
                            // the second submoon under a moon will be xcentre=true, rightshift=false
                            // if moveright then we always move right at this level - tweak used for top level planet barycentres, which means these are planets . We use planetsize below if so
                            // its all very complicated and took Robby a while to remember!

                            Point mmax = DrawNode(pc, moonnode, historicmats, curmats, pos, xiscentre, shiftrightifreq, out Rectangle moonimagepos, out int mooncentrex, 
                                            moveright && moonnode.BodyType != BodyDefinitions.BodyType.Barycentre ? planetsize : moonsize, 
                                            rnd, rightclickplanet, rightclickmats);

                            maxtreepos = new Point(Math.Max(maxtreepos.X, mmax.X), Math.Max(maxtreepos.Y, mmax.Y));

                            Point submoonpos = new Point(mmax.X + moonspacerx, pos.Y + planetsize.Height/2);      // same y, but to the right of this image and 1/2 down

                            // we draw the submoon tree with centrex off but allowing shift right to account for label
                            maxtreepos = DrawTree(pc, moonnode, submoonpos, xiscentre: false, shiftrightifreq: true, moveright:false, maxtreepos, historicmats, curmats, filter, rnd, rightclickplanet, rightclickmats);

                            // now, if xiscentre is on, we just move down, if its off, we use the output mooncentrex and turn xiscentre on and shiftright off
                            if (moveright)
                            {
                                pos = new Point(maxtreepos.X + moonspacerx, pos.Y);
                                shiftrightifreq = true;
                                xiscentre = false;
                            }
                            else if (xiscentre)
                            {
                                pos = new Point(pos.X, maxtreepos.Y + moonspacery + moonsize.Height / 2);
                            }
                            else
                            {
                                pos = new Point(mooncentrex, maxtreepos.Y + moonspacery + moonsize.Height / 2);
                                xiscentre = true;       // now go to centre placing
                                shiftrightifreq = false;
                            }
                        }
                    }
                }
            }

            return maxtreepos;
        }
    }
}
