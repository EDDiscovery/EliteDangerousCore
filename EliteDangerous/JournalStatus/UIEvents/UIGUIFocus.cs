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
using System;

namespace EliteDangerousCore.UIEvents
{
    public class UIGUIFocus : UIEvent
    {
        public enum Focus
        {
            NoFocus = 0,
            SystemPanel = 1,
            TargetPanel = 2,   
            CommsPanel = 3, // top
            RolePanel = 4,  // bottom
            StationServices = 5,
            GalaxyMap = 6,
            SystemMap = 7,
            Orrey=8,        //3.3
            FSSMode =9, //3.3
            SAAMode =10,//3.3
            Codex =11,//3.3
        }

        public UIGUIFocus(DateTime time, bool refresh) : base(UITypeEnum.GUIFocus, time, refresh)
        {
        }

        public UIGUIFocus(int focus, DateTime time, bool refresh) : this( time, refresh)
        {
            GUIFocus = (Focus)focus;
        }

        public Focus GUIFocus { get; private set; }

        public override string ToString()
        {
            return $"{GUIFocus}";
        }
    }
}
