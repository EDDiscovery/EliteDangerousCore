/*
 * Copyright © 2016-2022 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public class StoredShip : IEquatable<StoredShip>
    {
        public ulong ShipID { get; set; }      // both
        public string ShipType { get; set; } // both
        public string ShipType_Localised { get; set; } // both
        public string Name { get; set; }     // Both
        public long Value { get; set; }      // both
        public bool Hot { get; set; }        // both

        public string StarSystem { get; set; }   // remote only and when not in transit, but filled in for local
        public long ShipMarketID { get; set; }   //remote
        public long TransferPrice { get; set; }  //remote
        public long TransferTime { get; set; }   //remote
        public bool InTransit { get; set; }      //remote, and that means StarSystem is not there.

        public string StationName { get; set; }  // local only, null otherwise, not compared due to it being computed
        public string ShipTypeFD { get; set; } // both, computed
        public System.TimeSpan TransferTimeSpan { get; set; }        // computed
        public string TransferTimeString { get; set; } // computed

        public void Normalise()
        {
            ShipTypeFD = JournalFieldNaming.NormaliseFDShipName(ShipType);
            ShipType = JournalFieldNaming.GetBetterShipName(ShipTypeFD);
            ShipType_Localised = ShipType_Localised.Alt(ShipType);
            TransferTimeSpan = new System.TimeSpan((int)(TransferTime / 60 / 60), (int)((TransferTime / 60) % 60), (int)(TransferTime % 60));
            TransferTimeString = TransferTimeSpan.ToString();
        }

        public bool Equals(StoredShip other)
        {
            return ShipID == other.ShipID && string.Compare(ShipType, other.ShipType) == 0 &&
                        string.Compare(ShipType_Localised, other.ShipType_Localised) == 0 && string.Compare(Name, other.Name) == 0 &&
                        Value == other.Value && Hot == other.Hot &&
                        string.Compare(StarSystem, other.StarSystem) == 0 && ShipMarketID == other.ShipMarketID && TransferPrice == other.TransferPrice &&
                        InTransit == other.InTransit;
        }
    }
}
