/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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
 */
using System;

namespace EliteDangerousCore.UIEvents
{
    public class UIPips : UIEvent
    {
        public UIPips(DateTime time, bool refresh) : base(UITypeEnum.Pips, time, refresh)
        {
        }

        public UIPips(Pips value, DateTime time, bool refresh) : this( time, refresh)
        {
            Value = value;
        }

        public Pips Value { get; private set; }     // these are in PIPS, not in half pips like the journal gives us.

        public class Pips
        {
            public Pips(int[] p)    // p can be null, p must be 3 long
            {
                if ( p!= null && p.Length == 3 )
                {
                    Systems = p[0] / 2.0;
                    Engines = p[1] / 2.0;
                    Weapons = p[2] / 2.0;
                }
                else
                    Systems = Engines = Weapons = double.MinValue;
            }

            public bool Equal(Pips other)
            {
                return Systems == other.Systems && Engines == other.Engines && Weapons == other.Weapons;
            }

            public bool Valid { get { return Systems > double.MinValue; } }

            public double Systems;
            public double Engines;
            public double Weapons;

        }
    }
}
