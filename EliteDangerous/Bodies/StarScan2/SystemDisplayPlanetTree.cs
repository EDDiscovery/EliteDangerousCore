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

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemDisplay
    {

        // return right bottom of area used
        private Point CreatePlanetTree(List<ExtPictureBox.ImageElement> pc, BodyNode planetnode,
                                        List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats,
                                         Point leftmiddle, string[] filter, bool habzone, out int planetcentrex, Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            // System.Diagnostics.Debug.WriteLine($"DrawPlanetTree {planetnode.OwnName} at leftmiddle {leftmiddle}");

            // draw the object, given left/middle position, and allow right shift for label growth

            Point maxtreepos = DrawNode(pc, planetnode, historicmats, curmats,
                                leftmiddle, false, true, out Rectangle imagerect, out planetcentrex, planetsize, rnd, rightclickplanet, rightclickmats, backwash: habzone ? Color.FromArgb(64, 0, 128, 0) : default(Color?));        // offset passes in the suggested offset, returns the centre offset

            // we output the planetcentrx, which we use to base the moon positions

            Point moonpos = new Point(planetcentrex, maxtreepos.Y + moonspacery + moonsize.Height / 2);     

            // draw primary moons under the planet centred with no right shift allowed

            maxtreepos = DrawTree(pc,planetnode, moonpos, true, false, maxtreepos,  historicmats,curmats,filter,rnd,rightclickplanet,rightclickmats);

            return maxtreepos;
        }

        // Draw a tree.
        // pos is either left/middle or centre/middle, dependent on xiscentre
        // shiftrightifreq can be set to shift image right if text exceeds pos.X
        // update maxtreepos and return it

        private Point DrawTree(List<ExtPictureBox.ImageElement> pc, BodyNode parent, Point pos, bool xiscentre, bool shiftrightifreq, Point maxtreepos,
                               List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats, string[] filter,
                                Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
            List<BodyNode> bodiestodisplay = parent.ChildBodies.Where(s => s.BodyType == BodyNode.BodyClass.PlanetMoon || s.BodyType == BodyNode.BodyClass.Star || s.BodyType == BodyNode.BodyClass.Barycentre || s.BodyType == BodyNode.BodyClass.BeltClusterBody).ToList();

            if (bodiestodisplay.Count > 0 && ShowMoons)
            {
                for (int mn = 0; mn < bodiestodisplay.Count; mn++)         
                {
                    BodyNode moonnode = bodiestodisplay[mn];

                    if (filter != null && moonnode.IsBodyTypeInFilter(filter, true) == false)       // if filter active, but no body or children in filter
                        continue;

                    bool nonwebscans = moonnode.DoesNodeHaveNonWebScansBelow();                     // is there any scans here, either at this node or below?

                    if (nonwebscans || ShowWebBodies)
                    {
                        // draw moon controlled by caller for positioning.
                        // the first moon under a planet will be xcentre=true rightshift=false
                        // the second moon will be xcentre=true rightshift=false
                        // the first submoon under a moon will be xcentre=false, rightshift=true
                        // the second submoon under a moon will be xcentre=true, rightshift=false
                        // its all very complicated and took Robby a while to remember!

                        Point mmax = DrawNode(pc, moonnode, historicmats, curmats,  pos, xiscentre, shiftrightifreq, out Rectangle moonimagepos, out int mooncentrex, moonsize, rnd, rightclickplanet, rightclickmats);

                        maxtreepos = new Point(Math.Max(maxtreepos.X, mmax.X), Math.Max(maxtreepos.Y, mmax.Y));

                        Point submoonpos = new Point(mmax.X + moonspacerx, pos.Y);      // left/middle, same level
                        // we draw the submoon tree with centrex off but allowing shift right to account for label
                        maxtreepos = DrawTree(pc, moonnode, submoonpos, false, true, maxtreepos, historicmats, curmats, filter, rnd, rightclickplanet, rightclickmats);

                        // now, if xiscentre is on, we just move down, if its off, we use the output mooncentrex and turn xiscentre on and shiftright off
                        if (xiscentre)
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

            return maxtreepos;
        }
    }
}
