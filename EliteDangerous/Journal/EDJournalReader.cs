/*
 * Copyright © 2016-2024 EDDiscovery development team
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
using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("{ID}: {FullName} P:{Pos} CQC:{cqc} Training:{training}")]
    public class EDJournalReader : TravelLogUnitLogReader
    {
        public EDJournalReader(string filename) : base(filename)
        {
        }

        public EDJournalReader(TravelLogUnit tlu) : base(tlu)
        {
        }

        // read journal lines from file, return if its read anything.
        // reporting if we have read anything is important.. it causes the TLU pos to be updated 
        // add to the lists all unfiltered journal events, all events passed by the filter, and uievents generated from journal entries due to the filtering
        // historyrefreshparsing = reading from DB, else reading dynamically during play
        // last continued contains the memory if the process of reading has given us a previous continue
        public bool ReadJournal(List<JournalEntry> jevents, List<UIEvent> uievents, bool historyrefreshparsing, ref JournalEvents.JournalContinued lastcontinued) 
        {
            bool readanything = false;

            while (true)
            {
                string line = ReadLine();           // read line from TLU.

                if (line == null)                   // null means finished, no more data
                    return readanything;

                //System.Diagnostics.Debug.WriteLine("Line read '" + line + "'");
                readanything = true;

                JournalEntry newentry = ProcessLine(line, historyrefreshparsing, ref lastcontinued);

                if (newentry != null)                           // if we got a record back, we may not because it may not be valid or be rejected..
                {
                    // if we don't have a commander yet, we need to queue it until we have one, since every entry needs a commander

                    if ((this.TravelLogUnit.CommanderId == null || this.TravelLogUnit.CommanderId < 0) && newentry.EventTypeID != JournalTypeEnum.LoadGame)
                    {
                        //System.Diagnostics.Debug.WriteLine("*** Delay " + newentry.JournalEntry.EventTypeStr);
                        StartEntries.Enqueue(newentry);         // queue..
                    }
                    else
                    {
                        while (StartEntries.Count != 0)     // we have a commander, anything queued up, play that in first.
                        {
                            var dentry = StartEntries.Dequeue();
                            dentry.SetCommander(TravelLogUnit.CommanderId.Value);
                            //System.Diagnostics.Debug.WriteLine("*** UnDelay " + dentry.JournalEntry.EventTypeStr);
                            JournalEventsManagement.FilterJournalEntriesToDBUI(dentry, jevents, uievents);
                        }

                        //System.Diagnostics.Debug.WriteLine("*** Send  " + newentry.JournalEntry.EventTypeStr);
                        JournalEventsManagement.FilterJournalEntriesToDBUI(newentry, jevents, uievents); // add newentry to jevents and/or uievents
                    }
                }
            }
        }

        // inhistoryrefreshparse = means reading history in batch mode
        // returns null if journal line is bad or its a repeat.. It does not throw
        private JournalEntry ProcessLine(string line, bool inhistoryrefreshparse, ref JournalEvents.JournalContinued lastcontinued)
        {
            //   System.Diagnostics.Debug.WriteLine("Line in '" + line + "'");
            int cmdrid = TravelLogUnit.CommanderId.HasValue ? TravelLogUnit.CommanderId.Value : -2; //-1 is hidden, -2 is never shown

            if (line.Length == 0)
                return null;

            JournalEntry je = null;

            try
            {           // use a try block in case anything in the creation goes tits up
                je = JournalEntry.CreateJournalEntry(line, true, true);       // save JSON, save json, don't return if bad
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"{TravelLogUnit.FullName} Exception Bad journal line: {line} {ex.Message} {ex.StackTrace}");
                return null;
            }

            if (je == null)
            {
                System.Diagnostics.Trace.WriteLine($"{TravelLogUnit.FullName} Bad journal line: {line}");
                return null;
            }

            if (je.EventTypeID == JournalTypeEnum.Fileheader)
            {
                JournalEvents.JournalFileheader header = (JournalEvents.JournalFileheader)je;

                if ((header.IsBeta && !EliteConfigInstance.InstanceOptions.DisableBetaCommanderCheck) || EliteConfigInstance.InstanceOptions.ForceBetaOnCommander) // if beta, and not disabled, or force beta
                {
                    TravelLogUnit.Type |= TravelLogUnit.BetaMarker;
                }

                TravelLogUnit.Build = header.Build ?? "";             // Probably never null, but we will protect
                TravelLogUnit.GameVersion = header.GameVersion ?? "";     // no trim

                if (header.Part > 1)
                {
                    // if we have a last continued, and its header parts match, and it has a commander, and its not too different in time..
                    if (lastcontinued != null && lastcontinued.Part == header.Part && lastcontinued.CommanderId >= 0 &&
                            Math.Abs(header.EventTimeUTC.Subtract(lastcontinued.EventTimeUTC).TotalSeconds) < 5)
                    {
                        cmdrid = lastcontinued.CommanderId;
                        TravelLogUnit.CommanderId = lastcontinued.CommanderId;      // copy commander across.
                    }
                    else
                    {           // this only works if you have a history... EDD does.
                        JournalEvents.JournalContinued contd = JournalEntry.GetLast<JournalEvents.JournalContinued>(je.EventTimeUTC.AddSeconds(1), e => e.Part == header.Part);

                        // Carry commander over from previous log if it ends with a Continued event.
                        if (contd != null && Math.Abs(header.EventTimeUTC.Subtract(contd.EventTimeUTC).TotalSeconds) < 5 && contd.CommanderId >= 0)
                        {
                            cmdrid = contd.CommanderId;
                            TravelLogUnit.CommanderId = contd.CommanderId;
                        }
                    }
                }

                lastcontinued = null; // used up, new file header, can't be continued now
            }
            else if (je.EventTypeID == JournalTypeEnum.Continued)
            {
                lastcontinued = je as JournalEvents.JournalContinued;       // save.. we are getting a new file soon
            }
            else if (je.EventTypeID == JournalTypeEnum.LoadGame)
            {
                var jlg = je as JournalEvents.JournalLoadGame;

                // for console logs, it will be [C] name
                // for journal logs, it will be frontier name

                string cmdrrootname = jlg.LoadGameCommander;

                if (TravelLogUnit.IsBetaFlag)
                {
                    cmdrrootname = EDCommander.AddBetaTagToName(cmdrrootname);
                }

                // a legacy loadgame, from 3.x or before, created after U14 release.
                // Gameversion appeared in Odyssey 2, and is not being sent by horizons clients
                // just in case they turn it on, 3.X is also a legacy version

                bool legacy = (jlg.GameVersion.IsEmpty() || jlg.GameVersion.Trim().StartsWith("3.")) && jlg.EventTimeUTC >= EliteReleaseDates.Odyssey14;

                // set TLU flags

                if (jlg.IsOdyssey == true)                                      // new! mark TLU with odyssey and horizons markers
                    TravelLogUnit.Type |= TravelLogUnit.OdysseyMarker;
                if (jlg.IsHorizons == true)
                    TravelLogUnit.Type |= TravelLogUnit.HorizonsMarker;

                if (jlg.GameVersion.HasChars())                                 // journals before odyssey did not have these, so strings will be empty
                    TravelLogUnit.GameVersion = jlg.GameVersion;                // no trim         
                if (jlg.Build.HasChars())
                    TravelLogUnit.Build = jlg.Build;

                // for made up entries without a TLU (EDSM downloads, EDD created ones) assign the default flag

                JournalEntry.DefaultHorizonsFlag = jlg.IsHorizons;
                JournalEntry.DefaultOdysseyFlag = jlg.IsOdyssey;
                JournalEntry.DefaultBetaFlag = jlg.IsBeta;

                // transform journal commander name  to db name
                // console commanders, with the [C] form placed in the downloader, are left alone, and (Legacy) is not added (even though legacy flag above will be true)
                // legacy commanders get (Legacy) added onto it
                // Live commanders are left alone

                string cmdrcreatedname = legacy && !EDCommander.NameIsConsoleCommander(cmdrrootname) ? EDCommander.AddLegacyTagToName(cmdrrootname) : cmdrrootname;

                EDCommander commander = EDCommander.GetCommander(cmdrcreatedname);

                if (commander == null)
                {
                    // in the default condition, we have a hidden commander, and first Cmdr. Jameson.
                    // get list of all active or deleted commanders - deleted cause we don't want to create one if purposely deleted

                    commander = EDCommander.GetCommander("Jameson (Default)");      // see if the place holder is there..

                    if (commander != null)      // if so, replace..
                    {
                        commander.Name = cmdrcreatedname;
                        commander.EdsmName = cmdrcreatedname;
                        commander.JournalDir = TravelLogUnit.Path;
                        commander.SyncToEddn = EliteConfigInstance.InstanceOptions.SetEDDNforNewCommanders;
                        EDCommander.Update(commander);
                    }
                    else
                    {
                        // make a new commander
                        // always add the path from now on
                        // note changing commander in EDCommander here does not work - it shows a mess of data - removed nov 22
                        commander = EDCommander.Add(name: cmdrcreatedname, journalpath: TravelLogUnit.Path, toeddn: EliteConfigInstance.InstanceOptions.SetEDDNforNewCommanders);
                    }

                    EDCommander.AddOrUpdateCount++;     // update the count, used to detect changes in commander by external systems

                    if (legacy)
                    {
                        commander.LegacyCommander = true;       // record legacy
                        System.Diagnostics.Trace.WriteLine($"Legacy commander {cmdrcreatedname} created");

                        var cmdr = EDCommander.GetCommander(cmdrrootname);     // if a live commander exist with this name

                        if (cmdr != null)
                        {
                            commander.SetLinkedCommander(cmdr.Id, EliteReleaseDates.Odyssey14);       // record linked
                            System.Diagnostics.Trace.WriteLine($"Found existing commander {cmdrrootname}, linked");
                        }

                        commander.Update();
                    }
                }

                commander.FID = jlg.FID;

                cmdrid = commander.Id;

                if (!TravelLogUnit.CommanderId.HasValue)        // we do not need to write to DB the TLU at this point, since we read something the upper layers will do that
                {
                    TravelLogUnit.CommanderId = cmdrid;
                    //System.Diagnostics.Trace.WriteLine(string.Format("TLU {0} updated with commander {1} at {2}", TravelLogUnit.Path, cmdrid, TravelLogUnit.Size));
                }

            }

            // if in dynamic read during play, and its an additional file JE, and we are NOT a console commander, see if we can pick up extra info

            if (!inhistoryrefreshparse && (je is IAdditionalFiles) && !(EDCommander.GetCommander(cmdrid)?.ConsoleCommander ?? false))
            {
                (je as IAdditionalFiles).ReadAdditionalFiles(TravelLogUnit.Path);       // try and read file dynamically written.
            }

            if (je.EventTypeID == JournalTypeEnum.FSDJump)
            {
                training |= (je as JournalEvents.JournalFSDJump).StarSystem == "Training";       // seen in logs..  we keep current training, but turn it on if star system is training
                                                                                                 // we need to keep current training as we may be jumping where we can't detect another training incident
            }
            else if (je.EventTypeID == JournalTypeEnum.Location)        // "event":"Location", "Docked":false, "StarSystem":"Training"
            {
                training |= (je as JournalEvents.JournalLocation).StarSystem == "Training"; // ditto above
            }
            else if (je.EventTypeID == JournalTypeEnum.Docked)          // "event":"Docked" "StationGovernment":"$government_None;", 
            {
                training = (je as JournalEvents.JournalDocked).Government == GovernmentDefinitions.Government.None; // all training has government none
            }
            // check some events for journal related events
            else if (je.EventTypeID == JournalTypeEnum.Undocked || je.EventTypeID == JournalTypeEnum.Died)
            {
                cqc = false;        // clear the cqc flag
            }
            else if (je.EventTypeID == JournalTypeEnum.LoadGame)
            {
                cqc = string.IsNullOrEmpty((je as JournalEvents.JournalLoadGame)?.GameMode);        // set cqc flag on if gamemode is present
                training = false;                                                                   // always clear training flag, never appears during it
            }
            else if (je.EventTypeID == JournalTypeEnum.Music)                                       // music is an indicator of CQC
            {
                var music = je as JournalEvents.JournalMusic;

                if (music?.MusicTrackID == JournalEvents.EDMusicTrackEnum.CQC || music?.MusicTrackID == JournalEvents.EDMusicTrackEnum.CQCMenu)
                {
                    cqc = true;
                }
            }

            if (cqc || training)  // Ignore events if in CQC or training
            {
                // System.Diagnostics.Debug.WriteLine($"Rejected journal {je.EventTypeStr} as in {(training ? "Training" : "CQC")}");
                return null;
            }

            // System.Diagnostics.Debug.WriteLine($"Accepted journal {je.EventTypeStr}");

            je.SetTLUCommander(TravelLogUnit.ID, cmdrid);       // update the JE with info from where it came from and the commander

            return je;
        }


        private bool cqc = false;
        private bool training = false;
        private Queue<JournalEntry> StartEntries = new Queue<JournalEntry>();

    }
}


