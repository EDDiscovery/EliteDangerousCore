/*
 * Copyright 2016 - 2022 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    static public class FrontierKeyConversion
    {
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

        static Tuple<string, string>[] frontiertovkeyname = new Tuple<string, string>[]     
        {
            new Tuple<string,string>(Keys.Escape.VKeyToString()      ,"Escape"),
            new Tuple<string,string>(Keys.Up.VKeyToString()          ,"UpArrow"),
            new Tuple<string,string>(Keys.Down.VKeyToString()        ,"DownArrow"),
            new Tuple<string,string>(Keys.Left.VKeyToString()        ,"LeftArrow"),
            new Tuple<string,string>(Keys.Right.VKeyToString()       ,"RightArrow"),
            new Tuple<string,string>(Keys.Return.VKeyToString()      ,"Enter"),
            new Tuple<string,string>(Keys.Capital.VKeyToString()     ,"CapsLock"),
            new Tuple<string,string>(Keys.NumLock.VKeyToString()     ,"NumLock"),
            new Tuple<string,string>(Keys.Subtract.VKeyToString()    ,"Numpad_Subtract"),
            new Tuple<string,string>(Keys.Divide.VKeyToString()      ,"Numpad_Divide"),
            new Tuple<string,string>(Keys.Multiply.VKeyToString()    ,"Numpad_Multiply"),
            new Tuple<string,string>(Keys.Add.VKeyToString()         ,"Numpad_Add"),
            new Tuple<string,string>(Keys.Decimal.VKeyToString()     ,"Numpad_Decimal"),
            new Tuple<string,string>(Keys.Insert.VKeyToString()     ,"Insert"),
            new Tuple<string,string>(Keys.Home.VKeyToString()     ,"Home"),
            new Tuple<string,string>(Keys.PageUp.VKeyToString()     ,"PageUp"),
            new Tuple<string,string>(Keys.Delete.VKeyToString()     ,"Delete"),
            new Tuple<string,string>(Keys.End.VKeyToString()     ,"End"),
            new Tuple<string,string>(Keys.PageDown.VKeyToString()     ,"PageDown"),
            new Tuple<string,string>("NumEnter", "Numpad_Enter"),
            new Tuple<string,string>(Keys.Space.VKeyToString(), "Space"),
            new Tuple<string,string>(Keys.Tab.VKeyToString(), "Tab"),
            new Tuple<string,string>(Keys.LShiftKey.VKeyToString(),"LeftShift"),
            new Tuple<string,string>(Keys.LControlKey.VKeyToString(),"LeftControl"),
            new Tuple<string,string>(Keys.LMenu.VKeyToString(),"LeftAlt"),
            new Tuple<string,string>(Keys.RShiftKey.VKeyToString(),"RightShift"),
            new Tuple<string,string>(Keys.RControlKey.VKeyToString(),"RightControl"),
            new Tuple<string,string>(Keys.RMenu.VKeyToString(),"RightAlt"),
            new Tuple<string,string>(Keys.Back.VKeyToString(),"Backspace"),
            new Tuple<string,string>(Keys.Scroll.VKeyToString(),"ScrollLock"),
            new Tuple<string,string>(Keys.Pause.VKeyToString(),"Pause"),
            new Tuple<string,string>(Keys.LWin.VKeyToString(),"LeftWin"),
            new Tuple<string,string>(Keys.RWin.VKeyToString(),"RightWin"),
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
        // april 6/7/8 '22 coded

        public static void Check()
        {
            InputLanguage defl = InputLanguage.CurrentInputLanguage;
            List<string> done = new List<string>();

            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (done.Contains(lang.LayoutName))
                    continue;

                InputLanguage.CurrentInputLanguage = lang;
                System.Diagnostics.Debug.WriteLine($"Checking {lang.LayoutName}");

                done.Add(lang.LayoutName);

#if true
                Check(Keys.Up, "Key_UpArrow");
                Check(Keys.Down, "Key_DownArrow");
                Check(Keys.Left, "Key_LeftArrow");
                Check(Keys.Right, "Key_RightArrow");
                Check(Keys.Back, "Key_Backspace");
                Check(Keys.Insert, "Key_Insert");
                Check(Keys.Home, "Key_Home");
                Check(Keys.PageUp, "Key_PageUp");
                Check(Keys.PageDown, "Key_PageDown");
                Check(Keys.Delete, "Key_Delete");
                Check(Keys.End, "Key_End");
                Check(Keys.Space, "Key_Space");
                Check(Keys.F1, "Key_F1");
                Check(Keys.F12, "Key_F12");

                Check(Keys.Tab, "Key_Tab");
                Check(Keys.Capital, "Key_CapsLock");
                Check(Keys.LShiftKey, "Key_LeftShift");
                Check(Keys.RShiftKey, "Key_RightShift");
                Check(Keys.LControlKey, "Key_LeftControl");
                Check(Keys.RControlKey, "Key_RightControl");
                Check(Keys.LMenu, "Key_LeftAlt");
                Check(Keys.RMenu, "Key_RightAlt");

                Check(Keys.NumPad0, "Key_Numpad_0");
                Check(Keys.NumPad9, "Key_Numpad_9");
                Check(KeyObjectExtensions.NumEnter, "Key_Numpad_Enter");
                Check(Keys.Multiply, "Key_Numpad_Multiply");
                Check(Keys.Add, "Key_Numpad_Add");
                Check(Keys.Subtract, "Key_Numpad_Subtract");
                Check(Keys.Decimal, "Key_Numpad_Decimal");
                Check(Keys.NumLock, "Key_NumLock");
#endif

                // 6/4/22 confirmed
                // Keys always listed in row order, top row first, middle row, bottom row
                // each keyboard layout is helpfully having different oem assigned to different keys! (crap)
                // Elite was used to see what frontier names were mapped to these oem keys, and http://kbdlayout.info/ was used to find the oem key assigned

#if true
                if (lang.LayoutName == "Portuguese")
                {
                    Check(Keys.Oem5, "Key_BackSlash");
                    Check(Keys.Oem4, "Key_Apostrophe");
                    Check(Keys.Oem6, "Key_«");

                    Check(Keys.Oemplus, "Key_Plus");
                    Check(Keys.Oem1, "Key_Acute");

                    Check(Keys.Oem3, "Key_ç");
                    Check(Keys.Oem7, "Key_º");
                    Check(Keys.Oem2, "Key_Tilde");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }
                if (lang.LayoutName.Contains("Portuguese (Brazil ABNT"))
                {
                    Check(Keys.Oem3, "Key_Apostrophe");
                    Check(Keys.OemMinus, "Key_Minus");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem4, "Key_Acute");
                    Check(Keys.Oem6, "Key_LeftBracket");

                    Check(Keys.Oem1, "Key_ç");
                    Check(Keys.Oem7, "Key_Tilde");
                    Check(Keys.Oem5, "Key_RightBracket");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.Oem2, "Key_SemiColon");

                }
                if (lang.LayoutName == "Turkish Q")
                {

                    Check(Keys.Oem3, "Key_DoubleQuote");
                    Check(Keys.Oem8, "Key_Asterisk");
                    Check(Keys.OemMinus, "Key_Minus");

                    Check(Keys.Oem4, "Key_ğ");
                    Check(Keys.Oem6, "Key_ü");

                    Check(Keys.Oem1, "Key_ş");
                    Check(Keys.I, "Key_I");
                    Check(Keys.Oemcomma, "Key_Comma");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oem2, "Key_ö");
                    Check(Keys.Oem5, "Key_ç");
                    Check(Keys.OemPeriod, "Key_Period");

                }

                if (lang.LayoutName == "Swedish")
                {
                    Check(Keys.Oem5, "Key_§");
                    Check(Keys.Oemplus, "Key_Plus");
                    Check(Keys.Oem4, "Key_Acute");

                    Check(Keys.Oem6, "Key_å");
                    Check(Keys.Oem1, "Key_Umlaut");

                    Check(Keys.Oem3, "Key_ö");
                    Check(Keys.Oem7, "Key_ä");
                    Check(Keys.Oem2, "Key_Apostrophe");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");

                }
                if (lang.LayoutName == "Danish")
                {
                    Check(Keys.Oem5, "Key_Half");
                    Check(Keys.Oemplus, "Key_Plus");
                    Check(Keys.Oem4, "Key_Acute");

                    Check(Keys.Oem6, "Key_å");
                    Check(Keys.Oem1, "Key_Umlaut");

                    Check(Keys.Oem3, "Key_æ");
                    Check(Keys.Oem7, "Key_ø");
                    Check(Keys.Oem2, "Key_Apostrophe");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");

                }

                if (lang.LayoutName == "US" || lang.LayoutName == "United States-International")
                {

                    Check(Keys.Oem3, "Key_Grave");
                    Check(Keys.OemMinus, "Key_Minus");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem4, "Key_LeftBracket");
                    Check(Keys.Oem6, "Key_RightBracket");

                    Check(Keys.Oem1, "Key_SemiColon");
                    Check(Keys.Oem7, "Key_Apostrophe");

                    // oem 102 is showing KeyBackslash, same as Oem 5. Table maps it to scan code 56
                    Check(Keys.Oem5, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");  // ok
                    Check(Keys.OemPeriod, "Key_Period");    //ok
                    Check(Keys.Oem2, "Key_Slash");  //ok
                }
                else if (lang.LayoutName == "United Kingdom")
                {
                    Check(Keys.Oem8, "Key_Grave");
                    Check(Keys.OemMinus, "Key_Minus");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem4, "Key_LeftBracket");
                    Check(Keys.Oem6, "Key_RightBracket");

                    Check(Keys.Oem1, "Key_SemiColon");
                    Check(Keys.Oem3, "Key_Apostrophe");
                    Check(Keys.Oem7, "Key_Hash");

                    Check(Keys.Oem5, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.Oem2, "Key_Slash");
                }
                if (lang.LayoutName == "German")
                {
                    Check(Keys.Oem5, "Key_Circumflex");
                    Check(Keys.Oem4, "Key_ß");
                    Check(Keys.Oem6, "Key_Acute");

                    Check(Keys.Oem1, "Key_ü");
                    Check(Keys.Oemplus, "Key_Plus");

                    Check(Keys.Oem3, "Key_ö");
                    Check(Keys.Oem7, "Key_ä");
                    Check(Keys.Oem2, "Key_Hash");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");

                }
                if (lang.LayoutName == "Spanish")
                {
                    Check(Keys.Oem5, "Key_Grave");
                    Check(Keys.Oem4, "Key_Minus");
                    Check(Keys.Oem6, "Key_Equals");

                    Check(Keys.Oem1, "Key_LeftBracket");
                    Check(Keys.Oemplus, "Key_RightBracket");

                    Check(Keys.Oem3, "Key_SemiColon");
                    Check(Keys.Oem7, "Key_Apostrophe");
                    Check(Keys.Oem2, "Key_Hash");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Slash");
                }
                if (lang.LayoutName == "French")
                {
                    Check(Keys.Oem7, "Key_SuperscriptTwo");
                    Check(Keys.Oem4, "Key_RightParenthesis");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem6, "Key_Circumflex");
                    Check(Keys.Oem1, "Key_Dollar");

                    Check(Keys.M, "Key_M");
                    Check(Keys.Oem3, "Key_ù");
                    Check(Keys.Oem5, "Key_Asterisk");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_SemiColon");
                    Check(Keys.Oem2, "Key_Colon");
                    Check(Keys.Oem8, "Key_ExclamationPoint");
                }
                if (lang.LayoutName.Contains("Polish"))
                {
                    Check(Keys.Oem3, "Key_Grave");

                    Check(Keys.Oemplus, "Key_Minus");
                    Check(Keys.Oem2, "Key_Equals");

                    Check(Keys.Oem4, "Key_LeftBracket");
                    Check(Keys.Oem6, "Key_RightBracket");

                    Check(Keys.Oem1, "Key_SemiColon");
                    Check(Keys.Oem7, "Key_Apostrophe");
                    Check(Keys.Oem5, "Key_Hash");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Slash");
                }
                if (lang.LayoutName.Contains("Italian"))
                {
                    Check(Keys.Oem5, "Key_BackSlash");
                    Check(Keys.Oem4, "Key_Apostrophe");
                    Check(Keys.Oem6, "Key_ì");

                    Check(Keys.Oem1, "Key_è");
                    Check(Keys.Oemplus, "Key_Plus");

                    Check(Keys.Oem3, "Key_ò");
                    Check(Keys.Oem7, "Key_à");
                    Check(Keys.Oem2, "Key_ù");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }

                if (lang.LayoutName.Contains("Norwegian"))
                {
                    Check(Keys.Oem5, "Key_|");
                    Check(Keys.Oemplus, "Key_Plus");
                    Check(Keys.Oem4, "Key_BackSlash");

                    Check(Keys.Oem6, "Key_å");
                    Check(Keys.Oem1, "Key_Umlaut");

                    Check(Keys.Oem3, "Key_ø");
                    Check(Keys.Oem7, "Key_æ");
                    Check(Keys.Oem2, "Key_Apostrophe");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }
                if (lang.LayoutName.Contains("Finnish"))
                {
                    Check(Keys.Oem5, "Key_§");
                    Check(Keys.Oemplus, "Key_Plus");
                    Check(Keys.Oem4, "Key_Acute");

                    Check(Keys.Oem6, "Key_å");
                    Check(Keys.Oem1, "Key_Umlaut");

                    Check(Keys.Oem3, "Key_ö");
                    Check(Keys.Oem7, "Key_ä");
                    Check(Keys.Oem2, "Key_Apostrophe");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }
                if (lang.LayoutName.Contains("Ukrainian (Enhanced)"))
                {
                    Check(Keys.Oem3, "Key_ё");
                    Check(Keys.OemMinus, "Key_Minus");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem4, "Key_х");
                    Check(Keys.Oem6, "Key_ї");

                    Check(Keys.Oem1, "Key_ж");
                    Check(Keys.Oem7, "Key_є");
                    Check(Keys.Oem5, "Key_BackSlash");

                    Check(Keys.Oem102, "Key_ґ");
                    Check(Keys.Oemcomma, "Key_б");
                    Check(Keys.OemPeriod, "Key_ю");
                    Check(Keys.Oem2, "Key_Period");
                }

                if (lang.LayoutName.Contains("Czech")) //rechecked 7th
                {
                    Check(Keys.Oem3, "Key_SemiColon");
                    Check(Keys.Oemplus, "Key_Equals");
                    Check(Keys.Oem2, "Key_Acute");

                    Check(Keys.Oem4, "Key_ú");
                    Check(Keys.Oem6, "Key_RightParenthesis");

                    Check(Keys.Oem1, "Key_ů");
                    Check(Keys.Oem7, "Key_§");
                    Check(Keys.Oem5, "Key_Umlaut");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }

                if (lang.LayoutName.Contains("Greek")) // 7/4/22
                {
                    Check(Keys.Oem3, "Key_Grave");
                    Check(Keys.OemMinus, "Key_Minus");
                    Check(Keys.Oemplus, "Key_Equals");

                    Check(Keys.Oem4, "Key_LeftBracket");
                    Check(Keys.Oem6, "Key_RightBracket");

                    Check(Keys.Oem1, "Key_΄");
                    Check(Keys.Oem7, "Key_Apostrophe");
                    Check(Keys.Oem5, "Key_BackSlash");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.Oem2, "Key_Slash");
                }


                if (lang.LayoutName.Contains("Lithuanian"))     // 7/4/22
                {
                    Check(Keys.Oem3, "Key_Grave");
                    Check(Keys.OemMinus, "Key_Underline");
                    Check(Keys.Oemplus, "Key_Plus");

                    Check(Keys.Oem4, "Key_į");
                    Check(Keys.Oem6, "Key_“");

                    Check(Keys.Oem1, "Key_ų");
                    Check(Keys.Oem7, "Key_ė");
                    Check(Keys.Oem5, "Key_|");

                    Check(Keys.Oem102, "Key_BackSlash");
                    Check(Keys.Oemcomma, "Key_č");
                    Check(Keys.OemPeriod, "Key_š");
                    Check(Keys.Oem2, "Key_ę");
                }

                if (lang.LayoutName.Contains("Slovak")) // 7/4/22
                {
                    Check(Keys.Oem3, "Key_SemiColon");
                    Check(Keys.Oem2, "Key_Equals");
                    Check(Keys.Oem8, "Key_Acute");

                    Check(Keys.Oem4, "Key_ú");
                    Check(Keys.Oem6, "Key_ä");

                    Check(Keys.Oem1, "Key_ô");
                    Check(Keys.Oem7, "Key_§");
                    Check(Keys.Oem5, "Key_ň");

                    Check(Keys.Oem102, "Key_Ampersand");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }

                if (lang.LayoutName.Contains("Slovenian"))  // 7/4/22
                {
                    Check(Keys.Oem3, "Key_¸");
                    Check(Keys.Oem2, "Key_Apostrophe");
                    Check(Keys.Oemplus, "Key_Plus");

                    Check(Keys.Oem4, "Key_š");
                    Check(Keys.Oem6, "Key_đ");

                    Check(Keys.Oem1, "Key_č");
                    Check(Keys.Oem7, "Key_ć");
                    Check(Keys.Oem5, "Key_ž");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.OemMinus, "Key_Minus");
                }

                if (lang.LayoutName.Contains("Romanian (Standard)"))    // 7/4/22
                {
                    Check(Keys.Oem3, "Key_RightBracket");
                    Check(Keys.OemMinus, "Key_Plus");
                    Check(Keys.Oemplus, "Key_Apostrophe");

                    Check(Keys.Oem4, "Key_ă");
                    Check(Keys.Oem6, "Key_î");

                    Check(Keys.Oem1, "Key_ş");
                    Check(Keys.Oem7, "Key_ţ");
                    Check(Keys.Oem5, "Key_â");

                    Check(Keys.Oem102, "Key_LessThan");
                    Check(Keys.Oemcomma, "Key_Comma");
                    Check(Keys.OemPeriod, "Key_Period");
                    Check(Keys.Oem2, "Key_Minus");
                }
#endif
                System.Diagnostics.Debug.WriteLine($"Finished {lang.LayoutName}" + Environment.NewLine);

            }

            InputLanguage.CurrentInputLanguage = defl;
        }


        static private void Check(Keys k, string key)
        {
            string output = FrontierToKeys(key);
            Keys kc = output.ToVkey();

            string check = "";
            if (kc != k)
                check = "********** ERROR";

            System.Diagnostics.Debug.WriteLine($"  Check Key {key} => Frontier: {output} Keyc: {kc} KcNorm: {KeyObjectExtensions.VKeyToString(kc)} {check}");
            if (kc != k)
            {

            }
        }

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
