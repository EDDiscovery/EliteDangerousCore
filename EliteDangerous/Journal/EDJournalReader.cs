/*
 * Copyright © 2016-2020 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public class EDJournalReader : TravelLogUnitLogReader
    {
        bool cqc = false;

        static JournalEvents.JournalContinued lastcontinued = null;

        private Queue<JournalEntry> StartEntries = new Queue<JournalEntry>();

        public EDJournalReader(string filename) : base(filename)
        {
        }

        public EDJournalReader(TravelLogUnit tlu) : base(tlu)
        {
        }

        // inhistoryrefreshparse = means reading history in batch mode
        // returns null if journal line is bad or its a repeat.. It does not throw
        private JournalEntry ProcessLine(string line, bool inhistoryrefreshparse)
        {
         //   System.Diagnostics.Debug.WriteLine("Line in '" + line + "'");
            int cmdrid = TravelLogUnit.CommanderId.HasValue  ? TravelLogUnit.CommanderId.Value  : -2; //-1 is hidden, -2 is never shown

            if (line.Length == 0)
                return null;

            JournalEntry je = null;

            try
            {           // use a try block in case anything in the creation goes tits up
                je = JournalEntry.CreateJournalEntry(line, true, true);       // save JSON, save json, don't return if bad
            }
            catch ( Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"{TravelLogUnit.FullName} Exception Bad journal line: {line} {ex.Message} {ex.StackTrace}");
                return null;
            }

            if ( je == null )
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
                            cmdrid = lastcontinued.CommanderId;
                            TravelLogUnit.CommanderId = contd.CommanderId;
                        }
                    }
                }
            }
            else if (je.EventTypeID == JournalTypeEnum.Continued)
            {
                lastcontinued = je as JournalEvents.JournalContinued;       // save.. we are getting a new file soon
            }
            else if (je.EventTypeID == JournalTypeEnum.LoadGame)
            {
                var jlg = je as JournalEvents.JournalLoadGame;
                string cmdrrootname = jlg.LoadGameCommander;

                if (TravelLogUnit.IsBetaFlag)
                {
                    cmdrrootname = "[BETA] " + cmdrrootname;
                }

                // a legacy loadgame, from 3.x or before, created after U14 release. Gameversion appeared in Odyssey 2, and is not being sent by horizons clients

                DateTime EDOdyssey14UTC = new DateTime(2022, 11, 29, 12, 0, 0);
                bool legacy = jlg.GameVersion.IsEmpty() && jlg.EventTimeUTC >= EDOdyssey14UTC;

                string cmdrcreatedname = legacy ? cmdrrootname + " (Legacy)" : cmdrrootname;

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

                // find commander

                EDCommander commander = EDCommander.GetCommander(cmdrcreatedname);

                if (commander == null )
                {
                    // in the default condition, we have a hidden commander, and first Cmdr. Jameson.

                    commander = EDCommander.GetListCommanders().FirstOrDefault();
                    if (EDCommander.NumberOfCommanders == 2 && commander != null && commander.Name == "Jameson (Default)")
                    {
                        commander.Name = cmdrcreatedname;
                        commander.EdsmName = cmdrcreatedname;
                        commander.JournalDir = TravelLogUnit.Path;
                        EDCommander.Update(commander);
                    }
                    else
                    {
                        // make a new commander
                        // always add the path from now on
                        // note changing commander in EDCommander here does not work - it shows a mess of data - removed nov 22
                        commander = EDCommander.Add(name: cmdrcreatedname, journalpath: TravelLogUnit.Path);        
                    }

                    if ( legacy )
                    {
                        commander.LegacyCommander = true;       // record legacy
                        System.Diagnostics.Trace.WriteLine($"Legacy commander {cmdrcreatedname} created");

                        var cmdr = EDCommander.GetCommander(cmdrrootname);     // if a commander exist

                        if (cmdr!=null)
                        {
                            commander.SetLinkedCommander(cmdr.Id, EDOdyssey14UTC);       // record linked
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
            else if (je is ISystemStationEntry && ((ISystemStationEntry)je).IsTrainingEvent)
            {
                //System.Diagnostics.Trace.WriteLine($"{filename} Training detected:\n{line}");
                return null;
            }

            // if in dynamic read during play, and its an additional file JE, and we are NOT a console commander, see if we can pick up extra info

            if (!inhistoryrefreshparse && (je is IAdditionalFiles) && !(EDCommander.GetCommander(cmdrid)?.ConsoleCommander ?? false) )
            {
                (je as IAdditionalFiles).ReadAdditionalFiles(TravelLogUnit.Path);       // try and read file dynamically written.
            }
           
            if (je.EventTypeID == JournalTypeEnum.Undocked || je.EventTypeID == JournalTypeEnum.LoadGame || je.EventTypeID == JournalTypeEnum.Died)            
            {
                 cqc = (je.EventTypeID == JournalTypeEnum.LoadGame) && string.IsNullOrEmpty((je as JournalEvents.JournalLoadGame)?.GameMode);
            }
            else if (je.EventTypeID == JournalTypeEnum.Music)
            {
                var music = je as JournalEvents.JournalMusic;
                
                if (music?.MusicTrackID == JournalEvents.EDMusicTrackEnum.CQC || music?.MusicTrackID == JournalEvents.EDMusicTrackEnum.CQCMenu)
                {
                    cqc = true;
                }
            }

            if (cqc)  // Ignore events if in CQC
            {
                return null;
            }

            je.SetTLUCommander(TravelLogUnit.ID, cmdrid);

            return je;
        }

        // function needs to report two things, list of JREs (may be empty) and UIs, and if it read something, bool.. hence form changed
        // bool reporting we have performed any sort of action is important.. it causes the TLU pos to be updated above even if we have junked all the events or delayed them
        // function does not throw.
        // historyrefreshparsing = reading from DB, else reading dynamically during play
        // True if anything was processed, even if we rejected it

        public bool ReadJournal(List<JournalEntry> jent, List<UIEvent> uievents, bool historyrefreshparsing ) 
        {
            bool readanything = false;

            while (true)
            {
                string line = ReadLine();           // read line from TLU.

                if (line == null)                   // null means finished, no more data
                    return readanything;

                //System.Diagnostics.Debug.WriteLine("Line read '" + line + "'");
                readanything = true;

                JournalEntry newentry = ProcessLine(line, historyrefreshparsing);

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
                            AddEntry(dentry, jent, uievents);
                        }

                        //System.Diagnostics.Debug.WriteLine("*** Send  " + newentry.JournalEntry.EventTypeStr);
                        AddEntry(newentry, jent, uievents);
                    }
                }
            }
        }

        // this class looks at the JE and decides if its really a UI not a journal entry

        private void AddEntry( JournalEntry newentry, List<JournalEntry> jent, List<UIEvent> uievents )
        {
            if (newentry.EventTypeID == JournalTypeEnum.Music)     // MANUALLY sync this list with ActionEventList.cs::EventList function
            {
                var jm = newentry as JournalEvents.JournalMusic;
                uievents.Add(new UIEvents.UIMusic(jm.MusicTrack, jm.MusicTrackID, jm.EventTimeUTC, false));
                return;
            }
            else if (newentry.EventTypeID == JournalTypeEnum.UnderAttack)
            {
                var ja = newentry as JournalEvents.JournalUnderAttack;
                uievents.Add(new UIEvents.UIUnderAttack(ja.Target, ja.EventTimeUTC, false));
                return;
            }
            else if (newentry.EventTypeID == JournalTypeEnum.SendText)
            {
                var jt = newentry as JournalEvents.JournalSendText;
                if (jt.Command)
                {
                    uievents.Add(new UIEvents.UICommand(jt.Message, jt.To, jt.EventTimeUTC, false));
                    return;
                }
            }
            else if (newentry.EventTypeID == JournalTypeEnum.ShipTargeted)
            {
                var jst = newentry as JournalEvents.JournalShipTargeted;
                if (jst.TargetLocked == false)
                {
                    uievents.Add(new UIEvents.UIShipTargeted(jst, jst.EventTimeUTC, false));
                    return;
                }

            }
            else if (newentry.EventTypeID == JournalTypeEnum.ReceiveText)
            {
                var jt = newentry as JournalEvents.JournalReceiveText;
                if (jt.Channel == "Info")
                {
                    uievents.Add(new UIEvents.UIReceiveText(jt, jt.EventTimeUTC, false));
                    return;
                }
            }
            else if (newentry.EventTypeID == JournalTypeEnum.FSDTarget)
            {
                var jt = newentry as JournalEvents.JournalFSDTarget;
                uievents.Add(new UIEvents.UIFSDTarget(jt, jt.EventTimeUTC, false));
                return;
            }
            else if ( newentry.EventTypeID == JournalTypeEnum.NavRouteClear)
            {
                var jnc = newentry as JournalEvents.JournalNavRouteClear;
                uievents.Add(new UIEvents.UINavRouteClear(jnc, jnc.EventTimeUTC, false));
                return;
            }

            jent.Add(newentry);
        }
    }
}


