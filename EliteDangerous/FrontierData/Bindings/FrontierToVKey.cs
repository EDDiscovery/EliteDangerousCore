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
using System.Web.Caching;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    /// <summary>
    /// This works in the current locale 
    /// working in other locales is dependent on the user installing the language packs, you just can't order up a locale and use the WIN32 api on it
    /// the pack needs to be installed.
    /// Hence its not possible to send in an arbitary locale to use
    /// </summary>
    static public class FrontierKeyConversion
    {
        // extracted frontier key names for key languages
        static public List<string> FrontierKeyNames(bool inclalphanumbersfkeys = true)
        {
            var ret = new List<string>();
            if (inclalphanumbersfkeys)
            {
                for (int i = 0; i < 26; i++)
                    ret.Add("Key_" + new string(new char[] { (char)(i + 'A') }));
                for (int i = 0; i < 10; i++)
                    ret.Add("Key_" + new string(new char[] { (char)(i + '0') }));
            }

            foreach (var x in frontiertovkeyname)
                ret.Add("Key_" + x.Item2);

            for (int i = 0; i < 10; i++)
                ret.Add("Key_Numpad_" + new string(new char[] { (char)(i + '0') }));

            if (inclalphanumbersfkeys)
            {
                for (int i = 0; i <= 24; i++)
                    ret.Add($"Key_F{i}");
            }

            string layoutname = InputLanguage.CurrentInputLanguage.LayoutName;

            // worked out from unittests binding checks in UnitTestFrontierKeys.cs - it collated a list of missing keys from the tests performed by the script

            if (layoutname == "United Kingdom")
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_SemiColon", "Key_Apostrophe", "Key_Hash", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Slash", });
            }
            else if (layoutname == "Czech")     
            {
                ret.AddRange(new string[] { "Key_ú", "Key_ů", "Key_§" , "Key_SemiColon", "Key_Equals", "Key_Acute", "Key_RightParenthesis",
                "Key_Umlaut","Key_BackSlash","Key_Comma", "Key_Period","Key_Minus"
                });
            }

            if (layoutname.Contains("Portuguese (Brazil ABNT)"))
            {
                ret.AddRange(new string[] { "Key_Apostrophe", "Key_Minus", "Key_Equals", "Key_Acute", "Key_LeftBracket", "Key_ç", "Key_Tilde", "Key_RightBracket", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_SemiColon", });
            }
            else if (layoutname.Contains("Portuguese"))
            {
                ret.AddRange(new string[] { "Key_BackSlash", "Key_Apostrophe", "Key_«", "Key_Plus", "Key_Acute", "Key_ç", "Key_º", "Key_Tilde", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Danish"))
            {
                ret.AddRange(new string[] { "Key_Half", "Key_Plus", "Key_Acute", "Key_å", "Key_Umlaut", "Key_æ", "Key_ø", "Key_Apostrophe", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("German"))
            {
                ret.AddRange(new string[] { "Key_Circumflex", "Key_ß", "Key_Acute", "Key_ü", "Key_Plus", "Key_ö", "Key_ä", "Key_Hash", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Greek"))
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_΄", "Key_Apostrophe", "Key_BackSlash", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Slash", });
            }
            else if (layoutname.Contains("US"))
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_SemiColon", "Key_Apostrophe", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Slash", });
            }
            else if (layoutname.Contains("Finnish"))
            {
                ret.AddRange(new string[] { "Key_§", "Key_Plus", "Key_Acute", "Key_å", "Key_Umlaut", "Key_ö", "Key_ä", "Key_Apostrophe", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("French"))
            {
                ret.AddRange(new string[] { "Key_SuperscriptTwo", "Key_RightParenthesis", "Key_Equals", "Key_Circumflex", "Key_Dollar", "Key_ù", "Key_Asterisk", "Key_LessThan", "Key_Comma", "Key_SemiColon", "Key_Colon", "Key_ExclamationPoint", });
            }
            else if (layoutname.Contains("Italian"))
            {
                ret.AddRange(new string[] { "Key_BackSlash", "Key_Apostrophe", "Key_ì", "Key_è", "Key_Plus", "Key_ò", "Key_à", "Key_ù", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Polish"))
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_SemiColon", "Key_Apostrophe", "Key_Hash", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Slash", });
            }
            else if (layoutname.Contains("Romanian"))
            {
                ret.AddRange(new string[] { "Key_RightBracket", "Key_Plus", "Key_Apostrophe", "Key_ă", "Key_î", "Key_ş", "Key_ţ", "Key_â", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Slovak"))
            {
                ret.AddRange(new string[] { "Key_SemiColon", "Key_Equals", "Key_Acute", "Key_ú", "Key_ä", "Key_ô", "Key_§", "Key_ň", "Key_Ampersand", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Swedish"))
            {
                ret.AddRange(new string[] { "Key_§", "Key_Plus", "Key_Acute", "Key_å", "Key_Umlaut", "Key_ö", "Key_ä", "Key_Apostrophe", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Turkish"))
            {
                ret.AddRange(new string[] { "Key_DoubleQuote", "Key_Asterisk", "Key_Minus", "Key_ğ", "Key_ü", "Key_ş", "Key_Comma", "Key_LessThan", "Key_ö", "Key_ç", "Key_Period", });
            }
            else if (layoutname.Contains("Ukrainian"))
            {
                ret.AddRange(new string[] { "Key_ё", "Key_Minus", "Key_Equals", "Key_х", "Key_ї", "Key_ж", "Key_є", "Key_BackSlash", "Key_ґ", "Key_б", "Key_ю", "Key_Period", });
            }
            else if (layoutname.Contains("Slovenian"))
            {
                ret.AddRange(new string[] { "Key_¸", "Key_Apostrophe", "Key_Plus", "Key_š", "Key_đ", "Key_č", "Key_ć", "Key_ž", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Lithuanian"))
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Underline", "Key_Plus", "Key_į", "Key_“", "Key_ų", "Key_ė", "Key_|", "Key_BackSlash", "Key_č", "Key_š", "Key_ę", });
            }
            else if (layoutname.Contains("Norwegian"))
            {
                ret.AddRange(new string[] { "Key_|", "Key_Plus", "Key_BackSlash", "Key_å", "Key_Umlaut", "Key_ø", "Key_æ", "Key_Apostrophe", "Key_LessThan", "Key_Comma", "Key_Period", "Key_Minus", });
            }
            else if (layoutname.Contains("Spanish"))
            {
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_SemiColon", "Key_Apostrophe", "Key_Hash", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Slash", });
            }
            else
            {
                // default set might not work
                ret.AddRange(new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_SemiColon", "Key_Apostrophe", "Key_BackSlash", "Key_Comma", "Key_Period", "Key_Slash", });
            }

            return ret;
        }

        // try and translate from vkeyname to frontier.
        static public string KeysToFrontier(string vkeyname)
        {
            if (vkeyname.Length == 1)
            {
                if (char.IsLetter(vkeyname[0]) || char.IsDigit(vkeyname[0]))
                    return "Key_" + vkeyname[0];
            }
            else if ( vkeyname.StartsWith("F") && int.TryParse(vkeyname.Substring(1), out int num))
            {
                return "Key_F" + num.ToStringInvariant();
            }
            else
            {
                var list = FrontierKeyNames(false);     // language specific, not A-Z 0-9 or F1-F24

                foreach (var x in list )       // manual search reverse lookup
                {
                    string vn = FrontierKeyConversion.FrontierToKeys(x);
                    if (vn == vkeyname)
                        return x;
                }
            }

            return "!No mapping from Frontier name to vkey name for " + vkeyname;
        }

        public static Dictionary<string, string> vkeytable = new Dictionary<string, string>();

        // Translate strange frontier name to vkeys name used by baseutils/winforms keys
        // tested on multiple languages.
        // function returns ! as first character if error occurred
        static public string FrontierToKeys(string frontiername)
        {
            string output;
            var y = InputLanguage.CurrentInputLanguage;
            string layoutname = InputLanguage.CurrentInputLanguage.LayoutName;

            if (frontiername.StartsWith("Key_"))
            {
                output = frontiername.Substring(4);

                int num;

                // these two languages appear by frontier to use standard names, instead of localised names!
                bool usestdnames = layoutname.Contains("Spanish") || layoutname.Contains("Polish");
             //   usestdnames = true;

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
                    output = "F" + num.ToStringInvariant();
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

                // is it a standard frontier name for a key
                else if ((num = Array.FindIndex(frontiertovkeyname, x => x.Item2.Equals(output))) >= 0)   
                {
                    //System.Diagnostics.Debug.WriteLine($"Translated thru frontiertovkeyname {output} -> {frontiertovkeyname[num].Item1}");
                    output = frontiertovkeyname[num].Item1;
                }

                // some languages uses standard name mode, try that.
                else if (usestdnames && (num = Array.FindIndex(defaultnamestoscancodes, x => x.Item1.Equals(output))) >= 0)
                {
                    //System.Diagnostics.Debug.WriteLine($"Needed to use function on {layoutname} {output}");

                    uint scancode = defaultnamestoscancodes[num].Item2;
                    System.Diagnostics.Debug.WriteLine($"Translated thru defaultnames {output} -> scancode {scancode:x}");

                    uint v = BaseUtils.Win32.UnsafeNativeMethods.MapVirtualKey(scancode, 3);

                    if (v != 0)
                    {
                        //System.Diagnostics.Debug.WriteLine("        .. {0} -> VK {1:x} {2}", scancode, v, ((Keys)v).VKeyToString());
                        output = ((Keys)v).VKeyToString();
                    }
                    else
                        output = null;
                }
 
                // try a logical name for a character
                else if ((num = Array.FindIndex(frontiernameforcharacters, x => x.Item1.Equals(output, StringComparison.InvariantCultureIgnoreCase))) >= 0) 
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
                //System.Diagnostics.Trace.WriteLine($"FrontierToVKey Failed to convert {frontiername} binding key in lang {layoutname}");
                output = "!Unknown Frontier Key " + frontiername + " in key layout " + layoutname;
            }
            else
            {
                vkeytable[frontiername] = output;
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
/// not used           new Tuple<string,string>(Keys.Oem102.VKeyToString(),"OEM_102"),
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

        // used on some layouts instead of local names.. no idea how its chosen

        static Tuple<string, uint>[] defaultnamestoscancodes = new Tuple<string, uint>[]
        {
            Tuple.Create("Grave",0x29u),   // uk oem8 

            Tuple.Create("Minus",0x0cu),   // uk oemminus
            Tuple.Create("Equals",0x0du),  // uk oemplus

            Tuple.Create("LeftBracket",0x1au), // uk oem4
            Tuple.Create("RightBracket",0x1bu),    // uk oem6

            Tuple.Create("SemiColon",0x27u),   // uk oem1
            Tuple.Create("Apostrophe",0x28u),  // uk oem3
            Tuple.Create("Hash",0x2bu),        // uk oem7

            Tuple.Create("BackSlash",0x56u),   // uk oem5
            Tuple.Create("Comma",0x33u),       // uk oemcomma
            Tuple.Create("Period",0x34u),      // uk oemperiod
            Tuple.Create("Slash",0x35u),       // uk oem2
        };

        static Tuple<string, Keys>[] defaultnamestovkey = new Tuple<string, Keys>[]
        {
            Tuple.Create("Grave",Keys.Oem8),   // uk oem8 

            Tuple.Create("Minus",Keys.OemMinus),   // uk oemminus
            Tuple.Create("Equals",Keys.Oemplus),  // uk oemplus

            Tuple.Create("LeftBracket",Keys.Oem4), // uk oem4
            Tuple.Create("RightBracket",Keys.Oem6),    // uk oem6

            Tuple.Create("SemiColon",Keys.Oem1),   // uk oem1
            Tuple.Create("Apostrophe",Keys.Oem3),  // uk oem3
            Tuple.Create("Hash",Keys.Oem7),        // uk oem7

            Tuple.Create("BackSlash",Keys.Oem5),   // uk oem5
            Tuple.Create("Comma",Keys.Oemcomma),       // uk oemcomma
            Tuple.Create("Period",Keys.OemPeriod),      // uk oemperiod
            Tuple.Create("Slash",Keys.Oem2),       // uk oem2
        };

        // logical name frontier uses for characters.. all found by trial and error
        static Tuple<string, char>[] frontiernameforcharacters = new Tuple<string, char>[]      
        {
            Tuple.Create("SuperscriptTwo",'²'),
            Tuple.Create("RightParenthesis",')'),
            Tuple.Create("Circumflex",'^'),
            Tuple.Create("Dollar",'$'),
            Tuple.Create("Asterisk",'*'),
            Tuple.Create("Comma",','),
            Tuple.Create("SemiColon",';'),
            Tuple.Create("Colon",':'),
            Tuple.Create("ExclamationPoint",'!'),
            Tuple.Create("LessThan",'<'),
            Tuple.Create("Minus",'-'),
            Tuple.Create("Period",'.'),
            Tuple.Create("Hash",'#'),
            Tuple.Create("Acute",'´'),
            Tuple.Create("Plus",'+'),
            Tuple.Create("Grave",'`'),
            Tuple.Create("Equals",'='),
            Tuple.Create("LeftBracket",'['),
            Tuple.Create("RightBracket",']'),
            Tuple.Create("Apostrophe",'\''),
            Tuple.Create("BackSlash",'\\'),
            Tuple.Create("Slash",'/'),
            Tuple.Create("Tilde",'~'),
            Tuple.Create("DoubleQuote",'"'),
            Tuple.Create("LessThan",'<'),
            Tuple.Create("Umlaut",'¨'),
            Tuple.Create("Half",'½'),
            Tuple.Create("Underline",'_'),
            Tuple.Create("Ampersand",'&'),
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
