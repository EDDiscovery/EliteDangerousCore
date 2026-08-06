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
        static public bool IsActor(ActorFDName fdname)
        {
            return actors.ContainsKey(fdname);
        }

        // actors are things like skimmer drones
        // may return null if not known
        static public Actor GetActor(ActorFDName fdname, string locname = null)         
        {
            if (actors.TryGetValue(fdname, out Actor var))
                return var;
            else
                return null;
        }

        // copes with $...;data actors found in NPC messages with semi colon seperated ID text
        static public Actor GetActorNPC(ActorFDName fdname)
        {
            int semi = fdname.ID.IndexOf(';');
            ActorFDName nosemi = new ActorFDName(semi > 0 ? fdname.ToLower().Substring(0, semi) : fdname.ToLower());

            if (actors.TryGetValue(nosemi, out Actor var))
            {
                return new Actor(var.Name + (semi > 0 ? ": " + fdname.ID.Substring(semi + 1).Trim() : ""));
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
        public static Dictionary<ActorFDName, Actor> actors = new Dictionary<ActorFDName, Actor>()
        {
             { new ActorFDName("skimmerdrone"), new Actor("Skimmer Drone") },
             { new ActorFDName("bombskimmerdrone"), new Actor("Bomb Skimmer Drone") },
             { new ActorFDName("skimmer"), new Actor("Skimmer Drone") },
             { new ActorFDName("missileskimmer"), new Actor("Skimmer Missile") },
             { new ActorFDName("bossskimmer"), new Actor("Boss Skimmer") },

             { new ActorFDName("thargon"), new Actor("Thargon") },
             { new ActorFDName("thargonswarm"), new Actor("Thargon Swarm") },
             { new ActorFDName("tg_skimmer_01"), new Actor("Thargoid Scavenger") },   // seen
             { new ActorFDName("tg_skimmer_02"), new Actor("Thargoid Scavenger") },
             { new ActorFDName("tg_skimmer_03"), new Actor("Thargoid Scavenger") },
             { new ActorFDName("tg_banshee_01"), new Actor("Thargoid Banshee Type 1") },
             { new ActorFDName("tg_banshee_02"), new Actor("Thargoid Banshee Type 2") },
             { new ActorFDName("tg_scavenger"), new Actor("Thargoid Scavenger") },
             { new ActorFDName("titan_hardpoint01"), new Actor("Thargoid Titan") },
             { new ActorFDName("titan_hardpoint02"), new Actor("Thargoid Titan") },   // seen
             { new ActorFDName("titan_hardpoint03"), new Actor("Thargoid Titan") },
             { new ActorFDName("titan"), new Actor("Titan") },
             { new ActorFDName("glaive"), new Actor("Thargoid Glaive") },        // seen
             { new ActorFDName("scythe"), new Actor("Scythe") },
             { new ActorFDName("scout_cargo"), new Actor("Cargo Scout") },

             { new ActorFDName("unknownsaucer"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_a"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_b"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_c"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_d"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_e"), new Actor("Thargoid") },  // seen
             { new ActorFDName("unknownsaucer_f"), new Actor("Thargoid") },
             { new ActorFDName("unknownsaucer_h"), new Actor("Thargoid") },  // seen

             { new ActorFDName("guardian_sentinel"), new Actor("Guardian Sentinel") },

             { new ActorFDName("ps_turretbasemedium02_6m"), new Actor("Turret medium 2-6-M") },
             { new ActorFDName("ps_turretbasesmall_3m"), new Actor("Turret Small 3 M") },
             { new ActorFDName("ps_turretbasemedium_skiff_6m"), new Actor("Turret Medium 6 M") },

             { new ActorFDName("poi_turretbasea"), new Actor("Turret Base") },
             { new ActorFDName("poi_turretbunkera"), new Actor("Turret Bunker A") },
             { new ActorFDName("poi_turretplatforma"), new Actor("Turret Platform A") },

             { new ActorFDName("mega_defences"), new Actor("Mega Defences") },
             { new ActorFDName("mega_turretbunkera"), new Actor("Mega Turret Type A") },
             { new ActorFDName("mega_turretplatforma"), new Actor("Mega Platform Type A") },
             { new ActorFDName("mega_turretplatformb"), new Actor("Mega Platform Type B") },

             { new ActorFDName("scout"), new Actor("Thargoid Scout") },
             { new ActorFDName("scout_q"), new Actor("Thargoid Scout (Q)") },
             { new ActorFDName("scout_hq"), new Actor("Thargoid Scout (HQ)") },
             { new ActorFDName("scout_nq"), new Actor("Thargoid Scout (NQ)") },

             { new ActorFDName("planetporta"), new Actor("Planet Port") },
             { new ActorFDName("planetportb"), new Actor("Planet Port") },
             { new ActorFDName("planetportc"), new Actor("Planet Port") },
             { new ActorFDName("planetportd"), new Actor("Planet Port") },
             { new ActorFDName("planetporte"), new Actor("Planet Port") },
             { new ActorFDName("planetportf"), new Actor("Planet Port") },
             { new ActorFDName("planetportg"), new Actor("Planet Port") },           // seen g, presuming at least a-f
             { new ActorFDName("planetporth"), new Actor("Planet Port") },
             { new ActorFDName("planetporti"), new Actor("Planet Port") },           // seen up to i now july 24
             { new ActorFDName("planetportj"), new Actor("Planet Port") },           // seen may 25

             { new ActorFDName("diamondback_taxi"), new Actor("Taxi (Diamondback)") },
             { new ActorFDName("viper_taxi"), new Actor("Taxi (Viper)") },
             { new ActorFDName("adder_taxi"), new Actor("Taxi (Adder)") },
             { new ActorFDName("vulture_taxi"), new Actor("Taxi (Vulture)") },

             { new ActorFDName("oneillcylinder"), new Actor("O'Neill Cylinder") },
             { new ActorFDName("oneillorbis"), new Actor("O'Neill Orbis") },
             { new ActorFDName("outpostcivilian"), new Actor("Civilian Outpost") },
             { new ActorFDName("outpostindustrial"), new Actor("Industrial Outpost") },
             { new ActorFDName("outpostcriminal"), new Actor("Criminal Outpost") },
             { new ActorFDName("outpostcommercial"), new Actor("Commercial Outpost") },
             { new ActorFDName("outpostscientific"), new Actor("Scientific Outpost") },
             { new ActorFDName("outpostmilitary"), new Actor("Military Outpost") },
             { new ActorFDName("outpost_weaponsplatform_depot"), new Actor("Weapons Platform in depot") },
             { new ActorFDName("megashipdockrehab"), new Actor("Mega Ship Prison") },
             { new ActorFDName("megashipdocka"), new Actor("Mega Ship Dock A") },
             { new ActorFDName("asteroidbase"), new Actor("Asteroid Base") },
             { new ActorFDName("bernalsphere"), new Actor("Station") },
             { new ActorFDName("coriolis"), new Actor("Coriolis Station") },

             { new ActorFDName("carrierdocka"), new Actor("Carrier Dock A") },
             { new ActorFDName("carrierdockb"), new Actor("Carrier Dock B") },
             { new ActorFDName("carrierdocka_squadron"), new Actor("Squadron Carrier Dock A") },
             { new ActorFDName("carrierdockb_squadron"), new Actor("Squadron Carrier Dock B") },

             { new ActorFDName("federation_capitalship"), new Actor("Federation Capital Ship") },

             { new ActorFDName("lizryder"), new Actor("Engineer Liz Ryder") },
             { new ActorFDName("heratani"), new Actor("Engineer Hera Tani") },
             { new ActorFDName("felicityfarseer"), new Actor("Engineer Felicity Farseer") },
             { new ActorFDName("thesarge"), new Actor("Engineer The Sarge") },

             { new ActorFDName("thedweller"), new Actor("The Dweller") },

             { new ActorFDName("$name_ax_military"), new Actor("AX Military Pilot") },       // seen in NPC texts

             { new ActorFDName("ms_dockablecoreb_twinhull"), new Actor("Dockable Twinhull") },
        };


    }
}

