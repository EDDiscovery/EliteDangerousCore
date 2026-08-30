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

using BaseUtils;
using BaseUtils.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EliteDangerousCore
{
    public partial class BindingsFile 
    {
        public bool IsLoaded => FileName != null;
        public string FileName { get; private set; }
        public DateTime FileWriteTime { get; private set; }
        public bool IsOutOfDate() => IsLoaded && File.GetLastWriteTimeUtc(FileName) > FileWriteTime;        // is our copy behind the one on the diskette?      
        public string PresetName { get { return RootAttributes.TryGetValue("PresetName", out string s) ? s : "Unknown"; } set { RootAttributes["PresetName"] = value; } }
        public string KeyboardCulture { get { return Elements.TryGetValue("KeyboardLayout", out BindingEntry v) ? v.Value : "Unknown"; } }
        public string KeyboardLayout { get { return FrontierKeyConversion.GetSupportedLayout(KeyboardCulture); } }      // if NULL, we do not know the keyboard layout
        public bool IsEditable => KeyboardLayout != null;       // we can edit it

        // element lists
        public IEnumerable<BindingEntry> Entries => Elements.Values;
        // which are bindings or keys
        public IEnumerable<BindingEntry> Assignments => Elements.Where(x => x.Value.KeyOrBinding).Select(x => x.Value);
        // which have values in attributes
        public IEnumerable<BindingEntry> Values => Elements.Where(x => x.Value.Attributes.Count>0).Select(x => x.Value);

        // allows user to define translations between external frontier device names and ones use internally in here (and therefore the bindings editor)
        // add to this to handle other specialise device names.
        // There must be a ExternalNoDeviceName
        private const string ExternalNoDeviceName = "{NoDevice}";
        public Dictionary<string, string> ConvertDeviceNameList = new Dictionary<string, string>() { [ExternalNoDeviceName] = "---" };
        public string NoDeviceName => ConvertDeviceNameList[ExternalNoDeviceName];      // this is the internal NoDeviceName
        public bool IsNoDevice(string device) => device == NoDeviceName;

        public List<string> DeviceList => devices.ToList();
        public List<string> DeviceListNoDevice => devices.Where(x => x != NoDeviceName).ToList();
        public List<string> DeviceListNoKeyboardMouse => devices.Where(x => x != DeviceKeyPair.KeyboardDeviceName && x != DeviceKeyPair.MouseDeviceName).ToList();
        public List<string> DeviceListNoKeyboardMouseDevice => devices.Where(x => x != DeviceKeyPair.KeyboardDeviceName && x != DeviceKeyPair.MouseDeviceName &&
                                                                                x != NoDeviceName).ToList();

        // creation
        public BindingsFile()
        {
            devices.Add(NoDeviceName);
        }

        // get bindings file name from path and odyssey
        public static string FindBindingsFile(string path, bool odyssey)
        {
            var presetfile = FindStartPreset(path, odyssey);
            if (presetfile != null)
                return FindBindingsFile(presetfile);
            else
                return null;
        }

        // Frontier only allows one preset file, referenced in startpreset*.start, which is normally Custom*
        // logically its only one custom file allowed.
        // users may change that name
        // return file name of preset file and the index number assigned. null if not found
        public static Tuple<string, DateTime, int> FindStartPreset(string path, bool odyssey)
        {
            if (Directory.Exists(path))
            {
                FileInfo[] allStarts = Directory.EnumerateFiles(path, odyssey ? "StartPreset.*.start" : "StartPreset.start", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                if (allStarts.Length == 0)
                    allStarts = Directory.EnumerateFiles(path, "StartPreset.start", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                if (allStarts.Length > 0)       // if there, may not be due to user not creating at least one key
                {
                    string sfile = allStarts[0].FullName;

                    // new startpreset.X.start files from odyssey 11 onwards.. isolate the number, 0 if not found
                    int i1 = sfile.IndexOf(".");
                    int i2 = sfile.IndexOf(".", i1 + 1);
                    int index = 0;
                    if (i1 >= 0 && i2 >= 0)
                        index = sfile.Substring(i1 + 1, i2 - i1 - 1).InvariantParseInt(0);

                    return Tuple.Create(sfile, File.GetLastWriteTimeUtc(sfile), index);
                }
            }

            return null;
        }

        public static string FindBindingsFile(Tuple<string, DateTime, int> presetfile )
        {
            string[] bindlist = FileHelpers.TryReadAllLinesFromFile(presetfile.Item1);

            if (bindlist != null)
            {
                System.Diagnostics.Trace.WriteLine($"Bindings preset file {presetfile.Item1} contents {string.Join(";", bindlist)} index {presetfile.Item2}");

                List<string> files = new List<string>();

                foreach (string selline in bindlist)    // find first entry with name..
                {
                    FileInfo[] allFiles = null;

                    string folder = Path.GetDirectoryName(presetfile.Item1);
                    // prefer files called with same index                     
                    if (presetfile.Item3 > 0)
                        allFiles = Directory.EnumerateFiles(folder, selline + "." + presetfile.Item3.ToStringInvariant() + ".*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                    // else any files with this prefix
                    if (allFiles == null || allFiles.Length == 0)
                        allFiles = Directory.EnumerateFiles(folder, selline + "*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                    if (allFiles.Length > 0)
                        return allFiles[0].FullName;
                }
            }

            return null;
        }


        // load file
        // give null if required since FindPresetFileName gives null if not set. This would occur for a user which never set a key
        // returns null if happy and loaded or error string
        public string Read(string filetoload)
        {
            FileName = null;
            FileWriteTime = DateTime.MinValue;

            if ( filetoload  == null || !File.Exists(filetoload))
            {
                return "File not found";
            }

            try  // XML crap excepts everywhere, catch all
            {

                XElement bindings = XElement.Load(filetoload);

                // here we store the root attributes in case we want to write the XML back out

                if (bindings.HasAttributes)
                {
                    foreach (XAttribute y in bindings.Attributes())
                    {
                        string attr = y.Name.ToString();
                        //System.Diagnostics.Debug.WriteLine($"Root {y.NodeType} {y.Name}");
                        RootAttributes[attr] = y.Value;

                    }
                }

                // for all top level elements

                foreach (XElement rootelement in bindings.Elements())
                {
                    BindingEntry entry = new BindingEntry(rootelement.Name.ToString(), rootelement.Value.ToString());

                    if (rootelement.HasElements)
                    {
                        foreach (XElement subelement in rootelement.Elements())
                        {
                            if (subelement.Name == "Binding")
                            {
                                //System.Diagnostics.Debug.WriteLine($"    {subelement.Name} `{rootelement.Name.LocalName}`");
                                entry.PrimaryKeys = AssignToDevice(rootelement.Name.ToString(), subelement);
                                entry.Binding = true;
                            }
                            else if (subelement.Name == "Primary")
                            {
                                //System.Diagnostics.Debug.WriteLine($"   {rootelement.Name.LocalName}:{subelement.Name}  = `{subelement.Value}`");
                                entry.PrimaryKeys = AssignToDevice(rootelement.Name.ToString(), subelement);
                            }
                            else if (subelement.Name == "Secondary")
                            {
                                //System.Diagnostics.Debug.WriteLine($"   {rootelement.Name.LocalName}:{subelement.Name}  = `{subelement.Value}`");
                                entry.SecondaryKeys = AssignToDevice(rootelement.Name.ToString(), subelement);
                            }
                            else 
                            {
                                var se = subelement.Attributes().ToArray();

                                if (se.Length == 1 && se[0].Name == "Value")
                                {
                                    entry.Values.Add(subelement.Name.ToString(), se[0].Value);
                                }
                                else
                                {
                                    System.Diagnostics.Trace.WriteLine($"Binding File Rejected Storing {rootelement.Name}: {subelement.Name}:{se[0].Name} = {se[0].Value}");
                                }
                            }
                        }

                        // if not binding, have primary, must have secondary
                        if (!entry.Binding && entry.PrimaryKeys.Count > 0 && entry.SecondaryKeys == null)       
                        {
                            System.Diagnostics.Debug.WriteLine($"Primary defined not secondary {entry.ToString()}");
                            entry.SecondaryKeys = new DeviceKeyPairList(NoDeviceName);
                        }

                        // fix any fuck ups by moving secondary back to primary if primary is empty
                        if ( entry?.PrimaryKeys?.Assigned == false && entry?.SecondaryKeys?.Assigned == true )      
                        {
                            System.Diagnostics.Debug.WriteLine($"Primary not defined secondary is {entry.ToString()}");
                            entry.PrimaryKeys = entry.SecondaryKeys;
                            entry.SecondaryKeys = new DeviceKeyPairList(NoDeviceName);
                            System.Diagnostics.Debug.WriteLine($" ->  {entry.ToString()}");
                        }

                        if ( entry.PrimaryKeys != null )
                            entry.ClassMode = FrontierBindingClassification.GetKeyClass(entry.Name);
                    }
                    else
                        entry.ClassMode = FrontierBindingClassification.GetValueClass(entry.Name);

                    if (rootelement.HasAttributes)
                    {
                        foreach (XAttribute y in rootelement.Attributes())
                        {
                            entry.Attributes.Add(y.Name.ToString(), y.Value);
                        }
                    }

                    Elements[rootelement.Name.ToString()] = entry;
                }

                FileName = filetoload;
                FileWriteTime = File.GetLastWriteTimeUtc(FileName);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Bindings exception " + ex);
                return "Bindings exception " + ex;
            }
        }

        public string ToXML()
        {
            XElement root = new XElement("Root");

            // all root attributes
            foreach (var kvp in RootAttributes)
            {
                XAttribute attr = new XAttribute(kvp.Key, kvp.Value);
                root.Add(attr);
            }

            // all elements in order of reading
            foreach (var kvp in Elements)
            {
                BindingEntry entry = kvp.Value;
                XElement elm = new XElement(kvp.Key);
                if (entry.Value.HasChars())
                    elm.Value = entry.Value;

                foreach (var k3 in entry.Attributes)
                {
                    XAttribute attr = new XAttribute(k3.Key, k3.Value);
                    elm.Add(attr);
                }

                if (entry.PrimaryKeys != null)
                {
                    var extdevname = ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.PrimaryKeys.Keys[0].Device).Key ?? entry.PrimaryKeys.Keys[0].Device;

                    if (entry.Binding)
                    {
                        XElement elem1 = new XElement("Binding");
                        elem1.Add(new XAttribute("Device", extdevname));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys.Keys[0].FrontierKeyName));
                        elm.Add(elem1);
                    }
                    else
                    {
                        XElement elem1 = new XElement("Primary");
                        elem1.Add(new XAttribute("Device", extdevname));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys.Keys[0].FrontierKeyName));

                        for (int i = 1; i < entry.PrimaryKeys.Keys.Count; i++)
                        {
                            XElement mod = new XElement("Modifier");
                            mod.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.PrimaryKeys.Keys[i].Device).Key ?? entry.PrimaryKeys.Keys[i].Device));
                            mod.Add(new XAttribute("Key", entry.PrimaryKeys.Keys[i].FrontierKeyName));
                            elem1.Add(mod);
                        }
                        elm.Add(elem1);

                        if (entry.SecondaryKeys != null)
                        {
                            XElement elemsecondary = new XElement("Secondary");
                            elemsecondary.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.SecondaryKeys.Keys[0].Device).Key ?? entry.SecondaryKeys.Keys[0].Device));
                            elemsecondary.Add(new XAttribute("Key", entry.SecondaryKeys.Keys[0].FrontierKeyName));

                            for (int i = 1; i < entry.SecondaryKeys.Keys.Count; i++)
                            {
                                XElement mod = new XElement("Modifier");
                                mod.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.SecondaryKeys.Keys[i].Device).Key ?? entry.SecondaryKeys.Keys[i].Device));
                                mod.Add(new XAttribute("Key", entry.SecondaryKeys.Keys[i].FrontierKeyName));
                                elemsecondary.Add(mod);
                            }
                            elm.Add(elemsecondary);
                        }
                    }
                }

                foreach (var element in entry.Values)
                {
                    var elem1 = new XElement(element.Key);
                    elem1.Add(new XAttribute("Value", element.Value));
                    elm.Add(elem1);
                }

                root.Add(elm);
            }

            string header = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine;
            string xml = root.ToString();
            //System.Diagnostics.Debug.WriteLine($"XML = {xml}");
            return header + xml;
        }

        // Save current setting to file
        public bool Save(bool createbackup) 
        {
            if ( createbackup )
            {
                int i = 1;
                while (File.Exists(FileName + $".{i}.bak"))     // determine back up
                    i++;
                BaseUtils.FileHelpers.TryCopy(FileName, FileName + $".{i}.bak", true);        // copy to a backup X.bak file
            }

            string xml = ToXML();

            if (BaseUtils.FileHelpers.TryWriteToFile(FileName, xml))
            {
                FileWriteTime = File.GetLastAccessTimeUtc(FileName);
                return true;
            }
            else
                return false;
        }

        public void Rename(string presetname)
        {
            int lastpos = FileName.IndexOfIIC(PresetName);
            if (lastpos>=0)
            {
                FileName = FileName.Substring(0, lastpos) + presetname + FileName.Substring(lastpos + PresetName.Length);
                PresetName = presetname;
            }
        }

        // call to assign vkeys to all bindings and keys given the layout name of the file
        public bool AssignVKeys()
        {
            if (KeyboardLayout != null)
            {
                foreach (var entry in Elements.Values)
                {
                    entry.PrimaryKeys?.AssignVKeyNames(KeyboardLayout);
                    entry.SecondaryKeys?.AssignVKeyNames(KeyboardLayout);
                }
                return true;
            }
            return false;
        }


        // used to report on entry and key set associated with a found device/keyname
        public class DeviceKeySet
        {
            public BindingEntry Entry { get; set; }
            public DeviceKeyPairList Keys { get; set; }   
            public bool Primary { get; set; }
            public DeviceKeySet(BindingEntry entry, DeviceKeyPairList keys, bool primary )
            {
                Entry = entry;
                Keys = keys;
                Primary = primary;
            }
        }

        // return information on all keys/mods which match devicename/keyname.
        // Keyname is the vkey name, not the frontier name. Must have called AssignVKey
        // if devicename = null just keynames are considered
        // if keyname = null just devices are considered
        // partial match allows for StartsWith
        // all device key pairs even mod ones are returned..
        public List<DeviceKeySet> FindDeviceVKey(string devicename, string vkeyname, bool partialmatch)
        {
            var ret = new List<DeviceKeySet>();
            foreach (var entry in Assignments)
            {
                if (entry.PrimaryKeys != null)
                {
                    foreach (var kp in entry.PrimaryKeys.Keys)
                    {
                        if ((devicename == null || kp.Device.EqualsIIC(devicename)) && (vkeyname == null || (partialmatch ? kp.VKeyName.StartsWithIIC(vkeyname) : kp.VKeyName.EqualsIIC(vkeyname))))
                            ret.Add(new DeviceKeySet(entry, entry.PrimaryKeys, true));
                    }
                }

                if (entry.SecondaryKeys != null)
                {
                    foreach (var kp in entry.SecondaryKeys.Keys)
                    {
                        if ((devicename == null || kp.Device.EqualsIIC(devicename)) && (vkeyname == null || (partialmatch ? kp.VKeyName.StartsWithIIC(vkeyname) : kp.VKeyName.EqualsIIC(vkeyname))))
                            ret.Add(new DeviceKeySet(entry, entry.SecondaryKeys, false));
                    }
                }
            }

            return ret;
        }

        public BindingEntry FindAction(string name, bool withkeys = true)
        {
            return Elements.TryGetValue(name,out var action) ? (withkeys ? (action.KeyOrBinding ? action : null) : null) : null;   
        }


        public void AddDevice(string name)
        {
            devices.Add(name);
        }

        public void RemoveDevice(string name, string nodevicename )
        {
            foreach (var element in Elements)
                element.Value.RemoveDevice(name, nodevicename);
            devices.Remove(name);
        }

        public void RenameDevice(string oldname, string newname)
        {
            foreach (var element in Elements)
                element.Value.RenameDevice(oldname,newname);
            devices.Remove(oldname);
            devices.Add(newname);
        }

        public void Clear()
        {
            foreach (var element in Elements)
                element.Value.ClearAll(NoDeviceName);
        }

        public string ListBindings()
        {
            string ret = "";
            foreach (var device in DeviceListNoDevice)
            {
                ret += device + Environment.NewLine;
                var dks = FindDeviceVKey(device, null, false);
                foreach (var x in dks)
                    ret += "  " + x.Keys.KeyDescription() + "=" + x.Entry.Name + Environment.NewLine;
            }
            return ret;
        }
        public string ListValues()
        {
            string ret = "";
            foreach (var kvp in Elements.Where(x=>x.Value.Values.Count>0 || x.Value.Attributes.Count>0))
            {
                foreach (var x in kvp.Value.Values)
                    ret += kvp.Value.Name + "." + x.Key + "=" + x.Value + Environment.NewLine;
                foreach (var x in kvp.Value.Attributes)
                    ret += kvp.Value.Name + "." + x.Key + "=" + x.Value + Environment.NewLine;
            }

            return ret;
        }

        #region private

        // assign to mapping
        private DeviceKeyPairList AssignToDevice(string actionname, XElement mapping)
        {
            XAttribute xdevice = mapping.Attribute("Device");       // always Device/Key even if axis
            XAttribute xkey = mapping.Attribute("Key");

            if (xdevice != null && xkey != null)
            {
                string assignmentxml = mapping.Name.ToString();         // 'Primary' 'Secondary' 'Binding'

                DeviceKeyPairList dvp = new DeviceKeyPairList();

                string extname = xdevice.Value;
                string devname = ConvertDeviceNameList.TryGetValue(extname, out string intname) ? intname : extname;
                string frontierkeyname = xkey.Value;

                devices.Add(devname);
                dvp.Add(new DeviceKeyPair(devname, frontierkeyname));      // push as first key

                foreach (XElement y in mapping.Descendants())
                {
                    if (y.Name == "Modifier")
                    {
                        extname = y.Attribute("Device").Value;
                        devname = ConvertDeviceNameList.TryGetValue(extname, out intname) ? intname : extname;
                        frontierkeyname = y.Attribute("Key").Value;

                        devices.Add(devname);
                        dvp.Add(new DeviceKeyPair(devname, frontierkeyname));
                    }
                }

                return dvp;
            }
            else
                return null;
        }

 
        // all root attributes
        private Dictionary<string, string> RootAttributes { get; set; } = new Dictionary<string, string>();

        // all xml elements
        private Dictionary<string, BindingEntry> Elements { get; set; } = new Dictionary<string, BindingEntry>();

        // device list
        private HashSet<string> devices { get; set; } = new HashSet<string>();

        // This prints out the vkeys associated with the OEM codes and using the custom binds file with those OEMs
        // keys assigned to frontier functions listed below shows you and builds the translation entries for frontiertovkey

        public void OemListKeys()
        {
            Dictionary<uint, string> scancodetoui = new Dictionary<uint, string>()
            {
                [0x29] = "UI_Up",       // `-=
                [0x0C] = "UI_Down",
                [0x0D] = "UI_Left",
                
                [0x1a] = "UI_Right",       // []
                [0x1b] = "UI_Select",

                [0x26] = "UI_Back",       // l;'#
                [0x27] = "UI_Toggle",
                [0x28] = "CycleNextPanel",
                [0x2B] = "CyclePreviousPanel",

                [0x56] = "CycleNextPage",       // \m,./
                [0x32] = "CyclePreviousPage",
                [0x33] = "CamPitchUp",
                [0x34] = "CamPitchDown",
                [0x35] = "CamYawLeft",
            };

            System.Diagnostics.Debug.WriteLine($"LName {InputLanguage.CurrentInputLanguage.LayoutName} Bindings Culture {KeyboardCulture} Bindings Name {KeyboardLayout}");
            foreach( var kvp in scancodetoui)
            {
                Keys vkey = (Keys)UnsafeNativeMethods.MapVirtualKey(kvp.Key, 1);
                string vkeyname = KeyObjectExtensions.VKeyToString(vkey);

                BindingEntry entry = Entries.Where(x => x.Name == kvp.Value).FirstOrDefault();
                string fname = entry.PrimaryKeys.Keys[0].FrontierKeyName;

                 if (fname.Substring(4) != vkeyname )
                {
                    //    System.Diagnostics.Debug.WriteLine($"Scan code {kvp.Key:X2} vkey {(int)vkey:X2} {vkeyname} = {entry.Name} = {entry.PrimaryKeys.Keys[0].FrontierKeyName}");
                    System.Diagnostics.Debug.WriteLine($"[Tuple.Create(\"{KeyboardLayout}\", \"{fname}\")] = \"{vkeyname}\",");
                }

                var vk = FrontierKeyConversion.FrontierToKeys(KeyboardLayout, fname);
                if ( vk != vkeyname) System.Diagnostics.Debug.WriteLine($"Error with [Tuple.Create(\"{KeyboardLayout}\", \"{fname}\")] = \"{vkeyname}\" {kvp.Value} Back with {vk}");

            }
        }

        #endregion
    }
}
