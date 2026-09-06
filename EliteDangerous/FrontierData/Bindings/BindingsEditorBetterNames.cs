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

        public class JoyStickRename
        {
            public string DeviceList { get; set; }
            public string[] Devices => DeviceList.Split(',');

            public class JoyStickName
            {
                public string Name { get; set; }
                public string Hint { get; set; }
            }

            public Dictionary<string, JoyStickName> Names = new Dictionary<string, JoyStickName>();
        }

        private List<JoyStickRename> renames = new List<JoyStickRename>();

        public void SetKeyConfigurationList(string json)
        {
            renames.Clear();    
            if (json != null)
            {
                JToken tk = JToken.Parse(json, out string err);

                if (tk != null)       // { Device : { bfname : rename, .. }, Device: } etc
                {
                    foreach (JObject devices in tk)
                    {
                        JoyStickRename r = new JoyStickRename();
                        r.DeviceList = devices["Devices"].Str();

                        foreach (var kvp in devices["Keys"].Object())
                        {
                            string orgname = kvp.Key;
                            r.Names.Add(kvp.Key, new JoyStickRename.JoyStickName() { Name = kvp.Value["Rename"].Str(), Hint = kvp.Value["Hint"].StrNull() });
                        }

                        renames.Add(r);
                    }
                }
            }
        }

        public string GetKeyConfigurationList()
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

        public void EditKeyConfigurationList(string device)
        {
            var dev = renames.Find(x => x.Devices.Contains(device));

            if ( dev != null )
            {
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
        }

        public string BetterKeyName(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out JoyStickRename.JoyStickName sn) ? sn.Name : bfname : bfname;
        }
        public string BetterKeyNameHint(string bfdev, string bfname)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? dev.Names.TryGetValue(bfname, out JoyStickRename.JoyStickName sn) ? sn.Hint : null : null;
        }

        public string OriginalKeyName(string bfdev, string name)
        {
            var dev = renames.Find(x => x.Devices.Contains(bfdev));
            return dev != null ? (dev.Names.Where(kvp => kvp.Value.Name == name).Select(x => x.Key).FirstOrDefault()??name) : name;
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
