/*
 * Copyright © 2021 EDDiscovery development team
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

        // from CarrierBuy
        public string StarSystem;                           // where is it?
        public string Body;                                 // may be null if not known
        public int BodyID = -1;                             // may be unknown, especially when first bought                        
        public long SystemAddress;                          // its ID
        public long Cost;                                   // cost to buy
        public string Variant;                              // and its variant type

        // from CarrierJumpRequest

        public string NextStarSystem;                       // null if not jumping
        public long NextSystemAddress;
        public string NextBody;
        public int NextBodyID;

        // from CarrierDecommision

        public DateTime? DecommisionTimeUTC;                // null if not, else time of decomission
        public long DecommissionRefund;                      // if State.PendingDecomission, value

        // From CarrierTradeOrder
        public List<JournalCarrierTradeOrder.TradeOrder> TradeOrders { get; private set; } = new List<JournalCarrierTradeOrder.TradeOrder>();

        public void Process(JournalEntry je)
        {
            if (je is ICarrierStats)
            {
                ((ICarrierStats)je).UpdateCarrierStats(this);
            }
        }

        public void Update(JournalCarrierBuy j)
        {
            State = new CarrierState();             // must be a new state, as its a new carrier
            State.CarrierID = j.CarrierID;
            State.Callsign = j.Callsign;
            StarSystem = j.Location;
            SystemAddress = j.SystemAddress;
            Cost = j.Price;
            Variant = j.Variant;
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

        public void Update(JournalCarrierStats j)
        {
            State = new CarrierState(j.State);      // State array is a direct copy of carrier..
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
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Jump Request but no carrier!");
        }

        public void Update(JournalCarrierJumpCancelled junused)
        {
            NextStarSystem = NextBody = null;       // clear next info
            NextBodyID = -1;
            NextSystemAddress = 0;
        }

        public void Update(JournalCarrierJump j)
        {
            StarSystem = j.StarSystem;                  // set new location
            SystemAddress = j.SystemAddress ?? 0;
            Body = NextBody;
            BodyID = NextBodyID;

            NextStarSystem = NextBody = null;
            NextBodyID = -1;
            NextSystemAddress = 0;
        }

        public void Update(JournalLocOrJump j)            // odyssey up to patch 13 is writing FSD Jumps not carrier jumps
        {
            if (NextStarSystem != null)               // if we have a pending jump, we have to assume that the carrier has moved.. 
            {
                StarSystem = NextStarSystem;
                SystemAddress = NextSystemAddress;
                Body = NextBody;
                BodyID = NextBodyID;
            }

            NextStarSystem = NextBody = null;
            NextBodyID = -1;
            NextSystemAddress = 0;
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
        public void Update(JournalCarrierCancelDecommission j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.PendingDecommission = false;
                DecommisionTimeUTC = null;
                DecommissionRefund = 0;
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
                if (State.Crew == null) // if no array, make it
                    State.Crew = new List<CarrierState.CrewClass>(); // checked this by making carrier states not set Crew.

                CarrierState.CrewClass cc = State.Crew.Find(x => x.CrewRole.Equals(j.CrewRole,StringComparison.InvariantCultureIgnoreCase));        
                if ( cc == null )       // if no CrewRoll, make one
                {
                    cc = new CarrierState.CrewClass() { CrewRole = j.CrewRole, CrewName=  j.CrewName };
                    State.Crew.Add(cc);
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
    }
}
