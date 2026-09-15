/*
 * Copyright 2026-2026 EDDiscovery development team
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

using ExtendedControls;
using QuickJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EliteDangerousCore.Bindings
{
    public class DeviceKeyNames : IEnumerable<DeviceKeyNames.DeviceNameSet>
    {
        [System.Diagnostics.DebuggerDisplay("{Device} {Buttons} {POV}")]
        public class DeviceNameSet
        {
            public DeviceNameSet(string s) { Device = s; }
            public string Device { get; set; }
            public string Name { get; set; }
            public int Buttons { get; set; } = 0;                   // default for these are not present
            public int POV { get; set; } = 0;    
            public string[] Axis { get; set; } = new string[0];     // never null, may be empty, always in XYZ form
            public string AxisList => Axis.Length > 0 ? string.Join(",", Axis) : "";      // not null
            public class KeyEntry
            {
                public string Name { get; set; }        // EDDtools ensure we have a text name
                public string Hint { get; set; }
                public string Icon { get; set; }

                public bool IconOnly { get; set; }  // if set, and we create button maps, only place the icon in the output
            }

            public Dictionary<string, KeyEntry> Names = new Dictionary<string, KeyEntry>();
        }

        public DeviceNameSet GetDevice(string frontierdevicename)
        {
            var dev = renames.Find(x => x.Device.EqualsIIC(frontierdevicename));
            return dev;
        }

        public void Add(DeviceNameSet dev)
        {
            renames.Add(dev);
        }

        public string GetRename(string bfdev, string bfname)
        {
            if (bfdev == Device.KeyboardDeviceName)
            {
                return bfname.Substring(4);     // remove KEY_
            }
            else
            {
                var dev = renames.Find(x => x.Device.EqualsIIC(bfdev));
                return dev != null ? dev.Names.TryGetValue(bfname, out DeviceNameSet.KeyEntry sn) ? sn.Name : bfname : bfname;
            }
        }
        public string GetHint(string bfdev, string bfname)
        {
            if (bfdev == Device.KeyboardDeviceName)
            {
                return null;
            }
            else
            {
                var dev = renames.Find(x => x.Device.EqualsIIC(bfdev));
                return dev != null ? dev.Names.TryGetValue(bfname, out DeviceNameSet.KeyEntry sn) ? sn.Hint : null : null;
            }
        }

        public string OriginalName(string bfdev, string rename)
        {
            if (bfdev == Device.KeyboardDeviceName)
            {
                return "Key_" + rename;
            }
            else
            {
                var dev = renames.Find(x => x.Device.EqualsIIC(bfdev));
                return dev != null ? (dev.Names.Where(kvp => kvp.Value.Name == rename).Select(x => x.Key).FirstOrDefault() ?? rename) : rename;
            }
        }

        public bool Set(string json)
        {
            renames.Clear();
            if (json != null)
            {
                JToken tk = JToken.Parse(json, out string err, JToken.ParseOptions.CheckEOL | JToken.ParseOptions.AllowTrailingCommas);

                if (tk?.IsObject == true)       // { Device : { bfname : rename, .. }, Device: } etc
                {
                    JArray devarray = tk["Devices"].Array();

                    foreach (JObject devices in devarray.EmptyIfNull())
                    {
                        DeviceNameSet r = new DeviceNameSet(devices["Device"].Str("Unknown"));
                        r.Name = devices["Name"].Str(r.Device);
                        r.Buttons = devices["Buttons"].Int(0);
                        r.POV = devices["POV"].Int(0);
                        r.Axis = devices["Axis"].Str("").SplitNoEmptyStrings(',');

                        foreach (var kvp in devices["Keys"].Object())
                        {
                            string orgname = kvp.Key;
                            r.Names.Add(kvp.Key, new DeviceNameSet.KeyEntry() 
                                {   
                                    Name = kvp.Value["Name"].Str(), 
                                    Hint = kvp.Value["Hint"].StrNull(),
                                    Icon = kvp.Value["Icon"].StrNull(),
                                    IconOnly = kvp.Value["IconOnly"].Bool(false),
                            });
                        }

                        renames.Add(r);
                    }

                    return true;
                }
            }
            return false;
        }

        public string Get(string onlydevice = null)
        {
            JObject jo = new JObject();
            jo["Version"] = "1.0.0.0";
            JArray devarray = new JArray();
            foreach (var dev in renames)
            {
                if (onlydevice == null || onlydevice == dev.Device)
                {
                    JObject obj = new JObject()
                    {
                        ["Device"] = dev.Device,
                        ["Name"] = dev.Name,
                        ["Buttons"] = dev.Buttons,
                        ["POV"] = dev.POV,
                        ["Axis"] = dev.AxisList
                    };

                    JObject keys = new JObject();
                    obj.Add("Keys", keys);
                    foreach (var x in dev.Names)
                    {
                        JObject o = new JObject();
                        o["Name"] = x.Value.Name;
                        if (x.Value.Hint.HasChars())
                            o["Hint"] = x.Value.Hint;
                        if (x.Value.Icon.HasChars())
                            o["Icon"] = x.Value.Icon;
                        if (x.Value.IconOnly)
                            o["IconOnly"] = true;
                        keys[x.Key] = o;
                    }

                    devarray.Add(obj);
                }
            }
            jo["Devices"] = devarray;
            return jo.ToString(true);
        }

        public string GetAsDeviceButtonMaps(string device)
        {
            XElement root = new XElement("Root");

            DeviceNameSet dev = renames.Find(x => x.Device == device);
            if (dev != null)
            {
                foreach( var kvp in dev.Names)
                {
                    XElement elm = new XElement(kvp.Key);
                    if (kvp.Value.IconOnly)
                        elm.Value = kvp.Value.Icon;
                    else
                        elm.Value = kvp.Value.Name + (kvp.Value.Icon.HasChars() ? " " + kvp.Value.Icon : "");
                    root.Add(elm);
                }
            }

            return $"<!--- {dev.Name} ({dev.Device}) -->" + Environment.NewLine + root.ToString();  
        }

        public void Edit(Form backcontrol, string layoutname, Device device)
        {
            // icons set, credit Frontier Elite Dangerous and Cmdr. Nutball
            // loadede at FrontierData.Bindings.KeyIcons.Name
            BaseUtils.Icons.IconSet set = new BaseUtils.Icons.IconSet();
            set.LoadIconsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // get icon list for drop down, prioritising none
            List<CheckedIconGroupUserControl.Item> icondropdownlist = new List<CheckedIconGroupUserControl.Item>();
            foreach (var icon in set.Names().OrderBy(x=>x))
            {
                Image s = set.Get(icon);
                string cutname = icon.Substring(icon.LastIndexOf(".") + 1);
                if ( cutname == "none")
                    icondropdownlist.Insert(0,new CheckedIconUserControl.Item(icon, cutname, s, button: true));
                else
                    icondropdownlist.Add(new CheckedIconUserControl.Item(icon, cutname, s, button: true));
            }

            ConfigurableForm f = new ConfigurableForm();

            // device, filled in by either by keynames info or by a hardware device, has the
            // info on numbers of axis buttons etc

            HashSet<string> devicekeys = FrontierKeyConversion.FrontierKeyNames(device,true,true);

            Size labelsize = new Size(100, 24);
            Size labelsize2 = new Size(60, 24);
            Size textboxsize = new Size(200, 24);
            Size iconsize = new Size(32, 32);
            Size iconcheckboxsize = new Size(40, 24);
            Size numberboxsize = new Size(80, 24);
            Size oksize = new Size(100, 24);
            int hintleft = labelsize.Width + textboxsize.Width + 10;
            int iconleft = hintleft + textboxsize.Width + 10;
            int icononlyleft = iconleft + iconsize.Width + 10;
            const int vspacing = 32;

            // find dns or not. Will be null if we don't have it
            DeviceNameSet dns = GetDevice(device.FrontierName);

            // top panel UI

            int vpos = 8;
            f.AddOK(new Point(icononlyleft + iconcheckboxsize.Width - oksize.Width, vpos), "Press to accept changes", oksize, paneltype: ConfigurableEntryList.Entry.PanelType.Top);
            f.AddCancel(new Point(icononlyleft + iconcheckboxsize.Width - oksize.Width*2 - 10, vpos), "Press to cancel changes", oksize, paneltype: ConfigurableEntryList.Entry.PanelType.Top);
            vpos += vspacing;
            f.AddLabelAndEntry("Device ID", new Point(4, 4), ref vpos, vspacing, labelsize,
                    new ConfigurableEntryList.Entry("Device-ID", typeof(ExtTextBox), dns?.Device ?? device.FrontierName, new Point(int.MinValue, 0), textboxsize, "Device ID, as per frontier device name") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });

            f.AddLabelAndEntry("Device Name", new Point(4, 4), ref vpos, vspacing, labelsize,
                    new ConfigurableEntryList.Entry("Device-Name", typeof(ExtTextBox), dns?.Name ?? "", new Point(int.MinValue, 0), textboxsize, "Device friendly name") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });

            int right = f.AddLabelAndEntry("Buttons", new Point(4, 4), ref vpos, 0, labelsize,
                    new ConfigurableEntryList.Entry("Button-Count", dns?.Buttons ?? 128, new Point(int.MinValue, 0), numberboxsize, "Number of Buttons") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });
            right += 8;
            right = f.AddLabelAndEntry("POVs", new Point(right, 4), ref vpos, 0, labelsize2,
                    new ConfigurableEntryList.Entry("POV-Count", dns?.POV ?? 1, new Point(int.MinValue, 0), numberboxsize, "Number of POVs") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });
            right += 8;
            f.AddLabelAndEntry("Axis", new Point(right, 4), ref vpos, vspacing, labelsize2,
                    new ConfigurableEntryList.Entry("Axis-Set", dns?.AxisList ?? Device.DefaultAxisList, new Point(int.MinValue, 0), textboxsize, "Axis Set") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });
            f.TopPanelHeight = vpos;

            // scroll panel UI
            vpos = 4;
            foreach( var keyname in devicekeys)
            {
                DeviceNameSet.KeyEntry re = null;
                dns?.Names.TryGetValue(keyname, out re);        // may not be present

                f.AddLabelAndEntry(keyname, new Point(4, 4), ref vpos, vspacing, labelsize, 
                            new ConfigurableEntryList.Entry(keyname, typeof(ExtTextBox), re?.Name ?? "", new Point(int.MinValue, 0), textboxsize, "Name of key"));
                f.Add(new ConfigurableEntryList.Entry("H-" + keyname, typeof(ExtTextBox), re?.Hint ?? "", new Point(hintleft, vpos - vspacing), textboxsize, "Tooltip Hint"));

                Image s = set.Get("FrontierData.Bindings.KeyIcons." + (re?.Icon ?? "[none]").Replace("[", "").Replace("]", ""));

                // text value holds the [name], and the ButtonImage overrides it with an image
                // we store the current setting in text value
                string iconname = re?.Icon ?? "[none]";
                f.Add(new ConfigurableEntryList.Entry("I-" + keyname, iconname, new Point(iconleft, vpos - vspacing), iconsize, $"{iconname} Icon for Frontier Button Display", icondropdownlist, s) { ImageSize = new Size(40,40), MultiColumns = true} );

                f.Add(new ConfigurableEntryList.Entry("C-" + keyname, re?.IconOnly ?? false, "I/O", new Point(icononlyleft, vpos - vspacing), iconcheckboxsize, "Select so when exported to Frontier Button Display only the icon is shown not the text"));
            }

            f.InstallStandardTriggers();

            f.TriggerAdv += (name, text, entry, obj2, obj3) =>
            {
                if (text.Contains("DropDownButtonPressed"))     // only respond to drop down event
                {
                    string sel = (string)entry;                 // full name of icon path
                    Image s = set.GetOrNull(sel);               // get image
                    string iconname = "[" + sel.Substring(sel.LastIndexOf(".") + 1) + "]";      // compute setting [name]
                    ConfigurableEntryList.Entry ent = obj2 as ConfigurableEntryList.Entry;
                    ((ExtButtonWithNewCheckedListBox)ent.Control).CloseDropDown();              // close drop down
                    ent.TextValue = iconname;       // record back the text setting (can't use Set since Set for a checkediconlistbutton changes the drop down setting)
                    f.Set(ent.Name, s);     // set the button image
                    f.SetToolTip(ent.Name, $"{iconname} Icon for Frontier Button Display");
                }
                
            };

            if (f.ShowDialogCentred(backcontrol, backcontrol.Icon, $"Edit Device") == DialogResult.OK)
            {
                if (dns == null)
                {
                    dns = new DeviceNameSet(f.Get("Device-ID"));
                    renames.Add(dns);
                }
                else
                {
                    dns.Device = f.Get("Device-ID");
                }

                dns.Name = f.Get("Device-Name");

                // triage and possibly set buttons pov and axis, if they are incorrect, ignore it

                int? buttons = f.GetInt("Button-Count");
                if (buttons != null)
                    dns.Buttons = buttons.Value;
                int? pov = f.GetInt("POV-Count");
                if (pov != null)
                    dns.POV = pov.Value;
                string[] axis = f.Get("Axis-Set").SplitNoEmptyStartFinish(',');
                string[] reordered = Device.TriageAxisList(axis);
                if (reordered != null)
                    dns.Axis = reordered;
                
                // update keys presented
                foreach (var keyname in devicekeys)
                {
                    string name = f.Get(keyname);
                    string hint = f.Get("H-" + keyname);
                    var entry = f.GetEntry("I-" + keyname);     // icon name is stored in ent.textvalue, get will return drop down setting not this, so direct access
                    string icon = entry.TextValue;
                    bool noicon = icon == "[none]";
                    var icononly = noicon ? false : f.GetBool("C-" + keyname);

                    if (name.HasChars())
                    {
                        dns.Names[keyname] = new DeviceNameSet.KeyEntry() { Name = name, Hint = hint.HasChars() ? hint : null , Icon = noicon ? null : icon, IconOnly = icononly.Value };
                    }
                    else
                        dns.Names.Remove(keyname);
                }

                System.Diagnostics.Debug.WriteLine(Get(device.FrontierName));
            }
        }

        public IEnumerator<DeviceNameSet> GetEnumerator()
        {
            return renames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private List<DeviceNameSet> renames = new List<DeviceNameSet>();
    }
}
