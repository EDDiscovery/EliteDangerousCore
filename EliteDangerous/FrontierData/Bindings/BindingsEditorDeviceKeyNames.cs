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

using BaseUtils;
using ExtendedConditionsForms;
using ExtendedControls;
using QuickJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EliteDangerousCore.Bindings
{
    public class DeviceKeyNames : IEnumerable<DeviceKeyNames.DeviceEntry>
    {
        [System.Diagnostics.DebuggerDisplay("{DeviceList} {Buttons} {POV}")]
        public class DeviceEntry
        {
            public string DeviceList { get; set; }
            public int Buttons { get; set; }        // 0 not known
            public int POV { get; set; }            // 0 not known
            public string[] Axis { get; set; }      // empty
            public string[] Devices => DeviceList.Split(',');

            public class RenameEntry
            {
                public string Name { get; set; }
                public string Hint { get; set; }
            }

            public Dictionary<string, RenameEntry> Names = new Dictionary<string, RenameEntry>();
        }

        public DeviceEntry GetDeviveList(string deviceList)
        {
            return renames.Find(x => x.DeviceList == deviceList);
        }
        public DeviceEntry GetDevice(string frontierdevicename)
        {
            var dev = renames.Find(x => x.Devices.Contains(frontierdevicename));
            return dev;
        }

        public void Add(DeviceEntry dev)
        {
            renames.Add(dev);
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
                        DeviceEntry r = new DeviceEntry();
                        r.DeviceList = devices["Devices"].Str();
                        r.Buttons = devices["Buttons"].Int(0);
                        r.POV = devices["POV"].Int(0);
                        r.Axis = devices["Axis"].Str("").SplitNoEmptyStrings(',');

                        foreach (var kvp in devices["Keys"].Object())
                        {
                            string orgname = kvp.Key;
                            r.Names.Add(kvp.Key, new DeviceEntry.RenameEntry() { Name = kvp.Value["Rename"].Str(), Hint = kvp.Value["Hint"].StrNull() });
                        }

                        renames.Add(r);
                    }
                }
            }
        }

        public string GetRename(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out DeviceEntry.RenameEntry sn) ? sn.Name : bfname : bfname;
        }
        public string GetHint(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out DeviceEntry.RenameEntry sn) ? sn.Hint : null : null;
        }

        public string OriginalName(string bfdev, string rename)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? (dev.Names.Where(kvp => kvp.Value.Name == rename).Select(x => x.Key).FirstOrDefault() ?? rename) : rename;
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

        public void Edit(Form backcontrol, string layoutname, string frontierdevicename)
        {
#if false
            DeviceEntry dev = renames.Find(x => x.Devices.Contains(frontierdevicename));

            ConfigurableForm f = new ConfigurableForm();

            var devkname = GetDeviveList(frontierdevicename);
            int joystickbuttons = devkname?.Buttons ?? 128;
            int joystickpov = devkname?.POV ?? 2;
            string[] joystickaxis = devkname?.Axis ?? DeviceKeyNames.DeviceEntry.DefaultAxis;
            HashSet<string> keys = FrontierKeyConversion.FrontierKeyNames(layoutname, joystickaxis, joystickbuttons, joystickpov, false, false, false);

            const int dataleft = 100;
            Size labelsize = new Size(dataleft-10, 24);
            Size textboxsize = new Size(200, 24);
            Size oksize = new Size(100, 24);
            int hintleft = dataleft + textboxsize.Width + 10;
            int vpos = 40;
            const int vspacing = 32;

            f.AddOK(new Point(hintleft + textboxsize.Width - oksize.Width, vpos), "Press to accept changes", oksize);
            f.AddCancel(new Point(hintleft + textboxsize.Width - oksize.Width*2 - 10, vpos), "Press to cancel changes", oksize);
            vpos += vspacing;

            foreach( var keyname in keys)
            {
                DeviceEntry.RenameEntry re = null;
                dev?.Names.TryGetValue(keyname, out re);
                f.AddLabelAndEntry(keyname, new Point(4, 4), ref vpos, vspacing, labelsize, new ConfigurableEntryList.Entry(keyname, typeof(ExtTextBox), re?.Name ?? "", new Point(dataleft, 0), textboxsize, "Name of key"));
                f.Add(new ConfigurableEntryList.Entry("H-"+keyname, typeof(ExtTextBox), re?.Hint ?? "", new Point(hintleft, vpos-vspacing), textboxsize, "Tooltip Hint"));
            }

            f.InstallStandardTriggers();

            if (f.ShowDialogCentred(backcontrol, backcontrol.Icon, $"Edit Device {frontierdevicename}") == DialogResult.OK)
            {
                foreach( var keyname in keys)
                {
                    string name = f.Get(keyname);
                    string hint = f.Get("H-" + keyname);
                    if (name.HasChars())
                    {
                        dev.Names[keyname] = new DeviceEntry.RenameEntry() { Name = name, Hint = hint };
                    }
                    else
                        dev.Names.Remove(keyname);
                }
            }

                //Variables v = new Variables();
                //v["Devices"] = dev.DeviceList;
                //foreach (var kvp in dev.Names)
                //{
                //    v[kvp.Key] = kvp.Value.Name + (kvp.Value.Hint!=null ? (";" + kvp.Value.Hint) : "");
                //}
                //VariablesForm vf = new VariablesForm();
                //vf.Init(v, "Key set for devices {dlist}",this.FindForm().Icon);
                //if ( vf.ShowDialog() == DialogResult.OK )
                //{

                //}
#endif
        }

        public IEnumerator<DeviceEntry> GetEnumerator()
        {
            return renames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private List<DeviceEntry> renames = new List<DeviceEntry>();
    }
}
