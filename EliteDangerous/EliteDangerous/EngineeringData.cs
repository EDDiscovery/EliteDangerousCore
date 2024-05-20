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
                            ret += BaseUtils.FieldBuilder.Build("", m.FriendlyLabel, "(;)", m.Label, "<: ;;N2", m.Value, "Original: ;;N2".T(EDCTx.EngineeringData_Original), m.OriginalValue, "< (Worse); (Better)".T(EDCTx.EngineeringData_Worse), better) + Environment.NewLine;
                        }
                        else
                            ret += BaseUtils.FieldBuilder.Build("", m.FriendlyLabel, "(;)", m.Label, "<: ;;N2", m.Value) + Environment.NewLine;
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
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Mass} -> {engineered.Mass}");
                    }
                    else if (mf.Label.EqualsIIC("Integrity"))
                    {
                        engineered.Integrity = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Integrity} -> {engineered.Integrity}");
                    }
                    else if (mf.Label.EqualsIIC("PowerDraw"))       // seen in edsy output as direct value
                    {
                        engineered.Power = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Power} -> {engineered.Power}");
                    }
                    else if (mf.Label.EqualsIIC("BootTime"))
                    {
                        engineered.BootTime = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BootTime} -> {engineered.BootTime}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankSpinUp"))
                    {
                        engineered.SCBSpinUp = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBSpinUp} -> {engineered.SCBSpinUp}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankDuration"))
                    {
                        engineered.SCBDuration = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBDuration} -> {engineered.SCBDuration}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankReinforcement"))
                    {
                        engineered.ShieldReinforcement = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ShieldReinforcement} -> {engineered.ShieldReinforcement}");
                    }
                    else if (mf.Label.EqualsIIC("ShieldBankHeat"))
                    {
                        engineered.SCBHeat = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.SCBHeat} -> {engineered.SCBHeat}");
                    }
                    else if (mf.Label.EqualsIIC("DamagePerSecond")) // seen in edsy output as direct value
                    {
                        engineered.DPS = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.DPS} -> {engineered.DPS}");
                    }
                    else if (mf.Label.EqualsIIC("Damage"))      // seen in edsy output as direct value
                    {
                        engineered.Damage = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Damage} -> {engineered.Damage}");
                    }
                    else if (mf.Label.EqualsIIC("DistributorDraw")) // seen in edsy output as direct value
                    {
                        engineered.DistributorDraw = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.DistributorDraw} -> {engineered.DistributorDraw}");
                    }
                    else if (mf.Label.EqualsIIC("ThermalLoad")) // seen in edsy output as direct value
                    {
                        engineered.ThermL = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermL} -> {engineered.ThermL}");
                    }
                    else if (mf.Label.EqualsIIC("ArmourPenetration"))
                    {
                        engineered.Pierce = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Pierce} -> {engineered.Pierce}");
                    }
                    else if (mf.Label.EqualsIIC("MaximumRange")) 
                    {
                        engineered.Range = (int)mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Range} -> {engineered.Range}");
                    }
                    else if (mf.Label.EqualsIIC("FalloffRange")) 
                    {
                        engineered.Falloff = (int)mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Falloff} -> {engineered.Falloff}");
                    }
                    else if (mf.Label.EqualsIIC("ShotSpeed"))
                    {
                        engineered.Speed = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Speed} -> {engineered.Speed}");
                    }
                    else if (mf.Label.EqualsIIC("RateOfFire"))
                    {
                        engineered.RateOfFire = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.RateOfFire} -> {engineered.RateOfFire}");
                    }

                    else if (mf.Label.EqualsIIC("BurstRateOfFire"))
                    {
                        engineered.BurstRateOfFire = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BurstRateOfFire} -> {engineered.BurstRateOfFire}");
                    }

                    else if (mf.Label.EqualsIIC("BurstSize")) // seen in edsy output as direct value
                    {
                        engineered.BurstSize = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BurstSize} -> {engineered.BurstSize}");
                    }
                    else if (mf.Label.EqualsIIC("AmmoClipSize"))
                    {
                        engineered.Clip = (int)mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Clip} -> {engineered.Clip}");
                    }
                    else if (mf.Label.EqualsIIC("AmmoMaximum"))
                    {
                        engineered.Ammo = (int)mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Ammo} -> {engineered.Ammo}");
                    }
                    else if (mf.Label.EqualsIIC("RoundsPerShot"))
                    {
                        engineered.Rounds = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Rounds} -> {engineered.Rounds}");
                    }
                    else if (mf.Label.EqualsIIC("ReloadTime")) 
                    {
                        engineered.ReloadTime = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ReloadTime} -> {engineered.ReloadTime}");
                    }
                    else if (mf.Label.EqualsIIC("BreachDamage"))
                    {
                        engineered.BreachDamage = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachDamage} -> {engineered.BreachDamage}");
                    }
                    else if (mf.Label.EqualsIIC("MinBreachChance"))
                    {
                        engineered.BreachMin = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachMin} -> {engineered.BreachMin}");
                    }
                    else if (mf.Label.EqualsIIC("MaxBreachChance"))
                    {
                        engineered.BreachMax = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.BreachMax} -> {engineered.BreachMax}");
                    }
                    else if (mf.Label.EqualsIIC("Jitter"))
                    {
                        engineered.Jitter = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Jitter} -> {engineered.Jitter}");
                    }
                    // ? DamageType



                    //else if (mf.Label.EqualsIIC("")) 
                    //{
                    //    engineered. = mf.Value;
                    //    System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.ThermL} -> {engineered.ThermL}");
                    //}


                    else if (mf.Label.EqualsIIC("DefenceModifierHealthMultiplier"))
                    {
                        engineered.HullStrengthBonus = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.HullStrengthBonus} -> {engineered.HullStrengthBonus}");
                    }
                    else if (mf.Label.EqualsIIC("KineticResistance"))
                    {
                        engineered.Kinetic = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Kinetic} -> {engineered.Kinetic}");

                    }
                    else if (mf.Label.EqualsIIC("ThermicResistance"))
                    {
                        engineered.Thermal = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Thermal} -> {engineered.Thermal}");

                    }
                    else if (mf.Label.EqualsIIC("ExplosiveResistance"))
                    {
                        engineered.Explosive = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.Explosive} -> {engineered.Explosive}");
                    }
                    else if (mf.Label.EqualsIIC("FSDOptimalMass"))
                    {
                        engineered.OptMass = (int)mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.OptMass} -> {engineered.OptMass}");
                    }
                    else if (mf.Label.EqualsIIC("MaxFuelPerJump"))
                    {
                        engineered.MaxFuelPerJump = mf.Value;
                        System.Diagnostics.Debug.WriteLine($"Engineer {original.EnglishModName} with {BlueprintName}: {mf.Label} {original.MaxFuelPerJump} -> {engineered.MaxFuelPerJump}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to engineer {original.EnglishModName} with {BlueprintName} due to {mf.Label}");
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


        // the whole thruster thing needs work tbd - to be removed
        public bool EngineerThrusters(ref double speed)
        {
            EngineeringModifiers mod = FindModification("EngineOptPerformance");
            if (mod != null)
            {
                speed *= mod.Value / 100.0;
                return true;
            }
            else
                return false;
        }

    }

}
