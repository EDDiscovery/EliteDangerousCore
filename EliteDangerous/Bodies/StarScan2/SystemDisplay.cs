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
using System.Drawing;
using System.Windows.Forms;

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemDisplay
    {
        #region Public 

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
        public ContextMenuStrip ContextMenuStripBodies { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripBelts { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripMats { get; set; } = null;      // if to assign a CMS
        public ContextMenuStrip ContextMenuStripSignals { get; set; } = null;      // if to assign a CMS
        public int ValueLimit { get; set; } = 50000;        // which ones get the value star against them
        public Font Font { get; set; } = null;              // must be set before call
        public Font FontUnderlined { get; set; } = null;    // can be set if required.. if not Font is used
        public Font FontLarge { get; set; } = null;         // can be set if required.. if not Font is used
        public Color TextBackColor { get; set; } = Color.Transparent;
        public Color TextForeColor { get; set; } = Color.DarkOrange;
        public Size StarSize { get { return starsize; } }

        // After create, this holds the used display area
        public Point DisplayAreaUsed { get; private set; }

        public SystemDisplay()
        {
        }

        // Clears the imagebox, create the objects, render to imagebox. Accepts systemnode = null to clear the display
        public void DrawSystemRender(ExtendedControls.ExtPictureBox imagebox, int widthavailable, SystemNode systemnode,
                               List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                               string opttext = null, string[] bodytypefilter = null,
                               Point? startpoint = null)
        {
            imagebox.ClearImageList();
            if (systemnode != null)
            {
                var list = CreateSystemImages(widthavailable, systemnode, historicmats, curmats, opttext, bodytypefilter, startpoint);
                imagebox.AddRange(list);
            }
            imagebox.Render();
        }

        // create images of system into an imagebox in widthavailable..  does not clear or render
        // curmats and history may be null
        // bodytype filters is star,body,barycentre,belt or all

        public List<ExtPictureBox.ImageElement> CreateSystemImages( int widthavailable, SystemNode systemnode,
                               List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                               string titletext = null, string[] bodytypefilters = null,
                               Point? startpoint = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);
            System.Diagnostics.Debug.Assert(systemnode != null);
            System.Diagnostics.Debug.Assert(!starsize.IsEmpty);


            // BodyToImages.DebugDisplayStarColourKey(imagebox, Font); enable for checking

            Random rnd = new Random(systemnode.System.Name.GetHashCode());         // always start with same seed so points are in same places

            // this is the draw cursor, its left middle position, place at start point
            Point cursorlm = new Point(startpoint?.X ?? leftmargin, (startpoint?.Y ?? topmargin) + starsize.Height * nodeheightratio / 2 / noderatiodivider);

            DisplayAreaUsed = new Point(0, 0);      // accumulating image size

            List<ExtPictureBox.ImageElement> starcontrols = new List<ExtPictureBox.ImageElement>();

            if (titletext != null)
            {
                ExtPictureBox.ImageElement lab = new ExtPictureBox.ImageElement();
                lab.TextAutoSize(new Point(cursorlm.X, 0), new Size(500, 30), titletext, FontLarge ?? Font, TextForeColor, TextBackColor);
                starcontrols.Add(lab);
                cursorlm.Y += lab.Image.Height + 8;
            }

            DisplayAreaUsed = cursorlm;

            //if ( bodytypefilters!=null ) $"Body filters {string.Join(",",bodytypefilters)}".DO();       

            bool drawnsignals = false;      // set if we drawn signals against any of the stars

            // skip over node ID if its a barycentre and jump down 1 level
            BodyNode toplevelbodieslist = systemnode.TopLevelBody();

            foreach (BodyNode starnode in toplevelbodieslist.ChildBodies)       // go thru top level list..
            {
                if (bodytypefilters != null && starnode.IsBodyTypeInFilter(bodytypefilters, true) == false)       // if filter active, but no body or children in filter
                {
                    System.Diagnostics.Debug.WriteLine($"System Display Rejected {starnode.OwnName}");
                    continue;
                }

                if (!starnode.DoesNodeHaveNonWebScansBelow() && !ShowWebBodies)      // if we don't have any non web bodies at or under the node, and we are not showing web bodies, ignore
                {
                    continue;
                }

                {
                    // draw the star/barycentre, given this body node
                    Point maxpos = DrawNode(starcontrols, starnode, historicmats, curmats,
                                            cursorlm, false, true, out Rectangle starimagepos, out int _, starsize, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxpos.X), Math.Max(DisplayAreaUsed.Y, maxpos.Y));

                    cursorlm = new Point(maxpos.X , cursorlm.Y);        // move on right to edge of this
                }

                // Draw signals (if required), if so move the cursor to the right of the draw
                if (!drawnsignals && (systemnode.FSSSignals?.Count > 0 || systemnode.CodexEntries?.Count > 0 || systemnode.OrbitingStations?.Count>0))
                {
                    drawnsignals = true;
                    Point maxpos = DrawSignals(starcontrols, new Point(cursorlm.X + moonspacerx, cursorlm.Y),
                                                    systemnode.FSSSignals,      // may be null
                                                     systemnode.CodexEntries,      // may be null
                                                     systemnode.OrbitingStations,      // may be null
                                                    starsize.Height * 6 / 4, 16, ContextMenuStripSignals);

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxpos.X), Math.Max(DisplayAreaUsed.Y, maxpos.Y));

                    cursorlm = new Point(maxpos.X + starfirstplanetspacerx, cursorlm.Y);        // move on right
                }
                else
                {
                    cursorlm.X += starfirstplanetspacerx;           // no signals, move the cursor over the spacing
                }

                // if child bodies

                if (starnode.ChildBodies.Count > 0)
                {
                    Point firstcolumn = cursorlm;           // record where the first body cursorlm is in case we need to shift to there

                    HabZones hz = starnode.Scan?.GetHabZones();

                    double habzonestartls = hz != null ? hz.HabitableZoneInner : 0;
                    double habzoneendls = hz != null ? hz.HabitableZoneOuter : 0;

                    foreach(BodyNode planetnode in starnode.ChildBodies)
                    {
                        if (bodytypefilters != null && planetnode.IsBodyTypeInFilter(bodytypefilters, true) == false)       // if filter active, but no body or children in filter
                        {
                            //System.Diagnostics.Debug.WriteLine("SDUC Rejected " + planetnode.fullname);
                            continue;
                        }

                        if (planetnode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                        {
                            List<ExtPictureBox.ImageElement> pc = new List<ExtPictureBox.ImageElement>();

                            bool habzone = false;

                            if (ShowHabZone && planetnode.Scan != null && !planetnode.Scan.IsOrbitingBarycentre && planetnode.Scan.nSemiMajorAxis.HasValue)
                            {
                                double dist = planetnode.Scan.nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m;  // m , converted to LS
                                habzone = dist >= habzonestartls && dist <= habzoneendls;
                            }

                            Point maxplanetmoonspos = CreatePlanetTree(pc, planetnode, historicmats, curmats, cursorlm, bodytypefilters, habzone, out int centreplanet, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                            Point pcnt = new Point(centreplanet, cursorlm.Y);       // centre planet point

                            if (maxplanetmoonspos.X > widthavailable)               // uh ohh too wide..
                            {
                                int xoff = firstcolumn.X - cursorlm.X;              // shift in pixels to firstcolumn.x from the cursor point
                                int yoff = (DisplayAreaUsed.Y + planetspacery) - (cursorlm.Y - planetsize.Height / 2);      // the current display area used, not including this draw, calculate the Y offset to apply

                                RepositionTree(pc, xoff, yoff);                     // shift co-ords of all you've drawn for this part

                                pcnt.X += xoff; pcnt.Y += yoff;                     // need to account for planet centre

                                maxplanetmoonspos = new Point(maxplanetmoonspos.X + xoff, maxplanetmoonspos.Y + yoff);     // add the shift on for the calculation of area used

                                cursorlm = new Point(maxplanetmoonspos.X + planetspacerx, cursorlm.Y + yoff);   // and set the curpos to maxpos.x + spacer, remove the shift from curpos.y
                            }
                            else
                            {
                                cursorlm = new Point(maxplanetmoonspos.X + planetspacerx, cursorlm.Y);     // shift current pos right, plus a spacer, past this planet tree
                            }

                            DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxplanetmoonspos.X), Math.Max(DisplayAreaUsed.Y, maxplanetmoonspos.Y));

                            starcontrols.AddRange(pc.ToArray());
                        }
                    }
                }       // end children

                // if always move down or children.. move down
                if (NoPlanetStarsOnSameLine || starnode.ChildBodies.Count > 0)
                {
                    // cursor back to start, move down below last draw
                    cursorlm = new Point(startpoint?.X ?? leftmargin, DisplayAreaUsed.Y + starplanetgroupspacery + starsize.Height / 2);
                }
                else
                {
                    // cursor move right
                    cursorlm = new Point(DisplayAreaUsed.X + starfirstplanetspacerx, cursorlm.Y);

                    // unless too far right
                    if (cursorlm.X + StarSize.Width > widthavailable) // if too far across..
                    {
                        cursorlm = new Point(startpoint?.X ?? leftmargin, DisplayAreaUsed.Y + starplanetgroupspacery + starsize.Height / 2);
                    }
                }
            }
           

            if (!drawnsignals && (systemnode.FSSSignals?.Count > 0 || systemnode.CodexEntries?.Count > 0))  // if no stars were drawn, but signals..
            {
                lock (gdilock)
                {
                    CreateImageAndLabel(starcontrols, BodyDefinitions.GetImageNotScannedCloned(), cursorlm, starsize, true, out Rectangle starpos, new string[] { "" }, "", ContextMenuStripSignals);

                    Point maxpos = DrawSignals(starcontrols, new Point(starpos.Right + moonspacerx, cursorlm.Y),
                                                    systemnode.FSSSignals, systemnode.CodexEntries, systemnode.OrbitingStations,
                                                    starsize.Height * 6 / 4, 16, ContextMenuStripSignals);       // draw them, nothing else to follow

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxpos.X), Math.Max(DisplayAreaUsed.Y, maxpos.Y));
                }
            }

            return starcontrols;
        }

        public void DrawSingleObject(ExtendedControls.ExtPictureBox imagebox, StarScan2.BodyNode node,
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                                       Point? startpoint = null)
        {
            imagebox.ClearImageList();  // does not clear the image, render will do that
            var ie = CreateSingleObject(node, historicmats, curmats, startpoint);
            imagebox.AddRange(ie);
            imagebox.Render();
        }

        // draw a single object to the imagebox
        public List<ExtPictureBox.ImageElement> CreateSingleObject(BodyNode bn,
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                                       Point? startpoint = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);

            Point leftmiddle = new Point(startpoint?.X ?? leftmargin, (startpoint?.Y ?? topmargin) + StarSize.Height * nodeheightratio / 2 / noderatiodivider);

            List<ExtPictureBox.ImageElement> ie = new List<ExtPictureBox.ImageElement>();

            Random rnd = new Random(bn.Name().GetHashCode());         // always start with same seed so points are in same places

            DrawNode(ie, bn, historicmats, curmats, leftmiddle, false, true, out Rectangle _, out int _, starsize, rnd, null, null);

            return ie;
        }

        public void SetSize(int stars)
        {
            starsize = new Size(stars, stars);
            planetsize = new Size(starsize.Width * 3 / 4, starsize.Height * 3 / 4);
            moonsize = new Size(starsize.Width * 2 / 4, starsize.Height * 2 / 4);
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

        #region Implementation

        private void RepositionTree(List<ExtPictureBox.ImageElement> pc, int xoff, int yoff)
        {
            foreach (ExtPictureBox.ImageElement c in pc)
            {
                c.Translate(xoff, yoff);
            }
        }


        private Size starsize;                   // size of stars
        private Size planetsize, moonsize, materialsize;
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

        #endregion


    }
}

