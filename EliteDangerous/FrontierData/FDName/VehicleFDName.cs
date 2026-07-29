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

    [System.Diagnostics.DebuggerDisplay("FD {Str()}: {VehicleType}")]
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
        public VehicleFDName(string fdname, VehicleType vh) : base(fdname)
        {
            Type = vh;
        }

        public VehicleFDName(QuickJSON.JToken token) : base(token)
        {
            SetVT();
        }

        public enum VehicleType { Unknown, Ship, Taxi, SRV, Fighter, Lander, Suit, Actor }    // Actor is not a vehicle, but class derives from it

        public VehicleType Type { get; protected set; }

        public bool IsSRVOrFighterOrLander => Type == VehicleType.SRV || Type == VehicleType.Fighter || Type == VehicleType.Lander;
        public bool IsShipSRVFighterLander => Type == VehicleType.Ship || Type == VehicleType.Fighter || Type == VehicleType.SRV || Type == VehicleType.Lander;

        public string StrLowerNoTaxi()
        {
            return ToLower().Replace("_taxi", "");
        }

        public VehicleFDName Clone()
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
                    BaseUtils.Debugger.TraceBreak($"*** Missing Ship {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");
                    return new VehicleFDName("Unknown Ship");
                }
            }
            else
            {
                var ret = new VehicleFDName(fdname);

                if ( ret.Type == VehicleType.Suit)
                { 
                    var suit = ItemData.GetSuit(new SuitFDName(ret.Str()));
                    name = suit?.Name ?? ("Unknown Suit " + fdname);
                }
                else
                {
                    var ship = ItemData.GetShipProperties(ret);
                    if (ship == null)
                    {
                        BaseUtils.Debugger.TraceBreak($"*** Unknown ship: `{fdname}` {ev?.EventTimeUTC.ToStringZulu()} {ev?.EventTypeStr}");
                        name = "Unknown Ship " + fdname;
                    }
                    else
                        name = ship.Name;
                }

                return ret;
            }
        }

        private void SetVT()
        {
            string str = Str();
            if (str.ContainsIIC(ItemData.Taxi_Postfix))
                Type = VehicleType.Taxi;
            else if (str.EqualsIIC(ItemData.SRV_ScarabFDName) || str.ContainsIIC(ItemData.SRV_Postfix))
                Type = VehicleType.SRV;
            else if (str.ContainsIIC(ItemData.LANDER_Prefix))
                Type = VehicleType.Lander;
            else if (str.ContainsIIC(ItemData.Fighter_Postfix))
                Type = VehicleType.Fighter;
            else if (str.ContainsIIC(ItemData.Suit_Postfix))
                Type = VehicleType.Suit;
            else
                Type = VehicleType.Ship;
        }

    }

    //public class ShipFDNameEqualityComparer : IEqualityComparer<VehicleFDName>
    //{
    //    public bool Equals(VehicleFDName left, VehicleFDName right)
    //    {
    //        return left.Equals(right);
    //    }

    //    public int GetHashCode(VehicleFDName obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}

    // Suit only
    public class SuitFDName : VehicleFDName
    {
        public SuitFDName() : base()
        {
        }

        public SuitFDName(string fdname) : base(fdname, VehicleType.Suit)
        {
        }

        public SuitFDName(QuickJSON.JToken token) : base(token)
        {
        }

        public static new SuitFDName Normalise(string fdname, out string name, JournalEntry ev, bool allownull = false)
        {
            var ret = VehicleFDName.Normalise(fdname, out name, ev, allownull);
            if (ret.Type != VehicleType.Suit)
                System.Diagnostics.Debug.WriteLine($"*** Suit not recognised properly {fdname}");
            return new SuitFDName(ret.Str());
        }

        public int GetClass()
        {
            int ci = ToLower().IndexOf("class");
            int classn = ci > 0 ? ToLower().Substring(ci + 5, 1).InvariantParseInt(0) : 0;
            return classn;
        }

    }

    //public class SuitFDNameEqualityComparer : IEqualityComparer<VehicleFDName>
    //{
    //    public bool Equals(VehicleFDName left, VehicleFDName right)
    //    {
    //        return left.Equals(right);
    //    }

    //    public int GetHashCode(VehicleFDName obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}

    // Vehicles, ship/lander/srv/fighter/suit and Actors
    public class VehicleActorSuitFDName : SuitFDName
    {
        public VehicleActorSuitFDName() : base()
        {
        }
        public VehicleActorSuitFDName(string fdname, VehicleType e) : base(fdname)
        {
            Type = e;
        }

        public VehicleActorSuitFDName(QuickJSON.JToken token) : base(token)
        {
            var ac = ItemData.GetActor(new ActorFDName(Str()));
            if (ac != null)
            {
                Type = VehicleType.Actor;
            }
        }

        public static new VehicleActorSuitFDName Normalise(string fdname, out string name, JournalEntry ev, bool allownull = false)
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
                    name = "Unknown Ship/Actor/Suit";
                    BaseUtils.Debugger.TraceBreak($"*** Missing Ship/Actor/Suit {ev?.EventTimeUTC} {ev?.EventTypeStr}");
                    return new VehicleActorSuitFDName(name);
                }
            }
            else
            {
                var ac = ItemData.GetActor(new ActorFDName(fdname));       
                if (ac != null)
                {
                    name = ac.Name;
                    return new VehicleActorSuitFDName(fdname, VehicleType.Actor);
                }
                else
                {
                    var ret = VehicleFDName.Normalise(fdname, out name, ev);
                    return new VehicleActorSuitFDName(ret.Str(), ret.Type);
                }
            }
        }

        public static VehicleActorSuitFDName Empty => new VehicleActorSuitFDName("", VehicleType.Unknown);
    }
}
