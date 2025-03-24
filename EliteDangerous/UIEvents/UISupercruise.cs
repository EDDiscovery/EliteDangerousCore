﻿/*
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
 *
 */
using EliteDangerousCore;
using System;

namespace EliteDangerousCore.UIEvents
{
    public class UISupercruise : UIEvent
    {
        public UISupercruise(DateTime time, bool refresh) : base(UITypeEnum.Supercruise, time, refresh)
        {
        }

        public UISupercruise(bool state, DateTime time, bool refresh) : this(time, refresh)
        {
            Supercruise = state;
        }

        public bool Supercruise { get; private set; }

        public override string ToString()
        {
            return $"{Supercruise}";
        }
    }

    public class UISupercruiseAssist : UIEvent
    {
        public UISupercruiseAssist(DateTime time, bool refresh) : base(UITypeEnum.SupercruiseAssist, time, refresh)
        {
        }

        public UISupercruiseAssist(bool state, DateTime time, bool refresh) : this(time, refresh)
        {
            SupercruiseAssist = state;
        }

        public bool SupercruiseAssist { get; private set; }

        public override string ToString()
        {
            return $"{SupercruiseAssist}";
        }

    }
    public class UISupercruiseOverdrive : UIEvent
    {
        public UISupercruiseOverdrive(DateTime time, bool refresh) : base(UITypeEnum.SupercruiseOverdrive, time, refresh)
        {
        }

        public UISupercruiseOverdrive(bool state, DateTime time, bool refresh) : this(time, refresh)
        {
            SupercruiseOverdrive = state;
        }

        public bool SupercruiseOverdrive { get; private set; }

        public override string ToString()
        {
            return $"{SupercruiseOverdrive}";
        }

    }
}
