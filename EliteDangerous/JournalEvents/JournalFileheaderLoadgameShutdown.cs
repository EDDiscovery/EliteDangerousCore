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
using QuickJSON;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.Fileheader)]
    public class JournalFileheader : JournalEntry
    {
        public JournalFileheader(JObject evt ) : base(evt, JournalTypeEnum.Fileheader)
        {
            GameVersion = evt["gameversion"].Str();
            Build = evt["build"].Str();
            Language = evt["language"].Str();
            Part = evt["part"].Int();
            Odyssey = evt["Odyssey"].Bool();
        }

        public override string GameVersion { get; }
        public override string Build { get; }
        public string Language { get; set; }
        public int Part { get; set; }
        public bool Odyssey { get; set; }       // NOTE 4.0 'Horizons' has this true, its indicating the client build, not if the user has odyssey. Source Fdev

        public override bool IsBeta
        {
            get
            {
                if (GameVersion.Contains("Beta", StringComparison.InvariantCultureIgnoreCase) ||
                    GameVersion.Contains("Gamma", StringComparison.InvariantCultureIgnoreCase) ||
                    GameVersion.Contains("Alpha", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                if (GameVersion.Contains("April Update EDH") && ( Build.Contains("r198057/r0") || Build.Contains("r197746/r0")))
                    return true;

                if (GameVersion.Equals("2.2") && (Build.Contains("r121645/r0") || Build.Contains("r129516/r0")))
                    return true;

                return false;
            }
        }

        public override void FillInformation(out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("Version: ".T(EDCTx.JournalEntry_Version), GameVersion , "Build: ".T(EDCTx.JournalEntry_Build), Build , "Part: ".T(EDCTx.JournalEntry_Part), Part);
            detailed = "";
        }
    }


    [JournalEntryType(JournalTypeEnum.LoadGame)]
    [System.Diagnostics.DebuggerDisplay("LoadGame {LoadGameCommander} {ShipId} {Ship} {GameMode}")]
    public class JournalLoadGame : JournalEntry, ILedgerJournalEntry, IShipInformation
    {
        public JournalLoadGame(JObject evt) : base(evt, JournalTypeEnum.LoadGame)
        {
            LoadGameCommander = JournalFieldNaming.SubsituteCommanderName( evt["Commander"].Str() );
            
            ShipFD = evt["Ship"].Str();
            Ship_Localised = evt["Ship_Localised"].StrNull();       // may not be present

            if (ShipFD.Length == 0)      // Vega logs show no ship on certain logs.. handle it to prevent warnings.
                Ship_Localised = ShipFD = "Unknown";

            if ( ItemData.IsShip(ShipFD) || ItemData.IsSRVOrFighter(ShipFD))
            { 
                ShipFD = JournalFieldNaming.NormaliseFDShipName(ShipFD);
                Ship = JournalFieldNaming.GetBetterShipSuitActorName(ShipFD);
            }
            else 
            {
                Ship = ShipFD.SplitCapsWordFull();
            }

            Ship_Localised = Ship_Localised.Alt(Ship);

            ShipId = evt["ShipID"].ULong();
            StartLanded = evt["StartLanded"].Bool();
            StartDead = evt["StartDead"].Bool();
            GameMode = evt["GameMode"].Str();
            Group = evt["Group"].Str();
            Credits = evt["Credits"].Long();
            Loan = evt["Loan"].Long();

            ShipName = evt["ShipName"].Str();
            ShipIdent = evt["ShipIdent"].Str();
            FuelLevel = evt["FuelLevel"].Double();
            FuelCapacity = evt["FuelCapacity"].Double();

            Horizons = evt["Horizons"].Bool();
            Odyssey = evt["Odyssey"].Bool();

            Language = evt["language"].Str();
            GameVersion = evt["gameversion"].Str();
            Build = evt["build"].Str();

            FID = JournalFieldNaming.SubsituteCommanderFID(evt["FID"].Str());     // 3.3 on
        }

        public string LoadGameCommander { get; set; }
        public string Ship { get; set; }        // friendly name, fer-de-lance, from our db.  Older Load games did not have Localised
        public string Ship_Localised { get; set; }   // localised
        public string ShipFD { get; set; }        // type, fd name
        public ulong ShipId { get; set; }
        public bool StartLanded { get; set; }
        public bool StartDead { get; set; }
        public string GameMode { get; set; }
        public string Group { get; set; }
        public long Credits { get; set; }
        public long Loan { get; set; }

        public string ShipName { get; set; } // : user-defined ship name
        public string ShipIdent { get; set; } //   user-defined ship ID string
        public double FuelLevel { get; set; }
        public double FuelCapacity { get; set; }

        public override bool IsHorizons { get { return Horizons; } }     // override base to get value of private value
        public override bool IsOdyssey { get { return Odyssey; } }

        public string Language { get; set; }         // odyssey release 2 27/5/21
        public override string GameVersion { get;  }      // odyssey release 2 27/5/21
        public override string Build { get; }            // odyssey release 2 27/5/21

        public string FID { get; set; }

        public bool InShip { get { return ItemData.IsShip(ShipFD); } }
        public bool InSuit { get { return ItemData.IsSuit(ShipFD); } }     // 4.0
        public bool InTaxi { get { return ItemData.IsTaxi(ShipFD); } }     // 4.0
        public bool InSRV { get { return ItemData.IsSRV(ShipFD); } }
        public bool InFighter { get { return ItemData.IsFighter(ShipFD); } }
        public bool InShipSRVOrFighter { get { return ItemData.IsShipSRVOrFighter(ShipFD); } }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Cmdr ", LoadGameCommander, "Ship: ".T(EDCTx.JournalEntry_Ship), Ship, "Name: ".T(EDCTx.JournalEntry_Name), ShipName, "Ident: ".T(EDCTx.JournalEntry_Ident), ShipIdent, "Credits: ;;N0".T(EDCTx.JournalEntry_Credits), Credits);
            detailed = BaseUtils.FieldBuilder.Build("Mode: ".T(EDCTx.JournalEntry_Mode), GameMode, "Group: ".T(EDCTx.JournalEntry_Group), Group, "Not Landed;Landed".T(EDCTx.JournalEntry_NotLanded), StartLanded, "Fuel Level: ;;0.0".T(EDCTx.JournalEntry_FuelLevel), FuelLevel, "Capacity: ;;0.0".T(EDCTx.JournalEntry_Capacity), FuelCapacity);
        }

        public void Ledger(Ledger mcl)
        {
            mcl.Loan = Loan;
            if (mcl.CashTotal != Credits)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "Cash total differs, adjustment".T(EDCTx.JournalEntry_Cashtotaldiffers), Credits - mcl.CashTotal);
            }
        }

        public void ShipInformation(ShipInformationList shp, string whereami, ISystem system)
        {
            if (InShipSRVOrFighter)        // only call if in these types from 4.0 we can be on foot or in a taxi
                shp.LoadGame(ShipId, Ship, ShipFD, ShipName, ShipIdent, FuelLevel, FuelCapacity);
        }

        private bool Horizons { get; set; }
        private bool Odyssey { get; set; }
    }

    [JournalEntryType(JournalTypeEnum.Shutdown)]
    public class JournalShutdown : JournalEntry
    {
        public JournalShutdown(JObject evt) : base(evt, JournalTypeEnum.Shutdown)
        {
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = "";
            detailed = "";
        }

    }

    [JournalEntryType(JournalTypeEnum.Continued)]
    public class JournalContinued : JournalEntry
    {
        public JournalContinued(JObject evt) : base(evt, JournalTypeEnum.Continued)
        {
            Part = evt["Part"].Int();
        }

        public int Part { get; set; }

        public override void FillInformation(out string info, out string detailed)
        {
            info = Part.ToString();
            detailed = "";
        }
    }

}
