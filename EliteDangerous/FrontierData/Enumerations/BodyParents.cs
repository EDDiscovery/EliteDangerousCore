/*
 * Copyright 2025-2025 EDDiscovery development team
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

using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("BodyParent {Type} {BodyID}")]
    public class BodyParent
    {
        public enum BodyType { Planet, Null, Star, Ring, Unknown };
        [PropertyNameAttribute("Type of node, Null = barycentre, Planet, Star, Ring (Beltcluster)")]
        public BodyType Type { get; set; }
        [PropertyNameAttribute("Frontier body ID")]
        public int BodyID { get; set; }
        [PropertyNameAttribute("Is node a barycentre")]
        public bool IsBarycentre { get { return Type == BodyType.Null; } }
        [PropertyNameAttribute("Is node a star")]
        public bool IsStar { get { return Type == BodyType.Star; } }
        [PropertyNameAttribute("Is node a planet")]
        public bool IsPlanet { get { return Type == BodyType.Planet; } }
        [PropertyNameAttribute("Is node a Beltcluster")]
        public bool IsBeltCluster { get { return Type == BodyType.Ring; } }
        [PropertyNameAttribute("Is node a Beltcluster")]
        public bool IsRing { get { return Type == BodyType.Ring; } }            // back compat

        [PropertyNameAttribute("Properties of the barycentre")]
        public JournalScanBaryCentre Barycentre { get; set; }        // set by star scan system if its a barycentre, for the queries system so you can do Parents[2].Barycentre.SemiMajorAxis

        public BodyParent()
        { }
        public BodyParent(BodyType t, int id)
        { Type = t; BodyID = id; }

        public static bool IsOrbiting(List<BodyParent> items, BodyType t)
        {
            if (items != null)
            {
                foreach (var y in items)
                {
                    if (y.Type == t)
                        return true;
                    if (!y.IsBarycentre)
                        return false;
                }
            }

            return false;
        }
    }
}
