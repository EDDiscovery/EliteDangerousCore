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
        public Size PlanetSize { get { return planetsize; } }
        public Size MoonSize { get { return moonsize; } }

        // After create, this holds the used display area
        public Point DisplayAreaUsed { get; private set; }

        public SystemDisplay()
        {
        }

        // Clears the imagebox, create the objects, render to imagebox. Accepts systemnode = null to clear the display
        public void DrawSystemRender(ExtendedControls.ExtPictureBox imagebox, int widthavailable, SystemNode systemnode,
                               List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                               string titletext = null, string[] bodytypefilter = null
                              )
        {
            imagebox.ClearImageList();
            if (systemnode != null)
            {
                var list = CreateSystemImages(systemnode, new Point(0,0), widthavailable, historicmats, curmats, titletext, bodytypefilter);
                imagebox.AddRange(list);
            }
            imagebox.Render();
        }

        public ExtendedControls.ImageElement.List CreateSystemImages(SystemNode systemnode, Point startpoint, int widthavailable,
                               List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null,
                               string titletext = null,
                               string[] bodytypefilters = null
                               )
        {
            System.Diagnostics.Debug.Assert(Font != null);
            System.Diagnostics.Debug.Assert(systemnode != null);
            System.Diagnostics.Debug.Assert(!starsize.IsEmpty);

            Random rnd = new Random(systemnode.System.Name.GetHashCode());         // always start with same seed so points are in same places

            NodePtr toplevelbodieslist = systemnode.BodiesSimplified(SimplifyDiagram);

            // filter off ones not to display
            var toplevelbodiestodisplay = toplevelbodieslist.ChildBodies.Where(b => (bodytypefilters == null || b.BodyNode.IsBodyTypeInFilter(bodytypefilters, true) == true) &&
                                                                                (b.BodyNode.DoesNodeHaveNonWebScansBelow() || ShowWebBodies)).ToList();

            // will be false if we need signals to be drawn, or true to fake that we drew them!
            bool drawnsignals = !(systemnode.FSSSignals?.Count > 0 || systemnode.CodexEntries?.Count > 0 || systemnode.OrbitingStations?.Count > 0);

            ExtendedControls.ImageElement.List imageList = new ExtendedControls.ImageElement.List();    // final image list to report
 
            int yoffset = starsize.Height * nodeheightratio / 2 / noderatiodivider;

            if (titletext.HasChars())
            {
                var lab = new ExtendedControls.ImageElement.Element();
                lab.TextAutoSize(new Point(startpoint.X, startpoint.Y), new Size(500, 30), titletext, FontLarge, TextForeColor, TextBackColor);
                imageList.Add(lab);
                yoffset += lab.Image.Height + 8;
            }

            // we draw all image sets of the top level does and hold them. They are all aligned to 0,0 as the centre of the top body

            List<ExtendedControls.ImageElement.List> imagesets = new List<ExtendedControls.ImageElement.List>();

            foreach (NodePtr starnode in toplevelbodiestodisplay)
            {
                // draw at 0,0 the star node
                var mainstar = DrawNode(starnode.BodyNode, historicmats, curmats, starsize, rnd, ContextMenuStripBodies, ContextMenuStripMats);//, backwash:Color.FromArgb(64,128,0,0));
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

                HabZones hz = starnode.BodyNode.Scan?.GetHabZones();
                double habzonestartls = hz != null ? hz.HabitableZoneInner : 0;
                double habzoneendls = hz != null ? hz.HabitableZoneOuter : 0;

                foreach(var toplevelnode in bodiestodisplay )
                {
                    var planetandtree = DrawTree(toplevelnode, new Point(0,0), 0, habzonestartls, habzoneendls, historicmats, curmats, bodytypefilters, rnd, ContextMenuStripBodies, ContextMenuStripMats);
                    imagesets.Add(planetandtree);
                }
            }

            // Draw signals (if required)
            if (!drawnsignals)
            {
                var img = BodyDefinitions.GetImageNotScanned().CloneLocked();
                var fakeplanet = CreateImageAndLabel(img, starsize, new string[] { "" }, "", ContextMenuStripSignals);
                imagesets.Add(fakeplanet);

                var signalimage = DrawSignals(systemnode.FSSSignals,      // may be null
                                               systemnode.CodexEntries,      // may be null
                                               systemnode.OrbitingStations,      // may be null
                                               starsize.Height * 6 / 4, ContextMenuStripSignals);

                imagesets.Add(signalimage);
                drawnsignals = true;
            }

            // Now position the image sets

            Point cursorlm = new Point(startpoint.X, startpoint.Y + yoffset);
            int curlinestart = 0;
            int maxy = 0;
            bool firstbary = true;

            for (int i = 0; i < imagesets.Count; i++)
            {
                var entry = imagesets[i];
                
                bool toplevelstar = entry.Tag is int;                       // tag either records an int, which means its a top level star, 
                var nodesizeinfo = entry.Tag as Tuple<BodyNode, int>;       // or info on the top level item

                // we shift across from .X which is left, adding on any negative pixels space in the entry
                int leftpoint = cursorlm.X - entry.Min.X;

                // if too far right, or we are on a main star and no planets on same line is on, or we painted children
                // we shift down the cursor
                if (leftpoint + entry.Size.Width > widthavailable || (toplevelstar && i > 0 && (NoPlanetStarsOnSameLine || i - curlinestart > 0)))
                {
                    cursorlm = new Point(startpoint.X, maxy + yoffset + starplanetgroupspacery);
                    leftpoint = cursorlm.X - entry.Min.X;
                    curlinestart = i;
                    firstbary = true;
                }
                
                // if we have a top level barycentre, and its a first, we shift the whole line down before it, and move the cursor down a line. Only do this once per line
                if (nodesizeinfo?.Item1.IsBarycentre ?? false)   
                {
                    if (firstbary)
                    {
                        int cursorshiftamount = nodesizeinfo.Item2 - planetsize.Height / 2;               // because of the icon, we can afford not to move down the whole shiftamount
                        for (int j = curlinestart; j < i; j++)      // all on current line, shift down
                        {
                            imagesets[j].Shift(new Point(0, cursorshiftamount));
                        }

                        cursorlm.Y += cursorshiftamount;                          // move cursor down
                        maxy += cursorshiftamount;

                        firstbary = false;
                    }

                    entry.Shift(new Point(0, -nodesizeinfo.Item2));     // but we need to move this up the whole amount to align the bit underneath it right
                }

                entry.Shift(new Point(leftpoint, cursorlm.Y));          // move the centre of the planet image to leftpoint,cursorlm.Y
                maxy = Math.Max(maxy, entry.Max.Y);                     // keep a record of max bottom for the carriage return above

                cursorlm = new Point(cursorlm.X + entry.Size.Width + starplanetgroupspacery, cursorlm.Y);
            }

            // accumulate and return image set
            foreach (var entry in imagesets)
                imageList.AddRange(entry);

            return imageList;
        }


        // draw a single object at 0,0 centred
        public ExtendedControls.ImageElement.List CreateSingleObject(BodyNode bn, Size size, List<MaterialCommodityMicroResource> historicmats = null, List<MaterialCommodityMicroResource> curmats = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);

            ExtendedControls.ImageElement.List ie = new ExtendedControls.ImageElement.List();
            Random rnd = new Random(bn.Name().GetHashCode());         // always start with same seed so points are in same places
            var image = DrawNode(bn, historicmats, curmats, size, rnd, null, null);
            return image;
        }

        // draw a single object at 0,0 centred
        public ExtendedControls.ImageElement.List CreateStar(EDStar starclass, Size size, string tooltip = null)
        {
            System.Diagnostics.Debug.Assert(Font != null);

            string imagename = BodyDefinitions.StarTypeImageName(starclass, 1.0, 5000);
            var starimage = BaseUtils.Icons.IconSet.GetIcon(imagename).CloneLocked();

            var nodelabels = new string[] { Stars.ToLocalisedLanguage(starclass) };
            var il = CreateImageAndLabel(starimage, size, nodelabels, tooltip, null);
            return il;
        }

        public void SetSize(int stars)
        {
            starsize = new Size(stars, stars);
            planetsize = new Size(starsize.Width * 3 / 4, starsize.Height * 3 / 4);
            moonsize = new Size(starsize.Width * 2 / 4, starsize.Height * 2 / 4);
            int matsize = stars >= 64 ? 18 : 12;
            materialsize = new Size(matsize, matsize);

            starplanetgroupspacery = 4;    
            moonspacerx = Math.Max(stars / 8, 4);
            moonspacery = Math.Max(stars / 8, 4);
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

