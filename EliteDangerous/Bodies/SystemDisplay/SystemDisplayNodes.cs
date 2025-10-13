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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class SystemDisplay
    {
        public enum DrawLevel { TopLevelStar, PlanetLevel, MoonLevel, NoText };

        private Dictionary<Bitmap, float> imageintensities = new Dictionary<Bitmap, float>();       // cached

        // return right bottom of area used from curpos

        public Point DrawNode(List<ExtPictureBox.ImageElement> pc,
                            StarScan.ScanNode sn,
                            List<MaterialCommodityMicroResource> historicmats,    // historicmats may be null
                            List<MaterialCommodityMicroResource> curmats,    // curmats may be null
                            Image notscanned,               // image if scan data is null
                            Point position,                 // position is normally left/middle, unless xiscentre is set.
                            bool xiscentre,                 // base x position as the centre, not the left
                            bool shiftrightifneeded,        // are we allowed to nerf the object right to allow for labels
                            out Rectangle imagepos,         // this is the rectangle used by the node to draw into
                            out int imagexcentre,           // this is the x centre of the image which may not be in the middle of the rectangle
                            Size size,                      // nominal size
                            DrawLevel drawtype,             // drawing.. determines text printed mostly
                            Random random,                  // for random placements
                            System.Windows.Forms.ContextMenuStrip rightclickmenubody,  // any right click CMS to assign
                            System.Windows.Forms.ContextMenuStrip rightclickmenumats,  // any right click CMS to assign
                            Color? backwash = null,         // optional back wash on image 
                            string appendlabeltext = ""    // any label text to append
                )
        {
            //System.Diagnostics.Debug.WriteLine($"DrawNode {sn.OwnName} at {position} xiscentre {xiscentre} : size {size} type {drawtype}");
  //backwash = Color.FromArgb(128, 40, 40, 40); // debug

            Point endpoint = position;
            imagepos = Rectangle.Empty;
            imagexcentre = 0;

#if DEBUG
            string presentationname = sn.BodyName!=null ? sn.BodyName + "/" + sn.OwnName : sn.OwnName;
#else
            string presentationname = sn.BodyNameOrOwnName;
#endif

            JournalScan sc = sn.ScanData;

            if (sc != null && (!sc.IsWebSourced || ShowWebBodies))     // has a scan and its our scan, or we are showing EDSM
            {
                if (sn.NodeType != StarScan.ScanNodeType.ring)       // not rings
                {
                    Bitmap nodeimage = (Bitmap)BaseUtils.Icons.IconSet.GetIcon(sc.GetStarPlanetTypeImageName());

                    string overlaytext = "";
                    var nodelabels = new string[2] { "", "" };

                    nodelabels[0] = presentationname;
                    if (sc.IsWebSourced)
                        nodelabels[0] = "_" + nodelabels[0];

                    if (sc.IsStar)                                      // Make node information for star
                    {
                        if (ShowStarClasses)
                            overlaytext = sc.StarClassificationAbv;

                        if (sc.nStellarMass.HasValue && ShowStarMass)
                            nodelabels[1] = nodelabels[1].AppendPrePad($"{sc.nStellarMass.Value:N2} SM", Environment.NewLine);

                        if (drawtype == DrawLevel.TopLevelStar)
                        {
                            if (sc.nAge.HasValue && ShowStarAge)
                                nodelabels[1] = nodelabels[1].AppendPrePad($"{sc.nAge.Value:N0} MY", Environment.NewLine);

                            if (ShowHabZone)
                            {
                                var habZone = sc.GetHabZoneStringLs();
                                if (habZone.HasChars())
                                    nodelabels[1] = nodelabels[1].AppendPrePad($"{habZone}", Environment.NewLine);
                            }
                        }
                    }
                    else
                    {                                                   // Make node information for planets
                        if (ShowPlanetClasses)
                            overlaytext = Planets.PlanetAbv(sc.PlanetTypeID);

                        if ((sn.ScanData.IsLandable || ShowAllG) && sn.ScanData.nSurfaceGravity != null)
                        {
                            nodelabels[1] = nodelabels[1].AppendPrePad($"{(sn.ScanData.nSurfaceGravity / BodyPhysicalConstants.oneGee_m_s2):N2}g", Environment.NewLine);
                        }

                        if (ShowPlanetMass && sn.ScanData.nMassEM.HasValue)
                        {
                            nodelabels[1] = nodelabels[1].AppendPrePad(sn.ScanData.MassEMMM, Environment.NewLine);
                        }
                    }

                    if (ShowDist && sn.ScanData.DistanceFromArrivalLS > 0)         // show distance, and not 0 (thus main star)
                    {
                        if (drawtype != DrawLevel.MoonLevel)       // other than moons
                        {
                            string s1 = sn.ScanData.DistanceFromArrivalLS < 10 ? $"{sn.ScanData.DistanceFromArrivalLS:N1}ls" : $"{sn.ScanData.DistanceFromArrivalLS:N0}ls";

                            if (sn.ScanData.IsOrbitingBarycentre)          // if in orbit of barycentre
                            {
                                string s = s1;
                                if (sn.ScanData.nSemiMajorAxis.HasValue)
                                    s += "/" + sn.ScanData.SemiMajorAxisLSKM;
                                nodelabels[1] = nodelabels[1].AppendPrePad(s, Environment.NewLine);
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine(sn.ScanData.BodyName + " SMA " + sn.ScanData.nSemiMajorAxis + " " + sn.ScanData.DistanceFromArrivalm);
                                string s2 = sn.ScanData.nSemiMajorAxis.HasValue && Math.Abs(sn.ScanData.nSemiMajorAxis.Value - sn.ScanData.DistanceFromArrivalm) > BodyPhysicalConstants.oneAU_m ? ("/" + sn.ScanData.SemiMajorAxisLSKM) : "";
                                nodelabels[1] = nodelabels[1].AppendPrePad(s1 + s2, Environment.NewLine);
                            }
                        }
                        else
                        {
                            if (!sn.ScanData.IsOrbitingBarycentre && sn.ScanData.nSemiMajorAxis.HasValue)          // if not in orbit of barycentre
                            {
                                nodelabels[1] = nodelabels[1].AppendPrePad($"{(sn.ScanData.nSemiMajorAxis / BodyPhysicalConstants.oneLS_m):N0}ls", Environment.NewLine);
                            }
                        }
                    }

#if DEBUG
                    //nodelabels[1] = nodelabels[1] + $" ID:{sc.BodyID}";
#endif

                    nodelabels[1] = nodelabels[1].AppendPrePad(appendlabeltext, Environment.NewLine);

                    //  nodelabels[1] = nodelabels[1].AppendPrePad("" + sn.ScanData?.BodyID, Environment.NewLine);

                    bool valuable = sc.GetEstimatedValues().EstimatedValue(sc.WasDiscovered, sc.WasMapped, true, true, false) >= ValueLimit;
                    bool isdiscovered = sc.IsPreviouslyDiscovered && sc.IsPlanet;
                    int numiconoverlays = ShowOverlays ? ((sc.Terraformable ? 1 : 0) + (sc.HasMeaningfulVolcanism ? 1 : 0) +
                                        (valuable ? 1 : 0) + (sc.Mapped ? 1 : 0) + (isdiscovered ? 1 : 0) + (sc.IsPreviouslyMapped ? 1 : 0) +
                                        (sn.Signals != null ? 1 : 0) + (sn.Organics != null ? 1 : 0)
                                        ) : 0;

                    bool materialsicon = sc.HasMaterials && !ShowMaterials;

                    int bitmapheight = size.Height * nodeheightratio / noderatiodivider;        // what height it is in units of noderatiodivider

                    // work out the width multipler dependent on what we draw
                    int imagewidthmultiplier16th = (sc.HasRingsOrBelts && drawtype != DrawLevel.TopLevelStar) ? 31 : materialsicon || sc.IsLandable ? 20 : 16;
                    // area used by image+optional overlay for planet image
                    int imagewidtharea = imagewidthmultiplier16th * size.Width / 16;

                    // this is the minimum divider for icon area to give width of icon based on height
                    const int minicondivider = 4;
                    // this divider is a max of number of icons and min divider
                    int iconsizedivider = Math.Max(numiconoverlays, minicondivider);
                    // space between icons if wanted
                    const int iconvspacing = 0;
                    // get icon size, bitmap height minus iconvspacing parts / divider.. (ie. 125 with 6 icons = 125-(6-1)*1 = 120 / 6 = 20
                    int iconsize = (bitmapheight - iconvspacing * (numiconoverlays - 1)) / iconsizedivider;
                    // actual area width, which is only set to iconsize if we have icons
                    int iconwidtharea = numiconoverlays > 0 ? iconsize : 0;

                    //System.Diagnostics.Debug.WriteLine($"..DrawNode {sn.OwnName} imagewidtharea {imagewidtharea} iconsize {iconsize} total {imagewidtharea+iconsize}");

                    // total width, of icon and image area
                    int bitmapwidth = iconwidtharea + imagewidtharea;

                    Bitmap bmp = new Bitmap(bitmapwidth, bitmapheight);

                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        if (backwash.HasValue)
                        {
                            using (Brush b = new SolidBrush(backwash.Value))
                            {
                                g.FillRectangle(b, new Rectangle(0, 0, bitmapwidth, bitmapheight));
                            }
                        }

                        int imageleft = iconwidtharea + imagewidtharea / 2 - size.Width / 2;  // calculate where the left of the image is 
                        int imagetop = bitmapheight / 2 - size.Height / 2;                  // and the top

                        if ((sc.WaterGiant || !sc.GasWorld) && (sc.HasAtmosphericComposition || sc.HasAtmosphere))            // show atmosphere as it is shown in game
                        {
                            g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Atmosphere"), imageleft, imagetop, size.Width, size.Height);
                        }

                        g.DrawImage(nodeimage, imageleft, imagetop, size.Width, size.Height);

                        if (sc.IsLandable)
                        {
                            int offset = size.Height * 4 / 16;
                            int scale = 5;
                            g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Landable"), new Rectangle(imageleft + size.Width / 2 - offset * scale / 2,
                                                                                           imagetop + size.Height / 2 - offset * scale / 2, offset * scale, offset * scale));
                        }

                        if (sc.HasRingsOrBelts && drawtype != DrawLevel.TopLevelStar)
                        {
                            g.DrawImage(sc.Rings.Count() > 1 ? BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingGap") : BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingOnly"),
                                            new Rectangle(imageleft - size.Width / 2, imagetop, size.Width * 2, size.Height));
                        }

                        int ss = StarScan.SurfaceFeatureListSettlementsCount(sc.SurfaceFeatures);
                        if (ss > 0)
                        {
                            using (Brush b = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
                            {
                                for (int i = 0; i < ss; i++)    // draw the number of settlements and pick alternately a random pos on X and top/bottom flick
                                {
                                    int hpos = imageleft + size.Width / 3 + random.Next(size.Width / 3);
                                    int vpos = (i % 2 == 0 ? imagetop + size.Height / 4 : imagetop + size.Height * 5 / 8) + random.Next(size.Width / 8);
                                    //System.Diagnostics.Debug.WriteLine($"Draw {sn.OwnName} settlement {hpos} {vpos}");
                                    g.FillRectangle(b, new Rectangle(hpos, vpos, 2, 2));
                                }
                            }
                        }

                        if (numiconoverlays > 0)
                        {
                            int vpos = 0;

                            if (sc.Terraformable)
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Terraformable"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.HasMeaningfulVolcanism) //this renders below the terraformable icon if present
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Volcanism"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (valuable)
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.HighValue"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.Mapped)
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Mapped"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.IsPreviouslyMapped)
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.MappedByOthers"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (isdiscovered)
                            {
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.DiscoveredByOthers"), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sn.Signals != null)
                            {
                                string image = "Controls.Scan.Bodies.Signals";
                                bool containsgeo = JournalSAASignalsFound.ContainsGeo(sn.Signals);
                                if (JournalSAASignalsFound.ContainsBio(sn.Signals))
                                    image = containsgeo ? "Controls.Scan.Bodies.SignalsGeoBio" : "Controls.Scan.Bodies.SignalsBio";
                                else if (containsgeo)
                                    image = "Controls.Scan.Bodies.SignalsGeo";
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon(image), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sn.Organics != null)
                            {
                                string imagename = sn.CountBioSignals == sn.CountOrganicsScansAnalysed ? "Journal.ScanOrganic" : "Controls.OrganicIncomplete";
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon(imagename), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }
                        }

                        if (materialsicon)
                        {
                            Image mm = BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.MaterialMore");
                            g.DrawImage(mm, new Rectangle(bmp.Width - mm.Width, bmp.Height - mm.Height, mm.Width, mm.Height));
                        }

                        if (overlaytext.HasChars())
                        {
                            float ii;
                            if (imageintensities.ContainsKey(nodeimage))        // find cache
                            {
                                ii = imageintensities[nodeimage];
                                //System.Diagnostics.Debug.WriteLine("Cached Image intensity of " + sn.fullname + " " + ii);
                            }
                            else
                            {
                                var imageintensity = nodeimage.Function(BitMapHelpers.BitmapFunction.Brightness, nodeimage.Width * 3 / 8, nodeimage.Height * 3 / 8, nodeimage.Width * 2 / 8, nodeimage.Height * 2 / 8);
                                ii = imageintensity.Item2;
                                imageintensities[nodeimage] = ii;
                                //System.Diagnostics.Debug.WriteLine("Calculated Image intensity of " + sn.fullname + " " + ii);
                            }

                            Color text = ii > 0.3f ? Color.Black : Color.FromArgb(255, 200, 200, 200);

                            using (Font f = new Font(Font.Name, size.Width / 5.0f))
                            {
                                using (Brush b = new SolidBrush(text))
                                {
                                    g.DrawString(overlaytext, f, b, new Rectangle(iconwidtharea, 0, bitmapwidth - iconwidtharea, bitmapheight), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                                }
                            }
                        }
                    }

                    // need left middle, if xiscentre, translate to it
                    Point postoplot = xiscentre ? new Point(position.X - imagewidtharea / 2 - iconwidtharea, position.Y) : position;

                    if (drawtype == DrawLevel.NoText)
                        nodelabels = null;

                    //System.Diagnostics.Debug.WriteLine("Body " + sc.BodyName + " plot at "  + postoplot + " " + bmp.Size + " " + (postoplot.X+imageleft) + "," + (postoplot.Y-bmp.Height/2+imagetop));
                    endpoint = CreateImageAndLabel(pc, bmp, postoplot, bmp.Size, shiftrightifneeded, out imagepos, nodelabels, sc.DisplayString(historicmats,curmats), rightclickmenubody);

                    int xshift = imagepos.X - postoplot.X;          // we may have shifted right if shiftrightifneeded is on, need to take account in imagexcentre

                    imagexcentre = (xiscentre ? position.X : postoplot.X + iconwidtharea + imagewidtharea / 2) + xshift;  // where the x centre of the planet is

                    //System.Diagnostics.Debug.WriteLine($"Draw {nodelabels[0]} at leftmid {postoplot}  out pos {imagepos} centre {imagexcentre}");

                    if (sc.HasMaterials && ShowMaterials)
                    {
                        Point matpos = new Point(endpoint.X + 4, position.Y);
                        Point endmat = CreateMaterialNodes(pc, sc, historicmats, curmats, matpos, materialsize, rightclickmenumats);
                        endpoint = new Point(Math.Max(endpoint.X, endmat.X), Math.Max(endpoint.Y, endmat.Y)); // record new right point..
                    }
                }
            }
            else if (sn.NodeType == StarScan.ScanNodeType.belt)
            {
                var tooltip = new System.Text.StringBuilder(256);

                if (sn.BeltData != null)
                    sn.BeltData.RingText(tooltip);
                else
                    tooltip.Append(sn.OwnName + Environment.NewLine + Environment.NewLine + "No scan data available".Tx());

                if (sn.Children != null && sn.Children.Count != 0)
                {
                    tooltip.AppendCR();
                    foreach (StarScan.ScanNode snc in sn.Children.Values)
                    {
                        if (snc.ScanData != null)
                        {
                            snc.ScanData.DisplaySummary(tooltip);
                            tooltip.AppendCR();
                        }
                    }
                }

                Size bmpsize = new Size(size.Width, planetsize.Height * nodeheightratio / noderatiodivider);

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;

                var nodelabels = new string[] { sn.OwnName.AppendPrePad(appendlabeltext, Environment.NewLine) };
                if (drawtype == DrawLevel.NoText)
                    nodelabels = null;

                endpoint = CreateImageAndLabel(pc, BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Belt"), postoplot, bmpsize, shiftrightifneeded, out imagepos, nodelabels, 
                                    tooltip.ToString(), rightclickmenubody, false, backwash);

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the belt is
            }
            else
            {
                var tooltip = new System.Text.StringBuilder(256);

                if (sn.NodeType == StarScan.ScanNodeType.barycentre)
                {
                    tooltip.AppendFormat("Barycentre of {0}".Tx(), sn.OwnName);
                }
                else
                {
                    tooltip.AppendLine(sn.BodyDesignator);
                    tooltip.AppendLine("No scan data available".Tx());
                    tooltip.AppendCR();

                    string addtext = "";

                    if (sn.SurfaceFeatures != null)
                    {
                        tooltip.AppendFormat("Surface features".Tx());
                        tooltip.Append(":");
                        StarScan.SurfaceFeatureList(tooltip, sn.SurfaceFeatures, 4, false, Environment.NewLine);
                        tooltip.AppendCR();
                    }
                    if (sn.Signals != null)
                    {
                        tooltip.AppendFormat("Signals".Tx());
                        tooltip.Append(":");
                        JournalSAASignalsFound.SignalList(tooltip, sn.Signals, 4, false, false, Environment.NewLine);
                        tooltip.AppendCR();
                    }
                    if (sn.Genuses != null)
                    {
                        tooltip.AppendFormat("Genuses".Tx());
                        tooltip.Append(":");
                        JournalSAASignalsFound.GenusList(tooltip, sn.Genuses, 4, false, false, Environment.NewLine);
                        tooltip.AppendCR();
                    }
                    if (sn.Organics != null)
                    {
                        tooltip.AppendFormat("Organics".Tx());
                        tooltip.Append(":");
                        JournalScanOrganic.OrganicList(tooltip, sn.Organics, 4, false, Environment.NewLine);
                        tooltip.AppendCR();

                    }

                    tooltip.Append(addtext);
                    tooltip.AppendCR();
                }

                var nodelabels = new string[] { presentationname.AppendPrePad(appendlabeltext, Environment.NewLine) };
                if (drawtype == DrawLevel.NoText)
                    nodelabels = null;

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;

                endpoint = CreateImageAndLabel(pc, notscanned, postoplot, size, shiftrightifneeded, out imagepos, nodelabels, tooltip.ToString(), rightclickmenubody, false, backwash);

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the not scanned thing is
            }

            //    System.Diagnostics.Debug.WriteLine("Node " + sn.ownname + " " + position + " " + size + " -> "+ endpoint);
            return endpoint;
        }


    }
}

