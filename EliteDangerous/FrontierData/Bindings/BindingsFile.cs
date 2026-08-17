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
using System.Xml.Linq;

namespace EliteDangerousCore
{
    public partial class BindingsFile 
    {
        public bool IsLoaded => FileName != null;
        public string FileName { get; private set; }
        public string PresetName { get { return RootAttributes.TryGetValue("PresetName", out string s) ? s : "Unknown"; } set { RootAttributes["PresetName"] = value; } }

        // which have a binding or key assignement
        public IEnumerable<BindingEntry> Assignments => Elements.Where(x => x.Value.HasAnyKeys).Select(x => x.Value);
        // which have values in attributes
        public IEnumerable<BindingEntry> Values => Elements.Where(x => x.Value.Attributes.Count>0).Select(x => x.Value);
        public List<string> DeviceList => Devices.ToList();
        public List<string> DeviceListNoKeyboardMouse => Devices.Where(x => x != "Keyboard" && x != "Mouse").ToList();
        public List<string> DeviceListNoKeyboardMouseDevice => Devices.Where(x => x != "Keyboard" && x != "Mouse" && x != "{NoDevice}").ToList();

        // Frontier only allows one preset file, referenced in startpreset*.start, which is normally Custom*
        // logically its only one custom file allowed.
        // users may change that name
        // return file name of preset file and the index number assigned. null if not found
        public static Tuple<string,int> FindStartPreset(string path, bool odyssey)
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

