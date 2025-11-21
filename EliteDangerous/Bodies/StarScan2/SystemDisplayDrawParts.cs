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

using EliteDangerousCore.JournalEvents;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BaseUtils;
using ExtendedControls.ImageElement;

namespace EliteDangerousCore.StarScan2
{
    public partial class SystemDisplay
    {
        // draw materials node, first at 0,0, across and down
        private ExtendedControls.ImageElement.List CreateMaterialNodes( JournalScan sn, 
                                            List<MaterialCommodityMicroResource> historicmats,      // may be null
                                            List<MaterialCommodityMicroResource> curmats,           // may be null
                                            Size matsize, 
                                            ContextMenuStrip rightclickmenu)
        {
            Point matpos = new Point(0,0);
            int noperline = 0;

            var matclicktext = new System.Text.StringBuilder(256);
            sn.DisplayMaterials(matclicktext, 2, historicmats, curmats);

            ExtendedControls.ImageElement.List images = new ExtendedControls.ImageElement.List();

            foreach (KeyValuePair<string, double> sd in sn.Materials)
            {
                Color fillc = Color.Yellow;
                string abv = sd.Key.Substring(0, 1);

                MaterialCommodityMicroResourceType mc = MaterialCommodityMicroResourceType.GetByFDName(sd.Key);

                if (mc != null)
                {
                    abv = mc.Shortname;
                    fillc = mc.Colour;
                    //System.Diagnostics.Debug.WriteLine("Colour {0} {1}", fillc.ToString(), fillc.GetBrightness());

                    if (HideFullMaterials)                 // check full
                    {
                        int? limit = mc.MaterialLimitOrNull();
                        MaterialCommodityMicroResource matnow = curmats?.Find(x => x.Details == mc);  // allow curmats = null

                        // debug if (matnow != null && mc.shortname == "Fe")  matnow.count = 10000;

                        if (matnow != null && limit != null && matnow.Count >= limit)        // and limit
                            continue;
                    }

                    if (ShowOnlyMaterialsRare && mc.IsCommonMaterial)
                        continue;
                }

                System.Drawing.Imaging.ColorMap colormap = new System.Drawing.Imaging.ColorMap();
                colormap.OldColor = Color.White;    // this is the marker colour to replace
                colormap.NewColor = fillc;

                // we clone the image, and colour replace it under GDI lock
                Bitmap mat = BaseUtils.BitMapHelpers.CloneReplaceColourLocked((Bitmap)BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Material"), 
                                                    new System.Drawing.Imaging.ColorMap[] { colormap });

                BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref mat, abv, Font, fillc.GetBrightness() > 0.4f ? Color.Black : Color.White);

                var tooltip = new System.Text.StringBuilder(256);
                sn.DisplayMaterial(tooltip, sd.Key, sd.Value, historicmats, curmats);

                ExtendedControls.ImageElement.Element ie = new ExtendedControls.ImageElement.Element(
                                new Rectangle(matpos.X, matpos.Y, matsize.Width, matsize.Height), mat, tooltip + "\n\n" + "All " + matclicktext.ToString(), tooltip.ToString());
                ie.Name = sd.Key;
                ie.ContextMenuStrip = rightclickmenu;
                images.Add(ie);

                if (++noperline == 4)
                {
                    matpos = new Point(0, matpos.Y + matsize.Height + materiallinespacerxy);
                    noperline = 0;
                }
                else
                    matpos.X += matsize.Width + materiallinespacerxy;
            }

            return images;
        }

