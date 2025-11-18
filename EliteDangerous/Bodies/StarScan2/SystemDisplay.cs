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

        public ExtPictureBox.ImageList CreateSystemImages(SystemNode systemnode, Point startpoint, int widthavailable,
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

            NodePtr toplevelbodieslist = systemnode.BodiesSimplified(SimplifyDiagram);

            // filter off ones not to display
            var toplevelbodiestodisplay = toplevelbodieslist.ChildBodies.Where(b => (bodytypefilters == null || b.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == true) &&
                                                                                (b.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)).ToList();

            // will be false if we need signals to be drawn, or true to fake that we drew them!
            bool drawnsignals = !(systemnode.FSSSignals?.Count > 0 || systemnode.CodexEntries?.Count > 0 || systemnode.OrbitingStations?.Count > 0);

            List<ExtPictureBox.ImageList> imagesets = new List<ExtPictureBox.ImageList>();

            // we draw all image sets of the top level does and hold them. They are all aligned to 0,0 as the centre of the top body

            foreach (NodePtr starnode in toplevelbodiestodisplay)       
            {
                // draw at 0,0 the star node
                var mainstar = DrawNode(starnode.BodyNode, historicmats, curmats,starsize, rnd, ContextMenuStripBodies, ContextMenuStripMats, backwash:Color.FromArgb(64,128,0,0));
                //System.Diagnostics.Debug.WriteLine($"Mainstar node {mainstar.Size} {mainstar.Min} .. {mainstar.Max}");

                mainstar.Tag = 0;       // mark as main star
                imagesets.Add(mainstar);

                // Draw signals (if required)
                if (!drawnsignals)
                {
                    var signalimage = DrawSignals(systemnode.FSSSignals,      // may be null
                                                   systemnode.CodexEntries,      // may be null
                                                   systemnode.OrbitingStations,      // may be null
                                                   starsize.Height * 6 / 4, ContextMenuStripSignals);

                    imagesets.Add(signalimage);
                    drawnsignals = true;
                }

                // thru the child bodies, include only who match the filters and is displayable due to web selection

                List<NodePtr> bodiestodisplay = starnode.ChildBodies.Where(b => (bodytypefilters == null || b.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == true) &&
                                                                                (b.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                                                                          ).ToList();

                if (bodiestodisplay.Count>0)
                {
                    HabZones hz = starnode.BodyNode.Scan?.GetHabZones();

                    double habzonestartls = hz != null ? hz.HabitableZoneInner : 0;
                    double habzoneendls = hz != null ? hz.HabitableZoneOuter : 0;

                    for( int i = 0; i < bodiestodisplay.Count;i++)
                    {
                        var toplevelnode = bodiestodisplay[i];

                        if (toplevelnode.BodyNode.IsBarycentre )        // pick off top level barycenters and draw at this level
                        {
                            List<NodePtr> barybodiestodisplay = toplevelnode.ChildBodies.Where(b => (bodytypefilters == null || b.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == true) &&
                                                                                            (b.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)
                                                                                          ).ToList();

                            // predraw the barynode and text
                            var barymarker1 = DrawNode(toplevelnode.BodyNode, historicmats, curmats, moonsize, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                            for (int j = 0; j < barybodiestodisplay.Count; j++)
                            {
                                var barybody = barybodiestodisplay[j];

                                // we draw the tree at 0,0

                                var planetandtree = DrawPlanetAndTree(barybody, habzonestartls, habzoneendls, historicmats, curmats, bodytypefilters, rnd, ContextMenuStripBodies, ContextMenuStripMats);

                                if (j == 0)     // first one
                                {
                                    // shift bary1 into position now we know the planet size
                                    barymarker1.Shift(new Point(0, planetandtree.Min.Y - barymarker1.Max.Y));           
                                    planetandtree.AddRange(barymarker1);
                                    planetandtree.Tag = barymarker1.Size.Height;          // mark height used by baryimage on first entry, for below to shift everything down to accomodate it
                                }
                                else
                                {
                                    // create a new bary dash with no text
                                    lock (gdilock)
                                    {
                                        var barymarker2 = CreateImageAndLabel(BodyDefinitions.GetBarycentreLeftBarImageCloned(), moonsize, null, "");
                                        // shift the image at 0,0 to the same centre as the bary1 first image (which is the bary marker image)
                                        barymarker2.Shift(new Point(0, barymarker1[0].PositionCentre.Y));
                                        planetandtree.AddRange(barymarker2);
                                    }
                                }

                                imagesets.Add(planetandtree);
                            }
                        }
                        else
                        {
                            var planetandtree = DrawPlanetAndTree(toplevelnode, habzonestartls, habzoneendls, historicmats, curmats, bodytypefilters, rnd, ContextMenuStripBodies, ContextMenuStripMats);
                            imagesets.Add(planetandtree);
                        }
                    }
                }
            }

            // Draw signals (if required)
            if (!drawnsignals)
            {
                lock (gdilock)
                {
                    var fakeplanet = CreateImageAndLabel(BodyDefinitions.GetImageNotScannedCloned(), starsize, new string[] { "" }, "", ContextMenuStripSignals);
                    imagesets.Add(fakeplanet);
                }

                var signalimage = DrawSignals(systemnode.FSSSignals,      // may be null
                                               systemnode.CodexEntries,      // may be null
                                               systemnode.OrbitingStations,      // may be null
                                               starsize.Height * 6 / 4, ContextMenuStripSignals);

                imagesets.Add(signalimage);
                drawnsignals = true;
            }

            // Now position the image sets

            int yoffset = starsize.Height * nodeheightratio / 2 / noderatiodivider;
            Point cursorlm = new Point(startpoint.X, startpoint.Y + yoffset );
            int curlinestart = 0;
            int maxy = 0;

            for( int i = 0; i < imagesets.Count; i++)
            {
                var entry = imagesets[i];
                int? tag = entry.Tag as int?;

                // we shift across from .X which is left, adding on any negative pixels space in the entry
                int leftpoint = cursorlm.X - entry.Min.X;

                // if too far right, or we are on a main star and no planets on same line is on, or we painted children
                if (leftpoint + entry.Size.Width > widthavailable || (tag == 0 && i >0 && (NoPlanetStarsOnSameLine || i - curlinestart > 0)))
                {
                    cursorlm = new Point(startpoint.X, maxy + yoffset + starplanetgroupspacery);
                    leftpoint = cursorlm.X - entry.Min.X;
                    curlinestart = i;
                }

                if ( tag > 0)      // barycentre start, we need to shift the line before down by an amount and set the cursor position lower
                {
                    int shiftamount = tag.Value;
                    for (int j = curlinestart; j < i; j++)      // all on current line, shift down
                    {
                        imagesets[j].Shift(new Point(0, shiftamount));
                    }

                    cursorlm.Y += shiftamount;                  // move cursor down
                    maxy += shiftamount;
                }

                entry.Shift(new Point(leftpoint, cursorlm.Y));          // move the centre of the planet image to leftpoint,cursorlm.Y
                maxy = Math.Max(maxy, entry.Max.Y);                     // keep a record of max bottom

                cursorlm = new Point(cursorlm.X + entry.Size.Width + starplanetgroupspacery, cursorlm.Y);
            }

            // accumulate and return image set

            ExtPictureBox.ImageList imageList = new ExtPictureBox.ImageList();
            foreach (var entry in imagesets)
                imageList.AddRange(entry);

            return imageList;
        }


        // draw a single object at 0,0 centred
        public ExtPictureBox.ImageList CreateSingleObject(BodyNode bn, Size size, List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);

            ExtPictureBox.ImageList ie = new ExtPictureBox.ImageList();
            Random rnd = new Random(bn.Name().GetHashCode());         // always start with same seed so points are in same places
            var image = DrawNode(bn, historicmats, curmats, size, rnd, null, null);
            return image;
        }

        public void SetSize(int stars)
        {
            starsize = new Size(stars, stars);
            planetsize = new Size(starsize.Width * 3 / 4, starsize.Height * 3 / 4);
            moonsize = new Size(starsize.Width * 2 / 4, starsize.Height * 2 / 4);
            int matsize = stars >= 64 ? 18 : 12;
            materialsize = new Size(matsize, matsize);

            starplanetgroupspacery = 4;    
            moonspacerx = Math.Min(stars / 4, 8);
            moonspacery = Math.Min(stars / 4, 8);
            materiallinespacerxy = 4;
        }


#endregion

        #region Implementation


        private Size starsize,planetsize, moonsize, materialsize;
        private int starplanetgroupspacery;        // distance between each star/planet grouping 
        private int moonspacerx;        // distance to move moon across
        private int moonspacery;        // distance to slide down moon vs planet
        private int materiallinespacerxy;   // extra distance to add around material output

        const int noderatiodivider = 8;     // in eighth sizes
        const int nodeheightratio = 12;     // nominal size 12/8th of Size

        #endregion


    }
}

