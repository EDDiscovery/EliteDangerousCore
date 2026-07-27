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
 *
 *
 */
using QuickJSON;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.JetConeBoost)]
    public class JournalJetConeBoost : JournalEntry
    {
        public JournalJetConeBoost(JObject evt ) : base(evt, JournalTypeEnum.JetConeBoost)
        {
            BoostValue = evt["BoostValue"].Double();

        }
        public double BoostValue { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("Boost: ;;0.0".Tx(), BoostValue);
        }
    }

    [JournalEntryType(JournalTypeEnum.JetConeDamage)]
    public class JournalJetConeDamage : JournalEntry
    {
        public JournalJetConeDamage(JObject evt) : base(evt, JournalTypeEnum.JetConeDamage)
        {
            string modid = evt["Module"].Str();
            if (modid.HasChars())     // earl ones from 2016 were really borked
            {
                ModuleFD = ModFDName.Normalise(modid, out string engname, this);
                Module = engname;
            }
            else
            {
                ModuleFD = ModFDName.Empty;
                Module = ModuleFD.Str();
            }

            ModuleLocalised = JournalFieldNaming.CheckLocalisation(evt["Module_Localised"].Str(), Module);
        }

        public ModFDName ModuleFD { get; set; }
        public string Module { get; set; }      // english name
        public string ModuleLocalised { get; set; }

        public override string GetInfo()
        {
            return ModuleFD.IsValid ? ModuleFD.GetForeignModuleName(ModuleLocalised) : "";
        }
    }

}
