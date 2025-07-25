/*
 * Copyright © 2022 - 2022 EDDiscovery development team
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

namespace EliteDangerousCore.UIEvents
{
    // will fire when added, or when removed (name="")
    public class UIDestination : UIEvent           
    {
        public UIDestination( DateTime time, bool refresh) : base(UITypeEnum.Destination, time, refresh)
        {
        }

        public UIDestination(string name, int body, long systemaddress, DateTime time, bool refresh) : this(time, refresh)
        {
            Name = name; BodyID = body;SystemAddress = systemaddress;
        }

        public string Name { get; private set; }
        public int BodyID { get; private set; }
        public long SystemAddress { get; private set; }
        public override string ToString()
        {
            return $"{Name}: {BodyID}: {SystemAddress}";
        }

    }
}
