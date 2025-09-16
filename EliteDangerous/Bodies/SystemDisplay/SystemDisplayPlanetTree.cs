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

namespace EliteDangerousCore
{
    public partial class SystemDisplay
    {

        // return right bottom of area used from curpos
        Point CreatePlanetTree(List<ExtPictureBox.ImageElement> pc, StarScan.ScanNode planetnode,
                                        List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats,
                                         Point leftmiddle, string[] filter, bool habzone, out int planetcentrex, Random rnd, ContextMenuStrip rightclickplanet, ContextMenuStrip rightclickmats)
        {
           // System.Diagnostics.Debug.WriteLine($"DrawPlanetTree {planetnode.OwnName} at leftmiddle {leftmiddle}");

            Color? backwash = null;
            if (habzone)
                backwash = Color.FromArgb(64, 0, 128, 0);       // transparent in case we have a non black background

            Image barycentre = BodyDefinitions.GetBarycentreImage();

            // draw the object, allow right shift

            Point maxtreepos = DrawNode(pc, planetnode, historicmats, curmats,
                                (planetnode.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre : BodyDefinitions.GetPlanetImageNotScanned(),
                                leftmiddle, false, true, out Rectangle imagerect, out planetcentrex, planetsize, DrawLevel.PlanetLevel, rnd, rightclickplanet, rightclickmats, backwash: backwash);        // offset passes in the suggested offset, returns the centre offset

            if (planetnode.Children != null && ShowMoons)
            {
                // moon pos, below planet, centre x coord based on above DrawNode report of the planet centre x

                Point moonposcentremid = new Point(planetcentrex, maxtreepos.Y + moonspacery + moonsize.Height / 2);    
               
                //System.Diagnostics.Debug.WriteLine($"..DrawMoontree centremid {moonposcentremid} imagerect of planet was {imagerect}");

                var moonnodes = planetnode.Children.Values.Where(n => n.NodeType != StarScan.ScanNodeType.barycentre).ToList();
                var mooncentres = new Dictionary<StarScan.ScanNode, Point>();

                for (int mn = 0; mn < moonnodes.Count; mn++)
                {
                    StarScan.ScanNode moonnode = moonnodes[mn];

                    if (filter != null && moonnode.IsBodyInFilter(filter, true) == false)       // if filter active, but no body or children in filter
                        continue;

                    bool nonedsmscans = moonnode.DoesNodeHaveNonWebScansBelow();     // is there any scans here, either at this node or below?

                    if (nonedsmscans || ShowWebBodies)
                    {
                        // draw moons, as they are underneath the planet, disable right shift

                        Point mmax = DrawNode(pc, moonnode, historicmats, curmats,
                                (moonnode.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre : BodyDefinitions.GetMoonImageNotScanned(),
                                moonposcentremid, true, false, out Rectangle moonimagepos, out int mooncentrex, moonsize, DrawLevel.MoonLevel, rnd, rightclickplanet, rightclickmats);

                        maxtreepos = new Point(Math.Max(maxtreepos.X, mmax.X), Math.Max(maxtreepos.Y, mmax.Y));

                        if (moonnode.Children != null)
                        {
                            Point submoonpos = new Point(mmax.X + moonspacerx, moonposcentremid.Y);     // first its left mid
                            bool xiscentre = false;

                            foreach (StarScan.ScanNode submoonnode in moonnode.Children.Values)
                            {
                                if (filter != null && submoonnode.IsBodyInFilter(filter, true) == false)       // if filter active, but no body or children in filter
                                    continue;

                                bool nonedsmsubmoonscans = submoonnode.DoesNodeHaveNonWebScansBelow();     // is there any scans here, either at this node or below?

                                if (nonedsmsubmoonscans || ShowWebBodies)
                                {
                                    // draw submoon, allow right shift if xiscentre is false, as its the first submoon, then disallow right shift

                                    Point sbmax = DrawNode(pc, submoonnode, historicmats, curmats,
                                            (moonnode.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre : BodyDefinitions.GetMoonImageNotScanned(),
                                            submoonpos, xiscentre, xiscentre == false, out Rectangle submoonimagepos, out int xsubmooncentre, moonsize, DrawLevel.MoonLevel, rnd, rightclickplanet, rightclickmats);

                                    if (xiscentre)
                                    {
                                        submoonpos = new Point(submoonpos.X, sbmax.Y + moonspacery + moonsize.Height / 2);
                                    }
                                    else
                                    {
                                        submoonpos = new Point(xsubmooncentre, sbmax.Y + moonspacery + moonsize.Height / 2);
                                        xiscentre = true;       // now go to centre placing
                                    }

                                    maxtreepos = new Point(Math.Max(maxtreepos.X, sbmax.X), Math.Max(maxtreepos.Y, sbmax.Y));
                                }
                            }

                        }

                        mooncentres[moonnode] = new Point(mooncentrex, moonposcentremid.Y);

                        moonposcentremid = new Point(moonposcentremid.X, maxtreepos.Y + moonspacery + moonsize.Height / 2);

                        //System.Diagnostics.Debug.WriteLine("Next moon centre at " + moonposcentremid );
                    }
                }

                //foreach (var n in moonnodes) StarScan.ScanNode.DumpTree(n, "MB", 0);

                //// now, taking the moon modes, create a barycentre tree with those inserted in 
                var barynodes = StarScan.ScanNode.PopulateBarycentres(moonnodes);  // children always made, barynode tree

                //foreach (var n in moonnodes) StarScan.ScanNode.DumpTree(n, "MA", 0);

                foreach (var k in barynodes.Children)   // for all barynodes.. display
                {
                    DisplayBarynode(k.Value, 0, mooncentres, moonnodes, pc, moonsize.Width * 5 / 4, true);
                }
            }

            return maxtreepos;
        }

    }
}

