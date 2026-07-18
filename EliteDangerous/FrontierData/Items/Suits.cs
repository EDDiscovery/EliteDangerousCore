/*
 * Copyright 2022-2024 EDDiscovery development team
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        static public bool IsSuitTypeName(FDName ifd)       // If a suit..
        {
            return ifd.Contains("suit");
        }


        static public Suit GetSuit(FDName fdname, string locname = null)         // suit weapons
        {
            if (suit.TryGetValue(fdname, out Suit var))
                return var;
            else
            {
                int classn = fdname.GetClass();
                Debugger.Break();
                System.Diagnostics.Debug.WriteLine($"Unknown Suit: {{ \"{fdname}\", new Suit(\"{locname??fdname.SplitCapsWordFull()}\",{classn},0,0,\"\",\"\",\"\",null) }},");
                return null;
            }
        }

        static public Tuple<FDName,Suit> GetNextSuit(FDName fdname, int newclass, string locname = null)         // given a suit, return next suit
        {
            string name = fdname.Str();
            if (name.Length > 0 )
            {
                fdname = new FDName(name.Substring(0, fdname.Str().Length - 1) + newclass.ToStringInvariant());

                if (suit.TryGetValue(fdname, out Suit var))
                    return new Tuple<FDName,Suit>(fdname,var);
            }

            System.Diagnostics.Trace.WriteLine($"Can't upgrade Suit {fdname}");
            return null;
        }

        public class SuitStats
        {
            public double HealthMultiplierKinetic;
            public double HealthMultiplierThermal;
            public double HealthMultiplierPlasma;
            public double HealthMultiplierExplosive;
            public double ShieldMultiplierKinetic;
            public double ShieldMultiplierThermal;
            public double ShieldMultiplierPlasma;
            public double ShieldMultiplierExplosive;
            public double ShieldRegen;     // BSH*OM = shield
            public double Shield;     // BSH*OM = shield
            public double EnergyCap;
            public int OxygenTime;
            public int ItemCap;
            public int ComponentCap;
            public int DataCap;
            public SuitStats(double hk, double ht, double hp, double he,
                             double sk, double st, double sp, double se,
                             double sregen, double stot, double ec, int o, int i, int c, int d)
            {
                HealthMultiplierKinetic = hk;
                HealthMultiplierThermal = ht;
                HealthMultiplierPlasma = hp;
                HealthMultiplierExplosive = he;
                ShieldMultiplierKinetic = hk;
                ShieldMultiplierThermal = ht;
                ShieldMultiplierPlasma = hp;
                ShieldMultiplierExplosive = he;
                ShieldRegen = sregen;
                Shield = stot;     // BSH*OM
                EnergyCap = ec;
                OxygenTime = o;
                ItemCap = i;
                ComponentCap = c;
                DataCap = d;
            }
        }

        public class Suit 
        {
            public string Type;         // Name in english
            public int Class;
            public string Name;         // Name and class
            public int PrimaryWeapons;
            public int SecondaryWeapons;
            public string U1;
            public string U2;
            public string U3;
            public SuitStats Stats;     // may be null! if for ai suits

            public Suit(string englishname, int cls, int primary, int secondary, string u1, string u2, string u3, SuitStats values)
            {
                Type = englishname; Class = cls; 
                Name = englishname + (Class > 0 ? " Class " + Class.ToStringInvariant() : "");
                PrimaryWeapons = primary;
                SecondaryWeapons = secondary;
                U1 = u1;
                U2 = u2;
                U3 = u3;
                Stats = values;
            }
        }

        // rob checked 20/8/21 for all suits to class 3 in game, class 4/5 according to wiki

        public static Dictionary<FDName, Suit> suit = new Dictionary<FDName, Suit>(new FDNameEqualityComparer())   // DO NOT USE DIRECTLY - public is for checking only
        {
                 { new FDName("flightsuit"), new Suit( "Flight Suit", 0, 0, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 1.7, 0.6, 1.2, 1, // health kinetic, thermal, plasma, explosive                  Greater is WORSE, so 1.7 is 70% worse
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                0.55, 7.5, // regen, shield health  
                7, 60, 5,10,10 )) }, // battery, oxygen, items, components, data     

                 { new FDName("tacticalsuit_class1"), new Suit( "Dominator Suit", 1, 2, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 1.5, 0.4, 1, 1, // health kinetic, thermal, plasma, explosive : correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.1, 15, // regen, shield health : correct
                10, 60, 5,10,10 )) }, // battery, oxygen, items, components, data : correct

                 { new FDName("tacticalsuit_class2"), new Suit( "Dominator Suit", 2, 2, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 1.26, 0.34, 0.84, 0.84, // health kinetic, thermal, plasma, explosive : correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.34, 18.3, // regen, shield health correct
                10, 60, 5,10,10 )) }, // battery, oxygen, items, components, data correct

                 { new FDName("tacticalsuit_class3"), new Suit( "Dominator Suit", 3, 2, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 1.07, 0.28, 0.71, 0.71, // health kinetic, thermal, plasma, explosive : correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.65, 22.5, // regen, shield health correct
                10, 60, 5,10,10 )) }, // battery, oxygen, items, components, data correct

                 { new FDName("tacticalsuit_class4"), new Suit( "Dominator Suit", 4, 2, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 0.89, 0.24, 0.59, 0.59, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                2.02, 27.6, // regen, shield health matches https://elite-dangerous.fandom.com/wiki/Artemis_Suit
                10, 60, 5,10,10 )) }, // battery, oxygen, items, components, data

                 { new FDName("tacticalsuit_class5"), new Suit( "Dominator Suit", 5, 2, 1, "Energylink", "Profile Analyser", "",
                new SuitStats( 0.75, 0.2, 0.5, 0.5, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                2.48, 33.8, // regen, shield health matches https://elite-dangerous.fandom.com/wiki/Artemis_Suit
                10, 60, 5,10,10 )) }, // battery, oxygen, items, components, data

                 { new FDName("explorationsuit_class1"), new Suit( "Artemis Suit", 1, 1, 1, "Energylink", "Profile Analyser", "Genetic Sampler",
                new SuitStats( 1.7, 0.6, 1.2, 1, // health kinetic, thermal, plasma, explosive : Correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                0.88, 12, // regen, shield health : wrong in frontier data, game says 0.88,12
                17, 60, 10,20,10 )) }, // battery, oxygen, items, components, data : correct

                 { new FDName("explorationsuit_class2"), new Suit( "Artemis Suit", 2, 1, 1, "Energylink", "Profile Analyser", "Genetic Sampler",
                new SuitStats( 1.43, 0.5, 1.01, 0.84, // health kinetic, thermal, plasma, explosive : correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.07, 14.7, // regen, shield health : wrong in frontier data, game says 1.07,14.7
                17, 60, 10,20,10 )) }, // battery, oxygen, items, components, data : correct

                 { new FDName("explorationsuit_class3"), new Suit( "Artemis Suit", 3, 1, 1, "Energylink", "Profile Analyser", "Genetic Sampler",
                new SuitStats( 1.21, 0.43, 0.85, 0.71, // health kinetic, thermal, plasma, explosive Correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.32, 18, // regen, shield health wrong in frontier data. fixed to game data
                17, 60, 10,20,10 )) }, // battery, oxygen, items, components, data Correct

                 { new FDName("explorationsuit_class4"), new Suit( "Artemis Suit", 4, 1, 1, "Energylink", "Profile Analyser", "Genetic Sampler",
                new SuitStats( 1, 0.35, 0.71, 0.59, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.62, 22.1, // regen, shield health - wrong in frontier data, corrected according to https://elite-dangerous.fandom.com/wiki/Artemis_Suit
                17, 60, 10,20,10 )) }, // battery, oxygen, items, components, data

                 { new FDName("explorationsuit_class5"), new Suit( "Artemis Suit", 5, 1, 1, "Energylink", "Profile Analyser", "Genetic Sampler",
                new SuitStats( 0.85, 0.3, 0.6, 0.5, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.98, 27, // regen, shield health - Artie supplied this one via discord
                17, 60, 10,20,10 )) }, // battery, oxygen, items, components, data

                 { new FDName("utilitysuit_class1"), new Suit( "Maverick Suit", 1, 1, 1, "Energylink", "Profile Analyser", "Arc Cutter",
                new SuitStats( 1.6, 0.5, 1.1, 1, // health kinetic, thermal, plasma, explosive : correct
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                0.99, 13.5, // regen, shield health wrong in frontier data, game says 0.99,13.5
                13.5, 60, 15,30,10 )) }, // battery, oxygen, items, components, data correct

                 { new FDName("utilitysuit_class2"), new Suit( "Maverick Suit", 2, 1, 1, "Energylink", "Profile Analyser", "Arc Cutter",
                new SuitStats( 1.34, 0.42, 0.92, 0.84, // health kinetic, thermal, plasma, explosive : Correct   
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.21, 16.5, // regen, shield health wrong in frontier data, game says 1.21,16.5 
                13.5, 60, 15,30,10 )) }, // battery, oxygen, items, components, data correct 

                 { new FDName("utilitysuit_class3"), new Suit( "Maverick Suit", 3, 1, 1, "Energylink", "Profile Analyser", "Arc Cutter",
                new SuitStats( 1.14, 0.36, 0.78, 0.71, // health kinetic, thermal, plasma, explosive        // 20/8/21
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.49, 20.3, // regen, shield health     // Wrong, game says 1.49, 20.3
                13.5, 60, 15,30,10 )) }, // battery, oxygen, items, components, data    // 20/8/21

                 { new FDName("utilitysuit_class4"), new Suit( "Maverick Suit", 4, 1, 1, "Energylink", "Profile Analyser", "Arc Cutter",
                new SuitStats( 0.94, 0.3, 0.65, 0.59, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                1.82, 24.9, // regen, shield health - wrong in frontier data, corrected according to https://elite-dangerous.fandom.com/wiki/Artemis_Suit
                13.5, 60, 15,30,10 )) }, // battery, oxygen, items, components, data

                 { new FDName("utilitysuit_class5"), new Suit( "Maverick Suit", 5, 1, 1, "Energylink", "Profile Analyser", "Arc Cutter",
                new SuitStats( 0.8, 0.25, 0.55, 0.5, // health kinetic, thermal, plasma, explosive
                0.4, 1.5, 1, 0.5, // shield kinetic, thermal, plasma, explosive
                2.23, 30.5, // regen, shield health wrong in frontier data, corrected according to https://elite-dangerous.fandom.com/wiki/Artemis_Suit
                13.5, 60, 15,30,10 )) }, // battery, oxygen, items, components, data


                 { new FDName("flightsuitai"), new Suit("AI Flight Suit",1,0,0,"","","",null) },
                 { new FDName("lightassaultsuitai_class1"), new Suit("AI Light Assault Suit",1,0,0,"","","",null) },
                 { new FDName("lightassaultsuitai_class2"), new Suit("AI Light Assault Suit",2,0,0,"","","",null) },
                 { new FDName("lightassaultsuitai_class3"), new Suit("AI Light Assault Suit",3,0,0,"","","",null) },
                 { new FDName("lightassaultsuitai_class4"), new Suit("AI Light Assault Suit",4,0,0,"","","",null) },
                 { new FDName("lightassaultsuitai_class5"), new Suit("AI Light Assault Suit", 5, 0, 0, "", "", "", null) },

                 { new FDName("assaultsuitai_class1"), new Suit("AI Assault Suit",1,0,0,"","","",null) },
                 { new FDName("assaultsuitai_class2"), new Suit("AI Assault Suit",2,0,0,"","","",null) },
                 { new FDName("assaultsuitai_class3"), new Suit("AI Assault Suit",3,0,0,"","","",null) },
                 { new FDName("assaultsuitai_class4"), new Suit("AI Assault Suit", 4, 0, 0, "", "", "", null) },
                 { new FDName("assaultsuitai_class5"), new Suit("AI Assault Suit", 5, 0, 0, "", "", "", null) },

                 { new FDName("closesuitai_class1"), new Suit("AI Close Suit", 1, 0, 0, "", "", "", null) },
                 { new FDName("closesuitai_class2"), new Suit("AI Close Suit", 2, 0, 0, "", "", "", null) },
                 { new FDName("closesuitai_class3"), new Suit("AI Close Suit", 3, 0, 0, "", "", "", null) },
                 { new FDName("closesuitai_class4"), new Suit("AI Close Suit", 4, 0, 0, "", "", "", null) },
                 { new FDName("closesuitai_class5"), new Suit("AI Close Suit", 5, 0, 0, "", "", "", null) },

                 { new FDName("rangedsuitai_class1"), new Suit("AI Ranger Suit", 1, 0, 0, "", "", "", null) },
                 { new FDName("rangedsuitai_class2"), new Suit("AI Ranger Suit", 2, 0, 0, "", "", "", null) },
                 { new FDName("rangedsuitai_class3"), new Suit("AI Ranger Suit", 3, 0, 0, "", "", "", null) },
                 { new FDName("rangedsuitai_class4"), new Suit("AI Ranger Suit", 4, 0, 0, "", "", "", null) },
                 { new FDName("rangedsuitai_class5"), new Suit("AI Ranger Suit", 5, 0, 0, "", "", "", null) },

                 { new FDName("heavysuitai_class1"), new Suit("AI Heavy Suit", 1, 0, 0, "", "", "", null) },
                 { new FDName("heavysuitai_class2"), new Suit("AI Heavy Suit", 2, 0, 0, "", "", "", null) },
                 { new FDName("heavysuitai_class3"), new Suit("AI Heavy Suit", 3, 0, 0, "", "", "", null) },       
                 { new FDName("heavysuitai_class4"), new Suit("AI Heavy Suit", 4, 0, 0, "", "", "", null) },       
                 { new FDName("heavysuitai_class5"), new Suit("AI Heavy Suit", 5, 0, 0, "", "", "", null) },

                 { new FDName("citizensuitai_admin"), new Suit("AI Citizen Suit Admin", 0, 0, 0, "", "", "", null) },
                 { new FDName("citizensuitai_scientific"), new Suit("AI Citizen Suit Scientific", 0, 0, 0, "", "", "", null) },
                 { new FDName("citizensuitai_industrial"), new Suit("AI Citizen Suit Industrial", 0, 0, 0, "", "", "", null) },

         };

    }
}
