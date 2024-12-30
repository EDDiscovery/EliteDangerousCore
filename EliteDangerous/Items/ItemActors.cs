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
        static public bool IsActor(string fdname)
        {
            return actors.ContainsKey(fdname.ToLowerInvariant());
        }

        // actors are things like skimmer drones
        // may return null if not known
        static public Actor GetActor(string fdname, string locname = null)         
        {
            fdname = fdname.ToLowerInvariant();
            if (actors.TryGetValue(fdname, out Actor var))
                return var;
            else
            {
                System.Diagnostics.Trace.WriteLine($"*** Unknown Actor: {{ \"{fdname}\", new Actor(\"{locname ?? fdname.SplitCapsWordFull()}\") }},");
                return null;
            }
        }

        // copes with $...;data actors found in NPC messages with semi colon seperated ID text
        static public Actor GetActorNPC(string fdname)
        {
            int semi = fdname.IndexOf(';');
            string id = semi>0 ? fdname.Substring(0, semi): fdname;
            id = id.ToLowerInvariant();

            if (actors.TryGetValue(id, out Actor var))
            {
                return new Actor(var.Name + (semi > 0 ? ": " + fdname.Substring(semi + 1).Trim() : ""));
            }
            else
                return null;
        }


        public class Actor
        {
            public string Name;
            public Actor(string name) { Name = name; }
        }

        public static Dictionary<string, Actor> actors = new Dictionary<string, Actor>   // DO NOT USE DIRECTLY - public is for checking only
        {
             { "skimmerdrone", new Actor("Skimmer Drone") },
             { "skimmer", new Actor("Skimmer Drone") },
             { "missileskimmer", new Actor("Skimmer Missile") },
             { "bossskimmer", new Actor("Boss Skimmer") },

             { "thargon", new Actor("Thargon") },
             { "thargonswarm", new Actor("Thargon Swarm") },
             { "tg_skimmer_01", new Actor("Thargoid Scavenger") },   // seen
             { "tg_skimmer_02", new Actor("Thargoid Scavenger") },
             { "tg_skimmer_03", new Actor("Thargoid Scavenger") },
             { "tg_banshee_01", new Actor("Thargoid Banshee Type 1") },
             { "tg_banshee_02", new Actor("Thargoid Banshee Type 2") },
             { "tg_scavenger", new Actor("Thargoid Scavenger") },
             { "titan_hardpoint01", new Actor("Thargoid Titan") },
             { "titan_hardpoint02", new Actor("Thargoid Titan") },   // seen
             { "titan_hardpoint03", new Actor("Thargoid Titan") },
             { "titan", new Actor("Titan") },
             { "glaive", new Actor("Thargoid Glaive") },        // seen
             { "scythe", new Actor("Scythe") },
             { "scout_cargo", new Actor("Cargo Scout") },

             { "unknownsaucer", new Actor("Thargoid") },
             { "unknownsaucer_a", new Actor("Thargoid") },
             { "unknownsaucer_b", new Actor("Thargoid") },
             { "unknownsaucer_c", new Actor("Thargoid") },
             { "unknownsaucer_d", new Actor("Thargoid") },
             { "unknownsaucer_e", new Actor("Thargoid") },  // seen
             { "unknownsaucer_f", new Actor("Thargoid") },
             { "unknownsaucer_h", new Actor("Thargoid") },  // seen

             { "guardian_sentinel", new Actor("Guardian Sentinel") },

             { "ps_turretbasemedium02_6m", new Actor("Turret medium 2-6-M") },
             { "ps_turretbasesmall_3m", new Actor("Turret Small 3 M") },
             { "ps_turretbasemedium_skiff_6m", new Actor("Turret Medium 6 M") },

             { "poi_turretbasea", new Actor("Turret Base") },
             { "poi_turretbunkera", new Actor("Turret Bunker A") },
             { "poi_turretplatforma", new Actor("Turret Platform A") },

             { "mega_defences", new Actor("Mega Defences") },
             { "mega_turretbunkera", new Actor("Mega Turret A") },

             { "scout", new Actor("Thargoid Scout") },
             { "scout_q", new Actor("Thargoid Scout (Q)") },
             { "scout_hq", new Actor("Thargoid Scout (HQ)") },
             { "scout_nq", new Actor("Thargoid Scout (NQ)") },

             { "planetporta", new Actor("Planet Port") },
             { "planetportb", new Actor("Planet Port") },
             { "planetportc", new Actor("Planet Port") },
             { "planetportd", new Actor("Planet Port") },
             { "planetporte", new Actor("Planet Port") },
             { "planetportf", new Actor("Planet Port") },
             { "planetportg", new Actor("Planet Port") },           // seen g, presuming at least a-f
             { "planetporth", new Actor("Planet Port") },
             { "planetporti", new Actor("Planet Port") },           // seen up to i now july 24

             { "diamondback_taxi", new Actor("Taxi (Diamondback)") },
             { "viper_taxi", new Actor("Taxi (Viper)") },
             { "adder_taxi", new Actor("Taxi (Adder)") },
             { "vulture_taxi", new Actor("Taxi (Vulture)") },

             { "oneillcylinder", new Actor("O'Neill Cylinder") },
             { "oneillorbis", new Actor("O'Neill Orbis") },
             { "outpostcivilian", new Actor("Civilian Outpost") },
             { "outpostindustrial", new Actor("Industrial Outpost") },
             { "outpostcriminal", new Actor("Criminal Outpost") },
             { "outpostcommercial", new Actor("Commercial Outpost") },
             { "outpostscientific", new Actor("Scientific Outpost") },
             { "megashipdockrehab", new Actor("Mega Ship Prison") },
             { "megashipdocka", new Actor("Mega Ship Dock A") },
             { "asteroidbase", new Actor("Asteroid Base") },
             { "bernalsphere", new Actor("Station") },
             { "coriolis", new Actor("Coriolis Station") },
             { "carrierdocka", new Actor("Carrier Dock A") },
             { "carrierdockb", new Actor("Carrier Dock B") },
             { "federation_capitalship", new Actor("Federation Capital Ship") },

             { "lizryder", new Actor("Engineer Liz Ryder") },
             { "heratani", new Actor("Engineer Hera Tani") },

             { "thedweller", new Actor("The Dweller") },

             { "$name_ax_military", new Actor("AX Military Pilot") },       // seen in NPC texts
        };


    }
}

