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
using EliteDangerousCore.JournalEvents;
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
        // draws nodes for Star, PlanetMoon, Barycentre, BeltCluster, BeltClusterBody
        // return right bottom of area used from curpos
        // centre Y position is given by position.Y
        // you can give either left position (position.X with xiscentre=false) or centre position (with xiscentre=true)
        // if shiftrightifneeded then the whole image moves 

        static object gdilock = new object();

        public Point DrawNode(List<ExtPictureBox.ImageElement> pc,
                            BodyNode bn,
                            List<MaterialCommodityMicroResource> historicmats,    // historicmats may be null
                            List<MaterialCommodityMicroResource> curmats,    // curmats may be null
                            Point position,                 // position is normally left/middle, unless xiscentre is set.
                            bool xiscentre,                 // position.X is the centre to align to, not the left
                            bool shiftrightifneeded,        // are we allowed to nerf the object right to allow for labels
                            out Rectangle imagepos,         // this is the rectangle used by the node to draw into
                            out int imagexcentre,           // this is the x centre of the image which may not be in the middle of the rectangle
                            Size size,                      // nominal size
                            Random random,                  // for random placements
                            System.Windows.Forms.ContextMenuStrip rightclickmenubody,  // any right click CMS to assign
                            System.Windows.Forms.ContextMenuStrip rightclickmenumats,  // any right click CMS to assign
                            Color? backwash = null,         // optional back wash on image 
                            bool notext = false             // don't put text at bottom
                )
        {
            //System.Diagnostics.Debug.WriteLine($"DrawNode {bn.OwnName} at {position} xiscentre {xiscentre} : size {size}");
            //backwash = Color.FromArgb(128, 40, 40, 40); // debug

            Point endpoint = position;
            imagepos = Rectangle.Empty;
            imagexcentre = 0;

            JournalScan sc = bn.Scan;

            string presentationname = bn.OwnName;
#if DEBUG
            if (bn.CanonicalNameNoSystemName().HasChars())
                presentationname += " | " + bn.CanonicalNameNoSystemName();
            //presentationname += $"({bn.BodyID} {bn.BodyType})";
            presentationname += $"({bn.BodyID})";
#endif

            // determine if star or planet.  We can normally for new modern scans rely on bn.Bodytype
            // for older scans the item is marked Unknown so lets see if the scan gives an idea

            if (sc != null && (!sc.IsWebSourced || ShowWebBodies) && !sc.IsBeltClusterBody)     // scan and we have source allowed and its not a BC body
            {
                string overlaytext = "";
                var nodelabels = new string[2] { "", "" };

                nodelabels[0] = presentationname;
                if (sc.IsWebSourced)
                    nodelabels[0] = "_" + nodelabels[0];

                bool toplevelstar = false;

                if (sc.IsStar)                                      // Make node information for star
                {
                    if (ShowStarClasses)
                        overlaytext = sc.StarClassificationAbv;

                    if (sc.nStellarMass.HasValue && ShowStarMass)
                        nodelabels[1] = nodelabels[1].AppendPrePad($"{sc.nStellarMass.Value:N2} SM", Environment.NewLine);

                    toplevelstar = bn.IsTopLevel(bn.BodyType);
                    if (toplevelstar)
                    {
                        if (sc.nAge.HasValue && ShowStarAge)
                            nodelabels[1] = nodelabels[1].AppendPrePad($"{sc.nAge.Value:N0} MY", Environment.NewLine);

                        if (ShowHabZone)
                        {
                            HabZones hz = sc.GetHabZones();
                            if (hz != null)
                            {
                                nodelabels[1] = nodelabels[1].AppendPrePad($"{hz.GetHabZoneStringLs()}", Environment.NewLine);
                            }
                        }
                    }
                }
                else
                {                                                   // Make node information for planets
                    if (ShowPlanetClasses)
                        overlaytext = Planets.PlanetAbv(sc.PlanetTypeID);

                    if ((sc.IsLandable || ShowAllG) && sc.nSurfaceGravity != null)
                    {
                        nodelabels[1] = nodelabels[1].AppendPrePad($"{(sc.nSurfaceGravity / BodyPhysicalConstants.oneGee_m_s2):N2}g", Environment.NewLine);
                    }

                    if (ShowPlanetMass && sc.nMassEM.HasValue)
                    {
                        nodelabels[1] = nodelabels[1].AppendPrePad(sc.MassEMMM, Environment.NewLine);
                    }
                }

                if (ShowDist && sc.DistanceFromArrivalLS > 0)         // show distance, and not 0 (thus main star)
                {
                    if (bn.IsTopLevel(bn.BodyType))
                    {
                        string s1 = sc.DistanceFromArrivalLS < 10 ? $"{sc.DistanceFromArrivalLS:N1}ls" : $"{sc.DistanceFromArrivalLS:N0}ls";

                        if (sc.IsOrbitingBarycentre)          // if in orbit of barycentre
                        {
                            string s = s1;
                            if (sc.nSemiMajorAxis.HasValue)
                                s += "/" + sc.SemiMajorAxisLSKM;
                            nodelabels[1] = nodelabels[1].AppendPrePad(s, Environment.NewLine);
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(sn.ScanData.BodyName + " SMA " + sn.ScanData.nSemiMajorAxis + " " + sn.ScanData.DistanceFromArrivalm);
                            string s2 = sc.nSemiMajorAxis.HasValue && Math.Abs(sc.nSemiMajorAxis.Value - sc.DistanceFromArrivalm) > BodyPhysicalConstants.oneAU_m ? ("/" + sc.SemiMajorAxisLSKM) : "";
                            nodelabels[1] = nodelabels[1].AppendPrePad(s1 + s2, Environment.NewLine);
                        }
                    }
                    else
                    {
                        if (!sc.IsOrbitingBarycentre && sc.nSemiMajorAxis.HasValue)          // if not in orbit of barycentre
                        {
                            nodelabels[1] = nodelabels[1].AppendPrePad($"{(sc.nSemiMajorAxis / BodyPhysicalConstants.oneLS_m):N0}ls", Environment.NewLine);
                        }
                    }
                }

#if DEBUG
                //nodelabels[1] = nodelabels[1] + $" ID:{sc.BodyID}";
#endif

                bool valuable = sc.GetEstimatedValues().EstimatedValue(sc.WasDiscovered, sc.WasMapped, true, true, false) >= ValueLimit;
                bool isdiscovered = sc.IsPreviouslyDiscovered && sc.IsPlanet;
                int numiconoverlays = ShowOverlays ? ((sc.Terraformable ? 1 : 0) + (sc.HasMeaningfulVolcanism ? 1 : 0) +
                                    (valuable ? 1 : 0) + (sc.Mapped ? 1 : 0) + (isdiscovered ? 1 : 0) + (sc.IsPreviouslyMapped ? 1 : 0) +
                                    (bn.Signals != null ? 1 : 0) + (bn.Organics != null ? 1 : 0) + (bn.CodexEntries!=null ? 1 : 0)
                                    ) : 0;

                bool materialsicon = sc.HasMaterials && !ShowMaterials;

                int bitmapheight = size.Height * nodeheightratio / noderatiodivider;        // what height it is in units of noderatiodivider

                // work out the width multipler dependent on what we draw
                int imagewidthmultiplier16th = (sc.HasRingsOrBelts && !toplevelstar) ? 31 : materialsicon || sc.IsLandable ? 20 : 16;
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

                lock (gdilock)      // accessing fixed GetIcon bits maps must be single threaded
                {
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

                        Image nodeimage = sc.IsStar ? BaseUtils.Icons.IconSet.GetIcon(sc.StarTypeImageName) :
                                            sc.IsPlanet ? BaseUtils.Icons.IconSet.GetIcon(sc.PlanetClassImageName) :
                                            BodyDefinitions.GetImageNotScanned();

                        //.BodyType == BodyNode.BodyClass.BeltCluster ? (Bitmap)BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Belt") :

                        g.DrawImage(nodeimage, imageleft, imagetop, size.Width, size.Height);

                        if (sc.IsLandable)
                        {
                            int offset = size.Height * 4 / 16;
                            int scale = 5;
                            g.DrawImage(BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Landable"), new Rectangle(imageleft + size.Width / 2 - offset * scale / 2,
                                                                                            imagetop + size.Height / 2 - offset * scale / 2, offset * scale, offset * scale));
                        }

                        if (sc.HasRingsOrBelts && !toplevelstar)
                        {
                            g.DrawImage(sc.Rings.Count() > 1 ? BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingGap") : BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RingOnly"),
                                            new Rectangle(imageleft - size.Width / 2, imagetop, size.Width * 2, size.Height));
                        }

                        int ss = bn.SurfaceFeatureListSettlementsCount();
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

                            if (bn.Signals != null)
                            {
                                string image = "Controls.Scan.Bodies.Signals";
                                bool containsgeo = JournalSAASignalsFound.ContainsGeo(bn.Signals);
                                if (JournalSAASignalsFound.ContainsBio(bn.Signals))
                                    image = containsgeo ? "Controls.Scan.Bodies.SignalsGeoBio" : "Controls.Scan.Bodies.SignalsBio";
                                else if (containsgeo)
                                    image = "Controls.Scan.Bodies.SignalsGeo";
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon(image), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (bn.Organics != null)
                            {
                                string imagename = bn.CountBioSignals == bn.CountOrganicsScansAnalysed ? "Journal.ScanOrganic" : "Controls.OrganicIncomplete";
                                g.DrawImage(BaseUtils.Icons.IconSet.GetIcon(imagename), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (bn.CodexEntries != null)
                            {
                                string imagename = "Journal.CodexEntry";
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
                                var imageintensity = ((Bitmap)nodeimage).Function(BitMapHelpers.BitmapFunction.Brightness, nodeimage.Width * 3 / 8, nodeimage.Height * 3 / 8, nodeimage.Width * 2 / 8, nodeimage.Height * 2 / 8);
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

                    if (notext)
                        nodelabels = null;

                    //System.Diagnostics.Debug.WriteLine("Body " + sc.BodyName + " plot at "  + postoplot + " " + bmp.Size + " " + (postoplot.X+imageleft) + "," + (postoplot.Y-bmp.Height/2+imagetop));
                    endpoint = CreateImageAndLabel(pc, bmp, postoplot, bmp.Size, shiftrightifneeded, out imagepos, nodelabels, sc.DisplayText(historicmats, curmats), rightclickmenubody);

                    int xshift = imagepos.X - postoplot.X;          // we may have shifted right if shiftrightifneeded is on, need to take account in imagexcentre

                    imagexcentre = (xiscentre ? position.X : postoplot.X + iconwidtharea + imagewidtharea / 2) + xshift;  // where the x centre of the planet is

                    //System.Diagnostics.Debug.WriteLine($"Draw {nodelabels[0]} at leftmid {postoplot}  out pos {imagepos} centre {imagexcentre}");

                    if (sc.HasMaterials && ShowMaterials)
                    {
                        Point matpos = new Point(endpoint.X + 4, position.Y);
                        Point endmat = CreateMaterialNodes(pc, sc, historicmats, curmats, matpos, materialsize, rightclickmenumats);
                        endpoint = new Point(Math.Max(endpoint.X, endmat.X), Math.Max(endpoint.Y, endmat.Y)); // record new right point..
                    }
                } // end gdi lock
            }
            else if (bn.BodyType == BodyNode.BodyClass.Barycentre)
            {
                string sma = bn.BarycentreScan?.SemiMajorAxisLSKM ?? "";
                var nodelabels = new string[] { presentationname.AppendPrePad(sma, Environment.NewLine) };

                //if (drawtype == DrawLevel.NoText)
                //  nodelabels = null;

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;

                String tooltip = string.Format("Barycentre of {0}".Tx(), presentationname);

                if (notext)
                    nodelabels = null;

                lock (gdilock)
                {
                    endpoint = CreateImageAndLabel(pc, BodyDefinitions.GetBarycentreImageCloned(), postoplot, size, shiftrightifneeded, out imagepos, nodelabels, tooltip, rightclickmenubody, backwash);
                }

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the not scanned thing is
            }
            else if (bn.BodyType == BodyNode.BodyClass.BeltCluster)
            {
                var tooltip = new System.Text.StringBuilder(256);

                if (bn.BeltData != null)
                    bn.BeltData.RingText(tooltip);
                else
                    tooltip.Append(presentationname + Environment.NewLine + Environment.NewLine + "No scan data available".Tx());

                tooltip.AppendCR();
                foreach (BodyNode snc in bn.ChildBodies)
                {
                    if (snc.Scan != null)
                    {
                        snc.Scan.DisplayText(tooltip);
                        tooltip.AppendCR();
                    }
                }

                Size bmpsize = new Size(size.Width, planetsize.Height * nodeheightratio / noderatiodivider);

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;

                string sma = bn.BeltData?.SemiMajorAxisLSKM ?? "";
                var nodelabels = new string[] { presentationname.AppendPrePad(sma, Environment.NewLine) };

                if (notext)
                    nodelabels = null;

                lock (gdilock)
                {
                    endpoint = CreateImageAndLabel(pc, BodyDefinitions.GetBeltImageCloned(), postoplot, bmpsize, shiftrightifneeded, out imagepos, nodelabels,
                                    tooltip.ToString(), rightclickmenubody, backwash);
                }

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the belt is
            }
            else if (bn.BodyType == BodyNode.BodyClass.BeltClusterBody)
            {
                var tooltip = new System.Text.StringBuilder(256);
                bn.Scan?.DisplayText(tooltip);

                var nodelabels = new string[] { presentationname};
                if (notext)
                    nodelabels = null;

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;
                Size bmpsize = new Size(size.Width / 2, size.Height / 2);

                lock (gdilock)
                {
                    endpoint = CreateImageAndLabel(pc, BodyDefinitions.GetBeltBodyImageCloned(), postoplot, bmpsize, shiftrightifneeded, out imagepos, nodelabels,
                                    tooltip.ToString(), rightclickmenubody,  backwash);
                }

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the belt is
            }
            else    // no scan, not one of the ones above, therefore unknown
            {
                // NOT SCANNED
                var tooltip = new System.Text.StringBuilder(256);

                tooltip.AppendLine(presentationname);
                tooltip.AppendLine("No scan data available".Tx());
                tooltip.AppendCR();

                string addtext = "";

                if (bn.Features != null)
                {
                    tooltip.AppendFormat("Surface features".Tx());
                    tooltip.Append(":");
                    JournalScan.DisplaySurfaceFeatures(tooltip, bn.Features, 4, false, Environment.NewLine);
                    tooltip.AppendCR();
                }
                if (bn.Signals != null)
                {
                    tooltip.AppendFormat("Signals".Tx());
                    tooltip.Append(":");
                    JournalSAASignalsFound.SignalList(tooltip, bn.Signals, 4, false, false, Environment.NewLine);
                    tooltip.AppendCR();
                }
                if (bn.Genuses != null)
                {
                    tooltip.AppendFormat("Genuses".Tx());
                    tooltip.Append(":");
                    JournalSAASignalsFound.GenusList(tooltip, bn.Genuses, 4, false, false, Environment.NewLine);
                    tooltip.AppendCR();
                }
                if (bn.Organics != null)
                {
                    tooltip.AppendFormat("Organics".Tx());
                    tooltip.Append(":");
                    JournalScanOrganic.OrganicList(tooltip, bn.Organics, 4, false, Environment.NewLine);
                    tooltip.AppendCR();
                }
                if (bn.CodexEntries != null)
                {
                    tooltip.Append("Codexs".Tx());
                    tooltip.Append(": ");
                    JournalCodexEntry.CodexList(tooltip, bn.CodexEntries, 4, false, Environment.NewLine);
                    tooltip.AppendCR();
                }

                tooltip.Append(addtext);
                tooltip.AppendCR();

                var nodelabels = new string[] { presentationname };

                Point postoplot = xiscentre ? new Point(position.X - size.Width / 2, position.Y) : position;

                if (notext)
                    nodelabels = null;

                lock (gdilock)
                {
                    endpoint = CreateImageAndLabel(pc, BodyDefinitions.GetImageNotScannedCloned(), postoplot, size, shiftrightifneeded, out imagepos, nodelabels, tooltip.ToString(), rightclickmenubody, backwash);
                }

                imagexcentre = imagepos.Left + imagepos.Width / 2;                 // where the x centre of the not scanned thing is
            }

            return endpoint;
        }


  
        private Dictionary<Image, float> imageintensities = new Dictionary<Image, float>();       // cached
    }

}


