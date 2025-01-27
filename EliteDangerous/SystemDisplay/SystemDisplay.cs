﻿/*
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
        public bool ShowWebBodies { get; set; }
        public bool ShowMoons { get; set; } = true;
        public bool ShowOverlays { get; set; } = true;
        public bool ShowMaterials { get; set; } = true;
        public bool ShowOnlyMaterialsRare { get; set; } = false;
        public bool HideFullMaterials { get; set; } = false;
        public bool ShowAllG { get; set; } = true;
        public bool ShowPlanetMass { get; set; } = true;
        public bool ShowStarMass { get; set; } = true;
        public bool ShowStarAge { get; set; } = true;
        public bool ShowHabZone { get; set; } = true;
        public bool ShowPlanetClasses { get; set; } = true;
        public bool ShowStarClasses { get; set; } = true;
        public bool ShowDist { get; set; } = true;
        public bool NoPlanetStarsOnSameLine { get; set; } = true;
        public ContextMenuStrip ContextMenuStripStars { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripPlanetsMoons { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripBelts { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripMats { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripSignals { get; set; } = null;      // if to assign a CMS

        public int ValueLimit { get; set; } = 50000;

        public Point DisplayAreaUsed { get; private set; }  // used area to display in
        public Size StarSize { get; private set; }  // size of stars

        public Font Font { get; set; } = null;              // these must be set before call
        public Font FontUnderlined { get; set; } = null;
        public Font LargerFont { get; set; } = null;
        public Color BackColor { get; set; } = Color.Black;
        public Color LabelColor { get; set; } = Color.DarkOrange;


        private Size beltsize, planetsize, moonsize, materialsize;
        private int starfirstplanetspacerx;        // distance between star and first planet
        private int starplanetgroupspacery;        // distance between each star/planet grouping 
        private int planetspacerx;       // distance between each planet in a row
        private int planetspacery;       // distance between each planet row
        private int moonspacerx;        // distance to move moon across
        private int moonspacery;        // distance to slide down moon vs planet
        private int materiallinespacerxy;   // extra distance to add around material output
        private int leftmargin;
        private int topmargin;

        const int noderatiodivider = 8;     // in eighth sizes
        const int nodeheightratio = 12;     // nominal size 12/8th of Size

        public SystemDisplay()
        {
        }
	
        #region Display

        // draw scannode into an imagebox in widthavailable..
        // curmats may be null
        public void DrawSystem(ExtendedControls.ExtPictureBox imagebox, int widthavailable,
                               StarScan.SystemNode systemnode, List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats,string opttext = null, string[] filter=  null ) 
        {
            imagebox.ClearImageList();  // does not clear the image, render will do that

           // BodyToImages.DebugDisplayStarColourKey(imagebox, Font); enable for checking
            
            if (systemnode != null)
            {
                Random rnd = new Random(systemnode.System.Name.GetHashCode());         // always start with same seed so points are in same places

                var notscannedbitmap = BodyToImages.GetStarImageNotScanned();

                Point leftmiddle = new Point(leftmargin, topmargin + StarSize.Height * nodeheightratio / 2 / noderatiodivider);  // half down (h/2 * ratio)

                if ( opttext != null )
                {
                    ExtPictureBox.ImageElement lab = new ExtPictureBox.ImageElement();
                    lab.TextAutoSize(new Point(leftmargin,0), new Size(500, 30), opttext, LargerFont, LabelColor, BackColor);
                    imagebox.Add(lab);
                    leftmiddle.Y += lab.Image.Height + 8;
                }

                DisplayAreaUsed = leftmiddle;
                List<ExtPictureBox.ImageElement> starcontrols = new List<ExtPictureBox.ImageElement>();

                bool displaybelts = filter == null || (filter.Contains("belt") || filter.Contains("All"));

                Point maxitemspos = new Point(0, 0);

                bool drawnsignals = false;      // set if we drawn signals against any of the stars

                foreach (StarScan.ScanNode starnode in systemnode.StarNodes.Values)        // always has scan nodes
                {
                    if (filter != null && starnode.IsBodyInFilter(filter, true) == false)       // if filter active, but no body or children in filter
                    {
                       // System.Diagnostics.Debug.WriteLine("SDUC Rejected " + starnode.fullname);
                        continue;
                    }

                    if (!starnode.DoesNodeHaveNonWebScansBelow() && !ShowWebBodies)      // if we don't have any non edsm bodies at or under the node, and we are not showing edsm bodies, ignore
                    {
                        continue;
                    }

                    {  // Draw star
                        Image barycentre =BodyToImages.GetBarycentreImage();

                        Point maxpos = DrawNode(starcontrols, starnode, historicmats, curmats,
                                (starnode.NodeType == StarScan.ScanNodeType.barycentre) ? barycentre: notscannedbitmap,
                                leftmiddle, false, true, out Rectangle starimagepos, out int _, StarSize, DrawLevel.TopLevelStar, rnd, ContextMenuStripStars, ContextMenuStripMats);       // the last part nerfs the label down to the right position

                        maxitemspos = new Point(Math.Max(maxitemspos.X, maxpos.X), Math.Max(maxitemspos.Y, maxpos.Y));

                        if (!drawnsignals && (systemnode.FSSSignalList.Count > 0 || systemnode.CodexEntryList.Count>0))           // Draw signals, if not drawn
                        {
                            drawnsignals = true;
                            Point maxsignalpos = DrawSignals(starcontrols, new Point(starimagepos.Right + moonspacerx, leftmiddle.Y), 
                                                            systemnode.FSSSignalList, systemnode.CodexEntryList, 
                                                            StarSize.Height * 6 / 4, 16, ContextMenuStripSignals);
                            maxitemspos = new Point(Math.Max(maxitemspos.X, maxsignalpos.X), Math.Max(maxitemspos.Y, maxsignalpos.Y));
                        }

                        leftmiddle = new Point(maxitemspos.X + starfirstplanetspacerx, leftmiddle.Y);       // move the cursor on to the right of the box, no spacing
                    }

                    if (starnode.Children != null)
                    {
                        Point firstcolumn = leftmiddle;

                        Queue<StarScan.ScanNode> belts;
                        if (starnode.ScanData != null && (!starnode.ScanData.IsWebSourced || ShowWebBodies))  // have scandata on star, and its not edsm or allowed edsm
                        {
                            belts = new Queue<StarScan.ScanNode>(starnode.Children.Values.Where(s => s.NodeType == StarScan.ScanNodeType.belt));    // find belts in children of star
                        }
                        else
                        {
                            belts = new Queue<StarScan.ScanNode>(); // empty array
                        }

                        StarScan.ScanNode lastbelt = belts.Count != 0 ? belts.Dequeue() : null;

                        EliteDangerousCore.JournalEvents.JournalScan.HabZones hz = starnode.ScanData?.GetHabZones();

                        double habzonestartls = hz != null ? hz.HabitableZoneInner : 0;
                        double habzoneendls = hz != null ? hz.HabitableZoneOuter : 0;

                        Image beltsi = BodyToImages.GetBeltImage();

                        // process body and stars only

                        List<StarScan.ScanNode> planetsinorder = starnode.Children.Values.Where(s => s.NodeType == StarScan.ScanNodeType.body || s.NodeType == StarScan.ScanNodeType.star).ToList();
                        var planetcentres = new Dictionary<StarScan.ScanNode, Point>();

                        for (int pn = 0; pn < planetsinorder.Count; pn++)
                        {
                            StarScan.ScanNode planetnode = planetsinorder[pn];

                            if (filter != null && planetnode.IsBodyInFilter(filter, true) == false)       // if filter active, but no body or children in filter
                            {
                                //System.Diagnostics.Debug.WriteLine("SDUC Rejected " + planetnode.fullname);
                                continue;
                            }

                            //System.Diagnostics.Debug.WriteLine("Draw " + planetnode.ownname + ":" + planetnode.type);

                            // if belt is before this, display belts here

                            while (displaybelts && lastbelt != null && planetnode.ScanData != null && (lastbelt.BeltData == null || planetnode.ScanData.IsOrbitingBarycentre || lastbelt.BeltData.OuterRad < planetnode.ScanData.nSemiMajorAxis))
                            {
                                // if too far across, go back to star
                                if (leftmiddle.X + planetsize.Width > widthavailable) // if too far across..
                                {
                                    leftmiddle = new Point(firstcolumn.X, maxitemspos.Y + planetspacery + planetsize.Height / 2); // move to left at maxy+space+h/2
                                }

                                string appendlabel = "";

                                if (lastbelt.BeltData != null)
                                {
                                    appendlabel = appendlabel.AppendPrePad($"{lastbelt.BeltData.OuterRad / BodyPhysicalConstants.oneLS_m:N1}ls", Environment.NewLine);
                                }

                                appendlabel = appendlabel.AppendPrePad("" + lastbelt.ScanData?.BodyID, Environment.NewLine);


                                Point maxbeltpos = DrawNode(starcontrols, lastbelt, historicmats, curmats, beltsi, leftmiddle,false, true, out Rectangle _,  out int _,
                                                beltsize, DrawLevel.PlanetLevel, rnd, ContextMenuStripBelts, null, appendlabeltext:appendlabel);

                                leftmiddle = new Point(maxbeltpos.X + planetspacerx, leftmiddle.Y);
                                lastbelt = belts.Count != 0 ? belts.Dequeue() : null;

                                maxitemspos = new Point(Math.Max(maxitemspos.X, maxbeltpos.X), Math.Max(maxitemspos.Y, maxbeltpos.Y));
                            }

                           //System.Diagnostics.Debug.WriteLine("Planet Node " + planetnode.ownname + " has scans " + nonedsmscans);

                            if (planetnode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                            {
                                List<ExtPictureBox.ImageElement> pc = new List<ExtPictureBox.ImageElement>();

                                bool habzone = false;

                                if (ShowHabZone && planetnode.ScanData != null && !planetnode.ScanData.IsOrbitingBarycentre && planetnode.ScanData.nSemiMajorAxis.HasValue)
                                {
                                    double dist = planetnode.ScanData.nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m;  // m , converted to LS
                                    habzone =  dist >= habzonestartls && dist <= habzoneendls;
                                }

                                Point maxplanetpos = CreatePlanetTree(pc, planetnode, historicmats, curmats, leftmiddle, filter, habzone , out int centreplanet, rnd, ContextMenuStripPlanetsMoons, ContextMenuStripMats);

                                Point pcnt = new Point(centreplanet, leftmiddle.Y);

                                if (maxplanetpos.X > widthavailable)          // uh oh too wide..
                                {
                                    int xoff = firstcolumn.X - leftmiddle.X;                     // shift to firstcolumn.x, maxitemspos.Y+planetspacer
                                    int yoff = (maxitemspos.Y+planetspacery) - (leftmiddle.Y-planetsize.Height/2);

                                    RepositionTree(pc, xoff, yoff);        // shift co-ords of all you've drawn - this will include any bary points drawn in moons

                                    pcnt.X += xoff; pcnt.Y += yoff; // need to account for planet centre

                                    maxplanetpos = new Point(maxplanetpos.X + xoff, maxplanetpos.Y + yoff);     // remove the shift from maxpos

                                    leftmiddle = new Point(maxplanetpos.X + planetspacerx, leftmiddle.Y + yoff);   // and set the curpos to maxpos.x + spacer, remove the shift from curpos.y
                                }
                                else
                                    leftmiddle = new Point(maxplanetpos.X + planetspacerx, leftmiddle.Y);     // shift current pos right, plus a spacer

                                maxitemspos = new Point(Math.Max(maxitemspos.X, maxplanetpos.X), Math.Max(maxitemspos.Y, maxplanetpos.Y));

                                starcontrols.AddRange(pc.ToArray());

                                planetcentres[planetnode] = pcnt;
                            }
                        }

                        // do any futher belts after all planets

                        while (displaybelts && lastbelt != null)
                        {
                            if (leftmiddle.X + planetsize.Width > widthavailable) // if too far across..
                            {
                                leftmiddle = new Point(firstcolumn.X, maxitemspos.Y + planetspacery + planetsize.Height / 2); // move to left at maxy+space+h/2
                            }

                            string appendlabel = "";

                            if (lastbelt.BeltData != null)
                            {
                                appendlabel = appendlabel.AppendPrePad($"{lastbelt.BeltData.OuterRad / BodyPhysicalConstants.oneLS_m:N1}ls", Environment.NewLine);
                            }

                            appendlabel = appendlabel.AppendPrePad("" + lastbelt.ScanData?.BodyID, Environment.NewLine);

                            Point maxbeltpos = DrawNode(starcontrols, lastbelt, historicmats, curmats, beltsi, leftmiddle, false, true, out Rectangle _, out int _,
                                        beltsize, DrawLevel.PlanetLevel, rnd, ContextMenuStripBelts,null, appendlabeltext: appendlabel);

                            leftmiddle = new Point(maxbeltpos.X + planetspacerx, leftmiddle.Y);
                            lastbelt = belts.Count != 0 ? belts.Dequeue() : null;

                            maxitemspos = new Point(Math.Max(maxitemspos.X, maxbeltpos.X), Math.Max(maxitemspos.Y, maxbeltpos.Y));
                        }

                        maxitemspos = leftmiddle = new Point(leftmargin, maxitemspos.Y + starplanetgroupspacery + StarSize.Height / 2);     // move back to left margin and move down to next position of star, allowing gap

                        // make a tree of the planets with their barycentres from the Parents information
                        var barynodes = StarScan.ScanNode.PopulateBarycentres(planetsinorder);  // children always made, barynode tree

                        //StarScan.ScanNode.DumpTree(barynodes, "TOP", 0);

                        List<ExtPictureBox.ImageElement> pcb = new List<ExtPictureBox.ImageElement>();

                        foreach (var k in barynodes.Children)   // for all barynodes.. display
                        {
                            DisplayBarynode(k.Value, 0, planetcentres, planetsinorder, pcb, planetsize.Height / 2);     // done after the reposition so true positions set up.
                        }

                        starcontrols.InsertRange(0,pcb); // insert at start so drawn under
                    }
                    else
                    {
                        if (NoPlanetStarsOnSameLine)     // no planets, config what to do
                        {
                            maxitemspos = leftmiddle = new Point(leftmargin, maxitemspos.Y + starplanetgroupspacery + StarSize.Height / 2); // move to left at maxy+space+h/2
                        }
                        else
                        {
                            leftmiddle = new Point(maxitemspos.X + starfirstplanetspacerx, leftmiddle.Y);

                            if (leftmiddle.X + StarSize.Width > widthavailable) // if too far across..
                            {
                                maxitemspos = leftmiddle = new Point(leftmargin, maxitemspos.Y + starplanetgroupspacery + StarSize.Height / 2); // move to left at maxy+space+h/2
                            }
                        }
                    }

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxitemspos.X), Math.Max(DisplayAreaUsed.Y, maxitemspos.Y));

                }

                if (!drawnsignals && (systemnode.FSSSignalList.Count > 0 || systemnode.CodexEntryList.Count > 0))  // if no stars were drawn, but signals..
                {
                    Point maxpos = CreateImageAndLabel(starcontrols, notscannedbitmap, leftmiddle, StarSize, true, out Rectangle starpos, new string[] { "" }, "", ContextMenuStripSignals, false);
                    DrawSignals(starcontrols, new Point(starpos.Right + moonspacerx, leftmiddle.Y), 
                                                        systemnode.FSSSignalList, systemnode.CodexEntryList,
                                                        StarSize.Height * 6 / 4, 16, ContextMenuStripSignals);       // draw them, nothing else to follow
                }

                imagebox.AddRange(starcontrols);
            }

            imagebox.Render();      // replaces image..
        }

        void RepositionTree(List<ExtPictureBox.ImageElement> pc, int xoff, int yoff)
        {
            foreach (ExtPictureBox.ImageElement c in pc)
            {
                c.Translate(xoff, yoff);

                var joinlist = c.Tag as List<BaryPointInfo>;        // barypoints need adjusting too
                if (joinlist != null)
                {
                    foreach (var p in joinlist)
                    {
                        p.point = new Point(p.point.X + xoff, p.point.Y + yoff);       
                        p.toppos = new Point(p.toppos.X + xoff, p.toppos.Y + yoff);
                    }
                }
            }
        }

        public void SetSize(int stars)
        {
            StarSize = new Size(stars, stars);
            beltsize = new Size(StarSize.Width * 1 / 2, StarSize.Height);
            planetsize = new Size(StarSize.Width * 3 / 4, StarSize.Height * 3 / 4);
            moonsize = new Size(StarSize.Width * 2 / 4, StarSize.Height * 2 / 4);
            int matsize = stars >= 64 ? 24 : 16;
            materialsize = new Size(matsize, matsize);

            starfirstplanetspacerx = Math.Min(stars / 2, 16);      // 16/2=8 to 16
            starplanetgroupspacery = Math.Min(stars / 2, 24);      // 16/2=8 to 24
            planetspacerx = Math.Min(stars / 4, 16);
            topmargin = planetspacery = 40;     // enough space for a decent number of barycentres
            moonspacerx = Math.Min(stars / 4, 8);
            moonspacery = Math.Min(stars / 4, 8);
            leftmargin = 8;
            materiallinespacerxy = 4;
        }

        #endregion

    }
}

