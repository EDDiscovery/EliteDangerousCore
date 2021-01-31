/*
 * Copyright © 2016 - 2020 EDDiscovery development team
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

using EliteDangerousCore.DB;
using EliteDangerousCore.EDSM;
using EliteDangerousCore.JournalEvents;
using System;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    [DebuggerDisplay("Event {EntryType} {System.Name} ({System.X,nq},{System.Y,nq},{System.Z,nq}) {EventTimeUTC} Inx:{Indexno} JID:{Journalid}")]
    public class HistoryEntry           // DONT store commander ID.. this history is externally filtered on it.
    {
        #region Public Variables

        public int Indexno;            // for display purposes.  from 1 to number of records

        public JournalEntry journalEntry;       // MUST be present

        public ISystem System;         // Must be set! All entries, even if they are not FSD entries.
                                       // The Minimum is name and edsm_id 
                                       // x/y/z can be NANs or position. 
                                       // if edsm_id = 0, no edsm match was found.
                                       // when the front end needs to use it, it will call EnsureSystemEDSM/FillInPositions to see if it can be filled up
                                       // if System.status != SystemStatusEnum.EDSC then its presumed its an inmemory load and the system table can be checked
                                       //       and if edsm_id>0 a load from SystemTable will occur with the edsm_id used
                                       //       if edsm_id=-1 a load from SystemTable will occur with the name used
                                       // SO the journal reader can just read data in that table only, does not need to do a system match

        public JournalTypeEnum EntryType { get { return journalEntry.EventTypeID; } }
        public long Journalid { get { return journalEntry.Id; } }
        public EDCommander Commander { get { return EDCommander.GetCommander(journalEntry.CommanderId); } }
        public DateTime EventTimeUTC { get { return journalEntry.EventTimeUTC; } }  // local removed to stop us using it!.
        public TimeSpan AgeOfEntry() { return DateTime.UtcNow - EventTimeUTC; }

        public string EventSummary { get { return journalEntry.SummaryName(System); } }

        public bool EdsmSync { get { return journalEntry.SyncedEDSM; } }           // flag populated from journal entry when HE is made. Have we synced?
        public bool EDDNSync { get { return journalEntry.SyncedEDDN; } }
        public bool StartMarker { get { return journalEntry.StartMarker; } }
        public bool StopMarker { get { return journalEntry.StopMarker; } }
        public bool IsFSDCarrierJump { get { return EntryType == JournalTypeEnum.FSDJump || EntryType == JournalTypeEnum.CarrierJump; } }
        public bool IsLocOrJump { get { return EntryType == JournalTypeEnum.FSDJump || EntryType == JournalTypeEnum.Location || EntryType == JournalTypeEnum.CarrierJump; } }
        public bool IsFuelScoop { get { return EntryType == JournalTypeEnum.FuelScoop; } }
        public bool IsShipChange { get { return (EntryType == JournalTypeEnum.LoadGame || EntryType == JournalTypeEnum.Docked) && ShipInformation != null; } }
        public bool IsBetaMessage { get { return journalEntry?.Beta ?? false; } }

        public double TravelledDistance { get { return TravelStatus.TravelledDistance; } }
        public TimeSpan TravelledSeconds { get { return TravelStatus.TravelledSeconds; } }
        public bool isTravelling { get { return TravelStatus.IsTravelling; } }
        public int TravelledMissingjump { get { return TravelStatus.TravelledMissingjump; } }
        public int Travelledjumps { get { return TravelStatus.Travelledjumps; } }
        public string TravelInfo() { return TravelStatus.ToString("TT: "); }
        public string TravelledJumpsAndMisses { get { return Travelledjumps.ToString() + ((TravelledMissingjump > 0) ? (" (" + TravelledMissingjump.ToString() + ")") : ""); } }

        public bool IsLanded { get { return EntryStatus.TravelState == HistoryEntryStatus.TravelStateType.Landed; } }
        public bool IsDocked { get { return EntryStatus.TravelState == HistoryEntryStatus.TravelStateType.Docked; } }
        public bool IsInHyperSpace { get { return EntryStatus.TravelState == HistoryEntryStatus.TravelStateType.Hyperspace; } }
        public string WhereAmI { get { return EntryStatus.StationName ?? EntryStatus.BodyName ?? "Unknown"; } }
        public string BodyType { get { return EntryStatus.BodyType ?? "Unknown"; } }
        public string ShipType { get { return EntryStatus.ShipType ?? "Unknown"; } }         // NOT FD - translated name
        public string ShipTypeFD { get { return EntryStatus.ShipTypeFD ?? "unknown"; } }
        public int ShipId { get { return EntryStatus.ShipID; } }
        public bool MultiPlayer { get { return EntryStatus.OnCrewWithCaptain != null; } }
        public string GameMode { get { return EntryStatus.GameMode ?? ""; } }
        public string Group { get { return EntryStatus.Group ?? ""; } }
        public string GameModeGroup { get { return GameMode + (String.IsNullOrEmpty(Group) ? "" : (":" + Group)); } }
        public bool Wanted { get { return EntryStatus.Wanted; } }
        public long? MarketID { get { return EntryStatus.MarketId; } }
        public int? BodyID { get { return EntryStatus.BodyID; } }           // NOTE -1 is used for no body, as well as null
        public bool HasBodyID { get { return EntryStatus.HasBodyID; } }
        public string StationFaction { get { return EntryStatus.StationFaction; } }
        public int Visits { get; set; }                                     // set by Historylist, visits up to this point in time

        public long? FullBodyID { get {                                     // only if at a body
                if (System.SystemAddress.HasValue && HasBodyID)
                    return System.SystemAddress.Value | ((long)EntryStatus.BodyID.Value << 55);
                else
                    return null;
            }}

        public string DebugStatus { get {      // Use as a replacement for note in travel grid to debug
                return
                     WhereAmI
                     + ", " +  (EntryStatus.BodyType ?? "Null")
                     + "," + (EntryStatus.BodyName ?? "Null")
                     + " SN:" + (EntryStatus.StationName ?? "Null") 
                     + " ST:" + (EntryStatus.StationType ?? "Null")
                     + " T:" + EntryStatus.TravelState
                     + " S:" + EntryStatus.ShipID + "," + EntryStatus.ShipType
                     + " GM:" + EntryStatus.GameMode
                     + " W:" + EntryStatus.Wanted
                     + " BA:" + EntryStatus.BodyApproached 
                     ;
            } }
  
        public long Credits { get; set; }       // set up by Historylist during ledger accumulation

        public bool ContainsRares() // function due to debugger and cost of working out
        {
            return MaterialCommodity != null && MaterialCommodity.ContainsRares();
        }

        // Calculated values, not from JE

        public MaterialCommoditiesList MaterialCommodity { get; private set; }
        public ShipInformation ShipInformation { get; private set; }     // may be null if not set up yet
        public ModulesInStore StoredModules { get; private set; }
        public uint MissionList { get; private set; }       // generation index
        public uint Statistics { get; private set; }     // generation index

        public SystemNoteClass snc;     // system note class found attached to this entry. May be null

        #endregion

        #region Private Variables

        private HistoryEntryStatus EntryStatus { get;  set; }
        private HistoryTravelStatus TravelStatus { get; set; }

        #endregion

        #region Constructors

        private HistoryEntry()
        {
        }

        public static HistoryEntry FromJournalEntry(JournalEntry je, HistoryEntry prev)
        {
            ISystem isys = prev == null ? new SystemClass("Unknown") : prev.System;
            int indexno = prev == null ? 1 : prev.Indexno + 1;

            if (je.EventTypeID == JournalTypeEnum.Location || je.EventTypeID == JournalTypeEnum.FSDJump || je.EventTypeID == JournalTypeEnum.CarrierJump)
            {
                JournalLocOrJump jl = je as JournalLocOrJump;

                ISystem newsys;

                if (jl != null && jl.HasCoordinate)       // LAZY LOAD IF it has a co-ord.. the front end will when it needs it
                {
                    newsys = new SystemClass(jl.StarSystem, jl.StarPos.X, jl.StarPos.Y, jl.StarPos.Z)
                    {
                        EDSMID = 0,         // not an EDSM entry
                        SystemAddress = jl.SystemAddress,
                        Population = jl.Population ?? 0,
                        Faction = jl.Faction,
                        Government = jl.EDGovernment,
                        Allegiance = jl.EDAllegiance,
                        State = jl.EDState,
                        Security = jl.EDSecurity,
                        PrimaryEconomy = jl.EDEconomy,
                        Power = jl.PowerList,
                        PowerState = jl.PowerplayState,
                        Source = jl.StarPosFromEDSM ? SystemSource.FromEDSM : SystemSource.FromJournal,
                    };

                    SystemCache.FindCachedJournalSystem(newsys);

                    // If it was a new system, pass the coords back to the StartJump
                    if (prev != null && prev.journalEntry is JournalStartJump)
                    {
                        prev.System = newsys;       // give the previous startjump our system..
                    }
                }
                else
                {
                    // NOTE Rob: 09-JAN-2018 I've removed the Jumpstart looking up a system by name since they were using up lots of lookup time during history reading.  
                    // This is used for pre 2.2 systems without co-ords, which now should be limited.
                    // JumpStart still gets the system when the FSD loc is processed, see above.
                    // Jumpstart was also screwing about with the EDSM ID fill in which was broken.  This is now working again.

                    newsys = new SystemClass(jl.StarSystem);         // this will be a synthesised one
                }

                isys = newsys;
            }

            HistoryEntry he = new HistoryEntry
            {
                Indexno = indexno,
                journalEntry = je,
                System = isys,
                EntryStatus = HistoryEntryStatus.Update(prev?.EntryStatus, je, isys.Name)
            };

            he.TravelStatus = HistoryTravelStatus.Update(prev?.TravelStatus, prev , he);    // need a real he so can't do that as part of the constructor.
            
            return he;
        }

        public void UpdateMaterialsCommodities(JournalEntry je, MaterialCommoditiesList prev)
        {
            MaterialCommodity = MaterialCommoditiesList.Process(je, prev);
        }

        public void UpdateStats(JournalEntry je, Stats stats, string station)
        {
            Statistics = stats.Process(je, station);
        }

        public void UpdateSystemNote()
        { 
            snc = SystemNoteClass.GetSystemNote(Journalid, IsFSDCarrierJump ?  System.Name : null);       // may be null
        }

        public void UpdateShipInformation(ShipInformation si)       // something externally updated SI
        {
            ShipInformation = si;
        }

        public void UpdateShipStoredModules(ModulesInStore ms)
        {
            StoredModules = ms;
        }

        public void UpdateMissionList(uint ml)
        {
            MissionList = ml;
        }

        public void UpdateMaterialCommodity(MaterialCommoditiesList mc)
        {
            MaterialCommodity = mc;
        }

        #endregion

        #region Interaction

        public void SetJournalSystemNoteText(string text, bool commit, bool sendtoedsm)
        {
            if (snc == null || snc.Journalid == 0)           // if no system note, or its one on a system, from now on we assign journal system notes only from this IF
                snc = SystemNoteClass.MakeSystemNote("", DateTime.Now, System.Name, Journalid, IsFSDCarrierJump);

            snc = snc.UpdateNote(text, commit, DateTime.Now, IsFSDCarrierJump);        // and update info, and update our ref in case it has changed or gone null
                                                                                       // remember for EDSM send purposes if its an FSD entry

            if (snc != null && commit && sendtoedsm && snc.FSDEntry)                   // if still have a note, and commiting, and send to esdm, and FSD jump
                EDSMClass.SendComments(snc.SystemName, snc.Note);
        }

        public bool IsJournalEventInEventFilter(string[] events)
        {
            return events.Contains(journalEntry.EventFilterName);
        }

        public bool IsJournalEventInEventFilter(string eventstr)
        {
            return eventstr == "All" || IsJournalEventInEventFilter(eventstr.Split(';'));
        }

        public EliteDangerousCalculations.FSDSpec.JumpInfo GetJumpInfo()      // can we calc jump range? null if we don't have the info
        {
            EliteDangerousCalculations.FSDSpec fsdspec = ShipInformation?.GetFSDSpec();

            if (fsdspec != null)
            {
                double mass = ShipInformation.HullMass() + ShipInformation.ModuleMass();

                if (mass > 0)
                    return fsdspec.GetJumpInfo(MaterialCommodity.CargoCount, mass, ShipInformation.FuelLevel, ShipInformation.FuelCapacity/2);
            }

            return null;
        }

        public void FillInformation(out string EventDescription, out string EventDetailedInfo)
        {
            journalEntry.FillInformation(System, out EventDescription, out EventDetailedInfo);
        }


        #endregion
    }
}
