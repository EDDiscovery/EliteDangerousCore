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

namespace EliteDangerousCore.Bindings
{
    public class DeviceKeyNames : IEnumerable<DeviceKeyNames.DeviceNameSet>
    {
        [System.Diagnostics.DebuggerDisplay("{DeviceList} {Buttons} {POV}")]
        public class DeviceNameSet
        {
            public DeviceNameSet(string s) { DeviceList = s; }
            public string DeviceList { get; set; }
            public int Buttons { get; set; } = -1;
            public int POV { get; set; } = -1;    
            public string[] Axis { get; set; } = new string[0];
            public string[] Devices => DeviceList.Split(',');

            public class KeyEntry
            {
                public string Name { get; set; }
                public string Hint { get; set; }
            }

            public Dictionary<string, KeyEntry> Names = new Dictionary<string, KeyEntry>();
        }

        public DeviceNameSet GetByDeviceList(string deviceList)
        {
            return renames.Find(x => x.DeviceList == deviceList);
        }

        public DeviceNameSet GetDevice(string frontierdevicename)
        {
            var dev = renames.Find(x => x.Devices.Contains(frontierdevicename));
            return dev;
        }

        public void Add(DeviceNameSet dev)
        {
            renames.Add(dev);
        }

        public string GetRename(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out DeviceNameSet.KeyEntry sn) ? sn.Name : bfname : bfname;
        }
        public string GetHint(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out DeviceNameSet.KeyEntry sn) ? sn.Hint : null : null;
        }

        public string OriginalName(string bfdev, string rename)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? (dev.Names.Where(kvp => kvp.Value.Name == rename).Select(x => x.Key).FirstOrDefault() ?? rename) : rename;
        }

        public void Set(string json)
        {
            renames.Clear();
            if (json != null)
            {
                JToken tk = JToken.Parse(json, out string err, JToken.ParseOptions.CheckEOL | JToken.ParseOptions.AllowTrailingCommas);

                if (tk?.IsObject == true)       // { Device : { bfname : rename, .. }, Device: } etc
                {
                    JArray devarray = tk["Devices"].Array();

                    foreach (JObject devices in devarray)
                    {
                        DeviceNameSet r = new DeviceNameSet(devices["Devices"].Str("Unknown"));
                        r.Buttons = devices["Buttons"].Int(-1);
                        r.POV = devices["POV"].Int(-1);
                        r.Axis = devices["Axis"].Str("").SplitNoEmptyStrings(',');

                        foreach (var kvp in devices["Keys"].Object())
                        {
                            string orgname = kvp.Key;
                            r.Names.Add(kvp.Key, new DeviceNameSet.KeyEntry() { Name = kvp.Value["Rename"].Str(), Hint = kvp.Value["Hint"].StrNull() });
                        }

                        renames.Add(r);
                    }
                }
            }
        }

        public string Get()
        {
            JObject jo = new JObject();
            jo["Version"] = "1.0.0.0";
            JArray devarray = new JArray();
            foreach (var dev in renames)
            {
                JObject obj = new JObject() { ["Devices"] = dev.DeviceList, ["Buttons"] = dev.Buttons,
                    ["POV"] = dev.POV,
                    ["Axis"] = String.Join(",",dev.Axis) };

                JObject keys = new JObject();
                obj.Add("Keys", keys);
                foreach (var x in dev.Names)
                {
                    keys[x.Key] = new JObject() { ["Rename"] = x.Value.Name, ["Hint"] = x.Value.Hint };
                }

                devarray.Add(obj);
            }
            jo["Devices"] = devarray;
            return jo.ToString(true);
        }

        public void Edit(Form backcontrol, string layoutname, Device device)
        {
            ConfigurableForm f = new ConfigurableForm();

            HashSet<string> keys = FrontierKeyConversion.FrontierKeyNames(device,true,true);

            const int dataleft = 100;
            Size labelsize = new Size(dataleft-10, 24);
            Size textboxsize = new Size(200, 24);
            Size oksize = new Size(100, 24);
            int hintleft = dataleft + textboxsize.Width + 10;
            const int vspacing = 32;

            // find dns or not, make dns
            DeviceNameSet dns = GetDevice(device.FrontierName);

            int vpos = 8;
            f.AddOK(new Point(hintleft + textboxsize.Width - oksize.Width, vpos), "Press to accept changes", oksize, paneltype: ConfigurableEntryList.Entry.PanelType.Top);
            f.AddCancel(new Point(hintleft + textboxsize.Width - oksize.Width*2 - 10, vpos), "Press to cancel changes", oksize, paneltype: ConfigurableEntryList.Entry.PanelType.Top);
            vpos += vspacing;
            f.AddLabelAndEntry("Device List", new Point(4, 4), ref vpos, vspacing, labelsize, 
                    new ConfigurableEntryList.Entry("Device-List", typeof(ExtTextBox), dns?.DeviceList ?? device.FrontierName, new Point(dataleft, 0), textboxsize, "Comma seperated list of frontier device names this applies to") { PlacedInPanel = ConfigurableEntryList.Entry.PanelType.Top });
            f.TopPanelHeight = vpos;


            vpos = 4;
            foreach( var keyname in keys)
            {
                DeviceNameSet.KeyEntry re = null;
                dns?.Names.TryGetValue(keyname, out re);
                f.AddLabelAndEntry(keyname, new Point(4, 4), ref vpos, vspacing, labelsize, new ConfigurableEntryList.Entry(keyname, typeof(ExtTextBox), re?.Name ?? "", new Point(dataleft, 0), textboxsize, "Name of key"));
                f.Add(new ConfigurableEntryList.Entry("H-"+keyname, typeof(ExtTextBox), re?.Hint ?? "", new Point(hintleft, vpos-vspacing), textboxsize, "Tooltip Hint"));
            }

            f.InstallStandardTriggers();

            if (f.ShowDialogCentred(backcontrol, backcontrol.Icon, $"Edit Device {device.BetterName}") == DialogResult.OK)
            {
                if (dns == null)
                {
                    dns = new DeviceNameSet(f.Get("Device-List"));
                    renames.Add(dns);
                }
                else
                {
                    dns.DeviceList = f.Get("Device-List");
                }

                foreach (var keyname in keys)
                {
                    string name = f.Get(keyname);
                    string hint = f.Get("H-" + keyname);
                    if (name.HasChars())
                    {
                        dns.Names[keyname] = new DeviceNameSet.KeyEntry() { Name = name, Hint = hint };
                    }
                    else
                        dns.Names.Remove(keyname);
                }
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
