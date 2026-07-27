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
using System.Collections.Generic;

namespace EliteDangerousCore
{

    public class ModFDName: FDName
    {
        public ModFDName() : base()
        {
        }

        public ModFDName(string fdname) : base(fdname) 
        {
        }

        public ModFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public new ModFDName Clone()
        {
            return new ModFDName(Str());
        }

        public static new ModFDName Empty => new ModFDName();

        public static ModFDName Normalise(string fdname, out string modulename, JournalEntry ev, bool allownull = false)
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
                    BaseUtils.Debugger.TraceBreak("*** Missing Module");
                    return new ModFDName("Unknown Module");
                }
            }
            else
            {
                if (fdname.Length >= 8 && fdname[0] == '$' && fdname.EndsWith("_name;", System.StringComparison.InvariantCultureIgnoreCase))
                    fdname = fdname.Substring(1, fdname.Length - 7);        // remove decoration

                var ret = new ModFDName(fdname);
                if (ItemData.TryGetShipModule(ret, out ItemData.ShipModule module, true))
                {
                    modulename = module.EnglishModName;
                }
                else
                {
                    BaseUtils.Debugger.TraceBreak("*** Unknown Module `{fdname}`");
                    modulename = "Unknown Module " + fdname;
                }

                return ret;
            }
        }

        public string GetForeignModuleName(string loc = null, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)
        {
            if (ItemData.TryGetShipModule(this, out ItemData.ShipModule item, true, slot))
                return item.TranslatedModTypeString();
            else
                return loc ?? Str();
        }
        public string GetForeignModuleType(string loc = null, ShipSlots.Slot slot = ShipSlots.Slot.Unknown)
        {
            if (ItemData.TryGetShipModule(this, out ItemData.ShipModule item, true, slot))
                return item.TranslatedModTypeString();
            else
                return loc ?? Str();
        }

        public bool IsWeaponArmour()
        {
            return ToLower().StartsWith("hpt_", StringComparison.InvariantCultureIgnoreCase) ||
                                            ToLower().StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase) ||
                                            ToLower().Contains("_armour_", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class ModFDNameEqualityComparer : IEqualityComparer<ModFDName>
    {
        public bool Equals(ModFDName left, ModFDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(ModFDName obj)
        {
            return obj.GetHashCode();
        }
    }
}
