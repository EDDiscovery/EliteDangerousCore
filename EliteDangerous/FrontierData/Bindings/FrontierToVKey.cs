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
    static public class FrontierKeyConversion
    {

        // will return null if not supported
        public static string GetSupportedLayout(string cultureid)
        {
            return Array.Find(SupportedLayoutCultures, x => x.Item2.EqualsIIC(cultureid))?.Item1 ?? null;
        }

        private static Dictionary<string,HashSet<string>> CachedLayouts = new Dictionary<string, HashSet<string>>();

        // extracted frontier key names for key languages
        static public HashSet<string> FrontierKeyNames(string layoutname, bool inclalphanumbersfkeys = true)
        {
            if (CachedLayouts.TryGetValue(layoutname,out HashSet<string> result))      // if already computed, return
                return result;

            var ret = new HashSet<string>();
            if (inclalphanumbersfkeys)
            {
                for (int i = 0; i < 26; i++)
                    ret.Add("Key_" + new string(new char[] { (char)(i + 'A') }));
                for (int i = 0; i < 10; i++)
                    ret.Add("Key_" + new string(new char[] { (char)(i + '0') }));
            }

            foreach (var x in frontiertovkeyname)
                ret.Add("Key_" + x.Key);

            for (int i = 0; i < 10; i++)
                ret.Add("Key_Numpad_" + new string(new char[] { (char)(i + '0') }));

            if (inclalphanumbersfkeys)
            {
                for (int i = 0; i <= 24; i++)
                    ret.Add($"Key_F{i}");
            }

            // look at our translation table and add appropriate layout ids

            foreach(var kvp in vkeylookup)
            {
                if ( kvp.Key.Item1 == layoutname)
                {
                    ret.Add(kvp.Key.Item2);
                   // System.Diagnostics.Debug.WriteLine($"Add special key {kvp.Key.Item2}");
                }
            }

            // default all
            string[] additional = new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_Apostrophe", "Key_Comma", "Key_Period", "Key_Slash", };

            // additional ones found needed above the ones given above

            if (layoutname.EqualsIIC("Czech")) additional = new string[] { "Key_Grave", "Key_Minus", "Key_Equals", "Key_LeftBracket", "Key_RightBracket", "Key_Apostrophe", "Key_Comma", "Key_Period", "Key_Slash", };

            else if (layoutname.EqualsIIC("Danish") ||layoutname.EqualsIIC("German") ||layoutname.EqualsIIC("Finnish") ||layoutname.EqualsIIC("Italian")
                            ||layoutname.EqualsIIC("Slovak") ||layoutname.EqualsIIC("Swedish") ||layoutname.EqualsIIC("Norwegian") ||layoutname.EqualsIIC("Portuguese")
                                ||layoutname.EqualsIIC("Slovenian") )
                additional = new string[] { "Key_Comma", "Key_Period", "Key_Minus", };


            else if ( layoutname.EqualsIIC("United Kingdom") ||layoutname.EqualsIIC("US") ||layoutname.EqualsIIC("Greek") ||layoutname.EqualsIIC("Polish (Programmers)")
                                ||layoutname.EqualsIIC("Portuguese (Brazil ABNT)")) 
                additional = new string[] { "Key_Minus", "Key_Equals", "Key_Comma", "Key_Period", };

            else if (layoutname.EqualsIIC("French (Legacy, AZERTY)")) additional = new string[] { "Key_Equals", "Key_Comma", };

            else if (layoutname.EqualsIIC("Romanian (Standard)")) additional = new string[] { "Key_Comma", "Key_Period", };
            else if (layoutname.EqualsIIC("Spanish")) additional = new string[] { "Key_Comma", "Key_Period", };
            else if (layoutname.EqualsIIC("Ukrainian (Enhanced)")) additional = new string[] { "Key_Minus", "Key_Equals", };
            else if (layoutname.EqualsIIC("Turkish Q")) additional = new string[] { "Key_Minus", "Key_Comma", "Key_Period", };

            foreach( var x in additional.EmptyIfNull())
                ret.Add(x);

            CachedLayouts[layoutname] = ret;
            return ret;
        }

        // try and translate from vkeyname to frontier.
        static public string KeysToFrontier(string layoutname, string vkeyname)
        {
            if (vkeyname.Length == 1)
            {
                if (char.IsLetter(vkeyname[0]) || char.IsDigit(vkeyname[0]))
                    return "Key_" + vkeyname[0];
            }
            else if (vkeyname.StartsWith("F") && int.TryParse(vkeyname.Substring(1), out int num))
            {
                return "Key_F" + num.ToStringInvariant();
            }
            else
            {
                var list = FrontierKeyNames(layoutname, false);     // language specific, not A-Z 0-9 or F1-F24

                foreach (var x in list)       // manual search reverse lookup
                {
                    string vn = FrontierKeyConversion.FrontierToKeys(layoutname, x);
                    if (vn == vkeyname)
                        return x;
                }
            }

            return "!No mapping from Frontier name to vkey name for " + vkeyname;
        }


        static public string FrontierToKeys(string layoutname, string frontiername)
        {
            if (frontiername.StartsWith("Key_"))
            {
                string name = frontiername.Substring(4);

                int num;

                // first simple keys
                if (name.Length == 1 && ((name[0] >= '0' && name[0] <= '9') || (name[0] >= 'A' && name[0] <= 'Z')))
                {
                    return name;
                }
                else if (name.StartsWith("Numpad_") && name.Length == 8 && char.IsDigit(name[7]))   // numpad 0-9
                {
                    return "NumPad" + name[7];
                }
                else if (name.StartsWith("F") && int.TryParse(name.Substring(1), out num))      // F keys
                {
                    return "F" + num.ToStringInvariant();
                }
                else if (frontiertovkeyname.TryGetValue(name, out string fname))
                {
                    //System.Diagnostics.Debug.WriteLine($"Translated thru frontiertovkeyname {output} -> {frontiertovkeyname[num].Item1}");
                    return fname;
                }
                else if (vkeylookup.TryGetValue(Tuple.Create(layoutname, frontiername), out string vkeyname))
                {
                    return vkeyname;
                }
                else if (name.Length == 1)
                {
                    // emergency single char, should not be needed for supported languages.
                    // This uses the current locale not the layout name,
                    // its not easy in win32 to get to the locale.

                    IntPtr layout = BaseUtils.Win32.UnsafeNativeMethods.GetKeyboardLayout(0);
                    short vkey = BaseUtils.Win32.UnsafeNativeMethods.VkKeyScanExW(name[0], layout);        // look up char->vkey
                    Keys k = (Keys)(vkey & 0xff);
                    System.Diagnostics.Debug.WriteLine($"FrontierKeys Emergency `{name}` -> {k}");
                    return vkey != -1 ? KeyObjectExtensions.VKeyToString(k) : name;
                }
                else
                    return name;    // else its just the name truncated
            }
            return null;
        }
        
        // list of layouts vs frontier culture name in the bindings file

        static public Tuple<string, string>[] SupportedLayoutCultures = new Tuple<string, string>[]
        {
            Tuple.Create("United Kingdom","en-GB"),Tuple.Create("Czech","cs-CZ"),Tuple.Create("Danish","da-DK"),
            Tuple.Create("German","de-DE"),Tuple.Create("Greek","el-GR"),
            Tuple.Create("US","en-US"),Tuple.Create("Finnish","fi-FI"),Tuple.Create("French (Legacy, AZERTY)","fr-FR"),
            Tuple.Create("Italian","it-IT"),Tuple.Create("Polish (Programmers)","pl-PL"),Tuple.Create("Portuguese (Brazil ABNT)","pt-BR"),
            Tuple.Create("Romanian (Standard)","ro-RO"),Tuple.Create("Slovak","sk-SK"),Tuple.Create("Swedish","sv-SE"),
            Tuple.Create("Turkish Q","tr-TR"),Tuple.Create("Ukrainian (Enhanced)","uk-UA"),Tuple.Create("Slovenian","sl-SI"),
            Tuple.Create("Lithuanian","lt-LT"),Tuple.Create("Norwegian","nn-NO"),
            Tuple.Create("Portuguese","pt-PT"),Tuple.Create("US","en-AU"),Tuple.Create("Spanish","es-ES"),
            Tuple.Create("Belgium French","fr-BE"),
            Tuple.Create("Austria","de-AT"),
            Tuple.Create("Canadian French","fr-CA")

            // canada french legacy/new
            // dutch
            // estonian
            // hungarian
            // slovak
            // de-AT
        };


        // special language handling of OEM keys, language/frontier name to vkey name
        // use the special bindings file in this folder which has UI* and others mapped to the OEM Keys
        // Operation:
        // 1. Use this custom bindings file in the frontier bindings folder
        // 2. Select language on language toolbar
        // 3. go into elite and edit controls - back out and the bindings file will have the updated key strokes and the updated keyboard culture - check - but with the same keys
        //      it seems dynamic and does not change the physical binding but does change the frontier key names in the file
        // 4. load up the bindings into the BindingsFile and run OemListKeys - it will print out the below entries
        // usedful https://kbdlayout.info/kbdlt1/scancodes this has scancode/layout->vkeys

        static Dictionary<Tuple<string, string>, string> vkeylookup = new Dictionary<Tuple<string, string>, string>
        {
            [Tuple.Create("United Kingdom", "Key_Grave")] = "Backquote",                //30/8/26
            [Tuple.Create("United Kingdom", "Key_LeftBracket")] = "OpenBrackets",
            [Tuple.Create("United Kingdom", "Key_RightBracket")] = "CloseBrackets",
            [Tuple.Create("United Kingdom", "Key_SemiColon")] = "Semicolon",
            [Tuple.Create("United Kingdom", "Key_Apostrophe")] = "Tilde",
            [Tuple.Create("United Kingdom", "Key_Hash")] = "Quotes",
            [Tuple.Create("United Kingdom", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("United Kingdom", "Key_Slash")] = "Question",

            [Tuple.Create("Czech", "Key_SemiColon")] = "Tilde",     //30/8/26
            [Tuple.Create("Czech", "Key_Acute")] = "Question",
            [Tuple.Create("Czech", "Key_ú")] = "OpenBrackets",
            [Tuple.Create("Czech", "Key_RightParenthesis")] = "CloseBrackets",
            [Tuple.Create("Czech", "Key_ů")] = "Semicolon",
            [Tuple.Create("Czech", "Key_§")] = "Quotes",
            [Tuple.Create("Czech", "Key_Umlaut")] = "Pipe",
            [Tuple.Create("Czech", "Key_BackSlash")] = "Backslash",

            [Tuple.Create("Danish", "Key_Half")] = "Pipe",                  //30/8/26
            [Tuple.Create("Danish", "Key_Plus")] = "Equals",
            [Tuple.Create("Danish", "Key_Acute")] = "OpenBrackets",
            [Tuple.Create("Danish", "Key_å")] = "CloseBrackets",
            [Tuple.Create("Danish", "Key_Umlaut")] = "Semicolon",
            [Tuple.Create("Danish", "Key_æ")] = "Tilde",
            [Tuple.Create("Danish", "Key_ø")] = "Quotes",
            [Tuple.Create("Danish", "Key_Apostrophe")] = "Question",
            [Tuple.Create("Danish", "Key_LessThan")] = "Backslash",

            [Tuple.Create("German", "Key_Circumflex")] = "Pipe", //30/8/26
            [Tuple.Create("German", "Key_ß")] = "OpenBrackets",
            [Tuple.Create("German", "Key_Acute")] = "CloseBrackets",
            [Tuple.Create("German", "Key_ü")] = "Semicolon",
            [Tuple.Create("German", "Key_Plus")] = "Equals",
            [Tuple.Create("German", "Key_ö")] = "Tilde",
            [Tuple.Create("German", "Key_ä")] = "Quotes",
            [Tuple.Create("German", "Key_Hash")] = "Question",
            [Tuple.Create("German", "Key_LessThan")] = "Backslash",


            [Tuple.Create("Greek", "Key_Grave")] = "Tilde",  //30/8/26
            [Tuple.Create("Greek", "Key_LeftBracket")] = "OpenBrackets",
            [Tuple.Create("Greek", "Key_RightBracket")] = "CloseBrackets",
            [Tuple.Create("Greek", "Key_΄")] = "Semicolon",
            [Tuple.Create("Greek", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("Greek", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("Greek", "Key_LessThan")] = "Backslash",
            [Tuple.Create("Greek", "Key_Slash")] = "Question",

            [Tuple.Create("US", "Key_Grave")] = "Tilde",
            [Tuple.Create("US", "Key_LeftBracket")] = "OpenBrackets",
            [Tuple.Create("US", "Key_RightBracket")] = "CloseBrackets",
            [Tuple.Create("US", "Key_SemiColon")] = "Semicolon",
            [Tuple.Create("US", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("US", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("US", "Key_Slash")] = "Question",

            [Tuple.Create("Finnish", "Key_§")] = "Pipe",
            [Tuple.Create("Finnish", "Key_Plus")] = "Equals",
            [Tuple.Create("Finnish", "Key_Acute")] = "OpenBrackets",
            [Tuple.Create("Finnish", "Key_å")] = "CloseBrackets",
            [Tuple.Create("Finnish", "Key_Umlaut")] = "Semicolon",
            [Tuple.Create("Finnish", "Key_ö")] = "Tilde",
            [Tuple.Create("Finnish", "Key_ä")] = "Quotes",
            [Tuple.Create("Finnish", "Key_Apostrophe")] = "Question",
            [Tuple.Create("Finnish", "Key_LessThan")] = "Backslash",

            [Tuple.Create("French (Legacy, AZERTY)", "Key_SuperscriptTwo")] = "Quotes",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_RightParenthesis")] = "OpenBrackets",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_Circumflex")] = "CloseBrackets",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_Dollar")] = "Semicolon",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_ù")] = "Tilde",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_Asterisk")] = "Pipe",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_LessThan")] = "Backslash",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_SemiColon")] = "Period",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_Colon")] = "Question",
            [Tuple.Create("French (Legacy, AZERTY)", "Key_ExclamationPoint")] = "Backquote",

            [Tuple.Create("Belgium French", "Key_SuperscriptTwo")] = "Quotes",      
            [Tuple.Create("Belgium French", "Key_RightParenthesis")] = "OpenBrackets",
            [Tuple.Create("Belgium French", "Key_Minus")] = "Minus",
            [Tuple.Create("Belgium French", "Key_Circumflex")] = "CloseBrackets",
            [Tuple.Create("Belgium French", "Key_Dollar")] = "Semicolon",
            [Tuple.Create("Belgium French", "Key_ù")] = "Tilde",
            [Tuple.Create("Belgium French", "Key_µ")] = "Pipe",
            [Tuple.Create("Belgium French", "Key_LessThan")] = "Backslash",         
            [Tuple.Create("Belgium French", "Key_Comma")] = "Comma",        
            [Tuple.Create("Belgium French", "Key_SemiColon")] = "Period",
            [Tuple.Create("Belgium French", "Key_Colon")] = "Question",
            [Tuple.Create("Belgium French", "Key_Equals")] = "Plus",

            [Tuple.Create("Italian", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("Italian", "Key_Apostrophe")] = "OpenBrackets",
            [Tuple.Create("Italian", "Key_ì")] = "CloseBrackets",
            [Tuple.Create("Italian", "Key_è")] = "Semicolon",
            [Tuple.Create("Italian", "Key_Plus")] = "Equals",
            [Tuple.Create("Italian", "Key_ò")] = "Tilde",
            [Tuple.Create("Italian", "Key_à")] = "Quotes",
            [Tuple.Create("Italian", "Key_ù")] = "Question",
            [Tuple.Create("Italian", "Key_LessThan")] = "Backslash",

            [Tuple.Create("Polish (Programmers)", "Key_Grave")] = "Tilde",
            [Tuple.Create("Polish (Programmers)", "Key_LeftBracket")] = "OpenBrackets",
            [Tuple.Create("Polish (Programmers)", "Key_RightBracket")] = "CloseBrackets",
            [Tuple.Create("Polish (Programmers)", "Key_SemiColon")] = "Semicolon",
            [Tuple.Create("Polish (Programmers)", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("Polish (Programmers)", "Key_Hash")] = "Pipe",
            [Tuple.Create("Polish (Programmers)", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Polish (Programmers)", "Key_Slash")] = "Question",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_Apostrophe")] = "Tilde",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_Acute")] = "OpenBrackets",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_LeftBracket")] = "CloseBrackets",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_ç")] = "Semicolon",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_Tilde")] = "Quotes",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_RightBracket")] = "Pipe",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Portuguese (Brazil ABNT)", "Key_SemiColon")] = "Question",

            [Tuple.Create("Romanian (Standard)", "Key_RightBracket")] = "Tilde",        //30/8/26
            [Tuple.Create("Romanian (Standard)", "Key_Plus")] = "Minus",
            [Tuple.Create("Romanian (Standard)", "Key_Apostrophe")] = "Equals",
            [Tuple.Create("Romanian (Standard)", "Key_ă")] = "OpenBrackets",
            [Tuple.Create("Romanian (Standard)", "Key_î")] = "CloseBrackets",
            [Tuple.Create("Romanian (Standard)", "Key_ş")] = "Semicolon",
            [Tuple.Create("Romanian (Standard)", "Key_ţ")] = "Quotes",
            [Tuple.Create("Romanian (Standard)", "Key_â")] = "Pipe",
            [Tuple.Create("Romanian (Standard)", "Key_LessThan")] = "Backslash",
            [Tuple.Create("Romanian (Standard)", "Key_Minus")] = "Question",
            
            [Tuple.Create("Slovak", "Key_SemiColon")] = "Tilde",
            [Tuple.Create("Slovak", "Key_Equals")] = "Question",
            [Tuple.Create("Slovak", "Key_Acute")] = "Backquote",
            [Tuple.Create("Slovak", "Key_ú")] = "OpenBrackets",
            [Tuple.Create("Slovak", "Key_ä")] = "CloseBrackets",
            [Tuple.Create("Slovak", "Key_ô")] = "Semicolon",
            [Tuple.Create("Slovak", "Key_§")] = "Quotes",
            [Tuple.Create("Slovak", "Key_ň")] = "Pipe",
            [Tuple.Create("Slovak", "Key_Ampersand")] = "Backslash",

            [Tuple.Create("Swedish", "Key_§")] = "Pipe",
            [Tuple.Create("Swedish", "Key_Plus")] = "Equals",
            [Tuple.Create("Swedish", "Key_Acute")] = "OpenBrackets",
            [Tuple.Create("Swedish", "Key_å")] = "CloseBrackets",
            [Tuple.Create("Swedish", "Key_Umlaut")] = "Semicolon",
            [Tuple.Create("Swedish", "Key_ö")] = "Tilde",
            [Tuple.Create("Swedish", "Key_ä")] = "Quotes",
            [Tuple.Create("Swedish", "Key_Apostrophe")] = "Question",
            [Tuple.Create("Swedish", "Key_LessThan")] = "Backslash",

            [Tuple.Create("Turkish Q", "Key_DoubleQuote")] = "Tilde",
            [Tuple.Create("Turkish Q", "Key_Asterisk")] = "Backquote",
            [Tuple.Create("Turkish Q", "Key_ğ")] = "OpenBrackets",
            [Tuple.Create("Turkish Q", "Key_ü")] = "CloseBrackets",
            [Tuple.Create("Turkish Q", "Key_ş")] = "Semicolon",
            [Tuple.Create("Turkish Q", "Key_LessThan")] = "Backslash",
            [Tuple.Create("Turkish Q", "Key_ö")] = "Question",
            [Tuple.Create("Turkish Q", "Key_ç")] = "Pipe",

            [Tuple.Create("Ukrainian (Enhanced)", "Key_ё")] = "Tilde",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_х")] = "OpenBrackets",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_ї")] = "CloseBrackets",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_ж")] = "Semicolon",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_є")] = "Quotes",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_ґ")] = "Backslash",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_б")] = "Comma",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_ю")] = "Period",
            [Tuple.Create("Ukrainian (Enhanced)", "Key_Period")] = "Question",

            [Tuple.Create("Slovenian", "Key_¸")] = "Tilde",
            [Tuple.Create("Slovenian", "Key_Apostrophe")] = "Question",
            [Tuple.Create("Slovenian", "Key_Plus")] = "Equals",
            [Tuple.Create("Slovenian", "Key_š")] = "OpenBrackets",
            [Tuple.Create("Slovenian", "Key_đ")] = "CloseBrackets",
            [Tuple.Create("Slovenian", "Key_č")] = "Semicolon",
            [Tuple.Create("Slovenian", "Key_ć")] = "Quotes",
            [Tuple.Create("Slovenian", "Key_ž")] = "Pipe",
            [Tuple.Create("Slovenian", "Key_LessThan")] = "Backslash",

            [Tuple.Create("Lithuanian", "Key_Grave")] = "Tilde",        // 30/8/26
            [Tuple.Create("Lithuanian", "Key_Underline")] = "Minus",
            [Tuple.Create("Lithuanian", "Key_Plus")] = "Equals",
            [Tuple.Create("Lithuanian", "Key_į")] = "OpenBrackets",
            [Tuple.Create("Lithuanian", "Key_“")] = "CloseBrackets",
            [Tuple.Create("Lithuanian", "Key_ų")] = "Semicolon",
            [Tuple.Create("Lithuanian", "Key_ė")] = "Quotes",
            [Tuple.Create("Lithuanian", "Key_|")] = "Pipe",
            [Tuple.Create("Lithuanian", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Lithuanian", "Key_č")] = "Comma",
            [Tuple.Create("Lithuanian", "Key_š")] = "Period",
            [Tuple.Create("Lithuanian", "Key_ę")] = "Question",

            [Tuple.Create("Norwegian", "Key_Grave")] = "Pipe",          // 30/8/26 rewritten
            [Tuple.Create("Norwegian", "Key_Minus")] = "Equals",
            [Tuple.Create("Norwegian", "Key_Equals")] = "OpenBrackets",
            [Tuple.Create("Norwegian", "Key_LeftBracket")] = "CloseBrackets",
            [Tuple.Create("Norwegian", "Key_RightBracket")] = "Semicolon",
            [Tuple.Create("Norwegian", "Key_SemiColon")] = "Tilde",
            [Tuple.Create("Norwegian", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("Norwegian", "Key_BackSlash")] = "Question",
            [Tuple.Create("Norwegian", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Norwegian", "Key_Slash")] = "Minus",

            [Tuple.Create("Portuguese", "Key_BackSlash")] = "Pipe",
            [Tuple.Create("Portuguese", "Key_Apostrophe")] = "OpenBrackets",
            [Tuple.Create("Portuguese", "Key_«")] = "CloseBrackets",
            [Tuple.Create("Portuguese", "Key_Plus")] = "Equals",
            [Tuple.Create("Portuguese", "Key_Acute")] = "Semicolon",
            [Tuple.Create("Portuguese", "Key_ç")] = "Tilde",
            [Tuple.Create("Portuguese", "Key_º")] = "Quotes",
            [Tuple.Create("Portuguese", "Key_Tilde")] = "Question",
            [Tuple.Create("Portuguese", "Key_LessThan")] = "Backslash",
            [Tuple.Create("Spanish", "Key_Grave")] = "Pipe",
            [Tuple.Create("Spanish", "Key_Minus")] = "OpenBrackets",
            [Tuple.Create("Spanish", "Key_Equals")] = "CloseBrackets",
            [Tuple.Create("Spanish", "Key_LeftBracket")] = "Semicolon",
            [Tuple.Create("Spanish", "Key_RightBracket")] = "Equals",
            [Tuple.Create("Spanish", "Key_SemiColon")] = "Tilde",
            [Tuple.Create("Spanish", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("Spanish", "Key_Hash")] = "Question",
            [Tuple.Create("Spanish", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Spanish", "Key_Slash")] = "Minus",

            [Tuple.Create("Austria", "Key_Grave")] = "Pipe",            // added, note frontier has a problem with scan code 28/2B and calls them the same
            [Tuple.Create("Austria", "Key_Minus")] = "OpenBrackets",
            [Tuple.Create("Austria", "Key_Equals")] = "CloseBrackets",
            [Tuple.Create("Austria", "Key_LeftBracket")] = "Semicolon",
            [Tuple.Create("Austria", "Key_RightBracket")] = "Equals",
            [Tuple.Create("Austria", "Key_SemiColon")] = "Tilde",
            [Tuple.Create("Austria", "Key_Apostrophe")] = "Quotes",
            [Tuple.Create("Austria", "Key_BackSlash")] = "Question",
            //[Tuple.Create("Austria", "Key_BackSlash")] = "Backslash",
            [Tuple.Create("Austria", "Key_Slash")] = "Minus",

            [Tuple.Create("Canadian French", "Key_Ring")] = "Quotes",
            [Tuple.Create("Canadian French", "Key_Circumflex")] = "OpenBrackets",
            [Tuple.Create("Canadian French", "Key_ç")] = "CloseBrackets",
            [Tuple.Create("Canadian French", "Key_SemiColon")] = "Semicolon",
            [Tuple.Create("Canadian French", "Key_è")] = "Tilde",
            [Tuple.Create("Canadian French", "Key_à")] = "Pipe",
            [Tuple.Create("Canadian French", "Key_ù")] = "Backslash",
            [Tuple.Create("Canadian French", "Key_é")] = "Question",
        };

        // standard key name conversion between frontier and vkey

        static private Dictionary<string, string> frontiertovkeyname = new Dictionary<string, string>
        {
            ["Escape"] = Keys.Escape.VKeyToString(),
            ["Backspace"] = Keys.Back.VKeyToString(),
            ["Tab"] = Keys.Tab.VKeyToString(),
            ["Enter"] = Keys.Return.VKeyToString(),
            ["LeftControl"] = Keys.LControlKey.VKeyToString(),
            ["LeftShift"] = Keys.LShiftKey.VKeyToString(),
            ["RightShift"] = Keys.RShiftKey.VKeyToString(),
            ["Numpad_Multiply"] = Keys.Multiply.VKeyToString(),
            ["LeftAlt"] = Keys.LMenu.VKeyToString(),
            ["Space"] = Keys.Space.VKeyToString(),
            ["CapsLock"] = Keys.Capital.VKeyToString(),
            ["NumLock"] = Keys.NumLock.VKeyToString(),
            ["ScrollLock"] = Keys.Scroll.VKeyToString(),
            ["Numpad_Subtract"] = Keys.Subtract.VKeyToString(),
            ["Numpad_Add"] = Keys.Add.VKeyToString(),
            ["Numpad_Decimal"] = Keys.Decimal.VKeyToString(),
            ["Kana"] = Keys.KanaMode.VKeyToString(),
            ["Convert"] = Keys.IMEConvert.VKeyToString(),
            ["NoConvert"] = Keys.IMENonconvert.VKeyToString(),
            ["PrevTrack"] = Keys.MediaPreviousTrack.VKeyToString(),
            ["Kanji"] = Keys.HanjaMode.VKeyToString(),
            ["Unlabeled"] = Keys.NoName.VKeyToString(),
            ["NextTrack"] = Keys.MediaNextTrack.VKeyToString(),
            ["Numpad_Enter"] = "NumEnter",
            ["RightControl"] = Keys.RControlKey.VKeyToString(),
            ["Mute"] = Keys.VolumeMute.VKeyToString(),
            ["PlayPause"] = Keys.MediaPlayPause.VKeyToString(),
            ["MediaStop"] = Keys.MediaStop.VKeyToString(),
            ["VolumeDown"] = Keys.VolumeDown.VKeyToString(),
            ["VolumeUp"] = Keys.VolumeUp.VKeyToString(),
            ["WebHome"] = Keys.BrowserHome.VKeyToString(),
            ["Numpad_Divide"] = Keys.Divide.VKeyToString(),
            ["SYSRQ"] = Keys.PrintScreen.VKeyToString(),
            ["RightAlt"] = Keys.RMenu.VKeyToString(),
            ["Pause"] = Keys.Pause.VKeyToString(),
            ["Home"] = Keys.Home.VKeyToString(),
            ["UpArrow"] = Keys.Up.VKeyToString(),
            ["PageUp"] = Keys.PageUp.VKeyToString(),
            ["LeftArrow"] = Keys.Left.VKeyToString(),
            ["RightArrow"] = Keys.Right.VKeyToString(),
            ["End"] = Keys.End.VKeyToString(),
            ["DownArrow"] = Keys.Down.VKeyToString(),
            ["PageDown"] = Keys.PageDown.VKeyToString(),
            ["Insert"] = Keys.Insert.VKeyToString(),
            ["Delete"] = Keys.Delete.VKeyToString(),
            ["LeftWin"] = Keys.LWin.VKeyToString(),
            ["RightWin"] = Keys.RWin.VKeyToString(),
            ["Apps"] = Keys.Apps.VKeyToString(),
            ["Sleep"] = Keys.Sleep.VKeyToString(),
            ["WebSearch"] = Keys.BrowserSearch.VKeyToString(),
            ["WebFavourites"] = Keys.BrowserFavorites.VKeyToString(),
            ["WebRefresh"] = Keys.BrowserRefresh.VKeyToString(),
            ["WebStop"] = Keys.BrowserStop.VKeyToString(),
            ["WebForward"] = Keys.BrowserForward.VKeyToString(),
            ["WebBack"] = Keys.BrowserBack.VKeyToString(),
            ["Mail"] = Keys.LaunchMail.VKeyToString(),
            ["MediaSelect"] = Keys.SelectMedia.VKeyToString(),

        };

    }
}
