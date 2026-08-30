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

namespace EliteDangerousCore
{
    public partial class BindingsFile 
    {
        [System.Diagnostics.DebuggerDisplay("{ToString()} : {Value}")]
        public class BindingEntry
        {
            public BindingEntry(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }                        // xml element which made the mapping
            public string Value { get; set; }                       // if a value entry

            public bool Binding { get; set; }                       // is it a joystick binding? 
            public bool KeyOrBinding => PrimaryKeys != null;        // is it a key or binding entry?
            public bool Assigned => PrimaryKeys?.Assigned == true;  // does we have any assignments? Primary must be defined
            public bool HasPrimaryAndSecondaryAssigned => PrimaryKeys != null && PrimaryKeys.Assigned && SecondaryKeys != null && SecondaryKeys.Assigned;

            public DeviceKeyPairList PrimaryKeys { get; set; }      // May be null
            public DeviceKeyPairList SecondaryKeys { get; set; }    // May be null
            public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();      // any elements with values
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();  // attributes of the binding 
            public Tuple<FrontierBindingClassification.Classification, FrontierBindingClassification.Mode> ClassMode { get; set; }   // null for non key entries

            public override string ToString() => $"{Name} : {Binding} : Prim `{PrimaryKeys?.KeyDescription()}` sec `{SecondaryKeys?.KeyDescription()}`";

            // See if either of these are keyboard pairs for use, prefer PrimaryKeys
            public DeviceKeyPairList FindKeyboardAssignment()
            {
                if (PrimaryKeys?.IsKeyboard == true)
                    return PrimaryKeys;
                if (SecondaryKeys?.IsKeyboard == true)
                    return SecondaryKeys;
                return null;
            }

            // do the 'keys' in other clash with our keys
            // return 1/2 in tuple position to indicate primary or secondary clash for us and other
            public Tuple<int, int> HasAnyKeysInCommon(BindingEntry other)
            {
                if (PrimaryKeys != null)
                {
                    if (PrimaryKeys.Equals(other.PrimaryKeys) == true)
                        return Tuple.Create(1, 1);
                    if (PrimaryKeys.Equals(other.SecondaryKeys) == true)
                        return Tuple.Create(1, 2);
                }

                if (SecondaryKeys != null)
                {
                    if (SecondaryKeys.Equals(other.PrimaryKeys) == true)
                        return Tuple.Create(2, 1);
                    if (SecondaryKeys.Equals(other.SecondaryKeys) == true)
                        return Tuple.Create(2, 2);
                }
                return null;
            }

            // remove assignments using this device
            public void RemoveDevice(string device, string NoDeviceName)
            {
                bool primarymatch = PrimaryKeys?.IsDevice(device) == true;
                bool secondarymatch = SecondaryKeys?.IsDevice(device) == true;

                if (primarymatch)
                {
                    if (secondarymatch)
                    {
                        ClearAll(NoDeviceName);
                    }
                    else if ( SecondaryKeys?.Assigned == true) // if secondary is assigned
                    {
                        PrimaryKeys = SecondaryKeys;        // move secondary to primary
                        SecondaryKeys = new DeviceKeyPairList(NoDeviceName);
                    }
                }
                else if (secondarymatch)
                {
                    SecondaryKeys.Clear(NoDeviceName);
                }
            }

            // rename device
            public void RenameDevice(string oldname, string newname)
            {
                PrimaryKeys?.RenameDevice(oldname, newname);
                SecondaryKeys?.RenameDevice(oldname, newname);
            }

            public bool SwapPrimarySecondary()
            {
                if (HasPrimaryAndSecondaryAssigned)
                {
                    var swap = new List<DeviceKeyPair>(SecondaryKeys.Keys);     // copy key list into temp array
                    SecondaryKeys = PrimaryKeys;
                    PrimaryKeys = new DeviceKeyPairList(swap);
                    return true;
                }
                else
                    return false;
            }

            public void ClearAll(string NoDeviceName)
            {
                if ( KeyOrBinding)
                { 
                    PrimaryKeys = new DeviceKeyPairList(NoDeviceName);
                    if ( !Binding )
                        SecondaryKeys = new DeviceKeyPairList(NoDeviceName);
                }
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
        }


        [System.Diagnostics.DebuggerDisplay("DKPL {KeyDescription()}")]

        public class DeviceKeyPairList
        {
            public List<DeviceKeyPair> Keys { get; set; }     // first is key, second is mod, always non null

            public int Count => Keys.Count;
            public bool Assigned => Keys.Count > 0 && Keys[0].Assigned;
            public bool IsKeyboard => Keys.Count > 0 && Keys[0].IsKeyboard && (Keys.Count == 1 || Keys[1].IsKeyboard);
            public bool IsJoystick(string nodevicename) => Assigned == true && Keys[0].Device != nodevicename && !Keys[0].IsKeyboard && !Keys[0].IsMouse;
            public bool IsDevice(string device) => Keys.Count > 0 && (Keys[0].Device == device || (Keys.Count > 1 && Keys[1].Device == device));

            public DeviceKeyPairList() { Keys = new List<DeviceKeyPair>(); }
            public DeviceKeyPairList(string nodevicename) { Keys = new List<DeviceKeyPair> { new DeviceKeyPair(nodevicename, "") }; }
            public DeviceKeyPairList(List<DeviceKeyPair> keylist) { Keys = keylist; }

