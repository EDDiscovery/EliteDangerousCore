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

        public ItemData.ShipModule EngineerModule(string modulefdname, ItemData.ShipModule original)
        {
            //System.Diagnostics.Debug.WriteLine($"*** Engineer module {fdname} ");

            var engineered = new ItemData.ShipModule(original);       // take a copy

            List<System.Reflection.PropertyInfo> proplist = ItemData.ShipModule.GetPropertiesInOrder().Keys.ToList();        // list of engineering properties

            // list of primary modifiers in use from the Modifiers list..

            List<string> primarymodifiers = new List<string>();
            foreach( var x in Modifiers)
            {
                if (modifierfdmapping.TryGetValue(x.Label, out string[] modifyarray))  // get the modifier primary control value if present
                    primarymodifiers.Add(modifyarray[0]);
            }

            // go thru modifiers
            foreach (EngineeringModifiers mf in Modifiers)      
            {
                if (modifierfdmapping.TryGetValue(mf.Label, out string[] modifyarray))  // get the modify commands from the label
                {
                    double ratio = 0;                                   // primary ratio

                    for (int pno = 0; pno < modifyarray.Length; pno++)        // for each modifier, 0 means primary
                    {
                        string pset = modifyarray[pno];                   // parameter, and optional set of cop outs
                        bool divit = pset.StartsWith("/");              // / means we divide not multiply the ratio when setting the para
                        if (divit)
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

                        if (pno > 0 && primarymodifiers.Find(x => x == pset) != null)
                        {
                            //  System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} {mf.Label} {pset} NOT changing to {valuetoset} due to primary modifier being present");
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
                                //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} {mf.Label} {pset} NOT changing due to condition {exceptiontype}");
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
                            if (pno == 0)           // primary modifier, we set it and record the ratio
                            {
                                valuetoset = mf.Value;
                                ratio = mf.Value / mf.OriginalValue;
                            }
                            else
                            {                       // secondary, we apply the ratio
                                if (divit)
                                    valuetoset = (double)orgvalue / ratio;
                                else
                                    valuetoset = (double)orgvalue * ratio;
                            }

                            //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} {mf.Label} {pset} {orgvalue} -> {valuetoset} ratio {ratio}");

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
                            System.Diagnostics.Trace.WriteLine($"*** Engineering setting a null value {this.BlueprintName} {this.ExperimentalEffect} {pset}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"*** Engineering unknown modifier {this.BlueprintName} {this.ExperimentalEffect} {mf.Label}");
                }
            }

            // now apply special effects

            if (ExperimentalEffect.HasChars())
            {
                if (ItemData.TryGetSpecialEffect(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
                {
                    foreach (var kvp in ItemData.ShipModule.GetPropertiesInOrder())     // all properties in the class
                    {
                        dynamic value = kvp.Key.GetValue(se);
                        if (value != null)
                        {
                            if (!primarymodifiers.Contains(kvp.Key.Name))        // if not null, and we have not set it above..
                            {
                                dynamic curvalue = kvp.Key.GetValue(original);        // get original value

                                if (!specialeffectmodcontrol.TryGetValue(kvp.Key.Name, out double controlmod))
                                    controlmod = 100;

                                dynamic nextvalue = controlmod == 0 ? value : controlmod == 1 ? curvalue + value : curvalue * (1 + value / controlmod);

                                kvp.Key.SetValue(engineered, nextvalue);

                                //System.Diagnostics.Debug.WriteLine($"SpecialEffect on {engineered.EnglishModName} SE {ExperimentalEffect} Property {kvp.Key.Name} adjust by {value}: {curvalue} -> {nextvalue}");
                            }
                            else
                            {
                                //  System.Diagnostics.Debug.WriteLine($"SpecialEffect on {engineered.EnglishModName} SE {ExperimentalEffect} Property {kvp.Key.Name} not changing due to change above");
                            }
                        }

                    }
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"*** Special effect in engineering not known {ExperimentalEffect}");
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
            ["DamagePerSecond"] = new string[] { "DPS", "Damage!-Damage|-RateOfFire",     // change Damage as long as .. modifier labels are not there
                                                        "BreachDamage!-Damage|-RateOfFire",           // change BreachDamage as long as .. is not there
                                                },
            ["Damage"] = new string[] { "Damage", "BreachDamage",
                                                  "BurstInterval!+hpt_railgun*|+Weapon_HighCapacity"   // change burstinterval if module is railgun and recipe is High Capacity
          
                                                },
            ["RateOfFire"] = new string[] { "RateOfFire", "/BurstInterval!-hpt_railgun*" },

            ["Mass"] = new string[] { "Mass", },
            ["Integrity"] = new string[] { "Integrity", },
            ["Integrity"] = new string[] { "Integrity", },
            ["PowerDraw"] = new string[] { "PowerDraw", },
            ["BootTime"] = new string[] { "BootTime", },
            ["ShieldBankSpinUp"] = new string[] { "SCBSpinUp", },
            ["ShieldBankDuration"] = new string[] { "SCBDuration", },
            ["ShieldBankReinforcement"] = new string[] { "ShieldReinforcement", },
            ["ShieldBankHeat"] = new string[] { "ThermalLoad", },
            ["DistributorDraw"] = new string[] { "DistributorDraw", },
            ["ThermalLoad"] = new string[] { "ThermalLoad", },
            ["ArmourPenetration"] = new string[] { "ArmourPiercing", },
            ["MaximumRange"] = new string[] { "Range" },
            ["FalloffRange"] = new string[] { "Falloff", },
            ["ShotSpeed"] = new string[] { "Speed", },
            ["BurstRateOfFire"] = new string[] { "BurstRateOfFire", },
            ["BurstSize"] = new string[] { "BurstSize", },
            ["AmmoClipSize"] = new string[] { "Clip", },
            ["AmmoMaximum"] = new string[] { "Ammo", },
            ["RoundsPerShot"] = new string[] { "Rounds", },
            ["ReloadTime"] = new string[] { "ReloadTime", },
            ["BreachDamage"] = new string[] { "BreachDamage", },
            ["MinBreachChance"] = new string[] { "BreachMin", },
            ["MaxBreachChance"] = new string[] { "BreachMax", },
            ["Jitter"] = new string[] { "Jitter", },

            ["ShieldGenMinimumMass"] = new string[] { "MinMass", },
            ["ShieldGenOptimalMass"] = new string[] { "OptMass", "MinMass" },
            ["ShieldGenMaximumMass"] = new string[] { "MaxMass", },

            ["ShieldGenMinStrength"] = new string[] { "MinStrength" },
            ["ShieldGenStrength"] = new string[] { "OptStrength", "MinStrength", "MaxStrength" },
            ["ShieldGenMaxStrength"] = new string[] { "MaxStrength" },

            ["RegenRate"] = new string[] { "RegenRate", },
            ["BrokenRegenRate"] = new string[] { "BrokenRegenRate", },
            ["EnergyPerRegen"] = new string[] { "MWPerUnit", },
            ["FSDOptimalMass"] = new string[] { "OptMass", },
            ["FSDHeatRate"] = new string[] { "ThermalLoad", },
            ["MaxFuelPerJump"] = new string[] { "MaxFuelPerJump", },
            ["EngineMinimumMass"] = new string[] { "MinMass", },
            ["EngineOptimalMass"] = new string[] { "OptMass", "MinMass", "MaxMass" },
            ["MaximumMass"] = new string[] { "MaxMass", },
            ["EngineMinPerformance"] = new string[] { "EngineMinMultiplier", },
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
            ["EngineMaxPerformance"] = new string[] { "EngineMaxMultiplier", },
            ["EngineHeatRate"] = new string[] { "ThermalLoad", },
            ["PowerCapacity"] = new string[] { "PowerGen", },
            ["HeatEfficiency"] = new string[] { "HeatEfficiency", },
            ["WeaponsCapacity"] = new string[] { "WeaponsCapacity", },
            ["WeaponsRecharge"] = new string[] { "WeaponsRechargeRate", },
            ["EnginesCapacity"] = new string[] { "EngineCapacity", },
            ["EnginesRecharge"] = new string[] { "EngineRechargeRate", },
            ["SystemsCapacity"] = new string[] { "SystemsCapacity", },
            ["SystemsRecharge"] = new string[] { "SystemsRechargeRate", },
            ["DefenceModifierHealthMultiplier"] = new string[] { "HullStrengthBonus", },
            ["DefenceModifierHealthAddition"] = new string[] { "HullReinforcement", },
            ["DefenceModifierShieldMultiplier"] = new string[] { "ShieldReinforcement", },
            ["DefenceModifierShieldAddition"] = new string[] { "AdditionalReinforcement", },
            ["KineticResistance"] = new string[] { "KineticResistance", },
            ["ThermicResistance"] = new string[] { "ThermalResistance", },
            ["ExplosiveResistance"] = new string[] { "ExplosiveResistance", },
            ["CausticResistance"] = new string[] { "CausticResistance", },
            ["FSDInterdictorRange"] = new string[] { "TargetMaxTime", },
            ["FSDInterdictorFacingLimit"] = new string[] { "Angle", },
            ["ScannerRange"] = new string[] { "Range", },
            ["MaxAngle"] = new string[] { "Angle", },
            ["ScannerTimeToScan"] = new string[] { "Time", },
            ["ChaffJamDuration"] = new string[] { "Time", },
            ["ECMRange"] = new string[] { "Range", },
            ["ECMTimeToCharge"] = new string[] { "Time", },
            ["ECMActivePowerConsumption"] = new string[] { "ActivePower", },
            ["ECMHeat"] = new string[] { "WasteHeat", },
            ["ECMCooldown"] = new string[] { "ReloadTime", },
            ["HeatSinkDuration"] = new string[] { "Time", },
            ["ThermalDrain"] = new string[] { "WasteHeat", },
            ["NumBuggySlots"] = new string[] { "Capacity", },
            ["CargoCapacity"] = new string[] { "Size", },
            ["MaxActiveDrones"] = new string[] { "Limpets", },
            ["DroneTargetRange"] = new string[] { "TargetRange", },
            ["DroneLifeTime"] = new string[] { "Time", },
            ["DroneSpeed"] = new string[] { "Speed", },
            ["DroneMultiTargetSpeed"] = new string[] { "MultiTargetSpeed", },
            ["DroneFuelCapacity"] = new string[] { "FuelTransfer", },
            ["DroneRepairCapacity"] = new string[] { "MaxRepairMaterialCapacity", },
            ["DroneHackingTime"] = new string[] { "HackTime", },
            ["DroneMinJettisonedCargo"] = new string[] { "MinCargo", },
            ["DroneMaxJettisonedCargo"] = new string[] { "MaxCargo", },
            ["FuelScoopRate"] = new string[] { "RefillRate", },
            ["FuelCapacity"] = new string[] { "Size", },
            ["OxygenTimeCapacity"] = new string[] { "Time", },
            ["RefineryBins"] = new string[] { "Capacity", },
            ["AFMRepairCapacity"] = new string[] { "Ammo", },
            ["AFMRepairConsumption"] = new string[] { "RateOfRepairConsumption", },
            ["AFMRepairPerAmmo"] = new string[] { "RepairCostPerMat", },
            ["MaxRange"] = new string[] { "Range", },
            ["SensorTargetScanAngle"] = new string[] { "Angle", },
            ["Range"] = new string[] { "TypicalEmission", "Range" },
            ["CabinCapacity"] = new string[] { "Passengers", },
            ["CabinClass"] = new string[] { "CabinClass", },
            ["DisruptionBarrierRange"] = new string[] { "Range", },
            ["DisruptionBarrierChargeDuration"] = new string[] { "Time", },
            ["DisruptionBarrierActivePower"] = new string[] { "MWPerSec", },
            ["DisruptionBarrierCooldown"] = new string[] { "ReloadTime", },
            ["FSDJumpRangeBoost"] = new string[] { "AdditionalRange", },
            ["ModuleDefenceAbsorption"] = new string[] { "Protection", },
            ["DSS_PatchRadius"] = new string[] { "ProbeRadius", },
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
