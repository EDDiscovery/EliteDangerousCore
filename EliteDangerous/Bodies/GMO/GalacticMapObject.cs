/*
 * Copyright © 2016-2024 EDDiscovery development team
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
using BaseUtils;

namespace EliteDangerousCore.GMO
{
    [System.Diagnostics.DebuggerDisplay("GMO {GalMapTypes[0].Group} {GalMapTypes[0].VisibleType} {NameList}")]
    public class GalacticMapObject
    {
        public int ID { get; set; }
        public List<GalMapType> GalMapTypes { get; private set; }  // object type list
        public List<string> DescriptiveNames { get; private set; }     // now have a naming list to remove duplicate positions
        public SystemClass StarSystem { get; private set; }     // set if it has a associated star system, else null
        public string GalMapUrl { get; private set; }
        public List<Vector3> Points { get; private set; }
        public string Description { get; private set; }     // accumulated descriptions
        public string NameList { get { return string.Join(", ", DescriptiveNames); } }
        
        public bool IsDescriptiveName(string name, bool wildcard, bool contains = false)
        {
            foreach (var gmoname in DescriptiveNames)
            {
                if (wildcard)
                {
                    if (gmoname.WildCardMatch(name, true))
                        return true;
                }
                else
                {
                    if ( gmoname.Equals(name, StringComparison.InvariantCultureIgnoreCase) || (contains && gmoname.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0))
                        return true;
                }
            }

            return false;
        }

        public GalacticMapObject()
        {
            Points = new List<Vector3>();
        }

        // programatically
        public GalacticMapObject(string type, string gmoname, string starname, string descriptivetext, Vector3 pos)
        {
            GalMapTypes = new List<GalMapType>() { GetGalMapTypeFromTypeName(type) };
            DescriptiveNames = new List<string> { gmoname };
            Description = descriptivetext;
            StarSystem = new SystemClass(starname,null,pos.X,pos.Y,pos.Z);
            GalMapUrl = "";
            Points = new List<Vector3>() { pos };

           // System.Diagnostics.Debug.WriteLine($"GMOp {DescriptiveNames[0]} {GalMapType.Group} {GalMapType.VisibleType} : {StarSystem} : {Points[0].X} {Points[0].Y} {Points[0].Z}");
        }

        // from Json
        public GalacticMapObject(JObject jo, int idoffset)
        {
            ID = jo["id"].Int() + idoffset;
            DescriptiveNames = new List<string> { jo["name"].Str("No name set") };
            GalMapUrl = jo["galMapUrl"].Str("");
            Description = jo["descriptionMardown"].Str("No description");       // default back up description in case html fails

            string summary = jo["summary"].Str("");     //GEC only

            var descriptionhtml = jo["descriptionHtml"].StrNull();

            if (descriptionhtml != null)
            {
                string xmltext = "<Body> " + descriptionhtml + " </Body>";
                xmltext = xmltext.Replace("&deg;", "\u00b0");       // new dec 24 xml reader barfs on this

                string res = xmltext.XMLtoText();
                if (res != null)
                    Description = res;
                else
                {
                    Description = summary;
                    Description = Description.AppendPrePad(Description, Environment.NewLine);
                    //System.Diagnostics.Debug.WriteLine($"Description XML in error for {Name}\r\n{xmltext.LineNumbering(1,"N0",newline:"\n")}");
                }
            }

            GalMapTypes = new List<GalMapType>() { GetGalMapTypeFromTypeName(jo["type"].Str("Not Set")) };

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
                System.Diagnostics.Trace.WriteLine("GalacticMapObject parse coordinate error: " + Description + " " + ex.Message);
                Points = null;
            }

            string name = jo["galMapSearch"].Str("");       // see if the star name is there, if so, and its 1 point, store
            if (name.HasChars() && Points.Count == 1)
                StarSystem = new SystemClass(name, null, Points[0].X, Points[0].Y, Points[0].Z);

           // System.Diagnostics.Debug.Assert(GalMapType.VisibleType == null || StarSystem != null);  // check for all visible markers have a StarSystem
           // System.Diagnostics.Debug.WriteLine($"GMO {DescriptiveNames[0]} : {GalMapType.Group} : {GalMapType.VisibleType} : ss `{StarSystem}`");
        }

        // some GMO objects have multiple names/types at same position, accumulate
        public void AddDuplicate1(string type, string nameofobject, string descriptivetext)
        {
            if (!DescriptiveNames.Contains(nameofobject))      // don't double add the same
                DescriptiveNames.Add(nameofobject);

            var ty = GetGalMapTypeFromTypeName(type);
            if (!GalMapTypes.Contains(ty))
                GalMapTypes.Add(ty);

            Description += descriptivetext;

            //System.Diagnostics.Debug.WriteLine($"GMO Object repeat type {type} : {nameofobject} {descriptivetext}");
        }

        private GalMapType GetGalMapTypeFromTypeName(string typename)
        {
            GalMapType ty = GalMapType.GalTypes.Find(x => x.TypeName.EqualsIIC(typename));

            if (ty == null)
                ty = GalMapType.GalTypes.Find(x => x.Description.ContainsIIC(typename));

            if (ty == null)
            {
                ty = GalMapType.GalTypes.Find(x => x.VisibleType == GalMapType.VisibleObjectsType.EDSMUnknown);      // select edsm unknown
                System.Diagnostics.Debug.WriteLine($"******** GMO unknown type {typename}");
            }

            return ty;
        }


        //public void PrintElement(XElement x, int level)
        //{
        //    string pad = "                    ".Substring(0, level);
        //    System.Diagnostics.Debug.WriteLine(pad + $"{x.NodeType} {x.Name} : {x.Value}");
        //    //                if (x.NodeType == System.Xml.XmlNodeType.Element)
        //    if (x.HasAttributes)
        //    {
        //        foreach (var y in x.Attributes())
        //        {
        //            System.Diagnostics.Debug.WriteLine(pad + $" {x.Name} attr {y.NodeType} {y.Name} : {y.Value}");
        //        }
        //    }
        //    if (x.HasElements)
        //    {
        //        foreach (XElement y in x.Descendants())
        //        {
        //            PrintElement(y, level + 1);
        //        }
        //    }
        //}

    }
}

