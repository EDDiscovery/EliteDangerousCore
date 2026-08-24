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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        // which have a binding or key assignement
        public IEnumerable<BindingEntry> Entries => Elements.Values;
        public IEnumerable<BindingEntry> Assignments => Elements.Where(x => x.Value.HasAnyKeys).Select(x => x.Value);
        // which have values in attributes
        public IEnumerable<BindingEntry> Values => Elements.Where(x => x.Value.Attributes.Count>0).Select(x => x.Value);

        public const string ExternalNoDeviceName = "{NoDevice}";

        // allows user to define translations between external frontier device names and ones use in here and bindings editor
        // add to this to handle other specialise device names.
        // There must be a ExternalNoDeviceName
        public Dictionary<string, string> ConvertDeviceNameList = new Dictionary<string, string>() { [ExternalNoDeviceName] = "---" };
        public string NoDeviceName => ConvertDeviceNameList[ExternalNoDeviceName];      // this is the internal NoDeviceName
        public bool IsJoystick(string device) => device != NoDeviceName && !DeviceKeyPair.KeyboardDevice(device) && !DeviceKeyPair.MouseDevice(device);
        public bool IsNoDevice(string device) => device == NoDeviceName;

        public List<string> DeviceList => Devices.ToList();
        public List<string> DeviceListNoDevice => Devices.Where(x => x != NoDeviceName).ToList();
        public List<string> DeviceListNoKeyboardMouse => Devices.Where(x => x != DeviceKeyPair.KeyboardDeviceName && x != DeviceKeyPair.MouseDeviceName).ToList();
        public List<string> DeviceListNoKeyboardMouseDevice => Devices.Where(x => x != DeviceKeyPair.KeyboardDeviceName && x != DeviceKeyPair.MouseDeviceName &&
                                                                                x != NoDeviceName).ToList();

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

        // get bindings file name from path and odyssey
        public static string FindBindingsFile(string path, bool odyssey)
        {
            var presetfile = FindStartPreset(path, odyssey);
            if (presetfile != null)
                return FindBindingsFile(presetfile);
            else
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
            Devices.Add(NoDeviceName);

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
                    BindingEntry entry = new BindingEntry(rootelement.Name.ToString(), rootelement.Value.ToString(), NoDeviceName);

                    if (rootelement.HasElements)
                    {
                        foreach (XElement subelement in rootelement.Elements())
                        {
                            if (subelement.Name == "Binding")
                            {
                                //System.Diagnostics.Debug.WriteLine($"    {subelement.Name} `{rootelement.Name.LocalName}`");
                                //AxisNames.Add(rootelement.Name.LocalName);
                                entry.PrimaryKeys = AssignToDevice(rootelement.Name.ToString(), subelement);
                                entry.Binding = true;
                            }
                            else if (subelement.Name == "Primary")
                            {
                                //System.Diagnostics.Debug.WriteLine($"   {rootelement.Name.LocalName}:{subelement.Name}  = `{subelement.Value}`");
                                //KeyNames.Add(rootelement.Name.LocalName);
                                entry.PrimaryKeys = AssignToDevice(rootelement.Name.ToString(), subelement);
                            }
                            else if (subelement.Name == "Secondary")
                            {
                                //System.Diagnostics.Debug.WriteLine($"   {rootelement.Name.LocalName}:{subelement.Name}  = `{subelement.Value}`");
                                //KeyNames.Add(rootelement.Name.LocalName);
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

                        entry.AssignKeys();

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
                    var extdevname = ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.PrimaryKeys[0].Device).Key ?? entry.PrimaryKeys[0].Device;

                    if (entry.Binding)
                    {
                        XElement elem1 = new XElement("Binding");
                        elem1.Add(new XAttribute("Device", extdevname));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys[0].FrontierKeyName));
                        elm.Add(elem1);
                    }
                    else
                    {
                        XElement elem1 = new XElement("Primary");
                        elem1.Add(new XAttribute("Device", extdevname));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys[0].FrontierKeyName));

                        for (int i = 1; i < entry.PrimaryKeys.Count; i++)
                        {
                            XElement mod = new XElement("Modifier");
                            mod.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.PrimaryKeys[i].Device).Key ?? entry.PrimaryKeys[i].Device));
                            mod.Add(new XAttribute("Key", entry.PrimaryKeys[i].FrontierKeyName));
                            elem1.Add(mod);
                        }
                        elm.Add(elem1);

                        if (entry.SecondaryKeys != null)
                        {
                            XElement elemsecondary = new XElement("Secondary");
                            elemsecondary.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.SecondaryKeys[0].Device).Key ?? entry.SecondaryKeys[0].Device));
                            elemsecondary.Add(new XAttribute("Key", entry.SecondaryKeys[0].FrontierKeyName));

                            for (int i = 1; i < entry.SecondaryKeys.Count; i++)
                            {
                                XElement mod = new XElement("Modifier");
                                mod.Add(new XAttribute("Device", ConvertDeviceNameList.FirstOrDefault(kk => kk.Value == entry.SecondaryKeys[i].Device).Key ?? entry.SecondaryKeys[i].Device));
                                mod.Add(new XAttribute("Key", entry.SecondaryKeys[i].FrontierKeyName));
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


        // used to report on entry and key set associated with a found device/keyname
        public class DeviceKeySet
        {
            public BindingEntry Entry { get; set; }
            public List<DeviceKeyPair> Keys { get; set; }   
            public bool Primary { get; set; }
            public DeviceKeySet(BindingEntry entry, List<DeviceKeyPair> keys, bool primary )
            {
                Entry = entry;
                Keys = keys;
                Primary = primary;
            }
        }

        // return information on all keys/mods which match devicename/keyname. Keyname is the physical key, not the frontier name
        // if devicename = null just keynames are considered
        // if keyname = null just devices are considered
        // partial match allows for StartsWith
        // all device key pairs even mod ones are returned..
        public List<DeviceKeySet> FindDeviceKey(string devicename, string keyname, bool partialmatch)
        {
            var ret = new List<DeviceKeySet>();
            foreach (var kvp in Elements)
            {
                foreach (var kp in kvp.Value.PrimaryKeys.EmptyIfNull())
                {
                    if ((devicename == null || kp.Device.EqualsIIC(devicename)) && (keyname == null || (partialmatch ? kp.Key.StartsWithIIC(keyname) : kp.Key.EqualsIIC(keyname))))
                        ret.Add(new DeviceKeySet(kvp.Value, kvp.Value.PrimaryKeys, true));
                }
                foreach (var kp in kvp.Value.SecondaryKeys.EmptyIfNull())
                {
                    if ((devicename == null || kp.Device.EqualsIIC(devicename)) && (keyname == null || (partialmatch ? kp.Key.StartsWithIIC(keyname) : kp.Key.EqualsIIC(keyname))))
                        ret.Add(new DeviceKeySet(kvp.Value, kvp.Value.SecondaryKeys, false));
                }
            }

            return ret;
        }

        public BindingEntry FindAction(string name, bool withkeys = true)
        {
            return Elements.TryGetValue(name,out var action) ? (withkeys ? (action.HasAnyKeys ? action : null) : null) : null;   
        }

        public void AddDevice(string name)
        {
            Devices.Add(name);
        }

        public void RemoveDevice(string name)
        {
            foreach (var element in Elements)
                element.Value.RemoveDevice(name);
            Devices.Remove(name);
        }
        public void RenameDevice(string oldname, string newname)
        {
            foreach (var element in Elements)
                element.Value.RenameDevice(oldname,newname);
            Devices.Remove(oldname);
            Devices.Add(newname);
        }

        public void Clear()
        {
            foreach (var element in Elements)
                element.Value.ClearAll();
        }

        public string ListBindings()
        {
            string ret = "";
            foreach (var device in DeviceListNoDevice)
            {
                ret += device + Environment.NewLine;
                var dks = FindDeviceKey(device, null, false);
                foreach (var x in dks)
                    ret += "  " + DeviceKeyPair.KeyDescription(x.Keys, true) + "=" + x.Entry.Name + Environment.NewLine;
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
        private List<DeviceKeyPair> AssignToDevice(string actionname, XElement mapping)
        {
            XAttribute xdevice = mapping.Attribute("Device");       // always Device/Key even if axis
            XAttribute xkey = mapping.Attribute("Key");

            if (xdevice != null && xkey != null)
            {
                string assignmentxml = mapping.Name.ToString();         // 'Primary' 'Secondary' 'Binding'

                List<DeviceKeyPair> dvp = new List<DeviceKeyPair>();

                string extname = xdevice.Value;
                string devname = ConvertDeviceNameList.TryGetValue(extname, out string intname) ? intname : extname;
                string frontierkeyname = xkey.Value;

                Devices.Add(devname);
                dvp.Add(new DeviceKeyPair(devname, frontierkeyname));      // push as first key

                foreach (XElement y in mapping.Descendants())
                {
                    if (y.Name == "Modifier")
                    {
                        extname = y.Attribute("Device").Value;
                        devname = ConvertDeviceNameList.TryGetValue(extname, out intname) ? intname : extname;
                        frontierkeyname = y.Attribute("Key").Value;

                        Devices.Add(devname);
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

        private Dictionary<string, BindingEntry> Elements { get; set; } = new Dictionary<string, BindingEntry>();

        // device list
        private HashSet<string> Devices { get; set; } = new HashSet<string>();

        #endregion
    }
}
