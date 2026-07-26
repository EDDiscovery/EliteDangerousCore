/*
 * Copyright 2026-2026 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License"); you may not use this
 * file except in coSmpliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using QuickJSON;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    // Vehicles, ship/lander/srv/fighter/suit
    public class VehicleFDName : FDName
    {
        public VehicleFDName() : base()
        {
            SetVT();
        }

        public VehicleFDName(string fdname) : base(fdname)
        {
            SetVT();
        }

        public VehicleFDName(QuickJSON.JToken token) : base(token)
        {
            SetVT();
        }

        public enum VehicleTypeEnum { Unknown, Ship, Taxi, SRV, Fighter, Lander, Suit, Actor }    // Actor is not a vehicle, but class derives from it

        public VehicleTypeEnum VehicleType { get; protected set; }

        public bool IsSRVOrFighterOrLander => VehicleType == VehicleTypeEnum.SRV || VehicleType == VehicleTypeEnum.Fighter || VehicleType == VehicleTypeEnum.Lander;
        public bool IsShipSRVFighterLander => VehicleType == VehicleTypeEnum.Ship || VehicleType == VehicleTypeEnum.Fighter || VehicleType == VehicleTypeEnum.SRV || VehicleType == VehicleTypeEnum.Lander;

        private void SetVT()
        {
            string shipfdname = "";     // TBD
            if (shipfdname.ContainsIIC("_taxi"))
                VehicleType = VehicleTypeEnum.Taxi;
            else if (shipfdname.EqualsIIC("testbuggy") || shipfdname.ContainsIIC("_SRV"))
                VehicleType = VehicleTypeEnum.SRV;
            else if (shipfdname.ContainsIIC("suit"))
                VehicleType = VehicleTypeEnum.Suit;
            else if (shipfdname.ContainsIIC("lander"))
                VehicleType = VehicleTypeEnum.Lander;
            else if (shipfdname.ContainsIIC("_fighter"))
                VehicleType = VehicleTypeEnum.Fighter;
            else
                VehicleType = VehicleTypeEnum.Ship;
        }

        public string StrLowerNoTaxi()
        {
            return ToLower().Replace("_taxi", "");
        }

        public new VehicleFDName Clone()
        {
            return new VehicleFDName(this.Str());
        }

        public static VehicleFDName Normalise(string fdname, out string name, JournalEntry ev, bool allownull = false)
        {
            if (fdname.IsEmpty())
            {
                if (allownull)
                {
                    name = null;
                    return null;
                }
                else
                {
                    name = "Unknown Ship";
                    BaseUtils.Debugger.TraceBreak($"*** Missing Ship {ev?.EventTimeUTC} {ev?.EventTypeStr}");
                    return new VehicleFDName("Unknown Ship");
                }
            }
            else
            {
                var ret = new VehicleFDName(fdname);

                if ( ret.VehicleType == VehicleTypeEnum.Suit)
                { 
                    var suit = ItemData.GetSuit(new SuitFDName(ret.Str()));
                    name = suit?.Name ?? ("Unknown Suit " + fdname);
                }
                else
                {
                    var ship = ItemData.GetShipProperties(ret);
                    if (ship == null)
                    {
                        BaseUtils.Debugger.TraceBreak($"*** Unknown ship: `{fdname}` {ev?.EventTimeUTC} {ev?.EventTypeStr}");
                        name = "Unknown Ship " + fdname;
                    }
                    else
                        name = ship.Name;
                }

                return ret;
            }
        }
    }

    public class ShipFDNameEqualityComparer : IEqualityComparer<VehicleFDName>
    {
        public bool Equals(VehicleFDName left, VehicleFDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(VehicleFDName obj)
        {
            return obj.GetHashCode();
        }
    }

    // Must be a suit

    public class SuitFDName : VehicleFDName
    {
        public SuitFDName() : base()
        {
        }

        public SuitFDName(string fdname) : base(fdname)
        {
        }

        public SuitFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static new SuitFDName Normalise(string fdname, out string name, JournalEntry ev, bool allownull = false)
        {
            var ret = VehicleFDName.Normalise(fdname, out name, ev, allownull);
            if (ret.VehicleType != VehicleTypeEnum.Suit)
                System.Diagnostics.Debug.WriteLine($"*** Suit not recognised properly {fdname}");
            return new SuitFDName(ret.Str());
        }
    }

    public class SuitFDNameEqualityComparer : IEqualityComparer<VehicleFDName>
    {
        public bool Equals(VehicleFDName left, VehicleFDName right)
        {
            return left.Equals(right);
        }

        public int GetHashCode(VehicleFDName obj)
        {
            return obj.GetHashCode();
        }
    }

    public class VehicleActorSuitFDName : SuitFDName
    {
        public VehicleActorSuitFDName() : base()
        {
        }
        public VehicleActorSuitFDName(string fdname, VehicleTypeEnum e) : base(fdname)
        {
            VehicleType = e;
        }

        public VehicleActorSuitFDName(QuickJSON.JToken token) : base(token)
        {
            var ac = ItemData.GetActor(new FDName(Str()));
            if (ac != null)
            {
                VehicleType = VehicleTypeEnum.Actor;
            }
        }

        public static new VehicleActorSuitFDName Normalise(string fdname, out string name, JournalEntry ev, bool allownull = false)
        {
            var ac = ItemData.GetActor(new FDName(fdname));
            if (ac != null)
            {
                name = ac.Name;
                return new VehicleActorSuitFDName(fdname, VehicleTypeEnum.Actor);
            }
            else
            {
                var ret = VehicleFDName.Normalise(fdname, out name, ev, allownull);
                return new VehicleActorSuitFDName(ret.Str(), ret.VehicleType);
            }
        }

        public static new VehicleActorSuitFDName Empty => new VehicleActorSuitFDName("", VehicleTypeEnum.Unknown);
    }
}
