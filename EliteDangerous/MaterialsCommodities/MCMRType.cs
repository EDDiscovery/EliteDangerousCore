/*
 * Copyright © 2016-2021 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("Mat {Category} {Type} {MaterialGroup} {Name} {FDName} {Shortname}")]
    public class MaterialCommodityMicroResourceType
    {
        public enum CatType
        {
            Commodity, Raw, Encoded, Manufactured,          // all
            Item,                                           // odyssey 4.0.  Called goods in game
            Component,                                      // odyssey 4.0.  Called assets in game
            Data,                                           // odyssey 4.0.  
            Consumable,                                     // odyssey 4.0. 
        };
        public CatType Category { get; private set; }               // see above

        public string TranslatedCategory { get; private set; }      // translation of above..

        public string Name { get; private set; }                    // name of it in nice text
        public string FDName { get; private set; }                  // fdname, lower case..

        public enum ItemType
        {
            VeryCommon, Common, Standard, Rare, VeryRare,           // materials
            Unknown,                                                // used for microresources
            ConsumerItems, Chemicals, Drones, Foods, IndustrialMaterials, LegalDrugs, Machinery, Medicines, Metals, Minerals, Narcotics, PowerPlay,     // commodities..
            Salvage, Slaves, Technology, Textiles, Waste, Weapons,
        };

        public ItemType Type { get; private set; }                  // and its type, for materials its commonality, for commodities its group ("Metals" etc), for microresources Unknown
        public string TranslatedType { get; private set; }          // translation of above..        

        public enum MaterialGroupType                               // Material trader group type
        {
            NA,
            RawCategory1, RawCategory2, RawCategory3, RawCategory4, RawCategory5, RawCategory6, RawCategory7,
            EncodedEmissionData, EncodedWakeScans, EncodedShieldData, EncodedEncryptionFiles, EncodedDataArchives, EncodedFirmware,
            ManufacturedChemical, ManufacturedThermic, ManufacturedHeat, ManufacturedConductive, ManufacturedMechanicalComponents,
            ManufacturedCapacitors, ManufacturedShielding, ManufacturedComposite, ManufacturedCrystals, ManufacturedAlloys,
        };

        public MaterialGroupType MaterialGroup { get; private set; } // only for materials, grouping
        public string TranslatedMaterialGroup { get; private set; }

        public string Shortname { get; private set; }               // short abv. name
        public Color Colour { get; private set; }                   // colour if its associated with one
        public bool Rarity { get; private set; }                    // if it is a rare commodity

        public bool IsCommodity { get { return Category == CatType.Commodity; } }
        public bool IsMaterial { get { return Category == CatType.Encoded || Category == CatType.Manufactured || Category == CatType.Raw; } }
        public bool IsRaw { get { return Category == CatType.Raw; } }
        public bool IsEncoded { get { return Category == CatType.Encoded; } }
        public bool IsManufactured { get { return Category == CatType.Manufactured; } }
        public bool IsEncodedOrManufactured { get { return Category == CatType.Encoded || Category == CatType.Manufactured; } }
        public bool IsRareCommodity { get { return Rarity && IsCommodity; } }
        public bool IsCommonMaterial { get { return Type == ItemType.Common || Type == ItemType.VeryCommon; } }

        public bool IsMicroResources { get { return Category >= CatType.Item; } }     // odyssey 4.0
        public bool IsConsumable { get { return Category == CatType.Consumable; } }     // odyssey 4.0
        public bool IsData { get { return Category == CatType.Data; } }
        public bool IsItem { get { return Category == CatType.Item; } }
        public bool IsComponent { get { return Category == CatType.Component; } }

        public bool IsJumponium
        {
            get
            {
                return (FDName.Contains("arsenic") || FDName.Contains("cadmium") || FDName.Contains("carbon")
                    || FDName.Contains("germanium") || FDName.Contains("niobium") || FDName.Contains("polonium")
                    || FDName.Contains("vanadium") || FDName.Contains("yttrium"));
            }
        }

        // expects lower case
        static public bool IsJumponiumType(string fdname)
        {
            return (fdname.Contains("arsenic") || fdname.Contains("cadmium") || fdname.Contains("carbon")
                || fdname.Contains("germanium") || fdname.Contains("niobium") || fdname.Contains("polonium")
                || fdname.Contains("vanadium") || fdname.Contains("yttrium"));
        }

        static public CatType? CategoryFrom(string s)
        {
            if (Enum.TryParse<CatType>(s, true, out CatType res))
                return res;
            else
                return null;
        }

        public const int VeryCommonCap = 300;
        public const int CommonCap = 250;
        public const int StandardCap = 200;
        public const int RareCap = 150;
        public const int VeryRareCap = 100;

        public int? MaterialLimit()
        {
            if (Type == ItemType.VeryCommon) return VeryCommonCap;
            if (Type == ItemType.Common) return CommonCap;
            if (Type == ItemType.Standard) return StandardCap;
            if (Type == ItemType.Rare) return RareCap;
            if (Type == ItemType.VeryRare) return VeryRareCap;
            return null;
        }

        #region Static interface

        // name key is lower case normalised
        private static Dictionary<string, MaterialCommodityMicroResourceType> cachelist = null;

        public static MaterialCommodityMicroResourceType GetByFDName(string fdname)
        {
            fdname = fdname.ToLowerInvariant();
            return cachelist.ContainsKey(fdname) ? cachelist[fdname] : null;
        }

        public static string GetNameByFDName(string fdname) // if we have it, give name, else give alt or splitcaps.  
        {
            fdname = fdname.ToLowerInvariant();
            return cachelist.ContainsKey(fdname) ? cachelist[fdname].Name : fdname.SplitCapsWordFull();
        }

        public static MaterialCommodityMicroResourceType GetByShortName(string shortname)
        {
            List<MaterialCommodityMicroResourceType> lst = cachelist.Values.ToList();
            int i = lst.FindIndex(x => x.Shortname.Equals(shortname));
            return i >= 0 ? lst[i] : null;
        }

        public static MaterialCommodityMicroResourceType GetByName(string longname)
        {
            List<MaterialCommodityMicroResourceType> lst = cachelist.Values.ToList();
            int i = lst.FindIndex(x => x.Name.Equals(longname));
            return i >= 0 ? lst[i] : null;
        }


        public static MaterialCommodityMicroResourceType[] GetAll()
        {
            return cachelist.Values.ToArray();
        }

        // use this delegate to find them
        public static MaterialCommodityMicroResourceType[] Get(Func<MaterialCommodityMicroResourceType, bool> func, bool sorted)
        {
            MaterialCommodityMicroResourceType[] items = cachelist.Values.Where(func).ToArray();

            if (sorted)
            {
                Array.Sort(items, delegate (MaterialCommodityMicroResourceType left, MaterialCommodityMicroResourceType right)     // in order, name
                {
                    return left.Name.CompareTo(right.Name.ToString());
                });

            }

            return items;
        }

        public static MaterialCommodityMicroResourceType[] GetCommodities(bool sorted)
        {
            return Get(x => x.IsCommodity, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetMaterials(bool sorted)
        {
            return Get(x => x.IsMaterial, sorted);
        }

        public static MaterialCommodityMicroResourceType[] GetMicroResources(bool sorted)
        {
            return Get(x => x.IsMicroResources, sorted);
        }

        public static Tuple<ItemType, string>[] GetTypes(Func<MaterialCommodityMicroResourceType, bool> func, bool sorted)        // given predate, return type/translated types combos.
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var types = mcs.Where(func).Select(x => new Tuple<ItemType, string>(x.Type, x.TranslatedType)).Distinct().ToArray();
            if (sorted)
                Array.Sort(types, delegate (Tuple<ItemType, string> l, Tuple<ItemType, string> r) { return l.Item2.CompareTo(r.Item2); });
            return types;
        }

        public static Tuple<CatType, string>[] GetCategories(Func<MaterialCommodityMicroResourceType, bool> func, bool sorted)   // given predate, return cat/translated cat combos.
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var types = mcs.Where(func).Select(x => new Tuple<CatType, string>(x.Category, x.TranslatedCategory)).Distinct().ToArray();
            if (sorted)
                Array.Sort(types, delegate (Tuple<CatType, string> l, Tuple<CatType, string> r) { return l.Item2.CompareTo(r.Item2); });
            return types;
        }

        public static MaterialCommodityMicroResourceType[] Get(Func<MaterialCommodityMicroResourceType, bool> func)   // given predate, return matching items
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var group = mcs.Where(func).Select(x => x).ToArray();
            return group;
        }

        public static string[] GetMembersOfType(ItemType typename, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            var members = mcs.Where(x => x.Type == typename).Select(x => x.Name).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }

        public static string[] GetFDNameMembersOfType(ItemType typename, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            string[] members = mcs.Where(x => x.Type == typename).Select(x => x.FDName).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }


        public static string[] GetFDNameMembersOfCategory(CatType catname, bool sorted)
        {
            MaterialCommodityMicroResourceType[] mcs = GetAll();
            string[] members = mcs.Where(x => x.Category == catname).Select(x => x.FDName).ToArray();
            if (sorted)
                Array.Sort(members);
            return members;
        }

        #endregion

        public MaterialCommodityMicroResourceType()
        {
        }

        public MaterialCommodityMicroResourceType(CatType cs, string n, string fd, ItemType t, MaterialGroupType mtg, string shortn, Color cl, bool rare)
        {
            Category = cs;
            TranslatedCategory = (Category ==CatType.Item) ? "Goods" : (Category==CatType.Component) ? "Assets" : Category.ToString();      // name is as the game does
            TranslatedCategory = TranslatedCategory.Tx(typeof(MaterialCommodityMicroResourceType));        // valid to pass this thru the Tx( system
            Name = n;
            FDName = fd;
            Type = t;
            string tn = Type.ToString().SplitCapsWord();
            TranslatedType = tn.Tx(typeof(MaterialCommodityMicroResourceType));                // valid to pass this thru the Tx( system
            MaterialGroup = mtg;
            TranslatedMaterialGroup = MaterialGroup.ToString().SplitCapsWordFull().Tx(typeof(MaterialCommodityMicroResourceType));                // valid to pass this thru the Tx( system
            Shortname = shortn;
            Colour = cl;
            Rarity = rare;
        }

        private void SetCache()
        {
            cachelist[this.FDName.ToLowerInvariant()] = this;
        }

        public static MaterialCommodityMicroResourceType EnsurePresent(string catname, string fdname, string locname = "")  // By FDNAME
        {
            var cat = CategoryFrom(catname);
            if (cat.HasValue)
                return EnsurePresent(cat.Value, fdname, locname);
            else
                return null;
        }


        public static MaterialCommodityMicroResourceType EnsurePresent(CatType cat, string fdname, string locname = "Unavailable")  // By FDNAME
        {
            if (!cachelist.ContainsKey(fdname.ToLowerInvariant()))
            {
                MaterialCommodityMicroResourceType mcdb = new MaterialCommodityMicroResourceType(cat, fdname.SplitCapsWordFull(), fdname, ItemType.Unknown, MaterialGroupType.NA, "", Color.Green, false);
                mcdb.SetCache();

                string shortnameguess = "MR" + cat.ToString()[0];
                foreach (var c in locname)
                {
                    if (char.IsUpper(c))
                        shortnameguess += c;
                }

                System.Diagnostics.Debug.WriteLine("** Made MCMRType: {0},{1},{2},{3}", "?", fdname, cat.ToString(), locname);
                System.Diagnostics.Debug.WriteLine("** AddMicroResource(CatType.{0},\"{1}\",\"{2}\",\"{3}\");", cat.ToString(), locname, fdname, shortnameguess);
            }

            return cachelist[fdname.ToLowerInvariant()];
        }


        #region Initial setup

        static Color CByType(ItemType s)
        {
            if (s == ItemType.VeryRare)
                return Color.Red;
            if (s == ItemType.Rare)
                return Color.Yellow;
            if (s == ItemType.VeryCommon)
                return Color.Cyan;
            if (s == ItemType.Common)
                return Color.Green;
            if (s == ItemType.Standard)
                return Color.SandyBrown;
            if (s == ItemType.Unknown)
                return Color.Red;
            System.Diagnostics.Debug.Assert(false);
            return Color.Black;
        }

        // Mats

        private static bool AddRaw(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Raw, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        private static bool AddEnc(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Encoded, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        private static bool AddManu(string name, ItemType typeofit, MaterialGroupType mt, string shortname, string fdname = "")
        {
            return AddEntry(CatType.Manufactured, CByType(typeofit), name, typeofit, mt, shortname, fdname);
        }

        // Commods

        private static bool AddCommodityRare(string aliasname, ItemType typeofit, string fdname)
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, "", fdname, true);
        }

        private static bool AddCommodity(string aliasname, ItemType typeofit, string fdname)        // fdname only if not a list.
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, "", fdname);
        }

        private static bool AddCommoditySN(string aliasname, ItemType typeofit, string shortname, string fdname)
        {
            return AddEntry(CatType.Commodity, Color.Green, aliasname, typeofit, MaterialGroupType.NA, shortname, fdname);
        }

        // fdname only useful if aliasname is not a list.
        private static bool AddCommodityList(string aliasnamelist, ItemType typeofit)
        {
            string[] list = aliasnamelist.Split(';');

            foreach (string name in list)
            {
                if (name.Length > 0)   // just in case a semicolon slips thru
                {
                    if (!AddEntry(CatType.Commodity, Color.Green, name, typeofit, MaterialGroupType.NA, "", ""))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Odyssey microresources

        private static bool AddMicroResource(CatType cat, string locname, string fdname, string shortname)
        {
            return AddEntry(cat, Color.Green, locname, ItemType.Unknown, MaterialGroupType.NA, shortname, fdname);
        }

        // common adds

        public static string FDNameCnv(string normal)
        {
            string n = new string(normal.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
            return n.ToLowerInvariant();
        }

        private static bool AddEntry(CatType catname, Color colour, string aliasname, ItemType typeofit, MaterialGroupType mtg, string shortname, string fdName, bool comrare = false)
        {
            if (shortname.HasChars() && cachelist.Values.ToList().Find(x => x.Shortname.Equals(shortname, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                System.Diagnostics.Debug.WriteLine("**** Shortname repeat for " + fdName);
            }
            System.Diagnostics.Debug.Assert(cachelist.ContainsKey(fdName) == false, "Repeated entry " + fdName);

            string fdn = (fdName.Length > 0) ? fdName.ToLowerInvariant() : FDNameCnv(aliasname);       // always lower case fdname

            MaterialCommodityMicroResourceType mc = new MaterialCommodityMicroResourceType(catname, aliasname, fdn, typeofit, mtg, shortname, colour, comrare);
            mc.SetCache();
            return true;
        }

        public static void FillTable()
        {
            #region Materials  

            cachelist = new Dictionary<string, MaterialCommodityMicroResourceType>();

            // NOTE KEEP IN ORDER BY Rarity and then Material Group Type

            // very common raw

            AddRaw("Carbon", ItemType.VeryCommon, MaterialGroupType.RawCategory1, "C");
            AddRaw("Phosphorus", ItemType.VeryCommon, MaterialGroupType.RawCategory2, "P");
            AddRaw("Sulphur", ItemType.VeryCommon, MaterialGroupType.RawCategory3, "S");
            AddRaw("Iron", ItemType.VeryCommon, MaterialGroupType.RawCategory4, "Fe");
            AddRaw("Nickel", ItemType.VeryCommon, MaterialGroupType.RawCategory5, "Ni");
            AddRaw("Rhenium", ItemType.VeryCommon, MaterialGroupType.RawCategory6, "Re");
            AddRaw("Lead", ItemType.VeryCommon, MaterialGroupType.RawCategory7, "Pb");

            // common raw

            AddRaw("Vanadium", ItemType.Common, MaterialGroupType.RawCategory1, "V");
            AddRaw("Chromium", ItemType.Common, MaterialGroupType.RawCategory2, "Cr");
            AddRaw("Manganese", ItemType.Common, MaterialGroupType.RawCategory3, "Mn");
            AddRaw("Zinc", ItemType.Common, MaterialGroupType.RawCategory4, "Zn");
            AddRaw("Germanium", ItemType.Common, MaterialGroupType.RawCategory5, "Ge");
            AddRaw("Arsenic", ItemType.Common, MaterialGroupType.RawCategory6, "As");
            AddRaw("Zirconium", ItemType.Common, MaterialGroupType.RawCategory7, "Zr");

            // standard raw

            AddRaw("Niobium", ItemType.Standard, MaterialGroupType.RawCategory1, "Nb");        // realign to Anthors standard
            AddRaw("Molybdenum", ItemType.Standard, MaterialGroupType.RawCategory2, "Mo");
            AddRaw("Cadmium", ItemType.Standard, MaterialGroupType.RawCategory3, "Cd");
            AddRaw("Tin", ItemType.Standard, MaterialGroupType.RawCategory4, "Sn");
            AddRaw("Tungsten", ItemType.Standard, MaterialGroupType.RawCategory5, "W");
            AddRaw("Mercury", ItemType.Standard, MaterialGroupType.RawCategory6, "Hg");
            AddRaw("Boron", ItemType.Standard, MaterialGroupType.RawCategory7, "B");

            // rare raw

            AddRaw("Yttrium", ItemType.Rare, MaterialGroupType.RawCategory1, "Y");
            AddRaw("Technetium", ItemType.Rare, MaterialGroupType.RawCategory2, "Tc");
            AddRaw("Ruthenium", ItemType.Rare, MaterialGroupType.RawCategory3, "Ru");
            AddRaw("Selenium", ItemType.Rare, MaterialGroupType.RawCategory4, "Se");
            AddRaw("Tellurium", ItemType.Rare, MaterialGroupType.RawCategory5, "Te");
            AddRaw("Polonium", ItemType.Rare, MaterialGroupType.RawCategory6, "Po");
            AddRaw("Antimony", ItemType.Rare, MaterialGroupType.RawCategory7, "Sb");

            // very common data
            AddEnc("Exceptional Scrambled Emission Data", ItemType.VeryCommon, MaterialGroupType.EncodedEmissionData, "ESED", "scrambledemissiondata");
            AddEnc("Atypical Disrupted Wake Echoes", ItemType.VeryCommon, MaterialGroupType.EncodedWakeScans, "ADWE", "disruptedwakeechoes");
            AddEnc("Distorted Shield Cycle Recordings", ItemType.VeryCommon, MaterialGroupType.EncodedShieldData, "DSCR", "shieldcyclerecordings");
            AddEnc("Unusual Encrypted Files", ItemType.VeryCommon, MaterialGroupType.EncodedEncryptionFiles, "UEF", "encryptedfiles");
            AddEnc("Anomalous Bulk Scan Data", ItemType.VeryCommon, MaterialGroupType.EncodedDataArchives, "ABSD", "bulkscandata");
            AddEnc("Specialised Legacy Firmware", ItemType.VeryCommon, MaterialGroupType.EncodedFirmware, "SLF", "legacyfirmware");

            // common data
            AddEnc("Irregular Emission Data", ItemType.Common, MaterialGroupType.EncodedEmissionData, "IED", "archivedemissiondata");
            AddEnc("Anomalous FSD Telemetry", ItemType.Common, MaterialGroupType.EncodedWakeScans, "AFT", "fsdtelemetry");
            AddEnc("Inconsistent Shield Soak Analysis", ItemType.Common, MaterialGroupType.EncodedShieldData, "ISSA", "shieldsoakanalysis");
            AddEnc("Tagged Encryption Codes", ItemType.Common, MaterialGroupType.EncodedEncryptionFiles, "TEC", "encryptioncodes");
            AddEnc("Unidentified Scan Archives", ItemType.Common, MaterialGroupType.EncodedDataArchives, "USA", "scanarchives");
            AddEnc("Modified Consumer Firmware", ItemType.Common, MaterialGroupType.EncodedFirmware, "MCF", "consumerfirmware");

            // standard data

            AddEnc("Unexpected Emission Data", ItemType.Standard, MaterialGroupType.EncodedEmissionData, "UED", "emissiondata");
            AddEnc("Strange Wake Solutions", ItemType.Standard, MaterialGroupType.EncodedWakeScans, "SWS", "wakesolutions");
            AddEnc("Untypical Shield Scans", ItemType.Standard, MaterialGroupType.EncodedShieldData, "USS", "shielddensityreports");
            AddEnc("Open Symmetric Keys", ItemType.Standard, MaterialGroupType.EncodedEncryptionFiles, "OSK", "symmetrickeys");
            AddEnc("Classified Scan Databanks", ItemType.Standard, MaterialGroupType.EncodedDataArchives, "CSD", "scandatabanks");
            AddEnc("Cracked Industrial Firmware", ItemType.Standard, MaterialGroupType.EncodedFirmware, "CIF", "industrialfirmware");

            // rare data
            AddEnc("Decoded Emission Data", ItemType.Rare, MaterialGroupType.EncodedEmissionData, "DED");
            AddEnc("Eccentric Hyperspace Trajectories", ItemType.Rare, MaterialGroupType.EncodedWakeScans, "EHT", "hyperspacetrajectories");
            AddEnc("Aberrant Shield Pattern Analysis", ItemType.Rare, MaterialGroupType.EncodedShieldData, "ASPA", "shieldpatternanalysis");
            AddEnc("Atypical Encryption Archives", ItemType.Rare, MaterialGroupType.EncodedEncryptionFiles, "AEA", "encryptionarchives");
            AddEnc("Divergent Scan Data", ItemType.Rare, MaterialGroupType.EncodedDataArchives, "DSD", "encodedscandata");
            AddEnc("Security Firmware Patch", ItemType.Rare, MaterialGroupType.EncodedFirmware, "SFP", "securityfirmware");

            // very rare data

            AddEnc("Abnormal Compact Emissions Data", ItemType.VeryRare, MaterialGroupType.EncodedEmissionData, "CED", "compactemissionsdata");
            AddEnc("Datamined Wake Exceptions", ItemType.VeryRare, MaterialGroupType.EncodedWakeScans, "DWEx", "dataminedwake");
            AddEnc("Peculiar Shield Frequency Data", ItemType.VeryRare, MaterialGroupType.EncodedShieldData, "PSFD", "shieldfrequencydata");
            AddEnc("Adaptive Encryptors Capture", ItemType.VeryRare, MaterialGroupType.EncodedEncryptionFiles, "AEC", "adaptiveencryptors");
            AddEnc("Classified Scan Fragment", ItemType.VeryRare, MaterialGroupType.EncodedDataArchives, "CFSD", "classifiedscandata");
            AddEnc("Modified Embedded Firmware", ItemType.VeryRare, MaterialGroupType.EncodedFirmware, "EFW", "embeddedfirmware");

            // very common manu

            AddManu("Chemical Storage Units", ItemType.VeryCommon, MaterialGroupType.ManufacturedChemical, "CSU");
            AddManu("Tempered Alloys", ItemType.VeryCommon, MaterialGroupType.ManufacturedThermic, "TeA");
            AddManu("Heat Conduction Wiring", ItemType.VeryCommon, MaterialGroupType.ManufacturedHeat, "HCW");
            AddManu("Basic Conductors", ItemType.VeryCommon, MaterialGroupType.ManufacturedConductive, "BaC");
            AddManu("Mechanical Scrap", ItemType.VeryCommon, MaterialGroupType.ManufacturedMechanicalComponents, "MS");
            AddManu("Grid Resistors", ItemType.VeryCommon, MaterialGroupType.ManufacturedCapacitors, "GR");
            AddManu("Worn Shield Emitters", ItemType.VeryCommon, MaterialGroupType.ManufacturedShielding, "WSE");
            AddManu("Compact Composites", ItemType.VeryCommon, MaterialGroupType.ManufacturedComposite, "CC");
            AddManu("Crystal Shards", ItemType.VeryCommon, MaterialGroupType.ManufacturedCrystals, "CS");
            AddManu("Salvaged Alloys", ItemType.VeryCommon, MaterialGroupType.ManufacturedAlloys, "SAll");

            // common manu

            AddManu("Chemical Processors", ItemType.Common, MaterialGroupType.ManufacturedChemical, "CP");
            AddManu("Heat Resistant Ceramics", ItemType.Common, MaterialGroupType.ManufacturedThermic, "HRC");
            AddManu("Heat Dispersion Plate", ItemType.Common, MaterialGroupType.ManufacturedHeat, "HDP");
            AddManu("Conductive Components", ItemType.Common, MaterialGroupType.ManufacturedConductive, "CCo");
            AddManu("Mechanical Equipment", ItemType.Common, MaterialGroupType.ManufacturedMechanicalComponents, "ME");
            AddManu("Hybrid Capacitors", ItemType.Common, MaterialGroupType.ManufacturedCapacitors, "HC");
            AddManu("Shield Emitters", ItemType.Common, MaterialGroupType.ManufacturedShielding, "SHE");
            AddManu("Filament Composites", ItemType.Common, MaterialGroupType.ManufacturedComposite, "FiC");
            AddManu("Flawed Focus Crystals", ItemType.Common, MaterialGroupType.ManufacturedCrystals, "FFC", "uncutfocuscrystals");
            AddManu("Galvanising Alloys", ItemType.Common, MaterialGroupType.ManufacturedAlloys, "GA");

            // Standard manu

            AddManu("Chemical Distillery", ItemType.Standard, MaterialGroupType.ManufacturedChemical, "CHD");
            AddManu("Precipitated Alloys", ItemType.Standard, MaterialGroupType.ManufacturedThermic, "PAll");
            AddManu("Heat Exchangers", ItemType.Standard, MaterialGroupType.ManufacturedHeat, "HE");
            AddManu("Conductive Ceramics", ItemType.Standard, MaterialGroupType.ManufacturedConductive, "CCe");
            AddManu("Mechanical Components", ItemType.Standard, MaterialGroupType.ManufacturedMechanicalComponents, "MC");
            AddManu("Electrochemical Arrays", ItemType.Standard, MaterialGroupType.ManufacturedCapacitors, "EA");
            AddManu("Shielding Sensors", ItemType.Standard, MaterialGroupType.ManufacturedShielding, "SS");
            AddManu("High Density Composites", ItemType.Standard, MaterialGroupType.ManufacturedComposite, "HDC");
            AddManu("Focus Crystals", ItemType.Standard, MaterialGroupType.ManufacturedCrystals, "FoC");
            AddManu("Phase Alloys", ItemType.Standard, MaterialGroupType.ManufacturedAlloys, "PA");

            // rare manu 

            AddManu("Chemical Manipulators", ItemType.Rare, MaterialGroupType.ManufacturedChemical, "CM");
            AddManu("Thermic Alloys", ItemType.Rare, MaterialGroupType.ManufacturedThermic, "ThA");
            AddManu("Heat Vanes", ItemType.Rare, MaterialGroupType.ManufacturedHeat, "HV");
            AddManu("Conductive Polymers", ItemType.Rare, MaterialGroupType.ManufacturedConductive, "CPo");
            AddManu("Configurable Components", ItemType.Rare, MaterialGroupType.ManufacturedMechanicalComponents, "CCom");
            AddManu("Polymer Capacitors", ItemType.Rare, MaterialGroupType.ManufacturedCapacitors, "PCa");
            AddManu("Compound Shielding", ItemType.Rare, MaterialGroupType.ManufacturedShielding, "CoS");
            AddManu("Proprietary Composites", ItemType.Rare, MaterialGroupType.ManufacturedComposite, "FPC", "fedproprietarycomposites");
            AddManu("Refined Focus Crystals", ItemType.Rare, MaterialGroupType.ManufacturedCrystals, "RFC");
            AddManu("Proto Light Alloys", ItemType.Rare, MaterialGroupType.ManufacturedAlloys, "PLA");

            // very rare manu

            AddManu("Pharmaceutical Isolators", ItemType.VeryRare, MaterialGroupType.ManufacturedChemical, "PI");
            AddManu("Military Grade Alloys", ItemType.VeryRare, MaterialGroupType.ManufacturedThermic, "MGA");
            AddManu("Proto Heat Radiators", ItemType.VeryRare, MaterialGroupType.ManufacturedHeat, "PHR");
            AddManu("Biotech Conductors", ItemType.VeryRare, MaterialGroupType.ManufacturedConductive, "BiC");
            AddManu("Improvised Components", ItemType.VeryRare, MaterialGroupType.ManufacturedMechanicalComponents, "IC");
            AddManu("Military Supercapacitors", ItemType.VeryRare, MaterialGroupType.ManufacturedCapacitors, "MSC");
            AddManu("Imperial Shielding", ItemType.VeryRare, MaterialGroupType.ManufacturedShielding, "IS");
            AddManu("Core Dynamics Composites", ItemType.VeryRare, MaterialGroupType.ManufacturedComposite, "FCC", "fedcorecomposites");
            AddManu("Exquisite Focus Crystals", ItemType.VeryRare, MaterialGroupType.ManufacturedCrystals, "EFC");
            AddManu("Proto Radiolic Alloys", ItemType.VeryRare, MaterialGroupType.ManufacturedAlloys, "PRA");

            // Obelisk

            AddEnc("Pattern Alpha Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PAOD", "ancientbiologicaldata");
            AddEnc("Pattern Beta Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PBOD", "ancientculturaldata");
            AddEnc("Pattern Gamma Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PGOD", "ancienthistoricaldata");
            AddEnc("Pattern Delta Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PDOD", "ancientlanguagedata");
            AddEnc("Pattern Epsilon Obelisk Data", ItemType.Rare, MaterialGroupType.NA, "PEOD", "ancienttechnologicaldata");

            // new to 3.1 frontier data

            AddManu("Guardian Power Cell", ItemType.VeryCommon, MaterialGroupType.NA, "GPCe", "guardian_powercell");
            AddManu("Guardian Power Conduit", ItemType.Common, MaterialGroupType.NA, "GPC", "guardian_powerconduit");
            AddManu("Guardian Technology Component", ItemType.Standard, MaterialGroupType.NA, "GTC", "guardian_techcomponent");
            AddManu("Guardian Sentinel Weapon Parts", ItemType.Standard, MaterialGroupType.NA, "GSWP", "guardian_sentinel_weaponparts");
            AddManu("Guardian Wreckage Components", ItemType.VeryCommon, MaterialGroupType.NA, "GSWC", "guardian_sentinel_wreckagecomponents");
            AddEnc("Guardian Weapon Blueprint Fragment", ItemType.Rare, MaterialGroupType.NA, "GWBS", "guardian_weaponblueprint");
            AddEnc("Guardian Module Blueprint Fragment", ItemType.Rare, MaterialGroupType.NA, "GMBS", "guardian_moduleblueprint");

            // new to 3.2 frontier data
            AddEnc("Guardian Vessel Blueprint Fragment", ItemType.VeryRare, MaterialGroupType.NA, "GMVB", "guardian_vesselblueprint");
            AddManu("Bio-Mechanical Conduits", ItemType.Standard, MaterialGroupType.NA, "BMC", "TG_BioMechanicalConduits");
            AddManu("Propulsion Elements", ItemType.VeryRare, MaterialGroupType.NA, "PE", "TG_PropulsionElement");
            AddManu("Weapon Parts", ItemType.Rare, MaterialGroupType.NA, "WP", "TG_WeaponParts");
            AddManu("Wreckage Components", ItemType.Standard, MaterialGroupType.NA, "WRC", "TG_WreckageComponents");
            AddEnc("Ship Flight Data", ItemType.Standard, MaterialGroupType.NA, "SFD", "TG_ShipFlightData");
            AddEnc("Ship Systems Data", ItemType.Rare, MaterialGroupType.NA, "SSD", "TG_ShipSystemsData");

            ItemType sv = ItemType.Salvage;
            AddCommodity("Thargoid Sensor", sv, "UnknownArtifact");
            AddCommodity("Thargoid Probe", sv, "UnknownArtifact2");
            AddCommodity("Thargoid Link", sv, "UnknownArtifact3");
            AddCommodity("Thargoid Resin", sv, "UnknownResin");
            AddCommodity("Thargoid Biological Matter", sv, "UnknownBiologicalMatter");
            AddCommodity("Thargoid Technology Samples", sv, "UnknownTechnologySamples");

            AddManu("Thargoid Carapace", ItemType.Common, MaterialGroupType.NA, "UKCP", "unknowncarapace");
            AddManu("Thargoid Energy Cell", ItemType.Standard, MaterialGroupType.NA, "UKEC", "unknownenergycell");
            AddManu("Thargoid Organic Circuitry", ItemType.VeryRare, MaterialGroupType.NA, "UKOC", "unknownorganiccircuitry");
            AddManu("Thargoid Technological Components", ItemType.Rare, MaterialGroupType.NA, "UKTC", "unknowntechnologycomponents");
            AddManu("Sensor Fragment", ItemType.VeryRare, MaterialGroupType.NA, "UES", "unknownenergysource");

            AddEnc("Thargoid Material Composition Data", ItemType.Standard, MaterialGroupType.NA, "UMCD", "tg_compositiondata");
            AddEnc("Thargoid Structural Data", ItemType.Common, MaterialGroupType.NA, "UKSD", "tg_structuraldata");
            AddEnc("Thargoid Residue Data", ItemType.Rare, MaterialGroupType.NA, "URDA", "tg_residuedata");
            AddEnc("Thargoid Ship Signature", ItemType.Standard, MaterialGroupType.NA, "USSig", "unknownshipsignature");
            AddEnc("Thargoid Wake Data", ItemType.Rare, MaterialGroupType.NA, "UWD", "unknownwakedata");

            #endregion

            #region Commodities 

            AddCommodity("Rockforth Fertiliser", ItemType.Chemicals, "RockforthFertiliser");
            AddCommodity("Agronomic Treatment", ItemType.Chemicals, "AgronomicTreatment");
            AddCommodity("Tritium", ItemType.Chemicals, "Tritium");
            AddCommodityList("Explosives;Hydrogen Fuel;Hydrogen Peroxide;Liquid Oxygen;Mineral Oil;Nerve Agents;Pesticides;Surface Stabilisers;Synthetic Reagents;Water", ItemType.Chemicals);

            ItemType ci = ItemType.ConsumerItems;
            AddCommodityList("Clothing;Consumer Technology;Domestic Appliances;Evacuation Shelter;Survival Equipment", ci);
            AddCommodity("Duradrives", ci, "Duradrives");

            ItemType fd = ItemType.Foods;
            AddCommodityList("Algae;Animal Meat;Coffee;Fish;Food Cartridges;Fruit and Vegetables;Grain;Synthetic Meat;Tea", fd);

            ItemType im = ItemType.IndustrialMaterials;
            AddCommodityList("Ceramic Composites;Insulating Membrane;Polymers;Semiconductors;Superconductors", im);
            AddCommoditySN("Meta-Alloys", im, "MA", "metaalloys");
            AddCommoditySN("Micro-Weave Cooling Hoses", im, "MWCH", "coolinghoses");
            AddCommoditySN("Neofabric Insulation", im, "NFI", "");
            AddCommoditySN("CMM Composite", im, "CMMC", "");

            ItemType ld = ItemType.LegalDrugs;
            AddCommodityList("Beer;Bootleg Liquor;Liquor;Tobacco;Wine", ld);

            ItemType m = ItemType.Machinery;
            AddCommodity("Atmospheric Processors", m, "AtmosphericExtractors");
            AddCommodity("Marine Equipment", m, "MarineSupplies");
            AddCommodity("Microbial Furnaces", m, "HeliostaticFurnaces");
            AddCommodity("Skimmer Components", m, "SkimerComponents");
            AddCommodityList("Building Fabricators;Crop Harvesters;Emergency Power Cells;Exhaust Manifold;Geological Equipment", m);
            AddCommoditySN("HN Shock Mount", m, "HNSM", "");
            AddCommodityList("Mineral Extractors;Modular Terminals;Power Generators", m);
            AddCommodityList("Thermal Cooling Units;Water Purifiers", m);
            AddCommoditySN("Heatsink Interlink", m, "HSI", "");
            AddCommoditySN("Energy Grid Assembly", m, "EGA", "powergridassembly");
            AddCommoditySN("Radiation Baffle", m, "RB", "");
            AddCommoditySN("Magnetic Emitter Coil", m, "MEC", "");
            AddCommoditySN("Articulation Motors", m, "AM", "");
            AddCommoditySN("Reinforced Mounting Plate", m, "RMP", "");
            AddCommoditySN("Power Transfer Bus", m, "PTB", "PowerTransferConduits");
            AddCommoditySN("Power Converter", m, "PC", "");
            AddCommoditySN("Ion Distributor", m, "ID", "");

            ItemType md = ItemType.Medicines;
            AddCommodityList("Advanced Medicines;Basic Medicines;Combat Stabilisers;Performance Enhancers;Progenitor Cells", md);
            AddCommodity("Agri-Medicines", md, "agriculturalmedicines");
            AddCommodity("Nanomedicines", md, "Nanomedicines"); // not in frontier data. Keep for now Jan 2020

            ItemType mt = ItemType.Metals;
            AddCommodityList("Aluminium;Beryllium;Bismuth;Cobalt;Copper;Gallium;Gold;Hafnium 178;Indium;Lanthanum;Lithium;Palladium;Platinum;Praseodymium;Samarium;Silver;Tantalum;Thallium;Thorium;Titanium;Uranium", mt);
            AddCommoditySN("Osmium", mt, "OSM", "osmium");
            AddCommodity("Platinum Alloy", mt, "PlatinumAloy");

            ItemType mi = ItemType.Minerals;
            AddCommodityList("Bauxite;Bertrandite;Bromellite;Coltan;Cryolite;Gallite;Goslarite;Methane Clathrate", mi);
            AddCommodityList("Indite;Jadeite;Lepidolite;Lithium Hydroxide;Moissanite;Painite;Pyrophyllite;Rutile;Taaffeite;Uraninite", mi);
            AddCommodity("Methanol Monohydrate Crystals", mi, "methanolmonohydratecrystals");
            AddCommodity("Low Temperature Diamonds", mi, "lowtemperaturediamond");
            AddCommodity("Void Opal", mi, "Opal");
            AddCommodity("Rhodplumsite", mi, "Rhodplumsite");
            AddCommodity("Serendibite", mi, "Serendibite");
            AddCommodity("Monazite", mi, "Monazite");
            AddCommodity("Musgravite", mi, "Musgravite");
            AddCommodity("Benitoite", mi, "Benitoite");
            AddCommodity("Grandidierite", mi, "Grandidierite");
            AddCommodity("Alexandrite", mi, "Alexandrite");

            // Salvage

            AddCommodity("Trinkets of Hidden Fortune", sv, "TrinketsOfFortune");
            AddCommodity("Gene Bank", sv, "GeneBank");
            AddCommodity("Time Capsule", sv, "TimeCapsule");
            AddCommodity("Damaged Escape Pod", sv, "DamagedEscapePod");
            AddCommodity("Thargoid Heart", sv, "ThargoidHeart");
            AddCommodity("Thargoid Cyclops Tissue Sample", sv, "ThargoidTissueSampleType1");
            AddCommodity("Thargoid Basilisk Tissue Sample", sv, "ThargoidTissueSampleType2");
            AddCommodity("Thargoid Medusa Tissue Sample", sv, "ThargoidTissueSampleType3");
            AddCommodity("Thargoid Scout Tissue Sample", sv, "ThargoidScoutTissueSample");
            AddCommodity("Wreckage Components", sv, "WreckageComponents");
            AddCommodity("Antique Jewellery", sv, "AntiqueJewellery");
            AddCommodity("Thargoid Hydra Tissue Sample", sv, "ThargoidTissueSampleType4");
            AddCommodity("Ancient Key", sv, "AncientKey");

            AddCommodityList("Ai Relics;Antimatter Containment Unit;Antiquities;Assault Plans;Data Core;Diplomatic Bag;Encrypted Correspondence;Fossil Remnants", sv);
            AddCommodityList("Geological Samples;Military Intelligence;Mysterious Idol;Occupied CryoPod;Personal Effects;Precious Gems;Prohibited Research Materials", sv);
            AddCommodityList("Sap 8 Core Container;Scientific Research;Scientific Samples;Space Pioneer Relics;Tactical Data;Unstable Data Core", sv);
            AddCommodity("Large Survey Data Cache", sv, "largeexplorationdatacash");
            AddCommodity("Small Survey Data Cache", sv, "smallexplorationdatacash");
            AddCommodity("Ancient Artefact", sv, "USSCargoAncientArtefact");
            AddCommodity("Black Box", sv, "USSCargoBlackBox");
            AddCommodity("Political Prisoners", sv, "PoliticalPrisoner");
            AddCommodity("Hostages", sv, "Hostage");
            AddCommodity("Commercial Samples", sv, "ComercialSamples");
            AddCommodity("Encrypted Data Storage", sv, "EncriptedDataStorage");
            AddCommodity("Experimental Chemicals", sv, "USSCargoExperimentalChemicals");
            AddCommodity("Military Plans", sv, "USSCargoMilitaryPlans");
            AddCommodity("Occupied Escape Pod", sv, "OccupiedCryoPod");
            AddCommodity("Prototype Tech", sv, "USSCargoPrototypeTech");
            AddCommodity("Rare Artwork", sv, "USSCargoRareArtwork");
            AddCommodity("Rebel Transmissions", sv, "USSCargoRebelTransmissions");
            AddCommodity("Technical Blueprints", sv, "USSCargoTechnicalBlueprints");
            AddCommodity("Trade Data", sv, "USSCargoTradeData");
            AddCommodity("Guardian Relic", sv, "AncientRelic");
            AddCommodity("Guardian Orb", sv, "AncientOrb");
            AddCommodity("Guardian Casket", sv, "AncientCasket");
            AddCommodity("Guardian Tablet", sv, "AncientTablet");
            AddCommodity("Guardian Urn", sv, "AncientUrn");
            AddCommodity("Guardian Totem", sv, "AncientTotem");

            AddCommodity("Mollusc Soft Tissue", sv, "M_TissueSample_Soft");
            AddCommodity("Pod Core Tissue", sv, "S_TissueSample_Cells");
            AddCommodity("Pod Dead Tissue", sv, "S_TissueSample_Surface");
            AddCommodity("Pod Surface Tissue", sv, "S_TissueSample_Core");
            AddCommodity("Mollusc Membrane", sv, "M3_TissueSample_Membrane");
            AddCommodity("Mollusc Mycelium", sv, "M3_TissueSample_Mycelium");
            AddCommodity("Mollusc Spores", sv, "M3_TissueSample_Spores");
            AddCommodity("Pod Shell Tissue", sv, "S6_TissueSample_Coenosarc");
            AddCommodity("Pod Mesoglea", sv, "S6_TissueSample_Mesoglea");
            AddCommodity("Pod Outer Tissue", sv, "S6_TissueSample_Cells");
            AddCommodity("Mollusc Fluid", ItemType.Salvage, "M_TissueSample_Fluid");
            AddCommodity("Mollusc Brain Tissue", ItemType.Salvage, "M_TissueSample_Nerves");
            AddCommodity("Pod Tissue", ItemType.Salvage, "S9_TissueSample_Shell");

            ItemType nc = ItemType.Narcotics;
            AddCommodity("Narcotics", nc, "BasicNarcotics");

            ItemType sl = ItemType.Slaves;
            AddCommodityList("Imperial Slaves;Slaves", sl);

            ItemType tc = ItemType.Technology;
            AddCommoditySN("Ion Distributor", tc, "IOD", "IonDistributor");
            AddCommodityList("Advanced Catalysers;Animal Monitors;Aquaponic Systems;Bioreducing Lichen;Computer Components", tc);
            AddCommodity("Auto-Fabricators", tc, "autofabricators");
            AddCommoditySN("Micro Controllers", tc, "MCC", "MicroControllers");
            AddCommodityList("Medical Diagnostic Equipment", tc);
            AddCommodityList("Nanobreakers;Resonating Separators;Robotics;Structural Regulators;Telemetry Suite", tc);
            AddCommodity("H.E. Suits", tc, "hazardousenvironmentsuits");
            AddCommoditySN("Hardware Diagnostic Sensor", tc, "DIS", "diagnosticsensor");
            AddCommodity("Muon Imager", tc, "mutomimager");
            AddCommodity("Land Enrichment Systems", tc, "TerrainEnrichmentSystems");

            ItemType tx = ItemType.Textiles;
            AddCommodityList("Conductive Fabrics;Leather;Military Grade Fabrics;Natural Fabrics;Synthetic Fabrics", tx);

            ItemType ws = ItemType.Waste;
            AddCommodityList("Biowaste;Chemical Waste;Scrap;Toxic Waste", ws);

            ItemType wp = ItemType.Weapons;
            AddCommodityList("Battle Weapons;Landmines;Personal Weapons;Reactive Armour", wp);
            AddCommodity("Non-Lethal Weapons", wp, "nonlethalweapons");

            #endregion

            #region Rare Commodities 

            AddCommodityRare("Apa Vietii", ItemType.Narcotics, "ApaVietii");
            AddCommodityRare("The Hutton Mug", ItemType.ConsumerItems, "TheHuttonMug");
            AddCommodityRare("Eranin Pearl Whisky", ItemType.LegalDrugs, "EraninPearlWhisky");
            AddCommodityRare("Lavian Brandy", ItemType.LegalDrugs, "LavianBrandy");
            AddCommodityRare("HIP 10175 Bush Meat", ItemType.Foods, "HIP10175BushMeat");
            AddCommodityRare("Albino Quechua Mammoth Meat", ItemType.Foods, "AlbinoQuechuaMammoth");
            AddCommodityRare("Utgaroar Millennial Eggs", ItemType.Foods, "UtgaroarMillenialEggs");
            AddCommodityRare("Witchhaul Kobe Beef", ItemType.Foods, "WitchhaulKobeBeef");
            AddCommodityRare("Karsuki Locusts", ItemType.Foods, "KarsukiLocusts");
            AddCommodityRare("Giant Irukama Snails", ItemType.Foods, "GiantIrukamaSnails");
            AddCommodityRare("Baltah'sine Vacuum Krill", ItemType.Foods, "BaltahSineVacuumKrill");
            AddCommodityRare("Ceti Rabbits", ItemType.Foods, "CetiRabbits");
            AddCommodityRare("Kachirigin Filter Leeches", ItemType.Medicines, "KachiriginLeaches");
            AddCommodityRare("Lyrae Weed", ItemType.Narcotics, "LyraeWeed");
            AddCommodityRare("Onionhead", ItemType.Narcotics, "OnionHead");
            AddCommodityRare("Tarach Spice", ItemType.Narcotics, "TarachTorSpice");
            AddCommodityRare("Wolf Fesh", ItemType.Narcotics, "Wolf1301Fesh");
            AddCommodityRare("Borasetani Pathogenetics", ItemType.Weapons, "BorasetaniPathogenetics");
            AddCommodityRare("HIP 118311 Swarm", ItemType.Weapons, "HIP118311Swarm");
            AddCommodityRare("Kongga Ale", ItemType.LegalDrugs, "KonggaAle");
            AddCommodityRare("Wuthielo Ku Froth", ItemType.LegalDrugs, "WuthieloKuFroth");
            AddCommodityRare("Alacarakmo Skin Art", ItemType.ConsumerItems, "AlacarakmoSkinArt");
            AddCommodityRare("Eleu Thermals", ItemType.ConsumerItems, "EleuThermals");
            AddCommodityRare("Eshu Umbrellas", ItemType.ConsumerItems, "EshuUmbrellas");
            AddCommodityRare("Karetii Couture", ItemType.ConsumerItems, "KaretiiCouture");
            AddCommodityRare("Njangari Saddles", ItemType.ConsumerItems, "NjangariSaddles");
            AddCommodityRare("Any Na Coffee", ItemType.Foods, "AnyNaCoffee");
            AddCommodityRare("CD-75 Kitten Brand Coffee", ItemType.Foods, "CD75CatCoffee");
            AddCommodityRare("Goman Yaupon Coffee", ItemType.Foods, "GomanYauponCoffee");
            AddCommodityRare("Volkhab Bee Drones", ItemType.Machinery, "VolkhabBeeDrones");
            AddCommodityRare("Kinago Violins", ItemType.ConsumerItems, "KinagoInstruments");
            AddCommodityRare("Nguna Modern Antiques", ItemType.ConsumerItems, "NgunaModernAntiques");
            AddCommodityRare("Rajukru Multi-Stoves", ItemType.ConsumerItems, "RajukruStoves");
            AddCommodityRare("Tiolce Waste2Paste Units", ItemType.ConsumerItems, "TiolceWaste2PasteUnits");
            AddCommodityRare("Chi Eridani Marine Paste", ItemType.Foods, "ChiEridaniMarinePaste");
            AddCommodityRare("Esuseku Caviar", ItemType.Foods, "EsusekuCaviar");
            AddCommodityRare("Live Hecate Sea Worms", ItemType.Foods, "LiveHecateSeaWorms");
            AddCommodityRare("Helvetitj Pearls", ItemType.Foods, "HelvetitjPearls");
            AddCommodityRare("HIP Proto-Squid", ItemType.Foods, "HIP41181Squid");
            AddCommodityRare("Coquim Spongiform Victuals", ItemType.Foods, "CoquimSpongiformVictuals");
            AddCommodityRare("Eden Apples Of Aerial", ItemType.Foods, "AerialEdenApple");
            AddCommodityRare("Neritus Berries", ItemType.Foods, "NeritusBerries");
            AddCommodityRare("Ochoeng Chillies", ItemType.Foods, "OchoengChillies");
            AddCommodityRare("Deuringas Truffles", ItemType.Foods, "DeuringasTruffles");
            AddCommodityRare("HR 7221 Wheat", ItemType.Foods, "HR7221Wheat");
            AddCommodityRare("Jaroua Rice", ItemType.Foods, "JarouaRice");
            AddCommodityRare("Belalans Ray Leather", ItemType.Textiles, "BelalansRayLeather");
            AddCommodityRare("Damna Carapaces", ItemType.Textiles, "DamnaCarapaces");
            AddCommodityRare("Rapa Bao Snake Skins", ItemType.Textiles, "RapaBaoSnakeSkins");
            AddCommodityRare("Vanayequi Ceratomorpha Fur", ItemType.Textiles, "VanayequiRhinoFur");
            AddCommodityRare("Bast Snake Gin", ItemType.LegalDrugs, "BastSnakeGin");
            AddCommodityRare("Thrutis Cream", ItemType.LegalDrugs, "ThrutisCream");
            AddCommodityRare("Wulpa Hyperbore Systems", ItemType.Machinery, "WulpaHyperboreSystems");
            AddCommodityRare("Aganippe Rush", ItemType.Medicines, "AganippeRush");
            AddCommodityRare("Terra Mater Blood Bores", ItemType.Medicines, "TerraMaterBloodBores");
            AddCommodityRare("Holva Duelling Blades", ItemType.Weapons, "HolvaDuellingBlades");
            AddCommodityRare("Kamorin Historic Weapons", ItemType.Weapons, "KamorinHistoricWeapons");
            AddCommodityRare("Gilya Signature Weapons", ItemType.Weapons, "GilyaSignatureWeapons");
            AddCommodityRare("Delta Phoenicis Palms", ItemType.Chemicals, "DeltaPhoenicisPalms");
            AddCommodityRare("Toxandji Virocide", ItemType.Chemicals, "ToxandjiVirocide");
            AddCommodityRare("Xihe Biomorphic Companions", ItemType.Technology, "XiheCompanions");
            AddCommodityRare("Sanuma Decorative Meat", ItemType.Foods, "SanumaMEAT");
            AddCommodityRare("Ethgreze Tea Buds", ItemType.Foods, "EthgrezeTeaBuds");
            AddCommodityRare("Ceremonial Heike Tea", ItemType.Foods, "CeremonialHeikeTea");
            AddCommodityRare("Tanmark Tranquil Tea", ItemType.Foods, "TanmarkTranquilTea");
            AddCommodityRare("AZ Cancri Formula 42", ItemType.Technology, "AZCancriFormula42");
            AddCommodityRare("Sothis Crystalline Gold", ItemType.Metals, "SothisCrystallineGold");
            AddCommodityRare("Kamitra Cigars", ItemType.LegalDrugs, "KamitraCigars");
            AddCommodityRare("Rusani Old Smokey", ItemType.LegalDrugs, "RusaniOldSmokey");
            AddCommodityRare("Yaso Kondi Leaf", ItemType.LegalDrugs, "YasoKondiLeaf");
            AddCommodityRare("Chateau De Aegaeon", ItemType.LegalDrugs, "ChateauDeAegaeon");
            AddCommodityRare("The Waters Of Shintara", ItemType.Medicines, "WatersOfShintara");
            AddCommodityRare("Ophiuch Exino Artefacts", ItemType.ConsumerItems, "OphiuchiExinoArtefacts");
            AddCommodityRare("Baked Greebles", ItemType.Foods, "BakedGreebles");
            AddCommodityRare("Aepyornis Egg", ItemType.Foods, "CetiAepyornisEgg");
            AddCommodityRare("Saxon Wine", ItemType.LegalDrugs, "SaxonWine");
            AddCommodityRare("Centauri Mega Gin", ItemType.LegalDrugs, "CentauriMegaGin");
            AddCommodityRare("Anduliga Fire Works", ItemType.Chemicals, "AnduligaFireWorks");
            AddCommodityRare("Banki Amphibious Leather", ItemType.Textiles, "BankiAmphibiousLeather");
            AddCommodityRare("Cherbones Blood Crystals", ItemType.Minerals, "CherbonesBloodCrystals");
            AddCommodityRare("Motrona Experience Jelly", ItemType.Narcotics, "MotronaExperienceJelly");
            AddCommodityRare("Geawen Dance Dust", ItemType.Narcotics, "GeawenDanceDust");
            AddCommodityRare("Gerasian Gueuze Beer", ItemType.LegalDrugs, "GerasianGueuzeBeer");
            AddCommodityRare("Haiden Black Brew", ItemType.Foods, "HaidneBlackBrew");
            AddCommodityRare("Havasupai Dream Catcher", ItemType.ConsumerItems, "HavasupaiDreamCatcher");
            AddCommodityRare("Burnham Bile Distillate", ItemType.LegalDrugs, "BurnhamBileDistillate");
            AddCommodityRare("Hip Organophosphates", ItemType.Chemicals, "HIPOrganophosphates");
            AddCommodityRare("Jaradharre Puzzle Box", ItemType.ConsumerItems, "JaradharrePuzzlebox");
            AddCommodityRare("Koro Kung Pellets", ItemType.Chemicals, "KorroKungPellets");
            AddCommodityRare("Void Extract Coffee", ItemType.Foods, "LFTVoidExtractCoffee");
            AddCommodityRare("Honesty Pills", ItemType.Medicines, "HonestyPills");
            AddCommodityRare("Non Euclidian Exotanks", ItemType.Machinery, "NonEuclidianExotanks");
            AddCommodityRare("LTT Hyper Sweet", ItemType.Foods, "LTTHyperSweet");
            AddCommodityRare("Mechucos High Tea", ItemType.Foods, "MechucosHighTea");
            AddCommodityRare("Medb Starlube", ItemType.IndustrialMaterials, "MedbStarlube");
            AddCommodityRare("Mokojing Beast Feast", ItemType.Foods, "MokojingBeastFeast");
            AddCommodityRare("Mukusubii Chitin-os", ItemType.Foods, "MukusubiiChitinOs");
            AddCommodityRare("Mulachi Giant Fungus", ItemType.Foods, "MulachiGiantFungus");
            AddCommodityRare("Ngadandari Fire Opals", ItemType.Minerals, "NgadandariFireOpals");
            AddCommodityRare("Tiegfries Synth Silk", ItemType.Textiles, "TiegfriesSynthSilk");
            AddCommodityRare("Uzumoku Low-G Wings", ItemType.ConsumerItems, "UzumokuLowGWings");
            AddCommodityRare("V Herculis Body Rub", ItemType.Medicines, "VHerculisBodyRub");
            AddCommodityRare("Wheemete Wheat Cakes", ItemType.Foods, "WheemeteWheatCakes");
            AddCommodityRare("Vega Slimweed", ItemType.Medicines, "VegaSlimWeed");
            AddCommodityRare("Altairian Skin", ItemType.ConsumerItems, "AltairianSkin");
            AddCommodityRare("Pavonis Ear Grubs", ItemType.Narcotics, "PavonisEarGrubs");
            AddCommodityRare("Jotun Mookah", ItemType.ConsumerItems, "JotunMookah");
            AddCommodityRare("Giant Verrix", ItemType.Machinery, "GiantVerrix");
            AddCommodityRare("Indi Bourbon", ItemType.LegalDrugs, "IndiBourbon");
            AddCommodityRare("Arouca Conventual Sweets", ItemType.Foods, "AroucaConventualSweets");
            AddCommodityRare("Tauri Chimes", ItemType.Medicines, "TauriChimes");
            AddCommodityRare("Zeessze Ant Grub Glue", ItemType.ConsumerItems, "ZeesszeAntGlue");
            AddCommodityRare("Pantaa Prayer Sticks", ItemType.Medicines, "PantaaPrayerSticks");
            AddCommodityRare("Fujin Tea", ItemType.Medicines, "FujinTea");
            AddCommodityRare("Chameleon Cloth", ItemType.Textiles, "ChameleonCloth");
            AddCommodityRare("Orrerian Vicious Brew", ItemType.Foods, "OrrerianViciousBrew");
            AddCommodityRare("Uszaian Tree Grub", ItemType.Foods, "UszaianTreeGrub");
            AddCommodityRare("Momus Bog Spaniel", ItemType.ConsumerItems, "MomusBogSpaniel");
            AddCommodityRare("Diso Ma Corn", ItemType.Foods, "DisoMaCorn");
            AddCommodityRare("Leestian Evil Juice", ItemType.LegalDrugs, "LeestianEvilJuice");
            AddCommodityRare("Azure Milk", ItemType.LegalDrugs, "BlueMilk");
            AddCommodityRare("Leathery Eggs", ItemType.ConsumerItems, "AlienEggs");
            AddCommodityRare("Alya Body Soap", ItemType.Medicines, "AlyaBodilySoap");
            AddCommodityRare("Vidavantian Lace", ItemType.ConsumerItems, "VidavantianLace");
            AddCommodityRare("Lucan Onionhead", ItemType.Narcotics, "TransgenicOnionHead");
            AddCommodityRare("Jaques Quinentian Still", ItemType.ConsumerItems, "JaquesQuinentianStill");
            AddCommodityRare("Soontill Relics", ItemType.ConsumerItems, "SoontillRelics");
            AddCommodityRare("Onionhead Alpha Strain", ItemType.Narcotics, "OnionHeadA");
            AddCommodityRare("Onionhead Beta Strain", ItemType.Narcotics, "OnionHeadB");
            AddCommodityRare("Galactic Travel Guide", sv, "GalacticTravelGuide");
            AddCommodityRare("Crom Silver Fesh", ItemType.Narcotics, "AnimalEffigies");
            AddCommodityRare("Shan's Charis Orchid", ItemType.ConsumerItems, "ShansCharisOrchid");
            AddCommodityRare("Buckyball Beer Mats", ItemType.ConsumerItems, "BuckyballBeerMats");
            AddCommodityRare("Master Chefs", ItemType.Slaves, "MasterChefs");
            AddCommodityRare("Personal Gifts", ItemType.ConsumerItems, "PersonalGifts");
            AddCommodityRare("Crystalline Spheres", ItemType.ConsumerItems, "CrystallineSpheres");
            AddCommodityRare("Ultra-Compact Processor Prototypes", ItemType.ConsumerItems, "Advert1");
            AddCommodityRare("Harma Silver Sea Rum", ItemType.Narcotics, "HarmaSilverSeaRum");
            AddCommodityRare("Earth Relics", sv, "EarthRelics");

            #endregion

            #region Powerplay 

            AddCommodity("Aisling Media Materials", ItemType.PowerPlay, "AislingMediaMaterials");
            AddCommodity("Aisling Sealed Contracts", ItemType.PowerPlay, "AislingMediaResources");
            AddCommodity("Aisling Programme Materials", ItemType.PowerPlay, "AislingPromotionalMaterials");
            AddCommodity("Alliance Trade Agreements", ItemType.PowerPlay, "AllianceTradeAgreements");
            AddCommodity("Alliance Legislative Contracts", ItemType.PowerPlay, "AllianceLegaslativeContracts");
            AddCommodity("Alliance Legislative Records", ItemType.PowerPlay, "AllianceLegaslativeRecords");
            AddCommodity("Lavigny Corruption Reports", ItemType.PowerPlay, "LavignyCorruptionDossiers");
            AddCommodity("Lavigny Field Supplies", ItemType.PowerPlay, "LavignyFieldSupplies");
            AddCommodity("Lavigny Garrison Supplies", ItemType.PowerPlay, "LavignyGarisonSupplies");
            AddCommodity("Core Restricted Package", ItemType.PowerPlay, "RestrictedPackage");
            AddCommodity("Liberal Propaganda", ItemType.PowerPlay, "LiberalCampaignMaterials");
            AddCommodity("Liberal Federal Aid", ItemType.PowerPlay, "FederalAid");
            AddCommodity("Liberal Federal Packages", ItemType.PowerPlay, "FederalTradeContracts");
            AddCommodity("Marked Military Arms", ItemType.PowerPlay, "LoanedArms");
            AddCommodity("Patreus Field Supplies", ItemType.PowerPlay, "PatreusFieldSupplies");
            AddCommodity("Patreus Garrison Supplies", ItemType.PowerPlay, "PatreusGarisonSupplies");
            AddCommodity("Hudson's Restricted Intel", ItemType.PowerPlay, "RestrictedIntel");
            AddCommodity("Hudson's Field Supplies", ItemType.PowerPlay, "RepublicanFieldSupplies");
            AddCommodity("Hudson Garrison Supplies", ItemType.PowerPlay, "RepublicanGarisonSupplies");
            AddCommodity("Sirius Franchise Package", ItemType.PowerPlay, "SiriusFranchisePackage");
            AddCommodity("Sirius Corporate Contracts", ItemType.PowerPlay, "SiriusCommercialContracts");
            AddCommodity("Sirius Industrial Equipment", ItemType.PowerPlay, "SiriusIndustrialEquipment");
            AddCommodity("Torval Trade Agreements", ItemType.PowerPlay, "TorvalCommercialContracts");
            AddCommodity("Torval Political Prisoners", ItemType.PowerPlay, "ImperialPrisoner");
            AddCommodity("Utopian Publicity", ItemType.PowerPlay, "UtopianPublicity");
            AddCommodity("Utopian Supplies", ItemType.PowerPlay, "UtopianFieldSupplies");
            AddCommodity("Utopian Dissident", ItemType.PowerPlay, "UtopianDissident");
            AddCommodity("Kumo Contraband Package", ItemType.PowerPlay, "IllicitConsignment");
            AddCommodity("Unmarked Military supplies", ItemType.PowerPlay, "UnmarkedWeapons");
            AddCommodity("Onionhead Samples", ItemType.PowerPlay, "OnionheadSamples");
            AddCommodity("Revolutionary supplies", ItemType.PowerPlay, "CounterCultureSupport");
            AddCommodity("Onionhead Derivatives", ItemType.PowerPlay, "OnionheadDerivatives");
            AddCommodity("Out Of Date Goods", ItemType.PowerPlay, "OutOfDateGoods");
            AddCommodity("Grom Underground Support", ItemType.PowerPlay, "UndergroundSupport");
            AddCommodity("Grom Counter Intelligence", ItemType.PowerPlay, "GromCounterIntelligence");
            AddCommodity("Yuri Grom's Military Supplies", ItemType.PowerPlay, "GromWarTrophies");
            AddCommodity("Marked Slaves", ItemType.PowerPlay, "MarkedSlaves");
            AddCommodity("Torval Deeds", ItemType.PowerPlay, "TorvalDeeds");

            #endregion

            #region Microresources

            AddMicroResource(CatType.Consumable, "E-Breach", "bypass", "MRCEB");
            AddMicroResource(CatType.Consumable, "Energy Cell", "energycell", "MRCEC");
            AddMicroResource(CatType.Consumable, "Medkit", "healthpack", "MRCM");
            AddMicroResource(CatType.Consumable, "Frag Grenade", "amm_grenade_frag", "MRCFG");
            AddMicroResource(CatType.Consumable, "Shield Disruptor", "amm_grenade_emp", "MRCSD");
            AddMicroResource(CatType.Consumable, "Shield Projector", "amm_grenade_shield", "MRCSP");

            AddMicroResource(CatType.Item, "Power Regulator", "largecapacitypowerregulator", "MRIPR");
            AddMicroResource(CatType.Item, "Compact Library", "compactlibrary", "MRICL");
            AddMicroResource(CatType.Item, "Infinity", "infinity", "MRII");
            AddMicroResource(CatType.Item, "Insight Entertainment Suite", "insightentertainmentsuite", "MRIIES");
            AddMicroResource(CatType.Item, "Lazarus", "lazarus", "MRIL");
            AddMicroResource(CatType.Item, "Universal Translator", "universaltranslator", "MRIUT");
            AddMicroResource(CatType.Item, "Biochemical Agent", "biochemicalagent", "MRIBA");
            AddMicroResource(CatType.Item, "Degraded Power Regulator", "degradedpowerregulator", "MRIDPR");
            AddMicroResource(CatType.Item, "Hush", "hush", "MRIH");
            AddMicroResource(CatType.Item, "Push", "push", "MRIP");
            AddMicroResource(CatType.Item, "Synthetic Pathogen", "syntheticpathogen", "MRISP");
            AddMicroResource(CatType.Item, "Building Schematic", "buildingschematic", "MRIBS");
            AddMicroResource(CatType.Item, "Insight", "insight", "MRInsight");
            AddMicroResource(CatType.Item, "Compression-Liquefied Gas", "compressionliquefiedgas", "MRICLG");
            AddMicroResource(CatType.Item, "Health Monitor", "healthmonitor", "MRIHM");
            AddMicroResource(CatType.Item, "Inertia Canister", "inertiacanister", "MRIIC");
            AddMicroResource(CatType.Item, "Ionised Gas", "ionisedgas", "MRIIG");
            AddMicroResource(CatType.Item, "Ship Schematic", "shipschematic", "MRIShipSch");
            AddMicroResource(CatType.Item, "Suit Schematic", "suitschematic", "MRISuitSch");
            AddMicroResource(CatType.Item, "Vehicle Schematic", "vehicleschematic", "MRIVS");
            AddMicroResource(CatType.Item, "Weapon Schematic", "weaponschematic", "MRIWS");
            AddMicroResource(CatType.Item, "True Form Fossil", "trueformfossil", "MRITFF");
            AddMicroResource(CatType.Item, "Agricultural Process Sample", "AgriculturalProcessSample", "MRIAPS");
            AddMicroResource(CatType.Item, "Biological Sample", "GeneticSample", "MRIBIOSAMP");
            AddMicroResource(CatType.Item, "Californium", "Californium", "MRIC");
            AddMicroResource(CatType.Item, "Cast Fossil", "CastFossil", "MRICF");
            AddMicroResource(CatType.Item, "Chemical Process Sample", "ChemicalProcessSample", "MRICPS");
            AddMicroResource(CatType.Item, "Chemical Sample", "ChemicalSample", "MRICS");
            AddMicroResource(CatType.Item, "Deep Mantle Sample", "DeepMantleSample", "MRIDMS");
            AddMicroResource(CatType.Item, "Genetic Repair Meds", "GeneticRepairMeds", "MRIGRM");
            AddMicroResource(CatType.Item, "G-Meds", "GMeds", "MRIGM");
            AddMicroResource(CatType.Item, "Inorganic Contaminant", "InorganicContaminant", "MRIINORGANC");
            AddMicroResource(CatType.Item, "Insight Data Bank", "InsightDataBank", "MRIIDB");
            AddMicroResource(CatType.Item, "Microbial Inhibitor", "MicrobialInhibitor", "MRIMI");
            AddMicroResource(CatType.Item, "Mutagenic Catalyst", "MutagenicCatalyst", "MRIMC");
            AddMicroResource(CatType.Item, "Nutritional Concentrate", "NutritionalConcentrate", "MRINC");
            AddMicroResource(CatType.Item, "Personal Computer", "PersonalComputer", "MRIPC");
            AddMicroResource(CatType.Item, "Personal Documents", "PersonalDocuments", "MRIPD");
            AddMicroResource(CatType.Item, "Petrified Fossil", "PetrifiedFossil", "MRIPF");
            AddMicroResource(CatType.Item, "Pyrolytic Catalyst", "PyrolyticCatalyst", "MRIPRYOCAT");
            AddMicroResource(CatType.Item, "Refinement Process Sample", "RefinementProcessSample", "MRIRPS");
            AddMicroResource(CatType.Item, "Surveillance Equipment", "SurveillanceEquipment", "MRISE");
            AddMicroResource(CatType.Item, "Synthetic Genome", "SyntheticGenome", "MRISG");

            AddMicroResource(CatType.Component, "Carbon Fibre Plating", "carbonfibreplating", "MRCCFP");
            AddMicroResource(CatType.Component, "Encrypted Memory Chip", "encryptedmemorychip", "MRCEMC");
            AddMicroResource(CatType.Component, "Epoxy Adhesive", "epoxyadhesive", "MRCEA");
            AddMicroResource(CatType.Component, "Graphene", "graphene", "MRCG");
            AddMicroResource(CatType.Component, "Memory Chip", "memorychip", "MRCMC");
            AddMicroResource(CatType.Component, "Microelectrode", "microelectrode", "MRCMICROELEC");
            AddMicroResource(CatType.Component, "Micro Hydraulics", "microhydraulics", "MRCMH");
            AddMicroResource(CatType.Component, "Micro Thrusters", "microthrusters", "MRCMT");
            AddMicroResource(CatType.Component, "Optical Fibre", "opticalfibre", "MRCOF");
            AddMicroResource(CatType.Component, "Optical Lens", "opticallens", "MRCOL");
            AddMicroResource(CatType.Component, "RDX", "rdx", "MRCR");
            AddMicroResource(CatType.Component, "Titanium Plating", "titaniumplating", "MRCTP");
            AddMicroResource(CatType.Component, "Tungsten Carbide", "tungstencarbide", "MRCTC");
            AddMicroResource(CatType.Component, "Weapon Component", "weaponcomponent", "MRCWC");
            AddMicroResource(CatType.Component, "Aerogel", "aerogel", "MRCA");
            AddMicroResource(CatType.Component, "Chemical Superbase", "chemicalsuperbase", "MRCCS");
            AddMicroResource(CatType.Component, "Circuit Board", "circuitboard", "MRCCB");
            AddMicroResource(CatType.Component, "Circuit Switch", "circuitswitch", "MRCCIRSWITCH");
            AddMicroResource(CatType.Component, "Electrical Wiring", "electricalwiring", "MRCEW");
            AddMicroResource(CatType.Component, "Electromagnet", "electromagnet", "MRCE");
            AddMicroResource(CatType.Component, "Metal Coil", "metalcoil", "MRCMETALCOIL");
            AddMicroResource(CatType.Component, "Motor", "motor", "MRCMOTOR");
            AddMicroResource(CatType.Component, "Chemical Catalyst", "chemicalcatalyst", "MRCCC");
            AddMicroResource(CatType.Component, "Micro Transformer", "microtransformer", "MRCMICTRANS");
            AddMicroResource(CatType.Component, "Electrical Fuse", "electricalfuse", "MRCEF");
            AddMicroResource(CatType.Component, "Micro Supercapacitor", "microsupercapacitor", "MRCMS");
            AddMicroResource(CatType.Component, "Viscoelastic Polymer", "viscoelasticpolymer", "MRCVP");
            AddMicroResource(CatType.Component, "Ion Battery", "ionbattery", "MRCIB");
            AddMicroResource(CatType.Component, "Scrambler", "scrambler", "MRCS");
            AddMicroResource(CatType.Component, "Oxygenic Bacteria", "oxygenicbacteria", "MRCOXYBAC");
            AddMicroResource(CatType.Component, "Epinephrine", "Epinephrine", "MRCEPINE");
            AddMicroResource(CatType.Component, "pH Neutraliser", "pHNeutraliser", "MRCPHN");
            AddMicroResource(CatType.Component, "Transmitter", "Transmitter", "MRCTX");

            AddMicroResource(CatType.Data, "Chemical Inventory", "chemicalinventory", "MRDCI");
            AddMicroResource(CatType.Data, "Duty Rota", "dutyrota", "MRDDR");
            AddMicroResource(CatType.Data, "Evacuation Protocols", "evacuationprotocols", "MRDEP");
            AddMicroResource(CatType.Data, "Exploration Journals", "explorationjournals", "MRDEJ");
            AddMicroResource(CatType.Data, "Faction News", "factionnews", "MRDFN");
            AddMicroResource(CatType.Data, "Financial Projections", "financialprojections", "MRDFP");
            AddMicroResource(CatType.Data, "Sales Records", "salesrecords", "MRDSR");
            AddMicroResource(CatType.Data, "Union Membership", "unionmembership", "MRDUM");
            AddMicroResource(CatType.Data, "Maintenance Logs", "maintenancelogs", "MRDML");
            AddMicroResource(CatType.Data, "Patrol Routes", "patrolroutes", "MRDPATR");
            AddMicroResource(CatType.Data, "Settlement Defence Plans", "settlementdefenceplans", "MRDSDP");
            AddMicroResource(CatType.Data, "Surveillance Logs", "surveilleancelogs", "MRDSL");
            AddMicroResource(CatType.Data, "Operational Manual", "operationalmanual", "MRDOM");
            AddMicroResource(CatType.Data, "Blacklist Data", "blacklistdata", "MRDBD");
            AddMicroResource(CatType.Data, "Air Quality Reports", "airqualityreports", "MRDAQR");
            AddMicroResource(CatType.Data, "Employee Directory", "employeedirectory", "MRDED");
            AddMicroResource(CatType.Data, "Faction Associates", "factionassociates", "MRDFA");
            AddMicroResource(CatType.Data, "Meeting Minutes", "meetingminutes", "MRDMM");
            AddMicroResource(CatType.Data, "Multimedia Entertainment", "multimediaentertainment", "MRDME");
            AddMicroResource(CatType.Data, "Network Access History", "networkaccesshistory", "MRDNAH");
            AddMicroResource(CatType.Data, "Purchase Records", "purchaserecords", "MRDPRCD");
            AddMicroResource(CatType.Data, "Radioactivity Data", "radioactivitydata", "MRDRD");
            AddMicroResource(CatType.Data, "Residential Directory", "residentialdirectory", "MRDRDIR");
            AddMicroResource(CatType.Data, "Shareholder Information", "shareholderinformation", "MRDSI");
            AddMicroResource(CatType.Data, "Travel Permits", "travelpermits", "MRDTP");
            AddMicroResource(CatType.Data, "Accident Logs", "accidentlogs", "MRDACCLOGS");
            AddMicroResource(CatType.Data, "Campaign Plans", "campaignplans", "MRDCP");
            AddMicroResource(CatType.Data, "Combat Training Material", "combattrainingmaterial", "MRDCTM");
            AddMicroResource(CatType.Data, "Internal Correspondence", "internalcorrespondence", "MRDIC");
            AddMicroResource(CatType.Data, "Payroll Information", "payrollinformation", "MRDPI");
            AddMicroResource(CatType.Data, "Personal Logs", "personallogs", "MRDPL");
            AddMicroResource(CatType.Data, "Weapon Inventory", "weaponinventory", "MRDWI");
            AddMicroResource(CatType.Data, "Atmospheric Data", "atmosphericdata", "MRDAD");
            AddMicroResource(CatType.Data, "Topographical Surveys", "topographicalsurveys", "MRDTS");
            AddMicroResource(CatType.Data, "Literary Fiction", "literaryfiction", "MRDLF");
            AddMicroResource(CatType.Data, "Reactor Output Review", "reactoroutputreview", "MRDROR");
            AddMicroResource(CatType.Data, "Next of Kin Records", "nextofkinrecords", "MRDNKR");
            AddMicroResource(CatType.Data, "Purchase Requests", "purchaserequests", "MRDPR");
            AddMicroResource(CatType.Data, "Tax Records", "taxrecords", "MRDTR");
            AddMicroResource(CatType.Data, "Visitor Register", "visitorregister", "MRDVISREG");
            AddMicroResource(CatType.Data, "Pharmaceutical Patents", "pharmaceuticalpatents", "MRDPP");
            AddMicroResource(CatType.Data, "Vaccine Research", "vaccineresearch", "MRDVACRES");
            AddMicroResource(CatType.Data, "Virology Data", "virologydata", "MRDVD");
            AddMicroResource(CatType.Data, "Vaccination Records", "vaccinationrecords", "MRDVR");
            AddMicroResource(CatType.Data, "Census Data", "censusdata", "MRDCD");
            AddMicroResource(CatType.Data, "Mineral Survey", "mineralsurvey", "MRDMS");
            AddMicroResource(CatType.Data, "Chemical Formulae", "chemicalformulae", "MRDCF");
            AddMicroResource(CatType.Data, "Chemical Experiment Data", "chemicalexperimentdata", "MRDCED");
            AddMicroResource(CatType.Data, "Chemical Patents", "chemicalpatents", "MRDCHEMPAT");
            AddMicroResource(CatType.Data, "Production Reports", "productionreports", "MRDPRODREP");
            AddMicroResource(CatType.Data, "Production Schedule", "productionschedule", "MRDPS");
            AddMicroResource(CatType.Data, "Blood Test Results", "bloodtestresults", "MRDBTR");
            AddMicroResource(CatType.Data, "Combatant Performance", "combatantperformance", "MRDCOMBPERF");
            AddMicroResource(CatType.Data, "Troop Deployment Records", "troopdeploymentrecords", "MRDTDR");
            AddMicroResource(CatType.Data, "Cat Media", "catmedia", "MRDCATMED");
            AddMicroResource(CatType.Data, "Employee Genetic Data", "employeegeneticdata", "MRDEGD");
            AddMicroResource(CatType.Data, "Faction Donator List", "factiondonatorlist", "MRDFDL");
            AddMicroResource(CatType.Data, "NOC Data", "nocdata", "MRDNOCD");
            AddMicroResource(CatType.Data, "Manufacturing Instructions", "manufacturinginstructions", "MRDMI");
            AddMicroResource(CatType.Data, "Propaganda", "propaganda", "MRPROPG");
            AddMicroResource(CatType.Data, "Security Expenses", "securityexpenses", "MRSECEXP");
            AddMicroResource(CatType.Data, "Weapon Test Data", "weapontestdata", "MRWTD");
            AddMicroResource(CatType.Data, "Audio Logs", "AudioLogs", "MRDAL");
            AddMicroResource(CatType.Data, "AX Combat Logs", "AXCombatLogs", "MRDACL");
            AddMicroResource(CatType.Data, "Ballistics Data", "BallisticsData", "MRDBALD");
            AddMicroResource(CatType.Data, "Biological Weapon Data", "BiologicalWeaponData", "MRDBWD");
            AddMicroResource(CatType.Data, "Biometric Data", "BiometricData", "MRDBIOD");
            AddMicroResource(CatType.Data, "Chemical Weapon Data", "ChemicalWeaponData", "MRDCWD");
            AddMicroResource(CatType.Data, "Classic Entertainment", "ClassicEntertainment", "MRDCE");
            AddMicroResource(CatType.Data, "Cocktail Recipes", "CocktailRecipes", "MRDCREC");
            AddMicroResource(CatType.Data, "Conflict History", "ConflictHistory", "MRDCH");
            AddMicroResource(CatType.Data, "Criminal Records", "CriminalRecords", "MRDCRIMREC");
            AddMicroResource(CatType.Data, "Crop Yield Analysis", "CropYieldAnalysis", "MRDCYA");
            AddMicroResource(CatType.Data, "Culinary Recipes", "CulinaryRecipes", "MRDCULREC");
            AddMicroResource(CatType.Data, "Digital Designs", "DigitalDesigns", "MRDDD");
            AddMicroResource(CatType.Data, "Employee Expenses", "EmployeeExpenses", "MRDEE");
            AddMicroResource(CatType.Data, "Employment History", "EmploymentHistory", "MRDEH");
            AddMicroResource(CatType.Data, "Enhanced Interrogation Recordings", "EnhancedInterrogationRecordings", "MRDEIR");
            AddMicroResource(CatType.Data, "Espionage Material", "EspionageMaterial", "MRDEM");
            AddMicroResource(CatType.Data, "Extraction Yield Data", "ExtractionYieldData", "MRDEYD");
            AddMicroResource(CatType.Data, "Fleet Registry", "FleetRegistry", "MRDFR");
            AddMicroResource(CatType.Data, "Gene Sequencing Data", "GeneSequencingData", "MRDGSD");
            AddMicroResource(CatType.Data, "Genetic Research", "GeneticResearch", "MRDGR");
            AddMicroResource(CatType.Data, "Geological Data", "GeologicalData", "MRDGD");
            AddMicroResource(CatType.Data, "Hydroponic Data", "HydroponicData", "MRDHD");
            AddMicroResource(CatType.Data, "Incident Logs", "IncidentLogs", "MRDIL");
            AddMicroResource(CatType.Data, "Influence Projections", "InfluenceProjections", "MRDIP");
            AddMicroResource(CatType.Data, "Interrogation Recordings", "InterrogationRecordings", "MRDINTERREC");
            AddMicroResource(CatType.Data, "Interview Recordings", "InterviewRecordings", "MRDINTERVREC");
            AddMicroResource(CatType.Data, "Job Applications", "JobApplications", "MRDJA");
            AddMicroResource(CatType.Data, "Kompromat", "Kompromat", "MRDK");
            AddMicroResource(CatType.Data, "Medical Records", "MedicalRecords", "MRDMR");
            AddMicroResource(CatType.Data, "Clinical Trial Records", "MedicalTrialRecords", "MRDCTR");
            AddMicroResource(CatType.Data, "Mining Analytics", "MiningAnalytics", "MRDMA");
            AddMicroResource(CatType.Data, "Network Security Protocols", "NetworkSecurityProtocols", "MRDNSP");
            AddMicroResource(CatType.Data, "Opinion Polls", "OpinionPolls", "MRDOP");
            AddMicroResource(CatType.Data, "Patient History", "PatientHistory", "MRDPH");
            AddMicroResource(CatType.Data, "Photo Albums", "PhotoAlbums", "MRDPHOTO");
            AddMicroResource(CatType.Data, "Plant Growth Charts", "PlantGrowthCharts", "MRDPGC");
            AddMicroResource(CatType.Data, "Political Affiliations", "PoliticalAffiliations", "MRDPOL");
            AddMicroResource(CatType.Data, "Prisoner Logs", "PrisonerLogs", "MRDPRISONL");
            AddMicroResource(CatType.Data, "Recycling Logs", "RecyclingLogs", "MRDRL");
            AddMicroResource(CatType.Data, "Risk Assessments", "RiskAssessments", "MRDRA");
            AddMicroResource(CatType.Data, "Seed Geneaology", "SeedGeneaology", "MRDSG");
            AddMicroResource(CatType.Data, "Settlement Assault Plans", "SettlementAssaultPlans", "MRDSAP");
            AddMicroResource(CatType.Data, "Slush Fund Logs", "SlushFundLogs", "MRDSFL");
            AddMicroResource(CatType.Data, "Smear Campaign Plans", "SmearCampaignPlans", "MRDSCP");
            AddMicroResource(CatType.Data, "Spectral Analysis Data", "SpectralAnalysisData", "MRDSAD");
            AddMicroResource(CatType.Data, "Spyware", "Spyware", "MRDS");
            AddMicroResource(CatType.Data, "Stellar Activity Logs", "StellarActivityLogs", "MRDSAL");
            AddMicroResource(CatType.Data, "Tactical Plans", "TacticalPlans", "MRDTACP");
            AddMicroResource(CatType.Data, "VIP Security Detail", "VIPSecurityDetail", "MRDVSD");
            AddMicroResource(CatType.Data, "Virus", "Virus", "MRDV");
            AddMicroResource(CatType.Data, "Xeno-Defence Protocols", "XenoDefenceProtocols", "MRDXDP");

            // not in frontier spreadsheet, but seen in alpha logs
            AddMicroResource(CatType.Data, "Geographical Data", "geographicaldata", "MRDGEOD");

            #endregion


            AddCommodity("Drones", ItemType.Drones, "Drones");

            foreach (var x in cachelist.Values)
            {
                x.Name = BaseUtils.Translator.Instance.Translate(x.Name, "MaterialCommodityMicroResourceType." + x.FDName);
            }

            // foreach (MaterialCommodityData d in cachelist.Values) System.Diagnostics.Debug.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", d.Category, d.Type.ToString().SplitCapsWord(), d.MaterialGroup.ToString(), d.FDName, d.Name, d.Shortname, d.Rarity ));
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

        static public string FDNameTranslation(string old)
        {
            old = old.ToLowerInvariant();
            if (fdnamemangling.ContainsKey(old))
            {
                //System.Diagnostics.Debug.WriteLine("Sub " + old);
                return fdnamemangling[old];
            }
            else
                return old;
        }

        #endregion
    }
}


