/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using QuickJSON;
using System.Diagnostics;
using System.Xml.Linq;
using BaseUtils;

namespace EliteDangerousCore.GMO
{
    [DebuggerDisplay("GMO {Type} {NameList}")]
    public class GalacticMapObject
    {
        public int ID { get; set; }
        public string Type { get; private set; }
        public List<string> Names { get; private set; }     // now have a naming list to remove duplicate positions
        public string Summary { get; private set; }         // GEC only
        public string GalMapSearch { get; private set; }
        public string GalMapUrl { get; private set; }
        public List<Vector3> Points { get; private set; }
        public string Description { get; private set; }
        public GalMapType GalMapType { get; private set; }
        public string NameList { get { return string.Join(", ", Names); } }
        public bool IsName(string name, bool contains = false)
        {
            foreach (var gmoname in Names)
            {
                if (gmoname.Equals(name, StringComparison.InvariantCultureIgnoreCase) || (contains && gmoname.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    return true;
            }

            return false;
        }
        public bool IsNameWildCard(string name, bool caseinsensitive)
        {
            foreach (var gmoname in Names)
            {
                if (gmoname.WildCardMatch(name,caseinsensitive))
                    return true;
            }

            return false;
        }

        public GalacticMapObject()
        {
            Points = new List<Vector3>();
        }


        // programatically
        public GalacticMapObject(string type, string name, string desc, Vector3 pos)
        {
            this.Type = type;
            this.Names = new List<string> { name };
            this.Description = desc;
            this.GalMapSearch = GalMapUrl = "";
            Points = new List<Vector3>() { pos };
            SetGalMapTypeFromTypeName();
        }

        // from EDSM JO
        public GalacticMapObject(JObject jo, int idoffset)
        {
            ID = jo["id"].Int() + idoffset;
            Type = jo["type"].Str("Not Set");
            Names = new List<string> { jo["name"].Str("No name set") };
            Summary = jo["summary"].Str("");
            GalMapSearch = jo["galMapSearch"].Str("");
            GalMapUrl = jo["galMapUrl"].Str("");
            Description = jo["descriptionMardown"].Str("No description");       // default back up description in case html fails

            var descriptionhtml = jo["descriptionHtml"].StrNull();

            if (descriptionhtml != null)
            {
                string xmltext = "<Body> " + descriptionhtml + " </Body>";
                string res = xmltext.XMLtoText();
                if (res != null)
                    Description = res;
                else
                {
                    Description = Summary + Environment.NewLine + Description;
                    //System.Diagnostics.Debug.WriteLine($"Description XML in error for {Name}\r\n{xmltext.LineNumbering(1,"N0",newline:"\n")}");
                }
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

           // System.Diagnostics.Debug.WriteLine($"GMO {Name} {Type} {Points[0].X} {Points[0].Y} {Points[0].Z}");
        }

        private void SetGalMapTypeFromTypeName()
        {
            GalMapType ty = GalMapType.GalTypes.Find(x => x.TypeName.Equals(Type));

            if (ty == null)
                ty = GalMapType.GalTypes.Find(x => x.Description.Contains(Type));
            if ( ty == null)
            {
                ty = GalMapType.GalTypes.Find(x => x.VisibleType == GalMapType.VisibleObjectsType.EDSMUnknown);      // select edsm unknown
                System.Diagnostics.Debug.WriteLine($"GMO unknown type {Type}");
            }

            GalMapType = ty;
        }

        public ISystem GetSystem()
        {
            return (Points.Count > 0) ? new SystemClass(Names[0], null, Points[0].X, Points[0].Y, Points[0].Z) : null;
        }

        public void AddDuplicateGMODescription(string name, string s)
        {
            Names.Add(name);
            Description += s;
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

