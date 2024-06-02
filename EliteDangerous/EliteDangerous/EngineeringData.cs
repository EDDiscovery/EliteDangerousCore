/*
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

namespace EliteDangerousCore
{
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

    public class EngineeringData
    {
        public string Engineer { get; set; }
        public string BlueprintName { get; set; }       // not case corrected - as inara gets it, best to leave its case.
        public string FriendlyBlueprintName { get; set; }
        public ulong EngineerID { get; set; }
        public ulong BlueprintID { get; set; }
        public int Level { get; set; }
        public double Quality { get; set; }
        public string ExperimentalEffect { get; set; }      // may be null
        public string ExperimentalEffect_Localised { get; set; }    // may be null
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
                ExperimentalEffect = evt.MultiStr(new string[] { "ExperimentalEffect", "ApplyExperimentalEffect" });
                ExperimentalEffect_Localised = JournalFieldNaming.CheckLocalisation(evt["ExperimentalEffect_Localised"].Str(), ExperimentalEffect);

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
                            ret += BaseUtils.FieldBuilder.Build("", m.FriendlyLabel,"<: ;;0.###", m.Value, "Original: ;;0.###".T(EDCTx.EngineeringData_Original), m.OriginalValue, "< (Worse); (Better)".T(EDCTx.EngineeringData_Worse), better) + Environment.NewLine;
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

        // take a module and engineer it with the modifiers
        // false if we don't know how to use all the modifiers given, the ones we do know have been modified
        public bool EngineerModule(ItemData.ShipModule original, out ItemData.ShipModule engineered)
        {
            engineered = new ItemData.ShipModule(original);       // take a copy

            bool good = true;

            foreach (var mf in Modifiers.EmptyIfNull())
            {
                try
                {
                    // in the order of eddb.js attributes

                    if (mf.Label.EqualsIIC("Mass"))
                    {
                        engineered.Mass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Mass} -> {engineered.Mass}");
                    }
                    else if (mf.Label.EqualsIIC("Integrity"))
                    {
                        engineered.Integrity = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Integrity} -> {engineered.Integrity}");
                    }
                    else if (mf.Label.EqualsIIC("PowerDraw"))       // seen in edsy output as direct value
                    {
                        engineered.Power = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Power} -> {engineered.Power}");
                    }
                    else if (mf.Label.EqualsIIC("BootTime"))
                    {
                        engineered.BootTime = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BootTime} -> {engineered.BootTime}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankSpinUp"))
                    {
                        engineered.SCBSpinUp = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBSpinUp} -> {engineered.SCBSpinUp}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankDuration"))
                    {
                        engineered.SCBDuration = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBDuration} -> {engineered.SCBDuration}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankReinforcement"))
                    {
                        engineered.ShieldReinforcement = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ShieldReinforcement} -> {engineered.ShieldReinforcement}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankHeat"))
                    {
                        engineered.SCBHeat = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBHeat} -> {engineered.SCBHeat}");
                    }
                    else if (mf.Label.EqualsIIC("DamagePerSecond")) // seen in edsy output as direct value
                    {
                        engineered.DPS = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.DPS} -> {engineered.DPS}");
                    }
                    else if (mf.Label.EqualsIIC("Damage"))      // seen in edsy output as direct value
                    {
                        engineered.Damage = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Damage} -> {engineered.Damage}");
                    }
                    else if (mf.Label.EqualsIIC("DistributorDraw")) // seen in edsy output as direct value
                    {
                        engineered.DistributorDraw = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.DistributorDraw} -> {engineered.DistributorDraw}");
                    }
                    else if (mf.Label.EqualsIIC("ThermalLoad")) // seen in edsy output as direct value
                    {
                        engineered.ThermL = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermL} -> {engineered.ThermL}");
                    }
                    else if (mf.Label.EqualsIIC("ArmourPenetration"))
                    {
                        engineered.Pierce = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Pierce} -> {engineered.Pierce}");
                    }
                    else if (mf.Label.EqualsIIC("MaximumRange")) 
                    {
                        engineered.Range = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Range} -> {engineered.Range}");
                    }
                    else if (mf.Label.EqualsIIC("FalloffRange")) 
                    {
                        engineered.Falloff = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Falloff} -> {engineered.Falloff}");
                    }
                    else if (mf.Label.EqualsIIC("ShotSpeed") || mf.Label.EqualsIIC("DroneSpeed"))   // shotspd maxspd
                    {
                        engineered.Speed = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Speed} -> {engineered.Speed}");
                    }
                    else if (mf.Label.EqualsIIC("RateOfFire"))
                    {
                        engineered.RateOfFire = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RateOfFire} -> {engineered.RateOfFire}");
                    }

                    else if (mf.Label.EqualsIIC("BurstRateOfFire"))
                    {
                        engineered.BurstRateOfFire = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BurstRateOfFire} -> {engineered.BurstRateOfFire}");
                    }

                    else if (mf.Label.EqualsIIC("BurstSize")) // seen in edsy output as direct value
                    {
                        engineered.BurstSize = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BurstSize} -> {engineered.BurstSize}");
                    }
                    else if (mf.Label.EqualsIIC("AmmoClipSize"))
                    {
                        engineered.Clip = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Clip} -> {engineered.Clip}");
                    }
                    else if (mf.Label.EqualsIIC("AmmoMaximum") || mf.Label.EqualsIIC("AFMRepairCapacity"))
                    {
                        engineered.Ammo = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Ammo} -> {engineered.Ammo}");
                    }
                    else if (mf.Label.EqualsIIC("RoundsPerShot"))
                    {
                        engineered.Rounds = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Rounds} -> {engineered.Rounds}");
                    }
                    else if (mf.Label.EqualsIIC("ReloadTime") 
                        || mf.Label.EqualsIIC("DisruptionBarrierCooldown")// .. barriercool
                        || mf.Label.EqualsIIC("ECMCooldown") //ecmcool
                        )   
                    {
                        engineered.ReloadTime = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ReloadTime} -> {engineered.ReloadTime}");
                    }
                    else if (mf.Label.EqualsIIC("BreachDamage"))
                    {
                        engineered.BreachDamage = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachDamage} -> {engineered.BreachDamage}");
                    }
                    else if (mf.Label.EqualsIIC("MinBreachChance"))
                    {
                        engineered.BreachMin = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachMin} -> {engineered.BreachMin}");
                    }
                    else if (mf.Label.EqualsIIC("MaxBreachChance"))
                    {
                        engineered.BreachMax = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachMax} -> {engineered.BreachMax}");
                    }
                    else if (mf.Label.EqualsIIC("Jitter"))
                    {
                        engineered.Jitter = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Jitter} -> {engineered.Jitter}");
                    }
                    // ? DamageTyp
                    // ? weaponMode


                    else if (mf.Label.EqualsIIC("ShieldGenMinimumMass"))    // genminmass
                    {
                        engineered.MinMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MinMass} -> {engineered.MinMass}");
                    }

                    else if (mf.Label.EqualsIIC("ShieldGenOptimalMass"))    // genoptmass
                    {
                        engineered.OptMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.OptMass} -> {engineered.OptMass}");
                    }

                    else if (mf.Label.EqualsIIC("ShieldGenMaximumMass"))// genmaxmass
                    {
                        engineered.MaxMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxMass} -> {engineered.MaxMass}");
                    }

                    else if (mf.Label.EqualsIIC("ShieldGenMinStrength"))            // genminmul
                    {
                        engineered.MinStrength = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MinStrength} -> {engineered.MinStrength}");
                    }

                    else if (mf.Label.EqualsIIC("ShieldGenStrength"))   // genoptmul
                    {
                        engineered.OptStrength = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.OptStrength} -> {engineered.OptStrength}");
                    }

                    else if (mf.Label.EqualsIIC("ShieldGenMaxStrength")) // genmaxmul
                    {
                        engineered.MaxStrength = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxStrength} -> {engineered.MaxStrength}");
                    }

                    else if (mf.Label.EqualsIIC("RegenRate"))   // genrate
                    {
                        engineered.RegenRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RegenRate} -> {engineered.RegenRate}");
                    }

                    else if (mf.Label.EqualsIIC("BrokenRegenRate")) // bgenrate
                    {
                        engineered.BrokenRegenRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BrokenRegenRate} -> {engineered.BrokenRegenRate}");
                    }

                    else if (mf.Label.EqualsIIC("EnergyPerRegen")) // genpwr
                    {
                        engineered.MWPerUnit = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MWPerUnit} -> {engineered.MWPerUnit}");
                    }

                    else if (mf.Label.EqualsIIC("FSDOptimalMass")) // fsdoptmass
                    {
                        engineered.OptMass = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.OptMass} -> {engineered.OptMass}");
                    }

                    else if (mf.Label.EqualsIIC("FSDHeatRate")) // fsdheat
                    {
                        engineered.ThermL = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermL} -> {engineered.ThermL}");
                    }

                    else if (mf.Label.EqualsIIC("MaxFuelPerJump")) // maxfuel
                    {
                        engineered.MaxFuelPerJump = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxFuelPerJump} -> {engineered.MaxFuelPerJump}");
                    }

                    else if (mf.Label.EqualsIIC("EngineMinimumMass")) // engminmass
                    {
                        engineered.MinMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MinMass} -> {engineered.MinMass}");
                    }

                    else if (mf.Label.EqualsIIC("EngineOptimalMass")) // engoptmass
                    {
                        engineered.OptMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.OptMass} -> {engineered.OptMass}");
                    }

                    else if (mf.Label.EqualsIIC("MaximumMass")) // engmaxmass
                    {
                        engineered.MaxMass = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxMass} -> {engineered.MaxMass}");
                    }

                    else if (mf.Label.EqualsIIC("EngineMinPerformance"))    // engminmul
                    {
                        engineered.EngineMinMultiplier = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.EngineMinMultiplier} -> {engineered.EngineMinMultiplier}");
                    }

                    else if (mf.Label.EqualsIIC("EngineOptPerformance"))    // engoptmul
                    {
                        engineered.EngineOptMultiplier = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.EngineOptMultiplier} -> {engineered.EngineOptMultiplier}");
                    }

                    else if (mf.Label.EqualsIIC("EngineMaxPerformance"))    // engmaxmul
                    {
                        engineered.EngineMaxMultiplier = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.EngineMaxMultiplier} -> {engineered.EngineMaxMultiplier}");
                    }

                    else if (mf.Label.EqualsIIC("EngineHeatRate"))  // engheat
                    {
                        engineered.ThermL = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermL} -> {engineered.ThermL}");
                    }

                    else if (mf.Label.EqualsIIC("PowerCapacity"))   // pwrcap
                    {
                        engineered.PowerGen = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.PowerGen} -> {engineered.PowerGen}");
                    }

                    else if (mf.Label.EqualsIIC("HeatEfficiency"))  // heategg
                    {
                        engineered.HeatEfficiency = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.HeatEfficiency} -> {engineered.HeatEfficiency}");
                    }

                    else if (mf.Label.EqualsIIC("WeaponsCapacity")) // wepcap WeaponCapacity
                    {
                        engineered.WeaponsCapacity = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.WeaponsCapacity} -> {engineered.WeaponsCapacity}");
                    }

                    else if (mf.Label.EqualsIIC("WeaponsRecharge")) //wepchg WeaponRechargeRate
                    {
                        engineered.WeaponsRechargeRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.WeaponsRechargeRate} -> {engineered.WeaponsRechargeRate}");
                    }

                    else if (mf.Label.EqualsIIC("EnginesCapacity")) // engcap EngineCapacity
                    {
                        engineered.EngineCapacity = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.EngineCapacity} -> {engineered.EngineCapacity}");
                    }

                    else if (mf.Label.EqualsIIC("EnginesRecharge"))   // engchg EngineRechargeRate
                    {
                        engineered.EngineRechargeRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.EngineRechargeRate} -> {engineered.EngineRechargeRate}");
                    }

                    else if (mf.Label.EqualsIIC("SystemsCapacity"))   // sysCap SystemsCapacity
                    {
                        engineered.SystemsCapacity = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SystemsCapacity} -> {engineered.SystemsCapacity}");
                    }

                    else if (mf.Label.EqualsIIC("SystemsRecharge"))   // syschg SystemsMW
                    {
                        engineered.SystemsRechargeRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SystemsRechargeRate} -> {engineered.SystemsRechargeRate}");
                    }

                    else if (mf.Label.EqualsIIC("DefenceModifierHealthMultiplier")) // hullbst = Hull Boost - this would affect the ships hull boost value = HullStrengthBonus = armour
                    {
                        engineered.HullStrengthBonus = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.HullStrengthBonus} -> {engineered.HullStrengthBonus}");
                    }

                    else if (mf.Label.EqualsIIC("DefenceModifierHealthAddition")) // hullrnf direct - hull reinforcements = HullReinforcement
                    {
                        engineered.HullReinforcement = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.HullReinforcement} -> {engineered.HullReinforcement}");
                    }

                    else if (mf.Label.EqualsIIC("DefenceModifierShieldMultiplier")) // shieldbst = Shield Boost = shield booster  = ShieldReinforcement
                    {
                        engineered.ShieldReinforcement = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ShieldReinforcement} -> {engineered.ShieldReinforcement}");
                    }

                    else if (mf.Label.EqualsIIC("DefenceModifierShieldAddition")) // shieldrnf = guardian shield boost = direct = AdditionalStrength
                    {
                        engineered.AdditionalReinforcement = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.AdditionalReinforcement} -> {engineered.AdditionalReinforcement}");
                    }


                    else if (mf.Label.EqualsIIC("CollisionResistance")) //absres - not seemingly used
                    {
                        //System.Diagnostics.Debug.WriteLine($"*** Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} ");
                    }

                    else if (mf.Label.EqualsIIC("KineticResistance"))   // kinres
                    {
                        engineered.KineticResistance = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.KineticResistance} -> {engineered.KineticResistance}");
                    }


                    else if (mf.Label.EqualsIIC("ThermicResistance"))       //thmres
                    {
                        engineered.ThermalResistance = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermalResistance} -> {engineered.ThermalResistance}");
                    }


                    else if (mf.Label.EqualsIIC("ExplosiveResistance"))     // expres
                    {
                        engineered.ExplosiveResistance = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ExplosiveResistance} -> {engineered.ExplosiveResistance}");
                    }


                    else if (mf.Label.EqualsIIC("CausticResistance"))       // caures
                    {
                        engineered.CausticResistance = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.CausticResistance} -> {engineered.CausticResistance}");
                    }


                    else if (mf.Label.EqualsIIC("FSDInterdictorRange")) // timerng
                    {
                        engineered.TargetMaxTime = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.TargetMaxTime} -> {engineered.TargetMaxTime}");
                    }


                    else if (mf.Label.EqualsIIC("FSDInterdictorFacingLimit")    // facinglim
                                || mf.Label.EqualsIIC("SensorTargetScanAngle")) // scanangle
                    {
                        engineered.Angle = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Angle} -> {engineered.Angle}");
                    }


                    else if (mf.Label.EqualsIIC("ScannerRange") || mf.Label.EqualsIIC("MaxAngle") || mf.Label.EqualsIIC("ECMRange")    // scanrng, maxangle, ecmrng
                                || mf.Label.EqualsIIC("DisruptionBarrierRange") // barrierrng
                                || mf.Label.EqualsIIC("MaxRange") ) // maxrng
                    {
                        engineered.Range = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Range} -> {engineered.Range}");
                    }


                    else if (mf.Label.EqualsIIC("ScannerTimeToScan") || mf.Label.EqualsIIC("ChaffJamDuration") || mf.Label.EqualsIIC("EMCTimeToCharge") || // scantime, jamdur, ecmdur
                            mf.Label.EqualsIIC("HeatSinkDuration") ||  // hsdur
                            mf.Label.EqualsIIC("OxygenTimeCapacity") || // emgcylife
                            mf.Label.EqualsIIC("DisruptionBarrierChangeDuration") || // barrierdur
                            mf.Label.EqualsIIC("DroneLifeTime")    // limpettime 
                            )
                    {
                        engineered.Time = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Time} -> {engineered.Time}");
                    }


                    else if (mf.Label.EqualsIIC("ECMActivePowerConsumption"))   // ecmpwr
                    {
                        engineered.ActivePower = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ActivePower} -> {engineered.ActivePower}");
                    }
                    else if (mf.Label.EqualsIIC("ECMHeat") || mf.Label.EqualsIIC("ThermalDrain" )) //ecmheat, thmdrain
                    {
                        engineered.WasteHeat = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.WasteHeat} -> {engineered.WasteHeat}");
                    }
                    else if (mf.Label.EqualsIIC("NumBuggySlots") || // vslots
                                mf.Label.EqualsIIC("RefineryBins"))    // bins
                    {
                        engineered.Capacity = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Capacity} -> {engineered.Capacity}");
                    }
                    else if (mf.Label.EqualsIIC("CargoCapacity")    //cargocap
                                || mf.Label.EqualsIIC("FuelCapacity"))    // fuelcap)
                    {
                        engineered.Size = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Size} -> {engineered.Size}");
                    }
                    else if (mf.Label.EqualsIIC("MaxActiveDrones"))   // maxlimpet
                    {
                        engineered.Limpets= (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Limpets} -> {engineered.Limpets}");
                    }
                    else if (mf.Label.EqualsIIC("DroneTargetRange")) // targetrng
                    {
                        engineered.TargetRange = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.TargetRange} -> {engineered.TargetRange}");
                    }
                    else if (mf.Label.EqualsIIC("DroneMultiTargetSpeed"))   // multispd
                    {
                        engineered.MultiTargetSpeed = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MultiTargetSpeed} -> {engineered.MultiTargetSpeed}");
                    }
                    else if (mf.Label.EqualsIIC("DroneFuelCapacity"))   // fuelxfer
                    {
                        engineered.FuelTransfer = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.FuelTransfer} -> {engineered.FuelTransfer}");
                    }
                    else if (mf.Label.EqualsIIC("DroneRepairCapacity")) // lmprepcap
                    {
                        engineered.MaxRepairMaterialCapacity = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxRepairMaterialCapacity} -> {engineered.MaxRepairMaterialCapacity}");
                    }
                    else if (mf.Label.EqualsIIC("DroneHackingTime"))    // hacktime
                    {
                        engineered.HackTime = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.HackTime} -> {engineered.HackTime}");
                    }
                    else if (mf.Label.EqualsIIC("DroneMinJettisonedCargo")) // mincargo
                    {
                        engineered.MinCargo = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MinCargo} -> {engineered.MinCargo}");
                    }
                    else if (mf.Label.EqualsIIC("DroneMaxJettisonedCargo")) // maxcargo
                    {
                        engineered.MaxCargo = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxCargo} -> {engineered.MaxCargo}");
                    }
                    else if (mf.Label.EqualsIIC("FuelScoopRate"))   // scooprate
                    {
                        engineered.RefillRate = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RefillRate} -> {engineered.RefillRate}");
                    }
                    else if (mf.Label.EqualsIIC("AFMRepairConsumption"))    // afmrepcap
                    {
                        engineered.RateOfRepairConsumption = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RateOfRepairConsumption} -> {engineered.RateOfRepairConsumption}");
                    }
                    else if (mf.Label.EqualsIIC("AFMRepairPerAmmo"))    // repairrtg
                    {
                        engineered.RepairCostPerMat = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RepairCostPerMat} -> {engineered.RepairCostPerMat}");
                    }
                    else if (mf.Label.EqualsIIC("Range"))   // typemis (its right mapping)
                    {
                        engineered.TypicalEmission = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.TypicalEmission} -> {engineered.TypicalEmission}");
                    }

                    else if (mf.Label.EqualsIIC("CabinCapacity"))   // cabincap
                    {
                        engineered.Passengers = (int)mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Passengers} -> {engineered.Passengers}");
                    }

                    // CabinClass not implemented as its not in a recipe

                    else if (mf.Label.EqualsIIC("DisruptionBarrierActivePower"))    // barrierpwr
                    {
                        engineered.MWPerSec = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MWPerSec} -> {engineered.MWPerSec}");
                    }
                    else if (mf.Label.EqualsIIC("FSDJumpRangeBoost"))
                    {
                        engineered.AdditionalRange = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.AdditionalRange} -> {engineered.AdditionalRange}");
                    }
                    else if (mf.Label.EqualsIIC("ModuleDefenceAbsorption"))
                    {
                        engineered.Protection = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Protection} -> {engineered.Protection}");
                    }
                    else if (mf.Label.EqualsIIC("DSS_PatchRadius")) // proberad
                    {
                        engineered.ProbeRadius = mf.Value;
                        //System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ProbeRadius} -> {engineered.ProbeRadius}");
                    }
                    else
                    {
                        //System.Diagnostics.Trace.WriteLine($"*** Failed to engineer {original.EnglishModName} with {BlueprintName} due to {mf.Label}");
                        good = false;
                    }
                }
                catch( Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"*** Engineering Craft failed {original.EnglishModName} with {BlueprintName} due to {mf.Label} : {ex} ");
                    good = false;
                }
            }

            return good;
        }

    }

}
