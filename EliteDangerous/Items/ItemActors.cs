/*
 * Copyright © 2016-2021 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 *
 * Data courtesy of Coriolis.IO https://github.com/EDCD/coriolis , data is intellectual property and copyright of Frontier Developments plc ('Frontier', 'Frontier Developments') and are subject to their terms and conditions.
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

        static public Actor GetActor(string fdname, string locname = null)         // actors are things like skimmer drones
        {
            fdname = fdname.ToLowerInvariant();
            if (actors.TryGetValue(fdname, out Actor var))
                return var;
            else
            {
                System.Diagnostics.Debug.WriteLine("*********** Unknown Actor: {{ \"{0}\", new Actor(\"{1}\") }},", fdname, locname ?? fdname.SplitCapsWordFull());
                return null;
            }
        }

        public class Actor : IModuleInfo
        {
            public string Name;
            public Actor(string name) { Name = name; }
        }

        public static Dictionary<string, Actor> actors = new Dictionary<string, Actor>   // DO NOT USE DIRECTLY - public is for checking only
        {
             { "skimmerdrone", new Actor("Skimmer Drone") },
             { "skimmer", new Actor("Skimmer Drone") },
             { "ps_turretbasemedium02_6m", new Actor("Turret medium 2-6-M") },
             { "ps_turretbasesmall_3m", new Actor("Turret Small 3 M") },
             { "ps_turretbasemedium_skiff_6m", new Actor("Turret Medium 6 M") },
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
             { "megashipdockrehab", new Actor("Mega Ship Prison") },
             { "diamondback_taxi", new Actor("Taxi (Diamondback)") },
             { "viper_taxi", new Actor("Taxi (Viper)") },
             { "adder_taxi", new Actor("Taxi (Adder)") },
             { "oneillcylinder", new Actor("O'Neill Cylinder") },
             { "outpostcivilian", new Actor("Civilian Outpost") },
             { "asteroidbase", new Actor("Asteroid Base") },
             { "unknownsaucer", new Actor("Thargoid") },
             { "unknownsaucer_f", new Actor("Thargoid") },
             { "unknownsaucer_h", new Actor("Thargoid") },
             { "thargon", new Actor("Thargon") },
             { "coriolis", new Actor("Coriolis Station") },
             { "carrierdocka", new Actor("Carrier Dock A") },
             { "carrierdockb", new Actor("Carrier Dock B") },
             { "missileskimmer", new Actor("Skimmer Missile") },
             { "bossskimmer", new Actor("Boss Skimmer") }
        };


    }
}

