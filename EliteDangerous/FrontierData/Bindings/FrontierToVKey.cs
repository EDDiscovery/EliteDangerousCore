/*
 * Copyright 2016 - 2026 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    static public class FrontierKeyConversion
    {
        static public List<string> FrontierKeyNames()
        {
            var ret = new List<string>();
            for (int i = 0; i < 26; i++)
                ret.Add("Key_" + new string(new char[] { (char)(i + 'A') }));
            for (int i = 0; i < 10; i++)
                ret.Add("Key_" + new string(new char[] { (char)(i + '0') }));
            ret.AddRange(new string[] { "Key_ё", "Key_ґ", "Key_į", "Key_“", "Key_ų", "Key_ė", "Key_č", "Key_š", "Key_ę", "Key_¸", "Key_ş", "Key_ţ" });
            ret.Add("Key_Umlaut");
            ret.Add("Key_Ampersand");
            ret.Add("Key_Acute");
            ret.Add("Key_Apostrophe");
            foreach (var x in frontiertovkeyname)
                ret.Add("Key_" + x.Item2);
            for (int i = 0; i < 10; i++)
                ret.Add("Key_Numpad_" + new string(new char[] { (char)(i + '0') }));
            for (int i = 0; i < 24; i++)
                ret.Add($"Key_F{i}");
            foreach (var x in frontiernameforcharacters)
                ret.Add("Key_" + x.Item1);
            return ret;
        }

        //  Name            Row1                    Row2                        Row3                                Row4
        // Polish           Grave, Minus Equals     LeftBracket RightBracket    SemiColon Apostrophe BackSlash      Backslash Comma Period Slash

        // Translate strange frontier name to vkeys name used by baseutils
        // tested on multiple languages.
        // function returns ! as first character if error occurred
        static public string FrontierToKeys(string frontiername)
        {
            string output;
            string layoutname = InputLanguage.CurrentInputLanguage.LayoutName;

            if (frontiername.StartsWith("Key_"))
            {
                output = frontiername.Substring(4);

                int num;

                // these two languages appear by frontier to use standard names, instead of localised names!
                bool usestdnames = layoutname.Contains("Spanish") || layoutname.Contains("Polish");

                // first simple keys
                if (output.Length == 1 && ((output[0] >= '0' && output[0] <= '9') || (output[0] >= 'A' && output[0] <= 'Z')))
                {
                    // no action - output same as input
                }
                else if (output.StartsWith("Numpad_") && output.Length == 8 && char.IsDigit(output[7]))   // numpad 0-9
                {
                    output = "NumPad" + output[7];
                }
                else if (output.StartsWith("F") && int.TryParse(output.Substring(1), out num))      // F keys
                {
                    output = "F" + num;
                }
                else if (output.Length == 1) // single chars
                {
                    IntPtr layout = BaseUtils.Win32.UnsafeNativeMethods.GetKeyboardLayout(0);
                    short vkey = BaseUtils.Win32.UnsafeNativeMethods.VkKeyScanExW(output[0], layout);        // look up char->vkey

                    if (layoutname == "Ukrainian (Enhanced)")
                    {
                        if (output == "ё")      // frontier is writing this for oem3, which is Ukranian non enhanced, which is not what it is at http://kbdlayout.info/kbdur1 or in real life, fix
                            vkey = (short)Keys.Oem3;
                        else if (output == "ґ") // VKeyScanW does not seem to work with this value
                            vkey = (short)Keys.Oem102;
                    }
                    else if (layoutname == "Lithuanian")
                    {
                        // Frontier are writing out these for the Lithuanian keyboard, even though http://kbdlayout.info/kbdlt1/scancodes and the real keys do not match these
                        // so mangle

                        if (output == "į")
                            vkey = (short)Keys.Oem4;
                        else if (output == "“")
                            vkey = (short)Keys.Oem6;
                        else if (output == "ų")
                            vkey = (short)Keys.Oem1;
                        else if (output == "ė")
                            vkey = (short)Keys.Oem7;
                        else if (output == "č")
                            vkey = (short)Keys.Oemcomma;
                        else if (output == "š")
                            vkey = (short)Keys.OemPeriod;
                        else if (output == "ę")
                            vkey = (short)Keys.Oem2;
                    }
                    else if (layoutname == "Slovenian")
                    {
                        if (output == "¸")
                            vkey = (short)Keys.Oem3;
                    }
                    else if (layoutname == "Romanian (Standard)")
                    {
                        if (output == "ş")
                            vkey = (short)Keys.Oem1;
                        if (output == "ţ")
                            vkey = (short)Keys.Oem7;
                    }

                    if (vkey != -1)
                    {
                        Keys k = (Keys)(vkey & 0xff);
                        // System.Diagnostics.Debug.WriteLine($"Translated thru VkKeyScanEx '{output}' -> {(int)output[0]:x} -> vkey {vkey:x} -> {k} -> {KeyObjectExtensions.VKeyToString(k)}");
                        output = KeyObjectExtensions.VKeyToString(k);
                    }
                    else
                        output = null;
                }
                else if ((num = Array.FindIndex(frontiertovkeyname, x => x.Item2.Equals(output))) >= 0)    // a standard frontier name for a key
                {
                    //System.Diagnostics.Debug.WriteLine($"Translated thru frontiertovkeyname {output} -> {frontiertovkeyname[num].Item1}");
                    output = frontiertovkeyname[num].Item1;
                }
                else if (usestdnames && (num = Array.FindIndex(defaultnamestoscancodes, x => x.Item1.Equals(output))) >= 0)    // if in standard name mode, try that.
                {
                    uint scancode = defaultnamestoscancodes[num].Item2;
                    //System.Diagnostics.Debug.WriteLine($"Translated thru defaultnames {output} -> scancode {scancode:x}");

                    uint v = BaseUtils.Win32.UnsafeNativeMethods.MapVirtualKey(scancode, 3);

                    if (v != 0)
                    {
                        // System.Diagnostics.Debug.WriteLine("        .. {0} -> VK {1:x} {2}", sc, v, ((Keys)v).VKeyToString());
                        output = ((Keys)v).VKeyToString();
                    }
                    else
                        output = null;
                }
                else if ((num = Array.FindIndex(frontiernameforcharacters, x => x.Item1.Equals(output, StringComparison.InvariantCultureIgnoreCase))) >= 0) // try a logical name for a character
                {
                    char ch = frontiernameforcharacters[num].Item2;
                    IntPtr layout = BaseUtils.Win32.UnsafeNativeMethods.GetKeyboardLayout(0);
                    short vkey = BaseUtils.Win32.UnsafeNativeMethods.VkKeyScanExW(ch, layout);

                    if (layoutname == "Czech")     // above seems to fail with Czech, manually fix
                    {
                        if (output == "BackSlash")
                            vkey = (short)Keys.Oem102;
                        else if (output == "Acute")
                            vkey = (short)Keys.Oem2;
                        else if (output == "Equals")
                            vkey = (short)Keys.Oemplus;
                        else if (output == "Umlaut")
                            vkey = (short)Keys.Oem5;
                    }
                    else if (layoutname == "Turkish Q")
                    {
                        if (output == "LessThan")
                            vkey = (short)Keys.Oem102;
                    }
                    else if (layoutname == "Slovak")
                    {
                        if (output == "Ampersand")
                            vkey = (short)Keys.Oem102;
                        else if (output == "Acute")
                            vkey = (short)Keys.Oem8;
                    }
                    else if (layoutname == "Lithuanian")
                    {
                        if (output == "BackSlash")
                            vkey = (short)Keys.Oem102;
                    }
                    else if (layoutname == "Slovenian")
                    {
                        if (output == "LessThan")
                            vkey = (short)Keys.Oem102;
                    }
                    else if (layoutname == "Romanian (Standard)")
                    {
                        if (output == "RightBracket")       // keys produced in romanian standard match the website, the oem codes do, yet frontier produces this strange set
                            vkey = (short)Keys.Oem3;
                        else if (output == "Plus")
                            vkey = (short)Keys.OemMinus;
                        else if (output == "Apostrophe")
                            vkey = (short)Keys.Oemplus;
                        else if (output == "LessThan")
                            vkey = (short)Keys.Oem102;
                        else if (output == "Minus")
                            vkey = (short)Keys.Oem2;
                    }

                    if (vkey != -1)
                    {
                        Keys k = (Keys)(vkey & 0xff);
                        if (k == Keys.Decimal)              // italian returned this, instead of oem period. UK returns period
                            k = Keys.OemPeriod;
                        //System.Diagnostics.Debug.WriteLine($"Translated thru frontiernameforchars VkKeyScanEx {output} -> '{ch}' -> vkey {vkey:x} {k} -> {KeyObjectExtensions.VKeyToString(k)}");
                        output = KeyObjectExtensions.VKeyToString(k);
                    }
                    else
                        output = null;
                }
                else
                    output = null;
            }
            else
                output = null;


            if (output == null)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to convert {frontiername} binding key in lang {layoutname}");
                output = "!Unknown Frontier Key " + frontiername + " in key layout " + layoutname;
            }

            return output;

        }

        // in frontier devices help.txt file inside controlschemes

        static private Tuple<string, string>[] frontiertovkeyname = new Tuple<string, string>[]     
        {
            new Tuple<string,string>(Keys.Escape.VKeyToString()      ,"Escape"),
            // 1-0 handled, minus, equals, 
            new Tuple<string,string>(Keys.Back.VKeyToString(),"Backspace"),
            new Tuple<string,string>(Keys.Tab.VKeyToString(), "Tab"),
            // q-p handled, leftbracket, rightbracket handled
            new Tuple<string,string>(Keys.Return.VKeyToString()      ,"Enter"),
            new Tuple<string,string>(Keys.LControlKey.VKeyToString(),"LeftControl"),
            // a-l, semicolon, apost, grave handled
            new Tuple<string,string>(Keys.LShiftKey.VKeyToString(),"LeftShift"),
            // backslash handled
            // z-m, comma, period, slash handled
            new Tuple<string,string>(Keys.RShiftKey.VKeyToString(),"RightShift"),
            new Tuple<string,string>(Keys.Multiply.VKeyToString()    ,"Numpad_Multiply"),
            new Tuple<string,string>(Keys.LMenu.VKeyToString(),"LeftAlt"),
            new Tuple<string,string>(Keys.Space.VKeyToString(), "Space"),
            new Tuple<string,string>(Keys.Capital.VKeyToString()     ,"CapsLock"),
            // F1-F10 handled
            new Tuple<string,string>(Keys.NumLock.VKeyToString()     ,"NumLock"),
            new Tuple<string,string>(Keys.Scroll.VKeyToString(),"ScrollLock"),
            // numpad 7-9 handled
            new Tuple<string,string>(Keys.Subtract.VKeyToString()    ,"Numpad_Subtract"),
            // numpad 4-6
            new Tuple<string,string>(Keys.Add.VKeyToString()         ,"Numpad_Add"),
            // numpad 1-0
            new Tuple<string,string>(Keys.Decimal.VKeyToString()     ,"Numpad_Decimal"),
            new Tuple<string,string>(Keys.Oem102.VKeyToString(),"OEM_102"),
            // F11-F15
            new Tuple<string,string>(Keys.KanaMode.VKeyToString(),"Kana"),
            // ? ABNT_C1
            new Tuple<string,string>(Keys.IMEConvert.VKeyToString(),"Convert"),
            new Tuple<string,string>(Keys.IMENonconvert.VKeyToString(),"NoConvert"),
            // ? Yen
            // ? ABNT_C2
            // ? Numpad_Equals
            new Tuple<string,string>(Keys.MediaPreviousTrack.VKeyToString()      ,"PrevTrack"),
            // ? AT
            // Colon, Underline handled
            new Tuple<string,string>(Keys.KanjiMode.VKeyToString(),"Kanji"),
            // ? Stop
            // ? AX
            new Tuple<string,string>(Keys.NoName.VKeyToString()      ,"Unlabeled"),
            new Tuple<string,string>(Keys.MediaNextTrack.VKeyToString()      ,"NextTrack"),
            new Tuple<string,string>("NumEnter", "Numpad_Enter"),
            new Tuple<string,string>(Keys.RControlKey.VKeyToString(),"RightControl"),
            new Tuple<string,string>(Keys.VolumeMute.VKeyToString(),"Mute"),
            // ? Calculator
            new Tuple<string,string>(Keys.MediaPlayPause.VKeyToString(),"PlayPause"),
            new Tuple<string,string>(Keys.MediaStop.VKeyToString(),"MediaStop"),
            new Tuple<string,string>(Keys.VolumeDown.VKeyToString(),"VolumeDown"),
            new Tuple<string,string>(Keys.VolumeUp.VKeyToString(),"VolumeUp"),
            new Tuple<string,string>(Keys.BrowserHome.VKeyToString(),"WebHome"),
            // ? Numpad_Comma
            new Tuple<string,string>(Keys.Divide.VKeyToString()      ,"Numpad_Divide"),
            new Tuple<string,string>(Keys.PrintScreen.VKeyToString()      ,"SYSRQ"),
            new Tuple<string,string>(Keys.RMenu.VKeyToString(),"RightAlt"),
            new Tuple<string,string>(Keys.Pause.VKeyToString(),"Pause"),
            new Tuple<string,string>(Keys.Home.VKeyToString()     ,"Home"),
            new Tuple<string,string>(Keys.Up.VKeyToString()          ,"UpArrow"),
            new Tuple<string,string>(Keys.PageUp.VKeyToString()     ,"PageUp"),
            new Tuple<string,string>(Keys.Left.VKeyToString()        ,"LeftArrow"),
            new Tuple<string,string>(Keys.Right.VKeyToString()       ,"RightArrow"),
            new Tuple<string,string>(Keys.End.VKeyToString()     ,"End"),
            new Tuple<string,string>(Keys.Down.VKeyToString()        ,"DownArrow"),
            new Tuple<string,string>(Keys.PageDown.VKeyToString()     ,"PageDown"),
            new Tuple<string,string>(Keys.Insert.VKeyToString()     ,"Insert"),
            new Tuple<string,string>(Keys.Delete.VKeyToString()     ,"Delete"),
            new Tuple<string,string>(Keys.LWin.VKeyToString(),"LeftWin"),
            new Tuple<string,string>(Keys.RWin.VKeyToString(),"RightWin"),
            new Tuple<string,string>(Keys.Apps.VKeyToString(),"Apps"),
            // ?Power
            new Tuple<string,string>(Keys.Sleep.VKeyToString(),"Sleep"),
            // ?Wake
            new Tuple<string,string>(Keys.BrowserSearch.VKeyToString(),"WebSearch"),
            new Tuple<string,string>(Keys.BrowserFavorites.VKeyToString(),"WebFavourites"),
            new Tuple<string,string>(Keys.BrowserRefresh.VKeyToString(),"WebRefresh"),
            new Tuple<string,string>(Keys.BrowserStop.VKeyToString(),"WebStop"),
            new Tuple<string,string>(Keys.BrowserForward.VKeyToString(),"WebForward"),
            new Tuple<string,string>(Keys.BrowserBack.VKeyToString(),"WebBack"),
            // ?MyComputer
            new Tuple<string,string>(Keys.LaunchMail.VKeyToString(),"Mail"),
            new Tuple<string,string>(Keys.SelectMedia.VKeyToString(),"MediaSelect"),
            // ?green modifier
            // ?orange modifer
         };

        static Tuple<string, uint> Create(string name, uint sc)
        {
            return new Tuple<string, uint>(name, sc);
        }

        static Tuple<string, uint>[] defaultnamestoscancodes = new Tuple<string, uint>[]       // used on some layouts instead of local names.. no idea how its chosen
        {
            Create("Grave",0x29),   // uk oem8 

            Create("Minus",0x0c),   // uk oemminus
            Create("Equals",0x0d),  // uk oemplus

            Create("LeftBracket",0x1a), // uk oem4
            Create("RightBracket",0x1b),    // uk oem6

            Create("SemiColon",0x27),   // uk oem1
            Create("Apostrophe",0x28),  // uk oem3
            Create("Hash",0x2b),        // uk oem7

            Create("BackSlash",0x56),   // uk oem5
            Create("Comma",0x33),       // uk oemcomma
            Create("Period",0x34),      // uk oemperiod
            Create("Slash",0x35),       // uk oem2
        };

        static Tuple<string, char> Create(string name, char ch)
        {
            return new Tuple<string, char>(name, ch);
        }

        // logical name frontier uses for characters.. all found by trial and error

        static Tuple<string, char>[] frontiernameforcharacters = new Tuple<string, char>[]      
        {
            Create("SuperscriptTwo",'²'),
            Create("RightParenthesis",')'),
            Create("Circumflex",'^'),
            Create("Dollar",'$'),
            Create("Asterisk",'*'),
            Create("Comma",','),
            Create("SemiColon",';'),
            Create("Colon",':'),
            Create("ExclamationPoint",'!'),
            Create("LessThan",'<'),
            Create("Minus",'-'),
            Create("Period",'.'),
            Create("Hash",'#'),
            Create("Acute",'´'),
            Create("Plus",'+'),
            Create("Grave",'`'),
            Create("Equals",'='),
            Create("LeftBracket",'['),
            Create("RightBracket",']'),
            Create("Apostrophe",'\''),
            Create("BackSlash",'\\'),
            Create("Slash",'/'),
            Create("Tilde",'~'),
            Create("DoubleQuote",'"'),
            Create("LessThan",'<'),
            Create("Umlaut",'¨'),
            Create("Half",'½'),
            Create("Underline",'_'),
            Create("Ampersand",'&'),
        };

#if false
        static private void DumpVK()
        {
            for (int i = 0x20; i < 0x500; i++)        // char->vkey
            {
                IntPtr layout = BaseUtils.Win32.UnsafeNativeMethods.GetKeyboardLayout(0);
                short vkey = BaseUtils.Win32.UnsafeNativeMethods.VkKeyScanExW((char)i, layout);        // look up char->vkey
                System.Diagnostics.Debug.WriteLine($"{i:x} {(char)i} = {vkey:x}");
            }
        }
#endif

    }

}
