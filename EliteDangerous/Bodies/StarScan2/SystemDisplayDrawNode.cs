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
        // return images
        // body is painted centred on 0,0, text below

        private ExtendedControls.ImageElement.List DrawNode(
                            BodyNode bn,
                            List<MaterialCommodityMicroResource> historicmats,    // historicmats may be null
                            List<MaterialCommodityMicroResource> curmats,    // curmats may be null
                            Size size,                      // nominal size
                            Random random,                  // for random placements
                            ContextMenuStrip rightclickmenubody,  // any right click CMS to assign
                            ContextMenuStrip rightclickmenumats,  // any right click CMS to assign
                            Color? backwash = null,         // optional back wash on image 
                            bool notext = false             // don't put text at bottom
                )
        {
            //System.Diagnostics.Debug.WriteLine($"DrawNode {bn.OwnName} size {size}");
            //backwash = Color.FromArgb(128, 40, 40, 40); // debug

            JournalScan sc = bn.Scan;

            string presentationname = bn.Name();
#if DEBUG
            if ( bn.CanonicalNameDepth>=0)
                presentationname += $" ({bn.BodyID} L{bn.CanonicalNameDepth})";
            else
                presentationname += $" ({bn.BodyID})";
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
                                    (bn.Signals != null ? 1 : 0) + (bn.Organics != null ? 1 : 0) + (bn.CodexEntries != null ? 1 : 0)
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
                            g.DrawImageLocked(BodyDefinitions.GetImageAtmosphere(), imageleft, imagetop, size.Width, size.Height);
                        }

                        Image nodeimage = sc.IsStar ? BaseUtils.Icons.IconSet.GetIcon(sc.StarTypeImageName) :
                                            sc.IsPlanet ? BaseUtils.Icons.IconSet.GetIcon(sc.PlanetClassImageName) :
                                            BodyDefinitions.GetImageNotScanned();

                        g.DrawImageLocked(nodeimage, imageleft, imagetop, size.Width, size.Height);

                        if (sc.IsLandable)
                        {
                            int offset = size.Height * 4 / 16;
                            int scale = 5;
                            g.DrawImageLocked(BodyDefinitions.GetImageLandable(), new Rectangle(imageleft + size.Width / 2 - offset * scale / 2,
                                                                                            imagetop + size.Height / 2 - offset * scale / 2, offset * scale, offset * scale));
                        }

                        if (sc.HasRingsOrBelts && !toplevelstar)
                        {
                            g.DrawImageLocked(sc.Rings.Count() > 1 ? BodyDefinitions.GetImageRingGap() : BodyDefinitions.GetImageRingOnly(),
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
                                g.DrawImageLocked(BodyDefinitions.GetImageTerraFormable(),new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.HasMeaningfulVolcanism) //this renders below the terraformable icon if present
                            {
                                g.DrawImageLocked(BodyDefinitions.GetImageVolcanism(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (valuable)
                            {
                                g.DrawImageLocked(BodyDefinitions.GetImageHighValue(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.Mapped)
                            {
                                g.DrawImageLocked(BodyDefinitions.GetImageMapped(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (sc.IsPreviouslyMapped)
                            {
                                g.DrawImageLocked( BodyDefinitions.GetImageMappedByOthers(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (isdiscovered)
                            {
                                g.DrawImageLocked(BodyDefinitions.GetImageDiscoveredByOthers(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (bn.Signals != null)
                            {
                                Image img = BodyDefinitions.GetImageSignals();
                                bool containsgeo = JournalSAASignalsFound.ContainsGeo(bn.Signals);
                                if (JournalSAASignalsFound.ContainsBio(bn.Signals))
                                    img = containsgeo ? BodyDefinitions.GetImageGeoBioSignals() : BodyDefinitions.GetImageBioSignals();
                                else if (containsgeo)
                                    img = BodyDefinitions.GetImageGeoSignals();

                                g.DrawImageLocked(img, new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (bn.Organics != null)
                            {
                                g.DrawImageLocked(bn.CountBioSignals == bn.CountOrganicsScansAnalysed  ? BodyDefinitions.GetImageOrganicsScanned(): BodyDefinitions.GetImageOrganicsIncomplete(),
                                                        new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }

                            if (bn.CodexEntries != null)
                            {
                                g.DrawImageLocked(BodyDefinitions.GetImageCodexEntry(), new Rectangle(0, vpos, iconsize, iconsize));
                                vpos += iconsize + iconvspacing;
                            }
                        }

                        if (materialsicon)
                        {
                            var mm = BodyDefinitions.GetImageMoreMaterials();
                            g.DrawImageLocked(mm, new Rectangle(bmp.Width - mm.Width, bmp.Height - mm.Height, mm.Width, mm.Height));
                        }

                        if (overlaytext.HasChars())
                        {
                            float ii = nodeimage.CentralImageIntensity();
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
                    if (notext)
                        nodelabels = null;

                    //System.Diagnostics.Debug.WriteLine("Body " + sc.BodyName + " plot at "  + postoplot + " " + bmp.Size + " " + (postoplot.X+imageleft) + "," + (postoplot.Y-bmp.Height/2+imagetop));
                    var il = CreateImageAndLabel(bmp, bmp.Size, nodelabels, sc.DisplayText(historicmats, curmats), rightclickmenubody
                        //,backwash: Color.FromArgb(128, 0, 0, 255)
                        );

                    //System.Diagnostics.Debug.WriteLine($"Draw {nodelabels[0]} at leftmid {postoplot}  out pos {imagepos} centre {imagexcentre}");

                    if (sc.HasMaterials && ShowMaterials)
                    {
                        var matimages = CreateMaterialNodes(sc, historicmats, curmats, materialsize, rightclickmenumats);
                        
                        if (il.Min.Y + matimages.Size.Height > il[0].Bounds.Bottom)       // too big
                            matimages.Shift(new Point(il.Max.X, il.Min.Y)); // shift to right completely, and move up to min Y
                        else
                            matimages.Shift(new Point(il[0].Bounds.Right, il.Min.Y)); // We can use a minimum shift right to the planet image, as it fits under the first text

                        il.AddRange(matimages);
                    }

                    return il;
                } // end gdi lock
            }
            else if (bn.BodyType == BodyDefinitions.BodyType.Barycentre)
            {
                string sma = bn.BarycentreScan?.SemiMajorAxisLSKM ?? "";
                var nodelabels = new string[] { presentationname.AppendPrePad(sma, Environment.NewLine) };

                //if (drawtype == DrawLevel.NoText)
                //  nodelabels = null;

                String tooltip = string.Format("Barycentre of {0}".Tx(), presentationname);

                if (notext)
                    nodelabels = null;

                var img = BodyDefinitions.GetImageBarycentre().CloneLocked();
                var il = CreateImageAndLabel(img, size, nodelabels, tooltip, rightclickmenubody, backwash);
                return il;
            }
            else if (bn.BodyType == BodyDefinitions.BodyType.StellarRing)
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

                string sma = bn.BeltData?.SemiMajorAxisLSKM ?? "";
                var nodelabels = new string[] { presentationname.AppendPrePad(sma, Environment.NewLine) };

                if (notext)
                    nodelabels = null;

                var img = BodyDefinitions.GetImageBeltCluster().CloneLocked();
                var il = CreateImageAndLabel(img, bmpsize, nodelabels,
                                tooltip.ToString(), rightclickmenubody, backwash);
                return il;
            }
            else if (bn.BodyType == BodyDefinitions.BodyType.AsteroidCluster)
            {
                var tooltip = new System.Text.StringBuilder(256);
                bn.Scan?.DisplayText(tooltip);

                var nodelabels = new string[] { presentationname };
                if (notext)
                    nodelabels = null;

                Size bmpsize = new Size(size.Width / 2, size.Height / 2);

                var img = BodyDefinitions.GetImageBeltBody().CloneLocked();
                var il = CreateImageAndLabel(img, bmpsize, nodelabels, tooltip.ToString(), rightclickmenubody, backwash);
                return il;
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

                if (notext)
                    nodelabels = null;

                var img = BodyDefinitions.GetImageNotScanned().CloneLocked();
                var il = CreateImageAndLabel(img, size, nodelabels, tooltip.ToString(), rightclickmenubody, backwash);
                return il;
            }
        }

  
    }

}