            public void Add(DeviceKeyPair key) => Keys.Add(key);

            public void Clear(string nodevicename)
            {
                Keys = new List<DeviceKeyPair> { new DeviceKeyPair(nodevicename, "") };
            }

            public void ClearMod()
            {
                if (Keys.Count >= 2)
                    Keys.RemoveAt(1);
            }

            public void SetMod(string device, string key)
            {
                if (Keys.Count == 1)
                    Keys.Add(new DeviceKeyPair(device, key));
                else
                    Keys[0] = new DeviceKeyPair(device, key);
            }

            public void RenameDevice(string oldname, string newname)
            {
                foreach (var x in Keys.EmptyIfNull().Where(x => x.Device == oldname))
                    x.Device = newname;
            }


            // do the key set including modifier in the other one match with our keys
            // other may be null
            public bool Equals(DeviceKeyPairList other)
            {
                if (other != null && other.Keys.Count == Keys.Count && Keys[0].Assigned)
                {
                    if (Keys[0].Equals(other.Keys[0]) && (Keys.Count == 1 || (Keys[1].Assigned && Keys[1].Equals(other.Keys[1]))))      // use Equals, which is overloaded
                        return true;
                }
                return false;
            }

            // produce a key description string in the format Device:Key or (Device:Key,Device:Key)
            // use either the internal key name or frontier name
            public string KeyDescription()
            {
                if (Keys?.Count < 1)
                    return "";
                if (!Keys[0].Assigned)
                    return "---";

                string part = Keys[0].Device + ":" +  Keys[0].FrontierKeyName;
                if (Keys.Count > 1)
                    return "(" + part + "," + Keys[1].Device + ":" + Keys[1].FrontierKeyName+ ")";
                else
                    return part;
            }

            // do we have keys in common, other can be null
            public bool HasVKeyInCommon(DeviceKeyPairList other)        // TBD why vkeys?
            {
                if (other != null)
                {
                    foreach (var o in other.Keys.EmptyIfNull())
                    {
                        foreach (DeviceKeyPair k in Keys.EmptyIfNull())
                        {
                            if (k.VKeyName.Equals(o.VKeyName))
                                return true;
                        }
                    }
                }

                return false;
            }

            public string AssignVKeyNames(string layoutname)
            {
                string firstname = Keys[0].FrontierKeyName;
                string ErrorList = "";

                if (firstname.HasChars())       // empty frontier entries get null
                {
                    // pov evil.. frontier code these as primary (l/r/u/d) and modifier (l/r/u/d) in no particular order.  If POV, only the first entry gets a Key name, the second entry is null

                    int povindex = firstname.IndexOf("POV");
                    if (povindex >= 0 && Keys.Count > 1)       // if first and possibly second POV entries
                    {
                        string povroot = Keys[0].FrontierKeyName.Truncate(0, povindex + 4);
                        string secondname = Keys[1].FrontierKeyName;
                        if (secondname.Truncate(0, povroot.Length).Equals(povroot))       // POV pair..  lets adjust so its just the major entry.. makes it easier
                        {
                            if (firstname.Contains("Left") || secondname.Contains("Left"))
                                firstname = povroot + ((firstname.Contains("Up") || secondname.Contains("Up")) ? "UpLeft" : "DownLeft");
                            else if (firstname.Contains("Right") || secondname.Contains("Right"))
                                firstname = povroot + ((firstname.Contains("Up") || secondname.Contains("Up")) ? "UpRight" : "DownRight");
                        }

                        Keys[0].VKeyName = firstname;     // only the first entry get our key name
                        Keys[1].VKeyName = "POV-KEY-IGNORE";      // gets a text marker so its not null
                    }
                    else
                    {
                        foreach (var x in Keys)
                        {
                            x.VKeyName = x.FrontierKeyName;      // always set key name

                            if (x.Device == DeviceKeyPair.KeyboardDeviceName)
                            {
                                string ourkeyname = FrontierKeyConversion.FrontierToKeys(layoutname, x.FrontierKeyName);
                                if (ourkeyname.StartsWith("!"))
                                    ErrorList = ErrorList.AppendPrePad(ourkeyname.Substring(1), Environment.NewLine);
                                else
                                    x.VKeyName = ourkeyname;
                            }
                        }
                    }
                }
                return ErrorList;
            }
        }

        // convert the frontier name for POV's and keyboard keys to our naming and store in Key

        // Device vs Key/Frontier Key Name

        [System.Diagnostics.DebuggerDisplay("DKP {Device}:{FrontierKeyName}")]        
        public class DeviceKeyPair
        {
            public string Device { get; set; }                  // internal name of device
            public string FrontierKeyName { get; set; }         // frontier name
            public string VKeyName { get; set; }                // not part of bindings editor, but can be set up using bindindfile set vkey names

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

            public DeviceKeyPair(string nodevicename)
            {
                Device = nodevicename;
                FrontierKeyName = "";
            }

            public bool Equals(DeviceKeyPair other) => Device == other.Device && FrontierKeyName == other.FrontierKeyName;

            public const string KeyboardDeviceName = "Keyboard";
            public const string MouseDeviceName = "Mouse";
        }
    }
}
