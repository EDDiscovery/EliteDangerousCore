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
    public class SignalDefinitions
    {
        public enum Classification { Station, Installation, NotableStellarPhenomena, ConflictZone, ResourceExtraction, Carrier, USS, Megaship, Other, NavBeacon, Titan, TouristBeacon, Codex };

        // SignalType could be null/empty, in which case its based on SignalName/IsStation/Localised string
        // older entries did not have SignalType.
        public static Classification GetClassification(string signalname, string signaltype, bool isstation, string loc)
        {
            Classification signalclass = Classification.Other;

            if (signaltype.HasChars())
            {
                if (signaltype.Contains("Station", StringComparison.InvariantCultureIgnoreCase) || (signaltype.Equals("Outpost", StringComparison.InvariantCultureIgnoreCase)))
                    signalclass = Classification.Station;
                else if (signaltype.Equals("FleetCarrier", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Carrier;
                else if (signaltype.Equals("Installation", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Installation;
                else if (signaltype.Equals("Megaship", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Megaship;
                else if (signaltype.Equals("Combat", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ConflictZone;
                else if (signaltype.Equals("ResourceExtraction", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ResourceExtraction;
                else if (signaltype.Equals("NavBeacon", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NavBeacon;
                else if (signaltype.Equals("Titan", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Titan;
                else if (signaltype.Equals("TouristBeacon", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.TouristBeacon;
                else if (signaltype.Equals("USS", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.USS;
                else if (signaltype.Equals("Generic", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Other;
                else if (signaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase) && signalname.StartsWith("$Fixed_Event_Life", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (signaltype.Equals("Codex", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.Codex;
                else
                    signalclass = Classification.Other;
            }
            else
            {
                if (isstation == true)          // station flag
                {
                    int dash = signalname.LastIndexOf('-');
                    if (signalname.Length >= 5 && dash == signalname.Length - 4 && char.IsLetterOrDigit(signalname[dash + 1]) && char.IsLetterOrDigit(signalname[dash - 1]))
                        signalclass = Classification.Carrier;
                    else
                        signalclass = Classification.Station;
                }
                else if (signalname.StartsWith("$USS", StringComparison.InvariantCultureIgnoreCase) || signalname.StartsWith("$RANDOM", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.USS;
                else if (signalname.StartsWith("$Warzone", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ConflictZone;
                else if (signalname.StartsWith("$Fixed_Event_Life", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.NotableStellarPhenomena;
                else if (signalname.StartsWith("$MULTIPLAYER_SCENARIO14", StringComparison.InvariantCultureIgnoreCase) || signalname.StartsWith("$MULTIPLAYER_SCENARIO7", StringComparison.InvariantCultureIgnoreCase))
                    signalclass = Classification.ResourceExtraction;
                else if (signalname.Contains("-class"))
                    signalclass = Classification.Megaship;
                else if (loc.Length == 0)      // other types, and old station entries, don't have localisation, so its an installation, put at end of list because other things than installations have no localised name too
                    signalclass = Classification.Installation;
                else
                    signalclass = Classification.Other;
            }

            return signalclass;
        }

    }
}


