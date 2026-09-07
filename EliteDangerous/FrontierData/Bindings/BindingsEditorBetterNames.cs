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

namespace EliteDangerousCore
{
    public partial class BindingsEditor : UserControl
    {
        #region KeyNames

        public class KeyRenames : IEnumerable<KeyRenames.DeviceEntry>
        {
            public class DeviceEntry
            {
                public string DeviceList { get; set; }
                public string[] Devices => DeviceList.Split(',');

                public class RenameEntry
                {
                    public string Name { get; set; }
                    public string Hint { get; set; }
                }

                public Dictionary<string, RenameEntry> Names = new Dictionary<string, RenameEntry>();
            }

            public DeviceEntry Get(string deviceList)
            {
                return renames.Find(x => x.DeviceList == deviceList);
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
                    JToken tk = JToken.Parse(json, out string err);

                    if (tk != null)       // { Device : { bfname : rename, .. }, Device: } etc
                    {
                        foreach (JObject devices in tk)
                        {
                            DeviceEntry r = new DeviceEntry();
                            r.DeviceList = devices["Devices"].Str();

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
                JArray ja = new JArray();
                foreach (var dev in renames)
                {
                    JObject obj = new JObject() { ["Devices"] = dev.DeviceList };
                    JObject keys = new JObject();
                    obj.Add("Keys", keys);
                    foreach (var x in dev.Names)
                    {
                        keys[x.Key] = new JObject() { ["Rename"] = x.Value.Name, ["Hint"] = x.Value.Hint };
                    }

                    ja.Add(obj);
                }
                return ja.ToString(true);
            }
            public void Edit(Form backcontrol, string layoutname, string device)
            {
                DeviceEntry dev = renames.Find(x => x.Devices.Contains(device));

                ConfigurableForm f = new ConfigurableForm();

                HashSet<string> keys = FrontierKeyConversion.FrontierKeyNames(layoutname, true, true, false, false);

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

                if (f.ShowDialogCentred(backcontrol, backcontrol.Icon, $"Edit Device {device}") == DialogResult.OK)
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



        #endregion


        #region Naming

        public string BetterName(string name)
        {
            return showFrontierNamesToolStripMenuItem.Checked ? name : name.SplitCapsWordFull().Replace("Buggy", "SRV").Replace("Turret", "SRV Turret").Replace("Humanoid", "On Foot").ReplaceIfStartsWith("Cam ", "Galaxy Map ");
        }
        public string BetterDevice(string name)
        {
            return ConvertDeviceNameList.TryGetValue(name, out var bettername) ? bettername : name;
        }
        public string OriginalDeviceName(string name)
        {
            return ConvertDeviceNameList.Where(kvp => kvp.Value == name).Select(x => x.Key).FirstOrDefault() ?? name;
        }

        #endregion

    }
}
