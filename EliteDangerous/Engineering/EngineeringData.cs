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

    [System.Diagnostics.DebuggerDisplay("{Engineer] {BlueprintName} {Level} {ExperimentalEffect]")]
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

        // Post engineering changes
        public EngineeringData(JObject evt)
        {
            Engineer = evt["Engineer"].Str();
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
                    var recp = Recipes.FindRecipe(ExperimentalEffect);  // see if we have that recipe for backup name
                    // seen records with localised=experimental effect so protect that.
                    ExperimentalEffect_Localised = JournalFieldNaming.CheckLocalisation(!loc.EqualsIIC(ExperimentalEffect) ? loc : null, recp?.Name ?? ExperimentalEffect.SplitCapsWordFull());
                    //System.Diagnostics.Debug.WriteLine($"Exp effect {ExperimentalEffect} loc {loc} recp {recp?.Name} = {ExperimentalEffect_Localised}");
                }

                Modifiers = evt["Modifiers"]?.ToObject<EngineeringModifiers[]>(ignoretypeerrors: true, checkcustomattr: false);     // instances of Value being wrong type - ignore and continue

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

        public override string ToString()
        {
            string ret = BaseUtils.FieldBuilder.BuildSetPad(Environment.NewLine,
                    "Engineer:".T(EDCTx.EngineeringData_Engineer) + " ", Engineer,
                    "Blueprint:".T(EDCTx.EngineeringData_Blueprint) + " ", FriendlyBlueprintName,
                    "Level:".T(EDCTx.EngineeringData_Level) + " ", Level,
                    "Quality:".T(EDCTx.EngineeringData_Quality) + " ", Quality,
                    "Experimental Effect:".T(EDCTx.EngineeringData_ExperimentalEffect) + " ", ExperimentalEffect_Localised);

            if (ExperimentalEffect.HasChars())
            {
                if (ItemData.TryGetSpecialEffect(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
                {
                    foreach (var kvp in ItemData.ShipModule.GetPropertiesInOrder())     // all properties in the class
                    {
                        dynamic value = kvp.Key.GetValue(se);                       // if not null, we apply
                        if (value != null)
                        {
                            ret = ret.AppendPrePad($"   {kvp.Key.Name}: {value}", Environment.NewLine);
                        }
                    }
                }
            }

            if (Modifiers != null)
            {
                ret += Environment.NewLine;

                foreach (EngineeringModifiers m in Modifiers)
                {
                    if (m.ValueStr != null)
                        ret += BaseUtils.FieldBuilder.Build("", m.Label, "<:", m.ValueStr) + Environment.NewLine;
                    else
                    {
                        if (m.Value != m.OriginalValue)
                        {
                            bool better = m.LessIsGood ? (m.Value < m.OriginalValue) : (m.Value > m.OriginalValue);
                            double mul = m.Value / m.OriginalValue * 100 - 100;
                            ret += BaseUtils.FieldBuilder.Build("", m.FriendlyLabel,"<: ;;0.###", m.Value, "Original: ;;0.###".T(EDCTx.EngineeringData_Original), m.OriginalValue, "Mult: ;%;N1", mul , "< (Worse); (Better)".T(EDCTx.EngineeringData_Worse), better) + Environment.NewLine;
                        }
                        else
                            ret += BaseUtils.FieldBuilder.Build("", m.FriendlyLabel, "<: ;;0.###", m.Value) + Environment.NewLine;
                    }
                }
            }

            return ret;
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

        public ItemData.ShipModule EngineerModule(ItemData.ShipModule original, string modulefdname = "", ShipSlots.Slot slotfd = ShipSlots.Slot.Unknown, bool debugit = false)
        {
            if (debugit)
                System.Diagnostics.Debug.WriteLine($"*** Engineer module {modulefdname} in {slotfd}");

            var engineered = new ItemData.ShipModule(original);       // take a copy

            List<System.Reflection.PropertyInfo> proplist = ItemData.ShipModule.GetPropertiesInOrder().Keys.ToList();        // list of engineering properties

            // list of primary modifiers in use from the Modifiers list..

            List<string> primarymodifiers = new List<string>();
            foreach( var x in Modifiers)
            {
                if (modifierfdmapping.TryGetValue(x.Label, out string[] modifyarray) && modifyarray.Length>0)  // get the modifier primary control value if present
                    primarymodifiers.Add(modifyarray[0]);
            }

            // go thru modifiers
            foreach (EngineeringModifiers mf in Modifiers)      
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

                        if (orgvalue != null)         // it may be null, because it may not be there..  thats a failure
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
                        else
                        {
                            if (pset == "PowerDraw" && mf.Value == 0)       // this occurs for engineering a detailed surface scanner, the power draw 0->0, but it may be more than just this module, so generic catch
                            {
                                if ( debugit )
                                    System.Diagnostics.Debug.WriteLine($"*** Engineering setting a null value to zero {modulefdname} {this.BlueprintName} {this.ExperimentalEffect} {pset} ignoring it silently");
                            }
                            else
                            {
                                System.Diagnostics.Trace.WriteLine($"*** Engineering setting a null value in module {modulefdname} at {slotfd} blueprint '{this.BlueprintName}' se '{this.ExperimentalEffect}' para '{pset}' value {mf.Value}");
                                
                                if ( prop.PropertyType.FullName.Contains("System.Double"))
                                {
                                    prop.SetValue(engineered, mf.Value);
                                }
                                else
                                    prop.SetValue(engineered, (int)mf.Value);
                            }
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"*** Engineering unknown modifier {modulefdname} {this.BlueprintName} {this.ExperimentalEffect} {mf.Label}");
                }
            }

            // now apply special effects

            if (ExperimentalEffect.HasChars())
            {
                if (ItemData.TryGetSpecialEffect(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
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

                                if (kvp.Key.Name == "Damage")
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
                    System.Diagnostics.Trace.WriteLine($"*** Special effect in engineering not known {modulefdname} {BlueprintName} {ExperimentalEffect}");
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

        Dictionary<string, string[]> modifierfdmapping = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
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
        };

        // for special effects, what to do..
        // 0 = set, 1 = add, 2 means mod 100 on primary value, else its modmod together in %

        Dictionary<string, double> specialeffectmodcontrol = new Dictionary<string, double>
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
