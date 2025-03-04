/*
 * Copyright © 2022-2024 EDDiscovery development team
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
 
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class CarrierStats
    {
        // from journalcarrier, a copy of the carrier stats journal record. 
        // use State.HaveCarrier to determine carrier state

        #region Vars
        public CarrierState State { get; private set; } = new CarrierState();

        // from CarrierBuy, CarrierJump
        public ISystem StarSystem { get; private set; } = new SystemClass("Unknown");  // where is it?. always set
        public string Body { get; private set; } = "";                           // normally set by carrierjump/carrierbuy
        public int BodyID { get; private set; } = 0;                             
        public long SystemAddress { get; private set; }                          // its ID

        public string CurrentPositionText { get { return StarSystem.Name + (Body.Equals(StarSystem.Name, StringComparison.InvariantCultureIgnoreCase) || Body.IsEmpty() ? "" : (": " + Body.ReplaceIfStartsWith(StarSystem.Name).Trim())); } }

        // CarrierBuy
        public long Cost { get; private set; }                                   // cost to buy
        public string Variant { get; private set; }                              // and its variant type

        // CarrierModulePack/ShipPack (not CarrierStats does not carry cost but these do, so cache). key is PackTheme:PackTier
        public Dictionary<string, long> PackCost { get; set; } = new Dictionary<string, long>();
        static public string PackCostKey(string name, int tier) { return name + ":" + tier.ToStringInvariant(); }
        static public string PackCostKey(CarrierState.PackClass pc) { return pc.PackTheme + ":" + pc.PackTier.ToStringInvariant(); }

        // from CarrierJumpRequest
        public string NextStarSystem { get; private set; }                       // null if not jumping
        public long NextSystemAddress { get; private set; }
        public string NextBody { get; private set; }            // null if not jumping. If jumping, its empty or the body name
        public int NextBodyID { get; private set; }             // 0 or the body id
        public DateTime? EstimatedJumpTimeUTC { get; private set; }               // we set this up from carrier jump request

        public bool IsJumping { get { return EstimatedJumpTimeUTC.HasValue; } }
        public TimeSpan TimeTillJump { get { return IsJumping ? (EstimatedJumpTimeUTC.Value - DateTime.UtcNow) : new TimeSpan(0); } }

        public string NextDestinationText { get { return IsJumping ? (NextStarSystem + (NextBody.Equals(NextStarSystem, StringComparison.InvariantCultureIgnoreCase) ? "" : (": " + NextBody.ReplaceIfStartsWith(NextStarSystem).Trim()))) : ""; } }


        const int CarrierNormalJumpTimeSeconds = 15*60;            // normal jump time..
        const int CarrierJumpTimeMarginSeconds = 60;         // Lets leave a margin so we let the normal events pass thru first

        // from CarrierDecommision

        public DateTime? DecommisionTimeUTC { get; private set; }                // null if not, else time of decomission
        public long DecommissionRefund { get; private set; }                      // if State.PendingDecomission, value

        public bool IsDecommisioned { get { return DecommisionTimeUTC.HasValue && DateTime.UtcNow >= DecommisionTimeUTC; } }
        public bool IsDecommisioning { get { return DecommisionTimeUTC.HasValue && DateTime.UtcNow < DecommisionTimeUTC; } }

        public class Jumps
        {
            public Jumps(ISystem sys, string body, int bodyid, DateTime jumptime)
            {
                System.Diagnostics.Debug.Assert(body != null && sys != null && sys.Name != null);
                StarSystem = sys; Body = body;BodyID = bodyid;JumpTime = jumptime;
            }

            public ISystem StarSystem { get; private set; }
            public string Body { get; private set; }        // may be null, may be empty.
            public int BodyID { get; private set; }
            public DateTime JumpTime { get; private set; }
            public string PositionText { get { return StarSystem.Name + (Body.IsEmpty() || Body.Equals(StarSystem.Name, StringComparison.InvariantCultureIgnoreCase) ? "" : (": " + Body)); } }

            public void SetSystem(ISystem sys)
            {
                StarSystem = sys;
            }
        }

        public List<Jumps> JumpHistory { get; private set; } = new List<Jumps>();

        public string LastJumpText(int offset)          // 0 = last one, 1 = one before last, etc
        {
            int i = JumpHistory.Count - 1 - offset;
            if (i >= 0)
            {
                var it = JumpHistory[i];
                return it.PositionText;
            }
            else
                return null;
        }


        public class LedgerEntry
        {
            public LedgerEntry(JournalEntry r, ISystem starsystem, string body, long balance, string notes)
            {
                JournalEntry = r; StarSystem = starsystem; Body = body; Balance = balance; Notes = notes;
            }

            public ISystem StarSystem { get; set; }
            public string Body { get; set; }
            public JournalEntry JournalEntry { get; set; }
            public long Balance { get; set; }
            public string Notes { get; set; }
        }

        public List<LedgerEntry> Ledger { get; private set; } = new List<LedgerEntry>();       // in add order, ascending time

        // From CarrierTradeOrder
        public List<JournalCarrierTradeOrder.TradeOrder> TradeOrders { get; private set; } = new List<JournalCarrierTradeOrder.TradeOrder>();

        #endregion

        #region Process

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
                //System.Diagnostics.Debug.WriteLine($"Carrier presumed jumped at {EstimatedJumpTimeUTC} curtime {curtime} to {NextStarSystem}");
                StarSystem = new SystemClass(NextStarSystem);       // don't have position..
                SystemAddress = NextSystemAddress;
                Body = NextBody;
                BodyID = NextBodyID;
                JumpHistory.Add(new Jumps(StarSystem, Body, BodyID, EstimatedJumpTimeUTC.Value));
                ClearNextJump();
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Journal Events

        public void Update(JournalCarrierStats j)
        {
            State = new CarrierState(j.State);      // State array is a direct copy of carrier..

            ////debug
            //var si = State.GetService(JournalCarrierCrewServices.ServiceType.BlackMarket);
            //if (si != null) si.Activated = false;
            //si = State.GetService(JournalCarrierCrewServices.ServiceType.Exploration);
            //if (si != null) si.Enabled = false;
            //// end debug

            if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
            {
                Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance,""));
            }
        }

        public void Update(JournalCarrierBuy j)
        {
            State = new CarrierState();             // must be a new state, as its a new carrier
            State.CarrierID = j.CarrierID;
            State.Callsign = j.Callsign;
            StarSystem = new SystemClass(j.Location);   // no position
            Body = j.Location;                     // body is same as system
            BodyID = 0;
            SystemAddress = j.SystemAddress;
            Cost = j.Price;
            Variant = j.Variant;
            ClearNextJump();            // clear jump
            ClearDecommisioning();      // clear decommisioning
            TradeOrders = new List<JournalCarrierTradeOrder.TradeOrder>();  // reset order list
            JumpHistory = new List<Jumps>();
            JumpHistory.Add(new Jumps(StarSystem, Body, BodyID, j.EventTimeUTC));
            Ledger.Add(new LedgerEntry(j, StarSystem, Body, 0, ""));

            // DecommisionTimeUTC = new DateTime(2022, 12, 12, 0, 0, 0); 
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
                SetCarrierJump(j.SystemName, j.SystemAddress, j.Body, j.BodyID, j.EventTimeUTC,j.DepartureTime);
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Jump Request but no carrier!");
        }

        public void SetCarrierJump(string system, long sysaddr, string body, int bodyid, DateTime utcjt, DateTime? esttime)
        {
            System.Diagnostics.Debug.Assert(body != null && system != null);
            NextStarSystem = system;
            NextSystemAddress = sysaddr;
            NextBody = body;
            NextBodyID = bodyid;
            EstimatedJumpTimeUTC = esttime ?? utcjt.AddSeconds(CarrierNormalJumpTimeSeconds);
        }

        public void Update(JournalCarrierJumpCancelled junused)
        {
            ClearNextJump();
        }

        public void Update(JournalCarrierJump j)
        {
            StarSystem = new SystemClass(j.StarSystem, null, j.StarPos.X, j.StarPos.Y, j.StarPos.Z);                  // set new location with position
            SystemAddress = j.SystemAddress ?? 0;
            Body = NextBody ?? j.StarSystem;        // you should always have a nextbody, but if debugging.. 
            BodyID = NextBodyID;
            JumpHistory.Add(new Jumps(StarSystem, Body, BodyID, j.EventTimeUTC));
            ClearNextJump();
        }

        public void Update(JournalLocation j, bool onfootfleetcarrier)            
        {
            // odyssey up to patch 13 is writing Location on jump if in ship or on foot
            // if we have a location, and station type is fleet carrier, it jumped

            if (NextStarSystem != null && (onfootfleetcarrier || j.FDStationType == StationDefinitions.StarportTypes.FleetCarrier)) 
            {
                StarSystem = new SystemClass(j.StarSystem, null, j.StarPos.X, j.StarPos.Y, j.StarPos.Z);                  // set new location with position
                SystemAddress = NextSystemAddress;
                Body = NextBody;
                BodyID = NextBodyID;
                JumpHistory.Add(new Jumps(StarSystem, Body, BodyID, j.EventTimeUTC));
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
                Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance,"Player: " + j.PlayerBalance.ToString("N0") ));
            }
            else
                System.Diagnostics.Debug.WriteLine($"Carrier Bank transfer but no carrier!");
        }
        public void Update(JournalCarrierFinance j)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                State.Finance = new CarrierState.FinanceClass(j.Finance);
                if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
                {
                    Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance, "Available: " + j.Finance.AvailableBalance.ToString("N0")));
                }
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
        public void Update(JournalCarrierCrewServices jentry)
        {
            if (State.HaveCarrier)                     // must have a carrier
            {
                // ensure we have a State.Services for the Crew Roll

                if (State.Services == null) // if no array, make it
                    State.Services = new List<CarrierState.ServicesClass>(); // checked this by making carrier states not set Crew.

                CarrierState.ServicesClass srventry = State.Services.Find(x => x.CrewRole.Equals(jentry.CrewRole,StringComparison.InvariantCultureIgnoreCase));        
                if ( srventry == null )       // if no CrewRoll, make one
                {
                    srventry = new CarrierState.ServicesClass() { CrewRole = jentry.CrewRole, CrewName=  jentry.CrewName };
                    State.Services.Add(srventry);
                }

                // on operation type..

                var optype = jentry.GetOperation();

                if (optype == JournalCarrierCrewServices.OperationType.Activate)
                {
                    srventry.Enabled = srventry.Activated = true;
                    srventry.CrewName = jentry.CrewName;

                    var sdata = jentry.GetDataOnService;     // lookup fixed service data for the service

                    if (sdata != null)      // may fail due to not having the right name in the table in the future. If not, update ledger
                    {
                        State.Finance.CarrierBalance -= sdata.InstallCost;
                        Ledger.Add(new LedgerEntry(jentry, StarSystem, Body, State.Finance.CarrierBalance, "+ " + jentry.CrewRole.SplitCapsWordFull()));
                    }

                }
                else if (optype == JournalCarrierCrewServices.OperationType.Deactivate)
                {
                    srventry.Enabled = srventry.Activated = false;
                }
                else if (optype == JournalCarrierCrewServices.OperationType.Pause)
                {
                    srventry.Enabled = false;
                }
                else if (optype == JournalCarrierCrewServices.OperationType.Resume)
                {
                    srventry.Enabled = true;
                }
                else if (optype == JournalCarrierCrewServices.OperationType.Replace)
                {
                    srventry.CrewName = jentry.CrewName;   // set crewname
                }
                else
                    System.Diagnostics.Debug.WriteLine($"Crew services unknown action {jentry.Operation}");
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
                    if (sp != null)
                        State.ShipPacks.Remove(sp);

                    if (j.Refund.HasValue)     // should do of course
                        State.Finance.CarrierBalance += j.Refund.Value;

                    if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
                    {
                        Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance, "- " + j.PackTheme + ":" + j.PackTier.ToString("N0")));
                    }
                }
                else
                {
                    if (j.Cost.HasValue)
                    {
                        State.Finance.CarrierBalance -= j.Cost.Value;
                        if (buy)
                            PackCost[PackCostKey(sp)] = j.Cost.Value;
                    }

                    if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
                    {
                        Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance, (restock ? "Restock: " : "+ ") + j.PackTheme + ":" + j.PackTier.ToString("N0")));
                    }
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
                    if (mp != null)
                        State.ModulePacks.Remove(mp);

                    if (j.Refund.HasValue)     // should do of course
                        State.Finance.CarrierBalance += j.Refund.Value;

                    if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
                    {
                        Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance, "- " + j.PackTheme + ":" + j.PackTier.ToString("N0")));
                    }
                }
                else
                {
                    if (j.Cost.HasValue)
                    {
                        State.Finance.CarrierBalance -= j.Cost.Value;
                        if (buy)
                            PackCost[PackCostKey(mp)] = j.Cost.Value;
                    }

                    if (Ledger.Count == 0 || State.Finance.CarrierBalance != Ledger.Last().Balance)
                    {
                        Ledger.Add(new LedgerEntry(j, StarSystem, Body, State.Finance.CarrierBalance, (restock ? "Restock: " : "+ ") + j.PackTheme + ":" + j.PackTier.ToString("N0")));
                    }
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
                    TradeOrders.Remove(to);     // kill it first

                if (j.CancelTrade != true)      // then possibly add it back in
                {
                    TradeOrders.Add(new JournalCarrierTradeOrder.TradeOrder(j.Order));   
                }
            }
            else
                System.Diagnostics.Debug.WriteLine($"Trade order but no carrier!");
        }

        #endregion

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
