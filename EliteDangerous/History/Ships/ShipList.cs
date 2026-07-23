/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using BaseUtils;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{currentid} ships {Ships.Count}")]
    public class ShipList
    {
        public int Count => ships.Count;
        public Ship this[int n] => ships.Values.ToArray()[n];
        public IEnumerable<Ship> OwnedSpaceShips() { return ships.Where(x => x.Value.State == Ship.ShipState.Owned && ItemData.IsShip(x.Value.ShipFD)).Select(x => x.Value); }
        public IEnumerable<Ship> SoldDestroyedSpaceShips() { return ships.Where(x => x.Value.State != Ship.ShipState.Owned && ItemData.IsShip(x.Value.ShipFD)).Select(x => x.Value); }

        public ShipModulesInStore StoredModules { get; private set; }       // stored modules

        public bool HaveCurrentShip { get { return currentid != null; } }

        [QuickJSON.JsonIgnore()]
        public Ship CurrentShip { get { return HaveCurrentShip ? ships[currentid] : null; } }

        // IDs have been repeated, need more than just that
        private string Key(FDName fdname, ulong i) { return fdname.ToLower() + ":" + i.ToStringInvariant(); }

        public Ship GetShipByShortName(string sn)
        {
            List<Ship> lst = ships.Values.ToList();
            int index = lst.FindIndex(x => x.ShipShortName.Equals(sn));
            return (index >= 0) ? lst[index] : null;
        }

        public Ship GetShipByNameIdentType(string sn)
        {
            List<Ship> lst = ships.Values.ToList();
            int index = lst.FindIndex(x => x.ShipNameIdentType.Equals(sn));
            return (index >= 0) ? lst[index] : null;
        }

        public Ship GetShipByFullInfoMatch(string sn)
        {
            List<Ship> lst = ships.Values.ToList();
            int index = lst.FindIndex(x => x.ShipFullInfo().IndexOf(sn, StringComparison.InvariantCultureIgnoreCase) != -1);
            return (index >= 0) ? lst[index] : null;
        }
        public Ship GetShip(ulong id)
        {
            List<Ship> lst = ships.Values.ToList();
            int index = lst.FindIndex(x => x.ID == id);
            return (index >= 0) ? lst[index] : null;
        }

        public Ship GetSRVOrLanderOrFighter(ulong id)       // ID and must be a SRV/Fighter/Lander, and this is because in debug logs we could have a repeat over time of the same ID
        {
            List<Ship> lst = ships.Values.ToList();
            int index = lst.FindIndex(x => x.ID == id && ItemData.IsSRVOrFighterOrLander(x.ShipFD));
            return (index >= 0) ? lst[index] : null;
        }

        private ulong newsoldid = ulong.MaxValue / 2;

        public ShipList()
        {
            ships = new Dictionary<string, Ship>();
            StoredModules = new ShipModulesInStore();
            itemlocalisation = new Dictionary<FDName, string>(new FDNameEqualityComparer());
            currentid = null;
        }

        public void Loadout(ulong id, string ship, FDName shipfd, string name, string ident, List<ShipModule> modulelist,
                        long HullValue, long ModulesValue, long Rebuy, double unladenmass, double reservefuelcap, double hullhealth, bool? Hot)
        {
            string sid = Key(shipfd, id);

            //DebuggerHelpers.DP("SL","Loadout {0} {1} {2} {3}", id, ship, name, ident);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            ships[sid] = sm = sm.SetShipDetails(ship, shipfd, name, ident, 0, 0, HullValue, ModulesValue, Rebuy, unladenmass, reservefuelcap, hullhealth, Hot);     // update ship key, make a fresh one if required.

            //DebuggerHelpers.DP("SL","Loadout " + sid);

            Ship newsm = null;       // if we change anything, we need a new clone..

            Dictionary<ShipSlots.Slot, ShipModule> moduleSlots = sm.Modules.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);     // clone

            foreach (ShipModule m in modulelist)
            {
                if (!sm.Contains(m.SlotFD) || !sm.Same(m))  // no slot, or not the same data.. (ignore localised item)
                {
                    if (m.LocalisedItem == null && itemlocalisation.ContainsKey(m.ItemFD))        // if we have a cached localisation, use it
                    {
                        m.LocalisedItem = itemlocalisation[m.ItemFD];
                        //                        DebuggerHelpers.DP("SL","Have localisation for " + m.Item + ": " + m.LocalisedItem);
                    }

                    if (newsm == null)              // if not cloned
                    {
                        newsm = sm.ShallowClone();  // we need a clone, pointing to the same modules, but with a new dictionary
                        ships[sid] = newsm;              // update our record of ship to this
                    }

                    newsm.SetModule(m);                   // update entry only.. rest will still point to same entries
                }

                moduleSlots.Remove(m.SlotFD);
            }

            // Remove modules not in loadout
            if (moduleSlots.Count != 0)                 // any slots remaining from previous build
            {
                List<ShipModule> modulesToRemove = moduleSlots.Values.ToList();

                if (newsm == null)
                {
                    newsm = sm.ShallowClone();
                }

                foreach (ShipModule m in modulesToRemove)
                {
                    //System.Diagnostics.Trace.WriteLine($"Warning: Module {m.Item} in slot {m.Slot} is missing from loadout");
                    newsm = newsm.RemoveModule(m.SlotFD, m.ItemFD);
                }

                ships[sid] = newsm;
            }
            VerifyList();
        }

        public void ModuleInfo(List<ShipModule> ShipModules)
        {
            if (CurrentShip != null && ShipModules != null)
            {
                Ship newsm = null;       // if we change anything, we need a new clone..
                string sid = Key(CurrentShip.ShipFD, CurrentShip.ID);

                foreach (ShipModule shipModule in ShipModules)
                {
                    // if priority has changed

                    if (shipModule.Priority != null && CurrentShip.Modules.TryGetValue(shipModule.SlotFD, out ShipModule sm) && sm.Priority != shipModule.Priority)
                    {
                        if (newsm == null)              // if not cloned
                        {
                            newsm = CurrentShip.ShallowClone();  // we need a clone, pointing to the same modules, but with a new dictionary
                            ships[sid] = newsm;              // update our record of last module list for this ship
                        }

                        sm.SetPriority(shipModule.Priority.Value);
                        Debugger.DP("SL",$"Module Info reset ship priority {sm.ItemFD.Str()} to {shipModule.Priority.Value}");
                    }
                }
            }
        }


        public void LoadGame(ulong id, string ship, FDName shipfd, string name, string ident, double fuellevel, double fueltotal)        // LoadGame..
        {
            string sid = Key(shipfd, id);
            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            ships[sid] = sm = sm.SetShipDetails(ship, shipfd, name, ident, fuellevel, fueltotal);   // this makes a shallow copy if any data has changed..

            //DebuggerHelpers.DP("SL","Load Game " + sid);

            if (ItemData.IsShip(shipfd))
                currentid = sid;
            VerifyList();
        }

        public void LaunchSRV()
        {
            //DebuggerHelpers.DP("SL","Launch SRV");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.SRV);
            VerifyList();
        }

        public void DockSRV()
        {
            //DebuggerHelpers.DP("SL","Dock SRV");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }
        public void DockLander()
        {
            //DebuggerHelpers.DP("SL","Dock Lander");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }

        public void DestroyedSRV()
        {
            //DebuggerHelpers.DP("SL","Destroyed SRV");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }
        public void DestroyedLander()
        {
            //DebuggerHelpers.DP("SL","Destroyed Lander");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }

        public void LaunchFighter(bool pc)
        {
            //DebuggerHelpers.DP("SL","Launch Fighter");
            if (HaveCurrentShip && pc == true)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.Fighter);
            VerifyList();
        }

        public void LaunchLander()
        {
            //DebuggerHelpers.DP("SL","Launch Fighter");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.Lander);
            VerifyList();
        }

        public void RestockVehicle(ulong id, FDName shipfd, string ship, string Loadout)
        {
            string sid = Key(shipfd, id);
            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            ships[sid] = sm.SetShipDetails(ship, shipfd);   // this makes a shallow copy if any data has changed..
            VerifyList();
        }

        public void DockFighter()
        {
            //DebuggerHelpers.DP("SL","Dock Fighter");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }

        public void FighterDestroyed()      // even if NPC controlled, no harm in setting back to none since we must be in ship
        {
            //DebuggerHelpers.DP("SL","Fighter Destroyed");
            if (HaveCurrentShip)
                ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            VerifyList();
        }

        public void Resurrect(bool abandonedship)
        {
            if (HaveCurrentShip)           // resurrect always in ship
            {
                if (abandonedship)
                    ships[currentid] = ships[currentid].Destroyed();
                else
                    ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            }
            VerifyList();
        }

        public void VehicleSwitch(string to)
        {
            if (HaveCurrentShip)
            {
                if (to == "Fighter")
                    ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.Fighter);
                else
                    ships[currentid] = ships[currentid].SetSubVehicle(Ship.SubVehicleType.None);
            }
            VerifyList();
        }

        public void ShipyardSwap(JournalShipyardSwap e, string station, string system)
        {
            if (e.StoreShipId.HasValue)    // if we have an old ship ID (old records do not)
            {
                string oldship = Key(e.StoreOldShipFD, e.StoreShipId.Value);

                if (ships.ContainsKey(oldship))
                {
                    //DebuggerHelpers.DP("SL",oldship + " Swap Store at " + system + ":" + station);
                    ships[oldship] = ships[oldship].Store(station, system);
                }
                else
                {
                    Debugger.DP("SL",e.StoreOldShipFD.Str() + " Cant find to swap");
                }
            }
            else
            {
                Debugger.DP("SL",e.StoreOldShipFD.Str() + " Cant find to swap");
            }

            string sid = Key(e.ShipFD, e.ShipId);           //swap to new ship

            //DebuggerHelpers.DP("SL",sid + " Swap to at " + system);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            sm = sm.SetShipDetails(e.ShipType, e.ShipFD);   // shallow copy if changed
            sm = sm.SwapTo();                               // swap into
            ships[sid] = sm;
            currentid = sid;
            VerifyList();
        }

        public void ShipyardNew(string ship, FDName shipfd, ulong id)
        {
            string sid = Key(shipfd, id);
            //DebuggerHelpers.DP("SL",sid + " New");

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            ships[sid] = sm.SetShipDetails(ship, shipfd); // shallow copy if changed
            currentid = sid;
            VerifyList();
        }

        public void Sell(FDName shipfd, ulong id)
        {
            string sid = Key(shipfd, id);
            if (ships.ContainsKey(sid))       // if we don't have it, don't worry
            {
                //DebuggerHelpers.DP("SL",sid + " Sold ");
                ships[sid] = ships[sid].SellShip();
            }
            else
            {
                Debugger.DP("SL",sid + " can't find to Sell");
            }
            VerifyList();
        }

        public void Transfer(string ship, FDName shipFD, ulong id, string fromsystem, string tosystem, string tostation, DateTime arrivaltime)
        {
            string sid = Key(shipFD, id);
            Ship sm = EnsureShip(sid);              // this either gets current ship or makes a new one.
            sm = sm.SetShipDetails(ship, shipFD);               // set up minimum stuff we know about it
            sm = sm.Transfer(tosystem, tostation, arrivaltime);    // transfer set up
            ships[sid] = sm;
            //DebuggerHelpers.DP("SL",shipFD + " Transfer from " + fromsystem + " to " + tosystem + ":" + tostation + " arrives " + arrivaltime.ToString());
            VerifyList();
        }

        public void Store(FDName shipfd, ulong id, string station, string system)
        {
            string sid = Key(shipfd, id);
            if (ships.ContainsKey(sid))       // if we don't have it, don't worry
            {
                //DebuggerHelpers.DP("SL",sid + " store on buy at " + system);
                ships[sid] = ships[sid].Store(station, system);
            }
            else
            {
                Debugger.DP("SL",sid + " cannot find ship to store on buy");
            }
            VerifyList();
        }

        public void StoredShips(StoredShip[] ships)
        {
            foreach (var i in ships)
            {
                string sid = Key(i.ShipTypeFD, i.ShipID);
                //DebuggerHelpers.DP("SL",sid + " Stored info " + i.StarSystem + ":" + i.StationName + " transit" + i.InTransit);

                Ship sm = EnsureShip(sid);              // this either gets current ship or makes a new one.
                sm = sm.SetShipDetails(i.ShipType, i.ShipTypeFD, i.Name, hot: i.Hot);  // set up minimum stuff we know about it

                if (!i.InTransit)                                 // if in transit, we don't know where it is, ignore
                    sm = sm.Store(i.StationName, i.StarSystem);         // ship is not with us, its stored, so store it.

                this.ships[sid] = sm;
            }
            VerifyList();
        }

        public void SetUserShipName(JournalSetUserShipName e)
        {
            string sid = Key(e.ShipFD, e.ShipID);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            ships[sid] = sm.SetShipDetails(e.Ship, e.ShipFD, e.ShipName, e.ShipIdent); // will clone if data changed..
            VerifyList();
        }

        public void ModuleBuy(JournalModuleBuy e, ISystem sys)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);              // this either gets current ship or makes a new one.

            ships[sid] = sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // shallow copy if changed, store back into array (bug may 24!)

            if (e.StoredItemFD != null)                             // if we stored something
                StoredModules = StoredModules.StoreModule(e.StoredItemFD, e.StoredItem, e.StoredItemLocalised, sys);

            ships[sid] = sm.AddModule(e.Slot, e.SlotFD, e.BuyItem, e.BuyItemFD, e.BuyItemLocalised);      // replace the slot with this

            itemlocalisation[e.BuyItemFD] = e.BuyItemLocalised;       // record any localisations
            if (e.SellItemFD != null)
                itemlocalisation[e.SellItemFD] = e.SellItemLocalised;
            if (e.StoredItemFD != null)
                itemlocalisation[e.StoredItemFD] = e.StoredItemLocalised;

            VerifyList();
        }

        public void ModuleBuyAndStore(JournalModuleBuyAndStore e, ISystem sys)
        {
            StoredModules = StoredModules.StoreModule(e.BuyItemFD, e.BuyItem, e.BuyItemLocalised, sys);
            VerifyList();
        }

        public void ModuleSell(JournalModuleSell e)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.

            sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // shallow copy if changed
            ships[sid] = sm.RemoveModule(e.SlotFD, e.SellItemFD);

            if (e.SellItem.Length > 0)
                itemlocalisation[e.SellItemFD] = e.SellItemLocalised;

            VerifyList();
        }

        public void ModuleSwap(JournalModuleSwap e)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // shallow copy if changed
            ships[sid] = sm.SwapModule(e.FromSlot, e.FromSlotFD, e.FromItem, e.FromItemFD, e.FromItemLocalised,
                                            e.ToSlot, e.ToSlotFD, e.ToItem, e.ToItemFD, e.ToItemLocalised);
            VerifyList();
        }

        public void ModuleStore(JournalModuleStore e, ISystem sys)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.

            sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // shallow copy if changed

            if (e.ReplacementItemFD != null)
                ships[sid] = sm.AddModule(e.Slot, e.SlotFD, e.ReplacementItem, e.ReplacementItemFD, e.ReplacementItemLocalised);
            else
                ships[sid] = sm.RemoveModule(e.SlotFD, e.StoredItemFD);

            StoredModules = StoredModules.StoreModule(e, sys);
            VerifyList();
        }

        public void ModuleRetrieve(JournalModuleRetrieve e, ISystem sys)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.

            sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // shallow copy if changed
            if (e.SwapOutItemFD != null)
                StoredModules = StoredModules.StoreModule(e.SwapOutItemFD, e.SwapOutItem, e.SwapOutItemLocalised, sys);

            ships[sid] = sm.AddModule(e.Slot, e.SlotFD, e.RetrievedItem, e.RetrievedItemFD, e.RetrievedItemLocalised);

            StoredModules = StoredModules.RemoveModuleUsingEnglishName(e.RetrievedItem);
            VerifyList();
        }

        public void ModuleSellRemote(JournalModuleSellRemote e)
        {
            StoredModules = StoredModules.RemoveModuleUsingEnglishName(e.SellItem);
        }

        public void MassModuleStore(JournalMassModuleStore e, ISystem sys)
        {
            string sid = Key(e.ShipFD, e.ShipId);

            Ship sm = EnsureShip(sid);            // this either gets current ship or makes a new one.
            sm = sm.SetShipDetails(e.Ship, e.ShipFD);   // will clone if data changed..
            ships[sid] = sm.RemoveModules(e.ModuleItems);
            StoredModules = StoredModules.StoreModule(e.ModuleItems, itemlocalisation, sys);
            VerifyList();
        }

        public void UpdateStoredModules(JournalStoredModules s)
        {
            StoredModules = StoredModules.UpdateStoredModules(s.ModuleItems);
            VerifyList();
        }

        public void SupercruiseEntry(JournalSupercruiseEntry e)
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetSubVehicle(Ship.SubVehicleType.None);
            }
            VerifyList();
        }

        public void FSDJump(JournalFSDJump e)
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetFuelLevel(e.FuelLevel).SetSubVehicle(Ship.SubVehicleType.None);
            }
            VerifyList();
        }

        public void FuelScoop(JournalFuelScoop e)
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetFuelLevel(e.Total);
            }
            VerifyList();
        }

        public void FuelReservoirReplenished(JournalReservoirReplenished e)
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetFuelLevel(e.FuelMain, e.FuelReservoir);
            }
            VerifyList();
        }

        public void UIFuel(UIEvents.UIFuel e)       // called by controller if a new UI fuel is found
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetFuelLevel(e.Fuel, e.FuelRes);
            }
            VerifyList();
        }

        public void RefuelAll(JournalRefuelAll e)
        {
            if (HaveCurrentShip)
            {
                ships[currentid] = CurrentShip.SetFuelLevel(CurrentShip.FuelCapacity);
            }
            VerifyList();
        }

        public void RefuelPartial(JournalRefuelPartial e)
        {
            if (HaveCurrentShip)
            {
                // Amount includes reserve
                double level = CurrentShip.FuelLevel + e.Amount - 0.1;

                // If amount refuelled is less than 10%, then the tank is full
                if (e.Amount < CurrentShip.FuelCapacity / 10 || level > CurrentShip.FuelCapacity)
                    level = CurrentShip.FuelCapacity;

                ships[currentid] = CurrentShip.SetFuelLevel(level);
            }
            VerifyList();
        }

        public void EngineerCraft(JournalEngineerCraftBase c)
        {
            if (HaveCurrentShip)
            {
                System.Diagnostics.Debug.Assert(c.Engineering != null);     // double check its being passed a good engineering

                ships[currentid] = CurrentShip.Craft(c.SlotFD, c.ModuleFD, c.Engineering);
            }
            VerifyList();
        }

        #region Helpers

        private Ship EnsureShip(string id)      // ensure we have an ID of this type..
        {
            if (ships.ContainsKey(id))
            {
                Ship sm = ships[id];

                if (sm.State == Ship.ShipState.Owned)               // if owned, ok
                    return sm;
                else
                {
                    ships[Key(sm.ShipFD, newsoldid++)] = sm;              // okay, we place this information on back ID list+  all Ids of this will now refer to new entry
                }
            }

            ulong i = id.Substring(id.IndexOf(":") + 1).InvariantParseULong(0);
            //DebuggerHelpers.DP("SL",$"ShipList made new ship {id}.. {i}");
            Ship smn = new Ship(i);
            ships[id] = smn;
            return smn;
        }

        void VerifyList()       // included so when debugging we can turn this on and verify the list after every action. Journals are so random they sometimes throw up problems.
        {
            //foreach( KeyValuePair<string,ShipInformation> i in Ships)
            //{
            //System.Diagnostics.Debug.Assert(i.Value.ShipFD.HasChars());
            //}
        }

        #endregion

        #region process

        public Tuple<Ship, ShipModulesInStore> Process(JournalEntry je, string whereami, ISystem system ,bool multicrew)
        {
            if (je is IShipInformation)
            {
                if (multicrew)
                {
                    //DebuggerHelpers.DP("SL",$"ShipList Ignore {je.EventTimeUTC} {je.EventTypeStr} due to multicrew");
                }
                else
                {
                    IShipInformation e = je as IShipInformation;
                    e.ShipInformation(this, whereami, system);                             // not cloned.. up to callers to see if they need to
                }
            }

            return new Tuple<Ship, ShipModulesInStore>(CurrentShip, StoredModules);
        }

        #endregion

        #region vars
        private Dictionary<FDName, string> itemlocalisation;        // cache of modules vs item localisation
        private string currentid;
        private Dictionary<string, Ship> ships { get; set; }         // by shipid key
        #endregion
    }


}
