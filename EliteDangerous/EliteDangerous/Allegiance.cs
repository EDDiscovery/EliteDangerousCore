/*
 * Copyright © 2023-2023 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System;

namespace EliteDangerousCore
{
    public class AllegianceDefinitions
    {
        public enum Allegiance
        {
            Unknown = 0,
            None,   // addition, for fleet carriers etc
            Federation,
            Empire,
            Independent,
            Alliance,
            Guardian,
            Thargoid,
            PilotsFederation,
        }

        public static Allegiance ToEnum(string englishname)
        {
            if (Enum.TryParse(englishname, true, out Allegiance value))
            {
                return value;
            }
            else
                return Allegiance.Unknown;
        }
    }
}


