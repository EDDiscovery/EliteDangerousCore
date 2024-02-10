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
                    { // by an automated device
                        Killers = new Killer[1] { new Killer { Name = "", Name_Localised = "", Ship = kship } };
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

        public override void FillInformation(out string info, out string detailed) 
        {
            info = "";
            if (Killers != null)
            {
                foreach (Killer k in Killers)
                {
                    string kstr="";
                   
                    if (ItemData.IsSuit(k.Ship))
                    {
                        string type = k.Ship.ContainsIIC("Citizen") ? k.FriendlyShip.Replace("Suit ", "") : k.FriendlyShip.Replace("Suit", "Trooper");
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "", type);
                    }
                    else if ( ItemData.IsShip(k.Ship))
                    {
                        kstr = string.Format("{0} in ship type {1} rank {2}".T(EDCTx.JournalEntry_Died), k.Name_Localised, k.FriendlyShip, k.Rank ?? "?");
                    }
                    else if ( k.FriendlyShip.HasChars() )
                    {
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "", k.FriendlyShip, "Rank: ".T(EDCTx.JournalEntry_Rank), k.Rank);
                    }
                    else
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "Rank: ".T(EDCTx.JournalEntry_Rank), k.Rank);

                    info = info.AppendPrePad(kstr, ", ");
                }

                info = "Killed by ".T(EDCTx.JournalEntry_Killedby) + info;
            }

            detailed = "";
        }

    }


    [JournalEntryType(JournalTypeEnum.SelfDestruct)]
    public class JournalSelfDestruct : JournalEntry
    {
        public JournalSelfDestruct(JObject evt) : base(evt, JournalTypeEnum.SelfDestruct)
        {
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = "Boom!".T(EDCTx.JournalEntry_Boom);
            detailed = "";
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

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            shp.Resurrect(Option.Equals("free", System.StringComparison.InvariantCultureIgnoreCase));    // if free, we did not rebuy the ship
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Option: ".T(EDCTx.JournalEntry_Option), Option, "Cost: ; cr;N0".T(EDCTx.JournalEntry_Cost), Cost, ";Bankrupt".T(EDCTx.JournalEntry_Bankrupt), Bankrupt);
            detailed = "";
        }
    }


}
