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
using System;

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
            public FDName Ship;             // always non null 
            public RankDefinitions.CombatRank Rank;  // may be unknown

            public string FriendlyShip;     // EDD addition, always non null
        }

        public JournalDied(JObject evt ) : base(evt, JournalTypeEnum.Died)
        {
            //System.Diagnostics.Debug.WriteLine($"Died {evt.ToString()}");

            if (evt.Contains("Killers"))
            {
                Killers = evt["Killers"].ToObject<Killer[]>(process: (type, str) => Enum.TryParse<RankDefinitions.CombatRank>(str, true, out RankDefinitions.CombatRank cr) ? cr : RankDefinitions.CombatRank.Unknown);
                foreach (var x in Killers.EmptyIfNull())
                {
                    x.Ship = FDNameHelpers.NormaliseShipOrSuitOrActor(x.Ship.Str(), out string engname, this);
                    x.FriendlyShip = engname;
                    x.Name_Localised = x.Name_Localised != null ? x.Name_Localised : x.Name;
                    x.Name = x.Name != null && !x.Name.ContainsIIC("$UNKNOWN") ? x.Name : engname;
                }
            }
            else if (evt.Contains("KillerName") || evt.Contains("KillerShip"))
            {
                string killerName = evt["KillerName"].StrNull();        // may not be there
                var ShipType = FDNameHelpers.NormaliseShipOrSuitOrActor(evt["KillerShip"].Str(), out string engname, this);
                var name = killerName != null && !killerName.ContainsIIC("$UNKNOWN") ? killerName : engname;         // Killer Name can be missing

                Killers = new Killer[1]
                {
                    new Killer {  Name = name,
                                Name_Localised = evt["KillerName_Localised"].Str(name),
                                Ship = ShipType,
                                Rank = Enum.TryParse<RankDefinitions.CombatRank>(evt["KillerRank"].Str(),true, out RankDefinitions.CombatRank cr) ? cr : RankDefinitions.CombatRank.Unknown,
                                FriendlyShip = engname,
                     }
                };
            }
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

                foreach (Killer k in Killers)
                {
                    string kstr = "";

                    if (ItemData.IsSuitTypeName(k.Ship))
                    {
                        string type = k.Ship.Contains("citizen") ? k.FriendlyShip.Replace("Suit ", "") : k.FriendlyShip.Replace("Suit", "Trooper");
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "", type);
                    }
                    else if (ItemData.IsShip(k.Ship))
                    {
                        kstr = string.Format("{0} in ship type {1} rank {2}".Tx(), k.Name_Localised, k.FriendlyShip, k.Rank.ToString());
                    }
                    else if (k.FriendlyShip.HasChars())
                    {
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised != "Unknown" ? k.Name_Localised : null, "", k.FriendlyShip, "Rank".Tx() + ": ", k.Rank.ToString());
                    }
                    else
                        kstr = BaseUtils.FieldBuilder.Build("", k.Name_Localised, "Rank".Tx() + ": ", k.Rank);

                    sb.AppendPrePad(kstr, ", ");
                }

                return "Killed by ".Tx() + sb.ToString();
            }
            else
                return null;;
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
            FDOption = Enum.TryParse(evt["Option"].Str(), true, out ResurrectTypes s) ? s : ResurrectTypes.Unknown;
            if (FDOption == ResurrectTypes.Unknown) 
            { 
                BaseUtils.Debugger.TraceBreak($"*** Unknown Resurrect {(evt["Option"].Str())}");
                ;  
            }
            Option = FDOption.ToString().SplitCapsWordFull();
            Cost = evt["Cost"].Long();
            Bankrupt = evt["Bankrupt"].Bool();
        }

        public string Option { get; set; }      // Friendly, not FDName

        public enum ResurrectTypes { Free, Rebuy, Recover, HandIn, Rejoin, Escape, Unknown };
        public ResurrectTypes FDOption { get; set; }
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
            return BaseUtils.FieldBuilder.Build("Option".Tx()+": ", Option, "Cost: ; cr;N0".Tx(), Cost, ";Bankrupt".Tx(), Bankrupt);
        }
    }


}
