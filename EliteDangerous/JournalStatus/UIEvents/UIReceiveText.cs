﻿/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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
    public class UIReceiveText : UIEvent           // used for some ShipTargeted events which blam us
    {
        public UIReceiveText(DateTime time, bool refresh) : base(UITypeEnum.ReceiveText, time, refresh)
        {
        }

        public UIReceiveText(JournalEvents.JournalReceiveText text, DateTime time, bool refresh) : this(time, refresh)
        {
            Text = text;
        }

        public JournalEvents.JournalReceiveText Text { get; private set; }

        public override string ToString()
        {
            return $"{Text.Message} : {Text.MessageLocalised}";
        }

    }
}
