/*
 * Copyright © 2016 - 2023 EDDiscovery development team
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

using EliteDangerousCore.DB;
using EliteDangerousCore.JournalEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EliteDangerousCore
{
    [DebuggerDisplay("Event {EntryType} {System.Name} ({System.X,nq},{System.Y,nq},{System.Z,nq}) {EventTimeUTC} Inx:{Index} JID:{Journalid}")]
    public class HistoryEntry           // DONT store commander ID.. this history is externally filtered on it.
    {
        #region Public Variables

        public int EntryNumber { get { return Index + 1; } }   // for display purposes.  from 1 to number of records
        public int Index { get; set; } = -1; // zero based filtered index number - after reorder/remove, and is the entry number in historylist
        public int UnfilteredIndex { get; set; } = -1; // zero based unfiltered index number

        public JournalEntry journalEntry { get; private set; }       // MUST be present

        public ISystem System { get; private set; }         // Must be set! All entries, even if they are not FSD entries.
                                                            // The Minimum is name 
                                                            // x/y/z can be NANs or position. 

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
        public bool IsFSDLocationCarrierJump { get { return EntryType == JournalTypeEnum.FSDJump || EntryType == JournalTypeEnum.CarrierJump || EntryType == JournalTypeEnum.Location; } }
        public bool IsFSD{ get { return EntryType == JournalTypeEnum.FSDJump;  } }
        public bool IsLocOrJump { get { return EntryType == JournalTypeEnum.FSDJump || EntryType == JournalTypeEnum.Location || EntryType == JournalTypeEnum.CarrierJump; } }
        public bool IsFuelScoop { get { return EntryType == JournalTypeEnum.FuelScoop; } }

        public bool isTravelling { get { return TravelStatus.IsTravelling; } }
        public TimeSpan TravelledSeconds { get { return TravelStatus.IsTravelling ? (EventTimeUTC - TravelStatus.TravelStartTimeUTC) : new TimeSpan(0); } }  // 0 if not travelling, else time since start
        public double TravelledDistance { get { return TravelStatus.TravelledDistance(EventTimeUTC); } }
        public TimeSpan TravelledTime { get { return TravelStatus.TravelledTime(EventTimeUTC); } }
        public int TravelledJumps { get { return TravelStatus.TravelledJumps(EventTimeUTC); } }
        public int TravelledMissingJumps { get { return TravelStatus.TravelledMissingjump; } }
        public string TravelledStats { get { return TravelStatus.Stats(EventTimeUTC); } }
        public HistoryEntryStatus.TravelStateType TravelState { get { return EntryStatus.TravelState; } }

        public HistoryEntryStatus Status { get { return EntryStatus; } }
        public string WhereAmI { get { return EntryStatus.StationName ?? EntryStatus.BodyName ?? "Unknown"; } }
        public string GameModeGroup { get { return Status.GameMode + (String.IsNullOrEmpty(Status.Group) ? "" : (":" + Status.Group)); } }


        public int Visits { get; private set; }                                     // set by Historylist, visits up to this point in time

        public long? FullBodyID { get {                                     // only if at a body
                if (System.SystemAddress.HasValue && Status.HasBodyID)
                    return System.SystemAddress.Value | ((long)EntryStatus.BodyID.Value << 55);
                else
                    return null;
            } }

        public string GetNoteText { get { return journalEntry.SNC?.Note ?? ""; } }      // get SNC note text or empty string

        public string DebugStatus { get {      // Use as a replacement for note in travel grid to debug
                return
                     WhereAmI
                     + ", " + (EntryStatus.BodyType ?? "Null")
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

        // These parts are held here so we don't create new Status Entries each time

        public long Credits { get; set; }       // set up by Historylist during ledger accumulation
        public long Loan { get; set; }          // set up by Historylist during ledger accumulation
        public long Assets { get; set; }       // set up by Historylist during ledger accumulation

        // Calculated values, not from JE

        public uint MaterialCommodity { get; private set; } // generation index
        public Ship ShipInformation { get; private set; }     // may be null if not set up yet
        public ShipModulesInStore StoredModules { get; private set; }
        public uint MissionList { get; private set; }       // generation index
        public uint Statistics { get; private set; }        // generation index
        public uint Weapons { get; private set; }           // generation index
        public uint Suits { get; private set; }             // generation index
        public uint Loadouts { get; private set; }          // generation index
        public uint Engineering { get; private set; }       // generation index

        public StarScan.ScanNode ScanNode {get; set; } // only for journal scan, and only after you called FillScanNode in history list.

        #endregion

        #region Private Variables

        private HistoryEntryStatus EntryStatus { get;  set; }
        private HistoryTravelStatus TravelStatus { get; set; }

        #endregion

        #region Constructors

        private HistoryEntry()
        {
        }

        public static HistoryEntry FromJournalEntry(JournalEntry je, HistoryEntry prev, Dictionary<string, EDStar> starclasses)
        {
            ISystem isys = prev == null ? new SystemClass("Unknown") : prev.System;

            if (je.EventTypeID == JournalTypeEnum.Location || je.EventTypeID == JournalTypeEnum.FSDJump || je.EventTypeID == JournalTypeEnum.CarrierJump)
            {
                JournalLocOrJump jl = je as JournalLocOrJump;

                ISystem newsys;

                if (jl != null && jl.HasCoordinate)       // LAZY LOAD IF it has a co-ord.. the front end will when it needs it
                {
                    EDStar starclass = EDStar.Unknown;
                    starclasses?.TryGetValue(jl.StarSystem, out starclass);     // see if its there, and star classes is defined
                    newsys = new SystemClass(jl.StarSystem, jl.SystemAddress, jl.StarPos.X, jl.StarPos.Y, jl.StarPos.Z, jl.StarPosFromEDSM ? SystemSource.FromEDSM : SystemSource.FromJournal, starclass);
                    SystemCache.AddSystemToCache(newsys);        // this puts it in the cache

                    // If it was a new system, pass the coords back to the StartJump
                    if (prev != null && prev.journalEntry is JournalStartJump)
                    {
                        prev.System = newsys;       // give the previous startjump our system..
                    }
                }
                else
                {
                    newsys = new SystemClass(jl.StarSystem);         // this will be a synthesised one
                }

                isys = newsys;
            }

            HistoryEntry he = new HistoryEntry
            {
                journalEntry = je,
                System = isys,
                EntryStatus = HistoryEntryStatus.Update(prev?.EntryStatus, je, isys.Name),
            };

            return he;
        }

        // these are done purposely this way for ease of finding out who is updating these elements

        public void UpdateMaterialsCommodities(uint gen)
        {
            MaterialCommodity = gen;
        }

        public void UpdateStats(JournalEntry je, Stats stats, string station)
        {
            Statistics = stats.Process(je, station);
        }

        public void UpdateShipInformation(Ship si)       // something externally updated SI
        {
            ShipInformation = si;
        }

        public void UpdateShipStoredModules(ShipModulesInStore ms)
        {
            StoredModules = ms;
        }

        public void UpdateMissionList(uint ml)
        {
            MissionList = ml;
        }

        public void UpdateSuits(uint gen)
        {
            Suits = gen;
        }

        public void UpdateLoadouts(uint gen)
        {
            Loadouts = gen;
        }

        public void UpdateWeapons(uint gen)
        {
            Weapons = gen;
        }

        public void UpdateSystem(ISystem sys)
        {
            System = sys;
        }

        public void UpdateEngineering(uint gen)
        {
            Engineering = gen;
        }
        public void UpdateVisits(int visits)
        {
            Visits = visits;
        }

        public void UpdateTravelStatus(HistoryEntry prev)      // update travel status from previous given current.
        {
            TravelStatus = HistoryTravelStatus.Update(prev?.TravelStatus, prev, this);
        }

        public void ReplaceJournalEntry(JournalEntry p, DateTime utc)
        {
            journalEntry = p;
            journalEntry.EventTimeUTC = utc;
        }

        #endregion

        #region Interaction



        public bool IsJournalEventInEventFilter(string[] events)
        {
            return events.Contains(journalEntry.EventFilterName);
        }

        public bool IsJournalEventInEventFilter(string eventstr)
        {
            return eventstr == "All" || IsJournalEventInEventFilter(eventstr.Split(';'));
        }

        public EliteDangerousCalculations.FSDSpec.JumpInfo GetJumpInfo(int cargo)      // can we calc jump range? null if we don't have the info
        {
            EliteDangerousCalculations.FSDSpec fsdspec = ShipInformation?.GetFSDSpec();

            if (fsdspec != null)
            {
                double mass = ShipInformation.HullMass() + ShipInformation.ModuleMass();

                if (mass > 0)
                    return fsdspec.GetJumpInfo(cargo, mass, ShipInformation.FuelLevel, ShipInformation.FuelCapacity/2, Status.CurrentBoost);
            }

            return null;
        }

        public void FillInformation(out string eventDescription, out string eventDetailedInfo)
        {
            journalEntry.FillInformation(out eventDescription, out eventDetailedInfo);

            if (eventDescription == null)       // FillInformation was not implemented, try the next one
            {
                JournalEntry.FillInformationData fid = new JournalEntry.FillInformationData() 
                { System = this.System, WhereAmI = this.WhereAmI , NextJumpSystemName = this.Status.FSDJumpNextSystemName, NextJumpSystemAddress = this.Status.FSDJumpNextSystemAddress };

                journalEntry.FillInformationExtended(fid,out eventDescription, out eventDetailedInfo);
            }

            if (isTravelling && (IsFSD || StopMarker))
            {
                string ts = TravelledStats;
                if ( ts.Length>0)
                    eventDescription = ts + ", " + eventDescription;
            }
        }

        public void SetStartStop()
        {
            if (journalEntry.StartMarker || journalEntry.StopMarker)
                journalEntry.ClearStartEndFlag();
            else if (isTravelling)
                journalEntry.SetEndFlag();
            else
                journalEntry.SetStartFlag();
        }

        #endregion
    }
}
