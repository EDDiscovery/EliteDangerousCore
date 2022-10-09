/*
 * Copyright © 2022-2022 EDDiscovery development team
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
 
using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public class CarrierStats
    {
        // from journalcarrier, a copy of the carrier stats journal record. 
        public CarrierState State { get; private set; } = new CarrierState();

        // from CarrierBuy, CarrierJump
        public ISystem StarSystem { get; private set; }                           // where is it?. May have position
        public string Body { get; private set; }                                 // may be null if not known
        public int BodyID { get; private set; } = -1;                             // may be unknown, especially when first bought                        
        public long SystemAddress { get; private set; }                          // its ID

        // CarrierBuy

        public long Cost { get; private set; }                                   // cost to buy
        public string Variant { get; private set; }                              // and its variant type

        // from CarrierJumpRequest

        public string NextStarSystem { get; private set; }                       // null if not jumping
        public long NextSystemAddress { get; private set; }
        public string NextBody { get; private set; }
        public int NextBodyID { get; private set; }
        public DateTime? EstimatedJumpTimeUTC { get; private set; }               // we set this up from carrier jump request

        public bool IsJumping { get { return EstimatedJumpTimeUTC.HasValue; } }
        public TimeSpan TimeTillJump { get { return IsJumping ? (EstimatedJumpTimeUTC.Value - DateTime.UtcNow) : new TimeSpan(0); } }

        const int CarrierNormalJumpTimeSeconds = 15*60;            // normal jump time..
        const int CarrierJumpTimeMarginSeconds = 60;         // Lets leave a margin so we let the normal events pass thru first

        // from CarrierDecommision

        public DateTime? DecommisionTimeUTC { get; private set; }                // null if not, else time of decomission
        public long DecommissionRefund { get; private set; }                      // if State.PendingDecomission, value

        public bool IsDecommisioned { get { return DecommisionTimeUTC.HasValue && DateTime.UtcNow >= DecommisionTimeUTC; } }
        public bool IsDecommisioning { get { return DecommisionTimeUTC.HasValue && DateTime.UtcNow < DecommisionTimeUTC; } }

        public class Jumps
        {
            public ISystem StarSystem { get; set; }
            public string Body { get; set; }        // may be null, may be empty.
            public int BodyID { get; set; }
            public DateTime JumpTime { get; set; }
        }

        public List<Jumps> JumpHistory { get; private set; } = new List<Jumps>();

        public string LastJumpText(int offset)          // 0 = last one, 1 = one before last, etc
        {
            int i = JumpHistory.Count - 1 - offset;
            if (i >= 0)
            {
                var it = JumpHistory[i];
                return it.StarSystem.Name + (it.Body.HasChars() ? ": " + it.Body : "");
            }
            else
                return "";
        }

        // From CarrierTradeOrder
        public List<JournalCarrierTradeOrder.TradeOrder> TradeOrders { get; private set; } = new List<JournalCarrierTradeOrder.TradeOrder>();

        public void Process(JournalEntry je, bool onfootfleetcarrier)
        {
            if (je is ICarrierStats)
            {
                ((ICarrierStats)je).UpdateCarrierStats(this,onfootfleetcarrier);
            }

            CheckCarrierJump(je.EventTimeUTC);
        }

        public bool CheckCarrierJump(DateTime curtime)
        {
            if (EstimatedJumpTimeUTC.HasValue && EstimatedJumpTimeUTC.Value.AddSeconds(CarrierJumpTimeMarginSeconds) < curtime)
            {
                System.Diagnostics.Debug.WriteLine($"{Environment.NewLine} Carrier presumed jumped at {EstimatedJumpTimeUTC} curtime {curtime} to {NextStarSystem}");
                StarSystem = new SystemClass(NextStarSystem);       // don't have position..
                SystemAddress = NextSystemAddress;
                Body = NextBody;
                BodyID = NextBodyID;
                JumpHistory.Add(new Jumps() { StarSystem = StarSystem, Body = Body, BodyID = BodyID, JumpTime = EstimatedJumpTimeUTC.Value });
                ClearNextJump();
                return true;
            }
            else
                return false;
        }


        public void Update(JournalCarrierStats j)
        {
            State = new CarrierState(j.State);      // State array is a direct copy of carrier..
        }

        public void Update(JournalCarrierBuy j)
        {
            State = new CarrierState();             // must be a new state, as its a new carrier
            State.CarrierID = j.CarrierID;
            State.Callsign = j.Callsign;
            StarSystem = new SystemClass(j.Location);   // no position
            SystemAddress = j.SystemAddress;
            Cost = j.Price;
            Variant = j.Variant;
            ClearNextJump();            // clear jump
            ClearDecommisioning();      // clear decommisioning
            TradeOrders = new List<JournalCarrierTradeOrder.TradeOrder>();  // reset order list
            JumpHistory = new List<Jumps>();
            JumpHistory.Add(new Jumps() { StarSystem = StarSystem, Body = Body, BodyID = BodyID, JumpTime = j.EventTimeUTC});
        }

        public void Update(JournalCarrierNameChange j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.CarrierID = j.CarrierID;
                State.Callsign = j.Callsign;
                State.Name = j.Name;
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Name Change but no carrier!");
        }

        public void Update(JournalCarrierJumpRequest j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.CarrierID = j.CarrierID;
                NextStarSystem = j.SystemName;
                NextSystemAddress = j.SystemAddress;
                NextBody = j.Body;
                NextBodyID = j.BodyID;
                EstimatedJumpTimeUTC = j.EventTimeUTC.AddSeconds(CarrierNormalJumpTimeSeconds);
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Jump Request but no carrier!");
        }

        public void Update(JournalCarrierJumpCancelled junused)
        {
            ClearNextJump();
        }

        public void Update(JournalCarrierJump j)
        {
            StarSystem = new SystemClass(j.StarSystem, j.StarPos.X, j.StarPos.Y, j.StarPos.Z);                  // set new location with position
            SystemAddress = j.SystemAddress ?? 0;
            Body = NextBody;
            BodyID = NextBodyID;
            JumpHistory.Add(new Jumps() { StarSystem = StarSystem, Body = Body, BodyID = BodyID, JumpTime = j.EventTimeUTC });
            ClearNextJump();
        }

        public void Update(JournalLocation j, bool onfootfleetcarrier)            // odyssey up to patch 13 is writing Location on jump if in ship or on foot
        {
            // if we have a location, and station type is fleet carrier, it jumped
            
            if (NextStarSystem != null && (onfootfleetcarrier || j.StationType.Contains("carrier",StringComparison.InvariantCultureIgnoreCase))) 
            {
                StarSystem = new SystemClass(j.StarSystem, j.StarPos.X, j.StarPos.Y, j.StarPos.Z);                  // set new location with position
                SystemAddress = NextSystemAddress;
                Body = NextBody;
                BodyID = NextBodyID;
                JumpHistory.Add(new Jumps() { StarSystem = StarSystem, Body = Body, BodyID = BodyID, JumpTime = j.EventTimeUTC });
                ClearNextJump();
            }
        }

        public void Update(JournalCarrierDecommission j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.PendingDecommission = true;
                DecommisionTimeUTC = j.ScrapDateTimeUTC;
                DecommissionRefund = j.ScrapRefund;
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Decommission but no carrier!");
        }
        public void Update(JournalCarrierCancelDecommission junused)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                ClearDecommisioning();
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Jump Request but no carrier!");
        }

        public void Update(JournalCarrierDepositFuel j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.FuelLevel = j.Total;
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Deposit Fuel but no carrier!");
        }

        public void Update(JournalCarrierBankTransfer j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.Finance.CarrierBalance = j.CarrierBalance;
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Bank transfer but no carrier!");
        }
        public void Update(JournalCarrierFinance j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.Finance = new CarrierState.FinanceClass(j.Finance);
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier finance but no carrier!");
        }
        public void Update(JournalCarrierDockingPermission j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.DockingAccess = j.DockingAccess;
                State.AllowNotorious = j.AllowNotorious;
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Docking perm but no carrier!");
        }
        public void Update(JournalCarrierCrewServices j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                if (State.Services == null) // if no array, make it
                    State.Services = new List<CarrierState.ServicesClass>(); // checked this by making carrier states not set Crew.

                CarrierState.ServicesClass cc = State.Services.Find(x => x.CrewRole.Equals(j.CrewRole,StringComparison.InvariantCultureIgnoreCase));        
                if ( cc == null )       // if no CrewRoll, make one
                {
                    cc = new CarrierState.ServicesClass() { CrewRole = j.CrewRole, CrewName=  j.CrewName };
                    State.Services.Add(cc);
                }

                if (j.Operation.Equals("activate", StringComparison.InvariantCultureIgnoreCase))
                {
                    cc.Enabled = cc.Activated = true;
                }
                else if (j.Operation.Equals("deactivate", StringComparison.InvariantCultureIgnoreCase))
                {
                    cc.Enabled = cc.Activated = false;
                }
                else if (j.Operation.Equals("pause", StringComparison.InvariantCultureIgnoreCase))
                {
                    cc.Enabled = false;
                }
                else if (j.Operation.Equals("resume", StringComparison.InvariantCultureIgnoreCase))
                {
                    cc.Enabled = true;
                }
                else if (j.Operation.Equals("replace", StringComparison.InvariantCultureIgnoreCase))
                {
                    cc.CrewName = j.CrewName;   // set crewname
                }
                else
                    System.Diagnostics.Debug.WriteLine($"Crew services unknown action {j.Operation}");
            }
            else
                System.Diagnostics.Debug.WriteLine($"Crew services but no carrier!");
        }
        public void Update(JournalCarrierShipPack j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                if (State.ShipPacks == null) // if no array, make it
                    State.ShipPacks = new List<CarrierState.PackClass>(); 

                CarrierState.PackClass sp = State.ShipPacks.Find(x => x.PackTheme.Equals(j.PackTheme, StringComparison.InvariantCultureIgnoreCase) &&
                                                                       x.PackTier == j.PackTier);

                bool buy = j.Operation.Equals("buypack", StringComparison.InvariantCultureIgnoreCase);
                bool restock = j.Operation.Equals("restockpack", StringComparison.InvariantCultureIgnoreCase);

                if (sp == null && (buy | restock))       // if not there
                {
                    sp = new CarrierState.PackClass() { PackTheme = j.PackTheme, PackTier = j.PackTier };
                    State.ShipPacks.Add(sp);
                }

                if (!buy && !restock)                   // if not buy/restock, remove
                {
                    if (j.Refund.HasValue)     // should do of course
                        State.Finance.CarrierBalance += j.Refund.Value;

                    if (sp != null)
                        State.ShipPacks.Remove(sp);
                }
                else
                {
                    if (j.Cost.HasValue)
                        State.Finance.CarrierBalance -= j.Cost.Value;
                }
            }
            else
                System.Diagnostics.Debug.WriteLine($"Ship pack but no carrier!");
        }
        public void Update(JournalCarrierModulePack j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                if (State.ModulePacks == null) // if no array, make it
                    State.ModulePacks = new List<CarrierState.PackClass>();

                CarrierState.PackClass mp = State.ModulePacks.Find(x => x.PackTheme.Equals(j.PackTheme, StringComparison.InvariantCultureIgnoreCase) &&
                                                                       x.PackTier == j.PackTier);

                bool buy = j.Operation.Equals("buypack", StringComparison.InvariantCultureIgnoreCase);
                bool restock = j.Operation.Equals("restockpack", StringComparison.InvariantCultureIgnoreCase);

                if (mp == null && (buy | restock))       // if not there
                {
                    mp = new CarrierState.PackClass() { PackTheme = j.PackTheme, PackTier = j.PackTier };
                    State.ModulePacks.Add(mp);
                }

                if (!buy && !restock)                   // if not buy/restock, remove
                {
                    if ( j.Refund.HasValue)     // should do of course
                        State.Finance.CarrierBalance += j.Refund.Value;
                    if (mp != null)
                        State.ModulePacks.Remove(mp);
                }
                else
                {
                    if (j.Cost.HasValue)
                        State.Finance.CarrierBalance -= j.Cost.Value;
                }
            }
            else
                System.Diagnostics.Debug.WriteLine($"Ship pack but no carrier!");
        }

        public void Update(JournalCarrierTradeOrder j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                JournalCarrierTradeOrder.TradeOrder to = TradeOrders.Find(x => x.Equals(j.Order));      // have we got one?

                if (to != null)
                    TradeOrders.Remove(to);

                if (j.CancelTrade != true)
                {
                    TradeOrders.Add(new JournalCarrierTradeOrder.TradeOrder(j.Order));            // even if its the same, we add a copy again, as we can have repeats
                }
            }
            else
                System.Diagnostics.Debug.WriteLine($"Trade order but no carrier!");
        }

        #region Helpers
        private void ClearDecommisioning()
        {
            State.PendingDecommission = false;
            DecommisionTimeUTC = null;
            DecommissionRefund = 0;
        }

        private void ClearNextJump()
        {
            NextStarSystem = NextBody = null;       // clear next info
            NextBodyID = -1;
            NextSystemAddress = 0;
            EstimatedJumpTimeUTC = null;
        }

        #endregion

    }
}
