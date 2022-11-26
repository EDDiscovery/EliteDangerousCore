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
 *
 */

using System;

namespace EliteDangerousCore.UIEvents
{
    public class UIBodyName : UIEvent
    {
        public UIBodyName(DateTime time, bool refresh) : base(UITypeEnum.BodyName, time, refresh)
        {
        }

        public UIBodyName(string status, DateTime time, bool refresh) : this(time, refresh)
        {
            BodyName = status;
        }

        public string BodyName { get; private set; } // may be null, meaning not known

        public override string ToString()
        {
            return $"{BodyName}";
        }

    }
}
