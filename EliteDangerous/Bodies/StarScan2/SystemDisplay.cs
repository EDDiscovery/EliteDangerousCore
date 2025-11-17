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
using System.Linq;
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
        public bool SimplifyDiagram { get; set; } = true;   
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
                               string opttext = null, string[] bodytypefilter = null
                              )
        {
            imagebox.ClearImageList();
            if (systemnode != null)
            {
                var list = CreateSystemImages(systemnode, new Point(0,0), widthavailable, historicmats, curmats, opttext, bodytypefilter);
                imagebox.AddRange(list);
            }
            imagebox.Render();
        }


















        // create images of system into an imagebox in widthavailable..  does not clear or render
        // curmats and history may be null
        // bodytype filters is star,body,barycentre,belt or all

        public List<ExtPictureBox.ImageElement> CreateSystemImages(SystemNode systemnode, Point startpoint, int widthavailable,
                               List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                               string titletext = null, 
                               string[] bodytypefilters = null
                               )
        {
            System.Diagnostics.Debug.Assert(Font != null);
            System.Diagnostics.Debug.Assert(systemnode != null);
            System.Diagnostics.Debug.Assert(!starsize.IsEmpty);

            // BodyToImages.DebugDisplayStarColourKey(imagebox, Font); enable for checking

            Random rnd = new Random(systemnode.System.Name.GetHashCode());         // always start with same seed so points are in same places

            // this is the draw cursor, its left middle position, place at start point
            Point cursorlm = new Point(startpoint.X, startpoint.Y + starsize.Height * nodeheightratio / 2 / noderatiodivider);

            ExtPictureBox.ImageList starcontrols = new ExtPictureBox.ImageList();

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

            // get node list, thru the preprocessor of BodiesSimplified

            NodePtr toplevelbodieslist = systemnode.BodiesSimplified(SimplifyDiagram);

            ExtPictureBox.ImageList imageline = new ExtPictureBox.ImageList();

            foreach (NodePtr starnode in toplevelbodieslist.ChildBodies)       // go thru top level list..
            {
                if (bodytypefilters != null && starnode.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == false)       // if filter active, but no body or children in filter
                {
                    System.Diagnostics.Debug.WriteLine($"System Display Rejected {starnode.BodyNode.OwnName}");
                    continue;
                }

                if (!starnode.BodyNode.DoesNodeHaveNonWebScansBelow() && !ShowWebBodies)      // if we don't have any non web bodies at or under the node, and we are not showing web bodies, ignore
                {
                    continue;
                }

                {
                    // draw the star/barycentre, given this body node
                    Point maxpos = DrawNode(imageline, starnode.BodyNode, historicmats, curmats,
                                            cursorlm, false, true, out Rectangle starimagepos, out int _, starsize, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                    DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxpos.X), Math.Max(DisplayAreaUsed.Y, maxpos.Y));

                    cursorlm = new Point(maxpos.X , cursorlm.Y);        // move on right to edge of this
                }

                // Draw signals (if required), if so move the cursor to the right of the draw
                if (!drawnsignals && (systemnode.FSSSignals?.Count > 0 || systemnode.CodexEntries?.Count > 0 || systemnode.OrbitingStations?.Count>0))
                {
                    drawnsignals = true;
                    Point maxpos = DrawSignals(imageline, new Point(cursorlm.X + moonspacerx, cursorlm.Y),
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

                if (starnode.BodyNode.ChildBodies.Count > 0)
                {
                    Point firstcolumn = cursorlm;           // record where the first body cursorlm is in case we need to shift to there

                    HabZones hz = starnode.BodyNode.Scan?.GetHabZones();

                    double habzonestartls = hz != null ? hz.HabitableZoneInner : 0;
                    double habzoneendls = hz != null ? hz.HabitableZoneOuter : 0;

                    foreach(NodePtr toplevelnode in starnode.ChildBodies)
                    {
                        if (bodytypefilters != null && toplevelnode.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == false)       // if filter active, but no body or children in filter
                        {
                            //System.Diagnostics.Debug.WriteLine("SDUC Rejected " + planetnode.fullname);
                            continue;
                        }

                        if (toplevelnode.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                        {
                            ExtPictureBox.ImageList nodeimages = new ExtPictureBox.ImageList();

                            bool habzone = false;

                            if (ShowHabZone && toplevelnode.BodyNode.Scan != null && !toplevelnode.BodyNode.Scan.IsOrbitingBarycentre && toplevelnode.BodyNode.Scan.nSemiMajorAxis.HasValue)
                            {
                                double dist = toplevelnode.BodyNode.Scan.nSemiMajorAxis.Value / BodyPhysicalConstants.oneLS_m;  // m , converted to LS
                                habzone = dist >= habzonestartls && dist <= habzoneendls;
                            }

                            // we draw at drawpos the node
                            // we output the node x, which we use to base the sub body positions

                            Point maxnodepos = DrawNode(nodeimages, toplevelnode.BodyNode, historicmats, curmats,
                                                    cursorlm, false, true, out Rectangle imagerect, out int centretoplevelnodex, planetsize, rnd, ContextMenuStripBodies, ContextMenuStripMats, backwash: habzone ? Color.FromArgb(64, 0, 128, 0) : default(Color?));        // offset passes in the suggested offset, returns the centre offset

                            // place subbodies under the centre of the node, spaced down

                            Point subbodypos = new Point(centretoplevelnodex, maxnodepos.Y + moonspacery + moonsize.Height / 2);

                            // draw primary moons under the planet centred with no right shift allowed

                            Point maxtreepos = maxnodepos;
                        //    DrawTree(nodeimages, toplevelnode, subbodypos, true, false, toplevelnode.BodyNode.BodyType == BodyDefinitions.BodyType.Barycentre, ref maxtreepos, historicmats, curmats, bodytypefilters, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                            //if (toplevelnode.BodyNode.IsBarycentre)
                            //{
                            //    if (!linehasbarycentre)
                            //    {
                            //        ExtPictureBox.Reposition(imageline, 0, nodeimagesize.Height);      // shift all we had drawn so far down
                            //        linehasbarycentre = true;
                            //    }
                            //}
                            //else if (linehasbarycentre)
                            //{
                            //    ExtPictureBox.Reposition(nodeimages, 0, nodeimagesize.Height);      // shift all we had down
                            //}

                            if (maxtreepos.X > widthavailable)               // uh ohh too wide..
                            {
                                int xoff = firstcolumn.X - cursorlm.X;              // shift in pixels to firstcolumn.x from the cursor point
                                int yoff = (DisplayAreaUsed.Y + planetspacery) - (cursorlm.Y - planetsize.Height / 2);      // the current display area used, not including this draw, calculate the Y offset to apply

                                nodeimages.Reposition(xoff, yoff);                     // shift co-ords of all you've drawn for this part

                                maxtreepos = new Point(maxtreepos.X + xoff, maxtreepos.Y + yoff);     // add the shift on for the calculation of area used

                                cursorlm = new Point(maxtreepos.X + planetspacerx, cursorlm.Y + yoff);   // and set the curpos to maxpos.x + spacer, remove the shift from curpos.y

                                starcontrols.AddRange(imageline.Enumerable);       // add previous line
                                imageline.Clear();                      // clear and start new line
                                imageline.AddRange(nodeimages.Enumerable);
                            }
                            else
                            {
                                cursorlm = new Point(maxtreepos.X + planetspacerx, cursorlm.Y);     // shift current pos right, plus a spacer, past this planet tree
                                imageline.AddRange(nodeimages.Enumerable);
                            }

                            DisplayAreaUsed = new Point(Math.Max(DisplayAreaUsed.X, maxtreepos.X), Math.Max(DisplayAreaUsed.Y, maxtreepos.Y));

                        }
                    }
                }       // end children

                // cursor move right as a default..
                cursorlm = new Point(DisplayAreaUsed.X + starfirstplanetspacerx, cursorlm.Y);

                // new star group upcoming..
                // if always move down or children was drawn or too wide .. move down
                if ( NoPlanetStarsOnSameLine || starnode.BodyNode.ChildBodies.Count > 0 || cursorlm.X + StarSize.Width > widthavailable)
                {
                    // cursor back to start, move down below last draw
                    cursorlm = new Point(startpoint.X, DisplayAreaUsed.Y + starplanetgroupspacery + starsize.Height / 2);

                    starcontrols.AddRange(imageline.Enumerable);       // add previous line
                    imageline.Clear();                      // clear and start new line
                }
            }

            starcontrols.AddRange(imageline.Enumerable);

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

            return starcontrols.Enumerable.ToList();
        }

        public void DrawSingleObject(ExtendedControls.ExtPictureBox imagebox, StarScan2.BodyNode node, Point startpoint,
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null)
                                        
        {
            imagebox.ClearImageList();  // does not clear the image, render will do that
            var ie = CreateSingleObject(node, startpoint, historicmats, curmats);
            imagebox.AddRange(ie);
            imagebox.Render();
        }

        // draw a single object to the imagebox
        public List<ExtPictureBox.ImageElement> CreateSingleObject(BodyNode bn, Point startpoint, 
                                       List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null)
                                        
        {
            System.Diagnostics.Debug.Assert(Font != null);

            Point cursorlm = new Point(startpoint.X, startpoint.Y + StarSize.Height * nodeheightratio / 2 / noderatiodivider);

            ExtPictureBox.ImageList ie = new ExtPictureBox.ImageList();

            Random rnd = new Random(bn.Name().GetHashCode());         // always start with same seed so points are in same places

            DrawNode(ie, bn, historicmats, curmats, cursorlm, false, true, out Rectangle _, out int _, starsize, rnd, null, null);

            return ie.Enumerable.ToList();
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
            planetspacery = 40;
            moonspacerx = Math.Min(stars / 4, 8);
            moonspacery = Math.Min(stars / 4, 8);
            materiallinespacerxy = 4;
        }


        #endregion

        #region Implementation


        private Size starsize,planetsize, moonsize, materialsize;
        private int starfirstplanetspacerx;        // distance between star and first planet
        private int starplanetgroupspacery;        // distance between each star/planet grouping 
        private int planetspacerx;       // distance between each planet in a row
        private int planetspacery;       // distance between each planet row
        private int moonspacerx;        // distance to move moon across
        private int moonspacery;        // distance to slide down moon vs planet
        private int materiallinespacerxy;   // extra distance to add around material output

        const int noderatiodivider = 8;     // in eighth sizes
        const int nodeheightratio = 12;     // nominal size 12/8th of Size

        #endregion


    }
}

