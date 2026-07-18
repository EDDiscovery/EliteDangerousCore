/*
 * Copyright 2026-2026 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in coSmpliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using QuickJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Schema;

namespace EliteDangerousCore
{
    // purposely not doing auto conversion to/from string so the use of FDName can be found easier

    [System.Diagnostics.DebuggerDisplay("FD {fdname}")]
    public class FDName : IEquatable<FDName>, IComparable<FDName>, IEquatable
    {
        public FDName()
        {
            this.fdname = this.fdname_lower = "Unknown";
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public FDName(string fdname)
        {
            this.fdname = fdname.HasChars() ? fdname : "Unknown";
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public FDName(QuickJSON.JToken token)      // new July 26 QuickJson constructor
        {
            string txt = token.Str("Unknown");
            this.fdname = txt;
            this.fdname_lower = this.fdname.ToLowerInvariant();
            this.hashcode = this.fdname_lower.GetHashCode();
        }

        public static FDName Empty => new FDName();

        public FDName Clone()
        {
            return new FDName(this.fdname);
        }

        public bool IsValid => !this.fdname.Equals("Unknown");

        public string Str()
        {
            return fdname;
        }
        public string WithQuotes()
        {
            return fdname.AlwaysQuoteString();
        }
        public string ToLower()
        {
            return fdname_lower;
        }
        public string SplitCapsWordFull()
        {
            return fdname.SplitCapsWordFull();
        }

        public QuickJSON.JToken ToJToken()      // new July26 converter for JTOKEN
        {
            return new JToken(fdname);
        }

        #region Compare

        public static bool operator ==(FDName left, FDName right) { return left is null && right is null ? true : right is null ? false : left.Equals(right); }
        public static bool operator !=(FDName left, FDName right) { return left is null && right is null ? false : left is null ? true : !left.Equals(right); }

        public override bool Equals(Object obj)        // other may be null
        {
            return obj is FDName other ? other.fdname_lower.EqualsIIC(this.fdname_lower) : false;
        }

        public bool Equals(FDName right)        // other may be null
        {
            return !(right is null) && right.fdname_lower.EqualsIIC(this.fdname_lower);
        }

        public bool Equals(string right)        // other may be null
        {
            return right is null ? false : this.fdname_lower.EqualsIIC(right);
        }
        public int CompareTo(FDName other)
        {
            return fdname_lower.CompareTo(other.fdname_lower); 
        }

        public int GetHashCode(FDName obj)
        {
            return obj.hashcode;
        }
        public override int GetHashCode()
        {
            return hashcode;
        }

        public bool Contains(string partname)
        {
            return fdname_lower.ContainsIIC(partname.ToLowerInvariant());
        }

        public bool EndsWith(string partname)
        {
            return fdname_lower.EndsWithIIC(partname.ToLowerInvariant());
        }

        #endregion

        #region Misc
        public int GetClass()
        {
            int ci = fdname_lower.IndexOf("class");
            int classn = ci > 0 ? fdname_lower.Substring(ci + 5, 1).InvariantParseInt(0) : 0;
            return classn;
        }

        public bool IsWeaponArmour()
        {
            return fdname_lower.StartsWith("hpt_", StringComparison.InvariantCultureIgnoreCase) ||
                                            fdname_lower.StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase) ||
                                            fdname_lower.Contains("_armour_", StringComparison.InvariantCultureIgnoreCase);
        }

        public string GetForeignModuleName(string loc = null, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)
        {
            if (ItemData.TryGetShipModule(this, out ItemData.ShipModule item, true, slot))
                return item.TranslatedModTypeString();
            else
                return loc ?? fdname;
        }
        public string GetForeignModuleType(string loc = null, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)
        {
            if (ItemData.TryGetShipModule(this, out ItemData.ShipModule item, true, slot))
                return item.TranslatedModTypeString();
            else
                return loc ?? fdname;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }


        #endregion

        #region vars
        private string fdname;
        private string fdname_lower;
        private int hashcode;
        #endregion
    }

    public class FDNameEqualityComparer : IEqualityComparer<FDName>
    {
        public bool Equals(FDName left, FDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(FDName obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class FDNameHelpers
    {
        public static FDName FDName(this JToken tk)     // always gives a non null fdname with non null str()
        {
            return new FDName(tk != null ? tk.Str() : null);
        }
        
        public static bool IsValid(this FDName s)       // tdb?
        {
            return s?.IsValid == true;
        }

        public static FDName ToFD(this string str)
        {
            return new FDName(str);
        }

        public static string StrNull(this FDName s)
        {
            return s?.Str();
        }
        public static string StrAlt(this FDName s, string alt)
        {
            return s != null ? s.Str() : alt;
        }

        public static string RemoveFDDecoration(this string fdname)
        {
            if (fdname.Length >= 8 && fdname.StartsWith("$") && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                return fdname.Substring(1, fdname.Length - 7); // 1 for '$' plus 6 for '_name;'

            string s = fdname;
            if (s.StartsWith("$int_"))
                s = s.Replace("$int_", "Int_");
            if (s.StartsWith("int_"))
                s = s.Replace("int_", "Int_");
            if (s.StartsWith("$hpt_"))
                s = s.Replace("$hpt_", "Hpt_");
            if (s.StartsWith("hpt_"))
                s = s.Replace("hpt_", "Hpt_");
            if (s.Contains("_armour_"))
                s = s.Replace("_armour_", "_Armour_");      // normalise to Armour upper cas.. its a bit over the place with case..
            if (s.EndsWith("_name;", StringComparison.InvariantCultureIgnoreCase))
            {
                //System.Diagnostics.Debug.WriteLine("Correct " + s);
                s = s.Substring(0, s.Length - 6);
            }
            if (s.StartsWith("$"))                          // seen instances of $python_armour..
                s = s.Substring(1);

            return s;
        }

        public static FDName NormaliseShip(string fdname, out string shipname, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    shipname = null;
                    return null;
                }
                else
                {
                    shipname = "Unknown Ship";
                    return new FDName("Unknown Ship");
                }
            }
            else
            {
                var ret = new FDName(fdname);
                var ship = ItemData.GetShipProperties(ret);
                if (ship == null)
                {
                    System.Diagnostics.Trace.WriteLine("*** Unknown FD ship ID:" + fdname);
                    Debugger.Break();
                    shipname = "Unknown Ship " + fdname;
                }
                else
                    shipname = ship.Name;

                return ret;
            }

        }

        public static FDName NormaliseShipOrSuitOrActor(string fdname, out string name, bool allownull = false)
        {
            if (fdname != null)
            {
                FDName fd = new FDName(fdname);
                if (ItemData.IsSuitTypeName(fd))
                {
                    var suit = ItemData.GetSuit(fd);
                    name = suit?.Name ?? ("Unknown Suit " + fdname);
                    return fd;
                }
                else if (ItemData.IsActor(fd))
                {
                    name = ItemData.GetActor(fd).Name;
                    return fd;
                }
            }
            
            return NormaliseShip(fdname, out name, allownull);
        }

        public static FDName NormaliseModules(string fdname, out string modulename, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    modulename = null;
                    return null;
                }
                else
                {
                    modulename = "Unknown Module";
                    return new FDName("Unknown Module");
                }
            }
            else
            {
                if (fdname.Length >= 8 && fdname[0] == '$' && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                    fdname = fdname.Substring(1, fdname.Length - 7);        // remove decoration

                var ret = new FDName(fdname);
                if (ItemData.TryGetShipModule(ret, out ItemData.ShipModule module, true))
                {
                    modulename = module.EnglishModName;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("*** Unknown Module ID:" + fdname);
                    Debugger.Break();
                    modulename = "Unknown Module " + fdname;
                }

                return ret;
            }
        }
        public static FDName NormaliseMatCommods(string fdname, out string matname, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    matname = null;
                    return null;
                }
                else
                {
                    matname = "Unknown Material/Commodity";
                    return new FDName("Unknown Material/Commodity");
                }
            }
            else
            {
                if (fdname.Length >= 8 && fdname[0] == '$' && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                    fdname = fdname.Substring(1, fdname.Length - 7);        // remove decoration

                if (fdnamemangling.TryGetValue(fdname.ToLower(), out string value))     // fix some renaming issues
                    fdname = value;

                if (MaterialCommodityMicroResourceType.TryGet(fdname, out MaterialCommodityMicroResourceType item))
                {
                    matname = item.EnglishName;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("*** Unknown Mat/Commod ID:" + fdname);
                    Debugger.Break();
                    matname = fdname.SplitCapsWordFull();
                }

                return new FDName(fdname);
            }
        }

        public static FDName NormaliseMissionName(string fdname, out string engname)
        {
            if (fdname.HasChars())
            {
                engname = fdname.Replace("_name", "").SplitCapsWordFull();
                return new FDName(fdname);
            }
            else
            {
                engname = "Missing Mission Name";
                Debugger.Break();
                return new FDName(engname);
            }
        }

        public static FDName NormaliseBlueprint(string fdname, out string engname, bool allownull =false) // always gives a non null fdname with non null str()
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    engname = null;
                    return null;
                }
                else
                {
                    engname = "Unknown Blueprint";
                    return new FDName("Unknown Blueprint");
                }
            }
            else
            {
                var fd = new FDName(fdname);
                var rp = Recipes.FindRecipe(fd);
                if (rp != null)
                    engname = rp.Name;
                else
                {
                    System.Diagnostics.Trace.WriteLine("*** Unknown Blueprint Name:" + fdname);
                    Debugger.Break();
                    engname = fdname.SplitCapsWordFull();
                }

                return fd;
            }
        }

        public static FDName NormaliseExperimentalEffect(string fdname, out string engname, bool allownull = false) // always gives a non null fdname with non null str()
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    engname = null;
                    return null;
                }
                else
                {
                    engname = "Unknown Experimental Effect";
                    return new FDName("Unknown Experimental Effect");
                }
            }
            else
            {
                var fd = new FDName(fdname);
                var rp = Recipes.FindRecipe(fd);
                if (rp != null)
                    engname = rp.Name;
                else
                {
                    System.Diagnostics.Trace.WriteLine("*** Unknown Experimental Effect Name:" + fdname);
                    Debugger.Break();
                    engname = fdname.SplitCapsWordFull();
                }

                return fd;
            }
        }


        public static FDName NormaliseSignals(string fdname)
        {
            if (fdname.HasChars())
                return new FDName(fdname.Replace("$SAA_SignalType_", "").Replace(";", "").SplitCapsWordFull());
            else
            {
                Debugger.Break();
                return new FDName("Error in Signal no data");
            }
        }
        public static FDName NormaliseGenus(string fdname)
        {
            if (fdname.HasChars())
                return new FDName(fdname.Replace("$Codex_Ent_", "").Replace("_Name;", "").Replace(";", "").Replace("$Codex_", "").SplitCapsWordFull());
            else
            {
                Debugger.Break();
                return new FDName("Error in Genus no data");
            }
        }

        public static Dictionary<string, string> fdnamemangling = new Dictionary<string, string>() // Key: old_identifier, Value: new_identifier
        {
            //2.2 to 2.3 changed some of the identifier names.. change the 2.2 ones to 2.3!  Anthor data from his materials db file

            // July 2018 - removed many, changed above, to match FD 3.1 excel output - we use their IDs.  Netlogentry frontierdata checks these..

            { "aberrantshieldpatternanalysis"       ,  "shieldpatternanalysis" },
            { "adaptiveencryptorscapture"           ,  "adaptiveencryptors" },
            { "alyabodysoap"                        ,  "alyabodilysoap" },
            { "anomalousbulkscandata"               ,  "bulkscandata" },
            { "anomalousfsdtelemetry"               ,  "fsdtelemetry" },
            { "atypicaldisruptedwakeechoes"         ,  "disruptedwakeechoes" },
            { "atypicalencryptionarchives"          ,  "encryptionarchives" },
            { "azuremilk"                           ,  "bluemilk" },
            { "cd-75kittenbrandcoffee"              ,  "cd75catcoffee" },
            { "crackedindustrialfirmware"           ,  "industrialfirmware" },
            { "dataminedwakeexceptions"             ,  "dataminedwake" },
            { "distortedshieldcyclerecordings"      ,  "shieldcyclerecordings" },
            { "eccentrichyperspacetrajectories"     ,  "hyperspacetrajectories" },
            { "edenapplesofaerial"                  ,  "aerialedenapple" },
            { "eraninpearlwhiskey"                  ,  "eraninpearlwhisky" },
            { "exceptionalscrambledemissiondata"    ,  "scrambledemissiondata" },
            { "inconsistentshieldsoakanalysis"      ,  "shieldsoakanalysis" },
            { "kachiriginfilterleeches"             ,  "kachiriginleaches" },
            { "korokungpellets"                     ,  "korrokungpellets" },
            { "leatheryeggs"                        ,  "alieneggs" },
            { "lucanonionhead"                      ,  "transgeniconionhead" },
            { "modifiedconsumerfirmware"            ,  "consumerfirmware" },
            { "modifiedembeddedfirmware"            ,  "embeddedfirmware" },
            { "opensymmetrickeys"                   ,  "symmetrickeys" },
            { "peculiarshieldfrequencydata"         ,  "shieldfrequencydata" },
            { "rajukrumulti-stoves"                 ,  "rajukrustoves" },
            { "sanumadecorativemeat"                ,  "sanumameat" },
            { "securityfirmwarepatch"               ,  "securityfirmware" },
            { "specialisedlegacyfirmware"           ,  "legacyfirmware" },
            { "strangewakesolutions"                ,  "wakesolutions" },
            { "taggedencryptioncodes"               ,  "encryptioncodes" },
            { "unidentifiedscanarchives"            ,  "scanarchives" },
            { "unusualencryptedfiles"               ,  "encryptedfiles" },
            { "utgaroarmillennialeggs"              ,  "utgaroarmillenialeggs" },
            { "xihebiomorphiccompanions"            ,  "xihecompanions" },
            { "zeesszeantgrubglue"                  ,  "zeesszeantglue" },

            {"micro-weavecoolinghoses","coolinghoses"},
            {"energygridassembly","powergridassembly"},

            {"methanolmonohydrate","methanolmonohydratecrystals"},
            {"muonimager","mutomimager"},
            {"hardwarediagnosticsensor","diagnosticsensor"},

        };
    }

}
