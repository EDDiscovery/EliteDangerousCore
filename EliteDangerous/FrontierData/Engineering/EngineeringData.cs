/*t
 * Copyright © 2018-2024 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{Label} {OriginalValue} -> {Value}")]
    public class EngineeringModifiers
    {
        public string Label { get; set; }
        public string FriendlyLabel { get; set; }
        public string ValueStr { get; set; }            // 3.02 if set, means ones further on do not apply. check first
        public string ValueStr_Localised { get; set; }
        public double Value { get; set; }               // may be 0
        public double OriginalValue { get; set; }
        public bool LessIsGood { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Engineer} {BlueprintName} {Level} {ExperimentalEffect}")]
    public class EngineeringData
    {
        public string Engineer { get; set; }
        public string BlueprintName { get; set; }       // not case corrected - as inara gets it, best to leave its case.
        public string FriendlyBlueprintName { get; set; }
        public ulong EngineerID { get; set; }
        public ulong BlueprintID { get; set; }
        public int Level { get; set; }
        public double Quality { get; set; }
        public string ExperimentalEffect { get; set; }      // may be null or maybe empty (due to frontier) use HasChars()
        public string ExperimentalEffect_Localised { get; set; }    // may be null or maybe empty (due to frontier)
        public EngineeringModifiers[] Modifiers { get; set; }       // may be null

        public bool IsValid { get { return Level >= 1 && BlueprintName.HasChars(); } }

        // Post engineering changes
        public EngineeringData(JObject evt)
        {
            Engineer = evt["Engineer"].Str("Unknown");
            Level = evt["Level"].Int();

            if (evt.Contains("Blueprint"))     // old form
            {
                BlueprintName = evt["Blueprint"].Str();
            }
            else
            {
                EngineerID = evt["EngineerID"].ULong();     // NEW FORM after engineering changes in about 2018
                BlueprintName = evt["BlueprintName"].Str();
                BlueprintID = evt["BlueprintID"].ULong();
                Quality = evt["Quality"].Double(0);

                // EngineerCraft has it as Apply.. Loadout has just ExperimentalEffect.  Check both
                ExperimentalEffect = evt.MultiStr(new string[] { "ExperimentalEffect", "ApplyExperimentalEffect" }, null);
                if (ExperimentalEffect.HasChars())
                {
                    string loc = evt["ExperimentalEffect_Localised"].StrNull();

                    // seen records with localised=experimental effect so protect that.
                    if (loc.EqualsIIC(ExperimentalEffect))
                    {
                        var recp = Recipes.FindRecipe(ExperimentalEffect);  // see if we have that recipe for backup name
                        ExperimentalEffect_Localised = recp?.Name ?? ExperimentalEffect.SplitCapsWordFull();
                    }
                    else
                        ExperimentalEffect_Localised = loc;

                    //System.Diagnostics.Debug.WriteLine($"Exp effect {ExperimentalEffect} loc {loc} recp {recp?.Name} = {ExperimentalEffect_Localised}");
                }

                Modifiers = evt["Modifiers"]?.ToObject<EngineeringModifiers[]>(true);     // instances of Value being wrong type - ignore and continue

                if (Modifiers != null)
                {
                    foreach (EngineeringModifiers v in Modifiers)
                        v.FriendlyLabel = v.Label.Replace("_", " ").SplitCapsWord();
                }
                else
                {

                }
            }

            FriendlyBlueprintName = BlueprintName.HasChars() ? Recipes.GetBetterNameForEngineeringRecipe(BlueprintName) : "??";       // some journal entries has empty blueprints
        }

        public JObject ToJSONLoadout()  // reproduce the loadout format..
        {
            var jo = new JObject();
            jo["Engineer"] = Engineer;
            jo["EngineerID"] = EngineerID;
            jo["BlueprintID"] = BlueprintID;
            jo["BlueprintName"] = BlueprintName;
            jo["Level"] = Level;
            jo["Quality"] = Quality;

            if (ExperimentalEffect.HasChars())      // not always present..
            {
                jo["ExperimentalEffect"] = ExperimentalEffect;
                jo["ExperimentalEffect_Localised"] = ExperimentalEffect_Localised;
            }

            if (Modifiers != null)
            {
                var modarray = new JArray();
                foreach (EngineeringModifiers m in Modifiers)
                {
                    JObject mod = new JObject();
                    mod["Label"] = m.Label;
                    if (m.ValueStr.HasChars())      // if set, its just a string value
                    {
                        mod["ValueStr"] = m.ValueStr;
                    }
                    else
                    {
                        mod["Value"] = m.Value;
                        mod["OriginalValue"] = m.OriginalValue;
                        mod["LessIsGood"] = m.LessIsGood ? 1 : 0;       // written 1/0 in file, not true/false.
                    }

                    modarray.Add(mod);
                }

                jo["Modifiers"] = modarray;
            }

            return jo;
        }

        public void Build(System.Text.StringBuilder sb)
        {
            sb.BuildSetPad(Environment.NewLine,
                    "Engineer:".T(EDCTx.EngineeringData_Engineer) + " ", Engineer,
                    "Blueprint:".T(EDCTx.EngineeringData_Blueprint) + " ", FriendlyBlueprintName,
                    "Level:".T(EDCTx.EngineeringData_Level) + " ", Level,
                    "Quality:".T(EDCTx.EngineeringData_Quality) + " ", Quality,
                    "Experimental Effect:".T(EDCTx.EngineeringData_ExperimentalEffect) + " ", ExperimentalEffect_Localised);

            if (ExperimentalEffect.HasChars())
            {
                if (specialeffects.TryGetValue(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
                {
                    foreach (var kvp in ItemData.ShipModule.GetPropertiesInOrder())     // all properties in the class
                    {
                        dynamic value = kvp.Key.GetValue(se);                       // if not null, we apply
                        if (value != null)
                        {
                            sb.AppendPrePad($"   {kvp.Key.Name}: {value}", Environment.NewLine);
                        }
                    }
                }
            }

            if (Modifiers != null)
            {
                sb.AppendCR();

                foreach (EngineeringModifiers m in Modifiers)
                {
                    if (m.ValueStr != null)
                    {
                        sb.Build("", m.Label, "<:", m.ValueStr_Localised ?? m.ValueStr ?? "Not set");
                    }
                    else
                    {
                        if (m.Value != m.OriginalValue)
                        {
                            bool better = m.LessIsGood ? (m.Value < m.OriginalValue) : (m.Value > m.OriginalValue);
                            double mul = m.Value / m.OriginalValue * 100 - 100;
                            sb.Build("", m.FriendlyLabel, "<: ;;0.###", m.Value, "Original: ;;0.###".T(EDCTx.EngineeringData_Original), m.OriginalValue, "Mult: ;%;N1", mul, "< (Worse); (Better)".T(EDCTx.EngineeringData_Worse), better);
                        }
                        else
                        {
                            sb.Build("", m.FriendlyLabel, "<: ;;0.###", m.Value);
                        }
                    }
                    sb.AppendCR();
                }
            }
        }

        public bool Same(EngineeringData other)
        {
            if (other == null || Engineer != other.Engineer || BlueprintName != other.BlueprintName || EngineerID != other.EngineerID || BlueprintID != other.BlueprintID
                || Level != other.Level || Quality != other.Quality || ExperimentalEffect != other.ExperimentalEffect || ExperimentalEffect_Localised != other.ExperimentalEffect_Localised)
            {
                return false;
            }
            else if (Modifiers != null || other.Modifiers != null)
            {
                if (Modifiers == null || other.Modifiers == null || Modifiers.Length != other.Modifiers.Length)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < Modifiers.LongLength; i++)
                    {
                        if (Modifiers[i].Label != other.Modifiers[i].Label || Modifiers[i].ValueStr != other.Modifiers[i].ValueStr ||
                            Modifiers[i].Value != other.Modifiers[i].Value || Modifiers[i].OriginalValue != other.Modifiers[i].OriginalValue || Modifiers[i].LessIsGood != other.Modifiers[i].LessIsGood)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public EngineeringModifiers FindModification(string name)
        {
            return Modifiers != null ? Array.Find(Modifiers, x => x.Label.Equals(name, StringComparison.InvariantCultureIgnoreCase)) : null;
        }

        public ItemData.ShipModule EngineerModule(ItemData.ShipModule original, out string report, string modulefdname = "", ShipSlots.Slot slotfd = ShipSlots.Slot.Unknown, bool debugit = false)
        {
            report = "";

            if (debugit)
                System.Diagnostics.Debug.WriteLine($"###### Engineer module {modulefdname} in {slotfd}");

            var engineered = new ItemData.ShipModule(original);       // take a copy

            List<System.Reflection.PropertyInfo> proplist = ItemData.ShipModule.GetPropertiesInOrder().Keys.ToList();        // list of engineering properties

            // list of primary modifiers in use from the Modifiers list..

            List<string> primarymodifiers = new List<string>();
            foreach( var x in Modifiers.EmptyIfNull())
            {
                if (modifierfdmapping.TryGetValue(x.Label, out string[] modifyarray) && modifyarray.Length>0)  // get the modifier primary control value if present
                    primarymodifiers.Add(modifyarray[0]);
            }

            // go thru modifiers
            foreach (EngineeringModifiers mf in Modifiers.EmptyIfNull())        // modifiers may be null
            {
                if (modifierfdmapping.TryGetValue(mf.Label, out string[] modifyarray))  // get the modify commands from the label
                {
                    if ( modifyarray.Length == 0 )
                    {
                        if (debugit)
                            System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName}, fd {mf.Label}: No variables associated with this FD property");
                        continue;
                    }

                    double ratio = 0;                                   // primary ratio

                    for (int pno = 0; pno < modifyarray.Length; pno++)        // for each modifier, 0 means primary
                    {
                        string pset = modifyarray[pno];                   // parameter, and optional set of cop outs
                        bool divit = pset.StartsWith("/");              // / means we divide not multiply the ratio when setting the para
                        if (divit)
                            pset = pset.Substring(1);
                        bool doubleit = pset.StartsWith("2");
                        if ( doubleit)
                            pset = pset.Substring(1);

                        string[] exceptiontypes = new string[0];        // split string into primary (pset) and exception list
                        int excl = pset.IndexOf('!');
                        if (excl > 0)
                        {
                            string ctrl = pset.Substring(excl + 1);
                            exceptiontypes = ctrl.SplitNoEmptyStartFinish('|');
                            pset = pset.Substring(0, excl);
                        }

                        // if we are a secondary, but we are changing a primary modified value, don't change

                        string debugpad = pno == 0 ? "" : "   ";

                        if (pno > 0 && primarymodifiers.Find(x => x == pset) != null)
                        {
                            if (debugit)
                                System.Diagnostics.Debug.WriteLine($"{debugpad}Engineer {original.EnglishModName}, fd {mf.Label}, para {pset}: NOT changing due to primary modifier being present");
                            continue;
                        }

                        // for all exception types listed against this, see if we have an exception

                        bool stop = false;
                        foreach (var exceptiontype in exceptiontypes)
                        {
                            bool negativecheck = exceptiontype[0] == '-';
                            string exceptiontext = exceptiontype.Substring(1);

                            bool anyfound = Array.Find(Modifiers, x => x.Label.EqualsIIC(exceptiontext)) != null ||
                                          modulefdname.WildCardMatch(exceptiontext, true) == true ||
                                          BlueprintName.EqualsIIC(exceptiontext);

                            if (negativecheck ? anyfound == true : anyfound == false)        // negative check means can't have any, position check means must have something
                            {
                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($"{debugpad}Engineer {original.EnglishModName}, fd {mf.Label}, para {pset}: NOT changing due to condition {exceptiontype}");
                                stop = true;
                                break;
                            }
                        }

                        if (stop)       // abandon if check failed
                            continue;

                        System.Reflection.PropertyInfo prop = proplist.Find(x => x.Name == pset);       // find parameter we are setting
                        dynamic orgvalue = prop.GetValue(original);

                        if (prop.PropertyType.FullName.Contains("System.String"))     // if its string, value is localised or valuestr.  We accept that a string could be null setting in the module data and just override it
                        {
                            string value = (mf.ValueStr_Localised ?? mf.ValueStr ?? "");
                            value = value.Replace("$INT_PANEL_module_", "").Replace(";", "").SplitCapsWordFull();
                            prop.SetValue(engineered, value);
                        }
                        else if (orgvalue != null)         // if its non null, we override it
                        {
                            double valuetoset;
                            double ratiotoapply = ratio;

                            if (pno == 0)           // primary modifier, we set it and record the ratio
                            {
                                valuetoset = mf.Value;
                                ratio = ratiotoapply = mf.Value / mf.OriginalValue;
                            }
                            else
                            {                       // secondary, we apply the ratio
                                if (doubleit)
                                    ratiotoapply = ((ratio - 1) * 2) + 1;      // take off the 1 to get scalar direct (say 0.21), then double it (0.42), and move back to 1. this is different to just doubling 1.21. Its a percentage double

                                if (divit)
                                    valuetoset = (double)orgvalue * (1 - (ratiotoapply - 1));   // apply using it as a percentage
                                else
                                    valuetoset = (double)orgvalue * ratiotoapply;
                            }

                            if (debugit)
                                System.Diagnostics.Debug.WriteLine($"{debugpad}Engineer {original.EnglishModName}, fd {mf.Label}, para {pset}: orgvalue {orgvalue} -> {valuetoset} ratio {ratiotoapply}");

                            if (orgvalue is double?)
                            {
                                prop.SetValue(engineered, valuetoset);
                            }
                            else if (orgvalue is int?)
                            {
                                prop.SetValue(engineered, (int)valuetoset);
                            }
                        }
                        else           // else it was null, so we moan gently, then set it
                        {                   
                            if (pset == "PowerDraw" && mf.Value == 0)       // this occurs for engineering a detailed surface scanner, the power draw 0->0, but it may be more than just this module, so generic catch
                            {
                                if ( debugit )
                                    System.Diagnostics.Debug.WriteLine($"*** Engineering setting a null value to zero, module {modulefdname} at {slotfd}, blueprint '{this.BlueprintName}' se '{this.ExperimentalEffect}' para '{pset}' ignoring it silently");
                            }
                            else
                            {
                                string msg = $"Engineering setting a null value in module {modulefdname} at {slotfd} blueprint '{this.BlueprintName}' se '{this.ExperimentalEffect}' para '{pset}' value {mf.Value}";
                                System.Diagnostics.Trace.WriteLine(msg);
                                report += msg + Environment.NewLine;

                                // don't need to do system.string as we accept nulls for it above
                                if (prop.PropertyType.FullName.Contains("System.Double"))
                                {
                                    prop.SetValue(engineered, mf.Value);
                                }
                                else if (prop.PropertyType.FullName.Contains("System.Int32"))
                                {
                                    prop.SetValue(engineered, (int)mf.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string msg = $"*** Engineering unknown modifier for module {modulefdname} at {slotfd}, blueprint '{this.BlueprintName}' se '{this.ExperimentalEffect}' para '{mf.Label}'";
                    System.Diagnostics.Trace.WriteLine(msg);
                    report += msg + Environment.NewLine;
                }
            }

            // now apply special effects

            if (ExperimentalEffect.HasChars())
            {
                if (specialeffects.TryGetValue(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
                {
                    foreach (var kvp in ItemData.ShipModule.GetPropertiesInOrder())     // all properties in the class
                    {
                        dynamic modificationvalue = kvp.Key.GetValue(se);
                        if (modificationvalue != null)
                        {
                            if (!primarymodifiers.Contains(kvp.Key.Name))        // if not null, and we have not set it above..
                            {
                                dynamic curvalue = kvp.Key.GetValue(original);        // get original value

                                if (!specialeffectmodcontrol.TryGetValue(kvp.Key.Name, out double controlmod))
                                    controlmod = 100;

                                dynamic nextvalue = controlmod == 0 ? modificationvalue : controlmod == 1 ? curvalue + modificationvalue : curvalue * (1 + modificationvalue / controlmod);

                                kvp.Key.SetValue(engineered, nextvalue);

                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($"SpecialEffect on {engineered.EnglishModName} SE {ExperimentalEffect} Property {kvp.Key.Name} adjust by {modificationvalue}: {curvalue} -> {nextvalue}");

                                if (kvp.Key.Name == "Damage")       // special code for Damage, do not apply if DPS is a primary 
                                {
                                    if (!primarymodifiers.Contains("DPS"))
                                    {
                                        curvalue = original.DPS;
                                        nextvalue = controlmod == 0 ? modificationvalue : controlmod == 1 ? curvalue + modificationvalue : curvalue * (1 + modificationvalue / controlmod);
                                        engineered.DPS = nextvalue;

                                        curvalue = original.BreachDamage;
                                        nextvalue = controlmod == 0 ? modificationvalue : controlmod == 1 ? curvalue + modificationvalue : curvalue * (1 + modificationvalue / controlmod);
                                        engineered.BreachDamage = nextvalue;
                                    }
                                }
                            }
                            else
                            {
                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($"SpecialEffect on {engineered.EnglishModName} SE {ExperimentalEffect} Property {kvp.Key.Name} not changing due to change above");
                            }
                        }

                    }
                }
                else
                {
                    string msg = $"*** Special effect in engineering not known {modulefdname} {BlueprintName} {ExperimentalEffect}";
                    System.Diagnostics.Trace.WriteLine(msg);
                    report += msg + Environment.NewLine;
                }
            }

            return engineered;
        }

        const string enginefastonly = "!+*_class5_fast";     // only for these do we have these engine parameters

        // key is the frontier label name
        // for the string[]
        // first entry must just be the name only and is the primary modifier
        // second and further entries:
        //      / means divide the primary ratio not multiply
        //      ! don't do if the exceptions stop the application. A list of exceptions,  | separated
        //          An exception is +/- <Engineering Variable>|<module name>|<blueprint name>.  - means it can't be true, + means it must be true

        static private Dictionary<string, string[]> modifierfdmapping = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            // multiple ones

            ["DamagePerSecond"] = new string[] { "DPS", "Damage!-Damage|-RateOfFire",     // change Damage as long as .. modifier labels are not there
                                                        "BreachDamage!-Damage|-RateOfFire",           // change BreachDamage as long as .. is not there
                                                },
            ["Damage"] = new string[] { "Damage", "BreachDamage",
                                                  "BurstInterval!+hpt_railgun*|+Weapon_HighCapacity",   // change burstinterval if module is railgun and recipe is High Capacity
                                                  // error "BurstInterval!+hpt_guardian_gausscannon*"   // change burstinterval if module is guass cannon
          
                                                },
            ["RateOfFire"] = new string[] { "RateOfFire", 
                                                   "/BurstInterval!-hpt_railgun*|-hpt_slugshot*",       // reduce by as long as not these types
                                                   "/2BurstInterval!+hpt_guardian_gausscannon*",       // double reduce if gauss cannon (this overrides above)
                                           },

            ["ShieldGenStrength"] = new string[] { "OptStrength", "MinStrength", "MaxStrength" },

            ["ShieldGenOptimalMass"] = new string[] { "OptMass", "MinMass" },

            ["EngineOptimalMass"] = new string[] { "OptMass", "MinMass", "MaxMass" },

            ["EngineOptPerformance"] = new string[] { "EngineOptMultiplier",
                                                                nameof(ItemData.ShipModule.EngineMinMultiplier) ,
                                                                nameof(ItemData.ShipModule.EngineMaxMultiplier),
                                                                nameof(ItemData.ShipModule.MinimumSpeedModifier)+ enginefastonly ,
                                                                nameof(ItemData.ShipModule.OptimalSpeedModifier)+ enginefastonly,
                                                                nameof(ItemData.ShipModule.MaximumSpeedModifier)+ enginefastonly,
                                                                nameof(ItemData.ShipModule.MinimumAccelerationModifier) + enginefastonly ,
                                                                nameof(ItemData.ShipModule.OptimalAccelerationModifier)+ enginefastonly,
                                                                nameof(ItemData.ShipModule.MaximumAccelerationModifier)+ enginefastonly,
                                                                nameof(ItemData.ShipModule.MinimumRotationModifier) + enginefastonly,
                                                                nameof(ItemData.ShipModule.OptimalRotationModifier)+ enginefastonly,
                                                                nameof(ItemData.ShipModule.MaximumRotationModifier)+ enginefastonly,
                                                    },
            ["Range"] = new string[] { "TypicalEmission", "Range", },

            // simples. Empty string[] means there is no equivalent engineering variable we know about..

            ["Mass"] = new string[] { nameof(ItemData.ShipModule.Mass), },
            ["Integrity"] = new string[] { nameof(ItemData.ShipModule.Integrity) },
            ["PowerDraw"] = new string[] { nameof(ItemData.ShipModule.PowerDraw) },
            ["BootTime"] = new string[] { nameof(ItemData.ShipModule.BootTime) },
            ["ShieldBankSpinUp"] = new string[] { nameof(ItemData.ShipModule.SCBSpinUp) },
            ["ShieldBankDuration"] = new string[] { nameof(ItemData.ShipModule.SCBDuration) },
            ["ShieldBankReinforcement"] = new string[] { nameof(ItemData.ShipModule.ShieldReinforcement) },
            ["ShieldBankHeat"] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            ["DistributorDraw"] = new string[] { nameof(ItemData.ShipModule.DistributorDraw) },
            ["ThermalLoad"] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            ["ArmourPenetration"] = new string[] { nameof(ItemData.ShipModule.ArmourPiercing) },
            ["MaximumRange"] = new string[] { nameof(ItemData.ShipModule.Range) },
            ["FalloffRange"] = new string[] { nameof(ItemData.ShipModule.Falloff) },
            ["ShotSpeed"] = new string[] { nameof(ItemData.ShipModule.Speed) },
            ["BurstRateOfFire"] = new string[] { nameof(ItemData.ShipModule.BurstRateOfFire) },
            ["BurstSize"] = new string[] { nameof(ItemData.ShipModule.BurstSize) },
            ["AmmoClipSize"] = new string[] { nameof(ItemData.ShipModule.Clip) },
            ["AmmoMaximum"] = new string[] { nameof(ItemData.ShipModule.Ammo) },
            ["RoundsPerShot"] = new string[] { nameof(ItemData.ShipModule.Rounds) },
            ["ReloadTime"] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            ["BreachDamage"] = new string[] { nameof(ItemData.ShipModule.BreachDamage) },
            ["BreachPercent"] = new string[] { nameof(ItemData.ShipModule.BreachModuleDamageAfterBreach), },
            ["MinBreachChance"] = new string[] { nameof(ItemData.ShipModule.BreachMin) },
            ["MaxBreachChance"] = new string[] { nameof(ItemData.ShipModule.BreachMax) },
            ["Jitter"] = new string[] { nameof(ItemData.ShipModule.Jitter) },
            ["WeaponMode"] = new string[] { },
            ["DamageType"] = new string[] { },
            ["ShieldGenMinimumMass"] = new string[] { nameof(ItemData.ShipModule.MinMass) },
            ["ShieldGenMaximumMass"] = new string[] { nameof(ItemData.ShipModule.MaxMass) },
            ["ShieldGenMinStrength"] = new string[] { nameof(ItemData.ShipModule.MinStrength) },
            ["ShieldGenMaxStrength"] = new string[] { nameof(ItemData.ShipModule.MaxStrength) },
            ["RegenRate"] = new string[] { nameof(ItemData.ShipModule.RegenRate) },
            ["BrokenRegenRate"] = new string[] { nameof(ItemData.ShipModule.BrokenRegenRate) },
            ["EnergyPerRegen"] = new string[] { nameof(ItemData.ShipModule.MWPerUnit) },
            ["FSDOptimalMass"] = new string[] { nameof(ItemData.ShipModule.OptMass) },
            ["FSDHeatRate"] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            ["MaxFuelPerJump"] = new string[] { nameof(ItemData.ShipModule.MaxFuelPerJump) },
            ["EngineMinimumMass"] = new string[] { nameof(ItemData.ShipModule.MinMass) },
            ["MaximumMass"] = new string[] { nameof(ItemData.ShipModule.MaxMass) },
            ["EngineMinPerformance"] = new string[] { nameof(ItemData.ShipModule.EngineMinMultiplier) },
            ["EngineMaxPerformance"] = new string[] { nameof(ItemData.ShipModule.EngineMaxMultiplier) },
            ["EngineHeatRate"] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            ["PowerCapacity"] = new string[] { nameof(ItemData.ShipModule.PowerGen) },
            ["HeatEfficiency"] = new string[] { nameof(ItemData.ShipModule.HeatEfficiency) },
            ["WeaponsCapacity"] = new string[] { nameof(ItemData.ShipModule.WeaponsCapacity) },
            ["WeaponsRecharge"] = new string[] { nameof(ItemData.ShipModule.WeaponsRechargeRate) },
            ["EnginesCapacity"] = new string[] { nameof(ItemData.ShipModule.EngineCapacity) },
            ["EnginesRecharge"] = new string[] { nameof(ItemData.ShipModule.EngineRechargeRate) },
            ["SystemsCapacity"] = new string[] { nameof(ItemData.ShipModule.SystemsCapacity) },
            ["SystemsRecharge"] = new string[] { nameof(ItemData.ShipModule.SystemsRechargeRate) },
            ["DefenceModifierHealthMultiplier"] = new string[] { nameof(ItemData.ShipModule.HullStrengthBonus) },
            ["DefenceModifierHealthAddition"] = new string[] { nameof(ItemData.ShipModule.HullReinforcement) },
            ["DefenceModifierShieldMultiplier"] = new string[] { nameof(ItemData.ShipModule.ShieldReinforcement) },
            ["DefenceModifierShieldAddition"] = new string[] { nameof(ItemData.ShipModule.AdditionalReinforcement) },
            ["CollisionResistance"] = new string[] { },
            ["KineticResistance"] = new string[] { nameof(ItemData.ShipModule.KineticResistance) },
            ["ThermicResistance"] = new string[] { nameof(ItemData.ShipModule.ThermalResistance) },
            ["ExplosiveResistance"] = new string[] { nameof(ItemData.ShipModule.ExplosiveResistance) },
            ["CausticResistance"] = new string[] { nameof(ItemData.ShipModule.CausticResistance) },
            ["FSDInterdictorRange"] = new string[] { nameof(ItemData.ShipModule.TargetMaxTime) },
            ["FSDInterdictorFacingLimit"] = new string[] { nameof(ItemData.ShipModule.Angle) },
            ["ScannerRange"] = new string[] { nameof(ItemData.ShipModule.Range) },
            ["DiscoveryScannerRange"] = new string[] { },
            ["DiscoveryScannerPassiveRange"] = new string[] { },
            ["MaxAngle"] = new string[] { nameof(ItemData.ShipModule.Angle) },
            ["ScannerTimeToScan"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["ChaffJamDuration"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["ECMRange"] = new string[] { nameof(ItemData.ShipModule.Range) },
            ["ECMTimeToCharge"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["ECMActivePowerConsumption"] = new string[] { nameof(ItemData.ShipModule.ActivePower) },
            ["ECMHeat"] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            ["ECMCooldown"] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            ["HeatSinkDuration"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["ThermalDrain"] = new string[] { nameof(ItemData.ShipModule.ThermalDrain) },
            ["NumBuggySlots"] = new string[] { nameof(ItemData.ShipModule.Capacity) },
            ["CargoCapacity"] = new string[] { nameof(ItemData.ShipModule.Size) },
            ["MaxActiveDrones"] = new string[] { nameof(ItemData.ShipModule.Limpets) },
            ["DroneTargetRange"] = new string[] { nameof(ItemData.ShipModule.TargetRange) },
            ["DroneLifeTime"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["DroneSpeed"] = new string[] { nameof(ItemData.ShipModule.Speed) },
            ["DroneMultiTargetSpeed"] = new string[] { nameof(ItemData.ShipModule.MultiTargetSpeed) },
            ["DroneFuelCapacity"] = new string[] { nameof(ItemData.ShipModule.FuelTransfer) },
            ["DroneRepairCapacity"] = new string[] { nameof(ItemData.ShipModule.MaxRepairMaterialCapacity) },
            ["DroneHackingTime"] = new string[] { nameof(ItemData.ShipModule.HackTime) },
            ["DroneMinJettisonedCargo"] = new string[] { nameof(ItemData.ShipModule.MinCargo) },
            ["DroneMaxJettisonedCargo"] = new string[] { nameof(ItemData.ShipModule.MaxCargo) },
            ["FuelScoopRate"] = new string[] { nameof(ItemData.ShipModule.RefillRate) },
            ["FuelCapacity"] = new string[] { nameof(ItemData.ShipModule.Size) },
            ["OxygenTimeCapacity"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["RefineryBins"] = new string[] { nameof(ItemData.ShipModule.Capacity) },
            ["AFMRepairCapacity"] = new string[] { nameof(ItemData.ShipModule.Ammo) },
            ["AFMRepairConsumption"] = new string[] { nameof(ItemData.ShipModule.RateOfRepairConsumption) },
            ["AFMRepairPerAmmo"] = new string[] { nameof(ItemData.ShipModule.RepairCostPerMat) },
            ["MaxRange"] = new string[] { nameof(ItemData.ShipModule.Range) },
            ["SensorTargetScanAngle"] = new string[] { nameof(ItemData.ShipModule.Angle) },
            ["VehicleCargoCapacity"] = new string[] { },
            ["VehicleHullMass"] = new string[] { },
            ["VehicleFuelCapacity"] = new string[] { },
            ["VehicleArmourHealth"] = new string[] { },
            ["VehicleShieldHealth"] = new string[] { },
            ["FighterMaxSpeed"] = new string[] { },
            ["FighterBoostSpeed"] = new string[] { },
            ["FighterPitchRate"] = new string[] { },
            ["FighterDPS"] = new string[] { },
            ["FighterYawRate"] = new string[] { },
            ["FighterRollRate"] = new string[] { },
            ["CabinCapacity"] = new string[] { nameof(ItemData.ShipModule.Passengers) },
            ["CabinClass"] = new string[] { nameof(ItemData.ShipModule.CabinClass) },
            ["DisruptionBarrierRange"] = new string[] { nameof(ItemData.ShipModule.Range) },
            ["DisruptionBarrierChargeDuration"] = new string[] { nameof(ItemData.ShipModule.Time) },
            ["DisruptionBarrierActivePower"] = new string[] { nameof(ItemData.ShipModule.MWPerSec) },
            ["DisruptionBarrierCooldown"] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            ["WingDamageReduction"] = new string[] { },
            ["WingMinDuration"] = new string[] { },
            ["WingMaxDuration"] = new string[] { },
            ["ShieldSacrificeAmountRemoved"] = new string[] { },
            ["ShieldSacrificeAmountGiven"] = new string[] { },
            ["FSDJumpRangeBoost"] = new string[] { nameof(ItemData.ShipModule.AdditionalRange) },
            ["FSDFuelUseIncrease"] = new string[] { },
            ["BoostSpeedMultiplier"] = new string[] { },
            ["BoostAugmenterPowerUse"] = new string[] { },
            ["ModuleDefenceAbsorption"] = new string[] { nameof(ItemData.ShipModule.Protection) },
            ["DSS_RangeMult"] = new string[] { },
            ["DSS_AngleMult"] = new string[] { },
            ["DSS_RateMult"] = new string[] { },
            ["DSS_PatchRadius"] = new string[] { nameof(ItemData.ShipModule.ProbeRadius) },

            ["BurstRate"] = new string[] { nameof(ItemData.ShipModule.BurstRateOfFire) },
            ["BurstSize"] = new string[] { nameof(ItemData.ShipModule.BurstSize) },
            ["DamageFalloffRange"] = new string[] { nameof(ItemData.ShipModule.Falloff) },

            ["GuardianModuleResistance"] = new string[] { nameof(ItemData.ShipModule.GuardianModuleResistance) },       // add in edsy aug 24 version. String, Active or ""
        };

        static private Dictionary<string, ItemData.ShipModule> specialeffects = new Dictionary<string, ItemData.ShipModule>
        {
            ["special_auto_loader"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Auto reload while firing") { },
            ["special_concordant_sequence"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Wing shield regen increased") { ThermalLoad = 50 },
            ["special_corrosive_shell"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target armor hardness reduced") { Ammo = -20 },
            ["special_blinding_shell"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target sensor acuity reduced") { },
            ["special_dispersal_field"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target gimbal/turret tracking reduced") { },
            ["special_weapon_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_drag_munitions"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target speed reduced") { },
            ["special_emissive_munitions"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target signature increased") { ThermalLoad = 100 },
            ["special_feedback_cascade_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target shield cell disrupted") { Damage = -20, ThermalLoad = -40 },
            ["special_weapon_efficient"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            ["special_force_shell"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target pushed off course") { Speed = -16.666666666666671 },
            ["special_fsd_interrupt"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target FSD reboots") { Damage = -30, BurstInterval = 50 },
            ["special_high_yield_shell"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { Damage = -35, BurstInterval = 11.111111111111111, KineticProportionDamage = 50, ExplosiveProportionDamage = 50 },
            ["special_incendiary_rounds"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { BurstInterval = 5.2631578947368416, ThermalLoad = 200, KineticProportionDamage = 10, ThermalProportionDamage = 90 },
            ["special_distortion_field"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Damage = 50, KineticProportionDamage = 50, ThermalProportionDamage = 50, Jitter = 3 },
            ["special_choke_canister"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target thrusters reboot") { },
            ["special_mass_lock"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target FSD inhibited") { },
            ["special_weapon_rateoffire"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, BurstInterval = -2.9126213592233 },
            ["special_overload_munitions"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ThermalProportionDamage = 50, ExplosiveProportionDamage = 50 },
            ["special_weapon_damage"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, Damage = 3 },
            ["special_penetrator_munitions"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { },
            ["special_deep_cut_payload"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { },
            ["special_phasing_sequence"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "10% of damage bypasses shields") { Damage = -10 },
            ["special_plasma_slug"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Reload from ship fuel") { Damage = -10, Ammo = -100 },
            ["special_plasma_slug_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Reload from ship fuel") { Damage = -10, ThermalLoad = -40, Ammo = -100 },
            ["special_radiant_canister"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Area heat increased and sensors disrupted") { },
            ["special_regeneration_sequence"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target wing shields regenerated") { Damage = -10 },
            ["special_reverberating_cascade"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target shield generator damaged") { },
            ["special_scramble_spectrum"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target modules malfunction") { BurstInterval = 11.111111111111111 },
            ["special_screening_shell"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Effective against munitions") { ReloadTime = -50 },
            ["special_shiftlock_canister"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Area FSDs reboot") { },
            ["special_smart_rounds"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "No damage to untargeted ships") { },
            ["special_weapon_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_super_penetrator_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { ThermalLoad = -40, ReloadTime = 50 },
            ["special_lock_breaker"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target loses target lock") { },
            ["special_thermal_cascade"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Shielded target heat increased") { },
            ["special_thermal_conduit"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Damage increases with heat level") { },
            ["special_thermalshock"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target heat increased") { },
            ["special_thermal_vent"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Heat reduced when striking a target") { },
            ["special_shieldbooster_explosive"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, ExplosiveResistance = 2 },
            ["special_shieldbooster_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_shieldbooster_efficient"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            ["special_shieldbooster_kinetic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, KineticResistance = 2 },
            ["special_shieldbooster_chunky"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = 5, KineticResistance = -2, ThermalResistance = -2, ExplosiveResistance = -2 },
            ["special_shieldbooster_thermic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, ThermalResistance = 2 },
            ["special_armour_kinetic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, KineticResistance = 8 },
            ["special_armour_chunky"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = 8, KineticResistance = -3, ThermalResistance = -3, ExplosiveResistance = -3 },
            ["special_armour_explosive"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, ExplosiveResistance = 8 },
            ["special_armour_thermic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, ThermalResistance = 8 },
            ["special_powerplant_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_powerplant_highcharge"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = 10, PowerGen = 5 },
            ["special_powerplant_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_powerplant_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HeatEfficiency = -10 },
            ["special_engine_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_engine_overloaded"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { EngineOptMultiplier = 4, ThermalLoad = 10 },
            ["special_engine_haulage"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptMass = 10 },
            ["special_engine_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_engine_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = 5, ThermalLoad = -10 },
            ["special_fsd_fuelcapacity"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, MaxFuelPerJump = 10 },
            ["special_fsd_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 25 },
            ["special_fsd_heavy"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = -8, OptMass = 4 },
            ["special_fsd_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_fsd_cooled"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ThermalLoad = -10 },
            ["special_powerdistributor_capacity"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { WeaponsCapacity = 8, WeaponsRechargeRate = -2, EngineCapacity = 8, EngineRechargeRate = -2, SystemsCapacity = 8, SystemsRechargeRate = -2 },
            ["special_powerdistributor_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_powerdistributor_efficient"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            ["special_powerdistributor_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_powerdistributor_fast"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { WeaponsCapacity = -4, WeaponsRechargeRate = 4, EngineCapacity = -4, EngineRechargeRate = 4, SystemsCapacity = -4, SystemsRechargeRate = 4 },
            ["special_hullreinforcement_kinetic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, KineticResistance = 2 },
            ["special_hullreinforcement_chunky"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = 10, KineticResistance = -2, ThermalResistance = -2, ExplosiveResistance = -2 },
            ["special_hullreinforcement_explosive"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, ExplosiveResistance = 2 },
            ["special_hullreinforcement_thermic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, ThermalResistance = 2 },
            ["special_shieldcell_oversized"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { SCBSpinUp = 20, ShieldReinforcement = 5 },
            ["special_shieldcell_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_shieldcell_efficient"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            ["special_shieldcell_gradual"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { SCBDuration = 10, ShieldReinforcement = -5 },
            ["special_shieldcell_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_shield_toughened"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            ["special_shield_regenerative"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { RegenRate = 15, BrokenRegenRate = 15, KineticResistance = -1.5, ThermalResistance = -1.5, ExplosiveResistance = -1.5 },
            ["special_shield_kinetic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptStrength = -3, KineticResistance = 8 },
            ["special_shield_health"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 10, OptStrength = 6, MWPerUnit = 25 },
            ["special_shield_efficient"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -20, OptStrength = -2, MWPerUnit = -20, KineticResistance = -1, ThermalResistance = -1, ExplosiveResistance = -1 },
            ["special_shield_resistive"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 10, MWPerUnit = 25, KineticResistance = 3, ThermalResistance = 3, ExplosiveResistance = 3 },
            ["special_shield_lightweight"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            ["special_shield_thermic"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptStrength = -3, ThermalResistance = 8 },

            // added older no longer supported ones
            ["special_feedback_cascade"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { },
            ["special_super_penetrator"] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { },

        };

        // for special effects, what to do..
        // 0 = set, 1 = add, 2 means mod 100 on primary value, else its modmod together in %

        static private Dictionary<string, double> specialeffectmodcontrol = new Dictionary<string, double>
        {
            ["BurstRateOfFire"] = 0,
            ["BurstSize"] = 0,
            ["Rounds"] = 1,
            ["Jitter"] = 1,
            ["KineticProportionDamage"] = 0,
            ["ThermalProportionDamage"] = 0,
            ["ExplosiveProportionDamage"] = 0,
            ["AbsoluteProportionDamage"] = 0,
            ["CausticPorportionDamage"] = 0,
            ["AXPorportionDamage"] = 0,
            ["HullStrengthBonus"] = 100,
            ["ShieldReinforcement"] = 100,
            ["KineticResistance"] = -100,
            ["ThermalResistance"] = -100,
            ["ExplosiveResistance"] = -100,
            ["CausticResistance"] = -100,
            ["AXResistance"] = -100,
            ["Capacity"] = 1,
            ["Limpets"] = 1,
            ["MinCargo"] = 1,
            ["MaxCargo"] = 1,
            ["Capacity"] = 1,
        };


    }

}
