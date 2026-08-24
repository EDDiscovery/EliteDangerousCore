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
        [System.Diagnostics.DebuggerDisplay("{Name} {Value} B:{Binding}")]
        public class BindingEntry
        {
            public BindingEntry(string name, string value, string nodevicename)
            {
                Name = name;
                Value = value;
                NoDeviceName = nodevicename;            // we keep this so we know how to cancel stuff
            }

            public string Name { get; set; }                        // xml element which made the mapping
            public string Value { get; set; }                       // if a value entry
            public string NoDeviceName { get; set; }                // so we know how to cancel

            public Tuple<FrontierBindingClassification.Classification,FrontierBindingClassification.Mode> ClassMode { get; set; }   // null for non key entries
            public bool Binding { get; set; }                       // is it a joystick binding? 
            public List<DeviceKeyPair> PrimaryKeys { get; set; }    // first is key, second is mod, may be null
            public List<DeviceKeyPair> SecondaryKeys { get; set; }  // first is key, second is mod, may be null
            public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();      // any elements with values
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();  // attributes of the binding 

            public bool HasAnyKeys => PrimaryKeys != null;
            public bool HasPrimaryAndSecondary => PrimaryKeys != null && PrimaryKeys[0].Assigned && SecondaryKeys != null && SecondaryKeys[0].Assigned;
            public string PrimaryDevice => PrimaryKeys != null ? PrimaryKeys[0].Device : null;

            // return either Device:Key or (Device:Key,Device:Key) Key is either Frontier or Internal. 
            // empty string if KeyPair is null or NoDevice
            public string PrimaryFrontierKeyList() => DeviceKeyPair.KeyDescription(PrimaryKeys, false);
            public string SecondaryFrontierKeyList() => DeviceKeyPair.KeyDescription(SecondaryKeys, false);
            public string PrimaryKeyList() => DeviceKeyPair.KeyDescription(PrimaryKeys, true);
            public string SecondaryKeyList() => DeviceKeyPair.KeyDescription(SecondaryKeys, true);

            // See if either of these are keyboard pairs for use, prefer PrimaryKeys
            public List<DeviceKeyPair> FindKeyboardAssignment()
            {
                if (PrimaryKeys != null && PrimaryKeys[0].IsKeyboard && (PrimaryKeys.Count == 1 || PrimaryKeys[1].IsKeyboard))
                    return PrimaryKeys;
                if (SecondaryKeys != null && SecondaryKeys[0].IsKeyboard && (SecondaryKeys.Count == 1 || SecondaryKeys[1].IsKeyboard))
                    return SecondaryKeys;
                return null;
            }

            // do the 'keys' in other clash with our keys
            // return 1/2 in tuple position to indicate primary or secondary clash for us and other
            public Tuple<int, int> HasAnyKeysInCommon(BindingEntry other)
            {
                string pk = PrimaryFrontierKeyList();
                string sk = SecondaryFrontierKeyList();
                string opk = other.PrimaryFrontierKeyList();
                string osk = other.SecondaryFrontierKeyList();
                if (pk.IsEmpty())       // no keys, can't clash, can't have secondary
                    return null;
                else if (pk == opk)
                    return Tuple.Create(1, 1);
                else if (pk == osk)
                    return Tuple.Create(1, 2);
                else if (sk.IsEmpty())
                    return null;
                else if (sk == opk)
                    return Tuple.Create(2, 1);
                else if (sk == osk)
                    return Tuple.Create(2, 2);
                else
                    return null;
            }

            // remove assignments using this device
            public void RemoveDevice(string device)
            {
                if ( PrimaryKeys != null)
                {
                    bool primarymatch = PrimaryKeys[0].Device == device || (PrimaryKeys.Count > 1 && PrimaryKeys[1].Device == device);
                    bool secondarymatch = SecondaryKeys != null && (SecondaryKeys[0].Device == device || (SecondaryKeys.Count > 1 && SecondaryKeys[1].Device == device));

                    if (primarymatch)
                    {
                        if (secondarymatch)
                        {
                            ClearAll();
                        }
                        else
                        {
                            PrimaryKeys = SecondaryKeys;        // move secondary to primary
                            ClearSecondary();
                        }
                    }
                    else if (secondarymatch)
                    {
                        ClearSecondary();
                    }
                }
            }

            // rename device
            public void RenameDevice(string oldname, string newname)
            {
                foreach (var x in PrimaryKeys.EmptyIfNull().Where(x => x.Device == oldname))
                    x.Device = newname;
                foreach (var x in SecondaryKeys.EmptyIfNull().Where(x => x.Device == oldname))
                    x.Device = newname;
            }

            public void ClearAll()
            {
                ClearPrimary();
                ClearSecondary();
            }

            public void ClearPrimary()
            {
                PrimaryKeys = new List<DeviceKeyPair> { new DeviceKeyPair(NoDeviceName, "") };
            }
            public void ClearSecondary()
            {
                if (!Binding) // bindings do not have secondary keys set
                {
                    SecondaryKeys = new List<DeviceKeyPair> { new DeviceKeyPair(NoDeviceName, "") };
                }
            }

            public void ClearPrimaryMod()
            {
                if (PrimaryKeys?.Count > 1)
                    PrimaryKeys.RemoveAt(1);
            }

            public void SetPrimary(string device, string key)
            {
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

            public void ClearSecondaryMod()
            {
                if (SecondaryKeys?.Count > 1)
                    SecondaryKeys.RemoveAt(1);
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

            public bool SwapPrimarySecondary()
            {
                if (HasPrimaryAndSecondary && PrimaryKeys[0].Assigned && SecondaryKeys[0].Assigned)
                {
                    var swap = new List<DeviceKeyPair>(SecondaryKeys);
                    SecondaryKeys = PrimaryKeys;
                    PrimaryKeys = swap;
                    return true;
                }
                else
                    return false;
            }

            public void AssignKeys()
            {
                if (PrimaryKeys != null)
                    AssignKeyNames(PrimaryKeys);
                if (SecondaryKeys != null)
                    AssignKeyNames(SecondaryKeys);
            }

            // format up values for display
            public static string ValuesAsList(string entryname, Dictionary<string, string> v, bool displaykeyname)
            {
                string vlist = "";
                foreach (var kvp in v.EmptyIfNull())
                {
                    var options = FrontierBindingClassification.GetValueOptions(displaykeyname ? kvp.Key : entryname, kvp.Value);        // see if we have a combobox option for this pair
                    string value = kvp.Value;
                    if (options != null)
                    {
                        for (int i = 0; i < options.Length - 1; i += 2)
                        {
                            if (options[i].EqualsIIC(value))
                            {
                                value = options[i + 1];
                                break;
                            }
                        }
                    }

                    //                    vlist = vlist.AppendPrePad((displaykeyname ? (kvp.Key + "=") : "") + value + " (" + kvp.Value+ ")", Environment.NewLine);
                    vlist = vlist.AppendPrePad((displaykeyname ? (kvp.Key + "=") : "") + value, Environment.NewLine);
                }
                return vlist;
            }

            // convert the frontier name for POV's and keyboard keys to our naming and store in Key
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

                        key[0].Key = firstname;     // only the first entry get our key name
                        key[1].Key = "POV-KEY-IGNORE";      // gets a text marker so its not null
                    }
                    else
                    {
                        foreach (var x in key)
                        {
                            x.Key = x.FrontierKeyName;      // always set key name

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
            public string Device { get; set; }                   // internal name of device
            public string FrontierKeyName { get; set; }          // original frontier name
            public string Key { get; set; }                      // Keyboard: in Keys naming convention - converted from Frontier on input.
            public bool Assigned => FrontierKeyName.HasChars();

            // these are fixed names at this level, the other names have to be handled at bindingfile level
            public bool IsKeyboard => KeyboardDevice(Device);
            public bool IsMouse => MouseDevice(Device);
            public static bool KeyboardDevice(string device) { return device == KeyboardDeviceName; }
            public static bool MouseDevice(string device) { return device == MouseDeviceName; }


            public DeviceKeyPair(string internaldevicename, string frontierkeyname)
            {
                Device = internaldevicename; 
                FrontierKeyName = frontierkeyname;
            }

              // do ours have a key assignment in common with other. Based on internal Key
            public static bool HasInternalKeyInCommon(List<DeviceKeyPair> ours, List<DeviceKeyPair> other)
            {
                foreach (DeviceKeyPair o in other)
                {
                    foreach (DeviceKeyPair k in ours)
                    {
                        if (k.Key.Equals(o.Key))
                            return true;
                    }
                }

                return false;
            }

            // produce a key description string in the format Device:Key or (Device:Key,Device:Key)
            // use either the internal key name or frontier name
            public static string KeyDescription(List<DeviceKeyPair> keyPair, bool internalkeyname)
            {
                if (keyPair == null || keyPair.Count < 1 || !keyPair[0].Assigned)
                    return "";
                string part = keyPair[0].Device + ":" + (internalkeyname ? keyPair[0].Key : keyPair[0].FrontierKeyName);
                if (keyPair.Count > 1)
                    return "(" + part + "," + keyPair[1].Device + ":" + (internalkeyname ? keyPair[1].Key : keyPair[1].FrontierKeyName) + ")";
                else
                    return part;
            }


            public const string KeyboardDeviceName = "Keyboard";
            public const string MouseDeviceName = "Mouse";
        }

    }
}
