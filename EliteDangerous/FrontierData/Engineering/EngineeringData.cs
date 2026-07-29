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
        public ModLabelFDName Label { get; set; }               // identifier, matched with itemdata values
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
        public EngineerFDName Engineer { get; set; }
        public EngineeringRecipeFDName BlueprintName { get; set; }       
        public string FriendlyBlueprintName { get; set; }
        public ulong EngineerID { get; set; }
        public ulong BlueprintID { get; set; }
        public int Level { get; set; }
        public double Quality { get; set; }
        public EngineeringRecipeFDName ExperimentalEffect { get; set; }      // may be null or maybe empty (due to frontier) 
        public string FriendlyExperimentalEffect { get; set; }      // may be null or maybe empty (due to frontier) 
        public string ExperimentalEffect_Localised { get; set; }    // may be null or maybe empty (due to frontier)
        public EngineeringModifiers[] Modifiers { get; set; }       // may be null
        public bool IsValid { get { return Level >= 1 && BlueprintName.IsValid; } }

        // Post engineering changes.  visible moan turns on off complaining about blueprint/effect misses
        public EngineeringData(JObject evt, JournalEntry ev)
        {
            Engineer = new EngineerFDName(evt["Engineer"].Str());
            Level = evt["Level"].Int();

            if (evt.Contains("Blueprint"))     // old form
            {
                // old pre 3.0 form, don't moan about the recipies
                BlueprintName = EngineeringRecipeFDName.Normalise(evt["Blueprint"].Str(), out string engname, ev);
                FriendlyBlueprintName = engname;
            }
            else
            {
                EngineerID = evt["EngineerID"].ULong();     // NEW FORM after engineering changes in about 2018
                BlueprintID = evt["BlueprintID"].ULong();
                Quality = evt["Quality"].Double(0);

                BlueprintName = EngineeringRecipeFDName.Normalise(evt["BlueprintName"].Str(), out string engname, ev);
                FriendlyBlueprintName = engname;

                // EngineerCraft has it as Apply.. Loadout has just ExperimentalEffect.  Check both
                string effect = evt.MultiStr(new string[] { "ExperimentalEffect", "ApplyExperimentalEffect" }, null);

                if (effect.HasChars())
                {
                    ExperimentalEffect = EngineeringRecipeFDName.Normalise(effect, out engname, ev);
                    FriendlyExperimentalEffect = engname;
                    ExperimentalEffect_Localised = JournalFieldNaming.CheckLocalisation(evt["ExperimentalEffect_Localised"].Str(), engname);
                }

                Modifiers = evt["Modifiers"]?.ToObject<EngineeringModifiers[]>(true);     // instances of Value being wrong type - ignore and continue

                if (Modifiers != null)
                {
                    foreach (EngineeringModifiers v in Modifiers)
                        v.FriendlyLabel = v.Label.SplitCapsWordFull();
                }
            }
        }

        public JObject ToJSONLoadout()  // reproduce the loadout format..
        {
            var jo = new JObject();
            jo["Engineer"] = Engineer.Str();
            jo["EngineerID"] = EngineerID;
            jo["BlueprintID"] = BlueprintID;
            jo["BlueprintName"] = BlueprintName.Str();
            jo["Level"] = Level;
            jo["Quality"] = Quality;

            if (ExperimentalEffect != null)      // not always present..
            {
                jo["ExperimentalEffect"] = ExperimentalEffect.Str();
                jo["ExperimentalEffect_Localised"] = ExperimentalEffect_Localised;
            }

            if (Modifiers != null)
            {
                var modarray = new JArray();
                foreach (EngineeringModifiers m in Modifiers)
                {
                    JObject mod = new JObject();
                    mod["Label"] = m.Label.Str();
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
                    "Engineer".Tx()+": "+ " ", Engineer.Str(),
                    "Blueprint".Tx()+": "+ " ", FriendlyBlueprintName,
                    "Level".Tx()+": "+ " ", Level,
                    "Quality".Tx()+": "+ " ", Quality,
                    "Experimental Effect".Tx()+": "+ " ", ExperimentalEffect_Localised);

            if (ExperimentalEffect != null)
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
                        sb.Build("", m.FriendlyLabel, "<:", m.ValueStr_Localised ?? m.ValueStr ?? "Not set");
                    }
                    else
                    {
                        if (m.Value != m.OriginalValue)
                        {
                            bool better = m.LessIsGood ? (m.Value < m.OriginalValue) : (m.Value > m.OriginalValue);
                            double mul = m.Value / m.OriginalValue * 100 - 100;
                            sb.Build("", m.FriendlyLabel, "<: ;;0.###", m.Value, "Original: ;;0.###".Tx(), m.OriginalValue, "Mult: ;%;N1", mul, "< (Worse); (Better)".Tx(), better);
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

        public EngineeringModifiers FindModification(ModFDName name)
        {
            return Modifiers != null ? Array.Find(Modifiers, x => x.Label == name) : null;
        }

        public ItemData.ShipModule EngineerModule(ItemData.ShipModule original, out string report, ModFDName modulefdname, ShipSlots.Slot slotfd = ShipSlots.Slot.Unknown, bool debugit = false)
        {
            report = "";

            if (debugit)
                System.Diagnostics.Debug.WriteLine($"###### Engineer module {modulefdname} in {slotfd}");

            var engineered = new ItemData.ShipModule(original);       // take a copy

            List<System.Reflection.PropertyInfo> proplist = ItemData.ShipModule.GetPropertiesInOrder().Keys.ToList();        // list of engineering properties

            // list of primary modifiers in use from the Modifiers list..

            List<string> primarymodifiers = new List<string>();
            foreach( EngineeringModifiers em in Modifiers.EmptyIfNull())
            {
                if (modifierfdmapping.TryGetValue(em.Label, out string[] modifyarray) && modifyarray.Length>0)  // get the modifier primary control value if present
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

                            bool anyfound = Array.Find(Modifiers, x => x.Label.Str().EqualsIIC(exceptiontext)) != null ||
                                          modulefdname.Str().WildCardMatch(exceptiontext, true) == true ||
                                          BlueprintName.Str().EqualsIIC(exceptiontext);

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
                                    BaseUtils.Debugger.TraceBreak($"*** Engineering setting a null value to zero, module {modulefdname} at {slotfd}, blueprint '{this.BlueprintName}' se '{this.ExperimentalEffect}' para '{pset}' ignoring it silently");
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

            if (ExperimentalEffect != null)
            {
                if (specialeffects.TryGetValue(ExperimentalEffect, out ItemData.ShipModule se))   // get the experimental effect ship module modifier
                {
                    foreach (var kvp in ItemData.ShipModule.GetPropertiesInOrder())     // all properties in the class
                    {
                        // keys of propertyinfo/OrderedPropertyNameAttribute
                        dynamic modificationvalue = kvp.Key.GetValue(se);       // get original value

                        if (modificationvalue != null && kvp.Key.CanWrite)      // if non null, and settable..  and can write (may not, may be a get only item)
                        {
                            // and we have not modified it directly above

                            if (!primarymodifiers.Contains(kvp.Key.Name))        // if not null, and we have not set it above..
                            {
                                dynamic curvalue = kvp.Key.GetValue(original);        // get original value

                                if (!specialeffectmodcontrol.TryGetValue(new ModLabelFDName(kvp.Key.Name), out double controlmod))
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

        static private Dictionary<ModLabelFDName, string[]> modifierfdmapping = new Dictionary<ModLabelFDName, string[]>()
        {
            // multiple ones

            [new ModLabelFDName("DamagePerSecond")] = new string[] { "DPS", "Damage!-Damage|-RateOfFire",     // change Damage as long as .. modifier labels are not there
                                                        "BreachDamage!-Damage|-RateOfFire",           // change BreachDamage as long as .. is not there
                                                },
            [new ModLabelFDName("Damage")] = new string[] { "Damage", "BreachDamage",
                                                  "BurstInterval!+hpt_railgun*|+Weapon_HighCapacity",   // change burstinterval if module is railgun and recipe is High Capacity
                                                  // error "BurstInterval!+hpt_guardian_gausscannon*"   // change burstinterval if module is guass cannon
          
                                                },
            [new ModLabelFDName("RateOfFire")] = new string[] { "RateOfFire",
                                                   "/BurstInterval!-hpt_railgun*|-hpt_slugshot*",       // reduce by as long as not these types
                                                   "/2BurstInterval!+hpt_guardian_gausscannon*",       // double reduce if gauss cannon (this overrides above)
                                           },

            [new ModLabelFDName("ShieldGenStrength")] = new string[] { "OptStrength", "MinStrength", "MaxStrength" },

            [new ModLabelFDName("ShieldGenOptimalMass")] = new string[] { "OptMass", "MinMass" },

            [new ModLabelFDName("EngineOptimalMass")] = new string[] { "OptMass", "MinMass", "MaxMass" },

            [new ModLabelFDName("EngineOptPerformance")] = new string[] { "EngineOptMultiplier",
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
            [new ModLabelFDName("Range")] = new string[] { "TypicalEmission", "Range", },

            // simples. Empty string[] means there is no equivalent engineering variable we know about..

            [new ModLabelFDName("Mass")] = new string[] { nameof(ItemData.ShipModule.Mass), },
            [new ModLabelFDName("Integrity")] = new string[] { nameof(ItemData.ShipModule.Integrity) },
            [new ModLabelFDName("PowerDraw")] = new string[] { nameof(ItemData.ShipModule.PowerDraw) },
            [new ModLabelFDName("BootTime")] = new string[] { nameof(ItemData.ShipModule.BootTime) },
            [new ModLabelFDName("ShieldBankSpinUp")] = new string[] { nameof(ItemData.ShipModule.SCBSpinUp) },
            [new ModLabelFDName("ShieldBankDuration")] = new string[] { nameof(ItemData.ShipModule.SCBDuration) },
            [new ModLabelFDName("ShieldBankReinforcement")] = new string[] { nameof(ItemData.ShipModule.ShieldReinforcement) },
            [new ModLabelFDName("ShieldBankHeat")] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            [new ModLabelFDName("DistributorDraw")] = new string[] { nameof(ItemData.ShipModule.DistributorDraw) },
            [new ModLabelFDName("ThermalLoad")] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            [new ModLabelFDName("ArmourPenetration")] = new string[] { nameof(ItemData.ShipModule.ArmourPiercing) },
            [new ModLabelFDName("MaximumRange")] = new string[] { nameof(ItemData.ShipModule.Range) },
            [new ModLabelFDName("FalloffRange")] = new string[] { nameof(ItemData.ShipModule.Falloff) },
            [new ModLabelFDName("ShotSpeed")] = new string[] { nameof(ItemData.ShipModule.Speed) },
            [new ModLabelFDName("BurstRateOfFire")] = new string[] { nameof(ItemData.ShipModule.BurstRateOfFire) },
            [new ModLabelFDName("BurstSize")] = new string[] { nameof(ItemData.ShipModule.BurstSize) },
            [new ModLabelFDName("AmmoClipSize")] = new string[] { nameof(ItemData.ShipModule.Clip) },
            [new ModLabelFDName("AmmoMaximum")] = new string[] { nameof(ItemData.ShipModule.Ammo) },
            [new ModLabelFDName("RoundsPerShot")] = new string[] { nameof(ItemData.ShipModule.Rounds) },
            [new ModLabelFDName("ReloadTime")] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            [new ModLabelFDName("BreachDamage")] = new string[] { nameof(ItemData.ShipModule.BreachDamage) },
            [new ModLabelFDName("BreachPercent")] = new string[] { nameof(ItemData.ShipModule.BreachModuleDamageAfterBreach), },
            [new ModLabelFDName("MinBreachChance")] = new string[] { nameof(ItemData.ShipModule.BreachMin) },
            [new ModLabelFDName("MaxBreachChance")] = new string[] { nameof(ItemData.ShipModule.BreachMax) },
            [new ModLabelFDName("Jitter")] = new string[] { nameof(ItemData.ShipModule.Jitter) },
            [new ModLabelFDName("WeaponMode")] = new string[] { },
            [new ModLabelFDName("DamageType")] = new string[] { },
            [new ModLabelFDName("$Thermic;")] = new string[] { },       // new june 26
            [new ModLabelFDName("$Kinetic;")] = new string[] { },       // new june 26
            [new ModLabelFDName("ShieldGenMinimumMass")] = new string[] { nameof(ItemData.ShipModule.MinMass) },
            [new ModLabelFDName("ShieldGenMaximumMass")] = new string[] { nameof(ItemData.ShipModule.MaxMass) },
            [new ModLabelFDName("ShieldGenMinStrength")] = new string[] { nameof(ItemData.ShipModule.MinStrength) },
            [new ModLabelFDName("ShieldGenMaxStrength")] = new string[] { nameof(ItemData.ShipModule.MaxStrength) },
            [new ModLabelFDName("RegenRate")] = new string[] { nameof(ItemData.ShipModule.RegenRate) },
            [new ModLabelFDName("BrokenRegenRate")] = new string[] { nameof(ItemData.ShipModule.BrokenRegenRate) },
            [new ModLabelFDName("EnergyPerRegen")] = new string[] { nameof(ItemData.ShipModule.MWPerUnit) },
            [new ModLabelFDName("FSDOptimalMass")] = new string[] { nameof(ItemData.ShipModule.OptMass) },
            [new ModLabelFDName("FSDHeatRate")] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            [new ModLabelFDName("MaxFuelPerJump")] = new string[] { nameof(ItemData.ShipModule.MaxFuelPerJump) },
            [new ModLabelFDName("EngineMinimumMass")] = new string[] { nameof(ItemData.ShipModule.MinMass) },
            [new ModLabelFDName("MaximumMass")] = new string[] { nameof(ItemData.ShipModule.MaxMass) },
            [new ModLabelFDName("EngineMinPerformance")] = new string[] { nameof(ItemData.ShipModule.EngineMinMultiplier) },
            [new ModLabelFDName("EngineMaxPerformance")] = new string[] { nameof(ItemData.ShipModule.EngineMaxMultiplier) },
            [new ModLabelFDName("EngineHeatRate")] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            [new ModLabelFDName("PowerCapacity")] = new string[] { nameof(ItemData.ShipModule.PowerGen) },
            [new ModLabelFDName("HeatEfficiency")] = new string[] { nameof(ItemData.ShipModule.HeatEfficiency) },
            [new ModLabelFDName("WeaponsCapacity")] = new string[] { nameof(ItemData.ShipModule.WeaponsCapacity) },
            [new ModLabelFDName("WeaponsRecharge")] = new string[] { nameof(ItemData.ShipModule.WeaponsRechargeRate) },
            [new ModLabelFDName("EnginesCapacity")] = new string[] { nameof(ItemData.ShipModule.EngineCapacity) },
            [new ModLabelFDName("EnginesRecharge")] = new string[] { nameof(ItemData.ShipModule.EngineRechargeRate) },
            [new ModLabelFDName("SystemsCapacity")] = new string[] { nameof(ItemData.ShipModule.SystemsCapacity) },
            [new ModLabelFDName("SystemsRecharge")] = new string[] { nameof(ItemData.ShipModule.SystemsRechargeRate) },
            [new ModLabelFDName("DefenceModifierHealthMultiplier")] = new string[] { nameof(ItemData.ShipModule.HullStrengthBonus) },
            [new ModLabelFDName("DefenceModifierHealthAddition")] = new string[] { nameof(ItemData.ShipModule.HullReinforcement) },
            [new ModLabelFDName("DefenceModifierShieldMultiplier")] = new string[] { nameof(ItemData.ShipModule.ShieldReinforcement) },
            [new ModLabelFDName("DefenceModifierShieldAddition")] = new string[] { nameof(ItemData.ShipModule.AdditionalReinforcement) },
            [new ModLabelFDName("CollisionResistance")] = new string[] { },
            [new ModLabelFDName("KineticResistance")] = new string[] { nameof(ItemData.ShipModule.KineticResistance) },
            [new ModLabelFDName("ThermicResistance")] = new string[] { nameof(ItemData.ShipModule.ThermalResistance) },
            [new ModLabelFDName("ExplosiveResistance")] = new string[] { nameof(ItemData.ShipModule.ExplosiveResistance) },
            [new ModLabelFDName("CausticResistance")] = new string[] { nameof(ItemData.ShipModule.CausticResistance) },
            [new ModLabelFDName("FSDInterdictorRange")] = new string[] { nameof(ItemData.ShipModule.TargetMaxTime) },
            [new ModLabelFDName("FSDInterdictorFacingLimit")] = new string[] { nameof(ItemData.ShipModule.Angle) },
            [new ModLabelFDName("ScannerRange")] = new string[] { nameof(ItemData.ShipModule.Range) },
            [new ModLabelFDName("DiscoveryScannerRange")] = new string[] { },
            [new ModLabelFDName("DiscoveryScannerPassiveRange")] = new string[] { },
            [new ModLabelFDName("MaxAngle")] = new string[] { nameof(ItemData.ShipModule.Angle) },
            [new ModLabelFDName("ScannerTimeToScan")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("ChaffJamDuration")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("ECMRange")] = new string[] { nameof(ItemData.ShipModule.Range) },
            [new ModLabelFDName("ECMTimeToCharge")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("ECMActivePowerConsumption")] = new string[] { nameof(ItemData.ShipModule.ActivePower) },
            [new ModLabelFDName("ECMHeat")] = new string[] { nameof(ItemData.ShipModule.ThermalLoad) },
            [new ModLabelFDName("ECMCooldown")] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            [new ModLabelFDName("HeatSinkDuration")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("ThermalDrain")] = new string[] { nameof(ItemData.ShipModule.ThermalDrain) },
            [new ModLabelFDName("NumBuggySlots")] = new string[] { nameof(ItemData.ShipModule.Capacity) },
            [new ModLabelFDName("CargoCapacity")] = new string[] { nameof(ItemData.ShipModule.Size) },
            [new ModLabelFDName("MaxActiveDrones")] = new string[] { nameof(ItemData.ShipModule.Limpets) },
            [new ModLabelFDName("DroneTargetRange")] = new string[] { nameof(ItemData.ShipModule.TargetRange) },
            [new ModLabelFDName("DroneLifeTime")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("DroneSpeed")] = new string[] { nameof(ItemData.ShipModule.Speed) },
            [new ModLabelFDName("DroneMultiTargetSpeed")] = new string[] { nameof(ItemData.ShipModule.MultiTargetSpeed) },
            [new ModLabelFDName("DroneFuelCapacity")] = new string[] { nameof(ItemData.ShipModule.FuelTransfer) },
            [new ModLabelFDName("DroneRepairCapacity")] = new string[] { nameof(ItemData.ShipModule.MaxRepairMaterialCapacity) },
            [new ModLabelFDName("DroneHackingTime")] = new string[] { nameof(ItemData.ShipModule.HackTime) },
            [new ModLabelFDName("DroneMinJettisonedCargo")] = new string[] { nameof(ItemData.ShipModule.MinCargo) },
            [new ModLabelFDName("DroneMaxJettisonedCargo")] = new string[] { nameof(ItemData.ShipModule.MaxCargo) },
            [new ModLabelFDName("FuelScoopRate")] = new string[] { nameof(ItemData.ShipModule.RefillRate) },
            [new ModLabelFDName("FuelCapacity")] = new string[] { nameof(ItemData.ShipModule.Size) },
            [new ModLabelFDName("OxygenTimeCapacity")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("RefineryBins")] = new string[] { nameof(ItemData.ShipModule.Capacity) },
            [new ModLabelFDName("AFMRepairCapacity")] = new string[] { nameof(ItemData.ShipModule.Ammo) },
            [new ModLabelFDName("AFMRepairConsumption")] = new string[] { nameof(ItemData.ShipModule.RateOfRepairConsumption) },
            [new ModLabelFDName("AFMRepairPerAmmo")] = new string[] { nameof(ItemData.ShipModule.RepairCostPerMat) },
            [new ModLabelFDName("MaxRange")] = new string[] { nameof(ItemData.ShipModule.Range) },
            [new ModLabelFDName("SensorTargetScanAngle")] = new string[] { nameof(ItemData.ShipModule.Angle) },
            [new ModLabelFDName("VehicleCargoCapacity")] = new string[] { },
            [new ModLabelFDName("VehicleHullMass")] = new string[] { },
            [new ModLabelFDName("VehicleFuelCapacity")] = new string[] { },
            [new ModLabelFDName("VehicleArmourHealth")] = new string[] { },
            [new ModLabelFDName("VehicleShieldHealth")] = new string[] { },
            [new ModLabelFDName("FighterMaxSpeed")] = new string[] { },
            [new ModLabelFDName("FighterBoostSpeed")] = new string[] { },
            [new ModLabelFDName("FighterPitchRate")] = new string[] { },
            [new ModLabelFDName("FighterDPS")] = new string[] { },
            [new ModLabelFDName("FighterYawRate")] = new string[] { },
            [new ModLabelFDName("FighterRollRate")] = new string[] { },
            [new ModLabelFDName("CabinCapacity")] = new string[] { nameof(ItemData.ShipModule.Passengers) },
            [new ModLabelFDName("CabinClass")] = new string[] { nameof(ItemData.ShipModule.CabinClass) },
            [new ModLabelFDName("DisruptionBarrierRange")] = new string[] { nameof(ItemData.ShipModule.Range) },
            [new ModLabelFDName("DisruptionBarrierChargeDuration")] = new string[] { nameof(ItemData.ShipModule.Time) },
            [new ModLabelFDName("DisruptionBarrierActivePower")] = new string[] { nameof(ItemData.ShipModule.MWPerSec) },
            [new ModLabelFDName("DisruptionBarrierCooldown")] = new string[] { nameof(ItemData.ShipModule.ReloadTime) },
            [new ModLabelFDName("WingDamageReduction")] = new string[] { },
            [new ModLabelFDName("WingMinDuration")] = new string[] { },
            [new ModLabelFDName("WingMaxDuration")] = new string[] { },
            [new ModLabelFDName("ShieldSacrificeAmountRemoved")] = new string[] { },
            [new ModLabelFDName("ShieldSacrificeAmountGiven")] = new string[] { },
            [new ModLabelFDName("FSDJumpRangeBoost")] = new string[] { nameof(ItemData.ShipModule.AdditionalRange) },
            [new ModLabelFDName("FSDFuelUseIncrease")] = new string[] { },
            [new ModLabelFDName("BoostSpeedMultiplier")] = new string[] { },
            [new ModLabelFDName("BoostAugmenterPowerUse")] = new string[] { },
            [new ModLabelFDName("ModuleDefenceAbsorption")] = new string[] { nameof(ItemData.ShipModule.Protection) },
            [new ModLabelFDName("DSS_RangeMult")] = new string[] { },
            [new ModLabelFDName("DSS_AngleMult")] = new string[] { },
            [new ModLabelFDName("DSS_RateMult")] = new string[] { },
            [new ModLabelFDName("DSS_PatchRadius")] = new string[] { nameof(ItemData.ShipModule.ProbeRadius) },

            [new ModLabelFDName("BurstRate")] = new string[] { nameof(ItemData.ShipModule.BurstRateOfFire) },
            [new ModLabelFDName("BurstSize")] = new string[] { nameof(ItemData.ShipModule.BurstSize) },
            [new ModLabelFDName("DamageFalloffRange")] = new string[] { nameof(ItemData.ShipModule.Falloff) },

            [new ModLabelFDName("GuardianModuleResistance")] = new string[] { nameof(ItemData.ShipModule.GuardianModuleResistance) },       // add in edsy aug 24 version. String, Active or ""
        };



        static private Dictionary<EngineeringRecipeFDName, ItemData.ShipModule> specialeffects = new Dictionary<EngineeringRecipeFDName, ItemData.ShipModule>()
        {
            [new EngineeringRecipeFDName("special_auto_loader")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Auto reload while firing") { },
            [new EngineeringRecipeFDName("special_concordant_sequence")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Wing shield regen increased") { ThermalLoad = 50 },
            [new EngineeringRecipeFDName("special_corrosive_shell")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target armor hardness reduced") { Ammo = -20 },
            [new EngineeringRecipeFDName("special_blinding_shell")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target sensor acuity reduced") { },
            [new EngineeringRecipeFDName("special_dispersal_field")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target gimbal/turret tracking reduced") { },
            [new EngineeringRecipeFDName("special_weapon_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_drag_munitions")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target speed reduced") { },
            [new EngineeringRecipeFDName("special_emissive_munitions")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target signature increased") { ThermalLoad = 100 },
            [new EngineeringRecipeFDName("special_feedback_cascade_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target shield cell disrupted") { Damage = -20, ThermalLoad = -40 },
            [new EngineeringRecipeFDName("special_weapon_efficient")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            [new EngineeringRecipeFDName("special_force_shell")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target pushed off course") { Speed = -16.666666666666671 },
            [new EngineeringRecipeFDName("special_fsd_interrupt")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target FSD reboots") { Damage = -30, BurstInterval = 50 },
            [new EngineeringRecipeFDName("special_high_yield_shell")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { Damage = -35, BurstInterval = 11.111111111111111, KineticProportionDamage = 50, ExplosiveProportionDamage = 50 },
            [new EngineeringRecipeFDName("special_incendiary_rounds")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { BurstInterval = 5.2631578947368416, ThermalLoad = 200, KineticProportionDamage = 10, ThermalProportionDamage = 90 },
            [new EngineeringRecipeFDName("special_distortion_field")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Damage = 50, KineticProportionDamage = 50, ThermalProportionDamage = 50, Jitter = 3 },
            [new EngineeringRecipeFDName("special_choke_canister")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target thrusters reboot") { },
            [new EngineeringRecipeFDName("special_mass_lock")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target FSD inhibited") { },
            [new EngineeringRecipeFDName("special_weapon_rateoffire")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, BurstInterval = -2.9126213592233 },
            [new EngineeringRecipeFDName("special_overload_munitions")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ThermalProportionDamage = 50, ExplosiveProportionDamage = 50 },
            [new EngineeringRecipeFDName("special_weapon_damage")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, Damage = 3 },
            [new EngineeringRecipeFDName("special_penetrator_munitions")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { },
            [new EngineeringRecipeFDName("special_deep_cut_payload")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { },
            [new EngineeringRecipeFDName("special_phasing_sequence")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "10% of damage bypasses shields") { Damage = -10 },
            [new EngineeringRecipeFDName("special_plasma_slug")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Reload from ship fuel") { Damage = -10, Ammo = -100 },
            [new EngineeringRecipeFDName("special_plasma_slug_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Reload from ship fuel") { Damage = -10, ThermalLoad = -40, Ammo = -100 },
            [new EngineeringRecipeFDName("special_radiant_canister")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Area heat increased and sensors disrupted") { },
            [new EngineeringRecipeFDName("special_regeneration_sequence")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target wing shields regenerated") { Damage = -10 },
            [new EngineeringRecipeFDName("special_reverberating_cascade")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target shield generator damaged") { },
            [new EngineeringRecipeFDName("special_scramble_spectrum")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target modules malfunction") { BurstInterval = 11.111111111111111 },
            [new EngineeringRecipeFDName("special_screening_shell")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Effective against munitions") { ReloadTime = -50 },
            [new EngineeringRecipeFDName("special_shiftlock_canister")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Area FSDs reboot") { },
            [new EngineeringRecipeFDName("special_smart_rounds")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "No damage to untargeted ships") { },
            [new EngineeringRecipeFDName("special_weapon_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_super_penetrator_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target module damage") { ThermalLoad = -40, ReloadTime = 50 },
            [new EngineeringRecipeFDName("special_lock_breaker")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target loses target lock") { },
            [new EngineeringRecipeFDName("special_thermal_cascade")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Shielded target heat increased") { },
            [new EngineeringRecipeFDName("special_thermal_conduit")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Damage increases with heat level") { },
            [new EngineeringRecipeFDName("special_thermalshock")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Target heat increased") { },
            [new EngineeringRecipeFDName("special_thermal_vent")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "Heat reduced when striking a target") { },
            [new EngineeringRecipeFDName("special_shieldbooster_explosive")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, ExplosiveResistance = 2 },
            [new EngineeringRecipeFDName("special_shieldbooster_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_shieldbooster_efficient")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            [new EngineeringRecipeFDName("special_shieldbooster_kinetic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, KineticResistance = 2 },
            [new EngineeringRecipeFDName("special_shieldbooster_chunky")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = 5, KineticResistance = -2, ThermalResistance = -2, ExplosiveResistance = -2 },
            [new EngineeringRecipeFDName("special_shieldbooster_thermic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ShieldReinforcement = -1, ThermalResistance = 2 },
            [new EngineeringRecipeFDName("special_armour_kinetic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, KineticResistance = 8 },
            [new EngineeringRecipeFDName("special_armour_chunky")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = 8, KineticResistance = -3, ThermalResistance = -3, ExplosiveResistance = -3 },
            [new EngineeringRecipeFDName("special_armour_explosive")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, ExplosiveResistance = 8 },
            [new EngineeringRecipeFDName("special_armour_thermic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullStrengthBonus = -3, ThermalResistance = 8 },
            [new EngineeringRecipeFDName("special_powerplant_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_powerplant_highcharge")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = 10, PowerGen = 5 },
            [new EngineeringRecipeFDName("special_powerplant_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_powerplant_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HeatEfficiency = -10 },
            [new EngineeringRecipeFDName("special_engine_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_engine_overloaded")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { EngineOptMultiplier = 4, ThermalLoad = 10 },
            [new EngineeringRecipeFDName("special_engine_haulage")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptMass = 10 },
            [new EngineeringRecipeFDName("special_engine_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_engine_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = 5, ThermalLoad = -10 },
            [new EngineeringRecipeFDName("special_fsd_fuelcapacity")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 5, MaxFuelPerJump = 10 },
            [new EngineeringRecipeFDName("special_fsd_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 25 },
            [new EngineeringRecipeFDName("special_fsd_heavy")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = -8, OptMass = 4 },
            [new EngineeringRecipeFDName("special_fsd_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_fsd_cooled")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { ThermalLoad = -10 },
            [new EngineeringRecipeFDName("special_powerdistributor_capacity")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { WeaponsCapacity = 8, WeaponsRechargeRate = -2, EngineCapacity = 8, EngineRechargeRate = -2, SystemsCapacity = 8, SystemsRechargeRate = -2 },
            [new EngineeringRecipeFDName("special_powerdistributor_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_powerdistributor_efficient")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            [new EngineeringRecipeFDName("special_powerdistributor_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_powerdistributor_fast")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { WeaponsCapacity = -4, WeaponsRechargeRate = 4, EngineCapacity = -4, EngineRechargeRate = 4, SystemsCapacity = -4, SystemsRechargeRate = 4 },
            [new EngineeringRecipeFDName("special_hullreinforcement_kinetic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, KineticResistance = 2 },
            [new EngineeringRecipeFDName("special_hullreinforcement_chunky")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = 10, KineticResistance = -2, ThermalResistance = -2, ExplosiveResistance = -2 },
            [new EngineeringRecipeFDName("special_hullreinforcement_explosive")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, ExplosiveResistance = 2 },
            [new EngineeringRecipeFDName("special_hullreinforcement_thermic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { HullReinforcement = -5, ThermalResistance = 2 },
            [new EngineeringRecipeFDName("special_shieldcell_oversized")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { SCBSpinUp = 20, ShieldReinforcement = 5 },
            [new EngineeringRecipeFDName("special_shieldcell_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_shieldcell_efficient")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -10 },
            [new EngineeringRecipeFDName("special_shieldcell_gradual")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { SCBDuration = 10, ShieldReinforcement = -5 },
            [new EngineeringRecipeFDName("special_shieldcell_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_shield_toughened")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Integrity = 15 },
            [new EngineeringRecipeFDName("special_shield_regenerative")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { RegenRate = 15, BrokenRegenRate = 15, KineticResistance = -1.5, ThermalResistance = -1.5, ExplosiveResistance = -1.5 },
            [new EngineeringRecipeFDName("special_shield_kinetic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptStrength = -3, KineticResistance = 8 },
            [new EngineeringRecipeFDName("special_shield_health")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 10, OptStrength = 6, MWPerUnit = 25 },
            [new EngineeringRecipeFDName("special_shield_efficient")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = -20, OptStrength = -2, MWPerUnit = -20, KineticResistance = -1, ThermalResistance = -1, ExplosiveResistance = -1 },
            [new EngineeringRecipeFDName("special_shield_resistive")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { PowerDraw = 10, MWPerUnit = 25, KineticResistance = 3, ThermalResistance = 3, ExplosiveResistance = 3 },
            [new EngineeringRecipeFDName("special_shield_lightweight")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { Mass = -10 },
            [new EngineeringRecipeFDName("special_shield_thermic")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { OptStrength = -3, ThermalResistance = 8 },

            // added older no longer supported ones
            [new EngineeringRecipeFDName("special_feedback_cascade")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { },
            [new EngineeringRecipeFDName("special_super_penetrator")] = new ItemData.ShipModule(0, ItemData.ShipModule.ModuleTypes.SpecialEffect, "") { },

        };

        // for special effects, what to do..
        // 0 = set, 1 = add, 2 means mod 100 on primary value, else its modmod together in %

        static private Dictionary<ModLabelFDName, double> specialeffectmodcontrol = new Dictionary<ModLabelFDName, double>()
        {
            [new ModLabelFDName("BurstRateOfFire")] = 0,
            [new ModLabelFDName("BurstSize")] = 0,
            [new ModLabelFDName("Rounds")] = 1,
            [new ModLabelFDName("Jitter")] = 1,
            [new ModLabelFDName("KineticProportionDamage")] = 0,
            [new ModLabelFDName("ThermalProportionDamage")] = 0,
            [new ModLabelFDName("ExplosiveProportionDamage")] = 0,
            [new ModLabelFDName("AbsoluteProportionDamage")] = 0,
            [new ModLabelFDName("CausticPorportionDamage")] = 0,
            [new ModLabelFDName("AXPorportionDamage")] = 0,
            [new ModLabelFDName("HullStrengthBonus")] = 100,
            [new ModLabelFDName("ShieldReinforcement")] = 100,
            [new ModLabelFDName("KineticResistance")] = -100,
            [new ModLabelFDName("ThermalResistance")] = -100,
            [new ModLabelFDName("ExplosiveResistance")] = -100,
            [new ModLabelFDName("CausticResistance")] = -100,
            [new ModLabelFDName("AXResistance")] = -100,
            [new ModLabelFDName("Capacity")] = 1,
            [new ModLabelFDName("Limpets")] = 1,
            [new ModLabelFDName("MinCargo")] = 1,
            [new ModLabelFDName("MaxCargo")] = 1,
            [new ModLabelFDName("Capacity")] = 1,
        };
    }

}
