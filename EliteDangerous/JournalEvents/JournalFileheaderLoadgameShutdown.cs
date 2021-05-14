/*
 * Copyright © 2016-2018 EDDiscovery development team
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
using BaseUtils.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        }

        public string GameVersion { get; set; }
        public string Build { get; set; }
        public string Language { get; set; }
        public int Part { get; set; }

        public override bool IsBeta
        {
            get
            {
                if (GameVersion.Contains("Beta", StringComparison.InvariantCultureIgnoreCase) || GameVersion.Contains("Alpha", StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (GameVersion.Contains("April Update EDH") && ( Build.Contains("r198057/r0") || Build.Contains("r197746/r0")))
                    return true;

                if (GameVersion.Equals("2.2") && (Build.Contains("r121645/r0") || Build.Contains("r129516/r0")))
                    return true;

                return false;
            }
        }

        public override void FillInformation(ISystem sys, out string info, out string detailed) 
        {
            info = BaseUtils.FieldBuilder.Build("Version:".T(EDTx.JournalEntry_Version), GameVersion , "Build:".T(EDTx.JournalEntry_Build), Build , "Part:".T(EDTx.JournalEntry_Part), Part);
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
            if (ShipFD.Length == 0)      // Vega logs show no ship on certain logs.. handle it to prevent warnings.
                ShipFD = "Unknown";

            if ( ItemData.IsShip(ShipFD))
            { 
                ShipFD = JournalFieldNaming.NormaliseFDShipName(ShipFD);
                Ship = JournalFieldNaming.GetBetterShipName(ShipFD);
            }
            else
            {
                Ship = ShipFD.SplitCapsWordFull();
            }

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

            FID = JournalFieldNaming.SubsituteCommanderFID(evt["FID"].Str());     // 3.3 on
        }

        public string LoadGameCommander { get; set; }
        public string Ship { get; set; }        // type, fer-de-lance
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

        public string FID { get; set; }

        public bool InShip { get { return ItemData.IsShip(ShipFD); } }
        public bool InSuit { get { return ItemData.IsSuit(ShipFD); } }     // 4.0
        public bool InTaxi { get { return ItemData.IsTaxi(ShipFD); } }     // 4.0
        public bool InSRV { get { return ItemData.IsSRV(ShipFD); } }
        public bool InFighter { get { return ItemData.IsFighter(ShipFD); } }
        public bool InShipSRVOrFighter { get { return ItemData.IsShipSRVOrFighter(ShipFD); } }

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("Cmdr ", LoadGameCommander, "Ship:".T(EDTx.JournalEntry_Ship), Ship, "Name:".T(EDTx.JournalEntry_Name), ShipName, "Ident:".T(EDTx.JournalEntry_Ident), ShipIdent, "Credits:;;N0".T(EDTx.JournalEntry_Credits), Credits);
            detailed = BaseUtils.FieldBuilder.Build("Mode:".T(EDTx.JournalEntry_Mode), GameMode, "Group:".T(EDTx.JournalEntry_Group), Group, "Not Landed;Landed".T(EDTx.JournalEntry_NotLanded), StartLanded, "Fuel Level:;;0.0".T(EDTx.JournalEntry_FuelLevel), FuelLevel, "Capacity:;;0.0".T(EDTx.JournalEntry_Capacity), FuelCapacity);
        }

        public void Ledger(Ledger mcl)
        {
            if (mcl.CashTotal != Credits)
            {
                mcl.AddEvent(Id, EventTimeUTC, EventTypeID, "Cash total differs, adjustment".T(EDTx.JournalEntry_Cashtotaldiffers), Credits - mcl.CashTotal);
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

        public override void FillInformation(ISystem sys, out string info, out string detailed)
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

        public override void FillInformation(ISystem sys, out string info, out string detailed)
        {
            info = Part.ToString();
            detailed = "";
        }
    }

}
