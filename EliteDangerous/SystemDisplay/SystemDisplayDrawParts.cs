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

using EliteDangerousCore.JournalEvents;
using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class SystemDisplay
    {
        // curmats may be null
        private Point CreateMaterialNodes(List<ExtPictureBox.ImageElement> pc, JournalScan sn, List<MaterialCommodityMicroResource> historicmats, List<MaterialCommodityMicroResource> curmats,
                                Point matpos, Size matsize)
        {
            Point startpos = matpos;
            Point maximum = matpos;
            int noperline = 0;

            var matclicktext = new System.Text.StringBuilder(256);
            sn.DisplayMaterials(matclicktext, 2, historicmats, curmats);

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
                        int? limit = mc.MaterialLimit();
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

                Bitmap mat = BaseUtils.BitMapHelpers.ReplaceColourInBitmap((Bitmap)BaseUtils.Icons.IconSet.GetIcon("Controls.Scan.Bodies.Material"), new System.Drawing.Imaging.ColorMap[] { colormap });

                BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref mat, abv, Font, fillc.GetBrightness() > 0.4f ? Color.Black : Color.White);

                var tooltip = new System.Text.StringBuilder(256);
                sn.DisplayMaterial(tooltip, sd.Key, sd.Value, historicmats, curmats);

                ExtPictureBox.ImageElement ie = new ExtPictureBox.ImageElement(
                                new Rectangle(matpos.X, matpos.Y, matsize.Width, matsize.Height), mat, tooltip + "\n\n" + "All " + matclicktext.ToString(), tooltip.ToString());

                pc.Add(ie);

                maximum = new Point(Math.Max(maximum.X, matpos.X + matsize.Width), Math.Max(maximum.Y, matpos.Y + matsize.Height));

                if (++noperline == 4)
                {
                    matpos = new Point(startpos.X, matpos.Y + matsize.Height + materiallinespacerxy);
                    noperline = 0;
                }
                else
                    matpos.X += matsize.Width + materiallinespacerxy;
            }

            return maximum;
        }

        // Create a signals list
        private Point DrawSignals(List<ExtPictureBox.ImageElement> pc, Point leftmiddle,
                                List<JournalFSSSignalDiscovered> listfsd, List<JournalCodexEntry> codex,
                                int height, int shiftrightifreq)
        {
            const int maxicons = 5;
            int iconsize = height / maxicons;
            Bitmap bmp = new Bitmap(iconsize, height);

            var signallist = JournalFSSSignalDiscovered.SignalList(listfsd);

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

            if (icons < maxicons && codex.Count > 0)        // if we have space for codex, and we have codex, add in
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
                //    g.Clear(Color.FromArgb(20, 64, 64)); // debug
                for (int i = 0; i < count.Length; i++)
                {
                    if (count[i] > 0)
                    {
                        g.DrawImage(images[i], new Rectangle(0, vpos, iconsize, iconsize));
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
                tip = tip.AppendPrePad("Expired:".T(EDCTx.UserControlScan_Expired), Environment.NewLine + Environment.NewLine);
                foreach (var sig in expired)
                    tip = tip.AppendPrePad(sig.ToString(true), Environment.NewLine);
            }

            if (codex.Count > 0)
            {
                tip = tip.AppendPrePad("Codex".T(EDCTx.ScanDisplayUserControl_Codex) + ":", Environment.NewLine + Environment.NewLine);
                foreach (var c in codex)
                {
                    tip = tip.AppendPrePad(c.Info(), Environment.NewLine);
                }
            }

            if (icons > 4)
                leftmiddle.X += shiftrightifreq;

            return CreateImageAndLabel(pc, bmp, leftmiddle, bmp.Size, false, out Rectangle _, new string[] { "" }, tip, false);
        }


        // plot at leftmiddle the image of size, return bot left accounting for label 
        // label can be null.
        // returns max point and in imageloc the area drawn
        // you can shift right if required so you don't clip right

        private Point CreateImageAndLabel(List<ExtPictureBox.ImageElement> c, Image i, Point leftmiddle, Size size, bool shiftrightifneeded, out Rectangle imageloc,
                                    string[] labels, string ttext, bool imgowned = true, Color? backwash = null)
        {

            ExtPictureBox.ImageElement ie = new ExtPictureBox.ImageElement(new Rectangle(leftmiddle.X, leftmiddle.Y - size.Height / 2, size.Width, size.Height), i, ttext, ttext, imgowned);

            if (backwash.HasValue)  // this is dodgy as its filling source bitmap but it shows rect
            {
                using( Graphics gr = Graphics.FromImage(ie.Image))
                {
                    using (Brush b = new SolidBrush(backwash.Value))
                    {
                        gr.FillRectangle(b, new Rectangle(0, 0, ie.Image.Width,ie.Image.Height));
                    }
                }
            }

            Point max = new Point(leftmiddle.X + size.Width, leftmiddle.Y + size.Height / 2);

            var labelie = new List<ExtPictureBox.ImageElement>();
            int laboff = 0;
            int vpos = leftmiddle.Y + size.Height / 2;

            foreach (string label in labels)
            {
                if (label.HasChars())
                {
                    Font f = Font;
                    int labcut = 0;
                    if (label[0] == '_')
                    {
                        f = FontUnderlined;
                        labcut = 1;
                    }

                    Point labposcenthorz = new Point(leftmiddle.X + size.Width / 2, vpos);

                    ExtPictureBox.ImageElement labie = new ExtPictureBox.ImageElement();

                    using (var frmt = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        labie.TextCentreAutoSize(labposcenthorz, new Size(0, 1000), label.Substring(labcut), f, LabelColor, BackColor, frmt: frmt);
                    }

                    labelie.Add(labie);

                    // if we are allowed, and we are left of the instructed position in the label, set the laboff

                    if (shiftrightifneeded && labie.Location.X < leftmiddle.X)      
                        laboff = Math.Max(laboff, leftmiddle.X - labie.Location.X);

                    vpos += labie.Location.Height;
                }
            }

           // System.Diagnostics.Debug.WriteLine($"..Create Image and Label {leftmiddle} {size} {labels[0]} shiftright {laboff}");

            foreach (var l in labelie)
            {
                l.Translate(laboff, 0);
                c.Add(l);
                max = new Point(Math.Max(max.X, l.Location.Right), Math.Max(max.Y, l.Location.Bottom));
            }

            ie.Translate(laboff, 0);
            max = new Point(Math.Max(max.X, ie.Location.Right), Math.Max(max.Y, ie.Location.Bottom));
            c.Add(ie);

            imageloc = ie.Location;

            //System.Diagnostics.Debug.WriteLine(".. Max " + max);

            return max;
        }



    }
}

