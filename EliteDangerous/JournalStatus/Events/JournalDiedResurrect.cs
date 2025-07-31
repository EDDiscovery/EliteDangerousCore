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
 *
 *
 */
using QuickJSON;

namespace EliteDangerousCore.JournalEvents
{
    // { } - no other fields
    // {  "KillerShip":"tg_skimmer_01" }
    // {  "KillerName":"Cmdr Bobby Stacks", "KillerShip":"tacticalsuit_class3", "KillerRank":"Deadly" }
    // {  "KillerName":"$UNKNOWN;", "KillerName_Localised":"Unknown", "KillerShip":"scout", "KillerRank":"Elite" }
    // {  "KillerName":"$UNKNOWN;", "KillerName_Localised":"Unknown", "KillerShip":"scout_nq", "KillerRank":"Elite" }
    // {  "KillerName":"$UNKNOWN;", "KillerName_Localised":"Unknown", "KillerShip":"scout_q", "KillerRank":"Elite" }
    // {  "Killers":[ { "Name":"Cmdr Bobby Stacks", "Ship":"tacticalsuit_class3", "Rank":"Deadly" }, { "Name":"Cmdr Death of Morpheus", "Ship":"tacticalsuit_class3", "Rank":"Expert" } ] }

    [JournalEntryType(JournalTypeEnum.Died)]
    public class JournalDied : JournalEntry, IMissions, ICommodityJournalEntry
    {
        public class Killer
        {
            public string Name;             // always non null 
            public string Name_Localised;   // always non null 
            public string Ship;             // always non null 
            public string Rank;             // may be null

            public string FriendlyShip;     // EDD addition, always non null
        }

        public JournalDied(JObject evt ) : base(evt, JournalTypeEnum.Died)
        {
            //System.Diagnostics.Debug.WriteLine($"Died {evt.ToString()}");

            string killerName = evt["KillerName"].Str();

            if (killerName.IsEmpty())                       // no killer name
            {
                if (evt["Killers"] != null)                 // by a wing
                {
                    Killers = evt["Killers"].ToObjectQ<Killer[]>();
                }
                else 
                {
                    string kship = evt["KillerShip"].StrNull();
                    if (kship != null)
                    {
                        Killers = new Killer[1] { new Killer { Name = kship, Name_Localised = kship.SplitCapsWordFull(), Ship = kship } };
                    }
                }
            }
            else
            {
                // it was an individual
                Killers = new Killer[1]
                {
                    new Killer {  Name = killerName, Name_Localised = evt["KillerName_Localised"].Str(), Ship = evt["KillerShip"].Str(),  Rank = evt["KillerRank"].Str() }
                };
            }

            if (Killers != null)
            {
                foreach (Killer k in Killers)
                {
                    k.Name = k.Name ?? "";      // ensure set - may not be set for a bad Killers array
                    k.Name_Localised = JournalFieldNaming.CheckLocalisation(k.Name_Localised ?? "", k.Name);
                    k.Ship = k.Ship ?? "";      // ensure set
                    k.FriendlyShip = k.Ship.HasChars() ? JournalFieldNaming.GetBetterShipSuitActorName(k.Ship) : "";
                    //System.Diagnostics.Debug.WriteLine($" >> Died '{k.Name}' '{k.Name_Localised}' '{k.Ship}' '{k.FriendlyShip}' '{k.Rank}'");
                }
            }

            //FillInformation(null, null, out string info, out string detailed); System.Diagnostics.Debug.WriteLine($"Died: {info}");
        }

        public Killer[] Killers { get; set; }           // may be null if no killer listed

        public void UpdateMissions(MissionListAccumulator mlist, EliteDangerousCore.ISystem sys, string body)
        {
            mlist.Died(this.EventTimeUTC);
        }

        public void UpdateCommodities(MaterialCommoditiesMicroResourceList mc, bool unusedinsrv)
        {
            mc.Clear(0, MaterialCommodityMicroResourceType.CatType.Commodity);      // clear all count zero of commodities
            // clear all backpack items on death..
            mc.Clear(MicroResource.BackPack, MaterialCommodityMicroResourceType.CatType.Component, MaterialCommodityMicroResourceType.CatType.Data, MaterialCommodityMicroResourceType.CatType.Consumable, MaterialCommodityMicroResourceType.CatType.Item );      // clear all count zero of commodities
        }

        public override string GetInfo() 
        {
            if (Killers != null)
            {
                var sb = new System.Text.StringBuilder(256);
                sb.Append("Killed by ".Tx());

                foreach (Killer k in Killers)
                {
                    string kstr = "";

                    if (ItemData.IsSuit(k.Ship))
                    {
                        string type = k.Ship.ContainsIIC("Citizen") ? k.FriendlyShip.Replace("Suit ", "") : k.FriendlyShip.Replace("Suit", "Trooper");
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "", type);
                    }
                    else if (ItemData.IsShip(k.Ship))
                    {
                        kstr = string.Format("{0} in ship type {1} rank {2}".Tx(), k.Name_Localised, k.FriendlyShip, k.Rank ?? "?");
                    }
                    else if (k.FriendlyShip.HasChars())
                    {
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "", k.FriendlyShip, "Rank: ".Tx(), k.Rank);
                    }
                    else
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "Rank: ".Tx(), k.Rank);

                    sb.AppendPrePad(kstr, ", ");
                }
                return sb.ToString();
            }
            else
                return null;
        }

    }


    [JournalEntryType(JournalTypeEnum.SelfDestruct)]
    public class JournalSelfDestruct : JournalEntry
    {
        public JournalSelfDestruct(JObject evt) : base(evt, JournalTypeEnum.SelfDestruct)
        {
        }

        public override string GetInfo()
        {
            return "Boom!".Tx();
        }
    }



    [JournalEntryType(JournalTypeEnum.Resurrect)]
    public class JournalResurrect : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalResurrect(JObject evt) : base(evt, JournalTypeEnum.Resurrect)
        {
            FDOption = evt["Option"].Str();
            Option = JournalFieldNaming.ResurrectOption(FDOption);
            Cost = evt["Cost"].Long();
            Bankrupt = evt["Bankrupt"].Bool();
        }

        public string Option { get; set; }      // Friendly, not FDName
        public string FDOption { get; set; }
        public long Cost { get; set; }
        public bool Bankrupt { get; set; }

        public void Ledger(Ledger mcl)
        {
            if ( Cost != 0 )
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, Option, -Cost);
        }

        public void ShipInformation(ShipList shp, string whereami, ISystem system)
        {
            shp.Resurrect(Option.Equals("free", System.StringComparison.InvariantCultureIgnoreCase));    // if free, we did not rebuy the ship
        }

        public override string GetInfo()
        {
            return BaseUtils.FieldBuilder.Build("Option: ".Tx(), Option, "Cost: ; cr;N0".Tx(), Cost, ";Bankrupt".Tx(), Bankrupt);
        }
    }


}
