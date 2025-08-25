﻿/*
 * Copyright © 2025-2025 EDDiscovery development team
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
    public class CarrierDefinitions
    {
        public enum CarrierType { FleetCarrier, SquadronCarrier,  UnknownType };
    
        // maps the allegiance fdname to an enum.  Spaces can be in the name ("Pilots Federation") to cope with Spansh
        // If null is passed in, its presumed field is missing and thus Unknown.
        public static CarrierType ToEnum(string fdname)
        {
            if (!fdname.HasChars()) // null or empty
                return CarrierType.FleetCarrier;

            if (Enum.TryParse<CarrierType>(fdname, true, out CarrierType type))
                return type;
            else
                return CarrierType.FleetCarrier;
        }
        public static string ToEnglish(CarrierType al)
        {
            return al.ToString().SplitCapsWordFull();
        }

        public static string ToLocalisedLanguage(CarrierType al)
        {
            return ToEnglish(al).Tx();
        }
    }
}

