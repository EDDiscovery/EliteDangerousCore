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
    public class UICargo : UIEvent
    {
        public UICargo(DateTime time, bool refresh) : base(UITypeEnum.Cargo, time, refresh)
        {
        }

        public UICargo(int value, UIMode.ModeType shiptype, DateTime time, bool refresh) : this(time, refresh)
        {
            Count = value;
            Mode = shiptype;
        }

        public int Count { get; private set; }     // number of items
        public UIMode.ModeType Mode { get; private set; }   // Ship type flags.. per flags
    }
}
