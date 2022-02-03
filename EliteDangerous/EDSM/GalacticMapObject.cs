/*
 * Copyright © 2016-2020 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using QuickJSON;
using System.Diagnostics;
using System.Xml.Linq;
using BaseUtils;

namespace EliteDangerousCore.EDSM
{
    public class GalacticMapObject
    {
        public int ID { get; private set; }
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string GalMapSearch { get; private set; }
        public string GalMapUrl { get; private set; }
        public List<Vector3> Points { get; private set; }
        public string Description { get; private set; }
        public GalMapType GalMapType { get; private set; }

        public GalacticMapObject()
        {
            Points = new List<Vector3>();
        }

        public GalacticMapObject(string type, string name, string desc, Vector3 pos)
        {
            this.Type = type;
            this.Name = name;
            this.Description = desc;
            this.GalMapSearch = GalMapUrl = "";
            Points = new List<Vector3>() { pos };
            SetGalMapTypeFromTypeName();
        }

        // from EDSM JO
        public GalacticMapObject(JObject jo)
        {
            ID = jo["id"].Int();
            Type = jo["type"].Str("Not Set");
            Name = jo["name"].Str("No name set");
            GalMapSearch = jo["galMapSearch"].Str("");
            GalMapUrl = jo["galMapUrl"].Str("");
            Description = jo["descriptionMardown"].Str("No description");       // default back up description in case html fails

            var descriptionhtml = jo["descriptionHtml"].StrNull();

            if (descriptionhtml != null)
            {
                string res = ("<Body>" + descriptionhtml + "</Body>").XMLtoText();
                if (res != null)
                    Description = res;
            }

            SetGalMapTypeFromTypeName();

            Points = new List<Vector3>();

            try
            {
                JArray coords = (JArray)jo["coordinates"];

                if (coords.Count > 0)
                {
                    if (coords[0].IsArray)
                    {
                        foreach (JArray ja in coords)
                        {
                            float x, y, z;
                            x = ja[0].Float();
                            y = ja[1].Float();
                            z = ja[2].Float();
                            Points.Add(new Vector3(x, y, z));
                        }
                    }
                    else
                    {
                        JArray plist = coords;

                        float x, y, z;
                        x = plist[0].Float();
                        y = plist[1].Float();
                        z = plist[2].Float();
                        Points.Add(new Vector3(x, y, z));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("GalacticMapObject parse coordinate error: type" + Type + " " + ex.Message);
                Points = null;
            }
        }

        private void SetGalMapTypeFromTypeName()
        {
            GalMapType ty = GalMapType.GalTypes.Find(x => x.TypeName.Equals(Type));

            if (ty == null)
            {
                ty = GalMapType.GalTypes.Find(x => x.VisibleType == GalMapType.VisibleObjectsType.EDSMUnknown);      // select edsm unknown
                System.Diagnostics.Debug.WriteLine($"GMO unknown type {Type}");
            }

            GalMapType = ty;
        }

        public GalacticMapSystem GetSystem(ISystem sys = null)
        {
            if (sys != null)
                return new EDSM.GalacticMapSystem(sys, this);
            else
                return new EDSM.GalacticMapSystem(this);
        }

        public void PrintElement(XElement x, int level)
        {
            string pad = "                    ".Substring(0, level);
            System.Diagnostics.Debug.WriteLine(pad + $"{x.NodeType} {x.Name} : {x.Value}");
            //                if (x.NodeType == System.Xml.XmlNodeType.Element)
            if (x.HasAttributes)
            {
                foreach (var y in x.Attributes())
                {
                    System.Diagnostics.Debug.WriteLine(pad + $" {x.Name} attr {y.NodeType} {y.Name} : {y.Value}");
                }
            }
            if (x.HasElements)
            {
                foreach (XElement y in x.Descendants())
                {
                    PrintElement(y, level + 1);
                }
            }
        }

    }
}

