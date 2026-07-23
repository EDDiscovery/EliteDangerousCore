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
using System.Linq;

namespace EliteDangerousCore
{
    public partial class ItemData
    {
        static public bool IsActor(FDName fdname)
        {
            return actors.ContainsKey(fdname);
        }

        // actors are things like skimmer drones
        // may return null if not known
        static public Actor GetActor(FDName fdname, string locname = null)         
        {
            if (actors.TryGetValue(fdname, out Actor var))
                return var;
            else
            {
               BaseUtils.Debugger.TraceBreak($"*** Unknown Actor: {{ \"{fdname}\"), new Actor(\"{locname ?? fdname.SplitCapsWordFull()}\") }},");
                return null;
            }
        }

        // copes with $...;data actors found in NPC messages with semi colon seperated ID text
        static public Actor GetActorNPC(FDName fdname)
        {
            int semi = fdname.Str().IndexOf(';');
            FDName nosemi = new FDName(semi > 0 ? fdname.ToLower().Substring(0, semi) : fdname.ToLower());

            if (actors.TryGetValue(nosemi, out Actor var))
            {
                return new Actor(var.Name + (semi > 0 ? ": " + fdname.Str().Substring(semi + 1).Trim() : ""));
            }
            else
                return null;
        }


        public class Actor
        {
            public string Name;
            public Actor(string name) { Name = name; }
        }