        // Create a signals image, with single image at 0,0 centred
        private ExtendedControls.ImageElement.List DrawSignals( List<FSSSignal> signallist,         // may be null
                                                     List<JournalCodexEntry> codex,      // may be null
                                                     List<IBodyFeature> stations,        // may be null
                                                     int height,  ContextMenuStrip rightclickmenu)
        {
            const int maxicons = 5;
            int iconsize = height / maxicons;
            Bitmap bmp = new Bitmap(iconsize, height);

            if (signallist == null)
                signallist = new List<FSSSignal>();     // makes code simpler if we have one

            int[] count = new int[]     // in priority order
            {
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Station).Count(),
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Carrier).Count(),
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.Installation || x.ClassOfSignal == SignalDefinitions.Classification.Megaship).Count(), //before the megaship calssification they were counted as installations, so put them here to not lose the count - might need something better in the future like their own icon
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.NotableStellarPhenomena).Count(),
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.ResourceExtraction).Count(),
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.ConflictZone).Count(),
                signallist.Where(x => x.ClassOfSignal == SignalDefinitions.Classification.USS).Count(),
                0, // 7, slot for others
                0, // 8, slot for codex
            };

            count[7] = signallist.Count - (from x in count select x).Sum();        // set seven to signals left

            count[0] += stations?.Count() ?? 0;         // stations are also present in this entry

            int icons;
            int knockout = 6;       // starting from this signal, work backwards knocking out if required
            while (true)
            {
                icons = (from x in count where x > 0 select 1).Sum();           // how many are set?
                if (icons > maxicons)        // too many
                {
                    count[7] = 1;               // okay set the generic signal one
                    count[knockout--] = 0;      // and knock this out
                }
                else
                    break;
            }

            if (icons < maxicons && codex?.Count > 0)        // if we have space for codex, and we have codex, add in
            {
                icons++;
                count[8] = 1;
            }

            Image[] images = new Image[]
            {
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Stations"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Carriers"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Installations"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.NSP"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.RES"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.CZ"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.USS"),
                BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Signals"),
                BaseUtils.Icons.IconSet.GetIcon("Journal.CodexEntry"),
            };

            int vpos = height / 2 - iconsize * icons / 2;

            using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < count.Length; i++)
                {
                    if (count[i] > 0)
                    {
                        g.DrawImageLocked(images[i], new Rectangle(0, vpos, iconsize, iconsize));
                        vpos += iconsize;
                    }
                }
            }

            string tip = "";

            var notexpired = FSSSignal.NotExpiredSorted(signallist);
            foreach (var sig in notexpired)
                tip = tip.AppendPrePad(sig.ToString(true), Environment.NewLine);

            var expired = FSSSignal.ExpiredSorted(signallist);
            if (expired.Count > 0)
            {
                tip = tip.AppendPrePad("Expired".Tx() + ": ", Environment.NewLine + Environment.NewLine);
                foreach (var sig in expired)
                    tip = tip.AppendPrePad(sig.ToString(true), Environment.NewLine);
            }

            if (codex?.Count > 0)
            {
                tip = tip.AppendPrePad("Codex".Tx() + ":", Environment.NewLine + Environment.NewLine);
                foreach (var c in codex)
                {
                    tip = tip.AppendPrePad(c.Info(), Environment.NewLine);
                }
            }

            return CreateImageAndLabel(bmp, bmp.Size, new string[] { "" }, tip, rightclickmenu);
        }

        // Draw Image and labels below it. Image is owned by us
        // Image is centred at 0,0 co-ords
        // Labels below
        private ExtendedControls.ImageElement.List CreateImageAndLabel( Image image, 
                                                             Size imagesize, 
                                                             string[] labels,  // may be null, no labels
                                                             string tooltiptext,       // tooltip
                                                             ContextMenuStrip rightclickmenu = null,
                                                             Color? backwash = null
                                                            )
        {
            var il = new ExtendedControls.ImageElement.List();

            Rectangle imagebox = new Rectangle(-imagesize.Width / 2, -imagesize.Height / 2, imagesize.Width, imagesize.Height);     // centre on 0,0

            if (backwash.HasValue)  // this is dodgy as its filling source bitmap but it shows rect
            {
                using (Graphics gr = Graphics.FromImage(image))
                {
                    using (Brush b = new SolidBrush(backwash.Value))
                    {
                        gr.FillRectangle(b, new Rectangle(0,0,image.Width,image.Height));
                    }
                }
            }

            ExtendedControls.ImageElement.Element ie = new ExtendedControls.ImageElement.Element(imagebox, image, tooltiptext, tooltiptext, true);
            ie.Name = labels!=null ? labels[0] : "IL";
            il.Add(ie);

            ie.ContextMenuStrip = rightclickmenu;

            if (labels != null)
            {
                int vpos = imagesize.Height / 2;

                foreach (string label in labels)
                {
                    if (label.HasChars())
                    {
                        Font f = Font;
                        int labcut = 0;
                        if (label[0] == '_')
                        {
                            f = FontUnderlined ?? Font;
                            labcut = 1;
                        }

                        var labie = new ExtendedControls.ImageElement.Element();

                        using (var frmt = new StringFormat() { Alignment = StringAlignment.Center })
                        {
                            labie.TextCentreAutoSize(new Point(0, vpos), new Size(0, 1000), label.Substring(labcut), f, TextForeColor, TextBackColor, frmt: frmt);
                        }

                        labie.Name = ie.Name;
                        il.Add(labie);

                        vpos += labie.Bounds.Height;
                    }
                }

                // System.Diagnostics.Debug.WriteLine($"..Create Image and Label {leftmiddle} {size} {labels[0]} shiftright {laboff}");
            }

            //System.Diagnostics.Debug.WriteLine(".. Max " + max);

            return il;
        }
    }
}