                    return Tuple.Create(sfile, index);
                }
            }

            return null;
        }

        // get preset file name currently selected, null if start preset or the file is not there
        public static string FindPresetFileName(string path, bool odyssey)
        {
            var presetfile = FindStartPreset(path, odyssey);

            if (presetfile!=null)
            {
                string[] bindlist = FileHelpers.TryReadAllLinesFromFile(presetfile.Item1);

                if (bindlist != null)
                {
                    System.Diagnostics.Trace.WriteLine($"Bindings preset file {presetfile.Item1} contents {string.Join(";", bindlist)} index {presetfile.Item2}");

                    List<string> files = new List<string>();

                    foreach (string selline in bindlist)    // find first entry with name..
                    {
                        FileInfo[] allFiles = null;

                        // prefer files called with same index                     
                        if (presetfile.Item2 > 0)
                            allFiles = Directory.EnumerateFiles(path, selline + "." + presetfile.Item2.ToStringInvariant() + ".*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                        // else any files with this prefix
                        if (allFiles == null || allFiles.Length == 0)
                            allFiles = Directory.EnumerateFiles(path, selline + "*.binds", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                        if (allFiles.Length > 0)
                            return allFiles[0].FullName;
                    }
                }
            }

            return null;
        }

        // load file
        // give null if required since FindPresetFileName gives null if not set. This would occur for a user which never set a key
        // returns null if happy and loaded or error string
        public string Read(string filetoload)
        {
            Devices.Add("{NoDevice}");

            FileName = null;

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

                        if ( entry.PrimaryKeys != null )
                            entry.ClassMode = FrontierKeyClassification.GetClass(entry.Name);
                    }

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
                    if (entry.Binding)
                    {
                        XElement elem1 = new XElement("Binding");
                        elem1.Add(new XAttribute("Device", entry.PrimaryKeys[0].Device));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys[0].FrontierKeyName));
                        elm.Add(elem1);
                    }
                    else
                    {
                        XElement elem1 = new XElement("Primary");
                        elem1.Add(new XAttribute("Device", entry.PrimaryKeys[0].Device));
                        elem1.Add(new XAttribute("Key", entry.PrimaryKeys[0].FrontierKeyName));

                        for (int i = 1; i < entry.PrimaryKeys.Count; i++)
                        {
                            XElement mod = new XElement("Modifier");
                            mod.Add(new XAttribute("Device", entry.PrimaryKeys[i].Device));
                            mod.Add(new XAttribute("Key", entry.PrimaryKeys[i].FrontierKeyName));
                            elem1.Add(mod);
                        }
                        elm.Add(elem1);

                        if (entry.SecondaryKeys != null)
                        {
                            XElement elemsecondary = new XElement("Secondary");
                            elemsecondary.Add(new XAttribute("Device", entry.SecondaryKeys[0].Device));
                            elemsecondary.Add(new XAttribute("Key", entry.SecondaryKeys[0].FrontierKeyName));

                            for (int i = 1; i < entry.SecondaryKeys.Count; i++)
                            {
                                XElement mod = new XElement("Modifier");
                                mod.Add(new XAttribute("Device", entry.SecondaryKeys[i].Device));
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

        public void Rename(string presetname)
        {
            int lastpos = FileName.IndexOfIIC(PresetName);
            if (lastpos>=0)
            {
                FileName = FileName.Substring(0, lastpos) + presetname + FileName.Substring(lastpos + PresetName.Length);
                PresetName = presetname;
            }
        }

        public string FindDevice(string name, Guid instanceguid, Guid productguid, int productid, int vendorid)    // best match of physical device info to our binding devices
        {
            string bestmatch = null;
            int besttotal = 0;

            string frontiername = devicemapping.ContainsKey(new Tuple<int, int>(productid, vendorid)) ? devicemapping[new Tuple<int, int>(productid, vendorid)] : null;

            foreach (string dv in Devices)
            {
                if (dv.Equals(name, StringComparison.InvariantCultureIgnoreCase))      // exact match
                    return dv;

                if (frontiername != null && dv.Equals(frontiername, StringComparison.InvariantCultureIgnoreCase))
                    return dv;

                if (dv.Equals(GuidExtract(instanceguid, false), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(instanceguid, true), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(productguid, false), StringComparison.InvariantCultureIgnoreCase))
                    return dv;
                if (dv.Equals(GuidExtract(productguid, true), StringComparison.InvariantCultureIgnoreCase))
                    return dv;

                int total = dv.SplitCapsWord().ToLowerInvariant().ApproxMatch(name.ToLowerInvariant(), 4);
                if (total > besttotal)
                {
                    besttotal = total;
                    bestmatch = dv;
                }
            }

            if (bestmatch != null)
                return bestmatch;

            return null;
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
                element.Value.ClearAllKeys();
        }

        // given a action name, return a list of assigned keys/devices

        public BindingEntry FindAction(string name)
        {
            return Elements.TryGetValue(name,out var action) ? action : null;   
        }

        // used to find the keys associated with an logical action name in ActionKeyED and web pressed logical function
        // NULL if no match found 

        //public List<FrontierActionToKeys> FindAction(string actionname, string preferreddevice = null)
        //{
        //    if (AssignedActions.ContainsKey(actionname))
        //    {
        //        List<FrontierActionToKeys> ret = new List<FrontierActionToKeys>();

        //        foreach (var a in AssignedActions[actionname])        // search assignments under this name
        //        {
        //            if (preferreddevice == null || a.AllOfDevice(preferreddevice))      // if null, or all of this device.. return
        //                ret.Add(a);
        //        }

        //        return ret.Count > 0 ? ret : null;
        //    }

        //    return null;
        //}

        //public Dictionary<string, string> FindActionValues(string actionname)
        //{
        //    return AssignedActionsValues.TryGetValue(actionname, out Dictionary<string, string> v) ? v : null;
        //}

        // given a key name, find the list of assignments of FrontierActionToKeys for it
        //public List<FrontierActionToKeys> Find(string keyname, bool partialmatch)       
        //{
        //    List<FrontierActionToKeys> ret = new List<FrontierActionToKeys>();

        //    foreach (Device dv in Devices.Values)   // all devices..
        //    {
        //        List<FrontierActionToKeys> f = dv.Find(keyname, partialmatch);
        //        if (f != null)
        //            ret.AddRange(f);
        //    }

        //    return ret;
        //}

        //public string ListBindings()
        //{
        //    string ret = "";
        //    foreach (string s in Devices.Keys)
        //    {
        //        ret += s + Environment.NewLine + Mappings(s, "  ") + Environment.NewLine;
        //    }
        //    return ret;
        //}

        //public string ListKeyNames(string prefix = "", string postfix = "")
        //{
        //    return String.Join(Environment.NewLine, (from x in KeyNames select prefix + x + postfix));
        //}



        #region private


        // assign to mapping
        private List<DeviceKeyPair> AssignToDevice(string actionname, XElement mapping)
        {
            XAttribute xdevice = mapping.Attribute("Device");       // always Device/Key even if axis
            XAttribute xkey = mapping.Attribute("Key");

            if (xdevice != null && xkey != null)
            {
                string assignmentxml = mapping.Name.ToString();         // 'Primary' 'Secondary' 'Binding'
                string frontierkeyname = xkey.Value;

                List<DeviceKeyPair> dvp = new List<DeviceKeyPair>();

                foreach (XElement y in mapping.Descendants())
                {
                    if (y.Name == "Modifier")
                    {
                        string km = y.Attribute("Device").Value;
                        string vm = y.Attribute("Key").Value;

                        Devices.Add(km);

                        dvp.Add(new DeviceKeyPair(km, y.Attribute("Key").Value));
                    }
                }

                Devices.Add(xdevice.Value);
                // push as first key
                dvp.Insert(0, new DeviceKeyPair(xdevice.Value, frontierkeyname));

                return dvp;
            }
            else
                return null;
        }

        private string GuidExtract(Guid g, bool rev)
        {
            string s = g.ToString();
            int slash = s.IndexOf('-');
            if (slash >= 0)
                s = s.Substring(0, slash);

            if (rev && s.Length == 8)
            {
                s = s.Substring(4, 4) + s.Substring(0, 4);
            }

            return s;
        }


        //private string Mappings(string devicename, string prefix)
        //{
        //    StringBuilder s = new StringBuilder(128);
        //    if (Devices.ContainsKey(devicename))
        //    {
        //        foreach (string keyname in Devices[devicename].Assignments.Keys)
        //        {
        //            foreach (FrontierActionToKeys a in Devices[devicename].Assignments[keyname])
        //            {
        //                s.Append(prefix + a.ToString(devicename));
        //                s.AppendLine();
        //            }
        //        }
        //    }
        //    return s.ToString();
        //}


        private string ChkValue(string s)
        {
            return (s == "Value") ? "" : ("." + s);
        }


        // From frontier DeviceMapping.xml table using EDDTest devicemappings import, 8/4/22
        private Dictionary<Tuple<int, int>, string> devicemapping = new Dictionary<Tuple<int, int>, string>()
        {
          {  new Tuple<int,int>(0x28E, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x28F, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x2FF, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x5D04, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x2A1, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x4716, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x213, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x291, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x719, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0xF07, 0x44F), "GamePad" },
        {  new Tuple<int,int>(0xB326, 0x44F), "GamePad" },
        {  new Tuple<int,int>(0xC21D, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC21E, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC21F, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xC242, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0xCA84, 0x46D), "GamePad" },
        {  new Tuple<int,int>(0x4540, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4556, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4718, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4726, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4728, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4738, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x4740, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x6040, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xB726, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xBEEF, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xCB02, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xCB03, 0x738), "GamePad" },
        {  new Tuple<int,int>(0xF738, 0x738), "GamePad" },
        {  new Tuple<int,int>(0x8802, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x8809, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x880A, 0xC12), "GamePad" },
        {  new Tuple<int,int>(0x5, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x6, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x105, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x113, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x201, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x21F, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x301, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x401, 0xE6F), "GamePad" },
        {  new Tuple<int,int>(0x201, 0xE8F), "GamePad" },
        {  new Tuple<int,int>(0x3008, 0xE8F), "GamePad" },
        {  new Tuple<int,int>(0xA, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0xD, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0x16, 0xF0D), "GamePad" },
        {  new Tuple<int,int>(0x202, 0xF30), "GamePad" },
        {  new Tuple<int,int>(0x8888, 0xF30), "GamePad" },
        {  new Tuple<int,int>(0xFF0C, 0x102C), "GamePad" },
        {  new Tuple<int,int>(0x4, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x301, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x8809, 0x12AB), "GamePad" },
        {  new Tuple<int,int>(0x4748, 0x1430), "GamePad" },
        {  new Tuple<int,int>(0x8888, 0x1430), "GamePad" },
        {  new Tuple<int,int>(0x601, 0x146B), "GamePad" },
        {  new Tuple<int,int>(0x37, 0x1532), "GamePad" },
        {  new Tuple<int,int>(0x3F00, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0x3F0A, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0x3F10, 0x15E4), "GamePad" },
        {  new Tuple<int,int>(0xBEEF, 0x162E), "GamePad" },
        {  new Tuple<int,int>(0xFD00, 0x1689), "GamePad" },
        {  new Tuple<int,int>(0xFD01, 0x1689), "GamePad" },
        {  new Tuple<int,int>(0x2, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0x3, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF016, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF023, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF028, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF038, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF900, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF901, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0xF903, 0x1BAD), "GamePad" },
        {  new Tuple<int,int>(0x5000, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5300, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5303, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5500, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5501, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5506, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x5B02, 0x24C6), "GamePad" },
        {  new Tuple<int,int>(0x2D1, 0x45E), "GamePad" },
        {  new Tuple<int,int>(0x317, 0x7B5), "BlackWidow" },
        {  new Tuple<int,int>(0xC28, 0x6A3), "SaitekAV8R03" },
        {  new Tuple<int,int>(0x461, 0x6A3), "SaitekAV8R03" },
        {  new Tuple<int,int>(0x2215, 0x738), "SaitekX55Joystick" },
        {  new Tuple<int,int>(0x2221, 0x738), "SaitekX56Joystick" },
        {  new Tuple<int,int>(0xA215, 0x738), "SaitekX55Throttle" },
        {  new Tuple<int,int>(0xA221, 0x738), "SaitekX56Throttle" },
        {  new Tuple<int,int>(0x762, 0x6A3), "SaitekX52Pro" },
        {  new Tuple<int,int>(0x75C, 0x6A3), "SaitekX52" },
        {  new Tuple<int,int>(0x255, 0x6A3), "SaitekX52" },
        {  new Tuple<int,int>(0xC2AA, 0x46D), "LogitechG940Pedals" },
        {  new Tuple<int,int>(0xC2A8, 0x46D), "LogitechG940Joystick" },
        {  new Tuple<int,int>(0xC2A9, 0x46D), "LogitechG940Throttle" },
        {  new Tuple<int,int>(0x402, 0x44F), "ThrustMasterWarthogJoystick" },
        {  new Tuple<int,int>(0x404, 0x44F), "ThrustMasterWarthogThrottle" },
        {  new Tuple<int,int>(0xFFFF, 0x44F), "ThrustMasterWarthogCombined" },
        {  new Tuple<int,int>(0x1302, 0x738), "SaitekFLY5" },
        {  new Tuple<int,int>(0xB108, 0x44F), "ThrustMasterTFlightHOTASX" },
        {  new Tuple<int,int>(0xB67C, 0x44F), "ThrustMasterHOTAS4" },
        {  new Tuple<int,int>(0xB67B, 0x44F), "ThrustMasterHOTAS4" },
        {  new Tuple<int,int>(0xB68D, 0x44F), "TFlightHotasOne" },
        {  new Tuple<int,int>(0xB10A, 0x44F), "T16000M" },
        {  new Tuple<int,int>(0xB687, 0x44F), "T16000MTHROTTLE" },
        {  new Tuple<int,int>(0xB679, 0x44F), "T-Rudder" },
        //{  new Tuple<int,int>(0xFFFF, 0x44F), "TMwTARGET" },  // removed duplicate
        {  new Tuple<int,int>(0xC215, 0x46D), "LogitechExtreme3DPro" },
        {  new Tuple<int,int>(0xC121, 0x25F0), "GioteckPS3WiredController" },
        {  new Tuple<int,int>(0x53C, 0x6A3), "SaitekX45" },
        {  new Tuple<int,int>(0x8037, 0x2341), "EDTracker" },
        {  new Tuple<int,int>(0xC219, 0x46D), "Logitech710WirelessGamepad" },
        {  new Tuple<int,int>(0xC285, 0x46D), "LogitechWingManStrikeForce3D" },
        {  new Tuple<int,int>(0x5F0D, 0x6A3), "SaitekP2600RumbleForce" },
        {  new Tuple<int,int>(0xFF0C, 0x6A3), "SaitekP2500RumbleForce" },
        {  new Tuple<int,int>(0x353E, 0x6A3), "SaitekCyborgEvoWireless" },
        {  new Tuple<int,int>(0x764, 0x6A3), "SaitekProFlightCombatRudderPedals" },
        {  new Tuple<int,int>(0x763, 0x6A3), "SaitekProFlightRudderPedals" },
        {  new Tuple<int,int>(0xBEAD, 0x1234), "vJoy" },
        {  new Tuple<int,int>(0xF1, 0x68E), "CHProThrottle1" },
        {  new Tuple<int,int>(0xC0F1, 0x68E), "CHProThrottle2" },
        {  new Tuple<int,int>(0xF3, 0x68E), "CHFighterStick" },
        {  new Tuple<int,int>(0xC0F2, 0x68E), "CHProPedals" },
        {  new Tuple<int,int>(0xC0F4, 0x68E), "CHCombatStick" },
        {  new Tuple<int,int>(0xF4, 0x68E), "CHCombatStick" },
        {  new Tuple<int,int>(0x3, 0xE8F), "TrustPredator" },
        {  new Tuple<int,int>(0x268, 0x54C), "Playstation3Controller" },
        {  new Tuple<int,int>(0x460, 0x6A3), "SaitekST290Pro" },
        {  new Tuple<int,int>(0x3001, 0x47D), "XterminatorDualControl" },
        {  new Tuple<int,int>(0x1B, 0x45E), "SideWinderForceFeedback2" },
        {  new Tuple<int,int>(0x8036, 0x2341), "ArduinoLeonardo" },
        {  new Tuple<int,int>(0x11, 0x4D8), "SlawFlightControlRudder" },
        {  new Tuple<int,int>(0x211, 0x2833), "OculusTouch" },
        {  new Tuple<int,int>(0xBA0, 0x54C), "DualShock4" },
        {  new Tuple<int,int>(0x5C4, 0x54C), "DualShock4" },
        {  new Tuple<int,int>(0x9CC, 0x54C), "DualShock4" },

        };


        // all root attributes
        private Dictionary<string, string> RootAttributes { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, BindingEntry> Elements { get; set; } = new Dictionary<string, BindingEntry>();

        // device list
        private HashSet<string> Devices { get; set; } = new HashSet<string>();

        #endregion
    }
}