        // DO NOT USE DIRECTLY - public is for checking only
        public static Dictionary<FDName, Actor> actors = new Dictionary<FDName, Actor>(new FDNameEqualityComparer())
        {
             { new FDName("skimmerdrone"), new Actor("Skimmer Drone") },
             { new FDName("bombskimmerdrone"), new Actor("Bomb Skimmer Drone") },
             { new FDName("skimmer"), new Actor("Skimmer Drone") },
             { new FDName("missileskimmer"), new Actor("Skimmer Missile") },
             { new FDName("bossskimmer"), new Actor("Boss Skimmer") },

             { new FDName("thargon"), new Actor("Thargon") },
             { new FDName("thargonswarm"), new Actor("Thargon Swarm") },
             { new FDName("tg_skimmer_01"), new Actor("Thargoid Scavenger") },   // seen
             { new FDName("tg_skimmer_02"), new Actor("Thargoid Scavenger") },
             { new FDName("tg_skimmer_03"), new Actor("Thargoid Scavenger") },
             { new FDName("tg_banshee_01"), new Actor("Thargoid Banshee Type 1") },
             { new FDName("tg_banshee_02"), new Actor("Thargoid Banshee Type 2") },
             { new FDName("tg_scavenger"), new Actor("Thargoid Scavenger") },
             { new FDName("titan_hardpoint01"), new Actor("Thargoid Titan") },
             { new FDName("titan_hardpoint02"), new Actor("Thargoid Titan") },   // seen
             { new FDName("titan_hardpoint03"), new Actor("Thargoid Titan") },
             { new FDName("titan"), new Actor("Titan") },
             { new FDName("glaive"), new Actor("Thargoid Glaive") },        // seen
             { new FDName("scythe"), new Actor("Scythe") },
             { new FDName("scout_cargo"), new Actor("Cargo Scout") },

             { new FDName("unknownsaucer"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_a"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_b"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_c"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_d"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_e"), new Actor("Thargoid") },  // seen
             { new FDName("unknownsaucer_f"), new Actor("Thargoid") },
             { new FDName("unknownsaucer_h"), new Actor("Thargoid") },  // seen

             { new FDName("guardian_sentinel"), new Actor("Guardian Sentinel") },

             { new FDName("ps_turretbasemedium02_6m"), new Actor("Turret medium 2-6-M") },
             { new FDName("ps_turretbasesmall_3m"), new Actor("Turret Small 3 M") },
             { new FDName("ps_turretbasemedium_skiff_6m"), new Actor("Turret Medium 6 M") },

             { new FDName("poi_turretbasea"), new Actor("Turret Base") },
             { new FDName("poi_turretbunkera"), new Actor("Turret Bunker A") },
             { new FDName("poi_turretplatforma"), new Actor("Turret Platform A") },

             { new FDName("mega_defences"), new Actor("Mega Defences") },
             { new FDName("mega_turretbunkera"), new Actor("Mega Turret Type A") },
             { new FDName("mega_turretplatforma"), new Actor("Mega Platform Type A") },
             { new FDName("mega_turretplatformb"), new Actor("Mega Platform Type B") },

             { new FDName("scout"), new Actor("Thargoid Scout") },
             { new FDName("scout_q"), new Actor("Thargoid Scout (Q)") },
             { new FDName("scout_hq"), new Actor("Thargoid Scout (HQ)") },
             { new FDName("scout_nq"), new Actor("Thargoid Scout (NQ)") },

             { new FDName("planetporta"), new Actor("Planet Port") },
             { new FDName("planetportb"), new Actor("Planet Port") },
             { new FDName("planetportc"), new Actor("Planet Port") },
             { new FDName("planetportd"), new Actor("Planet Port") },
             { new FDName("planetporte"), new Actor("Planet Port") },
             { new FDName("planetportf"), new Actor("Planet Port") },
             { new FDName("planetportg"), new Actor("Planet Port") },           // seen g, presuming at least a-f
             { new FDName("planetporth"), new Actor("Planet Port") },
             { new FDName("planetporti"), new Actor("Planet Port") },           // seen up to i now july 24
             { new FDName("planetportj"), new Actor("Planet Port") },           // seen may 25

             { new FDName("diamondback_taxi"), new Actor("Taxi (Diamondback)") },
             { new FDName("viper_taxi"), new Actor("Taxi (Viper)") },
             { new FDName("adder_taxi"), new Actor("Taxi (Adder)") },
             { new FDName("vulture_taxi"), new Actor("Taxi (Vulture)") },

             { new FDName("oneillcylinder"), new Actor("O'Neill Cylinder") },
             { new FDName("oneillorbis"), new Actor("O'Neill Orbis") },
             { new FDName("outpostcivilian"), new Actor("Civilian Outpost") },
             { new FDName("outpostindustrial"), new Actor("Industrial Outpost") },
             { new FDName("outpostcriminal"), new Actor("Criminal Outpost") },
             { new FDName("outpostcommercial"), new Actor("Commercial Outpost") },
             { new FDName("outpostscientific"), new Actor("Scientific Outpost") },
             { new FDName("outpostmilitary"), new Actor("Military Outpost") },
             { new FDName("outpost_weaponsplatform_depot"), new Actor("Weapons Platform in depot") },
             { new FDName("megashipdockrehab"), new Actor("Mega Ship Prison") },
             { new FDName("megashipdocka"), new Actor("Mega Ship Dock A") },
             { new FDName("asteroidbase"), new Actor("Asteroid Base") },
             { new FDName("bernalsphere"), new Actor("Station") },
             { new FDName("coriolis"), new Actor("Coriolis Station") },

             { new FDName("carrierdocka"), new Actor("Carrier Dock A") },
             { new FDName("carrierdockb"), new Actor("Carrier Dock B") },
             { new FDName("carrierdocka_squadron"), new Actor("Squadron Carrier Dock A") },
             { new FDName("carrierdockb_squadron"), new Actor("Squadron Carrier Dock B") },

             { new FDName("federation_capitalship"), new Actor("Federation Capital Ship") },

             { new FDName("lizryder"), new Actor("Engineer Liz Ryder") },
             { new FDName("heratani"), new Actor("Engineer Hera Tani") },
             { new FDName("felicityfarseer"), new Actor("Engineer Felicity Farseer") },
             { new FDName("thesarge"), new Actor("Engineer The Sarge") },

             { new FDName("thedweller"), new Actor("The Dweller") },

             { new FDName("$name_ax_military"), new Actor("AX Military Pilot") },       // seen in NPC texts

             { new FDName("ms_dockablecoreb_twinhull"), new Actor("Dockable Twinhull") },
        };


    }
}

