/*
 * Copyright 2016-2026 EDDiscovery development team
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
using System.Linq;
using System.Text;

namespace EliteDangerousCore
{
    public partial class BindingsFile 
    {
        public class BindingEntry
        {
            public string Name { get; set; }                        // xml element which made the mapping
            public string Value { get; set; }                       // if a value entry
            public DeviceKeyPair Binding { get; set; }              // may be null
            public List<DeviceKeyPair> PrimaryKeys { get; set; }    // first is key, second is mod, may be null
            public List<DeviceKeyPair> SecondaryKeys { get; set; }  // first is key, second is mod, may be null
            public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

            public bool HasKeys => PrimaryKeys != null;
            public bool HasBinding => Binding != null;

            public string ValuesAsList()
            {
                string vlist = "";
                foreach (var kvp in Values.EmptyIfNull())
                {
                    vlist = vlist.AppendPrePad(kvp.Key + "=" + kvp.Value, Environment.NewLine);
                }
                return vlist;
            }

            // See if either of these are keyboard pairs for use
            public List<DeviceKeyPair> FindKeyAssignment()
            {
                if (PrimaryKeys != null && PrimaryKeys[0].IsKeyboard && (PrimaryKeys.Count == 1 || PrimaryKeys[1].IsKeyboard))
                    return PrimaryKeys;
                if (SecondaryKeys != null && SecondaryKeys[0].IsKeyboard && (SecondaryKeys.Count == 1 || SecondaryKeys[1].IsKeyboard))
                    return SecondaryKeys;
                return null;
            }

            public void RemoveDevice(string device)
            {
                if ( Binding!=null && Binding.Device == device )
                {
                    ClearAllKeys();
                }
                else if ( PrimaryKeys != null)
                {
                    bool primarymatch = PrimaryKeys[0].Device == device || (PrimaryKeys.Count > 1 && PrimaryKeys[1].Device == device);
                    bool secondarymatch = SecondaryKeys != null && (SecondaryKeys[0].Device == device || (SecondaryKeys.Count > 1 && SecondaryKeys[1].Device == device));

                    if (primarymatch)
                    {
                        if (secondarymatch)
                            ClearAllKeys();
                        else
                        {
                            PrimaryKeys = SecondaryKeys;        // move secondary to primary
                            SecondaryKeys = new List<DeviceKeyPair> { new DeviceKeyPair() };
                        }
                    }
                    else if ( secondarymatch )
                        ClearSecondaryKeys();
                }
            }

            public void RenameDevice(string oldname, string newname)
            {
                if (Binding?.Device == oldname)
                {
                    Binding.Device = newname;
                }

                foreach (var x in PrimaryKeys.EmptyIfNull())
                {
                    if (x.Device == oldname)
                        x.Device = newname;
                }
                foreach (var x in SecondaryKeys.EmptyIfNull())
                {
                    if (x.Device == oldname)
                        x.Device = newname;
                }
            }

            public void ClearAllKeys()
            {
                if (Binding != null)
                {
                    Binding.Reset();
                }
                else
                {
                    if (PrimaryKeys != null)
                    {
                        PrimaryKeys.Clear();
                        PrimaryKeys.Add(new DeviceKeyPair());
                        ClearSecondaryKeys();
                    }
                }
            }

            public void ClearSecondaryKeys()
            {
                SecondaryKeys.Clear();
                SecondaryKeys.Add(new DeviceKeyPair());
            }

            public void SetPrimary(string device, string key)
            {
                if (Binding != null)
                    Binding = new DeviceKeyPair(device, key);
                else
                    PrimaryKeys[0] = new DeviceKeyPair(device, key);

                AssignKeys();
            }
            public void SetPrimaryMod(string device, string key)
            {
                if (PrimaryKeys.Count == 1)
                    PrimaryKeys.Add(new DeviceKeyPair(device, key));
                else
                    PrimaryKeys[1] = new DeviceKeyPair(device, key);

                AssignKeys();
            }

            public void SetSecondary(string device, string key)
            {
                SecondaryKeys[0] = new DeviceKeyPair(device, key);
                AssignKeys();
            }
            public void SetSecondaryMod(string device, string key)
            {
                if (SecondaryKeys.Count == 1)
                    SecondaryKeys.Add(new DeviceKeyPair(device, key));
                else
                    SecondaryKeys[1] = new DeviceKeyPair(device, key);
                AssignKeys();
            }

            public void AssignKeys()
            {
                if (Binding != null)
                    Binding.Key = Binding.FrontierKeyName;
                if (PrimaryKeys != null)
                    AssignKeyNames(PrimaryKeys);
                if (SecondaryKeys != null)
                    AssignKeyNames(SecondaryKeys);
            }

            private static string AssignKeyNames(List<DeviceKeyPair> key)
            {
                string firstname = key[0].FrontierKeyName;
                string ErrorList = "";

                if (firstname.HasChars())       // empty frontier entries get null
                {
                    // pov evil.. frontier code these as primary (l/r/u/d) and modifier (l/r/u/d) in no particular order.  If POV, only the first entry gets a Key name, the second entry is null

                    int povindex = firstname.IndexOf("POV");
                    if (povindex >= 0 && key.Count > 1)       // if first and possibly second POV entries
                    {
                        string povroot = key[0].FrontierKeyName.Truncate(0, povindex + 4);
                        string secondname = key[1].FrontierKeyName;
                        if (secondname.Truncate(0, povroot.Length).Equals(povroot))       // POV pair..  lets adjust so its just the major entry.. makes it easier
                        {
                            if (firstname.Contains("Left") || secondname.Contains("Left"))
                                firstname = povroot + ((firstname.Contains("Up") || secondname.Contains("Up")) ? "UpLeft" : "DownLeft");
                            else if (firstname.Contains("Right") || secondname.Contains("Right"))
                                firstname = povroot + ((firstname.Contains("Up") || secondname.Contains("Up")) ? "UpRight" : "DownRight");
                        }

                        key[0].Key = firstname;     // only the first entry get our key name, the second is left null
                    }
                    else
                    {
                        foreach (var x in key)
                        {
                            x.Key = x.FrontierKeyName;
                            if (x.Device == "Keyboard")
                            {
                                string ourkeyname = FrontierKeyConversion.FrontierToKeys(x.FrontierKeyName);
                                if (ourkeyname.StartsWith("!"))
                                    ErrorList = ErrorList.AppendPrePad(ourkeyname.Substring(1), Environment.NewLine);
                                else
                                    x.Key = ourkeyname;
                            }
                        }
                    }
                }
                return ErrorList;
            }

        }

        // Device vs Key/Frontier Key Name

        [System.Diagnostics.DebuggerDisplay("DKP {Device} : {FrontierKeyName} : {Key}")]        
        public class DeviceKeyPair
        {
            public string Device;
            public string Key;                      // Keyboard: in Keys naming convention - converted from Frontier on input.
            public string FrontierKeyName;          // original frontier name
            public bool IsNoDevice => Device == "{NoDevice}";
            public bool IsKeyboard => Device == "Keyboard";
            public bool IsMouse => Device == "Mouse";

            public static bool NoDevice(string device) { return device == "{NoDevice}"; }
            public static bool KeyboardDevice(string device) { return device == "Keyboard"; }

            public DeviceKeyPair()
            {
                Reset();
            }
            public DeviceKeyPair(string device, string frontierkeyname)
            {
                Device = device; FrontierKeyName = frontierkeyname;
            }

            public void Reset()
            {
                Device = "{NoDevice}";
                Key = null;
                FrontierKeyName = "";
            }
        }

    }
}
