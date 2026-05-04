/*
 * Copyright 2026-2026 EDDiscovery development team
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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("")]
    public class ShipModuleDisplay
    {
        #region Information interface

        public Font Font { get; set; } = null;              // must be set before call
        public Font FontLarge { get; set; } = null;              // only for titles
        public Color TextBackColor { get; set; } = Color.Transparent;
        public Color TextForeColor { get; set; } = Color.White;
        public Color BoxBorderColor { get; set; } = Color.DarkOrange;
        public Color BoxBackColor1 { get; set; } = Color.Gray;
        public Color BoxBackColor2 { get; set; } = Color.DarkGray;
        public float GradientDirection { get; set; } = 90F;
        public float DisabledPercentage { get; set; } = 70F;
        public Size BoxSize { get; set; } = new Size(300, 48);
        public Size BoxSpacing { get; set; } = new Size(16, 8);

        // in order
        public bool DisplayDPS { get; set; } = false;
        public bool DisplayAmmo { get; set; } = false;
        public bool DisplayMW { get; set; } = false;
        public bool DisplayHealth { get; set; } = true;
        public bool DisplayPriority { get; set; } = true;

        #endregion

        #region Implementation

        public void DrawRender(ExtPictureBox imagebox, ItemData.ShipProperties ship, Ship instanceship, int widthavailable = 1920, string titletext = null )
        {
            imagebox.ClearImageList();
            var list = CreateImages(ship, instanceship, new Point(0, 0), widthavailable, titletext);
            imagebox.AddRange(list);
            imagebox.Render();
        }

        // draw ship
        // shipinstance can be null if you don't have a ship
        public ExtendedControls.ImageElement.List CreateImages(ItemData.ShipProperties ship, Ship instanceship, Point startpoint, int widthavailable = 1920, string titletext = null, bool tagitems=true)
        {
            System.Diagnostics.Debug.Assert(Font != null);
            System.Diagnostics.Debug.Assert(!BoxSize.IsEmpty);

            ExtendedControls.ImageElement.List imageList = new ExtendedControls.ImageElement.List();    // final image list to report

            int yoffset = 0;

            if (titletext.HasChars())
            {
                System.Diagnostics.Debug.Assert(FontLarge != null);
                var lab = new ExtendedControls.ImageElement.Element();
                lab.TextAutoSize(new Point(startpoint.X, startpoint.Y), new Size(500, 30), titletext, FontLarge, TextForeColor, TextBackColor);
                imageList.Add(lab);
                yoffset += lab.Image.Height + 8;
            }

            int initialx = startpoint.X;

            var h = DrawState(ship.Hardpoints, startpoint ,instanceship, tagitems);
            imageList.AddRange(h);
            startpoint.X += BoxSize.Width + BoxSpacing.Width;
            if (startpoint.X + BoxSize.Width > widthavailable)
                startpoint = new Point(initialx, imageList.Max.Y + BoxSpacing.Height*2);

            var c = DrawState(ship.Component, startpoint, instanceship, tagitems);
            imageList.AddRange(c);
            startpoint.X += BoxSize.Width + BoxSpacing.Width;
            if (startpoint.X + BoxSize.Width > widthavailable)
                startpoint = new Point(initialx, imageList.Max.Y + BoxSpacing.Height*2);

            var i = DrawState(ship.Internal, startpoint, instanceship, tagitems);
            imageList.AddRange(i);
            startpoint.X += BoxSize.Width + BoxSpacing.Width;
            if (startpoint.X + BoxSize.Width > widthavailable)
                startpoint = new Point(initialx, imageList.Max.Y + BoxSpacing.Height*2);

            var m = DrawState(ship.Military, startpoint, instanceship, tagitems);
            if (m.Count > 0)
            {
                startpoint.Y = m.Max.Y + BoxSpacing.Height * 2 + BoxSize.Height;
                imageList.AddRange(m);
            }
            var u = DrawState(ship.Utility, startpoint, instanceship, tagitems);
            imageList.AddRange(u);

            return imageList;
        }

        // Draw slots in Array
        // shipinstance can be null if you don't have a ship
        private ExtendedControls.ImageElement.List DrawState(ShipSlots.SlotAndSize[] array, Point startpoint, Ship shipinstance, bool tagitems)
        {
            ExtendedControls.ImageElement.List il = new ExtendedControls.ImageElement.List();

            foreach (var slsz in array)
            {
                Bitmap bmp = new Bitmap(BoxSize.Width, BoxSize.Height);

                int iconsize = Font.Height * 8 / 8;
                int topbot = iconsize + 4;
                Rectangle boxarea = new Rectangle(new Point(0, 0), BoxSize);
                Rectangle border = new Rectangle(new Point(0, 0), new Size(BoxSize.Width - 1, BoxSize.Height - 1));

                int leftx = Font.Height * 3 / 2;
                int rightsplit = BoxSize.Width - iconsize - 2;

                Rectangle slottypearea = new Rectangle(new Point(0, 0), new Size(leftx-2, BoxSize.Height));
                Rectangle toptextarea = new Rectangle(new Point(leftx, 0), new Size(rightsplit - leftx, topbot));
                Rectangle bottextarea = new Rectangle(new Point(leftx, BoxSize.Height - topbot), new Size(rightsplit - leftx, topbot));
                Rectangle midtextarea = new Rectangle(new Point(leftx, toptextarea.Bottom), new Size(rightsplit - leftx, bottextarea.Top- toptextarea.Bottom));

                ShipModule module = shipinstance?.GetModuleInSlot(slsz.Slot);      // may be null
                ItemData.ShipModule engmod = module?.GetModuleEngineered(out string report); // may be null

                string tooltip = null;

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (var b = new LinearGradientBrush(boxarea, BoxBackColor1, BoxBackColor2, GradientDirection))
                    {
                        g.FillRectangle(b, boxarea);       // linear grad brushes do not respect smoothing mode, btw
                    }

                    g.SmoothingMode = SmoothingMode.None;

                    using (Pen pen = new Pen(BoxBorderColor, 1))
                    {
                        g.DrawRectangle(pen, border);
                        g.DrawLine(pen, new Point(slottypearea.Right, 0), new Point(slottypearea.Right, BoxSize.Height - 1));
                    }

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    Brush textbrush = new SolidBrush(TextForeColor);
                    Brush textbrushdisabled = new SolidBrush(TextForeColor.Multiply(module?.Enabled == false ? DisabledPercentage / 100.0F : 1.0F));

                    using (StringFormat ftext = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        string text = slsz.IsHardpoint || slsz.IsUtility ? "USMLH"[slsz.Size].ToString() : slsz.Size.ToString();
                        if (slsz.IsPassenger)
                            text += "P";
                        else if (slsz.IsMilitary)
                            text += "M";
                        g.DrawString(text, Font, textbrush, slottypearea, ftext);
                    }
                    using (StringFormat fleft = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    {
                        if (module != null)
                        {
                            {
                                string txt = "";
                                if (engmod.Class != null)
                                    txt += $"{engmod.Class} ";
                                if (engmod.Rating != null)
                                    txt += $"{engmod.Rating} ";
                                txt += $"{engmod.TranslatedShortModName}";

                                Image hpt = engmod.Mount == "F" ? BaseUtils.Icons.IconSet.GetImage($"Controls.Fixed48") :
                                            engmod.Mount == "G" ? BaseUtils.Icons.IconSet.GetImage($"Controls.Gimbal48") :
                                            engmod.Mount == "T" ? BaseUtils.Icons.IconSet.GetImage($"Controls.Turret48") : null;

                                int xoff = midtextarea.X;
                                if (hpt != null)
                                {
                                    g.DrawImage(hpt, new Rectangle(midtextarea.X, midtextarea.Y + midtextarea.Height / 2 - iconsize / 2, iconsize, iconsize), new Rectangle(0, 0, hpt.Width, hpt.Height), GraphicsUnit.Pixel);
                                    xoff += iconsize + 2;
                                }

                                g.DrawString(txt, Font, textbrush, new Rectangle(xoff, midtextarea.Y, midtextarea.Width - xoff, midtextarea.Height), fleft);
                            }
                            // use Module.engineering.Build() for tooltip
                            System.Diagnostics.Debug.WriteLine($"Module {engmod.EnglishModName} in {slsz.Slot} : `{engmod.TranslatedShortModName}` vs `{engmod.TranslatedModName}` P{module.Priority} {module.Health}%");

                            if (module.Engineering != null)
                            {
                                int xoff = bottextarea.X;
                                var ei = BaseUtils.Icons.IconSet.GetImage($"Controls.Engineered48");
                                g.DrawImage(ei, new Rectangle(xoff, bottextarea.Y + bottextarea.Height / 2 - iconsize / 2, iconsize, iconsize), new Rectangle(0, 0, ei.Width, ei.Height), GraphicsUnit.Pixel);
                                xoff += iconsize + 2;
                                string txt = $"{module.Engineering.Level}: {module.Engineering.FriendlyBlueprintName}";
                                g.DrawString(txt, Font, textbrush, new Rectangle(xoff, bottextarea.Y, bottextarea.Width - xoff, bottextarea.Height), fleft);
                            }

                            if (module.Enabled == true)
                            {
                                var ei = BaseUtils.Icons.IconSet.GetImage($"Controls.PowerOn48");
                                g.DrawImage(ei, new Rectangle(rightsplit, toptextarea.Height / 2 - iconsize / 2, iconsize, iconsize), new Rectangle(0, 0, ei.Width, ei.Height), GraphicsUnit.Pixel);
                            }

                            using (StringFormat fright = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
                            {
                                string txt = "";

                                if (DisplayDPS && engmod?.DPS > 0)
                                    txt += $"DPS {engmod.DPS:0.0}/s ";
                                if (DisplayAmmo && engmod?.Ammo > 0)
                                    txt += $"{engmod.Clip}/{engmod.Ammo} ";
                                if (DisplayMW && engmod?.PowerDraw > 0)
                                    txt += $"{engmod.PowerDraw:0.0}MW ";
                                if (DisplayHealth && module.Health.HasValue)
                                    txt += $"{module.Health}% ";
                                if (DisplayPriority && module.Priority.HasValue && slsz.HasPriority)
                                    txt += $"P{module.Priority + 1} ";

                                g.DrawString(txt, Font, textbrushdisabled, toptextarea, fright);
                            }

                            if (engmod != null)     // must know about the module
                            {
                                tooltip = engmod.ToString(" " + Environment.NewLine);
                                if (module.Engineering != null)
                                {
                                    System.Text.StringBuilder sbtt = new System.Text.StringBuilder(1024);
                                    module.Engineering.Build(sbtt);
                                    tooltip += Environment.NewLine + sbtt.ToString();
                                }
                            }
                        }
                        else
                        {
                            if (shipinstance != null)
                            {
                                if ( shipinstance.Modules.Count == 0)
                                    g.DrawString($"Unknown", Font, textbrush, midtextarea, fleft);
                                else
                                    g.DrawString($"Empty", Font, textbrush, midtextarea, fleft);
                            }
                            else
                            {
                                g.DrawString(ShipSlots.ToLocalisedLanguage(slsz.Slot), Font, textbrush, midtextarea, fleft);
                            }
                        }
                    }

                    textbrush.Dispose();
                    textbrushdisabled.Dispose();
                }

                // image tag for clicking on is the slot ID
                object tag = null;
                if (tagitems)
                    tag = slsz.Slot;
                il.Add(new ExtendedControls.ImageElement.Element(new Rectangle(startpoint,BoxSize), bmp, tag, tooltip));

                startpoint.Y += boxarea.Height + BoxSpacing.Height;
            }

            return il;
        }

        // output the image to a file (or just create it if path=null)
        public static void DrawToFile(string path, ItemData.ShipProperties ship, Ship instance, int width = 1920 , string title = null)
        {
            ShipModuleDisplay md = new ShipModuleDisplay();
            md.Font = new System.Drawing.Font("Arial", 8);
            md.FontLarge = new System.Drawing.Font("Arial", 12);

            var il = md.CreateImages(ship, instance, new Point(0,0), width, title);

            if ( path != null )
            {
                using (var bmp = il.Paint(Color.AliceBlue))
                {
                    bmp.Save(path);
                }
            }
        }

        #endregion
    }
}

